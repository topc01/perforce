

using VersionControlSystem;

namespace Perforce;

public class ChangeList {
    // Tupla: (Tipo de cambio, ruta al archivo a cambiar, nuevo contenido del archivo)
    //        cuando el nuevo contenigo es null significa que el archivo debe ser borrado
    private List<(string, string, string?)> Changes = new();

    public ChangeList(string type, string[] filePaths)
    {
        for (int i = 0; i < filePaths.Length; i++)
            Changes.Add((type, filePaths[i], null));
    }

    public ChangeList(string type, string[] filePaths, string[] contents)
    {
        for (int i = 0; i < filePaths.Length; i++)
            Changes.Add((type, filePaths[i], contents[i]));
    }

    public ChangeList() {

    }

    public object GetPathByIndex(int index)
        => Changes[index].Item2;

    public bool IsEmpty()
        => Changes.Count == 0;

    public void Validate(IComponent _root, string path1)
    {
        foreach (var change in Changes)
        {
            FilePath path = FilePath.Combine(path1, change.Item2);
            if (change.Item1 == "add")
            {
                // Add significa agregar un nuevo archivo. Esto falla si es que el archivo ya existe
                // o si el contenido del archivo que se quiere agregar es "null"
                if (_root.Exists(path))
                {
                    throw new Exception($"Invalid add change: File {change.Item2} already exists");
                }

                if (change.Item3 == null)
                {
                    throw new Exception($"Invalid add change: File {change.Item2} has no content");
                }
            }
            if (change.Item1 == "delete")
            {
                // Para borrar un archivo se tiene que ingresar la ruta del archivo a borrar y decir que su nuevo
                // contenido es "null". Por eso la operación falla si es que el archivo no existe o el contenido
                // a subir es distinto de null
                if (!_root.Exists(path))
                {
                    throw new Exception($"Invalid delete change: File {change.Item2} doesn't exists");
                }
                
                if (change.Item3 != null)
                {
                    throw new Exception($"Invalid delete change: File {change.Item2} has content");
                }
            }
            if (change.Item1 == "edit")
            {
                // Para editar un archivo, el archivo debe existir y el nuevo contenido NO puede ser null
                // ... de lo contrario, equivaldría a borrar dicho archivo.
                if (!_root.Exists(path))
                {
                    throw new Exception($"Invalid edit change: File {change.Item2} doesn't exists");
                }
                
                if (change.Item3 == null)
                {
                    throw new Exception($"Invalid edit change: File {change.Item2} has no content");
                }
            }
            // Se espera agregar más casos a futuro
        }
    }

    public void ApplyChanges(IComponent root, string path1)
    {
        Validate(root, path1); // acá path1 es el depot
        foreach (var change in Changes)
        {
            // change.Item2 es la ruta del archivo desde el depot del cliente
            // por eso para obtener la ruta absoluta al archivo hay que combinar la ruta del depot con change.Item2
            FilePath path = FilePath.Combine(path1, change.Item2);
            root.Add(path, change.Item3);
        }
    }
}