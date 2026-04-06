using System.Globalization;

namespace InventarioApp.Utils;

public static class ParseUtils
{
    public static bool TryParseDecimal(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        string text = input.Trim();

        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt(string? input, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        string text = input.Trim();
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
            || int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static bool IsNullOrWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value);
}

