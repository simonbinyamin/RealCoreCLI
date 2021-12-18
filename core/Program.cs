using System.Collections.Generic;
using System;
using System.Diagnostics;

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

            await GetBuild(cli, projname, layered);

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

                            if (slnProj == 0 && slnData == 0 && slnDomain == 0 && slnBusiness == 0)
                            {

                                var deletefiles = await RunCli("rm realcorecli realcorecli.deps.json realcorecli.pdb realcorecli.dll");
                                if (deletefiles == 0)
                                {
                                    await RunCli("rm realcorecli.runtimeconfig.json");
                                }

                            }
                        }
                    }
                }
                else
                {
                    var app = await RunCli(cli);
                    if (app == 0)
                    {
                        await RunCli("rm realcorecli realcorecli.deps.json realcorecli.pdb realcorecli.runtimeconfig.json realcorecli.dll");
                    }
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
