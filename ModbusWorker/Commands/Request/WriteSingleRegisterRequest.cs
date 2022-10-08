namespace ModbusWorker.Commands.Request
{
    public class WriteSingleRegisterRequest : WriteSingleMessage
    {
        public WriteSingleRegisterRequest(byte unitId) : base(unitId, ModbusFunctionCode.WriteSingleRegister)
        {
        }

        public WriteSingleRegisterRequest(byte unitId, ushort startingAddress, ushort value) : base(unitId, ModbusFunctionCode.WriteSingleRegister, startingAddress, value)
        {
        }
    }
}
