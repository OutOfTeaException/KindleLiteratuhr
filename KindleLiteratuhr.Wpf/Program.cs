﻿using System;

namespace KindleLiteratuhr.Wpf
{
    class Program
    {
        private const string TARGET_DIR = @"c:\temp\images";
        private const string TIMEDATA_FILE = @"..\..\..\Resources\litclock_annotated.csv";
        
        static void Main(string[] args)
        {
            var imageGenerator = new KindleImageGenerator(TIMEDATA_FILE, TARGET_DIR);
            imageGenerator.GenerateImages();
        }
    }
}
