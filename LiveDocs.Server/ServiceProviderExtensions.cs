using System;

namespace LiveDocs.Server
{
    public static class ServiceProviderExtensions
    {
        public static object GetServiceByRegisteredTypeName(this IServiceProvider serviceProvider, string name)
        {
            var desiredType = Type.GetType($"LiveDocs.Server.Replacers.{name}");
            return serviceProvider.GetService(desiredType);
        }
    }
}