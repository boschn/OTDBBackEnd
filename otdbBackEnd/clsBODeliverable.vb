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
Imports OnTrack.Calendar
Imports OnTrack.Commons

Namespace OnTrack.Deliverables


    '************************************************************************************
    '***** CLASS CurrentTarget is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' Current target object points to the current clsOTDBDeliverableTarget 
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=CurrentTarget.ConstObjectID, description:="reference of the current target per workspace", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
    Public Class CurrentTarget
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of CurrentTarget)

        Public Const ConstObjectID = "CurrentTarget"
        '** Schema Table
        <ormSchemaTable(Version:=3)> Public Const ConstTableID = "tblCurrTargets"

        '** PrimaryKey
        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, primarykeyordinal:=1, _
                       useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNWorkspace = Schedule.ConstFNWorkspaceID

        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, primarykeyordinal:=2, _
                        useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
                        XID:="CDT1", aliases:={"UID"})> Public Const ConstFNUid = Deliverable.constFNUid

        '** other columns
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
           title:="Revision", description:="revision of the target", XID:="T9")> Public Const ConstFNRevision = "rev"
        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=0, dbdefaultvalue:="0", _
         title:="UpdateCount", description:="update number of the target", XID:="T10")> Public Const ConstFNUpdc = "updc"
        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
          title:="is active", description:="is the target active", XID:="DT4")> Public Const ConstFNIsActive = "isactive"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
              useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** mappings
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String = ""
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long    ' UPDC of target
        <ormEntryMapping(EntryName:=ConstFNIsActive)> Private _isActive As Boolean



#Region "Properties"
        ''' <summary>
        ''' returns the deliverable UID
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
        ''' returns the workspace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property WorkspaceID() As String
            Get
                WorkspaceID = _workspace
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the revision of the target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Revision() As String
            Get
                Return _rev
            End Get
            Set(value As String)
                SetValue(ConstFNRevision, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the updc of the target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UPDC() As Long
            Get
                UPDC = _updc
            End Get
            Set(value As Long)
                SetValue(ConstFNUpdc, value)
            End Set
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
                Call CoreMessageHandler(exception:=ex, subname:="CurrentTarget.AllByUID")
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
                Call CoreMessageHandler(exception:=ex, subname:="CurrentTarget.AllByWorkspace", arg1:=workspaceID)
                Return aCollection
            End Try

        End Function

        ''' <summary>
        ''' Loads and infuses a Current Target dependent on the workspaceID
        ''' </summary>
        ''' <param name="uid">deliverable uid</param>
        ''' <param name="workspaceID">the workspaceID to look into - default workspaceID used</param>
        ''' <returns>true if successful</returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal uid As Long, Optional ByVal workspaceID As String = "") As CurrentTarget
            Dim aWS As Object

            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim aWSObj As Workspace = Workspace.Retrieve(id:=workspaceID)
            '*
            If aWSObj Is Nothing Then
                Call CoreMessageHandler(message:="Can't load workspaceID definition", subname:="CurrentTarget.Retrieve", arg1:=workspaceID)
                Return Nothing
            End If

            ' check now the stack
            For Each aWS In aWSObj.FCRelyingOn
                ' check if in workspaceID any data -> fall back to default (should be base)
                Dim aCurrentTarget = CurrentTarget.RetrieveUniqueBy(uid:=uid, workspaceID:=aWS)
                If aCurrentTarget IsNot Nothing Then
                    Return aCurrentTarget
                End If
            Next aWS

            Return Nothing
        End Function


        ''' <summary>
        ''' load a unique current Target by its primary keys
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveUniqueBy(ByVal uid As Long, ByVal workspaceID As String) As CurrentTarget
            Dim pkarry() As Object = {workspaceID, uid}
            Return ormDataObject.Retrieve(Of CurrentTarget)(pkarry)
        End Function
        ''' <summary>
        ''' create a current Target by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal uid As Long, Optional ByVal workspaceID As String = "", Optional ByVal domainID As String = "") As CurrentTarget
            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim pkarray() As Object = {workspaceID, uid}
            Return ormDataObject.CreateDataObject(Of CurrentTarget)(pkarray, checkUnique:=True)
        End Function

    End Class

    ''' <summary>
    ''' target object for the deliverable class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=OnTrack.Deliverables.Target.ConstObjectID, description:="target definition per workspace of a deliverable e.g. date to be delivered", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
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

        <ormObjectEntry(typeid:=otDataType.Long, defaultValue:="0", primaryKeyordinal:=2, _
            description:="update count of the target date", title:="Update count", XID:="DT2", aliases:={"UPDC"})> Public Const constFNUpdc = "updc"

        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            Description:="workspaceID ID of the schedule")> Public Const ConstFNWorkspace = Schedule.ConstFNWorkspaceID

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
            description:="current target date", title:="target date", XID:="DT6", aliases:={"T2"})> Public Const constFNTarget = "targetdate"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
            description:="previous target date", title:="previous target date", XID:="DT5", aliases:={"T1"})> Public Const constFNPrevTarget = "pvtd"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, title:="target revision", Description:="revision of the target", _
           XID:="DT4", aliases:={"t9"}, isnullable:=True)> Public Const ConstFNRevision = "rev"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
          description:="target change timestamp", title:="target change", XID:="DT7", aliases:={"A6"})> Public Const constFNTargetChanged = "tchg"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
          title:="No Target", description:="no target by intention", XID:="DT2")> Const ConstFNNoTarget = "notarget"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
          title:="Type", description:="type of the target", XID:="DT3")> Const ConstFNType = "typeid"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
           title:="Responsible OrgUnit", description:=" organization unit responsible for the target", XID:="DT5")> Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, isnullable:=True, _
            title:="Responsible Person", description:="responsible person for the target", XID:="DT6")> Public Const constFNResp = "resp"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
            title:="Comment", Description:="comment of the target", XID:="DT7", isnullable:=True)> Public Const ConstFNComment = "cmt"

        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID


        '*** variables
        <ormEntryMapping(EntryName:=constFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=constFNUpdc)> Private _updc As Long

        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=constFNTarget)> Private _targetdate As Date?
        <ormEntryMapping(EntryName:=constFNPrevTarget)> Private _prevTarget As Date?
        <ormEntryMapping(EntryName:=constFNTargetChanged)> Private _TargetChangedDate As Date?
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""
        <ormEntryMapping(EntryName:=ConstFNNoTarget)> Private _notargetByItention As Boolean
        <ormEntryMapping(EntryName:=ConstFNType)> Private _typeid As String
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOU As String
        <ormEntryMapping(EntryName:=constFNResp)> Private _resp As String
        <ormEntryMapping(EntryName:=ConstFNComment)> Private _cmt As String
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
                Return _uid
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
                Return _updc
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the Target Date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Target() As Date?
            Get
                Return _targetdate
            End Get
            Set(value As Date?)
                SetValue(constFNTarget, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the previous target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrevTarget() As Date?
            Get
                Return _prevTarget
            End Get
            Set(value As Date?)
                SetValue(constFNPrevTarget, value)
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
                SetValue(constFNResp, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the resp OU.
        ''' </summary>
        ''' <value>The resp OU.</value>
        Public Property ResponsibleOU() As String
            Get
                Return _respOU
            End Get
            Set(value As String)
                SetValue(constFNRespOU, value)
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
                SetValue(ConstFNComment, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the timestamp of the  target date (changed on)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ChangedDate() As Date?
            Get
                ChangedDate = _TargetChangedDate
            End Get
            Set(value As Date?)
                SetValue(constFNTargetChanged, value)
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
        ''' <summary>
        ''' gets or sets the Workspace ID of the Target
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
        Public Overloads Shared Function Create(ByVal uid As Long, ByVal updc As Long) As Target
            Dim pkarray() As Object = {uid, updc}
            Return ormDataObject.CreateDataObject(Of Target)(pkarray, checkUnique:=True)
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
        Public Shared Function Retrieve(uid As Long, updc As Long) As Target
            Dim pkarray() As Object = {uid, updc}
            Return ormDataObject.Retrieve(Of Target)(pkarray)
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
            If workspaceID = "" Then
                If (Me.IsLoaded Or Me.IsCreated) AndAlso Me.WorkspaceID <> "" Then
                    workspaceID = Me.WorkspaceID
                Else
                    workspaceID = CurrentSession.CurrentWorkspaceID
                End If

            Else
                workspaceID = CStr(workspaceID)
            End If


            '** if UID is not provided than do use this TargetObject
            If UID = 0 Then
                If Not Me.IsLoaded And Not Me.IsCreated Then
                    PublishNewTarget = False
                    Exit Function
                End If

                anOldTarget = Me
                anUID = anOldTarget.UID
                anUPDC = Me.UPDC
                aCurrTarget = CurrentTarget.Retrieve(uid:=anUID, workspaceID:=workspaceID)
                If aCurrTarget Is Nothing Then
                    aCurrTarget = CurrentTarget.Create(uid:=anUID, workspaceID:=workspaceID)
                End If
                '*** only if loaded and not created get an new updc key and clone !
                If anOldTarget.IsLoaded Then
                    anUPDC = 0   ' increase by clone
                    ' clone
                    aNewTarget = anOldTarget.Clone(uid:=anUID, updc:=anUPDC)
                    aNewTarget.WorkspaceID = workspaceID
                ElseIf anOldTarget.IsCreated Then
                    aNewTarget = anOldTarget
                    aNewTarget.WorkspaceID = workspaceID
                End If
                '** if UID is provided than load oldTargetObject or create Target
            Else
                '** load the current UID of the current Target object
                aCurrTarget = CurrentTarget.Retrieve(anUID, workspaceID)
                If aCurrTarget IsNot Nothing Then
                    anUPDC = aCurrTarget.UPDC
                Else
                    Call aCurrTarget.Create(uid:=anUID, workspaceID:=workspaceID)
                    anUPDC = 1
                End If

                ' no Target exists ?!
                anOldTarget = Me.Retrieve(anUID, anUPDC)
                If anOldTarget IsNot Nothing Then
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
        Public Function RunXPreCheck(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean Implements iotXChangeable.RunXPreCheck

        End Function
        ''' <summary>
        ''' run the XChange on the Deliverable Target for an Envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean Implements iotXChangeable.RunXChange

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
                                        subname:="clsOTDBDeliverableTarget.Clone", messagetype:=otCoreMessageType.InternalError, tablename:=PrimaryTableID)
                Return Nothing
            End If
            If pkarray.Length = 1 OrElse pkarray(1) Is Nothing OrElse pkarray(0) = 0 Then
                If Not Me.PrimaryTableStore.CreateUniquePkValue(pkarray) Then
                    Call CoreMessageHandler(message:="failed to create an unique primary key value", arg1:=pkarray, _
                                            subname:="clsOTDBDeliverableTarget.Clone", messagetype:=otCoreMessageType.InternalError, tablename:=PrimaryTableID)
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


    ''' <summary>
    ''' deliverable track class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=Track.ConstObjectID, description:="tracking status of a deliverable per target and schedule", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> Public Class Track
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable
        Implements iotCloneable(Of Track)


        Public Const ConstObjectID = "Track"
        '** Table
        <ormSchemaTable(version:=2)> Public Const ConstTableID = "tblDeliverableTracks"
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

        ''' <summary>
        ''' foreign key
        ''' </summary>
        ''' <remarks></remarks>
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
            XID:="DTR9", isnullable:=True)> Public Const ConstFNMSIDDelivered = "msfinid"
        <ormObjectEntry(typeid:=otDataType.Date, title:="current forecast", Description:="forecast date for deliverable delivered", _
            XID:="DTR10", isnullable:=True)> Public Const ConstFNForecast = "fcdate"
        <ormObjectEntry(typeid:=otDataType.Date, title:="current target", Description:="target date for deliverable", _
            XID:="DTR11", isnullable:=True)> Public Const ConstFNCurTargetDate = "targetdate"

        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNlcstatus, _
            XID:="DTR12", aliases:={"SC7"}, isnullable:=True)> Public Const ConstFNLCStatus = Schedule.ConstFNlcstatus
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNpstatus, _
            XID:="DTR13", aliases:={"SC8"}, isnullable:=True)> Public Const ConstFNPStatus = Schedule.ConstFNpstatus

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, title:="Synchro status", Description:="schedule synchro status", _
            XID:="DTR14", isnullable:=True)> Public Const ConstFNSyncStatus = "sync"
        <ormObjectEntry(typeid:=otDataType.Date, title:="Synchro check date", Description:="date of last synchro check status", _
            XID:="DTR15", isnullable:=True)> Public Const ConstFNSyncDate = "syncchkon"
        <ormObjectEntry(typeid:=otDataType.Date, title:="Going Alive Date", Description:="date of schedule going alive", _
           XID:="DTR16", isnullable:=True)> Public Const ConstFNGoingAliveDate = "goal"
        <ormObjectEntry(typeid:=otDataType.Bool, title:="Delivered", Description:="True if deliverable is delivered", _
          XID:="DTR17", isnullable:=True)> Public Const ConstFNIsFinished = "isfinished"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Blocking Item Reference", description:="Blocking Item Reference id for the deliverable", XID:="DTR18", aliases:={"DLV17"})> _
        Public Const constFNBlockingItemReference = Deliverable.constFNBlockingItemReference
        <ormObjectEntry(typeid:=otDataType.Date, title:="Delivery Date", Description:="date for deliverable to be delivered / finished", _
          XID:="DTR19", isnullable:=True)> Public Const ConstFNFinishedOn = "finish"

        <ormObjectEntry(typeid:=otDataType.Long, title:="Forecast Gap", Description:="gap in working days between forecast and target", _
         XID:="DTR20")> Public Const constFNFCGap = "fcgap"
        <ormObjectEntry(typeid:=otDataType.Long, title:="BaseLine Gap", Description:="gap in working days between forecast and target", _
         XID:="DTR21")> Public Const constFNBLGap = "blgap"
        <ormObjectEntry(typeid:=otDataType.Date, title:="Schedule Change Date", Description:="forecast last changed on", _
          XID:="DTR23")> Public Const constFNFcChanged = "fcchanged"
        <ormObjectEntry(typeid:=otDataType.Date, title:="Baseline Delivery Date", Description:="delivery date from the baseline", _
          XID:="DTR24")> Public Const ConstFNBaselineFinish = "basefinish"
        <ormObjectEntry(typeid:=otDataType.Bool, title:="Schedule Frozen", Description:="True if schedule is frozen / a baseline exists", _
         XID:="DTR25", aliases:={"SC6"})> Public Const constFNIsFrozen = Schedule.ConstFNisfrozen
        <ormObjectEntry(typeid:=otDataType.Long, title:="Schedule UpdateCount", description:="update count of the schedule", _
            XID:="DTR26", aliases:={"SC17"})> Public Const constFNBaselineUPDC = Schedule.ConstFNBlUpdc

        <ormObjectEntry(typeid:=otDataType.Date, title:="Baseline Reference Date", Description:="reference date for baseline", _
         XID:="DTR27", isnullable:=True)> Public Const ConstFNBaseLineFrom = Schedule.ConstFNBlDate


        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** MAPPING
        <ormEntryMapping(EntryName:=constFNDeliverableUid)> Private _deliverableUID As Long
        <ormEntryMapping(EntryName:=constFNTargetUpdc)> Private _targetUPDC As Long
        <ormEntryMapping(EntryName:=constFNScheduleUid)> Private _scheduleUID As Long
        <ormEntryMapping(EntryName:=constFNScheduleUpdc)> Private _scheduleUPDC As Long

        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspaceID As String
        <ormEntryMapping(EntryName:=ConstFNMSIDDelivered)> Private _MSIDFinish As String
        <ormEntryMapping(EntryName:=ConstFNForecast)> Private _currFC As Date?
        <ormEntryMapping(EntryName:=ConstFNCurTargetDate)> Private _currTarget As Date?
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String
        <ormEntryMapping(EntryName:=ConstFNLCStatus)> Private _FCLCStatus As String
        <ormEntryMapping(EntryName:=ConstFNTypeid)> Private _scheduletype As String
        <ormEntryMapping(EntryName:=ConstFNScheduleRevision)> Private _ScheduleRevision As String
        <ormEntryMapping(EntryName:=ConstFNTargetRevision)> Private _TargetRevision As String
        <ormEntryMapping(EntryName:=ConstFNGoingAliveDate)> Private _GoingAliveDate As Date?
        <ormEntryMapping(EntryName:=ConstFNBaselineFinish)> Private _BaseLineFinishDate As Date?
        <ormEntryMapping(EntryName:=ConstFNBaseLineFrom)> Private _BaseLineFromDate As Date?
        <ormEntryMapping(EntryName:=constFNFcChanged)> Private _FClastchangeDate As Date?
        <ormEntryMapping(EntryName:=constFNIsFrozen)> Private _isFrozen As Boolean
        <ormEntryMapping(EntryName:=ConstFNFinishedOn)> Private _finishedOn As Date?
        <ormEntryMapping(EntryName:=ConstFNIsFinished)> Private _isFinished As Boolean
        <ormEntryMapping(EntryName:=constFNBaselineUPDC)> Private _BaselineUPDC As Long?
        <ormEntryMapping(EntryName:=ConstFNSyncStatus)> Private _SyncStatus As String
        <ormEntryMapping(EntryName:=ConstFNPStatus)> Private _pstatus As String
        <ormEntryMapping(EntryName:=ConstFNSyncDate)> Private _syncFrom As Date?
        <ormEntryMapping(EntryName:=constFNFCGap)> Private _FCgapToTarget As Long?
        <ormEntryMapping(EntryName:=constFNBLGap)> Private _BaselineGapToTarget As Long?

        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private s_msglogtag As String


        '********* dynamic
        Private _schedule As New Schedule
        Private _dlvTarget As New Target
        Private _deliverable As New Deliverable

#Region "Properties"

        ''' <summary>
        ''' gets the uid of the deliverable to be tracked
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DeliverableUID() As Long
            Get
                Return _deliverableUID
            End Get

        End Property
        ''' <summary>
        ''' gets the target updc of the target to be tracked
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TargetUPDC() As Long
            Get
                Return _targetUPDC
            End Get
        End Property
        ''' <summary>
        ''' gets the schedule uid to be tracked
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ScheduleUID() As Long
            Get
                Return _scheduleUID
            End Get

        End Property
        ''' <summary>
        ''' gets the updc of the schedule to be tracked
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ScheduleUPDC() As Long
            Get
                ScheduleUPDC = _scheduleUPDC
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the workspace id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property workspaceID() As String
            Get
                Return _workspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspace, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Milestone ID which finishes the Tracking
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MSIDFinish() As String
            Get
                Return _MSIDFinish
            End Get
            Set(value As String)
                SetValue(ConstFNMSIDDelivered, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the schedule type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Scheduletype() As String
            Get
                Return _scheduletype
            End Get
            Set(value As String)
                SetValue(ConstFNTypeid, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the current forecast
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CurrentForecast As Date?
            Get
                Return _currFC
            End Get
            Set(value As Date?)
                SetValue(ConstFNForecast, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the finished dated
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FinishedOn() As Date?
            Get
                Return _finishedOn
            End Get
            Set(value As Date?)
                SetValue(ConstFNFinishedOn, value)
            End Set
        End Property
        ''' <summary>
        ''' get or sets the CurrentTarget Date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CurrentTargetDate As Date?
            Get
                Return _currTarget
            End Get
            Set(value As Date?)
                SetValue(ConstFNCurTargetDate, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the current Gap to Target in units
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property GAPToTarget() As Long?
            Get
                Return _FCgapToTarget
            End Get
            Set(value As Long?)
                SetValue(constFNFCGap, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Gap from Baseline To Target in units
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaselineGAPToTarget() As Long?
            Get
                Return _BaselineGapToTarget
            End Get
            Set(value As Long?)
                SetValue(constFNBLGap, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Forecast changed date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForecastChangedOn() As Date
            Get
                Return _FClastchangeDate
            End Get
            Set(value As Date)
                SetValue(constFNFcChanged, value)
            End Set
        End Property
        ''' <summary>
        ''' get or sets the forecast lifecycle status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FCLCStatus() As String
            Get
                Return _FCLCStatus
            End Get
            Set(value As String)
                SetValue(ConstFNLCStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the process status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessStatus() As String
            Get
                Return _pstatus
            End Get
            Set(value As String)
                SetValue(ConstFNPStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the schedule revision
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ScheduleRevision() As String
            Get
                Return _ScheduleRevision
            End Get
            Set(value As String)
                SetValue(ConstFNScheduleRevision, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the target revision
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TargetRevision() As String
            Get
                Return _ScheduleRevision
            End Get
            Set(value As String)
                SetValue(ConstFNTargetRevision, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the blocking item
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BlockingItemID() As String
            Get
                Return _blockingitemID
            End Get
            Set(value As String)
                SetValue(constFNBlockingItemReference, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the frozen schedule flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsFrozen() As Boolean
            Get
                Return _isFrozen
            End Get
            Set(value As Boolean)
                SetValue(constFNIsFrozen, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the isfinished flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsFinished() As Boolean
            Get
                Return _isFinished
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsFinished, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the baseline updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaseLineUPDC() As Long?
            Get
                BaseLineUPDC = _BaselineUPDC
            End Get
            Set(value As Long?)
                If value <> _BaselineUPDC Then
                    _BaselineUPDC = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the baseline finish date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaseLineFinishDate() As Date?
            Get
                Return _BaseLineFinishDate
            End Get
            Set(value As Date?)
                SetValue(ConstFNBaselineFinish, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the baseline From Date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaseLineFinishDateFrom() As Date?
            Get
                Return _BaseLineFromDate
            End Get
            Set(value As Date?)
                SetValue(ConstFNBaseLineFrom, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the going alive date of the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property GoingAliveDate() As Date?
            Get
                Return _GoingAliveDate
            End Get
            Set(value As Date?)
                SetValue(ConstFNGoingAliveDate, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the synchronization status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SynchronizationStatus() As String
            Get
                Return _SyncStatus
            End Get
            Set(value As String)
                SetValue(ConstFNSyncStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the synchronization date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SychonizationDate() As Date
            Get
                Return _syncFrom
            End Get
            Set(value As Date)
                SetValue(ConstFNSyncDate, value)
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


#End Region


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
                                Call aTarget.PublishNewTarget(NewTargetDate:=constNullDate, workspaceID:=aTrack.workspaceID, UID:=aDeliverable.Uid)
                                aTarget.Revision = aTrack.TargetRevision
                                aTarget.Target = aTrack.CurrentTargetDate
                                aTarget.WorkspaceID = aTrack.workspaceID
                                aTarget.Persist()

                            End If

                            ' update the forecast
                            If aSchedule.IsLoaded Or aSchedule.IsCreated Then
                                If aSchedule.HasMilestone(aTrack.MSIDFinish, hasData:=True) Then
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
                                        aTrack.FinishedOn = constNullDate
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
        Public Overloads Shared Function Create(ByVal deliverableUID As Long, _
                                ByVal scheduleUID As Long, _
                                ByVal scheduleUPDC As Long, _
                                ByVal targetUPDC As Long) As Track
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            Return ormDataObject.CreateDataObject(Of Track)(pkarray, checkUnique:=True)
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
        Public Overloads Shared Function Retrieve(ByVal deliverableUID As Long, ByVal scheduleUID As Long, ByVal scheduleUPDC As Long, ByVal targetUPDC As Long) As Track
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            Return ormDataObject.Retrieve(Of Track)(pkarray)
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
                If _dlvTarget Is Nothing Then
                    UpdateFromTarget = False
                    Exit Function
                ElseIf Not _dlvTarget.IsCreated And Not _dlvTarget.IsLoaded Then
                    _dlvTarget = Nothing
                    UpdateFromTarget = False
                    Exit Function
                Else
                    aNewTarget = _dlvTarget
                    dlvUID = aNewTarget.UID
                    tUPDC = aNewTarget.UPDC
                End If
            Else
                aNewTarget = target
                dlvUID = target.UID
                tUPDC = target.UPDC
            End If
            ' set the objects
            aCurrSCHEDULE = _deliverable.GetCurrSchedule(workspaceID:=aWorkspace)
            If aCurrSCHEDULE Is Nothing Then
                sUID = 0
                sUPDC = 0
                _schedule = Nothing
            Else
                sUID = aCurrSCHEDULE.UID
                sUPDC = aCurrSCHEDULE.UPDC
                aSchedule = Schedule.Retrieve(UID:=sUID, updc:=sUPDC)
                If aSchedule IsNot Nothing Then
                    aNewSchedule = aSchedule
                Else
                    _schedule = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsAlive(throwError:=False) Then
                If Not Me.Create({dlvUID, sUID, sUPDC, tUPDC}) Then
                    Call Me.Inject({dlvUID, sUID, sUPDC, tUPDC})
                End If
            End If

            '**** create -> init
            _schedule = aNewSchedule
            _dlvTarget = aNewTarget

            With Me
                .workspaceID = aWorkspace
                .TargetRevision = _dlvTarget.Revision
                .CurrentTargetDate = _dlvTarget.Target

                ' schedule
                .ScheduleRevision = _schedule.Revision
                .IsFrozen = _schedule.IsFrozen
                .IsFinished = _schedule.IsFinished
                If _schedule.HasMilestoneDate("bp10") Then
                    .FinishedOn = _schedule.GetMilestoneValue("bp10")
                Else
                    .FinishedOn = constNullDate
                End If
                .MSIDFinish = "bp9"
                If _schedule.HasMilestoneDate(.MSIDFinish) Then
                    .CurrentForecast = _schedule.GetMilestoneValue(.MSIDFinish)
                Else
                    .CurrentForecast = constNullDate
                End If
                If checkGAP Then .CheckOnGap()
                If _schedule.IsBaseline Then
                    .BaseLineFinishDate = _schedule.GetMilestoneValue(.MSIDFinish)
                    .BaseLineFinishDateFrom = _schedule.CreatedOn
                    .BaseLineUPDC = _schedule.Updc
                    If checkGAP Then .CheckOnBaselineGap()
                End If
                .FCLCStatus = _schedule.LFCStatus
                .ProcessStatus = _schedule.ProcessStatus
                'If .GoingAliveDate <> ot.ConstNullDate  and .fclcstatus = "g1" Then
                '    .GoingAliveDate = s_schedule.createdOn
                'End If
                .ForecastChangedOn = _schedule.LastForecastUpdate
                .Scheduletype = _schedule.Typeid

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
                If _schedule Is Nothing Then
                    UpdateFromSchedule = False
                    Exit Function
                ElseIf Not _schedule.IsCreated And Not _schedule.IsLoaded Then
                    _schedule = Nothing
                    UpdateFromSchedule = False
                    Exit Function
                Else
                    aNewSchedule = _schedule
                    dlvUID = _schedule.Uid    ' assumption
                    sUID = _schedule.Uid
                    sUPDC = _schedule.Updc
                End If
            Else
                aNewSchedule = schedule
                dlvUID = aNewSchedule.Uid    ' assumption
                sUID = aNewSchedule.Uid
                sUPDC = aNewSchedule.Updc
            End If

            '*** Target is the Current if not specified otherwise
            If targetUPDC = -1 Then
                aCurrTarget = Deliverables.CurrentTarget.Retrieve(uid:=dlvUID, workspaceID:=workspaceID)
                If aCurrTarget Is Nothing Then
                    tUPDC = 0
                    aNewTarget = Nothing
                Else
                    tUPDC = aCurrTarget.UPDC
                    aTarget = Target.Retrieve(uid:=dlvUID, updc:=tUPDC)
                    If aTarget IsNot Nothing Then
                        aNewTarget = aTarget
                    Else
                        aNewTarget = Nothing
                    End If
                End If
            Else
                tUPDC = targetUPDC
                aTarget = Target.Retrieve(uid:=dlvUID, updc:=tUPDC)
                If aTarget IsNot Nothing Then
                    aNewTarget = aTarget
                Else
                    aNewTarget = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsAlive(throwError:=False) Then
                If Not Me.Create({dlvUID, sUID, sUPDC, tUPDC}) Then
                    Call Me.Inject({dlvUID, sUID, sUPDC, tUPDC})
                End If
            End If

            '** initialize in create/Inject !!
            _deliverableUID = dlvUID
            _scheduleUID = sUID
            _scheduleUPDC = sUPDC
            _targetUPDC = tUPDC
            _schedule = aNewSchedule
            _dlvTarget = aNewTarget

            With Me
                .workspaceID = aWorkspace


                '* finished
                .IsFinished = _schedule.IsFinished
                If _schedule.HasMilestoneDate("bp10") Then
                    .FinishedOn = _schedule.GetMilestoneValue("bp10")
                Else
                    .FinishedOn = constNullDate
                End If
                '* forecast
                .MSIDFinish = "bp9"
                If _schedule.HasMilestoneDate(.MSIDFinish) Then
                    .CurrentForecast = _schedule.GetMilestoneValue(.MSIDFinish)
                Else
                    .CurrentForecast = constNullDate
                End If

                '* check the gap
                If checkGAP Then .CheckOnGap()

                '* baseline itself
                If _schedule.IsBaseline Then
                    .IsFrozen = True
                    .ScheduleRevision = _schedule.Revision
                    .BaseLineFinishDate = _schedule.GetMilestoneValue(.MSIDFinish)
                    If _schedule.BaselineRefDate = constNullDate Then
                        .BaseLineFinishDateFrom = _schedule.CreatedOn
                    Else
                        .BaseLineFinishDateFrom = _schedule.BaselineRefDate
                    End If
                    .BaseLineUPDC = _schedule.Updc
                    If checkGAP Then .CheckOnBaselineGap()

                    '* take the data from the frozen one
                ElseIf _schedule.IsFrozen Then
                    .IsFrozen = True
                    .ScheduleRevision = _schedule.Revision
                    .BaseLineUPDC = _schedule.BaselineUPDC
                    If _schedule.BaselineRefDate = constNullDate Then
                        .BaseLineFinishDateFrom = _schedule.CreatedOn
                    Else
                        .BaseLineFinishDateFrom = _schedule.BaselineRefDate
                    End If
                    Dim aBaseline As Schedule = schedule.Retrieve(UID:=_schedule.Uid, updc:=_schedule.BaselineUPDC)
                    If aBaseline IsNot Nothing Then
                        .BaseLineFinishDate = aBaseline.GetMilestoneValue(.MSIDFinish)
                        If checkGAP Then .CheckOnBaselineGap()
                    End If
                    '* reset the freeze
                Else
                    .IsFrozen = False
                    .ScheduleRevision = ""
                    .BaseLineUPDC = -1
                    .BaseLineFinishDate = constNullDate
                    .BaseLineFinishDateFrom = constNullDate
                End If

                '* take the status
                .FCLCStatus = _schedule.LFCStatus
                .ProcessStatus = _schedule.ProcessStatus
                'If .GoingAliveDate <> ot.ConstNullDate  and .fclcstatus = "g1" Then
                '    .GoingAliveDate = s_schedule.createdOn
                'End If
                .ForecastChangedOn = _schedule.LastForecastUpdate
                .Scheduletype = _schedule.Typeid
                .TargetRevision = _dlvTarget.Revision

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
                If _deliverable Is Nothing Then
                    UpdateFromDeliverable = False
                    Exit Function
                Else
                    dlvUID = _deliverable.Uid
                End If
            Else
                _deliverable = deliverable
                dlvUID = deliverable.Uid
            End If

            ' set the objects
            aCurrSCHEDULE = _deliverable.GetCurrSchedule(workspaceID:=aWorkspace)
            If aCurrSCHEDULE Is Nothing Then
                sUID = 0
                sUPDC = 0
                _schedule = Nothing
            Else
                sUID = aCurrSCHEDULE.UID
                sUPDC = aCurrSCHEDULE.UPDC
                aSchedule = Schedule.Retrieve(UID:=sUID, updc:=sUPDC)
                If aSchedule IsNot Nothing Then
                    _schedule = aSchedule
                    If _schedule.workspaceID <> aWorkspace Then
                        aWorkspace = _schedule.workspaceID
                    End If
                Else
                    _schedule = Nothing
                End If
            End If

            aCurrTarget = _deliverable.GetCurrTarget(workspaceID:=aWorkspace)
            If aCurrTarget Is Nothing Then
                tUPDC = 0
                _dlvTarget = Nothing
            Else
                tUPDC = aCurrTarget.UPDC
                aTarget = Target.Retrieve(uid:=dlvUID, updc:=tUPDC)
                If aTarget IsNot Nothing Then
                    _dlvTarget = aTarget
                Else
                    _dlvTarget = Nothing
                End If
            End If

            ' load or create
            If Not Me.IsAlive(throwError:=False) Then
                If Not Me.Inject({dlvUID, sUID, sUPDC, tUPDC}) Then
                    Call Me.Create({dlvUID, sUID, sUPDC, tUPDC})
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

            If Not Me.IsLoaded And Not Me.IsCreated Then
                SetTarget = False
                Exit Function
            End If

            If Not _dlvTarget Is Nothing Then
                If (_dlvTarget.IsLoaded Or _dlvTarget.IsCreated) And _dlvTarget.UID = Me.DeliverableUID And _dlvTarget.UPDC = Me.TargetUPDC Then
                    SetTarget = True
                    Exit Function
                End If
            End If
            Dim aTarget As Target = Target.Retrieve(uid:=Me.DeliverableUID, updc:=Me.TargetUPDC)
            If aTarget Is Nothing Then
                _dlvTarget = Nothing
                SetTarget = False
                Exit Function
            End If

            _dlvTarget = aTarget
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
            Dim aSchedule As Schedule
            If Not Me.IsLoaded And Not Me.IsCreated Then
                SetSchedule = False
                Exit Function
            End If

            If Not _schedule Is Nothing Then
                If (_schedule.IsLoaded Or _schedule.IsCreated) And _schedule.Uid = Me.ScheduleUID And _schedule.Updc = Me.ScheduleUPDC Then
                    SetSchedule = True
                    Exit Function
                End If
            End If
            aSchedule = Schedule.Retrieve(UID:=Me.ScheduleUID, updc:=Me.ScheduleUPDC)
            If aSchedule IsNot Nothing Then
                _schedule = Nothing
                SetSchedule = False
                Exit Function
            End If

            _schedule = aSchedule
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

            If Not Me.IsLoaded And Not Me.IsCreated Then
                CheckOnGap = False
                Exit Function
            End If

            ' set the objects
            If Me.CurrentTargetDate = constNullDate Then
                If SetTarget() Then
                    Me.CurrentTargetDate = _dlvTarget.Target
                Else
                    CheckOnGap = False
                    Exit Function
                End If
            End If
            If Not Me.IsFinished And Me.CurrentForecast = constNullDate Then
                If SetSchedule() Then
                    If _schedule.HasMilestoneDate(Me.MSIDFinish) Then
                        Me.CurrentForecast = _schedule.GetMilestoneValue(Me.MSIDFinish)

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
            ElseIf Me.IsFinished And Me.FinishedOn = constNullDate Then
                If SetSchedule() Then
                    aDefScheduleMS = _schedule.GetScheduleMilestoneDefinition(Me.MSIDFinish)
                    actual = aDefScheduleMS.ActualOfFC
                    If _schedule.HasMilestoneDate(actual) Then
                        Me.FinishedOn = _schedule.GetMilestoneValue(Me.MSIDFinish)
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

            If aDate <> constNullDate And Me.CurrentTargetDate <> constNullDate Then
                aCE.Datevalue = aDate
                gap = aCE.DeltaDay(Me.CurrentTargetDate, considerAvailibilty:=True)
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

            If Not Me.IsLoaded And Not Me.IsCreated Then
                CheckOnBaselineGap = False
                Exit Function
            End If

            ' set the objects
            If Me.CurrentTargetDate = constNullDate Then
                If SetTarget() Then
                    Me.CurrentTargetDate = _dlvTarget.Target
                Else
                    CheckOnBaselineGap = False
                    Exit Function
                End If
            End If
            If Me.BaseLineFinishDate = constNullDate Then
                CheckOnBaselineGap = False
                Exit Function
            End If
            If Me.BaseLineFinishDate <> constNullDate And Me.CurrentTargetDate <> constNullDate Then
                aCE.Datevalue = Me.BaseLineFinishDate
                gap = aCE.DeltaDay(Me.CurrentTargetDate, considerAvailibilty:=True)
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
    <ormObject(id:=DeliverableType.ConstObjectID, description:="type definition of a deliverable. Defines default setting and some general logic.", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Class DeliverableType
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "DeliverableType"
        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID = "tblDefDeliverableTypes"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=constFNTypeID, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"

        ''' <summary>
        ''' keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primarykeyordinal:=1, isnullable:=True, _
           title:="Type", description:="type of the deliverable", XID:="DLVT1")> Public Const constFNTypeID = "id"
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
        <ormObjectEntry(referenceobjectentry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, isnullable:=True, _
            title:="Schedule Type", description:="default schedule type of the deliverable", XID:="DLVT21")> Public Const constFNDefScheduleType = "defscheduletype"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            title:="Organization Unit", description:="default organization unit responsible of the deliverable", XID:="DLVT22")> Public Const constFNDefRespOU = "defrespOU"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, isnullable:=True, _
           title:="Function", description:="default function type of the deliverable", XID:="DLVT23")> Public Const constFNDefFunction = "deffunction"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
          title:="Function", description:="default target responsible organization Unit", XID:="DLVT24")> Public Const constFNDefTargetOU = "deftargetOu"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultValue:=False, _
          title:="Target Necessary", description:="has mandatory target data", XID:="DLVT25")> Public Const constFNhastarget = "hastargetdata"


        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
         title:="Description", description:="description of the deliverable type", XID:="DLVT3")> Public Const constFNDescription = "desc"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
        title:="comment", description:="comments of the deliverable", XID:="DLVT10")> Public Const constFNComment = "cmt"

        '*** Mapping
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String
        <ormEntryMapping(EntryName:=constFNDefScheduleType)> Private _defScheduleType As String
        <ormEntryMapping(EntryName:=constFNDefFunction)> Private _deffunction As String
        <ormEntryMapping(EntryName:=constFNDefRespOU)> Private _defRespOU As String
        <ormEntryMapping(EntryName:=constFNDefTargetOU)> Private _defTargetOU As String
        <ormEntryMapping(EntryName:=constFNhastarget)> Private _hasAlwasyTarget As Boolean = False

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the has alwasy target.
        ''' </summary>
        ''' <value>The has alwasy target.</value>
        Public Property HasAlwasyTarget() As Boolean
            Get
                Return Me._hasAlwasyTarget
            End Get
            Set(value As Boolean)
                SetValue(constFNhastarget, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the def target OU.
        ''' </summary>
        ''' <value>The def target OU.</value>
        Public Property DefTargetOU() As String
            Get
                Return Me._defTargetOU
            End Get
            Set(value As String)
                SetValue(constFNDefTargetOU, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the def resp OU.
        ''' </summary>
        ''' <value>The def resp OU.</value>
        Public Property DefRespOU() As String
            Get
                Return Me._defRespOU
            End Get
            Set(value As String)
                SetValue(constFNDefRespOU, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the deffunction.
        ''' </summary>
        ''' <value>The deffunction.</value>
        Public Property Deffunction() As String
            Get
                Return Me._deffunction
            End Get
            Set(value As String)
                SetValue(constFNDefFunction, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the type of the def schedule.
        ''' </summary>
        ''' <value>The type of the def schedule.</value>
        Public Property DefScheduleType() As String
            Get
                Return Me._defScheduleType
            End Get
            Set(value As String)
                SetValue(constFNDefScheduleType, value)
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
        Public Shared Function Create(ByVal typeid As String, Optional ByVal domainID As String = "") As DeliverableType
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {typeid, domainID}
            Return CreateDataObject(Of DeliverableType)(pkArray:=primarykey, domainID:=domainID, checkUnique:=True)
        End Function


        ''' <summary>
        ''' Retrieve a deliverable Type object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal typeid As String, Optional ByVal domainID As String = "", Optional forcereload As Boolean = False) As DeliverableType
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {typeid, domainID}
            Return Retrieve(Of DeliverableType)(pkArray:=pkarray, forceReload:=forcereload)
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

    ''' <summary>
    ''' Deliverable class for arbitrary tracking
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=Deliverable.ConstObjectID, description:="arbitrary object for tracking, scheduling, change and configuration mgmt.", _
        modulename:=ConstModuleDeliverables, useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False, Version:=1)> Public Class Deliverable
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iotCloneable(Of Deliverable)

        Public Const ConstObjectID = "Deliverable"
        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID = "tblDeliverables"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomainID, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=constFNfuid, columnname3:=ConstFNIsDeleted)> Public Const constIndexRevisions = "indRevisions"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=ConstFNIsDeleted)> Public Const constIndexDelete = "indDeletes"
        <ormSchemaIndex(columnName1:=constFNPartID, columnname2:=ConstFNIsDeleted)> Public Const constIndexParts = "indParts"
        <ormSchemaIndex(columnName1:=constFNWBSID, columnname2:=constFNWBSCode, columnname3:=constFNUid, columnname4:=ConstFNIsDeleted)> Public Const constIndexWBS = "indWBS"
        <ormSchemaIndex(columnname1:=constFNMatchCode, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexMatchcode = "indmatchcode"
        <ormSchemaIndex(columnname1:=constFNCategory, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexcategory = "indcategory"
        <ormSchemaIndex(columnname1:=constFNFunction, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexFunction = "indFunction"
        <ormSchemaIndex(columnname1:=constFNTypeID, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexType = "indType"

        '*** primary key
        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=1, _
            title:="Unique ID", description:="unique id of the deliverable", XID:="DLV1", aliases:={"UID"})> _
        Public Const constFNUid = "uid"

        '** fields
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
            title:="category", description:="category of the deliverable", XID:="DLV2")> Public Const constFNCategory = "cat"
        <ormObjectEntry(typeid:=otDataType.Text, size:=255, isnullable:=True, _
            title:="id", description:="id of the deliverable", XID:="DLV3")> Public Const constFNDeliverableID = "id"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Matchcode", description:="match code of the deliverable", XID:="DLV4")> Public Const constFNMatchCode = "matchcode"


        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            isnullable:=True, _
            dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
        Public Const ConstFNDomain = "DOMAIN" '' different name since we donot want to get it deactivated due to missing domain behavior

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            description:="not used and should be not active", _
            useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID  '' const not overidable
        '
        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            Description:="workspaceID ID of the deliverable", dbdefaultvalue:="@", isnullable:=True, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> Public Const ConstFNWorkspace = Workspace.ConstFNID

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Revision", description:="revision of the deliverable", XID:="DLV6")> Public Const constFNRevision = "drev"

        <ormObjectEntry(referenceobjectentry:=ConstObjectID & "." & constFNUid, title:="First Revision UID", description:="unique id of the first revision deliverable", _
            XID:="DLV7", aliases:={}, isnullable:=True)> Public Const constFNfuid = "fuid"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Change Reference", description:="change reference of the deliverable", XID:="DLV8")> Public Const constFNChangeRef = "chref"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Format", description:="format of the deliverable", XID:="DLV9")> Public Const constFNFormat = "frmt"

        <ormObjectEntry(typeid:=otDataType.Text, size:=255, isnullable:=True, _
            title:="Description", description:="description of the deliverable", XID:="DLV10")> Public Const constFNDescription = "desc"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            title:="Responsible OrgUnit", description:=" organization unit responsible for the deliverable", XID:="DLV11")> _
        Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Part.ConstObjectID & "." & Part.ConstFNPartID, _
            isnullable:=True, description:="part id of the deliverable", XID:="DLV12", _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNPartID = Part.ConstFNPartID

        <ormObjectEntry(referenceobjectentry:=DeliverableType.ConstObjectID & "." & DeliverableType.constFNTypeID, _
            title:="Type", description:="type of the deliverable", XID:="DLV13", _
             useforeignkey:=otForeignKeyImplementation.ORM)> Public Const constFNTypeID = "typeid"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, _
            title:="Responsible", description:="responsible person for the deliverable", XID:="DLV16")> Public Const constFNResponsiblePerson = "resp"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="blocking item reference", description:="blocking item reference id for the deliverable", XID:="DLV17")> Public Const constFNBlockingItemReference = "blitemid"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
            title:="comment", description:="comments and extended description of the deliverable", XID:="DLV18")> Public Const constFNComment = "cmt"

        <ormObjectEntry(referenceobjectentry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)>
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
        title:="wbs reference", description:="work break down structure for the deliverable", XID:="DLV22")> _
        Public Const constFNWBSID = "wbs"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
        title:="wbscode reference", description:="wbscode for the deliverable", XID:="DLV23")> _
        Public Const constFNWBSCode = "wbscode"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            title:="Function", description:="function of the deliverable", XID:="DLV30")> Public Const constFNFunction = "function"

        <ormObjectEntry(typeid:=otDataType.Text, size:=150, isnullable:=True, _
           XID:="DLV31", Title:="Workpackage", description:="workpackage of the deliverable")> Public Const ConstFNWorkpackage = "wkpk"



        '*** mappings
        <ormEntryMapping(EntryName:=constFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=constFNfuid)> Private _firstrevUID As Long?
        <ormEntryMapping(EntryName:=constFNDeliverableID)> Private _deliverableID As String
        <ormEntryMapping(EntryName:=constFNRevision)> Private _revision As String
        <ormEntryMapping(EntryName:=constFNFormat)> Private _format As String
        <ormEntryMapping(EntryName:=constFNCategory)> Private _category As String
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String
        'Private s_customerID As String = "" outdated movved to targets
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOUID As String
        <ormEntryMapping(EntryName:=constFNMatchCode)> Private _matchcode As String
        'Private s_assycode As String = "" obsolete
        <ormEntryMapping(EntryName:=constFNPartID)> Private _partID As String
        <ormEntryMapping(EntryName:=constFNChangeRef)> Private _changerefID As String
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String
        <ormEntryMapping(EntryName:=constFNResponsiblePerson)> Private _responsibleID As String
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String

        <ormEntryMapping(EntryName:=constFNWBSID)> Private _wbsid As String
        <ormEntryMapping(EntryName:=constFNWBSCode)> Private _wbscode As String
        <ormEntryMapping(EntryName:=constFNFunction)> Private _function As String
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _wspaceID As String
        <ormEntryMapping(EntryName:=ConstFNWorkpackage)> Private _workpackage As String
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
                Return _revision
            End Get
            Set(value As String)
                SetValue(constFNRevision, value)
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
                Return _format
            End Get
            Set(value As String)
                SetValue(constFNFormat, value)
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
                Return _deliverableID
            End Get
            Set(value As String)
                SetValue(constFNDeliverableID, value)
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
                Return _wspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspace, value)
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
                Return _description
            End Get
            Set(value As String)
                SetValue(constFNDescription, value)
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
                Return _category
            End Get
            Set(value As String)
                SetValue(constFNCategory, value)
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
                Return _responsibleID
            End Get
            Set(value As String)
                SetValue(constFNResponsiblePerson, value)
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
                Return _respOUID
            End Get
            Set(value As String)
                SetValue(constFNRespOU, value)
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
        ''' gets or sets the Part Id related to the deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PartID() As String
            Get
                Return _partID
            End Get
            Set(value As String)
                SetValue(constFNPartID, value)
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
                Return _changerefID
            End Get
            Set(value As String)
                SetValue(constFNChangeRef, value)
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
                Return _typeid
            End Get
            Set(value As String)
                SetValue(constFNTypeID, value)
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
                Return _wbsid
            End Get
            Set(value As String)
                SetValue(constFNWBSID, value)
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
                Return _workpackage
            End Get
            Set(value As String)
                SetValue(ConstFNWorkpackage, value)
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
                Return _wbscode
            End Get
            Set(value As String)
                SetValue(constFNWBSCode, value)
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
                Return _function
            End Get
            Set(value As String)
                SetValue(constFNFunction, value)
            End Set
        End Property
        ReadOnly Property MsglogTag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = GetUniqueTag()
                End If
                Return _msglogtag
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
                Return _blockingitemID
            End Get
            Set(value As String)
                SetValue(constFNBlockingItemReference, value)
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
                Return _comment
            End Get
            Set(value As String)
                SetValue(constFNComment, value)
            End Set
        End Property



#End Region

        '****** getUniqueTag
        Public Function GetUniqueTag()
            GetUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _uid & ConstDelimiter
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
        ''' Retrieve the Deliverable
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(uid As Long) As Deliverable
            Dim pkarray() As Object = {uid}
            Return ormDataObject.Retrieve(Of Deliverable)(pkArray:=pkarray)
        End Function

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
            If Not Me.IsAlive(subname:="GetPart") Then Return Nothing
            Return Part.Retrieve(Me.PartID)
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
                    aCurrTarget = Me.GetCurrTarget(workspaceID)
                    If aCurrTarget Is Nothing Then
                        targetUPDC = 0
                    Else
                        targetUPDC = aCurrTarget.UPDC
                    End If
                End If
                If scheduleUPDC > 0 Then
                    Return Track.Retrieve(Me.Uid, scheduleUID:=Me.Uid, scheduleUPDC:=scheduleUPDC, targetUPDC:=targetUPDC)
                End If
            End If

            Return Nothing
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
            If Not IsAlive(subname:="GetCurrTarget") Then Return Nothing
            Return CurrentTarget.Retrieve(Me.Uid, workspaceID)
        End Function
        ''' <summary>
        ''' retrieve the current schedule object
        ''' </summary>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns>the data object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetCurrSchedule(Optional ByVal workspaceID As String = "") As CurrentSchedule
            If Not IsAlive(subname:="GetCurrSchedule") Then Return Nothing
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            Dim aSchedulelink = ScheduleLink.Retrieve(fromid:=Me.ConstObjectID, fromuid:=Me.Uid)
            If aSchedulelink IsNot Nothing Then
                Return CurrentSchedule.Retrieve(UID:=aSchedulelink.ToUid, workspaceID:=workspaceID)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' retrieves the active and curent schedule object for the deliverable 
        ''' </summary>
        ''' <param name="workspaceID">workspaceID id</param>
        ''' <returns>a scheduling object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetSchedule(Optional ByVal workspaceID As String = "") As Schedule
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If Not IsAlive(subname:="GetSchedule") Then Return Nothing

            ' get
            Dim aCurrSCHEDULE As CurrentSchedule = Me.GetCurrSchedule(workspaceID:=workspaceID)
            ' load
            If aCurrSCHEDULE IsNot Nothing Then
                Return Schedule.Retrieve(UID:=Me.Uid, updc:=aCurrSCHEDULE.UPDC)
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

            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If Not IsAlive(subname:="GetTarget") Then Return Nothing
            ' get
            Dim aCurrTarget As CurrentTarget = Me.GetCurrTarget(workspaceID:=workspaceID)
            If aCurrTarget IsNot Nothing Then
                ' load the current schedule

                Return Target.Retrieve(uid:=Me.Uid, updc:=aCurrTarget.UPDC)
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' retrieve maximum update count from the datastore
        ''' </summary>
        ''' <param name="max">the max to be set</param>
        ''' <param name="workspaceID">optional workspaceID</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function GetNewUID(ByRef max As Long, Optional domainID As String = "") As Boolean
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
            aNewTarget.WorkspaceID = CurrentSession.CurrentWorkspaceID
            aNewTarget.Persist()

            Dim anewCurrTarget As CurrentTarget = CurrentTarget.Create(newDeliverable.Uid, workspaceID:=CurrentSession.CurrentWorkspaceID)
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
            If Not Me.IsLoaded And Not Me.IsCreated Then
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
                aNewTarget.WorkspaceID = aFirstSchedule.workspaceID
                aNewTarget.Persist()

                Dim anewCurrTarget As CurrentTarget = CurrentTarget.Create(newDeliverable.Uid, workspaceID:=aFirstSchedule.workspaceID)
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
        Public Function CloneCartypes(ByVal newUID As Long) As clsLEGACYCartypes
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim pkarry() As Object
            Dim aCartypes As New clsLEGACYCartypes
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
                                            tablename:=PrimaryTableID, entryname:="uid", messagetype:=otCoreMessageType.InternalError)
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
