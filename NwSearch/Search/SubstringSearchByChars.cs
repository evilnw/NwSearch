using System.Collections.Generic;
using System.Linq;
using System;
using NwSearch.Entities;

namespace NwSearch.Search
{
    /// <summary>
    /// Находит слова, которые состоят из определенных символов.
    /// </summary>
    public class SubstringSearchByChars : ISubstringSearch
    {
        private string[] _wordsSeparator;
        
        private readonly List<string> _ignoredWords = new List<string>();
        
        private readonly List<WordSynonyms> _wordsSynonyms = new List<WordSynonyms>();

        private readonly Dictionary<string, int> _customKeywordsScores = new Dictionary<string, int>();
        
        private List<char> _chars;

        private readonly ITextSearch _textSearch;

        private readonly IWordSearch _wordsSearch;
        
        public string JoinNameSeparator { get; set; } = " ";
        /// <summary>
        /// Значимость(Score) по умолчанию для найденых ключевых слов
        /// </summary>
        public int DefaultKeywordScore { get; set; } = 1;
        
        /// <summary>
        /// Минимальная длинна каждого найденного в тексте слова.
        /// </summary>
        public int MinWordLength { get; set; } = 1;

        /// <summary>
        /// Следует ли игнорировать найденную подстроку, если она полностью состоит из цифр, которых входили в Chars
        /// </summary>
        public bool IsIgnoreOnlyDigitsName { get; set; } = true;

        /// <summary>
        /// Список слов, которые следует проигнорировать даже если они удоволетворяют поиску
        /// </summary>
        public IEnumerable<string> IgnoredWords => _ignoredWords.ToArray();

        /// <summary>
        /// Имена ключевых слов, у которых значимость(score) отличается от стандартного значения DefaultKeywordScore
        /// </summary>
        public Dictionary<string, int> CustomKeywordsScores => new Dictionary<string, int>(_customKeywordsScores);

        public IEnumerable<string> WordsSeparator
        {
            get => _wordsSeparator.ToArray();
            set
            {
                _wordsSeparator = value.ToArray();
                _textSearch.WordsSeparator = value;
                _wordsSearch.WordsSeparator = value;
            }
        }

        /// <summary>
        /// Список синонимов, которые не следует игнорировать, если они НЕ удоволетворяют условию поиску по символам, но встречаются в тексте.
        /// </summary>
        public IEnumerable<WordSynonyms> WordsSynonyms => _wordsSynonyms.ToArray();

        /// <summary>
        /// Список символов, которые используются для поиска слов в тексте.
        /// </summary>
        public IEnumerable<char> Chars
        {
            get => _chars.ToArray();
            set => _chars = new List<char>(value);
        }

        public SubstringSearchByChars(
            IEnumerable<string> wordsSeparator, 
            IEnumerable<char> chars)
        {
            _wordsSeparator = wordsSeparator.ToArray();
            _chars = new List<char>(chars);
            _textSearch = new TextSearch(wordsSeparator);
            _wordsSearch = new WordSearch(wordsSeparator);
        }

        public void AddCustomKeywordScore(string keywordName, int score)
            => _customKeywordsScores[keywordName] = score;

        public void AddCustomKeywordsScores(IEnumerable<KeyValuePair<string, int>> pairs)
            => pairs.ToList().ForEach(pair => AddCustomKeywordScore(pair.Key, pair.Value));

        public void AddSynonym(WordSynonyms synonymWord)
            => _wordsSynonyms.Add(synonymWord);

        public void AddSynonyms(IEnumerable<WordSynonyms> synonymWords)
            => _wordsSynonyms.AddRange(synonymWords);

        public void RemoveSynonym(WordSynonyms synonymWord)
            => _wordsSynonyms.Remove(synonymWord);

        public void AddIgnoredWord(string ignoredWord)
            => _ignoredWords.Add(ignoredWord);

        public void AddIgnoredWords(IEnumerable<string> ignoredWords)
            => _ignoredWords.AddRange(ignoredWords);

        public void RemoveIgnoredWord(string ignoredWord)
            => _ignoredWords.Remove(ignoredWord);

        /// <summary>
        /// Находит в тексте все слова, которые удоволетворяют настройкам поиска.
        /// </summary>
        /// <example>
        /// Пример:
        /// text = "сколько стоит adobe фотошоп версия cloud - 13 шт".
        /// Условия поиска:
        /// В _synonymWords хранятся синонимы для слова "photoshop" - "фотошоп", "фотошопе", "пхотошоп".
        /// В _chars - хранятся все английские символы('a', 'b', 'c' и т.д.)
        /// Результат:
        /// В результате SearchResult.SearchItem.Value будет хранится строка("adobe фотошоп cloud")
        /// В SearchResult.KeywordsMatch и SearchResult.SearchItem.Keywords будут хранится ключевые слова:
        /// "adobe", "photoshop", "cloud"
        /// </example>
        public SearchResult<string> FindSubstring(string text)
        {
            var searchItem = CreateSearchItem(text);
            
            if (String.IsNullOrEmpty(searchItem.Value))
            {
                return new SearchResult<string>() { Status = SearchResultStatus.Empty };
            }
            
            var digits = _chars.Where(ch => char.IsDigit(ch));
            
            bool isOnlyDigits = searchItem.Value
                .Split(_wordsSeparator, StringSplitOptions.RemoveEmptyEntries)
                .All(word => IsIgnoreOnlyDigitsName == true
                            && word.All(ch => digits
                                .Any(digit => ch == digit)));
            
            var searchResultStatus = (isOnlyDigits == true) ? SearchResultStatus.Empty : SearchResultStatus.Success;

            return new SearchResult<string>(searchResultStatus,
                                            searchItem,
                                            searchItem.Keywords.Sum(keyword => keyword.Score),
                                            searchItem.Keywords);
        }

        private IEnumerable<Keyword> MergeKeywords(IEnumerable<Keyword> keywords)
        {
            var mergedKeywords = new List<Keyword>();
            var keywordsNames = keywords
                .Select(keyword => keyword.Name)
                .Distinct();

            foreach (string keywordName in keywordsNames)
            {
                mergedKeywords.Add(new Keyword(keywordName, keywords.Count(keyword => keyword.Name == keywordName)));
            }

            return mergedKeywords;
        }

        /* Пример алгоритма формирования имени:
            1. Найти все словосочетния(2 и более слова) из _synonymWords, которые есть в тексте.  
            2. Для каждого словосочетния получить его все индексы в тексте.
            3. Произвести иттерацию по каждому индексу.
            4. Находим все словосочетния синонимы, которые находятся по данному индексу и соритурем их в порядке убывания размера словосочетания.
            5. Из подстроки между currentTextPosition и индексом словосочетния извлекаем все одиночные слова, а также формируем ключевые слова.
            6. После итерации индексов извлекаем слова из оставшегося необработанного текста между currentTextPosition и text.Lenght
         */
        private SearchItem<string> CreateSearchItem(string text)
        {
            string name = "";
            var keywords = new List<Keyword>();

            var containedMultiWordsSynonyms = GetMultiWordsSynonyms(text);
            // ключ - имя синонима, значение - список индексов, где находится синоним в тексте
            var synonymsIndexesDic = _textSearch.CreateSubstringIndexesBySeparator(text, containedMultiWordsSynonyms);
            // Список индексов в порядке очереди.
            var synonymsIndexesArray = synonymsIndexesDic
                .SelectMany(synonymIdexesDic => synonymIdexesDic.Value)
                .OrderBy(index => index)
                .Distinct();

            int currentTextPosition = 0;    // позиция в тексте до которой извлекается название
            
            foreach (int index in synonymsIndexesArray)
            {
                var synonyms = synonymsIndexesDic
                    .Where(synonymIndexesDic => synonymIndexesDic.Value
                        .Any(indexIter => indexIter == index))
                    .OrderByDescending(synonymIndexesDic => synonymIndexesDic.Key.Length);

                if (!synonyms.Any())
                {
                    continue;
                }

                if (currentTextPosition > index + synonyms.First().Key.Length)
                {
                    continue;
                }

                if (currentTextPosition > index)
                {
                    name += text.Substring(currentTextPosition, index + synonyms.First().Key.Length - currentTextPosition);
                    currentTextPosition = index + synonyms.First().Key.Length;
                    keywords.AddRange(CreateKeywordsArray(synonyms.Select(synonym => synonym.Key)));
                    continue;
                }

                var substringWords = text.Substring(currentTextPosition, index - currentTextPosition);
                var nameWordsArray = GetSimpleNameWords(substringWords);
                var nameSubstring = String.Join(JoinNameSeparator, nameWordsArray);

                name = name
                    + ((!String.IsNullOrEmpty(name) && (!String.IsNullOrEmpty(nameSubstring) || (!String.IsNullOrEmpty(synonyms.First().Key))) ? JoinNameSeparator : ""))
                    + nameSubstring + (String.IsNullOrEmpty(nameSubstring) ? "" : JoinNameSeparator)
                    + synonyms.First().Key;
                keywords.AddRange(CreateKeywordsArray(nameWordsArray));
                keywords.AddRange(CreateKeywordsArray(synonyms.Select(synonym => synonym.Key)));
                currentTextPosition = index + synonyms.First().Key.Length;
            }

            if (currentTextPosition < text.Length - 1)
            {
                var substringWords = text.Substring(currentTextPosition, text.Length - currentTextPosition);
                var nameWordsArray = GetSimpleNameWords(substringWords);
                var nameSubstring = String.Join(JoinNameSeparator, nameWordsArray);

                name = name
                    + (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(nameSubstring) ? JoinNameSeparator : "")
                    + nameSubstring;
                keywords.AddRange(CreateKeywordsArray(nameWordsArray));
            }

            return new SearchItem<string>(name, MergeKeywords(keywords), 1);
        }

        private IEnumerable<string> GetSimpleNameWords(string text)
        {
            return text
                .Split(_wordsSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => _wordsSynonyms
                    .Any(synonymWord => synonymWord.Synonyms.Any(synonym => synonym == word)
                                    || word.All(ch => _chars.Any(chIter => chIter == ch))
                                    && word.Length >= MinWordLength
                                    && !IsIgnoredWord(word)));
        }

        private bool IsIgnoredWord(string word)
        {
            return _ignoredWords.Any(ingoredWord => ingoredWord == word);
        }

        private IEnumerable<Keyword> CreateKeywordsArray(IEnumerable<string> words)
        {
            var keywords = new List<Keyword>();
            foreach (var word in words)
            {
                keywords.AddRange(CreateKeywordsArray(word));
            }
            return keywords;
        }

        private IEnumerable<Keyword> CreateKeywordsArray(string word)
        {
            var keywords = new List<Keyword>();
            var containedWordsSynonyms = _wordsSynonyms
                .Where(synonymWord => synonymWord.Synonyms
                    .Any(synonym => synonym == word));
            if (containedWordsSynonyms.Any())
            {
                foreach (var wordSynonym in containedWordsSynonyms)
                {
                    keywords.Add(CreateKeyword(wordSynonym.Word));
                }
            }
            else
            {
                keywords.Add(CreateKeyword(word));
            }
            return keywords;
        }

        private IEnumerable<string> GetMultiWordsSynonyms(string text)
        {
            var allSynonyms = _wordsSynonyms
                .SelectMany(synonymWord => synonymWord.Synonyms);
            var containedSynonyms = _textSearch.FindContainedWords(text, allSynonyms);

            return _wordsSearch.FindMultiWords(containedSynonyms);
        }

        private Keyword CreateKeyword(string keywordName)
            => new Keyword(
                name: keywordName,
                score: _customKeywordsScores.ContainsKey(keywordName) ? _customKeywordsScores[keywordName] : DefaultKeywordScore);
    }
}
