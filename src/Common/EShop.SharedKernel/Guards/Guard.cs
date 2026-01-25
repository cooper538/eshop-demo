namespace EShop.SharedKernel.Guards;

public static class Guard
{
    public static class Against
    {
        public static T Null<T>(T? value, string parameterName)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static string NullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty.", parameterName);
            }

            return value;
        }

        public static int NegativeOrZero(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Value must be greater than zero."
                );
            }

            return value;
        }

        public static decimal Negative(decimal value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
            }

            return value;
        }
    }
}
