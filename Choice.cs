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
    }
}