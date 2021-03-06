﻿Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    Public Interface ISchema
        ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer)
        Function GetOrdinal(name As String) As Integer
    End Interface

    Public Class RowBuilder

        ''' <summary>
        ''' 总的列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Columns As ComponentModels.StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider

        Public ReadOnly Property IndexedFields As StorageProvider()
        Public ReadOnly Property NonIndexed As Dictionary(Of String, Integer)
        Public ReadOnly Property HaveMetaAttribute As Boolean

        Sub New(SchemaProvider As SchemaProvider)
            Me.SchemaProvider = SchemaProvider
            Me.Columns = ({
            SchemaProvider.Columns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.EnumColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.KeyValuePairColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.CollectionColumns.ToArray(Function(field) DirectCast(field, ComponentModels.StorageProvider)),
            New StorageProvider() {DirectCast(SchemaProvider.MetaAttributes, StorageProvider)}}).MatrixToVector
            Me.Columns = (From field As StorageProvider In Me.Columns
                          Where Not field Is Nothing
                          Select field).ToArray
            HaveMetaAttribute = Not SchemaProvider.MetaAttributes Is Nothing
        End Sub

        Public Sub Indexof(schema As ISchema)
            Dim LQuery = (From field As StorageProvider In Columns
                          Let ordinal As Integer = schema.GetOrdinal(field.Name)
                          Select field.InvokeSet(NameOf(field.Ordinal), ordinal)).ToArray
            _IndexedFields = (From field In LQuery Where field.Ordinal > -1 Select field).ToArray
            Dim Indexed = IndexedFields.ToArray(Function(field) field.Name.ToLower)
            '没有被建立索引的都可能会当作为字典数据
            _NonIndexed = (From colum As KeyValuePair(Of String, Integer)
                           In schema.SchemaOridinal
                           Where Array.IndexOf(Indexed, colum.Key.ToLower) = -1
                           Select colum).ToDictionary(Function(field) field.Key, elementSelector:=Function(field) field.Value)
        End Sub

        Public Function FillData(Of T As Class)(row As DocumentStream.RowObject, obj As T) As T
            obj = __tryFill(Of T)(row, obj)

            If HaveMetaAttribute Then
                Dim values = (From field As KeyValuePair(Of String, Integer)
                              In NonIndexed
                              Select name = field.Key,
                                  value = SchemaProvider.MetaAttributes.LoadMethod(row.DirectGet(field.Value))).ToArray
                Dim meta As IDictionary = SchemaProvider.MetaAttributes.CreateDictionary

                For Each x In values
                    Call meta.Add(x.name, x.value)
                Next

                Call SchemaProvider.MetaAttributes.BindProperty.SetValue(obj, meta)
            End If

            Return obj
        End Function

        Private Function __tryFill(Of T As Class)(row As DocumentStream.RowObject, obj As T) As T
            Dim i As Integer, column As StorageProvider = Nothing

            For i = 0 To IndexedFields.Length - 1
                column = IndexedFields(i)

                Dim value As String = row.Column(column.Ordinal)
                Dim propValue As Object = column.LoadMethod()(value)

                Call column.BindProperty.SetValue(obj, propValue)
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return SchemaProvider.ToString
        End Function
    End Class
End Namespace