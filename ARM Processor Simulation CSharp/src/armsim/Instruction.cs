//--------------------------------------------------------------------------------------------
//File:   Instruction.cs
//Desc:   This file defines a class Instruction which is the base class for all the instructions
//         implemented.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace armsim
{
    public abstract class Instruction
    {

        /// <summary>
        /// Defines base class for all Instructions
        /// </summary>

        public string[] CondSufx = { 
            "eq", "ne", "cs", "cc", "mi", "pl", "vs",
            "vc", "hi", "ls", "ge", "lt", "gt", "le" 
        }; 


        public uint Rd, Cond;                         //Destination Register common for all instructions     
        public string ASMRepr { get; set; }     //String represention of the instruction
        public uint Inst { get; set; }          //Holds the actual numerical value of the instruction
        public uint InstAddr { get; set; }

        public Memory I_Reg { get; set; }                   //Memory reference for registers
        public Memory I_RAM { get; set; }                   //Memory reference for RAM
        public StringBuilder I_Console_Ref { get; set; }
        public List<char> I_Input_Buff { get; set; }
        public char I_Last_Char { get; set; }
        

        //Logic for which type of instruction to create and returns the Instruction
        public static Instruction CreateInstr(uint instr, Memory reg, Memory ram)
        {             
            uint typebits = Memory.ExtractBits(instr, 4, 6);
            uint[] bx = { Memory.ExtractBits(instr, 7, 11), Memory.ExtractBits(instr, 24, 27), Memory.ExtractBits(instr, 12, 23) };

            Instruction I_Instr;
            if(typebits == 0b111 && Memory.ExtractBits(instr, 7, 7 ) == 1){
                I_Instr = new SWI();

            } else if((typebits == 0b101) || (typebits == 0 && bx[0] == 18 && bx[1] == 1 && bx[2] == 0xFFF)){
                I_Instr = new Branch();

            }else if(typebits > 0b1){
                I_Instr = new LoadStore();

            }else{
                uint sbit = Memory.ExtractBits(instr, 11, 11);
                uint opcode = Memory.ExtractBits(instr, 7, 10);

                if(sbit == 0 && opcode > 0b111 && opcode < 0b1100)
                {
                    uint bit10 = Memory.ExtractBits(instr, 10, 10);
                    I_Instr = (bit10 == 0b1) ? new MSR() as Instruction: new MRS();
                }
                else
                {
                    I_Instr = new DataProccess();
                }
                
            }

            I_Instr.I_RAM = ram;
            I_Instr.I_Reg = reg;
            I_Instr.Inst = instr;
            I_Instr.Cond = Memory.ExtractBits(instr, 0, 3);

            return I_Instr;
            
        }

        //General defintion: Extracts the bits Executes needs to run and stores them in variables.
        public abstract void DecodeInst();

        //General definition: Uses bits extracted by DecodeInstr to execute the sub classes intructions
        public abstract void Execute();

        //General definition: returns ASMRepr - will be adjusting later to remove duplicates
        public override string ToString() {
            return ASMRepr;
        }

    }

    public class SWI: Instruction
    {
        /// <summary>
        /// Implements the SWI Arm Instruction Logic 
        /// </summary>

        public uint cond, typ, swinum;  //condition bits, type bits, and swinumber
        public bool I_Reading { get; set; }
        //See general definition
        public override void DecodeInst() {
            typ = Memory.ExtractBits(Inst, 4, 7);
            swinum = Memory.ExtractBits(Inst, 8, 31);
        }

        //Simply throws an exception for the Step function to know SWI has executed.
        public override void Execute() { //
            DecodeInst();

            if (swinum == 0x11)
            {
                throw new OperationCanceledException(); //halt
            }

            CPU.Exception_Process(I_Reg, 0b10011);            
        }

        //See general definition
        public override string ToString() {
            DecodeInst();

            ASMRepr = "svc"+ (Cond < 0b1110 ? CondSufx[Cond] : "") + "\t0x" + swinum.ToString("X8");
            return ASMRepr; 
        }
    }

    //for saving status register
    class MRS : Instruction
    {
        //uint Rd;
        bool Rbit;
        public override void DecodeInst()
        {
            Rd = Memory.ExtractBits(Inst, 16, 19);
            Rbit = Convert.ToBoolean(Memory.ExtractBits(Inst, 9, 9));
        }

        public override void Execute()
        {
            DecodeInst();

            uint mode = Memory.ExtractBits((uint)CPU.GetRegr(I_Reg, 16), 27, 31);
            int which = 16;

            if (mode == 0b10010)
            {
                which = 19;
            }
            else if (mode == 0b10011)
            {
                which = 22;
            }

            CPU.SetReg(I_Reg, (int)Rd, CPU.GetRegr(I_Reg, (Rbit ? which : 16)));
        }

        public override string ToString()
        {
            DecodeInst();
            //Update StrReg to reflect the state for the 
            ASMRepr = "mrs" + (Cond < 0b1110 ? CondSufx[Cond] : "") + "\t" + CPU.GetStrRegr(I_Reg, (int)Rd) + ", " + (Rbit ? "SPSR, " : "CPSR, ");
            return base.ToString();
        }
    }

    //For restoring status register
    class MSR : Instruction
    {
        public uint cond, Rm;
        public bool regimm, Rbit;  //Rbit tells me whether or not I am dealing with SPSR or CPSR
        public uint imm_val;

        public override void DecodeInst()
        {
            cond = Memory.ExtractBits(Inst, 0, 3);
            Rbit = Convert.ToBoolean(Memory.ExtractBits(Inst, 9, 9));
            regimm = Convert.ToBoolean(Memory.ExtractBits(Inst, 6, 6));

            Rm = Memory.ExtractBits(Inst, 28, 31);

            imm_val = Memory.ExtractBits(Inst, 20, 31);
            Operand2 temp = new Oper2_RORImm() { OperBits = imm_val };
            imm_val = (uint)temp.GetValue();
        }

        public override void Execute()
        {
            DecodeInst();

            uint operand = regimm ? imm_val : (uint)CPU.GetRegr(I_Reg, (int)Rm);

            if (Rbit == false)
            {
                CPU.SetReg(I_Reg, 16, (int)operand);
            }
            else
            {
                uint mode = Memory.ExtractBits((uint)CPU.GetRegr(I_Reg, 16), 27, 31);
                if(mode == 0b10010 || mode == 0b10011)
                {
                    int indx = mode == 0b10010 ? 19 : 22;
                    CPU.SetReg(I_Reg, indx, (int)operand);
                }                
            }
        }

        public override string ToString()
        {
            DecodeInst();
            ASMRepr = "msr" + (Cond < 0b1110 ? CondSufx[Cond] : "") + "\t" + (Rbit ? "SPSR, " : "CPSR, ") + 
                (regimm ? "#" + imm_val.ToString(): CPU.GetStrRegr(I_Reg, (int)Rm));
            return ASMRepr;
        }
    }

   
}