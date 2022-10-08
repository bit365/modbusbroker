namespace ModbusWorker.Commands.Response
{
    public class ReadInputRegistersResponse : ReadRegistersResponse
    {
        public ReadInputRegistersResponse(byte unitId) : base(unitId, ModbusFunctionCode.ReadInputRegisters)
        {
        }

        public ReadInputRegistersResponse(byte unitId, ushort[] registers) : base(unitId, ModbusFunctionCode.ReadInputRegisters, registers)
        {
        }
    }
}
