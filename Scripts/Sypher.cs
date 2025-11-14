public static class Sypher
{
    private static int mod = char.MaxValue + 1;

    private static int a = 11;
    private static int b = 3;

    public static int[] ConvertToCharCodes(string text)
        => text.Select(symbol => (int)symbol).ToArray();

    public static string ConvertFromCharCodes(int[] codes)
        => new string(codes.Select(code => (char)code).ToArray());

    public static int[] TransformCodes(int[] codes)
    {
        int[] result = new int[codes.Length];
        for (int i = 0; i < codes.Length; i++)
            result[i] = ((a * (codes[i] + b) + i) % mod);
        return result;
    }

    public static int[] InverseTransformCodes(int[] codes)
    {
        int inverseA = ModInverse(a, mod);
        int[] result = new int[codes.Length];

        for (int i = 0; i < codes.Length; i++)
        {
            int y = (codes[i] + mod) % mod; 
            int temp = (y - i + mod) % mod;              
            int x = ((inverseA * temp) % mod - b + mod) % mod; 
            result[i] = x;
        }

        return result;
    }

    public static int ModInverse(int a, int m)
    {
        int m0 = m, x0 = 0, x1 = 1;
        if (m == 1) return 0;

        while (a > 1)
        {
            int q = a / m;
            (a, m) = (m, a % m);
            (x0, x1) = (x1 - q * x0, x0);
        }

        if (x1 < 0)
            x1 += m0;

        return x1;
    }

    public static string Encode(string text)
    {
        return ConvertFromCharCodes(TransformCodes(ConvertToCharCodes(text)));
    }

    public static string Decode(string text)
    {
        return ConvertFromCharCodes(InverseTransformCodes(ConvertToCharCodes(text)));
    }
}