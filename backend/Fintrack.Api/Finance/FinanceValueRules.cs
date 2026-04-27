namespace Fintrack.Api.Finance;

internal static class FinanceValueRules
{
    public static bool HasMaxScale(decimal value, int maxScale)
    {
        var scale = (decimal.GetBits(value)[3] >> 16) & 0x7F;
        return scale <= maxScale;
    }

    public static decimal RoundCurrency(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
