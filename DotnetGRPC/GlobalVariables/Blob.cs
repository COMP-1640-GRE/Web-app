using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;

namespace DotnetGRPC.GlobalVariables
{
    public class Blob
    {
        public static string Key { get; set; }
        public static string Secret { get; set; }
        public const string URL = $"https://{Space}.{Region}.digitaloceanspaces.com";
        public const string Space = "comp1640-blob";
        public const string Region = "sgp1";
        public const string StorageType = "STANDARD";

        public static AmazonS3Config AmazonConfig = new AmazonS3Config
            {
                ServiceURL = $"https://{Space}.{Region}.digitaloceanspaces.com",
                ForcePathStyle = true
            };

        public static AmazonS3Client Client = new AmazonS3Client(Key, Secret, AmazonConfig);
    }
}