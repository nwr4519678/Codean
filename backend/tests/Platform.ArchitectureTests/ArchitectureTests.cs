using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace Platform.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchitectureCache().Architecture;

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInNamespace("Platform.Domain..", true).As("Domain Layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInNamespace("Platform.Application..", true).As("Application Layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInNamespace("Platform.Infrastructure..", true).As("Infrastructure Layer");

    private static readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInNamespace("Platform.Api..", true).As("Api Layer");

    [Fact]
    public void Domain_ShouldNotDependOn_ApplicationOrInfrastructureOrApi()
    {
        IArchRule rule = Types()
            .That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void Application_ShouldNotDependOn_InfrastructureOrApi()
    {
        IArchRule rule = Types()
            .That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void Handlers_ShouldHaveNameEndingWithHandler()
    {
        IArchRule rule = Classes()
            .That().ResideInNamespace("Platform.Application..", true)
            .And().HaveNameEndingWith("Handler")
            .Should().BeSealed();

        rule.Check(Architecture);
    }
}

internal sealed class ArchitectureCache
{
    public Architecture Architecture { get; } = new ArchLoader()
        .LoadAssemblies(
            typeof(Platform.Domain.Primitives.AggregateRoot).Assembly,
            typeof(Platform.Application.Common.Abstractions.IUnitOfWork).Assembly,
            typeof(Platform.Infrastructure.DependencyInjection).Assembly,
            typeof(Platform.Api.Controllers.AuthController).Assembly
        )
        .Build();
}
