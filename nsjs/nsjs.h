#ifndef NSJS_H
#define NSJS_H

#include <v8.h>
#include <stdint.h>

#include "NSJSVirtualMachine.h"
#include "Environment.h"

#ifdef _X86_
extern "C" { int _afxForceUSRDLL; }
#else
extern "C" { int __afxForceUSRDLL; }
#endif

#ifdef _EXEC_CHARACTER_SET_UTF8
#pragma execution_character_set("utf-8")  
#endif

#define DLLEXPORT extern "C" __declspec(dllexport) 
#define DLLEXPORTNSAPI __cdecl

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set(const v8::FunctionCallbackInfo<v8::Value>& info, NSJSLocalValue* value);
DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_boolean(const v8::FunctionCallbackInfo<v8::Value>& info, bool value);
DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_int32(const v8::FunctionCallbackInfo<v8::Value>& info, int32_t value);
DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_uint32(const v8::FunctionCallbackInfo<v8::Value>& info, uint32_t value);
DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_float64(const v8::FunctionCallbackInfo<v8::Value>& info, double value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_returnvalue_get(const v8::FunctionCallbackInfo<v8::Value>& info);
DLLEXPORT NSJSVirtualMachine* DLLEXPORTNSAPI nsjs_virtualmachine_new();
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_free(NSJSVirtualMachine* machine);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_abort(NSJSVirtualMachine* machine);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_initialize(NSJSVirtualMachine* machine);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_add_c_extension(NSJSVirtualMachine* machine);
DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_run(NSJSVirtualMachine* machine, const char* source, const char* alias, NSJSException* exception);
DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_eval(NSJSVirtualMachine* machine, const char* expression, NSJSException* exception);
DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_call(NSJSVirtualMachine* machine, const char* function_name, int argc, NSJSLocalValue** argv, NSJSException* exception);
DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_call2(NSJSVirtualMachine* machine, const char* function_name, int argc, const char** argv, NSJSException* exception);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_virtualmachine_callvir(NSJSVirtualMachine* machine, const char* function_name, int argc, NSJSLocalValue** argv, NSJSException* exception);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_join(NSJSVirtualMachine* machine, NSJSJoinCallback callback, void* state);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_set_data(NSJSVirtualMachine* machine, int solt, void* value);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_set_data2(v8::Isolate* isolate, int solt, void* value);
DLLEXPORT const void* DLLEXPORTNSAPI nsjs_virtualmachine_get_data(NSJSVirtualMachine* machine, int solt);
DLLEXPORT const void* DLLEXPORTNSAPI nsjs_virtualmachine_get_data2(v8::Isolate* isolate, int solt);
DLLEXPORT v8::Isolate* DLLEXPORTNSAPI nsjs_virtualmachine_get_isolate(NSJSVirtualMachine* machine);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_virtualmachine_get_global(v8::Isolate* isolate);

DLLEXPORT NSJSVirtualMachine::ExtensionObjectTemplate* DLLEXPORTNSAPI nsjs_virtualmachine_get_extension_object_template(NSJSVirtualMachine* machine);
DLLEXPORT NSJSVirtualMachine::ExtensionObjectTemplate* DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_new(v8::FunctionCallback constructor);
DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_free(NSJSVirtualMachine::ExtensionObjectTemplate* object_template);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_boolean(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, bool value, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_number(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, double value, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_string(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, const char* value, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_object(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, NSJSVirtualMachine::ExtensionObjectTemplate* value, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_function(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, v8::FunctionCallback value, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_null(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_set_undefined(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name, v8::PropertyAttribute attr);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_extension_object_template_del_value(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* name);

DLLEXPORT void DLLEXPORTNSAPI nsjs_uninitialize();
DLLEXPORT void DLLEXPORTNSAPI nsjs_idlelocalvalues_setcapacity(int capacity);
DLLEXPORT int DLLEXPORTNSAPI nsjs_idlelocalvalues_getcapacity();
DLLEXPORT int DLLEXPORTNSAPI nsjs_activelocalvalues_getcount();
DLLEXPORT void DLLEXPORTNSAPI nsjs_initialize(const char* exce_path);
DLLEXPORT void* DLLEXPORTNSAPI nsjs_memory_alloc(const uint32_t size);
DLLEXPORT void DLLEXPORTNSAPI nsjs_memory_free(const void* memory);
DLLEXPORT void DLLEXPORTNSAPI nsjs_exception_throw_value(v8::Isolate* isolate, NSJSLocalValue* exception);
DLLEXPORT void DLLEXPORTNSAPI nsjs_exception_throw_error(v8::Isolate* isolate, const char* message, NSJSErrorKind kind);

DLLEXPORT int DLLEXPORTNSAPI nsjs_argument_get_length(const v8::FunctionCallbackInfo<v8::Value>& info);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_this(const v8::FunctionCallbackInfo<v8::Value>& info);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_data(const v8::FunctionCallbackInfo<v8::Value>& info);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_callee(const v8::FunctionCallbackInfo<v8::Value>& info);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_solt(const v8::FunctionCallbackInfo<v8::Value>& info, int solt);
DLLEXPORT v8::Isolate* DLLEXPORTNSAPI nsjs_argument_get_isolate(const v8::FunctionCallbackInfo<v8::Value>& info);

DLLEXPORT BOOL DLLEXPORTNSAPI nsjs_localvalue_is_cross_threading(NSJSLocalValue* value);
DLLEXPORT BOOL DLLEXPORTNSAPI nsjs_localvalue_set_cross_threading(v8::Isolate* isolate, NSJSLocalValue* value, BOOL disabled);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_array_indexget(NSJSLocalValue* value, uint32_t index);
DLLEXPORT int8_t DLLEXPORTNSAPI nsjs_localvalue_int8array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT uint8_t DLLEXPORTNSAPI nsjs_localvalue_uint8array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT int16_t DLLEXPORTNSAPI nsjs_localvalue_int16array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT uint16_t DLLEXPORTNSAPI nsjs_localvalue_uint16array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT int32_t DLLEXPORTNSAPI nsjs_localvalue_int32array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT uint32_t DLLEXPORTNSAPI nsjs_localvalue_uint32array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT float DLLEXPORTNSAPI nsjs_localvalue_float32array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT double DLLEXPORTNSAPI nsjs_localvalue_float64array_indexget(NSJSLocalValue* s, uint32_t index);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_array_indexset(NSJSLocalValue* s, uint32_t index, NSJSLocalValue* value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int8array_indexset(NSJSLocalValue* s, uint32_t index, int8_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint8array_indexset(NSJSLocalValue* s, uint32_t index, uint8_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int16array_indexset(NSJSLocalValue* s, uint32_t index, int16_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint16array_indexset(NSJSLocalValue* s, uint32_t index, uint16_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int32array_indexset(NSJSLocalValue* s, uint32_t index, int32_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint32array_indexset(NSJSLocalValue* s, uint32_t index, uint32_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_float32array_indexset(NSJSLocalValue* s, uint32_t index, float value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_float64array_indexset(NSJSLocalValue* s, uint32_t index, double value);

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_array_new(v8::Isolate* isolate, int length);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int8array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint8array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int32array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint32array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int16array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint16array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_float32array_get_length(NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_float64array_get_length(NSJSLocalValue* value);

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_localvalue_json_stringify(v8::Isolate* isolate, NSJSLocalValue* value, int& len);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_json_parse(v8::Isolate* isolate, const char* json, int jsonlen);

DLLEXPORT char* DLLEXPORTNSAPI nsjs_localvalue_typeof(v8::Isolate* isolate, NSJSLocalValue* value);
DLLEXPORT int DLLEXPORTNSAPI nsjs_stacktrace_getcurrent(v8::Isolate* isolate, NSJSStackTrace* stacktrace);
DLLEXPORT NSJSDataType DLLEXPORTNSAPI nsjs_localvalue_get_typeid(NSJSLocalValue* value);
DLLEXPORT const char* DLLEXPORTNSAPI nsjs_localvalue_get_string(NSJSLocalValue* value, int& len);
DLLEXPORT const int8_t* DLLEXPORTNSAPI nsjs_localvalue_get_int8array(NSJSLocalValue* value, int& len);
DLLEXPORT const uint8_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint8array(NSJSLocalValue* value, int& len);
DLLEXPORT const int32_t* DLLEXPORTNSAPI nsjs_localvalue_get_int32array(NSJSLocalValue* value, int& len);
DLLEXPORT const uint32_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint32array(NSJSLocalValue* value, int& len);
DLLEXPORT const int16_t* DLLEXPORTNSAPI nsjs_localvalue_get_int16array(NSJSLocalValue* value, int& len);
DLLEXPORT const uint16_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint16array(NSJSLocalValue* value, int& len);
DLLEXPORT const float_t* DLLEXPORTNSAPI nsjs_localvalue_get_float32array(NSJSLocalValue* value, int& len);
DLLEXPORT const double_t* DLLEXPORTNSAPI nsjs_localvalue_get_float64array(NSJSLocalValue* value, int& len);
DLLEXPORT const int32_t DLLEXPORTNSAPI nsjs_localvalue_get_int32(NSJSLocalValue* value);
DLLEXPORT const uint32_t DLLEXPORTNSAPI nsjs_localvalue_get_uint32(NSJSLocalValue* value);
DLLEXPORT const bool DLLEXPORTNSAPI nsjs_localvalue_get_boolean(NSJSLocalValue* value);
DLLEXPORT const double_t DLLEXPORTNSAPI nsjs_localvalue_get_float64(NSJSLocalValue* value);

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_property_call(v8::Isolate* isolate,
	NSJSLocalValue* recv,
	NSJSLocalValue* function, int argc,
	NSJSLocalValue** argv, NSJSException* exception);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_property_get(v8::Isolate* isolate, NSJSLocalValue* value, const char* key);
DLLEXPORT const char** DLLEXPORTNSAPI nsjs_localvalue_object_getallkeys(NSJSLocalValue* value, int& count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_delete(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_get(NSJSLocalValue* obj, int solt);
DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_count(NSJSLocalValue* obj);
DLLEXPORT void DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_set(NSJSLocalValue* obj, int solt, NSJSLocalValue* value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_undefined(v8::Isolate* isolate);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_null(v8::Isolate* isolate);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_free(NSJSLocalValue* value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_equals(NSJSLocalValue* x, NSJSLocalValue* y);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_new(v8::Isolate* isolate, int fieldcount);
DLLEXPORT int64_t DLLEXPORTNSAPI nsjs_localvalue_get_int64(NSJSLocalValue* localValue);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int32_new(v8::Isolate* isolate, int32_t value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint32_new(v8::Isolate* isolate, uint32_t value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float64_new(v8::Isolate* isolate, double_t value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_datetime_new(v8::Isolate* isolate, int64_t value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_boolean_new(v8::Isolate* isolate, bool value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_function_new(v8::Isolate* isolate, v8::FunctionCallback value);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_string_new(v8::Isolate* isolate, const char* data, int datalen);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int32array_new(v8::Isolate* isolate, const int32_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint32array_new(v8::Isolate* isolate, const uint32_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int16array_new(v8::Isolate* isolate, const int16_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint16array_new(v8::Isolate* isolate, const uint16_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int8array_new(v8::Isolate* isolate, const int8_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint8array_new(v8::Isolate* isolate, const uint8_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float32array_new(v8::Isolate* isolate, const float_t* buffer, uint32_t count);
DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float64array_new(v8::Isolate* isolate, const double_t* buffer, uint32_t count);

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, NSJSLocalValue* value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_string(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, const char* value, int valuelen);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int32(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int32_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint32(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint32_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_boolean(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, bool value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float64(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, double_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_datetime(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint64_t value);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int8array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int8_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint8array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint8_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int16array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int16_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint16array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint16_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int16_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint16_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, float_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float64array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, double_t* buffer, uint32_t count);
DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_function(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, v8::FunctionCallback value);
#endif