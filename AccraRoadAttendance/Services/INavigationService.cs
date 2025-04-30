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

    public class NavigationService : INavigationService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceScope? _currentScope;
        private ContentControl? _mainContent;

        public NavigationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void SetContentFrame(ContentControl frame)
        {
            _mainContent = frame;
        }

        public void NavigateTo<T>() where T : UserControl
        {
            EnsureFrame();
            DisposeCurrentScope();

            _currentScope = _scopeFactory.CreateScope();
            var page = _currentScope.ServiceProvider.GetRequiredService<T>();
            _mainContent!.Content = page;
        }

        public void NavigateTo<T>(object parameter) where T : UserControl
        {
            EnsureFrame();
            DisposeCurrentScope();

            _currentScope = _scopeFactory.CreateScope();
            var page = _currentScope.ServiceProvider.GetRequiredService<T>();
            if (page is IParameterReceiver receiver)
                receiver.ReceiveParameter(parameter);

            _mainContent!.Content = page;
        }

        private void EnsureFrame()
        {
            if (_mainContent == null)
                throw new InvalidOperationException(
                    "You must call SetContentFrame(...) before navigating.");
        }

        private void DisposeCurrentScope()
        {
            _currentScope?.Dispose();
            _currentScope = null;
        }

        public void Dispose()
        {
            DisposeCurrentScope();
        }
    }

    public interface IParameterReceiver
    {
        void ReceiveParameter(object parameter);
    }
}
