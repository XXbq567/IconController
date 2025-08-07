#include "pch.h"
#include "../res/resource.h"

extern bool RegisterTray(HWND);
extern void ShowSettingsDlg(HWND);

int WINAPI wWinMain(HINSTANCE h, HINSTANCE, PWSTR, int) {
    InitCommonControls();
    CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);

    WNDCLASSW wc{ 0 };
    wc.lpfnWndProc = DefWindowProcW;
    wc.hInstance = h;
    wc.lpszClassName = L"DeskIconCtlCls";
    RegisterClassW(&wc);

    HWND hwnd = CreateWindowW(wc.lpszClassName, L"", 0, 0, 0, 0, 0,
                              HWND_MESSAGE, nullptr, h, nullptr);

    RegisterHotKey(hwnd, 1, MOD_CONTROL | MOD_SHIFT, 'D');
    RegisterTray(hwnd);

    MSG msg;
    while (GetMessageW(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }
    UnregisterHotKey(hwnd, 1);
    CoUninitialize();
    return 0;
}
