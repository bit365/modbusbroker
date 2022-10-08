using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using ModbusWorker.Commands;
using static ModbusWorker.Commands.ModbusTcpFrame;

namespace ModbusWorker.Handlers
{
    public class ModbusTcpMessageEncoder : MessageToByteEncoder<ModbusMessage>
    {
        private ushort transactionIdentifier = 0;

        protected override void Encode(IChannelHandlerContext context, ModbusMessage message, IByteBuffer output)
        {
            context.Channel.Pipeline.Remove<ModbusTcpMessageDecoder>();
            context.Channel.Pipeline.AddBefore(nameof(ModbusMessageHandler), null, new ModbusTcpMessageDecoder());

            SetTransactionIdentifier();

            ModbusHeader modbusHeader = new(transactionIdentifier);

            ModbusTcpFrame modbusTcpFrame = new(modbusHeader, message);

            output.WriteBytes(modbusTcpFrame.Encode());
        }

        private void SetTransactionIdentifier()
        {
            if (transactionIdentifier < ushort.MaxValue)
            {
                transactionIdentifier++;
            }
            else
            {
                transactionIdentifier = 1;
            }
        }
    }
}