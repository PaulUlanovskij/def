using System.Net;
using System.Net.Sockets;
using System.Text;
using Mono.Nat;
using System.Text.Json;

namespace def;

public enum DataCodes : byte
{
  Blank = 0,
  Entry = 1,
  Item = 2,
  Message = 3,
}
public static class ConnectionManager
{
  private const int port = 15171;
  private static INatDevice? device;
  private static Mapping connection_map = new Mapping(Protocol.Tcp, port, port);

  private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
  private static List<Socket> connected_sockets = new();
  private static Dictionary<EndPoint, CharacterSheet> ip_to_char_map = new();

  public static async void SendTo(string player_name, DataCodes code, string data)
  {
    foreach (var pair in ip_to_char_map)
    {
      if (pair.Value.character_name == player_name)
      {
        foreach (var s in connected_sockets)
        {
          if (s.LocalEndPoint == pair.Key)
          {
            if (IsSocketConnected(s))
            {
              List<byte> bytes = new();
              bytes.Add((byte)code);
              bytes.AddRange(Encoding.ASCII.GetBytes(data));

              s.Send(bytes.ToArray(), SocketFlags.None);
            }
          }
        }
      }
    }
  }
  public static async void Send(DataCodes code, string data)
  {
    SieveOutDisconnected();
    List<byte> bytes = new();
    bytes.Add((byte)code);
    bytes.AddRange(Encoding.ASCII.GetBytes(data));
    connected_sockets.ForEach(x => x.Send(bytes.ToArray(), SocketFlags.None));
  }
  public static void Receive()
  {
    byte[] buffer = new byte[4096 * 16];
    int len = socket.Receive(buffer);
    if (len == 0)
    {
      return;
    }

    switch ((DataCodes)buffer[0])
    {
      case DataCodes.Blank:
        break;
      case DataCodes.Entry:
        {
          Program.TryParseJson(new string(Encoding.ASCII.GetChars(buffer[1..len])), out Entry[] entries);
          PlayerUILogic.PlayOutEntries(entries);
          break;
        }
    }
  }

  public static bool Join()
  {
    Console.WriteLine(">>> Enter office network ip: ");
    string ip = Console.ReadLine() ?? "";
    UIManager.BlankPreviousLine();
    UIManager.BlankPreviousLine();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[SYSTEM]Trying to establish connection to {0}", ip);
    try
    {
      socket.Connect(ip, port);
    }
    catch
    {
      Console.WriteLine($"[SYSTEM]Failed to connect to ip {ip}");
      return false;
    }
    UIManager.BlankPreviousLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("[SYSTEM]Connection established");
    Console.ForegroundColor = ConsoleColor.White;
    socket.Send(Encoding.ASCII.GetBytes(Program.SerializeJson(Program.character)));
    return true;
  }
  public static void HostChecksLoop()
  {
    byte[] buffer = new byte[4096 * 16];
    while (Program.run_supportThreads)
    {
      try
      {
        SieveOutDisconnected();
        Socket client = socket.Accept();

        connected_sockets.Add(client);

        var len = client.Receive(buffer, SocketFlags.None);
        Program.TryParseJson(new string(Encoding.ASCII.GetChars(buffer[0..len])), out CharacterSheet character);
        Console.WriteLine($"[SYSTEM]{character.character_name} connected! from {client.LocalEndPoint}");
        ip_to_char_map.Add(client.LocalEndPoint, character);
        Program.connected_player_characters.Add(character);

      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
      }
      Thread.Sleep(5000);
    }
  }
  public static bool IsSocketConnected(Socket socket)
  {
    try
    {
      if (socket == null || !socket.Connected)
        return false;

      if (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0)
        return false;

      return true;
    }
    catch
    {
      return false;
    }
  }
  private static void SieveOutDisconnected()
  {
    for (int i = connected_sockets.Count - 1; i >= 0; i--)
    {
      Socket s = connected_sockets[i];
      if (!IsSocketConnected(s))
      {
        var charac = ip_to_char_map[s.LocalEndPoint];
        Program.connected_player_characters.Remove(charac);
        Console.WriteLine($"[SYSTEM]{charac.character_name} disconnected.");
        ip_to_char_map.Remove(s.LocalEndPoint);
        connected_sockets.RemoveAt(i);
      }
    }
  }
  public static void Host()
  {
    socket.Bind(new IPEndPoint(IPAddress.Any, port));
    socket.Listen();
  }
  public static void ClosePortForwart()
  {
    if (socket.Connected)
    {
      socket.Shutdown(SocketShutdown.Both);
      socket.Close();
      socket.Dispose();
    }
    connected_sockets.Clear();
    device?.DeletePortMap(connection_map);
  }
  public static bool SetupPortForward()
  {
    bool portMapSet = false;
    bool fail = false;

    NatUtility.DeviceFound += async (object sender, DeviceEventArgs args) =>
    {
      try
      {
        device = args.Device;

        UIManager.BlankPreviousLine();
        Console.ForegroundColor = ConsoleColor.Green;

        Console.Write(">>> Gateway found: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(device.NatProtocol);
        Console.WriteLine(">>> Gateway type: {0}", device.GetType().Name);
        Console.WriteLine(">>> Gateway IP: {0}", await device.GetExternalIPAsync());
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(">>> Hjacking gateway...");
        //Thread.Sleep(100);

        var res = await device.CreatePortMapAsync(connection_map);

        UIManager.BlankPreviousLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($">>> Hjacked gateway successfully");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($">>> gateway opened publicly at port {res.PublicPort}");
        Console.WriteLine($">>> gateway opened privatly at port {res.PrivatePort}");

        portMapSet = true;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[SYSTEM] Connection established");
      }
      catch (Exception e)
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[SYSTEM]: Critical error occured. Shutdown sequence activated.");
        Console.WriteLine(e.Message);
        fail = true;
      }
    };
    Console.WriteLine("[SYSTEM] Connection sequence initialized");
    //Thread.Sleep(100);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(">>> Looking for gateway...");
    NatUtility.StartDiscovery();
    while (true)
    {
      if (portMapSet || fail) break;
      Thread.Sleep(1);
    }

    return !fail;
  }

  public static void ReceiveCheckLoop()
  {
    while (Program.run_supportThreads)
    {
      try
      {
        Receive();
      }
      catch (SocketException e)
      {
        if (e.SocketErrorCode != SocketError.ConnectionAborted)
        {
          throw e;
        }
      }
      catch (Exception e)
      {
        throw e;
      }
      Thread.Sleep(1000);
    }
  }
}
