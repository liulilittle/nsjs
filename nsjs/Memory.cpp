#include "memory.h"

#ifndef _NEDMALLOC_NO_USE
#include "nedmalloc.h"
#endif

#include <stdint.h>
#include <malloc.h>
#include <Windows.h>

#ifdef NEDMALLOC_H

void* nedalloc_alloc(size_t size, size_t align)
{
	if (size <= 0)
	{
		return NULL;
	}
	const size_t SIMD_ALIGNMENT = 16;
	return align ? nedalloc::nedmemalign(align, size) : nedalloc::nedmemalign(SIMD_ALIGNMENT, size);
}

void* nedalloc_alloc(size_t size)
{
	if (size <= 0)
	{
		return NULL;
	}
	return nedalloc::nedmalloc(size);
}

void nedalloc_free(void* ptr)
{
	if (ptr != NULL)
	{
		nedalloc::nedfree(ptr);
	}
}

#endif

void* Memory::Alloc(uint32_t count, uint32_t size)
{
	if (count <= 0 || size <= 0)
	{
		return NULL;
	}
#ifdef NEDMALLOC_H
	void* memory = NULL;
	memory = nedalloc_alloc(count * size);
	return memory;
#else
	return new char[count * size];
#endif
}

void* Memory::Alloc(uint32_t size)
{
	if (size <= 0)
	{
		return NULL;
	}
#ifdef NEDMALLOC_H
	void* memory = NULL;
	memory = nedalloc_alloc(size);
	return memory;
#else
	return malloc(size);
#endif
}

void Memory::Free(const void* memory)
{
	if (memory != NULL || IsBadReadPtr(memory, 0x01))
	{
#ifdef NEDMALLOC_H
		nedalloc_free((void*)memory);
#else
		delete[] memory; 
#endif
	}
}

bool Memory::IsNedallocAllocation()
{
#ifdef NEDMALLOC_H
	return true;
#else
	return false;
#endif
}

bool Memory::IsMallocAllocation()
{
#ifndef NEDMALLOC_H
	return true;
#else
	return false;
#endif
}
