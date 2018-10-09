using System;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class Publish : FSConsole
    {
        public String Package { get; private set; }
        public String Version { get; private set; }
        public String Setting { get; private set; }

        public Publish(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Package = container.ParseParameters(xe.Attribute("Package").Value);
            this.Version = container.ParseParameters(xe.Attribute("Version").Value);
            this.Setting = container.ParseParameters(xe.Attribute("Setting").Value);

            this.Arguments = "\\PUBLISH \\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\""
                + " \\SETTING \"" + this.Setting + "\"";
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            if (this.ExitCode1 == -1)
            {
                this.TaskFailed = true;
            }
        }

        public override string Description
        {
            get
            {
                return "FS " + this.FSVer.ToString(2) + " " + this.Rep + " Publish "
                    + this.Package + " - " + this.Version
                    + " Setting=" + this.Setting +
                    (this.ExitCode1.HasValue && this.ExitCode1 != 0 && this.ExitCode1 != -1 ? (" (" + this.ExitCode1.Value + ")") : "");
            }
        }
    }
}