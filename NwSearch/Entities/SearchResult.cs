using System.Collections.Generic;

namespace NwSearch.Entities
{
    public class SearchResult<T>
    {
        private readonly List<Keyword> _matchedKeywords = new List<Keyword>();

        public SearchResultStatus Status { get; set; } = SearchResultStatus.Empty;

        public int MatchScore { get; set; } = 0;

        public SearchItem<T>? SearchItem { get; set; } = default;

        public IEnumerable<Keyword> MatchedKeywords => _matchedKeywords.ToArray();

        public SearchResult()
        { }

        public SearchResult(
            SearchResultStatus status,
            SearchItem<T>? searchItem,
            int matchScore,
            IEnumerable<Keyword>? matchedKeywords)
        {
            Status = status;
            SearchItem = searchItem;
            MatchScore = matchScore;
            AddKeywords(matchedKeywords);
        }

        public void AddKeywords(IEnumerable<Keyword>? keywords)
        {
            if (keywords == null)
            {
                return;
            }
            foreach (var keyword in keywords)
            {
                AddKeyword(keyword);
            }
        }

        public bool AddKeyword(Keyword keyword)
        {
            if (keyword == null)
            {
                return false;
            }
            _matchedKeywords.Add(keyword);
            return true;
        }

        public bool RemoveKeyword(Keyword keyword)
            => _matchedKeywords.Remove(keyword);

        public void RemoveKeywordsByName(string keywordName)
            => _matchedKeywords.RemoveAll(keyword => keyword.Name == keywordName);

    }
}