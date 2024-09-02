# MAUIApp 
This project implements a robust MAUI Hybrid application that expects the Blazor UI to be in a BlazorUI razor library.

This project is a companion to the WASMApp project. There are no dendencies among the WASMApp and MAUIApp projects. The WASMApp project is a Blazor WebAssembly project that is used to host the Blazor UI. The MAUIApp project is a .NET MAUI project that is used to host the Blazor UI.

The objective of the MAUIApp and WASMApp projects is to isolate target specific code such that applications can use these two projects without substantial modifications.

## MAUI has fallen and it can't get up
MAUI is still a little rough around the edges. There are times when compiliation starts failing without any code changes in the project. This generally happens when you change some of the packages referenced by projects referenced by the MAUIApp. 

The compiler starts complaining about missing resources in the android manifest.

When this happens, the following steps can be taken to resolve the issue:

1. In the MAUIApp project, in powershell, run the following command:
```dotnet workload restore```
2. Close and repoen the solution.
3. Clean the projecct.
4. Unload the project. 
5. Reload the project with all dependencies.
6. Clean the project. 
7. Close and reopen the solution.
8. Rebuild the project.

Most of the time, this will resolve the issue.


