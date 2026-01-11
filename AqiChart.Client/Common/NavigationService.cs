using Caliburn.Micro;
using System;

namespace AqiChart.Client.Common
{
    public class CustomNavigationService : INavigationService
    {
        public bool CanGoBack => throw new NotImplementedException();

        public bool CanGoForward => throw new NotImplementedException();

        public object CurrentContent => throw new NotImplementedException();

        public event System.Windows.Navigation.NavigatedEventHandler Navigated;
        public event System.Windows.Navigation.NavigatingCancelEventHandler Navigating;
        public event System.Windows.Navigation.NavigationFailedEventHandler NavigationFailed;
        public event System.Windows.Navigation.NavigationStoppedEventHandler NavigationStopped;
        public event System.Windows.Navigation.FragmentNavigationEventHandler FragmentNavigation;

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void GoForward()
        {
            throw new NotImplementedException();
        }

        public void NavigateToViewModel(Type viewModel, object extraData = null)
        {
            throw new NotImplementedException();
        }

        public void NavigateToViewModel<TViewModel>(object extraData = null)
        {
            throw new NotImplementedException();
        }

        public System.Windows.Navigation.JournalEntry RemoveBackEntry()
        {
            throw new NotImplementedException();
        }

        public void StopLoading()
        {
            throw new NotImplementedException();
        }
    }
}
