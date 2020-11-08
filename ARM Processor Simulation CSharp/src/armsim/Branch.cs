//--------------------------------------------------------------------------------------------
//File:   Branch.cs
//Desc:   This file defines a class Branch that contains logic for the Branch instructions.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    public class Branch : Instruction
    {
        public List<string> strInst = new List<string>(){"b", "bl", "bx" };

        public const int TYPE = 0b100;
        public uint cond, typ, L, X, Rm = 18;
        public int PC_SOffset, indx = 0;

        //like will be a store of 
        public override void DecodeInst() {
            cond = Memory.ExtractBits(Inst, 0, 3);
            typ = Memory.ExtractBits(Inst, 4, 6);

            L = Memory.ExtractBits(Inst, 7, 7);
           
            if(typ == 0)
            {
                Rm = Memory.ExtractBits(Inst, 28, 31);
                indx = 2;
            }
            else
            {
                PC_SOffset = (((int)(Memory.ExtractBits(Inst, 8, 31) << 8)) >> 8) << 2;
                indx = (int)L;
            }            
        }

        public virtual void Execute(int Rm) { }
        public override void Execute() {
            List<Branch> binst = new List<Branch>() { new B(), new BX() };

            DecodeInst();

            Branch br = binst[indx > 1 ? 1: 0];
            br.L = L;
            br.I_Reg = I_Reg;
            br.I_RAM = I_RAM;
            br.InstAddr = InstAddr;
            
            if(indx > 1){ br.Execute((int)Rm); }
            else { br.Execute(PC_SOffset); }
        }
        public override string ToString()
        {
            DecodeInst();

            ASMRepr = strInst[indx] + (Cond < 0b1110 ? CondSufx[Cond] : "") + "\t" + (indx > 1 ? CPU.GetStrRegr(I_Reg, (int)Rm) : 
                (InstAddr + 8 + PC_SOffset).ToString());
            
            return ASMRepr;
        }
    }

    // branches/branch & Link to offset
    class B: Branch
    {
        public override void Execute(int offset)
        {
            int pc = CPU.GetRegr(I_Reg, 15); //returns pc + 8;
            pc -= 4;    //this might be a problem
            if (L == 1)
                CPU.SetReg(I_Reg, 14, pc);
            CPU.SetReg(I_Reg, 15, ((int)InstAddr + 8 + offset));
        }
    }

    //branches to RM address pointed to.
    class BX: Branch
    {
        public override void Execute(int Rm)
        {
            uint rm0 = (uint)CPU.GetRegr(I_Reg, Rm) & 0xFFFFFFFE;
            CPU.SetReg(I_Reg, 15, (int)(rm0));
        }
    }

}
