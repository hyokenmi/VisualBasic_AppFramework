﻿
Namespace ComponentModel

    ''' <summary>
    ''' 对象类型的组合输出工具，即目标类型的集合之中的元素两两组合配对
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Comb(Of T)

        Dim lstSource As List(Of T)
        Dim p As Integer = 1
        Dim _NewLine As Boolean = True

        ''' <summary>
        ''' 对象列表是否已经完全组合输出
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property EOL As Boolean
            Get
                Return Not lstSource.Count >= 2
            End Get
        End Property

        ''' <summary>
        ''' 是否已经开始读取新的一行数据
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NewLine As Boolean
            Get
                Return _NewLine
            End Get
        End Property

        Public ReadOnly Property CombList As KeyValuePair(Of T, T)()()
            Get
                Dim LQuery = (From Handle As Integer In lstSource.Sequence.Take(lstSource.Count - 1).AsParallel
                              Let ArrayList = (From Hwnd As Integer In lstSource.Skip(Handle + 1).Sequence
                                               Let b = Handle + 1
                                               Select New KeyValuePair(Of T, T)(lstSource(Handle), lstSource(b + Hwnd))).ToArray
                              Select ArrayList
                              Order By ArrayList.Length Descending).ToArray
                Return LQuery

                'Dim n As Integer = ObjectCollection.Count - 1
                'Dim List As List(Of KeyValuePair(Of T, T)()) = New List(Of KeyValuePair(Of T, T)())
                'Dim TEMP As List(Of KeyValuePair(Of T, T)) = New List(Of KeyValuePair(Of T, T))

                'For i As Integer = 0 To ObjectCollection.Count - 1
                '    For idx As Integer = i + 1 To ObjectCollection.Count - 1
                '        Call TEMP.Add(New KeyValuePair(Of T, T)(ObjectCollection(i), ObjectCollection(idx)))
                '    Next
                '    Call List.Add(TEMP.ToArray)
                '    Call TEMP.Clear()
                'Next

                'Return List.ToArray
            End Get
        End Property

        Public Function GetObjectPair() As KeyValuePair(Of T, T)
            If lstSource.Count = 1 Then
                Return Nothing
            End If

            If p < lstSource.Count Then
                Dim [Object] As T = lstSource(p)
                _NewLine = False
                p += 1
                Return New KeyValuePair(Of T, T)(lstSource(0), [Object])
            Else
                lstSource.RemoveAt(0)
                p = 1
                Dim [ObjectPair] = GetObjectPair()
                _NewLine = True

                Return ObjectPair
            End If
        End Function

        Friend Sub New()
        End Sub

        Public Shared Function CreateObject(Collection As Generic.IEnumerable(Of T)) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = Collection.ToList}
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("Comb(Of {0})::There is {1} object last.", GetType(T).FullName, lstSource.Count)
        End Function

        Public Shared Widening Operator CType(ObjectCollection As T()) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = ObjectCollection.ToList}
        End Operator

        Public Shared Widening Operator CType(ObjectCollection As List(Of T)) As Comb(Of T)
            Return New Comb(Of T) With {.lstSource = ObjectCollection}
        End Operator

        ''' <summary>
        ''' Creates the completely combination of the elements in the target input collection source.
        ''' (创建完完全全的两两配对)
        ''' </summary>
        ''' <param name="Collection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateCompleteObjectPairs(Collection As Generic.IEnumerable(Of T)) As KeyValuePair(Of T, T)()()
            Dim LQuery = (From i As Integer In Collection.Sequence
                          Select (From j As Integer
                                  In Collection.Sequence
                                  Select New KeyValuePair(Of T, T)(Collection(i), Collection(j))).ToArray).ToArray
            Return LQuery
        End Function
    End Class

    ''' <summary>
    ''' 任意多个集合之间的对象之间相互组成组合输出
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Comb

        Public Function CreateCombos(Of TA, TB)(sourceA As Generic.IEnumerable(Of TA), sourceB As Generic.IEnumerable(Of TB)) As KeyValuePair(Of TA, TB)()
            Dim List As New List(Of KeyValuePair(Of TA, TB))

            For Each ItemA In sourceA
                For Each itemB In sourceB
                    Call List.Add(New KeyValuePair(Of TA, TB)(ItemA, itemB))
                Next
            Next

            Return List.ToArray
        End Function

        Public Function Generate(Of T)(source As T()()) As T()()
            Dim FirstCollection = source.First

            If source.Length = 2 Then '只剩下两个的时候，会退出递归操作
                Dim LastCollection = source.Last
                Dim ChunkBuffer As List(Of T()) = New List(Of T())

                For Each item In FirstCollection
                    For Each _item In LastCollection
                        Call ChunkBuffer.Add(New T() {item, _item})
                    Next
                Next

                Return ChunkBuffer.ToArray
            Else
                Dim ChunkBuffer As List(Of T()) = New List(Of T())
                Dim LastCollection = Generate(source.Skip(1).ToArray)

                Call Console.Write(source.Length & ",")

                For Each item In FirstCollection
                    For Each ItemCollection In LastCollection
                        Dim TempChunk = New List(Of T) From {item}
                        Call TempChunk.AddRange(ItemCollection)
                        Call ChunkBuffer.Add(TempChunk.ToArray)
                    Next
                Next

                Return ChunkBuffer.ToArray
            End If
        End Function
    End Module
End Namespace