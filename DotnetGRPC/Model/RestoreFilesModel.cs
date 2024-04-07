using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;

namespace DotnetGRPC.Model
{
    public class RestoreFilesModel
    {
        public Xml Xml { get; set; }
        public EnumerationResults EnumerationResults { get; set; }
    }

    public class Xml
    {
        [JsonProperty("@version")]
        public string Version { get; set; }

        [JsonProperty("@encoding")]
        public string Encoding { get; set; }
    }

    public class EnumerationResults
    {
        [JsonProperty("@ContainerName")]
        public string ContainerName { get; set; }

        public Blobs Blobs { get; set; }
        public object NextMarker { get; set; }
    }

    public class Blobs
    {
        public List<BlobRes> Blob { get; set; }
    }

    public class BlobRes
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonProperty("Last-Modified")]
        public DateTime LastModified { get; set; }

        public string Etag { get; set; }

        [JsonProperty("Content-Length")]
        public int ContentLength { get; set; }

        [JsonProperty("Content-Type")]
        public string ContentType { get; set; }

        [JsonProperty("Content-Encoding")]
        public string ContentEncoding { get; set; }

        [JsonProperty("Content-Language")]
        public string ContentLanguage { get; set; }

        [JsonProperty("Content-MD5")]
        public string ContentMd5 { get; set; }

        [JsonProperty("Cache-Control")]
        public string CacheControl { get; set; }

        [JsonProperty("BlobType")]
        public string BlobType { get; set; }

        [JsonProperty("LeaseStatus")]
        public string LeaseStatus { get; set; }
    }

    
}