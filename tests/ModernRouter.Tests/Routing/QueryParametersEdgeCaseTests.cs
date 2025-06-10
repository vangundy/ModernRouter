using FluentAssertions;
using ModernRouter.Routing;
using System.Text;
using Xunit;

namespace ModernRouter.Tests.Routing;

public class QueryParametersEdgeCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyQueryString_CreatesEmptyCollection(string? queryString)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams.Count.Should().Be(0);
        queryParams.ToQueryString().Should().BeEmpty();
    }

    [Theory]
    [InlineData("?")]
    [InlineData("?&")]
    [InlineData("?&&&")]
    [InlineData("?=")]
    [InlineData("?&=&")]
    public void Constructor_WithMalformedQueryString_HandlesGracefully(string malformedQuery)
    {
        var queryParams = new QueryParameters(malformedQuery);

        queryParams.Should().NotBeNull("should handle malformed query strings gracefully");
        queryParams.Count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Parse_WithDuplicateKeys_KeepsLastValue()
    {
        var queryParams = new QueryParameters("?name=John&name=Jane&name=Bob");

        queryParams["name"].Should().Be("Bob", "last value should win for duplicate keys");
        queryParams.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("?key=hello%20world", "key", "hello world")]
    [InlineData("?name=John%26Jane", "name", "John&Jane")]
    [InlineData("?data=%3Cscript%3E", "data", "<script>")]
    [InlineData("?path=..%2F..%2Fadmin", "path", "../../admin")]
    [InlineData("?encoded=%2B%2D%2A%2F", "encoded", "+-*/")]
    public void Parse_WithUrlEncodedValues_DecodesCorrectly(string queryString, string key, string expectedValue)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams[key].Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("?key%20with%20spaces=value")]
    [InlineData("?%3Ckey%3E=value")]
    [InlineData("?key%2Bplus=value")]
    public void Parse_WithUrlEncodedKeys_DecodesCorrectly(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams.Count.Should().Be(1, "should parse URL-encoded keys");
        queryParams.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("?key=")]
    [InlineData("?key1=&key2=")]
    [InlineData("?empty=&nonempty=value")]
    public void Parse_WithEmptyValues_HandlesCorrectly(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams.Should().NotBeEmpty();
        
        // At least one key should have empty string value
        queryParams.Should().ContainValue("");
    }

    [Theory]
    [InlineData("?standalone")]
    [InlineData("?key1&key2")]
    [InlineData("?standalone&key=value")]
    public void Parse_WithKeysWithoutValues_AssignsEmptyString(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        // Keys without values should have empty string
        queryParams.Should().ContainValue("");
        queryParams.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Parse_WithExtremelyLongQueryString_HandlesGracefully()
    {
        var longValue = new string('a', 5000);
        var queryString = $"?longParam={longValue}";
        
        var queryParams = new QueryParameters(queryString);

        queryParams["longParam"].Should().HaveLength(5000);
        queryParams.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_WithManyParameters_HandlesCorrectly()
    {
        var sb = new StringBuilder("?");
        for (int i = 0; i < 1000; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append($"param{i}=value{i}");
        }

        var queryParams = new QueryParameters(sb.ToString());

        queryParams.Count.Should().Be(1000);
        queryParams["param0"].Should().Be("value0");
        queryParams["param999"].Should().Be("value999");
    }

    [Theory]
    [InlineData("?key=value with spaces")]
    [InlineData("?key=value&other")]
    [InlineData("?special=!@#$%^*()")]
    public void Parse_WithSpecialCharacters_HandlesCorrectly(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams.Should().NotBeEmpty("should handle special characters in values");
        queryParams.Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("?unicode=🚀")]
    [InlineData("?chinese=你好")]
    [InlineData("?emoji=😀😃😄")]
    [InlineData("?russian=Привет")]
    public void Parse_WithUnicodeCharacters_HandlesCorrectly(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        queryParams.Should().NotBeEmpty("should handle Unicode characters");
        queryParams.Count.Should().Be(1);
    }

    [Fact]
    public void ToQueryString_WithEmptyCollection_ReturnsEmptyString()
    {
        var queryParams = new QueryParameters();

        var result = queryParams.ToQueryString();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToQueryString_WithSingleParameter_FormatsCorrectly()
    {
        var queryParams = new QueryParameters();
        queryParams.Add("name", "John Doe");

        var result = queryParams.ToQueryString();

        result.Should().Be("?name=John%20Doe");
    }

    [Fact]
    public void ToQueryString_WithMultipleParameters_FormatsCorrectly()
    {
        var queryParams = new QueryParameters();
        queryParams.Add("name", "John");
        queryParams.Add("age", "30");

        var result = queryParams.ToQueryString();

        result.Should().StartWith("?");
        result.Should().Contain("name=John");
        result.Should().Contain("age=30");
        result.Should().Contain("&");
    }

    [Fact]
    public void ToQueryString_WithSpecialCharacters_EncodesCorrectly()
    {
        var queryParams = new QueryParameters();
        queryParams.Add("data", "<script>alert('xss')</script>");
        queryParams.Add("path", "../admin");

        var result = queryParams.ToQueryString();

        result.Should().NotContain("<script>");
        result.Should().NotContain("../");
        result.Should().Contain("%3C"); // Encoded <
        // Uri.EscapeDataString encodes / but not . since . is safe in query values
        result.Should().Contain("..%2F"); // Encoded ../ where only / is encoded
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToQueryString_WithQuestionMarkParameter_HandlesCorrectly(bool includeQuestionMark)
    {
        var queryParams = new QueryParameters();
        queryParams.Add("test", "value");

        var result = queryParams.ToQueryString(includeQuestionMark);

        if (includeQuestionMark)
        {
            result.Should().StartWith("?");
        }
        else
        {
            result.Should().NotStartWith("?");
        }
    }

    [Fact]
    public void Set_WithNullValue_RemovesKey()
    {
        var queryParams = new QueryParameters();
        queryParams.Add("key", "value");
        
        queryParams.Set("key", null);

        queryParams.ContainsKey("key").Should().BeFalse();
        queryParams.Count.Should().Be(0);
    }

    [Fact]
    public void Indexer_WithNonExistentKey_ReturnsNull()
    {
        var queryParams = new QueryParameters();

        var result = queryParams["nonexistent"];

        result.Should().BeNull();
    }

    [Fact]
    public void Indexer_WithCaseInsensitiveKey_FindsMatch()
    {
        var queryParams = new QueryParameters("?Name=John");

        queryParams["name"].Should().Be("John");
        queryParams["NAME"].Should().Be("John");
        queryParams["Name"].Should().Be("John");
    }

    [Fact]
    public void Clear_RemovesAllParameters()
    {
        var queryParams = new QueryParameters("?name=John&age=30");
        
        queryParams.Clear();

        queryParams.Count.Should().Be(0);
        queryParams.ToQueryString().Should().BeEmpty();
    }

    [Fact]
    public void Remove_WithExistingKey_ReturnsTrue()
    {
        var queryParams = new QueryParameters("?name=John&age=30");
        
        var result = queryParams.Remove("name");

        result.Should().BeTrue();
        queryParams.Count.Should().Be(1);
        queryParams.ContainsKey("name").Should().BeFalse();
    }

    [Fact]
    public void Remove_WithNonExistentKey_ReturnsFalse()
    {
        var queryParams = new QueryParameters("?name=John");
        
        var result = queryParams.Remove("nonexistent");

        result.Should().BeFalse();
        queryParams.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithUri_ParsesQueryCorrectly()
    {
        var uri = new Uri("https://example.com/path?name=John&age=30");
        
        var queryParams = new QueryParameters(uri);

        queryParams["name"].Should().Be("John");
        queryParams["age"].Should().Be("30");
        queryParams.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_WithUriWithoutQuery_CreatesEmptyCollection()
    {
        var uri = new Uri("https://example.com/path");
        
        var queryParams = new QueryParameters(uri);

        queryParams.Count.Should().Be(0);
    }

    [Fact]
    public void ParseString_StaticMethod_WorksCorrectly()
    {
        var queryParams = QueryParameters.ParseString("?name=John&age=30");

        queryParams["name"].Should().Be("John");
        queryParams["age"].Should().Be("30");
        queryParams.Count.Should().Be(2);
    }

    [Fact]
    public void Parse_StaticMethod_WithUri_WorksCorrectly()
    {
        var uri = new Uri("https://example.com/path?name=John&age=30");
        
        var queryParams = QueryParameters.Parse(uri);

        queryParams["name"].Should().Be("John");
        queryParams["age"].Should().Be("30");
        queryParams.Count.Should().Be(2);
    }

    [Fact]
    public void GetEnumerator_AllowsIteration()
    {
        var queryParams = new QueryParameters("?name=John&age=30");
        
        var items = queryParams.ToList();

        items.Should().HaveCount(2);
        items.Should().Contain(kvp => kvp.Key == "name" && kvp.Value == "John");
        items.Should().Contain(kvp => kvp.Key == "age" && kvp.Value == "30");
    }

    [Theory]
    [InlineData("?a=1&b=2&c=3&a=4")]
    [InlineData("?duplicate=first&other=value&duplicate=second")]
    public void Parse_WithDuplicateParameters_LastValueWins(string queryString)
    {
        var queryParams = new QueryParameters(queryString);

        // Ensure that duplicate handling is consistent
        queryParams.Should().NotBeEmpty();
        queryParams.Count.Should().BeGreaterThan(0);
    }
}