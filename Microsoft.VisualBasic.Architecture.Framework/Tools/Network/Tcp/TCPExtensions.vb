﻿Imports System.Net.NetworkInformation
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocol
Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Runtime.CompilerServices
Imports System.Reflection

Namespace Net

    Public Module TCPExtensions

        ''' <summary>
        ''' -1标识Ping不通
        ''' </summary>
        ''' <param name="operationTimeOut">ms</param>
        ''' <returns></returns>
        Public Function Ping(ep As System.Net.IPEndPoint, Optional operationTimeOut As Integer = 3 * 1000) As Double
            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim request As RequestStream = RequestStream.SystemProtocol(RequestStream.Protocols.Ping, PING_REQUEST)
            Dim socket As New AsynInvoke(ep)
            Dim response As RequestStream = socket.SendMessage(request, OperationTimeOut:=operationTimeOut)

            If HTTP_RFC.RFC_REQUEST_TIMEOUT = response.Protocol Then
                Return -1
            End If

            Return sw.ElapsedMilliseconds
        End Function

        Public Const PING_REQUEST As String = "PING/TTL-78973"

        Public Function IPEndPoint(str As String) As System.Net.IPEndPoint
            Dim Tokens As String() = str.Split(":"c)
            Dim IPAddress As String = Tokens.First
            Dim Port As Integer = CInt(Val(Tokens(1)))
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port)
        End Function

        ''' <summary>
        ''' 假若不能成功的建立起连接的话，则会抛出错误
        ''' </summary>
        ''' <param name="server"></param>
        ''' <param name="port"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ConnectSocket(server As String, port As Integer) As System.Net.IPEndPoint

            ' Get host related information.
            Dim HostEntry As IPHostEntry = Dns.GetHostEntry(server)

            ' Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            ' an exception that occurs when the host host IP Address is not compatible with the address family
            ' (typical in the IPv6 case).
            For Each Address As IPAddress In HostEntry.AddressList
                Dim endPoint As New System.Net.IPEndPoint(Address, port)
                Dim Socket As Socket = New Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)

                Try
                    Call Socket.Connect(endPoint)
                Catch ex As Exception
                    Continue For
                End Try

                If Socket.Connected Then
                    Return endPoint
                End If
            Next

            Throw New Exception(String.Format("The target connection to {0}:{1} can not be made!", server, port))
        End Function

        ''' <summary>
        ''' Get the first available TCP port on this local machine.(获取第一个可用的端口号)
        ''' </summary>
        ''' <param name="BEGIN_PORT">Check the local port available from this port value.(从这个端口开始检测)</param>
        ''' <returns></returns>
        Public Function GetFirstAvailablePort(Optional BEGIN_PORT As Integer = 100) As Integer
            Dim MAX_PORT As Integer = 65535    '系统tcp/udp端口数最大是65535

            For i As Integer = BEGIN_PORT To MAX_PORT - 1
                If PortIsAvailable(port:=i) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 获取操作系统已用的端口号
        ''' </summary>
        ''' <returns></returns>
        Public Function PortIsUsed() As Integer()
            '获取本地计算机的网络连接和通信统计数据的信息
            Dim ipGlobalProperties__1 As IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()

            '返回本地计算机上的所有Tcp监听程序
            Dim ipsTCP As System.Net.IPEndPoint() = ipGlobalProperties__1.GetActiveTcpListeners()

            '返回本地计算机上的所有UDP监听程序
            Dim ipsUDP As System.Net.IPEndPoint() = ipGlobalProperties__1.GetActiveUdpListeners()

            '返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            Dim tcpConnInfoArray As TcpConnectionInformation() = ipGlobalProperties__1.GetActiveTcpConnections()

            Dim allPorts As List(Of Integer) = New List(Of Integer)
            Call allPorts.AddRange((From ep As System.Net.IPEndPoint In ipsTCP Select ep.Port).ToArray)
            Call allPorts.AddRange((From ep As System.Net.IPEndPoint In ipsUDP Select ep.Port).ToArray)
            Call allPorts.AddRange((From conn As TcpConnectionInformation In tcpConnInfoArray Select conn.LocalEndPoint.Port).ToArray)

            Return allPorts.ToArray
        End Function

        ''' <summary>
        ''' 检查指定端口是否已用
        ''' </summary>
        ''' <param name="port"></param>
        ''' <returns></returns>
        Public Function PortIsAvailable(port As Integer) As Boolean
            Dim portUsed As Integer() = PortIsUsed()

            For Each p As Integer In portUsed
                If p = port Then
                    Return False
                End If
            Next

            Return True
        End Function

#Region "OAuth Arguments"

        Const hash As String = "hash"
        Const uid As String = "uid"

        <Extension> Public Function BuildOAuth(ca As Net.SSL.Certificate) As String
            Dim array As KeyValuePair(Of String, String)() = {
                New KeyValuePair(Of String, String)(hash, ca.PrivateKey),
                New KeyValuePair(Of String, String)(uid, ca.uid)
            }
            Dim oauth As String = WebServices.BuildArgvs(array)
            Return oauth
        End Function

        <Extension> Public Function GetCA(args As String) As Net.SSL.Certificate
#If DEBUG Then
            Call $"{MethodBase.GetCurrentMethod.GetFullName} ==> {args}".__DEBUG_ECHO
#End If
            Dim dict = WebServices.requestParser(args, False)
#If DEBUG Then
            Call String.Join("; ", dict.ToArray(Function(obj) obj.ToString)).__DEBUG_ECHO
#End If
            Dim privateKey As String = dict(hash)
            Dim uid As Long = Scripting.CTypeDynamic(Of Long)(dict(TCPExtensions.uid))
            Return Net.SSL.Certificate.Install(privateKey, uid)
        End Function

#End Region

    End Module
End Namespace
