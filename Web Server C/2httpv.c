// File: httpv.c
// Submitted by: Melchisedek Dulcio (mdulc928)
//
#define _GNU_SOURCE
#include <stdio.h>
#include <string.h>
#include <bsd/string.h>
#include <stdlib.h>
#include <limits.h>
#include <errno.h>
#include <ctype.h>

#define MAX_HEADERS 10

//----------------Macros: parseHttp return codes-----------------------------
#define GOOD_RQ 1
#define GENBAD_RQ -1
#define IO_ERR -2
#define ALLOC_FAILED -3
#define INVLD_VERB -4
#define INVLD_PATH -5
#define MSSNG_VRSN -6
#define OTHER_ERR -7

//Define my http_header type with the components of the header
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

//opens file if provided and returns it else it returns stdin
FILE *parseArgs(int argc, char **argv)
{
    FILE *in = NULL;
    if(argc > 1){
        in = fopen(argv[1], "r");
        in != NULL ? fprintf(stdout, "Reading from %s...\n", argv[1]) : fprintf(stderr, "Unable to open %s\n", argv[1]);
    }else{
        fprintf(stdout, "Reading from stdin...\n");
        in = stdin;
    }
    return in;
}

// Returns GOOD_RQ on success,
// GENBAD_RQ on invalid HTTP request,
// IO_ERR on I/O error,
// ALLOC_FAILED on malloc failure
int parseHttp(FILE *in, http_request_t **request) {
	/*------------------Declaration of resources-------------------*/
    http_request_t *req = NULL;
    int rc = OTHER_ERR, cnt = 0;
	
	char *buff = NULL, *str_handle = NULL, *sv_ptr = NULL;
	char *sp = " ", *vld_chars = "./";
	size_t buff_ptr = 0, bufflen = 0;

    /*------------Allocate memory for resources---------------*/
	req = calloc(1, sizeof(http_request_t));
	if(req == NULL){
		rc = ALLOC_FAILED;
		goto cleanup;
	}

    /*---------------Read HTTP request from <in>---------------------*/	
	if((bufflen = getline(&buff, &buff_ptr, in)) == -1){
		rc = IO_ERR;
		goto cleanup;
	}
	if(feof(in)){
		rc = IO_ERR;
		goto cleanup;
	}
	
	if(buff[bufflen - 1] == 10){buff[bufflen - 1] = 0; bufflen -= 1;} //check for new line;
	if(buff[bufflen - 1] == 13){buff[bufflen - 1] = 0; bufflen -= 1;} //check for carriage return;
	
	str_handle = strtok_r(buff, sp, &sv_ptr);
	req->verb =  strdup(str_handle);

	str_handle = strtok_r(NULL, sp, &sv_ptr);
	req->path =  strdup(str_handle);
	
	req->version =  strdup(sv_ptr);
	
	/*-------------------Validate HTTP--------------------------------------*/
	if(strcmp(req->verb, "GET") != 0 && strcmp(req->verb, "POST") != 0){
		rc = INVLD_VERB;
		goto cleanup;
	}
	
	for(int i = 0; i < strlen(req->path); ++i){
		if(i == 0 && req->path[i] != '/'){
			rc = INVLD_PATH;
			goto cleanup;
		}
		if((isalnum(req->path[i]) == 0) && (strchr(vld_chars, req->path[i]) == NULL)){
			rc = INVLD_PATH;
			goto cleanup;
		}
	}
	
	for(int i = 0; i < strlen(req->version); ++i){
		if(isspace(req->version[i]) != 0 || isprint(req->version[i]) == 0){
			rc = MSSNG_VRSN;
			goto cleanup;
		}
	}
	
	/*-------------------------Parsing Headers--------------------------*/
	while(((bufflen = getline(&buff, &buff_ptr, in)) != -1) && cnt < MAX_HEADERS){
		if(bufflen != 2 && buff[bufflen - 1] == 10){buff[bufflen - 1] = 0; bufflen -= 1;} //check for new line;
		if(bufflen != 2 && buff[bufflen - 2] == 13){buff[bufflen - 2] = 0; bufflen -= 1;} //check for carriage return;
		
		str_handle = strtok_r(buff, ":", &sv_ptr);
		
		asprintf(&req->headers[cnt].name, "%s", str_handle);
		asprintf(&req->headers[cnt].value, "%s", ++sv_ptr);
		++cnt;
	}
	req->num_headers = cnt - 1; 
	
	while(getline(&buff, &buff_ptr, in) != EOF){;}
	if(strcmp(buff, "\r\n") != 0){
		rc = IO_ERR;
		goto cleanup;
	}
	if(ferror(in)){
		rc = IO_ERR;
		goto cleanup;
	}
	
    /*-----------------On success, return req-----------------------*/
	*request = req;
    rc = GOOD_RQ;
	
	/*--------------------Cleanup Code----------------------*/
cleanup:	
    if(rc != GOOD_RQ){
		free(req->verb);
		free(req->path);
		free(req->version);
		for(int i = 0; i < MAX_HEADERS; ++i){
			free(req->headers[i].name);
			free(req->headers[i].value);
		}
		free(req);  
	}
	free(buff);				// It's OK to free() a NULL pointer.
    return rc;
}

//--------------------Main Program-------------------------
int main(int argc, char **argv)
{
    FILE *f = parseArgs(argc, argv);
    if (f == NULL) {
        exit(1);
    }

    http_request_t *request = NULL;
    int result = 0;
	
    result = parseHttp(f, &request);
	
	char* err_indc[3] = {"Invalid verb", "Invalid path", "Missing Version"};	
	switch (result) {
		case GOOD_RQ:
			printf("Verb: %s\n", request->verb);
			printf("Path: %s\n", request->path);
			printf("Version: %s\n", request->version);
			printf("\n%d header(s):\n", request->num_headers);
			for (int i = 0; i < request->num_headers; ++i) {
				 printf("* %s is %s\n", request->headers[i].name, request->headers[i].value);
			}
			break;
		case MSSNG_VRSN ... INVLD_VERB:
			fprintf(stderr, "** ERROR: Illegal HTTP stream(%s).\n", err_indc[(result + 4) * (-1)]);
			goto main_cleanup;
			break;
		case IO_ERR:
			fprintf(stderr, "** ERROR: I/O error while reading request.\n");
			break;
		case ALLOC_FAILED:
			fprintf(stderr, "** ERROR: malloc failure.\n");
			break;
		default:
			printf("Unexpected return code %d.\n", result);
    }

main_cleanup:
	if(result == GOOD_RQ){
		free(request->verb);
		free(request->path);
		free(request->version);
		
		for(int i = 0; i < request->num_headers + 1; ++i){
			free(request->headers[i].name);
			free(request->headers[i].value);
		}
	}
	free(request);
    fclose(f);
}