using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace realclicore
{
    class Program
    {
        public static List<Choice> _mainchoices;
        public static List<List<Choice>> _frameworks;
        static void Main(string[] args)
        {
            int index = 0;
            Console.TreatControlCAsInput = true;
            
            _frameworks = new List<List<Choice>> {
                FindList("net6.0"), FindList("net5.0"), FindList("netcoreapp3.1")
            };
         
            _mainchoices = new List<Choice>
            {
                new Choice("[.NET6]", () => RunApp(index, _frameworks[0], _frameworks[0][index])),
                new Choice("[.NET5]", () => RunApp(index, _frameworks[1], _frameworks[1][index])),
                new Choice("[.NET3.1]", () => RunApp(index, _frameworks[2], _frameworks[2][index])),
                new Choice(" Exit", () => Environment.Exit(0)),
            };

            RunApp(index, _mainchoices, _mainchoices[index]);

            Console.ReadKey();

        }


        static async Task CreateAsync(string cli, bool layered)
        {
            string projname = "";
            
            if(layered) {
                Console.WriteLine("Project name:");
                projname = Console.ReadLine();
            }

            await Build.GetBuild(cli, projname, layered);

            Console.Clear();
            Environment.Exit(0);
        }

        static void RunApp(int index, List<Choice> choices, Choice selectedchoice)
        {
            Console.Clear();

            foreach (Choice choice in choices)
            {
                if (choice == selectedchoice)
                {
                    Console.Write("=> ");
                }
                else
                {
                    Console.Write("   ");
                }

                Console.WriteLine(choice.Name);
            }

            ConsoleKeyInfo keyinfo;

            keyinfo = Console.ReadKey();

            if (keyinfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < choices.Count)
                {
                    index++;
                    RunApp(index, choices, choices[index]);
                }
            }
            if (keyinfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 >= 0)
                {
                    index--;
                    RunApp(index, choices, choices[index]);
                }
            }

            if (keyinfo.Key == ConsoleKey.Enter)
            {
                choices[index].Selected.Invoke();
                index = 0;
            }

            if ((keyinfo.Modifiers & ConsoleModifiers.Control) != 0)
            {
                if ((keyinfo.Key & ConsoleKey.C) != 0)
                {
                    Environment.Exit(0);
                }
            }
        }


        
        
        static List<Choice> FindList(string runtime){
            return new List<Choice>
            {
                new Choice(runtime+" webapi", async () => await CreateAsync("dotnet new webapi --framework "+runtime, false)),
                new Choice(runtime+" mvc", async () => await CreateAsync("dotnet new mvc --framework "+runtime, false)),
                new Choice(runtime+" console", async () => await CreateAsync("dotnet new console --framework "+runtime, false)),
                new Choice(runtime+" individual webapi", async () => await CreateAsync("dotnet new webapi --auth Individual --framework "+runtime, false)),
                new Choice(runtime+" individual mvc", async () => await CreateAsync("dotnet new mvc --auth Individual --framework "+runtime, false)),
                new Choice(runtime+" layered webapi", async () => await CreateAsync("dotnet new webapi --framework "+runtime, true)),
                new Choice(runtime+" layered mvc", async () => await CreateAsync("dotnet new mvc --framework "+runtime, true)),
                new Choice(runtime+" layered individual webapi", async () => await CreateAsync("dotnet new webapi --auth Individual --framework "+runtime, true)),
                new Choice(runtime+" layered individual mvc", async () => await CreateAsync("dotnet new mvc --auth Individual --framework "+runtime, true)),
                new Choice("Exit", () => Environment.Exit(0))

            };
        }
    }
}
