//--------------------------------------------------------------------------------------------
//File:   DataProcess.cs
//Desc:   This file defines a class DataPrcess with subclasses that contains logic for the 
//           dataprocessing instructions.
//---------------------------------------------------------------------------------------------

using NUnit.Framework.Constraints;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace armsim
{
    //INSTRUCTIONS: MOV(), MVN(), ADD(), SUB(), RSB(), 
    // AND(), ORR(), EOR(), BIC(), MUL()

    //Offsets: cond = 0 - 3, type = 4-6(I=6), Opcode = 7-10, S = 11-11, Rn=12-15, Rd=16-19, oper2 = 20-31,

    //all child class will have their own TYPE so DATA; In comments shift_operand refers to the Operand2 type
    public class DataProccess : Instruction
    {
        public List<string> strInst = new List<string>()
        {
            "and", "eor", "sub", "rsb", "add",
            null, null, null, null, null, "cmp", null, 
            "orr", "mov", "bic", "mvn", "mul"
        };

        public const int TYPE = 0b0;

        public uint cond, typ, opcode, Rn, Rm, Rs, bit7, bit4;
        public bool regimm, sbit;
        public Operand2 Oper2;
        
        //See general definition in parent(Instruction)
        public override void DecodeInst() {
            cond    =   Memory.ExtractBits(Inst, 0, 3);
            typ     =   Memory.ExtractBits(Inst, 4, 6);
            bit7    =   Memory.ExtractBits(Inst, 24, 24);
            bit4    =   Memory.ExtractBits(Inst, 27, 27);

            if (typ == 0 &&  bit7 == 1 && bit4 == 1)
            {
                opcode = 0x10;                             //for convenience, I suppose.
                Rd = Memory.ExtractBits(Inst, 12, 15);
                Rs = Memory.ExtractBits(Inst, 20, 23);
                Rm = Memory.ExtractBits(Inst, 28, 31); 
            }
            else
            {
                regimm = Convert.ToBoolean(Memory.ExtractBits(typ, 31, 31)); //true == 1; false = 0;
                opcode = Memory.ExtractBits(Inst, 7, 10);
                sbit = Convert.ToBoolean(Memory.ExtractBits(Inst, 11, 11)); //same as regimm;
                Rn = Memory.ExtractBits(Inst, 12, 15);
                Rd = Memory.ExtractBits(Inst, 16, 19);
                Oper2 = Operand2.GetOper2(regimm, Memory.ExtractBits(Inst, 20, 31));
                Oper2.Oper2Regs = I_Reg;
            }
        }

        //override for MUL execute.
        public virtual void Execute(uint Rd, uint Rm, uint Rs) {; }

        //override for execute which all subclasses, except for MUL, use.
        public virtual void Execute(uint Rn, uint Rd, Operand2 oper2) {; }

        //calls subclasses execute method that takes an operand2, registers
        public override void Execute() {
            List<DataProccess> dpinst = new List<DataProccess>()
            {
                new AND(), new EOR(), new SUB(), new RSB(), new ADD(),
                null, null, null, null, null, new CMP(), null,
                new ORR(), new MOV(), new BIC(), new MVN(), new MUL()
            };

            DecodeInst();
            DataProccess instR = dpinst[(int)opcode];
            instR.I_Reg = I_Reg;
            instR.sbit = sbit;

            if (opcode == 0x10){
                instR.Execute(Rd, Rm, Rs);
            } else { 
                instR.Execute(Rn, Rd, Oper2); 
            }
        }

        public override string ToString()
        {
            DecodeInst();

            ASMRepr = strInst[(int)opcode] + (sbit ? ((opcode != 10)? "s": ""): "")+ (Cond < 0b1110 ? CondSufx[Cond] : "") + 
                "\t" + CPU.GetStrRegr(I_Reg, (opcode == 10 ? (int)Rn : (int)Rd));

            if (strInst[(int)opcode] == "mul")
            {
                ASMRepr += ", " + CPU.GetStrRegr(I_Reg, (int)Rm) + ", " + CPU.GetStrRegr(I_Reg, (int)Rs);//check on that

            } else if(new List<string>() { "mov", "mvn", "cmp" }.Contains(strInst[(int)opcode]))
            {
                ASMRepr += ", " + Oper2.ToString();
            }
            else
            {
                ASMRepr += ", " + CPU.GetStrRegr(I_Reg, (int)Rn) + ", " + Oper2.ToString();
            }

            return ASMRepr;
        }
    }

    class CMP : DataProccess
    {
        public new const int TYPE = 0b1010; 
        //Update flags after Rn - shifter_operand
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            uint rnval = (uint)CPU.GetRegr(I_Reg, (int)Rn), op2val = (uint)oper2.GetValue();
            int val = (int)(rnval - op2val);
            uint N = Memory.ExtractBits((uint)val, 0, 0);
            int Z = val == 0 ? 1 : 0;
            int C = op2val <= rnval ? 1 : 0;  //May not be correctly handling C flag will need to modify 

            int rnv = (int)rnval, op2v = (int)op2val;
            val =  rnv - op2v;
            int V = (rnv > 0 && op2v < 0 && val < 0) || (rnv < 0 && op2v > 0 && val > 0) ? 1 : 0;

            bool[] flags = { Convert.ToBoolean(N), Convert.ToBoolean(Z), Convert.ToBoolean(C), Convert.ToBoolean(V) };
            for(int i = 0; i < 4; ++i)
                I_Reg.SetFlag(4 * 16, i, flags[i]);
        }

    }

    class MOV : DataProccess    
    {
        
        public new const int TYPE = 0b1101;
        //Rd := shifter_operand (no Rn)
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            CPU.SetReg(I_Reg, (int)Rd, oper2.GetValue());

            int mode = (int)Memory.ExtractBits((uint)CPU.GetRegr(I_Reg, 16), 27, 31);
            if(sbit && Rd == 15) 
            {
                int spsr = CPU.GetRegr(I_Reg, 16);
                if(mode == 0b10010)
                {                    
                    spsr = CPU.GetRegr(I_Reg, 19); //because I had to place them weirdly.                    
                    //CPU.SetReg(I_Reg, (int)Rd, oper2.GetValue() - 4); //adjust the LR for the PC when IRQ
                }
                else if (mode == 0b10011)
                {
                    spsr = CPU.GetRegr(I_Reg, 22);
                }

                CPU.SetReg(I_Reg, 16, spsr);
            }            
        }
    }

    class MVN : DataProccess  
    {
        public new const int TYPE = 0b1111;

        //Rd := NOT shifter_operand (no Rn)
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            CPU.SetReg(I_Reg, (int)Rd, ~oper2.GetValue());
        }
        
    }    

    class ADD: DataProccess
    {
        public new const int TYPE = 0b0100;

        // Rd := Rn + shifter_operand
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val =  CPU.GetRegr(I_Reg, (int)Rn) + oper2.GetValue();
            CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    class SUB: DataProccess
    {
        public new const int TYPE = 0b0010;

        //Rd := Rn - shifter_operand
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val = CPU.GetRegr(I_Reg, (int)Rn) - oper2.GetValue();
            CPU.SetReg(I_Reg, (int)Rd, val);
        } 
    }

    class RSB: DataProccess
    {
        public new const int TYPE = 0b0011;

        //Rd := shifter_operand - Rn
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val =  oper2.GetValue() - CPU.GetRegr(I_Reg, (int)Rn);
            CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    class MUL: DataProccess
    {
        //Rd := Rm * Rs;
        public override void Execute(uint Rd, uint Rm, uint Rs){
            int first = CPU.GetRegr(I_Reg, (int)Rm);
            int second = CPU.GetRegr(I_Reg, (int)Rs);
            long res = (first * second) & 0xFFFFFFFF; 
            CPU.SetReg(I_Reg, (int)Rd, (int)res);
        }
    }

    class AND: DataProccess
    {
        public new const int TYPE = 0b0000;

         // Rd := Rn AND shifter_operand(oper2)
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
          int val = CPU.GetRegr(I_Reg, (int)Rn) & oper2.GetValue();
          CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    class ORR: DataProccess
    {
        public new const int TYPE = 0b1100;

        // Rd := Rn OR shifter_operand
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val = CPU.GetRegr(I_Reg, (int)Rn) | oper2.GetValue();
            CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    class EOR: DataProccess
    {
        public new const int TYPE = 0b0001;

        //Rd := Rn EOR shifter_operand
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val = CPU.GetRegr(I_Reg, (int)Rn) ^ oper2.GetValue();
            CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    class BIC: DataProccess
    {
        public new const int TYPE = 0b1110;

        //Rd := Rn AND NOT(shifter_operand)
        public override void Execute(uint Rn, uint Rd, Operand2 oper2)
        {
            int val = CPU.GetRegr(I_Reg, (int)Rn) & (~oper2.GetValue());
            CPU.SetReg(I_Reg, (int)Rd, val);
        }
    }

    //-------------------Passed my IS A test----------------
    public class Operand2
    {
        public uint OperBits { get; set; }
        public Memory Oper2Regs { get; set; }
        public string Oper2_Repr{ get; set; }
        //public List<string> Oper2_Str_Regs { get; set; }
        
        private string[] repr_shifts = new string[] { "lsl", "lsr", "asr", "ror" };
        public string[] Repr_shifts { get { return repr_shifts; } }

        //creates an Operand2 object for the DP instruction based on the operand type bits to use.
        public static Operand2 GetOper2(bool regimm, uint bits)
        {
            Operand2 oper2; 
            if (regimm)
            {
                oper2 = new Oper2_RORImm();
            }
            else if (Convert.ToBoolean(Memory.ExtractBits(bits, 27, 27)))
            {
                oper2 = new Oper2_RegSReg();
            }else
            {
                oper2 = new Oper2_RegSImm();
            }

            oper2.OperBits = bits;
            return oper2;
        }

        //Uses BarrelShift to get value for Operand2
        public virtual int GetValue() { return 0; }
        public override string ToString() { return ""; }
    }

    public class Oper2_RegSReg : Operand2
    {
        public int reg, reg2;
        public uint shift;

        public override int GetValue()
        {
            reg = (int)Memory.ExtractBits(OperBits, 28, 31);
            shift = Memory.ExtractBits(OperBits, 25, 26);
            reg2 = (int)Memory.ExtractBits(OperBits, 20, 23);

            return (int)BarrelShift.Compute(shift, (uint)CPU.GetRegr(Oper2Regs, reg), (uint)CPU.GetRegr(Oper2Regs, reg2));
        }

        public override string ToString()
        {
            GetValue();
            return CPU.GetStrRegr(reg) + ", " + Repr_shifts[shift] + " " + CPU.GetStrRegr(reg2);
        }
    }

    public class Oper2_RegSImm : Operand2
    {
        public uint reg, imm, shift;
        public override int GetValue()
        {
            reg = Memory.ExtractBits(OperBits, 28, 31);
            shift = Memory.ExtractBits(OperBits, 25, 26);
            imm = Memory.ExtractBits(OperBits, 20, 24);

            return (int)BarrelShift.Compute(shift, (uint)CPU.GetRegr(Oper2Regs, (int)reg), imm);
        }

        public override string ToString()
        {
            GetValue();
            if(imm > 0)
            {
                return CPU.GetStrRegr((int)reg) + ", " + Repr_shifts[shift] + " #" + imm.ToString();
            }
            else
            {
                return CPU.GetStrRegr((int)reg);
            }            
        }

    }

    public class Oper2_RORImm : Operand2
    {
        uint rot, num;
        
        public override int GetValue()
        {
            rot = Memory.ExtractBits(OperBits, 20, 23) * 2;
            num = Memory.ExtractBits(OperBits, 24, 31);

            return (int)BarrelShift.Compute(0x11, num, rot);
        }

        public override string ToString()
        {
            return "#" + GetValue().ToString();
        }
    }

    //---------------------Does not pass IS A test, therefore the LSL, LSR, ASL, ASR are functions of BarrelShift------------------------------
    public class BarrelShift
    {
        //based on the bitpattern of code, do bitwise operations and return the results.
        public static uint Compute(uint code, uint toShift, uint displcmnt)
        {
            switch (code)
            {
                case 0:  // lsl
                    return (toShift << (int)displcmnt);
                case 1:  // lsr
                    return (toShift >> (int)displcmnt);
                case 2: // asr
                    return (uint)((int)(toShift) >> (int)displcmnt);
                default: // ror
                    uint high = Memory.ExtractBits(toShift, (31 - displcmnt), 31) << (int)(32 - displcmnt);
                    uint low = toShift >> (int)displcmnt;
                    
                    //used in ROR_IMM Oper2
                    return (high | low); 
            }
        }
    }
}
