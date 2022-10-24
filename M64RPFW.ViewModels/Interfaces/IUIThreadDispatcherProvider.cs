using System;

namespace M64RPFW.ViewModels.Interfaces
{
    public interface IUIThreadDispatcherProvider
    {
        public void Execute(Action action);
    }
}
