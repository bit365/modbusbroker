using DotNetty.Buffers;
using ModbusWorker.Handlers;

namespace ModbusWorker.Commands
{
    public class ModbusRtuFrame
    {
        public ModbusMessage Message { get; set; } = default!;

        public ushort CrcValue { get; } = default!;

        public ModbusRtuFrame(ModbusMessage message)
        {
            Message = message;

            IByteBuffer messageBuffer = message.Encode();

            if (messageBuffer.HasArray)
            {
                int length = messageBuffer.ReadableBytes;

                byte[] bytes = new byte[length];

                messageBuffer.ReadBytes(bytes);

                CrcValue = ModbusCrcCalculator.GetModbusCrc(bytes, length);
            }
        }

        public IByteBuffer Encode()
        {
            IByteBuffer buffer = Unpooled.Buffer();

            buffer.WriteBytes(Message.Encode());
            buffer.WriteUnsignedShortLE(CrcValue);

            return buffer;
        }
    }
}