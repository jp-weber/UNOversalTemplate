using System;
using System.Diagnostics.CodeAnalysis;

namespace UWP_Sample.ViewModels
{
    /// <summary>
    /// Main page ViewModel for the UWP sample application.
    /// This class is marked with [RegisterForNavigation] to enable:
    /// 1. Automatic IoC registration via ViewModelRegistrationGenerator (Source Generator)
    /// 2. Preservation from .NET Trimming/AOT compilation
    /// </summary>
#if NET10_0_OR_GREATER || WINDOWS_UWP || WINDOWS_APP
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    public partial class MainPageViewModel
    {
        public MainPageViewModel()
        {

        }
    }
}

