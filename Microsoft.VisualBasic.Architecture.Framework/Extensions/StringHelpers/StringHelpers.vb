﻿Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ConsoleDevice
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions

<PackageNamespace("StringHelpers", Publisher:="amethyst.asuka@gcmodeller.org", Url:="http://gcmodeller.org")>
Public Module StringHelpers

    <ExportAPI("ZeroFill")>
    Public Function ZeroFill(n As String, len As Integer) As String
        Return STDIO__.I_FormatProvider.d.ZeroFill(n, len)
    End Function

    <ExportAPI("s.Parts")>
    Public Function Parts(s As String, len As String) As String
        Dim sbr As New StringBuilder
        Call Parts(s, len, sbr)
        Return sbr.ToString
    End Function

    Public Sub Parts(s As String, len As String, ByRef sbr As StringBuilder)
        Do While Not String.IsNullOrEmpty(s)
            Call sbr.Append(Mid(s, 1, len))
            s = Mid(s, len + 1)
            If String.IsNullOrEmpty(s) Then
                Return
            End If
            Dim fs As Integer = InStr(s, " ")

            If fs = 0 Then
                Call sbr.AppendLine(s)
                Return
            End If

            Dim Firts As String = Mid(s, 1, fs - 1)
            s = Mid(s, fs + 1)
            Call sbr.AppendLine(Firts)
        Loop
    End Sub

    Const REGEX_EMAIL As String = "[a-z0-9\._-]+@[a-z0-9\._-]+"
    Const REGEX_URL As String = "(ftp|http(s)?)[:]//[a-z0-9\.-_]+\.[a-z]+/*[^""]*"

    <ExportAPI("Parsing.E-Mails")>
    Public Function GetEMails(s As String) As String()
        Dim values As String() = Regex.Matches(s, REGEX_EMAIL, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray
        Return values
    End Function

    <ExportAPI("Parsing.URLs")>
    Public Function GetURLs(s As String) As String()
        Dim values As String() = Regex.Matches(s, REGEX_URL, RegexOptions.IgnoreCase Or RegexOptions.Singleline).ToArray
        Return values
    End Function

    ''' <summary>
    ''' 计数在字符串之中所出现的指定的字符的出现的次数
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="ch"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Count", Info:="Counting the specific char in the input string value.")>
    <Extension> Public Function Count(str As String, ch As Char) As Integer
        If String.IsNullOrEmpty(str) Then
            Return 0
        End If

        Dim LQuery = (From chr As Char In str Where ch = chr Select 1).ToArray
        Return LQuery.Length
    End Function

    ''' <summary>
    ''' 获取""或者其他字符所包围的字符串的值
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="wrapper"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Wrapper.Trim")>
    <Extension> Public Function GetString(s As String, Optional wrapper As Char = """"c) As String
        If String.IsNullOrEmpty(s) OrElse Len(s) = 1 Then
            Return s
        End If
        If s.First = wrapper AndAlso s.Last = wrapper Then
            Return Mid(s, 2, Len(s) - 2)
        Else
            Return s
        End If
    End Function

    <ExportAPI("FormatZero")>
    <Extension> Public Function FormatZero(n As Integer, Optional fill As String = "00") As String
        Dim s = n.ToString
        Dim d = Len(fill) - Len(s)

        If d < 0 Then
            Return s
        Else
            Return Mid(fill, 1, d) & s
        End If
    End Function

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <param name="Chunkbuffer"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Intersection")>
    <Extension> Public Function Intersection(Chunkbuffer As Generic.IEnumerable(Of Generic.IEnumerable(Of String))) As String()
        Chunkbuffer = (From line In Chunkbuffer Select (From strValue As String In line Select strValue Distinct Order By strValue Ascending).ToArray).ToArray
        Dim Union As List(Of String) = New List(Of String)
        For Each Line As String() In Chunkbuffer
            Call Union.AddRange(Line)
        Next
        Union = (From strValue As String In Union Select strValue Distinct Order By strValue Ascending).ToList  '获取并集，接下来需要从并集之中去除在两个集合之中都不存在的
        For Each Line In Chunkbuffer
            For Each Collection In Chunkbuffer       '遍历每一个集合
                Dim LQuery = (From strvalue As String In Collection Where Array.IndexOf(Line, strvalue) = -1 Select strvalue).ToArray
                For Each value As String In LQuery
                    Call Union.Remove(value) '假若line之中存在不存在的元素，则从并集之中移除
                Next
            Next
        Next
        Return Union.ToArray
    End Function

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Intersection")>
    Public Function Intersection(ParamArray values As String()()) As String()
        Return values.Intersection
    End Function

    <ExportAPI("Matched?")>
    <Extension> Public Function Matches(str As String, regex As String) As Boolean
        Return System.Text.RegularExpressions.Regex.Match(str, regex).Success
    End Function

    ''' <summary>
    ''' Searches the specified input string for the first occurrence of the specified regular expression.
    ''' </summary>
    ''' <param name="input">The string to search for a match.</param>
    ''' <param name="pattern">The regular expression pattern to match.</param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    <ExportAPI("Regex", Info:="Searches the specified input string for the first occurrence of the specified regular expression.")>
    <Extension> Public Function Match(<Parameter("input", "The string to search for a match.")> input As String,
                                      <Parameter("Pattern", "The regular expression pattern to match.")> pattern As String,
                                      Optional options As System.Text.RegularExpressions.RegexOptions = RegularExpressions.RegexOptions.Multiline) As String
        Return System.Text.RegularExpressions.Regex.Match(input, pattern, options).Value
    End Function

    <ExportAPI("Match")>
    <Extension> Public Function Match(input As System.Text.RegularExpressions.Match,
                                      pattern As String,
                                      Optional options As System.Text.RegularExpressions.RegexOptions = RegularExpressions.RegexOptions.Multiline) As String
        Return System.Text.RegularExpressions.Regex.Match(input.Value, pattern, options).Value
    End Function

    <Extension>
    <ExportAPI("Write.Dictionary")>
    Public Function SaveTo(dict As IDictionary(Of String, String), path As String) As Boolean
        Dim values As String() = dict.ToArray(Function(x) x.Key & vbTab & x.Value)
        Return values.SaveTo(path)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Count the string value numbers.(请注意，这个函数是倒序排序的)
    ''' </summary>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Tokens.Count", Info:="Count the string value numbers.")>
    <Extension> Public Function CountStringTokens(Collection As Generic.IEnumerable(Of String), Optional IgnoreCase As Boolean = False) As KeyValuePair(Of String, Integer)()
#Else
    <Extension> Public Function CountStringTokens(Collection As Generic.IEnumerable(Of String), Optional IgnoreCase As Boolean = False) As KeyValuePair(Of String, Integer)()
#End If

        If Not IgnoreCase Then '大小写敏感
            Dim GroupList = (From s As String In Collection Select s Group s By s Into Group).ToArray
            Dim ChunkBuffer = (From item In GroupList Select data = New KeyValuePair(Of String, Integer)(item.s, item.Group.Count) Order By data.Value Descending).ToArray
            Return ChunkBuffer
        End If

        Dim Uniques = (From s As String
                       In (From strValue As String In Collection Select strValue Distinct).ToArray
                       Let data As String = s
                       Select UNIQUE_KEY = s.ToLower, data
                       Group By UNIQUE_KEY Into Group).ToArray
        Dim ChunkList As List(Of KeyValuePair(Of String, Integer)) = New List(Of KeyValuePair(Of String, Integer))

        Dim LQuery = (From UniqueString In Uniques
                      Let s As String = UniqueString.UNIQUE_KEY
                      Let Count As Integer = (From strValue As String In Collection Where String.Equals(strValue, s, StringComparison.OrdinalIgnoreCase) Select 1).ToArray.Length
                      Let ori = (From nn In UniqueString.Group Select nn.data).ToArray
                      Let DataItem As KeyValuePair(Of String, Integer) = New KeyValuePair(Of String, Integer)(ori((UniqueString.Group.Count - 1) * Rnd()).ToString, Count)
                      Select DataItem
                      Order By DataItem.Value Descending).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' This method is used to replace most calls to the Java String.split method.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="regexDelimiter"></param>
    ''' <param name="trimTrailingEmptyStrings"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("StringsSplit", Info:="This method is used to replace most calls to the Java String.split method.")>
    <Extension> Public Function StringSplit(Source As String, RegexDelimiter As String, Optional TrimTrailingEmptyStrings As Boolean = False) As String()
        Dim splitArray As String() = System.Text.RegularExpressions.Regex.Split(Source, RegexDelimiter)

        If Not TrimTrailingEmptyStrings OrElse splitArray.Length <= 1 Then Return splitArray

        For i As Integer = splitArray.Length To 1 Step -1

            If splitArray(i - 1).Length > 0 Then
                If i < splitArray.Length Then
                    Call System.Array.Resize(splitArray, i)
                End If

                Exit For
            End If
        Next

        Return splitArray
    End Function

    ''' <summary>
    ''' String compares using <see cref="system.String.Equals"/>, if the target value could not be located, then -1 will be return from this function.  
    ''' </summary>
    ''' <param name="collection"></param>
    ''' <param name="Text"></param>
    ''' <param name="caseSensitive"></param>
    ''' <returns></returns>
    <ExportAPI("Located", Info:="String compares using String.Equals")>
    <Extension> Public Function Located(collection As Generic.IEnumerable(Of String), Text As String, Optional caseSensitive As Boolean = True) As Integer
        Dim Method = If(caseSensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim Len As Integer = collection.Count - 1
        Dim array = collection.ToArray '为了保证性能的需要，这里的代码会比较复杂

        For i As Integer = 0 To Len
            If String.Equals(array(i), Text, Method) Then
                Return i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Search the string by keyword in a string collection. Unlike search function <see cref="StringHelpers.Located(IEnumerable(Of String), String, Boolean)"/> 
    ''' using function <see cref="String.Equals"/> function to search string, this function using <see cref="Strings.InStr(String, String, CompareMethod)"/> 
    ''' to search the keyword.
    ''' </summary>
    ''' <param name="collection"></param>
    ''' <param name="keyword"></param>
    ''' <param name="caseSensitive"></param>
    ''' <returns></returns>
    <ExportAPI("Lookup", Info:="Search the string by keyword in a string collection.")>
    <Extension>
    Public Function Lookup(collection As Generic.IEnumerable(Of String), keyword As String, Optional caseSensitive As Boolean = True) As Integer
        Dim Method = If(caseSensitive, CompareMethod.Binary, CompareMethod.Text)
        Dim Len As Integer = collection.Count - 1
        Dim array = collection.ToArray '为了保证性能的需要，这里的代码会比较复杂

        For i As Integer = 0 To Len
            If InStr(array(i), keyword, Method) > 0 Then
                Return i
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' 判断目标字符串是否与字符串参数数组之中的任意一个字符串相等，大小写不敏感，假若没有相等的字符串，则会返回空字符串，假若找到了相等的字符串，则会返回该字符串
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="compareTo"></param>
    ''' <returns></returns>
    <Extension>
    <ExportAPI("Equals.Any")>
    Public Function EqualsAny(source As String, ParamArray compareTo As String()) As String
        Dim index As Integer = compareTo.Located(source, False)
        If index = -1 Then
            Return ""
        Else
            Return compareTo(index)
        End If
    End Function

    ''' <summary>
    ''' 查找到任意一个既返回位置，大小写不敏感
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="find"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("InStr.Any")>
    <Extension> Public Function InStrAny(source As String, ParamArray find As String()) As Integer
        For Each Token As String In find
            Dim idx As Integer = Strings.InStr(source, Token, CompareMethod.Text)
            If idx > 0 Then
                Return idx
            End If
        Next

        Return 0
    End Function

    ''' <summary>
    ''' 函数对文本进行分行操作，由于在Windows(<see cref="VbCrLf"/>)和Linux(<see cref="vbCr"/>, <see cref="vbLf"/>)平台上面所生成的文本文件的换行符有差异，所以可以使用这个函数来进行统一的分行操作
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("lTokens")>
    <Extension> Public Function lTokens(text As String) As String()
        If String.IsNullOrEmpty(text) Then
            Return New String() {}
        End If

        Dim lf As Boolean = InStr(text, vbLf) > 0
        Dim cr As Boolean = InStr(text, vbCr) > 0

        If Not (cr OrElse lf) Then  ' 没有分行符，则只有一行
            Return New String() {text}
        End If

        If lf AndAlso cr Then
            text = text.Replace(vbCr, "")
            Return Strings.Split(text, vbLf)
        End If

        If lf Then
            Return Strings.Split(text, vbLf)
        End If

        Return Strings.Split(text, vbCr)
    End Function

    <ExportAPI("IsNullOrEmpty")>
    Public Function IsNullOrEmpty(values As Generic.IEnumerable(Of String)) As Boolean
        If values.IsNullOrEmpty Then
            Return True
        End If

        If values.Count = 1 AndAlso String.IsNullOrEmpty(values.First) Then
            Return True
        End If

        Return False
    End Function

    <ExportAPI("As.Array")>
    <Extension> Public Function ToArray(source As MatchCollection) As String()
        Dim LQuery As String() = (From m As Match
                                  In source
                                  Select m.Value).ToArray
        Return LQuery
    End Function

    <Extension> Public Function ToArray(Of T)(source As MatchCollection, [CType] As Func(Of String, T)) As T()
        Dim LQuery As T() = (From m As Match
                             In source
                             Let s As String = m.Value
                             Select [CType](s)).ToArray
        Return LQuery
    End Function
End Module
