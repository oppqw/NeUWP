using NeUWP.Utilities;
using NeUWP.ViewModels;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NeUWP.Controls
{
    public class XListView : ListView, IDeferUpdate
    {
        public event EventHandler<RoutedEventArgs> LoadItems;
        public event EventHandler<RoutedEventArgs> LoadMoreItems;

        private ScrollViewer scrollViewer { get; set; }

        public XListView()
        { 
            DefaultStyleKey = typeof(XListView);

            Loaded += ControlLoaded;
            Unloaded += ControlUnloaded;
            SizeChanged += XListView_SizeChanged;
        }

        public double MinHorizonPadding { set; get; }
        public double MaxContentWidth { set; get; }
        public bool IsAutoPadding { set; get; }

        private bool initialized = false;

        private void XListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsAutoPadding)
            {
                if (!initialized)
                {
                    initialized = true;

                    OnUpdate();
                }
                else
                {
                    DeferUpdateHelper.Instance.Register(this);
                }
            }
        }

        public void OnUpdate()
        {
            UpdatePadding();
        }

        private void UpdatePadding()
        {
            var hPadding = MinHorizonPadding;
            if (2 * hPadding + MaxContentWidth < this.ActualWidth)
            {
                hPadding = (this.ActualWidth - MaxContentWidth) / 2;
            }
            Padding = new Thickness(hPadding, Padding.Top, hPadding, Padding.Bottom);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (DevUtil.DeviceFamily != DeviceFamilyType.Mobile)
            {
                var itemViewModel = item as IAlternateBackgroundConverter;
                if (itemViewModel != null)
                {
                    var listViewItem = element as ListViewItem;
                    if (listViewItem != null)
                    {
                        listViewItem.SetBinding(
                            ListViewItem.BackgroundProperty,
                            new Binding()
                            {
                                Path = new PropertyPath("Position"),
                                Source = item,
                                Converter = new ListViewBackgroundConverter(),
                                Mode = BindingMode.OneWay
                            });
                    }
                }
            }
        }

        #region Load More
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            RefreshIcon = GetTemplateChild("RefreshIcon") as FrameworkElement;
            RefreshingIcon = GetTemplateChild("RefreshingIcon") as FrameworkElement;
            RefreshSymble = GetTemplateChild("RefreshSymble") as FrameworkElement;
            if (RefreshIcon != null && IsPullRefresh)
                RefreshIcon.Visibility = Visibility.Visible;
            HookEvents();
        }

        public void ScrollChangeView(double verticalOffset)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ChangeView(null, verticalOffset, null, true);
            }
        }

        private void ControlLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HookEvents();
        }

        private void ControlUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UnhookEvents();

            initialized = false;
        }

        private object hookLocker = new object();
        private bool hooked { get; set; }

        private void HookEvents()
        {
            lock (hookLocker)
            {
                if (scrollViewer != null)
                {
                    if (!hooked)
                    {
                        hooked = true;

                        scrollViewer.ViewChanged += ViewChanged;
                        if (IsPullRefresh)
                        {
                            AddPullRefreshEvents();
                        }
                    }
                   
                }
            }
        }
        public bool IsPullRefresh { set; get; }
        private void UnhookEvents()
        {
            lock (hookLocker)
            {
                if (scrollViewer != null)
                {
                    scrollViewer.ViewChanged -= ViewChanged;
                    if (IsPullRefresh)
                    {
                        RemovePullRefreshEvents();
                    }
                }

                hooked = false;
            }
        }

        private void ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var loadMore = DataContext as ISupportLoadMore;

            if (LoadItems == null
                && LoadMoreItems == null
                && loadMore == null)
            {
                return;
            }

            var sv = sender as ScrollViewer;
            if (sv != null)
            {
                if (Items != null
                    && Items.Count > 0)
                {
                    if (sv.VerticalOffset > 0.0
                        && sv.ScrollableHeight == sv.VerticalOffset)
                    {
                        if (loadMore != null)
                        {
                            loadMore.LoadMore();
                        }
                        else
                        {
                            var handler = LoadMoreItems;
                            if (handler != null)
                            {
                                handler(this, null);
                            }
                        }
                    }
                    else if (sv.VerticalOffset <= 0.0
                        && sv.ScrollableHeight > sv.VerticalOffset)
                    {
                        var handler = LoadItems;
                        if (handler != null)
                        {
                            handler(this, null);
                        }
                    }
                }
            }
        }
        #endregion

        #region pull refresh

        CompositionPropertySet _scrollerViewerManipulation;
        ExpressionAnimation _rotationAnimation, _opacityAnimation, _offsetAnimation;

        Visual _refreshIconVisual;
        Visual _refreshSymbleVisual;
        float _refreshIconOffsetY;
        const float REFRESH_ICON_MAX_OFFSET_Y = 60.0f;

        bool _refresh;
        FrameworkElement RefreshIcon = null;
        FrameworkElement RefreshingIcon = null;
        FrameworkElement RefreshSymble = null;
        DateTime _pulledDownTime, _restoredTime;
        private void AddPullRefreshEvents() {
            scrollViewer.DirectManipulationStarted += OnDirectManipulationStarted;
            scrollViewer.DirectManipulationCompleted += OnDirectManipulationCompleted;
            _scrollerViewerManipulation = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            var compositor = _scrollerViewerManipulation.Compositor;
            _rotationAnimation = compositor.CreateExpressionAnimation("min(max(0, ScrollManipulation.Translation.Y) * Multiplier, MaxDegree)");
            _rotationAnimation.SetScalarParameter("Multiplier", 10.0f);
            _rotationAnimation.SetScalarParameter("MaxDegree", 400.0f);
            _rotationAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);

            _opacityAnimation = compositor.CreateExpressionAnimation("min(max(0, ScrollManipulation.Translation.Y) / Divider, 1)");
            _opacityAnimation.SetScalarParameter("Divider", 30.0f);
            _opacityAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);
             
            _offsetAnimation = compositor.CreateExpressionAnimation("(min(max(0, ScrollManipulation.Translation.Y) / Divider, 1)) * MaxOffsetY");
            _offsetAnimation.SetScalarParameter("Divider", 30.0f);
            _offsetAnimation.SetScalarParameter("MaxOffsetY", REFRESH_ICON_MAX_OFFSET_Y);
            _offsetAnimation.SetReferenceParameter("ScrollManipulation", _scrollerViewerManipulation);

            _refreshIconVisual = ElementCompositionPreview.GetElementVisual(RefreshIcon);
            _refreshIconVisual.CenterPoint = new Vector3(Convert.ToSingle(RefreshIcon.Width / 2), Convert.ToSingle(RefreshIcon.Height / 2), 0);

            _refreshSymbleVisual = ElementCompositionPreview.GetElementVisual(RefreshSymble);
            _refreshSymbleVisual.CenterPoint = new Vector3(Convert.ToSingle(RefreshSymble.Width / 2), Convert.ToSingle(RefreshSymble.Height / 2), 0);
            // Kick off the animations.
            OnRefreshComplete();

        }
        private void RemovePullRefreshEvents() {
            scrollViewer.DirectManipulationStarted -= OnDirectManipulationStarted;
            scrollViewer.DirectManipulationCompleted -= OnDirectManipulationCompleted;
        }
        void OnDirectManipulationStarted(object sender, object e)
        {
            isStart = true;
            if (isRefreshing)
                return;
            OnRefreshComplete();
            Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
            _refresh = false;
        }

        bool isRefreshing = false;
        bool isStart = false;
       async void OnDirectManipulationCompleted(object sender, object e)
        {
            Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
            var cancelled = (_restoredTime - _pulledDownTime) > TimeSpan.FromMilliseconds(250);

            if (_refresh)
            {
                _refresh = false;

                if (cancelled)
                {
                    Debug.WriteLine("Refresh cancelled...");
                }
                else {
                    if (OnRefresh == null && !(this.DataContext is IPullRefresh))
                        return;
                    Debug.WriteLine("Refresh now!!!");
                    isRefreshing = true;
                    OnPrepareRefresh();
                    RefreshingIcon.Visibility = Visibility.Visible;
                  
                    if (OnRefresh != null)
                        await OnRefresh();
                    else
                    {
                        var data = this.DataContext as IPullRefresh;
                        await data.Onrefresh();
                    }
                    RefreshingIcon.Visibility = Visibility.Collapsed;
                    if (!isStart)
                    {
                        OnRefreshComplete();
                    }
                    isRefreshing = false;
                }
            }
        }

        bool isSetAni = false;
        private void OnPrepareRefresh() {
            if (!isSetAni)
                return;
            isSetAni = false;
            _refreshSymbleVisual.StopAnimation("RotationAngleInDegrees");
            _refreshIconVisual.StopAnimation("Opacity");
            _refreshIconVisual.StopAnimation("Offset.Y");
        }
        private void OnRefreshComplete() {
            if (isSetAni)
                return;
            isSetAni = true;
            _refreshSymbleVisual.StartAnimation("RotationAngleInDegrees", _rotationAnimation);
            _refreshIconVisual.StartAnimation("Opacity", _opacityAnimation);
            _refreshIconVisual.StartAnimation("Offset.Y", _offsetAnimation);
        }

        void OnCompositionTargetRendering(object sender, object e)
        {
            _refreshIconVisual.StopAnimation("Offset.Y");
            _refreshIconOffsetY = _refreshIconVisual.Offset.Y;
            if (!_refresh)
            {
                _refresh = _refreshIconOffsetY == REFRESH_ICON_MAX_OFFSET_Y;
            }

            if (_refreshIconOffsetY == REFRESH_ICON_MAX_OFFSET_Y)
            {
                _pulledDownTime = DateTime.Now;
            }

            if (_refresh && _refreshIconOffsetY <= 1)
            {
                _restoredTime = DateTime.Now;
            }
            _refreshIconVisual.StartAnimation("Offset.Y", _offsetAnimation);
        }
        #endregion
        public event Func<Task> OnRefresh;
    }

    public interface IPullRefresh
    {
        Task Onrefresh();
    }
    public interface IAlternateBackgroundConverter
    {
        int Position { get; set; }
    }
    public class ListViewBackgroundConverter : IValueConverter
    {
        public static Brush OddRowBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x07, 0x1F, 0x21, 0x33));
        public static Brush EvenRowBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                var position = (int)value;
                if (position >= 0 && position % 2 == 0)
                {
                    return OddRowBackgroundBrush;
                }
            }
            return EvenRowBackgroundBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
