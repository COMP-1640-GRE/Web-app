using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetGRPC.Model
{
    public class RecoveryPointResponseModel
    {
        public List<ValueModel> value { get; set; }
    }

    public class PropertiesModel
    {
        public string objectType { get; set; }
        public string recoveryPointId { get; set; }
        public DateTime recoveryPointTime { get; set; }
        public string recoveryPointType { get; set; }
        public string friendlyName { get; set; }
        public List<RecoveryPointDataStoresDetailModel> recoveryPointDataStoresDetails { get; set; }
        public string retentionTagName { get; set; }
        public string retentionTagVersion { get; set; }
        public string policyName { get; set; }
        public object policyVersion { get; set; }
        public string expiryTime { get; set; } = "";
        public object recoveryPointState { get; set; }
    }

    public class RecoveryPointDataStoresDetailModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public DateTime creationTime { get; set; }
        public string expiryTime { get; set; } = "";
        public object metaData { get; set; }
        public bool visible { get; set; }
        public string state { get; set; }
        public object rehydrationExpiryTime { get; set; }
        public object rehydrationStatus { get; set; }
    }

    public class ValueModel
    {
        public PropertiesModel properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

}