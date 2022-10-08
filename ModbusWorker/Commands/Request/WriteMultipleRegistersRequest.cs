using DotNetty.Buffers;

namespace ModbusWorker.Commands.Request
{
    public class WriteMultipleRegistersRequest : ReadWriteMultipleMessage
    {
        public ushort[] Registers { get; private set; } = default!;

        private ushort byteCount = default!;

        public WriteMultipleRegistersRequest(byte unitId) : base(unitId, ModbusFunctionCode.WriteMultipleRegisters)
        {
        }

        public WriteMultipleRegistersRequest(byte unitId, ushort startingAddress, params ushort[] registers) : base(unitId, ModbusFunctionCode.WriteMultipleRegisters, startingAddress, (ushort)registers.Length)
        {
            byteCount = (ushort)(Quantity * 2);
            Registers = registers;
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            byteCount = buffer.ReadByte();

            Registers = new ushort[byteCount / 2];

            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = buffer.ReadUnsignedShort();
            }
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            buffer.WriteByte(byteCount);

            foreach (var register in Registers)
            {
                buffer.WriteShort(register);
            }

            return buffer;
        }
    }
}
