using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using DotnetGRPC;
using System.IO;
using Google.Protobuf;
using Grpc.Core;

Console.WriteLine("Hello World!");

using var channel = GrpcChannel.ForAddress("https://comp1640api.azurewebsites.net");
var client = new FileTransfer.FileTransferClient(channel);

var request = new DownloadMultipleFileAsZipRequest
{
    Filename = "file",
    Urls =
    {
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/attachments/13/Will%20AI%20Lead%20To%20The%20End%20Of%20Marketing%20Jobs?.docx",
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/attachments/13/image1.webp",
    }
};

using AsyncServerStreamingCall<DownloadMultipleFileAsZipResponse> call = client.DownloadMultipleFileAsZip(request);

// if file exists, delete it
if (File.Exists(request.Filename + ".zip"))
{
    File.Delete(request.Filename + ".zip");
}

long totalBytesRead = 0;
long totalBytes = 0;
await foreach (var response in call.ResponseStream.ReadAllAsync())
{
    totalBytesRead += response.Zip.Length;
    totalBytes = response.Size;
    double percent = (double)totalBytesRead / totalBytes * 100;
    Console.WriteLine($"Downloaded {percent}%");

    // Append the chunk to a file
    using var fileStream = File.Open(request.Filename + ".zip", FileMode.Append);
    response.Zip.WriteTo(fileStream);
}

Console.WriteLine($"File downloaded as {request.Filename}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();