  
/* Web Server: an example usage of EzNet
 * (c) 2016, Bob Jones University
 */
#define _GNU_SOURCE

#include <stdbool.h>    // For access to C99 "bool" type
#include <stdio.h>      // Standard I/O functions
#include <stdlib.h>     // Other standard library functions
#include <bsd/string.h>	// Non-standard library string functions
#include <string.h>     // Standard string functions
#include <ctype.h>		// Character checking functions
#include <errno.h>      // Global errno variable

#include <stdarg.h>     // Variadic argument lists (for blog function)
#include <time.h>       // Time/date formatting function (for blog function)

#include <unistd.h>     // Standard system calls
#include <signal.h>     // Signal handling system calls (sigaction(2))

#include <wait.h>		// Child process cleanup
#include <sys/types.h>	// Access to system types 

#include "eznet.h"      // Custom networking library
#include "utils.h"

#define MAX_HEADERS 10
//#define PATH_MAX 4096    /* # chars in a path name including nul */

//----------------Macros: Http return codes-----------------------------
#define HTTP_OK 0
#define FILE_OUT 1
#define NE_FILE 2
#define GENBAD_RQ 3
#define NO_SUPPORT 4
#define OTHER_ERR 5
#define IO_ERR 6

//GLOBAL: Reponses and Codes for HTTP Processing that must be globally accessible.
struct http_reponse {
	const int code;
	const char *reason;
	const char *message;
} http_reponses[] = {
	{.code = 200, .reason = "OK", .message = "Success"},
	{.code = 403, .reason = "Forbidden", .message = "Requested file outside the \"document root\""},
	{.code = 404, .reason = "Not Found", .message =  "Requested a non-existent file"},
	{.code = 400, .reason = "Bad Request", .message = "Malformed Request"},
	{.code = 501, .reason = "Not Implemented", .message = "Unsupported request \"verb\""},
	{.code = 500, .reason = "Internal Server Error", .message = "Could not process request"},
};

// Define the http_header type with the components of the header
typedef struct http_header {
    char *name;
    char *value;
} http_header_t;

//Define http_request type with its compenents
typedef struct http_request {
    char *verb;
    char *path;
    char *version;
    int num_headers;
    http_header_t headers[MAX_HEADERS];
} http_request_t;

//Define a key/value pair for my Content-Types
typedef struct {
	const char *key;
	const char *value;
} dict_t;

//array containing a list of key-value pairs for my types of file endings
dict_t types[] = {
	{.key = "html", .value = "text/html",},
	{.key = "htm", .value = "text/html",},
	{.key = "gif", .value = "image/gif",},
	{.key = "jpeg", .value = "image/jpeg",},
	{.key = "jpg", .value = "image/jpeg",},
	{.key = "png", .value = "image/png",},
	{.key = "css", .value = "text/css",},
	{.key = "txt", .value = "text/plain",},
	{.key = NULL, .value = "application/octet-stream",},
}; 

// Returns the Content-type of the file requested by the client
char *getType(http_request_t *req){
	char *ext, *type = NULL;
	int i = 0;
	ext = strrchr(req->path, '.');
	ext != NULL ? ++ext : NULL;
	
	while(type == NULL && i < 9){
		if(ext == NULL  || types[i].key == NULL){
			i = 8;
			type = (char*)types[i].value;
		}else if(strcmp(ext, types[i].key) == 0){
			type = (char *)types[i].value;
		}
		++i;
	}
	return type;
}

// GLOBAL: settings structure instance
struct settings {
    const char *bindhost;   // Hostname/IP address to bind/listen on
    const char *bindport;   // Portnumber (as a string) to bind/listen on
	const char *bindroot;	// Root directory from which to serve files
	int  bindLimt; 	// Number of clients that can be connected
} g_settings = {
    .bindhost = "localhost",    // Default: listen only on localhost interface
    .bindport = "6000",         // Default: listen on TCP port 6000
	.bindroot = NULL,			// Default: use the current directory
	.bindLimt = 5,
};

// Parse commandline options and sets g_settings accordingly.
// Returns 0 on success, -1 on false...
int parse_options(int argc, char * const argv[]) {
    int ret = -1; 

    char op;
    while ((op = getopt(argc, argv, "h:p:r:w:")) > -1) {
        switch (op) {
            case 'h':
                g_settings.bindhost = optarg;
                break;
            case 'p':
                g_settings.bindport = optarg;
                break;
			case 'r':
				g_settings.bindroot = optarg;
				break;
			case 'w':
				g_settings.bindLimt = strtol(optarg, NULL, 10); //should I or not check for error?
				break;
            default:
                // Unexpected argument--abort parsing
                goto cleanup;
        }
    }

    ret = 0;
cleanup:
    return ret;
}

// GLOBAL: flag indicating when to shut down server
volatile bool server_running = false;

// GLOBAL: tracker for the number of clients connected
volatile int clients_connected = 0;

// GLOBAL: Parent's pid
pid_t parent_id = 0;

//GLOBAL: 
volatile bool detected_sigpipe = false;

// SIGINT handler that detects Ctrl-C and sets the "stop serving" flag
void sigint_handler(int signum) {
    getpid() == parent_id ? blog("Ctrl-C (SIGINT) detected; shutting down...") : NULL;
    server_running = false;
}

// SIGCHLD handler that cleans up when child has died
void sigchld_handler(int signum){
	while(wait3(NULL, WNOHANG, NULL) > 0){
		--clients_connected;
		blog("Number of clients connected: %d", clients_connected);
	}
}

void sigpipe_handler(int signum){
	blog("SIGPIPE detected, closing client ...");
	detected_sigpipe = true;
}

/*---------------------Does my HTTP Validation Logic--------------------------------*/
int validateHttp(http_request_t *req){
	int retcod = -1, len = 0;
	char *vld_chars = "HTTP/", head[6];

	if(strcmp(req->verb, "GET") != 0){
		retcod = NO_SUPPORT;
		goto cleanup;
	}	
	if(req->path[0] != '/'){
		retcod = GENBAD_RQ;
		goto cleanup;
	}

	strlcpy(head, req->version, sizeof(head));
	for(int i = 0; i < strlen(req->version); ++i){
		if(isspace(req->version[i]) != 0 || isprint(req->version[i]) == 0 ||
		 strcmp(vld_chars, head) != 0){
			retcod = GENBAD_RQ;
			goto cleanup;
		}
	}

	retcod = HTTP_OK;

cleanup:
	return retcod;
}

// Processes URL and returning the error or success codes. 
int parseHttp(FILE *in, http_request_t **request) {
	/*------------------Declaration of resources-------------------*/
    http_request_t *req = NULL;
    int rc = OTHER_ERR, cnt = 0;
	
	char *buff = NULL, *str_handle = NULL, *sv_ptr = NULL;
	char *verb = NULL, *path = NULL, *sp = " ";
	
	size_t buff_ptr = 0, bufflen = 0;

    /*------------Allocate memory for resources---------------*/
	req = calloc(1, sizeof(http_request_t)); 	//will need to watch out for that.
	if(req == NULL){
		rc = OTHER_ERR;
		goto cleanup;
	}

    /*---------------Read HTTP request URL from <in> & Tokenize---------------------*/
	bufflen = getline(&buff, &buff_ptr, in);	
	if( bufflen < 2 || feof(in) > 0 || ferror(in)){
		rc = IO_ERR;
		goto cleanup;
	}

	if(buff[bufflen - 1] == 10){buff[bufflen - 1] = 0; bufflen -= 1;} //check for new line;
	if(buff[bufflen - 1] == 13){buff[bufflen - 1] = 0; bufflen -= 1;} //check for carriage return;
	
	verb = strtok_r(buff, sp, &sv_ptr);
	path = strtok_r(NULL, sp, &sv_ptr);

	if(verb == NULL || path == NULL || sv_ptr == NULL){
		rc = IO_ERR;
		goto cleanup;
	}

	req->verb =  strdup(verb); req->path =  strdup(path);
	req->version =  strdup(sv_ptr);
	
	if((rc = validateHttp(req)) != HTTP_OK){ goto cleanup;}				//Validate URL format
    /*-----------------On success, return req-----------------------*/
	*request = req;
    rc = HTTP_OK;
	
	/*--------------------Cleanup Code----------------------*/
cleanup:
	while(!feof(in) && rc != IO_ERR){
		getline(&buff, &buff_ptr, in);

		//Check for Error after or before is the question
		if(strcmp(buff, "\r\n") == 0){ break; }
		if(ferror(in) || strlen(buff) == 0){
			rc = IO_ERR;
			goto cleanup;
		}
	}

    if(rc != HTTP_OK){
		free(req->verb); free(req->path);
		free(req->version); free(req);  
	}
	
	free(buff); 	// It's OK to free() NULL pointer.
    return rc;
}

// Sends HTTP Response for a correctly formatted request back to client
int  respond(FILE *fd_client, http_request_t *req, int code){
	FILE *fresult = NULL;
	int ret = 0, res = 0;
	char buff[1], *full_path = NULL;
	size_t bufflen = 0, plen = 0;

	g_settings.bindroot != NULL ? asprintf(&full_path, "%s%s", g_settings.bindroot, req->path) : asprintf(&full_path, "%s", ++req->path);
	if((fresult = fopen(full_path, "r")) == NULL){
		res = 1;
		code = NE_FILE;
	}
	g_settings.bindroot == NULL ? --req->path : NULL;
	
	fprintf(fd_client, "%s %d %s\n", req->version, http_reponses[code].code, http_reponses[code].reason);
	if(res == 0){
		fprintf(fd_client, "Content-Type: %s\n\n", getType(req));		
		fflush(fd_client);
		if(detected_sigpipe){goto cleanup;}

		while(fread(buff, sizeof(char), 1, fresult)){
			fputc(*buff, fd_client);
			if(detected_sigpipe){goto cleanup;}		
		}
		fflush(fd_client);	
		if(detected_sigpipe){goto cleanup;}
	}else{
		if(detected_sigpipe){goto cleanup;}	
		fprintf(fd_client, "Content-Type: text/plain\n\n");
		fprintf(fd_client, "**Error**: %s\n", http_reponses[code].message);
		fflush(fd_client);
		if(detected_sigpipe){goto cleanup;}
	}
cleanup:
	free(full_path);
	if(res == 0){fclose(fresult);}
}

// Connection handling logic: reads/echos lines of text until error/EOF,
// then tears down connection.
void handle_client(struct client_info *client) {
    FILE *stream = NULL;
	http_request_t *request = NULL;
    int result = 0;
	
	//	char* err_indc[3] = {"Invalid verb", "Invalid path", "Missing Version"};
    // Wrap the socket file descriptor in a read/write FILE stream
    // so we can use tasty stdio functions like getline(3)
    // [dup(2) the file descriptor so that we don't double-close;
    // fclose(3) will close the underlying file descriptor,
    // and so will destroy_client()]
    if ((stream = fdopen(dup(client->fd), "r+"))== NULL) {
        perror("unable to wrap socket");
        goto main_cleanup;
    }

	result = parseHttp(stream, &request);
	
	switch (result) {		
		case HTTP_OK ... NE_FILE:		
			respond(stream, request, result);
			break;
		case GENBAD_RQ ... OTHER_ERR:
			if(!detected_sigpipe){
				fprintf(stream, "HTTP/1.0 %d %s\n", http_reponses[result].code, http_reponses[result].reason);
				fprintf(stream, "Content-Type: text/plain\n\n");
				fprintf(stream, "**Error**: %s\n", http_reponses[result].message);
				fflush(stream);
			}			
			break;
		default:
			blog("I/O Error from: %s:%d", client->ip, client->port);
			break;
    }

main_cleanup:
	if(result == HTTP_OK){
		free(request->verb);
		free(request->path);
		free(request->version);
	}
	free(request);
	
    // Shutdown this client
    if (stream) fclose(stream);
    printf("\tSession ended.\n");
	exit(0);
}

int main(int argc, char **argv) {
    int ret = 1;

    // Network server/client context
    int server_sock = -1;

	// Capture PID of parent
	parent_id = getpid();

    // Handle our options
    if (parse_options(argc, argv)) {
        printf("usage: %s [-p PORT] [-h HOSTNAME/IP]\n", argv[0]);
        goto cleanup;
    }

    // Install signal handler for SIGINT, SIGCHILD, and SIGPIPE
    struct sigaction sa_handlers[] = {
		{.sa_handler = sigint_handler},
		{.sa_handler = sigchld_handler},
		{.sa_handler = sigpipe_handler},
    };

    if (sigaction(SIGINT, &sa_handlers[0], NULL)) {
        LOG_ERROR("sigaction(SIGINT, ...) -> '%s'", strerror(errno));
        goto cleanup;
    }
	if(sigaction(SIGCHLD, &sa_handlers[1], NULL)) {
		LOG_ERROR("sigaction(SIGCHILD, ...) -> '%s'", strerror(errno));
		goto cleanup;
	}
	if(sigaction(SIGPIPE, &sa_handlers[2], NULL)) {
		LOG_ERROR("sigaction(SIGPIPE, ...) -> '%s'", strerror(errno));
		goto cleanup;
	}

    // Start listening on a given port number
    server_sock = create_tcp_server(g_settings.bindhost, g_settings.bindport);
    if (server_sock < 0) {
        perror("unable to create socket");
        goto cleanup;
    }
    blog("Bound and listening on %s:%s", g_settings.bindhost, g_settings.bindport);

    server_running = true;
    while (server_running) {
        struct client_info client;
		if(clients_connected == g_settings.bindLimt){
			continue;
		}
        // Wait for a connection on that socket
        if (wait_for_client(server_sock, &client)) {
            // Check to make sure our "failure" wasn't due to
            // a signal interrupting our accept(2) call; if
            // it was  "real" error, report it, but keep serving.
            if (errno != EINTR) { perror("unable to accept connection"); }
        } else {
			blog("connection from %s:%d", client.ip, client.port);
			++clients_connected;
			blog("Number of clients connected: %d", clients_connected);

			if(fork()){
				destroy_client_info(&client);
			}else{
				close(server_sock);
				handle_client(&client); // Client gets cleaned up in here
			}
        }
    }
    ret = 0;

cleanup:	
    if (server_sock >= 0) close(server_sock);
    return ret;
}
