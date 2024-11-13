namespace VersionControlSystem;

class ContentManager(ComponentMap components,
    FilePath filePath, string? content)
{
    public void Handle()
    {
        if (IsPathAFileName())
            AddContentToFile();
        else
        {
            AddContentToChildDirectory();
        }
    }
    
    private bool IsPathAFileName()
    {
        return filePath.Length == 1;
    }
    
    private void AddContentToFile()
    {
        if (!components.Contains(filePath.GetFirstFolder()))
            components.Add(filePath.GetFirstFolder(), new File(content!));
        else
        {
            IComponent component = components.Get(filePath.GetFirstFolder());
            component.Add(new FilePath(), content);
        }
    }
    
    private void AddContentToChildDirectory()
    {
        if (!components.Contains(filePath.GetFirstFolder()))
            components.Add(filePath.GetFirstFolder(), new Directory());
        IComponent component = components.Get(filePath.GetFirstFolder());
        component.Add(filePath.GetSubPath(), content);
    }
}