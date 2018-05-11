#ifndef THREAD_H
#define THREAD_H

#include "NSJSVirtualMachine.h"

#include <stdio.h>
#include <stdlib.h>
#include <v8.h>

namespace Thread
{
	void Initialize(NSJSVirtualMachine& machine);
	void usleep(const v8::FunctionCallbackInfo<v8::Value>& info);
	void sleep(const v8::FunctionCallbackInfo<v8::Value>& info);
	void nanosleep(const v8::FunctionCallbackInfo<v8::Value>& info);
}
#endif