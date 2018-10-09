using System.Xml.Linq;

namespace PBE
{
    internal class ExecutableCondition : ExecutableSequence
    {
        public ExecutableCondition(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var xaValue = xe.Attribute("Value");
            if (xaValue != null)
            {
                this.Value = container.ParseParameters(xaValue.Value);
            }

            var xeEquales = xe.Attribute("Equals");
            if (xeEquales != null)
            {
                this.EqualsValue = container.ParseParameters(xeEquales.Value);
            }

            // Wenn die Condition nicht stimmt, dann die ActionList leeren, damit auch nichts ausgeführt wird.
            if (this.Value != this.EqualsValue)
            {
                this.ActionList.Clear();
            }
        }

        public string Value { get; private set; }
        public string EqualsValue { get; private set; }
        public bool ShouldExecute { get { return this.Value == this.EqualsValue; } }

        public override string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    return "Condition " + this.Name + " \"" + this.Value + "\"==\"" + this.EqualsValue + "\" " + (ShouldExecute ? "(Execute)" : "(Skip)");
                }
                return "Condition \"" + this.Value + "\"==\"" + this.EqualsValue + "\" " + (ShouldExecute ? "(Execute)" : "(Skip)");
            }
        }

        public override void ExecuteAction()
        {
            if (Value == EqualsValue)
            {
                base.ExecuteAction();
            }
        }
    }
}