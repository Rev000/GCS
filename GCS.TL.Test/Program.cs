using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCS.TL.Test;


namespace GCS.TL.Test
{
    class Program
    {
        static void Main()
        {
            ConvData convdata = new ConvData { info = "AA", name = "BB" };

            InfeedConv conv2 = new InfeedConv(convdata);           
            
            Console.WriteLine(conv2.Data.info);
         
        }
    }
}
