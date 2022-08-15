using M64RPFW.Views;

namespace M64RPFW.Presenters;

public class SettingsPresenter
{
    public SettingsPresenter(SettingsView view)
    {
        _view = view;
    }

    private readonly SettingsView _view;
}