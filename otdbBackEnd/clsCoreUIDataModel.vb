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

        ''' <summary>
        '''  Event Args
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EventArgs
            Inherits System.EventArgs

            Private _row As DataRow
            Private _object As iormPersistable
            Private _exception As Exception
            Private _message As String
            Public Sub New(Optional row As DataRow = Nothing, Optional [object] As iormPersistable = Nothing, Optional exception As Exception = Nothing, Optional message As String = Nothing)
                _row = row
                _object = [object]
                _exception = exception
                _message = message
            End Sub
            ''' <summary>
            ''' Gets the message.
            ''' </summary>
            ''' <value>The message.</value>
            Public ReadOnly Property Message() As String
                Get
                    Return Me._message
                End Get
            End Property

            ''' <summary>
            ''' Gets the exception.
            ''' </summary>
            ''' <value>The exception.</value>
            Public ReadOnly Property Exception() As Exception
                Get
                    Return Me._exception
                End Get
            End Property

            ''' <summary>
            ''' Gets the object.
            ''' </summary>
            ''' <value>The object.</value>
            Public ReadOnly Property [Object]() As iormPersistable
                Get
                    Return Me._object
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the row.
            ''' </summary>
            ''' <value>The row.</value>
            Public Property Row() As DataRow
                Get
                    Return Me._row
                End Get
                Set
                    Me._row = Value
                End Set
            End Property

        End Class

        ''' <summary>
        '''  internal variables
        ''' </summary>
        ''' <remarks></remarks>
        Private _id As String = ""
        Private _queriedenumeration As iormQueriedEnumeration
        Private _isInitialized As Boolean = False
        Private _isloaded As Boolean = False
        Private _isloading As Boolean = False
        Private _ChangedColumns As New Dictionary(Of String, Object)

        ''' <summary>
        ''' public constants
        ''' </summary>
        ''' <remarks></remarks>
        Public Const constQRYRowReference = "$$QRYRowReference"

        ''' <summary>
        ''' public events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event ObjectPersistFailed(sender As Object, e As ormModelTable.EventArgs)
        Public Event ObjectDeleteFailed(sender As Object, e As ormModelTable.EventArgs)
        Public Event ObjectUpdateFailed(sender As Object, e As ormModelTable.EventArgs)
        Public Event ObjectReferenceMissing(sender As Object, e As ormModelTable.EventArgs)
        Public Event ObjectCreateFailed(sender As Object, e As ormModelTable.EventArgs)
        Public Event OperationMessage(sender As Object, e As ormModelTable.EventArgs)
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="queriedenumeration"></param>
        ''' <remarks></remarks>
        Public Sub New(queriedenumeration As iormQueriedEnumeration)
            MyBase.New(queriedenumeration.id)
            _queriedenumeration = queriedenumeration
            _id = queriedenumeration.ID
        End Sub

#Region "Property"


        ''' <summary>
        ''' Gets the id.
        ''' </summary>
        ''' <value>The id.</value>
        Public ReadOnly Property Id() As String
            Get
                Return Me._id
            End Get
        End Property

        ''' <summary>
        ''' gets the object id of the qry enumeration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectID() As String
            Get
                If _queriedenumeration.AreObjectsEnumerated Then
                    Return Me._queriedenumeration.GetObjectDefinition.ID
                Else
                    Return Nothing
                End If

            End Get
        End Property
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
        ''' Gets or sets the isLoading Flag
        ''' </summary>
        ''' <value>The is loaded.</value>
        Public Property IsLoading() As Boolean
            Get
                Return _isloading
            End Get
            Private Set(value As Boolean)
                _isloading = value
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

#End Region

        ''' <summary>
        ''' returns the ObjectEntries handled in this ormModelTable
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntries() As IOrderedEnumerable(Of iormObjectEntry)
            Return _queriedenumeration.GetObjectEntries
        End Function

        ''' <summary>
        ''' Initialize the Table with the columns from the query
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            If _isInitialized Then Return True


            Try
                _ChangedColumns.Clear()

                ''' set up the row reference since a datarow has no index or tag
                ''' 
                Dim RowColumn As DataColumn = New DataColumn
                With RowColumn
                    .ColumnName = Me.constQRYRowReference
                    .DataType = GetType(ULong)
                    .Unique = True
                End With
                Me.Columns.Add(RowColumn)

                ''' set up the columns
                ''' 
                Dim i As Integer = 1
                For Each aName In _queriedenumeration.ObjectEntryNames
                    Dim aColumn As New DataColumn
                    Dim anObjectEntry As iormObjectEntry = _queriedenumeration.GetObjectEntry(aName)

                    ''' create a Column
                    ''' in the table
                    With aColumn
                        .ColumnName = anObjectEntry.Entryname.ToUpper
                        .Caption = anObjectEntry.Title
                        .ReadOnly = anObjectEntry.IsReadonly
                        ''' Objects
                        ''' 
                        If _queriedenumeration.AreObjectsEnumerated Then
                            Dim aDescription = _queriedenumeration.GetObjectClassDescription
                            Dim aFieldinfo As Reflection.FieldInfo = aDescription.GetEntryFieldInfos(entryname:=aName).First

                            ''' valuetypes or string
                            ''' 
                            If aFieldinfo.FieldType.IsValueType OrElse aFieldinfo.FieldType.Equals(GetType(String)) Then
                                ''' nullable type
                                If Nullable.GetUnderlyingType(aFieldinfo.FieldType) IsNot Nothing Then
                                    .AllowDBNull = True
                                    .DataType = Nullable.GetUnderlyingType(aFieldinfo.FieldType)
                                Else
                                    .DataType = aFieldinfo.FieldType
                                    .AllowDBNull = anObjectEntry.IsNullable
                                End If
                                If anObjectEntry.DefaultValue IsNot Nothing Then .DefaultValue = CTypeDynamic(anObjectEntry.DefaultValue, .DataType)

                                ''' nor valuetype or object put it to string
                                ''' 
                            Else
                                .DataType = GetType(String)
                                If anObjectEntry.DefaultValue IsNot Nothing Then .DefaultValue = anObjectEntry.DefaultValue.ToString
                            End If

                            ''' Records
                            ''' 
                        Else
                            Dim aType = ot.GetDatatypeMappingOf(anObjectEntry.Datatype)
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

                RaiseEvent OperationMessage(Me, New ormModelTable.EventArgs(message:="table initialized"))
                _isInitialized = True
                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormModelTable.Initialize")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns the ObjectEntry Definition of a column
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntry(columnname As String) As iormObjectEntry
            If Me.Columns.Contains(columnname.ToUpper) Then
                Return _queriedenumeration.GetObjectEntry(name:=columnname.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' loads data from the QryEnumeration in the table
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load(Optional refresh As Boolean = False) As Boolean
            If Not _isInitialized AndAlso Not Initialize() Then Return False
            '** clear all rows
            Me.Rows.Clear()
            If refresh Then _queriedenumeration.Reset()

            Try
                ''' fill all the object entries in the corresponding columns
                ''' 
                For i As Long = 0 To _queriedenumeration.Count - 1
                    Dim anObject As iormPersistable = _queriedenumeration.GetObject(i)
                    Me.IsLoading = True
                    Dim aRow As DataRow = Me.NewRow
                    ''' set the reference to the row no in the queriedenumeration
                    ''' 
                    aRow.Item(Me.constQRYRowReference) = i
                    ''' set the fields in the datatable
                    ''' 
                    Dim j As Integer = 1
                    For Each aName In _queriedenumeration.ObjectEntryNames
                        Dim aValue = anObject.GetValue(aName)
                        If aValue Is Nothing Then aValue = DBNull.Value

                        If (aValue.GetType.IsValueType OrElse aValue.GetType.Equals(GetType(String))) AndAlso Not aValue.GetType.IsArray Then
                            aRow.Item(j) = CTypeDynamic(aValue, Me.Columns.Item(j).DataType)
                        ElseIf Not DBNull.Value.Equals(aValue) Then
                            aRow.Item(j) = Converter.Enumerable2otString(aValue)
                        End If

                        j += 1
                    Next

                    Me.Rows.Add(aRow)
                    Me.IsLoading = False
                Next

                RaiseEvent OperationMessage(Me, New ormModelTable.EventArgs(message:="data loaded from database"))
                Me.IsLoaded = True
                Return True

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormModelTable.Load")
                Me.IsLoading = False
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Event handler for the Delete Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDeleting(sender As Object, e As DataRowChangeEventArgs) Handles Me.RowDeleting
            If Not Me.isloading Then
                If _queriedenumeration.AreObjectsEnumerated Then
                    Dim aValue As Object = e.Row.Item(Me.constQRYRowReference)
                    If aValue Is Nothing Then
                        RaiseEvent ObjectReferenceMissing(Me, New ormModelTable.EventArgs(row:=e.Row))
                        Exit Sub
                    End If
                    Dim i As ULong = Convert.ToUInt64(aValue)
                    If Not _queriedenumeration.DeleteObject(i) Then
                        Dim anObject As iormPersistable = _queriedenumeration.GetObject(i)
                        RaiseEvent ObjectDeleteFailed(Me, New ormModelTable.EventArgs(row:=e.Row, [object]:=anObject))
                        Exit Sub
                    Else
                        RaiseEvent OperationMessage(Me, New ormModelTable.EventArgs(message:="object deleted in database"))
                    End If

                End If
            End If
        End Sub

        ''' <summary>
        ''' Event handler for the RowChanged Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnChanging(sender As Object, e As DataRowChangeEventArgs) Handles Me.RowChanging
            If Not Me.isloading Then
                If e.Action = DataRowAction.Change Or e.Action = DataRowAction.Add Then
                    ''' nothing yet
                End If
            End If


        End Sub
        ''' <summary>
        ''' Event handler for the RowChanged Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRowChanged(sender As Object, e As DataRowChangeEventArgs) Handles Me.RowChanged

            If Not Me.IsLoading Then
                ''' persist the object changed if running on objects
                If _queriedenumeration.AreObjectsEnumerated Then

                    ''' Add DataRow to Objects
                    ''' 
                    If e.Action = DataRowAction.Add Then
                        If AddNewObject(e.Row) Then
                            RaiseEvent OperationMessage(Me, New ormModelTable.EventArgs(message:="object added and stored to database"))
                        End If
                        Exit Sub

                        ''' change object
                        ''' 
                    ElseIf e.Action = DataRowAction.Change Then

                        If UpdateObject(e.Row) Then
                            RaiseEvent OperationMessage(Me, New ormModelTable.EventArgs(message:="object updated in database"))
                        End If
                        Exit Sub
                    End If

                End If
            End If

        End Sub
        ''' <summary>
        ''' Event handler for the ColumnChanged Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnColumnChanged(sender As Object, e As DataColumnChangeEventArgs) Handles Me.ColumnChanged
            If Not Me.isloading Then
                ''' not change on objects entries
                If _queriedenumeration.AreObjectsEnumerated Then
                    If _ChangedColumns.ContainsKey(e.Column.ColumnName) Then
                        _ChangedColumns.Remove(key:=e.Column.ColumnName)
                    End If
                    _ChangedColumns.Add(e.Column.ColumnName, value:=e.ProposedValue)
                End If
            End If
        End Sub

        ''' <summary>
        ''' gets the iormpersistable index of the row by number
        ''' </summary>
        ''' <param name="index"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DataObject(rowno As UInteger) As iormPersistable
            Get
                Dim avalue As Object = Me.Rows(rowno).Item(Me.constQRYRowReference)
                Dim i As ULong = Convert.ToUInt64(avalue)
                Dim anObject As iormPersistable = _queriedenumeration.GetObject(i)
                Return anObject
            End Get
        End Property

        ''' <summary>
        ''' update an object from a row
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function UpdateObject(row As DataRow) As Boolean
            Dim aValue As Object = row.Item(Me.constQRYRowReference)
            Dim result As Boolean = True
            Dim changed As Boolean = False

            If aValue Is Nothing Then
                RaiseEvent ObjectReferenceMissing(Me, New ormModelTable.EventArgs(row:=row))
                Exit Function
            End If
            Dim i As ULong = Convert.ToUInt64(aValue)
            Dim anObject As iormPersistable = Me.DataObject(i)
            ''' set the values
            ''' 
            For Each aColumnname In _ChangedColumns.Keys
                aValue = row.Item(aColumnname)
                If _queriedenumeration.GetObjectDefinition.HasEntry(aColumnname) Then
                    result = result And anObject.SetValue(entryname:=aColumnname, value:=aValue)
                    If Not result Then
                        RaiseEvent ObjectUpdateFailed(Me, New ormModelTable.EventArgs(row:=row, object:=anObject))
                        Return False
                    Else
                        changed = True
                        Try
                            Me.IsLoading = True
                            Dim areturnValue As Object = anObject.GetValue(entryname:=aColumnname) '' maybe the object is slightly changed
                            If areturnValue Is Nothing Then areturnValue = DBNull.Value

                            If (areturnValue.GetType.IsValueType OrElse areturnValue.GetType.Equals(GetType(String))) AndAlso Not areturnValue.GetType.IsArray Then
                                row.Item(aColumnname) = CTypeDynamic(aValue, Me.Columns.Item(aColumnname).DataType)
                            ElseIf Not DBNull.Value.Equals(areturnValue) Then
                                row.Item(aColumnname) = Converter.Enumerable2otString(areturnValue)
                            End If
                            Me.IsLoading = False
                        Catch ex As Exception
                            CoreMessageHandler(exception:=ex, subname:="ormModelTable.UpdateObject")
                            Me.IsLoading = False
                        End Try
                    End If
                End If

            Next
            ''' persist
            ''' 
            If changed Then
                If Not anObject.Persist Then
                    RaiseEvent ObjectPersistFailed(Me, New ormModelTable.EventArgs(row:=row))
                    Return False
                Else
                    _ChangedColumns.Clear()
                End If
            End If


            Return True
        End Function
        ''' <summary>
        ''' Add a new Object out of the row
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddNewObject(row As DataRow) As Boolean
            If Not Me.IsLoading Then
                ''' check on the new row
                ''' 
                If _queriedenumeration.AreObjectsEnumerated Then
                    Try

                        Me.IsLoading = True ' set loading
                        Dim anObjectdefinition = _queriedenumeration.GetObjectDefinition
                        Dim pknames As List(Of String) = anObjectdefinition.GetKeyNames
                        Dim pkarray As Object()
                        ReDim pkarray(pknames.Count - 1)
                        ''' set the primary keys
                        For j As UShort = 0 To pknames.Count - 1
                            pkarray(j) = row.Item(pknames(j))
                        Next
                        ''' create object and set all the data we have
                        Dim anObject As iormPersistable = ormDataObject.CreateDataObject(pkArray:=pkarray, type:=Type.GetType(anObjectdefinition.Classname))
                        For Each aColumn As DataColumn In row.Table.Columns
                            If Not pknames.Contains(aColumn.ColumnName) AndAlso anObjectdefinition.HasEntry(aColumn.ColumnName) Then
                                Dim aValue As Object = row.Item(aColumn.ColumnName)
                                Dim aDefaultValue = anObject.GetValue(aColumn.ColumnName)
                                ''' set initial values
                                If aValue IsNot Nothing AndAlso Not DBNull.Value.Equals(aValue) Then
                                    If Not anObject.SetValue(aColumn.ColumnName, aValue) Then
                                        RaiseEvent ObjectCreateFailed(Me, New ormModelTable.EventArgs(row:=row, [object]:=anObject))
                                        Me.IsLoading = False
                                        Exit Function
                                    Else
                                        Try
                                            Me.IsLoading = True
                                            row.Item(aColumn.ColumnName) = anObject.GetValue(entryname:=aColumn.ColumnName) '' maybe the object is slightly changed
                                            Me.IsLoading = False
                                        Catch ex As Exception
                                            CoreMessageHandler(exception:=ex, subname:="ormModelTable.UpdateObject")
                                            Me.IsLoading = False
                                        End Try
                                    End If
                                ElseIf aDefaultValue IsNot Nothing Then
                                    Try
                                        Me.IsLoading = True
                                        row.Item(aColumn.ColumnName) = aDefaultValue
                                        Me.IsLoading = False
                                    Catch ex As Exception
                                        CoreMessageHandler(exception:=ex, subname:="ormModelTable.UpdateObject")
                                        Me.IsLoading = False
                                    End Try
                                End If
                            End If
                        Next
                        Dim i As ULong
                        If _queriedenumeration.AddObject(anObject, i) Then
                            row.Item(Me.constQRYRowReference) = i
                        End If
                        Me.IsLoading = False
                        If Not anObject.Persist() Then
                            RaiseEvent ObjectCreateFailed(Me, New ormModelTable.EventArgs(row:=row, [object]:=anObject))
                            Me.IsLoading = False
                            Exit Function
                        End If

                    Catch ex As Exception
                        RaiseEvent ObjectCreateFailed(Me, New ormModelTable.EventArgs(row:=row, exception:=ex))
                        Me.IsLoading = False
                        Exit Function
                    End Try
                End If
            End If
        End Function
        ''' <summary>
        ''' Event handler for the TableNewRow Event (provides empty row)
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnNewRowAdded(sender As Object, e As DataTableNewRowEventArgs) Handles Me.TableNewRow
        End Sub

    End Class
End Namespace

