using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ModuleTestV8
{
    class TestModule
    {
        private const int DefaultCmdTimeout = 1000;
        private const int ScanBaudCount = 2;
        public static GpsMsgParser.ParsingStatus[] dvResult;
        public static UInt32 gdClockOffset = 0;
        public enum ClearType
        {
            All,
            Upper,
            Bottom
        }

        public static void ClearResult(ClearType ct)
        {
            if (dvResult == null)
            {
                return;
            }

            int start = 0;
            int limit = ModuleTestForm.ModuleCount;
            if(ct == ClearType.Upper)
            {
                limit = 5;
            }
            if(ct == ClearType.Bottom)
            {
                start = 5;
            }
            for (int i = start; i < limit; ++i)
            {
                dvResult[i].ClearAllSate();
                dvResult[i].SetPositionFixResult(0);
            }
        }

        public TestModule()
        {
            dvResult = new GpsMsgParser.ParsingStatus[ModuleTestForm.ModuleCount];
            for (int i = 0; i < ModuleTestForm.ModuleCount; i++)
            {
                dvResult[i] = new GpsMsgParser.ParsingStatus();
            }
        }

        private static EventWaitHandle controllerEvent = new ManualResetEvent(false);
#if !(NO_LOCK)
        private static Object _lockWaitingCount = new Object();
        private static Object _lockTotalCount = new Object();
#endif
        private static int _totalTestCount = 0;
        private static int _waitingCount = 0;
#if !(NO_LOCK)
        private static void _IncWaitingCount()
        {
            lock (_lockWaitingCount)
            {
                _waitingCount++;
            }
        }

        private static void _DecWaitingCount()
        {
            lock (_lockWaitingCount)
            {
                _waitingCount--;
            }
        }

        private static void _ClearWaitingCount()
        {
            lock (_lockWaitingCount)
            {
                _waitingCount = 0;
            }
        }

        private static int _GetWaitingCount()
        {
            return _waitingCount;
        }

        private static void _IncTotalCount()
        {
            lock (_lockTotalCount)
            {
                _totalTestCount++;
            }
        }

        private static void _DecTotalCount()
        {
            lock (_lockTotalCount)
            {
                _totalTestCount--;
            }
        }

        private static void _ClearTotalCount()
        {
            lock (_lockTotalCount)
            {
                _totalTestCount = 0;
            }
        }

        private static int _GetTotalCount()
        {
            return _totalTestCount;
        }
#else
        private static int d = 200;
        private static void _IncWaitingCount() { Thread.Sleep(rand.Next(0, d)); _waitingCount++; }
        private static void _DecWaitingCount() { Thread.Sleep(rand.Next(0, d)); _waitingCount--; }
        private static void _ClearWaitingCount() { Thread.Sleep(rand.Next(0, d)); _waitingCount = 0; }
        private static int _GetWaitingCount() { Thread.Sleep(rand.Next(0, d)); return _waitingCount; }
        private static void _IncTotalCount() { Thread.Sleep(rand.Next(0, d)); _totalTestCount++; }
        private static void _DecTotalCount() { Thread.Sleep(rand.Next(0, d)); _totalTestCount--; }
        private static void _ClearTotalCount() { Thread.Sleep(rand.Next(0, d)); _totalTestCount = 0; }
        private static int _GetTotalCount() { Thread.Sleep(rand.Next(0, d)); return _totalTestCount; }
#endif
        public static bool IncreaseWaitingCount()
        {
            lock (_lockWaitingCount)
            {
                _IncWaitingCount();
                if (_GetWaitingCount() >= _GetTotalCount())
                {
                    controllerEvent.Set();
                    _ClearWaitingCount();
                    return true;
                }
                controllerEvent.Reset();
            }
            return false;
        }

        public static void ResetWaitingCount()
        {
            _ClearWaitingCount();
            controllerEvent.Reset();
        }

        public static void IncreaseTotalTestCount()
        {
            _IncTotalCount();
        }

        public static void DecreaseTotalTestCount()
        {
            _DecTotalCount();
            if (_GetTotalCount() <= _GetWaitingCount())
            {
                controllerEvent.Set();
            }
        }

        public static void ResetTotalTestCount()
        {
            _ClearTotalCount();
            _ClearWaitingCount();
            controllerEvent.Reset();
        }

        private void CheckControllerEvent(WorkerParam p, int timeCount)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Before WaitOn.";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            if (!IncreaseWaitingCount())
            {
                int waitCount = timeCount;
                while (!controllerEvent.WaitOne(10))
                {
                    if (--waitCount <= 0)
                    {
                        //break;
                    }
                }
            }
            r.output = "After WaitOn.";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(100);
        }

        static int motoPosition = 0;
        public static bool ResetMotoPosition(string mcuPort)
        {
            if (motoPosition == 1 || motoPosition == 2)
            {
                if (controllerIO == null)
                {
                    return false;
                }
                GPS_RESPONSE rep;
                rep = controllerIO.SetControllerMoto(0, 31, 30, 28, 20, 16, motorDelay, motorStep);

                if (GPS_RESPONSE.ACK != rep)
                {
                    return false;
                }
                Thread.Sleep(3000);
                motoPosition = 0;
            }
            motoPosition = 0;
            return true;
        }

        const int GpsCompareCount = 3;
        const int GlonassCompareCount = 2;
        const int BeidouCompareCount = 2;
        const int NavicCompareCount = 2;
        private bool CompareResult(WorkerParam p, ref WorkerReportParam r, GpsMsgParser.SateType t)
        {
            GpsMsgParser.ParsingStatus.sateInfo[] dvSate = null;
            int snrLimit = 0;
            String usingSnrTxt;
            String dvSnrTxt;
            String gdSnrTxt;
            String dvSnrAvgTxt;
            String gdSnrAvgTxt;
            String gdSnrLimitErrorTxt;
            String dvSnrLimitErrorTxt;
            //int passSel = 0;
            int testSnrUpper = 0;
            int testSnrLower = 0;
            String dvTestPassTxt;
            double snrOffset = 0.0;
            int CompareCount = 0;
            switch (t)
            {
                case GpsMsgParser.SateType.Gps:
                    dvSate = dvResult[p.index].GetSortedGpsSateArray();
                    snrLimit = p.profile.gpSnrLimit;
                    usingSnrTxt = "Using GPS PRN : ";
                    dvSnrTxt = "Device GPS SNR : ";
                    gdSnrTxt = "Golden GPS SNR : ";
                    dvSnrAvgTxt = "Device average GPS SNR" + "(" + p.gpSnrOffset.ToString() + ") : ";
                    gdSnrAvgTxt = "Golden average GPS SNR : ";
                    gdSnrLimitErrorTxt = "Golden sample GPS SNR over the limit.";
                    dvSnrLimitErrorTxt = "Device sample GPS SNR over the limit.";
                    //passSel = p.profile.gpPassSel;
                    testSnrUpper = p.profile.gpSnrUpper;
                    testSnrLower = p.profile.gpSnrLower;
                    dvTestPassTxt = "Device GPS SNR test pass.";
                    snrOffset = p.gpSnrOffset;
                    CompareCount = GpsCompareCount;
                    break;
                case GpsMsgParser.SateType.Glonass:
                    dvSate = dvResult[p.index].GetSortedGlonassSateArray();
                    snrLimit = p.profile.glSnrLimit;
                    usingSnrTxt = "Using Glonass PRN : ";
                    dvSnrTxt = "Device Glonass SNR : ";
                    gdSnrTxt = "Golden Glonass SNR : ";
                    dvSnrAvgTxt = "Device average Glonass SNR" + "(" + p.glSnrOffset.ToString() + ") : ";
                    gdSnrAvgTxt = "Golden average Glonass SNR : ";
                    gdSnrLimitErrorTxt = "Golden sample Glonass SNR over the limit.";
                    dvSnrLimitErrorTxt = "Device sample Glonass SNR over the limit.";
                    //passSel = p.profile.glPassSel;
                    testSnrUpper = p.profile.glSnrUpper;
                    testSnrLower = p.profile.glSnrLower;
                    dvTestPassTxt = "Device Glonass SNR test pass.";
                    snrOffset = p.glSnrOffset;
                    CompareCount = GlonassCompareCount;
                    break;
                case GpsMsgParser.SateType.Beidou:
                    dvSate = dvResult[p.index].GetSortedBeidouSateArray();
                    snrLimit = p.profile.bdSnrLimit;
                    usingSnrTxt = "Using Beidou PRN : ";
                    dvSnrTxt = "Device Beidou SNR : ";
                    gdSnrTxt = "Golden Beidou SNR : ";
                    dvSnrAvgTxt = "Device average Beidou SNR" + "(" + p.bdSnrOffset.ToString() + ") : ";
                    gdSnrAvgTxt = "Golden average Beidou SNR : ";
                    gdSnrLimitErrorTxt = "Golden sample Beidou SNR over the limit.";
                    dvSnrLimitErrorTxt = "Device sample Beidou SNR over the limit.";
                    //passSel = p.profile.bdPassSel;
                    testSnrUpper = p.profile.bdSnrUpper;
                    testSnrLower = p.profile.bdSnrLower;
                    dvTestPassTxt = "Device Beidou test pass.";
                    snrOffset = p.bdSnrOffset;
                    CompareCount = BeidouCompareCount;
                    break;
                case GpsMsgParser.SateType.Navic:
                    dvSate = dvResult[p.index].GetSortedNavicSateArray();
                    snrLimit = p.profile.giSnrLimit;
                    usingSnrTxt = "Using Navic PRN : ";
                    dvSnrTxt = "Device Navic SNR : ";
                    gdSnrTxt = "Golden Navic SNR : ";
                    dvSnrAvgTxt = "Device average Navic SNR" + "(" + p.giSnrOffset.ToString() + ") : ";
                    gdSnrAvgTxt = "Golden average Navic SNR : ";
                    gdSnrLimitErrorTxt = "Golden sample Navic SNR over the limit.";
                    dvSnrLimitErrorTxt = "Device sample Navic SNR over the limit.";
                    //passSel = p.profile.giPassSel;
                    testSnrUpper = p.profile.giSnrUpper;
                    testSnrLower = p.profile.giSnrLower;
                    dvTestPassTxt = "Device Navic test pass.";
                    snrOffset = p.giSnrOffset;
                    CompareCount = NavicCompareCount;
                    break;
                default:
                    dvSate = dvResult[p.index].GetSortedGpsSateArray();
                    snrLimit = p.profile.gpSnrLimit;
                    usingSnrTxt = "Using GPS PRN : ";
                    dvSnrTxt = "Device GPS SNR : ";
                    gdSnrTxt = "Golden GPS SNR : ";
                    dvSnrAvgTxt = "Device average GPS SNR" + "(" + p.gpSnrOffset.ToString() + ") : ";
                    gdSnrAvgTxt = "Golden average GPS SNR : ";
                    gdSnrLimitErrorTxt = "Golden sample Gps SNR over the limit.";
                    dvSnrLimitErrorTxt = "Device sample Gps SNR over the limit.";
                    //passSel = p.profile.gpPassSel;
                    testSnrUpper = p.profile.gpSnrUpper;
                    testSnrLower = p.profile.gpSnrLower;
                    dvTestPassTxt = "Device Gps SNR test pass.";
                    snrOffset = p.gpSnrOffset;
                    CompareCount = GpsCompareCount;
                    break;
            }

            if (dvSate[2].snr <= 0 || dvSate[1].snr <= 0 || dvSate[0].snr <= 0)
            {
                //return false;
            }

            GpsMsgParser.ParsingStatus.sateInfo[] selGd = new GpsMsgParser.ParsingStatus.sateInfo[CompareCount];
            GpsMsgParser.ParsingStatus.sateInfo[] selDv = new GpsMsgParser.ParsingStatus.sateInfo[CompareCount];
            int idxGd = 0;
            int idxDv = 0;
            do
            {
                if (dvSate[idxDv].snr < 0)
                {
                    break;
                }

                if (t == GpsMsgParser.SateType.Glonass &&
                    (dvSate[idxDv].prn == 6 || dvSate[idxDv].prn == -7))
                {   //Pass glonass satellite with off-peak frequency.
                    ++idxDv;
                    continue;
                }

                if (dvResult[0].GetSnr(dvSate[idxDv].prn, t).snr > 0)
                {
                    selGd[idxGd] = dvResult[0].GetSnr(dvSate[idxDv].prn, t);
                    selDv[idxGd] = dvSate[idxDv];
                    ++idxGd;
                }
                ++idxDv;
            }
            while (idxGd < CompareCount);
            if (idxGd < CompareCount)
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            StringBuilder sb = new StringBuilder(128);
            sb.Append(usingSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selDv[i].prn.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            sb.Remove(0, sb.Length);
            sb.Append(dvSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selDv[i].snr.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            sb.Remove(0, sb.Length);
            sb.Append(gdSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selGd[i].snr.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            double gdAverage = 0, dvAverage = 0;
            for (int i = 0; i < CompareCount; ++i)
            {
                gdAverage += selGd[i].snr;
                dvAverage += selDv[i].snr;
            }
            gdAverage /= CompareCount;
            dvAverage /= CompareCount;
            dvAverage += snrOffset;

            r.output = dvSnrAvgTxt + dvAverage.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            r.output = gdSnrAvgTxt + gdAverage.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (gdAverage > snrLimit)
            {
                r.output = gdSnrLimitErrorTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (dvAverage > p.profile.gpSnrLimit)
            {
                r.output = dvSnrLimitErrorTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            double diff = dvAverage - gdAverage;
            if (diff <= testSnrUpper &&
                diff >= testSnrLower)
            {
                r.output = dvTestPassTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }
            return false;
        }

        private bool CompareBeidouResult(WorkerParam p, ref WorkerReportParam r)
        {
            GpsMsgParser.ParsingStatus.sateInfo[] dvSate = null;
            int snrLimit = 0;
            String usingSnrTxt;
            String dvSnrTxt;
            String gdSnrTxt;
            String dvSnrAvgTxt;
            String gdSnrAvgTxt;
            String gdSnrLimitErrorTxt;
            String dvSnrLimitErrorTxt;
            //int passSel = 0;
            int testSnrUpper = 0;
            int testSnrLower = 0;
            String dvTestPassTxt;
            double snrOffset = 0.0;
            int CompareCount = 0;

            dvSate = dvResult[p.index].GetSortedBeidouSateArray();
            snrLimit = p.profile.bdSnrLimit;
            usingSnrTxt = "Using Beidou PRN : ";
            dvSnrTxt = "Device Beidou SNR : ";
            gdSnrTxt = "Golden Beidou SNR : ";
            dvSnrAvgTxt = "Device average Beidou SNR" + "(" + p.bdSnrOffset.ToString() + ") : ";
            gdSnrAvgTxt = "Golden average Beidou SNR : ";
            gdSnrLimitErrorTxt = "Golden sample Beidou SNR over the limit.";
            dvSnrLimitErrorTxt = "Device sample Beidou SNR over the limit.";
            //passSel = p.profile.bdPassSel;
            testSnrUpper = p.profile.bdSnrUpper;
            testSnrLower = p.profile.bdSnrLower;
            dvTestPassTxt = "Device Beidou test pass.";
            snrOffset = p.bdSnrOffset;
            CompareCount = BeidouCompareCount;

            if (dvSate[2].snr <= 0 || dvSate[1].snr <= 0 || dvSate[0].snr <= 0)
            {
                //return false;
            }

            GpsMsgParser.ParsingStatus.sateInfo[] selGd = new GpsMsgParser.ParsingStatus.sateInfo[CompareCount];
            GpsMsgParser.ParsingStatus.sateInfo[] selDv = new GpsMsgParser.ParsingStatus.sateInfo[CompareCount];
            int idxGd = 0;
            int idxDv = 0;
            do
            {
                if (dvSate[idxDv].snr < 0)
                {
                    break;
                }

                if (dvResult[0].GetRealBeidouSnr(dvSate[idxDv].prn, GpsMsgParser.SateType.Beidou).snr > 0)
                {
                    selGd[idxGd] = dvResult[0].GetRealBeidouSnr(dvSate[idxDv].prn, GpsMsgParser.SateType.Beidou);
                    selDv[idxGd] = dvSate[idxDv];
                    ++idxGd;
                }
                ++idxDv;
            }
            while (idxGd < CompareCount);
            if (idxGd < CompareCount)
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            StringBuilder sb = new StringBuilder(128);
            sb.Append(usingSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selDv[i].prn.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            sb.Remove(0, sb.Length);
            sb.Append(dvSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selDv[i].snr.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            sb.Remove(0, sb.Length);
            sb.Append(gdSnrTxt);
            for (int i = 0; i < CompareCount; ++i)
            {
                sb.Append(selGd[i].snr.ToString());
                if (i < CompareCount - 1)
                {
                    sb.Append(", ");
                }
            }
            r.output = sb.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            double gdAverage = 0, dvAverage = 0;
            for (int i = 0; i < CompareCount; ++i)
            {
                gdAverage += selGd[i].snr;
                dvAverage += selDv[i].snr;
            }
            gdAverage /= CompareCount;
            dvAverage /= CompareCount;
            dvAverage += snrOffset;

            r.output = dvSnrAvgTxt + dvAverage.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            r.output = gdSnrAvgTxt + gdAverage.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (gdAverage > snrLimit)
            {
                r.output = gdSnrLimitErrorTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (dvAverage > p.profile.gpSnrLimit)
            {
                r.output = dvSnrLimitErrorTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            double diff = dvAverage - gdAverage;
            if (diff <= testSnrUpper &&
                diff >= testSnrLower)
            {
                r.output = dvTestPassTxt;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }
            return false;
        }

        public bool CheckAdc(uint adc)
        {
            const int adcRangeTop = 0x70;
            const int adcRangeBottom = 0x10;
            const int adcCenter = 0x300;
            if (adc > (adcCenter - adcRangeBottom) && adc < (adcCenter + adcRangeTop))
            {
                return true;
            }
            return false;
        }

        public static EventWaitHandle antennaEvent = new AutoResetEvent(false);
        private bool OpenDevice(WorkerParam p, WorkerReportParam r, int baudIdx)
        {
            GPS_RESPONSE rep = p.gps.Open(p.comPort, baudIdx);

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                //EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " in " +
                    p.gps.GetBaudRate().ToString() + " successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }
        private bool DoHotStart(WorkerParam p, WorkerReportParam r, int timeout)
        {
            Int32 ts = 0, ds = 0;
            if (p.parser.parsingStat.dateString.Length > 0)
            {
                ds = Convert.ToInt32(p.parser.parsingStat.dateString);
            }
            else
            {
                ds = DateTime.UtcNow.Year * 10000 + DateTime.UtcNow.Month * 100 + DateTime.UtcNow.Day;
            }

            if (p.parser.parsingStat.timeString.Length > 0)
            {
                ts = Convert.ToInt32(p.parser.parsingStat.timeString.Split('.')[0]);
            }
            else
            {
                ts = DateTime.UtcNow.Hour * 10000 + DateTime.UtcNow.Minute * 100 + DateTime.UtcNow.Second;
            }

            DateTime t = new DateTime(2000 + ds / 10000, (ds / 100) % 100, ds % 100,
                ts / 10000, (ts / 100) % 100, ts % 100);

            Int16 lat = 2400, lon = 12100, alt = 0;
            //SendHotStart(int timeout, Int16 lat, Int16 lon, Int16 alt, DateTime time)
            GPS_RESPONSE rep = p.gps.SendHotStart(timeout, lat, lon, alt, t);
            if (GPS_RESPONSE.ACK == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Hot start successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Hot start failed!";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return false;
        }

        private bool DoColdStart(WorkerParam p, WorkerReportParam r, int retry)
        {
            for (int tryCount = 3; tryCount > 0; --tryCount)
            {
                GPS_RESPONSE rep = p.gps.SendColdStart(retry, 1500);
                if (GPS_RESPONSE.ACK == rep || GPS_RESPONSE.NACK == rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Cold start successfully";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return true;
                }
            }

            r.reportType = WorkerReportParam.ReportType.ShowError;
            p.error = WorkerParam.ErrorType.ColdStartError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return false;
        }

        private bool DoWarmStart(WorkerParam p, WorkerReportParam r, bool romType, int lon, int lat, int alt, int timeout)
        {
            GPS_RESPONSE rep = p.gps.SendWarmStart(romType, lon, lat, alt, timeout);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Warm start successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(500);  //For venus 6 testing.
            return true;
        }

        private bool SetTestHighBaudRate(WorkerParam p, WorkerReportParam r)
        {
            if (p.profile.fwProfile.dvBaudRate >= 115200)
            {
                return true;
            }

            GPS_RESPONSE rep = p.gps.ChangeBaudrate((byte)5, 2, false);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ChangeBaudRateFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Change baud rate successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool DoQueryVersion(WorkerParam p, WorkerReportParam r, bool isSlave)
        {
            String kVer = "";
            String sVer = "";
            String rev = "";
            GPS_RESPONSE rep = p.gps.QueryVersion(DefaultCmdTimeout, (byte)((isSlave) ? 2 : 1), ref kVer, ref sVer, ref rev);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.QueryVersionError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (isSlave)
            {
                if (!p.profile.slaveFwProfile.kVersion.Equals(kVer) ||
                    !p.profile.slaveFwProfile.sVersion.Equals(sVer) ||
                    !p.profile.slaveFwProfile.rVersion.Equals(rev))
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.FirmwareVersionError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
            }
            else
            {
                if (!p.profile.fwProfile.kVersion.Equals(kVer) ||
                    !p.profile.fwProfile.sVersion.Equals(sVer) ||
                    !p.profile.fwProfile.rVersion.Equals(rev))
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.FirmwareVersionError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Check version pass";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool TestNavSparkIo(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            String dbgOutput = "";
            rep = p.gps.SendLoaderDownload(ref dbgOutput, p.profile.dlBaudSel, false);
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestLoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Loader download successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            rep = p.gps.UploadLoader(Properties.Resources.NavSparkIoTester);
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUploadLoaderFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Upload loader successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(1000);


            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Reset();
            w.Start();

            bool ioTestPass = false;
            uint adc = 0;
            while (w.ElapsedMilliseconds < 5000)
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);

                if (line.Length > 0)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                if (line.Contains("FINISH"))
                {
                    ioTestPass = true;
                    break;
                }
                if (line.Contains("FAIL"))
                {
                    break;
                }
                if (line.Contains("ADC:"))
                {
                    uint a = Convert.ToUInt32(line.Split(' ')[2], 16);
                    if (CheckAdc(a))
                    {
                        adc = a;
                    }
                }
            }

            if (ioTestPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Test IO successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestIoFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (CheckAdc(adc))
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Test ADC successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAdcFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            return true;
        }

        private bool TestNavSparkMiniIo(WorkerParam p, WorkerReportParam r)
        {
            //TEST01 = Flag IoCount io1 io2 io3 io4 ......
            //Flag - bit wise for test function : 0 - IO Test, 1 - GSN MAG Test, 2 - Rtc Test
            //IoCount - Test IO pair count
            //io1, io2... - High byte - gpio pin from, Low byte gpio pin to.
            return DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0001 0004 0304 0305 1E1F 1C1D ", 5000, false);
            //     DoIoSrecTest(p, r, Properties.Resources.IoTesterSrec, "TEST01 = 0001 000F 0119 1C1D 0C0D 0E16 0809 151B 100F 1406 0002 181A 1807 0B0A 0B17 031E 031F ", 5000))
        }

        private bool DoIoSrecTest(WorkerParam p, WorkerReportParam r, string testSrec, string srecCmd, int timeout, bool useBinCmd)
        {
            String dbgOutput = "";
            GPS_RESPONSE rep = p.gps.SendLoaderDownload(ref dbgOutput, p.profile.dlBaudSel, useBinCmd);
            if (dbgOutput != "")
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = dbgOutput;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestLoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Loader download successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                rep = p.gps.UploadLoader(testSrec);
                if (GPS_RESPONSE.OK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.TestUploadLoaderFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Upload loader successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(1000);
            }
            //TEST01 = Flag IoCount io1 io2 io3 io4 ......
            //Flag - bit wise for test function : 0 - IO Test, 1 - GSN MAG Test, 2 - Rtc Test
            //IoCount - Test IO pair count
            //io1, io2... - High byte - gpio pin from, Low byte gpio pin to.
            if (srecCmd.Length > 0)
            {
                rep = p.gps.SendTestSrecCmd(srecCmd, 1000);
            }
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Reset();
            w.Start();

            bool ioTestPass = true;
            bool ioTestFinished = false;
            while (w.ElapsedMilliseconds < timeout)
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);

                if (line.Length > 0)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = line;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                if (line.Contains("FINISH"))
                {
                    ioTestFinished = true;
                    break;
                }
                if (line.Contains("FAIL"))
                {
                    ioTestPass = false;
                    break;
                }
            };

            if (!ioTestFinished || !ioTestPass)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestIoTestFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "IO Test pass";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            return true;
        }
        
        private bool DoConfigureRtkMode(WorkerParam p, WorkerReportParam r)
        {
            SkytraqGps.RtkModeInfo rtkInfo = new SkytraqGps.RtkModeInfo();
            rtkInfo.rtkMode = SkytraqGps.RtkModeInfo.RtkMode.RTK_Rover;
            rtkInfo.optMode = SkytraqGps.RtkModeInfo.RtkOperationMode.Rover_Normal;

            GPS_RESPONSE rep = p.gps.ConfigRtkModeAndOptFunction(DefaultCmdTimeout, rtkInfo, SkytraqGps.Attributes.Sram);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ConfigRtkModeError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Configure RTK mode as a normal rover";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return GPS_RESPONSE.ACK == rep;
        }

        private bool DoQueryCrc(WorkerParam p, WorkerReportParam r, bool isSlave, bool isInMaster)
        {
            uint crc = 0;
            GPS_RESPONSE rep = p.gps.QueryCrc(DefaultCmdTimeout, (byte)((isInMaster && isSlave) ? 2 : 1), ref crc);
            if(isSlave && GPS_RESPONSE.NACK == rep)
            {   //Firmware doesn't support
                return false;
            }

            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = (rep == GPS_RESPONSE.NACK) ? WorkerParam.ErrorType.QueryCrcNack : WorkerParam.ErrorType.QueryCrcTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "FW CRC: " + crc.ToString("X4") + ", File CRC: " +
                ((isSlave) ? p.profile.slaveFwProfile.crc.ToString("X4") : p.profile.fwProfile.crc.ToString("X4"));
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (!isSlave && p.profile.fwProfile.crc != crc)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.FirmwareCrcError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else if (isSlave && p.profile.slaveFwProfile.crc != crc)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.SlaveFirmwareCrcError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Check CRC pass";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool TestVoltage(WorkerParam p, WorkerReportParam r)
        {
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Get Voltage " + p.voltage[p.index - 1].ToString("##.##") + "V";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            if (p.voltage[p.index - 1] <= p.profile.volrageUpper && p.voltage[p.index - 1] >= p.profile.volrageLower)
            {
                return true;
            }
            r.reportType = WorkerReportParam.ReportType.ShowError;
            p.error = WorkerParam.ErrorType.CheckVoltageError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return false;
        }

        private bool TestRtc(WorkerParam p, WorkerReportParam r)
        {
            UInt32 rtc1 = 0, rtc2 = 0;
            GPS_RESPONSE rep = p.gps.QueryRtc(ref rtc1);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.QueryRtcError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC1 " + rtc1.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                Thread.Sleep(1010);
                rep = p.gps.QueryRtc(ref rtc2);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.QueryRtcError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get RTC2 " + rtc2.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                if ((rtc2 - rtc1) > 3 || (rtc2 - rtc1) < 1)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.CheckRtcError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Check rtc pass";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool TestAntenna(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            antennaEvent.WaitOne();
            SkytraqGps annIO = new SkytraqGps();

            for (int i = 0; i < 2; ++i)
            {
                rep = annIO.Open(p.annIoPort, 5);
                if (GPS_RESPONSE.UART_FAIL == rep)
                {
                    annIO.Close();
                }
                else
                {
                    break;
                }
            }

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.IoControllerFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                EndAntennaProcess(annIO);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open controller IO " + p.annIoPort + " in 115200 successfully.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            rep = annIO.AntennaIO(0x0A);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            Thread.Sleep(300);
            byte detect = 0;
            rep = p.gps.QueryAntennaDetect(ref detect);
            if (GPS_RESPONSE.ACK != rep || detect != 0x02)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Query antenna detect status #A pass.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            /////////////////////////////////////////////////            
            rep = annIO.AntennaIO(0x0B);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            Thread.Sleep(300);
            rep = p.gps.QueryAntennaDetect(ref detect);
            if (GPS_RESPONSE.ACK != rep || detect != 0x01)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Query antenna detect status #B pass.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            /////////////////////////////////////////////////            
            rep = annIO.AntennaIO(0x0C);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            Thread.Sleep(300);
            rep = p.gps.QueryAntennaDetect(ref detect);
            if (GPS_RESPONSE.ACK != rep || detect != 0x03)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Query antenna detect status #C pass.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            EndAntennaProcess(annIO);
            return true;
        }

        private bool TestUart2TxRx(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            //ioreg[0xf014/4] &= (~(0x3UL << 9));
            UInt32 reg = 0;
            rep = p.gps.GetRegister(DefaultCmdTimeout, 0x2000F014, ref reg);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get Reg(2000F014) successfully.";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.SetRegister(DefaultCmdTimeout, 0x2000F014, reg & (~(0x3U << 9)));
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Set Reg(2000F014) successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            /////////////////////////////////////////////////////////////////////
            rep = p.gps.GetRegister(DefaultCmdTimeout, 0x2000F078, ref reg);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get Reg(0x2000F078) successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.SetRegister(DefaultCmdTimeout, 0x2000F078, reg | (0x1U << 1));
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Set Reg(0x2000F078) successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }


            antennaEvent.WaitOne();
            SkytraqGps annIO = new SkytraqGps();

            for (int i = 0; i < 2; ++i)
            {
                rep = annIO.Open(p.annIoPort, 5);
                if (GPS_RESPONSE.UART_FAIL == rep)
                {
                    annIO.Close();
                }
                else
                {
                    break;
                }
            }

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.IoControllerFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                EndAntennaProcess(annIO);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open IO controller " + p.annIoPort + " in 115200 successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = annIO.AntennaIO(0x0A);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Set IO controller to FE00000A.";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(300);

            rep = p.gps.GetRegister(DefaultCmdTimeout, 0x20001008, ref reg);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Get GPIO Reg 0x20001008 : " + reg.ToString("X4");
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            if (GPS_RESPONSE.ACK != rep || (reg & 0x6) != 0x4)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            ///////////////////////////////////////////////////////////////////////////////////
            rep = annIO.AntennaIO(0x0B);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Set IO controller to FE00000B.";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(300);

            rep = p.gps.GetRegister(DefaultCmdTimeout, 0x20001008, ref reg);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Get GPIO Reg 0x20001008 : " + reg.ToString("X4");
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            if (GPS_RESPONSE.ACK != rep || (reg & 0x6) != 0x2)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            ///////////////////////////////////////////////////////////////////////////////////
            rep = annIO.AntennaIO(0x0C);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestAntennaFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndAntennaProcess(annIO);
                EndProcess(p);
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Set IO controller to FE00000C.";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(300);

            rep = p.gps.GetRegister(DefaultCmdTimeout, 0x20001008, ref reg);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Get GPIO Reg 0x20001008 : " + reg.ToString("X4");
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            if (GPS_RESPONSE.ACK != rep || (reg & 0x6) != 0x6)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.TestUART2Fail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            EndAntennaProcess(annIO);
            return true;
        }


        private bool GetGoldenPosition(ref int lon, ref int lat, ref int alt)
        {
            if (goldenLonString == "" || goldenLatString == "" || goldenAltString == "" ||
                goldenNS == ' ' || goldenEW == ' ')
            {
                return false;
            }
            String[] d = goldenLonString.Split('.');
            lon = Convert.ToInt32(d[0]) + ((d[1][0] >= '5') ? 1 : 0);
            lon *= (goldenEW == 'W') ? -1 : 1;

            d = goldenLatString.Split('.');
            lat = Convert.ToInt32(d[0]) + ((d[1][0] >= '5') ? 1 : 0);
            lat *= (goldenEW == 'S') ? -1 : 1;

            d = goldenAltString.Split('.');
            alt = Convert.ToInt32(d[0]) + ((d[1][0] >= '5') ? 1 : 0);

            return true;
        }

        private bool TestSnr(WorkerParam p, WorkerReportParam r)
        {
            bool gpSnrPass = !p.profile.testGpSnr;
            bool glSnrPass = !p.profile.testGlSnr;
            bool bdSnrPass = !p.profile.testBdSnr;
            bool giSnrPass = !p.profile.testGiSnr;
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;

            //if (p.profile.testGlSnr && !p.profile.waitPositionFix && !p.profile.testToRtkFix && !p.profile.testToRtkFloat)
            if (p.profile.testGlSnr && !p.profile.waitPositionFix)
            {
                rep = p.gps.SetRegister(2000, 0x90000000, 0x01);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK)
                        ? WorkerParam.ErrorType.SetPsti50Nack
                        : WorkerParam.ErrorType.SetPsti50Timeout;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Set PSTI 50 Interval successfully";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            rep = p.gps.ConfigMessageOutput(0x01);
            if (GPS_RESPONSE.NACK == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config message output NACK";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else if(GPS_RESPONSE.ACK == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config message output successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ConfigMessageOutputError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }

            rep = p.gps.ConfigNmeaOutput(1, 1, 1, 0, 1, 0, 0, 0);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                p.error = WorkerParam.ErrorType.ConfigMessageOutputError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config NMEA interval successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            //if (p.profile.testToRtkFloat || p.profile.testToRtkFix)
            if (p.profile.waitPositionFix && p.profile.testFixedType >= ModuleTestProfile.TestFixType.RtkFloat)
            {
                rep = p.gps.QueryAlphaLicense(DefaultCmdTimeout);
                if (rep == GPS_RESPONSE.ACK)
                {
                    rep = p.gps.SetRegister(DefaultCmdTimeout, 0xFE00007C, 3);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        //Thread.Sleep(3000);
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.ConfigRtkModeError;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Temporarily active license";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            bool positionFixed = false;
            do
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (!GpsMsgParser.CheckNmea(line))
                {
                    continue;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = line.Substring(0, line.Length - 2);
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                GpsMsgParser.ParsingResult ps = p.parser.ParsingNmea(line);
                if (GpsMsgParser.ParsingResult.UpdateSate == ps)
                {   //Update SNR Chart
                    p.parser.parsingStat.CopyTo(dvResult[p.index]);
                    r.reportType = WorkerReportParam.ReportType.UpdateSnrChart;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }

                if (!gpSnrPass && p.profile.testGpSnr && GpsMsgParser.ParsingResult.UpdateSate == ps)
                {
                    gpSnrPass = CompareResult(p, ref r, GpsMsgParser.SateType.Gps);
                }
                if (!glSnrPass && p.profile.testGlSnr && GpsMsgParser.ParsingResult.UpdateSate == ps)
                {
                    glSnrPass = CompareResult(p, ref r, GpsMsgParser.SateType.Glonass);
                }
                if (!bdSnrPass && p.profile.testBdSnr && GpsMsgParser.ParsingResult.UpdateSate == ps)
                {
                    bdSnrPass = CompareBeidouResult(p, ref r);
                }
                if (!giSnrPass && p.profile.testGiSnr && GpsMsgParser.ParsingResult.UpdateSate == ps)
                {
                    giSnrPass = CompareResult(p, ref r, GpsMsgParser.SateType.Navic);
                }

                if (GpsMsgParser.ParsingResult.UpdateFixMode == ps)
                {
                    if (p.profile.waitPositionFix)
                    {
                        if (p.profile.testFixedType == ModuleTestProfile.TestFixType.RtkFixRatio10)
                        {
                            positionFixed = (p.parser.parsingStat.IsRtkFix() && p.parser.parsingStat.rtkRatio >= 10);
                        }
                        else if (p.profile.testFixedType == ModuleTestProfile.TestFixType.RtkFix)
                        {
                            positionFixed = p.parser.parsingStat.IsRtkFix();
                        }
                        else if (p.profile.testFixedType == ModuleTestProfile.TestFixType.RtkFloat)
                        {
                            positionFixed = p.parser.parsingStat.IsRtkFloat();
                        }
                        else if (p.profile.testFixedType == ModuleTestProfile.TestFixType.PositionFix)
                        {
                            positionFixed = p.parser.parsingStat.IsFixed();
                        }
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Fix status : " + p.parser.parsingStat.GetFixStatusString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                if (p.profile.waitPositionFix &&
                    p.profile.testFixedType >= ModuleTestProfile.TestFixType.RtkFloat &&
                    GpsMsgParser.ParsingResult.UpdateRtkRatio == ps)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = string.Format("RTK Ratio : {0} ({1})", p.parser.parsingStat.rtkRatio.ToString(),  p.parser.parsingStat.GetFixStatusString());
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    if (p.profile.testFixedType == ModuleTestProfile.TestFixType.RtkFixRatio10)
                    {
                        positionFixed = (p.parser.parsingStat.IsRtkFix() && p.parser.parsingStat.rtkRatio >= 10);
                    }
                }

                if (!p.profile.waitPositionFix)
                {
                    positionFixed = true;
                }
            } while (!(gpSnrPass && glSnrPass && bdSnrPass && giSnrPass && positionFixed) && !p.bw.CancellationPending);

            if (gpSnrPass && glSnrPass && bdSnrPass && giSnrPass && positionFixed)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Test SNR successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.NoError;
                if (!gpSnrPass)
                {
                    p.error |= WorkerParam.ErrorType.GpsSnrError;
                }
                if (!glSnrPass)
                {
                    p.error |= WorkerParam.ErrorType.GlonassSnrError;
                }
                if (!bdSnrPass)
                {
                    p.error |= WorkerParam.ErrorType.BeidouSnrError;
                }
                if (!giSnrPass)
                {
                    p.error |= WorkerParam.ErrorType.NavicSnrError;
                }

                if (!positionFixed)
                {
                    p.error |= WorkerParam.ErrorType.PositionFixError;
                }
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            rep = p.gps.ConfigMessageOutput(0x00);
            if (GPS_RESPONSE.NACK == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config message output NACK";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else if (GPS_RESPONSE.ACK == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config message output successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ConfigMessageOutputError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            return true;
        }

        public bool DoTest(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;

            if (p.profile.testVoltage && !TestVoltage(p, r))
            {
                return false;
            }
            if (!OpenDevice(p, r, GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate)))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3))
            {
                return false;
            }
            Thread.Sleep(2500);

            //if (!SetTestHighBaudRate(p, r))
            //{
            //    return false;
            //}

            //20181224, test to RTK float and test to RTK fix also need upload ephemeris, request from Angus 
            //Configure RTK mode to normal rover
            if (p.profile.waitPositionFix && 
                p.profile.testFixedType >= ModuleTestProfile.TestFixType.RtkFloat)
            {
                if (!DoConfigureRtkMode(p, r))
                {
                    return false;
                }
            }
            if (p.profile.waitPositionFix)
            {
                int lon = 0, lat = 0, alt = 0;
                bool romType = !GetGoldenPosition(ref lon, ref lat, ref alt);
                bool ret = DoWarmStart(p, r, romType, lon, lat, alt, 2000);
                if (ret && (p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr))
                {
                    SetGpsEphemeris(p, r);
                }
            }

            if (!DoQueryVersion(p, r, false))
            {
                return false;
            }

            if (p.profile.checkPromCrc && !DoQueryCrc(p, r, false, true))
            {
                return false;
            }

            if (p.profile.checkRtc && !TestRtc(p, r))
            {
                return false;
            }

            if (p.profile.testAntenna && !TestAntenna(p, r))
            {
                return false;
            }

            if (p.profile.testUart2TxRx && !TestUart2TxRx(p, r))
            {
                return false;
            }

            if (p.profile.checkSlavePromCrc)
            {
                if (!DoQueryCrc(p, r, true, true))
                {
                    //Enter slave pass-through
                    if (!DrPassThrough(p, true, true, false, 1))
                    {
                        return false;
                    }

                    if (!DoQueryCrc(p, r, true, false))
                    {
                        return false;
                    }

                    //Leave slave pass-through
                    if (!DrPassThrough(p, true, false, false, 3))
                    {
                        return false;
                    }
                    Thread.Sleep(1000);
                }
            }

            //Launch Timer
            r.reportType = WorkerReportParam.ReportType.LaunchTimer;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr)
            {
                if (!TestSnr(p, r))
                    return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            if (p.profile.testClockOffset && !TestClockOffset(p, r))
            {
                return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            if (!SetFactoryReset(p, r))
            {
                return false;
            }

            if (p.profile.testIo && !TestIo(p, r))
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            EndProcess(p);
            return true;
        }

        private static SkytraqGps controllerIO = null;
        private static bool controllerInit = false;
        private static bool DoDrInitIo(WorkerParam p, WorkerReportParam r)
        {
            if (controllerInit)
            {
                Thread.Sleep(200);
                return true;
            }
            controllerInit = true;
            if (controllerIO == null)
            {
                controllerIO = new SkytraqGps();
            }

            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            for (int tryCount = 3; tryCount != 0; --tryCount)
            {
                if (processSimulation)
                {
                    Thread.Sleep(50 + rand.Next(0, 5));
                    rep = GPS_RESPONSE.UART_OK;
                    break;
                }

                rep = controllerIO.Open(p.annIoPort, 5);
                if (GPS_RESPONSE.UART_OK == rep)
                {
                    break;
                }

                if (GPS_RESPONSE.UART_FAIL == rep)
                {
                    controllerIO.Close();
                }
                Thread.Sleep(100);
            }

            if (processSimulation)
            {
                Thread.Sleep(500 + rand.Next(0, 50));
                return true;
            }

            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                controllerIO.Close();
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.IoControllerFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                controllerInit = false;
                return false;
            }
            //DR test use GPIO 1,2,3,4,5,6,10,12,13,16,20,28,30,31, 
            // 1111-0000-0101-0001-0011-0100-0111-1110 = F051347E h
            //GPIO 28, 30, 31 for input, 
            // 1101 -0000-0000-0000-0000-0000-0000-0000 = D0000000h
            // GPIO 28, 30, 31 - Localation sensor CW End, CCW End, Home
            // GPIO 10, 12, 13 - RSTN for Slot 1, 2, 3
            // GPIO 20, 16 - Motor control direction, Motor control clock
            // GPIO 1, 3, 5, 22 - ODO direction for Slot 1, 2, 3, 4
            // GPIO 2, 4, 6, 29 - ODO speed clock for Slot 1, 2, 3, 4
            rep = controllerIO.InitControllerIO(0xF051347E, 0xd0000000);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.McuFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                controllerInit = false;
                controllerIO.Close();
                return false;
            }

            if (p.profile.useSensor)
            {   //Find Home
                //function 2: find home cw first, 3: ccw first.
                rep = controllerIO.SetControllerSensor(3, 31, 30, 28, 20, 16, motorDelay, 950);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.DrSensorFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    controllerInit = false;
                    controllerIO.Close();
                    return false;
                }
            }

            //Reset for Slot 1, 2, 3 in GPIO 10, 12, 13, 0011-0100-0000-0000 = 3400h
            // 0000-0000-0100-0000-0011-0100-0000-0000 = 0040002A, use GPIO 10, 12, 13 for ODO
            // 0000-0000-0100-0000-0000-0000-0010-1010 = 0040002A, use GPIO 1, 3, 5, 22 for ODO
            rep = controllerIO.SetControllerIO(0x00003400, 0x0040002A);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.McuFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                controllerInit = false;
                controllerIO.Close();
                return false;
            }
            return true;
        }

        public static void ResetDrMcuStatus()
        {
            setDrMcuStep = false;
            controllerInit = false;
            if (controllerIO != null)
            {
                controllerIO.Close();
            }
        }

        private static bool setDrMcuStep = false;
        private static readonly uint motorDelay = 8000;
        private static readonly uint motorStep = 752;
        private static bool SetDrMcuStep(WorkerParam p, WorkerReportParam r, byte step)
        {
            if (setDrMcuStep)
            {
                Thread.Sleep(500);
                return true;
            }
            setDrMcuStep = true;
            try
            {
                if (controllerIO == null)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.IoControllerFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                GPS_RESPONSE rep;
                if (step == 0)
                {   //Turn Dir to 1, Rotate CCW 80 degree.
                    //MCU IO Set for DR Test all direction high
                    //6f 02 00 40 00 2A 00 00 00 00
                    rep = controllerIO.SetControllerIO(0x0040002A, 0x00000000);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.McuFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    //Start ODO clock, Turn Dir to 0
                    if (p.profile.testInsDrGyro)
                    {   // 0010-0000-0100-0000-0000-0000-0101-0100 = 20000054, use GPIO 2, 4, 6, 29 for ODO pulse
                        rep = controllerIO.SetControllerClock(1, 1000 / (50 * 2), 0x20000054);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                        // 0000-0000-0100-0000-0000-0000-0010-1010 = 0040002A, use GPIO 1, 3, 5, 22 for Direction
                        rep = controllerIO.SetControllerIO(0x00000000, 0x0040002A);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                    }
                    //6f 03 00 1f 1e 1c 14 10 00 00 1D B0 00 00 02 F0
                    if (p.profile.testDrCyro)
                    {
                        if (p.profile.useSensor)
                        {
                            rep = controllerIO.SetControllerSensor((p.profile.reverseRotation) ? (byte)0 : (byte)1, 31, 30, 28, 20, 16, motorDelay, motorStep);
                        }
                        else
                        {
                            rep = controllerIO.SetControllerMoto((p.profile.reverseRotation) ? (byte)0 : (byte)1, 31, 30, 28, 20, 16, motorDelay, motorStep);
                        }
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Roteat to 90.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        MyMessageBox.Show("Rotate to the mark.", "Warning");
                    }
                    motoPosition = 1;
                }
                else if (step == 1)
                {   //Turn Dir to 0, Start ODO clock,
                    //GPIO 2, 4, 6 for ODO Clock
                    if (p.profile.testDrCyro)
                    {
                        rep = controllerIO.SetControllerClock(1, 1000 / (50 * 2), 0x00000054);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }

                        //MCU IO Set for DR Test all direction low
                        //6f 02 00 00 00 00 00 00 00 2A
                        // 0000-0000-0100-0000-0000-0000-0010-1010 = 0040002A, use GPIO 1, 3, 5 for ODO
                        rep = controllerIO.SetControllerIO(0x00000000, 0x0040002A);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                    }

                    //6f 03 00 1f 1e 1c 14 10 00 00 1D B0 00 00 02 F0
                    //rep = controllerIO.SetControllerMoto(0, 31, 30, 28, 20, 16, motorDelay, motorStep);
                    //if (GPS_RESPONSE.ACK != rep)
                    //{
                    //    r.reportType = WorkerReportParam.ReportType.ShowError;
                    //    p.error = WorkerParam.ErrorType.McuFail;
                    //    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    //    return false;
                    //}
                    motoPosition = 2;

                }
                else if (step == 2)
                {   //Turn Dir to 0, stop ODO clock, Rotate CW 80 degree.
                    if (p.profile.testDrCyro)
                    {
                        rep = controllerIO.SetControllerClock(0, 0, 0x00000054);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                    }
                    //MCU IO Set for DR Test all direction high
                    //6f 02 00 40 00 2A 00 00 00 00
                    rep = controllerIO.SetControllerIO(0x0040002A, 0x00000000);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.McuFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }

                    if (p.profile.testDrCyro)
                    {
                        //6f 03 00 1f 1e 1c 14 10 00 00 1D B0 00 00 02 F0
                        if (p.profile.useSensor)
                        {
                            rep = controllerIO.SetControllerSensor((p.profile.reverseRotation) ? (byte)1 : (byte)0, 31, 30, 28, 20, 16, motorDelay, motorStep);
                        }
                        else
                        {
                            rep = controllerIO.SetControllerMoto((p.profile.reverseRotation) ? (byte)1 : (byte)0, 31, 30, 28, 20, 16, motorDelay, motorStep);
                        }
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.McuFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Please move the fixture to home point. Press the OK button when finished.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        MyMessageBox.Show("Rotate to the home.", "Warning");
                    }
                    motoPosition = 3;
                }
                else if (step == 3)
                {
                    //6f 03 00 1f 1e 1c 14 10 00 00 1D B0 00 00 02 F0
                    //rep = controllerIO.SetControllerMoto(1, 31, 30, 28, 20, 16, motorDelay, motorStep);
                    //if (GPS_RESPONSE.ACK != rep)
                    //{
                    //    r.reportType = WorkerReportParam.ReportType.ShowError;
                    //    p.error = WorkerParam.ErrorType.McuFail;
                    //    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    //    return false;
                    //}
                    motoPosition = 4;
                }
            }
            catch
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "MCU exception!";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            setDrMcuStep = false;
            return true;
        }

        private static bool SetDrMcuReset(WorkerParam p, WorkerReportParam r)
        {
            if (setDrMcuStep)
            {
                Thread.Sleep(500);
                return true;
            }
            setDrMcuStep = true;
            try
            {
                if (controllerIO == null)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.IoControllerFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                GPS_RESPONSE rep;
                if (processSimulation)
                {
                    Thread.Sleep(100 + rand.Next(0, 10));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = controllerIO.SetControllerIO(0x00000000, 0x00003400);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.McuFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    controllerInit = false;
                    controllerIO.Close();
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "RSTN low";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
                Thread.Sleep(1000);

                if (processSimulation)
                {
                    Thread.Sleep(100 + rand.Next(0, 10));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = controllerIO.SetControllerIO(0x00003400, 0x00000000);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.McuFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    controllerInit = false;
                    controllerIO.Close();
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "RSTN high";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }

            }
            catch
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "MCU exception!";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            setDrMcuStep = false;
            return true;
        }

        private bool NoFixture = false;
        public bool DoDrTest(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
#if !(DEBUG)
            NoFixture = false;
#endif
            if (p.profile.testVoltage && !TestVoltage(p, r))
            {
                return false;
            }

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            double initTime = 0;

            if (NoFixture)
            {
                controllerInit = true;
                Thread.Sleep(1000);
            }
            else if (!DoDrInitIo(p, r))
            {
                return false;
            }
            initTime = sw.ElapsedMilliseconds / 1000.0;
            sw.Stop();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "homing time consuming:" + initTime.ToString("F1");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (initTime >= 1.0)
            {
                p.profile.AddTestPeriodCounter((int)initTime);
            }
            CheckControllerEvent(p, 30);
            if (!controllerInit)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.McuFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            Thread.Sleep(1000);
            ResetWaitingCount();

            if (!OpenDevice(p, r, GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate)))
            {
                return false;
            }
            //#if !(DEBUG)
            if (!DoColdStart(p, r, 3))
            {
                return false;
            }

            //#endif
            if (!SetTestHighBaudRate(p, r))
            {
                return false;
            }

            //20181224, test to RTK float and test to RTK fix also need upload ephemeris, request from Angus 
            //Configure RTK mode to normal rover
            if (p.profile.waitPositionFix &&
                p.profile.testFixedType >= ModuleTestProfile.TestFixType.RtkFloat)
            {
                if (!DoConfigureRtkMode(p, r))
                {
                    return false;
                }
            }
            if (p.profile.waitPositionFix)
            {
                int lon = 0, lat = 0, alt = 0;
                bool romType = !GetGoldenPosition(ref lon, ref lat, ref alt);
                bool ret = DoWarmStart(p, r, romType, lon, lat, alt, 2000);
                if (ret && (p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr))
                {
                    SetGpsEphemeris(p, r);
                }
            }

            if (!DoQueryVersion(p, r, false))
            {
                return false;
            }

            if (p.profile.checkPromCrc && !DoQueryCrc(p, r, false, true))
            {
                return false;
            }

            if (p.profile.checkRtc && !TestRtc(p, r))
            {
                return false;
            }

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 0
            //Test Here
            UInt32 temp = 0, odo_plus = 0;
            float gyro = 0, staticGyro = 0;
            byte odo_bw = 0;
            double gyroCcw = 0, gyroCw = 0, gyro_avg;
            const int testTimes = 8;

            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuStep(p, r, 0))
            {
                return false;
            }
            Thread.Sleep(1500);

            for (int i = 0; i < testTimes; ++i)
            {
                if (NoFixture)
                {
                    rep = GPS_RESPONSE.ACK;
                    gyro = -4000;
                    odo_plus = 500;
                    odo_bw = 0;
                }
                else
                {
                    rep = p.gps.QueryDrStatus(1000, ref temp, ref gyro, ref odo_plus, ref odo_bw);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.OdoPluseFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "gyro:" + gyro.ToString("F2") + " odo_plus:" + odo_plus.ToString() + " odo_bw:" + odo_bw.ToString();
                gyroCcw += gyro;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(100);
            }

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 1
            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuStep(p, r, 1))
            {
                return false;
            }
            Thread.Sleep(2000);

            //Test Here
            if (NoFixture)
            {
                rep = GPS_RESPONSE.ACK;
                staticGyro = 0;
            }
            else
            {
                rep = p.gps.QueryDrStatus(1000, ref temp, ref staticGyro, ref odo_plus, ref odo_bw);
            }
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OdoPluseFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "static gyro:" + staticGyro.ToString("F2") + " odo_plus:" + odo_plus.ToString() + " odo_bw:" + odo_bw.ToString();
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (p.profile.reverseRotation)
            {
                gyro_avg = gyroCcw / testTimes - staticGyro;
            }
            else
            {
                gyro_avg = staticGyro - gyroCcw / testTimes;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "gyro ccw avg calibration:" + gyro_avg.ToString("F3");
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(1000);

            if (gyro_avg > p.profile.uslAnticlockWise || gyro_avg < p.profile.lslAnticlockWise)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.GyroFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (odo_plus > 600 || odo_plus < 400)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OdoPluseFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (odo_bw != 0)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OdoDirectionFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            Thread.Sleep(200);

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 2
            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuStep(p, r, 2))
            {
                return false;
            }
            Thread.Sleep(2000);
            for (int i = 0; i < testTimes; ++i)
            {
                if (NoFixture)
                {
                    rep = GPS_RESPONSE.ACK;
                    gyro = 4000;
                    odo_bw = 1;
                }
                else
                {
                    rep = p.gps.QueryDrStatus(1000, ref temp, ref gyro, ref odo_plus, ref odo_bw);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.OdoPluseFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "gyro:" + gyro.ToString("F2") + " odo_plus:" + odo_plus.ToString() + " odo_bw:" + odo_bw.ToString();
                gyroCw += gyro;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                Thread.Sleep(100);
            }

            if (p.profile.reverseRotation)
            {
                gyro_avg = staticGyro - gyroCw / testTimes;
            }
            else
            {
                gyro_avg = gyroCw / testTimes - staticGyro;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "gyro cw avg calibration:" + gyro_avg.ToString("F3");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (gyro_avg > p.profile.uslClockWise || gyro_avg < p.profile.lslClockWise)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.GyroFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (odo_bw != 1)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OdoDirectionFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (p.profile.testAntenna && !TestAntenna(p, r))
            {
                return false;
            }

            if (p.profile.testUart2TxRx && !TestUart2TxRx(p, r))
            {
                return false;
            }

            if ((p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr) && !TestSnr(p, r))
            {
                return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            if (p.profile.testClockOffset && !TestClockOffset(p, r))
            {
                return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            if (!SetFactoryReset(p, r))
            {
                return false;
            }

            if (p.profile.testIo && !TestIo(p, r))
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            EndProcess(p);
            return true;
        }

        public bool DoInsDrTest(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
#if !(DEBUG)
            NoFixture = false;
#endif
            if (p.profile.testVoltage && !TestVoltage(p, r))
            {
                return false;
            }

            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            double initTime = 0;

            if (NoFixture)
            {
                controllerInit = true;
                Thread.Sleep(1000);
            }
            else if (!DoDrInitIo(p, r))
            {
                return false;
            }
            initTime = sw.ElapsedMilliseconds / 1000.0;
            sw.Stop();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "homing time consuming:" + initTime.ToString("F1");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            //if (initTime >= 1.0)
            //{
            //    p.profile.AddTestPeriodCounter((int)initTime);
            //}

            CheckControllerEvent(p, 50);
            if (!controllerInit)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.McuFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            Thread.Sleep(1000);
            ResetWaitingCount();

            if (!OpenDevice(p, r, GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate)))
            {
                return false;
            }

            if (!DoColdStart(p, r, 3))
            {
                return false;
            }
            Thread.Sleep(2500);

            //20181224, test to RTK float and test to RTK fix also need upload ephemeris, request from Angus 
            //Configure RTK mode to normal rover
            if (p.profile.waitPositionFix &&
                p.profile.testFixedType >= ModuleTestProfile.TestFixType.RtkFloat)
            {
                if (!DoConfigureRtkMode(p, r))
                {
                    return false;
                }
            }
            if (p.profile.waitPositionFix)
            {
                int lon = 0, lat = 0, alt = 0;
                bool romType = !GetGoldenPosition(ref lon, ref lat, ref alt);
                bool ret = DoWarmStart(p, r, romType, lon, lat, alt, 2000);
                if (ret && (p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr))
                {
                    SetGpsEphemeris(p, r);
                }
            }

            if (!DoQueryVersion(p, r, false))
            {
                return false;
            }

            if (p.profile.checkPromCrc && !DoQueryCrc(p, r, false, true))
            {
                return false;
            }

            if (p.profile.checkRtc && !TestRtc(p, r))
            {
                return false;
            }

            //Test Here
            SkytraqGps.InsDrStatus insDrStatus = new SkytraqGps.InsDrStatus();
            if (!p.profile.skipSpdDir)
            {
                rep = p.gps.QueryInsDrStatus(1000, ref insDrStatus);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.QueryInsdrStatusError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "ODO Distance: " + insDrStatus.odoDistance.ToString() + ", ODO Direction: " + insDrStatus.odoFwBwSts.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                if (insDrStatus.odoFwBwSts != 0)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.OdoDirectionFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Test odo direction pass";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            //Enter slave pass-through
            if (!DrPassThrough(p, true, true, false, 1))
            {
                return false;
            }

            if (p.profile.checkSlavePromCrc && !DoQueryCrc(p, r, true, false))
            {
                return false;
            }

            rep = p.gps.InsdrAccumulateAngleStart(1000);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.InsdrAccumulateAngleFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Accumulate angle start";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 0
            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuStep(p, r, 0))
            {
                return false;
            }
            if (Global.drTestType == Global.DrTestType.UseMotor)
                Thread.Sleep(1500);

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 1
            if (NoFixture)
            {
                Thread.Sleep(1000);
            }
            else if (!SetDrMcuStep(p, r, 1))
            {
                return false;
            }
            if (Global.drTestType == Global.DrTestType.UseMotor)
                Thread.Sleep(4000);

            float angleX = 0, angleY = 0, angleZ = 0;
            rep = p.gps.InsdrAccumulateAngleStop(1000, ref angleX, ref angleY, ref angleZ);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.InsdrAccumulateAngleFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Accumulate angle stop";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            //Leave slave pass-through
            if (!DrPassThrough(p, true, false, false, 3))
            {
                return false;
            }
            Thread.Sleep(1000);

            insDrStatus = new SkytraqGps.InsDrStatus();
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Gyro rotation angle(X, Y, Z):" + angleX.ToString("F2") + ", " + angleY.ToString("F2") + ", :" + angleZ.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (angleZ > p.profile.insDrGyroUpper || angleZ < p.profile.insDrGyroLower)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.GyroFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Test gyro pass";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            //Enter slave pass-through
            if (!DrPassThrough(p, true, true, false, 1))
            {
                return false;
            }

            rep = p.gps.InsdrAccumulateAngleStart(1000);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.InsdrAccumulateAngleFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Accumulate angle start";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            Thread.Sleep(200);

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            //==================== DR Test Step 2
            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuStep(p, r, 2))
            {
                return false;
            }
            if (Global.drTestType == Global.DrTestType.UseMotor)
                Thread.Sleep(4000);

            CheckControllerEvent(p, 90);
            Thread.Sleep(1000);
            ResetWaitingCount();

            rep = p.gps.InsdrAccumulateAngleStop(1000, ref angleX, ref angleY, ref angleZ);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.InsdrAccumulateAngleFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Accumulate angle stop";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            //Leave slave pass-through
            if (!DrPassThrough(p, true, false, false, 3))
            {
                return false;
            }
            Thread.Sleep(1000);

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Gyro rotation angle(X, Y, Z):" + angleX.ToString("F2") + ", " + angleY.ToString("F2") + ", :" + angleZ.ToString("F2");
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (angleZ > (0 - p.profile.insDrGyroLower) || angleZ < (0 - p.profile.insDrGyroUpper))
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.GyroFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Test gyro pass";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (p.profile.testAntenna && !TestAntenna(p, r))
            {
                return false;
            }

            if (p.profile.testUart2TxRx && !TestUart2TxRx(p, r))
            {
                return false;
            }

            //Launch Timer
            r.reportType = WorkerReportParam.ReportType.LaunchTimer;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (p.profile.testGpSnr || p.profile.testGlSnr || p.profile.testBdSnr || p.profile.testGiSnr)
            {
                if(!TestSnr(p, r))
                    return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            //rep = p.gps.QueryInsDrStatus(1000, ref insDrStatus);
            //if (GPS_RESPONSE.ACK != rep)
            //{
            //    r.reportType = WorkerReportParam.ReportType.ShowError;
            //    p.error = WorkerParam.ErrorType.QueryInsdrStatusError;
            //    p.bw.ReportProgress(0, new WorkerReportParam(r));
            //    return false;
            //}

            if (p.profile.testClockOffset && !TestClockOffset(p, r))
            {
                return false;
            }

            if (!TestBarometerOdoAcc(p, r))
            {
                return false;
            }

            if (p.bw.CancellationPending)
            {
                return false;
            }

            if (!SetFactoryReset(p, r))
            {
                return false;
            }

            if (p.profile.testIo && !TestIo(p, r))
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            EndProcess(p);
            return true;
        }

        public bool OpenPortTest(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;

            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            while (!p.bw.CancellationPending)
            {
                w.Reset();
                w.Start();
                rep = p.gps.Open(p.comPort, 1);
                if (GPS_RESPONSE.UART_FAIL == rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.OpenPortFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    w.Stop();
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open spend " + w.ElapsedMilliseconds + " ms";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    p.gps.Close();
                }
                Thread.Sleep(500);
            }
            return true;
        }

        private byte CalcCheckSum16(byte[] data, int start, int len)
        {
            UInt16 checkSum = 0;
            for (int i = 0; i < len; i += sizeof(UInt16))
            {
                UInt16 word = Convert.ToUInt16(data[start + i + 1] | data[start + i] << 8);
                checkSum += word;
            }
            return Convert.ToByte(((checkSum >> 8) + (checkSum & 0xFF)) & 0xFF);
        }

        private int ScanBaudRate(WorkerParam p, WorkerReportParam r, int first, int delayTime)
        {
            GPS_RESPONSE rep;
            int TestDeviceTimeout = 500;
            int[] testingOrder = { 5, 1, 3, 4, 0, 2, 6, 7, 8 };
            int[] baudIdxTimeout = { 4000, 1000, 500, 300, 300, 300, 300, 300, 300 };

            if (first != -1)
            {
                rep = p.gps.Open(p.comPort, first);
                if (GPS_RESPONSE.UART_OK != rep)
                {   //This com port can't open.
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " failed!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return -1;
                }
                if (ModuleIniParser.scanDelay > 0)
                {
                    Thread.Sleep(ModuleIniParser.scanDelay);
                }
                //TestDeviceTimeout = (first < 2) ? 1500 : 1000;
                TestDeviceTimeout = baudIdxTimeout[first];
                if (processSimulation)
                {
                    Thread.Sleep(800 + rand.Next(0, 80));
                    rep = GPS_RESPONSE.NACK;
                }
                else
                {
                    //rep = p.gps.TestDevice(TestDeviceTimeout, 1);
                    rep = p.gps.TestDevice2(TestDeviceTimeout, 1);
                }
                if (GPS_RESPONSE.ACK == rep || GPS_RESPONSE.NACK == rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Found working baud rate " + GpsBaudRateConverter.Index2BaudRate(first).ToString() + ".";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return first;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Baud rate " + GpsBaudRateConverter.Index2BaudRate(first).ToString() + " invalid.";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    p.gps.Close();
                }
            }

            foreach (int i in testingOrder)
            {
                if (i == first)
                {
                    continue;
                }
                if (p.bw.CancellationPending)
                {
                    return -1;
                }

                rep = p.gps.Open(p.comPort, i);
                if (GPS_RESPONSE.UART_OK != rep)
                {   //This com port can't open.
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " failed!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return -1;
                }
                if (ModuleIniParser.scanDelay > 0)
                {
                    Thread.Sleep(ModuleIniParser.scanDelay);
                }

                TestDeviceTimeout = baudIdxTimeout[i];
                if (processSimulation)
                {
                    Thread.Sleep(800 + rand.Next(0, 80));
                    rep = GPS_RESPONSE.NACK;
                }
                else
                {
                    //rep = p.gps.TestDevice(TestDeviceTimeout, 1);
                    rep = p.gps.TestDevice2(TestDeviceTimeout, 1);
                }

                if (GPS_RESPONSE.ACK == rep || GPS_RESPONSE.NACK == rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Found working baud rate " + GpsBaudRateConverter.Index2BaudRate(i).ToString() + ".";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return i;

                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Baud rate " + GpsBaudRateConverter.Index2BaudRate(i).ToString() + " invalid!(" + TestDeviceTimeout.ToString() + ")";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    p.gps.Close();
                }
            }
            return -1;
        }

#if !(DEBUG)
        private static bool processSimulation = false;    //Don't modify this!
#else
        private static bool processSimulation = false;
#endif

        private static Random rand = new Random();
        private static int lastDeviceBaudIdx = -1;
        private int lastRomBaudIdx = 1;
        public bool DoDownload(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (p.profile.enableSlaveDownload)
            {
                try
                {
                    if (NoFixture)
                    {
                        controllerInit = true;
                        Thread.Sleep(1000);
                    }
                    else if (!DoDrInitIo(p, r))
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.McuFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                CheckControllerEvent(p, 90);
                if (!controllerInit)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.McuFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                ResetWaitingCount();
            }

            bool downloadResult = DoDownloadX(p, false, false);
            if (processSimulation)
            {
                Thread.Sleep(10);
            }
            //else
            //{
            //    EndProcess(p);
            //}
            if (!downloadResult)
            {
                return false;
            }

            if (!p.profile.enableSlaveDownload)
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Total time : " + (sw.ElapsedMilliseconds / 1000).ToString() + " seconds";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                r.reportType = WorkerReportParam.ReportType.ShowFinished;
                p.error = WorkerParam.ErrorType.NoError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return true;
            }

            CheckControllerEvent(p, 90);
            ResetWaitingCount();

            if (NoFixture)
            {
                Thread.Sleep(100);
            }
            else if (!SetDrMcuReset(p, r))
            {
                return false;
            }
            Thread.Sleep(1500);

            CheckControllerEvent(p, 90);
            //Thread.Sleep(1000);
            ResetWaitingCount();

            //Slave download is always at 115200 bps.
            //int baudDlBk = p.profile.dlBaudSel;
            //int baudDvBk = lastDeviceBaudIdx;
            //p.profile.dlBaudSel = 5;
            //lastDeviceBaudIdx = GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate);
            downloadResult = DoDownloadX(p, true, true);
            if (!downloadResult)
            {
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Total time : " + (sw.ElapsedMilliseconds / 1000).ToString() + " seconds";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private bool WaitDeviceActive(WorkerParam p, int timeout, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                GPS_RESPONSE rep = GPS_RESPONSE.NONE;
                if (processSimulation)
                {
                    Thread.Sleep(100 + rand.Next(0, 10));
                    rep = GPS_RESPONSE.NACK;
                }
                else
                {
                    //rep = p.gps.TestDevice(timeout, 1);
                    rep = p.gps.TestDevice2(timeout, 1);
                }

                if (GPS_RESPONSE.ACK == rep)
                {
                    return true;
                }
            }
            return false;   
        }

        private bool DrPassThrough(WorkerParam p, bool showErrorMessage, bool isEnter, bool isRom, int tryTimes)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            bool ret = false;
            for (int i = 0; i < tryTimes; ++i)
            {
                GPS_RESPONSE rep = GPS_RESPONSE.NONE;
                if (processSimulation)
                {
                    Thread.Sleep(100 + rand.Next(0, 10));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = p.gps.EnterDrSlavePassThrough(isEnter, isRom);
                }
                if (!isEnter && GPS_RESPONSE.ACK != rep)
                {
                    continue;
                }

                if (GPS_RESPONSE.ACK != rep)
                {
                    p.error = WorkerParam.ErrorType.EnterPassThroughError;
                    if (showErrorMessage)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    if (isEnter)
                    {
                        r.output = "Enter the slave pass-through mode successfully";
                    }
                    else
                    {
                        r.output = "Leave the slave pass-through mode successfully";
                    }
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    ret = true;
                    Thread.Sleep((isEnter) ? 1000 : 2500);
                    break;
                }
            }
            return ret;
        }

        private bool DoDownloadX(WorkerParam p, bool downloadFw2, bool showPassThroughError)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep = GPS_RESPONSE.UART_OK;

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Scanning " + p.comPort + " baud rate...";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            // Retry three times for disable auto uart firmware, it'll 
            // change uart output baud rate after 5 seconds.
            int baudIdx = -1;
            int lastBaudIdx = (downloadFw2) ? GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate) : lastDeviceBaudIdx;
            for (int i = 0; i < ScanBaudCount; ++i)
            {
                baudIdx = ScanBaudRate(p, r, lastBaudIdx, i * 200);
                if (-1 != baudIdx)
                {
                    if (!downloadFw2)
                    {
                        lastDeviceBaudIdx = baudIdx;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Open " + p.comPort + " in " +
                        GpsBaudRateConverter.Index2BaudRate(baudIdx).ToString() +
                        " successfully";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    break;
                }
                Thread.Sleep(50);
            }

            if (-1 == baudIdx)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            if (!p.profile.enableSlaveDownload)
            {   //Pass through download can't boot to ROM code.
                String kVer = "";
                String sVer = "";
                String rev = "";
                if (processSimulation)
                {
                    rev = "20150521";
                    Thread.Sleep(100 + rand.Next(0, 20));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = p.gps.QueryVersion(DefaultCmdTimeout, 1, ref kVer, ref sVer, ref rev);
                }

                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.QueryVersionError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Query Version :" + rev.ToString();
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                if (rev != "20130221" && rev != "20150521")
                {
                    //Reboot to ROM Code
                    if (processSimulation)
                    {
                        Thread.Sleep(500 + rand.Next(0, 100));
                        rep = GPS_RESPONSE.ACK;
                    }
                    else
                    {
                        rep = p.gps.SetRegister(2000, 0x2000F050, 0x00000000);
                    }
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.ColdStartError;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Reboot from ROM successfully";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        p.gps.Close();
                        Thread.Sleep(3000);  //Waiting for reboot
                    }

                    // Retry three times for disable auto uart firmware, it'll 
                    // change uart output baud rate after 5 seconds.
                    baudIdx = -1;
                    for (int i = 0; i < ScanBaudCount; ++i)
                    {
                        baudIdx = ScanBaudRate(p, r, lastRomBaudIdx, i * 200);
                        if (-1 != baudIdx)
                        {
                            lastRomBaudIdx = baudIdx;
                            r.reportType = WorkerReportParam.ReportType.ShowProgress;
                            r.output = "Open " + p.comPort + " in " +
                                GpsBaudRateConverter.Index2BaudRate(baudIdx).ToString() +
                                    " successfully";
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            break;
                        }
                        Thread.Sleep(50);
                    }

                    if (-1 == baudIdx)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.OpenPortFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                }
            }

            int downloadBaud = (downloadFw2) ? 5 : p.profile.dlBaudSel;
            if (p.gps.GetBaudRate() != GpsBaudRateConverter.Index2BaudRate(downloadBaud))
            {
                if (processSimulation)
                {
                    Thread.Sleep(1200 + +rand.Next(0, 100));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = p.gps.ChangeBaudrate((byte)downloadBaud, 2, false);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.ChangeBaudRateFail;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Change baud rate to " + GpsBaudRateConverter.Index2BaudRate(downloadBaud) + " successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (downloadFw2 && !DrPassThrough(p, showPassThroughError, true, true, 1))
            {   //Enter slave pass-through
                return false;
            }
            WaitDeviceActive(p, 600, 5);

            String dbgOutput = "";
            if (processSimulation)
            {
                Thread.Sleep(1000 + rand.Next(0, 300));
                rep = GPS_RESPONSE.OK;
            }
            else
            {
                for (int retry = 5; retry != 0; --retry)
                {
                    rep = p.gps.SendLoaderDownload(ref dbgOutput, p.profile.dlBaudSel, downloadFw2);
                    if (GPS_RESPONSE.OK == rep)
                    {
                        break;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "SendLoaderDownload retry...";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    Thread.Sleep(100);
                }
            }
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.LoaderDownloadFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Loader download successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (processSimulation)
            {
                Thread.Sleep(1000 + rand.Next(0, 500));
                rep = GPS_RESPONSE.OK;
            }
            else
            {
                rep = p.gps.UploadLoader(LoaderData.v8TagLoader);
            }
            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.UploadLoaderFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Upload loader successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(1000);

            if ((!downloadFw2) &&
                (p.profile.fwProfile.tagAddress == 0 && p.profile.fwProfile.tagContent == 0) ||
                (p.profile.fwProfile.tagAddress == 0xAAAAAAAA && p.profile.fwProfile.tagContent == 0x55555555))
            {   //Clear old tag
                if (processSimulation)
                {
                    Thread.Sleep(1000 + rand.Next(0, 500));
                    rep = GPS_RESPONSE.OK;
                }
                else
                {
                    rep = p.gps.SendTagBinSize(p.profile.fwProfile.promRaw.Length, p.profile.fwProfile.CalcPromRawCheckSum(),
                        p.profile.dlBaudSel, 0xAAA56556, 0x55555555);
                }
            }
            else
            {
                if (processSimulation)
                {
                    Thread.Sleep(1000 + rand.Next(0, 500));
                    rep = GPS_RESPONSE.OK;
                }
                else
                {
                    rep = p.gps.SendTagBinSize(
                        (downloadFw2) ? p.profile.slaveFwProfile.promRaw.Length : p.profile.fwProfile.promRaw.Length,
                        (downloadFw2) ? p.profile.slaveFwProfile.CalcPromRawCheckSum() : p.profile.fwProfile.CalcPromRawCheckSum(),
                        p.profile.dlBaudSel,
                        (downloadFw2) ? p.profile.slaveFwProfile.tagAddress : p.profile.fwProfile.tagAddress,
                        (downloadFw2) ? p.profile.slaveFwProfile.tagContent : p.profile.fwProfile.tagContent);
                }
            }

            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.BinsizeCmdTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Start update firmware";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            const int nFlashBytes = 8 * 1024;
            const int headerSize = 3;

            byte[] header = new byte[headerSize];
            int lSize = (downloadFw2) ? p.profile.slaveFwProfile.promRaw.Length : p.profile.fwProfile.promRaw.Length;
            int sentBytes = 0;
            int totalByte = 0;
            UInt16 sequence = 0;
            int rawItr = 0;

            int failCount = 0;
            while (lSize > 0)
            {
                sentBytes = (lSize >= nFlashBytes) ? nFlashBytes : lSize;
                totalByte += sentBytes;

                header[0] = (byte)(sequence >> 24 & 0xFF);
                header[1] = (byte)(sequence & 0xff);
                header[2] = CalcCheckSum16((downloadFw2) ? p.profile.slaveFwProfile.promRaw : p.profile.fwProfile.promRaw, rawItr, sentBytes);

                //p.gps.SendDataNoWait(header, headerSize);
                if (processSimulation)
                {
                    Thread.Sleep(rand.Next(0, 5));
                    rep = GPS_RESPONSE.OK;
                }
                else
                {
                    rep = p.gps.SendDataWaitStringAck((downloadFw2) ? p.profile.slaveFwProfile.promRaw : p.profile.fwProfile.promRaw, rawItr, sentBytes, 10000, "OK\0");
                }
                if (rep == GPS_RESPONSE.OK)
                {
                    sequence++;
                    lSize -= sentBytes;
                    rawItr += nFlashBytes;
                }
                else
                {
                    if (++failCount > 0)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.DownloadWriteFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        EndProcess(p);
                        return false;
                    }
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Write block respone " + rep.ToString() + ", retry " + failCount.ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    continue;
                }
                failCount = 0;
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "left " + lSize.ToString() + " bytes";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }
            if (processSimulation)
            {
                Thread.Sleep(1000 + rand.Next(0, 5000));
                rep = GPS_RESPONSE.OK;
            }
            else
            {
                rep = p.gps.WaitStringAck(10000, "END\0");
            }

            if (GPS_RESPONSE.OK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.DownloadEndTimeOut;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }

            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            if (downloadFw2)
            {
                r.output = p.profile.slaveFwProfile.crc.ToString("X4") + " download OK";
            }
            else
            {
                r.output = p.profile.fwProfile.crc.ToString("X4") + " download OK";
            }
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            if (downloadFw2)
            {   //Leave slave pass-through
                if (processSimulation)
                {
                    Thread.Sleep(200 + rand.Next(0, 20));
                    rep = GPS_RESPONSE.ACK;
                }
                else
                {
                    rep = p.gps.EnterDrSlavePassThrough(false, true);
                }
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.EnterPassThroughError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Leave the slave pass-through mode successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (downloadFw2 == false)
            {
                return true;
            }
            r.reportType = WorkerReportParam.ReportType.ShowFinished;
            p.error = WorkerParam.ErrorType.NoError;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return true;
        }

        private static UInt32[] goldenPrnTable = new UInt32[GpsMsgParser.ParsingStatus.MaxChannels];
        private static UInt32[] goldenFreqTable = new UInt32[GpsMsgParser.ParsingStatus.MaxChannels];
        bool GetPrnDopplerFreq(UInt32 prn, ref UInt32 freq)
        {
            for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxChannels; ++i)
            {
                if (goldenPrnTable[i] == prn)
                {
                    freq = goldenFreqTable[i];
                    return true;
                }
            }
            return false;
        }

        private static String goldenLonString;
        private static String goldenLatString;
        private static String goldenAltString;
        private static char goldenNS;
        private static char goldenEW;
        private static float goldenPressure = 0;
        private static float goldenTemperature = 0;
        public bool DoGolden(WorkerParam p)
        {
            WorkerReportParam r = new WorkerReportParam();
            r.index = p.index;
            GPS_RESPONSE rep;
            rep = p.gps.Open(p.comPort, p.profile.gdBaudSel);
            if (GPS_RESPONSE.UART_FAIL == rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.OpenPortFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Open " + p.comPort + " successfully -------------";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            //if (p.profile.testGlSnr)
            if (p.profile.testGlSnr && !p.profile.waitPositionFix)
            {
                rep = p.gps.SetRegister(1000, 0x90000000, 0x01);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK)
                        ? WorkerParam.ErrorType.SetPsti50Nack
                        : WorkerParam.ErrorType.SetPsti50Timeout;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Set PSTI 50 interval successfully";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }
            else
            {
                rep = p.gps.SetRegister(2000, 0x90000000, 0x00);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = (rep == GPS_RESPONSE.NACK)
                        ? WorkerParam.ErrorType.SetPsti50Nack
                        : WorkerParam.ErrorType.SetPsti50Timeout;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Set PSTI 50 Interval successfully";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }
            }

            if (p.profile.testClockOffset)
            {

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Waiting for position fix to get clock offset...";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                r.reportType = WorkerReportParam.ReportType.ShowWaitingGoldenSample;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                do
                {
                    byte[] buff = new byte[256];
                    int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                    string line = Encoding.UTF8.GetString(buff, 0, l);
                    if (GpsMsgParser.CheckNmea(line))
                    {
                        if (GpsMsgParser.ParsingResult.UpdateSate == p.parser.ParsingNmea(line))
                        {
                            p.parser.parsingStat.CopyTo(dvResult[0]);
                            r.reportType = WorkerReportParam.ReportType.UpdateSnrChart;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                        }
                    }
                } while (!p.parser.parsingStat.IsFixed() && !p.bw.CancellationPending);
                r.reportType = WorkerReportParam.ReportType.HideWaitingGoldenSample;
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                if (p.bw.CancellationPending)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.TestNotComplete;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }

                rep = p.gps.GetRegister(DefaultCmdTimeout, 0x00000001, ref gdClockOffset);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;

                    p.error = (rep == GPS_RESPONSE.NACK)
                        ? WorkerParam.ErrorType.GetClockOffsetNack
                        : WorkerParam.ErrorType.GetClockOffsetTimeout;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get clock offset : " + unchecked((int)gdClockOffset).ToString();
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                }

                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Updating the Doppler frequency table...";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                //if (!p.profile.testGpSnr && p.profile.testGiSnr)
                //{
                //    //Navic change start channel from 0, request from Terrance 20181002
                //    //for (int i = GpsMsgParser.ParsingStatus.NavicChannelStart; i < GpsMsgParser.ParsingStatus.MaxNavicChannels; ++i)
                //    for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxChannels; i++)
                //    {
                //        GpsMsgParser.ParsingStatus.sateInfo s = TestModule.dvResult[r.index].GetNavicSate(i);
                //        UInt32 prn = 0, freq = 0;
                //        rep = p.gps.QueryChannelDoppler((byte)i, ref prn, ref freq);
                //        goldenPrnTable[i - GpsMsgParser.ParsingStatus.NavicChannelStart] = prn;
                //        goldenFreqTable[i - GpsMsgParser.ParsingStatus.NavicChannelStart] = freq;
                //        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                //        r.output = "CH=" + i + ", PRN=" + prn + ", FRQ=" + ((Int16)freq).ToString("F0");
                //        p.bw.ReportProgress(0, new WorkerReportParam(r));
                //    }
                //}
                //else
                //{
                    for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxChannels; i++)
                    {
                        GpsMsgParser.ParsingStatus.sateInfo s = TestModule.dvResult[r.index].GetGpsSate(i);
                        UInt32 prn = 0, freq = 0;
                        rep = p.gps.QueryChannelDoppler((byte)i, ref prn, ref freq);
                        goldenPrnTable[i] = prn;
                        goldenFreqTable[i] = freq;
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "CH=" + i + ", PRN=" + prn + ", FRQ=" + ((Int16)freq).ToString("F0");
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }   //for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                //}
            }

            //20181224, test to RTK float and test to RTK fix also need upload ephemeris, request from Angus 
            //if (p.profile.waitPositionFix)
            if (p.profile.waitPositionFix)
            {   //Get Golden Ephemeris
                goldenLonString = p.parser.parsingStat.lonString;
                goldenLatString = p.parser.parsingStat.latString;
                goldenAltString = p.parser.parsingStat.altitudeString;
                goldenNS = p.parser.parsingStat.ns;
                goldenEW = p.parser.parsingStat.ew;
                UpdateGoldenEphemeris(p, r);
            }

            rep = p.gps.ConfigMessageOutput(0x01);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ConfigMessageOutputError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config message output successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            rep = p.gps.ConfigNmeaOutput(1, 1, 1, 0, 1, 1, 0, 0);
            if (GPS_RESPONSE.ACK != p.gps.ConfigNmeaOutput(1, 1, 1, 0, 1, 1, 0, 0))
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ConfigMessageOutputError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            else
            {
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Config NMEA interval successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
            }

            if (p.profile.testBaro)
            {
                SkytraqGps.InsDrStatus insDrStatus = new SkytraqGps.InsDrStatus();
                rep = p.gps.QueryInsDrStatus(1000, ref insDrStatus);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.QueryInsdrStatusError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    EndProcess(p);
                    return false;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Golden Pressure: " + insDrStatus.baroPresure.ToString("F1") + ", Temperature: " + insDrStatus.sensor1Temp.ToString("F2");
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    goldenPressure = insDrStatus.baroPresure;
                    goldenTemperature = insDrStatus.sensor1Temp;
                }
            }

            r.reportType = WorkerReportParam.ReportType.GoldenSampleReady;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            do
            {
                byte[] buff = new byte[256];
                int l = p.gps.ReadLineNoWait(buff, 256, 2000);
                string line = Encoding.UTF8.GetString(buff, 0, l);
                if (!GpsMsgParser.CheckNmea(line))
                {
                    continue;
                }

                if (GpsMsgParser.ParsingResult.UpdateSate != p.parser.ParsingNmea(line))
                {
                    continue;
                }
                p.parser.parsingStat.CopyTo(dvResult[0]);
                r.reportType = WorkerReportParam.ReportType.UpdateSnrChart;
                p.bw.ReportProgress(0, new WorkerReportParam(r));

            } while (p.bw.CancellationPending != true);

            DoHotStart(p, r, 1500);
            EndProcess(p);
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Closed UART";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            return false;
        }

        public void EndProcess(WorkerParam p)
        {
            p.gps.Close();
        }

        private void EndAntennaProcess(SkytraqGps p)
        {
            p.Close();
            antennaEvent.Set();
        }

        //private GPS_RESPONSE GetNavicClockOffset(WorkerParam p, UInt32 gdClockOffset, UInt32 prn, UInt32 freq, ref Int32 clkData)
        //{
        //    GPS_RESPONSE rep = GPS_RESPONSE.TIMEOUT;
        //    for (int i = GpsMsgParser.ParsingStatus.NavicChannelStart; i < GpsMsgParser.ParsingStatus.MaxNavicChannels; ++i)
        //    {
        //        //GpsMsgParser.ParsingStatus.sateInfo s = TestModule.dvResult[r.index].GetNavicSate(i);
        //        UInt32 myPrn = 0, myFreq = 0;
        //        rep = p.gps.QueryChannelDoppler((byte)i, ref myPrn, ref myFreq);
        //        if(rep != GPS_RESPONSE.ACK)
        //        {
        //            return rep;
        //        }
        //        if(prn != myPrn)
        //        {
        //            continue;
        //        }

        //        Int16 sGdOffset = (Int16)((UInt16)(gdClockOffset));
        //        Int16 sMyFreq = (Int16)((UInt16)(myFreq));
        //        Int16 sFreq = (Int16)((UInt16)(freq));

        //        clkData = sGdOffset - sMyFreq - sFreq;
        //        return GPS_RESPONSE.ACK;
        //    }
        //    return GPS_RESPONSE.TIMEOUT;
        //}

        private bool TestClockOffset(WorkerParam p, WorkerReportParam r)
        {
            int tryCount = 3;

            while (--tryCount >= 0)
            {
                int sumOfClockOffset = 0;
                int prnCount = 0;
                GPS_RESPONSE rep = GPS_RESPONSE.NONE;
                if (p.profile.waitPositionFix)
                {
                    UInt32 clk = 0;
                    rep = p.gps.GetRegister(DefaultCmdTimeout, 0x00000001, ref clk);
                    if (GPS_RESPONSE.ACK != rep)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.GetClockOffsetFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    sumOfClockOffset = (Int16)clk;
                    prnCount = 1;
                }
                else
                {
                    bool gotDoppler = false;
                    int getCount = 5;
                    while (--getCount >= 0)
                    {
                        int selectedPrn = 0, selectedSnr = 0, selectedChn = 0;
                        //Find the maximum SNR channel
                        if (!p.profile.testGpSnr && p.profile.testGiSnr)
                        {   //NAVIC only
                            for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                            {
                                GpsMsgParser.ParsingStatus.sateInfo s = TestModule.dvResult[r.index].GetNavicSate(i);
                                if (s.snr > selectedSnr)
                                {
                                    selectedPrn = s.prn;
                                    selectedSnr = s.snr;
                                    selectedChn = i;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < GpsMsgParser.ParsingStatus.MaxSattellite; i++)
                            {
                                GpsMsgParser.ParsingStatus.sateInfo s = TestModule.dvResult[r.index].GetGpsSate(i);
                                if (s.snr > selectedSnr)
                                {
                                    selectedPrn = s.prn;
                                    selectedSnr = s.snr;
                                    selectedChn = i;
                                }
                            }
                        }

                        UInt32 prn = (UInt32)selectedPrn, freq = 0;
                        Int32 clkData = 0;
                        if (!GetPrnDopplerFreq(prn, ref freq))
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowProgress;
                            r.output = "GetPrnDopplerFreq failed!" + rep.ToString() + ", " +
                                prn.ToString();
                            p.bw.ReportProgress(0, new WorkerReportParam(r));

                            Thread.Sleep(500);
                            continue;
                        }

                        rep = p.gps.QueryChannelClockOffset(gdClockOffset, prn, freq, ref clkData);
                        if (GPS_RESPONSE.ACK != rep)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowProgress;
                            r.output = "QueryChannelClockOffset failed!" + rep.ToString() + ", " +
                                prn.ToString() + ", " + freq.ToString();
                            p.bw.ReportProgress(0, new WorkerReportParam(r));

                            Thread.Sleep(500);
                            continue;
                        }
                        else
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowProgress;
                            r.output = "PRN=" + prn + ", GDCLK=" + ((Int16)gdClockOffset) + ", FRQ=" + ((Int16)freq) + ", CLK=" + ((Int16)clkData);
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                        }
                        sumOfClockOffset += unchecked((Int16)clkData);
                        prnCount++;

                        if (prnCount < 1)
                        {
                            sumOfClockOffset = 0;
                            prnCount = 0;
                        }
                        else
                        {
                            gotDoppler = true;
                            break;
                        }
                    }

                    if (!gotDoppler)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.GetClockOffsetFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                }

                double avgClockOffset = sumOfClockOffset / prnCount;
                double clkPpm = avgClockOffset / (96.25 * 16.367667);
                if (avgClockOffset.ToString("F0") == "-1")
                {
                    Console.WriteLine("avgClockOffset:" + avgClockOffset.ToString());
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Device clock offset " + avgClockOffset.ToString("F0") + "(" + clkPpm.ToString("F2") + " ppm)";
                p.bw.ReportProgress(0, new WorkerReportParam(r));

                if (clkPpm < 0)
                {
                    clkPpm = -clkPpm;
                }
                if (clkPpm <= p.profile.clockOffsetThreshold)
                {
                    return true;
                }
                else
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Test Clock Offset failed!";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    Thread.Sleep(300);
                }
            }
            r.reportType = WorkerReportParam.ReportType.ShowError;
            p.error = WorkerParam.ErrorType.CheckClockOffsetFail;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return false;
        }

        private bool TestBarometerOdoAcc(WorkerParam p, WorkerReportParam r)
        {
            if(p.profile.testBaro == false && p.profile.skipSpdDir == true)
            {
                return true;
            }

            int tryCount = 4;
            GPS_RESPONSE rep = GPS_RESPONSE.NONE;
            SkytraqGps.InsDrStatus insDrStatus = new SkytraqGps.InsDrStatus();
            do
            {
                rep = p.gps.QueryInsDrStatus(2000, ref insDrStatus);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowError;
                    p.error = WorkerParam.ErrorType.QueryInsdrStatusError;
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    return false;
                }

                if (p.profile.testAcc)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Accelerometer X: " + insDrStatus.averageAccX.ToString("F2") + ", Y: " + 
                        insDrStatus.averageAccY.ToString("F2") + ", Z: " + insDrStatus.averageAccZ.ToString("F2");
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    if (insDrStatus.averageAccX < p.profile.accXLower ||
                        insDrStatus.averageAccX > p.profile.accXUpper ||
                        insDrStatus.averageAccY < p.profile.accYLower ||
                        insDrStatus.averageAccY > p.profile.accYUpper ||
                        insDrStatus.averageAccZ < p.profile.accZLower ||
                        insDrStatus.averageAccZ > p.profile.accZUpper)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.TestAccelerometerFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Test accelerometer pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }

                if (p.profile.testBaro)
                {
                    if (insDrStatus.baroPresure == 0 || insDrStatus.sensor1Temp == 0)
                    {
                        Thread.Sleep(500);
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Waiting barometer sensor ready...";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));

                        if (tryCount == 0)
                        {
                            r.reportType = WorkerReportParam.ReportType.ShowError;
                            p.error = WorkerParam.ErrorType.TestPressureFail;
                            p.bw.ReportProgress(0, new WorkerReportParam(r));
                            return false;
                        }
                        continue;
                    }

                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Pressure: " + insDrStatus.baroPresure.ToString("F1") + ", Temperature: " + insDrStatus.sensor1Temp.ToString("F2");
                    p.bw.ReportProgress(0, new WorkerReportParam(r));

                    if (insDrStatus.baroPresure < (goldenPressure - p.profile.pressureCriteria) ||
                        insDrStatus.baroPresure > (goldenPressure + p.profile.pressureCriteria))
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.TestPressureFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Test barometer pressure pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }

                    if (insDrStatus.sensor1Temp < (goldenTemperature - p.profile.tempCriteria) ||
                        insDrStatus.sensor1Temp > (goldenTemperature + p.profile.tempCriteria))
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.TestTemperatureFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Test barometer temperature pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }

                if (!p.profile.skipSpdDir)
                {
                    if (!p.profile.skipSpdDir)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "ODO Pulse Distance: " + insDrStatus.odoDistance.ToString() + ", ODO Direction: " + insDrStatus.odoFwBwSts.ToString();
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }

                    if (insDrStatus.odoFwBwSts != 1)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.OdoDirectionFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Test odo direction pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }

                    if (insDrStatus.odoDistance < 48 || insDrStatus.odoDistance > 52)
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowError;
                        p.error = WorkerParam.ErrorType.OdoPluseFail;
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                        return false;
                    }
                    else
                    {
                        r.reportType = WorkerReportParam.ReportType.ShowProgress;
                        r.output = "Test odo pulse pass";
                        p.bw.ReportProgress(0, new WorkerReportParam(r));
                    }
                }
                break;    
            } while (--tryCount > 0);
            return true;
        }

        private bool SetFactoryReset(WorkerParam p, WorkerReportParam r)
        {
            GPS_RESPONSE rep = p.gps.FactoryReset();
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.FactoryResetError;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                EndProcess(p);
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Factory reset successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            Thread.Sleep(1200);
            return true;
        }

        private bool SetGpsEphemeris(WorkerParam p, WorkerReportParam r)
        {
            for (int retry = 3; retry != 0; --retry)
            {
                GPS_RESPONSE rep = p.gps.SetGpsEphemeris(goldenGpsEphemeris);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Set golden GPS ephemeris faild";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    continue;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Set golden GPS ephemeris successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                goldenGpsEphemerisTime = DateTime.Now;
                return true;
            }
            return false;
        }

        private bool TestIo(WorkerParam p, WorkerReportParam r)
        {
            if (!OpenDevice(p, r, GpsBaudRateConverter.BaudRate2Index(p.profile.fwProfile.dvBaudRate)))
            {
                return false;
            }

            GPS_RESPONSE rep = p.gps.ChangeBaudrate((byte)5, 2, false);
            if (GPS_RESPONSE.ACK != rep)
            {
                r.reportType = WorkerReportParam.ReportType.ShowError;
                p.error = WorkerParam.ErrorType.ChangeBaudRateFail;
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                return false;
            }
            r.reportType = WorkerReportParam.ReportType.ShowProgress;
            r.output = "Change baud rate successfully";
            p.bw.ReportProgress(0, new WorkerReportParam(r));

            switch (p.profile.testIoType)
            {
                case ModuleTestProfile.IoType.NavSpark:
                    if (!TestNavSparkIo(p, r))
                    {
                        return false;
                    }
                    break;
                case ModuleTestProfile.IoType.NavSparkMini:
                    if (!TestNavSparkMiniIo(p, r))
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        private static byte[][] goldenGpsEphemeris = new byte[SkytraqGps.GPSCount][];
        private static DateTime goldenGpsEphemerisTime = new DateTime(2000, 1, 1);
        private bool UpdateGoldenEphemeris(WorkerParam p, WorkerReportParam r)
        {
            TimeSpan diff = DateTime.Now - goldenGpsEphemerisTime;
            if (diff.Minutes < 10 && GetGoldenGpsValidateCount() >= 12)
            {
                return true;
            }

            for (int retry = 5; retry != 0; --retry)
            {
                GPS_RESPONSE rep = p.gps.QueryGpsEphemeris(ref goldenGpsEphemeris);
                if (GPS_RESPONSE.ACK != rep)
                {
                    r.reportType = WorkerReportParam.ReportType.ShowProgress;
                    r.output = "Get golden GPS ephemeris failed";
                    p.bw.ReportProgress(0, new WorkerReportParam(r));
                    continue;
                }
                r.reportType = WorkerReportParam.ReportType.ShowProgress;
                r.output = "Get golden GPS ephemeris successfully";
                p.bw.ReportProgress(0, new WorkerReportParam(r));
                goldenGpsEphemerisTime = DateTime.Now;
                return true;
            }
            r.reportType = WorkerReportParam.ReportType.ShowError;
            p.error = WorkerParam.ErrorType.GetGoldenEphemerisFail;
            p.bw.ReportProgress(0, new WorkerReportParam(r));
            return false;
        }

        private int GetGoldenGpsValidateCount()
        {
            int count = 0;
            foreach (byte[] p in goldenGpsEphemeris)
            {
                count += (p != null) ? 1 : 0;
            }
            return count;
        }
    }
}
