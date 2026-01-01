namespace TheKittySaver.AdoptionSystem.Primitives.Extensions;

public static class MathExtensions
{
    extension(Math)
    {
        public static double RoundMathematically(double value, int digits = 0)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }
        
        public static decimal RoundMathematically(decimal value, int digits = 0)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }
    }
}
