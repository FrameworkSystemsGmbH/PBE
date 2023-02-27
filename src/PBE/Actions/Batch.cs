using PBE.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class Batch : Executable
    {
        public string Cmd { get; private set; }
        public string Directory { get; private set; }
        public string Args { get; private set; }

        public Batch(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Cmd = container.ParseParameters(xe.Attribute("Cmd").Value);

            this.Directory = container.ParseParameters((string)xe.Attribute("Directory"));
            if (!string.IsNullOrWhiteSpace(this.Directory))
                this.Directory = Path.GetFullPath(this.Directory);

            this.Args = ParseArgs(xe, container);

            this.LogDetails += Cmd + " " + this.Args + Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(this.Directory))
                this.LogDetails += "Working Directory: " + this.Directory + Environment.NewLine;
            this.LogDetails += "-----------------------" + Environment.NewLine;
        }

        public override string Description
        {
            get
            {
                if (!String.IsNullOrEmpty(this.Name))
                {
                    return "Batch " + this.Name;
                }
                return "Batch " + ExecutableContainer.MakeShortText(this.Cmd);
            }
        }

        public override void ExecuteAction()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = this.Cmd,
                Arguments = this.Args,
                WindowStyle = ProcessWindowStyle.Minimized,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            Process proc = new Process();
            proc.StartInfo = startInfo;
            if (!string.IsNullOrWhiteSpace(this.Directory))
                proc.StartInfo.WorkingDirectory = this.Directory;

            proc.OutputDataReceived += Proc_DataReceived;
            proc.ErrorDataReceived += Proc_DataReceived;

            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                LogDetails += "====================="
                    + Environment.NewLine
                    + "ExitCode: " + proc.ExitCode;

                //Robocopy ab ExitCode 8 wird als Fehler betrachtet
                // https://learn.microsoft.com/de-de/troubleshoot/windows-server/backup-and-storage/return-codes-used-robocopy-utility
                if (!string.Equals(this.Cmd, "robocopy", StringComparison.OrdinalIgnoreCase) || proc.ExitCode >= 8)
                {
                    this.TaskFailed = true;
                }
            }
        }

        private void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (this)
            {
                this.LogDetails += e.Data + Environment.NewLine;
            }
        }
    }
}
