﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marten.Linq;
using Shouldly;
using Xunit;

namespace Marten.Testing.Bugs
{
    public class Bug_605_unary_expressions_in_where_clause_of_compiled_query : IntegratedFixture
    {
        [Fact]
        public void with_flag_as_true()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var results = query.Query(new FlaggedTrueTargets());

                var expected = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Where(x => x.Color == Colors.Green)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(20)
                    .Take(15)
                    .ToList();

                results.Count().ShouldBe(15);


                results.Select(x => x.Id)
                    .ShouldHaveTheSameElementsAs(expected.Select(x => x.Id));
            }
        }

        [Fact]
        public void with_flag_as_false()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            theStore.BulkInsert(targets);

            using (var query = theStore.QuerySession())
            {
                var results = query.Query(new FlaggedFalseTargets());

                var expected = query.Query<Target>()
                    .SelectMany(x => x.Children)
                    .Where(x => x.Color == Colors.Green)
                    .Where(x => !x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(20)
                    .Take(15)
                    .ToList();

                results.Count().ShouldBe(15);


                results.Select(x => x.Id)
                    .ShouldHaveTheSameElementsAs(expected.Select(x => x.Id));
            }
        }


        public class FlaggedTrueTargets : ICompiledListQuery<Target>
        {
            public Expression<Func<IQueryable<Target>, IEnumerable<Target>>> QueryIs()
            {
                return q => q.SelectMany(x => x.Children)
                    .Where(x => x.Color == Color)
                    .Where(x => x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(Skip)
                    .Take(Take);
            }

            public Colors Color { get; set; } = Colors.Green;

            public int Skip { get; set; } = 20;
            public int Take { get; set; } = 15;
        }

        public class FlaggedFalseTargets : ICompiledListQuery<Target>
        {
            public Expression<Func<IQueryable<Target>, IEnumerable<Target>>> QueryIs()
            {
                return q => q.SelectMany(x => x.Children)
                    .Where(x => x.Color == Colors.Green)
                    .Where(x => !x.Flag)
                    .OrderBy(x => x.Id)
                    .Skip(Skip)
                    .Take(Take);
            }

            public int Skip { get; set; } = 20;
            public int Take { get; set; } = 15;
        }
    }
}