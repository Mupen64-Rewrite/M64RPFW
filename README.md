# M64RPFW (WIP)

![M64RPFW Logo](MiscAssets/rpfw.svg)  
A new and improved TASing emulator powered by [Mupen64Plus](https://github.com/mupen64plus), with lots of extra bells 
and whistles.

## Related projects

- [mupen64plus-core-rr](https://github.com/Mupen64-Rewrite/mupen64plus-core-rr) - our fork of mupen64plus-core with handy TASing features
- [TinCan.NET](https://github.com/Mupen64-Rewrite/TinCan.NET) - an input plugin specifically designed for TASing

## Building from source

As with all things, this might change if we improve the build process.

### Dependencies
- .NET 7 or higher
- (if using Visual Studio) Visual Studio 2022

Any remaining dependencies _should_ be handled by NuGet. If it doesn't, please report an issue.

### Rider/Visual Studio 2022

1. Clone the source code from Git, then open `M64RPFW.sln`.
2. Simply build and run `M64RPFW.Views.Avalonia`.
   1. **VS users:** If this doesn't work, ensure that `M64RPFW.Views.Avalonia` is set as the startup project.

### .NET CLI

1. Clone the source code from Git.
2. Run the following commands:
    ```bash
    dotnet restore
    cd M64RPFW.Views.Avalonia && dotnet run
    ```
