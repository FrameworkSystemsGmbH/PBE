﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace PBE
{
    internal class ExecutableCondition : ExecutableSequence
    {
        // In diesem Dictionary können Funktionen registriert werden, die die Bedingung
        // bei der Abarbeitung dann aufruft. In der Bedingung in der PBE-Konfiguration ist
        // der hier angegebene Schlüssel mit dem Prefix '#' zu versehen. Groß-/Kleinschreibung
        // spielt keine Rolle
        private IDictionary<string, Func<string, string>> FuncCache = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["EXISTS"] = ExistsMethod,
            ["CONTAINS_FILES"] = ContainsFilesMethod
        };

        public ExecutableCondition(XElement xe, ExecutableContainer container, int indent)
            : base(xe, container, indent)
        {
            var xaValue = xe.Attribute("Value");
            if (xaValue != null)
            {
                this.Value = container.ParseParameters(xaValue.Value);
                this.ValueOrg = this.Value;
                this.IsMethodCall = this.Value.StartsWith("#");
            }

            var xeEquales = xe.Attribute("Equals");
            if (xeEquales != null)
            {
                this.EqualsValue = container.ParseParameters(xeEquales.Value);
            }

            // Wenn die Condition nicht stimmt, dann die ActionList leeren, damit auch nichts ausgeführt wird.
            // Bei Methoden (starten mit '#') gilt das nicht, da der Auswertungszeitpunkt erst zur Ausführung
            // der Aktion ist.
            if (this.Value != this.EqualsValue && !this.IsMethodCall)
            {
                this.ActionList.Clear();
            }
        }

        private string ValueOrg { get; set; }
        public string Value { get; private set; }
        public bool IsMethodCall { get; private set; }
        public string EqualsValue { get; private set; }
        public bool ShouldExecute { get { return this.Value == this.EqualsValue || this.IsMethodCall; } }

        public override string Description
        {
            get
            {
                string conditionName = this.Name;
                if (string.IsNullOrWhiteSpace(conditionName))
                    conditionName = "[unnamed]";
                string info = this.ShouldExecute ? "Execute" : "Skip";
                if (this.IsMethodCall)
                    info += ", late evaluation)";
                string value = this.Value;
                if (this.IsMethodCall)
                    value = $"{this.ValueOrg} -> {value}";
                return $"Condition {conditionName} \"{value}\" == \"{this.EqualsValue}\" ({info})";
            }
        }

        public override void ExecuteAction()
        {
            int startPos = this.Value.IndexOf("(");
            if (this.IsMethodCall && startPos > 0)
            {
                this.Value = CallMethod(this.Value.Substring(1, startPos - 1)
                    , this.Value.Substring(startPos + 1, this.Value.Length - startPos - 2));
            }
            if (this.Value == this.EqualsValue)
            {
                base.ExecuteAction();
            }
        }

        /// <summary>
        /// führt die angegebene Funktion mit dem angegebenen Parameter aus, sofern
        /// diese im Functionscache registriert ist.
        /// </summary>
        /// <param name="function">Funktion, die aufgerufen werden soll.</param>
        /// <param name="value">Übergabeparameter, der an die Funktion übergeben werden soll.</param>
        /// <returns>Das Ergebnis der Funktion als string</returns>
        private string CallMethod(string function, string value)
        {
            if (!FuncCache.ContainsKey(function))
                throw new ArgumentException($"Scriptmethod '{function}' does not exist, check yout syntax.");
            return FuncCache[function](value);
        }

        /// <summary>
        /// Prüft ob der angegebene Datei-/Verzeichnis-Pfad existiert.
        /// </summary>
        /// <param name="value">Pfad zu einer Datei / einem Verzeichnis.</param>
        /// <returns>"True" wenn die Datei / das Verzeichnis existiert, ansonsten "False".</returns>
        private static string ExistsMethod(string value)
        {
            return (File.Exists(value) || Directory.Exists(value)).ToString();
        }

        /// <summary>
        /// Prüft ob das angegebene Verzeichnis nicht leer ist.
        /// </summary>
        /// <param name="value">Pfad zu einem Verzeichnis.</param>
        /// <returns>"True" wenn das Verzeichnis Dateien enthält. "False" wenn das Verzeichnis leer ist, oder nicht existiert.</returns>
        private static string ContainsFilesMethod(string value)
        {
            return (Directory.Exists(value) 
                && Directory.EnumerateFileSystemEntries(value).Any()).ToString();
        }
    }
}