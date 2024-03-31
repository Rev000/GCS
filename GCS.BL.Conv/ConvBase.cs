using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using GCS.BL.Conv;

namespace GCS.BL.Conv
{
    /// <summary>
    /// Convayor Device Class
    /// </summary>


    abstract public class ConvBase //추상화 클래스로 상속으로만 사용 가능
    {
       private static readonly ILog logger = LogManager.GetLogger("ConvBase");


        public CPIO PIO = new CPIO();

        protected bool dirCw;
        protected bool dirCcw;

        protected bool Auto;
        protected bool Manual;

        protected int step;
        protected int oldStep;
        protected int countConv;

        protected ConvBase(CPIO pio)
        {
            this.PIO = pio;
        }
    }




}
