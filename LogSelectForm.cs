using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ModuleTestV8
{
    public partial class LogSelectForm : Form
    {
        public LogSelectForm()
        {
            InitializeComponent();
        }

        private struct ListMember
        {
            private string name;
            private string fullPathName;
            private string outputPathName;

            public ListMember(string logFolder, string dateFolder)
            {
                name = Path.GetFileName(logFolder) + "\\" + Path.GetFileName(dateFolder);
                fullPathName = dateFolder + "\\" + ModuleTestV8.ModuleTestForm.LogFileName;
                outputPathName = logFolder + "_" + Path.GetFileName(dateFolder) + ".xlsx";
            }

            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    name = value;
                }
            }

            public string FullPathName
            {
                get
                {
                    return fullPathName;
                }
                set
                {
                    fullPathName = value;
                }
            }

            public string OutputPathName
            {
                get
                {
                    return outputPathName;
                }
            }
        }

        List<ListMember> listItems = new List<ListMember>(); 
        private void LogSelectForm_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\\{1}", Login.loginInfo.currentPath, ModuleTestV8.ModuleTestForm.LogFolderName);
            if (!Directory.Exists(sb.ToString()))
            {
                return;
            }

            //Scan the Log folder
            foreach (string d1 in Directory.GetDirectories(sb.ToString()))
            {
                foreach (string d2 in Directory.GetDirectories(d1))
                {
                    //string displayName = Path.GetFileName(d1) + "\\" + Path.GetFileName(d2);
                    //logList.Items.Add(displayName, CheckState.Checked);
                    ListMember l = new ListMember(d1, d2);
                    listItems.Add(l);
  
                    /*
                    String excelFile = sb.ToString() + "\\" + Path.GetFileName(d1) + "_" + Path.GetFileName(d2) + ".xlsx";
                    if (HasWritePermission(excelFile))
                    {

                        ExcelDocument ed = new ExcelDocument(excelFile);
                        ed.InitDocument();
                        ParsingResultXml(d2 + "\\" + ModuleTestV8.Form1.LogFileName, ed);
                        ed.Save();
                    }
                    */
                    //return;
                }
            }
            logList.DataSource = listItems;
            logList.DisplayMember = "Name";

            for (int i = 0; i < logList.Items.Count; i++)
            {
                if (!File.Exists(listItems[i].OutputPathName))
                    logList.SetItemChecked(i, true);
            }         
        }

        private BackgroundWorker bw = new BackgroundWorker();
        private void convertBtn_Click(object sender, EventArgs e)
        {
            if (logList.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one log file!", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(BwDoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(BwProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BwRunWorkerCompleted);
            bw.RunWorkerAsync();
            convertBtn.Enabled = false;

            progressBar.Maximum = logList.CheckedItems.Count;
            progressBar.Minimum = 0;
            progressBar.Value = 0;
        }

        private void BwDoWork(object sender, DoWorkEventArgs e)
        {
            ExcelWriter ew = new ExcelWriter();
            int count = 0;
            foreach (object o in logList.CheckedItems)
            {
                ListMember l = (ListMember)o;
                ew.DoConvert(l.FullPathName, l.OutputPathName);
                bw.ReportProgress(++count, null);
            }
        }

        private void BwProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
            progressBar.Value = e.ProgressPercentage;

        }

        private void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = (sender as BackgroundWorker);
            convertBtn.Enabled = true;
        }
    }
}
