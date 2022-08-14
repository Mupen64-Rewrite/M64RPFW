using Eto.Drawing;
using Eto.Forms;

namespace M64RPFW.Views;

public class SettingsView : Dialog
{
    public SettingsView()
    {
        Title = "Settings";
        ClientSize = new Size(480, 640);
        Content = new TabControl
        {
            Pages =
            {
                new TabPage
                {
                    Text = "General"
                },
                new TabPage
                {
                    Text = "Plugins"
                }
            }
        };
    }
}