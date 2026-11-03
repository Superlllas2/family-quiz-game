using System.Globalization;

namespace FamilyQuiz.Core.Content
{
    /// <summary>How the TV and the phones write a closest value number: no thousands separator, at most 2 decimals.</summary>
    public static class NumberText
    {
        public static string Format(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>"{value} {unit}" style texts with an empty unit lose the trailing space.</summary>
        public static string WithUnit(string text)
        {
            return text?.Trim();
        }
    }
}
