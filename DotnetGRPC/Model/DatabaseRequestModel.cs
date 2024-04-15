using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetGRPC.Model
{
    public class DatabaseRequestModel
    {
        public string objectType { get; set; }
        public string recoveryPointId { get; set; }
        public string sourceDataStoreType { get; set; }
        public string sourceResourceId { get; set; }
        public RestoreTargetInfo restoreTargetInfo { get; set; }
    }
    public class RestoreTargetInfo
    {
        public string objectType { get; set; }
        public string recoveryOption { get; set; }
        public TargetDetails targetDetails { get; set; }
        public string restoreLocation { get; set; }
    }

    public class TargetDetails
    {
        public string url { get; set; }
        public string filePrefix { get; set; }
        public string restoreTargetLocationType { get; set; }
    }
}