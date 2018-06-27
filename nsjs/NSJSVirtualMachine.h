#ifndef JSENGINECORE_H
#define JSENGINECORE_H

#include <v8.h>
#include <string>
#include <hash_map>
#include <hash_set>

#ifdef ENABLE_MONITOR_LOCK
#include "Monitor.h"
#else
#include "SpinLock.h"
#endif
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
} NSJSDataType;

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
#ifdef ENABLE_MONITOR_LOCK
	Monitor locker;
#else
	SpinLock locker;
#endif
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
	public:
		class Value {
		public:
			enum Kind {
				kUndefined = 0,
				kNull = 1,
				kObject = 2,
				kFunction = 3,
				kNumber = 4,
				kBoolean = 5,
				kString = 6,
			};
			v8::PropertyAttribute attr;
			Kind kind;
			int64_t data;
			Value() : kind(Kind::kUndefined), data(0), attr(v8::PropertyAttribute::None) { /*--default-init--*/ }
		};
		ExtensionObjectTemplate() : ExtensionObjectTemplate(NULL) { /*--default-init--*/ }
		ExtensionObjectTemplate(v8::FunctionCallback constructor) 
		{ 
			this->constructor = constructor;
		}

	private:
#ifdef ENABLE_MONITOR_LOCK
		Monitor locker;
#else
		SpinLock locker;
#endif
		v8::FunctionCallback constructor;
		std::hash_map<std::string, Value*> extensions;
		static void ReleaseValue(Value* value);
		static Value* NewValue(Value::Kind kind, int64_t data = 0x00L, v8::PropertyAttribute attr = v8::PropertyAttribute::None);

	public:
		virtual bool Contains(const char* name);
		virtual bool SetNull(const char* name, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetUndefined(const char* name, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetObject(const char* name, NSJSVirtualMachine::ExtensionObjectTemplate* value, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetFunction(const char* name, v8::FunctionCallback value, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetNumber(const char* name, double value, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetBoolean(const char* name, bool value, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetString(const char* name, const char* value, v8::PropertyAttribute attr = v8::PropertyAttribute::None);
		virtual bool SetValue(const char* name, Value* value);
		virtual bool RemoveValue(const char* name);
		virtual std::hash_map<std::string, Value*>& GetAll();
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
	NSJSVirtualMachine::ExtensionObjectTemplate extensions;
	v8::Global<v8::Context> context;
	v8::Handle<v8::ObjectTemplate> GetTemplate(v8::Isolate* isolate);

public:
	NSJSVirtualMachine();
	virtual ~NSJSVirtualMachine();
	virtual const char* Run(const char* expression, const char* alias, NSJSException* exception);
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
	virtual NSJSVirtualMachine::ExtensionObjectTemplate& GetExtension();
	static void Initialize(const char* exec_path);
	static void Exit();
	static NSJSDataType GetType(const v8::Local<v8::Value> value);
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

