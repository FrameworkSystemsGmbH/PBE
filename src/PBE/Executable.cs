using PBE.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PBE
{
    public abstract class Executable
    {
        public const string HTMLClass_Pending = "Pending";
        public const string HTMLClass_Running = "Running";
        public const string HTMLClass_Completed = "Completed";
        public const string HTMLClass_CompletedError = "CompletedError";

        public const string HTMLTag_Table = "<table class=\"{0}\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">";
        public const string HTMLTag_Td = "<td class=\"{0}\">";
        public const string HTMLTag_TdSpan = "<td class=\"{0}\" colspan=\"{1}\">";

        public const string HTMLA_ShowHide = "<a id=\"Task{0}Show\" href=\"javascript:showRow('Task{0}')\" class=\"Show\">+</a><a id=\"Task{0}Hide\" href=\"javascript:hideRow('Task{0}')\" class=\"Hide\">-</a>&nbsp;&nbsp;";

        public abstract void ExecuteAction();

        public String Name { get; private set; }
        public int Indent { get; private set; }

        private DateTime? StartTime;
        private DateTime? EndTime;

        public string LogFile { get; protected set; }
        public string LogDetails { get; protected set; }
        public string LogDetailsSub { get; protected set; }
        private string ExceptionDetails = String.Empty;

        public bool TaskFailed { get; protected set; }

        public int Taskid { get; private set; }
        private static int TaskCounter = 0;

        public virtual bool IsComplex { get { return false; } }

        public virtual bool ApplyFilter(HashSet<String> TaskFilters)
        {
            if (TaskFilters.Count > 0)
            {
                if (TaskFilters.Contains(this.Name))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public void Execute()
        {
            StartTime = DateTime.Now;

            int cLeft = 0;
            int cTop = 0;

            lock (ExecutableContainer.ConsoleLocker)
            {
                string indentString = new string(' ', this.Indent);
                Console.Write(indentString + this.Description + " " + StartTime.Value.ToString("HH:mm") + " - ");
                cLeft = Console.CursorLeft;
                cTop = Console.CursorTop;
                Console.WriteLine("...");
            }

            StartTime = DateTime.Now;
            Stopwatch stop = new Stopwatch();
            stop.Start();
            try
            {
                this.ExecuteAction();
            }
            catch (Exception ex)
            {
                ExceptionDetails = ex.ToString();
                this.TaskFailed = true;
            }
            stop.Stop();
            EndTime = DateTime.Now;

            lock (ExecutableContainer.ConsoleLocker)
            {
                int cLeftKeep = Console.CursorLeft;
                int cTopKeep = Console.CursorTop;

                Console.SetCursorPosition(cLeft, cTop);
                Console.Write(EndTime.Value.ToString("HH:mm") + " (" + stop.ElapsedMilliseconds / 60000 + "min)");
                Console.SetCursorPosition(cLeftKeep, cTopKeep);
            }

            // Logfile einlesen, löschen und verarbeiten
            if (!String.IsNullOrEmpty(this.LogFile))
            {
                // lock wird benötigt, weil evtl. gerade die HTML-Datei geschrieben wird.
                lock (ExecutableContainer.htmlWriteLocker)
                {
                    int tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            tries--;
                            this.LogDetails = ParallelHelper.FileReadAllText(this.LogFile);
                            tries = 0;
                        }
                        catch { System.Threading.Thread.Sleep(50); }
                    }
                    tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            tries--;
                            File.Delete(this.LogFile);
                            tries = 0;
                        }
                        catch { System.Threading.Thread.Sleep(50); }
                    }

                    this.ProcessLogdetails();
                }
            }
        }

        /// <summary>
        /// Diese Methode wird am Ende der Verarbeitung aufgerufen, um Informationen aus dem Logfile zu siehen.
        /// Sie muss in speziellen Task überschrieben werden.
        /// </summary>
        protected virtual void ProcessLogdetails()
        {
        }

        public void WriteToHtml(StringBuilder sb)
        {
            string tableClass = HTMLClass_Pending;
            if (this.EndTime.HasValue)
            {
                if (this.TaskFailed)
                {
                    tableClass = HTMLClass_CompletedError;
                }
                else if (!this.IsComplex)
                {
                    tableClass = HTMLClass_Completed;
                }
            }
            else if (this.StartTime.HasValue)
            {
                tableClass = HTMLClass_Running;
            }

            sb.AppendFormat(HTMLTag_Td, "SubTask");
            sb.AppendFormat(HTMLTag_Table, "Task " + tableClass);
            sb.Append("<tr>");
            sb.AppendFormat(HTMLTag_TdSpan, "TaskHeader " + tableClass, this.GetHtmlColspan());

            // Aktuellen Stand des Logfiles einlesen
            if (!String.IsNullOrEmpty(this.LogFile))
            {
                {
                    int tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            tries--;
                            this.LogDetails = ParallelHelper.FileReadAllText(this.LogFile);
                            tries = 0;
                        }
                        catch { }
                    }
                }

                // Die Dateien der Unterprozesse Einlesen.
                this.LogDetailsSub = String.Empty;
                FileInfo fi = new FileInfo(this.LogFile);
                try
                {
                    foreach (var file in fi.Directory.EnumerateFiles(Path.GetFileNameWithoutExtension(fi.Name) + "_*.TRACE"))
                    {
                        int tries = 3;
                        while (tries > 0)
                        {
                            try
                            {
                                tries--;
                                string subText = ParallelHelper.FileReadAllText(file.FullName);
                                tries = 0;
                                this.LogDetailsSub += Environment.NewLine +
                                    "=================== " + file.Name + " ===================" +
                                    Environment.NewLine +
                                    subText;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            string detailsText = String.Empty;
            if (!String.IsNullOrEmpty(this.ExceptionDetails))
            {
                detailsText = ExceptionDetails;
            }
            if (!String.IsNullOrEmpty(this.LogDetails))
            {
                if (!String.IsNullOrEmpty(detailsText))
                {
                    detailsText += "\r\n************************************************\r\n";
                }
                detailsText += this.LogDetails;
            }
            detailsText += this.LogDetailsSub;

            if (!String.IsNullOrEmpty(detailsText))
            {
                sb.AppendFormat(HTMLA_ShowHide, this.Taskid);
            }
            sb.Append(this.Description);

            if (this.EndTime.HasValue)
            {
                if (this.TaskFailed)
                {
                    sb.Append(" <span class=\"TaskState\">");
                    sb.Append("Completed with ERROR");
                    sb.Append("</span>");
                }
                else
                {
                    // COmpleted wid nicht extra ausgegeben.
                    // sb.Append("Completed");
                }
            }
            else if (this.StartTime.HasValue)
            {
                sb.Append(" <span class=\"TaskState\">");
                sb.Append("Running...");
                sb.Append("</span>");
            }
            sb.Append(" <span class=\"TaskDuration\">");
            if (this.EndTime.HasValue)
            {
                //sb.Append(/*"Duration: " + */TimeSpan.FromSeconds((long)(EndTime.Value - StartTime.Value).TotalSeconds).ToString("g")
                //    + " (" + StartTime.Value.ToString("HH:mm") + " - " + EndTime.Value.ToString("HH:mm") + ")");
                sb.Append(StartTime.Value.ToString("HH:mm") + " - " + EndTime.Value.ToString("HH:mm") +
                    " (" + TimeSpan.FromSeconds((long)(EndTime.Value - StartTime.Value).TotalSeconds).ToString("g") + ")");
            }
            else if (this.StartTime.HasValue)
            {
                sb.Append("since " + StartTime.Value.ToString("HH:mm"));
            }
            sb.Append("</span>");

            sb.Append("</td>");
            sb.Append("</tr>");

            this.WriteContentToHtml(sb);

            if (!String.IsNullOrEmpty(detailsText))
            {
                sb.AppendFormat("<tr id=\"Task{0}\" class=\"TaskDetails\"><td><pre class=\"Logdetails\">{1}</pre></td></tr>",
                    this.Taskid,
                    detailsText);
            }

            sb.Append("</table>");
            sb.Append("</td>");
        }

        protected virtual void WriteContentToHtml(StringBuilder sb)
        {
        }

        public virtual int GetHtmlColspan()
        {
            return 1;
        }

        public Executable(XElement xe, ExecutableContainer container, int indent)
        {
            this.Taskid = System.Threading.Interlocked.Increment(ref TaskCounter);

            this.Indent = indent;
            var attrName = xe.Attribute("Name");
            if (attrName != null)
            {
                this.Name = attrName.Value;
            }
            this.Container = container;
        }

        public virtual String Description
        {
            get { return String.IsNullOrEmpty(this.Name) ? this.GetType().Name : this.Name; }
        }

        public ExecutableContainer Container { get; private set; }

        public static Executable Create(XElement xe, ExecutableContainer container, int indent)
        {
            switch (xe.Name.LocalName)
            {
                case "Sequence":
                    return new ExecutableSequence(xe, container, indent);

                case "Condition":
                    return new ExecutableCondition(xe, container, indent);

                case "Parallel":
                    return new ExecutableParallel(xe, container, indent);

                case "FSConsole":
                    return new Actions.FSConsole(xe, container, indent);

                case "MD":
                    return new Actions.MakeDir(xe, container, indent);

                case "RD":
                    return new Actions.RemoveDir(xe, container, indent);

                case "CompileRun":
                    return new Actions.CompileRun(xe, container, indent);

                case "Export":
                    return new Actions.Export(xe, container, indent);

                case Actions.ApprovedExport.TypeName:
                    return new Actions.ApprovedExport(xe, container, indent);

                case "Import":
                    return new Actions.Import(xe, container, indent);

                case Actions.ApprovedImport.TypeName:
                    return new Actions.ApprovedImport(xe, container, indent);

                case Actions.ImportQueue.TypeName:
                    return new Actions.ImportQueue(xe, container, indent);

                case "Publish":
                    return new Actions.Publish(xe, container, indent);

                case "Publish2Go":
                    return new Actions.Publish2Go(xe, container, indent);

                case "ExportDoc":
                    return new Actions.ExportDoc(xe, container, indent);

                case "Batch":
                    return new Actions.Batch(xe, container, indent);
            }

            return null;
        }

        /// <summary>
        /// Liest die Argumente ein.
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        protected static string ParseArgs(XElement xe, ExecutableContainer container)
        {
            string args = null;

            var attrArgs = xe.Attribute("Args");
            if (attrArgs != null)
            {
                args = container.ParseParameters(attrArgs.Value);
            }

            var xeArgs = xe.Element("Args");
            if (xeArgs != null)
            {
                var elementArgs = FSUtils.EscapeCommandLineArgs(xeArgs.Elements("Arg")
                    .Select(xeArg => container.ParseParameters(xeArg.Value)));
                if (!string.IsNullOrWhiteSpace(elementArgs))
                {
                    if (!string.IsNullOrWhiteSpace(args))
                        args += " " + elementArgs;
                    else
                        args = elementArgs;
                }
            }

            return args;
        }
    }
}