using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Résultat du slicing Bambu Studio
    /// </summary>
    public class ResultatSlicing
    {
        public bool Succes { get; set; }
        public string MessageErreur { get; set; } = "";
        public decimal TempsMinutes { get; set; }
        public decimal PoidsFilamentGrammes { get; set; }
        public decimal PoidsPurgeGrammes { get; set; }
        public decimal FilamentUtiliseMetres { get; set; }
    }

    /// <summary>
    /// Gestionnaire du slicer Bambu Studio CLI.
    /// Détecte l'installation, lance le slicing en arrière-plan,
    /// et parse les résultats du 3MF slicé.
    /// </summary>
    public static class SlicerManager
    {
        // Chemins de détection par défaut
        private static readonly string[] CheminsRecherche = new string[]
        {
            @"C:\Program Files\Bambu Studio\bambu-studio.exe",
            @"C:\Program Files (x86)\Bambu Studio\bambu-studio.exe"
        };

        private static string cheminSlicerCache;

        // ═══════════════════════════════════════════════════════
        // DÉTECTION DU SLICER
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Vérifie si Bambu Studio est installé
        /// </summary>
        public static bool EstInstalle()
        {
            return !string.IsNullOrEmpty(ObtenirCheminSlicer());
        }

        /// <summary>
        /// Retourne le chemin du slicer (configuré ou auto-détecté)
        /// </summary>
        public static string ObtenirCheminSlicer()
        {
            if (cheminSlicerCache != null)
                return cheminSlicerCache;

            // 1. Vérifier le chemin configuré dans les paramètres
            try
            {
                var parametres = ParametresImpressionForm.ObtenirParametres();
                if (!string.IsNullOrEmpty(parametres.CheminSlicer) && File.Exists(parametres.CheminSlicer))
                {
                    cheminSlicerCache = parametres.CheminSlicer;
                    return cheminSlicerCache;
                }
            }
            catch (Exception ex) { LogManager.Erreur("Lecture chemin slicer depuis paramètres", ex); }

            // 2. Auto-détection dans les chemins connus
            foreach (string chemin in CheminsRecherche)
            {
                if (File.Exists(chemin))
                {
                    cheminSlicerCache = chemin;
                    return cheminSlicerCache;
                }
            }

            cheminSlicerCache = "";
            return "";
        }

        /// <summary>
        /// Réinitialise le cache du chemin slicer
        /// </summary>
        public static void InvaliderCache()
        {
            cheminSlicerCache = null;
        }

        // ═══════════════════════════════════════════════════════
        // SLICING EN ARRIÈRE-PLAN
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Lance le slicing en arrière-plan et appelle le callback avec le résultat
        /// </summary>
        public static void SlicerEnArrierePlan(string fichier3mfEntree, Action<ResultatSlicing> callback, int timeoutSecondes = 300)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ResultatSlicing resultat;
                try
                {
                    resultat = ExecuterSlicing(fichier3mfEntree, timeoutSecondes);
                }
                catch (Exception ex)
                {
                    resultat = new ResultatSlicing
                    {
                        Succes = false,
                        MessageErreur = $"Erreur inattendue : {ex.Message}"
                    };
                }
                callback?.Invoke(resultat);
            });
        }

        /// <summary>
        /// Exécution synchrone du slicing (appelé dans le thread de travail)
        /// </summary>
        private static ResultatSlicing ExecuterSlicing(string fichier3mfEntree, int timeoutSecondes)
        {
            string cheminSlicer = ObtenirCheminSlicer();
            if (string.IsNullOrEmpty(cheminSlicer))
            {
                return new ResultatSlicing
                {
                    Succes = false,
                    MessageErreur = "Bambu Studio non trouvé"
                };
            }

            // Créer un répertoire temporaire
            string dossierTemp = Path.Combine(Path.GetTempPath(), "LogicielImpression3D_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dossierTemp);

            string fichierSortie = Path.Combine(dossierTemp, "sortie.3mf");

            try
            {
                // Lancer Bambu Studio en mode CLI
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = cheminSlicer,
                    Arguments = $"--slice 0 --outputdir \"{dossierTemp}\" --export-3mf \"sortie.3mf\" \"{fichier3mfEntree}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(cheminSlicer)
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    string sortieStandard = "";
                    string sortieErreur = "";

                    process.OutputDataReceived += (s, e) => { if (e.Data != null) sortieStandard += e.Data + "\n"; };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) sortieErreur += e.Data + "\n"; };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool termine = process.WaitForExit(timeoutSecondes * 1000);

                    if (!termine)
                    {
                        try { process.Kill(); } catch (Exception ex) { LogManager.Erreur("Kill process slicer", ex); }
                        return new ResultatSlicing
                        {
                            Succes = false,
                            MessageErreur = $"Timeout : le slicing a dépassé {timeoutSecondes} secondes"
                        };
                    }

                    // Vérifier si le fichier de sortie existe
                    if (!File.Exists(fichierSortie))
                    {
                        return new ResultatSlicing
                        {
                            Succes = false,
                            MessageErreur = $"Le slicer n'a pas généré de fichier de sortie.\n{sortieErreur}"
                        };
                    }

                    // Parser le 3MF de sortie
                    return ParserResultatSlicing(fichierSortie);
                }
            }
            finally
            {
                // Nettoyer le dossier temporaire
                try
                {
                    if (Directory.Exists(dossierTemp))
                        Directory.Delete(dossierTemp, true);
                }
                catch (Exception ex) { LogManager.Erreur("Nettoyage dossier temp slicer", ex); }
            }
        }

        // ═══════════════════════════════════════════════════════
        // PARSING DU 3MF SLICÉ
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Parse le fichier 3MF de sortie pour extraire temps/poids/purge
        /// </summary>
        private static ResultatSlicing ParserResultatSlicing(string fichier3mfSortie)
        {
            ResultatSlicing resultat = new ResultatSlicing();

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(fichier3mfSortie))
                {
                    // 1. Chercher slice_info.config (XML)
                    var sliceInfo = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals("Metadata/slice_info.config", StringComparison.OrdinalIgnoreCase));

                    if (sliceInfo != null)
                    {
                        ParserSliceInfoConfig(sliceInfo, resultat);
                    }

                    // 2. Chercher les fichiers G-code pour le temps et la purge
                    var gCodeEntries = archive.Entries.Where(e =>
                        e.FullName.StartsWith("Metadata/plate_", StringComparison.OrdinalIgnoreCase) &&
                        e.FullName.EndsWith(".gcode", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    decimal tempsTotalMinutes = 0;
                    decimal purgeTotal = 0;

                    foreach (var gcode in gCodeEntries)
                    {
                        var tempsEtPurge = ParserGcodeCommentaires(gcode);
                        tempsTotalMinutes += tempsEtPurge.Item1;
                        purgeTotal += tempsEtPurge.Item2;
                    }

                    if (tempsTotalMinutes > 0)
                        resultat.TempsMinutes = tempsTotalMinutes;

                    if (purgeTotal > 0)
                        resultat.PoidsPurgeGrammes = purgeTotal;

                    resultat.Succes = resultat.PoidsFilamentGrammes > 0 || resultat.TempsMinutes > 0;

                    if (!resultat.Succes)
                        resultat.MessageErreur = "Aucune donnée de slicing trouvée dans le fichier de sortie";
                }
            }
            catch (Exception ex)
            {
                resultat.Succes = false;
                resultat.MessageErreur = $"Erreur lors du parsing : {ex.Message}";
            }

            return resultat;
        }

        /// <summary>
        /// Parse le fichier XML slice_info.config pour extraire poids et longueur filament
        /// </summary>
        private static void ParserSliceInfoConfig(ZipArchiveEntry entry, ResultatSlicing resultat)
        {
            try
            {
                using (Stream stream = entry.Open())
                {
                    XDocument doc = XDocument.Load(stream);

                    // Chercher les metadata (prediction = temps en secondes, weight = poids en grammes)
                    var metadatas = doc.Descendants("metadata");
                    foreach (var meta in metadatas)
                    {
                        string key = meta.Attribute("key")?.Value;
                        string value = meta.Attribute("value")?.Value;
                        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

                        if (key == "prediction")
                        {
                            decimal secondes;
                            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out secondes))
                                resultat.TempsMinutes += secondes / 60m;
                        }
                        else if (key == "weight")
                        {
                            decimal poids;
                            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out poids))
                                resultat.PoidsFilamentGrammes += poids;
                        }
                    }

                    // Chercher aussi les éléments filament pour longueur et poids détaillés
                    var filaments = doc.Descendants("filament");
                    decimal longueurTotale = 0;

                    foreach (var filament in filaments)
                    {
                        string usedM = filament.Attribute("used_m")?.Value;

                        if (!string.IsNullOrEmpty(usedM))
                        {
                            decimal longueur;
                            if (decimal.TryParse(usedM, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out longueur))
                                longueurTotale += longueur;
                        }
                    }

                    if (longueurTotale > 0)
                        resultat.FilamentUtiliseMetres = longueurTotale;
                }
            }
            catch (Exception ex) { LogManager.Erreur("Parsing slice_info.config", ex); }
        }

        /// <summary>
        /// Parse les commentaires G-code pour extraire temps et purge
        /// Retourne (tempsMinutes, purgeGrammes)
        /// </summary>
        private static Tuple<decimal, decimal> ParserGcodeCommentaires(ZipArchiveEntry entry)
        {
            decimal tempsMinutes = 0;
            decimal purgeGrammes = 0;

            try
            {
                using (Stream stream = entry.Open())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string ligne;
                    string contenuComplet = reader.ReadToEnd();
                    string[] lignes = contenuComplet.Split(new[] { '\n' }, StringSplitOptions.None);

                    // Parcourir les 300 premières et 300 dernières lignes
                    for (int i = 0; i < lignes.Length; i++)
                    {
                        if (i > 300 && i < lignes.Length - 300)
                            continue;

                        ligne = lignes[i].Trim();

                        if (!ligne.StartsWith(";"))
                            continue;

                        // Temps d'impression : "; model printing time: 1h 16m 56s"
                        if (ligne.Contains("model printing time:") || ligne.Contains("total estimated time:"))
                        {
                            decimal temps = ParserTempsImpression(ligne);
                            if (temps > tempsMinutes)
                                tempsMinutes = temps;
                        }

                        // Poids total filament : "; total filament used [g] = 24.5"
                        if (ligne.Contains("total filament used") && ligne.Contains("[g]"))
                        {
                            Match m = Regex.Match(ligne, @"=\s*([\d.]+)");
                            if (m.Success)
                            {
                                decimal poids;
                                if (decimal.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out poids))
                                {
                                    // Ne pas écraser la valeur de slice_info.config si déjà remplie
                                }
                            }
                        }

                        // Purge : "; filament_purge [g] = 3.2" ou "; total flush [g] ="
                        if ((ligne.Contains("filament_purge") || ligne.Contains("total flush")) && ligne.Contains("[g]"))
                        {
                            Match m = Regex.Match(ligne, @"=\s*([\d.]+)");
                            if (m.Success)
                            {
                                decimal purge;
                                if (decimal.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out purge))
                                    purgeGrammes += purge;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { LogManager.Erreur("Parsing commentaires G-code", ex); }

            return Tuple.Create(tempsMinutes, purgeGrammes);
        }

        /// <summary>
        /// Parse le temps depuis une chaîne comme "1h 16m 56s" ou "76m 56s" en minutes
        /// </summary>
        private static decimal ParserTempsImpression(string ligne)
        {
            decimal totalMinutes = 0;

            // Extraire les heures
            Match mHeures = Regex.Match(ligne, @"(\d+)\s*h");
            if (mHeures.Success)
                totalMinutes += int.Parse(mHeures.Groups[1].Value) * 60;

            // Extraire les minutes
            Match mMinutes = Regex.Match(ligne, @"(\d+)\s*m(?!s)");
            if (mMinutes.Success)
                totalMinutes += int.Parse(mMinutes.Groups[1].Value);

            // Extraire les secondes
            Match mSecondes = Regex.Match(ligne, @"(\d+)\s*s");
            if (mSecondes.Success)
                totalMinutes += int.Parse(mSecondes.Groups[1].Value) / 60m;

            return totalMinutes;
        }
    }
}
