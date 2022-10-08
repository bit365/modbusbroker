using System.Collections;

namespace ModbusWorker.Commands.Response
{
    public class ReadCoilsResponse : ReadCoilsInputsResponse
    {
        public BitArray Coils => CoilsOrInputs;

        public ReadCoilsResponse(byte unitId) : base(unitId, ModbusFunctionCode.ReadCoils)
        {
        }

        public ReadCoilsResponse(byte unitId, BitArray coils) : base(unitId, ModbusFunctionCode.ReadCoils, coils)
        {
        }
    }
}
