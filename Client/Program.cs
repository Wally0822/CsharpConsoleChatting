using System.Net.Sockets;

namespace Client
{
    public class TcpClientManager
    {
        public static async Task Main()
        {
            StartScene startScene = new StartScene();
            startScene.Start();

            using var client = new TcpClient();
            await client.ConnectAsync("localhost", 5001);

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            var nickname = Console.ReadLine();
            await writer.WriteLineAsync(nickname);

            _ = Task.Run(async () =>
            {
                while(true)
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null)
                    {
                        break;
                    }

                    Console.WriteLine(message);
                }
            });

            string? dataToSend;
            while ((dataToSend = Console.ReadLine()) != "<EOF>")
            {
                await writer.WriteLineAsync($"{dataToSend}");
            }
        }
    }
}