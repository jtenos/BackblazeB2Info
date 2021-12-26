using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BackblazeB2Info;
using BackblazeB2Info.ApplicationConfiguration;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true)
    .Build();
AccountConfiguration accountConfig = new();
config.Bind("AccountConfiguration", accountConfig);

AuthorizationResponse authResponse = Authenticate();
ListFiles(authResponse.ApiUrl, authResponse.AuthorizationToken, accountConfig.Accounts[0].Buckets[0].BucketId);

AuthorizationResponse Authenticate()
{
    string applicationKeyId = accountConfig.Accounts[0].ApplicationKeyId;
    string applicationKey = accountConfig.Accounts[0].ApplicationKey;
    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.backblazeb2.com/b2api/v2/b2_authorize_account");
    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(applicationKeyId + ":" + applicationKey));
    webRequest.Headers.Add("Authorization", "Basic " + credentials);
    webRequest.ContentType = "application/json; charset=utf-8";
    WebResponse response = (HttpWebResponse)webRequest.GetResponse();
    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
    response.Close();
    return JsonSerializer.Deserialize<AuthorizationResponse>(json)!;
}

void ListFiles(string apiUrl, string accountAuthorizationToken, string bucketId)
{
    SortedList<string, BasicCsvRecord> fileList = new();

    FileCollectionResult fileCollection;
    string? startFileName = null;
    do
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v2/b2_list_file_names");
        object requestBody = new
        {
            bucketId = bucketId,
            maxFileCount = 10000,
            startFileName = startFileName
        };
        string body = JsonSerializer.Serialize(requestBody);
        var data = Encoding.UTF8.GetBytes(body);
        webRequest.Method = "POST";
        webRequest.Headers.Add("Authorization", accountAuthorizationToken);
        webRequest.ContentType = "application/json; charset=utf-8";
        webRequest.ContentLength = data.Length;
        using (var stream = webRequest.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
            stream.Close();
        }
        WebResponse response = (HttpWebResponse)webRequest.GetResponse();
        string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        response.Close();
        fileCollection = JsonSerializer.Deserialize<FileCollectionResult>(json)!;
        foreach (var file in fileCollection.Files)
        {
            string key = Regex.Replace(file.FileName, @"\.bzEmpty$", "");
            if (fileList.ContainsKey(key)) { continue; }

            Console.WriteLine(file.FileName);
            fileList.Add(key, new() { Name = Regex.Replace(file.FileName, @"\.bzEmpty$", ""), Size = file.Size });
        }

        startFileName = fileCollection.NextFileName;
        Console.WriteLine($"startFileName={startFileName}");
    } while (startFileName is { Length: > 0 });

    using (StreamWriter basicWriter = new("files-basic.csv"))
    {
        using CsvWriter csvBasic = new(basicWriter, CultureInfo.CurrentCulture);
        csvBasic.WriteRecords(fileList.Select(x => x.Value));
    }
    Console.WriteLine("Done");
}

class BasicCsvRecord
{
    public string Name { get; set; } = default!;
    public long Size { get; set; }
}
class BasicRecordCsvRecordMap : ClassMap<BasicCsvRecord>
{
    public BasicRecordCsvRecordMap()
    {
        Map(x => x.Name);
        Map(x => x.Size);
    }
}
