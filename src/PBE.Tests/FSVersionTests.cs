using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PBE.Utils;
using System;
using static FluentAssertions.FluentActions;

namespace PBE.Tests
{
    [TestClass]
    public class FSVersionTests
    {
        [TestMethod]
        public void CreateInstance_Test()
        {
            Invoking(() => new FSVersion("")).Should().Throw<ArgumentException>();
            Invoking(() => new FSVersion(null)).Should().Throw<ArgumentException>();
            Invoking(() => new FSVersion("4.5.x")).Should().Throw<ArgumentException>();
            Invoking(() => new FSVersion("x.5x6")).Should().Throw<ArgumentException>();

            FSVersion v = new FSVersion("4.5.0.0");

            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeFalse();

            v = new FSVersion("4.5");

            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeFalse();

            v = new FSVersion("4.5.pre");

            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeTrue();
        }

        [TestMethod]
        public void ToString_Amount_Test()
        {
            FSVersion v = new FSVersion("4.5.0.0");

            Invoking(() => v.ToString(-1)).Should().Throw<ArgumentException>();
            v.ToString(0).Should().Be(string.Empty);
            v.ToString(1).Should().Be("4");
            v.ToString(2).Should().Be("4.5");
            v.ToString(3).Should().Be("4.5.0");
            v.ToString(4).Should().Be("4.5.0.0");
            Invoking(() => v.ToString(5)).Should().Throw<ArgumentException>();

            v = new FSVersion("4.5");

            v.ToString(0).Should().Be(string.Empty);
            v.ToString(1).Should().Be("4");
            v.ToString(2).Should().Be("4.5");
            v.ToString(3).Should().Be("4.5.0");
            v.ToString(4).Should().Be("4.5.0.0");

            v = new FSVersion("4.5.pre");

            v.ToString(0).Should().Be("4.5.pre");
            v.ToString(1).Should().Be("4.5.pre");
            v.ToString(2).Should().Be("4.5.pre");
            v.ToString(3).Should().Be("4.5.pre");
            v.ToString(4).Should().Be("4.5.pre");
        }

        [TestMethod]
        public void ToDisplayString_Test()
        {
            FSVersion v = new FSVersion("4.5");
            v.ToDisplayString().Should().Be("4.5");

            v = new FSVersion("4.5.0.0");
            v.ToDisplayString().Should().Be("4.5");

            v = new FSVersion("4.5.0.1");
            v.ToDisplayString().Should().Be("4.5.0.1");

            v = new FSVersion("4.5.1.0");
            v.ToDisplayString().Should().Be("4.5.1");

            v = new FSVersion("4.5.1.1");
            v.ToDisplayString().Should().Be("4.5.1.1");

            v = new FSVersion("4.5.pre");
            v.ToDisplayString().Should().Be("4.5 Preview");
        }

        [TestMethod]
        public void ToString_Test()
        {
            FSVersion v = new FSVersion("4.5");
            v.ToString().Should().Be("4.5");

            v = new FSVersion("4.5.0.0");
            v.ToString().Should().Be("4.5");

            v = new FSVersion("4.5.0.1");
            v.ToString().Should().Be("4.5.0.1");

            v = new FSVersion("4.5.1.0");
            v.ToString().Should().Be("4.5.1");

            v = new FSVersion("4.5.1.1");
            v.ToString().Should().Be("4.5.1.1");

            v = new FSVersion("4.5.pre");
            v.ToString().Should().Be("4.5.pre");
        }

        [TestMethod]
        public void TryParse_Test()
        {
            bool success = FSVersion.TryParse("4.5.x", out FSVersion v);
            v.Should().BeNull();
            success.Should().BeFalse();

            success = FSVersion.TryParse("", out v);
            v.Should().BeNull();
            success.Should().BeFalse();

            success = FSVersion.TryParse(null, out v);
            v.Should().BeNull();
            success.Should().BeFalse();

            success = FSVersion.TryParse("4.5.0.0", out v);
            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeFalse();
            success.Should().BeTrue();

            success = FSVersion.TryParse("4.5", out v);

            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeFalse();
            success.Should().BeTrue();

            success = FSVersion.TryParse("4.5.pre", out v);

            v.Should().NotBeNull();
            v.Version.Should().Be(new Version(4, 5, 0, 0));
            v.IsPreview.Should().BeTrue();
            success.Should().BeTrue();
        }
    }
}