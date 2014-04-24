
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs CLASSES: Schedules and Schedule Definitions
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
Imports System.Diagnostics.Debug

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.Deliverables
Imports OnTrack.XChange
Imports OnTrack.Calendar
Imports OnTrack.Commons

Namespace OnTrack.Scheduling
    ''' <summary>
    ''' enumeration of milestone types
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otMilestoneType
        [Date] = 1
        Status = 2
    End Enum
    ''' <summary>
    ''' Enumeration and other definitions
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otScheduleLinkType
        Deliverable = 1
    End Enum

    ''' <summary>
    ''' milestone definition class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(version:=1, ID:=MileStoneDefinition.ConstObjectID, Modulename:=ConstModuleScheduling, _
        Description:="definition of milestones for all schedule types", useCache:=True, addDomainBehavior:=True, adddeletefieldbehavior:=True)> _
    Public Class MileStoneDefinition
        Inherits ormDataObject


        Public Const ConstObjectID = "MilestoneDefinition"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefMilestones"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=20, defaultValue:="", primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
            XID:="bpd1", title:="ID", description:="id of the milestone")> Public Const ConstFNID = "id"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
           XID:="bpd2", title:="Description", description:="description of the milestone")> Public Const ConstFNDescription = "desc"

        <ormObjectEntry(typeid:=otDataType.Text, defaultvalue:=otMilestoneType.Date, _
           XID:="bpd3", title:="Type", description:="type of the milestone")> Public Const ConstFNType = "typeid"

        <ormObjectEntry(typeid:=otDataType.Text, defaultvalue:=otDataType.Text, _
           XID:="bpd4", title:="Datatype", description:="datatype of the milestone")> Public Const ConstFNDatatype = "datatype"

        <ormObjectEntry(referenceobjectentry:=StatusItem.ConstObjectID & "." & StatusItem.constFNType, _
          XID:="bpd5", title:="Status Item Type", description:="status item type of the milestone")> Public Const ConstFNStatusType = "status"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, _
         XID:="bpd6", title:="Forecast", description:="set if milestone is a forecast")> Public Const ConstFNIsForecast = "isforecast"

        <ormObjectEntry(referenceobjectentry:=ConstObjectID & "." & ConstFNID, isnullable:=True, _
        XID:="bpd7", title:="Reference", description:="set if milestone is a reference")> Public Const ConstFNRefID = "refid"



        '** MAPPING
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""  ' id
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNType)> Private _typeid As otMilestoneType
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType
        <ormEntryMapping(EntryName:=ConstFNRefID)> Private _refid As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsForecast)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNStatusType)> Private _statustypeid As String = ""



#Region "Properties"


        ''' <summary>
        ''' gets  the ID of the Milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                Return _id
            End Get

        End Property

        Public Property Datatype() As otDataType
            Get
                Datatype = _datatype
            End Get
            Set(value As otDataType)
                SetValue(ConstFNDatatype, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the type of the milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Typeid() As otMilestoneType
            Get
                Typeid = _typeid
            End Get
            Set(value As otMilestoneType)
                SetValue(ConstFNType, value)
            End Set
        End Property
        ''' <summary>
        ''' returns True if the Milestone is a date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOfDate() As Boolean
            Get
                If _typeid = otMilestoneType.Date Then
                    Return True
                Else
                    Return False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    Me.Typeid = otMilestoneType.Date
                Else
                    Me.Typeid = otMilestoneType.Status
                End If
            End Set
        End Property

        ''' <summary>
        ''' returns true if the milestone is a status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOfStatus() As Boolean
            Get
                If _typeid = otMilestoneType.Status Then
                    Return True
                Else
                    Return False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    Me.Typeid = otMilestoneType.Status
                Else
                    Me.Typeid = otMilestoneType.Date
                End If
            End Set
        End Property

        ''' <summary>
        ''' returns true if the milestone is an actual milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActual() As Boolean
            Get
                Return Not Me.IsForecast
            End Get
            Set(value As Boolean)
                Me.IsForecast = Not value
            End Set
        End Property
        ''' <summary>
        ''' gets or sets true if the milestone is a forecast
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsForecast() As Boolean
            Get
                Return _isForecast
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsForecast, value)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the description of the milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Return _description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the status type id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property statustypeid() As String
            Get
                Return _statustypeid
            End Get
            Set(value As String)
                SetValue(ConstFNStatusType, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the reference milestone for this milestone (actual <-> forecast)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property referingToID() As String
            Get
                Return _refid
            End Get
            Set(value As String)
                SetValue(ConstFNRefID, value)
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As MileStoneDefinition
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {id, domainID}
            Return Retrieve(Of MileStoneDefinition)(pkArray:=primarykey, domainID:=domainID, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' Return a collection of all def Milestones
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainID As String = "") As List(Of MileStoneDefinition)
            Return ormDataObject.AllDataObject(Of MileStoneDefinition)(domainID:=domainID)
        End Function

        ''' <summary>
        ''' create persistable object with primary key ID
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal ID As String, Optional domainID As String = "") As MileStoneDefinition
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {ID, domainID}
            Return ormDataObject.CreateDataObject(Of MileStoneDefinition)(pkarray, checkUnique:=True, domainID:=domainID)
        End Function

    End Class


    ''' <summary>
    ''' Definition of a  milestone per schedule definition class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleMilestoneDefinition.ConstObjectID, modulename:=ConstModuleScheduling, _
        Version:=1, description:="declaration of milestones specific in a schedule type", _
        addDomainBehavior:=True, adddeletefieldbehavior:=True, useCache:=True)> _
    Public Class ScheduleMilestoneDefinition
        Inherits ormDataObject

        Public Const ConstObjectID = "ScheduleMilestoneDefinition"

        ''' <summary>
        ''' table definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID As String = "tblDefScheduleMilestones"
        '*** Index
        <ormSchemaIndex(columnname1:=ConstFNType, columnname2:=ConstFNOrdinal)> Public Const ConstIndOrder = "orderby"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="SCT1", referenceobjectentry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, _
            primaryKeyordinal:=1, aliases:={"bs4"}, title:="schedule type", _
            description:=" type of schedule definition")> Public Const ConstFNType = "scheduletype"

        <ormObjectEntry(XID:="BPD1", referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, _
            primaryKeyordinal:=2, title:="milestone id", description:=" id of milestone in schedule")> Public Const ConstFNID = "id"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(XID:="BSD1", typeid:=otDataType.Text, isnullable:=True, _
            title:="description", description:="description of milestone in schedule")> Public Const ConstFNDesc = "desc"

        <ormObjectEntry(XID:="BSD2", typeid:=otDataType.Long, defaultvalue:=1, dbdefaultvalue:="1", _
            title:="ordinal", description:="ordinal of milestone in schedule")> Public Const ConstFNOrdinal = "ordinal"

        <ormObjectEntry(XID:="BSD3", isnullable:=True, _
            referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, _
            title:="actual of fc milestone id", description:=" actual id of this milestone in schedule")> Public Const ConstFNActualID = "actualid"

        <ormObjectEntry(XID:="BSD4", typeid:=otDataType.Bool, _
            title:="is forecast", description:=" milestone is forecast in schedule")> Public Const ConstFNIsFC = "isfc"

        <ormObjectEntry(XID:="BSD5", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is facilitative", description:=" milestone is facilitative in schedule")> Public Const ConstFNIsFacultative = "isfacultative"

        <ormObjectEntry(XID:="BSD6", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is prohibited", description:=" milestone is prohibited in schedule")> Public Const ConstFNIsProhibited = "isprohibited"

        <ormObjectEntry(XID:="BSD7", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is mandatory", description:=" milestone is mandatory in schedule")> Public Const ConstFNIsMandatory = "ismandatory"

        <ormObjectEntry(XID:="BSD8", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is input", description:=" milestone is input deliverable in schedule")> Public Const ConstFNIsINPUT = "isinput"

        <ormObjectEntry(XID:="BSD9", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is output", description:=" milestone is output deliverable in schedule")> Public Const ConstFNIsOutPut = "isoutput"

        <ormObjectEntry(XID:="BSD10", typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
            title:="is finish", description:=" milestone is end of schedule")> Public Const ConstFNIsFinish = "isfinish"

        ''' <summary>
        ''' Mapping
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNType)> Private _scheduletype As String = ""
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNDesc)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNOrdinal)> Private _Ordinal As Long
        <ormEntryMapping(EntryName:=ConstFNIsFC)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNActualID)> Private _actualid As String = ""

        <ormEntryMapping(EntryName:=ConstFNIsMandatory)> Private _isMandatory As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsProhibited)> Private _isProhibited As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsFacultative)> Private _isFacultative As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsFinish)> Private _isFinish As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsINPUT)> Private _isInputDeliverable As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsOutPut)> Private _isOutputDeliverable As Boolean


        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub


#Region "Properties"

        ''' <summary>
        ''' gets the schedule type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ScheduleType() As String
            Get
                Return _scheduletype
            End Get
        End Property

        ''' <summary>
        ''' gets the Milestone ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                Return _id
            End Get

        End Property

        ''' <summary>
        ''' gets or sets the actual milestone id (counterpart) a forecast milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ActualOfFC() As String
            Get
                Return _actualid
            End Get
            Set(value As String)
                SetValue(ConstFNActualID, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if actual
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActual() As Boolean
            Get
                Return Not _isForecast
            End Get
            Set(value As Boolean)
                Me.IsForecast = Not value
            End Set
        End Property

        ''' <summary>
        ''' gets or sets if the milestone is a forecast
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsForecast() As Boolean
            Get
                Return _isForecast
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsFC, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the flag for marking the milestone as end milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsFinish() As Boolean
            Get
                Return _isFinish
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsFinish, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the mandatory flag for the milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsMandatory() As Boolean
            Get
                Return _isMandatory
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsMandatory, value)
                If value Then
                    SetValue(ConstFNIsFacultative, False)
                    SetValue(ConstFNIsProhibited, False)
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the prohibited flag for this milestone in the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsProhibited() As Boolean
            Get
                Return _isProhibited
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsProhibited, value)
                If value Then
                    SetValue(ConstFNIsFacultative, False)
                    SetValue(ConstFNIsMandatory, False)
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the faculatative flag for this milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsFacultative() As Boolean
            Get
                Return _isFacultative
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsFacultative, value)
                If value Then
                    SetValue(ConstFNIsMandatory, False)
                    SetValue(ConstFNIsProhibited, False)
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets or set the outdeliverable flag for this milestone in this schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsOutputDeliverable() As Boolean
            Get
                Return _isOutputDeliverable
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsOutPut, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the input deliverable flag for this milestone in this schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsInputDeliverable() As Boolean
            Get
                Return _isInputDeliverable
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsINPUT, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Return _description
            End Get
            Set(value As String)
                SetValue(ConstFNDesc, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Ordinal() As Long
            Get
                Return _Ordinal
            End Get
            Set(value As Long)
                SetValue(ConstFNOrdinal, value)
            End Set
        End Property
#End Region


        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal scheduletype As String, ByVal ID As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As ScheduleMilestoneDefinition
            Return Retrieve(Of ScheduleMilestoneDefinition)(pkArray:={scheduletype, ID, domainID}, domainID:=domainID, forceReload:=forcereload)
        End Function

        '**** getDefMilestone
        '****
        ''' <summary>
        ''' retrieve related Milestone Definition Object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMilestoneDefinition() As MileStoneDefinition
            If Not IsAlive(subname:="GetMilestoneDefinition") Then Return Nothing
            Return MileStoneDefinition.Retrieve(id:=Me.ID)
        End Function


        ''' <summary>
        ''' Persist the Object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.constNullDate) As Boolean
            Dim aDefMS As MileStoneDefinition
            Dim aCompDesc As New ormCompoundDesc
            Dim aSchemaDefTable As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=Schedule.ConstTableID)

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If


            Try

                If MyBase.Persist(timestamp) Then

                    '*** create compound for schedules
                    '***
                    'If aSchemaDefTable Is Nothing Then
                    '    aSchemaDefTable.Create(objectID:=Schedule.ConstObjectID)
                    'End If

                    'aCompDesc.Tablename = Schedule.ConstTableID.ToLower
                    'aCompDesc.compound_Tablename = ScheduleMilestone.constTableID.ToLower
                    'aCompDesc.ID = _id
                    'aCompDesc.compound_Relation = New String() {"uid", "updc"}
                    'aCompDesc.compound_IDFieldname = "id"
                    'aCompDesc.compound_ValueFieldname = "value"
                    'aDefMS = Me.GetMilestoneDefinition
                    'If Not aDefMS Is Nothing Then
                    '    aCompDesc.Datatype = aDefMS.Datatype
                    'End If
                    ''aCompDesc.Aliases= {}
                    'aCompDesc.Parameter = ""
                    'aCompDesc.Title = "Milestone " & _id

                    'If aSchemaDefTable.AddEntry(aCompDesc) Then
                    '    aSchemaDefTable.Persist()
                    'End If

                    Return True
                End If

                Return False


            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTBDefScheduleMilestone.Persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' retrieve a collection of all schedule milestone definition objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of ScheduleMilestoneDefinition)
            Return ormDataObject.AllDataObject(Of ScheduleMilestoneDefinition)()
        End Function

        ''' <summary>
        ''' returns a List of Schedule Milestone Definitions by scheduletype
        ''' </summary>
        ''' <param name="scheduletype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByType(ByVal scheduletype As String) As List(Of ScheduleMilestoneDefinition)

            Dim aStore As iormDataStore = ot.GetTableStore(ConstTableID)
            Dim acollection As List(Of ScheduleMilestoneDefinition)

            Try
                ' To do ... load by a select
                'acollection = Cache.LoadFromCache(ConstTableID, scheduletype)
                'If acollection IsNot Nothing Then
                '    Return acollection
                'Else
                '    acollection = New List(Of ScheduleMilestoneDefinition)
                'End If

                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allbytype")
                If Not aCommand.Prepared Then
                    aCommand.Where = ConstFNType & "=@type"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@type", ColumnName:=ConstFNType, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@type", value:=scheduletype)
                Dim aRecordcollection As List(Of ormRecord) = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordcollection
                    Dim aNewObject As New ScheduleMilestoneDefinition
                    If InfuseDataObject(record:=aRecord, dataobject:=aNewObject) Then
                        acollection.Add(item:=aNewObject)
                    End If

                Next aRecord

                Return acollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefScheduleMilestone.AllByType")
                Return acollection
            End Try


        End Function
        ''' <summary>
        ''' create the persistable object
        ''' </summary>
        ''' <param name="SCHEDULETYPE"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal scheduletype As String, ByVal ID As String, Optional domainID As String = "") As ScheduleMilestoneDefinition
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim pkarray() As Object = {LCase(scheduletype), LCase(ID), domainID}
            Return ormDataObject.CreateDataObject(Of ScheduleMilestoneDefinition)(pkarray, domainID:=domainID, checkUnique:=True)
        End Function

    End Class

    ''' <summary>
    ''' schedule definition object
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleDefinition.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, _
        description:="definition of schedules (types)", useCache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Class ScheduleDefinition
        Inherits ormDataObject


        Public Const ConstObjectID = "ScheduleDefinition"

        ''' <summary>
        ''' TableDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=2, usecache:=True)> Public Const ConstTableID = "tblDefSchedules"

        ''' <summary>
        ''' keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, title:="ID", size:=50, _
            properties:={ObjectEntryProperty.Keyword}, validationPropertystrings:={ObjectValidationProperty.NotEmpty}, _
            Description:="Unique ID of the schedule type definition", _
            primaryKeyordinal:=1, xid:="SCT1", aliases:={"bs4"})> Public Const ConstFNType = "scheduletype"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
            title:="description", Description:="description of the schedule definition", _
            xid:="SCT2")> Public Const ConstFNDescription = "desc"


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNType)> Private _scheduletype As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""

        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaRelation(linkobject:=GetType(ScheduleMilestoneDefinition), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNType}, toEntries:={ScheduleMilestoneDefinition.ConstFNType})> Public Const ConstRMilestones = "Milestones"

        <ormEntryMapping(RelationName:=ConstRMilestones, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ScheduleMilestoneDefinition.ConstFNID})> _
        Private WithEvents _milestoneCollection As New ormRelationCollection(Of ScheduleMilestoneDefinition)(Me, {ScheduleMilestoneDefinition.ConstFNID})


#Region "properties"
        ''' <summary>
        ''' gets the schedule Type ID 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        ReadOnly Property ScheduleType As String
            Get
                Return _scheduletype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description As String
            Get
                Return _description
            End Get
            Set(value As String)
                SetValue(ConstFNDescription, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the Floag for no active Schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property isNoSchedule() As Boolean
            Get
                If _milestoneCollection.Count > 0 Then
                    isNoSchedule = False
                Else
                    isNoSchedule = True
                End If
            End Get
        End Property
        ''' <summary>
        ''' returns the number of milestones
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NoMembers() As Long
            Get
                Return _milestoneCollection.Count
            End Get
        End Property

        ''' <summary>
        ''' returns the Milestones of this schedule definition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Milestones As iormRelationalCollection(Of ScheduleMilestoneDefinition)
            Get
                Return _milestoneCollection
            End Get
        End Property
        ''' <summary>
        ''' gets the Milestones ordered by their ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MilestonesOrderedByOrdinal As ICollection(Of ScheduleMilestoneDefinition)
            Get
                Return _milestoneCollection.OrderBy(Function(x) x.Ordinal)
            End Get
        End Property
#End Region

        ''' <summary>
        ''' returns the maximum ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaximumMilestoneOrdinal() As Long
            If _milestoneCollection.Count > 0 Then Return _milestoneCollection.Select(Function(x) x.Ordinal).Max
            Return 0
        End Function

        ''' <summary>
        ''' legacy function
        ''' </summary>
        ''' <param name="AliasID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMilestoneIDByAlias(AliasID As String) As String
            ' legacy code:
            '''

            ' load aliases
            'If aTableEntry.LoadBy(objectname:=aSchedule.TableID, entryname:=anEntry.ID) Then
            '    For Each m In aTableEntry.Aliases
            '        If _aliases.ContainsKey(key:=LCase(m)) Then
            '            Call _aliases.Remove(key:=LCase(m))
            '        End If
            '        Call _aliases.Add(key:=LCase(m), value:=anEntry.ID)
            '    Next m
            'End If

            Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(Schedule.ConstObjectID)
            If anObjectDefinition IsNot Nothing Then
                For Each anEntry In anObjectDefinition.GetEntries
                    ''' return the XID = Milestone ID of the Compund
                    If anEntry.IsCompound AndAlso anEntry.Aliases.Contains(AliasID) Then Return anEntry.XID
                Next
            End If

            Return ""
        End Function
        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal scheduletype As String, Optional domainid As String = "", Optional forcereload As Boolean = False) As ScheduleDefinition
            Return Retrieve(Of ScheduleDefinition)(pkArray:={scheduletype}, domainID:=domainid, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' create the data object by primary key
        ''' </summary>
        ''' <param name="SCHEDULETYPE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal scheduletype As String, Optional domainid As String = "") As ScheduleDefinition
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim pkarray() As String = {LCase(scheduletype), domainid}
            Return ormDataObject.CreateDataObject(Of ScheduleDefinition)(pkarray, domainID:=domainid, checkUnique:=False)
        End Function

    End Class

    ''' <summary>
    ''' schedule class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(Version:=1, ID:=Schedule.ConstObjectID, modulename:=ConstModuleScheduling, _
        addDomainBehavior:=False, AddDeleteFieldBehavior:=True, _
        Title:="Schedule", Description:="schedules for business objects")> _
    Public Class Schedule
        Inherits ormDataObject
        Implements iotXChangeable
        Implements iormInfusable
        Implements iormPersistable
        Implements iotHasCompounds
        Implements iotCloneable(Of Schedule)

        Public Const ConstObjectID = "Schedule"

        ''' <summary>
        ''' TableDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTableAttribute(Version:=2)> Public Const ConstTableID = "tblschedules"
        '** Indexes
        <ormSchemaIndexAttribute(columnname1:=ConstFNWorkspaceID, columnname2:=ConstFNUid, columnname3:=ConstFNUpdc)> Public Const ConstIndexWS = "workspaceID"
        <ormSchemaIndexAttribute(columnname1:=ConstFNUid)> Public Const ConstIndexUID = "uidIndex"

        ''' <summary>
        ''' Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Long, title:="unique ID", Description:="Unique ID of the schedule", _
            lowerrange:=0, _
            primaryKeyordinal:=1, XID:="SC2", aliases:={"SUID"})> Public Const ConstFNUid = "uid"
        <ormObjectEntry(typeid:=otDataType.Long, title:="update count", Description:="Update count of the schedule", _
            lowerrange:=0, _
           primaryKeyordinal:=2, XID:="SC3", aliases:={"BS3"})> Public Const ConstFNUpdc = "updc"


        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Long, _
            title:="forecast count", Description:="number of forecast udates of this schedule" _
          )> Public Const ConstFNfcupdc = "fcupdc"

        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            Description:="workspaceID ID of the schedule", useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
             foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> Public Const ConstFNWorkspaceID = Workspace.ConstFNID

        ' deactivate ForeignKEy we do not have this object in domains
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            title:="revision", Description:="revision of the schedule", _
            XID:="SC5", aliases:={"BS2"})> Public Const ConstFNPlanRev = "plrev"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="is frozen", Description:="schedule is frozen flag", _
            XID:="SC6")> Public Const ConstFNisfrozen = "isfrozen"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            title:="lifecycle status", Description:="lifecycle status of the schedule", _
            XID:="SC7", aliases:={"BS1"})> Public Const ConstFNlcstatus = "lcstatus"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            title:="process status", Description:="process status of the schedule", _
            XID:="SC8", aliases:={"S1"})> Public Const ConstFNpstatus = "pstatus"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
            title:="check timestamp", Description:="timestamp of check status of the schedule", _
            XID:="SC9")> Public Const ConstFNCheckedOn = "checkedon"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
            title:="planner", Description:="responsible planner of the schedule", _
            XID:="SC10")> Public Const ConstFNPlanner = "resp"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
            title:="comment", Description:="comment of the schedule", _
            XID:="SC12", aliases:={}, Defaultvalue:="", parameter:="")> Public Const ConstFNComment = "cmt"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
            title:="last fc update", Description:="last forecast change of the schedule", _
            XID:="SC13")> Public Const ConstFNFCupdatedOn = "fcupdon"

        <ormObjectEntry(referenceObjectEntry:=ScheduleDefinition.ConstObjectID & "." & ScheduleDefinition.ConstFNType, _
            title:="type", Description:="type of the schedule", _
            XID:="SC14", aliases:={"BS4"}, isnullable:=True)> Public Const ConstFNTypeid = "typeid"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
            title:="baseline flag", Description:="flag if the schedule is a baseline", _
            XID:="SC15")> Public Const ConstFNIsBaseline = "isbaseline"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
            title:="baseline date", Description:="date of the baseline creation", _
            XID:="SC16", aliases:={})> Public Const ConstFNBlDate = "bldate"

        <ormObjectEntry(typeid:=otDataType.Long, isnullable:=True, _
            title:="baseline updc", Description:="updc of the last baseline of this schedule", _
            XID:="SC17")> Public Const ConstFNBlUpdc = "blupdc"

        <ormObjectEntry(typeid:=otDataType.Numeric, isnullable:=True, _
            title:="required capacity", Description:="required capacity of this schedule", _
            XID:="SC20", aliases:={"WBS2"})> Public Const ConstFNRequCap = "requ"

        <ormObjectEntry(typeid:=otDataType.Numeric, isnullable:=True, _
            title:="used capacity", Description:="used capacity of this schedule", _
            XID:="SC21", aliases:={"WBS3"}, Defaultvalue:="0")> Public Const ConstFNUsedCap = "used"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
            title:="used capacity reference date", Description:="used capacity reference date of this schedule", _
            XID:="SC22", aliases:={"WBS4"})> Public Const ConstFNUsedCapRef = "ufdt"


        <ormObjectEntry(referenceObjectEntry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag


        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNfcupdc)> Private _fcupdc As Long    ' update count of just fc
        <ormEntryMapping(EntryName:=ConstFNPlanRev)> Private _plrev As String = ""
        <ormEntryMapping(EntryName:=ConstFNPlanner)> Private _planner As String = ""
        <ormEntryMapping(EntryName:=ConstFNisfrozen)> Private _isFrozen As Boolean
        <ormEntryMapping(EntryName:=ConstFNpstatus)> Private _pstatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNlcstatus)> Private _lfcstatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNCheckedOn)> Private _checkedOn As Date = constNullDate
        <ormEntryMapping(EntryName:=ConstFNFCupdatedOn)> Private _fcUpdatedOn As Date = constNullDate
        <ormEntryMapping(EntryName:=ConstFNIsBaseline)> Private _isBaseline As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNBlDate)> Private _baselineDate As Date = constNullDate
        <ormEntryMapping(EntryName:=ConstFNBlUpdc)> Private _baselineUPDC As Long = 0

        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=ConstFNTypeid)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=ConstFNRequCap)> Private _requ As Double = 0
        <ormEntryMapping(EntryName:=ConstFNUsedCap)> Private _used As Double = 0
        <ormEntryMapping(EntryName:=ConstFNUsedCapRef)> Private _ufdt As Date = constNullDate
        <ormEntryMapping(EntryName:=ConstFNComment)> Private _comment As String = ""
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""

        ''' <summary>
        ''' Relations
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaRelation(linkobject:=GetType(ScheduleMilestone), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNUid}, toEntries:={ScheduleMilestone.ConstFNUid})> Public Const ConstRMilestones = "MILESTONES"

        <ormEntryMapping(RelationName:=ConstRMilestones, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ScheduleMilestone.ConstFNID})> Private WithEvents _milestoneCollection As New ormRelationCollection(Of ScheduleMilestone)(Me, {ScheduleMilestone.ConstFNID})


        ' components itself per key:=id, item:=clsOTDBXScheduleMilestone
        'Private s_members As New Dictionary(Of String, ScheduleMilestone)
        Private _originalMilestoneValues As New Dictionary(Of String, Object)   'orgmembers -> original members before any change

        ' dynamic
        Private _haveMilestonesChanged As Boolean
        Private _isForeCastChanged As Boolean
        'Private s_milestones As New Dictionary -> superseded with members
        Private _loadedFromHost As Boolean
        Private _savedToHost As Boolean
        Private _defschedule As New ScheduleDefinition

        Private _msglog As New ObjectLog


#Region "Properties"
        ''' <summary>
        ''' gets the UID of the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        ReadOnly Property Uid() As Long
            Get
                Return _uid
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the comment for this schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Return _comment
            End Get
            Set(value As String)
                SetValue(ConstFNComment, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the workspace for the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property workspaceID() As String
            Get
                Return _workspace

            End Get
            Set(value As String)
                SetValue(ConstFNWorkspaceID, value)
                ''' change the workspace also for all milestones !
                For Each aMilestone In _milestoneCollection
                    aMilestone.WorkspaceID = value
                Next
            End Set
        End Property
        ''' <summary>
        ''' gets the number of milestones in the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property NoMilestones() As Long
            Get
                Return _milestoneCollection.Count
            End Get
        End Property
        ''' <summary>
        ''' returns true if the the forecast is changed since Inject / last persist
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsForecastChanged() As Boolean
            Get
                Return _isForeCastChanged
            End Get
        End Property
        ''' <summary>
        ''' returns the type id of the schedule type of this schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Typeid() As String
            Get
                Typeid = _typeid
            End Get
            Set(value As String)
                Dim defschedule As ScheduleDefinition
                ' set the internal defschedule link
                If LCase(_typeid) <> LCase(value) Then
                    defschedule = ScheduleDefinition.Retrieve(scheduletype:=value)
                    If defschedule Is Nothing Then
                        Call CoreMessageHandler(message:="TypeID has not schedule defined", subname:="Schedule.typeID", _
                                              arg1:=value)
                    Else
                        _defschedule = defschedule
                        _typeid = value
                        Me.IsChanged = True
                    End If
                    ' load the milestones
                    If Not LoadMilestones(scheduletypeid:=_typeid) Then
                        Call CoreMessageHandler(message:="Milestones of TypeID couldnot loaded", _
                                              subname:="Schedule.typeID let", _
                                              arg1:=value)
                    End If
                End If

            End Set
        End Property
        ''' <summary>
        ''' get the loaded from Host Application flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LoadedFromHost() As Boolean
            Get
                LoadedFromHost = _loadedFromHost
            End Get
            Set(value As Boolean)
                _loadedFromHost = value
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the required capaciyty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RequiredCapacity() As Double
            Get
                Return _requ
            End Get
            Set(value As Double)
                SetValue(ConstFNRequCap, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the used capacity
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UsedCapacity() As Double
            Get
                Return _used
            End Get
            Set(value As Double)
                SetValue(ConstFNUsedCap, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the used capacity reference date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UsedCapacityRefDate() As Date
            Get
                Return _ufdt
            End Get
            Set(value As Date)
                SetValue(ConstFNUsedCapRef, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the planning revision
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Revision() As String
            Get
                Return _plrev
            End Get
            Set(value As String)
                SetValue(ConstFNPlanRev, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the planner
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Planner() As String
            Get
                Return _planner
            End Get
            Set(value As String)
                SetValue(ConstFNPlanner, value)
            End Set
        End Property
        ''' <summary>
        ''' getrs or sets the process status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessStatus() As String
            Get
                Return _pstatus
            End Get
            Set(value As String)
                SetValue(ConstFNpstatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the lifecycle status of the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LFCStatus() As String
            Get
                Return _lfcstatus
            End Get
            Set(value As String)
                SetValue(ConstFNlcstatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the isfrozen flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsFrozen() As Boolean
            Get
                Return _isFrozen
            End Get
            Set(value As Boolean)
                SetValue(ConstFNisfrozen, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the Baseline flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsBaseline() As Boolean
            Get
                Return _isBaseline
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsBaseline, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the status checked date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property StatusCheckedOn() As Date
            Get
                Return _checkedOn
            End Get
            Set(value As Date)
                SetValue(ConstFNCreatedOn, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the baseline reference date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaselineRefDate() As Date
            Get
                Return _baselineDate
            End Get
            Set(value As Date)
                SetValue(ConstFNBlDate, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the baseline updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property BaselineUPDC() As Long
            Get
                Return _baselineUPDC
            End Get
            Set(value As Long)
                SetValue(ConstFNBlUpdc, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the last forecast update date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LastForecastUpdate() As Date
            Get
                Return _fcUpdatedOn
            End Get
            Set(value As Date)
                SetValue(ConstFNFCupdatedOn, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the forecast update count
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FCupdc() As Long
            Get
                Return _fcupdc
            End Get

        End Property
        ''' <summary>
        ''' gets the updc
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Updc() As Long
            Get
                Return _updc
            End Get

        End Property
        ''' <summary>
        ''' gets the msglogtag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = getUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get
        End Property

        ''' <summary>
        ''' true if a milestone was changed after last load / persist / publish
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property haveMileStonesChanged() As Boolean
            Get
                Return _haveMilestonesChanged
            End Get
        End Property
        ''' <summary>
        ''' gets the Milestones of this schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Milestones As iormRelationalCollection(Of ScheduleMilestone)
            Get
                Return _milestoneCollection
            End Get
        End Property
#End Region

        ''' <summary>
        ''' retrieve the related Schedule Definition object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetScheduleDefinition() As ScheduleDefinition
            If Not _defschedule.IsAlive(throwError:=False) Then
                _defschedule = ScheduleDefinition.Retrieve(scheduletype:=_typeid)
                If _defschedule Is Nothing Then
                    Call CoreMessageHandler(message:="schedule defintion doesn't exist", subname:="Schedule.defSchedule", _
                                          arg1:=_typeid)
                    _defschedule = New ScheduleDefinition
                End If
            End If
            Return _defschedule
        End Function
        ''' <summary>
        ''' retrieve the related Schedule Milestone Definition Object
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetScheduleMilestoneDefinition(ByVal ID As String) As ScheduleMilestoneDefinition
            Dim aDefScheduleMS As ScheduleMilestoneDefinition = ScheduleMilestoneDefinition.Retrieve(scheduletype:=_typeid, ID:=ID)
            If aDefScheduleMS Is Nothing Then
                Call CoreMessageHandler(message:="schedule milestone definition doesn't exist", _
                                      subname:="Schedule.getDefScheduleMilestone", _
                                      arg1:=_typeid & "-" & ID)
                aDefScheduleMS = Nothing
            End If

            Return aDefScheduleMS
        End Function

        '*** increment the updc version
        Public Function Incupdc() As Long
            _updc = _updc + 1
            Incupdc = _updc
            Me.IsChanged = True
        End Function
        '*** increment the updc version
        Public Function Incfcupdc() As Long
            _fcupdc = _fcupdc + 1
            Incfcupdc = _fcupdc
            Me.IsChanged = True
        End Function
        '****** getUniqueTag
        Public Function getUniqueTag()
            getUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & _uid & ConstDelimiter & _updc & ConstDelimiter
        End Function


        '''' <summary>
        '''' Initialize the data object
        '''' </summary>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Function Initialize() As Boolean
        '    Initialize = MyBase.Initialize
        '    s_members = New Dictionary(Of String, ScheduleMilestone)
        '    s_orgMSvalues = New Dictionary(Of String, Object)
        '    _workspace = CurrentSession.CurrentWorkspaceID
        '    _haveMilestonesChanged = False
        '    _ufdt = ConstNullDate
        '    _checkedOn = ConstNullDate
        '    _fcUpdatedOn = ConstNullDate
        '    _baselineDate = ConstNullDate
        '    _ufdt = ConstNullDate
        '    _isForeCastChanged = False
        '    SerializeWithHostApplication = isDefaultSerializeAtHostApplication(ConstTableID)
        '    _defschedule = New ScheduleDefinition
        '    's_parameter_date1 = ot.ConstNullDate
        '    's_parameter_date2 = ot.ConstNullDate
        '    's_parameter_date3 = ot.ConstNullDate

        'End Function

        ''' <summary>
        ''' milestone returns the Milestone Value as object or Null if not exists
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ORIGINAL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMilestoneValue(ByVal ID As String, Optional ORIGINAL As Boolean = False) As Object
            Dim aMember As New ScheduleMilestone
            Dim aDefSchedule As ScheduleDefinition = Me.GetScheduleDefinition
            Dim aRealID As String
            ID = ID.ToUpper

            If Not IsAlive(subname:="GetMilestoneValue") Then Return Nothing

            ' check aliases
            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return Nothing
            End If

            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=ID)
            If aRealID = "" Then aRealID = ID
            '

            ' return not original
            If _milestoneCollection.ContainsKey({aRealID}) Then
                aMember = _milestoneCollection.Item({aRealID})
                If Not ORIGINAL Then
                    Return aMember.Value
                Else
                    Return _originalMilestoneValues.Item(aRealID)
                End If
            End If
            'If s_members.ContainsKey(key:=LCase(aRealID)) Then
            '    aMember = s_members.Item(key:=LCase(aRealID))
            '    If Not ORIGINAL Then
            '        Return aMember.Value
            '    ElseIf s_orgMSvalues.ContainsKey(LCase(aRealID)) Then
            '        Return s_orgMSvalues.Item(LCase(aRealID))
            '    Else
            '        Return Nothing
            '    End If

            'Else
            '    Return Nothing
            'End If

            Return Nothing
        End Function
        ''' <summary>
        ''' milestone returns the Milestone Value as object or Null if not exists
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ORIGINAL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMilestone(ByVal ID As String) As ScheduleMilestone
            Dim aMember As New ScheduleMilestone
            Dim aDefSchedule As ScheduleDefinition = Me.GetScheduleDefinition
            Dim aRealID As String
            ID = ID.ToUpper
            If Not IsAlive(subname:="getMilestone") Then Return Nothing

            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return Nothing
            End If

            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=ID)
            If aRealID = "" Then aRealID = ID


            ' return not original
            ' return not original
            If _milestoneCollection.ContainsKey({aRealID}) Then
                Return _milestoneCollection.Item({aRealID})
            End If

            'If s_members.ContainsKey(key:=LCase(aRealID)) Then
            '    aMember = s_members.Item(key:=LCase(aRealID))
            '    Return aMember
            'Else
            '    Return Nothing
            'End If

            Return Nothing
        End Function
        '******* setMilestone ID to Value
        '*******
        ''' <summary>
        ''' setMilestone ID to Value
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="Value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetMilestone(ByVal ID As String, ByVal Value As Object, Optional setNull As Boolean = False) As Boolean
            Dim aMember As New ScheduleMilestone
            Dim isMemberchanged As Boolean
            Dim aDefSchedule As ScheduleDefinition = Me.GetScheduleDefinition
            Dim aRealID As String
            ID = ID.ToUpper

            If Not IsAlive(subname:="SetMilestone") Then Return False

            ' check aliases
            If aDefSchedule Is Nothing Then
                CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return False
            End If

            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=ID)
            If aRealID = "" Then aRealID = ID


            If _milestoneCollection.ContainsKey({aRealID}) Then
                aMember = _milestoneCollection.Item({aRealID})
            Else
                Call CoreMessageHandler(arg1:=ID, subname:="Schedule.setMilestone", tablename:=ConstTableID, _
                                      message:="ID doesnot exist in Milestone Entries")
                Return False
            End If

            isMemberchanged = False


            ' if the Member is only a Cache ?!
            If aMember.IsCacheNoSave Then
                Call CoreMessageHandler(message:="setMilestone to cached Item", subname:="Schedule.setMilestone", messagetype:=otCoreMessageType.ApplicationError, _
                                      arg1:=LCase(ID) & ":" & CStr(Value))
                Return False
            End If

            ' convert it
            If (aMember.Datatype = otDataType.[Date] Or aMember.Datatype = otDataType.Timestamp) Then
                If IsDate(Value) And Not setNull Then
                    If aMember.Value <> CDate(Value) Then
                        aMember.Value = CDate(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If aMember.Value <> constNullDate Then
                        aMember.Value = constNullDate
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of date cannot set to", subname:="Schedule.setMilestone", _
                                                         arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            ElseIf aMember.Datatype = otDataType.Numeric Then
                If IsNumeric(Value) And Not setNull Then
                    If aMember.Value <> CDbl(Value) Then
                        aMember.Value = CDbl(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If aMember.Value <> 0 Then
                        aMember.Value = 0
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of numeric cannot set to", subname:="Schedule.setMilestone", _
                                                        arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            ElseIf aMember.Datatype = otDataType.[Long] Then
                If IsNumeric(Value) And Not setNull Then
                    If aMember.Value <> CLng(Value) Then
                        aMember.Value = CLng(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If aMember.Value <> 0 Then
                        aMember.Value = 0
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of long cannot set to", subname:="Schedule.setMilestone", _
                                                        arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            ElseIf aMember.Datatype = otDataType.Bool Then
                If Not setNull Then
                    If aMember.Value <> CBool(Value) Then
                        aMember.Value = CBool(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If aMember.Value <> False Then
                        aMember.Value = False
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of bool cannot set to", subname:="Schedule.setMilestone", _
                                                        arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            Else
                If Not setNull Then
                    If aMember.Value <> CStr(Value) Then
                        aMember.Value = CStr(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If String.IsNullOrEmpty(aMember.Value) Then
                        aMember.Value = CStr(Value)
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of string cannot set to", subname:="Schedule.setMilestone", _
                                                        arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            End If


            ' save it to dictionary
            ' get Member
            If isMemberchanged Then
                'Call s_members.add(Key:=LCase(aRealID), Item:=aMember) -> should be ok since referenced
                _haveMilestonesChanged = True
                If aMember.IsForecast Then
                    _isForeCastChanged = True
                End If
                Return True
            Else
                Return True
            End If

            Return False

        End Function

        '******** moveMilestone
        '********
        ''' <summary>
        ''' move the milestone in date range
        ''' </summary>
        ''' <param name="noDays"></param>
        ''' <param name="MSID"></param>
        ''' <param name="considerWorkingDays"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MoveMilestone(ByVal noDays As Long, _
                        Optional ByVal MSID As String = "", _
                        Optional considerWorkingDays As Boolean = True) As Boolean
            Dim aScheduleMSDef As New ScheduleMilestoneDefinition
            Dim aScheduleMSDefColl As New List(Of ScheduleMilestoneDefinition)
            Dim aCE As New CalendarEntry
            Dim flag As Boolean
            Dim aDate As Object
            Dim actDate As Object

            If Not Me.IsLoaded And Not Me.IsCreated Then
                MoveMilestone = False
                Exit Function
            End If

            If Me.IsFinished Then
                MoveMilestone = False
                Exit Function
            End If

            aScheduleMSDef = ScheduleMilestoneDefinition.Retrieve(scheduletype:=Me.Typeid, ID:=MSID)
            If aScheduleMSDef Is Nothing Then
                MoveMilestone = False
                Exit Function
            End If
            ' if we have a forecast -> need to look for the actual
            If aScheduleMSDef.IsForecast Then
                aScheduleMSDefColl = GetDefScheduleMSbyOrder(justDates:=True)
                If aScheduleMSDefColl Is Nothing Or aScheduleMSDefColl.Count = 0 Then
                    MoveMilestone = False
                    Exit Function
                End If
                flag = False
                For Each aScheduleMSDef In aScheduleMSDefColl
                    If (aScheduleMSDef.ActualOfFC = LCase(MSID) And aScheduleMSDef.ActualOfFC <> "") Then
                        flag = True
                        Exit For
                    End If
                Next aScheduleMSDef
                If Not flag Then
                    MoveMilestone = False
                    Exit Function
                End If
            End If
            'actual found -> checkit
            actDate = Me.GetMilestoneValue(aScheduleMSDef.ID)
            aDate = Me.GetMilestoneValue(aScheduleMSDef.ActualOfFC)
            If aDate <> constNullDate And IsDate(aDate) And actDate = constNullDate And IsDate(actDate) And aScheduleMSDef.ActualOfFC <> "" And aScheduleMSDef.ID <> "" Then
                ' only if there is this milestone
                aCE.Timestamp = aDate
                aDate = aCE.AddDay(noDays, considerAvailibilty:=considerWorkingDays, calendarname:=CurrentSession.DefaultCalendarName)
                Call Me.SetMilestone(aScheduleMSDef.ActualOfFC, aDate)
                '*******
                '******* we need to check ascending condition !!

                MoveMilestone = True
                Exit Function
            End If

            MoveMilestone = False
            Exit Function

        End Function

        '******** moveSchedule
        '********
        ''' <summary>
        ''' move the full schedule in date range
        ''' </summary>
        ''' <param name="noDays"></param>
        ''' <param name="startMS"></param>
        ''' <param name="considerWorkingDays"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MoveSchedule(ByVal noDays As Long, _
        Optional ByVal startMS As String = "", _
        Optional considerWorkingDays As Boolean = True) As Boolean
            Dim aScheduleMSDefColl As New List(Of ScheduleMilestoneDefinition)
            Dim aScheduleMSDef As New ScheduleMilestoneDefinition
            Dim aCE As New CalendarEntry
            Dim started As Boolean
            Dim aDate As Object
            Dim actDate As Object

            If Not Me.IsLoaded And Not Me.IsCreated Then
                MoveSchedule = False
                Exit Function
            End If

            If Me.IsFinished Then
                MoveSchedule = False
                Exit Function
            End If

            aScheduleMSDefColl = GetDefScheduleMSbyOrder(justDates:=True)
            If aScheduleMSDefColl Is Nothing Or aScheduleMSDefColl.Count = 0 Then
                MoveSchedule = False
                Exit Function
            End If

            started = False
            ' go through the milestones in order and move them if they are not actual
            For Each aScheduleMSDef In aScheduleMSDefColl
                If aScheduleMSDef.ID = LCase(startMS) Or (aScheduleMSDef.ActualOfFC = LCase(startMS) And aScheduleMSDef.ActualOfFC <> "") Or startMS = "" Then
                    started = True
                End If
                If Not aScheduleMSDef.IsForecast And started Then
                    ' no actual found -> calculate on the fc
                    actDate = Me.GetMilestoneValue(aScheduleMSDef.ID)
                    aDate = Me.GetMilestoneValue(aScheduleMSDef.ActualOfFC)
                    If aDate <> constNullDate And IsDate(aDate) And _
                    actDate = constNullDate And IsDate(actDate) And aScheduleMSDef.ActualOfFC <> "" And aScheduleMSDef.ID <> "" Then
                        ' only if there is this milestone
                        aCE.Timestamp = aDate
                        aDate = aCE.AddDay(noDays, considerAvailibilty:=considerWorkingDays, calendarname:=CurrentSession.DefaultCalendarName)
                        Call Me.SetMilestone(aScheduleMSDef.ActualOfFC, aDate)
                    End If
                End If
            Next aScheduleMSDef
            ' move it
            MoveSchedule = True
        End Function
        '******** getDefScheduleMSbyOrder returns a Collection of ScheduleMilestones by Orderno
        '********
        ''' <summary>
        ''' getDefScheduleMSbyOrder returns a Collection of ScheduleMilestones by Orderno
        ''' </summary>
        ''' <param name="justDates"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDefScheduleMSbyOrder(Optional justDates As Boolean = True) As List(Of ScheduleMilestoneDefinition)
            Dim aScheduleDef As New ScheduleDefinition
            Dim atypeid As String
            Dim aDeliverableTrack As New Track
            Dim aCollection As New List(Of ScheduleMilestoneDefinition)
            Dim aMSDefCollection As New Collection
            Dim aScheduleMSDef As New ScheduleMilestoneDefinition
            Dim aMilestoneDef As New MileStoneDefinition

            If Not Me.IsLoaded And Not Me.IsCreated Then
                GetDefScheduleMSbyOrder = Nothing
                Exit Function
            End If

            If Me.Typeid = "" Then
                aDeliverableTrack = Me.GetDeliverableTrack
                If aDeliverableTrack Is Nothing Then
                    GetDefScheduleMSbyOrder = Nothing
                    Exit Function
                Else
                    atypeid = aDeliverableTrack.Scheduletype
                End If
            Else
                atypeid = Me.Typeid
            End If

            aScheduleDef = ScheduleDefinition.Retrieve(scheduletype:=atypeid)
            If aScheduleDef Is Nothing Then
                Call CoreMessageHandler(subname:="Schedule.getDefScheduleMSbyOrder", message:=" scheduletype of '" & atypeid & "' not found", arg1:=atypeid)
                Return Nothing
            Else
                aMSDefCollection = aScheduleDef.Milestones     ' should be in the order
                If aMSDefCollection Is Nothing Or aMSDefCollection.Count = 0 Then
                    GetDefScheduleMSbyOrder = Nothing
                    Exit Function
                End If
                ' go through
                For Each aScheduleMSDef In aMSDefCollection
                    aMilestoneDef = MileStoneDefinition.Retrieve(id:=aScheduleMSDef.ID)
                    If aMilestoneDef IsNot Nothing Then
                        If (aMilestoneDef.Datatype = otMilestoneType.Status And Not justDates) Or justDates Then
                            Call aCollection.Add(item:=aScheduleMSDef)
                        End If
                    Else
                        Call CoreMessageHandler(subname:="Schedule.getDefScheduleMSbyOrder", message:=" milestone with id '" & aScheduleMSDef.ID & "' not found", arg1:=atypeid)
                    End If

                Next aScheduleMSDef
            End If
            ' return value
            GetDefScheduleMSbyOrder = aCollection
        End Function

        ''' <summary>
        ''' return a collection of all schedules of a uid
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function AllByUID(UID As Long) As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            Dim aRecord As ormRecord

            Try
                aStore = GetTableStore(ConstTableID)
                Dim pkarray() As Object = {UID}
                aRecordCollection = aStore.GetRecordsByIndex(ConstIndexUID, pkarray, True)

                If Not aRecordCollection Is Nothing Then
                    For Each aRecord In aRecordCollection
                        Dim aNewSchedule As New Schedule
                        If InfuseDataObject(record:=aRecord, dataobject:=aNewSchedule) Then
                            aCollection.Add(Item:=aNewSchedule)
                        End If
                    Next aRecord
                End If
                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Schedule.AllByUID")
                Return aCollection
            End Try

        End Function

        ''' <summary>
        ''' load all Milestones as Members -> look for Actuals access
        ''' </summary>
        ''' <param name="scheduletypeid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadMilestones(ByVal scheduletypeid As String) As Boolean
            If Not IsAlive(subname:="LoadMilestones") Then Return False

            ''' load the milestones
            If Not InfuseRelation(id:=ConstRMilestones) Then
                CoreMessageHandler(message:="could not load and infuse the milestones for this schedule #" & _uid & "." & _updc, _
                                    messagetype:=otCoreMessageType.InternalError, arg1:=Me.Typeid)
            End If
            Return True
        End Function
        ''' <summary>
        ''' event handler for relation loaded
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnRelationLoad(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnRelationLoad

            If e.RelationID = ConstRMilestones Then
                Dim CurrenWorkspace As Workspace = Workspace.Retrieve(Me.workspaceID)
                Dim aCurrSCHEDULE As New CurrentSchedule
                Dim updc As Long
                Dim isCache As Boolean
                Dim aWSID As String
                Dim meme = TryCast(e.DataObject, Schedule)

                If meme Is Nothing Then
                    CoreMessageHandler(message:="data object could not be cast to Schedule", subname:="Schedule_OnRelationload", messagetype:=otCoreMessageType.InternalError)
                    Exit Sub
                End If
                Dim aScheduleDefinition As ScheduleDefinition = ScheduleDefinition.Retrieve(scheduletype:=meme.Typeid)
                If aScheduleDefinition Is Nothing Then
                    CoreMessageHandler(message:="schedule definition could not be retrieved", arg1:=meme.Typeid, _
                                       subname:="Schedule_OnRelationload", messagetype:=otCoreMessageType.InternalError)
                    Exit Sub
                End If

                Dim aCollection As iormRelationalCollection(Of ScheduleMilestoneDefinition) = aScheduleDefinition.Milestones

                ''' reworked to load really the actuals from the actual workspace
                ''' disable the milestones which are not set int his schedule
                For Each aMilestone In meme.Milestones

                    If Not aCollection.ContainsKey({aMilestone.ID}) Then
                        aMilestone.IsEnabled = False
                    Else
                        Dim aScheduleMSDef As ScheduleMilestoneDefinition = aCollection.Item({aMilestone.ID})
                        Dim aMSDef As MileStoneDefinition = MileStoneDefinition.Retrieve(aScheduleMSDef.ID)

                        If Not aScheduleMSDef.IsProhibited AndAlso aMSDef IsNot Nothing Then
                            isCache = False
                            ' check if actuals are kept in this workspaceID
                            If Not CurrenWorkspace.HasActuals And aScheduleMSDef.IsActual Then
                                updc = 0
                                isCache = True    ' find or not we are true
                                ' search for the next wspace in stack with actuals
                                For Each aWSID In CurrenWorkspace.ACTRelyingOn
                                    Dim aWS As Workspace = Workspace.Retrieve(aWSID)
                                    If Not aWS Is Nothing Then
                                        If aWS.HasActuals Then
                                            ' load the current
                                            aCurrSCHEDULE = CurrentSchedule.RetrieveUnique(UID:=_uid, workspaceID:=aWSID)
                                            If aCurrSCHEDULE IsNot Nothing Then
                                                updc = aCurrSCHEDULE.UPDC
                                                Exit For
                                            End If
                                        End If
                                    End If
                                Next
                                '** load the actual milestone
                                Dim anotherMilestone As ScheduleMilestone = ScheduleMilestone.Retrieve(UID:=_uid, updc:=updc, ID:=aScheduleMSDef.ID)
                                aMilestone.IsCacheNoSave = True
                                aMilestone.Value = anotherMilestone.Value

                            End If    ' actuals

                        Else
                            aMilestone.IsEnabled = False
                        End If
                    End If
                Next

            End If

        End Sub
        ''' <summary>
        ''' handles the OnInfused Event - load the milestones
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused

            Try
                '*** overload it from the Application Container
                '***
                If Me.SerializeWithHostApplication Then
                    If overloadFromHostApplication(Record) Then
                        _loadedFromHost = True
                    End If
                End If

                _haveMilestonesChanged = False
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Schedule.Infuse")
            End Try

        End Sub

        ''' <summary>
        ''' Event Handler for the Added Milestones
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnAdded(sender As Object, e As ormRelationCollection(Of ScheduleMilestone).EventArgs) Handles _milestoneCollection.OnAdded
            If Not IsAlive(subname:="Schedule_ONAdded") Then
                e.Cancel = True
                Exit Sub
            End If

            '** save original values
            If Not _originalMilestoneValues.ContainsKey(key:=e.Dataobject.ID) Then
                _originalMilestoneValues.Add(key:=e.Dataobject.ID, value:=e.Dataobject.Value)
            End If
        End Sub


        ''' <summary>
        ''' loads an schedule from store
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal UID As Long, ByVal updc As Long) As Schedule
            Return Retrieve(Of Schedule)(pkArray:={UID, updc})
        End Function

        ''' <summary>
        ''' creates all the default milestones for this schedule dependend on the schedule type
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CreateDefaultMilestones() As Boolean
            Dim CurrenWorkspace As Workspace = Workspace.Retrieve(Me.workspaceID)
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim anUpdc As Long
            Dim isCache As Boolean
            Dim aWSID As String

            If _typeid Is Nothing OrElse _typeid = "" Then
                CoreMessageHandler(message:="schedule type of this schedule is not set - can not create default milestones", _
                                    arg1:=_uid, messagetype:=otCoreMessageType.ApplicationError, subname:="Schedule.CreateDefaultMilestones")
                Return False
            End If

            Dim aSchedule As ScheduleDefinition = ScheduleDefinition.Retrieve(scheduletype:=Me.Typeid)
            If aSchedule Is Nothing Then
                CoreMessageHandler(message:="schedule type of this schedule could not be retrieved - can not create default milestones", _
                                   arg1:=_typeid, messagetype:=otCoreMessageType.ApplicationError, subname:="Schedule.CreateDefaultMilestones")
                Return False
            End If

            Dim aCollection As iormRelationalCollection(Of ScheduleMilestoneDefinition) = aSchedule.Milestones
            ''' switch off all not used in the schedule
            ''' 
            For Each aMilestone In Me.Milestones
                If Not aCollection.ContainsKey(aMilestone.ID) Then
                    aMilestone.IsEnabled = False
                End If
            Next

            ''' check the ones in the definition - create them or overload the actuals if necessary
            ''' 
            For Each aScheduleMSDef In aCollection
                ' get the milestone definition
                Dim aMSDef As MileStoneDefinition = MileStoneDefinition.Retrieve(aScheduleMSDef.ID)

                If Not aScheduleMSDef.IsProhibited AndAlso aMSDef IsNot Nothing Then
                    isCache = False
                    ' check if actuals are kept in this workspaceID
                    If Not CurrenWorkspace.HasActuals And aScheduleMSDef.IsActual Then
                        anUpdc = 0
                        isCache = True    ' find or not we are true
                        ' search for the next wspace in stack with actuals
                        For Each aWSID In CurrenWorkspace.ACTRelyingOn
                            Dim aWS As Workspace = Workspace.Retrieve(aWSID)
                            If Not aWS Is Nothing Then
                                If aWS.HasActuals Then
                                    ' load the current
                                    aCurrSCHEDULE = CurrentSchedule.RetrieveUnique(UID:=_uid, workspaceID:=aWSID)
                                    If aCurrSCHEDULE IsNot Nothing Then
                                        anUpdc = aCurrSCHEDULE.UPDC
                                        Exit For
                                    End If
                                End If
                            End If
                        Next
                    Else
                        anUpdc = _updc
                        isCache = False
                    End If    ' actuals

                    '** load the milestone
                    Dim aMilestone As ScheduleMilestone
                    If Not isCache Then
                        '' create
                        aMilestone = ScheduleMilestone.Create(UID:=_uid, updc:=anUpdc, ID:=aScheduleMSDef.ID)
                    Else
                        '' retrieve
                        aMilestone = ScheduleMilestone.Retrieve(UID:=_uid, updc:=anUpdc, ID:=aScheduleMSDef.ID)
                    End If

                    If aMilestone IsNot Nothing Then
                        ' iscache must be kept
                        aMilestone.IsCacheNoSave = isCache
                        '** include
                        _milestoneCollection.Add(aMilestone)
                    Else
                        CoreMessageHandler(message:="Milestone for uid #" & _uid & " from definition '" & aScheduleMSDef.ScheduleType & "' could not be created or retrieved", _
                                           arg1:=aScheduleMSDef.ID, tablename:=ConstTableID, subname:="Schedule.CreateDEfaultMilestones", _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    End If

                End If

            Next aScheduleMSDef

            Return True
        End Function
        ''' <summary>
        ''' handles the OnCreated Event to create also all the milestones dependend on the schedule type
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Call Me.CreateDefaultMilestones()
        End Sub
        ''' <summary>
        ''' Property Change Handler
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnPropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles MyBase.PropertyChanged
            If e.PropertyName = ConstFNTypeid Then
                CreateDefaultMilestones()
            End If
        End Sub
        ''' <summary>
        ''' handles the OnCreated Event to create also all the milestones dependend on the schedule type
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreating
            Dim anUpdc As Long? = e.Record.GetValue(ConstFNUpdc)
            Dim aWorkspaceID As String = e.Record.GetValue(ConstFNWorkspaceID)
            '* new key ?!
            If Not anUpdc.HasValue OrElse anUpdc = 0 Then
                If Not Me.GetMaxUpdc(max:=anUpdc.Value, workspaceID:=aWorkspaceID) Then
                    Call CoreMessageHandler(message:=" primary key values could not be created - cannot create object", _
                                            subname:="Schedule.create", tablename:=PrimaryTableID, messagetype:=otCoreMessageType.InternalError)
                    e.AbortOperation = True
                    Exit Sub
                End If
                '* increase
                anUpdc += 1
                e.Record.SetValue(ConstFNUpdc, anUpdc)
            End If
        End Sub
        ''' <summary>
        ''' create a persistable schedule
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name=constFNupdc></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="SCHEDULETYPEID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal uid As Long, _
                                Optional ByVal updc As Long = 0, _
                                Optional ByVal workspaceID As String = "", _
                                Optional ByVal scheduletypeid As String = "") As Schedule


            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If scheduletypeid = "" Then scheduletypeid = CurrentSession.DefaultScheduleTypeID
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNUid, uid)
                .SetValue(ConstFNUpdc, updc)
                .SetValue(ConstFNWorkspaceID, workspaceID)
                .SetValue(ConstFNTypeid, scheduletypeid)
            End With

            Return ormDataObject.CreateDataObject(Of Schedule)(aRecord, checkUnique:=True)

        End Function

        '**** getDeliverableTrack -> get Track for the corresponding Deliverable (same uid)
        '****
        ''' <summary>
        ''' retrieve the corresponding deliverableTrack
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDeliverableTrack() As Track
            Throw New NotImplementedException
            'If Not IsAlive(subname:="GetDeliverableTrack") Then Return Nothing
            'Dim aTrackDef As Track
            'Dim aTarget As CurrentTarget = CurrentTarget()

            'If IsLoaded Then
            '    If Not aTarget.Inject(Uid:=Me.Uid, workspaceID:=Me.workspaceID) Then
            '        aTarget.UPDC = 0
            '    End If
            '    If Track.Retrieve(deliverableUID:=Me.Uid, _
            '                        scheduleUID:=Me.Uid, _
            '                        scheduleUPDC:=Me.Updc, _
            '                        targetUPDC:=aTarget.UPDC) Then
            '        GetDeliverableTrack = aTrackDef
            '    End If
            'End If

            'GetDeliverableTrack = Nothing
        End Function

        '******* existsMilestone: checks if the Milestone by ID exists and is Of Type
        '*******
        ''' <summary>
        ''' if the milestone by id exists
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="MSTYPEID"></param>
        ''' <param name="HASDATA"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasMilestone(ByVal ID As String, _
                                     Optional ByVal mstypeid As otMilestoneType = 0, _
                                     Optional ByVal hasData As Boolean = True) As Boolean
            Dim aVAlue As Object
            Dim aDefSchedule As ScheduleDefinition = Me.GetScheduleDefinition
            Dim aRealID As String = ""
            'Dim aDefScheduleMilestone As clsOTDBDefScheduleMilestone = clsOTDBDefScheduleMilestone.Retrieve(scheduletype:=Me.Typeid, ID:=aRealID)
            Dim aScheduleMilestone As ScheduleMilestone
            Dim aDefMilestone As MileStoneDefinition = MileStoneDefinition.Retrieve(id:=aRealID)
            ID = ID.ToUpper

            If Not IsAlive(subname:="hasMilestone") Then Return False


            ' check aliases
            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return False
            End If
            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=ID)
            If aRealID = "" Then aRealID = ID

            ' get the DefSchedule Milestone
            ' if mstypeid is missing
            If mstypeid = 0 And aDefMilestone IsNot Nothing Then
                mstypeid = aDefMilestone.Typeid
            End If

            ' if milestone exists in members
            If _milestoneCollection.ContainsKey({aRealID}) Then
                aScheduleMilestone = _milestoneCollection.Item({aRealID})
                aVAlue = aScheduleMilestone.Value

                Select Case mstypeid

                    ' check date
                    Case otMilestoneType.[Date]
                        If IsDate(aVAlue) Then
                            If hasData And aVAlue <> constNullDate Then
                                HasMilestone = True
                            ElseIf Not hasData Then
                                HasMilestone = True
                            Else
                                HasMilestone = False
                            End If
                        ElseIf Not hasData Then
                            HasMilestone = True
                        Else
                            HasMilestone = False
                        End If
                        '
                        ' check status
                    Case otMilestoneType.Status
                        If Trim(CStr(aVAlue)) <> "" And hasData Then
                            HasMilestone = True

                        ElseIf Trim(CStr(aVAlue)) = "" And hasData Then
                            HasMilestone = False
                        ElseIf Not hasData Then
                            HasMilestone = True
                        Else
                            HasMilestone = True
                        End If
                End Select

            Else
                HasMilestone = False
                Exit Function
            End If

        End Function
        ''' <summary>
        ''' has the Milestone date data ?!
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasMilestoneDate(ByVal ID As String) As Boolean
            HasMilestoneDate = Me.HasMilestone(ID:=ID, mstypeid:=otMilestoneType.[Date], hasData:=False)
        End Function
        ''' <summary>
        ''' returns true if the milestone has no data or does not exist (optional ifNotExists)
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ifNotExists"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsMilestoneValueMissing(ByVal ID As String, Optional ByVal ifNotExists As Boolean = True) As Boolean
            ' check milestone on data 
            If Not Me.HasMilestone(ID:=ID, hasData:=True) Then
                Return ifNotExists
            Else
                Return False ' false = not missing value
            End If

        End Function


        ''' <summary>
        ''' is the schedule finished
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsFinished() As Boolean
            Dim aVAlue As Object

            If Me.IsLoaded Or Me.IsCreated Then
                '''
                ''' TO DO HACK !!
                If _milestoneCollection.ContainsKey({"BP10"}) Then
                    aVAlue = Me.GetMilestoneValue("bp10")
                    If IsDate(aVAlue) And aVAlue <> constNullDate Then
                        IsFinished = True
                        Exit Function
                    Else
                        IsFinished = False
                        Exit Function
                    End If
                ElseIf Me.Typeid.ToLower = "none" Then
                    IsFinished = True
                    Exit Function
                Else
                    WriteLine("milestone bp10 is missing ?!")
                End If
            End If
        End Function

        '******* returns a TimeInterval for Task
        '*******
        ''' <summary>
        ''' timeinterval for the task
        ''' </summary>
        ''' <param name="TaskTypeID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTimeInterval(TaskTypeID As String) As clsHELPERTimeInterval
            Dim aVAlue As Object
            Dim aTimeInterval As New clsHELPERTimeInterval

            If Not Me.IsLoaded And Not Me.IsCreated Then
                GetTimeInterval = Nothing
                Exit Function
            End If

            Select Case LCase(TaskTypeID)

                ' return the Developement Task
                Case "development"

                    ' determine the end
                    ' HACK !
                    ' CASE 1 we are not ended and have a FC End
                    If Me.IsMilestoneValueMissing("bp4") And Not Me.IsMilestoneValueMissing("bp3") Then
                        aTimeInterval.endcmt = "bp3"
                        aTimeInterval.isActEnd = False
                        ' CASE 2 we are ended and have not a FC end
                    ElseIf Not Me.IsMilestoneValueMissing("bp4") And Me.IsMilestoneValueMissing("bp3") Then
                        aTimeInterval.endcmt = "bp4"
                        aTimeInterval.isActEnd = True
                        ' CASE 3 we have no FAP ends but a PDM Entry or Approval ?!
                        ' we have both ends -> which to take ?!
                    ElseIf Not Me.IsMilestoneValueMissing("bp3") And Not Me.IsMilestoneValueMissing("bp4") Then
                        aTimeInterval.endcmt = "bp4"
                        aTimeInterval.isActEnd = True
                    ElseIf Me.IsMilestoneValueMissing("bp3") And Me.IsMilestoneValueMissing("bp4") And _
                    (Not Me.IsMilestoneValueMissing("bp7") Or Not Me.IsMilestoneValueMissing("bp8") Or Not Me.IsMilestoneValueMissing("bp9") Or Not Me.IsMilestoneValueMissing("bp10")) Then
                        If Not Me.IsMilestoneValueMissing("bp7") Then
                            aTimeInterval.endcmt = "bp7"
                            aTimeInterval.isActEnd = False
                        ElseIf Not Me.IsMilestoneValueMissing("bp8") Then
                            aTimeInterval.endcmt = "bp8"
                            aTimeInterval.isActEnd = True
                        ElseIf Not Me.IsMilestoneValueMissing("bp9") Then
                            aTimeInterval.endcmt = "bp9"
                            aTimeInterval.isActEnd = False
                        ElseIf Not Me.IsMilestoneValueMissing("bp10") Then
                            aTimeInterval.endcmt = "bp10"
                            aTimeInterval.isActEnd = True
                        Else
                            aTimeInterval.endcmt = ""
                        End If

                        ' CASE 4 we have no end atall
                    Else
                        aTimeInterval.endcmt = "no end could be found ?!"
                    End If
                    ' set the end
                    aVAlue = Me.GetMilestoneValue(aTimeInterval.endcmt)
                    If IsDate(aVAlue) And aVAlue <> constNullDate Then
                        aTimeInterval.enddate = CDate(aVAlue)
                    Else
                        aTimeInterval.enddate = constNullDate
                    End If

                    ' determine the start
                    ' CASE 1 we are not started and have a FC Start
                    If Me.IsMilestoneValueMissing("bp12") And Not Me.IsMilestoneValueMissing("bp11") Then
                        aTimeInterval.startcmt = "bp11"
                        aTimeInterval.isActStart = False
                        ' CASE 2 we are started and have not a FC Start
                    ElseIf Not Me.IsMilestoneValueMissing("bp12") And Me.IsMilestoneValueMissing("bp11") Then
                        aTimeInterval.startcmt = "bp12"
                        aTimeInterval.isActStart = True
                        ' CASE 3 we have no starts but a FC Freeze
                    ElseIf Me.IsMilestoneValueMissing("bp12") And Me.IsMilestoneValueMissing("bp11") And Not Me.IsMilestoneValueMissing("bp1") Then
                        aTimeInterval.startcmt = "bp1"
                        aTimeInterval.isActStart = False
                        ' CASE 4 we are started -> what to take ??
                    ElseIf Not Me.IsMilestoneValueMissing("bp11") And Not Me.IsMilestoneValueMissing("bp12") Then
                        aVAlue = DateDiff("d", Me.GetMilestoneValue("bp12"), aTimeInterval.enddate)
                        ' still time fo the proposed end -> take it
                        If aVAlue >= 0 Then
                            aTimeInterval.startcmt = "bp12"
                            aTimeInterval.isActStart = True
                        Else
                            'the actual start is later than the end (actual or fc)
                            ' take the fc start if it fits
                            aVAlue = DateDiff("d", Me.GetMilestoneValue("bp11"), aTimeInterval.enddate)
                            ' still time fo the proposed end -> take it
                            If aVAlue >= 0 Then
                                aTimeInterval.startcmt = "bp11"
                                aTimeInterval.isActStart = False
                            Else
                                ' the start is later than the end ?!
                                aTimeInterval.startcmt = "start of bp11,bp12 is later than the end of" & aTimeInterval.endcmt
                            End If
                        End If
                    End If


                    aVAlue = Me.GetMilestoneValue(aTimeInterval.startcmt)
                    If IsDate(aVAlue) And aVAlue <> constNullDate Then
                        aTimeInterval.startdate = CDate(aVAlue)
                    Else
                        ' error no  valid date in schedule
                    End If

                Case Else
                    System.Diagnostics.Debug.WriteLine("mismatch in getTimeInterval")
            End Select

            GetTimeInterval = aTimeInterval
        End Function

        '**** drawBaseline: creates out of this Schedule a new Baseline and updates all other Objects
        '****
        ''' <summary>
        ''' creates out of this Schedule a new Baseline and updates all other Objects
        ''' </summary>
        ''' <param name="MSGLOG"></param>
        ''' <param name="REFDATE"></param>
        ''' <param name="TIMESTAMP"></param>
        ''' <param name="ForceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DrawBaseline(Optional ByRef MSGLOG As ObjectLog = Nothing, _
                                     Optional ByVal REFDATE As Date = Nothing, _
                                     Optional ByVal TIMESTAMP As Date = Nothing, _
                                     Optional ByVal ForceSerializeToOTDB As Boolean = False) As Boolean

            Dim aTrack As New Track
            Dim allSchedules As New Collection
            Dim allTracks As New Collection
            Dim aSchedule As New Schedule

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    DrawBaseline = False
                    Exit Function
                End If
            End If
            If Not IsCreated And Not IsLoaded Then
                DrawBaseline = False
                Exit Function
            End If

            If IsMissing(REFDATE) Then
                REFDATE = Now
            End If

            'if we have a baseline
            If Me.IsBaseline Then
                Call CoreMessageHandler(message:=" Schedule for uid #" & Me.Uid & " is already baselined with this updc #" & Me.Updc, _
                                      subname:="Schedule.drawBaseline", arg1:=Me.Uid & "." & Me.Updc, break:=False)
                DrawBaseline = True
                Exit Function
            End If

            '** set it
            Me.IsBaseline = True
            '** add BaseLineDate
            Me.BaselineRefDate = REFDATE
            Me.BaselineUPDC = Me.Updc
            Me.IsFrozen = True
            ' add plan version
            If Me.IsFrozen Then
                ' set the revision
                If Me.Revision = "" Then
                    Me.Revision = ConstFirstPlanRevision
                Else
                    Me.Revision = Me.IncreaseRevison(MajorFlag:=True, MinorFlag:=False)
                End If
            End If
            '*** persist
            If Not Me.Persist(timestamp:=TIMESTAMP) Then
                DrawBaseline = False
                Exit Function
            End If


            '** go through all schedules (also me) in the same workspaceID
            '**
            allSchedules = Me.AllByUID(Me.Uid)
            For Each aSchedule In allSchedules
                If aSchedule.workspaceID = Me.workspaceID And aSchedule.CreatedOn >= Me.CreatedOn And _
                   aSchedule.FCupdc >= Me.FCupdc Then
                    '** freeze it if the schedule was not frozen through al later baseline
                    '**
                    ' freeze again ?!
                    If aSchedule.IsFrozen = True Then
                        Call CoreMessageHandler(message:=" Schedule was baselined again at a later point of time", _
                                              subname:="Schedule.drawBaseline", arg1:=Me.Uid & "." & Me.Updc, break:=False)

                    End If
                    If aSchedule.Updc <> Me.Updc Then
                        aSchedule.IsFrozen = True
                        aSchedule.BaselineUPDC = Me.Updc
                        aSchedule.Revision = Me.Revision
                        aSchedule.BaselineRefDate = Me.BaselineRefDate
                        aSchedule.Persist()
                    End If
                    'update the Tracks associated with this schedule (moving targets)
                    allTracks = aTrack.AllByUID(Me.Uid, scheduleUPDC:=aSchedule.Updc)
                    For Each aTrack In allTracks
                        If Not aTrack.UpdateFromSchedule(aSchedule, targetUPDC:=aTrack.TargetUPDC) Then
                            Debug.Assert(False)
                        End If
                    Next aTrack

                End If
            Next aSchedule

            DrawBaseline = True
        End Function


        '******** Increase the Revision in Form VXX.YY
        '********
        ''' <summary>
        ''' increase revision in Form VXX.YY
        ''' </summary>
        ''' <param name="MajorFlag"></param>
        ''' <param name="MinorFlag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function IncreaseRevison(MajorFlag As Boolean, MinorFlag As Boolean) As String
            Dim i, j, k As Integer
            Dim minor As Integer
            Dim major As Integer
            Dim aValue As Object

            If Not IsLoaded And Not IsCreated Then
                IncreaseRevison = ""
                Exit Function
            End If


            If Me.Revision <> "" And UCase(Me.Revision) Like "V*.*" Then
                aValue = Mid(Me.Revision, _
                             InStr(UCase(Me.Revision), "V") + 1, _
                             InStr(Me.Revision, ".") - InStr(UCase(Me.Revision), "V"))
                If IsNumeric(aValue) Then
                    major = CInt(aValue)

                    aValue = (Mid(Me.Revision, InStr(Me.Revision, ".") + 1))
                    If IsNumeric(aValue) Then
                        minor = CInt(aValue)
                    Else
                        minor = 0
                    End If

                    If MajorFlag Then
                        major = major + 1
                        minor = 0
                    ElseIf MinorFlag Then
                        minor = minor + 1
                    End If

                    Me.Revision = "V" & major & "." & minor
                End If
            ElseIf Me.Revision <> "" And UCase(Me.Revision) Like "V*" Then
                aValue = Mid(Me.Revision, _
                             InStr(UCase(Me.Revision), "V") + 1, _
                             Len(Me.Revision) - InStr(UCase(Me.Revision), "V"))
                If IsNumeric(aValue) Then
                    major = CInt(aValue)
                    minor = 0
                    If MajorFlag Then
                        major = major + 1
                        minor = 0
                    ElseIf MinorFlag Then
                        minor = minor + 1
                    End If

                    Me.Revision = "V" & major & "." & minor
                End If

            ElseIf Me.Revision = "" Then
                Me.Revision = ConstFirstPlanRevision
            Else
                Call CoreMessageHandler(message:=("me.revision " & Me.Revision & " not increasable since not in VXX.YY"), arg1:=Me.Revision, _
                                      subname:="Schedule.increaserevision", break:=False)
                Return Me.Revision
            End If
            ' exit
            IncreaseRevison = Me.Revision

        End Function

        '**** publish: create new versions or fully initialize the newly created, set current if changed -> returns the new schedule object
        '****
        ''' <summary>
        ''' publish is a persist with history and baseline integrated functions. It takes either aSchedule.publish if aSchedule was changed or
        ''' aSchedule
        ''' </summary>
        ''' <param name="newschedule"></param>
        ''' <param name="msglog"></param>
        ''' <param name="timestamp"></param>
        ''' <param name="forceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Publish(Optional ByRef workspaceid As String = "", _
                                Optional ByRef msglog As ObjectLog = Nothing, _
                                Optional ByVal timestamp As Date = ot.constNullDate, _
                                Optional ByVal forceSerializeToOTDB As Boolean = False) As Boolean
            Dim aNewUPDC As Long = 0
            Dim isProcessable As Boolean = True
            Dim aCurrSCHEDULE As CurrentSchedule
            Dim aTrack As New Track


            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Publish = False
                    Exit Function
                End If
            End If
            If Not IsCreated And Not IsLoaded Then
                Return False
            End If

            If workspaceid = "" And Me.workspaceID <> "" Then
                workspaceid = Me.workspaceID
            ElseIf Me.workspaceID = "" Then
                workspaceid = CurrentSession.CurrentWorkspaceID
            End If
            If Workspace.Retrieve(id:=workspaceid) Is Nothing Then
                CoreMessageHandler(message:="workspaceID Definition does not exist", arg1:=workspaceid, messagetype:=otCoreMessageType.ApplicationError, _
                                    subname:="Schedule.publish")
                Return False
            End If
            ' set msglog
            If msglog Is Nothing Then
                If _msglog Is Nothing Then
                    _msglog = New ObjectLog
                End If
                msglog = _msglog
                msglog.Create(Me.Msglogtag)
            End If
            ' TIMESTAMP
            If IsMissing(timestamp) Or Not IsDate(timestamp) Then
                timestamp = Now
            End If

            '** if any of the milestones is changed
            '**
            isProcessable = True

            '** condition
            If _haveMilestonesChanged Then

                '****
                '**** 1. CHECK Conditions of the schedule
                '****
                '**** 1.1 check ascending order

                '**** 1.2 check condition of providing actuals in the past
                '****                     or forecasts in the past

                '**** 2. CHECK Condtions of Approval Queue
                '****

                '**** 3. Publish new Schedule
                '****

                If Me.IsLoaded Or Me.IsCreated Then
                    If Not Me.GetMaxUpdc(max:=aNewUPDC, workspaceID:=workspaceid) Then
                        CoreMessageHandler(message:="no updc for schedule #" & Me.Uid.ToString & " could be created", arg1:=workspaceid, _
                                            subname:="Schedule.Publish", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        '** here we change our IDENTITY UPDC !
                        aNewUPDC += 1
                        _updc = aNewUPDC
                    End If
                    Me.workspaceID = workspaceid

                End If


                If isProcessable Then
                    If Me.IsForecastChanged Then
                        Me.Incfcupdc()
                        Me.LastForecastUpdate = timestamp
                        '**
                        '** right-move of new Schedule if we are frozen
                        If Me.IsFrozen Then
                            '** HACK !
                            Dim aNewDate As Date
                            Dim anOldDate As Date

                            aNewDate = Me.GetMilestoneValue("bp9")
                            anOldDate = Me.GetMilestoneValue("bp9", ORIGINAL:=True) ' 
                            If Not IsNull(aNewDate) And Not IsNull(anOldDate) Then
                                If IsDate(aNewDate) And IsDate(anOldDate) Then
                                    If DateDiff("d", anOldDate, aNewDate) >= 0 Then
                                        '** Now we should approve ??!
                                        '** at least we increase the revision count
                                        Me.Revision = Me.IncreaseRevison(MajorFlag:=False, MinorFlag:=True)
                                    End If
                                End If
                            End If

                        End If
                    End If
                    ' save it
                    isProcessable = Me.Persist(timestamp)

                    '** change THE current schedule
                    '**
                    aCurrSCHEDULE = CurrentSchedule.Retrieve(UID:=Me.Uid, workspaceID:=Me.workspaceID)
                    If aCurrSCHEDULE Is Nothing Then
                        Call aCurrSCHEDULE.Create(UID:=Me.Uid, workspaceID:=Me.workspaceID)
                    End If
                    aCurrSCHEDULE.UPDC = Me.Updc
                    If isProcessable Then
                        isProcessable = aCurrSCHEDULE.Persist(timestamp)
                    End If
                    '** update Track
                    If isProcessable Then
                        Call aTrack.UpdateFromSchedule(Me, workspaceID:=Me.workspaceID, persist:=True, checkGAP:=True)
                    End If
                Else
                    isProcessable = False
                    Debug.Assert(False)

                End If
            ElseIf IsChanged Then
                '**** save without Milestone checking
                isProcessable = Me.Persist(timestamp:=timestamp)
                '** update Track
                Call aTrack.UpdateFromSchedule(Me, workspaceID:=Me.workspaceID, persist:=True, checkGAP:=True)
            Else
                '** nothing changed
                '***
                Publish = False
                Exit Function
            End If

            Publish = isProcessable
        End Function

        ' •———————————————————————————————————————————————————————————•
        ' | ''' <summary>                                             |
        ' | ''' Update the record                                     |
        ' | ''' </summary>                                            |
        ' | ''' <returns></returns>                                   |
        ' | ''' <remarks></remarks>                                   |
        ' | Public Function UpdateRecord() As Boolean                 |
        ' |     Dim aTable As iormDataStore                              |
        ' |     Dim i As Integer                                      |
        ' |     Dim fieldname As String                               |
        ' |     Dim aVAlue As Object                                  |
        ' |                                                           |
        ' |     '* init                                               |
        ' |     If Not Me.IsInitialized Then                          |
        ' |         If Not Me.Initialize() Then                       |
        ' |             UpdateRecord = False                          |
        ' |             Exit Function                                 |
        ' |         End If                                            |
        ' |     End If                                                |
        ' |     If Not IsLoaded And Not IsCreated Then                |
        ' |         UpdateRecord = False                              |
        ' |         Exit Function                                     |
        ' |     End If                                                |
        ' |                                                           |
        ' |                                                           |
        ' |     'On Error GoTo errorhandle                            |
        ' |     Call Me.Record.SetValue(ConstFNUid, _uid)             |
        ' |     'Call me.record.setValue("drev", s_drev)              |
        ' |     Call Me.Record.SetValue(ConstFNWorkspace, _workspace) |
        ' |     Call Me.Record.SetValue("cmt", _comment)              |
        ' |     Call Me.Record.SetValue(ConstFNUpdc, _updc)           |
        ' |     Call Me.Record.SetValue(ConstFNfcupdc, _fcupdc)       |
        ' |     Call Me.Record.SetValue(ConstFNRequCap, _requ)        |
        ' |     Call Me.Record.SetValue(ConstFNUsedCap, _used)        |
        ' |     Call Me.Record.SetValue(ConstFNTypeid, _typeid)       |
        ' |     Call Me.Record.SetValue("ufdt", _ufdt)                |
        ' |     Call Me.Record.SetValue("checkedon", _checkedOn)      |
        ' |     Call Me.Record.SetValue("pstatus", _pstatus)          |
        ' |     Call Me.Record.SetValue("resp", _planner)             |
        ' |     Call Me.Record.SetValue("lcstatus", _lfcstatus)       |
        ' |     Call Me.Record.SetValue("plrev", _plrev)              |
        ' |     Call Me.Record.SetValue("isfrozen", _isFrozen)        |
        ' |     Call Me.Record.SetValue("isbaseline", _isBaseline)    |
        ' |     Call Me.Record.SetValue("bldate", _baselineDate)      |
        ' |     Call Me.Record.SetValue("blupdc", _baselineUPDC)      |
        ' |     Call Me.Record.SetValue("fcupdon", _fcUpdatedOn)      |
        ' |                                                           |
        ' |                                                           |
        ' |     'fill the Milestone Dictionary                        |
        ' |     'aTable = getOTDBTableClass(ourTableName)             |
        ' |     'For i = 1 To aTable.NoFields                         |
        ' |     ' fieldname = aTable.getFieldName(i)                  |
        ' |     'If UCase(fieldname) Like "BP*" Then                  |
        ' |     'aVAlue = getMilestone(LCase(fieldname))              |
        ' |     'If Not isNull(aVAlue) Then                           |
        ' |     'Call Me.Record.setValue(fieldname, aVAlue)           |
        ' |     'End If                                               |
        ' |     'End If                                               |
        ' |     'Next i                                               |
        ' |                                                           |
        ' |     UpdateRecord = True                                   |
        ' | End Function                                              |
        ' •———————————————————————————————————————————————————————————• */

        ''' <summary>
        ''' Feeding Event 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnFeeding(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnFeeding
            ' set last forecast update
            If Me.IsForecastChanged Then
                Me.LastForecastUpdate = e.Timestamp
            End If
        End Sub

        ''' <summary>
        ''' onPersisted Handler for reseting
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub Schedule_OnPersisted(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnPersisted
            _isForeCastChanged = False
            _haveMilestonesChanged = False
        End Sub

        ''' <summary>
        ''' clones an object
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkarray() As Object) As Schedule Implements iotCloneable(Of OnTrack.Scheduling.Schedule).Clone

            Dim aNewRecord As ormRecord
            Dim aMember As ScheduleMilestone
            Dim aCloneMember As ScheduleMilestone

            If Not IsAlive(subname:="Clone") Then Return Nothing

            Try

                If Not Feed() Then
                    Return Nothing
                End If

                '*** key ?
                If Updc = 0 Then
                    If Not Me.GetMaxUpdc(max:=pkarray(1), workspaceID:=Me.workspaceID) Then
                        Call CoreMessageHandler(message:="cannot create unique primary key values - abort clone", arg1:=pkarray, _
                                                     tablename:=PrimaryTableID, messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                    pkarray(1) += 1
                End If
                '*** now we copy the object
                Dim aNewObject As Schedule = MyBase.Clone(Of Schedule)(pkarray)
                If Not aNewObject Is Nothing Then
                    aNewRecord = aNewObject.Record
                    ' overwrite the primary keys
                    Call aNewRecord.SetValue(Me.ConstFNUid, pkarray(0))
                    Call aNewRecord.SetValue(Me.ConstFNUpdc, pkarray(1))

                    ' actually here it we should clone all members too !

                    If InfuseDataObject(record:=aNewRecord, dataobject:=aNewObject) Then
                        ' now clone the Members (Milestones)
                        For Each aMember In _milestoneCollection
                            aCloneMember = aMember.Clone(UID:=Uid, updc:=Updc, ID:=aMember.ID)
                            If Not aCloneMember Is Nothing Then
                                Call aNewObject.Milestones.Add(aCloneMember)
                            End If
                        Next
                        Return aNewObject
                    Else
                        Return Nothing
                    End If
                End If

                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(subname:="Schedule.Clone", exception:=ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' clone the object and its members
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(Optional ByVal updc As Long = 0) As Schedule
            Dim pkArray() As Object = {Me.Uid, updc}
            Return Me.Clone(pkarray:=pkArray)
        End Function

        ''' <summary>
        ''' clone this schedule to a specific workspaceID by id
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <param name="setCurrSchedule"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CloneToWorkspace(ByVal workspaceID As String, _
                                        ByVal UID As Long, _
                                        Optional ByVal updc As Long = 0, _
                                        Optional ByVal setCurrSchedule As Boolean = False) As Boolean

            Dim aNewObject As Schedule
            Dim newRecord As ormRecord
            Dim aWorkspace As Workspace
            Dim aCurrSCHEDULE As CurrentSchedule
            Dim newUPDC As Long

            If Not IsAlive(subname:="CloneToWorkspace") Then Return False

            '**
            aWorkspace = Workspace.Retrieve(id:=workspaceID)
            If aWorkspace Is Nothing Then
                Call CoreMessageHandler(arg1:=workspaceID, subname:="Schedule.cloneToWorkspace", message:="couldn't load workspace")
                Return False
            End If

            ' get the new updc
            If Me.GetMaxUpdc(max:=newUPDC, workspaceID:=workspaceID) Then
                If newUPDC = 0 Then
                    newUPDC = aWorkspace.MinScheduleUPDC
                Else
                    newUPDC = newUPDC + 1
                End If
            End If

            '** clone
            aNewObject = Me.Clone(updc:=newUPDC)
            If aNewObject Is Nothing Then
                Call CoreMessageHandler(arg1:=workspaceID, subname:="Schedule.cloneToWorkspace", _
                                      message:="couldn't clone schedule (" & Me.Uid & "," & Me.Updc & ") to new updc(" & newUPDC)
                CloneToWorkspace = False
                Exit Function
            End If

            '** set the workspaceID !
            aNewObject.workspaceID = workspaceID
            CloneToWorkspace = aNewObject.Persist

            ' set the currschedule
            If setCurrSchedule Then
                aCurrSCHEDULE = CurrentSchedule.Retrieve(UID:=Me.Uid, workspaceID:=workspaceID)
                If aCurrSCHEDULE Is Nothing Then
                    aCurrSCHEDULE = CurrentSchedule.Create(UID:=Me.Uid, workspaceID:=workspaceID)
                End If
                aCurrSCHEDULE.UPDC = newUPDC
                'aCurrSchedule.rev = Me.v
                CloneToWorkspace = aCurrSCHEDULE.Persist
            End If
        End Function
        ''' <summary>
        ''' retrieve maximum update count from the datastore
        ''' </summary>
        ''' <param name="max">the max to be set</param>
        ''' <param name="workspaceID">optional workspaceID</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxUpdc(ByRef max As Long, Optional ByVal workspaceID As String = "") As Boolean
            Dim aWorkspaceDef As New Workspace
            Dim mymax As Long
            Dim pkarray() As Object = {workspaceID}

            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID

            Try
                ' get
                Dim aStore As iormDataStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="getmaxupdc", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = "max(updc)"
                    aCommand.Where = "uid=@uid and wspace=@wspace"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@uid", ColumnName:=ConstFNUid, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@wspace", ColumnName:=ConstFNWorkspaceID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@uid", value:=Uid)
                aCommand.SetParameterValue(ID:="@wspace", value:=workspaceID)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect
                aWorkspaceDef = Workspace.Retrieve(id:=workspaceID)

                If theRecords.Count > 0 Then
                    If Not IsNull(theRecords.Item(0).GetValue(1)) And IsNumeric(theRecords.Item(0).GetValue(1)) Then
                        mymax = CLng(theRecords.Item(0).GetValue(1))
                        If Not aWorkspaceDef Is Nothing Then
                            If mymax >= (aWorkspaceDef.MaxScheduleUPDC - 10) Then
                                Call CoreMessageHandler(showmsgbox:=True, message:="Number range for workspaceID ends", _
                                                      arg1:=workspaceID, messagetype:=otCoreMessageType.ApplicationWarning)
                            End If
                        End If
                    Else
                        If aWorkspaceDef IsNot Nothing Then
                            mymax = aWorkspaceDef.MinScheduleUPDC
                        Else
                            GetMaxUpdc = False
                        End If

                    End If
                    GetMaxUpdc = True

                Else
                    If aWorkspaceDef IsNot Nothing Then
                        mymax = aWorkspaceDef.MinScheduleUPDC
                    Else
                        GetMaxUpdc = False
                    End If
                End If
                If GetMaxUpdc Then
                    max = mymax
                End If
                Return GetMaxUpdc
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, exception:=ex, subname:="Schedule.getMaxUPDC")
                Return False
            End Try
        End Function

#Region "XChange Functions"
        ''' <summary>
        ''' run XChange on an envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXPrecheck(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean Implements iotXChangeable.RunXPreCheck

        End Function
        ''' <summary>
        ''' run XChange on an envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(ByRef envelope As XEnvelope, Optional ByRef msglog As ObjectLog = Nothing) As Boolean Implements iotXChangeable.RunXChange

            Dim aXCmd As otXChangeCommandType = envelope.GetObjectXCmd(objectname:=Me.ObjectID)
            Dim aValue, anOldValue As Object

            '* load the schedule from the envelope
            If Not Me.Inject(envelope:=envelope) Then
                ' could not load the envelope -> Add ?!
                Dim anUID As Object = envelope.GetSlotValueByObjectEntryName(entryname:=Me.ConstFNUpdc, objectname:=Me.ObjectID)
                Dim aTypeid As String = envelope.GetSlotValueByObjectEntryName(entryname:=ScheduleDefinition.ConstFNType, objectname:=ScheduleDefinition.ConstObjectID)
                Dim anWSId As String = envelope.GetSlotValueByXID(xid:="WS")
                If aXCmd = otXChangeCommandType.UpdateCreate Then
                    If anUID Is Nothing Then
                        CoreMessageHandler(message:="could not load or create new schedule from envelope - uid is missing", subname:="Schedule.RunXChange", messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    Else
                        anUID = CLng(anUID)
                    End If
                    If anWSId Is Nothing Then
                        anWSId = CurrentSession.CurrentWorkspaceID
                    End If
                    If aTypeid Is Nothing Then
                        aTypeid = ""
                    End If
                    ' create new schedule 
                    Me.Create(uid:=anUID, workspaceID:=anWSId, scheduletypeid:=aTypeid)
                    Me.Publish()
                Else
                    Call envelope.MsgLog.AddMsg("203", envelope.Xchangeconfig.Configname, Nothing, Nothing, _
                                           envelope.Xchangeconfig.Configname, anUID & ", <none>")
                    CoreMessageHandler(message:="could not load or create new schedule from envelope", arg1:=anUID, subname:="Schedule.RunXChange", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '* set the milestones
            Select Case aXCmd
                Case otXChangeCommandType.Update, otXChangeCommandType.UpdateCreate

                    '*** change the schedule and the compounds (milestone)
                    '***
                    For Each aSlot In envelope.GetSlotByObject(objectname:=ConstTableID)
                        If aSlot.XAttribute.IsXChanged And Not aSlot.XAttribute.IsReadOnly Then

                            '** publish only on milestones which are compounds
                            If aSlot.XAttribute.IsCompound Then
                                If Me.HasMilestone(ID:=aSlot.XAttribute.XID) Then
                                    If Not aSlot.IsEmpty Then
                                        If Not Me.SetMilestone(ID:=aSlot.XAttribute.XID, Value:=aSlot.DBValue, setNull:=aSlot.IsNull) Then
                                            '*** error
                                        End If
                                    End If
                                Else
                                    '** error
                                End If
                            Else
                                '* change the underlying record
                                Me.Record.SetValue(index:=aSlot.XAttribute.ObjectEntryname, value:=aSlot.DBValue)
                            End If
                        End If

                    Next

                    '** if we have a change
                    If Me.IsChanged Or Me.haveMileStonesChanged Or Me.Record.IsChanged Then
                        If Me.Publish() Then
                            envelope.AddSlotByObjectEntryName(entryname:=ConstFNUpdc, objectname:=Me.ObjectID, value:=Me.Updc, _
                                                        isHostValue:=False, extendXConfig:=True, xcmd:=otXChangeCommandType.Read, isReadonly:=True)
                        Else
                            '*** error !
                        End If

                    End If
                Case otXChangeCommandType.Delete
                    Throw New NotImplementedException
                    Return False
                Case otXChangeCommandType.Duplicate
                    Throw New NotImplementedException
                    Return False
                Case otXChangeCommandType.Read
                    Return envelope.RunXChangeCommand(Me, Me.GetType, msglog:=msglog)
            End Select

        End Function

        ''' <summary>
        ''' loads the schedule from the Store by values from the envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByRef envelope As XEnvelope) As Boolean
            Dim uid As Long
            Dim updc As Long
            Dim wsID As String = ""
            Dim aValue As Object

            '***
            '*** Determine the Primary key of a Schedule
            If Not Me.IsLoaded And Not Me.IsCreated Then
                '** UID
                If envelope.HasSlotByObjectEntryName(entryname:=Me.ConstFNUid, objectname:=Me.ObjectID) Then
                    aValue = envelope.GetSlotValueByObjectEntryName(entryname:=Me.ConstFNUid, objectname:=Me.ObjectID, asHostValue:=False)
                Else
                    aValue = Nothing
                End If
                If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                    CoreMessageHandler(message:="Envelope has no id 'uid'", messagetype:=otCoreMessageType.ApplicationError, _
                                       subname:="Schedule.Inject(Envelope)")
                    If envelope.Xchangeconfig.GetEntryByObjectEntryName(entryname:=Me.ConstFNUid, objectname:=Me.ObjectID) Is Nothing Then
                        Call envelope.MsgLog.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ConstTableID, envelope.Xchangeconfig.Configname)
                    Else
                        Call envelope.MsgLog.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ConstTableID, envelope.Xchangeconfig.Configname)
                    End If

                    Return False
                Else
                    uid = CLng(aValue)
                End If
                '** WS
                If envelope.HasSlotByXID(xid:="WS") Then
                    aValue = envelope.GetSlotValueByXID(xid:="WS", asHostValue:=False)
                    wsID = CStr(aValue)
                Else
                    wsID = CurrentSession.CurrentWorkspaceID
                    envelope.AddSlotByXID(xid:="WS", value:=wsID, isHostValue:=False, extendXConfig:=True, replaceSlotIfExists:=True)
                End If

                '** updc
                If envelope.HasSlotByObjectEntryName(entryname:=Me.ConstFNUpdc, objectname:=Me.ObjectID) Then
                    aValue = envelope.GetSlotValueByObjectEntryName(entryname:=Me.ConstFNUpdc, objectname:=Me.ObjectID, asHostValue:=False)
                Else
                    aValue = Nothing
                End If
                If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                    Dim aCurrSchedule As CurrentSchedule = CurrentSchedule.Retrieve(UID:=uid, workspaceID:=wsID)
                    If aCurrSchedule IsNot Nothing Then
                        updc = aCurrSchedule.UPDC
                        envelope.AddSlotByXID(xid:="SC3", value:=updc, isHostValue:=False, extendXConfig:=True)
                    Else
                        'CoreMessageHandler(message:="Envelope has no determinable id 'SC3'", messagetype:=otCoreMessageType.ApplicationError, _
                        '                   subname:="Schedule.Inject(Envelope)")
                        Return False
                    End If
                Else
                    updc = CLng(aValue)
                End If
                '*** inject myself
                If Not Me.Inject({uid, updc}) Then
                    CoreMessageHandler(message:="could not load the schedule ", arg1:=CStr(uid) & "." & CStr(updc), _
                                       messagetype:=otCoreMessageType.ApplicationError, subname:="Schedule.Inject(Envelope)")
                    Return False
                End If
            Else
                '** exists
                uid = Me.Uid
                envelope.AddSlotByXID(xid:="SC2", value:=uid, isHostValue:=False, extendXConfig:=True)
                updc = Me.Updc
                envelope.AddSlotByXID(xid:="SC3", value:=updc, isHostValue:=False, extendXConfig:=True)
            End If

            Return True
        End Function
        ''' <summary>
        ''' Add Compounds Slots to the Envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddSlotCompounds(ByRef envelope As XEnvelope) As Boolean Implements iotHasCompounds.AddSlotCompounds
            Dim avalue As Object

            If Not Me.Inject(envelope:=envelope) Then
                CoreMessageHandler(message:="Schedule could not be loaded from envelope", subname:="Schedule.AddCompounds", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If
            '***
            '*** Add all compounds to the envelope
            Dim anObjectDef As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=Me.ObjectID)
            For Each anObjectEntry In anObjectDef.GetEntries
                Dim anXEntry As XChange.XChangeObjectEntry = envelope.Xchangeconfig.GetEntryByXID(XID:=anObjectEntry.XID, objectname:=Me.ObjectID)
                If anXEntry IsNot Nothing AndAlso anXEntry.IsCompound Then
                    '** COMPOUNDS ARE ALWAYS MILESTONES FOR SCHEDULES
                    '**
                    avalue = Me.GetMilestoneValue(ID:=anXEntry.XID)
                    If avalue IsNot Nothing Then
                        envelope.AddSlotByXID(xid:=anXEntry.XID, objectname:=Me.ObjectID, value:=avalue, isHostValue:=False)
                    Else
                        ' if its nothing could also mean that we have the mile stone but no value
                    End If
                End If

            Next

            Return True
        End Function
#End Region

    End Class

    ''' <summary>
    ''' Schedule Milestone Class (runtime data of a schedule)
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleMilestone.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, adddeletefieldbehavior:=True, _
        description:="milestone data for schedules")> Public Class ScheduleMilestone
        Inherits ormDataObject
        Implements iormPersistable
        Implements iotCloneable(Of ScheduleMilestone)
        Implements iormInfusable

        Public Const ConstObjectID = "ScheduleMilestone"

        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=2)> Public Const constTableID = "tblScheduleMilestones"

        '** Index
        <ormSchemaIndex(columnname1:=ConstFNUid, columnname2:=ConstFNUpdc)> Public constIndexCompound = ConstDefaultCompoundIndexName

        ''' <summary>
        ''' Primary KEys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUid, _
             primaryKeyordinal:=1, XID:="MST1", aliases:={"SUID"})> Public Const ConstFNUid = Schedule.ConstFNUid

        <ormObjectEntry(referenceObjectEntry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc, _
           primaryKeyordinal:=2, XID:="MST2")> _
        Public Const ConstFNUpdc = Schedule.ConstFNUpdc
        '** link together
        <ormSchemaForeignKey(entrynames:={ConstFNUid, ConstFNUpdc}, foreignkeyreferences:={Schedule.ConstObjectID & "." & Schedule.ConstFNUid, _
                Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc}, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKSchedule = "fkschedules"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, defaultvalue:="", _
            title:="milestone id", Description:="id of the milestone", _
          primaryKeyordinal:=3, XID:="MST3")> Public Const ConstFNID = "id"

        ''' <summary>
        ''' Fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, defaultvalue:="", isnullable:=True, _
           title:="value", Description:="text presentation of the milestone value", XID:="MST4")> Public Const ConstFNvalue = "value"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
          title:="value", Description:="date presentation of the milestone value", XID:="MST5")> Public Const ConstFNvaluedate = "valuedate"

        <ormObjectEntry(typeid:=otDataType.Numeric, isnullable:=True, _
                 title:="value", Description:="numeric presentation of the milestone value", XID:="MST6")> Public Const ConstFNvaluenumeric = "valuenumeric"

        <ormObjectEntry(typeid:=otDataType.Bool, isnullable:=True, _
        title:="value", Description:="bool presentation of the milestone value", XID:="MST7")> Public Const ConstFNvaluebool = "valuebool"

        <ormObjectEntry(typeid:=otDataType.Long, isnullable:=True, _
        title:="value", Description:="long presentation of the milestone value", XID:="MST8")> Public Const ConstFNvaluelong = "valuelong"

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=otDataType.Date, dbdefaultvalue:="6", _
        title:="datatype", Description:="datatype of the milestone value", XID:="MST10")> Public Const ConstFNDatatype = "datatype"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
        title:="is a forecast", Description:="true if the milestone is a forecast", XID:="MST11")> Public Const ConstFNIsForecast = "isforecast"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
        title:="is a status", Description:="true if the milestone is a status", XID:="MST12")> Public Const ConstFNIsStatus = "isstatus"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
        title:="is enabled", Description:="true if the milestone is enabled", XID:="MST13")> Public Const ConstFNIsEnabled = "isenabled"

        <ormObjectEntry(referenceObjectEntry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
             Description:="workspaceID ID of the schedule", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNWorkspace = Workspace.ConstFNID

        ' deactivate ForeignKEy we do not have this object in domains
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otDataType.Text, isnullable:=True, _
                     title:="comment", Description:="comment", XID:="MST14")> Public Const ConstFNcmt = "cmt"


        ' fields

        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""

        '<ormEntryMapping(EntryName:=ConstFNUid)> -> special infuse
        Private _value As Object

        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType
        <ormEntryMapping(EntryName:=ConstFNcmt)> Private _cmt As String = ""
        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspaceID As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsStatus)> Private _isStatus As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsEnabled)> Private _isEnabled As Boolean = True

        'Private s_isActual As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsForecast)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""


        'dynamic
        Private _loadedFromHost As Boolean
        Private _savedToHost As Boolean
        Private _isCacheNoSave As Boolean    ' if set this is not saved since taken from another updc
        Private _msglog As New ObjectLog


#Region "Properties"

        Public Property IsCacheNoSave() As Boolean
            Get
                IsCacheNoSave = _isCacheNoSave
            End Get
            Set(value As Boolean)
                If value Then
                    _isCacheNoSave = True
                Else
                    _isCacheNoSave = False
                End If
            End Set
        End Property
        ''' <summary>
        ''' unique Tag
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UniqueTag()
            Get
                Return ConstDelimiter & constTableID & ConstDelimiter & _uid & ConstDelimiter & _updc & ConstDelimiter & _id & ConstDelimiter

            End Get
        End Property
        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = UniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get

        End Property
        ''' <summary>
        ''' get the uid 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UID() As Long
            Get

                UID = _uid
            End Get

        End Property
        ''' <summary>
        ''' get the updc of the schedule
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Updc() As Long
            Get
                Updc = _updc
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ID() As String
            Get
                Return _id
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the Workspace ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WorkspaceID() As String
            Get
                Return _workspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspace, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value() As Object
            Get
                Return _value
            End Get
            Set(ByVal value As Object)
                SetValue(ConstFNvalue, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the datatype
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype() As otDataType
            Get
                Return _datatype
            End Get
            Set(value As otDataType)
                SetValue(ConstFNDatatype, value)
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
                Return _cmt
            End Get
            Set(value As String)
                SetValue(ConstFNcmt, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the forecast flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsForecast() As Boolean
            Get
                Return _isForecast
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsForecast, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if the milestone is a actual
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActual() As Boolean
            Get
                Return Not _isForecast
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsForecast, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if the milestone is a status
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsStatus() As Boolean
            Get
                Return _isStatus
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if the milestone is enabled
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsEnabled() As Boolean
            Get
                Return _isEnabled
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsEnabled, value)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            _workspaceID = CurrentSession.CurrentWorkspaceID
            Return MyBase.Initialize()
        End Function

        ''' <summary>
        ''' Infuse the data object by record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused
            Dim aVAlue As Object


            Try
                '*** overload it from the Application Container
                '***
                If Me.SerializeWithHostApplication Then
                    If overloadFromHostApplication(Record) Then
                        _loadedFromHost = True
                    End If
                End If

                _datatype = CLng(Record.GetValue(ConstFNDatatype))
                aVAlue = Record.GetValue(ConstFNvalue)
                ' select on Datatype
                Select Case _datatype

                    Case otDataType.Numeric
                        aVAlue = Record.GetValue(ConstFNvaluenumeric)
                        _value = CDbl(aVAlue)
                    Case otDataType.Text

                        _value = CStr(aVAlue)
                    Case otDataType.Runtime, otDataType.Formula, otDataType.Binary
                        _value = ""
                        Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", messagetype:=otCoreMessageType.ApplicationError, _
                                              message:="runtime, formular, binary can't infuse", msglog:=_msglog, arg1:=aVAlue)
                    Case otDataType.[Date], otDataType.Timestamp
                        aVAlue = Record.GetValue(ConstFNvaluedate)
                        If IsDate(aVAlue) Then
                            _value = CDate(aVAlue)
                        Else
                            Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", _
                                            message:="Value supposed to be a date cannot be converted", _
                                            messagetype:=otCoreMessageType.ApplicationError, _
                                            msglog:=_msglog, arg1:=aVAlue)

                        End If

                    Case otDataType.[Long]
                        aVAlue = Record.GetValue(ConstFNvaluelong)
                        _value = CLng(aVAlue)
                    Case otDataType.Bool
                        aVAlue = Record.GetValue(ConstFNvaluebool)
                        _value = CBool(aVAlue)
                    Case otDataType.Memo
                        _value = CStr(aVAlue)
                    Case Else
                        Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", _
                                              message:="unknown datatype to be infused", msglog:=_msglog, arg1:=aVAlue)
                End Select


            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ScheduleMilestone.Infuse")
            End Try


        End Sub

        ''' <summary>
        ''' Load and infuse the schedule milestone from the data store by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal UID As Long, ByVal updc As Long, ByVal ID As String) As ScheduleMilestone
            Dim pkarray() As Object = {UID, updc, ID}
            Return ormDataObject.Retrieve(Of ScheduleMilestone)(pkArray:=pkarray)
        End Function

        ''' <summary>
        ''' Update the record from the properties
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnFeedRecord(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnFed


            Try
                '** special Handling
                Call Me.Record.SetValue(ConstFNDatatype, DirectCast(e.DataObject, ScheduleMilestone).Datatype)
                Call Me.Record.SetValue(ConstFNvaluedate, Nothing)
                Call Me.Record.SetValue(ConstFNvaluenumeric, Nothing)
                Call Me.Record.SetValue(ConstFNvaluelong, Nothing)
                Call Me.Record.SetValue(ConstFNvaluebool, Nothing)

                Dim avalue = DirectCast(e.DataObject, ScheduleMilestone).Value

                Select Case DirectCast(e.DataObject, ScheduleMilestone).Datatype

                    Case otDataType.Numeric
                        If IsNumeric(avalue) Then Call Me.Record.SetValue(ConstFNvaluenumeric, CDbl(avalue))
                        Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                    Case otDataType.Text, otDataType.Memo
                        Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                    Case otDataType.Runtime, otDataType.Formula, otDataType.Binary
                        Call CoreMessageHandler(subname:="ScheduleMilestone.persist", _
                                              message:="datatype (runtime, formular, binary) not specified how to be persisted", msglog:=_msglog, arg1:=_datatype)
                    Case otDataType.[Date]
                        If IsDate(avalue) Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CDate(avalue))
                            Call Me.Record.SetValue(ConstFNvalue, Format(avalue, "dd.mm.yyyy"))
                        Else
                            Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                        End If
                    Case otDataType.[Long]
                        If IsNumeric(avalue) Then Call Me.Record.SetValue(ConstFNvaluelong, CLng(avalue))
                        Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                    Case otDataType.Timestamp
                        If IsDate(avalue) Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CDate(avalue))
                            Call Me.Record.SetValue(ConstFNvalue, Format(avalue, "dd.mm.yyyy hh:mm:ss"))
                        Else
                            Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                        End If
                    Case otDataType.Bool
                        If avalue = "" Or IsEmpty(avalue) Or IsNull(avalue) Or avalue Is Nothing Then
                            Call Me.Record.SetValue(ConstFNvaluebool, False)
                        ElseIf avalue = True Or avalue = False Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CBool(avalue))
                        Else
                            Call Me.Record.SetValue(ConstFNvaluedate, True)
                        End If
                        Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                    Case Else
                        Call Me.Record.SetValue(ConstFNvalue, CStr(avalue))
                        Call CoreMessageHandler(subname:="ScheduleMilestone.persist", _
                                              message:="datatype not specified how to be persisted", msglog:=_msglog, arg1:=_datatype)
                End Select



            Catch ex As Exception
                Call CoreMessageHandler(subname:="ScheduleMilestone.UpdateRecord", exception:=ex)
            End Try
        End Sub

        ''' <summary>
        ''' handler for the serialize with Host Application 
        ''' </summary>
        ''' <param name="aTimestamp"></param>
        ''' <param name="forceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Sub ScheduleMilestone_OnPersist(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnPersisting
            '*** overload it from the Application Container
            '***
            If Me.SerializeWithHostApplication Then
                If overwriteToHostApplication(Me.Record) Then
                    _savedToHost = True
                    e.Proceed = False
                End If
            End If
        End Sub

        ''' <summary>
        ''' create a persistable schedule milestone by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <param name="ID"></param>
        ''' <param name="FORCE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal UID As Long, ByVal updc As Long, ByVal ID As String) As ScheduleMilestone
            Dim pkarray() As Object = {UID, updc, ID}
            Return ormDataObject.CreateDataObject(Of ScheduleMilestone)(pkarray, checkUnique:=True)
        End Function

        ''' <summary>
        ''' Clone the schedule milestone by new primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(ByVal UID As Long, ByVal updc As Long, ByVal ID As String) As ScheduleMilestone
            Dim pkarray() As Object = {UID, updc, ID}
            Return Me.Clone(pkarray)
        End Function
        ''' <summary>
        ''' clone the data object by primary key array
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(pkArray() As Object) As ScheduleMilestone Implements iotCloneable(Of ScheduleMilestone).Clone
            Dim aNewObject As ScheduleMilestone

            If Not IsLoaded And Not IsCreated Then
                Return Nothing
            End If
            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Clone = Nothing
                    Exit Function
                End If
            End If

            'update our Record
            If Not Me.Feed() Then
                Clone = Nothing
                Exit Function
            End If

            aNewObject = MyBase.Clone(Of ScheduleMilestone)(pkArray)
            If Not aNewObject Is Nothing Then
                ' overwrite the primary keys
                Call aNewObject.Record.SetValue(ConstFNUid, UID)
                Call aNewObject.Record.SetValue(ConstFNUpdc, Updc)
                Call aNewObject.Record.SetValue(ConstFNID, ID)
                Return aNewObject
            End If

            Return Nothing
        End Function


    End Class


    ''' <summary>
    ''' the current schedule class links the current schedule updc to a scheduled object 
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleLink.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, adddeletefieldbehavior:=True, _
        description:="link definitions between schedules and other business objects")> _
    Public Class ScheduleLink
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "ScheduleLink"

        '** Schema Table
        <ormSchemaTable(version:=1)> Public Const ConstTableID = "tblScheduleLinks"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNToObject, columnname2:=ConstFNToUID, columnname3:=ConstFNFromObject, columnname4:=ConstFNFromUID)> Public Const ConstIndTag = "used"

        ''' <summary>
        ''' Primary key of the schedule link object
        ''' FROM an ObjectID, UID, MS ("" if null)
        ''' TO   an OBJECTID, UID, MS
        ''' 
        ''' links a deliverable or other business objects with a schedule
        ''' also capable of linking schedules to schedules or milestones of schedules to schedules
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        ''' 

        ''' from Section
        ''' 
        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, _
            validationPropertyStrings:={ObjectValidationProperty.NotEmpty, ObjectValidationProperty.UseLookup}, _
            LookupPropertyStrings:={LookupProperty.UseAttributeValues}, _
            values:={Deliverable.ConstObjectID}, dbdefaultvalue:=Deliverable.ConstObjectID, defaultvalue:=Deliverable.ConstObjectID, _
            XID:="SL1", title:="Linked From Object", description:="object link from the scheduled object")> _
        Public Const ConstFNFromObject = "FROMOBJECTID"

        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=2, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="SL2", title:="Linked from UID", description:="uid link from the scheduled object")> _
        Public Const ConstFNFromUID = "FROMUID"

        <ormObjectEntry(referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, primarykeyordinal:=3, _
            dbdefaultValue:="", _
             properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
             LookupPropertyStrings:={LookupProperty.UseAttributeReference}, _
            XID:="SL3", title:="Linked from Milestone", description:="uid link from the scheduled object milestone")> _
        Public Const ConstFNFromMilestone = "FROMMS"

        ''' <summary>
        ''' fields
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, _
             properties:={ObjectEntryProperty.Keyword}, _
             validationPropertyStrings:={ObjectValidationProperty.NotEmpty, ObjectValidationProperty.UseLookup}, _
             LookupPropertyStrings:={LookupProperty.UseAttributeValues}, _
            values:={Schedule.ConstObjectID}, dbdefaultvalue:=Schedule.ConstObjectID, defaultvalue:=Schedule.ConstObjectID, _
            XID:="SL4", title:="Linked to Object", description:="object link to the scheduled object")> _
        Public Const ConstFNToObject = "ToObjectID"

        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=5, dbdefaultvalue:="0", lowerrange:=0, _
            XID:="SL5", title:="Linked to UID", description:="uid link to the scheduled object")> _
        Public Const ConstFNToUID = "TOUID"

        <ormObjectEntry(referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, _
            primarykeyordinal:=6, dbdefaultValue:="", _
             properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.UseLookup}, _
             LookupPropertyStrings:={LookupProperty.UseAttributeReference}, _
            XID:="SL6", title:="Linked to Milestone", description:="uid link to the scheduled object milestone")> _
        Public Const CONSTFNTOMS = "TOMS"

        ' deactivate ForeignKEy we do not have this object in domains
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, _
            XID:="SL7", title:="Linke Type", description:="object link type")> Public Const ConstFNTypeID = "typeid"

        '** Mapping
        <ormEntryMapping(EntryName:=ConstFNFromObject)> Private _fromObjectID As String
        <ormEntryMapping(EntryName:=ConstFNFromUID)> Private _fromUID As Long
        <ormEntryMapping(EntryName:=ConstFNFromMilestone)> Private _FromMilestone As String
        <ormEntryMapping(EntryName:=ConstFNToObject)> Private _ToObjectID As String
        <ormEntryMapping(EntryName:=ConstFNToUID)> Private _ToUID As Long
        <ormEntryMapping(EntryName:=CONSTFNTOMS)> Private _ToMilestone As String
        <ormEntryMapping(EntryName:=ConstFNTypeID)> Private _type As otScheduleLinkType
        ''' <summary>
        ''' constructor of Current schedule
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "properties"

        ''' <summary>
        ''' Gets or sets the type.
        ''' </summary>
        ''' <value>The type.</value>
        Public Property Type() As otScheduleLinkType
            Get
                Return Me._type
            End Get
            Set(value As otScheduleLinkType)
                Me._type = value
            End Set
        End Property

        ''' <summary>
        ''' gets the linking Object ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromObjectID() As String
            Get
                Return _fromObjectID

            End Get

        End Property
        ''' <summary>
        ''' gets linking Object UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromUID() As Long
            Get
                Return _fromUID
            End Get

        End Property
        ''' <summary>
        ''' gets the linking Milestone or "" if none
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property FromMilestone() As String
            Get
                Return _FromMilestone
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the linked Object ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToObjectID() As String

            Get
                Return _ToObjectID
            End Get
            Set(value As String)
                SetValue(ConstFNToObject, value)
            End Set

        End Property
        ''' <summary>
        ''' gets or sets the linking object UID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToUid() As Long
            Get
                Return _ToUID
            End Get
            Set(value As Long)
                SetValue(ConstFNToUID, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the linking Milestone or "" if not applicable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToMilestone() As String
            Get
                Return _ToMilestone
            End Get
            Set(value As String)
                SetValue(CONSTFNTOMS, value)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Event Handler for on Creating for validating the keys
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub OnCreating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreating

        End Sub

        ''' <summary>
        ''' Event Handler for validating
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnValidating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnValidating

        End Sub
        ''' <summary>
        ''' create a persitable link object
        ''' </summary>
        ''' <param name="fromid"></param>
        ''' <param name="fromuid"></param>
        ''' <param name="toid"></param>
        ''' <param name="touid"></param>
        ''' <param name="frommilestone"></param>
        ''' <param name="tomilestone"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(fromid As String, fromuid As Long, _
                                      Optional frommilestone As String = "") As ScheduleLink
            Dim primarykey As Object() = {fromid, fromuid, frommilestone}
            Return ormDataObject.CreateDataObject(Of ScheduleLink)(primarykey, checkUnique:=True)
        End Function

        ''' <summary>
        ''' retrieve a persitable link object
        ''' </summary>
        ''' <param name="fromid"></param>
        ''' <param name="fromuid"></param>
        ''' <param name="toid"></param>
        ''' <param name="touid"></param>
        ''' <param name="frommilestone"></param>
        ''' <param name="tomilestone"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(fromid As String, fromuid As Long, _
                                      Optional frommilestone As String = "") As ScheduleLink
            Dim primarykey As Object() = {fromid, fromuid, frommilestone}
            Return ormDataObject.Retrieve(Of ScheduleLink)(primarykey)
        End Function
    End Class


    ''' <summary>
    ''' the current schedule class links the current schedule updc  in a given workspace
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=CurrentSchedule.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, _
        adddeletefieldbehavior:=True, adddomainbehavior:=False, _
        description:="pointer (updc) to the current schedule in a workspace")> _
    Public Class CurrentSchedule
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "CurrentSchedule"
        '** Table Schema
        <ormSchemaTable(version:=2)> Public Const ConstTableID = "tblCurrSchedule"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNUID, columnname2:=ConstFNWorkspaceID)> Public Const ConstIndTag = "UIDs"

        '** keys
        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, primarykeyordinal:=1, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            validationPropertyStrings:={ObjectValidationProperty.UseLookup}, lookupPropertyStrings:={LookupProperty.UseAttributeReference})> _
        Public Const ConstFNWorkspaceID = Workspace.ConstFNID

        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUid, primarykeyordinal:=2)> _
        Public Const ConstFNUID = Schedule.ConstFNUid

        '** fields
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc, _
            title:="Current Updc", description:="the current schedule update counter" _
            )> Public Const ConstFNUPDC = Schedule.ConstFNUpdc


        <ormObjectEntry(typeid:=otDataType.Bool, XID:="CS5", title:="Is Active", description:="set if active")> _
        Public Const ConstFNIsActive = "isactive"



        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            useforeignkey:=otForeignKeyImplementation.None)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** Mapping
        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _workspaceID As String
        <ormEntryMapping(EntryName:=ConstFNUID)> Private _uid As Long

        <ormEntryMapping(EntryName:=ConstFNUPDC)> Private _updc As Long
        <ormEntryMapping(EntryName:=ConstFNIsActive)> Private _isActive As Boolean
        ''' <summary>
        ''' constructor of Current schedule
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "properties"
        ''' <summary>
        ''' sets or gets the workspace ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WorkspaceID() As String
            Get
                Return _workspaceID
            End Get
            Set(value As String)
                SetValue(ConstFNWorkspaceID, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the schedule UID
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
        ''' true if the current schedule updc is active / enabled
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActive() As Boolean
            Get
                Return _isActive
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsActive, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the current Schedule Update Counter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UPDC() As Long
            Get
                UPDC = _updc
            End Get
            Set(value As Long)
                SetValue(ConstFNUPDC, value)
            End Set
        End Property
#End Region

        '****** allByUID: "static" function to return a collection of curSchedules by key
        '******
        Public Function allByUID(UID As Long) As Collection
            Dim aCollection As New Collection
            Dim aRECORDCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim Key(0) As Object
            Dim aRECORD As ormRecord
            ' set the primaryKey

            Key(0) = UID

            On Error GoTo error_handler

            aTable = GetTableStore(ConstTableID)
            aRECORDCollection = aTable.GetRecordsBySql(wherestr:=" uid = " & CStr(UID))

            If aRECORDCollection Is Nothing Then
                Me.Unload()
                allByUID = Nothing
                Exit Function
            Else
                For Each aRECORD In aRECORDCollection
                    Dim aNewcurSchedule As New CurrentSchedule
                    If InfuseDataObject(record:=aRECORD, dataobject:=aNewcurSchedule) Then
                        aCollection.Add(Item:=aNewcurSchedule)
                    End If
                Next
                allByUID = aCollection
                Exit Function
            End If

error_handler:

            allByUID = Nothing
            Exit Function
        End Function


        ''' <summary>
        ''' retrieves a a current schedule object from store
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal UID As Long, Optional ByVal workspaceID As String = "") As CurrentSchedule
            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            End If

            Dim aWSObj As Workspace = Workspace.Retrieve(id:=workspaceID)
            '*
            If aWSObj Is Nothing Then
                Call CoreMessageHandler(message:="Can't load workspaceID definition", _
                                      subname:="clsOTDBCurrSchedule.Retrieve", _
                                      arg1:=workspaceID)
                Return Nothing
            End If

            ' check now the stack
            For Each aWorkspaceID In aWSObj.FCRelyingOn
                ' check if in workspaceID any data -> fall back to default (should be base)
                Dim primarykey As Object() = {aWorkspaceID, UID}
                Dim aCurrSchedule As CurrentSchedule = ormDataObject.Retrieve(Of CurrentSchedule)(pkArray:=primarykey)
                If aCurrSchedule IsNot Nothing AndAlso aCurrSchedule.IsActive Then
                    Return aCurrSchedule
                End If
            Next

            Return Nothing
        End Function


        '' <summary>
        ''' load the object by the PrimaryKeys unique and do not overload from other workspaces
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveUnique(ByVal UID As Long, Optional ByVal workspaceID As String = "") As CurrentSchedule
            Dim pkarry() As Object = {Trim(workspaceID), UID}
            Return ormDataObject.Retrieve(Of CurrentSchedule)(pkArray:=pkarry)
        End Function

        '**** getthe TrackDef
        '****
        Public Function GetDeliverableTrack() As Track
            Throw New NotImplementedException
            'Dim aTrackDef As New Track
            'Dim aCurrTarget As New CurrentTarget

            'If IsLoaded Then
            '    '-> UID= ME.UID
            '    If Not aCurrTarget.Inject(uid:=Me.UID, workspaceID:=Me.WorkspaceID) Then
            '        aCurrTarget.UPDC = 0
            '    End If
            '    If aTrackDef.Inject(deliverableUID:=Me.UID, _
            '                        scheduleUID:=Me.UID, _
            '                        scheduleUPDC:=Me.UPDC, _
            '                        targetUPDC:=aCurrTarget.UPDC) Then
            '        GetDeliverableTrack = aTrackDef
            '    End If
            'End If

            'GetDeliverableTrack = Nothing
        End Function

        ''' <summary>
        ''' create the persistable currentschedule object
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal UID As Long, Optional ByVal workspaceID As String = "") As CurrentSchedule
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            Dim primarykeys As Object() = {UID, workspaceID}
            Return ormDataObject.CreateDataObject(Of CurrentSchedule)(pkArray:=primarykeys, checkUnique:=True)
        End Function

    End Class

End Namespace
