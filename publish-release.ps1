# Script de publication automatique d'une release GitHub
# Utilisation: .\publish-release.ps1 [-Version "1.0.2.0"] [-Message "Description"]
# Le token GitHub est lu depuis parametres_impression.dat (ligne 5)
# Si -Version n'est pas fourni, la version est lue depuis AssemblyInfo.cs

param(
    [Parameter(Mandatory=$false)]
    [string]$Version,

    [Parameter(Mandatory=$false)]
    [string]$Message
)

$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  Publication de release GitHub" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# --- Etape 1 : Lire le token depuis parametres_impression.dat ---
Write-Host "Etape 1/6 : Lecture du token GitHub..." -ForegroundColor $InfoColor

$datPath = Join-Path $scriptDir "parametres_impression.dat"
if (-not (Test-Path $datPath)) {
    Write-Host "ERREUR : parametres_impression.dat introuvable" -ForegroundColor $ErrorColor
    Write-Host "  Lancez le logiciel et configurez vos parametres d'abord" -ForegroundColor $WarningColor
    exit 1
}

$datLines = Get-Content $datPath
$token = $null

# Le token est sur la ligne 5 (index 4), seulement si elle ne contient pas de "|"
# (les lignes avec "|" sont des bobines)
if ($datLines.Count -ge 5 -and $datLines[4] -notmatch '\|') {
    $token = $datLines[4].Trim()
}

if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host "ERREUR : Aucun token GitHub configure" -ForegroundColor $ErrorColor
    Write-Host "  Ouvrez le logiciel > Parametres d'impression > Token GitHub" -ForegroundColor $WarningColor
    Write-Host "  Creez un token sur : https://github.com/settings/tokens" -ForegroundColor $WarningColor
    exit 1
}

Write-Host "OK Token GitHub trouve" -ForegroundColor $SuccessColor

# --- Etape 2 : Determiner la version ---
Write-Host "Etape 2/6 : Determination de la version..." -ForegroundColor $InfoColor

$assemblyInfoPath = Join-Path $scriptDir "Properties\AssemblyInfo.cs"
if (-not (Test-Path $assemblyInfoPath)) {
    Write-Host "ERREUR : Properties\AssemblyInfo.cs introuvable" -ForegroundColor $ErrorColor
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    # Lire la version depuis AssemblyInfo.cs
    $assemblyContent = Get-Content $assemblyInfoPath -Raw
    if ($assemblyContent -match 'AssemblyVersion\("(\d+\.\d+\.\d+\.\d+)"\)') {
        $Version = $matches[1]
        Write-Host "  Version lue depuis AssemblyInfo.cs : $Version" -ForegroundColor $InfoColor
    } else {
        Write-Host "ERREUR : Impossible de lire la version depuis AssemblyInfo.cs" -ForegroundColor $ErrorColor
        exit 1
    }
} else {
    # Verifier le format
    if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
        Write-Host "ERREUR : Format de version invalide: $Version (attendu: X.X.X.X)" -ForegroundColor $ErrorColor
        exit 1
    }

    # Mettre a jour AssemblyInfo.cs
    $content = Get-Content $assemblyInfoPath
    $content = $content -replace '\[assembly: AssemblyVersion\(".*"\)\]', "[assembly: AssemblyVersion(`"$Version`")]"
    $content = $content -replace '\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$Version`")]"
    Set-Content $assemblyInfoPath $content
    Write-Host "  AssemblyInfo.cs mis a jour avec la version $Version" -ForegroundColor $InfoColor
}

if ([string]::IsNullOrWhiteSpace($Message)) {
    $Message = "Version $Version"
}

$tag = "v$Version"
Write-Host "OK Version : $Version (tag: $tag)" -ForegroundColor $SuccessColor

# --- Etape 3 : Build en Release ---
Write-Host "Etape 3/6 : Compilation en mode Release..." -ForegroundColor $InfoColor

$msbuildPath = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msbuildPath)) {
    # Chercher MSBuild dans d'autres emplacements
    $possiblePaths = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($p in $possiblePaths) {
        if (Test-Path $p) {
            $msbuildPath = $p
            break
        }
    }
}

if (-not (Test-Path $msbuildPath)) {
    Write-Host "ERREUR : MSBuild introuvable" -ForegroundColor $ErrorColor
    Write-Host "  Installez Visual Studio ou les Build Tools" -ForegroundColor $WarningColor
    exit 1
}

$csprojPath = Join-Path $scriptDir "logiciel d'impression 3d.csproj"
& $msbuildPath $csprojPath /p:Configuration=Release /v:minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR : La compilation a echoue" -ForegroundColor $ErrorColor
    exit 1
}

$exePath = Join-Path $scriptDir "bin\Release\logiciel d'impression 3d.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "ERREUR : Exe introuvable apres compilation" -ForegroundColor $ErrorColor
    exit 1
}

$exeSize = [math]::Round((Get-Item $exePath).Length / 1KB)
Write-Host "OK Compilation reussie ($exeSize Ko)" -ForegroundColor $SuccessColor

# --- Etape 4 : Commit et push ---
Write-Host "Etape 4/6 : Commit et push..." -ForegroundColor $InfoColor

$gitStatus = git status --porcelain
if ($gitStatus) {
    git add .
    git commit -m "Release $tag - $Message"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Aucun changement a committer" -ForegroundColor $WarningColor
    }
}

git push origin master
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR : Push echoue" -ForegroundColor $ErrorColor
    Write-Host "  Verifiez votre connexion et vos identifiants" -ForegroundColor $WarningColor
    exit 1
}

Write-Host "OK Code pousse sur GitHub" -ForegroundColor $SuccessColor

# --- Etape 5 : Creer la release GitHub (draft) ---
Write-Host "Etape 5/6 : Creation de la release GitHub..." -ForegroundColor $InfoColor

$owner = "demouflette"
$repo = "logiciel-impression-3d"

$headers = @{
    "Authorization" = "token $token"
    "Accept" = "application/vnd.github+json"
    "User-Agent" = "logiciel-impression-3d"
}

# Verifier si le tag/release existe deja
try {
    $existingRelease = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/tags/$tag" -Headers $headers -Method Get -ErrorAction Stop
    Write-Host "ATTENTION : La release $tag existe deja (id: $($existingRelease.id))" -ForegroundColor $WarningColor
    $response = Read-Host "Voulez-vous la supprimer et la recreer? (O/N)"
    if ($response -eq 'O' -or $response -eq 'o') {
        Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/$($existingRelease.id)" -Headers $headers -Method Delete
        # Supprimer aussi le tag distant
        try { git push origin ":refs/tags/$tag" 2>$null } catch {}
        Write-Host "  Ancienne release supprimee" -ForegroundColor $InfoColor
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Publication annulee" -ForegroundColor $ErrorColor
        exit 1
    }
} catch {
    # La release n'existe pas, c'est normal
}

# Creer la release en mode draft
$releaseBody = @{
    tag_name = $tag
    target_commitish = "master"
    name = "Version $Version"
    body = $Message
    draft = $true
    prerelease = $false
} | ConvertTo-Json

try {
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases" -Headers $headers -Method Post -Body $releaseBody -ContentType "application/json"
} catch {
    Write-Host "ERREUR : Impossible de creer la release" -ForegroundColor $ErrorColor
    Write-Host "  $($_.Exception.Message)" -ForegroundColor $ErrorColor
    exit 1
}

$releaseId = $release.id
$uploadUrl = $release.upload_url -replace '\{\?name,label\}', ''
Write-Host "  Release draft creee (id: $releaseId)" -ForegroundColor $InfoColor

# --- Etape 6 : Upload de l'exe et publication ---
Write-Host "Etape 6/6 : Upload de l'exe et publication..." -ForegroundColor $InfoColor

$exeName = "logiciel.d.impression.3d.exe"
$uploadHeaders = @{
    "Authorization" = "token $token"
    "Accept" = "application/vnd.github+json"
    "Content-Type" = "application/octet-stream"
    "User-Agent" = "logiciel-impression-3d"
}

try {
    $asset = Invoke-RestMethod -Uri "$uploadUrl`?name=$exeName" -Headers $uploadHeaders -Method Post -InFile $exePath
    Write-Host "  Exe uploade : $($asset.name) ($($asset.size) octets)" -ForegroundColor $InfoColor
} catch {
    Write-Host "ERREUR : Upload de l'exe echoue" -ForegroundColor $ErrorColor
    Write-Host "  $($_.Exception.Message)" -ForegroundColor $ErrorColor
    # Supprimer la release draft
    Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/$releaseId" -Headers $headers -Method Delete
    exit 1
}

# Publier la release (passer de draft a publie)
$publishBody = @{ draft = $false } | ConvertTo-Json
try {
    $published = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/$releaseId" -Headers $headers -Method Patch -Body $publishBody -ContentType "application/json"
} catch {
    Write-Host "ERREUR : Publication de la release echouee" -ForegroundColor $ErrorColor
    Write-Host "  $($_.Exception.Message)" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "OK Release publiee !" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host "  Release $tag publiee avec succes !" -ForegroundColor $SuccessColor
Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "  URL : $($published.html_url)" -ForegroundColor $InfoColor
Write-Host ""

# Ouvrir dans le navigateur
$openBrowser = Read-Host "Ouvrir la release dans le navigateur? (O/N)"
if ($openBrowser -eq 'O' -or $openBrowser -eq 'o') {
    Start-Process $published.html_url
}

Write-Host "Termine !" -ForegroundColor $SuccessColor
