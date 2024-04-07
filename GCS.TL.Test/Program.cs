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
        static async Task Main()
        {
            ConvData convdata = new ConvData();
            InfeedConv conv2 = new InfeedConv(convdata);           
            Console.WriteLine("c");
        }
    }
}
