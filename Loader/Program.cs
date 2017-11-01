using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DTS.DAL.DataModel;

namespace Loader
{
    [Flags]
    public enum ExecMode { Check = 1, Load = 2 };

    class Program
    {
        public const string string_line = "-------------------------------------------------------------------------------------------";

        static void Main(string[] args)
        {
            //Определение парметров запуска программы
            if (args.Length == 0)
            {
                Console.WriteLine("Необходимо указать командный файл в строке параметров");
                return;
            }
            //Получение файла команд
            string filename = args[0];
            if (!File.Exists(filename))
            {
                Console.WriteLine("Файл {0} не найден по пути {1}", filename, Directory.GetCurrentDirectory());
                return;
            }
            //Считывание файла команд
            string[] lines = File.ReadAllLines(filename);
            if (lines.Length == 0)
            {
                Console.WriteLine("Файл {0} пустой", filename);
                return;
            }
            //Создание/очистка файла лога
            string log_filename = Path.ChangeExtension(filename, ".log");
            if (File.Exists(log_filename))
            {
                File.Delete(log_filename);
            }
            //Выполнение последовательности команд
            Console.WriteLine("Выполнение файла {0}", filename);
            List<Operation> operations = new List<Operation>();
            foreach (string line in lines)
            {
                Console.WriteLine("Выполнение команды {0}", line);
                Operation operation = new Operation();
                //Извлечь операцию из строки
                if (!operation.TryParse(line))
                {
                    Console.WriteLine(operation.Result.Description);
                    return;
                }
                //Если это не первая операция в последовательности, то передаем значение Document из предыдущей операции
                if (operations.Count > 0)
                {
                    operation.Document = operations.Last().Document;
                }
                //Добавляем операцию в последовательность операций
                operations.Add(operation);
                //Выполняем команду
                operation.Execute();
                //Записываем результат
                operation.Write(log_filename);
                //Заканчиваем выполнение команд, если выполнение последней было неуспешно
                if (operation.Result.Code != 0) break;
            }
            //Console.ReadKey();            
        }
    }
}
