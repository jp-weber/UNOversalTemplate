## About UNOversalTemplate

UNOversalTemplate is a template for the [Uno platform](https://github.com/unoplatform/uno) based on [PrismLibrary/Prism](https://github.com/PrismLibrary/Prism) v8.1.97 and a fork of [Template10](https://github.com/2fast-team/Template10?organization=2fast-team&organization=2fast-team). Both the UWP and WinUI templates are supported, so that an existing UWP app can use the UNOversalTemplate template and on other hand the mobile apps can use the newer WinUI template in UNOversalTemplate. The current goal is to keep the functionality as close to UWP as possible. This also goes hand in hand with the navigation service from Template10, where the service is built on the basis of the normal navigation.

### Features
- latest Uno Platform version is supported
- DryIoC is supported for IoC
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) is used for MVVM

### Features WIP
- Region support from Prism

## Getting Started

The following steps will help if you want to contribute or work on the application.

### Prerequirements

- [Visual Studio 2022](https://visualstudio.microsoft.com/)
	- Don't forget to select the *Mobile development with .NET* package in the installation process 
	- .NET Multi-Platform App UI development
- Windows 10, version >= `1809`, October update 2018 (for the universal Windows application)
	- The latest Windows 10 SDK is required
- Android, version >= 7.0 (for the Android application)
	- Remember to trust 3rd party Apps by enabling this in the Android developer settings if you want to build from source

### Sample apps
Currently two sample apps (UWP and Uno app with iOS/Android/macOS/MacCatalyst).
