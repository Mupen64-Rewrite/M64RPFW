using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Mupen64PlusRR.ViewModels.Interfaces;

public interface ISystemDialogService
{
    Task<string[]?> ShowOpenDialog(string title, List<FileDialogFilter> filters, bool allowMulti = true);
    Task<string?> ShowSaveDialog(string title, List<FileDialogFilter> filters);
}