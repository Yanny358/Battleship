using System;

namespace Operators
{
    public class Operation
    {
        private static double CalculatorCurrentDisplay = 0.0;

        public static double GetCurrentNumber() => CalculatorCurrentDisplay;
       

        public static string Abs()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write($"Absolute value of a {CalculatorCurrentDisplay} is: ");

            CalculatorCurrentDisplay = Math.Abs(CalculatorCurrentDisplay);
            Console.WriteLine($"{CalculatorCurrentDisplay}");
            
            return "";
        }
        
        public static string Plus()
        {
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write("plus number: ");
            
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            CalculatorCurrentDisplay += converted;
            Console.WriteLine($"Current result is: {CalculatorCurrentDisplay}");

            
            return "";
        }
        public static string Divide()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.WriteLine("divide by number: ");
            
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            CalculatorCurrentDisplay /= converted;
            Console.WriteLine($"Current result is: {CalculatorCurrentDisplay}");

            
            return "";
        }
        public static string Multiplicate()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.WriteLine("multiplicate");
            Console.Write("number: ");
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            CalculatorCurrentDisplay *= converted;
            Console.WriteLine($"Current result is: {CalculatorCurrentDisplay}");

            
            return "";
        }
        public static string Negate()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write("Negative number is: ");

            CalculatorCurrentDisplay *= -1;
            Console.WriteLine($"{CalculatorCurrentDisplay}");
            
            return "";
        }
        public static string Pow()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.WriteLine("to the power of a number: ");
            
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            CalculatorCurrentDisplay = Math.Pow(CalculatorCurrentDisplay, converted);
            Console.WriteLine($"Current result is: {CalculatorCurrentDisplay}");
            
            return "";
        }
        public static string SquareNumber()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write($"Square of a {CalculatorCurrentDisplay} is: ");

            CalculatorCurrentDisplay *= CalculatorCurrentDisplay;
            Console.WriteLine($"{CalculatorCurrentDisplay}");
            
            return "";
        }
        public static string Root()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write($"Square root of a {CalculatorCurrentDisplay} is: ");
            
            CalculatorCurrentDisplay = Math.Sqrt(CalculatorCurrentDisplay);
            Console.WriteLine($"{CalculatorCurrentDisplay}");
            
            return "";
        }
        public static string Minus()
        {
            
            // CalculatorCurrentDisplay
            Console.WriteLine("Current value: " + CalculatorCurrentDisplay);
            Console.Write("minus number: ");
           
            var n = Console.ReadLine()?.Trim();
            double.TryParse(n, out var converted);

            CalculatorCurrentDisplay -= converted;
            Console.WriteLine($"Current result is: {CalculatorCurrentDisplay}");

            
            return "";
        }
    }
}