using NwSearch.Entities;

namespace NwSearch.Search;

/// <summary>
/// Поиск по одному из ключевых слов в SearchItems.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SearchBySingleKeyword<T> : ISearchBySingleKeyword<T>
{
    private readonly List<SearchItem<T>> _searchItems = new List<SearchItem<T>>();

    public ITextSearch TextSearch { get; }

    public IEnumerable<SearchItem<T>> SearchItems => _searchItems.ToArray();
            
    public SearchItem<T>? DefaultSearchItem { get; set; }

    public SearchBySingleKeyword(ITextSearch textSearch)
    {
        TextSearch = textSearch;
    }

    public SearchBySingleKeyword(
        ITextSearch textSearch,
        IEnumerable<SearchItem<T>> searchItems)
    {
        TextSearch = textSearch;
        _searchItems.AddRange(searchItems);
    }

    public void AddSearchItems(IEnumerable<SearchItem<T>> searchItems)
        => _searchItems.AddRange(searchItems);

    public void AddSearchItem(SearchItem<T> searchItem)
        => _searchItems.Add(searchItem);

    public void RemoveSearchItem(SearchItem<T> searchItem)
        => _searchItems.RemoveAll(searchItemIter => searchItemIter == searchItem);


    /// <summary>
    /// Находит searchItem, если в тексте есть одно из его ключевых слов(Keyword) со Score больше 0. 
    /// Если есть несколько подходящих SearchItem, то возвращает у кого больше приоритет(Priority).
    /// </summary>
    /* Алгоритм работы:
    1. Получаем названия всех ключевых слов, которые есть в _searchItems
    2. Находим название ключевых слов, которые есть в тексте.
    3. Находим все searchItems, в которых есть ключевые слова с названиеями, которые нашли ранее.
    4. searchItems с самым большим приоритетом.
    5. Формируем результат поиска. */
    public SearchResult<T> Find(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return new SearchResult<T>() { SearchItem = DefaultSearchItem };
        }
        
        // 1
        var keywordsNames = _searchItems
            .SelectMany(searchItems => searchItems.Keywords.Select(keyword => keyword.Name));
        // 2
        var containedKeywordsNames = TextSearch.FindContainedWords(text, keywordsNames);
        // 3
        var containedSearchItems = _searchItems
            .Where(searchItem => searchItem.Priority > 0)
            .Where(searchItem => searchItem.Keywords
                .Any(keyword => containedKeywordsNames.Any(textword => keyword.Name == textword && keyword.Score > 0)));
        // 4
        var resultSearchItem = containedSearchItems
            .OrderBy(searchItem => searchItem.Priority)
            .LastOrDefault() ?? DefaultSearchItem;
        // 5
        SearchResultStatus searchResultStatus = (containedSearchItems.Any()) ? SearchResultStatus.Success : SearchResultStatus.Empty;
        
        var containedKeywords = resultSearchItem?.Keywords
            .Where(keyword => containedKeywordsNames
                .Any(keywordName => keywordName == keyword.Name));
        
        return new SearchResult<T>(searchResultStatus,
                                resultSearchItem,
                                containedKeywords?.Sum(keyword => keyword.Score) ?? 0,
                                containedKeywords);
    }

    /// <summary>
    /// Находит все searchItem, если в тексте есть одно из его ключевых слов(Keyword) со Score больше 0. 
    /// </summary>
    public IEnumerable<SearchResult<T>> FindAll(string text)
    {
        var searchResults = new List<SearchResult<T>>();
        
        if (String.IsNullOrEmpty(text))
        {
            return searchResults;
        }

        var keywordsNames = _searchItems
            .SelectMany(searchItems => searchItems.Keywords.Select(keyword => keyword.Name));

        var containedKeywordsNames = TextSearch.FindContainedWords(text, keywordsNames);

        var containedSearchItems = _searchItems
            .Where(searchItem => searchItem.Priority > 0)
            .Where(searchItem => searchItem.Keywords
                .Any(keyword => containedKeywordsNames.Any(textword => keyword.Name == textword && keyword.Score > 0)));

        foreach (var searchItem in containedSearchItems)
        {
            var containedKeywords = searchItem.Keywords
                .Where(keyword => containedKeywordsNames.Any(keywordName => keywordName == keyword.Name));

            searchResults.Add(
                new SearchResult<T>(SearchResultStatus.Success,
                                    searchItem,
                                    containedKeywords.Sum(keyword => keyword.Score),
                                    containedKeywords));
        }

        return searchResults;
    }
}
