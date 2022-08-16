using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Models.Helpers;

namespace M64RPFW.Controls;

/// <summary>
/// Eto's built-in file picker kinda sucks. So I made this.
/// </summary>
public partial class CustomFilePicker : Panel
{
    public CustomFilePicker()
    {
        Padding = Padding.Empty;

        _pathTextBox = new TextBox();
        _browseButton = new Button
        {
            Text = "Browse..."
        };

        Content = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Items =
            {
                new StackLayoutItem(_pathTextBox, expand: true),
                new StackLayoutItem(_browseButton, expand: false)
            }
        };

        _browseButton.Command = DoBrowseCommand;


        _pathTextBox.GotFocus += (_, _) =>
        {
            _prevText = _pathTextBox.Text;
        };
        _pathTextBox.LostFocus += (_, _) =>
        {
            string newText = _pathTextBox.Text;
            if (!PathHelper.IsValid(newText))
            {
                _pathTextBox.Text = _prevText;
            }
            else if (PathValidator != null && !PathValidator(newText))
            {
                _pathTextBox.Text = _prevText;
            }
        };


        // Properties
        Filters = new Collection<FileFilter>();
        FileAction = FileAction.OpenFile;
        PathValidator = null;
    }

    [RelayCommand]
    private void DoBrowse()
    {
        string chosenPath;
        switch (FileAction)
        {
            case FileAction.OpenFile:
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                // start dialog in user home directory
                if (_pathTextBox.Text == "")
                    fileDialog.Directory = new Uri(new Uri("file://"),
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                else
                    fileDialog.Directory = new Uri(new Uri("file://"), Path.GetDirectoryName(_pathTextBox.Text));
                foreach (var filter in Filters)
                    fileDialog.Filters.Add(filter);

                var result = fileDialog.ShowDialog(this);
                if (result == DialogResult.Cancel)
                    return;

                chosenPath = fileDialog.FileName;

                break;
            }
            case FileAction.SaveFile:
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                if (_pathTextBox.Text == "")
                    fileDialog.Directory = new Uri(new Uri("file://"),
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                else
                    fileDialog.Directory = new Uri(new Uri("file://"), Path.GetDirectoryName(_pathTextBox.Text));
                foreach (var filter in Filters)
                    fileDialog.Filters.Add(filter);

                var result = fileDialog.ShowDialog(this);
                if (result == DialogResult.Cancel)
                    return;

                chosenPath = fileDialog.FileName;

                break;
            }
            case FileAction.SelectFolder:
            {
                SelectFolderDialog folderDialog = new SelectFolderDialog();
                // start dialog in user home directory
                if (_pathTextBox.Text == "")
                    folderDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                else
                    folderDialog.Directory = _pathTextBox.Text;

                var result = folderDialog.ShowDialog(this);
                if (result == DialogResult.Cancel)
                    return;

                chosenPath = folderDialog.Directory;
                
                break;
            }
            default:
                throw new ApplicationException("Invalid FileAction?");
        }

        if (PathValidator is not null && !PathValidator(chosenPath))
            return;

        _pathTextBox.Text = chosenPath;
    }

    public Collection<FileFilter> Filters { get; }
    public FileAction FileAction { get; set; }

    public Func<string, bool>? PathValidator { get; set; }

    public string CurrentPath
    {
        get => _pathTextBox.Text;
        set => _pathTextBox.Text = value;
    }

    public BindableBinding<TextControl, string> CurrentPathBinding => _pathTextBox.TextBinding;

    private readonly TextBox _pathTextBox;
    private readonly Button _browseButton;
    private string? _prevText;
}