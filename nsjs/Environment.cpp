#include "Environment.h"
#include "Memory.h"

#include <stdlib.h>
#include <stdint.h>
#include <Windows.h>
#include <string>
#include <v8.h>

using namespace std;
using namespace v8;

wchar_t* ASCIIToUnicode(const char* s)
{
	int len;
	return ASCIIToUnicode(s, len);
}

wchar_t* ASCIIToUnicode(const char* s, int& len)
{
	len = MultiByteToWideChar(GetACP(), 0, (char*)s, -1, 0, 0) * 2;
	if (len <= 0)
	{
		return NULL;
	}
	wchar_t* ch = (wchar_t*)Memory::Alloc(len + 1);
	MultiByteToWideChar(GetACP(), 0, (char*)s, -1, ch, len);
	return ch;
}

char* UnicodeToASCII(const wchar_t* s)
{
	int len;
	return UnicodeToASCII(s, len);
}

char* UnicodeToASCII(const wchar_t* s, int& len)
{
	len = WideCharToMultiByte(GetACP(), 0, (wchar_t*)s, -1, NULL, 0, 0, 0);
	if (len <= 0)
	{
		return NULL;
	}
	char* ch = (char*)Memory::Alloc(len + 1);
	ch[0] = '\0';
	WideCharToMultiByte(GetACP(), 0, (wchar_t*)s, -1, ch, len, 0, 0);
	return ch;
}

wchar_t* Utf8ToUnicode(const char* s)
{
	int len;
	return Utf8ToUnicode(s, len);
}

wchar_t* Utf8ToUnicode(const char* s, int& len)
{
	len = MultiByteToWideChar(CP_UTF8, NULL, s, -1, NULL, 0) * 2;
	if (len <= 0)
	{
		return NULL;
	}
	wchar_t* ch = (wchar_t*)Memory::Alloc(len + 1);
	ch[0] = L'\0';
	MultiByteToWideChar(CP_UTF8, NULL, s, -1, ch, len);
	return ch;
}

char* Utf8ToASCII(const char* s)
{
	int len;
	return Utf8ToASCII(s, len);
}

char* Utf8ToASCII(const char* s, int& len)
{
	wchar_t* x1 = Utf8ToUnicode(s, len);
	char* x2 = UnicodeToASCII(x1, len);
	Memory::Free(x1);
	return x2;
}

char* ASCIIToUtf8(const char* s)
{
	int len;
	return ASCIIToUtf8(s, len);
}

char* ASCIIToUtf8(const char* s, int& len)
{
	wchar_t* x1 = ASCIIToUnicode(s, len);
	char* x2 = UnicodeToUtf8(x1, len);
	Memory::Free(x1);
	return x2;
}

char* UnicodeToUtf8(const wchar_t* s)
{
	int len;
	return UnicodeToUtf8(s, len);
}

char* UnicodeToUtf8(const wchar_t* s, int& len)
{
	len = WideCharToMultiByte(CP_UTF8, 0, s, -1, NULL, 0, NULL, NULL);
	if (len <= 0)
	{
		return NULL;
	}
	char* ch = (char*)Memory::Alloc(len + 1);
	WideCharToMultiByte(CP_UTF8, 0, s, -1, ch, len, NULL, NULL);
	return ch;
}

uint64_t GetSystemTickCount64()
{
	LARGE_INTEGER TicksPerSecond = { 0 };
	LARGE_INTEGER Tick;
	if (!TicksPerSecond.QuadPart)
	{
		QueryPerformanceFrequency(&TicksPerSecond);
	}
	QueryPerformanceCounter(&Tick);
	uint64_t Seconds = Tick.QuadPart / TicksPerSecond.QuadPart;
	uint64_t LeftPart = Tick.QuadPart - (TicksPerSecond.QuadPart*Seconds);
	uint64_t MillSeconds = LeftPart * 1000 / TicksPerSecond.QuadPart;
	uint64_t Result = Seconds * 1000 + MillSeconds;
	return Result;
}

void FreeStringMemory(const void* s)
{
	Memory::Free(s);
}

int GetTextFileBufferCharacterSet(const char* s)
{
	if (s == NULL)
	{
		return CP_ACP;
	}
	const uint8_t* p = (uint8_t*)s;
	if (p[0] == 0xEF && p[1] == 0xBB && p[2] == 0xBF)
	{
		return CP_UTF8; // Utf8
	}
	else if (p[0] == 0xFE && p[1] == 0xFF && p[2] == 0x00)
	{
		return 1201; // BigEndianUnicode
	}
	else if(p[0] == 0xFF && p[1] == 0xFE && p[2] == 0x41)
	{
		return CP_WINUNICODE; // Unicode
	}
	return CP_ACP; // ASCII--GetACP()
}

uint32_t VMID(v8::Isolate* isolate)
{
	if (isolate == NULL)
	{
		return 0x00;
	}
	uint8_t* s = reinterpret_cast<uint8_t*>(&isolate);
	uint32_t seed = 131; // 31 131 1313 13131 131313 etc..
	uint32_t hash = 0;
	int32_t i = 0;
	while (i < sizeof(Isolate*))
	{
		hash = hash * seed + s[i++];
	}
	return (hash & 0x7FFFFFFF);
}

int DumpWriteStackTrace(v8::Local<v8::StackTrace>& src, NSJSStackTrace* destination)
{
	if (destination == NULL)
	{
		return 0;
	}
	if (*src == NULL)
	{
		destination->Count = 0;
		return 0;
	}
	destination->Count = src->GetFrameCount();
	int count = destination->Count;
	if (count > MAXSTACKFRAMECOUNT)
	{
		count = MAXSTACKFRAMECOUNT;
	}
	for (int i = 0; i < count; i++)
	{
		NSJSStackFrame* f = &destination->Frame[i];
		v8::Local<v8::StackFrame> frame = src->GetFrame((uint32_t)i);
		f->Column = frame->GetColumn();
		f->FunctionName = Utf8ToASCII(*v8::String::Utf8Value(frame->GetFunctionName()));
		f->LineNumber = frame->GetLineNumber();
		f->ScriptId = frame->GetScriptId();
		f->ScriptName = Utf8ToASCII(*v8::String::Utf8Value(frame->GetScriptName()));
		f->ScriptNameOrSourceURL = Utf8ToASCII(*v8::String::Utf8Value(frame->
			GetScriptNameOrSourceURL()));
		f->IsConstructor = frame->IsConstructor();
		f->IsEval = frame->IsEval();
		f->IsWasm = frame->IsWasm();
	}
	return count;
}

bool DumpWriteExceptionInfo(v8::TryCatch& try_catch, v8::Isolate* isolate, NSJSException* exception)
{
	if (isolate == NULL || exception == NULL)
	{
		return false;
	}
	v8::Local<v8::Message> message = try_catch.Message(); // v8::Exception::GetStackTrace(try_catch.Exception());
	DumpWriteStackTrace(message->GetStackTrace(), &exception->StackTrace);
	exception->NowIsWrong = true;
	exception->ExceptionMessage = Utf8ToASCII(*v8::String::Utf8Value(try_catch.Exception()));
	exception->ErrorLevel = message->ErrorLevel();
	exception->EndColumn = message->GetEndColumn();
	exception->EndPosition = message->GetEndPosition();
	exception->LineNumber = message->GetLineNumber();
	v8::ScriptOrigin origin = message->GetScriptOrigin();
	exception->ResourceColumnOffset = origin.ResourceColumnOffset()->IntegerValue();
	exception->ResourceLineOffset = origin.ResourceLineOffset()->IntegerValue();
	exception->ResourceName = Utf8ToASCII(*v8::String::Utf8Value(origin.ResourceName()));
	exception->SourceMapUrl = Utf8ToASCII(*v8::String::Utf8Value(origin.SourceMapUrl()));
	exception->ScriptId = origin.ScriptID()->IntegerValue();
	exception->ScriptResourceName = Utf8ToASCII(*v8::String::Utf8Value(message->GetScriptResourceName()));
	exception->SourceLine = Utf8ToASCII(*v8::String::Utf8Value(message->GetSourceLine()));
	exception->StartColumn = message->GetStartColumn();
	exception->StartPosition = message->GetStartPosition();
	exception->IsSharedCrossOrigin = message->IsSharedCrossOrigin();
	return true;
}

void Environment::Initialize(NSJSVirtualMachine & machine)
{
	NSJSVirtualMachine::ExtensionObjectTemplate* environment = new NSJSVirtualMachine::ExtensionObjectTemplate;
	if (!machine.AddObject("Environment", environment))
	{
		delete environment;
	}
	else
	{
		environment->AddFunction("GetVirtualMachineId", &Environment::GetVirtualMachineId);
		environment->AddFunction("GetApplicationStartupPath", &Environment::GetApplicationStartupPath);
		environment->AddFunction("GetProcessorCount", &Environment::GetProcessorCount);
		environment->AddFunction("GetTickCount", &Environment::GetTickCount);
		environment->AddFunction("GetApplicationFileName", &Environment::GetApplicationFileName);
		environment->AddFunction("GetApplicationCommandLine", &Environment::GetApplicationCommandLine);
	}
}

void Environment::GetApplicationFileName(const FunctionCallbackInfo<Value>& info)
{
	char ch[MAX_PATH];
	GetModuleFileNameA(NULL, ch, MAX_PATH);
	string path = ch;
	int position = (int)path.rfind('\\');
	Local<Value> result;
	if (position < 0)
	{
		result = v8::Undefined(info.GetIsolate());
	}
	else
	{
		result = String::NewFromUtf8(info.GetIsolate(), path.substr(position + 1).data(), NewStringType::kNormal).ToLocalChecked();
	}
	info.GetReturnValue().Set(result);
}

void Environment::GetApplicationStartupPath(const FunctionCallbackInfo<Value>& info)
{
	char ch[MAX_PATH];
	GetModuleFileNameA(NULL, ch, MAX_PATH);
	string path = ch;
	int position = (int)path.find_last_of('\\', path.length());
	Local<Value> result;
	if (position < 0)
	{
		result = v8::Undefined(info.GetIsolate());
	}
	else
	{
		result = String::NewFromUtf8(info.GetIsolate(), path.substr(0, position).data(), NewStringType::kNormal).ToLocalChecked();
	}
	info.GetReturnValue().Set(result);
}

void Environment::GetProcessorCount(const FunctionCallbackInfo<Value>& info)
{
	SYSTEM_INFO si = { 0 };
	GetSystemInfo(&si);
	info.GetReturnValue().Set((uint32_t)si.dwNumberOfProcessors);
}

void Environment::GetApplicationCommandLine(const FunctionCallbackInfo<Value>& info)
{
	info.GetReturnValue().Set(String::NewFromUtf8(info.GetIsolate(), GetCommandLineA(), NewStringType::kNormal).ToLocalChecked());
}

void Environment::GetTickCount(const FunctionCallbackInfo<Value>& info)
{
	info.GetReturnValue().Set(Integer::NewFromUnsigned(info.GetIsolate(), (uint32_t)GetSystemTickCount64()));
}

void Environment::GetVirtualMachineId(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	Isolate* isolate = info.GetIsolate();
	info.GetReturnValue().Set(VMID(isolate));
}
