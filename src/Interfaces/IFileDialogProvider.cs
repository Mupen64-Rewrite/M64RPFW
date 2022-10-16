using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Interfaces
{
    public interface IFileDialogProvider
    {
        public (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions);
        public (string ReturnedPath, bool Cancelled) SaveFileDialogPrompt(string[] validExtensions);
    }
}
