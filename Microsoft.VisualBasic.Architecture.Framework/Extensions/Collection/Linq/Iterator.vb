﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization

Namespace Linq

    Public Module IteratorExtensions

        <Extension>
        Public Iterator Function SeqIterator(Of T)(source As IEnumerable(Of T), Optional offset As Integer = 0) As IEnumerable(Of SeqValue(Of T))
            If Not source.IsNullOrEmpty Then
                Dim idx As Integer = offset

                For Each x As T In source
                    Yield New SeqValue(Of T)(idx, x)
                    idx += 1
                Next
            End If
        End Function

        <Extension>
        Public Iterator Function SeqIterator(Of T1, T2)(seqFrom As IEnumerable(Of T1),
                                                        follows As IEnumerable(Of T2),
                                                        Optional offset As Integer = 0) As IEnumerable(Of SeqValue(Of T1, T2))
            Dim x As T1() = seqFrom.ToArray
            Dim y As T2() = follows.ToArray

            For i As Integer = 0 To x.Length - 1
                Yield New SeqValue(Of T1, T2)(i + offset, x(i), y.Get(i))
            Next
        End Function
    End Module

    Public Structure SeqValue(Of T1, T2) : Implements IAddressHandle

        Public Property Pos As Integer
        Public Property obj As T1
        Public Property Follow As T2

        Private Property AddrHwnd As Long Implements IAddressHandle.AddrHwnd
            Get
                Return CLng(Pos)
            End Get
            Set(value As Long)
                Pos = CInt(value)
            End Set
        End Property

        Sub New(i As Integer, x As T1, y As T2)
            Pos = i
            obj = x
            Follow = y
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Structure

    Public Structure SeqValue(Of T) : Implements IAddressHandle

        Public Property Pos As Integer
        Public Property obj As T

        Private Property AddrHwnd As Long Implements IAddressHandle.AddrHwnd
            Get
                Return CLng(Pos)
            End Get
            Set(value As Long)
                Pos = CInt(value)
            End Set
        End Property

        Sub New(i As Integer, x As T)
            Pos = i
            obj = x
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As T
            Return x.obj
        End Operator

        Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As Integer
            Return x.Pos
        End Operator

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Structure
End Namespace