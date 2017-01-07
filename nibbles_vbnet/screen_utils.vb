Option Strict On

Public Module ScreenUtilities
    Private Const MF_BYCOMMAND As Integer = &H0
    Private Const SC_CLOSE As Integer = &HF060
    Private Const SC_MINIMIZE As Integer = &HF020
    Private Const SC_MAXIMIZE As Integer = &HF030
    Private Const SC_SIZE As Integer = &HF000

    Private Declare Function DeleteMenu Lib "user32.dll" (ByVal hMenu As IntPtr, ByVal nPosition As Integer, ByVal wFlags As Integer) As Integer
    Private Declare Function GetSystemMenu Lib "user32.dll" (hWnd As IntPtr, bRevert As Boolean) As IntPtr

    Sub DisableConsoleResizing()

        Dim handle As IntPtr
        handle = Process.GetCurrentProcess.MainWindowHandle ' Get the handle to the console window

        Dim sysMenu As IntPtr
        sysMenu = GetSystemMenu(handle, False) ' Get the handle to the system menu of the console window

        If handle <> IntPtr.Zero Then
            DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND) ' To prevent user from closing console window
            DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND) 'To prevent user from minimizing console window
            DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND) 'To prevent user from maximizing console window
            DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND) 'To prevent the use from re-sizing console window
        End If
    End Sub
End Module
