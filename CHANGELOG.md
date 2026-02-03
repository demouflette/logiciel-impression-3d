# Changelog

Toutes les modifications notables de ce projet seront documentées dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/lang/fr/).

---

## [Non publié]

### À venir
- Calcul depuis fichier 3MF
- Export des devis en PDF
- Historique des devis

---

## [1.0.0.0] - 2024-XX-XX

### ?? Version initiale

#### Ajouté
- ? **Système de calcul de devis** pour impression 3D
  - Calcul automatique du prix de revient
  - Support du poids et temps d'impression
  - Calcul de la consommation électrique
  
- ? **Gestion multi-couleurs**
  - Support AMS Bambu Lab (jusqu'à 4 unités)
  - Configuration flexible des plateaux et couleurs
  - DataGridView pour saisie des couleurs
  
- ? **Support multi-imprimantes**
  - Bambu Lab (X1 Carbon, P1P, P2S, A1 Mini)
  - Creality Ender 3
  - Prusa i3 MK3S+
  - Anycubic Kobra
  
- ? **Système de gestion utilisateurs**
  - Inscription et connexion sécurisée
  - Profils utilisateurs personnalisables
  - Sauvegarde chiffrée des données
  
- ? **Paramètres d'impression**
  - Configuration des coûts (électricité, bobines)
  - Gestion des marges et TVA
  - Sauvegarde des paramètres par utilisateur
  
- ? **Mise à jour automatique**
  - Vérification au démarrage
  - Téléchargement depuis GitHub Releases
  - Installation en un clic
  
- ? **Infrastructure de build**
  - GitHub Actions workflow
  - Création automatique d'installateur (Inno Setup)
  - Publication automatique des releases

#### Technique
- Framework : .NET Framework 4.8.1
- UI : Windows Forms
- Installateur : Inno Setup 6
- CI/CD : GitHub Actions
- Hébergement : GitHub Releases

---

## Guide des versions

### Format de version : `MAJOR.MINOR.PATCH.BUILD`

Exemple : `1.2.3.4`

- **MAJOR** (1.x.x.x) : Changements majeurs incompatibles
- **MINOR** (x.2.x.x) : Nouvelles fonctionnalités (compatibles)
- **PATCH** (x.x.3.x) : Corrections de bugs
- **BUILD** (x.x.x.4) : Build/release number

### Types de changements

- **Ajouté** : Nouvelles fonctionnalités
- **Modifié** : Changements dans fonctionnalités existantes
- **Déprécié** : Fonctionnalités qui seront retirées
- **Retiré** : Fonctionnalités retirées
- **Corrigé** : Corrections de bugs
- **Sécurité** : Correctifs de sécurité

---

## Template pour nouvelles versions

```markdown
## [1.0.1.0] - 2024-XX-XX

### Ajouté
- Nouvelle fonctionnalité X
- Support de Y

### Modifié
- Amélioration de Z
- Optimisation de W

### Corrigé
- Correction du bug A
- Résolution du problème B

### Sécurité
- Correctif de sécurité pour C
```

---

## Liens

- [Code source](https://github.com/VOTRE_NOM/logiciel-impression-3d)
- [Releases](https://github.com/VOTRE_NOM/logiciel-impression-3d/releases)
- [Issues](https://github.com/VOTRE_NOM/logiciel-impression-3d/issues)
- [Pull Requests](https://github.com/VOTRE_NOM/logiciel-impression-3d/pulls)

---

**Mis à jour le** : 2024  
**Format** : Keep a Changelog 1.0.0
