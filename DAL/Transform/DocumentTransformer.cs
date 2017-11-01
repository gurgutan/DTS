using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DTS.DAL.DataModel;

namespace DTS.DAL.Transform
{
    /// <summary>
    /// Класс предназначен для изменения документа в соответствии с некоторым набором правил.
    /// Изменениям могут быть подвергнуты атрибуты, файлы, состав (дочерние документы). Правила изменения могут быть заданы в общем виде
    /// или на языке регулярных выражений.
    /// </summary>
    public class DocumentTransformer
    {
        public string Id;

        public string Name;
        /// <summary>
        /// Предикат для определения условия на документ, подвергаемый трансформации. По умолчанию всегда истина.
        /// </summary>
        public Predicate<DocumentClass> Condition = new Predicate<DocumentClass>(doc => true);

        /// <summary>
        /// Описание операции над документом
        /// </summary>
        public Func<DocumentClass, bool> Action = new Func<DocumentClass, bool>(doc => { return true; });

        public Dictionary<string, DocumentTransformer> Childs = new Dictionary<string, DocumentTransformer>();

        public DocumentTransformer() { Id = "null"; }

        public DocumentTransformer(string _id, Predicate<DocumentClass> condition, Func<DocumentClass, bool> action)
        {
            Id = _id;
            Condition = condition;
            Action = action;
        }

        /// <summary>
        /// Трансформация документа doc с рекурсивным преобразованием потомков
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool Transform(DocumentClass doc)
        {
            if (!Condition(doc)) return false;
            bool result = Action(doc);
            //Трансформация потомков в соответствии с правилами Childs
            var subdocs = doc.Childs;
            foreach (var doc_child in subdocs)
            {
                //Если у потомков this есть те, которые обрабатывают doc_child.Id, то применяем их
                if (Childs.ContainsKey(doc_child.Id))
                    Childs[doc_child.Id].Transform(doc_child);
            }
            return true;
        }
    }
}
