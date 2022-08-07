using Eto.Drawing;
using Eto.Forms;

namespace M64RPFW.Views;

public class RecentROMPanel : Panel
{
    public RecentROMPanel()
    {
        Grid = new GridView();
        {
            Grid.Columns.Add(new GridColumn
            {
                HeaderText = "Name"
            });
            Grid.Columns.Add(new GridColumn
            {
                HeaderText = "Region"
            });
            Grid.Columns.Add(new GridColumn
            {
                HeaderText = "Filename"
            });
        }

        Content = Grid;
        Padding = Padding.Empty;
    }

    internal readonly GridView Grid;
}