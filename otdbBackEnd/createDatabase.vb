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

Namespace OnTrack.Database

    Public Module createDatabase
        ''' <summary>
        ''' creates or updates all schematas for scheduling objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Scheduling()

            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_Schedule", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If


            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aCurrTarget As New CurrentTarget

            'Dim aMSPivot As New clsOTDBMilestonePivot
            'Dim aPivotMSP As New clsOTDBPivotMSPSchedule
            Dim aDepend As New clsOTDBDependMember
            Dim aCluster As New clsOTDBCluster




            Dim aSchedule As New Schedule
            If Not Schedule.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", _
                                             message:="Schema Schedle couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aSchedule.TableID)

            End If
            ' Create the CurrSchedule
            If Not CurrentSchedule.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema CurrSchedule couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="CurrentSchedule is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aCurrSCHEDULE.TableID)


            End If
            If Not CurrentTarget.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="currTarget couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Current Target is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aCurrTarget.TableID)


            End If


            Dim aMilestone As New ScheduleMilestone
            If Not ScheduleMilestone.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema aMQF couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Milestone is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aMilestone.TableID)


            End If

            Dim aScheduleDef As New ScheduleDefinition
            If Not ScheduleDefinition.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema " & aScheduleDef.TableID & " couldn't be created")
            Else

                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Defintion is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.TableID)


                aScheduleDef = New ScheduleDefinition
                With aScheduleDef
                    If Not .Create("full") Then .Inject("full")
                    .description = "full engineering cycle (3D Design)"
                    .Persist()

                End With
                aScheduleDef = New ScheduleDefinition
                With aScheduleDef
                    If Not .Create("pdm") Then .Inject("pdm")
                    .description = "pdm entry cycle for non 3D Design items"
                    .Persist()

                End With
                aScheduleDef = New ScheduleDefinition
                With aScheduleDef
                    If Not .Create("none") Then
                        .description = "no schedule"
                        .Persist()
                    End If
                End With
                aScheduleDef = New ScheduleDefinition
                With aScheduleDef
                    If Not .Create("nocad") Then .Inject("nocad")
                    .description = "design for non-mechanical (3D) design"
                    .Persist()

                End With
            End If


            Dim aScheduleDefM As New ScheduleMilestoneDefinition
            If Not ScheduleMilestoneDefinition.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema " & aScheduleDefM.TableID & " couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Milestone Defintion is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDefM.TableID)

                '****
                '**** full
                '****

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp11") Then .Inject("full", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp12") Then .Inject("full", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp1") Then .Inject("full", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp2") Then .Inject("full", "bp2")
                    .ActualOfFC = "bp1"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp13") Then .Inject("full", "bp13")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 20
                    .Description = "ifm status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp3") Then .Inject("full", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp4") Then .Inject("full", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp5") Then .Inject("full", "bp5")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 35
                    .Description = "dmu status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp6") Then .Inject("full", "bp6")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 35
                    .Description = "dmu date"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp20") Then .Inject("full", "bp20")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 40
                    .Description = "fc fem"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp21") Then .Inject("full", "bp21")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 40
                    .Description = "fem status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp22") Then .Inject("full", "bp22")
                    .ActualOfFC = "bp20"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 40
                    .Description = "fem status date"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp7") Then .Inject("full", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp8") Then .Inject("full", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM

                    If Not .Create("full", "bp9") Then .Inject("full", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp10") Then .Inject("full", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp80") Then .Inject("full", "bp80")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 95
                    .Description = "pdm first approval"
                    .Persist()
                End With
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Defintion for 'FULL' is updated", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.TableID)

                '****
                '**** nocad
                '****

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp11") Then .Inject("nocad", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = False
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp12") Then .Inject("nocad", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsFacultative = False
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp1") Then .Inject("nocad", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp2") Then .Inject("nocad", "bp2")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp3") Then .Inject("nocad", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 30
                    .Description = "design freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp4") Then .Inject("nocad", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 30
                    .Description = "design freeze"
                    .Persist()

                End With

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp7") Then .Inject("nocad", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp8") Then .Inject("nocad", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp9") Then .Inject("nocad", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = False
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp10") Then .Inject("nocad", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp80") Then .Inject("nocad", "bp80")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 95
                    .Description = "pdm first approval"
                    .Persist()

                End With

                '****
                '**** pdm
                '****

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp11") Then .Inject("pdm", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp12") Then .Inject("pdm", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp1") Then .Inject("pdm", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp2") Then .Inject("pdm", "bp2")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp3") Then .Inject("pdm", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp4") Then .Inject("pdm", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp7") Then .Inject("pdm", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp8") Then .Inject("pdm", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp9") Then .Inject("pdm", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp10") Then .Inject("pdm", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp80") Then .Inject("pdm", "bp80")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 95
                    .Description = "pdm first approval"
                    .Persist()
                End With

                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Defintion for 'PDM' is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.TableID)

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("none", "bp80") Then .Inject("none", "bp80")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 95
                    .Description = "pdm first approval"
                    .Persist()

                End With
            End If


            Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Defintion for 'NONE' is up-to-date", _
                                         messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.TableID)

            Dim aScheduleTaskDef As New clsOTDBDefScheduleTask
            If Not aScheduleTaskDef.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema " & aScheduleTaskDef.TableID & " couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Task Defintion is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleTaskDef.TableID)

                aScheduleTaskDef = New clsOTDBDefScheduleTask
                With aScheduleTaskDef
                    If Not .Create("full", "synchro") Then .Inject("full", "synchro")
                    .Description = "task for synchronization"
                    .StartID = "bp11"
                    .ActstartID = "bp12"
                    .AlternativeStartIDs = New String() {"bp1", "bp3"}
                    .FinishID = "bp3"
                    .ActfinishID = "bp4"
                    .AlternativeFinishIDs = New String() {""}
                    .takeActualIfFCisMissing = True
                    .IsFacultative = True
                    .parameter_txt1 = "bp1"
                    .Persist()

                End With
                aScheduleTaskDef = New clsOTDBDefScheduleTask
                With aScheduleTaskDef
                    If Not .Create("full", "development") Then .Inject("full", "development")
                    .Description = "3D Development"
                    .StartID = "bp11"
                    .ActstartID = "bp12"
                    .AlternativeStartIDs = New String() {"bp1", "bp3"}
                    .FinishID = "bp7"
                    .ActfinishID = "bp8"
                    .AlternativeFinishIDs = New String() {""}
                    .takeActualIfFCisMissing = True
                    .IsMandatory = True
                    .Persist()

                End With
                aScheduleTaskDef = New clsOTDBDefScheduleTask
                With aScheduleTaskDef
                    If Not .Create("full", "approve") Then .Inject("full", "approve")
                    .Description = "approval"
                    .StartID = "bp7"
                    .ActstartID = "bp8"
                    .AlternativeStartIDs = New String() {"bp9"}
                    .FinishID = "bp9"
                    .ActfinishID = "bp10"
                    .AlternativeFinishIDs = New String() {""}
                    .takeActualIfFCisMissing = True
                    .Persist()

                End With
                aScheduleTaskDef = New clsOTDBDefScheduleTask
                With aScheduleTaskDef
                    If Not .Create("pdm", "approve") Then .Inject("pdm", "approve")
                    .Description = "approval"
                    .StartID = "bp7"
                    .ActstartID = "bp8"
                    .AlternativeStartIDs = New String() {"bp9"}
                    .IsMandatory = True
                    .FinishID = "bp9"
                    .ActfinishID = "bp10"
                    .AlternativeFinishIDs = New String() {""}
                    .takeActualIfFCisMissing = True
                    .Persist()

                End With

            End If

            Dim aDefMilestone As New MileStoneDefinition
            If Not MileStoneDefinition.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema " & aMilestone.TableID & " couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Milestone Definition is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefMilestone.TableID)

                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp11") Then .Inject("bp11")
                    .Description = "FC start work"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()
                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp12") Then .Inject("bp12")
                    .Description = "start work"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp1") Then .Inject("bp1")
                    .Description = "FC IFM freeze gate"
                    .IsForecast = True
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp2") Then .Inject("bp2")
                    .Description = "IFM freeze gate"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp13") Then .Inject("bp13")
                    .Description = "current IFM freeze status"
                    .IsForecast = False
                    .IsOfStatus = True
                    .Datatype = otFieldDataType.Text
                    .statustypeid = "ifmstatus"
                    .referingToID = "bp15"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp15") Then .Inject("bp15")
                    .Description = "current IFM freeze status date"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .referingToID = "bp13"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp3") Then .Inject("bp3")
                    .Description = "FC FAP / Design Freeze status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp4") Then .Inject("bp4")
                    .Description = "FAP / Design Freeze gate"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp5") Then .Inject("bp5")
                    .Description = "dmu status"
                    .Datatype = otFieldDataType.Text
                    .IsForecast = False
                    .IsOfStatus = True
                    .statustypeid = "dmustatus"
                    .referingToID = "bp6"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp6") Then .Inject("bp6")
                    .Description = "dmu status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .referingToID = "bp5"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp20") Then .Inject("bp20")
                    .Description = "FC FEM result date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp22") Then .Inject("bp22")
                    .Description = "FEM status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .referingToID = "bp21"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp21") Then .Inject("bp21")
                    .Description = "FEM Status"
                    .Datatype = otFieldDataType.Text
                    .IsForecast = False
                    .IsOfStatus = True
                    .statustypeid = "femstatus"
                    .referingToID = "bp22"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp7") Then .Inject("bp7")
                    .Description = "FC PDM entry date (outgoing ENG)"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp8") Then .Inject("bp8")
                    .Description = "entry PDM date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp14") Then .Inject("bp14")
                    .Description = "outgoing PDM DRL date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp9") Then .Inject("bp9")
                    .Description = "FC PDM approval date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp10") Then .Inject("bp10")
                    .Description = "PDM approval date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp80") Then .Inject("bp80")
                    .Description = "first PDM approval date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Milestone sample definition created", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aScheduleDef.TableID)

            End If

            If Not ScheduleLink.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema " & ScheduleLink.ConstTableID & " couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Schedule Link Definition is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=ScheduleLink.ConstTableID)
            End If


            Dim aDependCheck As New clsOTDBDependCheck
            If Not aDependCheck.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="partsdependeny couldn't be created")
            Else

                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="Dependency Check Object is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDependCheck.TableID)

            End If

            If Not aDepend.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="dependency object couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="dependency object is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDepend.TableID)

            End If


            If Not aCluster.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="cluster couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_Schedule", message:="dependency cluster is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aCluster.TableID)

            End If



        End Sub
        ''' <summary>
        ''' creates or updates all schemata for Configurable object
        ''' </summary>
        ''' <remarks></remarks>
        'Public Sub Configurables()

        '    If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
        '        Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_CONFIG", _
        '                                     messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
        '        Exit Sub
        '    End If


        '    Dim aDefConfigItem As New clsOTDBDefConfigurationItem
        '    If Not clsOTDBDefConfigurationItem.CreateSchema() Then
        '        Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
        '                                     message:="Schema  couldn't be created")

        '    End If

        '    Dim aDefConfiguration As New clsOTDBDefConfiguration
        '    Dim i As Integer

        '    With aDefConfiguration
        '        If .Inject(CONFIGNAME:="CTUSAGE") Then
        '            .delete()
        '        End If
        '        Call .create(CONFIGNAME:="CTUSAGE")
        '        For i = 1 To 26

        '            Call .addItemByValues(ID:="CT" & i, DATATYPE:=otFieldDataType.[Long], TITLE:="Usage on cartype H" & Format(i, "0#"))
        '        Next i
        '        .Persist()
        '    End With


        '    Dim aConfig As New clsOTDBConfigurable
        '    If Not aConfig.createSchema() Then
        '        Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
        '                                     message:="Schema Schedle couldn't be created")


        '    End If
        '    Dim aConfigItem As New clsOTDBConfigurableItem
        '    If Not aConfigItem.createSchema() Then
        '        Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
        '                                     message:="Schema Schedle couldn't be created")
        '    End If

        '    Dim aConfigLink As New clsOTDBConfigurableLink
        '    If Not aConfigLink.createSchema() Then
        '        Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
        '                                     message:="Schema Schedle couldn't be created")
        '    End If

        'End Sub
        '******* createDatabase_ConfigSection
        '*******
        Public Sub XChange()


            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_MQFXCHANGE", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If


            ' create
            Dim aMQF As New clsOTDBMessageQueue
            If Not aMQF.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema aMQF couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="MQF is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aMQF.TableID)
            End If

            Dim aMQFE As New clsOTDBMessageQueueEntry
            If Not aMQFE.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema aMQF couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="MQF Queue Entry is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aMQFE.TableID)

            End If
            Dim aMQFMember As New clsOTDBMessageQueueMember
            If Not aMQFMember.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema aMQF couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="MQF Queue Member is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aMQFMember.TableID)

            End If

            Dim aXChangeConfig As New clsOTDBXChangeConfig
            If Not aXChangeConfig.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="XChangeConfig couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="XChangeConfig is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aXChangeConfig.TableID)

            End If

            Dim aXChange As New clsOTDBXChangeMember
            If Not aXChange.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="XChangeMember couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="XChangeMember is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aXChange.TableID)

            End If

            Dim anOutline As New XOutline
            If Not XOutline.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="Outline couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="Outline is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=XOutline.constTableID)

            End If


            If Not XOutlineItem.CreateSchema() Then
                Call CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="OutlineItem couldn't be created", _
                                             messagetype:=otCoreMessageType.InternalError, tablename:=XOutlineItem.constTableID)
            Else
                Call CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_MQFXCHANGE", message:="OutlineItem is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=XOutlineItem.constTableID)

            End If

        End Sub
        ''' <summary>
        ''' creates or updates the schemata for all core definition objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub CoreDefinition()

            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_CoreData", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If


            ' create
            Dim aDefLogMsg As New ObjectLogMessageDef

            If Not ObjectLogMessageDef.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="DefLogMsg couldn't be created", _
                                             messagetype:=otCoreMessageType.ApplicationError, tablename:=aDefLogMsg.TableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="DefLogMsg is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefLogMsg.TableID)
            End If

            Dim aLogMessage As New ObjectLogMessage
            If Not ObjectLogMessage.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:="Schema MessageLogMember couldn't be created", messagetype:=otCoreMessageType.ApplicationError, tablename:=aLogMessage.TableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:=" MessageLogMember is-up-to-date", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aLogMessage.TableID)

            End If

            '***
            'If Not Group.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="dataobject  couldn't be created", _
            '                                 messagetype:=otCoreMessageType.InternalError, objectname:=Group.ConstObjectID, tablename:=Group.ConstTableID)
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="data object could be created", _
            '                                               messagetype:=otCoreMessageType.ApplicationInfo, objectname:=Group.ConstObjectID, tablename:=Group.ConstTableID)
            '    Dim aGroup As Group = Group.Create(groupname:="admin")
            '    If aGroup IsNot Nothing Then
            '        aGroup.Description = "Administratio group"
            '        aGroup.HasAlterSchemaRights = True
            '        aGroup.HasReadRights = True
            '        aGroup.HasUpdateRights = True
            '        aGroup.HasNoRights = False
            '        If aGroup.Persist() Then
            '            Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", objectname:=Group.ConstObjectID, _
            '                                        message:="Group Admin created", messagetype:=otCoreMessageType.ApplicationInfo)
            '        End If

            '    End If

            'End If

            'If Not GroupMember.CreateSchema() Then
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="dataobject  couldn't be created", _
            '                                 messagetype:=otCoreMessageType.InternalError, objectname:=GroupMember.ConstObjectID, tablename:=GroupMember.ConstTableID)
            'Else
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="data object could be created", _
            '                                               messagetype:=otCoreMessageType.ApplicationInfo, objectname:=GroupMember.ConstObjectID, tablename:=GroupMember.ConstTableID)
            'End If
            '** User Setting
            If Not UserSetting.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="UserSettings couldn't be created", _
                                             messagetype:=otCoreMessageType.ApplicationError, tablename:=UserSetting.ConstTableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="UserSettings could be created", _
                                                           messagetype:=otCoreMessageType.ApplicationInfo, tablename:=UserSetting.ConstTableID)
            End If
            '*** Create Default Users
            Dim anUser As User = User.Create(username:="Admin")
            If anUser IsNot Nothing Then
                anUser.Description = "Administrator"
                anUser.DefaultWorkspaceID = "@"
                anUser.GroupNames = {"admin"}
                anUser.Password = "Admin"
                anUser.HasAlterSchemaRights = True
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = True
                anUser.IsAnonymous = False
                anUser.Persist()
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
                                             message:="User Admin created", messagetype:=otCoreMessageType.ApplicationInfo)
            End If
            anUser = User.Create(username:="boschnei")
            If anUser IsNot Nothing Then
                anUser.Description = "Boris Schneider"
                anUser.GroupNames = {"admin"}
                anUser.DefaultWorkspaceID = "@"
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
                anUser.GroupNames = {"anon"}
                anUser.DefaultWorkspaceID = "@"
                anUser.Password = ""
                anUser.HasAlterSchemaRights = False
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = False
                anUser.IsAnonymous = True
                anUser.PersonName = ""
                anUser.Persist()
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
                                             message:="User anonymous for read created", messagetype:=otCoreMessageType.ApplicationInfo)
            End If
            

                Dim aPerson As New Person
                If Not Person.CreateSchema() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aPerson.TableID, _
                                                 message:="Schema DefPerson couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
                Else
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aPerson.TableID, _
                                                 message:="Schema DefPerson up-to-date", messagetype:=otCoreMessageType.ApplicationInfo)
                End If
                ' create
                Dim aOrgUnit As New OrgUnit
                If Not OrgUnit.CreateSchema() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aOrgUnit.TableID, _
                                                 message:="Schema OrgUnit couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
                Else
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aOrgUnit.TableID, _
                                                 message:="Schema OrgUnit upt-to-date", messagetype:=otCoreMessageType.ApplicationInfo)
                End If


                ' create
                Dim aSite As New Site
                If Not Site.CreateSchema() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aSite.TableID, _
                                                 message:="Schema DefSite couldn't be created", messagetype:=otCoreMessageType.ApplicationError)

                Else
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aSite.TableID, _
                                                 message:="Schema DefSite up-to-date", messagetype:=otCoreMessageType.ApplicationInfo)

                   
                End If

                Dim aStatusItem As New StatusItem
                If Not StatusItem.CreateSchema() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aStatusItem.TableID, _
                                                 message:="Schema " & aStatusItem.TableID & " couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
                Else
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aStatusItem.TableID, _
                                                 message:="Schema " & aStatusItem.TableID & " up-to-date", messagetype:=otCoreMessageType.ApplicationInfo)
                End If


                Dim aDBWareHouse As New clsOTDBDataWareHouse
                If Not aDBWareHouse.createSchema() Then
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="datawarehouse couldn't be created", _
                                                 tablename:=aDBWareHouse.TableID, messagetype:=otCoreMessageType.ApplicationError)
                Else
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                 message:="schema datawarehouse up-to-date", _
                                                 tablename:=aDBWareHouse.TableID, messagetype:=otCoreMessageType.ApplicationInfo)
                End If


        End Sub

        ''' <summary>
        ''' creates or updates all schemata for deliverables business objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Deliverables()
            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_BO", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If

            Dim aDeliverableTarget As New Target
            Dim aDeliverableTrack As New Track
            Dim aDeliverable As New Deliverable

            ' Create the DeliverableTarget
            If Not Target.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema DeliverableTarget couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Deliverable Target is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDeliverableTarget.TableID)

            End If


            ' Create the DeliverableTrack
            If Not Track.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema DeliverableTrack couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Deliverable Track is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDeliverableTrack.TableID)

            End If

            ' Create the DeliverableTrack
            If Not Deliverable.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema DeliverableTrack couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Deliverable  is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDeliverable.TableID)


            End If

            Dim aPart As New Part
            If Not Part.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Parts couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Part  is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aPart.TableID)


            End If

           

        End Sub
        ''' <summary>
        ''' creates or updates all schemata for pats business objects
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Parts()
            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_BO", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If

            Dim aPart As New Part
            If Not Part.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Parts couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Part  is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aPart.TableID)


            End If



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
                Dim anObjectDefinition = ot.CurrentSession.Objects.GetObject(objectname:=anObjectID, runtimeOnly:=CurrentSession.IsBootstrappingInstallationRequested)
                If anObjectDefinition IsNot Nothing Then
                    theObjects.Add(anObjectDefinition)
                End If
            Next

            '*** create all the schema for the objects
            For Each anobjectdefinition In theObjects
                result = result And anobjectdefinition.CreateObjectSchema(silent:=True)
                If result Then
                    Call ot.CoreMessageHandler(subname:="createDatabase.CreateAndPersist", _
                                                           message:="Schema for  Object " & anobjectdefinition.ID & " updated or created to version " & anobjectdefinition.Version & ". Tables created or updated:" & Converter.Enumerable2String(anobjectdefinition.Tablenames), _
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
        Public Sub Run(Optional modules As IEnumerable(Of String) = Nothing, Optional force As Boolean = False)

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
                                               subname:="createDatabase.RUN")
            ElseIf Convert.ToUInt64(schemaversion) < ot.SchemaVersion Then
                Call CoreMessageHandler(message:="Schema version for database available - assuming upgrade installation", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="createDatabase.RUN", arg1:=schemaversion)
            ElseIf Convert.ToUInt64(schemaversion) > ot.SchemaVersion Then
                Call CoreMessageHandler(message:="Schema version for database available but higher ( " & schemaversion & " ) - downgrading ?!", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="createDatabase.RUN", arg1:=ot.SchemaVersion)
            Else
                Call CoreMessageHandler(message:="Schema version for database available - assuming repair installation", messagetype:=otCoreMessageType.InternalInfo, _
                                               subname:="createDatabase.RUN", arg1:=schemaversion)
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
                                            subname:="createDatabase.RUN", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
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
                    Call CoreMessageHandler(message:="Administrator account created - please use it to log into the OnTrack Database for further schema setup", messagetype:=otCoreMessageType.InternalInfo, _
                                                 subname:="createDatabase.RUN", break:=False, showmsgbox:=True, noOtdbAvailable:=True)

                Else
                    Call CoreMessageHandler(message:="Administrator Account could not be created - Please see your system administrator.", messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="modCreateDB.createDatabase_CoreData", _
                                                break:=False, showmsgbox:=True, noOtdbAvailable:=True)
                    Return
                End If
            End If

            '*** create global domain
            If CurrentDBDriver.CreateGlobalDomain(nativeConnection:=aNativeConnection) Then
                Call CoreMessageHandler(message:="global domain created", arg1:=ConstGlobalDomain, messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="createDatabase.RUN")
            End If

            '*** set objects to load
            Call CurrentDBDriver.SetDBParameter(ConstPNObjectsLoad, _
                                                         Schedule.ConstObjectID & ", " & _
                                                         ScheduleMilestone.ConstObjectID & ", " & _
                                                         Deliverable.ConstObjectID, silent:=True)
            '*** bootstrap checksum
            CurrentDBDriver.SetDBParameter(ConstPNBootStrapSchemaChecksum, value:=ot.GetBootStrapSchemaChecksum, silent:=True)

            '**** Create the core objects first
            '****
            If modules.Contains(ConstModuleCore.ToUpper) Then
                descriptions = ot.GetObjectClassDescriptionsForModule(ConstModuleCore)
                objectids = New List(Of String)

                For Each description In descriptions
                    Dim addflag As Boolean = False

                    For Each tablename In description.Tables
                        Dim value = GetDBParameter(ConstPNBSchemaVersion_TableHeader & tablename, silent:=True)
                        If value Is Nothing OrElse Not IsNumeric(value) OrElse Not CurrentDBDriver.HasTable(tablename) Then
                            addflag = True
                        ElseIf Convert.ToUInt64(value) > description.GetSchemaTableAttribute(tablename).Version Then
                            CoreMessageHandler(message:="WARNING ! Version of Table in database is higher ( " & value & ") than in class description ( " & description.GetSchemaTableAttribute(tablename).Version & "). Downgrading ?!", messagetype:=otCoreMessageType.InternalWarning, _
                                                subname:="createDatabase.RUN", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
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
                If modulename <> ConstModuleCore Then
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
                                                    subname:="createDatabase.RUN", tablename:=tablename, objectname:=description.ID, arg1:=description.GetSchemaTableAttribute(tablename).Version)
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
                    Call ot.CoreMessageHandler(subname:="modCreateDB.createDatabase_RUN", _
                                                      message:="foreign keys created for table " & aTable.Name, _
                                                      tablename:=aTable.Name, _
                                                      messagetype:=otCoreMessageType.ApplicationInfo)
                Else
                    Call ot.CoreMessageHandler(subname:="modCreateDB.createDatabase_RUN", _
                                                     message:="Error while creating foreign keys for table " & aTable.Name, _
                                                     tablename:=aTable.Name, _
                                                     messagetype:=otCoreMessageType.InternalError)
                End If
            Next
            '*** set the current schema version
            CurrentDBDriver.SetDBParameter(parametername:=ConstPNBSchemaVersion, value:=ot.SchemaVersion, silent:=True)

            '*** request end of bootstrap
            '***
            If Not CurrentSession.RequestEndofBootstrap() Then
                Call ot.CoreMessageHandler(showmsgbox:=True, subname:="modCreateDB.createDatabase_RUN", _
                                                       message:="failed to create tables for object repository - abort the installation", _
                                                       messagetype:=otCoreMessageType.InternalError)
                Return
            End If

            '*** start a session
            Dim sessionrunning As Boolean = CurrentSession.IsRunning
            Dim sessionstarted As Boolean = False
            If Not sessionrunning Then
                sessionstarted = CurrentSession.StartUp(otAccessRight.AlterSchema, messagetext:="Please start up a Session to setup initial data")
            End If

            '***
            '*** Initialize Data
            If sessionrunning OrElse sessionstarted Then
                If Not InitialCoreData() Then
                    Call ot.CoreMessageHandler(showmsgbox:=True, subname:="modCreateDB.createDatabase_RUN", _
                                                          message:="failed to write initial core data - core might not be working correctly", _
                                                          messagetype:=otCoreMessageType.InternalError)
                Else
                    ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_RUN", _
                                                          message:="core objects with data instanced and persisted", _
                                                          messagetype:=otCoreMessageType.InternalInfo)
                End If

                If Not InitializeCalendar() Then
                    Call ot.CoreMessageHandler(showmsgbox:=True, subname:="modCreateDB.createDatabase_RUN", _
                                                              message:="failed to write initial calendar data - calendar might not be working correctly", _
                                                              messagetype:=otCoreMessageType.InternalError)
                Else
                    ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_RUN", _
                                                         message:="calendar instanced and persisted", _
                                                         messagetype:=otCoreMessageType.InternalInfo)
                End If
            End If
            '*** shutdown a session
            If CurrentSession.IsRunning AndAlso sessionstarted Then
                CurrentSession.ShutDown(force:=True)
            End If
        End Sub
        ''' <summary>
        ''' Initialize the Calendar
        ''' </summary>
        ''' <remarks></remarks>
        Public Function InitializeCalendar() As Boolean

            ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_RUN", _
                                                     message:="creating calendar - please stand by ...", _
                                                     messagetype:=otCoreMessageType.ApplicationInfo)
            ''' generate the days
            CalendarEntry.GenerateDays(fromdate:=CDate("01.01.2013"), untildate:=CDate("01.01.2016"), name:=ot.CurrentSession.DefaultCalendarName)

            Dim acalentry As CalendarEntry
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    ' additional
                    .Datevalue = CDate("29.03.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If

            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("01.04.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("09.05.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If

            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("10.05.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Christi Himmelfahrt Brückentag"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.05.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Reformationstag (Sachsen)"
                  End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.11.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Buß- und Bettag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("18.04.2014")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("01.04.2014")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("29.05.2013")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("20.05.2014")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2014")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Reformationstag (Sachsen)"
                    .Persist()
               End With
            End If
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("19.11.2014")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Buß- und Bettag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("03.04.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Karfreitag (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("06.04.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "EasterMonday (Eastern)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("14.05.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Christi Himmelfahrt"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("25.05.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Pfingsten"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("31.10.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Reformationstag (Sachsen)"
                    .Persist()
                End With
            End If
            acalentry = CalendarEntry.Create()
            If acalentry IsNot Nothing Then
                With acalentry
                    .Datevalue = CDate("18.11.2015")
                    .entrytype = otCalendarEntryType.DayEntry
                    .notAvailable = True
                    .description = "Buß- und Bettag (Sachsen)"
                    .Persist()

                End With
            End If

            Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=acalentry.TableID, _
                                         message:="Calendar until 31.12.2016 created", messagetype:=otCoreMessageType.ApplicationInfo)

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
                aDomain.SetSetting(id:=Session.ConstCPDependencySynchroMinOverlap, datatype:=otFieldDataType.Long, value:=7)
                aDomain.SetSetting(id:=Session.ConstCPDefaultWorkspace, datatype:=otFieldDataType.Text, value:="@")
                aDomain.SetSetting(id:=Session.ConstCPDefaultCalendarName, datatype:=otFieldDataType.Text, value:="default")
                aDomain.SetSetting(id:=Session.ConstCPDefaultTodayLatency, datatype:=otFieldDataType.Long, value:=-14)
                aDomain.SetSetting(id:=Session.ConstCDefaultScheduleTypeID, datatype:=otFieldDataType.Text, value:="none")
                aDomain.SetSetting(id:=Session.ConstCDefaultDeliverableTypeID, datatype:=otFieldDataType.Text, value:="")
                aDomain.Persist()
            End If

            '*** Project Base workspaceID
            Dim aWorkspace = Workspace.Create("@")
            If aWorkspace IsNot Nothing Then
                aWorkspace.Description = "base workspaceID for SBB ENG Planning"
                aWorkspace.IsBasespace = True
                aWorkspace.FCRelyingOn = New String() {"@"}
                aWorkspace.ACTRelyingOn = New String() {"@"}
                aWorkspace.AccesslistIDs = New String() {"PrjPlanner"}
                aWorkspace.HasActuals = True
                aWorkspace.Min_schedule_updc = 1
                aWorkspace.Max_schedule_updc = 999
                aWorkspace.Min_target_updc = 1
                aWorkspace.Max_target_updc = 999
                aWorkspace.Persist()

                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.InitialCoreData", _
                                             message:="base workspaceID @ created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aWorkspace.TableID)
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
            '    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.InitialCoreData", _
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
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.InitialCoreData", objectname:=Group.ConstObjectID, _
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
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.InitialCoreData", objectname:=Group.ConstObjectID, _
                                                message:="Group Readers created", messagetype:=otCoreMessageType.ApplicationInfo)
                End If

            End If
            '*** Create Default Users
            '***
            Dim anUser As User = User.Create(username:="admin")
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
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
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
                anUser.Password = ""
                anUser.HasAlterSchemaRights = False
                anUser.HasNoRights = False
                anUser.HasReadRights = True
                anUser.HasUpdateRights = False
                anUser.IsAnonymous = True
                anUser.PersonName = ""
                anUser.Persist()
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
                                             message:="User anonymous for read created", messagetype:=otCoreMessageType.ApplicationInfo)
            End If

            '*** different Sites
            Dim aSite As Site
            aSite = Site.Create("GO")
            If aSite IsNot Nothing Then
                aSite.Description = "Görlitz"
                aSite.Persist()
            End If
            aSite = Site.Create("HE")
            If aSite IsNot Nothing Then
                aSite.Description = "Hennigsdorf"
                aSite.Persist()
            End If

            Return True
        End Function
    End Module
End Namespace