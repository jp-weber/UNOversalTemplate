# UNOversal Template Sample Application

A Universal Windows Platform application built on .NET 10 using the UNOversalTemplate framework with native AOT compilation and full trimming support.

## Overview

This sample demonstrates:
- **Dependency Injection** using Microsoft.Extensions.DependencyInjection with AOT compatibility
- **Async Application Lifecycle** with proper async/await patterns
- **Full AOT + Trimming Support** with DynamicallyAccessedMembers attributes and TrimmerRootDescriptor
- **.NET 10 Native Interoperability** with CsWinRT and Windows APIs

## Key Features

### Dependency Injection with AOT Support

The sample uses Microsoft's dependency injection container with AOT-safe generic registration:

```csharp
// In App.xaml.cs RegisterTypes():
registry.RegisterSingleton<ILoggerFacade, DebugLogger>();
registry.RegisterSingleton<IEventAggregator, EventAggregator>();
registry.RegisterSingleton<IMyService, MyServiceImplementation>();  // AOT-safe
registry.RegisterSingleton<IMyService, MyServiceImplementation>("namedInstance");  // Also supported
```

### ViewModel Registration

ViewModels must be registered with the container, either manually or via `RegisterForNavigation` extension method:

```csharp
// Manual registration in App.xaml.cs RegisterTypes():
registry.RegisterForNavigation<MainPage, MainPageViewModel>();

// Mark ViewModels with [DynamicallyAccessedMembers] for AOT/Trimming support:
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class MainPageViewModel 
{
	private readonly ILoggerFacade _logger;

	public MainPageViewModel(ILoggerFacade logger) 
	{ 
		_logger = logger;
	}
}
```

### Navigation Registration (Uno Pattern)

```csharp
// In App.xaml.cs RegisterTypes():
ViewRouter.Register(
	new NavigationViewMap(typeof(MainPage), typeof(MainPageViewModel)),
	new NavigationViewMap(typeof(ShellPage), typeof(ShellPageViewModel))
);
```

## Prerequisites

- **Visual Studio 2026** or later
- **.NET 10 SDK** installed
- **Windows 10/11** with SDK version 10.0.17763 or later
- **CsWinRT** package (automatically included)

## Building & Running

### Restore Dependencies
```bash
dotnet restore UNOversalTemplate_Sample.sln
```

### Build the Project
```bash
# Build with default configuration
dotnet build Samples/UWP_Sample/UWPNetSample_MSIoC.csproj

# Build with AOT validation (trimming analysis)
dotnet build Samples/UWP_Sample/UWPNetSample_MSIoC.csproj -c Release
```

### Run the Application
```bash
dotnet run -p Samples/UWP_Sample/UWPNetSample_MSIoC.csproj
```

## AOT & Trimming Configuration

This project is configured with:
- `<PublishAot>true</PublishAot>` - Enables Native AOT
- `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` - Runtime trimming analysis
- `TrimmerRootDescriptor.xml` - Preserves reflection-required types

### Type Preservation Strategies

1. **Generic Type Parameters** - Use `[DynamicallyAccessedMembers]` on method signatures:
```csharp
registry.RegisterSingleton<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFrom,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTo>(...)
```

2. **Class Constructors** - Annotate injection targets:
```csharp
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)]
public class DebugLogger : ILoggerFacade { ... }
```

3. **Trimmer Configuration** - `TrimmerRootDescriptor.xml` declares namespaces and types to preserve

## Troubleshooting

### Issue: Trimming Warnings or Runtime Type Resolution Failures

**Solution:** Add `[DynamicallyAccessedMembers]` to your services:

```csharp
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class MyService : IMyService { ... }
```

Update `TrimmerRootDescriptor.xml` to preserve your assemblies:

```xml
<assembly fullname="YourNamespace.Services">
	<namespace fullname="YourNamespace.Services" preserve="all" />
</assembly>
```

## Project Structure

```
Samples/UWP_Sample/
├── App.xaml                      # Application definition
├── App.xaml.cs                   # Bootstrapper & DI setup
├── UWPNetSample_MSIoC.csproj     # Project configuration (AOT enabled)
├── TrimmerRootDescriptor.xml     # Type preservation rules
├── ViewModels/
│   └── MainPageViewModel.cs      # [DynamicallyAccessedMembers]
├── Views/
│   ├── MainPage.xaml
│   └── ShellPage.xaml
└── README.md

UNOversalTemplate.Core/
├── Ioc/
│   ├── ContainerRegistryExtensions.cs   # Generic registration with AOT support
│   ├── ContainerLocator.cs
│   └── AutoWireViewModelHelper.cs
├── Navigation/
│   ├── ViewRouter.cs             # Uno-style explicit registration
│   └── NavigationService.cs
├── Mvvm/
│   ├── ViewModelBase.cs
│   └── ViewModelLocationProvider.cs
├── Events/
│   └── EventAggregator.cs        # [DynamicallyAccessedMembers]
├── Logging/
│   ├── ILoggerFacade.cs
│   └── DebugLogger.cs            # [DynamicallyAccessedMembers]
└── UNOversalApplicationBase.cs   # Base application class

UNOversalTemplate.IoC.MS/
└── MSIocContainerExtension.cs    # Microsoft.Extensions.DependencyInjection wrapper
```

## Important Notes

### Dependency Injection Order
The framework ensures proper initialization order:
1. Create container extension
2. Register core services (ILoggerFacade, IEventAggregator)
3. Call user's `RegisterTypes()`
4. Finalize container
5. Resolve logger and begin logging

### Generic Registration with Names
Both parameterless and named registrations are supported with full AOT compatibility:

```csharp
// ✅ Works with AOT
registry.RegisterSingleton<IMyService, MyService>();
registry.RegisterSingleton<IMyService, MyService>("variant1");
registry.RegisterSingleton<IMyService, MyService>("variant2");

// ✅ Factory methods also work
registry.RegisterSingleton<IMyService>(provider => new MyService(...));
```

### Trimming Best Practices

1. **Always use `[DynamicallyAccessedMembers]`** on injectable types
2. **Update TrimmerRootDescriptor.xml** when adding new service namespaces
3. **Test with Release builds** to validate trimming configuration
4. **Use `RegisterForNavigation`** for manual ViewModel/View registration
5. **Avoid dynamic type creation** without explicit preservation

## References

- [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection)
- [Trimming .NET Applications](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)
- [DynamicallyAccessedMembers Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.codeanalysis.dynamicallyaccessedmembersattribute)
- [UNO Platform Documentation](https://uno-platform.github.io/)
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)

## License

This project is part of the UNOversalTemplate framework. See the repository for licensing details.
