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
                Choice.List("net6.0"), 
                Choice.List("net5.0"), 
                Choice.List("netcoreapp3.1")
            };

            _mainchoices = new List<Choice>
            {
                new Choice("[.NET6]", () => InitializeMenu(index, _frameworks[0], _frameworks[0][index])),
                new Choice("[.NET5]", () => InitializeMenu(index, _frameworks[1], _frameworks[1][index])),
                new Choice("[.NET3.1]", () => InitializeMenu(index, _frameworks[2], _frameworks[2][index])),
                new Choice(" Exit", () => Environment.Exit(0)),
            };

            InitializeMenu(index, _mainchoices, _mainchoices[index]);
            Console.ReadKey();

        }


        static void InitializeMenu(int index, List<Choice> choices, Choice selectedchoice)
        {
            Console.Clear();

            foreach (Choice choice in choices)
            {
                Console.Write(choice == selectedchoice? "=> ": "   ");
                Console.WriteLine(choice.Name);

            }

            ConsoleKeyInfo keyinfo;

            keyinfo = Console.ReadKey();

            if (keyinfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < choices.Count)
                {
                    index++;
                    InitializeMenu(index, choices, choices[index]);
                }
            }
            if (keyinfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 >= 0)
                {
                    index--;
                    InitializeMenu(index, choices, choices[index]);
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
    }
}
