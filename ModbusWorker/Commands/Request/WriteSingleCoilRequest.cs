namespace ModbusWorker.Commands.Request
{
    public class WriteSingleCoilRequest : WriteSingleMessage
    {
        public bool State => Value == 0xFF00;

        public WriteSingleCoilRequest(byte unitId) : base(unitId, ModbusFunctionCode.WriteSingleCoil)
        {
        }

        public WriteSingleCoilRequest(byte unitId, ushort startingAddress, bool state) : base(unitId, ModbusFunctionCode.WriteSingleCoil, startingAddress, (ushort)(state ? 0xFF00 : 0x0000))
        {
        }
    }
}
