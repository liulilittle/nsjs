#ifndef EXTENSION_H
#define EXTENSION_H

#include <stdint.h>
#include <v8.h>
#include "NSJSVirtualMachine.h"

namespace Extension
{
	void Initialize(NSJSVirtualMachine& machine);
}
#endif