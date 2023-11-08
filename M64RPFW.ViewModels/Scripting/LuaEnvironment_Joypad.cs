using System.Diagnostics;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;


public partial class LuaEnvironment
{
    [LibFunction("joypad.get")]
    private LuaTable GetJoypad(uint port)
    {
        var table = _lua.NewUnnamedTable();
        
        var buttons = Mupen64Plus.VCR_GetKeys(port);

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
    private void SetJoypad(uint port, LuaTable table)
    {
        var buttons = new Mupen64PlusTypes.Buttons();
    
        // TODO: implement
        // idk how to check if key is present in table, since its a pseudo-dictionary
        buttons.BtnMask |= table["right"] != null ? Mupen64PlusTypes.ButtonMask.DRight : 0;
        buttons.BtnMask |= table["left"] != null ? Mupen64PlusTypes.ButtonMask.DLeft : 0;
        buttons.BtnMask |= table["up"] != null ? Mupen64PlusTypes.ButtonMask.DUp : 0;
        buttons.BtnMask |= table["down"] != null ? Mupen64PlusTypes.ButtonMask.DDown : 0;
        buttons.BtnMask |= table["start"] != null ? Mupen64PlusTypes.ButtonMask.Start : 0;
        buttons.BtnMask |= table["Z"] != null ? Mupen64PlusTypes.ButtonMask.Z : 0;
        buttons.BtnMask |= table["B"] != null ? Mupen64PlusTypes.ButtonMask.B : 0;
        buttons.BtnMask |= table["A"] != null ? Mupen64PlusTypes.ButtonMask.A : 0;
        buttons.BtnMask |= table["Cright"] != null ? Mupen64PlusTypes.ButtonMask.CRight : 0;
        buttons.BtnMask |= table["Cleft"] != null ? Mupen64PlusTypes.ButtonMask.CLeft : 0;
        buttons.BtnMask |= table["Cup"] != null ? Mupen64PlusTypes.ButtonMask.CUp : 0;
        buttons.BtnMask |= table["Cdown"] != null ? Mupen64PlusTypes.ButtonMask.CDown : 0;
        buttons.BtnMask |= table["R"] != null ? Mupen64PlusTypes.ButtonMask.R : 0;
        buttons.BtnMask |= table["L"] != null ? Mupen64PlusTypes.ButtonMask.L : 0;
        buttons.JoyX = table["X"] switch
        {
            double n => (sbyte) (int) n,
            int n => (sbyte) n,
            _ => 0
        };
        buttons.JoyY = table["Y"] switch
        {
            double n => (sbyte) (int) n,
            int n => (sbyte) n,
            _ => 0
        };
        
        Mupen64Plus.VCR_SetOverlay(buttons, port);
    }
}