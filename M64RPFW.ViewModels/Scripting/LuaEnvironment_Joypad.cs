using System.Diagnostics;
using M64RPFW.Models.Types;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("joypad.get")]
    private LuaTable GetJoypad(int port)
    {
        var table = _lua.NewUnnamedTable();
        
        // TODO: GetKeys is not implemented fully in core
        var buttons = new Mupen64PlusTypes.Buttons();

        table["right"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DRight);
        table["left"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DLeft);
        table["down"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DDown);
        table["up"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DUp);
        table["start"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Start);
        table["Z"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Z);
        table["B"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.B);
        table["A"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.A);
        table["Cright"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CRight);
        table["Cleft"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CLeft);
        table["Cdown"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CDown);
        table["Cup"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CUp);
        table["R"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.R);
        table["L"] = buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.L);
        table["X"] = buttons.JoyX;
        table["Y"] = buttons.JoyY;
        
        return table;
    }

    [LibFunction("joypad.set")]
    private void SetJoypad(int port, LuaTable table)
    {
        // TODO: SetKeys is not implemented fully in core
    }
}