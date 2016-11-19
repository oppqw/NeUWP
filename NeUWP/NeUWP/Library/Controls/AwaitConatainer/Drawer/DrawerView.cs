using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NeUWP.Controls
{
        public enum DrawerMode { Top, Bottom, Left, Right, Full }

        //抽屉类型  
        //Mode 控制抽屉的位置  可放置上下左右
        //PanelWidth  PanelHeight  抽屉的宽高  如果不设  则和父容器一样当大
        //MaskBrush 抽屉打开时 其余部分的色值  如果Panel和父控件一样大  则看不见
        public sealed class DrawerView : Windows.UI.Xaml.Controls.ContentControl, IBackAble
        {
            public DrawerView()
            {
                this.DefaultStyleKey = typeof(DrawerView);
            }
            public static readonly DependencyProperty MaskBrushProperty = DependencyProperty.Register("MaskBrush", typeof(Brush), typeof(DrawerView), null);

            public Brush MaskBrush
            {
                get { return (Brush)GetValue(MaskBrushProperty); }
                set { SetValue(MaskBrushProperty, value); }
            }

            public static readonly DependencyProperty PanelWidthProperty = DependencyProperty.Register("PanelWidth", typeof(double), typeof(DrawerView), new PropertyMetadata(double.NaN));

            public double PanelWidth
            {
                get { return (double)GetValue(PanelWidthProperty); }
                set { SetValue(PanelWidthProperty, value); }
            }
            public static readonly DependencyProperty PanelHeightProperty = DependencyProperty.Register("PanelHeight", typeof(double), typeof(DrawerView), new PropertyMetadata(double.NaN, new PropertyChangedCallback(PanelHeightChangedCallback)));

            private static void PanelHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if ((double)e.OldValue == double.NaN)
                    return;
                DrawerView sender = d as DrawerView;
                sender.OnPanelHeightChanged((double)e.OldValue, (double)e.NewValue);
            }

            private void OnPanelHeightChanged(double old, double newValue)
            {
                if (IsOpen)
                {
                    double offset = newValue - old;
                    int time = 300;
                    switch (Mode)
                    {
                        case DrawerMode.Bottom:
                            _translate.Y = offset;
                            DoTranslate(0, 0, time);
                            break;
                        case DrawerMode.Top:
                            _translate.Y = -offset;
                            DoTranslate(0, 0, time);
                            break;
                        case DrawerMode.Left:
                            _translate.X = -offset;
                            DoTranslate(0, 0, time);
                            break;
                        case DrawerMode.Right:
                            _translate.X = offset;
                            DoTranslate(0, 0, time);
                            break;
                    }
                }
                else {
                    OnModeChanged(Mode, false);
                }
            }


            public double PanelHeight
            {
                get { return (double)GetValue(PanelHeightProperty); }
                set { SetValue(PanelHeightProperty, value); }
            }

            public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(DrawerMode), typeof(DrawerView), new PropertyMetadata(DrawerMode.Right, new PropertyChangedCallback(ModeChangedCallback)));

            public DrawerMode Mode
            {
                get { return (DrawerMode)GetValue(ModeProperty); }
                set { SetValue(ModeProperty, value); }
            }

            private static void ModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                DrawerMode mode = (DrawerMode)e.NewValue;
                DrawerView _self = d as DrawerView;
                _self.OnModeChanged(mode);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                if (double.IsNaN(PanelWidth))
                    _container.Width = finalSize.Width;
                if (double.IsNaN(PanelHeight))
                    _container.Height = finalSize.Height;
                OnModeChanged(Mode);
                return base.ArrangeOverride(finalSize);
            }

            private const double MASK_INSTANCE = 20;
            private void OnModeChanged(DrawerMode mode, bool IsAnimation = false)
                                        {
                int time = 200;
                if (_mask == null || _translate == null)
                    return;

                switch (mode)
                {
                    case DrawerMode.Bottom:
                        _mask.Height = MASK_INSTANCE;
                        _mask.Width = _container.Width;
                        _mask.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _mask.VerticalAlignment = VerticalAlignment.Bottom;
                        _container.VerticalAlignment = VerticalAlignment.Bottom;
                        DoTranslate(0, _container.Height, time, IsAnimation);
                        break;
                    case DrawerMode.Left:
                        _mask.Width = MASK_INSTANCE;
                        _mask.Height = _container.Height;
                        _mask.VerticalAlignment = VerticalAlignment.Stretch;
                        _mask.HorizontalAlignment = HorizontalAlignment.Left;
                        _container.HorizontalAlignment = HorizontalAlignment.Left;
                        DoTranslate(-_container.Width, 0, time, IsAnimation);
                        break;
                    case DrawerMode.Right:
                        _mask.Width = MASK_INSTANCE;
                        _mask.Height = _container.Height;
                        _mask.VerticalAlignment = VerticalAlignment.Stretch;
                        _mask.HorizontalAlignment = HorizontalAlignment.Right;
                        _container.HorizontalAlignment = HorizontalAlignment.Right;
                        DoTranslate(_container.Width, 0, time, IsAnimation);
                        break;
                    case DrawerMode.Top:
                        _mask.Height = MASK_INSTANCE;
                        _mask.Width = _cp.Width;
                        _mask.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _mask.VerticalAlignment = VerticalAlignment.Top;
                        _container.VerticalAlignment = VerticalAlignment.Top;
                        DoTranslate(0, -_container.Height, time, IsAnimation);
                        break;
                    case DrawerMode.Full:
                        _mask.Height = _cp.Height;
                        _mask.Width = _cp.Width;
                        _mask.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _mask.VerticalAlignment = VerticalAlignment.Stretch;
                        DoTranslate(0, 0, time, IsAnimation);
                        break;
                }
            }

            Grid _mask;
            ContentPresenter _cp;
            Grid _container;
            Grid _root;
            TranslateTransform _translate;

            Storyboard _animation;
            DoubleAnimation _translateX;
            DoubleAnimation _translateY;
            protected override void OnApplyTemplate()
            {
                base.OnApplyTemplate();
                _cp = this.GetTemplateChild("ContentPresenter") as ContentPresenter;
                _mask = this.GetTemplateChild("mask") as Grid;
                _root = this.GetTemplateChild("root") as Grid;
                _translate = this.GetTemplateChild("translate") as TranslateTransform;
                _animation = this.GetTemplateChild("animation") as Storyboard;
                _container = this.GetTemplateChild("ContentContainer") as Grid;
                _animation.Completed += _animation_Completed;
                _translateX = this.GetTemplateChild("translateX") as DoubleAnimation;
                _translateY = this.GetTemplateChild("translateY") as DoubleAnimation;
                _mask.PointerEntered += _mask_PointerPressed;
                _mask.PointerReleased += _mask_PointerReleased;
                _mask.ManipulationDelta += _mask_ManipulationDelta;
                _mask.ManipulationStarted += _mask_ManipulationStarted;
                _mask.ManipulationCompleted += _mask_ManipulationCompleted;
                _mask.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY|ManipulationModes.TranslateInertia;
                _cp.ManipulationCompleted += _cp_ManipulationCompleted;
                _cp.ManipulationDelta += _mask_ManipulationDelta;
                _cp.ManipulationStarted += _cp_ManipulationStarted;
                _cp.ManipulationMode = ManipulationModes.TranslateRailsX | ManipulationModes.TranslateRailsY;
                _container.SizeChanged += _container_SizeChanged;
                _cp.Tapped += (S, E) => { E.Handled = true; };
                _mask.Tapped += (s, e) => { if (_isOpen) e.Handled = true; };
                _root.ManipulationStarted += (s, e) => { Close(); };
                _root.Tapped += (s, e) =>
                {
                    Close();
                };
            }


            Point _cpPos;
            private void _cp_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
            {
                _cpPos = e.Position;
                e.Handled = true;
            }

            private const double CLOSE_DISTANCE = 40;
            private void _cp_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
            {
                if (!_isOpen)
                    return;
                var _x = e.Position.X - _cpPos.X;
                var _y = e.Position.Y - _cpPos.Y;
                if (Math.Abs(_x) > Math.Abs(_y))//水平
                {
                    if (_x > CLOSE_DISTANCE && Mode == DrawerMode.Right)
                    {
                        Close();
                    }
                    else if (_x < -CLOSE_DISTANCE && Mode == DrawerMode.Left)
                    {
                        Close();
                    }

                }
                else
                {
                    if (_y > CLOSE_DISTANCE && Mode == DrawerMode.Bottom)
                    {
                        Close();
                    }
                    else if (_y < -CLOSE_DISTANCE && Mode == DrawerMode.Top)
                    {
                        Close();
                    }

                }
            }

            bool _isOpen = false;
            public bool IsOpen { get { return _isOpen; } set { } }



            public void Open()
            {
                Window.Current.SizeChanged += Current_SizeChanged;
                int time = 100;
                DoTranslate(0, 0, time);
                _mask.ManipulationMode = ManipulationModes.None;
                _isOpen = true;
            }

            private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
            {
                Close();
            }

            public void Close()
            {
                Window.Current.SizeChanged -= Current_SizeChanged;
                if (!_isOpen)
                    return;
                OnModeChanged(Mode, true);
                _mask.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                _isOpen = false;
            }

            private void _container_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                // OnModeChanged(Mode);
            }



            private void _mask_PointerReleased(object sender, PointerRoutedEventArgs e)
            {
                if (_isOpen)
                    return;
                if(isAnimationing&& _animation != null)
                {
                    _animation.Stop();
                isAnimationing = false;
                }
                OnModeChanged(Mode);
            }
            

            private void _mask_PointerPressed(object sender, PointerRoutedEventArgs e)
            {
                if (_isOpen)
                    return;
                int time = 100;
                switch (Mode)
                {
                    case DrawerMode.Bottom:
                        DoTranslate(0, _translate.Y - 3 * MASK_INSTANCE, time);
                        break;
                    case DrawerMode.Left:
                        DoTranslate(_translate.X + 3 * MASK_INSTANCE, 0, time);
                        break;
                    case DrawerMode.Right:
                        DoTranslate(_translate.X - 3 * MASK_INSTANCE, 0, time);
                        break;
                    case DrawerMode.Top:
                        DoTranslate(0, _translate.Y + 3 * MASK_INSTANCE, time);
                        break;
                    case DrawerMode.Full:

                        break;
                }
            }

            Point _startPos;
            private void _mask_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
            {
                _startPos = e.Cumulative.Translation;
            }


            private void _mask_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
            {
                if (Mode == DrawerMode.Left && _translate.X < -PanelWidth / 3*2)
                    OnModeChanged(Mode);
                else
                    Open();
                e.Handled = true;
            }

            private void _mask_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
            {
                switch (Mode)
                {
                    case DrawerMode.Top:
                        _translate.Y = -_container.ActualHeight + 3 * MASK_INSTANCE + e.Cumulative.Translation.Y - _startPos.Y;
                        if (_translate.Y > 0)
                            _translate.Y = 0;
                        else if (_translate.Y < -_container.ActualHeight)
                            _translate.Y = -_container.ActualHeight;
                        break;
                    case DrawerMode.Bottom:
                        _translate.Y = _container.ActualHeight - 3 * MASK_INSTANCE + e.Cumulative.Translation.Y - _startPos.Y;
                        if (_translate.Y > _container.ActualHeight)
                            _translate.Y = _container.ActualHeight;
                        else if (_translate.Y < 0)
                            _translate.Y = 0;
                        break;
                    case DrawerMode.Left:
                        _translate.X = -_container.ActualWidth + 3 * MASK_INSTANCE + e.Cumulative.Translation.X - _startPos.X;
                        if (_translate.X > 0)
                            _translate.X = 0;
                        else if (_translate.X < -_container.ActualWidth)
                            _translate.X = -_container.ActualWidth;
                        break;
                    case DrawerMode.Right:
                        _translate.X = _container.ActualWidth - 3 * MASK_INSTANCE + e.Cumulative.Translation.X - _startPos.X;
                        if (_translate.X > _container.ActualWidth)
                            _translate.X = _container.ActualWidth;
                        else if (_translate.X < 0)
                            _translate.X = 0;
                        break;
                }

            }


            bool isAnimationing = false;
            private void DoTranslate(double xTo, double yTo, int time, bool isNeedAnimation = true)
            {
              
                if (_translate == null)
                    return;
                if (isAnimationing)
                {
                isAnimationing = false;
                    _animation.Stop();
                }
                if (!isNeedAnimation)
                {
                        _translate.X = xTo;
                        _translate.Y = yTo;
                        return;
                }
                _translateX.From = _translate.X;
                _translateX.To = xTo;
                _translateX.Duration = TimeSpan.FromMilliseconds(time);
                _translateY.From = _translate.Y;
                _translateY.To = yTo;
                _translateY.Duration = TimeSpan.FromMilliseconds(time);
                isAnimationing = true;
                _animation.Begin();
            }
            private void _animation_Completed(object sender, object e)
            {
                isAnimationing = false;
                if (_isOpen)
                {
                    _root.ManipulationMode = ManipulationModes.All;
                    _root.Background = MaskBrush;
                    if (OnOpened != null)
                        OnOpened();
                }
                else
                {

                    _root.ManipulationMode = ManipulationModes.None;
                    _root.Background = null;
                    if (OnClosed != null)
                        OnClosed();
                }
            }
            public event Action OnOpened;
            public event Action OnClosed;
        }
    }
 