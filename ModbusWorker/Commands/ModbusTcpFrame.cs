using DotNetty.Buffers;

namespace ModbusWorker.Commands
{
    public class ModbusTcpFrame
    {
        public ModbusHeader Header { get; set; } = default!;

        public ModbusMessage Message { get; set; } = default!;

        public ModbusTcpFrame(ModbusHeader header, ModbusMessage message)
        {
            Header = header;
            Message = message;
        }

        public IByteBuffer Encode()
        {
            IByteBuffer messageBuffer = Message.Encode();

            Header.Length = (ushort)messageBuffer.ReadableBytes;

            IByteBuffer buffer = Unpooled.Buffer();

            buffer.WriteBytes(Header.Encode());
            buffer.WriteBytes(Message.Encode());

            return buffer;
        }

        public class ModbusHeader
        {
            public ushort TransactionId { get; set; }

            public ushort ProtocolId { get; set; }

            public ushort Length { get; set; }

            public ModbusHeader(IByteBuffer buffer)
            {
                TransactionId = buffer.ReadUnsignedShort();
                ProtocolId = buffer.ReadUnsignedShort();
                Length = buffer.ReadUnsignedShort();
            }

            public ModbusHeader(ushort transactionId, ushort protocolId = 0x0000)
            {
                TransactionId = transactionId;
                ProtocolId = protocolId;
            }

            public IByteBuffer Encode()
            {
                IByteBuffer buffer = Unpooled.Buffer();

                buffer.WriteUnsignedShort(TransactionId);
                buffer.WriteUnsignedShort(ProtocolId);
                buffer.WriteUnsignedShort(Length);

                return buffer;
            }
        }
    }
}