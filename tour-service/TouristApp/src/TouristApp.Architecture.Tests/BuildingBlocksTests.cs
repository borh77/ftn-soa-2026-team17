using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace TouristApp.Architecture.Tests;

public class BuildingBlocksTests : BaseArchitecturalTests
{
    [Fact]
    public void Core_should_not_reference_other_projects()
    {
        var examinedTypes = GetExaminedTypes("TouristApp.BuildingBlocks.Core");
        var forbiddenTypes = GetForbiddenTypes("TouristApp.BuildingBlocks.Core");

        var rule = Types().That().Are(examinedTypes).Should().NotDependOnAny(forbiddenTypes);

        rule.Check(Architecture);
    }

    [Fact]
    public void Infrastructure_should_not_reference_other_projects_apart_from_core()
    {
        var examinedTypes = GetExaminedTypes("TouristApp.BuildingBlocks.Infrastructure");
        var forbiddenTypes = GetForbiddenTypes("TouristApp.BuildingBlocks.Infrastructure", "TouristApp.BuildingBlocks.Core");

        var rule = Types().That().Are(examinedTypes).Should().NotDependOnAny(forbiddenTypes);

        rule.Check(Architecture);
    }
}