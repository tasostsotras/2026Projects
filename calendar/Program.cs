
using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();

            Console.Write("Enter a year (Q to quit): ");
            string input = Console.ReadLine()!.Trim();

            // Quit
            if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
                break;

            // Try to convert the input to BigInteger
            if (!BigInteger.TryParse(input, out BigInteger year))
            {
                Console.WriteLine();
                Console.WriteLine("Invalid year.");
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
                continue;
            }

            Console.Clear();
            PrintCalendar(year);

            Console.WriteLine();
            Console.WriteLine("Press Enter to enter another year...");
            Console.ReadLine();
        }

        Console.Clear();
        Console.WriteLine("Goodbye!");
    }

    static void PrintCalendar(BigInteger year)
    {
        string[] monthNames =
        {
            "January", "February", "March",
            "April", "May", "June",
            "July", "August", "September",
            "October", "November", "December"
        };

        int[] daysInMonth =
        {
            31, 28, 31, 30, 31, 30,
            31, 31, 30, 31, 30, 31
        };

        if (IsLeapYear(year))
            daysInMonth[1] = 29;

        Console.WriteLine($"Calendar for {year}");
        Console.WriteLine();

        for (int month = 1; month <= 12; month++)
        {
            PrintMonth(
                year,
                month,
                monthNames[month - 1],
                daysInMonth[month - 1]);
        }
    }

    static void PrintMonth(
        BigInteger year,
        int month,
        string monthName,
        int days)
    {
        Console.WriteLine($"      {monthName}");
        Console.WriteLine("Su Mo Tu We Th Fr Sa");

        // 0 = Sunday
        // 1 = Monday
        // ...
        // 6 = Saturday
        int firstDay = DayOfWeek(year, month, 1);

        for (int i = 0; i < firstDay; i++)
            Console.Write("   ");

        for (int day = 1; day <= days; day++)
        {
            Console.Write($"{day,2} ");

            if ((firstDay + day) % 7 == 0)
                Console.WriteLine();
        }

        Console.WriteLine();
    }

    static bool IsLeapYear(BigInteger year)
    {
        return Mod(year, 400) == 0 ||
               (Mod(year, 4) == 0 && Mod(year, 100) != 0);
    }

    static int DayOfWeek(
        BigInteger year,
        int month,
        int day)
    {
        BigInteger a = FloorDivide(14 - month, 12);
        BigInteger y = year + 4800 - a;
        BigInteger m = month + 12 * a - 3;

        BigInteger jdn =
            day
            + FloorDivide(153 * m + 2, 5)
            + 365 * y
            + FloorDivide(y, 4)
            - FloorDivide(y, 100)
            + FloorDivide(y, 400)
            - 32045;

        return (int)Mod(jdn + 1, 7);
    }

    static BigInteger Mod(
        BigInteger value,
        BigInteger modulus)
    {
        BigInteger result = value % modulus;

        if (result < 0)
            result += modulus;

        return result;
    }

    static BigInteger FloorDivide(
        BigInteger numerator,
        BigInteger denominator)
    {
        BigInteger quotient = numerator / denominator;
        BigInteger remainder = numerator % denominator;

        if (remainder != 0 &&
            ((remainder > 0) != (denominator > 0)))
        {
            quotient--;
        }

        return quotient;
    }
}
