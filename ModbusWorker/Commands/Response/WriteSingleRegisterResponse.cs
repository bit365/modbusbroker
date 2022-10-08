namespace ModbusWorker.Commands.Response
{
    public class WriteSingleRegisterResponse : WriteSingleMessage
    {
        public WriteSingleRegisterResponse(byte unitId) : base(unitId, ModbusFunctionCode.WriteSingleRegister)
        {
        }

        public WriteSingleRegisterResponse(byte unitId, ushort startingAddress, ushort value) : base(unitId, ModbusFunctionCode.WriteSingleRegister, startingAddress, value)
        {
        }
    }
}
