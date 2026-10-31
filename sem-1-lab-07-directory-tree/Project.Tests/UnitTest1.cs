namespace Project.Tests;

using sem_1_lab_07_directory_tree;
using System.IO;
using NUnit.Framework;

public class TreeBuilderTests
{
    [Test]
    public void BuildTree_SimpleTree_ReturnsCorrectFormat()
    {
        string[] inputLines =
        [
            "A",
            "BEGIN B",
            "END",
            "END"
        ];

        var builder = new TreeBuilder(inputLines);

        string actualResult = builder.BuildTree();

        string expectedResult = "A\r\n└─ B\r\n";
            
            // В NUnit для сравнения строк с учетом переносов лучше использовать:
            Assert.That(actualResult, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void BuildTree_ComplexTree_ReturnsCorrectFormat()
    {
        string[] inputLines =
        [
            "A",
            "BEGIN B",
            "BEGIN C",
            "END",
            "BEGIN D",
            "END",
            "END",
            "BEGIN E",
            "END",
            "END"
        ];
        
        var builder = new TreeBuilder(inputLines);
        
        string actualResult = builder.BuildTree();

        string expectedResult =
            "A\r\n├─ B\r\n│ ├─ C\r\n│ └─ D\r\n└─ E\r\n";
        
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void BuildTree_EmptyArray_ReturnsCorrectFormat()
    {
        string[] inputLines = [];
        
        var builder = new TreeBuilder(inputLines);
        string actualResult = builder.BuildTree();
        string expectedResult = "";
        
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void BuildTree_CorruptedArray_ReturnsError()
    {
        string[] inputLines = 
        [
            "A",
            "BEGIN B",
            "BEGIN C",
            "END",
            "END",
            "END",
            "BEGIN D"
        ];
        
        var builder = new TreeBuilder(inputLines);
        string actualResult = builder.BuildTree();
        string expectedResult = "Неправильный формат файла";
        
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }
}