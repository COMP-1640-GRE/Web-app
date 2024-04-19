using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using DotnetGRPC.Model;
using Grpc.Core;
using ProtoBuf;
using Newtonsoft.Json;
using static DotnetGRPC.RecoveryPoint;
using Microsoft.Azure.Management.PostgreSQL.FlexibleServers.Models;
using Microsoft.Azure.Management.PostgreSQL.FlexibleServers;
using Microsoft.Azure.Management.PostgreSQL.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;

namespace DotnetGRPC.Services
{

    public class DatabaseService : Database.DatabaseBase
    {
        public override async Task<RecoveryPointsResponse> GetDatabaseRecoveryPoints(Empty request, ServerCallContext context)
        {
            var token = DotnetGRPC.GlobalVariables.Database.BackupToken;
            var recoveryPoints = new List<RecoveryPoint>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetStringAsync("https://management.azure.com/subscriptions/5f459f53-780f-4ffc-8604-0e47bbbfb746/resourceGroups/Comp-1640/providers/Microsoft.DataProtection/backupVaults/comp1640backup/backupInstances/comp1640-comp1640-81d34c1e-0ddd-48ed-a8a4-dbe6a1d3e015/recoveryPoints?api-version=2023-01-01");

            Console.WriteLine(response);
            var recoveryPointsData = Newtonsoft.Json.JsonConvert.DeserializeObject<RecoveryPointResponseModel>(response);
            // Console.WriteLine(recoveryPointsData);
            Console.WriteLine(recoveryPointsData);

            foreach (var value in recoveryPointsData.value)
            {
                var recoveryPoint = new RecoveryPoint
                {
                    Properties = new Properties
                    {
                        ObjectType = value.properties.objectType,
                        RecoveryPointId = value.properties.recoveryPointId,
                        RecoveryPointTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value.properties.recoveryPointTime),
                        RecoveryPointType = value.properties.recoveryPointType,
                        FriendlyName = value.properties.friendlyName,
                        RetentionTagName = value.properties.retentionTagName,
                        RetentionTagVersion = value.properties.retentionTagVersion,
                        PolicyName = value.properties.policyName,
                        PolicyVersion = (Google.Protobuf.WellKnownTypes.Any)value.properties.policyVersion,
                        ExpiryTime = value.properties.expiryTime == null ? "" : value.properties.expiryTime,
                        RecoveryPointState = (Google.Protobuf.WellKnownTypes.Any)value.properties.recoveryPointState
                    },
                    Id = value.id,
                    Name = value.name,
                    Type = value.type
                };

                foreach (var detail in value.properties.recoveryPointDataStoresDetails)
                {
                    recoveryPoint.Properties.RecoveryPointDataStoresDetails.Add(new RecoveryPointDataStoresDetails
                    {
                        Id = detail.id,
                        Type = detail.type,
                        CreationTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(detail.creationTime),
                        ExpiryTime = detail.expiryTime == null ? "" : detail.expiryTime,
                        MetaData = (Google.Protobuf.WellKnownTypes.Any)detail.metaData,
                        Visible = detail.visible,
                        State = detail.state,
                        RehydrationExpiryTime = (Google.Protobuf.WellKnownTypes.Any)detail.rehydrationExpiryTime,
                        RehydrationStatus = (Google.Protobuf.WellKnownTypes.Any)detail.rehydrationStatus
                    });
                }

                recoveryPoints.Add(recoveryPoint);
            }

            var recoveryPointsResponse = new RecoveryPointsResponse();
            recoveryPointsResponse.RecoveryPoints.AddRange(recoveryPoints);


            return recoveryPointsResponse;
        }

        public override async Task<RestoreFilesResponse> GetRestoreFiles(Empty request, ServerCallContext context)
        {
            // Call get to this url https://cs1100320024861958a.blob.core.windows.net/comp1640blob?restype=container&comp=list
            // Get the response and parse it to get the RestoreFilesResponse
            // Return the RestoreFilesResponse
            // this call returns xml
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://cs1100320024861958a.blob.core.windows.net/comp1640blob?restype=container&comp=list");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            Console.WriteLine(jsonText);

            var responseData = JsonConvert.DeserializeObject<RestoreFilesModel>(jsonText);

            // Console.WriteLine(responseData.Xml.Version);


            var restoreFilesResponse = new RestoreFilesResponse();
            foreach (var blob in responseData.EnumerationResults.Blobs.Blob)
            {
                var restoreFile = new BlobRes
                {
                    Name = blob.Name,
                    Url = blob.Url,
                    Properties = new BlobProperties
                    {
                        LastModified = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(blob.Properties.LastModified),
                        Etag = blob.Properties.Etag == null ? "" : blob.Properties.Etag,
                        ContentLength = blob.Properties.ContentLength,
                        ContentType = blob.Properties.ContentType,
                        ContentEncoding = blob.Properties.ContentEncoding == null ? "" : blob.Properties.ContentEncoding,
                        ContentLanguage = blob.Properties.ContentLanguage == null ? "" : blob.Properties.ContentLanguage,
                        ContentMd5 = blob.Properties.ContentMd5 == null ? "" : blob.Properties.ContentMd5,
                        CacheControl = blob.Properties.CacheControl == null ? "" : blob.Properties.CacheControl
                    }
                };

                restoreFilesResponse.Blob.Add(restoreFile);
            }



            return restoreFilesResponse;
        }

        public override async Task<Empty> RestoreDatabase(RestoreDatabaseRequest restoreDatabaseRequest, ServerCallContext context)
        {
            // Call the restore endpoint with the restoreDatabaseRequest
            // Return the Empty response
            var token = DotnetGRPC.GlobalVariables.Database.BackupToken;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var databaseRequest = new DatabaseRequestModel
            {
                objectType = "AzureBackupRecoveryPointBasedRestoreRequest",
                recoveryPointId = restoreDatabaseRequest.RecoveryPointId,
                sourceDataStoreType = "VaultStore",
                sourceResourceId = "/subscriptions/5f459f53-780f-4ffc-8604-0e47bbbfb746/resourceGroups/Comp-1640/providers/Microsoft.DBforPostgreSQL/flexibleServers/comp1640",
                restoreTargetInfo = new RestoreTargetInfo
                {
                    objectType = "RestoreFilesTargetInfo",
                    recoveryOption = "FailIfExists",
                    targetDetails = new TargetDetails
                    {
                        url = "https://cs1100320024861958a.blob.core.windows.net/comp1640blob",
                        filePrefix = "restoredblob",
                        restoreTargetLocationType = "AzureBlobs"
                    },
                    restoreLocation = "southeastasia"
                }
            };

            var restoreDatabaseRequestJson = JsonConvert.SerializeObject(databaseRequest);
            Console.WriteLine(restoreDatabaseRequestJson);
            var content = new StringContent(restoreDatabaseRequestJson, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("https://management.azure.com/subscriptions/5f459f53-780f-4ffc-8604-0e47bbbfb746/resourceGroups/Comp-1640/providers/Microsoft.DataProtection/backupVaults/comp1640backup/backupInstances/comp1640-comp1640-81d34c1e-0ddd-48ed-a8a4-dbe6a1d3e015/restore?api-version=2023-01-01", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new RpcException(new Status(StatusCode.Internal, "Error restoring database"));
            }

            return new Empty();
        }

        public override async Task<Empty> RestoreDatabaseToAnother(RestoreDatabaseToAnotherRequest restoreDatabaseToAnotherRequest, ServerCallContext context)
        {
            // Create a PostgreSQL management client
            var credentials = new Microsoft.Rest.TokenCredentials(GlobalVariables.Database.BackupToken);
            var postgresqlManagementClient = new PostgreSQLManagementClient(credentials)
            {
                SubscriptionId = GlobalVariables.Azure.SubscriptionId
            };

            // Create a new server instance
            var server = new Microsoft.Azure.Management.PostgreSQL.FlexibleServers.Models.Server
            {
                Location = "southeastasia",
                CreateMode = "PointInTimeRestore",
                PointInTimeUTC = DateTime.Parse(restoreDatabaseToAnotherRequest.RestoreDate),
                SourceServerResourceId = $"/subscriptions/{GlobalVariables.Azure.SubscriptionId}/resourceGroups/{GlobalVariables.Azure.ResourceGroup}/providers/Microsoft.DBforPostgreSQL/flexibleServers/{restoreDatabaseToAnotherRequest.SourceName}",
                Sku = new Microsoft.Azure.Management.PostgreSQL.FlexibleServers.Models.Sku
                {
                    Name = "Standard_B1ms",
                    Tier = "Burstable"
                },

            };

            // Start the create operation
            var operation = await postgresqlManagementClient.Servers.CreateAsync(GlobalVariables.Azure.ResourceGroup, restoreDatabaseToAnotherRequest.ServerName, server);         

            Console.WriteLine($"Done create the {operation.Id}");

            return new Empty();
        }

        public override async Task<Empty> StopDatabase(StopDatabaseRequest stopDatabaseRequest, ServerCallContext context)
        {
            // Create a PostgreSQL management client
            var credentials = new Microsoft.Rest.TokenCredentials(GlobalVariables.Database.BackupToken);
            var postgresqlManagementClient = new PostgreSQLManagementClient(credentials)
            {
                SubscriptionId = GlobalVariables.Azure.SubscriptionId
            };

            // Stop the server
            await postgresqlManagementClient.Servers.StopAsync(GlobalVariables.Azure.ResourceGroup, stopDatabaseRequest.ServerName);

            return new Empty();
        }

        public override async Task<Empty> StartDatabase(StartDatabaseRequest startDatabaseRequest, ServerCallContext context)
        {
            // Create a PostgreSQL management client
            var credentials = new Microsoft.Rest.TokenCredentials(GlobalVariables.Database.BackupToken);
            var postgresqlManagementClient = new PostgreSQLManagementClient(credentials)
            {
                SubscriptionId = GlobalVariables.Azure.SubscriptionId
            };

            // Stop the server
            await postgresqlManagementClient.Servers.StartAsync("Comp-1640", startDatabaseRequest.ServerName);

            return new Empty();
        }

        public override async Task<DatabaseServersResponse> GetDatabaseServers(Empty request, ServerCallContext context)
        {
            var token = GlobalVariables.Database.BackupToken; // Replace with your JWT token

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"https://management.azure.com/subscriptions/{GlobalVariables.Azure.SubscriptionId}/resourceGroups/{GlobalVariables.Azure.ResourceGroup}/providers/Microsoft.DBforPostgreSQL/flexibleServers?api-version=2023-12-01-preview";
                var response = await client.GetAsync(url);

                var databaseServersResponse = new DatabaseServersResponse();
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Console.WriteLine(content);
                    var resources = JObject.Parse(content)["value"].ToObject<JArray>();
                    Console.WriteLine(resources);

                    foreach (var resource in resources)
                    {
                        var databaseServer = new DatabaseServer
                        {
                            Name = resource["name"].ToString(),
                            CreatedAt = resource["systemData"]["createdAt"].ToString(),
                            State = resource["properties"]["state"].ToString(),
                            BackupRetentionDays = int.Parse(resource["properties"]["backup"]["backupRetentionDays"].ToString()),
                            EarliestRestoreDate = resource["properties"]["backup"]["earliestRestoreDate"].ToString(),
                        };

                        databaseServersResponse.DatabaseServers.Add(databaseServer);
                    }
                }
                else
                {
                    // Handle the error...
                }

                return databaseServersResponse;
            }
        }
    }
}