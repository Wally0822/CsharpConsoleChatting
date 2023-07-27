using Microsoft.VisualBasic;

namespace Client;

public class StartScene
{
    public async Task Start()
    {
        Console.Title = "Console Chatting : Client";
        await RunMainMenu();
    }

    private async Task RunMainMenu()
    {
        string prompt = @"


                     _____                      _         _____ _           _   _   _             
                    / ____|                    | |       / ____| |         | | | | (_)            
                   | |     ___  _ __  ___  ___ | | ___  | |    | |__   __ _| |_| |_ _ _ __   __ _ 
                   | |    / _ \| '_ \/ __|/ _ \| |/ _ \ | |    | '_ \ / _` | __| __| | '_ \ / _` |
                   | |___| (_) | | | \__ \ (_) | |  __/ | |____| | | | (_| | |_| |_| | | | | (_| |
                    \_____\___/|_| |_|___/\___/|_|\___|  \_____|_| |_|\__,_|\__|\__|_|_| |_|\__, |
                                                                                             __/ |
                                                                                            |___/                                  
                    Use the ↑,↓ keys to cycle through options and pressed enter to select an option.



";
        string[] options = { "Multi Chat", "Chat Room", "Exit" };
        Menu mainMenu = new Menu(prompt, options);
        int selectedIndex = mainMenu.Run();

        switch (selectedIndex)
        {
            case 0:
                await StartMultiChatting();
                break;
            case 1:
                await StartChatRoom();
                break;
            case 2:
                ExitChatting();
                break;
        }
    }

    private async Task StartMultiChatting()
    {
        TcpClientManager clientManager = new TcpClientManager();
        await clientManager.InitializeConnection();
        await clientManager.SendMode("MULTI_CHAT");

        Console.Clear();
        Console.Write("Enter your nickname for multi chat : ");
        string nickname = Console.ReadLine();

        await clientManager.MultiChatting(nickname);
    }

    private async Task StartChatRoom()
    {
        TcpClientManager tcpClientManager = new TcpClientManager();
        await tcpClientManager.InitializeConnection();
        await tcpClientManager.SendMode("CHAT_ROOM");

        Console.Clear();
        Console.Write("Enter Room Name : ");
        string roomName = Console.ReadLine();

        Console.Write("Enter nickname : ");
        string nickname = Console.ReadLine();

        await tcpClientManager.CreateChatRoom(roomName);

        string[] chatRooms;
        do
        {
            await Task.Delay(1000); 
            chatRooms = await tcpClientManager.GetChatRoomList();
        } while (!chatRooms.Contains(roomName));

        Console.WriteLine("Chat Room List:");
        for (int i = 0; i < chatRooms.Length; i++)
        {
            Console.WriteLine($"{i + 1}: {chatRooms[i]}");
        }

        Console.Write("Select the chat room to join: ");
        int selectedIndex = int.Parse(Console.ReadLine());

        roomName = chatRooms[selectedIndex - 1];
        await tcpClientManager.JoinChatRoom(nickname, roomName);
    }

    private void ExitChatting()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey(true);
        Environment.Exit(0);
    }
}