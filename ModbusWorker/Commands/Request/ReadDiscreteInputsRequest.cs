namespace ModbusWorker.Commands.Request
{
    public class ReadDiscreteInputsRequest : ReadWriteMultipleMessage
    {
        public ReadDiscreteInputsRequest(byte unitId) : base(unitId, ModbusFunctionCode.ReadDiscreteInputs)
        {
        }

        public ReadDiscreteInputsRequest(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.ReadDiscreteInputs, startingAddress, quantity)
        {
        }
    }
}
