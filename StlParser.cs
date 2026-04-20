using System;
using System.IO;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Résultat du parsing d'un fichier STL
    /// </summary>
    public class ResultatStl
    {
        public bool Succes { get; set; }
        public string MessageErreur { get; set; } = "";
        public string NomFichier { get; set; }
        public int NombreTriangles { get; set; }

        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public float MinZ { get; set; }
        public float MaxZ { get; set; }

        public decimal SizeX => (decimal)(MaxX - MinX);
        public decimal SizeY => (decimal)(MaxY - MinY);
        public decimal SizeZ => (decimal)(MaxZ - MinZ);

        // Estimation poids/temps — mêmes formules que 3MF (triangles × 3 ≈ vertex refs)
        public decimal PoidsEstimeGrammes =>
            CalibrationManager.EstimerPoids(NombreTriangles * 3, "PLA", 15);
        public decimal TempsEstimeMinutes =>
            CalibrationManager.EstimerTemps(NombreTriangles * 3, "");
    }

    /// <summary>
    /// Parseur de fichiers STL (binaire et ASCII)
    /// </summary>
    public static class StlParser
    {
        /// <summary>
        /// Parse un fichier STL et retourne les informations détectées
        /// </summary>
        public static ResultatStl Parser(string cheminFichier)
        {
            var resultat = new ResultatStl
            {
                NomFichier = Path.GetFileName(cheminFichier)
            };

            try
            {
                if (EstBinaire(cheminFichier))
                    ParserBinaire(cheminFichier, resultat);
                else
                    ParserAscii(cheminFichier, resultat);

                resultat.Succes = resultat.NombreTriangles > 0;
                if (!resultat.Succes)
                    resultat.MessageErreur = "Aucun triangle trouvé dans le fichier STL.";
            }
            catch (Exception ex)
            {
                resultat.Succes = false;
                resultat.MessageErreur = $"Erreur lecture STL : {ex.Message}";
            }

            return resultat;
        }

        // ═══════════════════════════════════════════════════════
        // DÉTECTION DU FORMAT
        // ═══════════════════════════════════════════════════════

        private static bool EstBinaire(string chemin)
        {
            // Un STL ASCII commence par "solid" — un STL binaire a 80 octets d'en-tête
            // Vérification double : en-tête + taille attendue
            long tailleReelle = new FileInfo(chemin).Length;
            if (tailleReelle < 84) return false;

            using (var fs = File.OpenRead(chemin))
            {
                byte[] entete = new byte[80];
                fs.Read(entete, 0, 80);
                string debut = System.Text.Encoding.ASCII.GetString(entete).Trim().ToLower();

                // Si le fichier ne commence pas par "solid", c'est forcément binaire
                if (!debut.StartsWith("solid"))
                    return true;

                // Si ça commence par "solid", vérifier si la taille correspond à du binaire
                byte[] countBytes = new byte[4];
                fs.Read(countBytes, 0, 4);
                uint count = BitConverter.ToUInt32(countBytes, 0);
                long tailleAttendue = 84L + (long)count * 50L;
                return tailleReelle == tailleAttendue;
            }
        }

        // ═══════════════════════════════════════════════════════
        // PARSING BINAIRE
        // ═══════════════════════════════════════════════════════

        private static void ParserBinaire(string chemin, ResultatStl resultat)
        {
            using (var reader = new BinaryReader(File.OpenRead(chemin)))
            {
                reader.ReadBytes(80); // en-tête (80 octets)
                uint count = reader.ReadUInt32();
                resultat.NombreTriangles = (int)count;

                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;

                for (uint i = 0; i < count; i++)
                {
                    reader.ReadBytes(12); // normale (3 × float)

                    for (int v = 0; v < 3; v++)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();

                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                    }

                    reader.ReadUInt16(); // attribut
                }

                resultat.MinX = minX; resultat.MaxX = maxX;
                resultat.MinY = minY; resultat.MaxY = maxY;
                resultat.MinZ = minZ; resultat.MaxZ = maxZ;
            }
        }

        // ═══════════════════════════════════════════════════════
        // PARSING ASCII
        // ═══════════════════════════════════════════════════════

        private static void ParserAscii(string chemin, ResultatStl resultat)
        {
            int vertexCount = 0;
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            using (var reader = new StreamReader(chemin))
            {
                string ligne;
                while ((ligne = reader.ReadLine()) != null)
                {
                    ligne = ligne.Trim();
                    if (!ligne.StartsWith("vertex ", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string[] parts = ligne.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) continue;

                    float x, y, z;
                    if (float.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out x) &&
                        float.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out y) &&
                        float.TryParse(parts[3], System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out z))
                    {
                        vertexCount++;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                    }
                }
            }

            resultat.NombreTriangles = vertexCount / 3;
            resultat.MinX = minX; resultat.MaxX = maxX;
            resultat.MinY = minY; resultat.MaxY = maxY;
            resultat.MinZ = minZ; resultat.MaxZ = maxZ;
        }

        /// <summary>
        /// Génère un rapport texte pour un fichier STL analysé
        /// </summary>
        public static string GenererRapport(ResultatStl r)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("══════════════════════════════════════════════════════");
            sb.AppendLine("       RAPPORT D'ANALYSE FICHIER STL                  ");
            sb.AppendLine("══════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"  Fichier : {r.NomFichier}");
            sb.AppendLine($"  Triangles : {r.NombreTriangles:N0}");
            sb.AppendLine();
            sb.AppendLine("──────────────────────────────────────────────────────");
            sb.AppendLine("  DIMENSIONS (mm)");
            sb.AppendLine("──────────────────────────────────────────────────────");
            sb.AppendLine($"  Largeur  (X) : {r.SizeX:F2} mm");
            sb.AppendLine($"  Profondeur (Y) : {r.SizeY:F2} mm");
            sb.AppendLine($"  Hauteur  (Z) : {r.SizeZ:F2} mm");
            sb.AppendLine();
            sb.AppendLine("──────────────────────────────────────────────────────");
            sb.AppendLine("  ESTIMATIONS (formules calibrées)");
            sb.AppendLine("──────────────────────────────────────────────────────");
            sb.AppendLine($"  Poids filament estimé : {r.PoidsEstimeGrammes:F2} g");
            decimal heures = r.TempsEstimeMinutes / 60m;
            sb.AppendLine($"  Temps estimé : {(int)heures}h {(int)(r.TempsEstimeMinutes % 60):D2}min");
            sb.AppendLine();
            sb.AppendLine("  ⚠ Ces estimations sont approximatives.");
            sb.AppendLine("    Pour des valeurs précises, importez le G-code");
            sb.AppendLine("    exporté depuis OrcaSlicer.");
            sb.AppendLine("══════════════════════════════════════════════════════");
            return sb.ToString();
        }
    }
}
