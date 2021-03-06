﻿Imports System.Runtime.CompilerServices

Public Module Matrix

    ''' <summary>
    ''' 生成一个有m行n列的矩阵，但是是使用数组来表示的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="m"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function MAT(Of T)(m As Integer, n As Integer) As T()()
        Dim newMAT As T()() = New T(m - 1)() {}

        For i As Integer = 0 To m - 1
            newMAT(i) = New T(n - 1) {}
        Next

        Return newMAT
    End Function

    ''' <summary>
    ''' Convert the data collection into a matrix value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source">The elements number in each collection should be agreed!(要求集合之中的每一列之中的数据的元素数目都相等)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToMatrix(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As T(,)
        Dim width As Integer = source.First.Count
        Dim array As IEnumerable(Of T)() = source.ToArray
        Dim height As Integer = array.Length
        Dim MAT As T(,) = New T(height - 1, width - 1) {}

        For i As Integer = 0 To height - 1
            Dim row As T() = array(i).ToArray

            For j As Integer = 0 To width - 1
                MAT(i, j) = row(j)
            Next
        Next

        Return MAT
    End Function

    ''' <summary>
    ''' Convert the matrix data into a collection of collection data type.(将矩阵对象转换为集合的集合的类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToVectorList(Of T)(MAT As T(,)) As List(Of T())
        Dim ChunkList As List(Of T()) = New List(Of T())
        Dim width As Integer = MAT.GetLength(1)
        Dim height As Integer = MAT.GetLength(0)

        For i As Integer = 0 To height - 1
            Dim Temp As T() = New T(width - 1) {}

            For j As Integer = 0 To width - 1
                Temp(j) = MAT(i, j)
            Next

            Call ChunkList.Add(Temp)
        Next

        Return ChunkList
    End Function

End Module
