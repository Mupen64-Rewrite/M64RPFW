using M64RPFW.Models.Emulation;
using M64RPFW.Services.Abstractions;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("savestate.savefile")]
    private void SaveToFile(string path)
    {
        Mupen64Plus.SaveStateToFile(path);
    }

    [LibFunction("savestate.loadfile")]
    private void LoadFromFile(string path)
    {
        Mupen64Plus.LoadStateFromFile(path);
    }
    
    [LibFunction("ioHelper.filediag")]
    private string ShowFileDialog(string filter, int type)
    {
        return type switch
        {
            0 => _filePickerService.ShowOpenFilePickerAsync(options: new[]
                {
                    new FilePickerOption("Supported files", filter.Split(";"))
                }, allowMultiple: false)
                .Result![0],
            1 => _filePickerService.ShowSaveFilePickerAsync(options: new[]
                {
                    new FilePickerOption("Supported files", filter.Split(";"))
                })
                .Result!,
            _ => throw new ArgumentException("Invalid file dialog type")
        };
    }
}