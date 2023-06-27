﻿using System;
using System.Numerics;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia.Services;

// NOTE: This reaches its tentacles into view state, since it's a monolithic multi-responsibility service
// Don't attempt to clean up until an acceptable level of API compatibility and maturity is reached
public class FrontendScriptingService : IFrontendScriptingService
{
    private readonly LuaWindow _luaWindow;
    private LuaViewModel LuaViewModel => LuaWindow.LuaViewModels[_luaWindow].ViewModel;

    public FrontendScriptingService(LuaWindow luaWindow)
    {
        _luaWindow = luaWindow;
    }

    public void Print(string value)
    {
        _luaWindow.Print(value);
    }

    public (int Width, int Height) GetWindowSize()
    {
        return ((int Width, int Height))(WindowHelper.MainWindow.Width, WindowHelper.MainWindow.Height);
    }

    public void SetWindowSize(int width, int height)
    {
        WindowHelper.MainWindow.Width = width;
        WindowHelper.MainWindow.Height = height;
    }
}