﻿Imports System.ComponentModel
Imports System.Reflection
Imports System.Xml.Serialization

Namespace Scripting.MetaData

    ''' <summary>
    ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
    ''' </summary>
    <XmlType("PackageNamespace", [Namespace]:="Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a")>
    Public Class PackageNamespace : Inherits CommandLine.Reflection.[Namespace]

        ''' <summary>
        ''' This plugins project's home page url.
        ''' </summary>
        ''' <returns></returns>
        Public Property Url As String
        ''' <summary>
        ''' Your name or E-Mail
        ''' </summary>
        ''' <returns></returns>
        Public Property Publisher As String
        Public Property Revision As Integer
        ''' <summary>
        ''' 这个脚本模块包的文献引用列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Cites As String
        Public Property Category As APICategories = APICategories.SoftwareTools

        Public Overloads Shared ReadOnly Property TypeInfo As System.Type =
            GetType(PackageNamespace)

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        ''' <param name="ns"></param>
        Public Sub New(ns As Microsoft.VisualBasic.CommandLine.Reflection.Namespace)
            Me.Namespace = ns.Namespace
            Me.Description = ns.Description
        End Sub

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        ''' <param name="[Namespace]"></param>
        Public Sub New([Namespace] As String)
            Me.Namespace = [Namespace]
        End Sub

        ''' <summary>
        ''' This attribute provides a more details information about a namepace package module in your scripting plugins.
        ''' </summary>
        Protected Sub New()
        End Sub

        Public Shared Function GetEntry(type As Type) As PackageNamespace
            Dim attrs = type.GetCustomAttributes(Of PackageNamespace)(inherit:=True)
            Return attrs.FirstOrDefault
        End Function
    End Class

    Public Enum APICategories As Integer
        ''' <summary>
        ''' API for facilities of the software development.
        ''' </summary>
        <Description("API for facilities of the software development.")>
        SoftwareTools = 0
        ''' <summary>
        ''' Analysis Tools API that applied on your scientific research or industry production on computer science.
        ''' </summary>
        <Description("Analysis Tools API that applied on your scientific research or industry production on computer science.")>
        ResearchTools = 2
        ''' <summary>
        ''' Something small utilities for facility the scripting, makes your programming more easily.
        ''' </summary>
        <Description("Something small utilities for facility the scripting, makes your programming more easily.")>
        UtilityTools = 4
        ''' <summary>
        ''' CLI program help manual.
        ''' </summary>
        <Description("CLI program help manual.")>
        CLI_MAN = 8
    End Enum
End Namespace