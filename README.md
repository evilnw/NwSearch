# NwSearch

Библиотека для поиска разных значений в тексте.

1. QuantitySearch<T> - используется для поиска количества по ключевым словам.

```cs
// Если ключевое слово перед значением.

string text = "adobe photoshop 9 шт"
var wordsSeparator = new List<string>() { " ", ".", "," };                      // разделитель слов в тексте.
var quantityKeywords = new List<Keyword>() { new Keyword("шт", 5), new Keyword("штук", 5) };
var quantitySearch = new QuantitySearch<int>(wordsSeparator, quantityKeywords);
var quantityResultsCollection = quantitySearch.FindBeforeKeywords(textRow);     // получить список с результатами поиска.

foreach (var quantityResult in quantityResultsCollection)
{
    int quantity = quantityResult.SearchItem.Value;                 // извлечь найденное значение из результата
    var keyword = quantityResult.KeywordsMatchCollection.First()    // ключевое слово перед которым стояло значение.
}

// Если значение внутри ключевого слова. Например "9шт", то используется метод:

text = "adobe photoshop 9шт"
quantityResultsCollection = quantitySearch.FindInsideWords(textRow);

foreach (var quantityResult in quantityResultsCollection)
{
    int quantity = quantityResult.SearchItem.Value;                 // извлечь найденное значение из результата
    var keyword = quantityResult.KeywordsMatchCollection.First()    // ключевое слово перед которым стояло значение.
}

// Если значение было написано словом. Например "пять штук". В DigitsNames передать словарь с синонимами цифр.
// key - синоним цифры, value - значение.

text = "adobe photoshop пять штук"
var digitsNames = new Dictionary<string, int>() { { "одну", 1 }, {"одна", 1}, { "две", 2 }, { "три", 3 }, { "четыре", 4 }, { "пять", 5 } };
quantitySearch.DigitsNames = digitsNames;
var quantityResultsCollection = quantitySearch.FindBeforeKeywords(textRow);
```

2. SubstringSearchByChars - Поиск подстроки, которая состоит из букв с определенными символами. Например, если надо из строчки извлечь англоязычное название продукта.

```cs
string text = "клиенту требуется adobe фотошоп cloud версия 2022.";
var wordsSeparator = new List<string>() { " ", ".", "," };
var chars = "abcdefghijklmnopqrstuvwxyz0123456789".Select(ch => ch);
var substringSearchByChars = new SubstringSearchByChars(wordsSeparator, chars);
var resultName = substringSearchByChars.FindSubstring(text);
string productName = resultName.SearchItem.Value;               // результат "adobe cloud 2022"
var keywords = resultName.KeywordsMatchCollection               // будет содержаться список ключевых слов с именами: "adobe" "cloud" "2022"

// Можно добавить Список синонимов, которые не следует игнорировать, если они НЕ удоволетворяют условию поиску по символам, но встречаются в тексте
var synonym = new SynonymWord("photoshop", new List<string>() { "фотошоп", "фотошопе", "пхотошопе" };
substringSearchByChars.AddSynonym(synonym);
resultName = substringSearchByChars.FindSubstring(text);
productName = resultName.SearchItem.Value;                      // результат "adobe фотошоп cloud 2022"
keywords = resultName.KeywordsMatchCollection                   // будет содержаться список ключевых слов с именами: "adobe" "photoshop" "cloud" "2022"

// Можно добавить слова, которые будут игнорироваться в процессе формирования подстроки.
var ignoredWords = new List<string>() { "adobe", "cloud" };
substringSearchByChars.AddIgnoredWords(ignoredWords);
resultName = substringSearchByChars.FindSubstring(text);
productName = resultName.SearchItem.Value;                      // результат "фотошоп 2022"
keywords = resultName.KeywordsMatchCollection                   // будет содержаться список ключевых слов с именами: "photoshop" "2022"
```

3. SearchByKeywords

4. SearchBySingleKeyword