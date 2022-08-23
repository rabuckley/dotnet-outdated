using System.IO.Abstractions.TestingHelpers;
using DotNetOutdated.Core.Services;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace DotNetOutdated.Tests;

public class CentralPackageVersionManagementTests
{
    [Fact]
    public void UpgradingCPVMEnabledPackage_UpdatesNearestCPVMFile()
    {
        SetupCPVMMocks(out var mockRestoreService, out var mockFileSystem, out var path, out var nearestCPVMFilePath, out var rootCPVMFilePath, out var rootCPVMFileContent, out _, out _);

        var subject = new CentralPackageVersionManagementService(mockFileSystem, mockRestoreService.Object);
        var status = subject.AddPackage(path, "FakePackage", new NuGetVersion(2, 0, 0), false);

        Assert.NotNull(status);
        Assert.Equal(0, status.ExitCode);

        Assert.Equal(rootCPVMFileContent, mockFileSystem.GetFile(rootCPVMFilePath).TextContents);
        Assert.NotEqual(rootCPVMFileContent, mockFileSystem.GetFile(nearestCPVMFilePath).TextContents);
    }

    [Fact]
    public void UpgradingCPVMEnabledPackage_DoesNotModifyProjectFile()
    {
        SetupCPVMMocks(out var mockRestoreService, out var mockFileSystem, out var path, out _, out _, out _, out _, out var projectFileContent);

        var subject = new CentralPackageVersionManagementService(mockFileSystem, mockRestoreService.Object);
        subject.AddPackage(path, "FakePackage", new NuGetVersion(2, 0, 0), false);

        Assert.Equal(projectFileContent, mockFileSystem.GetFile(path).TextContents);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpgradingCPVMEnabledPackage_RespectsNoRestoreFlag(bool noRestore)
    {
        SetupCommonMocks(out var mockRestoreService, out var mockFileSystem, out var projectPath, out _);

        var subject = new CentralPackageVersionManagementService(mockFileSystem, mockRestoreService.Object);
        subject.AddPackage(projectPath, "FakePackage", new NuGetVersion(1, 0, 0), noRestore);

        if (noRestore)
        {
            Assert.Empty(mockRestoreService.Invocations);
        }
        else
        {
            mockRestoreService.Verify(x => x.Restore(projectPath));
        }
    }

    private void SetupCPVMMocks(out Mock<IDotNetRestoreService> mockRestoreService, out MockFileSystem mockFileSystem, out string projectPath, out string nearestCPVMFilePath, out string rootCPVMFilePath, out string rootCPVMFileContent, out string nearestCPVMFileContent, out string projectFileContent)
    {
        SetupCommonMocks(out mockRestoreService, out mockFileSystem, out projectPath, out projectFileContent);

        nearestCPVMFilePath = MockUnixSupport.Path(@"c:\source\project\Directory.Packages.props");
        mockFileSystem.AddFileFromEmbeddedResource(nearestCPVMFilePath, GetType().Assembly, "DotNetOutdated.Tests.TestData.CPVMFile.props");
        nearestCPVMFileContent = mockFileSystem.GetFile(nearestCPVMFilePath).TextContents;

        rootCPVMFilePath = MockUnixSupport.Path(@"c:\source\Directory.Packages.props");
        mockFileSystem.AddFileFromEmbeddedResource(rootCPVMFilePath, GetType().Assembly, "DotNetOutdated.Tests.TestData.CPVMFile.props");
        rootCPVMFileContent = mockFileSystem.GetFile(rootCPVMFilePath).TextContents;
    }

    private void SetupCommonMocks(out Mock<IDotNetRestoreService> mockRestoreService, out MockFileSystem mockFileSystem, out string projectPath, out string projectFileContent)
    {
        mockRestoreService = new Mock<IDotNetRestoreService>();
        mockRestoreService.Setup(x => x.Restore(It.IsAny<string>())).Returns(new RunStatus(string.Empty, string.Empty, 0));

        mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        projectPath = MockUnixSupport.Path(@"c:\source\project\app\app.csproj");

        mockFileSystem.AddFileFromEmbeddedResource(projectPath, GetType().Assembly, "DotNetOutdated.Tests.TestData.CPVMProject.csproj");
        projectFileContent = mockFileSystem.GetFile(projectPath).TextContents;
    }
}
