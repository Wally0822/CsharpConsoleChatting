using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client;

public class Menu
{
    private int selectedIndex;
    private string[] options;
    private string prompt;

    public Menu(string prompt, string[] options)
    {
        this.prompt = prompt;
        this.options = options;
        selectedIndex = 0;
    }

    private void DisplayOptions()
    {
        Console.Write(prompt);

        for(int i = 0; i < options.Length; i ++)
        {
            string currentOption = options[i];
            string prefix;

            Console.ForegroundColor = i == selectedIndex ? ConsoleColor.Black : ConsoleColor.White;
            Console.BackgroundColor = i == selectedIndex ? ConsoleColor.White : ConsoleColor.Black;

            prefix = i == selectedIndex ? "*" : " ";

            Console.WriteLine($"                                                  {prefix} >> {currentOption} ");
        }

        Console.ResetColor();
    }

    public int Run()
    {
        ConsoleKey keyPressed;

        do
        {
            Console.Clear();
            DisplayOptions();

            var keyInfo = Console.ReadKey(true);
            keyPressed = keyInfo.Key;

            if(keyPressed == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex -1 + options.Length) % options.Length;
            }
            else if (keyPressed == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex + 1) % options.Length;
            }

        } while (keyPressed != ConsoleKey.Enter);

        return selectedIndex;
    }
}