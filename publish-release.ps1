# Script de publication automatique d'une nouvelle version
# Utilisation: .\publish-release.ps1 -Version "1.0.1.0" -Message "Description des changements"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$Message = "Version $Version"
)

# Couleurs
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host "  Publication de la version $Version" -ForegroundColor $InfoColor
Write-Host "========================================" -ForegroundColor $InfoColor
Write-Host ""

# Vérifier que Git est installé
try {
    $gitVersion = git --version
    Write-Host "? Git détecté: $gitVersion" -ForegroundColor $SuccessColor
} catch {
    Write-Host "? Git n'est pas installé ou n'est pas dans le PATH" -ForegroundColor $ErrorColor
    Write-Host "  Installez Git depuis: https://git-scm.com/download/win" -ForegroundColor $WarningColor
    exit 1
}

# Vérifier le format de version (X.X.X.X)
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    Write-Host "? Format de version invalide: $Version" -ForegroundColor $ErrorColor
    Write-Host "  Format attendu: X.X.X.X (ex: 1.0.0.0)" -ForegroundColor $WarningColor
    exit 1
}

Write-Host "? Format de version valide: $Version" -ForegroundColor $SuccessColor
Write-Host ""

# Vérifier qu'on est dans le bon répertoire
$assemblyInfoPath = "Properties\AssemblyInfo.cs"
if (-not (Test-Path $assemblyInfoPath)) {
    Write-Host "? Fichier AssemblyInfo.cs non trouvé" -ForegroundColor $ErrorColor
    Write-Host "  Assurez-vous d'exécuter ce script depuis la racine du projet" -ForegroundColor $WarningColor
    exit 1
}

# Vérifier s'il y a des changements non commités
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "? Changements non commités détectés:" -ForegroundColor $WarningColor
    Write-Host $gitStatus
    Write-Host ""
    
    $response = Read-Host "Voulez-vous committer tous les changements? (O/N)"
    if ($response -ne 'O' -and $response -ne 'o') {
        Write-Host "? Publication annulée" -ForegroundColor $ErrorColor
        exit 1
    }
}

Write-Host ""
Write-Host "Étape 1/5: Mise à jour du numéro de version..." -ForegroundColor $InfoColor

# Lire le fichier AssemblyInfo.cs
$content = Get-Content $assemblyInfoPath

# Remplacer les versions
$content = $content -replace '\[assembly: AssemblyVersion\(".*"\)\]', "[assembly: AssemblyVersion(`"$Version`")]"
$content = $content -replace '\[assembly: AssemblyFileVersion\(".*"\)\]', "[assembly: AssemblyFileVersion(`"$Version`")]"

# Sauvegarder
Set-Content $assemblyInfoPath $content

Write-Host "? Version mise à jour dans AssemblyInfo.cs" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Étape 2/5: Ajout des fichiers à Git..." -ForegroundColor $InfoColor
git add .
Write-Host "? Fichiers ajoutés" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Étape 3/5: Création du commit..." -ForegroundColor $InfoColor
git commit -m "Release v$Version - $Message"
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Aucun changement à committer (peut-être déjà fait)" -ForegroundColor $WarningColor
}
Write-Host "? Commit créé" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Étape 4/5: Push vers GitHub..." -ForegroundColor $InfoColor
git push origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Échec du push vers GitHub" -ForegroundColor $ErrorColor
    Write-Host "  Vérifiez vos identifiants et votre connexion Internet" -ForegroundColor $WarningColor
    exit 1
}
Write-Host "? Code poussé sur GitHub" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Étape 5/5: Création du tag de release..." -ForegroundColor $InfoColor

# Vérifier si le tag existe déjà
$existingTag = git tag -l "v$Version"
if ($existingTag) {
    Write-Host "? Le tag v$Version existe déjà" -ForegroundColor $WarningColor
    $response = Read-Host "Voulez-vous le supprimer et le recréer? (O/N)"
    if ($response -eq 'O' -or $response -eq 'o') {
        git tag -d "v$Version"
        git push origin ":refs/tags/v$Version"
        Write-Host "? Ancien tag supprimé" -ForegroundColor $SuccessColor
    } else {
        Write-Host "? Publication annulée" -ForegroundColor $ErrorColor
        exit 1
    }
}

# Créer le tag
git tag -a "v$Version" -m "$Message"
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Échec de la création du tag" -ForegroundColor $ErrorColor
    exit 1
}

# Pousser le tag
git push origin "v$Version"
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Échec du push du tag vers GitHub" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "? Tag v$Version créé et poussé" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host "  ? Publication lancée avec succès !" -ForegroundColor $SuccessColor
Write-Host "========================================" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "Prochaines étapes:" -ForegroundColor $InfoColor
Write-Host "1. GitHub Actions va compiler votre projet" -ForegroundColor $InfoColor
Write-Host "2. L'installateur sera créé automatiquement" -ForegroundColor $InfoColor
Write-Host "3. Une release GitHub sera publiée" -ForegroundColor $InfoColor
Write-Host ""

# Obtenir l'URL du dépôt
$remoteUrl = git config --get remote.origin.url
if ($remoteUrl -match 'github.com[:/](.+)/(.+?)(\.git)?$') {
    $owner = $matches[1]
    $repo = $matches[2]
    
    Write-Host "?? Suivez la progression sur:" -ForegroundColor $InfoColor
    Write-Host "   Actions: https://github.com/$owner/$repo/actions" -ForegroundColor $InfoColor
    Write-Host "   Release: https://github.com/$owner/$repo/releases/tag/v$Version" -ForegroundColor $InfoColor
    Write-Host ""
    
    # Ouvrir automatiquement la page Actions
    $openActions = Read-Host "Voulez-vous ouvrir la page GitHub Actions dans votre navigateur? (O/N)"
    if ($openActions -eq 'O' -or $openActions -eq 'o') {
        Start-Process "https://github.com/$owner/$repo/actions"
    }
}

Write-Host "? Terminé !" -ForegroundColor $SuccessColor
