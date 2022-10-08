using DotNetty.Transport.Channels;
using ModbusWorker.Commands;

namespace ModbusWorker.Handlers
{
    public class ModbusMessageHandler : SimpleChannelInboundHandler<ModbusMessage>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, ModbusMessage msg)
        {
            if (ctx.HasAttribute(SessionInfo.AttributeKey))
            {
                var sessionInfo = ctx.GetAttribute(SessionInfo.AttributeKey).Get();
                sessionInfo.Awaitable?.TrySetResult(msg);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            var sessionInfo = context.GetAttribute(SessionInfo.AttributeKey).Get();

            sessionInfo?.Awaitable?.TrySetException(exception);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var sessionInfo = context.GetAttribute(SessionInfo.AttributeKey).GetAndRemove();

            if (sessionInfo is not null)
            {
                sessionInfo.Awaitable?.TrySetCanceled();
                ModbusBridge.DeviceLogout(sessionInfo.DeviceId);
            }

            base.ChannelInactive(context);
        }
    }
}
