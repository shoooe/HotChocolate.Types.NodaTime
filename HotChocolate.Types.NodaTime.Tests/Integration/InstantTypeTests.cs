using System.Linq;
using HotChocolate.Execution;
using HotChocolate.Types.NodaTime.Extensions;
using NodaTime;
using Xunit;

namespace HotChocolate.Types.NodaTime.Tests
{
    public class InstantTypeIntegrationTests
    {
        public static class Schema
        {
            public class Query
            {
                public Instant One => Instant.FromUtc(2020, 02, 20, 17, 42, 59);
            }

            public class Mutation
            {
                public Instant Test(Instant arg)
                {
                    return arg + Duration.FromMinutes(10);
                }
            }
        }

        private readonly IQueryExecutor testExecutor;
        public InstantTypeIntegrationTests()
        {
            testExecutor = SchemaBuilder.New()
                .AddQueryType<Schema.Query>()
                .AddMutationType<Schema.Mutation>()
                .AddNodaTime()
                .Create()
                .MakeExecutable();
        }

        [Fact]
        public void QueryReturnsUtc()
        {
            var result = testExecutor.Execute("query { test: one }");
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("2020-02-20T17:42:59Z", queryResult.Data["test"]);
        }

        [Fact]
        public void ParsesVariable()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation($arg: Instant!) { test(arg: $arg) }")
                    .SetVariableValue("arg", "2020-02-21T17:42:59Z")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("2020-02-21T17:52:59Z", queryResult.Data["test"]);
        }

        [Fact]
        public void DoesntParseAnIncorrectVariable()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation($arg: Instant!) { test(arg: $arg) }")
                    .SetVariableValue("arg", "2020-02-20T17:42:59")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.DoesNotContain("test", queryResult.Data);
            Assert.Equal(1, queryResult.Errors.Count);
            Assert.Equal("EXEC_INVALID_TYPE", queryResult.Errors.First().Code);
        }

        [Fact]
        public void ParsesLiteral()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation { test(arg: \"2020-02-20T17:42:59Z\") }")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("2020-02-20T17:52:59Z", queryResult.Data["test"]);
        }

        [Fact]
        public void DoesntParseIncorrectLiteral()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation { test(arg: \"2020-02-20T17:42:59\") }")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.DoesNotContain("test", queryResult.Data);
            Assert.Equal(1, queryResult.Errors.Count);
            Assert.Null(queryResult.Errors.First().Code);
            Assert.Equal("Unable to deserialize string to Instant", queryResult.Errors.First().Message);
        }
    }
}
