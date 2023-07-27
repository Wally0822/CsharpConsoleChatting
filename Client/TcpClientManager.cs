using System.ComponentModel.Design;
using System.Net.Sockets;
using System.Text.Json;

namespace Client;

public class TcpClientManager
{
    private TcpClient _client;
    private StreamWriter _writer;
    private StreamReader _reader;

    public TcpClientManager()
    {
        _client = new TcpClient();
    }

    public async Task InitializeConnection()
    {
        try
        {
            await _client.ConnectAsync("localhost", 5001);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Connectiong to Server : " + e.Message);
            throw;
        }

        var stream = _client.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
    }

    public async Task SendMode(string mode)
    {
        try
        {
            if(_client == null || _client.Connected == false)
            {
                await InitializeConnection();
            }

            await _writer.WriteLineAsync(mode);
        }
        catch(Exception e) 
        {
            Console.WriteLine($"Error sending mode to server: {e.Message}");
        }
    }

    public async Task MultiChatting(string nickname)
    {
        if (_client == null || _client.Connected == false)
        {
            await InitializeConnection();
        }

        await _writer.WriteLineAsync(nickname);

        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var message = await _reader.ReadLineAsync();
                    if (message == null)
                    {
                        break;
                    }

                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading message : {e.Message}");
                    break;
                }
            }
        });

        string? dataToSend;
        while ((dataToSend = Console.ReadLine()) != "<EOF>")
        {
            try
            {
                var timestamp = DateTime.Now;
                var formattedMessage = $"[{timestamp}] {dataToSend}";
                await _writer.WriteLineAsync($"{dataToSend}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending meaaged : {e.Message}");
                break;
            }
        }
    }

    public async Task CreateChatRoom(string roomName)
    {
        try
        {
            if (_client == null || _client.Connected == false)
            {
                await InitializeConnection();
            }

            await _writer.WriteLineAsync($"CREATE_ROOM {roomName}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending mode to server: {e.Message}");
        }
    }

    public async Task<string[]> GetChatRoomList()
    {
        await _writer.WriteLineAsync("LIST_ROOMS");
        string response = await _reader.ReadLineAsync();

        return JsonSerializer.Deserialize<string[]>(response);
    }

    public async Task JoinChatRoom(string nickname, string roomName)
    {
        try
        {
            Console.WriteLine("Press the 'Esc' key to leave the chat room at any time.");
            Console.WriteLine("Chatting Start ! ");

            if (_client == null || _client.Connected == false)
            {
                await InitializeConnection();
            }

            await _writer.WriteLineAsync($"{nickname}");
            await _writer.WriteLineAsync($"{roomName}");

            CancellationTokenSource cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var message = await _reader.ReadLineAsync();
                        if (message == null)
                        {
                            break;
                        }

                        Console.WriteLine(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error ChatRoom message : {e.Message}");
                        break;
                    }
                }
            });

            string dataToSend = "";
            while (!cts.IsCancellationRequested)
            {
                var keyInfo = Console.ReadKey(true);

                if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape)
                {
                    dataToSend += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
                else if(keyInfo.Key == ConsoleKey.Enter)
                {
                    try
                    {
                        Console.WriteLine();
                        var timestamp = DateTime.Now;
                        var formattedMessage = $"[{timestamp}] {dataToSend}";
                        await _writer.WriteLineAsync($"{dataToSend}");
                        dataToSend = "";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error sending message : {e.Message}");
                        break;
                    }
                }
                else if(keyInfo.Key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                    break;
                }
            }

            if (cts.IsCancellationRequested)
            {
                await _writer.WriteLineAsync("LEAVE_ROOM");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error joining chat room: {e.Message}");
        }
        finally
        {
            StartScene startScene = new StartScene();
            await startScene.Start();
        }
    }
}