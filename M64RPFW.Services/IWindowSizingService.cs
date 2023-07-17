

using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

public interface IWindowSizingService
{
    public WindowSize GetWindowSize();
    public void SizeToFit(WindowSize size);
    public void UnlockWindowSize();
}