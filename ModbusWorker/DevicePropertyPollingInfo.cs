using ModbusWorker.ModbusModels;

namespace ModbusWorker
{
    public class DevicePropertyPollingInfo
    {
        public ModbusPropertyInfo Property { get; set; } = default!;

        public DateTimeOffset PollingTime { get; set; } = DateTimeOffset.MinValue;

        public object? PropertyValue { get; set; }
    }
}
