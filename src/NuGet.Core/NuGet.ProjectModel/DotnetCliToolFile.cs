using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace NuGet.ProjectModel
{
    public class DotnetCliToolFile
    {
        /// <summary>
        /// File json.
        /// </summary>
        public JObject Json { get; }

        /// <summary>
        /// File version.
        /// </summary>
        public int FormatVersion { get; set; } = 1;

        /// <summary>
        /// True if all packages were restored.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Tool id.
        /// </summary>
        public string ToolId { get; set; }

        /// <summary>
        /// Resolved tool version.
        /// </summary>
        public NuGetVersion ToolVersion { get; set; }

        /// <summary>
        /// Requested dependency range.
        /// </summary>
        public VersionRange DependencyRange { get; set;}

        /// <summary>
        /// Package folder and package fallback folders.
        /// These are ordered by precedence.
        /// </summary>
        public IList<string> PackageFolders { get; set; } = new List<string>();

        /// <summary>
        /// Framework -> Lib folder path
        /// </summary>
        public IDictionary<NuGetFramework, string> DepsFiles { get; set; } = new Dictionary<NuGetFramework, string>();

        /// <summary>
        /// Restore errors and warnings.
        /// </summary>
        public IList<FileLogEntry> Log { get; set; } = new List<FileLogEntry>();

        public DotnetCliToolFile(JObject json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            ParseJson(json);

            Json = json;
        }

        public DotnetCliToolFile()
        {
            Json = new JObject();
        }

        public static DotnetCliToolFile Load(string path)
        {
            var json = ReadJson(path);

            return Load(json);
        }

        public static DotnetCliToolFile Load(JObject json)
        {
            return new DotnetCliToolFile(json);
        }

        public void Save(string path)
        {
            var json = GetJson(spec: this);

            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var textWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;
                json.WriteTo(jsonWriter);
            }
        }

        public static JObject GetJson(DotnetCliToolFile spec)
        {
            var json = new JObject();

            json.Add("formatVersion", spec.FormatVersion);
            json.Add("success", spec.Success);
            json.Add("toolId", spec.ToolId);
            json.Add("toolVersion", spec.ToolVersion.ToNormalizedString());
            json.Add("dependencyRange", spec.DependencyRange.ToNormalizedString());

            var targetsObj = new JObject();
            json.Add("depsFiles", targetsObj);

            foreach (var target in spec.DepsFiles)
            {
                targetsObj.Add(target.Key.ToString(), target.Value);
            }

            var packageFoldersObj = new JObject();
            json.Add("packageFolders", packageFoldersObj);

            foreach (var folder in spec.PackageFolders)
            {
                packageFoldersObj.Add(folder, new JObject());
            }

            var logArray = new JArray();
            json.Add("log", logArray);

            foreach (var entry in spec.Log)
            {
                var entryObj = new JObject();
                logArray.Add(entryObj);

                entryObj.Add("type", entry.Type.ToString().ToString().ToLowerInvariant());
                entryObj.Add("message", entry.Message);
            }

            return json;
        }

        private void ParseJson(JObject json)
        {
            FormatVersion = json.GetValue<int>("formatVersion");
            Success = json.GetValue<bool>("success");
            ToolId = json.GetValue<string>("toolId");
            ToolVersion = NuGetVersion.Parse(json.GetValue<string>("toolVersion"));
            DependencyRange = VersionRange.Parse(json.GetValue<string>("dependencyRange"));

            foreach (var prop in json.GetValue<JObject>("depsFiles").Properties())
            {
                var framework = NuGetFramework.Parse(prop.Name);

                if (!DepsFiles.ContainsKey(framework)
                    && framework.IsSpecificFramework)
                {
                    DepsFiles.Add(framework, prop.Value.ToObject<string>());
                }
            }

            foreach (var prop in json.GetValue<JObject>("packageFolders").Properties())
            {
                PackageFolders.Add(prop.Name);
            }

            foreach (var entry in json.GetValue<JArray>("log"))
            {
                FileLogEntryType entryType;
                var typeString = entry.GetValue<string>("type");
                Enum.TryParse<FileLogEntryType>(typeString, ignoreCase: true, result: out entryType);

                Log.Add(new FileLogEntry(entryType, entry.GetValue<string>("message")));
            }
        }

        private static JObject ReadJson(string packageSpecPath)
        {
            JObject json;

            using (var stream = new FileStream(packageSpecPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new JsonTextReader(new StreamReader(stream)))
            {
                try
                {
                    json = JObject.Load(reader);
                }
                catch (JsonReaderException ex)
                {
                    throw FileFormatException.Create(ex, packageSpecPath);
                }
            }

            return json;
        }
    }
}
