using NwSearch.Entities;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NwSearch.Search
{
    /// <summary>
    /// По ключевым словам ищет подходящие SearchItems
    /// </summary>
    public class SearchByKeywords : ISearchByKeywords
    {
        private readonly List<SearchItem<string>> _searchItems = new List<SearchItem<string>>();

        public ITextSearch TextSearch { get; }

        /// <summary>
        /// минимальная сумма очков(Score) в SearchItem, чтобы результат считался успешным.
        /// </summary>
        public int MinAmountScores { get; set; }

        /// <summary>
        /// Список SearchItems среди которых осуществляется поиск.
        /// </summary>
        public IEnumerable<SearchItem<string>> SearchItems  => _searchItems.ToArray();
        
        public SearchByKeywords(
            ITextSearch textSearch, 
            int minAmountScores)
        {
            TextSearch = textSearch;
            MinAmountScores = minAmountScores;
        }

        public SearchByKeywords(
            ITextSearch textSearch,
            IEnumerable<SearchItem<string>> searchItems, 
            int minAmountScores)
        {
            _searchItems.AddRange(searchItems);
            TextSearch = textSearch;
            MinAmountScores = minAmountScores;
        }

        public void AddSearchItems(IEnumerable<SearchItem<string>> searchItems)
            => _searchItems.AddRange(searchItems);

        public void AddSearchItem(SearchItem<string> searchItem)
            => _searchItems.Add(searchItem);

        public void RemoveSearchItem(SearchItem<string> searchItem)
            => _searchItems.RemoveAll(searchItemIter => searchItemIter == searchItem);

        /// <summary>
        /// Находит все подходящие SearchItems, которые удоволетворяют поиску по ключевым словам.
        /// </summary>
        /// <param name="keywords">список ключевых слов, по которым осуществлять поиск</param>
        public IEnumerable<SearchResult<string>> FindAll(IEnumerable<Keyword> keywords)
        {
            var searchResults = new List<SearchResult<string>>();
            var keywordsNames = keywords
                .Where(keyword => !String.IsNullOrEmpty(keyword.Name))
                .Select(keyword => keyword.Name);

            foreach (var searchItem in _searchItems)
            {                
                if (String.IsNullOrEmpty(searchItem.Value) || searchItem.Priority <= 0)
                {
                    continue;
                }
                
                var containedKeywordsNames = TextSearch.FindContainedWords(searchItem.Value, keywordsNames);
                var containedKeywords = keywords
                    .Where(keyword => containedKeywordsNames
                        .Any(keywordName => keyword.Name == keywordName));
                int amountScores = containedKeywords.Sum(keyword => keyword.Score);

                if (amountScores < MinAmountScores)
                {
                    continue;
                }
                
                searchResults.Add(
                    new SearchResult<string>(SearchResultStatus.Success,
                                            searchItem,
                                            amountScores,
                                            containedKeywords));
            }

            return searchResults;
        }
    }
}


