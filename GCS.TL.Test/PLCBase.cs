using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace GCS.TL.Test
{
    public abstract class PLCBase
    {
        public struct Data
        {
            public float delayTime;
            public string name;
            public string info;
            public int useCount;
        }

        public required PIO pio;
        public Data data;
        public virtual void InitSetting();

        public virtual void Using()
        {
            Console.WriteLine($"{0} useCount is : {1}", data.name, data.useCount);

        }

    }

}

