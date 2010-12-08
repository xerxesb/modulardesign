using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace PointOfSaleTests
{
    public interface IBarcodeReader
    {
        event Action<string> OnBarcode;
    }

    public class BarcodeReader : IBarcodeReader
    {
        public event Action<string> OnBarcode;
    }

    public interface IConsoleOutput
    {
        void WriteLine(string content);
    }

    public class ConsoleOutput : IConsoleOutput
    {
        public void WriteLine(string content)
        {
            Console.WriteLine(content);
        }
    }

    public class SaleController
    {
        private readonly IConsoleOutput _consoleOutput;
        private readonly IDictionary<string, string> _pricesByProduct;

        public SaleController(IBarcodeReader barcodeReader, IConsoleOutput consoleOutput, IDictionary<string, string> pricesByProduct)
        {
            _consoleOutput = consoleOutput;
            _pricesByProduct = pricesByProduct;
            barcodeReader.OnBarcode += HandleBarcode;

            //if (pricesByProduct == null)
            //{
            //    throw new ArgumentNullException("pricesByProduct");
            //}
        }

        private void HandleBarcode(string barcode)
        {
            if (String.IsNullOrEmpty(barcode))
            {
                _consoleOutput.WriteLine("No barcode was provided");
                return;
            }

            if (!_pricesByProduct.ContainsKey(barcode))
            {
                _consoleOutput.WriteLine("Product code " + barcode + " not found");
                return;
            }

            _consoleOutput.WriteLine(_pricesByProduct[barcode]);
        }
    }

    public class Class1
    {
        [Test]
        public void Test_123Returns950()
        {
            var barcodeReader = Substitute.For<IBarcodeReader>();
            var consoleOutput = Substitute.For<IConsoleOutput>();

            var pricesByProduct = new Dictionary<string, string> { { "123", "$9.50" } };
            new SaleController(barcodeReader, consoleOutput, pricesByProduct);

            barcodeReader.OnBarcode += Raise.Action("123");

            consoleOutput.Received().WriteLine("$9.50");
        }

        [Test]
        public void Test_WhenNotFoundReturnsNoProductAvailable()
        {
            var barcodeReader = Substitute.For<IBarcodeReader>();
            var consoleOutput = Substitute.For<IConsoleOutput>();

            var pricesByProduct = new Dictionary<string, string>();
            new SaleController(barcodeReader, consoleOutput, pricesByProduct);

            barcodeReader.OnBarcode += Raise.Action("999");

            consoleOutput.Received().WriteLine("Product code 999 not found");
        }

        [Test]
        public void Test_WhenNoBarcodeProvided()
        {
            var barcodeReader = Substitute.For<IBarcodeReader>();
            var consoleOutput = Substitute.For<IConsoleOutput>();

            var pricesByProduct = new Dictionary<string, string>();
            new SaleController(barcodeReader, consoleOutput, pricesByProduct);

            barcodeReader.OnBarcode += Raise.Action("");

            consoleOutput.Received().WriteLine("No barcode was provided");
        }


        [Test]
        public void Test_WhenNoBarcodeProvidedAndNoLookupProvided()
        {
            var barcodeReader = Substitute.For<IBarcodeReader>();
            var consoleOutput = Substitute.For<IConsoleOutput>();

            new SaleController(barcodeReader, consoleOutput, null);

            barcodeReader.OnBarcode += Raise.Action("");

            consoleOutput.Received().WriteLine("No barcode was provided");
        }

    }
}
