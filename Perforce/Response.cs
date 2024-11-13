using VersionControlSystem;

namespace Perforce;

public struct Response {
    public bool IsSuccessful;
    public string? Message;
    public Files? Files;

    public static Response BuildSuccessfulResponseWithoutMessage()
        => new Response();

    public Response() 
        => IsSuccessful = true;

    public static Response BuildUnsuccessfulResponse(string message) 
        => new Response(false, message);

    private Response(bool isSuccessful, string message) {
        IsSuccessful = isSuccessful;
        Message = message;
    }

    public static Response BuildSuccessfulResponseWithFilesFromContent(string[] pathsWithoutSeparatorChar,
        string[] contents) => new(pathsWithoutSeparatorChar, contents);

    private Response(string[] pathsWithoutSeparatorChar, string[] contents) : this()
        => Files = new Files(pathsWithoutSeparatorChar, contents);

    public static Response BuildSuccessfulResponseWithMessage(string message)
        => new Response(message);

    private Response(string message) : this()
        => Message = message;
}