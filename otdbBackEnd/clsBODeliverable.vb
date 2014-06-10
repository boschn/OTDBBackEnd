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
Imports OnTrack.ObjectProperties

Namespace OnTrack.Deliverables


   
    ''' <summary>
    ''' Current target object points to the current clsOTDBDeliverableTarget 
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=WorkspaceTarget.ConstObjectID, description:="linking object to the current target per workspace", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
    Public Class WorkspaceTarget
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iormCloneable(Of WorkspaceTarget)

        Public Const ConstObjectID = "WORKSPACETARGET"
        '** Schema Table
        <ormSchemaTable(Version:=3)> Public Const ConstTableID = "TBLWORKSPACETARGETS"

        '** PrimaryKey
        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, primarykeyordinal:=1, _
                       useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNWorkspaceID = Workspace.ConstFNID

        <ormObjectEntry(referenceObjectEntry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, primarykeyordinal:=2, _
                        useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
                        XID:="CDT1", aliases:={"UID"})> Public Const ConstFNUid = Deliverable.constFNUid

        '** other columns
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, _
           title:="Revision", description:="revision of the target", XID:="T9")> Public Const ConstFNRevision = "rev"
        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
         title:="working counter", description:="update number of the working target", XID:="T10")> Public Const ConstWorkUPDC = "workupdc"
        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
         title:="Alive Counter", description:="update number of the alive target", XID:="T11")> Public Const ConstAliveUPDC = "aliveupdc"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
          title:="is active", description:="is the target active", XID:="DT4")> Public Const ConstFNIsActive = "isactive"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
              useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** mappings
        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String = ""
        <ormEntryMapping(EntryName:=ConstWorkUPDC)> Private _workupdc As Long?    ' UPDC of target
        <ormEntryMapping(EntryName:=ConstAliveUPDC)> Private _aliveupdc As Long?    ' UPDC of target
        <ormEntryMapping(EntryName:=ConstFNIsActive)> Private _isActive As Boolean = True 'explicitly set to be active in the beginning !

        ''' <summary>
        ''' Relation to alive Schedule edition - will be resolved by events
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Target), ToPrimaryKeys:={ConstFNUid, ConstAliveUPDC}, _
                     cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=True)> _
        Public Const ConstRAliveTarget = "REL_ALIVETARGET"

        <ormEntryMapping(relationName:=ConstRAliveTarget, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand Or otInfuseMode.OnInject)> _
        Private _alivetarget As Target

        ''' <summary>
        ''' Relation to alive Schedule edition - will be resolved by events
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Target), createObjectifnotretrieved:=True, _
                    ToPrimaryKeys:={ConstFNUid, ConstWorkUPDC}, _
                     cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=True)> _
        Public Const ConstRWorkTarget = "REL_WORKINGTARGET"

        <ormEntryMapping(relationName:=ConstRWorkTarget, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand Or otInfuseMode.OnInject)> _
        Private _workingtarget As Target

        ''' <summary>
        ''' Relation to alive Schedule edition - will be resolved by events
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Deliverable), _
                     ToPrimaryKeys:={ConstFNUid}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRDeliverable = "REL_DELIVERABLE"

        <ormEntryMapping(relationName:=ConstRDeliverable, infusemode:=otInfuseMode.OnDemand)> Private _deliverable As Deliverable 'Backlink


        ''' <summary>
        ''' dynamic data
        ''' </summary>
        ''' <remarks></remarks>


        ''' <summary>
        ''' Define the constants for accessing the compounds
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstOPGetTarget = "GETTARGET"
        Public Const ConstOPSetTarget = "SETTARGET"
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            AddHandler CurrentSession.OnWorkspaceChanged, AddressOf Me.WorkspaceTarget_OnWorkspaceChanged
        End Sub

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the deliverable.
        ''' </summary>
        ''' <value>The deliverable.</value>
        Public ReadOnly Property Deliverable() As Deliverable
            Get
                If Me.GetRelationStatus(ConstRDeliverable) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRDeliverable)
                Return Me._deliverable
            End Get

        End Property

        ''' <summary>
        ''' Gets the target object
        ''' </summary>
        ''' <value>The target.</value>
        Public ReadOnly Property Target() As Target
            Get
                If Me.WorkingTargetUPDC.HasValue Then
                    Return Me.WorkingTarget
                ElseIf Me.AliveTargetUPDC.HasValue Then
                    Return Me.AliveTarget
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the working target object
        ''' </summary>
        ''' <value>The target.</value>
        Public ReadOnly Property WorkingTarget() As Target
            Get
                If GetRelationStatus(ConstRWorkTarget) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRWorkTarget)
                Return Me._workingtarget
            End Get
        End Property


        ''' <summary>
        ''' Gets the alive target object
        ''' </summary>
        ''' <value>The target.</value>
        Public ReadOnly Property AliveTarget() As Target
            Get
                If GetRelationStatus(ConstRWorkTarget) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRAliveTarget)
                Return Me._alivetarget
            End Get
        End Property
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
        Public ReadOnly Property UPDC() As Long?
            Get
                If _workupdc.HasValue Then Return _workupdc
                Return _aliveupdc
            End Get
        End Property

        ''' <summary>
        ''' gets the updc of the working target version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WorkingTargetUPDC() As Long?
            Get
                Return _workupdc
            End Get
            Set(value As Long?)
                SetValue(ConstWorkUPDC, value)
            End Set
        End Property

        ''' <summary>
        ''' gets the updc of the working target version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AliveTargetUPDC() As Long?
            Get
                Return _aliveupdc
            End Get
            Set(value As Long?)
                SetValue(ConstAliveUPDC, value)
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
        ''' operation to Access the Milestone's Value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPGetTarget, tag:=ObjectCompoundEntry.ConstCompoundGetter, _
            parameterEntries:={ObjectCompoundEntry.ConstFNEntryName, ObjectCompoundEntry.ConstFNValues})> _
        Public Function GetTarget(id As String, ByRef value As Object) As Boolean
            If Not IsAlive(subname:="GetTarget") Then Return Nothing

            If _workingtarget IsNot Nothing Then
                value = _workingtarget.Target
                Return True
            ElseIf _alivetarget IsNot Nothing Then
                value = _alivetarget.Target
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' operation to Access the Milestone's Value
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPSetTarget, tag:=ObjectCompoundEntry.ConstCompoundSetter, _
            parameterEntries:={ObjectCompoundEntry.ConstFNEntryName, ObjectCompoundEntry.ConstFNValues})> _
        Public Function SetTarget(id As String, value As Object) As Boolean
            If Not IsAlive(subname:="SetTarget") Then Return Nothing

            If _workingtarget Is Nothing Then

                If _alivetarget IsNot Nothing Then
                    _workingtarget = _alivetarget.Clone()

                End If
            ElseIf _workingtarget IsNot Nothing Then
                Return _workingtarget.SetValue(Target.constFNTarget, value:=value)
            End If

            Return False
        End Function
        ''' <summary>
        ''' publish is a persist with history and baseline integrated functions. It sets the working edition as the alive edition
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Publish(Optional ByRef msglog As ObjectMessageLog = Nothing, _
                                Optional ByVal timestamp As Date? = Nothing) As Boolean
            Dim isProcessable As Boolean = True

            Dim aWorkingTarget As Target = _workingtarget

            '* init
            If Not Me.IsAlive(subname:="Publish") Then Return False


            ' TIMESTAMP
            If timestamp Is Nothing Then timestamp = Date.Now


            '** if any of the milestones is changed
            '**
            isProcessable = True

            '** condition
            If aWorkingTarget IsNot Nothing AndAlso aWorkingTarget.IsChanged Then

                If isProcessable Then
                    Dim publishflag As Boolean = False
                    Dim aNewDate As Date?
                    Dim anOldDate As Date?
                    aNewDate = aWorkingTarget.Target
                    anOldDate = aWorkingTarget.PrevTarget
                    If aNewDate.HasValue AndAlso anOldDate.HasValue Then
                        If DateDiff("d", anOldDate, aNewDate) >= 0 Then
                            '** Now we should approve ??!
                            '** at least we increase the revision count
                            aWorkingTarget.Revision = aWorkingTarget.IncreaseRevison(majorFlag:=False, minorFlag:=True)

                            publishflag = True
                        End If
                    ElseIf aNewDate.HasValue Then
                        aWorkingTarget.Revision = "V1.0"
                        publishflag = True
                    End If

                    '** change over THE working schedule to alive scheudle
                    '**
                    If publishflag Then
                        Me.AliveTargetUPDC = aWorkingTarget.UPDC
                        _alivetarget = aWorkingTarget

                        Me.WorkingTargetUPDC = Nothing
                        '' cannot generate an new updc on a created edition (getmax will not work on unpersisted objects)
                        If _alivetarget.IsCreated Then
                            _workingtarget = aWorkingTarget.Clone(_alivetarget.UID, _alivetarget.UPDC + 1)
                        Else
                            _workingtarget = aWorkingTarget.Clone()
                        End If
                        '* should be cloned but to make sure
                        _workingtarget.DomainID = aWorkingTarget.DomainID

                        Me.WorkingTargetUPDC = _workingtarget.UPDC
                    End If

                    ''' save the workspace schedule itself and the
                    ''' related objects
                    isProcessable = MyBase.Persist(timestamp)


                Else
                    isProcessable = False
                    Debug.Assert(False)

                End If
            ElseIf Me.IsChanged Or Me.IsCreated Then
                '**** save without Milestone checking
                isProcessable = MyBase.Persist(timestamp:=timestamp)
            Else
                '** nothing changed
                '***
                Return False
            End If

            Return isProcessable
        End Function



        ''' <summary>
        ''' Persist with checking on publish
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <param name="doFeedRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.constNullDate, Optional doFeedRecord As Boolean = True) As Boolean Implements iormPersistable.Persist
            Dim myDeliverable As Deliverable = Me.Deliverable
            Dim autopublish As Boolean = CurrentSession.AutoPublishTarget
            If myDeliverable IsNot Nothing Then
                Dim adeltype As DeliverableType = myDeliverable.DeliverableType
                If adeltype IsNot Nothing Then
                    autopublish = adeltype.AutoPublishTarget
                End If
            End If
            If autopublish Then
                Return Publish(timestamp:=timestamp)
            Else
                Return MyBase.Persist(timestamp:=timestamp, doFeedRecord:=doFeedRecord)
            End If
        End Function

        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(pkarray() As Object, Optional runtimeOnly As Boolean? = Nothing) As WorkspaceTarget Implements iormCloneable(Of WorkspaceTarget).Clone
            Return MyBase.Clone(Of WorkspaceTarget)(pkarray)
        End Function
        ''' <summary>
        ''' Clone this data object by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal uid As Long, Optional ByVal workspaceID As String = "") As WorkspaceTarget
            Dim pkarray() As Object = {uid, workspaceID}
            Return Me.Clone(Of WorkspaceTarget)(pkarray)
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
                    Dim aCurrTarget As New WorkspaceTarget
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
                    aCommand.Where = "[" & ConstFNWorkspaceID & "] = @wspace "
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@wspace", tablename:=ConstTableID, ColumnName:=ConstFNWorkspaceID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@wspace", value:=workspaceID)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aCurrTarget As New WorkspaceTarget
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
        Public Shared Function Retrieve(ByVal uid As Long, Optional ByVal workspaceID As String = "") As WorkspaceTarget
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
                Dim aCurrentTarget = WorkspaceTarget.RetrieveUniqueBy(uid:=uid, workspaceID:=aWS)
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
        Public Shared Function RetrieveUniqueBy(ByVal uid As Long, ByVal workspaceID As String) As WorkspaceTarget
            Dim pkarry() As Object = {workspaceID, uid}
            Return ormDataObject.Retrieve(Of WorkspaceTarget)(pkarry)
        End Function


        ''' <summary>
        ''' create a current Target by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal uid As Long, Optional ByVal workspaceID As String = "", Optional ByVal domainid As String = Nothing) As WorkspaceTarget
            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim pkarray() As Object = {workspaceID, uid}
            Return ormDataObject.CreateDataObject(Of WorkspaceTarget)(pkarray, checkUnique:=True)
        End Function

        ''' <summary>
        ''' the default values needed event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub WorkspaceTarget_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreateDefaultValuesNeeded

            If Not e.Record.HasIndex(ConstFNWorkspaceID) OrElse e.Record.GetValue(ConstFNWorkspaceID) = "" Then
                e.Record.SetValue(ConstFNWorkspaceID, CurrentSession.CurrentWorkspaceID)
            End If
        End Sub


        ''' <summary>
        ''' handles the relationCreateNeeded Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub WorkspaceTarget_OnRelationCreateNeeded(sender As Object, e As ormDataObjectRelationEventArgs) Handles Me.OnRelationCreateNeeded
            If Not Me.IsAlive(subname:="WorkspaceTarget_OnRelationCreateNeeded") Then Return

            If e.RelationID = ConstRWorkTarget Then
                ''' always gives the current workspace
                Dim aTarget As Target
                If Me.WorkingTargetUPDC.HasValue AndAlso Me.WorkingTargetUPDC <> 0 Then
                    aTarget = Deliverables.Target.Retrieve(uid:=Me.UID, updc:=Me.WorkingTargetUPDC)
                Else
                    aTarget = Deliverables.Target.Create(uid:=Me.UID, workspaceID:=Me.WorkspaceID)
                End If
                If aTarget IsNot Nothing Then
                    Me.WorkingTargetUPDC = aTarget.UPDC
                    ' we cannot reach the deliveable from here -> done in the Deliverable
                    'If Not needsTarget Then aTarget.NotargetByItention = True
                    'aTarget.ResponsibleOU = defaultTargetOUT

                    e.RelationObjects.Add(aTarget)
                    e.Finished = True
                End If

            End If

        End Sub
        ''' <summary>
        ''' Event Handler for Workspace Change
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub WorkspaceTarget_OnWorkspaceChanged(sender As Object, e As SessionEventArgs)
            Throw New NotImplementedException("Workspace Target Event Reaction on OnWorkspaceChanged to be implemented")
        End Sub


    End Class

    ''' <summary>
    ''' target object for the deliverable class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=OnTrack.Deliverables.Target.ConstObjectID, description:="target definition per workspace of a deliverable e.g. date to be delivered", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True)> _
    Public Class Target
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable
        Implements iormCloneable(Of Target)

        Public Const ConstObjectID As String = "Target"
        '** Schema Table
        <ormSchemaTableAttribute(version:=2)> Public Const constTableID = "tblDeliverableTargets"
        '** Index
        <ormSchemaIndexAttribute(columnname1:=constFNUid, columnname2:=constFNUpdc, columnname3:=ConstFNIsDeleted)> Public Const constIndexUID = "uid"


        '** Keys
        <ormObjectEntry(referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, _
            defaultValue:="0", primaryKeyordinal:=1, useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            XID:="DT1", aliases:={"UID"})> Public Const constFNUid = Deliverable.constFNUid

        <ormObjectEntry(Datatype:=otDataType.Long, defaultValue:="0", primaryKeyordinal:=2, _
            description:="update count of the target date", title:="Update count", XID:="DT2", aliases:={"UPDC"})> Public Const constFNUpdc = "updc"

        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            Description:="workspaceID ID of the schedule")> Public Const ConstFNWorkspaceID = ScheduleEdition.ConstFNWorkspaceID

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
          title:="No Target", description:="no target by intention", XID:="DT3")> Public Const ConstFNNoTarget = "notarget"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
          title:="Type", description:="type of the target", XID:="DT4")> Public Const ConstFNType = "typeid"

        <ormObjectEntry(Datatype:=otDataType.Date, isnullable:=True, _
           description:="previous target date", title:="previous target date", XID:="DT5")> Public Const constFNPrevTarget = "PVTD"

        <ormObjectEntry(Datatype:=otDataType.Date, isnullable:=True, _
            description:="current target date", title:="target date", XID:="DT6")> Public Const constFNTarget = "TARGETDATE"



        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, _
          description:="target change timestamp", title:="target change", XID:="DT7")> Public Const constFNTargetChanged = "tchg"



        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, title:="target revision", Description:="revision of the target", _
         XID:="DT14", isnullable:=True)> Public Const ConstFNRevision = "rev"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
           title:="Responsible OrgUnit", description:=" organization unit responsible for the target", XID:="DT15")> Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, isnullable:=True, _
            title:="Responsible Person", description:="responsible person for the target", XID:="DT16")> Public Const constFNResp = "resp"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, _
            title:="Comment", Description:="comment of the target", XID:="DT17", isnullable:=True)> Public Const ConstFNComment = "cmt"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID


        '*** variables
        <ormEntryMapping(EntryName:=constFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=constFNUpdc)> Private _updc As Long

        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _WorkspaceID As String = ""
        <ormEntryMapping(EntryName:=constFNTarget)> Private _targetdate As Date?
        <ormEntryMapping(EntryName:=constFNPrevTarget)> Private _prevTarget As Date?
        <ormEntryMapping(EntryName:=constFNTargetChanged)> Private _TargetChangedDate As Date?
        <ormEntryMapping(EntryName:=ConstFNRevision)> Private _rev As String
        <ormEntryMapping(EntryName:=ConstFNNoTarget)> Private _notargetByItention As Boolean
        <ormEntryMapping(EntryName:=ConstFNType)> Private _typeid As String
        <ormEntryMapping(EntryName:=constFNRespOU)> Private _respOU As String
        <ormEntryMapping(EntryName:=constFNResp)> Private _resp As String
        <ormEntryMapping(EntryName:=ConstFNComment)> Private _cmt As String

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New()
            AddHandler Me.OnPersisted, AddressOf Track.Track_OnPersisted
        End Sub

#Region "properties"
        ''' <summary>
        ''' Gets or sets the target changed date.
        ''' </summary>
        ''' <value>The target changed date.</value>
        Public Property TargetChangedDate() As DateTime?
            Get
                Return Me._TargetChangedDate
            End Get
            Set(value As DateTime?)
                SetValue(constFNTargetChanged, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the notarget by itention.
        ''' </summary>
        ''' <value>The notarget by itention.</value>
        Public Property NotargetByItention() As Boolean
            Get
                Return Me._notargetByItention
            End Get
            Set(value As Boolean)
                SetValue(ConstFNNoTarget, Value)
            End Set
        End Property

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
                Return _rev
            End Get
            Set(value As String)
                SetValue(ConstFNRevision, value)
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
                Return _WorkspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspaceID, value)
            End Set
        End Property




#End Region


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
                        Dim aNewcurSchedule As New WorkspaceSchedule
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
        ''' handles the OnCreating Event to generate an new UID if necessary
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Target_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreating
            Dim anUid As Long? = e.Record.GetValue(constFNUid)
            Dim anUpdc As Long? = e.Record.GetValue(constFNUpdc)

            '* new uid
            If Not anUpdc.HasValue OrElse anUpdc = 0 Then
                anUpdc = Nothing
                Dim primarykey As Object() = {anUid, anUpdc}
                If e.DataObject.PrimaryTableStore.CreateUniquePkValue(pkArray:=primarykey) Then
                    e.Record.SetValue(constFNUpdc, primarykey(1)) ' to be created
                    e.Result = True
                    e.Proceed = True
                Else
                    CoreMessageHandler(message:="primary keys couldnot be created ?!", subname:="Target.Target_OnCreating", _
                                       messagetype:=otCoreMessageType.InternalError)
                End If

            End If


        End Sub

        ''' <summary>
        ''' create the persistent target by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal uid As Long, Optional ByVal updc As Long = 0, Optional workspaceID As String = "") As Target
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(constFNUid, uid)
                .SetValue(constFNUpdc, updc)
                If workspaceID <> "" Then .SetValue(ConstFNWorkspaceID, workspaceID)
            End With
            Return ormDataObject.CreateDataObject(Of Target)(aRecord, checkUnique:=True)
        End Function

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
            Dim aCurrTarget As New WorkspaceTarget
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
                aCurrTarget = WorkspaceTarget.Retrieve(uid:=anUID, workspaceID:=workspaceID)
                If aCurrTarget Is Nothing Then
                    aCurrTarget = WorkspaceTarget.Create(uid:=anUID, workspaceID:=workspaceID)
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
                aCurrTarget = WorkspaceTarget.Retrieve(anUID, workspaceID)
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
            'aCurrTarget.UPDC = anUPDC
            aCurrTarget.Revision = aNewTarget.Revision
            PublishNewTarget = aCurrTarget.Persist

            '***
            '***
            '' Call aTrack.UpdateTracking(Me, workspaceID:=workspaceID, persist:=True, checkGAP:=True)

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
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(pkarray() As Object, Optional runtimeOnly As Boolean? = Nothing) As Target Implements iormCloneable(Of Target).Clone
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
        Public Overloads Function Clone(Optional ByVal uid? As Long = Nothing, Optional ByVal updc? As Long = Nothing) As Target
            If Not uid.HasValue Then uid = Me.UID
            Dim pkarray() As Object = {uid, updc}
            Return Me.Clone(pkarray)
        End Function

        ''' <summary>
        ''' Default Values Handler
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Target_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreateDefaultValuesNeeded

            Dim anUID As Long? = e.Record.GetValue(constFNUid)

            If Not e.Record.HasIndex(ConstFNWorkspaceID) OrElse e.Record.GetValue(ConstFNWorkspaceID) = "" Then
                e.Record.SetValue(ConstFNWorkspaceID, CurrentSession.CurrentWorkspaceID)
            End If
        End Sub

        ''' <summary>
        ''' Property change Handler -- saving the old target in the previous target
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Target_PropertyChanging(sender As Object, e As ormDataObjectEntryEventArgs) Handles Me.OnEntryChanging
            If e.ObjectEntryName = constFNTarget Then
                Dim aoldDate As Date? = GetValue(constFNTarget)
                If aoldDate.HasValue Then SetValue(constFNPrevTarget, aoldDate.Value)
            End If
        End Sub
    End Class


    ''' <summary>
    ''' deliverable track class
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=Track.ConstObjectID, description:="tracking status of a deliverable per target and schedule", _
        modulename:=ConstModuleDeliverables, Version:=1, useCache:=True, adddeletefieldbehavior:=True, addsparefieldsbehavior:=True)> Public Class Track
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable
        Implements iormCloneable(Of Track)


        Public Const ConstObjectID = "Track"
        '** Table
        <ormSchemaTable(version:=2)> Public Const ConstTableID = "tblDeliverableTracks"
        '** Index
        <ormSchemaIndex(columnname1:=ConstFNWorkspace, columnname2:=constFNDeliverableUid, columnname3:=constFNScheduleUid, columnname4:=constFNScheduleUpdc, columnname5:=constFNTargetUpdc)> _
        Public Const constIndWSpace = "indWorkspace"

        '** primary keys
        <ormObjectEntry(referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNUid, primarykeyordinal:=1, _
            XID:="DTR2", aliases:={"UID"})> Public Const constFNDeliverableUid = Deliverable.constFNUid

        <ormObjectEntry(referenceobjectentry:=ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNUid, primarykeyordinal:=2, _
             XID:="DTR3")> Public Const constFNScheduleUid = "suid"
        <ormObjectEntry(referenceobjectentry:=ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNUpdc, primarykeyordinal:=3, _
           XID:="DTR4")> Public Const constFNScheduleUpdc = "supdc"
        '**
        <ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            entrynames:={constFNScheduleUid, constFNScheduleUpdc}, _
            foreignkeyreferences:={ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNUid, _
            ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNUpdc})> _
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

        <ormObjectEntry(referenceobjectentry:=Scheduledefinition.ConstObjectID & "." & Scheduledefinition.ConstFNType, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
             foreignkeyProperties:={ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")"}, _
             XID:="DTR6", aliases:={"SC14"}, isnullable:=True)> Public Const ConstFNTypeid = ScheduleEdition.ConstFNTypeid

        <ormObjectEntry(referenceobjectentry:=ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNPlanRev, _
          XID:="DTR7", aliases:={"SC5"}, isnullable:=True)> Public Const ConstFNScheduleRevision = ScheduleEdition.ConstFNPlanRev

        <ormObjectEntry(referenceobjectentry:=Target.ConstObjectID & "." & Target.ConstFNRevision, title:="target revision", Description:="revision of the target", _
          XID:="DTR8", aliases:={"DT4"}, isnullable:=True)> Public Const ConstFNTargetRevision = "trev"

        <ormObjectEntry(referenceobjectentry:=ScheduleMilestone.ConstObjectID & "." & ScheduleMilestone.ConstFNID, _
            title:="milestone ID delivered", Description:="schedule definition milestone ID for fc delivered", _
            XID:="DTR9", isnullable:=True)> Public Const ConstFNMSIDDelivered = "msfinid"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="current forecast", Description:="forecast date for deliverable delivered", _
            XID:="DTR10", isnullable:=True)> Public Const ConstFNForecast = "fcdate"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="current target", Description:="target date for deliverable", _
            XID:="DTR11", isnullable:=True, ALIASes:={"DT6"})> Public Const ConstFNCurTargetDate = "targetdate"

        <ormObjectEntry(referenceobjectentry:=Target.ConstObjectID & "." & Target.ConstFNNoTarget, dbdefaultvalue:="1", defaultvalue:=True, _
            XID:="DTR28", aliases:={"DT2"})> Public Const constFNNoTarget = Target.ConstFNNoTarget

        <ormObjectEntry(referenceobjectentry:=ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNlcstatus, _
            XID:="DTR12", aliases:={"SC7"}, isnullable:=True)> Public Const ConstFNLCStatus = ScheduleEdition.ConstFNlcstatus

        <ormObjectEntry(referenceobjectentry:=ScheduleEdition.ConstObjectID & "." & ScheduleEdition.ConstFNpstatus, _
            XID:="DTR13", aliases:={"SC8"}, isnullable:=True)> Public Const ConstFNPStatus = ScheduleEdition.ConstFNpstatus

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, title:="Synchro status", Description:="schedule synchro status", _
            XID:="DTR14", isnullable:=True)> Public Const ConstFNSyncStatus = "sync"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Synchro check date", Description:="date of last synchro check status", _
            XID:="DTR15", isnullable:=True)> Public Const ConstFNSyncDate = "syncchkon"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Going Alive Date", Description:="date of schedule going alive", _
           XID:="DTR16", isnullable:=True)> Public Const ConstFNGoingAliveDate = "goal"

        <ormObjectEntry(Datatype:=otDataType.Bool, title:="Delivered", defaultvalue:=False, dbdefaultvalue:="0", _
            Description:="True if deliverable is delivered", XID:="DTR17")> Public Const ConstFNIsFinished = "isfinished"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Blocking Item Reference", description:="Blocking Item Reference id for the deliverable", XID:="DTR18", aliases:={"DLV17"})> _
        Public Const constFNBlockingItemReference = Deliverable.constFNBlockingItemReference

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Delivery Date", Description:="date for deliverable to be delivered / finished", _
          XID:="DTR19", isnullable:=True)> Public Const ConstFNFinishedOn = "finish"

        <ormObjectEntry(Datatype:=otDataType.Long, title:="Forecast Gap", isnullable:=True, Description:="gap in working days between forecast and target", _
         XID:="DTR20")> Public Const constFNFCGap = "fcgap"

        <ormObjectEntry(Datatype:=otDataType.Long, title:="BaseLine Gap", isnullable:=True, Description:="gap in working days between forecast and target", _
         XID:="DTR21")> Public Const constFNBLGap = "blgap"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Schedule Change Date", isnullable:=True, Description:="forecast last changed on", _
          XID:="DTR23")> Public Const constFNFcChanged = "fcchanged"

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Baseline Delivery Date", isnullable:=True, Description:="delivery date from the baseline", _
          XID:="DTR24")> Public Const ConstFNBaselineFinish = "basefinish"

        <ormObjectEntry(Datatype:=otDataType.Bool, title:="Schedule Frozen", defaultvalue:=False, dbdefaultvalue:="0", _
            Description:="True if schedule is frozen / a baseline exists", XID:="DTR25", aliases:={"SC6"})> Public Const constFNIsFrozen = ScheduleEdition.ConstFNisfrozen

        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, title:="Schedule Baseline UpdateCount", description:="update count of the schedule", _
            XID:="DTR26", aliases:={"SC17"})> Public Const constFNBaselineUPDC = ScheduleEdition.ConstFNBlUpdc

        <ormObjectEntry(Datatype:=otDataType.Date, title:="Baseline Reference Date", Description:="reference date for baseline", _
         XID:="DTR27", isnullable:=True)> Public Const ConstFNBaseLineFrom = ScheduleEdition.ConstFNBlDate

        <ormObjectEntry(referenceobjectentry:=StatusItem.ConstObjectID & "." & StatusItem.constFNCode, _
            Title:="Tracking Status", description:="status of the tracking of schedule against target", _
           XID:="DTR30", isnullable:=True)> Public Const ConstFNStatus = "STATUS"

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
        <ormEntryMapping(EntryName:=constFNNoTarget)> Private _noTargetByIntention As Boolean
        <ormEntryMapping(EntryName:=ConstFNStatus)> Private _TStatus As String
        ''' <summary>
        ''' Relation to ScheduleDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ScheduleDefinition), toprimaryKeys:={ConstFNTypeid}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRScheduledefinition = "RelScheduleDefinition"

        <ormEntryMapping(relationName:=ConstRScheduledefinition, infusemode:=otInfuseMode.OnDemand)> Private _scheduledefinition As ScheduleDefinition

        ''' <summary>
        ''' Relation to ScheduleEdition
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ScheduleEdition), toprimaryKeys:={constFNScheduleUid, constFNScheduleUpdc}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRScheduleEdition = "RelScheduleEdition"

        <ormEntryMapping(relationName:=ConstRScheduleEdition, infusemode:=otInfuseMode.OnDemand)> Private _schedule As ScheduleEdition

        ''' <summary>
        ''' Relation to Target
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Target), toprimaryKeys:={constFNDeliverableUid, constFNTargetUpdc}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRTarget = "RelTarget"

        <ormEntryMapping(relationName:=ConstRTarget, infusemode:=otInfuseMode.OnDemand)> Private _Target As Target

        ''' <summary>
        ''' Relation to Deliverable
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Deliverable), toprimaryKeys:={constFNDeliverableUid}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRDeliverable = "RelDeliverable"

        <ormEntryMapping(relationName:=ConstRDeliverable, infusemode:=otInfuseMode.OnDemand)> Private _deliverable As Deliverable ' backlink


#Region "Properties"

        ''' <summary>
        ''' Gets or sets the Tracking Status
        ''' </summary>
        ''' <value>The T status.</value>
        Public Property TrackingStatus() As String
            Get
                Return Me._TStatus
            End Get
            Private Set(value As String)
                SetValue(ConstFNStatus, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the no target by intention.
        ''' </summary>
        ''' <value>The no target by intention.</value>
        Public Property NoTargetByIntention() As Boolean
            Get
                Return Me._noTargetByIntention
            End Get
            Set(value As Boolean)
                SetValue(constFNNoTarget, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets  scheduledefinition.
        ''' </summary>
        ''' <value>The scheduledefinition.</value>
        Public ReadOnly Property Scheduledefinition() As ScheduleDefinition
            Get
                If Me.GetRelationStatus(ConstRScheduledefinition) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRScheduledefinition)
                Return Me._scheduledefinition
            End Get

        End Property

        ''' <summary>
        ''' Gets  the schedule.
        ''' </summary>
        ''' <value>The schedule.</value>
        Public ReadOnly Property ScheduleEdition() As ScheduleEdition
            Get
                If Me.GetRelationStatus(ConstRScheduleEdition) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRScheduleEdition)
                Return Me._schedule
            End Get

        End Property

        ''' <summary>
        ''' Gets  the target.
        ''' </summary>
        ''' <value>The target.</value>
        Public ReadOnly Property Target() As Target
            Get
                If Me.GetRelationStatus(ConstRTarget) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRTarget)
                Return Me._Target
            End Get

        End Property

        ''' <summary>
        ''' Gets  the deliverable.
        ''' </summary>
        ''' <value>The deliverable.</value>
        Public ReadOnly Property Deliverable() As Deliverable
            Get
                If Me.GetRelationStatus(ConstRDeliverable) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRDeliverable)
                Return Me._deliverable
            End Get

        End Property

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
        Public Property ForecastChangedOn() As Date?
            Get
                Return _FClastchangeDate
            End Get
            Set(value As Date?)
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
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim aWorkspace As Workspace = Workspace.Retrieve(id:=workspaceID)
            If aWorkspace Is Nothing Then
                Call CoreMessageHandler(message:="workspaceID '" & workspaceID & "' is not defined", subname:="Track.UpdateAllTrack", _
                                        showmsgbox:=True, _
                                        messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            Dim aTarget As New Target
            Dim aCurrTarget As New WorkspaceTarget
            Dim aDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim aScheduleEdition As New ScheduleEdition
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

                    ''' Update the Tracking of the Deliverable
                    '''
                    UpdateFromDeliverable(aDeliverable)

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
        ''' <summary>
        ''' retrieves a List of all Tracks by deliverable UID and as option by schedule uid / updc and targetupdc
        ''' </summary>
        ''' <param name="deliverableUID"></param>
        ''' <param name="scheduleUPDC"></param>
        ''' <param name="targetUPDC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByDeliverable(deliverableUID As Long, _
                                                 Optional ByVal scheduleUID As Long = -1, _
                                        Optional ByVal scheduleUPDC As Long = -1, _
                                        Optional ByVal targetUPDC As Long = -1) As iormRelationalCollection(Of Track)

            Dim aCollection As ormRelationCollection(Of Track) = New ormRelationCollection(Of Track)(container:=Nothing, _
                                keyentrynames:={Track.constFNDeliverableUid, Track.constFNScheduleUid, Track.constFNScheduleUpdc, Track.constFNTargetUpdc})

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
                        Dim aTrack As New Track
                        If InfuseDataObject(record:=aRecord, dataobject:=aTrack) Then
                            If (scheduleUID <= 0 OrElse _
                                (scheduleUID = aTrack.ScheduleUID AndAlso (scheduleUPDC > 0 AndAlso scheduleUPDC = aTrack.ScheduleUPDC))) _
                             AndAlso (targetUPDC <= 0 OrElse targetUPDC = aTrack.TargetUPDC) Then
                                aCollection.Add(item:=aTrack)
                            End If
                        End If
                    Next aRecord
                    Return aCollection
                End If

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Track.AllByDeliverable", objectname:=ConstObjectID, tablename:=ConstTableID, messagetype:=otCoreMessageType.InternalException)
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
        Public Overloads Shared Function Create(ByVal deliverableUID As Long, ByVal scheduleUID As Long, ByVal scheduleUPDC As Long, ByVal targetUPDC As Long, _
                                                Optional domainid As String = Nothing) As Track
            Dim pkarray() As Object = {deliverableUID, scheduleUID, scheduleUPDC, targetUPDC}
            If String.IsNullOrWhiteSpace(domainid) Then domainid = CurrentSession.CurrentDomainID
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(constFNDeliverableUid, deliverableUID)
                .SetValue(constFNScheduleUid, scheduleUID)
                .SetValue(constFNScheduleUpdc, scheduleUPDC)
                .SetValue(constFNTargetUpdc, targetUPDC)
                .SetValue(ConstFNDomainID, domainid)
            End With
            Return ormDataObject.CreateDataObject(Of Track)(aRecord, domainID:=domainid, checkUnique:=True)
        End Function

        ''' <summary>
        ''' clone the track
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkarray() As Object, Optional runtimeOnly As Boolean? = Nothing) As Track Implements iormCloneable(Of Track).Clone
            Return MyBase.Clone(Of Track)(pkarray)
        End Function
        ''' <summary>
        ''' clone the deliverable track
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal deliverableUID As Long, ByVal scheduleUID As Long, ByVal scheduleUPDC As Long, ByVal targetUPDC As Long) As Track
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


        ''' <summary>
        ''' OnPersisted Event Handler: Checks which objects are persisted and if these are Deliverable, Target, ScheduleEdition
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Shared Sub Track_OnPersisted(sender As Object, e As ormDataObjectEventArgs)
            Dim aTrack As Track
            Dim aTarget As Target
            Dim aDeliverable As Deliverable
            Dim aScheduleEdition As ScheduleEdition

            Select Case sender.GetType

                '''
                ''' On Persisting a Deliverable
                ''' 
                Case GetType(Deliverables.Deliverable)

                    '''
                    ''' On Peristing a Target
                    ''' 
                Case GetType(Deliverables.Target)
                    aTarget = TryCast(e.DataObject, Target)
                    aDeliverable = Deliverable.Retrieve(uid:=aTarget.UID)
                    If aDeliverable IsNot Nothing Then
                        aScheduleEdition = aDeliverable.GetWorkScheduleEdition(aTarget.WorkspaceID)
                        If aScheduleEdition IsNot Nothing Then
                            aTrack = Track.Retrieve(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                            If aTrack Is Nothing Then
                                aTrack = Track.Create(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc, domainid:=aDeliverable.DomainID)
                            End If
                            If aTrack IsNot Nothing Then
                                '** save only if the dependend objects have been saved
                                aTrack.UpdateTracking(persist:=aTarget.IsLoaded And aScheduleEdition.IsLoaded, checkGAP:=True)
                            End If
                        Else
                            CoreMessageHandler(message:="deliverable has no working scheduling edition", arg1:=aTarget.UID, messagetype:=otCoreMessageType.InternalWarning, _
                                                subname:="Track.ClassOnPersisted")
                        End If
                    End If

                    '''
                    ''' On Peristing a ScheduleEdition
                Case GetType(Scheduling.ScheduleEdition)
                    aScheduleEdition = TryCast(e.DataObject, ScheduleEdition)
                    Dim aLink As ScheduleLink = ScheduleLink.RetrieveDeliverableLinkTo(scheduleUID:=aScheduleEdition.Uid)
                    If aLink IsNot Nothing Then
                        aDeliverable = Deliverable.Retrieve(uid:=aLink.FromUID)
                        If aDeliverable IsNot Nothing Then
                            aTarget = aDeliverable.GetTarget(aScheduleEdition.WorkspaceID)
                            If aTarget IsNot Nothing Then
                                aTrack = Track.Retrieve(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                                If aTrack Is Nothing Then
                                    aTrack = Track.Create(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc, domainid:=aDeliverable.DomainID)
                                End If
                                If aTrack IsNot Nothing Then
                                    '** save only if the depend objects have been saved !
                                    aTrack.UpdateTracking(persist:=aTarget.IsLoaded And aScheduleEdition.IsLoaded, checkGAP:=True)
                                End If
                            Else
                                CoreMessageHandler(message:="deliverable has no target", arg1:=aTarget.UID, messagetype:=otCoreMessageType.InternalWarning, _
                                                    subname:="Track.ClassOnPersisted")
                            End If
                        End If
                    End If

            End Select
        End Sub
        ''' <summary>
        '''  updateFromTarget -> update a Track from a given Target
        ''' </summary>
        ''' <param name="TARGET"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="PERSIST"></param>
        ''' <param name="checkGAP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateTracking(Optional ByVal workspaceID As String = "", _
                            Optional ByVal persist As Boolean = True, _
                            Optional ByVal checkGAP As Boolean = True) As Boolean

            If Not Me.IsAlive(subname:="UpdateTracking") Then Return False
            ' workspaceID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            Try
                With Me
                    If Me.DeliverableUID = 2 Then
                        Debug.Write("")

                    End If
                    ''' target
                    ''' 
                    .TargetRevision = Me.Target.Revision
                    .CurrentTargetDate = Me.Target.Target
                    .NoTargetByIntention = Me.Target.NotargetByItention

                    ''' schedule
                    ''' 
                    .workspaceID = Me.ScheduleEdition.WorkspaceID
                    .Scheduletype = Me.ScheduleEdition.Typeid
                    .ScheduleRevision = Me.ScheduleEdition.Revision
                    .IsFrozen = Me.ScheduleEdition.IsFrozen
                    .IsFinished = Me.ScheduleEdition.IsFinished
                    .FCLCStatus = Me.ScheduleEdition.LifeCycleStatusCode
                    .ProcessStatus = Me.ScheduleEdition.ProcessStatusCode
                    If Me.ScheduleEdition.IsFrozen Then .GoingAliveDate = Me.ScheduleEdition.CreatedOn
                    .ForecastChangedOn = Me.ScheduleEdition.LastForecastUpdate
                    .FinishedOn = Me.ScheduleEdition.FinishedOn
                    .IsFinished = Me.ScheduleEdition.IsFinished
                    .CurrentForecast = Me.ScheduleEdition.FinishOn
                    Dim FinishIDs As String() = Me.ScheduleEdition.ScheduleDefinition.GetFCFinishID
                    If FinishIDs Is Nothing OrElse FinishIDs.Count = 0 Then
                        CoreMessageHandler(message:="schedule definition has no finish milestones", arg1:=Me.ScheduleEdition.Typeid, subname:="Track.UpdateTracking", _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Else
                        .MSIDFinish = Me.ScheduleEdition.ScheduleDefinition.GetFCFinishID.First
                    End If

                    '''
                    ''' calculate the gap
                    If checkGAP Then .CheckOnGap()

                    ''' baseline
                    ''' 
                    If Me.ScheduleEdition.IsBaseline Then
                        .BaseLineFinishDate = Me.ScheduleEdition.GetMilestoneValue(.MSIDFinish)
                        .BaseLineFinishDateFrom = Me.ScheduleEdition.CreatedOn
                        .BaseLineUPDC = Me.ScheduleEdition.Updc
                        If checkGAP Then .CheckOnBaselineGap()
                    End If




                End With

                '''
                ''' persist
                If persist And Me.IsChanged Then
                    Return Me.Persist
                Else
                    Return True
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="Track.UpdateTracking")
                Return False
            End Try


        End Function


        ''' <summary>
        ''' updateFromDeliverable -> updated a Track from a given deliverable
        ''' </summary>
        ''' <param name="DELIVERABLE"></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="checkGAP"></param>
        ''' <param name="PERSIST"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function UpdateFromDeliverable(ByRef deliverable As Deliverable, _
                                        Optional ByVal checkGAP As Boolean = False, _
                                        Optional ByVal persist As Boolean = True _
                                        ) As Boolean



            Dim aTarget As Target
            Dim aTrack As Track
            Dim aScheduleEdition As ScheduleEdition
            Dim aWorkspaceSchedule As WorkspaceSchedule

            For Each anWorkspace In Commons.Workspace.All

                ''' get the target
                ''' 
                aTarget = deliverable.GetTarget(workspaceID:=anWorkspace.ID)

                ''' get the schedule edition
                '''
                aWorkspaceSchedule = deliverable.GetWorkspaceSchedule(workspaceID:=anWorkspace.ID)

                ''' 
                If aTarget.WorkspaceID = anWorkspace.ID OrElse anWorkspace.ID = aWorkspaceSchedule.WorkspaceID Then

                    ''' Track the Alive Edition
                    ''' 
                    aScheduleEdition = aWorkspaceSchedule.AliveEdition()
                    If aScheduleEdition IsNot Nothing Then
                        aTrack = Track.Retrieve(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                        If aTrack Is Nothing Then
                            aTrack = Track.Create(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                        End If
                        If aTrack IsNot Nothing Then
                            aTrack.UpdateTracking(persist:=persist, checkGAP:=checkGAP)
                        End If
                    End If

                    ''' Track the Working edition
                    ''' 
                    aScheduleEdition = aWorkspaceSchedule.WorkingEdition()
                    If aScheduleEdition IsNot Nothing Then
                        aTrack = Track.Retrieve(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                        If aTrack Is Nothing Then
                            aTrack = Track.Create(deliverableUID:=aTarget.UID, targetUPDC:=aTarget.UPDC, scheduleUID:=aScheduleEdition.Uid, scheduleUPDC:=aScheduleEdition.Updc)
                        End If
                        If aTrack IsNot Nothing Then
                            aTrack.UpdateTracking(persist:=persist, checkGAP:=checkGAP)
                        End If
                    Else
                        CoreMessageHandler(message:="deliverable has no working scheduling edition", arg1:=aTarget.UID, messagetype:=otCoreMessageType.InternalWarning, _
                                            subname:="Track.UpdateFromDeliverabble")
                    End If
                End If

            Next

            Return True
        End Function


        ''' <summary>
        ''' checkOnGAP -> Calculate the GAP
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckOnGap() As Boolean
            Dim aCE As New CalendarEntry
            Dim aDefScheduleMS As ScheduleMilestoneDefinition
            Dim aDate As Date?
            Dim aTargetDate As Date?
            Dim actual As String
            Dim gap As Long

            If Not Me.IsAlive(subname:="CheckOnGap") Then Return False

            ''' check on Target
            ''' 
            If Not Me.NoTargetByIntention AndAlso (Me.CurrentTargetDate = constNullDate OrElse Me.CurrentTargetDate Is Nothing) Then
                If Me.Target Is Nothing Then
                    Return False
                Else
                    Me.CurrentTargetDate = Me.Target.Target
                    Me.NoTargetByIntention = Me.Target.NotargetByItention
                End If
            End If



            ''' check on Finish
            ''' 
            If Not Me.IsFinished AndAlso (Me.CurrentForecast Is Nothing OrElse Me.CurrentForecast = constNullDate) Then
                If Me.ScheduleEdition Is Nothing Then
                    Return False
                ElseIf Me.ScheduleEdition.HasMilestoneDate(Me.MSIDFinish) Then
                    Me.CurrentForecast = Me.ScheduleEdition.GetMilestoneValue(Me.MSIDFinish)
                End If
            ElseIf Me.IsFinished AndAlso (Me.FinishedOn Is Nothing OrElse Me.FinishedOn = constNullDate) Then
                If Me.ScheduleEdition Is Nothing Then
                    Return False
                Else
                    aDefScheduleMS = Me.ScheduleEdition.GetScheduleMilestoneDefinition(Me.MSIDFinish)
                    actual = aDefScheduleMS.ActualOfFC
                    If Me.ScheduleEdition.HasMilestoneDate(actual) Then
                        Me.FinishedOn = Me.ScheduleEdition.GetMilestoneValue(Me.MSIDFinish)
                    End If
                End If
            End If

            '''
            ''' get the Date
            If Me.IsFinished Then
                aDate = Me.FinishedOn
            ElseIf Me.CurrentForecast < Date.Now() Then
                aDate = Date.Now()
            Else
                aDate = Me.CurrentForecast
            End If

            ''' set the target
            If Me.CurrentTargetDate IsNot Nothing Then
                aTargetDate = Me.CurrentTargetDate
            ElseIf Me.NoTargetByIntention Then
                aTargetDate = aDate
            Else
                ''' error condition !
                ''' 
            End If

            ''' calculate the gap
            ''' 
            If (aDate IsNot Nothing AndAlso aDate <> constNullDate) AndAlso (aTargetDate IsNot Nothing AndAlso aTargetDate <> constNullDate) Then
                aCE.Datevalue = aDate
                gap = aCE.DeltaDay(aTargetDate, considerAvailibilty:=True)
                Me.GAPToTarget = gap
                Return True
            Else
                ''' error condition !
                ''' 
                Me.GAPToTarget = 0
                Return False
            End If
        End Function

        ''' <summary>
        ''' checkOnBaselineGAP -> Calculate the baseline GAP
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckOnBaselineGap() As Boolean
            Dim aCE As New CalendarEntry
            Dim gap As Long
            Dim aDate As Date?
            Dim aTargetDate As Date?
            If Not Me.IsAlive(subname:="CheckOnBaselineGap") Then Return False

            ''' check on Target
            ''' 
            If Not Me.NoTargetByIntention AndAlso (Me.CurrentTargetDate = constNullDate OrElse Me.CurrentTargetDate Is Nothing) Then
                If Me.Target Is Nothing Then
                    Return False
                Else
                    Me.CurrentTargetDate = Me.Target.Target
                    Me.NoTargetByIntention = Me.Target.NotargetByItention
                End If
            End If

            ''' set the target
            If Not Me.NoTargetByIntention Then
                aTargetDate = Me.CurrentTargetDate
                If aTargetDate Is Nothing Then
                    ''' error condition !
                    ''' 
                End If
            Else
                aTargetDate = aDate
            End If

            ''' calculate the gap
            ''' 
            If (Me.BaseLineFinishDate IsNot Nothing AndAlso Me.BaseLineFinishDate <> constNullDate) AndAlso (aTargetDate IsNot Nothing AndAlso aTargetDate <> constNullDate) Then
                aCE.Datevalue = Me.BaseLineFinishDate
                gap = aCE.DeltaDay(aTargetDate, considerAvailibilty:=True)
                Me.BaselineGAPToTarget = gap
                Return True
            Else
                ''' error condition !
                ''' 
                Me.BaselineGAPToTarget = 0
                Return False
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
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primarykeyordinal:=1, _
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
        ''' 

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultValue:=False, _
         title:="scheduled", description:="deliverable is always scheduled", XID:="DLVT20")> Public Const ConstFNScheduled = "isscheduled"

        <ormObjectEntry(referenceobjectentry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, isnullable:=True, _
            title:="Schedule Type", description:="default schedule type of the deliverable", XID:="DLVT21")> Public Const constFNDefScheduleType = "defscheduletype"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            title:="Organization Unit", description:="default organization unit responsible of the deliverable", XID:="DLVT22")> Public Const constFNDefRespOU = "defrespOU"


        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, isnullable:=True, isnullable:=True, _
           title:="Function", description:="default function type of the deliverable", XID:="DLVT23")> Public Const constFNDefFunction = "deffunction"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
          title:="Responsible Unit Target", description:="default target responsible organization Unit", XID:="DLVT24")> Public Const constFNDefTargetOU = "deftargetOu"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultValue:=False, _
          title:="Target Necessary", description:="has mandatory target data", XID:="DLVT25")> Public Const constFNhastarget = "hastargetdata"


        <ormObjectEntry(Datatype:=otDataType.Bool, defaultValue:=False, _
          title:="Target Autopublish", description:="target will autopublish if changed", XID:="DLVT28")> Public Const ConstFNAutoPublish = "AUTOPUBLISH"


        <ormObjectEntry(referenceobjectentry:=ObjectPropertyValueLot.ConstObjectID & "." & ObjectPropertyValueLot.ConstFNSets, isnullable:=True, _
         title:="default property sets", description:="default property sets", XID:="DLVT26")> Public Const constFNdefSets = "defaultsetids"

        <ormObjectEntry(referenceobjectentry:=Deliverable.ConstObjectID & "." & Deliverable.constFNRevision, isnullable:=True, _
            title:="Revision", description:="default revision value of the deliverable", XID:="DLVT27")> Public Const constFNDefRevision = "defrev"


        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, _
         title:="Description", description:="description of the deliverable type", XID:="DLVT3")> Public Const constFNDescription = "desc"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, _
        title:="comment", description:="comments of the deliverable", XID:="DLVT10")> Public Const constFNComment = "cmt"

        '*** Mapping
        <ormEntryMapping(EntryName:=constFNTypeID)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=constFNDescription)> Private _description As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String
        <ormEntryMapping(EntryName:=constFNDefScheduleType)> Private _defScheduleType As String
        <ormEntryMapping(EntryName:=constFNDefFunction)> Private _deffunction As String
        <ormEntryMapping(EntryName:=constFNDefRespOU)> Private _defRespOU As String
        <ormEntryMapping(EntryName:=constFNDefTargetOU)> Private _defTargetOU As String
        <ormEntryMapping(EntryName:=constFNDefRevision)> Private _defRevision As String
        <ormEntryMapping(EntryName:=ConstFNScheduled)> Private _IsScheduled As Boolean = False
        <ormEntryMapping(EntryName:=constFNhastarget)> Private _mustHaveTarget As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNAutoPublish)> Private _TargetAutoPublish As Boolean = False
        <ormEntryMapping(EntryName:=constFNdefSets)> Private _defSets As String()

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the is scheduled.
        ''' </summary>
        ''' <value>The is scheduled.</value>
        Public Property IsScheduled() As Boolean
            Get
                Return Me._IsScheduled
            End Get
            Set(value As Boolean)
                Me._IsScheduled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the has alwasy target.
        ''' </summary>
        ''' <value>The has alwasy target.</value>
        Public Property MustHaveTarget() As Boolean
            Get
                Return Me._mustHaveTarget
            End Get
            Set(value As Boolean)
                SetValue(constFNhastarget, value)
            End Set
        End Property

        ''' <summary>
        ''' returns true if the Target is autopublished
        ''' </summary>
        ''' <value>The has alwasy target.</value>
        Public Property AutoPublishTarget() As Boolean
            Get
                Return Me._TargetAutoPublish
            End Get
            Set(value As Boolean)
                SetValue(ConstFNAutoPublish, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the default property sets
        ''' </summary>
        ''' <value>The def target OU.</value>
        Public Property DefaultPropertySets() As String()
            Get
                Return Me._defSets
            End Get
            Set(value As String())
                SetValue(constFNdefSets, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the default revision value
        ''' </summary>
        ''' <value>The def target OU.</value>
        Public Property DefaultRevision() As String
            Get
                Return Me._defRevision
            End Get
            Set(value As String)
                SetValue(constFNDefRevision, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the def target OU.
        ''' </summary>
        ''' <value>The def target OU.</value>
        Public Property DefaultTargetOU() As String
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
        Public Property DefaultRespOU() As String
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
        Public Property DefaultFunction() As String
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
        Public Property DefaultScheduleType() As String
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
        Public Shared Function Create(ByVal typeid As String, Optional ByVal domainid As String = Nothing) As DeliverableType
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {typeid, domainID}
            Return CreateDataObject(Of DeliverableType)(pkArray:=primarykey, domainID:=domainID, checkUnique:=True)
        End Function


        ''' <summary>
        ''' Retrieve a deliverable Type object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal typeid As String, Optional ByVal domainid As String = Nothing, Optional forcereload As Boolean = False) As DeliverableType
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {typeid, domainID}
            Return Retrieve(Of DeliverableType)(pkArray:=pkarray, forceReload:=forcereload)
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of Delivertype) for the DomainID
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainid As String = Nothing) As List(Of DeliverableType)
            Dim aCollection As New List(Of DeliverableType)
            Dim aDomainDir As New Dictionary(Of String, DeliverableType)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            '** set the domain
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

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
    ''' <remarks>
    ''' 
    ''' Design principles :
    ''' 
    ''' 1. always supply a deliverable type id for default values of schedule and property sets etc.
    ''' 
    ''' 2. Add Properties
    ''' 
    '''    aDeliverable = Deliverable.retrieve(uid:=xxx)
    '''    aDeliverable.GetProperties.AddSet("FBL_SBB") ' add the predefined set by name
    '''    aDeliverable.SetValue("BLTEST", "test4") ' sets the property value of BLTESt to test4
    '''    aDeliverable.Persist()
    ''' 
    ''' 3. Add Schedules
    ''' 
    '''   aDeliverable = Deliverable.create(uid:=xxx) ' will attachh a schedule of the default schedule type if deliverable is scheduled
    '''   aDeliverable.setvalue("bp1", #1/1/2014#)    ' set the milestone
    '''   aDeliverable.GetWorkspaceSchedule.persist
    ''' 
    '''   OR
    ''' 
    '''   dim aSchedule = new WorkspaceSchedule(scheduletype:=yyyy)
    '''   aSchedule, setMilestoneValue("bp1", #1/1/2014#)
    '''   aDeliverable.attachSchedule(aSchedule)
    '''   aSchedule.persists
    ''' </remarks>

    <ormObject(id:=Deliverable.ConstObjectID, description:="arbitrary object for tracking, scheduling, change and configuration mgmt.", _
        modulename:=ConstModuleDeliverables, useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False, Version:=1)> Public Class Deliverable
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable


        Public Const ConstObjectID = "DELIVERABLE"
        '** Table
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID = "tblDeliverables"

        '** indexes
        <ormSchemaIndex(columnName1:=ConstFNDomain, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const constIndexDomain = "indDomains"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=constFNfuid, columnname3:=ConstFNIsDeleted)> Public Const constIndexRevisions = "indRevisions"
        <ormSchemaIndex(columnName1:=constFNUid, columnname2:=ConstFNIsDeleted)> Public Const constIndexDelete = "indDeletes"
        <ormSchemaIndex(columnName1:=constFNPartID, columnname2:=ConstFNIsDeleted)> Public Const constIndexParts = "indParts"
        <ormSchemaIndex(columnName1:=constFNWBSID, columnname2:=constFNWBSCode, columnname3:=constFNUid, columnname4:=ConstFNIsDeleted)> Public Const constIndexWBS = "indWBS"
        <ormSchemaIndex(columnname1:=constFNMatchCode, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexMatchcode = "indmatchcode"
        <ormSchemaIndex(columnname1:=constFNCategory, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexcategory = "indcategory"
        <ormSchemaIndex(columnname1:=constFNFunction, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexFunction = "indFunction"
        <ormSchemaIndex(columnname1:=constFNDeliverableTypeID, columnname2:=constFNUid, columnname3:=ConstFNIsDeleted)> Public Const ConstIndexType = "indType"

        '*** primary key
        <ormObjectEntry(Datatype:=otDataType.Long, primarykeyordinal:=1, _
            title:="Unique ID", description:="unique id of the deliverable", XID:="DLV1", aliases:={"UID"})> _
        Public Const constFNUid = "uid"

        '** fields
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, _
            title:="category", description:="category of the deliverable", XID:="DLV2")> Public Const constFNCategory = "cat"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
            title:="id", description:="id of the deliverable", XID:="DLV3")> Public Const constFNDeliverableID = "id"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Matchcode", description:="match code of the deliverable", XID:="DLV4")> Public Const constFNMatchCode = "matchcode"


        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            isnullable:=True, _
            defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
        Public Const ConstFNDomain = "DOMAIN" '' different name since we donot want to get it deactivated due to missing domain behavior

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            description:="not used and should be not active", dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.None, enabled:=False)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID  '' const not overidable
        '
        <ormObjectEntry(referenceobjectentry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            Description:="workspaceID ID of the deliverable", dbdefaultvalue:="@", isnullable:=True, _
            useforeignkey:=otForeignKeyImplementation.ORM, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.SetDefault & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> Public Const ConstFNWorkspace = Workspace.ConstFNID

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Revision", description:="revision of the deliverable", XID:="DLV6")> Public Const constFNRevision = "drev"

        <ormObjectEntry(referenceobjectentry:=ConstObjectID & "." & constFNUid, title:="First Revision UID", description:="unique id of the first revision deliverable", _
            XID:="DLV7", isnullable:=True, aliases:={""})> Public Const constFNfuid = "fuid"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Change Reference", description:="change reference of the deliverable", XID:="DLV8")> Public Const constFNChangeRef = "chref"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
            title:="Format", description:="format of the deliverable", XID:="DLV9")> Public Const constFNFormat = "frmt"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
            title:="Description", description:="description of the deliverable", XID:="DLV10")> Public Const constFNDescription = "desc"

        <ormObjectEntry(referenceobjectentry:=OrgUnit.ConstObjectID & "." & OrgUnit.ConstFNID, isnullable:=True, _
            title:="Responsible OrgUnit", description:=" organization unit responsible for the deliverable", XID:="DLV11")> _
        Public Const constFNRespOU = "respou"

        <ormObjectEntry(referenceobjectentry:=Parts.Part.ConstObjectID & "." & Parts.Part.ConstFNPartID, _
            isnullable:=True, description:="part id of the deliverable", XID:="DLV12", _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFNPartID = Parts.Part.ConstFNPartID

        <ormObjectEntry(referenceobjectentry:=DeliverableType.ConstObjectID & "." & DeliverableType.constFNTypeID, _
            title:="Type", description:="type of the deliverable", XID:="DLV13", _
            LookupPropertyStrings:={LookupProperty.UseAttributeReference}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
            useforeignkey:=otForeignKeyImplementation.ORM)> Public Const constFNDeliverableTypeID = "typeid"

        <ormObjectEntry(referenceobjectentry:=Person.ConstObjectID & "." & Person.constFNID, _
            title:="Responsible", description:="responsible person for the deliverable", XID:="DLV16")> Public Const constFNResponsiblePerson = "resp"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
            title:="blocking item reference", description:="blocking item reference id for the deliverable", XID:="DLV17")> Public Const constFNBlockingItemReference = "blitemid"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, _
            title:="comment", description:="comments and extended description of the deliverable", XID:="DLV18")> Public Const constFNComment = "cmt"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
        title:="wbs reference", description:="work break down structure for the deliverable", XID:="DLV22")> _
        Public Const constFNWBSID = "wbs"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, isnullable:=True, _
        title:="wbscode reference", description:="wbscode for the deliverable", XID:="DLV23")> _
        Public Const constFNWBSCode = "wbscode"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, isnullable:=True, _
            title:="Function", description:="function of the deliverable", XID:="DLV30")> Public Const constFNFunction = "function"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, isnullable:=True, _
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
        <ormEntryMapping(EntryName:=ConstFNDomain)> Private _domainID As String
        'Private s_assycode As String = "" obsolete
        <ormEntryMapping(EntryName:=constFNPartID)> Private _partID As String
        <ormEntryMapping(EntryName:=constFNChangeRef)> Private _changerefID As String
        <ormEntryMapping(EntryName:=constFNDeliverableTypeID)> Private _typeid As String
        <ormEntryMapping(EntryName:=constFNResponsiblePerson)> Private _responsibleID As String
        <ormEntryMapping(EntryName:=constFNBlockingItemReference)> Private _blockingitemID As String
        <ormEntryMapping(EntryName:=constFNComment)> Private _comment As String

        <ormEntryMapping(EntryName:=constFNWBSID)> Private _wbsid As String
        <ormEntryMapping(EntryName:=constFNWBSCode)> Private _wbscode As String
        <ormEntryMapping(EntryName:=constFNFunction)> Private _function As String
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _wspaceID As String
        <ormEntryMapping(EntryName:=ConstFNWorkpackage)> Private _workpackage As String

        ''' <summary>
        ''' Relation to Responsible Person
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Commons.Person), toprimaryKeys:={constFNResponsiblePerson}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRRespPerson = "RELResponsiblePerson"

        <ormEntryMapping(relationName:=ConstRRespPerson, infusemode:=otInfuseMode.OnDemand)> Private _respPerson As Person

        ''' <summary>
        ''' Relation to Responsible OU
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Commons.OrgUnit), toprimaryKeys:={constFNRespOU}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRRespou = "RELRespOU"

        <ormEntryMapping(relationName:=ConstRRespou, infusemode:=otInfuseMode.OnDemand)> Private _respou As OrgUnit

        ''' <summary>
        ''' Relation to Parts
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Parts.Part), toprimaryKeys:={constFNPartID}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRPart = "RELPart"

        <ormEntryMapping(relationName:=ConstRPart, infusemode:=otInfuseMode.OnDemand)> Private _part As Part


        ''' <summary>
        ''' Relation to DeliverableType
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(DeliverableType), toprimaryKeys:={constFNDeliverableTypeID, ConstFNDomain}, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRDeliverableType = "RELDeliverableType"

        <ormEntryMapping(relationName:=ConstRDeliverableType, infusemode:=otInfuseMode.OnDemand)> Private _deliverableType As DeliverableType

        ''' <summary>
        ''' Relation to Deliverable Target - will be resolved by event handling
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(Deliverables.WorkspaceTarget), createobjectifnotretrieved:=True, _
                     cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=True)> _
        Public Const ConstRWorkspaceTarget = "RELWORKSPACETARGET"

        <ormEntryMapping(relationName:=ConstRWorkspaceTarget, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> Private _workspaceTarget As WorkspaceTarget

        ''' <summary>
        ''' Relation to PropertyLink - will be resolved via event handling from the relation manager
        ''' createObjectIfNotRetrieved is set -> always a Link + Lot
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ObjectPropertyLink), createobjectifnotretrieved:=True, _
                     cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=True)> _
        Public Const ConstRPropertyLink = "RELPROPERTYLINK"

        <ormEntryMapping(relationName:=ConstRPropertyLink, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> Private _propertyLink As ObjectPropertyLink

        ''' <summary>
        ''' Relation to ScheduleLink - will be resolved via event handling
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ScheduleLink), createobjectifnotretrieved:=True, _
                            cascadeonCreate:=True, cascadeOnDelete:=False, cascadeOnUpdate:=True)> _
        Public Const ConstRScheduleLink = "RELSCHEDULELINK"

        <ormEntryMapping(relationName:=ConstRScheduleLink, infusemode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> Private _scheduleLink As ScheduleLink

        ''' <summary>
        ''' Relation to Tracks - will be resolved via event handling
        ''' </summary>
        ''' <remarks>
        ''' track object is not finished add createobjectifnotretrieved:=True again if Track can build itself from otherobjects
        ''' </remarks>
        <ormRelation(linkObject:=GetType(Track), _
                            cascadeonCreate:=True, cascadeOnDelete:=True, cascadeOnUpdate:=False)> _
        Public Const ConstRTrack = "RELTRACK"

        <ormEntryMapping(relationName:=ConstRTrack, infusemode:=otInfuseMode.OnDemand)> Private _trackCollection As ormRelationCollection(Of Track) = _
        New ormRelationCollection(Of Track)(container:=Me, keyentrynames:={Track.constFNDeliverableUid, Track.constFNScheduleUid, Track.constFNScheduleUpdc, Track.constFNTargetUpdc})

        ''' <summary>
        ''' Operations
        ''' </summary>
        ''' <remarks></remarks>
        Public Const constOPGetPropertyValueLot = "GetPropertyValueLot"

        ''' <summary>
        ''' dynamic runtime members
        ''' </summary>
        ''' <remarks></remarks>

        Private _UniqueEntriesAreTouched As Boolean = False 'flag to raise if a unique entry check need to be done before persisting
        Private _UniqueEntries As String()

#Region "properties"

        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Property DomainID() As String
            Get
                Return Me._domainID
            End Get
            Set(value As String)
                SetValue(ConstFNDomain, value)
            End Set
        End Property

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
        ''' retrieves a PropertyLink object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property PropertyLink As ObjectPropertyLink
            Get
                If Not Me.IsAlive(subname:="PropertyLink") Then Return Nothing
                If Me.InfuseRelation(ConstRPropertyLink) Then
                    Return _propertyLink
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' retrieves a ScheduleLink object for this Deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ScheduleLink As ScheduleLink
            Get
                If Not Me.IsAlive(subname:="ScheduleLink") Then Return Nothing
                If _scheduleLink Is Nothing Then
                    Me.InfuseRelation(ConstRScheduleLink)
                End If

                Return _scheduleLink

            End Get
        End Property
        ''' <summary>
        ''' retrieves a DeliverableType object of this Deliverable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DeliverableType As DeliverableType
            Get
                If Not Me.IsAlive(subname:="DeliverableType") Then Return Nothing
                Me.InfuseRelation(ConstRDeliverableType)
                Return _deliverableType

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
        Public Property DeliverableTypeID() As String
            Get
                Return _typeid
            End Get
            Set(value As String)
                SetValue(constFNDeliverableTypeID, value)
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
        Public Property [Function]() As String
            Get
                Return _function
            End Get
            Set(value As String)
                SetValue(constFNFunction, value)
            End Set
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

        ''' <summary>
        ''' retrieve the related part object
        ''' </summary>
        ''' <returns>the part object</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Part() As Part
            Get
                If Not Me.IsAlive(subname:="Part") Then Return Nothing
                Me.InfuseRelation(ConstRPart)
                Return _part
            End Get
        End Property


#End Region


        ''' <summary>
        ''' returns the PropertyLink object in relation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(Description:="retrieves a property link object", operationname:=constOPGetPropertyValueLot)> _
        Public Function GetProperties() As ObjectProperties.ObjectPropertyValueLot
            If Not Me.IsAlive(subname:="GetPropertyValueLot") Then Return Nothing

            '''
            ''' get the link
            If Me.PropertyLink IsNot Nothing Then
                Return Me.PropertyLink.PropertyValueLot
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' OnInitialized Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Deliverable_OnInitialized(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnInitialized
            ''' initialize
            _UniqueEntries = CurrentSession.DeliverableUniqueEntries
        End Sub

        ''' <summary>
        ''' Event Handler for the RelationRetrieveNeeded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Deliverable_OnRelationRetrieveNeeded(sender As Object, e As ormDataObjectRelationEventArgs) Handles Me.OnRelationRetrieveNeeded
            If Not Me.IsAlive(subname:="Deliverable_OnRelationRetrieveNeeded") Then Return
            ''' check on PropertyLink
            If e.RelationID = ConstRPropertyLink Then
                Dim aPropertyLink As ObjectPropertyLink = ObjectPropertyLink.Retrieve(fromObjectID:=Deliverable.ConstObjectID, fromUid:=Me.Uid, fromUpdc:=0)
                If aPropertyLink IsNot Nothing Then
                    e.RelationObjects.Add(aPropertyLink)
                End If
                e.Finished = True


            ElseIf e.RelationID = ConstRScheduleLink Then
                Dim aScheduleLink As ScheduleLink = Scheduling.ScheduleLink.RetrieveDeliverableLinkFrom(deliverableUID:=Me.Uid)
                If aScheduleLink IsNot Nothing Then
                    e.RelationObjects.Add(aScheduleLink)
                End If
                e.Finished = True

            ElseIf e.RelationID = ConstRWorkspaceTarget Then
                ''' always gives the current workspace
                Dim aWorkspaceTarget As WorkspaceTarget = Deliverables.WorkspaceTarget.Retrieve(uid:=Me.Uid)
                If aWorkspaceTarget IsNot Nothing Then e.RelationObjects.Add(aWorkspaceTarget)
                e.Finished = True

            ElseIf e.RelationID = ConstRTrack Then
                Dim aCollection = Deliverables.Track.AllByDeliverable(deliverableUID:=Me.Uid)
                e.RelationObjects.AddRange(aCollection)
                e.Finished = True
            End If
        End Sub
        ''' <summary>
        ''' Event Handler for the RelationRetrieveNeeded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Deliverable_OnRelationCreateNeeded(sender As Object, e As ormDataObjectRelationEventArgs) Handles Me.OnRelationCreateNeeded
            If Not Me.IsAlive(subname:="Deliverable_OnRelationCreateNeeded") Then Return

            ''' check on PropertyLink
            ''' 
            If e.RelationID = ConstRPropertyLink Then
                Dim aPropertyLink As ObjectPropertyLink = ObjectPropertyLink.Create(fromObjectID:=Deliverable.ConstObjectID, fromuid:=Me.Uid, fromupdc:=0)
                If aPropertyLink Is Nothing Then aPropertyLink = ObjectPropertyLink.Retrieve(fromObjectID:=Deliverable.ConstObjectID, fromUid:=Me.Uid, fromUpdc:=0)
                Dim aPropertyLot As ObjectPropertyValueLot = ObjectPropertyValueLot.Create(domainid:=Me.DomainID)
                If aPropertyLink IsNot Nothing Then
                    aPropertyLink.ToUID = aPropertyLot.UID
                    aPropertyLink.ToUpdc = aPropertyLot.UPDC
                    ' we have what we need
                    e.RelationObjects.Add(aPropertyLink)
                    aPropertyLink.InfuseRelation(aPropertyLink.ConstRPropertyValueLot)
                    If Me.DeliverableType IsNot Nothing Then
                        For Each aSetid In Me.DeliverableType.DefaultPropertySets
                            aPropertyLot.AddSet(aSetid, domainid:=Me.DomainID)
                        Next
                    End If

                End If
                e.Finished = True

                ''' check on ScheduleLink
                ''' 
            ElseIf e.RelationID = ConstRScheduleLink Then
                Dim myself As Deliverable = TryCast(e.DataObject, Deliverable)
                ''' create the full path
                Dim aScheduletype As String
                Dim aWorkspaceID As String = CurrentSession.CurrentWorkspaceID ' could als be the workspaceID
                If myself.DeliverableType IsNot Nothing Then aScheduletype = myself.DeliverableType.DefaultScheduleType
                If aScheduletype Is Nothing Or aScheduletype = "" Then aScheduletype = CurrentSession.DefaultScheduleTypeID

                Dim aScheduleLink As ScheduleLink = Scheduling.ScheduleLink.RetrieveDeliverableLinkFrom(deliverableUID:=Me.Uid)
                If aScheduleLink Is Nothing Then
                    Dim aSchedule As WorkspaceSchedule = WorkspaceSchedule.Create(scheduletypeid:=myself.DeliverableType.DefaultScheduleType, domainid:=Me.DomainID, workspaceID:=aWorkspaceID)
                    aScheduleLink = Scheduling.ScheduleLink.Create(fromObjectID:=Me.ObjectID, fromuid:=Me.Uid, toScheduleUid:=aSchedule.UID)
                    ''' back to the ScheduleLink
                    If aScheduleLink IsNot Nothing Then
                        ' we have what we need
                        e.RelationObjects.Add(aScheduleLink)
                        e.Finished = True
                    End If
                Else
                    '' try to retrieve if there is already a link for some reasons
                    Dim aSchedule As WorkspaceSchedule = WorkspaceSchedule.Retrieve(UID:=aScheduleLink.ToUid, workspaceID:=aWorkspaceID)
                    '' create
                    If aSchedule Is Nothing Then
                        aSchedule = WorkspaceSchedule.Create(scheduletypeid:=aScheduletype, DomainID:=Me.DomainID, workspaceID:=aWorkspaceID)
                    End If
                    ''' back to the ScheduleLink
                    If aSchedule IsNot Nothing Then
                        ' we have what we need
                        e.RelationObjects.Add(aScheduleLink)
                        e.Finished = True
                    End If
                End If


                ''' Workspace Targets
                ''' 
            ElseIf e.RelationID = ConstRWorkspaceTarget Then
                Dim myself As Deliverable = TryCast(e.DataObject, Deliverable)
                ''' create the full path
                Dim needsTarget As Boolean?
                Dim defaultTargetOUT As String
                If myself.DeliverableType IsNot Nothing Then
                    needsTarget = myself.DeliverableType.MustHaveTarget
                    defaultTargetOUT = myself.DeliverableType.DefaultTargetOU
                End If

                ''' always gives the current workspace
                Dim aWorkspaceTarget As WorkspaceTarget = Deliverables.WorkspaceTarget.Create(uid:=Me.Uid, domainID:=Me.DomainID)
                If aWorkspaceTarget Is Nothing Then aWorkspaceTarget = Deliverables.WorkspaceTarget.Retrieve(uid:=Me.Uid)
                If aWorkspaceTarget IsNot Nothing AndAlso myself.DeliverableType IsNot Nothing Then
                    If aWorkspaceTarget.Target IsNot Nothing Then
                        aWorkspaceTarget.Target.ResponsibleOU = defaultTargetOUT
                        If needsTarget = False Then
                            aWorkspaceTarget.Target.NotargetByItention = True
                            aWorkspaceTarget.Target.Target = Nothing
                        End If

                    End If
                    ' done in the workspace target create relation handler event
                    'Dim aTarget As Target = Target.Create(uid:=Me.Uid)
                    'If aTarget IsNot Nothing Then
                    'aWorkspaceTarget.UPDC = aTarget.UPDC
                    '    If Not needsTarget Then aTarget.NotargetByItention = True
                    '    aTarget.ResponsibleOU = defaultTargetOUT
                    'End If
                    e.RelationObjects.Add(aWorkspaceTarget)
                    e.Finished = True
                End If

            ElseIf e.RelationID = ConstRTrack Then
                Throw New NotImplementedException

                If Me.GetRelationStatus(ConstRScheduleLink) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRScheduleLink)
                If Me.GetRelationStatus(ConstRWorkspaceTarget) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRWorkspaceTarget)

                Dim aSchedule = Me.GetWorkspaceSchedule()
                Dim aScheduleUPDC As Long?
                If aSchedule IsNot Nothing Then
                    If aSchedule.AliveEditionUpdc.HasValue Then
                        aScheduleUPDC = aSchedule.AliveEditionUpdc
                    ElseIf aSchedule.WorkingEditionUpdc.HasValue Then
                        aScheduleUPDC = aSchedule.WorkingEditionUpdc
                    End If

                    Dim aTarget = Me.GetTarget()

                    If aScheduleUPDC.HasValue AndAlso aTarget IsNot Nothing Then
                        Dim aTrack As Track = Track.Create(deliverableUID:=Me.Uid, scheduleUID:=aSchedule.UID, scheduleUPDC:=aSchedule.WorkingEditionUpdc, targetUPDC:=aTarget.UPDC, DomainID:=Me.DomainID)
                        Dim aCollection = Deliverables.Track.AllByDeliverable(deliverableUID:=Me.Uid)
                        e.RelationObjects.AddRange(aCollection)
                    End If

                    e.Finished = True
                End If

            End If


        End Sub
        ''' <summary>
        ''' Purge revisions of a deliverable
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Purge() As Boolean

            Dim otdbCol As Collection
            Dim aDelivTrack As New Track
            Dim aCurSchedule As New WorkspaceSchedule
            Dim aSchedule As New ScheduleEdition
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
                otdbCol = aDelivTrack.AllByDeliverable(Me.Uid)
                If Not otdbCol Is Nothing Then
                    For Each aDelivTrack In otdbCol
                        Call aDelivTrack.Delete()
                    Next aDelivTrack
                End If
                'delete the Schedule
                For Each aSchedule In aSchedule.AllByUID(Me.Uid)
                    Call aSchedule.Delete()
                Next aSchedule

                'delete the  object itself
                Me.IsDeleted = Me.Record.Delete()
                If Me.IsDeleted Then
                    Me.Unload()
                End If
                Purge = Me.IsDeleted
                Exit Function
            End If
        End Function



#Region "Static"
        ''' <summary>
        ''' retrieve maximum update count from the datastore
        ''' </summary>
        ''' <param name="max">the max to be set</param>
        ''' <param name="workspaceID">optional workspaceID</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function GenerateNewUID(ByRef newuid As Long, Optional domainid As String = Nothing) As Boolean
            Dim aDomain As Domain
            Dim mymax As Long


            '** default domain
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID


            Try
                ' get
                Dim aStore As iormDataStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="getnewUid", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = "max([" & constFNUid & "])"
                    aCommand.Where = "[" & ConstFNDomain & "] = @domain"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomain, tablename:=ConstTableID))
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
                                GenerateNewUID = True
                            ElseIf mymax < aDomain.MinDeliverableUID Then
                                Call CoreMessageHandler(showmsgbox:=False, message:="number range for deliverables in domain '" & domainID & "' is less than the min uid - new deliverable set to minimum ", _
                                                     arg1:=domainID, messagetype:=otCoreMessageType.InternalInfo)
                                mymax = aDomain.MinDeliverableUID
                                GenerateNewUID = True
                            End If
                        End If
                    Else
                        If aDomain IsNot Nothing Then
                            mymax = aDomain.MinDeliverableUID
                        Else
                            GenerateNewUID = False
                        End If

                    End If
                    GenerateNewUID = True

                Else
                    If aDomain IsNot Nothing Then
                        mymax = aDomain.MinDeliverableUID
                    Else
                        GenerateNewUID = False
                    End If
                End If
                If GenerateNewUID Then
                    newuid = mymax + 1
                End If
                Return GenerateNewUID
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, exception:=ex, subname:="Deliverable.getNewUID")
                Return False
            End Try
        End Function
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
        Public Shared Function All(Optional justdeleted As Boolean = False, Optional domainid As String = Nothing) As List(Of Deliverable)

            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            '** set the domain
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

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
        Public Shared Function AllByMatchcode(ByVal matchcode As String, Optional domainid As String = Nothing) As List(Of Deliverable)
            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            '** set the domain
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

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
        Public Shared Function AllByPnid(ByVal partid As String, Optional domainid As String = Nothing) As List(Of Deliverable)
            Dim aCollection As New List(Of Deliverable)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            '** set the domain
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

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


        '**** getthe Track
        '****
        Public Function GetTrack(Optional workspaceID As String = "", _
        Optional scheduleUID As Long = 0, _
        Optional scheduleUPDC As Long = 0, _
        Optional targetUPDC As Long = 0) As Track
            Dim aTrackDef As New Track
            Dim aCurrSCHEDULE As New WorkspaceSchedule
            Dim aCurrTarget As New WorkspaceTarget

            If IsLoaded Or IsCreated Then
                If scheduleUPDC = 0 Then
                    ' get
                    aCurrSCHEDULE = Me.GetWorkspaceSchedule(workspaceID:=workspaceID)
                    scheduleUPDC = aCurrSCHEDULE.AliveEditionUpdc
                End If
                If targetUPDC = 0 Then
                    aCurrTarget = Me.GetWorkspaceTarget(workspaceID)
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
        Public Function GetMatchcodes(ByRef list As IEnumerable, Optional domainid As String = Nothing) As Boolean
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore

            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

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
        ''' return the  current workspace target object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetWorkspaceTarget(Optional ByVal workspaceID As String = Nothing) As WorkspaceTarget
            If Not IsAlive(subname:="GetCurrTarget") Then Return Nothing
            If workspaceID IsNot Nothing Then workspaceID = CurrentSession.CurrentWorkspaceID
            If workspaceID <> CurrentSession.CurrentWorkspaceID Then
                _workspaceTarget = WorkspaceTarget.Retrieve(uid:=Me.Uid, workspaceID:=workspaceID)
            Else
                InfuseRelation(ConstRWorkspaceTarget)
            End If
            Return _workspaceTarget
        End Function
        ''' <summary>
        ''' retrieve the current workspace schedule object
        ''' </summary>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns>the data object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetWorkspaceSchedule(Optional ByVal workspaceID As String = "") As WorkspaceSchedule
            If Not IsAlive(subname:="GetCurrSchedule") Then Return Nothing
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            If Me.ScheduleLink IsNot Nothing Then Return Me.ScheduleLink.WorkspaceSchedule(workspaceid:=workspaceID)
            Return Nothing
        End Function

        ''' <summary>
        ''' adds / attaches a workspace schedule to this deliverable and setup the links
        ''' </summary>
        ''' <param name="workspaceSchedule"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AttachSchedule(workspaceSchedule As WorkspaceSchedule) As Boolean
            If Not Me.IsAlive("AtachWorkSchedule") Then Return False
            Try
                '' this can only be done once
                If Me.GetRelationStatus(ConstRScheduleLink) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRScheduleLink)
                If _scheduleLink Is Nothing Then _scheduleLink = Scheduling.ScheduleLink.Create(fromObjectID:=Me.ConstObjectID, fromuid:=Me.Uid, toScheduleUid:=workspaceSchedule.UID)
                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="Deliverable.AttachSchedule")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' retrieves the active and work schedule object for the deliverable 
        ''' </summary>
        ''' <param name="workspaceID">workspaceID id</param>
        ''' <returns>a scheduling object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetWorkScheduleEdition(Optional ByVal workspaceID As String = "") As ScheduleEdition
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If Not IsAlive(subname:="GetWorkScheduleEdition") Then Return Nothing

            ' get
            Dim aSchedule As WorkspaceSchedule = Me.GetWorkspaceSchedule(workspaceID:=workspaceID)
            If aSchedule IsNot Nothing Then Return aSchedule.WorkingEdition
            Return Nothing
        End Function

        ''' <summary>
        ''' retrieves the active and alive schedule object for the deliverable 
        ''' </summary>
        ''' <param name="workspaceID">workspaceID id</param>
        ''' <returns>a scheduling object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetAliveScheduleEdition(Optional ByVal workspaceID As String = "") As ScheduleEdition
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If Not IsAlive(subname:="GetAliveScheduleEdition") Then Return Nothing

            ' get
            Dim aSchedule As WorkspaceSchedule = Me.GetWorkspaceSchedule(workspaceID:=workspaceID)
            If aSchedule IsNot Nothing Then Return aSchedule.AliveEdition
            Return Nothing
        End Function

        ''' <summary>
        ''' retrieves the target object (most current)
        ''' </summary>
        ''' <param name="workspaceID">optional workspaceID id</param>
        ''' <returns>the data object or nothing</returns>
        ''' <remarks></remarks>
        Public Function GetTarget(Optional ByVal workspaceID As String = Nothing) As Target
            If Not IsAlive(subname:="GetTarget") Then Return Nothing
            Dim aWorkspaceTarget As WorkspaceTarget = Me.GetWorkspaceTarget(workspaceID:=workspaceID)
            If aWorkspaceTarget IsNot Nothing Then Return aWorkspaceTarget.WorkingTarget

            Return Nothing
        End Function

        ''' <summary>
        ''' On Entry Changed Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Deliverable_OnEntryChanged(sender As Object, e As ormDataObjectEntryEventArgs) Handles Me.OnEntryChanged

            If _UniqueEntries IsNot Nothing AndAlso _UniqueEntries.Length > 0 AndAlso _UniqueEntriesAreTouched = False Then
                If _UniqueEntries.Contains(e.ObjectEntryName) Then
                    _UniqueEntriesAreTouched = True
                End If
            End If

        End Sub

        ''' <summary>
        ''' handler for default Value for an entry needed
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Deliverable_OnDefaultValueNeeded(sender As Object, e As ormDataObjectEntryEventArgs) Handles Me.OnDefaultValueNeeded
            Select Case e.ObjectEntryName
                Case ConstFNDomain
                    e.Value = CurrentSession.CurrentDomainID
                    e.Result = True
                Case ConstFNWorkspace
                    e.Value = CurrentSession.CurrentWorkspaceID
                    e.Result = True
                Case constFNDeliverableTypeID
                    e.Value = CurrentSession.DefaultDeliverableTypeID
                    e.Result = True
            End Select
        End Sub
        ''' <summary>
        ''' Handles the On DefaultValues Needed
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Deliverable_OnCreateDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreateDefaultValuesNeeded
            '' set these Values too ...
            '' OnDefaultValuesNeeded is not called before this event

            Dim aValue As Object = e.Record.GetValue(ConstFNDomain)
            If aValue Is Nothing OrElse aValue = "" Then e.Record.SetValue(ConstFNDomain, CurrentSession.CurrentDomainID)
            aValue = e.Record.GetValue(ConstFNWorkspace)
            If aValue Is Nothing OrElse aValue = "" Then e.Record.SetValue(ConstFNWorkspace, CurrentSession.CurrentWorkspaceID)
            aValue = e.Record.GetValue(constFNDeliverableTypeID)
            If aValue Is Nothing OrElse aValue = "" Then
                If CurrentSession.DefaultDeliverableTypeID IsNot Nothing AndAlso CurrentSession.DefaultDeliverableTypeID <> "" Then
                    aValue = CurrentSession.DefaultDeliverableTypeID
                    e.Record.SetValue(constFNDeliverableTypeID, aValue)
                End If
            End If

            ''' Get the Values from the Type
            ''' 
            Dim domainID As String = e.Record.GetValue(ConstFNDomain)
            If domainID Is Nothing OrElse String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

            aValue = e.Record.GetValue(constFNDeliverableTypeID)
            If Not String.IsNullOrWhiteSpace(aValue) Then
                Dim aDeliverableType = DeliverableType.Retrieve(typeid:=aValue, domainID:=domainID)
                If aDeliverableType IsNot Nothing Then
                    With aDeliverableType
                        If e.Record.GetValue(constFNFunction) Is Nothing Then e.Record.SetValue(constFNFunction, .DefaultFunction)
                        If e.Record.GetValue(constFNRespOU) Is Nothing Then e.Record.SetValue(constFNRespOU, .DefaultRespOU)
                        If e.Record.GetValue(constFNRevision) Is Nothing Then e.Record.SetValue(constFNRevision, .DefaultRevision)
                    End With
                End If
            End If

        End Sub
        ''' <summary>
        ''' On Creating Handler to set the UID
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Deliverable_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnCreating
            Dim domainid As String = e.Record.GetValue(ConstFNDomain)
            If domainid Is Nothing Then domainid = CurrentSession.CurrentDomainID
            Dim uid As Long? = e.Record.GetValue(constFNUid)
            Dim aNewUid As Long
            ' get NEW UID
            If uid.HasValue OrElse uid = 0 Then
                If Not Me.GenerateNewUID(aNewUid, domainID:=domainid) Then
                    Call CoreMessageHandler(message:="could not generate new UID", subname:="Deliverable.OnCreating", _
                                            arg1:=uid, messagetype:=otCoreMessageType.InternalError)
                End If
                e.Record.SetValue(constFNUid, aNewUid)
            End If

        End Sub
        ''' <summary>
        ''' create unique persistable object by primary key
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(Optional ByVal uid As Long = 0, _
                                      Optional domainid As String = Nothing, _
                                      Optional workspaceID As String = "", _
                                      Optional typeid As String = "") As Deliverable
            Dim aRecord As New ormRecord
            '* defaults
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If typeid = "" Then typeid = CurrentSession.DefaultDeliverableTypeID

            With aRecord
                .SetValue(ConstFNDomain, domainID)
                .SetValue(ConstFNWorkspace, workspaceID)
                .SetValue(constFNUid, uid)
                .SetValue(constFNDeliverableTypeID, typeid)
            End With
            Return ormDataObject.CreateDataObject(Of Deliverable)(aRecord, domainID:=domainID, checkUnique:=True)
        End Function

        ''' <summary>
        ''' Handler for the On Cloned Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Deliverable_OnCloned(sender As Object, e As ormDataObjectCloneEventArgs) Handles Me.OnCloned

            Dim aDeliverableClone As Deliverable = TryCast(e.DataObject, Deliverable)
            ''' reset the entrys
            ''' 
            For Each anEntryname In CurrentSession.DeliverableOnCloningResetEntries
                Dim anEntry As iormObjectEntry = Me.ObjectDefinition.GetEntry(anEntryname)
                If anEntry Is Nothing Then
                    CoreMessageHandler(message:="Entry could not found", subname:="Deliverable_OnCloned", objectname:=Me.ObjectID, entryname:=anEntryname, _
                                        messagetype:=otCoreMessageType.ApplicationError)
                Else
                    ''' reset to default values 
                    ''' might fail since we are not calling OnCreateDefaultValuesNeeded (was called during called)
                    ''' 
                    Dim aValue As Object = Me.ObjectEntryDefaultValue(entryname:=anEntryname)
                    aDeliverableClone.SetValue(entryname:=anEntryname, value:=aValue)
                End If

            Next

            ''' clone also
            ''' 
            For Each anObjectID In CurrentSession.DeliverableOnCloningCloneAlso
                If anObjectID.ToUpper = ObjectProperties.ObjectPropertyValueLot.ConstObjectID.ToUpper Then
                    If Me.PropertyLink IsNot Nothing Then
                        Dim aPropertyValueLot As ObjectPropertyValueLot = aDeliverableClone.GetProperties
                        For Each aValue In Me.GetProperties.Values
                            aPropertyValueLot.SetPropertyValue(id:=aValue.PropertyID, value:=aValue.ValueString, domainid:=Me.DomainID)
                        Next
                    End If

                Else
                    CoreMessageHandler(message:="object id  not found", subname:="Deliverable_OnCloned", objectname:=anObjectID, _
                                      messagetype:=otCoreMessageType.ApplicationError)
                End If

            Next

            ''' take the first revision forward
            ''' 
            If Me.FirstRevisionUID.HasValue AndAlso Me.FirstRevisionUID <> 0 Then
                aDeliverableClone.FirstRevisionUID = Me.FirstRevisionUID
            End If
        End Sub


        ''' <summary>
        ''' Clone the deliverable
        ''' </summary>
        ''' <param name="UID">new uid If 0 then generate a new uid</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(Optional ByVal uid As Long = 0) As Deliverable
            Return Me.Clone(Of Deliverable)({uid})
        End Function


        ''' <summary>
        ''' Clone the deliverable to a revision
        ''' </summary>
        ''' <param name="UID">new uid If 0 then generate a new uid</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function AddRevisionClone(Optional ByVal uid As Long = 0) As Deliverable
            Dim aNewRevision = Me.Clone(uid:=uid)

            If aNewRevision IsNot Nothing Then
                If Me.FirstRevisionUID.HasValue AndAlso Me.FirstRevisionUID <> 0 Then
                    If Not aNewRevision.FirstRevisionUID.HasValue Then
                        aNewRevision.FirstRevisionUID = Me.FirstRevisionUID
                    End If
                Else
                    aNewRevision.FirstRevisionUID = Me.Uid
                End If
            End If

            Return aNewRevision
        End Function

        ''' <summary>
        ''' Check if the additional UniqueIDs are unique 
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CheckUniqueEntries(Optional msglog As ObjectMessageLog = Nothing) As Boolean
            If Not Me.IsAlive("CheckUniqueIDs") Then Return False
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog
            ''' no uniqueentries ?
            If _UniqueEntries Is Nothing OrElse _UniqueEntries.Count = 0 Then Return True


            ''' build a select
            ''' 
            Dim aStore As iormDataStore = Me.PrimaryTableStore
            Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand("UniqueEntryCheck", addAllFields:=False)

            If Not aCommand.Prepared Then
                aCommand.select = "[" & constFNUid & "]"
                For Each anEntryname In _UniqueEntries
                    If Not Me.ObjectDefinition.HasEntry(anEntryname) Then
                        CoreMessageHandler(message:="entry name is not defined for this object", entryname:=anEntryname, objectname:=Me.ObjectID, messagetype:=otCoreMessageType.ApplicationError)
                    Else
                        Dim anEntry As ObjectColumnEntry = Me.ObjectDefinition.GetEntry(anEntryname)
                        If Not String.IsNullOrWhiteSpace(aCommand.Where) Then aCommand.Where &= " AND "
                        aCommand.Where &= " ([" & anEntryname & "] = @" & anEntryname
                        aCommand.AddParameter(New ormSqlCommandParameter("@" & anEntryname, columnname:=anEntry.Columnname, tablename:=Me.PrimaryTableID))
                        If anEntry.IsNullable Then
                            aCommand.Where &= "  OR ([" & anEntryname & "] IS NULL and 1=@" & anEntryname & "flag))"
                            aCommand.AddParameter(New ormSqlCommandParameter("@" & anEntryname & "flag", notcolumn:=True, datatype:=otDataType.Long))

                        Else
                            aCommand.Where &= " ) "
                        End If
                    End If
                Next
            End If
            aCommand.Prepare()
            Dim values As New List(Of String)
            For Each anEntryname In _UniqueEntries
                If Not Me.ObjectDefinition.HasEntry(anEntryname) Then
                    CoreMessageHandler(message:="entry name is not defined for this object", entryname:=anEntryname, objectname:=Me.ObjectID, messagetype:=otCoreMessageType.ApplicationError)
                Else
                    Dim anEntry As ObjectColumnEntry = Me.ObjectDefinition.GetEntry(anEntryname)
                    Dim aValue As Object = Me.GetValue(anEntryname)
                    If anEntry.IsNullable And aValue Is Nothing Then
                        aCommand.SetParameterValue("@" & anEntryname & "flag", 1)
                    ElseIf anEntry.IsNullable And aValue IsNot Nothing Then
                        aCommand.SetParameterValue("@" & anEntryname & "flag", 0)
                    End If
                    ''' retrieve a default value even if nullable -> if null the select will fail once
                    If aValue Is Nothing Then aValue = ot.GetDefaultValue(anEntry.Datatype)
                    If aValue IsNot Nothing Then values.Add(aValue.ToString)
                    aCommand.SetParameterValue("@" & anEntryname, aValue)

                End If
            Next

            Dim aRecordCollection As List(Of ormRecord) = aCommand.RunSelect

            If aRecordCollection.Count = 0 Then Return True
            '1121;@;VALIDATOR;object validation for %1% failed. The values ('%3') of entries '%2%' must be unique.;Provide a correct value;90;Error;false;|R1|R1|;|OBJECTVALIDATOR|XCHANGEENVELOPE|
            msglog.Add(1121, Nothing, Nothing, Nothing, Nothing, Me, _
                       Me.ObjectID, Converter.Array2StringList(_UniqueEntries), Converter.Enumerable2StringList(values))
            Return False
        End Function
        ''' <summary>
        ''' Validated Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Deliverable_OnValidated(sender As Object, e As ormDataObjectValidationEventArgs) Handles Me.OnValidated

            ''' check if the additional unique IDs are touched
            ''' if then check the unique IDs are still unique
            If _UniqueEntriesAreTouched Then
                e.AbortOperation = Not Me.CheckUniqueEntries(e.Msglog)
                If e.AbortOperation Then e.ValidationResult = otValidationResultType.FailedNoProceed
            End If
        End Sub
        ''' <summary>
        ''' Event Handler for OnPersisted Event to reset the UniqueEntries are Touched Flag
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub Deliverable_OnPersisted(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnPersisted
            If Not e.AbortOperation AndAlso _UniqueEntriesAreTouched Then _UniqueEntriesAreTouched = False
        End Sub
    End Class
End Namespace
