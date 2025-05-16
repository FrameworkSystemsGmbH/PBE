using PBE.Utils;
using System;
using System.IO;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class Import : FSConsole
    {
        public string Package { get; private set; }
        public string Version { get; private set; }
        public string ExportFile { get; private set; }
        public string Dir { get; private set; }
        public string Mode { get; private set; }

        public Import(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var attrDir = xe.Attribute("Dir");
            if (attrDir != null)
            {
                Dir = container.ParseParameters(attrDir.Value);
            }
            else
            {
                Dir = container.ParseParameters("{ExportDir}");
            }

            var xaFile = xe.Attribute("ExportFile");
            if (xaFile != null)
            {
                this.ExportFile = container.ParseParameters(xaFile.Value);
                string file = Path.Combine(this.Dir, this.ExportFile);

                bool fsVerOk = FSVersion.TryParse(this.FSVersionString, out FSVersion fsVer);

                this.Arguments = "\\IMPORT \"" + file + "\"";
                // Seit FS 3.5 wird das Debug-Paket automatisch mit importiert
                // und seit FS 3.10 gibt es keine Separate Debug-Datei mehr - da passt diese Bedingung hier auch.
                if (fsVerOk && fsVer.Version < new Version(3, 5))
                {
                    if (file.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Arguments2 = "\\IMPORT \"" + Path.ChangeExtension(file, ".debugdb") + "\"";
                    }
                    else if (file.EndsWith(".bugfixdb", StringComparison.OrdinalIgnoreCase))
                    {
                        if (fsVer.Version >= new Version(3, 4))
                            this.Arguments2 = "\\IMPORT \"" + Path.ChangeExtension(file, ".debugbugfixdb") + "\"";
                        else
                            this.Arguments2 = "\\IMPORT \"" + Path.ChangeExtension(file, ".debugdb") + "\"";
                    }
                }
            }
            else
            {
                Package = container.ParseParameters(xe.Attribute("Package").Value);
                Version = container.ParseParameters(xe.Attribute("Version").Value);
                var attrMode = xe.Attribute("Mode");
                if (attrMode != null)
                {
                    this.Mode = attrMode.Value;
                }

                string file = Path.Combine(this.Dir, container.CreateExportFileName(this.Package, this.Version, this.FSVersionString));

                bool fsVerOk = FSVersion.TryParse(this.FSVersionString, out FSVersion fsVer);

                if ("Bugfix".Equals(this.Mode, StringComparison.OrdinalIgnoreCase))
                {
                    if (fsVerOk && fsVer.Version >= new Version(3, 8))
                    {
                        // seit FS 3.8 heißt "Bugfix" "Service Release" (SR)
                        this.Arguments = "\\IMPORT \"" + file + ".srdb\"";
                    }
                    else if (fsVerOk && fsVer.Version >= new Version(3, 5))
                    {
                        this.Arguments = "\\IMPORT \"" + file + ".bugfixdb\"";
                        // seit FS 3.5 wird das Debug-Paket automatisch mit importiert
                    }
                    else if (fsVerOk && fsVer.Version >= new Version(3, 4))
                    {
                        this.Arguments = "\\IMPORT \"" + file + ".bugfixdb\"";
                        // seit FS 3.4 gibt es die debugbugfixdb
                        this.Arguments2 = "\\IMPORT \"" + file + ".debugbugfixdb\"";
                    }
                    else
                    {
                        this.Arguments = "\\IMPORT \"" + file + ".bugfixdb\"";
                        this.Arguments2 = "\\IMPORT \"" + file + ".debugdb\"";
                    }
                }
                else
                {
                    this.Arguments = "\\IMPORT \"" + file + ".db\"";
                    // Seit FS 3.5 wird das Debug-Paket automatisch mit importiert
                    if (!fsVerOk || fsVer.Version < new Version(3, 5))
                    {
                        this.Arguments2 = "\\IMPORT \"" + file + ".debugdb\"";
                    }
                }
            }
        }

        public override void ExecuteAction()
        {
            lock (this.Container.GetImportLocker(this.Rep))
            {
                base.ExecuteAction();
            }

            if (this.ExitCode1 == -1)
            {
                this.TaskFailed = true;
            }
        }

        public override string Description
        {
            get
            {
                return "FS " + this.FSVer.ToString(2) + " " + this.Rep + " Import " + this.Mode + " "
                    + (!string.IsNullOrEmpty(this.ExportFile)
                        ? ExportFile
                        : (this.Package + " - " + this.Version));
            }
        }
    }
}