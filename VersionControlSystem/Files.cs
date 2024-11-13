

namespace VersionControlSystem;

public class Files {
    private Dictionary<string, string> _files = new();

    public Files(string[] pathsWithoutSeparatorChar, string[] contents) {
        for (int i = 0; i < pathsWithoutSeparatorChar.Length; i++)
        {
            AddFile(pathsWithoutSeparatorChar[i], contents[i]);
        }
    }

    private void AddFile(string pathWithoutSeparatorChar, string content) {
        string[] folders = pathWithoutSeparatorChar.Split();
        string path = Path.Combine(folders);
        _files[path] = content; 
    }

    public Files(){

    }

    public void Add(string path, string content)
    {
        _files[path] = content;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != typeof(Files)) {
            return false;
        }

        return AreAllElementsEqual((Files) obj);
    }

    private bool AreAllElementsEqual(Files filesToCompare)
    {
        if (_files.Count != filesToCompare._files.Count) return false;

        foreach (var item in _files)
        {
            if (!filesToCompare._files.ContainsKey(item.Key)) return false;
            if (filesToCompare._files[item.Key] != item.Value) return false;
        }

        return true;
    }

    public void Concat(Files files)
    {
        foreach (var item in files._files)
        {
            _files[item.Key] = item.Value;
        }
    }
}