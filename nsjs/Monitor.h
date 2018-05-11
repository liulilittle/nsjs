#ifndef MONITOR_H
#define MONITOR_H

#include <stdio.h>
#include <stdint.h>

class Monitor
{
private:
	void* cs = NULL;

public:
	Monitor();
	virtual ~Monitor();
	virtual void Enter();
	virtual void Exit();
};
#endif
