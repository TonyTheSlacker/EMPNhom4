using System.Globalization; // Provides CultureInfo for locale-specific formatting
using System.Text; // Provides StringBuilder for efficient string manipulation

namespace EmployeeManagementSystem { // Application namespace
    public static class CurrencyFormatter { // Utility class for currency formatting/parsing (pure logic; no DB or external data access)
        private static readonly CultureInfo Vi = new CultureInfo("vi-VN"); // Vietnamese culture for thousand separators (.) and grouping

        // formatting (10.000.000)
        public static string Format(long value) => value.ToString("N0", Vi); // Format long with thousand separators, no decimals
        public static string Format(int value) => value.ToString("N0", Vi); // Format int with thousand separators, no decimals
        public static string Format(long? value) => Format(value.GetValueOrDefault()); // Null-safe long formatting (null -> 0)
        public static string Format(int? value) => Format(value.GetValueOrDefault()); // Null-safe int formatting (null -> 0)
        public static string Format(decimal value) => value.ToString("N0", Vi); // Format decimal with thousand separators, no decimals

        // Parse a formatted currency-like string to int by stripping non-digits (e.g., "10.000.000 VNĐ" -> 10000000)
        public static int ParseToInt(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return 0; // Empty or whitespace -> 0
            var chars = text.Trim().ToCharArray(); // Work over characters
            var buf = new StringBuilder(chars.Length); // Buffer for digits only
            foreach (var ch in chars)
            {
                if (char.IsDigit(ch)) // Keep only digits (drop dots, commas, spaces, currency symbol)
                    buf.Append(ch);
            }
            if (int.TryParse(buf.ToString(), out var val))
                return val; // Return parsed number
            return 0; // Fallback if parsing fails
        }
    }
}
