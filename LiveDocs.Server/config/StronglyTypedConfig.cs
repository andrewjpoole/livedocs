namespace LiveDocs.Server.config
{
    public class StronglyTypedConfig
    {
        public class AzureAd
        {
            public const string ConfigKey = "AzureAD";
            public string TenantId { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string Resource { get; set; }
        }

        public class ServiceBus
        {
            public string ResourceGroupName { get; set; }
            public string NamespaceName { get; set; }
        }

        public class LiveDocs
        {
            public const string ConfigKey = "LiveDocs";
            public string AzureResourceManagementApiBaseUri { get; set; }
            public string SubscriptionId { get; set; }
            public ServiceBus ServiceBus { get; set; }
            public string ResourceDocumentationFileListing { get; set; }
        }
    }
}
