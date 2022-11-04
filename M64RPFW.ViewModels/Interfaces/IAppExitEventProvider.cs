using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.ViewModels.Interfaces
{
    /// <summary>
    /// The default <see langword="interface"/> for a provider that exposes app exit event functionality
    /// </summary>
    internal interface IAppExitEventProvider
    {
        /// <summary>
        /// Register an action to be invoked when the app exits
        /// </summary>
        /// <param name="action">The action which should be invoked on app exit</param>
        internal void Register(Action action);
    }
}
