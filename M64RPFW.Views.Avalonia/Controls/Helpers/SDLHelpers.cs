using System;
using Windows.Win32.Foundation;
using Avalonia.Input;
using Avalonia.Platform;
using M64RPFW.Models.Types;
using M64RPFW.Views.Avalonia.Controls.Helpers.Platform;
using M64RPFW.Views.Avalonia.Helpers.Platform;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
// ReSharper disable ParameterHidesMember

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static unsafe class SDLHelpers
{
    private static Sdl? _sdl;
    public static Sdl sdl = _sdl ??= Sdl.GetApi();
    
    public static GLattr ToSdlAttr(Mupen64PlusTypes.GLAttribute attr)
    {
        return attr switch
        {
            Mupen64PlusTypes.GLAttribute.DoubleBuffer => GLattr.Doublebuffer,
            Mupen64PlusTypes.GLAttribute.BufferSize => GLattr.BufferSize,
            Mupen64PlusTypes.GLAttribute.DepthSize => GLattr.DepthSize,
            Mupen64PlusTypes.GLAttribute.RedSize => GLattr.RedSize,
            Mupen64PlusTypes.GLAttribute.GreenSize => GLattr.GreenSize,
            Mupen64PlusTypes.GLAttribute.BlueSize => GLattr.BlueSize,
            Mupen64PlusTypes.GLAttribute.AlphaSize => GLattr.AlphaSize,
            Mupen64PlusTypes.GLAttribute.MultisampleBuffers => GLattr.Multisamplebuffers,
            Mupen64PlusTypes.GLAttribute.MultisampleSamples => GLattr.Multisamplesamples,
            Mupen64PlusTypes.GLAttribute.ContextMajorVersion => GLattr.ContextMajorVersion,
            Mupen64PlusTypes.GLAttribute.ContextMinorVersion => GLattr.ContextMinorVersion,
            _ => (GLattr) (-1)
        };
    }

    public static int GetMupenGLAttribute(this Sdl sdl, Mupen64PlusTypes.GLAttribute attr)
    {
        switch (attr)
        {
            case Mupen64PlusTypes.GLAttribute.SwapControl:
                return sdl.GLGetSwapInterval();
            case Mupen64PlusTypes.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = 0;
                sdl.GLGetAttribute(GLattr.ContextProfileMask, (int*) &profile);
                return profile switch
                {
                    GLprofile.Core => (int) Mupen64PlusTypes.GLContextType.Core,
                    GLprofile.Compatibility => (int) Mupen64PlusTypes.GLContextType.Compatibilty,
                    GLprofile.ES => (int) Mupen64PlusTypes.GLContextType.ES,
                    _ => 0
                };
            }
            default:
            {
                int value = 0;
                var sdlAttr = ToSdlAttr(attr);

                if (sdlAttr == (GLattr) (-1))
                    return 0;

                sdl.GLGetAttribute(sdlAttr, ref value);
                return value;
            }
        }
    }

    public static void SetMupenGLAttribute(this Sdl sdl, Mupen64PlusTypes.GLAttribute attr, int value)
    {
        switch (attr)
        {
            case Mupen64PlusTypes.GLAttribute.SwapControl:
                sdl.GLSetSwapInterval(value);
                return;
            case Mupen64PlusTypes.GLAttribute.ContextProfileMask:
            {
                GLprofile profile = (Mupen64PlusTypes.GLContextType) value switch
                {
                    Mupen64PlusTypes.GLContextType.Core => GLprofile.Core,
                    Mupen64PlusTypes.GLContextType.Compatibilty => GLprofile.Compatibility,
                    Mupen64PlusTypes.GLContextType.ES => GLprofile.ES,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
                sdl.GLSetAttribute(GLattr.ContextProfileMask, (int) profile);
                return;
            }
            default:
            {
                var sdlAttr = ToSdlAttr(attr);
                if (sdlAttr == (GLattr) (-1))
                    return;

                sdl.GLSetAttribute(sdlAttr, value);
                return;
            }
        }
    }

    public static Scancode ToSDLScancode(Key key)
    {
        return key switch
        {
            Key.None => Scancode.ScancodeUnknown,
            Key.Cancel => Scancode.ScancodeCancel,
            Key.Back => Scancode.ScancodeACBack,
            Key.Tab => Scancode.ScancodeTab,
            Key.LineFeed => Scancode.ScancodeReturn2,
            Key.Clear => Scancode.ScancodeClear,
            Key.Return => Scancode.ScancodeReturn,
            Key.Pause => Scancode.ScancodePause,
            Key.CapsLock => Scancode.ScancodeCapslock,

            Key.HangulMode => Scancode.ScancodeLang1,
            Key.JunjaMode => Scancode.ScancodeUnknown,
            Key.FinalMode => Scancode.ScancodeUnknown,
            Key.KanjiMode => Scancode.ScancodeLang2,

            Key.Escape => Scancode.ScancodeEscape,

            Key.ImeConvert => Scancode.ScancodeUnknown,
            Key.ImeNonConvert => Scancode.ScancodeUnknown,
            Key.ImeAccept => Scancode.ScancodeUnknown,
            Key.ImeModeChange => Scancode.ScancodeUnknown,

            Key.Space => Scancode.ScancodeSpace,

            Key.PageUp => Scancode.ScancodePageup,
            Key.PageDown => Scancode.ScancodePagedown,
            Key.End => Scancode.ScancodeEnd,
            Key.Home => Scancode.ScancodeHome,
            Key.Left => Scancode.ScancodeLeft,
            Key.Up => Scancode.ScancodeUp,
            Key.Right => Scancode.ScancodeRight,
            Key.Down => Scancode.ScancodeDown,

            Key.Select => Scancode.ScancodeSelect,
            Key.Print => Scancode.ScancodePrintscreen,
            Key.Execute => Scancode.ScancodeExecute,
            Key.PrintScreen => Scancode.ScancodePrintscreen,
            Key.Insert => Scancode.ScancodeInsert,
            Key.Delete => Scancode.ScancodeDelete,
            Key.Help => Scancode.ScancodeHelp,

            Key.D0 => Scancode.Scancode0,
            Key.D1 => Scancode.Scancode1,
            Key.D2 => Scancode.Scancode2,
            Key.D3 => Scancode.Scancode3,
            Key.D4 => Scancode.Scancode4,
            Key.D5 => Scancode.Scancode5,
            Key.D6 => Scancode.Scancode6,
            Key.D7 => Scancode.Scancode7,
            Key.D8 => Scancode.Scancode8,
            Key.D9 => Scancode.Scancode9,

            Key.A => Scancode.ScancodeA,
            Key.B => Scancode.ScancodeB,
            Key.C => Scancode.ScancodeC,
            Key.D => Scancode.ScancodeD,
            Key.E => Scancode.ScancodeE,
            Key.F => Scancode.ScancodeF,
            Key.G => Scancode.ScancodeG,
            Key.H => Scancode.ScancodeH,
            Key.I => Scancode.ScancodeI,
            Key.J => Scancode.ScancodeJ,
            Key.K => Scancode.ScancodeK,
            Key.L => Scancode.ScancodeL,
            Key.M => Scancode.ScancodeM,
            Key.N => Scancode.ScancodeN,
            Key.O => Scancode.ScancodeO,
            Key.P => Scancode.ScancodeP,
            Key.Q => Scancode.ScancodeQ,
            Key.R => Scancode.ScancodeR,
            Key.S => Scancode.ScancodeS,
            Key.T => Scancode.ScancodeT,
            Key.U => Scancode.ScancodeU,
            Key.V => Scancode.ScancodeV,
            Key.W => Scancode.ScancodeW,
            Key.X => Scancode.ScancodeX,
            Key.Y => Scancode.ScancodeY,
            Key.Z => Scancode.ScancodeZ,

            Key.LWin => Scancode.ScancodeLgui,
            Key.RWin => Scancode.ScancodeRgui,
            Key.Apps => Scancode.ScancodeApplication,
            Key.Sleep => Scancode.ScancodeSleep,

            Key.NumPad0 => Scancode.ScancodeKP0,
            Key.NumPad1 => Scancode.ScancodeKP1,
            Key.NumPad2 => Scancode.ScancodeKP2,
            Key.NumPad3 => Scancode.ScancodeKP3,
            Key.NumPad4 => Scancode.ScancodeKP4,
            Key.NumPad5 => Scancode.ScancodeKP5,
            Key.NumPad6 => Scancode.ScancodeKP6,
            Key.NumPad7 => Scancode.ScancodeKP7,
            Key.NumPad8 => Scancode.ScancodeKP8,
            Key.NumPad9 => Scancode.ScancodeKP9,
            Key.Multiply => Scancode.ScancodeKPMultiply,
            Key.Add => Scancode.ScancodeKPPlus,
            Key.Separator => Scancode.ScancodeSeparator,
            Key.Subtract => Scancode.ScancodeKPMinus,
            Key.Decimal => Scancode.ScancodeKPDecimal,
            Key.Divide => Scancode.ScancodeKPDivide,
            
            Key.F1 => Scancode.ScancodeF1,
            Key.F2 => Scancode.ScancodeF2,
            Key.F3 => Scancode.ScancodeF3,
            Key.F4 => Scancode.ScancodeF4,
            Key.F5 => Scancode.ScancodeF5,
            Key.F6 => Scancode.ScancodeF6,
            Key.F7 => Scancode.ScancodeF7,
            Key.F8 => Scancode.ScancodeF8,
            Key.F9 => Scancode.ScancodeF9,
            Key.F10 => Scancode.ScancodeF10,
            Key.F11 => Scancode.ScancodeF11,
            Key.F12 => Scancode.ScancodeF12,
            Key.F13 => Scancode.ScancodeF13,
            Key.F14 => Scancode.ScancodeF14,
            Key.F15 => Scancode.ScancodeF15,
            Key.F16 => Scancode.ScancodeF16,
            Key.F17 => Scancode.ScancodeF17,
            Key.F18 => Scancode.ScancodeF18,
            Key.F19 => Scancode.ScancodeF19,
            Key.F20 => Scancode.ScancodeF20,
            Key.F21 => Scancode.ScancodeF21,
            Key.F22 => Scancode.ScancodeF22,
            Key.F23 => Scancode.ScancodeF23,
            Key.F24 => Scancode.ScancodeF24,
            
            Key.NumLock => Scancode.ScancodeNumlockclear,
            Key.Scroll => Scancode.ScancodeScrolllock,
            Key.LeftShift => Scancode.ScancodeLshift,
            Key.RightShift => Scancode.ScancodeRshift,
            Key.LeftCtrl => Scancode.ScancodeLctrl,
            Key.RightCtrl => Scancode.ScancodeRctrl,
            Key.LeftAlt => Scancode.ScancodeLalt,
            Key.RightAlt => Scancode.ScancodeRalt,
            
            Key.BrowserBack => Scancode.ScancodeACBack,
            Key.BrowserForward => Scancode.ScancodeACForward,
            Key.BrowserRefresh => Scancode.ScancodeACRefresh,
            Key.BrowserStop => Scancode.ScancodeACStop,
            Key.BrowserSearch => Scancode.ScancodeACSearch,
            Key.BrowserFavorites => Scancode.ScancodeACBookmarks,
            Key.BrowserHome => Scancode.ScancodeACHome,
            
            Key.VolumeMute => Scancode.ScancodeMute,
            Key.VolumeDown => Scancode.ScancodeVolumedown,
            Key.VolumeUp => Scancode.ScancodeVolumeup,
            
            Key.MediaNextTrack => Scancode.ScancodeAudionext,
            Key.MediaPreviousTrack => Scancode.ScancodeAudioprev,
            Key.MediaStop => Scancode.ScancodeAudiostop,
            Key.MediaPlayPause => Scancode.ScancodeAudioplay,
            
            Key.LaunchMail => Scancode.ScancodeMail,
            Key.SelectMedia => Scancode.ScancodeMediaselect,
            Key.LaunchApplication1 => Scancode.ScancodeApp1,
            Key.LaunchApplication2 => Scancode.ScancodeApp2,
            

            Key.OemSemicolon => Scancode.ScancodeSemicolon,
            Key.OemPlus => Scancode.ScancodeEquals,
            Key.OemComma => Scancode.ScancodeComma,
            Key.OemMinus => Scancode.ScancodeMinus,
            Key.OemPeriod => Scancode.ScancodePeriod,
            Key.OemQuestion => Scancode.ScancodeSlash,
            Key.OemTilde => Scancode.ScancodeGrave,
            Key.AbntC1 => Scancode.ScancodeUnknown,
            Key.AbntC2 => Scancode.ScancodeUnknown,
            Key.OemOpenBrackets => Scancode.ScancodeLeftbracket,
            Key.OemPipe => Scancode.ScancodeBackslash,
            Key.OemCloseBrackets => Scancode.ScancodeRightbracket,
            Key.OemQuotes => Scancode.ScancodeApostrophe,
            Key.Oem8 => Scancode.ScancodeRctrl,
            Key.OemBackslash => Scancode.ScancodeNonusbackslash,
            
            Key.ImeProcessed => Scancode.ScancodeUnknown,
            Key.System => Scancode.ScancodeUnknown,
            
            Key.OemAttn => Scancode.ScancodeSysreq,
            
            // Key.OemFinish => Scancode.ScancodeUnknown,
            // Key.OemCopy => Scancode.ScancodeUnknown,
            // Key.OemAuto => Scancode.ScancodeUnknown,
            // Key.OemEnlw => Scancode.ScancodeUnknown,
            // Key.OemBackTab => Scancode.ScancodeUnknown,
            // Key.DbeNoRoman => Scancode.ScancodeUnknown,
            
            Key.CrSel => Scancode.ScancodeCrsel,
            Key.ExSel => Scancode.ScancodeExsel,
            
            // Key.EraseEof => Scancode.ScancodeUnknown,
            // Key.DbeCodeInput => Scancode.ScancodeUnknown,
            // Key.DbeNoCodeInput => Scancode.ScancodeUnknown,
            // Key.DbeDetermineString => Scancode.ScancodeUnknown,
            // Key.Pa1 => Scancode.ScancodeUnknown,
            // Key.OemClear => Scancode.ScancodeUnknown,
            
            Key.DeadCharProcessed => Scancode.ScancodeUnknown,
            
            Key.FnLeftArrow => Scancode.ScancodeLeft,
            Key.FnRightArrow => Scancode.ScancodeRight,
            Key.FnUpArrow => Scancode.ScancodeUp,
            Key.FnDownArrow => Scancode.ScancodeDown,

            _ => Scancode.ScancodeUnknown
        };
    }

    public static Keymod ToSDLKeymod(KeyModifiers mod)
    {
        Keymod res = 0;
        res |= (mod & KeyModifiers.Alt) == KeyModifiers.Alt ? Keymod.Alt : 0;
        res |= (mod & KeyModifiers.Control) == KeyModifiers.Control ? Keymod.Ctrl : 0;
        res |= (mod & KeyModifiers.Shift) == KeyModifiers.Shift ? Keymod.Shift : 0;
        res |= (mod & KeyModifiers.Meta) == KeyModifiers.Meta ? Keymod.Gui : 0;
        return res;
    }

    private unsafe class RestoreSdlContext : IDisposable
    {
        private Sdl _sdl;
        private Window* _win;
        private void* _gl;

        public RestoreSdlContext(Sdl sdl)
        {
            _sdl = sdl;
            _win = _sdl.GLGetCurrentWindow();
            _gl = _sdl.GLGetCurrentContext();
        }

        public void Dispose()
        {
            _sdl.GLMakeCurrent(_win, _gl);
        }
    }

    public static IDisposable GLMakeCurrentTemp(this Sdl sdl, Window* win, void* ctx)
    {
        var handle = new RestoreSdlContext(sdl);
        sdl.GLMakeCurrent(win, null);
        if (sdl.GLMakeCurrent(win, ctx) < 0)
            throw new SDLException();
        return handle;
    }

    public static Window* CreateWindow(this Sdl sdl, string title, int x, int y, int w, int h, WindowFlags flags)
    {
        return sdl.CreateWindow(title, x, y, w, h, (uint) flags);
    }

    public static GL GetGLBinding(this Sdl sdl)
    {
        return GL.GetApi(sym => (IntPtr) sdl.GLGetProcAddress(sym));
    }
}