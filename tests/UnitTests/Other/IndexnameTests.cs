// Copyright (c) Free Mind Labs, Inc. All rights reserved.
using FreeMindLabs.KernelMemory.Elasticsearch;
using FreeMindLabs.KernelMemory.Elasticsearch.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Other;

public class IndexnameTests
{
    private readonly ITestOutputHelper _output;

    public IndexnameTests(ITestOutputHelper output)
    {
        this._output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Theory]
    [InlineData("")] // default index
    [InlineData("nondefault")]
    [InlineData("WithUppercase")]
    [InlineData("With-Dashes")]
    [InlineData("123numberfirst")]
    public void GoodIndexNamesAreAccepted(string indexName)
    {
        Assert.True(Indexname.TryConvert(indexName, out var convResult));
        Assert.Empty(convResult.Errors);

        this._output.WriteLine($"The index name '{indexName}' will be translated to '{convResult.ActualIndexName}'.");
    }

    [Theory]
    // An index name cannot start with a hyphen (-) or underscore (_).
    [InlineData("-test", 1)]
    [InlineData("test_", 1)]
    // An index name can only contain letters, digits, and hyphens (-).
    [InlineData("test space", 1)]
    [InlineData("test/slash", 1)]
    [InlineData("test\\backslash", 1)]
    [InlineData("test.dot", 1)]
    [InlineData("test:colon", 1)]
    [InlineData("test*asterisk", 1)]
    [InlineData("test<less", 1)]
    [InlineData("test>greater", 1)]
    [InlineData("test|pipe", 1)]
    [InlineData("test?question", 1)]
    [InlineData("test\"quote", 1)]
    [InlineData("test'quote", 1)]
    [InlineData("test`backtick", 1)]
    [InlineData("test~tilde", 1)]
    [InlineData("test!exclamation", 1)]
    // Avoid names that are dot-only or dot and numbers
    // Multi error
    [InlineData(".", 2)]
    [InlineData("..", 2)]
    [InlineData("1.2.3", 2)]
    [InlineData("_test", 2)]

    public void BadIndexNamesAreRejected(string indexName, int errorCount)
    {
        // Creates the index using IMemoryDb
        var exception = Assert.Throws<InvalidIndexNameException>(() =>
        {
            Indexname.Convert(indexName);
        });

        this._output.WriteLine(
            $"The index name '{indexName}' had the following errors:\n{string.Join("\n", exception.Errors)}" +
            $"" +
            $"The expected number of errors was {errorCount}.");

        Assert.True(errorCount == exception.Errors.Count(), $"The number of errprs expected is different than the number of errors found.");
    }

    [Fact]
    public void IndexNameCannotBeLongerThan255Bytes()
    {
        var indexName = new string('a', 256);
        var exception = Assert.Throws<InvalidIndexNameException>(() =>
        {
            Indexname.Convert(indexName);
        });

        Assert.Equal(1, exception.Errors.Count());
    }
}
