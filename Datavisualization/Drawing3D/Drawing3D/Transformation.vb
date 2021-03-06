﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Drawing3D

    ''' <summary>
    ''' 3D coordinate transformation tools.
    ''' </summary>
    <PackageNamespace("Coordinate.Transformation",
                      Category:=APICategories.UtilityTools,
                      Publisher:="xie.guigang@gmail.com",
                      Description:="3D coordinate transformation tools.")>
    Public Module Transformation

        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Double) As Point
            Dim X As Double = Math.Cos(xRotate) * pt3D.X + pt3D.Y
            Dim Y As Double = Math.Sin(xRotate) * pt3D.X - pt3D.Z

            Return New Point(X, Y)
        End Function

        <ExportAPI("SpaceToGrid")>
        <Extension> Public Function SpaceToGrid(pt3D As Point3D, xRotate As Double, offset As Point) As Point
            Dim X As Double = Math.Cos(xRotate) * pt3D.X + pt3D.Y + offset.X
            Dim Y As Double = Math.Sin(xRotate) * pt3D.X - pt3D.Z + offset.Y

            Return New Point(X, Y)
        End Function
    End Module
End Namespace