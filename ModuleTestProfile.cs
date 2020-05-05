using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;

namespace ModuleTestV8
{
    public class Crc32
    {
        uint[] table;

        public uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public uint ComputeChecksum(String s)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(s);
            return ComputeChecksum(bytes);
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        public Crc32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }
    }

    public class ModuleTestProfile
    {
        public int gdBaudSel { get; set; }
        //Module
        public int moduleType { get; set; }
        public int gpModuleSel { get; set; }
        public int glModuleSel { get; set; }
        public int bdModuleSel { get; set; }
        public int gaModuleSel { get; set; }
        public int giModuleSel { get; set; }
        public String moduleName { get; set; }
        //Device
        public String iniFileName { get; set; }
        public String slaveIniFileName { get; set; }

        public int snrTestPeriod { get; set; }
        public bool testGpSnr { get; set; }
        public bool testGlSnr { get; set; }
        public bool testBdSnr { get; set; }
        public bool testGaSnr { get; set; }
        public bool testGiSnr { get; set; }

        public int gpSnrUpper { get; set; }
        public int gpSnrLower { get; set; }
        public int glSnrUpper { get; set; }
        public int glSnrLower { get; set; }
        public int bdSnrUpper { get; set; }
        public int bdSnrLower { get; set; }
        public int gaSnrUpper { get; set; }
        public int gaSnrLower { get; set; }
        public int giSnrUpper { get; set; }
        public int giSnrLower { get; set; }

        public int gpSnrLimit { get; set; }
        public int glSnrLimit { get; set; }
        public int bdSnrLimit { get; set; }
        public int gaSnrLimit { get; set; }
        public int giSnrLimit { get; set; }

        //Testing
        public enum IoType
        {
            NavSpark,
            NavSparkMini
        }
        //Testing
        public enum TestFixType
        {
            PositionFix,
            RtkFloat,
            RtkFix,
            RtkFixRatio10,
        }

        public bool testIo { get; set; }
        public IoType testIoType { get; set; }
        public bool testAntenna { get; set; }
        public bool testUart2TxRx { get; set; }
        public bool enableDownload { get; set; }
        public bool enableSlaveDownload { get; set; }
        public int dlBaudSel { get; set; }
        public bool twoUartDownload { get; set; }
        

        //public bool testBootStatus { get; set; }
        public bool checkPromCrc { get; set; }
        public bool checkSlavePromCrc { get; set; }
        public bool checkRtc { get; set; }
        public bool testVoltage { get; set; }
        public double volrageLower { get; set; }
        public double volrageUpper { get; set; }
        public bool waitPositionFix { get; set; }
        public bool testClockOffset { get; set; }
        public double clockOffsetThreshold { get; set; }
        public bool writeClockOffset { get; set; }
        public bool testEcompass { get; set; }
        public bool testMiniHommer { get; set; }
        public bool testDrCyro { get; set; }
        public bool useSensor { get; set; }
        public int testDrDuration { get; set; }
        public double uslClockWise { get; set; }
        public double uslAnticlockWise { get; set; }
        public double lslClockWise { get; set; }
        public double lslAnticlockWise { get; set; }
        public double thresholdCog { get; set; }
        //INS-DR new items
        public bool testInsDrGyro { get; set; }
        public bool testBaro { get; set; }
        public bool skipSpdDir { get; set; }
        public int insDrGyroLower { get; set; }
        public int insDrGyroUpper { get; set; }
        public bool testAcc { get; set; }
        public double accXUpper { get; set; }
        public double accXLower { get; set; }
        public double accYUpper { get; set; }
        public double accYLower { get; set; }
        public double accZUpper { get; set; }
        public double accZLower { get; set; }
        //public int baroLower { get; set; }
        //public int baroUpper { get; set; }
        public int pressureCriteria { get; set; }
        public double tempCriteria { get; set; }
        //Support reverse roation
        public bool reverseRotation { get; set; }
        //For V827 module S1216DR8P, S1216RTK
        //public bool testToRtkFloat { get; set; }
        //public bool testToRtkFix { get; set; }
        public TestFixType testFixedType { get; set; }

        public class FirmwareProfile
        {
            public String promFile { get; set; }
            public String kVersion { get; set; }
            public String sVersion { get; set; }
            public String rVersion { get; set; }
            public UInt32 crc { get; set; }
            public String crcTxt { get; set; }
            public int dvBaudRate { get; set; }
            public UInt32 tagAddress { get; set; }
            public UInt32 tagContent { get; set; }
            public byte[] promRaw = null;

            public byte CalcPromRawCheckSum()
            {
                byte c = 0;
                foreach (byte b in promRaw)
                {
                    c += b;
                }
                return c;
            }

            public bool ReadePromRawData(String path)
            {
                //promRaw
                if (!File.Exists(path))
                {
                    return false;
                }
                try
                {
                    promRaw = File.ReadAllBytes(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool GenerateXml(ref XmlElement item, XmlDocument doc)
            {
                XmlElement itemData = doc.CreateElement("ItemData");
                itemData.SetAttribute("PF", promFile.ToString());
                itemData.SetAttribute("KV", kVersion.ToString());
                itemData.SetAttribute("SV", sVersion.ToString());
                itemData.SetAttribute("RV", rVersion.ToString());
                itemData.SetAttribute("CR", crc.ToString());
                itemData.SetAttribute("BR", dvBaudRate.ToString());
                itemData.SetAttribute("TA", tagAddress.ToString());
                itemData.SetAttribute("TC", tagContent.ToString());
                itemData.SetAttribute("PS", (promRaw == null) ? "0" : promRaw.Length.ToString());
                item.AppendChild(itemData);

                Crc32 crc32 = new Crc32();
                XmlElement itemKey = doc.CreateElement("ItemKey");
                itemKey.SetAttribute("Key", crc32.ComputeChecksum(itemData.OuterXml).ToString());
                item.AppendChild(itemKey);

                return true;
            }
        }

        public static String GpsCriteriaStrings(int lower, int upper)
        {
            return lower.ToString() + " ~ " + upper.ToString();
        }

        public static int GetPassSelUpperBound(int sel)
        {
            switch (sel)
            {
                case -1:
                    return InitSnrUpper;
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return MaxSnrValue;
                case 4:
                    return 3;
                case 5:
                    return MaxSnrValue;
                case 6:
                    return 5;
                case 7:
                    return MaxSnrValue;
                default:
                    return 0;
            }
        }

        public static int GetPassSelLowerBound(int sel)
        {
            switch (sel)
            {
                case -1:
                    return InitSnrLower;
                case 0:
                    return 0;
                case 1:
                    return -1;
                case 2:
                    return -2;
                case 3:
                    return -2;
                case 4:
                    return -3;
                case 5:
                    return -3;
                case 6:
                    return -5;
                case 7:
                    return -5;
                default:
                    return 0;
            }
        }

        public FirmwareProfile fwProfile;
        public FirmwareProfile slaveFwProfile;
        public bool ReadePromIniFile()
        {
            return ReadPromIniFileX(iniFileName, ref fwProfile);
        }

        public bool IsNeedSlavePromIniFile()
        {
            return (enableSlaveDownload || checkSlavePromCrc || twoUartDownload);
        }

        public bool ReadeSlavePromIniFile()
        {
            return ReadPromIniFileX(slaveIniFileName, ref slaveFwProfile);
        }

        public bool ReadPromIniFileX(string iniFileName, ref FirmwareProfile fwProfile)
        {
            String path = Login.loginInfo.currentPath + "\\" + iniFileName;
            FirmwareProfile tmpFwProfile = new FirmwareProfile();
            fwProfile = null;

            StringBuilder temp = new StringBuilder(MaxReadLength);
            if (0 == GetPrivateProfileString("Firmware", "Prom", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.promFile = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "K_Version", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.kVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "S_Version", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.sVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "Rev", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.rVersion = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "CRC", "", temp, MaxReadLength, path))
            {
                return false;
            }
            tmpFwProfile.crc = Convert.ToUInt32(temp.ToString(), 16);
            tmpFwProfile.crcTxt = temp.ToString();

            if (0 == GetPrivateProfileString("Firmware", "Baudrate", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.dvBaudRate = Convert.ToInt32(temp.ToString());

            if (0 == GetPrivateProfileString("Firmware", "Address", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.tagAddress = Convert.ToUInt32(temp.ToString(), 16);

            if (0 == GetPrivateProfileString("Firmware", "Value", "", temp, MaxReadLength, path))
            {
                //return false;
            }
            tmpFwProfile.tagContent = Convert.ToUInt32(temp.ToString(), 16);
            fwProfile = tmpFwProfile;
            return true;
        }

        public enum ErrorCode
        {
            NoError,
            InvalidateFormat,
        }
        public ErrorCode error { get; set; }
        public const int MaxSnrValue = 999;
        public const int MinSnrValue = -999;

        private const int MaxReadLength = 512;
        private const int InitGoldenBaudrate = 1;
        private const int InitPassCriteria = -1;
        private const int InitSnrUpper = 5;
        private const int InitSnrLower = -2;

        private const int InitSnrLimit = 48;
        private const int InitSnrTestPeriod = 10;
        private const int InitDownloadBaudrate = 7;

        private const String InitIniFileName = "prom.ini";
        private const String InitSlaveIniFileName = "prom2.ini";
        private const double InitClockOffsetThreshold = 2.5;
        private const int InitDrDuration = 10;
        private const double InitVoltageLower = 4.7;
        private const double InitVoltageUpper = 5.3;

        public ModuleTestProfile()
        {
            error = ErrorCode.NoError;
            gdBaudSel = InitGoldenBaudrate;

            gpSnrUpper = InitSnrUpper;
            glSnrUpper = InitSnrUpper;
            bdSnrUpper = InitSnrUpper;
            gaSnrUpper = InitSnrUpper;
            giSnrUpper = InitSnrUpper;
            gpSnrLower = InitSnrLower;
            glSnrLower = InitSnrLower;
            bdSnrLower = InitSnrLower;
            gaSnrLower = InitSnrLower;
            giSnrLower = InitSnrLower;

            snrTestPeriod = InitSnrTestPeriod;
            gpSnrLimit = InitSnrLimit;
            glSnrLimit = InitSnrLimit;
            bdSnrLimit = InitSnrLimit;
            gaSnrLimit = InitSnrLimit;
            giSnrLimit = InitSnrLimit;
            testGpSnr = true;

            iniFileName = InitIniFileName;
            slaveIniFileName = InitSlaveIniFileName;
            clockOffsetThreshold = InitClockOffsetThreshold;
            volrageLower = InitVoltageLower;
            volrageUpper = InitVoltageUpper;

            testDrDuration = InitDrDuration;
            dlBaudSel = InitDownloadBaudrate;
        }

        public ModuleTestProfile(ModuleTestProfile r)
        {
            gdBaudSel = r.gdBaudSel;
            moduleType = r.moduleType;
            gpModuleSel = r.gpModuleSel;
            glModuleSel = r.glModuleSel;
            bdModuleSel = r.bdModuleSel;
            gaModuleSel = r.gaModuleSel;
            giModuleSel = r.giModuleSel;
            moduleName = r.moduleName;

            testGpSnr = r.testGpSnr;
            testGlSnr = r.testGlSnr;
            testBdSnr = r.testBdSnr;
            testGaSnr = r.testGaSnr;
            testGiSnr = r.testGiSnr;

            gpSnrUpper = r.gpSnrUpper;
            glSnrUpper = r.glSnrUpper;
            bdSnrUpper = r.bdSnrUpper;
            gaSnrUpper = r.gaSnrUpper;
            giSnrUpper = r.giSnrUpper;
            gpSnrLower = r.gpSnrLower;
            glSnrLower = r.glSnrLower;
            bdSnrLower = r.bdSnrLower;
            gaSnrLower = r.gaSnrLower;
            giSnrLower = r.giSnrLower;

            snrTestPeriod = r.snrTestPeriod;
            gpSnrLimit = r.gpSnrLimit;
            glSnrLimit = r.glSnrLimit;
            bdSnrLimit = r.bdSnrLimit;
            gaSnrLimit = r.gaSnrLimit;
            giSnrLimit = r.giSnrLimit;

            testIo = r.testIo;
            testIoType = r.testIoType;
            testAntenna = r.testAntenna;
            testUart2TxRx = r.testUart2TxRx;
            iniFileName = r.iniFileName;
            slaveIniFileName = r.slaveIniFileName;
            enableDownload = r.enableDownload;
            enableSlaveDownload = r.enableSlaveDownload;
            dlBaudSel = r.dlBaudSel;
            twoUartDownload = r.twoUartDownload;

            checkPromCrc = r.checkPromCrc;
            checkSlavePromCrc = r.checkSlavePromCrc;
            checkRtc = r.checkRtc;
            testVoltage = r.testVoltage;
            volrageLower = r.volrageLower;
            volrageUpper = r.volrageUpper;
            waitPositionFix = r.waitPositionFix;
            testClockOffset = r.testClockOffset;
            clockOffsetThreshold = r.clockOffsetThreshold;
            writeClockOffset = r.writeClockOffset;
            testEcompass = r.testEcompass;
            testMiniHommer = r.testMiniHommer;
            testDrCyro = r.testDrCyro;
            useSensor = r.useSensor;
            testDrDuration = r.testDrDuration;
            uslClockWise = r.uslClockWise;
            uslAnticlockWise = r.uslAnticlockWise;
            lslClockWise = r.lslClockWise;
            lslAnticlockWise = r.lslAnticlockWise;
            thresholdCog = r.thresholdCog;
            error = ErrorCode.NoError;
            //INS-DR new items
            testInsDrGyro = r.testInsDrGyro;
            testBaro = r.testBaro;
            skipSpdDir = r.skipSpdDir;
            insDrGyroLower = r.insDrGyroLower;
            insDrGyroUpper = r.insDrGyroUpper;
            testAcc = r.testAcc;
            accXUpper = r.accXUpper;
            accXLower = r.accXLower;
            accYUpper = r.accYUpper;
            accYLower = r.accYLower;
            accZUpper = r.accZUpper;
            accZLower = r.accZLower;

            //baroLower = r.baroLower;
            //baroUpper = r.baroUpper;
            pressureCriteria = r.pressureCriteria;
            tempCriteria = r.tempCriteria;
            //Support reverse roation
            reverseRotation = r.reverseRotation;
            //For V827 module S1216DR8P, S1216RTK
            //testToRtkFloat = r.testToRtkFloat;
            //testToRtkFix = r.testToRtkFix;
            testFixedType = r.testFixedType;
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        private String GetVerification(String path, bool isNewFile)
        {
            StreamReader sr = new StreamReader(path);
            UInt32[] hashTable = { 0xE3D930C4, 0x8CE0F048, 0x21697CF8, 0x36F78E93,
                                     0x47DE9E6E, 0x58C8631E, 0x6683FFC4, 0x70BBDEFC };
            int tailPos = 0, lineCount = 0;
            bool inTheTail = false;

            Crc32 crc32 = new Crc32();

            while (!sr.EndOfStream)
            {   // Read one line until end of file.
                string line = sr.ReadLine();
                if (0 == line.CompareTo("[Verification]"))
                {
                    inTheTail = true;
                    tailPos = lineCount;
                }

                if (!inTheTail)
                {
                    uint ucrc = crc32.ComputeChecksum(line);
                    for (int i = 0; i < hashTable.Length; i++)
                    {
                        hashTable[i] = hashTable[i] ^ ucrc;
                    }
                }
                lineCount++;
            }
            sr.Close();
            if (lineCount != (tailPos + 2) && !isNewFile)
            {
                return "";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (UInt32 a in hashTable)
            {
                sb.Append(a.ToString("X"));
            }
            return sb.ToString();
        }

        public bool LoadFromIniFile(String path)
        {
            String vKey = GetVerification(path, false);
            if (vKey.Length == 0)
            {
                error = ErrorCode.InvalidateFormat;
                return false;
            }

            StringBuilder temp = new StringBuilder(MaxReadLength);
            GetPrivateProfileString("Verification", "Key", "", temp, MaxReadLength, path);
            if (vKey.CompareTo(temp.ToString()) != 0)
            {
                error = ErrorCode.InvalidateFormat;
                return false;
            }

            GetPrivateProfileString("Golden", "Golden_Baud_Rate", InitGoldenBaudrate.ToString(), temp, MaxReadLength, path);
            gdBaudSel = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Module", "Module_Type", "0", temp, MaxReadLength, path);
            moduleType = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "GPS_Module_Select", "0", temp, MaxReadLength, path);
            gpModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Glonass_Module_Select", "0", temp, MaxReadLength, path);
            glModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Beidou_Module_Select", "0", temp, MaxReadLength, path);
            bdModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Galileo_Module_Select", "0", temp, MaxReadLength, path);
            gaModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Navic_Module_Select", "0", temp, MaxReadLength, path);
            giModuleSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Module", "Module_Name", "", temp, MaxReadLength, path);
            moduleName = temp.ToString();

            GetPrivateProfileString("Device", "Ini_File", InitIniFileName, temp, MaxReadLength, path);
            iniFileName = temp.ToString();
            GetPrivateProfileString("Device", "Slave_Ini_File", InitSlaveIniFileName, temp, MaxReadLength, path);
            slaveIniFileName = temp.ToString();
            GetPrivateProfileString("Device", "SNR_Test_Period", InitSnrTestPeriod.ToString(), temp, MaxReadLength, path);
            snrTestPeriod = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "Test_GPS_SNR", "True", temp, MaxReadLength, path);
            testGpSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Glonass_SNR", "False", temp, MaxReadLength, path);
            testGlSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Beidou_SNR", "False", temp, MaxReadLength, path);
            testBdSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Galileo_SNR", "False", temp, MaxReadLength, path);
            testGaSnr = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Device", "Test_Navic_SNR", "False", temp, MaxReadLength, path);
            testGiSnr = Convert.ToBoolean(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int gpPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int glPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int bdPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int gaPassSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Navic_SNR_Criteria", "-1", temp, MaxReadLength, path);
            int giPassSel = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Upper_Bound", GetPassSelUpperBound(gpPassSel).ToString(), temp, MaxReadLength, path);
            gpSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "GPS_SNR_Lower_Bound", GetPassSelLowerBound(gpPassSel).ToString(), temp, MaxReadLength, path);
            gpSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Upper_Bound", GetPassSelUpperBound(glPassSel).ToString(), temp, MaxReadLength, path);
            glSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Lower_Bound", GetPassSelLowerBound(glPassSel).ToString(), temp, MaxReadLength, path);
            glSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Upper_Bound", GetPassSelUpperBound(bdPassSel).ToString(), temp, MaxReadLength, path);
            bdSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Lower_Bound", GetPassSelLowerBound(bdPassSel).ToString(), temp, MaxReadLength, path);
            bdSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Upper_Bound", GetPassSelUpperBound(gaPassSel).ToString(), temp, MaxReadLength, path);
            gaSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Lower_Bound", GetPassSelLowerBound(gaPassSel).ToString(), temp, MaxReadLength, path);
            gaSnrLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Navic_SNR_Upper_Bound", GetPassSelUpperBound(giPassSel).ToString(), temp, MaxReadLength, path);
            giSnrUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Navic_SNR_Lower_Bound", GetPassSelLowerBound(giPassSel).ToString(), temp, MaxReadLength, path);
            giSnrLower = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Device", "GPS_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            gpSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Glonass_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            glSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Beidou_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            bdSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Galileo_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            gaSnrLimit = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Device", "Navic_SNR_Limit", InitSnrLimit.ToString(), temp, MaxReadLength, path);
            giSnrLimit = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Testing", "Enable_Download", "False", temp, MaxReadLength, path);
            enableDownload = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Enable_Slave_Download", "False", temp, MaxReadLength, path);
            enableSlaveDownload = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Two_Uart_Download", "False", temp, MaxReadLength, path);
            twoUartDownload = Convert.ToBoolean(temp.ToString());

            GetPrivateProfileString("Testing", "Download_Baud_Rate", InitDownloadBaudrate.ToString(), temp, MaxReadLength, path);
            dlBaudSel = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Io", "False", temp, MaxReadLength, path);
            testIo = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Io_Type", "0", temp, MaxReadLength, path);
            testIoType = (IoType)Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Antenna_Detect", "False", temp, MaxReadLength, path);
            testAntenna = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_UART2_TXRX", "False", temp, MaxReadLength, path);
            testUart2TxRx = Convert.ToBoolean(temp.ToString());

            GetPrivateProfileString("Testing", "Check_Prom_Crc", "False", temp, MaxReadLength, path);
            checkPromCrc = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Check_Slave_Prom_Crc", "False", temp, MaxReadLength, path);
            checkSlavePromCrc = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Check_Rtc", "True", temp, MaxReadLength, path);
            checkRtc = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Voltage", "False", temp, MaxReadLength, path);
            testVoltage = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Voltage_Lower", InitVoltageLower.ToString(), temp, MaxReadLength, path);
            volrageLower = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Voltage_Upper", InitVoltageUpper.ToString(), temp, MaxReadLength, path);
            volrageUpper = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Wait_Position_Fix", "False", temp, MaxReadLength, path);
            waitPositionFix = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Colok_Offset", "False", temp, MaxReadLength, path);
            testClockOffset = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Clock_Offset_Threshold", InitClockOffsetThreshold.ToString(), temp, MaxReadLength, path);
            clockOffsetThreshold = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Write_Clock_Offset", "False", temp, MaxReadLength, path);
            writeClockOffset = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_E_Compass", "False", temp, MaxReadLength, path);
            testEcompass = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_miniHommer", "False", temp, MaxReadLength, path);
            testMiniHommer = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_DR_Cyro", "False", temp, MaxReadLength, path);
            testDrCyro = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Use_Sensor", "True", temp, MaxReadLength, path);
            useSensor = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_DR_Duration", InitDrDuration.ToString(), temp, MaxReadLength, path);
            testDrDuration = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "USL_Clockwise", "0", temp, MaxReadLength, path);
            uslClockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "USL_Anticlockwise", "0", temp, MaxReadLength, path);
            uslAnticlockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "LSL_Clockwise", "0", temp, MaxReadLength, path);
            lslClockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "LSL_Anticlockwise", "0", temp, MaxReadLength, path);
            lslAnticlockWise = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Threshold_Of_Cog", "0", temp, MaxReadLength, path);
            thresholdCog = Convert.ToDouble(temp.ToString());
            //INS-DR new items
            GetPrivateProfileString("Testing", "Test_INSDR_Gyro", "False", temp, MaxReadLength, path);
            testInsDrGyro = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Baro", "False", temp, MaxReadLength, path);
            testBaro = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Skip_Test_Speed_Direction", "False", temp, MaxReadLength, path);
            skipSpdDir = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "INSDR_GYRO_Lower_Bound", "0", temp, MaxReadLength, path);
            insDrGyroLower = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "INSDR_GYRO_Upper_Bound", "0", temp, MaxReadLength, path);
            insDrGyroUpper = Convert.ToInt32(temp.ToString());

            GetPrivateProfileString("Testing", "Test_Accelerometer", "False", temp, MaxReadLength, path);
            testAcc = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Accelerometer_X_Upper", "-1.5", temp, MaxReadLength, path);
            accXUpper = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Accelerometer_X_Lower", "-2.5", temp, MaxReadLength, path);
            accXLower = Convert.ToDouble(temp.ToString());

            GetPrivateProfileString("Testing", "Accelerometer_Y_Upper", "1.0", temp, MaxReadLength, path);
            accYUpper = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Accelerometer_Y_Lower", "-1.0", temp, MaxReadLength, path);
            accYLower = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Accelerometer_Z_Upper", "10.5", temp, MaxReadLength, path);
            accZUpper = Convert.ToDouble(temp.ToString());
            GetPrivateProfileString("Testing", "Accelerometer_Z_Lower", "9.5", temp, MaxReadLength, path);
            accZLower = Convert.ToDouble(temp.ToString());

            //GetPrivateProfileString("Testing", "Baro_Lower_Bound", "0", temp, MaxReadLength, path);
            //baroLower = Convert.ToInt32(temp.ToString());
            //GetPrivateProfileString("Testing", "Baro_Upper_Bound", "0", temp, MaxReadLength, path);
            //baroUpper = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "Pressure_Criteria", "100", temp, MaxReadLength, path);
            pressureCriteria = Convert.ToInt32(temp.ToString());
            GetPrivateProfileString("Testing", "Temperature_Criteria", "5.0", temp, MaxReadLength, path);
            tempCriteria = Convert.ToDouble(temp.ToString());
            //Support reverse roation
            GetPrivateProfileString("Testing", "Reverse_Rotation", "True", temp, MaxReadLength, path);
            reverseRotation = Convert.ToBoolean(temp.ToString());
            //For V827 module S1216DR8P, S1216RTK
            //GetPrivateProfileString("Testing", "Test_To_Rtk_Float", "False", temp, MaxReadLength, path);
            //testToRtkFloat = Convert.ToBoolean(temp.ToString());
            //GetPrivateProfileString("Testing", "Test_To_Rtk_Fix", "False", temp, MaxReadLength, path);
            //testToRtkFix = Convert.ToBoolean(temp.ToString());
            GetPrivateProfileString("Testing", "Test_Fixed_Type", "0", temp, MaxReadLength, path);
            testFixedType = (TestFixType)Convert.ToInt32(temp.ToString());

            return true;
        }

        public int GetTotalTestPeriod()
        {
            //int defaultTestPeriod = (testDrCyro) ? 17 : 0;
            //return defaultTestPeriod + snrTestPeriod;
            return snrTestPeriod;
        }

        private int testPeriodCounter = 0;
        public int SetTestPeriodCounter(int c)
        {
            testPeriodCounter = c;
            return testPeriodCounter;
        }

        public int AddTestPeriodCounter(int a)
        {
            testPeriodCounter += a;
            return testPeriodCounter;
        }

        public int DecreaseTestPeriodCounter()
        {
            return --testPeriodCounter;
        }

        public int IncreaseTestPeriodCounter()
        {
            return ++testPeriodCounter;
        }

        public bool SaveToIniFile(String path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            WritePrivateProfileString("Golden", "Golden_Baud_Rate", gdBaudSel.ToString(), path);

            WritePrivateProfileString("Module", "Module_Type", moduleType.ToString(), path);
            WritePrivateProfileString("Module", "GPS_Module_Select", gpModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Glonass_Module_Select", glModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Beidou_Module_Select", bdModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Galileo_Module_Select", gaModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Navic_Module_Select", giModuleSel.ToString(), path);
            WritePrivateProfileString("Module", "Module_Name", moduleName, path);

            WritePrivateProfileString("Device", "Ini_File", iniFileName, path);
            WritePrivateProfileString("Device", "Slave_Ini_File", slaveIniFileName, path);
            WritePrivateProfileString("Device", "SNR_Test_Period", snrTestPeriod.ToString(), path);

            WritePrivateProfileString("Device", "Test_GPS_SNR", testGpSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Glonass_SNR", testGlSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Beidou_SNR", testBdSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Galileo_SNR", testGaSnr.ToString(), path);
            WritePrivateProfileString("Device", "Test_Navic_SNR", testGiSnr.ToString(), path);

            WritePrivateProfileString("Device", "GPS_SNR_Upper_Bound", gpSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "GPS_SNR_Lower_Bound", gpSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Upper_Bound", glSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Lower_Bound", glSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Upper_Bound", bdSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Lower_Bound", bdSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Upper_Bound", gaSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Lower_Bound", gaSnrLower.ToString(), path);
            WritePrivateProfileString("Device", "Navic_SNR_Upper_Bound", giSnrUpper.ToString(), path);
            WritePrivateProfileString("Device", "Navic_SNR_Lower_Bound", giSnrLower.ToString(), path);

            WritePrivateProfileString("Device", "GPS_SNR_Limit", gpSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Glonass_SNR_Limit", glSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Beidou_SNR_Limit", bdSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Galileo_SNR_Limit", gaSnrLimit.ToString(), path);
            WritePrivateProfileString("Device", "Navic_SNR_Limit", giSnrLimit.ToString(), path);

            WritePrivateProfileString("Testing", "Test_Io", testIo.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Io_Type", ((int)testIoType).ToString(), path);
            WritePrivateProfileString("Testing", "Test_Antenna_Detect", testAntenna.ToString(), path);
            WritePrivateProfileString("Testing", "Test_UART2_TXRX", testUart2TxRx.ToString(), path);
            WritePrivateProfileString("Testing", "Enable_Download", enableDownload.ToString(), path);
            WritePrivateProfileString("Testing", "Enable_Slave_Download", enableSlaveDownload.ToString(), path);
            WritePrivateProfileString("Testing", "Two_Uart_Download", twoUartDownload.ToString(), path);

            WritePrivateProfileString("Testing", "Download_Baud_Rate", dlBaudSel.ToString(), path);
            WritePrivateProfileString("Testing", "Check_Prom_Crc", checkPromCrc.ToString(), path);
            WritePrivateProfileString("Testing", "Check_Slave_Prom_Crc", checkSlavePromCrc.ToString(), path);
            WritePrivateProfileString("Testing", "Check_Rtc", checkRtc.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Voltage", testVoltage.ToString(), path);
            WritePrivateProfileString("Testing", "Voltage_Lower", volrageLower.ToString(), path);
            WritePrivateProfileString("Testing", "Voltage_Upper", volrageUpper.ToString(), path);
            WritePrivateProfileString("Testing", "Wait_Position_Fix", waitPositionFix.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Colok_Offset", testClockOffset.ToString(), path);
            WritePrivateProfileString("Testing", "Clock_Offset_Threshold", clockOffsetThreshold.ToString(), path);
            WritePrivateProfileString("Testing", "Write_Clock_Offset", writeClockOffset.ToString(), path);
            WritePrivateProfileString("Testing", "Test_E_Compass", testEcompass.ToString(), path);
            WritePrivateProfileString("Testing", "Test_miniHommer", testMiniHommer.ToString(), path);
            WritePrivateProfileString("Testing", "Test_DR_Cyro", testDrCyro.ToString(), path);
            WritePrivateProfileString("Testing", "Use_Sensor", useSensor.ToString(), path);
            WritePrivateProfileString("Testing", "Test_DR_Duration", testDrDuration.ToString(), path);
            WritePrivateProfileString("Testing", "USL_Clockwise", uslClockWise.ToString(), path);
            WritePrivateProfileString("Testing", "USL_Anticlockwise", uslAnticlockWise.ToString(), path);
            WritePrivateProfileString("Testing", "LSL_Clockwise", lslClockWise.ToString(), path);
            WritePrivateProfileString("Testing", "LSL_Anticlockwise", lslAnticlockWise.ToString(), path);
            WritePrivateProfileString("Testing", "Threshold_Of_Cog", thresholdCog.ToString(), path);
            //INS-DR new items
            WritePrivateProfileString("Testing", "Test_INSDR_Gyro", testInsDrGyro.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Baro", testBaro.ToString(), path);
            WritePrivateProfileString("Testing", "Skip_Test_Speed_Direction", skipSpdDir.ToString(), path);
            WritePrivateProfileString("Testing", "INSDR_GYRO_Lower_Bound", insDrGyroLower.ToString(), path);
            WritePrivateProfileString("Testing", "INSDR_GYRO_Upper_Bound", insDrGyroUpper.ToString(), path);

            WritePrivateProfileString("Testing", "Test_Accelerometer", testAcc.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_X_Upper", accXUpper.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_X_Lower", accXLower.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_Y_Upper", accYUpper.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_Y_Lower", accYLower.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_Z_Upper", accZUpper.ToString(), path);
            WritePrivateProfileString("Testing", "Accelerometer_Z_Lower", accZLower.ToString(), path);

            //WritePrivateProfileString("Testing", "Baro_Lower_Bound", baroLower.ToString(), path);
            //WritePrivateProfileString("Testing", "Baro_Upper_Bound", baroUpper.ToString(), path);
            WritePrivateProfileString("Testing", "Pressure_Criteria", pressureCriteria.ToString(), path);
            WritePrivateProfileString("Testing", "Temperature_Criteria", tempCriteria.ToString(), path);
            //Support reverse roation
            WritePrivateProfileString("Testing", "Reverse_Rotation", reverseRotation.ToString(), path);
            //For V827 module S1216DR8P, S1216RTK
            //WritePrivateProfileString("Testing", "Test_To_Rtk_Float", testToRtkFloat.ToString(), path);
            //WritePrivateProfileString("Testing", "Test_To_Rtk_Fix", testToRtkFix.ToString(), path);
            WritePrivateProfileString("Testing", "Test_Fixed_Type", ((int)testFixedType).ToString(), path);

            String vKey = GetVerification(path, true);
            WritePrivateProfileString("Verification", "Key", vKey, path);
            return true;
        }
        public bool GenerateXml(ref XmlElement item, XmlDocument doc)
        {
            XmlElement itemData = doc.CreateElement("ItemData");
            itemData.SetAttribute("GB", gdBaudSel.ToString());
            itemData.SetAttribute("MT", moduleType.ToString());
            itemData.SetAttribute("MN", moduleName);
            itemData.SetAttribute("GPM", gpModuleSel.ToString());
            itemData.SetAttribute("GLM", glModuleSel.ToString());
            itemData.SetAttribute("BDM", bdModuleSel.ToString());
            itemData.SetAttribute("GAM", gaModuleSel.ToString());
            itemData.SetAttribute("GIM", giModuleSel.ToString());
            itemData.SetAttribute("STP", snrTestPeriod.ToString());
            itemData.SetAttribute("TGP", testGpSnr.ToString());
            itemData.SetAttribute("TGL", testGlSnr.ToString());
            itemData.SetAttribute("TBD", testBdSnr.ToString());
            itemData.SetAttribute("TGA", testGaSnr.ToString());
            itemData.SetAttribute("TGI", testGiSnr.ToString());
            itemData.SetAttribute("GPU", gpSnrUpper.ToString());
            itemData.SetAttribute("GPL", gpSnrLower.ToString());
            itemData.SetAttribute("GLU", glSnrUpper.ToString());
            itemData.SetAttribute("GLL", glSnrLower.ToString());
            itemData.SetAttribute("BDU", bdSnrUpper.ToString());
            itemData.SetAttribute("BDL", bdSnrLower.ToString());
            itemData.SetAttribute("GAU", gaSnrUpper.ToString());
            itemData.SetAttribute("GAL", gaSnrLower.ToString());
            itemData.SetAttribute("GIU", giSnrUpper.ToString());
            itemData.SetAttribute("GIL", giSnrLower.ToString());
            itemData.SetAttribute("GPS", gpSnrLimit.ToString());
            itemData.SetAttribute("GLS", glSnrLimit.ToString());
            itemData.SetAttribute("BDS", bdSnrLimit.ToString());
            itemData.SetAttribute("GAS", gaSnrLimit.ToString());
            itemData.SetAttribute("GIS", giSnrLimit.ToString());
            itemData.SetAttribute("TI", testIo.ToString());
            itemData.SetAttribute("TIT", ((int)testIoType).ToString());
            itemData.SetAttribute("AD", testAntenna.ToString());
            itemData.SetAttribute("U2T", testUart2TxRx.ToString());

            itemData.SetAttribute("ED", enableDownload.ToString());
            itemData.SetAttribute("ESD", enableSlaveDownload.ToString());
            itemData.SetAttribute("TUD", twoUartDownload.ToString());
            itemData.SetAttribute("DB", dlBaudSel.ToString());
            itemData.SetAttribute("CPC", checkPromCrc.ToString());
            itemData.SetAttribute("CSC", checkSlavePromCrc.ToString());
            itemData.SetAttribute("CRT", checkRtc.ToString());
            itemData.SetAttribute("TVT", testVoltage.ToString());
            itemData.SetAttribute("VLR", volrageLower.ToString());
            itemData.SetAttribute("VUR", volrageUpper.ToString());
            itemData.SetAttribute("WPF", waitPositionFix.ToString());
            itemData.SetAttribute("TCO", testClockOffset.ToString());
            itemData.SetAttribute("COT", clockOffsetThreshold.ToString());
            itemData.SetAttribute("WCO", writeClockOffset.ToString());
            itemData.SetAttribute("TEC", testEcompass.ToString());
            itemData.SetAttribute("TMH", testMiniHommer.ToString());
            itemData.SetAttribute("TDC", testDrCyro.ToString());
            itemData.SetAttribute("DUS", useSensor.ToString());
            itemData.SetAttribute("TDD", testDrDuration.ToString());
            itemData.SetAttribute("USC", uslClockWise.ToString());
            itemData.SetAttribute("USA", uslAnticlockWise.ToString());
            itemData.SetAttribute("LSC", lslClockWise.ToString());
            itemData.SetAttribute("LSA", lslAnticlockWise.ToString());
            itemData.SetAttribute("TOC", thresholdCog.ToString());
            //INS-DR new items
            itemData.SetAttribute("TIG", testInsDrGyro.ToString());
            itemData.SetAttribute("TBA", testBaro.ToString());
            itemData.SetAttribute("SSD", skipSpdDir.ToString());
            itemData.SetAttribute("IGL", insDrGyroLower.ToString());
            itemData.SetAttribute("IGU", insDrGyroUpper.ToString());

            itemData.SetAttribute("TAC", testAcc.ToString());
            itemData.SetAttribute("AXU", accXUpper.ToString());
            itemData.SetAttribute("AXL", accXLower.ToString());
            itemData.SetAttribute("AYU", accYUpper.ToString());
            itemData.SetAttribute("AYL", accYLower.ToString());
            itemData.SetAttribute("AZU", accZUpper.ToString());
            itemData.SetAttribute("AZL", accZLower.ToString());

            //itemData.SetAttribute("BAL", baroLower.ToString());
            //itemData.SetAttribute("BAU", baroUpper.ToString());
            itemData.SetAttribute("PRC", pressureCriteria.ToString());
            itemData.SetAttribute("TMC", tempCriteria.ToString());
            item.AppendChild(itemData);
            //Support reverse roation
            itemData.SetAttribute("RRO", reverseRotation.ToString());
            //For V827 module S1216DR8P, S1216RTK
            //itemData.SetAttribute("TTL", testToRtkFloat.ToString());
            //itemData.SetAttribute("TTI", testToRtkFix.ToString());
            itemData.SetAttribute("TFT", ((int)testFixedType).ToString());
            
            Crc32 crc32 = new Crc32();
            XmlElement itemKey = doc.CreateElement("ItemKey");
            itemKey.SetAttribute("Key", crc32.ComputeChecksum(itemData.OuterXml).ToString());
            item.AppendChild(itemKey);

            return true;
        }
    }
}