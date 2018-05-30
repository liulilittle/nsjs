#ifndef ENVIRONMENT_H
#define ENVIRONMENT_H

#include "NSJSVirtualMachine.h"

#include <stdint.h>
#include <v8.h>

#define GetBufferNativeString(out, callback) \
{ \
	out = NULL; \
	size_t size = MAX_PATH; \
	void* buffer = NULL; \
	do \
	{ \
		if (buffer != NULL) \
		{ \
			Memory::Free(buffer); \
		} \
		buffer = Memory::Alloc((uint32_t)size); \
		uint32_t len = (uint32_t)callback; \
		if (len <= 0) \
		{ \
			Memory::Free(buffer); \
			buffer = NULL; \
			break; \
		} \
		if (!(len >= (size - 1))) \
		{ \
			break; \
		} \
		size *= 2; \
	} while (true); \
	out = buffer; \
}

#define GetBufferLocalString(isolate, out, callback) \
{ \
	void* buffer = NULL; \
	GetBufferNativeString(buffer, callback); \
	const char* data = (buffer == NULL ? NULL : ASCIIToUtf8((char*)buffer)); \
	if (data == NULL || !v8::String::NewFromUtf8(isolate, data, \
		v8::NewStringType::kNormal).ToLocal(&out)) \
	{ \
		out = v8::Undefined(isolate); \
	} \
	Memory::Free(buffer); \
	Memory::Free(data); \
} \

char* UnicodeToUtf8(const wchar_t* s);
char* UnicodeToUtf8(const wchar_t* s, int& len);
wchar_t* ASCIIToUnicode(const char* s);
wchar_t* ASCIIToUnicode(const char* s, int& len);
char* UnicodeToASCII(const wchar_t* s);
char* UnicodeToASCII(const wchar_t* s, int& len);
wchar_t* Utf8ToUnicode(const char* s);
wchar_t* Utf8ToUnicode(const char* s, int& len);
char* Utf8ToASCII(const char* s);
char* Utf8ToASCII(const char* s, int& len);
char* ASCIIToUtf8(const char* s);
char* ASCIIToUtf8(const char* s, int& len);
uint64_t GetSystemTickCount64();
void FreeStringMemory(const void* s);
int GetTextFileBufferCharacterSet(const char* s);
uint32_t VMID(v8::Isolate* isolate);
int DumpWriteStackTrace(v8::Local<v8::StackTrace>* src, NSJSStackTrace* destination);
bool DumpWriteExceptionInfo(v8::TryCatch* try_catch, v8::Isolate* isolate, NSJSException* exception);

namespace Environment
{
	void Initialize(NSJSVirtualMachine& machine);
	void GetApplicationFileName(const v8::FunctionCallbackInfo<v8::Value>& info);
	void GetApplicationStartupPath(const v8::FunctionCallbackInfo<v8::Value>& info);
	void GetProcessorCount(const v8::FunctionCallbackInfo<v8::Value>& info);
	void GetApplicationCommandLine(const v8::FunctionCallbackInfo<v8::Value>& info);
	void CurrentDirectory(const v8::FunctionCallbackInfo<v8::Value>& info);
	void GetTickCount(const v8::FunctionCallbackInfo<v8::Value>& info);
	void GetVirtualMachineId(const v8::FunctionCallbackInfo<v8::Value>& info);
};
#endif
