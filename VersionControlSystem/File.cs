namespace VersionControlSystem;

class File(string content) : IComponent
{
    private readonly List<string?> _versions = [content];

    public void Add(FilePath filePath, string? content)
    {
        _versions.Add(content);
    }

    public bool Exists(FilePath path)
    {
        return _versions[^1] != null;
    }

    public Files Get(FilePath path, string pathToFile)
    {
        Files files = new Files();
        if (_versions[^1] != null)
            files.Add(pathToFile, _versions[^1]!);
        return files;
    }
}