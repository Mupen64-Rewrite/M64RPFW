

using System;
using Eto.Forms;
using static M64RPFW.Interfaces.LibSDL;

namespace M64RPFW.Presenters.Helpers;

/// <summary>
/// Converts between SDL keys and Eto keys.
/// </summary>
public class SDLKeyConverter
{
    private static SDL_Keymod GetKeyMod(Keys key)
    {
        return key switch
        {
            Keys.LeftControl => SDL_Keymod.KMOD_LCTRL,
            Keys.RightControl => SDL_Keymod.KMOD_RCTRL,
            Keys.LeftAlt => SDL_Keymod.KMOD_LALT,
            Keys.RightAlt => SDL_Keymod.KMOD_RALT,
            Keys.LeftShift => SDL_Keymod.KMOD_LSHIFT,
            Keys.RightShift => SDL_Keymod.KMOD_RSHIFT,
            Keys.LeftApplication => SDL_Keymod.KMOD_LGUI,
            Keys.RightApplication => SDL_Keymod.KMOD_RGUI,
            Keys.NumberLock => SDL_Keymod.KMOD_NUM,
            Keys.CapsLock => SDL_Keymod.KMOD_CAPS,
            Keys.ScrollLock => SDL_Keymod.KMOD_SCROLL,
            _ => SDL_Keymod.KMOD_NONE
        };
    }

    private static SDL_Scancode GetScanCode(Keys key)
    {
        return key switch
        {
            // number keys
            Keys.D0 => SDL_Scancode.SDL_SCANCODE_0,
            Keys.D1 => SDL_Scancode.SDL_SCANCODE_1,
            Keys.D2 => SDL_Scancode.SDL_SCANCODE_2,
            Keys.D3 => SDL_Scancode.SDL_SCANCODE_3,
            Keys.D4 => SDL_Scancode.SDL_SCANCODE_4,
            Keys.D5 => SDL_Scancode.SDL_SCANCODE_5,
            Keys.D6 => SDL_Scancode.SDL_SCANCODE_6,
            Keys.D7 => SDL_Scancode.SDL_SCANCODE_7,
            Keys.D8 => SDL_Scancode.SDL_SCANCODE_8,
            Keys.D9 => SDL_Scancode.SDL_SCANCODE_9,
            // alphabetic keys
            Keys.A => SDL_Scancode.SDL_SCANCODE_A,
            Keys.B => SDL_Scancode.SDL_SCANCODE_B,
            Keys.C => SDL_Scancode.SDL_SCANCODE_C,
            Keys.D => SDL_Scancode.SDL_SCANCODE_D,
            Keys.E => SDL_Scancode.SDL_SCANCODE_E,
            Keys.F => SDL_Scancode.SDL_SCANCODE_F,
            Keys.G => SDL_Scancode.SDL_SCANCODE_G,
            Keys.H => SDL_Scancode.SDL_SCANCODE_H,
            Keys.I => SDL_Scancode.SDL_SCANCODE_I,
            Keys.J => SDL_Scancode.SDL_SCANCODE_J,
            Keys.K => SDL_Scancode.SDL_SCANCODE_K,
            Keys.L => SDL_Scancode.SDL_SCANCODE_L,
            Keys.M => SDL_Scancode.SDL_SCANCODE_M,
            Keys.N => SDL_Scancode.SDL_SCANCODE_N,
            Keys.O => SDL_Scancode.SDL_SCANCODE_O,
            Keys.P => SDL_Scancode.SDL_SCANCODE_P,
            Keys.Q => SDL_Scancode.SDL_SCANCODE_Q,
            Keys.R => SDL_Scancode.SDL_SCANCODE_R,
            Keys.S => SDL_Scancode.SDL_SCANCODE_S,
            Keys.T => SDL_Scancode.SDL_SCANCODE_T,
            Keys.U => SDL_Scancode.SDL_SCANCODE_U,
            Keys.V => SDL_Scancode.SDL_SCANCODE_V,
            Keys.W => SDL_Scancode.SDL_SCANCODE_W,
            Keys.X => SDL_Scancode.SDL_SCANCODE_X,
            Keys.Y => SDL_Scancode.SDL_SCANCODE_Y,
            Keys.Z => SDL_Scancode.SDL_SCANCODE_Z,
            // symbols
            Keys.Grave => SDL_Scancode.SDL_SCANCODE_GRAVE,
            Keys.Minus => SDL_Scancode.SDL_SCANCODE_MINUS,
            Keys.Equal => SDL_Scancode.SDL_SCANCODE_EQUALS,
            Keys.LeftBracket => SDL_Scancode.SDL_SCANCODE_LEFTBRACKET,
            Keys.RightBracket => SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET,
            Keys.Backslash => SDL_Scancode.SDL_SCANCODE_BACKSLASH,
            Keys.Semicolon => SDL_Scancode.SDL_SCANCODE_SEMICOLON,
            Keys.Quote => SDL_Scancode.SDL_SCANCODE_APOSTROPHE,
            Keys.Comma => SDL_Scancode.SDL_SCANCODE_COMMA,
            Keys.Period => SDL_Scancode.SDL_SCANCODE_PERIOD,
            Keys.Slash => SDL_Scancode.SDL_SCANCODE_SLASH,
            // F keys
            Keys.F1 => SDL_Scancode.SDL_SCANCODE_F1,
            Keys.F2 => SDL_Scancode.SDL_SCANCODE_F2,
            Keys.F3 => SDL_Scancode.SDL_SCANCODE_F3,
            Keys.F4 => SDL_Scancode.SDL_SCANCODE_F4,
            Keys.F5 => SDL_Scancode.SDL_SCANCODE_F5,
            Keys.F6 => SDL_Scancode.SDL_SCANCODE_F6,
            Keys.F7 => SDL_Scancode.SDL_SCANCODE_F7,
            Keys.F8 => SDL_Scancode.SDL_SCANCODE_F8,
            Keys.F9 => SDL_Scancode.SDL_SCANCODE_F9,
            Keys.F10 => SDL_Scancode.SDL_SCANCODE_F10,
            Keys.F11 => SDL_Scancode.SDL_SCANCODE_F11,
            Keys.F12 => SDL_Scancode.SDL_SCANCODE_F12,
            Keys.F13 => SDL_Scancode.SDL_SCANCODE_F13,
            Keys.F14 => SDL_Scancode.SDL_SCANCODE_F14,
            Keys.F15 => SDL_Scancode.SDL_SCANCODE_F15,
            Keys.F16 => SDL_Scancode.SDL_SCANCODE_F16,
            Keys.F17 => SDL_Scancode.SDL_SCANCODE_F17,
            Keys.F18 => SDL_Scancode.SDL_SCANCODE_F18,
            Keys.F19 => SDL_Scancode.SDL_SCANCODE_F19,
            Keys.F20 => SDL_Scancode.SDL_SCANCODE_F20,
            Keys.F21 => SDL_Scancode.SDL_SCANCODE_F21,
            Keys.F22 => SDL_Scancode.SDL_SCANCODE_F22,
            Keys.F23 => SDL_Scancode.SDL_SCANCODE_F23,
            Keys.F24 => SDL_Scancode.SDL_SCANCODE_F24,
            // Arrow keys
            Keys.Up => SDL_Scancode.SDL_SCANCODE_UP,
            Keys.Down => SDL_Scancode.SDL_SCANCODE_DOWN,
            Keys.Left => SDL_Scancode.SDL_SCANCODE_LEFT,
            Keys.Right => SDL_Scancode.SDL_SCANCODE_RIGHT,
            // Special non-modifier keys
            Keys.Backspace => SDL_Scancode.SDL_SCANCODE_BACKSPACE,
            Keys.Enter => SDL_Scancode.SDL_SCANCODE_RETURN,
            Keys.Space => SDL_Scancode.SDL_SCANCODE_SPACE,
            Keys.Tab => SDL_Scancode.SDL_SCANCODE_TAB,
            Keys.Escape => SDL_Scancode.SDL_SCANCODE_ESCAPE,
            Keys.Insert => SDL_Scancode.SDL_SCANCODE_INSERT,
            Keys.PrintScreen => SDL_Scancode.SDL_SCANCODE_PRINTSCREEN,
            Keys.Pause => SDL_Scancode.SDL_SCANCODE_PAUSE,
            Keys.Home => SDL_Scancode.SDL_SCANCODE_HOME,
            Keys.End => SDL_Scancode.SDL_SCANCODE_END,
            Keys.Delete => SDL_Scancode.SDL_SCANCODE_DELETE,
            Keys.PageUp => SDL_Scancode.SDL_SCANCODE_PAGEUP,
            Keys.PageDown => SDL_Scancode.SDL_SCANCODE_PAGEDOWN,
            // Modifiers
            Keys.LeftControl => SDL_Scancode.SDL_SCANCODE_LCTRL,
            Keys.RightControl => SDL_Scancode.SDL_SCANCODE_RCTRL,
            Keys.LeftAlt => SDL_Scancode.SDL_SCANCODE_LALT,
            Keys.RightAlt => SDL_Scancode.SDL_SCANCODE_RALT,
            Keys.LeftShift => SDL_Scancode.SDL_SCANCODE_LSHIFT,
            Keys.RightShift => SDL_Scancode.SDL_SCANCODE_RSHIFT,
            Keys.LeftApplication => SDL_Scancode.SDL_SCANCODE_LGUI,
            Keys.RightApplication => SDL_Scancode.SDL_SCANCODE_RGUI,
            Keys.NumberLock => SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR,
            Keys.CapsLock => SDL_Scancode.SDL_SCANCODE_CAPSLOCK,
            Keys.ScrollLock => SDL_Scancode.SDL_SCANCODE_SCROLLLOCK,
            // Numpad
            Keys.Keypad0 => SDL_Scancode.SDL_SCANCODE_KP_0,
            Keys.Keypad1 => SDL_Scancode.SDL_SCANCODE_KP_1,
            Keys.Keypad2 => SDL_Scancode.SDL_SCANCODE_KP_2,
            Keys.Keypad3 => SDL_Scancode.SDL_SCANCODE_KP_3,
            Keys.Keypad4 => SDL_Scancode.SDL_SCANCODE_KP_4,
            Keys.Keypad5 => SDL_Scancode.SDL_SCANCODE_KP_5,
            Keys.Keypad6 => SDL_Scancode.SDL_SCANCODE_KP_6,
            Keys.Keypad7 => SDL_Scancode.SDL_SCANCODE_KP_7,
            Keys.Keypad8 => SDL_Scancode.SDL_SCANCODE_KP_8,
            Keys.Keypad9 => SDL_Scancode.SDL_SCANCODE_KP_9,
            Keys.Add => SDL_Scancode.SDL_SCANCODE_KP_PLUS,
            Keys.Subtract => SDL_Scancode.SDL_SCANCODE_KP_MINUS,
            Keys.Multiply => SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY,
            Keys.Divide => SDL_Scancode.SDL_SCANCODE_KP_DIVIDE,
            Keys.Decimal => SDL_Scancode.SDL_SCANCODE_KP_DECIMAL,
#pragma warning disable CS0618
            Keys.KeypadEqual => SDL_Scancode.SDL_SCANCODE_KP_EQUALS,

            // Other random keys that are in this enum
            Keys.Help => SDL_Scancode.SDL_SCANCODE_HELP,
            Keys.Menu => SDL_Scancode.SDL_SCANCODE_MENU,
            Keys.ContextMenu => SDL_Scancode.SDL_SCANCODE_APPLICATION,
            Keys.Clear => SDL_Scancode.SDL_SCANCODE_CLEAR,
            
            _ => SDL_Scancode.SDL_SCANCODE_UNKNOWN
        };
    }
#pragma warning restore CS0618

    private SDL_Keymod _currentMods = 0;
    
    public uint ProcessKeyEvent(KeyEventArgs args)
    {
        SDL_Keymod mod = GetKeyMod(args.Key);
        if (mod != SDL_Keymod.KMOD_NONE)
        {
            if (args.KeyEventType == KeyEventType.KeyDown)
                _currentMods |= mod;
            else
                _currentMods &= ~mod;
        }

        SDL_Scancode code = GetScanCode(args.Key);
        
        // ideally we can just pass modifiers to the input plugin
        // but mupen64plus-input-sdl does *not* seem to like it
        return (uint) code | ((uint) (mod != SDL_Keymod.KMOD_NONE ? 0 : _currentMods) << 16);
    }
}