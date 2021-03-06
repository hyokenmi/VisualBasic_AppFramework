﻿Imports System.Drawing

Namespace Drawing3D

    ''' <summary>
    ''' Defines the Point3D class that represents points in 3D space.
    ''' Developed by leonelmachava &lt;leonelmachava@gmail.com>
    ''' http://codentronix.com
    '''
    ''' Copyright (c) 2011 Leonel Machava
    ''' </summary>
    Public Class Point3D

        Public Sub New(x As Double, y As Double, Optional z As Double = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public Sub New(Position As Point)
            Call Me.New(Position.X, Position.Y)
        End Sub

        Public Property X As Double
        Public Property Y As Double
        Public Property Z As Double

        Public Function RotateX(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, yn As Double, zn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            yn = Me.Y * cosa - Me.Z * sina
            zn = Me.Y * sina + Me.Z * cosa
            Return New Point3D(Me.X, yn, zn)
        End Function

        Public Function RotateY(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Zn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Zn = Me.Z * cosa - Me.X * sina
            Xn = Me.Z * sina + Me.X * cosa

            Return New Point3D(Xn, Me.Y, Zn)
        End Function

        Public Function RotateZ(angle As Integer) As Point3D
            Dim rad As Double, cosa As Double, sina As Double, Xn As Double, Yn As Double

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Xn = Me.X * cosa - Me.Y * sina
            Yn = Me.X * sina + Me.Y * cosa
            Return New Point3D(Xn, Yn, Me.Z)
        End Function

        Public Function Project(viewWidth, viewHeight, fov, viewDistance)
            Dim factor As Double, Xn As Double, Yn As Double
            factor = fov / (viewDistance + Me.Z)
            Xn = Me.X * factor + viewWidth / 2
            Yn = Me.Y * factor + viewHeight / 2
            Return New Point3D(Xn, Yn, Me.Z)
        End Function
    End Class
End Namespace