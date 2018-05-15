#ifndef JSENGINECORE_H
#define JSENGINECORE_H

#include <v8.h>
#include <string>
#include <hash_map>
#include <hash_set>

#include "Monitor.h"
#include "LinkedList.h"

#define MAXSTACKFRAMECOUNT 100

typedef enum {
	kUndefined = 0x000000,
	kString = 0x000001,
	kInt32 = 0x000002,
	kDouble = 0x000004,
	kBoolean = 0x000008,
	kNull = 0x000010,
	kDateTime = 0x000020,
	kFunction = 0x000040,
	kObject = 0x000080,
	kArray = 0x000100,
	kUInt32 = 0x000200,
	kInt32Array = 0x000400,
	kUInt32Array = 0x000800,
	kInt8Array = 0x001000,
	kUInt8Array = 0x002000,
	kInt16Array = 0x004000,
	kUInt16Array = 0x008000,
	kFloat32Array = 0x010000,
	kFloat64Array = 0x020000,
	kInt64 = 0x040000,
} NSJSValueType;

typedef enum
{
	kError = 0,
	kRangeError = 1,
	kReferenceError = 2,
	kSyntaxError = 3,
	kTypeError = 4,
} NSJSErrorKind;

typedef struct {
public:
	int Column;
	const char* FunctionName;
	int LineNumber;
	int ScriptId;
	const char* ScriptName;
	const char* ScriptNameOrSourceURL;
	bool IsConstructor;
	bool IsEval;
	bool IsWasm;
} NSJSStackFrame;

typedef struct {
public:
	int Count;
	NSJSStackFrame* Frame;
} NSJSStackTrace;

typedef struct {
public:
	bool NowIsWrong;
	int ErrorLevel;
	int EndColumn;
	int EndPosition;
	int LineNumber;
	int64_t ResourceColumnOffset;
	int64_t ResourceLineOffset;
	const char* ResourceName;
	const char* SourceMapUrl;
	int64_t ScriptId;
	const char* ScriptResourceName;
	const char* SourceLine;
	int StartColumn;
	int StartPosition;
	bool IsSharedCrossOrigin;
	const char* ExceptionMessage;
	const char* StackTrace;
} NSJSException;

typedef struct NSJSLocalValue {
public:
	v8::Local<v8::Value> LocalValue;
	v8::Persistent<v8::Value> PersistentValue;
	v8::Isolate* Isolate = NULL;
	bool CrossThreading = false;
	LinkedListNode<NSJSLocalValue*>* LinkedListNode = NULL;
} NSJSLocalValue;

class NSJSLocalValueAllocator
{
private:
	Monitor locker;
	LinkedList<NSJSLocalValue*> actives;
	LinkedList<NSJSLocalValue*> frees;
	void Clear(NSJSLocalValue* value);

public:
	static NSJSLocalValueAllocator DefaultAllocator;
	NSJSLocalValue* Alloc(void);
	bool Free(NSJSLocalValue* value);
	int GetActiveValueCount(void);
	int GetIdleValueCapacity(void);
	void SetIdleValueCapacity(int capacity);
};

typedef void(*NSJSJoinCallback)(void* sender, void* state);

class NSJSVirtualMachine
{
public:
	class ExtensionObjectTemplate
	{
	private:
		std::hash_map<std::string, v8::FunctionCallback> extension_functions;
		std::hash_map<std::string, NSJSVirtualMachine::ExtensionObjectTemplate*> extension_objects;

	public:
		virtual bool AddFunction(const char* name, v8::FunctionCallback function);
		virtual bool RemoveFunction(const char* name);
		virtual bool AddObject(const char* name, NSJSVirtualMachine::ExtensionObjectTemplate* extension);
		virtual bool RemoveObject(const char* name);
		virtual std::hash_map<std::string, v8::FunctionCallback> GetFunctionCollection();
		virtual std::hash_map<std::string, NSJSVirtualMachine::ExtensionObjectTemplate*> GetObjectCollection();
		static v8::Handle<v8::ObjectTemplate> New(v8::Isolate* isolate, NSJSVirtualMachine::ExtensionObjectTemplate* object_template);
	};

	class ArrayBufferAllocator : public v8::ArrayBuffer::Allocator 
	{
	public:
		virtual void* Allocate(size_t length);
		virtual void* AllocateUninitialized(size_t length);
		virtual void Free(void* data, size_t length);
	};

private:
	static v8::Platform* platform; 
	v8::Isolate* isolate = NULL;
	v8::ArrayBuffer::Allocator* array_buffer_allocator = NULL;
	NSJSVirtualMachine::ExtensionObjectTemplate extension_object_template;
	v8::Global<v8::Context> context;
	v8::Handle<v8::ObjectTemplate> GetTemplate(v8::Isolate* isolate);

public:
	NSJSVirtualMachine();
	virtual ~NSJSVirtualMachine();
	virtual const char* Run(const char* expression, const char* alias, NSJSException* exception);
	virtual bool AddFunction(const char* name, v8::FunctionCallback function);
	virtual bool RemoveFunction(const char* name);
	virtual bool AddObject(const char* name, NSJSVirtualMachine::ExtensionObjectTemplate* extension);
	virtual bool RemoveObject(const char* name);
	virtual const char* Call(const char* name, NSJSException* exception);
	virtual const char* Call(const char* name, int argc, const char** argv, NSJSException* exception);
	virtual const char* Call(const char* name, int argc, v8::Local<v8::Value>* argv, NSJSException* exception);
	virtual v8::Local<v8::Value> Callvir(const char* name, int argc, v8::Local<v8::Value>* argv, NSJSException* exception);
	virtual const char* Eval(const char* expression, NSJSException* exception);
	virtual void Join(NSJSJoinCallback callback, void* state);
	virtual v8::Isolate* GetIsolate();
	virtual v8::Local<v8::Context> GetContext();
	virtual void Initialize();
	virtual void Dispose();
	static void Initialize(const char* exec_path);
	static void Exit();
	static NSJSValueType GetType(const v8::Local<v8::Value> value);
};
#endif

#define NSJSFreeLocalValue(value) \
{ \
	return NSJSLocalValueAllocator::DefaultAllocator.Free(value); \
}

#define NSJSNewLocalValue(value) \
{ \
	value = NSJSLocalValueAllocator::DefaultAllocator.Alloc(); \
}

