﻿Imports System.Runtime.CompilerServices
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Text
Imports System
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Reflection

''' <summary>
''' GDI+
''' </summary>
''' 
<PackageNamespace("GDI+", Description:="GDI+ GDIPlus Extensions Module to provide some useful interface.",
                  Publisher:="xie.guigang@gmail.com",
                  Revision:=58,
                  Url:="http://gcmodeller.org")>
Public Module GDIPlusExtensions

    <Extension> Public Function GetCenter(size As Size) As Point
        Return New Point(size.Width / 2, size.Height / 2)
    End Function

    <ExportAPI("To.Icon")>
    <Extension> Public Function GetIcon(res As Image) As Icon
        Return Drawing.Icon.FromHandle(New Bitmap(res).GetHicon)
    End Function

    <ExportAPI("To.Icon")>
    <Extension> Public Function GetIcon(res As Bitmap) As Icon
        Return Drawing.Icon.FromHandle(res.GetHicon)
    End Function

    ''' <summary>
    ''' Load image from a file and then close the file handle.
    ''' (使用<see cref="Image.FromFile(String)"/>函数在加载完成图像到Dispose这段之间内都不会释放文件句柄，
    ''' 则使用这个函数则没有这个问题，在图片加载之后会立即释放掉文件句柄)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <ExportAPI("LoadImage"), Extension>
    Public Function LoadImage(path As String) As Image
        Dim stream As Byte() = FileIO.FileSystem.ReadAllBytes(path)
        Dim res = Image.FromStream(stream:=New IO.MemoryStream(stream))
        Return res
    End Function

    <ExportAPI("LoadImage")>
    <Extension> Public Function LoadImage(rawStream As Byte()) As Image
        Dim res = Image.FromStream(stream:=New IO.MemoryStream(rawStream))
        Return res
    End Function

    ''' <summary>
    ''' 将图片对象转换为原始的字节流
    ''' </summary>
    ''' <param name="image"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Get.RawStream")>
    <Extension> Public Function GetRawStream(image As Image) As Byte()
        Dim stream As New IO.MemoryStream
        Call image.Save(stream, Imaging.ImageFormat.Png)
        Return stream.ToArray
    End Function

    Private ReadOnly GrCommon As Graphics = Graphics.FromImage(New Bitmap(64, 64))

    <Extension> Public Function MeasureString(s As String, Font As Font, Optional XScaleSize As Single = 1, Optional YScaleSize As Single = 1) As Size
        Call GrCommon.ScaleTransform(XScaleSize, YScaleSize)
        Dim Size = GrCommon.MeasureString(s, Font)
        Return New Size(Size.Width, Size.Height)
    End Function

    <ExportAPI("GrayBitmap", Info:="Create the gray color of the target image.")>
    <Extension> Public Function CreateGrayBitmap(res As Image) As Image
        Dim Gr = DirectCast(res.Clone, Image).GdiFromImage
        Call System.Windows.Forms.ControlPaint.DrawImageDisabled(Gr.Gr_Device, res, 0, 0, Color.FromArgb(0, 0, 0, 0))
        Return Gr.ImageResource
    End Function

    ''' <summary>
    ''' 微软雅黑字体的名称
    ''' </summary>
    Public Const FONT_FAMILY_MICROSOFT_YAHEI As String = "Microsoft YaHei"
    Public Const FONT_FAMILY_UBUNTU As String = "Ubuntu"
    Public Const FONT_FAMILY_SEGOE_UI As String = "Segoe UI"

    ''' <summary>
    ''' Adding a frame box to the target image source.(为图像添加边框)
    ''' </summary>
    ''' <param name="Handle"></param>
    ''' <param name="pen">Default pen width is 1px and with color <see cref="Color.Black"/>.(默认的绘图笔为黑色的1个像素的边框)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ImageAddFrame(Handle As GDIPlusDeviceHandle,
                                              Optional pen As Drawing.Pen = Nothing,
                                              Optional offset As Integer = 0) As GDIPlusDeviceHandle

        Dim TopLeft As New Point(offset, offset)
        Dim TopRight As New Point(Handle.Width - offset, 1 + offset)
        Dim BottomLeft As New Point(offset + 1, Handle.Height - offset)
        Dim BottomRight As New Point(Handle.Width - offset, Handle.Height - offset)

        If pen Is Nothing Then
            pen = Drawing.Pens.Black
        End If

        Call Handle.Gr_Device.DrawLine(pen, TopLeft, TopRight)
        Call Handle.Gr_Device.DrawLine(pen, TopRight, BottomRight)
        Call Handle.Gr_Device.DrawLine(pen, BottomRight, BottomLeft)
        Call Handle.Gr_Device.DrawLine(pen, BottomLeft, TopLeft)

        Call Handle.Gr_Device.FillRectangle(New SolidBrush(pen.Color), New Rectangle(New Point, New Size(1, 1)))

        Return Handle
    End Function

    ''' <summary>
    ''' 创建一个GDI+的绘图设备
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">默认的背景填充颜色为白色</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("GDI+.Create")>
    <Extension> Public Function CreateGDIDevice(r As Drawing.SizeF, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
        Return (New Size(CInt(r.Width), CInt(r.Height))).CreateGDIDevice(filled)
    End Function

    <Extension> Public Function OpenDevice(ctrl As System.Windows.Forms.Control) As GDIPlusDeviceHandle
        Dim ImageRes As Image

        'If ctrl.BackgroundImage Is Nothing Then
        ImageRes = New Bitmap(ctrl.Width, ctrl.Height)
        'Else
        'ImageRes = ctrl.BackgroundImage
        'End If

        Dim Device = ImageRes.GdiFromImage

        If ctrl.BackgroundImage Is Nothing Then
            Call Device.Gr_Device.FillRectangle(Brushes.White, New Rectangle(New Point, ImageRes.Size))
        End If

        Return Device
    End Function

    ''' <summary>
    ''' 从指定的文件之中加载GDI+设备的句柄
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("GDI+.Create")>
    <Extension> Public Function GDIPlusDeviceHandleFromImageFile(path As String) As GDIPlusDeviceHandle
        Dim Image As Image = LoadImage(path)
        Dim GrDevice As Graphics = Graphics.FromImage(Image)
        GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        GrDevice.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Return GDIPlusDeviceHandle.CreateObject(GrDevice, Image)
    End Function

    ''' <summary>
    ''' 无需处理图像数据，这个函数已经自动克隆了该对象，不会影响到原来的对象
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("GDI+.Create")>
    <Extension> Public Function GdiFromImage(Image As Image) As GDIPlusDeviceHandle
        Dim Gr = Image.Size.CreateGDIDevice
        Call Gr.Gr_Device.DrawImage(Image, 0, 0, Gr.Width, Gr.Height)
        Return Gr
    End Function

    <ExportAPI("Offset")>
    <Extension> Public Function OffSet(p As Point, x As Integer, y As Integer) As Point
        Return New Point(x + p.X, y + p.Y)
    End Function

    <Extension> Public Function IsValidGDIParameter(size As Size) As Boolean
        Return size.Width > 0 AndAlso size.Height > 0
    End Function

    ''' <summary>
    ''' 创建一个GDI+的绘图设备
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">默认的背景填充颜色为白色</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("GDI+.Create")>
    <Extension> Public Function CreateGDIDevice(r As Drawing.Size, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
        Dim Bitmap As Bitmap

        'Call Console.WriteLine($"[DEBUG {Now.ToString}] device area ==> {r.ToString}.")

        If r.Width = 0 OrElse r.Height = 0 Then
            Throw New Exception("One of the size parameter for the gdi+ device is not valid!")
        End If

        Try
            Bitmap = New Bitmap(r.Width, r.Height)
        Catch ex As Exception
            ex = New Exception(r.ToString, ex)
            Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
            Throw ex
        End Try

        Dim GrDevice As Graphics = Graphics.FromImage(Bitmap)

        If filled = Nothing Then
            filled = Color.White
        End If

        Call GrDevice.FillRectangle(New SolidBrush(filled), New Rectangle(New Point, Bitmap.Size))

        GrDevice.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        GrDevice.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
        GrDevice.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

        Return GDIPlusDeviceHandle.CreateObject(GrDevice, Bitmap)
    End Function

    ''' <summary>
    ''' 图片剪裁小方块区域
    ''' </summary>
    ''' <param name="pos">左上角的坐标位置</param>
    ''' <param name="size">剪裁的区域的大小</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Image.Corp")>
    <Extension> Public Function ImageCrop(source As Image, pos As Point, size As Size) As Image
        Dim CloneRect As New Rectangle(pos, size)
        Dim CloneBitmap As Bitmap = CType(source.Clone, Bitmap)
        Dim crop As Bitmap = CloneBitmap.Clone(CloneRect, source.PixelFormat)
        Return crop
    End Function

    <ExportAPI("Image.Resize")>
    Public Function Resize(Image As Image, newSize As Size) As Image
        Dim Gr = newSize.CreateGDIDevice

        Call Gr.Gr_Device.DrawImage(Image, 0, 0, newSize.Width, newSize.Height)
        Return Gr.ImageResource
    End Function

    ''' <summary>
    ''' 图片剪裁为圆形的头像
    ''' </summary>
    ''' <param name="resAvatar">要求为正方形或者近似正方形</param>
    ''' <param name="OutSize"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimRoundAvatar(resAvatar As Image, OutSize As Integer) As Image
        If resAvatar Is Nothing Then
            Return Nothing
        End If

        Dim Bitmap As Bitmap = New Bitmap(OutSize, OutSize)

        resAvatar = DirectCast(resAvatar.Clone, Image)

        Using Gr = Graphics.FromImage(Bitmap)

            Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            Gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            Gr.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

            resAvatar = Resize(resAvatar, Bitmap.Size)

            Dim rH As Drawing.Brush = New Drawing.TextureBrush(resAvatar)
            Call Gr.FillPie(rH, New Rectangle(0, 0, OutSize, OutSize), 0, 360)
            Return Bitmap
        End Using
    End Function

    <Extension> Public Function Clone(res As Bitmap) As Bitmap
        If res Is Nothing Then Return Nothing
        Return DirectCast(res.Clone, Bitmap)
    End Function

    <Extension> Public Function Clone(res As Image) As Image
        If res Is Nothing Then Return Nothing
        Return DirectCast(res.Clone, Image)
    End Function

    Const RGB_EXPRESSION As String = "\d+,\d+,\d+"

    <ExportAPI("Get.Color")>
    <Extension> Public Function ToColor(str As String) As Color
        Dim s As String = Regex.Match(str, RGB_EXPRESSION).Value
        If String.IsNullOrEmpty(s) Then
            Return Color.FromName(str)
        Else
            Dim Tokens = s.Split(","c)
            Dim R As Integer = CInt(Val(Tokens(0))), G As Integer = CInt(Val(Tokens(1))), B As Integer = CInt(Val(Tokens(2)))
            Return Color.FromArgb(R, G, B)
        End If
    End Function

    ''' <summary>
    ''' Determine that the target color value is a empty variable.(判断目标颜色值是否为空值)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function IsNullOrEmpty(Color As Color) As Boolean
        Return Color = Nothing OrElse Color.IsEmpty
    End Function

    ''' <summary>
    ''' 羽化
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <param name="y1"></param>
    ''' <param name="y2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Vignette(Image As Image, y1 As Integer, y2 As Integer, Optional RenderColor As Color = Nothing) As Image
        Dim Gr = Image.GdiFromImage
        Dim Alpha As Integer = 0
        Dim delta = (Math.PI / 2) / Math.Abs(y1 - y2)
        Dim offset As Double = 0

        If RenderColor = Nothing OrElse RenderColor.IsEmpty Then
            RenderColor = Color.White
        End If

        For y As Integer = y1 To y2
            Dim Color = System.Drawing.Color.FromArgb(Alpha, RenderColor.R, RenderColor.G, RenderColor.B)
            Call Gr.Gr_Device.DrawLine(New Pen(Color), New Point(0, y), New Point(Gr.Width, y))

            Alpha = CInt(255 * Math.Sin(offset) ^ 2)
            offset += delta
        Next

        Dim Rect = New Rectangle(New Point(0, y2), New Size(Image.Width, Image.Height - y2))
        Call Gr.Gr_Device.FillRectangle(New SolidBrush(RenderColor), Rect)

        Return Gr.ImageResource
    End Function

    ''' <summary>
    ''' 确定边界，然后进行剪裁
    ''' </summary>
    ''' <param name="res"></param>
    ''' <param name="margin"></param>
    ''' <returns></returns>
    <ExportAPI("Image.CorpBlank")>
    <Extension> Public Function CorpBlank(res As Image, Optional margin As Integer = 0, Optional blankColor As Color = Nothing) As Image
        If blankColor.IsNullOrEmpty Then
            blankColor = Color.White
        End If

        Dim top As Integer
        Dim left As Integer
        Dim bmp As New Bitmap(res)

        ' top

        For top = 0 To res.Height - 1
            Dim find As Boolean = False

            For left = 0 To res.Width - 1
                Dim p = bmp.GetPixel(left, top)
                If Not Equals(p, blankColor) Then
                    ' 在这里确定了左右
                    find = True
                    Exit For
                End If
            Next

            If find Then
                Exit For
            End If
        Next

        Dim region As New Rectangle(0, top, res.Width, res.Height - top)
        res = res.ImageCrop(region.Location, region.Size)
        bmp = New Bitmap(res)

        ' left

        For left = 0 To res.Width - 1
            Dim find As Boolean = False

            For top = 0 To res.Height - 1
                Dim p = bmp.GetPixel(left, top)
                If Not Equals(p, blankColor) Then
                    ' 在这里确定了左右
                    find = True
                    Exit For
                End If
            Next

            If find Then
                Exit For
            End If
        Next

        region = New Rectangle(left, 0, res.Width - left, res.Height)
        res = res.ImageCrop(region.Location, region.Size)
        bmp = New Bitmap(res)

        Dim right As Integer
        Dim bottom As Integer

        ' bottom

        For bottom = res.Height - 1 To 0 Step -1
            Dim find As Boolean = False

            For right = res.Width - 1 To 0 Step -1
                Dim p = bmp.GetPixel(right, bottom)
                If Not Equals(p, blankColor) Then
                    ' 在这里确定了左右
                    find = True
                    Exit For
                End If
            Next

            If find Then
                Exit For
            End If
        Next

        region = New Rectangle(0, 0, res.Width, bottom)
        res = res.ImageCrop(region.Location, region.Size)
        bmp = New Bitmap(res)

        ' right

        For right = res.Width - 1 To 0 Step -1
            Dim find As Boolean = False

            For bottom = res.Height - 1 To 0 Step -1
                Dim p = bmp.GetPixel(right, bottom)
                If Not Equals(p, blankColor) Then
                    ' 在这里确定了左右
                    find = True
                    Exit For
                End If
            Next

            If find Then
                Exit For
            End If
        Next

        region = New Rectangle(0, 0, right, res.Height)
        res = res.ImageCrop(region.Location, region.Size)

        If margin > 0 Then
            Dim gr = New Size(res.Width + margin * 2, res.Height + margin * 2).CreateGDIDevice
            Call gr.Gr_Device.DrawImage(res, New Point(margin, margin))
            res = gr.ImageResource
        End If

        Return res
    End Function

    <Extension> Public Function Equals(a As Color, b As Color) As Boolean
        If a.A <> b.A Then
            Return False
        End If
        If a.B <> b.B Then
            Return False
        End If
        If a.G <> b.G Then
            Return False
        End If
        If a.R <> b.R Then
            Return False
        End If

        Return True
    End Function
End Module
