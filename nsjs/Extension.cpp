#include "Extension.h"
#include "Environment.h"
#include "Environment.h"
#include "Thread.h"

#include <v8.h>
#include <Windows.h>

void Extension::Initialize(NSJSVirtualMachine& machine)
{
	Thread::Initialize(machine);
	Environment::Initialize(machine);
}