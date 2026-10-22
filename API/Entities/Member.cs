using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    public class Member
    {
        public string Id { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public string ImageUrl { get; set; } = null!;

        public required string DisplayName { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public required string Gender { get; set; }

        public string? Description { get; set; }

        public required string City { get; set; }

        public required string Country  { get; set; }

        // Navigation property (hidden from member JSON response)
        [JsonIgnore]
        public List<Photo> Photos { get; set; } = [];

        [ForeignKey(nameof(Id))]
        [JsonIgnore]
        public AppUser User { get; set; } = null!;
    }
}
