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

    public class Pos
    {
        private readonly IConsoleOutput _consoleOutput;

        public Pos(IBarcodeReader barcodeReader, IConsoleOutput consoleOutput)
        {
            _consoleOutput = consoleOutput;
            barcodeReader.OnBarcode += HandleBarcode;
        }

        private void HandleBarcode(string barcode)
        {
            _consoleOutput.WriteLine("$9.50");
        }
    }

    public class Class1
    {
        [Test]
        public void ShouldReturnNineFifty()
        {
            var barcodeReader = Substitute.For<IBarcodeReader>();
            var consoleOutput = Substitute.For<IConsoleOutput>();

            new Pos(barcodeReader, consoleOutput);

            barcodeReader.OnBarcode += Raise.Action("123");

            consoleOutput.Received().WriteLine("$9.50");
        }

    }
}
