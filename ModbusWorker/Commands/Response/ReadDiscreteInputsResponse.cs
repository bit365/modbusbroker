using System.Collections;

namespace ModbusWorker.Commands.Response
{
    public abstract class ReadDiscreteInputsResponse : ReadCoilsInputsResponse
    {
        public BitArray Inputs => CoilsOrInputs;

        public ReadDiscreteInputsResponse(byte unitId) : base(unitId, ModbusFunctionCode.ReadDiscreteInputs)
        {
        }

        public ReadDiscreteInputsResponse(byte unitId, BitArray inputs) : base(unitId, ModbusFunctionCode.ReadDiscreteInputs, inputs)
        {
        }
    }
}
