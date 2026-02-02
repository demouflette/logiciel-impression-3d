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
        private const string UrlVersionServeur = "https://github.com/demouflette/logiciel-impression-3d-updates/raw/refs/heads/main/version.txt";
        private const string UrlTelechargement = "https://github.com/demouflette/logiciel-impression-3d-updates/raw/refs/heads/main/logiciel_impression_3d.exe";

        public static void VerifierMiseAJour(bool afficherSiAJour = false)
        {
            try
            {
                string versionActuelle = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                
                using (WebClient client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
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
