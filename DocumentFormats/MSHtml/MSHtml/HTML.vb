﻿Imports System.Text
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DDM

    Public Class HTML

        Public Property Head As HtmlHead
        Public Property Body As HtmlElement

        Public Property Language As String = "zh-cn"

        Sub New(Document As HtmlDocument)

        End Sub

        Sub New()
        End Sub

        Public Function ToArray() As HtmlDocument
            Dim array As New List(Of PlantText)
            Call array.Add(Head)
            Call array.Add(Body)

            Return New HtmlDocument With {
            .Tags = {New HtmlElement With {
                .HtmlElements = array.ToArray,
                .Name = "html"}
            }
        }
        End Function

        Public Overrides Function ToString() As String
            Return DocumentWriter.ToString(ToArray)
        End Function
    End Class

    Public Class HtmlHead : Inherits HtmlElement

        Public Property CSS As CSS
        Public Property Title As HtmlElement

        Public Sub SetBodyBackground(color As String)
            If CSS Is Nothing Then
                CSS = New CSS
            End If

            Dim backColor As New Microsoft.VisualBasic.ComponentModel.KeyValuePair With {
            .Key = "background-color",
            .Value = color
        }
            Dim cssElement = New CSSElement With {
            .Name = "body",
            .Properties = New List(Of ComponentModel.KeyValuePair)({backColor})
        }
            Call CSS.Add(cssElement)
        End Sub

    End Class

    Public Class CSS : Inherits HtmlElement

        Public ReadOnly Property Elements As IReadOnlyList(Of CSSElement)
            Get
                Return _cssElements
            End Get
        End Property

        Dim _cssElements As List(Of CSSElement)

        Sub New()
            _cssElements = New List(Of CSSElement)
        End Sub

        Public Overloads Sub Add(element As CSSElement)
            Call _cssElements.Add(element)
        End Sub

        Public Overrides Property InnerText As String
            Get
                Dim values As String() = _cssElements.ToArray(Function(css) css.ToString)
                Return String.Join(vbCrLf, values)
            End Get
            Set(value As String)
                MyBase.InnerText = value
            End Set
        End Property

    End Class

    Public Class CSSElement

        Public Property Properties As List(Of Microsoft.VisualBasic.ComponentModel.KeyValuePair)
        Public Property Name As String

        Public Overrides Function ToString() As String
            Dim pValues As String() = Properties.ToArray(Function(prop) $"{prop.Key}: {prop.Value}")
            Return $"{Name} {"{"} {String.Join("; ", pValues)} {"}"}"
        End Function

    End Class
End Namespace