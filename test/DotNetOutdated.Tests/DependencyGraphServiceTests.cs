using System.IO.Abstractions.TestingHelpers;
using DotNetOutdated.Core.Exceptions;
using DotNetOutdated.Core.Services;
using Moq;
using Xunit;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace DotNetOutdated.Tests;

public class DependencyGraphServiceTests
{
    private readonly string _path = XFS.Path(@"c:\path");
    private readonly string _solutionPath = XFS.Path(@"c:\path\proj.sln");

    [Fact]
    public async Task SuccessfulDotNetRunnerExecution_ReturnsDependencyGraph()
    {
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        // Arrange
        var dotNetRunner = new Mock<IDotNetRunner>();
        dotNetRunner.Setup(runner => runner.Run(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(Task.FromResult(new RunStatus(string.Empty, string.Empty, 0)))
            .Callback((string _, string[] arguments) =>
            {
                // Grab the temp filename that was passed...
                var tempFileName = arguments[3].Replace("/p:RestoreGraphOutputPath=", string.Empty).Trim('"');

                // ... and stuff it with our dummy dependency graph
                mockFileSystem.AddFileFromEmbeddedResource(tempFileName, GetType().Assembly, "DotNetOutdated.Tests.TestData.test.dg");
            });

        var graphService = new DependencyGraphService(dotNetRunner.Object, mockFileSystem);

        // Act
        var dependencyGraph = await graphService.GenerateDependencyGraphAsync(_path).ConfigureAwait(false);

        // Assert
        Assert.NotNull(dependencyGraph);
        Assert.Equal(3, dependencyGraph.Projects.Count);

        dotNetRunner.Verify(runner => runner.Run(XFS.Path(@"c:\"), It.Is<string[]>(a => a[0] == "msbuild" && a[1] == '\"' + _path + '\"')));
    }

    [Fact]
    public async Task UnsuccessfulDotNetRunnerExecution_Throws()
    {
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        // Arrange
        var dotNetRunner = new Mock<IDotNetRunner>();
        dotNetRunner.Setup(runner => runner.Run(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(Task.FromResult(new RunStatus(string.Empty, string.Empty, 1)));

        var graphService = new DependencyGraphService(dotNetRunner.Object, mockFileSystem);

        // Assert
        await Assert.ThrowsAsync<CommandValidationException>(async () => await graphService.GenerateDependencyGraphAsync(_path).ConfigureAwait(false)).ConfigureAwait(false);
    }

    [Fact]
    public async Task EmptySolution_ReturnsEmptyDependencyGraph()
    {
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        // Arrange
        var dotNetRunner = new Mock<IDotNetRunner>();

        dotNetRunner.Setup(runner => runner.Run(It.IsAny<string>(), It.Is<string[]>(a => a[0] == "msbuild" && a[2] == "/t:Restore,GenerateRestoreGraphFile")))
            .Returns(Task.FromResult(new RunStatus(string.Empty, string.Empty, 0)))
            .Callback((string _, string[] arguments) =>
            {
                // Grab the temp filename that was passed...
                var tempFileName = arguments[3].Replace("/p:RestoreGraphOutputPath=", string.Empty).Trim('"');

                // ... and stuff it with our dummy dependency graph
                mockFileSystem.AddFileFromEmbeddedResource(tempFileName, GetType().Assembly, "DotNetOutdated.Tests.TestData.empty.dg");
            });

        var graphService = new DependencyGraphService(dotNetRunner.Object, mockFileSystem);

        // Act
        var dependencyGraph = await graphService.GenerateDependencyGraphAsync(_solutionPath).ConfigureAwait(false);

        // Assert
        Assert.NotNull(dependencyGraph);
        Assert.Equal(0, dependencyGraph.Projects.Count);

        dotNetRunner.Verify(runner => runner.Run(_path, It.Is<string[]>(a => a[0] == "msbuild" && a[1] == '\"' + _solutionPath + '\"' && a[2] == "/t:Restore,GenerateRestoreGraphFile")));
    }
}
