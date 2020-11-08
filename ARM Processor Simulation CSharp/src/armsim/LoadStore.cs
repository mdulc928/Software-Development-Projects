//--------------------------------------------------------------------------------------------
//File:   LoadStore.cs
//Desc:   This file defines a class LoadStore that contains logic for the LoadStore instructions.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    public class LoadStore : Instruction
    {
        /// <summary>
        /// Defines attributes and methods for LoadStore
        /// </summary>
        /// 

        string[] StrInstr = new string[] { "str", "ldr", "strb", "ldrb", "stm", "ldm", "push", "pop" };

        public const int TYPE = 0b010;
        public uint typ, P, U, B, W, L, Rn; //Rd inherited
        public bool I;

        public Offset LsOffset;
        public List<int> Reglist;

        public int indx = 0;

       // Extracts the bits Executes needs to run and stores them in variables.
        public override void DecodeInst() {
            typ = Memory.ExtractBits(Inst, 4, 6);
            I = Convert.ToBoolean(Memory.ExtractBits(Inst, 6, 6));
            P = Memory.ExtractBits(Inst, 7, 7);
            U = Memory.ExtractBits(Inst, 8, 8);
            B = Memory.ExtractBits(Inst, 9, 9);
            W = Memory.ExtractBits(Inst, 10, 10);
            L = Memory.ExtractBits(Inst, 11, 11);
            Rn = Memory.ExtractBits(Inst, 12, 15);

            if(typ != 4)
            {
                Rd = Memory.ExtractBits(Inst, 16, 19);
                LsOffset = Offset.GetOffset(I, Memory.ExtractBits(Inst, 20, 31));
                LsOffset.Offset_Regs = I_Reg;
                LsOffset.U = (int)U;
            }
            else
            {
                Reglist = GetList(Inst);
                L += 4;         //----------------------------See comment below about index
            }
        }

        public virtual void Execute(uint Rn, List<int> reglist) {; }
        public virtual void Execute(uint Rn, uint Rd, Offset offst) {; }

        public override void Execute() {
            DecodeInst();

            List<LoadStore> instr = new List<LoadStore>() {  new STR(), new LDR(),  new STM(), new LDM() };
            
            LoadStore ls = instr[(int)(L < 4 ? L : L - 2)]; //--------------------using L to index may not be the best idea-
            ls.I_RAM = I_RAM;
            ls.I_Reg = I_Reg;

            ls.I_Console_Ref = I_Console_Ref;
            ls.I_Last_Char = I_Last_Char;
            ls.I_Input_Buff = I_Input_Buff;

            ls.P = P; ls.U = U; ls.B = B; ls.W = W;   

            if (typ != 4)
            {
                ls.Execute(Rn, Rd, LsOffset);
            }
            else
            {
                ls.Execute(Rn, Reglist);
            }
        }

        public override string ToString()
        {
            DecodeInst();                 //------FD(IA)---------------   ----------FD(DB)----------
            bool pushpop = (Rn == 13) && ((P == 0 && U == 1 && W == 1) || (P == 1 && U == 0 && W == 1));
            int indx = (int)(L < 4 ? (L + (B == 1 ? 2 : B)) : (L + (!pushpop ? 0 : 2))); 
            ASMRepr = StrInstr[indx] + (Cond < 0b1110 ? CondSufx[Cond] : "") + "\t";

            if (typ != 4)
            {
                ASMRepr += CPU.GetStrRegr(I_Reg, (int)Rd) + ", [" + CPU.GetStrRegr(I_Reg, (int)Rn) + LsOffset.ToString() + "]" + (W == 1 ? "!" : "");
            }
            else
            {
                string strReglist = "";
                for (int i = 0; i < Reglist.Count; ++i)
                    strReglist += (i == 0 ? "" : ", ") + CPU.GetStrRegr(I_Reg, Reglist[i]);

                ASMRepr += (pushpop ? "" : CPU.GetStrRegr(I_Reg, (int)Rn) + (W == 1 ? "!" : "") + ", ") + "{" + strReglist + "}";
            }
            return ASMRepr;
        }

        public int GetEffAddr(uint rn, Offset offst)
        {
            long EA = (CPU.GetRegr(I_Reg, (int)rn) + (U == 1 ? offst.GetValue() : -offst.GetValue())) & 0xFFFFFFFF;
            return (int)EA;
        }

        List<int> GetList(uint list)
        {
            List<int> reglst = new List<int>();

            for (int cnt = 0; cnt < 16; ++cnt)
            {
                if (Memory.ExtractBits(list, (uint)(31 - cnt), (uint)(31 - cnt)) == 1)
                {
                    reglst.Add(cnt);
                }
            }

            return reglst;
        }
    }

    public class LDR: LoadStore
    {
        //Loads word from memory address: 
        public override void Execute(uint Rn, uint Rd, Offset offst) {
            //LDR   <Rd>, <addressing_mode>
            int EA = GetEffAddr(Rn, offst);         //((int)Rn + (U == 1 ? offst.GetValue(): -offst.GetValue())) & 0xFFFFFFFF;

            if(EA == 0x100001)
            {
                //check after check if input buff is empty
                I_Last_Char = I_Input_Buff.Count != 0 ? I_Input_Buff[I_Input_Buff.Count - 1] : (char)0;
                CPU.SetReg(I_Reg, (int)Rd, Convert.ToInt32(I_Last_Char));
            }
            else
            {
                if (B == 1)
                {
                    CPU.SetReg(I_Reg, (int)Rd, I_RAM.ReadByte((uint)EA));
                }
                else
                {
                    CPU.SetReg(I_Reg, (int)Rd, I_RAM.ReadWord(EA)); //Remember to check what memreads return if invalid EA
                }

                if (W == 1)
                    CPU.SetReg(I_Reg, (int)Rn, EA);
            }            
        }
    }

    public class STR: LoadStore
    {
        //stores word to memory address:
        public override void Execute(uint Rn, uint Rd, Offset offst)
        {
            int EA = GetEffAddr(Rn, offst);        //((int)Rn + (U == 1 ? offst.GetValue() : -offst.GetValue())) & 0xFFFFFFFF;

            if(EA == 0x100000)
            {                
                I_Console_Ref.Append((char)CPU.GetRegr(I_Reg, (int)Rd));
            }
            else
            {
                if (B == 1)
                {
                    byte b = Convert.ToByte(CPU.GetRegr(I_Reg, (int)Rd) & 0xFF);
                    I_RAM.WriteByte(b, EA);
                }
                else {
                    I_RAM.WriteWord(CPU.GetRegr(I_Reg, (int)Rd), EA);
                }

                if (W == 1)
                    CPU.SetReg(I_Reg, (int)Rn, EA);
            }            
        }
    }

    public class STM : LoadStore
    {
        //pushes register list onto stack
        public override void Execute(uint Rn, List<int> Reglist)
        {
            int EA = CPU.GetRegr(I_Reg, (int)Rn);
            EA -= (4 * Reglist.Count);

            if (W == 1)
                CPU.SetReg(I_Reg, (int)Rn, EA);

            foreach (int i in Reglist)
            {
                int val = CPU.GetRegr(I_Reg, i);
                I_RAM.WriteWord(val, EA);
                EA += 4;
            }
        }
    }

    //pops values from stack to registers in list
    public class LDM : LoadStore
    {
        public override void Execute(uint Rn, List<int> Reglist) {
            int EA = CPU.GetRegr(I_Reg, (int)Rn);

            foreach (int i in Reglist)
            {
                int val = I_RAM.ReadWord(EA);              
                CPU.SetReg(I_Reg, i, val);
                EA += 4;
            }

            if (W == 1)
                CPU.SetReg(I_Reg, (int)Rn, EA);
        }
    }

    //Defines logic for getting Offset--------------------------------------
    public class Offset
    {        
        public uint OffBits;
        public Memory Offset_Regs { get; set; }
        public string Offset_Repr { get; set; }
        public int U = 0;
        private string[] repr_shifts = new string[] { "lsl", "lsr", "asr", "ror" };
        public string[] Repr_shifts { get { return repr_shifts; } }

        //creates an Operand2 object for the DP instruction based on the operand type bits to use.
        public static Offset GetOffset(bool regimm, uint bits)
        {
            Offset offst;
            if (regimm)
            {
                offst = new Offset_Reg();
            }
            else
            {
                offst = new Offset_Imm();
            }
            offst.OffBits = bits;
            return offst;
        }

        //Uses BarrelShift to get value for OffSet
        public virtual int GetValue() { return 0; }

        public override string ToString() { return ""; }
    }

    public class Offset_Imm : Offset
    {
        public override int GetValue()
        {
            return (int)OffBits;
        }

        public override string ToString()
        {
            int offbits = GetValue();
            return offbits > 0 ? ", #" + (U == 1 ? "" : "-") + offbits.ToString() : "";
        }
    }

    public class Offset_Reg : Offset
    {
        public uint reg, imm, shift;
        public override int GetValue()
        {
            reg = Memory.ExtractBits(OffBits, 28, 31);
            shift = Memory.ExtractBits(OffBits, 25, 26);
            imm = Memory.ExtractBits(OffBits, 20, 24);

            return (int)BarrelShift.Compute(shift, (uint)CPU.GetRegr(Offset_Regs, (int)reg), imm);
        }

        public override string ToString()
        {
            GetValue();  
            return ", " + (U == 1 ? "" : "-") + CPU.GetStrRegr((int)reg) + (imm > 0 ? ", " + Repr_shifts[shift] + " #" + imm.ToString() : "");
            
        }
    }
}
