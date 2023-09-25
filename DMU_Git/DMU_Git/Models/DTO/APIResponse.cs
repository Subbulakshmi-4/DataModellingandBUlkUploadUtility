using System.Net;

namespace DMU_Git.Models.DTO
{
    public class APIResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> ErrorMessage { get; set; }
        public T Result { get; set; }
    }
}
