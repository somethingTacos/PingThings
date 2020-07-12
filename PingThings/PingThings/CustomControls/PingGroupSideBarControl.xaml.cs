using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PingThings.CustomControls
{
    /// <summary>
    /// Interaction logic for PingGroupSideBarControl.xaml
    /// </summary>
    public partial class PingGroupSideBarControl : UserControl
    {
        public PingGroupSideBarControl()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(PingGroupSideBarControl), new PropertyMetadata(String.Empty));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(PingGroupSideBarControl), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(PingGroupSideBarControl), new UIPropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
