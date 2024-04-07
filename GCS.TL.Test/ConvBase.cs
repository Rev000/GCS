using GCS.TL.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GCS.TL.Test
{
    public struct ConvData
    {
        public string name;
        public string info;
        public float delayTime;
        public int useCount;
    }

    public abstract class ConvBase
    {
        public ConvData Data;
        public PIO pio;

        public ConvBase(ConvData data)
        {
            Data = data;
        }

        public virtual void InitSetting(float delayTime, string name, string info, int useCount)
        {
            Data.name = name;
            Data.info = info;
            Data.delayTime = delayTime;
            Data.useCount = useCount;
        }

        public virtual void Using()
        {
            Console.WriteLine($"{Data.name} useCount is : {Data.useCount}");
        }

        public virtual void InitPio()
        {
            pio.Busy = false;
            pio.Compt = false;
            pio.TrReq = false;
        }
    }
}