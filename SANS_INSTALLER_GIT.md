# ? Vous ne pouvez pas installer Git ? Pas de problème !

## ?? 2 Solutions SANS installer Git

---

## Solution 1 : GitHub Desktop (RECOMMANDÉ) ?

### C'est quoi ?
Une application avec des **boutons** au lieu de lignes de commande.

### Installation
1. Téléchargez : https://desktop.github.com/
2. Installez (comme n'importe quelle application)
3. Connectez-vous à GitHub

### Avantages
- ? **Aucune ligne de commande**
- ? **Build automatique** sur GitHub
- ? **Gratuit**
- ? **Facile** - tout en cliquant
- ? **Votre PC peut s'éteindre** après 1 minute ! ??

? **Mon PC doit-il rester allumé ?**  
? **NON !** GitHub fait tout dans le cloud.  
?? [Explications détaillées](PC_DOIT_ETRE_ALLUME.md)

### Documentation complète
?? **[ALTERNATIVE_GITHUB_DESKTOP.md](ALTERNATIVE_GITHUB_DESKTOP.md)**

---

## Solution 2 : Google Drive / Dropbox

### C'est quoi ?
Vous uploadez vos fichiers sur Google Drive ou Dropbox.

### Installation
1. Rien à installer ! Utilisez votre compte Google/Dropbox
2. Uploadez 3 fichiers :
   - `version.txt`
   - `logiciel_impression_3d.exe`
   - (optionnel) l'installateur

### Avantages
- ? **Aucun compte GitHub** nécessaire
- ? **Aucun Git** nécessaire
- ? **Très simple** - comme uploader une photo
- ? **Gratuit**

### Inconvénients
- ? **Pas de build automatique** (vous compilez vous-même)
- ? Upload manuel à chaque version

### Documentation complète
?? **[ALTERNATIVE_SANS_GITHUB.md](ALTERNATIVE_SANS_GITHUB.md)**

---

## ?? Comparaison visuelle

```
??????????????????????????????????????????????
?         GITHUB DESKTOP                      ?
?                                             ?
?  Vous : Clic clic clic                      ?
?         ?                                   ?
?  GitHub Actions : Build automatique         ?
?         ?                                   ?
?  Release : Créée automatiquement            ?
?                                             ?
?  ?? Temps : 2 minutes par version           ?
?  ?? Coût : Gratuit                          ?
?  ?? Difficulté : ? Facile                  ?
??????????????????????????????????????????????

??????????????????????????????????????????????
?         GOOGLE DRIVE / DROPBOX              ?
?                                             ?
?  Vous : Compilez dans Visual Studio         ?
?         ?                                   ?
?  Vous : Uploadez sur Google Drive           ?
?         ?                                   ?
?  Vous : Changez version.txt                 ?
?                                             ?
?  ?? Temps : 5 minutes par version           ?
?  ?? Coût : Gratuit                          ?
?  ?? Difficulté : ?? Moyen                  ?
??????????????????????????????????????????????
```

---

## ?? Laquelle choisir ?

### Choisissez GitHub Desktop si :
- ? Vous pouvez créer un compte GitHub (gratuit)
- ? Vous voulez un build automatique
- ? Vous voulez une solution professionnelle

### Choisissez Google Drive si :
- ? Vous ne voulez vraiment pas créer de compte GitHub
- ? Vous voulez quelque chose de très simple
- ? Vous ne publiez pas souvent de versions

---

## ?? Démarrage rapide

### Pour GitHub Desktop

```
1. Installez GitHub Desktop
2. Créez un compte GitHub
3. Cliquez sur "Publish repository"
4. Créez une release sur GitHub.com
? C'est automatique !
```

?? Guide complet : **[ALTERNATIVE_GITHUB_DESKTOP.md](ALTERNATIVE_GITHUB_DESKTOP.md)**

### Pour Google Drive

```
1. Créez un dossier dans Google Drive
2. Partagez-le publiquement
3. Uploadez vos 3 fichiers
4. Copiez les URLs dans UpdateManager.cs
? Terminé !
```

?? Guide complet : **[ALTERNATIVE_SANS_GITHUB.md](ALTERNATIVE_SANS_GITHUB.md)**

---

## ? Questions ?

### "Je ne veux rien installer du tout"
? **Google Drive** - Utilisez juste votre navigateur web

### "Je veux que ce soit automatique"
? **GitHub Desktop** - Installez juste cette application (pas Git)

### "C'est compliqué ?"
? **Non !** Les deux solutions sont simples. Lisez les guides.

---

## ?? Tous les documents

| Document | Description |
|----------|-------------|
| **[CHOISIR_VOTRE_SOLUTION.md](CHOISIR_VOTRE_SOLUTION.md)** | Comparaison détaillée |
| **[ALTERNATIVE_GITHUB_DESKTOP.md](ALTERNATIVE_GITHUB_DESKTOP.md)** | Guide GitHub Desktop |
| **[ALTERNATIVE_SANS_GITHUB.md](ALTERNATIVE_SANS_GITHUB.md)** | Guide Google Drive/FTP |

---

<div align="center">

## ? Vous n'avez PAS besoin d'installer Git !

**Utilisez GitHub Desktop ou Google Drive**

</div>
