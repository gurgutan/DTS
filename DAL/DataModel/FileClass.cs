using System;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DTS.DAL.DataModel
{
    [DataContract(Name = "File", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("Files")]    
    public class FileClass 
    {
        [Key]
        public int Handle { get; set; }
        [DataMember]
        public string Id = "";
        [DataMember]
        public string FullName { get; set; }
        public string ShortName
        {
            get { return Path.GetFileName(FullName); }
        }
        [DataMember]
        public string GUID;
        [IgnoreDataMember]
        private long size = 0;
        [IgnoreDataMember]
        public long Size
        {
            get
            {
                if (Data == null)
                {
                    if (!File.Exists(FullName)) return 0;
                    FileInfo file = new FileInfo(FullName);
                    return file.Length;
                }
                else
                    return size;
            }
            set
            {
                size = value;
            }
        }

        public byte[] Data { get; set; }

        public FileClass()
        {
        }

        public FileClass(string _id, string _fullName, string _guid = "", byte[] _data = null)
        {
            //if (_guid == "") _guid = Guid.NewGuid().ToString();
            Id = _id;
            FullName = _fullName;
            Data = _data;
            GUID = _guid;
        }

        public FileClass(FileClass file)
        {
            Id = file.Id;
            FullName = file.FullName;
            GUID = file.GUID;
            size = file.size;
            CopyData(file);
        }

        public string AsText(Encoding e)
        {
            return e.GetString(Data);
        }

        public bool Load()
        {
            if (!File.Exists(FullName)) return false;
            Data = File.ReadAllBytes(FullName);
            FileInfo file = new FileInfo(FullName);
            size = file.Length;
            return true;
        }

        public void Save(string _path)
        {
            File.WriteAllBytes(_path, Data);
        }

        public void Move(string _path)
        {
            File.Move(FullName, _path);
        }

        public void Copy(string _path)
        {
            File.Copy(FullName, _path, true);
        }

        public void CopyData(FileClass _file)
        {
            if (_file.Data != null)
                _file.Data.CopyTo(Data, 0);
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }
    }
}
