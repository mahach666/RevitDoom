namespace DoomNetFrameworkEngine.DoomEntity.MathUtils
{
    internal static class NetFunc
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float RoundF(float value)
        {
            return (float)System.Math.Round(value);
        }
        public static float RoundF(float value, int digits)
        {
            return (float)System.Math.Round(value, digits);
        }

    }
}