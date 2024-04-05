using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using DotnetGRPC;
using System.IO;
using Google.Protobuf;

Console.WriteLine("Hello World!");

using var channel = GrpcChannel.ForAddress("https://comp1640api.azurewebsites.net");
var client = new FileTransfer.FileTransferClient(channel);

var request = new DownloadMultipleFileAsZipRequest
{
    Filename = "file",
    Urls =
    {
        "https://comp1640-blob.sgp1.digitaloceanspaces.com/comp1640-blob/path/to/files/file1.txt",
        "https://comp1640-blob.sgp1.digitaloceanspaces.com/comp1640-blob/path/to/files/file2.txt",
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/123/comp1640ssh.pem"
    }
};

using var call = client.DownloadMultipleFileAsZip(request);

long totalBytesRead = 0;
long totalBytes = 0;
await foreach (var response in call.ResponseStream.ReadAllAsync())
{
    totalBytesRead += response.Zip.Length;
    totalBytes = response.Size;
    double percent = (double)totalBytesRead / totalBytes * 100;
    Console.WriteLine($"Downloaded {percent}%");

    // Write the chunk to a file
    using (var stream = new FileStream(response.Filename, FileMode.Append))
    {
        var bytes = response.Zip.ToByteArray();
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }
}

Console.WriteLine($"File downloaded as {request.Filename}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();