﻿Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.Runtime.CompilerServices
Imports System.Text

<Extension> Public Module CodeDOMExtension

    ''' <summary>
    ''' 设置所编译的应用程序的图标
    ''' </summary>
    ''' <param name="iconPath"></param>
    ''' <returns></returns>
    Public Function Icon(iconPath As String) As String
        Return $"/target:winexe /win32icon:""{iconPath}"""
    End Function

    <Extension> Public Function ImportsNamespace(refList As String()) As CodeDom.CodeNamespaceImport()
        Dim nsArray As CodeDom.CodeNamespaceImport() = New CodeDom.CodeNamespaceImport() {
                New CodeDom.CodeNamespaceImport("Microsoft.VisualBasic"),
                New CodeDom.CodeNamespaceImport("System"),
                New CodeDom.CodeNamespaceImport("System.Collections"),
                New CodeDom.CodeNamespaceImport("System.Collections.Generic"),
                New CodeDom.CodeNamespaceImport("System.Data"),
                New CodeDom.CodeNamespaceImport("System.Diagnostics"),
                New CodeDom.CodeNamespaceImport("System.Linq"),
                New CodeDom.CodeNamespaceImport("System.Xml.Linq"),
                New CodeDom.CodeNamespaceImport("System.Text.RegularExpressions")}
        Return nsArray
    End Function

    Public Function [GetType]() As CodeTypeReference
        Throw New NotImplementedException()
    End Function

    ''' <summary>
    ''' Generate the source code from the CodeDOM object model.(根据对象模型生成源代码以方便调试程序)
    ''' </summary>
    ''' <param name="NameSpace"></param>
    ''' <param name="CodeStyle">VisualBasic, C#</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' You can easily convert the source code between VisualBasic and C# using this function just by makes change in statement: 
    ''' CodeDomProvider.GetCompilerInfo("VisualBasic").CreateProvider().GenerateCodeFromNamespace([NameSpace], sWriter, Options)
    ''' Modify the VisualBasic in to C#
    ''' </remarks>
    <Extension> Public Function GenerateCode([NameSpace] As CodeDom.CodeNamespace, Optional CodeStyle As String = "VisualBasic") As String
        Dim sBuilder As StringBuilder = New StringBuilder()

        Using sWriter As IO.StringWriter = New System.IO.StringWriter(sBuilder)
            Dim Options As New CodeGeneratorOptions() With {
                .IndentString = "  ",
                .ElseOnClosing = True,
                .BlankLinesBetweenMembers = True
            }
            CodeDomProvider.GetCompilerInfo(CodeStyle).CreateProvider().GenerateCodeFromNamespace([NameSpace], sWriter, Options)
            Return sBuilder.ToString()
        End Using
    End Function

    ''' <summary>
    ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
    ''' </summary>
    ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
    ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
    ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
    ''' <param name="CodeStyle">VisualBasic, C#</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Compile(ObjectModel As CodeDom.CodeNamespace,
                                        Reference As String(),
                                        DotNETReferenceAssembliesDir As String,
                                        Optional CodeStyle As String = "VisualBasic") As System.Reflection.Assembly
        Dim assembly = New CodeDom.CodeCompileUnit
        Call assembly.Namespaces.Add(ObjectModel)
        Return Compile(assembly, Reference, DotNETReferenceAssembliesDir, CodeStyle)
    End Function

    ''' <summary>
    ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
    ''' </summary>
    ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
    ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
    ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
    ''' <param name="CodeStyle">VisualBasic, C#</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Compile(ObjectModel As CodeDom.CodeNamespace,
                                        Reference As String(),
                                        DotNETReferenceAssembliesDir As String,
                                        Options As CodeDom.Compiler.CompilerParameters,
                                        Optional CodeStyle As String = "VisualBasic") As System.Reflection.Assembly
        Dim assembly = New CodeDom.CodeCompileUnit
        Call assembly.Namespaces.Add(ObjectModel)
        Return Compile(assembly, Reference, DotNETReferenceAssembliesDir, Options, CodeStyle)
    End Function

    ''' <summary>
    ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
    ''' </summary>
    ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
    ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
    ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
    ''' <param name="CodeStyle">VisualBasic, C#</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Compile(ObjectModel As CodeDom.CodeCompileUnit,
                                        Reference As String(),
                                        DotNETReferenceAssembliesDir As String,
                                        Optional CodeStyle As String = "VisualBasic") As System.Reflection.Assembly

        Dim Options As CodeDom.Compiler.CompilerParameters = New CodeDom.Compiler.CompilerParameters
        Options.GenerateInMemory = True
        Options.IncludeDebugInformation = False
        Options.GenerateExecutable = False

        Return Compile(ObjectModel, Reference, DotNETReferenceAssembliesDir, Options, CodeStyle)
    End Function

    ''' <summary>
    ''' .exe的编译配置文件
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ExecutableProfile As CodeDom.Compiler.CompilerParameters
        Get
            Dim Options As CodeDom.Compiler.CompilerParameters = New CodeDom.Compiler.CompilerParameters
            Options.GenerateInMemory = False
            Options.IncludeDebugInformation = True
            Options.GenerateExecutable = True

            Return Options
        End Get
    End Property

    ''' <summary>
    ''' .Dll的编译配置文件
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DllProfile As CodeDom.Compiler.CompilerParameters
        Get
            Dim Options As CodeDom.Compiler.CompilerParameters = New CodeDom.Compiler.CompilerParameters
            Options.GenerateInMemory = False
            Options.IncludeDebugInformation = True
            Options.GenerateExecutable = False
            Return Options
        End Get
    End Property

    ''' <summary>
    ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
    ''' </summary>
    ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
    ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
    ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
    ''' <param name="CodeStyle">VisualBasic, C#</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Compile(ObjectModel As CodeDom.CodeCompileUnit,
                                        Reference As String(),
                                        DotNETReferenceAssembliesDir As String,
                                        Options As CodeDom.Compiler.CompilerParameters,
                                        Optional CodeStyle As String = "VisualBasic") As System.Reflection.Assembly

        Dim CodeDomProvider As CodeDom.Compiler.CodeDomProvider = CodeDom.Compiler.CodeDomProvider.CreateProvider(CodeStyle)

        If Not Reference.IsNullOrEmpty Then
            Call Options.ReferencedAssemblies.AddRange((From path As String In Reference
                                                        Where Array.IndexOf(DotNETFramework, IO.Path.GetFileNameWithoutExtension(path)) = -1
                                                        Select path).ToArray)
        End If

        Call Options.ReferencedAssemblies.AddRange(New String() {
               DotNETReferenceAssembliesDir & "\System.dll",
               DotNETReferenceAssembliesDir & "\System.Core.dll",
               DotNETReferenceAssembliesDir & "\System.Data.dll",
               DotNETReferenceAssembliesDir & "\System.Data.DataSetExtensions.dll",
               DotNETReferenceAssembliesDir & "\System.Xml.dll",
               DotNETReferenceAssembliesDir & "\System.Xml.Linq.dll"})

        Dim Compiled = CodeDomProvider.CompileAssemblyFromDom(Options, ObjectModel)

        Call GetDebugInformation(Compiled, ObjectModel).__DEBUG_ECHO

        Return Compiled.CompiledAssembly
    End Function

    ''' <summary>
    ''' 基本的引用集合
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DotNETFramework As String() = {
        "System",
        "System.Core",
        "System.Data",
        "System.Data.DataSetExtensions",
        "System.Xml",
        "System.Xml.Linq"
    }

    <Extension> Public Function GetDebugInformation(CompiledResult As CodeDom.Compiler.CompilerResults,
                                                    Assembly As CodeDom.CodeCompileUnit) As String
        Dim sBuilder As StringBuilder = New StringBuilder
        Call sBuilder.AppendLine(GenerateCode(Assembly.Namespaces.Item(0)) & vbCrLf)

        sBuilder.AppendLine("Error Information: ")
        For Each [Error] In CompiledResult.Errors
            sBuilder.AppendLine([Error].ToString)
        Next
        sBuilder.AppendLine(vbCrLf & "Compiler Output:")
        For Each Line As String In CompiledResult.Output
            sBuilder.AppendLine(Line)
        Next

        Call sBuilder.ToString.SaveTo(".\CodeDom.log")

        Return sBuilder.ToString
    End Function
End Module