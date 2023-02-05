using Prism.Modularity;
#if WINDOWS_UWP
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml.Markup;
#endif

namespace UNOversal.Modularity
{
    /// <summary>
    /// The <see cref="ModuleCatalog"/> holds information about the modules that can be used by the 
    /// application. Each module is described in a <see cref="ModuleInfo"/> class, that records the 
    /// name and type of the module. 
    /// </summary>
    [ContentProperty(Name = nameof(Items))]
    public class ModuleCatalog : ModuleCatalogBase
    {

    }
}
