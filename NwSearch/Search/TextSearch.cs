using System;
using System.Linq;
using System.Collections.Generic;

namespace NwSearch.Search
{
    public class TextSearch : ITextSearch
    {
        private string[] _wordsSeparator;
        
        private readonly WordSearch _wordSearch;

        public IEnumerable<string> WordsSeparator
        {
            get => _wordsSeparator.ToArray();
            set
            {
                _wordSearch.WordsSeparator = value;
                _wordsSeparator = value.ToArray();
            }
        }

        public TextSearch(IEnumerable<string> wordsSeparator)
        {
            _wordsSeparator = wordsSeparator.ToArray();
            _wordSearch = new WordSearch(wordsSeparator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Текст в котором ищем слова</param>
        /// <param name="words">Слова для поиска</param>
        /// <returns>Список слов, которые были найдены</returns>
        public IEnumerable<string> FindContainedWords(string text, IEnumerable<string> words)
        {
            // поиск одиночных слов. 
            var containedWords = text
                .Split(_wordsSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Where(textWord => words
                    .Any(word => textWord == word))
                .ToList();

            // если в words содержатся словосочетания, которые надо найти в тексте.
            var multiwords = _wordSearch.FindMultiWords(words);

            foreach (string multiword in multiwords)
            {
                if (!text.Contains(multiword))
                {
                    continue;
                }

                var allIndexes = AllIndexOfBySeparator(text, multiword);

                if (allIndexes.Any())
                {
                    containedWords.Add(multiword);
                }
            }
            return containedWords;
        }

        public IEnumerable<int> AllIndexOfByEndSeparator(string text, string substring)
        {
            var indexes = new List<int>();
            int startIndex = 0;
            int wordIndex;

            while ((wordIndex = text.IndexOf(substring, startIndex)) >= 0)
            {
                startIndex = wordIndex + substring.Length;
                // если подстрока находится в конце текста
                if (wordIndex + substring.Length == text.Length)
                {
                    indexes.Add(wordIndex);
                    break;
                }
                
                // если после подстроки идет разделитель слова
                if (IsSeparatorAfterSubstring(text, wordIndex, substring.Length))
                {
                    indexes.Add(wordIndex);
                }
            }

            return indexes;
        }

        public IEnumerable<int> AllIndexOfBySeparator(string text, string substring)
        {
            var indexes = new List<int>();
            int startIndex = 0;             // с какой позиции в тексте осуществляется поиск
            int substringIndex;              // индекс найденного слова

            if (String.IsNullOrEmpty(substring) || String.IsNullOrEmpty(text))
            {
                return indexes;
            }

            while ((substringIndex = text.IndexOf(substring, startIndex)) >= 0)
            {
                startIndex = substringIndex + substring.Length;

                // если строка полностью состоит из слова, которое ищем
                if (substringIndex == 0 && substring.Length == text.Length)
                {
                    indexes.Add(substringIndex);
                    break;
                }

                // Если нужное слово находится в начале строки и за ним идет разделитель слов
                if (substringIndex == 0 && IsSeparatorAfterSubstring(text, substringIndex, substring.Length))
                {
                    indexes.Add(substringIndex);
                    continue;
                }

                // если нужное слово в конце строки и перед ним разделитель слов
                if (substringIndex + substring.Length == text.Length
                    && IsSeparatorBeforeSubstring(text, substringIndex))
                {
                    indexes.Add(substringIndex);
                    continue;
                }

                // Если слово находится в середине текста между разделителями слов
                if (IsSeparatorBeforeSubstring(text, substringIndex) 
                    && IsSeparatorAfterSubstring(text, substringIndex, substring.Length))
                {
                    indexes.Add(substringIndex);
                    continue;
                }
            }

            return indexes;
        }

        public Dictionary<string, List<int>> CreateSubstringIndexesBySeparator(string text, IEnumerable<string> substringsArray)
        {
            var substringsIndexes = new Dictionary<string, List<int>>();
            foreach (var substring in substringsArray)
            {
                if (!substringsIndexes.ContainsKey(substring))
                {
                    substringsIndexes[substring] = new List<int>();
                }
                substringsIndexes[substring].AddRange(AllIndexOfBySeparator(text, substring));
            }
            return substringsIndexes;
        }

        public Dictionary<string, List<int>> CreateSubstringIndexesByEndSeparator(string text, IEnumerable<string> substringsArray)
        {
            var substringsIndexes = new Dictionary<string, List<int>>();
            foreach (var substring in substringsArray)
            {
                if (!substringsIndexes.ContainsKey(substring))
                {
                    substringsIndexes[substring] = new List<int>();
                }
                substringsIndexes[substring].AddRange(AllIndexOfByEndSeparator(text, substring));
            }
            return substringsIndexes;
        }


        private bool IsSeparatorAfterSubstring(string text, int substringIndex, int substringLength)
            => _wordsSeparator
                .Any(separator => text.IndexOf(separator, substringIndex + substringLength) == substringIndex + substringLength);

        
        private bool IsSeparatorBeforeSubstring(string text, int substringIndex)
            => _wordsSeparator
                .Any(separator => text.LastIndexOf(separator, substringIndex) != -1
                               && text.LastIndexOf(separator, substringIndex) == substringIndex - separator.Length);
    }
}
