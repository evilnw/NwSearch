using System;
using System.Collections.Generic;
using System.Text;
using NwSearch.Entities;

namespace NwSearch.Search
{
    public interface ISearchByKeywords
    {
        IEnumerable<SearchResult<string>> FindAll(IEnumerable<Keyword> keywords);
    }
}
