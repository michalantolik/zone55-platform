using System.ComponentModel.DataAnnotations;

namespace Zone55.Api.Controllers.LearnKit.Admin.Models;

public sealed record UpdateLearningStructureItemRequest(
    [param: Required, MaxLength(200)] string Title,
    [param: MaxLength(2000)] string? Summary);
