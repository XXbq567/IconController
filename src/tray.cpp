#include "pch.h"
#include "../res/resource.h"

#define WM_TRAY (WM_USER + 1)

extern void ToggleDesktopIcons();
extern void ShowSettingsDlg(HWND);

// 真正的窗口过程（普通函数，可捕获 nid）
LRESULT CALLBACK TrayWndProc(HWND h, UINT m, WPARAM w, LPARAM l) {
    static NOTIFYICONDATAW nid{ sizeof(nid) };

    switch (m) {
    case WM_CREATE: {
        nid.hWnd = h;
        nid.uID = 100;
        nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
        nid.uCallbackMessage = WM_TRAY;
        nid.hIcon = LoadIcon(GetModuleHandle(nullptr), MAKEINTRESOURCE(IDI_ICON1));
        wcscpy_s(nid.szTip, L"桌面图标控制器");
        Shell_NotifyIconW(NIM_ADD, &nid);
        return 0;
    }
    case WM_TRAY:
        if (l == WM_RBUTTONUP) {
            POINT pt; GetCursorPos(&pt);
            HMENU menu = CreatePopupMenu();
            AppendMenuW(menu, MF_STRING, 1000, L"设置");
            AppendMenuW(menu, MF_STRING, 1001, L"退出");
            SetForegroundWindow(h);
            TrackPopupMenu(menu, TPM_RIGHTBUTTON, pt.x, pt.y, 0, h, nullptr);
            DestroyMenu(menu);
        }
        return 0;
    case WM_COMMAND:
        if (LOWORD(w) == 1000) ShowSettingsDlg(h);
        if (LOWORD(w) == 1001) {
            Shell_NotifyIconW(NIM_DELETE, &nid);
            PostQuitMessage(0);
        }
        return 0;
    case WM_HOTKEY:
        if (w == 1) ToggleDesktopIcons();
        return 0;
    case WM_DESTROY:
        Shell_NotifyIconW(NIM_DELETE, &nid);
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProc(h, m, w, l);
}

bool RegisterTray(HWND hWnd) {
    // 把窗口过程换成 TrayWndProc
    SetWindowLongPtrW(hWnd, GWLP_WNDPROC, (LONG_PTR)TrayWndProc);
    return true;
}
