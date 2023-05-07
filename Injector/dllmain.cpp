// this is literally just a port of injector to a dll
// the license doesnt apply to whoever else's code was used (as noted in comments)

#include "pch.h"
#include "api.h"

#include <Psapi.h>
#include <iostream>
#include <string>

#include <TlHelp32.h>
#include <AccCtrl.h>
#include <Aclapi.h>
#include <Sddl.h>

#include <locale>
#include <codecvt>
#include <string>

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

// stolen from https://github.com/Wunkolo/UWPDumper/blob/9fb0a040e674521c1413276bcea6e4e708f34d19/UWPInjector/source/main.cpp#L226 which was in https://github.com/fligger/FateInjector

void SetAccessControl(const std::wstring& ExecutableName, const wchar_t* AccessString)
{
	PSECURITY_DESCRIPTOR SecurityDescriptor = nullptr;
	EXPLICIT_ACCESSW ExplicitAccess = { 0 };

	ACL* AccessControlCurrent = nullptr;
	ACL* AccessControlNew = nullptr;

	SECURITY_INFORMATION SecurityInfo = DACL_SECURITY_INFORMATION;
	PSID SecurityIdentifier = nullptr;

	if (
		GetNamedSecurityInfoW(
			ExecutableName.c_str(),
			SE_FILE_OBJECT,
			DACL_SECURITY_INFORMATION,
			nullptr,
			nullptr,
			&AccessControlCurrent,
			nullptr,
			&SecurityDescriptor
		) == ERROR_SUCCESS
		)
	{
		ConvertStringSidToSidW(AccessString, &SecurityIdentifier);
		if (SecurityIdentifier != nullptr)
		{
			ExplicitAccess.grfAccessPermissions = GENERIC_READ | GENERIC_EXECUTE;
			ExplicitAccess.grfAccessMode = SET_ACCESS;
			ExplicitAccess.grfInheritance = SUB_CONTAINERS_AND_OBJECTS_INHERIT;
			ExplicitAccess.Trustee.TrusteeForm = TRUSTEE_IS_SID;
			ExplicitAccess.Trustee.TrusteeType = TRUSTEE_IS_WELL_KNOWN_GROUP;
			ExplicitAccess.Trustee.ptstrName = reinterpret_cast<wchar_t*>(SecurityIdentifier);

			if (
				SetEntriesInAclW(
					1,
					&ExplicitAccess,
					AccessControlCurrent,
					&AccessControlNew
				) == ERROR_SUCCESS
				)
			{
				SetNamedSecurityInfoW(
					const_cast<wchar_t*>(ExecutableName.c_str()),
					SE_FILE_OBJECT,
					SecurityInfo,
					nullptr,
					nullptr,
					AccessControlNew,
					nullptr
				);
			}
		}
	}
	if (SecurityDescriptor)
	{
		LocalFree(reinterpret_cast<HLOCAL>(SecurityDescriptor));
	}
	if (AccessControlNew)
	{
		LocalFree(reinterpret_cast<HLOCAL>(AccessControlNew));
	}
}

void Error(const char* msg) {
	MessageBoxA(NULL, msg, "Latite Injector", MB_ICONERROR | MB_OK);
}

bool DoInject(DWORD pid, const char* location) {
    // ...
	std::string myLoc = location;
	std::wstring ws(myLoc.size(), L' ');
	ws.resize(std::mbstowcs(&ws[0], myLoc.c_str(), myLoc.size()));
	SetAccessControl(L"Minecraft.Windows.exe", ws.c_str());

	auto hProc = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
		PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, pid);
	if (hProc == NULL) {
		return false;
	}

	auto loc = VirtualAllocEx(hProc, nullptr, strlen(location), 12288, 64);
	if (loc == 0) Error("Could not allocate!");
	if (!WriteProcessMemory(hProc, loc, location, strlen(location) + 2, nullptr))
	{
		Error("Could not write process memory!");
		return false;
	}
	auto hThread = CreateRemoteThread(hProc, nullptr, 0, (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandleA("Kernel32.dll"), "LoadLibraryW"), loc, 0, 0);

	Sleep(500);

	VirtualFreeEx(hProc, loc, 0, 0x8000 /*fully release*/);

	if (hThread == 0)
	{
		Error("Could not create remote thread!");
		return false;
	}
	CloseHandle(hThread);
	CloseHandle(hProc);
	return true;
}