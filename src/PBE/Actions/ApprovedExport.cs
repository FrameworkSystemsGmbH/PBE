using System;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class ApprovedExport : Export
    {
        public const String TypeName = "ApprovedExport";

        public ApprovedExport(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
        }

        protected override string ExportDirParameter
        {
            get { return "ApprovedExportDir"; }
        }

        /// <summary>
        /// Beim Approved Export darf es keine Queue geben.
        /// </summary>
        public override string Queue
        {
            get { return null; }
            protected set { return; }
        }
    }
}