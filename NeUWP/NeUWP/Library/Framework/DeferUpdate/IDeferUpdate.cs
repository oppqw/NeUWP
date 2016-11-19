using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeUWP.Controls
{
   public  interface IDeferUpdate
    {
        void OnUpdate();
    }

    public class DeferUpdater<T> : IDeferUpdate
    {
        public T Target { set; get; }
        public Action<T> Callback;
        public DeferUpdater(T target, Action<T> callback)
        {
            Target = target;
            Callback = callback;
        }
        public void OnUpdate()
        {
            if (Target == null)
                return;
            if (Callback != null)
                Callback(Target);
        }
    }
}
