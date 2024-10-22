﻿using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia.Services;

public class LuaWindowService : ILuaWindowService
{
    private readonly LuaWindow _luaWindow;
    private LuaViewModel LuaViewModel => _luaWindow.ViewModel;

    public LuaWindowService(LuaWindow luaWindow)
    {
        _luaWindow = luaWindow;
    }

    public void Print(string value)
    {
        _luaWindow.Print(value);
    }
}