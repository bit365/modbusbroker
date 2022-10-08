using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace ModbusWorker.Handlers
{
    public class HeartBeatHandler : ChannelHandlerAdapter
    {
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent stateEvent && stateEvent.State == IdleState.ReaderIdle)
            {
                context.CloseAsync();
                return;
            }

            base.UserEventTriggered(context, evt);
        }
    }
}