# ??? Alternative : Utiliser GitHub Desktop (Sans Git en ligne de commande)

## ? Avantages
- ? **Pas besoin d'installer Git** séparément
- ? Interface graphique simple
- ? Tout se fait en cliquant
- ? Fonctionne avec le même système de mise à jour

---

## ?? Installation de GitHub Desktop

### Étape 1 : Télécharger GitHub Desktop

1. **Allez sur** : https://desktop.github.com/
2. **Cliquez sur** : "Download for Windows"
3. **Installez** : Exécutez le fichier téléchargé

### Étape 2 : Connectez-vous à GitHub

1. **Lancez** GitHub Desktop
2. **Cliquez sur** : "Sign in to GitHub.com"
3. **Entrez** vos identifiants GitHub
4. **Autorisez** GitHub Desktop

---

## ?? Utilisation avec GitHub Desktop

### Première publication (Configuration initiale)

#### 1. Créer le dépôt sur GitHub.com

1. Allez sur : https://github.com/new
2. Nom du dépôt : `logiciel-impression-3d`
3. Sélectionnez **Public** ou **Private**
4. Cliquez sur **"Create repository"**

#### 2. Publier le projet avec GitHub Desktop

1. **Dans GitHub Desktop** :
   - Menu **File** ? **Add Local Repository**
   - Sélectionnez : `D:\projet_3d\logiciel d'impression 3d`

2. **Si le dépôt n'est pas initialisé** :
   - GitHub Desktop vous proposera de le créer
   - Cliquez sur **"create a repository"**

3. **Publier sur GitHub** :
   - Cliquez sur **"Publish repository"** (en haut à droite)
   - Nom : `logiciel-impression-3d`
   - Décochez "Keep this code private" si vous voulez un dépôt public
   - Cliquez sur **"Publish repository"**

#### 3. Configurer UpdateManager.cs

Avant de publier, modifiez `UpdateManager.cs` :

```csharp
private const string GithubOwner = "demouflette";  // Votre nom d'utilisateur
private const string GithubRepo = "logiciel-impression-3d";
```

Puis dans GitHub Desktop :
- Les changements apparaissent automatiquement
- Cochez les fichiers modifiés
- En bas à gauche, écrivez : "Configure GitHub repository"
- Cliquez sur **"Commit to main"**
- Cliquez sur **"Push origin"** (en haut)

#### 4. Créer un tag pour la release

**IMPORTANT** : GitHub Desktop ne permet pas de créer des tags facilement.

**Solution** : Créer le tag directement sur GitHub.com

1. Allez sur : `https://github.com/demouflette/logiciel-impression-3d`
2. Cliquez sur **"Releases"** (à droite)
3. Cliquez sur **"Create a new release"**
4. **Tag version** : Écrivez `v1.0.0.0` et cliquez sur "Create new tag"
5. **Release title** : `Version 1.0.0.0`
6. **Description** : Décrivez la version
7. Cliquez sur **"Publish release"**

? Le workflow GitHub Actions se déclenchera automatiquement !

---

### Publier une nouvelle version

#### Méthode avec GitHub Desktop + GitHub.com

1. **Modifiez votre code** dans Visual Studio

2. **Dans GitHub Desktop** :
   - Les changements apparaissent automatiquement
   - Sélectionnez les fichiers à committer
   - Écrivez un message de commit
   - Cliquez sur **"Commit to main"**
   - Cliquez sur **"Push origin"**

3. **Sur GitHub.com** :
   - Allez dans **Releases**
   - Cliquez sur **"Draft a new release"**
   - **Tag** : `v1.0.1.0` (nouvelle version)
   - **Title** : `Version 1.0.1.0`
   - Cliquez sur **"Publish release"**

? Le build se lance automatiquement !

---

## ?? Workflow complet avec GitHub Desktop

```
1. Codez dans Visual Studio
         ?
2. GitHub Desktop détecte les changements automatiquement
         ?
3. Commit dans GitHub Desktop (bouton)
         ?
4. Push dans GitHub Desktop (bouton)
         ?
5. Créez une release sur GitHub.com (cliquez)
         ?
6. GitHub Actions build automatiquement
         ?
7. Release publiée !
```

---

## ? Avantages de cette méthode

- ? **Aucune ligne de commande** nécessaire
- ? **Interface graphique** simple
- ? **Même système de mise à jour** automatique
- ? **Pas besoin d'installer Git** séparément

---

## ?? Résumé des outils

| Outil | Usage | Installation |
|-------|-------|--------------|
| **GitHub Desktop** | Gérer le code (commit, push) | https://desktop.github.com/ |
| **GitHub.com** | Créer les releases (tags) | Navigateur web |
| **GitHub Actions** | Build automatique | Déjà configuré ? |

---

## ?? En cas de problème

### GitHub Desktop ne détecte pas le dépôt

1. Menu **File** ? **Add Local Repository**
2. Si erreur "not a git repository" :
   - Cliquez sur **"create a repository"**
   - GitHub Desktop l'initialisera

### Les changements n'apparaissent pas

1. Vérifiez que vous êtes dans le bon dépôt (en haut)
2. Cliquez sur **"Fetch origin"** pour synchroniser

### Impossible de pusher

1. Vérifiez votre connexion Internet
2. Vérifiez que vous êtes connecté à GitHub (Menu **File** ? **Options** ? **Accounts**)

---

## ?? Documentation

- GitHub Desktop : https://docs.github.com/en/desktop
- Créer des releases : https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository

---

**Avec GitHub Desktop, aucune ligne de commande n'est nécessaire ! ??**
