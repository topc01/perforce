namespace Perforce;

public class Client
{
    public string ClientName;
    public string DepotPath;
    
    public Client(string clientName, string depotPath)
    {
        ClientName = clientName;
        DepotPath = depotPath;
    }

    public override string ToString()
        => $"{ClientName} {DepotPath}";
}