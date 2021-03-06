﻿Imports System.Text.RegularExpressions

Imports Microsoft.VisualBasic.Mathematical.Helpers

Namespace Types

    ''' <summary>
    ''' A class object stand for a very simple mathematic expression that have no bracket or function.
    ''' It only contains limited operator such as +-*/\%!^ in it.
    ''' (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
    ''' 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlType>
    Public Class UnitExpression

        ''' <summary>
        ''' Arithmetic operator(运算符) 
        ''' </summary>
        ''' <remarks></remarks>
        <Xml.Serialization.XmlAttribute> Public [Operator] As Char

        ''' <summary>
        ''' The number a in the function of "Arithmetic.Evaluate".
        ''' (函数'Arithmetic.Evaluate'中的参数'a')
        ''' </summary>
        ''' <remarks></remarks>
        <Xml.Serialization.XmlAttribute> Public LEFT As Double
        ''' <summary>
        ''' The number b in the function of "Arithmetic.Evaluate".
        ''' (函数'Arithmetic.Evaluate'中的参数'b')
        ''' </summary>
        ''' <remarks></remarks>
        <Xml.Serialization.XmlAttribute> Public RIGHT As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} {1} {2} = {3}", LEFT, [Operator], RIGHT, Evaluate)
        End Function

        ''' <summary>
        ''' Calculate the value of this simple expression object.
        ''' (计算这一个简单表达式对象的值)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Evaluate() As Double
            Return Arithmetic.Evaluate(LEFT, RIGHT, [Operator])
        End Function

        ''' <summary>
        ''' Get the value of this simple expression object.
        ''' (计算这一个简单表达式对象的值)
        ''' </summary>
        ''' <param name="e"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Narrowing Operator CType(e As UnitExpression) As Double
            Return Arithmetic.Evaluate(e.LEFT, e.RIGHT, e.Operator)
        End Operator

        ''' <summary>
        ''' Convert the expression in the string type to this class object type.
        ''' (将字符串形式的简单表达式转换为本对象类型)
        ''' </summary>
        ''' <param name="expression">
        ''' The string type arithmetic expression, please make sure that it must be contains no blank 
        ''' space char exists in this string.
        ''' (字符串类型的算术表达式，请确保本字符串中没有任何的空格符号)
        ''' </param>
        ''' <returns></returns>
        ''' <exception cref="DataException">Expression contains no number(表达式中没有任何数字)</exception>
        ''' <remarks></remarks>
        Public Shared Widening Operator CType(expression As String) As UnitExpression
            Dim Match = Regex.Match(expression, Arithmetic.DOUBLE_NUMBER_REGX)

            If Match.Success Then 'match the first number in the expression string.
                Dim Left = Val(Match.Value), l = Len(Match.Value)
                Dim Right = Val(Mid$(expression, l + 2))
                Dim o As Char = expression.Chars(l)

                Return New UnitExpression With {.LEFT = Left, .RIGHT = Right, .Operator = o}
            Else
                Return Nothing   'expression contains no number!
            End If
        End Operator

        Public Shared Function Evaluate(expression As String) As Double
            Return CType(expression, UnitExpression).Evaluate
        End Function
    End Class
End Namespace