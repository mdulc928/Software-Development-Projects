import sys, os, subprocess, re

cmd = "ls -G -l /proc"
proc = subprocess.Popen(cmd.split(), stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)

proc.stdout.readline()
for entry in proc.stdout:
    match = re.search(r"[drxw-]+\s\d+\s([^\s]+)\s+\d.*\s(\d+)$", entry)
    if match == None:
        break
    owner, pid = match.group(1, 2)
    
    path = f"/proc/{pid}/status"
    exect = 0
    size = 0
    try:
        with open(path, "r") as comm:
            exect= re.findall(r"(?:Name:\s+([^\s]+))", comm.readline())
            size = re.findall(r"VmRSS:\s+(\d+)", comm.read())
            print(exect, size)
        path = f"/proc/{pid}/stat"
        cptime = 0
        stime = 0 
        with open(path, "r") as ctime:
            info = str(ctime.read()).split()
            cptime = info[13]

        

    finally:
        pass
    
   


    uptm = 0
    with open("/proc/uptime", "r") as rtime:
        uptm = rtime.readline().split()[0]

    print(owner, pid )