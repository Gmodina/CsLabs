using System.Text;

namespace sem_1_lab_07_directory_tree;

using System;
using System.IO;

public static class Program
{
    private static void Main()
    {
        string[]  lines = File.ReadAllLines("tree.txt");
        var printer = new TreeBuilder(lines);
        Console.WriteLine(printer.BuildTree());
    }
}

    //добавлен класс для тестирования и улучшения читаемости 
   public class TreeBuilder(string[] lines)
    {
        // Current line in file
        private int _lineIndex;

        // Current nesting level
        private int _depth;

        private readonly StringBuilder _result = new();

        public string BuildTree()
        {
            _result.AppendLine(lines[0]);
            _lineIndex = 1;
            PrintTree();
            return _result.ToString();
        }
        
        private void PrintTree()
        {
            //добавлено -1 из-за выхода за пределы массива
            while (_lineIndex < lines.Length-1)
            {
                var line = lines[_lineIndex].Trim();
            
                //if заменён на switch для улучшения читаемости
                switch (line)
                {
                    case "":
                        //+=2 заменено на +=1 для корректного пропуска строки
                        _lineIndex += 1;
                        continue;
                    case "END":
                        _depth--;
                        _lineIndex++;
                        return;
                }

                if (line.StartsWith("BEGIN "))
                {
                    var name = line.Substring(5);

                    _result.Append(GetIndent());
                    //добавлен блок для написания символа "└─" в конец ветки
                    if (_lineIndex + 2 <= lines.Length-1)
                    {
                        if (lines[_lineIndex + 2].Trim() == "END" && lines[_lineIndex + 1].Trim() == "END")
                        {
                            _result.AppendLine("└─" + name);
                        }
                        else
                        {
                            _result.AppendLine("├─" + name);
                        }
                    }
                    else
                    {
                        _result.AppendLine("└─" + name);
                    }
                

                    _depth++;
                    //перемещено внутрь цикла из-за возникновения бесконечной рекурсии
                    _lineIndex++;
                    PrintTree();
                }
            }
        }
        
        private string GetIndent()
        {
            var result = "";

            //i = 0 исправлено на i=1 из-за неправильной отрисовки глубины древа
            for (var i = 1; i <= _depth; i++)
            {
                result += "│ ";
            }

            return result;
        }
    }