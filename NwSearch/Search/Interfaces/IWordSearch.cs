namespace NwSearch.Search;

public interface IWordSearch
{
    IEnumerable<string> WordsSeparator { get; set; }

    public IEnumerable<string> FindMultiWords(IEnumerable<string> words, int minWordsCount = 2);
}
