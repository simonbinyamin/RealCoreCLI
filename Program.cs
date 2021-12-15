using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace realclicore
{
    class Program
    {
        public static List<Choice> _choices;
        static void Main(string[] args)
        {
            _choices = new List<Choice>
            {
                new Choice(".NET6 webapi", async () => await WriteAsync("dotnet new webapi", true)),
                // new Choice(".NET6 mvc", () =>  Write("You")),
                // new Choice(".NET6 console", () =>  Write("Are")),
                
                // new Choice(".NET5 webapi", () => Write("Hi")),
                // new Choice(".NET5 mvc", () =>  Write("You")),
                // new Choice(".NET5 console", () =>  Write("Are")),
                
                // new Choice(".NET6 Layered webapi", () => Write("Hi")),
                // new Choice(".NET6 Layered mvc", () =>  Write("You")),
                // new Choice(".NET6 Layered console", () =>  Write("Are")),
                
                // new Choice(".NET5 Layered webapi", () => Write("Hi")),
                // new Choice(".NET5 Layered mvc", () =>  Write("You")),
                // new Choice(".NET5 Layered console", () =>  Write("Are")),
                
                new Choice(" Exit", () => Environment.Exit(0)),
            };

            int index = 0;
            Console.TreatControlCAsInput = true;

            RunApp(index, _choices, _choices[index]);

            Console.ReadKey();

        }


        static void RunApp(int index, List<Choice> choices, Choice selectedchoice)
        {
			Console.Clear();

            foreach (Choice choice in choices)
            {
                if (choice == selectedchoice)
                {
                    Console.Write("=>");
                }
                else
                {
                    Console.Write("  ");
                }

                Console.WriteLine(choice.Name);
            }
			
            ConsoleKeyInfo keyinfo;

            keyinfo = Console.ReadKey();

            if (keyinfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < _choices.Count)
                {
                    index++;
					RunApp(index, _choices, _choices[index]);
                }
            }
            if (keyinfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 >= 0)
                {
                    index--;
					RunApp(index, _choices, _choices[index]);
                }
            }

            if (keyinfo.Key == ConsoleKey.Enter)
            {
                _choices[index].Selected.Invoke();
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


        static async Task WriteAsync(string cli, bool layered)
        {   Console.WriteLine("Project name:");
            var projname = Console.ReadLine();
            await GetBuild(cli, projname, layered);
            
            Console.Clear();
            Console.WriteLine("done");
            Environment.Exit(0);
        }
        
        public static async Task<string> GetBuild(string cli, string projectname, bool layered)
        {
        try
        {
            if (layered)
            {
                var sln = await RunCli("dotnet new sln --name " + projectname);
                var app = await RunCli(cli + " --output " + projectname);
                if (app == 0)
                {
                    var domain = await RunCli("dotnet new classlib --output " + projectname + "." + "Domain");
                    var data = await RunCli("dotnet new classlib --output " + projectname + "." + "Data");
                    var business = await RunCli("dotnet new classlib --output " + projectname + "." + "Business");

                    if (domain == 0 && data == 0 && business == 0)
                    {

                        var slnProj = await RunCli("dotnet sln add " + projectname + "/" + projectname + ".csproj");
                        var slnData = await RunCli("dotnet sln add " + projectname + ".Data/" + projectname + ".Data.csproj");
                        var slnDomain = await RunCli("dotnet sln add " + projectname + ".Domain/" + projectname + ".Domain.csproj");
                        var slnBusiness = await RunCli("dotnet sln add " + projectname + ".Business/" + projectname + ".Business.csproj");
                        
                    }
                }
            }
            else
            {
                var app = await RunCli(cli + " --output " + projectname);
            }
            return "build";
        }
        catch (Exception)
        {
            return "build failed";
        }
    }

    static Task<int> RunCli(string cmd)
    {
        var tasksource = new TaskCompletionSource<int>();
        var escapingArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapingArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.Exited += (sender, args) =>
         {
             if (process.ExitCode == 0)
                 tasksource.SetResult(0);
             else
                 tasksource.SetResult(process.ExitCode);
             process.Dispose();
         };

        try
        {
            process.Start();
            Console.Write(process.StandardError.ReadToEnd());
            Console.Write(process.StandardOutput.ReadToEnd());
        }
        catch (Exception e)
        {
            tasksource.SetException(e);
            Console.Write(e);
        }

        return tasksource.Task;
    }
    }
}
