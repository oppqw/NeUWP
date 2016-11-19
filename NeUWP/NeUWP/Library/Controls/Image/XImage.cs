using NeUWP.Utilities;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NeUWP.Controls
{
    public class XImage : Control
    {
        public event EventHandler<ImageOpenedEventArgs> ImageOpened;
        public event EventHandler<EventArgs> ImageFailed;

        #region Properties
        #region Image Properties
        /// <summary>
        /// Gets or sets the source for the image.
        /// </summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(XImage), new PropertyMetadata(null, OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnSourceChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnSourceChanged(object oldValue, object newValue)
        {
            lock (loadedLocker)
            {
                if (!loaded)
                {
                    return;
                }
            }

            if (overrideImage != null
                && oldValue != newValue)
            {
                UnloadBitmap();

                overrideImage.ImageSource = newValue as ImageSource;
            }
        }

        /// <summary>
        /// Gets or sets the source for the image.
        /// </summary>
        public string SourceUri
        {
            get { return (string)GetValue(SourceUriProperty); }
            set { SetValue(SourceUriProperty, value); }
        }

        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register("SourceUri", typeof(string), typeof(XImage), new PropertyMetadata(null, OnSourceUriPropertyChanged));

        private static void OnSourceUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnSourceUriChanged(e.OldValue as string, e.NewValue as string);
            }
        }

        private void OnSourceUriChanged(string oldValue, string newValue)
        {
            lock (loadedLocker)
            {
                if (!loaded)
                {
                    return;
                }
            }

            if (overrideImage != null
                && !string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
            {
                if (Source != null)
                {
                    overrideImage.ImageSource = null;

                    Source = null;
                }

                LoadBitmap(newValue);
            }
        }

        /// <summary>
        /// Gets or sets a value that describes how an System.Windows.Controls.Image
        /// should be stretched to fill the destination rectangle.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(XImage), new PropertyMetadata(Stretch.Fill, OnStretchPropertyChanged));

        private static void OnStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnStretchChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnStretchChanged(object oldValue, object newValue)
        {
            if (overrideImage == null
                || oldValue == newValue)
            {
                return;
            }

            overrideImage.Stretch = (Stretch)newValue;
        }
        #endregion

        #region DefaultImage Properties
        /// <summary>
        /// Gets or sets the default source for the image.
        /// </summary>
        public ImageSource DefaultSource
        {
            get { return (ImageSource)GetValue(DefaultSourceProperty); }
            set { SetValue(DefaultSourceProperty, value); }
        }

        public static readonly DependencyProperty DefaultSourceProperty =
            DependencyProperty.Register("DefaultSource", typeof(ImageSource), typeof(XImage), new PropertyMetadata(null, OnDefaultSourcePropertyChanged));

        private static void OnDefaultSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnDefaultSourceChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnDefaultSourceChanged(object oldValue, object newValue)
        {
            if (defaultImage == null
                || oldValue == newValue)
            {
                return;
            }
            defaultImage.ImageSource = (ImageSource)newValue;
        }

        /// <summary>
        /// Gets or sets a value that describes how an System.Windows.Controls.Image
        /// should be stretched to fill the destination rectangle.
        /// </summary>
        public Stretch DefaultStretch
        {
            get { return (Stretch)GetValue(DefaultStretchProperty); }
            set { SetValue(DefaultStretchProperty, value); }
        }

        public static readonly DependencyProperty DefaultStretchProperty =
            DependencyProperty.Register("DefaultStretch", typeof(Stretch), typeof(XImage), new PropertyMetadata(Stretch.Fill, OnDefaultStretchPropertyChanged));

        private static void OnDefaultStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnDefaultStretchChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnDefaultStretchChanged(object oldValue, object newValue)
        {
            if (defaultImage == null
                || oldValue == newValue)
            {
                return;
            }
            defaultImage.Stretch = (Stretch)newValue;
        }
        #endregion

        #region Round Properties
        /// <summary>
        /// Gets or sets a value that describes how an System.Windows.Controls.Image
        /// should be stretched to fill the destination rectangle.
        /// </summary>
        public bool Round
        {
            get { return (bool)GetValue(RoundProperty); }
            set { SetValue(RoundProperty, value); }
        }

        public static readonly DependencyProperty RoundProperty =
            DependencyProperty.Register("Round", typeof(bool), typeof(XImage), new PropertyMetadata(false, OnRoundPropertyChanged));

        private static void OnRoundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (XImage)d;
            if (control != null)
            {
                control.OnRoundChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnRoundChanged(object oldValue, object newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            UpdateRadius(true);
        }

        private void UpdateRadius(bool changed = false)
        {
            if (!Round && !changed)
            {
                return;
            }

            var radiusValue = 0.0;
            if (Round)
            {
                radiusValue = Height / 2.0;
            }

            if (defaultGrid != null)
            {
                defaultGrid.CornerRadius = new CornerRadius(radiusValue);
            }

            if (overrideGrid != null)
            {
                overrideGrid.CornerRadius = new CornerRadius(radiusValue);
            }

            if (borderGrid != null)
            {
                borderGrid.CornerRadius = new CornerRadius(radiusValue);
            }
        }
        #endregion
        #endregion

        #region Base
        private Grid defaultGrid { get; set; }
        private ImageBrush defaultImage { get; set; }

        private Grid overrideGrid { get; set; }
        private ImageBrush overrideImage { get; set; }

        private Grid borderGrid { get; set; }

        public XImage()
        {
            DefaultStyleKey = typeof(XImage);

            bitmap = new XBitmapImage();
            bitmap.ImageOpened += BitmapImageOpened;
            bitmap.ImageFailed += BitmapImageFailed;

            SizeChanged += SizeChangedHandler;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            defaultGrid = GetTemplateChild("DefaultGrid") as Grid;
            overrideGrid = GetTemplateChild("OverrideGrid") as Grid;
            borderGrid = GetTemplateChild("BorderGrid") as Grid;

            UpdateRadius(false);

            defaultImage = GetTemplateChild("DefaultImage") as ImageBrush;
            if (defaultImage != null)
            {
                defaultImage.Stretch = DefaultStretch;
                defaultImage.ImageSource = DefaultSource;
            }

            overrideImage = GetTemplateChild("OverrideImage") as ImageBrush;
            if (overrideImage != null)
            {
                overrideImage.Stretch = Stretch;

                if (Source != null
                    || !string.IsNullOrEmpty(SourceUri))
                {
                    LoadBitmap(SourceUri);
                }

                lock (loadedLocker)
                {
                    if (!loaded)
                    {
                        loaded = true;

                        Loaded += LoadedHandler;
                        Unloaded += UnloadedHandler;
                    }
                }
            }
        }

        private object loadedLocker = new object();

        private bool loaded { get; set; }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            lock (loadedLocker)
            {
                if (!loaded)
                {
                    return;
                }
            }

            if (Source != null
                || !string.IsNullOrEmpty(SourceUri))
            {
                LoadBitmap(SourceUri);
            }
        }

        private void UnloadedHandler(object sender, RoutedEventArgs e)
        {
            lock (loadedLocker)
            {
                if (!loaded)
                {
                    return;
                }
            }

            UnloadBitmap();
        }

        public double HWRate { set; get; }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            if (HWRate > 0)
            {
                var nwitdh = e.NewSize.Width;
                if (nwitdh != e.PreviousSize.Width)
                {
                    var height = nwitdh * HWRate;
                    if (Math.Abs(e.NewSize.Height - height) >= 2)
                    {
                        Height = height;
                    }
                }
            }

            UpdateRadius(false);
        }
        #endregion

        private XBitmapImage bitmap { get; set; }

        public void LoadBitmap(string uri, byte[] data = null)
        {
            try
            {
                if (overrideImage != null)
                {
                    if (Source != null)
                    {
                        overrideImage.ImageSource = Source;
                    }
                    else if (bitmap != null)
                    {
                        overrideImage.ImageSource = bitmap.Source;

                        bitmap.Load(uri, data);
                    }
                }
            }
            catch
            {

            }
        }

        private void UnloadBitmap()
        {
            if (!string.IsNullOrEmpty(SourceUri))
            {
                bitmap?.Cancel();
            }
        }

        private async void BitmapImageOpened(object sender, ImageOpenedEventArgs e)
        {
            await DispatcherUtil.RunAsync(Dispatcher, () =>
            {
                var handler = ImageOpened;
                if (handler != null)
                {
                    handler(this, e);
                }
            });
        }

        private async void BitmapImageFailed(object sender, EventArgs e)
        {
            await DispatcherUtil.RunAsync(Dispatcher, () =>
            {
                var handler = ImageFailed;
                if (handler != null)
                {
                    handler(this, e);
                }
            });
        }
    }
}
