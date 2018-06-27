#include "Thread.h"
#include "Environment.h"

#include <stdint.h>
#include <Windows.h>
#include <time.h>
#include <queue>
#include <string>

using v8::Value;
using v8::FunctionCallbackInfo;
using v8::Integer;
using v8::String;
using v8::Local;
using v8::Isolate;
using v8::Handle;
using v8::Function;
using v8::Context;
using v8::NewStringType;

#define SLEEPRX100NS_SPIN(ns, radix) \
{ \
	if (ns > 0) \
	{ \
		LARGE_INTEGER nBeginTime; \
		LARGE_INTEGER nCurrTime; \
		LARGE_INTEGER nFreq; \
		QueryPerformanceFrequency(&nFreq); \
		LONGLONG nEndValue = nFreq.QuadPart * ns / radix; \
		QueryPerformanceCounter(&nBeginTime); \
		do \
		{ \
			QueryPerformanceCounter(&nCurrTime); \
		} while ((nCurrTime.QuadPart - nBeginTime.QuadPart) < nEndValue); \
	} \
} 
#define SLEEPRX100NS_OSCORE(ns, radix) \
{ \
	if(ns > 0) \
	{ \
		LARGE_INTEGER duetime; \
		duetime.QuadPart = -radix * ns; \
		HANDLE hWaitTimer = CreateWaitableTimer(NULL, true, NULL); \
		SetWaitableTimer(hWaitTimer, &duetime, 0, NULL, NULL, false); \
		while (MsgWaitForMultipleObjects(1, &hWaitTimer, false, INFINITE, QS_TIMER)); \
		CloseHandle(hWaitTimer); \
	} \
}
#define SLEEPRX1MS(ms) \
{ \
	if (ms > 0) \
	{ \
		timeBeginPeriod(1); \
		Sleep(ms); \
		timeEndPeriod(1); \
	} \
}

void Thread::nanosleep(const FunctionCallbackInfo<Value>& info)
{
	uint64_t ns = info[0]->ToInteger()->Value();
	SLEEPRX100NS_SPIN(ns, 10000000);
}

void Thread::usleep(const FunctionCallbackInfo<Value>& info)
{
	uint64_t us = info[0]->ToInteger()->Value();
	SLEEPRX100NS_SPIN(us, 1000000);
}

void Thread::sleep(const FunctionCallbackInfo<Value>& info)
{
	uint32_t ms = info[0]->ToInt32()->Value();
	SLEEPRX1MS(ms);
}

void Thread::Initialize(NSJSVirtualMachine& machine)
{
	NSJSVirtualMachine::ExtensionObjectTemplate* threading = new NSJSVirtualMachine::ExtensionObjectTemplate;
	if (!machine.GetExtension().SetObject("Thread", threading))
	{
		delete threading;
	}
	else
	{
		threading->SetFunction("usleep", &Thread::usleep);
		threading->SetFunction("sleep", &Thread::sleep);
		threading->SetFunction("nanosleep", &Thread::nanosleep);
	}
}
