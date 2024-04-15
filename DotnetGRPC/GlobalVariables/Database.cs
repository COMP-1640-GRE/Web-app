using Azure.Core;
using Azure.Identity;

namespace DotnetGRPC.GlobalVariables;

public class Database
{
    public static string BackupToken
    {
        get
        {
            var chainedTokenCredential = new ChainedTokenCredential(new DefaultAzureCredential());
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var token = chainedTokenCredential.GetToken(tokenRequestContext, default).Token;
            return token;
        }
    }
}
