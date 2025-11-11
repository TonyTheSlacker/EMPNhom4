using System.Globalization;
using System.Text;

namespace EmployeeManagementSystem {
    public static class CurrencyFormatter {
        private static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        //formatting (10.000.000)
        public static string Format(long value) => value.ToString("N0", Vi);
        public static string Format(int value) => value.ToString("N0", Vi);
        public static string Format(long? value) => Format(value.GetValueOrDefault());
        public static string Format(int? value) => Format(value.GetValueOrDefault());
        public static string Format(decimal value) => value.ToString("N0", Vi);


        public static int ParseToInt(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            var chars = text.Trim().ToCharArray();
            var buf = new StringBuilder(chars.Length);
            foreach (var ch in chars)
            {
                if (char.IsDigit(ch))
                    buf.Append(ch);
            }
            if (int.TryParse(buf.ToString(), out var val))
                return val;
            return 0;
        }
    }
}
