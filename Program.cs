using ForgeModPackDownloader;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Immutable;
using System.Security;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Net.Mime;

public partial class Program
{

    static Stream Zip(string loc)
    {
        Stream stream = new FileStream(loc, FileMode.Open);

        ZipArchive archive = new ZipArchive(stream);


        foreach (var ent in archive.Entries)
        {
            if (ent.FullName.EndsWith(".json"))
                return ent.Open();
        }
        return null;
    }
    public static void Main(string[] args)
    {
        string zzipFileLocation = "";

        if (args.Length > 0)
        {
            zzipFileLocation= args[0];
        }
        else
        {
            Console.WriteLine("Please drag and drop the mod pack zip file");
            zzipFileLocation = Console.ReadLine();
        }
        if (!File.Exists(zzipFileLocation)) 
        {
            Console.WriteLine($"File \"{zzipFileLocation}\" doesn't exist!");
            return;
        }

        Stream jsonStream;
        if (zzipFileLocation.EndsWith(".zip"))
        {
            jsonStream = Zip(zzipFileLocation);
        }
        else
        {
            jsonStream = new FileStream(zzipFileLocation, FileMode.Open);
        }
        string jsonText;
        using (StreamReader reader = new StreamReader(jsonStream))
        {
            jsonText = reader.ReadToEnd();
        }
        JsonSerializerOptions options = new JsonSerializerOptions()
        { 
            AllowTrailingCommas = true,
           
        };
    
        packMetaData metaData = JsonSerializer.Deserialize<packMetaData>(jsonText, options);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Name: " + metaData.name);
        Console.WriteLine($"Minecraft Version: " + metaData.minecraft.version);
        Console.WriteLine($"ModLoader: " + metaData.minecraft.modLoaders[0].id);
        Console.WriteLine($"Number of mods: " + metaData.files.Count);
        Console.ForegroundColor = ConsoleColor.Gray;

        //CookieContainer cookie = cookies();
        string downloadFolder = Directory.GetParent(zzipFileLocation).FullName + "\\" + Path.GetFileName(zzipFileLocation) + "_downloaded";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Do you want to install this directly to your minecraft mod folder in appdata?\nPress Y for yes, anything else for no");
        if (Console.ReadKey().KeyChar == 'y')
        {
            downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft\\mods\\";
            Console.WriteLine("New destination set to " + downloadFolder);
        }
        Directory.CreateDirectory(downloadFolder);
        foreach (var file in metaData.files)
        {
            Console.CursorTop = 5;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Downloaded mod (" + metaData.files.IndexOf(file) + "/" + metaData.files.Count + ")");
            var task = DownloadCurseForgemod( file.projectID, file.fileID, downloadFolder);
            task.Wait();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Finished downloading!");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Press any key to close!");
        Console.ReadLine();
    }
    public static string fileNamePattern = @"/([^/]+)\.jar";
    public class MyWebClient : WebClient
    {
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            (request as HttpWebRequest).AllowAutoRedirect = true;
            WebResponse response = base.GetWebResponse(request);
            return response;
        }
    }
    public static async Task DownloadCurseForgemod( int projectId, int fileId, string downloadLocation)
    {
        string url = $"https://www.curseforge.com/api/v1/mods/{projectId}/files/{fileId}/download";

        try
        {
            string fileName = getFileName(url);
            string outLocation = downloadLocation + "\\" + fileName;
            if (File.Exists(outLocation))
            {
                return;
            }
            WebClient wc = new WebClient();
            var data = wc.DownloadData(url);

            File.WriteAllBytes(outLocation, data);

        }
        catch (Exception e) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ToString());    
        }
    }
  
    public static string getFileName(string url)
    {
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
       // string header_contentDisposition = resp.Headers["content-disposition"];
        //Console.WriteLine(resp.ResponseUri);
        return new Regex(fileNamePattern).Match(resp.ResponseUri.AbsoluteUri).Value.Replace("%2B", "+");
    }
}