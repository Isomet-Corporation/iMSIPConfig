using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iMS;

namespace iMSAccess
{
    public sealed class IMSAccess : IDisposable
    {
        private IMSAccess()
        {
            iMSNET.Init();

            connList = new ConnectionList();
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        private static IMSAccess instance = null;
        private static ConnectionList connList = null;

        private GCHandle _iMSHandle;

        public static IMSAccess Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new IMSAccess();
                }
                return instance;
            }
        }

        public IMSList Scan()
        {
            IMSList fulliMSList = connList.Scan();
            _iMSHandle = GCHandle.Alloc(fulliMSList);

            return fulliMSList;
        }

        public StringList Modules
        {
            get { return connList.Modules(); }
        }

        public ConnectionConfig Config(string module)
        {
            return connList.Config(module);
        }

        /// <summary>
        /// Connection/Disconnection Routines
        /// </summary>
        private static bool isConnected = false;
        public bool Connect(IMSSystem dev)
        {
            DeviceRef = dev;

            DeviceRef.Connect();
            if ((DeviceRef.Synth().IsValid() == true) && (DeviceRef.Ctlr().IsValid() == true))
            {
                isConnected = true;
            }
            else
            {
                isConnected = false;
            }
            return isConnected;
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                if (signalPath != null) signalPath = null;
                DeviceRef.Disconnect();
                DeviceRef = null;
                isConnected = false;
            }
        }
        public bool IsConnected
        {
            get { return isConnected; }
        }

        /// <summary>
        /// Provide a Reference to the iMS Object for user application code
        /// </summary>
        public IMSSystem DeviceRef { get; set; }

        /// <summary>
        /// Single SignalPath Reference
        /// </summary>
        private SignalPath signalPath = null;
        public SignalPath SignalPath
        {
            get
            {
                if (!isConnected) return null;
                if (signalPath == null)
                {
                    signalPath = new SignalPath(DeviceRef);
                }
                return signalPath;
            }
        }


        /// <summary>
        /// Single SystemFunc Reference
        /// </summary>
        private SystemFunc systemFunc = null;
        public SystemFunc SystemFunc
        {
            get
            {
                if (!isConnected) return null;
                if (systemFunc == null)
                {
                    systemFunc = new SystemFunc(DeviceRef);
                }
                return systemFunc;
            }
        }

    }

}
