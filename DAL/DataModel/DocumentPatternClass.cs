using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DTS.DAL.DataModel;


namespace DTS.DAL.DataModel
{
    [DataContract(Name = "DocumentPattern", Namespace = "http://www.gpp.docker.ru")]
    [System.ComponentModel.DataAnnotations.Schema.Table("DocumentsPatterns")]    
    public class DocumentPatternClass
    {
        [Key]
        public int Handle { get; set; }

        [DataMember(Order = 1)]
        public string Id = "";

        [DataMember(Order = 2)]
        public string GUID = "";

        [DataMember(Order = 3)]
        public string Name = "";

        [DataMember(Order = 4)]
        public string AttributesPatternString = "";

        [DataMember(Order = 5)]
        public CompareMode AttributesCompareMode = CompareMode.ExistsInDocument; //По умолчанию необходимо наличие в документе атрибутов из шаблона

        [DataMember(Order = 6)]
        public List<AttributePatternClass> AttributesPatterns { get; set; }

        [DataMember(Order = 7)]
        public List<FilePatternClass> FilesPatterns { get; set; }

        [DataMember(Order = 8)]
        public List<DocumentPatternClass> ChildsPatterns { get; set; }

        private MatchResult result = new MatchResult();
        [DataMember(Order = 0)]
        public MatchResult Result
        {
            get { return result; }
            private set { result = value; }
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }

        public DocumentPatternClass()
        {
            AttributesPatterns = new List<AttributePatternClass>();
            FilesPatterns = new List<FilePatternClass>();
            ChildsPatterns = new List<DocumentPatternClass>();
        }

        /// <summary>
        /// Функция пытается сопоставить данный шаблон документу doc
        /// </summary>
        /// <param name="doc">проверяемый документ</param>
        /// <param name="amplifyIfMatch">флаг "дополнять" разрешает заполнять недостающие части документа в соответствии с шаблоном</param>
        /// <returns>истина, если сопоставление успешно</returns>
        public bool IsMatch(DocumentClass doc, bool amplifyIfMatch = false)
        {
            //Создаем объект для хранения результата сопоставления
            result = new MatchResult(this, doc);
            if (doc == null) return false;
            result.Success =
                IsIdMatch(doc, amplifyIfMatch) &&
                IsAttributesMatch(doc, amplifyIfMatch) &&
                IsFilesMatch(doc, amplifyIfMatch) &&
                IsChildsMatch(doc, amplifyIfMatch);
            if (result.Success)
            {
                result.Text = "Проверка " + doc.Name + " по шаблону (" + this.Name + " - " + this.Id + ") успешна";
                //Заполняем Id документа, в случае успеха проверки
                if (amplifyIfMatch && doc.Id == "") doc.Id = this.Id;
            }
            else
            {
                result.Text = "Документ " + doc.Name + " не прошел проверку по шаблону (" + this.Name + "-" + this.Id + ")\n" + result.Text;
            }
            return result.Success;
        }

        /// <summary>
        /// Проверка соответствия Id документа и шаблона. Для документа с Id = "", проверка считается успешной
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private bool IsIdMatch(DocumentClass doc, bool amplifyIfMatch = false)
        {
            if (this.Id != "" && doc.Id != "" && this.Id != doc.Id)
            {
                //result.Text += "Не совпадают Id: " + this.Id + "!=" + doc.Id;
                return false;
            }
            //Пока не решено, как проверять, когда извест
            //if (amplifyIfMatch && doc.Id == "") doc.Id = this.Id;
            return true;
        }

        /// <summary>
        /// Функция возвращает истину, если сопоставление атрибутов документа шаблону успешно. 
        /// Правило сопоставления зависит от значения поля AttributesCompareMode.
        /// Возможны варианты: 
        ///     CompareMode.ExistsInDocument - проверка наличия соответствующего атрибута для каждого шаблона атрибута;
        ///     CompareMode.ExistsInPattern - проверка наличия шаблона атрибута для каждого атрибута документа;
        ///     CompareMode.Equality - проверка полного соответствия атрибутов шаблонам
        /// </summary>
        /// <param name="doc">проверяемый документ</param>
        /// <param name="amplifyIfMatch">указывает необходимость заполнения значений</param>
        /// <returns></returns>
        private bool IsAttributesMatch(DocumentClass doc, bool amplifyIfMatch = false)
        {
            //Флаг  успеха сравнения
            bool success = true;
            //Если в шаблоне документа не было заявлено атрибутов, то проверка считается успешной
            if (AttributesPatterns.Count == 0) return true;
            //Временная коллекция для сравнения. Содержит существующие и предполагаемые атрибуты документа
            List<AttributeClass> attributes = new List<AttributeClass>();
            //Если строка шаблона не пустая, то пытаемся извлечь атрибуты из имени и добавляем во временную коллецию
            if (AttributesPatternString != "")
                attributes = AttributesFromString(doc.Name, AttributesPatternString);
            //Если у документа уже есть атрибуты, то добавляем их во временную коллекцию
            attributes.AddRange(doc.Attributes);
            //Проверяем наличие в документе атрибутов, описанных в шаблоне
            if (AttributesCompareMode == CompareMode.ExistsInDocument)
            {
                foreach (AttributePatternClass ptrn in this.AttributesPatterns)
                {
                    AttributeClass attr = attributes.FirstOrDefault(a => ptrn.IsMatch(a));
                    if (attr == null)
                    {
                        result.Text += "\nАтрибут не найден: '" + ptrn.Id + "'";
                        success = false;
                    }
                    else
                    {
                        result.Text += ptrn.MatchResult;
                    }
                }
            }
            //Проверяем наличие в шаблоне атрибутов, описанных в документе
            else if (AttributesCompareMode == CompareMode.ExistsInPattern)
            {
                //Каждый из атрибутов документа проверяем на соответствие какому-либо шаблону
                foreach (AttributeClass attr in attributes)
                {
                    //Ищем шаблон атрибута, которому удовлетворяет attr
                    AttributePatternClass ptrn = AttributesPatterns.FirstOrDefault(a => a.IsMatch(attr));
                    //Если условия не выполнены
                    if (ptrn == null)
                    {
                        result.Text += "\nАтрибут не предусмотрен: '" + attr.Id + "=" + attr.AsString + "' {" +
                            AttributesPatterns.
                            Where(p => p.MatchResult != "").
                            Aggregate("", (cur, next) => cur == "" ? cur + next : cur + "; " + next) + "}";
                        success = false;
                    }
                    else
                    {
                        result.Text += ptrn.MatchResult;
                    }
                }
            }
            //Проверяем взаимнооднозначное соответствие атрибутов документа атрибутам шаблона
            else if (AttributesCompareMode == CompareMode.Equality)
            {
                //Временная коллекция шаблонов атрибутов
                List<AttributePatternClass> patterns = new List<AttributePatternClass>(this.AttributesPatterns);
                foreach (AttributeClass attr in attributes)
                {
                    //Ищем шаблон атрибута, которому удовлетворяет attr
                    AttributePatternClass ptrn = patterns.FirstOrDefault(a => a.IsMatch(attr));
                    //Если такой атрибут в шаблоне отсутствует
                    if (ptrn == null)
                    {
                        result.Text += "\nАтрибут не предусмотрен: '" + attr.Id + "=" + attr.AsString + "' {" +
                            AttributesPatterns.
                            Where(p => p.MatchResult != "").
                            Aggregate("", (cur, next) => cur == "" ? cur + next : cur + "; " + next) + " }";
                        success = false;
                    }
                    else
                    {
                        //Если шаблон атрибута найден, то при следующей проверке он не используется
                        patterns.Remove(ptrn);
                    }
                }
                //Если все атрибуты документа проверены и для каждого нашелся свой шаблон, то проверка успешна
                success = success && (patterns.Count == 0);
            }
            //Дополняем описание атрибутов, если сопоставление успешно
            if (amplifyIfMatch && success) doc.Attributes = attributes;
            return success;
        }

        private bool IsFilesMatch(DocumentClass doc, bool amplifyIfMatch = false)
        {
            bool success = true;
            foreach (FileClass file in doc.Files)
            {
                FilePatternClass ptrn = FilesPatterns.FirstOrDefault(f => f.IsMatch(file));
                if (ptrn == null)
                {
                    //В сообщение о несоответствии собираем все непустые сообщения о результатах проверки
                    result.Text += "Несоответствие файла '" + (file.Id == "" ? "" : file.Id + ":") + file.ShortName + "'" +
                        FilesPatterns.
                        Where(p => p.MatchResult != "").
                        Select(presult => presult.MatchResult).
                        Aggregate(":", (cur, next) => cur == ":" ? cur + next : cur + "; " + next);
                    success = false;

                }
                else
                {
                    result.Text += ptrn.MatchResult;
                    //Дополняем описание файла, если сопоставление успешно
                    if (amplifyIfMatch && file.Id == "") file.Id = ptrn.Id;
                }
            }
            return success;
        }

        private bool IsChildsMatch(DocumentClass doc, bool amplifyIfMatch = false)
        {
            bool success = true;
            foreach (DocumentClass child in doc.Childs)
            {
                //Ищем первый шаблон, в списке дочерних шаблонов, которому удовлетворяет child 
                //TODO: Вообще, подходящих шаблонов может быть несколько и, тогда нужны дополнительные данные для определения, какой шаблон правильный
                DocumentPatternClass ptrn = ChildsPatterns.FirstOrDefault(p => p.IsMatch(child, amplifyIfMatch));
                if (ptrn == null)
                {
                    result.Text += "Дочерний документ не прошел проверку " + child.Name + "\n";
                    //Запишем результаты неуспешных проверок дочерних шаблонов в результат проверки текущего шаблона
                    result.Childs.AddRange(ChildsPatterns.Select(p => p.Result));
                    success = false;
                }
                else
                {
                    //Добавим в дерево результат проверки дочернего документа
                    result.Childs.Add(ptrn.Result);
                }
            }
            return success;
        }

        /// <summary>
        /// Метод пытается распознать атрибуты из строки _input в соответствии с шаблоном _pattern
        /// </summary>
        /// <param name="_input">строка, в которой содержаться значения атрибутов (например, имя папки или файла)</param>
        /// <param name="_pattern">регулярное выражение, для парсинга строки. Именованные группы этого выражения будут атрибутами. Id атрибута = имя группы</param>
        /// <returns>Массив атрибутов. Если распознавание успешно, то размер массива больше 0</returns>
        public List<AttributeClass> AttributesFromString(string _input, string _pattern = "")
        {
            if (_pattern == "") _pattern = AttributesPatternString;
            //Выделим имена атрибутов из строки шаблона
            Regex r = new Regex(_pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
            string[] names = r.GetGroupNames();
            //Выделим значения атрибутов из входной строки
            Match match = r.Match(_input);
            Dictionary<string, AttributeClass> attributes = new Dictionary<string, AttributeClass>();
            if (!match.Success) return attributes.Values.ToList();
            for (int i = 1; i < names.Length; i++)
            {
                Group group = match.Groups[names[i]];
                if (!attributes.ContainsKey(names[i]))
                    attributes.Add(names[i], new AttributeClass(names[i], group.Value));
            }
            return attributes.Values.ToList();
        }

    }
}
