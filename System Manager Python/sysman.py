#!/usr/bin/env python3
#-------------------------------------------------------------------------
# File:   sysman.py
# Author: Melchisedek Dulcio
# Descr:  Contains program for system monitoring and management services
#-------------------------------------------------------------------------

import sys, os              # for system level operations
import signal               # siganl handling
import subprocess           # launching external processes
import socket               # networking
import re, argparse         # for parsing
from datetime import datetime
import threading

import time

parent_pid = 0
running = True
clients_tracker = 0

class Sysman: 
    def __init__(self):
        pass
    
    def sysinfo(self, out_stream=sys.stdout):
        """ returns the number of users on the system and available memory """
        users = 0; mem = ""                 
        uproc = subprocess.Popen(["users"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
        
        for user in uproc.stdout:
            users += 1
        
        with open("/proc/meminfo", 'r') as mproc:
            mem = re.findall(r"MemFree:\s+(\d+)", mproc.read())

        ret = f"{users} users, {mem[0]} bytes available\n"
        out_stream.write(ret)
        out_stream.flush()
            
    def ps(self, out_stream = sys.stdout):
        """ lists processes in a table containing: pid, name of exec, process
            owner, physical memory consumed RSS, total CPU time"""
        cmd = "ps -eo pid,comm,user,rss,cputime"
        proc = subprocess.Popen(cmd.split(), stdout=subprocess.PIPE, 
                                stderr=subprocess.PIPE, universal_newlines=True)
        for line in proc.stdout:
            out_stream.write(line)
            out_stream.flush()
            
    def exec(self, cmd: str, out_stream = sys.stdout):
        """ execute cmd passed in param"""
        proc = 0
        try:
            proc = subprocess.Popen(cmd.split(), stdout=subprocess.PIPE, stderr=subprocess.PIPE,universal_newlines=True)
            for out in proc.stdout:
                out_stream.write(out)
                out_stream.flush()
        except Exception as e:
            print(e)

class Client_Threads(threading.Thread):
    lock = threading.Lock()
    def __init__(self, client_socket, conn_addr):
        threading.Thread.__init__(self, daemon=True)
        self.clnt_sckt = client_socket
        self.con_addr = conn_addr

    def run(self):
        sm = Sysman()
        with self.clnt_sckt.makefile('rw') as client_stream:
            client_stream.write("Sysman, at your service!\n")
            client_stream.flush()
            try:
                cmd = str(client_stream.readline()).split()
                while len(cmd) > 0 and running == True:
                    if cmd[0] == "SYSINFO":
                        sm.sysinfo(client_stream)
                    elif cmd[0] == "PS":
                        sm.ps(client_stream)
                    elif cmd[0] == "EXEC":
                        cmd.pop(0)
                        sm.exec(" ".join(cmd), client_stream)
                    elif cmd[0] == "QUIT":
                        break
                    cmd = client_stream.readline().split()
            except Exception:
                pass 
            finally:
                log(False, self.con_addr)
        self.clnt_sckt.close()

        return None

def monitor(handle: str):
    cmd = "tcpdump -tttt --immediate-mode -l -q --direction=in -n ip"
    srcip, logip, logprt = handle.split(":")

    udp_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    proc = subprocess.Popen(cmd.split(), stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)

    for line in proc.stdout:
        match = re.search(r"(.+)\b(\d\d:\d\d:\d\d)\.\d+\sIP\s(\d+\.\d+\.\d+\.\d+)\..*\s(\d+)$", line)
        
        if match == None:
            break
        date, time, ip, size = match.group(1, 2, 3, 4)
        if ip == srcip:
            f = f"{date} {time} {ip} {size}\n"
            udp_sock.sendto(f.encode(), (logip, int(logprt)))
    exit(0)

def log(msg_type: bool, conn_addr):
    global clients_tracker
    with Client_Threads.lock:
        if msg_type == True:
            clients_tracker += 1
            print(f"Connection request from: {conn_addr[0]}:{conn_addr[1]}. {clients_tracker} client(s) connected.")
        else:
            clients_tracker -= 1
            print(f"Client {conn_addr[0]}:{conn_addr[1]} disconnected. {clients_tracker} client(s) remaining.")
    
def sigint_handler(signal, frame):
    # TODO: sigint handler to maybe do a bit of cleanup work 
    global parent_pid, running
    running = False
    if parent_pid == os.getpid():
        print("Ctrl-C (SIGINT) detected. Shutting down...")
    sys.exit()


def main(argv: list) -> int:
    global parent_pid, running
    parent_pid = os.getpid()
    signal.signal(signal.SIGINT, sigint_handler)
    
    parser = argparse.ArgumentParser(
        usage="./sysman [--sysinfo] [--ps] [--exec <command>] [--listen <port>] [--monitor <srcipaddr>:<logipaddr>:<logport>]",
        description="Provides a system monitoring and management service")
    parser.add_argument('--sysinfo', action="store_true", required=False, help="Displays number of users and available memory")
    parser.add_argument('--ps', action="store_true", required=False, help="Displays process table")
    parser.add_argument('--exec', required=False, help="execute command provided")
    parser.add_argument('--listen', nargs='?', type=int, required=False, help="open TCP server connection on port provided")
    parser.add_argument('--monitor', required=False, help="opens network monitoring for packets from <srcipaddr>")

    args = parser.parse_args()
    sm = Sysman()
    if args.sysinfo:
        sm.sysinfo()
    elif args.ps:
        sm.ps()
    elif args.exec:
        sm.exec(args.exec)
    elif args.listen:
        if args.monitor:
            network_monitor = threading.Thread(target=monitor, args=(args.monitor,), daemon=True)
            network_monitor.start()
        #---------------Server logic ---------------------------
        serv_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        serv_sock.bind(('', args.listen))
        serv_sock.listen(1)

        worker_sock = 0
        try:
            print(f"Sysman listening on port {args.listen}. Waiting for incoming requests..." )
            while running:
                worker_sock, conn_addr = serv_sock.accept()
                log(True, conn_addr)

                client = Client_Threads(worker_sock, conn_addr)
                client.start()
        finally:
            if worker_sock:
                worker_sock.close()
            serv_sock.close()        
    else:
        parser.print_help()

    return 0

if __name__ == "__main__":
    rc = main(sys.argv)
    exit(rc)
    