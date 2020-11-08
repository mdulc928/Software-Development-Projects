#include<stdio.h>
#include<string.h>

int main(){
	char buff[10]; 
	size_t len = 0;
	len = fread(buff, sizeof(buff), sizeof(buff), stdin);
	fprintf(stdout, "%s", buff);
	printf("hllee");
	return 0;
}