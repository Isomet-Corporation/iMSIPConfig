using iMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iMSIPConfig
{
    class EthernetSettings
    {
        private IMSSystem device;
        private CS_ETH cs_eth;

        public EthernetSettings(IMSSystem myiMS)
        {
            device = myiMS;
            cs_eth = device.RetrieveSettings_ETH();
        }

        public IPAddress Address
        {
            get
            {
                return IPAddress.Parse(cs_eth.addr);
            }
            set
            {
                cs_eth.addr = value.ToString();
            }
        }

        public IPAddress Netmask
        {
            get
            {
                return IPAddress.Parse(cs_eth.mask);
            }
            set
            {
                cs_eth.mask = value.ToString();
            }
        }

        public IPAddress Gateway
        {
            get
            {
                return IPAddress.Parse(cs_eth.gw);
            }
            set
            {
                cs_eth.gw = value.ToString();
            }
        }

        public bool UseDHCP
        { 
            get { return cs_eth.dhcp; }
            set { cs_eth.dhcp = value; }
        }
        
        public bool Configure()
        {
            return device.ApplySettings(cs_eth);
        }
    }
}
