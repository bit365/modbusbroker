using DotNetty.Buffers;

namespace ModbusWorker.Commands.Response
{
    public abstract class ReadRegistersResponse : ModbusMessage
    {
        protected ushort ByteCount { get; set; } = default!;

        public ushort[] Registers { get; private set; } = default!;

        public ReadRegistersResponse(byte unitId, ModbusFunctionCode functionCode) : base(unitId, functionCode)
        {
        }

        public ReadRegistersResponse(byte unitId, ModbusFunctionCode functionCode, ushort[] registers) : base(unitId, functionCode)
        {
            Registers = registers;
            ByteCount = (ushort)(registers.Length * 2);
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            ByteCount = buffer.ReadByte();
            Registers = new ushort[ByteCount / 2];

            for (int i = 0; i < Registers.Length; i++)
            {
                Registers[i] = buffer.ReadUnsignedShort();
            }
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            buffer.WriteByte(ByteCount);

            foreach (var register in Registers)
            {
                buffer.WriteUnsignedShort(register);
            }

            return buffer;
        }
    }
}
