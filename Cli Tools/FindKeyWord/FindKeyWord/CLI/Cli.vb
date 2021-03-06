﻿Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("TextFile.Utilities", Category:=APICategories.CLI_MAN,
                  Url:="",
                  Description:="Utilities tool for the text file, including finding keywords, large size text file viewer.",
                  Publisher:="xie.guigang@gmail.com")>
Module CLI

    <ExportAPI("Peeks", Usage:="Peeks /in <input.txt> [/length 1024 /out <out.txt>]")>
    Public Function Peeks(args As CommandLine.CommandLine) As Integer
        Dim Input As String = args("/in")
        Dim Out As String = args("/out")
        Dim Len As Integer = args.GetValue("/length", 1024)

        If String.IsNullOrEmpty(Out) Then
            Out = Input.TrimFileExt & ".out.txt"
        End If

        Call Microsoft.VisualBasic.Peeks(Input, Len).SaveTo(Out)

        Return App.Exit(0)
    End Function

    <ExportAPI("Find", Usage:="Find /regex /filtering --key <expression> [--dir <dir> --ext <ext_list>]")>
    Public Function Found(argvs As CommandLine.CommandLine) As Integer
        Dim Key As String = argvs("--key")
        Dim DIR As String = argvs("--dir")
        Dim Ext As String = argvs("--ext")
        Dim Regex As Boolean = argvs.GetBoolean("/regex")
        Dim FilteringExt As Boolean = argvs.GetBoolean("/filtering")

        If String.IsNullOrEmpty(Key) Then
            Call Console.WriteLine("Please input a not null keyword/regex expression.")
            Return -10
        End If

        If String.IsNullOrEmpty(DIR) Then
            DIR = FileIO.FileSystem.CurrentDirectory
        End If

        Dim Result = Found(Keyword:=Key,
                           Dir:=DIR,
                           FilteringExt:=FilteringExt,
                           _extList:=Ext,
                           _usingRegex:=Regex,
                           Process:=Sub(out, percentage) Call out.__DEBUG_ECHO)
        Dim Output As String = String.Join(vbCrLf, (From item In Result Select item.GenerateOutput).ToArray)

        Call Console.WriteLine(Output)

        Return 0
    End Function

    Public Class FoundResult
        Public Property File As String
        Public Property Index As FileIndex()

        Public Function GenerateOutput() As String
            Dim File As String = Me.File.CliPath
            Dim LQuery = (From idx In Index Select $"{File}{vbTab}{idx.Line}{vbTab}{idx.TextLine }")
            Return String.Join(vbCrLf, LQuery)
        End Function
    End Class

    Public Structure FileIndex
        Public Property Line As Integer
        Public Property TextLine As String
    End Structure

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Keyword"></param>
    ''' <param name="Dir"></param>
    ''' <param name="_extList">a.ext;b.ext;c.ext</param>
    ''' <param name="_usingRegex"></param>
    ''' <param name="FilteringExt"></param>
    ''' <returns></returns>
    Public Function Found(Keyword As String,
                          Dir As String,
                          _extList As String,
                          _usingRegex As Boolean,
                          FilteringExt As Boolean,
                          Optional Process As Action(Of String, Integer) = Nothing) As FoundResult()

        If Process Is Nothing Then
            Process = AddressOf __emptyAction
        End If

        Call Process("Scanning for files...", 1)

        Dim Files = FileIO.FileSystem.GetFiles(Dir, FileIO.SearchOption.SearchAllSubDirectories).ToArray
        Dim ExtList As String()
        Dim FileNumbers As Long = Files.Count

        If Not String.IsNullOrEmpty(_extList) Then
            ExtList = (From s In _extList.Trim.Split(";"c) Select s.Trim.Split("."c).Last.ToLower).ToArray
        Else
            ExtList = New String() {}
        End If

        If FilteringExt Then  '出现的文件名后缀都过滤掉
            Files = (From file As String In Files.AsParallel
                     Let ext = file.Split("."c).Last.ToLower
                     Where Array.IndexOf(ExtList, ext) = -1
                     Select file).ToArray
        Else '只搜索指定的后缀名

            If Not ExtList.IsNullOrEmpty Then
                Files = (From file As String In Files.AsParallel
                         Let ext = file.Split("."c).Last.ToLower
                         Where Array.IndexOf(ExtList, ext) > -1
                         Select file).ToArray
            End If

        End If

        Dim FoundResult As FoundResult()

        Call Process("Pre-processing files...", 1)

        Dim GetFiles As String() = (From path As String In Files.AsParallel
                                    Where path.IsTextFile
                                    Select path).ToArray
        Dim percentage As Integer
        Dim sw = Stopwatch.StartNew

        If _usingRegex Then  '使用正则表达式
            FoundResult = (From File As String In GetFiles.AsParallel
                           Let result = __foundRegex(percentage, GetFiles.Length, File, Keyword, Process)
                           Where Not result Is Nothing
                           Select result).ToArray
        Else
            FoundResult = (From File As String In GetFiles.AsParallel
                           Let result = __foundKeyMatch(percentage, GetFiles.Length, File, Keyword, Process)
                           Where Not result Is Nothing
                           Select result).ToArray
        End If

        Call Process($"Search job done: Processing {FileNumbers} files，using time {sw.ElapsedMilliseconds}ms!", 100)

        Return FoundResult
    End Function

    Private Function __foundRegex(ByRef percentage As Integer, Counts As Integer,
                                  File As String,
                                  Keyword As String,
                                  Process As Action(Of String, Integer)) As FoundResult

        Dim ChunkBuffer As String = IO.File.ReadAllText(File)
        Dim Find As String() = (From m As Match
                                In Regex.Matches(ChunkBuffer, Keyword, RegexOptions.Singleline)
                                Select m.Value).ToArray

        Call Threading.Interlocked.Increment(percentage)
        Call Process($"{File.ToFileURL} searched...", 100 * percentage / Counts)

        If Find.Length > 0 Then
            Return New FoundResult With {
                .File = File,
                .Index = (From s As String
                          In Find
                          Select New FileIndex With {
                              .Line = InStr(ChunkBuffer, s),
                              .TextLine = s}).ToArray
            }
        Else
            Return Nothing
        End If
    End Function

    Private Function __foundKeyMatch(ByRef percentage As Integer, Counts As Integer,
                                     File As String,
                                     Keyword As String,
                                     Process As Action(Of String, Integer)) As FoundResult

        Dim ChunkBuffer As String() = IO.File.ReadAllLines(File)
        Dim Find As FileIndex() = (From i As Integer In ChunkBuffer.Sequence
                                   Let Line As String = ChunkBuffer(i)
                                   Where Not String.IsNullOrEmpty(Line) AndAlso InStr(Line, Keyword, CompareMethod.Text) > 0
                                   Select New FileIndex With {.Line = i, .TextLine = Line}).ToArray

        Call Threading.Interlocked.Increment(percentage)
        Call Process($"{File.ToFileURL} searched...", 100 * percentage / Counts)

        If Find.Length > 0 Then
            Return New FoundResult With {
                .File = File,
                .Index = Find
            }
        Else
            Return Nothing
        End If
    End Function

    Private Sub __emptyAction(process As String, percentage As Integer)
        ' DO_NOTHING
    End Sub

    <ExportAPI("/Tails", Usage:="/Tails /in <in.txt> [/len 1024 /out <out.txt>]")>
    Public Function Tails(args As CommandLine.CommandLine) As Integer
        Dim inFile As String = args("/in")
        Dim out As String = args("/out")
        Dim len As Integer = args.GetValue("/len", 1024)
        Dim value As String = Microsoft.VisualBasic.Tails(inFile, len)

        If String.IsNullOrEmpty(out) Then
            Call Console.WriteLine()
            Call Console.WriteLine(value)
        Else
            Call value.SaveTo(out)
        End If

        Return 0
    End Function
End Module
