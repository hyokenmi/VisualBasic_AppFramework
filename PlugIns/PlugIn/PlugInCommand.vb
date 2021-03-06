﻿''' <summary>
''' Function Main(Target As Form) As Object.(应用于目标模块中的一个函数的自定义属性，相对应于菜单中的一个项目)
''' </summary>
''' <remarks></remarks>
<AttributeUsage(AttributeTargets.Method, allowmultiple:=False, inherited:=True)>
Public Class PlugInCommand : Inherits CommandBase

    ''' <summary>
    ''' The menu path for this plugin command.(这个插件命令的菜单路径)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Path As String = "\"

    Dim Method As Reflection.MethodInfo

    Public Overrides Function ToString() As String
        If String.IsNullOrEmpty(Path) OrElse String.Equals("\", Path) Then
            Return String.Format("Name:={0}; Path:\\Root", Name)
        Else
            Return String.Format("Name:={0}; Path:\\{1}", Name, Path)
        End If
    End Function

    Public Function Invoke(Target As System.Windows.Forms.Form) As Object
        Return PlugInEntry.Invoke({Target}, Method)
    End Function

    Friend Function Initialize(Method As Reflection.MethodInfo) As PlugInCommand
        Me.Method = Method
        Return Me
    End Function
End Class
