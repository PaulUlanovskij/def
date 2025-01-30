
namespace def;

public static class UIManager
{
  public static string LimitByLength(string str, int len)
  {
    return str.Length > len ? str[0..(len - 1)] + "…" : str;
  }
  public static string[] SplitByLength(string str, int len)
  {
    List<string> parts = new();
    while (str.Length > len)
    {
      int split_index = str[0..len].LastIndexOf(' ');
      parts.Add(str[0..(split_index + 1)]);
      str = str[(split_index + 1)..];
    }
    parts.Add(str);
    return parts.ToArray();
  }
  public static void WriteLeftaligned(string str, int width, ConsoleColor fgc = ConsoleColor.Gray, ConsoleColor bgc = ConsoleColor.Black)
  {
    Console.ForegroundColor = fgc;
    Console.BackgroundColor = bgc;
    Console.Write(str);
    Console.ResetColor();
    Console.Write(new string(' ', width - str.Length));
  }
  public static void WriteRightaligned(string str, int width, ConsoleColor fgc = ConsoleColor.Gray, ConsoleColor bgc = ConsoleColor.Black)
  {
    Console.Write(new string(' ', width - str.Length));
    Console.ForegroundColor = fgc;
    Console.BackgroundColor = bgc;
    Console.Write(str);
    Console.ResetColor();
  }
  public static void WriteCentered(string str, int width, ConsoleColor fgc = ConsoleColor.Gray, ConsoleColor bgc = ConsoleColor.Black)
  {
    int padleft = (width - str.Length) / 2;
    int padright = width - str.Length - padleft;

    Console.Write(new string(' ', padleft));
    Console.ForegroundColor = fgc;
    Console.BackgroundColor = bgc;
    Console.Write(str);
    Console.ResetColor();
    Console.Write(new string(' ', padright));
  }
  public static void RollDiceMenu()
  {
    Console.WriteLine("What dice to roll?");
    int die = GetInputOptions(new string[] { "d4", "d6", "d8", "d10", "d12", "d20" });
    BlankPreviousLine();
    switch (die)
    {
      case 1:
        Console.WriteLine($"Rolling d4 ... {Dice.Rolld4()}");
        break;
      case 2:
        Console.WriteLine($"Rolling d6 ... {Dice.Rolld6()}");
        break;
      case 3:
        Console.WriteLine($"Rolling d8 ... {Dice.Rolld8()}");
        break;
      case 4:
        Console.WriteLine($"Rolling d10 ... {Dice.Rolld10()}");
        break;
      case 5:
        Console.WriteLine($"Rolling d12 ... {Dice.Rolld12()}");
        break;
      case 6:
        Console.WriteLine($"Rolling d20 ... {Dice.Rolld20()}");
        break;
    }
  }

  public static int ReadInt(string prompt = "")
  {
    while (true)
    {
      if (prompt != "")
      {
        Console.WriteLine(prompt);
      }
      string input = Console.ReadLine() ?? "";

      if (int.TryParse(input, System.Globalization.NumberStyles.Integer, null, out int result))
      {
        if (prompt != "")
        {
          BlankPreviousLine();
        }
        BlankPreviousLine();

        return result;
      }
      if (prompt != "")
      {
        BlankPreviousLine();
      }
      BlankPreviousLine();
    }
  }
  public static int ReadIntClamped(string prompt, int min_inc, int max_inc)
  {
    int input = min_inc - 1;
    while (input < min_inc || input > max_inc)
    {
      input = ReadInt(prompt);
    }
    return input;
  }
  public static int GetInputOptions(string[] options)
  {
    if (options.Length < 1)
    {
      throw new ArgumentOutOfRangeException("Invalid amount of options");
    }

    int amount_of_lines = 0;

    for (int i = 0; i < options.Length; i++)
    {
      amount_of_lines += PrintBlockOffseted($"{i+1}", ConsoleColor.Gray, options[i]);
    }

    int choosen = ReadIntClamped("", 1, options.Length);
    BlankPreviousLines(amount_of_lines);

    return choosen;
  }
  public static int PrintBlockOffseted(string name, ConsoleColor fgc, string text)
  {
    var splitText = UIManager.SplitByLength(text, 80);
    Console.ForegroundColor = fgc;
    Console.Write(name);
    Console.ResetColor();
    Console.Write(" >>> ");
    int offset = name.Length + 5;
    Console.WriteLine(splitText[0]);
    for (int i = 1; i < splitText.Length; i++)
    {
      Console.WriteLine(new string(' ', offset) + splitText[i]);
    }
    return splitText.Length;
  }
  public static void BlankCurrentLine()
  {
    Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
  }
  public static void BlankPreviousLine()
  {
    Console.SetCursorPosition(0, Console.CursorTop - 1);
    Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
  }
  public static void BlankPreviousLines(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException("can not blank out less than zero lines");
    }
    for (int x = 0; x < amount; x++)
    {
      BlankPreviousLine();
    }
  }

}
