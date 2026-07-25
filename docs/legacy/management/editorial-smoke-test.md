> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# LearnKit editorial smoke test

This test verifies the complete Management-to-Portal workflow without starting Umbraco.

## Prerequisites

Start SQL Server, API, Portal, and Management using the default local configuration. Keep the legacy CMS stopped.

## Scenario

1. Sign in to Zone55 Management.
2. Create a draft article with a unique slug in an existing learning step.
3. Add one block of every supported editor choice:
   - Markdown;
   - Code;
   - PlantUML;
   - Mermaid;
   - Table;
   - Callout;
   - Summary.
4. Reorder at least two blocks.
5. Save the draft and refresh the browser.
6. Confirm that the article details, block content, and ordering persist.
7. Open Split view and verify that unsaved changes appear in the Portal preview.
8. Publish the article.
9. Open the public Portal article and verify that every block renders.
10. Unpublish the article and confirm that the public route no longer exposes it.
11. Edit the draft, save it, and restart the API.
12. Confirm that the saved changes remain after restart.
13. Delete the test article and confirm that it disappears from Management.

## Expected result

- Save, preview, publish, unpublish, and delete are separate actions.
- Preview uses the same Portal renderer as the public article.
- Every supported block renders without an unsupported-block message.
- Content and ordering survive refresh and API restart.
- The workflow completes while the Umbraco CMS is stopped.

Record the date, environment, tester, and any deviations in the pull request or release verification notes.
