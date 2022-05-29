using System.Collections.Generic;
using System.Linq;

namespace NwSearch.Entities
{
    /// <summary>
    /// Список синонимов слов, которые могут быть связаны с каким-то определенным словом. 
    /// Например, со словом "photoshop" связаны слова "фотошоп", "фотошопе" и т.д.
    /// </summary>
    public class SynonymWord
    {
        private List<string> _synomyms = new List<string>();

        public string Word { get; }

        public IEnumerable<string> Synonyms { get => _synomyms.ToList(); }

        public SynonymWord(string word)
        {
            Word = word;
        }

        public SynonymWord(string word, IEnumerable<string> synonyms)
        {
            Word = word;
            _synomyms.AddRange(synonyms);
        }

        public bool AddSynonym(string text)
        {
            bool isContainsSynonym = _synomyms.Any(synonym => synonym == text);
            if (isContainsSynonym)
            {
                return false;
            }
            _synomyms.Add(text);
            return true;
        }

        public void AddSynonyms(IEnumerable<string> synonyms)
        {
            foreach (var synonym in synonyms)
            {
                AddSynonym(synonym);
            }
        }

        public bool RemoveSynonym(string text)
            => _synomyms.RemoveAll(synonym => synonym == text) > 0 ? true : false;
    }
}