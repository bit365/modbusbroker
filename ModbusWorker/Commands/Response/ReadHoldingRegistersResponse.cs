namespace ModbusWorker.Commands.Response
{
    public class ReadHoldingRegistersResponse : ReadRegistersResponse
    {
        public ReadHoldingRegistersResponse(byte unitId) : base(unitId, ModbusFunctionCode.ReadHoldingRegisters)
        {
        }

        public ReadHoldingRegistersResponse(byte unitId, ushort[] registers) : base(unitId, ModbusFunctionCode.ReadHoldingRegisters, registers)
        {
        }
    }
}