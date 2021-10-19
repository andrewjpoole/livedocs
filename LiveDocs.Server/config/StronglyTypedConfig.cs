namespace LiveDocs.Server.config
{
    public class StronglyTypedConfig
    {
        public class AzureAd
        {
            public const string ConfigKey = "AzureAD";
            public string TenantId { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;
            public string Resource { get; set; } = string.Empty;
        }

        public class ServiceBus
        {
            public string ResourceGroupName { get; set; } = string.Empty;
            public string NamespaceName { get; set; } = string.Empty;
        }

        public class LiveDocs
        {
            public const string ConfigKey = "LiveDocs";
            public string AzureDevOpsPat { get; set; } = string.Empty;
            public string AzureResourceManagementApiBaseUri { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            public ServiceBus ServiceBus { get; set; } = new();
            public string ResourceDocumentationFileListing { get; set; } = string.Empty;
        }
    }
}
