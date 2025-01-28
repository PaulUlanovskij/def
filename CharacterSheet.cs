namespace def;

public class Item{
  public string name = "";
  public string description = "";
  public int amount = 0;
}
public class CharacterSheet
{
  //General info
  public string character_name = "";
  public string gender = "";
  public int age = 0;
  
  //Stats and mods
  public int[] stats = new int[24];
  public int[] mods = new int[24];

  //Style
  public string clothes = "";
  public string hair = "";
  public string affectations = "";
  public string ethnicity = "";
  public string language = "";

  //Family
  public string family_background = "";
  public List<string> siblings = new ();
  
  //Motivations
  public string traits = "";
  public string valued_person = "";
  public string value_most = "";
  public string feel_about_people = "";
  public string valued_posessions = "";

  //Life
  public List<string> life_events = new();

  //Items
  public List<Item> items = new();

  public static string IndexToName(int index)
  {
    return index switch
    {
      0 => "LOGic",
      1 => "ENCyclopedia",
      2 => "RHEtoric",
      3 => "DRAma",
      4 => "CONceptualization",
      5 => "VISual Calculus",
      6 => "VOLition",
      7 => "INLand empire",
      8 => "EMPathy",
      9 => "AUThority",
      10 => "ESPirit de corps",
      11 => "SUGgestion",
      12 => "ENDurance",
      13 => "PAIn threshold",
      14 => "PHYsical instrument",
      15 => "ELEctro- chemistry",
      16 => "SHIvers",
      17 => "HALf light",
      18 => "HANd-eye coordination",
      19 => "PERception",
      20 => "REAction speed",
      21 => "SAViour faire",
      22 => "INTerfacing",
      23 => "COMposure",
      _ => "FAILURE"
    };
  }
  public static int NameToIndex(string name)
  {
    return name switch
    {
      "LOGic" => 0,
      "ENCyclopedia" => 1,
      "RHEtoric" => 2,
      "DRAma" => 3,
      "CONceptualization" => 4,
      "VISual Calculus" => 5,
      "VOLition" => 6,
      "INLand empire" => 7,
      "EMPathy" => 8,
      "AUThority" => 9,
      "ESPirit de corps" => 10,
      "SUGgestion" => 11,
      "ENDurance" => 12,
      "PAIn threshold" => 13,
      "PHYsical instrument" => 14,
      "ELEctro- chemistry" => 15,
      "SHIvers" => 16,
      "HALf light" => 17,
      "HANd-eye coordination" => 18,
      "PERception" => 19,
      "REAction speed" => 20,
      "SAViour faire" => 21,
      "INTerfacing" => 22,
      "COMposure" => 23,
      _ => -1,
    };
  }
  public static ConsoleColor GetStatColor(int index)
  {
    if (index < 6)
    {
      return ConsoleColor.Cyan;
    }
    else if (index < 12)
    {
      return ConsoleColor.DarkMagenta;
    }
    else if (index < 18)
    {
      return ConsoleColor.Red;
    }
    else if (index < 24)
    {
      return ConsoleColor.Yellow;
    }
    return ConsoleColor.White;
  }
}

