# UNOversalTemplate — Strukturielle Übersicht

## 1. Projektübersicht

```
UNOversalTemplate/
├── UNOversalTemplate.Core/                          [.shproj – Shared Project]
│   ├── UNOversalApplicationBase.cs                  ← Base Application (abstract)
│   ├── Navigation/                                  ← Navigationsinfrastruktur
│   ├── Ioc/                                         ← IoC-Abstraktionsschicht
│   ├── Mvvm/                                        ← MVVM-Basis-Klassen
│   ├── Logging/                                     ← Logger-Abstraktion
│   ├── Events/                                      ← EventAggregator
│   ├── Modularity/                                  ← Modulverwaltung (commented out)
│   └── Services/ ...                                ← (separate shproj, s.u.)
│
├── UNOversalTemplate.Services/                      [.shproj – Shared Project]
│   ├── Dialogs/                                     ← DialogServices
│   ├── File/                                        ← FileService
│   ├── Gesture/                                     ← GestureService
│   ├── Logging/                                     ← DebugLogger etc.
│   ├── Network/                                     ← ConnectivityService
│   ├── Secrets/                                     ← Secrets-Storage
│   ├── Serialization/                               ← JsonSerializer
│   ├── Settings/                                    ← SettingsService
│   └── Window/                                      ← WindowHelpers
│
├── UNOversalTemplate.IoC.DryIoC/                   [.shproj – DryIoc-Implementation]
│   ├── DryIocContainerExtension.cs                  ← IContainerExtension für DryIoc
│   ├── UNOversalApplication.cs                      ← Base App mit DryIoc-Container
│   └── UNOversalIocExtensions.cs                    ← Helper-Extensions
│
├── UNOversalTemplate.IoC.MS/                        [.shproj – MS.DI-Implementation]
│   ├── MSIocContainerExtension.cs                   ← IContainerExtension für MS.DI
│   ├── UNOversalApplication.cs                      ← Base App mit MS.DI-Container
│   └── UNOversalIocExtensions.cs                    ← Helper-Extensions
│
└── Samples/
	├── UWP_Sample/
	│   └── UWPNetSample_MSIoC.csproj                → verwendet: UNOversalTemplate.IoC.MS
	└── UnoSample_DryIoC/
		└── UnoSample_DryIoC.csproj                  → verwendet: UNOversalTemplate.IoC.DryIoC
```

---

## 2. Kernarchitektur — Schichtenmodell

```
┌─────────────────────────────────────────────────┐
│              Application Layer                   │
│         (App.xaml.cs derives from ...)          │
│                                                 │
│   UWPNetSample_MSIoC: App : UNOversalApplication│
│        UNOversalTemplate.IoC.MS                  │
│                                                 │
│   UnoSample_DryIoC:     App : UNOversalApplication│
│        UNOversalTemplate.IoC.DryIoC              │
└───────────────┬─────────────────────────────────┘
				│ inherits / implements
┌───────────────v─────────────────────────────────┐
│         UNOversalApplicationBase                 │
│       (UNOversalTemplate.Core)                   │
│                                                  │
│  - Application.Current Single-Accessor           │
│  - InternalInitialize()                          │
│    ├─ ContainerLocator.SetContainerExtension()   │
│    ├─ RegisterRequiredTypes(containerRegistry)   │
│    ├─ RegisterTypes(containerRegistry) [VIRT.]   │
│    ├─ RegisterInternalTypes()                    │
│    └─ FinalizeExtension()                        │
│  - InternalStartAsync()                          │
│    ├─ OnInitialize() / OnInitializeAsync()       │
│    ├─ CreateShell() → UIElement           [ABST]│
│    ├─ InitializeShell(shell)                     │
│    └─ OnStart() / OnStartAsync()           [VIRT]│
│                                                  │
│  Abstrakte/virtuelle Hooks für Derived Classes:   │
│  - abstract CreateContainerExtension() → ICont.Ext│
│  - abstract RegisterTypes(IContainerRegistry)     │
│  - abstract CreateShell() → UIElement             │
│  - virtual OnStart(), OnStartAsync()              │
│  - virtual OnStop(), OnStopAsync()                │
│  - virtual ConfigureViewModelLocator()            │
│  - virtual RegisterFrameworkExceptionTypes()      │
└───────────────┬─────────────────────────────────┘
				│ interfaces
┌───────────────v─────────────────────────────────┐
│         IoC Abstraction Layer                    │
│       (UNOversalTemplate.Core/Ioc/)              │
│                                                  │
│  ┌──────────────┐      ┌──────────────────────┐ │
│  │IContainerExt.│◄────▶│IContainerProvider    │ │
│  │<TContainer>  │      │                      │ │
│  ├──────────────┤      ├──────────────────────┤ │
│  │Instance {get}│      │Resolve(Type)          │ │
│  │FinalizeExt() │      │Resolve(Type, name)    │ │
│  └──────┬───────┘      │CreateScope()          │ │
│         │              └──────────┬───────────┘ │
│         │                         │              │
│  ┌──────v───────┐      ┌─────────v───────────┐  │
│  │IContainerReg.│      │IScopedProvider       │  │
│  ├──────────────┤      ├─────────────────────┐│
│  │RegisterInst. │      │Resolve(Type)         ││
│  │RegisterSingl.│      │CreateScope()         ││
│  │Register(...)/│      │Dispose()             ││
│  │RegisterMany()│      └──────────────────────┘│
│  │RegisterScoped│                                 │
│  │IsRegistered()│                                 │
│  └──────────────┘                                 │
│                                                  │
│  ContainerLocator (static)                       │
│  ├─ static IContainerExtension Current           │
│  ├─ static SetContainerExtension(Func)           │
│  └─ static ResetContainer()                      │
└──────────────────────────────────────────────────┘
				│ Concrete Implementations
	┌───────────┴───────────┐
	│                        │
┌───v───────┐        ┌──────v────────┐
│DryIoc     │        │ MS.DI         │
│Container  │        │ Container     │
│Extension  │        │ Extension     │
└───────────┘        └───────────────┘
```

---

## 3. IoC-Komponenten im Detail

### 3.1 Abstraktionsschicht (`UNOversalTemplate.Core/Ioc/`)

| Datei | Typ | Namespace | Verantwortung |
|-------|-----|-----------|---------------|
| `IContainerExtension.cs` | Interface | `UNOversal.Ioc` | Kombiniert `IContainerProvider` + `IContainerRegistry`, stark typiert via `<TContainer>` |
| `IContainerProvider.cs` | Interface | `UNOversal.Ioc` | `Resolve(Type)`, `Resolve(Type, name, params)`, `CreateScope()` |
| `IContainerRegistry.cs` | Interface | `UNOversal.Ioc` | Registrierungen: `RegisterInstance`, `RegisterSingleton`, `Register`(Transient), `RegisterScoped`, `RegisterMany`, `IsRegistered` |
| `ContainerLocator.cs` | Class (static) | `UNOversal.Ioc` | Globaler Container-Zugriff via Lazy-Initialisierung |
| `ContainerProvider.cs` | Class | `UNOversal.Ioc` | Wrapper-Implementierung für `IContainerProvider` |
| `ContainerExtensionExtensions.cs` | Static | `UNOversal.Ioc` | Helper-Extensions |
| `ContainerRegistryExtensions.cs` | Static | `UNOversal.Ioc` | Registrierungs-Helfer |
| `ContainerResolutionException.cs` | Exception | `UNOversal.Ioc` | Spezifische Excepcion bei Resolve-Fehlern |

### 3.2 DryIoc-Implementation (`UNOversalTemplate.IoC.DryIoC/`)

| Datei | Klasse / Interface | Verantwortung |
|-------|-------------------|---------------|
| `DryIocContainerExtension.cs` | `DryIocContainerExtension : IContainerExtension<IContainer>, IContainerInfo, IDisposable` | Vollständige.Adapter-Implementierung für DryIoc `Container` mit Rules-Support (`DefaultRules`) |
| `UNOversalApplication.cs` | `UNOversalApplication : UNOversalApplicationBase` | Base-Class — setzt `DryIocContainerExtension(new Container(CreateContainerRules()))`. Virtuelle Methode `CreateContainerRules()` für Custom-Rules |
| `UNOversalIocExtensions.cs` | `UNOversalIocExtensions (static)` | `GetServiceProvider(IContainerProvider/IContainerRegistry)` Extensions |

### 3.3 MS.DI-Implementation (`UNOversalTemplate.IoC.MS/`)

| Datei | Klasse / Interface | Verantwortung |
|-------|-------------------|---------------|
| `MSIocContainerExtension.cs` | `MSIocContainerExtension : IContainerExtension<IServiceProvider>, IContainerInfo, IDisposable` | Adapter für `Microsoft.Extensions.DependencyInjection.ServiceCollection` → `ServiceProvider` |
| `UNOversalApplication.cs` | `UNOversalApplication : UNOversalApplicationBase` | Base-Class — setzt `MSIocContainerExtension(new ServiceCollection())`. Virtuelle Methode `ConfigureServiceCollection(ServiceCollection)` für Custom-Registrierungen |
| `UNOversalIocExtensions.cs` | `UNOversalIocExtensions (static)` | `GetServiceProvider(IContainerProvider/IContainerRegistry)` Extensions. `GetServiceCollection()` stub (nicht implementiert) |
| | | |

### 3.4 Registrierungsablauf (Startup-Sequenz)

```
App-Konstruktor:
 │
 ├─ InternalInitialize()                      [UNOversalApplicationBase]
 │   │
 │   ├─ ContainerLocator.SetContainerExtension(factory)     // Lazy-Registry
 │   ├─ _containerExtension = ContainerLocator.Current      // Factory wird aufgerufen
 │   │                                                  // → CreateContainerOverride abgeleitet Klasse (DryIoc MS.DI)
 │   │
 │   ├─ registry.RegisterSingleton<ILoggerFacade>(...)    // Kern-Dienste vor user-Code
 │   ├─ registry.RegisterSingleton<IEventAggregator,  >
 │   │
 │   ├─ RegisterRequiredTypes(_containerExtension)        // VIRT — Core-Typen registrieren
 │   │  └─ MS.DI:         INavigationService (mit Frame-lambda)
 │   │  └─ DryIoc:      (keine speziellen Core-Typen)
 │   │
 │   ├─ RegisterTypes(_containerExtension)                // ABSTR — USER: RegisterTypen
 │   │
 │   ├─ RegisterInternalTypes(registry)                   // VIRT
 │   │
 │   └─ _containerExtension.FinalizeExtension()          // Container finalisieren
 │
 ├─ RegisterFrameworkExceptionTypes()                    // VIRT
 │
 ├─ _logger = Container.Resolve<ILoggerFacade>()        // Logging aktivieren
 │
 └─ ConfigureViewModelLocator()                         // MVVM-Location setzen
	  │
	  └─ ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
			  _containerExtension.ResolveViewModelForView(view, type))
```

---

## 4. Navigationsinfrastruktur (`UNOversalTemplate.Core/Navigation/`)

```
Navigation/
├── Abstract/                                       [Interfaces]
│   ├── INavigationService.cs                       ← Haupt-Interface
│   ├── INavigationService2.cs                      →扩展 (FrameFacade access)
│   ├── INavigationResult.cs                        ← Ergebnis-Typ
│   ├── INavigationParameters.cs                    ← Schlüssel-Wert-Paare
│   ├── NavigationPath.cs                           [class]
│   ├── INavigationAware.cs                         ← VM-Notify während Nav.
│   └── ...
│
├── FrameFacade.cs                                  [class]
│   ├─ Wrappt XAML Frame auf plattformübergreifende Weise    │
│   ├─ Implementiert: IFrameFacade, IFrameFacade2        │
│   ├─ CanGoBack/CanGoForward-Events                    │
│   └─ NavigateAsync/GofaBack/GoldenForward async       │
│
├── NavigationService.cs                              [class]
│   ├─ wrappt XAML Frame auf plattformübergreifende Weise    │
│   ├─ Implementiert: INavigationService, IFrameFacade2  │
│   ├─ Static: Dictionary<Frame, INavService> Instances │
│   └─ Delegates alle Nav-Aufrufe an _frame (IFrameFacade) │
│
├── NavigationFactory.cs                              [class]
│   ├─ Create(id?)       → default Frame + NavService    │
│   └─ Create(Frame, id?) → NavService für exist. Frame  │
│
├── PageNavigationRegistry.cs                         [class]
│   ├─ Static Dictionary<string, PageNavigationInfo>   │
│   ├─ Register(key, (View, VM))                       │
│   └─ TryGetRegistration(...) / GetPageType(...)      │
│
├── FrameFacadeExtensions.cs                          [static classes]
├── NavigationEventArgsExtensions.cs                  [event helpers]
├── NavigationMode.cs                                 [enum: Navigate/Back/Forward]
├── NavigationParametersExtensions.s                  [helper]
└── NavigationPath.cs                                 [class]
```

### 4.1 `INavigationService` — Hauptinterface

| Member | Signatur | Beschreibung |
|--------|----------|-------------|
| `NavigateAsync(string name)` | `Task<INavigationResult>` | Navigiert zu Seitennamen (z.B. "MainPage") |
| `NavigateAsync(string name, params parameters)` | `Task<INavigationResult>` | Mit Key-Value Parameters |
| `NavigateAsync(Uri uri)` | `Task<INavigationResult>` | Mit URI |
| `NavigateAsync(Uri, INatigationParameters)` | `NavigationParameters` | Mit Parameters |
| `GoBackAsync()` | `Task<INavigationResult>` | Go back |
| `GoForwardAsync()` | `Task<INavigationResult>` | Go forward |
| `CanGoBack()` / `CanGoForward()` | `bool` | Zustandsabfrage |
| Events | `CanGoBackChanged`, `CanGoForwardChanged` | Events |

### 4.2 Navigationsweg während Shell-Initialisierung (UWPNetSample_MSIoC)

```
App.OnStartAsync():
 └─ shell = new ShellPage()
	└─ ShellPage-Konstruktor:
	   ├─ NavigationService = NavigationFactory.Create(MainFrame)
		 .AttachGestures(Window.Current, Gesture.Back, Forward)
	   │  → erstellt neuen FrameFacade(MainFrame, NavService, id)
	   │  → registriert NavService in NavigationService.Instances[MainFrame]
	   └─ OnNavigatedToFrame() → RegisterViewModelForView()
```

### 4.3 Registrierter Seiten-Navigation (`PageNavigationRegistry`)

| Schritt | Methode / Hook | Beschreibung |
|---------|---------------|-------------|
| 1 | `container.RegisterForNavigation<MainPage, MainPageViewModel>()` | Extension-Methode auf IContainerRegistry, callst PageNavigationRegistry.Register() |
| 2 | Navigation → `PageTypeMapper.GetPageType(name)` prüft PageNavigationRegistry | Look up by name or key |
| 3 | Erstelle Seite via Reflection oder Resolve (über ViewModelLocationProvider) | View wird instanziiert + ViewModel attached |

---

## 5. MVVM-Basis (`UNOversalTemplate.Core/Mvvm/`)

| Datei | Typ | Verantwortung |
|-------|-----|---------------|
| `ViewModelBase.cs` | Class | Basis-Klasse für ViewModels, INotifyPropertyChanged etc. |
| `ViewModelLocationProvider.cs` | Static | ViewModel-Resolver und Locator-Pattern |
| `ViewModelLocator.cs` | Class | Hilfsklasse für XAML-basierter VM-Locator |
| `ErrorsContainer.cs` | Class | Error-Handling-Container |
| `PropertySupport.cs` | Static | Eigenschaftsreflektion-Helfer |

---

## 6. Sample-Projekt: UWPNetSample_MSIoC

### 6.1 Project References / Abhängigkeiten

```
UWPNetSample_MSIoC.csproj
 ├── UNOversalTemplate.Core (shproj)
 ├── UNOversalTemplate.IoC.MS (shproj)        ← IoC-Backend: MS.DI
 └── Microsoft.Extensions.DependencyInjection  ← NuGet-Paket
```

### 6.2 App.xaml.cs

```csharp
class App : UNOversalApplication    // = UNOversalTemplate.IoC.MS.UNOversalApplication

protected override UIElement CreateShell()
{ ShellPageInstance = new ShellPage(); return ShellPageInstance; }

public override void RegisterTypes(IContainerRegistry container)
{   container.RegisterForNavigation<MainPage, MainPageViewModel>();  }

public override async Task OnStartAsync(IApplicationArgs args)
{
	Window.Current.Content = ShellPageInstance;
	await ShellPageInstance.NavigationService.NavigateAsync(nameof(MainPage));
	Window.Current.Activate();
}
```

### 6.3 ShellPage.xaml.cs

```csharp
class ShellPage : Page
{   public INavigationService NavigationService { get; internal set; }

	ShellPage()
	{
		InitializeComponent();
		GestureService.SetupWindowListeners(Window.Current.CoreWindow);
		NavigationService = NavigationFactory.Create(MainFrame)
				.AttachGestures(Gesture.Back, Gesture.Forward);
	}
}
```

### 6.4 MainPageViewModel.cs

```csharp
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
class MainPageViewModel   // Markiert mit [RegisterForNavigation] via SourceGen für AOT/Trimming-Kompatibilität
{
	public MainPageViewModel() { }    // ctor per DI resolvierbar
}
```

---

## 7. Vergleich: DryIoc vs MS.DI Implementation

| Merkmal | UNOversalTemplate.IoC.DryYoC | UNOversalTemplate.IoC.MS |
|---------|-----------------|-----------|
| Package | DryIoc | Microsoft.Extensions.DependencyInjection |
| Container-Typ | `DryIoc.IContainer` | `IServiceProvider` (+ internal `ServiceCollection`) |
| User-Hook | `protected virtual Rules CreateContainerRules()` | `protected virtual void ConfigureServiceCollection(ServiceCollection)` |
| Default-Rules | `DefaultRules` mit `ConstructorWithResolvableArguments`, `TrackingDisposableTransients` etc. | Standard MS-DI Verhalten |
| Scoped-Unterstützung | `DryIocScopedProvider : IScopedProvider` | Über `IServiceScopeFactory` (built-in) |
| Resolve-Pattern | `(Type, object)[] parameters` tuple pattern | Native `IServiceCollection.AddXxx()` / `AddTransient()` etc. |
| FinalizeExtension() | `{ }` (nicht nötig) | `services.BuildServiceProvider()` |

---

## 8. Abhängigkeitsmatrix

```
						   DryIoc-        MS.DI-          UnoSample       UWPNetSample
						   Sample         Sample          DryIoC          MSIoC
						  ──────────      ──────────      ────────        ──────────
  UNOversalTemplate.Core  ████████        ████████        ████████        ████████
  UNOversalTemplate.IoC   ████████        ████████       (verwendet)     (verwendet)
   .DryIoC               ████████                              ████████
  UNOversalTemplate.IoC              ████████                                ████████
   .MS                                       ████████
  Services                ████████                          ████████        ████████
```

---

## 9. Wichtige Konventionen & Patterns

1. **IoC-Agnostik**: User-code implementiert *nie* ein konkretes Container-Interface, sondern nur `IContainerRegistry`/`IContainerProvider`. Die Wahl des Backends ist eine Konfigurationsentscheidung (shproj).

2. **Navigation via String-Namen**: `container.RegisterForNavigation<TPage, TViewModel>()` registriert View+VM-Typ-Paare in der statischen `PageNavigationRegistry`. Navigation erfolgt dann per String-Name (`"MainPage"`), nicht per Type.

3. **Frame-spezifische Navigation**: Jeder XAML Frame erhält via `NavigationFactory.Create(Frame)` einen eigenen `INavigationService`. Die statische `Instances`-Map ermöglicht Lookup über `NavigationService.Instances[frame]`.

4. **ViewModel-Autowiring**: Über `ViewModelLocationProvider.SetDefaultViewModelFactory()` wird jeder View automatisch mit seinem registrierten ViewModel versorgt. Views müssen im Konstruktor keine VM instanziieren.

5. **SourceGenerator-Integration für AOT**: `[RegisterForNavigation]` Attribut + SourceGen generiert Reflection-freie Registrations für .NET NativeAOT/Trimming-Kompatibilität.

6. **Platform-Switching via Preprozessor-Direktiven**: `#if WINDOWS_UWP / #else` für WinUI3 vs UWP Frame-Typen (`Windows.UI.Xaml.Controls.Frame` vs `Microsoft.UI.Xaml.Controls.Frame`).

---

## 10. AOT / Trimming — Problemanalyse (NET 10)

### 10.1 Kritisches Problemfeld: `MSIocContainerExtension` (`UNOversalTemplate.IoC.MS`)

#### CRIT-01: `Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)` — Kein `[DynamicallyAccessedMembers]`
**File:** `MSIocContainerExtension.cs`, lines 299, 372, ~400  
**Problem:** Der Trimmer kann nicht statisch nachvollziehen, welche Typen reflexiv aufgelöst werden müssen. GetConstructors mit BindingFlags ist ein klassischer "dynamical-access" — der Trimmer entfernt alle Constructor-Zeilen metadata.

```csharp
// Zeile 299: Resolve(Type type)
var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var bestCtor = ctors.OrderByDescending(...).FirstOrDefault();
...
return bestCtor.Invoke(resolvedArgs);   // ← Invoke benötigt ConstructorInfo!
```

**Schweregrad: CRITICAL** — `Resolve<T>()` wird zur Laufzeit fehlschlagen, wenn der Konstruktor getrimmt wurde.  
**Fix:** `type`-Parameter mit `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]` annotieren (bereits auf Zeile 279 vorhanden, ABER die Methode `TryResolveWithCtorDeps` hat es auch — prüfe).

---

#### CRIT-02: `Activator.CreateInstance(type)` ohne DAm-Metadaten
**File:** `MSIocContainerExtension.cs`, lines 358, ~410  
**Problem:** Aufruf von Activator.CreateInstance mit einem dynamischen Type-Argument hat *keine* `[DynamicallyAccessedMembers]` auf dem Parameter. Der Trimmer entfernt die public parameterless constructor.

```csharp
// Zeile 358: Last-resort fallback
return Activator.CreateInstance(type)!;  
// Ohne DAm auf 'type': der Konstruktor kann getrimmt werden!
```

**Schweregrad: CRITICAL** — Wird triggered, wenn `GetService(type)` null zurückgibt und ctor-deps nicht auflösbar sind.  
**Fix:** Parameter `try` mit DAm annotieren oder `CreateInstance<T>` overloads mit known-Typen bevorzugen.

---

#### CRIT-03: `Register(Type, Type)` Overloads — Keine `[DynamicallyAccessedMembers]`
**File:** `MSIocContainerExtension.cs`, lines 192–254  
**Problem:** Alle `Register(Type from, Type to)`, `RegisterSingleton(Type from, Type to)`, `RegisterScoped(Type from, Type to)` — die `to`-Parameter haben keine DAm-Annotation. Der Trimmer weiß nicht, dass der Konstruktor von `to` erhalten bleiben muss.  
Im Gegensatz dazu haben `RegisterSingleton(Type from, [DAm(...)]Type to)` (line 85) und die benannte Variant (line 98) Annotations — **inkonsistent**.

**Schweregrad: HIGH** — Alle Registrierungen via Type-Only API sind betroffen.

---

#### CRIT-04: `RegisterMany / RegisterManySingleton` — `type.GetInterfaces()`
**File:** `MSIocContainerExtension.cs`, lines 139, 246  
**Problem:** In `RegisterMany(Type type, params Type[] serviceTypes)` wird `type.GetInterfaces()` verwendet. Wenn der Trimmer die Interface-Typen selbst entfernt (weil sie nur reflexiv referenziert werden), bricht die Registrierung.

```csharp
if (serviceTypes.Length == 0) {
    serviceTypes = type.GetInterfaces();  // ← GetInterfaces ohne DAm!
}
foreach (var serviceType in serviceTypes)
    _serviceCollection.AddTransient(serviceType, type);
```

**Schweregrad: HIGH** — Wird bei Interface-Registrierungen aktiv.  
**Fix:** `serviceTypes` als optional mit `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]` annotieren.

---

### 10.2 Kritisches Problemfeld: `ViewModelLocationProvider` (`UNOversalTemplate.Core/Mvvm/`)

#### CRIT-05: `_defaultViewTypeToViewModelTypeResolver` — `Type.GetType(assemblyQualifiedName)`
**File:** `ViewModelLocationProvider.cs`, lines 50–59  
**Problem:** Convention-basierte VM-Auflösung verwendet `Type.GetType(fullName)`. Assembly-qualified Names können nach dem Trimming **nicht** aufgelöst werden, da die Typen im Image fehlen.

```csharp
var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
var viewModelName = String.Format(... "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
return Type.GetType(viewModelName);  // ← Fällt still auf null!
```

**Schweregrad: HIGH** — Stille Failure. VM ist null → App crasht beim DataContext Binding.  
**Workaround-bereits vorhanden:** Code-Kommentar (line 43–45) warnt explizit. User muss explizite Registrierung über `Register<T,VM>()` verwenden.

---

#### CRIT-06: `CreateViewModelInstance` — Activator ohne DAm-Annotation auf *allen* Aufrufpfaden?
**File:** `ViewModelLocationProvider.cs`, lines 64–66  
**Problem:** 

```csharp
private static object CreateViewModelInstance(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    => Activator.CreateInstance(type)!;
```

Die Signatur **hat** DAm! Aber der Aufrufer `AutoWireViewModelChanged` (line 101+) übergibt einen nicht-annotierten Typ → der Trimmer verwirft die Annotation-Wirkungskette. Der `_defaultViewModelFactory` und `_defaultViewModelFactoryWithViewParameter` Delegates haben ebenfalls keine Parameter-Annotationen.

**Schweregrad: HIGH** — VM-Konstruktor kann removed werden.  
**Fix:** Delegate-Typen mit DAm annotieren oder `Func<Type, object>` durch generische Variante ersetzen.

---

### 10.3 Problemfeld: `ModuleInfo` & `ModuleInitializer` (`UNOversalTemplate.Core/Modularity/`)

#### CRIT-07: Mehrfacher `Type.GetType(string)` ohne Annotation — unmarkierte Reflektion
**File:** `ModuleInfo.cs`, lines 42, 104; `ModuleInitializer.cs`, line 19  

```csharp
// ModuleInfo.cs line 42
ModuleType = Type.GetType(type) ?? throw new ...

// ModuleInfo.cs line 104 (property setter)
set => ModuleType = Type.GetType(value);

// ModuleInitializer.cs line 19
var module = CreateModule(Type.GetType(moduleInfo.ModuleType, true));
```

**Schweregrad: CRITICAL** — Modul-Discovery wird in AOT-Builds **vollständig brechen**.  
**Hinweis:** Modularity ist aktuell **kommentiert/ausgeschaltet** (siehe UNOversalApplicationBase.cs lines 80, 99–106). Trotzdem: Wenn jemals aktiviert, muss dies refaktorisiert werden.

---

### 10.4 Problemfeld: `BindingUtilities` — Reflection auf XAML-Internals

#### MID-08: `.GetTypeInfo().GetDeclaredField("Bindings")` + `GetRuntimeMethod("Update/...")`
**File:** `BindingUtilities.cs`, entire file  
**Problem:** Reflexivzugriff auf private XAML-Fields ("Bindings"). Mit `# pragma warning disable IL2075 / IL2072` unterdrückt — **aber der Trimmer entfernt das Field trotzdem**. Die Warnungen sind Suppressions, keine Garantien.

```csharp
var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
var bindings = field?.GetValue(page);
var update = bindings?.GetType().GetRuntimeMethod("Update", Array.Empty<Type>());
update?.Invoke(bindings, null);  // ← Field und Method werden evtl. entfernt!
```

**Schweregrad: MEDIUM** — Funktioniert ggf. zur Laufzeit (XAML Internals bleiben oft erhalten über WinRT/COM-Kanäle), aber keine garantierte AOT-Sicherheit.  
**Empfehlung:** Durch `BindingExpression`-APIs oder XAML-Compiler-Tricks ersetzen falls nötig.

---

### 10.5 Problemfeld: `EventAggregator` — `new()` constraint ohne DAm

#### MID-09: `[Databound]` Generic-Methode mit `new()` constraint
**File:** `EventAggregator.cs`, line 27  

```csharp
public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
{
    TEventType newEvent = new TEventType();  // ← ctor muss erhalten bleiben!
}
```

**Schweregrad: MEDIUM** — In .NET 10 mit NativeAOT/Trimming kann der `new()` constraint nicht durchgesetzt werden ohne `[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(TEventType))]` am Method-Level.  
**Fix:** Annotation auf die Methode hinzufügen oder auf `new()` verzichten.

---

### 10.6 Problemfeld: `ParametersExtensions` — `Activator.CreateInstance` ohne DAm

#### MID-10: `GetDefault(Type type)` → `Activator.CreateInstance(type)`
**File:** `Common/ParametersExtensions.cs`  
**Problem:** Der Parameter `type` hat keine `[DynamicallyAccessedMembers]`, obwohl die Methode explizit den public parameterless ctor benötigt:

```csharp
private static object GetDefault(
    [DynamicallyAccessedMember(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
{
    return Activator.CreateInstance(type);  // ✓ hat DAm auf Parameter!
}
```

**Korrektur:** Diese Stelle **hat bereits die Annotation**. Nur die Generic-Varianten (GetValue, GetValues mit `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]`) haben Interfaces-Annotation — das ist inkonsistent (Interfaces ≠ Constructors).

**Schweregrad: LOW** — Falls `ParametersExtensions.cs` korrekt annotiert ist.  
**Empfehlung:** Generic-Varianten auf `PublicConstructors` oder `PublicParameterlessConstructor` setzen, nicht nur `Interfaces`.

---

### 10.7 Problemfeld: `ContainerResolutionException` — `GetConstructors()` ohne DAm

**File:** `ContainerResolutionException.cs`  
```csharp
var ctors = implementingType.GetConstructors();
// Ohne DAm auf implementingType — falls implementingType von einem dynamischen Type kommt!
```

**Schweregrad: LOW** — Tritt nur in Exception-Handling auf, aber potentiell fehleranfällig.  
**Fix:** Parameter mit `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]` annotieren.

---

### 10.8 Problemfeld: `PropertySupport` & Expression-Bäume

**File:** `PropertySupport.cs`  
```csharp
var getMethod = property.GetMethod;
if (getMethod.IsStatic) throw ...
```

**Schweregrad: LOW** — Expression-based Reflektion ist trimm-sicherer, da der Compiler bereits metadata generiert. Aber: Wenn die Property selbst entfernt wird, crasht es zur Laufzeit. Nichts zu tun falls Properties explizit referenziert werden (eher unwahrscheinlich).

---

## 10.9 Zusammenfassung — Priorisierte Issue-Tabelle

| ID | Datei | Problem | severity | Fix-Typ |
|----|-------|---------|----------|---------|
| ~~CRIT-01~~ | `MSIocContainerExtension.cs:299,372` | ~~`GetConstructors(BindingFlags)` ohne DAm~~ | **CRITICAL** | ✅ Alle Resolve()-Overloads mit DAm annotiert (PublicConstructors) |
| CRIT-02 | `MSIocContainerExtension.cs:358` | `Activator.CreateInstance(type)` ohne DAm | **CRITICAL** | DAm auf try-Parameter |
| HIGH-01 | `ViewModelLocationProvider.cs:50–59` | Convention-basierte VM-Auflösung via `Type.GetType(aqn)` | **HIGH** | User-Musste explizit registrieren (bereits dokumentiert) |
| CRIT-03 | `MSIocContainerExtension.cs:192–254` | `Register(Type, Type)` Overloads ohne DAm | **CRITICAL** | Annotations hinzufügen |
| ~~HIGH-02~~ | `MSIocContainerExtension.cs:139,246` | ~~`type.GetInterfaces()` ohne DAm~~ | **HIGH** | ✅ Parameter mit DAm annotieren (NET10_0_OR_GREATER) |
| CRIT-04 | `ViewModelLocationProvider.cs:64–66` | Delegate-Parameter-Kette bricht DAm-Kette ab | **HIGH** | Delegate-Typ-DAm hinzufügen |
| CRIT-05 | `ModuleInfo.cs` + `ModuleInitializer.cs` | Unmarkierte `Type.GetType()` Aufrufe | **CRITICAL** | Nur relevant wenn Modularity aktiviert wird |
| MED-01 | `BindingUtilities.cs` | Reflection auf XAML Internals ohne Garantie | **MEDIUM** | Suppressions vorhanden aber keine Garantie |
| MED-02 | `EventAggregator.cs:27` | `new()` constraint ohne DAm am Method-Level | **MEDIUM** | DynamicDependency hinzufügen |
| LOW-01 | `ParametersExtensions.cs` | Generic-Varianten mit `Interfaces` statt `Constructors` DAm | **LOW** | Annotation prüfen/ankorrigieren |
| LOW-02 | `ContainerResolutionException.cs` | `GetConstructors()` auf nicht-annotiertem Typ | **LOW** | Parameter annotieren |

### 10.10 AOT-Sicherheitsmatrix nach Projekt

| Projekt | Crit-Issues | High-Issues | Gesamt-Risiko |
|---------|-------------|-------------|---------------|
| `UNOversalTemplate.IoC.MS` | ~~CRIT-01~~, CRIT-02, CRIT-03 | ~~HIGH-02~~, HIGH-01 | 🔴 **HOCH** — Kernproblem: IoC-Reflection ohne DAm-Kette |
| `UNOversalTemplate.IoC.DryIoC` | 0 | LOW-01 (GetConstructors in Scope) | 🟡 **MITTEL** — DryIoc nutzt eigene Rules, aber Resolve-Pattern ähnlich |
| `UNOversalTemplate.Core/Mvvm` | CRIT-04 | HIGH-01 | 🔴 **HOCH** — ViewModelLocationProvider DAm-Kette gebrochen |
| `UNOversalTemplate.Core/Navigation` | 0 | 0 | 🟢 **NIEDRIG** — Kein Reflektions-basiertes Typ-Auflösen |
| `UNOversalTemplate.Core/Modularity` | CRIT-05 (latent) | 0 | 🔴 **HOCH** wenn aktiv |
| `UNOversalTemplate.Services/` | 0 | 0 | 🟢 **NIEDRIG** — Keine kritischen Reflektions-Pfade |
| `UWPNetSample_MSIoC` | 0 | 0 | 🟢 **NIEDRIG** — Alle Typen compile-time bekannt, RegisterForNavigation Generator |

---

*Ende der AOT/Trimming-Analyse. Diese Issues müssen vor einem Release mit `<PublishAot>true</PublishAot>` oder `<TrimmerRemoveSymbols>true</TrimmerRemoveSymbols>` gelöst werden.*

