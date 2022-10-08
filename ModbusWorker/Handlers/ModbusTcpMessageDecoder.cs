using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ModbusWorker.Commands;
using static ModbusWorker.Commands.ModbusTcpFrame;

namespace ModbusWorker.Handlers
{
    public class ModbusTcpMessageDecoder : LengthFieldBasedFrameDecoder
    {
        public ModbusTcpMessageDecoder() : base(byte.MaxValue, 4, 2) { }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            //Transaction Identifier + Protocol Identifier + Length + Unit Identifier + Function Code

            object obj = Decode(context, input);

            if (obj is IByteBuffer buffer)
            {
                ModbusHeader modbusHeader = new(buffer);

                byte unitId = buffer.GetByte(buffer.ReaderIndex);

                byte functionCodeValue = buffer.GetByte(buffer.ReaderIndex + 1);

                if (Enum.IsDefined(typeof(ModbusFunctionCode), functionCodeValue))
                {
                    var functionCode = (ModbusFunctionCode)functionCodeValue;

                    string modbusMessageTypeName = $"{functionCode}Response";

                    ModbusMessage modbusMessage = ModbusMessageFactory.CreateModbusMessage(modbusMessageTypeName, unitId);

                    if (functionCodeValue >= 0x80)
                    {
                        modbusMessage = new ModbusErrorMessage(unitId, functionCode);
                    }
                    else
                    {
                        modbusMessage ??= new ModbusErrorMessage(unitId, functionCode, 0x01);
                    }

                    modbusMessage.Decode(buffer);

                    ModbusTcpFrame modbusTcpFrame = new(modbusHeader, modbusMessage);

                    output.Add(modbusTcpFrame.Message);
                }
            }
        }
    }
}