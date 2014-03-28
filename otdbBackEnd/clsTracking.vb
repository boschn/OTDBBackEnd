

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

    ''' <summary>
    ''' List of Tracking Items
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=TrackItem.constobjectID, version:=1, modulename:=ot.ConstModuleDeliverables, Description:="member of tracking lists" _
        )> Public Class TrackItem
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '***
        Public Const constObjectID = "TrackItem"
        '*** TABLE
        <ormSchemaTableAttribute(version:=1, addsparefields:=True, adddeletefieldbehavior:=True)> Public Const constTableID = "tblTrackItems"
        '** Index
        <ormSchemaIndexAttribute(columnname1:=constFNID, columnname2:=constFNOrdinal)> Public Const constIndexOrder = "orderby"
        <ormSchemaIndexAttribute(columnname1:=constFNID)> Public Const constIndexList = "lists"

        '** Primary Keys
        <ormObjectEntry(XID:="TI1", title:="List ID", description:="name of the tracking item list", _
            typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1)> Public Const constFNID = "listid"

        <ormObjectEntry(XID:="TI2", title:="List Pos", description:="entry number in the tracking item list", _
            typeid:=otFieldDataType.Long, primaryKeyordinal:=2)> Public Const constFNPos = "posno"

        '*** fields
        <ormObjectEntry(referenceObjectentry:=Parts.Part.ConstObjectID & "." & Parts.Part.ConstFNPartID, _
            XID:="TI3", description:="part id of the item to be tracked", _
           isnullable:=True, useforeignkey:=otForeignKeyImplementation.ORM)> Public Const constFNPartid = "partid"

        <ormObjectEntry(XID:="TI4", title:="order", description:="ordinal in the list to be sorted", _
           typeid:=otFieldDataType.Long)> Public Const constFNOrdinal = "order"

        <ormObjectEntry(XID:="TI5", title:="matchcode", description:="matchcode for items", _
           typeid:=otFieldDataType.Text, size:=100)> Public Const constFNMatchCode = "MATCHCODE"

        <ormObjectEntry(referenceObjectentry:=Deliverables.Deliverable.ConstObjectID & "." & Deliverables.Deliverable.constFNUid, _
                XID:="TI7", description:="UID of the deliverable to be tracked", _
          isnullable:=True, useforeignkey:=otForeignKeyImplementation.ORM)> Public Const constFNDLVUID = "DLVUID"

        <ormObjectEntry(XID:="TI6", title:="Comments", description:="comment for the item", _
         typeid:=otFieldDataType.Memo)> Public Const constFNComment = "cmt"

        '*** Mappings
        <ormEntryMapping(EntryName:=constFNID)> Private _listid As String = ""
        <ormEntryMapping(EntryName:=constFNPos)> Private _posno As Long
        <ormEntryMapping(EntryName:=constFNPartid)> Private _pnid As String
        <ormEntryMapping(EntryName:=constFNOrdinal)> Private _ordinal As Long
        <ormEntryMapping(EntryName:=constFNComment)> Private _cmt As String
        <ormEntryMapping(EntryName:=constFNMatchCode)> Private _matchcode As String = ""
        <ormEntryMapping(EntryName:=constFNDLVUID)> Private _dlvuid As Long?

#Region "Properties"
        ''' <summary>
        ''' gets the id of the tracking list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Listid() As String
            Get
                Return _listid
            End Get

        End Property
        ''' <summary>
        ''' gets the position number in the list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Posno() As Long
            Get
                Return _posno
            End Get

        End Property

        ''' <summary>
        ''' gets or set the part id to be tracked - might be null / nothing
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PartID() As String
            Get
                Return _pnid
            End Get
            Set(value As String)
                SetValue(constFNPartid, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets some comments and textfield
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Return _cmt
            End Get
            Set(value As String)
                SetValue(constFNComment, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the matchcode
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Matchcode() As String
            Get
                Return _matchcode
            End Get
            Set(value As String)
                SetValue(constFNMatchCode, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ordinal in the list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Ordinal() As Long
            Get
                Return _ordinal

            End Get
            Set(value As Long)
                SetValue(constFNOrdinal, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the deliverable uid to be tracked - might be nothing / nullable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DlvUid() As Long?
            Get
                Return _dlvuid
            End Get
            Set(value As Long?)
                SetValue(constFNDLVUID, value)
            End Set
        End Property

#End Region
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
        End Sub
       

        ''' <summary>
        ''' Retrieve a trackitem from the data store
        ''' </summary>
        ''' <param name="listid"></param>
        ''' <param name="posno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal listid As String, ByVal posno As Long) As TrackItem
            Dim primarykey() As Object = {listid, posno}
            Return ormDataObject.Retrieve(Of TrackItem)(primarykey)
        End Function

       
        ''' <summary>
        ''' create a persistable track list item
        ''' </summary>
        ''' <param name="listid"></param>
        ''' <param name="posno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal listid As String, ByVal posno As Long) As TrackItem
            Dim primarykey() As Object = {listid, posno}
            Return ormDataObject.CreateDataObject(Of TrackItem)(primarykey, checkUnique:=True)
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
                    Dim anEntry As New TrackItem
                    If InfuseDataObject(record:=aRecord, dataobject:=anEntry) Then
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
        Public Shared Function All() As List(Of TrackItem)
            Return ormDataObject.AllDataObject(Of TrackItem)(ID:="all")
        End Function

    End Class
End Namespace


