using NwSearch.Entities;

namespace NwSearch.Search;

public interface ISubstringSearch
{
    SearchResult<string> FindSubstring(string text);
}
