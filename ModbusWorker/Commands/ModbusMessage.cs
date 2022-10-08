using DotNetty.Buffers;

namespace ModbusWorker.Commands
{
    public abstract class ModbusMessage
    {
        public byte UnitId { get; set; }

        public ModbusFunctionCode FunctionCode { get; set; }

        protected ModbusMessage(byte unitId, ModbusFunctionCode functionCode)
        {
            UnitId = unitId;
            FunctionCode = functionCode;
        }

        public virtual void Decode(IByteBuffer buffer)
        {
            UnitId = buffer.ReadByte();
            FunctionCode = (ModbusFunctionCode)buffer.ReadByte();
        }

        public virtual IByteBuffer Encode()
        {
            IByteBuffer buffer = Unpooled.Buffer();

            buffer.WriteByte(UnitId);
            buffer.WriteByte((byte)FunctionCode);

            return buffer;
        }

        public override string ToString() => ByteBufferUtil.HexDump(Encode());
    }
}