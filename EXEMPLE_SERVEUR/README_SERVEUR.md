# Configuration du serveur de mise à jour

## Fichiers nécessaires

### 1. version.txt
Contient la version actuelle du logiciel (format: X.X.X.X)
```
1.0.0.0
```

### 2. imprimantes_specs.txt
Format : `Nom|PuissanceMax(W)|ConsommationMoyenne(W)|Source`
```
Bambu Lab X1 Carbon|350|200|Spécifications Bambu Lab officielles
Bambu Lab P1P|300|180|Spécifications Bambu Lab officielles
```

### 3. logiciel_impression_3d_latest.exe
L'exécutable de la dernière version

## Hébergement

### Option A : GitHub (Gratuit, recommandé)
1. Créez un repository public sur GitHub
2. Uploadez les fichiers :
   - version.txt
   - imprimantes_specs.txt
   - logiciel_impression_3d_latest.exe (dans Releases)

3. Utilisez les URLs brutes :
   ```
   https://raw.githubusercontent.com/VOTRE_USERNAME/VOTRE_REPO/master/version.txt
   https://raw.githubusercontent.com/VOTRE_USERNAME/VOTRE_REPO/master/imprimantes_specs.txt
   ```

### Option B : Serveur web personnel
1. Uploadez les fichiers sur votre serveur
2. URLs directes :
   ```
   https://votre-domaine.com/version.txt
   https://votre-domaine.com/imprimantes_specs.txt
   https://votre-domaine.com/logiciel_impression_3d_latest.exe
   ```

## Configuration dans le code

Modifiez les URLs dans :
- `UpdateManager.cs` ligne 10-11
- `ImprimanteSpecsManager.cs` ligne 10

```csharp
private const string UrlVersionServeur = "VOTRE_URL/version.txt";
private const string UrlApiSpecs = "VOTRE_URL/imprimantes_specs.txt";
```

## Mise à jour de la version

1. Projet ? Propriétés ? Application ? Informations d'assembly
2. Modifier le numéro de version
3. Recompiler
4. Uploader le nouvel .exe sur le serveur
5. Mettre à jour version.txt avec le nouveau numéro

## Test

Depuis le logiciel :
- Menu Aide ? Vérifier les mises à jour
- Menu Aide ? Rafraîchir specs imprimantes
