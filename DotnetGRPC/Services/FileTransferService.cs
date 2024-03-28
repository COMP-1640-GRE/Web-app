using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Grpc.Core;
using Microsoft.VisualBasic;
using DotnetGRPC.GlobalVariables;
using Amazon.S3.Model;
using Google.Protobuf;
using System.Net.Mime;
using System.IO.Compression;

namespace DotnetGRPC.Services
{
    public class FileTransferService : FileTransfer.FileTransferBase
    {
        private readonly ILogger<FileTransferService> _logger;
        private AmazonS3Config _config;
        public FileTransferService(ILogger<FileTransferService> logger)
        {
            _logger = logger;
            _config = new AmazonS3Config
            {
                ServiceURL = $"https://{Blob.Space}.{Blob.Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };
        }

        public override async Task<UploadFilesResponse> UploadFiles(UploadFilesRequest request, ServerCallContext context)
        {
            if (request.Files.Count == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "No file uploaded"));
            }

            if (request.Filenames.Count != request.Files.Count)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Number of filenames does not match number of files"));
            }

            List<string> urls = new();

            try
            {
                using var client = new AmazonS3Client(Blob.Key, Blob.Secret, _config);
                for (int i = 0; i < request.Files.Count; i++)
                {
                    ByteString file = request.Files[i];
                    byte[] byteArray = file.ToByteArray();
                    Stream stream = new MemoryStream(byteArray);
                    string extension = Path.GetExtension(request.Filenames[i]).ToLower();
                    string contentType;
                    switch (extension)
                    {
                        case ".txt":
                            contentType = MediaTypeNames.Text.Plain;
                            break;
                        case ".jpg":
                        case ".jpeg":
                            contentType = MediaTypeNames.Image.Jpeg;
                            break;
                        case ".png":
                            contentType = MediaTypeNames.Image.Png;
                            break;
                        case ".gif":
                            contentType = MediaTypeNames.Image.Gif;
                            break;
                        // Add more cases as needed
                        default:
                            contentType = "application/octet-stream"; // Default to binary data
                            break;
                    }
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = Blob.Space,
                        Key = request.Filepath + "/" + request.Filenames[i],
                        InputStream = stream,
                        ContentType = contentType,
                        CannedACL = S3CannedACL.PublicRead,
                        StorageClass = S3StorageClass.FindValue(Blob.StorageType)
                    };

                    await client.PutObjectAsync(putRequest);
                    string fileNameTemp = request.Filenames[i].Replace(" ", "%20");
                    urls.Add($"{Blob.URL}/{Blob.Space}/{request.Filepath}/{fileNameTemp}");
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }
            return new UploadFilesResponse
            {
                Urls = { urls }
            };
        }

        public override async Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context)
        {
            string url = request.Url;
            string fileKey;
            try
            {
                using var client = new AmazonS3Client(Blob.Key, Blob.Secret, _config);

                var uri = new Uri(url);
                string[] parts = uri.AbsoluteUri.Split(new[] { Blob.Space }, StringSplitOptions.None);
                fileKey = parts.Length > 2 ? parts[2].TrimStart('/') : "";
                fileKey = fileKey.Replace("%20", " ");

                if (!await Functions.FileFuncs.VerifyFile(client, fileKey))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
                }
                await client.DeleteObjectAsync(Blob.Space, fileKey);

            }
            catch (AmazonS3Exception e)
            {
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }

            return new DeleteFileResponse
            {
                Message = $"Files {fileKey} deleted successfully"
            };
        }

        public override async Task<DownloadMultipleFileAsZipResponse> DownloadMultipleFileAsZip (DownloadMultipleFileAsZipRequest request, ServerCallContext context)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            List<string> fileKeys = request.Urls.Select(url =>
            {
                var uri = new Uri(url);
                string[] parts = uri.AbsoluteUri.Split(new[] { Blob.Space }, StringSplitOptions.None);
                return parts.Length > 2 ? parts[2].TrimStart('/') : "";
            }).ToList();

            try
            {
                using var client = new AmazonS3Client(Blob.Key, Blob.Secret, _config);
                if (!await Functions.FileFuncs.VerifyMultipleFiles(client, fileKeys))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
                }

                foreach (var url in request.Urls)
                {
                    var uri = new Uri(url);
                    string[] parts = uri.AbsoluteUri.Split(new[] { Blob.Space }, StringSplitOptions.None);
                    string fileKey = parts.Length > 2 ? parts[2].TrimStart('/') : "";
                    await Functions.FileFuncs.DownloadAndSaveFile(new HttpClient(), url, tempFolder);
                }
            }
            catch (AmazonS3Exception e)
            {
                throw new RpcException(new Status(StatusCode.Internal, e.Message));
            }

            string zipFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            ZipFile.CreateFromDirectory(tempFolder, zipFile);
            byte[] byteArray = File.ReadAllBytes(zipFile);
            return new DownloadMultipleFileAsZipResponse
            {
                Name = request.Filename + ".zip",
                Zip = ByteString.CopyFrom(byteArray)
            };
        }
    }

}