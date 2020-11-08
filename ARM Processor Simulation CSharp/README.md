# Project ARMSim


**Student Name:** Melchisedek Dulcio

**Course Number and Name:** CpS 310 Microprocessor Architecture

**Submission Date:** 11/3/2020

**Total Time:** 33 hrs

## **Overview**
    This Armsim loads an executable file into a simulated RAM and registers, allowing the user to perform actions like running the program that was loaded into RAM, add a breaking point for the program, stop the program while it is running, and step through the program one instruction at a time.


## **Features** 
* Phase 1, Loader-
    * All commandline options are supported for this phase.
    * Logging Framework partially implemented
    * All Design Requirements for classes were implemented completely, 
    * All Unit Tests are implemented for what was properly completed.
    * Errors are handled gracefully.


 * Phase 2, GUI-
    * The commandline options are supported based on the instructions before they were updated.
    * All Design Requirements for classes areimplemented completely, 
    * The units tests for Computer and CPU class are not complete.
    * Most errors are handled gracefully.
    * Panel resizing partially implemented.

* Phase 3, Execute 1-
    * C-Level Features + 2 B-Level functions(LDR, STR), with some minor issues documented in the Bug Summary.

## **Prequisites** 
* Platform - Windows 10
* Software:
    * Visual Studio 2017/2019
    * Developper Command Prompt for VS 2019

## **Build and Test**         
* ***How to Compile the solution from the command line***
    >* NOTE: To use the Windows CMD, the user must have .NET frameworks added to the system environment variables.       
    > 1. Open the Windows command prompt (see _Note_) or the Developer Command Prompt for VS 2019 and navigate to the folder containing the solution for this project. 
    > 2. Run the command `msbuild` to compile the solution, and the _executable_ will be placed in the `bin\Debug`.  

* ***How to Compile and Run Unit Tests***
    > 1. Install NUnit3-Console from NUnit and add to the system or environment path variable.
    > 2. Open the Developer Command Prompt for VS 2019, also known as the `x64 Native Tools Command Prompt for VS 2019`, or any command prompt of choice.
    > 3. Navigate to the location containing the project solution which will also contain `armsim.csproj`.
    > 4. Run `nunit3-console armsim.csproj` and watch for the results.

## **Configuration**
* To configure the logging framework, locate the `armsim.exe.config` file that located in the folder as the executable file, and read the comment at the top that tells the user how to turn the logging on or off. 

* To redirect the logging output to a file, follow the comments in section in the config file labeled `"This controls logging section output"`. 
    
## **User Guide**     
The usage for this software from the commandline: 
1. In the commandline of your choice navigate to the location of the executable file, called armsim.exe.
2. The usage to the run the application is: _armsim [ --mem memory-size ] --exec elf-file_, where _mem_ is the optional memory size specification for program, _--exec_ indicates whether to start executing the file, and _elf-file_ is the executable the user wants to load its contents.

## **Instruction implementation:**
**DataProcessing**
* MOV, MOVS MVN, AND, ORR, EOR, BIC, MUL, ADD, SUB, RSB, CMP

**LoadStore**
* LDR, STR, LDM, STM

**Branch**
* B, BL, BX
**Addressing Modes**
* RegistershiftImm, 12-bit Imm

## **Bug Summary**      

**Loader:**
I added `msbuild.bat` in the armsim folder directory with the specified paths for the build to succeed. Credits: [Martin Ullrich](https://learningintheopen.org/2019/05/24/msbuild-error-error-msb4066-the-attribute-version-in-element-is-unrecognized/) (link may be outdated).

I was able to get the project to build successfully from the CMD, until I tried the Native Tools Dev Prompt to try building the tests using mstest. After spending 3 hours just trying to complete a successful build, I decided to let it be, so the **Build and Test** section is what I expect to happen when the project is successfully built.

**GUI:**
None known.

**Execute I**
1. When the program is run from the Visual Studio properties with the option --exec, the file is loaded and run, and the trace is written to a file. However, when run from the commandline with the same options, the application window comes up. I was not sure what to make of that.
2. The first time a file is loaded into the Computer simulator, the register displays updates automatically, but when you reset and run the program again, the display does not update automatically. This may cause some malfunctioning of the trace button.

**Execute II**
1. See comment 2 for Execute I.
2. The terminal cursor is set to the beginning of the selection each time the Console variable bound to it updates.

**Link:** [Link to Journal](https://bju-my.sharepoint.com/:t:/g/personal/mdulc928_students_bju_edu/EVdJB8j0LapIg45SmqwAU7kB-sr5-0ABoru7RqtLxuvmNQ?e=ds3aeE)

**Academic Integrity Statement:**
"By affixing my signature below, I certify that the accompanying work represents my own intellectual effort. Furthermore, I have received no outside help other than what is documented below."

- _Melchisedek Dulcio_
