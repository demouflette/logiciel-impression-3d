# ?? Alternative : Système de Mise à Jour sans GitHub

## Pour les utilisateurs qui NE PEUVENT PAS utiliser GitHub

Cette solution utilise :
- ? **Votre propre serveur web** (ou FTP, Google Drive, Dropbox, etc.)
- ? **Build manuel** (pas de GitHub Actions)
- ? **Même système de détection** de mise à jour

---

## ?? Prérequis

Vous avez besoin de :
- Un espace web accessible par URL (exemples ci-dessous)
- Possibilité d'uploader des fichiers

### Options d'hébergement

| Service | Gratuit ? | URL accessible ? |
|---------|-----------|------------------|
| **Google Drive** | ? Oui | ? Oui (avec partage public) |
| **Dropbox** | ? Oui | ? Oui (avec lien public) |
| **OneDrive** | ? Oui | ? Oui (avec partage) |
| **Serveur FTP** | ?? Payant | ? Oui |
| **GitHub Pages** | ? Oui | ? Oui (mais nécessite GitHub) |

---

## ?? Configuration

### Étape 1 : Choisir l'hébergement

#### Option A : Google Drive (RECOMMANDÉ - Gratuit)

1. **Créez un dossier** dans Google Drive nommé `logiciel-impression-3d-updates`
2. **Partagez-le** :
   - Clic droit ? Partager
   - Changez en "Tous les utilisateurs avec le lien"
   - Copiez le lien de partage

#### Option B : Dropbox

1. **Créez un dossier** `logiciel-impression-3d-updates`
2. **Créez un lien public** :
   - Clic droit ? Partager ? Créer un lien
   - Copiez le lien

#### Option C : Votre propre serveur web

- Créez un dossier accessible par HTTP
- Exemple : `https://monsite.com/updates/`

---

### Étape 2 : Préparer les fichiers

Vous devez uploader **3 fichiers** :

1. **`version.txt`** - Contient la version actuelle
   ```
   1.0.0.0
   ```

2. **`logiciel_impression_3d.exe`** - L'exécutable portable

3. **`Logiciel_Impression_3D_Setup.exe`** - L'installateur (optionnel)

---

### Étape 3 : Obtenir les URLs directes

#### Pour Google Drive

Les liens Google Drive ressemblent à :
```
https://drive.google.com/file/d/XXXXXXXXXX/view?usp=sharing
```

Pour obtenir un lien de **téléchargement direct** :
```
https://drive.google.com/uc?export=download&id=XXXXXXXXXX
```

Remplacez `XXXXXXXXXX` par l'ID du fichier.

#### Pour Dropbox

Remplacez `www.dropbox.com` par `dl.dropboxusercontent.com` et retirez `?dl=0`

**Avant** :
```
https://www.dropbox.com/s/abc123/version.txt?dl=0
```

**Après** :
```
https://dl.dropboxusercontent.com/s/abc123/version.txt
```

---

### Étape 4 : Modifier UpdateManager.cs

Créez un nouveau fichier `UpdateManager_Simple.cs` :

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    public class UpdateManager
    {
        // ?????? MODIFIEZ CES URLS ??????
        
        // URL du fichier version.txt
        private const string UrlVersionServeur = "https://votreurl.com/version.txt";
        
        // URL de l'exécutable à télécharger
        private const string UrlTelechargement = "https://votreurl.com/logiciel_impression_3d.exe";
        
        // ?????? MODIFIEZ CES URLS ??????

        public static void VerifierMiseAJour(bool afficherSiAJour = false)
        {
            try
            {
                string versionActuelle = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                
                using (WebClient client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    
                    // Télécharger le fichier version.txt
                    string versionDistante = client.DownloadString(UrlVersionServeur).Trim();
                    
                    Version verActuelle = new Version(versionActuelle);
                    Version verDistante = new Version(versionDistante);
                    
                    if (verDistante > verActuelle)
                    {
                        DialogResult result = MessageBox.Show(
                            $"Une nouvelle version est disponible !\n\n" +
                            $"Version actuelle : {versionActuelle}\n" +
                            $"Nouvelle version : {versionDistante}\n\n" +
                            $"Voulez-vous télécharger la mise à jour ?",
                            "Mise à jour disponible",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            TelechargerMiseAJour();
                        }
                    }
                    else if (afficherSiAJour)
                    {
                        MessageBox.Show(
                            $"Vous utilisez la dernière version ({versionActuelle}).",
                            "À jour",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (WebException)
            {
                if (afficherSiAJour)
                {
                    MessageBox.Show("Impossible de vérifier les mises à jour.\nVérifiez votre connexion Internet.", 
                        "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur vérification MAJ: {ex.Message}");
                if (afficherSiAJour)
                {
                    MessageBox.Show($"Erreur lors de la vérification : {ex.Message}", 
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void TelechargerMiseAJour()
        {
            try
            {
                string cheminTemp = Path.Combine(Path.GetTempPath(), "logiciel_impression_3d_update.exe");
                
                Form progressForm = new Form
                {
                    Text = "Téléchargement en cours...",
                    Size = new System.Drawing.Size(400, 150),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                ProgressBar progressBar = new ProgressBar
                {
                    Location = new System.Drawing.Point(20, 40),
                    Size = new System.Drawing.Size(340, 30)
                };
                progressForm.Controls.Add(progressBar);

                Label lblStatus = new Label
                {
                    Text = "Téléchargement de la mise à jour...",
                    Location = new System.Drawing.Point(20, 15),
                    AutoSize = true
                };
                progressForm.Controls.Add(lblStatus);

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                        lblStatus.Text = $"Téléchargement : {e.ProgressPercentage}%";
                    };

                    client.DownloadFileCompleted += (s, e) =>
                    {
                        progressForm.Close();
                        
                        if (e.Error == null)
                        {
                            MessageBox.Show("Téléchargement terminé ! L'application va redémarrer.", 
                                "Mise à jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            Process.Start(cheminTemp);
                            Application.Exit();
                        }
                        else
                        {
                            MessageBox.Show($"Erreur lors du téléchargement : {e.Error.Message}", 
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    client.DownloadFileAsync(new Uri(UrlTelechargement), cheminTemp);
                    progressForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du téléchargement : {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
```

---

### Étape 5 : Workflow de publication

#### Quand vous avez une nouvelle version :

1. **Compilez** votre projet en Release (Visual Studio)
2. **Créez** l'installateur (si vous utilisez Inno Setup) :
   ```cmd
   .\build_installer.bat
   ```
3. **Mettez à jour** `version.txt` :
   ```
   1.0.1.0
   ```
4. **Uploadez** les fichiers sur votre hébergement :
   - `version.txt`
   - `logiciel_impression_3d.exe`
   - `Logiciel_Impression_3D_Setup.exe` (optionnel)

C'est tout ! Les utilisateurs recevront la notification automatiquement.

---

## ?? Exemple avec Google Drive

### Configuration complète

1. **Créez** 3 fichiers dans Google Drive :
   - `version.txt` (contenu : `1.0.0.0`)
   - `logiciel_impression_3d.exe`
   - `Logiciel_Impression_3D_Setup.exe`

2. **Partagez** chaque fichier publiquement

3. **Obtenez les IDs** des fichiers :
   ```
   version.txt       ? ID: ABC123
   logiciel_impression_3d.exe ? ID: DEF456
   ```

4. **Modifiez** `UpdateManager.cs` :
   ```csharp
   private const string UrlVersionServeur = 
       "https://drive.google.com/uc?export=download&id=ABC123";
   
   private const string UrlTelechargement = 
       "https://drive.google.com/uc?export=download&id=DEF456";
   ```

---

## ? Avantages

- ? **Pas besoin de Git** ou GitHub
- ? **Gratuit** (avec Google Drive/Dropbox)
- ? **Simple** - juste uploader des fichiers
- ? **Fonctionne** avec n'importe quel hébergement web

## ?? Inconvénients

- ? **Build manuel** (pas d'automatisation)
- ? **Upload manuel** des fichiers
- ? **Pas d'historique** de versions
- ? Moins professionnel que GitHub Releases

---

## ?? Script PowerShell pour automatiser l'upload

Si vous utilisez un serveur FTP, voici un script :

```powershell
# upload-update.ps1
param(
    [string]$Version = "1.0.0.0",
    [string]$FtpServer = "ftp://monserveur.com/updates/",
    [string]$Username = "user",
    [string]$Password = "pass"
)

# Créer version.txt
Set-Content -Path "version.txt" -Value $Version

# Upload des fichiers via FTP
$files = @(
    "version.txt",
    "bin\Release\logiciel d'impression 3d.exe",
    "Installer\Logiciel_Impression_3D_Setup_$Version.exe"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        $webclient = New-Object System.Net.WebClient
        $webclient.Credentials = New-Object System.Net.NetworkCredential($Username, $Password)
        $uri = New-Object System.Uri($FtpServer + (Split-Path $file -Leaf))
        $webclient.UploadFile($uri, $file)
        Write-Host "? Uploadé: $file"
    }
}

Write-Host "? Mise à jour publiée: v$Version"
```

Usage :
```powershell
.\upload-update.ps1 -Version "1.0.1.0" -FtpServer "ftp://monsite.com/updates/" -Username "user" -Password "pass"
```

---

## ?? Comparaison des solutions

| Critère | GitHub | Google Drive | Serveur FTP |
|---------|--------|--------------|-------------|
| **Gratuit** | ? Oui | ? Oui | ? Non |
| **Build auto** | ? Oui | ? Non | ? Non |
| **Facile** | ?? Moyen | ? Oui | ?? Moyen |
| **Pro** | ? Oui | ? Non | ?? Moyen |

---

**Cette solution ne nécessite ni Git ni GitHub ! ??**
