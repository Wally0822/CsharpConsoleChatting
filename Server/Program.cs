using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Server;

public class Server
{
    private static readonly ConcurrentDictionary<Socket, (StreamWriter, string)> clients = new();
    private static int clientCounter = 0;

    public static async Task Main()
    {
        Console.Title = "Server";
        Console.WriteLine($"Hello ! I am Server. \nToday : {DateTime.Now:MM-dd}");

        var ipAd = IPAddress.Parse("127.0.0.1");
        TcpListener? tcpListener = null;

        try
        {
            tcpListener = new TcpListener(ipAd, 5001);
            tcpListener.Start();

            while (true)
            {
                var clientSocket = await tcpListener.AcceptSocketAsync();
                var clientNumber = Interlocked.Increment(ref clientCounter);

                _ = Task.Run(async () =>
                {
                    using var stream = new NetworkStream(clientSocket);
                    using var reader = new StreamReader(stream);
                    string messageType = await reader.ReadLineAsync();

                    Console.WriteLine($"Received message type from client {clientNumber}: {messageType}");

                    string nickname = $"Client [{clientNumber}]";
                    switch (messageType)
                    {
                        case "MULTI_CHAT":
                            nickname = await HandleMultiChat(clientSocket, clientNumber);
                            break;
                        case "CHAT_ROOM":
                            await HandleChatRoom(clientSocket, clientNumber);
                            break;
                    }
                });
            }
        }
        finally
        {
            tcpListener?.Stop();
        }
    }

    private static async Task<string> HandleMultiChat(Socket socket, int clientNumber)
    {
        string clientNickname = $"Client [{clientNumber}]";

        try
        {
            using var stream = new NetworkStream(socket);
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            clientNickname = await reader.ReadLineAsync();
            clientNickname ??= $"Client {clientNumber}"; // Null 복합 할당 연산자 (??=)

            Console.WriteLine($"[MultiChat] [{clientNickname}] has joined.");
            foreach (var (clientSocket, (clientWriter, _)) in clients)
            {
                if (clientSocket != socket)
                {
                    await clientWriter.WriteLineAsync($"[{clientNickname}] has joined.");
                }
            }

            clients.TryAdd(socket, (writer, clientNickname));

            string? message;
            while ((message = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine($"[MultiChat] [{DateTime.Now:HH:mm:ss}] [{clientNickname}]: {message}");

                foreach (var (clientSocket, (clientWriter, _)) in clients)
                {
                    if (clientSocket != socket)
                    {
                        await clientWriter.WriteLineAsync($"[{clientNickname}]: {message}");
                    }
                }
            }

            return clientNickname;
        }
        finally
        {
            clients.TryRemove(socket, out _);

            Console.WriteLine($"[MultiChat] [{clientNickname}] has left.");

            foreach (var (clienSocket, (clientWriter, _)) in clients)
            {
                if (clienSocket != socket)
                {
                    await clientWriter.WriteLineAsync($"[{clientNickname}] has left.");
                }
            }

            if (clients.Count == 0)
            {
                Console.WriteLine("[MultiChat] There are no clients currently connected.");
            }

            socket.Close();
        }
    }

    private static readonly ConcurrentDictionary<string, ChatRoom> chatRooms = new();

    private static async Task HandleChatRoom(Socket socket, int clientNumber)
    { 
        string roomName = string.Empty;
        string clientNickname = string.Empty;

        try
        {
            using var stream = new NetworkStream(socket);
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            ChatRoom room;
            string? receivedMessage;
            bool isNicknameReceived = false;

            while ((receivedMessage = await reader.ReadLineAsync()) != null)
            {
                if (receivedMessage.StartsWith("CREATE_ROOM"))
                {
                    roomName = receivedMessage.Split(' ')[1];
                    room = new ChatRoom { Name = roomName };
                    chatRooms.TryAdd(roomName, room);
                    Console.WriteLine($"[ChatRoom] [New Room : {roomName}] has been created.");
                }
                else if (receivedMessage == "LIST_ROOMS")
                {
                    var roomNames = chatRooms.Keys.ToList();
                    var roomNameJson = JsonSerializer.Serialize(roomNames);
                    await writer.WriteLineAsync(roomNameJson);
                }
                else
                {
                    if (isNicknameReceived == false)
                    {
                        clientNickname = receivedMessage;
                        isNicknameReceived = true;
                    }
                    else
                    {
                        roomName = receivedMessage;

                        if (chatRooms.TryGetValue(roomName, out room) == false)
                        {
                            room = new ChatRoom { Name = roomName };
                            chatRooms.TryAdd(roomName, room);
                            Console.WriteLine($"[ChatRoom] [New Room : {roomName}] has been created.");
                        }

                        room.Clients.TryAdd(socket, (writer, clientNickname));
                        Console.WriteLine($"[ChatRoom] [{roomName}][{clientNickname}] has joined.");

                        string? message;
                        while ((message = await reader.ReadLineAsync()) != null)
                        {
                            if (message == "LEAVE_ROOM")
                            {
                                if (chatRooms.TryGetValue(roomName, out room))
                                {
                                    room.Clients.TryRemove(socket, out _);
                                    Console.WriteLine($"[ChatRoom] [ROOM : {roomName}][Client : {clientNickname}] has left.");
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[ROOM : {roomName}] [{DateTime.Now:HH:mm:ss}] [Client : {clientNickname}]: {message}");

                                if (chatRooms.TryGetValue(roomName, out room))
                                {
                                    foreach (var (clientSocket, (clientWriter, otherClientNickname)) in room.Clients)
                                    {
                                        if (clientSocket != socket)
                                        {
                                            await clientWriter.WriteLineAsync($"{otherClientNickname}: {message}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling client {clientNumber}:" + e.Message);
        }
        finally
        {
            if (chatRooms.TryGetValue(roomName, out var room)) 
            {
                room.Clients.TryRemove(socket, out _);
                Console.WriteLine($"[ChatRoom] [{roomName}][{clientNickname}] has left.");
            }
            socket.Close();
        }
    }
}