using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using NeUWP.Controls.Extense;
using NeUWP.Controls;

namespace NeUWP.Controls
{
    public class DrawerCloserObserver : IDeferUpdate
    {
        private FrameworkElement _view = null;
        private Action _callback = null;
        public DrawerCloserObserver(FrameworkElement view,Action callback)
        {
            _view = view;
            _callback = callback;
        }
        public async void OnUpdate()
        {
            if (_view == null)
                return;
            var _drawer = _view.GetFirstAncestorOfType<MDrawer>();
            if (_drawer != null)
                return;
             MDrawer.TryCloseCurrent();
            if (_callback != null)
                _callback();
        }

        public void TryUpdate(bool isDeffer) {
            if (isDeffer)
                DeferUpdateHelper.Instance.Register(this);
            else
                OnUpdate();
        }
    }
}
