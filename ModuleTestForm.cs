using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;   // required for Marshal
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Xml;
using NationalInstruments.DAQmx;

namespace ModuleTestV8
{
    public partial class ModuleTestForm : Form
    {
        public ModuleTestForm()
        {
            InitializeComponent();
            Global.Init();
        }

        public const int ModuleCount = 9;
        public const String DefaultProfileName = "SkytraqTest.dat";
        private bool startCounting = false;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);        // call default p
            if (m.Msg == WM_DEVICECHANGE)
            {
                // WM_DEVICECHANGE can have several meanings depending on the WParam value...
                int msgType = m.WParam.ToInt32();
                if (msgType == DBT_DEVICEARRIVAL || msgType == DBT_DEVICEREMOVECOMPLETE)
                {
                    int devType = Marshal.ReadInt32(m.LParam, 4);
                    if (DBT_DEVTYP_PORT == devType)
                    {

                        DEV_BROADCAST_PORT vol;
                        vol = (DEV_BROADCAST_PORT)
                            Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_PORT));

                        int step = (vol.dbcp_name[1] == 0x00) ? 2 : 1;
                        StringBuilder sb = new StringBuilder(8);
                        for (int i = 0; i < vol.dbcp_name.Length; i += step)
                        {
                            if (vol.dbcp_name[i] == 0x00)
                            {
                                break;
                            }
                            sb.Append(vol.dbcp_name[i]);
                        }
                        if (TestRunning == TestStatus.Finished || TestRunning == TestStatus.Ready)
                        {
                            if (msgType == DBT_DEVICEARRIVAL)
                            {
                                AddMessage(0, sb.ToString() + " plugged-in.");
                            }
                            else
                            {
                                AddMessage(0, sb.ToString() + " removed.");
                            }
                            InitComSel();
                        }
                    }
                }
            }
        }

        private ModuleTestProfile profile;

        private CheckBox[] disableTable;
        private ComboBox[] comSelTable;
        private Panel[] panelTable;
        private Label[] resultTable;
        private ListBox[] messageTable;
        private PictureBox[] snrChartTable;

        private int[] failCount;
        private int[] passCount;
        private Label[] failTable;
        private Label[] totalTable;
        private Label[] yieldTable;

        private SessionReport rp = new SessionReport();
        private XmlDocument doc = new XmlDocument();
        private XmlElement root;
        private XmlElement testSession;

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Login.loginInfo.DebugMode)
            {   //不是Debug Mode要問密碼才能改設定
                Password pwdForm = new Password();
                if (DialogResult.OK != pwdForm.ShowDialog())
                {
                    return;
                }
                else
                {

                }
            }

            ModuleTestProfile tempProfile = new ModuleTestProfile(profile);
            SettingForm frmSetting = new SettingForm(tempProfile);
            frmSetting.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == frmSetting.ShowDialog())
            {
                if (!tempProfile.ReadePromIniFile() || 
                    (tempProfile.IsNeedSlavePromIniFile() && !tempProfile.ReadeSlavePromIniFile()))
                {
                    ErrorMessage.Show(ErrorMessage.Errors.NoFwIniError);
                }
                else
                {
                    if (tempProfile.enableDownload && null == tempProfile.fwProfile.promRaw)
                    {
                        if (!tempProfile.fwProfile.ReadePromRawData(Login.loginInfo.currentPath + "\\" + tempProfile.fwProfile.promFile))
                        {
                            ErrorMessage.Show(ErrorMessage.Errors.NoPromFileError);
                        }
                    }

                    if (tempProfile.IsNeedSlavePromIniFile() && null == tempProfile.slaveFwProfile.promRaw)
                    {
                        if (!tempProfile.slaveFwProfile.ReadePromRawData(Login.loginInfo.currentPath + "\\" + tempProfile.slaveFwProfile.promFile))
                        {
                            ErrorMessage.Show(ErrorMessage.Errors.NoPromFileError);
                        }
                    }
                }

                profile = tempProfile;
                UpdatePanelInfo();
                if (!Login.loginInfo.DebugMode)
                {
                    Login.loginInfo.DebugMode = true;
                    DebugModeUI();
                    ErrorMessage.Show(ErrorMessage.Warnings.EnterDebugMode);
                }
            }
        }
        private void AddComSel(ComboBox c, string[] ports, string sel)
        {
            String selItem = "";
            if (c.SelectedIndex >= 0)
            {
                selItem = c.Text;
            }
            else
            {
                selItem = sel;
            }

            c.Items.Clear();
            foreach (string port in ports)
            {
                int n = c.Items.Add(port);
                if (port == selItem)
                {
                    c.SelectedIndex = n;
                }
            }
        }
        private void InitComSel()
        {
            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;
            string[] comPortSetting = { o.gdComPort, o.a1ComPort, o.a2ComPort, o.a3ComPort,
                    o.a4ComPort, o.b1ComPort, o.b2ComPort, o.b3ComPort, o.b4ComPort };

            string[] ports = SerialPort.GetPortNames();

            Array.Sort(ports, delegate (string s1, string s2)
            {
                int ns1 = Convert.ToInt32(s1.Replace("COM", ""));
                int ns2 = Convert.ToInt32(s2.Replace("COM", ""));
                return ((ns1 == ns2) ? 0 : ((ns1 > ns2) ? 1 : -1));
            });

            for (int i = 0; i < ModuleCount; i++)
            {
                ComboBox c = comSelTable[i];
                if (c == null)
                {
                    continue;
                }
                AddComSel(c, ports, comPortSetting[i]);
            }
            string sel = anCtrlSel.Text;
            AddComSel(anCtrlSel, ports, sel);
        }

        //Properties載入後，更新UI使之同步
        private void UpdateUIBySetting()
        {
            Properties.Settings o = Properties.Settings.Default;
            bool[] disableSetting = { o.gdDisable, o.a1Disable, o.a2Disable, o.a3Disable,
                    o.a4Disable, o.b1Disable, o.b2Disable, o.b3Disable, o.b4Disable };

            for (int i = 0; i < ModuleCount; i++)
            {
                if ((disableTable[i] as CheckBox) != null)
                {
                    (disableTable[i] as CheckBox).Checked = disableSetting[i];
                }
            }
        }

        //UI變更後，寫入Properties。
        private void UpdateSettingByUI()
        {
            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;

            o.gdDisable = (disableTable[0] as CheckBox) != null ? (disableTable[0] as CheckBox).Checked : false;
            o.a1Disable = (disableTable[1] as CheckBox) != null ? (disableTable[1] as CheckBox).Checked : false;
            o.a2Disable = (disableTable[2] as CheckBox) != null ? (disableTable[2] as CheckBox).Checked : false;
            o.a3Disable = (disableTable[3] as CheckBox) != null ? (disableTable[3] as CheckBox).Checked : false;
            o.a4Disable = (disableTable[4] as CheckBox) != null ? (disableTable[4] as CheckBox).Checked : false;
            o.b1Disable = (disableTable[5] as CheckBox) != null ? (disableTable[5] as CheckBox).Checked : false;
            o.b2Disable = (disableTable[6] as CheckBox) != null ? (disableTable[6] as CheckBox).Checked : false;
            o.b3Disable = (disableTable[7] as CheckBox) != null ? (disableTable[7] as CheckBox).Checked : false;
            o.b4Disable = (disableTable[8] as CheckBox) != null ? (disableTable[8] as CheckBox).Checked : false;

            String s;
            s = (comSelTable[0] as ComboBox) != null ? (comSelTable[0] as ComboBox).Text : "";
            if (s.Length > 0) o.gdComPort = s;
            s = (comSelTable[1] as ComboBox) != null ? (comSelTable[1] as ComboBox).Text : "";
            if (s.Length > 0) o.a1ComPort = s;
            s = (comSelTable[2] as ComboBox) != null ? (comSelTable[2] as ComboBox).Text : "";
            if (s.Length > 0) o.a2ComPort = s;
            s = (comSelTable[3] as ComboBox) != null ? (comSelTable[3] as ComboBox).Text : "";
            if (s.Length > 0) o.a3ComPort = s;
            s = (comSelTable[4] as ComboBox) != null ? (comSelTable[4] as ComboBox).Text : "";
            if (s.Length > 0) o.a4ComPort = s;
            s = (comSelTable[5] as ComboBox) != null ? (comSelTable[5] as ComboBox).Text : "";
            if (s.Length > 0) o.b1ComPort = s;
            s = (comSelTable[6] as ComboBox) != null ? (comSelTable[6] as ComboBox).Text : "";
            if (s.Length > 0) o.b2ComPort = s;
            s = (comSelTable[7] as ComboBox) != null ? (comSelTable[7] as ComboBox).Text : "";
            if (s.Length > 0) o.b3ComPort = s;
            s = (comSelTable[8] as ComboBox) != null ? (comSelTable[8] as ComboBox).Text : "";
            if (s.Length > 0) o.b4ComPort = s;

            s = (anCtrlSel as ComboBox) != null ? (anCtrlSel as ComboBox).Text : "";
            if (s.Length > 0) o.annCtrlPort = s;
        }
        
        enum ResultDisplayType
        {
            None,
            Testing,
            Downloading,
            Fail,
            Pass,
        }

        private void UpdateSlotStatus(int index)
        {
            failTable[index].Text = failCount[index].ToString();
            totalTable[index].Text = passCount[index].ToString();

            if ((failCount[index] + passCount[index]) == 0)
            {
                yieldTable[index].Text = "0.0%";
            }
            yieldTable[index].Text = ((double)passCount[index] / (failCount[index] + passCount[index]) * 100.0).ToString("F1") + "%";
        }

        private Font f11 = null;
        private Font f16 = null;
        private void SetResultDisplay(System.Windows.Forms.Label l, ResultDisplayType r)
        {
            FontStyle fs = l.Font.Style;
            FontFamily fm = new FontFamily(l.Font.Name);
            if (f11 == null)
            {
                f11 = new Font(fm, 11, fs);
            }
            if (f16 == null)
            {
                f16 = new Font(fm, 16, fs);
            }
            
            switch (r)
            {
                case ResultDisplayType.None:
                    l.Text = "";
                    l.ForeColor = System.Drawing.Color.Black;
                    break;
                case ResultDisplayType.Testing:
                    l.Font = f16;
                    l.Text = "Testing...";
                    l.ForeColor = System.Drawing.Color.Green;
                    break;
                case ResultDisplayType.Downloading:
                    l.Font = f11;
                    l.Text = "Downloading...";
                    l.ForeColor = System.Drawing.Color.Green;
                    break;
                case ResultDisplayType.Fail:
                    l.Font = f16;
                    l.Text = "FAIL";
                    l.ForeColor = System.Drawing.Color.Red;
                    break;
                case ResultDisplayType.Pass:
                    l.Font = f16;
                    l.Text = "PASS";
                    l.ForeColor = System.Drawing.Color.Blue;
                    break;
            }
        }

        System.Drawing.Font snrFont = new Font(new FontFamily(SystemFonts.DialogFont.Name), 7);
       
        StringFormat drawFormat = new StringFormat();
        
        const int barWidth = 16;
        const int txtXo = 7;
        const int MaxSnrLine = 16;
        private int DrawSnr(int startPos, GpsMsgParser.ParsingStatus o, Graphics g, GpsMsgParser.SateType t)
        {
            for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
            {
                GpsMsgParser.ParsingStatus.sateInfo s = null;
                Brush inUseBarBrush = null;
                Pen noUseBarPen = null;
                Brush inUseIcoBrush = null;
                Brush noUseIcoBrush = null;
                Brush inUseBarTxtBrush = null;
                Brush inUseIcoTxtBrush = null;
                Brush noUseBarTxtBrush = null;
                Brush noUseIcoTxtBrush = null;

                if (GpsMsgParser.SateType.Gps == t)
                {
                    inUseBarBrush = Brushes.Blue;
                    noUseBarPen = Pens.Blue;
                    inUseIcoBrush = Brushes.Blue;
                    noUseIcoBrush = Brushes.Blue;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Blue;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGpsSate(i);
                }
                else if(GpsMsgParser.SateType.Glonass == t)
                {
                    inUseBarBrush = Brushes.DarkOrchid;
                    noUseBarPen = Pens.DarkOrchid;
                    inUseIcoBrush = Brushes.DarkOrchid;
                    noUseIcoBrush = Brushes.DarkOrchid;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.DarkOrchid;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGlonassSate(i);
                }
                else if(GpsMsgParser.SateType.Beidou == t)
                {
                    inUseBarBrush = Brushes.Orange;
                    noUseBarPen = Pens.Orange;
                    inUseIcoBrush = Brushes.Orange;
                    noUseIcoBrush = Brushes.Orange;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Orange;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetBeidouSate(i);
                }
                else if (GpsMsgParser.SateType.Navic == t)
                {
                    inUseBarBrush = Brushes.Maroon;
                    noUseBarPen = Pens.Maroon;
                    inUseIcoBrush = Brushes.Maroon;
                    noUseIcoBrush = Brushes.Maroon;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Maroon;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetNavicSate(i);
                }
                else
                {
                    inUseBarBrush = Brushes.Blue;
                    noUseBarPen = Pens.Blue;
                    inUseIcoBrush = Brushes.Blue;
                    noUseIcoBrush = Brushes.Blue;
                    inUseBarTxtBrush = Brushes.White;
                    inUseIcoTxtBrush = Brushes.White;
                    noUseBarTxtBrush = Brushes.Blue;
                    noUseIcoTxtBrush = Brushes.White;
                    s = o.GetGpsSate(i);
                }                

                if (s.prn == GpsMsgParser.ParsingStatus.NullValue)
                {
                    break;
                }
                if(s.snr == 0 || s.snr == GpsMsgParser.ParsingStatus.NullValue)
                {
                    continue;
                } 
                int barHeight = (s.snr > 45) ? 45 : s.snr;
                if (s.inUse)
                {
                    g.FillEllipse(inUseIcoBrush, barWidth * startPos - 1, 46, barWidth, barWidth);
                    g.DrawString(s.prn.ToString(), snrFont, inUseIcoTxtBrush, barWidth * startPos + txtXo, 49F, drawFormat);
                    if (s.snr != GpsMsgParser.ParsingStatus.NullValue)
                    {
                        g.FillRectangle(inUseBarBrush, barWidth * startPos, 45 - s.snr, barWidth - 1, s.snr);
                        g.DrawString(s.snr.ToString(), snrFont, inUseBarTxtBrush, barWidth * startPos + txtXo, 33F, drawFormat);
                    }
                }
                else 
                {
                    g.FillEllipse(noUseIcoBrush, barWidth * startPos - 1, 46, barWidth, barWidth);
                    g.DrawString(s.prn.ToString(), snrFont, noUseIcoTxtBrush, barWidth * startPos + txtXo, 49F, drawFormat);
                    if (s.snr != GpsMsgParser.ParsingStatus.NullValue)
                    {
                        g.DrawRectangle(noUseBarPen, barWidth * startPos, 45 - s.snr, barWidth - 2, s.snr);
                        g.DrawString(s.snr.ToString(), snrFont, noUseBarTxtBrush, barWidth * startPos + txtXo, 33F, drawFormat);
                    }
                }
                if (++startPos >= MaxSnrLine)
                {
                    break;
                }
            }
            return startPos;
        }

        void MySnrChartPaint(object sender, PaintEventArgs pea)
        {
            int idx = (int)((sender as PictureBox).Tag);
            if (disableTable[idx].Checked)
            {
                return;
            }

            if (TestModule.dvResult == null)
            {
                return;
            } 
                        
            GpsMsgParser.ParsingStatus o = TestModule.dvResult[idx];
            int lastChannel = 0;
            lastChannel = DrawSnr(0, o, pea.Graphics, GpsMsgParser.SateType.Gps);
            lastChannel = DrawSnr(lastChannel, o, pea.Graphics, GpsMsgParser.SateType.Glonass);
            lastChannel = DrawSnr(lastChannel, o, pea.Graphics, GpsMsgParser.SateType.Beidou);
            lastChannel = DrawSnr(lastChannel, o, pea.Graphics, GpsMsgParser.SateType.Navic);
        }

        private SQLiteConnection sqliteConn;
        private SQLiteCommand sqliteCmd;  
        void TestSQLite()
        {
            sqliteConn = new SQLiteConnection("Data source=database.db");
            // Open
            sqliteConn.Open();
            //Get Command instance
            sqliteCmd = sqliteConn.CreateCommand();
            //sqliteCmd.CommandText = "CREATE TABLE test (id integer primary key, text varchar(10));";
            //sqliteCmd.ExecuteNonQuery();
            //sqliteCmd.CommandText = "INSERT INTO test (id, text) VALUES (1, '測試1');";
            //sqliteCmd.ExecuteNonQuery();
            sqliteCmd.CommandText = "SELECT * FROM test";
            SQLiteDataReader sqliteDataReader = sqliteCmd.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                // Print out the content of the text field:
                String s = sqliteDataReader["text"].ToString();
                MessageBox.Show(s);

            }
            // End
            sqliteConn.Close();
        }
        //private bool DebugMode { get; set; }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.ModuleTest;
            DoLogin();
        }

        private void DoiCacheTesterSetting()
        {
            resetTesterLogin rstLogin = new resetTesterLogin();
            if (DialogResult.OK != rstLogin.ShowDialog())
            {
                this.Close();
                return;
            }
            //this.Icon = Properties.Resources.iCacheTester;
            InitMainForm();
            this.Text = "i-cache Tester - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            firstWork.Text = "i-cache Tester";
            firstWork.ForeColor = Color.Green;
            cancel.Visible = true;
            workFormNo.Text = "";
            testCounter.Text = resetTesterLogin.TestPeriod.ToString();
            testCounter.ForeColor = Color.Black;
            moduleName.Text = "";
            settingGroup.Visible = true;
            gdBaudrateTitle.Text = "Test Baud Rate";
            gdBaudrate.Left += 30;
            gdBaudrate.Text = GpsBaudRateConverter.Index2BaudRate(resetTesterLogin.BootBaudRate).ToString();
            testGpTitle.Text = "Test Period";
            testGpSnrPass.Left += 30;
            testGpSnrPass.Text = resetTesterLogin.TestPeriod.ToString() + " seconds";

            testBdTitle.Visible = false;
            testGiTitle.Visible = false;
            dlBaudrateTitle.Visible = false;
            testGpLimitTitle.Visible = false;
            testGlLimitTitle.Visible = false;
            testBdLimitTitle.Visible = false;
            testGiLimitTitle.Visible = false;
            testBdSnrPass.Visible = false;
            testGiSnrPass.Visible = false;
            dlBaudrate.Visible = false;
            testGpLimit.Visible = false;
            testGlLimit.Visible = false;
            testBdLimit.Visible = false;
            testGiLimit.Visible = false;
            testGlTitle.Visible = false;
            testGlSnrPass.Visible = false;

            firmwareGroup.Visible = false;
            startDownload.Visible = false;
            startTesting.Visible = false;
            cancel.Visible = true;

            Size s = testCounter.Size;
            s.Width += 240;
            testCounter.Size = s;
            testCounter.TextAlign = ContentAlignment.MiddleLeft;

            disableTable[0].Visible = false;
            //panelTable[0].Visible = false;
            gdComSel.Visible = false;
            gdComSel_t.Visible = false;
            anCtrlSel.Visible = false;
            a4SnrChart.Visible = false;

            settingToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Enabled = false;

            for (int i = 0; i < ModuleCount; i++)
            {
                if (snrChartTable[i] != null && messageTable[i] != null)
                {
                    messageTable[i].Size = new Size(messageTable[i].Width, snrChartTable[i].Height + messageTable[i].Height);
                    messageTable[i].Top = snrChartTable[i].Top;
                    snrChartTable[i].Visible = false;
                }
            }
            TestRunning = TestStatus.Ready;
        }

        private void DoResetTesterSetting()
        {
            resetTesterLogin rstLogin = new resetTesterLogin();
            if (DialogResult.OK != rstLogin.ShowDialog())
            {
                this.Close();
                return;
            }
            //this.Icon = Properties.Resources.ResetTester;
            InitMainForm();
            this.Text =  "Reset Tester - "  + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            firstWork.Text = "Reset Tester";
            firstWork.ForeColor = Color.Green;
            cancel.Visible = true;
            workFormNo.Text = "";
            testCounter.Text = resetTesterLogin.TestPeriod.ToString();
            testCounter.ForeColor = Color.Black;
            moduleName.Text = "";
            settingGroup.Visible = true;
            gdBaudrateTitle.Text = "Test Baud Rate";
            gdBaudrate.Left += 30;
            gdBaudrate.Text = GpsBaudRateConverter.Index2BaudRate(resetTesterLogin.BootBaudRate).ToString();
            testGpTitle.Text = "Test Period";
            testGpSnrPass.Left += 30;
            testGpSnrPass.Text = resetTesterLogin.TestPeriod.ToString() + " seconds";
            testGlTitle.Text = "Check NMEA Interval";
            testGlSnrPass.Left += 30;
            testGlSnrPass.Text = resetTesterLogin.CheckInterval.ToString() + " ms";

            testBdTitle.Visible = false;
            testGiTitle.Visible = false;
            dlBaudrateTitle.Visible = false;
            testGpLimitTitle.Visible = false;
            testGlLimitTitle.Visible = false;
            testBdLimitTitle.Visible = false;
            testGiLimitTitle.Visible = false;
            testBdSnrPass.Visible = false;
            testGiSnrPass.Visible = false;
            dlBaudrate.Visible = false;
            testGpLimit.Visible = false;
            testGlLimit.Visible = false;
            testBdLimit.Visible = false;
            testGiLimit.Visible = false;

            firmwareGroup.Visible = false;
            startDownload.Visible = false;
            startTesting.Visible = false;
            cancel.Visible = false;

            Size s = testCounter.Size;
            s.Width += 240;
            testCounter.Size = s;
            testCounter.TextAlign = ContentAlignment.MiddleLeft;

            disableTable[0].Visible = false;
            panelTable[0].Visible = false;

            settingToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Enabled = false;
        }

        private void DoOpenPortTesterSetting()
        {
            InitMainForm();
            this.Text = "Open Port Tester - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            firstWork.Text = "Open Port Tester";
            firstWork.ForeColor = Color.Coral;
            cancel.Visible = true;
            workFormNo.Text = "";
            testCounter.Text = resetTesterLogin.TestPeriod.ToString();
            testCounter.ForeColor = Color.Black;
            moduleName.Text = "";
            settingGroup.Visible = false;
            firmwareGroup.Visible = false;
            startDownload.Visible = false;
            startTesting.Visible = false;
            cancel.Visible = true;

            disableTable[0].Visible = false;
            panelTable[0].Visible = false;

            settingToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Enabled = false;
        }

        private void DoLogin()
        {
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterScreen;
            if (DialogResult.OK != login.ShowDialog())
            {
                this.Close();
                return;
            }

            if (!Login.loginInfo.DebugMode && login.Profile() == null)
            {   //非Debug Mode一定要有Profile才能進入
                this.Close();
                return;
            }

            InitMainForm();

            if (Login.loginInfo.DebugMode && login.Profile() == null)
            {
                profile = new ModuleTestProfile();
            }
            else
            {
                profile = login.Profile();
            }

            this.Text = ((Login.loginInfo.DebugMode) ? "Module Test (Debug Mode) - " : "Module Test - ") + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (!profile.ReadePromIniFile())
            {
                ErrorMessage.Show(ErrorMessage.Errors.NoFwIniError);
            }
            else if (profile.enableDownload && null == profile.fwProfile.promRaw)
            {
                if (!profile.fwProfile.ReadePromRawData(Login.loginInfo.currentPath + "\\" + profile.fwProfile.promFile))
                {
                    ErrorMessage.Show(ErrorMessage.Errors.NoPromFileError);
                }
            }

            if(profile.enableSlaveDownload)
            { 
                if (!profile.ReadeSlavePromIniFile())
                {
                    ErrorMessage.Show(ErrorMessage.Errors.NoFwIniError);
                }
                else if (profile.slaveFwProfile != null && profile.enableDownload && null == profile.slaveFwProfile.promRaw)
                {
                    if (!profile.slaveFwProfile.ReadePromRawData(Login.loginInfo.currentPath + "\\" + profile.slaveFwProfile.promFile))
                    {
                        ErrorMessage.Show(ErrorMessage.Errors.NoPromFileError);
                    }
                }
            }

            workFormNo.Text = Login.loginInfo.WorkFormNumber;
            DebugModeUI();
            UpdatePanelInfo();
            TestRunning = TestStatus.Ready;

            root = doc.CreateElement("Root");
            doc.AppendChild(root);

            //if (!Login.loginInfo.DebugMode)
            {
                if (!PrepareReportData())
                {
                    this.Close();
                    return;
                }
            }
            testSession = doc.CreateElement("TestSession");
            root.AppendChild(testSession);
        }

        private void InitMainForm()
        {
            drawFormat.Alignment = StringAlignment.Center;

            //Establish UI controls table
            disableTable = new CheckBox[ModuleCount] {gdDisable, a1Disable, a2Disable, a3Disable, a4Disable, 
                b1Disable, b2Disable, b3Disable, b4Disable };
            comSelTable = new ComboBox[ModuleCount] {gdComSel, a1ComSel, a2ComSel, a3ComSel, a4ComSel, 
                b1ComSel, b2ComSel, b3ComSel, b4ComSel };
            panelTable = new Panel[ModuleCount] {gdPanel, a1Panel, a2Panel, a3Panel, a4Panel, 
                b1Panel, b2Panel, b3Panel, b4Panel };
            resultTable = new Label[ModuleCount] {gdResult, a1Result, a2Result, a3Result, a4Result, 
                b1Result, b2Result, b3Result, b4Result };

            messageTable = new ListBox[ModuleCount] {gdMessage, a1Message, a2Message, a3Message, a4Message, 
                b1Message, b2Message, b3Message, b4Message };

            snrChartTable = new PictureBox[ModuleCount] {gdSnrChart, a1SnrChart, a2SnrChart, a3SnrChart, a4SnrChart, 
                b1SnrChart, b2SnrChart, b3SnrChart, b4SnrChart };

            failCount = new int[ModuleCount];
            passCount = new int[ModuleCount];

            failTable = new Label[ModuleCount] {null, a1FailCount, a2FailCount, a3FailCount, a4FailCount, 
                b1FailCount, b2FailCount, b3FailCount, b4FailCount };

            totalTable = new Label[ModuleCount] {null, a1TotalCount, a2TotalCount, a3TotalCount, a4TotalCount, 
                b1TotalCount, b2TotalCount, b3TotalCount, b4TotalCount };

            yieldTable = new Label[ModuleCount] {null, a1Yield, a2Yield, a3Yield, a4Yield, 
                            b1Yield, b2Yield, b3Yield, b4Yield };

            for (int i = 0; i < ModuleCount; i++)
            {
                if (snrChartTable[i] != null)
                {
                    snrChartTable[i].Tag = i;
                    snrChartTable[i].Paint += new PaintEventHandler(MySnrChartPaint);
                }
            }

            InitComSel();
            foreach (Label l in resultTable)
            {
                if (l != null)
                {
                    SetResultDisplay(l, ResultDisplayType.None);
                }
            }

            initBackgroundWorker();
            UpdateUIBySetting();
            testTimer.Tick += new EventHandler(TimerEventProcessor);
            openPortTimer.Tick += new EventHandler(OpenPortTimerEventProcessor);
        }

        public static readonly String LogFolderName = "Log";
        public static readonly String LogFileName = "result.xml";

        private bool PrepareReportData()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}\\{1}", Login.loginInfo.currentPath, LogFolderName);
                if (!Directory.Exists(sb.ToString()))
                {
                    Directory.CreateDirectory(sb.ToString());
                }
                if (Login.loginInfo.WorkFormNumber.Length > 0)
                {
                    sb.AppendFormat("\\{0}", Login.loginInfo.WorkFormNumber);
                }
                else
                {
                    sb.AppendFormat("\\{0}", "DebugMode");
                }
                sb.AppendFormat("\\{0}", Login.loginInfo.loginTime.ToString("yyyy-MM-dd-HHmmss"));
                if (!Directory.Exists(sb.ToString()))
                {
                    Directory.CreateDirectory(sb.ToString());
                }
                sb.AppendFormat("\\{0}", LogFileName);
                Login.loginInfo.logFile = sb.ToString();
            }
            catch
            {
                ErrorMessage.Show(ErrorMessage.Errors.CreateFolderFail);
                return false;
            }

            //XmlDocument doc = new XmlDocument();
            //XmlElement root = doc.CreateElement("Root");

            //Write Login data
            XmlElement loginData = doc.CreateElement("LoginData");
            WriteLoginDataToXml(ref loginData, doc);
            root.AppendChild(loginData);

            //Write Test profile
            XmlElement testProfile = doc.CreateElement("TestProfile");
            WriteTestProfileToXml(ref testProfile, doc);
            root.AppendChild(testProfile);

            //Write Firmware information
            XmlElement firmwareInfo = doc.CreateElement("FirmwareInfo");
            WriteFirmwareInfoToXml(ref firmwareInfo, doc);
            root.AppendChild(firmwareInfo);

            doc.Save(Login.loginInfo.logFile);

            return true;
        }

        private void WriteLoginDataToXml(ref XmlElement e, XmlDocument doc)
        {
            XmlElement item = doc.CreateElement("Item");
            e.AppendChild(item);
            Login.loginInfo.GenerateXml(ref item, doc);
        }

        private void WriteTestProfileToXml(ref XmlElement e, XmlDocument doc)
        {
            XmlElement item = doc.CreateElement("Item");
            e.AppendChild(item);
            profile.GenerateXml(ref item, doc);
        }

        private void WriteFirmwareInfoToXml(ref XmlElement e, XmlDocument doc)
        {
            XmlElement item = doc.CreateElement("Item");
            e.AppendChild(item);
            if (profile.fwProfile == null)
            {
                return;
            }
            profile.fwProfile.GenerateXml(ref item, doc);
        }

        private void WriteTestSessionToXml(ref XmlElement e, XmlDocument doc)
        {
            XmlElement item = doc.CreateElement("Item");
            e.AppendChild(item);
            XmlElement itemData1 = doc.CreateElement("UISetting");
            itemData1.SetAttribute("TY", ((int)rp.sessionType).ToString());
            itemData1.SetAttribute("ST", rp.startTime.ToString("u"));
            itemData1.SetAttribute("ED", rp.endTime.ToString("u"));
            itemData1.SetAttribute("GDDS", (disableTable[0] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("A1DS", (disableTable[1] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("A2DS", (disableTable[2] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("A3DS", (disableTable[3] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("A4DS", (disableTable[4] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("B1DS", (disableTable[5] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("B2DS", (disableTable[6] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("B3DS", (disableTable[7] as CheckBox).Checked.ToString());
            itemData1.SetAttribute("B4DS", (disableTable[8] as CheckBox).Checked.ToString());

            itemData1.SetAttribute("GDCM", (null == testParam[0].comPort) ? "" : testParam[0].comPort.ToString());
            itemData1.SetAttribute("A1CM", (null == testParam[1].comPort) ? "" : testParam[1].comPort.ToString());
            itemData1.SetAttribute("A2CM", (null == testParam[2].comPort) ? "" : testParam[2].comPort.ToString());
            itemData1.SetAttribute("A3CM", (null == testParam[3].comPort) ? "" : testParam[3].comPort.ToString());
            itemData1.SetAttribute("A4CM", (null == testParam[4].comPort) ? "" : testParam[4].comPort.ToString());
            itemData1.SetAttribute("B1CM", (null == testParam[5].comPort) ? "" : testParam[5].comPort.ToString());
            itemData1.SetAttribute("B2CM", (null == testParam[6].comPort) ? "" : testParam[6].comPort.ToString());
            itemData1.SetAttribute("B3CM", (null == testParam[7].comPort) ? "" : testParam[7].comPort.ToString());
            itemData1.SetAttribute("B4CM", (null == testParam[8].comPort) ? "" : testParam[8].comPort.ToString());
            item.AppendChild(itemData1);

            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;
            XmlElement itemData2 = doc.CreateElement("SnrOffset");
            itemData2.SetAttribute("A1GP", o.a1GpSnrOffset.ToString());
            itemData2.SetAttribute("A2GP", o.a2GpSnrOffset.ToString());
            itemData2.SetAttribute("A3GP", o.a3GpSnrOffset.ToString());
            itemData2.SetAttribute("A4GP", o.a4GpSnrOffset.ToString());
            itemData2.SetAttribute("B1GP", o.b1GpSnrOffset.ToString());
            itemData2.SetAttribute("B2GP", o.b2GpSnrOffset.ToString());
            itemData2.SetAttribute("B3GP", o.b3GpSnrOffset.ToString());
            itemData2.SetAttribute("B4GP", o.b4GpSnrOffset.ToString());

            itemData2.SetAttribute("A1GL", o.a1GlSnrOffset.ToString());
            itemData2.SetAttribute("A2GL", o.a2GlSnrOffset.ToString());
            itemData2.SetAttribute("A3GL", o.a3GlSnrOffset.ToString());
            itemData2.SetAttribute("A4GL", o.a4GlSnrOffset.ToString());
            itemData2.SetAttribute("B1GL", o.b1GlSnrOffset.ToString());
            itemData2.SetAttribute("B2GL", o.b2GlSnrOffset.ToString());
            itemData2.SetAttribute("B3GL", o.b3GlSnrOffset.ToString());
            itemData2.SetAttribute("B4GL", o.b4GlSnrOffset.ToString());

            itemData2.SetAttribute("A1BD", o.a1BdSnrOffset.ToString());
            itemData2.SetAttribute("A2BD", o.a2BdSnrOffset.ToString());
            itemData2.SetAttribute("A3BD", o.a3BdSnrOffset.ToString());
            itemData2.SetAttribute("A4BD", o.a4BdSnrOffset.ToString());
            itemData2.SetAttribute("B1BD", o.b1BdSnrOffset.ToString());
            itemData2.SetAttribute("B2BD", o.b2BdSnrOffset.ToString());
            itemData2.SetAttribute("B3BD", o.b3BdSnrOffset.ToString());
            itemData2.SetAttribute("B4BD", o.b4BdSnrOffset.ToString());
            item.AppendChild(itemData2);

            XmlElement itemData3 = doc.CreateElement("Tester");
            if (!(disableTable[1] as CheckBox).Checked)
            {
                XmlElement a1 = doc.CreateElement("a1");
                a1.SetAttribute("DU", testParam[1].duration.ToString());
                a1.SetAttribute("RT", ((UInt64)testParam[1].error).ToString());
                a1.InnerText = testParam[1].log.ToString();
                itemData3.AppendChild(a1);
            }

            if (!(disableTable[2] as CheckBox).Checked)
            {
                XmlElement a2 = doc.CreateElement("a2");
                a2.SetAttribute("DU", testParam[2].duration.ToString());
                a2.SetAttribute("RT", ((UInt64)testParam[2].error).ToString());
                a2.InnerText = testParam[2].log.ToString();
                itemData3.AppendChild(a2);
            }

            if (!(disableTable[3] as CheckBox).Checked)
            {
                XmlElement a3 = doc.CreateElement("a3");
                a3.SetAttribute("DU", testParam[3].duration.ToString());
                a3.SetAttribute("RT", ((UInt64)testParam[3].error).ToString());
                a3.InnerText = testParam[3].log.ToString();
                itemData3.AppendChild(a3);
            }

            if (!(disableTable[4] as CheckBox).Checked)
            {
                XmlElement a4 = doc.CreateElement("a4");
                a4.SetAttribute("DU", testParam[4].duration.ToString());
                a4.SetAttribute("RT", ((UInt64)testParam[4].error).ToString());
                a4.InnerText = testParam[4].log.ToString();
                itemData3.AppendChild(a4);
            }

            if (!(disableTable[5] as CheckBox).Checked)
            {
                XmlElement b1 = doc.CreateElement("b1");
                b1.SetAttribute("DU", testParam[5].duration.ToString());
                b1.SetAttribute("RT", ((UInt64)testParam[5].error).ToString());
                b1.InnerText = testParam[5].log.ToString();
                itemData3.AppendChild(b1);
            }
            
            if (!(disableTable[6] as CheckBox).Checked)
            {
                XmlElement b2 = doc.CreateElement("b2");
                b2.SetAttribute("DU", testParam[6].duration.ToString());
                b2.SetAttribute("RT", ((UInt64)testParam[6].error).ToString());
                b2.InnerText = testParam[6].log.ToString();
                itemData3.AppendChild(b2);
            }

            if (!(disableTable[7] as CheckBox).Checked)
            {
                XmlElement b3 = doc.CreateElement("b3");
                b3.SetAttribute("DU", testParam[7].duration.ToString());
                b3.SetAttribute("RT", ((UInt64)testParam[7].error).ToString());
                b3.InnerText = testParam[7].log.ToString();
                itemData3.AppendChild(b3);
            }

            if (!(disableTable[8] as CheckBox).Checked)
            {
                XmlElement b4 = doc.CreateElement("b4");
                b4.SetAttribute("DU", testParam[8].duration.ToString());
                b4.SetAttribute("RT", ((UInt64)testParam[8].error).ToString());
                b4.InnerText = testParam[8].log.ToString();
                itemData3.AppendChild(b4);
            }
            item.AppendChild(itemData3);

            Crc32 crc32 = new Crc32();
            XmlElement itemKey = doc.CreateElement("ItemKey");
            UInt64 c = crc32.ComputeChecksum(itemData1.OuterXml) ^
                crc32.ComputeChecksum(itemData2.OuterXml) ^
                crc32.ComputeChecksum(itemData3.OuterXml);
            itemKey.SetAttribute("Key", c.ToString());
            item.AppendChild(itemKey);
        }

        private void DebugModeUI()
        {
            if (Login.loginInfo.DebugMode)
            {
                firstWork.Text = "Debug Mode";
                firstWork.ForeColor = Color.Red;
            }
            else if (Login.loginInfo.FirstTest)
            {
                firstWork.Text = "第一次測試";
                firstWork.ForeColor = Color.Blue;
            }
            else
            {
                firstWork.Text = "重測、重工";
                firstWork.ForeColor = Color.Blue;
            }
            cancel.Visible = Login.loginInfo.DebugMode;
        }

        private void UpdatePanelInfo()
        {
            testCounter.Text = profile.SetTestPeriodCounter(profile.GetTotalTestPeriod()).ToString();
            testCounter.ForeColor = Color.Black;

            moduleName.Text = profile.moduleName;
            gdBaudrate.Text = GpsBaudRateConverter.Index2BaudRate(profile.gdBaudSel).ToString();

            testGpTitle.Enabled = profile.testGpSnr;
            testGpSnrPass.Enabled = profile.testGpSnr;
            testGpLimitTitle.Enabled = profile.testGpSnr;
            testGpLimit.Enabled = profile.testGpSnr;
            testGpSnrPass.Text = ModuleTestProfile.GpsCriteriaStrings(profile.gpSnrLower, profile.gpSnrUpper);
            testGpLimit.Text = profile.gpSnrLimit.ToString();

            testGlTitle.Enabled = profile.testGlSnr;
            testGlSnrPass.Enabled = profile.testGlSnr;
            testGlLimitTitle.Enabled = profile.testGlSnr;
            testGlLimit.Enabled = profile.testGlSnr;
            testGlSnrPass.Text = ModuleTestProfile.GpsCriteriaStrings(profile.glSnrLower, profile.glSnrUpper);
            testGlLimit.Text = profile.glSnrLimit.ToString();

            testBdTitle.Enabled = profile.testBdSnr;
            testBdSnrPass.Enabled = profile.testBdSnr;
            testBdLimitTitle.Enabled = profile.testBdSnr;
            testBdLimit.Enabled = profile.testBdSnr;
            testBdSnrPass.Text = ModuleTestProfile.GpsCriteriaStrings(profile.bdSnrLower, profile.bdSnrUpper);
            testBdLimit.Text = profile.bdSnrLimit.ToString();

            testGiTitle.Enabled = profile.testGiSnr;
            testGiSnrPass.Enabled = profile.testGiSnr;
            testGiLimitTitle.Enabled = profile.testGiSnr;
            testGiLimit.Enabled = profile.testGiSnr;
            testGiSnrPass.Text = ModuleTestProfile.GpsCriteriaStrings(profile.giSnrLower, profile.giSnrUpper);
            testGiLimit.Text = profile.giSnrLimit.ToString();

            dlBaudrate.Text = GpsBaudRateConverter.Index2BaudRate(profile.dlBaudSel).ToString();
            dlBaudrate.Enabled = profile.enableDownload;
            dlBaudrateTitle.Enabled = profile.enableDownload;

            anCtrlSel.Visible = profile.enableSlaveDownload || profile.testAntenna || profile.testUart2TxRx || profile.testDrCyro || profile.testInsDrGyro;
            anCtrlSel_t.Visible = profile.enableSlaveDownload || profile.testAntenna || profile.testUart2TxRx || profile.testDrCyro || profile.testInsDrGyro;

            if (profile.fwProfile != null)
            {
                fwBaudrate.Text = profile.fwProfile.dvBaudRate.ToString();
                kVer.Text = profile.fwProfile.kVersion;
                sVer.Text = profile.fwProfile.sVersion;
                rVer.Text = profile.fwProfile.rVersion;
                crc.Text = profile.fwProfile.crcTxt;
                fwSize.Text = (profile.fwProfile.promRaw == null) ? "0" : profile.fwProfile.promRaw.Length.ToString();
            }
            else
            {
                fwBaudrate.Text = "Unknown";
                kVer.Text = "Unknown";
                sVer.Text = "Unknown";
                rVer.Text = "Unknown";
                crc.Text = "Unknown";
                fwSize.Text = "Unknown";
            }

            if (profile.slaveFwProfile != null && profile.enableSlaveDownload)
            {
                slaveFirmwareGroup.Enabled = true;
                fw2Baudrate.Text = profile.slaveFwProfile.dvBaudRate.ToString();
                kVer2.Text = profile.slaveFwProfile.kVersion;
                sVer2.Text = profile.slaveFwProfile.sVersion;
                rVer2.Text = profile.slaveFwProfile.rVersion;
                crc2.Text = profile.slaveFwProfile.crcTxt;
                fw2Size.Text = (profile.slaveFwProfile.promRaw == null) ? "0" : profile.slaveFwProfile.promRaw.Length.ToString();
            }
            else
            {
                slaveFirmwareGroup.Enabled = false;
                fw2Baudrate.Text = "Unknown";
                kVer2.Text = "Unknown";
                sVer2.Text = "Unknown";
                rVer2.Text = "Unknown";
                crc2.Text = "Unknown";
                fw2Size.Text = "Unknown";
            }
            EnableButton(true);
        }

        private void EnableButton(bool e)
        {
            if (!e)
            {
                startTesting.Enabled = false;
                startDownload.Enabled = false;
                startDownload2.Enabled = false;
                return;
            }

            startTesting.Enabled = profile.fwProfile != null;
            if (profile.enableDownload && profile.fwProfile != null && profile.fwProfile.promRaw != null)
            {
                startDownload.Enabled = true;
                if (profile.enableSlaveDownload && profile.twoUartDownload)
                {
                    startDownload.Height = 30;
                    startDownload2.Visible = true;
                    if (profile.slaveFwProfile != null && profile.slaveFwProfile.promRaw != null)
                    {
                        startDownload2.Enabled = true;
                    }
                }
            }
            else
            {
                startDownload.Enabled = false;
                if (profile.enableSlaveDownload && profile.twoUartDownload)
                {
                    startDownload.Height = startTesting.Height;
                    startDownload2.Visible = false;
                }
            }
        }

        private int FindIndex(object sender)
        {
            if (sender is CheckBox)
            {
                for (int i = 0; i < ModuleCount; i++)
                {
                    if (sender == disableTable[i])
                        return i;
                }
                return -1;
            }
            if (sender is ComboBox)
            {
                for (int i = 0; i < ModuleCount; i++)
                {
                    if (sender == comSelTable[i])
                        return i;
                }
                return -1;
            }
            return -1;
        }

        private void StopTesting()
        {
            testTimer.Stop();
            StopAllWorker();
        }

        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (!CheckTestBusy(false))
            {
                CancelTest(false);
                return;
            } 
            
            if (TestRunning == TestStatus.Downloading)
            {
                testTimer.Stop();
                return;
            }

            if (startCounting)
            {
                int count = profile.DecreaseTestPeriodCounter();
                testCounter.Text = count.ToString();
                if (count > 0)
                {
                    testCounter.ForeColor = Color.Red;
                    return;
                }
                testCounter.ForeColor = Color.Blue;
                StopTesting();
            }
        }

        private void OpenPortTimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            int count = profile.IncreaseTestPeriodCounter();
            testCounter.Text = count.ToString();
        }

        private void disable_CheckedChanged(object sender, EventArgs e)
        {
            int index = FindIndex(sender);
            (panelTable[index] as Panel).Enabled = !(sender as CheckBox).Checked;
            UpdateSettingByUI();
            ModuleTestV8.Properties.Settings.Default.Save();
        }

        private void comSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettingByUI();
            ModuleTestV8.Properties.Settings.Default.Save();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ModuleTestV8.Properties.Settings.Default.Save();
        }

        private BackgroundWorker[] bkWorker = new BackgroundWorker[ModuleCount];
        private void initBackgroundWorker()
        {
            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;
            double[] gpSnrOffsetTable = {0, o.a1GpSnrOffset, o.a2GpSnrOffset, o.a3GpSnrOffset, o.a4GpSnrOffset, 
                                   o.b1GpSnrOffset, o.b2GpSnrOffset, o.b3GpSnrOffset, o.b4GpSnrOffset};
            double[] glSnrOffsetTable = {0, o.a1GlSnrOffset, o.a2GlSnrOffset, o.a3GlSnrOffset, o.a4GlSnrOffset, 
                                   o.b1GlSnrOffset, o.b2GlSnrOffset, o.b3GlSnrOffset, o.b4GlSnrOffset};
            double[] bdSnrOffsetTable = {0, o.a1BdSnrOffset, o.a2BdSnrOffset, o.a3BdSnrOffset, o.a4BdSnrOffset, 
                                   o.b1BdSnrOffset, o.b2BdSnrOffset, o.b3BdSnrOffset, o.b4BdSnrOffset};
            double[] giSnrOffsetTable = {0, o.a1GiSnrOffset, o.a2GiSnrOffset, o.a3GiSnrOffset, o.a4GiSnrOffset,
                                   o.b1GiSnrOffset, o.b2GiSnrOffset, o.b3GiSnrOffset, o.b4GiSnrOffset};

            for (int i = 0; i < ModuleCount; i++)
            {
                bkWorker[i] = new BackgroundWorker();
                bkWorker[i].WorkerReportsProgress = true;
                bkWorker[i].WorkerSupportsCancellation = true;
                bkWorker[i].DoWork += new DoWorkEventHandler(bw_DoWork);
                bkWorker[i].ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bkWorker[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                testParam[i] = new WorkerParam();
                testParam[i].index = i;
                testParam[i].bw = bkWorker[i];
                testParam[i].gps = new SkytraqGps();
                testParam[i].parser = new GpsMsgParser();
                testParam[i].gpSnrOffset = gpSnrOffsetTable[i];
                testParam[i].glSnrOffset = glSnrOffsetTable[i];
                testParam[i].bdSnrOffset = bdSnrOffsetTable[i];
                testParam[i].giSnrOffset = giSnrOffsetTable[i];
                testParam[i].log = new StringBuilder();
            }
        }
        private WorkerParam[] testParam = new WorkerParam[ModuleCount];
        private static System.Windows.Forms.Timer testTimer = new System.Windows.Forms.Timer();
        private static System.Windows.Forms.Timer openPortTimer = new System.Windows.Forms.Timer();
        private enum TestStatus
        {
            NotReady,
            Ready,
            Waiting,
            GoldenLaunched,
            Testing,
            Downloading,
            Finished
        } 
        private TestStatus TestRunning { get; set; }
        private bool IsDeviceChecked(int index)
        {
            CheckBox c = disableTable[index] as CheckBox;
            if (c == null || c.Checked)
            {
                return false;
            }
            if ((comSelTable[index] as ComboBox).SelectedIndex < 0)
            {   // doesn't select a baud rate, disable it!
                c.Checked = true;
                return false;
            }
            return true;
        }

        private void StopAllWorker()
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }

                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
             }
        }

        private void AddMessage(int i, String s)
        {
            WriteLogFile(i, s);
            ListBox b = messageTable[i] as ListBox;

            bool scroll = (b.TopIndex == b.Items.Count - (int)(b.Height / b.ItemHeight));
            b.Items.Add(s);
            if (scroll)
            {
                b.TopIndex = b.Items.Count - (int)(b.Height / b.ItemHeight);
            }
            testParam[i].log.AppendLine(s);
        }

        private void LaunchTestDevice()
        {
            for (int i = 1; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                testParam[i].error = WorkerParam.ErrorType.TestNotComplete;
                bkWorker[i].RunWorkerAsync(testParam[i]);
                AddMessage(i, "-------------------- Begin testing --------------------");
                Thread.Sleep(20);
            }

            //testCounter.Text = profile.GetTotalTestPeriod().ToString();
            testCounter.Text = profile.SetTestPeriodCounter(profile.GetTotalTestPeriod()).ToString();
            testCounter.ForeColor = Color.Red;
            startCounting = false;
            testTimer.Interval = 1000;
            testTimer.Start();
        }

        private void startTesting_Click(object sender, EventArgs e)
        {
            PressStrsvrButton(true);
            rp.NewSession(SessionReport.SessionType.Testing);

            if ((disableTable[0] as CheckBox).Checked || (comSelTable[0] as ComboBox).SelectedIndex < 0)
            {
                ErrorMessage.Show(ErrorMessage.Warnings.NoGoldenSelectWarning);
                return;
            }

            bool hasWork = false;
            TestModule.ResetTotalTestCount();
            TestModule.ResetDrMcuStatus();

            for (int i = 0; i < ModuleCount; ++i)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                snrChartTable[i].Invalidate();
                testParam[i].comPort = (comSelTable[i] as ComboBox).Text;
                testParam[i].profile = profile;
                testParam[i].log.Remove(0, testParam[i].log.Length);
                testParam[i].annIoPort = (anCtrlSel as ComboBox).Text;

                if (i != 0)
                {
                    TestModule.IncreaseTotalTestCount();
                    hasWork = true;
                }

                if (hasWork)
                {
                    SetResultDisplay(resultTable[i] as Label, ResultDisplayType.Testing);
                }
            }

            if(!hasWork) 
            {
                MessageBox.Show("Please select at least one slot!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((profile.testAntenna || profile.testDrCyro || profile.testUart2TxRx || profile.testInsDrGyro) && 
                (anCtrlSel as ComboBox).Text == "")
            {
                MessageBox.Show("Please select MCU COM!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (profile.testVoltage)
            {
                if(!ReadVoltage())
                {
                    MessageBox.Show("Please connect to USB-6000 and make sure it has been driven!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            TestModule.ClearResult(TestModule.ClearType.All);
            SetResultDisplay(resultTable[0] as Label, ResultDisplayType.Testing);
            bkWorker[0].RunWorkerAsync(testParam[0]);
            EnableButton(false);
            TestRunning = TestStatus.GoldenLaunched;
            testCounter.Text = profile.GetTotalTestPeriod().ToString();
            testCounter.ForeColor = Color.Green;
            TestModule.antennaEvent.Set();
        }

        private double[] voltage = new double[8];
        private bool ReadVoltage()
        {
            try
            {
                for(int i = 0; i < 4; ++i)
                {
                    voltage[i] = GetAnalogVoltage(i);
                }
            }
            catch(Exception)
            {
                //MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }

        private double GetAnalogVoltage(int index)
        {
            Task tsk;
            //Create a new task
            using (tsk = new Task())
            {
                string[] devices = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);

                //Create a virtual channel
                tsk.AIChannels.CreateVoltageChannel(devices[index], "",
                    (AITerminalConfiguration)(-1), -10.0, 10.0, AIVoltageUnits.Volts);
                AnalogMultiChannelReader reader = new AnalogMultiChannelReader(tsk.Stream);
                //Verify the Task
                tsk.Control(TaskAction.Verify);
                //Plot Multiple Channels to the table
                double[] data = reader.ReadSingleSample();
                return data[0];
            }
        }

        //background
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerParam p = e.Argument as WorkerParam;
            TestModule t = new TestModule();
            Stopwatch w = new Stopwatch();
            p.voltage = voltage;
            w.Start();
            if (p.index == 0)
            {
                if (t.DoGolden(p))
                {
                    e.Cancel = true;
                }
            }
            else
            {
                if (TestRunning == TestStatus.Downloading)
                {
                    if (t.DoDownload(p))
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    if(p.profile.testDrCyro)
                    {
                        if (t.DoDrTest(p))
                        {
                            e.Cancel = true;
                        }
                    }
                    else if (profile.testInsDrGyro)
                    {
                        if (t.DoInsDrTest(p))
                        {
                            e.Cancel = true;
                        }
                    }
                    else
                    {
                        if (t.DoTest(p))
                        {
                            e.Cancel = true;
                        }
                    }
                }
                TestModule.DecreaseTotalTestCount();
            }

            t.EndProcess(p);
            p.duration = w.ElapsedMilliseconds;
            w.Stop();
        }

        private void SaveReport()
        {
            if (Global.functionType != Global.FunctionType.ModuleTest &&
                Global.functionType != Global.FunctionType.ModuleTestDr)
            {
                return;
            }
            //Write Login data
            WriteTestSessionToXml(ref testSession, doc);
            //doc.AppendChild(root);
            doc.Save(Login.loginInfo.logFile);
        }

        private WaitingGoldenForm waitingForm = new WaitingGoldenForm();
        private void WriteLogFile(int idx, String log)
        {
            if (Global.functionType != Global.FunctionType.iCacheTester)
                return;

            String[] logTitle = { "GD-", "A1-", "A2-", "A3-", "A4-", "B1-", "B2-", "B3-", "B4-" };
            if (idx < 0 || idx >= logTitle.Length)
            {
                return;
            }

            String logFile = resetTesterLogin.currentPath + "\\" + logTitle[idx] + resetTesterLogin.loginTime.ToString("yyMMdd-HHmmss") + ".log";
            System.IO.File.AppendAllText(logFile, DateTime.Now.ToString("HH:mm:ss") + " " + log);
            try
            {
                if (log.Length < 2 || log.Substring(log.Length - 2, 2) != "\r\n")
                {
                    System.IO.File.AppendAllText(logFile, "\r\n");
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                {
                    Console.WriteLine("Exception : {0}", e.ToString());
                }
                return;
            }
        }
        //處理進度
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
            WorkerReportParam r = e.UserState as WorkerReportParam;
            if (r.reportType == WorkerReportParam.ReportType.ShowProgress)
            {
                AddMessage(r.index, r.output);
            }
            else if (r.reportType == WorkerReportParam.ReportType.UpdateSnrChart)
            {
                snrChartTable[r.index].Invalidate();
            }
            else if (r.reportType == WorkerReportParam.ReportType.LaunchTimer)
            {
                startCounting = true;
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowError)
            {
                WorkerParam.ErrorType er = testParam[r.index].error;
                if (r.index == 0)
                {   //Golden sample error.
                    StopTesting();
                    MessageBox.Show("Golden Sample error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AddMessage(r.index, "Error : " + er.ToString());
                    AddMessage(r.index, "Error Code : " + WorkerParam.GetErrorString(er));
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Fail);
                }
                else if (er == WorkerParam.ErrorType.ResetDetectError)
                {   //Using FirmwareVersionError to display Reset Detect
                    int tt = resetTesterLogin.TestPeriod - Convert.ToInt32(testCounter.Text);
                    AddMessage(r.index, "Error : ResetDetect in " + tt.ToString() + " seconds");
                    AddMessage(r.index, "Error Code : " + WorkerParam.GetErrorString(er));
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Fail);
                    failCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
                else if (er == WorkerParam.ErrorType.NmeaDelayDetectError)
                {   //Using CheckRtcError to display NMEA Delay Detect
                    int tt = resetTesterLogin.TestPeriod - Convert.ToInt32(testCounter.Text);
                    AddMessage(r.index, "Error : NMEADelayDetect in " + tt.ToString() + " seconds");
                    AddMessage(r.index, "Error Code : " + WorkerParam.GetErrorString(er));
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Fail);
                    failCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
                else
                {   //Test device error.
                    //
                    AddMessage(r.index, "Error : " + er.ToString());
                    AddMessage(r.index, "Error Code : " + WorkerParam.GetErrorString(er));
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Fail);
                    failCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowFinished)
            {
                if (TestRunning == TestStatus.Downloading)
                {
                    AddMessage(r.index, "Download Completed.");
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Pass);
                    passCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
                else
                {
                    AddMessage(r.index, "Test Completed.");
                    SetResultDisplay((resultTable[r.index] as Label), ResultDisplayType.Pass);
                    passCount[r.index]++;
                    UpdateSlotStatus(r.index);
                }
            }
            else if (r.reportType == WorkerReportParam.ReportType.GoldenSampleReady)
            {
                LaunchTestDevice();
                TestRunning = TestStatus.Testing;
            }
            else if (r.reportType == WorkerReportParam.ReportType.ShowWaitingGoldenSample)
            {
                waitingForm.StartPosition = FormStartPosition.CenterScreen;
                waitingForm.main = this;
                hiddenNotify.Text = "";
                waitingForm.Show();
            }
            else if (r.reportType == WorkerReportParam.ReportType.HideWaitingGoldenSample)
            {
                waitingForm.Hide();
            }
        }

        //執行完成
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);

            if (!CheckTestBusy(true))
            {
                if (TestRunning == TestStatus.Downloading)
                {
                    CancelDownload();
                }
                else
                {
                    CancelTest(false);
                }
            }
           
            int busyCount = GetBusyCount();
            //AddMessage(0, "BusyCount = " + busyCount.ToString());
            
            if (0 == busyCount)
            {
                if ((profile.testDrCyro || profile.testInsDrGyro) && profile.useSensor)
                {
                    bool reset = TestModule.ResetMotoPosition((anCtrlSel as ComboBox).Text);
                    if (!reset)
                    {
                        MessageBox.Show("Moto Reset fail!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                rp.EndSession();
                SaveReport();
                TestRunning = TestStatus.Finished;
                SetResultDisplay(resultTable[0] as Label, ResultDisplayType.None);
                EnableButton(true);
                PressStrsvrButton(false);

                if (continueMode && (profile.testDrCyro || profile.testInsDrGyro))
                {
                    Thread.Sleep(1000);
                    MouseClickInStartTesting();
                }
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        public void MouseClickInStartTesting()
        {
            Point sp = startTesting.PointToScreen(startTesting.Location);

            int x = sp.X + 5;
            int y = sp.Y + 5;
            
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private bool CheckTestBusy(bool includeGolden)
        {
            int start = (includeGolden) ? 0 : 1;
            for (int i = start; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    return true;
                }
            }
            return false;
        }
        private int GetBusyCount()
        {
            int count = 0;
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    count++;
                }
            }
            return count;
        }

        private void CancelTest(bool userCancel)
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
                if (i > 0 && userCancel)
                {
                    testParam[i].error |= WorkerParam.ErrorType.TestNotComplete;
                }
            }
            
            //startTesting.Enabled = true;
            TestRunning = TestStatus.Finished;
            testTimer.Stop();
            testCounter.ForeColor = Color.Blue;
            //testCounter.Text = profile.snrTestPeriod.ToString();
            SetResultDisplay(resultTable[0] as Label, ResultDisplayType.None);
            
        }

        private void CancelDownload()
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i].IsBusy)
                {
                    bkWorker[i].CancelAsync();
                }
            }

            if (profile.enableDownload)
            {
                startDownload.Enabled = true;
            } 
            TestRunning = TestStatus.Finished;
            SetResultDisplay(resultTable[0] as Label, ResultDisplayType.None);
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            if (TestRunning == TestStatus.Downloading)
            {
                //CancelDownload();
            }
            else
            {
                if (Global.functionType == Global.FunctionType.OpenPortTester)
                {
                    for (int i = 0; i < ModuleCount; i++)
                    {
                        if (bkWorker[i].IsBusy)
                        {
                            bkWorker[i].CancelAsync();
                        }
                    }
                }
                else
                {
                    if (waitingForm.Visible == true)
                    {
                        waitingForm.Hide();
                    }
                    CancelTest(true);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < ModuleCount; i++)
            {
                if (bkWorker[i] != null && bkWorker[i].IsBusy)
                {
                    MessageBox.Show("BackgroundWroker is still running!", "Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private const int DBT_DEVTYP_HANDLE = 6;
        private const int DBT_DEVTYP_PORT = 3;

        private const int BROADCAST_QUERY_DENY = 0x424D5144;
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000; // system detected a new device
        //private const int DBT_DEVICEQUERYREMOVE = 0x8001;   // Preparing to remove (any program can disable the removal)
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // removed 
        private const int DBT_DEVTYP_VOLUME = 0x00000002; // drive type is logical volume

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_PORT
        {
            public int dbcp_size;
            public int dbcp_devicetype;
            public int dbcp_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] dbcp_name;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options optForm = new Options();
            optForm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == optForm.ShowDialog())
            {
                ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;
                double[] gpSnrOffsetTable = {0, o.a1GpSnrOffset, o.a2GpSnrOffset, o.a3GpSnrOffset, o.a4GpSnrOffset, 
                                   o.b1GpSnrOffset, o.b2GpSnrOffset, o.b3GpSnrOffset, o.b4GpSnrOffset};
                double[] glSnrOffsetTable = {0, o.a1GlSnrOffset, o.a2GlSnrOffset, o.a3GlSnrOffset, o.a4GlSnrOffset, 
                                   o.b1GlSnrOffset, o.b2GlSnrOffset, o.b3GlSnrOffset, o.b4GlSnrOffset};
                double[] bdSnrOffsetTable = {0, o.a1BdSnrOffset, o.a2BdSnrOffset, o.a3BdSnrOffset, o.a4BdSnrOffset, 
                                   o.b1BdSnrOffset, o.b2BdSnrOffset, o.b3BdSnrOffset, o.b4BdSnrOffset};
                double[] giSnrOffsetTable = {0, o.a1GiSnrOffset, o.a2GiSnrOffset, o.a3GiSnrOffset, o.a4GiSnrOffset,
                                   o.b1GiSnrOffset, o.b2GiSnrOffset, o.b3GiSnrOffset, o.b4GiSnrOffset};

                for (int i = 0; i < ModuleCount; i++)
                {
                    testParam[i].gpSnrOffset = gpSnrOffsetTable[i];
                    testParam[i].glSnrOffset = glSnrOffsetTable[i];
                    testParam[i].bdSnrOffset = bdSnrOffsetTable[i];
                    testParam[i].giSnrOffset = giSnrOffsetTable[i];
                }
            }
        }

        private void hiddenNotify_TextChanged(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text == "WaitingCancel")
            {
                CancelTest(true);
            }
        }

        private void generateReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogSelectForm form = new LogSelectForm();
            if (DialogResult.OK != form.ShowDialog())
            {
                //this.Close();
                //return;
            }
        }

        private void errorMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String errorFile = null;
            //if (Global.functionType == Global.FunctionType.ModuleTest)
            //{
                errorFile = Login.loginInfo.currentPath + "\\" + "Error.txt";
            //}
            //else
            //{
            //    errorFile = resetTesterLogin.currentPath + "\\" + "Error.txt";
            //}

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(errorFile))
            {
                file.WriteLine("Error " + 0.ToString() + " : " + ((WorkerParam.ErrorType)((ulong)0)).ToString());

                for (int i = 1; i <= WorkerParam.ErrorCount; i++)
                {
                    file.WriteLine("Error " + i.ToString() + " : " + ((WorkerParam.ErrorType)((ulong)1 << i)).ToString());
                }

            }

            Process notePad = new Process();
            notePad.StartInfo.FileName = "notepad.exe";
            notePad.StartInfo.Arguments = errorFile;
            notePad.Start();
        }

        private static int[] cheatCode = { 0x26, 0x26, 0x28, 0x28, 
                0x25, 0x27, 0x25, 0x27, 0x42, 0x41 };
        private int cheats = 0;
        private bool continueMode = false;
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            if ((msg.Msg == WM_KEYDOWN))
            {
                if (msg.WParam.ToInt32() == cheatCode[cheats])
                {
                    if (++cheats == cheatCode.Length)
                    {   //Complete Cheat.
                        continueMode = (continueMode) ? false : true;
                        MessageBox.Show("Continue Mode " + ((continueMode) ? "ON" : "OFF"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cheats = 0;
                    }
                }
                else
                {
                    cheats = 0;
                }
            }
            return false;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private void PressStrsvrButton(bool isStart)
        {
            const int BM_CLICK = 0x00F5;
            IntPtr mainWnd = FindWindow("TMainForm", "STRSVR ver.2.4.3 b29");
            IntPtr childWnd = FindWindowEx(mainWnd, IntPtr.Zero, "TBitBtn", (isStart) ? "&Start" : "S&top");
            SendMessage((int)childWnd, BM_CLICK, 0, 0);
        }

        private void startDownload_Click(object sender, EventArgs e)
        {
            PressStrsvrButton(false);

            rp.NewSession(SessionReport.SessionType.Download);

            bool hasWork = false;
            TestModule.ResetTotalTestCount();
            TestModule.ResetDrMcuStatus();

            for (int i = 1; i < 5; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                snrChartTable[i].Invalidate();
                testParam[i].comPort = (comSelTable[i] as ComboBox).Text;
                testParam[i].profile = profile;
                testParam[i].log.Remove(0, testParam[i].log.Length);
                testParam[i].annIoPort = (anCtrlSel as ComboBox).Text;

                TestModule.IncreaseTotalTestCount();
                hasWork = true;
                SetResultDisplay(resultTable[i] as Label, ResultDisplayType.Downloading);
            }

            if (!hasWork)
            {
                ErrorMessage.Show(ErrorMessage.Warnings.NoDeviceSelectWarning);
                return;
            }

            if ((profile.enableSlaveDownload) && (anCtrlSel as ComboBox).Text == "")
            {
                MessageBox.Show("Please select MCU COM!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TestModule.ClearResult((profile.enableDownload && profile.twoUartDownload) ? TestModule.ClearType.Upper : TestModule.ClearType.All);
            EnableButton(false);
            TestRunning = TestStatus.Downloading;
            TestModule.antennaEvent.Set();
            for (int i = 1; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                bkWorker[i].RunWorkerAsync(testParam[i]);
                AddMessage(i, "-------------------- Begin Download --------------------");
            }

            startCounting = true;
            testTimer.Interval = 500;
            testTimer.Start();
        }

        private void startDownload2_Click(object sender, EventArgs e)
        {
            PressStrsvrButton(false);

            rp.NewSession(SessionReport.SessionType.Download);

            bool hasWork = false;
            //TestModule.ResetTotalTestCount();
            //TestModule.ResetDrMcuStatus();

            for (int i = 5; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                snrChartTable[i].Invalidate();
                testParam[i].comPort = (comSelTable[i] as ComboBox).Text;
                testParam[i].profile = profile;
                testParam[i].log.Remove(0, testParam[i].log.Length);
                testParam[i].annIoPort = (anCtrlSel as ComboBox).Text;

                //TestModule.IncreaseTotalTestCount();
                hasWork = true;
                SetResultDisplay(resultTable[i] as Label, ResultDisplayType.Downloading);
            }

            if (!hasWork)
            {
                ErrorMessage.Show(ErrorMessage.Warnings.NoDeviceSelectWarning);
                return;
            }

            //if ((profile.enableSlaveDownload) && (anCtrlSel as ComboBox).Text == "")
            //{
            //    MessageBox.Show("Please select MCU COM!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            TestModule.ClearResult(TestModule.ClearType.Bottom);
            EnableButton(false);
            TestRunning = TestStatus.Downloading;
            //TestModule.antennaEvent.Set();
            for (int i = 5; i < ModuleCount; i++)
            {
                if (!IsDeviceChecked(i))
                {
                    continue;
                }
                bkWorker[i].RunWorkerAsync(testParam[i]);
                AddMessage(i, "---------------- Begin Download slave ----------------");
            }

            startCounting = true;
            testTimer.Interval = 500;
            testTimer.Start();
        }
    }

    public class SessionReport
    {
        public enum SessionType { Testing, Download }

        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public SessionType sessionType { get; set; }

        public void NewSession(SessionType s)
        {
            sessionType = s;
            startTime = DateTime.Now;
        }
        public void EndSession()
        {
            endTime = DateTime.Now;
        }

    }
}
