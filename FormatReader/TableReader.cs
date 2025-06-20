using System.Globalization;
using System.Text.RegularExpressions;

namespace FormatReader
{
    public class TableReader
    {
        public static string? LabelReader(string input, string labelName)
        {
            input = input.Replace("\r\n", "\n");
            Regex labelRegex = new(labelName + @":\s*\n.*\n");
            Match m = labelRegex.Match(input);
            if (!m.Success)
                return null;
            return m.Value.Split('\n')[1].Trim();
        }
        private static readonly string[] tableType = ["db", "db", "db", "dw", "dw", "dl", "dl", "dd", "dd"];
        public static List<(string, List<int>)> ReadTable(string input, string tableName, int digitsPerValue, params string[] subTableSufixes)
        {
            input = input.Replace("\r\n", "\n");
            string sufixesPattern = "_(" + string.Join('|', subTableSufixes) + ')';
            string twodotsjump = @":\s*\n";
            string tableDigits = tableType[Math.Clamp(digitsPerValue, 1, tableType.Length - 1)];
            string valuePattern = @"\$[a-fA-F0-9]{" + digitsPerValue + @"}";
            string subTablePattern = @"[a-zA-Z][\w-]*" + sufixesPattern + twodotsjump + @"(\s*" +
                tableDigits + @"\s+" + @"(" + valuePattern + "(," + valuePattern + @")*\s*\n?))+";
            Regex fulltableRegex = new(tableName + twodotsjump + @$"({subTablePattern})+");
            
            Match m = fulltableRegex.Match(input);
            if (!m.Success)
                return [];
            Regex subTableRegex = new(subTablePattern);
            MatchCollection matches = subTableRegex.Matches(m.Value);

            Regex valuesRegex = new(valuePattern);
            List<int> values;
            MatchCollection valuesMatches;
            Regex suffixRegex = new(sufixesPattern);
            string name;
            string line1;

            List<(string, List<int>)> ret = [];

            foreach (Match match in matches)
            {
                values = [];
                valuesMatches = valuesRegex.Matches(match.Value);
                foreach (Match match2 in valuesMatches)
                {
                    values.Add(int.Parse(match2.Value[1..], NumberStyles.AllowHexSpecifier));
                }
                line1 = match.Value.Split('\n')[0];
                name = line1.Trim()[..^1];
                ret.Add((name, values));
            }
            return ret;
        }
    }
}
