using NUnit.Framework.Internal;

namespace Inix.Tests
{
#pragma warning disable CS8602
#pragma warning disable CS8600

    public class Tests
    {
        private readonly InixLoader loader = new();

        [SetUp]
        public void Setup()
        {
            loader.parse("Test Data\\custom.ini");

            loader.printDictionary();
        }

        [Test]
        public void TestParsing()
        {
            Assert.Pass();
        }

        [Test]
        public void TestComment()
        {
            //There is 4 global comments in total, therefore we can check each
            //using the brackets [] operator.

            Assert.That(loader.getComment(1).rawData, Is.EqualTo("; global comment 1"));
            Assert.That(loader.getComment(2).rawData, Is.EqualTo("// global comment 2"));
            Assert.That(loader.getComment(3).rawData, Is.EqualTo("; global comment 3"));
            Assert.That(loader.getComment(4).rawData, Is.EqualTo("// global comment 4"));
        }

        [Test]
        public void TestHeaderCount()
        {
            //4 comments & 2 headers == 6
            Assert.That(loader.objectCount() == 6);
        }

        [Test]
        public void TestFruitsHeader()
        {
            Assert.That(loader.containsHeader("FRUITS"));

            Assert.That(loader["FRUITS"].properties.Count, Is.EqualTo(5));
        }

        [Test]
        public void TestFruitsProperties()
        {
            Dictionary<string, InixProperty> fruitProps = loader["FRUITS"].properties;

            Assert.That(fruitProps["APPLE"].value == "1");
            Assert.That(fruitProps["ORANGE"].value == "2");
            Assert.That(fruitProps["BANANA"].value == "3");
            Assert.That(fruitProps["KIWI"].value == "4");
            Assert.That(fruitProps["PASSIONFRUIT"].value == "5");
        }

        [Test]
        public void TestGamesHeader()
        {
            Assert.That(loader.containsHeader("GAMES"));

            Assert.That(loader["GAMES"].properties.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestGamesProperties()
        {
            Dictionary<string, InixProperty> gameProps = loader["GAMES"].properties;

            //Check properties are intact
            Assert.That(gameProps["COD"].value == "Call of Duty");
            Assert.That(gameProps["APEX"].value == "Apex Legends");

            //Check comments are intact
            Assert.That(gameProps["COD"].comment == "This is call of duty.");
            Assert.That(gameProps["APEX"].comment == "This is apex legends.");
        }

        [Test]
        public void TestGameHeaderComment()
        {
            InixLogger.log(loader["GAMES"].rawData);
            Assert.That(loader["GAMES"].rawData == "This is a header comment.");
        }

        [Test]
        public void TestReconstruction()
        {
            InixLogger.log(loader.ToString());

            Assert.Pass();
        }
    }
}