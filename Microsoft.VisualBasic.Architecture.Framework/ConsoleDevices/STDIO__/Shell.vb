﻿Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices

Namespace ConsoleDevice.STDIO__

    '
    ' * Created by SharpDevelop.
    ' * User: WORKGROUP
    ' * Date: 2015/2/26
    ' * Time: 0:13
    ' * 
    ' * To change this template use Tools | Options | Coding | Edit Standard Headers.
    ' 

    Public Module Shell

        ''' <summary>
        ''' You can create a console window In a Windows Forms project.  Project + properties, turn off "Enable application framework" 
        ''' And Set Startup Object To "Sub Main". 
        ''' 
        ''' Modify the Application.Run() statement To create the proper startup form, If necessary.
        ''' </summary>
        ''' <returns></returns>
        Public Declare Auto Function AllocConsole Lib "kernel32.dll" () As Boolean

        <DllImport("user32.dll")> _
        Public Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
        End Function

        <DllImport("kernel32")> _
        Public Function GetConsoleWindow() As IntPtr
        End Function

        <DllImport("Kernel32")> _
        Private Function SetConsoleCtrlHandler(handler As EventHandler, add As Boolean) As Boolean
        End Function

        Private ReadOnly hConsole As IntPtr = GetConsoleWindow()

        Public Sub HideConsoleWindow()
            If IntPtr.Zero <> hConsole Then
                Call ShowWindow(hConsole, 0)
            End If
        End Sub

        ''' <summary>
        ''' 为WinForm应用程序分配一个终端窗口，这个函数一般是在Debug模式之下进行程序调试所使用的
        ''' </summary>
        Public Sub ShowConsoleWindows()
            If IntPtr.Zero <> hConsole Then
                Call ShowWindow(hConsole, 1)
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CommandLine"></param>
        ''' <param name="WindowStyle"></param>
        ''' <param name="WaitForExit">If NOT, then the function returns the associated process id value. Else returns the process exit code.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Shell(CommandLine As String, Optional WindowStyle As System.Diagnostics.ProcessWindowStyle = ProcessWindowStyle.Normal, Optional WaitForExit As Boolean = False) As Integer
            Dim Tokens = Regex.Split(CommandLine, Global.Microsoft.VisualBasic.CommandLine.SPLIT_REGX_EXPRESSION)
            Dim EXE As String = Tokens.First
            Dim Arguments As String = Mid$(CommandLine, Len(EXE) + 1)
            Dim Process As System.Diagnostics.Process = New Process
            Dim pInfo As System.Diagnostics.ProcessStartInfo = New ProcessStartInfo(EXE, Arguments)

            Process.StartInfo = pInfo
            Process.StartInfo.WindowStyle = WindowStyle

            Call Process.Start()

            If Not WaitForExit Then Return Process.Id

            Call Process.WaitForExit()
            Return Process.ExitCode
        End Function
    End Module
End Namespace