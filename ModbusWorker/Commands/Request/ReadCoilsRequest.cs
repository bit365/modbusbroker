namespace ModbusWorker.Commands.Request
{
    public class ReadCoilsRequest : ReadWriteMultipleMessage
    {
        public ReadCoilsRequest(byte unitId) : base(unitId, ModbusFunctionCode.ReadCoils)
        {
        }

        public ReadCoilsRequest(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.ReadCoils, startingAddress, quantity)
        {
        }
    }
}
