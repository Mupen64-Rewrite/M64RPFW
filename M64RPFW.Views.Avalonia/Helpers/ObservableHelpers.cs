using System;

namespace M64RPFW.Views.Avalonia.Helpers;

public static class ObservableHelpers
{
    private class FuncObserver<T> : IObserver<T>
    {
        public FuncObserver(Action<T> listener)
        {
            _listener = listener;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            _listener(value);
        }

        private Action<T> _listener;
    }

    public static void ObserveChanges<T>(this IObservable<T> observable, Action<T> listener)
    {
        observable.Subscribe(new FuncObserver<T>(listener));
    }
}