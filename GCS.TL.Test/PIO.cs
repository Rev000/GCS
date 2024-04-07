using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCS.TL.Test
{
    public class PIO
    {
        private bool _trReq;
        private bool _busy;
        private bool _compt;
        private bool _uReq;
        private bool _lReq;
        private bool _ready;

        public bool TrReq
        {
            get { return _trReq; }
            set { _trReq = value; }
        }
        public bool Busy
        {
            get { return _busy; }
            set { _busy = value; }
        }
        public bool Compt
        {
            get { return _compt; }
            set { _compt = value; }
        }
        public bool UReq
        {
            get { return _uReq; }
        }
        public bool LReq
        {
            get { return _lReq; }
        }
        public bool Ready
        {
            get { return _ready; }
        }
    }
}
