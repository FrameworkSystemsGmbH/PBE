using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PBE
{
    /// <summary>
    /// Enthaltene Aktionen werden der Reihe nach ausgeführt
    /// </summary>
    internal class ExecutableSequence : Executable
    {
        public ExecutableSequence(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            this.ActionList = xe.Elements().Select(xeSub => Executable.Create(xeSub, container, indent + 1)).ToList();
        }

        public override bool IsComplex { get { return true; } }

        protected List<Executable> ActionList;

        public override string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    return "Sequence " + this.Name;
                }
                return "Sequence";
            }
        }

        protected override void WriteContentToHtml(System.Text.StringBuilder sb)
        {
            foreach (var exec in this.ActionList)
            {
                if (exec != null)
                {
                    sb.Append("<tr>");
                    exec.WriteToHtml(sb);
                    sb.AppendLine("</tr>");
                }
            }
        }

        public override bool ApplyFilter(HashSet<String> TaskFilters)
        {
            // wenn der Filter zum Task passt, dann OK. Alle SubTasks sollen ausgeführt werden.
            if (base.ApplyFilter(TaskFilters))
            {
                return true;
            }

            // alle SubTasks entfernen bei denen der Filter nicht passt
            this.ActionList.RemoveAll((x) => !x.ApplyFilter(TaskFilters));

            // wenn keine SubTasks übrig bleiben, dann ist auch dieser Task invalid.
            return this.ActionList.Count > 0;
        }

        public override void ExecuteAction()
        {
            foreach (var exec in this.ActionList)
            {
                if (exec != null)
                {
                    exec.Execute();
                }
            }
        }
    }
}