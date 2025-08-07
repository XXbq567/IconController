// main.cpp
#include "pch.h"
#include "resource.h"

extern bool RegisterTray(HWND hWnd);
extern void ShowSettingsDlg(HWND hParent);

int WINAPI wWinMain(HINSTANCE hInst, HINSTANCE, LPWSTR, int)
{
    InitCommonControls();
    CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);

    WNDCLASSW wc{0};
    wc.lpfnWndProc   = [](HWND h, UINT m, WPARAM w, LPARAM l)->LRESULT{
        if(m==WM_DESTROY){ PostQuitMessage(0); return 0; }
        return DefWindowProc(h,m,w,l);
    };
    wc.hInstance     = hInst;
    wc.lpszClassName = L"DeskIconCtlCls";
    RegisterClassW(&wc);

    HWND hWnd = CreateWindowW(wc.lpszClassName, L"", 0, 0,0,0,0,
                              HWND_MESSAGE, nullptr, hInst, nullptr);

    RegisterHotKey(hWnd, 1, MOD_CONTROL|MOD_SHIFT, 'D');
    RegisterTray(hWnd);

    MSG msg;
    while(GetMessageW(&msg,nullptr,0,0)){
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
    UnregisterHotKey(hWnd,1);
    CoUninitialize();
    return 0;
}
