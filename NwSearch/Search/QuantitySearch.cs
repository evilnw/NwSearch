using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using NwSearch.Entities;

namespace NwSearch.Search
{
    /// <summary>
    /// Поиск значений, которые указывают на количество.
    /// </summary>
    /// <typeparam name="T">Только целочисленные(Int16, Int32 и т.д.)</typeparam>
    public class QuantitySearch<T>
    {
        private string[] _wordsSeparator;
        
        private List<Keyword> _quantityKeywords;
        
        private Dictionary<string, T> _digitsNames;
        
        private TextSearch _textSearch;

        public CultureInfo? CultureInfo { get; set; }

        public ITypeDescriptorContext? TypeDescriptorContext { get; set; }

        public IEnumerable<string> WordsSeparator
        {
            get => _wordsSeparator.ToArray();
            set
            {
                _textSearch.WordsSeparator = value;
                _wordsSeparator = value.ToArray();
            }
        }
        /// <summary>
        /// Ключевые слова которые указывают на количество. (шт, штук и т.д.)
        /// </summary>
        public IEnumerable<Keyword> QuantityKeywords
        {
            get => _quantityKeywords.ToArray();
            set => _quantityKeywords = new List<Keyword>(value);
        }
        /// <summary>
        /// Словарь со словами, которые указывают на количество (одна - 1, две - 2, три - 3 и т.д.)
        /// </summary>
        public Dictionary<string, T> DigitsNames
        {
            get => new Dictionary<string, T>(_digitsNames);
            set => _digitsNames = new Dictionary<string, T>(value);
        }

        public QuantitySearch(
            IEnumerable<string> wordsSeparator, 
            IEnumerable<Keyword> quantityKeywordsCollection)
        {
            _wordsSeparator = wordsSeparator.ToArray();
            _textSearch = new TextSearch(wordsSeparator);
            _quantityKeywords = new List<Keyword>(quantityKeywordsCollection);
            _digitsNames = new Dictionary<string, T>();

            if (!IsNumericType())
            {
                throw new Exception("Only Numeric Type(int, double, etc)");
            }
        }

        /// <summary>
        /// Найходит в тексте значения перед ключевым словом, которые указывают на количество(например "13 шт".
        /// </summary>
        /// <param name="text">текст, в которых происходит поиск</param>
        /// <returns></returns>
        public IEnumerable<SearchResult<T>> FindBeforeKeyword(string text)
        {
            var keywordsNamesIndexesDic = _textSearch
                .CreateSubstringIndexesBySeparator(text, _quantityKeywords.Select(keyword => keyword.Name))
                .Where(keywordNameIndexesDic => keywordNameIndexesDic.Value.Any());
            
            return GetSearchResults(text, keywordsNamesIndexesDic);
        }

        // ищет количество, если цифра написана слитно вместе с ключевым словом. Например "требуется 13шт" где "шт" - ключевое слово.
        /// <summary>
        /// Найходит в тексте значения, которые указывают на количество и распрогаются внутри ключевого слова(Например "13шт").
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<SearchResult<T>> FindInsideKeyword(string text)
        {
            var keywordsNamesIndexesDic = _textSearch
                .CreateSubstringIndexesByEndSeparator(text, _quantityKeywords.Select(keyword => keyword.Name))
                .Where(keywordNameIndexesDic => keywordNameIndexesDic.Value.Any());
            return GetSearchResults(text, keywordsNamesIndexesDic, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">текст в котором ищем ключевые слова</param>
        /// <param name="keywordsNamesIndexesDic">Имена ключевых слов и их позиции в тексте. 
        ///     key - имя ключевого слова. value - список позиций в тексте</param>
        /// <param name="IsCheckInsideWord">Требуется ли поиск, если количество написано слитно(13шт) вместе с ключевым словом</param>
        /// <returns></returns>
        private IEnumerable<SearchResult<T>> GetSearchResults(
            string text,                                
            IEnumerable<KeyValuePair<string, List<int>>> keywordsNamesIndexesDic,
            bool IsCheckInsideWord = false)
        {
            var searchResults = new List<SearchResult<T>>();

            var indexesArray = keywordsNamesIndexesDic
                .SelectMany(keywordNameIndexesDic => keywordNameIndexesDic.Value)
                .Distinct()
                .Where(index => index != 0)
                .OrderBy(index => index);

            foreach (var keywordIndex in indexesArray)
            {
                string substring = text.Substring(0, keywordIndex);

                if (IsCheckInsideWord == true && IsSeparatorEndOfString(substring))
                {
                    continue;
                }

                var substringWords = substring.Split(_wordsSeparator, StringSplitOptions.RemoveEmptyEntries);

                if (!substringWords.Any())
                {
                    continue;
                }

                T value;
                if (!TryParse(substringWords.Last(), out value))
                {
                    continue;
                }

                var keywordsNames = keywordsNamesIndexesDic
                    .Where(namesIndexDic => namesIndexDic.Value
                        .Any(indexIter => indexIter == keywordIndex))
                    .Select(namesIndexDic => namesIndexDic.Key);

                var keywords = _quantityKeywords
                    .Where(keywordIter => keywordsNames
                        .Any(keywordName => keywordIter.Name == keywordName));

                searchResults.Add(
                    new SearchResult<T>()
                    {
                        Status = SearchResultStatus.Success,
                        MatchScore = keywords.Sum(keyword => keyword.Score),
                        KeywordsMatchCollection = keywords,
                        SearchItem = new SearchItem<T>(value, keywords, 1)
                    });
            }
            return searchResults;
        }

        private bool IsSeparatorEndOfString(string text)
            => _wordsSeparator
                .Any(separator => text.LastIndexOf(separator) != -1
                                && text.LastIndexOf(separator) + separator.Length == text.Length);

        private bool TryParse(string word, out T value)
        {
            value = default(T)!;
            try
            {
                if (_digitsNames.ContainsKey(word))
                {
                    value = _digitsNames[word];
                    return true;
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                value = (T)converter.ConvertFromString(TypeDescriptorContext, CultureInfo, word);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsNumericType()
        {
            if (typeof(T) == typeof(Byte)
                || typeof(T) == typeof(SByte)
                || typeof(T) == typeof(UInt16)
                || typeof(T) == typeof(UInt32)
                || typeof(T) == typeof(UInt64)
                || typeof(T) == typeof(Int16)
                || typeof(T) == typeof(Int32)
                || typeof(T) == typeof(Int64))
            {
                return true;
            }

            return false;
        }
    }
}
