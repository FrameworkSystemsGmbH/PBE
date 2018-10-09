using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class ApprovedImport : Executable
    {
        public const String TypeName = "ApprovedImport";
        public override bool IsComplex { get { return true; } }

        public String Dir { get; private set; }
        public String HistoryDir { get; private set; }
        public String Rep { get; private set; }

        private List<Import> ImportList = new List<Import>();

        public ApprovedImport(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var xaDir = xe.Attribute("Dir");
            this.Dir = container.ParseParameters(xaDir != null ? xaDir.Value : "{ApprovedImportDir}");

            var xaHistoryDir = xe.Attribute("HistoryDir");
            this.HistoryDir = container.ParseParameters(xaDir != null ? xaDir.Value : "{ApprovedHistoryDir}");

            // nicht parsen - wird erst im Execute geparsed
            this.Rep = xe.Attribute("Rep").Value;
        }

        public override void ExecuteAction()
        {
            var dir = new DirectoryInfo(this.Dir);
            if (!dir.Exists)
                return;

            // Die dateien sortiert verarbeiten, falls es für ein Package mehrere Exporte gibt,
            // dann werden die älteren Dateien zuerst verarbeitet.
            foreach (var file in dir.EnumerateFiles().OrderBy(file => file.Name))
            {
                var xeImport = new XElement("Import");

                switch (file.Extension.ToLowerInvariant())
                {
                    // Nur die Haupt-Dateien verarbeiten. Die Bebug-Inormationen kommen von alleine mit.
                    case ".db":
                    case ".srdb":
                    case ".bugfixdb":
                        // Die FS-Version ermitteln
                        var match = Regex.Match(file.Name, "[(]FS[ ](?<version>.*)[)][.][a-zA-Z]+$");
                        if (match == null)
                            continue;

                        var group = match.Groups["version"];
                        if (group == null)
                            continue;

                        var fsVersion = Version.Parse(group.Value);
                        fsVersion = new Version(
                            fsVersion.Major,
                            fsVersion.Minor,
                            fsVersion.Build == -1 ? 0 : fsVersion.Build,
                            fsVersion.Revision == -1 ? 0 : fsVersion.Revision);

                        xeImport.Add(new XAttribute("FS", fsVersion.ToString(4)));
                        xeImport.Add(new XAttribute("Rep", this.Rep));
                        xeImport.Add(new XAttribute("Dir", this.Dir));
                        xeImport.Add(new XAttribute("ExportFile", file.Name));

                        ImportList.Add(new Import(xeImport, this.Container, this.Indent + 1));

                        break;
                }
            }

            DirectoryInfo historyDir;
            if (string.IsNullOrEmpty(this.HistoryDir))
            {
                historyDir = null;
            }
            else
            {
                historyDir = new DirectoryInfo(this.HistoryDir);
                if (!historyDir.Exists)
                    historyDir.Create();
            }

            // Die gesammelten Importe jetzt der Reihe nach ausführen.
            foreach (var import in this.ImportList)
            {
                import.Execute();

                try
                {
                    // Nach erfolgreichem Import die Dateien in den Histroy-Ordner verschieben.
                    if (!import.TaskFailed)
                    {
                        foreach (var file in dir.EnumerateFiles(Path.GetFileNameWithoutExtension(import.ExportFile) + ".*"))
                        {
                            if (historyDir == null)
                            {
                                // wen kein Histroy-Ordner angegeben ist, dann die Dateien löschen.
                                file.Delete();
                            }
                            else
                            {
                                var targetFile = Path.Combine(this.HistoryDir, file.Name);
                                // Wenn die Datei schon im Ziel-Ordner vorhanden ist, dann zuvor löschen.
                                if (File.Exists(targetFile))
                                    File.Delete(targetFile);
                                file.MoveTo(targetFile);
                            }
                        }

                        // Auch den Ordner des Service-Release-Export verschieben
                        var directory = new DirectoryInfo(Path.Combine(this.Dir, Path.GetFileNameWithoutExtension(import.ExportFile)));
                        if (directory.Exists)
                        {
                            if (historyDir == null)
                            {
                                // wen kein Histroy-Ordner angegeben ist, dann die Dateien löschen.
                                directory.Delete(true);
                            }
                            else
                            {
                                var targetDir = Path.Combine(this.HistoryDir, directory.Name);
                                // Wenn die Datei schon im Ziel-Ordner vorhanden ist, dann zuvor löschen.
                                if (Directory.Exists(targetDir))
                                    Directory.Delete(targetDir, true);
                                directory.MoveTo(targetDir);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.LogDetails += "Error moving files " + import.ExportFile + "\r\n" + ex.ToString() + "\r\n";
                    this.TaskFailed = true;
                }
            }
        }

        protected override void WriteContentToHtml(System.Text.StringBuilder sb)
        {
            foreach (var exec in this.ImportList)
            {
                if (exec != null)
                {
                    sb.Append("<tr>");
                    exec.WriteToHtml(sb);
                    sb.AppendLine("</tr>");
                }
            }
        }

        public override string Description
        {
            get
            {
                if (!String.IsNullOrEmpty(this.Name))
                    return this.Name;
                return "ApprovedImport " + this.Rep;
            }
        }
    }
}