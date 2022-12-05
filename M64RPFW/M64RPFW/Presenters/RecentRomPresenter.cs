using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using M64RPFW.Models;
using M64RPFW.Models.Settings;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal class RecentRomPresenter
{
    public RecentRomPresenter(RecentRomView view, MainView parent)
    {
        _view = view;
        _parent = parent.Presenter;

        var setting = Settings.RPFW.Roms.Recent;

        RecentRoms = new(setting.Select(path => new RomFile(path)));

        RecentRoms.CollectionChanged += (_, args) =>
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    setting.InsertRange(args.NewStartingIndex, args.NewItems!.Cast<RomFile>().Select(rom => rom._path));
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    setting.RemoveRange(args.OldStartingIndex, args.OldItems!.Count);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    int len = args.OldItems!.Count;
                    int begin = args.OldStartingIndex;
                    
                    for (int i = 0; i < len; i++)
                    {
                        setting[begin + i] = ((RomFile) args.NewItems![i]!)._path;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    setting.RemoveRange(args.OldStartingIndex, args.OldItems!.Count);
                    setting.InsertRange(args.NewStartingIndex, args.OldItems!.Cast<RomFile>().Select(rom => rom._path));
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    setting.Clear();
                    setting.AddRange(args.NewItems!.Cast<RomFile>().Select(rom => rom._path));
                    break;
                }
            }
            Console.WriteLine($"P: {RecentRoms}\nS: {setting}\n");
        };
    }
    
    public void SelectAndRunROM(int index)
    {
        if (index < 0)
            return;
        _parent.LaunchRom(RecentRoms[index]);
    }

    private RecentRomView _view;
    private MainPresenter _parent;
    public ObservableCollection<RomFile> RecentRoms { get; }
}