using System.Text.Json;

namespace def;

class Program
{
  public static CharacterSheet character = new();
  public static List<CharacterSheet> connected_player_characters = new();
  public static string character_file_path = "joe.json";

  public static JsonSerializerOptions jso = new() { AllowTrailingCommas = true, IncludeFields = true, ReadCommentHandling = JsonCommentHandling.Skip };

  public static bool run_supportThreads;
  static void Main(string[] args)
  {
    if (ConnectionManager.SetupPortForward())
    {
      run_supportThreads = true;
      int op = UIManager.GetInputOptions(new string[] { "Host office network", "Join office network", "Edit Character" });
      if (op == 1)
      {
        MasterUILogic.Start();
      }
      else if (op == 2)
      {
        PlayerUILogic.Start();
        WriteJson(character_file_path, character);
      }
      else if (op == 3)
      {
        CharacterEditLogic.Start();
        WriteJson(character_file_path, character);
      }
      run_supportThreads = false;
      ConnectionManager.ClosePortForwart();

    }
    else
    {
      System.Console.WriteLine("Failed to port forward");
    }
  }
  public static bool TryReadJson<T>(string path, out T parsed)
  {
    parsed = default;
    if (File.Exists(path))
    {
      return TryParseJson(File.ReadAllText(path), out parsed);
    }
    return false;
  }
  public static bool TryParseJson<T>(string json, out T parsed)
  {
    parsed = default;
      try
      {
        parsed = JsonSerializer.Deserialize<T>(json, jso);
        return true;
      }
      catch
      {
        return false;
      }
    return false;
  }
  public static void WriteJson<T>(string path, T obj)
  {
    File.WriteAllText(path, JsonSerializer.Serialize(obj, jso));
  }
  public static string SerializeJson<T>(T obj)
  {
    return JsonSerializer.Serialize(obj, jso);
  }
}
