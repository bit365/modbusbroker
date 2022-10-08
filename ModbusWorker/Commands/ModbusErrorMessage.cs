using DotNetty.Buffers;

namespace ModbusWorker.Commands
{
    public class ModbusErrorMessage : ModbusMessage
    {
        public ushort ErrorCode { get; private set; }

        public ModbusErrorMessage(byte unitId, ModbusFunctionCode functionCode) : base(unitId, functionCode)
        {
        }

        public ModbusErrorMessage(byte unitId, ModbusFunctionCode functionCode, ushort errorCode) : base(unitId, functionCode)
        {
            ErrorCode = errorCode;
        }

        private static readonly Dictionary<ushort, string> errors = new()
        {
            {0x01, "Illegal Function"},
            {0x02, "Illegal Data Address"},
            {0x03, "Illegal Data Value"},
            {0x04, "Slave Device Failure"},
            {0x05, "Acknowledge"},
            {0x06, "Slave Device Busy"},
            {0x08, "Memory Parity Error"},
            {0x0A, "Gateway Path Unavailable"},
            {0x0B, "Gateway Target Device Failed To Respond"},
        };

        public string ErrorMessage => errors[ErrorCode];

        public override void Decode(IByteBuffer buffer)
        {
            base.Decode(buffer);
            ErrorCode = buffer.ReadByte();
        }

        public override IByteBuffer Encode()
        {
            IByteBuffer buffer = base.Encode();

            buffer.WriteByte(ErrorCode);

            return buffer;
        }

        public override string ToString() => ErrorMessage;
    }
}
