# ?? Démarrage Rapide - Système de Mise à Jour GitHub

Ce guide en 5 étapes vous permet de configurer rapidement le système de mise à jour automatique.

## ?? Temps estimé : 15 minutes

---

## Étape 1?? : Installer Git (5 min)

```powershell
# Téléchargez et installez Git
# URL: https://git-scm.com/download/win

# Vérifiez l'installation
git --version
```

? **Résultat attendu** : `git version 2.x.x`

---

## Étape 2?? : Créer le dépôt GitHub (3 min)

1. Allez sur **https://github.com/new**
2. Nom du dépôt : `logiciel-impression-3d`
3. Sélectionnez **Public** ou **Private**
4. **NE COCHEZ RIEN** d'autre
5. Cliquez sur **"Create repository"**

? **Notez votre URL** : `https://github.com/VOTRE_NOM/logiciel-impression-3d`

---

## Étape 3?? : Pousser le code sur GitHub (5 min)

Ouvrez PowerShell dans le dossier du projet :

```powershell
# Remplacez VOTRE_NOM par votre nom d'utilisateur GitHub
$githubUser = "VOTRE_NOM"
$githubRepo = "logiciel-impression-3d"

# Initialisez Git
git init
git config user.name "Votre Nom"
git config user.email "votre.email@example.com"

# Ajoutez tout le code
git add .
git commit -m "Initial commit - v1.0.0.0"

# Configurez le dépôt distant
git remote add origin "https://github.com/$githubUser/$githubRepo.git"
git branch -M main

# Poussez le code
git push -u origin main
```

?? **Authentification** : Utilisez un **Personal Access Token** au lieu d'un mot de passe
- Créez-en un ici : https://github.com/settings/tokens
- Sélectionnez : **"Generate new token (classic)"**
- Cochez : **"repo"** (accès complet)
- Copiez le token et utilisez-le comme mot de passe

? **Vérification** : Visitez `https://github.com/VOTRE_NOM/logiciel-impression-3d` ? Vous devez voir vos fichiers

---

## Étape 4?? : Configurer UpdateManager.cs (1 min)

Ouvrez `UpdateManager.cs` et modifiez ces lignes :

```csharp
private const string GithubOwner = "VOTRE_NOM";  // ?? Changez ici
private const string GithubRepo = "logiciel-impression-3d";
```

**Exemple** :
```csharp
private const string GithubOwner = "demouflette";
private const string GithubRepo = "logiciel-impression-3d";
```

Sauvegardez et committez :

```powershell
git add UpdateManager.cs
git commit -m "Configure GitHub repository for updates"
git push origin main
```

? **Vérification** : Le fichier est mis à jour sur GitHub

---

## Étape 5?? : Créer la première release (1 min)

```powershell
# Créez un tag pour déclencher le build
git tag v1.0.0.0
git push origin v1.0.0.0
```

? **Vérification** :
1. Allez sur **https://github.com/VOTRE_NOM/logiciel-impression-3d/actions**
2. Vous devez voir un workflow **"Build and Release"** en cours d'exécution
3. Attendez 5-10 minutes que le build se termine
4. Allez sur **https://github.com/VOTRE_NOM/logiciel-impression-3d/releases**
5. Vous devez voir **"Version 1.0.0.0"** avec 3 fichiers téléchargeables

---

## ?? C'est terminé !

Votre système de mise à jour est maintenant fonctionnel !

### ?? Pour publier de nouvelles versions

**Méthode 1 : Script automatique (recommandé)**

```powershell
# Utilisez le script publish-release.ps1
.\publish-release.ps1 -Version "1.0.1.0" -Message "Correction de bugs"
```

**Méthode 2 : Manuelle**

```powershell
# 1. Modifiez Properties\AssemblyInfo.cs
[assembly: AssemblyVersion("1.0.1.0")]

# 2. Committez et poussez
git add .
git commit -m "Version 1.0.1.0"
git push origin main

# 3. Créez un tag
git tag v1.0.1.0
git push origin v1.0.1.0
```

---

## ?? Résumé des URLs importantes

Remplacez `VOTRE_NOM` par votre nom d'utilisateur GitHub :

- **Dépôt** : `https://github.com/VOTRE_NOM/logiciel-impression-3d`
- **Actions** : `https://github.com/VOTRE_NOM/logiciel-impression-3d/actions`
- **Releases** : `https://github.com/VOTRE_NOM/logiciel-impression-3d/releases`
- **Créer un token** : `https://github.com/settings/tokens`

---

## ? Problèmes courants

### Le workflow GitHub Actions échoue

? **Solution** : Consultez les logs dans l'onglet "Actions" pour voir l'erreur exacte

### L'application ne détecte pas les mises à jour

? **Solutions** :
1. Vérifiez que `GithubOwner` et `GithubRepo` sont corrects dans `UpdateManager.cs`
2. Testez l'URL : `https://api.github.com/repos/VOTRE_NOM/logiciel-impression-3d/releases/latest`
3. Vérifiez votre connexion Internet

### Git demande un mot de passe à chaque fois

? **Solution** : Configurez le credential helper
```powershell
git config --global credential.helper wincred
```

---

## ?? Documentation complète

Pour plus de détails, consultez : **`GUIDE_MISE_A_JOUR_GITHUB.md`**

---

**Bon développement ! ??**
