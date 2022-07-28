using Avalonia.Markup.Xaml;
using System;

namespace M64RPFWAvalonia.UI.ViewModels.Extensions
{
    public sealed class Int32Extension : MarkupExtension
    {
        public Int32Extension(int value) { this.Value = value; }
        public int Value { get; set; }
        public override object ProvideValue(IServiceProvider sp) { return Value; }
    }
}
