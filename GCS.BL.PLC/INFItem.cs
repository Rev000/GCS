using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCS.BL.PLC
{
    public class INFItem
    {
        public string SourceID { get; set; }
        public string SourceAddr { get; set; }
        public PLCBase Target { get; set; }
        public string TargetID { get; set; }
        public string TargetAddr { get; set; }
        public string BitReverse { get; set; }
        public string AfterReset { get; set; }
        public string UseYN { get; set; }
        public bool LastBit { get; set; }

        public override string ToString()
        {
            return string.Format("SourceID:{0}, TargetID:{1}, TargetAddr:{2}, BitReverse:{3}, AfterReset:{4},UseYN:{5}"
                , SourceID
                , TargetID
                , TargetAddr
                , BitReverse
                , AfterReset
                , UseYN);
        }
    }
}
