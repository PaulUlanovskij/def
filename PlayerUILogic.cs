namespace def;

public static class PlayerUILogic
{
  public static bool allow_prgress_entry = true;
  public static void Start()
  {
    LoadCharacter();
    if (!ConnectionManager.Join())
    {
      return;
    }

    Console.Title = "S - show stats; R - roll; B - bio; I - inventory; L - leave;";

    Thread receiveCheck = new Thread(new ThreadStart(ConnectionManager.ReceiveCheckLoop));
    receiveCheck.Start();
    while (true)
    {
      ConsoleKey input = Console.ReadKey().Key;
      UIManager.BlankCurrentLine();
      switch (input)
      {
        case ConsoleKey.S:
          BanProgressEntry();
          OpenStatsMenu();
          AllowProgressEntry();
          break;
        case ConsoleKey.R:
          BanProgressEntry();
          UIManager.RollDiceMenu();
          AllowProgressEntry();
          break;
        case ConsoleKey.B:
          BanProgressEntry();
          OpenBioMenu();
          AllowProgressEntry();
          break;
        case ConsoleKey.I:
          BanProgressEntry();
          OpenInventory();
          AllowProgressEntry();
          break;
        case ConsoleKey.L:
          return;
      }
      Console.Title = "S - show stats; R - roll; B - bio; I - inventory; L - leave;";
    }
  }



  public static int PlayOutEntry(Entry entry, Entry[] entries)
  {
AH_HERE_WE_GO_AGAIN:
    if (allow_prgress_entry == false)
    {
      Thread.Sleep(100);
      goto AH_HERE_WE_GO_AGAIN;
    }


    if (!ThoughtInitChallenge(entry))
    {
      return entry.init_fail;
    }

    string stat_name = CharacterSheet.IndexToName(entry.stat);
    ConsoleColor stat_color = CharacterSheet.GetStatColor(entry.stat);

    UIManager.PrintBlockOffseted(stat_name, stat_color, entry.text);

    int lines_total = 0;

    for (int i = 0; i < entry.options.Length; i++)
    {
      EntryResolution resolution = entry.options[i];
      if (resolution.link != int.MaxValue)
      {
        var link_entry = entries[resolution.link];
        if (link_entry.target != 0)
        {
          lines_total += WriteChallengeBlockOffseted(resolution, link_entry, i);
        }
        else
        {
          lines_total += UIManager.PrintBlockOffseted($"{i + 1}.", ConsoleColor.Gray, resolution.text);
        }
      }
      else
      {
        lines_total += UIManager.PrintBlockOffseted($"{i + 1}.", ConsoleColor.Gray, resolution.text);
      }
    }

    int choice = 0;
    while (choice < 1 || choice > entry.options.Length)
    {
      while (true)
      {
        if (allow_prgress_entry is false)
        {
          goto AH_HERE_WE_GO_AGAIN;
        }
        string read_input = Console.ReadLine() ?? "";

        if (allow_prgress_entry is false)
        {
          goto AH_HERE_WE_GO_AGAIN;
        }

        if (int.TryParse(read_input, System.Globalization.NumberStyles.Integer, null, out choice))
        {
          UIManager.BlankPreviousLine();

          break;
        }
        UIManager.BlankPreviousLine();
      }

    }
    UIManager.BlankPreviousLines(lines_total);

    EntryResolution res = entry.options[choice-1];
      if (res.link != int.MaxValue)
      {
        var link_entry = entries[res.link];
        if (link_entry.target != 0)
        {
          WriteChallengeBlockOffseted(res, link_entry, choice-1);
        }
        else
        {
          UIManager.PrintBlockOffseted($"{choice}.", ConsoleColor.Gray, res.text);
        }
      }
      else
      {
        UIManager.PrintBlockOffseted($"{choice}.", ConsoleColor.Gray, res.text);
      }


    int link = res.link;
    if (link == int.MaxValue)
    {
      return int.MaxValue;
    }

    var linked_entry = entries[link];

    if (linked_entry.target == 0)
    {
      return link;
    }
    else
    {

      if (EntryOptionChallenge(linked_entry))
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{CharacterSheet.IndexToName(linked_entry.stat)}] Success!");
        Console.ResetColor();
        return linked_entry.success;
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{CharacterSheet.IndexToName(linked_entry.stat)}] Failure!");
        Console.ResetColor();
        return linked_entry.fail;
      }
    }


    int WriteChallengeBlockOffseted(EntryResolution er, Entry e, int index)
    {
      var splitText = UIManager.SplitByLength(er.text, 80);
      string name = CharacterSheet.IndexToName(e.stat);
      string chance = CalculateSuccessChance(e.stat, e.target).ToString();

      Console.Write($"{index + 1}. ");

      Console.ForegroundColor = CharacterSheet.GetStatColor(e.stat);
      Console.Write($"[ {name} ][ {chance} ]");
      Console.ResetColor();

      Console.Write(" >>> ");

      int offset = 3 + name.Length + chance.Length + 8 + 5;
      Console.WriteLine(splitText[0]);

      for (int i = 1; i < splitText.Length; i++)
      {
        Console.WriteLine(new string(' ', offset) + splitText[i]);
      }

      return splitText.Length;
    }
    bool ThoughtInitChallenge(Entry e)
    {
      int _base_stat = Program.character.stats[e.stat];
      int _stat_mod = Program.character.mods[e.stat];
      int _roll_result = Dice.Rolld6(2);
      int _check_value = _base_stat + _stat_mod + _roll_result;

      bool _greater = (e.type.Contains("g") && _check_value > e.target);
      bool _equals = (e.type.Contains("e") && e.target == _check_value);
      bool _less = (e.type.Contains("l") && _check_value < e.target);

      return (_greater || _equals || _less || e.type == "" || _roll_result == 12);
    }
    bool EntryOptionChallenge(Entry e)
    {
      int _base_stat = Program.character.stats[e.stat];
      int _stat_mod = Program.character.mods[e.stat];
      int _roll_result = Dice.Rolld6(2);
      int _check_value = _base_stat + _stat_mod + _roll_result;

      bool _nat_pass = (_check_value >= e.target);
      return (_nat_pass || _roll_result == 12);
    }
  }
  public static int CalculateSuccessChance(int stat, int target)
  {
    int base_value = Program.character.stats[stat] + Program.character.mods[stat];
    int diff = target - base_value;
    if (diff < 3)
    {
      return 100;
    }
    if (diff > 12)
    {
      return 3;
    }
    return diff switch
    {
      3 => 97,
      4 => 91,
      5 => 83,
      6 => 72,
      7 => 58,
      8 => 42,
      9 => 28,
      10 => 17,
      11 => 9,
      12 => 3,
    };
  }



  public static void BanProgressEntry()
  {
    allow_prgress_entry = false;
  }
  public static void AllowProgressEntry()
  {
    allow_prgress_entry = true;
  }


  public static void PlayOutEntries(Entry[] entries)
  {
    int next_entry = 0;
    while (next_entry != int.MaxValue)
    {
      next_entry = PlayOutEntry(entries[next_entry], entries);
    }
    Console.WriteLine("Thought fades...");
  }
  public static CharacterSheet LoadCharacter()
  {
    CharacterSheet? sheet = null;

    while (sheet == null)
    {
      Console.WriteLine("[SYSTEM] Authorization required. Awaiting id... ");
      var path = Console.ReadLine();
      UIManager.BlankPreviousLines(2);

      if (Program.TryReadJson(path, out sheet))
      {
        Program.character_file_path = path;
      }
      else
      {
        Console.WriteLine("[SYSTEM] Failed to locate id. Try again.");
      }
    }
    return sheet;
  }
  public static void OpenStatsMenu()
  {
    Console.Title = "C - stat value; M - mod value; S - close;";
    var character = Program.character;

    int cursorX = 0;
    int cursorY = 0;

    int statsPanelHeight = DrawStats(cursorX, cursorY);
    Thread.Sleep(100);
    while (true)
    {
      ConsoleKey key = Console.ReadKey().Key;
      UIManager.BlankCurrentLine();
      switch (key)
      {
        case ConsoleKey.UpArrow:
          cursorY = (4 + cursorY - 1) % 4;
          break;
        case ConsoleKey.DownArrow:
          cursorY = (cursorY + 1) % 4;
          break;
        case ConsoleKey.RightArrow:
          cursorX = (cursorX + 1) % 6;
          break;
        case ConsoleKey.LeftArrow:
          cursorX = (6 + cursorX - 1) % 6;
          break;
        case ConsoleKey.C:
          {
            int newValue = UIManager.ReadInt($"Current stat value: {character.stats[cursorY * 6 + cursorX]}. New Value: ");
            character.stats[cursorY * 6 + cursorX] = newValue;
          }
          break;
        case ConsoleKey.M:
          {
            int newValue = UIManager.ReadInt($"Current mod value: {character.mods[cursorY * 6 + cursorX]}. New Value: ");
            character.mods[cursorY * 6 + cursorX] = newValue;
          }
          break;
        case ConsoleKey.S:
          UIManager.BlankPreviousLines(statsPanelHeight);
          return;
      }
      UIManager.BlankPreviousLines(statsPanelHeight);
      DrawStats(cursorX, cursorY);
    }
  }
  private static int DrawStats(int cursorX, int cursorY)
  {
    var character = Program.character;

    int[] maxColumnWidths = new int[6];
    int[] maxRowHeights = new int[4];
    string[][] splits = new string[24][];
    int padBetweenColumns = 4;
    string padString = new string(' ', padBetweenColumns);

    for (int i = 0; i < 24; i++)
    {
      splits[i] = CharacterSheet.IndexToName(i).Split(' ');

      int r = i / 6;
      int c = i % 6;

      int maxSplitLength = splits[i].Max(x => x.Length);
      if (maxSplitLength > maxColumnWidths[c])
      {
        maxColumnWidths[c] = maxSplitLength;
      }

      if (maxRowHeights[r] < splits[i].Length)
      {
        maxRowHeights[r] = splits[i].Length;
      }
    }

    int maxRowLength = maxColumnWidths.Sum() + 7 * padBetweenColumns;
    string rowDelimiter = "+" + new string('-', maxRowLength) + "+";
    Console.WriteLine(rowDelimiter);

    for (int r = 0; r < 4; r++)
    {
      for (int h = 0; h < maxRowHeights[r]; h++)
      {
        Console.Write("|");
        for (int c = 0; c < 6; c++)
        {
          Console.Write(padString);
          var split = splits[r * 6 + c];
          string str = (split.Length > h) ? split[h] : "";
          UIManager.WriteCentered(str, maxColumnWidths[c], CharacterSheet.GetStatColor(r * 6 + c));
        }
        Console.WriteLine(padString + "|");
      }
      Console.Write("|");
      for (int c = 0; c < 6; c++)
      {
        int i = r * 6 + c;
        int stat = character.stats[i];
        int mod = character.mods[i];

        Console.Write(padString);

        string str = $"{stat} ({((mod < 0) ? "" : "+")}{character.mods[i]})";
        var bgc = (c == cursorX && r == cursorY) ? ConsoleColor.DarkGray : ConsoleColor.Black;
        UIManager.WriteCentered(str, maxColumnWidths[c], CharacterSheet.GetStatColor(r * 6 + c), bgc);

      }
      Console.WriteLine(padString + $"|\n{rowDelimiter}");

    }
    return maxRowHeights.Sum() + 4 + 5;
  }
  private static void OpenBioMenu()
  {

  }
  public static void OpenInventory()
  {
    Console.Title = "A - add; R - remove; C - amount; N - name; D - description; M - import item; I - close;";
    var character = Program.character;

    int cursorX = 0;
    int cursorY = 0;

    int inventoryPanelHeight = DrawInventory(cursorX, cursorY);
    Thread.Sleep(100);
    while (true)
    {
      ConsoleKey key = Console.ReadKey().Key;
      UIManager.BlankCurrentLine();
      switch (key)
      {
        case ConsoleKey.UpArrow:
          cursorY = (10 + cursorY - 1) % 10;
          break;
        case ConsoleKey.DownArrow:
          cursorY = (cursorY + 1) % 10;
          break;
        case ConsoleKey.RightArrow:
          cursorX = (cursorX + 1) % 3;
          break;
        case ConsoleKey.LeftArrow:
          cursorX = (3 + cursorX - 1) % 3;
          break;
        case ConsoleKey.A:
          {
            Item item = new();

            Console.WriteLine("Item name: ");
            item.name = Console.ReadLine() ?? "";

            Console.WriteLine("Item description: ");
            item.description = Console.ReadLine() ?? "";

            item.amount = UIManager.ReadInt("Item amount: ");
            UIManager.BlankPreviousLines(4);

            character.items.Add(item);
          }
          break;
        case ConsoleKey.R:
          {
            if (TryGetItem(out var item))
            {
              character.items.RemoveAt(cursorY * 3 + cursorX);
            }
          }
          break;
        case ConsoleKey.N:
          {
            if (TryGetItem(out var item))
            {
              Console.WriteLine($"Current item name: {item.name}. New name: ");
              string newName = Console.ReadLine() ?? "";
              UIManager.BlankPreviousLines(2);
              item.name = newName;
            }
          }
          break;
        case ConsoleKey.D:
          {
            if (TryGetItem(out var item))
            {
              Console.WriteLine($"Current item description: {item.description}. New description: ");
              string newDescription = Console.ReadLine() ?? "";
              UIManager.BlankPreviousLines(2);
              item.description = newDescription;
            }
          }
          break;
        case ConsoleKey.C:
          {
            if (TryGetItem(out var item))
            {
              int newAmount = UIManager.ReadInt($"Current item amount: {item.amount}. New amount: ");
              item.amount = newAmount;
            }
          }
          break;
        case ConsoleKey.M:
          {
            ImportItem();
          }
          break;
        case ConsoleKey.I:
          UIManager.BlankPreviousLines(inventoryPanelHeight);
          return;
      }
      UIManager.BlankPreviousLines(inventoryPanelHeight);
      DrawInventory(cursorX, cursorY);
    }
    bool TryGetItem(out Item? item)
    {
      if (character.items.Count > cursorY * 3 + cursorX)
      {
        item = character.items[cursorY * 3 + cursorX];
        return true;
      }
      item = null;
      return false;
    }
  }
  private static int DrawInventory(int cursorX, int cursorY)
  {
    var items = Program.character.items;

    int columns = 3;
    int rows = 10;
    int column_width = 24;
    int namebar_width = 15;
    int amount_width = 4;

    int row_width = 76;

    string partial_delimiter = new string('-', column_width);
    string row_delimiter = $"+{partial_delimiter}+{partial_delimiter}+{partial_delimiter}+";
    string full_delimiter = row_delimiter + partial_delimiter + "+";
    string empty_item = new string(' ', namebar_width) + " | " + new string(' ', amount_width);

    Item selected_item = (items.Count > (cursorY * columns + cursorX)) ? items[cursorY * columns + cursorX] : null;
    bool selected_item_exists = selected_item is not null;

    string selected_item_name = "";
    string[] description = Array.Empty<string>();
    if (selected_item_exists)
    {
      selected_item_name = UIManager.LimitByLength(selected_item.name, column_width - 2);
      description = UIManager.SplitByLength(selected_item.description, column_width - 2);
    }

    if (selected_item_exists)
    {
      Console.WriteLine(full_delimiter);
    }
    else
    {
      Console.WriteLine(row_delimiter);
    }

    for (int r = 0; r < rows; r++)
    {
      for (int c = 0; c < columns; c++)
      {
        Item item = items.Count > r * columns + c ? items[r * columns + c] : null;

        Console.Write("| ");

        ConsoleColor bgc = (cursorY == r && cursorX == c) ? ConsoleColor.DarkGray : ConsoleColor.Black;
        if (item is null)
        {
          UIManager.WriteLeftaligned(empty_item, column_width - 2, ConsoleColor.Gray, bgc);
        }
        else
        {
          string item_name = UIManager.LimitByLength(item.name, namebar_width);
          UIManager.WriteLeftaligned(item_name, namebar_width, ConsoleColor.Gray, bgc);

          Console.Write(" | ");

          string amount = UIManager.LimitByLength(item.amount.ToString(), amount_width);
          UIManager.WriteRightaligned(amount, amount_width, ConsoleColor.Gray, bgc);
        }

        Console.Write(" ");
      }
      if (selected_item_exists)
      {
        Console.Write("| ");
        if (r == 0)
        {
          UIManager.WriteLeftaligned(selected_item_name, column_width - 2);
        }
        else
        {
          if (description.Length > (r * 2 - 2))
          {
            UIManager.WriteLeftaligned(description[r * 2 - 2], column_width - 2);
          }
          else
          {
            Console.Write(new string(' ', column_width - 2));
          }
        }
        Console.WriteLine(" |");
      }
      else
      {
        Console.WriteLine("|");
      }
      if (r != (rows - 1))
      {
        Console.Write(row_delimiter);
        if (selected_item_exists)
        {
          if (r == 0)
          {
            Console.WriteLine(partial_delimiter + "+");
          }
          else
          {
            Console.Write(" ");
            if (description.Length > (r * 2 - 1))
            {
              UIManager.WriteLeftaligned(description[r * 2 - 1], column_width - 2);
            }
            else
            {
              Console.Write(new string(' ', column_width - 2));
            }
            Console.WriteLine(" |");
          }

        }
        else
        {
          Console.WriteLine();
        }
      }
    }
    if (selected_item_exists)
    {
      Console.WriteLine(full_delimiter);
    }
    else
    {
      Console.WriteLine(row_delimiter);
    }


    return 21;
  }
  public static void ImportItem(){
    Console.WriteLine("Path to item:");

    var item_path = Console.ReadLine() ?? "";
    UIManager.BlankPreviousLines(2);

    if (Program.TryReadJson(item_path, out Item item))
    {
        Program.character.items.Add(item);
    }

  }
}

