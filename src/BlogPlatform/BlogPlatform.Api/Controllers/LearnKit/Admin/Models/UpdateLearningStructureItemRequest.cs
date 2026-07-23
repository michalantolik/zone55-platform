using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

public sealed record UpdateLearningStructureItemRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: MaxLength(2000)] string? Summary);
