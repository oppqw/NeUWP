using NeUWP.Helpers;
using NeUWP.Utilities;
using System;
using System.IO;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace NeUWP.Controls
{
    public class ImageOpenedEventArgs : EventArgs
    {
        public int PixelHeight { get; set; }
        public int PixelWidth { get; set; }
    }

    public class XBitmapImage
    {
        protected enum ImageStatus
        {
            None,
            Loading,
            Loaded,
            Failed
        }

        public BitmapImage Source { get; set; }

        public event EventHandler<ImageOpenedEventArgs> ImageOpened;
        public event EventHandler<EventArgs> ImageFailed;

        public XBitmapImage()
        {
            Source = new BitmapImage();
            Source.ImageOpened += SourceOpened;
            Source.ImageFailed += SourceFailed;
        }

        protected object locker = new object();

        protected ImageStatus Status { get; set; }

        private string Uri { get; set; }

        private IAsyncAction AsyncWork { get; set; }

        public void Load(string uri, byte[] data = null)
        {
            lock (locker)
            {
                if (!string.IsNullOrEmpty(uri)
                    && !string.Equals(uri, Uri, StringComparison.OrdinalIgnoreCase))
                {
                    RunCancel();
                }
                else if (Status == ImageStatus.Loaded
                    || Status == ImageStatus.Loading)
                {
                    return;
                }

                Uri = uri;

                Clear();

                if (!string.IsNullOrEmpty(Uri))
                {
                    Status = ImageStatus.Loading;

                    if (data != null
                        && data.Length > 0)
                    {
                        RunRenderBitmapImage(data);
                    }
                    else
                    {
                        RequestBitmapImage(uri);
                    }
                }
                else
                {
                    Status = ImageStatus.Loaded;
                }
            }
        }

        public void Cancel()
        {
            lock (locker)
            {
                //RunCancel();
            }
        }

        private void RunCancel()
        {
            if (Status == ImageStatus.Loading
                && !string.IsNullOrEmpty(Uri))
            {
                try
                {
                    if (IsHttpRequest(Uri))
                    {
                        if (AsyncWork != null
                            && AsyncWork.Status == AsyncStatus.Started)
                        {
                            AsyncWork.Cancel();
                        }

                      //  CacheHelper.RemoveImage(Uri);
                    }
                    else
                    {
                        Source.UriSource = null;
                    }

                }
                finally
                {
                    Status = ImageStatus.None;
                }
            }
        }

        private bool IsHttpRequest(string uri)
        {
            return false;//uri.StartsWith("http");//替换下面文件方法后，可将缓存加载打开
        }

        protected virtual void RequestBitmapImage(string uri)
        {
            if (IsHttpRequest(uri))
            {
                //CacheHelper.LoadImage(uri, CacheJobCallback);
            }
            else
            {
                Source.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            }
        }

        protected virtual void RunRenderBitmapImage(byte[] data)
        {
            DispatcherUtil.Run(Source.Dispatcher, () =>
            {
                lock (locker)
                {
                    if (Status != ImageStatus.Loading)
                    {
                        return;
                    }
                }

                RenderBitmapImage(data);

            }, CoreDispatcherPriority.Low);
        }

        protected MemoryStream renderStream { get; set; }
        protected IRandomAccessStream renderStreamSource { get; set; }

        private void RenderBitmapImage(byte[] data)
        {
            try
            {
                renderStream = new MemoryStream(data);
                renderStreamSource = renderStream.AsRandomAccessStream();

                RenderBitmapImage();
            }
            catch
            {
                ClearRender();

                lock (locker)
                {
                    if (Status == ImageStatus.Loading)
                    {
                        Status = ImageStatus.Failed;

                        InvokeImageFailed();
                    }
                }
            }
        }

        protected void RenderBitmapImage()
        {
            lock (locker)
            {
                if (Status == ImageStatus.Loading)
                {
                    AsyncWork = Source.SetSourceAsync(renderStreamSource);
                    AsyncWork.Completed = RenderAsyncActionCompletedHandler;
                }
            }
        }

        private void CacheJobCallback(string uri, byte[] data)
        {
            lock (locker)
            {
                if (Status != ImageStatus.Loading
                    || uri == null
                    || !uri.Equals(Uri, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (data == null
                    || data.Length <= 0)
                {
                    Status = ImageStatus.Failed;

                    InvokeImageFailed();
                }
                else
                {
                    RunRenderBitmapImage(data);
                }
            }
        }

        private void RenderAsyncActionCompletedHandler(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            lock (locker)
            {
                if (AsyncWork != null
                    && asyncInfo != null
                    && AsyncWork.Id == asyncInfo.Id)
                {
                    AsyncWork = null;

                    ClearRender();

                    switch (asyncStatus)
                    {
                        case AsyncStatus.Completed:
                            Status = ImageStatus.Loaded;

                            InvokeImageOpened();
                            break;

                        case AsyncStatus.Error:
                            Status = ImageStatus.Failed;

                            InvokeImageFailed();
                            break;

                        //case AsyncStatus.Canceled:
                        //    System.Diagnostics.Debug.WriteLine("Canceled...");
                        //    break;
                    }
                }
            }
        }

        private void SourceOpened(object sender, RoutedEventArgs e)
        {
            lock (locker)
            {
                if (Status == ImageStatus.Loading)
                {
                    Status = ImageStatus.Loaded;

                    InvokeImageOpened();
                }
            }
        }

        private void SourceFailed(object sender, ExceptionRoutedEventArgs e)
        {
            lock (locker)
            {
                if (Status == ImageStatus.Loading)
                {
                    Status = ImageStatus.Failed;

                    InvokeImageFailed();
                }
            }
        }

        protected virtual void Clear()
        {
            try
            {
                if (Source != null)
                {
                    Source.UriSource = null;
                }

                ClearRender();
            }
            finally
            {
                Status = ImageStatus.None;
            }
        }

        protected virtual void ClearRender()
        {
            try
            {
                if (renderStreamSource != null)
                {
                    renderStreamSource.Dispose();
                }
            }
            finally
            {
                renderStreamSource = null;
            }
            
            try
            {
                if (renderStream != null)
                {
                    renderStream.Dispose();
                }
            }
            finally
            {
                renderStream = null;
            }
        }

        protected void InvokeImageOpened()
        {
            if (Source != null)
            {
                var handler = ImageOpened;
                if (handler != null)
                {
                    var arg = new ImageOpenedEventArgs()
                    {
                        PixelHeight = Source.PixelHeight,
                        PixelWidth = Source.PixelWidth
                    };

                    handler(this, arg);
                }
            }
        }

        protected void InvokeImageFailed()
        {
            var handler = ImageFailed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
