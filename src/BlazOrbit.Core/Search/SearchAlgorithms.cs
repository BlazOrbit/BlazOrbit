namespace BlazOrbit.Components;

public static class SearchAlgorithms
{
    public static IEnumerable<SearchResult<T>> Search<T>(
        IEnumerable<T> items,
        string query,
        Func<T, string> textSelector,
        SearchMode mode = SearchMode.Smart)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return items.Select(item => new SearchResult<T>(item, 1.0, SearchMatchType.Exact));
        }

        string normalizedQuery = query.Trim().ToLowerInvariant();

        return mode switch
        {
            SearchMode.StartsWith => SearchStartsWith(items, normalizedQuery, textSelector),
            SearchMode.Contains => SearchContains(items, normalizedQuery, textSelector),
            SearchMode.Fuzzy => SearchFuzzy(items, normalizedQuery, textSelector),
            SearchMode.Smart or _ => SearchSmart(items, normalizedQuery, textSelector)
        };
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return target?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        int sourceLength = source.Length;
        int targetLength = target.Length;

        int[,] distance = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++)
        {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= targetLength; j++)
        {
            distance[0, j] = j;
        }

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(
                        distance[i - 1, j] + 1,
                        distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceLength, targetLength];
    }

    private static bool MatchesAcronym(string text, string query)
    {
        string[] words = text.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length < query.Length)
        {
            return false;
        }

        int queryIndex = 0;

        foreach (string word in words)
        {
            if (queryIndex < query.Length &&
                word.Length > 0 &&
                char.ToLowerInvariant(word[0]) == query[queryIndex])
            {
                queryIndex++;
            }

            if (queryIndex == query.Length)
            {
                return true;
            }
        }

        return queryIndex == query.Length;
    }

    private static bool MatchesWordStart(string text, string query)
    {
        string[] words = text.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
        int queryIndex = 0;

        foreach (string word in words)
        {
            if (queryIndex < query.Length && word.StartsWith(query[queryIndex].ToString()))
            {
                queryIndex++;
                int matchLength = 1;

                while (queryIndex < query.Length &&
                       matchLength < word.Length &&
                       word[matchLength] == query[queryIndex])
                {
                    queryIndex++;
                    matchLength++;
                }
            }

            if (queryIndex == query.Length)
            {
                return true;
            }
        }

        return false;
    }

    private static SearchResult<T>? ScoreItem<T>(T item, string text, string query)
    {
        if (text == query)
        {
            return new SearchResult<T>(item, 1.0, SearchMatchType.Exact);
        }

        if (text.StartsWith(query))
        {
            double score = 0.9 + (0.1 * query.Length / text.Length);
            return new SearchResult<T>(item, score, SearchMatchType.StartsWith);
        }

        if (MatchesWordStart(text, query))
        {
            return new SearchResult<T>(item, 0.8, SearchMatchType.WordStart);
        }

        if (MatchesAcronym(text, query))
        {
            return new SearchResult<T>(item, 0.7, SearchMatchType.Acronym);
        }

        if (text.Contains(query))
        {
            int index = text.IndexOf(query);
            double score = 0.5 + (0.1 * (1.0 - ((double)index / text.Length)));
            return new SearchResult<T>(item, score, SearchMatchType.Contains);
        }

        int distance = LevenshteinDistance(text, query);
        int maxDistance = Math.Max(1, query.Length / 3);

        if (distance <= maxDistance)
        {
            double score = 0.3 * (1.0 - ((double)distance / query.Length));
            return new SearchResult<T>(item, score, SearchMatchType.Fuzzy);
        }

        return null;
    }

    private static IEnumerable<SearchResult<T>> SearchContains<T>(
        IEnumerable<T> items,
        string query,
        Func<T, string> textSelector)
    {
        return items
            .Where(item => textSelector(item).ToLowerInvariant().Contains(query))
            .Select(item => new SearchResult<T>(item, 1.0, SearchMatchType.Contains));
    }

    private static IEnumerable<SearchResult<T>> SearchFuzzy<T>(
        IEnumerable<T> items,
        string query,
        Func<T, string> textSelector)
    {
        int maxDistance = Math.Max(1, query.Length / 3);

        return items
            .Select(item =>
            {
                string text = textSelector(item).ToLowerInvariant();
                int distance = LevenshteinDistance(text, query);
                double score = 1.0 - ((double)distance / Math.Max(text.Length, query.Length));
                return (Item: item, Distance: distance, Score: score);
            })
            .Where(x => x.Distance <= maxDistance)
            .OrderBy(x => x.Distance)
            .Select(x => new SearchResult<T>(x.Item, x.Score, SearchMatchType.Fuzzy));
    }

    private static IEnumerable<SearchResult<T>> SearchSmart<T>(
                                IEnumerable<T> items,
        string query,
        Func<T, string> textSelector)
    {
        List<SearchResult<T>> results = [];

        foreach (T item in items)
        {
            string text = textSelector(item).ToLowerInvariant();
            SearchResult<T>? result = ScoreItem(item, text, query);

            if (result.HasValue)
            {
                results.Add(result.Value);
            }
        }

        return results.OrderByDescending(r => r.Score).ThenBy(r => textSelector(r.Item));
    }

    private static IEnumerable<SearchResult<T>> SearchStartsWith<T>(
        IEnumerable<T> items,
        string query,
        Func<T, string> textSelector)
    {
        return items
            .Where(item => textSelector(item).ToLowerInvariant().StartsWith(query))
            .Select(item => new SearchResult<T>(item, 1.0, SearchMatchType.StartsWith));
    }
}