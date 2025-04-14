using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class ImportQueue : Executable
    {
        public const String TypeName = "ImportQueue";
        public override bool IsComplex { get { return true; } }

        public String QueueName { get; private set; }
        public String Rep { get; private set; }

        private ConcurrentQueue<Import> importQueue = new ConcurrentQueue<Import>();
        private ConcurrentQueue<Import> importList = new ConcurrentQueue<Import>();

        public ImportQueue(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.QueueName = xe.Attribute("QueueName").Value;
            this.Rep = xe.Attribute("Rep").Value;

            // Die ImportQueue muss sich im Container registirieren
            container.ImportQueues.Add(this.QueueName, this);
        }

        public void Enqueue(Export export)
        {
            var xeImport = new XElement("Import",
                export.Name != null ? new XAttribute("Name", export.Name) : null,
                new XAttribute("FS", export.FSVersion),
                new XAttribute("Rep", this.Rep),
                new XAttribute("Package", export.Package),
                new XAttribute("Version", export.Version),
                !string.IsNullOrEmpty(export.Dir) ? new XAttribute("Dir", export.Dir) : null,
                export.Mode != null ? new XAttribute("Mode", export.Mode) : null);

            Import import = new Import(xeImport, this.Container, this.Indent + 1);

            importQueue.Enqueue(import);
            importList.Enqueue(import);
        }

        private bool joining = false;
        private bool joined = false;

        public void Join()
        {
            joining = true;
            while (!joined)
            {
                Thread.Sleep(10);
            }
        }

        public override void ExecuteAction()
        {
            while (!joining || !this.importQueue.IsEmpty)
            {
                Import import;
                while (this.importQueue.TryDequeue(out import))
                {
                    import.Execute();
                }

                Thread.Sleep(100);
            }
            joined = true;
        }

        protected override void WriteContentToHtml(System.Text.StringBuilder sb)
        {
            foreach (var exec in this.importList)
            {
                if (exec != null)
                {
                    sb.Append("<tr>");
                    exec.WriteToHtml(sb);
                    sb.AppendLine("</tr>");
                }
            }
        }
    }
}