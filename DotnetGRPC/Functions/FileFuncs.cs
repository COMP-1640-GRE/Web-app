using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace DotnetGRPC.Functions
{
    public class FileFuncs
    {
        private const string Space = "comp1640-blob";
        public static async Task<bool> VerifyFile(AmazonS3Client client, string fileKey)
        {
            // transform any %20 in the fileKey to spaces
            fileKey = fileKey.Replace("%20", " ");
            var getRequest = new GetObjectRequest
            {
                BucketName = Space,
                Key = fileKey
            };

            try
            {
                var getResponse = await client.GetObjectAsync(getRequest);
            }
            catch (AmazonS3Exception e)
            {
                if (e.ErrorCode == "NoSuchKey")
                {
                    return false;
                }
                throw;
            }
            return true;
        }

        public static async Task<bool> VerifyMultipleFiles(AmazonS3Client client, List<string> fileKeys)
        {
            Console.WriteLine("Verifying files");
            Console.WriteLine("Files: " + string.Join(", ", fileKeys));
            var tasks = fileKeys.Select(file => VerifyFile(client, file)).ToList();
            var results = await Task.WhenAll(tasks);

            if (results.Any(result => result == false))
            {
                return false;
            }
            return true;
        }

        public static async Task DownloadAndSaveFile(HttpClient client, string url, string tempFolder)
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var contentStream = await response.Content.ReadAsStreamAsync();
                var uri = new Uri(url);
                var fileKey = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(Space) + Space.Length + 1);
                var filePath = Path.Combine(tempFolder, fileKey.Substring(fileKey.LastIndexOf("/") + 1));
                var directoryPath = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directoryPath);
                using var fileStream = System.IO.File.Create(filePath);


                await

                contentStream.CopyToAsync(fileStream);
            }
            else
            {
                throw new Exception("Failed to download file");
            }
        }
    }
}