using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Rendering.Composition;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class CompositionHelpers
{
    public async static Task<T> GetRenderInterfaceFeature<T>(this Compositor compositor)
    {
        var feature =  await compositor.TryGetRenderInterfaceFeature(typeof(T));
        if (feature == null)
            throw new PlatformNotSupportedException($"Feature {typeof(T).FullName} not supported on this platform");
        return (T) feature;
    }

    

    public class CompositionUpdateAwaitable
    {
        private static Func<CompositionUpdateAwaitable, CompositionUpdateAwaiter> AwaiterFactory = null!;

        static CompositionUpdateAwaitable()
        {
            RuntimeHelpers.RunClassConstructor(typeof(CompositionUpdateAwaitable).TypeHandle);
        }
        
        public class CompositionUpdateAwaiter : INotifyCompletion
        {
            static CompositionUpdateAwaiter()
            {
                AwaiterFactory = parent => new CompositionUpdateAwaiter(parent);
            }

            private CompositionUpdateAwaitable _parent;

            private CompositionUpdateAwaiter(CompositionUpdateAwaitable parent)
            {
                _parent = parent;
            }

            public bool IsCompleted => _parent._completionFlag != 0;

            public void OnCompleted(Action continuation)
            {
                _parent._compositor.RequestCompositionUpdate(() =>
                {
                    Interlocked.Exchange(ref _parent._completionFlag, 1);
                    continuation();
                });
            }
        
            public void GetResult() {}
        }
        
        private Compositor _compositor;
        private int _completionFlag;
        
        internal CompositionUpdateAwaitable(Compositor compositor)
        {
            _compositor = compositor;
            _completionFlag = 0;
        }

        public CompositionUpdateAwaiter GetAwaiter()
        {
            return AwaiterFactory(this);
        }
    }

    public static CompositionUpdateAwaitable NextVSync(this Compositor compositor)
    {
        return new CompositionUpdateAwaitable(compositor);
    }
}