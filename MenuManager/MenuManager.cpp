// MenuManager.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

#include <Windows.h>
#include <appmodel.h>

#include <string>
#include <winrt/base.h>
#include <winrt/windows.foundation.h>
#include <winrt/windows.management.deployment.h>

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Management::Deployment;

using namespace std;

bool IsRunningWithIdentity()
{
    UINT32 length = 0;
    LONG rc = GetCurrentPackageFullName(&length, NULL);
    if (rc == APPMODEL_ERROR_NO_PACKAGE)
    {
        wprintf(L"Process has no package identity\n");
        return false;
    }
    return true;
}

bool registerSparsePackage(wstring externalLocation, wstring sparsePkgPath)
{
    bool registration = false;

    std::wcout << L"exe Location " << externalLocation << std::endl;
    std::wcout << L"msix Address " << sparsePkgPath << std::endl;

    Uri externalLocationUri(externalLocation);
    Uri packageUri(sparsePkgPath);

    PackageManager packageManager;

    //Declare use of an external location
    AddPackageOptions options;
    options.ExternalLocationUri(externalLocationUri);;

    auto result = packageManager.AddPackageByUriAsync(packageUri, options).get();

    if (result.IsRegistered())
    {
        registration = true;
        std::wcout << L"Package Registration succeeded!" << std::endl;
    }
    else
    {
        std::wcout << L"Installation Error: " <<std::hex << result.ExtendedErrorCode() << std::endl;
        std::wcout << L"Detailed Error Text: " << result.ErrorText().c_str() << std::endl;
    }

    return registration;
}

int main()
{
    std::wstring externalLocation = L"D:\\Code\\Repos\\MenuManager\\x64\\Debug";
    std::wstring sparsePackageLocation = L"D:\\Code\\Repos\\MenuManager\\x64\\Debug\\ContextMenuySparseAppx.msix";
    if (!IsRunningWithIdentity())
    {
        registerSparsePackage(externalLocation, sparsePackageLocation);
    }

    int value = 0;
    std::cin >> value;
    return value;
}