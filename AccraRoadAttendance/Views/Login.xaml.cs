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
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;
using AccraRoadAttendance.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public bool IsDarkTheme { get; set; }
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private readonly UserManager<User> _userManager;
        private readonly CurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;
        public Login(UserManager<User> userManager, CurrentUserService currentUserService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _userManager = userManager;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Login Error");
                return;
            }

            bool loginSuccessful = await _currentUserService.LoginAsync(email, password);
            if (loginSuccessful)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Failed");
            }
        }


        private void toggleTheme(object sender, RoutedEventArgs e)
        {
            Theme theme = paletteHelper.GetTheme();

            if (IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark)
            {
                IsDarkTheme = false;
                theme.SetBaseTheme(BaseTheme.Light);
                // Change image for light theme
                logoImage.Source = new BitmapImage(new Uri("pack://application:,,,/AccraRoadAttendance;component/AppImages/CLogoc.png"));
            }
            else
            {
                IsDarkTheme = true;
                theme.SetBaseTheme(BaseTheme.Dark);
                // Change image for light theme
                logoImage.Source = new BitmapImage(new Uri("pack://application:,,,/AccraRoadAttendance;component/AppImages/CLogocw.png"));
            }
            paletteHelper.SetTheme(theme);
        }

        private void exitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
