﻿
namespace M64RPFW.ViewModels.Interfaces
{
    public interface ISettingsProvider
    {
        public T GetSetting<T>(string key);
        public void SetSetting<T>(string key, T value, bool saveAfter = false);
        public void Save();
    }
}
