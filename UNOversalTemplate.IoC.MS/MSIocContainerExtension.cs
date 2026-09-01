using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOversal.Ioc.Internals;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace UNOversal.Ioc
{
    public partial class MSIocContainerExtension : IContainerExtension<IServiceProvider>, IContainerInfo, IDisposable
    {
        private readonly ServiceCollection _serviceCollection = new();
        private ServiceProvider _provider;
        private IScopedProvider _currentScope;

        public IServiceProvider Instance { get; private set; }

        static MSIocContainerExtension()
        {
        }

#if !ContainerExtensions
        /// <summary>
        /// Constructs a default instance of the <see cref="MSIocContainerExtension" />
        /// </summary>
        public MSIocContainerExtension()
            : this(new ServiceCollection())
        {
        }

        /// <summary>
        /// Constructs a new <see cref="MSIocContainerExtension" /> with the given service descriptors.
        /// </summary>
        public MSIocContainerExtension(IReadOnlyList<ServiceDescriptor> serviceDescriptors)
        {
            foreach (var descriptor in serviceDescriptors)
            {
                _serviceCollection.Add(descriptor);
            }

            // Defer provider building to FinalizeExtension()
        }

        /// <summary>
        /// Constructs a new <see cref="MSIocContainerExtension" /> with the given service collection.
        /// </summary>
        public MSIocContainerExtension(ServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            // Defer provider building to FinalizeExtension()
        }
#endif

        public IScopedProvider CurrentScope => _currentScope;

        /// <summary>
        /// Used to perform any final steps for configuring the extension that may be required by the container.
        /// </summary>
        public void FinalizeExtension()
        {
            _provider = _serviceCollection.BuildServiceProvider();
            Instance = _provider;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            _serviceCollection.AddSingleton(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            _serviceCollection.Add(new ServiceDescriptor(type, (sp) => instance, ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            _serviceCollection.AddSingleton(from, to);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <param name="name">The name or key to register the service</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            _serviceCollection.AddSingleton(from, to);
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service <see cref="Type" /> factory delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type type, Func<object> factoryMethod)
        {
            _serviceCollection.AddSingleton(type, _ => factoryMethod());
            return this;
        }

        /// <summary>
        /// Registers a Singleton with the given service <see cref="Type" /> factory delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method using <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterSingleton(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            _serviceCollection.AddSingleton(type, _ => factoryMethod(this));
            return this;
        }

        /// <summary>
        /// Registers a Singleton Service which implements service interfaces
        /// </summary>
        /// <param name="type">The implementation <see cref="Type" />.</param>
        /// <param name="serviceTypes">The service <see cref="Type"/>'s.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        /// <remarks>Registers all interfaces if none are specified.</remarks>
        public IContainerRegistry RegisterManySingleton(Type type, params Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                serviceTypes = type.GetInterfaces();
            }

            foreach (var serviceType in serviceTypes)
            {
                _serviceCollection.AddSingleton(serviceType, type);
            }

            return this;
        }

        /// <summary>
        /// Registers a scoped service
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type from, Type to)
        {
            _serviceCollection.AddScoped(from, to);
            return this;
        }

        /// <summary>
        /// Registers a scoped service using a delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type type, Func<object> factoryMethod)
        {
            _serviceCollection.AddScoped(type, _ => factoryMethod());
            return this;
        }

        /// <summary>
        /// Registers a scoped service using a delegate method.
        /// </summary>
        /// <param name="type">The service <see cref="Type"/>.</param>
        /// <param name="factoryMethod">The delegate method using the <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry RegisterScoped(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            _serviceCollection.AddScoped(type, _ => factoryMethod(this));
            return this;
        }

        /// <summary>
        /// Registers a Transient with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type from, Type to)
        {
            _serviceCollection.AddTransient(from, to);
            return this;
        }

        /// <summary>
        /// Registers a Transient with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <param name="name">The name or key to register the service</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type from, Type to, string name)
        {
            _serviceCollection.AddTransient(from, to);
            return this;
        }

        /// <summary>
        /// Registers a Transient Service using a delegate method
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type type, Func<object> factoryMethod)
        {
            _serviceCollection.AddTransient(type, _ => factoryMethod());
            return this;
        }

        /// <summary>
        /// Registers a Transient Service using a delegate method
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="factoryMethod">The delegate method using <see cref="IContainerProvider"/>.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public IContainerRegistry Register(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            _serviceCollection.AddTransient(type, _ => factoryMethod(this));
            return this;
        }

        /// <summary>
        /// Registers a Transient Service which implements service interfaces
        /// </summary>
        /// <param name="type">The implementing <see cref="Type" />.</param>
        /// <param name="serviceTypes">The service <see cref="Type"/>'s.</param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        /// <remarks>Registers all interfaces if none are specified.</remarks>
        public IContainerRegistry RegisterMany(Type type, params Type[] serviceTypes)
        {
            if (serviceTypes.Length == 0)
            {
                serviceTypes = type.GetInterfaces();
            }

            foreach (var serviceType in serviceTypes)
            {
                _serviceCollection.AddTransient(serviceType, type);
            }

            return this;
        }

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type) =>
            Resolve(type, Array.Empty<(Type, object)>());

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <param name="name">The service name/key used when registering the <see cref="Type"/></param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, string name) =>
            Resolve(type, name, Array.Empty<(Type, object)>());

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            try
            {
                // Optimize: Cache the ServiceProvider. Rebuilding it on every single resolve is an expensive reflection operation that freezes the UI during startup/navigation.
                var serviceProvider = _provider ??= _serviceCollection.BuildServiceProvider();

                IServiceProvider provider = _currentScope is ScopedProvider scope 
                    ? scope.ServiceProvider 
                    : serviceProvider;

                using var scope1 = provider.CreateScope();

                // Attempt to resolve from container first, fallback to Activator.CreateInstance if not registered
                var instance = scope1.ServiceProvider.GetService(type);
                return instance ?? Activator.CreateInstance(type)!;
            }
            catch (Exception ex) when (!(ex is ArgumentNullException))
            {
                throw new ContainerResolutionException(type, ex);
            }
        }

        /// <summary>
        /// Resolves a given <see cref="Type"/>
        /// </summary>
        /// <param name="type">The service <see cref="Type"/></param>
        /// <param name="name">The service name/key used when registering the <see cref="Type"/></param>
        /// <param name="parameters">Typed parameters to use when resolving the Service</param>
        /// <returns>The resolved Service <see cref="Type"/></returns>
        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            try
            {
                IServiceProvider provider = _currentScope is ScopedProvider scope 
                    ? scope.ServiceProvider 
                    : (_provider ??= _serviceCollection.BuildServiceProvider());

                using var scope1 = provider.CreateScope();

                if (!string.IsNullOrEmpty(name))
                    return scope1.ServiceProvider.GetRequiredService(type) ?? Activator.CreateInstance(type)!;

                return scope1.ServiceProvider.GetRequiredService(type);
            }
            catch (Exception ex) when (!(ex is ArgumentNullException))
            {
                throw new ContainerResolutionException(type, name, ex);
            }
        }

        /// <summary>
        /// Determines if a given service is registered
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <returns><c>true</c> if the service is registered.</returns>
        public bool IsRegistered(Type type)
        {
            return _serviceCollection.Any(s => s.ServiceType == type);
        }

        /// <summary>
        /// Determines if a given service is registered with the specified name
        /// </summary>
        /// <param name="type">The service <see cref="Type" /></param>
        /// <param name="name">The service name or key used</param>
        /// <returns><c>true</c> if the service is registered.</returns>
        public bool IsRegistered(Type type, string name)
        {
            return _serviceCollection.Any(s => s.ServiceType == type);
        }

        Type IContainerInfo.GetRegistrationType(string key)
        {
            var registration = _serviceCollection.FirstOrDefault(r => 
                (r.ImplementationType?.Name == key) || 
                (r.ServiceType.Name == key));
            return registration?.ImplementationType ?? typeof(void);
        }

        Type IContainerInfo.GetRegistrationType(Type serviceType)
        {
            var registration = _serviceCollection.FirstOrDefault(x => x.ServiceType == serviceType);
            return registration?.ImplementationType;
        }

        /// <summary>
        /// Creates a new Scope
        /// </summary>
        public virtual IScopedProvider CreateScope() =>
            CreateScopeInternal();

        /// <summary>
        /// Creates a new Scope and provides the updated ServiceProvider
        /// </summary>
        protected IScopedProvider CreateScopeInternal()
        {
            var scope = Instance.CreateScope();
            _currentScope = new ScopedProvider(scope.ServiceProvider);
            return _currentScope;
        }

        private partial class ScopedProvider : IScopedProvider
        {
            public IServiceProvider BaseServiceProvider { get; }

            public ScopedProvider(IServiceProvider serviceProvider)
            {
                BaseServiceProvider = serviceProvider;
            }

            public bool IsAttached { get; set; }

            public IScopedProvider CurrentScope => this;

            public IServiceProvider ServiceProvider => BaseServiceProvider;

            public IScopedProvider CreateScope()
            {
                var newScope = BaseServiceProvider.CreateScope();
                return new ScopedProvider(newScope.ServiceProvider);
            }

            public void Dispose()
            {
                // Scope wird vom ServiceProvider Management gehandhabt
            }

            public object Resolve(Type type) =>
                GetRequiredService(type);

            public object Resolve(Type type, string name) =>
                GetRequiredService(type);

            public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
            {
                return GetRequiredService(type);
            }

            public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
            {
                return GetRequiredService(type);
            }

            private object GetRequiredService(Type type)
            {
                try
                {
                    return BaseServiceProvider.GetRequiredService(type);
                }
                catch (Exception ex)
                {
                    throw new ContainerResolutionException(type, ex);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentScope?.Dispose();
            }
        }
    }
}