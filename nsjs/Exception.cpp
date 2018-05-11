#include "Exception.h"
#include "Memory.h"

#include <stdio.h>
#include <Windows.h>
#include <Dbghelp.h>

#pragma comment(lib, "Dbghelp.lib")

Exception::Exception(const char* message)
{
	this->except_message = message;
}

const char* Exception::Message()
{
	return this->except_message;
}

const char* Exception::Name()
{
	return "Exception";
}

void Exception::PrintStackTrace()
{
	printf("%s\n", this->Name());
	printf("\t%s\n", this->Message());
	unsigned int   i;
	void         * stack[1000];
	unsigned short frames;
	SYMBOL_INFO  * symbol;
	HANDLE         process;
	process = GetCurrentProcess();
	SymInitialize(process, NULL, TRUE);
	frames = CaptureStackBackTrace(0, 1000, stack, NULL);
	symbol = (SYMBOL_INFO*)Memory::Alloc(sizeof(SYMBOL_INFO) + 256 * sizeof(char), 1);
	symbol->MaxNameLen = MAX_PATH;
	symbol->SizeOfStruct = sizeof(SYMBOL_INFO);
	for (i = 0; i < frames; i++)
	{
		SymFromAddr(process, (DWORD64)(stack[i]), 0, symbol);
		printf("%02d: %s - 0x%I64X\n", i, symbol->Name, symbol->Address);
	}
	Memory::Free(symbol);
}