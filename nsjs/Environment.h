#ifndef ENVIRONMENT_H
#define ENVIRONMENT_H

#include "NSJSVirtualMachine.h"

#include <stdint.h>
#include <v8.h>

#ifndef LocalArrayToBufferValueEx
#define LocalArrayToBufferValueEx(cchtype, assign, value, typetoken) \
{ \
	const char* __xd_cc_offset__ = *String::Utf8Value(value); \
	int __xd_cc_len__ = String::Utf8Value(value).length(); \
	const char* __xd_cc_end__ = &__xd_cc_offset__[__xd_cc_len__]; \
	const char* __xd_cc_last__ = __xd_cc_offset__; \
	int __xd_cc_count__ = 1; \
	while (__xd_cc_offset__ != __xd_cc_end__) \
	{ \
		if (*__xd_cc_offset__ == ',') \
		{ \
			__xd_cc_count__++; \
		} \
		__xd_cc_offset__++; \
	} \
	if (__xd_cc_count__ <= 0) \
	{ \
		assign = NULL; \
	} \
	else \
	{ \
		cchtype* __xd_cc_ch_buf_tt__ = (cchtype*)Memory::Alloc(sizeof(cchtype) * __xd_cc_count__); \
		__xd_cc_offset__ = __xd_cc_last__; \
		int __xd_cc_ch_ii_t__ = 0; \
		while (__xd_cc_offset__ != __xd_cc_end__) \
		{ \
			if (*__xd_cc_offset__ == ',') \
			{ \
				if(typetoken == 0) { \
					__xd_cc_ch_buf_tt__[__xd_cc_ch_ii_t__] = (cchtype)atoll(__xd_cc_last__); \
				} else { \
					__xd_cc_ch_buf_tt__[__xd_cc_ch_ii_t__] = (cchtype)atof(__xd_cc_last__); \
				}\
				__xd_cc_ch_ii_t__++; \
				__xd_cc_last__ = __xd_cc_offset__ + 1; \
			} \
			__xd_cc_offset__++; \
		} \
		assign = __xd_cc_ch_buf_tt__; \
	} \
}
#endif

#ifndef LocalArrayToBufferValue
#define LocalArrayToBufferValue(cchtype, assign, assignlen, value, valuetype, procgetvalue) \
{ \
	v8::Local<valuetype> __xd_cc_ssarr__ = value.As<valuetype>(); \
	int __xd_cc_len__ = (int)__xd_cc_ssarr__->Length(); \
	cchtype* __xd_cc_cch__ = (cchtype*)Memory::Alloc((size_t)(sizeof(cchtype) * __xd_cc_len__)); \
	for (int __xd_cc__jj_ii__ = 0; __xd_cc__jj_ii__ < __xd_cc_len__; __xd_cc__jj_ii__++) \
	{ \
		__xd_cc_cch__[__xd_cc__jj_ii__] = (cchtype)(__xd_cc_ssarr__->Get(__xd_cc__jj_ii__)->procgetvalue()); \
	} \
	assign = __xd_cc_cch__; \
	assignlen = __xd_cc_len__; \
}
#endif

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
