using System;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class MakeDir : Executable
    {
        public MakeDir(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Dir = container.ParseParameters(xe.Attribute("Dir").Value);
        }

        public String Dir { get; private set; }

        public override string Description
        {
            get
            {
                return "MD " + ExecutableContainer.MakeShortText(this.Dir);
            }
        }

        public override void ExecuteAction()
        {
            System.IO.Directory.CreateDirectory(this.Dir);
        }
    }

    internal class RemoveDir : Executable
    {
        public RemoveDir(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Dir = container.ParseParameters(xe.Attribute("Dir").Value);
        }

        public String Dir { get; private set; }

        public override string Description
        {
            get
            {
                return "RD " + ExecutableContainer.MakeShortText(this.Dir);
            }
        }

        public override void ExecuteAction()
        {
            System.IO.Directory.Delete(this.Dir, true);
        }
    }
}