using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;

namespace ServiceBusDemoWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusConfigOption _configOption;
        private readonly TelemetryClient _telemetryClient;
        private readonly ServiceBusClientFactory _factory;


        private ServiceBusClient _serviceBusClient;
        private ServiceBusProcessor _queue1Processor;
        private ServiceBusProcessor _queue2Processor;
        private ServiceBusSender _secondQueueSender;

        public Worker(ILogger<Worker> logger, IOptions<ServiceBusConfigOption> option,
            TelemetryClient client,
            ServiceBusClientFactory clientFactory)
        {
            _logger = logger;
            _configOption = option.Value;
            _telemetryClient = client;
            _factory = clientFactory;

            _serviceBusClient = _factory.GetClient();

            _queue1Processor = _serviceBusClient.CreateProcessor(_configOption.FirstQueue, new ServiceBusProcessorOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });
            _queue2Processor = _serviceBusClient.CreateProcessor(_configOption.SecondQueue, new ServiceBusProcessorOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });
            _secondQueueSender = _serviceBusClient.CreateSender(_configOption.SecondQueue);
            _queue1Processor.ProcessMessageAsync += _queue1Processor_ProcessMessageAsync;
            _queue1Processor.ProcessErrorAsync += _queue1Processor_ProcessErrorAsync;
            _queue2Processor.ProcessMessageAsync += _queue2Processor_ProcessMessageAsync;
            _queue2Processor.ProcessErrorAsync += _queue2Processor_ProcessErrorAsync;
        }

        private Task _queue2Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task _queue1Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task _queue2Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("operation"))
            {
                _logger.LogInformation($"Received message in queue 2 {arg.Message.Body.ToString()}");
                await arg.CompleteMessageAsync(arg.Message);
            }
        }

        private async Task _queue1Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("operation"))
            {
                string message = arg.Message.Body.ToString();
                _logger.LogInformation($"Received message in queue 2 {message}");
             //   _telemetryClient.TrackEvent("MessageReceivedQueue1", new Dictionary<string,StringHandle>() { })
                string newMessage = message + "First Queue ACK";
                _logger.LogInformation($"Modified message {newMessage}");
                await _secondQueueSender.SendMessageAsync(new ServiceBusMessage(newMessage));
                _logger.LogInformation($"Sent Modified message {newMessage} to Queue 2");
                await arg.CompleteMessageAsync(arg.Message);
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _queue1Processor.StartProcessingAsync();
            await _queue2Processor.StartProcessingAsync();
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //   _secondQueueSender.SendMessageAsync(new ServiceBusMessage("Stand Aline"));
            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //}



                //       await Task.Delay(1000, stoppingToken);
            }

            await _queue1Processor.StopProcessingAsync();
            await _queue2Processor.StartProcessingAsync();
            await _queue1Processor.DisposeAsync();
            await _queue2Processor.DisposeAsync();

            //_queue1Processor.StopProcessingAsync();
            //_queue2Processor.StopProcessingAsync();
        }
    }
}
