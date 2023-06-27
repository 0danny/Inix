using NUnit.Framework.Internal;

namespace Inix.Tests
{
#pragma warning disable CS8602
#pragma warning disable CS8600

    public class Tests
    {
        private readonly InixLoader loader = new();
        private InixFile iniFile { get; set; }

        [SetUp]
        public void Setup()
        {
            iniFile = loader.parse("Test Data\\custom.ini");

            iniFile.printDictionary();
        }

        [Test]
        public void TestParsing()
        {
            Assert.That(iniFile.errors.Count == 0);
        }

        [Test]
        public void TestComment()
        {
            //There is 4 global comments in total, therefore we can check each
            //using the brackets [] operator.

            Assert.That(iniFile.getComment(1).comment, Is.EqualTo("; global comment 1"));
            Assert.That(iniFile.getComment(2).comment, Is.EqualTo("// global comment 2"));
            Assert.That(iniFile.getComment(3).comment, Is.EqualTo("; global comment 3"));
            Assert.That(iniFile.getComment(4).comment, Is.EqualTo("// global comment 4"));
        }

        [Test]
        public void TestHeaderCount()
        {
            //4 comments & 2 headers == 6
            Assert.That(iniFile.objectCount() == 6);
        }

        [Test]
        public void TestFruitsHeader()
        {
            Assert.That(iniFile.containsHeader("FRUITS"));

            Assert.That(iniFile["FRUITS"].properties.Count, Is.EqualTo(5));
        }

        [Test]
        public void TestFruitsProperties()
        {
            Dictionary<string, InixProperty> fruitProps = iniFile["FRUITS"].properties;

            Assert.That(fruitProps["APPLE"].value == "1");
            Assert.That(fruitProps["ORANGE"].value == "2");
            Assert.That(fruitProps["BANANA"].value == "3");
            Assert.That(fruitProps["KIWI"].value == "4");
            Assert.That(fruitProps["PASSIONFRUIT"].value == "5");
        }

        [Test]
        public void TestGamesHeader()
        {
            Assert.That(iniFile.containsHeader("GAMES"));

            Assert.That(iniFile["GAMES"].properties.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestGamesProperties()
        {
            Dictionary<string, InixProperty> gameProps = iniFile["GAMES"].properties;

            //Check properties are intact
            Assert.That(gameProps["COD"].value == "Call of Duty");
            Assert.That(gameProps["APEX"].value == "Apex Legends");

            //Check comments are intact
            Assert.That(gameProps["COD"].comment == "This is call of duty. ; Secondary comment check");
            Assert.That(gameProps["APEX"].comment == "This is apex legends.");
        }

        [Test]
        public void TestGameHeaderComment()
        {
            InixLogger.log(iniFile["GAMES"].comment);
            Assert.That(iniFile["GAMES"].comment == "This is a header comment.");
        }

        [Test]
        public void TestReconstruction()
        {
            InixLogger.log(iniFile.ToString());

            Assert.Pass();
        }
    }
}