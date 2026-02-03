# ?? Configuration Express - Une Page

## ? En 3 commandes (après avoir créé le dépôt GitHub)

```powershell
# 1. Configuration automatique
.\setup-github-updates.ps1 -GithubUsername "VOTRE_NOM_GITHUB"

# 2. Poussez sur GitHub
git push -u origin main

# 3. Créez la première release
git tag v1.0.0.0
git push origin v1.0.0.0
```

**C'est tout ! ??**

---

## ?? Checklist Rapide

### Avant de commencer

- [ ] Git installé ? https://git-scm.com/download/win
- [ ] Compte GitHub créé ? https://github.com/signup
- [ ] Dépôt GitHub créé ? https://github.com/new
  - Nom : `logiciel-impression-3d`
  - Type : Public ou Private
  - NE RIEN COCHER d'autre

### Configuration (5 minutes)

```powershell
# Dans PowerShell, allez dans le dossier du projet
cd "D:\projet_3d\logiciel d'impression 3d"

# Lancez le script de configuration
.\setup-github-updates.ps1
```

Le script vous demandera :
1. ? Votre nom d'utilisateur GitHub
2. ? Votre nom pour Git
3. ? Votre email pour Git

### Pousser sur GitHub (2 minutes)

```powershell
# Poussez le code
git push -u origin main
```

**?? Authentification** :
- Nom d'utilisateur : `VOTRE_NOM_GITHUB`
- Mot de passe : **Utilisez un Personal Access Token**
  - Créez-en un : https://github.com/settings/tokens
  - Cochez : `repo`
  - Copiez et utilisez comme mot de passe

### Créer la release (1 minute)

```powershell
git tag v1.0.0.0
git push origin v1.0.0.0
```

### Vérifier (5-10 minutes)

1. **GitHub Actions** : https://github.com/VOTRE_NOM/logiciel-impression-3d/actions
   - Attendez que le workflow se termine (?? vert = succès)

2. **Release** : https://github.com/VOTRE_NOM/logiciel-impression-3d/releases
   - Vous devez voir "Version 1.0.0.0" avec 3 fichiers

---

## ?? Publier de nouvelles versions

### Méthode 1 : Script automatique (recommandé)

```powershell
.\publish-release.ps1 -Version "1.0.1.0" -Message "Corrections de bugs"
```

### Méthode 2 : Manuelle

```powershell
# 1. Modifiez Properties\AssemblyInfo.cs
[assembly: AssemblyVersion("1.0.1.0")]

# 2. Committez
git add .
git commit -m "Version 1.0.1.0"
git push

# 3. Créez le tag
git tag v1.0.1.0
git push origin v1.0.1.0
```

---

## ?? Problèmes courants

### Git n'est pas reconnu
```powershell
# Installez Git : https://git-scm.com/download/win
# Puis relancez PowerShell
```

### Le workflow échoue
1. Allez sur : Actions ? Cliquez sur le workflow
2. Regardez quelle étape a échoué
3. Lisez le message d'erreur

### L'app ne détecte pas les mises à jour
1. Vérifiez `UpdateManager.cs` lignes 12-13 :
   ```csharp
   private const string GithubOwner = "VOTRE_NOM";
   ```
2. Testez : https://api.github.com/repos/VOTRE_NOM/logiciel-impression-3d/releases/latest

---

## ?? Documentation complète

- **[QUICK_START.md](QUICK_START.md)** - Guide détaillé en 5 étapes
- **[GUIDE_MISE_A_JOUR_GITHUB.md](GUIDE_MISE_A_JOUR_GITHUB.md)** - Documentation complète
- **[RECAPITULATIF_CONFIGURATION.md](RECAPITULATIF_CONFIGURATION.md)** - Vue d'ensemble

---

## ? Résultat final

```
Vous créez un tag (v1.0.1.0)
         ?
GitHub Actions compile automatiquement
         ?
Installateur créé
         ?
Release publiée sur GitHub
         ?
Les utilisateurs reçoivent la mise à jour !
```

**Tout automatique ! ??**

---

<div align="center">

**Questions ?** Consultez [GUIDE_MISE_A_JOUR_GITHUB.md](GUIDE_MISE_A_JOUR_GITHUB.md)

</div>
