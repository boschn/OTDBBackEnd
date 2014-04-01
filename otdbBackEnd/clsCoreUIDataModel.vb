REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** UI Data Model Classes for ORM iormPersistables 
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-03-14
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2014
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections.Generic
Imports System.Data
Imports System.Diagnostics.Debug

Imports OnTrack.Database

Namespace OnTrack.UI
    ''' <summary>
    ''' a model class for multiple data rows from different sources for User Interfaces
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormModelTable
        Inherits DataTable

        Private _queriedenumeration As iormQueriedEnumeration
        Private _isInitialized As Boolean = False
        Private _isloaded As Boolean = False

        Public Const constQRYRowReference = "$$QRYRowReference"
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="queriedenumeration"></param>
        ''' <remarks></remarks>
        Public Sub New(queriedenumeration As iormQueriedEnumeration)
            MyBase.New(queriedenumeration.id)
            _queriedenumeration = queriedenumeration

        End Sub

        ''' <summary>
        ''' Gets or sets the is loaded.
        ''' </summary>
        ''' <value>The is loaded.</value>
        Public Property IsLoaded() As Boolean
            Get
                Return _isloaded
            End Get
            Private Set(value As Boolean)
                _isloaded = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is initialized.
        ''' </summary>
        ''' <value>The is initialized.</value>
        Public Property IsInitialized() As Boolean
            Get
                Return Me._isInitialized
            End Get
            Private Set(value As Boolean)
                Me._isInitialized = Value
            End Set
        End Property

        ''' <summary>
        ''' returns the ObjectEntries handled in this ormModelTable
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntries() As IList(Of iormObjectEntry)
            Return _queriedenumeration.getobjectEntries
        End Function

        ''' <summary>
        ''' Initialize the Table with the columns from the query
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            If _isInitialized Then Return True


            Try
                ''' set up the columns
                ''' 
                Dim RowColumn As DataColumn = New DataColumn
                With RowColumn
                    .AllowDBNull = False
                    .ColumnName = constQRYRowReference
                    .DataType = GetType(Long)
                    .Unique = True
                End With
                Me.Columns.Add(RowColumn)
                Dim i As Integer = 1
                For Each aName In _queriedenumeration.ObjectEntryNames
                    Dim aColumn As New DataColumn
                    Dim anObjectEntry As iormObjectEntry = _queriedenumeration.GetObjectEntry(aName)
                    With aColumn
                        .ColumnName = anObjectEntry.Entryname
                        .Caption = anObjectEntry.Title
                        If _queriedenumeration.AreObjectsEnumerated Then
                            Dim aDescription = _queriedenumeration.GetObjectClassDescription
                            Dim aFieldinfo As Reflection.FieldInfo = aDescription.GetEntryFieldInfos(entryname:=aName).First
                            If aFieldinfo.FieldType.IsValueType OrElse aFieldinfo.FieldType.Equals(GetType(String)) Then
                                If Nullable.GetUnderlyingType(aFieldinfo.FieldType) IsNot Nothing Then
                                    .AllowDBNull = True
                                    .DataType = Nullable.GetUnderlyingType(aFieldinfo.FieldType)
                                ElseIf aFieldinfo.FieldType.Equals(GetType(String)) Then
                                    ''' TO DO String is not nullable but might be nothing
                                    ''' 
                                    If anObjectEntry.IsNullable Then
                                        .AllowDBNull = True
                                    Else
                                        .AllowDBNull = False
                                        .DefaultValue = ""
                                    End If
                                ElseIf aFieldinfo.FieldType.Equals(GetType(Object)) Then
                                    ''' TO DO String is not nullable but might be nothing
                                    ''' 
                                    If anObjectEntry.IsNullable Then
                                        .AllowDBNull = True
                                    Else
                                        .AllowDBNull = False
                                        .DefaultValue = ""
                                    End If
                                Else
                                    .DataType = aFieldinfo.FieldType
                                    .AllowDBNull = False
                                End If
                                If anObjectEntry.DefaultValue IsNot Nothing Then
                                    If .DataType.IsEnum Then
                                        .DefaultValue = CTypeDynamic([Enum].Parse(.DataType, anObjectEntry.DefaultValue), .DataType)
                                    Else
                                        ' .DefaultValue = CTypeDynamic(anObjectEntry.DefaultValue, .DataType)
                                    End If
                                End If

                                ' ERROR
                                'If .DataType.Equals(GetType(String)) Then .MaxLength = anObjectEntry.Size
                                '
                            Else
                                .DataType = GetType(String)
                                .DefaultValue = TryCast(anObjectEntry.DefaultValue, String)
                            End If
                        Else
                            Dim aType = ot.DatatypeMapping(anObjectEntry.Datatype)
                            If aType.IsValueType Then
                                .DataType = aType
                                .DefaultValue = anObjectEntry.DefaultValue
                                If .DataType.Equals(GetType(String)) Then .MaxLength = anObjectEntry.Size
                                .AllowDBNull = anObjectEntry.IsNullable
                            Else
                                .DataType = GetType(String)
                                .DefaultValue = anObjectEntry.DefaultValue
                            End If


                        End If


                    End With
                    Me.Columns.Add(aColumn)
                    aColumn.SetOrdinal(i)
                    i += 1
                Next

                _isInitialized = True
                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormModelTable.Initialize")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' loads data from the QryEnumeration in the table
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load() As Boolean
            If Not _isInitialized AndAlso Not Initialize() Then Return False

            Try
                ''' fill all the object entries in the corresponding columns
                ''' 
                For i As Long = 0 To _queriedenumeration.Count - 1
                    Dim anObject As iormPersistable = _queriedenumeration.GetObject(i)

                    Dim aRow As DataRow = Me.NewRow
                    ''' set the reference to the row no in the queriedenumeration
                    ''' 
                    aRow.Item(Me.constQRYRowReference) = i
                    ''' set the fields in the datatable
                    ''' 
                    Dim j As Integer = 1
                    For Each aName In _queriedenumeration.ObjectEntryNames
                        Dim aValue = anObject.getValue(aName)
                        If aValue Is Nothing Then aValue = DBNull.Value

                        If (aValue.GetType.IsValueType OrElse aValue.GetType.Equals(GetType(String))) AndAlso Not aValue.GetType.IsArray Then
                            aRow.Item(j) = CTypeDynamic(aValue, Me.Columns.Item(j).DataType)
                        ElseIf Not DBNull.Value.Equals(aValue) Then
                            aRow.Item(j) = Converter.Enumerable2String(aValue)
                        End If

                        j += 1
                    Next
                    Me.Rows.Add(aRow)
                Next

                Me.isloaded = True
                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormModelTable.Load")
                Return False
            End Try

        End Function
    End Class
End Namespace

