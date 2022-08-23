namespace DotNetOutdated.Core;

class TempDirectory : IDisposable
{
    private readonly string tempPath;
    private readonly string tempDirName;

    public TempDirectory()
    {
        tempPath = Path.GetTempPath();
        tempDirName = Path.GetRandomFileName();
        Directory.CreateDirectory(DirectoryPath);
    }

    public void Dispose()
    {
        Directory.Delete(DirectoryPath, true);
    }

    public string DirectoryPath => Path.Combine(tempPath, tempDirName);
}
