# LearnKit editorial workspace

Zone55 Management uses a focused article workspace instead of a generic CMS form.

## Workflow

1. Edit article metadata and blocks.
2. Review the current unsaved draft in the Portal-rendered preview.
3. Save the draft.
4. Publish or unpublish only after article details are saved.

The preview iframe is owned by `PersistentArticlePreviewHost` in the application layout. It remains mounted while the editor switches between Editor, Split, and Preview modes. Viewport and full-screen controls only change the container around the iframe.

## Appearance

Theme definitions, JavaScript integration, and shared CSS variables currently live inside `Zone55.Management`. The structure is intentionally kept local until Portal adopts the same system and there is a proven shared boundary.

Available themes:

- Light
- Dark
- Forest
- Ocean
- Ember

Management references this project now. The Portal can reference the same project later, keeping one theme catalog and one variable contract without coupling the two applications.

## Localization

Management includes a small PL/EN/DE localization service. Polish is the default language. Language and theme choices are stored in browser local storage and can be changed without reloading the application.
