using System;
using Microsoft.Extensions.DependencyInjection;
using UNOversal.Ioc;
using ExceptionExtensions = System.ExceptionExtensions;


namespace UNOversal.Ioc
{
    /// <summary>
    /// Base application class that uses <see cref="MSIocContainerExtension"/> as its container.
    /// </summary>
    public abstract partial class UNOversalApplication : UNOversalApplicationBase
    {
        /// <summary>
        /// Configures additional service descriptors for the container
        /// </summary>
        /// <param name="services">The <see cref="ServiceCollection"/> to configure.</param>
        protected virtual void ConfigureServiceCollection(ServiceCollection services)
        {
        }

        /// <summary>
        /// Create a new <see cref="MSIocContainerExtension"/> used by Prism.
        /// </summary>
        /// <returns>A new <see cref="MSIocContainerExtension"/>.</returns>
        protected override IContainerExtension CreateContainerExtension()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServiceCollection(serviceCollection);
            return new MSIocContainerExtension(serviceCollection);
        }

        /// <summary>
        /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
        /// root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected override void RegisterFrameworkExceptionTypes()
        {
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(InvalidOperationException));
        }
    }
}
