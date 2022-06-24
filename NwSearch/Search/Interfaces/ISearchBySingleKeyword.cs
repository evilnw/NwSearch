using NwSearch.Entities;

namespace NwSearch.Search;

public interface ISearchBySingleKeyword<T>
{
    IEnumerable<SearchItem<T>> SearchItems { get; }

    void AddSearchItems(IEnumerable<SearchItem<T>> searchItems);

    void AddSearchItem(SearchItem<T> searchItem);

    void RemoveSearchItem(SearchItem<T> searchItem);

    SearchResult<T> Find(string text);

    IEnumerable<SearchResult<T>> FindAll(string text);
}
