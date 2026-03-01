using iMSAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using iMS;
using System.Net.NetworkInformation;

namespace iMSIPConfig
{
    public partial class Form1 : Form
    {
        private Scanner scanner = null;
        private Thread scannerThread;
        private EthernetSettings settings;
        public Form1()
        {
            InitializeComponent();

            // Initialise network adapters
            comboBoxNIC.Items.Add("ALL");
            comboBoxNIC.Items.AddRange(NetworkAdapters().ToArray());
            comboBoxNIC.SelectedIndex = 0;

            timer1.Start();
        }

        public System.Collections.Generic.List<String> NetworkAdapters()
        {
            List<String> values = new List<String>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                i.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
            {
                values.Add(nic.Name);
            }
            return values;
        }

        private void buttonScan_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = String.Format("Scanning...");
            var pb = toolStripProgressBar1.ProgressBar;
            pb.Style = ProgressBarStyle.Marquee;
            pb.MarqueeAnimationSpeed = 30;
            pb.Show();

            // Configure to only look for Ethernet devices
            foreach (var cm in IMSAccess.Inst.Modules)
            {
                IMSAccess.Inst.Config(cm).IncludeInScan = false;
            }
            IMSAccess.Inst.Config("CM_ETH").IncludeInScan = true;

            if (checkBox1.Checked)
            {
                IMSAccess.Inst.Config("CM_USBLITE").IncludeInScan = true;
                IMSAccess.Inst.Config("CM_USBSS").IncludeInScan = true;
            }

            // Use All Ethernet interfaces
            IMSAccess.Inst.Config("CM_ETH").PortMask.Clear();

            // Retrict to specific NIC
            if ((!string.IsNullOrEmpty(comboBoxNIC.Text)) && (comboBoxNIC.SelectedIndex > 0))
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces().Where(p => String.Equals(p.Name, comboBoxNIC.Text, StringComparison.CurrentCulture));
                if (nic != null) {
                    foreach (UnicastIPAddressInformation ip in nic.FirstOrDefault().GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IMSAccess.Inst.Config("CM_ETH").PortMask.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            // Start a new thread to perform scan process
            scanner = new Scanner(IMSAccess.Inst);
            scannerThread = new Thread(new ThreadStart(scanner.Start));
            scannerThread.Start();

            listView1.Items.Clear();
            buttonScan.Enabled = false;
        }

        private void populateListBox(IMSList list)
        {
            var items = listView1.Items;
            items.Clear();
            foreach (var dev in list)
            {
                String[] separator = { ":" };
                String[] portlist = dev.ConnPort().Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                Array.Resize(ref portlist, 2); // Ensure there are two items
                items.Add(new ListViewItem(portlist));
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (scanner != null)
            {
                if (scanner.isDone())
                {
                    toolStripStatusLabel1.Text = String.Format("Discovered {0} iMS Systems", scanner.iMSList.Count);
                    var pb = toolStripProgressBar1.ProgressBar;
                    pb.Hide();

                    populateListBox(scanner.iMSList);
                    buttonScan.Enabled = true;
                }
            }

            buttonConnect.Enabled = (listView1.SelectedItems.Count > 0);
        }

        private bool EntryBoxesEnabled
        {
            set
            {
                textBox1.Enabled = value;
                textBox2.Enabled = value;
                textBox3.Enabled = value;
                textBox4.Enabled = value;
                textBox5.Enabled = value;
                textBox6.Enabled = value;
                textBox7.Enabled = value;
                textBox8.Enabled = value;
                textBox9.Enabled = value;
                textBox10.Enabled = value;
                textBox11.Enabled = value;
                textBox12.Enabled = value;
            }
        }

        private void UpdateFormWithConfiguration()
        {
            if (settings.UseDHCP)
            {
                radioButton1.Checked = true;
                EntryBoxesEnabled = false;
            }
            else
            {
                radioButton2.Checked = true;
                EntryBoxesEnabled = true;
            }

            var ip = settings.Address.GetAddressBytes();
            textBox1.Text = ip[0].ToString();
            textBox2.Text = ip[1].ToString();
            textBox3.Text = ip[2].ToString();
            textBox4.Text = ip[3].ToString();

            var nm = settings.Netmask.GetAddressBytes();
            textBox5.Text = nm[0].ToString();
            textBox6.Text = nm[1].ToString();
            textBox7.Text = nm[2].ToString();
            textBox8.Text = nm[3].ToString();

            var gw = settings.Gateway.GetAddressBytes();
            textBox9.Text = gw[0].ToString();
            textBox10.Text = gw[1].ToString();
            textBox11.Text = gw[2].ToString();
            textBox12.Text = gw[3].ToString();

            System.Net.IPAddress addr;
            if (System.Net.IPAddress.TryParse(listView1.Items[listView1.SelectedIndices[0]].SubItems[1].Text, out addr))
            {
                textBox13.Text = MACResolver.FormatMac(MACResolver.GetRemoteMAC(addr.ToString()), ':').ToUpper();
            }
            else textBox13.Text = "N/A";
        }

        private void UpdateConfigurationWithForm()
        {
            byte[] ip = new byte[4];
            byte[] nm = new byte[4];
            byte[] gw = new byte[4];

            byte.TryParse(textBox1.Text, out ip[0]);
            byte.TryParse(textBox2.Text, out ip[1]);
            byte.TryParse(textBox3.Text, out ip[2]);
            byte.TryParse(textBox4.Text, out ip[3]);

            byte.TryParse(textBox5.Text, out nm[0]);
            byte.TryParse(textBox6.Text, out nm[1]);
            byte.TryParse(textBox7.Text, out nm[2]);
            byte.TryParse(textBox8.Text, out nm[3]);

            byte.TryParse(textBox9.Text, out gw[0]);
            byte.TryParse(textBox10.Text, out gw[1]);
            byte.TryParse(textBox11.Text, out gw[2]);
            byte.TryParse(textBox12.Text, out gw[3]);

            settings.Address = new System.Net.IPAddress(ip);
            settings.Netmask = new System.Net.IPAddress(nm);
            settings.Gateway = new System.Net.IPAddress(gw);
            settings.UseDHCP = radioButton1.Checked;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IMSAccess.Inst.IsConnected)
            {
                // Disconnect
                IMSAccess.Inst.Disconnect();

                toolStripStatusLabel1.Text = "Select iMS System or start a new scan";
                buttonConnect.Text = "Connect";

                buttonScan.Enabled = true;
                listView1.Enabled = true;
                comboBoxNIC.Enabled = true;
                checkBox1.Enabled = true;

                groupBox2.Enabled = false;
            }
            else
            {
                // Connect
                if (listView1.SelectedItems.Count > 0)
                {
                    var myiMS = scanner.iMSList[listView1.SelectedIndices[0]];
                    if (IMSAccess.Inst.Connect(myiMS))
                    {
                        toolStripStatusLabel1.Text = String.Format("Connected to {0}", myiMS.ConnPort());
                        
                        buttonConnect.Text = "Disconnect";
                        buttonScan.Enabled = false;
                        listView1.Enabled = false;
                        comboBoxNIC.Enabled = false;
                        checkBox1.Enabled = false;

                        groupBox2.Enabled = true;

                        settings = new EthernetSettings(myiMS);
                        UpdateFormWithConfiguration();
                    } else
                    {
                        MessageBox.Show("Unable to connect to iMS System");
                    }
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            EntryBoxesEnabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            EntryBoxesEnabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfigurationWithForm();

            if (settings.Configure())
            {
                MessageBox.Show("iMS IP Configuration Successfully updated!");

                // Disconnect
                buttonConnect_Click(buttonConnect, new EventArgs());
            }
        }
    }
}
