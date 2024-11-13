using VersionControlSystem;

namespace Perforce;

public class Controller
{
    // Diccionario: nombreUsuario, luego nombreCliente, y finalmente el cliente
    private Dictionary<string, Dictionary<string, Client>> clients = new ();
    private IComponent _root;

    public Controller(string path)
    {
        // Al inicio, leemos los archivos ya existentes en el servidor desde la carpeta root
        _root = new VersionControlSystem.Directory();
        string[] paths = System.IO.Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var path1 in paths)
        {
            // Leemos un archivo
            IEnumerable<string> lines = File.ReadLines(path1);
            string content = string.Join("\n", lines);
            // Agregamos el archivo a root. Ojo que debemos eliminar "path" de "path1"
            // ... FilePath.BuildFromPathAndPrefix(...) hace eso por nosotros 
            _root.Add(FilePath.RemovePrefix(path1, path), content);
        }
    }
    
    public Response Handle(Request request)
    {
        Response response = new Response();

        if (request.Type == "client")
        {
            if (request.DepotPath == null) 
            {
                // Cuando el request es "client" pero no se especifica un DepotPath, se asume que se quiere obtener 
                // la información de ese cliente (por ejemplo, su DepotPath)
                //
                // Casos de error: El usuario o cliente no existen
                if (clients.ContainsKey(request.Username))
                {
                    if (clients[request.Username].ContainsKey(request.ClientName))
                        response.Message = clients[request.Username][request.ClientName].ToString();
                    else
                    {
                        response.IsSuccessful = false;
                        response.Message = $"User {request.Username} doesn't have client {request.ClientName}";
                    }
                }
                else
                {
                    response.IsSuccessful = false;
                    response.Message = $"User {request.Username} not found";
                }
            }
            else
            {
                // Cuando el request es "client" y se especifica un DepotPath, se asume que se quiere crear
                // un nuevo cliente con el depot entregado
                //
                // Casos de error: El cliente ya existe
                if (_root.Exists(new FilePath(request.DepotPath)))
                {
                    if (!clients.ContainsKey(request.Username)) clients[request.Username] = new Dictionary<string, Client>();
                    clients[request.Username][request.ClientName] = new Client(request.ClientName, request.DepotPath);
                }
                else
                {
                    response.IsSuccessful = false;
                    response.Message = "Invalid depot path";
                }
            }
        } else if ( request.Type == "sync")
        {
            if (clients.ContainsKey(request.Username))
            {
                if (clients[request.Username].ContainsKey(request.ClientName))
                {
                    Client client = clients[request.Username][request.ClientName];
                    if (request.FilePath == null)
                    {
                        // Cuando el request es "sync" pero no es especifica un FilePath sobre el cual sincronizar
                        // se asume que el cliente quiere obtener TODOS los archivos de su depot
                        //
                        // Casos de error: El cliente no existe (manejado más arriba)
                        FilePath path = new FilePath(client.DepotPath);
                        response.Files = _root.Get(path, ""); 
                    }
                    else
                    {
                        // Cuando el request es "sync" y se especifica un FilePath sobre el cual sincronizar
                        // se sincroniza solo el archivo referido en FilePath
                        //
                        // Casos de error: El cliente no existe (manejado más arriba) o el archivo no existe
                        
                        // El FilePath es la ruta al archivo dentro del depot del cliente. Para obtener la ruta 
                        // absoluta del archivo hay que combinar el client.DepotPath con request.FilePath
                        FilePath path = FilePath.Combine(client.DepotPath, request.FilePath!);
                        if (!_root.Exists(path))
                        {
                            response.IsSuccessful = false;
                            response.Message = $"Invalid file path {request.FilePath}";   
                        }
                        else
                        {
                            response.Files = _root.Get(path, request.FilePath);
                        }
                    }
                }
                else
                {
                    response.IsSuccessful = false;
                    response.Message = $"User {request.Username} doesn't have client {request.ClientName}";
                }
            }
            else
            {
                response.IsSuccessful = false;
                response.Message = $"User {request.Username} not found";
            }
        } else if (request.Type == "submit")
        {
            if (clients.ContainsKey(request.Username))
            {
                if (clients[request.Username].ContainsKey(request.ClientName))
                {
                    // Cuando el request es "submit" se realizan todos los cambios indicados en ChangeList
                    //
                    // Casos de error: El cliente no existe, los cambios son nulos o vacíos, y
                    //                 muchos otros (ver ChangeList.cs)
                    Client client = clients[request.Username][request.ClientName];
                    if (request.ChangeList == null || request.ChangeList.IsEmpty())
                    {
                        response.IsSuccessful = false;
                        response.Message = "Invalid changelist";
                    }
                    else
                    {
                        try
                        {
                            request.ChangeList.ApplyChanges(_root, client.DepotPath);
                        }
                        catch (Exception e)
                        {
                            response.IsSuccessful = false;
                            response.Message = e.Message;
                        }
                    }
                }
                else
                {
                    response.IsSuccessful = false;
                    response.Message = $"User {request.Username} doesn't have client {request.ClientName}";
                }
            }
            else
            {
                response.IsSuccessful = false;
                response.Message = $"User {request.Username} not found";
            }
        }
        // Se espera agregar más casos a futuro

        return response;
    }
}