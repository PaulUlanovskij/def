namespace def;

public static class Dice
{
  public static int Rolld4(int amount = 1){
    return RollDie(4, amount);
  }
  public static int Rolld6(int amount = 1)
  {
    return RollDie(6, amount);
  }
  public static int Rolld8(int amount = 1)
  {
    return RollDie(8, amount);
  }
  public static int Rolld10(int amount = 1)
  {
    return RollDie(10, amount);
  }
  public static int Rolld12(int amount = 1)
  {
    return RollDie(12, amount);
  }
  public static int Rolld20(int amount = 1)
  {
    return RollDie(20, amount);
  }
  private static int RollDie(int sides, int amount)
  {
    if (amount < 1)
    {
      throw new ArgumentOutOfRangeException("can not roll less than one dice");
    }
    int sum = 0;
    for (int x = 0; x < amount; x++)
    {
      sum += Random.Shared.Next(sides) + 1;
    }
    return sum;

  }
}

