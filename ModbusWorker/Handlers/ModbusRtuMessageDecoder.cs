using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ModbusWorker.Commands;

namespace ModbusWorker.Handlers
{
    public class ModbusRtuMessageDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            //Unit Identifier + Function Code + Crc Value

            if (input.ReadableBytes < 1 + 1 + 2)
            {
                return;
            }

            byte unitId = input.GetByte(input.ReaderIndex);

            byte functionCodeValue = input.GetByte(input.ReaderIndex + 1);

            if (!Enum.IsDefined(typeof(ModbusFunctionCode), functionCodeValue))
            {
                return;
            }

            var functionCode = (ModbusFunctionCode)functionCodeValue;

            string modbusMessageTypeName = $"{functionCode}Response";

            ModbusMessage modbusMessage = ModbusMessageFactory.CreateModbusMessage(modbusMessageTypeName, unitId);

            if (functionCodeValue >= 0x80)
            {
                modbusMessage = new ModbusErrorMessage(unitId, functionCode);
            }
            else modbusMessage ??= new ModbusErrorMessage(unitId, functionCode, 0x01);

            input.MarkReaderIndex();

            try
            {
                modbusMessage.Decode(input);

                ModbusRtuFrame modbusRtuFrame = new(modbusMessage);

                ushort crcValue = input.ReadUnsignedShortLE();

                IByteBuffer messageBuffer = modbusRtuFrame.Encode();

                if (messageBuffer.HasArray)
                {
                    int length = messageBuffer.ReadableBytes - 2;

                    byte[] bytes = new byte[length];

                    messageBuffer.ReadBytes(bytes);

                    var crcCalculated = ModbusCrcCalculator.GetModbusCrc(bytes, length);

                    if (crcCalculated == crcValue)
                    {
                        output.Add(modbusRtuFrame.Message);
                    }
                }
            }
            catch
            {
                input.ResetReaderIndex();
            }
        }
    }
}
