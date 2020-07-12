using System;
using System.Windows;
using System.Windows.Controls;

namespace PingThings.CustomControls
{
    /// <summary>
    /// Interaction logic for PingThingControl.xaml
    /// </summary>
    public partial class PingThingControl : UserControl
    {
        public PingThingControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(PingThingControl), new PropertyMetadata(String.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty HostProperty =
            DependencyProperty.Register("Host", typeof(string), typeof(PingThingControl), new PropertyMetadata(String.Empty));

        public string Host
        {
            get => (string)GetValue(HostProperty);
            set => SetValue(HostProperty, value);
        }

        public static readonly DependencyProperty TotalSentProperty =
            DependencyProperty.Register("TotalSent", typeof(string), typeof(PingThingControl), new PropertyMetadata(String.Empty));

        public string TotalSent
        {
            get => (string)GetValue(TotalSentProperty);
            set => SetValue(TotalSentProperty, value);
        }

        public static readonly DependencyProperty TotalRepliesProperty =
            DependencyProperty.Register("TotalReplies", typeof(string), typeof(PingThingControl), new PropertyMetadata(String.Empty));

        public string TotalReplies
        {
            get => (string)GetValue(TotalRepliesProperty);
            set => SetValue(TotalRepliesProperty, value);
        }

        public static readonly DependencyProperty TotalFailedProperty =
            DependencyProperty.Register("TotalFailed", typeof(string), typeof(PingThingControl), new PropertyMetadata(String.Empty));

        public string TotalFailed
        {
            get => (string)GetValue(TotalFailedProperty);
            set => SetValue(TotalFailedProperty, value);
        }
    }
}
