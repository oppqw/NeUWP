using NeUWP.Utilities;
using NeUWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace NeUWP.Controls
{
    public class XGridView : GridView, IDeferUpdate
    {
        public event EventHandler<RoutedEventArgs> LoadMoreItems;

        private ScrollViewer scrollViewer { get; set; }

        public XGridView()
        {
            DefaultStyleKey = typeof(XGridView);

            Loaded += ControlLoaded;
            Unloaded += ControlUnloaded;
            SizeChanged += XListView_SizeChanged;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            if (IsAutoSize)
            {
                var _presenter = GetTemplateChild("Presenter") as ItemsPresenter;
                if (_presenter != null)
                {
                    _presenter.Margin = new Thickness(0, 0, -HorizonMargin, 0);
                }
            }
            HookEvents();
        }

        public void ScrollChangeView(double verticalOffset)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ChangeView(null, verticalOffset, null);
            }
        }

        private void ControlLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HookEvents();
        }

        private void ControlUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            UnhookEvents();
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
                    }
                }
            }
        }

        private void UnhookEvents()
        {
            lock (hookLocker)
            {
                if (scrollViewer != null)
                {
                    scrollViewer.ViewChanged -= ViewChanged;
                }

                hooked = false;
            }
        }

        private void ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var loadMore = DataContext as ISupportLoadMore;

            if (LoadMoreItems == null
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
                }
            }
        }

        #region size adapt
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (IsAutoSize)
            {
                if (item != null && this.Items != null && this.Items.IndexOf(item) == 0)
                {
                    if (ItemsPanelRoot != null)
                    {
                        ItemsPanelRoot.SetBinding(ItemsWrapGrid.ItemWidthProperty, new Binding() { Path = new PropertyPath("ItemWidth"), Source = this });
                        ItemsPanelRoot.SetBinding(ItemsWrapGrid.ItemHeightProperty, new Binding() { Path = new PropertyPath("ItemHeight"), Source = this });
                    }

                    OnUpdate();
                }

                var frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
                {
                    if(isUseItemMargin)
                        frameworkElement.Margin = _itemMargin;
                    else
                        frameworkElement.Margin = new Thickness(0, 0, HorizonMargin, VerticalMargin);
                }
            }

            base.PrepareContainerForItemOverride(element, item);
        }

        private void XListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsAutoSize)
            {
                if (_visibleColumns > 0)
                {
                    DeferUpdateHelper.Instance.Register(this);
                }
                else
                {
                    OnUpdate();
                }
            }
        }

        public void OnUpdate()
        {
            try
            {
                if (IsAutoPadding)
                {
                    UpdatePadding();
                }

                if (ActualWidth > 0)
                {
                    UpdateMaxItemWidth(ActualWidth);
                }
            }
            catch
            {
            }
        }

        private void UpdatePadding()
        {
            var hPadding = MinHorizonPadding;
            if (2 * hPadding + MaxContentWidth < ActualWidth)
            {
                hPadding = (ActualWidth - MaxContentWidth) / 2;
            }
            Padding = new Thickness(hPadding, Padding.Top, hPadding, Padding.Bottom);
        }

        //x:显示可见item列数  W:控件除边距宽  w:item宽 f:item间边距  Min、Max:item最大最小宽度
        //(x-1)f+xw=W  0<Min<=w<=Max  
        //推得:    (W+f)/(f+Max)<=x<=(W+f)/(f+Min) x取最小 w=(W-(x-1)f)/x
        private double _visibleColumns = -1;

        private void UpdateMaxItemWidth(double width)
        {
            if (width <= 0)
                return;
            if ((MaxItemWidth < MinItemWidth || MaxItemWidth == double.MaxValue || width <= 0) && UseConstColumnCount <= 0)// this.ItemsPanelRoot == null || (!(this.ItemsPanelRoot is ItemsWrapGrid) && !(this.ItemsPanelRoot is VariableSizedWrapGrid)) || 
                return;
            width = width - this.Padding.Left - this.Padding.Right;
            double target = UseConstColumnCount;
            if (UseConstColumnCount <= 0)
            {
                double minCount = Math.Ceiling((width + HorizonMargin) / (HorizonMargin + MaxItemWidth));
                int maxCount = MinItemWidth > 0 ? (int)((width + HorizonMargin) / (HorizonMargin + MinItemWidth)) : int.MaxValue;
                target = Math.Min(minCount, maxCount);
            }
            double targetW = (width - (target - 1) * HorizonMargin) / target;
            //if (target > 1)
            targetW += HorizonMargin;
            ItemWidth = Math.Floor(targetW);

            if (ColumnCountChanged != null && _visibleColumns != target)
                ColumnCountChanged(this, (int)target);
            _visibleColumns = target;
        }

        public event Action<object, int> ColumnCountChanged;

        #region property
        public bool IsAutoSize { set; get; }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(XGridView), new PropertyMetadata(0));

        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(XGridView), new PropertyMetadata(double.NaN));

        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        public static readonly DependencyProperty UseConstColumnCountProperty = DependencyProperty.Register("UseConstColumnCount", typeof(int), typeof(XGridView), new PropertyMetadata(-1, new PropertyChangedCallback(OnPropertyChanged)));

        public int UseConstColumnCount
        {
            get { return (int)GetValue(UseConstColumnCountProperty); }
            set { SetValue(UseConstColumnCountProperty, value); }
        }

        public double MinHorizonPadding { set; get; }
        public double MaxContentWidth { set; get; }
        public bool IsAutoPadding { set; get; }

        double _horizonOffset = DevUtil.DeviceFamily==DeviceFamilyType.Mobile?0:20;

        public double HorizonMargin
        {
            set { _horizonOffset = value; }
            get { return _horizonOffset; }
        }

        double _verticalOffset = 28;
        public double VerticalMargin
        {
            set { _verticalOffset = value; }
            get { return _verticalOffset; }
        }

        Thickness _itemMargin;
        private bool isUseItemMargin = false;
        //支持右上下，左不支持
        public Thickness ItemMargin { set { _itemMargin = value; isUseItemMargin = true;
                HorizonMargin = _itemMargin.Right;
            } get { return _itemMargin;
               
            } }

        public double MinItemWidth
        {
            set { SetValue(MinItemWidthProperty, value); }
            get { return (double)GetValue(MinItemWidthProperty); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _gv = d as XGridView;
            if (_gv != null)
            {
                _gv.OnUpdate();
            }
        }

        public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register("MinItemWidth", typeof(double), typeof(XGridView), new PropertyMetadata(0.0, new PropertyChangedCallback(OnPropertyChanged)));

        public double MaxItemWidth
        {
            set { SetValue(MaxItemWidthProperty, value); }
            get { return (double)GetValue(MaxItemWidthProperty); }
        }

        public static readonly DependencyProperty MaxItemWidthProperty = DependencyProperty.Register("MaxItemWidth", typeof(double), typeof(XGridView), new PropertyMetadata(double.MaxValue, new PropertyChangedCallback(OnPropertyChanged)));
        #endregion
        #endregion
    }
}
