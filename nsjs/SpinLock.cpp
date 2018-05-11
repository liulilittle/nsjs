#include "SpinLock.h"
#include "Environment.h"
#include "Exception.h"

#include <Windows.h>

SpinLock::SpinLock()
{
	this->__refcount = 0x00L;
	this->__signal = 0x00L;
	this->__threadid = 0x00L;
}

SpinLock::~SpinLock()
{
	this->__refcount = 0x00L;
	this->__signal = 0x00L;
	this->__threadid = 0x00L;
}

void SpinLock::Enter(bool* localTaken)
{
	uint32_t timeval = INFINITE;
	this->Enter(localTaken, &timeval);
}

void SpinLock::Enter(bool* localTaken, uint64_t* iterations)
{
	this->Enter(localTaken, iterations, NULL);
}

void SpinLock::Enter(bool* localTaken, uint32_t* timeval)
{
	this->Enter(localTaken, NULL, timeval);
}

void SpinLock::Enter(bool* localTaken, uint64_t* iterations, uint32_t* timeval)
{
	if (localTaken == NULL)
	{
		throw new ArgumentNullException("localTaken");
	}
	if (*localTaken)
	{
		throw new ArgumentException("localTaken");
	}
	int threadid = GetCurrentThreadId();
	if (threadid == InterlockedCompareExchange(&__threadid, 0x00L, 0x00L))
	{
		*localTaken = true;
	}
	else
	{
		uint64_t startMilliseconds = GetSystemTickCount64();
		long count = 0;
		while (!*localTaken)
		{
			if (iterations && (*iterations != INFINITE && ++count > *iterations))
			{
				break;
			}
			if (timeval && *timeval != INFINITE)
			{
				uint64_t elapsedMilliseconds = GetSystemTickCount64() - startMilliseconds;
				if (elapsedMilliseconds >= *timeval)
				{
					break;
				}
			}
			if (InterlockedIncrement(&__signal) == 0x01) // 获取到锁信号
			{
				*localTaken = true;
				InterlockedExchange(&__threadid, threadid);
			}
		}
	}
	if (*localTaken)
	{
		InterlockedIncrement(&__refcount);
	}
}

void SpinLock::Exit()
{
	if (InterlockedCompareExchange(&__threadid, 0x00L, 0x00L))
	{
		throw new InvalidOperationException("this");
	}
	if (InterlockedDecrement(&__refcount) <= 0x00L)
	{
		InterlockedExchange(&__threadid, 0x00L);
		InterlockedExchange(&__signal, 0x00L);
	}
}
