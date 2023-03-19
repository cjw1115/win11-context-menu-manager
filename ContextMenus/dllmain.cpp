#include "pch.h"
#include <wrl/module.h>
#include <wrl/implements.h>
#include <wrl/client.h>
#include <shobjidl_core.h>
#include <wil\resource.h>
#include <string>
#include <vector>
#include <sstream>
#include <fstream>
#include <winrt/base.h>

#include "ExplorerCommandBase.h"

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

STDAPI DllGetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IActivationFactory** factory)
{
    return S_OK;
    //return Module<ModuleType::InProc>::GetModule().GetActivationFactory(activatableClassId, factory);
}

STDAPI DllCanUnloadNow()
{
    return S_OK;
    //return Module<InProc>::GetModule().GetObjectCount() == 0 ? S_OK : S_FALSE;
}

struct callback_factory : winrt::implements<callback_factory, IClassFactory>
{
    callback_factory(REFGUID guid)
    {
        m_classGuid = winrt::to_hstring(guid);
    }

    HRESULT __stdcall CreateInstance(
        IUnknown* outer,
        GUID const& iid,
        void** result) noexcept final
    {
        *result = nullptr;

        if (outer)
        {
            return CLASS_E_NOAGGREGATION;
        }

        
        auto it = winrt::make_self<ExplorerCommandBase>(m_classGuid.c_str());
        return it->QueryInterface(iid, result);
    }

    HRESULT __stdcall LockServer(BOOL) noexcept final
    {
        return S_OK;
    }

private:
    winrt::hstring m_classGuid;
};

STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _COM_Outptr_ void** instance)
{
    /*int timer = 5000;
    while (!IsDebuggerPresent() && timer > 0)
    {
        Sleep(10);
        timer -= 10;
    }*/

    *instance = (void*)winrt::make_self<callback_factory>(rclsid).detach();
    return S_OK;
}