using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveDocs.Server.config;
using LiveDocs.Server.Models;
using LiveDocs.Server.Services;
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
            
            var localDevMachineNamePrefix = Debugger.IsAttached ? $"{Environment.MachineName}-" : string.Empty;
            localDevMachineNamePrefix = "";
            queueName = $"{localDevMachineNamePrefix}{queueName}";

            var requestUri = $"subscriptions/{_subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ServiceBus/namespaces/{serviceBusNamespace}/queues/{queueName}?api-version=2017-04-01";
            var stats = await _azureRmApiClient.Query<ServiceBusGetQueueResponse>(requestUri);

            var activeMessageCount = stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.activeMessageCount;
            var scheduledMessageCount = stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.scheduledMessageCount;
            var deadLetterMessageCount = stats.ServiceBusQueueProperties.ServiceBusQueueCountDetails.deadLetterMessageCount;

            var dlIcon = deadLetterMessageCount == 0 ? "far:fa-trash-alt" : "fas:fa-trash-alt";

            return @$"""{queueName} <br /> far:fa-envelope-open {activeMessageCount} | far:fa-clock {scheduledMessageCount} | {dlIcon} {deadLetterMessageCount}""";
        }
    }
}