﻿Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocol
Imports Microsoft.VisualBasic.Net.Protocol.Reflection

Namespace Net

    ''' <summary>
    ''' Socket listening object which is running at the server side asynchronous able multiple threading.
    ''' (运行于服务器端上面的Socket监听对象，多线程模型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TcpSynchronizationServicesSocket
        Implements System.IDisposable
        Implements ComponentModel.DataSourceModel.IObjectModel_Driver
        Implements Net.Abstract.IServicesSocket

#Region "INTERNAL FIELDS"

        Dim _ThreadEndAccept As Boolean = True
        Dim __exceptionHandle As ExceptionHandler
        Dim _ServicesSocket As Socket

#End Region

        ''' <summary>
        ''' The server services listening on this local port.(当前的这个服务器对象实例所监听的本地端口号)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LocalPort As Integer Implements IServicesSocket.LocalPort

        ''' <summary>
        ''' This function pointer using for the data request handling of the data request from the client socket.   
        ''' [Public Delegate Function DataResponseHandler(str As <see cref="System.String"/>, RemoteAddress As <see cref="System.Net.IPEndPoint"/>) As <see cref="System.String"/>]
        ''' (这个函数指针用于处理来自于客户端的请求)
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Responsehandler As DataRequestHandler Implements IServicesSocket.Responsehandler

        Public ReadOnly Property IsShutdown As Boolean Implements IServicesSocket.IsShutdown
            Get
                Return disposedValue
            End Get
        End Property

        ''' <summary>
        ''' 消息处理的方法接口： Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String
        ''' </summary>
        ''' <param name="LocalPort">监听的本地端口号，假若需要进行端口映射的话，则可以在<see cref="Run"></see>方法之中设置映射的端口号</param>
        ''' <remarks></remarks>
        Sub New(Optional LocalPort As Integer = 11000,
                Optional exHandler As ExceptionHandler = Nothing)

            Me._LocalPort = LocalPort
            Me.__exceptionHandle = If(exHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), exHandler)
        End Sub

        Sub New(DataArrivalEventHandler As Net.Abstract.DataRequestHandler, LocalPort As Integer, Optional exHandler As ExceptionHandler = Nothing)
            Me.Responsehandler = DataArrivalEventHandler
            Me.__exceptionHandle = If(exHandler Is Nothing, Sub(ex As Exception) Call Console.WriteLine(ex.ToString), exHandler)
            Me._LocalPort = LocalPort
        End Sub

        ''' <summary>
        ''' 函数返回Socket的注销方法
        ''' </summary>
        ''' <param name="DataArrivalEventHandler">Public Delegate Function DataResponseHandler(str As String, RemotePort As Integer) As String</param>
        ''' <param name="LocalPort"></param>
        ''' <param name="exHandler"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function BeginListen(DataArrivalEventHandler As DataRequestHandler,
                                           Optional LocalPort As Integer = 11000,
                                           Optional exHandler As ExceptionHandler = Nothing) As Action
            Dim Socket As New TcpSynchronizationServicesSocket(DataArrivalEventHandler, LocalPort, exHandler)
            Call (Sub() Call Socket.Run()).BeginInvoke(Nothing, Nothing)
            Return AddressOf Socket.Dispose
        End Function

        Public Function LoopbackEndPoint(Port As Integer) As System.Net.IPEndPoint
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Loopback, Port)
        End Function

        Public Overrides Function ToString() As String
            Return $"{GetIPAddress()}:{LocalPort}"
        End Function

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run() As Integer Implements IObjectModel_Driver.Run, IServicesSocket.Run

            ' Establish the local endpoint for the socket.
            Dim localEndPoint As System.Net.IPEndPoint =
                New System.Net.IPEndPoint(System.Net.IPAddress.Any, _LocalPort)
            Return Run(localEndPoint)
        End Function 'Main

        ''' <summary>
        ''' This server waits for a connection and then uses  asychronous operations to
        ''' accept the connection, get data from the connected client,
        ''' echo that data back to the connected client.
        ''' It then disconnects from the client and waits for another client.(请注意，当服务器的代码运行到这里之后，代码将被阻塞在这里)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Run(localEndPoint As System.Net.IPEndPoint) As Integer Implements IServicesSocket.Run

            _LocalPort = localEndPoint.Port

            ' Create a TCP/IP socket.
            _ServicesSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            '_InternalSocketListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
            ' Bind the socket to the local endpoint and listen for incoming connections.

            Try
                Call _ServicesSocket.Bind(localEndPoint)
                Call _ServicesSocket.ReceiveBufferSize.InvokeSet(4096000)
                Call _ServicesSocket.SendBufferSize.InvokeSet(4096000)
                Call _ServicesSocket.Listen(backlog:=1000)
            Catch ex As Exception
                Dim exMessage As String =
                    "Exception on try initialize the socket connection local_EndPoint=" & localEndPoint.ToString &
                    vbCrLf &
                    vbCrLf &
                    ex.ToString
                Call Me.__exceptionHandle(New Exception(exMessage))
                Throw
            Finally
#If DEBUG Then
                Call $"{MethodBase.GetCurrentMethod().GetFullName}  ==> {localEndPoint.ToString}".__DEBUG_ECHO
#End If
            End Try
#Region ""
            'If SelfMapping Then  '端口转发映射设置
            '    Call Console.WriteLine("Self port mapping @wan_port={0} --->@lan_port", _LocalPort)
            '    If Microsoft.VisualBasic.PortMapping.SetPortsMapping(_LocalPort, _LocalPort) = False Then
            '        Call Console.WriteLine("Ports mapping is not successful!")
            '    End If
            'Else
            '    If Not PortMapping < 100 Then
            '        Call Console.WriteLine("Ports mapping wan_port={0}  ----->  lan_port={1}", PortMapping, LocalPort)
            '        If False = SetPortsMapping(PortMapping, _LocalPort) Then
            '            Call Console.WriteLine("Ports mapping is not successful!")
            '        End If
            '    End If
            'End If
#End Region
            _ThreadEndAccept = True
            _Running = True

            While Not Me.disposedValue

                If _ThreadEndAccept Then
                    _ThreadEndAccept = False

                    Dim Callback As AsyncCallback = New AsyncCallback(AddressOf AcceptCallback)
                    Try
                        Call _ServicesSocket.BeginAccept(Callback, _ServicesSocket)  ' Free 之后可能会出现空引用错误，则忽略掉这个错误，退出线程
                    Catch ex As Exception
                        Exit While
                    End Try
                End If

                Call Thread.Sleep(1)
            End While
            _Running = False

            Return 0
        End Function

        Public ReadOnly Property Running As Boolean = False Implements IServicesSocket.IsRunning

        Public Sub WaitForStart()
            Do While Running = False
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub AcceptCallback(ar As IAsyncResult)

            ' Get the socket that handles the client request.
            Dim listener As Socket = DirectCast(ar.AsyncState, Socket)

            ' End the operation.
            Dim handler As Socket

            Try
                handler = listener.EndAccept(ar)
            Catch ex As Exception
                _ThreadEndAccept = True
                Return
            End Try

            ' Create the state object for the async receive.
            Dim state As StateObject = New StateObject With {.workSocket = handler}

            Try
                Call handler.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
            Catch ex As Exception
                ' 远程强制关闭主机连接，则放弃这一条数据请求的线程
                Call ForceCloseHandle(handler.RemoteEndPoint)
            End Try

            _ThreadEndAccept = True

        End Sub 'AcceptCallback

        Private Sub ForceCloseHandle(RemoteEndPoint As EndPoint)
            Call $"Connection was force closed by {RemoteEndPoint.ToString}, services thread abort!".__DEBUG_ECHO
        End Sub

        Private Sub ReadCallback(ar As IAsyncResult)
            ' Retrieve the state object and the handler socket
            ' from the asynchronous state object.
            Dim state As StateObject = DirectCast(ar.AsyncState, StateObject)
            Dim handler As Socket = state.workSocket
            ' Read data from the client socket.
            Dim bytesRead As Integer

            Try
                bytesRead = handler.EndReceive(ar)  '在这里可能发生远程客户端主机强制断开连接，由于已经被断开了，客户端已经放弃了这一次数据请求，所有在这里讲这个请求线程放弃
            Catch ex As Exception
                Call ForceCloseHandle(handler.RemoteEndPoint)
                Return
            End Try

            If bytesRead > 0 Then  '有新的数据

                ' There  might be more data, so store the data received so far.
                state.ChunkBuffer.AddRange(state.readBuffer.Takes(bytesRead))
                ' Check for end-of-file tag. If it is not there, read
                ' more data.
                state.readBuffer = state.ChunkBuffer.ToArray

                Dim requestData As RequestStream = New RequestStream(state.readBuffer) '得到的是原始的请求数据

                If requestData.FullRead Then
                    Call HandleRequest(handler, requestData)
                Else
                    Try
                        ' Not all data received. Get more.
                        Call handler.BeginReceive(state.readBuffer, 0, StateObject.BufferSize, 0, New AsyncCallback(AddressOf ReadCallback), state)
                    Catch ex As Exception
                        Call ForceCloseHandle(handler.RemoteEndPoint)
                        Return
                    End Try
                End If
            End If
        End Sub 'ReadCallback

        ''' <summary>
        ''' All the data has been read from the client. Display it on the console.
        ''' Echo the data back to the client.
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <param name="requestData"></param>
        Private Sub HandleRequest(handler As Socket, requestData As Net.Protocol.RequestStream)
            ' All the data has been read from the
            ' client. Display it on the console.
            ' Echo the data back to the client.

            Dim remoteEP = DirectCast(handler.RemoteEndPoint, System.Net.IPEndPoint)

            Try
                If requestData.IsPing Then
                    requestData = NetResponse.RFC_OK
                Else
                    requestData = Me.Responsehandler()(requestData.uid, requestData, remoteEP)
                End If
                Call Send(handler, requestData)
            Catch ex As Exception
                Call __exceptionHandle(ex)
                '错误可能是内部处理请求的时候出错了，则将SERVER_INTERNAL_EXCEPTION结果返回给客户端
                Try
                    Call Send(handler, NetResponse.RFC_INTERNAL_SERVER_ERROR)
                Catch ex2 As Exception '这里处理的是可能是强制断开连接的错误
                    Call __exceptionHandle(ex2)
                End Try
            End Try
        End Sub

        ''' <summary>
        ''' Server reply the processing result of the request from the client.
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Private Sub Send(handler As Socket, data As String)

            ' Convert the string data to byte data using ASCII encoding.
            Dim byteData As Byte() = Encoding.UTF8.GetBytes(data)
            byteData = New Net.Protocol.RequestStream(0, 0, byteData).Serialize
            ' Begin sending the data to the remote device.
            Call handler.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf SendCallback), handler)
        End Sub 'Send

        Private Sub Send(handler As Socket, data As RequestStream)
            ' Convert the string data to byte data using ASCII encoding.
            Dim byteData As Byte() = data.Serialize
            ' Begin sending the data to the remote device.
            Call handler.BeginSend(byteData, 0, byteData.Length, 0, New AsyncCallback(AddressOf SendCallback), handler)
        End Sub

        Private Sub SendCallback(ar As IAsyncResult)

            ' Retrieve the socket from the state object.
            Dim handler As Socket = DirectCast(ar.AsyncState, Socket)
            ' Complete sending the data to the remote device.
            Dim bytesSent As Integer = handler.EndSend(ar)
            'Console.WriteLine("Sent {0} bytes to client.", bytesSent)
            Call handler.Shutdown(SocketShutdown.Both)
            Call handler.Close()
        End Sub 'SendCallback

        ''' <summary>
        ''' SERVER_INTERNAL_EXCEPTION，Server encounter an internal exception during processing
        ''' the data request from the remote device.
        ''' (判断是否服务器在处理客户端的请求的时候，发生了内部错误)
        ''' </summary>
        ''' <param name="replyData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsServerInternalException(replyData As String) As Boolean
            Return String.Equals(replyData, NetResponse.RFC_INTERNAL_SERVER_ERROR.GetUTF8String)
        End Function

#Region "IDisposable Support"

        ''' <summary>
        ''' 退出监听线程所需要的
        ''' </summary>
        ''' <remarks></remarks>
        Private disposedValue As Boolean = False  ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then

                    Call _ServicesSocket.Dispose()
                    Call _ServicesSocket.Free()
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

        ''' <summary>
        ''' Stop the server socket listening threads.(终止服务器Socket监听线程)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
