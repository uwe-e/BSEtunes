# Azure Key Vault – Self-Signed Certificate Setup

This guide describes how to create a self-signed certificate, register it with an Azure AD App Registration, configure Azure Key Vault access, and install the certificate on the on-premises server that runs the BSEtunes API.

---

## Overview

The BSEtunes API authenticates against Azure Key Vault using **certificate-based authentication** via an Azure AD App Registration. The flow is:

```
BSEtunes API (on-prem)
  └─ presents certificate (thumbprint in appsettings)
	   └─ Azure AD App Registration (certificate uploaded)
			└─ Key Vault Access Policy / RBAC
				 └─ Key Vault secrets retrieved at runtime
```

---

## Prerequisites

- Windows Server (on-premises) with PowerShell 5.1 or later
- Azure subscription with permissions to:
  - Create / manage App Registrations in Azure Active Directory
  - Create / manage a Key Vault
- The `Az` PowerShell module **or** access to the Azure Portal

---

## Step 1 – Create the Self-Signed Certificate

Run the following commands in an **elevated PowerShell session** on the on-premises server.

```powershell
# --- Adjust these values ---
$certSubject  = "CN=BSEtunes-KeyVault"
$certStore    = "Cert:\LocalMachine\My"
$exportPath   = "C:\Certs\BSEtunes-KeyVault.pfx"
$exportPathCer = "C:\Certs\BSEtunes-KeyVault.cer"
$pfxPassword  = Read-Host -AsSecureString "Enter PFX export password"
# ---------------------------

# Create the certificate (valid 2 years)
$cert = New-SelfSignedCertificate `
	-Subject         $certSubject `
	-CertStoreLocation $certStore `
	-KeyExportPolicy Exportable `
	-KeySpec         Signature `
	-KeyLength       2048 `
	-HashAlgorithm   SHA256 `
	-NotAfter        (Get-Date).AddYears(2)

Write-Host "Certificate thumbprint: $($cert.Thumbprint)"

# Export the PFX (private key – kept on the server)
New-Item -ItemType Directory -Force -Path (Split-Path $exportPath) | Out-Null
Export-PfxCertificate -Cert "$certStore\$($cert.Thumbprint)" `
	-FilePath    $exportPath `
	-Password    $pfxPassword

# Export the public CER (uploaded to Azure AD)
Export-Certificate -Cert "$certStore\$($cert.Thumbprint)" `
	-FilePath $exportPathCer `
	-Type     CERT
```

> **Keep the PFX file secure.** The `.cer` file (public key only) is what you upload to Azure.

---

## Step 2 – Register an Application in Azure Active Directory

1. Open the [Azure Portal](https://portal.azure.com) and navigate to  
   **Azure Active Directory → App registrations → New registration**.

2. Fill in:
   | Field | Value |
   |---|---|
   | Name | `BSEtunes API` (or any descriptive name) |
   | Supported account types | *Accounts in this organizational directory only* |
   | Redirect URI | *(leave blank)* |

3. Click **Register**. Note the values that appear on the overview page:
   | Value | Maps to `appsettings.json` key |
   |---|---|
   | **Application (client) ID** | `KeyVault:AzureADApplicationId` |
   | **Directory (tenant) ID** | `KeyVault:AzureADDirectoryId` |

---

## Step 3 – Upload the Certificate to the App Registration

1. In the App Registration, go to **Certificates & secrets → Certificates → Upload certificate**.
2. Select the `.cer` file created in Step 1.
3. Click **Add**.

The certificate thumbprint shown in the portal must match the value you will set in `appsettings.json` (`KeyVault:AzureADCertThumbprint`).

---

## Step 4 – Create and Configure Azure Key Vault

### 4.1 Create the Key Vault

In the Azure Portal, navigate to **Key Vaults → Create**.

| Setting | Recommended value |
|---|---|
| Resource group | your existing resource group |
| Key vault name | `kv-bse-tunes-api-prod` |
| Region | same region as your other resources |
| Pricing tier | Standard |

Complete the wizard and click **Create**.

### 4.2 Grant the App Registration Access to the Key Vault

#### Option A – Azure RBAC (recommended)

1. Open the Key Vault → **Access control (IAM) → Add role assignment**.
2. Role: **Key Vault Secrets User**.
3. Members: select the App Registration created in Step 2.
4. Click **Review + assign**.

#### Option B – Vault Access Policies (legacy)

1. Open the Key Vault → **Access policies → Create**.
2. Under **Secret permissions**, tick **Get** and **List**.
3. Under **Principal**, search for and select the App Registration.
4. Click **Create**.

### 4.3 Add the Required Secrets

In the Key Vault, navigate to **Secrets → Generate/Import** and create each secret
using the naming convention `DeployConfig-production-<field>`:

| Secret name | Description |
|---|---|
| `DeployConfig-production-targetServer` | Target IIS server hostname |
| `DeployConfig-production-deploymentPath` | UNC path to the IIS site root |
| `DeployConfig-production-appPoolName` | IIS application pool name |
| `DeployConfig-production-websiteName` | IIS website name |
| `DeployConfig-production-deploymentUsername` | Deployment account (`DOMAIN\user`) |
| `DeployConfig-production-deploymentPassword` | Deployment account password |
| `DeployConfig-production-backupPath` | UNC path for pre-deployment backup |

---

## Step 5 – Install the PFX Certificate on the On-Premises Server

Run the following in an **elevated PowerShell session** on the server that hosts the BSEtunes API.

```powershell
$exportPath  = "C:\Certs\BSEtunes-KeyVault.pfx"
$pfxPassword = Read-Host -AsSecureString "Enter PFX password"

# Import into the Local Machine personal store
Import-PfxCertificate `
	-FilePath          $exportPath `
	-CertStoreLocation "Cert:\LocalMachine\My" `
	-Password          $pfxPassword
```

Verify the import:

```powershell
Get-ChildItem "Cert:\LocalMachine\My" | Where-Object { $_.Subject -like "*BSEtunes*" } |
	Select-Object Subject, Thumbprint, NotAfter
```

or

## Step 5a - Install the PFX Certificate with the Microsoft Management Console

1. Start the Microsoft Management Console by entering mmc.exe at a command prompt.
2. Click File > Add/Remove Snap-in
The Add/Remove Snap-in window displays with the Standalone tab selected.
3. Click Add.
An Add Standalone Snap-in window displays. 
4. Select Certificates from the list of available standalone snap-ins, then click Add.
A Certificate snap-in window displays.
5. Select Computer account to be able to manage all computer account certificates.
6. Click Next.
7. Accept the default Local Computer and click Finish.
8. Click Close then OK to close the Add/Remove Snap-in window.
The Console Root window is again visable.
9. Under Certificates (Local computer), right-click Personal Certificates
10. Click All Tasks > Import.
The Certificate Import Wizard window displays.
11. Click Next.
12. Browse to, and select the pfx certificate (file) to install.
13. Click Next.
A Certificate Store window displays.
14. Select Place all certificates in the following store.
15. Confirm or if necessary, select by clicking the Browse button, the Certificate store: Trusted Root Certificate Authorities
16. Click Next.
17. Click Yes to install the certificate. 

![mmc.exe installed certificate](/docs/images/mmc-installed-cert.png)


---

## Step 6 – Grant the IIS Application Pool Read Access to the Certificate Private Key

The BSEtunes API runs under an IIS application pool identity. That identity must be allowed to
read the private key of the certificate.

```powershell
# Replace with your actual thumbprint and app pool name
$thumbprint   = "<paste thumbprint here>"
$appPoolUser  = "IIS AppPool\BSEtunesAppPool"   # adjust to your pool name

$certPath = "Cert:\LocalMachine\My\$thumbprint"
$cert     = Get-Item $certPath

# Locate the private key file on disk
$rsaKey = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($cert)
$keyName = $rsaKey.Key.UniqueName

$keyFilePath = Join-Path "$env:ProgramData\Microsoft\Crypto\RSA\MachineKeys" $keyName
if (-not (Test-Path $keyFilePath)) {
	# CNG keys live under a different path
	$keyFilePath = Join-Path "$env:ProgramData\Microsoft\Crypto\Keys" $keyName
}

Write-Host "Private key file: $keyFilePath"

# Grant read permission to the app pool identity
$acl  = Get-Acl $keyFilePath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
	$appPoolUser, "Read", "Allow"
)
$acl.AddAccessRule($rule)
Set-Acl -Path $keyFilePath -AclObject $acl

Write-Host "Read access granted to '$appPoolUser'."
```

> If the API runs as a **Windows service account** instead of an app pool identity,
> replace `$appPoolUser` with the service account name (e.g. `DOMAIN\svc-bsetunes`).

---

or

## Step 6a – Grant the IIS Application Pool Read Access to the Certificate Private Key with the Microsoft Management Console

1. select the previous installed certificate
2. choose manage private keys
![manage private key](/docs/images/mmc-manage-private-key.png)
3. grant read access to the Networkservice
![configure read access](/docs/images/mmc-read-access.png)

## Step 7 – Configure `appsettings.Production.json`

Add or update the `KeyVault` section with the values collected in the previous steps:

```jsonc
{
  "KeyVault": {
	"Name": "kv-bse-tunes-api-prod",
	"AzureADCertThumbprint": "<thumbprint from Step 1>",
	"AzureADApplicationId": "<Application (client) ID from Step 2>",
	"AzureADDirectoryId": "<Directory (tenant) ID from Step 2>"
  }
}
```

> In production, these values should be stored as **environment variables** or injected by the
> deployment pipeline rather than committed to source control.

---

## Step 8 – Verify the Connection

Start the BSEtunes API and check the logs. A successful Key Vault connection produces a log line similar to:

```
[Information] Azure Key Vault configuration loaded from 'kv-bse-tunes-api-prod'.
```

If authentication fails, Serilog will log an `AuthenticationFailedException` or
`Azure.RequestFailedException`. Common causes:

| Symptom | Likely cause |
|---|---|
| `AADSTS700027` – certificate not found | The `.cer` was not uploaded to the App Registration (Step 3) |
| `Access denied` / `403 Forbidden` | The App Registration lacks a Key Vault access policy / RBAC role (Step 4.2) |
| `CryptographicException` – key not accessible | The app pool identity cannot read the private key (Step 6) |
| Wrong thumbprint | Mismatch between `appsettings` and the installed certificate |

---

## Certificate Renewal

Self-signed certificates expire. Set a calendar reminder **before** `NotAfter` and repeat
Steps 1 – 6 with a new certificate. You can upload multiple certificates to the App Registration
simultaneously, allowing a zero-downtime rotation:

1. Create and install the new certificate.
2. Upload the new `.cer` to the App Registration.
3. Update `AzureADCertThumbprint` in the configuration.
4. Restart the API and verify.
5. Remove the old certificate from the App Registration and the server's certificate store.
