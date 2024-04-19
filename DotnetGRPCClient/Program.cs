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
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/attachments/21/Screenshot%202024-04-03%20173308.png",
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/attachments/21/Screenshot%202024-04-11%20212622.png",
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/attachments/21/chap78.docx"
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