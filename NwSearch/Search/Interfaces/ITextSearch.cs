using System.Collections.Generic;

namespace NwSearch.Search;

public interface ITextSearch
{
    IEnumerable<string> WordsSeparator { get; set; }

    IEnumerable<string> FindContainedWords(string text, IEnumerable<string> words);

    IEnumerable<int> AllIndexOfByEndSeparator(string text, string substring);

    IEnumerable<int> AllIndexOfBySeparator(string text, string substring);

    Dictionary<string, List<int>> CreateSubstringIndexesBySeparator(string text, IEnumerable<string> substringsArray);

    Dictionary<string, List<int>> CreateSubstringIndexesByEndSeparator(string text, IEnumerable<string> substringsArray);
}
