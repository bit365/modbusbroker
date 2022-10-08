using DotNetty.Buffers;

namespace ModbusWorker.Commands
{
    public abstract class ReadWriteMultipleMessage : ModbusMessage
    {
        public ushort StartingAddress { get; private set; }

        public ushort Quantity { get; private set; }

        public ReadWriteMultipleMessage(byte unitId, ModbusFunctionCode functionCode) : base(unitId, functionCode) { }

        public ReadWriteMultipleMessage(byte unitId, ModbusFunctionCode functionCode, ushort startingAddress, ushort quantity) : base(unitId, functionCode)
        {
            StartingAddress = startingAddress;
            Quantity = quantity;
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            StartingAddress = buffer.ReadUnsignedShort();
            Quantity = buffer.ReadUnsignedShort();
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            buffer.WriteUnsignedShort(StartingAddress);
            buffer.WriteUnsignedShort(Quantity);

            return buffer;
        }
    }
}
