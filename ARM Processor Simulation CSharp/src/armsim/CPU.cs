//--------------------------------------------------------------------------------------------
//File:   CPU.cs
//Desc:   This file defines a class CPU which defines all the logic for the CPU actions and 
//          components.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace armsim
{
    public class CPU
    {
        /// <summary>
        /// contains logic for CPU class.
        /// </summary>
        public Memory CPU_Registers;//{ get; set; }
        public Memory CPU_RAM; //{ get; set; }

        public StringBuilder CPU_Console_Ref;
        public List<char> CPU_Input_Buff;

        public bool IRQ { get; set; }
     // need to change the return true statement

        public CPU(Memory ram, Memory reg)
        {
            CPU_RAM = ram;
            CPU_Registers = reg;
        }

        //Read word from RAM by val in PC reg
        public uint fetch() {
            uint val = (uint)CPU_RAM.ReadWord(PC);
            PC += 4;
            return val;
        }

        // Create an instruction
        public Instruction decode(uint instr) {
            Instruction inst = Instruction.CreateInstr(instr, CPU_Registers, CPU_RAM);

            inst.InstAddr = (uint)(PC - 4);
            inst.I_Console_Ref = CPU_Console_Ref;
            inst.I_Input_Buff = CPU_Input_Buff;

            return inst;
        }

        // Pause 1 quarter second
        public void execute(Instruction instr) {            
            if(CompCond(instr.Cond, (uint)CPSR))
                instr.Execute();                    //will need to test Flag instead.
        }

        //Test flags for Conditional execution
        bool CompCond(uint cond, uint flags) //will need to come fix this to use TestFlag instead
        {
            bool[] compared = {
                (Memory.ExtractBits(flags, 1, 1) == 1), (Memory.ExtractBits(flags, 1, 1) == 0), //eq, ne
                (Memory.ExtractBits(flags, 2, 2) == 1), (Memory.ExtractBits(flags, 2, 2) == 0), //cs, cc
                (Memory.ExtractBits(flags, 0, 0) == 1), (Memory.ExtractBits(flags, 0, 0) == 0), //mi, pl
                (Memory.ExtractBits(flags, 3, 3) == 1), (Memory.ExtractBits(flags, 3, 3) == 0), //vs, vc

                (Memory.ExtractBits(flags, 2, 2) == 1) && (Memory.ExtractBits(flags, 1, 1) == 0), //hi
                (Memory.ExtractBits(flags, 2, 2) == 0) || (Memory.ExtractBits(flags, 1, 1) == 1), //ls

                (Memory.ExtractBits(flags, 0, 0) == Memory.ExtractBits(flags, 3, 3)), //ge
                (Memory.ExtractBits(flags, 0, 0) != Memory.ExtractBits(flags, 3, 3)), //lt

                ((Memory.ExtractBits(flags, 1, 1) == 0) && (Memory.ExtractBits(flags, 0, 0) == Memory.ExtractBits(flags, 3, 3))), //gt
                ((Memory.ExtractBits(flags, 1, 1) == 1) || (Memory.ExtractBits(flags, 0, 0) != Memory.ExtractBits(flags, 3, 3)))  //le
            };

            if (cond < 14)
                return compared[cond];
            
            return true;
        }

        //Calls Exception Processing with the correct mode.
        public void Do_IRQProcessing()
        {
            Exception_Process(CPU_Registers, 0b10010);
        }

        //Processes all exceptions
        public static void Exception_Process(Memory reg, uint mode)
        {
            uint cpsr = (uint)GetRegr(reg, 16);
            int pc = GetRegr(reg, 15);

            pc -= 4;

            int modespsr = mode == 0b10010 ? 19 : 22;
            int vector_addr = mode == 0b10010 ? 0x18 : 0x8;

            SetReg(reg, modespsr, (int)cpsr); //save to spsr_mode;
            cpsr = ((cpsr >> 5) << 5) | mode; //Change mode bits to mode

            SetReg(reg, 16, (int)cpsr); //changed the order to do updating mode bits first
            SetReg(reg, 14, pc); //then update lr_mode  

            reg.SetFlag(16 * 4, 24, true);  //set I-bit in CPSR            
            SetReg(reg, 15, vector_addr); //set PC to address in table
        }

        //finds the offset in the registers array based on the current mode.
        static int getoffset(uint mode, int num)
        {
            int[] banked = { 13, 14 }; 
            //checking for mode requested and asking which one
            if (mode == 0b10011)
            {
                num = (banked.Contains(num)) ? num + 7 : num;

            }else if (mode == 0b10010)
            {
                num = (banked.Contains(num)) ? num + 4 : num;
            }
            return num;
        }

        //for getting register values;
        public static int GetRegr(Memory reg, int num){
            uint mode = Memory.ExtractBits((uint)reg.ReadWord(16 * 4), 27, 31);
            num = getoffset(mode, num);

            return num == 15 ? reg.ReadWord((num * 4)) + 4 :  reg.ReadWord((num * 4));
        }

        //for setting registers
        public static void SetReg(Memory reg, int nreg, int val){
            uint mode = Memory.ExtractBits((uint)reg.ReadWord(16 * 4), 27, 31);
            nreg = getoffset(mode, nreg);

            reg.WriteWord(val, (nreg * 4));
        }

        //--------------------For string processing of registers-------------------------
        static List<string> regs = new List<string>()
        {
            "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "r8",
            "r9", "r10", "r11", "ip", "sp", "lr", "pc", "CSPR",
            "sp_irq", "lr_irq", "SPSR_irq", "sp_svc", "lr_svc", "SPSR_svc"
        };

        public static string GetStrRegr(Memory reg, int ind)
        {
            uint mode = Memory.ExtractBits((uint)reg.ReadWord(16 * 4), 27, 31);
            ind = getoffset(mode, ind);           

            return regs[ind];
        }

        public static string GetStrRegr(int ind)
        { 
            return regs[ind];
        }

        //Register Properties
        public int IP { get { return CPU_Registers.ReadWord(0x30); } set { CPU_Registers.WriteWord(value, 0x30); } }
        public int SP { get { return CPU_Registers.ReadWord(0x34); } set { CPU_Registers.WriteWord(value, 0x34); } }
        public int R14 { get { return CPU_Registers.ReadWord(0x38); } set { CPU_Registers.WriteWord(value, 0x38); } }
        public int PC { get { return CPU_Registers.ReadWord(0x3C); } set { CPU_Registers.WriteWord(value, 0x3C); } }
        public int CPSR { get { return CPU_Registers.ReadWord(0x40); } set { CPU_Registers.WriteWord(value, 0x40); } }

    }
}

