using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PLS.Agit;

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

        public string CurrentHash
        {
            get => GetBranchHash(Head);
            set => SetBranchHash(Head, value);
        }

        public void Tag(string name, string hash = null)
        {
            File.WriteAllText(At($"tags/{name}"), hash ?? CurrentHash);
        }

        public IEnumerable<string> Tags => Directory.EnumerateFiles(At("tags"));

        public void SetBranchHash(string branchName, string hash = null)
        {
            File.WriteAllText(At($"branches/{branchName}"), hash ?? CurrentHash);
        }

        public string GetBranchHash(string branchName)
        {
            return File.ReadAllText(At($"branches/{branchName}"));
        }

        private const string EmptySha1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

        public void Commit(string obj, string authorOpt = "", string messageOpt = "", string[] parents = null)
        {
            var commitObj = JObject.FromObject(new
            {
                CreatedAt = DateTime.Now,
                Author = authorOpt,
                Message = messageOpt,
                Parents = parents ?? new [] { CurrentHash },
                Content = WriteObject(obj)
            });

            var commitHash = Hash(commitObj);

            File.WriteAllText(At($"commits/{commitHash}.json"), commitObj.ToString());

            CurrentHash = commitHash;
        }

        public string Hash(JToken patchContent)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(patchContent.ToString()));
            var hashStr = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hashStr;
        }

        public string WriteObject(JToken obj)
        {
            var hash = Hash(obj);
            File.WriteAllText(At($"objects/{hash}.json"), obj.ToString());
            return hash;
        }

        public void Branch(string branchName, string hash)
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
            Directory.CreateDirectory(At("objects"));
            Directory.CreateDirectory(At("commits"));
            Directory.CreateDirectory(At("tags"));

            Branch("master", EmptySha1);
            Head = "master";
        }

        public AgitRepository(string directory)
        {
            _directory = directory;
        }
    }
}