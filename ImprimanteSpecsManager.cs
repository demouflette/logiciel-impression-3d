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
        private const string FichierCacheLocal = "imprimantes_specs_cache.dat";
        private static Dictionary<string, SpecsImprimante> specsCache;
        private static DateTime derniereMiseAJour;

        public static SpecsImprimante ObtenirSpecs(string nomImprimante)
        {
            // Charger depuis le cache si disponible et récent (moins de 7 jours)
            if (specsCache == null || (DateTime.Now - derniereMiseAJour).TotalDays > 7)
            {
                ChargerSpecsDepuisInternet();
            }

            if (specsCache != null && specsCache.ContainsKey(nomImprimante))
            {
                return specsCache[nomImprimante];
            }

            // Retourner des specs par défaut si non trouvé
            return ObtenirSpecsParDefaut(nomImprimante);
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
                // Format: Nom|PuissanceWatts|ConsommationMoyenne|Source
                string[] parts = line.Split('|');
                if (parts.Length >= 2)
                {
                    string nom = parts[0].Trim();
                    decimal puissance = ParseDecimal(parts[1], 250);
                    decimal consoMoyenne = parts.Length > 2 ? ParseDecimal(parts[2], puissance) : puissance;
                    string source = parts.Length > 3 ? parts[3] : "API en ligne";
                    
                    specs[nom] = new SpecsImprimante
                    {
                        Nom = nom,
                        PuissanceMaxWatts = puissance,
                        ConsommationMoyenneWatts = consoMoyenne,
                        SourceDonnees = source,
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
            return new Dictionary<string, SpecsImprimante>
            {
                { "Bambu Lab X1 Carbon", new SpecsImprimante 
                    { 
                        Nom = "Bambu Lab X1 Carbon", 
                        PuissanceMaxWatts = 350, 
                        ConsommationMoyenneWatts = 200,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Bambu Lab P1P", new SpecsImprimante 
                    { 
                        Nom = "Bambu Lab P1P", 
                        PuissanceMaxWatts = 300, 
                        ConsommationMoyenneWatts = 180,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Bambu Lab P2S", new SpecsImprimante 
                    { 
                        Nom = "Bambu Lab P2S", 
                        PuissanceMaxWatts = 300, 
                        ConsommationMoyenneWatts = 180,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Bambu Lab A1 Mini", new SpecsImprimante 
                    { 
                        Nom = "Bambu Lab A1 Mini", 
                        PuissanceMaxWatts = 200, 
                        ConsommationMoyenneWatts = 120,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Creality Ender 3", new SpecsImprimante 
                    { 
                        Nom = "Creality Ender 3", 
                        PuissanceMaxWatts = 270, 
                        ConsommationMoyenneWatts = 150,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Prusa i3 MK3S+", new SpecsImprimante 
                    { 
                        Nom = "Prusa i3 MK3S+", 
                        PuissanceMaxWatts = 240, 
                        ConsommationMoyenneWatts = 140,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                },
                { "Anycubic Kobra", new SpecsImprimante 
                    { 
                        Nom = "Anycubic Kobra", 
                        PuissanceMaxWatts = 250, 
                        ConsommationMoyenneWatts = 145,
                        SourceDonnees = "Spécifications fabricant",
                        DateMiseAJour = DateTime.Now
                    } 
                }
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
