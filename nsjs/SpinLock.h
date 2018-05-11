#ifndef SPINLOCK_H
#define SPINLOCK_H

#include <stdint.h>

class SpinLock
{
private:
	volatile uint64_t __signal = 0x00; // 原子信号
	uint64_t __threadid = 0x00; // 加锁线程
	uint64_t __refcount = 0x00; // 引用计数

public:
	SpinLock();
	~SpinLock();
	void Enter(bool* localTaken);
	void Enter(bool* localTaken, uint64_t* iterations);
	void Enter(bool* localTaken, uint32_t* timeval);
	void Enter(bool* localTaken, uint64_t* iterations, uint32_t* timeval);
	void Exit();
};
#endif

