using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using NeUWP.Controls.Extense;
using Windows.Foundation;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NeUWP.Controls
{
    public sealed class MDilog : ContentControl, IControlAwait
    {
        public MDilog()
        {
            this.DefaultStyleKey = typeof(MDilog);
        }

        public AwaitableContainer AwaitableContainer
        {
            set; get;
        }


        public void HideWithResult(object value = null)
        {
            OnPrepareClose();
            if (AwaitableContainer != null)
                AwaitableContainer.SetResult(value);
            else if (this.Parent != null)
            {
                this.Visibility = Visibility.Collapsed;
            }

        }

        public void ChangeChildView(FrameworkElement view)
        {
            var _old = this.Content as IDialogChild;
            PrepareDialogChild(view as IDialogChild);
            this.Content = view;
            ClearDialogChild(_old);
        }

        private void OnPrepareClose()
        {

            if (_root != null)
            {
                _root.Tapped -= OnViewTapped;
            }
            if (_childContainer != null)
            {
                _childContainer.Tapped -= OnViewTapped;
            }
            if (_close != null)
            {
                _close.Click -= CloseClick;
                _close = null;
            }
            if (this.Content is IDialogChild)
            {
                ClearDialogChild(Content as IDialogChild);
            }
            this.Content = null;
        }

        private void ClearDialogChild(IDialogChild view)
        {
            if (view == null)
                return;
            view.ParentDialog = null;
        }
        private void PrepareDialogChild(IDialogChild view)
        {
            if (view == null)
                return;
            view.ParentDialog = this;
        }


        public void Hide()
        {
            HideWithResult();
        }

        private bool _isBackgroundTransparent = false;
        private bool _isHasCloseButton = true;
        private bool _isTapDismiss = true;
        private bool _isUseAbsoluatePosition = false;


        private static MDilog PrepareView(FrameworkElement view, bool isBackgroundTransparent, bool isHasCloseButton, bool isTapDismiss)
        {
            MDilog _view = new MDilog() { Content = view, _isBackgroundTransparent = isBackgroundTransparent, _isHasCloseButton = isHasCloseButton, _isTapDismiss = isTapDismiss };
            if (view is IDialogChild)
            {
                _view.PrepareDialogChild(view as IDialogChild);
            }
            return _view;
        }

        public static Task<object> Show(FrameworkElement view, bool isBackgroundTransparent, bool isHasCloseButton, bool isTapDismiss,bool isIgnoreBack=false)
        {
            var _view = PrepareView(view, isBackgroundTransparent, isHasCloseButton, isTapDismiss);
            return AwaitableContainer.Show(_view, isIgnoreBack);
        }

        public static Task<object> ShowFromView(FrameworkElement view, FrameworkElement attachView)
        {
            var _view = PrepareView(view, true, false, true);
            _view._isUseAbsoluatePosition = true;
            _view.Loaded += (sender, e) => {
                var _rect = attachView.GetBoundingRect();
                _view.SetViewPosition(_rect);
            };
            return AwaitableContainer.Show(_view, false);
        }

        public static Task<object> ShowAtPosition(FrameworkElement view, Point position)
        {
            var _view = PrepareView(view, true, false, true);
            _view._isUseAbsoluatePosition = true;
            _view.Loaded += (sender, e) => {
                var _rect = new Rect(position.X, position.Y, 0, 0);
                _view.SetViewPosition(_rect);
            };
            return AwaitableContainer.Show(_view, false);
        }

        private const double ConstOffset = 10;
        private void SetViewPosition(Rect _rect)
        {
            if (_childContainer == null || _childContainer.ActualWidth == 0 || _childContainer.ActualHeight == 0)
            {
                return;
            }
            var _frame = Window.Current.Content as FrameworkElement;
            if (_rect == null || _frame == null)
                return;
            bool isTop = _rect.Top > _frame.ActualHeight - _rect.Bottom;
            double verticalOffset = isTop ? _rect.Top - ConstOffset - _childContainer.ActualHeight : _rect.Bottom + ConstOffset;
            if (verticalOffset < 0)
            {
                verticalOffset = ConstOffset;
            }
            double horizonOffset = 0;
            if (_rect.Width > _childContainer.ActualWidth)
            {
                horizonOffset = _rect.Left + (_rect.Width - _childContainer.ActualWidth) / 2;
            }
            else
            {
                horizonOffset = _rect.Left - (_childContainer.ActualWidth - _rect.Width) / 2;
                if (horizonOffset + _childContainer.ActualWidth > _frame.ActualWidth)
                {
                    horizonOffset = _frame.ActualWidth - ConstOffset - _childContainer.ActualWidth;
                }
                if (horizonOffset < 0)
                    horizonOffset = ConstOffset;
            }
            _childContainer.Margin = new Thickness(horizonOffset, verticalOffset, 0, 0);
        }

        FrameworkElement _root;
        Button _close;
        FrameworkElement _childContainer;
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _root = this.GetTemplateChild("root") as FrameworkElement;
            if (_isTapDismiss || !_isBackgroundTransparent)
                Background = _isBackgroundTransparent ? new SolidColorBrush(Colors.Transparent) : (Brush)App.Current.Resources["DilogBackground"];
            if (_isHasCloseButton)
            {
                var _close = this.GetTemplateChild("close") as Button;
                if (_close != null)
                {
                    _close.Visibility = Visibility.Visible;
                    _close.Click += CloseClick;
                }
            }
            _childContainer = this.GetTemplateChild("ChildContainer") as FrameworkElement;
            if (_isTapDismiss)
            {

                if (_root != null && _childContainer != null)
                {
                    _childContainer.Tapped += OnViewTapped;
                    _root.Tapped += OnViewTapped;
                }
            }
            if (_isUseAbsoluatePosition)
            {
                _childContainer.HorizontalAlignment = HorizontalAlignment.Left;
                _childContainer.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        private void OnViewTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (sender == _root)
            {
                Hide();
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }

    interface IDialogChild
    {
        MDilog ParentDialog { set; get; }
    }
}
