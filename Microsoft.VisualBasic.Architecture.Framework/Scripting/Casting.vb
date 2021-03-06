﻿Imports System.Drawing
Imports System.IO
Imports System.Text

Namespace Scripting

    Public Module Casting

        Private Function val(s As String) As Double
            s = s.Replace(",", "")
            Return Conversion.Val(s)
        End Function

        Public Function CastString(obj As String) As String
            Return obj
        End Function

        Public Function CastChar(obj As String) As Char
            Return If(String.IsNullOrEmpty(obj), NIL, obj.First)
        End Function

        Public Function CastInteger(obj As String) As Integer
            Return CInt(val(obj))
        End Function

        Public Function CastDouble(obj As String) As Double
            Return Val(obj)
        End Function

        Public Function CastLong(obj As String) As Long
            Return CLng(Val(obj))
        End Function

        Public Function CastBoolean(obj As String) As Boolean
            Return obj.getBoolean
        End Function

        Public Function CastCharArray(obj As String) As Char()
            Return obj.ToArray
        End Function

        Public Function CastDate(obj As String) As DateTime
            Return DateTime.Parse(obj)
        End Function

        Public Function CastStringBuilder(obj As String) As StringBuilder
            Return New StringBuilder(obj)
        End Function

        Public Function CastCommandLine(obj As String) As CommandLine.CommandLine
            Return CommandLine.TryParse(obj)
        End Function

        Public Function CastImage(path As String) As Image
            Return LoadImage(path)
        End Function

        Public Function CastFileInfo(path As String) As FileInfo
            Return FileIO.FileSystem.GetFileInfo(path)
        End Function

        Public Function CastGDIPlusDeviceHandle(path As String) As GDIPlusDeviceHandle
            Return GDIPlusDeviceHandleFromImageFile(path)
        End Function

        Public Function CastColor(rgb As String) As Color
            Return rgb.ToColor
        End Function

        Public Function CastFont(face As String) As Font
            Return New Font(face, 10)
        End Function

        Public Function CastIPEndPoint(addr As String) As System.Net.IPEndPoint
            Return New Net.IPEndPoint(addr).GetIPEndPoint
        End Function

        Public Function CastLogFile(path As String) As Logging.LogFile
            Return New Logging.LogFile(path)
        End Function

        Public Function CastProcess(exe As String) As Process
            Return Process.Start(exe)
        End Function

    End Module
End Namespace