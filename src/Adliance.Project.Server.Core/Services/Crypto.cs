using System.Text;

namespace Adliance.Project.Server.Core.Services;

public static class Crypto
{
    public static string RandomString(int length)
    {
        var random = new Random();
        var sb = new StringBuilder();

        var availableCharacters = new List<char>();

        // get all characters and numbers from ASCII. Look it up: http://www.asciitable.com/.
        for (var i = 48; i <= 122; i++)
        {
            if (i is >= 58 and <= 64 || i is >= 91 and <= 96) continue;
            availableCharacters.Add((char)i);
        }

        for (var i = 1; i <= length; i++) sb.Append(availableCharacters[random.Next(0, availableCharacters.Count - 1)]);
        return sb.ToString();
    }
}