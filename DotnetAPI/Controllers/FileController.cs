using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Web;
using System.Text;
using DotnetAPI.Functions;
using System.IO.Compression;

namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private const string Space = "comp1640-blob";
        private const string Region = "sgp1";
        private const string StorageType = "STANDARD";
        private string Key = Constants.Keys.SpacesKey;
        private string Secret = Constants.Keys.SpacesSecret;



        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(List<IFormFile> files, string filepath)
        {
            if (files.Count == 0)
            {
                return BadRequest("No file uploaded");
            }
            List<string> urls = new List<string>();

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{Space}.{Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };

            try
            {

                using var client = new AmazonS3Client(Key, Secret, config);
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = Space,
                            Key = filepath + "/" + formFile.FileName,
                            InputStream = formFile.OpenReadStream(),
                            ContentType = formFile.ContentType,
                            CannedACL = S3CannedACL.PublicRead,
                            StorageClass = S3StorageClass.FindValue(StorageType)
                        };

                        await client.PutObjectAsync(putRequest);
                        // replace space in filename with %20
                        string fileNameTemp = formFile.FileName.Replace(" ", "%20");
                        urls.Add($"https://{Space}.{Region}.digitaloceanspaces.com/{Space}/{filepath}/{fileNameTemp}");
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);

            }

            return Ok(urls);

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string fileUrl)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{Space}.{Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };

            try
            {
                using var client = new AmazonS3Client(Key, Secret, config);

                // Extract the file key from the URL
                var uri = new Uri(fileUrl);
                var fileKey = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(Space) + Space.Length + 1);
                // change %20 to space in fileKey
                fileKey = fileKey.Replace("%20", " ");
                Console.WriteLine(fileKey);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = Space,
                    Key = fileKey
                };
                if (!await FileFuncs.VerifyFile(client, fileKey))
                {
                    return BadRequest("File not found");
                }

                await client.DeleteObjectAsync(deleteRequest);

            }
            catch (Amazon.Runtime.Internal.HttpErrorResponseException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return BadRequest($"Amazon error: {ex.Message}");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok("File deleted successfully");
        }



        [HttpGet]
        public async Task<IActionResult> GetFileAsync(string filepath)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{Space}.{Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(Key, Secret, config);
            var request = new ListObjectsV2Request
            {
                BucketName = Space,
                Prefix = filepath
            };
            List<string> urls = new List<string>();
            try
            {
                var response = await client.ListObjectsV2Async(request);

                foreach (var entry in response.S3Objects)
                {
                    urls.Add($"https://{Space}.{Region}.digitaloceanspaces.com/{Space}/{entry.Key}");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(urls);
        }

        [HttpPost("downloadMultiple")]
        public async Task<IActionResult> DownloadFiles([FromBody] List<string> urls, string outputName = "files")
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            using var client = new HttpClient();

            var fileKeys = urls.Select(url =>
            {
                var uri = new Uri(url);
                return uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(Space) + Space.Length + 1);
            }).ToList();

            if (!await FileFuncs.VerifyMultipleFiles(new AmazonS3Client
                (Key,
                Secret,
                new AmazonS3Config
                {
                    ServiceURL = $"https://{Space}.{Region}.digitaloceanspaces.com",
                    ForcePathStyle = true
                }),
                fileKeys)
                )
            {
                return BadRequest("One or more files not found");
            }

            var tasks = urls.Select(url => FileFuncs.DownloadAndSaveFile(client, url, tempFolder)).ToList();
            await Task.WhenAll(tasks);

            var zipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            ZipFile.CreateFromDirectory(tempFolder, zipPath);

            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
            return File(zipBytes, "application/zip", outputName + ".zip");
        }
    }
}