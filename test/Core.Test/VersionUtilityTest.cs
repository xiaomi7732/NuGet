using System;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuGet.Test {
    [TestClass]
    public class VersionUtilityTest {
        [TestMethod]
        public void ParseFrameworkNameNormalizesSupportedNetFrameworkNames() {
            // Arrange
            var knownNameFormats = new[] { ".net", ".netframework", "net", "netframework" };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames) {
                Assert.AreEqual(".NETFramework", frameworkName.Identifier);
                Assert.AreEqual(defaultVersion, frameworkName.Version);
            }
        }

        [TestMethod]
        public void ParseFrameworkNameNormalizesSupportedNetMicroFrameworkNames() {
            // Arrange
            var knownNameFormats = new[] { "netmf4.1", ".NETMicroFramework4.1" };
            Version version41 = new Version("4.1");

            // Act
            var frameworkNames = knownNameFormats.Select(fmt => VersionUtility.ParseFrameworkName(fmt));

            // Assert
            foreach (var frameworkName in frameworkNames) {
                Assert.AreEqual(".NETMicroFramework", frameworkName.Identifier);
                Assert.AreEqual(version41, frameworkName.Version);
            }
        }

        [TestMethod]
        public void ParseFrameworkNameNormalizesSupportedSilverlightNames() {
            // Arrange
            var knownNameFormats = new[] { "sl", "SL", "SilVerLight", "Silverlight", "Silverlight " };
            Version defaultVersion = new Version("0.0");

            // Act
            var frameworkNames = knownNameFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames) {
                Assert.AreEqual("Silverlight", frameworkName.Identifier);
                Assert.AreEqual(defaultVersion, frameworkName.Version);
            }
        }

        [TestMethod]
        public void ParseFrameworkNameReturnsUnsupportedFrameworkNameIfUnrecognized() {
            // Arrange
            Version version20 = new Version("2.0");

            // Act
            var frameworkName1 = VersionUtility.ParseFrameworkName("NETCF20");
            var frameworkName2 = VersionUtility.ParseFrameworkName("NET40ClientProfile");
            var frameworkName3 = VersionUtility.ParseFrameworkName("NET40Foo");

            // Assert
            Assert.AreEqual("Unsupported", frameworkName1.Identifier);
            Assert.AreEqual("Unsupported", frameworkName2.Identifier);
            Assert.AreEqual("Unsupported", frameworkName3.Identifier);
        }

        [TestMethod]
        public void ParseFrameworkNameUsesNetFrameworkIfOnlyVersionSpecified() {
            // Arrange
            Version version20 = new Version("2.0");

            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("20");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(version20, frameworkName.Version);
        }

        [TestMethod]
        public void ParseFrameworkNameVersionFormats() {
            // Arrange
            var versionFormats = new[] { "4.0", "40", "4" };
            Version version40 = new Version("4.0");

            // Act
            var frameworkNames = versionFormats.Select(VersionUtility.ParseFrameworkName);

            // Assert
            foreach (var frameworkName in frameworkNames) {
                Assert.AreEqual(".NETFramework", frameworkName.Identifier);
                Assert.AreEqual(version40, frameworkName.Version);
            }
        }

        [TestMethod]
        public void ParseFrameworkNameVersionIntegerLongerThan4CharsTrimsExccess() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("NET41235");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.1.2.3"), frameworkName.Version);
        }

        [TestMethod]
        public void ParseFrameworkNameInvalidVersionFormatUsesDefaultVersion() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("NET4.1.4.5.5");

            // Assert
            Assert.AreEqual("Unsupported", frameworkName.Identifier);
        }

        [TestMethod]
        public void ParseFrameworkNameWithProfile() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-client");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.0"), frameworkName.Version);
            Assert.AreEqual("Client", frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithUnknownProfileUsesProfileAsIs() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-other");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.0"), frameworkName.Version);
            Assert.AreEqual("other", frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithFullProfileNoamlizesToEmptyProfile() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net40-full");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.0"), frameworkName.Version);
            Assert.AreEqual(String.Empty, frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithWPProfileGetNormalizedToWindowsPhone() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("sl4-wp");

            // Assert
            Assert.AreEqual("Silverlight", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.0"), frameworkName.Version);
            Assert.AreEqual("WindowsPhone", frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithCFProfileGetNormalizedToCompactFramework() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("net20-cf");

            // Assert
            Assert.AreEqual(".NETFramework", frameworkName.Identifier);
            Assert.AreEqual(new Version("2.0"), frameworkName.Version);
            Assert.AreEqual("CompactFramework", frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithEmptyProfile() {
            // Act
            var frameworkName = VersionUtility.ParseFrameworkName("sl4-");

            // Assert
            Assert.AreEqual("Silverlight", frameworkName.Identifier);
            Assert.AreEqual(new Version("4.0"), frameworkName.Version);
            Assert.AreEqual(String.Empty, frameworkName.Profile);
        }

        [TestMethod]
        public void ParseFrameworkNameWithInvalidFrameworkNameThrows() {
            // Act
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("-"), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("-client"), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName(""), "frameworkName", "Framework name is missing.");
            ExceptionAssert.ThrowsArgumentException(() => VersionUtility.ParseFrameworkName("---"), "frameworkName", "Invalid framework name format. Expected {framework}{version}-{profile}.");
        }

        [TestMethod]
        public void ParseFrameworkFolderName() {
            // foo.dll
            // sub\foo.dll -> Unsupported since we can't tell if this was meant to be a framework name or not
            // {FrameworkName}{Version}\foo.dll
            // {FrameworkName}{Version}\sub1\foo.dll
            // {FrameworkName}{Version}\sub1\sub2\foo.dll
            var f1 = VersionUtility.ParseFrameworkFolderName(@"foo.dll");
            var f2 = VersionUtility.ParseFrameworkFolderName(@"sub\foo.dll");
            var f3 = VersionUtility.ParseFrameworkFolderName(@"SL4\foo.dll");
            var f4 = VersionUtility.ParseFrameworkFolderName(@"SL3\sub1\foo.dll");
            var f5 = VersionUtility.ParseFrameworkFolderName(@"SL20\sub1\sub2\foo.dll");
            var f6 = VersionUtility.ParseFrameworkFolderName(@"net\foo.dll");

            Assert.IsNull(f1);
            Assert.AreEqual("Unsupported", f2.Identifier);
            Assert.AreEqual("Silverlight", f3.Identifier);
            Assert.AreEqual(new Version("4.0"), f3.Version);
            Assert.AreEqual("Silverlight", f4.Identifier);
            Assert.AreEqual(new Version("3.0"), f4.Version);
            Assert.AreEqual("Silverlight", f5.Identifier);
            Assert.AreEqual(new Version("2.0"), f5.Version);
            Assert.AreEqual(".NETFramework", f6.Identifier);
            Assert.AreEqual(new Version(), f6.Version);
        }

        [TestMethod]
        public void GetFrameworkStringFromFrameworkName() {
            // Arrange
            var net40 = new FrameworkName(".NETFramework", new Version(4, 0));
            var net40Client = new FrameworkName(".NETFramework", new Version(4, 0), "Client");
            var sl3 = new FrameworkName("Silverlight", new Version(3, 0));
            var sl4 = new FrameworkName("Silverlight", new Version(4, 0));
            var wp7 = new FrameworkName("Silverlight", new Version(4, 0), "WindowsPhone");
            var wp7Mango = new FrameworkName("Silverlight", new Version(4, 0), "WindowsPhone71");
            var netMicro41 = new FrameworkName(".NETMicroFramework", new Version(4, 1));

            // Act
            string net40Value = VersionUtility.GetFrameworkString(net40);
            string net40ClientValue = VersionUtility.GetFrameworkString(net40Client);
            string sl3Value = VersionUtility.GetFrameworkString(sl3);
            string sl4Value = VersionUtility.GetFrameworkString(sl4);
            string wp7Value = VersionUtility.GetFrameworkString(wp7);
            string wp7MangoValue = VersionUtility.GetFrameworkString(wp7Mango);
            string netMicro41Value = VersionUtility.GetFrameworkString(netMicro41);

            // Assert
            Assert.AreEqual(".NETFramework4.0", net40Value);
            Assert.AreEqual(".NETFramework4.0-Client", net40ClientValue);
            Assert.AreEqual("Silverlight3.0", sl3Value);
            Assert.AreEqual("Silverlight4.0", sl4Value);
            Assert.AreEqual("Silverlight4.0-WindowsPhone", wp7Value);
            Assert.AreEqual("Silverlight4.0-WindowsPhone71", wp7MangoValue);
            Assert.AreEqual(".NETMicroFramework4.1", netMicro41Value);
        }

        [TestMethod]
        public void ParseVersionSpecWithNullThrows() {
            // Act & Assert
            ExceptionAssert.ThrowsArgNull(() => VersionUtility.ParseVersionSpec(null), "value");
        }

        [TestMethod]
        public void ParseVersionSpecSimpleVersionNoBrackets() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("1.2");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual(null, versionInfo.MaxVersion);
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecSimpleVersionNoBracketsExtraSpaces() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("  1  .   2  ");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual(null, versionInfo.MaxVersion);
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecMaxOnlyInclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(,1.2]");

            // Assert
            Assert.AreEqual(null, versionInfo.MinVersion);
            Assert.IsFalse(versionInfo.IsMinInclusive);
            Assert.AreEqual("1.2", versionInfo.MaxVersion.ToString());
            Assert.IsTrue(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecMaxOnlyExclusive() {
            var versionInfo = VersionUtility.ParseVersionSpec("(,1.2)");
            Assert.AreEqual(null, versionInfo.MinVersion);
            Assert.IsFalse(versionInfo.IsMinInclusive);
            Assert.AreEqual("1.2", versionInfo.MaxVersion.ToString());
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecExactVersion() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2]");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual("1.2", versionInfo.MaxVersion.ToString());
            Assert.IsTrue(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecMinOnlyExclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,)");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsFalse(versionInfo.IsMinInclusive);
            Assert.AreEqual(null, versionInfo.MaxVersion);
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecRangeExclusiveExclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,2.3)");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsFalse(versionInfo.IsMinInclusive);
            Assert.AreEqual("2.3", versionInfo.MaxVersion.ToString());
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecRangeExclusiveInclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("(1.2,2.3]");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsFalse(versionInfo.IsMinInclusive);
            Assert.AreEqual("2.3", versionInfo.MaxVersion.ToString());
            Assert.IsTrue(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecRangeInclusiveExclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2,2.3)");
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual("2.3", versionInfo.MaxVersion.ToString());
            Assert.IsFalse(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecRangeInclusiveInclusive() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("[1.2,2.3]");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual("2.3", versionInfo.MaxVersion.ToString());
            Assert.IsTrue(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void ParseVersionSpecRangeInclusiveInclusiveExtraSpaces() {
            // Act
            var versionInfo = VersionUtility.ParseVersionSpec("   [  1 .2   , 2  .3   ]  ");

            // Assert
            Assert.AreEqual("1.2", versionInfo.MinVersion.ToString());
            Assert.IsTrue(versionInfo.IsMinInclusive);
            Assert.AreEqual("2.3", versionInfo.MaxVersion.ToString());
            Assert.IsTrue(versionInfo.IsMaxInclusive);
        }

        [TestMethod]
        public void TrimVersionTrimsRevisionIfZero() {
            // Act
            var version = VersionUtility.TrimVersion(new Version("1.2.3.0"));

            // Assert
            Assert.AreEqual(new Version("1.2.3"), version);
        }

        [TestMethod]
        public void TrimVersionTrimsRevisionAndBuildIfZero() {
            // Act
            var version = VersionUtility.TrimVersion(new Version("1.2.0.0"));

            // Assert
            Assert.AreEqual(new Version("1.2"), version);
        }

        [TestMethod]
        public void TrimVersionTrimsBuildIfRevisionIsNonZero() {
            // Act
            var version = VersionUtility.TrimVersion(new Version("1.2.0.5"));

            // Assert
            Assert.AreEqual(new Version("1.2.0.5"), version);
        }

        [TestMethod]
        public void GetSafeVersions() {
            // Act
            IVersionSpec versionSpec1 = VersionUtility.GetSafeRange(new Version("1.3"));
            IVersionSpec versionSpec2 = VersionUtility.GetSafeRange(new Version("0.9"));
            IVersionSpec versionSpec3 = VersionUtility.GetSafeRange(new Version("2.9.45.6"));

            // Assert
            AssertSafeVersion(versionSpec1, new Version("1.3"), new Version("1.4"));
            AssertSafeVersion(versionSpec2, new Version("0.9"), new Version("0.10"));
            AssertSafeVersion(versionSpec3, new Version("2.9.45.6"), new Version("2.10"));
        }

        private void AssertSafeVersion(IVersionSpec versionSpec, Version minVer, Version maxVer) {
            Assert.IsTrue(versionSpec.IsMinInclusive);
            Assert.IsFalse(versionSpec.IsMaxInclusive);
            Assert.AreEqual(versionSpec.MinVersion, minVer);
            Assert.AreEqual(versionSpec.MaxVersion, maxVer);
        }

        [TestMethod]
        public void TrimVersionThrowsIfVersionNull() {
            ExceptionAssert.ThrowsArgNull(() => VersionUtility.TrimVersion(null), "version");
        }

        [TestMethod]
        public void IsCompatibleReturnsFalseForSlAndWindowsPhoneFrameworks() {
            // Arrange
            FrameworkName sl3 = VersionUtility.ParseFrameworkName("sl3");
            FrameworkName wp7 = VersionUtility.ParseFrameworkName("sl3-wp");

            // Act
            bool wp7CompatibleWithSl = VersionUtility.IsCompatible(sl3, wp7);
            bool slCompatibleWithWp7 = VersionUtility.IsCompatible(wp7, sl3);

            // Assert
            Assert.IsFalse(slCompatibleWithWp7);
            Assert.IsFalse(wp7CompatibleWithSl);
        }

        [TestMethod]
        public void IsCompatibleWindowsPhoneVersions() {
            // Arrange
            FrameworkName wp7 = VersionUtility.ParseFrameworkName("sl3-wp");
            FrameworkName wp7Mango = VersionUtility.ParseFrameworkName("sl4-wp71");

            // Act
            bool wp7MangoCompatibleWithwp7 = VersionUtility.IsCompatible(wp7, wp7Mango);
            bool wp7CompatibleWithwp7Mango = VersionUtility.IsCompatible(wp7Mango, wp7);

            // Assert
            Assert.IsFalse(wp7MangoCompatibleWithwp7);
            Assert.IsTrue(wp7CompatibleWithwp7Mango);
        }

        [TestMethod]
        public void NetFrameworkCompatibiilityIsCompatibleReturns() {
            // Arrange
            FrameworkName net40 = VersionUtility.ParseFrameworkName("net40");
            FrameworkName net40Client = VersionUtility.ParseFrameworkName("net40-client");

            // Act
            bool netClientCompatibleWithNet = VersionUtility.IsCompatible(net40, net40Client);
            bool netCompatibleWithClient = VersionUtility.IsCompatible(net40Client, net40);

            // Assert
            Assert.IsTrue(netClientCompatibleWithNet);
            Assert.IsTrue(netCompatibleWithClient);
        }

        [TestMethod]
        public void LowerFrameworkVersionsAreNotCompatibleWithHigherFrameworkVersionsWithSameFrameworkName() {
            // Arrange
            FrameworkName net40 = VersionUtility.ParseFrameworkName("net40");
            FrameworkName net20 = VersionUtility.ParseFrameworkName("net20");

            // Act
            bool net40CompatibleWithNet20 = VersionUtility.IsCompatible(net20, net40);
            bool net20CompatibleWithNet40 = VersionUtility.IsCompatible(net40, net20);

            // Assert
            Assert.IsFalse(net40CompatibleWithNet20);
            Assert.IsTrue(net20CompatibleWithNet40);
        }
    }
}
