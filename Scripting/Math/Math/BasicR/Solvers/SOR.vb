﻿'Namespace BasicR.Solvers

'    Public Class SOR : Implements BasicR.Solvers.ISolver

'#Region "Solver Controls"
'        ''' <summary>
'        ''' 松弛因子
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <Xml.Serialization.XmlAttribute> Public Property Omiga As Double = 1.2
'        ''' <summary>
'        ''' 最大允许迭代次数
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <Xml.Serialization.XmlAttribute> Public Property Iteration As Integer = 50
'        ''' <summary>
'        ''' 误差容限
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <Xml.Serialization.XmlAttribute> Public Property e As Double = 0.00000001
'#End Region

'        Public Function Solve(A As MATRIX, b As VECTOR) As VECTOR Implements ISolver.Solve
'            Dim N As Integer = A.Height
'            Dim x1 As VECTOR = New VECTOR(N), x As VECTOR = New VECTOR(N)

'            For k As Integer = 0 To Me.Iteration
'                For i As Integer = 0 To N - 1
'                    Dim sum As Double
'                    For j As Integer = 0 To N - 1
'                        If j < i Then
'                            sum += A(i, j) * x(j)
'                        ElseIf j > i Then
'                            sum += A(i, j) * x1(j)
'                        End If
'                    Next

'                    x(i) = (b(i) - sum) * Omiga / A(i, i) + (1.0 - Omiga) * x1(i)
'                Next
'#If DEBUG Then
'                Console.WriteLine(x.ToString)
'#End If
'                Dim dx As VECTOR = x - x1, err As Double = Math.Sqrt(dx.Mod)

'                If err < Me.e Then
'                    Exit For
'                End If

'                Call x.CopyTo(x1)
'            Next

'            Return x
'        End Function

'        Public Overrides Function ToString() As String
'            Return "BasicR -> Solver(SOR)"
'        End Function

'#Region "IDisposable Support"
'        Private disposedValue As Boolean ' 检测冗余的调用

'        ' IDisposable
'        Protected Overridable Sub Dispose(disposing As Boolean)
'            If Not Me.disposedValue Then
'                If disposing Then
'                    ' TODO:  释放托管状态(托管对象)。
'                End If

'                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
'                ' TODO:  将大型字段设置为 null。
'            End If
'            Me.disposedValue = True
'        End Sub

'        ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
'        'Protected Overrides Sub Finalize()
'        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
'        '    Dispose(False)
'        '    MyBase.Finalize()
'        'End Sub

'        ' Visual Basic 添加此代码是为了正确实现可处置模式。
'        Public Sub Dispose() Implements IDisposable.Dispose
'            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
'            Dispose(True)
'            GC.SuppressFinalize(Me)
'        End Sub
'#End Region
'    End Class
'End Namespace