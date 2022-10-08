using DotNetty.Common.Utilities;
using ModbusWorker.Commands;
using ModbusWorker.ModbusModels;

namespace ModbusWorker
{
    public class SessionInfo
    {
        public static readonly AttributeKey<SessionInfo> AttributeKey = AttributeKey<SessionInfo>.ValueOf(nameof(SessionInfo));

        public string DeviceId { get; set; } = default!;

        public string DeviceName { get; set; } = default!;

        public ModbusProtocolType ProtocolType { get; set; }

        public TaskCompletionSource<ModbusMessage> Awaitable { get; set; } = default!;

        public SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1);
    }
}
