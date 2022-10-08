namespace ModbusWorker.Commands.Response
{
    public class WriteSingleCoilResponse : WriteSingleMessage
    {
        public bool State => Value == 0xFF00;

        public WriteSingleCoilResponse(byte unitId) : base(unitId, ModbusFunctionCode.WriteSingleCoil)
        {
        }

        public WriteSingleCoilResponse(byte unitId, ushort startingAddress, bool state) : base(unitId, ModbusFunctionCode.WriteSingleCoil, startingAddress, (ushort)(state ? 0xFF00 : 0x0000))
        {
        }
    }
}