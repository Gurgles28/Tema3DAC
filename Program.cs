using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Citire credențiale din appsettings.json
        var config = JObject.Parse(File.ReadAllText("appsettings.json"));
        string clientId = config["GoogleDrive"]["ClientId"].ToString();
        string clientSecret = config["GoogleDrive"]["ClientSecret"].ToString();

        // Setarea de credențiale OAuth 2.0
        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            new[] { DriveService.Scope.Drive },
            "user",
            CancellationToken.None,
            new FileDataStore("GoogleDriveConsoleApp")
        );

        // Inițializarea serviciului Google Drive
        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GoogleDriveConsoleApp"
        });

        // Obținerea fișierelor din Google Drive (nivelul root)
        var request = service.Files.List();
        request.Q = "'root' in parents";
        request.Fields = "files(id, name)";

        var result = await request.ExecuteAsync();

        // Afișarea fișierelor în consolă
        Console.WriteLine("Fișiere în Google Drive:");
        foreach (var file in result.Files)
        {
            Console.WriteLine($"ID: {file.Id}, Name: {file.Name}");
        }

        // Apelarea funcției de încărcare a fișierului
        await UploadFile(service);
    }

    static async Task UploadFile(DriveService service)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = "SampleFile.txt",
            Parents = new List<string> { "root" }
        };

        // Fișierul care va fi încărcat (exemplu cu un fișier text)
        using var stream = new FileStream("SampleFile.txt", FileMode.Open);
        var request = service.Files.Create(fileMetadata, stream, "text/plain");
        request.Fields = "id";
        var file = await request.UploadAsync();

        if (file.Status == Google.Apis.Upload.UploadStatus.Completed)
        {
            Console.WriteLine("Fișierul a fost încărcat cu succes.");
        }
        else
        {
            Console.WriteLine("Încărcarea fișierului a eșuat.");
        }
    }
}
