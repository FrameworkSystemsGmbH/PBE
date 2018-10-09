using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PBE
{
    /// <summary>
    /// Enthaltene Aktionen werden parallel ausgeführt
    /// </summary>
    internal class ExecutableParallel : Executable
    {
        public ExecutableParallel(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var attrMaxTasks = xe.Attribute("MaxTasks");
            if (attrMaxTasks != null)
            {
                int maxTasks;
                if (Int32.TryParse(attrMaxTasks.Value, out maxTasks))
                {
                    this.MaxTasks = maxTasks;
                }
            }

            this.ActionList = xe.Elements().Select(xeSub => Executable.Create(xeSub, container, indent + 1)).ToList();
        }

        private int? MaxTasks;

        public override bool IsComplex { get { return true; } }

        private List<Executable> ActionList;

        public override string Description
        {
            get
            {
                string desc = "Parallel";
                if (this.MaxTasks.HasValue)
                {
                    desc += " [" + this.MaxTasks + "]";
                }
                if (!string.IsNullOrEmpty(this.Name))
                {
                    desc += " " + this.Name;
                }
                return desc;
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

        protected override void WriteContentToHtml(System.Text.StringBuilder sb)
        {
            int colCount = 0;
            sb.Append("<tr>");
            foreach (var exec in this.ActionList)
            {
                if (exec != null)
                {
                    colCount++;
                    // Zeilenumbruch, wenn MaxTasks definiert wurde
                    // maximal 2 Spalten nebenen
                    if (MaxTasks.HasValue)
                    {
                        if (colCount > 2)
                        {
                            colCount = 1;
                            sb.AppendLine("</tr>");
                            sb.Append("<tr>");
                        }
                    }

                    exec.WriteToHtml(sb);
                }
            }
            sb.AppendLine("</tr>");
        }

        public override int GetHtmlColspan()
        {
            if (this.MaxTasks.HasValue)
            {
                return 2;
            }
            else
            {
                return this.ActionList.Count;
            }
        }

        public override void ExecuteAction()
        {
            // Alle Actions, die nicht ImportQueue sind
            var ActionArray = ActionList
                .Where(a => !(a is Actions.ImportQueue))
                .Select<Executable, Action>(a => a.Execute)
                .ToArray();

            var ImportQueues = ActionList
                .Where(a => a is Actions.ImportQueue)
                .Select(a => a as Actions.ImportQueue)
                .ToArray();

            // zuerst Import-Queue Tasks starten
            if (ImportQueues.Length > 0)
            {
                ImportQueues
                    .Select<Executable, Task>(a => Task.Factory.StartNew(a.Execute))
                    .ToArray();
            }

            // Dann die anderen Tasks (bis zum Ende) ausführen.
            if (MaxTasks.HasValue)
            {
                ParallelOptions options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = MaxTasks.Value
                };
                Parallel.Invoke(options, ActionArray);
            }
            else
            {
                Parallel.Invoke(ActionArray);
            }

            // Wenn alle anderen Tasks fertig sind, dann noch den ImportQueues Bescheid geben
            // und auf deren Ende warten.
            if (ImportQueues.Length > 0)
            {
                foreach (var importQueue in ImportQueues)
                {
                    importQueue.Join();
                }
            }
        }
    }
}