#include "NSJSVirtualMachine.h"
#include "Exception.h"
#include "Memory.h"
#include "Environment.h"

#include <v8.h>
#include <v8-platform.h>
#include <libplatform/libplatform.h>
#include <limits.h>

#include <string>

#pragma comment(lib, "v8_base_0.lib")
#pragma comment(lib, "v8_base_1.lib")
#pragma comment(lib, "v8_libbase.lib")
#pragma comment(lib, "v8_external_snapshot.lib")
#pragma comment(lib, "v8_libplatform.lib")
#pragma comment(lib, "v8_libsampler.lib")
#pragma comment(lib, "icuuc.lib")
#pragma comment(lib, "icui18n.lib")
#pragma comment(lib, "inspector.lib")

#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "shlwapi.lib")

#ifdef NODEBUG
#pragma comment(lib, "msvcrtd.lib")
#endif

using v8::Platform;
using v8::ObjectTemplate;
using v8::Handle;
using std::hash_map;
using std::string;
using v8::FunctionCallback;
using v8::Local;
using v8::FunctionTemplate;
using v8::Isolate;
using v8::HandleScope;
using v8::Context;
using v8::String;
using v8::Script;
using v8::NewStringType;
using v8::Value;
using v8::Object;
using v8::Function;
using v8::V8;

NSJSLocalValueAllocator NSJSLocalValueAllocator::DefaultAllocator;
Platform* NSJSVirtualMachine::platform = NULL;

#define V8BEGIN_DOXFUNCTION() \
	v8::Locker				isolate_locker(this->isolate); \
	v8::Isolate::Scope		isolate_scope(this->isolate); \
	/* Create a stack-allocated handle scope. */ \
	v8::HandleScope			handle_scope(this->isolate); \
	/*Enter the context for compiling and running the hello world script. */ \
	v8::Local<v8::Context>  context = this->context.Get(this->isolate); \
	v8::Context::Scope		context_scope(context); \

Handle<ObjectTemplate> NSJSVirtualMachine::GetTemplate(v8::Isolate* isolate)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate can not be null");
	}
	return NSJSVirtualMachine::ExtensionObjectTemplate::New(isolate, &extensions);
}

NSJSVirtualMachine::NSJSVirtualMachine()
{
	this->array_buffer_allocator = NULL;
	this->isolate = NULL;
}

NSJSVirtualMachine::~NSJSVirtualMachine()
{
	this->Dispose();
}

const char* NSJSVirtualMachine::Run(const char* expression, const char* alias, NSJSException* exception)
{
	if (expression == NULL)
	{
		throw new ArgumentNullException("expression cannot be empty.");
	}
	V8BEGIN_DOXFUNCTION();
	// Create a string containing the JavaScript source code.
	Local<String> source = String::NewFromUtf8(isolate, expression, NewStringType::kNormal).ToLocalChecked();
	// Try to compile the source code or execute the error that occurred.
	v8::TryCatch try_catch(isolate);
	// Compile the source code.
	Local<Script> script;
	if (alias == NULL)
	{
		if (!Script::Compile(context, source).ToLocal(&script))
		{
			DumpWriteExceptionInfo(&try_catch, isolate, exception);
			return NULL;
		}
	}
	else
	{
		script = Script::Compile(source, v8::String::NewFromUtf8(isolate, alias, NewStringType::kNormal).ToLocalChecked());
		if (*script == NULL)
		{
			DumpWriteExceptionInfo(&try_catch, isolate, exception);
			return NULL;
		}
	}
	// Run the script to get the result.
	Local<Value> result;
	if (!script->Run(context).ToLocal(&result))
	{
		DumpWriteExceptionInfo(&try_catch, isolate, exception);
		return NULL;
	}
	// Convert the result to an UTF8 string and print it.
	return Utf8ToASCII(*String::Utf8Value(isolate, result));
}

const char* NSJSVirtualMachine::Call(const char* name, NSJSException* exception)
{
	return Call(name, 0, (const char**)NULL, exception);
}

const char* NSJSVirtualMachine::Call(const char* name, int argc, const char** argv, NSJSException* exception)
{
	if (name == NULL)
	{
		throw new ArgumentNullException("Specifies that the calling method name cannot be null");
	}
	if (argc < 0)
	{
		throw new ArgumentOutOfRangeException("Argc no exists less than 0");
	}
	v8::TryCatch try_catch(isolate);
	Local<Context> context = this->GetContext();
	Handle<Object> global = context->Global();
	Handle<Value> obj = global->Get(String::NewFromUtf8(isolate, name, NewStringType::kNormal).ToLocalChecked());
	Handle<Function> function = Handle<Function>::Cast(obj);

	Handle<Value>* args = argc <= 0 ? NULL : new Handle<Value>[argc];
	if (argc > 0)
	{
		memset(args, 0x00, sizeof(args));
		for (int i = 0; i < argc; i++)
		{
			args[i] = String::NewFromUtf8(isolate, argv[i], NewStringType::kNormal).ToLocalChecked();
		}
	}
	Local<Value> result;
	if (!function->Call(context, global, argc, args).ToLocal(&result))
	{
		DumpWriteExceptionInfo(&try_catch, isolate, exception);
		return NULL;
	}
	return Utf8ToASCII(*String::Utf8Value(isolate, result));
}

const char* NSJSVirtualMachine::Call(const char* name, int argc, v8::Local<v8::Value>* argv, NSJSException* exception)
{
	if (name == NULL)
	{
		throw new ArgumentNullException("Specifies that the calling method name cannot be null");
	}
	if (argc < 0)
	{
		throw new ArgumentOutOfRangeException("Argc no exists less than 0");
	}
	v8::TryCatch try_catch(isolate);

	Local<Context> context = this->GetContext();
	Handle<Object> global = context->Global();
	Handle<Value> obj = global->Get(String::NewFromUtf8(isolate, name, NewStringType::kNormal).ToLocalChecked());
	Handle<Function> function = Handle<Function>::Cast(obj);

	Local<Value> result;
	if (!function->Call(context, global, argc, argv).ToLocal(&result))
	{
		DumpWriteExceptionInfo(&try_catch, isolate, exception);
		return NULL;
	}
	return Utf8ToASCII(*String::Utf8Value(isolate, result));
}

v8::Local<v8::Value> NSJSVirtualMachine::Callvir(const char* name, int argc, v8::Local<v8::Value>* argv, NSJSException* exception)
{
	if (name == NULL)
	{
		throw new ArgumentNullException("Specifies that the calling method name cannot be null");
	}
	if (argc < 0)
	{
		throw new ArgumentOutOfRangeException("Argc no exists less than 0");
	}
	v8::TryCatch try_catch(isolate);

	Local<Context> context = this->GetContext();
	Handle<Object> global = context->Global();
	Handle<Value> obj = global->Get(String::NewFromUtf8(isolate, name, NewStringType::kNormal).ToLocalChecked());
	Handle<Function> function = Handle<Function>::Cast(obj);

	Local<Value> result;
	if (!function->Call(context, global, argc, argv).ToLocal(&result))
	{
		DumpWriteExceptionInfo(&try_catch, isolate, exception);
		result = v8::Undefined(isolate);
	}
	return result;
}

void NSJSVirtualMachine::Join(NSJSJoinCallback callback, void* state)
{
	if (callback == NULL)
	{
		throw new ArgumentNullException("callback is null");
	}
	V8BEGIN_DOXFUNCTION();
	callback(this, state);
}

const char* NSJSVirtualMachine::Eval(const char* expression, NSJSException* exception)
{
	if (expression == NULL)
	{
		throw new ArgumentNullException("Eval expression argument is not allowed to be null");
	}
	const char* argv[1] = { expression };
	return Call("eval", 1, argv, exception);
}

v8::Isolate* NSJSVirtualMachine::GetIsolate()
{
	return isolate;
}

v8::Local<v8::Context> NSJSVirtualMachine::GetContext()
{
	return context.Get(isolate);
}

void NSJSVirtualMachine::Initialize()
{
	if (this->isolate == NULL)
	{
		// Create a new Isolate and make it the current one.
		Isolate::CreateParams create_params;
		if (this->array_buffer_allocator == NULL)
		{
			this->array_buffer_allocator = new ArrayBufferAllocator;
		}
		create_params.array_buffer_allocator = this->array_buffer_allocator;
		this->isolate = Isolate::New(create_params);
		// Create a stack-allocated handle scope.
		HandleScope handle_scope(this->isolate);
		// Create a new context.
		context.Reset(this->isolate, Context::New(this->isolate, NULL, this->GetTemplate(this->isolate)));
	}
}

void NSJSVirtualMachine::Dispose()
{
	if (!context.IsEmpty())
	{
		context.ClearWeak();
		context.Reset();
	}
	context.Empty();
	if (isolate != NULL)
	{
		isolate->Dispose(); // Dispose the isolate and tear down V8.
		isolate = NULL;
	}
	if (array_buffer_allocator != NULL)
	{
		delete array_buffer_allocator;
		array_buffer_allocator = NULL;
	}
}

NSJSVirtualMachine::ExtensionObjectTemplate& NSJSVirtualMachine::GetExtension()
{
	return extensions;
}

void NSJSVirtualMachine::Initialize(const char* exec_path)
{
	// Initialize V8.
	V8::InitializeICUDefaultLocation(exec_path);
	V8::InitializeExternalStartupData(exec_path);
	NSJSVirtualMachine::platform = v8::platform::CreateDefaultPlatform();
	V8::InitializePlatform(NSJSVirtualMachine::platform);
	V8::InitializeICU();
	V8::Initialize();
}

void NSJSVirtualMachine::Exit()
{
	// Dispose the isolate and tear down V8.
	V8::Dispose();
	V8::ShutdownPlatform();
	delete NSJSVirtualMachine::platform;
}

NSJSDataType NSJSVirtualMachine::GetType(const v8::Local<v8::Value> value)
{
	NSJSDataType datatype = NSJSDataType::kUndefined;
	if (value->IsNull()) {
		datatype = NSJSDataType::kNull;
	}
	if (!value->IsUndefined()) {
		if (value->IsString()) {
			datatype = NSJSDataType::kString;
		}
		else if (value->IsInt32()) {
			datatype = NSJSDataType::kInt32;
		}
		else if (value->IsDate()) {
			datatype = NSJSDataType::kDateTime;
		}
		else if (value->IsNumber()) {
			datatype = NSJSDataType::kDouble;
		}
		else if (value->IsFunction()) {
			datatype = NSJSDataType::kFunction;
		}
		else if (value->IsUint32()) {
			datatype = NSJSDataType::kUInt32;
		}
		else if (value->IsBoolean()) {
			datatype = NSJSDataType::kBoolean;
		}
		else if (value->IsInt8Array()) {
			datatype = NSJSDataType::kInt8Array;
		}
		else if (value->IsUint8Array()) {
			datatype = NSJSDataType::kUInt8Array;
		}
		else if (value->IsInt32Array()) {
			datatype = NSJSDataType::kInt32Array;
		}
		else if (value->IsUint32Array()) {
			datatype = NSJSDataType::kUInt32Array;
		}
		else if (value->IsInt16Array()) {
			datatype = NSJSDataType::kInt16Array;
		}
		else if (value->IsUint16Array()) {
			datatype = NSJSDataType::kUInt16Array;
		}
		else if (value->IsFloat32Array()) {
			datatype = NSJSDataType::kFloat32Array;
		}
		else if (value->IsFloat64Array()) {
			datatype = NSJSDataType::kFloat64Array;
		}
		else if (value->IsArray()) {
			datatype = NSJSDataType::kArray;
		}
		if (value->IsInt32() || value->IsUint32()) {
			int64_t n = value->IntegerValue();
			if (n < INT_MIN || n > UINT_MAX) {
				datatype = NSJSDataType::kInt64;
			}
		}
		else if (value->IsObject()) {
			datatype = (NSJSDataType)(datatype | NSJSDataType::kObject);
		}
	}
	return datatype;
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetValue(const char* name, Value* value)
{
	if (name == NULL || value == NULL)
	{
		return false;
	}
#ifdef ENABLE_MONITOR_LOCK
	locker.Enter();
#else
	bool localTaken = false;
	locker.Enter(localTaken);
#endif
	{
		if (this->Contains(name))
		{
			Value* data = extensions[name];
			ReleaseValue(data);
			extensions[name] = value;
		}
		else
		{
			extensions.insert(std::hash_map<std::string, Value*>::value_type(name, value));
		}
	}
	locker.Exit();
	return true;
}

NSJSVirtualMachine::ExtensionObjectTemplate::Value* NSJSVirtualMachine::ExtensionObjectTemplate::NewValue(Value::Kind kind,
	int64_t data, v8::PropertyAttribute attr)
{
	Value* value = (Value*)Memory::Alloc(sizeof(Value));
	if (value != NULL)
	{
		value->kind = kind;
		value->data = data;
		value->attr = attr;
	}
	return value;
}

void NSJSVirtualMachine::ExtensionObjectTemplate::ReleaseValue(Value* value)
{
	Memory::Free(value);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::Contains(const char* name)
{
#ifdef ENABLE_MONITOR_LOCK
	locker.Enter();
#else
	bool localTaken = false;
	locker.Enter(localTaken);
#endif
	bool contains = false;
	{
		contains = extensions.find(name) != extensions.end();
	}
	locker.Exit();
	return contains;
}

#ifndef ExtensionSetValue
#define ExtensionSetValue(kind, value, attr) \
{ \
	Value* dataValue = NewValue(kind, value, attr); \
	if (SetValue(name, dataValue)) \
	{ \
		return true; \
	} \
	ReleaseValue(dataValue); \
	return false; \
} \

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetBoolean(const char* name, bool value, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kBoolean, value ? 0x01L : 0x00L, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetString(const char* name, const char* value, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kString, (int64_t)value, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetNull(const char* name, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kNull, NULL, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetUndefined(const char* name, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kUndefined, NULL, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetObject(const char* name, NSJSVirtualMachine::ExtensionObjectTemplate* value, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kObject, (int64_t)value, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetFunction(const char* name, v8::FunctionCallback value, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kFunction, (int64_t)value, attr);
}

bool NSJSVirtualMachine::ExtensionObjectTemplate::SetNumber(const char* name, double value, v8::PropertyAttribute attr)
{
	ExtensionSetValue(Value::kNumber, *(int64_t*)&value, attr);
}

#undef ExtensionSetValue
#endif

bool NSJSVirtualMachine::ExtensionObjectTemplate::RemoveValue(const char* name)
{
	if (name == NULL)
	{
		return false;
	}
	auto i = extensions.find(name);
	if (i != extensions.end())
	{
		extensions.erase(i);
		ReleaseValue(i->second);
	}
	return true;
}

std::hash_map<std::string, NSJSVirtualMachine::ExtensionObjectTemplate::Value*>& NSJSVirtualMachine::ExtensionObjectTemplate::GetAll()
{
	return extensions;
}

v8::Handle<v8::ObjectTemplate> NSJSVirtualMachine::ExtensionObjectTemplate::New(v8::Isolate* isolate, NSJSVirtualMachine::ExtensionObjectTemplate* extension)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate can not be null");
	}
	if (extension == NULL)
	{
		throw new ArgumentNullException("Parameter extension can not be null");
	}
	Handle<ObjectTemplate> owner;
	if (extension->constructor == NULL)
	{
		owner = ObjectTemplate::New(isolate);
	}
	else
	{
	 	v8::Local<v8::FunctionTemplate> constructor = v8::FunctionTemplate::New(isolate, extension->constructor);
		owner = ObjectTemplate::New(isolate, constructor);
	}
	std::hash_map<std::string, Value*>& externs = extension->GetAll();
	for (std::hash_map<std::string, Value*>::iterator i = externs.begin(); i != externs.end(); i++)
	{
		Value* data = i->second;
		if (data == NULL)
		{
			continue;
		}
		v8::Handle<v8::String> key = v8::String::NewFromUtf8(isolate, i->first.data());
		v8::Handle<v8::Value> value;
		if (data->kind == Value::kObject)
		{
			ExtensionObjectTemplate* object_template = (ExtensionObjectTemplate*)data->data;
			if (object_template != NULL)
			{
				owner->Set(key, New(isolate, object_template), data->attr);
				continue;
			}
			value = v8::Null(isolate);
		}
		else if (data->kind == Value::kFunction)
		{
			v8::FunctionCallback handler = (v8::FunctionCallback)data->data;
			if (handler != NULL)
			{
				owner->Set(key, v8::FunctionTemplate::New(isolate, handler), data->attr);
				continue;
			}
			value = v8::Null(isolate);
		}
		else if (data->kind == Value::kBoolean)
		{
			value = v8::Boolean::New(isolate, data->data != 0x00L);
		}
		else if (data->kind == Value::kNumber)
		{
			value = v8::Number::New(isolate, *(double*)&data->data);
		}
		else if (data->kind == data->kString)
		{
			const char* str = (const char*)data->data;
			if (str == NULL)
			{
				value = v8::Null(isolate);
			}
			else
			{
				value = v8::String::NewFromUtf8(isolate, str);
			}
		}
		else if (data->kind == data->kNull)
		{
			value = v8::Null(isolate);
		}
		else if (data->kind == data->kUndefined)
		{
			value = v8::Undefined(isolate);
		}
		owner->Set(key, value, data->attr);
	}
	return owner;
}

void* NSJSVirtualMachine::ArrayBufferAllocator::Allocate(size_t length)
{
	void* data = AllocateUninitialized(length);
	if (Memory::IsNedallocAllocation())
	{
		return data;
	}
	return data == NULL ? data : memset(data, 0, length);
}

void* NSJSVirtualMachine::ArrayBufferAllocator::AllocateUninitialized(size_t length)
{
	return Memory::Alloc((uint32_t)length);
}

void NSJSVirtualMachine::ArrayBufferAllocator::Free(void* data, size_t length)
{
	Memory::Free(data);
}

void NSJSLocalValueAllocator::Clear(NSJSLocalValue* value)
{
	if (value != NULL)
	{
		v8::Persistent<v8::Value>& persistent = value->PersistentValue;
		if (!persistent.IsEmpty())
		{
			persistent.ClearWeak();
			persistent.Reset();
		}
		persistent.Empty();
		value->Isolate = NULL;
		value->CrossThreading = false;
	}
}

NSJSLocalValue* NSJSLocalValueAllocator::Alloc(void)
{
	NSJSLocalValue* chunk = NULL;
#ifdef ENABLE_MONITOR_LOCK
	this->locker.Enter();
#else
	bool localTaken = false;
	this->locker.Enter(localTaken);
#endif
	{
		LinkedListNode<NSJSLocalValue*>* node = this->frees.First();
		if (node != NULL)
		{
			chunk = node->Value;
			this->frees.Remove(node);
		}
		else
		{
			chunk = new NSJSLocalValue;
			NSJSLocalValueAllocator::Clear(chunk);
			node = (LinkedListNode<NSJSLocalValue*>*)Memory::Alloc(sizeof(LinkedListNode<NSJSLocalValue*>));
			node->Value = chunk;
			chunk->LinkedListNode = node;
		}
		this->actives.AddLast(node);
	}
	this->locker.Exit();
	return chunk;
}

bool NSJSLocalValueAllocator::Free(NSJSLocalValue* value)
{
	if (value == NULL)
	{
		return false;
	}
	NSJSLocalValueAllocator::Clear(value);
	bool success = true;
#ifdef ENABLE_MONITOR_LOCK
	this->locker.Enter();
#else
	bool localTaken = false;
	this->locker.Enter(localTaken);
#endif
	{
		do
		{
			LinkedListNode<NSJSLocalValue*>* node = value->LinkedListNode;
			if (node == NULL) // this->actives.Find(value);
			{
				break;
			}
			if (node->LinkedList == &this->actives)
			{
				this->actives.Remove(node);
				this->frees.AddLast(node);
			}
		} while (false);
	}
	this->locker.Exit();
	return success;
}

int NSJSLocalValueAllocator::GetActiveValueCount(void)
{
	return this->actives.Count();
}

int NSJSLocalValueAllocator::GetIdleValueCapacity(void)
{
	return this->frees.Count();
}

void NSJSLocalValueAllocator::SetIdleValueCapacity(int capacity)
{
#ifdef ENABLE_MONITOR_LOCK
	this->locker.Enter();
#else
	bool localTaken = false;
	this->locker.Enter(localTaken);
#endif
	{
		int count = this->frees.Count();
		if (count < capacity)
		{
			LinkedListNode<NSJSLocalValue*>* node = NULL;
			int surplus = (capacity - count);
			for (int i = 0; i < surplus; i++)
			{
				node = (LinkedListNode<NSJSLocalValue*>*)Memory::Alloc(sizeof(LinkedListNode<NSJSLocalValue*>));
				NSJSLocalValue* chunk = new NSJSLocalValue;
				NSJSLocalValueAllocator::Clear(chunk);
				node->Value = chunk;
				chunk->LinkedListNode = node;
				this->frees.AddLast(node);
			}
		}
	}
	this->locker.Exit();
}