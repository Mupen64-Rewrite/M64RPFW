namespace M64RPFW.ViewModels.Interfaces
{
    public interface ILocalizationManager
    {
        public string GetString(string key);
        public void SetLocale(string localeKey);
    }
}
