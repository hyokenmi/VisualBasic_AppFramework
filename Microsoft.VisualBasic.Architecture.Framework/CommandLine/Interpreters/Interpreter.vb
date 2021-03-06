﻿Imports Microsoft.VisualBasic.ConsoleDevice.STDIO

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Reflection

Imports System.Windows.Forms

#Const NET_45 = 0

Namespace CommandLine

    ''' <summary>
    ''' Command line interpreter for your cli program.(命令行解释器，请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <[Namespace]("Interpreter")>
    Public Class Interpreter

        Implements System.IDisposable
        Implements System.Collections.Generic.IDictionary(Of String, EntryPoints.APIEntryPoint)

        ''' <summary>
        ''' 在添加之前请确保键名是小写的字符串
        ''' </summary>
        Protected _CommandInfoHash As Dictionary(Of String, EntryPoints.APIEntryPoint) =
            New Dictionary(Of String, EntryPoints.APIEntryPoint)
        Protected _nsRoot As String

        ''' <summary>
        ''' 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function __ExecuteFile(path As String, args As CommandLine) As Integer
        ''' <summary>
        ''' 假若所传入的命令行是空的，就会执行这个函数指针
        ''' </summary>
        ''' <returns></returns>
        Public Delegate Function __ExecuteEmptyCli() As Integer

        ''' <summary>
        ''' Public Delegate Function __ExecuteFile(path As String, args As String()) As Integer, 
        ''' (<seealso cref="__executefile"/>: 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针)
        ''' 这个函数指针一般是用作于执行脚本程序的
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteFile As __ExecuteFile
        ''' <summary>
        ''' Public Delegate Function __ExecuteEmptyCli() As Integer,
        ''' (<seealso cref="__ExecuteEmptyCli"/>: 假若所传入的命令行是空的，就会执行这个函数指针)
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteEmptyCli As __ExecuteEmptyCli

        ''' <summary>
        ''' Gets the dictionary data which contains all of the available command information in this assembly module.
        ''' (获取从本模块之中获取得到的所有的命令行信息) 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToDictionary() As Dictionary(Of String, EntryPoints.APIEntryPoint)
            Return _CommandInfoHash
        End Function

        Public Overrides Function ToString() As String
            Return "Cli://" & _nsRoot
        End Function

        ''' <summary>
        ''' Execute the specific command line using this interpreter.
        ''' </summary>
        ''' <param name="args">The user input command line string.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Execute(args As CommandLine) As Integer
            If Not args.IsNullOrEmpty Then
                Dim i As Integer = __methodInvoke(args.Name.ToLower, {args}, args.Parameters)
#If DEBUG Then
                Call Pause()
#End If
                Return i
            Else
                Return __executeEmpty() ' 命令行是空的
            End If
        End Function

        ''' <summary>
        ''' 命令行是空的
        ''' </summary>
        ''' <returns></returns>
        Private Function __executeEmpty() As Integer
            If Not ExecuteEmptyCli Is Nothing Then
                Return _ExecuteEmptyCli()
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 所有的命令行都从这里开始执行
        ''' </summary>
        ''' <param name="commandName"></param>
        ''' <param name="argvs">就只有一个命令行对象</param>
        ''' <param name="help_argvs"></param>
        ''' <returns></returns>
        Private Function __methodInvoke(commandName As String, argvs As Object(), help_argvs As String()) As Integer

            If _CommandInfoHash.ContainsKey(commandName) Then _
                Return _CommandInfoHash(commandName).Execute(argvs)

            If String.Equals(commandName, "?") Then
                If help_argvs.IsNullOrEmpty Then
                    Return Help("")
                Else
                    Return Help(help_argvs.First)
                End If

            ElseIf String.Equals(commandName, "man") Then
                Dim sdk As String = SDKdocs()
                Dim DocPath As String = $"{_CommandInfoHash?.FirstOrDefault.Value.EntryPoint.DeclaringType.Assembly.Location}.txt"

                Call Console.WriteLine(sdk)
                Call FileIO.FileSystem.WriteAllText(DocPath, sdk, append:=False)
                Return 0

            Else
                If commandName.FileExists AndAlso Not Me.ExecuteFile Is Nothing Then  '命令行的名称和上面的都不符合，但是可以在文件系统之中找得到一个相应的文件，则执行文件句柄
                    Return ExecuteFile()(path:=commandName, args:=DirectCast(argvs(Scan0), CommandLine))
                Else
                    Dim lst As String() = Me.ListPossible(commandName)

                    If lst.IsNullOrEmpty Then
                        Console.WriteLine(BAD_COMMAND_NAME, commandName)
                    Else
                        Call Console.WriteLine($"Bad command, no such a command named ""{commandName}"", but you probably want to use commands:")
                        For Each name As String In lst
                            Call Console.WriteLine("    " & name)
                        Next
                    End If
                End If
            End If

            Return -1
        End Function

        Const BAD_COMMAND_NAME As String = "Bad command, no such a command named ""{0}"", ? for command list or ""man"" for all of the commandline detail informations."

        ''' <summary>
        ''' Generate the sdk document for the target program assembly.(生成目标应用程序的命令行帮助文档)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SDKdocs() As String
            Dim sBuilder As StringBuilder = New StringBuilder($"{Application.ProductName} [version {Application.ProductVersion}]")
            Dim Index As Integer = 1

            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine($"Module AssemblyName: {_CommandInfoHash?.FirstOrDefault.Value.EntryPoint.DeclaringType.Assembly.Location.ToFileURL}")
            Call sBuilder.AppendLine("Root namespace: " & Me._nsRoot)
            Call sBuilder.AppendLine(vbCrLf & vbCrLf & HelpSummary())
            Call sBuilder.AppendLine("Commands")
            Call sBuilder.AppendLine("--------------------------------------------------------------------------------")

            For Each CmdlEntry As Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint In _CommandInfoHash.Values
                sBuilder.AppendLine(Index & ".  " & CmdlEntry.HelpInformation)
                Index += 1
            Next

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' Process the command option arguments of the main function:
        ''' <code>Public Function Main(argvs As String()) As Integer
        ''' </code>
        ''' </summary>
        ''' <param name="CommandLineArgs">The cli command line parameter string value collection.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Execute(CommandLineArgs As String()) As Integer
            Dim CommandName As String = CommandLineArgs.First
            Dim argvs As String() = CommandLineArgs.Skip(1).ToArray
            Dim i As Integer = __methodInvoke(CommandName, argvs, help_argvs:=argvs)
#If DEBUG Then
            Call Pause()
#End If
            Return i
        End Function

        Public Function Execute(CommandName As String, args As String()) As Integer
            Return __methodInvoke(CommandName.ToLower, args, help_argvs:=args)
        End Function

        ''' <summary>
        ''' Add a command in current cli interpreter.(x向当前的这个CLI命令行解释器之中添加一个命令)
        ''' </summary>
        ''' <param name="Command"></param>
        ''' <remarks></remarks>
        Public Sub AddCommand(Command As EntryPoints.APIEntryPoint)
            Dim NameId As String = Command.Name.ToLower

            If Not _CommandInfoHash.ContainsKey(NameId) Then
                Call _CommandInfoHash.Add(NameId, Command)
            End If
        End Sub

        ''' <summary>
        ''' Gets the help information of a specific command using its name property value.(获取某一个命令的帮助信息)
        ''' </summary>
        ''' <param name="CommandName">If the paramteer command name value is a empty string then this function 
        ''' will list all of the commands' help information.(假若本参数为空则函数会列出所有的命令的帮助信息)</param>
        ''' <returns>Error code, ZERO for no error</returns>
        ''' <remarks></remarks>
        <ExportAPI("?", Usage:="? [CommandName]", Info:="Show Application help", Example:="? example_commandName")>
        Public Function Help(CommandName As String) As Integer
            If String.IsNullOrEmpty(CommandName) Then 'List all commands.
                Call Console.WriteLine(HelpSummary)
            Else
                If _CommandInfoHash.ContainsKey(CommandName.ToLower.ShadowCopy(CommandName)) Then
                    Dim CommandInfo = _CommandInfoHash(CommandName)
                    Console.WriteLine(vbCrLf & CommandInfo.HelpInformation)
                Else
                    Dim lst As String() = Me.ListPossible(CommandName)

                    If lst.IsNullOrEmpty Then
                        Call Console.WriteLine($"Bad command, no such a command named ""{CommandName}"", ? for command list.")
                    Else
                        Call Console.WriteLine($"Bad command, no such a command named ""{CommandName}"", but you probably want to find commands:")
                        For Each name As String In lst
                            Call Console.WriteLine("    " & name)
                        Next
                    End If

                    Return -2
                End If
            End If

            Return 0
        End Function

        ''' <summary>
        ''' Returns the summary brief help information of all of the commands in current cli interpreter.
        ''' (枚举出本CLI解释器之中的所有的命令的帮助的摘要信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function HelpSummary() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Dim NameMaxLen As Integer = (From commandInfo As EntryPoints.APIEntryPoint
                                         In _CommandInfoHash.Values
                                         Select Len(commandInfo.Name)).ToArray.Max

            Call sBuilder.AppendLine("All of the command that available in this program has been list below:")
            Call sBuilder.AppendLine()

            For Each commandInfo As EntryPoints.APIEntryPoint In _CommandInfoHash.Values
                Call sBuilder.AppendLine(String.Format(" {0}:  {1}{2}",
                                                       commandInfo.Name,
                                                       New String(c:=" "c, count:=NameMaxLen - Len(commandInfo.Name)),
                                                       commandInfo.Info))
            Next

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' Returns the command entry info list array.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListCommandInfo As EntryPoints.APIEntryPoint()
            Get
                Return _CommandInfoHash.Values.ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Type">A module or a class which contains some shared method for the command entry.
        ''' (包含有若干使用<see cref="Reflection.ExportAPIAttribute"></see>进行标记的命令行执行入口点的Module或者Class对象类型，
        ''' 可以使用 Object.GetType/GetType 关键词操作来获取所需要的类型信息)</param>
        ''' <remarks></remarks>
        Sub New(Type As System.Type)
            For Each CommandInfo As EntryPoints.APIEntryPoint In __getsAllCommands(Type, False)
                Call _CommandInfoHash.Add(CommandInfo.Name.ToLower, CommandInfo)
            Next
            Me._nsRoot = Type.Namespace
        End Sub

        ''' <summary>
        ''' 导出所有符合条件的静态方法
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Protected Overridable Function __getsAllCommands(Type As System.Type, Optional [Throw] As Boolean = True) As List(Of EntryPoints.APIEntryPoint)
            Return GetAllCommands(Type, [Throw])
        End Function

        ''' <summary>
        ''' 导出所有符合条件的静态方法，请注意，在这里已经将外部的属性标记和所属的函数的入口点进行连接了
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Public Shared Function GetAllCommands(Type As System.Type, Optional [Throw] As Boolean = True) As List(Of EntryPoints.APIEntryPoint)
            If Type Is Nothing Then
                Return New List(Of EntryPoints.APIEntryPoint)
            End If

            Dim Methods = Type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
            Dim commandAttribute As System.Type = GetType(ExportAPIAttribute)
            Dim commandsInfo = From methodInfo As MethodInfo In Methods
                               Let commandInfo As EntryPoints.APIEntryPoint = __getsAPI(methodInfo, commandAttribute, [Throw])
                               Where Not commandInfo Is Nothing
                               Select commandInfo
                               Order By commandInfo.Name Ascending   '
            Return commandsInfo.ToList
        End Function

        Private Shared Function __getsAPI(methodInfo As MethodInfo, commandAttribute As System.Type, [throw] As Boolean) As EntryPoints.APIEntryPoint
            Try
                Dim attributes As Object() = methodInfo.GetCustomAttributes(commandAttribute, False)

                If attributes.IsNullOrEmpty Then Return Nothing

                Dim cmdAttr As ExportAPIAttribute = DirectCast(attributes(0), ExportAPIAttribute)
                Dim commandInfo As New EntryPoints.APIEntryPoint(cmdAttr, methodInfo, [throw]) '在这里将外部的属性标记和所属的函数的入口点进行连接

                Return commandInfo
            Catch ex As Exception
                Call App.LogException(New Exception(methodInfo.FullName(True), ex))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Create an empty cli command line interpreter object which contains no commands entry.(创建一个没有包含有任何命令入口点的空的CLI命令行解释器)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateEmptyCLIObject() As Interpreter
            Return New Interpreter(GetType(Interpreter))
        End Function

        ''' <summary>
        ''' Create a new interpreter instance from a specific type information.(从目标类型之中构造出一个命令行解释器)
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("CreateObject")>
        Public Shared Function CreateInstance(Type As System.Type) As Microsoft.VisualBasic.CommandLine.Interpreter
            Return New Microsoft.VisualBasic.CommandLine.Interpreter(Type)
        End Function

        ''' <summary>
        ''' Create a new interpreter instance using the specific type information.(使用所制定的目标类型信息构造出一个CLI命令行解释器)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateInstance(Of T As Class)() As Microsoft.VisualBasic.CommandLine.Interpreter
            Return New Microsoft.VisualBasic.CommandLine.Interpreter(Type:=GetType(Type))
        End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' Create a new interpreter instance from a specific dll/exe path, this program assembly file should be a standard .NET assembly.
        ''' (从一个标准的.NET程序文件之中构建出一个命令行解释器)
        ''' </summary>
        ''' <param name="assmPath">DLL/EXE file path.(标准的.NET程序集文件的文件路径)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("rundll")>
        Public Shared Function CreateInstance(assmPath As String) As Microsoft.VisualBasic.CommandLine.Interpreter
            Dim AssemblyType As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(assmPath)
            Dim EntryType As System.Type = GetType(Microsoft.VisualBasic.CommandLine.Reflection.[Namespace])
            Dim LQuery = From [Module] As System.Reflection.TypeInfo
                         In AssemblyType.DefinedTypes
                         Let attributes As Object() = [Module].GetCustomAttributes(EntryType, inherit:=False)
                         Where Not attributes Is Nothing AndAlso attributes.Length = 1
                         Select [Module] '

            If LQuery.Count > 0 Then
                Dim Type As System.Type = LQuery.First.GetType
                Return New Microsoft.VisualBasic.CommandLine.Interpreter(Type)
            Else  '没有找到执行入口点
                Return Nothing
            End If
        End Function
#End If

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#Region "Implements System.Collections.Generic.IReadOnlyDictionary(Of String, CommandInfo)"

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)) Implements IEnumerable(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).GetEnumerator
            For Each key As String In Me._CommandInfoHash.Keys
                Yield New KeyValuePair(Of String, EntryPoints.APIEntryPoint)(key, Me._CommandInfoHash(key))
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield Me.GetEnumerator
        End Function

        Public Sub Add(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Add
            Call _CommandInfoHash.Add(item.Key, item.Value)
        End Sub

        ''' <summary>
        ''' Clear the hash table of the cli command line interpreter command entry points.(清除本CLI解释器之中的所有的命令行执行入口点的哈希数据信息) 
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Clear
            Call _CommandInfoHash.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Contains
            Return _CommandInfoHash.Contains(item)
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of String, EntryPoints.APIEntryPoint), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).CopyTo
            Call _CommandInfoHash.ToArray.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets the command counts in current cli interpreter.(返回本CLI命令行解释器之中所包含有的命令的数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Count
            Get
                Return Me._CommandInfoHash.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Remove
            Return _CommandInfoHash.Remove(item.Key)
        End Function

        Public Sub Add(key As String, value As EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Add
            Call _CommandInfoHash.Add(key, value)
        End Sub

        ''' <summary>
        ''' The target command line command is exists in this cli interpreter using it name property?(判断目标命令行命令是否存在于本CLI命令行解释器之中)
        ''' </summary>
        ''' <param name="CommandName">The command name value is not case sensitive.(命令的名称对大小写不敏感的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExistsCommand(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).ContainsKey
            Return Me._CommandInfoHash.ContainsKey(CommandName.ToLower)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key">调用前需要转换为小写字母的形式</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(key As String) As EntryPoints.APIEntryPoint Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Item
            Get
                Return Me._CommandInfoHash(key)
            End Get
            Set(value As EntryPoints.APIEntryPoint)
                'DO NOTHING
            End Set
        End Property

        Public Function GetPossibleCommand(name As String) As EntryPoints.APIEntryPoint
            If Me._CommandInfoHash.ContainsKey(name.ToLower.ShadowCopy(name)) Then
                Return _CommandInfoHash(name)
            Else
                Dim LQuery = (From x As KeyValuePair(Of String, EntryPoints.APIEntryPoint)
                              In _CommandInfoHash
                              Let similarity = LevenshteinDistance.ComputeDistance(x.Key, name)
                              Where Not similarity Is Nothing
                              Select similarity.Score, x.Value
                              Order By Score Descending).ToArray
                If LQuery.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return LQuery.First.Value
                End If
            End If
        End Function

        ''' <summary>
        ''' 列举出所有可能的命令
        ''' </summary>
        ''' <param name="Name">模糊匹配</param>
        ''' <returns></returns>
        Public Function ListPossible(Name As String) As String()
            Name = Name.ToLower
            Dim LQuery = (From x As String In _CommandInfoHash.Keys.AsParallel
                          Let lev = LevenshteinDistance.ComputeDistance(x, Name)
                          Where Not lev Is Nothing AndAlso
                              lev.Score > 0.3
                          Select lev.Score, x
                          Order By Score Descending).ToArray
            Return LQuery.ToArray(Function(x) x.x)
        End Function

        ''' <summary>
        ''' List all of the command line entry point name which were contains in this cli interpreter.
        ''' (列举出本CLI命令行解释器之中的所有的命令行执行入口点的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListCommandsEntryName As ICollection(Of String) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Keys
            Get
                Return Me._CommandInfoHash.Keys
            End Get
        End Property

        Public Function Remove(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Remove
            Return _CommandInfoHash.Remove(CommandName)
        End Function

        Public Function TryGetValue(key As String, ByRef value As EntryPoints.APIEntryPoint) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).TryGetValue
            Return Me._CommandInfoHash.TryGetValue(key, value)
        End Function

        Public ReadOnly Property Values As ICollection(Of EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Values
            Get
                Return Me._CommandInfoHash.Values
            End Get
        End Property
#End Region
    End Class
End Namespace