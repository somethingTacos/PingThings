using System.Windows;
using System.Windows.Controls;

namespace PingThings.CustomControls
{
    /// <summary>
    /// Interaction logic for AddPingDialog.xaml
    /// </summary>
    public partial class CenteredDialogControl : UserControl
    {
        public CenteredDialogControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DialogProperty =
            DependencyProperty.Register("Dialog", typeof(object), typeof(CenteredDialogControl));

        public object Dialog
        {
            get => (object)GetValue(DialogProperty);
            set => SetValue(DialogProperty, value);
        }
    }
}
