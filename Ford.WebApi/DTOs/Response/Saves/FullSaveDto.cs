
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Dtos.Request;

namespace Ford.WebApi.Dtos.Response
{
    public class FullSaveDto : SaveDto, IStorageData
    {
        public ICollection<BoneDto> Bones { get; set; } = [];
    }
}
