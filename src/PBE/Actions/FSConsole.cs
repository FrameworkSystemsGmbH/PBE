using PBE.Utils;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class FSConsole : Executable
    {
        public FSConsole(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.FSVersionString = container.ParseParameters(xe.Attribute("FS").Value);

            if (FSVersion.TryParse(this.FSVersionString, out FSVersion fsVer))
            {
                this.FSVer = fsVer;
            }

            // nicht parsen - wird erst im Execute geparsed
            this.Rep = xe.Attribute("Rep").Value;

            var arguments = ParseArgs(xe, container);
            if (!string.IsNullOrEmpty(arguments))
            {
                this.Arguments = arguments;
            }
        }

        public string Rep { get; private set; }
        public string FSVersionString { get; private set; }
        public FSVersion FSVer { get; private set; }
        public string Arguments { get; protected set; }
        public string Arguments2 { get; protected set; }

        protected int? ExitCode1;
        protected int? ExitCode2;

        public override string Description
        {
            get
            {
                string desc = "FS " + FSVer.ToString(2) + " " + this.Rep;
                if (!string.IsNullOrEmpty(this.Name))
                {
                    desc += " " + this.Name;
                }
                else
                {
                    desc += " " + this.Arguments;
                }
                if (this.ExitCode1.HasValue)
                {
                    desc += " (" + this.ExitCode1.Value + ")";
                }
                return desc;
            }
        }

        public override void ExecuteAction()
        {
            string dir = this.Container.GetFSDirectory(this.FSVersionString);
            string execPath = Path.Combine(dir, "FSConsole.exe");

            string tempLogFile = PBEContext.CurrentContext.GetTempLogFile();
            this.LogFile = tempLogFile;

            {
                string args = this.Container.ParseParameters(this.Rep) + " " + Arguments + " \\LOGFILE \"" + tempLogFile + "\"";
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = execPath,
                    Arguments = args,
                    WorkingDirectory = dir,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true
                };
                Process proc = new Process();
                proc.StartInfo = startInfo;

                proc.Start();
                proc.WaitForExit();
                ExitCode1 = proc.ExitCode;
                if (proc.ExitCode < 0)
                {
                    this.TaskFailed = true;
                }
            }
            if (!string.IsNullOrWhiteSpace(this.Arguments2))
            {
                string args = this.Container.ParseParameters(this.Rep) + " " + Arguments2 + " \\LOGFILE \"" + tempLogFile + "\"";
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = execPath,
                    Arguments = args,
                    WorkingDirectory = dir,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    CreateNoWindow = true
                };
                Process proc = new Process();
                proc.StartInfo = startInfo;

                proc.Start();
                proc.WaitForExit();
                ExitCode2 = proc.ExitCode;
                if (proc.ExitCode < 0)
                {
                    this.TaskFailed = true;
                }
            }
        }
    }
}