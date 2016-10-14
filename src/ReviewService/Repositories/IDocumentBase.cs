using Newtonsoft.Json;

namespace ReviewService.Repositories
{
    public interface IDocumentBase
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
        string Type { get; }
    }
}

