[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PublishPath,
    [ValidateSet('simulate', 'deploy')]
    [string] $Mode = 'simulate'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$secretNames = @(
    'SMARTERASP_WEBDEPLOY_SITE_NAME',
    'SMARTERASP_WEBDEPLOY_URL',
    'SMARTERASP_WEBDEPLOY_USERNAME',
    'SMARTERASP_WEBDEPLOY_PASSWORD'
)
foreach ($name in $secretNames) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        throw "Missing repository secret: $name"
    }
}

# Limit this workflow to the site explicitly assigned to the API.
if ($env:SMARTERASP_WEBDEPLOY_SITE_NAME -ne 'sa8techno-001-site7') {
    throw 'The Web Deploy site name must be the technical name of site7: sa8techno-001-site7.'
}
$endpoint = $null
if (-not [Uri]::TryCreate($env:SMARTERASP_WEBDEPLOY_URL, [UriKind]::Absolute, [ref] $endpoint) -or
    $endpoint.Scheme -ne 'https' -or $endpoint.UserInfo.Length -ne 0 -or
    $endpoint.DnsSafeHost -notmatch '(?i)\.(site4now\.net|smarterasp\.net)$' -or
    $endpoint.AbsolutePath -ne '/MsDeploy.axd') {
    throw 'Use the full HTTPS Web Deploy service URL supplied by SmarterASP.'
}
Add-Type -AssemblyName System.Web
$query = [System.Web.HttpUtility]::ParseQueryString($endpoint.Query)
if ($query['site'] -ne $env:SMARTERASP_WEBDEPLOY_SITE_NAME) {
    throw 'The Web Deploy URL must specify the same site7 in its site query parameter.'
}

$publishDirectory = (Resolve-Path -LiteralPath $PublishPath).ProviderPath
foreach ($file in @('DunorGames.Api.dll', 'web.config', 'appsettings.Production.json')) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory $file) -PathType Leaf)) {
        throw "Missing API publish file: $file"
    }
}

$library = Join-Path $env:ProgramFiles 'IIS\Microsoft Web Deploy V3\Microsoft.Web.Deployment.dll'
if (-not (Test-Path -LiteralPath $library)) { throw 'Install Web Deploy 3.6 before running this script.' }
Add-Type -Path $library

$sourceOptions = New-Object Microsoft.Web.Deployment.DeploymentBaseOptions
$destinationOptions = New-Object Microsoft.Web.Deployment.DeploymentBaseOptions
$destinationOptions.ComputerName = $endpoint.AbsoluteUri
$destinationOptions.UserName = $env:SMARTERASP_WEBDEPLOY_USERNAME
$destinationOptions.Password = $env:SMARTERASP_WEBDEPLOY_PASSWORD
$destinationOptions.AuthenticationType = 'Basic'
# Keep the default TLS certificate validation. Credentials are never passed in command-line arguments.
$syncOptions = New-Object Microsoft.Web.Deployment.DeploymentSyncOptions
$syncOptions.WhatIf = ($Mode -eq 'simulate')
$syncOptions.DoNotDelete = $true
if ($Mode -eq 'deploy') {
    $offlineRule = [Microsoft.Web.Deployment.DeploymentSyncOptions]::GetAvailableRules() |
        Where-Object Name -eq 'AppOffline'
    if ($null -eq $offlineRule) { throw 'The Web Deploy AppOffline rule is unavailable.' }
    $syncOptions.Rules.Add($offlineRule)
}

$source = $null
try {
    $source = [Microsoft.Web.Deployment.DeploymentManager]::CreateObject(
        'contentPath', $publishDirectory, $sourceOptions)
    $null = $source.SyncTo('contentPath', $env:SMARTERASP_WEBDEPLOY_SITE_NAME,
        $destinationOptions, $syncOptions)
    Write-Output "Web Deploy $Mode completed for API site7. No database migrations were executed."
}
catch {
    # Do not emit raw DeploymentBaseOptions or an exception containing credentials.
    $message = $_.Exception.Message
    foreach ($name in $secretNames) {
        $value = [Environment]::GetEnvironmentVariable($name)
        if (-not [string]::IsNullOrEmpty($value)) { $message = $message.Replace($value, '[redacted]') }
    }
    throw "Web Deploy failed: $message"
}
finally {
    if ($null -ne $source) { $source.Dispose() }
    $destinationOptions.Password = $null
}
