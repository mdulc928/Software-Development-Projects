//------------------------------------------------------------------------------
//File:   Program.cs
//Desc:   This program displays the metadata in a PNG image file.
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace prog2
{
    class Program
    {

        static void Main(string[] args)
        {
            //Check length of argument
            if (args.Length != 1)
            {
                Console.WriteLine("\nYou did not supply an appropriate filename. Usage: <programName> <fileName>");
                Environment.Exit(1);
            }

            string thisFileName = args[0];

            //Check to see if file exists
            if (!File.Exists(thisFileName))
            {
                Console.WriteLine("The file you provided does not exist in the current directory.");
                Environment.Exit(1);
            }

            try {
                //guarding the file potentially not being a png file

                PNGFile fileChunks = PNGFile.Load(thisFileName);

                //Now display the metadata
                Console.WriteLine("\nMetadata in {0}:\n", thisFileName);
                foreach (PNGChunk pngChunk in fileChunks.PngChunks)
                {
                    string metadata;
                    if (pngChunk.ChunkType == "tEXt")
                    {
                        metadata = pngChunk.ExtractMData();
                        Console.WriteLine(metadata);
                    }
                }
            }
            catch(ArgumentException argExcep)
            {
                Console.WriteLine(argExcep.Message);
            }          

        }

    }
}
