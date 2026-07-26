using Backend55.Api.Controllers.LearnKit.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend55.Api.Controllers.LearnKit.Admin;

internal static class ManagementErrors
{
    public static NotFoundObjectResult NotFound(
        string code,
        string message) =>
        new(new ManagementErrorResponse(code, message));

    public static BadRequestObjectResult BadRequest(
        string code,
        string message) =>
        new(new ManagementErrorResponse(code, message));

    public static ConflictObjectResult Conflict(
        string code,
        string message) =>
        new(new ManagementErrorResponse(code, message));
}
