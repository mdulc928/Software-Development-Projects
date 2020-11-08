using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using armsim;
using System.Security.Cryptography.X509Certificates;

[TestFixture]
public class ARMSimTests
{
    public Dictionary<uint, string> Instrs = new Dictionary<uint, string>(){
            { 0xe3a014a1, "mov\tr1, #-1593835520"}, { 0xe1a0200f, "mov\tr2, pc"},
            { 0xe1a02141, "mov\tr2, r1, asr #2"},   { 0xe1a02121, "mov\tr2, r1, lsr #2"},
            { 0xe1a02081, "mov\tr2, r1, lsl #1"},   { 0xe1a02260, "mov\tr2, r0, ror #4"},
            { 0xe3e03001, "mvn\tr3, #1"},           { 0xe2802003, "add\tr2, r0, #3"},
            { 0xe2445003, "sub\tr5, r4, #3"},       { 0xe2645003, "rsb\tr5, r4, #3" },
            { 0xe20020ff, "and\tr2, r0, #255"},     { 0xe3802012, "orr\tr2, r0, #18"},
            { 0xe2202fb7, "eor\tr2, r0, #732"},     { 0xe3c020ff, "bic\tr2, r0, #255"},
            { 0xe3a02002, "mov\tr2, #2"},           { 0xe0050291, "mul\tr5, r1, r2"}, 
            { 0xef000011, "svc\t0x00000011"},       { 0xe8bd000a, "pop\t{r1, r3}"},
            { 0xe5021004, "str\tr1, [r2, #-4]" },   { 0xe7021004, "str\tr1, [r2, -r4]"},
            { 0Xe5c2100a, "strb\tr1, [r2, #10]" },  { 0xe78210c4, "str\tr1, [r2, r4, asr #1]"},
            { 0xe5126004, "ldr\tr6, [r2, #-4]" },   { 0xe7127004, "ldr\tr7, [r2, -r4]"},
            { 0xe92d0016, "push\t{r1, r2, r4}" },   { 0xe89d0016, "ldm\tsp, {r1, r2, r4}"},
            { 0xe5d2a00a, "ldrb\trl0, [r2, #10]"},   
        };
    //------------------------------------------------------------------------------------------
    [Test]
    public void LoadElFandCheckSum_UsesCheckSumToCheckRAMIsFilledCorrectly_IsFilledCorrectly()
    {
        Loader loder = new Loader();
        int mb = 32768, sum = 0;
        string[] files = new string[] { "test1.exe", "test2.exe", "test3.exe" };
        int[] expsizes = new int[] { 536861081, 536864433, 536861199 };

        for(int i = 0; i < files.Length; ++i)
        {
            Memory ram = new Memory(mb);
            loder.LoadElf(files[i], ram.Cells);
            sum = ram.CheckSum(ram.Cells);
            Assert.IsTrue( sum == expsizes[i]);
        }
    }

    [Test]
    public void ReadWrites_ChecksThatValueIsCorrectlyPlacedInRAM_IsCorrectlyPlaced()
    {
        int mb = 32768;
        int wd = 6592998;
        short sh = 300;
        byte bt = 255;
        int addr = 16;

        //------------Word--------------------
        Memory ram = new Memory(mb);

        ram.WriteWord(wd, addr);
        Assert.IsTrue(ram.ReadWord(addr) == wd);

        //---------HalfWord--------------------
        ram = new Memory(mb);

        ram.WriteHalfWord(sh, addr);
        Assert.IsTrue(ram.ReadHalfWord(addr) == sh);

        //-----------------Byte----------------------
        ram = new Memory(mb);

        ram.WriteByte(bt, addr);
        Assert.IsTrue(ram.ReadByte((uint)addr) == bt);
    }

    [Test]
    public void TestFlag_ChecksThatFlagWasSetCorrectly_FlagsAreSetCorrectly()
    {
        int addr = 16, mb = 32768;
        Memory ram = new Memory(mb);
        int b = 254;
        ram.WriteWord(b, addr);

        Assert.IsTrue(ram.TestFlag(addr, 31) == false);
        b = 255;
        ram.WriteWord(b, addr);
        Assert.IsTrue(ram.TestFlag(addr, 31) == true);
    }

    [Test]
    public void SetFlag_ChecksThatWasSettoTrue_FlagIsSetToTrue()
    {
        int addr = 16, mb = 32768;
        bool fl = true;
        Memory ram = new Memory(mb);
        int w = 254;

        ram.WriteWord(w, addr);
        ram.SetFlag(addr, 31, fl);
        Assert.IsTrue(ram.ReadWord(addr) == 255);
        fl = false;
        ram.SetFlag(addr, 31, fl);
        Assert.IsTrue(ram.ReadWord(addr) == 254);
    }

    [Test]
    public void ExtractBits_ChecksForProperExtractionOfBits_BitsCorrectlyExtracted()
    {
        uint num = 3, st = 31, ed = 31;
        Assert.IsTrue(Memory.ExtractBits(num, st, ed) == 1);
    }

    //---------------------------------------------------------------------------------------

    [Test]
    public void MOV_TestsIfOper2ValueIsPlacedInCorrectRegister_ValuePlacedCorrectly()
    {
        //This test was designed for the detailed design phase, so it is comprehensive.
        //The rest of the tests are more concise focusing on one functionality in a class.

        Memory reg = new Memory(17 * 4);
        Memory ram = new Memory(0);

        //----------------------------Test Rotated Immediate------------------------------------------------
        uint inst = 0xe3a02030;
        Instruction mov = Instruction.CreateInstr(inst, reg, ram);
        mov.Execute();

        Assert.IsTrue(reg.ReadWord((2 * 4)) == 48);
        Assert.IsTrue(mov.ToString() == "mov\tr2, #48");

        //----------------------------Test Register shift by Immediate------------------------------------------------
        //r2 = 0x30
        inst = 0xe1a08202; //mov r8, r2, lsl #4
        mov = Instruction.CreateInstr(inst, reg, ram);
        mov.Execute();

        Assert.IsTrue(reg.ReadWord((8 * 4)) == 0x300);
        Assert.IsTrue(mov.ToString() == "mov\tr8, r2, lsl #4");

        //----------------------------Test Register shift by Register------------------------------------------------
        //r2 = 0x30, r8 = 0x300
        CPU.SetReg(reg, 3, 0x4);
        inst = 0xe1a08318; //mov r8, r8, lsl r3
        mov = Instruction.CreateInstr(inst, reg, ram);
        mov.Execute();

        Assert.IsTrue(reg.ReadWord((8 * 4)) == 0x3000); //somewhere computing incorrectly
        Assert.IsTrue(mov.ToString() == "mov\tr8, r8, lsl r3");
    }

    //--------------------------------Barrel Shifter Test -------------------------------
    [Test]
    public void Compute_TestsShiftTypesForCorrectnessInInterpretation_CorrectlyOperatesOnGivenValue()
    {
        uint testnum = 0x320;
        int signed_testnum = -1;

        //LSL
        Assert.IsTrue(BarrelShift.Compute(0b00, testnum, 0x5) == 0x6400);

        //LSR
        Assert.IsTrue(BarrelShift.Compute(0b01, testnum, 0x5) == 0x19);
        //ASR
        Assert.IsTrue(BarrelShift.Compute(0b10, testnum, 0x5) == 0x19);
        Assert.IsTrue((int)BarrelShift.Compute(0b10, (uint)signed_testnum, 0x5) == -1);

        //ROR
        Assert.IsTrue(BarrelShift.Compute(0b11, testnum, 0x6) == 0x8000000c);
    }

    [Test]
    public void ToString_TestsForCorrectnessOfDisassembly_AssemblyIsCorrect()
    {

        Memory reg = new Memory(17 * 4);
        Memory ram = new Memory(0);

        Instruction inst;
        foreach(KeyValuePair<uint, string> p in Instrs)
        {
            inst = Instruction.CreateInstr(p.Key, reg, ram);
            Assert.IsTrue(inst.ToString() == p.Value);
        }
    }

    //-----------------------------------Instructions simplified Test--------------------------------
    [Test]
    public void Execute_TestsForCorrectnessInValuesOf_CorrectlyOperrates()
    {
        Memory reg = new Memory(17 * 4);
        Memory ram = new Memory(0);

        //int[] regnums = new int[] {0, 1, 2, 2, 2, 2, 2, 2, 3, 4, 2, 5, 5, 2, 2, 2, 2, 2, 5 }
        uint[] expVals = new uint[] { 0xa1000000, 0x1014, 0xE8400000, 0x28400000,
            0x42000000, 0x4000002D, 0xFFFFFFFE, 727, 1, 0xFFFFFFFF, 0xd4, 0x2d6, 0x8, 0x00000200,
            0x2, 0x42000000
        };
        CPU.SetReg(reg, 15, 0x1010);
        CPU.SetReg(reg, 0, 0x2d4);
        CPU.SetReg(reg, 4, 4);
        int i = 0;
        Instruction inst;
        foreach (KeyValuePair<uint, string> p in Instrs)
        {
            inst = Instruction.CreateInstr(p.Key, reg, ram);
            if (!inst.ToString().Contains("svc"))
            {
                inst.Execute();
                Assert.IsTrue((uint)CPU.GetRegr(reg, (int)inst.Rd) == expVals[i]);
            }
            ++i;
            if (!(i < expVals.Length))
                break;
        }
    }

    [Test]
    public void LoadStore_TestsIfValuesHaveBeenCorrectlyPlaced_ValuesCorrectlyPlaced()
    {
        Memory reg = new Memory(17 * 4);
        Memory ram = new Memory(Computer.MB_Size);

        uint[] setupinst = new uint[] { 0xe3a01efb, 0xe3a02a05, 0xe3a03a03, 0xe3a04008 };
        foreach(uint i in setupinst)
        {
            Instruction setup = Instruction.CreateInstr(i, reg, ram);
        }
        
        //uint[] toTest = new uint[] { 0xe5821000, 0xe5021004, 0xe7021004, 0xe7821004, 0xe78210c4, 0xe7821104 }

    }
}
