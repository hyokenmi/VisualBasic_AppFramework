﻿Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Drawing
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.ComponentModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization

#If FRAMEWORD_CORE Then
Imports Microsoft.VisualBasic.CommandLine.Reflection
#End If

Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Parallel.ParallelExtension
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

#If FRAMEWORD_CORE Then

''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>
<[PackageNamespace]("Framework.Extensions",
                    Description:="The common extension methods module in this Microsoft.VisualBasic program assembly." &
                                 "Common extension methods library for convenient the programming job.",
                    Publisher:="xie.guigang@gmail.com",
                    Revision:=8655,
                    Url:="http://gcmodeller.org")>
<System.Runtime.CompilerServices.ExtensionAttribute>
Public Module Extensions
#Else

''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>  
Public Module Extensions
#End If

    <Extension>
    Public Function IndexOf(Of T)(source As Queue(Of T), x As T) As Integer
        If source.IsNullOrEmpty Then
            Return -1
        Else
            Return source.ToList.IndexOf(x)
        End If
    End Function

    <Extension> Public Function Keys(Of T1, T2)(source As IEnumerable(Of KeyValuePair(Of T1, T2))) As T1()
        Return source.ToArray(Function(x) x.Key)
    End Function

    ''' <summary>
    ''' 性能测试工具
    ''' </summary>
    ''' <param name="work">需要测试性能的工作对象</param>
    ''' <returns></returns>
    Public Function Time(work As Action) As Long
        Dim sw As Stopwatch = Stopwatch.StartNew
        Call work()
        Call $"Work takes {sw.ElapsedMilliseconds}ms...".__DEBUG_ECHO
        Return sw.ElapsedMilliseconds
    End Function

    Public Function Time(Of T)(work As Func(Of T)) As T
        Dim sw As Stopwatch = Stopwatch.StartNew
        Dim value As T = work()
        Call $"Work takes {sw.ElapsedMilliseconds}ms...".__DEBUG_ECHO
        Return value
    End Function

    Public Delegate Function WaitHandle() As Boolean

    ''' <summary>
    ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
    ''' </summary>
    ''' <param name="handle"></param>
    <Extension> Public Sub Wait(handle As Func(Of Boolean))
        If handle Is Nothing Then
            Return
        End If

        Do While handle() = False
            Call Threading.Thread.Sleep(10)
            Call Application.DoEvents()
        Loop
    End Sub

    ''' <summary>
    ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
    ''' </summary>
    ''' <param name="handle"></param>
    <Extension> Public Sub Wait(handle As WaitHandle)
        If handle Is Nothing Then
            Return
        End If

        Do While handle() = False
            Call Threading.Thread.Sleep(10)
            Call Application.DoEvents()
        Loop
    End Sub

    <Extension>
    Public Function Switch(Of T)(b As Boolean, [true] As T, [false] As T) As T
        Return If(b, [true], [false])
    End Function

    <Extension> Public Function FlushAllLines(Of T)(data As Generic.IEnumerable(Of T),
                                                    SaveTo As String,
                                                    Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Dim strings As String() = data.ToArray(Function(obj) Scripting.ToString(obj))

        Try
            Dim parent As String = FileIO.FileSystem.GetParentPath(SaveTo)
            Call FileIO.FileSystem.CreateDirectory(parent)
            Call IO.File.WriteAllLines(SaveTo, strings, If(encoding Is Nothing, System.Text.Encoding.Default, encoding))
        Catch ex As Exception
            Call App.LogException(New Exception(SaveTo, ex))
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Dynamics add a element into the target array.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="value"></param>
    <Extension> Public Sub Add(Of T)(ByRef array As T(), value As T)
        Dim chunkBuffer As T() = New T(array.Length) {}
        Call System.Array.ConstrainedCopy(array, Scan0, chunkBuffer, Scan0, array.Length)
        chunkBuffer(array.Length) = value
        array = chunkBuffer
    End Sub

    ''' <summary>
    ''' 会自动跳过空集合，这个方法是安全的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="value"></param>
    <Extension> Public Sub Add(Of T)(ByRef array As T(), ParamArray value As T())
        If value.IsNullOrEmpty Then
            Return
        End If
        If array Is Nothing Then
            array = New T() {}
        End If

        Dim chunkBuffer As T() = New T(array.Length + value.Length - 1) {}
        Call System.Array.ConstrainedCopy(array, Scan0, chunkBuffer, Scan0, array.Length)
        Call System.Array.ConstrainedCopy(value, Scan0, chunkBuffer, array.Length, value.Length)
        array = chunkBuffer
    End Sub

    <Extension> Public Function Append(Of T)(buffer As T(), value As Generic.IEnumerable(Of T)) As T()
        If buffer Is Nothing Then
            Return value.ToArray
        End If

        Call buffer.Add(value.ToArray)
        Return buffer
    End Function

    <Extension> Public Sub Add(Of T)(ByRef array As T(), value As List(Of T))
        Call Add(Of T)(array, value.ToArray)
    End Sub

    ''' <summary>
    ''' Adds the elements of the specified collection to the end of the System.Collections.Generic.List`1.
    ''' (会自动跳过空集合，这个方法是安全的)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="value">The collection whose elements should be added to the end of the System.Collections.Generic.List`1.</param>
    <Extension> Public Sub Add(Of T)(ByRef list As List(Of T), ParamArray value As T())
        If value.IsNullOrEmpty Then
            Return
        Else
            Call list.AddRange(value)
        End If
    End Sub

    ''' <summary>
    ''' 假若下标越界的话会返回默认值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="index"></param>
    ''' <param name="[default]"></param>
    ''' <returns></returns>
    <Extension> Public Function [Get](Of T)(array As Generic.IEnumerable(Of T), index As Integer, Optional [default] As T = Nothing) As T
        If array.IsNullOrEmpty Then
            Return [default]
        End If

        If index < 0 OrElse index >= array.Count Then
            Return [default]
        End If

        Dim value As T = array(index)
        Return value
    End Function

    ''' <summary>
    ''' This is a safely method for gets the value in a array, if the index was outside of the boundary, then the default value will be return.
    ''' (假若下标越界的话会返回默认值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="index"></param>
    ''' <param name="[default]">Default value for return when the array object is nothing or index outside of the boundary.</param>
    ''' <returns></returns>
    <Extension> Public Function [Get](Of T)(array As T(), index As Integer, Optional [default] As T = Nothing) As T
        If array.IsNullOrEmpty Then
            Return [default]
        End If

        If index < 0 OrElse index >= array.Length Then
            Return [default]
        End If

        Dim value As T = array(index)
        Return value
    End Function

    <Extension> Public Function [Set](Of T)(ByRef array As T(), index As Integer, value As T) As T()
        If index < 0 Then
            Return array
        End If

        If array.Length >= index Then
            array(index) = value
        Else
            Dim copy As T() = New T(index) {}
            Call System.Array.ConstrainedCopy(array, Scan0, copy, Scan0, array.Length)
            copy(index) = value
            array = copy
        End If

        Return array
    End Function

#Region ""

    <ExportAPI("SendMessage")>
    <Extension> Public Sub SendMessage(host As System.Net.IPEndPoint, request As String, Callback As Action(Of String))
        Dim client As New Net.AsynInvoke(host)
        Call New Threading.Thread(Sub() Callback(client.SendMessage(request))).Start()
    End Sub

    <ExportAPI("SendMessage")>
    <Extension> Public Sub SendMessage(host As Net.IPEndPoint, request As String, Callback As Action(Of String))
        Call host.GetIPEndPoint.SendMessage(request, Callback)
    End Sub

#End Region

    ''' <summary>
    ''' Function test the Boolean expression and then decided returns which part of the value.
    ''' (这个函数主要是用于Delegate函数指针类型或者Lambda表达式的)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="expr"><see cref="Boolean"/> Expression</param>
    ''' <param name="[True]">value returns this parameter if the value of the expression is True</param>
    ''' <param name="[False]">value returns this parameter if the value of the expression is False</param>
    ''' <returns></returns>
    <Extension> Public Function [If](Of T)(expr As Boolean, [True] As T, [False] As T) As T
        If expr = True Then
            Return [True]
        Else
            Return [False]
        End If
    End Function

    ''' <summary>
    ''' DirectCast(obj, T)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    <Extension> Public Function [As](Of T)(obj) As T
        If obj Is Nothing Then
            Return Nothing
        End If
        Return DirectCast(obj, T)
    End Function

    ''' <summary>
    ''' 基类集合与继承类的集合约束
    ''' </summary>
    ''' <typeparam name="T">继承类向基类进行约束</typeparam>
    ''' <returns></returns>
    Public Function Constrain(Of TConstrain As Class, T As TConstrain)(source As Generic.IEnumerable(Of T)) As TConstrain()
        If source.IsNullOrEmpty Then
            Return New TConstrain() {}
        End If

        Dim ChunkBuffer As TConstrain() = New TConstrain(source.Count - 1) {}
        For i As Integer = 0 To ChunkBuffer.Length - 1
            ChunkBuffer(i) = source(i)
        Next
        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' 0 -> False
    ''' 1 -> True
    ''' </summary>
    ''' <param name="b"></param>
    ''' <returns></returns>
    <Extension> Public Function ToBoolean(b As Long) As Boolean
        If b = 0 Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="source">仅仅是起到类型复制的作用</param>
    ''' <returns></returns>
    <Extension> Public Function CopyTypeDef(Of TKey, TValue)(source As Dictionary(Of TKey, TValue)) As Dictionary(Of TKey, TValue)
        Dim hash As New Dictionary(Of TKey, TValue)
        Return hash
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="IList">仅仅是起到类型复制的作用</param>
    ''' <returns></returns>
    <Extension> Public Function CopyTypeDef(Of T)(IList As List(Of T)) As List(Of T)
        Return New List(Of T)
    End Function

    ''' <summary>
    ''' 假若不存在目标键名，则返回空值，默认值为空值
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="hash"></param>
    ''' <param name="Index"></param>
    ''' <param name="[default]"></param>
    ''' <returns></returns>
    <Extension> Public Function TryGetValue(Of TKey, TValue)(hash As Dictionary(Of TKey, TValue), Index As TKey, Optional [default] As TValue = Nothing) As TValue
        If hash Is Nothing Then
            Call PrintException("hash table is nothing!")
            Return [default]
        End If

        If hash.ContainsKey(Index) Then
            Return hash(Index)
        Else
#If DEBUG Then
            Call PrintException($"Index:={Scripting.ToString(Index)} is not exist in the hash table!")
#End If
            Return [default]
        End If
    End Function

    <Extension> Public Function TryGetValue(Of TKey, TValue, TProp)(hash As Dictionary(Of TKey, TValue), Index As TKey, prop As String) As TProp
        If hash Is Nothing Then
            Return Nothing
        End If

        If Not hash.ContainsKey(Index) Then
            Return Nothing
        End If

        Dim obj As TValue = hash(Index)
        Dim propertyInfo As PropertyInfo = obj.GetType.GetProperty(prop)

        If propertyInfo Is Nothing Then
            Return Nothing
        End If

        Dim value As Object = propertyInfo.GetValue(obj)
        Return DirectCast(value, TProp)
    End Function

    <Extension> Public Function AddRange(Of TKey, TValue)(ByRef hash As Dictionary(Of TKey, TValue), data As Generic.IEnumerable(Of KeyValuePair(Of TKey, TValue))) As Dictionary(Of TKey, TValue)
        If data.IsNullOrEmpty Then
            Return hash
        End If

        For Each obj In data
            Call hash.Add(obj.Key, obj.Value)
        Next
        Return hash
    End Function

    ''' <summary>
    ''' If the path string value is already wrappered by quot, then this function will returns the original string (DO_NOTHING).
    ''' (假若命令行之中的文件名参数之中含有空格的话，则可能会造成错误，需要添加一个双引号来消除歧义)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("CLI_PATH")>
    <Extension> Public Function CliPath(Path As String) As String
        If String.IsNullOrEmpty(Path) Then
            Return ""
        Else
            Path = Path.Replace("\", "/")  '这个是R、Java、Perl等程序对路径的要求所导致的
            Return Path.CliToken
        End If
    End Function

    ''' <summary>
    ''' <see cref="CliPath(String)"/>函数为了保持对Linux系统的兼容性会自动替换\为/符号，这个函数则不会执行这个替换
    ''' </summary>
    ''' <param name="Token"></param>
    ''' <returns></returns>
    <Extension> Public Function CliToken(Token As String) As String
        If String.IsNullOrEmpty(Token) OrElse Not Len(Token) > 2 Then
            Return Token
        End If

        If Token.First = """"c AndAlso Token.Last = """"c Then
            Return Token
        End If
        If Token.Contains(" "c) Then Token = $"""{Token}"""
        Return Token
    End Function

    ''' <summary>
    ''' 对Xml文件之中的特殊字符进行转义处理
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function NormalizeXMLString(str As String) As String
        Dim sBuilder As StringBuilder = New StringBuilder(str)

        Call sBuilder.Replace("&", "&amp;")
        Call sBuilder.Replace("""", "&quot;")
        Call sBuilder.Replace("×", "&times;")
        Call sBuilder.Replace("÷", "&divide;")
        Call sBuilder.Replace("<", "&lt;")
        Call sBuilder.Replace(">", "&gt;")

        Return sBuilder.ToString
    End Function

    Const A As Integer = Asc("A")
    Const Z As Integer = Asc("Z")

    ''' <summary>
    ''' You can using this method to create a empty list for the specific type of anonymous type object.
    ''' (使用这个方法获取得到匿名类型的列表数据集合对象)
    ''' </summary>
    ''' <typeparam name="TAnonymousType"></typeparam>
    ''' <param name="typedef">The temp object which was created anonymous.(匿名对象的集合)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetAnonymousTypeList(Of TAnonymousType As Class)(typedef As Generic.IEnumerable(Of TAnonymousType)) As List(Of TAnonymousType)
        Dim List = typedef.ToList
        Call List.Clear()
        Return List
    End Function

#If FRAMEWORD_CORE Then

    <Extension> Public Function ModuleVersion(Type As Type) As String
        Dim Assembly = Type.Assembly
        Dim attrs = Assembly.CustomAttributes

        Return ""
    End Function
#End If

    ''' <summary>
    ''' Removes VbCr and VbLf
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    <Extension> Public Function TrimVBCrLf(s As String) As String
        s = s.Replace(vbCrLf, "")
        s = s.Replace(vbCr, "").Replace(vbLf, "")
        Return s
    End Function

    ''' <summary>
    ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="slideWindowSize"></param>
    ''' <param name="offset"></param>
    ''' <param name="extTails">是否将尾巴补上？否则序列会烧掉<paramref name="slideWindowSize"/>大小的空缺，默认不用补全</param>
    ''' <returns></returns>
    <Extension> Public Function CreateSlideWindows(Of T)(data As Generic.IEnumerable(Of T),
                                                         slideWindowSize As Integer,
                                                         Optional offset As Integer = 1,
                                                         Optional extTails As Boolean = False) As SlideWindowHandle(Of T)()
        Return SlideWindow.CreateSlideWindows(Of T)(data, slideWindowSize, offset, extTails)
    End Function

    ''' <summary>
    ''' Chr(0): NULL char
    ''' </summary>
    ''' <remarks></remarks>
    Public Const NIL As Char = Chr(0)

    ''' <summary>
    ''' Format the datetime value in the format of yy/mm/dd hh:min 
    ''' </summary>
    ''' <param name="dat"></param>
    ''' <returns></returns>
    <ExportAPI("Date.ToString", Info:="Format the datetime value in the format of yy/mm/dd hh:min")>
    <Extension> Public Function DateToString(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero

        Return $"{yy}/{mm}/{dd} {hh}:{mmin}"
    End Function

    <ExportAPI("Date.ToNormalizedPathString")>
    <Extension> Public Function ToNormalizedPathString(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero

        Return String.Format("{0}-{1}-{2} {3}.{4}", yy, mm, dd, hh, mmin)
    End Function

    ''' <summary>
    ''' 将目标集合之中的数据按照<paramref name="parTokens"></paramref>参数分配到子集合之中，这个函数之中不能够使用并行化计数，以保证元素之间的相互原有的顺序
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="parTokens">每一个子集合之中的元素的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Split(Of T)(source As Generic.IEnumerable(Of T), parTokens As Integer) As T()()
        Dim chunkList As List(Of T()) = New List(Of T())
        Dim chunkBuffer As T() = source.ToArray
        Dim n As Integer = chunkBuffer.Length

        For i As Integer = 0 To n - 1 Step parTokens
            Dim buffer As T()

            If n - i >= parTokens Then
                buffer = New T(parTokens - 1) {}
            Else
                buffer = New T(n - i - 1) {}
            End If

            Call Array.ConstrainedCopy(chunkBuffer, i, buffer, Scan0, buffer.Length)
            Call chunkList.Add(buffer)
        Next

        Return chunkList.ToArray
    End Function

    ''' <summary>
    ''' Merge two type specific collection.(函数会忽略掉空的集合，函数会构建一个新的集合，原有的集合不受影响)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension> Public Function Join(Of T)(source As IEnumerable(Of T), data As IEnumerable(Of T)) As List(Of T)
        Dim srcList As List(Of T) = If(source.IsNullOrEmpty, New List(Of T), source.ToList)
        If Not data.IsNullOrEmpty Then
            Call srcList.AddRange(data)
        End If
        Return srcList
    End Function

    ''' <summary>
    ''' <see cref="String.Join"/>，这是一个安全的函数，当数组为空的时候回返回空字符串 
    ''' </summary>
    ''' <param name="Tokens"></param>
    ''' <param name="delimiter"></param>
    ''' <returns></returns>
    <Extension> Public Function JoinBy(Tokens As Generic.IEnumerable(Of String), delimiter As String) As String
        If Tokens Is Nothing Then
            Return ""
        End If
        Return String.Join(delimiter, Tokens.ToArray)
    End Function

    ''' <summary>
    ''' <see cref="String.Join"/>，这是一个安全的函数，当数组为空的时候回返回空字符串 
    ''' </summary>
    ''' <param name="values"></param>
    ''' <param name="delimiter"></param>
    ''' <returns></returns>
    <Extension> Public Function JoinBy(values As Generic.IEnumerable(Of Integer), delimiter As String) As String
        If values Is Nothing Then
            Return ""
        End If
        Return String.Join(delimiter, values.ToArray(Function(n) CStr(n)))
    End Function

    <Extension> Public Function Join(Of T)(Collection As Generic.IEnumerable(Of T), data As T) As List(Of T)
        Return Collection.Join({data})
    End Function

    <Extension> Public Function Join(Of T)(obj As T, collection As Generic.IEnumerable(Of T)) As List(Of T)
        Dim list As New List(Of T) From {obj}
        Call list.AddRange(collection)
        Return list
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("File.Select", Info:="Open the file open dialog to gets the file")>
    Public Function SelectFile(Optional ext As String = "*.*") As String
        Using Open = New OpenFileDialog With {.Filter = $"{ext}|{ext}"}

            If Open.ShowDialog = DialogResult.OK Then
                Return Open.FileName
            Else
                Return ""
            End If
        End Using
    End Function
#End If

    ''' <summary>
    ''' 本方法会执行外部命令并等待其执行完毕，函数返回状态值
    ''' </summary>
    ''' <param name="Process"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Invoke", Info:="Invoke a folked system process object to execute a parallel task.")>
    <Extension> Public Function Invoke(Process As Process) As Integer
        Call Process.Start()
        Call Process.WaitForExit()
        Return Process.ExitCode
    End Function

    ''' <summary>
    ''' Gets a random number in the region of [0,1]. (获取一个[0,1]区间之中的随机数，请注意：因为为了尽量做到随机化，这个函数会不断的初始化随机种子，
    ''' 故而性能较低，不可以在大量重复调用，或者在批量调用的时候请使用并行化拓展的LINQ)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Rand", Info:="Returns a random floating-point number between 0.0 and 1.0.")>
    Public Function RandomDouble() As Double
        Dim rand As New Random(SecurityString.MD5Hash.ToLong(SecurityString.MD5Hash.GetMd5Hash(Now.ToString)) / Integer.MaxValue)
        Dim n As Double = rand.Next(0, 100)
        n = n / 100
        Return n
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 非线程的方式启动，当前线程会被阻塞在这里直到运行完毕
    ''' </summary>
    ''' <param name="driver"></param>
    ''' <returns></returns>
    <ExportAPI("Run", Info:="Running the object model driver, the target object should implement the driver interface.")>
    Public Function RunDriver(driver As IObjectModel_Driver) As Integer
        Return driver.Run
    End Function

    ''' <summary>
    ''' 使用线程的方式启动
    ''' </summary>
    ''' <param name="driver"></param>
    <ExportAPI("Run", Info:="Running the object model driver, the target object should implement the driver interface.")>
    <Extension>
    Public Function DriverRun(driver As IObjectModel_Driver) As Threading.Thread
        Return Parallel.RunTask(AddressOf driver.Run)
    End Function
#End If

    ''' <summary>
    ''' Gets the element counts in the target data collection, if the collection object is nothing or empty 
    ''' then this function will returns ZERO, others returns Collection.Count.(返回一个数据集合之中的元素的数目，
    ''' 假若这个集合是空值或者空的，则返回0，其他情况则返回Count拓展函数的结果) 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetElementCounts(Of T)(Collection As Generic.IEnumerable(Of T)) As Integer
        If Collection.IsNullOrEmpty Then
            Return 0
        Else
            Return System.Linq.Enumerable.Count(Collection)
        End If
    End Function

#If FRAMEWORD_CORE Then

    '''<summary>
    '''  Looks up a localized string similar to                     GNU GENERAL PUBLIC LICENSE
    '''                       Version 3, 29 June 2007
    '''
    ''' Copyright (C) 2007 Free Software Foundation, Inc. &lt;http://fsf.org/&gt;
    ''' Everyone is permitted to copy and distribute verbatim copies
    ''' of this license document, but changing it is not allowed.
    '''
    '''                            Preamble
    '''
    '''  The GNU General Public License is a free, copyleft license for
    '''software and other kinds of works.
    '''
    '''  The licenses for most software and other practical works are designed
    '''to take away yo [rest of string was truncated]&quot;;.
    '''</summary>
    Public ReadOnly Property GPL3 As String
        Get
            Return My.Resources.gpl
        End Get
    End Property
#End If

#If NET_40 = 0 Then

    Private ReadOnly _AllDotNETPrefixColors As Color() =
        (From Color As Color In (From p As PropertyInfo  'Gets all of the known name color from the Color object its shared property.
                                 In GetType(Color).GetProperties(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Static)
                                 Where p.PropertyType = GetType(Color)
                                 Let ColorValue As Color = DirectCast(p.GetValue(Nothing), Color)
                                 Select ColorValue).ToArray
         Where Color <> Color.White
         Select Color).ToArray

    Public ReadOnly Property AllDotNetPrefixColors As Color()
        Get
            Return _AllDotNETPrefixColors.Randomize
        End Get
    End Property
#End If

    ''' <summary>
    ''' Free this variable pointer in the memory.(销毁本对象类型在内存之中的指针)
    ''' </summary>
    ''' <typeparam name="T">假若该对象类型实现了<see cref="System.IDisposable"></see>接口，则函数还会在销毁前调用该接口的销毁函数</typeparam>
    ''' <param name="obj"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub Free(Of T As Class)(ByRef obj As T)
        If Not obj Is Nothing Then
            Dim TypeInfo As Type = obj.GetType
            If Array.IndexOf(TypeInfo.GetInterfaces, GetType(System.IDisposable)) > -1 Then
                Try
                    Call DirectCast(obj, System.IDisposable).Dispose()
                Catch ex As Exception

                End Try
            End If
        End If

        obj = Nothing
        Call FlushMemory()
    End Sub

    ''' <summary>
    ''' Pause the console program.
    ''' </summary>
    ''' <param name="Prompted"></param>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Pause", Info:="Pause the console program.")>
    Public Sub Pause(Optional Prompted As String = "Press any key to continute...")
        Call Console.WriteLine(Prompted)
        Call Console.Read()
    End Sub

    Const _DOUBLE As String = "((-?\d\.\d+e[+-]\d+)|(-?\d+\.\d+)|(-?\d+))"

    ''' <summary>
    ''' 使用正则表达式解析目标字符串对象之中的一个实数
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Double.Match")>
    <Extension> Public Function ParseDouble(s As String) As Double
        Return Val(s.Match(_DOUBLE))
    End Function

    <ExportAPI("OffSet")>
    <Extension> Public Function Offset(ByRef array As Integer(), intOffset As Integer) As Integer()
        For i As Integer = 0 To array.Length - 1
            array(i) = array(i) + intOffset
        Next
        Return array
    End Function

    <ExportAPI("OffSet")>
    <Extension> Public Function Offset(ByRef array As Long(), intOffset As Integer) As Long()
        For i As Integer = 0 To array.Length - 1
            array(i) = array(i) + intOffset
        Next
        Return array
    End Function

    ''' <summary>
    ''' 空字符串会返回空的日期
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Date.Parse")>
    <Extension> Public Function ParseDateTime(s As String) As Date
        If String.IsNullOrEmpty(s) Then
            Return New Date
        Else
            Return DateTime.Parse(s)
        End If
    End Function

    ''' <summary>
    ''' 当所被读取的文本文件的大小超过了<see cref="System.Text.StringBuilder"></see>的上限的时候，就需要使用本方法进行读取操作了。<paramref name="Path">目标文件</paramref>必须是已经存在的文件
    ''' </summary>
    ''' <param name="Path">目标文件必须是已经存在的文件</param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ReadUltraLargeTextFile(Path As String, Encoding As System.Text.Encoding) As String
        Using FileStream As FileStream = New FileStream(Path, FileMode.Open)
            Dim ChunkBuffer As Byte() = New Byte(FileStream.Length - 1) {}
            Call FileStream.Read(ChunkBuffer, 0, ChunkBuffer.Count)
            Return Encoding.GetString(ChunkBuffer)
        End Using
    End Function

    ''' <summary>
    ''' Save the binary data into the filesystem.(保存二进制数据包值文件系统)
    ''' </summary>
    ''' <param name="ChunkBuffer">The binary bytes data of the target package's data.(目标二进制数据)</param>
    ''' <param name="SavePath">The saved file path of the target binary data chunk.(目标二进制数据包所要进行保存的文件名路径)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("FlushStream")>
    <Extension> Public Function FlushStream(ChunkBuffer As Generic.IEnumerable(Of Byte),
                                            <Parameter("Path.Save")> SavePath As String) As Boolean

        Dim ParentDir As String = If(String.IsNullOrEmpty(SavePath),
            FileIO.FileSystem.CurrentDirectory,
            FileIO.FileSystem.GetParentPath(SavePath))

        Call FileIO.FileSystem.CreateDirectory(ParentDir)
        Call FileIO.FileSystem.WriteAllBytes(SavePath, ChunkBuffer.ToArray, False)

        Return True
    End Function

    <Extension> Public Function FlushStream(stream As Net.Protocol.ISerializable, SavePath As String) As Boolean
        Dim rawStream As Byte() = stream.Serialize
        If rawStream Is Nothing Then
            rawStream = New Byte() {}
        End If
        Return rawStream.FlushStream(SavePath)
    End Function

#Region ""

    ''' <summary>
    ''' Assigning the value to the specific named property to the target object.
    ''' (将<paramref name="value"/>参数之中的值赋值给目标对象<paramref name="obj"/>之中的指定的<paramref name="name"/>属性名称的属性，如果发生错误，则原有的对象<paramref name="obj"/>不会被修改)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tvalue"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="Name">可以使用NameOf得到需要进行修改的属性名称</param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension> Public Function InvokeSet(Of T, Tvalue)(ByRef obj As T, Name As String, value As Tvalue) As T
        Dim Type As Type = GetType(T)
        Dim lstProp As PropertyInfo() =
            Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
        Dim p As PropertyInfo = (From pInfo As PropertyInfo
                                 In lstProp
                                 Where String.Equals(Name, pInfo.Name)
                                 Select pInfo).FirstOrDefault
        If Not p Is Nothing Then
            'Call Console.WriteLine(value.ToString)
            Call p.SetValue(obj, value)
        Else
            Dim lstName As String = String.Join("; ", (From pp In lstProp Select ss = pp.Name).ToArray)
#If DEBUG Then
            Call $"Could Not found the target parameter which is named {Name} // {lstName}".__DEBUG_ECHO
#End If
        End If

        Return obj
    End Function

    <Extension> Public Function InvokeSet(Of T As Class, Tvalue)(obj As T, [Property] As PropertyInfo, value As Tvalue) As T
        Call [Property].SetValue(obj, value)
        Return obj
    End Function

    ''' <summary>
    ''' Value assignment to the target variable.(将<paramref name="value"/>参数里面的值赋值给<paramref name="var"/>参数然后返回<paramref name="value"/>) 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="var"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function InvokeSet(Of T)(ByRef var As T, value As T) As T
        var = value
        Return value
    End Function

    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' (与函数<see cref="InvokeSet(Of T)(ByRef T, T)"/>)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="CopyTo"><paramref name="source"/> ==> <paramref name="CopyTo"/> target.</param>
    ''' <returns></returns>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef CopyTo As T) As T
        CopyTo = source
        Return CopyTo
    End Function

    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T) As T
        arg1 = source
        arg2 = source
        Return source
    End Function

    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T, ByRef arg11 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        arg11 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T, ByRef arg11 As T, ByRef arg12 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        arg11 = source
        arg12 = source
        Return source
    End Function
#End Region

#If NET_40 = 0 Then

    ''' <summary>
    ''' Modify target object property value using a <paramref name="valueModifier">specific value provider</paramref> and then return original instance object.
    ''' (修改目标对象的属性之后返回目标对象)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ModifyValue(Of T As Class)([property] As PropertyInfo, obj As T, valueModifier As Func(Of Object, Object)) As T
        Dim Value As Object = [property].GetValue(obj)
        Value = valueModifier(Value)
        Call [property].SetValue(obj, Value)

        Return obj
    End Function
#End If

    Public Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (process As IntPtr, minimumWorkingSetSize As Integer, maximumWorkingSetSize As Integer) As Integer

    ''' <summary>
    ''' Rabbish collection to free the junk memory.(垃圾回收)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("FlushMemory", Info:="Rabbish collection To free the junk memory.")>
    Public Sub FlushMemory()
        Call GC.Collect()
        Call GC.WaitForPendingFinalizers()

        If (Environment.OSVersion.Platform = PlatformID.Win32NT) Then
            Call SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
        End If
    End Sub

    <Extension> Public Function VectorCollectionToMatrix(Of T)(Vectors As IEnumerable(Of Generic.IEnumerable(Of T))) As T(,)
        Dim MAT As T(,) = New T(Vectors.Count, Vectors.First.Count) {}
        Dim Dimension As Integer = Vectors.First.Count

        For i As Integer = 0 To MAT.GetLength(Dimension)
            Dim Vector = Vectors(i)

            For j As Integer = 0 To Dimension
                MAT(i, j) = Vector(j)
            Next
        Next

        Return MAT
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 向字典对象之中更新或者插入新的数据，假若目标字典对象之中已经存在了一个数据的话，则会将原有的数据覆盖，并返回原来的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dict"></param>
    ''' <param name="item"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function InsertOrUpdate(Of T As sIdEnumerable)(ByRef dict As Dictionary(Of String, T), item As T) As T
        Dim pre As T

        If dict.ContainsKey(item.Identifier) Then
            pre = dict(item.Identifier)

            Call dict.Remove(item.Identifier)
            Call $"data was updated: {Scripting.ToString(pre)} -> {item.Identifier}".__DEBUG_ECHO
        Else
            pre = item
        End If

        Call dict.Add(item.Identifier, item)

        Return pre
    End Function

    <Extension> Public Function Remove(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable)(
                            ByRef dict As Dictionary(Of String, T), item As T) As T

        Call dict.Remove(item.Identifier)
        Return item
    End Function

    <Extension> Public Function AddRange(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable)(
                            ByRef dict As Dictionary(Of String, T),
                            data As Generic.IEnumerable(Of T)) _
        As Dictionary(Of String, T)

        For Each item In data
            Call InsertOrUpdate(dict, item)
        Next

        Return dict
    End Function
#End If

    <Extension> Public Function IsNullOrEmpty(sBuilder As StringBuilder) As Boolean
        Return sBuilder Is Nothing OrElse sBuilder.Length = 0
    End Function

    ''' <summary>
    ''' Merge the target array collection into one collection.(将目标数组的集合合并为一个数组)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixToVector(Of T)(Collection As Generic.IEnumerable(Of Generic.IEnumerable(Of T))) As T()
        Return MatrixToList(Collection).ToArray
    End Function

    ''' <summary>
    ''' Empty list will be skip and ignored.(这是一个安全的方法，空集合会被自动跳过，并且这个函数总是返回一个集合不会返回空值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension> Public Function MatrixToList(Of T)(source As IEnumerable(Of Generic.IEnumerable(Of T))) As List(Of T)
        Dim ChunkBuffer As List(Of T) = New List(Of T)

        For Each Line As Generic.IEnumerable(Of T) In source

            If Not Line.IsNullOrEmpty Then
                Call ChunkBuffer.AddRange(collection:=Line)
            End If
        Next

        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' Merge the target array collection into one collection.(将目标数组的集合合并为一个数组，这个方法是提供给超大的集合的，即元素的数目非常的多的，即超过了<see cref="Integer"></see>的上限值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixToUltraLargeVector(Of T)(Collection As Generic.IEnumerable(Of T())) As LinkedList(Of T)
        Dim ChunkBuffer As LinkedList(Of T) = New LinkedList(Of T)

        For Each Line As T() In Collection
            For Each item As T In Line
                Call ChunkBuffer.AddLast(item)
            Next
        Next

        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' Add a linked list of a collection of specific type of data.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension> Public Function AddRange(Of T)(list As LinkedList(Of T), data As Generic.IEnumerable(Of T)) As LinkedList(Of T)
        For Each item As T In data
            Call list.AddLast(item)
        Next

        Return list
    End Function

    ''' <summary>
    ''' 矩阵转置： 将矩阵之中的元素进行行列位置的互换
    ''' </summary>
    ''' <typeparam name="T">矩阵之中的元素类型</typeparam>
    ''' <param name="MAT">为了方便理解和使用，矩阵使用数组的数组来表示的</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixTranspose(Of T)(MAT As Generic.IEnumerable(Of T())) As T()()
        Dim LQuery As T()() = (From i As Integer
                               In MAT.First.Sequence
                               Select (From Line As T() In MAT Select Line(i)).ToArray).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 将矩阵之中的元素进行行列位置的互换，请注意，假若长度不一致的话，会按照最短的元素来转置，故而使用本函数可能会造成一些信息的丢失
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixTransposeIgnoredDimensionAgreement(Of T)(MAT As Generic.IEnumerable(Of T())) As T()()
        Dim LQuery = (From i As Integer
                      In (From n As T()
                          In MAT
                          Select n.Length
                          Order By Length Ascending).ToArray.First.Sequence
                      Select (From Line In MAT Select Line(i)).ToArray).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("Mv.Split")>
    Public Function SplitMV(dir As String, <Parameter("Dir.MoveTo")> moveTo As String, Split As Integer) As Integer
#Else
    Public Function SplitMV(dir As String, moveto As String, split As Integer) As Integer
#End If
        Dim Files As String() = FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchTopLevelOnly).ToArray
        Dim n As Integer
        Dim m As Integer = 1

        For i As Integer = 0 To Files.Length - 1
            If n < Split Then
                Call FileIO.FileSystem.MoveFile(Files(i), String.Format("{0}_{1}/{2}", moveTo, m, FileIO.FileSystem.GetFileInfo(Files(i)).Name))
                n += 1
            Else
                n = 0
                m += 1
            End If
        Next

        Return 0
    End Function

    <Extension> Public Function TryInvoke(Of T, TOut)(value As T,
                                                      proc As Func(Of T, TOut),
                                                      Optional [default] As TOut = Nothing) As TOut
        Try
            Return proc(value)
        Catch ex As Exception
            Call App.LogException(ex)
            Return [default]
        End Try
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection">请务必要确保集合之中的元素的<see cref="Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable.Identifier"></see></param>属性是唯一的，否则会出错
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToEntriesDictionary(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable)(Collection As Generic.IEnumerable(Of T)) As Dictionary(Of String, T)
        Dim Dictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)
        For Each Item As T In Collection
            Call Dictionary.Add(Item.Identifier, Item)
        Next

        Return Dictionary
    End Function
#End If

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 判断目标实数是否为一个无穷数或者非计算的数字，产生的原因主要来自于除0运算结果或者达到了<see cref="Double"></see>的上限或者下限
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Double.Is.NA",
               Info:="Is this double type of the number is an NA type infinity number. this is major comes from the devided by ZERO.")>
    <Extension> Public Function Is_NA_UHandle(n As Double) As Boolean
#Else
    <Extension> Public Function Is_NA_UHandle(n As Double) As Boolean
#End If
        Return Double.IsNaN(n) OrElse Double.IsInfinity(n) OrElse Double.IsNegativeInfinity(n) OrElse Double.IsPositiveInfinity(n)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Fuzzy match two string, this is useful for the text query or searching.
    ''' </summary>
    ''' <param name="Query"></param>
    ''' <param name="Subject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("FuzzyMatch", Info:="Fuzzy match two string, this is useful for the text query or searching.")>
    <Extension> Public Function FuzzyMatching(Query As String, Subject As String) As Boolean
        Return FuzzyMatchString.Equals(Query, Subject)
    End Function
#End If

    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BooleanValues As SortedDictionary(Of String, Boolean) =
        New SortedDictionary(Of String, Boolean) From {
 _
            {"t", True}, {"true", True},
            {"1", True},
            {"y", True}, {"yes", True}, {"ok", True},
            {"ok!", True},
            {"success", True}, {"successful", True}, {"successfully", True}, {"succeeded", True},
            {"right", True},
            {"wrong", False},
            {"failure", False}, {"failures", False},
            {"exception", False},
            {"error", False}, {"err", False},
            {"f", False}, {"false", False},
            {"0", False},
            {"n", False}, {"no", False}
        }

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.(请注意，空值字符串为False)
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Get.Boolean")> <Extension> Public Function getBoolean(str As String) As Boolean
#Else
    <Extension> Public Function get_BooleanValue(str As String) As Boolean
#End If
        If String.IsNullOrEmpty(str) Then
            Return False
        End If

        str = str.ToLower.Trim
        If BooleanValues.ContainsKey(key:=str) Then
            Return BooleanValues(str)
        Else
#If DEBUG Then
            Call $"""{str}"" {NameOf(System.Boolean)} (null_value_definition)  ==> False".__DEBUG_ECHO
#End If
            Return False
        End If
    End Function

    <Extension> <ExportAPI("Get.Boolean")> Public Function getBoolean(ch As Char) As Boolean
        If ch = NIL Then
            Return False
        End If

        Select Case ch
            Case "y"c, "Y"c, "t"c, "T"c, "1"c
                Return True
            Case "n"c, "N"c, "f"c, "F"c, "0"c
                Return False
        End Select

        Return True
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("Time2Binary", Info:="Convert the date time value into a long data type value.")>
    <Extension> Public Function ToBinary([Date] As Date) As Long
#Else
    <Extension> Public Function ToBinary([Date] As Date) As Long
#End If
        Return [Date].Year * 100000 + [Date].Month * 10000 + [Date].Day * 1000 +
                [Date].Hour * 100 + [Date].Minute * 10 + [Date].Second
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("Get.Item")>
    <Extension> Public Function GetItem(Of T)(Collection As Generic.IEnumerable(Of T), index As Integer) As T
#Else
    <Extension> Public Function GetItem(Of T)(Collection As Generic.IEnumerable(Of T), index As Integer) As T
#End If
        If Collection.IsNullOrEmpty OrElse index >= Collection.Count Then
            Return Nothing
        Else
            Return Collection(index)
        End If
    End Function

    ''' <summary>
    ''' 求取该数据集的标准差
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("StdError")>
    <Extension> Public Function StdError(data As Generic.IEnumerable(Of Double)) As Double
        Dim Average As Double = data.Average
        Dim Sum = (From n As Double In data Select (n - Average) ^ 2).ToArray.Sum
        Sum /= data.Count
        Return Global.System.Math.Sqrt(Sum)
    End Function

    ''' <summary>
    ''' The first element in a collection.
    ''' </summary>
    Public Const Scan0 As Integer = 0
    Public Const Second As Integer = 1

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data 
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Get.Description",
               Info:="Get the description data from a enum type value, if the target have no <see cref=""DescriptionAttribute""></see> attribute data then function will return the string value from the ToString() function.")>
    <Extension> Public Function Description(value As [Enum]) As String
#Else
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data 
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Description(e As [Enum]) As String
#End If
        Dim Type As Type = value.GetType()
        Dim s_Value As String = value.ToString
        Dim MemInfos As MemberInfo() = Type.GetMember(name:=s_Value)

        If MemInfos.IsNullOrEmpty Then
            Return s_Value
        End If

        Dim CustomAttrs As Object() =
            MemInfos(Scan0).GetCustomAttributes(GetType(DescriptionAttribute), inherit:=False)

        If Not CustomAttrs.IsNullOrEmpty Then
            Return CType(CustomAttrs(Scan0), DescriptionAttribute).Description
        Else
            Return s_Value
        End If
    End Function

    ''' <summary>
    ''' Enumerate all of the enum values in the specific <see cref="System.Enum"/> type data.(只允许枚举类型，其他的都返回空集合)
    ''' </summary>
    ''' <typeparam name="T">泛型类型约束只允许枚举类型，其他的都返回空集合</typeparam>
    ''' <returns></returns>
    Public Function Enums(Of T)() As T()
        Dim EnumType As Type = GetType(T)
        If Not EnumType.IsInheritsFrom(GetType(System.Enum)) Then
            Return Nothing
        End If

        Dim EnumValues As Object() =
            Scripting.CastArray(Of System.Enum)(EnumType.GetEnumValues).ToArray(Of Object)(
                Function(ar) DirectCast(ar, Object))
        Dim values As T() = EnumValues.ToArray(Of T)(Function([enum]) DirectCast([enum], T))
        Return values
    End Function

    ''' <summary>
    ''' 函数只返回有重复的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tag"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="getKey"></param>
    ''' <returns></returns>
    <Extension> Public Function CheckDuplicated(Of T, Tag)(source As IEnumerable(Of T), getKey As Func(Of T, Tag)) As GroupResult(Of T, Tag)()
        Dim Groups = From obj As T
                     In source
                     Select obj
                     Group obj By objTag = getKey(obj) Into Group '
        Dim KnowDuplicates = (From obj In Groups.AsParallel
                              Where obj.Group.Count > 1
                              Select New GroupResult(Of T, Tag) With {
                                  .TAG = obj.objTag,
                                  .Group = obj.Group.ToArray}).ToArray
        Return KnowDuplicates
    End Function

    ''' <summary>
    ''' 移除重复的对象，这个函数是根据对象所生成的标签来完成的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tag"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="getKey">得到对象的标签</param>
    ''' <returns></returns>
    <Extension> Public Function RemoveDuplicates(Of T, Tag)(source As IEnumerable(Of T), getKey As Func(Of T, Tag)) As T()
        Dim Groups = From obj As T
                     In source
                     Select obj
                     Group obj By objTag = getKey(obj) Into Group '
        Dim LQuery = (From obj In Groups Select obj.Group.First).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("NullValue.Trim", Info:="Remove all of the null object in the target object collection")>
    <Extension> Public Function TrimNull(Of T As Class)(source As IEnumerable(Of T)) As T()
#Else
    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimNull(Of T As Class)(Collection As Generic.IEnumerable(Of T)) As T()
#End If
        If source.IsNullOrEmpty Then
            Return New T() {}
        Else
            Return (From x In source Where Not x Is Nothing Select x).ToArray
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Return a collection with randomize element position in <paramref name="source">the original collection</paramref>.(从原有序序列中获取一个随机元素的序列)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Elements.Randomize")>
    <Extension> Public Function Randomize(Of T)(source As Generic.IEnumerable(Of T)) As T()
#Else
    ''' <summary>
    ''' Return a collection with randomize element position in <paramref name="Collection">the original collection</paramref>.(从原有序序列中获取一个随机元素的序列)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function RandomizeElements(Of T)(Collection As Generic.IEnumerable(Of T)) As T()
#End If
        Call VBMath.Randomize()

        Dim ChunkBuffer As T() = New T(source.Count - 1) {}
        Dim TempList = source.ToList
        Dim Seeds As Integer = (Rnd() * SecurityString.ToLong(SecurityString.GetMd5Hash(Now.ToString))) / CLng(Integer.MaxValue) * 2
        Dim Rand As New Random(Seed:=Seeds)
        Dim Length As Integer = TempList.Count - 1

        For i As Integer = 0 To ChunkBuffer.Length - 1
            Dim index As Integer = Rand.Next(minValue:=0, maxValue:=Length)
            ChunkBuffer(i) = TempList(index)
            Call TempList.RemoveAt(index)
            Length -= 1
        Next

        Return ChunkBuffer
    End Function

    <ExportAPI("Sequence.Random")>
    <Extension> Public Function SeqRandom(n As Integer) As Integer()
        Dim Original As Integer() = n.Sequence
        Dim Random As Integer() = Original.Randomize
        Return Random
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get a specific item value from the target collction data using its UniqueID property，
    ''' (请注意，请尽量不要使用本方法，因为这个方法的效率有些低，对于获取<see cref="Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable">
    ''' </see>类型的集合之中的某一个对象，请尽量先转换为字典对象，在使用该字典对象进行查找以提高代码效率，使用本方法的优点是可以选择忽略<paramref name="UniqueId">
    ''' </paramref>参数之中的大小写，以及对集合之中的存在相同的Key的这种情况的容忍)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="UniqueId"></param>
    ''' <param name="IgnoreCase"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Get.Item")>
    <Extension> Public Function GetItem(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable)(
        Collection As Generic.IEnumerable(Of T), UniqueId As String, Optional IgnoreCase As StringComparison = StringComparison.Ordinal) As T

        Dim LQuery = (From item In Collection Where String.Equals(UniqueId, item.Identifier, IgnoreCase) Select item).ToArray
        If Not LQuery.IsNullOrEmpty Then
            Return LQuery.First
        Else
            Return Nothing
        End If
    End Function
#End If

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Copy the value in <paramref name="value"></paramref> into target variable <paramref name="target"></paramref> and then return the target variable.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("value.Copy")>
    <Extension> Public Function CopyTo(Of T)(value As T, ByRef target As T) As T
#Else
    ''' <summary>
    ''' Copy the value in <paramref name="value"></paramref> into target variable <paramref name="target"></paramref> and then return the target variable.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CopyTo(Of T)(value As T, ByRef target As T) As T
#End If
        target = value
        Return value
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("Move.Next")>
    <Extension> Public Function MoveNext(ByRef p As Long) As Long
#Else
    <Extension> Public Function MoveNext(ByRef p As Long) As Long
#End If
        Dim value = p
        p += 1
        Return value
    End Function

    ''' <summary>
    ''' 变量<paramref name="p"/>移动距离<paramref name="d"/>然后返回其移动之前的值
    ''' </summary>
    ''' <param name="p"></param>
    ''' <param name="d"></param>
    ''' <returns></returns>
    <Extension> Public Function Move(ByRef p As Long, d As Integer) As Long
        Dim value = p
        p += d
        Return value
    End Function

    ''' <summary>
    ''' 变量<paramref name="p"/>移动距离<paramref name="d"/>然后返回其移动之前的值
    ''' </summary>
    ''' <param name="p"></param>
    ''' <param name="d"></param>
    ''' <returns></returns>
    <Extension> Public Function Move(ByRef p As Integer, d As Integer) As Integer
        Dim value = p
        p += d
        Return value
    End Function

    ''' <summary>
    ''' 变量<paramref name="p"/>移动距离<paramref name="d"/>然后返回其移动之前的值
    ''' </summary>
    ''' <param name="p"></param>
    ''' <param name="d"></param>
    ''' <returns></returns>
    <Extension> Public Function Move(ByRef p As Double, d As Integer) As Double
        Dim value = p
        p += d
        Return value
    End Function

    ''' <summary>
    ''' 随机的在目标集合中选取指定数目的子集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="Counts">当目标数目大于或者等于目标集合的数目的时候，则返回目标集合</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TakeRandomly(Of T)(Collection As Generic.IEnumerable(Of T), Counts As Integer) As T()
        If Counts >= Collection.Count Then
            Return Collection
        Else
            Dim chunkBuffer As T() = New T(Counts - 1) {}
            Dim OriginalList = Collection.ToList
            For i As Integer = 0 To Counts - 1
                Dim Index = RandomDouble() * (OriginalList.Count - 1)
                chunkBuffer(i) = OriginalList(Index)
                Call OriginalList.RemoveAt(Index)
            Next

            Return chunkBuffer
        End If
    End Function

    ''' <summary>
    ''' Convert target object type collection into a string array using the Object.ToString() interface function.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToStringArray(Of T)(Collection As Generic.IEnumerable(Of T)) As String()
        Dim LQuery = (From item In Collection Let strItem As String = item.ToString Select strItem).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' Get a sub set of the string data which is contains in both collection <paramref name="strArray1"></paramref> and <paramref name="strArray2"></paramref>
    ''' </summary>
    ''' <param name="strArray1"></param>
    ''' <param name="strArray2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Union")>
    <Extension> Public Function Union(strArray1 As String(), strArray2 As String()) As String()
        Dim LQuery = (From strItem As String In strArray1 Where Array.IndexOf(strArray2, strItem) > -1 Select strItem).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    <ExportAPI("Swap")>
    Public Sub Swap(Of T)(ByRef obj1 As T, ByRef obj2 As T)
#Else
    Public Sub Swap(Of T)(ByRef obj1 As T, ByRef obj2 As T)
#End If
        Dim objTemp As T = obj1
        obj1 = obj2
        obj2 = objTemp
    End Sub

    ''' <summary>
    ''' Swap the value in the two variables.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj1"></param>
    ''' <param name="obj2"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub SwapWith(Of T)(ByRef obj1 As T, ByRef obj2 As T)
        Dim objTemp As T = obj1
        obj1 = obj2
        obj2 = objTemp
    End Sub

    ''' <summary>
    ''' Swap the two item position in the target <paramref name="List">list</paramref>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="obj_1"></param>
    ''' <param name="obj_2"></param>
    <Extension> Public Sub SwapItem(Of T As Class)(ByRef List As List(Of T), obj_1 As T, obj_2 As T)
        Dim idx_1 As Integer = List.IndexOf(obj_1)
        Dim idx_2 As Integer = List.IndexOf(obj_2)

        If idx_1 = -1 OrElse idx_2 = -1 Then
            Return
        End If

        Call List.RemoveAt(idx_1)
        Call List.Insert(idx_1, obj_2)
        Call List.RemoveAt(idx_2)
        Call List.Insert(idx_2, obj_2)
    End Sub

    ''' <summary>
    ''' Replace the <see cref="vbCrLf"/> with the specific string. 
    ''' </summary>
    ''' <param name="strText"></param>
    ''' <param name="VbCRLF_Replace"></param>
    ''' <returns></returns>
#If FRAMEWORD_CORE Then
    <ExportAPI("Trim")>
    <Extension> Public Function TrimA(strText As String, <Parameter("vbCrLf.Replaced")> Optional VbCRLF_Replace As String = " ") As String
#Else
    <Extension> Public Function TrimA(strText As String, Optional VbCRLF_Replace As String = " ") As String
#End If
        strText = strText.Replace(vbCrLf, VbCRLF_Replace).Replace(vbCr, VbCRLF_Replace).Replace(vbLf, VbCRLF_Replace)
        strText = strText.Replace("  ", " ")
        Return Strings.Trim(strText)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 为列表中的对象添加对象句柄值
    ''' </summary>
    ''' <param name="source"></param>
    ''' <remarks></remarks>
    <Extension> Public Function [AddHandle](Of THandle As IAddressHandle)(ByRef source As IEnumerable(Of THandle), Optional offset As Integer = 0) As IEnumerable(Of THandle)
        Dim l As Integer = source.Count
        For i As Integer = 0 To l - 1
            source(i).AddrHwnd = i + offset
        Next
        Return source
    End Function
#End If

    ''' <summary>
    ''' <paramref name="p"></paramref> plus one and then return its previous value. (p++)
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MoveNext(ByRef p As Integer) As Integer
        Dim i As Integer = p
        p += 1
        Return i
    End Function

    <Extension> Public Function Increase(ByRef p As Integer) As Integer
        p += 1
        Return p
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Gets the subscript index of a generic collection.(获取某一个集合的下标的集合)
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="Collection">目标集合对象</param>
    ''' <returns>A integer array of subscript index of the target generic collection.</returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Sequence.Index", Info:="Gets the subscript index of a generic collection.")>
    <Extension> Public Function Sequence(Of T)(<Parameter("Data.Collection", "")> Collection As IEnumerable(Of T),
                                               <Parameter("Index.OffSet", "")> Optional OffSet As Integer = 0) _
        As <FunctionReturns("A integer array of subscript index of the target generic collection.")> Integer()
#Else
    ''' <summary>
    ''' 获取某一个集合的下标的集合
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="Collection">目标集合对象</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Sequence(Of T)(Collection As Generic.IEnumerable(Of T), Optional offset As Integer = 0) As Integer()
#End If
        If Collection Is Nothing OrElse Collection.Count = 0 Then
            Return New Integer() {}
        Else
            Dim List(Collection.Count - 1) As Integer
            For i As Integer = 0 To List.Length - 1
                List(i) = i + OffSet
            Next
            Return List
        End If
    End Function

    <Extension> Public Function LongSeq(Of T)(source As IEnumerable(Of T)) As Long()
        If source.IsNullOrEmpty Then
            Return New Long() {}
        Else
            Return CLng(source.Count).LongSeq
        End If
    End Function

    <Extension> Public Function LongSeq(n As Long) As Long()
        Dim array As Long() = New Long(n - 1) {}
        For i As Long = 0 To array.Length - 1
            array(i) = i
        Next
        Return array
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="IndexCollection">所要获取的目标对象的下表的集合</param>
    ''' <param name="reversedSelect">是否为反向选择</param>
    ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("takes")>
    <Extension> Public Function Takes(Of T)(Collection As Generic.IEnumerable(Of T),
                                            IndexCollection As Integer(),
                                            Optional OffSet As Integer = 0,
                                            Optional reversedSelect As Boolean = False) As T()
#Else
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="IndexCollection">所要获取的目标对象的下表的集合</param>
    ''' <param name="reversedSelect">是否为反向选择</param>
    ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Takes(Of T)(Collection As Generic.IEnumerable(Of T), IndexCollection As Integer(), Optional OffSet As Integer = 0, Optional reversedSelect As Boolean = False) As T()
#End If
        If reversedSelect Then
            Return __reversedTakeSelected(Collection, IndexCollection)
        End If

        Dim result As T()

        If OffSet = 0 Then
            result = (From idx As Integer In IndexCollection Select Collection(idx)).ToArray
        Else
            result = (From idx As Integer In IndexCollection Select Collection(idx + OffSet)).ToArray
        End If
        Return result
    End Function

    <Extension> Public Function Takes(Of T)(source As T(), count As Integer) As T()
        Dim ChunkBuffer As T() = New T(count - 1) {}
        Call Array.ConstrainedCopy(source, Scan0, ChunkBuffer, Scan0, count)
        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' 反选，即将所有不出现在<paramref name="indexs"></paramref>之中的元素都选取出来
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="coll"></param>
    ''' <param name="indexs"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function __reversedTakeSelected(Of T)(coll As Generic.IEnumerable(Of T), indexs As Integer()) As T()
        Dim result As T() = (From i As Integer In coll.Sequence Where Array.IndexOf(indexs, i) = -1 Select coll(i)).ToArray
        Return result
    End Function

    ''' <summary>
    ''' 将目标键值对对象的集合转换为一个字典对象
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="remoteDuplicates">当这个参数为False的时候，出现重复的键名会抛出错误，当为True的时候，有重复的键名存在的话，可能会丢失一部分的数据</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToDictionary(Of TKey, TValue)(Collection As IEnumerable(Of KeyValuePair(Of TKey, TValue)),
                                                              Optional remoteDuplicates As Boolean = False) As Dictionary(Of TKey, TValue)
        If remoteDuplicates Then
            Dim hash As Dictionary(Of TKey, TValue) = New Dictionary(Of TKey, TValue)

            For Each x In Collection
                If hash.ContainsKey(x.Key) Then
                    Call Console.WriteLine("  " & "[Duplicated] " & x.Key.ToString)
                Else
                    Call hash.Add(x.Key, x.Value)
                End If
            Next

            Return hash
        Else
            Dim Dictionary As Dictionary(Of TKey, TValue) =
                Collection.ToDictionary(Function(obj) obj.Key, Function(obj) obj.Value)
            Return Dictionary
        End If
    End Function

    ''' <summary>
    ''' This object collection is a null object or contains zero count items.(判断某一个对象集合是否为空)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function IsNullOrEmpty(Of T)(Collection As Generic.IEnumerable(Of T)) As Boolean
        Return Collection Is Nothing OrElse Collection.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of TKey, TValue)(dict As Dictionary(Of TKey, TValue)) As Boolean
        If dict Is Nothing Then
            Return True
        End If
        Return dict.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(queue As Queue(Of T)) As Boolean
        If queue Is Nothing Then
            Return True
        End If
        Return queue.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(Of T)(list As List(Of T)) As Boolean
        If list Is Nothing Then
            Return True
        End If
        Return list.Count = 0
    End Function

    ''' <summary>
    ''' This object array is a null object or contains zero count items.(判断某一个对象数组是否为空)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function IsNullOrEmpty(Of T)(array As T()) As Boolean
        Return array Is Nothing OrElse array.Length = 0
    End Function

    ''' <summary>
    ''' 判断这个字符串集合是否为空集合，函数会首先按照常规的集合为空进行判断，然后假若不为空的话，假若只含有一个元素并且该唯一的元素的值为空字符串，则也认为这个字符串集合为空集合
    ''' </summary>
    ''' <param name="stringCollection"></param>
    ''' <param name="strict">FALSE 为非严谨，只进行常规判断，TRUE 为严谨模式，会假若不为空的话，假若只含有一个元素并且该唯一的元素的值为空字符串，则也认为这个字符串集合为空集合</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("IsNullOrEmpty")>
    <Extension> Public Function IsNullOrEmpty(stringCollection As Generic.IEnumerable(Of String), strict As Boolean) As Boolean
        If Not strict Then Return stringCollection.IsNullOrEmpty

        If stringCollection.IsNullOrEmpty Then
            Return True
        Else
            Return (stringCollection.Count = 1 AndAlso String.IsNullOrEmpty(stringCollection.First))
        End If
    End Function

    ''' <summary>
    ''' Write the text file data into a file which was specific by the <paramref name="Path"></paramref> value, 
    ''' this function not append the new data onto the target file.
    ''' (将目标文本字符串写入到一个指定路径的文件之中，但是不会在文件末尾追加新的数据)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="TextValue"></param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(<Parameter("Text")> TextValue As String, Path As String, Optional Encoding As Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(Path) Then Return False
        If Encoding Is Nothing Then Encoding = System.Text.Encoding.Default
        Dim Dir As String
        Try
            Path = ProgramPathSearchTool.Long2Short(Path)
            Dir = FileIO.FileSystem.GetParentPath(Path)
        Catch ex As Exception
            Dim MSG As String = $" **** Directory string is illegal or string is too long:  [{NameOf(Path)}:={Path}] > 260".__DEBUG_ECHO
            Throw New Exception(MSG, ex)
        End Try

        If String.IsNullOrEmpty(Dir) Then
            Dir = FileIO.FileSystem.CurrentDirectory
        End If

        Try
            Call FileIO.FileSystem.CreateDirectory(Dir)
            Call FileIO.FileSystem.WriteAllText(Path, TextValue, append:=False, encoding:=Encoding)
        Catch ex As Exception
            ex = New Exception("[DIR]  " & Dir, ex)
            ex = New Exception("[Path]  " & Path, ex)
            Throw ex
        End Try

        Return True
    End Function

    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(value As XElement, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return value.Value.SaveTo(path, encoding)
    End Function

    ''' <summary>
    ''' 由于可能会产生数据污染，所以并不推荐使用这个函数来写文件
    ''' </summary>
    ''' <param name="TextValue"></param>
    ''' <param name="Path"></param>
    ''' <param name="WaitForRelease">当其他的进程对目标文件产生占用的时候，函数是否等待其他进程的退出释放文件句柄之后在进行数据的写入</param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(TextValue As String, Path As String, WaitForRelease As Boolean, Optional Encoding As System.Text.Encoding = Nothing) As Boolean
        If Path.FileOpened Then
            If WaitForRelease Then
                '假若文件被占用，则等待句柄的释放
                Do While Path.FileOpened
                    Call Threading.Thread.Sleep(10)
                Loop
            Else  '假若不等待句柄的释放的话，则直接返回失败
                Return False
            End If
        End If

        Return TextValue.SaveTo(Path, Encoding)
    End Function

    ''' <summary>
    ''' 判断是否是文本文件
    ''' </summary>
    ''' <param name="FilePath">文件全路径名称</param>
    ''' <returns>是返回True，不是返回False</returns>
    ''' <param name="ChunkLength">文件检查的长度，假若在这个长度内都没有超过null的阈值数，则认为该文件为文本文件，默认区域长度为4KB</param>
    ''' <remarks>2012年12月5日</remarks>
    ''' 
    <ExportAPI("IsTextFile")>
    <Extension> Public Function IsTextFile(FilePath As String, Optional ChunkLength As Integer = 4 * 1024) As Boolean
        Dim file As IO.FileStream = New System.IO.FileStream(FilePath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim byteData(1) As Byte
        Dim i As Integer
        Dim p As Integer

        While file.Read(byteData, 0, byteData.Length) > 0
            If byteData(0) = 0 Then i += 1
            If p <= ChunkLength Then p += 1 Else Exit While
        End While

        Return i <= 0.1 * p
    End Function

    ''' <summary>
    ''' 将目标字符串数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(array As IEnumerable(Of String), path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(path) Then Return False
        If encoding Is Nothing Then encoding = System.Text.Encoding.Default
        Dim Dir = FileIO.FileSystem.GetParentPath(path)
        Call FileIO.FileSystem.CreateDirectory(Dir)
        Call IO.File.WriteAllLines(path, array, encoding)
        Return True
    End Function

    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(sBuilder As StringBuilder, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return sBuilder.ToString.SaveTo(path, encoding)
    End Function

    <ExportAPI("CopyFile", Info:="kernel32.dll!CopyFileW")>
    <DllImport("kernel32.dll", EntryPoint:="CopyFileW", CharSet:=CharSet.Unicode, ExactSpelling:=False)>
    Public Function CopyFile(lpExistingFilename As String, lpNewFileName As String, bFailIfExists As Boolean) As Boolean
    End Function

    ''' <summary>
    ''' 默认是加载Xml文件的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="file"></param>
    ''' <param name="encoding"></param>
    ''' <param name="parser">default is Xml parser</param>
    ''' <param name="ThrowEx"></param>
    ''' <returns></returns>
    <Extension> Public Function LoadTextDoc(Of T As ITextFile)(file As String,
                                                               Optional encoding As Encoding = Nothing,
                                                               Optional parser As Func(Of String, Encoding, T) = Nothing,
                                                               Optional ThrowEx As Boolean = True) As T
        If parser Is Nothing Then
            parser = AddressOf LoadXml
        End If

        Dim FileObj As T

        Try
            FileObj = parser(file, encoding)
            FileObj.FilePath = file
        Catch ex As Exception
            Call App.LogException(New Exception(file.ToFileURL, ex))

            If ThrowEx Then
                Throw ex
            Else
#If DEBUG Then
                Call ex.PrintException
#End If
                Return Nothing
            End If
        End Try

        Return FileObj
    End Function

    ''' <summary>
    ''' 使用二进制序列化保存一个对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Serialize(Of T As Class)(obj As T, path As String) As Integer
        Dim Stream As Stream = New IO.FileStream(path, IO.FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)
        Dim buffer As Byte() = obj.GetSerializeBuffer
        Call Stream.Write(buffer, Scan0, buffer.Length)
        Call Stream.Flush()
        Call Stream.Close()
        Return 0
    End Function

    <Extension> Public Function GetSerializeBuffer(Of T As Class)(obj As T) As Byte()
        Dim IFormatter As IFormatter = New BinaryFormatter()
        Dim Stream As New IO.MemoryStream()
        Call IFormatter.Serialize(Stream, obj)
        Dim buffer As Byte() = New Byte(Stream.Length - 1) {}
        Call Stream.Read(buffer, Scan0, buffer.Length)
        Return buffer
    End Function

    <Extension> Public Function DeSerialize(Of T As Class)(bytes As Byte()) As T
        Dim obj As Object = (New BinaryFormatter).[Deserialize](New MemoryStream(bytes))
        Return DirectCast(obj, T)
    End Function

    ''' <summary>
    ''' 使用反二进制序列化从指定的文件之中加载一个对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Load(Of T As Class)(path As String) As T
        If Not FileIO.FileSystem.FileExists(path) Then
            Return DirectCast(Activator.CreateInstance(Of T)(), T)
        End If
        Using Stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim IFormatter As IFormatter = New BinaryFormatter()
            Dim obj As T = DirectCast(IFormatter.Deserialize(Stream), T)
            Return obj
        End Using
    End Function

    ''' <summary>
    ''' 0 for null object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <returns></returns>
    <Extension> Public Function GetLength(Of T)(array As T()) As Integer
        If array Is Nothing Then
            Return 0
        Else
            Return array.Length
        End If
    End Function

    <Extension> Public Function GetLength(Of T)(collect As Generic.IEnumerable(Of T)) As Integer
        If collect Is Nothing Then
            Return 0
        Else
            Return collect.Count
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 执行一个命令行语句，并返回一个IO重定向对象，以获取被执行的目标命令的标准输出
    ''' </summary>
    ''' <param name="CommandLine"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Shell")>
    <Extension> Public Function Shell(CommandLine As String) As Microsoft.VisualBasic.CommandLine.IORedirect
        Return CType(CommandLine, Microsoft.VisualBasic.CommandLine.IORedirect)
    End Function
#End If

    ''' <summary>
    ''' 获取一个实数集合中所有元素的积
    ''' </summary>
    ''' <param name="Elements"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("PI")>
    <Extension> Public Function π(Elements As Generic.IEnumerable(Of Double)) As Double
        If Elements.IsNullOrEmpty Then
            Return 0
        End If

        Dim result As Double = 1
        For i As Integer = 0 To Elements.Count - 1
            result *= Elements(i)
        Next

        Return result
    End Function

#If FRAMEWORD_CORE Then

    <Extension> Public Sub ClearParameters(Of InteropService As Microsoft.VisualBasic.CommandLine.InteropService)(Instance As InteropService)
        Call CommandLine.CLIBuildMethod.ClearParameters(Instance)
    End Sub

    ''' <summary>
    ''' Fill the newly created image data with the specific color brush
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <param name="FilledColor"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub FillBlank(ByRef Image As System.Drawing.Image, FilledColor As System.Drawing.Brush)
        If Image Is Nothing Then
            Return
        End If
        Using gr As Graphics = Graphics.FromImage(Image)
            Dim R As System.Drawing.Rectangle = New Rectangle(New Point, Image.Size)
            Call gr.FillRectangle(FilledColor, R)
        End Using
    End Sub
#End If

    Public Const Null As Object = Nothing

    ''' <summary>
    ''' Remove all of the element in the <paramref name="collection"></paramref> from target <paramref name="List">list</paramref>
    ''' </summary> 
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="collection"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub Removes(Of T)(ByRef List As List(Of T), collection As Generic.IEnumerable(Of T))
        For Each obj In collection
            Call List.Remove(obj)
        Next
    End Sub

    <Extension> Public Function RemoveLast(Of T)(ByRef list As List(Of T)) As List(Of T)
        If list.IsNullOrEmpty OrElse list.Count = 1 Then
            list = New List(Of T)
        Else
            Dim i As Integer = list.Count - 1
            Call list.RemoveAt(i)
        End If

        Return list
    End Function

    <Extension> Public Function RemoveFirst(Of T)(ByRef list As List(Of T)) As List(Of T)
        If list.IsNullOrEmpty OrElse list.Count = 1 Then
            list = New List(Of T)
        Else
            Call list.RemoveAt(Scan0)
        End If

        Return list
    End Function
End Module
