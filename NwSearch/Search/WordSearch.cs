using System;
using System.Linq;
using System.Collections.Generic;

namespace NwSearch.Search
{
    public class WordSearch : IWordSearch
    {
        private string[] _wordsSeparator;

        public IEnumerable<string> WordsSeparator
        {
            get => _wordsSeparator.ToArray();
            set => _wordsSeparator = value.ToArray();
        }

        public WordSearch(IEnumerable<string> wordsSeparator)
            => _wordsSeparator = wordsSeparator.ToArray();
        
        public IEnumerable<string> FindMultiWords(IEnumerable<string> words, int minWordsCount = 2)
            => words
            .Where(word => word
                .Split(_wordsSeparator, StringSplitOptions.RemoveEmptyEntries).Count() >= minWordsCount);
    }
}
