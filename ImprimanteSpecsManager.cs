using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace logiciel_d_impression_3d
{
    public class ImprimanteSpecsManager
    {
        private const string UrlApiSpecs = "https://github.com/demouflette/logiciel-impression-3d-updates/raw/refs/heads/main/imprimantes_specs.txt";
        private static readonly string FichierCacheLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imprimantes_specs_cache.dat");
        private static Dictionary<string, SpecsImprimante> specsCache;
        private static DateTime derniereMiseAJour;

        public static SpecsImprimante ObtenirSpecs(string nomImprimante)
        {
            // Charger depuis le cache local d'abord (instantané), puis rafraîchir en arrière-plan
            if (specsCache == null)
            {
                ChargerSpecsDepuisCacheLocal();
                LancerChargementAsynchrone();
            }
            else if ((DateTime.Now - derniereMiseAJour).TotalDays > 7)
            {
                LancerChargementAsynchrone();
            }

            if (specsCache != null && specsCache.ContainsKey(nomImprimante))
            {
                return specsCache[nomImprimante];
            }

            // Retourner des specs par défaut si non trouvé
            return ObtenirSpecsParDefaut(nomImprimante);
        }

        private static bool chargementEnCours = false;

        private static void LancerChargementAsynchrone()
        {
            if (chargementEnCours) return;
            chargementEnCours = true;

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ChargerSpecsDepuisInternet();
                }
                finally
                {
                    chargementEnCours = false;
                }
            });
        }

        private static void ChargerSpecsDepuisInternet()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string contenu = client.DownloadString(UrlApiSpecs);
                    specsCache = ParserSpecs(contenu);
                    derniereMiseAJour = DateTime.Now;
                    
                    // Sauvegarder en cache local
                    SauvegarderCacheLocal(contenu);
                    
                    System.Diagnostics.Debug.WriteLine("Specs chargées depuis Internet avec succès");
                }
            }
            catch (WebException)
            {
                System.Diagnostics.Debug.WriteLine("Impossible de charger les specs en ligne, utilisation du cache local");
                ChargerSpecsDepuisCacheLocal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement specs: {ex.Message}");
                specsCache = CreerSpecsParDefaut();
                derniereMiseAJour = DateTime.Now;
            }
        }

        private static void ChargerSpecsDepuisCacheLocal()
        {
            try
            {
                if (File.Exists(FichierCacheLocal))
                {
                    string contenu = File.ReadAllText(FichierCacheLocal, Encoding.UTF8);
                    specsCache = ParserSpecs(contenu);
                    derniereMiseAJour = File.GetLastWriteTime(FichierCacheLocal);
                }
                else
                {
                    specsCache = CreerSpecsParDefaut();
                    derniereMiseAJour = DateTime.Now;
                }
            }
            catch
            {
                specsCache = CreerSpecsParDefaut();
                derniereMiseAJour = DateTime.Now;
            }
        }

        private static void SauvegarderCacheLocal(string contenu)
        {
            try
            {
                File.WriteAllText(FichierCacheLocal, contenu, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde cache: {ex.Message}");
            }
        }

        private static Dictionary<string, SpecsImprimante> ParserSpecs(string contenu)
        {
            Dictionary<string, SpecsImprimante> specs = new Dictionary<string, SpecsImprimante>();
            
            string[] lines = contenu.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in lines)
            {
                // Format: Nom|PuissanceWatts|ConsommationMoyenne|Source|CoeffVitesse|CoeffDechet
                string[] parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    string nom = parts[0].Trim();
                    decimal puissance = ParseDecimal(parts[1], 250);
                    decimal consoMoyenne = parts.Length > 2 ? ParseDecimal(parts[2], puissance) : puissance;
                    string source = parts.Length > 3 ? parts[3] : "API en ligne";
                    decimal coeffVitesse = parts.Length > 4 ? ParseDecimal(parts[4], 1.0m) : 1.0m;
                    decimal coeffDechet = parts.Length > 5 ? ParseDecimal(parts[5], 1.0m) : 1.0m;

                    specs[nom] = new SpecsImprimante
                    {
                        Nom = nom,
                        PuissanceMaxWatts = puissance,
                        ConsommationMoyenneWatts = consoMoyenne,
                        SourceDonnees = source,
                        CoefficientVitesse = coeffVitesse,
                        CoefficientDechetAMS = coeffDechet,
                        DateMiseAJour = DateTime.Now
                    };
                }
            }
            
            return specs.Count > 0 ? specs : CreerSpecsParDefaut();
        }

        private static decimal ParseDecimal(string value, decimal defaultValue)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return defaultValue;
        }

        private static Dictionary<string, SpecsImprimante> CreerSpecsParDefaut()
        {
            string src = "Spécifications fabricant";
            return new Dictionary<string, SpecsImprimante>
            {
                // === Bambu Lab - Série X (Core XY haut de gamme) ===
                { "Bambu Lab X1 Carbon", new SpecsImprimante
                    { Nom = "Bambu Lab X1 Carbon", PuissanceMaxWatts = 350, ConsommationMoyenneWatts = 200,
                      CoefficientVitesse = 1.0m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab X1E", new SpecsImprimante
                    { Nom = "Bambu Lab X1E", PuissanceMaxWatts = 350, ConsommationMoyenneWatts = 200,
                      CoefficientVitesse = 1.0m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab X1", new SpecsImprimante
                    { Nom = "Bambu Lab X1", PuissanceMaxWatts = 350, ConsommationMoyenneWatts = 200,
                      CoefficientVitesse = 1.0m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },

                // === Bambu Lab - Série P (milieu de gamme) ===
                { "Bambu Lab P1P", new SpecsImprimante
                    { Nom = "Bambu Lab P1P", PuissanceMaxWatts = 300, ConsommationMoyenneWatts = 180,
                      CoefficientVitesse = 0.85m, CoefficientDechetAMS = 1.1m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab P1S", new SpecsImprimante
                    { Nom = "Bambu Lab P1S", PuissanceMaxWatts = 300, ConsommationMoyenneWatts = 180,
                      CoefficientVitesse = 0.85m, CoefficientDechetAMS = 1.1m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab P2S", new SpecsImprimante
                    { Nom = "Bambu Lab P2S", PuissanceMaxWatts = 300, ConsommationMoyenneWatts = 180,
                      CoefficientVitesse = 0.90m, CoefficientDechetAMS = 1.15m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },

                // === Bambu Lab - Série H (Core XY nouvelle gen, purge optimisée) ===
                { "Bambu Lab H2C", new SpecsImprimante
                    { Nom = "Bambu Lab H2C", PuissanceMaxWatts = 300, ConsommationMoyenneWatts = 170,
                      CoefficientVitesse = 1.1m, CoefficientDechetAMS = 0.85m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab H2D", new SpecsImprimante
                    { Nom = "Bambu Lab H2D", PuissanceMaxWatts = 300, ConsommationMoyenneWatts = 170,
                      CoefficientVitesse = 1.1m, CoefficientDechetAMS = 0.85m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },

                // === Bambu Lab - Série A (entrée de gamme) ===
                { "Bambu Lab A1", new SpecsImprimante
                    { Nom = "Bambu Lab A1", PuissanceMaxWatts = 250, ConsommationMoyenneWatts = 150,
                      CoefficientVitesse = 0.80m, CoefficientDechetAMS = 1.05m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab A1 Combo", new SpecsImprimante
                    { Nom = "Bambu Lab A1 Combo", PuissanceMaxWatts = 250, ConsommationMoyenneWatts = 150,
                      CoefficientVitesse = 0.80m, CoefficientDechetAMS = 1.05m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Bambu Lab A1 Mini", new SpecsImprimante
                    { Nom = "Bambu Lab A1 Mini", PuissanceMaxWatts = 200, ConsommationMoyenneWatts = 120,
                      CoefficientVitesse = 0.70m, CoefficientDechetAMS = 1.1m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },

                // === Autres marques ===
                { "Creality Ender 3", new SpecsImprimante
                    { Nom = "Creality Ender 3", PuissanceMaxWatts = 270, ConsommationMoyenneWatts = 150,
                      CoefficientVitesse = 0.50m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Prusa i3 MK3S+", new SpecsImprimante
                    { Nom = "Prusa i3 MK3S+", PuissanceMaxWatts = 240, ConsommationMoyenneWatts = 140,
                      CoefficientVitesse = 0.55m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } },
                { "Anycubic Kobra", new SpecsImprimante
                    { Nom = "Anycubic Kobra", PuissanceMaxWatts = 250, ConsommationMoyenneWatts = 145,
                      CoefficientVitesse = 0.50m, CoefficientDechetAMS = 1.0m, SourceDonnees = src, DateMiseAJour = DateTime.Now } }
            };
        }

        private static SpecsImprimante ObtenirSpecsParDefaut(string nomImprimante)
        {
            var specsDefaut = CreerSpecsParDefaut();
            if (specsDefaut.ContainsKey(nomImprimante))
            {
                return specsDefaut[nomImprimante];
            }
            
            return new SpecsImprimante 
            { 
                Nom = nomImprimante, 
                PuissanceMaxWatts = 250,
                ConsommationMoyenneWatts = 150,
                SourceDonnees = "Valeur estimée par défaut",
                DateMiseAJour = DateTime.Now
            };
        }

        public static List<string> ObtenirListeImprimantes()
        {
            if (specsCache == null)
            {
                ChargerSpecsDepuisCacheLocal();
                LancerChargementAsynchrone();
            }
            else if ((DateTime.Now - derniereMiseAJour).TotalDays > 7)
            {
                LancerChargementAsynchrone();
            }
            return specsCache != null ? specsCache.Keys.ToList() : CreerSpecsParDefaut().Keys.ToList();
        }

        public static void RafraichirSpecsDepuisInternet()
        {
            specsCache = null;
            ChargerSpecsDepuisInternet();
        }
    }

    public class SpecsImprimante
    {
        public string Nom { get; set; }
        public decimal PuissanceMaxWatts { get; set; }
        public decimal ConsommationMoyenneWatts { get; set; }
        public string SourceDonnees { get; set; } = "Local";
        public DateTime DateMiseAJour { get; set; } = DateTime.Now;

        /// <summary>
        /// Multiplicateur de vitesse (1.0 = référence X1 Carbon).
        /// Plus élevé = imprimante plus rapide → temps estimé plus court.
        /// </summary>
        public decimal CoefficientVitesse { get; set; } = 1.0m;

        /// <summary>
        /// Multiplicateur de déchet AMS (1.0 = référence X1 Carbon).
        /// Plus élevé = plus de purge. Plus bas = système de purge optimisé.
        /// </summary>
        public decimal CoefficientDechetAMS { get; set; } = 1.0m;

        public decimal PuissanceMaxKw
        {
            get { return PuissanceMaxWatts / 1000m; }
        }

        public decimal ConsommationMoyenneKw
        {
            get { return ConsommationMoyenneWatts / 1000m; }
        }

        public override string ToString()
        {
            return $"{Nom} - {ConsommationMoyenneWatts}W (max {PuissanceMaxWatts}W) - Source: {SourceDonnees}";
        }
    }
}
