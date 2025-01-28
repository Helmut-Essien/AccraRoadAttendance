using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AccraRoadAttendance.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : UserControl;
        void NavigateTo<T>(object parameter) where T : UserControl;
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private ContentControl _mainContent;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SetContentFrame(ContentControl frame)
        {
            _mainContent = frame;
        }

        public void NavigateTo<T>() where T : UserControl
        {
            var page = _serviceProvider.GetRequiredService<T>();
            _mainContent.Content = page;
        }

        public void NavigateTo<T>(object parameter) where T : UserControl
        {
            var page = _serviceProvider.GetRequiredService<T>();

            if (page is IParameterReceiver receiver)
            {
                receiver.ReceiveParameter(parameter);
            }

            _mainContent.Content = page;
        }
    }

    public interface IParameterReceiver
    {
        void ReceiveParameter(object parameter);
    }
}
