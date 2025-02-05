using System.IO.Abstractions.TestingHelpers;
using DotNetOutdated.Core.Exceptions;
using DotNetOutdated.Core.Resources;
using DotNetOutdated.Core.Services;
using Xunit;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace DotNetOutdated.Tests;

public class ProjectDiscoveryServiceTests
{
    private readonly string _path = XFS.Path(@"c:\path");
    private readonly string _someOtherPath = XFS.Path(@"c:\another_path");
    private readonly string _solution1 = XFS.Path(@"c:\path\solution1.sln");
    private readonly string _solution2 = XFS.Path(@"c:\path\solution2.sln");
    private readonly string _solutionFilter1 = XFS.Path(@"c:\path\solution1.slnf");
    private readonly string _project1 = XFS.Path(@"c:\path\project1.csproj");
    private readonly string _project2 = XFS.Path(@"c:\path\project2.csproj");
    private readonly string _project3 = XFS.Path(@"c:\path\project3.fsproj");
    private readonly string _project4 = XFS.Path(@"c:\path\sub\project4.csproj");
    private readonly string _nonProjectFile = XFS.Path(@"c:\path\file.cs");

    [Fact]
    public void SingleSolution_ReturnsSolution()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _solution1, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act
        var project = projectDiscoveryService.DiscoverProjects(_path).Single();

        // Assert
        Assert.Equal(_solution1, project);
    }

    [Fact]
    public void MultipleSolutions_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _solution1, new MockFileData("")},
            { _solution2, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var exception = Assert.Throws<CommandValidationException>(() => projectDiscoveryService.DiscoverProjects(_path));
        Assert.Equal(string.Format(ValidationErrorMessages.DirectoryContainsMultipleSolutions, _path), exception.Message);
    }

    [Fact]
    public void SingleProject_ReturnsCsProject()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _project1, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act
        var project = projectDiscoveryService.DiscoverProjects(_path).Single();

        // Assert
        Assert.Equal(_project1, project);
    }

    [Fact]
    public void SingleProject_ReturnsFsProject()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _project3, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act
        var project = projectDiscoveryService.DiscoverProjects(_path).Single();

        // Assert
        Assert.Equal(_project3, project);
    }

    [Fact]
    public void MultipleProjects_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _project1, new MockFileData("")},
            { _project2, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var exception = Assert.Throws<CommandValidationException>(() => projectDiscoveryService.DiscoverProjects(_path));
        Assert.Equal(string.Format(ValidationErrorMessages.DirectoryContainsMultipleProjects, _path), exception.Message);
    }

    [Fact]
    public void MultipleProjectsRecursive_ReturnsProjects()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _project1, new MockFileData("")},
            { _project3, new MockFileData("")},
            { _project4, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var projects = projectDiscoveryService.DiscoverProjects(_path, true);
        Assert.Equal(3, projects.Count);
    }

    [Fact]
    public void NoSolutionsOrProjects_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var exception = Assert.Throws<CommandValidationException>(() => projectDiscoveryService.DiscoverProjects(_path));
        Assert.Equal(string.Format(ValidationErrorMessages.DirectoryDoesNotContainSolutionsOrProjects, _path), exception.Message);
    }

    [Fact]
    public void NonExistentPath_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>(), _someOtherPath);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var exception = Assert.Throws<CommandValidationException>(() => projectDiscoveryService.DiscoverProjects(_path));
        Assert.Equal(string.Format(ValidationErrorMessages.DirectoryOrFileDoesNotExist, _path), exception.Message);
    }


    [Fact]
    public void NonSolution_Throws()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {_nonProjectFile, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act

        // Assert
        var exception = Assert.Throws<CommandValidationException>(() => projectDiscoveryService.DiscoverProjects(_nonProjectFile));
        Assert.Equal(string.Format(ValidationErrorMessages.FileNotAValidSolutionOrProject, _nonProjectFile), exception.Message);
    }

    [Fact]
    public void SingleSolutionFilter_ReturnsSolution()
    {
        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { _solutionFilter1, new MockFileData("")}
        }, _path);
        var projectDiscoveryService = new ProjectDiscoveryService(fileSystem);

        // Act
        var projects = projectDiscoveryService.DiscoverProjects(_path);

        // Assert
        Assert.Single(projects);
        Assert.Equal(_solutionFilter1, projects.First());
    }
}
