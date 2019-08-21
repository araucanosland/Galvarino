using NUnit.Framework;
using TinyCsvParser;
using Galvarino.Web.Models.Parser;
using System.Text;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class TyniCSVUnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(false, ';');
            CsvCreditosRecibidosMapping csvMapper = new CsvCreditosRecibidosMapping();
            CsvParser<CreditosRecibidos> csvParser = new CsvParser<CreditosRecibidos>(csvParserOptions, csvMapper);

            var result = csvParser
                .ReadFromFile(@"C:\galvarino\entradas_ftp\Rpt_LA_CRED_Recepcionados.csv", Encoding.ASCII)
                .ToList();

            Assert.AreEqual(107990, result.Count);

            Assert.IsTrue(result.All(x => x.IsValid));

        }
    }
}