# IIS Site and Application Pool Setup

This guide describes how to configure an IIS website and application pool for the BSEtunes API on a Windows Server,
including the required **Network Service** identity and file system permissions so that Serilog can write log files
to the `logs` directory.

---

## Prerequisites

- Windows Server with **IIS** and the **ASP.NET Core Hosting Bundle for .NET 9** installed
- An administrative account on the server
- The BSEtunes API published to a local folder (e.g. `C:\inetpub\bsetunes\`)

---

## Step 1 – Create the Application Pool

1. Open **Internet Information Services (IIS) Manager** (`inetmgr`).
2. In the **Connections** pane, expand the server node and click **Application Pools**.
3. In the **Actions** pane on the right, click **Add Application Pool…**
4. Fill in the dialog:

   | Field | Value |
   |---|---|
   | Name | `BSEtunesAppPool` |
   | .NET CLR version | **No Managed Code** *(ASP.NET Core runs out-of-process or in-process; no CLR version is needed)* |
   | Managed pipeline mode | **Integrated** |

5. Click **OK**.

### 1.1 Set the Identity to Network Service

1. In the **Application Pools** list, select `BSEtunesAppPool`.
2. In the **Actions** pane, click **Advanced Settings…**
3. Under the **Process Model** group, click the **Identity** row and then click the **…** button.
4. Select **Built-in account** and choose **NetworkService** from the dropdown.
5. Click **OK**, then **OK** again to close Advanced Settings.

> **Why Network Service?**  
> `NetworkService` is a least-privilege built-in account that can access the local file system
> and make authenticated network calls (e.g. to Azure Key Vault) using the machine account.
> It avoids the need for a dedicated service account while keeping the attack surface small.

---

## Step 2 – Create the IIS Website

1. In **IIS Manager**, right-click **Sites** and choose **Add Website…**
2. Fill in the dialog:

   | Field | Value |
   |---|---|
   | Site name | `BSEtunes` |
   | Application pool | `BSEtunesAppPool` *(select the pool created above)* |
   | Physical path | `C:\inetpub\bsetunes` *(the folder containing the published output)* |
   | Binding – Type | `https` |
   | Binding – IP address | `All Unassigned` |
   | Binding – Port | `443` |
   | Binding – Host name | `api.bsetunes.example.com` *(your actual hostname)* |
   | SSL certificate | select your TLS certificate |

3. Click **OK**.

> For HTTP → HTTPS redirection, add an additional binding on port `80` and configure
> an IIS URL Rewrite rule, or handle the redirect at the reverse-proxy / load-balancer level.

---

## Step 3 – Grant Network Service Write Access to the Deployment Folder

The BSEtunes API uses **Serilog** with a rolling file sink. By default the log files are written
to a `logs` sub-folder inside the application root. The `NetworkService` account must have
**write** (Modify) permission on that path.

### 3.1 Using File Explorer (GUI)

1. Navigate to `C:\inetpub\bsetunes` in File Explorer.
2. Right-click the folder → **Properties → Security → Edit → Add**.
3. In the **Select Users or Groups** dialog, type `NETWORK SERVICE` and click **Check Names**.
   The name resolves to `NT AUTHORITY\NETWORK SERVICE`.
4. Click **OK**.
5. In the permissions list, tick **Modify** (this automatically includes Read & Execute, List, Read, and Write).
6. Click **Apply** and **OK**.

### 3.2 Using PowerShell (recommended for automation)

```powershell
$appRoot = "C:\inetpub\bsetunes"

$acl  = Get-Acl $appRoot
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
	"NT AUTHORITY\NETWORK SERVICE",
	"Modify",
	"ContainerInherit, ObjectInherit",
	"None",
	"Allow"
)
$acl.AddAccessRule($rule)
Set-Acl -Path $appRoot -AclObject $acl

Write-Host "Modify permission granted to NETWORK SERVICE on '$appRoot'."
```

> **Scope the permission if preferred.**  
> If you want to restrict write access strictly to the `logs` sub-directory, create the folder
> first and run the script above against `C:\inetpub\bsetunes\logs` instead.
> The application root itself only needs **Read & Execute** for `NetworkService`.

```powershell
# Minimal permission variant – write only to the logs folder
$logsDir = "C:\inetpub\bsetunes\logs"
New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

$acl  = Get-Acl $logsDir
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
	"NT AUTHORITY\NETWORK SERVICE",
	"Modify",
	"ContainerInherit, ObjectInherit",
	"None",
	"Allow"
)
$acl.AddAccessRule($rule)
Set-Acl -Path $logsDir -AclObject $acl

Write-Host "Modify permission granted to NETWORK SERVICE on '$logsDir'."
```

---

## Step 4 – Verify the Serilog Log Path in `appsettings.json`

Confirm that the Serilog file sink path in `appsettings.Production.json` points to a location
inside the application root (relative paths resolve to the application root when hosted in IIS):

```jsonc
{
  "Serilog": {
	"WriteTo": [
	  {
		"Name": "File",
		"Args": {
		  "path": "logs/bsetunes-.log",
		  "rollingInterval": "Day",
		  "retainedFileCountLimit": 14
		}
	  }
	]
  }
}
```

A relative path such as `logs/bsetunes-.log` expands to `C:\inetpub\bsetunes\logs\bsetunes-.log`
when the application runs under IIS.

---

## Step 5 – Verify the Configuration

### 5.1 Check the Application Pool State

```powershell
Import-Module WebAdministration
Get-WebConfiguration "system.applicationHost/applicationPools/add[@name='BSEtunesAppPool']" |
	Select-Object name, processModel
```

The output should show `userName` as empty and `identityType` as `NetworkService`:

```
name             : BSEtunesAppPool
processModel     : @{identityType=NetworkService; userName=; password=...}
```

### 5.2 Browse to the Health Endpoint

After starting the site, navigate to `https://api.bsetunes.example.com/health` (or your configured
health-check route). A `200 OK` response confirms IIS is forwarding requests to the ASP.NET Core process.

### 5.3 Confirm Log File Creation

```powershell
$logsDir = "C:\inetpub\bsetunes\logs"
Get-ChildItem $logsDir | Sort-Object LastWriteTime -Descending | Select-Object -First 5
```

A log file dated today confirms that `NetworkService` can write to the directory.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| HTTP 500.19 – configuration error | `web.config` is missing or malformed | Re-publish; check the publish output |
| HTTP 502.5 – process failure | ASP.NET Core Hosting Bundle not installed | Install the [.NET 9 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9) and run `iisreset` |
| HTTP 500.30 – start failure | App crashes on startup (e.g. missing secrets) | Check the Windows Event Log → Application for `Application Error` entries |
| No log files created | `NetworkService` lacks write permission on `logs\` | Re-run the PowerShell snippet in Step 3 |
| Key Vault `403 Forbidden` | Machine account not permitted | Follow the certificate setup guide in [`azure-keyvault-certificate-setup.md`](azure-keyvault-certificate-setup.md) |
| App pool stops immediately | Unhandled exception at startup | Enable **stdout logging** temporarily in `web.config` (set `stdoutLogEnabled="true"`) |

### Enable Stdout Logging (temporary diagnostics only)

```xml
<!-- web.config – enable only for troubleshooting, disable afterwards -->
<aspNetCore processPath="dotnet"
			arguments=".\BSEtunes.Api.dll"
			stdoutLogEnabled="true"
			stdoutLogFile=".\logs\stdout"
			hostingModel="inprocess" />
```

Create the `logs` folder before enabling, then check `C:\inetpub\bsetunes\logs\stdout_*.log`
for startup errors. **Disable `stdoutLogEnabled` again once the issue is resolved.**

---

## Quick-Reference Checklist

- [ ] .NET 9 ASP.NET Core Hosting Bundle installed on the server
- [ ] Application pool `BSEtunesAppPool` created with **No Managed Code**
- [ ] Application pool identity set to **Network Service**
- [ ] IIS website created, pointing to the publish output folder
- [ ] HTTPS binding configured with a valid TLS certificate
- [ ] `NT AUTHORITY\NETWORK SERVICE` has **Modify** permission on the `logs` folder
- [ ] `appsettings.Production.json` Serilog path uses a relative `logs/` path
- [ ] Application pool is **Started** and the site is **Started** in IIS Manager
