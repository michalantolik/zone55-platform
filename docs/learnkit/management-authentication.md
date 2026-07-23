# LearnKit management authentication

The LearnKit management endpoints use bearer authentication.

## Local development

Start these projects:

1. `BlogPlatform.Api`
2. `Zone55.Management`

Open the Management address shown by Visual Studio and sign in with:

- Username: `admin`
- Password: `Zone55Dev!2026`

The development password is represented in API configuration as a SHA-256 hash. The browser stores only the issued access token in session storage. Closing the browser session removes the token.

## Production configuration

Do not use the development signing key or password in a deployed environment. Configure these values through environment variables or the deployment secret store:

- `LearnKitManagementAuth__Username`
- `LearnKitManagementAuth__PasswordSha256`
- `LearnKitManagementAuth__SigningKey`
- `LearnKitManagementAuth__TokenLifetimeMinutes`

`SigningKey` must contain at least 32 characters.

Generate a password hash in PowerShell:

```powershell
$password = "replace-with-a-strong-password"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
[Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
```

## Manual verification

1. Open the public portal and confirm that roadmap and article pages still load without signing in.
2. Open Management in a private browser window. It should redirect to `/login`.
3. Enter an incorrect password. The page should remain on the login form.
4. Sign in with the local development credentials. Articles and Structure should load.
5. Create or update a draft article and confirm that the operation succeeds.
6. Select **Sign out**. Opening Management pages should redirect to `/login` again.
7. In Swagger or another HTTP client, call `GET /api/learnkit/admin/articles` without a bearer token. The API should return `401 Unauthorized`.
8. Call `GET /api/learnkit/roadmaps/dotnet` without a bearer token. The API should return `200 OK` when the seeded path exists.
