using System.Collections.Concurrent;
using Karmak.Integrations.Volvo.Api.Infrastructure.Background;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Api.Tests.Infrastructure.Background
{
    public class ReactDataCleanupHostedServiceTests
    {
        private static readonly DateTimeOffset SixAmUtc = new(2026, 8, 1, 6, 0, 0, TimeSpan.Zero);
        private static readonly TimeSpan SevenAm = TimeSpan.FromHours(7);

        [Fact]
        public void GetNextRunUtc_RunTimeStillAhead_ReturnsToday()
        {
            var next = ReactDataCleanupHostedService.GetNextRunUtc(SixAmUtc, SevenAm);

            Assert.Equal(new DateTimeOffset(2026, 8, 1, 7, 0, 0, TimeSpan.Zero), next);
        }

        [Fact]
        public void GetNextRunUtc_RunTimeAlreadyPassed_ReturnsTomorrow()
        {
            var eightAmUtc = new DateTimeOffset(2026, 8, 1, 8, 0, 0, TimeSpan.Zero);

            var next = ReactDataCleanupHostedService.GetNextRunUtc(eightAmUtc, SevenAm);

            Assert.Equal(new DateTimeOffset(2026, 8, 2, 7, 0, 0, TimeSpan.Zero), next);
        }

        [Fact]
        public void GetNextRunUtc_ExactlyAtRunTime_ReturnsNowRatherThanSkippingADay()
        {
            var sevenAmUtc = new DateTimeOffset(2026, 8, 1, 7, 0, 0, TimeSpan.Zero);

            var next = ReactDataCleanupHostedService.GetNextRunUtc(sevenAmUtc, SevenAm);

            Assert.Equal(sevenAmUtc, next);
        }

        [Fact]
        public async Task ExecuteAsync_AtTheConfiguredTime_DeletesOncePerEntityTypeUsingTheRetentionCutoff()
        {
            var harness = new Harness();

            await harness.RunCyclesAsync(1);

            Assert.Equal(ReactDataEntityTypes.All, harness.Calls.Select(call => call.EntityType).ToList());
            Assert.All(harness.Calls, call =>
            {
                //Cleanup runs at 07:00 UTC, so a 365 day window cuts off at 07:00 UTC a year earlier
                Assert.Equal(new DateTime(2025, 8, 1, 7, 0, 0, DateTimeKind.Utc), call.CutoffUtc);
                Assert.Equal(1000, call.BatchSize);
            });
        }

        /// <summary>
        /// The clock does not move while a pass runs, so scheduling the next run off the current time
        /// would land back on the slot just completed and spin. The second cycle must be 24 hours out.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_OverTwoDays_RunsOncePerDayWithoutReFiringTheSameSlot()
        {
            var harness = new Harness();

            await harness.RunCyclesAsync(2);

            Assert.Equal(ReactDataEntityTypes.All.Count * 2, harness.Calls.Count);
            Assert.All(harness.Calls.Take(ReactDataEntityTypes.All.Count),
                call => Assert.Equal(new DateTime(2025, 8, 1, 7, 0, 0, DateTimeKind.Utc), call.CutoffUtc));
            Assert.All(harness.Calls.Skip(ReactDataEntityTypes.All.Count),
                call => Assert.Equal(new DateTime(2025, 8, 2, 7, 0, 0, DateTimeKind.Utc), call.CutoffUtc));
        }

        [Fact]
        public async Task ExecuteAsync_OneEntityTypeFails_StillDeletesTheRemainingEntityTypes()
        {
            var harness = new Harness { FailFor = ReactDataEntityTypes.All[1] };

            await harness.RunCyclesAsync(1);

            //The failing type is attempted and the rest still follow it
            Assert.Equal(ReactDataEntityTypes.All, harness.Calls.Select(call => call.EntityType).ToList());
        }

        [Fact]
        public async Task ExecuteAsync_Disabled_NeverTouchesTheRepository()
        {
            var harness = new Harness();
            harness.CleanupOptions.Enabled = false;

            await harness.RunNoOpAsync();

            Assert.Empty(harness.Calls);
        }

        [Fact]
        public async Task ExecuteAsync_NoConnectionStringConfigured_NeverTouchesTheRepository()
        {
            var harness = new Harness();
            harness.SqlOptions.ConnectionString = null;

            await harness.RunNoOpAsync();

            Assert.Empty(harness.Calls);
        }

        private sealed class Harness
        {
            //Real-clock allowances: how long to give the service's continuations to land, and how long
            //to watch for extra work after the expected calls have arrived
            private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);
            private static readonly TimeSpan SettlePeriod = TimeSpan.FromMilliseconds(250);

            private readonly IReactDataRepository _repository = Substitute.For<IReactDataRepository>();
            private readonly ConcurrentQueue<Call> _calls = new();

            public Harness()
            {
                _repository
                    .DeleteOlderThanAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                    .Returns(call =>
                    {
                        var entityType = (string)call[0];
                        _calls.Enqueue(new Call(entityType, (DateTime)call[1], (int)call[2]));

                        return entityType == FailFor
                            ? Task.FromException<int>(new InvalidOperationException("boom"))
                            : Task.FromResult(0);
                    });
            }

            public FakeTimeProvider TimeProvider { get; } = new(SixAmUtc);

            public SqlDataLayerOptions SqlOptions { get; } = new() { ConnectionString = "Server=(local);Database=Test;" };

            public ReactDataCleanupOptions CleanupOptions { get; } = new()
            {
                Enabled = true,
                RunAtUtc = SevenAm,
                RetentionDays = 365,
                BatchSize = 1000
            };

            /// <summary>Entity type whose delete should throw, or null for none.</summary>
            public string? FailFor { get; init; }

            public IReadOnlyList<Call> Calls => _calls.ToList();

            /// <summary>
            /// Starts the service, then walks the fake clock through <paramref name="cycles"/> daily runs,
            /// waiting for each one to land before moving the clock again.
            /// </summary>
            public async Task RunCyclesAsync(int cycles)
            {
                using var service = Build();
                await service.StartAsync(CancellationToken.None);

                for (int cycle = 1; cycle <= cycles; cycle++)
                {
                    //06:00 to the 07:00 slot, then a full day to each slot after it
                    TimeProvider.Advance(cycle == 1 ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1));

                    int expected = cycle * ReactDataEntityTypes.All.Count;
                    await WaitForCallsAsync(expected);

                    //A loop that re-fires its own slot would push the count past `expected` here
                    await Task.Delay(SettlePeriod);
                    Assert.Equal(expected, _calls.Count);
                }

                await service.StopAsync(CancellationToken.None);
            }

            /// <summary>
            /// Drives the cases that are expected to do nothing, where there is no call to wait on.
            /// </summary>
            public async Task RunNoOpAsync()
            {
                using var service = Build();
                await service.StartAsync(CancellationToken.None);

                TimeProvider.Advance(TimeSpan.FromDays(1));
                await Task.Delay(SettlePeriod);

                await service.StopAsync(CancellationToken.None);
            }

            private async Task WaitForCallsAsync(int expected)
            {
                var deadline = DateTime.UtcNow + WaitTimeout;

                while (_calls.Count < expected && DateTime.UtcNow < deadline)
                {
                    await Task.Delay(25);
                }

                Assert.Equal(expected, _calls.Count);
            }

            private ReactDataCleanupHostedService Build()
            {
                var provider = new ServiceCollection()
                    .AddSingleton(_repository)
                    .BuildServiceProvider();

                return new ReactDataCleanupHostedService(
                    provider,
                    Options.Create(SqlOptions),
                    Options.Create(CleanupOptions),
                    TimeProvider,
                    NullLogger<ReactDataCleanupHostedService>.Instance);
            }
        }

        private sealed record Call(string EntityType, DateTime CutoffUtc, int BatchSize);
    }
}
