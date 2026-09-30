using System;

class Program
{
    static void Main()
    {
        Console.Write("Enter number A: ");
        int A = int.Parse(Console.ReadLine());

        Console.Write("Enter number B: ");
        int B = int.Parse(Console.ReadLine());

        int sum = 0;

        // Find the proper divisors of A
        for (int i = 1; i <= A / 2; i++)
        {
            if (A % i == 0)
            {
                sum += i;
            }
        }

        // Check if the sum of the divisors of A equals B
        if (sum == B)
        {
            Console.WriteLine("The numbers are alter ego.");
        }
        else
        {
            Console.WriteLine("The numbers are not alter ego.");
        }

        Console.WriteLine("Sum of divisors of A = " + sum);
    }
}
