# ??? Logiciel de Calcul de Devis pour Impression 3D

> Application Windows pour calculer automatiquement les devis d'impression 3D avec support multi-couleurs et mise à jour automatique

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8.1-blue)](https://dotnet.microsoft.com/download/dotnet-framework/net481)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/VOTRE_NOM/logiciel-impression-3d)](https://github.com/VOTRE_NOM/logiciel-impression-3d/releases)

---

## ?? Table des matières

- [Fonctionnalités](#-fonctionnalités)
- [Installation](#-installation)
- [Démarrage rapide](#-démarrage-rapide)
- [Système de mise à jour](#-système-de-mise-à-jour)
- [Documentation](#-documentation)
- [Développement](#-développement)
- [Contribuer](#-contribuer)
- [License](#-license)

---

## ? Fonctionnalités

### Calcul de devis intelligent
- ? **Calcul automatique** du prix de revient
- ? **Support multi-couleurs** avec AMS Bambu Lab
- ? **Gestion des bobines** et tarifs
- ? **Calcul TVA** et marge personnalisable
- ? **Multiple imprimantes** supportées (Bambu Lab, Creality, Prusa, etc.)

### Gestion utilisateurs
- ? **Système de connexion** sécurisé
- ? **Profils utilisateurs** personnalisables
- ? **Paramètres d'impression** sauvegardés

### Mise à jour automatique
- ? **Détection automatique** des nouvelles versions
- ? **Téléchargement en un clic** depuis GitHub
- ? **Build automatique** via GitHub Actions
- ? **Releases GitHub** avec installateur

---

## ?? Installation

### Pour les utilisateurs

1. **Téléchargez la dernière version** :
   - Allez sur [Releases](https://github.com/VOTRE_NOM/logiciel-impression-3d/releases)
   - Téléchargez `Logiciel_Impression_3D_Setup_X.X.X.X.exe`

2. **Installez le logiciel** :
   - Exécutez l'installateur
   - Suivez les instructions à l'écran

3. **Lancez l'application** :
   - L'application se lance automatiquement
   - Créez votre compte ou connectez-vous

### Prérequis

- **Windows 10/11** (64-bit recommandé)
- **.NET Framework 4.8.1** (installé automatiquement si nécessaire)

---

## ? Démarrage rapide

### Première utilisation

1. **Créez un compte utilisateur**
   - Lancez l'application
   - Cliquez sur "S'inscrire"
   - Remplissez vos informations

2. **Configurez votre imprimante**
   - Sélectionnez votre modèle d'imprimante
   - Configurez les paramètres (AMS, nombre de couleurs, etc.)

3. **Calculez votre premier devis**
   - Entrez le nombre d'objets
   - Configurez les couleurs et poids
   - Entrez le temps d'impression
   - Cliquez sur "Calculer le devis"

---

## ?? Système de mise à jour

### Pour les utilisateurs

L'application **vérifie automatiquement** les mises à jour au démarrage.

- ? Une notification apparaît si une nouvelle version est disponible
- ? Téléchargement en un clic
- ? Installation automatique

**Vérification manuelle** :
- Menu **Aide** ? **Vérifier les mises à jour**

### Pour les développeurs

Le système de mise à jour utilise **GitHub Releases** et **GitHub Actions**.

#### Configuration initiale (une seule fois)

```powershell
# Utilisez le script de configuration automatique
.\setup-github-updates.ps1 -GithubUsername "VOTRE_NOM"
```

Ou suivez le guide : **[QUICK_START.md](QUICK_START.md)**

#### Publier une nouvelle version

```powershell
# Méthode automatique (recommandée)
.\publish-release.ps1 -Version "1.0.1.0" -Message "Description des changements"

# Méthode manuelle
git tag v1.0.1.0
git push origin v1.0.1.0
```

GitHub Actions s'occupe automatiquement de :
- ? Compiler le projet
- ? Créer l'installateur
- ? Publier la release
- ? Uploader les fichiers

---

## ?? Documentation

### Pour les utilisateurs

- **Guide d'utilisation** : *(À créer)*

### Pour les développeurs

| Document | Description | Temps de lecture |
|----------|-------------|------------------|
| **[QUICK_START.md](QUICK_START.md)** | Configuration rapide en 5 étapes | 5 min |
| **[GUIDE_MISE_A_JOUR_GITHUB.md](GUIDE_MISE_A_JOUR_GITHUB.md)** | Documentation complète du système de mise à jour | 20 min |
| **[RECAPITULATIF_CONFIGURATION.md](RECAPITULATIF_CONFIGURATION.md)** | Récapitulatif de la configuration | 10 min |

### Scripts disponibles

| Script | Usage | Description |
|--------|-------|-------------|
| `setup-github-updates.ps1` | Configuration initiale | Configure automatiquement le système de mise à jour |
| `publish-release.ps1` | Publication de version | Automatise la création de releases GitHub |
| `build_installer.bat` | Build local | Compile et crée l'installateur localement |

---

## ?? Développement

### Prérequis

- **Visual Studio 2022** (Community ou supérieur)
- **.NET Framework 4.8.1 SDK**
- **Inno Setup 6** (pour créer l'installateur)
- **Git** (pour le contrôle de version)

### Configuration de l'environnement

1. **Clonez le dépôt** :
   ```powershell
   git clone https://github.com/VOTRE_NOM/logiciel-impression-3d.git
   cd logiciel-impression-3d
   ```

2. **Ouvrez la solution** :
   - Ouvrez `logiciel d'impression 3d.sln` dans Visual Studio

3. **Restaurez les packages NuGet** :
   - Visual Studio le fait automatiquement

4. **Compilez et exécutez** :
   - Appuyez sur **F5** pour debugger
   - Ou **Ctrl+F5** pour exécuter sans debugger

### Structure du projet

```
logiciel-impression-3d/
??? .github/
?   ??? workflows/
?       ??? build-and-release.yml    # Workflow GitHub Actions
??? Properties/
?   ??? AssemblyInfo.cs              # Informations de version
??? bin/                             # Binaires compilés
??? obj/                             # Fichiers objets temporaires
??? Form1.cs                         # Fenêtre principale
??? LoginForm.cs                     # Fenêtre de connexion
??? UpdateManager.cs                 # Gestionnaire de mises à jour
??? UserManager.cs                   # Gestion des utilisateurs
??? ImprimanteSpecsManager.cs        # Spécifications imprimantes
??? setup_script.iss                 # Script Inno Setup
??? build_installer.bat              # Script de build local
??? setup-github-updates.ps1         # Configuration initiale
??? publish-release.ps1              # Publication automatique
??? README.md                        # Ce fichier
```

### Tests

```powershell
# Build en mode Release
msbuild "logiciel d'impression 3d.sln" /p:Configuration=Release

# Créer l'installateur localement
.\build_installer.bat
```

---

## ?? Contribuer

Les contributions sont les bienvenues !

### Comment contribuer

1. **Forkez le projet**
2. **Créez une branche** : `git checkout -b feature/ma-fonctionnalite`
3. **Committez vos changements** : `git commit -m 'Ajout d'une fonctionnalité'`
4. **Poussez la branche** : `git push origin feature/ma-fonctionnalite`
5. **Ouvrez une Pull Request**

### Guidelines

- ? Suivez le style de code existant
- ? Commentez votre code
- ? Testez vos modifications
- ? Mettez à jour la documentation si nécessaire

---

## ?? License

Ce projet est sous licence **MIT** - voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

## ????? Auteur

**DemouFlette**

- GitHub: [@demouflette](https://github.com/demouflette)
- Email: *(votre email)*

---

## ?? Remerciements

- **Bambu Lab** - Pour leurs imprimantes 3D innovantes
- **Inno Setup** - Pour le système d'installation
- **GitHub** - Pour l'hébergement et les GitHub Actions

---

## ?? Versions

- **v1.0.0.0** (2024) - Version initiale
  - ? Système de calcul de devis
  - ? Gestion utilisateurs
  - ? Support multi-couleurs
  - ? Mise à jour automatique

Voir [CHANGELOG.md](CHANGELOG.md) pour l'historique complet des versions.

---

## ?? Support

Pour toute question ou problème :

1. **Consultez la documentation** dans le dossier `docs/`
2. **Ouvrez une issue** : [GitHub Issues](https://github.com/VOTRE_NOM/logiciel-impression-3d/issues)
3. **Contactez-moi** : *(votre email)*

---

<div align="center">

**Fait avec ?? pour la communauté de l'impression 3D**

[? Retour en haut](#-logiciel-de-calcul-de-devis-pour-impression-3d)

</div>
