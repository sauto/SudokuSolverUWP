using System;
using Patagames.Ocr;
using Patagames.Ocr.Enums;

namespace OCRTest
{
    class Program
    {
        static readonly string filepath = @"C:\Users\yuuta\Desktop\ocr5.png";

        static void Main(string[] args)
        {
            using (var api = OcrApi.Create())
            {
                api.Init(Languages.Japanese);
                string plainText = api.GetTextFromImage(filepath);
                Console.WriteLine(plainText);
            }
            Console.ReadLine();
        }
    }
}
