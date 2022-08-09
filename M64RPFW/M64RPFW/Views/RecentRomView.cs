using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Presenters;

namespace M64RPFW.Views;

internal class RecentRomView : Panel
{
    public RecentRomView(MainView parent)
    {
        _presenter = new RecentRomPresenter(this, parent.Presenter);
        {
            // TEMPORARY TEST DATA
            _presenter.RecentRoms.Add(new RecentRom { Name = "Super Mario 64 (U) [!]", Region = "USA", Filename = "sm64-us.z64"});
            _presenter.RecentRoms.Add(new RecentRom { Name = "Super Mario 64 (J) [!]", Region = "Japan", Filename = "sm64-jp.z64"});
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
                    Binding = Binding.Property<RecentRom, string>(o => o.Name)
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Region",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RecentRom, string>(o => o.Region)
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Filename",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RecentRom, string>(o => o.Filename)
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