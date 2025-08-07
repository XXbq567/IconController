#include "pch.h"

void ToggleDesktopIcons() {
    IShellWindows* psw = nullptr;
    CoCreateInstance(CLSID_ShellWindows, nullptr, CLSCTX_ALL, IID_IShellWindows, (void**)&psw);
    if (!psw) return;

    IDispatch* pdisp = nullptr;
    VARIANT v{ VT_I4, CSIDL_DESKTOP };
    if (SUCCEEDED(psw->FindWindowSW(&v, nullptr, SWC_DESKTOP,
                                     (long*)(void*)&pdisp, SWFO_NEEDDISPATCH))) {
        IServiceProvider* psp = nullptr;
        if (SUCCEEDED(pdisp->QueryInterface(IID_IServiceProvider, (void**)&psp))) {
            IShellBrowser* psb = nullptr;
            if (SUCCEEDED(psp->QueryService(SID_STopLevelBrowser, IID_IShellBrowser, (void**)&psb))) {
                IShellView* psv = nullptr;
                if (SUCCEEDED(psb->QueryActiveShellView(&psv))) {
                    IFolderView2* pfv = nullptr;
                    if (SUCCEEDED(psv->QueryInterface(IID_IFolderView2, (void**)&pfv))) {
                        DWORD flags = 0;
                        pfv->GetCurrentFolderFlags(&flags);
                        pfv->SetCurrentFolderFlags(FWF_NOICONS,
                                                   flags & FWF_NOICONS ? 0 : FWF_NOICONS);
                        pfv->Release();
                    }
                    psv->Release();
                }
                psb->Release();
            }
            psp->Release();
        }
        pdisp->Release();
    }
    psw->Release();
}
