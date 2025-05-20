using PBE.CommandLineProcessor;
using PBE.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PBE
{
    public class ExecutableContainer
    {
        public string Logfile { get; private set; }
        public string Logarchive { get; private set; }
        public string XmlFilecontetForLog { get; private set; }

        private ConcurrentDictionary<string, string> FsVerdict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetFSDirectory(string fsVersion)
        {
            // Prio1 - Es gibt schon eine Information (z.B. über FSVersion definiert, oder bereits ausgewertet)
            if (this.FsVerdict.TryGetValue(fsVersion, out string dir))
                return dir;

            FSVersion fsVer = new FSVersion(fsVersion);
            string fsPath;

            // Prio2 - N&V Ordner-Struktur (neu - localappdata)
            dir = Path.Combine(ParseParameters("{LocalAppData}"), "Programs", "FS");
            if (TryGetFSVersionPath(dir, fsVer, out fsPath))
            {
                this.FsVerdict.TryAdd(fsVersion, fsPath);
                return fsPath;
            }

            // Prio3 - N&V Ordner-Struktur (alt)
            dir = "C:\\FS";
            if (TryGetFSVersionPath(dir, fsVer, out fsPath))
            {
                this.FsVerdict.TryAdd(fsVersion, fsPath);
                return fsPath;
            }

            // Prio4 - Standard Installations-Verzeichnis:
            // - Framework Studio\x.y
            dir = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "enventa Group", "Framework Studio");
            if (TryGetFSVersionPath(dir, fsVer, out fsPath))
            {
                this.FsVerdict.TryAdd(fsVersion, fsPath);
                return fsPath;
            }

            // Prio5 - altes Standard Installations-Verzeichnis in folgender Priorisierung:
            // - Framework Studio x.y.12
            // - Framework Studio x.y.2
            // - Framework Studio x.y
            // - Framework Studio x.y Beta2
            // - Framework Studio x.y Beta
            dir = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "Framework Systems");
            if (TryGetFSVersionPath(dir, fsVer, out fsPath))
            {
                this.FsVerdict.TryAdd(fsVersion, fsPath);
                return fsPath;
            }

            throw new ApplicationException("No installation found for Framework Studio " + fsVersion + ".");
        }

        private bool TryGetFSVersionPath(string basePath, FSVersion fsVersion, out string fsPath)
        {
            fsPath = string.Empty;

            if (!Directory.Exists(basePath))
                return false;

            string pattern = $@"^(Framework Studio )?{Regex.Escape(fsVersion.Version.ToString(2))}((\.\d*)*)?";

            var fsDirs = Directory.EnumerateDirectories(basePath)
                            .Where(d => Regex.IsMatch(Path.GetFileName(d), pattern, RegexOptions.IgnoreCase))
                            .Where(d => File.Exists(Path.Combine(d, "FSConsole.exe")));

            if (!fsDirs.Any())
                return false;

            // Preview Version auslesen
            if (fsVersion.IsPreview)
            {
                fsPath = fsDirs.Where(d => Regex.IsMatch(Path.GetFileName(d), "(.?Preview)$", RegexOptions.IgnoreCase)).FirstOrDefault();

                if (!string.IsNullOrEmpty(fsPath))
                    return true;
            }

            // Normale Version auslesen
            Version maxV = null;
            foreach (string path in fsDirs.Where(d => Regex.IsMatch(Path.GetFileName(d), @"(?<version>\d\.\d((\.\d*)*)?)$")))
            {
                Match m = Regex.Match(Path.GetFileName(path), @"(?<version>\d\.\d((\.\d*)*)?)$");

                if (Version.TryParse(m.Groups["version"].Value, out Version v) && (maxV == null || v > maxV))
                {
                    maxV = v;
                    fsPath = path;
                }
            }

            if (!string.IsNullOrEmpty(fsPath))
                return true;

            // Beta Version auslesen
            int maxBeta = -1;
            foreach (string path in fsDirs.Where(d => Regex.IsMatch(Path.GetFileName(d), @"(.?Beta(?<beta>\d*))$", RegexOptions.IgnoreCase)))
            {
                Match m = Regex.Match(Path.GetFileName(path), @"(.?Beta(?<beta>\d*))$");

                if (string.IsNullOrEmpty(m.Groups["beta"].Value))
                {
                    maxBeta = 0;
                    fsPath = path;
                }
                else if (int.TryParse(m.Groups["beta"].Value, out int beta) && beta > maxBeta)
                {
                    maxBeta = beta;
                    fsPath = path;
                }
            }

            return !string.IsNullOrEmpty(fsPath);
        }

        private void PrintHeader(CommandLineOptions options)
        {
            Console.WriteLine("Analyze options...");
            Console.WriteLine("  configuration file: " + PBEContext.CurrentContext.ConfigFile);
            Console.WriteLine("  default log directory: " + PBEContext.CurrentContext.LogFileDirectory);
            Console.WriteLine("  automatic mode: " + PBEContext.CurrentContext.AutomaticMode);
            Console.WriteLine("  Filter: ");
            foreach (string filter in PBEContext.CurrentContext.TaskFilters)
                Console.WriteLine("    " + filter);

            Console.WriteLine();
            Console.WriteLine("Analyze parameters...");
            SetParam("Weekday", DateTime.Now.ToString("ddd", System.Globalization.CultureInfo.GetCultureInfo("de-DE")));
            SetParam("Date", DateTime.Now.ToString("yyyyMMdd"));
            SetParam("DateTime", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            SetParam("DateTimeText", DateTime.Now.ToString("yyyy-MM-dd HH:mm (dddd)"));
            SetParam("ExportFilePrefix", DateTime.Now.ToString("yyyy-MM-dd") + "_");
            SetParam("Title", "Nachtlauf {DateTimeText}");
            SetParam("Machine", Environment.MachineName);
            SetParam("LocalAppData", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        public ExecutableContainer(CommandLineOptions options)
        {
            this.PrintHeader(options);

            XElement xe = XElement.Load(PBEContext.CurrentContext.ConfigFile);

            var xeFSVersions = xe.Element("FSVersions");
            if (xeFSVersions != null)
            {
                foreach (var xeFSVersion in xeFSVersions.Elements("FSVersion"))
                {
                    string ver = (string)xeFSVersion.Attribute("FS");
                    string dir = (string)xeFSVersion.Attribute("Dir");
                    if (File.Exists(Path.Combine(dir, "FSConsole.exe")))
                    {
                        this.FsVerdict.TryAdd(ver, dir);
                    }
                }
            }

            var xeParams = xe.Element("Params");
            foreach (var xeParam in xeParams.Elements("Param"))
            {
                SetParam(xeParam.Attribute("Name").Value, xeParam.Attribute("Value").Value);
            }

            // parse Logflie
            this.Logfile = this.ParseParameters(xe.Attribute("Logfile").Value);
            if (!Path.IsPathRooted(this.Logfile))
                this.Logfile = Path.Combine(PBEContext.CurrentContext.LogFileDirectory, this.Logfile);
            System.IO.FileInfo fiLog = new System.IO.FileInfo(this.Logfile);
            fiLog.Directory.Create();
            this.Logfile = fiLog.FullName;

            // parse LogfileArchive
            this.Logarchive = this.ParseParameters(xe.Attribute("Logarchive").Value);
            System.IO.FileInfo fiLoga = new System.IO.FileInfo(this.Logarchive);
            fiLoga.Directory.Create();
            this.Logarchive = fiLoga.FullName;

            // XML-Datei für das Protkoll vorbereiten.
            this.XmlFilecontetForLog = System.Net.WebUtility.HtmlEncode(xe.ToString())
                .Replace("\r\n", "<br/>\r\n")
                .Replace("  ", "&nbsp;&nbsp;");

            var xeRootSequence = xe.Element("Sequence");
            rootAction = Executable.Create(xeRootSequence, this, 0);
        }

        private void SetParam(string name, string value)
        {
            string message = string.Empty;
            Parameters[name] = ParseParameters(value);
            if (PBEContext.CurrentContext.Parameters.TryGetValue(name, out var paramValue) && paramValue != value)
            {
                value = paramValue;
                message = $"Override: {Parameters[name]} -> {value}";
                Parameters[name] = ParseParameters(value);
            }
            Console.Write($"  {name} = {value}");
            if (message != string.Empty)
                Console.Write($" ({message})");
            Console.WriteLine();
        }

        public string ParseParameters(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return Regex.Replace(value, "{(?<name>[^}]*)}", (match) =>
            {
                string key = match.Groups["name"].Value;

                string replaced;
                if (this.Parameters.TryGetValue(key, out replaced))
                {
                    return this.ParseParameters(replaced);
                }
                else
                {
                    return match.Value;
                }
            });
        }

        public string CreateExportFileName(string package, string version, string fsVersion = null, string fileNameP = null)
        {
            string fileName;
            if (!string.IsNullOrWhiteSpace(fileNameP))
            {
                fileName = fileNameP;
            }
            else
            {
                fileName = package;
                if (fileName.Equals("NVinity", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "eNVenta";
                }

                fileName = "{ExportFilePrefix}" + fileName + "_" + version;
                if (!string.IsNullOrEmpty(fsVersion) && FSVersion.TryParse(fsVersion, out FSVersion fsver))
                {
                    fileName += " (FS " + fsver.ToDisplayString() + ")";
                }
            }
            return ParseParameters(fileName);
        }

        private object logLocker = new object();
        public static readonly object ConsoleLocker = new object();

        private ConcurrentDictionary<string, object> importLocks = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public object GetImportLocker(string repName)
        {
            return this.importLocks.GetOrAdd(this.ParseParameters(repName), new object());
        }

        public void TransferTempLogFile(string tempFile, Executable exec)
        {
            lock (logLocker)
            {
                using (FileStream fsOut = new FileStream(this.Logfile, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fsOut))
                    {
                        sw.WriteLine(exec.Description);

                        if (File.Exists(tempFile))
                        {
                            using (FileStream fsIn = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                            {
                                using (StreamReader sr = new StreamReader(fsIn))
                                {
                                    while (!sr.EndOfStream)
                                    {
                                        sw.WriteLine("\t" + sr.ReadLine());
                                    }
                                }
                            }
                            File.Delete(tempFile);
                        }
                        sw.WriteLine();
                    }
                }
            }
        }

        internal readonly Dictionary<string, Actions.ImportQueue> ImportQueues = new Dictionary<string, Actions.ImportQueue>(StringComparer.Ordinal);

        public void WriteLog(String line)
        {
            lock (logLocker)
            {
                System.IO.File.AppendAllText(this.Logfile, line + Environment.NewLine);
            }
        }

        public static string MakeShortText(String text, int maxlength = 40)
        {
            // mind. eine Länge von 10
            maxlength = Math.Max(maxlength, 10);

            if (text != null && text.Length > maxlength)
            {
                return (text.Substring(0, maxlength / 2 - 3)) + "..." + text.Substring(text.Length - maxlength / 2 + 3);
            }
            return text;
        }

        private Dictionary<String, String> Parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        private Executable rootAction;

        private void WriteParametersToLog(StreamWriter sw)
        {
            sw.WriteLine("Parameters:");
            foreach (var pair in this.Parameters)
            {
                sw.WriteLine("{0,17}: {1}", pair.Key, pair.Value);
            }
            sw.WriteLine();
        }

        public static readonly object htmlWriteLocker = new object();

        private static string htmlTemplate;
        private const string PLACEHOLDER_Archive = "<!-- Archive -->";

        public void WriteToHtml()
        {
            lock (htmlWriteLocker)
            {
                // HTML-Vorlage auslesen und initialisieren
                if (htmlTemplate == null)
                {
                    htmlTemplate = File.ReadAllText(PBEContext.CurrentContext.LogFileTemplate);
                    htmlTemplate = this.ParseParameters(htmlTemplate);

                    // link auf die alte Logdatei einbauen
                    if (File.Exists(this.Logfile))
                    {
                        String oldLogfile = ParallelHelper.FileReadAllText(this.Logfile);
                        Regex reg = new Regex("[<]!-- Archive: (?<file>[^>]*) --[>]");
                        var match = reg.Match(oldLogfile);
                        if (match != null)
                        {
                            string oldArchive = match.Groups["file"].Value;
                            htmlTemplate = htmlTemplate.Replace(PLACEHOLDER_Archive, PLACEHOLDER_Archive + "&nbsp;&nbsp;&nbsp;<a href=\"" + oldArchive + "\">Previous Logilfe</a>");
                        }
                    }

                    // Aktuelle Achiv-Datei eintragen
                    htmlTemplate = htmlTemplate.Replace(PLACEHOLDER_Archive, "<!-- Archive: " + this.Logarchive + " -->");
                }

                // html-Content aufbereiten
                String htmlContent = htmlTemplate;

                StringBuilder sbParams = new StringBuilder();
                foreach (var pair in this.Parameters)
                {
                    sbParams.AppendFormat("<tr><td class=\"ParamName\">{0}</td><td class=\"ParamValue\">{1}</td></tr>\r\n",
                        HtmlHelper.HtmlEncode(pair.Key), HtmlHelper.HtmlEncode(pair.Value));
                }
                foreach (var pair in FsVerdict.ToArray().OrderBy(p => p.Key))
                {
                    sbParams.AppendFormat("<tr><td class=\"ParamName\">{0}</td><td class=\"ParamValue\">{1}</td></tr>\r\n",
                        HtmlHelper.HtmlEncode("(FS " + pair.Key + ")"), HtmlHelper.HtmlEncode(pair.Value));
                }

                htmlContent = htmlContent.Replace("<!-- ParamsPlaceholder -->", sbParams.ToString());

                htmlContent = htmlContent.Replace("<!-- PbeXmlPlaceHolder -->", this.XmlFilecontetForLog);

                StringBuilder sbTasks = new StringBuilder();
                this.rootAction.WriteToHtml(sbTasks);
                htmlContent = htmlContent.Replace("<!-- TasksPlaceHolder -->", sbTasks.ToString());

                int tries = 3;
                while (tries > 0)
                {
                    try
                    {
                        tries--;
                        File.WriteAllText(this.Logfile, htmlContent);
                        File.WriteAllText(this.Logarchive, htmlContent);
                        tries = 0;
                    }
                    catch { }
                }
            }
        }

        public void Execute()
        {
            if (!PBEContext.CurrentContext.AutomaticMode)
            {
                Console.WriteLine();
                Console.WriteLine("Program was startet manually. For automatic mode e.g. in scheduled tasks");
                Console.WriteLine("specify argument --auto. Example: PBE.exe --auto");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue or ESC to exit.");
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }

            rootAction.ApplyFilter(PBEContext.CurrentContext.TaskFilters);

            this.WriteToHtml();

            // Alle temporären Logfiles löschen
            File.Delete(this.Logfile);

            Console.WriteLine("Execute...");

            // html-Aktualisierung starten
            System.Threading.Thread htmlThread = new System.Threading.Thread(WriteHtmlThread);
            htmlThreadRunning = true;
            htmlThread.Start();

            rootAction.Execute();
            Console.WriteLine("Fertig");

            // html-Aktualisierung beenden
            htmlThreadRunning = false;
            htmlThread.Join();

            PBEContext.CurrentContext.TryCleanupTempLogDirectory();
        }

        private volatile bool htmlThreadRunning = false;

        private void WriteHtmlThread()
        {
            while (htmlThreadRunning)
            {
                this.WriteToHtml();
                // 5 Sekunden warten
                for (int waitcount = 0; waitcount < 1000 && htmlThreadRunning; waitcount++)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            // Am Ende noch einmal schreiben.
            this.WriteToHtml();
        }
    }
}