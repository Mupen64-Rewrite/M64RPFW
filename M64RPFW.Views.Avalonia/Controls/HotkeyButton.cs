using System;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace M64RPFW.Views.Avalonia.Controls;

public class HotkeyButton : Button, IStyleable
{
    public Type StyleKey => typeof(Button);

    protected override void OnClick()
    {
        base.OnClick();
        var root = this.GetVisualRoot();
        if (root is null) return;
        
    }
}