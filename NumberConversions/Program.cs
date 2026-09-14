
using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Numeral Base Converter ===");
        Console.WriteLine("Supported bases: 2 - 36");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Enter the number to convert (or 'q' to quit): ");
            string input = Console.ReadLine();

            if (input.ToLower() == "q")
                break;

            Console.Write("Enter the original base (2-36): ");
            int fromBase = int.Parse(Console.ReadLine());

            Console.Write("Enter the target base (2-36): ");
            int toBase = int.Parse(Console.ReadLine());

            if (fromBase < 2 || fromBase > 36 ||
                toBase < 2 || toBase > 36)
            {
                Console.WriteLine("Invalid base. Bases must be between 2 and 36.");
                Console.WriteLine();
                continue;
            }

            try
            {
                // Convert the input number to decimal
                BigInteger decimalValue = ToDecimal(input, fromBase);

                // Convert the decimal value to the target base
                string result = FromDecimal(decimalValue, toBase);

                Console.WriteLine();
                Console.WriteLine($"{input} (base {fromBase}) = {result} (base {toBase})");
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine("Invalid number for the specified base.");
                Console.WriteLine();
            }
        }

        Console.WriteLine("Goodbye!");
    }

    // Converts a number from any base to decimal
    static BigInteger ToDecimal(string number, int fromBase)
    {
        number = number.ToUpper();
        BigInteger result = 0;

        foreach (char c in number)
        {
            int digit;

            if (c >= '0' && c <= '9')
                digit = c - '0';
            else if (c >= 'A' && c <= 'Z')
                digit = c - 'A' + 10;
            else
                throw new FormatException();

            if (digit >= fromBase)
                throw new FormatException();

            result = result * fromBase + digit;
        }

        return result;
    }

    // Converts a decimal number to any base
    static string FromDecimal(BigInteger number, int toBase)
    {
        if (number == 0)
            return "0";

        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        bool negative = number < 0;
        if (negative)
            number = BigInteger.Abs(number);

        string result = "";

        while (number > 0)
        {
            int remainder = (int)(number % toBase);
            result = digits[remainder] + result;
            number /= toBase;
        }

        return negative ? "-" + result : result;
    }
}

