using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NeUWP.Controls
{
    public class DeferUpdateHelper
    {
        private const int TimeDuration = 100;
        private static DeferUpdateHelper _instance = new DeferUpdateHelper();
        public static DeferUpdateHelper Instance { get { return _instance; } }

        //IMItemsView 为要更新的View. long为注册更新时的时间
        private Dictionary<IDeferUpdate, long> _recoder = new Dictionary<IDeferUpdate, long>();

        private object _lockObj = new object();
        private object _lockRcoderObj = new object();

        public void Register(IDeferUpdate view)
        {
            if (view == null)
                return;

            lock (_lockObj)
            {
                _recoder[view] = DateTime.Now.Ticks;
            }

            TryStartTimer();
        }

        public void UnRegister(IDeferUpdate view)
        {
            lock (_lockObj)
            {
                if (_recoder.ContainsKey(view))
                    _recoder.Remove(view);
            }

            TryStopTimer();
        }

        private void TryStartTimer()
        {
            lock (_lockObj)
            {
                try
                {
                    if (timer == null)
                    {
                        timer = new DispatcherTimer();
                        timer.Tick += Timer_Tick;
                        timer.Interval = TimeSpan.FromMilliseconds(TimeDuration);
                        timer.Start();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            OnHandleTask();
        }

        private void TryStopTimer()
        {
            lock (_lockObj)
            {
                if (_recoder.Keys.Count == 0)
                {
                    try
                    {
                        if (timer != null)
                        {
                            timer.Tick -= Timer_Tick;
                            timer.Stop();
                        }
                    }
                    finally
                    {
                        timer = null;
                    }
                }
            }
        }

        private long TickOffset = TimeSpan.FromMilliseconds(TimeDuration).Ticks;

        private void OnHandleTask()
        {
            try
            {
                List<IDeferUpdate> _views = null;

                lock (_lockObj)
                {
                    long ticks = DateTime.Now.Ticks - TickOffset;
                    _views = _recoder.Keys.Where(key => _recoder[key] < ticks).ToList();
                    if (_views == null || _views.Count == 0)
                        return;
                }

                for (int i = 0; i < _views.Count; ++i)
                {
                    var view = _views[i];
                    if (view != null)
                    {
                        view.OnUpdate();
                        UnRegister(view);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        DispatcherTimer timer = null;
    }
}
