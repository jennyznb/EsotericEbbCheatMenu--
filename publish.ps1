[CmdletBinding()]
param(
    [string]$RepoUrl,
    [string]$GitHubUser,
    [string]$RepoName = "EsotericEbbCheatMenu",
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"
Set-Location -LiteralPath $PSScriptRoot

$existing = git remote get-url origin 2>$null
if ($LASTEXITCODE -ne 0) {
    $existing = $null
}

if (-not $RepoUrl) {
    if ($existing) {
        $RepoUrl = $existing
    } else {
        if (-not $GitHubUser) {
            $GitHubUser = Read-Host "GitHub username"
        }
        $RepoUrl = "https://github.com/$GitHubUser/$RepoName.git"
    }
}

Write-Host "Repository URL: $RepoUrl"

$createUrl = "https://github.com/new?name=$RepoName&description=BepInEx+cheat+menu+for+Esoteric+Ebb"
Write-Host ""
Write-Host "If the repository does not exist yet, create it as an EMPTY repository here:"
Write-Host "  $createUrl"
Write-Host "Do not add a README, .gitignore, or license on GitHub; this repo already has them."
try {
    Start-Process $createUrl
} catch {
}
Read-Host "After creating the empty repository, press Enter to continue"

if ($existing) {
    git remote set-url origin $RepoUrl
} else {
    git remote add origin $RepoUrl
}

git push -u origin $Branch
if ($LASTEXITCODE -ne 0) {
    throw "git push failed with exit code $LASTEXITCODE"
}

git push origin --tags
if ($LASTEXITCODE -ne 0) {
    throw "pushing tags failed with exit code $LASTEXITCODE"
}

Write-Host "Published to $RepoUrl"
