using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels.Interfaces
{
    /// <summary>
    /// The default <see langword="interface"/> for a provider that interacts with the recent <see cref="ObservableCollection{RomViewModel}"/>
    /// </summary>
    public interface IRecentRomsProvider
    {
        /// <summary>
        /// Fetches all recent Roms
        /// </summary>
        /// <returns>All recent roms</returns>
        public ObservableCollection<RomViewModel> Get();

        /// <summary>
        /// Adds a rom to the <see cref="ObservableCollection{RomViewModel}"/>
        /// </summary>
        /// <param name="Rom">The rom to be added</param>
        public void Add(RomViewModel Rom);
    }
}
