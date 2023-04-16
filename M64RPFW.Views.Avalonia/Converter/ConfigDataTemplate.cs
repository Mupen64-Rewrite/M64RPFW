using Avalonia.Controls;
using Avalonia.Controls.Templates;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Converter;

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
        throw new System.NotImplementedException();
    }
    
    private static Control? BuildFloatControl(ConfigKeyViewModel vm)
    {
        throw new System.NotImplementedException();
    }
    
    private static Control? BuildStringControl(ConfigKeyViewModel vm)
    {
        throw new System.NotImplementedException();
    }
    
    private static Control? BuildBoolControl(ConfigKeyViewModel vm)
    {
        throw new System.NotImplementedException();
    }
}