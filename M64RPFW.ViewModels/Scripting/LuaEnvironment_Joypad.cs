using System.Diagnostics;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Scripting.Extensions;
using NLua;

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
        //buttons.BtnMask |= (true ? Mupen64PlusTypes.ButtonMask.DRight : 0);
        
        Mupen64Plus.VCR_SetOverlay(buttons, port);
    }
}