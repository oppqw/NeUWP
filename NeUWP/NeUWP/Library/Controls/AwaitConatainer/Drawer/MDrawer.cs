using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using NeUWP.Controls.Extense;

using NeUWP.Utilities;
using Windows.UI;


// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NeUWP.Controls
{
    public interface IHideAble
    {
        Task<bool> Hide();
    }
    public interface IDrawerContent
    {
        IHideAble Drawer { set; get; }
    }
    public sealed class MDrawer : ContentControl, IControlAwait, IHideAble
    {
        public enum Mode { Right,Bottom}

        private Mode _mode { set; get; }

        public MDrawer(Mode mode)
        {
            _mode = mode;
            if (_mode == Mode.Right)
                this.DefaultStyleKey = typeof(MDrawer);
            else
                this.Style = App.Current.Resources["BottomDrawer"] as Style;
            DataContext = null;
        }

        public static readonly DependencyProperty PRangeProperty = DependencyProperty.Register("PRange", typeof(double), typeof(MDrawer), new PropertyMetadata(double.NaN));

        public double PRange
        {
            get { return (double)GetValue(PRangeProperty); }
            set { SetValue(PRangeProperty, value); }
        }


        public AwaitableContainer AwaitableContainer
        {
            set; get;
        }

        FrameworkElement _container = null;
        Grid _root = null;
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            VisualStateManager.GoToState(this, "Normal", false);
            var _trans = this.GetTemplateChild("translate") as TranslateTransform;
            _container = this.GetTemplateChild("Container") as FrameworkElement;
            _container.Tapped += _container_Tapped;
            _container.PointerPressed += _container_PointerPressed;
            
                _root= base.GetTemplateChild("root") as Grid;
            if (_isTapDismiss)
            {
                if (_root != null)
                {
                    if (Background == null)
                        Background = new SolidColorBrush(Colors.Transparent);
                    _root.ManipulationMode = ManipulationModes.All;
                    _root.Tapped += _container_Tapped;
                    _root.PointerPressed += _container_PointerPressed;
                }
            }
            else
            {
                _root.Background = null;
                Background = null;
            }

            var _closeSb = this.GetTemplateChild("PopupCloseStoryboard") as Storyboard;
            if (_closeSb != null)
            {
                _closeSb.Completed += _closeSb_Completed;
            }

            this.Opacity = 0.0;

            DispatcherUtil.Run(Dispatcher, () =>
            {
                if (double.IsNaN(PRange))
                {
                    PRange = _mode == Mode.Bottom ? _container.ActualHeight : _container.ActualWidth;
                }
                InitTrans(_trans);

                this.Opacity = 1.0;

                var content = Content as FrameworkElement;
                if (content.Opacity == 0.0)
                {
                    content.Opacity = 1.0;
                }

                VisualStateManager.GoToState(this, "Open", true);
            }, Windows.UI.Core.CoreDispatcherPriority.High);
        }

        private void InitTrans(TranslateTransform _trans) {
            if (_trans == null)
                return;
            switch (_mode)
            {
                case Mode.Right:
                    _trans.X = PRange;
                    break;
                case Mode.Bottom:
                    _trans.Y = PRange;
                    break;
            }
        }


        private void _closeSb_Completed(object sender, object e)
        {
            OnClosed();
        }

        private void _container_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isTapDismiss && sender == _root)
                Hide();
            e.Handled = true;
        }

        private void _container_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isTapDismiss && sender == _root)
                Hide();
            e.Handled = true;
        }

        private static MDrawer _current = null;

        public static MDrawer Current { get { return _current; } }

        public static object _lockObj = new object();
        public static  async Task TryCloseCurrent()
        {
            MDrawer drawer = null;
            lock (_lockObj)
            {
                drawer = _current;
                if (drawer == null||drawer.Parent==null)
                {
                    return;
                }
                _current = null;
            }
            await  drawer.Hide();

        }

        public static  Task ShowAuto(IDrawerContent childView, bool isTapDismiss = false, Mode mode = Mode.Right)
        {
           return Show(childView,double.NaN, isTapDismiss, mode);
        }

        private bool _isTapDismiss = false;
        public static async Task Show(IDrawerContent childView,double pRange=315.0,bool isTapDismiss=false ,Mode mode =Mode.Right)
        {
            TryCloseCurrent();

            MDrawer _view = new MDrawer(mode) { Content = childView ,PRange=pRange, _isTapDismiss =isTapDismiss};
            childView.Drawer = _view;
            _current = _view;
            await AwaitableContainer.Show(_view, false);
            if (childView is FrameworkElement) {
                (childView as FrameworkElement).DataContext = null;
            }
            if(_current==_view)
                _current = null;
            _view.Content = null;
            childView.Drawer = null;
        }

        private TaskCompletionSource<bool> _hideTask = null;
        public Task<bool> Hide()
        {
            if (_hideTask != null)
            {
                _hideTask.SetResult(false);
                _hideTask = null;
            }
            _hideTask = new TaskCompletionSource<bool>();
            VisualStateManager.GoToState(this, "Hide", true);
            return _hideTask.Task;
        }

        private void OnClosed()
        {
            if (AwaitableContainer != null)
            {
                AwaitableContainer.SetResult(null);
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                var _parent = this.Parent as Grid;
                if (_parent != null)
                    _parent.Children.Remove(this);
            }
           
            if (_hideTask != null)
            {
                _hideTask.SetResult(true);
                _hideTask = null;
            }
            if (OnHided != null)
                OnHided();
        }
        public event Action OnHided;
    }
}
