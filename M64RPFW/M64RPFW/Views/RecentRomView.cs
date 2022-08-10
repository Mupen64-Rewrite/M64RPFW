using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Models;
using M64RPFW.Presenters;

namespace M64RPFW.Views;

internal class RecentRomView : Panel
{
    public RecentRomView(MainView parent)
    {
        _presenter = new RecentRomPresenter(this, parent);
        {
            // TEMPORARY TEST DATA
            _presenter.RecentRoms.Add(new RomFile("/home/jgcodes/Public/VM/ROMS/sm64-us.z64"));
            _presenter.RecentRoms.Add(new RomFile("/home/jgcodes/Public/VM/ROMS/sm64-jp.z64"));
            _presenter.RecentRoms.Add(new RomFile("/home/jgcodes/Public/VM/ROMS/sm64-eu.z64"));
        }

        RomGrid = new GridView
        {
            // Bind VM's RomObjects as backing store for GridView
            DataStore = _presenter.RecentRoms
        };
        {
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Name",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RomFile, string>(o => o.FriendlyName)
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Region",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RomFile, string>(o => o.Region.ToString())
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Filename",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RomFile, string>(o => o.FileName)
                },
                Editable = false
            });
        }

        RomGrid.CellDoubleClick += (_, args) => _presenter.SelectAndRunROM(args.Row);

        Content = RomGrid;
        Padding = Padding.Empty;
    }

    private readonly RecentRomPresenter _presenter;
    internal readonly GridView RomGrid;
}