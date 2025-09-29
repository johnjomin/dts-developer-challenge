using FluentAssertions;

namespace CaseworkerTasks.Api.Tests;

public class HelloWorldTests
{
    [Fact]
    public void ShouldSayHello()
    {
        // Arrange & Act
        var greeting = "Hello, World!";

        // Assert
        greeting.Should().Be("Hello, World!");
    }
}