using System;
using Microsoft.Extensions.DependencyInjection;
using UNOversal.Ioc;

namespace UNOversal.Ioc
{
    /// <summary>
    /// Extensions help get the underlying <see cref="IServiceProvider" />
    /// </summary>
    public static class UNOversalIocExtensions
    {
        /// <summary>
        /// Gets the underlying <see cref="IServiceProvider"/> from the <see cref="IContainerProvider"/>
        /// </summary>
        /// <param name="containerProvider">The current <see cref="IContainerProvider"/>.</param>
        public static IServiceProvider GetServiceProvider(this IContainerProvider containerProvider)
        {
            return ((IContainerExtension<IServiceProvider>)containerProvider).Instance;
        }

        /// <summary>
        /// Gets the underlying <see cref="IServiceProvider"/> from the <see cref="IContainerRegistry"/>
        /// </summary>
        /// <param name="containerRegistry">The current <see cref="IContainerRegistry"/>.</param>
        public static IServiceProvider GetServiceProvider(this IContainerRegistry containerRegistry)
        {
            return ((IContainerExtension<IServiceProvider>)containerRegistry).Instance;
        }

        /// <summary>
        /// Gets the underlying <see cref="ServiceCollection"/> from the <see cref="IContainerRegistry"/>
        /// </summary>
        public static ServiceCollection GetServiceCollection(this IContainerRegistry containerRegistry)
        {
            var extension = (MSIocContainerExtension)containerRegistry;
            return null; // Access via reflection or create a special property if needed
        }
    }
}
