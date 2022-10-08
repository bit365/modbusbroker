using ModbusWorker.Commands.Request;
using ModbusWorker.Handlers;
using System.Text.Json;

namespace ModbusWorker
{
    public class TestWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool state = false;

            DeviceDataPollingHandler.PropertyValueChanged += DeviceDataPollingHandler_PropertyValueChanged;

            while (!stoppingToken.IsCancellationRequested)
            {
                WriteSingleCoilRequest request = new(2, (ushort)Random.Shared.Next(0, 3), state);

                state = !state;

                try
                {
                    var result = await ModbusBridge.SendingAsync("001", request, cancellationToken: stoppingToken);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private void DeviceDataPollingHandler_PropertyValueChanged(object? sender, DeviceDataPollingHandler.PropertyValueChangedEventArgs e)
        {
            Console.WriteLine($"PropertyValueChanged:{JsonSerializer.Serialize(e)}");
        }
    }
}