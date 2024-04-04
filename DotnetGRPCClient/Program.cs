using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using DotnetGRPC;
using System.IO;
using Google.Protobuf;

Console.WriteLine("Hello World!");

using var channel = GrpcChannel.ForAddress("https://comp1640api.azurewebsites.net");
var client = new FileTransfer.FileTransferClient(channel);

var reply = client.DownloadMultipleFileAsZip(
    new DownloadMultipleFileAsZipRequest
    {
        Filename = "file",
        Urls =
        {"https://comp1640-blob.sgp1.digitaloceanspaces.com/comp1640-blob/path/to/files/file1.txt",
        "https://comp1640-blob.sgp1.digitaloceanspaces.com/comp1640-blob/path/to/files/file2.txt",
        "https://comp1640-blob.sgp1.cdn.digitaloceanspaces.com/comp1640-blob/123/comp1640ssh.pem"
        }
    });
// Assuming you have a ByteString object
ByteString byteString = reply.Zip;

// Convert ByteString to byte array
byte[] bytes = byteString.ToByteArray();

// Write byte array to a file
File.WriteAllBytes(reply.Name, bytes);
Console.WriteLine($"File downloaded as {reply.Name}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();