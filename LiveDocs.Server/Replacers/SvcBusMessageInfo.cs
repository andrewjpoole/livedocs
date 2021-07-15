using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LiveDocs.Server.Models;
using LiveDocs.Server.Services;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server.Replacers
{
    public class SvcBusMessageInfo : IReplacer
    {
        private readonly IAzureRMApiClient _azureRmApiClient;
        private readonly IConfiguration _serviceBusConfiguration;
        private readonly string _subscriptionId;

        public SvcBusMessageInfo(IAzureRMApiClient azureRmApiClient, IConfiguration configuration)
        {
            _azureRmApiClient = azureRmApiClient;
            _subscriptionId = configuration.GetSection("livedocs")["subscriptionId"];
            _serviceBusConfiguration = configuration.GetSection("livedocs").GetSection("serviceBus");
        }

        public async Task<string> Render(string queueName)
        {
            var resourceGroup = _serviceBusConfiguration["resourceGroupName"];
            var serviceBusNamespace = _serviceBusConfiguration["namespaceName"];

            var requestUri = $"subscriptions/{_subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ServiceBus/namespaces/{serviceBusNamespace}/queues/{queueName}?api-version=2017-04-01";
            var stats = await _azureRmApiClient.Query<ServiceBusGetQueueResponse>(requestUri);

            return $"{queueName}  AM fa:fa-envelope-open:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.activeMessageCount} SM fa:fa-clock:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.scheduledMessageCount} DL fa:fa-book-dead:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.deadLetterMessageCount}";
        }
    }
}