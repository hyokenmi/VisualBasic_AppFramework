﻿
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SecurityString

    <PackageNamespace("Md5Hash", Publisher:="Microsoft Corp.", Description:="Represents the abstract class from which all implementations of the System.Security.Cryptography.MD5 hash algorithm inherit.")>
    Public Module MD5Hash

        ''' <summary>
        ''' 并行化的需求
        ''' </summary>
        ''' <remarks></remarks>
        Private Class __md5HashProvider

            ReadOnly md5Hash As Security.Cryptography.MD5 =
                Security.Cryptography.MD5.Create()

            Public Function GetMd5Hash(input As String) As String
                If String.IsNullOrEmpty(input) Then
                    Return ""
                End If

                Dim data As Byte() = Encoding.UTF8.GetBytes(input)  ' Convert the input string to a byte array and compute the hash. 
                Return GetMd5Hash(input:=data)
            End Function

            Public Function GetMd5Hash(input As Byte()) As String
                If input.IsNullOrEmpty Then
                    Return ""
                End If

                Dim data As Byte() = md5Hash.ComputeHash(input)    ' Convert the input string to a byte array and compute the hash. 
                ' Create a new Stringbuilder to collect the bytes 
                ' and create a string. 
                Dim sBuilder As New StringBuilder()

                ' Loop through each byte of the hashed data  
                ' and format each one as a hexadecimal string. 
                For i As Integer = 0 To data.Length - 1
                    sBuilder.Append(data(i).ToString("x2"))
                Next i

                Return sBuilder.ToString() ' Return the hexadecimal string. 
            End Function
        End Class

        <ExportAPI("Uid")>
        Public Function NewUid() As String
            Dim input As String = Guid.NewGuid.ToString & Now.ToString
            Return GetMd5Hash(input)
        End Function

        <ExportAPI("Md5")>
        Public Function GetMd5Hash(input As String) As String
            Return New __md5HashProvider().GetMd5Hash(input)
        End Function

        ''' <summary>
        ''' Gets the hashcode of the input string. (<paramref name="input"/> => <see cref="MD5Hash.GetMd5Hash"/> => <see cref="MD5Hash.ToLong(String)"/>)
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        <ExportAPI("GetHashCode", Info:="Gets the hashcode of the input string.")>
        Public Function GetHashCode(input As String) As Long
            Dim md5 As String = MD5Hash.GetMd5Hash(input)
            Return ToLong(md5)
        End Function

        ''' <summary>
        ''' Gets the hashcode of the input string.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("GetHashCode", Info:="Gets the hashcode of the input string.")>
        Public Function GetHashCode(data As Generic.IEnumerable(Of Byte)) As Long
            Return GetMd5Hash(data.ToArray).ToLong
        End Function

        ''' <summary>
        ''' Gets the hashcode of the md5 string.
        ''' </summary>
        ''' <param name="md5">计算所得到的MD5哈希值</param>
        ''' <returns></returns>
        <ExportAPI("As.Long", Info:="Gets the hashcode of the md5 string.")>
        <Extension> Public Function ToLong(md5 As String) As Long
            Dim bytes = StringToByteArray(md5)
            Return ToLong(bytes)
        End Function

        <ExportAPI("As.Long")> <Extension>
        Public Function ToLong(bytes As Byte()) As Long
            Dim result As Long

            For i As Integer = 0 To bytes.Length - 1
                result += bytes(i) ^ (i / 2.5 + 1.5)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 由于md5是大小写无关的，故而在这里都会自动的被转换为小写形式，所以调用这个函数的时候不需要在额外的转换了
        ''' </summary>
        ''' <param name="hex"></param>
        ''' <returns></returns>
        <ExportAPI("As.Bytes")>
        Public Function StringToByteArray(hex As String) As Byte()
            Dim NumberChars As Integer = hex.Length
            Dim bytes As Byte() = New Byte(NumberChars / 2 - 1) {}

            hex = hex.ToLower  ' MD5是大小写无关的

            For i As Integer = 0 To NumberChars - 2 Step 2
                bytes(i / 2) = Convert.ToByte(hex.Substring(i, 2), 16)
            Next
            Return bytes
        End Function

        <ExportAPI("Md5")>
        Public Function GetMd5Hash(input As Byte()) As String
            Return New __md5HashProvider().GetMd5Hash(input)
        End Function

        ''' <summary>
        ''' Verify a hash against a string. 
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="comparedHash"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Md5.Verify", Info:="Verify a hash against a string.")>
        Public Function VerifyMd5Hash(input As String, comparedHash As String) As Boolean
            If String.IsNullOrEmpty(input) OrElse String.IsNullOrEmpty(comparedHash) Then
                Return False
            End If

            Dim hashOfInput As String = GetMd5Hash(input)  ' Hash the input. 
            Return String.Equals(hashOfInput, comparedHash, StringComparison.OrdinalIgnoreCase)
        End Function 'VerifyMd5Hash

        ''' <summary>
        ''' 校验两个文件的哈希值是否一致
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="subject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("File.Equals")>
        Public Function VerifyFile(query As String, subject As String) As Boolean
            Return String.Equals(GetFileHashString(query), GetFileHashString(subject))
        End Function

        ''' <summary>
        ''' Get the md5 hash calculation value for a specific file.(获取文件对象的哈希值，请注意，当文件不存在或者文件的长度为零的时候，会返回空字符串)
        ''' </summary>
        ''' <param name="PathUri">The file path of the target file to be calculated.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("File.Md5", Info:="Get the md5 hash calculation value for a specific file.")>
        Public Function GetFileHashString(<Parameter("Path.Uri", "The file path of the target file to be calculated.")> PathUri As String) As String
            If Not PathUri.FileExists OrElse FileIO.FileSystem.GetFileInfo(PathUri).Length = 0 Then
                Return ""
            End If

            Dim ChunkBuffer As Byte() = IO.File.ReadAllBytes(PathUri)
            Return GetMd5Hash(ChunkBuffer)
        End Function

        ''' <summary>
        ''' SHA256 8 bits salt value for the private key.
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <ExportAPI("SaltValue", Info:="SHA256 8 bits salt value for the private key.")>
        Public Function SaltValue(value As String) As String
            Dim hash As String = GetMd5Hash(value)
            Dim chars As Char() = New Char() {hash(0), hash(1), hash(3), hash(5), hash(15), hash(23), hash(28), hash(31)}
            Return New String(chars)
        End Function
    End Module
End Namespace