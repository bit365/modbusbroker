namespace ModbusWorker.Commands.Request
{
    public class ReadInputRegistersRequest : ReadWriteMultipleMessage
    {
        public ReadInputRegistersRequest(byte unitId) : base(unitId, ModbusFunctionCode.ReadInputRegisters)
        {
        }

        public ReadInputRegistersRequest(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.ReadInputRegisters, startingAddress, quantity)
        {
        }
    }
}
