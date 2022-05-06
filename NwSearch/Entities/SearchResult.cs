using System.Collections.Generic;

namespace NwSearch.Entities
{
    public class SearchResult<T>
    {
        private List<Keyword> _keywordsMatchCollection = new List<Keyword>();

        public SearchResultStatus Status { get; set; } = SearchResultStatus.Empty;

        public int MatchScore { get; set; } = 0;

        public SearchItem<T>? SearchItem { get; set; } = default(SearchItem<T>);

        public IEnumerable<Keyword> KeywordsMatchCollection
        {
            get => _keywordsMatchCollection.ToArray();
            set
            {
                _keywordsMatchCollection = new List<Keyword>();
                if (value != null)
                {
                    _keywordsMatchCollection.AddRange(value);
                }
            }
        }
    }
}