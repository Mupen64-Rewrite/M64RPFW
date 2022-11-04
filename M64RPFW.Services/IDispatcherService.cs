using System;

namespace M64RPFW.Services
{
    /// <summary>
    /// The default <see langword="interface"/> for a service that invokes the application dispatcher
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// Queues an <see cref="Action"/> on the dispatcher
        /// </summary>
        /// <param name="action">The </param>
        public void Execute(Action action);
    }
}
