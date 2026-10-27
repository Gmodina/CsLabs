namespace sem_1_lab_07_directory_tree;

using System;
using System.IO;

internal class Program
{
    // Current line in file
    private static int _lineIndex;

    // Current nesting level
    private static int _depth;

    // File content
    private static string[] _lines = null!;

    private static void Main()
    {
        _lines = File.ReadAllLines("tree.txt");

        Console.WriteLine(_lines[0]);

        //=1 заменено на ++ для улучшения читаемости
        _lineIndex++;

        PrintTree();

        // Убрано Console.ReadLine(); за ненадобностью
    }

    private static void PrintTree()
    {
        //добавлено -1 из-за выхода за пределы массива
        while (_lineIndex < _lines.Length-1)
        {
            var line = _lines[_lineIndex].Trim();
            
            //if заменён на switch для улучшения читаемости
            switch (line)
            {
                case "":
                    _lineIndex += 2;
                    continue;
                case "END":
                    _depth--;
                    _lineIndex++;
                    return;
            }

            if (line.StartsWith("BEGIN "))
            {
                var name = line.Substring(5);

                Console.Write(GetIndent());
                //добавлен блок для написания символа "└─" в конец ветки
                try
                {
                    if (_lines[_lineIndex + 2].Trim() == "END" && _lines[_lineIndex + 1].Trim() == "END")
                    {
                        Console.WriteLine("└─" + name);
                    }
                    else
                    {
                        Console.WriteLine("├─" + name);
                    }
                }
                catch
                {
                    Console.WriteLine("└─" + name);
                }
                

                _depth++;
                //перемещено внутрь цикла из-за возникновения бесконечной рекурсии
                _lineIndex++;
                PrintTree();
            }
        }
    }

    private static string GetIndent()
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