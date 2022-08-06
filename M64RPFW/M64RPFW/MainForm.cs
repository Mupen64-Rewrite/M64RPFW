using System;
using Eto.Forms;
using Eto.Drawing;
using M64RPFW.Controls;
using M64RPFW.Models.Emulation.Mupen64Plus;
using OpenTK.Graphics.ES11;

namespace M64PRR;

public partial class MainForm : Form
{
    public MainForm()
    {
        Title = "My Eto Form";
        MinimumSize = new Size(200, 200);

        var subWindow = new GLSubWindow();

        Content = subWindow;

        // create a few commands that can be used for the menu and toolbar
        var openVideo = new Command { MenuText = "Open video surface" };
        openVideo.Executed += (sender, e) =>
        {
            if (!_videoModeSet)
            {
                subWindow.SetAttribute(Mupen64Plus.GLAttribute.ContextMajorVersion, 3);
                subWindow.SetAttribute(Mupen64Plus.GLAttribute.ContextMinorVersion, 3);
                subWindow.SetVideoMode(new System.Drawing.Size { Width = 640, Height = 480 }, 0,
                    Mupen64Plus.VideoMode.Windowed, 0);
                _videoModeSet = true;
                GL.LoadBindings(subWindow);
            }
            GL.ClearColor(0.5f, 0.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            subWindow.SwapBuffers();
        };

        var closeVideo = new Command { MenuText = "Close video surface" };
        closeVideo.Executed += (sender, args) =>
        {
            if (_videoModeSet)
            {
                subWindow.CloseVideo();
                _videoModeSet = false;
            }
        };

        // create menu
        Menu = new MenuBar
        {
            Items =
            {
                // File submenu
                new SubMenuItem { Text = "&File", Items = { openVideo, closeVideo } },
                // new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
                // new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
            },
            ApplicationItems =
            {
                // application (OS X) or file menu (others)
                new ButtonMenuItem { Text = "&Preferences..." },
            }
        };
    }

    private bool _videoModeSet = false;
}