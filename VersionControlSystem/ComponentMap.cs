namespace VersionControlSystem;

class ComponentMap
{
    private readonly Dictionary<string, IComponent> _components = new();
    
    public bool Contains(string componentKey)
    {
        return _components.ContainsKey(componentKey);
    }

    public void Add(string key, IComponent component)
    {
        _components[key] = component;
    }

    public IComponent Get(string key)
    {
        return _components[key];
    }

    public Files GetFile(FilePath path, string pathToFile)
    {
        return _components[path.GetFirstFolder()].Get(path.GetSubPath(), pathToFile);
    }

    public Files GetAllFiles(FilePath path, string pathToFile)
    {
        Files files = new Files();
        foreach (var item in _components)
        {
            var childFiles = item.Value.Get(path, pathToFile != "" ? $"{pathToFile}{Path.DirectorySeparatorChar}{item.Key}" : item.Key);
            files.Concat(childFiles);
        }

        return files;
    }
}