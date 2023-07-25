namespace Client;

public class StartScene
{
    public void Start()
    {
        Console.Title = "Console Chatting : Client";
        RunMainMenu();
    }

    private void RunMainMenu()
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
        string[] options = { "Chat", "Exit" };
        Menu mainMenu = new Menu(prompt, options);
        int selectedIndex = mainMenu.Run();

        switch (selectedIndex)
        {
            case 0:
                StartChatting();
                break;
            case 1:
                ExitChatting();
                break;
        }
    }

    private void ExitChatting()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey(true);
        Environment.Exit(0);
    }

    private void StartChatting()
    {
        Console.Clear();
        Console.Write("Enter your nickname: ");
    }
}