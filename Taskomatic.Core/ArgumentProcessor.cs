using System.Collections.Generic;
using System.Linq;

namespace Taskomatic.Core
{
    internal static class ArgumentProcessor
    {
        public static string CygwinArgumentsToString(IEnumerable<string> arguments) =>
            string.Join(" ", arguments.Select(CygwinPrepareArgument));

        public static string CygwinPrepareArgument(string argument)
        {
            string Escape() => argument.Replace("\\", "\\\\").Replace("\"", "\\\"");

            if (argument.Any(a => a == ' ' || a == '"' || a == '\\'))
            {
                return "\"" + Escape() + "\"";
            }

            return Escape();
        }
    }
}
