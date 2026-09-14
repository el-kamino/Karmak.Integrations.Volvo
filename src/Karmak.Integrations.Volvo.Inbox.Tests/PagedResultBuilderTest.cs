using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Integrations.Inbox.Core.Test
{
    public class PagedResultBuilderTest
    {
        private List<MetaMessageEnvelope> _unPagedResults = new List<MetaMessageEnvelope>();
        private PagedResultBuilder<MetaMessageEnvelope> _builder;

        public PagedResultBuilderTest()
        {
            BuildResultSet();
            _builder = new PagedResultBuilder<MetaMessageEnvelope>(_unPagedResults);
        }

        [Fact]
        public void BuildDefaultPagedSet()
        {
            var queryArgs = new QueryArguments
            {
                PageNumber = 1,
                ItemsPerPage = 50
            };

            var results = _builder.Build(queryArgs);

            Assert.True(results.HasMoreItems);
            Assert.Equal(1, results.CurrentPage);
            Assert.Equal("2", results.NextPageToken);
            Assert.Equal(50, results.ItemsPerPage);
            Assert.Equal(50, results.Items.Count);
            Assert.Equal("1", results.Items.First().Id);
            Assert.Equal("50", results.Items.Last().Id);
        }

        [Theory]
        [InlineData(2, "51", "100")]
        [InlineData(3, "101", "150")]
        [InlineData(4, "151", "160")]
        public void CanMoveThroughPagedSetWithPageNumbers(int pageNumber, string firstId, string lastId)
        {
            var queryArgs = new QueryArguments
            {
                PageNumber = pageNumber,
                ItemsPerPage = 50
            };

            var results = _builder.Build(queryArgs);
            Assert.Equal(pageNumber, results.CurrentPage);
            if (pageNumber < 4)
            {
                Assert.True(results.HasMoreItems);
                Assert.Equal((pageNumber + 1).ToString(), results.NextPageToken);
            }
            else
            {
                Assert.False(results.HasMoreItems);
                Assert.Null(results.NextPageToken);
            }
            Assert.Equal(firstId, results.Items.First().Id);
            Assert.Equal(lastId, results.Items.Last().Id);
        }

        [Fact]
        public void CanSortByFileTypeDescAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 60,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = false
            };
            var results = _builder.Build(queryArgs);
            Assert.Equal("M350", results.Items.First().FileType);
            Assert.Equal("L250", results.Items.Last().FileType);
        }

        [Fact]
        public void CanSortByFileTypeAscAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 60,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = true
            };
            var results = _builder.Build(queryArgs);
            Assert.Equal("F150", results.Items.First().FileType);
            Assert.Equal("L250", results.Items.Last().FileType);
        }

        [Fact]
        public void CanPageThroughResultsSortedByFileTypeAscAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 10,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = true
            };
            var results = _builder.Build(queryArgs);

            var first1Id = int.Parse(results.Items[0].Id);
            var last1Id = int.Parse(results.Items[9].Id);
            Assert.True(last1Id > first1Id);
            Assert.True(results.NextPageToken.Contains(nameof(MetaMessageEnvelope.FileType)));
            Assert.True(results.NextPageToken.Contains(true.ToString()));

            queryArgs.PageNumber = int.Parse(results.NextPageToken.Split(':')[0]);
            results = _builder.Build(queryArgs);
            var first2Id = int.Parse(results.Items[0].Id);
            var last2Id = int.Parse(results.Items[9].Id);
            Assert.True(first2Id > last1Id);
            Assert.True(last2Id > first2Id);
        }

        [Fact]
        public void CanPageThroughResultsSortedByFileTypeDescAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 10,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = false
            };
            var results = _builder.Build(queryArgs);

            var first1Id = int.Parse(results.Items[0].Id);
            var last1Id = int.Parse(results.Items[9].Id);
            Assert.True(first1Id > last1Id);
            Assert.True(results.NextPageToken.Contains(nameof(MetaMessageEnvelope.FileType)));
            Assert.True(results.NextPageToken.Contains(false.ToString()));

            queryArgs.PageNumber = int.Parse(results.NextPageToken.Split(':')[0]);
            results = _builder.Build(queryArgs);
            var first2Id = int.Parse(results.Items[0].Id);
            var last2Id = int.Parse(results.Items[9].Id);
            Assert.True(last1Id > first2Id);
            Assert.True(first2Id > last2Id);
        }

        [Fact]
        public void BuildHandlesNoPageNumberAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 10,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = false
            };

            var results = _builder.Build(queryArgs);

            Assert.Equal(1, results.CurrentPage);
            Assert.True(results.NextPageToken.Contains("2"));
        }

        [Fact]
        public void BuildHandlesEmptyResultsAsExpected()
        {
            _unPagedResults.Clear();
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 10,
                SortField = nameof(MetaMessageEnvelope.FileType),
                SortAscending = false
            };

            var results = _builder.Build(queryArgs);
            Assert.NotNull(results);
            Assert.Null(results.NextPageToken);
            Assert.False(results.HasMoreItems);
            Assert.False(results.Items.Any());
            Assert.Equal(1, results.CurrentPage);
        }

        [Fact]
        public void BuildHandlesInvalidSortFieldAsExpected()
        {
            var queryArgs = new QueryArguments
            {
                ItemsPerPage = 10,
                SortField = "ThisIsAMadeUpFieldName",
                SortAscending = false
            };
            Assert.Throws<InboxDataException>(() => _builder.Build(queryArgs));
        }

        private void BuildResultSet()
        {
            for (int i = 1; i <= 160; i++)
            {
                _unPagedResults.Add(BuildFakeInboxMessage(i));
            }
        }

        private static MetaMessageEnvelope BuildFakeInboxMessage(int fileId)
        {
            string fileType;
            DateTime createdDate;

            switch (fileId % 3)
            {
                case 0:
                    fileType = "F150";
                    createdDate = DateTime.Today.AddDays(-1);
                    break;
                case 1:
                    fileType = "L250";
                    createdDate = DateTime.Today.AddDays(-10);
                    break;
                default:
                    fileType = "M350";
                    createdDate = DateTime.Today.AddDays(-6);
                    break;
            }

            return new MetaMessageEnvelope
            {
                Id = fileId.ToString(),
                MessageType = InboxMessageType.FixedWidthFile,
                IsActive = true,
                FileType = fileType,
                CreatedDate = createdDate
            };
        }
    }
}