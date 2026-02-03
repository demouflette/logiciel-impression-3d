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
        // Configuration GitHub - REMPLACEZ par votre nom d'utilisateur et nom de dépôt
        // TODO: Remplacez "VOTRE_NOM_UTILISATEUR" par votre vrai nom d'utilisateur GitHub
        private const string GithubOwner = "demouflette";  // ⬅️ CHANGEZ ICI !
        private const string GithubRepo = "logiciel-impression-3d";
        
        // URLs GitHub Releases
        private const string UrlLatestRelease = "https://api.github.com/repos/{0}/{1}/releases/latest";
        
        // Nom des fichiers dans la release
        private const string InstallerFileName = "Logiciel_Impression_3D_Setup_";  // Suffixe avec version
        private const string ExeFileName = "logiciel-impression-3d.exe";

        public static void VerifierMiseAJour(bool afficherSiAJour = false)
        {
            try
            {
                string versionActuelle = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                
                // Récupérer les informations de la dernière release GitHub
                var releaseInfo = ObtenirDerniereRelease();
                
                if (releaseInfo == null)
                {
                    if (afficherSiAJour)
                    {
                        MessageBox.Show("Impossible de récupérer les informations de mise à jour.", 
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }
                
                string versionDistante = releaseInfo.Version;
                string urlTelechargement = releaseInfo.DownloadUrl;
                string urlPageRelease = releaseInfo.HtmlUrl;
                
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
                        TelechargerMiseAJour(urlTelechargement, $"logiciel_impression_3d_v{versionDistante}.exe");
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

        private static ReleaseInfo ObtenirDerniereRelease()
        {
            try
            {
                string url = string.Format(UrlLatestRelease, GithubOwner, GithubRepo);
                
                using (WebClient client = new WebClient())
                {
                    // GitHub API nécessite un User-Agent
                    client.Headers.Add("User-Agent", "Logiciel-Impression-3D-Updater");
                    
                    string json = client.DownloadString(url);
                    
                    // Parse simple du JSON (sans dépendance externe)
                    string version = ExtraireValeurJson(json, "tag_name");
                    version = version.Replace("v", "").Trim(); // Retirer le 'v' du tag
                    
                    string htmlUrl = ExtraireValeurJson(json, "html_url");
                    
                    // Récupérer l'URL de téléchargement de l'exécutable
                    string downloadUrl = TrouverUrlAsset(json, ExeFileName);
                    
                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        // Fallback: essayer de trouver l'installateur
                        downloadUrl = TrouverUrlAssetPartiel(json, InstallerFileName);
                    }
                    
                    return new ReleaseInfo
                    {
                        Version = version,
                        DownloadUrl = downloadUrl,
                        HtmlUrl = htmlUrl
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la récupération de la release : {ex.Message}");
                return null;
            }
        }

        private static string ExtraireValeurJson(string json, string cle)
        {
            try
            {
                string pattern = $"\"{cle}\":\"";
                int startIndex = json.IndexOf(pattern);
                if (startIndex == -1) return string.Empty;
                
                startIndex += pattern.Length;
                int endIndex = json.IndexOf("\"", startIndex);
                if (endIndex == -1) return string.Empty;
                
                return json.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string TrouverUrlAsset(string json, string nomFichier)
        {
            try
            {
                string pattern = $"\"name\":\"{nomFichier}\"";
                int nameIndex = json.IndexOf(pattern);
                if (nameIndex == -1) return string.Empty;
                
                // Chercher le browser_download_url dans le même objet
                int urlStartPattern = json.IndexOf("\"browser_download_url\":\"", nameIndex);
                if (urlStartPattern == -1) return string.Empty;
                
                urlStartPattern += "\"browser_download_url\":\"".Length;
                int urlEnd = json.IndexOf("\"", urlStartPattern);
                if (urlEnd == -1) return string.Empty;
                
                return json.Substring(urlStartPattern, urlEnd - urlStartPattern);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string TrouverUrlAssetPartiel(string json, string nomFichierPartiel)
        {
            try
            {
                // Chercher un asset dont le nom contient le pattern
                int searchIndex = 0;
                while (true)
                {
                    int nameStart = json.IndexOf("\"name\":\"", searchIndex);
                    if (nameStart == -1) return string.Empty;
                    
                    nameStart += "\"name\":\"".Length;
                    int nameEnd = json.IndexOf("\"", nameStart);
                    if (nameEnd == -1) return string.Empty;
                    
                    string fileName = json.Substring(nameStart, nameEnd - nameStart);
                    
                    if (fileName.Contains(nomFichierPartiel) && fileName.EndsWith(".exe"))
                    {
                        // Trouver l'URL de téléchargement pour cet asset
                        int urlStartPattern = json.IndexOf("\"browser_download_url\":\"", nameEnd);
                        if (urlStartPattern == -1) return string.Empty;
                        
                        urlStartPattern += "\"browser_download_url\":\"".Length;
                        int urlEnd = json.IndexOf("\"", urlStartPattern);
                        if (urlEnd == -1) return string.Empty;
                        
                        return json.Substring(urlStartPattern, urlEnd - urlStartPattern);
                    }
                    
                    searchIndex = nameEnd;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void TelechargerMiseAJour(string urlTelechargement, string nomFichier)
        {
            try
            {
                if (string.IsNullOrEmpty(urlTelechargement))
                {
                    MessageBox.Show("URL de téléchargement invalide.", 
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string cheminTemp = Path.Combine(Path.GetTempPath(), nomFichier);
                
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
                    client.Headers.Add("User-Agent", "Logiciel-Impression-3D-Updater");
                    
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                        lblStatus.Text = $"Téléchargement : {e.ProgressPercentage}% ({e.BytesReceived / 1024 / 1024} MB / {e.TotalBytesToReceive / 1024 / 1024} MB)";
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

                    client.DownloadFileAsync(new Uri(urlTelechargement), cheminTemp);
                    progressForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du téléchargement : {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Classe pour stocker les informations de release
        private class ReleaseInfo
        {
            public string Version { get; set; }
            public string DownloadUrl { get; set; }
            public string HtmlUrl { get; set; }
        }
    }
}
