using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Xunit;

namespace Integrations.Inbox.Core.Test
{
    public class QueryArgumentsTest
    {
        [Fact]
        public void PageNumberDefaultsTo1WhenNotProvided()
        {
            var queryArgs = new QueryArguments(new PagingArguments());
            Assert.Equal(1, queryArgs.PageNumber);
        }

        [Fact]
        public void PageNumberUsedWhenExplicitlySetAndNoNextPageToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ PageNumber = 3 });
            Assert.Equal(3, queryArgs.PageNumber);
        }

        [Fact]
        public void PageNumberPulledFromNextPageTokenWhenOnlyValueInToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = "3" });
            Assert.Equal(3, queryArgs.PageNumber);
        }
        
        [Fact]
        public void PageNumberPulledFromNextPageTokenWhenSortFieldsIncludedInToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = "3:FieldA:true" });
            Assert.Equal(3, queryArgs.PageNumber);
        }
        
        [Fact]
        public void PageNumberDefaultsTo1WhenCantBePulledSuccessulyFromNextPageTokenWhenOnlyValueInToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = "a" });
            Assert.Equal(1, queryArgs.PageNumber);
        }
        
        [Fact]
        public void PageNumberDefaultsTo1WhenCantBePulledSuccessfullyFromNextPageTokenWhenSortFieldsIncludedInToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = "1:FieldA:true" });
            Assert.Equal(1, queryArgs.PageNumber);
        }
        
        [Fact]
        public void SortFieldDefaultsToNull()
        {
            var queryArgs = new QueryArguments(new PagingArguments());
            Assert.Null(queryArgs.SortField);
            Assert.True(queryArgs.SortAscending);
        }

        [Fact]
        public void SortFieldUsesPassedInValueIfProvidedOverToken()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ SortField  = "FieldB", SortAscending = false, NextPageToken = "1:FieldA:true" });
            Assert.Equal("FieldB", queryArgs.SortField);
            Assert.False(queryArgs.SortAscending);
        }

        [Fact]
        public void SortFieldCanBeExtractedFromNextPageTokenWithNoSortAscendingFlag()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = "1:FieldA" });
            Assert.Equal("FieldA", queryArgs.SortField);
            Assert.True(queryArgs.SortAscending);
        }

        [Theory]
        [InlineData("False")]
        [InlineData("false")]
        public void SortFieldAndSortAscendingCanBeExtractedFromNextPageToken(string sortAscending)
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = $"1:FieldA:{sortAscending}" });
            Assert.Equal("FieldA", queryArgs.SortField);
            Assert.False(queryArgs.SortAscending);
        }

        [Fact]
        public void SortAscendingHandledAsExpectedWhenInvalidBooleanValue()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = $"1:FieldA:x" });
            Assert.Equal("FieldA", queryArgs.SortField);
            Assert.True(queryArgs.SortAscending);
        }

        [Fact]
        public void SortFieldHandlesCamelCasedFieldNameAsExpected()
        {
            var queryArgs = new QueryArguments(new PagingArguments { SortField = "fieldA"});
            Assert.Equal("FieldA", queryArgs.SortField);
        }

        [Fact]
        public void SortFieldInNextPageTokenHandlesCamelCasedFieldNameAsExpected()
        {
            var queryArgs = new QueryArguments(new PagingArguments{ NextPageToken = $"1:fieldA:true" });
            Assert.Equal("FieldA", queryArgs.SortField);
        }
    }
}