# LearnKit management API errors

LearnKit management endpoints return a stable JSON error body for expected management failures:

```json
{
  "code": "learning_step_not_empty",
  "message": "Move or delete the articles before deleting this step."
}
```

`code` is intended for client-side decisions and diagnostics. `message` is safe to display in the management panel.

The current error codes are:

- `article_not_found`
- `article_order_invalid`
- `learning_path_not_found`
- `learning_zone_not_found`
- `learning_zone_not_empty`
- `learning_zone_order_invalid`
- `learning_step_not_found`
- `learning_step_not_empty`
- `learning_step_order_invalid`

ASP.NET Core validation failures continue to use the standard validation problem response. Unexpected failures are not converted to management errors and remain server errors.
