using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iMS;
using iMSAccess;

namespace iMSIPConfig
{
    class Scanner
    {
        private bool started;
        private bool isBusy;
        private IMSAccess ims;
        public Scanner(IMSAccess ims)
        {
            this.ims = ims;
            isBusy = false;
            started = false;
        }

        // Call from its own thread!
        public void Start()
        {
            isBusy = true;
            started = true;
            iMSList = ims.Scan();
            isBusy = false;
        }

        public bool isDone()
        {
            if (started && !isBusy)
            {
                started = false;
                return true;
            }
            return false;
        }

        public IMSList iMSList { get; set; }
    }
}
