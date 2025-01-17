using System.Configuration;
using System.Data;
using System.Windows;
using AccraRoadAttendance.Data; // Assuming this is the namespace for AttendanceContext

namespace AccraRoadAttendance
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure the database is created when the application starts
            using (var context = new AttendanceContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}