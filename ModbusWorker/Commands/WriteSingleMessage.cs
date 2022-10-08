using DotNetty.Buffers;

namespace ModbusWorker.Commands
{
    public class WriteSingleMessage : ModbusMessage
    {
        public ushort StartingAddress { get; private set; }

        public ushort Value { get; private set; }

        public WriteSingleMessage(byte unitId, ModbusFunctionCode functionCode) : base(unitId, functionCode) { }

        public WriteSingleMessage(byte unitId, ModbusFunctionCode functionCode, ushort startingAddress, ushort value) : base(unitId, functionCode)
        {
            StartingAddress = startingAddress;
            Value = value;
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            StartingAddress = buffer.ReadUnsignedShort();
            Value = buffer.ReadUnsignedShort();
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            buffer.WriteUnsignedShort(StartingAddress);
            buffer.WriteShort(Value);

            return buffer;
        }
    }
}