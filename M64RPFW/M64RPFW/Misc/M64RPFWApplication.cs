using System;
using System.ComponentModel;
using Eto;
using Eto.Forms;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Models.Settings;
using M64RPFW.Views;
using UnhandledExceptionEventArgs = Eto.UnhandledExceptionEventArgs;

namespace M64RPFW.Misc;

public class M64RPFWApplication : Application
{
    
    public M64RPFWApplication() 
    {
        MainForm = new MainView();
    }
    
    public M64RPFWApplication(string platform) : base(platform) 
    {
        MainForm = new MainView();
    }
    
    public M64RPFWApplication(Platform platform) : base(platform) 
    {
        MainForm = new MainView();
    }

    protected override void OnInitialized(EventArgs e)
    {
        UIThreadCheckMode = UIThreadCheckMode.Warning;
        Settings.Init();

        MainForm.Visible = true;
    }

    protected override void OnUnhandledException(UnhandledExceptionEventArgs e)
    {
        object exc = e.ExceptionObject;
        string typeName = exc.GetType().FullName;
        MessageBox.Show($"A fatal exception occurred! ({typeName})", "Fatal Exception");
    }

    protected override void OnTerminating(CancelEventArgs e)
    {
        
    }
    
    
}