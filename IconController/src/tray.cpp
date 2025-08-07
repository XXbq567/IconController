// tray.cpp
#include "pch.h"
#include "resource.h"

#define WM_TRAY (WM_USER+1)
NOTIFYICONDATAW nid{};

bool RegisterTray(HWND hWnd)
{
    nid.cbSize           = sizeof(nid);
    nid.hWnd             = hWnd;
    nid.uID              = 100;
    nid.uFlags           = NIF_ICON|NIF_MESSAGE|NIF_TIP;
    nid.uCallbackMessage = WM_TRAY;
    nid.hIcon            = LoadIcon(GetModuleHandle(nullptr),
                                    MAKEINTRESOURCE(IDI_ICON1));
    wcscpy_s(nid.szTip, L"桌面图标控制器");

    Shell_NotifyIconW(NIM_ADD,&nid);

    HMENU hMenu = CreatePopupMenu();
    AppendMenuW(hMenu, MF_STRING, 1000, L"设置");
    AppendMenuW(hMenu, MF_STRING, 1001, L"退出");

    SetWindowLongPtrW(hWnd, GWLP_WNDPROC, (LONG_PTR)
        [](HWND h, UINT m, WPARAM w, LPARAM l)->LRESULT{
            if(m==WM_TRAY && l==WM_RBUTTONUP){
                POINT pt; GetCursorPos(&pt);
                HMENU hMenu = CreatePopupMenu();
                AppendMenuW(hMenu, MF_STRING, 1000, L"设置");
                AppendMenuW(hMenu, MF_STRING, 1001, L"退出");
                SetForegroundWindow(h);
                TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, pt.x,pt.y,0,h,nullptr);
                DestroyMenu(hMenu);
            }
            if(m==WM_COMMAND){
                if(LOWORD(w)==1000) ShowSettingsDlg(h);
                if(LOWORD(w)==1001){ Shell_NotifyIconW(NIM_DELETE,&nid); PostQuitMessage(0); }
            }
            if(m==WM_HOTKEY && w==1){
                extern void ToggleDesktopIcons();
                ToggleDesktopIcons();
            }
            return DefWindowProc(h,m,w,l);
        });
    return true;
}