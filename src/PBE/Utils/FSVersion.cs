using System;

namespace PBE.Utils
{
    public class FSVersion
    {
        public readonly Version Version;
        public readonly bool IsPreview;

        public FSVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException("Version string cannot be null or empty.", nameof(version));
            }

            if (version.EndsWith(".pre", StringComparison.OrdinalIgnoreCase))
            {
                IsPreview = true;
                version = version.Substring(0, version.Length - 4);
            }

            if (Version.TryParse(version, out Version v))
            {
                Version = new Version(
                    v.Major,
                    v.Minor,
                    v.Build == -1 ? 0 : v.Build,
                    v.Revision == -1 ? 0 : v.Revision);
            }
            else
            {
                throw new ArgumentException($"Version String {version} could not be Parsed!");
            }
        }

        public string ToString(int amount)
        {
            return IsPreview ? ToString() : Version.ToString(amount);
        }

        public string ToDisplayString()
        {
            string version = Version.Revision != 0 ? ToString(4) :
                                Version.Build != 0 ? ToString(3) :
                                    IsPreview ? Version.ToString(2) : ToString(2);

            return version + (IsPreview ? " Preview" : string.Empty);
        }

        public override string ToString()
        {
            if (IsPreview)
                return Version.ToString(2) + ".pre";

            return Version.Revision != 0 ? ToString(4) :
                    Version.Build != 0 ? ToString(3) :
                     ToString(2);
        }

        public static bool TryParse(string version, out FSVersion ver)
        {
            try
            {
                ver = new FSVersion(version);
                return true;
            }
            catch (Exception)
            {
                ver = null;
                return false;
            }
        }
    }
}