#ifndef MEMORY_H
#define MEMORY_H

#include <stdint.h>
#include "Monitor.h"

class Memory
{
public:
	static void* Alloc(uint32_t count, uint32_t size);
	static void* Alloc(uint32_t size);
	static void Free(const void* memory);
	static bool IsNedallocAllocation();
	static bool IsMallocAllocation();
};
#endif