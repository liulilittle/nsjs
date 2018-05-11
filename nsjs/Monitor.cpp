#include "Monitor.h"
#include "Memory.h"

#include <Windows.h>

Monitor::Monitor()
{
	cs = Memory::Alloc(sizeof(CRITICAL_SECTION));
	memset(cs, 0, sizeof(CRITICAL_SECTION));
	InitializeCriticalSection((CRITICAL_SECTION*)cs);
}

Monitor::~Monitor()
{
	DeleteCriticalSection((CRITICAL_SECTION*)cs);
	Memory::Free(cs);
}

void Monitor::Enter()
{
	EnterCriticalSection((CRITICAL_SECTION*)cs);
}

void Monitor::Exit()
{
	LeaveCriticalSection((CRITICAL_SECTION*)cs);
}
