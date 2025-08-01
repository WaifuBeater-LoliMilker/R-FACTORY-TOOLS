using R_Factory_Tools.Models;

namespace R_Factory_Tools.DTO
{
    public class DeviceParamDTO : DeviceParameters
    {
        public string CommunicationName { get; set; }
        public DeviceCommunicationParamConfig[]? ConfigValues { get; set; }
    }
}
