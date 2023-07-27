using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server;

public class ChatRoom
{
    public string Name { get; set; }
    public ConcurrentDictionary<Socket, (StreamWriter, string)> Clients { get; }
            = new ConcurrentDictionary<Socket, (StreamWriter, string)>();
}