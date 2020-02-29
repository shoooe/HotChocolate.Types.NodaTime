using System;
using System.Linq;
using HotChocolate.Execution;
using HotChocolate.Types.NodaTime.Extensions;
using NodaTime;
using Xunit;

namespace HotChocolate.Types.NodaTime.Tests
{
    public class OffsetTimeTypeIntegrationTests
    {
        public static class Schema
        {
            public class Query
            {
                public OffsetTime Hours =>
                    new OffsetTime(
                        LocalTime.FromHourMinuteSecondMillisecondTick(18, 30, 13, 10, 100),
                        Offset.FromHours(2));

                public OffsetTime HoursAndMinutes =>
                    new OffsetTime(
                        LocalTime.FromHourMinuteSecondMillisecondTick(18, 30, 13, 10, 100),
                        Offset.FromHoursAndMinutes(2, 35));
            }

            public class Mutation
            {
                public OffsetTime Test(OffsetTime arg) => arg;
            }
        }

        private readonly IQueryExecutor testExecutor;
        public OffsetTimeTypeIntegrationTests()
        {
            testExecutor = SchemaBuilder.New()
                .AddQueryType<Schema.Query>()
                .AddMutationType<Schema.Mutation>()
                .AddNodaTime()
                .Create()
                .MakeExecutable();
        }

        [Fact]
        public void QueryReturns()
        {
            var result = testExecutor.Execute("query { test: hours }");
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02", queryResult.Data["test"]);
        }

        [Fact]
        public void QueryReturnsWithMinutes()
        {
            var result = testExecutor.Execute("query { test: hoursAndMinutes }");
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02:35", queryResult.Data["test"]);
        }

        [Fact]
        public void ParsesVariable()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation($arg: OffsetTime!) { test(arg: $arg) }")
                    .SetVariableValue("arg", "18:30:13+02")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02", queryResult.Data["test"]);
        }

        [Fact]
        public void ParsesVariableWithMinutes()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation($arg: OffsetTime!) { test(arg: $arg) }")
                    .SetVariableValue("arg", "18:30:13+02:35")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02:35", queryResult.Data["test"]);
        }

        [Fact]
        public void DoesntParseAnIncorrectVariable()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation($arg: OffsetTime!) { test(arg: $arg) }")
                    .SetVariableValue("arg", "18:30:13")
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
                    .SetQuery("mutation { test(arg: \"18:30:13+02\") }")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02", queryResult.Data["test"]);
        }

        [Fact]
        public void ParsesLiteralWithMinutes()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation { test(arg: \"18:30:13+02:35\") }")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.Equal("18:30:13+02:35", queryResult.Data["test"]);
        }

        [Fact]
        public void DoesntParseIncorrectLiteral()
        {
            var result = testExecutor
                .Execute(QueryRequestBuilder.New()
                    .SetQuery("mutation { test(arg: \"18:30:13\") }")
                    .Create());
            var queryResult = result as IReadOnlyQueryResult;
            Assert.DoesNotContain("test", queryResult.Data);
            Assert.Equal(1, queryResult.Errors.Count);
            Assert.Null(queryResult.Errors.First().Code);
            Assert.Equal("Unable to deserialize string to OffsetTime", queryResult.Errors.First().Message);
        }
    }
}