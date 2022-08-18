using System;
using Eto.Forms;

namespace M64RPFW.Presenters.Helpers;

public static class ViewInitHelpers
{
    public static Action<Window, EventHandler<KeyEventArgs>, EventHandler<KeyEventArgs>>? RegisterWindowKeyHandlersImpl { private get; set; }

    public static void RegisterWindowKeyHandlers(Window window, EventHandler<KeyEventArgs> down, EventHandler<KeyEventArgs> up)
    {
        if (RegisterWindowKeyHandlersImpl != null)
            RegisterWindowKeyHandlersImpl(window, down, up);
        else
        {
            window.KeyDown += down;
            window.KeyUp += up;
        }
    }
    
    public static Action<Window>? RemoveWindowKeyHandlersImpl { private get; set; }

    public static void RemoveWindowKeyHandlers(Window window, EventHandler<KeyEventArgs> down, EventHandler<KeyEventArgs> up)
    {
        if (RemoveWindowKeyHandlersImpl != null)
            RemoveWindowKeyHandlersImpl(window);
        else
        {
            window.KeyDown -= down;
            window.KeyUp -= up;
        }
    }
}