﻿Imports System.Text
Imports System.Reflection
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace CommandLine.Reflection

    Public Class RunDllEntryPoint : Inherits [Namespace]

        Sub New(Name As String)
            Call MyBase.New(Name, "")
        End Sub
    End Class

    ''' <summary>
    ''' (<see cref="Microsoft.VisualBasic.CommandLine.Interpreter">CommandLine interpreter</see> executation Entry and the ShellScript software packages namespace.)这是一个命令行解释器所使用的执行入口点的集合
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class [Namespace] : Inherits Attribute

        ''' <summary>
        ''' A brief description text about the function of this namespace.(关于本模块之中的描述性的摘要文本)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description As String

        ''' <summary>
        ''' The name value of this namespace module.(本命名空间模块的名称值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute>
        Public Property [Namespace] As String

        Dim _TypeAutoExtract As Boolean

        <XmlIgnore> Public Property AutoExtract As Boolean
            Get
                Return _TypeAutoExtract
            End Get
            Protected Set(value As Boolean)
                _TypeAutoExtract = value
            End Set
        End Property

        ''' <summary>
        ''' The name value of this namespace module.(本命名空间模块的名称值)
        ''' </summary>
        ''' <param name="Namespace">The name value of this namespace module.(本命名空间模块的名称值)</param>
        ''' <remarks></remarks>
        Sub New([Namespace] As String, Optional Description As String = "")
            Me._Namespace = [Namespace]
            Me._Description = Description
            Me._TypeAutoExtract = False
        End Sub

        Protected Sub New()
        End Sub

        Friend Sub New([Namespace] As String, Description As String, auto As Boolean)
            Call Me.New([Namespace], Description)
            Me.AutoExtract = auto
        End Sub

        Public Shared ReadOnly Property TypeInfo As System.Type = GetType([Namespace])

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Description) Then
                Return String.Format("Namespace {0}", _Namespace)
            Else
                Return String.Format("Namespace {0} ({1})", _Namespace, Description)
            End If
        End Function

        ''' <summary>
        ''' 从目标类型之中构造出一个命令行解释器
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateInstance(Type As System.Type) As Microsoft.VisualBasic.CommandLine.Interpreter
            Return New Microsoft.VisualBasic.CommandLine.Interpreter(Type)
        End Function
    End Class

    ''' <summary>
    ''' Optional commandline arguments.(本属性标记一个命令行字符串之中的可选参数)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public Class [Optional] : Inherits Attribute

        ''' <summary>
        ''' The data type enumeration of the target optional parameter switch.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum [Types]
            ''' <summary>
            ''' String
            ''' </summary>
            ''' <remarks></remarks>
            [String]
            ''' <summary>
            ''' Int
            ''' </summary>
            ''' <remarks></remarks>
            [Integer]
            ''' <summary>
            ''' Real
            ''' </summary>
            ''' <remarks></remarks>
            [Double]
            [Boolean]
            ''' <summary>
            ''' File path, is equals most string
            ''' </summary>
            File
        End Enum

        Dim _name As String, _type As Types

        Public Property Name As String
            Get
                Return _name
            End Get
            Protected Set(value As String)
                _name = value
            End Set
        End Property
        Public Property Type As Types
            Get
                Return _type
            End Get
            Protected Set(value As Types)
                _type = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">The name value of the target parameter switch which will be marked as an optional parameter.
        ''' (目标将要被标记为可选参数的命令行参数开关对象)</param>
        ''' <param name="Type">The data type of the target command line parameter switch, default type is string type.</param>
        ''' <remarks></remarks>
        Public Sub New(Name As String, Optional Type As Types = Types.String)
            Me.Name = Name
            Me.Type = Type
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}", Type.ToString, Name)
        End Function
    End Class

    ''' <summary>
    ''' Use for the detail description for a specific commandline switch.(用于对某一个命令的开关参数的具体描述帮助信息)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class ParameterInfo : Inherits Attribute

        ''' <summary>
        ''' The name of this command line parameter switch.(该命令开关的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String

        Dim _Description As String

        ''' <summary>
        ''' The description and brief help information about this parameter switch, 
        ''' you can using the \n escape string to gets a VbCrLf value.
        ''' (对这个开关参数的具体的描述以及帮助信息，可以使用\n转义字符进行换行)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description As String
            Get
                Return _Description
            End Get
            Set(value As String)
                Dim Tokens As String() = Strings.Split(value, "\n")
                Dim sBuilder As StringBuilder = New StringBuilder(Tokens.First & vbCrLf)
                For i As Integer = 1 To Tokens.Length - 1
                    Call sBuilder.AppendLine("              " & Tokens(i))
                Next

                _Description = sBuilder.ToString
            End Set
        End Property

        ''' <summary>
        ''' The usage example of this parameter switch.(该开关的值的示例)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Example As String
        ''' <summary>
        ''' The usage syntax information about this parameter switch.(本开关参数的使用语法)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Usage As String

        ''' <summary>
        ''' Is this parameter switch is an optional value.(本开关是否为可选的参数)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property [Optional] As Boolean

        ''' <summary>
        ''' 对命令行之中的某一个参数进行描述性信息的创建，包括用法和含义
        ''' </summary>
        ''' <param name="Name">The name of this command line parameter switch.(该命令开关的名称)</param>
        ''' <param name="Optional">Is this parameter switch is an optional value.(本开关是否为可选的参数)</param>
        ''' <remarks></remarks>
        Sub New(Name As String, Optional [Optional] As Boolean = False)
            _Name = Name
            _Optional = [Optional]
        End Sub

        Public Overrides Function ToString() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            If [Optional] Then
                sBuilder.AppendLine(String.Format("   [{0}]", Name))
            Else
                sBuilder.AppendLine(Name)
            End If
            sBuilder.AppendLine(String.Format("    Description:  {0}", Description))
            sBuilder.AppendLine(String.Format("    Example:      {0} ""{1}""", Name, Example))

            Return sBuilder.ToString
        End Function
    End Class
End Namespace