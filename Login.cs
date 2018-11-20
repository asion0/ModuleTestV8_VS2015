using System;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace ModuleTestV8
{
    public partial class Login : Form
    {
        public class LoginInfo
        {
            public String TesterNumber { get; set; }
            public String WorkFormNumber { get; set; }
            public bool FirstTest { get; set; }
            public bool DebugMode { get; set; }
            public int FixtureNumber { get; set; }
            public DateTime loginTime { get; set; }
            public String currentPath;
            public String logFile;

            public bool GenerateXml(ref XmlElement item, XmlDocument doc)
            {
                XmlElement itemData = doc.CreateElement("ItemData");
                itemData.SetAttribute("TN", TesterNumber.ToString());
                itemData.SetAttribute("WN", WorkFormNumber.ToString());
                itemData.SetAttribute("FT", FirstTest.ToString());
                itemData.SetAttribute("FN", FixtureNumber.ToString());
                itemData.SetAttribute("LT", loginTime.ToString("u"));
                item.AppendChild(itemData);

                Crc32 crc32 = new Crc32();
                XmlElement itemKey = doc.CreateElement("ItemKey");
                itemKey.SetAttribute("Key", crc32.ComputeChecksum(itemData.OuterXml).ToString());
                item.AppendChild(itemKey);

                return true;
            }
        }

        public static LoginInfo loginInfo = new LoginInfo();
        private ModuleTestProfile profile;
        public ModuleTestProfile Profile() { return profile; }

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            AcceptButton = ok;
            CancelButton = cancel;

            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                Login.loginInfo.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                Login.loginInfo.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            }
            //MessageBox.Show(Login.loginInfo.currentPath);

            for(int i = 1; i <= 20; i++)
            {
                fixtureNo.Items.Add(i.ToString());
            }
            fixtureNo.SelectedIndex = ModuleTestV8.Properties.Settings.Default.FixtureNumber;
            firstTest.Checked = true;
            testerNo.Text = ModuleTestV8.Properties.Settings.Default.TesterNumber;

            String profile = Login.loginInfo.currentPath + "\\" + ModuleTestForm.DefaultProfileName;
            if (File.Exists(profile))
            {
                profilePath.Text = profile;
            }
            //Alex add for test
            //testBtn.Visible = true;
        }

        private void profileSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();

            openFileDlg.InitialDirectory = Login.loginInfo.currentPath;
            openFileDlg.Filter = "dat files (*.dat)|*.dat";
            openFileDlg.FilterIndex = 1;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                profilePath.Text = openFileDlg.FileName;
            }

        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (!CheckWorkNo())
            {
                ErrorMessage.Show(ErrorMessage.Errors.WrongWorkingNo);
                return;
            }

            if (!File.Exists(profilePath.Text))
            {
                ErrorMessage.Show(ErrorMessage.Errors.NoProfileError);
                return;
            }

            ModuleTestProfile p = SettingForm.LoadAndCheckProfile(profilePath.Text, false);
            if (p == null)
            {
                return;
            }
            profile = p;
            loginInfo.WorkFormNumber = workNo.Text;
            loginInfo.TesterNumber = testerNo.Text;

            ModuleTestV8.Properties.Settings.Default.TesterNumber = testerNo.Text;
            ModuleTestV8.Properties.Settings.Default.FixtureNumber = loginInfo.FixtureNumber;
            ModuleTestV8.Properties.Settings.Default.Save();

            loginInfo.loginTime = DateTime.Now;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool CheckWorkNo()
        {
            String s = workNo.Text;
            //A511-10201020001
            if (s.Length != 16 || s[4] != '-')
            {
                return false;
            }
            return true;
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void debugMode_Click(object sender, EventArgs e)
        {
            Password pwdForm = new Password();
            pwdForm.StartPosition = FormStartPosition.CenterParent;
            if (DialogResult.OK == pwdForm.ShowDialog())
            {
                loginInfo.DebugMode = true;
                bool hasProfile = false;
                hasProfile = File.Exists(profilePath.Text);

                if(hasProfile)
                {
                    ModuleTestProfile p = SettingForm.LoadAndCheckProfile(profilePath.Text, true);
                    if (p != null)
                    {
                        profile = p;
                    }
                }

                if(profile == null)
                {
                    ErrorMessage.Show(ErrorMessage.Warnings.NoProfileWarning);
                }

                loginInfo.WorkFormNumber = workNo.Text;
                loginInfo.TesterNumber = testerNo.Text;
                loginInfo.loginTime = DateTime.Now;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
        }

        private void firstTest_CheckedChanged(object sender, EventArgs e)
        {
            if( (sender as RadioButton).Checked)
            {
                loginInfo.FirstTest = true;
            }
        }

        private void rework_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                loginInfo.FirstTest = false;
            }
        }

        private void fixtureNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            loginInfo.FixtureNumber = (sender as ComboBox).SelectedIndex;
        }

        private void test_Click(object sender, EventArgs e)
        {
            //while (Test())
            //{
                
            //};
            //return;

            //LogSelectForm form = new LogSelectForm();
            //if (DialogResult.OK != form.ShowDialog())
            //{
            //    //this.Close();
            //    //return;
            //}
        }

        private bool Test()
        {
            OpenFileDialog ofdOpen = new System.Windows.Forms.OpenFileDialog();
            if ((ofdOpen.InitialDirectory == null) || (ofdOpen.InitialDirectory == string.Empty))
            {
            //    ofdOpen.InitialDirectory = "D:\\Firmware"; 
            }

            ofdOpen.Filter =
                            "Firmware files (*.bin)|*.bin|" +
                            "All Files (*.*)|*.*";
            ofdOpen.Title = "Open firmware binary file";
            ofdOpen.Multiselect = false; 

            if (ofdOpen.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
            {
                return false;
            }
            string filename = ofdOpen.FileName; //
            var fs = new FileStream(filename, FileMode.Open);
            var len = (int)fs.Length;

            int crc = 0;
            for (int i = 0; i < 0x80000 * 2; ++i)
            {
                if (i < len)
                    crc += fs.ReadByte();
                else
                    crc += 0xff;
                crc &= 0xffff;
            }
            fs.Close();
            MessageBox.Show(crc.ToString("X4"));
            return true;
        }
    }
}
