namespace VersionControlSystem;

public class Directory : IComponent
{
    private readonly ComponentMap _children = new();
    public void Add(FilePath filePath, string? content)
    {
        ContentManager manager = new ContentManager(_children, filePath, content);
        manager.Handle();
    }

    public bool Exists(FilePath path)
    {
        if (path.Length == 0) return true;
        return _children.Contains(path.GetFirstFolder()) 
               && _children.Get(path.GetFirstFolder()).Exists(path.GetSubPath());
    }

    public Files Get(FilePath path, string pathToFile)
    {
        return path.Length != 0 ? _children.GetFile(path, pathToFile) : _children.GetAllFiles(path, pathToFile);
    }
}