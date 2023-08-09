using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Controls.Templates;

public class ConfigDataTemplate : IRecyclingDataTemplate
{
    public Control? Build(object? param)
    {
        return Build(param, null);
    }

    public bool Match(object? data)
    {
        return data is ConfigKeyViewModel;
    }

    public Control? Build(object? data, Control? existing)
    {
        if (data is not ConfigKeyViewModel vm)
            return null;
        if (existing != null)
            return existing;

        return vm switch
        {
            {Value: int} => BuildIntControl(vm),
            {Value: float} => BuildFloatControl(vm),
            {Value: string} => BuildStringControl(vm),
            {Value: bool} => BuildBoolControl(vm),
            _ => null
        };
    }

    private static Control? BuildIntControl(ConfigKeyViewModel vm)
    {
        return new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding("Value"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
    
    private static Control? BuildFloatControl(ConfigKeyViewModel vm)
    {
        return new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding("Value"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
    
    private static Control? BuildStringControl(ConfigKeyViewModel vm)
    {
        return new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding("Value"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
    
    private static Control? BuildBoolControl(ConfigKeyViewModel vm)
    {
        return new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding("Value"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}