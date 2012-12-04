using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;

namespace Bendras.HashFile
{
    public class HashFile
    {
        private readonly IniData iniData;

        public HashFile(IniData iniData)
        {
            this.iniData = iniData;
        }

        public static HashFile OpenFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            using (var newFile = System.IO.File.Create(filePath))
            {
                newFile.Flush();
            }

            IniParser.FileIniDataParser parser = new IniParser.FileIniDataParser();
            var iniData = parser.LoadFile(filePath);

            return new HashFile(iniData) { FileName = filePath };
        }

        protected void Save(string filePath, IniData iniData)
        {
            IniParser.FileIniDataParser parser = new IniParser.FileIniDataParser();
            parser.SaveFile(filePath, iniData);
        }

        private string FileName { get; set; }

        public IEnumerable<FileHash> FileHashes
        {
            get
            {
                foreach (var item in this.iniData.Global)
                {
                    yield return new FileHash { FileName = item.KeyName, Hash = item.Value };
                }
            }
        }

        public void Add(string file, string hash, string hashType = "MD5")
        {
            if (this.iniData.Sections.ContainsSection(file))
            {
                this.iniData.Sections.RemoveSection(file);
            }

            this.iniData.Sections.AddSection(file);
            this.iniData.Sections[file].AddKey(hashType, hash);
        }

        public void Save()
        {
            Save(this.FileName, this.iniData);
        }

        public class FileHash
        {
            public string FileName { get; set; }

            public string Hash { get; set; }
        }
    }
}
