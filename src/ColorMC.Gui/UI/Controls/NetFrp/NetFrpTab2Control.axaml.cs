using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpTab2Control : UserControl
{
    public NetFrpTab2Control()
    {
        InitializeComponent();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is NetFrpModel model && model.NowView == 1)
        {
            if (e.Delta.Y < 0)
            {
                model.NowView++;
            }
            else if (e.Delta.Y > 0)
            {
                model.NowView--;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged -= ScrollViewer1_PointerWheelChanged;
    }
}