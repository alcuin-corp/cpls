using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PLS.CommandBuilders.Agit
{
    public class AgitRepository : IAgitRepository
    {
        private readonly string _directory;

        private string At(params string[] path) => Path.Combine(_directory, Path.Combine(path));

        public string Head
        {
            get => File.ReadAllText(At("HEAD"));
            set => File.WriteAllText(At("HEAD"), value);
        }

        public string CurrentBranch
        {
            get => GetBranch(Head);
            set => SetBranch(Head, value);
        }

        public void CreateTag(string tagName, string hash)
        {
            File.WriteAllText(At($"tags/{tagName}"), hash);
        }

        public void Tag(string tagName) => CreateTag(tagName, CurrentBranch);

        public IEnumerable<string> Tags => Directory.EnumerateFiles(At("tags"));

        public void SetBranch(string branchName, string hash)
        {
            File.WriteAllText(At($"branches/{branchName}"), hash);
        }

        public string GetBranch(string branchName)
        {
            return File.ReadAllText(At($"branches/{branchName}"));
        }

        public static string EmptySha1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

        public JToken FileAsJToken(string sourceFile) => JToken.Parse(File.ReadAllText(sourceFile));

        private void WritePatchFile(JToken obj, string hash, bool isIndented = false)
        {
            using (var file = new StreamWriter(File.Open(At($"patches/{hash}.json"), FileMode.Create, FileAccess.Write)))
            {
                file.Write(obj.ToString(isIndented ? Formatting.Indented : Formatting.None));
            }
        }

        public void Commit(string sourceFile, string authorOpt = "", string messageOpt = "")
        {
            var obj = FileAsJToken(sourceFile);
            var newObj = JObject.FromObject(new
            {
                Metadata = CreateMetadata(obj, authorOpt, messageOpt, new[] {GetBranch(Head)}),
                Content = obj["Content"],
            });
            var hash = Hash(newObj);

            WritePatchFile(newObj, hash);

            CurrentBranch = hash;
        }

        public string Hash(JToken patchContent)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(patchContent.ToString()));
            var hashStr = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hashStr;
        }

        public JObject CreateMetadata(JToken obj, string authorOpt = "", string messageOpt = "", string[] parentsOpt = null)
        {
            var metadataPpty = obj["Metadata"];
            return JObject.FromObject(new
            {
                ExportedAt = DateTime.Parse(metadataPpty["Date"].ToString()),
                CreatedAt = DateTime.Now,
                Author = authorOpt,
                Message = messageOpt,
                Version = metadataPpty["Version"],
                Parents = parentsOpt ?? new[] { EmptySha1 },
            });
        }

        public void CreateBranch(string branchName, string hash)
        {
            File.WriteAllText(At($"branches/{branchName}"), hash);
        }

        public bool IsReady => File.Exists(At("HEAD"));

        public void Initialize()
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            else if (Directory.GetFileSystemEntries(_directory).Any())
            {
                throw new Exception("You can't initiate a repository if it's not empty.");
            }

            Directory.CreateDirectory(At("branches"));
            Directory.CreateDirectory(At("patches"));
            Directory.CreateDirectory(At("tags"));

            CreateBranch("master", EmptySha1);
            Head = "master";
        }

        public AgitRepository(string directory)
        {
            _directory = directory;
        }
    }
}