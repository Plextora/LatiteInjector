#pragma once
#ifndef LATITE_EXPORTS
#define LATITE_API __declspec(dllexport)
#else
#define LATITE_API __declspec(dllimport)
#endif

#include "pch.h"
#include <windows.h>

extern "C" {
LATITE_API bool DoInject(DWORD pid, char* location);
}
