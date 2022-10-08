using ModbusWorker.Commands;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModbusWorker.Handlers
{
    public class ModbusMessageFactory
    {
        private static readonly Lazy<ConcurrentBag<Type>> _lazyModbusMessageTaypes = new(EnsureModbusMessagesInitialized, true);

        private static ConcurrentBag<Type> EnsureModbusMessagesInitialized()
        {
            var exportedTypes = Assembly.GetExecutingAssembly().ExportedTypes;

            var implementationTypes = exportedTypes.Where(t => t.IsAssignableTo(typeof(ModbusMessage)) && t.IsClass && !t.IsAbstract);

            return new ConcurrentBag<Type>(implementationTypes);
        }

        public static ModbusMessage CreateModbusMessage(string messageTypeName, byte unitId)
        {
            var modbusMessageType = _lazyModbusMessageTaypes.Value.SingleOrDefault(e => e.Name == messageTypeName);

            if (modbusMessageType is null)
            {
                throw new NotImplementedException(messageTypeName);
            }

            var message = (ModbusMessage?)Activator.CreateInstance(modbusMessageType, unitId);

            if (message is null)
            {
                throw new NotImplementedException(messageTypeName);
            }

            return message;
        }
    }
}
