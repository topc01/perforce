namespace Perforce;

public class Request
{
    public readonly string Type;
    public readonly string Username;
    public readonly string ClientName;
    public readonly string? DepotPath;
    public ChangeList? ChangeList;
    public string? FilePath;

    public static Request BuildSubmitRequest(string username, string clientName, ChangeList changeList)
        => new Request("submit", username, clientName) { ChangeList = changeList };

    public Request(string type, string username, string clientName)
    {
        Type = type;
        Username = username;
        ClientName = clientName;
    }
    
    public static Request BuildSingleFileSyncRequest(string username, string clientName, string filePath)
        => new Request("sync", username, clientName) { FilePath = filePath };

    public static Request BuildSyncRequest(string username, string clientName)
        => new Request("sync", username, clientName);

    public static Request BuildGetClientRequest(string username, string clientName)
        => new Request("client", username, clientName);
    
    public static Request BuildCreateClientRequest(string username, string clientName, string[] depotFolders) 
        => new Request(username, clientName, depotFolders);

    private Request(string username, string clientName, string[] depotFolders) {
        Type = "client";
        Username = username;
        ClientName = clientName;
        DepotPath = Path.Combine(depotFolders);
    }
}