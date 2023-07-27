using System.Net.Sockets;
using System.Text.Json;

namespace Client;

class Program
{
    static async Task Main(string[] args)
    {
        StartScene startScene = new StartScene();
        await startScene.Start();
    }
}