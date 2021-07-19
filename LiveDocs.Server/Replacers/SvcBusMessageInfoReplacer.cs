using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using LiveDocs.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Replacers
{
    public class SvcBusMessageInfoReplacer : ISvcBusMessageInfoReplacer
    {
        private readonly IAzureRMApiClient _azureRmApiClient;
        private readonly IOptions<StronglyTypedConfig.LiveDocs> _liveDocsOptions;
        private readonly string _subscriptionId;

        public SvcBusMessageInfoReplacer(IAzureRMApiClient azureRmApiClient, IOptions<StronglyTypedConfig.AzureAd> azureAdOptions, IOptions<StronglyTypedConfig.LiveDocs> liveDocsOptions)
        {
            _azureRmApiClient = azureRmApiClient;
            _liveDocsOptions = liveDocsOptions;
            _subscriptionId = liveDocsOptions.Value.SubscriptionId;
        }

        public async Task<string> Render(string queueName)
        {
            var resourceGroup = _liveDocsOptions.Value.ServiceBus.ResourceGroupName;
            var serviceBusNamespace = _liveDocsOptions.Value.ServiceBus.NamespaceName;

            var requestUri = $"subscriptions/{_subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ServiceBus/namespaces/{serviceBusNamespace}/queues/{queueName}?api-version=2017-04-01";
            var stats = await _azureRmApiClient.Query<ServiceBusGetQueueResponse>(requestUri);

            return $"{queueName}  AM fa:fa-envelope-open:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.activeMessageCount} SM fa:fa-clock:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.scheduledMessageCount} DL fa:fa-book-dead:{stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.deadLetterMessageCount}";
        }
    }
}