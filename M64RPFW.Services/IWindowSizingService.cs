

using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

public interface IWindowSizingService
{
    public WindowSize GetWindowSize();
    public void LayoutToFit(WindowSize size);
}