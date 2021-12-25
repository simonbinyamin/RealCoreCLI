using System.Diagnostics;
using System.Runtime.InteropServices;

class Build
{
    public static async Task<string> GetBuild(string cli, string projectname, bool layered)
    {
        string deletefilesCLI = "";
        string deletelastfileCLI = "";
        Func<string, Task<int>> platform;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            deletefilesCLI ="rm realcorecli.dll realcorecli.pdb";
            deletelastfileCLI = "rm realcorecli.runtimeconfig.json";
            platform = RunCLILinux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            deletefilesCLI ="del /f realcorecli.dll realcorecli.pdb";
            deletelastfileCLI = "del /f realcorecli.runtimeconfig.json";
            platform = RunCLIWin;
        }
        else
        {
            deletefilesCLI ="rm realcorecli.dll realcorecli.pdb";
            deletelastfileCLI = "rm realcorecli.runtimeconfig.json";
            platform = RunCLILinux;
        }

        try
        {
            if (layered)
            {
                var sln = await platform("dotnet new sln --name " + projectname);
                var app = await platform(cli + " --output " + projectname);
                if (app == 0)
                {
                    var domain = await platform("dotnet new classlib --output " + projectname + "." + "Domain");
                    var data = await platform("dotnet new classlib --output " + projectname + "." + "Data");
                    var business = await platform("dotnet new classlib --output " + projectname + "." + "Business");

                    if (domain == 0 && data == 0 && business == 0)
                    {

                        var slnProj = await platform("dotnet sln add " + projectname + "/" + projectname + ".csproj");
                        var slnData = await platform("dotnet sln add " + projectname + ".Data/" + projectname + ".Data.csproj");
                        var slnDomain = await platform("dotnet sln add " + projectname + ".Domain/" + projectname + ".Domain.csproj");
                        var slnBusiness = await platform("dotnet sln add " + projectname + ".Business/" + projectname + ".Business.csproj");

                        if (slnProj == 0 && slnData == 0 && slnDomain == 0 && slnBusiness == 0)
                        {

                            var deletefiles = await platform(deletefilesCLI);
                            if (deletefiles == 0)
                            {
                                await platform(deletelastfileCLI);
                            }

                        }
                    }
                }
            }
            else
            {
                var app = await platform(cli);
                if (app == 0)
                {
                    var deletefile = await platform(deletefilesCLI);
                    if(deletefile == 0) {
                        await platform(deletelastfileCLI);
                    }
                }
            }
            return "build";
        }
        catch (Exception)
        {
            return "build failed";
        }
    }

    static Task<int> RunCLIWin(string cmd)
    {
        var tasksource = new TaskCompletionSource<int>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                FileName = "cmd.exe",
                Arguments = "/c " + cmd
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

    static Task<int> RunCLILinux(string cmd)
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