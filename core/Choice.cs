namespace realclicore
{
    public class Choice
    {
        public string Name { get; }
        public Action Selected { get; }

        public Choice(string name, Action selected)
        {
            Name = name;
            Selected = selected;
        }
        
        private static async Task CreateAsync(string cli, bool layered)
        {
            string projname = "";

            if (layered)
            {
                Console.WriteLine("Project name:");
                projname = Console.ReadLine();
            }

            await Build.GetBuild(cli, projname, layered);

            Console.Clear();
            Environment.Exit(0);
        }
        
        public static List<Choice> List(string runtime)
        {
            return new List<Choice>
            {
                new Choice(runtime+" webapi", async () => await CreateAsync("dotnet new webapi --framework "+runtime, false)),
                new Choice(runtime+" mvc", async () => await CreateAsync("dotnet new mvc --framework "+runtime, false)),
                new Choice(runtime+" console", async () => await CreateAsync("dotnet new console --framework "+runtime, false)),
                new Choice(runtime+" individual mvc", async () => await CreateAsync("dotnet new mvc --auth Individual --framework "+runtime, false)),
                new Choice(runtime+" layered webapi", async () => await CreateAsync("dotnet new webapi --framework "+runtime, true)),
                new Choice(runtime+" layered mvc", async () => await CreateAsync("dotnet new mvc --framework "+runtime, true)),
                new Choice(runtime+" layered individual mvc", async () => await CreateAsync("dotnet new mvc --auth Individual --framework "+runtime, true)),
                new Choice("Exit", () => Environment.Exit(0))

            };
        }
        
    }
}