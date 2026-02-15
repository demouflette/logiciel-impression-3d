using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO.Compression;

namespace logiciel_d_impression_3d
{
    /// <summary>
    /// Parser pour les fichiers 3MF (3D Manufacturing Format)
    /// Les fichiers 3MF sont des archives ZIP contenant du XML
    /// </summary>
    public class ThreeMFParser
    {
        public class Object3D
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal SizeX { get; set; }
            public decimal SizeY { get; set; }
            public decimal SizeZ { get; set; }
            public decimal Volume { get; set; }
            public List<string> Colors { get; set; } = new List<string>();
            public List<Vector3D> Vertices { get; set; } = new List<Vector3D>();
        }

        public class Vector3D
        {
            public decimal X { get; set; }
            public decimal Y { get; set; }
            public decimal Z { get; set; }
        }

        public class ThreeMFFile
        {
            public string FileName { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<Object3D> Objects { get; set; } = new List<Object3D>();
            public decimal TotalVolume { get; set; }
            public int TotalVertices { get; set; }
            public string Unit { get; set; } = "mm";
            public string Producer { get; set; }
            
            // Métadonnées d'impression Bambu Lab
            public PrintMetadata PrintInfo { get; set; } = new PrintMetadata();
        }
        
        public class PrintMetadata
        {
            public string BedType { get; set; }
            public double NozzleDiameter { get; set; }
            public double LayerHeight { get; set; }
            public int PlateCount { get; set; }
            public List<string> FilamentColors { get; set; } = new List<string>();
            public List<string> FilamentIds { get; set; } = new List<string>();
            public double FirstLayerTime { get; set; }
            
            // Stats d'impression (si disponibles après slicing)
            public double PrintTime { get; set; } // en secondes
            public double FilamentUsed { get; set; } // en mm
            public double FilamentWeight { get; set; } // en grammes
            public int LayerCount { get; set; }
            public List<ObjectInfo> Objects { get; set; } = new List<ObjectInfo>();
        }
        
        public class ObjectInfo
        {
            public string Name { get; set; }
            public double Area { get; set; }
            public double LayerHeight { get; set; }
        }

        /// <summary>
        /// Parse un fichier 3MF et retourne les informations d�tect�es
        /// </summary>
        public static ThreeMFFile ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Le fichier {filePath} n'existe pas.");

            if (!filePath.EndsWith(".3mf", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Le fichier doit �tre un fichier 3MF (.3mf).");

            var result = new ThreeMFFile
            {
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    // Chercher le fichier 3D model principal
                    // Bambu Lab utilise 3D/3dmodel.model
                    // D'autres slicers utilisent 3D/model.xml
                    var modelEntry = archive.Entries.FirstOrDefault(e => 
                        (e.Name.Equals("3dmodel.model", StringComparison.OrdinalIgnoreCase) || 
                         e.Name.Equals("model.xml", StringComparison.OrdinalIgnoreCase)) && 
                        e.FullName.Contains("3D"));

                    if (modelEntry == null)
                        modelEntry = archive.Entries.FirstOrDefault(e => 
                            e.Name.Equals("3dmodel.model", StringComparison.OrdinalIgnoreCase) ||
                            e.Name.Equals("model.xml", StringComparison.OrdinalIgnoreCase));

                    if (modelEntry != null)
                    {
                        using (var stream = modelEntry.Open())
                        {
                            var doc = XDocument.Load(stream);
                            ParseModelXml(doc, result, archive);
                        }
                    }

                    // Chercher le fichier de relations pour les m�tadonn�es
                    var rels = archive.Entries.FirstOrDefault(e => 
                        e.FullName.Contains("_rels") && e.Name.EndsWith(".rels"));

                    // Chercher les m�tadonn�es
                    var contentTypes = archive.Entries.FirstOrDefault(e => 
                        e.Name.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase));
                    
                    // Extraire les m�tadonn�es d'impression Bambu Lab
                    ExtractBambuMetadata(archive, result);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors du parsing du fichier 3MF: {ex.Message}", ex);
            }

            return result;
        }

        private static void ParseModelXml(XDocument doc, ThreeMFFile result, ZipArchive archive)
        {
            // Namespace 3MF
            XNamespace ns3mf = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
            XNamespace nsCustom = "http://schemas.microsoft.com/3dmanufacturing/material/2015/02";
            XNamespace nsProduction = "http://schemas.microsoft.com/3dmanufacturing/production/2015/06";

            var root = doc.Root;
            if (root == null)
                return;

            // R�cup�rer l'unit�
            var unitAttr = root.Attribute("unit");
            if (unitAttr != null)
                result.Unit = unitAttr.Value;

            // Parser les objets 3D
            var resources = root.Element(ns3mf + "resources");
            if (resources != null)
            {
                // Parser les objets
                foreach (var obj in resources.Elements(ns3mf + "object"))
                {
                    var object3d = ParseObject(obj, ns3mf, nsProduction, archive);
                    if (object3d != null)
                    {
                        result.Objects.Add(object3d);
                        result.TotalVolume += object3d.Volume;
                        result.TotalVertices += object3d.Vertices.Count;
                    }
                }
            }

            // Parser la sc�ne (objets visibles) pour compter les objets imprimables
            var build = root.Element(ns3mf + "build");
            if (build != null)
            {
                int itemCount = build.Elements(ns3mf + "item").Count();
                // Si on a plus d'items dans build que d'objets, utiliser ce compte
                if (itemCount > result.Objects.Count)
                {
                    // Compl�ter avec des objets factices si n�cessaire
                    for (int i = result.Objects.Count; i < itemCount; i++)
                    {
                        result.Objects.Add(new Object3D { Id = i + 1, Name = $"Objet {i + 1}" });
                    }
                }
            }
        }

        private static Object3D ParseObject(XElement objElement, XNamespace ns, XNamespace nsProduction, ZipArchive archive)
        {
            var object3d = new Object3D();

            // R�cup�rer l'ID
            var idAttr = objElement.Attribute("id");
            if (idAttr != null && int.TryParse(idAttr.Value, out int id))
                object3d.Id = id;

            // R�cup�rer le nom
            var nameAttr = objElement.Attribute("name");
            if (nameAttr != null)
                object3d.Name = nameAttr.Value;
            else
                object3d.Name = $"Objet {object3d.Id}";

            // V�rifier si cet objet a des composants (Bambu Lab)
            var components = objElement.Element(ns + "components");
            if (components != null)
            {
                // Cet objet r�f�rence d'autres fichiers .model
                foreach (var component in components.Elements(ns + "component"))
                {
                    var pathAttr = component.Attribute(nsProduction + "path");
                    if (pathAttr != null)
                    {
                        // Charger le fichier .model r�f�renc�
                        string componentPath = pathAttr.Value.TrimStart('/');
                        var componentEntry = archive.Entries.FirstOrDefault(e => 
                            e.FullName.Replace("\\", "/") == componentPath);
                        
                        if (componentEntry != null)
                        {
                            using (var stream = componentEntry.Open())
                            {
                                var componentDoc = XDocument.Load(stream);
                                var componentRoot = componentDoc.Root;
                                if (componentRoot != null)
                                {
                                    var componentResources = componentRoot.Element(ns + "resources");
                                    if (componentResources != null)
                                    {
                                        // Parser le mesh du composant
                                        var componentObj = componentResources.Elements(ns + "object").FirstOrDefault();
                                        if (componentObj != null)
                                        {
                                            ParseMeshGeometry(componentObj, ns, object3d);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // IMPORTANT: Recalculer les dimensions après avoir chargé tous les composants
                if (object3d.Vertices.Count > 0)
                {
                    CalculateDimensions(object3d);
                }
                
                return object3d.Vertices.Count > 0 ? object3d : null;
            }

            // Parser la géométrie directe (format standard)
            ParseMeshGeometry(objElement, ns, object3d);

            return object3d.Vertices.Count > 0 ? object3d : null;
        }

        private static void CalculateDimensions(Object3D object3d)
        {
            if (object3d.Vertices.Count == 0)
                return;

            var minX = object3d.Vertices.Min(v => v.X);
            var maxX = object3d.Vertices.Max(v => v.X);
            var minY = object3d.Vertices.Min(v => v.Y);
            var maxY = object3d.Vertices.Max(v => v.Y);
            var minZ = object3d.Vertices.Min(v => v.Z);
            var maxZ = object3d.Vertices.Max(v => v.Z);

            object3d.SizeX = maxX - minX;
            object3d.SizeY = maxY - minY;
            object3d.SizeZ = maxZ - minZ;

            // Estimer le volume (approximation bounding box)
            object3d.Volume = object3d.SizeX * object3d.SizeY * object3d.SizeZ;
        }

        private static void ParseMeshGeometry(XElement objElement, XNamespace ns, Object3D object3d)
        {
            var mesh = objElement.Element(ns + "mesh");
            if (mesh != null)
            {
                // Parser les vertices
                var vertices = mesh.Element(ns + "vertices");
                if (vertices != null)
                {
                    foreach (var vertex in vertices.Elements(ns + "vertex"))
                    {
                        var v = new Vector3D();
                        
                        if (decimal.TryParse(vertex.Attribute("x")?.Value ?? "0", 
                            System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, out decimal x))
                            v.X = x;
                        if (decimal.TryParse(vertex.Attribute("y")?.Value ?? "0", 
                            System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, out decimal y))
                            v.Y = y;
                        if (decimal.TryParse(vertex.Attribute("z")?.Value ?? "0", 
                            System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, out decimal z))
                            v.Z = z;

                        object3d.Vertices.Add(v);
                    }
                }

                // Calculer les dimensions et le volume
                CalculateDimensions(object3d);

                // Parser les triangles pour r�cup�rer les couleurs
                var triangles = mesh.Element(ns + "triangles");
                if (triangles != null)
                {
                    foreach (var triangle in triangles.Elements(ns + "triangle"))
                    {
                        var pidAttr = triangle.Attribute("pid");
                        if (pidAttr != null && !object3d.Colors.Contains(pidAttr.Value))
                        {
                            object3d.Colors.Add(pidAttr.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// G�n�re un rapport texte des informations du fichier 3MF
        /// </summary>
        public static string GenerateReport(ThreeMFFile file)
        {
            var report = new System.Text.StringBuilder();

            report.AppendLine("??????????????????????????????????????????????????????????????");
            report.AppendLine("?        RAPPORT D'ANALYSE FICHIER 3MF                       ?");
            report.AppendLine("??????????????????????????????????????????????????????????????");
            report.AppendLine();

            report.AppendLine($"?? Fichier: {file.FileName}");
            report.AppendLine($"?? Unit�: {file.Unit}");
            report.AppendLine();

            report.AppendLine("?????????????????????????????????????????????????????????????");
            report.AppendLine($"?? Nombre d'objets d�tect�s: {file.Objects.Count}");
            report.AppendLine("?????????????????????????????????????????????????????????????");
            report.AppendLine();

            if (file.Objects.Count == 0)
            {
                report.AppendLine("??  Aucun objet trouv� dans le fichier 3MF");
                return report.ToString();
            }

            for (int i = 0; i < file.Objects.Count; i++)
            {
                var obj = file.Objects[i];
                report.AppendLine($"?? Objet {i + 1}: {obj.Name}");
                report.AppendLine($"   ?? ID: {obj.Id}");
                report.AppendLine($"   ?? Dimensions:");
                report.AppendLine($"   ?  ?? Largeur (X):  {obj.SizeX:F2} {file.Unit}");
                report.AppendLine($"   ?  ?? Profondeur (Y): {obj.SizeY:F2} {file.Unit}");
                report.AppendLine($"   ?  ?? Hauteur (Z):  {obj.SizeZ:F2} {file.Unit}");
                report.AppendLine($"   ?? Volume estim�: {obj.Volume:F2} {file.Unit}�");
                report.AppendLine($"   ?? Nombre de vertices: {obj.Vertices.Count}");
                
                if (obj.Colors.Count > 0)
                {
                    report.AppendLine($"   ?? Mat�riaux/Couleurs ({obj.Colors.Count}):");
                    foreach (var color in obj.Colors)
                    {
                        report.AppendLine($"   ?  ?? {color}");
                    }
                }
                else
                {
                    report.AppendLine($"   ?? Mat�riau: Non sp�cifi�");
                }

                report.AppendLine();
            }

            report.AppendLine("?????????????????????????????????????????????????????????????");
            report.AppendLine($"?? R�SUM� GLOBAL:");
            report.AppendLine($"   ?? Volume total estim�: {file.TotalVolume:F2} {file.Unit}�");
            report.AppendLine($"   ?? Poids estim� (PLA � 1.24g/cm�): {(file.TotalVolume / 1000m) * 1.24m:F2} g");
            
            // Afficher les m�tadonn�es d'impression si disponibles
            if (file.PrintInfo != null && (file.PrintInfo.PlateCount > 0 || file.PrintInfo.Objects.Count > 0))
            {
                report.AppendLine();
                report.AppendLine("?????????????????????????????????????????????????????????????");
                report.AppendLine("?? PARAM�TRES D'IMPRESSION D�TECT�S:");
                
                if (!string.IsNullOrEmpty(file.PrintInfo.BedType))
                    report.AppendLine($"   ?? Type de plateau: {file.PrintInfo.BedType}");
                    
                if (file.PrintInfo.NozzleDiameter > 0)
                    report.AppendLine($"   ?? Diam�tre buse: {file.PrintInfo.NozzleDiameter} mm");
                    
                if (file.PrintInfo.LayerHeight > 0)
                    report.AppendLine($"   ?? Hauteur de couche: {file.PrintInfo.LayerHeight:F3} mm");
                    
                if (file.PrintInfo.PlateCount > 0)
                    report.AppendLine($"   ?? Nombre de plateaux: {file.PrintInfo.PlateCount}");
                    
                if (file.PrintInfo.FilamentColors.Count > 0)
                {
                    report.AppendLine($"   ?? Couleurs de filament: {string.Join(", ", file.PrintInfo.FilamentColors)}");
                }
                
                if (file.PrintInfo.Objects.Count > 0)
                {
                    report.AppendLine($"   ?? Objets sur le plateau: {file.PrintInfo.Objects.Count}");
                    foreach (var obj in file.PrintInfo.Objects)
                    {
                        report.AppendLine($"      � {obj.Name} (aire: {obj.Area:F2} mm�)");
                    }
                }
                
                // Stats de slicing si disponibles
                if (file.PrintInfo.PrintTime > 0)
                {
                    int hours = (int)(file.PrintInfo.PrintTime / 3600);
                    int minutes = (int)((file.PrintInfo.PrintTime % 3600) / 60);
                    report.AppendLine($"   ?? Temps d'impression: {hours}h {minutes}min");
                }
                
                if (file.PrintInfo.FilamentUsed > 0)
                    report.AppendLine($"   ?? Filament utilis�: {file.PrintInfo.FilamentUsed / 1000:F2} m");
                    
                if (file.PrintInfo.FilamentWeight > 0)
                    report.AppendLine($"   ?? Poids filament: {file.PrintInfo.FilamentWeight:F2} g");
                    
                if (file.PrintInfo.LayerCount > 0)
                    report.AppendLine($"   ?? Nombre de couches: {file.PrintInfo.LayerCount}");
            }
            
            report.AppendLine("?????????????????????????????????????????????????????????????");

            return report.ToString();
        }
        
        /// <summary>
        /// Extrait les m�tadonn�es d'impression depuis les fichiers JSON Bambu Lab
        /// </summary>
        private static void ExtractBambuMetadata(ZipArchive archive, ThreeMFFile result)
        {
            try
            {
                int plateCount = 0;
                
                // Chercher tous les fichiers plate_X.json
                for (int i = 1; i <= 10; i++)
                {
                    var plateEntry = archive.Entries.FirstOrDefault(e => 
                        e.FullName.Equals($"Metadata/plate_{i}.json", StringComparison.OrdinalIgnoreCase));
                    
                    if (plateEntry != null)
                    {
                        plateCount++;
                        using (var stream = plateEntry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            string json = reader.ReadToEnd();
                            ParsePlateJson(json, result.PrintInfo);
                        }
                    }
                }
                
                result.PrintInfo.PlateCount = plateCount;
                
                // Extraire les statistiques du project_settings.config
                var projectSettingsEntry = archive.Entries.FirstOrDefault(e => 
                    e.FullName.Equals("Metadata/project_settings.config", StringComparison.OrdinalIgnoreCase));
                
                if (projectSettingsEntry != null)
                {
                    using (var stream = projectSettingsEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        ParseProjectSettings(jsonContent, result.PrintInfo);
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs de parsing JSON
            }
        }
        
        private static void ParseProjectSettings(string jsonContent, PrintMetadata metadata)
        {
            // Note: Les statistiques finales (temps, poids) ne sont PAS stockées dans le 3MF
            // Bambu Studio les calcule dynamiquement
            // Nous ne pouvons extraire que les paramètres d'impression (vitesses, températures, etc.)
            
            // Pour l'instant, nous ne faisons rien ici car ces valeurs doivent être calculées
            // à partir des objets et des paramètres
        }
        
        /// <summary>
        /// Parse un fichier plate JSON
        /// </summary>
        private static void ParsePlateJson(string json, PrintMetadata metadata)
        {
            try
            {
                // Parser simple sans d�pendance JSON
                if (json.Contains("\"bed_type\""))
                {
                    int bedTypeIndex = json.IndexOf("\"bed_type\":");
                    if (bedTypeIndex > 0)
                    {
                        int valueStart = json.IndexOf('"', bedTypeIndex + 11) + 1;
                        int valueEnd = json.IndexOf('"', valueStart);
                        if (valueEnd > valueStart)
                        {
                            metadata.BedType = json.Substring(valueStart, valueEnd - valueStart);
                        }
                    }
                }
                
                if (json.Contains("\"nozzle_diameter\""))
                {
                    int nozzleIndex = json.IndexOf("\"nozzle_diameter\":");
                    if (nozzleIndex > 0)
                    {
                        int valueStart = nozzleIndex + 18;
                        int valueEnd = json.IndexOfAny(new[] { ',', '}', '\n' }, valueStart);
                        if (valueEnd > valueStart)
                        {
                            string val = json.Substring(valueStart, valueEnd - valueStart).Trim();
                            if (double.TryParse(val, System.Globalization.NumberStyles.Any, 
                                System.Globalization.CultureInfo.InvariantCulture, out double nozzle))
                            {
                                metadata.NozzleDiameter = nozzle;
                            }
                        }
                    }
                }
                
                if (json.Contains("\"layer_height\""))
                {
                    // Chercher dans bbox_objects pour obtenir la hauteur de couche
                    int objIndex = json.IndexOf("\"bbox_objects\":");
                    if (objIndex > 0)
                    {
                        int layerIndex = json.IndexOf("\"layer_height\":", objIndex);
                        if (layerIndex > 0)
                        {
                            int valueStart = layerIndex + 16;
                            int valueEnd = json.IndexOfAny(new[] { ',', '}', '\n' }, valueStart);
                            if (valueEnd > valueStart)
                            {
                                string val = json.Substring(valueStart, valueEnd - valueStart).Trim();
                                if (double.TryParse(val, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double layer))
                                {
                                    metadata.LayerHeight = layer;
                                }
                            }
                        }
                    }
                }
                
                // Extraire les informations des objets
                if (json.Contains("\"bbox_objects\":"))
                {
                    int objIndex = json.IndexOf("\"bbox_objects\":");
                    if (objIndex > 0)
                    {
                        ParseObjectsFromJson(json.Substring(objIndex), metadata);
                    }
                }
                
                if (json.Contains("\"first_layer_time\""))
                {
                    int timeIndex = json.IndexOf("\"first_layer_time\":");
                    if (timeIndex > 0)
                    {
                        int valueStart = timeIndex + 19;
                        int valueEnd = json.IndexOfAny(new[] { ',', '}', '\n' }, valueStart);
                        if (valueEnd > valueStart)
                        {
                            string val = json.Substring(valueStart, valueEnd - valueStart).Trim();
                            if (double.TryParse(val, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out double time))
                            {
                                metadata.FirstLayerTime = time;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
        
        /// <summary>
        /// Extrait les informations des objets depuis le JSON
        /// </summary>
        private static void ParseObjectsFromJson(string json, PrintMetadata metadata)
        {
            try
            {
                int currentIndex = 0;
                while (true)
                {
                    int nameIndex = json.IndexOf("\"name\":", currentIndex);
                    if (nameIndex < 0) break;
                    
                    int nameStart = json.IndexOf('"', nameIndex + 7) + 1;
                    int nameEnd = json.IndexOf('"', nameStart);
                    
                    if (nameEnd > nameStart)
                    {
                        var objInfo = new ObjectInfo();
                        objInfo.Name = json.Substring(nameStart, nameEnd - nameStart);
                        
                        // Chercher l'aire en remontant avant le nom
                        int areaIndex = json.LastIndexOf("\"area\":", nameIndex);
                        if (areaIndex > currentIndex && areaIndex < nameIndex)
                        {
                            int areaStart = areaIndex + 7;
                            int areaEnd = json.IndexOfAny(new[] { ',', '}' }, areaStart);
                            if (areaEnd > areaStart)
                            {
                                string areaVal = json.Substring(areaStart, areaEnd - areaStart).Trim();
                                if (double.TryParse(areaVal, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double area))
                                {
                                    objInfo.Area = area;
                                }
                            }
                        }
                        
                        // Chercher layer_height après le nom
                        int layerIndex = json.IndexOf("\"layer_height\":", nameIndex);
                        if (layerIndex > nameIndex && layerIndex < nameIndex + 200)
                        {
                            int layerStart = layerIndex + 16;
                            int layerEnd = json.IndexOfAny(new[] { ',', '}' }, layerStart);
                            if (layerEnd > layerStart)
                            {
                                string layerVal = json.Substring(layerStart, layerEnd - layerStart).Trim();
                                if (double.TryParse(layerVal, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double layer))
                                {
                                    objInfo.LayerHeight = layer;
                                }
                            }
                        }
                        
                        metadata.Objects.Add(objInfo);
                    }
                    
                    currentIndex = nameEnd + 1;
                    if (currentIndex >= json.Length - 100) break;
                }
            }
            catch
            {
                // Ignorer les erreurs
            }
        }
    }
}
