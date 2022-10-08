namespace ModbusWorker.ModbusModels
{
    public class ModbusOriginalDataType
    {
        public ModbusDataType Type { get; set; }

        public OriginalDataTypeSpecs Specs { get; set; } = default!;
    }

    public class OriginalDataTypeSpecs
    {
        public int? RegisterCount { get; set; }

        public bool? SwapByte { get; set; }

        public bool? ReverseRegister { get; set; }
    }
}
