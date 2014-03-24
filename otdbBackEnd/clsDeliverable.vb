REM ***********************************************************************************************************************************************
REM *********** BUSINESS OBJECTs: DELIVERABLES Classes for On Track Database Backend Library
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
Imports OnTrack.XChange

Namespace OnTrack.Deliverables


    '************************************************************************************
    '***** CLASS clsOTDBCurrTarget is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' Current target object points to the current clsOTDBDeliverableTarget 
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=CurrentTarget.ConstObjectID, modulename:=ConstModuleDeliverables, Version:=1, useCache:=True)> _
    Public Class CurrentTarget
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of CurrentTarget)

        Public Const ConstObjectID = "CurrentTarget"
        '** Schema Table
        <ormSchemaTable(Version:=3, adddeletefieldbehavior:=True, addDomainBehavior:=True, addspareFields:=True)> Public Const ConstTableID = "tblCurrTargets"

        '** PrimaryKey
        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, primarykeyordinal:=1, _
                       useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNWorkspace = Schedule.ConstFNWorkspace

        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, primarykeyordinal:=2, _
                        useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
                        XID:="CDT1", aliases:={"UID"})> Public Const ConstFNUid = Deliverable.constFNUid

        '** other columns
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, _
           title:="Revision", description:="revision of the target", XID:="T9")> Public Const ConstFNRevision = "rev"
        <ormObjectEntry(typeid:=otFieldDataType.Long, size:=100, _
         title:="UpdateCount", description:="update number of the target", XID:="T10")> Public Const ConstFNUpdc = "updc"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, _
          title:="is active", description:="is the target active", XID:="DT4")> Public Const ConstFNIsActive = "isactive"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID
        '** mappings
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String = ""
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long    ' UPDC of target
        <ormEntryMapping(EntryName:=ConstFNIsActive)> Private _isActive As Boolean

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub




#Region "Properties"

        ReadOnly Property UID() As Long
            Get
                UID = _uid
            End Get
        End Property

        Public Property WorkspaceID() As String
            Get
                WorkspaceID = _workspace
            End Get
            Set(value As String)
                If UCase(value) <> _workspace Then
                    _workspace = UCase(value)
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Revision() As String
            Get
                Revision = _rev
            End Get
            Set(value As String)
                If value <> _rev Then
                    _rev = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property UPDC() As Long
            Get
                UPDC = _updc
            End Get
            Set(value As Long)
                If value <> _updc Then
                    _updc = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsActive() As Boolean
            Get
                IsActive = _isActive
            End Get
            Set(value As Boolean)
                If value <> _isActive Then
                    _isActive = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

#End Region
        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(pkarray() As Object) As CurrentTarget Implements iotCloneable(Of CurrentTarget).Clone
            Return MyBase.Clone(Of CurrentTarget)(pkarray)
        End Function
        ''' <summary>
        ''' Clone this data object by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal uid As Long, Optional ByVal workspaceID As String = "") As CurrentTarget
            Dim pkarray() As Object = {uid, workspaceID}
            Return Me.Clone(Of CurrentTarget)(pkarray)
        End Function
        ''' <summary>
        ''' returns a collection of objects filtered by uid
        ''' </summary>
        ''' <param name="uid">deliverable uid</param>
        ''' <returns>a collection</returns>
        ''' <remarks></remarks>
        Public Shared Function AllByUID(uid As Long) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aTable As iormDataStore

            Try

                aTable = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand("AllByUI")

                If Not aCommand.Prepared Then
                    aCommand.Where = " uid = @UID "
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@uid", tablename:=ConstTableID, columnname:="uid"))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@uid", value:=uid)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aCurrTarget As New CurrentTarget
                    If InfuseDataObject(record:=aRecord, dataobject:=aCurrTarget) Then
                        aCollection.Add(Item:=aCurrTarget)
                    End If
                Next aRecord

                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBCurrTarget.AllByUID")
                Return aCollection
            End Try
        End Function
        ''' <summary>
        ''' return a collection of current Targets filtered by workspaceID
        ''' </summary>
        ''' <param name="workspaceID">the workspaceID id</param>
        ''' <returns>a Collection</returns>
        ''' <remarks></remarks>
        Public Shared Function AllByWorkspace(workspaceID As String) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aTable As iormDataStore

            Try

                aTable = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand("AllByWorkspace")

                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNWorkspace & "] = @wspace "
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@wspace", tablename:=ConstTableID, ColumnName:=ConstFNWorkspace))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@wspace", value:=workspaceID)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aCurrTarget As New CurrentTarget
                    If InfuseDataObject(record:=aRecord, dataobject:=aCurrTarget) Then
                        aCollection.Add(Item:=aCurrTarget)
                    End If
                Next aRecord

                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBCurrTarget.AllByWorkspace", arg1:=workspaceID)
                Return aCollection
            End Try

        End Function



        ''' <summary>
        ''' create the schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of CurrentTarget)(silent:=silent)

            ''*** legacy code
            'Dim aFieldDesc As New ormFieldDescription
            'Dim primaryColumnNames As New Collection
            'Dim aTable As New ObjectDefinition

            'With aTable
            '    .Create(ConstTableID)
            '    .Delete()

            '    aFieldDesc.Tablename = ConstTableID
            '    aFieldDesc.ID = ""
            '    aFieldDesc.Parameter = ""

            '    '*** UID
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "workspaceID"
            '    aFieldDesc.ID = "ws"
            '    aFieldDesc.ColumnName = ConstFNWorkspace
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    primaryColumnNames.Add(aFieldDesc.ColumnName)

            '    '**** UID
            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "uid"
            '    aFieldDesc.ID = "uid"
            '    aFieldDesc.ColumnName = "uid"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    primaryColumnNames.Add(aFieldDesc.ColumnName)


            '    '**** rev
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "revision of the target"
            '    aFieldDesc.ID = "t9"
            '    aFieldDesc.ColumnName = "rev"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '**** updc
            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "update count of target"
            '    aFieldDesc.ID = "t10"
            '    aFieldDesc.ColumnName = "updc"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '***** isactive
            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "is an active setting"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.ID = "t11"
            '    aFieldDesc.ColumnName = "isactive"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '***** message log tag
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "message log tag"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.ID = ""
            '    aFieldDesc.ColumnName = "msglogtag"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '***
            '    '*** TIMESTAMP
            '    '****
            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "last Update"
            '    aFieldDesc.ColumnName = ConstFNUpdatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "creation Date"
            '    aFieldDesc.ColumnName = ConstFNCreatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' Index
            '    Call .AddIndex("PrimaryKey", primaryColumnNames, isprimarykey:=True)

            '    ' persist
            '    .Persist()
            '    ' change the database
            '    .CreateObjectSchema()

            'End With

            ''
            'CreateSchema = True
            'Exit Function


        End Function

        ''' <summary>
        ''' Loads and infuses a Current Target dependent on the workspaceID
        ''' </summary>
        ''' <param name="uid">deliverable uid</param>
        ''' <param name="workspaceID">the workspaceID to look into - default workspaceID used</param>
        ''' <returns>true if successful</returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal uid As Long, Optional ByVal workspaceID As String = "") As Boolean
            Dim anID As String
            Dim aWS As Object

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' if no workspaceID -> Default workspaceID
            If IsMissing(workspaceID) Then
                anID = CurrentSession.CurrentWorkspaceID
            Else
                anID = Trim(CStr(workspaceID))
            End If
            Dim aWSObj As Workspace = Workspace.Retrieve(id:=anID)
            '*
            If aWSObj Is Nothing Then
                Call CoreMessageHandler(message:="Can't load workspaceID definition", subname:="clsOTDBCurrTarget.Inject", arg1:=anID)
                Inject = False
                Exit Function
            End If

            ' check now the stack
            For Each aWS In aWSObj.FCRelyingOn
                ' check if in workspaceID any data -> fall back to default (should be base)
                If Me.LoadUniqueBy(uid:=uid, workspaceID:=Trim(CStr(aWS))) Then
                    If Me.IsActive And Not Me.IsDeleted Then
                        Inject = True
                        Exit Function
                    End If
                End If
            Next aWS


            ' return nothing
            Inject = False

        End Function
        ''' <summary>
        ''' load a unique current Target by its primary keys
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadUniqueBy(ByVal uid As Long, ByVal workspaceID As String) As Boolean
            Dim pkarry() As Object = {workspaceID, uid}
            Return MyBase.Inject(pkarry)
        End Function

        ''' <summary>
        ''' create a current Target by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal uid As Long, Optional ByVal workspaceID As String = "", Optional ByVal domainID As String = "") As Boolean
            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID

            End If
            Dim pkarray() As Object = {workspaceID, uid}
            If MyBase.Create(pkarray, checkUnique:=True) Then
                ' set the primaryKey
                _uid = uid
                _updc = UPDC
                _domainID = domainID
                _isActive = True
                Return Me.IsCreated
            Else
                Return False
            End If

        End Function

    End Class
    '************************************************************************************
    '***** CLASS clsOTDBDeliverableTarget is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' target object for the deliverable class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=OnTrack.Deliverables.Target.ConstObjectID, modulename:=ConstModuleDeliverables, Version:=1, useCache:=True)> _
    Public Class Target
        Inherits ormDataObject
        Implements iotXChangeable
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of Target)

        Public Const ConstObjectID As String = "Target"
        '** Schema Table
        <ormSchemaTableAttribute(version:=2)> Public Const constTableID = "tblDeliverableTargets"
        '** Index
        <ormSchemaIndexAttribute(columnname1:=constFNUid)> Public Const constIndexUID = "uid"


        '** Keys
        <ormObjectEntry(referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
            defaultValue:="0", primaryKeyordinal:=1, useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            XID:="DT1", aliases:={"UID"})> Public Const constFNUid = Deliverable.constFNUid

        <ormObjectEntry(typeid:=otFieldDataType.Long, defaultValue:="0", primaryKeyordinal:=2, _
            description:="update count of the target date", title:="Update count", XID:="DT2", aliases:={"UPDC"})> Public Const constFNUpdc = "updc"

        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            Description:="workspaceID ID of the schedule")> Public Const ConstFNWorkspace = Schedule.ConstFNWorkspace

        <ormObjectEntry(typeid:=otFieldDataType.Date, _
            description:="current target date", title:="target date", XID:="DT6", aliases:={"T2"})> Public Const constFNTarget = "targetdate"

        <ormObjectEntry(typeid:=otFieldDataType.Date, _
            description:="previous target date", title:="previous target date", XID:="DT5", aliases:={"T1"})> Public Const constFNPrevTarget = "pvtd"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="target revision", Description:="revision of the target", _
           XID:="DT4", aliases:={"t9"}, Defaultvalue:="")> Public Const ConstFNRevision = "rev"

        <ormObjectEntry(typeid:=otFieldDataType.Timestamp, _
          description:="target change timestamp", title:="target change", XID:="DT7", aliases:={"A6"})> Public Const constFNTargetChanged = "tchg"

        <ormObjectEntry(typeid:=otFieldDataType.Bool, _
          title:="No Target", description:="no target by intention", XID:="DT2")> Const ConstFNNoTarget = "notarget"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, _
          title:="Type", description:="type of the target", XID:="DT3")> Const ConstFNType = "typeid"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, defaultValue:="", _
           title:="Responsible OrgUnit", description:=" organization unit responsible for the target", XID:="DT5")> Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, defaultValue:="", _
            title:="Responsible Person", description:="responsible person for the target", XID:="DT6")> Public Const constFNResp = "resp"

        <ormObjectEntry(typeid:=otFieldDataType.Memo, _
            title:="Comment", Description:="comment of the target", XID:="DT7", Defaultvalue:="")> Public Const ConstFNComment = "cmt"

        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** variables
        <ormEntryMapping(EntryName:=constFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=constFNUpdc)> Private _updc As Long

        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=constFNTarget)> Private _targetdate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNPrevTarget)> Private _prevTarget As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNTargetChanged)> Private _changedDate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String = ""
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""
        <ormEntryMapping(EntryName:=ConstFNNoTarget)> Private _notargetByItention As Boolean
        <ormEntryMapping(EntryName:=ConstFNType)> Private _typeid As String
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOU As String = ""
        <ormEntryMapping(EntryName:=constFNResp)> Private _resp As String = ""
        <ormEntryMapping(EntryName:=ConstFNComment)> Private _cmt As String = ""
        'dynamic
        Private s_msglog As New ObjectLog
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableID)

        End Sub

#Region "properties"
        ''' <summary>
        ''' gets the UID of the Deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UID() As Long
            Get
                UID = _uid
            End Get

        End Property
        ''' <summary>
        ''' gets  the update counter of the target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UPDC() As Long
            Get
                UPDC = _updc
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the Target Date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Target() As Date
            Get
                Target = _targetdate
            End Get
            Set(value As Date)
                If value <> _targetdate Then
                    _targetdate = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or set the previous target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrevTarget() As Date
            Get
                PrevTarget = _prevTarget
            End Get
            Set(value As Date)
                If value <> _prevTarget Then
                    _prevTarget = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the resp.
        ''' </summary>
        ''' <value>The resp.</value>
        Public Property Responsible() As String
            Get
                Return Me._resp
            End Get
            Set(value As String)
                If value <> _resp Then
                    _resp = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the resp OU.
        ''' </summary>
        ''' <value>The resp OU.</value>
        Public Property ResponsibleOU() As String
            Get
                Return Me._respOU
            End Get
            Set(value As String)
                If value <> _respOU Then
                    _respOU = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the resp OU.
        ''' </summary>
        ''' <value>The resp OU.</value>
        Public Property Comment() As String
            Get
                Return Me._cmt
            End Get
            Set(value As String)
                If value <> _cmt Then
                    _cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the timestamp of the  target date (changed on)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ChangedDate() As Date
            Get
                ChangedDate = _changedDate
            End Get
            Set(value As Date)
                _changedDate = value
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the revision string for the target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Revision() As String
            Get
                Revision = _rev
            End Get
            Set(value As String)
                If value <> _rev Then
                    _rev = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property workspaceID() As String
            Get
                workspaceID = _workspace
            End Get
            Set(value As String)
                If UCase(value) <> _workspace Then
                    _workspace = UCase(value)
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get
        End Property

        ReadOnly Property Msglog() As ObjectLog
            Get
                If s_msglog Is Nothing Then
                    s_msglog = New ObjectLog
                End If
                If Not s_msglog.IsCreated And Not s_msglog.IsLoaded Then
                    'If Not s_msglog.Inject(Me.msglogtag()) Then
                    s_msglog.Create(Me.Msglogtag())
                    'End If
                End If
                Msglog = s_msglog
            End Get
        End Property

#End Region

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & _uid & ConstDelimiter & _updc & ConstDelimiter
        End Function

        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Target)()
            '            ''' legacy code
            '            Dim PrimaryColumnNames As New Collection
            '            Dim UsedKeyColumnNames As New Collection
            '            Dim uidkeycolumnnames As New Collection
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.Tablename = constTableID

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()
            '                '*** workspaceID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workspaceID"
            '                aFieldDesc.ID = "dt10"
            '                aFieldDesc.Aliases = New String() {"ws"}
            '                aFieldDesc.ColumnName = ConstFNWorkspace
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                'PrimaryColumnNames.ADD "wspace"

            '                '**** UID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "uid"
            '                aFieldDesc.ID = "dt1"
            '                aFieldDesc.ColumnName = "uid"
            '                aFieldDesc.Aliases = New String() {"uid"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                uidkeycolumnnames.Add(aFieldDesc.ColumnName)
            '                '**** updc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of target"
            '                aFieldDesc.ID = "dt2"
            '                aFieldDesc.ColumnName = "updc"
            '                aFieldDesc.Aliases = New String() {"t10"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                '**** rev
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "revision of the target"
            '                aFieldDesc.ID = "dt4"
            '                aFieldDesc.ColumnName = "rev"
            '                aFieldDesc.Aliases = New String() {"t9"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** previous target
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "previous target date (statistic)"
            '                aFieldDesc.ID = "dt5"
            '                aFieldDesc.ColumnName = "pvtd"
            '                aFieldDesc.Aliases = New String() {"t1"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** target date
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "target date"
            '                aFieldDesc.ID = "dt6"
            '                aFieldDesc.ColumnName = constFNTarget
            '                aFieldDesc.Aliases = New String() {"t2"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** tchg
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "last change to target date"
            '                aFieldDesc.ID = "dt7"
            '                aFieldDesc.ColumnName = "tchg"
            '                aFieldDesc.Aliases = New String() {"a6"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' msglogtag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "message log tag"
            '                aFieldDesc.ColumnName = "msglogtag"
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Aliases = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("uid", uidkeycolumnnames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .CreateObjectSchema()
            '            End With

            '            '
            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="clsOTDBDeliverableTarget.createSchema", tablename:=constTableID)
            '            CreateSchema = False
        End Function


        ''' <summary>
        ''' returns all Targets by Deliverable UID
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns>a Collection</returns>
        ''' <remarks></remarks>
        Public Shared Function AllByUid(uid As Long) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            Dim pkarray() As Object = {uid}
            Try
                aStore = GetTableStore(constTableID)
                aRecordCollection = aStore.GetRecordsByIndex(constIndexUID, pkarray, True)

                If aRecordCollection.Count > 0 Then
                    For Each aRecord As ormRecord In aRecordCollection
                        Dim aNewcurSchedule As New CurrentSchedule
                        If InfuseDataObject(record:=aRecord, dataobject:=aNewcurSchedule) Then
                            aCollection.Add(Item:=aNewcurSchedule)
                        End If
                    Next aRecord
                End If

                Return aCollection

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsotdbDeliverableTarget.AllByUID", exception:=ex)
                Return aCollection
            End Try

        End Function

        ''' <summary>
        ''' create the persistent target by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal uid As Long, ByVal updc As Long) As Boolean
            Dim pkarray() As Object = {uid, updc}
            If MyBase.Create(pkarray, checkUnique:=True) Then
                ' set the primaryKey
                _uid = uid
                _updc = updc
                Return Me.IsCreated
            End If
        End Function

        ' •—————————————————————————————————————————————————————————•
        ' | ''' <summary>                                           |
        ' | ''' update properties from record                       |
        ' | ''' </summary>                                          |
        ' | ''' <returns></returns>                                 |
        ' | ''' <remarks></remarks>                                 |
        ' | Private Function UpdateRecord() As Boolean              |
        ' |     '* init                                             |
        ' |     If Not Me.IsInitialized Then                        |
        ' |         If Not Me.Initialize() Then                     |
        ' |             UpdateRecord = False                        |
        ' |             Exit Function                               |
        ' |         End If                                          |
        ' |     End If                                              |
        ' |                                                         |
        ' |     Call Me.Record.SetValue("uid", _uid)                |
        ' |     Call Me.Record.SetValue("updc", _updc)              |
        ' |     Call Me.Record.SetValue("rev", _rev)                |
        ' |     Call Me.Record.SetValue(constFNTarget, _targetdate) |
        ' |     Call Me.Record.SetValue("wspace", _workspace)       |
        ' |     Call Me.Record.SetValue("tchg", _changedDate)       |
        ' |     Call Me.Record.SetValue("pvtd", _prevTarget)        |
        ' |     Call Me.Record.SetValue("msglogtag", _msglogtag)    |
        ' |                                                         |
        ' |     UpdateRecord = True                                 |
        ' | End Function                                            |
        '  •—————————————————————————————————————————————————————————• */

        ''' <summary>
        ''' load and infuse the object by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(uid As Long, updc As Long) As Boolean
            Dim pkarray() As Object = {uid, updc}
            Return MyBase.Inject(pkarray)
        End Function
        ''' <summary>
        ''' publish a new Target to the database from a Date
        ''' </summary>
        ''' <param name="NewTargetDate"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="UID"></param>
        ''' <param name="revision"></param>
        ''' <param name="NewDeliverableTarget"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PublishNewTarget(ByVal NewTargetDate As Date, _
                                            Optional ByVal workspaceID As String = "", _
                                            Optional ByVal UID As Long = 0, _
                                            Optional ByVal revision As String = "", _
                                            Optional ByRef NewDeliverableTarget As Target = Nothing) As Boolean
            Dim aNewTarget As New Target
            Dim anOldTarget As New Target
            Dim aCurrTarget As New CurrentTarget
            Dim aTrack As New Track
            Dim anUPDC As Long
            Dim anUID As Long

            '** workspaceID
            If IsMissing(workspaceID) Or workspaceID = "" Then
                If (_IsLoaded Or Me.IsCreated) AndAlso Me.workspaceID <> "" Then
                    workspaceID = Me.workspaceID
                Else
                    workspaceID = CurrentSession.CurrentWorkspaceID
                End If

            Else
                workspaceID = CStr(workspaceID)
            End If


            '** if UID is not provided than do use this TargetObject
            If UID = 0 Then
                If Not _IsLoaded And Not Me.IsCreated Then
                    PublishNewTarget = False
                    Exit Function
                End If

                anOldTarget = Me
                anUID = anOldTarget.UID
                anUPDC = Me.UPDC
                If Not aCurrTarget.Inject(uid:=anUID, workspaceID:=workspaceID) Then
                    Call aCurrTarget.Create(uid:=anUID, workspaceID:=workspaceID)
                End If
                '*** only if loaded and not created get an new updc key and clone !
                If anOldTarget.IsLoaded Then
                    anUPDC = 0   ' increase by clone
                    ' clone
                    aNewTarget = anOldTarget.Clone(uid:=anUID, updc:=anUPDC)
                    aNewTarget.workspaceID = workspaceID
                ElseIf anOldTarget.IsCreated Then
                    aNewTarget = anOldTarget
                    aNewTarget.workspaceID = workspaceID
                End If
                '** if UID is provided than load oldTargetObject or create Target
            Else
                '** load the current UID of the current Target object
                If aCurrTarget.Inject(anUID, workspaceID) Then
                    anUPDC = aCurrTarget.UPDC
                Else
                    Call aCurrTarget.Create(uid:=anUID, workspaceID:=workspaceID)
                    anUPDC = 1
                End If

                ' no Target exists ?!
                If anOldTarget.Inject(anUID, anUPDC) Then
                    anUPDC = 0   ' create by clone
                    ' clone
                    aNewTarget = anOldTarget.Clone(uid:=anUID, updc:=anUPDC)
                Else
                    ' create new date -> newTarget Object not necessary
                    Call aNewTarget.Create(anUID, anUPDC)
                End If
            End If

            '*** set the standards
            ' Increase the Revision (if we have something)
            If (revision Is Nothing OrElse revision = "") Then
                If Not anOldTarget Is Nothing AndAlso (anOldTarget.IsLoaded Or anOldTarget.IsCreated) Then
                    aNewTarget.Revision = anOldTarget.Revision
                    Call aNewTarget.IncreaseRevison(majorFlag:=False, minorFlag:=True)
                Else
                    aNewTarget.Revision = ConstFirstPlanRevision
                End If

            Else
                aNewTarget.Revision = CStr(revision)
            End If

            '** special save the previous target if not a previous is through updating
            aNewTarget.PrevTarget = anOldTarget.Target
            aNewTarget.Target = NewTargetDate
            aNewTarget.ChangedDate = Date.Now()
            PublishNewTarget = aNewTarget.Persist

            ' set the current Target
            ' save the object above
            'If Not aCurrTarget.Inject(UID:=anUID, workspaceID:=workspaceID) Then
            '    Call aCurrTarget.create(UID:=anUID, workspaceID:=workspaceID)
            'End If
            aCurrTarget.UPDC = anUPDC
            aCurrTarget.Revision = aNewTarget.Revision
            PublishNewTarget = aCurrTarget.Persist

            '***
            '***
            Call aTrack.UpdateFromTarget(Me, workspaceID:=workspaceID, persist:=True, checkGAP:=True)

            ' TODO: create track ?!
            '
            NewDeliverableTarget = aNewTarget
        End Function

        '******** Increase the Revision in Form VXX.YY
        '********
        ''' <summary>
        ''' Increase the Revision in Form VXX.YY
        ''' </summary>
        ''' <param name="majorFlag">is a major version - increase xx </param>
        ''' <param name="minorFlag">is a minor version - increase yy</param>
        ''' <returns>new revision</returns>
        ''' <remarks></remarks>
        Function IncreaseRevison(majorFlag As Boolean, minorFlag As Boolean) As String
            Dim i, j, k As Integer
            Dim minor As Integer
            Dim major As Integer
            Dim aVAlue As Object

            If Not IsLoaded And Not IsCreated Then
                IncreaseRevison = ""
                Exit Function
            End If


            If Me.Revision <> "" And UCase(Me.Revision) Like "V*.*" Then
                aVAlue = Mid(Me.Revision, InStr(UCase(Me.Revision), "V") + 1, _
                             InStr(Me.Revision, ".") - InStr(UCase(Me.Revision), "V"))
                If IsNumeric(aVAlue) Then
                    major = CInt(aVAlue)

                    aVAlue = (Mid(Me.Revision, InStr(Me.Revision, ".") + 1))
                    If IsNumeric(aVAlue) Then
                        minor = CInt(aVAlue)
                    Else
                        minor = 0
                    End If

                    If majorFlag Then
                        major = major + 1
                        minor = 0
                    ElseIf minorFlag Then
                        minor = minor + 1
                    End If

                    Me.Revision = "V" & major & "." & minor
                End If
            ElseIf Me.Revision <> "" And UCase(Me.Revision) Like "V*" Then
                aVAlue = Mid(Me.Revision, InStr(UCase(Me.Revision), "V") + 1, _
                             Len(Me.Revision) - InStr(UCase(Me.Revision), "V"))
                If IsNumeric(aVAlue) Then
                    major = CInt(aVAlue)
                    minor = 0
                    If majorFlag Then
                        major = major + 1
                        minor = 0
                    ElseIf minorFlag Then
                        minor = minor + 1
                    End If

                    Me.Revision = "V" & major & "." & minor
                End If

            ElseIf Me.Revision = "" Then
                Me.Revision = ConstFirstPlanRevision
            Else
                WriteLine("me.revision " & Me.Revision & " not increasable since not in VXX.YY")
                System.Diagnostics.Debug.Assert(False)
            End If
            ' exit
            IncreaseRevison = Me.Revision

        End Function
        ''' <summary>
        ''' Run the XPrecheck on the Target with the envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXPreCheck(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXPreCheck

        End Function
        ''' <summary>
        ''' run the XChange on the Deliverable Target for an Envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXChange

        End Function
        '***** runXChange runs the eXChange for the class
        '*****
        Public Function runXChangeOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
        ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aChangeMember As New clsOTDBXChangeMember

            Dim anUID As Long
            Dim anUPDC As Long
            Dim aTarget As New Target
            Dim aCurrTarget As New CurrentTarget
            Dim aDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim anObjectDef As New clsOTDBXChangeMember
            Dim aNewTarget As New Target
            Dim aWorkspace As String
            Dim setCurrTarget As Boolean
            Dim aRevision As String
            Dim aVAlue As Object

            Dim aTimestamp As Date

            If CHANGECONFIG.ProcessedDate <> ConstNullDate Then
                aTimestamp = CHANGECONFIG.ProcessedDate
            Else
                aTimestamp = Now
            End If
            '*** ObjectDefinition
            anObjectDef = CHANGECONFIG.ObjectByName(constTableID)

            ' set msglog
            If MSGLOG Is Nothing Then
                If s_msglog Is Nothing Then
                    s_msglog = New ObjectLog
                End If
                MSGLOG = s_msglog
                MSGLOG.Create(Me.Msglogtag)
            End If

            '*** set the workspaceID
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="WS", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                aWorkspace = CurrentSession.CurrentWorkspaceID
            Else
                aWorkspace = CStr(aVAlue)
            End If

            '** check on the min. required primary key uid
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT1", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("DT1")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXChangeOLD = False
                Exit Function
            Else
                anUID = CLng(aVAlue)
            End If


            ' optional key updc
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT2", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                'Call msglog.addMsg("201", Nothing, Nothing, "DT2", "DT2", ourTableName, ChangeConfig.ConfigName)
                anUPDC = -1
            ElseIf Not IsNumeric(aVAlue) Then
                anUPDC = -1
            Else
                anUPDC = CLng(aVAlue)
                setCurrTarget = False
            End If

            ' optional revision supplied ?!
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT4", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                aRevision = ""
            Else
                aRevision = CStr(aVAlue)
            End If

            '*** try to load the current Target
            If anUPDC = -1 Then
                ' check on set the current target (move to duplicate)
                ' if the target date is touched
                aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT6", mapping:=MAPPING)
                aChangeMember = CHANGECONFIG.AttributeByID("DT6", objectname:=Me.TableID)
                If Not IsNull(aVAlue) AndAlso Not aChangeMember Is Nothing AndAlso _
                (aChangeMember.XChangeCmd = otXChangeCommandType.Update OrElse _
                aChangeMember.XChangeCmd = otXChangeCommandType.UpdateCreate OrElse _
                aChangeMember.XChangeCmd = otXChangeCommandType.Duplicate) Then
                    '*** indeed we have something to update
                    setCurrTarget = True
                    'anObjectDef.xChangeCmd = otDuplicate -> problem with DefaultRunX
                End If

                ' get the updc
                If aCurrTarget.Inject(uid:=anUID, workspaceID:=aWorkspace) Then
                    anUPDC = aCurrTarget.UPDC
                    setCurrTarget = True
                    'aCurrTarget.initialize
                Else
                    'create necessary ?!
                    If anObjectDef.XChangeCmd <> otXChangeCommandType.UpdateCreate Then
                        Call MSGLOG.AddMsg("203", CHANGECONFIG.Configname, Nothing, Nothing, CHANGECONFIG.Configname, anUID & ", <none>")
                        runXChangeOLD = False
                        Exit Function
                    End If
                    ' create an new UPDC
                    anUPDC = 1
                End If
                ' add to the Mapping the UPDC / DT2 (
                ' otherwise DefaultXChange hasnot all keys
                aChangeMember = CHANGECONFIG.AttributeByID("DT2")
                If aChangeMember Is Nothing Then
                    Call CHANGECONFIG.AddAttributeByID(id:="DT2", objectname:=constTableID, _
                                                       isXChanged:=False, xcmd:=otXChangeCommandType.Read)    ' ordinal will be created
                    aChangeMember = CHANGECONFIG.AttributeByID("DT2")
                Else
                    If MAPPING.ContainsKey(key:=aChangeMember.ordinal.Value) Then
                        MAPPING.Remove(key:=aChangeMember.ordinal.Value)
                    End If
                End If

                Call MAPPING.Add(key:=aChangeMember.ordinal.Value, value:=anUPDC)
            End If

            '** load the target
            If Not aTarget.Inject(uid:=anUID, updc:=anUPDC) Then
                If anObjectDef.XChangeCmd <> otXChangeCommandType.UpdateCreate Then
                    Call MSGLOG.AddMsg("203", Nothing, Nothing, "DT2", CHANGECONFIG.Configname, anUID & "," & anUPDC)
                    runXChangeOLD = False
                    Exit Function
                Else
                    ' create with the given UPDC !
                    Call aTarget.Create(uid:=anUID, updc:=anUPDC)
                    Call aTarget.PublishNewTarget(NewTargetDate:=ConstNullDate, workspaceID:=aWorkspace)
                    setCurrTarget = True ' is now set
                End If
            End If

            '*** from here we can also go over the Default Update
            '*** routine if not setCurrTarget to be adjusted -> nothing special to handle
            If Not setCurrTarget Then
                If (anObjectDef.XChangeCmd = otXChangeCommandType.Read Or anObjectDef.XChangeCmd = otXChangeCommandType.Update) Then
                    runXChangeOLD = CHANGECONFIG.runDefaultXChange4Object(anObjectDef, MAPPING, MSGLOG)
                    Exit Function
                ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.UpdateCreate Then
                    '*** handle new entries on other objects such as Track ?!
                    System.Diagnostics.Debug.Assert(False)
                ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Delete Then
                    '*** handle new entries on other objects such as Track ?!
                    System.Diagnostics.Debug.Assert(False)
                ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Duplicate Then
                    '*** handle new entries on other objects such as Track ?!
                    System.Diagnostics.Debug.Assert(False)
                End If
            Else
                '*** setting the current
                ' just read -> standard Default
                If anObjectDef.XChangeCmd = otXChangeCommandType.Read Then
                    runXChangeOLD = CHANGECONFIG.runDefaultXChange4Object(anObjectDef, MAPPING, MSGLOG)
                    Exit Function
                    ' if any change -> new entry !
                ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Update Or _
                anObjectDef.XChangeCmd = otXChangeCommandType.UpdateCreate Or _
                anObjectDef.XChangeCmd = otXChangeCommandType.Duplicate Then

                    ' the target
                    aChangeMember = CHANGECONFIG.AttributeByID("DT6")
                    If (Not aChangeMember Is Nothing) AndAlso (aChangeMember.IsXChanged And _
                    (aChangeMember.XChangeCmd = otXChangeCommandType.Update Or aChangeMember.XChangeCmd = otXChangeCommandType.UpdateCreate)) Then

                        '*** here we go to the Object Routine
                        '***
                        aNewTarget = New Target
                        ' convert to DB
                        aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT6", objectname:=constTableID, mapping:=MAPPING)
                        If Not aVAlue Is Nothing Then
                            Call aChangeMember.convertValue2DB(aVAlue, aVAlue, existingValue:=False)
                            If aVAlue <> aTarget.Target Then
                                If aTarget.PublishNewTarget(CDate(aVAlue), _
                                                            workspaceID:=aWorkspace, _
                                                            revision:=aRevision, _
                                                            NewDeliverableTarget:=aNewTarget) Then

                                    ' add to the Mapping the UPDC / DT2 (
                                    ' otherwise DefaultXChange hasnot all keys
                                    aChangeMember = CHANGECONFIG.AttributeByID("DT2")
                                    If Not aChangeMember Is Nothing Then
                                        Call CHANGECONFIG.AddAttributeByID(id:="DT2", objectname:=constTableID, _
                                                                           isXChanged:=False, xcmd:=otXChangeCommandType.Read)    ' ordinal will be created
                                        aChangeMember = CHANGECONFIG.AttributeByID("DT2")
                                    End If
                                    If MAPPING.ContainsKey(key:=aChangeMember.ordinal.Value) Then
                                        Call MAPPING.Remove(key:=aChangeMember.ordinal.Value)
                                    End If

                                    Call MAPPING.Add(key:=aChangeMember.ordinal.Value, value:=anUPDC)

                                    ' save new target -> must have been done in setNewTarget

                                Else
                                    WriteLine("Houston ?!")
                                End If
                            End If
                        End If
                        ' rest is up to standard
                        runXChangeOLD = CHANGECONFIG.runDefaultXChange4Object(anObjectDef, MAPPING, MSGLOG)
                    End If    'otRead on member
                    ' delete
                ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Delete Then
                    '*** handle new entries on other objects such as Track ?!
                    System.Diagnostics.Debug.Assert(False)
                End If

            End If
            runXChangeOLD = True
        End Function

        '***** runXPreCheck runs the precheck
        '*****
        Public Function runXPreCheckOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
                                        ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
                                        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aCMupdc As clsOTDBXChangeMember
            Dim anObject As New clsOTDBXChangeMember
            Dim aVAlue As Object

            ' set msglog
            If MSGLOG Is Nothing Then
                MSGLOG = Me.Msglog
                MSGLOG.Create(Me.Msglogtag)
            End If

            '** check on the min. required primary key uid
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT1", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("DT1")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "DT1", "DT1", constTableID, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXPreCheckOLD = False
                Exit Function

            End If

            ' optional key updc
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="DT2", mapping:=MAPPING)
            '*
            If IsNull(aVAlue) Then
                'Call msglog.addMsg("201", "uid", -1, "dt2", ourTableName, ChangeConfig.ConfigName)
                'runXPreCheck = False
                'Exit Function
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "DT2", "DT2", constTableID, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXPreCheckOLD = False
                Exit Function
            End If

            ' generell tests
            anObject = CHANGECONFIG.ObjectByName(Me.TableID)
            runXPreCheckOLD = CHANGECONFIG.runDefaultXPreCheck(anObject:=anObject, _
                                                            aMapping:=MAPPING, MSGLOG:=MSGLOG)


        End Function

        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(pkarray() As Object) As Target Implements iotCloneable(Of Target).Clone
            If Not MyBase.Feed() Then
                Return Nothing
            End If

            If pkarray.Length = 0 OrElse pkarray(0) Is Nothing OrElse pkarray(0) = 0 Then
                Call CoreMessageHandler(message:="Deliverable UID cannot be 0 or Nothing or primary key array not set for clone - must be set", arg1:=pkarray, _
                                        subname:="clsOTDBDeliverableTarget.Clone", messagetype:=otCoreMessageType.InternalError, tablename:=TableID)
                Return Nothing
            End If
            If pkarray.Length = 1 OrElse pkarray(1) Is Nothing OrElse pkarray(0) = 0 Then
                If Not Me.TableStore.CreateUniquePkValue(pkarray) Then
                    Call CoreMessageHandler(message:="failed to create an unique primary key value", arg1:=pkarray, _
                                            subname:="clsOTDBDeliverableTarget.Clone", messagetype:=otCoreMessageType.InternalError, tablename:=TableID)
                    Return Nothing
                End If
            End If
            '**
            Return MyBase.Clone(Of Target)(pkarray)
        End Function

        ''' <summary>
        ''' clone the loaded or created dataobject object
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal uid As Long, ByVal updc As Long) As Target
            Dim pkarray() As Object = {uid, updc}
            Return Me.Clone(pkarray)
        End Function
    End Class

    '************************************************************************************
    '***** CLASS Track is the object for a OTDBRecord (which is the data store)
    '*****
    '*****
    ''' <summary>
    ''' deliverable track class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=Track.ConstObjectID, modulename:=ConstModuleDeliverables, Version:=1, useCache:=True)> Public Class Track
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable
        Implements iotCloneable(Of Track)


        Public Const ConstObjectID = "Track"
        '** Table
        <ormSchemaTable(version:=2, addDomainBehavior:=True, addsparefields:=True)> Public Const ConstTableID = "tblDeliverableTracks"
        '** Index
        <ormSchemaIndex(columnname1:=ConstFNWorkspace, columnname2:=constFNDeliverableUid, columnname3:=constFNScheduleUid, columnname4:=constFNScheduleUpdc, columnname5:=constFNTargetUpdc)> _
        Public Const constIndWSpace = "indWorkspace"

        '** primary keys
        <ormObjectEntry(referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, primarykeyordinal:=1, _
            XID:="DTR2", aliases:={"UID"})> Public Const constFNDeliverableUid = Deliverable.constFNUid

        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUid, primarykeyordinal:=2, _
             XID:="DTR3", aliases:={"SC2"})> Public Const constFNScheduleUid = "suid"
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc, primarykeyordinal:=3, _
           XID:="DTR4", aliases:={"SC2"})> Public Const constFNScheduleUpdc = "supdc"
        '**
        <ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            entrynames:={constFNScheduleUid, constFNScheduleUpdc}, _
            foreignkeyreferences:={Schedule.ConstObjectID & "." & Schedule.ConstFNUid, _
            Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc})> _
        Public Const constFKSchedule = "fkschedule"

        <ormObjectEntry(referenceobjectentry:=Target.ConstObjectID & "." & Target.constFNUpdc, primarykeyordinal:=4, _
           XID:="DTR5", aliases:={"DT2"})> Public Const constFNTargetUpdc = "tupdc"

        <ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            entrynames:={constFNDeliverableUid, constFNTargetUpdc}, _
            foreignkeyreferences:={Target.ConstObjectID & "." & Target.constFNUid, _
            Target.ConstObjectID & "." & Target.constFNUpdc})> _
        Public Const constFKTarget = "fkTarget"

        '** fields
        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
                        foreignkeyproperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                            ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"}, _
                        XID:="DTR1", aliases:={"WS"})> Public Const ConstFNWorkspace = Workspace.ConstFNID

        <ormObjectEntry(referenceobjectentry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
             foreignkeyProperties:={ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")"}, _
             XID:="DTR6", aliases:={"SC14"}, Defaultvalue:="none")> Public Const ConstFNTypeid = Schedule.ConstFNTypeid

        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNPlanRev, _
          XID:="DTR7", aliases:={"SC5"}, Defaultvalue:="0")> Public Const ConstFNScheduleRevision = Schedule.ConstFNPlanRev
        <ormObjectEntry(referenceobjectentry:=Target.ConstObjectID & "." & Target.ConstFNRevision, title:="target revision", Description:="revision of the target", _
          XID:="DTR8", aliases:={"DT4"}, Defaultvalue:="0")> Public Const ConstFNTargetRevision = "trev"
        <ormObjectEntry(referenceobjectentry:=ScheduleMilestone.ConstObjectID & "." & ScheduleMilestone.ConstFNID, _
            title:="milestone ID delivered", Description:="schedule definition milestone ID for fc delivered", _
            XID:="DTR9", Defaultvalue:="")> Public Const ConstFNMSIDDelivered = "msfinid"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="current forecast", Description:="forecast date for deliverable delivered", _
            XID:="DTR10", isnullable:=True)> Public Const ConstFNForecast = "fcdate"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="current target", Description:="target date for deliverable", _
            XID:="DTR11", isnullable:=True)> Public Const ConstFNTarget = "targetdate"

        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNlcstatus, _
            XID:="DTR12", aliases:={"SC7"}, Defaultvalue:="")> Public Const ConstFNLCStatus = Schedule.ConstFNlcstatus
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNpstatus, _
            XID:="DTR13", aliases:={"SC8"}, Defaultvalue:="")> Public Const ConstFNPStatus = Schedule.ConstFNpstatus

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="Synchro status", Description:="schedule synchro status", _
            XID:="DTR14", aliases:={}, Defaultvalue:="")> Public Const ConstFNSyncStatus = "sync"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Synchro check date", Description:="date of last synchro check status", _
            XID:="DTR15", Defaultvalue:="")> Public Const ConstFNSyncDate = "syncchkon"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Going Alive Date", Description:="date of schedule going alive", _
           XID:="DTR16", Defaultvalue:="")> Public Const ConstFNGoingAliveDate = "goal"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, title:="Delivered", Description:="True if deliverable is delivered", _
          XID:="DTR17", Defaultvalue:="")> Public Const constFNIsDelivered = "isfinished"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
                         title:="Blocking Item Reference", description:="Blocking Item Reference id for the deliverable", XID:="DTR18", aliases:={"DLV17"})> _
        Public Const constFNBlockingItemReference = Deliverable.constFNBlockingItemReference
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Delivery Date", Description:="date for deliverable to be delivered / finished", _
          XID:="DTR19", Defaultvalue:="")> Public Const constFNDelivery = "finish"

        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="Forecast Gap", Description:="gap in working days between forecast and target", _
         XID:="DTR20")> Public Const constFNFCGap = "fcgap"
        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="BaseLine Gap", Description:="gap in working days between forecast and target", _
         XID:="DTR21")> Public Const constFNBLGap = "blgap"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Schedule Change Date", Description:="forecast last changed on", _
          XID:="DTR23")> Public Const constFNFcChanged = "fcchanged"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Baseline Delivery Date", Description:="delivery date from the baseline", _
          XID:="DTR24")> Public Const constFNBaseDelivery = "basefinish"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, title:="Schedule Frozen", Description:="True if schedule is frozen / a baseline exists", _
         XID:="DTR25", aliases:={"SC6"})> Public Const constFNIsFrozen = Schedule.ConstFNisfrozen
        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="Schedule UpdateCount", description:="update count of the schedule", _
            XID:="DTR26", aliases:={"SC17"})> Public Const constFNBaselineUPDC = Schedule.ConstFNBlUpdc
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="Baseline Reference Date", Description:="reference date for baseline", _
         XID:="DTR27", Defaultvalue:="")> Public Const ConstFNBLFrom = Schedule.ConstFNBlDate
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
            title:="ActivityTag", description:="activity tag for the deliverable", XID:="DTR31")> _
        Public Const constFNActiveTag = "acttag"

        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** MAPPING
        <ormEntryMapping(EntryName:=constFNDeliverableUid)> Private _deliverableUID As Long
        <ormEntryMapping(EntryName:=constFNTargetUpdc)> Private _targetUPDC As Long
        <ormEntryMapping(EntryName:=constFNScheduleUid)> Private _scheduleUID As Long
        <ormEntryMapping(EntryName:=constFNScheduleUpdc)> Private _scheduleUPDC As Long

        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspaceID As String = ""
        <ormEntryMapping(EntryName:=ConstFNMSIDDelivered)> Private _MSIDFinish As String = ""
        <ormEntryMapping(EntryName:=ConstFNForecast)> Private s_currFC As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNTarget)> Private s_currTarget As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private s_blockingitemID As String = ""
        <ormEntryMapping(EntryName:=ConstFNLCStatus)> Private s_FCLCStatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNTypeid)> Private s_scheduletype As String = ""
        <ormEntryMapping(EntryName:=ConstFNScheduleRevision)> Private s_ScheduleRevision As String = ""
        <ormEntryMapping(EntryName:=ConstFNTargetRevision)> Private s_TargetRevision As String = ""
        <ormEntryMapping(EntryName:=ConstFNGoingAliveDate)> Private s_GoingAliveDate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNBaseDelivery)> Private s_BaseLineFinishDate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNBLFrom)> Private s_BaseLineFinishDateFrom As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNFcChanged)> Private s_FClastchangeDate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNIsFrozen)> Private s_isFrozen As Boolean
        <ormEntryMapping(EntryName:=constFNDelivery)> Private s_finishedOn As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNIsDelivered)> Private s_isFinished As Boolean
        <ormEntryMapping(EntryName:=constFNBaselineUPDC)> Private s_BaselineUPDC As Long
        <ormEntryMapping(EntryName:=ConstFNSyncStatus)> Private s_SyncStatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNPStatus)> Private s_pstatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNSyncDate)> Private s_syncFrom As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNFCGap)> Private s_FCgapToTarget As Long
        <ormEntryMapping(EntryName:=constFNBLGap)> Private s_BaselineGapToTarget As Long

        <ormEntryMapping(EntryName:=constFNActiveTag)> Private s_activetag As String = ""
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private s_msglogtag As String = ""


        '********* dynamic
        Private s_schedule As New Schedule
        Private s_dlvTarget As New Target
        Private s_deliverable As New Deliverable

#Region "Properties"

        ReadOnly Property DeliverableUID() As Long
            Get
                DeliverableUID = _deliverableUID
            End Get

        End Property

        ReadOnly Property TargetUPDC() As Long
            Get
                TargetUPDC = _targetUPDC
            End Get
        End Property
        ReadOnly Property ScheduleUID() As Long
            Get
                ScheduleUID = _scheduleUID
            End Get

        End Property
        ReadOnly Property ScheduleUPDC() As Long
            Get
                ScheduleUPDC = _scheduleUPDC
            End Get

        End Property
        Public Property workspaceID() As String
            Get
                workspaceID = _workspaceID
            End Get
            Set(value As String)
                If UCase(value) <> _workspaceID Then
                    _workspaceID = UCase(value)
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property MSIDFinish() As String
            Get
                MSIDFinish = _MSIDFinish
            End Get
            Set(value As String)
                If value.ToLower <> _MSIDFinish Then
                    _MSIDFinish = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Scheduletype() As String
            Get
                Scheduletype = s_scheduletype
            End Get
            Set(value As String)
                If value.ToLower <> _workspaceID Then
                    s_scheduletype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property CurrentForecast As Date
            Get
                CurrentForecast = s_currFC
            End Get
            Set(value As Date)
                If value <> s_currFC Then
                    s_currFC = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property FinishedOn() As Date
            Get
                FinishedOn = s_finishedOn
            End Get
            Set(value As Date)
                If value <> s_finishedOn Then
                    s_finishedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property CurrentTarget As Date
            Get
                CurrentTarget = s_currTarget
            End Get
            Set(value As Date)
                If value <> s_currTarget Then
                    s_currTarget = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property GAPToTarget() As Long
            Get
                GAPToTarget = s_FCgapToTarget
            End Get
            Set(value As Long)
                If value <> s_FCgapToTarget Then
                    s_FCgapToTarget = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property BaselineGAPToTarget() As Long
            Get
                BaselineGAPToTarget = s_BaselineGapToTarget
            End Get
            Set(value As Long)
                If value <> s_BaselineGapToTarget Then
                    s_BaselineGapToTarget = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ForecastChangedOn() As Date
            Get
                ForecastChangedOn = s_FClastchangeDate
            End Get
            Set(value As Date)
                If value <> s_FClastchangeDate Then
                    s_FClastchangeDate = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property FCLCStatus() As String
            Get
                FCLCStatus = s_FCLCStatus
            End Get
            Set(value As String)
                If value.ToLower <> s_FCLCStatus Then
                    s_FCLCStatus = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ProcessStatus() As String
            Get
                ProcessStatus = s_pstatus
            End Get
            Set(value As String)
                If value.ToLower <> value Then
                    s_pstatus = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ScheduleRevision() As String
            Get
                ScheduleRevision = s_ScheduleRevision
            End Get
            Set(value As String)
                If StrComp(value, s_ScheduleRevision) <> 0 Then
                    s_ScheduleRevision = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property TargetRevision() As String
            Get
                TargetRevision = s_ScheduleRevision
            End Get
            Set(value As String)
                If StrComp(value, s_TargetRevision) <> 0 Then
                    s_TargetRevision = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property BlockingItemID() As String
            Get
                BlockingItemID = s_blockingitemID
            End Get
            Set(value As String)
                If s_blockingitemID <> value Then
                    s_blockingitemID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsFrozen() As Boolean
            Get
                IsFrozen = s_isFrozen
            End Get
            Set(value As Boolean)
                If value <> s_isFrozen Then
                    s_isFrozen = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsFinished() As Boolean
            Get
                IsFinished = s_isFinished
            End Get
            Set(value As Boolean)
                If value <> s_isFinished Then
                    s_isFinished = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property BaseLineUPDC() As Long
            Get
                BaseLineUPDC = s_BaselineUPDC
            End Get
            Set(value As Long)
                If value <> s_BaselineUPDC Then
                    s_BaselineUPDC = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property BaseLineFinishDate() As Date
            Get
                BaseLineFinishDate = s_BaseLineFinishDate
            End Get
            Set(value As Date)
                If value <> s_BaseLineFinishDate Then
                    s_BaseLineFinishDate = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property BaseLineFinishDateFrom() As Date
            Get
                BaseLineFinishDateFrom = s_BaseLineFinishDateFrom
            End Get
            Set(value As Date)
                If value <> s_BaseLineFinishDateFrom Then
                    s_BaseLineFinishDateFrom = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property GoingAliveDate() As Date
            Get
                GoingAliveDate = s_GoingAliveDate
            End Get
            Set(value As Date)
                If value <> GoingAliveDate Then
                    s_GoingAliveDate = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property SyncStatus() As String
            Get
                SyncStatus = s_SyncStatus
            End Get
            Set(value As String)
                If value.ToLower <> SyncStatus.ToLower Then
                    s_SyncStatus = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property SyncFrom() As Date
            Get
                SyncFrom = s_syncFrom
            End Get
            Set(value As Date)
                If value <> s_syncFrom Then
                    s_syncFrom = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        '****** getUniqueTag
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _
            _deliverableUID & ConstDelimiter
        End Function
        ReadOnly Property msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = getUniqueTag()
                End If
                msglogtag = s_msglogtag
            End Get
        End Property
        ReadOnly Property activetag() As String
            Get
                If s_activetag = "" Then
                    s_activetag = getUniqueTag()
                End If
                activetag = s_activetag
            End Get
        End Property

#End Region

        '** initialize
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub

        ''' <summary>
        ''' Batch processing for updating all Tracks
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <remarks></remarks>
        Public Shared Function UpdateAllTracks(Optional workspaceID As String = "", _
                                               Optional workerthread As System.ComponentModel.BackgroundWorker = Nothing) As Boolean

            ' Connection
            If Not CurrentSession.RequireAccessRight(accessRequest:=otAccessRight.ReadUpdateData) Then
                CoreMessageHandler(showmsgbox:=True, message:="Rights not sufficient to exectue the requested operation", _
                                   messagetype:=otCoreMessageType.ApplicationError, subname:="Track.UpdateAllTrack")
                Return False
            End If

            '** workspaceID
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            End If
            Dim aWorkspace As Workspace = Workspace.Retrieve(id:=workspaceID)
            If aWorkspace Is Nothing Then
                Call CoreMessageHandler(message:="workspaceID '" & workspaceID & "' is not defined", subname:="Track.UpdateAllTrack", _
                                        showmsgbox:=True, _
                                        messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            Dim aTarget As New Target
            Dim aCurrTarget As New CurrentTarget
            Dim aDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim aSchedule As New Schedule
            Dim aTimestamp As Date = Date.Now
            Dim aGap As Long
            Dim flag As Boolean
            Dim progress As ULong
            Dim maximum As ULong



            Try
                ' init
                If workerthread IsNot Nothing Then
                    workerthread.ReportProgress(0, "#1 getting all deliverables")
                End If

                '*** Get List of Deliverables
                Dim aCollection As List(Of Deliverable) = aDeliverable.All
                maximum = aCollection.Count

                ' init
                If workerthread IsNot Nothing Then
                    workerthread.ReportProgress(0, "#2 checking " & aCollection.Count & " deliverables")
                End If

                For Each aDeliverable In aCollection
                    flag = False

                    '** progress
                    If Not workerthread Is Nothing Then
                        progress += 1
                        workerthread.ReportProgress((progress / maximum) * 100, "#4 checking progress: " & String.Format("{0:0%}", (progress / maximum)))
                    End If

                    aTrack = aDeliverable.GetTrack(workspaceID:=workspaceID)
                    aSchedule = aDeliverable.GetSchedule(workspaceID:=workspaceID)
                    aTarget = aDeliverable.GetTarget(workspaceID:=workspaceID)

                    ' Track exists
                    If aTrack IsNot Nothing Then
                        If aTrack.IsLoaded Or aTrack.IsCreated Then
                            ' check on Target actual
                            If Not aTarget.IsLoaded And Not aTarget.IsCreated Then
                                aTarget = New Target
                                Call aTarget.Create(uid:=aDeliverable.Uid, updc:=aTrack.TargetUPDC)
                                Call aTarget.PublishNewTarget(NewTargetDate:=ConstNullDate, workspaceID:=aTrack.workspaceID, UID:=aDeliverable.Uid)
                                aTarget.Revision = aTrack.TargetRevision
                                aTarget.Target = aTrack.CurrentTarget
                                aTarget.workspaceID = aTrack.workspaceID
                                aTarget.Persist()

                            End If

                            ' update the forecast
                            If aSchedule.IsLoaded Or aSchedule.IsCreated Then
                                If aSchedule.hasMilestone(aTrack.MSIDFinish, hasData:=True) Then
                                    If aTrack.CurrentForecast <> aSchedule.GetMilestoneValue(aTrack.MSIDFinish) Then
                                        aTrack.CurrentForecast = aSchedule.GetMilestoneValue(aTrack.MSIDFinish)
                                        flag = True
                                    End If
                                End If
                            End If
                            ' check on Target actual
                            If aTarget.IsLoaded Or aTarget.IsCreated Then
                                If aTrack.TargetUPDC <> aTarget.UPDC Then
                                    Call aTrack.UpdateFromTarget(aTarget, workspaceID:=workspaceID, persist:=True, checkGAP:=True)
                                End If
                            End If

                            ' GAP to Target
                            aGap = aTrack.GAPToTarget
                            If aTrack.CheckOnGap Then
                                If aGap <> aTrack.GAPToTarget Then
                                    flag = flag Or True
                                End If
                            End If
                            'GAP to Baseline
                            aGap = aTrack.BaselineGAPToTarget
                            If aTrack.CheckOnBaselineGap Then
                                If aGap <> aTrack.BaselineGAPToTarget Then
                                    flag = flag Or True
                                End If
                            End If


                            ' check on finish
                            If aSchedule IsNot Nothing Then
                                ' create new
                                If Not aSchedule.IsLoaded And Not aSchedule.IsCreated Then
                                    ''' HACK !
                                    If aTrack.Scheduletype <> "none" And aTrack.Scheduletype <> "" Then
                                        aSchedule = New Schedule
                                        Call aSchedule.Create(uid:=aDeliverable.Uid, updc:=aTrack.ScheduleUPDC)
                                        aSchedule.Typeid = aTrack.Scheduletype
                                        aSchedule.workspaceID = aTrack.workspaceID
                                        Call aSchedule.Publish()

                                    End If
                                End If

                                If Not aSchedule.IsLoaded And Not aSchedule.IsCreated Then
                                    If aSchedule.IsFinished And Not aTrack.IsFinished Then
                                        aTrack.IsFinished = aSchedule.IsFinished
                                        'hardcoded finish
                                        ''' HACK !
                                        aTrack.FinishedOn = aSchedule.GetMilestoneValue("bp10")
                                        flag = True
                                    ElseIf Not aSchedule.IsFinished And aTrack.IsFinished Then
                                        aTrack.IsFinished = False
                                        flag = True
                                        aTrack.FinishedOn = ConstNullDate
                                    End If

                                End If
                            End If
                            ' save
                            If flag Then
                                Call aTrack.Persist(timestamp:=aTimestamp)
                            End If
                        End If


                        ' no track
                        If Not (aTrack.IsLoaded Or aTrack.IsCreated) Then

                            ' create Target
                            If Not aTarget Is Nothing And Not aTarget.IsLoaded And Not aTarget.IsCreated Then
                                aTarget = New Target
                                Call aTarget.Create(uid:=aDeliverable.Uid, updc:=0)
                                aTarget.Persist()
                            End If

                            ' create schedule
                            If aSchedule Is Nothing And Not aSchedule.IsLoaded And Not aSchedule.IsCreated Then
                                aSchedule = New Schedule
                                Call aSchedule.Create(uid:=aDeliverable.Uid, updc:=aTrack.ScheduleUPDC)
                                aSchedule.Typeid = ""
                                aSchedule.Persist()
                            End If

                            ' create the missing track
                            If (aSchedule.IsLoaded Or aSchedule.IsCreated) And (aTarget.IsLoaded Or aTarget.IsCreated) Then
                                aTrack = New Track
                                aTrack.Scheduletype = aSchedule.Typeid

                                Call aTrack.Create(aDeliverable.Uid, aSchedule.Uid, aSchedule.Updc, aTarget.UPDC)
                                Call aTrack.UpdateFromTarget(aTarget, workspaceID:=workspaceID, persist:=True, checkGAP:=True)
                                Call aTrack.UpdateFromSchedule(aSchedule, workspaceID:=workspaceID, persist:=True, checkGAP:=True)
                            End If
                        End If

                    End If

                Next aDeliverable

                '** progress
                If Not workerthread Is Nothing Then
                    progress += 1
                    workerthread.ReportProgress(100, "#5 all deliverable tracks checked ")
                End If

                CoreMessageHandler(message:=maximum & " deliverables checked and tracks updated", messagetype:=otCoreMessageType.ApplicationInfo, _
                                   subname:="Track.UpdateAllTracks")
                Return True

                Exit Function

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="Track.updateAllTracks")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Create persistency schema for deliverable tracks
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function CreateSchema() As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Track)()

            '            Dim PrimaryColumnNames As New Collection
            '            Dim WorkspaceColumnNames As New Collection
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.Tablename = constTableID

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()

            '                '*** deliverableUID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workspaceID"
            '                aFieldDesc.Aliases = New String() {"ws"}
            '                aFieldDesc.ColumnName = constFNWorkspace
            '                aFieldDesc.ID = "dtr1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '**** deliverableUID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "UID of deliverable"
            '                aFieldDesc.Aliases = New String() {"uid"}
            '                aFieldDesc.ColumnName = constFNUid
            '                aFieldDesc.ID = "dtr2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '***** scheduleUID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "uid of schedule"
            '                aFieldDesc.ID = "dtr3"
            '                aFieldDesc.ColumnName = "suid"
            '                aFieldDesc.Aliases = New String() {"uid"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '***** scheduleUPDC
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of schedule"
            '                aFieldDesc.ID = "dtr4"
            '                aFieldDesc.ColumnName = "supdc"
            '                aFieldDesc.Aliases = New String() {"bs3"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '***** targetUPDC
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of target"
            '                aFieldDesc.ID = "dtr5"
            '                aFieldDesc.ColumnName = "tupdc"
            '                aFieldDesc.Aliases = New String() {"t10"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)


            '                '**** scheduel type
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "schedule type"
            '                aFieldDesc.Aliases = New String() {"bs4"}
            '                aFieldDesc.ColumnName = constFNTypeID
            '                aFieldDesc.ID = "dtr6"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** version
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "forecast planning version"
            '                aFieldDesc.Aliases = New String() {"bs2"}
            '                aFieldDesc.ColumnName = "plver"
            '                aFieldDesc.ID = "dtr7"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** version
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "target version"
            '                aFieldDesc.Aliases = New String() {"t9"}
            '                aFieldDesc.ColumnName = "tver"
            '                aFieldDesc.ID = "dtr8"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** Milestone for Finish
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "Milestone ID for FC Finish"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = "msfinid"
            '                aFieldDesc.ID = "dtr9"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** current fc
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "forecast date"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = "fcdate"
            '                aFieldDesc.ID = "dtr10"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** current target
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "curr. target date"
            '                aFieldDesc.Aliases = New String() {"t2"}
            '                aFieldDesc.ColumnName = constFNFinished
            '                aFieldDesc.ID = "dtr11"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** status
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "forecast lifecycle status"
            '                aFieldDesc.Aliases = New String() {"bs1"}
            '                aFieldDesc.ColumnName = "lcstatus"
            '                aFieldDesc.ID = "dtr12"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** process status
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "process status"
            '                aFieldDesc.Aliases = New String() {"s1"}
            '                aFieldDesc.ColumnName = "pstatus"
            '                aFieldDesc.ID = "dtr13"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** synchro check
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "synchro check status"
            '                aFieldDesc.Aliases = New String() {"bs6"}
            '                aFieldDesc.ColumnName = "sync"
            '                aFieldDesc.ID = "dtr14"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** synchro check date
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "synchro check status date"
            '                aFieldDesc.Aliases = New String() {"bs7"}
            '                aFieldDesc.ColumnName = "syncchkon"
            '                aFieldDesc.ID = "dtr15"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** going alive
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "forecast going alive date"
            '                aFieldDesc.Aliases = New String() {"a7"}
            '                aFieldDesc.ColumnName = "goal"
            '                aFieldDesc.ID = "dtr16"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** Isfinished
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "deliverable is delivered"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = constFNIsFinished
            '                aFieldDesc.ID = "dtr17"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** blockingITEMID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "blocking item"
            '                aFieldDesc.ColumnName = "blitemid"
            '                aFieldDesc.ID = "dtr18"
            '                aFieldDesc.Aliases = New String() {"bs5"}
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** Isfinished
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "deliverable is delivered on"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = constFNFinished
            '                aFieldDesc.ID = "dtr19"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** FCGap
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "curr. FC gap to Target"
            '                aFieldDesc.ID = "dtr20"
            '                aFieldDesc.ColumnName = "fcgap"
            '                aFieldDesc.Aliases = New String() {"t4"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** Baseline Gap
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "baseline gap to Target"
            '                aFieldDesc.ID = "dtr21"
            '                aFieldDesc.ColumnName = "blgap"
            '                aFieldDesc.Aliases = New String() {"t10"}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** baseline from
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "schedule / fc last changed on"
            '                aFieldDesc.Aliases = New String() {"a5"}
            '                aFieldDesc.ColumnName = "fcchanged"
            '                aFieldDesc.ID = "dtr23"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** Baseline Finish date
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "baseline schedule finish date"
            '                aFieldDesc.Aliases = New String() {"k1"}
            '                aFieldDesc.ColumnName = "basefinish"
            '                aFieldDesc.ID = "dtr24"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** IsFrozen
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "schedule is Baseline"
            '                aFieldDesc.Aliases = New String() {"k4"}
            '                aFieldDesc.ColumnName = "isfrozen"
            '                aFieldDesc.ID = "dtr25"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** baseline scheduleUPDC
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "scheduleUPDC of the baseline"
            '                aFieldDesc.Aliases = New String() {"k3"}
            '                aFieldDesc.ColumnName = "baseupdc"
            '                aFieldDesc.ID = "dtr26"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** baseline from
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "baseline from date"
            '                aFieldDesc.Aliases = New String() {"k2"}
            '                aFieldDesc.ColumnName = "baselinefrom"
            '                aFieldDesc.ID = "dtr27"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** messagelog
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "tag of messagelog"
            '                aFieldDesc.ID = "dtr30"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = "msglogtag"
            '                aFieldDesc.Size = 100
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** activeTag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "tag of activitiy"
            '                aFieldDesc.ID = "dtr31"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = "acttag"
            '                aFieldDesc.Size = 100
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_txt 1
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 1 of condition"
            '                aFieldDesc.ColumnName = "param_txt1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_txt 2
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 2 of condition"
            '                aFieldDesc.ColumnName = "param_txt2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_txt 2
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 3 of condition"
            '                aFieldDesc.ColumnName = "param_txt3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_num 1
            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "parameter numeric 1 of condition"
            '                aFieldDesc.ColumnName = "param_num1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_num 2
            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "parameter numeric 2 of condition"
            '                aFieldDesc.ColumnName = "param_num2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_num 2
            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "parameter numeric 3 of condition"
            '                aFieldDesc.ColumnName = "param_num3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_date 1
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "parameter date 1 of condition"
            '                aFieldDesc.ColumnName = "param_date1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_date 2
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "parameter date 2 of condition"
            '                aFieldDesc.ColumnName = "param_date2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_date 3
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "parameter date 3 of condition"
            '                aFieldDesc.ColumnName = "param_date3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_flag 1
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "parameter flag 1 of condition"
            '                aFieldDesc.ColumnName = "param_flag1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_flag 2
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "parameter flag 2 of condition"
            '                aFieldDesc.ColumnName = "param_flag2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_flag 3
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "parameter flag 3 of condition"
            '                aFieldDesc.ColumnName = "param_flag3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.Relation = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("WorkspacePrimary", WorkspaceColumnNames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .AlterSchema()
            '            End With

            '            '
            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="Track.createSchema", tablename:=constTableID)
            '            CreateSchema = False
        End Function

        '****** allByUID: "static" function to return a collection of curSchedules by key
        '******
        Public Shared Function AllByUID(deliverableUID As Long, _
                                        Optional ByVal scheduleUPDC As Long = -1, _
                                        Optional ByVal targetUPDC As Long = -1) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key() As Object


            ' set the primaryKey
            ReDim Key(OTDBConst_DependStatus_g1)
            Key(0) = deliverableUID

            Try

                aTable = GetTableStore(ConstTableID)
                aRecordCollection = aTable.GetRecordsByIndex(aTable.TableSchema.PrimaryKeyIndexName, Key, True)

                If aRecordCollection Is Nothing Then
                    Return aCollection
                Else
                    For Each aRecord As ormRecord In aRecordCollection
                        Dim aNewDelivTrack As New Track
                        If InfuseDataObject(record:=aRecord, dataobject:=aNewDelivTrack) Then
                            If ((aNewDelivTrack.ScheduleUPDC = scheduleUPDC And scheduleUPDC > -1) Or (scheduleUPDC = -1)) _
                            And ((aNewDelivTrack.TargetUPDC = targetUPDC And targetUPDC > -1) Or (targetUPDC = -1)) Then
                                aCollection.Add(Item:=aNewDelivTrack)
                            End If
                        End If
                    Next aRecord
                    Return aCollection
                End If

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Track.AllByUID", tablename:=ConstTableID, messagetype:=otCoreMessageType.InternalException)
                Return aCollection
            End Try

        End Function


        ''' <summary>
        ''' create the data object by primary key
        ''' </summary>
        ''' <param name="deliverableUID"></param>
        ''' <param name="scheduleUID"></param>
        ''' <param name="scheduleUPDC"></param>
        ''' <param name="targetUPDC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal deliverableUID As Long, _
                                ByVal scheduleUID As Long, _
                                ByVal scheduleUPDC As Long, _
                                ByVal targetUPDC As Long) As Boolean
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            If MyBase.Create(pkarray, checkUnique:=True) Then
                ' set the primaryKey
                _deliverableUID = deliverableUID
                _scheduleUID = scheduleUID
                _scheduleUPDC = scheduleUPDC
                _targetUPDC = targetUPDC
                Return True
            Else
                Return False
            End If

        End Function

        ''' <summary>
        ''' clone the track
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkarray() As Object) As Track Implements iotCloneable(Of Track).Clone
            If Not Feed() Then
                Return Nothing
            End If
            Return MyBase.Clone(Of Track)(pkarray)
        End Function
        ''' <summary>
        ''' clone the deliverable track
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal deliverableUID As Long, _
        ByVal scheduleUID As Long, _
        ByVal scheduleUPDC As Long, _
        ByVal targetUPDC As Long) As Track
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            Return Me.Clone(pkarray)
        End Function
        ''' <summary>
        ''' load and infuse data object by primary key
        ''' </summary>
        ''' <param name="deliverableUID"></param>
        ''' <param name="scheduleUID"></param>
        ''' <param name="scheduleUPDC"></param>
        ''' <param name="targetUPDC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal deliverableUID As Long, _
        ByVal scheduleUID As Long, _
        ByVal scheduleUPDC As Long, _
        ByVal targetUPDC As Long) As Boolean
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            Return MyBase.Inject(pkarray)
        End Function



        '****
        '**** updateFromTarget -> update a Track from a given Target
        '****
        ''' <summary>
        '''  updateFromTarget -> update a Track from a given Target
        ''' </summary>
        ''' <param name="TARGET"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="PERSIST"></param>
        ''' <param name="checkGAP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateFromTarget(ByRef target As Target, _
        Optional ByVal workspaceID As String = "", _
        Optional ByVal persist As Boolean = True, _
        Optional ByVal checkGAP As Boolean = True) As Boolean

            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aCurrTarget As New CurrentTarget
            Dim aSchedule As New Schedule
            Dim aWorkspace As String
            Dim aNewSchedule As New Schedule
            Dim aNewTarget As New Target
            Dim dlvUID As Long
            Dim sUID As Long
            Dim sUPDC As Long
            Dim tUPDC As Long
            ' workspaceID
            If IsMissing(workspaceID) Then
                aWorkspace = CurrentSession.CurrentWorkspaceID
            Else
                aWorkspace = CStr(workspaceID)
            End If

            If target Is Nothing Then
                UpdateFromTarget = False
                Exit Function
            End If

            If Not target.IsLoaded And Not target.IsCreated Then
                Call CoreMessageHandler(message:="input deliverable target is not created nor loaded", break:=False, _
                                        subname:="Track.updateFromTarget")
                If s_dlvTarget Is Nothing Then
                    UpdateFromTarget = False
                    Exit Function
                ElseIf Not s_dlvTarget.IsCreated And Not s_dlvTarget.IsLoaded Then
                    s_dlvTarget = Nothing
                    UpdateFromTarget = False
                    Exit Function
                Else
                    aNewTarget = s_dlvTarget
                    dlvUID = aNewTarget.UID
                    tUPDC = aNewTarget.UPDC
                End If
            Else
                aNewTarget = target
                dlvUID = target.UID
                tUPDC = target.UPDC
            End If
            ' set the objects
            aCurrSCHEDULE = s_deliverable.GetCurrSchedule(workspaceID:=aWorkspace)
            If aCurrSCHEDULE Is Nothing Then
                sUID = 0
                sUPDC = 0
                s_schedule = Nothing
            Else
                sUID = aCurrSCHEDULE.UID
                sUPDC = aCurrSCHEDULE.UPDC
                If aSchedule.Inject(UID:=sUID, updc:=sUPDC) Then
                    aNewSchedule = aSchedule

                Else
                    s_schedule = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsCreated And Not _IsLoaded Then
                If Not Me.Create(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC) Then
                    Call Me.Inject(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC)
                End If
            End If

            '**** create -> init
            s_schedule = aNewSchedule
            s_dlvTarget = aNewTarget

            With Me
                .workspaceID = aWorkspace
                .TargetRevision = s_dlvTarget.Revision
                .CurrentTarget = s_dlvTarget.Target

                ' schedule
                .ScheduleRevision = s_schedule.Revision
                .IsFrozen = s_schedule.IsFrozen
                .IsFinished = s_schedule.IsFinished
                If s_schedule.HasMilestoneDate("bp10") Then
                    .FinishedOn = s_schedule.GetMilestoneValue("bp10")
                Else
                    .FinishedOn = ConstNullDate
                End If
                .MSIDFinish = "bp9"
                If s_schedule.HasMilestoneDate(.MSIDFinish) Then
                    .CurrentForecast = s_schedule.GetMilestoneValue(.MSIDFinish)
                Else
                    .CurrentForecast = ConstNullDate
                End If
                If checkGAP Then .CheckOnGap()
                If s_schedule.IsBaseline Then
                    .BaseLineFinishDate = s_schedule.GetMilestoneValue(.MSIDFinish)
                    .BaseLineFinishDateFrom = s_schedule.CreatedOn
                    .BaseLineUPDC = s_schedule.Updc
                    If checkGAP Then .CheckOnBaselineGap()
                End If
                .FCLCStatus = s_schedule.LFCStatus
                .ProcessStatus = s_schedule.ProcessStatus
                'If .GoingAliveDate <> ot.ConstNullDate  and .fclcstatus = "g1" Then
                '    .GoingAliveDate = s_schedule.createdOn
                'End If
                .ForecastChangedOn = s_schedule.LastForecastUpdate
                .Scheduletype = s_schedule.Typeid

            End With

            If persist And Me.IsChanged Then
                UpdateFromTarget = Me.Persist
            Else
                UpdateFromTarget = True
            End If

        End Function
        '****
        '**** updateFromSchedule -> update a Track from a given Schedule
        '****
        ''' <summary>
        ''' updateFromSchedule -> update a Track from a given Schedule
        ''' </summary>
        ''' <param name="SCHEDULE"></param>
        ''' <param name="targetUPDC"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="PERSIST"></param>
        ''' <param name="checkGAP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateFromSchedule(ByRef schedule As Schedule, _
        Optional ByVal targetUPDC As Long = -1, _
        Optional ByVal workspaceID As String = "", _
        Optional ByVal persist As Boolean = True, _
        Optional ByVal checkGAP As Boolean = True) As Boolean

            Dim aTarget As New Target
            Dim aCurrTarget As New CurrentTarget
            Dim aSchedule As New Schedule
            Dim aNewSchedule As New Schedule
            Dim aNewTarget As New Target
            Dim aWorkspace As String
            Dim dlvUID As Long
            Dim sUID As Long
            Dim sUPDC As Long
            Dim tUPDC As Long
            ' workspaceID
            If IsMissing(workspaceID) Then
                aWorkspace = CurrentSession.CurrentWorkspaceID
            Else
                aWorkspace = CStr(workspaceID)
            End If

            If schedule Is Nothing Then
                UpdateFromSchedule = False
                Exit Function
            End If

            If Not schedule.IsLoaded And Not schedule.IsCreated Then
                Call CoreMessageHandler(message:="input deliverable SCHEDULE is not created nor loaded", break:=False, _
                                        subname:="Track.updateFromSchedule")
                If s_schedule Is Nothing Then
                    UpdateFromSchedule = False
                    Exit Function
                ElseIf Not s_schedule.IsCreated And Not s_schedule.IsLoaded Then
                    s_schedule = Nothing
                    UpdateFromSchedule = False
                    Exit Function
                Else
                    aNewSchedule = s_schedule
                    dlvUID = s_schedule.Uid    ' assumption
                    sUID = s_schedule.Uid
                    sUPDC = s_schedule.Updc
                End If
            Else
                aNewSchedule = schedule
                dlvUID = aNewSchedule.Uid    ' assumption
                sUID = aNewSchedule.Uid
                sUPDC = aNewSchedule.Updc
            End If

            '*** Target is the Current if not specified otherwise
            If targetUPDC = -1 Then
                If Not aCurrTarget.Inject(uid:=dlvUID, workspaceID:=workspaceID) Then
                    tUPDC = 0
                    aNewTarget = Nothing
                Else
                    tUPDC = aCurrTarget.UPDC
                    If aTarget.Inject(uid:=dlvUID, updc:=tUPDC) Then
                        aNewTarget = aTarget
                    Else
                        aNewTarget = Nothing
                    End If
                End If
            Else
                tUPDC = targetUPDC
                If aTarget.Inject(uid:=dlvUID, updc:=tUPDC) Then
                    aNewTarget = aTarget
                Else
                    aNewTarget = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsCreated And Not _IsLoaded Then
                If Not Me.Create(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC) Then
                    Call Me.Inject(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC)
                End If
            End If

            '** initialize in create/Inject !!
            _deliverableUID = dlvUID
            _scheduleUID = sUID
            _scheduleUPDC = sUPDC
            _targetUPDC = tUPDC
            s_schedule = aNewSchedule
            s_dlvTarget = aNewTarget

            With Me
                .workspaceID = aWorkspace


                '* finished
                .IsFinished = s_schedule.IsFinished
                If s_schedule.HasMilestoneDate("bp10") Then
                    .FinishedOn = s_schedule.GetMilestoneValue("bp10")
                Else
                    .FinishedOn = ConstNullDate
                End If
                '* forecast
                .MSIDFinish = "bp9"
                If s_schedule.HasMilestoneDate(.MSIDFinish) Then
                    .CurrentForecast = s_schedule.GetMilestoneValue(.MSIDFinish)
                Else
                    .CurrentForecast = ConstNullDate
                End If

                '* check the gap
                If checkGAP Then .CheckOnGap()

                '* baseline itself
                If s_schedule.IsBaseline Then
                    .IsFrozen = True
                    .ScheduleRevision = s_schedule.Revision
                    .BaseLineFinishDate = s_schedule.GetMilestoneValue(.MSIDFinish)
                    If s_schedule.BaselineRefDate = ConstNullDate Then
                        .BaseLineFinishDateFrom = s_schedule.CreatedOn
                    Else
                        .BaseLineFinishDateFrom = s_schedule.BaselineRefDate
                    End If
                    .BaseLineUPDC = s_schedule.Updc
                    If checkGAP Then .CheckOnBaselineGap()

                    '* take the data from the frozen one
                ElseIf s_schedule.IsFrozen Then
                    .IsFrozen = True
                    .ScheduleRevision = s_schedule.Revision
                    .BaseLineUPDC = s_schedule.BaselineUPDC
                    If s_schedule.BaselineRefDate = ConstNullDate Then
                        .BaseLineFinishDateFrom = s_schedule.CreatedOn
                    Else
                        .BaseLineFinishDateFrom = s_schedule.BaselineRefDate
                    End If
                    Dim aBaseline As New Schedule
                    If aBaseline.Inject(UID:=s_schedule.Uid, updc:=s_schedule.BaselineUPDC) Then
                        .BaseLineFinishDate = aBaseline.GetMilestoneValue(.MSIDFinish)
                        If checkGAP Then .CheckOnBaselineGap()
                    End If
                    '* reset the freeze
                Else
                    .IsFrozen = False
                    .ScheduleRevision = ""
                    .BaseLineUPDC = -1
                    .BaseLineFinishDate = ConstNullDate
                    .BaseLineFinishDateFrom = ConstNullDate
                End If

                '* take the status
                .FCLCStatus = s_schedule.LFCStatus
                .ProcessStatus = s_schedule.ProcessStatus
                'If .GoingAliveDate <> ot.ConstNullDate  and .fclcstatus = "g1" Then
                '    .GoingAliveDate = s_schedule.createdOn
                'End If
                .ForecastChangedOn = s_schedule.LastForecastUpdate
                .Scheduletype = s_schedule.Typeid
                .TargetRevision = s_dlvTarget.Revision

            End With

            If persist And Me.IsChanged Then
                UpdateFromSchedule = Me.Persist
            Else
                UpdateFromSchedule = True
            End If

        End Function
        '****
        '**** updateFromDeliverable -> updated a Track from a given deliverable
        '****
        ''' <summary>
        ''' updateFromDeliverable -> updated a Track from a given deliverable
        ''' </summary>
        ''' <param name="DELIVERABLE"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="checkGAP"></param>
        ''' <param name="PERSIST"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateFromDeliverable(ByRef deliverable As Deliverable, _
        Optional ByVal workspaceID As String = "", _
        Optional ByVal checkGAP As Boolean = False, _
        Optional ByVal persist As Boolean = True _
        ) As Boolean
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aCurrTarget As New CurrentTarget
            Dim aTarget As New Target
            Dim aSchedule As New Schedule
            Dim aWorkspace As String
            Dim dlvUID As Long
            Dim sUID As Long
            Dim sUPDC As Long
            Dim tUPDC As Long


            ' workspaceID
            If IsMissing(workspaceID) Then
                aWorkspace = CurrentSession.CurrentWorkspaceID
            Else
                aWorkspace = CStr(workspaceID)
            End If

            If deliverable Is Nothing Then
                UpdateFromDeliverable = False
                Exit Function
            End If
            '*** check deliverable
            If Not deliverable.IsLoaded And Not deliverable.IsCreated Then
                Call CoreMessageHandler(message:="input deliverable is not created nor loaded", break:=False, _
                                        subname:="Track.updateFromDeliverable")
                If s_deliverable Is Nothing Then
                    UpdateFromDeliverable = False
                    Exit Function
                Else
                    dlvUID = s_deliverable.Uid
                End If
            Else
                s_deliverable = deliverable
                dlvUID = deliverable.Uid
            End If

            ' set the objects
            aCurrSCHEDULE = s_deliverable.GetCurrSchedule(workspaceID:=aWorkspace)
            If aCurrSCHEDULE Is Nothing Then
                sUID = 0
                sUPDC = 0
                s_schedule = Nothing
            Else
                sUID = aCurrSCHEDULE.UID
                sUPDC = aCurrSCHEDULE.UPDC
                If aSchedule.Inject(UID:=sUID, updc:=sUPDC) Then
                    s_schedule = aSchedule
                    If s_schedule.workspaceID <> aWorkspace Then
                        aWorkspace = s_schedule.workspaceID
                    End If
                Else
                    s_schedule = Nothing
                End If
            End If

            aCurrTarget = s_deliverable.GetCurrTarget(workspaceID:=aWorkspace)
            If aCurrTarget Is Nothing Then
                tUPDC = 0
                s_dlvTarget = Nothing
            Else
                tUPDC = aCurrTarget.UPDC
                If aTarget.Inject(uid:=dlvUID, updc:=tUPDC) Then
                    s_dlvTarget = aTarget
                Else
                    s_dlvTarget = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsCreated And Not _IsLoaded Then
                If Not Me.Inject(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC) Then
                    Call Me.Create(deliverableUID:=dlvUID, scheduleUID:=sUID, scheduleUPDC:=sUPDC, targetUPDC:=tUPDC)
                End If
            End If
            '*** set the values
            With Me
                .workspaceID = aWorkspace
                '*** should come out of config -> hardcoded
                If .MSIDFinish.ToLower <> "bp9" Then
                    .MSIDFinish = "bp9"
                End If
                If aTarget.IsLoaded Or aTarget.IsCreated Then
                    Call .UpdateFromTarget(target:=aTarget, workspaceID:=aWorkspace, persist:=False, checkGAP:=False)
                End If
                If aSchedule.IsLoaded Or aSchedule.IsCreated Then
                    Call .UpdateFromSchedule(schedule:=aSchedule, workspaceID:=aWorkspace, persist:=False, checkGAP:=False)
                End If
                If checkGAP Then Call .CheckOnGap()
                If checkGAP Then Call .CheckOnBaselineGap()
            End With

            If persist And Me.IsChanged Then
                UpdateFromDeliverable = Me.Persist
            Else
                UpdateFromDeliverable = True
            End If
        End Function

        '***** setTarget -> set the Internal Target
        '*****
        ''' <summary>
        ''' setTarget -> set the Internal Target
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SetTarget() As Boolean
            Dim aTarget As New Target
            If Not _IsLoaded And Not Me.IsCreated Then
                SetTarget = False
                Exit Function
            End If

            If Not s_dlvTarget Is Nothing Then
                If (s_dlvTarget.IsLoaded Or s_dlvTarget.IsCreated) And s_dlvTarget.UID = Me.DeliverableUID And s_dlvTarget.UPDC = Me.TargetUPDC Then
                    SetTarget = True
                    Exit Function
                End If
            End If

            If Not aTarget.Inject(uid:=Me.DeliverableUID, updc:=Me.TargetUPDC) Then
                s_dlvTarget = Nothing
                SetTarget = False
                Exit Function
            End If

            s_dlvTarget = aTarget
            SetTarget = True
        End Function

        '***** setSchedule -> set the Internal Schedule
        '*****
        ''' <summary>
        ''' setSchedule -> set the Internal Schedule
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SetSchedule() As Boolean
            Dim aSchedule As New Schedule
            If Not _IsLoaded And Not Me.IsCreated Then
                SetSchedule = False
                Exit Function
            End If

            If Not s_schedule Is Nothing Then
                If (s_schedule.IsLoaded Or s_schedule.IsCreated) And s_schedule.Uid = Me.ScheduleUID And s_schedule.Updc = Me.ScheduleUPDC Then
                    SetSchedule = True
                    Exit Function
                End If
            End If

            If Not aSchedule.Inject(UID:=Me.ScheduleUID, updc:=Me.ScheduleUPDC) Then
                s_schedule = Nothing
                SetSchedule = False
                Exit Function
            End If

            s_schedule = aSchedule
            SetSchedule = True
        End Function
        '***** checkOnGAP -> Calculate the GAP
        '*****
        ''' <summary>
        ''' checkOnGAP -> Calculate the GAP
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckOnGap() As Boolean
            Dim aCE As New CalendarEntry
            Dim aDefScheduleMS As ScheduleMilestoneDefinition
            Dim aDate As Date
            Dim actual As String
            Dim gap As Long

            If Not _IsLoaded And Not Me.IsCreated Then
                CheckOnGap = False
                Exit Function
            End If

            ' set the objects
            If Me.CurrentTarget = ConstNullDate Then
                If SetTarget() Then
                    Me.CurrentTarget = s_dlvTarget.Target
                Else
                    CheckOnGap = False
                    Exit Function
                End If
            End If
            If Not Me.IsFinished And Me.CurrentForecast = ConstNullDate Then
                If SetSchedule() Then
                    If s_schedule.HasMilestoneDate(Me.MSIDFinish) Then
                        Me.CurrentForecast = s_schedule.GetMilestoneValue(Me.MSIDFinish)

                        'Set aDefScheduleMS = s_schedule.getDefScheduleMilestone(Me.MSIDFinish)
                        'actual = aDefScheduleMS.actualOfFC(Me.MSIDFinish)
                    Else
                        CheckOnGap = False
                        Exit Function
                    End If
                Else
                    CheckOnGap = False
                    Exit Function
                End If
            ElseIf Me.IsFinished And Me.FinishedOn = ConstNullDate Then
                If SetSchedule() Then
                    aDefScheduleMS = s_schedule.GetDefScheduleMilestone(Me.MSIDFinish)
                    actual = aDefScheduleMS.ActualOfFC
                    If s_schedule.HasMilestoneDate(actual) Then
                        Me.FinishedOn = s_schedule.GetMilestoneValue(Me.MSIDFinish)
                    End If
                Else
                    CheckOnGap = False
                    Exit Function
                End If
            End If

            If Me.IsFinished Then
                aDate = Me.FinishedOn
            ElseIf Me.CurrentForecast < Date.Now() Then
                aDate = Date.Now()
            Else
                aDate = Me.CurrentForecast
            End If

            If aDate <> ConstNullDate And Me.CurrentTarget <> ConstNullDate Then
                aCE.Datevalue = aDate
                gap = aCE.deltaDay(Me.CurrentTarget, considerAvailibilty:=True)
                Me.GAPToTarget = gap

                CheckOnGap = True
            Else
                CheckOnGap = False
                Me.GAPToTarget = 0
                Exit Function
            End If
        End Function
        '***** checkOnBaselineGAP -> Calculate the GAP
        '*****
        ''' <summary>
        ''' checkOnBaselineGAP -> Calculate the baseline GAP
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckOnBaselineGap() As Boolean
            Dim aCE As New CalendarEntry
            Dim gap As Long

            If Not _IsLoaded And Not Me.IsCreated Then
                CheckOnBaselineGap = False
                Exit Function
            End If

            ' set the objects
            If Me.CurrentTarget = ConstNullDate Then
                If SetTarget() Then
                    Me.CurrentTarget = s_dlvTarget.Target
                Else
                    CheckOnBaselineGap = False
                    Exit Function
                End If
            End If
            If Me.BaseLineFinishDate = ConstNullDate Then
                CheckOnBaselineGap = False
                Exit Function
            End If
            If Me.BaseLineFinishDate <> ConstNullDate And Me.CurrentTarget <> ConstNullDate Then
                aCE.Datevalue = Me.BaseLineFinishDate
                gap = aCE.deltaDay(Me.CurrentTarget, considerAvailibilty:=True)
                Me.BaselineGAPToTarget = gap

                CheckOnBaselineGap = True
            Else
                Me.BaselineGAPToTarget = 0
                CheckOnBaselineGap = False
            End If
        End Function

    End Class
    ''' <summary>
    ''' Definition class for Deliverables
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=DeliverableType.ConstObjectID, modulename:=ConstModuleDeliverables, Version:=1, useCache:=True)> Public Class DeliverableType
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of DeliverableType)

        Public Const ConstObjectID = "DeliverableType"
        '** Table
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addDomainBehavior:=True, addsparefields:=True)> _
        Public Const ConstTableID = "tblDefDeliverableTypes"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=constFNTypeID, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"

        '*** Fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, primarykeyordinal:=1, defaultValue:="", _
           title:="Type", description:="type of the deliverable", XID:="DLVT1")> Public Const constFNTypeID = "id"

        <ormObjectEntry(referenceobjectentry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, defaultValue:="", _
            title:="Schedule Type", description:="default schedule type of the deliverable", XID:="DLVT21")> _
        Public Const constFNDefScheduleType = "defscheduletype"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, defaultValue:="", _
            title:="Organization Unit", description:="default organization unit responsible of the deliverable", XID:="DLVT22")> _
        Public Const constFNDefRespOU = "defrespOU"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, defaultValue:="", _
           title:="Function", description:="default function type of the deliverable", XID:="DLVT23")> _
        Public Const constFNDefFunction = "deffunction"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, defaultValue:="", _
          title:="Function", description:="default target responsible organization Unit", XID:="DLVT24")> _
        Public Const constFNTargetOU = "deftargetOu"

        <ormObjectEntry(typeid:=otFieldDataType.Bool, size:=50, defaultValue:="0", _
          title:="Target Necessary", description:="has mandatory target data", XID:="DLVT25")> _
        Public Const constFNhastarget = "hastargetdata"


        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
         title:="Description", description:="description of the deliverable type", XID:="DLVT3")> _
        Public Const constFNDescription = "desc"

        <ormObjectEntry(typeid:=otFieldDataType.Memo, defaultValue:="", _
        title:="comment", description:="comments of the deliverable", XID:="DLVT10")> Public Const constFNComment = "cmt"

        '*** Mapping
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String = ""
        <ormEntryMapping(EntryName:=constFNDefScheduleType)> Private _defScheduleType As String = ""

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the type of the def schedule.
        ''' </summary>
        ''' <value>The type of the def schedule.</value>
        Public Property DefScheduleType() As String
            Get
                Return Me._defScheduleType
            End Get
            Set(value As String)
                Me._defScheduleType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the comment.
        ''' </summary>
        ''' <value>The comment.</value>
        Public Property Comment() As String
            Get
                Return Me._comment
            End Get
            Set(value As String)
                Me._comment = value
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
                Me._description = value
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
        Public Overloads Function Create(ByVal typeid As String, Optional ByVal domainID As String = "") As Boolean
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {typeid, domainID}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                _typeid = typeid
                _domainID = UCase(domainID)
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' loads and infuse the deliverable type by domainID first
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(typeid As String, Optional domainID As String = "") As Boolean

            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {typeid, domainID}

            If MyBase.Inject(pkarray) Then
                Return True
            Else
                Dim pkarrayGlobal() As Object = {typeid, ConstGlobalDomain}
                Return MyBase.Inject(pkarrayGlobal)
            End If
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal typeid As String, Optional ByVal domainID As String = "", Optional forcereload As Boolean = False) As DeliverableType
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {typeid, domainID}
            Dim aType As DeliverableType = Retrieve(Of DeliverableType)(pkArray:=pkarray, forceReload:=forcereload)
            If aType Is Nothing Then
                Dim pkGlobalArray() As Object = {typeid, ConstGlobalDomain}
                Return Retrieve(Of DeliverableType)(pkArray:=pkGlobalArray, forceReload:=forcereload)
            End If
        End Function
        ''' <summary>
        ''' Clone the object with its primary key array.
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <returns>the new object or nothing</returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkArray() As Object) As DeliverableType Implements iotCloneable(Of DeliverableType).Clone
            '*** now we copy the object
            Dim aNewObject As New DeliverableType

            '* must be loaded
            If Not IsLoaded And Not IsCreated Then
                Return Nothing
            End If

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Return Nothing
                End If
            End If
            '* update the record
            If Not MyBase.Feed() Then
                Return Nothing
            End If

            '** clone it
            aNewObject = Me.Clone(Of DeliverableType)(pkArray)
            If Not aNewObject Is Nothing Then
                aNewObject.Record.SetValue(constFNTypeID, pkArray(0))
                aNewObject._typeid = pkArray(0)
                aNewObject.Record.SetValue(ConstFNDomainID, pkArray(1))
                aNewObject._domainID = pkArray(0)
            End If

            Return aNewObject
        End Function
        ''' <summary>
        ''' Clone the deliverable type
        ''' </summary>
        ''' <param name="typeid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal typeid As String, Optional domainID As String = "") As DeliverableType
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Return Me.Clone({typeid, domainID})
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of DeliverableType)(silent:=silent)
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of Delivertype) for the DomainID
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainID As String = "") As List(Of DeliverableType)
            Dim aCollection As New List(Of DeliverableType)
            Dim aDomainDir As New Dictionary(Of String, DeliverableType)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            '** set the domain
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="all", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & ConstTableID & "." & constFNTypeID & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aRecordCollection = aCommand.RunSelect

                '** get the entries for the domain and global sorted out
                For Each aRecord As ormRecord In aRecordCollection
                    Dim aNewDeliverable As New DeliverableType
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewDeliverable) Then
                        If aDomainDir.ContainsKey(key:=aNewDeliverable.Typeid) Then
                            Dim anExist = aDomainDir.Item(key:=aNewDeliverable.Typeid)
                            If anExist.DomainID = ConstGlobalDomain And aNewDeliverable.DomainID = CurrentSession.CurrentDomainID Then
                                aDomainDir.Remove(key:=aNewDeliverable.Typeid)
                                aDomainDir.Add(key:=aNewDeliverable.Typeid, value:=aNewDeliverable)
                            End If
                        Else
                            aDomainDir.Add(key:=aNewDeliverable.Typeid, value:=aNewDeliverable)
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
    '************************************************************************************
    '***** CLASS Deliverable is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' Deliverable Class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=Deliverable.ConstObjectID, modulename:=ConstModuleDeliverables, useCache:=True, Version:=1)> Public Class Deliverable
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of Deliverable)

        Public Const ConstObjectID = "Deliverable"
        '** Table
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addDomainBehavior:=False, addsparefields:=True)> _
        Public Const ConstTableID = "tblDeliverables"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=constFNfuid, columnname3:=ConstFNIsDeleted)> Public Const constIndexRevisions = "indRevisions"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=ConstFNIsDeleted)> Public Const constIndexDelete = "indDeletes"
        <ormSchemaIndex(columnName1:=constFNPartID, columnname2:=ConstFNIsDeleted)> Public Const constIndexParts = "indParts"
        <ormSchemaIndex(columnName1:=constFNConfigTag, columnname2:=ConstFNIsDeleted)> Public Const constIndexConfigTags = "indConfigTag"
        <ormSchemaIndex(columnName1:=constFNActiveTag, columnname2:=ConstFNIsDeleted)> Public Const constIndexACtiveTags = "indActiveTag"
        <ormSchemaIndex(columnName1:=constFNWBSID, columnname2:=constFNWBSCode, columnname3:=constFNUid, columnname4:=ConstFNIsDeleted)> Public Const constIndexWBS = "indWBS"
        <ormSchemaIndex(columnname1:=constFNMatchCode, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexMatchcode = "indmatchcode"
        <ormSchemaIndex(columnname1:=constFNCategory, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexcategory = "indcategory"
        <ormSchemaIndex(columnname1:=constFNFunction, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexFunction = "indFunction"
        <ormSchemaIndex(columnname1:=constFNTypeID, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexType = "indType"

        '*** primary key
        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=1, _
            title:="Unique ID", description:="unique id of the deliverable", XID:="DLV1", aliases:={"UID"})> _
        Public Const constFNUid = "uid"

        '** fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, _
            title:="category", description:="category of the deliverable", XID:="DLV2")> Public Const constFNCategory = "cat"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
            title:="id", description:="id of the deliverable", XID:="DLV3")> Public Const constFNDeliverableID = "id"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            title:="Matchcode", description:="match code of the deliverable", XID:="DLV4")> Public Const constFNMatchCode = "matchcode"


        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID
        '
        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            Description:="workspaceID ID of the deliverable", defaultvalue:="@", _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.SetDefault & ")"})> Public Const ConstFNWorkspace = Workspace.ConstFNID

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            title:="Revision", description:="revision of the deliverable", XID:="DLV6")> Public Const constFNRevision = "drev"

        <ormObjectEntry(referenceobjectentry:=ConstObjectID & "." & constFNUid, title:="First Revision UID", description:="unique id of the first revision deliverable", _
            XID:="DLV7", isnullable:=True)> Public Const constFNfuid = "fuid"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            title:="Change Reference", description:="change reference of the deliverable", XID:="DLV8")> Public Const constFNChangeRef = "chref"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            title:="Format", description:="format of the deliverable", XID:="DLV9")> Public Const constFNFormat = "frmt"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
            title:="Description", description:="description of the deliverable", XID:="DLV10")> Public Const constFNDescription = "desc"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, _
            title:="Responsible OrgUnit", description:=" organization unit responsible for the deliverable", XID:="DLV11")> _
        Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Part.ConstObjectID & "." & Part.ConstFNPartID, _
            isnullable:=True, description:="part id of the deliverable", XID:="DLV12", _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNPartID = Part.ConstFNPartID

        <ormObjectEntry(referenceobjectentry:=DeliverableType.ConstObjectID & "." & DeliverableType.constFNTypeID, _
            title:="Type", description:="type of the deliverable", XID:="DLV13", _
             useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNTypeID = "typeid"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, _
            title:="Responsible", description:="responsible person for the deliverable", XID:="DLV16")> Public Const constFNResponsiblePerson = "resp"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
            title:="blocking item reference", description:="blocking item reference id for the deliverable", XID:="DLV17")> Public Const constFNBlockingItemReference = "blitemid"

        <ormObjectEntry(typeid:=otFieldDataType.Memo, defaultValue:="", _
            title:="comment", description:="comments of the deliverable", XID:="DLV18")> Public Const constFNComment = "cmt"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
        title:="ConfigTag", description:="config tag for the deliverable", XID:="DLV19")> Public Const constFNConfigTag = "cnftag"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultValue:="", _
        title:="ActivityTag", description:="activity tag for the deliverable", XID:="DLV20")> Public Const constFNActiveTag = "acttag"

        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)>
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
        title:="wbs reference", description:="work break down structure for the deliverable", XID:="DLV22")> _
        Public Const constFNWBSID = "wbs"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, defaultValue:="", _
        title:="wbscode reference", description:="wbscode for the deliverable", XID:="DLV23")> _
        Public Const constFNWBSCode = "wbscode"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, defaultValue:="", _
            title:="Function", description:="function of the deliverable", XID:="DLV30")> Public Const constFNFunction = "function"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=150, _
           XID:="DLV31", Title:="Workpackage", description:="workpackage of the deliverable")> Public Const ConstFNWorkpackage = "wkpk"



        '*** mappings
        <ormEntryMapping(EntryName:=constFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=constFNfuid)> Private _firstrevUID As Long?
        <ormEntryMapping(EntryName:=constFNDeliverableID)> Private _deliverableID As String = ""
        <ormEntryMapping(EntryName:=constFNRevision)> Private _revision As String = ""
        <ormEntryMapping(EntryName:=constFNFormat)> Private _format As String = ""
        <ormEntryMapping(EntryName:=constFNCategory)> Private _category As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String = ""
        'Private s_customerID As String = "" outdated movved to targets
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOUID As String = ""
        <ormEntryMapping(EntryName:=constFNMatchCode)> Private _matchcode As String = ""
        'Private s_assycode As String = "" obsolete
        <ormEntryMapping(EntryName:=constFNPartID)> Private _partID As String = ""
        <ormEntryMapping(EntryName:=constFNChangeRef)> Private _changerefID As String = ""
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=constFNResponsiblePerson)> Private _responsibleID As String = ""
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String = ""
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String = ""
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""
        <ormEntryMapping(EntryName:=constFNConfigTag)> Private _configtag As String = ""
        <ormEntryMapping(EntryName:=constFNActiveTag)> Private _activetag As String = ""
        <ormEntryMapping(EntryName:=constFNWBSID)> Private _wbsid As String = ""
        <ormEntryMapping(EntryName:=constFNWBSCode)> Private _wbscode As String = ""
        <ormEntryMapping(EntryName:=constFNFunction)> Private _function As String = ""
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _wspaceID As String = ""
        <ormEntryMapping(EntryName:=ConstFNWorkpackage)> Private _workpackage As String = ""
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub


#Region "properties"

        ''' <summary>
        ''' gets the UID of the deliverable (unique)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Uid() As Long
            Get
                Uid = _uid
            End Get
        End Property
        ''' <summary>
        ''' set or gets the first revision uid - might be null
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FirstRevisionUID() As Long?
            Get
                Return _firstrevUID
            End Get
            Set(value As Long?)
                SetValue(entryname:=constFNfuid, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the revision id 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Revision() As String
            Get
                Revision = _revision
            End Get
            Set(value As String)
                If value <> _revision Then
                    _revision = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the format of the deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [Format]() As String
            Get
                Format = _format
            End Get
            Set(value As String)
                If value <> _format Then
                    _format = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ID of the deliverable (non-unique)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DeliverableID() As String
            Get
                DeliverableID = _deliverableID
            End Get
            Set(value As String)
                If value <> _deliverableID Then
                    _deliverableID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ID of the deliverable (non-unique)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WorkspaceID() As String
            Get
                WorkspaceID = _wspaceID
            End Get
            Set(value As String)
                If value <> _wspaceID Then
                    _wspaceID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the description of the deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                If value <> _description Then
                    _description = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the category of the deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Category() As String
            Get
                Category = _category
            End Get
            Set(value As String)
                If value <> _category Then
                    _category = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' true if this is a revision 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsRevision() As Boolean
            Get
                If _firstrevUID <> 0 Then
                    IsRevision = True
                Else
                    IsRevision = False
                End If
            End Get

        End Property
        ''' <summary>
        ''' True if this deliverable is first revision
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsFirstRevision() As Boolean
            Get
                If _firstrevUID = 0 Then
                    IsFirstRevision = True
                Else
                    IsFirstRevision = False
                End If

            End Get
        End Property
        ''' <summary>
        '''  gets or sets the responsible Person ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property ResponsibleID() As String
            Get
                ResponsibleID = _responsibleID
            End Get
            Set(value As String)
                If value <> _responsibleID Then
                    _responsibleID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Responsible Organization Unit ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ResponsibleOUID() As String
            Get
                ResponsibleOUID = _respOUID
            End Get
            Set(value As String)
                If value <> _respOUID Then
                    _respOUID = value
                    Me.IsChanged = True
                End If
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
                Matchcode = _matchcode
            End Get
            Set(value As String)
                If value <> _matchcode Then
                    _matchcode = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Part Id related to the deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PartID() As String
            Get
                PartID = _partID
            End Get
            Set(value As String)
                If _partID <> value Then
                    _partID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Change Reference ID 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ChangeReferenceID() As String
            Get
                ChangeReferenceID = _changerefID
            End Get
            Set(value As String)
                If _changerefID <> value Then
                    _changerefID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the type ID 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TypeID() As String
            Get
                TypeID = _typeid
            End Get
            Set(value As String)
                If _typeid <> value Then
                    _typeid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the wbs ID 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WBSID() As String
            Get
                WBSID = _wbsid
            End Get
            Set(value As String)
                If _wbsid <> value Then
                    _wbsid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the workpackage code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Workpackage() As String
            Get
                Workpackage = _workpackage
            End Get
            Set(value As String)
                If _workpackage <> value Then
                    _workpackage = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the wbs code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WBSCode() As String
            Get
                WBSCode = _wbscode
            End Get
            Set(value As String)
                If _wbscode <> value Then
                    _wbscode = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the function
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [function]() As String
            Get
                [function] = _function
            End Get
            Set(value As String)
                If _function <> value Then
                    _function = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ReadOnly Property MsglogTag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                MsglogTag = _msglogtag
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the blocking item reference ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BlockingItemID() As String
            Get
                BlockingItemID = _blockingitemID
            End Get
            Set(value As String)
                If _blockingitemID <> value Then
                    _blockingitemID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the comment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Comment = _comment
            End Get
            Set(value As String)
                _comment = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' gets the activity tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Activetag() As String
            Get
                If _activetag = "" Then
                    _activetag = GetUniqueTag()
                End If
                Activetag = _activetag
            End Get
        End Property
        ''' <summary>
        ''' gets  the config tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Configtag()
            Get
                If _configtag = "" Then
                    _configtag = GetUniqueTag()
                End If
                Configtag = _configtag
            End Get
        End Property

#End Region

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _uid & ConstDelimiter
        End Function


        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Deliverable)(silent:=silent)
            'Dim aFieldDesc As New ormFieldDescription
            'Dim primaryColumnNames As New Collection
            'Dim aTable As New ObjectDefinition


            'aFieldDesc.ID = ""
            'aFieldDesc.Parameter = ""
            'aFieldDesc.Relation = New String() {}
            'aFieldDesc.Aliases = New String() {}
            'aFieldDesc.Tablename = ConstTableID

            'With aTable
            '    .Create(ConstTableID)
            '    .Delete()

            '    '***
            '    '*** Fields
            '    '****

            '    'Type
            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "uid of the deliverable"
            '    aFieldDesc.ColumnName = constFNUid
            '    aFieldDesc.Aliases = New String() {"uid"}
            '    aFieldDesc.ID = "dlv1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    primaryColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "category"
            '    aFieldDesc.ColumnName = "cat"
            '    aFieldDesc.ID = "dlv12"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "deliverable id"
            '    aFieldDesc.ColumnName = "dlvid"
            '    aFieldDesc.ID = "dlv3"
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "precode"
            '    aFieldDesc.ColumnName = constFNMatchCode
            '    aFieldDesc.ID = "dlv4"
            '    aFieldDesc.Aliases = New String() {"c3"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "assycode"
            '    aFieldDesc.ColumnName = "dasy"
            '    aFieldDesc.ID = "dlv5"
            '    aFieldDesc.Aliases = New String() {"c4"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "deliverable revision"
            '    aFieldDesc.ColumnName = constFNRevision
            '    aFieldDesc.ID = "dlv6"
            '    aFieldDesc.Aliases = New String() {"c16"}
            '    aFieldDesc.Size = 20
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "first revision uid"
            '    aFieldDesc.ColumnName = constFNfuid
            '    aFieldDesc.ID = "dlv7"
            '    aFieldDesc.Aliases = New String() {"c21"}
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "change reference tag"
            '    aFieldDesc.ColumnName = "chref"
            '    aFieldDesc.ID = "dlv8"
            '    aFieldDesc.Aliases = New String() {"c20"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "format"
            '    aFieldDesc.ColumnName = "frmt"
            '    aFieldDesc.ID = "dlv9"
            '    aFieldDesc.Aliases = New String() {"c5"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "description"
            '    aFieldDesc.ColumnName = constFNDescription
            '    aFieldDesc.ID = "dlv10"
            '    aFieldDesc.Aliases = New String() {"c6"}
            '    aFieldDesc.Size = 255
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "responsible OU name"
            '    aFieldDesc.ColumnName = "respou"
            '    aFieldDesc.ID = "dlv11"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parts-id"
            '    aFieldDesc.ColumnName = constFNPartID
            '    aFieldDesc.ID = "dlv12"
            '    aFieldDesc.Aliases = New String() {"c10"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "type of deliverable"
            '    aFieldDesc.ColumnName = constFNTypeID
            '    aFieldDesc.ID = "dlv13"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "customer OU"
            '    aFieldDesc.ColumnName = constFNCustomerOU
            '    aFieldDesc.ID = "dlv15"
            '    aFieldDesc.Aliases = New String() {"c12"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "responsible"
            '    aFieldDesc.ColumnName = "resp"
            '    aFieldDesc.ID = "dlv16"
            '    aFieldDesc.Aliases = New String() {"c14"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "blocking item"
            '    aFieldDesc.ColumnName = "blitemid"
            '    aFieldDesc.ID = "dlv17"
            '    aFieldDesc.Aliases = New String() {"bs5"}
            '    aFieldDesc.Size = 50
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' cmt
            '    aFieldDesc.Datatype = otFieldDataType.Memo
            '    aFieldDesc.Title = "comments"
            '    aFieldDesc.ColumnName = "cmt"
            '    aFieldDesc.ID = "dlv18"
            '    aFieldDesc.Size = 0
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    '**** configtag
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "tag of config"
            '    aFieldDesc.ID = "dlv19"
            '    aFieldDesc.Aliases = New String() {"cnfl4"}
            '    aFieldDesc.ColumnName = "cnftag"
            '    aFieldDesc.Size = 100
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    '**** activeTag
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "tag of activitiy"
            '    aFieldDesc.ID = "dlv20"
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.ColumnName = "acttag"
            '    aFieldDesc.Size = 100
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '    ' msglogtag
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "message log tag"
            '    aFieldDesc.ColumnName = "msglogtag"
            '    aFieldDesc.Size = 255
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "deleted flag"
            '    aFieldDesc.ColumnName = ConstFNIsDeleted
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "deleted date"
            '    aFieldDesc.ColumnName = ConstFNDeletedOn
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 1
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 1 of condition"
            '    aFieldDesc.ColumnName = "param_txt1"
            '    aFieldDesc.Size = 255
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 2
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 2 of condition"
            '    aFieldDesc.ColumnName = "param_txt2"
            '    aFieldDesc.Size = 255
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 2
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 3 of condition"
            '    aFieldDesc.ColumnName = "param_txt3"
            '    aFieldDesc.Size = 255
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_num 1
            '    aFieldDesc.Datatype = otFieldDataType.Numeric
            '    aFieldDesc.Title = "parameter numeric 1 of condition"
            '    aFieldDesc.ColumnName = "param_num1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_num 2
            '    aFieldDesc.Datatype = otFieldDataType.Numeric
            '    aFieldDesc.Title = "parameter numeric 2 of condition"
            '    aFieldDesc.ColumnName = "param_num2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' parameter_num 2
            '    aFieldDesc.Datatype = otFieldDataType.Numeric
            '    aFieldDesc.Title = "parameter numeric 3 of condition"
            '    aFieldDesc.ColumnName = "param_num3"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_date 1
            '    aFieldDesc.Datatype = otFieldDataType.[Date]
            '    aFieldDesc.Title = "parameter date 1 of condition"
            '    aFieldDesc.ColumnName = "param_date1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_date 2
            '    aFieldDesc.Datatype = otFieldDataType.[Date]
            '    aFieldDesc.Title = "parameter date 2 of condition"
            '    aFieldDesc.ColumnName = "param_date2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' parameter_date 3
            '    aFieldDesc.Datatype = otFieldDataType.[Date]
            '    aFieldDesc.Title = "parameter date 3 of condition"
            '    aFieldDesc.ColumnName = "param_date3"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' parameter_flag 1
            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "parameter flag 1 of condition"
            '    aFieldDesc.ColumnName = "param_flag1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_flag 2
            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "parameter flag 2 of condition"
            '    aFieldDesc.ColumnName = "param_flag2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' parameter_flag 3
            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "parameter flag 3 of condition"
            '    aFieldDesc.ColumnName = "param_flag3"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    '***
            '    '*** TIMESTAMP
            '    '****
            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "last Update"
            '    aFieldDesc.ColumnName = ConstFNUpdatedOn
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.Relation = New String() {}
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "creation Date"
            '    aFieldDesc.ColumnName = ConstFNCreatedOn
            '    aFieldDesc.Aliases = New String() {}
            '    aFieldDesc.Relation = New String() {}
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' Index
            '    Call .AddIndex("PrimaryKey", primaryColumnNames, isprimarykey:=True)
            '    Dim deletedCollection As New Collection
            '    deletedCollection.Add(ConstFNDeletedOn)
            '    Call .AddIndex("deleted", deletedCollection, isprimarykey:=False)
            '    ' persist
            '    .Persist()
            '    ' change the database
            '    .AlterSchema()
            'End With

            'CreateSchema = True
            'Exit Function


        End Function

        ''' <summary>
        ''' Purge revisions of a deliverable
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Purge() As Boolean

            Dim otdbCol As Collection
            Dim aDelivTrack As New Track
            Dim aCurSchedule As New CurrentSchedule
            Dim aSchedule As New Schedule
            Dim aDocTarget As New Target

            ' only if loaded
            If IsLoaded Then
                ' delete other reference records
                'delete the tbldeliverabletracks
                otdbCol = aDocTarget.AllByUid(Me.Uid)
                If Not otdbCol Is Nothing Then
                    For Each aDocTarget In otdbCol
                        Call aDocTarget.Delete()
                    Next aDocTarget
                End If
                'delete the curschedule
                otdbCol = aCurSchedule.allByUID(Me.Uid)
                If Not otdbCol Is Nothing Then
                    For Each aCurSchedule In otdbCol
                        Call aCurSchedule.Delete()
                    Next aCurSchedule
                End If
                'delete the DocTarget
                otdbCol = aDelivTrack.AllByUID(Me.Uid)
                If Not otdbCol Is Nothing Then
                    For Each aDelivTrack In otdbCol
                        Call aDelivTrack.Delete()
                    Next aDelivTrack
                End If
                'delete the Schedule
                otdbCol = aSchedule.AllByUID(Me.Uid)
                If Not otdbCol Is Nothing Then
                    For Each aSchedule In otdbCol
                        Call aSchedule.Delete()
                    Next aSchedule
                End If

                'delete the  object itself
                Me.IsDeleted = Me.Record.Delete()
                If Me.IsDeleted Then
                    Me.Unload()
                End If
                Purge = Me.IsDeleted
                Exit Function
            End If
        End Function

        ''' <summary>
        ''' loads and infuse the deliverable by primary key from the data store
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(uid As Long) As Boolean
            Dim pkarray() As Object = {uid}
            Return MyBase.Inject(pkarray)
        End Function

#Region "Static"
        ''' <summary>
        ''' returns a collection of all deliverables (not deleted)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional justdeleted As Boolean = False, Optional domainID As String = "") As List(Of Deliverable)

            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            '** set the domain
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="all", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = ConstFNIsDeleted & " = @deleted "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & ConstTableID & "." & constFNUid & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                If justdeleted Then
                    aCommand.SetParameterValue(ID:="@deleted", value:=True)
                Else
                    aCommand.SetParameterValue(ID:="@deleted", value:=False)
                End If
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)


                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aNewDeliverable As New Deliverable
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewDeliverable) Then
                        aCollection.Add(item:=aNewDeliverable)
                    End If
                Next

                Return aCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.All")
                Return aCollection

            End Try

        End Function

        ''' <summary>
        ''' return a Collection of deliverables filtered by precode
        ''' </summary>
        ''' <param name="precode"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByMatchcode(ByVal matchcode As String, Optional domainID As String = "") As List(Of Deliverable)
            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            '** set the domain
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            '** build query
            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allbymcod", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted AND " & constFNMatchCode & "] = @mcod"
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & constFNUid & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@mcod", ColumnName:=constFNMatchCode, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@mcod", value:=matchcode)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)

                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aNewDeliverable As New Deliverable
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewDeliverable) Then
                        aCollection.Add(item:=aNewDeliverable)
                    End If
                Next

                Return aCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.AllByMatchCode")
                Return aCollection

            End Try

        End Function
        ''' <summary>
        ''' returns a collection of all uids of deliverables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllUIDs() As List(Of Long)
            Dim aCollection As New List(Of Long)
            Dim aStore As iormDataStore
            Dim value As Object
            Dim abostrophNecessary As Boolean
            Dim cvtvalue As Object

            '**
            aStore = GetTableStore(ConstTableID)
            Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="alluids", addAllFields:=False)
            If Not aCommand.Prepared Then
                aCommand.select = "DISTINCT " & constFNUid
                aCommand.Prepare()
            End If

            Dim aRecordCollection As List(Of ormRecord) = aCommand.RunSelect

            For Each aRecord As ormRecord In aRecordCollection
                value = aRecord.GetValue(1)
                aStore.Convert2ObjectData(constFNUid, invalue:=value, outvalue:=cvtvalue, abostrophNecessary:=abostrophNecessary)
                aCollection.Add(item:=cvtvalue)
            Next

            Return aCollection

        End Function
        ''' <summary>
        ''' return a collection of deliverables filtered by partid
        ''' </summary>
        ''' <param name="partid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByPnid(ByVal partid As String, Optional domainID As String = "") As List(Of Deliverable)
            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            '** set the domain
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allbypnid", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted AND [" & constFNPartID & "] = @pnid"
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & constFNUid & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@pnid", columnname:="pnid", tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@pnid", value:=partid)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)

                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aNewDeliverable As New Deliverable
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewDeliverable) Then
                        aCollection.Add(item:=aNewDeliverable)
                    End If
                Next

                Return aCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.AllByPNID")
                Return aCollection

            End Try

        End Function

        '****** allRevisionUIDsBy: "static" function to return a List of UIDS of Deliverables sorted by Revisions
        '******
        ''' <summary>
        ''' returns all revisions of a firstrevision including it
        ''' </summary>
        ''' <param name="firstrevisionUID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllRevisionUIDsBy(ByVal firstrevisionUID As Long) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore


            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allrvisionUIDsby", addAllFields:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = "[" & constFNUid & "], [" & constFNRevision & "],[" & constFNfuid & "]"
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted and ([" & constFNUid & "] = @uid or [" & constFNfuid & "]=@uid)"
                    aCommand.OrderBy = "[" & ConstTableID & "." & constFNUid & "], [" & ConstTableID & "." & constFNRevision & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@uid", columnname:="uid", tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@uid", value:=firstrevisionUID)

                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim anUID As Long = aRecord.GetValue("uid")
                    Dim aFUID As Long = aRecord.GetValue(constFNfuid)

                    If anUID = firstrevisionUID And aFUID <> 0 Then
                        Return New Collection ' drop all we found since we are not a first revision
                    End If

                    aCollection.Add(anUID)
                Next

                Return aCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.allrevisionUIDsby")
                Return aCollection

            End Try

        End Function
#End Region
        ''' <summary>
        ''' Load the related part object
        ''' </summary>
        ''' <returns>clsOTDBPart or nothing if load failed</returns>
        ''' <remarks></remarks>
        Public Function GetPart() As Part
            Dim pkarray() As Object = {Me.PartID}
            If _IsLoaded Then
                Return Part.Retrieve(Of Part)(pkarray)
            Else
                Return Nothing
            End If
        End Function

        '**** getthe Track
        '****
        Public Function GetTrack(Optional workspaceID As String = "", _
        Optional scheduleUID As Long = 0, _
        Optional scheduleUPDC As Long = 0, _
        Optional targetUPDC As Long = 0) As Track
            Dim aTrackDef As New Track
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aCurrTarget As New CurrentTarget

            If IsLoaded Or IsCreated Then
                If scheduleUPDC = 0 Then
                    ' get
                    aCurrSCHEDULE = Me.GetCurrSchedule(workspaceID:=workspaceID)
                    scheduleUPDC = aCurrSCHEDULE.UPDC
                End If
                If targetUPDC = 0 Then
                    If Not aCurrTarget.Inject(uid:=Me.Uid, workspaceID:=workspaceID) Then
                        targetUPDC = 0
                    Else
                        targetUPDC = aCurrTarget.UPDC
                    End If
                End If
                If scheduleUPDC > 0 Then
                    aTrackDef = New Track
                    If aTrackDef.Inject(Me.Uid, scheduleUID:=Me.Uid, scheduleUPDC:=scheduleUPDC, targetUPDC:=targetUPDC) Then
                        GetTrack = aTrackDef
                        Exit Function
                    End If
                End If
            End If

            GetTrack = Nothing
        End Function

        ''' <summary>
        ''' retrieve a collection of all used precodes
        ''' </summary>
        ''' <param name="list">collection to be filled</param>
        ''' <param name="silent"></param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Public Function Getmatchcodes(ByRef list As IEnumerable, Optional domainID As String = "") As Boolean
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="matchcoded", addAllFields:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = " DISTINCT [" & constFNMatchCode & "]"
                    aCommand.Where = ConstFNIsDeleted & " = @deleted"
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.OrderBy = "[" & ConstTableID & "." & constFNMatchCode & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim pcod As String = aRecord.GetValue(1)
                    aCollection.Add(pcod)
                Next

                list = aCollection
                Return True


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="Deliverable.GetPrecodes")
                list = aCollection
                Return False

            End Try

        End Function
        ''' <summary>
        ''' return the related current target object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCurrTarget(Optional ByVal workspaceID As String = "") As CurrentTarget
            Dim aCurrTarget As New CurrentTarget
            Dim aWorkspace As String

            If IsMissing(workspaceID) Then
                aWorkspace = CurrentSession.CurrentWorkspaceID
            Else
                aWorkspace = Trim(CStr(workspaceID))
            End If

            '*
            If _IsLoaded Or Me.IsCreated Then
                ' check if in workspaceID any data -> fall back to default (should be base)
                If aCurrTarget.Inject(Me.Uid, workspaceID:=aWorkspace) Then
                    GetCurrTarget = aCurrTarget
                    Exit Function
                Else
                    GetCurrTarget = Nothing
                    Exit Function
                End If
            End If

            ' return nothing
            GetCurrTarget = Nothing

        End Function
        ''' <summary>
        ''' retrieve the current schedule object
        ''' </summary>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns>the data object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetCurrSchedule(Optional ByVal workspaceID As String = "") As CurrentSchedule
            '*
            If Me.IsLoaded Or Me.IsCreated Then
                ' if no workspaceID -> Default workspaceID
                If workspaceID = "" Then
                    workspaceID = CurrentSession.CurrentWorkspaceID
                End If

                ' check if in workspaceID any data -> fall back to default (should be base)
                Return CurrentSchedule.Retrieve(UID:=Me.Uid, workspaceID:=workspaceID)
            End If

            ' return nothing
            Return Nothing

        End Function

        ''' <summary>
        ''' retrieves the active and curent schedule object for the deliverable 
        ''' </summary>
        ''' <param name="workspaceID">workspaceID id</param>
        ''' <returns>a scheduling object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetSchedule(Optional ByVal workspaceID As String = "") As Schedule
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            '*
            If _IsLoaded Or Me.IsCreated Then
                ' get
                Dim aCurrSCHEDULE As CurrentSchedule = Me.GetCurrSchedule(workspaceID:=workspaceID)
                ' load
                If aCurrSCHEDULE.IsLoaded AndAlso aCurrSCHEDULE.UPDC > 0 Then
                    Return Schedule.Retrieve(UID:=Me.Uid, updc:=aCurrSCHEDULE.UPDC)
                End If
            End If

            Return Nothing

        End Function
        ''' <summary>
        ''' retrieves the target object (most current)
        ''' </summary>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns>the data object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetTarget(Optional ByVal workspaceID As String = "") As Target
            Dim aCurrTarget As New CurrentTarget
            Dim aTarget As Target

            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            End If

            '*
            If _IsLoaded Or Me.IsCreated Then
                ' get
                aCurrTarget = Me.GetCurrTarget(workspaceID:=workspaceID)
                If aCurrTarget.IsLoaded Then
                    aTarget = New Target
                    ' load the current schedule
                    If aCurrTarget.UPDC > 0 Then
                        If aTarget.Inject(uid:=Me.Uid, updc:=aCurrTarget.UPDC) Then
                            GetTarget = aTarget
                            Exit Function
                        End If
                    End If
                End If
            End If

            GetTarget = Nothing
            Exit Function

        End Function

        '****** HACK: runXChange for Cartypes
        '******

        Public Function runCartypesXChange(ByRef MAPPING As Dictionary(Of Object, Object), _
        ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
        ByRef MSGLOG As ObjectLog) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aCarName As String
            Dim aCarNo As Integer

            Dim aChangeMember As New clsOTDBXChangeMember
            Dim aCartypes As New clsCartypes

            Dim anUID As Long

            Dim aCollection As New Collection
            Dim aFlag As Boolean


            Dim aDeliverable As New Deliverable

            Dim anObjectDef As New clsOTDBXChangeMember
            Dim anAttribute As New clsOTDBXChangeMember

            Dim aValue As Object

            Dim aTimestamp As Date

            If CHANGECONFIG.ProcessedDate <> ConstNullDate Then
                aTimestamp = CHANGECONFIG.ProcessedDate
            Else
                aTimestamp = Now
            End If

            '*** ObjectDefinition
            anObjectDef = CHANGECONFIG.ObjectByName("tblconfigs")

            ' set msglog
            'If MSGLOG Is Nothing Then
            '    If s_msglog Is Nothing Then
            '        Set s_msglog = New clsOTDBMessageLog
            '    End If
            '    Set MSGLOG = s_msglog
            '    MSGLOG.create (Me.msglogtag)
            'End If

            '** check on the min. required primary key uid
            aValue = CHANGECONFIG.GetMemberValue(ID:="UID", mapping:=MAPPING)
            If IsNull(aValue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("UID")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "UID", "UID", ConstTableID, CHANGECONFIG.Configname)
                    runCartypesXChange = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "UID", "UID", ConstTableID, CHANGECONFIG.Configname)
                    runCartypesXChange = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aValue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "UID", "UID", ConstTableID, CHANGECONFIG.Configname, aValue, "numeric")
                runCartypesXChange = False
                Exit Function
            Else
                anUID = CLng(aValue)
            End If

            '** load the deliverable
            If Not aDeliverable.Inject(uid:=anUID) Then
                Call MSGLOG.AddMsg("203", Nothing, Nothing, "UID", CHANGECONFIG.Configname, anUID)
                runCartypesXChange = False
                Exit Function
            Else
                '** load the cartypes
                aCartypes = aDeliverable.GetCartypes()
                If aCartypes Is Nothing And anObjectDef.XChangeCmd <> otXChangeCommandType.UpdateCreate Then
                    Call MSGLOG.AddMsg("203", Nothing, Nothing, "UID", CHANGECONFIG.Configname, anUID & " (cartype)")
                    runCartypesXChange = False
                    Exit Function
                End If
            End If

            '*** set the Attributes if these are milestone=compounds
            '***

            For Each anAttribute In CHANGECONFIG.AttributesByObjectName(objectname:="tblconfigs")
                ' get the value
                aCarName = anAttribute.Entryname.ToLower
                If aCarName Like "ct*" Then
                    aCarNo = CInt(Mid(aCarName, 3, 2))
                    aValue = CHANGECONFIG.GetMemberValue(changemember:=anAttribute, objectname:="tblconfigs", _
                                                         mapping:=MAPPING)
                    '*** set the cartype
                    If Not IsNull(aValue) And _
                    (anAttribute.XChangeCmd = otXChangeCommandType.Update Or anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate _
                    Or anAttribute.XChangeCmd = otXChangeCommandType.Duplicate) Then
                        If Not aValue Is Nothing Then

                            ' convert to DB
                            Call anAttribute.convertValue2DB(aValue, aValue, existingValue:=False)
                            ' save
                            If IsEmpty(aValue) Then aValue = 0
                            Call aCartypes.addCartypeAmountByIndex(aCarNo, aValue)

                        End If

                        ' read or overwrite
                    ElseIf anAttribute.XChangeCmd = otXChangeCommandType.Read Or IsNull(aValue) Then

                        aValue = aCartypes.getCarAmount(aCarNo)
                        Call anAttribute.convertValue4DB(aValue, aValue)

                        '** special
                        If aValue = 0 Then aValue = "-"

                        If MAPPING.ContainsKey(anAttribute.ordinal) Then
                            Call MAPPING.Remove(key:=anAttribute.ordinal)
                        End If
                        Call MAPPING.Add(key:=anAttribute.ordinal, value:=aValue)

                    End If
                End If    ' compound
            Next anAttribute

            '********* check on the action xchange command
            '*********

            If anObjectDef.XChangeCmd = otXChangeCommandType.Read Then
                '* otRead with Compounds can be handled by the standard
                '*
                runCartypesXChange = True
                Exit Function

            ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Update _
            Or anObjectDef.XChangeCmd = otXChangeCommandType.UpdateCreate _
            Or anObjectDef.XChangeCmd = otXChangeCommandType.Duplicate Then


                runCartypesXChange = aDeliverable.SetCartypes(aCartypes)
                Exit Function
            ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Delete Then
                '*** handle new entries on other objects such as Track ?!
                '    Debug.Assert False
            End If


            runCartypesXChange = True
        End Function
        '****** getCartypes of the Document
        '******
        Public Function GetCartypes(Optional ByVal uid As Long = 0) As clsCartypes
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object
            Dim aCartypes As New clsCartypes
            Dim i As Integer
            Dim amount As Integer
            Dim fieldname As String

            ' set the primaryKey
            ReDim pkarry(1)
            If uid = 0 Then
                pkarry(0) = Me.Uid
            Else
                pkarry(0) = uid
            End If

            aTable = GetTableStore("tblcartypes")
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                GetCartypes = Nothing
                Exit Function
            Else
                For i = 1 To aCartypes.getNoCars
                    fieldname = "ct" & String.Format(i, "0#")
                    amount = CInt(aRecord.GetValue(fieldname))
                    Call aCartypes.addCartypeAmountByIndex(i, amount)
                Next i
                GetCartypes = aCartypes
                Exit Function
            End If


        End Function

        '****** HACK:setCartypes : persist the Cartypes for this Deliverable
        '******
        Public Function SetCartypes(Cartypes As clsCartypes) As Boolean
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object

            Dim i As Integer
            Dim amount As Integer
            Dim fieldname As String


            If Not _IsLoaded Then
                SetCartypes = False
                Exit Function
            End If

            ' set the primaryKey
            ReDim pkarry(1)
            If Uid = 0 Then
                pkarry(0) = Me.Uid
            Else
                pkarry(0) = Uid
            End If

            aTable = GetTableStore("tblcartypes")
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                aRecord = New ormRecord
                aRecord.SetTable("tblcartypes", fillDefaultValues:=True)
                Call aRecord.SetValue("uid", Uid)
            End If
            'set
            For i = 1 To Cartypes.getNoCars
                fieldname = "ct" & String.Format(i, "0#")
                Call aRecord.SetValue(fieldname, Cartypes.getCarAmount(i))
            Next i
            ' save
            SetCartypes = aRecord.Persist(DateTime.Now)


        End Function
        ''' <summary>
        ''' retrieve maximum update count from the datastore
        ''' </summary>
        ''' <param name="max">the max to be set</param>
        ''' <param name="workspaceID">optional workspaceID</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetNewUID(ByRef max As Long, Optional domainID As String = "") As Boolean
            Dim aDomain As Domain
            Dim mymax As Long


            '** default domain
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID


            Try
                ' get
                Dim aStore As iormDataStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="getnewUid", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = "max([" & constFNUid & "])"
                    aCommand.Where = "[" & ConstFNDomainID & "] = @domain"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@domain", value:=domainID)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect
                aDomain = Domain.Retrieve(id:=domainID)

                If theRecords.Count > 0 Then
                    If Not IsNull(theRecords.Item(0).GetValue(1)) And IsNumeric(theRecords.Item(0).GetValue(1)) Then
                        mymax = CLng(theRecords.Item(0).GetValue(1))
                        If Not aDomain Is Nothing Then
                            If mymax >= (aDomain.MaxDeliverableUID - 10) Then
                                Call CoreMessageHandler(showmsgbox:=True, message:="Number range for domain ID ends", _
                                                      arg1:=domainID, messagetype:=otCoreMessageType.ApplicationWarning)
                            End If
                        End If
                    Else
                        If aDomain IsNot Nothing Then
                            mymax = aDomain.MinDeliverableUID
                        Else
                            GetNewUID = False
                        End If

                    End If
                    GetNewUID = True

                Else
                    If aDomain IsNot Nothing Then
                        mymax = aDomain.MinDeliverableUID
                    Else
                        GetNewUID = False
                    End If
                End If
                If GetNewUID Then
                    max = mymax
                End If
                Return GetNewUID
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, exception:=ex, subname:="Deliverable.getNewUID")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' create unique persistable object by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(Optional ByVal uid As Long = 0, Optional domainID As String = "", Optional workspaceID As String = "", Optional typeid As String = "") As Boolean
            Dim pkArray() As Object = {uid}

            '* defaults
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If typeid = "" Then typeid = CurrentSession.DefaultDeliverableTypeID

            ' get NEW UID
            If uid = 0 Then
                If Not Me.GetNewUID(uid, domainID:=domainID) Then
                    Call CoreMessageHandler(message:="could not generate new UID", subname:="Deliverable.create", _
                                            arg1:=uid, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If

            If MyBase.Create(pkArray, checkUnique:=True) Then
                ' set the primaryKey
                _uid = pkArray(0)
                _domainID = domainID
                _wspaceID = workspaceID
                _typeid = typeid
                Return True
            Else
                Return False
            End If

        End Function

        '**** createFirstRevision : add a FirstRevision
        '****
        ''' <summary>
        ''' createFirstRevision : add a FirstRevision
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="newRevision"></param>
        ''' <param name="persist"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateFirstRevision(Optional ByVal uid As Long = 0, _
        Optional ByVal newRevision As String = "", _
        Optional ByVal persist As Boolean = True) As Deliverable

            Dim newDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim aFirstSchedule As New Schedule
            Dim aFirstRevision As New Deliverable
            Dim aNewSchedule As New Schedule
            Dim aNewTarget As New Target
            Dim aValue As Object

            '****
            '****



            If Not newDeliverable.Create Then
                Call CoreMessageHandler(subname:="Deliverable.createFirstRevision", message:=" clone failed", arg1:=uid)
                CreateFirstRevision = Nothing
                Exit Function
            End If



            '*** add Revision
            '***
            If newRevision <> "" Then
                newDeliverable.Revision = newRevision
            Else
                newDeliverable.Revision = "0"

            End If

            '*** save
            If Me.Persist() Then
                newDeliverable.Persist()
            End If

            '*** Schedule initialize
            Call aNewSchedule.Create(uid:=newDeliverable.Uid, updc:=0, scheduletypeid:=CurrentSession.DefaultScheduleTypeID)
            aNewSchedule.workspaceID = CurrentSession.CurrentWorkspaceID
            aNewSchedule.Persist()

            '** currSchedule
            Dim anewCurrSchedule As New CurrentSchedule
            Call anewCurrSchedule.Create(newDeliverable.Uid)
            anewCurrSchedule.UPDC = 0
            anewCurrSchedule.WorkspaceID = CurrentSession.CurrentWorkspaceID
            anewCurrSchedule.Persist()

            '*** Targetarget
            Call aNewTarget.Create(newDeliverable.Uid, updc:=0)
            aNewTarget.workspaceID = CurrentSession.CurrentWorkspaceID
            aNewTarget.Persist()

            Dim anewCurrTarget As New CurrentTarget
            Call anewCurrTarget.Create(newDeliverable.Uid)
            anewCurrTarget.UPDC = 0
            anewCurrTarget.WorkspaceID = CurrentSession.CurrentWorkspaceID
            anewCurrTarget.Persist()

            '*** Track
            Dim aNewTrack As New Track
            aNewTrack.workspaceID = CurrentSession.CurrentWorkspaceID
            aNewTrack.Scheduletype = aTrack.Scheduletype
            Call aNewTrack.UpdateFromDeliverable(deliverable:=newDeliverable)
            aNewTrack.Persist()

            CreateFirstRevision = newDeliverable

        End Function
        '**** addRevision : clone the deliverable and add a new revision
        '****
        ''' <summary>
        ''' clones the deliverable and inserts a new revision
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="newRevision"></param>
        ''' <param name="persist"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddRevision(Optional ByVal UID As Long = 0, Optional ByVal newRevision As String = "", Optional ByVal persist As Boolean = True) As Deliverable

            Dim newDeliverable As Deliverable
            Dim aTrack As Track
            Dim aFirstSchedule As New Schedule
            Dim aFirstRevision As New Deliverable
            Dim aNewSchedule As New Schedule
            Dim aNewTarget As New Target

            '****
            '****
            If Not _IsLoaded And Not Me.IsCreated Then
                AddRevision = Nothing
                Exit Function
            End If

            newDeliverable = Me.Clone(UID)
            If newDeliverable Is Nothing Then
                Call CoreMessageHandler(subname:="Deliverable.addRevision", message:=" clone failed", arg1:=UID)
                AddRevision = Nothing
                Exit Function
            End If

            '** add the first revision
            If Me.FirstRevisionUID = 0 Then
                newDeliverable.FirstRevisionUID = Me.Uid
                aFirstSchedule = Me.GetSchedule
                aFirstRevision = Me
            Else
                newDeliverable.FirstRevisionUID = Me.FirstRevisionUID
                If aFirstRevision.Inject(Me.FirstRevisionUID) Then
                    aFirstSchedule = aFirstRevision.GetSchedule
                End If
            End If

            '*** add Revision
            '***
            If newRevision <> "" Then
                newDeliverable.Revision = newRevision
            Else
                If Me.Revision <> "" Then
                    newDeliverable.Revision = UCase(Chr(Asc(Mid(Me.Revision, 1, 1)) + 1))
                    Me.Revision = "0"
                    If persist Then
                        Me.Persist()
                    End If
                Else
                    newDeliverable.Revision = "A"
                End If
            End If

            '*** save
            If persist Then
                newDeliverable.Persist()
            End If


            '**** hack new cartypes
            Me.CloneCartypes(newDeliverable.Uid)

            '*** create all the related Objects
            aTrack = Me.GetTrack
            If Not aTrack Is Nothing AndAlso aTrack.IsLoaded Then

                '*** Schedule
                Call aNewSchedule.Create(uid:=newDeliverable.Uid, updc:=0, scheduletypeid:=aTrack.Scheduletype)
                aNewSchedule.workspaceID = aFirstSchedule.workspaceID

                '** hack
                Call aNewSchedule.SetMilestone("bp80", aFirstSchedule.GetMilestoneValue("bp10"))
                aNewSchedule.Persist()

                '** currSchedule
                Dim anewCurrSchedule As New CurrentSchedule
                Call anewCurrSchedule.Create(newDeliverable.Uid)
                anewCurrSchedule.UPDC = 0
                anewCurrSchedule.WorkspaceID = aFirstSchedule.workspaceID
                anewCurrSchedule.Persist()

                '*** Targetarget
                Call aNewTarget.Create(newDeliverable.Uid, updc:=0)
                aNewTarget.workspaceID = aFirstSchedule.workspaceID
                aNewTarget.Persist()

                Dim anewCurrTarget As New CurrentTarget
                Call anewCurrTarget.Create(newDeliverable.Uid)
                anewCurrTarget.UPDC = 0
                anewCurrTarget.WorkspaceID = aFirstSchedule.workspaceID
                anewCurrTarget.Persist()

                '*** Track
                Dim aNewTrack As New Track
                aNewTrack.workspaceID = aFirstSchedule.workspaceID
                aNewTrack.Scheduletype = aTrack.Scheduletype
                Call aNewTrack.UpdateFromDeliverable(deliverable:=newDeliverable)
                aNewTrack.Persist()
            End If



            AddRevision = newDeliverable

        End Function

        '****** getCartypes of the Document
        '******
        Public Function CloneCartypes(ByVal newUID As Long) As clsCartypes
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object
            Dim aCartypes As New clsCartypes
            Dim i As Integer
            Dim amount As Integer
            Dim fieldname As String

            ' set the primaryKey
            ReDim pkarry(1)
            pkarry(0) = Me.Uid


            aTable = GetTableStore("tblcartypes")
            aRecord = aTable.GetRecordByPrimaryKey(pkarry)

            If aRecord Is Nothing Then
                CloneCartypes = Nothing
                Exit Function
            Else
                ' clone it
                Call aRecord.SetValue("uid", newUID)
                aRecord.Persist()
            End If
        End Function
        ''' <summary>
        ''' Clone the object with its primary key array. if {uid} = {0} generate a new uid
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <returns>the new object or nothing</returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkArray() As Object) As Deliverable Implements iotCloneable(Of Deliverable).Clone
            '*** now we copy the object
            Dim aNewObject As New Deliverable

            '* must be loaded
            If Not IsLoaded And Not IsCreated Then
                Return Nothing
            End If

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Return Nothing
                End If
            End If
            '* update the record
            If Not MyBase.Feed() Then
                Return Nothing
            End If
            '* get new uid
            If pkArray(0) Is Nothing OrElse pkArray(0) = 0 Then
                If Not Me.GetNewUID(pkArray(0), domainID:=Me.DomainID) Then
                    Call CoreMessageHandler(message:=" couldnot create unique primary key values - couldnot clone", arg1:=pkArray, _
                                            tablename:=TableID, entryname:="uid", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
            End If

            '** clone it
            aNewObject = Me.Clone(Of Deliverable)(pkArray)
            If Not aNewObject Is Nothing Then
                aNewObject.Record.SetValue(constFNUid, pkArray(0))
                aNewObject._uid = pkArray(0)
            End If

            Return aNewObject
        End Function
        ''' <summary>
        ''' Clone the deliverable
        ''' </summary>
        ''' <param name="UID">new uid If 0 then generate a new uid</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(Optional ByVal uid As Long = 0) As Deliverable
            Return Me.Clone({uid})
        End Function
    End Class ''' <summary>
End Namespace
