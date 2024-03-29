using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ModbusWorker.Handlers;
using System.Text;
using System.Text.Json;

namespace ModbusWorker
{
    public class Worker : BackgroundService
    {
        private readonly IEventLoopGroup _bossGroup = new MultithreadEventLoopGroup();
        private readonly IEventLoopGroup _workerGroup = new MultithreadEventLoopGroup();

        private IChannel _boundChannel = default!;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            InternalLoggerFactory.DefaultFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "[HH:mm:ss]";
            }));

            var bootstrap = new ServerBootstrap().Group(_bossGroup, _workerGroup).Channel<TcpServerSocketChannel>();

            bootstrap.Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler(DotNetty.Handlers.Logging.LogLevel.INFO))
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new IdleStateHandler(TimeSpan.FromMinutes(5), TimeSpan.Zero, TimeSpan.Zero));
                    pipeline.AddLast(new HeartBeatHandler());
                    pipeline.AddLast(new LoggingHandler(DotNetty.Handlers.Logging.LogLevel.INFO));
                    pipeline.AddLast(new StringDecoder(Encoding.ASCII), new RegisterMessageHandler());
                }));

            _boundChannel = await bootstrap.BindAsync(502);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _boundChannel.CloseAsync();

            await Task.WhenAll(
                  _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                  _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}