namespace NwSearch.Entities
{
    
    public class Keyword
    {
        public string Name { get; set; } = "";

        public int Score { get; set; } = 1;

        public Keyword()
        { }

        public Keyword(string name)
        {
            Name = name;
        }

        public Keyword(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }
}


