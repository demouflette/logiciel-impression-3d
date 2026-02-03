# ?? Guide de mise en place du système de mise à jour GitHub

Ce guide explique comment configurer le système de mise à jour automatique via GitHub pour votre logiciel d'impression 3D.

## ?? Table des matières

1. [Prérequis](#prérequis)
2. [Configuration du dépôt GitHub](#configuration-du-dépôt-github)
3. [Configuration du code](#configuration-du-code)
4. [Création de la première release](#création-de-la-première-release)
5. [Test de la mise à jour](#test-de-la-mise-à-jour)
6. [Publier de nouvelles versions](#publier-de-nouvelles-versions)
7. [Résolution des problèmes](#résolution-des-problèmes)

---

## ? Prérequis

### Installation de Git

1. **Téléchargez Git pour Windows** :
   - Visitez : https://git-scm.com/download/win
   - Téléchargez et installez la version 64-bit

2. **Vérifiez l'installation** :
   ```powershell
   git --version
   ```

### Compte GitHub

1. **Créez un compte GitHub** (si vous n'en avez pas) :
   - Visitez : https://github.com/signup
   - Suivez les instructions

---

## ?? Configuration du dépôt GitHub

### Étape 1 : Créer le dépôt

1. **Connectez-vous à GitHub** : https://github.com

2. **Créez un nouveau dépôt** :
   - Cliquez sur le bouton **"New"** (en haut à droite)
   - **Repository name** : `logiciel-impression-3d`
   - **Description** : `Logiciel de calcul de devis pour impression 3D`
   - Sélectionnez **Public** (pour les releases publiques) ou **Private**
   - ? **NE PAS** cocher "Add a README file"
   - Cliquez sur **"Create repository"**

3. **Notez l'URL de votre dépôt** :
   ```
   https://github.com/VOTRE_NOM_UTILISATEUR/logiciel-impression-3d
   ```

### Étape 2 : Initialiser Git localement

Ouvrez **PowerShell** dans le dossier de votre projet :

```powershell
# Naviguez vers votre projet
cd "D:\projet_3d\logiciel d'impression 3d"

# Initialisez Git
git init

# Configurez votre identité
git config user.name "Votre Nom"
git config user.email "votre.email@example.com"

# Ajoutez tous les fichiers
git add .

# Créez le premier commit
git commit -m "Initial commit - Version 1.0.0.0"

# Ajoutez le dépôt distant (REMPLACEZ VOTRE_NOM_UTILISATEUR)
git remote add origin https://github.com/VOTRE_NOM_UTILISATEUR/logiciel-impression-3d.git

# Renommez la branche en main
git branch -M main

# Poussez le code sur GitHub
git push -u origin main
```

**?? Authentification GitHub** :
- GitHub peut vous demander de vous authentifier
- Utilisez un **Personal Access Token** (PAT) au lieu d'un mot de passe
- Créez un PAT ici : https://github.com/settings/tokens
  - Sélectionnez **"Generate new token (classic)"**
  - Cochez **"repo"** (accès complet aux dépôts)
  - Copiez le token et utilisez-le comme mot de passe

---

## ?? Configuration du code

### Étape 3 : Mettre à jour UpdateManager.cs

Le fichier `UpdateManager.cs` a déjà été configuré, mais vérifiez les valeurs :

```csharp
private const string GithubOwner = "VOTRE_NOM_UTILISATEUR";  // ?? Remplacez ici
private const string GithubRepo = "logiciel-impression-3d";
```

**Exemple** :
- Si votre GitHub est `https://github.com/demouflette/logiciel-impression-3d`
- Alors : `GithubOwner = "demouflette"`

### Étape 4 : Vérifier la version dans AssemblyInfo.cs

Ouvrez `Properties\AssemblyInfo.cs` :

```csharp
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```

La version actuelle doit être **1.0.0.0** pour la première release.

---

## ?? Création de la première release

### Étape 5 : Créer et pousser un tag

Le workflow GitHub Actions se déclenche automatiquement quand vous créez un **tag de version**.

```powershell
# Créez un tag pour la version 1.0.0.0
git tag v1.0.0.0

# Poussez le tag sur GitHub
git push origin v1.0.0.0
```

### Étape 6 : Vérifier le build automatique

1. **Allez sur GitHub** :
   ```
   https://github.com/VOTRE_NOM_UTILISATEUR/logiciel-impression-3d
   ```

2. **Cliquez sur l'onglet "Actions"**

3. **Vous devriez voir un workflow en cours** :
   - Nom : `Build and Release`
   - Status : ?? En cours / ?? Réussi / ?? Échoué

4. **Attendez la fin du build** (environ 5-10 minutes)

### Étape 7 : Vérifier la release créée

1. **Allez dans l'onglet "Releases"** :
   ```
   https://github.com/VOTRE_NOM_UTILISATEUR/logiciel-impression-3d/releases
   ```

2. **Vous devriez voir "Version 1.0.0.0"** avec 3 fichiers :
   - ? `Logiciel_Impression_3D_Setup_1.0.0.0.exe` (Installateur)
   - ? `logiciel-impression-3d.exe` (Exécutable portable)
   - ? `version.txt` (Fichier de version)

---

## ?? Test de la mise à jour

### Étape 8 : Tester la détection de mise à jour

Pour tester, nous allons créer une **version 1.0.0.1** :

1. **Modifiez `Properties\AssemblyInfo.cs`** :
   ```csharp
   [assembly: AssemblyVersion("1.0.0.1")]
   [assembly: AssemblyFileVersion("1.0.0.1")]
   ```

2. **Committez et poussez** :
   ```powershell
   git add Properties\AssemblyInfo.cs
   git commit -m "Bump version to 1.0.0.1"
   git push origin main
   ```

3. **Créez un nouveau tag** :
   ```powershell
   git tag v1.0.0.1
   git push origin v1.0.0.1
   ```

4. **Attendez le build**

5. **Lancez votre ancienne version (1.0.0.0)** :
   - L'application devrait détecter la version 1.0.0.1
   - Une popup devrait apparaître proposant la mise à jour
   - Cliquez sur "Oui" pour télécharger et installer

---

## ?? Publier de nouvelles versions

### Processus de release

À chaque fois que vous voulez publier une nouvelle version :

```powershell
# 1. Modifiez le code
# 2. Mettez à jour la version dans AssemblyInfo.cs

# 3. Committez vos changements
git add .
git commit -m "Description des changements"
git push origin main

# 4. Créez un tag avec la nouvelle version
git tag v1.0.1.0
git push origin v1.0.1.0

# 5. GitHub Actions s'occupe du reste !
```

### Format de version

Utilisez le format **Semantic Versioning** :
- **v1.0.0.0** : Version initiale
- **v1.0.0.1** : Correction de bug mineure
- **v1.0.1.0** : Nouvelle fonctionnalité mineure
- **v1.1.0.0** : Nouvelle fonctionnalité majeure
- **v2.0.0.0** : Changement majeur (breaking changes)

---

## ??? Résolution des problèmes

### Le workflow GitHub Actions échoue

1. **Consultez les logs** :
   - Allez dans **Actions** > Cliquez sur le workflow échoué
   - Regardez quelle étape a échoué

2. **Problèmes courants** :

   **Erreur : "MSBuild not found"**
   - Le workflow utilise `microsoft/setup-msbuild@v2`
   - Vérifiez que l'étape "Setup MSBuild" s'est exécutée

   **Erreur : "Inno Setup compilation failed"**
   - Vérifiez les chemins dans `setup_script.iss`
   - Les chemins absolus doivent être remplacés par des chemins relatifs

   **Erreur : "GitHub Release creation failed"**
   - Vérifiez que le tag existe : `git tag -l`
   - Le tag doit commencer par `v` (ex: `v1.0.0.0`)

### L'application ne détecte pas les mises à jour

1. **Vérifiez la configuration dans UpdateManager.cs** :
   ```csharp
   private const string GithubOwner = "VOTRE_NOM_UTILISATEUR";
   private const string GithubRepo = "logiciel-impression-3d";
   ```

2. **Vérifiez que la release est publique** :
   - Allez sur la page de la release
   - Elle ne doit pas être marquée comme "Draft" ou "Pre-release"

3. **Testez l'API manuellement** :
   - Ouvrez dans votre navigateur :
   ```
   https://api.github.com/repos/VOTRE_NOM_UTILISATEUR/logiciel-impression-3d/releases/latest
   ```
   - Vous devriez voir un JSON avec les infos de la release

4. **Vérifiez la connexion Internet** :
   - L'application nécessite Internet pour vérifier les mises à jour

### Erreur "rate limit exceeded" de GitHub API

- GitHub limite à **60 requêtes/heure** pour les requêtes non authentifiées
- Pour lever cette limite, vous pouvez créer un **GitHub Token** :
  1. Créez un token : https://github.com/settings/tokens
  2. Cochez uniquement `public_repo`
  3. Modifiez `UpdateManager.cs` pour ajouter l'authentification

---

## ?? Notes importantes

### Sécurité

- ? **Ne committez JAMAIS** :
  - Mots de passe
  - Clés API privées
  - Certificats (.pfx)
  - Fichiers de configuration sensibles

- ? **Utilisez `.gitignore`** pour exclure :
  ```
  bin/
  obj/
  *.user
  *.suo
  *.pfx
  users.dat
  parametres_impression.dat
  ```

### Bonnes pratiques

1. **Testez localement avant de pousser** :
   ```powershell
   # Build en Release
   msbuild "logiciel d'impression 3d.sln" /p:Configuration=Release
   ```

2. **Créez des releases stables** :
   - Testez thoroughly avant de créer un tag
   - Utilisez des branches pour les développements

3. **Documentez vos releases** :
   - Modifiez le fichier `.github/workflows/build-and-release.yml`
   - Section `body:` pour personnaliser les notes de version

---

## ?? Félicitations !

Votre système de mise à jour automatique est maintenant configuré ! ??

### Récapitulatif du workflow

1. ? Vous codez une nouvelle fonctionnalité
2. ? Vous mettez à jour la version dans `AssemblyInfo.cs`
3. ? Vous committez et poussez sur GitHub
4. ? Vous créez un tag (ex: `v1.0.1.0`)
5. ? GitHub Actions build automatiquement
6. ? Une release est créée avec l'installateur
7. ? Les utilisateurs reçoivent la notification de mise à jour !

### Support

Si vous rencontrez des problèmes :
1. Consultez les logs GitHub Actions
2. Vérifiez la configuration dans `UpdateManager.cs`
3. Testez l'API GitHub manuellement

---

**Auteur** : Configuration automatique pour logiciel d'impression 3D  
**Date** : 2024  
**Version du guide** : 1.0
