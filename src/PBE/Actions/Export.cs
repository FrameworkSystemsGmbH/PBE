using System;
using System.IO;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class Export : FSConsole
    {
        public String Package { get; private set; }
        public String Version { get; private set; }
        public String Dir { get; private set; }
        public String Mode { get; private set; }
        public string IncludeBasePackages { get; private set; }
        public virtual String Queue { get; protected set; }
        protected virtual string ExportDirParameter { get { return "ExportDir"; } }
        public string ExportFile1 { get; private set; }
        public string ExportFile2 { get; private set; }
        public string ExportFileName { get; private set; }

        public Export(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var xaDir = xe.Attribute("Dir");
            if (xaDir != null)
            {
                Dir = container.ParseParameters(xaDir.Value);
            }
            else
            {
                Dir = container.ParseParameters("{" + this.ExportDirParameter + "}");
            }
            Package = container.ParseParameters(xe.Attribute("Package").Value);
            Version = container.ParseParameters(xe.Attribute("Version").Value);
            var xaExportFileName = xe.Attribute("ExportFileName");
            ExportFileName = xaExportFileName != null ? container.ParseParameters(xaExportFileName.Value) : null;
            var attrMode = xe.Attribute("Mode");

            if (attrMode != null)
            {
                this.Mode = attrMode.Value;
            }

            string file = Path.Combine(this.Dir, container.CreateExportFileName(this.Package, this.Version, this.FSVersion, this.ExportFileName));

            var xaQueue = xe.Attribute("Queue");
            if (xaQueue != null)
                this.Queue = xaQueue.Value;

            this.IncludeBasePackages = (string)xe.Attribute("IncludeBasePackages");
            string argumentIncludeBasePackages = null;
            if (!String.IsNullOrEmpty(this.IncludeBasePackages))
                argumentIncludeBasePackages = @" \IncludeBasePackages " + this.IncludeBasePackages;

            System.Version fsVer;
            bool fsVerOk = System.Version.TryParse(this.FSVersion, out fsVer);

            if ("Bugfix".Equals(this.Mode, StringComparison.OrdinalIgnoreCase))
            {
                if (fsVerOk && fsVer >= new System.Version(3, 10))
                {
                    // seit FS 3.10 gibt es keine separate Debug-Datei mehr
                    this.ExportFile1 = file + ".srdb";
                }
                else if (fsVerOk && fsVer >= new System.Version(3, 8))
                {
                    // seit FS 3.8 heißt Bugfix ServierRelease (SR)
                    this.ExportFile1 = file + ".srdb";
                    this.ExportFile2 = file + ".debugsrdb";
                }
                else if (fsVerOk && fsVer >= new System.Version(3, 4))
                {
                    // seit FS 3.4 gibt es die debugbugfixdb
                    this.ExportFile1 = file + ".bugfixdb";
                    this.ExportFile2 = file + ".debugbugfixdb";
                }
                else
                {
                    this.ExportFile1 = file + ".bugfixdb";
                    this.ExportFile2 = file + ".debugdb";
                }

                if (!String.IsNullOrEmpty(this.ExportFile1))
                    this.Arguments = "\\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\" \\EXPORT true \"" + this.ExportFile1 + "\"" + argumentIncludeBasePackages;
                if (!String.IsNullOrEmpty(this.ExportFile2))
                    this.Arguments2 = "\\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\" \\EXPORT true \"" + this.ExportFile2 + "\"" + argumentIncludeBasePackages;
            }
            else
            {
                if (fsVerOk && fsVer >= new System.Version(3, 10))
                {
                    // seit FS 3.10 gibt es keine separate Debug-Datei mehr
                    this.ExportFile1 = file + ".db";
                    this.Arguments = "\\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\" \\EXPORT true \"" + this.ExportFile1 + "\"" + argumentIncludeBasePackages;
                }
                else
                {
                    this.ExportFile1 = file + ".db";
                    this.Arguments = "\\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\" \\EXPORT true \"" + this.ExportFile1 + "\"";

                    this.ExportFile2 = file + ".debugdb";
                    this.Arguments2 = "\\PACKAGE \"" + this.Package + "\" \\VERSION \"" + this.Version + "\" \\EXPORT true \"" + this.ExportFile2 + "\"";
                }
            }
        }

        public override void ExecuteAction()
        {
            if (!string.IsNullOrEmpty(this.ExportFile1))
            {
                string dir1 = Path.GetDirectoryName(this.ExportFile1);
                if (!Directory.Exists(dir1))
                    Directory.CreateDirectory(dir1);
            }
            if (!string.IsNullOrEmpty(this.ExportFile2))
            {
                string dir2 = Path.GetDirectoryName(this.ExportFile2);
                if (!Directory.Exists(dir2))
                    Directory.CreateDirectory(dir2);
            }

            base.ExecuteAction();

            if (this.ExitCode1.GetValueOrDefault(0) < 0)
            {
                // Fehler beim Export => Sicherstellen evtl. schon teilweise erzeugte Package-Dateien gelöscht werden
                this.TaskFailed = true;
                if (!String.IsNullOrEmpty(this.ExportFile1) && File.Exists(this.ExportFile1))
                {
                    File.Delete(this.ExportFile1);
                }
                if (!String.IsNullOrEmpty(this.ExportFile2) && File.Exists(this.ExportFile2))
                {
                    File.Delete(this.ExportFile2);
                }
            }
            else if (this.ExitCode2.GetValueOrDefault(0) < 0)
            {
                // Debug konnte nicht exportiert werden => kein großes Problem
                if (!String.IsNullOrEmpty(this.ExportFile2) && File.Exists(this.ExportFile2))
                {
                    File.Delete(this.ExportFile2);
                }
            }

            if (!String.IsNullOrEmpty(this.Queue) &&
                this.ExitCode2.GetValueOrDefault(0) >= 0 &&
                this.ExitCode1.GetValueOrDefault(0) >= 0)
            {
                ImportQueue targetQueue;
                if (this.Container.ImportQueues.TryGetValue(this.Queue, out targetQueue))
                {
                    targetQueue.Enqueue(this);
                }
            }
        }

        public override string Description
        {
            get
            {
                return "FS " + this.FSVer.ToString(2) + " " + this.Rep + " Export " + this.Mode + " " + this.Package + " - " + this.Version
                     + (this.ExitCode1.HasValue ? (" (" + this.ExitCode1.Value + ")") : "");
            }
        }
    }
}
