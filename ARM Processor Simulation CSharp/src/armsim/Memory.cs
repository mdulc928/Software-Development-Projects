//--------------------------------------------------------------------------------------------
//File:   Memory.cs
//Desc:   This file defines a class Memory that contains logic for representing Computer Memory.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection.Emit;

namespace armsim
{
    public class Memory
    {
        //TraceSwitch memTrace = new TraceSwitch("MemoryTrace", "Switch for the memory class");
        private byte[] ram;

        public byte[] Cells { get { return ram; } }  //property for the class to use.

        //Constructor for RAM Simulation
        public Memory(int size)
        {
            ram = new byte[size];
        }

        //Validates that address is appropriate; type: 0 =short(16), 1=word(32)
        bool IsVldAddr(int addr, int type)
        {
            if (type == 0) { return addr % 2 == 0; }
            return addr % 4 == 0;
        }

        // All 3 receive a 32-bit address and returns the number of bits requested that are currently in the address, or -1 for incorrect address
        public int ReadWord(int addr)
        { //Question: what happens when I read the end of the file? Should I validate that the address in the memory location(?)
            if (IsVldAddr(addr, 1))
            {
                return (int)((Cells[addr + 3] << 24) + (Cells[addr + 2] << 16) + (Cells[addr + 1] << 8) + Cells[addr]);
            }
            return -1;
        }
        public short ReadHalfWord(int addr)
        {
            if (IsVldAddr(addr, 0))
            {
                return (short)((Cells[addr + 1] << 8) + Cells[addr]);
            }
            return 0;
        }
        public byte ReadByte(uint addr) { return ram[addr]; } //Occured to me that this could be used to read bytes ;)

        // All 3 receive a 32-bit address and  the number of bits requested that are currently in the address, or -1 for incorrect address
        //as long as there is space, this ok, but that is not always so. Must fix when writing unit tests. -Sone
        public void WriteWord(int val, int addr)
        {
            if (IsVldAddr(addr, 1))
            {
                int op = 0x000000FF;
                int byt;
                for (int i = 0; i < 4; ++i)
                {
                    byt = (val >> (8 * i)) & op;
                    WriteByte(Convert.ToByte(byt), addr + i);
                }
            }
        }
        public void WriteHalfWord(short val, int addr)
        {
            if (IsVldAddr(addr, 0))
            {
                int op = 0x000000FF;
                int byt2 = val & op;
                int byt1 = (val >> 8) & op;
                WriteByte(Convert.ToByte(byt2), addr);
                WriteByte(Convert.ToByte(byt1), addr + 1);
            }
        }
        public void WriteByte(byte val, int addr) { ram[addr] = val; }


        //Flags dealings
        public bool TestFlag(int addr, int bit)
        {
            int num = ReadWord(addr);
            return ((num >> (31 - bit)) & 0x00000001) == 1;
        }

        public void SetFlag(int addr, int bit, bool flag)
        {
            int num = ReadWord(addr);
            int flagged = flag ? num | (0x0000001 << (31 - bit)) : num & (~(0x0000001 << (31 - bit)));

            WriteWord(flagged, addr);
            
        }

        //Extracts bits from number
        public static uint ExtractBits(uint word, uint startBit, uint endBit)
        {
            word = word << (int)startBit;
            word = word >> (int)((31 - endBit) + startBit);
           
            return word;
        }

        //Computes Checksum of memory cells
        public int CheckSum(byte[] mem)
        {
            int cksum = 0;

            for (int i = 0; i < mem.Length; ++i)
            {
                cksum += mem[i] ^ i;
            }
            return cksum;
        }
    }

}
