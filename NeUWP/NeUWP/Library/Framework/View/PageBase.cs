using NeUWP.Controls;
using NeUWP.Controls.Extense;
using NeUWP.Frameworks;
using NeUWP.Utilities;

using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NeUWP.Views
{
    public abstract class PageBase : Page, IPageExtensions, ICleanup
    {
        protected virtual bool ShowPlayActionBar
        {
            get { return true; }
        }

        public PageBase()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("????? {0} ctor.", GetType().FullName));
#endif
            switch (DevUtil.DeviceFamily)
            {
                case DeviceFamilyType.Mobile:
                    NavigationCacheMode = NavigationCacheMode.Enabled;
                    break;
                default:
                    NavigationCacheMode = NavigationCacheMode.Disabled;
                    break;
            }
        }

#if DEBUG
        ~PageBase()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("????? {0} cctor release.", GetType().FullName));
        }
#endif

        protected virtual void OnStart()
        {
        }

        protected virtual void OnResumed()
        {
        }

        protected virtual void OnPaused()
        {
        }

        protected virtual void OnStopped()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                case NavigationMode.Refresh:
                    OnStart();
                    break;
                case NavigationMode.Forward:
                    break;
                case NavigationMode.Back:
                    OnResumed();
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            switch (e.NavigationMode)
            {
                case NavigationMode.New:
                case NavigationMode.Refresh:
                case NavigationMode.Forward:
                    OnPaused();
                    break;
                case NavigationMode.Back:
                    OnStopped();
                    break;
            }
        }

        public IViewModelPage PageViewModel { get; private set; }

        protected virtual IViewModelPage CreateViewModel()
        {
            return null;
        }

        protected virtual void OnBackRequested(BackRequestedEventArgs e)
        {
        }

        IViewModelPage IPageExtensions.CreateViewModel()
        {
            return CreateViewModel();
        }

        void IPageExtensions.OnBackRequested(BackRequestedEventArgs e)
        {
            OnBackRequested(e);
        }

        public virtual void Cleanup()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("##### {0} cleanup.", GetType().FullName));
#endif
        }

        private FrameworkElement _progress = null;
        public bool IsProgressShowing {
            get { return _progress != null; }
        }

        public bool IsShowProgress
        {
            get { return (bool)GetValue(IsShowProgressProperty); }
            set { SetValue(IsShowProgressProperty, value); }
        }

        public static readonly DependencyProperty IsShowProgressProperty =
            DependencyProperty.Register(
                "IsShowProgress",
                typeof(bool),
                typeof(PageBase),
                new PropertyMetadata(false, OnIsShowProgressPropertyChanged));

        private static void OnIsShowProgressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PageBase)d;
            if (control != null)
            {
               var _isShow = (bool) e.NewValue;
                if (_isShow)
                    control.ShowProgress();
                else
                    control.HideProgress();
            }
        }


        public void ShowProgress() {
            if (_progress != null)
                return;
            DispatcherUtil.Run(() =>
            {
                var _child = (App.Current.Resources["ProgressTemplate"] as DataTemplate).LoadContent() as FrameworkElement;
                var _container = this.GetFirstDescendantOfType<Grid>();
                if (_container != null && _child != null && _child.Parent == null)
                {
                    lock (this) {
                        _container.Children.Add(_child);
                        _progress = _child;
                    }
                    if (_container.RowDefinitions.Count > 0)
                        _child.SetValue(Grid.RowSpanProperty, _container.RowDefinitions.Count);
                    if (_container.ColumnDefinitions.Count > 0)
                        _child.SetValue(Grid.ColumnSpanProperty, _container.ColumnDefinitions.Count);
                }
            });
        }
        public void HideProgress() {
            DispatcherUtil.Run(() =>
            {
                lock (this)
                {
                    if(_progress!=null&& _progress.Parent != null)
                    {
                        (_progress.Parent as Grid).Children.Remove(_progress);
                    }
                    _progress = null;
                }
            });
        }
    }

    public class BackContainerHolder : IBackContainerInterface
    {
        private FrameworkElement _context;
        public BackContainerHolder(FrameworkElement context)
        {
            _context = context;
        }

        private List<IBackAble> _backs = new List<IBackAble>();

        private void UpdateBackButton()
        {
            // NavigationHelper.UpdateAppNavigationButtonState();
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (IBackAble _view in _backs)
            {
                bool isEnableClose = true;
                if (e.NavigationMode != NavigationMode.Back && (_view is ICorssPageAble))
                {
                    isEnableClose = !(_view as ICorssPageAble).IsCanCross;
                }
                if (_view.IsOpen && isEnableClose)
                    _view.Close();
            }

            lock (_lockObj)
            {
                _backs.Clear();
            }
        }

        public bool IsHasBackAbleOpened
        {
            get
            {
                return _backs != null && _backs.FirstOrDefault(b => b.IsOpen && !((b is IIgnoreBack) && (b as IIgnoreBack).IsIgnoreBack)) != null;
            }
        }


        Grid _container = null;

        private void FindContainer()
        {
            _container = null;
            if (_context == null)
                return;
            if (_context is ContentControl)
            {
                _container = (_context as ContentControl).Content as Grid;
            }
            if (_container == null)
            {
                if (_context is DependencyObject)
                {
                    _container = (_context as DependencyObject).GetFirstDescendantOfType<Grid>();
                }
            }
        }

        public void OpenBackAble(IBackAble _back)
        {
            if (_back != null)
            {
                lock (_lockObj)
                {
                    if (!_backs.Contains(_back))
                    {
                        _backs.Add(_back);
                    }
                }
                if (_container == null)
                    FindContainer();
                if (_container == null)
                    return;
                var _child = _back as FrameworkElement;
                if (_container != null && _child != null && _child.Parent == null)
                {
                    _container.Children.Add(_child);
                    if (_container.RowDefinitions.Count > 0)
                        _child.SetValue(Grid.RowSpanProperty, _container.RowDefinitions.Count);
                    if (_container.ColumnDefinitions.Count > 0)
                        _child.SetValue(Grid.ColumnSpanProperty, _container.ColumnDefinitions.Count);
                }

                if (!_back.IsOpen)
                {
                    _back.Open();
                }
            }

            UpdateBackButton();
        }

        public bool IsOpen
        {
            get
            {
                return true;
            }
        }

        public void CloseBackAble(IBackAble _back)
        {
            var _view = _back as FrameworkElement;
            var _parent = _view.Parent as Grid;
            if (_parent != null)
            {
                _parent.Children.Remove(_view);
            }
            lock (_lockObj)
            {
                if (_backs != null && _backs.Contains(_back))
                {
                    _backs.Remove(_back);
                }
            }
            UpdateBackButton();
        }

        private object _lockObj = new object();
        public virtual void Close()
        {
            lock (_lockObj)
            {
                for (int i = _backs.Count - 1; i >= 0; i--)
                {
                    var _back = _backs[i];
                    if (_back.IsOpen&&(!(_back is IIgnoreBack)||!(_back as IIgnoreBack).IsIgnoreBack))
                    {
                        _back.Close();
                        if (_backs.Contains(_back))
                            _backs.Remove(_back);
                        UpdateBackButton();
                        return;
                    }
                }
            }
            UpdateBackButton();
        }
    }

    public interface IPageExtensions
    {
        IViewModelPage PageViewModel { get; }

        IViewModelPage CreateViewModel();

        void OnBackRequested(BackRequestedEventArgs e);
    }

    public interface ICleanup
    {
        /// <summary>
        /// Cleans up the instance, for example by saving its state,
        /// removing resources, etc...
        /// </summary>
        void Cleanup();
    }
}
