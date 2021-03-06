﻿Imports System.Threading
Imports System.Threading.Thread

Namespace ComponentModel.DataSourceModel

    Public Class Iterator(Of T) : Inherits Iterator
        Implements IEnumerator(Of T)

        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(source)
        End Sub

        Public ReadOnly Property GetCurrent As T Implements IEnumerator(Of T).Current
            Get
                Return DirectCast(Current, T)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Implements for the IEnumerable(Of T), Supports a simple iteration over a non-generic collection.
    ''' </summary>
    Public Class Iterator : Implements IEnumerator
        Implements IDisposable

        ReadOnly _source As IEnumerable

        Sub New(source As IEnumerable)
            _source = source
            Reset()
        End Sub

        ''' <summary>
        ''' Gets the current element in the collection.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
        Public ReadOnly Property ReadDone As Boolean

        Dim receiveDone As New ManualResetEvent(False)

        Private Sub __moveNext()
            _ReadDone = False

            ' Single thread safely
            For Each x As Object In _source ' 单线程安全
                _Current = x

                Call receiveDone.WaitOne()
                Call receiveDone.Reset()
            Next

            _ReadDone = True
        End Sub

        ''' <summary>
        ''' Returns current and then automatically move to next position
        ''' </summary>
        ''' <returns></returns>
        Public Function Read() As Object
            Dim x As Object = Current
            Call MoveNext()
            Return x
        End Function

        Dim _forEach As Thread

        ''' <summary>
        ''' Sets the enumerator to its initial position, which is before the first element in the collection.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            If Not _forEach Is Nothing Then  ' 终止这条线程然后再新建
                Call _forEach.Abort()
            End If

            _forEach = New Thread(AddressOf __moveNext)
            _forEach.Start()
        End Sub

        ''' <summary>
        ''' Advances the enumerator to the next element of the collection.
        ''' </summary>
        ''' <returns>
        ''' true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        ''' </returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Call receiveDone.Set()
            Return Not ReadDone
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call _forEach.Abort()
                    Call _forEach.Free
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