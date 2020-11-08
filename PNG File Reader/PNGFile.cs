//--------------------------------------------------------------------------------------------
//File:   PNGFile.cs
//Desc:   This program defines a class PNGFile, which contains a method Load() to load
//         each chunk of a PNG image file seperately to a list of chunks.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


class PNGFile
{
    private List<PNGChunk> pngChunks;

    public PNGFile()
    {
        pngChunks = new List<PNGChunk> { };
    }

    public List<PNGChunk> PngChunks
    {
        get
        {
            return pngChunks;
        }
        set
        {
            pngChunks = value;
        }    
    }

    //Loads the chunks of the PNG file from the `filename` in the parameter and returns PNGFile object
    public static PNGFile Load(string fileName)
    {
        PNGFile pngChunkCol = new PNGFile();

        using (PNGFileReader pngReader = new PNGFileReader(fileName))
        {

            PNGChunk pChunk = pngReader.ReadChunk(); ;
            while (true)
            {
                if (pChunk.ChunkType == "IEND")
                {
                    pngChunkCol.PngChunks.Add(pChunk);
                    break;
                }
                else
                {
                    pngChunkCol.PngChunks.Add(pChunk);
                    pChunk = pngReader.ReadChunk();
                }    
            }
        }
        return pngChunkCol;
    }
    
}
