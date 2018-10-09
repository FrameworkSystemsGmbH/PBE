using System;
using System.Text;

namespace PBE
{
    internal class HtmlHelper
    {
        public static string HtmlEncode(String text)
        {
            StringBuilder sbResult = new StringBuilder();

            WriteHtmlEncode(sbResult, text);

            return sbResult.ToString();
        }

        public static void WriteHtmlEncode(StringBuilder sb, String text)
        {
            char cLast = ' ';
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\n':
                        if (cLast != '\r')
                        {
                            sb.Append("<br/>");
                        }
                        break;

                    case '\r':
                        sb.Append("<br/>");
                        break;

                    case '<':
                        sb.Append("&lt;");
                        break;

                    case '>':
                        sb.Append("&gt;");
                        break;

                    case 'ä':
                        sb.Append("&auml;");
                        break;

                    case 'Ä':
                        sb.Append("&Auml;");
                        break;

                    case 'ö':
                        sb.Append("&ouml;");
                        break;

                    case 'Ö':
                        sb.Append("&Ouml;");
                        break;

                    case 'ü':
                        sb.Append("&uuml;");
                        break;

                    case 'Ü':
                        sb.Append("&Uuml;");
                        break;

                    case 'ß':
                        sb.Append("&szlig;");
                        break;

                    case '&':
                        sb.Append("&amp;");
                        break;

                    default:
                        sb.Append(c);
                        break;
                }

                cLast = c;
            }
        }
    }
}