REM ***********************************************************************************************************************************************
REM *********** CREATE SCHEMA DATABASE MODULE for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Imports OnTrack.Scheduling
Imports OnTrack.Deliverables
Imports OnTrack.Parts
Imports OnTrack.Configurables
Imports OnTrack.XChange
Imports OnTrack.Calendar
Imports OnTrack.Commons

Namespace OnTrack.Database

    Public Module Installation
        ''' <summary>
        ''' creates or updates all schematas for scheduling objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Scheduling()

            'If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
            '    Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="Installation.createDatabase_Schedule", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
            '    Exit Sub
            'End If


            'Dim aCurrSCHEDULE As New WorkspaceSchedule
            'Dim aCurrTarget As New WorkspaceTarget

            ''Dim aMSPivot As New clsOTDBMilestonePivot
            ''Dim aPivotMSP As New clsOTDBPivotMSPSchedule
            'Dim aDepend As New clsOTDBDependMember
            'Dim aCluster As New clsOTDBCluster

            '''
            ''' LEGACY OBSOLETE CODE SAMPLES 
            ''' 


            'Dim aSchedule As New Schedule
            'If Not Schedule.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", _
            '                                 message:="Schema Schedle couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aSchedule.primaryTableID)

            'End If

            'If Not CurrentTarget.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="currTarget couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Current Target is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aCurrTarget.primaryTableID)


            'End If


            'Dim aMilestone As New ScheduleMilestone
            'If Not ScheduleMilestone.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
            '                                 message:="Schema aMQF couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Milestone is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aMilestone.primaryTableID)


            'End If

            'Dim aScheduleDef As New ScheduleDefinition
            'If Not ScheduleDefinition.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
            '                                 message:="Schema " & aScheduleDef.primaryTableID & " couldn't be created")
            'Else

            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Defintion is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.primaryTableID)


            '    aScheduleDef = New ScheduleDefinition
            '    With aScheduleDef
            '        If Not .Create("full") Then .Inject("full")
            '        .description = "full engineering cycle (3D Design)"
            '        .Persist()

            '    End With
            '    aScheduleDef = New ScheduleDefinition
            '    With aScheduleDef
            '        If Not .Create("pdm") Then .Inject("pdm")
            '        .description = "pdm entry cycle for non 3D Design items"
            '        .Persist()

            '    End With
            '    aScheduleDef = New ScheduleDefinition
            '    With aScheduleDef
            '        If Not .Create("none") Then
            '            .description = "no schedule"
            '            .Persist()
            '        End If
            '    End With
            '    aScheduleDef = New ScheduleDefinition
            '    With aScheduleDef
            '        If Not .Create("nocad") Then .Inject("nocad")
            '        .description = "design for non-mechanical (3D) design"
            '        .Persist()

            '    End With
            'End If


            'Dim aScheduleDefM As New ScheduleMilestoneDefinition
            'If Not ScheduleMilestoneDefinition.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
            '                                 message:="Schema " & aScheduleDefM.primaryTableID & " couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Milestone Defintion is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDefM.primaryTableID)

            '    '****
            '    '**** full
            '    '****

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp11") Then .Inject("full", "bp11")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = True
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp12") Then .Inject("full", "bp12")
            '        .ActualOfFC = "bp11"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp1") Then .Inject("full", "bp1")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()
            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp2") Then .Inject("full", "bp2")
            '        .ActualOfFC = "bp1"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp13") Then .Inject("full", "bp13")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 20
            '        .Description = "ifm status"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp3") Then .Inject("full", "bp3")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = True
            '        .Ordinal = 30
            '        .Description = "fap"
            '        .Persist()
            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp4") Then .Inject("full", "bp4")
            '        .ActualOfFC = "bp3"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 30
            '        .Description = "fap"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp5") Then .Inject("full", "bp5")
            '        .ActualOfFC = ""
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 35
            '        .Description = "dmu status"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp6") Then .Inject("full", "bp6")
            '        .ActualOfFC = ""
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 35
            '        .Description = "dmu date"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp20") Then .Inject("full", "bp20")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 40
            '        .Description = "fc fem"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp21") Then .Inject("full", "bp21")
            '        .ActualOfFC = ""
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 40
            '        .Description = "fem status"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp22") Then .Inject("full", "bp22")
            '        .ActualOfFC = "bp20"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 40
            '        .Description = "fem status date"
            '        .Persist()
            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp7") Then .Inject("full", "bp7")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp8") Then .Inject("full", "bp8")
            '        .ActualOfFC = "bp7"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM

            '        If Not .Create("full", "bp9") Then .Inject("full", "bp9")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = True
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp10") Then .Inject("full", "bp10")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("full", "bp80") Then .Inject("full", "bp80")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 95
            '        .Description = "pdm first approval"
            '        .Persist()
            '    End With
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Defintion for 'FULL' is updated", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.primaryTableID)

            '    '****
            '    '**** nocad
            '    '****

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp11") Then .Inject("nocad", "bp11")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = False
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()
            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp12") Then .Inject("nocad", "bp12")
            '        .ActualOfFC = "bp11"
            '        .IsForecast = False
            '        .IsFacultative = False
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp1") Then .Inject("nocad", "bp1")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsProhibited = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp2") Then .Inject("nocad", "bp2")
            '        .ActualOfFC = "bp3"
            '        .IsForecast = False
            '        .IsProhibited = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp3") Then .Inject("nocad", "bp3")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 30
            '        .Description = "design freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp4") Then .Inject("nocad", "bp4")
            '        .ActualOfFC = "bp3"
            '        .IsForecast = False
            '        .IsFacultative = True
            '        .Ordinal = 30
            '        .Description = "design freeze"
            '        .Persist()

            '    End With

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp7") Then .Inject("nocad", "bp7")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp8") Then .Inject("nocad", "bp8")
            '        .ActualOfFC = "bp7"
            '        .IsForecast = False
            '        .IsFacultative = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp9") Then .Inject("nocad", "bp9")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = False
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp10") Then .Inject("nocad", "bp10")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("nocad", "bp80") Then .Inject("nocad", "bp80")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 95
            '        .Description = "pdm first approval"
            '        .Persist()

            '    End With

            '    '****
            '    '**** pdm
            '    '****

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp11") Then .Inject("pdm", "bp11")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp12") Then .Inject("pdm", "bp12")
            '        .ActualOfFC = "bp11"
            '        .IsForecast = False
            '        .IsFacultative = True
            '        .Ordinal = 10
            '        .Description = "start work"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp1") Then .Inject("pdm", "bp1")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsProhibited = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp2") Then .Inject("pdm", "bp2")
            '        .ActualOfFC = "bp3"
            '        .IsForecast = False
            '        .IsProhibited = True
            '        .Ordinal = 20
            '        .Description = "ifm freeze"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp3") Then .Inject("pdm", "bp3")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsProhibited = True
            '        .Ordinal = 30
            '        .Description = "fap"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp4") Then .Inject("pdm", "bp4")
            '        .ActualOfFC = "bp3"
            '        .IsForecast = False
            '        .IsProhibited = True
            '        .Ordinal = 30
            '        .Description = "fap"
            '        .Persist()

            '    End With

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp7") Then .Inject("pdm", "bp7")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsFacultative = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp8") Then .Inject("pdm", "bp8")
            '        .ActualOfFC = "bp7"
            '        .IsForecast = False
            '        .IsFacultative = True
            '        .Ordinal = 80
            '        .Description = "pdm entry"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp9") Then .Inject("pdm", "bp9")
            '        .ActualOfFC = ""
            '        .IsForecast = True
            '        .IsMandatory = True
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp10") Then .Inject("pdm", "bp10")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 90
            '        .Description = "pdm approval"
            '        .Persist()

            '    End With
            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("pdm", "bp80") Then .Inject("pdm", "bp80")
            '        .ActualOfFC = "bp9"
            '        .IsForecast = False
            '        .IsMandatory = True
            '        .Ordinal = 95
            '        .Description = "pdm first approval"
            '        .Persist()
            '    End With

            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Defintion for 'PDM' is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.primaryTableID)

            '    aScheduleDefM = New ScheduleMilestoneDefinition
            '    With aScheduleDefM
            '        If Not .Create("none", "bp80") Then .Inject("none", "bp80")
            '        .ActualOfFC = ""
            '        .IsForecast = False
            '        .IsFacultative = True
            '        .Ordinal = 95
            '        .Description = "pdm first approval"
            '        .Persist()

            '    End With
            'End If


            'Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Defintion for 'NONE' is up-to-date", _
            '                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.primaryTableID)

            'Dim aScheduleTaskDef As New clsOTDBDefScheduleTask
            'If Not aScheduleTaskDef.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
            '                                 message:="Schema " & aScheduleTaskDef.primaryTableID & " couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Schedule Task Defintion is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleTaskDef.primaryTableID)

            '    aScheduleTaskDef = New clsOTDBDefScheduleTask
            '    With aScheduleTaskDef
            '        If Not .Create("full", "synchro") Then .Inject("full", "synchro")
            '        .Description = "task for synchronization"
            '        .StartID = "bp11"
            '        .ActstartID = "bp12"
            '        .AlternativeStartIDs = New String() {"bp1", "bp3"}
            '        .FinishID = "bp3"
            '        .ActfinishID = "bp4"
            '        .AlternativeFinishIDs = New String() {""}
            '        .takeActualIfFCisMissing = True
            '        .IsFacultative = True
            '        .parameter_txt1 = "bp1"
            '        .Persist()

            '    End With
            '    aScheduleTaskDef = New clsOTDBDefScheduleTask
            '    With aScheduleTaskDef
            '        If Not .Create("full", "development") Then .Inject("full", "development")
            '        .Description = "3D Development"
            '        .StartID = "bp11"
            '        .ActstartID = "bp12"
            '        .AlternativeStartIDs = New String() {"bp1", "bp3"}
            '        .FinishID = "bp7"
            '        .ActfinishID = "bp8"
            '        .AlternativeFinishIDs = New String() {""}
            '        .takeActualIfFCisMissing = True
            '        .IsMandatory = True
            '        .Persist()

            '    End With
            '    aScheduleTaskDef = New clsOTDBDefScheduleTask
            '    With aScheduleTaskDef
            '        If Not .Create("full", "approve") Then .Inject("full", "approve")
            '        .Description = "approval"
            '        .StartID = "bp7"
            '        .ActstartID = "bp8"
            '        .AlternativeStartIDs = New String() {"bp9"}
            '        .FinishID = "bp9"
            '        .ActfinishID = "bp10"
            '        .AlternativeFinishIDs = New String() {""}
            '        .takeActualIfFCisMissing = True
            '        .Persist()

            '    End With
            '    aScheduleTaskDef = New clsOTDBDefScheduleTask
            '    With aScheduleTaskDef
            '        If Not .Create("pdm", "approve") Then .Inject("pdm", "approve")
            '        .Description = "approval"
            '        .StartID = "bp7"
            '        .ActstartID = "bp8"
            '        .AlternativeStartIDs = New String() {"bp9"}
            '        .IsMandatory = True
            '        .FinishID = "bp9"
            '        .ActfinishID = "bp10"
            '        .AlternativeFinishIDs = New String() {""}
            '        .takeActualIfFCisMissing = True
            '        .Persist()

            '    End With

            'End If

            'Dim aDefMilestone As New MileStoneDefinition
            'If Not MileStoneDefinition.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
            '                                 message:="Schema " & aMilestone.primaryTableID & " couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Milestone Definition is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefMilestone.primaryTableID)

            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp11") Then .Inject("bp11")
            '        .Description = "FC start work"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = True
            '        .IsOfDate = True
            '        .Persist()
            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp12") Then .Inject("bp12")
            '        .Description = "start work"
            '        .IsForecast = False
            '        .Datatype = otFieldDataType.[Date]
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp1") Then .Inject("bp1")
            '        .Description = "FC IFM freeze gate"
            '        .IsForecast = True
            '        .Datatype = otFieldDataType.[Date]
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp2") Then .Inject("bp2")
            '        .Description = "IFM freeze gate"
            '        .IsForecast = False
            '        .Datatype = otFieldDataType.[Date]
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp13") Then .Inject("bp13")
            '        .Description = "current IFM freeze status"
            '        .IsForecast = False
            '        .IsOfStatus = True
            '        .Datatype = otFieldDataType.Text
            '        .statustypeid = "ifmstatus"
            '        .referingToID = "bp15"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp15") Then .Inject("bp15")
            '        .Description = "current IFM freeze status date"
            '        .IsForecast = False
            '        .Datatype = otFieldDataType.[Date]
            '        .IsOfDate = True
            '        .referingToID = "bp13"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp3") Then .Inject("bp3")
            '        .Description = "FC FAP / Design Freeze status date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = True
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp4") Then .Inject("bp4")
            '        .Description = "FAP / Design Freeze gate"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp5") Then .Inject("bp5")
            '        .Description = "dmu status"
            '        .Datatype = otFieldDataType.Text
            '        .IsForecast = False
            '        .IsOfStatus = True
            '        .statustypeid = "dmustatus"
            '        .referingToID = "bp6"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp6") Then .Inject("bp6")
            '        .Description = "dmu status date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .referingToID = "bp5"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp20") Then .Inject("bp20")
            '        .Description = "FC FEM result date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp22") Then .Inject("bp22")
            '        .Description = "FEM status date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .referingToID = "bp21"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp21") Then .Inject("bp21")
            '        .Description = "FEM Status"
            '        .Datatype = otFieldDataType.Text
            '        .IsForecast = False
            '        .IsOfStatus = True
            '        .statustypeid = "femstatus"
            '        .referingToID = "bp22"
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp7") Then .Inject("bp7")
            '        .Description = "FC PDM entry date (outgoing ENG)"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = True
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp8") Then .Inject("bp8")
            '        .Description = "entry PDM date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp14") Then .Inject("bp14")
            '        .Description = "outgoing PDM DRL date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp9") Then .Inject("bp9")
            '        .Description = "FC PDM approval date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = True
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp10") Then .Inject("bp10")
            '        .Description = "PDM approval date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    aDefMilestone = New MileStoneDefinition
            '    With aDefMilestone
            '        If Not .Create("bp80") Then .Inject("bp80")
            '        .Description = "first PDM approval date"
            '        .Datatype = otFieldDataType.[Date]
            '        .IsForecast = False
            '        .IsOfDate = True
            '        .Persist()

            '    End With
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Milestone sample definition created", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.primaryTableID)

            'End If


            'Dim aDependCheck As New clsOTDBDependCheck
            'If Not aDependCheck.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="partsdependeny couldn't be created")
            'Else

            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="Dependency Check Object is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDependCheck.PrimaryTableID)

            'End If

            'If Not aDepend.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="dependency object couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="dependency object is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDepend.PrimaryTableID)

            'End If


            'If Not aCluster.createSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="cluster couldn't be created")
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_Schedule", message:="dependency cluster is up-to-date", _
            '                                 messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aCluster.PrimaryTableID)

            'End If



        End Sub

        ''' <summary>
        ''' creates the schema and persist for a list of objects
        ''' </summary>
        ''' <param name="objects"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CreateAndPersist(objects As IEnumerable(Of String), Optional force As Boolean = False) As Boolean
            Dim theObjects As New List(Of ObjectDefinition)
            Dim result As Boolean = True

            For Each anObjectID In objects
                Dim anObjectDefinition = ot.CurrentSession.Objects.GetObject(objectid:=anObjectID, runtimeOnly:=CurrentSession.IsBootstrappingInstallationRequested)
                If anObjectDefinition IsNot Nothing Then
                    theObjects.Add(anObjectDefinition)
                End If
            Next

            '*** create all the schema for the objects
            For Each anobjectdefinition In theObjects
                result = result And anobjectdefinition.CreateObjectSchema(silent:=True)
                If result Then
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", _
                                                           message:="Schema for  Object " & anobjectdefinition.ID & " updated or created to version " & anobjectdefinition.Version & ". Tables created or updated:" & Converter.Enumerable2otString(anobjectdefinition.Tablenames), _
                                                           messagetype:=otCoreMessageType.ApplicationInfo, _
                                                           objectname:=anobjectdefinition.ID, noOtdbAvailable:=True)
                Else
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", showmsgbox:=True, _
                                                             message:="Schema for  Object " & anobjectdefinition.ID & " could not be updated nor created ! - Contact your administrator ", _
                                                             messagetype:=otCoreMessageType.InternalError, _
                                                             noOtdbAvailable:=True, objectname:=anobjectdefinition.ID)
                    Return result
                End If
            Next

            '** persist the objectdefinition
            For Each anobjectdefinition In theObjects
                '** switch off RuntimeMode
                If Not anobjectdefinition.SwitchRuntimeOff() Then
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", showmsgbox:=True, _
                                                           message:="Runtime for  Object " & anobjectdefinition.ID & " could not be switched off ! - Contact your administrator ", _
                                                           messagetype:=otCoreMessageType.InternalError, _
                                                          noOtdbAvailable:=True, objectname:=anobjectdefinition.ID)
                    Return result
                End If
                result = result And anobjectdefinition.Persist()
                If result Then
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", _
                                                           message:="Schema for  Object " & anobjectdefinition.ID & " persisted.", _
                                                           messagetype:=otCoreMessageType.ApplicationInfo, _
                                                           objectname:=anobjectdefinition.ID, noOtdbAvailable:=True)
                Else
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", showmsgbox:=True, _
                                                             message:="Schema for  Object " & anobjectdefinition.ID & " could not be peristed ! - Contact your administrator ", _
                                                             messagetype:=otCoreMessageType.InternalError, _
                                                            noOtdbAvailable:=True, objectname:=anobjectdefinition.ID)
                    Return result
                End If
            Next

            Return result
        End Function
        ''' <summary>
        ''' Creates or updates all the Database Schema for all objects or a subset
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub CreateDatabase(Optional modules As IEnumerable(Of String) = Nothing, Optional force As Boolean = False)

            Dim aNativeConnection = CurrentDBDriver.CurrentConnection.NativeConnection
            Dim repersistnecessary As Boolean = False
            Dim result As Boolean = True
            '** verify database bootstrap in detail to check if bootstrap is needed
            If Not CurrentSession.IsBootstrappingInstallationRequested Then
                CurrentDBDriver.VerifyOnTrackDatabase(install:=False, modules:=Nothing, verifySchema:=True) 'this will not ask to install but check on bootstrapping necessary
            End If
            '** create the db table
            result = result And CurrentDBDriver.CreateDBParameterTable(nativeConnection:=aNativeConnection)

            '*** get the current schema version
            Dim schemaversion = CurrentDBDriver.GetDBParameter(parametername:=ConstPNBSchemaVersion, silent:=True)
            If schemaversion Is Nothing OrElse Not IsNumeric(schemaversion) Then
                Call CoreMessageHandler(message:="No schema version for database available - assuming first time installation", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="Installation.createDatabase")
            ElseIf Convert.ToUInt64(schemaversion) < ot.SchemaVersion Then
                Call CoreMessageHandler(message:="Schema version for database available - assuming upgrade installation", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="Installation.createDatabase", arg1:=schemaversion)
            ElseIf Convert.ToUInt64(schemaversion) > ot.SchemaVersion Then
                Call CoreMessageHandler(message:="Schema version for database available but higher ( " & schemaversion & " ) - downgrading ?!", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="Installation.createDatabase", arg1:=ot.SchemaVersion)
            Else
                Call CoreMessageHandler(message:="Schema version for database available - assuming repair installation", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="Installation.createDatabase", arg1:=schemaversion)
            End If

            '** create the bootstrapping 
            '**
            Dim descriptions = ot.GetBootStrapObjectClassDescriptions
            Dim objectids As New List(Of String)

            For Each description In descriptions
                Dim addflag As Boolean = False

                For Each tablename In description.Tables
                    Dim value = GetDBParameter(ConstPNBSchemaVersion_TableHeader & tablename, silent:=True)
                    If value Is Nothing OrElse Not IsNumeric(value) OrElse Not CurrentDBDriver.HasTable(tablename) Then
                        addflag = True
                    ElseIf Convert.ToUInt64(value) > description.GetSchemaTableAttribute(tablename).Version Then
                        CoreMessageHandler(message:="WARNING ! Version of Bootstrapping Table in database is higher ( " & value & ") than in class description ( " & description.GetSchemaTableAttribute(tablename).Version & "). Downgrading ?!", messagetype:=otCoreMessageType.InternalWarning, _
                                            subname:="Installation.createDatabase", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
                    ElseIf force OrElse Convert.ToUInt64(value) < description.GetSchemaTableAttribute(tablename).Version Then
                        addflag = True
                    End If
                Next

                '** add it
                If addflag Then
                    objectids.Add(description.ID)
                End If
            Next

            '*** create it
            If objectids.Count > 0 Then
                result = result And CreateAndPersist(objectids, force:=force)
                repersistnecessary = True
            Else
                result = result And True
            End If

            '** Create SuperUser
            If Not CurrentSession.CurrentDBDriver.HasAdminUserValidation Then
                result = result And CurrentDBDriver.CreateDBUserDefTable(nativeConnection:=aNativeConnection)
                If result Then
                    Call CoreMessageHandler(message:="Administrator account created ", _
                                            messagetype:=otCoreMessageType.InternalInfo, _
                                            subname:="Installation.createDatabase", break:=False, noOtdbAvailable:=True)

                Else
                    Call CoreMessageHandler(message:="Administrator Account could not be created - Please see your system administrator.", messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="Installation.createDatabase_CoreData", _
                                                break:=False, showmsgbox:=True, noOtdbAvailable:=True)
                    Return
                End If
            End If

            '*** create global domain
            If CurrentDBDriver.CreateGlobalDomain(nativeConnection:=aNativeConnection) Then
                Call CoreMessageHandler(message:="global domain created", arg1:=ConstGlobalDomain, messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="Installation.createDatabase")
            End If

            '*** set objects to load
            Call CurrentDBDriver.SetDBParameter(ConstPNObjectsLoad, _
                                                         ScheduleEdition.ConstObjectID & ", " & _
                                                         ScheduleMilestone.ConstObjectID & ", " & _
                                                         Deliverable.ConstObjectID, silent:=True)
            '*** bootstrap checksum
            CurrentDBDriver.SetDBParameter(ConstPNBootStrapSchemaChecksum, value:=ot.GetBootStrapSchemaChecksum, silent:=True)

            '**** Create the core objects first
            '****
            If modules.Contains(ConstModuleCommons.ToUpper) Then
                descriptions = ot.GetObjectClassDescriptionsForModule(ConstModuleCommons)
                objectids = New List(Of String)

                For Each description In descriptions
                    Dim addflag As Boolean = False

                    For Each tablename In description.Tables
                        Dim value = GetDBParameter(ConstPNBSchemaVersion_TableHeader & tablename, silent:=True)
                        If value Is Nothing OrElse Not IsNumeric(value) OrElse Not CurrentDBDriver.HasTable(tablename) Then
                            addflag = True
                        ElseIf Convert.ToUInt64(value) > description.GetSchemaTableAttribute(tablename).Version Then
                            CoreMessageHandler(message:="WARNING ! Version of Table in database is higher ( " & value & ") than in class description ( " & description.GetSchemaTableAttribute(tablename).Version & "). Downgrading ?!", messagetype:=otCoreMessageType.InternalWarning, _
                                                subname:="Installation.createDatabase", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
                        ElseIf force OrElse Convert.ToUInt64(value) < description.GetSchemaTableAttribute(tablename).Version Then
                            addflag = True
                        End If
                    Next

                    '** add it
                    If (repersistnecessary OrElse addflag) AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(description.ID) Then
                        objectids.Add(description.ID)
                    End If
                Next

                '*** create it
                If objectids.Count > 0 Then
                    result = result And CreateAndPersist(objectids, force:=force)
                Else
                    result = result And True
                End If
            End If

            '**** Create the other modules
            '****
            For Each modulename In modules
                If modulename <> ConstModuleCommons Then
                    descriptions = ot.GetObjectClassDescriptionsForModule(modulename)
                    objectids = New List(Of String)

                    For Each description In descriptions
                        Dim addflag As Boolean = False

                        For Each tablename In description.Tables
                            Dim value = GetDBParameter(ConstPNBSchemaVersion_TableHeader & tablename, silent:=True)
                            If value Is Nothing OrElse Not IsNumeric(value) OrElse Not CurrentDBDriver.HasTable(tablename) Then
                                addflag = True
                            ElseIf Convert.ToUInt64(value) > description.GetSchemaTableAttribute(tablename).Version Then
                                CoreMessageHandler(message:="WARNING ! Version of Table in database is higher ( " & value & ") than in class description ( " & description.GetSchemaTableAttribute(tablename).Version & "). Downgrading ?!", messagetype:=otCoreMessageType.InternalWarning, _
                                                    subname:="Installation.createDatabase", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
                            ElseIf force OrElse Convert.ToUInt64(value) < description.GetSchemaTableAttribute(tablename).Version Then
                                addflag = True
                            End If
                        Next

                        '** add it
                        If (repersistnecessary OrElse addflag) AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(description.ID) Then
                            objectids.Add(description.ID)
                        End If

                    Next

                    '*** create it
                    If objectids.Count > 0 Then
                        result = result And CreateAndPersist(objectids, force:=force)
                    Else
                        result = result And True
                    End If
                End If
            Next

            '*** create all foreign keys
            '***

            For Each aTable In CurrentSession.Objects.TableDefinitions
                If aTable.AlterSchemaForeignRelations() Then
                    Call ot.CoreMessageHandler(subname:="Installation.createDatabase", _
                                                      message:="foreign keys created for table " & aTable.Name, _
                                                      tablename:=aTable.Name, _
                                                      messagetype:=otCoreMessageType.ApplicationInfo)
                Else
                    Call ot.CoreMessageHandler(subname:="Installation.createDatabase", _
                                                     message:="Error while creating foreign keys for table " & aTable.Name, _
                                                     tablename:=aTable.Name, _
                                                     messagetype:=otCoreMessageType.InternalError)
                End If
            Next

            '*** set the current schema version
            CurrentDBDriver.SetDBParameter(parametername:=ConstPNBSchemaVersion, value:=ot.SchemaVersion, silent:=True)
            Dim aSchemaChange As OnTrackChangeLogEntry = New OnTrackChangeLogEntry(application:=ConstApplicationBackend, [module]:=ConstPNBSchemaVersion, _
                                                                                   version:=ot.SchemaVersion, release:=0, patch:=0, changeimplno:=0, description:="installed schema")

            ot.OnTrackChangeLog.Add(aSchemaChange)

            '*** request end of bootstrap
            '***
            If Not CurrentSession.RequestEndofBootstrap() Then
                Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                       message:="failed to create tables for object repository - abort the installation", _
                                                       messagetype:=otCoreMessageType.InternalError)
                Return
            End If

            '*** start a session
            Dim sessionrunning As Boolean = CurrentSession.IsRunning
            Dim sessionstarted As Boolean = False
            Dim sessionaborted As Boolean = False

            '** if not global domain shutdown
            If (sessionrunning AndAlso ot.CurrentSession.CurrentDomainID <> ConstGlobalDomain) Then
                Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                       message:="shutting down current session since it is not in the global domain", _
                                                       messagetype:=otCoreMessageType.InternalInfo)
                CurrentSession.ShutDown(force:=True)
                sessionrunning = False
            End If
            '** no session runnnig -> startup
            If Not sessionrunning Then
                ''' if we have to abort the starting up
                If CurrentSession.IsStartingUp Then sessionaborted = CurrentSession.RequestToAbortStartingUp()
                sessionstarted = CurrentSession.StartUp(otAccessRight.AlterSchema, domainID:=ConstGlobalDomain, messagetext:="Please start up a Session to setup initial data")

            End If

            '***
            '*** Initialize Data
            If sessionrunning OrElse sessionstarted Then

                ''' Change Log Data
                ''' 
                If Not SaveChangeLog() Then
                    Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                          message:="failed to write change log data", _
                                                          messagetype:=otCoreMessageType.InternalError)
                    Return
                Else
                    ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase", _
                                                          message:="change log data persisted", _
                                                          messagetype:=otCoreMessageType.InternalInfo)
                End If

                ''' Core Data
                ''' 
                If Not InitialCoreData() Then
                    Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                          message:="failed to write initial core data - core might not be working correctly", _
                                                          messagetype:=otCoreMessageType.InternalError)
                    Return
                Else
                    ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase", _
                                                          message:="core objects with data instanced and persisted", _
                                                          messagetype:=otCoreMessageType.InternalInfo)
                End If

                ''' Initialize calendar
                ''' 
                Dim fromDate As Date = CDate(My.MySettings.Default.InitializeCalendarFrom)
                Dim ToDate As Date = CDate(My.MySettings.Default.InitializeCalendarTo)
                Dim valueFrom As Object = CurrentDBDriver.GetDBParameter(ConstPNCalendarInitializedFrom, silent:=True)
                Dim valueTo As Object = CurrentDBDriver.GetDBParameter(ConstPNCalendarInitializedto, silent:=True)

                ''' check if already initialized
                ''' 
                If valueFrom Is Nothing OrElse valueTo Is Nothing _
                    OrElse (IsDate(valueFrom) AndAlso CDate(valueFrom) <> fromDate) OrElse (IsDate(valueTo) AndAlso CDate(valueTo) <> ToDate) Then
                    ''' initialize if date is not there
                    If Not InitializeCalendar(fromDate:=fromDate, toDate:=ToDate) Then
                        Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                                  message:="failed to write initial calendar data - calendar might not be working correctly", _
                                                                  messagetype:=otCoreMessageType.InternalError)
                    Else
                        ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase", _
                                                             message:="calendar from " & fromDate & " until " & ToDate & " instanced and persisted", _
                                                             messagetype:=otCoreMessageType.InternalInfo)
                        CurrentDBDriver.SetDBParameter(ConstPNCalendarInitializedFrom, Format(fromDate, "yyyy-MM-dd"))
                        CurrentDBDriver.SetDBParameter(ConstPNCalendarInitializedto, Format(ToDate, "yyyy-MM-dd"))
                    End If
                End If


                ''' import the initial data
                ''' 
                Dim valueInitialPath As String = My.MySettings.Default.InitialCoreDirectory
                Dim searchpath As String = ""
                If valueInitialPath <> "" AndAlso Not IO.Directory.Exists(valueInitialPath) Then
                    searchpath = My.Application.Info.DirectoryPath & "\Resources\" & valueInitialPath
                ElseIf valueInitialPath = "" Then
                    searchpath = My.Application.Info.DirectoryPath & "\Resources"
                End If

                If IO.Directory.Exists(searchpath) Then
                    ot.CoreMessageHandler(message:="importing initial data ...", arg1:=searchpath, subname:="Installation.createDatabase", messagetype:=otCoreMessageType.InternalInfo)
                    FeedInInitialData(searchpath)
                Else
                    Dim uri As System.Uri
                    uri = New System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
                    searchpath = System.IO.Path.GetDirectoryName(uri.LocalPath) & "\Resources\" & valueInitialPath
                    If IO.Directory.Exists(searchpath) Then
                        ot.CoreMessageHandler(message:="importing initial data ...", arg1:=searchpath, subname:="Installation.createDatabase", messagetype:=otCoreMessageType.InternalInfo)
                        FeedInInitialData(searchpath)
                    End If

                End If

            End If

            ''' 
            '''shutdown a session
            If CurrentSession.IsRunning AndAlso sessionstarted Then
                CurrentSession.ShutDown(force:=True)
            End If
            If sessionaborted Then
                Call ot.CoreMessageHandler(showmsgbox:=True, subname:="Installation.createDatabase", _
                                                             message:="The session which triggered the install routines was aborted during setup. Please reconnect again !", _
                                                             messagetype:=otCoreMessageType.InternalInfo)
            End If
        End Sub

        ''' <summary>
        ''' Drop Database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DropDatabase() As Boolean
            If CurrentSession.RequestUserAccess(otAccessRight.AlterSchema, messagetext:="for dropping the database please provide a administration id") Then
                With New UI.CoreMessageBox
                    '* Message Heaxder
                    .Title = "CAUTION - PLEASE CONFIRM CRITICAL OPERATION"
                    .type = UI.CoreMessageBox.MessageType.Warning
                   
                    '* Message
                    .Message = "Please confirm that you really want to drop the database and therefore ALL DATA WILL BE LOST !" & vbLf & _
                        " Make sure you have a database backup at hand."
                    .buttons = UI.CoreMessageBox.ButtonType.YesNo
                    .Show()
                    If .result <> UI.CoreMessageBox.ResultType.Yes OrElse .result <> UI.CoreMessageBox.ResultType.Ok Then
                        Return False
                    End If
                End With

                Dim aDropSqlStatement As String = OnTrack.Database.Constants.DropAllTables
                If CurrentDBDriver.RunSqlStatement(aDropSqlStatement) Then
                    CurrentSession.ShutDown(force:=True)
                    Return True
                End If
            End If

        End Function

        ''' <summary>
        ''' Initialize Test Data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InitializeTestData() As Boolean

            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right could not be set to AlterSchema", subname:="Installation.InitializeTestData", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Return False
            End If

            'Dim aSet As ObjectProperties.ObjectPropertySet = ObjectProperties.ObjectPropertySet.Create(id:="FBL_SBB")
            'If aSet IsNot Nothing Then
            '    aSet.Description = "test"
            '    aSet.Ordinal = 1
            '    aSet.AttachedObjectIDs = {Deliverables.Deliverable.ConstObjectID}.ToList
            '    aSet.Persist()
            'End If


            'Dim aproperty As ObjectProperties.ObjectProperty = ObjectProperties.ObjectProperty.Create(setid:="FBL_SBB", ID:="BLTEST")
            'If aproperty IsNot Nothing Then
            '    aproperty.Datatype = otDataType.Text
            '    aproperty.Title = "BaseLine Test"
            '    aproperty.Ordinal = 1
            '    aproperty.Persist()
            'End If




            'Dim aMilestoneDef As MileStoneDefinition = MileStoneDefinition.Create(ID:="BP9")
            'If aMilestoneDef Is Nothing Then aMilestoneDef = MileStoneDefinition.Retrieve(id:="Bp9")

            'If aMilestoneDef IsNot Nothing Then
            '    aMilestoneDef.AttachedObjectids = {Deliverable.ConstObjectID}.ToList
            '    aMilestoneDef.Description = "fc finish"
            '    aMilestoneDef.IsForecast = True
            '    aMilestoneDef.Persist()
            'End If

            'aMilestoneDef = MileStoneDefinition.Create(ID:="BP10")
            'If aMilestoneDef Is Nothing Then aMilestoneDef = MileStoneDefinition.Retrieve(id:="BP10")
            'If aMilestoneDef IsNot Nothing Then
            '    aMilestoneDef.AttachedObjectids = {Deliverable.ConstObjectID}.ToList
            '    aMilestoneDef.Description = "actual finish"
            '    aMilestoneDef.IsForecast = False
            '    aMilestoneDef.Persist()
            'End If

            'Dim aScheduleDefinition As ScheduleDefinition = ScheduleDefinition.Create(id:="PDM")
            'If aScheduleDefinition Is Nothing Then aScheduleDefinition = ScheduleDefinition.Retrieve("PDM")
            'If aScheduleDefinition IsNot Nothing Then
            '    aScheduleDefinition.Description = "simple pdm entry schedule"
            '    aScheduleDefinition.Autopublish = True

            '    Dim aScheduleMilestone As ScheduleMilestoneDefinition = ScheduleMilestoneDefinition.Create(scheduletype:="PDM", ID:="BP9")
            '    If aScheduleMilestone Is Nothing Then aScheduleMilestone = ScheduleMilestoneDefinition.Retrieve(scheduletype:="PDM", ID:="Bp9")
            '    If aScheduleMilestone IsNot Nothing Then
            '        aScheduleMilestone.IsMandatory = True
            '        aScheduleMilestone.IsOutputDeliverable = True
            '        aScheduleMilestone.IsForecast = True
            '        aScheduleMilestone.IsFinish = True
            '    End If

            '    aScheduleMilestone = ScheduleMilestoneDefinition.Create(scheduletype:="PDM", ID:="BP10")
            '    If aScheduleMilestone Is Nothing Then aScheduleMilestone = ScheduleMilestoneDefinition.Retrieve(scheduletype:="PDM", ID:="Bp10")
            '    If aScheduleMilestone IsNot Nothing Then
            '        aScheduleMilestone.IsMandatory = True
            '        aScheduleMilestone.IsOutputDeliverable = True
            '        aScheduleMilestone.IsForecast = False
            '        aScheduleMilestone.ActualOfFC = "BP9"
            '    End If

            '    aScheduleDefinition.Persist()
            'End If

            'Dim aDeliverable As Deliverable ' = Deliverable.Create(Datatype:="FVDS")
            'If aDeliverable Is Nothing Then aDeliverable = Deliverable.Retrieve(uid:=295)

            'aDeliverable.Description = "TEST"
            'Debug.WriteLine(aDeliverable.GetValue("BLTEST"))
            'aDeliverable.SetValue("BLTEST", "test8")
            'Debug.WriteLine(aDeliverable.GetValue("BP9"))
            'aDeliverable.SetValue("BP9", #10/2/2014#)
            'aDeliverable.Persist()

            'Dim aTargetEntry As ObjectColumnEntry = CurrentSession.Objects.GetObject(Target.ConstObjectID).GetEntry(Target.constFNTarget)
            'If aTargetEntry IsNot Nothing Then
            '    '''
            '    ''' Deliverable
            '    ''' 
            '    Dim aCompound As ObjectCompoundEntry = ObjectCompoundEntry.Create(objectname:=Deliverable.ConstObjectID, entryname:=aTargetEntry.XID)
            '    If aCompound Is Nothing Then aCompound = ObjectCompoundEntry.Retrieve(objectname:=Deliverable.ConstObjectID, entryname:=aTargetEntry.XID)
            '    If aCompound IsNot Nothing Then
            '        aCompound.CompoundRelationPath = {Deliverable.ConstObjectID & "." & Deliverable.ConstRWorkspaceTarget, _
            '                                          WorkspaceTarget.ConstObjectID & "." & WorkspaceTarget.ConstRWorkTarget, _
            '                                          Target.ConstObjectID}
            '        aCompound.Datatype = aTargetEntry.Datatype
            '        ' ordinal calculate an ordinal
            '        aCompound.Ordinal = 200000 + aTargetEntry.Ordinal
            '        aCompound.Title = aTargetEntry.Title
            '        aCompound.Description = aTargetEntry.Description
            '        aCompound.XID = aTargetEntry.XID

            '        ''' special compound settings
            '        aCompound.CompoundObjectID = aTargetEntry.ConstObjectID
            '        aCompound.CompoundValueEntryName = Nothing
            '        aCompound.CompoundIDEntryname = Target.constFNTarget
            '        aCompound.CompoundGetterMethodName = Nothing
            '        aCompound.CompoundSetterMethodName = Nothing

            '        aCompound.Persist()
            '    End If

            ''' 
            ''' WorkspaceTarget -> done now via compoundentry.csv
            ''' 
            'aCompound = ObjectCompoundEntry.Create(objectname:=WorkspaceTarget.ConstObjectID, entryname:=aTargetEntry.XID)
            'If aCompound Is Nothing Then aCompound = ObjectCompoundEntry.Retrieve(objectname:=WorkspaceTarget.ConstObjectID, entryname:=aTargetEntry.XID)
            'If aCompound IsNot Nothing Then
            '    aCompound.CompoundRelationPath = {WorkspaceTarget.ConstObjectID & "." & WorkspaceTarget.ConstRWorkTarget, _
            '                                      Target.ConstObjectID}
            '    aCompound.Datatype = aTargetEntry.Datatype
            '    ' ordinal calculate an ordinal
            '    aCompound.Ordinal = 200000 + aTargetEntry.Ordinal
            '    aCompound.Title = aTargetEntry.Title
            '    aCompound.Description = aTargetEntry.Description
            '    aCompound.XID = aTargetEntry.XID

            '    ''' special compound settings
            '    aCompound.CompoundObjectID = aTargetEntry.ConstObjectID
            '    aCompound.CompoundValueEntryName = Nothing
            '    aCompound.CompoundIDEntryname = Target.constFNTarget
            '    aCompound.CompoundGetterMethodName = WorkspaceTarget.ConstOPGetTarget
            '    aCompound.CompoundSetterMethodName = WorkspaceTarget.ConstOPSetTarget

            '    aCompound.Persist()
            'End If
            'End If

            Return True
        End Function
        ''' <summary>
        ''' Initialize the Calendar
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitializeCalendar(fromDate As Date, toDate As Date) As Boolean

            ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase", _
                                                     message:="creating calendar from " & fromDate & " until " & toDate & " - please stand by ...", _
                                                     messagetype:=otCoreMessageType.ApplicationInfo)
            ''' generate the days
            CalendarEntry.GenerateDays(fromdate:=fromDate, untildate:=toDate, name:=ot.CurrentSession.DefaultCalendarName)

            Dim acalentry As CalendarEntry
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    ' additional
                    .Datevalue = CDate("29.03.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If

            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("01.04.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("09.05.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If

            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("10.05.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Christi Himmelfahrt Brückentag"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.05.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Reformationstag (Sachsen)"
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.11.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Buß- und Bettag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("18.04.2014")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("01.04.2014")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("29.05.2013")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.05.2014")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2014")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Reformationstag (Sachsen)"
                    .Persist()
                End With
            End If
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("19.11.2014")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Buß- und Bettag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("03.04.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("06.04.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("14.05.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("25.05.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Reformationstag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("18.11.2015")
                    .Type = otCalendarEntryType.DayEntry
                    .IsNotAvailable = True
                    .Description = "Buß- und Bettag (Sachsen)"
                    .Persist()

                End With
            End If

            Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_CoreData", tablename:=acalentry.PrimaryTableID, _
                                         message:="Calendar until 31.12.2016 created", messagetype:=otCoreMessageType.ApplicationInfo)

            Return True
        End Function
        ''' <summary>
        ''' save the ontrack change log to the database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SaveChangeLog() As Boolean

            ''' save all change log entries to the database
            ''' 
            For Each anEntry In ot.OnTrackChangeLog
                If Not anEntry.IsAlive(throwError:=False) Then
                    anEntry.Create() 'bring to alive
                End If
                If anEntry.RunTimeOnly Then anEntry.SwitchRuntimeOff() ' switch runtime off to make persistable
                anEntry.Persist()
            Next

            Return True
        End Function
        ''' <summary>
        ''' feeds the initial data from a path
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FeedInInitialData(path As String) As Boolean

            If Not IO.Directory.Exists(path) Then
                CoreMessageHandler(message:="path does not exist in filesystem", arg1:=path, subname:="createDatabase.FeedInitialData")
            Else

                CoreMessageHandler(message:="checking directory '" & path & "'", _
                                                  arg1:=path, username:=CurrentSession.Username, _
                                                  subname:="CreateDatabase.FeedInitialData", messagetype:=otCoreMessageType.InternalInfo)
            End If

            ''' try to feed in each File in the filepath
            For Each anEntry In IO.Directory.EnumerateFileSystemEntries(path)
                If IO.Directory.Exists(anEntry) Then
                    FeedInInitialData(anEntry)
                Else
                    ''' feed in the csv file if it is one
                    ''' 
                    If IO.Path.GetExtension(anEntry).ToUpper = ".CSV" Then
                        If CSVXChangeManager.FeedInCSV(anEntry) Then
                            CoreMessageHandler(message:="csv file '" & IO.Path.GetFileName(anEntry) & "' imported", _
                                               arg1:=path, username:=CurrentSession.Username, _
                                               subname:="CreateDatabase.FeedInitialData", messagetype:=otCoreMessageType.InternalInfo)
                        End If
                    End If
                End If

            Next

            Return True
        End Function
        ''' <summary>
        '''  Initial Core Data
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitialCoreData() As Boolean

            '**** default domain settings
            Dim aDomain = Domain.Retrieve(id:=ConstGlobalDomain)
            If aDomain IsNot Nothing Then
                '*** read the Domain Settings
                '***
                aDomain.SetSetting(id:=Session.ConstCPDependencySynchroMinOverlap, datatype:=otDataType.Long, value:=7)
                aDomain.SetSetting(id:=Session.ConstCPDefaultWorkspace, datatype:=otDataType.Text, value:="@")
                aDomain.SetSetting(id:=Session.ConstCPDefaultCalendarName, datatype:=otDataType.Text, value:="default")
                aDomain.SetSetting(id:=Session.ConstCPDefaultTodayLatency, datatype:=otDataType.Long, value:=-14)
                aDomain.SetSetting(id:=Session.ConstCDefaultScheduleTypeID, datatype:=otDataType.Text, value:="none")
                aDomain.SetSetting(id:=Session.ConstCPDefaultDeliverableTypeID, datatype:=otDataType.Text, value:="")
                aDomain.Persist()
            End If

            '*** Project Base workspaceID
            Dim aWorkspace = Workspace.Create("@")
            If aWorkspace IsNot Nothing Then
                aWorkspace.Description = "base workspaceID"
                aWorkspace.IsBasespace = True
                aWorkspace.FCRelyingOn = New String() {"@"}
                aWorkspace.ACTRelyingOn = New String() {"@"}
                aWorkspace.AccesslistIDs = New String() {}
                aWorkspace.HasActuals = True
                aWorkspace.MinScheduleUPDC = 1
                aWorkspace.MaxScheduleUPDC = 999
                aWorkspace.MinTargetUPDC = 1
                aWorkspace.MaxTargetUPDC = 999
                aWorkspace.Persist()

                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.InitialCoreData", _
                                             message:="base workspaceID @ created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aWorkspace.PrimaryTableID)
            End If
            '*** workspaceID
            'aWorkspace = Workspace.Create("PSIM01")
            'If aWorkspace IsNot Nothing Then
            '    aWorkspace.Description = "Project Simulation workspaceID"
            '    aWorkspace.IsBasespace = False
            '    aWorkspace.FCRelyingOn = New String() {"@", "PSIM01"}
            '    aWorkspace.ACTRelyingOn = New String() {"@"}
            '    aWorkspace.HasActuals = False
            '    aWorkspace.AccesslistIDs = New String() {"PrjPlanner"}
            '    aWorkspace.Min_schedule_updc = 1000
            '    aWorkspace.Max_schedule_updc = 1099
            '    aWorkspace.Min_target_updc = 1000
            '    aWorkspace.Max_target_updc = 1099
            '    aWorkspace.Persist()
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.InitialCoreData", _
            '                                 message:="workspaceID PSIM01 created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aWorkspace.TableID)

            'End If

            '*** Create Group
            Dim aGroup As Group = Group.Create(groupname:="admin")
            If aGroup IsNot Nothing Then
                aGroup.Description = "Administratio group"
                aGroup.HasAlterSchemaRights = True
                aGroup.HasReadRights = True
                aGroup.HasUpdateRights = True
                aGroup.HasNoRights = False
                If aGroup.Persist() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.InitialCoreData", objectname:=Group.ConstObjectID, _
                                                message:="Group Admin created", messagetype:=otCoreMessageType.ApplicationInfo)
                End If

            End If
            '*** Create Group
            aGroup = Group.Create(groupname:="readers")
            If aGroup IsNot Nothing Then
                aGroup.Description = "anonymous group"
                aGroup.HasAlterSchemaRights = False
                aGroup.HasReadRights = True
                aGroup.HasUpdateRights = False
                aGroup.HasNoRights = False
                If aGroup.Persist() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.InitialCoreData", objectname:=Group.ConstObjectID, _
                                                message:="Group Readers created", messagetype:=otCoreMessageType.ApplicationInfo)
                End If

            End If
            '*** Create Default Users
            '***
            Dim anUser As User = User.Create(username:="admin")
            If anUser Is Nothing Then anUser = User.Retrieve(username:="admin")
            If anUser IsNot Nothing Then
                anUser.Description = "Administrator"
                anUser.DefaultWorkspaceID = "@"
                anUser.DefaultDomainID = ConstGlobalDomain
                anUser.GroupNames = {"admin"}
                anUser.Password = "axs2ontrack"
                anUser.HasAlterSchemaRights = True
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = True
                anUser.IsAnonymous = False
                anUser.Persist()
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_CoreData", tablename:=anUser.PrimaryTableID, _
                                             message:="User Admin created", messagetype:=otCoreMessageType.ApplicationInfo)
            End If
            anUser = User.Create(username:="boschnei")
            If anUser IsNot Nothing Then
                anUser.Description = "Boris Schneider"
                anUser.GroupNames = {"admin"}
                anUser.DefaultWorkspaceID = "@"
                anUser.DefaultDomainID = ConstGlobalDomain
                anUser.Password = "zulu4Hart"
                anUser.HasAlterSchemaRights = True
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = True
                anUser.IsAnonymous = False
                anUser.PersonName = "Boris Schneider"
                anUser.Persist()
            End If
            anUser = User.Create(username:="anonymous")
            If anUser IsNot Nothing Then
                anUser.Description = "anonymous"
                anUser.GroupNames = {"readers"}
                anUser.DefaultWorkspaceID = "@"
                anUser.DefaultDomainID = ConstGlobalDomain
                anUser.Password = Nothing
                anUser.HasAlterSchemaRights = False
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = False
                anUser.IsAnonymous = True
                anUser.PersonName = Nothing
                anUser.Persist()
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="Installation.createDatabase_CoreData", tablename:=anUser.PrimaryTableID, _
                                             message:="User anonymous for read created", messagetype:=otCoreMessageType.ApplicationInfo)
            End If


            Return True
        End Function
    End Module
End Namespace