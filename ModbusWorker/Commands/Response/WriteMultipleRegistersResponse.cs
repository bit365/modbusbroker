namespace ModbusWorker.Commands.Response
{
    public class WriteMultipleRegistersResponse : ReadWriteMultipleMessage
    {
        public WriteMultipleRegistersResponse(byte unitId) : base(unitId, ModbusFunctionCode.WriteMultipleRegisters)
        {
        }

        public WriteMultipleRegistersResponse(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.WriteMultipleRegisters, startingAddress, quantity)
        {

        }
    }
}