using System;
using System.Numerics;

namespace EasterCalculator
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("================================================");
            Console.WriteLine("          PERPETUAL EASTER CALCULATOR");
            Console.WriteLine("================================================");
            Console.WriteLine();
            Console.WriteLine("Enter an integer year.");
            Console.WriteLine("There is no fixed year limit.");
            Console.WriteLine("Use Q to quit.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Year: ");

                string? input = Console.ReadLine();

                if (string.Equals(input, "q",
                    StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (!BigInteger.TryParse(
                        input,
                        out BigInteger year))
                {
                    Console.WriteLine("Invalid integer year.");
                    Console.WriteLine();
                    continue;
                }

                try
                {
                    CalculateAndDisplay(year);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Calculation error: {ex.Message}");
                }

                Console.WriteLine();
                Console.WriteLine("Press Enter to calculate another year.");
                Console.ReadLine();
                Console.WriteLine();
            }
        }

        // =========================================================
        // MAIN
        // =========================================================

        static void CalculateAndDisplay(BigInteger year)
        {
            GregorianResult gregorian =
                CalculateGregorianEaster(year);

            JulianResult julian =
                CalculateJulianEaster(year);

            HebrewDate passover =
                CalculatePassover(year);

            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine($"YEAR: {FormatYear(year)}");
            Console.WriteLine("================================================");
            Console.WriteLine();

            Console.WriteLine("COMPUTISTICAL PARAMETERS");
            Console.WriteLine("------------------------");

            Console.WriteLine(
                $"Golden Number:       {gregorian.GoldenNumber}");

            Console.WriteLine(
                $"Gregorian Epact:     {gregorian.Epact}");

            Console.WriteLine(
                $"Gregorian Dominical: {gregorian.DominicalLetter}");

            Console.WriteLine(
                $"Julian Dominical:    {julian.DominicalLetter}");

            Console.WriteLine();

            Console.WriteLine("GREGORIAN RECKONING");
            Console.WriteLine("-------------------");

            Console.WriteLine(
                $"Paschal Full Moon:   {gregorian.PaschalFullMoon}");

            Console.WriteLine(
                $"Easter Sunday:       {gregorian.Easter}");

            Console.WriteLine();

            Console.WriteLine("JULIAN RECKONING");
            Console.WriteLine("----------------");

            Console.WriteLine(
                $"Paschal Full Moon:   {julian.PaschalFullMoon}");

            Console.WriteLine(
                $"Easter Sunday:       {julian.Easter}");

            Console.WriteLine();

            Console.WriteLine("JULIAN → GREGORIAN");
            Console.WriteLine("------------------");

            Console.WriteLine(
                $"Julian Easter in Gregorian calendar:");

            Console.WriteLine(
                $"  {JulianToGregorianString(julian.Easter)}");

            Console.WriteLine();

            Console.WriteLine("JEWISH PASSOVER");
            Console.WriteLine("---------------");

            Console.WriteLine(
                $"15 Nisan:            {passover}");

            Console.WriteLine();

            Console.WriteLine("================================================");
        }

        // =========================================================
        // GREGORIAN EASTER
        // Meeus/Jones/Butcher integer algorithm.
        // =========================================================

        static GregorianResult CalculateGregorianEaster(
            BigInteger year)
        {
            BigInteger a =
                Mod(year, 19);

            BigInteger b =
                FloorDiv(year, 100);

            BigInteger c =
                Mod(year, 100);

            BigInteger d =
                FloorDiv(b, 4);

            BigInteger e =
                Mod(b, 4);

            BigInteger f =
                FloorDiv(b + 8, 25);

            BigInteger g =
                FloorDiv(b - f + 1, 3);

            BigInteger h =
                Mod(
                    19 * a +
                    b -
                    d -
                    g +
                    15,
                    30);

            BigInteger i =
                FloorDiv(c, 4);

            BigInteger k =
                Mod(c, 4);

            BigInteger l =
                Mod(
                    32 +
                    2 * e +
                    2 * i -
                    h -
                    k,
                    7);

            BigInteger m =
                FloorDiv(
                    a +
                    11 * h +
                    22 * l,
                    451);

            BigInteger month =
                FloorDiv(
                    h +
                    l -
                    7 * m +
                    114,
                    31);

            BigInteger day =
                Mod(
                    h +
                    l -
                    7 * m +
                    114,
                    31) + 1;

            CalendarDate easter =
                new CalendarDate(
                    year,
                    (int)month,
                    (int)day,
                    CalendarType.Gregorian);

            int goldenNumber =
                (int)Mod(year, 19) + 1;

            int epact =
                CalculateGregorianEpact(year);

            string dominical =
                CalculateGregorianDominicalLetter(year);

            CalendarDate fullMoon =
                CalculateGregorianPaschalFullMoon(year);

            return new GregorianResult
            {
                Easter = easter,
                PaschalFullMoon = fullMoon,
                GoldenNumber = goldenNumber,
                Epact = epact,
                DominicalLetter = dominical
            };
        }

        // =========================================================
        // GREGORIAN EPACT
        // =========================================================

        static int CalculateGregorianEpact(
            BigInteger year)
        {
            BigInteger goldenNumber =
                Mod(year, 19) + 1;

            BigInteger century =
                FloorDiv(year, 100);

            BigInteger solarCorrection =
                FloorDiv(century, 4);

            BigInteger lunarCorrection =
                FloorDiv(
                    8 * century + 13,
                    25);

            BigInteger epact =
                Mod(
                    11 * (goldenNumber - 1)
                    + 20
                    - solarCorrection
                    + lunarCorrection,
                    30);

            if (epact <= 0)
                epact += 30;

            return (int)epact;
        }

        // =========================================================
        // GREGORIAN DOMINICAL LETTER
        // =========================================================

        static string CalculateGregorianDominicalLetter(
            BigInteger year)
        {
            BigInteger jdn =
                GregorianToJdn(
                    year,
                    1,
                    1);

            int weekday =
                (int)Mod(jdn + 1, 7);

            string first =
                ((char)('A' + weekday)).ToString();

            if (!IsGregorianLeapYear(year))
                return first;

            int second =
                (int)Mod(
                    weekday - 1,
                    7);

            return first +
                   ((char)('A' + second));
        }

        // =========================================================
        // GREGORIAN PASCHAL FULL MOON
        // =========================================================

        static CalendarDate CalculateGregorianPaschalFullMoon(
            BigInteger year)
        {
            int epact =
                CalculateGregorianEpact(year);

            /*
             * Ecclesiastical Paschal full moon.
             *
             * The Gregorian Paschal full moon lies between
             * March 21 and April 18.
             */

            BigInteger day =
                44 - epact;

            BigInteger month = 3;

            while (day > DaysInMonthGregorian(
                       year,
                       (int)month))
            {
                day -= DaysInMonthGregorian(
                    year,
                    (int)month);

                month++;
            }

            while (day < 1)
            {
                month--;

                day += DaysInMonthGregorian(
                    year,
                    (int)month);
            }

            CalendarDate result =
                new CalendarDate(
                    year,
                    (int)month,
                    (int)day,
                    CalendarType.Gregorian);

            BigInteger resultJdn =
                GregorianToJdn(
                    result.Year,
                    result.Month,
                    result.Day);

            BigInteger march21 =
                GregorianToJdn(
                    year,
                    3,
                    21);

            while (resultJdn < march21)
            {
                resultJdn += 30;

                result =
                    GregorianFromJdn(resultJdn);
            }

            while (resultJdn > march21 + 28)
            {
                resultJdn -= 30;

                result =
                    GregorianFromJdn(resultJdn);
            }

            return result;
        }

        // =========================================================
        // JULIAN EASTER
        // =========================================================

        static JulianResult CalculateJulianEaster(
            BigInteger year)
        {
            BigInteger a =
                Mod(year, 4);

            BigInteger b =
                Mod(year, 7);

            BigInteger c =
                Mod(year, 19);

            BigInteger d =
                Mod(
                    19 * c + 15,
                    30);

            BigInteger e =
                Mod(
                    2 * a +
                    4 * b -
                    d +
                    34,
                    7);

            BigInteger month =
                FloorDiv(
                    d +
                    e +
                    114,
                    31);

            BigInteger day =
                Mod(
                    d +
                    e +
                    114,
                    31) + 1;

            CalendarDate easter =
                new CalendarDate(
                    year,
                    (int)month,
                    (int)day,
                    CalendarType.Julian);

            CalendarDate fullMoon =
                CalculateJulianPaschalFullMoon(year);

            return new JulianResult
            {
                Easter = easter,
                PaschalFullMoon = fullMoon,
                DominicalLetter =
                    CalculateJulianDominicalLetter(year)
            };
        }

        // =========================================================
        // JULIAN PASCHAL FULL MOON
        // =========================================================

        static CalendarDate CalculateJulianPaschalFullMoon(
            BigInteger year)
        {
            BigInteger goldenNumber =
                Mod(year, 19) + 1;

            BigInteger epact =
                Mod(
                    11 * (goldenNumber - 1),
                    30);

            BigInteger dayOfYear =
                44 - epact;

            while (dayOfYear < 22)
                dayOfYear += 30;

            while (dayOfYear > 56)
                dayOfYear -= 30;

            BigInteger month = 3;
            BigInteger day = dayOfYear;

            while (day > DaysInMonthJulian(
                       year,
                       (int)month))
            {
                day -= DaysInMonthJulian(
                    year,
                    (int)month);

                month++;
            }

            return new CalendarDate(
                year,
                (int)month,
                (int)day,
                CalendarType.Julian);
        }

        // =========================================================
        // JULIAN DOMINICAL LETTER
        // =========================================================

        static string CalculateJulianDominicalLetter(
            BigInteger year)
        {
            BigInteger jdn =
                JulianToJdn(
                    year,
                    1,
                    1);

            int weekday =
                (int)Mod(jdn + 1, 7);

            string first =
                ((char)('A' + weekday)).ToString();

            if (!IsJulianLeapYear(year))
                return first;

            int second =
                (int)Mod(
                    weekday - 1,
                    7);

            return first +
                   ((char)('A' + second));
        }

        // =========================================================
        // HEBREW / JEWISH CALENDAR
        // =========================================================

        static HebrewDate CalculatePassover(
            BigInteger gregorianYear)
        {
            /*
             * The Hebrew year containing Passover is approximately
             * Gregorian year + 3760.
             *
             * Because Nisan occurs before the Hebrew New Year,
             * this is the Hebrew year beginning in the previous
             * autumn.
             */

            BigInteger hebrewYear =
                gregorianYear + 3760;

            return new HebrewDate(
                hebrewYear,
                1,
                15);
        }

        static bool IsHebrewLeapYear(
            BigInteger year)
        {
            return Mod(
                7 * year + 1,
                19) < 7;
        }

        static int HebrewMonthsInYear(
            BigInteger year)
        {
            return IsHebrewLeapYear(year)
                ? 13
                : 12;
        }

        static BigInteger HebrewCalendarElapsedDays(
            BigInteger year)
        {
            BigInteger monthsElapsed =
                235 * FloorDiv(
                    year - 1,
                    19)
                +
                12 * Mod(
                    year - 1,
                    19)
                +
                FloorDiv(
                    7 * Mod(
                        year - 1,
                        19) + 1,
                    19);

            BigInteger parts =
                12084 +
                13753 * monthsElapsed;

            BigInteger day =
                29 * monthsElapsed +
                FloorDiv(
                    parts,
                    25920);

            BigInteger remainingParts =
                Mod(
                    parts,
                    25920);

            // Molad Zaken
            if (remainingParts >= 19440)
            {
                day++;
            }

            // GaTRaD
            else if (
                Mod(day, 7) == 2 &&
                remainingParts >= 9924 &&
                !IsHebrewLeapYear(year))
            {
                day++;
            }

            // BeTuTaKPaT
            else if (
                Mod(day, 7) == 1 &&
                remainingParts >= 16789 &&
                IsHebrewLeapYear(year - 1))
            {
                day++;
            }

            // Lo ADU Rosh Hashanah
            if (Mod(day, 7) == 0 ||
                Mod(day, 7) == 3 ||
                Mod(day, 7) == 5)
            {
                day++;
            }

            return day;
        }

        static BigInteger HebrewToJdn(
            BigInteger year,
            int month,
            int day)
        {
            BigInteger result =
                HebrewCalendarElapsedDays(year);

            if (month >= 7)
            {
                for (int m = 7;
                     m < month;
                     m++)
                {
                    result +=
                        HebrewMonthDays(
                            year,
                            m);
                }
            }
            else
            {
                for (int m = 7;
                     m <= HebrewMonthsInYear(year);
                     m++)
                {
                    result +=
                        HebrewMonthDays(
                            year,
                            m);
                }

                for (int m = 1;
                     m < month;
                     m++)
                {
                    result +=
                        HebrewMonthDays(
                            year,
                            m);
                }
            }

            result += day - 1;

            return result + 347997;
        }

        static int HebrewMonthDays(
            BigInteger year,
            int month)
        {
            switch (month)
            {
                case 1:
                    return 30; // Nisan

                case 2:
                    return 29; // Iyar

                case 3:
                    return 30; // Sivan

                case 4:
                    return 29; // Tammuz

                case 5:
                    return 30; // Av

                case 6:
                    return 29; // Elul

                case 7:
                    return 30; // Tishri

                case 8:
                    return IsHebrewCompleteYear(year)
                        ? 30
                        : 29;

                case 9:
                    return IsHebrewDeficientYear(year)
                        ? 29
                        : 30;

                case 10:
                    return 29;

                case 11:
                    return 30;

                case 12:
                    return IsHebrewLeapYear(year)
                        ? 30
                        : 29;

                case 13:
                    return 29;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(month));
            }
        }

        static BigInteger HebrewYearDays(
            BigInteger year)
        {
            return
                HebrewCalendarElapsedDays(
                    year + 1)
                -
                HebrewCalendarElapsedDays(
                    year);
        }

        static bool IsHebrewDeficientYear(
            BigInteger year)
        {
            BigInteger days =
                HebrewYearDays(year);

            return days == 353 ||
                   days == 383;
        }

        static bool IsHebrewCompleteYear(
            BigInteger year)
        {
            BigInteger days =
                HebrewYearDays(year);

            return days == 355 ||
                   days == 385;
        }

        // =========================================================
        // GREGORIAN CALENDAR
        // =========================================================

        static bool IsGregorianLeapYear(
            BigInteger year)
        {
            return
                Mod(year, 4) == 0 &&
                (
                    Mod(year, 100) != 0 ||
                    Mod(year, 400) == 0
                );
        }

        static int DaysInMonthGregorian(
            BigInteger year,
            int month)
        {
            switch (month)
            {
                case 1:
                    return 31;

                case 2:
                    return IsGregorianLeapYear(year)
                        ? 29
                        : 28;

                case 3:
                    return 31;

                case 4:
                    return 30;

                case 5:
                    return 31;

                case 6:
                    return 30;

                case 7:
                    return 31;

                case 8:
                    return 31;

                case 9:
                    return 30;

                case 10:
                    return 31;

                case 11:
                    return 30;

                case 12:
                    return 31;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(month));
            }
        }

        // =========================================================
        // JULIAN CALENDAR
        // =========================================================

        static bool IsJulianLeapYear(
            BigInteger year)
        {
            return Mod(year, 4) == 0;
        }

        static int DaysInMonthJulian(
            BigInteger year,
            int month)
        {
            switch (month)
            {
                case 1:
                    return 31;

                case 2:
                    return IsJulianLeapYear(year)
                        ? 29
                        : 28;

                case 3:
                    return 31;

                case 4:
                    return 30;

                case 5:
                    return 31;

                case 6:
                    return 30;

                case 7:
                    return 31;

                case 8:
                    return 31;

                case 9:
                    return 30;

                case 10:
                    return 31;

                case 11:
                    return 30;

                case 12:
                    return 31;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(month));
            }
        }

        // =========================================================
        // JULIAN DAY NUMBER - GREGORIAN
        // =========================================================

        static BigInteger GregorianToJdn(
            BigInteger year,
            int month,
            int day)
        {
            BigInteger a =
                FloorDiv(
                    14 - month,
                    12);

            BigInteger y =
                year + 4800 - a;

            BigInteger m =
                month +
                12 * a -
                3;

            return
                day
                +
                FloorDiv(
                    153 * m + 2,
                    5)
                +
                365 * y
                +
                FloorDiv(y, 4)
                -
                FloorDiv(y, 100)
                +
                FloorDiv(y, 400)
                -
                32045;
        }

        // =========================================================
        // JULIAN DAY NUMBER - JULIAN
        // =========================================================

        static BigInteger JulianToJdn(
            BigInteger year,
            int month,
            int day)
        {
            BigInteger a =
                FloorDiv(
                    14 - month,
                    12);

            BigInteger y =
                year + 4800 - a;

            BigInteger m =
                month +
                12 * a -
                3;

            return
                day
                +
                FloorDiv(
                    153 * m + 2,
                    5)
                +
                365 * y
                +
                FloorDiv(y, 4)
                -
                32083;
        }

        // =========================================================
        // JDN → GREGORIAN
        // =========================================================

        static CalendarDate GregorianFromJdn(
            BigInteger jdn)
        {
            BigInteger a =
                jdn + 32044;

            BigInteger b =
                FloorDiv(
                    4 * a + 3,
                    146097);

            BigInteger c =
                a -
                FloorDiv(
                    146097 * b,
                    4);

            BigInteger d =
                FloorDiv(
                    4 * c + 3,
                    1461);

            BigInteger e =
                c -
                FloorDiv(
                    1461 * d,
                    4);

            BigInteger m =
                FloorDiv(
                    5 * e + 2,
                    153);

            BigInteger day =
                e -
                FloorDiv(
                    153 * m + 2,
                    5)
                + 1;

            BigInteger month =
                m +
                3 -
                12 * FloorDiv(
                    m,
                    10);

            BigInteger year =
                100 * b +
                d -
                4800 +
                FloorDiv(
                    m,
                    10);

            return new CalendarDate(
                year,
                (int)month,
                (int)day,
                CalendarType.Gregorian);
        }

        // =========================================================
        // JULIAN → GREGORIAN
        // =========================================================

        static string JulianToGregorianString(
            CalendarDate julian)
        {
            BigInteger jdn =
                JulianToJdn(
                    julian.Year,
                    julian.Month,
                    julian.Day);

            CalendarDate gregorian =
                GregorianFromJdn(jdn);

            return gregorian.ToString();
        }

        // =========================================================
        // MATHEMATICAL MODULO
        // =========================================================

        static BigInteger Mod(
            BigInteger value,
            BigInteger modulus)
        {
            BigInteger result =
                value % modulus;

            if (result < 0)
                result += modulus;

            return result;
        }

        // =========================================================
        // MATHEMATICAL FLOOR DIVISION
        // =========================================================

        static BigInteger FloorDiv(
            BigInteger a,
            BigInteger b)
        {
            if (b == 0)
                throw new DivideByZeroException();

            BigInteger q = a / b;
            BigInteger r = a % b;

            if (r != 0 &&
                ((r > 0) != (b > 0)))
            {
                q--;
            }

            return q;
        }

        // =========================================================
        // YEAR DISPLAY
        // =========================================================

        static string FormatYear(
            BigInteger year)
        {
            if (year > 0)
                return year.ToString();

            if (year == 0)
                return "0 (astronomical year numbering)";

            return
                $"{BigInteger.Abs(year) + 1} BC";
        }
    }

    // =============================================================
    // CALENDAR TYPE
    // =============================================================

    internal enum CalendarType
    {
        Gregorian,
        Julian
    }

    // =============================================================
    // CALENDAR DATE
    // =============================================================

    internal readonly struct CalendarDate
    {
        public BigInteger Year { get; }

        public int Month { get; }

        public int Day { get; }

        public CalendarType Calendar { get; }

        public CalendarDate(
            BigInteger year,
            int month,
            int day,
            CalendarType calendar)
        {
            Year = year;
            Month = month;
            Day = day;
            Calendar = calendar;
        }

        public override string ToString()
        {
            string calendarName =
                Calendar == CalendarType.Gregorian
                    ? "Gregorian"
                    : "Julian";

            string yearText =
                Year >= 0
                    ? Year.ToString()
                    : $"{BigInteger.Abs(Year) + 1} BC";

            return
                $"{Day:D2}/{Month:D2}/{yearText} " +
                $"({calendarName})";
        }
    }

    // =============================================================
    // HEBREW DATE
    // =============================================================

    internal readonly struct HebrewDate
    {
        public BigInteger Year { get; }

        public int Month { get; }

        public int Day { get; }

        public HebrewDate(
            BigInteger year,
            int month,
            int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public override string ToString()
        {
            string monthName =
                Month switch
                {
                    1 => "Nisan",
                    2 => "Iyar",
                    3 => "Sivan",
                    4 => "Tammuz",
                    5 => "Av",
                    6 => "Elul",
                    7 => "Tishri",
                    8 => "Heshvan",
                    9 => "Kislev",
                    10 => "Tevet",
                    11 => "Shevat",
                    12 => "Adar I",
                    13 => "Adar II",
                    _ => "Unknown"
                };

            return
                $"{Day} {monthName} {Year}";
        }
    }

    // =============================================================
    // GREGORIAN RESULT
    // =============================================================

    internal sealed class GregorianResult
    {
        public CalendarDate Easter { get; set; }

        public CalendarDate PaschalFullMoon { get; set; }

        public int GoldenNumber { get; set; }

        public int Epact { get; set; }

        public string DominicalLetter { get; set; } = "";
    }

    // =============================================================
    // JULIAN RESULT
    // =============================================================

    internal sealed class JulianResult
    {
        public CalendarDate Easter { get; set; }

        public CalendarDate PaschalFullMoon { get; set; }

        public string DominicalLetter { get; set; } = "";
    }
}
