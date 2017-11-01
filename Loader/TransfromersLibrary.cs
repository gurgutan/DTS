using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTS.DAL.DataModel;
using DTS.DAL.Transform;

namespace Loader
{
    public static class TransfromersLibrary
    {
        /// <summary>
        /// Если в Основном комплекте лежит документ с Id == SOURCE, то все содержимое SOURCE перемещается в комплект, а сам SOURCE удаляется
        /// </summary>
        public static DocumentTransformer ПереместитьИсходники = new DocumentTransformer
        {
            Id = "COMPLECT",
            Name = "Добавление раздела",
            Condition = new Predicate<DocumentClass>(d => d.Id == "COMPLECT" || d.Id == "OBJ_PD_DOC"),
            Action = MoveSources
        };

        /// <summary>
        /// Ничего не делает с текущим разделом, но запускает преобразование комплектов в разделе
        /// </summary>
        public static DocumentTransformer ОбработатьРаздел = new DocumentTransformer
        {
            Id = "DOC__MARK",
            Name = "Обработка раздела",
            Condition = new Predicate<DocumentClass>(d => d.Id == "DOC__MARK"),
            Action = (doc => true),
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "COMPLECT", ПереместитьИсходники },
            }
        };

        public static DocumentTransformer ОбработатьРазделПД = new DocumentTransformer
        {
            Id = "OBJ_PD_FOLDER",
            Name = "Обработка раздела ПД",
            Condition = new Predicate<DocumentClass>(d => d.Id == "OBJ_PD_FOLDER"),
            Action = (doc => true),
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "OBJ_PD_DOC", ПереместитьИсходники }
            }
        };

        public static DocumentTransformer ОбработатьСоставПД = new DocumentTransformer
        {
            Id = "OBJ_PRJCONT_PRJ",
            Name = "Обработать состав проекта на объекты капитаольного строительства",
            Condition = new Predicate<DocumentClass>(d => d.Id == "OBJ_PRJCONT_PRJ"),
            Action = (doc => true),
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "OBJ_PD_FOLDER", ОбработатьРазделПД }
            }
        };

        public static DocumentTransformer ОбработатьСоставЛинейнойЧастиПД = new DocumentTransformer
        {
            Id = "OBJ_PD_PRJCONT_PRJ_LINEAR",
            Name = "Проектная документация на линейные объекты капитального строительства",
            Condition = new Predicate<DocumentClass>(d => d.Id == "OBJ_PD_PRJCONT_PRJ_LINEAR"),
            Action = (doc => true),
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "OBJ_PD_FOLDER", ОбработатьРазделПД }
            }
        };

        /// <summary>
        /// Словарь преобразований документов. Ключем является Id документа
        /// </summary>
        public static Dictionary<string, DocumentTransformer> Transformers = new Dictionary<string, DocumentTransformer>();

        /// <summary>
        /// Добавляет раздел в "Полный комплект на часть комплекса" и в "Полный комплект на сооружение", если комплекты лежат непосредственно в полном комплекте
        /// </summary>
        public static DocumentTransformer ДобавитьРаздел = new DocumentTransformer
        {
            Id = "DOC_FOR_COMPLEX_PART",
            Name = "Добавление раздела",
            Condition = new Predicate<DocumentClass>(d => d.Id == "DOC_FOR_COMPLEX_PART" || d.Id == "DOC_FOR_TITUL"),
            Action = InsertPartition,
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "DOC__MARK", ОбработатьРаздел }
            }
        };

        /// <summary>
        /// Ничего не делает с проектом, но запускает обработку дочерних полных комплектов
        /// </summary>
        public static DocumentTransformer ОбработатьПроект = new DocumentTransformer
        {
            Id = "DOC_FOR_COMPLEX",
            Name = "Обработка проекта",
            Condition = new Predicate<DocumentClass>(d => d.Id == "DOC_FOR_COMPLEX"),
            Action = (doc => true),
            Childs = new Dictionary<string, DocumentTransformer>
            {
                { "DOC_FOR_COMPLEX_PART", ДобавитьРаздел },
                { "DOC_FOR_TITUL", ДобавитьРаздел },
                { "OBJ_PRJCONT_PRJ", ОбработатьСоставПД },
                { "OBJ_PD_PRJCONT_PRJ_LINEAR", ОбработатьСоставЛинейнойЧастиПД }               
            }
        };

        /// <summary>
        /// Инициализация основного словаря преобразований
        /// </summary>
        static TransfromersLibrary()
        {
            Transformers.Add("DOC_FOR_COMPLEX_PART", ДобавитьРаздел);
            Transformers.Add("DOC_FOR_TITUL", ДобавитьРаздел);
            Transformers.Add("COMPLECT", ПереместитьИсходники);
            Transformers.Add("OBJ_PD_DOC", ПереместитьИсходники);
            Transformers.Add("DOC__MARK", ОбработатьРаздел);
            Transformers.Add("OBJ_PRJCONT_PRJ", ОбработатьСоставПД);
            Transformers.Add("OBJ_PD_PRJCONT_PRJ_LINEAR", ОбработатьСоставЛинейнойЧастиПД);
            Transformers.Add("DOC_FOR_COMPLEX", ОбработатьПроект);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        // Методы, необходимые для реализации преобразований
        //------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавление в документ типа DOC_FOR_COMPLEX_PART или DOC_FOR_TITUL раздела с перемещением в него соответствующих комплектов
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool InsertPartition(DocumentClass doc)
        {
            if (doc.Id != "DOC_FOR_COMPLEX_PART" && doc.Id != "DOC_FOR_TITUL") return false;
            //Перемещение основных комплектов в разделы соответствующих марок
            var complects = doc.Childs.Where(child => child.Id == "COMPLECT").ToList();
            foreach (DocumentClass child in complects)
            {
                //Определяем имя раздела для комплекта child
                string partitionName = GetPartitionNameFromComplectName(child.Name);
                //Пытаемся найти раздел в parent (возможно он был создан ранее)
                DocumentClass partition = doc.Childs.FirstOrDefault(c => c.Id == "DOC__MARK" && c.Name == partitionName);
                //Если раздел не найден в parent - создаем его и добавляем в parent
                if (partition == null)
                {
                    partition = new DocumentClass { Id = "DOC__MARK", Name = partitionName };
                    doc.Childs.Add(partition);
                }
                //Перемещаем основной комплект в найденный/созданный раздел
                partition.Add(child);
                doc.Childs.Remove(child);
            }
            return true;
        }

        /// <summary>
        /// Удаление из документа COMPLECT потомков типа SOURCE
        /// </summary>
        /// <param name="doc">документ типа COMPLECT</param>
        /// <returns></returns>
        public static bool MoveSources(DocumentClass doc)
        {
            if (doc.Id != "COMPLECT" && doc.Id != "OBJ_PD_DOC") return false;
            //Найдем все документы типа SOURCE в основном комплекте (документе вида COMPLECT)
            var sources = doc.Childs.Where(child => child.Id == "SOURCE").ToList();
            foreach (var source in sources)
            {
                //Добавляем потомков SOURCE в основной комплект
                doc.Childs.AddRange(source.Childs);
                //Удаляем потомков из SOURCE
                source.Childs.RemoveAll(c => true);
                //Удаляем SOURCE из основного комплекта
                doc.Childs.Remove(source);
            }
            return true;
        }

        //Функция выделяет имя раздела из имени основного комплекта, путем удаления последних 4-х символов
        private static string GetPartitionNameFromComplectName(string s)
        {
            if (s.EndsWith(".000"))
                return s.Remove(s.Length - 4);
            else
                throw new ArgumentException("TDMS: Обозначение комплекта должно заканчиваться символами '.000' :" + s);
        }
    }
}
