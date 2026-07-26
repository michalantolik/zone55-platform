using Microsoft.JSInterop;

namespace Backend55.Management.Localization;

public sealed class ManagementLocalizer(IJSRuntime javaScript)
{
    private const string StorageKey = "zone55.language";

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Translations =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["pl"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Nav.Articles"] = "Artykuły",
                ["Nav.Structure"] = "Struktura",
                ["Nav.SignOut"] = "Wyloguj",
                ["Theme.Label"] = "Motyw",
                ["Article.Back"] = "Artykuły",
                ["Article.Eyebrow"] = "Artykuł LearnKit",
                ["Article.Workspace"] = "Przestrzeń redakcyjna",
                ["Article.WorkspaceHint"] = "Edytuj treść i obserwuj żywy podgląd Portalu.",
                ["Article.Editor"] = "Edytor",
                ["Article.Split"] = "Podział",
                ["Article.Preview"] = "Podgląd",
                ["Article.SaveDraft"] = "Zapisz szkic",
                ["Article.Saving"] = "Zapisywanie…",
                ["Article.Publish"] = "Opublikuj",
                ["Article.Unpublish"] = "Cofnij publikację",
                ["Article.More"] = "Więcej",
                ["Article.Delete"] = "Usuń artykuł",
                ["Article.Saved"] = "Zapisano",
                ["Article.Unsaved"] = "Niezapisane zmiany",
                ["Article.Details"] = "Dane artykułu",
                ["Article.Content"] = "Treść artykułu",
                ["Article.Title"] = "Tytuł",
                ["Article.Slug"] = "Slug",
                ["Article.Step"] = "Krok nauki",
                ["Article.Order"] = "Kolejność",
                ["Article.Summary"] = "Podsumowanie",
                ["Article.Blocks"] = "Bloki treści",
                ["Article.AddBlock"] = "Dodaj blok",
                ["Article.ChooseBlock"] = "Wybierz typ bloku",
                ["Article.Cancel"] = "Anuluj",
                ["Article.Add"] = "Dodaj",
                ["Article.Edit"] = "Edytuj",
                ["Article.Duplicate"] = "Duplikuj",
                ["Article.DeleteBlock"] = "Usuń",
                ["Article.SaveBlock"] = "Zapisz blok",
                ["Article.NoBlocks"] = "Ten artykuł nie ma jeszcze bloków treści.",
                ["Preview.Live"] = "Podgląd aktywny",
                ["Preview.Desktop"] = "Desktop",
                ["Preview.Tablet"] = "Tablet",
                ["Preview.Phone"] = "Telefon",
                ["Preview.Expand"] = "Pełny ekran",
                ["Preview.Collapse"] = "Zamknij pełny ekran",
                ["Language.Label"] = "Język"
            },
            ["en"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Nav.Articles"] = "Articles", ["Nav.Structure"] = "Structure", ["Nav.SignOut"] = "Sign out",
                ["Theme.Label"] = "Theme", ["Article.Back"] = "Articles", ["Article.Eyebrow"] = "LearnKit article",
                ["Article.Workspace"] = "Editorial workspace", ["Article.WorkspaceHint"] = "Edit content and watch the live Portal preview.",
                ["Article.Editor"] = "Editor", ["Article.Split"] = "Split", ["Article.Preview"] = "Preview",
                ["Article.SaveDraft"] = "Save draft", ["Article.Saving"] = "Saving…", ["Article.Publish"] = "Publish",
                ["Article.Unpublish"] = "Unpublish", ["Article.More"] = "More", ["Article.Delete"] = "Delete article",
                ["Article.Saved"] = "Saved", ["Article.Unsaved"] = "Unsaved changes", ["Article.Details"] = "Article details",
                ["Article.Content"] = "Article content", ["Article.Title"] = "Title", ["Article.Slug"] = "Slug",
                ["Article.Step"] = "Learning step", ["Article.Order"] = "Sort order", ["Article.Summary"] = "Summary",
                ["Article.Blocks"] = "Content blocks", ["Article.AddBlock"] = "Add block", ["Article.ChooseBlock"] = "Choose a block type",
                ["Article.Cancel"] = "Cancel", ["Article.Add"] = "Add", ["Article.Edit"] = "Edit", ["Article.Duplicate"] = "Duplicate",
                ["Article.DeleteBlock"] = "Delete", ["Article.SaveBlock"] = "Save block", ["Article.NoBlocks"] = "This article has no content blocks yet.",
                ["Preview.Live"] = "Preview active", ["Preview.Desktop"] = "Desktop", ["Preview.Tablet"] = "Tablet",
                ["Preview.Phone"] = "Phone", ["Preview.Expand"] = "Full screen", ["Preview.Collapse"] = "Exit full screen",
                ["Language.Label"] = "Language"
            },
            ["de"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Nav.Articles"] = "Artikel", ["Nav.Structure"] = "Struktur", ["Nav.SignOut"] = "Abmelden",
                ["Theme.Label"] = "Design", ["Article.Back"] = "Artikel", ["Article.Eyebrow"] = "LearnKit-Artikel",
                ["Article.Workspace"] = "Redaktionsbereich", ["Article.WorkspaceHint"] = "Inhalte bearbeiten und die Live-Vorschau des Portals verfolgen.",
                ["Article.Editor"] = "Editor", ["Article.Split"] = "Geteilt", ["Article.Preview"] = "Vorschau",
                ["Article.SaveDraft"] = "Entwurf speichern", ["Article.Saving"] = "Speichern…", ["Article.Publish"] = "Veröffentlichen",
                ["Article.Unpublish"] = "Veröffentlichung zurücknehmen", ["Article.More"] = "Mehr", ["Article.Delete"] = "Artikel löschen",
                ["Article.Saved"] = "Gespeichert", ["Article.Unsaved"] = "Ungespeicherte Änderungen", ["Article.Details"] = "Artikeldaten",
                ["Article.Content"] = "Artikelinhalt", ["Article.Title"] = "Titel", ["Article.Slug"] = "Slug",
                ["Article.Step"] = "Lernschritt", ["Article.Order"] = "Reihenfolge", ["Article.Summary"] = "Zusammenfassung",
                ["Article.Blocks"] = "Inhaltsblöcke", ["Article.AddBlock"] = "Block hinzufügen", ["Article.ChooseBlock"] = "Blocktyp auswählen",
                ["Article.Cancel"] = "Abbrechen", ["Article.Add"] = "Hinzufügen", ["Article.Edit"] = "Bearbeiten", ["Article.Duplicate"] = "Duplizieren",
                ["Article.DeleteBlock"] = "Löschen", ["Article.SaveBlock"] = "Block speichern", ["Article.NoBlocks"] = "Dieser Artikel enthält noch keine Inhaltsblöcke.",
                ["Preview.Live"] = "Vorschau aktiv", ["Preview.Desktop"] = "Desktop", ["Preview.Tablet"] = "Tablet",
                ["Preview.Phone"] = "Telefon", ["Preview.Expand"] = "Vollbild", ["Preview.Collapse"] = "Vollbild schließen",
                ["Language.Label"] = "Sprache"
            }
        };

    public event Action? Changed;
    public string CurrentLanguage { get; private set; } = "pl";
    public IReadOnlyList<string> Languages { get; } = ["pl", "en", "de"];

    public string this[string key] =>
        Translations.TryGetValue(CurrentLanguage, out var language) && language.TryGetValue(key, out var value)
            ? value
            : Translations["en"].GetValueOrDefault(key, key);

    public async Task InitializeAsync()
    {
        var stored = await javaScript.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        CurrentLanguage = Languages.Contains(stored, StringComparer.OrdinalIgnoreCase) ? stored!.ToLowerInvariant() : "pl";
        Changed?.Invoke();
    }

    public async Task SetLanguageAsync(string language)
    {
        if (!Languages.Contains(language, StringComparer.OrdinalIgnoreCase)) return;
        CurrentLanguage = language.ToLowerInvariant();
        await javaScript.InvokeVoidAsync("localStorage.setItem", StorageKey, CurrentLanguage);
        Changed?.Invoke();
    }
}
