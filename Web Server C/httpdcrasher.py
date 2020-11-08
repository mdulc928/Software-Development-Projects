#!/usr/bin/env python3
import socket
import sys

def sendrequest(req, getreply):
    # Create a socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Connect to the remote host and port
    sock.connect((sys.argv[1], int(sys.argv[2])))

    dispReq = req.replace(b"\n", b"\\n")
    if len(dispReq) > 50:
        dispReq = dispReq[:50]
    print("Sending {0}...".format(dispReq))

    # Send a request to the host
    sock.send(req)

    if getreply:
        # Get the host's response, no more than, say, 1,024 bytes
        response_data = sock.recv(1024)
        firstline = response_data.split(b"\n")
        print("Got '{0}'".format(firstline[0]))

    # Terminate
    sock.close()

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: httpdcrasher.py hostname port\n")
        sys.exit(0)
      
    sendrequest(b"GET / HTTP/1.0\r\n\r\n", 1); # this request is valid but refers to a directory file, which may cause problems
    sendrequest(b"MUSH\r\n\r\n", 1); # invalid request
    sendrequest(b"MUSH /stuff HTTP/1.0\r\n\r\n", 1); # invalid request
    sendrequest(b"GET /" + (b'X' * 50000) + b" HTTP/1.0\r\n\r\n", 1); # a valid path with 50,000 X's
    sendrequest(b"GET / HTTP/1.0", 0); # invalid request (no newline)
