using System;

namespace LiveDocs.Server
{
    public static class ServiceProviderExtensions
    {
        public static object GetServiceByRegisteredTypeName(this IServiceProvider serviceProvider, string name)
        {
            var typeName = $"LiveDocs.Server.Replacers.{name}";
            var desiredType = Type.GetType(typeName) ?? throw new Exception($"Could not reolve replacer type {typeName}");
            return serviceProvider.GetService(desiredType) ?? throw new Exception($"Could not find type {typeName} in service provider DI container");
        }
    }
}