using System.Collections.Generic;
using System.Linq;

namespace NwSearch.Entities
{
    public class SearchItem<T>
    {
        protected List<Keyword> _keywords = new List<Keyword>();

        public T Value { get; }

        public IEnumerable<Keyword> Keywords => _keywords.ToArray();

        public int Priority { get; set; }

        public SearchItem(T value)
        {
            Value = value;
        }

        public SearchItem(T value, int priority)
        {
            Value = value;
            Priority = priority;
        }

        public SearchItem(T value, IEnumerable<Keyword> keywords, int priority)
        {
            Value = value;
            AddKeywords(keywords);
            Priority = priority;
        }

        public void AddKeywords(IEnumerable<Keyword> keywords)
        {
            if (keywords == null)
            {
                return;
            }
            
            foreach(var keyword in keywords)
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
            _keywords.Add(keyword);
            return true;
        }

        public bool RemoveKeyword(Keyword keyword)
            => _keywords.Remove(keyword);

        public void RemoveKeywordsByName(string keywordName)
            => _keywords.RemoveAll(keyword => keyword.Name == keywordName);
    }
}
