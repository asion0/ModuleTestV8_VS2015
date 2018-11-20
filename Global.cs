using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace ModuleTestV8
{
  
    class Global
    {
        public static void InjectionBaudRate(ComboBox c)
        {
            c.Items.AddRange(new object[] {
                4800, 9600, 19200, 38400, 57600,
                115200, 230400, 460800, 921600 });
        }

        public static int GetTextBoxPositiveInt(TextBox t)
        {
            int value = 0;
            try
            {
                value = Convert.ToInt32(t.Text);
                t.ForeColor = (value > 0) ? Color.Black : Color.Red;
            }
            catch
            {
                t.ForeColor = Color.Red;
            }
            return value;
        }

        public static int GetTextBoxInt(TextBox t)
        {
            int value = 0;
            try
            {
                value = Convert.ToInt32(t.Text);
                t.ForeColor = Color.Black;
            }
            catch
            {
                t.ForeColor = Color.Red;
            }
            return value;
        }

        public static double GetTextBoxPositiveDouble(TextBox t)
        {
            double value = 0.0;
            try
            {
                value = Convert.ToDouble(t.Text);
                t.ForeColor = (value >= 0.0) ? Color.Black : Color.Red;
            }
            catch
            {
                t.ForeColor = Color.Red;
            }
            return value;
        }

        public enum FunctionType
        {
            ModuleTest,
            ResetTester,
            OpenPortTester,
            iCacheTester,
            ModuleTestDr,
        }

        public enum DrTestType
        {
            UseMotor,
            NoMotorVersion,
        }

        public static FunctionType functionType = FunctionType.ModuleTest;
        public static DrTestType drTestType = DrTestType.UseMotor;

        [Conditional("_RESET_TESTER_")]
        public static void ResetTester()
        {
            functionType = FunctionType.ResetTester;
        }

        [Conditional("_OPEN_PORT_TESTER_")]
        public static void OpenPortTester()
        {
            functionType = FunctionType.OpenPortTester;
        }

        [Conditional("_ICACHE_TESTER_")]
        public static void iCacheTester()
        {
            functionType = FunctionType.iCacheTester;
        }

        [Conditional("NO_MOTOR_VERSION")]
        public static void NoMotorVersion()
        {
            functionType = FunctionType.ModuleTestDr;
            drTestType = DrTestType.NoMotorVersion;
        }

        [Conditional("MOTOR_VERSION")]
        public static void MotorVersion()
        {
            functionType = FunctionType.ModuleTestDr;
            drTestType = DrTestType.UseMotor;
        }

        public static void Init()
        {
            ResetTester();
            OpenPortTester();
            iCacheTester();
            NoMotorVersion();
            MotorVersion();
        }
    }
}
