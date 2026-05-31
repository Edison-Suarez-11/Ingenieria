using System.Globalization;
using System.Text.RegularExpressions;

namespace VerticeMusicasWeb.Helpers;

public static class NumeroColombianoHelper
{
    private static readonly CultureInfo CulturaCo = CultureInfo.GetCultureInfo("es-CO");
    private static readonly Regex DecimalConPunto = new(@"^\d+\.\d{1,2}$", RegexOptions.Compiled);

    public static bool TryParseDecimal(string? valor, out decimal resultado) =>
        TryParsePrecio(valor, out resultado);

    public static decimal ParseDecimal(string? valor)
    {
        if (!TryParsePrecio(valor, out decimal resultado))
        {
            throw new FormatException($"El valor numerico '{valor}' no es valido.");
        }

        return resultado;
    }

    /// <summary>
    /// Precios en pesos: 150, 150,50, 15.000, 15.000,50.
    /// Distingue "150.00" (ciento cincuenta) de "15.000" (quince mil).
    /// </summary>
    public static bool TryParsePrecio(string? valor, out decimal resultado)
    {
        resultado = 0;
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        string texto = valor.Trim()
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        if (texto.Contains(','))
        {
            return decimal.TryParse(texto, NumberStyles.Number, CulturaCo, out resultado);
        }

        if (DecimalConPunto.IsMatch(texto))
        {
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out resultado);
        }

        if (texto.Contains('.'))
        {
            return decimal.TryParse(texto, NumberStyles.Number, CulturaCo, out resultado);
        }

        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out resultado);
    }

    /// <summary>Porcentajes simples (10, 30,5). No usa separador de miles.</summary>
    public static bool TryParsePorcentaje(string? valor, out decimal resultado)
    {
        resultado = 0;
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        string texto = valor.Trim().Replace("%", string.Empty).Trim();

        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out resultado))
        {
            return true;
        }

        if (texto.Contains(',') && !texto.Contains('.'))
        {
            return decimal.TryParse(texto, NumberStyles.Number, CulturaCo, out resultado);
        }

        return false;
    }
}
