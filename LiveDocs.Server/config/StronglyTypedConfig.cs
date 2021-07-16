using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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

        public class File
        {
            public string Name { get; set; }
            public string MdPath { get; set; }
            public string JsonPath { get; set; }
        }

        public class LiveDocs
        {
            public const string ConfigKey = "LiveDocs";
            public string AzureResourceManagementApiBaseUri { get; set; }
            public string SubscriptionId { get; set; }
            public ServiceBus ServiceBus { get; set; }
            public List<File> Files { get; set; }
        }
    }
}
