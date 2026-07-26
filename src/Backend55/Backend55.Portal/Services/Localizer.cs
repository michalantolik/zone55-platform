using Microsoft.Extensions.Localization;

namespace Backend55.Portal.Services;

/// <summary>
/// Provides a concise, strongly scoped entry point to the shared application resources.
/// </summary>
public sealed class Localizer(IStringLocalizer<AppResources> localizer)
{
    public string this[string key] => localizer[key].Value;

    public string this[string key, params object[] arguments] =>
        localizer[key, arguments].Value;
}
