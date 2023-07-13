using System;
using Avalonia.Controls;

namespace M64RPFW.Views.Avalonia.Controls;

/// <summary>
/// Part of a kludge for setting the savestate slots. Opens up the
/// OnClick handler so that it may be manually called.
/// </summary>
public class ClickableRadioButton : RadioButton
{
    public new Type StyleKeyOverride => typeof(RadioButton);
    
    /// <summary>
    /// Manually clicks the RadioButton.
    /// </summary>
    public void ManualClick()
    {
        OnClick();
    }
}