using Azure;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusDemoWorkerService
{
    public  class ServiceBusClientFactory
    {
        private ServiceBusConfigOption _configOption;
        public ServiceBusClientFactory(IOptions<ServiceBusConfigOption> option)
        {
            _configOption = option.Value;
        }

        public ServiceBusClient GetClient()
        {
            if(_configOption.UseManagedIdentity)
            {
                var defaultCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions() {  ManagedIdentityClientId= _configOption.ManagedIdentity});
              return  new ServiceBusClient(_configOption.NameSpace, defaultCredentials, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            }
            else
            {
              return  new ServiceBusClient(_configOption.NameSpace, new AzureNamedKeyCredential(_configOption.SasKeyName, _configOption.SasKeyValue), new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            }
        }
    }
}
