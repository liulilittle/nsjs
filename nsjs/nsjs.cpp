// Copyright 2015 the V8 project authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <Windows.h>
#include <tchar.h>

#include "nsjs.h"
#include "NSJSVirtualMachine.h"
#include "Exception.h"
#include "Extension.h"
#include "Memory.h"
#include "File.h"

using v8::Value;
using v8::FunctionCallbackInfo;
using v8::String;

using v8::Local;
using v8::Object;
using v8::ReturnValue;

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

#define NSJSGetLocalValue(plocalvalueprx) \
	(plocalvalueprx->CrossThreading ? plocalvalueprx->PersistentValue.Get(plocalvalueprx->Isolate) : plocalvalueprx->LocalValue) \

#define NSJSArgumentGetArrayValueToBufferValue(value, len, cchtype, valuetype, procgetvalue) \
{ \
	if (localValue == NULL) \
	{ \
		throw new ArgumentNullException("Parameter localValue cannot be null"); \
	} \
	len = 0; \
	cchtype* ch = NULL; \
	LocalArrayToBufferValue(cchtype, ch, len, value, valuetype, procgetvalue); \
	return ch; \
}

#define NSJSObjectSetPropertyBaseValue(procnewdatetype) \
{ \
	if (isolate == NULL) \
	{ \
		throw new ArgumentNullException("Parameter isolate cannot be null"); \
	} \
	if (obj == NULL) \
	{ \
		throw new ArgumentNullException("Parameter obj cannot be null"); \
	} \
	if (key == NULL) \
	{ \
		throw new ArgumentNullException("Parameter key cannot be null"); \
	} \
	v8::Local<v8::Object> o = NSJSGetLocalValue(obj).As<v8::Object>(); \
	return o->Set(String::NewFromUtf8(isolate, key, v8::NewStringType::kNormal).ToLocalChecked(), procnewdatetype); \
}

#define NSJSObjectSetPropertyArrayValue(arraytype, elementtype) \
{ \
	if (isolate == NULL) \
	{ \
		throw new ArgumentNullException("Parameter isolate cannot be null"); \
	} \
	if (obj == NULL) \
	{ \
		throw new ArgumentNullException("Parameter obj cannot be null"); \
	} \
	if (key == NULL) \
	{ \
		throw new ArgumentNullException("Parameter key cannot be null"); \
	} \
	if (count > 0 && buffer == NULL) \
	{ \
		throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed"); \
	} \
	v8::Local<v8::Object> o = NSJSGetLocalValue(obj).As<v8::Object>(); \
	v8::Local<arraytype> s = arraytype::New(v8::ArrayBuffer::New(isolate, (size_t)count), 0, (size_t)count); \
	if (count > 0) \
	{ \
		for (uint32_t i = 0; i < count; i++) \
		{ \
			s->Set(i, elementtype::New(isolate, buffer[i])); \
		} \
	} \
	return o->Set(String::NewFromUtf8(isolate, key, v8::NewStringType::kNormal).ToLocalChecked(), s); \
} 

#define NSJSLocalValueNewArrayValue(arraytype, elementtype) \
{ \
	if (isolate == NULL) \
	{ \
		throw new ArgumentNullException("Parameter isolate cannot be null"); \
	} \
	if (count > 0 && buffer == NULL) \
	{ \
		throw new ArgumentOutOfRangeException("Parameter count is greater than 0 o'clock, buffer not null is not allowed"); \
	} \
	v8::Local<arraytype> s = arraytype::New(v8::ArrayBuffer::New(isolate, (size_t)count), 0, (size_t)count); \
	if (count > 0) \
	{ \
		for (uint32_t i = 0; i < count; i++) \
		{ \
			s->Set(i, elementtype::New(isolate, buffer[i])); \
		} \
	} \
	NSJSLocalValue* result; \
	NSJSNewLocalValue(result); \
	result->LocalValue = s; \
	return result; \
}

#define NSJSArrayGetLength(datatype) \
{ \
	if (value == NULL) \
	{ \
		throw new ArgumentNullException("Parameter value cannot be null"); \
	} \
	v8::Local<datatype> s = NSJSGetLocalValue(value).As<datatype>(); \
	return (int)s->Length(); \
}

#define NSJSESArraySetValue(s, index, value, arraytype, elementtype) \
{ \
	if (s == NULL) \
	{ \
		throw new ArgumentNullException("Parameter s cannot be null"); \
	} \
	if (value == NULL) \
	{ \
		throw new ArgumentNullException("Parameter value cannot be null"); \
	} \
	v8::Local<arraytype> arrays = NSJSGetLocalValue(s).As<arraytype>(); \
	v8::Isolate* isolate = arrays->GetIsolate(); \
	return arrays->Set(index, elementtype::New(isolate, value)); \
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_free(NSJSLocalValue* value)
{
	NSJSFreeLocalValue(value);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_returnvalue_get(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = info.GetReturnValue().Get();
	return result;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set(const v8::FunctionCallbackInfo<v8::Value>& info, NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	info.GetReturnValue().Set(NSJSGetLocalValue(localValue));
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_boolean(const v8::FunctionCallbackInfo<v8::Value>& info, bool value)
{
	info.GetReturnValue().Set(value);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_int32(const v8::FunctionCallbackInfo<v8::Value>& info, int32_t value)
{
	info.GetReturnValue().Set(value);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_uint32(const v8::FunctionCallbackInfo<v8::Value>& info, uint32_t value)
{
	info.GetReturnValue().Set(value);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_argument_returnvalue_set_float64(const v8::FunctionCallbackInfo<v8::Value>& info, double value)
{
	info.GetReturnValue().Set(value);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_argument_get_length(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	return info.Length();
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_this(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = info.This();
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_data(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = info.Data();
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_callee(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = info.Callee();
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_argument_get_solt(const v8::FunctionCallbackInfo<v8::Value>& info, int solt)
{
	if (solt < 0 || solt >= info.Length())
	{
		throw new ArgumentOutOfRangeException("The solt location of the access exceeds the specified range");
	}
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = info[solt];
	return localValue;
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_localvalue_get_string(NSJSLocalValue* localValue, int& len)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	v8::Local<v8::Value> value = NSJSGetLocalValue(localValue);
	return Utf8ToASCII(*String::Utf8Value(value), len);
}

DLLEXPORT const int32_t* DLLEXPORTNSAPI nsjs_localvalue_get_int32array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, int32_t, v8::Int32Array, Int32Value);
}

DLLEXPORT const uint32_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint32array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, uint32_t, v8::Uint32Array, Uint32Value);
}

DLLEXPORT const int16_t* DLLEXPORTNSAPI nsjs_localvalue_get_int16array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, int16_t, v8::Int16Array, Int32Value);
}

DLLEXPORT const uint16_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint16array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, uint16_t, v8::Uint16Array, Uint32Value);
}

DLLEXPORT const int8_t* DLLEXPORTNSAPI nsjs_localvalue_get_int8array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, int8_t, v8::Int8Array, Int32Value);
}

DLLEXPORT const uint8_t* DLLEXPORTNSAPI nsjs_localvalue_get_uint8array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, uint8_t, v8::Int16Array, Uint32Value);
}

DLLEXPORT const float_t* DLLEXPORTNSAPI nsjs_localvalue_get_float32array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, float_t, v8::Float32Array, NumberValue);
}

DLLEXPORT const double_t* DLLEXPORTNSAPI nsjs_localvalue_get_float64array(NSJSLocalValue* localValue, int& len)
{
	NSJSArgumentGetArrayValueToBufferValue(NSJSGetLocalValue(localValue), len, double_t, v8::Float64Array, NumberValue);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_array_new(v8::Isolate* isolate, int length)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Array::New(isolate, length);
	NSJSLocalValue* p;
	NSJSNewLocalValue(p);
	p->LocalValue = r;
	return p;
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int8array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Int8Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint8array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Uint8Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int32array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Int32Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint32array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Uint32Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_int16array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Int16Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_uint16array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Uint16Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_float32array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Float32Array);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_float64array_get_length(NSJSLocalValue* value)
{
	NSJSArrayGetLength(v8::Float64Array);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	v8::Local<v8::Array> arrays = NSJSGetLocalValue(s).As<v8::Array>();
	NSJSLocalValue* p;
	NSJSNewLocalValue(p);
	p->LocalValue = arrays->Get(index);
	return p;
}

DLLEXPORT int8_t DLLEXPORTNSAPI nsjs_localvalue_int8array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return (int8_t)NSJSGetLocalValue(s).As<v8::Int8Array>()->Get(index)->Int32Value();
}

DLLEXPORT uint8_t DLLEXPORTNSAPI nsjs_localvalue_uint8array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return (uint8_t)NSJSGetLocalValue(s).As<v8::Uint8Array>()->Get(index)->Uint32Value();
}

DLLEXPORT int16_t DLLEXPORTNSAPI nsjs_localvalue_int16array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return (int16_t)NSJSGetLocalValue(s).As<v8::Int16Array>()->Get(index)->Int32Value();
}

DLLEXPORT uint16_t DLLEXPORTNSAPI nsjs_localvalue_uint16array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return (uint16_t)NSJSGetLocalValue(s).As<v8::Uint16Array>()->Get(index)->Uint32Value();
}

DLLEXPORT int32_t DLLEXPORTNSAPI nsjs_localvalue_int32array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return NSJSGetLocalValue(s).As<v8::Int32Array>()->Get(index)->Int32Value();
}

DLLEXPORT uint32_t DLLEXPORTNSAPI nsjs_localvalue_uint32array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return NSJSGetLocalValue(s).As<v8::Uint32Array>()->Get(index)->Uint32Value();
}

DLLEXPORT float DLLEXPORTNSAPI nsjs_localvalue_float32array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return (float)NSJSGetLocalValue(s).As<v8::Float32Array>()->Get(index)->NumberValue();
}

DLLEXPORT double DLLEXPORTNSAPI nsjs_localvalue_float64array_indexget(NSJSLocalValue* s, uint32_t index)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	return NSJSGetLocalValue(s).As<v8::Float64Array>()->Get(index)->NumberValue();
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_array_indexset(NSJSLocalValue* s, uint32_t index, NSJSLocalValue* value)
{
	if (s == NULL)
	{
		throw new ArgumentNullException("Parameter s cannot be null");
	}
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	v8::Local<v8::Array> arrays = NSJSGetLocalValue(s).As<v8::Array>();
	return arrays->Set(index, NSJSGetLocalValue(value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int8array_indexset(NSJSLocalValue* s, uint32_t index, int8_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Int8Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint8array_indexset(NSJSLocalValue* s, uint32_t index, uint8_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Uint8Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int16array_indexset(NSJSLocalValue* s, uint32_t index, int16_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Int16Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint16array_indexset(NSJSLocalValue* s, uint32_t index, uint16_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Uint16Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_int32array_indexset(NSJSLocalValue* s, uint32_t index, int32_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Int32Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_uint32array_indexset(NSJSLocalValue* s, uint32_t index, uint32_t value)
{
	NSJSESArraySetValue(s, index, value, v8::Uint32Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_float32array_indexset(NSJSLocalValue * s, uint32_t index, float value)
{
	NSJSESArraySetValue(s, index, value, v8::Float32Array, v8::Number);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_float64array_indexset(NSJSLocalValue * s, uint32_t index, double value)
{
	NSJSESArraySetValue(s, index, value, v8::Float64Array, v8::Number);
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_localvalue_json_stringify(v8::Isolate* isolate, NSJSLocalValue* value, int& len)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	return Utf8ToASCII(*v8::String::Utf8Value(v8::JSON::Stringify(isolate->GetCurrentContext(), 
		NSJSGetLocalValue(value)).ToLocalChecked()), len);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_json_parse(v8::Isolate* isolate, const char* json)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (json == NULL)
	{
		throw new ArgumentNullException("Parameter json cannot be null");
	}
	v8::Local<v8::String> s = v8::String::NewFromUtf8(isolate, json, v8::NewStringType::kNormal).ToLocalChecked();
	v8::Local<v8::Value> r = v8::JSON::Parse(isolate, s).ToLocalChecked();
	NSJSLocalValue* p;
	NSJSNewLocalValue(p);
	p->LocalValue = r;
	return p;
}

DLLEXPORT char* DLLEXPORTNSAPI nsjs_localvalue_typeof(v8::Isolate* isolate, NSJSLocalValue* value)
{
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	return Utf8ToASCII(*v8::String::Utf8Value(NSJSGetLocalValue(value)->TypeOf(isolate)));
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_stacktrace_getcurrent(v8::Isolate* isolate, NSJSStackTrace* stacktrace)
{
	if (isolate == NULL || stacktrace == NULL)
	{
		return 0;
	}
	v8::Local<v8::StackTrace> src = v8::StackTrace::CurrentStackTrace(isolate, MAXSTACKFRAMECOUNT);
	return DumpWriteStackTrace(&src, stacktrace);
}

DLLEXPORT NSJSValueType DLLEXPORTNSAPI nsjs_localvalue_get_typeid(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSVirtualMachine::GetType(NSJSGetLocalValue(localValue));
}

DLLEXPORT int64_t DLLEXPORTNSAPI nsjs_localvalue_get_int64(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSGetLocalValue(localValue)->IntegerValue();
}

DLLEXPORT const int32_t DLLEXPORTNSAPI nsjs_localvalue_get_int32(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSGetLocalValue(localValue)->Int32Value();
}

DLLEXPORT const uint32_t DLLEXPORTNSAPI nsjs_localvalue_get_uint32(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSGetLocalValue(localValue)->Uint32Value();
}

DLLEXPORT const bool DLLEXPORTNSAPI nsjs_localvalue_get_boolean(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSGetLocalValue(localValue)->BooleanValue();
}

DLLEXPORT const double_t DLLEXPORTNSAPI nsjs_localvalue_get_float64(NSJSLocalValue* localValue)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	return NSJSGetLocalValue(localValue)->NumberValue();
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_count(NSJSLocalValue* obj)
{
	if (obj == NULL)
	{
		throw new ArgumentNullException("Parameter obj cannot be null");
	}
	v8::Local<v8::Object> o = NSJSGetLocalValue(obj).As<v8::Object>();
	return o->InternalFieldCount();
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_set(NSJSLocalValue* obj, int solt, NSJSLocalValue* value)
{
	if (obj == NULL)
	{
		throw new ArgumentNullException("Parameter obj cannot be null");
	}
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	v8::Local<v8::Object> o = NSJSGetLocalValue(obj).As<v8::Object>();
	if (solt < 0 || solt >= o->InternalFieldCount())
	{
		throw new ArgumentOutOfRangeException("The solt location of the access exceeds the specified range");
	}
	o->SetInternalField(solt, NSJSGetLocalValue(value));
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_internalfield_get(NSJSLocalValue* obj, int solt)
{
	if (obj == NULL)
	{
		throw new ArgumentNullException("Parameter obj cannot be null");
	}
	if (solt < 0 || solt >= nsjs_localvalue_object_internalfield_count(obj))
	{
		throw new ArgumentOutOfRangeException("The solt location of the access exceeds the specified range");
	}
	v8::Local<v8::Value> result = NSJSGetLocalValue(obj).As<v8::Object>()->GetInternalField(solt);
	NSJSLocalValue* rax;
	NSJSNewLocalValue(rax);
	rax->LocalValue = result;
	return rax;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_undefined(v8::Isolate* isolate)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = v8::Undefined(isolate);
	return localValue;
}

DLLEXPORT NSJSLocalValue *DLLEXPORTNSAPI nsjs_localvalue_null(v8::Isolate* isolate)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = v8::Null(isolate);
	return localValue;
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_equals(NSJSLocalValue* x, NSJSLocalValue* y)
{
	if (x == y)
	{
		return true;
	}
	if (x == NULL && y != NULL)
	{
		return false;
	}
	if (x != NULL && y == NULL)
	{
		return false;
	}
	return NSJSGetLocalValue(x)->Equals(NSJSGetLocalValue(y));
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_localvalue_hashcode(NSJSLocalValue* value)
{
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	v8::Local<v8::Value> o = NSJSGetLocalValue(value);
	return o.As<v8::Object>()->GetIdentityHash();
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_new(v8::Isolate* isolate, int fieldcount)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> result;
	if (fieldcount <= 0)
	{
		result = v8::Object::New(isolate);
	}
	else
	{
		v8::Local<v8::ObjectTemplate> object_template = v8::ObjectTemplate::New(isolate);
		object_template->SetInternalFieldCount(fieldcount);
		result = object_template->NewInstance();
	}
	NSJSLocalValue* rax;
	NSJSNewLocalValue(rax);
	rax->LocalValue = result;
	return rax;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int32_new(v8::Isolate* isolate, int32_t value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Int32::New(isolate, value);
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = r;
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint32_new(v8::Isolate* isolate, uint32_t value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Uint32::New(isolate, value);
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = r;
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float64_new(v8::Isolate* isolate, double_t value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Number::New(isolate, value);
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = r;
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_datetime_new(v8::Isolate* isolate, int64_t value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Date::New(isolate, (double)value);
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = r;
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_boolean_new(v8::Isolate* isolate, bool value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> r = v8::Boolean::New(isolate, value);
	NSJSLocalValue* localValue;
	NSJSNewLocalValue(localValue);
	localValue->LocalValue = r;
	return localValue;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_function_new(v8::Isolate* isolate, v8::FunctionCallback value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> rax = v8::Function::New(isolate, value);
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = rax;
	return result;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_string_new(v8::Isolate* isolate, const char* value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	v8::Local<v8::Value> rax = v8::String::NewFromUtf8(isolate, value, v8::NewStringType::kNormal).ToLocalChecked();
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = rax;
	return result;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int32array_new(v8::Isolate* isolate, const int32_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Int32Array, v8::Int32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint32array_new(v8::Isolate* isolate, const uint32_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Uint32Array, v8::Uint32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int16array_new(v8::Isolate* isolate, const int16_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Int16Array, v8::Int32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint16array_new(v8::Isolate* isolate, const uint16_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Uint16Array, v8::Uint32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_int8array_new(v8::Isolate* isolate, const int8_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Int8Array, v8::Int32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_uint8array_new(v8::Isolate* isolate, const uint8_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Uint8Array, v8::Uint32);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float32array_new(v8::Isolate* isolate, const float_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Float32Array, v8::Number);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_float64array_new(v8::Isolate* isolate, const double_t* buffer, uint32_t count)
{
	NSJSLocalValueNewArrayValue(v8::Float64Array, v8::Number);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, NSJSLocalValue* value)
{
	NSJSObjectSetPropertyBaseValue(NSJSGetLocalValue(value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_string(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, const char* value)
{
	NSJSObjectSetPropertyBaseValue(String::NewFromUtf8(isolate, value, v8::NewStringType::kNormal).ToLocalChecked());
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int32(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int32_t value)
{
	NSJSObjectSetPropertyBaseValue(v8::Int32::New(isolate, value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint32(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint32_t value)
{
	NSJSObjectSetPropertyBaseValue(v8::Uint32::New(isolate, value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_boolean(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, bool value)
{
	NSJSObjectSetPropertyBaseValue(v8::Boolean::New(isolate, value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float64(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, double_t value)
{
	NSJSObjectSetPropertyBaseValue(v8::Number::New(isolate, value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_datetime(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint64_t value)
{
	NSJSObjectSetPropertyBaseValue(v8::Date::New(isolate, (double)value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int8array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int8_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Int8Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint8array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint8_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Uint8Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int16array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int16_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Int16Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint16array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint16_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Uint16Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_int32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, int16_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Int32Array, v8::Int32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_uint32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, uint16_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Uint32Array, v8::Uint32);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float32array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, float_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Float32Array, v8::Number);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_float64array(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, double_t* buffer, uint32_t count)
{
	NSJSObjectSetPropertyArrayValue(v8::Float64Array, v8::Number);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_set_function(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key, v8::FunctionCallback value)
{
	NSJSObjectSetPropertyBaseValue(v8::Function::New(isolate, value));
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_localvalue_object_property_delete(v8::Isolate* isolate, NSJSLocalValue* obj, const char* key)
{
	if (obj == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	if (key == NULL)
	{
		throw new ArgumentNullException("Parameter key cannot be null");
	}
	v8::Local<v8::Object> value = NSJSGetLocalValue(obj).As<v8::Object>();
	return value->Delete(String::NewFromUtf8(isolate, key, v8::NewStringType::kNormal).ToLocalChecked());
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_property_call(v8::Isolate* isolate,
	NSJSLocalValue* recv,
	NSJSLocalValue* function, 
	int argc,
	NSJSLocalValue** argv, 
	NSJSException* exception)
{
	if (function == NULL)
	{
		throw new ArgumentNullException("Parameter function cannot be null");
	}
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (argc > 0 && argv == NULL)
	{
		throw new ArgumentOutOfRangeException("Parameter argc is greater than 0 o'clock, argv not null is not allowed");
	}
	v8::TryCatch try_catch(isolate);

	v8::Local<v8::Value>* args = (v8::Local<v8::Value>*)Memory::Alloc(sizeof(v8::Local<v8::Value>) * argc);
	for (int i = 0; i < argc; i++)
	{
		args[i] = NSJSGetLocalValue(argv[i]);
	}
	v8::Local<v8::Function> caller = NSJSGetLocalValue(function).As<v8::Function>();
	v8::Local<v8::Context> context = isolate->GetCurrentContext();
	v8::Local<v8::Value> out;
	v8::Local<v8::Value> self;
	if (recv == NULL)
	{
		self = context->Global();
	}
	else
	{
		self = NSJSGetLocalValue(recv);
	}
	bool freeMemory = true;
	if (args == NULL)
	{
		v8::Local<v8::Value> undefined = v8::Undefined(isolate);
		args = &undefined;
		freeMemory = false;
	}
	if (!caller->Call(context, self, argc, args).ToLocal(&out))
	{
		DumpWriteExceptionInfo(&try_catch, isolate, exception);
		return NULL;
	}
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = out;
	if (freeMemory)
	{
		Memory::Free(args);
	}
	return result;
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_localvalue_object_property_get(v8::Isolate* isolate, NSJSLocalValue* value, const char* key)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	if (key == NULL)
	{
		throw new ArgumentNullException("Parameter key cannot be null");
	}
	v8::Local<v8::Value> rcx = NSJSGetLocalValue(value);
	v8::Local<v8::String> rdx = String::NewFromUtf8(isolate, key, v8::NewStringType::kNormal).ToLocalChecked();
	rcx = rcx.As<v8::Object>()->Get(isolate->GetCurrentContext(), rdx).ToLocalChecked();
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = rcx;
	return result;
}

DLLEXPORT const char** DLLEXPORTNSAPI nsjs_localvalue_object_getallkeys(NSJSLocalValue* localValue, int& count)
{
	if (localValue == NULL)
	{
		throw new ArgumentNullException("Parameter localValue cannot be null");
	}
	v8::Local<v8::Object> value = NSJSGetLocalValue(localValue).As<v8::Object>();
	v8::Local<v8::Array> keys = value->GetPropertyNames();
	count = keys->Length();
	char** s = NULL;
	if (count > 0)
	{
		s = (char**)Memory::Alloc(sizeof(char*) * count);
		for (int i = 0; i < count; i++)
		{
			s[i] = Utf8ToASCII(*String::Utf8Value(keys->Get(i)));
		}
	}
	return (const char**)s;
}

DLLEXPORT v8::Isolate* DLLEXPORTNSAPI nsjs_argument_get_isolate(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	return info.GetIsolate();
}

DLLEXPORT v8::Isolate* DLLEXPORTNSAPI nsjs_virtualmachine_get_isolate(NSJSVirtualMachine* machine)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	return machine->GetIsolate();
}

DLLEXPORT BOOL DLLEXPORTNSAPI nsjs_localvalue_is_cross_threading(NSJSLocalValue* value)
{
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	return value->CrossThreading;
}

DLLEXPORT BOOL DLLEXPORTNSAPI nsjs_localvalue_set_cross_threading(v8::Isolate* isolate, NSJSLocalValue* value, BOOL disabled)
{
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (disabled)
	{
		value->CrossThreading = FALSE;
		if (value->PersistentValue.IsEmpty())
		{
			value->PersistentValue.ClearWeak();
			value->PersistentValue.Empty();
		}
	}
	else if (!value->CrossThreading)
	{
		value->Isolate = isolate;
		value->PersistentValue.Reset(isolate, NSJSGetLocalValue(value));
		value->CrossThreading = TRUE;
	}
	return TRUE;
}

#ifdef _DLLLIB_NO_CC

int main(int argc, char* argv[]) {
	nsjs_initialize(argv[0]);
	{
		NSJSVirtualMachine* machine = new NSJSVirtualMachine;
		Extension::Initialize(*machine);
		nsjs_virtualmachine_initialize(machine);
		{
			int characterset = 0;
			size_t size = 0;
			const char* src = File::ReadAllText(argv[1], size, characterset);
			NSJSException exception;
			const char* result = machine->Run(src, NULL, &exception);
			if (result != NULL)
			{
				printf("%s\n", result);
			}
		}
	}
	return getchar();
}
#else

BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		break;
	case DLL_PROCESS_DETACH:
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	}
	return (TRUE);
}
#endif

DLLEXPORT NSJSVirtualMachine* DLLEXPORTNSAPI nsjs_virtualmachine_new()
{
	return new NSJSVirtualMachine;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_free(NSJSVirtualMachine* machine)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	machine->Dispose();
	delete machine;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_abort(NSJSVirtualMachine* machine)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	v8::Isolate* isolate = machine->GetIsolate();
	isolate->CancelTerminateExecution();
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_initialize(NSJSVirtualMachine* machine)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	machine->Initialize();
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_add_c_extension(NSJSVirtualMachine* machine)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	Extension::Initialize(*machine);
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_run(NSJSVirtualMachine* machine, const char* source, const char* alias, NSJSException* exception)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (source == NULL)
	{
		throw new ArgumentNullException("Parameter source cannot be null");
	}
	return machine->Run(source, alias, exception);
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_eval(NSJSVirtualMachine* machine, const char* expression, NSJSException* exception)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (expression == NULL)
	{
		throw new ArgumentNullException("Parameter expression cannot be null");
	}
	return machine->Eval(expression, exception);
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_call2(NSJSVirtualMachine* machine, const char* function_name, int argc, const char** argv, NSJSException* exception)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	return machine->Call(function_name, argc, argv, exception);
}

DLLEXPORT const char* DLLEXPORTNSAPI nsjs_virtualmachine_call(NSJSVirtualMachine* machine, const char* function_name, int argc, NSJSLocalValue** argv, NSJSException* exception)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	if (argc > 0 && argv == NULL)
	{
		throw new ArgumentOutOfRangeException("Parameter argc is greater than 0 o'clock, argv not null is not allowed"); \
	}
	v8::Local<v8::Value>* args = (v8::Local<v8::Value>*)Memory::Alloc(sizeof(v8::Local<v8::Value>));
	for (int i = 0; i < argc; i++)
	{
		args[i] = NSJSGetLocalValue(argv[i]);
	}
	return machine->Call(function_name, argc, args, exception);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_virtualmachine_callvir(NSJSVirtualMachine* machine, const char* function_name, int argc, NSJSLocalValue** argv, NSJSException* exception)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	if (argc > 0 && argv == NULL)
	{
		throw new ArgumentOutOfRangeException("Parameter argc is greater than 0 o'clock, argv not null is not allowed"); \
	}
	v8::Local<v8::Value>* args = (v8::Local<v8::Value>*)Memory::Alloc(sizeof(v8::Local<v8::Value>));
	for (int i = 0; i < argc; i++)
	{
		args[i] = NSJSGetLocalValue(argv[i]);
	}
	v8::Local<v8::Value> r = machine->Callvir(function_name, argc, args, exception);
	NSJSLocalValue* lv;
	NSJSNewLocalValue(lv);
	lv->LocalValue = r;
	return lv;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_join(NSJSVirtualMachine* machine, NSJSJoinCallback callback, void* state)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	machine->Join(callback, state);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_set_data(NSJSVirtualMachine* machine, int solt, void* value)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (value == NULL)
	{
		throw new ArgumentNullException("Parameter value cannot be null");
	}
	nsjs_virtualmachine_set_data2(machine->GetIsolate(), solt, value);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_set_data2(v8::Isolate* isolate, int solt, void* value)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	isolate->SetData((uint32_t)solt, value);
}

DLLEXPORT const void* DLLEXPORTNSAPI nsjs_virtualmachine_get_data(NSJSVirtualMachine* machine, int solt)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	return nsjs_virtualmachine_get_data2(machine->GetIsolate(), solt);
}

DLLEXPORT const void* DLLEXPORTNSAPI nsjs_virtualmachine_get_data2(v8::Isolate* isolate, int solt)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	return isolate->GetData((uint32_t)solt);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_function_add(NSJSVirtualMachine* machine, const char* function_name, v8::FunctionCallback info)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	return machine->AddFunction(function_name, info);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_function_remove(NSJSVirtualMachine* machine, const char* function_name)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	return machine->RemoveFunction(function_name);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_add(NSJSVirtualMachine* machine, const char* object_name, NSJSVirtualMachine::ExtensionObjectTemplate* info)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (object_name == NULL)
	{
		throw new ArgumentNullException("Parameter object_name cannot be null");
	}
	if (info == NULL)
	{
		throw new ArgumentNullException("Parameter info cannot be null");
	}
	return machine->AddObject(object_name, info);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_remove(NSJSVirtualMachine* machine, const char* object_name)
{
	if (machine == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	if (object_name == NULL)
	{
		throw new ArgumentNullException("Parameter machine cannot be null");
	}
	return machine->RemoveObject(object_name);
}

DLLEXPORT NSJSVirtualMachine::ExtensionObjectTemplate* DLLEXPORTNSAPI nsjs_virtualmachine_object_new()
{
	NSJSVirtualMachine::ExtensionObjectTemplate* object_template = new NSJSVirtualMachine::ExtensionObjectTemplate;
	return object_template;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_virtualmachine_object_free(NSJSVirtualMachine::ExtensionObjectTemplate* object_template)
{
	if (object_template != NULL || !IsBadReadPtr(object_template, 0x01))
	{
		delete object_template;
	}
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_addfunction(NSJSVirtualMachine::ExtensionObjectTemplate* object_template, const char* function_name, v8::FunctionCallback callback)
{
	if (object_template == NULL)
	{
		throw new ArgumentNullException("Parameter object_template cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	if (callback == NULL)
	{
		throw new ArgumentNullException("Parameter callback cannot be null");
	}
	return object_template->AddFunction(function_name, callback);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_removefunction(NSJSVirtualMachine::ExtensionObjectTemplate* object_template, const char* function_name)
{
	if (object_template == NULL)
	{
		throw new ArgumentNullException("Parameter object_template cannot be null");
	}
	if (function_name == NULL)
	{
		throw new ArgumentNullException("Parameter function_name cannot be null");
	}
	return object_template->RemoveFunction(function_name);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_addobject(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* object_name, NSJSVirtualMachine::ExtensionObjectTemplate* object_template)
{
	if (object_template == NULL)
	{
		throw new ArgumentNullException("Parameter object_template cannot be null");
	}
	if (object_name == NULL)
	{
		throw new ArgumentNullException("Parameter object_name cannot be null");
	}
	if (owner == NULL)
	{
		throw new ArgumentNullException("Parameter owner cannot be null");
	}
	return owner->AddObject(object_name, object_template);
}

DLLEXPORT bool DLLEXPORTNSAPI nsjs_virtualmachine_object_removeobject(NSJSVirtualMachine::ExtensionObjectTemplate* owner, const char* object_name)
{
	if (object_name == NULL)
	{
		throw new ArgumentNullException("Parameter object_name cannot be null");
	}
	if (owner == NULL)
	{
		throw new ArgumentNullException("Parameter owner cannot be null");
	}
	return owner->RemoveObject(object_name);
}

DLLEXPORT NSJSLocalValue* DLLEXPORTNSAPI nsjs_virtualmachine_get_global(v8::Isolate* isolate)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	NSJSLocalValue* result;
	NSJSNewLocalValue(result);
	result->LocalValue = isolate->GetCurrentContext()->Global();
	return result;
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_uninitialize()
{
	NSJSVirtualMachine::Exit();
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_idlelocalvalues_setcapacity(int capacity)
{
	NSJSLocalValueAllocator& allocator = NSJSLocalValueAllocator::DefaultAllocator;
	allocator.SetIdleValueCapacity(capacity);
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_idlelocalvalues_getcapacity()
{
	NSJSLocalValueAllocator& allocator = NSJSLocalValueAllocator::DefaultAllocator;
	return allocator.GetIdleValueCapacity();
}

DLLEXPORT int DLLEXPORTNSAPI nsjs_activelocalvalues_getcount()
{
	NSJSLocalValueAllocator& allocator = NSJSLocalValueAllocator::DefaultAllocator;
	return allocator.GetActiveValueCount();
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_initialize(const char* exce_path)
{
	if (exce_path == NULL)
	{
		throw new ArgumentNullException("Parameter exce_path cannot be null");
	}
	NSJSVirtualMachine::Initialize(exce_path);
}

DLLEXPORT void* DLLEXPORTNSAPI nsjs_memory_alloc(const uint32_t size)
{
	if (size <= 0)
	{
		return NULL;
	}
	return Memory::Alloc(size);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_memory_free(const void* memory)
{
	return Memory::Free(memory);
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_exception_throw_value(v8::Isolate* isolate, NSJSLocalValue* exception)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (exception == NULL)
	{
		throw new ArgumentNullException("Parameter exception cannot be null");
	}
	isolate->ThrowException(NSJSGetLocalValue(exception));
}

DLLEXPORT void DLLEXPORTNSAPI nsjs_exception_throw_error(v8::Isolate* isolate, const char* message,
	NSJSErrorKind kind)
{
	if (isolate == NULL)
	{
		throw new ArgumentNullException("Parameter isolate cannot be null");
	}
	if (message == NULL)
	{
		throw new ArgumentNullException("Parameter message cannot be null");
	}
	v8::Local<v8::String> contents = v8::String::NewFromUtf8(isolate, message, v8::NewStringType::kNormal).ToLocalChecked();
	v8::Local<v8::Value> error;
	if (kind == NSJSErrorKind::kError)
	{
		error = v8::Exception::Error(contents);
	}
	else if (kind == NSJSErrorKind::kRangeError)
	{
		error = v8::Exception::RangeError(contents);
	}
	else if (kind == NSJSErrorKind::kReferenceError)
	{
		error = v8::Exception::ReferenceError(contents);
	}
	else if (kind == NSJSErrorKind::kSyntaxError)
	{
		error = v8::Exception::SyntaxError(contents);
	}
	else if (kind == NSJSErrorKind::kTypeError)
	{
		error = v8::Exception::TypeError(contents);
	}
	else
	{
		throw new ArgumentOutOfRangeException("kind value overflow optional range");
	}
	isolate->ThrowException(error);
}
