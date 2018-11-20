using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ModuleTestV8
{
    public class ModuleIniParser
    {
        public enum ErrorCode
        {
            NoError,
            NoGpsModule,
        }
        public static int scanDelay = 500;
        public static ErrorCode Load(String path, ref List<String> rGps, ref List<String> rGlonass, 
            ref List<String> rBeidou, ref List<String> rNavic)
        {
            StringBuilder temp = new StringBuilder(255);
            int n = GetPrivateProfileString("Module", "ScanDelay", "500", temp, 255, path);
            scanDelay = Convert.ToInt32(temp.ToString());

            if (!LoadModule(path, "gpCount", "GPS", ref rGps))
            {
                return ErrorCode.NoGpsModule;
            }
            LoadModule(path, "glCount", "GLONASS", ref rGlonass);
            LoadModule(path, "bdCount", "BEIDOU", ref rBeidou);
            LoadModule(path, "giCount", "NAVIC", ref rNavic);
            return ErrorCode.NoError;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        private static bool LoadModule(String path, String moduleKey, String moduleSec, ref List<String> r)
        {
            StringBuilder temp = new StringBuilder(255);
            int n = GetPrivateProfileString("Module", moduleKey, "0", temp, 255, path);
            int count = Convert.ToInt32(temp.ToString());
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {   //Read GPS Module
                    String key = "Module" + (i + 1);
                    n = GetPrivateProfileString(moduleSec, key, "", temp, 255, path);
                    r.Add(temp.ToString());
                }
            }
            return (r.Count > 0);
        }

    }
}
