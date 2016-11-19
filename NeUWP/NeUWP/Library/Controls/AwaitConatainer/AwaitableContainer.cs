using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using NeUWP.Views;
using NeUWP.Controls.Extense;
using System.Linq;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NeUWP.Controls
{
    public sealed class AwaitableContainer : ContentControl, IBackAble, ICorssPageAble, IIgnoreBack
    {
        TaskCompletionSource<object> _task;
        public AwaitableContainer()
        {
            this.DefaultStyleKey = typeof(AwaitableContainer);
            this.Visibility = Visibility.Collapsed;
            this.Unloaded += AwaitableContainer_Unloaded;
            _task = new TaskCompletionSource<object>();
        }

        public static Task<object> Show(IControlAwait view, bool isIgnoreBack=false, IBackContainerInterface page =null)
        {
            return Show(view, false, isIgnoreBack, null,null, page);
        }

        public static Task<object> Show(IControlAwait view, bool isCrossPage,bool isIgnoreBack,Action<AwaitableContainer> onOpened,Action<AwaitableContainer> onClosed , IBackContainerInterface page =null)
        {
            if (page == null)
                page = Window.Current.Content  as IBackContainerInterface;
            if (view == null)
                return Task.FromResult<object>(null);
            var _container = new AwaitableContainer() { Content = view, IsCanCross = isCrossPage ,IsIgnoreBack=isIgnoreBack};
            if (onOpened != null)
                _container.OnOpened += onOpened;
            if (onClosed != null)
                _container.OnClosed += onClosed;
            view.AwaitableContainer = _container;
            page.OpenBackAble(_container);
            return _container.WaitDataAsync();
        }

        private void AwaitableContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            OnResult(null);
        }


        Grid _root;
        ScaleTransform _scale = null;
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContentPresenter _presenter = GetTemplateChild("ContentPresenter") as ContentPresenter;
            _root = GetTemplateChild("root") as Grid;
            _scale = GetTemplateChild("scale") as ScaleTransform;
            if (_prepareTask != null)
                _prepareTask.SetResult(true);
        }

        private void _container_Closed(object sender, object e)
        {
            OnResult(null);
        }

        public void SetResult(object data)
        {
            OnResult(data);
            Close();
        }

        private void OnResult(object result)
        {
            if (_task != null)
                _task.SetResult(result);
            _task = null;
        }

        private TaskCompletionSource<object> _prepareTask = new TaskCompletionSource<object>();
        public Task<object> WaitDataAsync()
        {
            return _task.Task;
        }

        #region ibackable
        public bool IsOpen
        {
            get
            {
                return this.Visibility == Visibility.Visible;
            }
        }

        public bool IsCanCross
        {
            set;
            get;
        }

        public bool IsIgnoreBack
        {
            set; get;
        }

        private void DisConnectParent() {
            if (this.Content is IControlAwait)
            {
                (this.Content as IControlAwait).AwaitableContainer = null;
                this.Content = null;
            }
            var _page = this.GetAncestors().SingleOrDefault(s=>s is IBackContainerInterface) as IBackContainerInterface;
            if (_page == null)
            {
                var _parent = this.Parent as Grid;
                if (_parent != null)
                {
                    _parent.Children.Remove(this);
                }
            }
            else
            {
                _page.CloseBackAble(this);
            }
        }


        public async void Close()
        {
            if (this.Content is IHideAble) {
                var _drawer = this.Content as IHideAble;
                await  _drawer.Hide();
            }
            this.Visibility = Visibility.Collapsed;
            DisConnectParent();
            if (OnClosed != null)
                OnClosed(this);
            OnResult(null);
        }

        public Action<AwaitableContainer> OnClosed;
        public Action<AwaitableContainer> OnOpened;
        public async void Open()
        {
            this.Visibility = Visibility.Visible;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            if (OnOpened != null)
                OnOpened(this);
            if (_prepareTask != null)
                await _prepareTask.Task;
            _prepareTask = null;
        }
        #endregion
    }
}
