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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for LogoSplashWindow.xaml
    /// </summary>
    public partial class LogoSplashWindow : Window
    {
        public LogoSplashWindow()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                // Small delay ensures window is fully initialized
                await Task.Delay(50);
                var fadeIn = (Storyboard)this.Resources["FadeInStoryboard"];
                fadeIn.Begin(this);
            };
        }

        public Task FadeOutAndCloseAsync()
        {
            var tcs = new TaskCompletionSource<object>();

            Dispatcher.Invoke(() =>
            {
                var fadeOut = (Storyboard)this.Resources["FadeOutStoryboard"];
                fadeOut.Completed += (s, e) =>
                {
                    this.Close();
                    tcs.SetResult(null);
                };
                fadeOut.Begin(this);
            });

            return tcs.Task;
        }
    }

}
