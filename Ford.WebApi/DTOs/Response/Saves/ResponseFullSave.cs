
using Ford.WebApi.Dtos.Request;

namespace Ford.WebApi.Dtos.Response
{
    public class ResponseFullSave : ResponseSaveDto
    {
        public ICollection<BoneDto> Bones { get; set; } = [];
    }
}
