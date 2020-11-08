## **Journal** 

**Total Time:** 115 hours

**SubTotals**
* Loader - **22 hrs**
* GUI - **38 hrs**
* Execute I - **27 hrs**
* Execute II - **38 hrs**

**Current Phase:** Execute II - 38 hours

**Phase I :** _Loader_ 22

8/25/2020: 12:00 - 1:50 - writing functions for design requirement.

8/26/2020 5:50 -6:08/ 7:30-11:00 - Planned, and implemented to LoadElf

8/27/2020 3 hrs - Implemented the simulated RAM, but only some of the design requirements functions      
8/28/2020 3 hrs - Implemented commandline parsing with the Options class.

8/29/2020 5 hrs - Implemented design requirements, wrote gui for RAM and wrote Unit Tests.

8/30/2020 1 hr - completed the SetFlag function. I realized I had been thinking about it the wrong way which is why it took me so long to solve.

8/31/2020 1 hr - research on Logging. I was having trouble getting the switch for logging to work.

9/3/2020 1 hr - Completed logging functionality and error handling since the night before I asked Colten if it had taken him a while to get the logging framework to work too, which he confirmed.

**Phase 2:** _GUI_ 38

9/6/2020 3 hrs - I designed an icon, most of the time was spent messing around with different features and styles. I also set up the framework for the different design requirements near the end to implement soon.

9/7/2020 3 hrs - I spent some time trying to find the best container to design I want to have in it.

9/8/2020 5 hrs - I worked on Load File Feature, redesigning argument parsing, and implemented functions for the Design requirements

9/9/2020 5 hrs - I finished the Load File Feature and started learning how to work with the datagrid container I use to display the Table like data, such as Memory Cells, stack, and Disassembly, simply because Dr. Schaub had mentioned that my loader was slow and Colten Shipe mentioned that when his application's performance increased remarkably.

9/10/2020 8 hrs - Implemented the Registers Representations, and wasted 5 hours trying to figure out how to make the datagrid fill rows instead of columns, only to learn it could not be done simply.

9/11/2020 2 hrs - Implemented terminal panel, flags panel, fixing other issues with the window along the way. Also spent some time trying to figure out how to represent the data in the RAM array with a datagrid(failed miserably as of the time of writing this :( )

9/12/2020 3 hrs - Taking Dr. Schaub's advice, I reworked my Memory Grid using the class example of the gridview, which to my surprise only took about an hour and half to figure out. I then figured out how to use StringFormatting for my grid.

9/13/2020 3 hrs - Tried to figure out how to overlay elements, but could not find anything simple enough.

9/14/2020 1 hr - designed play button, and added it to GUI for run, and tested run using it.

9/15/2020 2 hrs - I implemented the Disassembly Grid using the same design I used for the Memory, and also implemented the highlighting to indicate which instruction the PC is currently on(majority of time went into figuring out how to find the item I needed, which was ridiculously simple once one knows how to do it).

9/16/2020 3 hrs - The former entry is combined with this one.

9/17/2020 4 hrs - Implemented shortcuts, reworked panel to auto-adjusting(somewhat working not fully functional)

9/18/2020 4 hrs - Implemented breakpoint dialog, had to rework register panel to actually update when it is supposed to. Added images for buttons, worked some more on resizing, and implemented stack panel layout.

Notes: Due to my late submission of the Loader, I was not able to start as early as I had desired. Also, a lot of the time was spent not actually writing code, but rather researching how to get a specific feature I desired in my window. If that research time is taken out, the total amount of coding and debugging is closer to 25 hours.

**Phase 3:** _SIM 1_ 27

9/18/2020 1 hr - created a draft for the Execute Phase 1

9/24/2020 6 hrs - organized the Instruction class into hierarchy, creating the different classes and subclasses
* also completed the implementation of the MOV class Execute Function

9/28/2020 1 hr - completed detailed design work entirely

9/29/2020 2 hrs - implemented Operand2 Class and BarrelShifter class and subclasses

9/30/2020 - Part of the hours above was spent on that day.

10/1/2020 6 hrs - Implemented all DataProcessing instructions and started testing them.
* Also implemented Tracing for the files;

10/2/2020 6 hrs - Completed Testing for all the DataProcessing(took 3 hrs); Started on LoadStore classes
* Also implemented Exec, and debugged Tracing.

10/19/2020 3 hrs - Wrote CMP instructions, and started on the Branch
10/20/2020 2 hrs - wrote B, BL, BX instructions

**Phase 4:** _SIM 2_ 38

10/21/2020 7hrs - Worked on implementing Branching instructions class and the CMP dataprocessing instruction

10/22/2020 1 hr - Trying to understand conditional execution and implementation, implemented traceall and reset additional functionality.

10/25/2020 6 hrs - Completely implemented conditional execution and representation in disassembly after more hours of research

10/26/2020 2 hrs - researched exception handling

10/27/2020 5 hrs - worked on status register processing by implementing MRS, MSR, SWI, and MOVS after working on B-Level I/O only to realize that it would be best go directly to A-Level I/O

10/28/2020 7 hrs - debugged I/O problems for 3 hrs and worked on resizing the GUI to look, neither which were willing to cooperate. 

11/2/2020 1 hr - troubleshooting I/O to no avail. All the code looks like it is doing what it is supposed to. The program is caught in a non-ending loop inside the swi 0x6a handler

11/3/2020 4 hrs - discovered where bug was after meticulously steping though each intruction to see where the code has stopped executing. Discovered 2 bugs: was changing SWI to the wrong exception mode, and movs was not updating CPSR register so the program could not switch modes which caused a loop. After fixing that, adjusted some variables and output started working.

11/4/2020 3 hrs - debugged input as it was not updating the Queue variable I had been using, and worked on the keyboard device so that it would accept capital letters, numbers, and return key.

11/5/2020 4 hrs - worked on GUI to fix resizing, finally managed to do so, while working on toolbar buttons to look more polished.

Though the work was not more than any of the other stages, there was certainly a lot more fine detail that if one was missed, the entire program is disfunctional, which is what happened to me at the very last stage. One variable I forgot to assign to the proper value costed me a couple days of work and late turn-in too, so I will be more careful next time.

