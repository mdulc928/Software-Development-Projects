//------------------------------------------------------------------------------
//File:   PNGFileReader.cs
//Desc:   This program defines a class PNGFileReader that returns a single chunk
//         of a PNG image file.
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


class PNGFileReader: IDisposable
{
    private BinaryReader fileReader;

    //Constructor that initializes a BinaryReader obj from the
    //in the `pngFile` parameter, validating that the file is truly a PNG file
    public PNGFileReader(string pngFile)
    {
        fileReader = new BinaryReader(File.Open(pngFile, FileMode.Open));

        byte[] pngSignature = fileReader.ReadBytes(8);
        ValidateSignature(pngSignature);
    }

    public PNGChunk ReadChunk()
    {
        PNGChunk chunk = new PNGChunk();

        byte[] lengthArray = new byte[4];
        byte[] typeArray = new byte[4];
        byte[] dataArray = new byte[] { };
        byte[] crcArray = new byte[4];

        //Read length and assign it to the ChunkLength
        lengthArray = fileReader.ReadBytes(4);
        chunk.ChunkLength = ReadLength(lengthArray);

        //Read the chunk type and assign it to the ChunkType
        typeArray = fileReader.ReadBytes(4);
        chunk.ChunkType = ReadType(typeArray);

        //Read the chunk data and assign it to the ChunkData
        chunk.ChunkData = fileReader.ReadBytes((int)chunk.ChunkLength);


        //Read the crc and assign it the Crc
        crcArray = fileReader.ReadBytes(4);
        chunk.Crc = ReadLength(crcArray);       //uses same code as the length because it is a 32-bit integer.

        return chunk;
    }

    //Reads a byte array from the param and returns an integer that contains the length of the chunk data
    private static uint ReadLength(byte[] chunkLength)
    {
        uint length = 0u;

        length = (uint)((chunkLength[0] << 24) + ((uint)chunkLength[1] << 16) + ((uint)chunkLength[2] << 8) + ((uint)chunkLength[3] << 0));

        return length;
    }

    //Reads a byte array and returns the string containing the chunk type
    private static string ReadType(byte[] chunkType)
    {
        string chunkTyp = "";

        for (int i = 0; i < 4; i++)
        {
            chunkTyp += (char)chunkType[i];
        }

        return chunkTyp;
    }

    //Checks to see if the first 8 bytes of the `signature` are those of a PNG file
    private static void ValidateSignature(byte[] signature)
    {
        List<int> sequence = new List<int> { 137, 80, 78, 71, 13, 10, 26, 10 };
        for (int i = 0; i < signature.Length; i++)
        {
            if (sequence[i] != signature[i])
            {
                throw new ArgumentException("\nThis is not a valid png file.");
            }
        }
    }

    //Implement the Dispose method of IDisposable and closes the BinaryReader
    public void Dispose()
    {
        fileReader.Close();
    }
}

