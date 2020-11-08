//--------------------------------------------------------------------------------------------
//File:   ComputerWindow.xaml.cs
//Desc:   This file defines a class ComputerWindow which contains all the logic and components of the 
//        ARM Simulator Window.
//---------------------------------------------------------------------------------------------

using System;
using System.Data;
using System.Dynamic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net.NetworkInformation;
using System.Windows.Documents;

namespace armsim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    //public Dictionary<Key, char> charset

    public partial class MainWindow : Window
    {
        List<String> FakeAsm = new List<string>() { "push	{fp}		; (str fp, [sp, #-4]!)", "add    fp, sp, #0", "sub	sp, sp, #12", "str	r0, [fp, #-8]",
            "add	sp, fp, #0",  "ldmfd	sp!, {fp}", "bx	lr", "push	{fp}		; (str fp, [sp, #-4]!)", "add	fp, sp, #0", "sub	sp, sp, #12", "str	r0, [fp, #-8]",
            "add	sp, fp, #0", "ldmfd	sp!, {fp}", "bx   lr", "push	{fp, lr}", "add	fp, sp, #4", "sub	sp, sp, #8", "mov	r3, #10", "str	r3, [fp, #-8]",
            "mov	r3, #0", "str	r3, [fp, #-12]", "b	174 <mystart+0x3c>", "ldr	r2, [fp, #-12]", "ldr	r3, [fp, #-8]", "add	r3, r2, r3", "str    r3, [fp, #-12]",
            "ldr	r3, [fp, #-8]", "sub	r3, r3, #1", "str	r3, [fp, #-8]", "ldr	r3, [fp, #-8]", "cmp	r3, #0", "bne	158 <mystart+0x20>", "bl	100 <putc>",
            "ldr	r0, [fp, #-12]", "ldr	r3, [pc, #28]	; 1ac <mystart+0x74>", "ldr	r3, [r3]", "mov	r0, r3", "bl	11c <puts>", "ldr	r3, [fp, #-12]",
            "mov	r0, r3", "sub	sp, fp, #4", "pop	{fp, lr}", "bx	lr", ".word	0x00000400"
        };

        int MB = 1048576;

        string[] messages = { "Good", "Loader failed to load file into memory." };

        public Computer asim;
        Loader fileLoader;
        Options Opt;

        bool FileSetup { get; set; }
        int CkSum { get; set; }

        int keyShift = 0;

        int MemStartAddr = 0;
        Thread runThread;

        public MainWindow(Options opt)
        {
            InitializeComponent();

            asim = new Computer(opt.Memsize, opt.Filename);               //Set Context as computer Simulator
            asim.CompTraceall = opt.Traceall;
            DataContext = asim;

            fileLoader = new Loader();
            Opt = opt;
            FileSetup = false;
            Setup();
        }

        //Used for setting up GUI 
        public void Setup()
        {
            AddShortcuts();

            txtFileName.SetBinding(TextBlock.TextProperty, "ProgName");
            txtStatus.SetBinding(TextBlock.TextProperty, "ProgStatus");
            txtCkSum.SetBinding(TextBlock.TextProperty, "SumRAM");
            txtTerm.SetBinding(TextBox.TextProperty, "Comp_Console");           

            lbl_ProcMod.SetBinding(ContentProperty, "Proc_Mode");
            chkBx_Trace.SetBinding(CheckBox.IsCheckedProperty, "Enable_Trace");            

            btn_N.SetBinding(IsEnabledProperty, "N_Flag");
            btn_Z.SetBinding(IsEnabledProperty, "Z_Flag");
            btn_C.SetBinding(IsEnabledProperty, "C_Flag");
            btn_V.SetBinding(IsEnabledProperty, "V_Flag");
            btn_I.SetBinding(IsEnabledProperty, "I_Flag");

            btnRun.IsEnabled = false;
            btnStep.IsEnabled = false;

            if (Opt.Filename != null)
            {
                ProccessFile(Opt.Filename, asim.CompRAM.Cells);
            }
            else
            {
                FillGrids(false);
            }//might be doing duplicate work in Options Validation check - fixed?;
        }

        //Processes file, displaying the result of the operations in the status bar.
        public void ProccessFile(string file, byte[] cells)
        {
            bool vldfile = Opt.Vldfile(file);
            bool fileLoaded = true;

            if (vldfile == true && (fileLoaded = fileLoader.LoadElf(file, cells)))
            {
                UpdateStatusBar(messages[0], file, asim.CompRAM.CheckSum(cells));

                asim.Registers.WriteWord(fileLoader.P_entry, 0x3c);
                FileSetup = true;
                
                //Can be simplified, but oh well..
                btnRun.IsEnabled = true;
                btnRun.SetBinding(Button.IsEnabledProperty, "Running");
                asim.Adjust_Reset();
                FillGrids(true);
            }
            else
            {
                string st = fileLoaded ? Opt.Valid : messages[1];
                UpdateStatusBar(st, "(None)", 0);
            }            
        }

        //-------------------------------------------------------------------------------------------
        public void UpdateStatusBar(string st, string prgname, int sum)
        {
            asim.ProgStatus = st;
            asim.ProgName = prgname;
            asim.SumRAM = sum;
        }

        public void FillMemory()
        {
            List<ReprMemory> lpr = new List<ReprMemory>();
            ReprMemory rp;
            byte[] row;

            int len = asim.CompRAM.Cells.Length, delt = 0;

            for(int i = MemStartAddr; i < len; i += 16 )
            {
                delt = len - i;

                if (delt > 16){delt = 16;}

                row = new byte[16];
                Array.Copy(asim.CompRAM.Cells, i, row, 0, delt);

                rp = new ReprMemory(i, row);
                lpr.Add(rp);
            }

            lvMemory.ItemsSource = lpr;
        }

        public void FillRegister()
        {
            List<string> regnames = new List<string>() { "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7",
                "R8", "R9", "R10", "R11", "IP", "SP", "R14", "PC", "CSPR", "SP_irq", "LR_irq", "SPSR_irq", "SP_svc", "LR_svc", "SPSR_svc"};

            List<ReprRegister> lregs = new List<ReprRegister>();            
            for(int i = 0; i < asim.Registers.Cells.Length; i += 4) //will need to come to fix
            {
                lregs.Add(new ReprRegister { Register = regnames[(i/4)], Value = ("=" + asim.Registers.ReadWord(i).ToString("X8"))});
            }

            lvRegisters.ItemsSource = lregs;
        }

        public void FillDisAsm()
        {
            Random ind = new Random();

            List<ReprDisAsm> disasm = new List<ReprDisAsm>();

            ReprDisAsm DisProw;
            int len = FakeAsm.Count, cmdind;

            Instruction inst;

            for(int i = (asim.CompCPU.PC > 16 ? asim.CompCPU.PC - 16: asim.CompCPU.PC); i < asim.CompCPU.PC + 240; i += 4)
            {
                uint ins = (uint)asim.CompRAM.ReadWord(i); 
                inst = Instruction.CreateInstr(ins, asim.Registers, asim.CompRAM);
                inst.InstAddr = (uint)i;
                cmdind = ind.Next(0, len - 1);
                DisProw = new ReprDisAsm() { Address = i, MachineCode = (int)ins, DecInstrct = ins != 0 ? inst.ToString() : "" };
                disasm.Add(DisProw);
            }

            lvDisassembly.ItemsSource = disasm;
        }

        public void FillStack()
        {
            List<ReprStack> stackp = new List<ReprStack>(); //In real use, will need to make global to actually store state, binding is found

            ReprStack row;
            for(int i = asim.CompCPU.SP; i > asim.CompCPU.SP - (10 * 4) && i > -1; i -= 4 )
            {
                row = new ReprStack() { StkAddr = i, StkVal = asim.CompRAM.ReadWord(i)};
                stackp.Add(row);
            }
            lvStack.ItemsSource = stackp;
        }

        void FillGrids(bool disasm) //bool value is to indicate whether or not to fill disassembly.
        {
            FillMemory();
            FillRegister();
            
            if (disasm)
            {
                FillStack();
                FillDisAsm();
                HighLightDisAsm();
            }

            //txtTerm.Width = colTerm.Width;
        }
        //--------------------------------------------------------------------------------------------------------
        void HightLightMemory(int addr)
        {
            int delt; 
            foreach(object item in lvMemory.Items)
            {
                delt = addr - ((ReprMemory)item).Address;

                if (delt < 16 && delt > -1)
                    lvMemory.SelectedItem = item;
            }
        }

        void HighLightDisAsm()
        {
            for(int i = 0; i < lvDisassembly.Items.Count; ++i){
                ReprDisAsm item = (ReprDisAsm)lvDisassembly.Items.GetItemAt(i);

                if(item.Address == asim.CompCPU.PC)
                {
                    lvDisassembly.SelectedItem = item;
                    break;
                }         
            }
        }

        private void AddShortcuts()
        {
            try
            {
                RoutedCommand shortcut;
                Dictionary<Key, ExecutedRoutedEventHandler> comboKeys = new Dictionary<Key, ExecutedRoutedEventHandler> {
                    { Key.O, OpenFile }, {Key.F5, btnRun_Click }, {Key.F10, btnStep_Click},
                    {Key.Q, btnStop_Click}, {Key.R, btnReset_Click}, {Key.B, btnBreakP_Click}, {Key.T, chkBx_Trace_Checked} };

                foreach( KeyValuePair<Key, ExecutedRoutedEventHandler> combo in comboKeys)
                {
                    shortcut = new RoutedCommand();

                    if(combo.Key == Key.F5 || combo.Key == Key.F10)
                    {
                        shortcut.InputGestures.Add(new KeyGesture(combo.Key));
                    }
                    else
                    {
                        shortcut.InputGestures.Add(new KeyGesture(combo.Key, ModifierKeys.Control));
                    }

                    CommandBindings.Add(new CommandBinding(shortcut, combo.Value));
                }
            }
            catch {; }
            finally {; }
                
        }

        private void EnableButtons(bool state)
        {
            btnStep.IsEnabled = state;
            btnReset.IsEnabled = state;
            btnBreakP.IsEnabled = state;       
        }

        //--------------------------------------------------------------------------------------------------

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = true;

            runThread = new Thread(asim.Run);
            runThread.Start();

            //Need to figure out how to update when thread stops.
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try { runThread.Abort(); } catch {; } finally {; }

            btnStop.IsEnabled = false;
        }

        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            asim.Step();
            FillGrids(true);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            asim.Reset();
            ProccessFile(asim.ProgName, asim.CompRAM.Cells);
            //asim.Adjust_Reset();
        }

        private void btnBreakP_Click(object sender, RoutedEventArgs e)
        {
            string bpaddr = "";
            //To be implemented. 
            if (btnBreakP.IsEnabled)
            {
                GetBreakPoint bpdialog = new GetBreakPoint(bpaddr);
                bpdialog.Show();

                Task.Run(() =>
                {
                    while (bpdialog.BPaddr.Equals("") && bpdialog.On == true) {; }


                    try { asim.AddBreakP(Convert.ToInt32(bpdialog.BPaddr, 16)); }
                    catch {; }
                    finally {; }
                });
            }            
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog winfiledialog = new Microsoft.Win32.OpenFileDialog();

            winfiledialog.DefaultExt = ".exe";
            winfiledialog.Filter = "EXE Files (*.exe)|*.exe| All Files (*.*) |*.*";

            if ((bool)winfiledialog.ShowDialog())
            {
                asim.Reset();
                ProccessFile(winfiledialog.FileName, asim.CompRAM.Cells);
            }
        }

        //Memory Addressing function
        private void AddrKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                MemStartAddr = Convert.ToInt32(txtStartAddr.Text, 16);
                FillGrids(false);
                HightLightMemory(MemStartAddr);
            }
        }

        //Shortcuts implementation
        private void Term_KeyDown(object sender, KeyEventArgs e)
        {

            if(e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                keyShift = 1;
            }
            else
            {
                char[] key = e.Key.ToString().ToCharArray();
                char akey = key.Length == 2 ? key[1] : key[0];

                akey = keyShift == 1 ? (Char.IsLetter(akey) ? akey : akey) : (Char.IsLetter(akey) ? Char.ToLower(akey) : akey);
                keyShift = 0;   

                if(e.Key == Key.Return)
                {
                    akey = '\r';
                }else if (e.Key == Key.Space)
                {
                    akey = (char)32;
                }

                asim.Input_Buffer.Add(akey);
                asim.CompCPU.IRQ = true;
            }  
            
            e.Handled = true;

        }

        //Updating Displays after Run has completed; does that most of the time
        private void btnRun_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (btnRun.IsEnabled && FileSetup)
            {
                FillGrids(true);
                btnStop.IsEnabled = false;
                EnableButtons(true);
            }
            else if (FileSetup) { btnStop.IsEnabled = true; EnableButtons(false); }
        }

        //For highlighting buttons that were clicked
        private void btn_Flag_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Button b = (Button)sender;
            HighLightBtn(b, b.IsEnabled);
        }

        //Goes with the previous function
        public void HighLightBtn(Button btn, bool which)
        {
            btn.ClearValue(BackgroundProperty);
            btn.ClearValue(BorderBrushProperty);

            if (which)
            {
                btn.Background = Brushes.LightGreen;
                btn.BorderBrush = Brushes.DarkGreen;
            } 
        }
        
        //Allows for a bidirectional binding to Enable/Disable Trace which is not possible without an eventhandler declaration
        private void chkBx_Trace_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == this.GetType()) { asim.Enable_Trace = !(bool)chkBx_Trace.IsChecked; }
        }

    }

    //Defines logic to represent Registers
    public class ReprRegister {
        public string Register { get; set; }
        public string Value { get; set; } 
    }

    //Defines logic to represent Memory
    public class ReprMemory
    {
        public ReprMemory(int addr, byte[] cells)
        {
            Address = addr;
            C0 = cells[0]; C1 = cells[1]; C2 = cells[2]; C3 = cells[3]; C4 = cells[4]; C5 = cells[5]; C6 = cells[6]; C7 = cells[7];
            C8 = cells[8]; C9 = cells[9]; CA = cells[10]; CB = cells[11]; CC = cells[12]; CD = cells[13]; CE = cells[14]; CF = cells[15];
        }
        public int Address { get; set; }
        public byte C0 { get; set; }
        public byte C1 { get; set; }
        public byte C2 { get; set; }
        public byte C3 { get; set; }
        public byte C4 { get; set; }
        public byte C5 { get; set; }
        public byte C6 { get; set; }
        public byte C7 { get; set; }
        public string Space { get { return " "; } set { } }
        public byte C8 { get; set; }
        public byte C9 { get; set; }
        public byte CA { get; set; }
        public byte CB { get; set; }
        public byte CC { get; set; }
        public byte CD { get; set; }
        public byte CE { get; set; }
        public byte CF { get; set; }
    }

    //Defines logic to represent Disassembly
    public class ReprDisAsm
    {
        //Will 4 instructions before PC addr/highlight the PC row.

        public int Address { get; set; }
        public int MachineCode { get; set; }
        public String DecInstrct { get; set; }
    }

    //Defines logic to represent Stack
    public class ReprStack
    {
        public int StkAddr { get; set; }
        public int StkVal { get; set; }
    }
}
