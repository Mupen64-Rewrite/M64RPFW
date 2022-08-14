using System;
using System.ComponentModel;
using Eto;
using Eto.Forms;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;

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

        MainForm.Visible = true;
    }

    protected override void OnTerminating(CancelEventArgs e)
    {
        
    }
    
    
}