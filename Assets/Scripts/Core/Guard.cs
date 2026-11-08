using System;

namespace LastOutpost.Core
{
    internal static class Guard
    {
        public static int Positive(int value, string name)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(name);
            return value;
        }

        public static int NonNegative(int value, string name)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(name);
            return value;
        }

        public static double NonNegativeFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new ArgumentOutOfRangeException(name);
            return value;
        }

        public static double PositiveFinite(double value, string name)
        {
            NonNegativeFinite(value, name);
            if (value == 0) throw new ArgumentOutOfRangeException(name);
            return value;
        }

        public static string Identifier(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("An identifier is required.", name);
            return value;
        }
    }
}
