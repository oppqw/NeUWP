using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeUWP.Controls

{
    public interface IBackAble
    {
        bool IsOpen {  get; }
        void Open();
        void Close();
    }
}
