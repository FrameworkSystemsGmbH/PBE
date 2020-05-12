using PBE.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class Batch : Executable
    {
        public String Cmd { get; private set; }
        public String Args { get; private set; }

        public Batch(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Cmd = container.ParseParameters(xe.Attribute("Cmd").Value);

            string args = null;
            var attrArgs = xe.Attribute("Args");
            if (attrArgs != null)
            {
                args = container.ParseParameters(attrArgs.Value);
            }

            var xeArgs = xe.Element("Args");
            if (xeArgs != null)
            {
                var elementArgs = FSUtils.EscapeCommandLineArgs(xeArgs.Elements("Arg")
                    .Select(xeArg => container.ParseParameters(xeArg.Value)));
                if (!string.IsNullOrWhiteSpace(elementArgs))
                {
                    if (!string.IsNullOrWhiteSpace(args))
                        args += " " + elementArgs;
                    else
                        args = elementArgs;
                }
            }

            this.Args = args;

            this.LogDetails += Cmd + " " + this.Args + Environment.NewLine +
                "-----------------------" + Environment.NewLine;
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
                UseShellExecute = false
            };
            Process proc = new Process();
            proc.StartInfo = startInfo;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;

            proc.OutputDataReceived += Proc_DataReceived;
            proc.ErrorDataReceived += Proc_DataReceived;

            proc.Start();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                this.TaskFailed = true;
        }

        private void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (this)
            {
                this.LogDetails += e.Data;
            }
        }
    }
}