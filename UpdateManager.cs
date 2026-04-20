using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Mécanisme de mise à jour automatique.
    /// Télécharge l'installeur (setup.exe) depuis GitHub et le lance en mode silencieux.
    /// L'installeur Inno Setup ferme automatiquement l'application avant de remplacer les fichiers.
    /// </summary>
    public class UpdateManager
    {
        private const string GithubOwner   = "demouflette";
        private const string GithubRepo    = "logiciel-impression-3d";
        private const string UrlRelease    = "https://api.github.com/repos/{0}/{1}/releases/latest";
        private const string NomSetupAsset = "setup.exe";
        private const string UrlPageTelechargement = "https://licence.demouflette.fr/telecharger";

        // ─────────────────────────────────────────────────────────────────────
        // Point d'entrée public
        // ─────────────────────────────────────────────────────────────────────

        public static void VerifierMiseAJour(bool afficherSiAJour = false)
        {
            try
            {
                string versionActuelle = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                ReleaseInfo release    = ObtenirDerniereRelease();

                if (release == null)
                {
                    if (afficherSiAJour)
                        MessageBox.Show("Impossible de récupérer les informations de mise à jour.",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Version verActuelle = new Version(versionActuelle);
                Version verDistante = new Version(release.Version);

                if (verDistante <= verActuelle)
                {
                    if (afficherSiAJour)
                        MessageBox.Show($"Vous utilisez la dernière version ({versionActuelle}).",
                            "À jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult rep = MessageBox.Show(
                    $"Une nouvelle version est disponible !\n\n" +
                    $"Version actuelle : {versionActuelle}\n" +
                    $"Nouvelle version  : {release.Version}\n\n" +
                    "Voulez-vous installer la mise à jour ?\n" +
                    "(Le logiciel se fermera puis redémarrera automatiquement.)",
                    "Mise à jour disponible",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (rep != DialogResult.Yes) return;

                if (string.IsNullOrEmpty(release.UrlSetup))
                {
                    // Pas d'installeur dans la release : renvoyer vers la page de téléchargement
                    OuvrirPageTelechargement();
                    return;
                }

                TelechargerEtInstaller(release.UrlSetup, release.Version);
            }
            catch (WebException)
            {
                if (afficherSiAJour)
                    MessageBox.Show("Impossible de vérifier les mises à jour.\nVérifiez votre connexion Internet.",
                        "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur vérification MAJ: {ex.Message}");
                if (afficherSiAJour)
                    MessageBox.Show($"Erreur lors de la vérification : {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Téléchargement de l'installeur avec barre de progression
        // ─────────────────────────────────────────────────────────────────────

        private static void TelechargerEtInstaller(string url, string version)
        {
            string fichierTemp = Path.Combine(
                Path.GetTempPath(),
                $"creacout3d_setup_{version}.exe");

            bool   erreur        = false;
            bool   termine       = false;
            string messageErreur = "";

            Form fmProgress = new Form
            {
                Text            = "Mise à jour – Crea-Coût 3D",
                Size            = new System.Drawing.Size(430, 130),
                StartPosition   = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                ControlBox      = false
            };
            Label lbl = new Label
            {
                Text     = "Téléchargement de la mise à jour…",
                Location = new System.Drawing.Point(20, 15),
                AutoSize = true
            };
            ProgressBar bar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, 45),
                Size     = new System.Drawing.Size(375, 25)
            };
            fmProgress.Controls.Add(lbl);
            fmProgress.Controls.Add(bar);

            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "CreaCout3D-Updater");

            client.DownloadProgressChanged += (s, e) =>
            {
                fmProgress.BeginInvoke(new Action(() =>
                {
                    bar.Value = e.ProgressPercentage;
                    long mo  = 1024 * 1024;
                    lbl.Text = $"Téléchargement : {e.ProgressPercentage}%  " +
                               $"({e.BytesReceived / mo} Mo / {e.TotalBytesToReceive / mo} Mo)";
                }));
            };

            client.DownloadFileCompleted += (s, e) =>
            {
                fmProgress.BeginInvoke(new Action(() =>
                {
                    termine = true;
                    if (e.Error != null)      { erreur = true; messageErreur = e.Error.Message; }
                    else if (e.Cancelled)     { erreur = true; messageErreur = "Téléchargement annulé."; }
                    fmProgress.Close();
                }));
            };

            // Démarrer le download depuis Shown : le SynchronizationContext UI est actif
            fmProgress.Shown += (s, e) => client.DownloadFileAsync(new Uri(url), fichierTemp);

            fmProgress.ShowDialog();
            client.Dispose();
            fmProgress.Dispose();

            if (!termine || erreur)
            {
                if (erreur)
                    MessageBox.Show($"Erreur lors du téléchargement :\n{messageErreur}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(fichierTemp) || new FileInfo(fichierTemp).Length < 1024)
            {
                MessageBox.Show("Le fichier téléchargé est invalide. Réessayez.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LancerInstalleur(fichierTemp);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Lancement de l'installeur Inno Setup en mode silencieux
        // /FORCECLOSEAPPLICATIONS : ferme le logiciel avant de remplacer les fichiers
        // /VERYSILENT             : aucune fenêtre d'installation
        // /NORESTART              : ne pas redémarrer Windows
        // ─────────────────────────────────────────────────────────────────────

        private static void LancerInstalleur(string cheminInstalleur)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName        = cheminInstalleur,
                    Arguments       = "/VERYSILENT /FORCECLOSEAPPLICATIONS /NORESTART",
                    UseShellExecute = true  // Nécessaire pour les exe avec manifest UAC
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Impossible de lancer l'installeur :\n{ex.Message}\n\n" +
                    "Téléchargez la mise à jour manuellement :",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OuvrirPageTelechargement();
                return;
            }

            // L'installeur va forcer la fermeture de cette instance
            // On quitte proprement de notre côté
            Environment.Exit(0);
        }

        private static void OuvrirPageTelechargement()
        {
            try { Process.Start(UrlPageTelechargement); } catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Parsing GitHub Releases API
        // ─────────────────────────────────────────────────────────────────────

        private static ReleaseInfo ObtenirDerniereRelease()
        {
            try
            {
                string url = string.Format(UrlRelease, GithubOwner, GithubRepo);
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "CreaCout3D-Updater");
                    string json = client.DownloadString(url);

                    string tag = ExtraireChaine(json, "tag_name").Replace("v", "").Trim();
                    if (string.IsNullOrEmpty(tag)) return null;

                    return new ReleaseInfo
                    {
                        Version  = tag,
                        UrlSetup = TrouverUrlAsset(json, NomSetupAsset),
                        UrlPage  = ExtraireChaine(json, "html_url")
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ObtenirDerniereRelease: {ex.Message}");
                return null;
            }
        }

        private static string ExtraireChaine(string json, string cle)
        {
            string pattern = $"\"{cle}\":\"";
            int debut = json.IndexOf(pattern);
            if (debut < 0) return string.Empty;
            debut += pattern.Length;
            int fin = json.IndexOf('"', debut);
            return fin < 0 ? string.Empty : json.Substring(debut, fin - debut);
        }

        private static string TrouverUrlAsset(string json, string nomFichier)
        {
            string pattern = $"\"name\":\"{nomFichier}\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return string.Empty;

            string urlPattern = "\"browser_download_url\":\"";
            int urlDebut = json.IndexOf(urlPattern, idx);
            if (urlDebut < 0) return string.Empty;
            urlDebut += urlPattern.Length;

            int urlFin = json.IndexOf('"', urlDebut);
            return urlFin < 0 ? string.Empty : json.Substring(urlDebut, urlFin - urlDebut);
        }

        private class ReleaseInfo
        {
            public string Version  { get; set; }
            public string UrlSetup { get; set; }
            public string UrlPage  { get; set; }
        }
    }
}
