using System.Collections;
using System.Text;
using System.Web;

namespace ModernRouter.Routing;

public class QueryParameters : IEnumerable<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, string> _parameters = new(StringComparer.OrdinalIgnoreCase);

    public QueryParameters() { }

    public QueryParameters(string? queryString)
    {
        if (!string.IsNullOrWhiteSpace(queryString))
            Parse(queryString);
    }

    public QueryParameters(Uri uri)
    {
        if (!string.IsNullOrEmpty(uri.Query))
            Parse(uri.Query);
    }

    public string? this[string key]
    {
        get => _parameters.TryGetValue(key, out var value) ? value : null;
        set => Set(key, value);
    }

    public int Count => _parameters.Count;

    public void Set(string key, string? value)
    {
        if (value == null)
            _parameters.Remove(key);
        else
            _parameters[key] = value;
    }

    public void Add(string key, string value) => _parameters[key] = value;

    public bool Remove(string key) => _parameters.Remove(key);

    public bool ContainsKey(string key) => _parameters.ContainsKey(key);

    public void Clear() => _parameters.Clear();

    public string ToQueryString(bool includeQuestionMark = true)
    {
        if (_parameters.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        if (includeQuestionMark)
            sb.Append('?');

        bool first = true;
        foreach (var kvp in _parameters)
        {
            if (!first)
                sb.Append('&');
            
            sb.Append(Uri.EscapeDataString(kvp.Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(kvp.Value));
            first = false;
        }

        return sb.ToString();
    }

    public static QueryParameters Parse(Uri uri) => new(uri);

    public static QueryParameters ParseString(string? queryString) => new(queryString);

    private void Parse(string queryString)
    {
        if (queryString.StartsWith('?'))
            queryString = queryString[1..];

        var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var equalIndex = pair.IndexOf('=');
            string key, value;

            if (equalIndex >= 0)
            {
                key = HttpUtility.UrlDecode(pair[..equalIndex]);
                value = HttpUtility.UrlDecode(pair[(equalIndex + 1)..]);
            }
            else
            {
                key = HttpUtility.UrlDecode(pair);
                value = string.Empty;
            }

            _parameters[key] = value;
        }
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}