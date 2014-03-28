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
        Private _isLoaded As Boolean = False

        Private Const constQRYRowReference = "$$QRYRowReference"
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
                Return Me._isLoaded
            End Get
            Private Set(value As Boolean)
                Me._isLoaded = Value
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
                Dim RowColumn As DataColumn
                With RowColumn
                    .AllowDBNull = False
                    .ColumnName = constQRYRowReference
                    .DataType = GetType(Long)
                    .Unique = True
                End With
                Dim i As Integer = 2
                For Each aName In _queriedenumeration.ObjectEntryNames
                    Dim aColumn As DataColumn
                    Dim anObjectEntry As iormObjectEntry = _queriedenumeration.GetObjectEntry(aName)
                    With aColumn
                        .AllowDBNull = anObjectEntry.IsNullable
                        .ColumnName = anObjectEntry.Entryname
                        .DefaultValue = anObjectEntry.DefaultValue
                        .DataType = ot.DatatypeMapping(anObjectEntry.Datatype)
                        .SetOrdinal(i)
                        .MaxLength = anObjectEntry.Size
                        .Caption = anObjectEntry.Title
                    End With
                    Me.Columns.Add(aColumn)
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
                For i As Long = 0 To _queriedenumeration.Count
                    Dim anObject As iormPersistable = _queriedenumeration.GetObject(i)

                    Dim aRow As DataRow = Me.NewRow
                    ''' set the reference to the row no in the queriedenumeration
                    ''' 
                    aRow.Item(Me.constQRYRowReference) = i
                    ''' set the fields in the datatable
                    ''' 
                    Dim j As Integer = 2
                    For Each aName In _queriedenumeration.ObjectEntryNames
                        aRow.Item(j) = anObject.getValue(aName)
                        j += 1
                    Next
                    Me.Rows.Add(aRow)
                Next

                _isLoaded = True
                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormModelTable.Load")
                Return False
            End Try
          
        End Function
    End Class
End Namespace

