using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ModbusWorker.Commands;

namespace ModbusWorker.Handlers
{
    public class ModbusRtuMessageEncoder : MessageToByteEncoder<ModbusMessage>
    {
        protected override void Encode(IChannelHandlerContext context, ModbusMessage message, IByteBuffer output)
        {
            context.Channel.Pipeline.Remove<ModbusRtuMessageDecoder>();
            context.Channel.Pipeline.AddBefore(nameof(ModbusMessageHandler), null, new ModbusRtuMessageDecoder());

            ModbusRtuFrame modbusRtuFrame = new(message);

            output.WriteBytes(modbusRtuFrame.Encode());
        }
    }
}