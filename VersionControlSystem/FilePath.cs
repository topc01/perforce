namespace VersionControlSystem;

public class FilePath(string[] path)
{
    public static FilePath RemovePrefix(string path, string prefix)
    {
        string[] pathFolders = path.Split(Path.DirectorySeparatorChar);
        string[] prefixFolders = prefix.Split(Path.DirectorySeparatorChar);
        return new FilePath(pathFolders[prefixFolders.Length..]);
    }
    public static FilePath Combine(string prefix, string suffix)
    {
        string path = prefix + Path.DirectorySeparatorChar + suffix;
        return new FilePath(path);
    }

    public FilePath() : this(Array.Empty<string>())
    {
    }

    public FilePath(string path) : this(path.Split(Path.DirectorySeparatorChar))
    {
    }

    public int Length => path.Length;

    public string GetFirstFolder() => path[0];

    public FilePath GetSubPath() => new(path[1..]);
}