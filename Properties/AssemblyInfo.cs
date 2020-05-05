using System.Reflection;
using System.Runtime.InteropServices;

// 組件的一般資訊是由下列的屬性集控制。
// 變更這些屬性的值即可修改組件的相關
// 資訊。
[assembly: AssemblyTitle("ModuleTestV8")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ModuleTestV8")]
[assembly: AssemblyCopyright("Copyright ©2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 將 ComVisible 設定為 false 會使得這個組件中的型別
// 對 COM 元件而言為不可見。如果您需要從 COM 存取這個組件中
// 的型別，請在該型別上將 ComVisible 屬性設定為 true。
[assembly: ComVisible(false)]

// 下列 GUID 為專案公開 (Expose) 至 COM 時所要使用的 typelib ID
[assembly: Guid("28386711-c315-42f3-9c0d-64c536f084f9")]

// 組件的版本資訊是由下列四項值構成:
//
//      主要版本
//      次要版本 
//      組建編號
//      修訂編號
//
// 您可以指定所有的值，也可以依照以下的方式，使用 '*' 將組建和修訂編號
// 指定為預設值:
// [assembly: AssemblyVersion("1.0.*")]
// 1.0.0.XX - ModuleTest V8 2015, support voltage test
// 1.0.1.XX - ModuleTest V8 2008, support Windows XP for 日越
// 1.0.2.XX - ModuleTest V8 2015, support voltage test, DR test
// 1.0.3.XX - ModuleTest V8 2008, support V827 Socket board test

#if NO_MOTOR_VERSION
[assembly: AssemblyVersion("1.0.2.84")]
[assembly: AssemblyFileVersion("1.0.2.84")]
#else
[assembly: AssemblyVersion("1.0.0.82")]
[assembly: AssemblyFileVersion("1.0.0.82")]
#endif

// 1.0.2.84 - 20191113 Add version information in log xml, report from Angus.
// 1.0.2.84 - 20191113 Fixed download result issue, report from Angus.
// 1.0.2.84 - 20191111 Add Hot start after golden finished test, report from Angus.
// 1.0.2.83 - 20191111 Add retry in clock offset test, report from Angus.
// 1.0.2.83 - 20190625 Modify scan baud for support NACK, report from Alex.
// 1.0.2.82 - 20181225 Mody some issues (Ephemeris and rtk mode config) for RTK test, report from Angus.
// 1.0.2.81 - 20181211 Fixed GI clock offset test bug, report from Angus.
// 1.0.2.81 - 20181130 Turn off PSTI50 when test to fix mode for display GL snr bar.
// 1.0.2.81 - 20181130 Turn off message in download for S2525 4800 bps ROM code.
// 1.0.2.80 - 20181128 Retry 5 times when SendLoaderDownload, sometimes its no response.
// 1.0.2.80 - 20181128 Auto press Start/Stop button on STRSVR 2.4.3 b29 when test.
// 1.0.2.79 - 20181123 Fix issue in download after test will cause 1 slot slow.
// 1.0.2.78 - 20181221 Temp version in Umec tech.
// 1.0.2.77 - 20181109 Add try count in cold start.
// 1.0.2.76 - 20181109 Fixed GLONASS SNR test issue and slave CRC check issue.
// 1.0.2.75 - 20181108 Detect alpha FW before open license.
// 1.0.2.74 - 20181002 Navic clock offset test from channel 0, request from Terrance.
// 1.0.2.74 - 20181002 Add RTK fix mode test and support S1216DR8 test, report from Angus.
// 1.0.2.73 - 20180423 Fix slave download no prom issue, report from Angus.
// 1.0.2.72 - 20180313 Fix motor version bug.
// 1.0.2.71 - 20180313 Fix prom2.ini bug.
// 1.0.2.70 - 20180312 DR Test support inverst rotation.
// 1.0.0.69 - 20171208 Fix launch timer bus.
// 1.0.0.68 - 20171206 Support NAVIC test.
// 1.0.0.67 - 20171114 Adjust odo test flow in INSDR test.
// 1.0.0.66 - 20171113 Adjust delay after cold start in INSDR test.
// 1.0.0.65 - 20171109 New INSDR production request.
// 1.0.0.64 - 20171026 Fixed INSDR issues.
// 1.0.0.63 - 20171026 Fixed INSDR issues.
// 1.0.0.62 - 20171020 Support INSDR gyro and barometer test.
// 1.0.0.61 - 20170406 Support new ROM code, avoid reboot twice.
// 1.0.0.60 - 20170329 Fix [Test Voltage] bugs.
// 1.0.0.59 - 20170328 Fix [Test Voltage] upper issue.
// 1.0.0.59 - 20170612 Fix Load profile bug.
// 1.0.0.58 - 20170315 Add [Test Voltage] to DrTest.
// 1.0.0.57 - 20170310 Add [Test Voltage] using NI USB-6000, and moving project to VS 2005.
// 1.0.0.56 - 20170117 [Wait position fixed] support DoDrTest and fix bugs.
// 1.0.0.55 - 20161222 Add [Wait position fixed] and modify Clock Offset test flow.
// 1.0.0.54 - 20160921 Add scanDelay in Module.ini for scan baud.
// 1.0.0.53 - 20160902 Modify DR Gryo test, calibration by static gyro and add homing consuming.
// 1.0.0.52 - 20160427 Modify for DR Test, using sensor.
// 1.0.0.51 - 20160119 Fixed [Test RTC] setting can't be save issue.
// 1.0.0.50 - 20151001 Turn off NMEA output after SNR test to avoid response too slow in 4800 bps.
// 1.0.0.49 - Support NavSpark-mini test.
// 1.0.0.48 - 20150709 DR Module Test flow change.
// 1.0.0.47 - 20150625 Add DR Module Test support.
// 1.0.0.46 - Support Winbond new flash and add new module name.
// 1.0.0.45 - Fix report convert issue when continuous conversion.
// 1.0.0.43 - Add write protected in download loader.
// 1.0.0.42 - Add FactoryReset after download finished for flash ic write protected.
// 1.0.0.41 - Add Report SNR fields.
// 1.0.0.40 - Add Report convert function.
// 1.0.0.39 - Add UART2 TXRX as GPIO Test.
// 1.0.0.38 - Add some modules.
// 1.0.0.37 - Fix Antenna IO synchronization problems and fix COM port lost issue after unplug / plug-in.
// 1.0.0.36 - Support Antenna Detect testing.
// 1.0.0.35 - Change NavSpark ADC Test pass range.
// 1.0.0.34 - Change NavSpark ADC Test in srec.
// 1.0.0.32 - Support NavSpark ADC Test.
// 1.0.0.32 - Support NavSpark IO Test.
// 1.0.0.31 - Fix Download fail issue.
// 1.0.0.30 - Fix Test Clock Offset Fail issue.
// 1.0.0.29 - Add Get Clock Offset timeout for 2 UART firmware.
// 1.0.0.28 - Modify download function, boot from ROM code before downloading.
// 1.0.0.27 - Add retry in get clock offset.
// 1.0.0.27 - Add timeout check for i cache tester.
// 1.0.0.26 - Change CPU clock to pllclk/2 or pllclk/3.
// 1.0.0.25 - Adjust i-cache srec on / off PSE frequency.
// 1.0.0.24 - change i-cache srec test pattern.
// 1.0.0.23 - Add i-cache Tester branch.
// 1.0.0.22 - Fix write tag srec problem for OLink Star customize firmware.
// 1.0.0.21 - Show login setting in main windows in Reset Tester.
// 1.0.0.21 - Show correct error code list in Reset Tester.
// 1.0.0.21 - Change default value in Reset Tester.
// 1.0.0.20 - Remove Check Version in Module Test.
// 1.0.0.19 - Show NMEA Delay Detect in different message.
// 1.0.0.18 - ResetTester will show trigger time.
// 1.0.0.17 - NMEA Parser compatible with Olinkstar NMEA.
// 1.0.0.17 - Add check crc, and fixed compare snr error on beidou / glonass only use 2 satellites.
// 1.0.0.16 - Support tag download.
// 1.0.0.15 - Catalog log file by working number. Add period duration time to log file.
// 1.0.0.14 - Show error code number and retry three times in scan baud rate. Add slot yield information.
// 1.0.0.13 - It's only compare 2 satellites in Beidou.
// 1.0.0.12 - Add test report log in xml. Change Download button position.
// 1.0.0.11 - Show total time after download finished. Show download baud rate in main panel.
// 1.0.0.10 - Glonass snr only test 2 satellites.
// 1.0.0.09 - Add retry loop in first time command after open com port.
// 1.0.0.08 - Try Ready() function to wait serial port data come in.
// 1.0.0.07 - add download error processing.
// 1.0.0.06 - add log and report function.
// 1.0.0.05 - doesn't use -7 and 6 in k num to compare glonass snr.
// 1.0.0.04 - Compare glonass SNR by K-Number, Change icon.
// 1.0.0.03 - Add app / dialog icon, wait golden dialog, change setting dialog.
// 1.0.0.02 - Add clock offset test.
// 1.0.0.01 - first version.