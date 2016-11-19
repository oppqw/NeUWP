using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NeUWP.Controls
{

    public class ScrollViewerBehavior4Drawer : DependencyObject, IBehavior
    {
        private ScrollViewer _sc = null;

        public DependencyObject AssociatedObject
        {
            get
            {
               return this._sc;
            }
        }

        private DrawerCloserObserver _drawerObser = null;

        public void Attach(DependencyObject associatedObject)
        {
            _sc = associatedObject as ScrollViewer;
            if (_sc == null)
                return;
            _drawerObser = new DrawerCloserObserver(_sc, null);
            _sc.DirectManipulationStarted -= _sc_DirectManipulationStarted;
            _sc.DirectManipulationStarted += _sc_DirectManipulationStarted;
            _sc.ViewChanged -= _sc_ViewChanged; ;
            _sc.ViewChanged += _sc_ViewChanged;
        }

        private void CloseDrawer(bool isDefer) {
            if (_drawerObser != null)
                _drawerObser.TryUpdate(isDefer);
        }


        private void _sc_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            CloseDrawer(true);
        }

        private void _sc_DirectManipulationStarted(object sender, object e)
        {
            CloseDrawer(false);
        }

        public void Detach()
        {
            _drawerObser = null;
            if (_sc == null)
                return;
            _sc.DirectManipulationStarted -= _sc_DirectManipulationStarted;
            _sc.ViewChanged -= _sc_ViewChanged; ;
            _sc = null;
        }
    }

}
