﻿using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Views;
using SkiaSharp;

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

    public IWindowSizingService WindowSizingService => WindowHelper.MainWindow;
    public event Action<SKCanvas>? OnUpdateScreen;

    public (double X, double Y) PointerPosition => (WindowHelper.MainWindow.PointerPosition.X,
        WindowHelper.MainWindow.PointerPosition.Y);

    public bool IsPrimaryPointerButtonHeld => WindowHelper.MainWindow.IsPrimaryPointerButtonHeld;
    public ICollection<string> HeldKeys => new List<string>(); // TODO: implement

    public void Print(string value)
    {
        _luaWindow.Print(value);
    }

    public void InvokeOnUpdateScreen(SKCanvas canvas)
    {
        OnUpdateScreen?.Invoke(canvas);
    }
}