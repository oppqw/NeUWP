using NeUWP.Helpers;
using NeUWP.Utilities;
using NeUWP.ViewModels;
using NeUWP.Views;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NeUWP.Controls
{
    public sealed partial class NMPageContainer : UserControl, IBackContainerInterface
    {

        public NMPageContainer()
        {
            this.InitializeComponent();
            backContainer = new BackContainerHolder(this);
            Initialize();
        }

        public Frame PageFrame
        {
            get { return FrameArea; }
        }

        public void ShowSplit()
        {
            SplitArea.Visibility = Visibility.Visible;
        }

        public void HideSplit()
        {
            SplitArea.Visibility = Visibility.Collapsed;
        }

        public void ShowSplitPane()
        {
            SplitArea.CompactPaneLength = 44;
        }

        public void HideSplitPane()
        {
            SplitArea.CompactPaneLength = 0;
        }


        private void Initialize()
        {
            var view = ApplicationView.GetForCurrentView();
            if (view != null)
            {
                view.TitleBar.BackgroundColor = Color.FromArgb(0xFF, 0xBC, 0x2F, 0x2E);
                view.TitleBar.InactiveBackgroundColor = Color.FromArgb(0xFF, 0xB5, 0x71, 0x71);

                view.TitleBar.ForegroundColor = Colors.White;
                view.TitleBar.InactiveForegroundColor = Colors.White;

                view.TitleBar.ButtonBackgroundColor = view.TitleBar.BackgroundColor;
                view.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0xE8, 0x10, 0x23);
                view.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0xF1, 0x70, 0x7A);
                view.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0xFF, 0xB5, 0x71, 0x71);

                view.TitleBar.ButtonForegroundColor = Colors.White;
                view.TitleBar.ButtonHoverForegroundColor = Colors.White;
                view.TitleBar.ButtonPressedForegroundColor = Colors.Black;
                view.TitleBar.ButtonInactiveForegroundColor = Colors.White;
            }
        }



        #region iback
        private BackContainerHolder backContainer = null;

        public bool IsHasBackAbleOpened
        {
            get { return backContainer.IsHasBackAbleOpened; }
        }

        public void OpenBackAble(IBackAble back)
        {
            backContainer.OpenBackAble(back);
        }

        public void Close()
        {
            backContainer.Close();
        }

        public void CloseBackAble(IBackAble iback)
        {
            backContainer.CloseBackAble(iback);
        }
        #endregion

        private void RootContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize != null)
            {
                if (e.PreviousSize == null
                    || e.NewSize.Width != e.PreviousSize.Width)
                {
                    //SplitArea.OpenPaneLength = e.NewSize.Width * 0.83;
                }
            }
        }
    }
}
