namespace def;

public class Entry
{
  public int stat = 0;
  public int target = 0;
  public string type = "";
  public string text = "";

  public int init_fail = int.MaxValue;
  public int success = int.MaxValue;
  public int fail = int.MaxValue;
  
  public EntryResolution[] options = Array.Empty<EntryResolution>();
}
public class EntryResolution{
  public string text = "";
  public int link = int.MaxValue;

}

