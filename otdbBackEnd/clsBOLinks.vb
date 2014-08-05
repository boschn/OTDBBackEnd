REM ***********************************************************************************************************************************************
REM *********** BUSINESS OBJECTs: DELIVERABLE LINKS Classes for On Track Database Backend Library
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

Imports System.Collections.Generic

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.Parts
Imports OnTrack.IFM
Imports OnTrack.Scheduling
Imports OnTrack.Xchange
Imports OnTrack.Calendar
Imports OnTrack.Commons
Imports OnTrack.ObjectProperties

Namespace OnTrack.Deliverables

    ''' <summary>
    ''' Definition class for LinkTypes
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=LinkType.ConstObjectID, description:="type definition of a deliverable link. Defines default setting and some general logic.", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> _
    Public Class LinkType
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "LinkType"
        '** Table
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID = "tblDefDeliverableLinkTypes"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=constFNTypeID, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"

        ''' <summary>
        ''' keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primarykeyordinal:=1, _
           title:="Type", description:="type of the deliverable link", XID:="DLVLT1")> Public Const constFNTypeID = "id"
        ' switch FK too NOOP since we have a dependency to deliverables
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                    ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' Fields
        ''' </summary>
        ''' <remarks></remarks>
        ''' 


        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, _
         title:="Description", description:="description of the deliverable link type", XID:="DLVTL3")> Public Const constFNDescription = "desc"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, _
        title:="comment", description:="comments of the deliverable link type", XID:="DLVLT10")> Public Const constFNComment = "cmt"

        '*** Mapping
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String


#Region "Properties"



        ''' <summary>
        ''' Gets or sets the comment.
        ''' </summary>
        ''' <value>The comment.</value>
        Public Property Comment() As String
            Get
                Return Me._comment
            End Get
            Set(value As String)
                SetValue(constFNComment, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(constFNDescription, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the typeid.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public ReadOnly Property Typeid() As String
            Get
                Return Me._typeid
            End Get

        End Property
#End Region

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal typeid As String, Optional ByVal domainid As String = Nothing) As LinkType
            If String.IsNullOrWhiteSpace(domainid) Then domainid = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {typeid, domainid}
            Return CreateDataObject(Of LinkType)(pkArray:=primarykey, domainID:=domainid, checkUnique:=True)
        End Function


        ''' <summary>
        ''' Retrieve a deliverable Type object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal typeid As String, Optional ByVal domainid As String = Nothing, Optional forcereload As Boolean = False) As LinkType
            If String.IsNullOrWhiteSpace(domainid) Then domainid = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {typeid, domainid}
            Return Retrieve(Of LinkType)(pkArray:=pkarray, forceReload:=forcereload)
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of Delivertype) for the DomainID
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainid As String = Nothing) As List(Of LinkType)
            Dim aCollection As New List(Of LinkType)
            Dim aDomainDir As New Dictionary(Of String, LinkType)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            '** set the domain
            If String.IsNullOrWhiteSpace(domainid) Then domainid = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="all", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & CurrentSession.CurrentDBDriver.GetNativeTableName(ConstTableID) & "].[" & constFNTypeID & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainid)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aRecordCollection = aCommand.RunSelect

                '** get the entries for the domain and global sorted out
                For Each aRecord As ormRecord In aRecordCollection
                    Dim anewLinktype As New LinkType
                    If InfuseDataObject(record:=aRecord, dataobject:=anewLinktype) Then
                        If aDomainDir.ContainsKey(key:=anewLinktype.Typeid) Then
                            Dim anExist = aDomainDir.Item(key:=anewLinktype.Typeid)
                            If anExist.DomainID = ConstGlobalDomain And anewLinktype.DomainID = CurrentSession.CurrentDomainID Then
                                aDomainDir.Remove(key:=anewLinktype.Typeid)
                                aDomainDir.Add(key:=anewLinktype.Typeid, value:=anewLinktype)
                            End If
                        Else
                            aDomainDir.Add(key:=anewLinktype.Typeid, value:=anewLinktype)
                        End If
                    End If
                Next
                '** return the ist
                Return aDomainDir.Values.ToList

            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.All")
                Return aCollection

            End Try

        End Function
#End Region
    End Class

    ''' <summary>
    ''' describes a versionizable deliverable link to a deliverable from other deliverables (inbound or pointing to the deliverable)
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    <ormObject(id:=CurrentLink.ConstObjectID, description:="describes a current link from other objects", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
    Public Class CurrentLink
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable


        Public Const ConstObjectID = "CURRENTLINK"
        '** Schema Table
        <ormSchemaTable(Version:=1)> Public Const ConstTableID = "TBLCURRDLVLINKS"

        ''' <summary>
        ''' Index
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaIndex(columnname1:=ConstFNTOUID, columnName2:=constFNLinkTypeID, columnname3:=ConstFNLinkUid)> Public Const constIndexFrom = "indfrom"
        <ormSchemaIndex(columnName1:=constFNLinkTypeID, columnname2:=ConstFNTOUID, columnname3:=ConstFNLinkUid)> Public Const constIndexTypeto = "indtypeto"

        ''' <summary>
        ''' Primary Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Long, title:="LINK ID", Description:="Unique ID of the link", _
           lowerrange:=0, primaryKeyordinal:=1, XID:="DLVCL1", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNLinkUid = "LUID"

        ''' <summary>
        ''' columns
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
                        xid:="DLVCL2", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNTOUID = "TOUID"

        <ormObjectEntry(referenceobjectentry:=LinkType.ConstObjectID & "." & LinkType.constFNTypeID, _
            title:="Type", description:="type of the deliverable link", XID:="DLVCL3", _
            LookupPropertyStrings:={LookupProperty.UseAttributeReference}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNLinkTypeID = "TYPEID"

        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
        title:="working counter", description:="update number of the working target", XID:="DLVCL10")> Public Const ConstFNWorkUPDC = "workupdc"
        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
         title:="Alive Counter", description:="update number of the alive target", XID:="DLVCL11")> Public Const ConstFNAliveUPDC = "aliveupdc"

      
        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
              useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** mappings
        <ormEntryMapping(EntryName:=ConstFNLinkUid)> Private _linkuid As Long
        <ormEntryMapping(EntryName:=ConstFNTOUID)> Private _touid As Long
        <ormEntryMapping(EntryName:=constFNLinkTypeID)> Private _typeid As String

        <ormEntryMapping(EntryName:=ConstFNWorkUPDC)> Private _workupdc As Long
        <ormEntryMapping(EntryName:=ConstFNAliveUPDC)> Private _aliveupdc As Long

        ''' <summary>
        ''' dynamic data
        ''' </summary>
        ''' <remarks></remarks>


#Region "Properties"

       

        ''' <summary>
        ''' Gets or sets the aliveupdc.
        ''' </summary>
        ''' <value>The aliveupdc.</value>
        Public Property Aliveupdc() As Long
            Get
                Return Me._aliveupdc
            End Get
            Set(value As Long)
                SetValue(ConstFNAliveUPDC, value)
                Me._aliveupdc = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the workupdc.
        ''' </summary>
        ''' <value>The workupdc.</value>
        Public Property Workupdc() As Long
            Get
                Return Me._workupdc
            End Get
            Set
                Me._workupdc = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the typeid.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public Property Typeid() As String
            Get
                Return Me._typeid
            End Get
            Set(value As String)
                SetValue(constFNLinkTypeID, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the TO UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ToUID() As Long
            Get
                Return _touid
            End Get
        End Property
        ''' <summary>
        ''' returns the From UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property LinkUID() As Long
            Get
                Return _linkuid
            End Get
        End Property


#End Region



    End Class

    ''' <summary>
    ''' describes a deliverable link to a deliverable from other deliverables (inbound or pointing to the deliverable)
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=Link.ConstObjectID, description:="describes a link from other objects", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
    Public Class Link
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable


        Public Const ConstObjectID = "LINK"
        '** Schema Table
        <ormSchemaTable(Version:=1)> Public Const ConstTableID = "TBLDLVLINKS"

        '** indexes
        <ormSchemaIndex(columnname1:=ConstFNFROMUID, columnname2:=ConstFNTOUID, columnName3:=constFNLinkTypeID)> Public Const constIndexFrom = "indfrom"
        <ormSchemaIndex(columnName1:=constFNLinkTypeID, columnname2:=ConstFNTOUID, columnname3:=ConstFNFROMUID)> Public Const constIndexTypeto = "indtypeto"
        <ormSchemaIndex(columnName1:=constFNLinkTypeID, columnname2:=ConstFNFROMUID, columnname3:=ConstFNTOUID)> Public Const constIndexTypeFrom = "indtypefrom"
        <ormSchemaIndex(columnname1:=ConstFNDESCUID, columnname2:=ConstFNTOUID, columnname3:=ConstFNFROMUID, columnName4:=constFNLinkTypeID)> Public Const constIndexDescUID = "indDescUID"


        ''' <summary>
        ''' Primary Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=CurrentLink.ConstObjectID & "." & CurrentLink.ConstFNLinkUid, primaryKeyordinal:=1, _
          lowerrange:=0, XID:="DLVL1")> Public Const ConstFNLinkUid = "LUID"
        <ormObjectEntry(Datatype:=otDataType.Long, title:="update count", Description:="Update count of the link", primaryKeyordinal:=2, _
            lowerrange:=0, XID:="DLVL2")> Public Const ConstFNUpdc = "UPDC"

       
        ''' <summary>
        ''' other Columns
        ''' </summary>
        ''' <remarks></remarks>

        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
                      xid:="DLVL3", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNFROMUID = "FROMUID"
        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
                        xid:="DLVL4", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNTOUID = "TOUID"
      

        <ormObjectEntry(referenceobjectentry:=LinkType.ConstObjectID & "." & LinkType.constFNTypeID, _
            title:="Type", description:="type of the deliverable link", XID:="DLVL5", _
            LookupPropertyStrings:={LookupProperty.UseAttributeReference}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNLinkTypeID = "typeid"

        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, isnullable:=True, _
                        Title:="describing deliverable", description:="link is described by this deliverable by uid", _
                       xid:="DLVL11", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDESCUID = "DESCUID"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
            title:="is active", description:="is the dependency active", XID:="DLVL12")> Public Const ConstFNIsActive = "ISACTIVE"

        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, _
            XID:="DLVL13", title:="valid from", description:="link is valid from ")> Public Const ConstFNValidFrom = "validfrom"

        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, _
            XID:="DLVL14", title:="valid until", description:="link is valid until ")> Public Const ConstFNValiduntil = "validuntil"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
              useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNLinkUid)> Private _linkuid As Long
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long
        <ormEntryMapping(EntryName:=ConstFNTOUID)> Private _touid As Long
        <ormEntryMapping(EntryName:=ConstFNFROMUID)> Private _fromuid As Long
        <ormEntryMapping(EntryName:=constFNLinkTypeID)> Private _typeid As String
        <ormEntryMapping(EntryName:=ConstFNDESCUID)> Private _descuid As Long
        <ormEntryMapping(EntryName:=ConstFNIsActive)> Private _isActive As Boolean = True 'explicitly set to be active in the beginning !
        <ormEntryMapping(EntryName:=ConstFNValidFrom)> Private _validfrom As DateTime?
        <ormEntryMapping(EntryName:=ConstFNValiduntil)> Private _validuntil As DateTime?

        ''' <summary>
        ''' dynamic data
        ''' </summary>
        ''' <remarks></remarks>


#Region "Properties"
        ''' <summary>
        ''' Gets or sets the validto date.
        ''' </summary>
        ''' <value>The validto.</value>
        Public Property ValidUntil() As DateTime?
            Get
                Return Me._validuntil
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNValiduntil, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the validfrom.
        ''' </summary>
        ''' <value>The validfrom.</value>
        Public Property Validfrom() As DateTime?
            Get
                Return Me._validfrom
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNValidFrom, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the UID of the describing deliverable of the link.
        ''' </summary>
        ''' <value>The descuid.</value>
        Public Property DescribedByUID() As Long
            Get
                Return Me._descuid
            End Get
            Set(value As Long)
                SetValue(ConstFNDESCUID, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the typeid.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public Property Typeid() As String
            Get
                Return Me._typeid
            End Get
            Set(value As String)
                SetValue(constFNLinkTypeID, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the TO UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ToUID() As Long
            Get
                Return _touid
            End Get
        End Property
        ''' <summary>
        ''' returns the From UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromUID() As Long
            Get
                Return _fromuid
            End Get
        End Property

        ''' <summary>
        ''' gets or sets the active flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActive() As Boolean
            Get
                IsActive = _isActive
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsActive, value)
            End Set
        End Property


#End Region



    End Class
End Namespace
