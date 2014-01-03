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
                    If Not .Create("full") Then .LoadBy("full")
                    .description = "full engineering cycle (3D Design)"
                    .Persist()

                End With
                aScheduleDef = New ScheduleDefinition
                With aScheduleDef
                    If Not .Create("pdm") Then .LoadBy("pdm")
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
                    If Not .Create("nocad") Then .LoadBy("nocad")
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
                    If Not .Create("full", "bp11") Then .LoadBy("full", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp12") Then .LoadBy("full", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp1") Then .LoadBy("full", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp2") Then .LoadBy("full", "bp2")
                    .ActualOfFC = "bp1"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp13") Then .LoadBy("full", "bp13")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 20
                    .Description = "ifm status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp3") Then .LoadBy("full", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp4") Then .LoadBy("full", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp5") Then .LoadBy("full", "bp5")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 35
                    .Description = "dmu status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp6") Then .LoadBy("full", "bp6")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 35
                    .Description = "dmu date"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp20") Then .LoadBy("full", "bp20")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 40
                    .Description = "fc fem"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp21") Then .LoadBy("full", "bp21")
                    .ActualOfFC = ""
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 40
                    .Description = "fem status"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp22") Then .LoadBy("full", "bp22")
                    .ActualOfFC = "bp20"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 40
                    .Description = "fem status date"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp7") Then .LoadBy("full", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp8") Then .LoadBy("full", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM

                    If Not .Create("full", "bp9") Then .LoadBy("full", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp10") Then .LoadBy("full", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("full", "bp80") Then .LoadBy("full", "bp80")
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
                    If Not .Create("nocad", "bp11") Then .LoadBy("nocad", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = False
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()
                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp12") Then .LoadBy("nocad", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsFacultative = False
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp1") Then .LoadBy("nocad", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp2") Then .LoadBy("nocad", "bp2")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp3") Then .LoadBy("nocad", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 30
                    .Description = "design freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp4") Then .LoadBy("nocad", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 30
                    .Description = "design freeze"
                    .Persist()

                End With

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp7") Then .LoadBy("nocad", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp8") Then .LoadBy("nocad", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp9") Then .LoadBy("nocad", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = False
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp10") Then .LoadBy("nocad", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("nocad", "bp80") Then .LoadBy("nocad", "bp80")
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
                    If Not .Create("pdm", "bp11") Then .LoadBy("pdm", "bp11")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp12") Then .LoadBy("pdm", "bp12")
                    .ActualOfFC = "bp11"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 10
                    .Description = "start work"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp1") Then .LoadBy("pdm", "bp1")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp2") Then .LoadBy("pdm", "bp2")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 20
                    .Description = "ifm freeze"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp3") Then .LoadBy("pdm", "bp3")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsForbidden = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp4") Then .LoadBy("pdm", "bp4")
                    .ActualOfFC = "bp3"
                    .IsForecast = False
                    .IsForbidden = True
                    .Orderno = 30
                    .Description = "fap"
                    .Persist()

                End With

                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp7") Then .LoadBy("pdm", "bp7")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp8") Then .LoadBy("pdm", "bp8")
                    .ActualOfFC = "bp7"
                    .IsForecast = False
                    .IsFacultative = True
                    .Orderno = 80
                    .Description = "pdm entry"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp9") Then .LoadBy("pdm", "bp9")
                    .ActualOfFC = ""
                    .IsForecast = True
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp10") Then .LoadBy("pdm", "bp10")
                    .ActualOfFC = "bp9"
                    .IsForecast = False
                    .IsMandatory = True
                    .Orderno = 90
                    .Description = "pdm approval"
                    .Persist()

                End With
                aScheduleDefM = New ScheduleMilestoneDefinition
                With aScheduleDefM
                    If Not .Create("pdm", "bp80") Then .LoadBy("pdm", "bp80")
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
                    If Not .Create("none", "bp80") Then .LoadBy("none", "bp80")
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
                    If Not .Create("full", "synchro") Then .LoadBy("full", "synchro")
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
                    If Not .Create("full", "development") Then .LoadBy("full", "development")
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
                    If Not .Create("full", "approve") Then .LoadBy("full", "approve")
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
                    If Not .Create("pdm", "approve") Then .LoadBy("pdm", "approve")
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
                    If Not .Create("bp11") Then .LoadBy("bp11")
                    .Description = "FC start work"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()
                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp12") Then .LoadBy("bp12")
                    .Description = "start work"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp1") Then .LoadBy("bp1")
                    .Description = "FC IFM freeze gate"
                    .IsForecast = True
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp2") Then .LoadBy("bp2")
                    .Description = "IFM freeze gate"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp13") Then .LoadBy("bp13")
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
                    If Not .Create("bp15") Then .LoadBy("bp15")
                    .Description = "current IFM freeze status date"
                    .IsForecast = False
                    .Datatype = otFieldDataType.[Date]
                    .IsOfDate = True
                    .referingToID = "bp13"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp3") Then .LoadBy("bp3")
                    .Description = "FC FAP / Design Freeze status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp4") Then .LoadBy("bp4")
                    .Description = "FAP / Design Freeze gate"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp5") Then .LoadBy("bp5")
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
                    If Not .Create("bp6") Then .LoadBy("bp6")
                    .Description = "dmu status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .referingToID = "bp5"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp20") Then .LoadBy("bp20")
                    .Description = "FC FEM result date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp22") Then .LoadBy("bp22")
                    .Description = "FEM status date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .referingToID = "bp21"
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp21") Then .LoadBy("bp21")
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
                    If Not .Create("bp7") Then .LoadBy("bp7")
                    .Description = "FC PDM entry date (outgoing ENG)"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp8") Then .LoadBy("bp8")
                    .Description = "entry PDM date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp14") Then .LoadBy("bp14")
                    .Description = "outgoing PDM DRL date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp9") Then .LoadBy("bp9")
                    .Description = "FC PDM approval date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = True
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp10") Then .LoadBy("bp10")
                    .Description = "PDM approval date"
                    .Datatype = otFieldDataType.[Date]
                    .IsForecast = False
                    .IsOfDate = True
                    .Persist()

                End With
                aDefMilestone = New MileStoneDefinition
                With aDefMilestone
                    If Not .Create("bp80") Then .LoadBy("bp80")
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
        Public Sub Configurables()

            If Not ot.CurrentSession.RequireAccessRight(otAccessRight.AlterSchema) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase_CONFIG", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If


            Dim aDefConfigItem As New clsOTDBDefConfigurationItem
            If Not clsOTDBDefConfigurationItem.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema  couldn't be created")

            End If

            Dim aDefConfiguration As New clsOTDBDefConfiguration
            Dim i As Integer

            With aDefConfiguration
                If .loadBy(CONFIGNAME:="CTUSAGE") Then
                    .delete()
                End If
                Call .create(CONFIGNAME:="CTUSAGE")
                For i = 1 To 26

                    Call .addItemByValues(ID:="CT" & i, DATATYPE:=otFieldDataType.[Long], TITLE:="Usage on cartype H" & Format(i, "0#"))
                Next i
                .Persist()
            End With


            Dim aConfig As New clsOTDBConfigurable
            If Not aConfig.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Schedle couldn't be created")


            End If
            Dim aConfigItem As New clsOTDBConfigurableItem
            If Not aConfigItem.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Schedle couldn't be created")
            End If

            Dim aConfigLink As New clsOTDBConfigurableLink
            If Not aConfigLink.createSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Schedle couldn't be created")
            End If

        End Sub
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

            ' DomainSetting
            If Not DomainSetting.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                      message:="Schema domain setting couldn't be created", messagetype:=otCoreMessageType.ApplicationError, _
                                      tablename:=DomainSetting.ConstTableID)

            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                            message:="Schema domains settings could be created", messagetype:=otCoreMessageType.ApplicationInfo, _
                                                            tablename:=DomainSetting.ConstTableID)
            End If
            ' Domains
            Dim aDomain As New Domain
            If Not Domain.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                       message:="Schema domains couldn't be created", messagetype:=otCoreMessageType.ApplicationError, tablename:=Domain.constTableID)

            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                            message:="Schema domains could be created", messagetype:=otCoreMessageType.ApplicationInfo, _
                                                            tablename:=Domain.constTableID)
                '*** 
                If Not aDomain.Create(domainID:=ConstGlobalDomain) Then
                    aDomain.LoadBy(domainID:=ConstGlobalDomain)
                End If

                aDomain.Description = "global domain for all Ontrack data in this database"
                aDomain.IsGlobal = True
                aDomain.MinDeliverableUID = 1
                aDomain.MaxDeliverableUID = 999999999
                aDomain.SetSetting(id:=Session.ConstCPDefaultWorkspace, datatype:=otFieldDataType.Text, value:="@")
                aDomain.SetSetting(id:=Session.ConstCDefaultScheduleTypeID, datatype:=otFieldDataType.Text, value:="full")
                aDomain.SetSetting(id:=Session.ConstCPDefaultCalendarName, datatype:=otFieldDataType.Text, value:="default")
                aDomain.SetSetting(id:=Session.ConstCPDefaultTodayLatency, datatype:=otFieldDataType.Long, value:=7)
                aDomain.SetSetting(id:=Session.ConstCPDependencySynchroMinOverlap, datatype:=otFieldDataType.Long, value:=-14)
                aDomain.Persist()

            End If

            ' workspaceID
            Dim aDefWorkspace As New Workspace
            If Not Workspace.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:="Schema workspaces couldn't be created", messagetype:=otCoreMessageType.ApplicationError, tablename:=aDefWorkspace.TableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:="Schema workspaces could be created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefWorkspace.TableID)
                '*** Project Base workspaceID
                If aDefWorkspace.Create("@") Then
                    aDefWorkspace.Description = "base workspaceID for SBB ENG Planning"
                    aDefWorkspace.IsBasespace = True
                    aDefWorkspace.FCRelyingOn = New String() {"@"}
                    aDefWorkspace.ACTRelyingOn = New String() {"@"}
                    aDefWorkspace.AccesslistIDs = New String() {"PrjPlanner"}
                    aDefWorkspace.HasActuals = True
                    aDefWorkspace.Min_schedule_updc = 1
                    aDefWorkspace.Max_schedule_updc = 999
                    aDefWorkspace.Min_target_updc = 1
                    aDefWorkspace.Max_target_updc = 999
                    aDefWorkspace.Persist()

                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                 message:="base workspaceID @ created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefWorkspace.TableID)
                End If
                '*** workspaceID
                aDefWorkspace = New Workspace
                If aDefWorkspace.Create("PSIM01") Then
                    aDefWorkspace.Description = "Project Simulation workspaceID"
                    aDefWorkspace.IsBasespace = False
                    aDefWorkspace.FCRelyingOn = New String() {"@", "PSIM01"}
                    aDefWorkspace.ACTRelyingOn = New String() {"@"}
                    aDefWorkspace.HasActuals = False
                    aDefWorkspace.AccesslistIDs = New String() {"PrjPlanner"}
                    aDefWorkspace.Min_schedule_updc = 1000
                    aDefWorkspace.Max_schedule_updc = 1099
                    aDefWorkspace.Min_target_updc = 1000
                    aDefWorkspace.Max_target_updc = 1099
                    aDefWorkspace.Persist()
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                 message:="workspaceID PSIM01 created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefWorkspace.TableID)

                End If
                '*** workspaceID
                aDefWorkspace = New Workspace
                If aDefWorkspace.Create("APQ") Then
                    aDefWorkspace.Description = "Approval Queue workspaceID"
                    aDefWorkspace.IsBasespace = False
                    aDefWorkspace.FCRelyingOn = New String() {"@", "APQ"}
                    aDefWorkspace.ACTRelyingOn = New String() {"@"}
                    aDefWorkspace.HasActuals = False
                    aDefWorkspace.AccesslistIDs = New String() {"PrjPlanner"}
                    aDefWorkspace.Min_schedule_updc = 2000
                    aDefWorkspace.Max_schedule_updc = 2099
                    aDefWorkspace.Min_target_updc = 2000
                    aDefWorkspace.Max_target_updc = 2099
                    aDefWorkspace.Persist()
                    Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                 message:="base workspaceID APQ created", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aDefWorkspace.TableID)

                End If

            End If

            Dim anAccessList As New clsOTDBDefUserAccessList
            If Not clsOTDBDefUserAccessList.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:="Schema DefUserAccessList couldn't be created", messagetype:=otCoreMessageType.ApplicationError, tablename:=anAccessList.TableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                             message:="Schema DefUserAccessList up-to-date", messagetype:=otCoreMessageType.ApplicationInfo, tablename:=anAccessList.TableID)

                anAccessList = New clsOTDBDefUserAccessList
                If anAccessList.Create(id:="PrjPlanner", username:="boschnei") Then
                    anAccessList.HasUpdateRights = True
                    anAccessList.Description = "Project Planner List Member"
                    anAccessList.Persist()
                End If
            End If
            '** User Setting
            If Not UserSetting.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="UserSettings couldn't be created", _
                                             messagetype:=otCoreMessageType.ApplicationError, tablename:=UserSetting.ConstTableID)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", message:="UserSettings could be created", _
                                                           messagetype:=otCoreMessageType.ApplicationInfo, tablename:=UserSetting.ConstTableID)
            End If
            Dim anUser As New User
            If Not User.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
                                             message:="Schema DefUser couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=anUser.TableID, _
                                             message:="Schema DefUser up-to-date", messagetype:=otCoreMessageType.ApplicationInfo)
                anUser = New User
                If anUser.Create("Admin") Then
                    anUser.Description = "Administrator"
                    anUser.DefaultWorkspaceID = "@"
                    anUser.Group = "admin"
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
                anUser = New User
                If anUser.Create("boschnei") Then
                    anUser.Description = "Boris Schneider"
                    anUser.Group = "admin"
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
                anUser = New User
                If anUser.Create("anonymous") Then
                    anUser.Description = "anonymous"
                    anUser.Group = "anon"
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

                If aSite.Create("GO") Then
                    aSite.Description = "Görlitz"
                    aSite.Persist()
                End If
                If aSite.Create("HE") Then
                    aSite.Description = "Hennigsdorf"
                    aSite.Persist()
                End If
                If aSite.Create("BR") Then
                    aSite.Description = "Bruegge"
                    aSite.Persist()
                End If
                If aSite.Create("KAS") Then
                    aSite.Description = "Kassel"
                    aSite.Persist()
                End If
                If aSite.Create("DER") Then
                    aSite.Description = "Derby"
                    aSite.Persist()
                End If
                If aSite.Create("VIL") Then
                    aSite.Description = "Villeneuve"
                    aSite.Persist()
                End If
                If aSite.Create("AM") Then
                    aSite.Description = "Ammendorf"
                    aSite.Persist()
                End If
                If aSite.Create("VL") Then
                    aSite.Description = "Vado Ligure"
                    aSite.Persist()
                End If
            End If

            Dim aCalEntry As New CalendarEntry
            If Not CalendarEntry.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aCalEntry.TableID, _
                                             message:="Schema " & aCalEntry.TableID & " couldn't be created", messagetype:=otCoreMessageType.ApplicationError)
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aCalEntry.TableID, _
                                             message:="Schema " & aCalEntry.TableID & " up-to-date", messagetype:=otCoreMessageType.ApplicationInfo)
                aCalEntry = New CalendarEntry
                With aCalEntry
                    Call .GenerateDays(fromdate:=CDate("01.01.2011"), untildate:=CDate("01.01.2016"), name:=ot.CurrentSession.DefaultCalendarName)
                    ' additional

                    If .Create() Then
                        .Datevalue = CDate("29.03.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Karfreitag (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("01.04.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "EasterMonday (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("09.05.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Christi Himmelfahrt"
                        .Persist()
                    End If
                End With

                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("10.05.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Christi Himmelfahrt Brückentag"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("20.05.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Pfingsten"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("31.10.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Reformationstag (Sachsen)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("20.11.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Buß- und Bettag (Sachsen)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("18.04.2014")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Karfreitag (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("01.04.2014")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "EasterMonday (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("29.05.2013")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Christi Himmelfahrt"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("20.05.2014")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Pfingsten"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("31.10.2014")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Reformationstag (Sachsen)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("19.11.2014")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Buß- und Bettag (Sachsen)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("03.04.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Karfreitag (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("06.04.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "EasterMonday (Eastern)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("14.05.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Christi Himmelfahrt"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("25.05.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Pfingsten"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("31.10.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Reformationstag (Sachsen)"
                        .Persist()
                    End If
                End With
                aCalEntry = New CalendarEntry
                With aCalEntry
                    If .Create() Then
                        .Datevalue = CDate("18.11.2015")
                        .entrytype = otCalendarEntryType.DayEntry
                        .notAvailable = True
                        .description = "Buß- und Bettag (Sachsen)"
                        .Persist()
                    End If
                End With
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", tablename:=aCalEntry.TableID, _
                                             message:="Calendar until 31.12.2016 created", messagetype:=otCoreMessageType.ApplicationInfo)
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

            Dim aPart As New clsOTDBPart
            If Not clsOTDBPart.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Parts couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Part  is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aPart.TableID)


            End If

            ' Create the Trackitems
            If Not clsOTDBTrackItem.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase_BO", _
                                             message:="Schema TrackITems couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Trackitem is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=clsOTDBTrackItem.constTableID)

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

            Dim aPart As New clsOTDBPart
            If Not clsOTDBPart.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", _
                                             message:="Schema Parts couldn't be created")
            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_BO", message:="Part  is up-to-date", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, tablename:=aPart.TableID)


            End If



        End Sub
        ''' <summary>
        ''' Creates or updates all the Database Schema for all object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Run()

            Dim aNativeConnection = CurrentDBDriver.CurrentConnection.NativeConnection
            CurrentDBDriver.CreateDBParameterTable(nativeConnection:=aNativeConnection)
            '** User ?!
            If Not CurrentDBDriver.HasTable(User.ConstTableID, nativeConnection:=aNativeConnection) Then
                '** Create SuperUser
                If Not CurrentDBDriver.CreateDBUserDefTable(nativeConnection:=aNativeConnection) Then
                    Call CoreMessageHandler(message:="User Table could not be created", messagetype:=otCoreMessageType.InternalError, _
                                                 subname:="clsADONETDBDriver.getUserValidation", _
                                                 break:=False, showmsgbox:=True, noOtdbAvailable:=True)

                Else
                    Call CoreMessageHandler(message:="User Table could not be created", messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="clsADONETDBDriver.getUserValidation", _
                                                break:=False, showmsgbox:=True, noOtdbAvailable:=True)
                End If

            End If

            ' Create the SchemaDirectory
            If Not ObjectEntryDefinition.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="Schema Directory couldn't be created")
            End If
            ' DomainSetting
            If Not DomainSetting.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                      message:="Schema domain setting couldn't be created", messagetype:=otCoreMessageType.ApplicationError, _
                                      tablename:=DomainSetting.ConstTableID)

            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                            message:="Schema domains settings could be created", messagetype:=otCoreMessageType.ApplicationInfo, _
                                                            tablename:=DomainSetting.ConstTableID)
            End If
            ' Domains
            Dim aDomain As New Domain
            If Not Domain.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                       message:="Schema domains couldn't be created", messagetype:=otCoreMessageType.ApplicationError, tablename:=Domain.ConstTableID)

            Else
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="modCreateDB.createDatabase_CoreData", _
                                                            message:="Schema domains could be created", messagetype:=otCoreMessageType.ApplicationInfo, _
                                                            tablename:=Domain.ConstTableID)
                '*** 
                If Not aDomain.Create(domainID:=ConstGlobalDomain) Then
                    aDomain.LoadBy(domainID:=ConstGlobalDomain)
                End If

                aDomain.Description = "global domain for all Ontrack data in this database"
                aDomain.IsGlobal = True
                aDomain.MinDeliverableUID = 1
                aDomain.MaxDeliverableUID = 999999999
                aDomain.SetSetting(id:=Session.ConstCPDefaultWorkspace, datatype:=otFieldDataType.Text, value:="@")
                aDomain.SetSetting(id:=Session.ConstCDefaultDeliverableTypeID, datatype:=otFieldDataType.Text, value:="")
                aDomain.SetSetting(id:=Session.ConstCPDefaultCalendarName, datatype:=otFieldDataType.Text, value:="default")
                aDomain.SetSetting(id:=Session.ConstCPDefaultTodayLatency, datatype:=otFieldDataType.Long, value:=7)
                aDomain.SetSetting(id:=Session.ConstCPDependencySynchroMinOverlap, datatype:=otFieldDataType.Long, value:=-14)
                aDomain.Persist()

            End If

            ' Create the Session ErrorLog
            If Not CoreError.CreateSchema() Then
                Call ot.CoreMessageHandler(showmsgbox:=False, subname:="createDatabase", message:="Schema ErrorLog couldn't be created")
            End If
            ' Create the MSOTask
            'Dim aMSPTask As New clsOTDBMSPTask
            'If Not aMSPTask.createSchema() Then
            'Call OTDBErrorHandler(SHOWMSGBOX:=False, SUBNAME:="createDatabase", message:="Schema MSPTasks couldn't be created")
            'End If

            If Not ot.CurrentSession.RequireAccessRight(accessRequest:=otAccessRight.AlterSchema, reLogin:=True) Then
                Call ot.CoreMessageHandler(message:="Access right couldnot be set to AlterSchema", subname:="modCreateDB.createDatabase", _
                                             messagetype:=otCoreMessageType.ApplicationInfo, break:=False)
                Exit Sub
            End If

            '** create the different Object Types
            CoreDefinition()
            'Call createDatabase_CONFIG() not finished coding
            'Deliverables()
            'Parts()
            'XChange()
            Scheduling()
        End Sub

    End Module
End Namespace