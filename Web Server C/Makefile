CFLAGS = -g

#target that builds both the echoserver and client
all: echoclient debug

#builds the echoclient
echoclient: echoclient.c eznet.c
	gcc $(CFLAGS) -Wall -DSHOW_LOG_ERROR -Werror -oechoclient echoclient.c eznet.c

#builds the echo server
#webserver: webserver.c eznet.c utils.c
#	gcc $(CFLAGS) -Wall -DSHOW_LOG_ERROR -Werror -owebserver webserver.c eznet.c

#removes the client and server executables
clean:
	rm *.o webserver

#Actual Executable
debug: CFLAGS += -std=gnu11 -fsanitize=address -fno-omit-frame-pointer -Wall -Werror
debug: webserver.o utils.o eznet.o
	gcc $(CFLAGS) webserver.o utils.o eznet.o -lbsd -o webserver

release: webserver.o utils.o eznet.o
	gcc -g webserver.o utils.o eznet.o -lbsd -o webserver

#All my other symbol files
webserver.o:
	gcc -g -c webserver.c -o webserver.o


utils.o:
	gcc -g -c utils.c -o utils.o


eznet.o:
	gcc -g -c eznet.c -o eznet.o

