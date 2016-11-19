using NeUWP.Controls;
using NeUWP.Helpers;
using NeUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace NeUWP.Frameworks
{
    public abstract class ViewModelPageBase : ViewModelBase
    {
        #region Cache
        public Task ClearCache(string name)
        {
            return Task.Run(() =>
            {
                //DataStorage.Instance.ClearPageCache(name);  //可使用自己的文件接口替换
            });
        }

        protected Task CacheData(string name, object data)
        {
            return Task.Run(() =>
            {
                //DataStorage.Instance.SavePageCache(name, data is string ? data as string : JsonUtil.Serialize(data));//可使用自己的文件接口替换
            });
        }

        protected Task<T> GetCache<T>(string name)
        {
            var tcs = new TaskCompletionSource<T>();

            Task.Run(() =>
            {
                var content = "";//DataStorage.Instance.GetPageCache(name);//可使用自己的文件接口替换
                if (string.IsNullOrEmpty(content))
                {
                    tcs.TrySetResult(default(T));
                }
                else
                {
                    tcs.TrySetResult(JsonUtil.Deserialize<T>(content));
                }
            });

            return tcs.Task;
        }
        #endregion

        #region Cleanup
        public override void Cleanup()
        {
            base.Cleanup();
        }
        #endregion
    }
}
