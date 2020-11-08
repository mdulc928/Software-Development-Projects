#!/bin/bash

echo -en "GET /test1.txt HTTP/1.1\r\n\r\n" | nc localhost 8080
echo -en "GET /test2.stuff.jpeg HTTP/1.1\r\n\r\n" | nc localhost 8080
echo -en "GET /test3.missing HTTP/1.1\r\n\r\n" | nc localhost 8080
echo -en "GET /test4/index.htm HTTP/1.1\r\n\r\n" | nc localhost 8080
echo -en "OOPS /test1.txt HTTP/1.1\r\n\r\n" | nc localhost 8080
echo -en "ILOVEPYTHON" | nc localhost 8080
