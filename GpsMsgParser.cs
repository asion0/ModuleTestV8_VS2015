using System;
using System.Collections.Generic;
using System.Text;

namespace ModuleTestV8
{
    public class GpsMsgParser
    {
        public enum SateType
        {
            Unknown,
            Gps,
            Glonass,
            Beidou,
            Galileo,
            Navic
        }

        public class ParsingStatus
        {
            public const int MaxSattellite = 24;
            public const int MaxChannels = 12;
            public const int NavicChannelStart = 16;
            public const int MaxNavicChannels = 24;
            public const int NullValue = int.MinValue;

            public ParsingStatus()
            {
                for (int i = 0; i < MaxSattellite; i++)
                {
                    gpSate[i] = new sateInfo();
                    glSate[i] = new sateInfo();
                    bdSate[i] = new sateInfo();
                    giSate[i] = new sateInfo();
                }
            }

            public class sateInfo
            {
                public sateInfo()
                {
                    Clear();
                }

                public sateInfo(sateInfo s)
                {
                    prn = s.prn;
                    snr = s.snr;
                    inUse = s.inUse;
                }

                public void Clear()
                {
                    prn = NullValue;
                    snr = NullValue;
                    inUse = false;
                }
                public int prn { get; set; }
                public int snr { get; set; }
                public bool inUse { get; set; }
            }
            public sateInfo GetGpsSate(int i)
            {
                return GetSate(i, SateType.Gps);
            }
            public sateInfo GetGlonassSate(int i)
            {
                return GetSate(i, SateType.Glonass);
            }
            public sateInfo GetBeidouSate(int i)
            {
                return GetSate(i, SateType.Beidou);
            }
            public sateInfo GetNavicSate(int i)
            {
                return GetSate(i, SateType.Navic);
            }

            public int GetSpecGpsSate(int id1, int id2, int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                return GetSpecSate(SateType.Gps, id1, id2, id3, ref sn1, ref sn2, ref sn3);
            }
            public int GetSpecGlonassSate(int id1, int id2, int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                return GetSpecSate(SateType.Glonass, id1, id2, id3, ref sn1, ref sn2, ref sn3);
            }
            public int GetSpecBeidouSate(int id1, int id2, int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                return GetSpecSate(SateType.Beidou, id1, id2, id3, ref sn1, ref sn2, ref sn3);
            }
            public int GetSpecNavicSate(int id1, int id2, int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                return GetSpecSate(SateType.Navic, id1, id2, id3, ref sn1, ref sn2, ref sn3);
            }

            //public int GetFrontGpsSate(ref int id1, ref int id2, ref int id3, ref int sn1, ref int sn2, ref int sn3)
            //{
            //    return GetFrontSate(SateType.Gps, ref id1, ref id2, ref id3, ref sn1, ref sn2, ref sn3);
            //}
            //public int GetFrontGlonassSate(ref int id1, ref int id2, ref int id3, ref int sn1, ref int sn2, ref int sn3)
            //{
            //    return GetFrontSate(SateType.Glonass, ref id1, ref id2, ref id3, ref sn1, ref sn2, ref sn3);
            //}
            //public int GetFrontBeidouSate(ref int id1, ref int id2, ref int id3, ref int sn1, ref int sn2, ref int sn3)
            //{
            //    return GetFrontSate(SateType.Beidou, ref id1, ref id2, ref id3, ref sn1, ref sn2, ref sn3);
            //}
            public sateInfo[] GetSortedGpsSateArray()
            {
                return GetSortedSateArray(SateType.Gps);
            }
            public sateInfo[] GetSortedGlonassSateArray()
            {
                return GetSortedSateArray(SateType.Glonass);
            }
            public sateInfo[] GetSortedBeidouSateArray()
            {
                return GetSortedSateArray(SateType.Beidou);
            }
            public sateInfo[] GetSortedNavicSateArray()
            {
                return GetSortedSateArray(SateType.Navic);
            }

            public bool AddInUseGNPrn(int prn, int systemId)
            {
                if (systemId == 0)
                {   //system ID == 0, detect constellation type by prn
                    return AddInUsePrn(prn, GetTypeByPrn(prn));
                }
                else
                {   
                    switch(systemId)
                    {
                        case 1:
                            return AddInUsePrn(prn, SateType.Gps);
                        case 2:
                            return AddInUsePrn(prn, SateType.Glonass);
                        case 3:
                            return AddInUsePrn(prn, SateType.Galileo);
                        case 4:
                            return AddInUsePrn(prn, SateType.Navic);
                    }
                }
                return AddInUsePrn(prn, GetTypeByPrn(prn));
            }

            public bool AddInUseGpsPrn(int prn)
            {
                return AddInUsePrn(prn, SateType.Gps);
            }
            public bool AddInUseGlonassPrn(int prn)
            {
                return AddInUsePrn(prn, SateType.Glonass);
            }
            public bool AddInUseBeidouPrn(int prn)
            {
                return AddInUsePrn(prn, SateType.Beidou);
            }
            public bool AddInUseNavicPrn(int prn)
            {
                return AddInUsePrn(prn, SateType.Navic);
            }

            public bool UpdateGNSnr(int prn, int snr)
            {
                return UpdateSnr(prn, snr, GetTypeByPrn(prn));
            }
            public bool UpdateGpsSnr(int prn, int snr)
            {
                return UpdateSnr(prn, snr, SateType.Gps);
            }
            public bool UpdateGlonassSnr(int prn, int snr)
            {
                return UpdateSnr(prn, snr, SateType.Glonass);
            }
            public bool UpdateBeidouSnr(int prn, int snr)
            {
                return UpdateSnr(prn, snr, SateType.Beidou);
            }
            public bool UpdateNavicSnr(int prn, int snr)
            {
                return UpdateSnr(prn, snr, SateType.Navic);
            }

            public sateInfo GetGpsSnr(int prn)
            {
                return GetSnr(prn, SateType.Gps);
            }
            public sateInfo GetGlonassSnr(int prn)
            {
                return GetSnr(prn, SateType.Glonass);
            }
            public sateInfo GetBeidouSnr(int prn)
            {
                return GetSnr(prn, SateType.Beidou);
            }
            public sateInfo GetNavicSnr(int prn)
            {
                return GetSnr(prn, SateType.Navic);
            }
            public sateInfo GetSnr(int prn, SateType t)
            {
                sateInfo[] p = GetSateArray(t);

                for (int i = 0; i < MaxSattellite; i++)
                {
                    if (p[i].prn == prn)
                    {
                        return p[i];
                    }
                }
                return NullSate;
            }

            public sateInfo GetRealBeidouSnr(int prn, SateType t)
            {
                sateInfo[] p = GetSateArray(t);

                for (int i = 0; i < MaxSattellite; i++)
                {
                    if (GetRealBeidouPrn(p[i].prn) == GetRealBeidouPrn(prn))
                    {
                        return p[i];
                    }
                }
                return NullSate;
            }

            public void ClearAllSate()
            {
                for (int i = 0; i < MaxSattellite; i++)
                {
                    gpSate[i].Clear();
                    glSate[i].Clear();
                    bdSate[i].Clear();
                    giSate[i].Clear();
                }
            }
            public void CopyTo(ParsingStatus s)
            {
                for (int i = 0; i < MaxSattellite; i++)
                {
                    s.gpSate[i] = gpSate[i];
                    s.glSate[i] = glSate[i];
                    s.bdSate[i] = bdSate[i];
                    s.giSate[i] = giSate[i];
                }
                s.positionFixResult = positionFixResult;
            }

            private int positionFixResult = 0;
            public bool IsFixed()
            {
                return (positionFixResult != 0);
            }
            public bool IsRtkFloat()
            {
                return ((positionFixResult == 5) || (positionFixResult == 4));
            }
            public bool IsRtkFix()
            {
                return (positionFixResult == 4);
            }

            public string GetFixStatusString()
            {
                switch (positionFixResult)
                {
                    case 4:
                        return "RTK Fix";
                    case 5:
                        return "Rtk Float";
                    case 2:
                        return "DGPS";
                    case 1:
                    case 3:
                        return "Position Fixed";
                }
                return "No Fix";
            }

            public bool SetPositionFixResult(int p)
            {
                if (p == positionFixResult)
                {
                    return false;
                }
                positionFixResult = p;
                return true;
            }

            private sateInfo NullSate = new sateInfo();
            private sateInfo[] gpSate = new sateInfo[MaxSattellite];
            private sateInfo[] glSate = new sateInfo[MaxSattellite];
            private sateInfo[] bdSate = new sateInfo[MaxSattellite];
            private sateInfo[] giSate = new sateInfo[MaxSattellite];

            public String latString = "";
            public char ns = ' ';
            public String lonString = "";
            public char ew = ' ';
            public String dateString = "";
            public String timeString = "";
            public String altitudeString = "";

            private SateType GetTypeByPrn(int prn)
            {
                if (prn >= 1 && prn <= 50 || prn == 193)
                {
                    return SateType.Gps;
                }
                if (prn >= 65 && prn <= 96)
                {   //GLONASS satellite using PSTI for testing.
                    return SateType.Unknown;
                }
                if (prn >= 100 && prn <= 137)
                {   //OlinkStar NMEA
                    return SateType.Beidou;
                }
                if (prn >= 160 && prn <= 192)
                {   //Unicore NMEA
                    return SateType.Beidou;
                }
                if (prn >= 200 && prn <= 232)
                {   //Unicore NMEA
                    return SateType.Beidou;
                }
                return SateType.Beidou;
            }

            public static int GetBeidouPrnBase(int prn)
            {
                if (prn >= 1 && prn <= 50 || prn == 193)
                {
                    return 0;
                }
                if (prn >= 65 && prn <= 96)
                {   //GLONASS satellite using PSTI for testing.
                    return 0;
                }
                if (prn >= 100 && prn <= 137)
                {   //OlinkStar NMEA
                    return 100;
                }
                if (prn >= 160 && prn <= 192)
                {   //Unicore NMEA
                    return 160;
                }
                if (prn >= 200 && prn <= 232)
                {   //Unicore NMEA
                    return 200;
                }
                return 0;
            }

            public static int GetRealBeidouPrn(int prn)
            {
                return prn - GetBeidouPrnBase(prn);
            }
            
            private sateInfo GetSate(int i, SateType t)
            {
                switch (t)
                {
                    case SateType.Gps:
                        return gpSate[i];
                    case SateType.Glonass:
                        return glSate[i];
                    case SateType.Beidou:
                        return bdSate[i];
                    case SateType.Navic:
                        return giSate[i];
                }
                return NullSate;
            }
            private int GetSpecSate(SateType t, int id1, int id2, int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                int getCount = 0;
                int snr;

                snr = GetSnr(id1, t).snr;
                if (snr != NullValue)
                {
                    sn1 = snr;
                    getCount++;
                }
                snr = GetSnr(id2, t).snr;
                if (snr != NullValue)
                {
                    sn2 = snr;
                    getCount++;
                }
                snr = GetSnr(id3, t).snr;
                if (snr != NullValue)
                {
                    sn3 = snr;
                    getCount++;
                }
                return getCount;
            }
            private int GetFrontSate(SateType t, ref int id1, ref int id2, ref int id3, ref int sn1, ref int sn2, ref int sn3)
            {
                sateInfo[] sortedSateArray = GetSateArray(t).Clone() as sateInfo[];

                for (int i = 0; i < MaxSattellite; i++)
                {
                    for (int j = i + 1; j < MaxSattellite; j++)
                    {
                        if (sortedSateArray[i].snr < sortedSateArray[j].snr)
                        {
                            sateInfo tmp = sortedSateArray[i];
                            sortedSateArray[i] = sortedSateArray[j];
                            sortedSateArray[j] = tmp;
                        }
                    }
                }
                if (sortedSateArray[0].snr == NullValue)
                {
                    return 0;
                }
                id1 = sortedSateArray[0].prn;
                sn1 = sortedSateArray[0].snr;

                if (sortedSateArray[1].snr == NullValue)
                {
                    return 1;
                }
                id2 = sortedSateArray[1].prn;
                sn2 = sortedSateArray[1].snr;

                if (sortedSateArray[2].snr == NullValue)
                {
                    return 2;
                }
                id3 = sortedSateArray[2].prn;
                sn3 = sortedSateArray[2].snr;
                return 3;
            }
            private sateInfo[] GetSortedSateArray(SateType t)
            {
                sateInfo[] s = GetSateArray(t).Clone() as sateInfo[];
                Array.Sort(s, delegate(sateInfo s1, sateInfo s2)
                {
                    return s2.snr.CompareTo(s1.snr);
                } );
                return s;
            }
            private bool AddInUsePrn(int prn, SateType type)
            {
                for (int i = 0; i < MaxSattellite; i++)
                {
                    switch (type)
                    {
                        case SateType.Gps:
                            if (prn < 1 || prn > 32)
                            {   //pass the special satellite for gps. In different module it may not have these satellite.
                                return false;
                            }

                            if (gpSate[i].prn == NullValue)
                            {
                                gpSate[i].prn = prn;
                                gpSate[i].inUse = true;
                                return true;
                            }
                            else if (gpSate[i].prn == prn)
                            {
                                gpSate[i].inUse = true;
                                return true;
                            }
                            break;
                        case SateType.Glonass:
                            if (glSate[i].prn == NullValue)
                            {
                                glSate[i].prn = prn;
                                glSate[i].inUse = true;
                                return true;
                            }
                            else if (glSate[i].prn == prn)
                            {
                                glSate[i].inUse = true;
                                return true;
                            } 
                            break;
                        case SateType.Beidou:
                            if (bdSate[i].prn == NullValue)
                            {
                                bdSate[i].prn = prn;
                                bdSate[i].inUse = true;
                                return true;
                            }
                            else if (bdSate[i].prn == prn)
                            {
                                bdSate[i].inUse = true;
                                return true;
                            } 
                            break;
                        case SateType.Navic:
                            if (giSate[i].prn == NullValue)
                            {
                                giSate[i].prn = prn;
                                giSate[i].inUse = true;
                                return true;
                            }
                            else if (giSate[i].prn == prn)
                            {
                                giSate[i].inUse = true;
                                return true;
                            }
                            break;
                        case SateType.Galileo:
                            break;
                    }
                }
                return false;
            }
            private bool UpdateSnr(int prn, int snr, SateType type)
            {
                for (int i = 0; i < MaxSattellite; i++)
                {
                    switch (type)
                    {
                        case SateType.Gps:
                            if (prn < 1 || prn > 32)
                            {   //pass the special satellite for gps. In different module it may not have these satellite.
                                return false;
                            }
                            if (gpSate[i].prn == NullValue)
                            {
                                gpSate[i].prn = prn;
                                gpSate[i].snr = snr;
                                gpSate[i].inUse = false;
                                return true;
                            }
                            else if (gpSate[i].prn == prn)
                            {
                                gpSate[i].snr = snr;
                                return true;
                            }
                            break;
                        case SateType.Glonass:
                            if (glSate[i].prn == NullValue)
                            {
                                glSate[i].prn = prn;
                                glSate[i].snr = snr;
                                glSate[i].inUse = false;
                                return true;
                            }
                            else if (glSate[i].prn == prn)
                            {
                                glSate[i].snr = snr;
                                return true;
                            }
                            break;
                        case SateType.Beidou:
                            if (bdSate[i].prn == NullValue)
                            {
                                bdSate[i].prn = prn;
                                bdSate[i].snr = snr;
                                bdSate[i].inUse = false;
                                return true;
                            }
                            else if (bdSate[i].prn == prn)
                            {
                                bdSate[i].snr = snr;
                                return true;
                            } 
                            break;
                        case SateType.Navic:
                            if (giSate[i].prn == NullValue)
                            {
                                giSate[i].prn = prn;
                                giSate[i].snr = snr;
                                giSate[i].inUse = false;
                                return true;
                            }
                            else if (giSate[i].prn == prn)
                            {
                                giSate[i].snr = snr;
                                return true;
                            }
                            break;
                        case SateType.Galileo:
                            break;
                    }
                }
                return false;
            }
            private sateInfo[] GetSateArray(SateType t)
            {
                switch (t)
                {
                    case SateType.Gps:
                        return gpSate;
                    case SateType.Glonass:
                        return glSate;
                    case SateType.Beidou:
                        return bdSate;
                    case SateType.Navic:
                        return giSate;
                    case SateType.Galileo:
                        return null;
                    default:
                        return null;
                }
            }
        }

        public ParsingStatus parsingStat = new ParsingStatus();

        public static bool CheckNmea(String s)
        {
            if (s.Length < 6)
            {
                return false;
            }
            if (s[0] != '$')
            {
                return false;
            }
            if(s[s.Length-2] != 0x0d && s[s.Length-1] != 0x0a)
            {
                return false;
            }
            if (s[s.Length - 5] != '*')
            {
                return false;
            }
            byte checkSum = 0;
            for (int i = 1; i < s.Length - 5; ++i)
            {
                checkSum ^= (byte)s[i];
            }
            String checkString = s.Substring(s.Length - 4, 2);
            byte b = Convert.ToByte(s.Substring(s.Length - 4, 2), 16);
            if (checkSum != Convert.ToByte(s.Substring(s.Length - 4, 2), 16))
            {
                return false;
            }
            return true;
        }
        public static byte CheckBinaryCommand(byte[] cmd, int l)
        {
            if (l < 8)
            {
                return 0;
            }
            if (cmd[0] != 0xa0 || cmd[1] != 0xa1)
            {   //check header format
                return 0;
            }
            if (cmd[l - 2] != 0x0d && cmd[l - 1] != 0x0a)
            {   //check tail format
                return 0;
            }

            int s = (cmd[2] << 8) | cmd[3];
            if (s != l - 7)
            {   //maybe contain 0x0d 0x0a, must read one more line.
                return 0;
            }

            byte checkSum = 0;
            for (int i = 0; i < s; ++i)
            {
                checkSum ^= (byte)cmd[i + 4];
            }

            if (checkSum != cmd[l - 3])
            {   //checksum error
                return 0;
            }
            return cmd[4];
        }

        [Flags]
        public enum ParsingResult : ulong
        {
            None,
            UpdateFixPosition = 1UL << 1,
            UpdateSate = 1UL << 2,
            UpdateLocation = 1UL << 3,
            UpdateFixMode = 1UL << 4,

            Reboot,
        }

        public ParsingResult ParsingNmea(String s)
        {
            ParsingResult status = ParsingResult.None;
            NmeaTypeIdentify.NmeaType t = nti.GetNmeaType(s);
            switch (t)
            {
                case NmeaTypeIdentify.NmeaType.GNGGA:
                    status |= ParsingGga(s, t);
                    break;
                case NmeaTypeIdentify.NmeaType.GPGSA:
                case NmeaTypeIdentify.NmeaType.GLGSA:
                case NmeaTypeIdentify.NmeaType.BDGSA:
                case NmeaTypeIdentify.NmeaType.GIGSA:
                case NmeaTypeIdentify.NmeaType.GNGSA:
                    status = ParsingGsa(s, t);
                    break;
                case NmeaTypeIdentify.NmeaType.GPGSV:
                case NmeaTypeIdentify.NmeaType.GLGSV:
                case NmeaTypeIdentify.NmeaType.BDGSV:
                case NmeaTypeIdentify.NmeaType.GIGSV:
                case NmeaTypeIdentify.NmeaType.GNGSV:
                    status = ParsingGsv(s, t);
                    break;
                case NmeaTypeIdentify.NmeaType.PSTI:
                    status = ParsingPsti(s, t);
                    break;
                default:
                    break;
            }

            return status;
        }

        private class NmeaTypeIdentify
        {

            public enum NmeaType
            {
                Unknown,
                GPGSA, GLGSA, BDGSA, GNGSA, GIGSA,
                GPGSV, GLGSV, BDGSV, GNGSV, GIGSV,
                GNRMC, GNGGA, PSTI
            }
            Dictionary<string, NmeaType> d = new Dictionary<string, NmeaType>();
            public NmeaTypeIdentify()
            {
                d.Add("GPGSA", NmeaType.GPGSA);
                d.Add("GLGSA", NmeaType.GLGSA);
                d.Add("BDGSA", NmeaType.BDGSA);
                d.Add("GIGSA", NmeaType.GIGSA);
                d.Add("GNGSA", NmeaType.GNGSA);

                d.Add("GPGSV", NmeaType.GPGSV);
                d.Add("GLGSV", NmeaType.GLGSV);
                d.Add("BDGSV", NmeaType.BDGSV);
                d.Add("GIGSV", NmeaType.GIGSV);
                d.Add("GNGSV", NmeaType.GNGSV);

                d.Add("GPRMC", NmeaType.GNRMC);
                d.Add("GLRMC", NmeaType.GNRMC);
                d.Add("BDRMC", NmeaType.GNRMC);
                d.Add("GIRMC", NmeaType.GNRMC);
                d.Add("GNRMC", NmeaType.GNRMC);

                d.Add("GPGGA", NmeaType.GNGGA);
                d.Add("GLGGA", NmeaType.GNGGA);
                d.Add("BDGGA", NmeaType.GNGGA);
                d.Add("GIGGA", NmeaType.GNGGA);
                d.Add("GNGGA", NmeaType.GNGGA);

                d.Add("PSTI,", NmeaType.PSTI);
            }

            public NmeaType GetNmeaType(String s)
            {
                if(s.Length < 7)
                {
                    return NmeaType.Unknown;
                }

                String k = s.Substring(1, 5);
                if (d.ContainsKey(k))
	            {
	                return d[k];
	            }   
                return NmeaType.Unknown;
            }
        }

        NmeaTypeIdentify nti = new NmeaTypeIdentify();
        private ParsingResult ParsingGsa(String s, NmeaTypeIdentify.NmeaType t)
        {
            char[] delimiterChars = { ',', '*' };
            String[] param = s.Split(delimiterChars);
            if (param.Length < 18)
            {
                return ParsingResult.None;
            }

            int systemId = 0;
            if(param.Length >= 20)
            {   //GSA field 18 is system ID.
                systemId = Convert.ToInt32(param[18]);
            }

            int posFix = Convert.ToInt32(param[2]);
            for (int i = 3; i < 15; i++)
            {
                String pi = param[i];
                if(pi.Length <= 0)
                {
                    break;
                }
                int prn = Convert.ToInt32(pi);
                switch (t)
                {
                    case NmeaTypeIdentify.NmeaType.GPGSA:
                        parsingStat.AddInUseGpsPrn(prn);
                        break;
                    case NmeaTypeIdentify.NmeaType.GLGSA:
                        parsingStat.AddInUseGlonassPrn(prn);
                        break;
                    case NmeaTypeIdentify.NmeaType.BDGSA:
                        parsingStat.AddInUseBeidouPrn(prn);
                        break;
                    case NmeaTypeIdentify.NmeaType.GIGSA:
                        parsingStat.AddInUseNavicPrn(prn);
                        break;
                    case NmeaTypeIdentify.NmeaType.GNGSA:
                        parsingStat.AddInUseGNPrn(prn, systemId);
                        break;
                }
            }
            return ParsingResult.None;
        }

        private int totalGsv = -1;
        private int lastGsv = -1;
        private ParsingResult ParsingGsv(String s, NmeaTypeIdentify.NmeaType t)
        {
            char[] delimiterChars = { ',', '*' };
            String[] param = s.Split(delimiterChars);
            if ((param.Length != 9 && param.Length != 13 && param.Length != 17 && param.Length != 21) &&
                (param.Length != 10 && param.Length != 14 && param.Length != 18 && param.Length != 22))     //Has Signal ID
            {
                return ParsingResult.None;
            }

            int total = Convert.ToInt32(param[1]);
            int current = Convert.ToInt32(param[2]);
            int totalSate = Convert.ToInt32(param[3]);

            if(current==1)
            {
                totalGsv = total;
                lastGsv = current;    
            }

            int MaxParam;
            switch (param.Length)
            {
                case 9:
                    MaxParam = 8;
                    break;
                case 13:
                    MaxParam = 12;
                    break;
                case 17:
                    MaxParam = 16;
                    break;
                case 21:
                    MaxParam = 20;
                    break;
                //Has Signal ID
                case 10:
                    MaxParam = 8;
                    break;
                case 14:
                    MaxParam = 12;
                    break;
                case 18:
                    MaxParam = 16;
                    break;
                case 22:
                    MaxParam = 20;
                    break;
                default:
                    MaxParam = 0;
                    break;
            }
            for (int i = 4; i < MaxParam; i += 4)
            {
                String pi = param[i];
                if (pi.Length <= 0)
                {
                    break;
                }
                String pi3 = param[i + 3];
                int id = Convert.ToInt32(pi);
                int snr = (pi3.Length <= 0) ? 0 : Convert.ToInt32(pi3);

                switch (t)
                {
                    case NmeaTypeIdentify.NmeaType.GPGSV:
                        parsingStat.UpdateGNSnr(id, snr);
                        break;
                    case NmeaTypeIdentify.NmeaType.GLGSV:
                        parsingStat.UpdateGlonassSnr(id, snr);
                        break;
                    case NmeaTypeIdentify.NmeaType.BDGSV:
                        parsingStat.UpdateBeidouSnr(id, snr);
                        break;
                    case NmeaTypeIdentify.NmeaType.GIGSV:
                        parsingStat.UpdateNavicSnr(id, snr);
                        break;
                    case NmeaTypeIdentify.NmeaType.GNGSV:
                        parsingStat.UpdateGNSnr(id, snr);
                        break;
                }
            }

            if(total==current)
            {
                totalGsv = -1;
                lastGsv = -1;
                return ParsingResult.UpdateSate;
            }
            return ParsingResult.None;
        }
        private ParsingResult ParsingPsti(String s, NmeaTypeIdentify.NmeaType t)
        {
            char[] delimiterChars = { ',', '*' };
            String[] param = s.Split(delimiterChars);
            if (param[1] == "050")
            {
                return ParsingPsti50(param, t);
            }
            return ParsingResult.None;
        }

        private int totalPsti50 = -1;
        private int lastPsti50 = -1;
        private ParsingResult ParsingPsti50(String[] param, NmeaTypeIdentify.NmeaType t)
        {
            if (param.Length != 18)
            {
                return ParsingResult.None;
            }

            int total = Convert.ToInt32(param[2]);
            int current = Convert.ToInt32(param[3]);
            int totalSate = Convert.ToInt32(param[4]);

            if (current == 1)
            {
                totalPsti50 = total;
                lastPsti50 = current;
            }

            int MaxParam = 17;
            for (int i = 5; i < MaxParam; i += 3)
            {
                String pi = param[i];
                if (pi.Length <= 0)
                {
                    break;
                }
                String pi2 = param[i + 2];
                int id = Convert.ToInt32(pi);
                int snr = (pi2.Length <= 0) ? 0 : Convert.ToInt32(pi2);
                parsingStat.UpdateGlonassSnr(id, snr);
            }

            if (total == current)
            {
                totalGsv = -1;
                lastGsv = -1;
                return ParsingResult.UpdateSate;
            }
            return ParsingResult.None;
        }

        private ParsingResult ParsingGga(String s, NmeaTypeIdentify.NmeaType t)
        {
            char[] delimiterChars = { ',', '*' };
            String[] param = s.Split(delimiterChars);
            if (param.Length < 10)
            {
                return ParsingResult.None;
            }
            int posFix = 0;
            try
            {
                if (param[1].Length > 6)
                {
                    parsingStat.timeString = param[1];
                }
                if (param[2].Length > 5)
                {
                    parsingStat.latString = param[2];
                }
                if (param[3].Length == 1)
                {
                    parsingStat.ns = param[3][0];
                }
                if (param[4].Length > 5)
                {
                    parsingStat.lonString = param[4];
                }
                if (param[5].Length == 1)
                {
                    parsingStat.ew = param[5][0];
                }
                if (param[9].Length > 2)
                {
                    parsingStat.altitudeString = param[9];
                }
                posFix = Convert.ToInt32(param[6]);
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }

            if (parsingStat.SetPositionFixResult(posFix))
            {
                return ParsingResult.UpdateFixMode;
            }
            return ParsingResult.None;
        }

    }
}
