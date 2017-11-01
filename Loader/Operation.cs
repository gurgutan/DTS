using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DTS.DAL.DataModel;

namespace Loader
{
    [DataContract(Name = "OperationType", Namespace = "http://www.gpp.docker.ru")]
    public enum OperationType
    {
        [EnumMember(Value = "Read")]
        Read,
        [EnumMember(Value = "Write")]
        Write,
        [EnumMember(Value = "Change")]
        Change,
        [EnumMember(Value = "Check")]
        Check,
        [EnumMember(Value = "Help")]
        Help
    };

    /// <summary>
    /// Класс для описания результата операции
    /// </summary>
    [DataContract(Name = "OperationResult", Namespace = "http://www.gpp.docker.ru")]
    public class OperationResult
    {
        /// <summary>
        /// Код завершения операции (0 - успешно, другие значения - ошибка или предупреждение)
        /// </summary>
        [DataMember(Order = 1)]
        public int Code;

        /// <summary>
        /// Операция-владелец данного результата. Инициализируется при создании.
        /// </summary>
        public Operation Owner;

        /// <summary>
        /// Текст с описанием результата операции
        /// </summary>
        [DataMember(Order = 5)]
        public string Description = "";

        public OperationResult(Operation o)
        {
            Owner = o;
            Code = 0;
        }

        public OperationResult(Operation o, int code)
        {
            Owner = o;
            Code = code;
        }

        public OperationResult(Operation o, int code, DocumentClass doc)
        {
            Owner = o;
            Code = code;
        }

        public override string ToString()
        {
            return JsonHelper.Json(this);
        }
    }

    /// <summary>
    /// Класс для описания операции над документом
    /// </summary>
    [DataContract(Name = "Operation", Namespace = "http://www.gpp.docker.ru")]
    public class Operation
    {
        [DataMember(Order = 11)]
        public DateTime TimeStart { get; set; }

        [DataMember(Order = 12)]
        public DateTime TimeFinish { get; set; }

        [DataMember(Order = 13)]
        public OperationType Type { get; set; } = OperationType.Read;

        [DataMember(Order = 14)]
        public string Path { get; set; }

        [DataMember(Order = 15)]
        public DocumentClass Document { get; set; } = null;

        [DataMember(Order = 16)]
        public DocumentPatternClass Pattern { get; set; } = null;

        [DataMember(Order = 17)]
        public OperationResult Result { get; set; } = null;

        public Operation() { Result = new OperationResult(this); }

        public Operation(OperationType _type)
        {
            Type = _type;
        }

        public Operation(OperationType _type, DocumentClass _doc)
        {
            Type = _type;
            Document = _doc;
        }

        public override string ToString()
        {
            return DTS.DAL.DataModel.JsonHelper.Json(this);
        }

        public void Write(string filename)
        {
            File.AppendAllText(filename, StringHelper.FormatLinesByScopes(this.ToString()));
        }

        public void Execute()
        {
            //Записываем время начала исполнения
            TimeStart = DateTime.Now;
            //Создаем объект для записи результата операции
            Result = new OperationResult(this);
            switch (Type)
            {
                case OperationType.Read: ReadDocument(); break;
                case OperationType.Check: CheckDocument(); break;
                case OperationType.Change: ChangeDocument(); break;
                case OperationType.Write: WriteDocument(); break;
                case OperationType.Help: Help(); break;
                default: throw new InvalidOperationException("Неизвестный тип операции: " + Type.ToString());
            }
            TimeFinish = DateTime.Now;
        }

        public void Execute(DocumentClass doc)
        {
            Document = doc;
            this.Execute();
        }

        public void Execute(DocumentClass doc, OperationType op_type)
        {
            Document = doc;
            Type = op_type;
            this.Execute();
        }

        public void Help()
        {
            Result.Description = @"Формат команды: (read|check|write|help) PATH [PATTERN]";
            Result.Code = 0;
        }

        /// <summary>
        /// Чтение документа из папки на диске
        /// </summary>
        private void ReadDocument()
        {
            //Проверка пути загрузки
            if (String.IsNullOrEmpty(Path))
            {
                Result.Description = "Ошибка: пустой путь";
                Result.Code = 101;
                return;
            }
            try
            {
                Document = DocumentClass.LoadFromFolder(Path);
            }
            catch (DirectoryNotFoundException e)
            {
                Result.Description = String.Format("Ошибка доступа к папке: {0}", e.Message);
                Result.Code = 102;
                return;
            }
            catch (FileNotFoundException e)
            {
                Result.Description = String.Format("Ошибка доступа к файлу: {0}" + e.Message);
                Result.Code = 103;
                return;
            }
            catch (ArgumentException e)
            {
                Result.Description = String.Format("Ошибка в пути {0}: {1}", Path, e.Message);
                Result.Code = 104;
                return;
            }
            //Если все-таки в результате загрузки не получен документ, то записываем ошибку и выходим
            if (Document == null)
            {
                Result.Description = String.Format("Ошибка чтения документа из папки {0}", Path);
                Result.Code = 105;
                return;
            }
            Result.Description = "Документ загружен";
            Result.Code = 0;
        }

        /// <summary>
        /// Проверка документа на соответствие шаблону
        /// </summary>
        private void CheckDocument()
        {
            if (Document == null)
            {
                Result.Description = "Ошибка: документ не загружен";
                Result.Code = 201;
                return;
            }
            // Если тип документа (Id) пустой, то попытаемся определить его по библиотеке шаблонов
            if (Document.Id == "")
            {
                Console.WriteLine("Проверка по всем шаблонам");
                //Перебор всех шаблонов, пока не встретим тот, которому соответствует документ
                foreach (DocumentPatternClass pattern in PatternsLibrary.PatternsDictionary.Values)
                {
                    Pattern = pattern;
                    //Скопируем документ для проверки
                    DocumentClass clone = new DocumentClass(Document);
                    //Если проверка по шаблону pattern дала положительный результат, возвращаем этот документ, с заполненными атрибутами
                    if (pattern.IsMatch(clone, true))
                    {
                        Document = clone;
                        break;
                    }
                }
            }
            else
            {
                if (!PatternsLibrary.PatternsDictionary.ContainsKey(Document.Id))
                {
                    Result.Description = String.Format("Ошибка: не найден шаблон {0} для проверки документа", Document.Id);
                    Result.Code = 202;
                    return;
                }
                else
                {
                    DocumentClass clone = new DocumentClass(Document);
                    Pattern = PatternsLibrary.PatternsDictionary[Document.Id];
                    //Если проверка по шаблону pattern дала положительный результат, возвращаем этот документ, с заполненными атрибутами
                    if (Pattern.IsMatch(clone, true))
                    {
                        Document = clone;
                    }
                }
            }
            Result.Description = "Документ проверен";
            Result.Code = 0;
        }

        private void ChangeDocument()
        {
            if (Document == null)
            {
                Result.Description = "Ошибка: документ не загружен";
                Result.Code = 201;
                return;
            }
            if (TransfromersLibrary.Transformers.ContainsKey(Document.Id))
            {
                TransfromersLibrary.Transformers[Document.Id].Transform(Document);
                Result.Description = String.Format("Документ {0} преобразован по шаблону {1}", Document.Name, TransfromersLibrary.Transformers[Document.Id].Name);
                Result.Code = 0;
                return;
            }
            else
            {
                Result.Description = String.Format("Ошибка: не найден шаблон преобразования для {0}", Document.Id);
                Result.Code = 202;
                return;
            }
        }

        private void WriteDocument()
        {
            if (Document == null)
            {
                Result.Description = "Ошибка: документ не инициализирован";
                return;
            }
            try
            {
                File.WriteAllText(Path, StringHelper.FormatLinesByScopes(Document.ToString()));
            }
            catch (ArgumentException e)
            {
                Result.Description = String.Format("Ошибка имени файла: {0}", Path);
                return;
            }
            catch (DirectoryNotFoundException e)
            {
                Result.Description = String.Format("Ошибка: путь {0} не найден ", Path);
                return;
            }
            catch (IOException e)
            {
                Result.Description = String.Format("Ошибка открытия файла {0}", Path);
                return;
            }
            catch (NotSupportedException e)
            {
                Result.Description = String.Format("Не поддерживаемый формат имени файла {0}", Path);
                return;
            }
        }

        public bool TryParse(string arg)
        {
            //Набор регулярных выражений, для выделения команды в строке
            string regex = @"(?'TYPE'(read|check|write|help))(\s+(?'PATH'""[^""]+""))?(\s+(?'PATTERN'[\w\d]+))?";
            Match match = Regex.Match(arg, regex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            if (!match.Success)
            {
                Result.Description = String.Format("Ошибка интерпретации строки {0}", arg);
                return false;
            }
            string type = match.Groups[1].Value.ToLower();
            string path = match.Groups[2].Value;
            string ptrn = match.Groups[3].Value;
            //Тип операции
            switch (type)
            {
                case "read": this.Type = OperationType.Read; break;
                case "check": this.Type = OperationType.Check; break;
                case "write": this.Type = OperationType.Write; break;
                case "help": this.Type = OperationType.Help; break;
                default:
                    {
                        Result.Code = -1;
                        Result.Description = String.Format("Ошибка интерпретации: неизвестная команда {0}", type);
                        return false;
                    }
            }
            //Путь к объекту
            this.Path = path.Trim('"');
            //Шаблон
            if (!String.IsNullOrEmpty(ptrn))
            {
                if (!PatternsLibrary.PatternsDictionary.ContainsKey(ptrn))
                {
                    Result.Code = -2;
                    Result.Description = String.Format("Ошибка интерпретации: отсутствует шаблон документа {0}", ptrn);
                    return false;
                }
                this.Pattern = PatternsLibrary.PatternsDictionary[ptrn];
            }
            return true;
        }
    }

    //read "file://d:/temp"
    //write "tdms://{5876A35E-F558-4CEA-803A-83A5DD32805B}"
}
