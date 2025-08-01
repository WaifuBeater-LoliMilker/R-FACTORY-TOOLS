using R_Factory_Tools.Models;

namespace R_Factory_Tools.DTO
{
    public class DeviceCommunicationParamConfigDTO : DeviceCommunicationParamConfig
    {
        public string ParamKey { get; set; }
        public int SortOrder { get; set; }
    }
}
