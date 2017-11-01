using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DTS.DAL.DataModel;

namespace DTS.DAL.DataModel
{
    [DataContract(Name = "Document", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("Documents")]    
    public partial class DocumentClass 
    {
        [Key]
        public int Handle { get; set; }

        [DataMember(Order = 1)]
        public string Id = "";

        [DataMember(Order = 2)]
        public string GUID { get; set; }

        [DataMember(Order = 3)]
        public string Name { get; set; }

        [DataMember(Order = 4)]
        public string ParentGUID { get; set; }

        [DataMember(Order = 5)]
        public List<AttributeClass> Attributes { get; set; }

        [DataMember(Order = 6)]
        public List<FileClass> Files { get; set; }

        [DataMember(Order = 7)]
        public List<DocumentClass> Childs { get; set; }

        public DocumentClass()
        {
            //GUID = Guid.NewGuid().ToString();
            Attributes = new List<AttributeClass>();
            //Tables = new List<DataTable>();
            Files = new List<FileClass>();
            Childs = new List<DocumentClass>();
        }

        public DocumentClass(string _name = "", string _guid = "")
        {
            //if (_guid == "") _guid = Guid.NewGuid().ToString();
            Name = _name;
            GUID = _guid;
            Attributes = new List<AttributeClass>();
            //Tables = new List<DataTable>();
            Files = new List<FileClass>();
            Childs = new List<DocumentClass>();
        }

        /// <summary>
        /// Создается документ в который копируются все атрибуты, файлы и копии дочерних документов doc
        /// </summary>
        /// <param name="doc">документ оригинал</param>
        public DocumentClass(DocumentClass doc)
        {
            Id = doc.Id;
            GUID = doc.GUID;
            Name = doc.Name;
            ParentGUID = doc.ParentGUID;
            Attributes = new List<AttributeClass>(doc.Attributes);
            Files = new List<FileClass>(doc.Files);
            Childs = new List<DocumentClass>();
            foreach (DocumentClass child in doc.Childs)
            {
                DocumentClass thisChild = new DocumentClass(child);
                thisChild.ParentGUID = this.GUID;
                Childs.Add(thisChild);
            }
        }

        public void Add(DocumentClass child)
        {
            child.ParentGUID = GUID;
            Childs.Add(child);
        }

        public void Add(AttributeClass attr)
        {
            Attributes.Add(attr);
        }

        public void Add(FileClass file)
        {
            Files.Add(file);
        }

        public void AssembleFiles(string _path)
        {
            if (Files.Count > 0)
            {
                string docPath = Path.Combine(_path, GUID);
                System.IO.Directory.CreateDirectory(docPath);
                foreach (FileClass file in Files)
                    file.Copy(Path.Combine(docPath, file.GUID));
            }
            foreach (DocumentClass child in Childs)
                child.AssembleFiles(_path);
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }

        /// <summary>
        /// Метод загружает документ из папки. Создает дочерние документы рекурсивно в соответствии со структурой папок
        /// </summary>
        /// <param name="folderName">Полный путь для загрузки документа - эта папка соответствует создаваемому документу</param>
        /// <returns>созданный документ типа DocumentClass</returns>
        /// <exception cref="DirectoryNotFoundException">Выбрасывается в случае невозможности получить доступ к папке folderName</exception>
        public static DocumentClass LoadFromFolder(string folderName)
        {
            if (String.IsNullOrEmpty(folderName)) throw new DirectoryNotFoundException("Не указана папка для загрузки документа");
            folderName = folderName.TrimEnd('\\');
            folderName.Replace(@"\", @"\\");
            DirectoryInfo rootFolder = new DirectoryInfo(folderName);
            if (!rootFolder.Exists) throw new DirectoryNotFoundException("Не найдена папка " + folderName);
            DocumentClass doc = new DocumentClass(rootFolder.Name);
            //Добавим описания файлов
            FileInfo[] files = rootFolder.GetFiles();
            foreach (FileInfo file in files)
            {
                FileClass docFile = new FileClass("", file.FullName);
                doc.Files.Add(docFile);
            }
            //Добавим состав
            DirectoryInfo[] childFolders = rootFolder.GetDirectories();
            foreach (DirectoryInfo childFolder in childFolders)
            {
                DocumentClass child = DocumentClass.LoadFromFolder(childFolder.FullName);
                child.ParentGUID = doc.GUID;
                doc.Childs.Add(child);
            }
            return doc;
        }

        //Загрузить атрибуты в doc из attributes. Существующие атрибуты с таким же Id перезаписываются
        public static void SetAttributes(DocumentClass doc, IEnumerable<AttributeClass> attributes)
        {

            foreach (AttributeClass attr in attributes)
            {
                AttributeClass docAttr = doc.Attributes.FirstOrDefault(a => a.Id == attr.Id);
                if (docAttr == null)
                {
                    doc.Attributes.Add(new AttributeClass(attr));
                }
                else
                {
                    docAttr = new AttributeClass(attr);
                }
            }
        }

        /// <summary>
        /// Функция возвращает истину, если документ соответствует шаблону pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public bool IsMatchPattern(DocumentPatternClass pattern)
        {
            return pattern.IsMatch(this);
        }

        /// <summary>
        /// Метод применяет шаблон pattern к документу, заполняя его атрибуты
        /// </summary>
        /// <param name="pattern"></param>
        public bool AmplifyIfMatch(DocumentPatternClass pattern)
        {
            return pattern.IsMatch(this, true);
        }

    }
}
