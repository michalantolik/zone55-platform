> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# BlogPlatform.Cms

## Install the template

1. Install the latest .NET SDK.
2. Run `dotnet new install Umbraco.Templates` to install the project templates.

## Create the Visual Studio project

1. Go to **File > New > Project/Solution**.
2. Search for `Umbraco` in the *Search for templates* field.
3. Select **Umbraco Project (Umbraco HQ)**.
4. Click **Next**.
5. Enter a **Project name**.
6. Select **.NET 10.0 Long-Term Support (LTS)** from the **Framework** dropdown. The rest of the fields are optional.
7. Click **Create**.

## Configure Umbraco with SQL LocalDB

1. Open `appsettings.Develpopment.json` from your Umbraco project.
2. Replace existing connection strings with these:

```
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=(localdb)\\MSSQLLocalDB;Database=BlogPlatformUmbracoDb;Integrated Security=true;TrustServerCertificate=true;",
    "umbracoDbDSN_ProviderName": "Microsoft.Data.SqlClient"
  },
```

## Set "Umbraco.CMS.Imaging.HMACSecretKey" User Secret for Development Environment

```bash
dotnet user-secrets set "Umbraco:CMS:Imaging:HMACSecretKey" "$(openssl rand -base64 64)"
```

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 64
$rng.GetBytes($bytes)
$secret = [Convert]::ToBase64String($bytes)

dotnet user-secrets init --project .\src\BlogPlatform\BlogPlatform.Cms

dotnet user-secrets set "Umbraco:CMS:Imaging:HMACSecretKey" "$secret" --project .\src\BlogPlatform\BlogPlatform.Cms
```
