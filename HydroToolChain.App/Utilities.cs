using System.Globalization;
using System.Text;
using HydroToolChain.App.Configuration.Data;

namespace HydroToolChain.App;

internal static class Utilities
{
    public static string GetOutFile(ProjectData project)
    {
        var fileName = $"{project.ModIndex}-{project.Name}_P_old.pak";
        var outPath = Path.Combine(project.OutputPath, "dist");
        var outputFile = Path.Combine(outPath, fileName);

        return outputFile;
    }

    public static IReadOnlyCollection<int> SearchBytePattern(byte[] pattern, byte[] bytes)
    {
        List<int> positions = new List<int>();
        int patternLength = pattern.Length;
        int totalLength = bytes.Length;
        byte firstMatchByte = pattern[0];
        for (int i = 0; i < totalLength; i++)
        {
            if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
            {
                byte[] match = new byte[patternLength];
                Array.Copy(bytes, i, match, 0, patternLength);
                if (match.SequenceEqual<byte>(pattern))
                {
                    positions.Add(i);
                    i += patternLength - 1;
                }
            }
        }

        if (positions.Count == 0)
        {
            return Array.Empty<int>();
        }

        return positions;
    }

    public static byte[] Hex2Binary(string hex)
    {
        var chars = hex.ToCharArray();
        var bytes = new List<byte>();
        for (int index = 0; index < chars.Length; index += 2)
        {
            var chunk = new string(chars, index, 2);
            bytes.Add(byte.Parse(chunk, NumberStyles.AllowHexSpecifier));
        }
        return bytes.ToArray();
    }
    
    public static byte[] String2Binary(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }
}