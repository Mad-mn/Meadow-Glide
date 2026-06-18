using System.Collections.Generic;
using System.Text;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.LocalizationModule.Scripts.Utils
{
    public static class CsvParser
    {
        public static (List<Language> languages, Dictionary<Language, Dictionary<LocalizationKey, string>> data) Parse(string csvContent)
        {
            var languages = new List<Language>();
            var data = new Dictionary<Language, Dictionary<LocalizationKey, string>>();
            var lines = csvContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
                return (languages, data);

            var header = ParseCsvLine(lines[0]);

            for (int i = 2; i < header.Length; i++)
            {
                if (System.Enum.TryParse<Language>(header[i], true, out var lang))
                {
                    languages.Add(lang);
                    data[lang] = new Dictionary<LocalizationKey, string>();
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length < 3) continue;

                if (!System.Enum.TryParse<LocalizationKey>(columns[1], true, out var key))
                    continue;

                for (int j = 2; j < columns.Length && j - 2 < languages.Count; j++)
                {
                    data[languages[j - 2]][key] = columns[j];
                }
            }

            return (languages, data);
        }

        public static string GenerateCsv(List<Language> languages, Dictionary<Language, Dictionary<LocalizationKey, string>> data)
        {
            var sb = new StringBuilder();

            sb.Append("Id,Key");
            foreach (var lang in languages)
            {
                sb.Append(',');
                sb.Append(lang.ToString());
            }
            sb.AppendLine();

            var allKeys = new HashSet<LocalizationKey>();
            foreach (var langData in data.Values)
            {
                foreach (var key in langData.Keys)
                {
                    allKeys.Add(key);
                }
            }

            foreach (var key in allKeys)
            {
                sb.Append((int)key);
                sb.Append(',');
                sb.Append(key.ToString());

                foreach (var lang in languages)
                {
                    sb.Append(',');
                    if (data.TryGetValue(lang, out var langData) && langData.TryGetValue(key, out var value))
                    {
                        sb.Append(EscapeCsvValue(value));
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}