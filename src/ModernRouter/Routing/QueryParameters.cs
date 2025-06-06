using System.Collections;
using System.Text;
using System.Web;

namespace ModernRouter.Routing;

/// <summary>
/// Utility class for parsing, manipulating, and generating query parameters
/// </summary>
public class QueryParameters : IEnumerable<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, List<string>> _parameters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates empty query parameters
    /// </summary>
    public QueryParameters()
    {
    }

    /// <summary>
    /// Creates query parameters from a query string
    /// </summary>
    /// <param name="queryString">Query string with or without leading '?'</param>
    public QueryParameters(string? queryString)
    {
        if (!string.IsNullOrEmpty(queryString))
        {
            Parse(queryString);
        }
    }

    /// <summary>
    /// Creates query parameters from a URI
    /// </summary>
    /// <param name="uri">URI containing query parameters</param>
    public QueryParameters(Uri uri)
    {
        if (!string.IsNullOrEmpty(uri.Query))
        {
            Parse(uri.Query);
        }
    }

    /// <summary>
    /// Gets the first value for the specified key
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <returns>First value or null if not found</returns>
    public string? this[string key]
    {
        get => GetFirst(key);
        set => Set(key, value);
    }

    /// <summary>
    /// Gets the number of unique parameter keys
    /// </summary>
    public int Count => _parameters.Count;

    /// <summary>
    /// Gets all parameter keys
    /// </summary>
    public IEnumerable<string> Keys => _parameters.Keys;

    /// <summary>
    /// Gets the first value for the specified key
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <returns>First value or null if not found</returns>
    public string? GetFirst(string key)
    {
        return _parameters.TryGetValue(key, out var values) && values.Count > 0 ? values[0] : null;
    }

    /// <summary>
    /// Gets all values for the specified key
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <returns>All values for the key</returns>
    public IReadOnlyList<string> GetAll(string key)
    {
        return _parameters.TryGetValue(key, out var values) ? values.AsReadOnly() : Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the specified key exists
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <returns>True if key exists</returns>
    public bool ContainsKey(string key)
    {
        return _parameters.ContainsKey(key);
    }

    /// <summary>
    /// Sets a single value for the specified key, replacing any existing values
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <param name="value">Parameter value</param>
    public void Set(string key, string? value)
    {
        if (value == null)
        {
            Remove(key);
        }
        else
        {
            _parameters[key] = new List<string> { value };
        }
    }

    /// <summary>
    /// Adds a value for the specified key, preserving existing values
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <param name="value">Parameter value</param>
    public void Add(string key, string value)
    {
        if (!_parameters.TryGetValue(key, out var values))
        {
            values = new List<string>();
            _parameters[key] = values;
        }
        values.Add(value);
    }

    /// <summary>
    /// Removes all values for the specified key
    /// </summary>
    /// <param name="key">Parameter key</param>
    /// <returns>True if key was removed</returns>
    public bool Remove(string key)
    {
        return _parameters.Remove(key);
    }

    /// <summary>
    /// Removes all parameters
    /// </summary>
    public void Clear()
    {
        _parameters.Clear();
    }

    /// <summary>
    /// Gets a typed value for the specified key
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="key">Parameter key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <returns>Converted value or default</returns>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        var value = GetFirst(key);
        if (value == null)
            return defaultValue;

        try
        {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Tries to get a typed value for the specified key
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="key">Parameter key</param>
    /// <param name="value">Output value</param>
    /// <returns>True if conversion succeeded</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        value = default;
        var stringValue = GetFirst(key);
        if (stringValue == null)
            return false;

        try
        {
            value = (T?)Convert.ChangeType(stringValue, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts to query string format
    /// </summary>
    /// <param name="includeQuestionMark">Whether to include leading '?'</param>
    /// <returns>Query string</returns>
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
            foreach (var value in kvp.Value)
            {
                if (!first)
                    sb.Append('&');
                
                sb.Append(HttpUtility.UrlEncode(kvp.Key));
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(value));
                first = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a copy of these query parameters
    /// </summary>
    /// <returns>New QueryParameters instance</returns>
    public QueryParameters Clone()
    {
        var clone = new QueryParameters();
        foreach (var kvp in _parameters)
        {
            clone._parameters[kvp.Key] = new List<string>(kvp.Value);
        }
        return clone;
    }

    /// <summary>
    /// Parses a query string into parameters
    /// </summary>
    /// <param name="queryString">Query string with or without leading '?'</param>
    private void Parse(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return;

        // Remove leading '?' if present
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

            Add(key, value);
        }
    }

    /// <summary>
    /// Returns an enumerator for the first value of each parameter
    /// </summary>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var kvp in _parameters)
        {
            if (kvp.Value.Count > 0)
                yield return new KeyValuePair<string, string>(kvp.Key, kvp.Value[0]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Parses query parameters from a query string
    /// </summary>
    /// <param name="queryString">Query string with or without leading '?'</param>
    /// <returns>QueryParameters instance</returns>
    public static QueryParameters ParseString(string? queryString)
    {
        return new QueryParameters(queryString);
    }

    /// <summary>
    /// Parses query parameters from a URI
    /// </summary>
    /// <param name="uri">URI containing query parameters</param>
    /// <returns>QueryParameters instance</returns>
    public static QueryParameters Parse(Uri uri)
    {
        return new QueryParameters(uri);
    }

    public override string ToString() => ToQueryString();
}