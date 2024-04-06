using System.IO;

namespace DotnetGRPC.Model.DTO
{
    public class Email
    {
        public string[] To { get; set; }
        public string[] Cc { get; set; }
        public string[] Bcc { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public FileInfo FileAttachment { get; set; }
    }
}