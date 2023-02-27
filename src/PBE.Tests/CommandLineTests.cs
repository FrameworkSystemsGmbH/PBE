using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace PBE.Tests
{
    [TestClass]
    public class CommandLineTests
    {
        [TestMethod]
        public void Auto_IfNotDefined_ShouldBeFalse()
        {
            var args = new string[] { };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.AutomaticMode.Should().BeFalse();
        }

        [TestMethod]
        public void Auto_IfDefined_ShouldBeTrue()
        {
            var args = new string[] { "--auto" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.AutomaticMode.Should().BeTrue();
        }

        [TestMethod]
        public void Auto_IfDefinedShort_ShouldBeTrue()
        {
            var args = new string[] { "-a" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.AutomaticMode.Should().BeTrue();
        }

        [TestMethod]
        public void Config_IsNotDefined_ShouldBePbeXml()
        {
            var args = new string[] { };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.ConfigFile.Should().Be(Path.Combine(Environment.CurrentDirectory, "PBE.xml"));
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Config_IsDefinedInvalidPath_ShouldThrowException()
        {
            var args = new string[] { "--config", @"C:\InvalidPath\InvalidPbe.xml" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);
        }

        [TestMethod]
        public void Param_OneParameter()
        {
            var args = new string[] { "--param", "param1=value1" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param1", "value1")
                });
        }

        [TestMethod]
        public void Param_MultipleParameters_SeparatedWithSemikolon()
        {
            var args = new string[] { "--param", "param1=value1;param2=value2;param3=value3" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param1", "value1"),
                    new KeyValuePair<string, string>("param2", "value2"),
                    new KeyValuePair<string, string>("param3", "value3"),
                });
        }

        [TestMethod]
        public void Param_WithSpaces()
        {
            var args = new string[] { "--param", "param 1=value 1;param 2=value 2" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param 1", "value 1"),
                    new KeyValuePair<string, string>("param 2", "value 2"),
                });
        }

        [TestMethod]
        public void ParamValueWithSeparator()
        {
            // Das Semikolon wird durch doppelte Semikolon escaped.
            // Im Values sollte ein einfaches Semikolon ankommen.
            var args = new string[] { "--param", "param1=value;;1;param2=value;;2" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param1", "value;1"),
                    new KeyValuePair<string, string>("param2", "value;2"),
                });
        }

        [TestMethod]
        public void Param_ValueShouldNotBeTrimmed()
        {
            var args = new string[] { "--param", "param1=  value 1 ;param2= value 2 " };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param1", "  value 1 "),
                    new KeyValuePair<string, string>("param2", " value 2 "),
                });
        }

        [TestMethod]
        public void Param_NameShouldBeTrimmed()
        {
            var args = new string[] { "--param", "  param1  =value1;  param2 =value2" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("param1", "value1"),
                    new KeyValuePair<string, string>("param2", "value2"),
                });
        }

        [TestMethod]
        public void Param_ValueIsFilePath()
        {
            var args = new string[] { "--param", @"path=C:\Folder\File.txt;param2=value2" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("path", @"C:\Folder\File.txt"),
                    new KeyValuePair<string, string>("param2", "value2"),
                });
        }

        [TestMethod]
        public void Param_ValueContainsStar()
        {
            // Intern wird der Stern als Platzhalter für das Escapen verwendet.
            // Er sollte korrekt verareitet werden.
            var args = new string[] { "--param", @"path=C:\Folder\*.txt;param2=*" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.Parameters.Should().BeEquivalentTo(new[] {
                    new KeyValuePair<string, string>("path", @"C:\Folder\*.txt"),
                    new KeyValuePair<string, string>("param2", "*"),
                });
        }

        [TestMethod]
        public void Filter_IfNotSet_TaskFiltersShouldBeEmpty()
        {
            var args = new string[] { };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.TaskFilters.Should().BeEmpty();
        }

        [TestMethod]
        public void Filter_IfSpecified_TaskFiltersShouldBeFilled()
        {
            var args = new string[] { "--filter", "Task1" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.TaskFilters.Should().BeEquivalentTo("Task1");
        }

        [TestMethod]
        public void Filter_Multiple_SplittedWithColon()
        {
            var args = new string[] { "--filter", "Task1:Task2:Task3" };

            CommandLineProcessor.CommandLineParser.ParseOptions(args, PBEContext.Create);

            PBEContext.CurrentContext.TaskFilters.Should().BeEquivalentTo("Task1", "Task2", "Task3");
        }
    }
}