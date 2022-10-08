using DotNetty.Transport.Channels;
using ModbusWorker.Commands;
using ModbusWorker.Commands.Request;
using ModbusWorker.Commands.Response;
using ModbusWorker.ModbusModels;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace ModbusWorker
{
    public class ModbusBridge
    {
        private static readonly ConcurrentDictionary<string, IChannel> deviceChannels = new();

        internal static async Task DeviceLoginAsync(IChannel channel, SessionInfo session)
        {
            if (deviceChannels.TryRemove(session.DeviceId, out IChannel? originalChannel))
            {
                await originalChannel.DisconnectAsync().ConfigureAwait(false);
            }

            deviceChannels[session.DeviceId] = channel;
        }

        internal static void DeviceLogout(string deviceId) => deviceChannels.TryRemove(deviceId, out _);

        public static async Task<ModbusMessage> SendingAsync(string deviceId, ModbusMessage request, int timeoutMilliseconds = 10000, CancellationToken cancellationToken = default)
        {
            if (!deviceChannels.ContainsKey(deviceId))
            {
                throw new ApplicationException("The device id is unregistered");
            }

            var deviceChannel = deviceChannels[deviceId];

            var sessionInfo = deviceChannel.GetAttribute(SessionInfo.AttributeKey).Get();

            await sessionInfo.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds));

            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken.Token, cancellationToken);

            try
            {
                sessionInfo.Awaitable = new TaskCompletionSource<ModbusMessage>();

                await deviceChannel.WriteAndFlushAsync(request).ConfigureAwait(false);

                using (tokenSource.Token.Register(() => sessionInfo.Awaitable.TrySetCanceled()))
                {
                    return await sessionInfo.Awaitable.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                sessionInfo.Semaphore.Release();
            }
        }

        public static async Task<object?> GetPropertyValueAsync(string deviceId, ModbusPropertyInfo propertyInfo)
        {
            object? result = null;

            var startAddress = Convert.ToUInt16(propertyInfo.StartAddress, 16);

            static ushort GetDataTypeRegisterQuantity(ModbusPropertyInfo propertyInfo)
            {
                var strCount = propertyInfo.DataType.Specs.RegisterCount;

                ushort quantity = propertyInfo.DataType.Type switch
                {
                    ModbusDataType.Int16 or ModbusDataType.UInt16 or ModbusDataType.Bool or ModbusDataType.Bits => 1,
                    ModbusDataType.Int32 or ModbusDataType.UInt32 or ModbusDataType.Float => 2,
                    ModbusDataType.Double or ModbusDataType.Int64 or ModbusDataType.UInt64 => 4,
                    ModbusDataType.String => strCount.HasValue ? (ushort)strCount.Value : ushort.MinValue,
                    _ => throw new NotImplementedException()
                };

                return quantity;
            }

            static object? GetPropertyValue(ModbusPropertyInfo propertyInfo, IEnumerable<ushort> registerValues)
            {
                byte[] bytes = registerValues.SelectMany(e => BitConverter.GetBytes(e)).ToArray();

                bool reverseRegister = propertyInfo.DataType.Specs.ReverseRegister.HasValue && propertyInfo.DataType.Specs.ReverseRegister.Value;

                bytes = reverseRegister && bytes.Length % 4 == 0 ? ByteSwapper.ByteSwap(bytes, 4) : bytes;

                bool swapByte = propertyInfo.DataType.Specs.SwapByte.HasValue && propertyInfo.DataType.Specs.SwapByte.Value;

                bytes = swapByte ? ByteSwapper.ByteSwap(bytes, 2) : bytes;

                object? result = propertyInfo.DataType.Type switch
                {
                    ModbusDataType.Int16 => BitConverter.ToInt16(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.UInt16 => BitConverter.ToUInt16(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.Int32 => BitConverter.ToInt32(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.UInt32 => BitConverter.ToUInt32(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.Int64 => BitConverter.ToInt64(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.UInt64 => BitConverter.ToUInt64(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.Float => BitConverter.ToSingle(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.Double => BitConverter.ToDouble(bytes) * propertyInfo?.Scaling,
                    ModbusDataType.Bool => BitConverter.ToBoolean(bytes),
                    ModbusDataType.String => BitConverter.ToString(bytes),
                    ModbusDataType.Bits => new BitArray(bytes).Get(propertyInfo.BitMask),
                    _ => null,
                };

                if (propertyInfo?.Expression is not null)
                {
                    string expression = propertyInfo.Expression.Replace("X", result?.ToString(), StringComparison.OrdinalIgnoreCase);
                    result = new System.Data.DataTable().Compute(expression, null);
                }

                return result;
            }

            switch (propertyInfo.OperateType)
            {
                case ModbusOperateType.CoilStatus:
                    {
                        var request = new ReadCoilsRequest(propertyInfo.UnitId, startAddress, 1);
                        var response = (ReadCoilsResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                        result = response.Coils.Get(0);
                        break;
                    }

                case ModbusOperateType.DiscreteInput:
                    {
                        var request = new ReadDiscreteInputsRequest(propertyInfo.UnitId, startAddress, 1);
                        var response = (ReadDiscreteInputsResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                        result = response.Inputs.Get(0);
                        break;
                    }

                case ModbusOperateType.HoldingRegister:
                    {
                        var quantity = GetDataTypeRegisterQuantity(propertyInfo);
                        var request = new ReadHoldingRegistersRequest(propertyInfo.UnitId, startAddress, quantity);
                        var response = (ReadHoldingRegistersResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);

                        result = GetPropertyValue(propertyInfo, response.Registers);

                        break;
                    }
                case ModbusOperateType.InputRegister:
                    {
                        var quantity = GetDataTypeRegisterQuantity(propertyInfo);
                        var request = new ReadInputRegistersRequest(propertyInfo.UnitId, startAddress, quantity);
                        var response = (ReadInputRegistersResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);

                        result = GetPropertyValue(propertyInfo, response.Registers);

                        break;
                    }
            }

            return result;
        }

        public static async Task<object?> GetPropertyValueAsync(string deviceId, string propertyName)
        {
            var propertyInfo = DeviceService.GetExtensionProperties(deviceId)?.SingleOrDefault(e => e.CorrelationId == propertyName);

            propertyInfo = propertyInfo ?? throw new ArgumentException(null, nameof(propertyName));

            return await GetPropertyValueAsync(deviceId, propertyInfo).ConfigureAwait(false);
        }

        public static async Task<bool> SetPropertyValueAsync(string deviceId, ModbusPropertyInfo propertyInfo, object value)
        {
            if (!propertyInfo.WriteFunctionCode.HasValue)
            {
                throw new InvalidOperationException("Read only property cannot be changed");
            }

            var startAddress = Convert.ToUInt16(propertyInfo.StartAddress, 16);

            static ushort[] GetRegistersValues(ModbusPropertyInfo propertyInfo, object value)
            {
                byte[] bytes = Array.Empty<byte>();

                switch (propertyInfo.DataType.Type)
                {
                    case ModbusDataType.Int16:
                        {
                            short propertyValue = Convert.ToInt16(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.UInt16:
                        {
                            ushort propertyValue = Convert.ToUInt16(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.Int32:
                        {
                            int propertyValue = Convert.ToInt32(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.UInt32:
                        {
                            uint propertyValue = Convert.ToUInt32(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.Int64:
                        {
                            long propertyValue = Convert.ToInt64(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.UInt64:
                        {
                            ulong propertyValue = Convert.ToUInt64(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.Float:
                        {
                            float propertyValue = Convert.ToSingle(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.Double:
                        {
                            double propertyValue = Convert.ToDouble(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.String:
                        {
                            string propertyValue = Convert.ToString(value) ?? string.Empty;
                            bytes = Encoding.ASCII.GetBytes(propertyValue);
                            break;
                        }

                    case ModbusDataType.Bool:
                        {
                            bool propertyValue = Convert.ToBoolean(value);
                            bytes = BitConverter.GetBytes(propertyValue);
                            break;
                        }
                }

                bool reverseRegister = propertyInfo.DataType.Specs.ReverseRegister.HasValue && propertyInfo.DataType.Specs.ReverseRegister.Value;

                bytes = reverseRegister && bytes.Length % 4 == 0 ? ByteSwapper.ByteSwap(bytes, 4) : bytes;

                bool swapByte = propertyInfo.DataType.Specs.SwapByte.HasValue && propertyInfo.DataType.Specs.SwapByte.Value;

                bytes = swapByte ? ByteSwapper.ByteSwap(bytes, 2) : bytes;

                List<ushort> ushorts = new();

                for (int i = 0; i < bytes.Length; i += 2)
                {
                    ushorts.Add(BitConverter.ToUInt16(bytes, i));
                }

                return ushorts.ToArray();
            }

            ModbusMessage responseMessage = default!;

            switch (propertyInfo.OperateType)
            {
                case ModbusOperateType.CoilStatus:
                    {
                        bool propertyValue = Convert.ToBoolean(value);

                        switch ((ModbusFunctionCode)propertyInfo.WriteFunctionCode.Value)
                        {
                            case ModbusFunctionCode.WriteSingleCoil:
                                {
                                    var request = new WriteSingleCoilRequest(propertyInfo.UnitId, startAddress, propertyValue);
                                    responseMessage = (ReadCoilsResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                                    break;
                                }

                            case ModbusFunctionCode.WriteMultipleCoils:
                                {
                                    var request = new WriteMultipleCoilsRequest(propertyInfo.UnitId, startAddress, propertyValue);
                                    responseMessage = (WriteMultipleCoilsResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                                    break;
                                }
                        }
                        break;
                    }

                case ModbusOperateType.HoldingRegister:
                    {
                        var registersValues = GetRegistersValues(propertyInfo, value);

                        switch ((ModbusFunctionCode)propertyInfo.WriteFunctionCode.Value)
                        {
                            case ModbusFunctionCode.WriteSingleRegister:
                                {
                                    var request = new WriteSingleRegisterRequest(propertyInfo.UnitId, startAddress, registersValues.First());
                                    responseMessage = (WriteSingleRegisterResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                                    break;
                                }

                            case ModbusFunctionCode.WriteMultipleRegisters:
                                {
                                    var request = new WriteMultipleRegistersRequest(propertyInfo.UnitId, startAddress, registersValues);
                                    responseMessage = (WriteMultipleRegistersResponse)await SendingAsync(deviceId, request).ConfigureAwait(false);
                                    break;
                                }
                        }
                        break;
                    }
            }

            return responseMessage is not ModbusErrorMessage;
        }

        public static async Task<bool> SetPropertyValueAsync(string deviceId, string propertyName, object value)
        {
            var propertyInfo = DeviceService.GetExtensionProperties(deviceId)?.SingleOrDefault(e => e.CorrelationId == propertyName);

            propertyInfo = propertyInfo ?? throw new ArgumentException(null, nameof(propertyName));

            return await SetPropertyValueAsync(deviceId, propertyInfo, value).ConfigureAwait(false);
        }
    }
}