//--------------------------------------------------------------------------------------------
//File:   Computer.cs
//Desc:   This file defines a class Computer which contains all the logic and components of the 
//        ARM simulator.
//---------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Media;
using System.IO;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;

namespace armsim
{
    //Class that puts it all together
    public class Computer : INotifyPropertyChanged
    {
        /// <summary>
        /// Define logic for Computer simulation
        /// </summary>
        /// 

        public const int MB_Size = 32768;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<int> Breakpoints = new List<int>();

        //-----------------Tracing--------------------------
        public static StreamWriter Tracelog = null;
        public string[] modes = { "SVC", "SYS", "IRQ" };
        //--------------------------------------------------

        //---------------------RAM, Registers, CPU------------------------------------
        int reg_num = 23 * 4; //16 norms + 1 CPSR(16), 1 SP_irq(17), 1 LR_irq(18), 1 SPSR_irq(19), 1 SP_svc(20), 1 LR_svc(21), 1 SPSR_svc(22) 
        
        private Memory ram;
        private Memory registers;
        private CPU cpu;

        public Memory CompRAM { get { return ram; } set { ram = value; } }
        public Memory Registers { get { return registers; } set { registers = value; } }
        public CPU CompCPU { get { return cpu; } set { cpu = value; } }
        //-----------------------------------------------------------------------------

        //-------------------Status Bar: Filename, Program Status, CheckSum of RAM----------------------------
        private string progName;
        private string progStatus;
        private int sum;

        public string ProgName { get { return progName; } set { progName = value; SetProperty("ProgName"); } }
        public string ProgStatus { get { return progStatus; } set { progStatus = value; SetProperty("ProgStatus"); } }
        public int SumRAM { get { return sum; } set { sum = value; SetProperty("SumRAM"); } }
        //----------------------------------------------------------

        //--------------------Bindings for CPSR State-----------------
        public Dictionary<uint, string> proc_modes = new Dictionary<uint, string>{
            { 0b10011, "Supervisor" }, { 0b11111, "System" }, { 0b10010, "IRQ" } };
        private uint Curr_Mode;
        public string Proc_Mode { get { return proc_modes[Curr_Mode]; } set { SetProperty("Proc_Mode"); } }

        public bool N_Flag { get { return registers.TestFlag(16 * 4, 0); } set { SetProperty("N_Flag"); } }
        public bool Z_Flag { get { return registers.TestFlag(16 * 4, 1); } set { SetProperty("Z_Flag"); } }
        public bool C_Flag { get { return registers.TestFlag(16 * 4, 2); } set { SetProperty("C_Flag"); } }
        public bool V_Flag { get { return registers.TestFlag(16 * 4, 3); } set { SetProperty("V_Flag"); } }
        public bool I_Flag { get { return registers.TestFlag(16 * 4, 24); } set { SetProperty("I_Flag"); } }

        //For toolbar and trace.
        public bool running = false, trace_closed = false, trace = true;
        public bool Running { get { return !running; } set { SetProperty("Running"); } }
        
        public bool Enable_Trace { get { return trace; } set { trace = value; SetProperty("Enable_Trace"); } } 
        public bool CompTraceall { get; set; }
        public int Step_Cnt = 1;

        //-------------Console-----------------------------
        public List<char> Input_Buffer = new List<char>();
        public StringBuilder ConsoleBuilder = new StringBuilder();        
        public string Comp_Console { get { return ConsoleBuilder.ToString(); } set { SetProperty("Comp_Console"); } }

        // ------------------------------------------------------------------------------------

        public Computer(int size, string filename){
            ram = new Memory(size);
            registers = new Memory(reg_num);
            cpu = new CPU(ram, registers);

            cpu.CPU_Console_Ref = ConsoleBuilder;
            cpu.CPU_Input_Buff = Input_Buffer;

            ProgName = filename == null ? "(None)" : filename ;

            Enable_Trace = true;
            Tracelog = File.CreateText(Directory.GetCurrentDirectory() + "\\trace.log");
        }

        //------------------------------------------------------------------------------------------
        // perform fde cycle until fetch or breakpoint encountered returns 0
        public void Run() {
            uint val;

            try
            {
                Running = true;
                do
                {
                    if (Breakpoints.Contains(CompCPU.PC)) { break; }
                    val = Step();
                    
                } while (val != 0);
            }
            catch (Exception) { }
            finally { ; }
            running = false;
            Running = true;
        }

        //For updating Flags and Console
        void Update_FlagsMode()
        {
            N_Flag = true;
            Z_Flag = true;
            C_Flag = true;
            V_Flag = true;
            I_Flag = true;

            Curr_Mode = Memory.ExtractBits((uint)CompCPU.CPSR, 27, 31);
            Proc_Mode = Curr_Mode.ToString();
            Comp_Console = "";
        }

        // 1 fde cycle
        public uint Step() {
            
            uint val = (uint)CompCPU.fetch();
            int pc = CompCPU.PC;

            Instruction inst = CompCPU.decode(val);
            try
            {
                CompCPU.execute(inst);     //Should Execute be in Task.Run()? because of loop           
            } catch (OperationCanceledException) { 
                val = 0;
            }
            finally {; }

            if (val != 0 && CompCPU.IRQ && !Registers.TestFlag(16 * 4, 24))
            {
                CompCPU.Do_IRQProcessing();
                CompCPU.IRQ = false;

            }
            Update_FlagsMode();
            Trace(pc);
            if(val == 0) { Enable_Trace = false; }
            return val;
        } 

        //Zeroes out Registers and Memory 
        public void Reset()
        {
            CompRAM = new Memory(MB_Size);
            Registers = new Memory(reg_num);

            CompCPU = new CPU(ram, registers);
            CompCPU.CPU_Console_Ref = ConsoleBuilder;
            Input_Buffer.Clear();
            
            if(ConsoleBuilder.Length != 0)
            {
                ConsoleBuilder.Append("\n");
            }            

            CompCPU.CPU_Input_Buff = Input_Buffer;

            CompCPU.SP = 0x7000;
            CompCPU.CPSR = 0x13;
            
            Step_Cnt = 1;

            if (!trace_closed)
            {
                Tracelog.Flush(); Tracelog.Close();
                trace_closed = true;
            }
                
            
            if (Enable_Trace && trace_closed) { 
                Tracelog = File.CreateText(Directory.GetCurrentDirectory() + "\\trace.log");
                trace_closed = false;
            }
            
        }

        //Adjusts if no OS loaded
        public void Adjust_Reset()
        {
            if(CompRAM.ReadByte(0) != 0)
            {
                CompCPU.PC = 0;
            }
            else
            {
                CompCPU.CPSR = 0x1F;
            }

            Update_FlagsMode();
        }

        //Add a breakpoint address to a list of breakpoints if not already contained.
        public void AddBreakP(int addr)
        {
            if (!Breakpoints.Contains(addr)) { Breakpoints.Add(addr); }
        }

        //---------------------------------------------------------------------------------------------------------        
        public void Trace(int pc)
        {
            string format = "{0:000000} {1:X8} {2:X8} {3} {19} 0={4:X8} 1={5:X8} 2={6:X8} 3={7:X8} 4={8:X8} 5={9:X8} 6={10:X8}" +
                 " 7={11:X8} 8={12:X8} 9={13:X8} 10={14:X8} 11={15:X8} 12={16:X8} 13={17:X8} 14={18:X8} ";

            int[] regs = new int[15];
            for(int i = 0; i < 15; ++i) { regs[i] = CPU.GetRegr(Registers, i);  }

            if (Enable_Trace)
            {
                if (trace_closed) {
                    Tracelog = File.CreateText(Directory.GetCurrentDirectory() + "\\trace.log");
                    trace_closed = false;
                }
                uint intcpsr = Memory.ExtractBits((uint)CompCPU.CPSR, 0, 3);
                uint mode = Memory.ExtractBits((uint)CompCPU.CPSR, 27, 31);
                string cpsr = Convert.ToString(intcpsr, 2).PadLeft(4, '0');

                //Check for traceall flag
                if(CompTraceall || mode == 0x1F)
                {
                    int indxMode = (mode == 0x1F) ? 1 : (mode == 0x12 ? 2 : 0);
                    Tracelog.WriteLine(format, Step_Cnt, pc - 4, CompRAM.CheckSum(CompRAM.Cells), cpsr, regs[0], regs[1], 
                        regs[2], regs[3], regs[4], regs[5], regs[6], regs[7], regs[8], regs[9], regs[10], regs[11], 
                        regs[12], regs[13], regs[14], modes[indxMode]);
                    Tracelog.Flush();
                    ++Step_Cnt;
                }
                
            }
            else
            {
                if (!trace_closed)
                {
                    Tracelog.Flush();
                    Tracelog.Close();
                    trace_closed = true;
                }                    
            }
        }

        //Event handler for the ProperyChanged event Notifying the object bound of the change in the source in its parameters. 
        protected void SetProperty(string source)
        {
            PropertyChangedEventHandler handle = PropertyChanged;
            if(handle != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(source));
            }
        }
    }
}
