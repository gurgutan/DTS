using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace DTS.DAL.DataModel
{
    [DataContract(Name = "FilePattern", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("FilesPatterns")]    
    public class FilePatternClass
    {
        [Key]
        public int Handle { get; set; }

        [DataMember(Order = 1)]
        public string Id = "";

        //[DataMember(Order = 2)]
        public string FileNamePattern = "";

        [DataMember(Order = 3)]
        public long MinFileSize = 0;

        [DataMember(Order = 4)]
        public long MaxFileSize = long.MaxValue;

        [IgnoreDataMember]
        public string MatchResult;

        public FilePatternClass() { }

        public FilePatternClass(string _id, string _fileNamePattern)
        {
            Id = _id;
            FileNamePattern = _fileNamePattern;
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }

        public bool IsMatch(FileClass file)
        {
            if (file == null)
            {
                throw new Exception("Matching: файл " + Id + " : null\n");
            }
            if (file.Id != "" && this.Id != "" && file.Id != this.Id)
                return false;
            if (FileNamePattern != "" && !Regex.IsMatch(file.ShortName, FileNamePattern))
            {
                MatchResult = "имя файла " + file.ShortName + " не удовлетворяет шаблону";
                return false;
            }
            if (file.Size < MinFileSize || file.Size > MaxFileSize)
            {
                MatchResult = "размер файла " + file.ShortName + " лежит вне диапазона [" + MinFileSize + "," + MaxFileSize + "]";
                return false;
            }
            return true;
        }

    }
}
