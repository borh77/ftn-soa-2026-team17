using TouristApp.BuildingBlocks.Tests;
using TouristApp.Blog.Infrastructure.Database;

namespace TouristApp.Blog.Tests;

/// <summary>
/// Base class for Blog integration tests.
/// </summary>
public abstract class BaseBlogIntegrationTest : BaseWebIntegrationTest<BlogTestFactory>
{
    protected BaseBlogIntegrationTest(BlogTestFactory factory) : base(factory) { }
}
