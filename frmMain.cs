using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;
using SerialToKeyboard.Control;
using System.IO;
using System.Xml;


namespace SerialToKeyboard
{
    public partial class FrmMain : Form
    {
        private bool _isListening;
        private readonly int[] _bauds = new[] { 4800, 9600, 19200, 38400, 57600, 115200 };
        private ComToKey _transfer;
        public string Endchart;

        public FrmMain()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.KeyCode == Keys.Return)
            {
                Debug.Print("Enter received");
            }
            //keyEventArgs.SuppressKeyPress = true;
            //keyEventArgs.Handled = true;
        }

        private void FrmMainLoad(object sender, EventArgs e)
        {
            Debug.Print("FrmMainLoad");
            FillPortList();
            FillBaudList();

            BtnTransferClick(sender, e);



            string pat = Application.StartupPath + @"\comsetings.xml";
            if (File.Exists(pat))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(pat);

                    XmlNodeList comlist = doc.GetElementsByTagName("com");
                    //string com = comlist[0].InnerXml;
                    cmbPort.SelectedIndex = cmbPort.Items.IndexOf(comlist[0].InnerXml);

                    XmlNodeList baudslist = doc.GetElementsByTagName("bauds");
                    //string bauds = baudslist[0].InnerXml;
                    //cmbBaud.SelectedIndex = cmbBaud.Items.IndexOf(baudslist[0].InnerXml);

                    XmlNodeList WinStatelist = doc.GetElementsByTagName("WindowState");
                    string WinState = WinStatelist[0].InnerXml;

                    XmlNodeList WinStartlist = doc.GetElementsByTagName("WinStart");
                    string WinStart = WinStartlist[0].InnerXml;

                    XmlNodeList EndChartlist = doc.GetElementsByTagName("Endchart");
                    Endchart = EndChartlist[0].InnerXml;
                    
                    

                    if (WinState == "minimized")
                    {
                        WindowState = FormWindowState.Minimized;
                    }
                    else
                    {
                        WindowState = FormWindowState.Normal;
                    }

                    //Iniciar captura automatica
                    _isListening = true;
                    btnTransfer.Text = "Detener";
                    StartListening();

                    //Iniciar con Windows
                    if (WinStart == "true")
                    {
                        Microsoft.Win32.RegistryKey key;
                        key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\");
                        key.SetValue("Serial_To_Keyboard", Application.ExecutablePath);
                        key.Close();
                    }
                    else
                    {
                        Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\Serial_To_Keyboard");
                    }
                }
                catch (Exception ex)
                {
                    //notifyIcon1.Visible = true;
                    notifyIcon1.BalloonTipText = "Main Error " + ex.Message;
                    notifyIcon1.ShowBalloonTip(100);
                }
            }
        }


        
        private void FillBaudList()
        {
            foreach (var baud in _bauds)
            {
                cmbBaud.Items.Add(baud);
            }
            if (cmbBaud.Items.Count > 0)
                cmbBaud.SelectedItem = 9600;
        }

        private void FillPortList()
        {
            cmbPort.Sorted = true;
            var s = SerialPort.GetPortNames();
            foreach (var s1 in s)
            {
                cmbPort.Items.Add(s1);
            }
            if (cmbPort.Items.Count > 0)
                cmbPort.SelectedIndex = 0;
        }

        private void BtnTransferClick(object sender, EventArgs e)
        {
            if (e is MouseEventArgs)
            {
                if (!_isListening)
                {
                    _isListening = true;
                    btnTransfer.Text = "Detener";
                    StartListening();
                    WindowState = FormWindowState.Minimized;
                }
                else
                {
                    _isListening = false;
                    btnTransfer.Text = "Iniciar";
                    StopListening();
                }
            }
        }

        private void StopListening()
        {
            _transfer.Stop();
            _transfer.Dispose();
            SetInterfaceEnable(true);
        }

        private void StartListening()
        {
            if (_transfer != null)
                _transfer.Dispose();

            SetInterfaceEnable(false);
            var pName = cmbPort.SelectedItem.ToString();
            int pBaud;
            int.TryParse(cmbBaud.SelectedItem.ToString(), out pBaud);
            _transfer = new ComToKey(new SerialPort(pName, pBaud, Parity.None, 8, StopBits.One), Endchart);
            _transfer.Start();
        }

        private void SetInterfaceEnable(bool b)
        {
            cmbBaud.Enabled = b;
            cmbPort.Enabled = b;
        }

        private void FrmMainActivated(object sender, EventArgs e)
        {
            Debug.Print("FrmMainActivated");
        }

        private void FrmMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "Lectua activa";
                notifyIcon1.ShowBalloonTip(100);
                this.Hide();
                this.ShowInTaskbar = false;
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
