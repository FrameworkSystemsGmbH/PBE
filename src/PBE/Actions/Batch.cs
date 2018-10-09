using System;
using System.Diagnostics;
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
            var attrArgs = xe.Attribute("Args");
            if (attrArgs != null)
            {
                this.Args = container.ParseParameters(attrArgs.Value);
            }
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
                WindowStyle = ProcessWindowStyle.Minimized
            };
            Process proc = new Process();
            proc.StartInfo = startInfo;

            proc.Start();
            proc.WaitForExit();
        }
    }
}