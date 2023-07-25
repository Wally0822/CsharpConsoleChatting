using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Server;

public class Server
{
    private static readonly ConcurrentDictionary<Socket, (StreamWriter, string)> clients = new();
    private static int clientCounter = 0;

    public static async Task Main()
    {
        Console.Title = "Server";

        var ipAd = IPAddress.Parse("127.0.0.1");
        TcpListener? tcpListener = null;

        try
        {
            tcpListener = new TcpListener(ipAd, 5001);
            tcpListener.Start();

            while(true)
            {
                var clientSocket = await tcpListener.AcceptSocketAsync();
                var clientNumber = Interlocked.Increment(ref clientCounter);
                _ = Task.Run(() => HandleClient(clientSocket, clientNumber));
            }
        }
        finally
        {
            tcpListener?.Stop();
        }
    }

    private static async Task HandleClient(Socket socket, int clientNumber)
    {
        using var stream = new NetworkStream(socket);
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true}; 

        var clientNickname = await reader.ReadLineAsync();
        clientNickname ??= $"Client {clientNumber}"; // Null 복합 할당 연산자 (??=)
        //clientNickname = clientNickname ?? $"Client {clientNumber}"; // Null 조건부 연산자 (??)
        clients.TryAdd(socket, (writer, clientNickname));

        try
        {
            string? message;
            while((message = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Client {clientNickname}]: {message}");
                
                foreach (var (clientSocket, (clientWriter, _)) in clients)
                {
                    if(clientSocket != socket)
                    {
                        await clientWriter.WriteLineAsync($"{clientNickname}: {message}");
                    }
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine($"Server Exception : {e.Message}");
        }
        finally
        {
            clients.TryRemove(socket, out _);
            socket.Close();
        }
    }
}