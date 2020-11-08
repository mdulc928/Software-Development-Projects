//------------------------------------------------------------------------------
//File:   PNGChunk.cs
//Desc:   This program defines a class PNGChunk which holds the information
//         for a single chunk of PNG image file.
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PNGChunk
{
    private uint chunkLength;
    private string chunkType;
    private byte[] chunkData;
    private uint crc;

    public PNGChunk()
    {
        chunkLength = 0u;
        chunkType = "";
        chunkData = new byte[] { };
        crc = 0u;

    }

    public uint ChunkLength
    {
        get
        {
            return chunkLength;
        }
        set
        {
            chunkLength = value;
        }
    }

    public string ChunkType
    {
        get
        {
            return chunkType;
        }
        set
        {
            chunkType = value;
        }
    }

    public byte[] ChunkData
    {
        get
        {
            return chunkData;
        }
        set
        {
            chunkData = value;
        }
    }

    public uint Crc
    {
        get
        {
            return crc;
        }
        set
        {
            crc = value;
        }
    }

    //Extracts the metadata in the ChunkData and returns it as a string
    public string ExtractMData()
    {
        string keyWrdsValues = "";

        foreach (byte b in ChunkData)
        {
            if ((char)b == '\0')
            {
                keyWrdsValues += ": ";
            }
            else
            {
                keyWrdsValues += (char)b;
            }  
        }
        return keyWrdsValues;
    }
}