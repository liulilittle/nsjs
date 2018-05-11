#include "File.h"
#include "Memory.h"
#include "Environment.h"

#include <stdio.h>
#include <stdlib.h>
#include <Windows.h>

bool File::Exists(const char* path)
{
	FILE* file = NULL;
	fopen_s(&file, path, "rb");
	if (file == NULL)
	{
		return false;
	}
	fclose(file);
	return true;
}

const char* File::ReadAllBytes(const char* path, size_t& size)
{
	FILE* file = NULL;
	size = 0;
	fopen_s(&file, path, "rb");
	if (file == NULL)
	{
		return NULL;
	}
	fseek(file, 0, SEEK_END);
	size = ftell(file);
	rewind(file);
	if (ferror(file))
	{
		fclose(file);
		return NULL;
	}
	char* result = (char*)Memory::Alloc((uint32_t)(size + 1));
	result[size] = '\0';
	fread(result, size, 1, file);
	fclose(file);
	return result;
}

const char* File::ReadAllText(const char* path, size_t& size, int& characterset)
{
	characterset = 0;
	size = 0;
	char* s = (char*)File::ReadAllBytes(path, size);
	if (s == NULL)
	{
		return NULL;
	}
	characterset = GetTextFileBufferCharacterSet(s);
	if (characterset && size > 3)
	{
		s = &s[3];
		size -= 3;
	}
	return s;
}

size_t File::GetFileSize(const char* path)
{
	FILE* file = NULL;
	fopen_s(&file, path, "rb");
	if (file == NULL)
	{
		return 0;
	}
	fseek(file, 0, SEEK_END);
	size_t size = ftell(file);
	fclose(file);
	return size;
}
