﻿Option Strict Off

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream.Linq

    ''' <summary>
    ''' 文件写入流，这个一般是在遇到非常大的文件流的时候才需要使用
    ''' </summary>
    Public Class WriteStream(Of T As Class)
        Implements System.IDisposable

        ReadOnly handle As String
        ReadOnly _fileIO As System.IO.StreamWriter
        ReadOnly RowWriter As ComponentModels.RowWriter

        Sub New(path As String, Optional Explicit As Boolean = False)
            Dim typeDef As Type = GetType(T)
            Dim Schema = Csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject(typeDef, Explicit).CopyReadDataFromObject

            RowWriter = New ComponentModels.RowWriter(Schema)
            handle = FileIO.FileSystem.GetFileInfo(path).FullName
            _fileIO = New IO.StreamWriter(path:=handle)

            Dim title As RowObject = RowWriter.GetRowNames
            Dim sTitle As String = title.AsLine
            Call _fileIO.WriteLine(sTitle)
        End Sub

        Public Overrides Function ToString() As String
            Return handle.ToFileURL
        End Function

        ''' <summary>
        ''' 将对象的数据源写入Csv文件之中
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function Flush(source As Generic.IEnumerable(Of T)) As Boolean
            If source.IsNullOrEmpty Then
                Return True  ' 要不然会出现空行，会造成误解的，所以要在这里提前结束
            End If

            Dim LQuery As String() = (From line As T In source.AsParallel
                                      Where Not line Is Nothing  ' 忽略掉空值对象，否则会生成空行
                                      Let CreatedRow As RowObject = RowWriter.ToRow(line)
                                      Select CreatedRow).ToArray(Function(x) x.AsLine) ' 对象到数据的投影

            Dim block As String = LQuery.JoinBy(vbCrLf)
            Call _fileIO.WriteLine(block)
            Call _fileIO.Flush()

            Return True
        End Function

        Public Function Flush(obj As T) As Boolean
            If obj Is Nothing Then
                Return False
            End If

            Dim line As String = RowWriter.ToRow(obj).AsLine
            Call _fileIO.WriteLine(line)
            Call _fileIO.Flush()

            Return True
        End Function

        Public Function ToArray(Of Tsrc)(_ctype As Func(Of Tsrc, T())) As Action(Of Tsrc)
            Return AddressOf New __ctypeTransform(Of Tsrc) With {
                .__IO = Me,
                .__ctypeArray = _ctype
            }.WriteArray
        End Function

        Public Function [Ctype](Of Tsrc)(_ctype As Func(Of Tsrc, T)) As Action(Of Tsrc)
            Return AddressOf New __ctypeTransform(Of Tsrc) With {
                .__IO = Me,
                .__ctyper = _ctype
            }.WriteObj
        End Function

        Private Class __ctypeTransform(Of Tsrc)
            Public __IO As WriteStream(Of T)
            Public __ctypeArray As Func(Of Tsrc, T())
            Public __ctyper As Func(Of Tsrc, T)

            Public Sub WriteArray(source As Tsrc)
                Dim array As T() = __ctypeArray(source)
                Call __IO.Flush(array)
            End Sub

            Public Sub WriteObj(source As Tsrc)
                Dim obj As T = __ctyper(source)
                Call __IO.Flush(obj)
            End Sub

            Public Overrides Function ToString() As String
                Return $"{GetType(Tsrc).FullName} ->> {GetType(T).FullName}  @{__IO.ToString}"
            End Function
        End Class

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call _fileIO.Close()
                    Call _fileIO.Dispose()    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace