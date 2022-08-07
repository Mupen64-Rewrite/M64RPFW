using Eto.Drawing;
using Eto.Forms;
using M64RPFW.ViewModels;

namespace M64RPFW.Views;

internal class RecentRomView : Panel
{
    public RecentRomView()
    {
        _viewModel = new RecentRomViewModel();
        {
            // TEMPORARY TEST DATA
            _viewModel.RomObjects.Add(new RecentROM { Name = "Super Mario 64 (U) [!]", Region = "USA", Filename = "sm64-us.z64"});
            _viewModel.RomObjects.Add(new RecentROM { Name = "Super Mario 64 (J) [!]", Region = "Japan", Filename = "sm64-jp.z64"});
        }

        RomGrid = new GridView
        {
            // Bind VM's RomObjects as backing store for GridView
            DataStore = _viewModel.RomObjects
        };
        {
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Name",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RecentROM, string>(o => o.Name)
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Region",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RecentROM, string>(o => o.Region)
                },
                Editable = false
            });
            RomGrid.Columns.Add(new GridColumn
            {
                HeaderText = "Filename",
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<RecentROM, string>(o => o.Filename)
                },
                Editable = false
            });
        }

        RomGrid.CellDoubleClick += (_, args) =>
        {
            _viewModel.SelectAndRunROM(args.Row);
        };

        Content = RomGrid;
        Padding = Padding.Empty;
    }

    private readonly RecentRomViewModel _viewModel;
    internal readonly GridView RomGrid;
}