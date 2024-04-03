
using Ford.WebApi.Dtos.Request;

namespace Ford.WebApi.Dtos.Response
{
    public class FullSaveDto : SaveDto
    {
        public ICollection<BoneDto> Bones { get; set; } = [];
    }
}
