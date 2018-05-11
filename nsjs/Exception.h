#ifndef EXCEPTION_H
#define EXCEPTION_H 

#include <stdlib.h>

class Exception 
{
private:
	const char* except_message = NULL;

public:
	Exception(const char* message);
	virtual const char* Message();
	virtual const char* Name();
	virtual void PrintStackTrace();
};

class ArgumentOutOfRangeException : public Exception
{
public:
	ArgumentOutOfRangeException(const char* message) : Exception(message)
	{
		
	}
	const char* Name() override
	{
		return "ArgumentOutOfRangeException";
	}
};

class ArgumentNullException : public Exception
{
public:
	ArgumentNullException(const char* message) : Exception(message)
	{

	}
	const char* Name() override
	{
		return "ArgumentNullException";
	}
};

class ArgumentException : public Exception
{
public:
	ArgumentException(const char* message) : Exception(message)
	{

	}
	const char* Name() override
	{
		return "ArgumentException";
	}
};

class InvalidOperationException : public Exception
{
public:
	InvalidOperationException(const char* message) : Exception(message)
	{

	}
	const char* Name() override
	{
		return "InvalidOperationException";
	}
};
#endif