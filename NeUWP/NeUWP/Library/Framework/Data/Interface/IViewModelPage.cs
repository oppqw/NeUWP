using Windows.UI.Core;

namespace NeUWP.Frameworks
{
    public interface IViewModelPage
    {
        void OnActivate(CoreDispatcher dispatcher, bool resume, object parameter);

        void OnDeactivate(bool close);
    }
}
