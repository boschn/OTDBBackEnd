

REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs CLASSES: Tracking Classes
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************


Option Explicit On
Imports System.Diagnostics
Imports System.Linq
Imports System.Diagnostics.Debug

Imports OnTrack.Database
Imports OnTrack

Namespace OnTrack.Deliverables


    '************************************************************************************
    '***** CLASS clsOTDBTrackItem list is a arbitrary List of trackable Items
    '*****
    '*****
    ''' <summary>
    ''' List of trackable items
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOTDBTrackItem
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        <ormSchemaTableAttribute(version:=1)> Public Const constTableID = "tblTrackItems"
        <ormSchemaIndexAttribute(columnname1:=constFNID, columnname2:=constFNOrder)> Public Const constIndexOrder = "orderby"
        <ormSchemaIndexAttribute(columnname1:=constFNID)> Public Const constIndexList = "lists"

        <ormSchemaColumnAttribute(iD:="TI1", title:="List ID", description:="name of the tracking item list", _
            typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1)> Public Const constFNID = "listid"

        <ormSchemaColumnAttribute(iD:="TI2", title:="List Pos", description:="entry number in the tracking item list", _
            typeid:=otFieldDataType.Long, primaryKeyordinal:=4)> Public Const constFNPos = "posno"

        <ormSchemaColumnAttribute(iD:="TI3", title:="part id", description:="part id of the item to be tracked", _
           typeid:=otFieldDataType.Text, size:=50)> Public Const constFNPartid = "partid"

        <ormSchemaColumnAttribute(iD:="TI4", title:="order", description:="ordinal in the list to be sorted", _
           typeid:=otFieldDataType.Long)> Public Const constFNOrder = "order"

        <ormSchemaColumnAttribute(iD:="TI5", title:="matchcode", description:="matchcode for items", _
           typeid:=otFieldDataType.Text, size:=100)> Public Const constFNPrecode = "precode"

        <ormSchemaColumnAttribute(iD:="TI7", title:="Deliverable UID", description:="UID of the deliverable to be tracked", _
          typeid:=otFieldDataType.Long, size:=100)> Public Const constFNDLVUID = "dlvuid"

        <ormSchemaColumnAttribute(iD:="TI6", title:="Comments", description:="comment for the item", _
         typeid:=otFieldDataType.Memo)> Public Const constFNComment = "cmt"

        <ormColumnMappingAttribute(ColumnName:=ConstFNID)> Private s_listid As String = ""
        <ormColumnMappingAttribute(ColumnName:=ConstFNPos)> Private s_posno As Long

        <ormColumnMappingAttribute(ColumnName:=ConstFNPartid)> Private s_pnid As String
        <ormColumnMappingAttribute(ColumnName:=ConstFNOrder)> Private s_order As Long
        <ormColumnMappingAttribute(ColumnName:=ConstFNComment)> Private s_cmt As String
        <ormColumnMappingAttribute(ColumnName:=ConstFNPrecode)> Private s_precode As String
        <ormColumnMappingAttribute(ColumnName:=ConstFNDLVUID)> Private s_DLVUID As Long

#Region "Properties"
        ReadOnly Property Listid() As String
            Get
                Listid = s_listid
            End Get

        End Property

        ReadOnly Property Posno() As Long
            Get
                Posno = s_posno
            End Get

        End Property

        Public Property PartID() As String
            Get
                PartID = s_pnid
            End Get
            Set(value As String)
                If value <> s_pnid Then
                    s_pnid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Comment() As String
            Get
                Comment = s_cmt
            End Get
            Set(value As String)
                s_cmt = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Precode() As String
            Get
                Precode = s_precode
            End Get
            Set(value As String)
                If s_precode <> value Then
                    s_precode = value
                    Me.IsChanged = True
                End If

            End Set
        End Property

        Public Property Order() As Long
            Get
                Order = s_order

            End Get
            Set(value As Long)
                If value <> s_order Then
                    s_order = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property DlvUid() As Long
            Get
                DlvUid = s_DLVUID
            End Get
            Set(value As Long)
                If s_DLVUID <> value Then
                    s_DLVUID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

#End Region
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(constTableID)
        End Sub
        ''' <summary>
        ''' loads the data object (the Track Item) from the store
        ''' </summary>
        ''' <param name="listid"></param>
        ''' <param name="posno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal listid As String, ByVal posno As Long) As Boolean
            Dim primarykey() As Object = {listid, posno}
            Return MyBase.LoadBy(primarykey)
        End Function

        ''' <summary>
        ''' creates the schema for persistency
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of clsOTDBTrackItem)(silent:=silent)
        End Function
        ''' <summary>
        ''' create a persistable track list item
        ''' </summary>
        ''' <param name="listid"></param>
        ''' <param name="posno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal listid As String, ByVal posno As Long) As Boolean
            Dim primarykey() As Object = {listid, posno}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                s_listid = listid
                s_posno = posno
                Return Me.IsCreated
            End If
            Return False
        End Function

        ''' <summary>
        ''' get the items by list
        ''' </summary>
        ''' <param name="listid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetTrackItemsList(listid As String) As Collection
            Dim aTable As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aCollection As New Collection
            Dim primarykey() As Object = {listid}
            ' set the primaryKey
            aTable = GetTableStore(constTableID)
            aRecordCollection = aTable.GetRecordsByIndex(indexname:=constIndexOrder, keyArray:=primarykey)

            If Not aRecordCollection Is Nothing AndAlso aRecordCollection.Count > 0 Then
                ' records read
                For Each aRecord In aRecordCollection
                    Dim anEntry As New clsOTDBTrackItem
                    If anEntry.Infuse(aRecord) Then
                        aCollection.Add(Item:=anEntry)
                    End If
                Next aRecord
            End If
            Return aCollection

        End Function

        ''' <summary>
        ''' retrieve a collection of all Items
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of clsOTDBTrackItem)
            Return ormDataObject.All(Of clsOTDBTrackItem)(ID:="all")
        End Function

    End Class
End Namespace


