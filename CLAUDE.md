# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

StoreApp is a cross-platform Blazor application built with the LazyMagic framework implementing MVVM architecture. It supports both Web (WASM) and Mobile (MAUI) deployments from a shared codebase.

### Project Structure
- **WASMApp**: Blazor WebAssembly deployment (SPA/PWA)
- **MAUIApp**: .NET MAUI hybrid app (iOS, Android, Windows, MacOS) using WebView
- **BlazorUI**: Shared Razor components and pages (Views in MVVM)
- **ViewModels**: Shared business logic and state management
- **MakeRelease**: Build and release automation project

### Key Dependencies
- **LazyMagic Framework**: Core framework providing MVVM, authentication, and client SDK
- **ReactiveUI**: Reactive programming for data flow and change propagation
- **MudBlazor**: Material Design component library
- **BaseApp Libraries**: Shared application components from BaseAppLib package
- **StoreApi**: Generated client SDK for Store API communication

## Common Development Commands

### Building the Application
```bash
# Build entire solution
dotnet build StoreApp.sln

# Build specific projects
dotnet build WASMApp/WASMApp.csproj
dotnet build MAUIApp/MAUIApp.csproj
dotnet build BlazorUI/BlazorUI.csproj
dotnet build ViewModels/ViewModels.csproj
```

### Running the Application
```bash
# Run WASM app locally (default: https://localhost:7218)
cd WASMApp
dotnet run

# Run with local backend environment
cd WASMApp
dotnet run --launch-profile "https Local"

# Run with cloud backend
cd WASMApp
dotnet run --launch-profile "https Cloud"
```

### Building MAUI App for Specific Platforms
```bash
cd MAUIApp

# Windows
dotnet build -f net9.0-windows10.0.19041.0

# Android
dotnet build -f net9.0-android

# iOS (requires Mac)
dotnet build -f net9.0-ios

# Mac Catalyst
dotnet build -f net9.0-maccatalyst
```

### Clean Build
```powershell
# Remove all obj and bin directories
.\DeleteObjAndBin.ps1 -RootPath . -Confirm:$false
```

## Code Generation and Analyzers

The project uses extensive code generation through .NET Analyzers:

1. **LazyMagic.LzItemViewModelGenerator**: Generates Model classes from DTOs
   - Creates partial classes that can be extended
   - Generates validation classes for FluentValidation

2. **LazyMagic.DIHelper**: Auto-registers DI services
   - Classes implementing ILzTransient, ILzSingleton, or ILzScoped

3. **LazyMagic.FactoryGenerator**: Generates factory classes
   - For classes with [Factory] attribute
   - Primarily used with ViewModels

Generated files use `.g.cs` extension and should not be manually edited.

## MVVM Architecture Implementation

### ViewModels
- Inherit from `LzItemViewModelBase<T>` (single item) or `LzItemsViewModelBase<T>` (collections)
- Located in `/ViewModels/` directory
- Use ReactiveUI for reactive programming
- Provide CRUDL operations through StoreClientSDK

### Views (Blazor Components)
- Located in `/BlazorUI/` directory
- Inherit from `LzCoreComponentBase<T>`
- Shared between WASM and MAUI deployments
- Use MudBlazor components for UI

### Models
- Generated from DTOs in StoreApi package
- Extended through partial classes in ViewModels project
- Validation via FluentValidation in generated validation classes

## Package Management

Uses Central Package Version Management (CPVM):
- Package versions defined in `Directory.Packages.props`
- Projects reference packages without Version attribute
- LazyMagic packages: Version 3.0.1
- BaseApp packages: Version 1.0.0
- StoreApi package: Version 1.0.0

## Local Development Configuration

### Launch Profiles
WASMApp supports two profiles in `launchSettings.json`:
- **https Local**: Connects to local backend (ASPNETCORE_ENVIRONMENT=Localhost)
- **https Cloud**: Connects to cloud backend

### Authentication
StoreApp uses TenantAuth (Cognito User Pool) for store employees/admins authentication through the StoreApi.

## Important Patterns

1. **Reactive Data Flow**: All data operations use ReactiveUI observables
2. **Code-First Generation**: DTOs and API clients generated from OpenAPI specs
3. **Shared UI Strategy**: Single UI codebase for Web and Mobile
4. **Dependency Injection**: Automatic service registration via analyzers
5. **Partial Class Extension**: Generated code extended through partial classes