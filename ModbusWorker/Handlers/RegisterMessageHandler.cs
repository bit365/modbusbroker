using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ModbusWorker.ModbusModels;

namespace ModbusWorker.Handlers
{
    public class RegisterMessageHandler : SimpleChannelInboundHandler<string>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, string msg)
        {
            if (!ctx.HasAttribute(SessionInfo.AttributeKey))
            {
                string deviceSecret = msg;

                ctx.Executor.Execute(async () =>
                {
                    var session = DeviceService.GetSessionInfo(deviceSecret);

                    if (session != null)
                    {
                        ctx.Channel.Pipeline.Remove<StringDecoder>();
                        ctx.Channel.Pipeline.Remove(this);

                        ctx.GetAttribute(SessionInfo.AttributeKey).Set(session);

                        await ModbusBridge.DeviceLoginAsync(ctx.Channel, session).ConfigureAwait(false);

                        if (session.ProtocolType == ModbusProtocolType.ModbusRtu)
                        {
                            ctx.Channel.Pipeline.AddLast(new ModbusRtuMessageDecoder());
                            ctx.Channel.Pipeline.AddLast(new ModbusRtuMessageEncoder());
                        }

                        if (session.ProtocolType == ModbusProtocolType.ModbusTcp)
                        {
                            ctx.Channel.Pipeline.AddLast(new ModbusTcpMessageDecoder());
                            ctx.Channel.Pipeline.AddLast(new ModbusTcpMessageEncoder());
                        }

                        ctx.Channel.Pipeline.AddLast(nameof(ModbusMessageHandler), new ModbusMessageHandler());

                        var propertyInfos = DeviceService.GetExtensionProperties(session.DeviceId);
                        ctx.Channel.Pipeline.AddLast(new DeviceDataPollingHandler(session.DeviceId, propertyInfos));
                    }
                });
            }
        }
    }
}