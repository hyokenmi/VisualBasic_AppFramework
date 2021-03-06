﻿Option Strict Off

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream.Linq

    Public Class LinqWhere(Of T As Class)

        Public ReadOnly Property Tokens As ExprToken()

        ReadOnly _schema As SchemaProvider
        ReadOnly _lstOper As New List(Of Func(Of T, Boolean))
        ''' <summary>
        ''' 操作符代码
        ''' </summary>
        ReadOnly _operations As Dictionary(Of String, __test) =
            New Dictionary(Of String, __test) From {
 _
            {">", AddressOf __gt},
            {"<", AddressOf __lt},
            {"=", AddressOf __eq},
            {"!=", AddressOf __neq},
            {"%", AddressOf __instr},
            {"@", AddressOf __regex}
        }

        Protected Sub New()
            _schema = SchemaProvider.CreateObject(Of T)(False)
            _schema = _schema.CopyWriteDataToObject
        End Sub

        Dim __testInvoke As Func(Of T, Boolean)

        ''' <summary>
        ''' LINQ WHERE TEST(x) = TRUE
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Test(x As T) As Boolean
            Return __testInvoke(x)
        End Function

        ''' <summary>
        ''' 编译LINQ数据库查询引擎之中的条件表达式
        ''' OR 运算的级别是最低的
        ''' a and b and c or (d and e and f or (g and h))
        ''' </summary>
        ''' <returns></returns>
        Protected Function Compile() As LinqWhere(Of T)
            Dim lst As New List(Of ExprToken)(Tokens)
            Dim stack As Func(Of T, Boolean) = AddressOf __getTest(lst.First).TestToken

            Call lst.RemoveAt(Scan0)

            If lst.Count = 0 Then
                __testInvoke = stack
            Else
                __testInvoke = __compile(lst, stack)
            End If

            Return Me
        End Function

        Private Function __getTest(token As ExprToken) As __invoke
            Dim oprFrom = (From x As Column In _schema.Columns
                           Where String.Equals(token.Column, x.Name, StringComparison.OrdinalIgnoreCase)
                           Select x).FirstOrDefault
            If oprFrom Is Nothing Then
                Dim exMsg As String = $"Could not found column entry for ""{token.Column}""!"
                Throw New EntryPointNotFoundException(exMsg)
            End If

            Dim opr As __test = _operations(token.Operator)
            Dim testToken As New __invoke With {
                .test = opr,
                .oprFrom = oprFrom,
                .oprToken = token,
                .type = oprFrom.BindProperty.PropertyType
            }
            Return testToken
        End Function

        ''' <summary>
        ''' 编译查询条件表达式
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="stack"></param>
        ''' <returns></returns>
        Private Function __compile(ByRef tokens As List(Of ExprToken),
                                   stack As Func(Of T, Boolean)) As Func(Of T, Boolean)

            Dim lst As ExprToken() = tokens.Take(2).ToArray
            Dim opr As ExprToken = lst(Scan0)
            Dim param As ExprToken = lst(1)

            Call tokens.RemoveAt(Scan0)  ' 去掉第一个逻辑操作符
            Call tokens.RemoveAt(Scan0)

            If String.Equals(opr.Operator, "and", StringComparison.OrdinalIgnoreCase) Then
                Dim _innerStack As Func(Of T, Boolean) =    ' 对当前的运算做堆栈处理
                    Function(x As T) stack(x) AndAlso __getTest(param).TestToken(x) ' 从左到右递归编译右边的表达式

                If tokens.Count = 0 Then
                    Return _innerStack
                Else
                    Return __compile(tokens, _innerStack)
                End If
            ElseIf String.Equals(opr.Operator, "or", StringComparison.OrdinalIgnoreCase) Then
                Dim left As Func(Of T, Boolean) = AddressOf __getTest(param).TestToken ' 从左到右递归编译右边的表达式

                If tokens.Count = 0 Then
                    Return Function(x As T) stack(x) OrElse left(x)
                Else
                    Dim _inner As Func(Of T, Boolean) = __compile(tokens, left)  ' 对当前的运算做堆栈处理
                    Return Function(x As T) stack(x) OrElse _inner(x)
                End If
            Else
                Throw New InvalidOperationException($"{opr.ToString} is not a valid logical operator!")
            End If
        End Function

        Private Class __invoke
            Public oprFrom As Column, oprToken As ExprToken, type As Type
            Public test As __test

            Public Function TestToken(x As T) As Boolean
                Dim value As Object = oprFrom.BindProperty.GetValue(x, Nothing)
                Dim result As Boolean = test(value, oprToken.Value, type)
                Return result
            End Function

            Public Overrides Function ToString() As String
                Return oprToken.ToString
            End Function
        End Class

#Region "inner test token"

        Private Delegate Function __test(a As Object, test As String, type As Type) As Boolean

        ''' <summary>
        ''' a > b
        ''' </summary>
        ''' <returns></returns>
        Public Function __gt(a As Object, test As String, type As Type) As Boolean
            Select Case type
                Case GetType(String) : Return DirectCast(a, String) > test
                Case GetType(Integer) : Return DirectCast(a, Integer) > CInt(Val(test))
                Case GetType(Long) : Return DirectCast(a, Long) > CLng(Val(test))
                Case GetType(Double) : Return DirectCast(a, Double) > Val(test)
                Case GetType(UInteger) : Return DirectCast(a, UInteger) > CInt(Val(test))
                Case GetType(ULong) : Return DirectCast(a, ULong) > CLng(Val(test))
                Case GetType(Boolean) : Return DirectCast(a, Boolean) > test.getBoolean
                Case GetType(DateTime) : Return DirectCast(a, DateTime) > DateTime.Parse(test)
                Case Else ' Object
                    Return a > test
            End Select
        End Function

        ''' <summary>
        ''' a &lt; b
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="test"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function __lt(a As Object, test As String, type As Type) As Boolean
            Select Case type
                Case GetType(String) : Return DirectCast(a, String) < test
                Case GetType(Integer) : Return DirectCast(a, Integer) < CInt(Val(test))
                Case GetType(Long) : Return DirectCast(a, Long) < CLng(Val(test))
                Case GetType(Double) : Return DirectCast(a, Double) < Val(test)
                Case GetType(UInteger) : Return DirectCast(a, UInteger) < CInt(Val(test))
                Case GetType(ULong) : Return DirectCast(a, ULong) < CLng(Val(test))
                Case GetType(Boolean) : Return DirectCast(a, Boolean) < test.getBoolean
                Case GetType(DateTime) : Return DirectCast(a, DateTime) < DateTime.Parse(test)
                Case Else ' Object
                    Return a < test
            End Select
        End Function

        ''' <summary>
        ''' a = b
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="test"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function __eq(a As Object, test As String, type As Type) As Boolean
            Select Case type
                Case GetType(String) : Return String.Equals(DirectCast(a, String), test)
                Case GetType(Integer) : Return DirectCast(a, Integer) = CInt(Val(test))
                Case GetType(Long) : Return DirectCast(a, Long) = CLng(Val(test))
                Case GetType(Double) : Return DirectCast(a, Double) = Val(test)
                Case GetType(UInteger) : Return DirectCast(a, UInteger) = CInt(Val(test))
                Case GetType(ULong) : Return DirectCast(a, ULong) = CLng(Val(test))
                Case GetType(Boolean) : Return DirectCast(a, Boolean) = test.getBoolean
                Case GetType(DateTime) : Return DirectCast(a, DateTime) = DateTime.Parse(test)
                Case Else ' Object
                    Return a.Equals(test)
            End Select
        End Function

        ''' <summary>
        ''' a != b
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="test"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function __neq(a As Object, test As String, type As Type) As Boolean
            Return Not __eq(a, test, type)
        End Function

        ''' <summary>
        ''' InStr(a, b) > 0
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="test"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function __instr(a As Object, test As String, type As Type) As Boolean
            Dim sa As String = Scripting.ToString(a)
            Return InStr(sa, test, CompareMethod.Text) > 0
        End Function

        ''' <summary>
        ''' Regex.Match(a, b).Success
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="test"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function __regex(a As Object, test As String, type As Type) As Boolean
            Dim sa As String = Scripting.ToString(a)
            Return Regex.Match(sa, test, RegexOptions.IgnoreCase).Success
        End Function
#End Region

        Public Overrides Function ToString() As String
            Return Tokens.ToArray(Function(x) x.ToString).JoinBy(" ")
        End Function

        ''' <summary>
        ''' column &lt;opr> value
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        Public Shared Function TryParse(expr As String) As LinqWhere(Of T)
            Dim tokens = CommandLine.GetTokens(expr)
            Dim lstCond = tokens.ToArray(Function(s) __tokenParser(s))
            Return New LinqWhere(Of T) With {
            ._Tokens = lstCond
        }.Compile
        End Function

        Private Shared Function __tokenParser(s As String) As ExprToken
            Dim name As String
            Dim opr As Integer, [operator] As String = ""

            If s.First = """"c Then

                s = Mid(s, 2)

                Dim p As Integer = InStr(s, """")

                If p = 0 Then
                    Return Nothing
                End If

                name = Mid(s, 1, p - 1)
                s = Mid(s, p + 1)
                opr = __getOperator(s, [operator])
            Else
                opr = __getOperator(s, [operator])
                If opr = -1 Then  ' 可能是逻辑运算符
                    Return New ExprToken With {.Operator = s}
                End If
                name = Mid(s, 1, opr - 1)
            End If

            s = Mid(s, opr + Len([operator]))
            s = s.GetString

            Return New ExprToken With {
                .Column = name,
                .Operator = [operator],
                .Value = s
            }
        End Function

        Private Shared Function __getOperator(expr As String, ByRef opr As String) As Integer
            Dim p As Integer

            If InStr(expr, "<").ShadowCopy(p) > 0 Then
                opr = "<"
            ElseIf InStr(expr, ">").ShadowCopy(p) > 0 Then
                opr = ">"
            ElseIf InStr(expr, "!=").ShadowCopy(p) > 0 Then
                opr = "!="
            ElseIf InStr(expr, "=").ShadowCopy(p) > 0 Then
                opr = "="
            ElseIf InStr(expr, "%").ShadowCopy(p) > 0 Then
                opr = "%"
            ElseIf InStr(expr, "@").ShadowCopy(p) > 0 Then
                opr = "@"
            Else
                p = -1
            End If

            Return p
        End Function
    End Class

    Public Class ExprToken

        Public Property Column As String
        ''' <summary>
        ''' > (int) greater than, 
        ''' &lt; (int) less than, 
        ''' = (int, string, boolean) equals, 
        ''' != (int, string, boolean) not equals, 
        ''' % (string) InStr, 
        ''' @ (string) Regex Matches
        ''' </summary>
        ''' <returns></returns>
        Public Property [Operator] As String
        Public Property Value As String

        Public ReadOnly Property IsLogical As Boolean
            Get
                If String.IsNullOrEmpty(Column) AndAlso String.IsNullOrEmpty(Value) Then
                    Return String.Equals([Operator], "and") OrElse
                        String.Equals([Operator], "or") OrElse
                        String.Equals([Operator], "not")
                End If

                Return False
            End Get
        End Property

        Public Overrides Function ToString() As String
            If IsLogical Then
                Return $" {[Operator]} "
            Else
                Return "$" & $"{Column} {[Operator]} {Value}"
            End If
        End Function
    End Class
End Namespace