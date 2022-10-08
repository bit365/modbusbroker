namespace ModbusWorker.ModbusModels
{
    public class ModbusPropertyInfo
    {
        public string CorrelationId { get; set; } = default!;

        public byte UnitId { get; set; } = default!;

        public string StartAddress { get; set; } = default!;

        public ModbusOperateType OperateType { get; set; } = default!;

        public ModbusOriginalDataType DataType { get; set; } = default!;

        public double? Scaling { get; set; }

        public string? Expression { get; set; }

        public ModbusReportMethod ReportMethod { get; set; }

        public byte? WriteFunctionCode { get; set; }

        public int BitMask { get; set; }

        public int PollingInterval { get; set; } = 1000;
    }
}