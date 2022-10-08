namespace ModbusWorker.Commands.Request
{
    public class ReadHoldingRegistersRequest : ReadWriteMultipleMessage
    {
        public ReadHoldingRegistersRequest(byte unitId) : base(unitId, ModbusFunctionCode.ReadHoldingRegisters)
        {
        }

        public ReadHoldingRegistersRequest(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.ReadHoldingRegisters, startingAddress, quantity)
        {
        }
    }
}