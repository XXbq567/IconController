#include "pch.h"
#include "../res/resource.h"

struct Config {
    bool hideIcons      = true;
    bool showInSysTray  = true;
    int  transparency   = 0;
} g_cfg;

static void SaveCfg() {
    WCHAR path[MAX_PATH];
    SHGetFolderPathW(nullptr, CSIDL_APPDATA, nullptr, 0, path);
    wcscat_s(path, L"\\DeskIconCtl.ini");
    WritePrivateProfileStringW(L"cfg", L"hideIcons",    g_cfg.hideIcons     ? L"1" : L"0", path);
    WritePrivateProfileStringW(L"cfg", L"showInSysTray", g_cfg.showInSysTray ? L"1" : L"0", path);
    WritePrivateProfileStringW(L"cfg", L"transparency",  std::to_wstring(g_cfg.transparency).c_str(), path);
}
static void LoadCfg() {
    WCHAR path[MAX_PATH];
    SHGetFolderPathW(nullptr, CSIDL_APPDATA, nullptr, 0, path);
    wcscat_s(path, L"\\DeskIconCtl.ini");
    g_cfg.hideIcons     = GetPrivateProfileIntW(L"cfg", L"hideIcons",    1, path) != 0;
    g_cfg.showInSysTray = GetPrivateProfileIntW(L"cfg", L"showInSysTray", 1, path) != 0;
    g_cfg.transparency  = GetPrivateProfileIntW(L"cfg", L"transparency", 0, path);
}

void ShowSettingsDlg(HWND hParent) {
    LoadCfg();
    DialogBoxParamW(GetModuleHandle(nullptr), MAKEINTRESOURCE(IDD_SETTINGS), hParent,
        [](HWND h, UINT m, WPARAM w, LPARAM l)->INT_PTR {
            switch (m) {
            case WM_INITDIALOG:
                CheckDlgButton(h, IDC_HIDE, g_cfg.hideIcons ? BST_CHECKED : BST_UNCHECKED);
                CheckDlgButton(h, IDC_TRAY, g_cfg.showInSysTray ? BST_CHECKED : BST_UNCHECKED);
                HWND cb = GetDlgItem(h, IDC_TRANS);
                SendMessageW(cb, CB_ADDSTRING, 0, (LPARAM)L"完全隐藏");
                SendMessageW(cb, CB_ADDSTRING, 0, (LPARAM)L"透明度10%(占位)");
                SendMessageW(cb, CB_SETCURSEL, g_cfg.transparency, 0);
                return TRUE;
            case WM_COMMAND:
                if (LOWORD(w) == IDOK) {
                    g_cfg.hideIcons     = IsDlgButtonChecked(h, IDC_HIDE) == BST_CHECKED;
                    g_cfg.showInSysTray = IsDlgButtonChecked(h, IDC_TRAY) == BST_CHECKED;
                    g_cfg.transparency  = (int)SendDlgItemMessage(h, IDC_TRANS, CB_GETCURSEL, 0, 0);
                    SaveCfg();
                    EndDialog(h, 1);
                }
                if (LOWORD(w) == IDCANCEL) EndDialog(h, 0);
                break;
            }
            return FALSE;
        }, 0);
}
