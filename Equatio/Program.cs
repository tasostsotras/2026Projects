using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Quadratic Equation Solver");
        Console.WriteLine("Equation: ax² + bx + c = 0");

        Console.Write("Enter a: ");
        double a = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter b: ");
        double b = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter c: ");
        double c = Convert.ToDouble(Console.ReadLine());

        if (a == 0)
        {
            // Linear equation: bx + c = 0
            if (b == 0)
            {
                Console.WriteLine(c == 0
                    ? "There are infinitely many solutions."
                    : "There is no solution.");
            }
            else
            {
                double x = -c / b;
                Console.WriteLine($"Solution: x = {x}");
            }

            return;
        }

        // Calculate the discriminant
        double discriminant = b * b - 4 * a * c;

        if (discriminant > 0)
        {
            double x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double x2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

            Console.WriteLine($"Two solutions:");
            Console.WriteLine($"x1 = {x1}");
            Console.WriteLine($"x2 = {x2}");
        }
        else if (discriminant == 0)
        {
            double x = -b / (2 * a);

            Console.WriteLine($"One solution:");
            Console.WriteLine($"x = {x}");
        }
        else
        {
            Console.WriteLine("There are no real solutions.");
        }
    }
}
