using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class GameRules
{
    private readonly List<string> moves;
    private readonly int halfSize;

    public GameRules(List<string> moves)
    {
        this.moves = moves;
        halfSize = moves.Count / 2;
    }

    public int CompareMoves(int userMove, int computerMove)
    {
        int movesCount = moves.Count;

        if (userMove == computerMove)
        {
            return 0;
        }
        else if ((userMove + halfSize) % movesCount == computerMove || (computerMove + halfSize) % movesCount == userMove)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public string GetMoveName(int move)
    {
        if (move >= 1 && move <= moves.Count)
        {
            return moves[move - 1];
        }

        return string.Empty;
    }

    public List<string> GetMoves()
    {
        return moves;
    }

    public int GetMovesCount()
    {
        return moves.Count;
    }
}

class KeyGenerator
{
    public static byte[] GenerateKey()
    {
        byte[] key = new byte[32];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        return key;
    }
}

class HMACCalculator
{
    public static string CalculateHMAC(byte[] key, byte[] data)
    {
        using (HMACSHA256 hmac = new HMACSHA256(key))
        {
            byte[] hashBytes = hmac.ComputeHash(data);

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }
}

class MenuPrinter
{
    private readonly GameRules rules;

    public MenuPrinter(GameRules rules)
    {
        this.rules = rules;
    }

    public void PrintMenu()
    {
        Console.WriteLine("Available moves:");

        for (int i = 0; i < rules.GetMovesCount(); i++)
        {
            int moveNumber = i + 1;
            string moveName = rules.GetMoveName(moveNumber);

            Console.WriteLine($"{moveNumber} - {moveName}");
        }

        Console.WriteLine("0 - exit");
        Console.WriteLine("? - help");
    }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3 || args.Length % 2 == 0)
        {
            Console.WriteLine("Please provide an odd number of unique moves (>= 3) as command-line arguments.");
            return;
        }

        List<string> moves = args.ToList();
        for (int i = 0; i < moves.Count; i++)
        {
            for (int j = 1; j < moves.Count; j++)
            {
                if (moves[i] == moves[j])
                {
                    Console.WriteLine("Moves should be unique");
                    return;
                }
            }
        }

        GameRules rules = new GameRules(moves);
        MenuPrinter menuPrinter = new MenuPrinter(rules);

        byte[] key = KeyGenerator.GenerateKey();

        int computerMove = new Random().Next(1, rules.GetMovesCount() + 1);

        string computerMoveName = rules.GetMoveName(computerMove);
        Console.WriteLine($"HMAC: {HMACCalculator.CalculateHMAC(key, Encoding.UTF8.GetBytes(computerMoveName))}");

        menuPrinter.PrintMenu();

        while (true)
        {
            Console.Write("Enter your move: ");

            string input = Console.ReadLine().Trim().ToLower();

            if (input == "?")
            {
                PrintHelpTable(rules);

                continue;
            }

            if (input == "0")
            {
                break;
            }

            if (!int.TryParse(input, out int userMove) || userMove < 1 || userMove > rules.GetMovesCount())
            {
                Console.WriteLine("Invalid input. Try again or enter '?' for help.");

                continue;
            }

            string userMoveName = rules.GetMoveName(userMove);
            Console.WriteLine($"Your move: {userMoveName}");

            Console.WriteLine($"Computer move: {computerMoveName}");

            int result = rules.CompareMoves(userMove, computerMove);
            if (result == 0)
            {
                Console.WriteLine("It's a draw!");
            }
            else if (result == 1)
            {
                Console.WriteLine("You win!");
            }
            else
            {
                Console.WriteLine("You lose!");
            }

            Console.WriteLine($"HMAC key: {BitConverter.ToString(key).Replace("-", string.Empty)}");

            break;
        }
    }

    private static void PrintHelpTable(GameRules rules)
    {
        List<string> moves = rules.GetMoves();
        int movesCount = moves.Count;

        Console.Write("Moves / Results".PadRight(18));

        for (int i = 0; i < movesCount; i++)
        {
            Console.Write(moves[i].PadRight(12));
        }

        Console.WriteLine();

        for (int i = 0; i < movesCount; i++)
        {
            Console.Write(moves[i].PadRight(18));

            for (int j = 0; j < movesCount; j++)
            {
                int result = rules.CompareMoves(i + 1, j + 1);
                if (result == 0)
                {
                    Console.Write("Draw".PadRight(12));
                }
                else if (result == 1)
                {
                    Console.Write("Win".PadRight(12));
                }
                else
                {
                    Console.Write("Lose".PadRight(12));
                }
            }

            Console.WriteLine();
        }
    }
}
