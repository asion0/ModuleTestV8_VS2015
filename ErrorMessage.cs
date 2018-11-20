using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public class ErrorMessage
    {
        public enum Errors 
        { 
            NoGpsModule,
            ProfileFormatError,
            InvalidGpsModule,
            InvalidGlonassModule,
            InvalidBeidouModule,
            InvalidGalileoModule,
            InvalidNavicModule,
            ProfileHasInvalidModule,
            NoProfileError,
            NoFwIniError,
            PasswordError,
            WrongWorkingNo,
            NoPromFileError,
            CreateFolderFail,
        }

        public static void Show(Errors er)
        {
            switch (er)
            {
                case Errors.NoGpsModule:
                    MessageBox.Show(Program.rm.GetString("NoGpsModuleError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.ProfileFormatError:
                    MessageBox.Show(Program.rm.GetString("ProfileFormatError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.InvalidGpsModule:
                    MessageBox.Show(Program.rm.GetString("InvalidGpsModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.InvalidGlonassModule:
                    MessageBox.Show(Program.rm.GetString("InvalidGlonassModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.InvalidBeidouModule:
                    MessageBox.Show(Program.rm.GetString("InvalidBeidouModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.InvalidNavicModule:
                    MessageBox.Show(Program.rm.GetString("InvalidNavicModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.InvalidGalileoModule:
                    MessageBox.Show(Program.rm.GetString("InvalidGalileoModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.ProfileHasInvalidModule:
                    MessageBox.Show(Program.rm.GetString("ProfileHasInvalidModule"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.NoProfileError:
                    MessageBox.Show(Program.rm.GetString("NoProfileError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.NoFwIniError:
                    MessageBox.Show(Program.rm.GetString("NoFwIniError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.PasswordError:
                    MessageBox.Show(Program.rm.GetString("PasswordError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.WrongWorkingNo:
                    MessageBox.Show(Program.rm.GetString("WrongWorkingNo"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.NoPromFileError:
                    MessageBox.Show(Program.rm.GetString("NoPromFileError"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Errors.CreateFolderFail:
                    MessageBox.Show(Program.rm.GetString("CreateFolderFail"), Program.rm.GetString("MessageBoxErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        public enum Warnings
        {
            NoProfileWarning,
            NoGoldenSelectWarning,
            NoDeviceSelectWarning,
            EnterDebugMode,
        }

        public static void Show(Warnings er)
        {
            switch (er)
            {
                case Warnings.NoProfileWarning:
                    MessageBox.Show(Program.rm.GetString("NoProfileWarning"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case Warnings.NoGoldenSelectWarning:
                    MessageBox.Show(Program.rm.GetString("NoGoldenSelectWarning"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case Warnings.NoDeviceSelectWarning:
                    MessageBox.Show(Program.rm.GetString("NoDeviceSelectWarning"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case Warnings.EnterDebugMode:
                    MessageBox.Show(Program.rm.GetString("EnterDebugMode"), Program.rm.GetString("MessageBoxWarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }
    }

}
