#ifndef FILE_H
#define FILE_H

class File
{
public:
	static bool Exists(const char* path);
	static const char* ReadAllBytes(const char* path, size_t& size);
	static const char* ReadAllText(const char* path, size_t& size, int& characterset);
	static size_t GetFileSize(const char* path);
};
#endif