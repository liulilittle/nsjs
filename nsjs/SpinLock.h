#ifndef SPINLOCK_H
#define SPINLOCK_H

#include <stdint.h>

class SpinLock
{
private:
	volatile uint32_t m_signal = 0x00; // ԭ���ź�
	uint32_t m_threadid = 0x00; // �����߳�
	uint32_t m_refcount = 0x00; // ���ü���

public:
	SpinLock();
	~SpinLock();
	void Enter(bool& localTaken);
	void Enter(bool& localTaken, uint32_t* iterations);
	void Enter(bool& localTaken, int32_t* timeval);
	void Enter(bool& localTaken, uint32_t* iterations, int32_t* timeval);
	void Exit();
};
#endif

