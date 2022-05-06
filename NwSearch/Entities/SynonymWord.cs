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
        private List<string> _synonymsCollection = new List<string>();

        public string Word { get; }

        public IEnumerable<string> SynonymsCollection { get => _synonymsCollection.ToList(); }

        public SynonymWord(string word)
        {
            Word = word;
        }

        public SynonymWord(string word, IEnumerable<string> synonymsCollection)
        {
            Word = word;
            _synonymsCollection.AddRange(synonymsCollection);
        }

        public bool AddSynonym(string text)
        {
            bool isContainsSynonym = _synonymsCollection.Any(synonym => synonym == text);
            if (isContainsSynonym)
            {
                return false;
            }
            _synonymsCollection.Add(text);
            return true;
        }

        public void AddSynonyms(IEnumerable<string> synonymsCollection)
        {
            foreach (var synonym in synonymsCollection)
            {
                AddSynonym(synonym);
            }
        }

        public bool RemoveSynonym(string text)
            => _synonymsCollection.RemoveAll(synonym => synonym == text) > 0 ? true : false;
    }
}