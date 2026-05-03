using System.Text;

namespace BlazOrbit.Core.Utilities;

public static class StringExtensions
{
    public static string ToKebabCase(this string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        StringBuilder sb = new(propertyName.Length + 4);
        sb.Append(char.ToLowerInvariant(propertyName[0]));
        for (int i = 1; i < propertyName.Length; i++)
        {
            char c = propertyName[i];
            if (char.IsUpper(c))
            {
                sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
