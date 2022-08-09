using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Presenters;

namespace M64RPFW.Views;

public class EmulatorView : Panel
{
    public EmulatorView(MainView parent)
    {
        _presenter = new EmulatorPresenter(this, parent.Presenter);
        
        SubWindow = new GLSubWindow();
        Content = SubWindow;
        
        Padding = Padding.Empty;
    }


    private EmulatorPresenter _presenter;
    internal GLSubWindow SubWindow { get; }
}