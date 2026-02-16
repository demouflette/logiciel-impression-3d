using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace logiciel_d_impression_3d
{
    public class DonneeCalibration
    {
        public string NomFichier { get; set; } = "";
        public long NombreVertices { get; set; }
        public decimal PoidsReel { get; set; }
        public decimal TempsReel { get; set; }
        public string Matiere { get; set; } = "PLA";
        public int PourcentageInfill { get; set; } = 20;
        public string Imprimante { get; set; } = "";
        public DateTime DateSaisie { get; set; } = DateTime.Now;
        public string Utilisateur { get; set; } = "";

        public decimal RatioPoids
        {
            get { return NombreVertices > 0 ? PoidsReel / NombreVertices : 0; }
        }

        public decimal RatioTemps
        {
            get { return NombreVertices > 0 ? TempsReel / NombreVertices : 0; }
        }

        public string ToLine()
        {
            return string.Join("|",
                NomFichier,
                NombreVertices.ToString(CultureInfo.InvariantCulture),
                PoidsReel.ToString(CultureInfo.InvariantCulture),
                TempsReel.ToString(CultureInfo.InvariantCulture),
                Matiere,
                PourcentageInfill.ToString(CultureInfo.InvariantCulture),
                Imprimante,
                DateSaisie.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Utilisateur);
        }

        public static DonneeCalibration FromLine(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length < 9) return null;

            try
            {
                return new DonneeCalibration
                {
                    NomFichier = parts[0],
                    NombreVertices = long.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out long v) ? v : 0,
                    PoidsReel = ParseDecimal(parts[2], 0),
                    TempsReel = ParseDecimal(parts[3], 0),
                    Matiere = parts[4],
                    PourcentageInfill = int.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out int inf) ? inf : 20,
                    Imprimante = parts[6],
                    DateSaisie = DateTime.TryParseExact(parts[7], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? dt : DateTime.Now,
                    Utilisateur = parts[8]
                };
            }
            catch
            {
                return null;
            }
        }

        private static decimal ParseDecimal(string value, decimal defaultValue)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : defaultValue;
        }
    }

    public class CalibrationManager
    {
        private const string FichierLocal = "calibration_donnees.dat";
        private const string UrlCalibration = "https://github.com/demouflette/logiciel-impression-3d-updates/raw/refs/heads/main/calibration_donnees.txt";
        private const string GithubOwner = "demouflette";
        private const string GithubRepo = "logiciel-impression-3d-updates";
        private const string FichierDistant = "calibration_donnees.txt";

        // Ratios par défaut (calibration initiale basée sur 2 impressions)
        private const decimal RatioPoidsDefaut = 0.0000395m;  // g/vertex
        private const decimal RatioTempsDefaut = 0.000124m;   // min/vertex

        private static List<DonneeCalibration> donneesLocales;
        private static List<DonneeCalibration> donneesDistantes;
        private static DateTime dernierTelechargement;

        /// <summary>
        /// Charge toutes les données de calibration (locales + distantes)
        /// </summary>
        public static List<DonneeCalibration> ChargerToutesDonnees()
        {
            if (donneesLocales == null)
            {
                donneesLocales = ChargerDonneesLocales();
            }

            if (donneesDistantes == null || (DateTime.Now - dernierTelechargement).TotalDays > 7)
            {
                donneesDistantes = TelechargerDonneesDistantes();
            }

            // Fusionner les données (dédoublonnage par NomFichier+Utilisateur)
            var toutes = new List<DonneeCalibration>(donneesLocales);
            foreach (var distante in donneesDistantes)
            {
                bool existe = toutes.Any(d =>
                    string.Equals(d.NomFichier, distante.NomFichier, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(d.Utilisateur, distante.Utilisateur, StringComparison.OrdinalIgnoreCase));

                if (!existe)
                {
                    toutes.Add(distante);
                }
            }

            return toutes;
        }

        /// <summary>
        /// Enregistre une nouvelle donnée de calibration localement
        /// </summary>
        public static void EnregistrerDonnee(DonneeCalibration donnee)
        {
            if (donneesLocales == null)
            {
                donneesLocales = ChargerDonneesLocales();
            }

            // Remplacer si même fichier + même utilisateur
            donneesLocales.RemoveAll(d =>
                string.Equals(d.NomFichier, donnee.NomFichier, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(d.Utilisateur, donnee.Utilisateur, StringComparison.OrdinalIgnoreCase));

            donneesLocales.Add(donnee);
            SauvegarderDonneesLocales(donneesLocales);
        }

        /// <summary>
        /// Calcule le ratio poids/vertex basé sur les données de calibration
        /// </summary>
        public static decimal ObtenirRatioPoids(string matiere, int pourcentageInfill)
        {
            var donnees = ChargerToutesDonnees();

            // Chercher des données pour cette matière et infill exact
            var correspondances = donnees.Where(d =>
                string.Equals(d.Matiere, matiere, StringComparison.OrdinalIgnoreCase) &&
                d.PourcentageInfill == pourcentageInfill &&
                d.NombreVertices > 0 && d.PoidsReel > 0).ToList();

            // Si pas de match exact, élargir à ±5% infill
            if (correspondances.Count == 0)
            {
                correspondances = donnees.Where(d =>
                    string.Equals(d.Matiere, matiere, StringComparison.OrdinalIgnoreCase) &&
                    Math.Abs(d.PourcentageInfill - pourcentageInfill) <= 5 &&
                    d.NombreVertices > 0 && d.PoidsReel > 0).ToList();
            }

            // Si toujours pas de match, chercher juste par matière
            if (correspondances.Count == 0)
            {
                correspondances = donnees.Where(d =>
                    string.Equals(d.Matiere, matiere, StringComparison.OrdinalIgnoreCase) &&
                    d.NombreVertices > 0 && d.PoidsReel > 0).ToList();
            }

            if (correspondances.Count > 0)
            {
                return correspondances.Average(d => d.RatioPoids);
            }

            return RatioPoidsDefaut;
        }

        /// <summary>
        /// Calcule le ratio temps/vertex basé sur les données de calibration
        /// </summary>
        public static decimal ObtenirRatioTemps(string imprimante)
        {
            var donnees = ChargerToutesDonnees();

            // Chercher des données pour cette imprimante
            var correspondances = donnees.Where(d =>
                string.Equals(d.Imprimante, imprimante, StringComparison.OrdinalIgnoreCase) &&
                d.NombreVertices > 0 && d.TempsReel > 0).ToList();

            // Si pas de match, utiliser toutes les données disponibles
            if (correspondances.Count == 0)
            {
                correspondances = donnees.Where(d =>
                    d.NombreVertices > 0 && d.TempsReel > 0).ToList();
            }

            if (correspondances.Count > 0)
            {
                return correspondances.Average(d => d.RatioTemps);
            }

            return RatioTempsDefaut;
        }

        /// <summary>
        /// Estime le poids basé sur les données de calibration
        /// </summary>
        public static decimal EstimerPoids(long nombreVertices, string matiere, int pourcentageInfill)
        {
            decimal ratio = ObtenirRatioPoids(matiere, pourcentageInfill);
            return nombreVertices * ratio;
        }

        /// <summary>
        /// Estime le temps basé sur les données de calibration et le coefficient de vitesse de l'imprimante
        /// </summary>
        public static decimal EstimerTemps(long nombreVertices, string imprimante)
        {
            decimal ratio = ObtenirRatioTemps(imprimante);
            decimal tempsBase = nombreVertices * ratio;

            // Appliquer le coefficient de vitesse de l'imprimante
            var specs = ImprimanteSpecsManager.ObtenirSpecs(imprimante);
            if (specs.CoefficientVitesse > 0)
            {
                tempsBase = tempsBase / specs.CoefficientVitesse;
            }

            return tempsBase;
        }

        /// <summary>
        /// Retourne le nombre de données de calibration disponibles
        /// </summary>
        public static int ObtenirNombreDonnees()
        {
            return ChargerToutesDonnees().Count;
        }

        /// <summary>
        /// Partage les données locales vers GitHub via l'API
        /// </summary>
        public static void PartagerDonnees(string tokenGithub)
        {
            if (string.IsNullOrWhiteSpace(tokenGithub))
            {
                throw new InvalidOperationException("Le token GitHub n'est pas configuré. Allez dans Paramètres pour le configurer.");
            }

            if (donneesLocales == null || donneesLocales.Count == 0)
            {
                throw new InvalidOperationException("Aucune donnée locale à partager.");
            }

            // 1. Lire le fichier existant depuis GitHub API
            string contenuExistant = "";
            string sha = "";

            try
            {
                string urlApi = $"https://api.github.com/repos/{GithubOwner}/{GithubRepo}/contents/{FichierDistant}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlApi);
                request.Method = "GET";
                request.UserAgent = "Logiciel-Impression-3D";
                request.Headers.Add("Authorization", $"token {tokenGithub}");
                request.Accept = "application/vnd.github.v3+json";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string json = reader.ReadToEnd();

                    // Extraire le sha pour le PUT
                    sha = ExtraireValeurJson(json, "sha");

                    // Extraire le contenu (base64)
                    string contenuBase64 = ExtraireValeurJson(json, "content");
                    if (!string.IsNullOrEmpty(contenuBase64))
                    {
                        contenuBase64 = contenuBase64.Replace("\n", "").Replace("\\n", "");
                        contenuExistant = Encoding.UTF8.GetString(Convert.FromBase64String(contenuBase64));
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse resp = ex.Response as HttpWebResponse;
                if (resp != null && resp.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException($"Erreur lors de la lecture du fichier distant : {ex.Message}");
                }
                // 404 = le fichier n'existe pas encore, on va le créer
            }

            // 2. Parser les données existantes distantes
            var donneesExistantes = new List<DonneeCalibration>();
            if (!string.IsNullOrEmpty(contenuExistant))
            {
                string[] lignes = contenuExistant.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ligne in lignes)
                {
                    var donnee = DonneeCalibration.FromLine(ligne);
                    if (donnee != null)
                    {
                        donneesExistantes.Add(donnee);
                    }
                }
            }

            // 3. Fusionner avec les données locales
            foreach (var locale in donneesLocales)
            {
                bool existe = donneesExistantes.Any(d =>
                    string.Equals(d.NomFichier, locale.NomFichier, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(d.Utilisateur, locale.Utilisateur, StringComparison.OrdinalIgnoreCase));

                if (!existe)
                {
                    donneesExistantes.Add(locale);
                }
            }

            // 4. Encoder le nouveau contenu
            StringBuilder sb = new StringBuilder();
            foreach (var donnee in donneesExistantes)
            {
                sb.AppendLine(donnee.ToLine());
            }
            string nouveauContenu = Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));

            // 5. PUT vers GitHub
            string urlPut = $"https://api.github.com/repos/{GithubOwner}/{GithubRepo}/contents/{FichierDistant}";
            HttpWebRequest putRequest = (HttpWebRequest)WebRequest.Create(urlPut);
            putRequest.Method = "PUT";
            putRequest.UserAgent = "Logiciel-Impression-3D";
            putRequest.Headers.Add("Authorization", $"token {tokenGithub}");
            putRequest.Accept = "application/vnd.github.v3+json";
            putRequest.ContentType = "application/json";

            string jsonBody = "{" +
                "\"message\":\"Mise à jour des données de calibration\"," +
                "\"content\":\"" + nouveauContenu + "\"" +
                (string.IsNullOrEmpty(sha) ? "" : ",\"sha\":\"" + sha + "\"") +
                "}";

            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            putRequest.ContentLength = bodyBytes.Length;
            using (Stream requestStream = putRequest.GetRequestStream())
            {
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            using (HttpWebResponse putResponse = (HttpWebResponse)putRequest.GetResponse())
            {
                if (putResponse.StatusCode != HttpStatusCode.OK && putResponse.StatusCode != HttpStatusCode.Created)
                {
                    throw new InvalidOperationException($"Erreur lors de l'upload : {putResponse.StatusCode}");
                }
            }
        }

        #region Méthodes privées

        private static List<DonneeCalibration> ChargerDonneesLocales()
        {
            var donnees = new List<DonneeCalibration>();
            try
            {
                if (File.Exists(FichierLocal))
                {
                    string[] lignes = File.ReadAllLines(FichierLocal, Encoding.UTF8);
                    foreach (string ligne in lignes)
                    {
                        var donnee = DonneeCalibration.FromLine(ligne);
                        if (donnee != null)
                        {
                            donnees.Add(donnee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement calibration locale : {ex.Message}");
            }
            return donnees;
        }

        private static void SauvegarderDonneesLocales(List<DonneeCalibration> donnees)
        {
            try
            {
                var lignes = donnees.Select(d => d.ToLine()).ToArray();
                File.WriteAllLines(FichierLocal, lignes, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde calibration : {ex.Message}");
            }
        }

        private static List<DonneeCalibration> TelechargerDonneesDistantes()
        {
            var donnees = new List<DonneeCalibration>();
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string contenu = client.DownloadString(UrlCalibration);
                    string[] lignes = contenu.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ligne in lignes)
                    {
                        var donnee = DonneeCalibration.FromLine(ligne);
                        if (donnee != null)
                        {
                            donnees.Add(donnee);
                        }
                    }
                    dernierTelechargement = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur téléchargement calibration distante : {ex.Message}");
                dernierTelechargement = DateTime.Now; // Éviter de retenter immédiatement
            }
            return donnees;
        }

        private static string ExtraireValeurJson(string json, string cle)
        {
            string recherche = "\"" + cle + "\":\"";
            int index = json.IndexOf(recherche);
            if (index < 0) return "";

            int debut = index + recherche.Length;
            int fin = json.IndexOf("\"", debut);
            if (fin < 0) return "";

            return json.Substring(debut, fin - debut);
        }

        #endregion
    }
}
