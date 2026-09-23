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
            _serviceCollection.AddSingleton<IContainerProvider>(this);
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
                var serviceProvider = _provider ??= _serviceCollection.BuildServiceProvider();

                // Always resolve from the root ServiceProvider, not a scope.
                // Creating multiple scopes on every call is expensive and may not have all services registered.
                IServiceProvider innerProvider = _currentScope is ScopedProvider scope
                    ? scope.ServiceProvider
                    : serviceProvider;

                // Use ActivatorUtilities to create type with explicit ctor params + DI resolution for remaining deps.
                // This matches DryIoc's ConstructorWithResolvableArguments behavior.
                if (parameters != null && parameters.Length > 0)
                {
                    try
                    {
                        // Find best constructor and resolve all its arguments: first from explicit params, rest from DI.
                        var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        var bestCtor = ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

                        if (bestCtor != null)
                        {
                            var ctorParams = bestCtor.GetParameters();
                            var resolvedArgs = new object[ctorParams.Length];
                            bool canResolve = true;

                            for (int i = 0; i < ctorParams.Length; i++)
                            {
                                var pType = ctorParams[i].ParameterType;

                                // Try to match from explicit params by type first
                                bool foundExplicit = false;
                                foreach (var ep in parameters)
                                    if (ep.Type == pType)
                                    {
                                        resolvedArgs[i] = ep.Instance;
                                        foundExplicit = true;
                                        break;
                                    }

                                if (!foundExplicit)
                                {
                                    // Resolve remaining args from DI
                                    var service = innerProvider.GetService(pType);
                                    if (service != null)
                                    {
                                        resolvedArgs[i] = service;
                                    }
                                    else if (pType.IsValueType || ctorParams[i].ParameterType.IsClass == false)
                                    {
                                        canResolve = false;
                                    }
                                }
                            }

                            if (canResolve && resolvedArgs.All(a => a != null))
                            {
                                return bestCtor.Invoke(resolvedArgs);
                            }
                        }
                    }
                    catch
                    {
                        // Some deps weren't available — fall through to fallback
                    }
                }

                // Fallback: attempt DI resolution, then manually resolve ctor args + Activator.CreateInstance
                var instance = innerProvider.GetService(type);
                if (instance != null) return instance;

                // Try to resolve constructor parameters from DI and invoke directly
                TryResolveWithCtorDeps(innerProvider, type, out var manualResult);
                if (manualResult != null) return manualResult;

                // Last resort: no-arg creation
                return Activator.CreateInstance(type)!;
            }
            catch (Exception ex) when (!(ex is ArgumentNullException))
            {
                throw new ContainerResolutionException(type, ex);
            }
        }

        /// <summary>
        /// Attempts to resolve a type by manually resolving all constructor parameters from DI.
        /// </summary>
        private static bool TryResolveWithCtorDeps(IServiceProvider provider, Type type, out object result)
        {
            var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var bestCtor = ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (bestCtor == null)
            {
                result = null;
                return false;
            }

            var ctorParams = bestCtor.GetParameters();
            var resolvedArgs = new object[ctorParams.Length];
            bool canResolve = true;

            for (int i = 0; i < ctorParams.Length; i++)
            {
                var pType = ctorParams[i].ParameterType;
                var service = provider.GetService(pType);

                if (service != null)
                {
                    resolvedArgs[i] = service;
                }
                else if (pType.IsValueType || ctorParams[i].ParameterType.IsClass == false)
                {
                    canResolve = false;
                }
            }

            if (canResolve && resolvedArgs.All(a => a != null))
            {
                try
                {
                    result = bestCtor.Invoke(resolvedArgs);
                    return true;
                }
                catch
                {
                    result = null;
                    return false;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Resolves a given service by registered name/key.
        /// </summary>
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
                var innerProvider = scope1.ServiceProvider;

                if (!string.IsNullOrEmpty(name))
                {
                    var instance = innerProvider.GetService(type);
                    if (instance != null) return instance;

                    if (TryResolveWithCtorDeps(innerProvider, type, out var result))
                        return result;

                    return Activator.CreateInstance(type)!;
                }

                return provider.GetRequiredService(type);
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
                if (parameters != null && parameters.Length > 0)
                {
                    var resolvedArgs = new object[type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Max(c => c.GetParameters().Length)];
                    var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var bestCtor = ctors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

                    if (bestCtor != null)
                    {
                        var ctorParams = bestCtor.GetParameters();
                        resolvedArgs = new object[ctorParams.Length];
                        bool canResolve = true;

                        for (int i = 0; i < ctorParams.Length; i++)
                        {
                            var pType = ctorParams[i].ParameterType;
                            foreach (var ep in parameters)
                                if (ep.Type == pType)
                                {
                                    resolvedArgs[i] = ep.Instance;
                                    break;
                                }

                            if (resolvedArgs[i] == null)
                            {
                                var service = BaseServiceProvider.GetService(pType);
                                if (service != null) resolvedArgs[i] = service;
                                else if (pType.IsValueType || ctorParams[i].ParameterType.IsClass == false) canResolve = false;
                            }
                        }

                        if (canResolve && resolvedArgs.All(a => a != null))
                        {
                            try { return bestCtor.Invoke(resolvedArgs); }
                            catch { }
                        }
                    }
                }

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