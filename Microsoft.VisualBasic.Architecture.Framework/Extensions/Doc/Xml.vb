﻿Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("Doc.Xml", Description:="Tools for read and write sbml, KEGG document, etc, xml based documents...")>
Public Module XmlDoc

    ''' <summary>
    ''' 从文件之中加载XML之中的数据至一个对象类型之中
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="XmlFile">XML文件的文件路径</param>
    ''' <param name="ThrowEx">当反序列化出错的时候是否抛出错误？假若不抛出错误，则会返回空值</param>
    ''' <param name="preprocess">Xml文件的预处理操作</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function LoadXml(Of T)(XmlFile As String,
                                              Optional encoding As Encoding = Nothing,
                                              Optional ThrowEx As Boolean = True,
                                              Optional preprocess As Func(Of String, String) = Nothing) As T
        If encoding Is Nothing Then encoding = Encoding.Default

        If (Not XmlFile.FileExists) OrElse FileIO.FileSystem.GetFileInfo(XmlFile).Length = 0 Then
            Dim exMsg As String =
                $"{XmlFile.ToFileURL} is not exists on your file system or it is ZERO length content!"
            Dim ex As New Exception(exMsg)
            Call App.LogException(ex)
            If ThrowEx Then
                Throw ex
            Else
                Return Nothing
            End If
        End If

        Dim XmlDoc As String = IO.File.ReadAllText(XmlFile, encoding)

        If Not preprocess Is Nothing Then
            XmlDoc = preprocess(XmlDoc)
        End If

        Using Stream As New StringReader(s:=XmlDoc)
            Try
                Dim Type = GetType(T)
                Dim Data = New XmlSerializer(Type).Deserialize(Stream)
                Return DirectCast(Data, T)
            Catch ex As Exception
                ex = New Exception(XmlFile.ToFileURL, ex)
                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
                If ThrowEx Then
                    Throw ex
                Else
                    Return Nothing
                End If
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Serialization the target object type into a XML document.(将一个类对象序列化为XML文档)
    ''' </summary>
    ''' <typeparam name="T">The type of the target object data should be a class object.(目标对象类型必须为一个Class)</typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetXml(Of T As Class)(obj As T, Optional ThrowEx As Boolean = True) As String
        Dim sBuilder As StringBuilder = New StringBuilder(1024)

        Using StreamWriter As StringWriter = New StringWriter(sb:=sBuilder)
            Try
                Call (New XmlSerializer(GetType(T))).Serialize(StreamWriter, obj)
            Catch ex As Exception
                Call App.LogException(ex)

                If ThrowEx Then
                    Throw ex
                Else
                    Return Nothing
                End If
            End Try
            Return sBuilder.ToString
        End Using
    End Function

    ''' <summary>
    ''' Save the object as the XML document.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="saveXml"></param>
    ''' <param name="throwEx"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension> Public Function SaveAsXml(Of T As Class)(obj As T, saveXml As String,
                                                         Optional throwEx As Boolean = True,
                                                         Optional encoding As Encoding = Nothing,
                                                         <CallerMemberName> Optional caller As String = "") As Boolean
        Dim xmlDoc As String = obj.GetXml(throwEx)
        Try
            Return xmlDoc.SaveTo(saveXml, encoding)
        Catch ex As Exception
            ex = New Exception(caller, ex)
            If throwEx Then
                Throw ex
            Else
                Call App.LogException(ex)
                Call ex.PrintException
                Return False
            End If
        End Try
    End Function

    <ExportAPI("Xml.GetAttribute")>
    <Extension> Public Function GetXmlAttrValue(strData As String, Name As String) As String
        Dim m As Match = Regex.Match(strData, Name & "=(("".+?"")|[^ ]*)")
        If Not m.Success Then Return ""

        strData = m.Value.Replace(Name & "=", "")
        If strData.First = """"c AndAlso strData.Last = """"c Then
            strData = Mid(strData, 2, Len(strData) - 2)
        End If
        Return strData
    End Function

    ''' <summary>
    ''' Generate a specific type object from a xml document stream.(使用一个XML文本内容创建一个XML映射对象)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Xml">This parameter value is the document text of the xml file, not the file path of the xml file.(是Xml文件的文件内容而非文件路径)</param>
    ''' <param name="ThrowEx">Should this program throw the exception when the xml deserialization error happens? 
    ''' if False then this function will returns a null value instead of throw exception.
    ''' (在进行Xml反序列化的时候是否抛出错误，默认抛出错误，否则返回一个空对象)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateObjectFromXml(Of T As Class)(Xml As String, Optional ThrowEx As Boolean = True) As T
        Using Stream As New StringReader(s:=Xml)
            Try
                Dim Type = GetType(T)
                Dim Data = New XmlSerializer(Type).Deserialize(Stream)
                Return DirectCast(Data, T)
            Catch ex As Exception
                Call App.LogException(New Exception(Xml, ex), MethodBase.GetCurrentMethod.GetFullName)
                If ThrowEx Then
                    Throw
                Else
                    Return Nothing
                End If
            End Try
        End Using
    End Function

    <ExportAPI("Xml.CreateObject")>
    <Extension> Public Function CreateObjectFromXml(Xml As StringBuilder, typeInfo As Type) As Object
        Dim doc As String = Xml.ToString

        Using Stream As New StringReader(doc)
            Try
                Dim obj As Object = New XmlSerializer(typeInfo).Deserialize(Stream)
                Return obj
            Catch ex As Exception
                ex = New Exception(doc, ex)
                ex = New Exception(typeInfo.FullName, ex)

                Call App.LogException(ex)

                Throw ex
            End Try
        End Using
    End Function

    ''' <summary>
    ''' 使用一个XML文本内容的一个片段创建一个XML映射对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Xml">是Xml文件的文件内容而非文件路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateObjectFromXmlFragment(Of T As Class)(Xml As String) As T
        Using Stream As New StringReader(s:="<?xml version=""1.0""?>" & vbCrLf & Xml)
            Return DirectCast(New XmlSerializer(GetType(T)).Deserialize(Stream), T)
        End Using
    End Function
End Module
