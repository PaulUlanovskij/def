namespace def;

public static class CharacterEditLogic {
  public static void Start()
  {

    PlayerUILogic.LoadCharacter();


    Console.Title = "S - show stats; R - roll; I - inventory; L - leave;";

    while (true)
    {
      ConsoleKey input = Console.ReadKey().Key;
      UIManager.BlankCurrentLine();
      switch (input)
      {
        case ConsoleKey.S:
          PlayerUILogic.OpenStatsMenu();
          break;
        case ConsoleKey.R:
          UIManager.RollDiceMenu();
          break;
        case ConsoleKey.I:
          PlayerUILogic.OpenInventory();
          break;
        case ConsoleKey.L:
          return;
      }
      Console.Title = "S - show stats; R - roll; I - inventory; L - leave;";
    }
  }
 
}
