using AccraRoadAttendance.Views.Pages.Members;
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
using System.Windows.Shapes;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set default content (Dashboard)
            //MainContent.Content = new Dashboard();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var viewName = button.CommandParameter as string;

            switch (viewName)
            {
                case "Members":
                    MainContent.Content = new Members();
                    break;
            }
            //    case "MarkAttendance":
            //        MainContent.Content = new MarkAttendance();
            //        break;
            //    case "Reports":
            //        MainContent.Content = new Reports();
            //        break;
            //    case "Settings":
            //        MainContent.Content = new Settings();
            //        break;
            //}
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            // Logic to toggle light/dark theme
        }
    }
}
