using Microsoft.Extensions.DependencyInjection;
using System;

namespace StrangeFog.Docker.Monitor.Services
{
    public static class ServicesContainer
    {
        public static IServiceProvider Provider { get; set; }

        public static void Configure(Action<IServiceCollection> servicesConfigurator)
        {
            var collection = new ServiceCollection();
            servicesConfigurator.Invoke(collection);
            Provider = collection.BuildServiceProvider();
        }
    }
}
