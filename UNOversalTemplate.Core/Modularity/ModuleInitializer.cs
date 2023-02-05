﻿using UNOversal.Ioc;
using System;
using Prism.Ioc;
using Prism.Modularity;

namespace UNOversal.Modularity
{
    public class ModuleInitializer : IModuleInitializer
    {
        private IContainerExtension _container { get; }

        public ModuleInitializer(IContainerExtension container)
        {
            _container = container;
        }

        public void Initialize(IModuleInfo moduleInfo)
        {
            var module = CreateModule(Type.GetType(moduleInfo.ModuleType, true));
            if (module != null)
            {
                module.RegisterTypes(_container);
                module.OnInitialized(_container);
            }
        }

        protected virtual IModule CreateModule(Type moduleType)
        {
            return (IModule)_container.Resolve(moduleType);
        }
    }
}
