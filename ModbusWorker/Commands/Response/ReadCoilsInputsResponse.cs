using DotNetty.Buffers;
using System.Collections;

namespace ModbusWorker.Commands.Response
{
    public class ReadCoilsInputsResponse : ModbusMessage
    {
        protected ushort ByteCount { get; private set; } = default!;

        protected BitArray CoilsOrInputs { get; private set; } = default!;

        public ReadCoilsInputsResponse(byte unitId, ModbusFunctionCode functionCode) : base(unitId, functionCode)
        {
        }

        public ReadCoilsInputsResponse(byte unitId, ModbusFunctionCode functionCode, BitArray coilsOrInputs) : base(unitId, functionCode)
        {
            CoilsOrInputs = coilsOrInputs;
            ByteCount = (ushort)(CoilsOrInputs.Length / 8);
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            ByteCount = buffer.ReadByte();
            var coilsOrInputs = new byte[ByteCount];
            buffer.ReadBytes(coilsOrInputs);

            CoilsOrInputs = new BitArray(coilsOrInputs);
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            var coilsOrInputs = new byte[ByteCount];
            CoilsOrInputs.CopyTo(coilsOrInputs, 0);

            buffer.WriteByte(ByteCount);
            buffer.WriteBytes(coilsOrInputs);

            return buffer;
        }
    }
}
