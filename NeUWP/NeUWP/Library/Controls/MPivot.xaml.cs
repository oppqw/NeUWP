using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NeUWP.Controls.Extense;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NeUWP.Controls
{
    public sealed partial class MPivot : Pivot, IDeferUpdate
    {
        public MPivot()
        {
            this.InitializeComponent();
        }

        private void Header_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _header = sender as FrameworkElement; 
            DeferUpdateHelper.Instance.Register(this);
        }


        FrameworkElement _mask = null;
        FrameworkElement _header = null;
        CompositeTransform _transform;

        private void Grid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _mask = sender as FrameworkElement;
            _transform = _mask.RenderTransform as CompositeTransform;
            DeferUpdateHelper.Instance.Register(this);
        }

        private void TryInitDefaultHeadState()
        {
            InitHeadItems();
            if (_headItems != null && Items != null)
            {
                if (Items.Count == 0)
                    return;
                var index = SelectedIndex < 0 ? 0 : SelectedIndex;
                if (index >= _headItems.Count)
                    return;
                OnSelectedHeaderSelected(_headItems[index]);
            }
        }


        private void SelectionStates_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("changing");
        }

        private Control _current;
        private Control _pre;
        private Control _next;

        private void OnSelectedHeaderSelected(Control view,bool isForce=false)
        {
            try
            {
                if (view == _current && !isForce)
                    return;
                var _heads = _header.GetDescendantsOfType<PivotHeaderItem>();
                if (_mask == null)
                    return;
                var _rect = view.GetBoundingRect(_header);
                view.Tag = _rect;
                ChangeMask(view.ActualWidth, _rect.X, _current != null);
                if (_mask.Visibility == Visibility.Collapsed)
                    _mask.Visibility = Visibility.Visible;
                _current = view;
            }
            catch { }
        }

        private const double originSize = 100;


        private void ChangeMask(double width, double offset, bool isAnimation = true)
        {
            double targetScaleX = width / originSize;
            _transform.CenterX = 50;
            if (IsDynamicHeaderMode)
                offset = 0;
            double targetX = offset + (width - originSize) / 2;

            if (!isAnimation)
            {
                _transform.TranslateX = targetX;
                _transform.ScaleX = targetScaleX;
                return;
            }
            double time = 0.3;
            Storyboard _sb = new Storyboard();
            DoubleAnimation _ani = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(time), To = targetScaleX };
            Storyboard.SetTarget(_ani, _transform);
            Storyboard.SetTargetProperty(_ani, "ScaleX");
            _sb.Children.Add(_ani);

            _ani = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(time), To = targetX };
            Storyboard.SetTarget(_ani, _transform);
            Storyboard.SetTargetProperty(_ani, "TranslateX");
            _sb.Children.Add(_ani);

            _sb.Completed += _sb_Completed;
            //lock (_lockObj)
            //{
            _isAnimationing = true;
            _sb.Begin();

            //}
        }

        private object _lockObj = new object();
        private bool _isAnimationing = false;

        private void _sb_Completed(object sender, object e)
        {
            Storyboard _sb = sender as Storyboard;
            _sb.Completed -= _sb_Completed;
            //lock (_lockObj)
            //{
            _isAnimationing = false;
            //}
        }

        private void SelectionStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name.Contains("Selected"))
            {
                OnSelectedHeaderSelected(e.Control);
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (_isAnimationing || !isAllowCheckSV)
                return;
            if (e.IsInertial)
            {
                return;
            }
            UpdateMaskDelta();
            System.Diagnostics.Debug.WriteLine("changing");
        }

        private void UpdateMaskDelta()
        {
            if (_isAnimationing)
                return;
            var _item = this.ContainerFromIndex(this.SelectedIndex) as PivotItem;
            if (_item == null || _current == null || _current.Tag == null)
                return;
            var _rect = _item.GetBoundingRect(this);
            var _actor = Math.Min(_rect.X / _rect.Width, 1.0);
            _actor = Math.Max(_actor, -1.0);
            var _currentRect = (Rect)_current.Tag;
            double targetWidth = _currentRect.Width;
            double targetOffset = _currentRect.X;
            if (_rect.X > 0 && _pre != null)
            {
                if (_pre.Tag != null)
                {
                    var _r = (Rect)_pre.Tag;
                    targetWidth = _current.ActualWidth + (_pre.ActualWidth - _current.ActualWidth) * _actor;
                    targetOffset = _currentRect.X + (_r.X - _currentRect.X) * _actor;
                }
            }
            else if (_rect.X < 0 && _next != null)
            {
                if (_next.Tag != null)
                {
                    var _r = (Rect)_next.Tag;
                    targetWidth = _current.ActualWidth - (_next.ActualWidth - _current.ActualWidth) * _actor;
                    targetOffset = _currentRect.X - (_r.X - _currentRect.X) * _actor;
                }
            }
            ChangeMask(targetWidth, targetOffset, false);
        }

        private List<PivotHeaderItem> _headItems = null;
        bool isAllowCheckSV = false;
       public bool IsDisableAutoDivider { set; get; }
       public int ItemDivideCount { set; get; }
        private void InitHeadItems()
        {
            var _items = _header.GetDescendantsOfType<PivotHeaderItem>();
            if (_items == null)
                return;
            _headItems = _items.ToList();
            try
            {
                if (_headItems == null || _headItems.Count == 0)
                    return;
                var width = this.ActualWidth / (ItemDivideCount>0? ItemDivideCount:_headItems.Count);
                foreach (var item in _headItems)
                {
                    item.Tag = item.GetBoundingRect(_header);
                    if(!IsDisableAutoDivider)
                        item.Width = width;
                }
                if (!IsDisableAutoDivider)
                {
                    var _container = _header.GetFirstAncestorOfType<ContentControl>();
                    _header.Width = this.ActualWidth;
                    _container.Clip.Rect = new Rect(0, 0, this.ActualWidth, _container.Clip.Bounds.Height);
                }
                this.Opacity = 1;
            }
            catch { }
        }

        private void ScrollViewer_DirectManipulationStarted(object sender, object e)
        {
            isAllowCheckSV = true;
            if (_headItems == null || _headItems.Count != this.Items.Count)
                InitHeadItems();
            if (_headItems != null && _current != null)
            {
                var _index = _headItems.IndexOf(_current as PivotHeaderItem);
                _next = _index >= _headItems.Count - 1 ? _headItems[0] : _headItems[_index + 1];
                _pre = _index <= 0 ? _headItems.Last() : _headItems[_index - 1];

            }
        }

        private void ScrollViewer_DirectManipulationCompleted(object sender, object e)
        {
            isAllowCheckSV = false;
            if (_current == null || _isAnimationing)
                return;
            OnSelectedHeaderSelected(_current,true);
        }

        private void ScrollViewer_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {

        }

        private void HeadContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _current = null;
            // DeferUpdateHelper.Instance.Register(this);
            OnUpdate();
        }

        public void OnUpdate()
        {
            TryInitDefaultHeadState();
        }

        private FrameworkElement _dyHeader = null;
        private FrameworkElement _staHeader = null;

        public bool IsDynamicHeaderMode
        {
            get { return _dyHeader != null && _dyHeader.Visibility == Visibility.Visible; }
        }

        private void DynamicHeaderLoaded(object sender, RoutedEventArgs e)
        {
            _dyHeader = sender as FrameworkElement;
        }

        private void StaticHeaderLoaded(object sender, RoutedEventArgs e)
        {
            _staHeader = sender as FrameworkElement;
        }
    }
}
