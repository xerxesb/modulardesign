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

    public interface ICatalog
    {
        bool ContainsBarcode(string barcode);
        string FindPrice(string barcode);
    }

    public class Catalog : ICatalog
    {
        private readonly IDictionary<string, string> _pricesByProduct;

        public Catalog(IDictionary<string, string> pricesByProduct)
        {
            _pricesByProduct = pricesByProduct;
        }

        public bool ContainsBarcode(string barcode)
        {
            return _pricesByProduct.ContainsKey(barcode);
        }

        public string FindPrice(string barcode)
        {
            return _pricesByProduct[barcode];
        }

    }

    public interface ISaleView
    {
        void DisplayPrice(string price);
        void DisplayProductNotFound(string barcode);
        void DisplayNoBarcodeProvided();
    }

    public class SaleView : ISaleView
    {
        private readonly IConsoleOutput _consoleOutput;

        public SaleView(IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;
        }

        public void DisplayPrice(string price)
        {
            _consoleOutput.WriteLine(price);
        }

        public void DisplayProductNotFound(string barcode)
        {
            _consoleOutput.WriteLine("Product code " + barcode + " not found");
        }

        public void DisplayNoBarcodeProvided()
        {
            _consoleOutput.WriteLine("No barcode was provided");
        }

    }

    public class SaleController
    {
        private readonly ISaleView _saleView;
        private readonly ICatalog _catalog;

        public SaleController(IBarcodeReader barcodeReader, IConsoleOutput consoleOutput,
                              IDictionary<string, string> pricesByProduct) : this(barcodeReader, new SaleView(consoleOutput), new Catalog(pricesByProduct))
        {
        }

        public SaleController(IBarcodeReader barcodeReader, ISaleView saleView, ICatalog catalog)
        {
            _saleView = saleView;
            _catalog = catalog;
            barcodeReader.OnBarcode += HandleBarcode;
        }

        private void HandleBarcode(string barcode)
        {
            // this refactor means pulling pout a higher class to perform validation and another controller would use these two classes as collaborators. 
            // the salecontroller doesnt know anything about validation.
            if (String.IsNullOrEmpty(barcode))
            {
                _saleView.DisplayNoBarcodeProvided();
                return;
            }

            if (!_catalog.ContainsBarcode(barcode))
            {
                _saleView.DisplayProductNotFound(barcode);
                return;
            }

            _saleView.DisplayPrice(_catalog.FindPrice(barcode));
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

            new SaleController(barcodeReader, consoleOutput, (IDictionary<string, string>)null);

            barcodeReader.OnBarcode += Raise.Action("");

            consoleOutput.Received().WriteLine("No barcode was provided");
        }

    }
}
