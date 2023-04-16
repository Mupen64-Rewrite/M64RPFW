using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Converter;

public class ConfigEditingDataTemplate : IRecyclingDataTemplate
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

    private static Control BuildIntControl(ConfigKeyViewModel vm)
    {
        return new NumericUpDown
        {
            [!NumericUpDown.ValueProperty] = new Binding("Value", BindingMode.TwoWay)
            {
                Converter = new TwoFuncValueConverter<int, decimal>(i => i, d => (int) d)
            },
            Increment = 1M,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            FormatString = "{0:F0}"
        };
    }

    private static Control BuildFloatControl(ConfigKeyViewModel vm)
    {
        return new NumericUpDown
        {
            [!NumericUpDown.ValueProperty] = new Binding("Value", BindingMode.TwoWay)
            {
                Converter = new TwoFuncValueConverter<float, decimal>(i => (decimal) i, d => (float) d)
            },
            Increment = 0.1M,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }
    
    private static Control BuildStringControl(ConfigKeyViewModel vm)
    {
        return new TextBox
        {
            [!TextBox.TextProperty] = new Binding("Value", BindingMode.TwoWay),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }
    
    private static Control BuildBoolControl(ConfigKeyViewModel vm)
    {
        return new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding("Value", BindingMode.TwoWay),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }
}