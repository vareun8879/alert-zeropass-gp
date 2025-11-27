using System.Windows;
using System.Windows.Controls;
using ZeroPassAlert.LongPolling;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.Overlays
{
    public partial class AlertOverlay : UserControl
    {
        public AlertOverlay(VisitorEventVO evt)
        {
            InitializeComponent();
            this.DataContext = evt; // 바인딩 연결
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = this.Parent as Panel;
            parent?.Children.Remove(this);
        }
    }
}