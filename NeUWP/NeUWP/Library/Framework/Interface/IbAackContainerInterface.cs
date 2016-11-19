namespace NeUWP.Controls
{
   public  interface IBackContainerInterface
    {
        //为true表示能后退
        bool IsHasBackAbleOpened { get; }

        //打开iback
        void OpenBackAble(IBackAble _back);

        //关闭 iback
        void CloseBackAble(IBackAble _iback);

        //关闭该容器最上层iback
        void Close();
    }
}
