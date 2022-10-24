using System;

namespace M64RPFW.ViewModels.Interfaces
{
    public interface IDrawingSurfaceProvider
    {
        public bool IsCreated { get; }
        public void Create(int width, int height);
        public void Draw(Array buffer, int width, int height);
    }
}
