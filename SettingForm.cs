using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace ModuleTestV8
{
    public partial class SettingForm : Form
    {
        private ModuleTestProfile profile;
        public SettingForm()
        {
            InitializeComponent();
            profile = new ModuleTestProfile();
        }

        public SettingForm(ModuleTestProfile p)
        {
            InitializeComponent();
            profile = p;
        }

        private void InitNoMotorSettingForm()
        {
            testInsDrGyro.Visible = true;
            drPanel.Visible = true;
            //drPanel.Left = 14;
            //drPanel.Top = 324;
        }

        private void InitNormalSettingForm()
        {
            testInsDrGyro.Visible = false;
            drPanel.Visible = false;
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            if (Global.functionType == Global.FunctionType.ModuleTestDr)
                InitNoMotorSettingForm();
            else
                InitNormalSettingForm();

            Global.InjectionBaudRate(gdBaudSel);
            Global.InjectionBaudRate(dlBaudSel);

            String iniFile = Environment.CurrentDirectory + "\\Module.ini";
            List<String> rGps = new List<String>();
            List<String> rGlonass = new List<String>();
            List<String> rBeidou = new List<String>();
            //List<String> rGalileo = new List<String>();
            List<String> rNavic = new List<String>();

            ModuleIniParser.ErrorCode er = ModuleIniParser.Load(iniFile, ref rGps, ref rGlonass, ref rBeidou, ref rNavic);
            if(er==ModuleIniParser.ErrorCode.NoGpsModule)
            {
                ErrorMessage.Show(ErrorMessage.Errors.NoGpsModule);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            InjectionModule(gpModuleSel, rGps);
            InjectionModule(glModuleSel, rGlonass);
            InjectionModule(bdModuleSel, rBeidou);
            //InjectionModule(gaModuleSel, rGalileo);
            InjectionModule(giModuleSel, rNavic);

            AdjustUIByProfile();
            UpdateStstus();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            profile.gdBaudSel = gdBaudSel.SelectedIndex;
            DialogResult = DialogResult.OK;
            Close();
        }

        //private void InjectionPassCriteria(ComboBox c)
        //{
        //    c.Items.AddRange(ModuleTestProfile.CriteriaStrings);
        //}

        private void InjectionModule(ComboBox c, List<String> s)
        {
            foreach (String m in s)
            {
                c.Items.Add(m);
            }
        }

        public enum ModuleTypes { GpsModule, GlonassModule, BeidouModule, NavicModule, GalileoModule }
        private void AdjustUIByProfile()
        {
            //Golden Sample
            gdBaudSel.SelectedIndex = profile.gdBaudSel;

            //Download Setting
            enableDownload.Checked = profile.enableDownload;
            dlBaudSel.SelectedIndex = profile.dlBaudSel;
            enableSlaveDownload.Checked = profile.enableSlaveDownload;
            twoUartDownload.Checked = profile.twoUartDownload;
            slaveIniFileName.Text = profile.slaveIniFileName;

            //Test Module
            gpModuleSel.SelectedIndex = profile.gpModuleSel;
            if (glModuleSel.Items.Count > 0)
            {
                glModuleSel.SelectedIndex = profile.glModuleSel;
            }
            else
            {
                glModule.Enabled = false;
            }

            if (bdModuleSel.Items.Count > 0)
            {
                bdModuleSel.SelectedIndex = profile.bdModuleSel;
            }
            else
            {
                bdModule.Enabled = false;
            }

            if (giModuleSel.Items.Count > 0)
            {
                giModuleSel.SelectedIndex = profile.giModuleSel;
            }
            else
            {
                giModule.Enabled = false;
            }

            switch ((ModuleTypes)profile.moduleType)
            {
                case ModuleTypes.GpsModule:
                    gpModule.Checked = true;
                    break;
                case ModuleTypes.GlonassModule:
                    glModule.Checked = true;
                    break;
                case ModuleTypes.BeidouModule:
                    bdModule.Checked = true;
                    break;
                case ModuleTypes.NavicModule:
                    giModule.Checked = true;
                    break;
            }

            //Test Module Setting
            iniFileName.Text = profile.iniFileName;
            snrTestPeriod.Text = profile.snrTestPeriod.ToString();

            gpPassCriteria.Checked = profile.testGpSnr;
            glPassCriteria.Checked = profile.testGlSnr;
            bdPassCriteria.Checked = profile.testBdSnr;
            giPassCriteria.Checked = profile.testGiSnr;

            gpSnrUpper.Text = profile.gpSnrUpper.ToString();
            gpSnrLower.Text = profile.gpSnrLower.ToString();
            glSnrUpper.Text = profile.glSnrUpper.ToString();
            glSnrLower.Text = profile.glSnrLower.ToString();
            bdSnrUpper.Text = profile.bdSnrUpper.ToString();
            bdSnrLower.Text = profile.bdSnrLower.ToString();
            giSnrUpper.Text = profile.giSnrUpper.ToString();
            giSnrLower.Text = profile.giSnrLower.ToString();

            gpSnrLimit.Text = profile.gpSnrLimit.ToString();
            glSnrLimit.Text = profile.glSnrLimit.ToString();
            bdSnrLimit.Text = profile.bdSnrLimit.ToString();
            giSnrLimit.Text = profile.giSnrLimit.ToString();

            //Test Items
            checkPromCrc.Checked = profile.checkPromCrc;
            checkSlavePromCrc.Checked = profile.checkSlavePromCrc;
            waitPositionFix.Checked = profile.waitPositionFix;
            checkRtc.Checked = profile.checkRtc;
            testClockOffset.Checked = profile.testClockOffset;
            clockOffsetThreshold.Text = profile.clockOffsetThreshold.ToString();
            writeClockOffset.Checked = profile.writeClockOffset;

            testIo.Checked = profile.testIo;
            ioTypeCombo.SelectedIndex = (int)profile.testIoType;
            testAntenna.Checked = profile.testAntenna;
            testUart2TxRx.Checked = profile.testUart2TxRx;
            testVoltage.Checked = profile.testVoltage;
            voltageLower.Text = profile.volrageLower.ToString();
            voltageUpper.Text = profile.volrageUpper.ToString();
            testEcompass.Checked = profile.testEcompass;
            testMiniHommer.Checked = profile.testMiniHommer;
            testDrCyro.Checked = profile.testDrCyro;
            useSensor.Checked = profile.useSensor;
            uslClockWise.Text = profile.uslClockWise.ToString();
            uslAnticlockWise.Text = profile.uslAnticlockWise.ToString();
            lslClockWise.Text = profile.lslClockWise.ToString();
            lslAnticlockWise.Text = profile.lslAnticlockWise.ToString();
            //INS-DR new items
            testInsDrGyro.Checked = profile.testInsDrGyro;
            insDrGyroLower.Text = profile.insDrGyroLower.ToString();
            insDrGyroUpper.Text = profile.insDrGyroUpper.ToString();

            testAcc.Checked = profile.testAcc;
            accXUpper.Text = profile.accXUpper.ToString();
            accXLower.Text = profile.accXLower.ToString();
            accYUpper.Text = profile.accYUpper.ToString();
            accYLower.Text = profile.accYLower.ToString();
            accZUpper.Text = profile.accZUpper.ToString();
            accZLower.Text = profile.accZLower.ToString();

            testBaro.Checked = profile.testBaro;
            pressureCriteria.Text = profile.pressureCriteria.ToString();
            tempCriteria.Text = profile.tempCriteria.ToString();
            skipSpdDir.Checked = profile.skipSpdDir;
            //Support reverse roation
            reverseRotation.Checked = profile.reverseRotation;
            //For V827 module S1216DR8P, S1216RTK
            //testToRtkFloat.Checked = profile.testToRtkFloat;
            //testToRtkFix.Checked = profile.testToRtkFix;
            testFixedType.SelectedIndex = (int)profile.testFixedType;
        }

        private void UpdateStstus()
        {
            if (gpModule.Checked)
            {
                gpModuleSel.Enabled = true;
                glModuleSel.Enabled = false;
                bdModuleSel.Enabled = false;
                giModuleSel.Enabled = false;
                moduleName.Text = gpModuleSel.SelectedItem.ToString();
            }
            if (glModule.Checked)
            {
                gpModuleSel.Enabled = false;
                glModuleSel.Enabled = true;
                bdModuleSel.Enabled = false;
                giModuleSel.Enabled = false;
                moduleName.Text = glModuleSel.SelectedItem.ToString();
            }
            if (bdModule.Checked)
            {
                gpModuleSel.Enabled = false;
                glModuleSel.Enabled = false;
                bdModuleSel.Enabled = true;
                giModuleSel.Enabled = false;
                moduleName.Text = bdModuleSel.SelectedItem.ToString();
            }
            if (giModule.Checked)
            {
                gpModuleSel.Enabled = false;
                glModuleSel.Enabled = false;
                bdModuleSel.Enabled = false;
                giModuleSel.Enabled = true;
                moduleName.Text = giModuleSel.SelectedItem.ToString();
            }

            gpSnrUpper.Enabled = gpPassCriteria.Checked;
            gpSnrLower.Enabled = gpPassCriteria.Checked;
            gpSnrLimit.Enabled = gpPassCriteria.Checked;

            glSnrUpper.Enabled = glPassCriteria.Checked;
            glSnrLower.Enabled = glPassCriteria.Checked;
            glSnrLimit.Enabled = glPassCriteria.Checked;

            bdSnrUpper.Enabled = bdPassCriteria.Checked;
            bdSnrLower.Enabled = bdPassCriteria.Checked;
            bdSnrLimit.Enabled = bdPassCriteria.Checked;

            giSnrUpper.Enabled = giPassCriteria.Checked;
            giSnrLower.Enabled = giPassCriteria.Checked;
            giSnrLimit.Enabled = giPassCriteria.Checked;

            dlBaudSel.Enabled = enableDownload.Checked;
            slaveIniFileName.Enabled = enableSlaveDownload.Checked;
            browseSlaveIni.Enabled = enableSlaveDownload.Checked;
            twoUartDownload.Enabled = enableSlaveDownload.Checked;
            ioTypeCombo.Enabled = testIo.Checked;
            clockOffsetThreshold.Enabled = testClockOffset.Checked;
            writeClockOffset.Enabled = testClockOffset.Checked;
            voltageLower.Enabled = testVoltage.Checked;
            voltageUpper.Enabled = testVoltage.Checked;
            uslClockWise.Enabled = testDrCyro.Checked;
            uslAnticlockWise.Enabled = testDrCyro.Checked;
            lslClockWise.Enabled = testDrCyro.Checked;
            lslAnticlockWise.Enabled = testDrCyro.Checked;
            useSensor.Enabled = testDrCyro.Checked;
            insDrGyroLower.Enabled = testInsDrGyro.Checked;
            insDrGyroUpper.Enabled = testInsDrGyro.Checked;

            accXUpper.Enabled = testAcc.Checked;
            accXLower.Enabled = testAcc.Checked;
            accYUpper.Enabled = testAcc.Checked;
            accYLower.Enabled = testAcc.Checked;
            accZUpper.Enabled = testAcc.Checked;
            accZLower.Enabled = testAcc.Checked;

            pressureCriteria.Enabled = testBaro.Checked;
            tempCriteria.Enabled = testBaro.Checked;
            //Support reverse roation
            reverseRotation.Enabled = testDrCyro.Checked;
        }

        private void gpModule_CheckedChanged(object sender, EventArgs e)
        {
            profile.moduleType = (int)ModuleTypes.GpsModule;
            UpdateStstus();
        }

        private void glModule_CheckedChanged(object sender, EventArgs e)
        {
            profile.moduleType = (int)ModuleTypes.GlonassModule;
            UpdateStstus();
        }

        private void bdModule_CheckedChanged(object sender, EventArgs e)
        {
            profile.moduleType = (int)ModuleTypes.BeidouModule;
            UpdateStstus();
        }

        private void giModule_CheckedChanged(object sender, EventArgs e)
        {
            profile.moduleType = (int)ModuleTypes.NavicModule;
            UpdateStstus();
        }

        private void gpModuleSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == profile.gpModuleSel)
            {
                return;
            }
            profile.gpModuleSel = ((ComboBox)sender).SelectedIndex;
            moduleName.Text = gpModuleSel.SelectedItem.ToString();
        }

        private void glModuleSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == profile.glModuleSel)
            {
                return;
            }
            profile.glModuleSel = ((ComboBox)sender).SelectedIndex;
            moduleName.Text = glModuleSel.SelectedItem.ToString();
        }

        private void bdModuleSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == profile.bdModuleSel)
            {
                return;
            }
            profile.bdModuleSel = ((ComboBox)sender).SelectedIndex;
            moduleName.Text = bdModuleSel.SelectedItem.ToString();
        }

        private void giModuleSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == profile.giModuleSel)
            {
                return;
            }
            profile.giModuleSel = ((ComboBox)sender).SelectedIndex;
            moduleName.Text = giModuleSel.SelectedItem.ToString();
        }

        private void moduleName_TextChanged(object sender, EventArgs e)
        {
            profile.moduleName = moduleName.Text;
        }

        private void gdBaudSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            profile.gdBaudSel = (sender as ComboBox).SelectedIndex;
        }

        private void dlBaudSel_SelectedIndexChanged(object sender, EventArgs e)
        {
            profile.dlBaudSel = (sender as ComboBox).SelectedIndex;
        }

        private void gpPassCriteria_CheckedChanged(object sender, EventArgs e)
        {
            profile.testGpSnr = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void glPassCriteria_CheckedChanged(object sender, EventArgs e)
        {
            profile.testGlSnr = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void bdPassCriteria_CheckedChanged(object sender, EventArgs e)
        {
            profile.testBdSnr = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void giPassCriteria_CheckedChanged(object sender, EventArgs e)
        {
            profile.testGiSnr = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void snrTestPeriod_TextChanged(object sender, EventArgs e)
        {
            profile.snrTestPeriod = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private void gpSnrLimit_TextChanged(object sender, EventArgs e)
        {
            profile.gpSnrLimit = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private void glSnrLimit_TextChanged(object sender, EventArgs e)
        {
            profile.glSnrLimit = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private void bdSnrLimit_TextChanged(object sender, EventArgs e)
        {
            profile.bdSnrLimit = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private void giSnrLimit_TextChanged(object sender, EventArgs e)
        {
            profile.giSnrLimit = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private int GetTextBoxSnrInt(TextBox t)
        {
            int value = 0;
            try
            {
                value = Convert.ToInt32(t.Text);
                t.ForeColor = (value >= -999 || value <= 999) ? Color.Black : Color.Red;
            }
            catch
            {
                t.ForeColor = Color.Red;
            }
            return value;
        }

        private void testIo_CheckedChanged(object sender, EventArgs e)
        {
            profile.testIo = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void testAntenna_CheckedChanged(object sender, EventArgs e)
        {
            profile.testAntenna = (sender as CheckBox).Checked;
        }

        private void testUart2TxRx_CheckedChanged(object sender, EventArgs e)
        {
            profile.testUart2TxRx = (sender as CheckBox).Checked;
        }

        private void iniFileName_TextChanged(object sender, EventArgs e)
        {
            profile.iniFileName = (sender as TextBox).Text;
        }

        private void browseIni_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = Login.loginInfo.currentPath;
            openFileDlg.Filter = "ini files (*.ini)|*.ini|All files (*.*)|*.*";
            openFileDlg.FilterIndex = 1;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                //It will crash in Test PC (3F).
                //iniFileName.Text = openFileDlg.SafeFileName;
                iniFileName.Text = Path.GetFileName(openFileDlg.FileName);
            }
        }

        private void enableDownload_CheckedChanged(object sender, EventArgs e)
        {
            profile.enableDownload = (sender as CheckBox).Checked;
            UpdateStstus();
        }    

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void testClockOffset_CheckedChanged(object sender, EventArgs e)
        {
            profile.testClockOffset = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void clockOffsetThreshold_TextChanged(object sender, EventArgs e)
        {
            profile.clockOffsetThreshold = Global.GetTextBoxPositiveDouble((TextBox)sender);
        }

        private void writeClockOffset_CheckedChanged(object sender, EventArgs e)
        {
            profile.writeClockOffset = (sender as CheckBox).Checked;
        }

        private void testEcompass_CheckedChanged(object sender, EventArgs e)
        {
            profile.testEcompass = (sender as CheckBox).Checked;
        }

        private void testMiniHommer_CheckedChanged(object sender, EventArgs e)
        {
            profile.testMiniHommer = (sender as CheckBox).Checked;
        }

        private void testDrCyro_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked && profile.testInsDrGyro)
            {
                MessageBox.Show("\"Test DR Gyro\" and \"Tes INS DR Gyro\" can not be checked at the same time");
                (sender as CheckBox).Checked = false;
                return;
            }
            profile.testDrCyro = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void testDrDuration_TextChanged(object sender, EventArgs e)
        {
            profile.testDrDuration = Global.GetTextBoxPositiveInt(sender as TextBox);
        }

        private void uslClockWise_TextChanged(object sender, EventArgs e)
        {
            profile.uslClockWise = Global.GetTextBoxPositiveDouble(sender as TextBox);
        }

        private void uslAnticlockWise_TextChanged(object sender, EventArgs e)
        {
            profile.uslAnticlockWise = Global.GetTextBoxPositiveDouble(sender as TextBox);
        }

        private void lslClockWise_TextChanged(object sender, EventArgs e)
        {
            profile.lslClockWise = Global.GetTextBoxPositiveDouble(sender as TextBox);
        }

        private void lslAnticlockWise_TextChanged(object sender, EventArgs e)
        {
            profile.lslAnticlockWise = Global.GetTextBoxPositiveDouble(sender as TextBox);
        }

        private void thresholdCogWise_TextChanged(object sender, EventArgs e)
        {
            profile.thresholdCog = Global.GetTextBoxPositiveDouble(sender as TextBox);
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDlg = new SaveFileDialog();

            saveFileDlg.InitialDirectory = Login.loginInfo.currentPath;
            saveFileDlg.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
            saveFileDlg.FilterIndex = 1;
            saveFileDlg.RestoreDirectory = false;
            saveFileDlg.FileName = ModuleTestForm.DefaultProfileName;

            if (saveFileDlg.ShowDialog() == DialogResult.OK)
            {
                profile.SaveToIniFile(saveFileDlg.FileName);
            }
        }

        private bool CheckProfileValidity(ModuleTestProfile p)
        {
            if (p.gpModuleSel > gpModuleSel.Items.Count)
            {
                ErrorMessage.Show(ErrorMessage.Errors.InvalidGpsModule);
                return false;
            }
            if (p.glModuleSel > glModuleSel.Items.Count)
            {
                ErrorMessage.Show(ErrorMessage.Errors.InvalidGlonassModule);
                return false;
            }
            if (p.bdModuleSel > bdModuleSel.Items.Count)
            {
                ErrorMessage.Show(ErrorMessage.Errors.InvalidBeidouModule);
                return false;
            }
            if (p.giModuleSel > giModuleSel.Items.Count)
            {
                ErrorMessage.Show(ErrorMessage.Errors.InvalidNavicModule);
                return false;
            }
            //if (p.gaModuleSel > gaModuleSel.Items.Count)
            //{
            //    ErrorMessage.Show(ErrorMessage.Errors.InvalidGalileoModule);
            //    return false;
            //}
            if ((ModuleTypes.GpsModule == (ModuleTypes)p.moduleType && 
                    0 == gpModuleSel.Items.Count) || 
                (ModuleTypes.GlonassModule == (ModuleTypes)p.moduleType && 
                    0 == glModuleSel.Items.Count) ||                
                (ModuleTypes.BeidouModule == (ModuleTypes)p.moduleType && 
                    0 == bdModuleSel.Items.Count) /*||                
                (ModuleTypes.GalileoModule == (ModuleTypes)p.moduleType && 
                    0 == gaModuleSel.Items.Count)*/ )             
            {
                ErrorMessage.Show(ErrorMessage.Errors.ProfileHasInvalidModule);
                return false;
            }
            return true;
        }

        private static bool CheckProfileValidityInModule(ModuleTestProfile p, bool silent)
        {
            String iniFile = Environment.CurrentDirectory + "\\Module.ini";
            List<String> rGps = new List<String>();
            List<String> rGlonass = new List<String>();
            List<String> rBeidou = new List<String>();
            List<String> rNavic = new List<String>();

            ModuleIniParser.ErrorCode er = ModuleIniParser.Load(iniFile, ref rGps, ref rGlonass, ref rBeidou, ref rNavic);
            if (er == ModuleIniParser.ErrorCode.NoGpsModule)
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.NoGpsModule);
                    MessageBox.Show(iniFile);
                }
                return false;
            }
            if (p.gpModuleSel > rGps.Count)
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.InvalidGpsModule);
                }
                return false;
            }
            if (p.glModuleSel > rGlonass.Count)
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.InvalidGlonassModule);
                }
                return false;
            }
            if (p.bdModuleSel > rBeidou.Count)
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.InvalidBeidouModule);
                }
                return false;
            }
            if (p.giModuleSel > rNavic.Count)
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.InvalidNavicModule);
                }
                return false;
            }

            if ((ModuleTypes.GpsModule == (ModuleTypes)p.moduleType &&
                    0 == rGps.Count) ||
                (ModuleTypes.GlonassModule == (ModuleTypes)p.moduleType &&
                    0 == rGlonass.Count) ||
                (ModuleTypes.BeidouModule == (ModuleTypes)p.moduleType &&
                    0 == rBeidou.Count) ||
                (ModuleTypes.NavicModule == (ModuleTypes)p.moduleType &&
                    0 == rNavic.Count))
            {
                if (!silent)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.ProfileHasInvalidModule);
                }
                return false;
            }
            return true;
        }

        public static ModuleTestProfile LoadAndCheckProfile(String path, bool silent)
        {
            ModuleTestProfile p = new ModuleTestProfile();
            if (!p.LoadFromIniFile(path))
            {
                if (!silent && p.error == ModuleTestProfile.ErrorCode.InvalidateFormat)
                {
                    ErrorMessage.Show(ErrorMessage.Errors.ProfileFormatError);
                }
                return null;
            }
            if (!CheckProfileValidityInModule(p, silent))
            {
                return null;
            }
            return p;
        }

        private void loadFrom_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();

            openFileDlg.InitialDirectory = Login.loginInfo.currentPath;
            openFileDlg.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
            openFileDlg.FilterIndex = 1;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ModuleTestProfile p = LoadAndCheckProfile(openFileDlg.FileName, false);
            if (p == null)
            {
                return;
            }
            profile = p;
            AdjustUIByProfile();
        }

        private void gpSnrLower_TextChanged(object sender, EventArgs e)
        {
            profile.gpSnrLower = GetTextBoxSnrInt(sender as TextBox);
        }

        private void glSnrLower_TextChanged(object sender, EventArgs e)
        {
            profile.glSnrLower = GetTextBoxSnrInt(sender as TextBox);
        }

        private void bdSnrLower_TextChanged(object sender, EventArgs e)
        {
            profile.bdSnrLower = GetTextBoxSnrInt(sender as TextBox);
        }

        private void giSnrLower_TextChanged(object sender, EventArgs e)
        {
            profile.giSnrLower = GetTextBoxSnrInt(sender as TextBox);
        }

        //private void gaSnrLower_TextChanged(object sender, EventArgs e)
        //{
        //    profile.gaSnrLower = GetTextBoxSnrInt(sender as TextBox);
        //}

        private void gpSnrUpper_TextChanged(object sender, EventArgs e)
        {
            profile.gpSnrUpper = GetTextBoxSnrInt(sender as TextBox);
        }

        private void glSnrUpper_TextChanged(object sender, EventArgs e)
        {
            profile.glSnrUpper = GetTextBoxSnrInt(sender as TextBox);
        }

        private void bdSnrUpper_TextChanged(object sender, EventArgs e)
        {
            profile.bdSnrUpper = GetTextBoxSnrInt(sender as TextBox);
        }

        private void giSnrUpper_TextChanged(object sender, EventArgs e)
        {
            profile.giSnrUpper = GetTextBoxSnrInt(sender as TextBox);
        }

        //private void gaSnrUpper_TextChanged(object sender, EventArgs e)
        //{
        //    profile.gaSnrUpper = GetTextBoxSnrInt(sender as TextBox);
        //}

        private void checkPromCrc_CheckedChanged(object sender, EventArgs e)
        {
            profile.checkPromCrc = (sender as CheckBox).Checked;
        }

        private void testRtc_CheckedChanged(object sender, EventArgs e)
        {
            profile.checkRtc = (sender as CheckBox).Checked;
        }

        private void waitPositionFix_CheckedChanged(object sender, EventArgs e)
        {
            profile.waitPositionFix = (sender as CheckBox).Checked;
        }        
        
        private void ioTypeChk_SelectedIndexChanged(object sender, EventArgs e)
        {
            profile.testIoType = (ModuleTestProfile.IoType)(sender as ComboBox).SelectedIndex;
        }

        private void useSensor_CheckedChanged(object sender, EventArgs e)
        {
            profile.useSensor = (sender as CheckBox).Checked;
        }

        private void testVoltage_CheckedChanged(object sender, EventArgs e)
        {
            profile.testVoltage = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void voltageLower_TextChanged(object sender, EventArgs e)
        {
            profile.volrageLower = Global.GetTextBoxPositiveDouble((TextBox)sender);
        }

        private void voltageUpper_TextChanged(object sender, EventArgs e)
        {
            profile.volrageUpper = Global.GetTextBoxPositiveDouble((TextBox)sender);
        }

        private void testInsDrGyro_CheckedChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked && profile.testDrCyro)
            {
                MessageBox.Show("\"Test DR Gyro\" and \"Tes INS DR Gyro\" can not be checked at the same time");
                (sender as CheckBox).Checked = false;
                return;
            }
            profile.testInsDrGyro = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void testBaro_CheckedChanged(object sender, EventArgs e)
        {
            profile.testBaro = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void skipSpdDir_CheckedChanged(object sender, EventArgs e)
        {
            profile.skipSpdDir = (sender as CheckBox).Checked;
        }

        private void insDrGyroLower_TextChanged(object sender, EventArgs e)
        {
            profile.insDrGyroLower = Global.GetTextBoxInt((TextBox)sender);
        }

        private void insDrGyroUpper_TextChanged(object sender, EventArgs e)
        {
            profile.insDrGyroUpper = Global.GetTextBoxInt((TextBox)sender);
        }

        private void testAcc_CheckedChanged(object sender, EventArgs e)
        {
            profile.testAcc = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void accXUpper_TextChanged(object sender, EventArgs e)
        {
            profile.accXUpper = Global.GetTextBoxInt((TextBox)sender);
        }

        private void accXLower_TextChanged(object sender, EventArgs e)
        {
            profile.accXLower = Global.GetTextBoxInt((TextBox)sender);
        }

        private void accYUpper_TextChanged(object sender, EventArgs e)
        {
            profile.accYUpper = Global.GetTextBoxInt((TextBox)sender);
        }

        private void accYLower_TextChanged(object sender, EventArgs e)
        {
            profile.accYLower = Global.GetTextBoxInt((TextBox)sender);
        }

        private void accZUpper_TextChanged(object sender, EventArgs e)
        {
            profile.accZUpper = Global.GetTextBoxInt((TextBox)sender);
        }

        private void accZLower_TextChanged(object sender, EventArgs e)
        {
            profile.accZLower = Global.GetTextBoxInt((TextBox)sender);
        }

        //private void baroLower_TextChanged(object sender, EventArgs e)
        //{
        //    profile.baroLower = Global.GetTextBoxInt((TextBox)sender);
        //}

        //private void baroUpper_TextChanged(object sender, EventArgs e)
        //{
        //    profile.baroUpper = Global.GetTextBoxInt((TextBox)sender);
        //}

        private void browseSlaveIni_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = Login.loginInfo.currentPath;
            openFileDlg.Filter = "ini files (*.ini)|*.ini|All files (*.*)|*.*";
            openFileDlg.FilterIndex = 1;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                //It will crash in Test PC (3F).
                //iniFileName.Text = openFileDlg.SafeFileName;
                slaveIniFileName.Text = Path.GetFileName(openFileDlg.FileName);
            }
        }

        //private void testToRtkFloat_CheckedChanged(object sender, EventArgs e)
        //{
        //    profile.testToRtkFloat = (sender as CheckBox).Checked;
        //}

        //private void testToRtkFix_CheckedChanged(object sender, EventArgs e)
        //{
        //    profile.testToRtkFix= (sender as CheckBox).Checked;
        //}

        private void testFixedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            profile.testFixedType = (ModuleTestProfile.TestFixType)(sender as ComboBox).SelectedIndex;
        }

        private void enableSlaveDownload_CheckedChanged(object sender, EventArgs e)
        {
            profile.enableSlaveDownload = (sender as CheckBox).Checked;
            UpdateStstus();
        }

        private void slaveIniFileName_TextChanged(object sender, EventArgs e)
        {
            profile.slaveIniFileName = (sender as TextBox).Text;
        }

        private void checkSlavePromCrc_CheckedChanged(object sender, EventArgs e)
        {
            profile.checkSlavePromCrc = (sender as CheckBox).Checked;
        }

        private void pressureCriteria_TextChanged(object sender, EventArgs e)
        {
            profile.pressureCriteria = Global.GetTextBoxPositiveInt((TextBox)sender);
        }

        private void tempCriteria_TextChanged(object sender, EventArgs e)
        {
            profile.tempCriteria = Global.GetTextBoxPositiveDouble((TextBox)sender);
        }

        private void reverseRotation_CheckedChanged(object sender, EventArgs e)
        {
            profile.reverseRotation = (sender as CheckBox).Checked;
        }

        private void twoUartDownload_CheckedChanged(object sender, EventArgs e)
        {
            profile.twoUartDownload = (sender as CheckBox).Checked;
            UpdateStstus();
        }
    }
}
