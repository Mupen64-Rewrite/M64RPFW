using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Models;
using M64RPFW.Presenters;

using static M64RPFW.Views.Helpers.FormCommandHelper;

namespace M64RPFW.Views;

internal class RecentRomView : Panel
{
    public RecentRomView(MainView parent)
    {
        DataContext = new RecentRomPresenter(this, parent);

        Content = DoPostInit(new GridView
        {
            // Bind VM's RomObjects as backing store for GridView
            DataStore = Presenter.RecentRoms,
            Columns =
            {
                new GridColumn
                {
                    HeaderText = "Name",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<RomFile, string>(o => o.FriendlyName)
                    },
                    Editable = false
                },
                new GridColumn
                {
                    HeaderText = "Region",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<RomFile, string>(o => o.Region.ToString())
                    },
                    Editable = false
                },
                new GridColumn
                {
                    HeaderText = "Filename",
                    DataCell = new TextBoxCell
                    {
                        Binding = Binding.Property<RomFile, string>(o => o.FileName)
                    },
                    Editable = false
                }
            }
        }, grid =>
        {
            grid.CellDoubleClick += (_, args) => Presenter.SelectAndRunROM(args.Row);
        });
        Padding = Padding.Empty;
    }

    internal RecentRomPresenter Presenter => (RecentRomPresenter) DataContext;
}