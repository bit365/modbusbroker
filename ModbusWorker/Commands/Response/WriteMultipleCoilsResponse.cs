namespace ModbusWorker.Commands.Response
{
    public class WriteMultipleCoilsResponse : ReadWriteMultipleMessage
    {
        public WriteMultipleCoilsResponse(byte unitId) : base(unitId, ModbusFunctionCode.WriteMultipleCoils)
        {
        }

        public WriteMultipleCoilsResponse(byte unitId, ushort startingAddress, ushort quantity) : base(unitId, ModbusFunctionCode.WriteMultipleCoils, startingAddress, quantity)
        {

        }
    }
}
