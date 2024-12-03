using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool startDrawRect = false;

        private System.Windows.Point sPoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBoxItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem) { myListBox.SelectedIndex = myListBox.ItemContainerGenerator.IndexFromContainer(listBoxItem); }
        }

        private void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            sPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            startDrawRect = true;
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            if (!startDrawRect)
            {
                return;
            }

            var ePoint = e.GetPosition((System.Windows.Controls.Image)sender);

            double left;
            double top;
            if (sPoint.X < ePoint.X)
            {
                left = sPoint.X;
            }
            else
            {
                left = ePoint.X;
            }

            if (sPoint.Y < ePoint.Y)
            {
                top = sPoint.Y;
            }
            else
            {
                top = ePoint.Y;
            }

            rectangle.SetValue(Canvas.LeftProperty, left);
            rectangle.SetValue(Canvas.TopProperty, top);
            rectangle.Width = Math.Abs(sPoint.X - ePoint.X);
            rectangle.Height = Math.Abs(sPoint.Y - ePoint.Y);
            rectangle.Stroke = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.75f };
            rectangle.StrokeThickness = 2;
        }

        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startDrawRect = false;

            var vm = (MainWindowVM)this.DataContext;

            vm.SendActualWidthAndActualHeight(img.ActualWidth, img.ActualHeight);
        }
    }
}
