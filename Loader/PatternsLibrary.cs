using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTS.DAL.DataModel;

namespace Loader
{
    /// <summary>
    /// Набор шаблонов соответствует структуре объектов TDMS
    /// </summary>
    public static class PatternsLibrary
    {
        public static class RegexpStrings
        {
            public const string Проект = @"(?'DOG_NUM'[\w\d\-]+)\.(?'DOP_SOGL_NUM'[\w\d]+)\.(?'STAGE_TYPE'[\w\d]+)\.(?'COM_ATTR_COMPLEXTYPE'[\w\d]+)";
            public const string ЧастьКомплекса = @"(?'COM_ATTR_CPARTTYPE_CODE'[\w\d\-]+)\.(?'BLDCPART_ATTR_SPCODE'[\w\d]+)";
            public const string Сооружение = @"(?'BLDCONSTRUCTION_ATTR_GPCODE'[\w\d]+)";
            public const string Марка = @"(?'MARK'\w+)";
            public const string ТипДокумента = @"(?'PSD_TYPE'\w{1,4})";
            public const string ДопКод = @"(?'ADD_CODE'[\d\-]{1,8})?";
            public const string ТипДокументаПД = @"(?'ATR_PD_PRJPART_TYPE'Раздел|Подраздел|Часть|Книга|Том)";
            public const string ПолныйНомерПД = @"(?'ATR_PD_FNUM'[\d]{1,2}(\.[\d]{1,2}(\^[\d])?)+)";
            public const string НаименованиеПД = @"(?'ATR_PD_TITLE'[\w\s\d]+)";
            public const string ОбозначениеПД = Проект + @"\.(?'MARK'[\w\d\-]+)\." + ТипДокумента;
            public const string НомерИзмененияПД = @"(?'VERSION'\(\d+\))?";
        }

        //Шаблоны имен файлов
        public static class FilesPatterns
        {
            public static FilePatternClass ANYFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.[\w\d]{3}$");
            public static FilePatternClass DWGFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.dwg$");
            public static FilePatternClass PDFFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.pdf$");
            public static FilePatternClass DOCFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(docx|doc|rtf)$");
            public static FilePatternClass ODTFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(odt)$");
            public static FilePatternClass XLSFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(xlsx|xls)$");
            public static FilePatternClass XMLFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.xml$");
            public static FilePatternClass TIFFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(tif|tiff)$");
            public static FilePatternClass PNGFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.png$");
            public static FilePatternClass JPGFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.jpg$");
            public static FilePatternClass CDRFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.cdr$");
            public static FilePatternClass HTMLFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(htm|html)$");
            public static FilePatternClass ARCFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.(zip|rar|7z|\d{1,3})$");
            public static FilePatternClass TXTFile = new FilePatternClass("", @"^[\w\s\d\-\.]+\.txt$");
        }

        public static class DocumentsPatterns
        {
            //---------------------------------------------------------------------------------------------------------------------------------------
            //Рабочая документация
            public static DocumentPatternClass Чертеж = new DocumentPatternClass
            {
                Id = "DRAWING",
                Name = "Чертеж",
                AttributesPatternString = @"^(Лист\s*\№)(?'LIST_NUM'\d+)",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("LIST_NUM") },
                FilesPatterns = new List<FilePatternClass> { FilesPatterns.PDFFile, FilesPatterns.DWGFile, FilesPatterns.DOCFile }
            };

            public static DocumentPatternClass Документ = new DocumentPatternClass
            {
                Id = "PSD",
                Name = "Документ",
                AttributesPatternString = "(?'DOC_UNUM'" +
                    RegexpStrings.Проект + @"\." +
                    RegexpStrings.ЧастьКомплекса + @"\." +
                    RegexpStrings.Сооружение + @"\." +
                    RegexpStrings.Марка + @"\." +
                    RegexpStrings.ТипДокумента +
                    RegexpStrings.ДопКод + ")",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("DOC_UNUM") },
                FilesPatterns = new List<FilePatternClass> { 
                    FilesPatterns.PDFFile, 
                    FilesPatterns.XLSFile, 
                    FilesPatterns.DOCFile, 
                    FilesPatterns.HTMLFile, 
                    FilesPatterns.XMLFile, 
                    FilesPatterns.JPGFile 
                }
            };

            public static DocumentPatternClass ИсходныеДокументы = new DocumentPatternClass
            {
                Id = "SOURCE",
                Name = "Исходные документы",
                AttributesPatternString = "^Source$",
                AttributesPatterns = new List<AttributePatternClass> { },
                ChildsPatterns = new List<DocumentPatternClass> { Документ, Чертеж }
            };

            public static DocumentPatternClass ОсновнойКомплект = new DocumentPatternClass
            {
                Id = "COMPLECT",
                Name = "Основной комплект",
                AttributesPatternString = "^(?'C_UNUM'" +
                    RegexpStrings.Проект + @"\." +
                    RegexpStrings.ЧастьКомплекса + @"\." +
                    RegexpStrings.Сооружение + @"\." +
                    RegexpStrings.Марка + @"\.000)$",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("C_UNUM"), new AttributePatternClass("MARK") },
                FilesPatterns = new List<FilePatternClass> { FilesPatterns.PDFFile },
                ChildsPatterns = new List<DocumentPatternClass> { ИсходныеДокументы }
            };

            public static DocumentPatternClass Раздел = new DocumentPatternClass
            {
                Id = "DOC__MARK",
                Name = "Раздел",
                AttributesPatternString = "^(?'RAZD_UNUM'" +
                    RegexpStrings.Проект + @"\." +
                    RegexpStrings.ЧастьКомплекса + @"\." +
                    RegexpStrings.Сооружение + @"\." +
                    RegexpStrings.Марка + ")$",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("RAZD_UNUM"), new AttributePatternClass("MARK") },
                ChildsPatterns = new List<DocumentPatternClass> { ОсновнойКомплект }
            };

            public static DocumentPatternClass КомплектСооружения = new DocumentPatternClass
            {
                Id = "DOC_FOR_TITUL",
                Name = "Полный комплект на сооружение",
                AttributesPatternString = @"^(?'UNUM'" +
                    RegexpStrings.Проект + @"\." +
                    RegexpStrings.ЧастьКомплекса + @"\." +
                    RegexpStrings.Сооружение + ")$",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("UNUM") },
                ChildsPatterns = new List<DocumentPatternClass> { ОсновнойКомплект }
            };

            public static DocumentPatternClass КомплектЧастиКомплекса = new DocumentPatternClass
            {
                Id = "DOC_FOR_COMPLEX_PART",
                Name = "Полный комплект на часть комплекса",
                AttributesPatternString = "^(?'UNUM'" + RegexpStrings.Проект + @"\." + RegexpStrings.ЧастьКомплекса + ")$",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("UNUM") },
                ChildsPatterns = new List<DocumentPatternClass> { КомплектСооружения, ОсновнойКомплект }
            };

            public static DocumentPatternClass Проект = new DocumentPatternClass
            {
                Id = "DOC_FOR_COMPLEX",
                Name = "Проект",
                AttributesPatternString = "^(?'UNUM'" + RegexpStrings.Проект + ")$",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("UNUM") },
                ChildsPatterns = new List<DocumentPatternClass> { DocumentsPatterns.КомплектЧастиКомплекса, DocumentsPatterns.ОсновнойКомплект }
            };

            //---------------------------------------------------------------------------------------------------------------------------------------
            //Проектная документация
            public static DocumentPatternClass ТекстоваяЧасть = new DocumentPatternClass
            {
                Id = "OBJ_NOTETEXT",
                Name = "Текстовая часть",
                AttributesPatternString = "^Текстовая часть$",
                AttributesPatterns = new List<AttributePatternClass> { },
                ChildsPatterns = new List<DocumentPatternClass> { }
            };

            public static DocumentPatternClass Приложение = new DocumentPatternClass
            {
                Id = "OBJ_NOTEATT",
                Name = "Приложение",
                AttributesPatternString = @"^Приложение\s*(?'NOTEATT_ATTR_NUM'[\w\d]{1,2})",
                AttributesPatterns = new List<AttributePatternClass> { new AttributePatternClass("NOTEATT_ATTR_NUM") },
                ChildsPatterns = new List<DocumentPatternClass> { }
            };

            public static DocumentPatternClass ИсходныеДокументыПД = new DocumentPatternClass
            {
                Id = "SOURCE",
                Name = "Исходные документы",
                AttributesPatternString = "^Source$",
                AttributesPatterns = new List<AttributePatternClass> { },
                ChildsPatterns = new List<DocumentPatternClass> { Приложение, ТекстоваяЧасть }
            };

            public static DocumentPatternClass ДокументПД = new DocumentPatternClass
            {
                Id = "OBJ_PD_DOC",
                Name = "Документ состава проекта",
                AttributesPatternString =
                    RegexpStrings.ТипДокументаПД + @"\s*" +
                    RegexpStrings.ПолныйНомерПД + @"\s*(\-)?\s*" +
                    RegexpStrings.ОбозначениеПД + @"\s*" +
                    RegexpStrings.НомерИзмененияПД,
                AttributesPatterns = new List<AttributePatternClass> 
                { 
                    new AttributePatternClass("ATR_PD_PRJPART_TYPE"),
                    new AttributePatternClass("ATR_PD_FNUM"),
                    new AttributePatternClass("ATR_PD_TITLE")
                },
                ChildsPatterns = new List<DocumentPatternClass> { ИсходныеДокументыПД }
            };

            public static DocumentPatternClass РазделПД = new DocumentPatternClass
            {
                Id = "OBJ_PD_FOLDER",
                Name = "Раздел состава проекта",
                AttributesPatternString = RegexpStrings.ТипДокументаПД + @"\s*" + RegexpStrings.ПолныйНомерПД,
                AttributesPatterns = new List<AttributePatternClass> 
                { 
                    new AttributePatternClass("ATR_PD_PRJPART_TYPE"),
                    new AttributePatternClass("ATR_PD_FNUM")
                },
                ChildsPatterns = new List<DocumentPatternClass> { РазделПД, ДокументПД }
            };

            public static DocumentPatternClass ПроектнаяДокументацияНаОбъектыКапитальногоСтроительства = new DocumentPatternClass
            {
                Id = "OBJ_PRJCONT_PRJ",
                Name = "Проектная документация на объекты капитального строительства",
                AttributesPatternString = "",
                AttributesPatterns = new List<AttributePatternClass>(),
                ChildsPatterns = new List<DocumentPatternClass> { DocumentsPatterns.РазделПД }
            };
        }

        /// <summary>
        /// Словарь, ключами в котором являются Id документа, значениями - сами документы
        /// </summary>
        public static Dictionary<string, DocumentPatternClass> PatternsDictionary = new Dictionary<string, DocumentPatternClass>();

        /// <summary>
        /// Создание шаблонов документов и добавление их в словарь Patterns
        /// </summary>
        static PatternsLibrary()
        {
            //Рабочая документация
            PatternsDictionary.Add("DRAWING", DocumentsPatterns.Чертеж);
            PatternsDictionary.Add("PSD", DocumentsPatterns.Документ);
            PatternsDictionary.Add("COMPLECT", DocumentsPatterns.ОсновнойКомплект);
            PatternsDictionary.Add("DOC__MARK", DocumentsPatterns.Раздел);
            PatternsDictionary.Add("DOC_FOR_TITUL", DocumentsPatterns.КомплектСооружения);
            PatternsDictionary.Add("DOC_FOR_COMPLEX_PART", DocumentsPatterns.КомплектЧастиКомплекса);
            PatternsDictionary.Add("DOC_FOR_COMPLEX", DocumentsPatterns.Проект);
            //Проектная документация
            PatternsDictionary.Add("OBJ_NOTETEXT", DocumentsPatterns.ТекстоваяЧасть);
            PatternsDictionary.Add("OBJ_NOTEATT", DocumentsPatterns.Приложение);
            PatternsDictionary.Add("SOURCE", DocumentsPatterns.ИсходныеДокументыПД);
            PatternsDictionary.Add("OBJ_PD_DOC", DocumentsPatterns.ДокументПД);
            PatternsDictionary.Add("OBJ_PD_FOLDER", DocumentsPatterns.РазделПД);
            PatternsDictionary.Add("OBJ_PRJCONT_PRJ", DocumentsPatterns.ПроектнаяДокументацияНаОбъектыКапитальногоСтроительства);
        }
    }
}
