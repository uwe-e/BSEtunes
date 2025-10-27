# Scaffold-MySqlModels.ps1
param (
    [string]$ConnectionString = "Server=localhost;Database=Platten;User=myuser;Password='mypassword';",
    [string]$Provider = "Pomelo.EntityFrameworkCore.MySql",
    [string]$OutputDir = "Models",
    [string]$ContextDir = "Data",
    [string]$ContextName = "RecordsDbContext",
    [string[]]$Tables = @(),  # Optional: specify tables
    [switch]$Force = $true
)

# Ensure EF CLI is available
if (-not (Get-Command "dotnet-ef" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing EF Core CLI tools..."
    dotnet tool install --global dotnet-ef
}

# Build scaffold command
$tablesArg = if ($Tables.Count -gt 0) { $Tables | ForEach-Object { "--table $_" } } else { "" }
$forceArg = if ($Force) { "--force" } else { "" }

$cmd = @(
    "dotnet ef dbcontext scaffold",
    "`"$ConnectionString`"",
    $Provider,
    "--output-dir $OutputDir",
    "--context-dir $ContextDir",
    "--context $ContextName",
    "--use-database-names",
    $tablesArg,
    $forceArg
) -join " "

Write-Host "Running scaffold command..."
Invoke-Expression $cmd
