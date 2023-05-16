using System.Text;

public static class StringHelper
{
    public static List<string> SplitString(string input)
    {
        List<string> result = new List<string>();
        StringBuilder buffer = new StringBuilder();
        bool singleQuote = false;

        for (int i = 0; i < input.Length; i++)
        {
            char character = input[i];

            if (character == '\'')
            {
                buffer.Append(character);
                singleQuote = !singleQuote;
            }
            else if (!singleQuote && (character == 'a' || character == 'o') && i + 1 < input.Length && (input.Substring(i, 3) == "and" || input.Substring(i, 2) == "or"))
            {
                if (buffer.Length > 0)
                {
                    result.Add(buffer.ToString().Trim());
                    buffer.Clear();
                }

                result.Add(input.Substring(i, 3).Trim());
                i += 2;
            }
            else
            {
                buffer.Append(character);
            }
        }

        if (buffer.Length > 0)
        {
            result.Add(buffer.ToString().Trim());
        }

        return result;
    }

    public static List<string> SplitStringByWhitespace(string str)
    {
        List<string> parts = new List<string>();
        StringBuilder sb = new StringBuilder();
        bool singleQuote = false;

        foreach (char character in str)
        {
            switch (character)
            {
                case ' ':
                    if (singleQuote)
                    {
                        sb.Append(character);
                    }
                    else
                    {
                        parts.Add(sb.ToString());
                        sb.Clear();
                    }
                    break;
                case '\'':
                    singleQuote = !singleQuote;
                    sb.Append(character);
                    break;
                default:
                    sb.Append(character);
                    break;
            }
        }

        parts.Add(sb.ToString());

        return parts;
    }
}