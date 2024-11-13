namespace VersionControlSystem;

public interface IComponent
{
    void Add(FilePath filePath, string? content);
    bool Exists(FilePath path);
    Files Get(FilePath path, string pathToFile);
}