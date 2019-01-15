using System;

namespace FleetTrack.Core.Common
{
    /// <summary>
    /// Small helpers for argument validation.
    /// </summary>
    public static class Guard
    {
        public static T NotNull<T>(T value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        public static string NotEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", name);
            }

            return value;
        }

        public static int Positive(int value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value must be positive.");
            }

            return value;
        }

        public static double Positive(double value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value must be positive.");
            }

            return value;
        }

        public static double NotNegative(double value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value cannot be negative.");
            }

            return value;
        }

        public static decimal NotNegative(decimal value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value cannot be negative.");
            }

            return value;
        }

        public static int InRange(int value, int min, int max, string name)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(name, value, $"Value must be between {min} and {max}.");
            }

            return value;
        }

        public static double InRange(double value, double min, double max, string name)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(name, value, $"Value must be between {min} and {max}.");
            }

            return value;
        }
    }
}
