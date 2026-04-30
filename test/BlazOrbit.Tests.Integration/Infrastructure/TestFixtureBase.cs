using BlazOrbit.Tests.Integration.Infrastructure.Contexts;

namespace BlazOrbit.Tests.Integration.Infrastructure;

public abstract class TestFixtureBase<TContext> : IClassFixture<TContext>
    where TContext : BlazorTestContextBase
{
    protected TestFixtureBase(TContext context) => Context = context;

    protected TContext Context { get; }
}