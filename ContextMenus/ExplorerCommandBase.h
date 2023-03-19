#include <Unknwn.h>
#include <winrt/base.h>
#include <winrt/windows.foundation.h>

#include <wil\resource.h>

#include <shellapi.h>
#include <shobjidl_core.h>

class ExplorerCommandBase : public winrt::implements<ExplorerCommandBase, IExplorerCommand, IObjectWithSite>
{
public:
	ExplorerCommandBase(std::wstring guid);
	~ExplorerCommandBase();

	virtual const EXPCMDFLAGS Flags();
	virtual const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection);

	IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name);
	IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon);
	IFACEMETHODIMP GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip);
	IFACEMETHODIMP GetCanonicalName(_Out_ GUID* guidCommandName);
	IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState);
	IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx* ctx) noexcept;
	IFACEMETHODIMP GetFlags(_Out_ EXPCMDFLAGS* flags);
	IFACEMETHODIMP EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands);

	// IObjectWithSite
	IFACEMETHODIMP SetSite(_In_ IUnknown* site) noexcept;
	IFACEMETHODIMP GetSite(_In_ REFIID riid, _COM_Outptr_ void** site) noexcept;

protected:
	winrt::com_ptr<IUnknown> m_site;

private:
	winrt::hstring m_commandInterfaceId;
	winrt::hstring m_title;
	winrt::hstring m_icon;
	winrt::hstring m_target;
};