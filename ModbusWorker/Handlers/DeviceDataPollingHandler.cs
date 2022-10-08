using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using ModbusWorker.ModbusModels;

namespace ModbusWorker.Handlers
{
    public class DeviceDataPollingHandler : ChannelHandlerAdapter
    {
        private readonly TimeSpan _pollingTime;

        private readonly string _deviceId;

        private readonly List<DevicePropertyPollingInfo> _pollingInfos;

        private CancellationTokenSource _stoppingToken = default!;

        private Task _pollingTask = default!;

        public static event EventHandler<PropertyValueChangedEventArgs>? PropertyValueChanged;

        public class PropertyValueChangedEventArgs : EventArgs
        {
            public string DeviceId { get; set; } = default!;

            public string CorrelationId { get; set; } = default!;

            public object? OldValue { get; set; }

            public object? NewValue { get; set; }
        }

        public DeviceDataPollingHandler(string deviceId, IEnumerable<ModbusPropertyInfo> propertyInfos, TimeSpan? pollingTime = null)
        {
            _deviceId = deviceId;
            _pollingInfos = propertyInfos.Select(e => new DevicePropertyPollingInfo { Property = e }).ToList();
            _pollingTime = pollingTime ?? TimeSpan.FromSeconds(1);
        }

        private async Task HandlePolling(IChannelHandlerContext context, CancellationToken cancellationToken = default)
        {
            foreach (var pollingInfo in _pollingInfos)
            {
                bool needSend = DateTimeOffset.Now.Subtract(pollingInfo.PollingTime) > TimeSpan.FromMilliseconds(pollingInfo.Property.PollingInterval);

                if (!cancellationToken.IsCancellationRequested && needSend)
                {
                    pollingInfo.PollingTime = DateTimeOffset.Now;

                    try
                    {
                        var result = await ModbusBridge.GetPropertyValueAsync(_deviceId, pollingInfo.Property);

                        PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs
                        {
                            DeviceId = _deviceId,
                            CorrelationId = pollingInfo.Property.CorrelationId,
                            OldValue = pollingInfo.PropertyValue,
                            NewValue = result
                        });

                        pollingInfo.PropertyValue = result;
                    }
                    catch { }
                }
            }

            _pollingTask = context.Executor.ScheduleAsync(async ctx =>
            {
                await HandlePolling((IChannelHandlerContext)ctx, _stoppingToken.Token);
            }, context, _pollingTime, _stoppingToken.Token);
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            if (context.Channel.Active && context.Channel.Registered)
            {
                // channelActive() event has been fired already, which means this.channelActive() will
                // not be invoked. We have to initialize here instead.
                Initialize(context);
            }
        }
        public override void HandlerRemoved(IChannelHandlerContext context) => Destroy();

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            // Initialize early if channel is active already.
            if (context.Channel.Active)
            {
                Initialize(context);
            }

            base.ChannelRegistered(context);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            // This method will be invoked only if this handler was added
            // before channelActive() event is fired.  If a user adds this handler
            // after the channelActive() event, initialize() will be called by beforeAdd().
            Initialize(context);
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Destroy();
            base.ChannelInactive(context);
        }

        private void Initialize(IChannelHandlerContext context)
        {
            if (_stoppingToken is null)
            {
                _stoppingToken = new CancellationTokenSource();

                _pollingTask = context.Executor.ScheduleAsync(async ctx =>
                {
                    await HandlePolling((IChannelHandlerContext)ctx, _stoppingToken.Token);
                }, context, TimeSpan.FromMilliseconds(10), _stoppingToken.Token);
            }
        }

        private void Destroy()
        {
            _stoppingToken?.Cancel();
            _pollingTask?.Dispose();
        }
    }
}