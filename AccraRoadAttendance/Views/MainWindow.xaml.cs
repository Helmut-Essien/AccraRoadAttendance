using AccraRoadAttendance.Data;
using AccraRoadAttendance.Views.Pages.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public readonly IServiceProvider _serviceProvider;
        private AttendanceDbContext _context;
        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            // Initialize context here, outside of any using block
            _context = serviceProvider.GetRequiredService<AttendanceDbContext>();
            // Optionally set default content
            // MainContent.Content = _serviceProvider.GetRequiredService<Dashboard>();

        }

        // Add a method to navigate to AddMembers
        public void NavigateToAddMembers()
        {
            var addMembersView = new AddMembers(_context);
            MainContent.Content = addMembersView;
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var viewName = button.CommandParameter as string;

            if (viewName != null)
            {
                switch (viewName)
                {
                    case "Members":
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
                            var membersView = new Members(context, this);
                            MainContent.Content = membersView;
                        }
                        break;
                        // Add other cases for other views here
                }
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
