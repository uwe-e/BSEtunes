# BSEtunes

A self-hosted music streaming backend built with ASP.NET Core 9. It exposes a RESTful API for browsing albums, tracks, playlists, genres, play history, and file streaming, secured with JWT-based authentication and refresh-token support backed by ASP.NET Core Identity.

## Repository

[https://github.com/uwe-e/BSEtunes](https://github.com/uwe-e/BSEtunes)

---

## Solution Structure

```
BSEtunes.sln
├── src/
│   ├── BSEtunes.Api            # ASP.NET Core Web API – entry point (.NET 9)
│   ├── BSEtunes.Application    # Application services and AutoMapper profiles
│   ├── BSEtunes.Contracts      # Shared DTOs, enums, interfaces
│   ├── BSEtunes.Domain         # Domain entities
│   ├── BSEtunes.Identity       # ASP.NET Core Identity + JWT bearer auth
│   └── BSEtunes.Infrastructure # EF Core (MySQL via Pomelo), repositories, file access
├── BSEtunes.UnitTests          # xUnit unit tests
└── deployment/                 # Deployment scripts and configuration
```

### Key Technologies

| Area | Technology |
|---|---|
| Runtime | .NET 9 |
| Web Framework | ASP.NET Core 9 Web API |
| ORM | Entity Framework Core 9 – Pomelo MySQL |
| Authentication | ASP.NET Core Identity + JWT Bearer + Refresh Tokens |
| Object Mapping | AutoMapper 16 |
| Logging | Serilog (Console + rolling File sink) |
| API Documentation | Swashbuckle / Swagger UI |
| Secrets (Production) | Azure Key Vault (certificate-based) |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- MySQL or MariaDB server – two databases required (music records + ASP.NET Identity)
- **Production only:** IIS with the [ASP.NET Core Hosting Bundle for .NET 9](https://dotnet.microsoft.com/download/dotnet/9)

---

## Configuration

The API is configured through `appsettings.json` and environment-specific overrides
(`appsettings.Development.json`, `appsettings.Production.json`).
Sensitive values should be provided via **User Secrets** in development and **Azure Key Vault** in production.

### Required configuration keys

```jsonc
{
  // Music records database
  "tunes": {
    "backend": {
      "server": "localhost",
      "port": "3306",
      "database": "bsetunes",
      "userid": "<db_user>",
      "password": "<db_password>"
    },
    "fileshare": {
      // Windows only: impersonation credentials for UNC file share access
      "username": "",
      "password": "",
      "domain": ""
    }
  },

  // ASP.NET Core Identity database
  "identity": {
    "backend": {
      "server": "localhost",
      "port": "3306",
      "database": "bsetunes_identity",
      "userid": "<db_user>",
      "password": "<db_password>"
    }
  },

  // JWT token settings
  "Jwt": {
    "Issuer": "BSE",
    "Audience": "BSEtunes",
    "Key": "<minimum-32-char-signing-key>"
  },

  // AutoMapper license (required for v16+)
  "AutoMapper": {
    "LicenseKey": "<license_key>"
  },

  // Azure Key Vault – Production only
  "KeyVault": {
    "Name": "kv-bse-tunes-api-prod",
    "AzureADCertThumbprint": "<thumbprint>",
    "AzureADApplicationId": "<app_id>",
    "AzureADDirectoryId": "<tenant_id>"
  }
}
```

---

## Running Locally

```powershell
# 1. Restore and build
dotnet build BSEtunes.sln

# 2. Set User Secrets for the API project
dotnet user-secrets set "tunes:backend:server"   "localhost"                        --project src/BSEtunes.Api
dotnet user-secrets set "tunes:backend:database" "bsetunes"                         --project src/BSEtunes.Api
dotnet user-secrets set "tunes:backend:userid"   "root"                             --project src/BSEtunes.Api
dotnet user-secrets set "tunes:backend:password" "secret"                           --project src/BSEtunes.Api
dotnet user-secrets set "Jwt:Key"                "your-secret-key-32-chars-minimum" --project src/BSEtunes.Api
# Repeat the identity:backend:* keys as needed

# 3. Run the API
dotnet run --project src/BSEtunes.Api
# Swagger UI -> https://localhost:<port>/swagger
```

---

## Running Tests

```powershell
dotnet test BSEtunes.UnitTests/BSEtunes.UnitTests.csproj
```

---

## Deploying the API

The project uses a PowerShell-based deployment framework.
Project-specific configuration lives in [`deployment/deploy-config.json`](deployment/deploy-config.json)
and the launcher script is [`deployment/Deploy.ps1`](deployment/Deploy.ps1).

`Deploy.ps1` is a thin wrapper around a shared deployment framework whose scripts must be available at one
of the auto-detected locations (e.g. a `DeploymentScripts` folder next to the solution root), or supplied
explicitly with `-SharedScriptsPath`.

### Prerequisites on the target server

- Windows Server with IIS installed
- Microsoft .NET 9 - Windows Server Hosting
- [Microsoft .NET Runtime - 9 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-9.0.20-windows-x64-installer?cid=getdotnetcore)
- An IIS application pool and website already created for the API
- The deployment user has write permissions on the IIS site folder and the backup folder

---

### 1. Deploy to Production (Azure Key Vault)

Production deployments retrieve all sensitive parameters (server name, paths, credentials)
from **Azure Key Vault**. Authenticate to Azure before running the script.

```powershell
Connect-AzAccount

cd deployment
.\Deploy.ps1 -Environment production -UseKeyVault
```

The Key Vault secrets follow the naming convention `DeployConfig-production-<field>`:

| Secret name | Description |
|---|---|
| `DeployConfig-production-targetServer` | Target IIS server hostname |
| `DeployConfig-production-deploymentPath` | UNC path to the IIS site root |
| `DeployConfig-production-appPoolName` | IIS application pool name |
| `DeployConfig-production-websiteName` | IIS website name |
| `DeployConfig-production-deploymentUsername` | Deployment account (`DOMAIN\user`) |
| `DeployConfig-production-deploymentPassword` | Deployment account password |
| `DeployConfig-production-backupPath` | UNC path for the pre-deployment backup |

---

### 2. Additional deploy flags

| Flag | Description |
|---|---|
| `-SkipBuild` | Skip `dotnet publish`; deploy existing publish output |
| `-SkipBackup` | Skip backup of the current live deployment |
| `-SharedScriptsPath <path>` | Override the path to the shared deployment framework |

---

### 3. IIS `web.config`

The repository includes a `web.config` (in-process hosting) and a `web.Production.config` XDT transform
that is applied automatically during `dotnet publish -c Release`.
Ensure `ASPNETCORE_ENVIRONMENT` is set to `Production` on the IIS server – either in the
`web.config` `<environmentVariables>` section or as an IIS application pool environment variable.

---

### 4. Runtime secrets via Azure Key Vault

At runtime the API reads `appsettings.Production.json` for Key Vault coordinates and authenticates
using a **client certificate** installed in the Local Machine certificate store.
If the certificate is not found, the API logs a warning and continues without Key Vault
(useful when reusing the same config file in development).

---

### 5. Logging

Serilog writes rolling daily log files under the `logs/` folder inside the deployment directory.
`stdoutLogEnabled` in `web.config` is intentionally **false** because the `logs/` directory is
created by the application at runtime, not by the installer.

---

## API Endpoints

| Controller | Route prefix | Description |
|---|---|---|
| `AlbumController` | `/api/albums` | Album listing, details, and cover art |
| `TracksController` | `/api/tracks` | Track listing and audio streaming |
| `PlaylistsController` | `/api/playlists` | Playlist CRUD |
| `GenreController` | `/api/genres` | Genre lookup |
| `SearchController` | `/api/search` | Full-text search |
| `HistoryController` | `/api/history` | Play history |
| `FilesController` | `/api/files` | Direct file access |
| `SystemController` | `/api/system` | Health and system info |

Full interactive documentation is available via Swagger UI at `/swagger` in Development mode.

---

## Additional documents

- [Azure Key Vault – Self-Signed Certificate Setup](/docs/azure-keyvault-certificate-setup.md)
- [IIS Site and Application Pool Setup](/docs/iis-site-and-apppool-setup.md)
