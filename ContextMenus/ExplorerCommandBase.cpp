#include "pch.h"
#include "ExplorerCommandBase.h"

#include <fstream>
#include <string>
#include <sstream>
#include <nlohmann/json.hpp>

#pragma comment(lib,"Shell32.lib")

using namespace winrt;
using json = nlohmann::json;

constexpr char MENU_CONFIG_FILE[] = "D:\\Code\\Repos\\MenuManager\\MenuManagerNet\\bin\\x64\\Debug\\net6.0-windows10.0.22000.0\\menus_config.json";

ExplorerCommandBase::ExplorerCommandBase(std::wstring commandInterface)
{
	m_commandInterfaceId = commandInterface;

	std::ifstream menuConfigFile(MENU_CONFIG_FILE);
	json config = json::parse(menuConfigFile);
	for (size_t i = 0; i < config.size(); i++)
	{
		std::wstringstream comServer;
		comServer << "{" << winrt::to_hstring(config[i]["ComServer"]).c_str() << "}";
		if (m_commandInterfaceId == comServer.str())
		{
			m_title = winrt::to_hstring(config[i]["Title"]);
			m_target = winrt::to_hstring(config[i]["Target"]);

			break;
		}
	}
}

ExplorerCommandBase::~ExplorerCommandBase()
{
}

const EXPCMDFLAGS ExplorerCommandBase::Flags() { return ECF_DEFAULT; }
const EXPCMDSTATE ExplorerCommandBase::State(_In_opt_ IShellItemArray* selection) { return ECS_ENABLED; }

IFACEMETHODIMP ExplorerCommandBase::GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
{
	*name = nullptr;
	auto title = wil::make_cotaskmem_string_nothrow(m_title.c_str());
	RETURN_IF_NULL_ALLOC(title);
	*name = title.release();
	return S_OK;
}

IFACEMETHODIMP ExplorerCommandBase::GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon)
{
	*icon = nullptr;
	auto title = wil::make_cotaskmem_string_nothrow(m_target.c_str());
	RETURN_IF_NULL_ALLOC(title);
	*icon = title.release();
	return S_OK;
}

IFACEMETHODIMP ExplorerCommandBase::GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip) { *infoTip = nullptr; return E_NOTIMPL; }
IFACEMETHODIMP ExplorerCommandBase::GetCanonicalName(_Out_ GUID* guidCommandName) { *guidCommandName = GUID_NULL;  return S_OK; }
IFACEMETHODIMP ExplorerCommandBase::GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
{
	*cmdState = State(selection);
	return S_OK;
}
IFACEMETHODIMP ExplorerCommandBase::Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx* ctx) noexcept try
{
	HWND parent = nullptr;
	/*if (m_site)
	{
		winrt::com_ptr<IOleWindow> oleWindow;
		RETURN_IF_FAILED((HRESULT)m_site.as(winrt::guid_of<IOleWindow>(), oleWindow.put_void()));
		RETURN_IF_FAILED(oleWindow->GetWindow(&parent));
	}*/

	winrt::com_ptr<IShellItem> item;
	HRESULT hr = selection->GetItemAt(0, item.put());
	LPWSTR filePath;
	hr = item->GetDisplayName(SIGDN::SIGDN_FILESYSPATH, &filePath);
	ShellExecute(parent, L"open", m_target.c_str(), filePath, NULL, SW_SHOWNORMAL | SW_NORMAL);
	CoTaskMemFree(filePath);
	return S_OK;
}
CATCH_RETURN();

IFACEMETHODIMP ExplorerCommandBase::GetFlags(_Out_ EXPCMDFLAGS* flags) { *flags = Flags(); return S_OK; }
IFACEMETHODIMP ExplorerCommandBase::EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands) { *enumCommands = nullptr; return E_NOTIMPL; }

// IObjectWithSite
IFACEMETHODIMP ExplorerCommandBase::SetSite(_In_ IUnknown* site) noexcept
{
	m_site.copy_from(site);
	return S_OK;
}
IFACEMETHODIMP ExplorerCommandBase::GetSite(_In_ REFIID riid, _COM_Outptr_ void** site) noexcept
{
	return m_site.as(riid, site);
}