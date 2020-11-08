//------------------------------------------------------------------------------
//File:   PNGFileTest.cs
//Desc:   This program contains tests for the Load() method of the PNGFile class.
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;


[TestFixture]
public class PNGFileTest
{
    [Test]
    public void Load_ValidPNG_ChunksLoadedCorrectly()
    {
        string fileName = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "ice.png");
        string[] chunkNames = new string[] {"IHDR", "gAMA", "bKGD", "IDAT", "IDAT", "IDAT", "tEXt", "tEXt", "tEXt", "IEND"};

        PNGFile pngChunkCol = PNGFile.Load(fileName);

        Assert.True(pngChunkCol.PngChunks.Count == 10);
        for (int i = 0; i < pngChunkCol.PngChunks.Count; i++)
        {
            Assert.True(pngChunkCol.PngChunks[i].ChunkType == chunkNames[i]);
        }
    }

    [Test]
    public void ValidateSignature_InvalidPngFile_DoesNotLoad()
    {
        string fileName = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "shovel.jpeg");
        try
        {
            PNGFile pngChunkCol = PNGFile.Load(fileName);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
        }
    }
}
