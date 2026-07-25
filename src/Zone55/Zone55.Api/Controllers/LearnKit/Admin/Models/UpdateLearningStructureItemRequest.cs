using System.ComponentModel.DataAnnotations;

namespace Zone55.Api.Controllers.LearnKit.Admin.Models;

public sealed record UpdateLearningStructureItemRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: MaxLength(2000)] string? Summary);
