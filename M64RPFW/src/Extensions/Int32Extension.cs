using System;
using System.Windows.Markup;

namespace M64RPFW.Extensions;

public sealed class Int32Extension : MarkupExtension
{
    public Int32Extension(int value)
    {
        Value = value;
    }

    public int Value { get; set; }

    public override object ProvideValue(IServiceProvider sp)
    {
        return Value;
    }
}