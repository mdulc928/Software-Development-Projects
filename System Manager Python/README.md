# Objective
A program that provides a management and monitoring service.

# Specifications
The program must support the following command line arguments:

```./sysman [--sysinfo] [--ps] [--exec <command>] [--listen <port>]```
### These options are as follows:

* **--sysinfo** should display the following information about the system: number of users, available memory.
* **--ps** should display a table of the current list of processes, including pid, name of executable, owner of process, physical memory consumed by process (RSS), cumulative CPU time.
* **--exec** should execute the requested command and display the result. The command must be provided as a single quoted string, like this:
* **./sysman** --exec 'ls -l /home'
* **--listen** should start the program in server mode. It should open a TCP server socket on the specified port and handle incoming connections as described in “Server Mode” below.
The options are mutually exclusive: only one will be supplied. 

## Total Hours: 14 Hrs

| Date | Accomplished | Time|
|---|---|---|
| 4/21/2020 | Getting set up | 30 mins |
| 4/24/2020 | Commenting and more set up | 50 minutes |
| 4/25/2020 | implementing basics | 3 hours |
| 4/26/2020 | Server and IO Issues | 4 hours |
| 4/26/2020 | 90 & 100 Level, having to fix stopping thread issue | 3 hrs |
| 4/29/2020 | EC attempt | 2 hours | 