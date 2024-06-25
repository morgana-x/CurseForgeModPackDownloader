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
        Console.WriteLine(jsonText);
        packMetaData metaData = JsonSerializer.Deserialize<packMetaData>(jsonText, options);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Name: " + metaData.name);
        Console.WriteLine($"Minecraft Version: " + metaData.minecraft.version);
        Console.WriteLine($"ModLoader: " + metaData.minecraft.modLoaders[0].id);
        Console.WriteLine($"Number of mods: " + metaData.files.Count);
        Console.ForegroundColor = ConsoleColor.Gray;

        //CookieContainer cookie = cookies();
        string downloadFolder = Directory.GetParent(zzipFileLocation).FullName + "\\" + "minecraftDownload";

        Directory.CreateDirectory(downloadFolder);
        string cookie = "Unique_ID_v2=64df25e6ab7841619bb8e09bea298e24; AWSALB=LZ2InFtPhNhk9eLFza56olau12nMoU2J8TpD29Med04QQXWM4tRt6BXL03msaCYoc4BAPgFGOCSyJz3HTCfMEPgiFGKBs6rPmTB8KI6oxpAjI+vMBfFw7j3xYWRU; AWSALBCORS=LZ2InFtPhNhk9eLFza56olau12nMoU2J8TpD29Med04QQXWM4tRt6BXL03msaCYoc4BAPgFGOCSyJz3HTCfMEPgiFGKBs6rPmTB8KI6oxpAjI+vMBfFw7j3xYWRU; __cf_bm=4IouV.sj7Os_quYvFx1gG1nAxOzumGvPYzQlHdeK6e4-1719326801-1.0.1.1-fUcI_MxyfvuEVEAAYd7laku0G0CTRuBC_UBvJ8jlf0RzUokh2svdPqxDp2tEVSdoPxIApcTNHly.OOR5MFBIXAKI17VSSN6jLuzxHuXSej0";
        foreach (var file in metaData.files)
        {
            Console.WriteLine("File Project Id: " + file.projectID);
            Console.WriteLine("File File Id: " + file.fileID);
            var task = DownloadCurseForgemod(cookie, file.projectID, file.fileID, downloadFolder + "\\");
            task.Wait();
        }
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
    public static async Task DownloadCurseForgemod(string cookies, int projectId, int fileId, string downloadLocation)
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