namespace Perforce.Tests;

public class ControllerTests
{
    private readonly Response _successfulResponseWithoutMessage = Response.BuildSuccessfulResponseWithoutMessage();
    private Controller _controller = new(Path.Combine(["depots", "DCC"]));

    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" })]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2023-2" })]
    public void Handle_CreateClient(string username, string clientName, string[] depotFolders)
    {
        Request request = Request.BuildCreateClientRequest(username, clientName, depotFolders);
        
        Response response = _controller.Handle(request);
        
        AssertResponsesAreEqual(_successfulResponseWithoutMessage, response);
    }

    private void AssertResponsesAreEqual(Response expected, Response actual)
    {
        Assert.Equal(expected.IsSuccessful, actual.IsSuccessful);
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(expected.Files, actual.Files);
    }

    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2233" })]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2024-2" })]
    public void Handle_TryCreateClientWithInvalidDepotPath(string username, string clientName, string[] depotFolders)
    {
        Request request = Request.BuildCreateClientRequest(username, clientName, depotFolders);
        Response expectedResponse = Response.BuildUnsuccessfulResponse("Invalid depot path");

        Response response = _controller.Handle(request);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" })]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2023-2" })]
    public void Handle_GetClient(string username, string clientName, string[] depotFolders)
    {
        Request getClientRequest = Request.BuildGetClientRequest(username, clientName);
        Response expectedResponse = Response.BuildSuccessfulResponseWithMessage($"{clientName} {Path.Combine(depotFolders)}");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(getClientRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]{
            "# IIC2113 2023-2\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza\n## Adyuantes\n- Francisco Gazitúa\n- Miguel Martinez",
            "# Raw Deal"
        }, new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2023-2" },
        new[]{
            "# IIC2113 2023-2\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza\n## Adyuantes\n- Francisco Gazitúa\n- Miguel Martinez",
            "# Raw Deal"
        }, new []
        {
            "README.md",
            "RawDeal README.md"
        })]
    public void Handle_Sync(string username, string clientName, string[] depotFolders, string[] contents, string[] pathsWithoutSeparatorChar)
    {
        Request syncRequest = Request.BuildSyncRequest(username, clientName);
        Response expectedResponse = Response.BuildSuccessfulResponseWithFilesFromContent(pathsWithoutSeparatorChar, contents);
      
        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(syncRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" }, "2024-1 README.md")]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2023-2" }, "Exam Uno README.md")]
    public void Handle_TrySyncANonExistingFile(string username, string clientName, string[] depotFolders, string pathWithoutSeparatorChar)
    {
        string filePath = Path.Combine(pathWithoutSeparatorChar.Split());
        Request syncRequest = Request.BuildSingleFileSyncRequest(username, clientName, filePath);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid file path {filePath}");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(syncRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]{
            "# IIC2113 2023-2\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza\n## Adyuantes\n- Francisco Gazitúa\n- Estefania Pakarati\n- Raimundo Murúa",
            "# Raw Deal\n- Owner: Rodrigo Toro"
        }, new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    public void Handle_SubmitAnEditSuccessfully(string username, string clientName, string[] depotFolders, string[] contents, string[] pathsWithoutSeparatorChar)
    {
        Response expectedResponse = Response.BuildSuccessfulResponseWithoutMessage();
        ChangeList changeList = new ChangeList("edit", AddSeparator(pathsWithoutSeparatorChar), contents);
        Request request = Request.BuildSubmitRequest(username, clientName, changeList);
        Request syncRequest = Request.BuildSingleFileSyncRequest(username, clientName, "");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(request);
        
        AssertResponsesAreEqual(_successfulResponseWithoutMessage, response);
        AssertMultipleSuccessfulSyncRequest(syncRequest, expectedResponse, contents, pathsWithoutSeparatorChar);
    }

    private string[] AddSeparator(string[] pathsWithoutSeparatorChar)
    {
        List<string> ret = new List<string>();
        for (int i = 0; i < pathsWithoutSeparatorChar.Length; i++)
            ret.Add(Path.Combine(pathsWithoutSeparatorChar[i].Split()));
        return ret.ToArray();
    }

    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    public void Handle_TrySubmitAnEditWithoutContent(string username, string clientName, string[] depotFolders, string[] pathsWithoutSeparatorChar)
    {
        ChangeList changeList = new ChangeList("edit", AddSeparator(pathsWithoutSeparatorChar));
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse =
            Response.BuildUnsuccessfulResponse(
                $"Invalid edit change: File {changeList.GetPathByIndex(0)} has no content");
     
        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]{
            "# IIC2113 2023-2\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza\n## Adyuantes\n- Francisco Gazitúa\n- Estefania Pakarati\n- Raimundo Murúa",
            "# Raw Deal\n- Owner: Rodrigo Toro"
        }, new[]
        {
            "2023-1 README.md",
            "2023-1 FireEmblem README.md"
        })]
    public void Handle_TrySubmitAnEditWithAFileThatDoesNotExists(string username, string clientName, string[] depotFolders, string[] contents, string[] pathsWithoutSeparatorChar)
    {
        ChangeList changeList = new ChangeList("edit", AddSeparator(pathsWithoutSeparatorChar), contents);
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid edit change: File {changeList.GetPathByIndex(0)} doesn't exists");
      
        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hvaldivieso","hvaldivieso-client", new[] { "DCC" },
        new[]{
            "# IIC2233 2024-1\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza",
            "# IIC2026 2024-1\n## Profesores\n- Hernán Valdivieso\n- Daniela Concha\n-Francisca Ibarra\n-Dante Pinto\n-Francisca Cattan",
        }, new[]
        {
            "IIC2233 2024-1 README.md",
            "IIC2026 2024-1 README.md"
        })]
    public void Handle_SubmitAnAddSuccessfully(string username, string clientName, string[] depotFolders, string[] contents, string[] pathsWithoutSeparatorChar)
    {
        _controller = new(Path.Combine(["depots"]));
        Response expectedResponse = Response.BuildSuccessfulResponseWithoutMessage();
        ChangeList changeList = new ChangeList("add", AddSeparator(pathsWithoutSeparatorChar), contents);
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Request syncRequest = Request.BuildSingleFileSyncRequest(username, clientName, "");
        
        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(_successfulResponseWithoutMessage, response);
        AssertMultipleSuccessfulSyncRequest(syncRequest, expectedResponse, contents, pathsWithoutSeparatorChar);
    }

    private void AssertMultipleSuccessfulSyncRequest(Request syncRequest, Response expectedResponse,
     string[] contents, string[] pathsWithoutSeparatorChar)
    {
        for (int i = 0; i < pathsWithoutSeparatorChar.Length; i++)
        {
            syncRequest.FilePath = Path.Combine(pathsWithoutSeparatorChar[i].Split());
            expectedResponse.Files = new();
            expectedResponse.Files.Add(syncRequest.FilePath, contents[i]);
            
            Response response = _controller.Handle(syncRequest);
          
            AssertResponsesAreEqual(expectedResponse, response);
        }
    }

    [Theory]
    [InlineData("hvaldivieso","hvaldivieso-client", new[] { "DCC" },
        new[]
        {
            "IIC2233 2024-1 README.md",
            "IIC2026 2024-1 README.md"
        })]
    public void Handle_TrySubmitAnAddWithoutContent(string username, string clientName, string[] depotFolders, string[] pathsWithoutSeparatorChar)
    {
        _controller = new Controller(Path.Combine(["depots"]));
        ChangeList changeList = new ChangeList("add", AddSeparator(pathsWithoutSeparatorChar));
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid add change: File {changeList.GetPathByIndex(0)} has no content");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]{
            "# IIC2113 2023-2\n## Profesores\n- Rodrigo Toro\n- Cristian Hinostroza\n## Adyuantes\n- Francisco Gazitúa\n- Estefania Pakarati\n- Raimundo Murúa",
            "# Raw Deal\n- Owner: Rodrigo Toro"
        }, new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    public void Handle_TrySubmitAnAddWithAFileThatExists(string username, string clientName, string[] depotFolders, string[] contents, string[] pathsWithoutSeparatorChar)
    {
        ChangeList changeList = new ChangeList("add", AddSeparator(pathsWithoutSeparatorChar), contents);
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid add change: File {changeList.GetPathByIndex(0)} already exists");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    public void Handle_SubmitADeleteSuccessfully(string username, string clientName, string[] depotFolders, string[] pathsWithoutSeparatorChar)
    {
        ChangeList changeList = new ChangeList("delete", AddSeparator(pathsWithoutSeparatorChar));
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Request syncRequest = Request.BuildSingleFileSyncRequest(username, clientName, "");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(_successfulResponseWithoutMessage, response);
        AssertMultipleInvalidSyncRequest(syncRequest, pathsWithoutSeparatorChar);
    }

    private void AssertMultipleInvalidSyncRequest(Request syncRequest, string[] pathsWithoutSeparatorChar)
    {
        foreach (var pathWithoutSeparatorChar in pathsWithoutSeparatorChar)
        {
            syncRequest.FilePath = Path.Combine(pathWithoutSeparatorChar.Split());
            Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid file path {syncRequest.FilePath}");

            Response response = _controller.Handle(syncRequest);

            AssertResponsesAreEqual(expectedResponse, response);
        }
    }

    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" },
        new[]
        {
            "2023-2 README.md",
            "2023-2 RawDeal README.md"
        })]
    public void Handle_TrySubmitAnDeleteWithContent(string username, string clientName, string[] depotFolders, string[] pathsWithoutSeparatorChar)
    {
        string[] contents = pathsWithoutSeparatorChar.Select(_=> "").ToArray();
        ChangeList changeList = new ChangeList("delete", AddSeparator(pathsWithoutSeparatorChar), contents);
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid delete change: File {changeList.GetPathByIndex(0)} has content");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" }, new[]
        {
            "2023-1 README.md",
            "2023-1 FireEmblem README.md"
        })]
    public void Handle_TrySubmitADeleteWithAFileThatDoesNotExists(string username, string clientName, string[] depotFolders, string[] pathsWithoutSeparatorChar)
    {
        ChangeList changeList = new ChangeList("delete", AddSeparator(pathsWithoutSeparatorChar));
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"Invalid delete change: File {changeList.GetPathByIndex(0)} doesn't exists");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" })]
    public void Handle_TrySubmitWithAnEmptyCL(string username, string clientName, string[] depotFolders)
    {
        ChangeList changeList = new ChangeList();
        Request submitRequest = Request.BuildSubmitRequest(username, clientName, changeList);
        Response expectedResponse = Response.BuildUnsuccessfulResponse("Invalid changelist");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" })]
    public void Handle_TrySubmitWithANullCL(string username, string clientName, string[] depotFolders)
    {
        Request submitRequest = new("submit", username, clientName);
        Response expectedResponse = Response.BuildUnsuccessfulResponse("Invalid changelist");
        
        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(submitRequest);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", "client")]
    [InlineData("rtoro","rtoro-client", "sync")]
    [InlineData("hin", "hin-client", "submit")]
    public void Handle_TryRequestWhenUserDoNotHaveClients(string username, string clientName, string requestType)
    {
        Request request = new(requestType, username, clientName);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"User {username} not found");

        Response response = _controller.Handle(request);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }
    
    [Theory]
    [InlineData("hin", "hin-client", new[] { "IIC2113" }, "cahinostroza", "client")]
    [InlineData("rtoro","rtoro-client", new[] { "IIC2113", "2023-2" }, "toro", "sync")]
    [InlineData("hin", "hin-client", new[] { "IIC2113" }, "cahinostroza", "submit")]
    public void Handle_TryRequestWhenClientDoesNotExists(string username, string clientName, string[] depotFolders, string otherClientName, string requestType)
    {
        Request request = new(requestType, username, otherClientName);
        Response expectedResponse = Response.BuildUnsuccessfulResponse($"User {username} doesn't have client {otherClientName}");

        CreateClient(username, clientName, depotFolders);
        Response response = _controller.Handle(request);
        
        AssertResponsesAreEqual(expectedResponse, response);
    }

    private void CreateClient(string username, string clientName, string[] depotFolders) {
        Request createClientRequest = Request.BuildCreateClientRequest(username, clientName, depotFolders);
        _controller.Handle(createClientRequest);
    }
}