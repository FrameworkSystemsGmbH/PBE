using System.IO;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class RemoveDir : Executable
    {
        public RemoveDir(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.Dir = container.ParseParameters(xe.Attribute("Dir").Value);
        }

        public string Dir { get; private set; }

        public override string Description
        {
            get
            {
                return "RD " + ExecutableContainer.MakeShortText(this.Dir);
            }
        }

        public override void ExecuteAction()
        {
            if (Directory.Exists(this.Dir))
                Directory.Delete(this.Dir, true);
        }
    }
}