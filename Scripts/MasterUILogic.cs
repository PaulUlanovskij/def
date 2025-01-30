namespace def;

public static class MasterUILogic
{
  public static void Start()
  {
    ConnectionManager.Host();
    Thread disconnectionCheck = new Thread(new ThreadStart(ConnectionManager.HostChecksLoop));
    disconnectionCheck.Start();
    Console.Title = "E - send entry; I - send item; M - send message; L - network shutdown";

    while (true)
    {
      ConsoleKey key = Console.ReadKey().Key;
      UIManager.BlankCurrentLine();
      switch (key)
      {
        case ConsoleKey.E:
          {
            SendEntry();
          }
          break;
        case ConsoleKey.I:
          {
            SendItem();
          }
          break;
        case ConsoleKey.M:
          {
            SendMessage();
          }
          break;
        case ConsoleKey.L:
          return;
      }
    }
  }
  private static void SendEntry()
  {
    Console.WriteLine("Await entry:");
    string entry_path = Console.ReadLine() ?? "";
    UIManager.BlankPreviousLines(2);

    if (Program.TryReadJson(entry_path, out Entry[] entry))
    {
        ConnectionManager.Send(DataCodes.Entry, File.ReadAllText(entry_path));
    }
    else
    {
      Console.WriteLine("Failed to parse entry");
    }
  }

  private static void SendItem()
  {
    var players = Program.connected_player_characters;
    if (players.Count == 0)
    {
      return;
    }

    Console.WriteLine("Choose player to send item to");
    int op = UIManager.GetInputOptions(players.Select(x => x.character_name).ToArray());
    string player_name = players[op - 1].character_name;

    UIManager.BlankPreviousLines(players.Count + 1);
    Console.WriteLine("Path to item:");

    var item_path = Console.ReadLine() ?? "";
    UIManager.BlankPreviousLines(2);

    if (Program.TryReadJson(item_path, out Item item))
    {
        ConnectionManager.SendTo(player_name, DataCodes.Item, File.ReadAllText(item_path));
        Console.WriteLine($"Sent {item.name} to {player_name}");
    }
    else
    {
      Console.WriteLine("Failed to parse item");
    }
  }
  private static void SendMessage()
  {
    var players = Program.connected_player_characters;
    if (players.Count == 0)
    {
      return;
    }
    Console.WriteLine("Choose player to whisper to");
    int op = UIManager.GetInputOptions(players.Select(x => x.character_name).ToArray());
    UIManager.BlankPreviousLine();

    ConnectionManager.SendTo(players[op - 1].character_name, DataCodes.Message, Console.ReadLine());
  }

}

