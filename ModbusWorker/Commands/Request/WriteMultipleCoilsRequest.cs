using DotNetty.Buffers;
using System.Collections;

namespace ModbusWorker.Commands.Request
{
    public class WriteMultipleCoilsRequest : ReadWriteMultipleMessage
    {
        protected ushort ByteCount { get; set; } = default!;

        public BitArray Coils { get; private set; } = default!;

        public WriteMultipleCoilsRequest(byte unitId) : base(unitId, ModbusFunctionCode.WriteMultipleCoils)
        {
        }

        public WriteMultipleCoilsRequest(byte unitId, ushort startingAddress, params bool[] states) : base(unitId, ModbusFunctionCode.WriteMultipleCoils, startingAddress, (ushort)states.Length)
        {
            var length = Quantity + (8 - Quantity % 8) % 8;

            if (length > Quantity)
            {
                var finalCoils = new List<bool>();
                finalCoils.AddRange(states);
                for (var i = Quantity; i < length; i++)
                {
                    finalCoils.Add(false);
                }

                Coils = new BitArray(finalCoils.ToArray());
            }
            else
            {
                Coils = new BitArray(states);
            }

            ByteCount = (ushort)(Coils.Length / 8);
        }

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);

            ByteCount = buffer.ReadByte();
            var coils = new byte[ByteCount];
            buffer.ReadBytes(coils);

            Coils = new BitArray(coils);
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            var coils = new byte[ByteCount];
            Coils.CopyTo(coils, 0);

            buffer.WriteByte(ByteCount);

            buffer.WriteBytes(coils);

            return buffer;
        }
    }
}
