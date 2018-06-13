#include "SpinLock.h"
#include "Environment.h"
#include "Exception.h"

#include <Windows.h>

SpinLock::SpinLock()
{
	this->m_refcount = 0x00;
	this->m_signal = 0x00;
	this->m_threadid = 0x00;
}

SpinLock::~SpinLock()
{
	this->m_refcount = 0x00;
	this->m_signal = 0x00;
	this->m_threadid = 0x00;
}

void SpinLock::Enter(bool& localTaken)
{
	int32_t tv = INFINITE;
	this->Enter(localTaken, &tv);
}

void SpinLock::Enter(bool& localTaken, uint32_t* iterations)
{
	this->Enter(localTaken, iterations, NULL);
}

void SpinLock::Enter(bool& localTaken, int32_t* timeval)
{
	this->Enter(localTaken, NULL, timeval);
}

void SpinLock::Enter(bool& localTaken, uint32_t* iterations, int32_t* timeval)
{
	if (localTaken)
	{
		throw new ArgumentException("localTaken");
	}
	uint32_t threadid = (uint32_t)GetCurrentThreadId();
	if (threadid == InterlockedCompareExchange(&m_threadid, 0x00, 0x00))
	{
		localTaken = true;
	}
	else
	{
		uint64_t startMilliseconds = GetSystemTickCount64();
		uint64_t count = 0;
		while (!localTaken)
		{
			if (iterations != NULL && *iterations != INFINITE)
			{
				if (++count >= *iterations)
				{
					break;
				}
			}
			if (timeval != NULL && *timeval != INFINITE)
			{
				uint64_t elapsedMilliseconds = GetSystemTickCount64() - startMilliseconds;
				if (elapsedMilliseconds >= *timeval)
				{
					break;
				}
			}
			if (InterlockedCompareExchange(&m_signal, 0x01, 0x00) == 0x00) // 获取到锁信号
			{
				localTaken = true;
				InterlockedExchange(&m_threadid, threadid);
			}
		}
	}
	if (localTaken)
	{
		InterlockedIncrement(&m_refcount);
	}
}

void SpinLock::Exit()
{
	uint32_t threadid = (uint32_t)GetCurrentThreadId();
	if (threadid != InterlockedCompareExchange(&m_threadid, 0x00, 0x00))
	{
		throw new InvalidOperationException("this");
	}
	if (InterlockedDecrement(&m_refcount) <= 0x00)
	{
		InterlockedExchange(&m_threadid, 0x00);
		InterlockedCompareExchange(&m_signal, 0x00, 0x01);
	}
}
