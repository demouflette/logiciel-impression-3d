# Script de configuration initiale du système de mise à jour GitHub
# Ce script guide l'utilisateur à travers toute la configuration

param(
    [Parameter(Mandatory=$false)]
    [string]$GithubUsername = "",
    
    [Parameter(Mandatory=$false)]
    [string]$GithubRepo = "logiciel-impression-3d"
)

# Couleurs
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"
$TitleColor = "Magenta"

function Write-Title {
    param([string]$Text)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor $TitleColor
    Write-Host "  $Text" -ForegroundColor $TitleColor
    Write-Host "========================================" -ForegroundColor $TitleColor
    Write-Host ""
}

function Write-Step {
    param([string]$Number, [string]$Text)
    Write-Host ""
    Write-Host "Étape $Number : $Text" -ForegroundColor $InfoColor
    Write-Host ("?" * 50) -ForegroundColor $InfoColor
}

function Write-Success {
    param([string]$Text)
    Write-Host "? $Text" -ForegroundColor $SuccessColor
}

function Write-Error-Custom {
    param([string]$Text)
    Write-Host "? $Text" -ForegroundColor $ErrorColor
}

function Write-Warning-Custom {
    param([string]$Text)
    Write-Host "? $Text" -ForegroundColor $WarningColor
}

function Write-Info {
    param([string]$Text)
    Write-Host "  $Text" -ForegroundColor $InfoColor
}

Clear-Host
Write-Title "Configuration du Système de Mise à Jour GitHub"

Write-Host "Ce script va vous guider à travers la configuration complète." -ForegroundColor $InfoColor
Write-Host "Temps estimé : 10-15 minutes" -ForegroundColor $InfoColor
Write-Host ""

# ============================================
# Étape 1 : Vérifier Git
# ============================================
Write-Step "1/6" "Vérification de Git"

try {
    $gitVersion = git --version
    Write-Success "Git est installé : $gitVersion"
} catch {
    Write-Error-Custom "Git n'est pas installé"
    Write-Info "Veuillez installer Git depuis : https://git-scm.com/download/win"
    Write-Info "Puis relancez ce script."
    
    $openBrowser = Read-Host "Voulez-vous ouvrir la page de téléchargement? (O/N)"
    if ($openBrowser -eq 'O' -or $openBrowser -eq 'o') {
        Start-Process "https://git-scm.com/download/win"
    }
    exit 1
}

# ============================================
# Étape 2 : Récupérer le nom d'utilisateur GitHub
# ============================================
Write-Step "2/6" "Configuration GitHub"

if ($GithubUsername -eq "") {
    Write-Host "Entrez votre nom d'utilisateur GitHub :" -ForegroundColor $InfoColor
    Write-Host "(Exemple : si votre profil est https://github.com/john-doe, entrez 'john-doe')" -ForegroundColor $WarningColor
    $GithubUsername = Read-Host "Nom d'utilisateur"
    
    if ($GithubUsername -eq "") {
        Write-Error-Custom "Nom d'utilisateur requis"
        exit 1
    }
}

Write-Success "Nom d'utilisateur : $GithubUsername"
Write-Success "Nom du dépôt : $GithubRepo"
Write-Info "URL complète : https://github.com/$GithubUsername/$GithubRepo"

# ============================================
# Étape 3 : Mettre à jour UpdateManager.cs
# ============================================
Write-Step "3/6" "Mise à jour de UpdateManager.cs"

$updateManagerPath = "UpdateManager.cs"
if (Test-Path $updateManagerPath) {
    $content = Get-Content $updateManagerPath -Raw
    
    # Remplacer le nom d'utilisateur GitHub
    $content = $content -replace 'private const string GithubOwner = ".*";', "private const string GithubOwner = `"$GithubUsername`";"
    $content = $content -replace 'private const string GithubRepo = ".*";', "private const string GithubRepo = `"$GithubRepo`";"
    
    Set-Content $updateManagerPath $content
    
    Write-Success "UpdateManager.cs configuré avec :"
    Write-Info "  - GithubOwner = `"$GithubUsername`""
    Write-Info "  - GithubRepo = `"$GithubRepo`""
} else {
    Write-Error-Custom "Fichier UpdateManager.cs non trouvé"
    Write-Info "Assurez-vous d'exécuter ce script depuis la racine du projet"
    exit 1
}

# ============================================
# Étape 4 : Initialiser Git
# ============================================
Write-Step "4/6" "Initialisation de Git"

# Vérifier si Git est déjà initialisé
if (Test-Path ".git") {
    Write-Warning-Custom "Git est déjà initialisé dans ce dossier"
    $reinit = Read-Host "Voulez-vous réinitialiser? (O/N)"
    if ($reinit -ne 'O' -and $reinit -ne 'o') {
        Write-Info "Conservation de la configuration Git existante"
    } else {
        Remove-Item -Recurse -Force ".git"
        git init
        Write-Success "Git réinitialisé"
    }
} else {
    git init
    Write-Success "Git initialisé"
}

# Configurer l'utilisateur Git
Write-Host ""
Write-Host "Configuration de l'identité Git :" -ForegroundColor $InfoColor
$gitName = Read-Host "Votre nom (ex: Jean Dupont)"
$gitEmail = Read-Host "Votre email (ex: jean.dupont@example.com)"

git config user.name "$gitName"
git config user.email "$gitEmail"

Write-Success "Identité Git configurée"

# ============================================
# Étape 5 : Créer le premier commit
# ============================================
Write-Step "5/6" "Préparation du code"

Write-Info "Ajout de tous les fichiers..."
git add .

Write-Info "Création du commit initial..."
git commit -m "Initial commit - Configuration du système de mise à jour GitHub"

if ($LASTEXITCODE -eq 0) {
    Write-Success "Premier commit créé"
} else {
    Write-Warning-Custom "Aucun changement à committer (peut-être déjà fait)"
}

# ============================================
# Étape 6 : Configuration du dépôt distant
# ============================================
Write-Step "6/6" "Configuration du dépôt GitHub"

$remoteUrl = "https://github.com/$GithubUsername/$GithubRepo.git"

# Vérifier si le remote existe déjà
$existingRemote = git remote get-url origin 2>$null
if ($existingRemote) {
    Write-Warning-Custom "Un dépôt distant 'origin' existe déjà : $existingRemote"
    $changeRemote = Read-Host "Voulez-vous le changer pour $remoteUrl ? (O/N)"
    if ($changeRemote -eq 'O' -or $changeRemote -eq 'o') {
        git remote set-url origin $remoteUrl
        Write-Success "Dépôt distant mis à jour"
    }
} else {
    git remote add origin $remoteUrl
    Write-Success "Dépôt distant configuré : $remoteUrl"
}

# Renommer la branche en main
git branch -M main
Write-Success "Branche renommée en 'main'"

# ============================================
# Instructions finales
# ============================================
Write-Title "Configuration terminée !"

Write-Host "? Votre projet est maintenant configuré pour utiliser GitHub !" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "?? Prochaines étapes :" -ForegroundColor $TitleColor
Write-Host ""

Write-Host "1??  Créez le dépôt sur GitHub :" -ForegroundColor $InfoColor
Write-Info "   • Allez sur : https://github.com/new"
Write-Info "   • Repository name : $GithubRepo"
Write-Info "   • Sélectionnez Public ou Private"
Write-Info "   • NE COCHEZ RIEN d'autre"
Write-Info "   • Cliquez sur 'Create repository'"
Write-Host ""

Write-Host "2??  Créez un Personal Access Token (si pas encore fait) :" -ForegroundColor $InfoColor
Write-Info "   • Allez sur : https://github.com/settings/tokens"
Write-Info "   • Cliquez sur 'Generate new token (classic)'"
Write-Info "   • Cochez 'repo' (accès complet)"
Write-Info "   • Copiez le token généré"
Write-Host ""

Write-Host "3??  Poussez le code vers GitHub :" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "   git push -u origin main" -ForegroundColor $SuccessColor
Write-Host ""
Write-Info "   (Utilisez votre token comme mot de passe)"
Write-Host ""

Write-Host "4??  Créez la première release :" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "   git tag v1.0.0.0" -ForegroundColor $SuccessColor
Write-Host "   git push origin v1.0.0.0" -ForegroundColor $SuccessColor
Write-Host ""

Write-Host "5??  Suivez le build sur GitHub Actions :" -ForegroundColor $InfoColor
Write-Info "   https://github.com/$GithubUsername/$GithubRepo/actions"
Write-Host ""

Write-Host "6??  Vérifiez la release créée :" -ForegroundColor $InfoColor
Write-Info "   https://github.com/$GithubUsername/$GithubRepo/releases"
Write-Host ""

Write-Host ("?" * 60) -ForegroundColor $TitleColor
Write-Host ""

$openDocs = Read-Host "Voulez-vous ouvrir la documentation complète? (O/N)"
if ($openDocs -eq 'O' -or $openDocs -eq 'o') {
    if (Test-Path "QUICK_START.md") {
        Start-Process "QUICK_START.md"
    }
}

$openGithub = Read-Host "Voulez-vous ouvrir la page de création de dépôt GitHub? (O/N)"
if ($openGithub -eq 'O' -or $openGithub -eq 'o') {
    Start-Process "https://github.com/new"
}

Write-Host ""
Write-Host "? Configuration terminée avec succès ! Bon développement ! ?" -ForegroundColor $SuccessColor
Write-Host ""
