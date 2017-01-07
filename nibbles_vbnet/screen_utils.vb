Option Strict On

Public Module ScreenUtilities
    Private Const MF_BYCOMMAND As Integer = &H0
    Private Const SC_MAXIMIZE As Integer = &HF030
    Private Const SC_SIZE As Integer = &HF000

    Private Declare Function DeleteMenu Lib "user32.dll" (
        ByVal hMenu As IntPtr, ByVal nPosition As Integer,
        ByVal wFlags As Integer) As Integer

    Private Declare Function GetSystemMenu Lib "user32.dll" (
        hWnd As IntPtr, bRevert As Boolean) As IntPtr

    Sub DisableConsoleResizing()
        ' Get the handle to the console window
        Dim handle As IntPtr = Process.GetCurrentProcess.MainWindowHandle
        If handle <> IntPtr.Zero Then Return

        ' Get the handle to the system menu of the console window
        Dim sysMenu As IntPtr = GetSystemMenu(handle, False)
        ' Prevent the user from maximizing console window.
        DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND)
        ' Prevent the user from resizing console window.
        DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND)
    End Sub
End Module
