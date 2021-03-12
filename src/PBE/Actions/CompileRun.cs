using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PBE.Actions
{
    internal class CompileRun : FSConsole
    {
        public String Run { get; private set; }
        public int MaxParallal { get; private set; }

        public CompileRun(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            Run = container.ParseParameters(xe.Attribute("Run").Value);

            this.Arguments = "\\COMPILERUN \"" + this.Run + "\" \\NewProcessPerStep";

            // Ab FS 3.6 werden die MaxTasks unterstützt.
            if (this.FSVer >= new Version(3, 6))
            {
                var xaMaxParallel = xe.Attribute("MaxParallel");
                if (xaMaxParallel != null)
                {
                    int maxParallel = 0;
                    if (int.TryParse(xaMaxParallel.Value, out maxParallel))
                        this.MaxParallal = maxParallel;
                }

                if (this.MaxParallal > 0)
                {
                    this.Arguments += " \\MaxParallel " + this.MaxParallal;
                }
            }

            System.Version fsVer;
            bool fsVerOk = System.Version.TryParse(this.FSVersion, out fsVer);
            if (this.FSVer >= new System.Version(3, 4))
            {
                if ((bool?)xe.Attribute("DeleteCompileDir") != false)
                {
                    // Dieser Parameter steht seit FS 3.4 zur Verfügung
                    this.Arguments += " \\DELETECOMPILEDIR";
                }
            }
            else
            {
                // bis Version 3.3 musste Debug-Code explizit gesetzt sein
                this.Arguments += " \\DebugCode";
            }
        }

        public override string Description
        {
            get
            {
                return "FS " + this.FSVer.ToString(2) + " " + this.Rep + " CompileRun " + this.Run;
            }
        }

        private List<string> labelsCompiled = new List<string>();
        private List<string> labelsSkipped = new List<string>();
        private List<string> labelsFailed = new List<string>();

        protected override void WriteContentToHtml(StringBuilder sb)
        {
            if (this.labelsCompiled.Count > 0 || this.labelsSkipped.Count > 0 || this.labelsFailed.Count > 0)
            {
                sb.Append("<tr><td>");
                sb.Append("<table>");

                if (this.labelsFailed.Count > 0)
                {
                    sb.Append("<tr><td class=\"RunLabel Failed\">Failed:</td><td class=\"RunLabelContent Failed\">");
                    foreach (string label in this.labelsFailed)
                    {
                        sb.Append(HtmlHelper.HtmlEncode(label));
                        sb.Append("<br/>");
                    }
                    sb.Append("</td></tr>");
                }
                if (this.labelsSkipped.Count > 0)
                {
                    sb.Append("<tr><td class=\"RunLabel Skipped\">Skipped:</td><td class=\"RunLabelContent Skppied\">");
                    foreach (string label in this.labelsSkipped)
                    {
                        sb.Append(HtmlHelper.HtmlEncode(label));
                        sb.Append("<br/>");
                    }
                    sb.Append("</td></tr>");
                }
                if (this.labelsCompiled.Count > 0)
                {
                    sb.Append("<tr><td class=\"RunLabel Compiled\">Compiled:</td><td class=\"RunLabelContent Compiled\">");
                    foreach (string label in this.labelsCompiled)
                    {
                        sb.Append(HtmlHelper.HtmlEncode(label));
                        sb.Append("<br/>");
                    }
                    sb.Append("</td></tr>");
                }
                sb.Append("</table>");
                sb.Append("</td></tr>");
            }
            base.WriteContentToHtml(sb);
        }

        protected override void ProcessLogdetails()
        {
            if (String.IsNullOrEmpty(this.LogDetails))
                return;

            using (System.IO.StringReader rd = new System.IO.StringReader(this.LogDetails))
            {
                // 1 = Labels compiled, 2 = Labels skipped, 3 = Labels failed
                int readingMode = 0;

                String line;
                while ((line = rd.ReadLine()) != null)
                {
                    if (line.Length >= 24)
                    {
                        string linepart = line.Substring(24);

                        if (this.FSVer >= new Version(3, 6) && linepart.StartsWith("Package versions compiled:", StringComparison.Ordinal) ||
                            linepart.StartsWith("Labels compiled: ", StringComparison.Ordinal))
                        {
                            readingMode = 1;
                        }
                        else if (this.FSVer >= new Version(3, 6) && linepart.StartsWith("Package versions skipped:", StringComparison.Ordinal) ||
                            linepart.StartsWith(" Labels skipped: ", StringComparison.Ordinal))
                        {
                            readingMode = 2;
                        }
                        else if (this.FSVer >= new Version(3, 6) && linepart.StartsWith("Package versions failed:", StringComparison.Ordinal) ||
                            linepart.StartsWith("  Labels failed: ", StringComparison.Ordinal))
                        {
                            readingMode = 3;
                        }
                        else
                        {
                            if (readingMode > 0)
                            {
                                if (linepart.StartsWith("         ", StringComparison.Ordinal))
                                {
                                    string label = linepart.Substring(9);
                                    switch (readingMode)
                                    {
                                        case 1:
                                            labelsCompiled.Add(label);
                                            break;

                                        case 2:
                                            labelsSkipped.Add(label);
                                            break;

                                        case 3:
                                            labelsFailed.Add(label);
                                            this.TaskFailed = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    readingMode = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}