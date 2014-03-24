
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

Namespace OnTrack.Scheduling


    ''' <summary>
    ''' milestone definition class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(version:=1, ID:=MileStoneDefinition.ConstObjectID, Modulename:=ConstModuleScheduling, _
        Description:="definition of milestones for all schedule types", useCache:=True)> _
    Public Class MileStoneDefinition
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Public Const ConstObjectID = "MilestoneDefinition"
        <ormSchemaTable(version:=2, addDomainBehavior:=True, addsparefields:=True, adddeletefieldbehavior:=True)> Public Const ConstTableID As String = "tblDefMilestones"

        '** keys
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=20, defaultValue:="", primarykeyordinal:=1, _
            XID:="bpd1", title:="ID", description:="id of the milestone")> Public Const ConstFNID = "id"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=2)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, _
           XID:="bpd2", title:="Description", description:="description of the milestone")> Public Const ConstFNDescription = "desc"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
           XID:="bpd3", title:="Type", description:="type of the milestone")> Public Const ConstFNType = "typeid"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
           XID:="bpd4", title:="Datatype", description:="datatype of the milestone")> Public Const ConstFNDatatype = "datatype"
        <ormObjectEntry(referenceobjectentry:=StatusItem.ConstObjectID & "." & StatusItem.constFNType, _
          XID:="bpd5", title:="Status Item Type", description:="status item type of the milestone")> Public Const ConstFNStatus = "status"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, _
         XID:="bpd6", title:="Forecast", description:="set if milestone is a forecast")> Public Const ConstFNIsForecast = "isforecast"
        <ormObjectEntry(referenceobjectentry:=ConstObjectID & "." & ConstFNID, _
        XID:="bpd7", title:="Reference", description:="set if milestone is a reference")> Public Const ConstFNRefID = "refid"



        '** MAPPING
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""  ' id
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNType)> Private _typeid As otMilestoneType
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otFieldDataType
        <ormEntryMapping(EntryName:=ConstFNRefID)> Private _refid As String = ""
        <ormEntryMapping(EntryName:=ConstFNIsForecast)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNStatus)> Private _statustypeid As String = ""



#Region "Properties"


        ' further internals
        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get

        End Property

        Public Property Datatype() As otFieldDataType
            Get
                Datatype = _datatype
            End Get
            Set(value As otFieldDataType)
                _datatype = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Typeid() As otMilestoneType
            Get
                Typeid = _typeid
            End Get
            Set(value As otMilestoneType)
                _typeid = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsOfDate() As Boolean
            Get
                If _typeid = 1 Then
                    IsOfDate = True
                Else
                    IsOfDate = False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    _typeid = 1
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsOfStatus() As Boolean
            Get
                If _typeid = 2 Then
                    IsOfStatus = True
                Else
                    IsOfStatus = False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    _typeid = 2
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsActual() As Boolean
            Get
                If Not _isForecast Then
                    IsActual = True
                Else
                    IsActual = False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    _isForecast = False
                Else
                    _isForecast = True
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsForecast() As Boolean
            Get
                If _isForecast Then
                    IsForecast = True
                Else
                    IsForecast = False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    _isForecast = True
                Else
                    _isForecast = False
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property statustypeid() As String
            Get
                statustypeid = _statustypeid
            End Get
            Set(value As String)
                _statustypeid = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property referingToID() As String
            Get
                referingToID = _refid
            End Get
            Set(value As String)
                _refid = value
                Me.IsChanged = True
            End Set
        End Property

#End Region



        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub


        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional domainID As String = "", Optional forcereload As Boolean = False) As MileStoneDefinition
            Dim primarykey() As Object = {id, domainID}
            Return Retrieve(Of MileStoneDefinition)(pkArray:=primarykey, domainID:=domainID, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' load and infuse a milestone definition 
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal ID As String, Optional domainID As String = "") As Boolean
            Dim pkarray() As Object = {LCase(ID), domainID}
            Return MyBase.Inject(pkarray, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create a persistance schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of MileStoneDefinition)(silent:=silent)
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim primaryColumnNames As New Collection
            '            'Dim workspaceColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.Tablename = constTableID

            '            ' delete it
            '            With aTable
            '                .Inject(constTableID)
            '                .Delete()
            '            End With

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()

            '                'Tablename
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "id of the milestone"
            '                aFieldDesc.ID = "bpd1"
            '                aFieldDesc.ColumnName = "id"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                primaryColumnNames.Add(aFieldDesc.ColumnName)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "description"
            '                aFieldDesc.ID = "bpd2"
            '                aFieldDesc.ColumnName = "desc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'Fieldnames
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "type of milestone (1=date, 2=status)"
            '                aFieldDesc.ID = "bpd3"
            '                aFieldDesc.ColumnName = "typeid"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "statustypeid if status"
            '                aFieldDesc.ID = "bpd4"
            '                aFieldDesc.ColumnName = "statustypeid"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is Milestone Forecast ?"
            '                aFieldDesc.ID = "bpd5"
            '                aFieldDesc.ColumnName = "isforecast"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "referring to Milestone"
            '                aFieldDesc.ID = "bpd6"
            '                aFieldDesc.ColumnName = "refid"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'Fieldnames
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "datatype of milestone"
            '                aFieldDesc.ID = "bpd7"
            '                aFieldDesc.ColumnName = "datatype"
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
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", primaryColumnNames, isprimarykey:=True)
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
            '            Call CoreMessageHandler(subname:="clsOTDBDefScheduleMilestone.createSchema", tablename:=constTableID)
            '            CreateSchema = False
        End Function


        ''' <summary>
        ''' Return a collection of all def Milestones
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All(Optional domainID As String = "") As List(Of MileStoneDefinition)
            Return ormDataObject.All(Of MileStoneDefinition)(domainID:=domainID)
        End Function



        ''' <summary>
        ''' create persistable object with primary key ID
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal ID As String, Optional domainID As String = "") As Boolean
            Dim pkarray() As Object = {ID, domainID}
            Return Me.Create(pkarray, checkUnique:=True, domainID:=domainID)
        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefScheduleTask is the object for a OTDBRecord (which is the datastore)
    '*****       Defines the Schedule Task of Milestones
    '*****
    ''' <summary>
    ''' Definition class of a schedule task
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOTDBDefScheduleTask
        Inherits ormDataObject

        Public Const constTableID As String = "tblDefScheduleTasks"

        ' fields
        Private s_scheduletype As String = ""
        Private s_taskid As String = ""
        Private s_description As String = ""
        Private s_orderNo As Long

        Private s_startID As String = ""
        Private s_finishID As String = ""
        Private s_actStartID As String = ""
        Private s_actFinishID As String = ""

        Private s_takeActIfFCisMissing As Boolean

        Private s_altstartids As String = ""
        Private s_altfinishids As String = ""

        Private s_isMandatory As Boolean
        Private s_isForbidden As Boolean
        Private s_isFacultative As Boolean

        Private s_parameter_txt1 As String = ""
        Private s_parameter_txt2 As String = ""
        Private s_parameter_txt3 As String = ""
        Private s_parameter_num1 As Double
        Private s_parameter_num2 As Double
        Private s_parameter_num3 As Double
        Private s_parameter_date1 As Date = ConstNullDate
        Private s_parameter_date2 As Date = ConstNullDate
        Private s_parameter_date3 As Date = ConstNullDate
        Private s_parameter_flag1 As Boolean
        Private s_parameter_flag2 As Boolean
        Private s_parameter_flag3 As Boolean

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableID)
        End Sub

#Region "Properties"
        ReadOnly Property ScheduleType() As String
            Get
                ScheduleType = s_scheduletype
            End Get

        End Property

        ReadOnly Property ID() As String
            Get
                ID = s_taskid
            End Get

        End Property

        Public Property IsMandatory() As Boolean
            Get
                IsMandatory = s_isMandatory
            End Get
            Set(value As Boolean)

                s_isMandatory = value
                If value Then
                    s_isFacultative = False
                    s_isForbidden = False
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsForbidden() As Boolean
            Get
                IsForbidden = s_isForbidden
            End Get
            Set(value As Boolean)
                s_isForbidden = value
                If value Then
                    s_isFacultative = False
                    s_isMandatory = False
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsFacultative() As Boolean
            Get
                IsFacultative = s_isFacultative
            End Get
            Set(value As Boolean)

                s_isFacultative = value
                If value Then
                    s_isForbidden = False
                    s_isMandatory = False
                End If

                Me.IsChanged = True
            End Set
        End Property

        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                s_description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property StartID() As String
            Get
                StartID = s_startID
            End Get
            Set(value As String)
                s_startID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property FinishID() As String
            Get
                FinishID = s_finishID
            End Get
            Set(value As String)
                s_finishID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property ActstartID() As String
            Get
                ActstartID = s_actStartID
            End Get
            Set(value As String)
                s_actStartID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property ActfinishID() As String
            Get
                ActfinishID = s_actFinishID
            End Get
            Set(value As String)
                s_actFinishID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property AlternativeStartIDs() As String()
            Get
                AlternativeStartIDs = SplitMultiDelims(text:=s_altstartids, DelimChars:=ConstDelimiter)
            End Get
            Set(avalue As String())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = avalue(i)
                        Else
                            aStrValue = aStrValue & ConstDelimiter & avalue(i)
                        End If
                    Next i
                    s_altstartids = aStrValue
                    Me.IsChanged = True
                    'ElseIf Not isEmpty(Trim(avalue)) And Trim(avalue) <> "" And Not isNull(avalue) Then
                    '   s_altstartids = CStr(Trim(avalue))
                Else
                    s_altstartids = ""
                End If
            End Set
        End Property

        Public Property AlternativeFinishIDs() As String()
            Get
                AlternativeFinishIDs = SplitMultiDelims(text:=s_altfinishids, DelimChars:=ConstDelimiter)
            End Get
            Set(avalue As String())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = avalue(i)
                        Else
                            aStrValue = aStrValue & ConstDelimiter & Trim(avalue(i))
                        End If
                    Next i
                    s_altfinishids = aStrValue
                    Me.IsChanged = True
                    'ElseIf Not isEmpty(Trim(avalue)) And Trim(avalue) <> "" And Not isNull(avalue) Then
                    's_altfinishids = CStr(Trim(avalue))
                Else
                    s_altfinishids = ""
                End If

            End Set
        End Property

        Public Property orderno() As Long
            Get
                orderno = s_orderNo
            End Get
            Set(value As Long)
                s_orderNo = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property takeActualIfFCisMissing() As Boolean
            Get
                takeActualIfFCisMissing = s_takeActIfFCisMissing
            End Get
            Set(value As Boolean)
                s_takeActIfFCisMissing = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property parameter_num1() As Double
            Get
                parameter_num1 = s_parameter_num1
            End Get
            Set(value As Double)
                If s_parameter_num1 <> value Then
                    s_parameter_num1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_num2() As Double
            Get
                parameter_num2 = s_parameter_num2
            End Get
            Set(value As Double)
                If s_parameter_num2 <> value Then
                    s_parameter_num2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_num3() As Double
            Get
                parameter_num3 = s_parameter_num3
            End Get
            Set(value As Double)
                If s_parameter_num3 <> value Then
                    s_parameter_num3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date1() As Date
            Get
                parameter_date1 = s_parameter_date1
            End Get
            Set(value As Date)
                If s_parameter_date1 <> value Then
                    s_parameter_date1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date2() As Date
            Get
                parameter_date2 = s_parameter_date2
            End Get
            Set(value As Date)
                If s_parameter_date2 <> value Then
                    s_parameter_date2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_date3() As Date
            Get
                parameter_date3 = s_parameter_date3
            End Get
            Set(value As Date)
                s_parameter_date3 = value
                Me.IsChanged = True
            End Set
        End Property
        Public Property parameter_flag1() As Boolean
            Get
                parameter_flag1 = s_parameter_flag1
            End Get
            Set(value As Boolean)
                If s_parameter_flag1 <> value Then
                    s_parameter_flag1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_flag3() As Boolean
            Get
                parameter_flag3 = s_parameter_flag3
            End Get
            Set(value As Boolean)
                If s_parameter_flag3 <> value Then
                    s_parameter_flag3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_flag2() As Boolean
            Get
                parameter_flag2 = s_parameter_flag2
            End Get
            Set(value As Boolean)
                If s_parameter_flag2 <> value Then
                    s_parameter_flag2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt1() As String
            Get
                parameter_txt1 = s_parameter_txt1
            End Get
            Set(value As String)
                If s_parameter_txt1 <> value Then
                    s_parameter_txt1 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt2() As String
            Get
                parameter_txt2 = s_parameter_txt2
            End Get
            Set(value As String)
                If s_parameter_txt2 <> value Then
                    s_parameter_txt2 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property parameter_txt3() As String
            Get
                parameter_txt3 = s_parameter_txt3
            End Get
            Set(value As String)
                If s_parameter_txt3 <> value Then
                    s_parameter_txt3 = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
#End Region


        ''' <summary>
        ''' Infuse the data object by the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        'Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean

        '    '* init
        '    If Not Me.IsInitialized Then
        '        If Not Me.Initialize() Then
        '            Infuse = False
        '            Exit Function
        '        End If
        '    End If


        '    Try

        '        s_scheduletype = CStr(record.GetValue("scheduletype"))
        '        s_taskid = CStr(record.GetValue("id"))
        '        s_description = CStr(record.GetValue("desc"))
        '        s_orderNo = CLng(record.GetValue("orderno"))
        '        s_takeActIfFCisMissing = CBool(record.GetValue("actoverfc"))
        '        s_isMandatory = CBool(record.GetValue("ismand"))
        '        s_isFacultative = CBool(record.GetValue("isfac"))
        '        s_isForbidden = CBool(record.GetValue("isforb"))

        '        s_altstartids = CStr(record.GetValue("altstartids"))
        '        s_altfinishids = CStr(record.GetValue("altfinishids"))
        '        s_startID = CStr(record.GetValue("startid"))
        '        s_finishID = CStr(record.GetValue("finishid"))
        '        s_actStartID = CStr(record.GetValue("actstartid"))
        '        s_actFinishID = CStr(record.GetValue("actfinishid"))

        '        s_parameter_txt1 = CStr(record.GetValue("param_txt1"))
        '        s_parameter_txt2 = CStr(record.GetValue("param_txt2"))
        '        s_parameter_txt3 = CStr(record.GetValue("param_txt3"))
        '        s_parameter_num1 = CDbl(record.GetValue("param_num1"))
        '        s_parameter_num2 = CDbl(record.GetValue("param_num2"))
        '        s_parameter_num3 = CDbl(record.GetValue("param_num3"))
        '        s_parameter_date1 = CDate(record.GetValue("param_date1"))
        '        s_parameter_date2 = CDate(record.GetValue("param_date2"))
        '        s_parameter_date3 = CDate(record.GetValue("param_date3"))
        '        s_parameter_flag1 = CBool(record.GetValue("param_flag1"))
        '        s_parameter_flag2 = CBool(record.GetValue("param_flag2"))
        '        s_parameter_flag3 = CBool(record.GetValue("param_flag3"))


        '        Return MyBase.Infuse(record)

        '    Catch ex As Exception
        '        Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefSchelueTask.Infuse")
        '        Return False

        '    End Try


        'End Function

        ''' <summary>
        ''' loads and infuses the schedule task definition by primary key
        ''' </summary>
        ''' <param name="scheduletype"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal scheduletype As String, ByVal ID As String) As Boolean
            Dim pkarray() As Object = {LCase(scheduletype), LCase(ID)}
            Return MyBase.Inject(pkarray)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim OrderByColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Tablename = constTableID

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()

            '                'Tablename

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "scheduletype"
            '                aFieldDesc.ID = "bs4"
            '                aFieldDesc.ColumnName = "scheduletype"
            '                aFieldDesc.Size = 50
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                OrderByColumnNames.Add(aFieldDesc.ColumnName)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "task id"
            '                aFieldDesc.ID = "bpt1"
            '                aFieldDesc.ColumnName = "id"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                'Fieldnames
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "description"
            '                aFieldDesc.ID = "bpt2"
            '                aFieldDesc.ColumnName = "desc"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "order no"
            '                aFieldDesc.ID = "bpt3"
            '                aFieldDesc.ColumnName = "orderno"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                OrderByColumnNames.Add("orderno")

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "start milestone id"
            '                aFieldDesc.ID = "bpt4"
            '                aFieldDesc.ColumnName = "startid"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "finish milestone id"
            '                aFieldDesc.ID = "bpt5"
            '                aFieldDesc.ColumnName = "finishid"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "start actual milestone id"
            '                aFieldDesc.ID = "bpt6"
            '                aFieldDesc.ColumnName = "actstartid"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "finish actual milestone id"
            '                aFieldDesc.ID = "bpt7"
            '                aFieldDesc.ColumnName = "actfinishid"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "take actual over forecast milestone id"
            '                aFieldDesc.ID = "bpt8"
            '                aFieldDesc.ColumnName = "actoverfc"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "alternative start milestone ids"
            '                aFieldDesc.ID = "bpt9"
            '                aFieldDesc.ColumnName = "altstartids"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "alternative finish milestone ids"
            '                aFieldDesc.ID = "bpt10"
            '                aFieldDesc.ColumnName = "altfinishids"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is facultative"
            '                aFieldDesc.ID = "bpt11"
            '                aFieldDesc.ColumnName = "isfac"

            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is forbidden"
            '                aFieldDesc.ID = "bpt12"
            '                aFieldDesc.ColumnName = "isforb"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is mandatory"
            '                aFieldDesc.ID = "bpt13"
            '                aFieldDesc.ColumnName = "ismand"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_txt 1
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 1 of condition"
            '                aFieldDesc.ColumnName = "param_txt1"
            '                aFieldDesc.Size = 0
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
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("Orderby", OrderByColumnNames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .CreateObjectSchema()
            '            End With


            '            ' Handle the error
            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="clsOTDBDefScheduleMilestone.createSchema", tablename:=constTableID)
            '            CreateSchema = False
        End Function

        ''' <summary>
        ''' Persist the data object 
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If



            Try
                Call Me.Record.SetValue("scheduletype", s_scheduletype)
                Call Me.Record.SetValue("id", s_taskid)
                Call Me.Record.SetValue("desc", s_description)
                Call Me.Record.SetValue("orderno", s_orderNo)

                Call Me.Record.SetValue("startid", s_startID)
                Call Me.Record.SetValue("finishid", s_finishID)

                Call Me.Record.SetValue("actstartid", s_actStartID)
                Call Me.Record.SetValue("actfinishid", s_actFinishID)

                Call Me.Record.SetValue("altstartids", s_altstartids)
                Call Me.Record.SetValue("altfinishids", s_altfinishids)

                Call Me.Record.SetValue("actoverfc", s_takeActIfFCisMissing)
                Call Me.Record.SetValue("isfac", s_isFacultative)
                Call Me.Record.SetValue("ismand", s_isMandatory)
                Call Me.Record.SetValue("isforb", s_isForbidden)

                Call Me.Record.SetValue("param_txt1", s_parameter_txt1)
                Call Me.Record.SetValue("param_txt2", s_parameter_txt2)
                Call Me.Record.SetValue("param_txt3", s_parameter_txt3)
                Call Me.Record.SetValue("param_date1", s_parameter_date1)
                Call Me.Record.SetValue("param_date2", s_parameter_date2)
                Call Me.Record.SetValue("param_date3", s_parameter_date3)
                Call Me.Record.SetValue("param_num1", s_parameter_num1)
                Call Me.Record.SetValue("param_num2", s_parameter_num2)
                Call Me.Record.SetValue("param_num3", s_parameter_num3)
                Call Me.Record.SetValue("param_flag1", s_parameter_flag1)
                Call Me.Record.SetValue("param_flag2", s_parameter_flag2)
                Call Me.Record.SetValue("param_flag3", s_parameter_flag3)

                Return MyBase.Persist(timestamp)


            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBDefScheduleTask.Persist", exception:=ex)
                Return False

            End Try


        End Function
        ''' <summary>
        ''' retrieve a collection of all schedule task definition object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All() As List(Of clsOTDBDefScheduleTask)
            Return ormDataObject.All(Of clsOTDBDefScheduleTask)()
        End Function

        ''' <summary>
        ''' create the data object with primary key
        ''' </summary>
        ''' <param name="scheduletype"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal scheduletype As String, ByVal ID As String) As Boolean
            Dim pkarray() As Object = {LCase(scheduletype), LCase(ID)}
            If MyBase.Create(pkarray, checkUnique:=True) Then
                ' set the primaryKey
                s_scheduletype = LCase(scheduletype)
                s_taskid = LCase(ID)
                Return Me.IsCreated
            Else
                Return False
            End If

        End Function

    End Class

    ''' <summary>
    ''' Definition of a schedule milestone class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleMilestoneDefinition.ConstObjectID, modulename:=ConstModuleScheduling, _
        Version:=1, description:="declaration of milestones specific in a schedule type", useCache:=True)> _
    Public Class ScheduleMilestoneDefinition
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Public Const ConstObjectID = "ScheduleMilestoneDefinition"

        '** Schema Table
        <ormSchemaTable(version:=2, addDomainBehavior:=True, adddeletefieldbehavior:=True, addSpareFields:=True)> Public Const ConstTableID As String = "tblDefScheduleMilestones"
        '*** Index
        <ormSchemaIndex(columnname1:=ConstFNType, columnname2:=ConstFNOrderNo)> Public Const ConstIndOrder = "orderby"

        '** keys
        <ormObjectEntry(XID:="SCT1", typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=1, aliases:={"bs4"}, title:="schedule type", _
            description:=" type of schedule definition")> Public Const ConstFNType = "scheduletype"
        <ormObjectEntry(XID:="BPD1", typeid:=otFieldDataType.Text, size:=50, primaryKeyordinal:=2, title:="milestone id", _
            description:=" id of milestone in schedule")> Public Const ConstFNID = "id"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** fields
        <ormObjectEntry(XID:="BSD1", typeid:=otFieldDataType.Text, size:=255, _
            title:="description", description:="description of milestone in schedule")> Public Const ConstFNDesc = "desc"
        <ormObjectEntry(XID:="BSD2", typeid:=otFieldDataType.Long, _
           title:="ordinal", description:="ordinal of milestone in schedule")> Public Const ConstFNOrderNo = "orderno"
        <ormObjectEntry(XID:="BSD3", typeid:=otFieldDataType.Text, size:=50, _
            title:="actual of fc milestone id", description:=" actual id of this milestone in schedule")> Public Const ConstFNActualID = "actualid"
        <ormObjectEntry(XID:="BSD4", typeid:=otFieldDataType.Bool, _
        title:="is forecast", description:=" milestone is forecast in schedule")> Public Const ConstFNIsFC = "isfc"
        <ormObjectEntry(XID:="BSD5", typeid:=otFieldDataType.Bool, _
        title:="is facilitative", description:=" milestone is facilitative in schedule")> Public Const ConstFNIsFAC = "isfac"
        <ormObjectEntry(XID:="BSD6", typeid:=otFieldDataType.Bool, _
        title:="is prohibited", description:=" milestone is prohibited in schedule")> Public Const ConstFNIsFORB = "isforb"
        <ormObjectEntry(XID:="BSD7", typeid:=otFieldDataType.Bool, _
        title:="is mandatory", description:=" milestone is mandatory in schedule")> Public Const ConstFNIsMAND = "ismand"
        <ormObjectEntry(XID:="BSD8", typeid:=otFieldDataType.Bool, _
        title:="is input", description:=" milestone is input deliverable in schedule")> Public Const ConstFNIsINPUT = "isinput"
        <ormObjectEntry(XID:="BSD9", typeid:=otFieldDataType.Bool, _
        title:="is output", description:=" milestone is output deliverable in schedule")> Public Const ConstFNIsOutPut = "isoutput"
        <ormObjectEntry(XID:="BSD10", typeid:=otFieldDataType.Bool, _
       title:="is finish", description:=" milestone is end of schedule")> Public Const ConstFNIsFinish = "isfinish"



        '** mapping
        <ormEntryMapping(EntryName:=ConstFNType)> Private _scheduletype As String = ""
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNDesc)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNOrderNo)> Private _orderNo As Long
        <ormEntryMapping(EntryName:=ConstFNIsFC)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNActualID)> Private _actualid As String = ""

        <ormEntryMapping(EntryName:=ConstFNIsMAND)> Private _isMandatory As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsFORB)> Private _isForbidden As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsFAC)> Private _isFacultative As Boolean
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

        ReadOnly Property ScheduleType() As String
            Get
                ScheduleType = _scheduletype
            End Get

        End Property

        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get

        End Property

        Public Property ActualOfFC() As String
            Get
                ActualOfFC = _actualid
            End Get
            Set(value As String)
                _actualid = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsActual() As Boolean
            Get
                IsActual = Not _isForecast
            End Get
            Set(value As Boolean)
                If value Then
                    _isForecast = False
                Else
                    _isForecast = True
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsForecast() As Boolean
            Get
                If _isForecast Then
                    IsForecast = True
                Else
                    IsForecast = False
                End If
            End Get
            Set(value As Boolean)
                If value Then
                    _isForecast = True
                Else
                    _isForecast = False
                End If
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsFinish() As Boolean
            Get
                Return _isFinish
            End Get
            Set(value As Boolean)
                If value Then
                    _isFinish = True
                Else
                    _isFinish = False
                End If
                Me.IsChanged = True
            End Set
        End Property
        Public Property IsMandatory() As Boolean
            Get
                IsMandatory = _isMandatory
            End Get
            Set(value As Boolean)
                _isMandatory = value
                If value Then
                    _isFacultative = False
                    _isForbidden = False
                End If

                Me.IsChanged = True
            End Set
        End Property

        Public Property IsForbidden() As Boolean
            Get
                IsForbidden = _isForbidden
            End Get
            Set(value As Boolean)

                _isForbidden = value
                If value Then
                    _isFacultative = False
                    _isMandatory = False
                End If

                Me.IsChanged = True
            End Set
        End Property

        Public Property IsFacultative() As Boolean
            Get
                IsFacultative = _isFacultative
            End Get
            Set(value As Boolean)
                _isFacultative = value
                If value Then
                    _isForbidden = False
                    _isMandatory = False
                End If

                Me.IsChanged = True
            End Set
        End Property

        Public Property IsOutputDeliverable() As Boolean
            Get
                IsOutputDeliverable = _isOutputDeliverable
            End Get
            Set(value As Boolean)
                _isOutputDeliverable = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsInputDeliverable() As Boolean
            Get
                IsInputDeliverable = _isInputDeliverable
            End Get
            Set(value As Boolean)
                _isInputDeliverable = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Orderno() As Long
            Get
                Orderno = _orderNo
            End Get
            Set(value As Long)
                If value <> _orderNo Then
                    _orderNo = value
                    Me.IsChanged = True
                End If
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
        Public Function GetDefMilestone() As MileStoneDefinition

            If Not IsCreated And Not IsLoaded Then
                Return Nothing
            End If

            Dim adefmilestone As MileStoneDefinition = MileStoneDefinition.Retrieve(id:=Me.ID)
            Return adefmilestone
        End Function

        ''' <summary>
        ''' load the object by primary keys
        ''' </summary>
        ''' <param name="scheduletype"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal scheduletype As String, ByVal ID As String, Optional domainID As String = "") As Boolean
            Dim pkarray() As Object = {LCase(scheduletype), LCase(ID), domainID}
            Return MyBase.Inject(pkarray, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ScheduleMilestoneDefinition)()

            '            '*** OUTDATED CODE
            '            '****
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim OrderByColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Tablename = ConstTableID

            '            With aTable
            '                .Create(ConstTableID)
            '                .Delete()
            '                'Tablename

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "type of schedule"
            '                aFieldDesc.Aliases = New String() {"bs4"}
            '                aFieldDesc.ID = "sct1"
            '                aFieldDesc.ColumnName = "scheduletype"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                OrderByColumnNames.Add(aFieldDesc.ColumnName)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "milestone id"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "bpd1"
            '                aFieldDesc.ColumnName = "id"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                'Fieldnames
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "description"
            '                aFieldDesc.ID = "bsd1"
            '                aFieldDesc.ColumnName = "desc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "order no"
            '                aFieldDesc.ID = "bsd2"
            '                aFieldDesc.ColumnName = "orderno"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                OrderByColumnNames.Add(aFieldDesc.ColumnName)

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "actual of id"
            '                aFieldDesc.ID = "bsd3"
            '                aFieldDesc.ColumnName = "actualid"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is forecast"
            '                aFieldDesc.ID = "bsd4"
            '                aFieldDesc.ColumnName = "isfc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is facultative"
            '                aFieldDesc.ID = "bsd5"
            '                aFieldDesc.ColumnName = "isfac"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is forbidden"
            '                aFieldDesc.ID = "bsd6"
            '                aFieldDesc.ColumnName = "isforb"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is mandatory"
            '                aFieldDesc.ID = "bsd7"
            '                aFieldDesc.ColumnName = "ismand"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is input deliverable"
            '                aFieldDesc.ID = "bsd8"
            '                aFieldDesc.ColumnName = "isinput"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is output deliverable"
            '                aFieldDesc.ID = "bsd9"
            '                aFieldDesc.ColumnName = "isoutput"
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
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("Orderby", OrderByColumnNames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .AlterSchema()
            '            End With


            '            ' Handle the error
            '            CreateSchema = True
            '            Exit Function
            'error_handle:
            '            Call CoreMessageHandler(subname:="clsOTDBDefScheduleMilestone.createSchema", tablename:=ConstTableID)
            '            CreateSchema = False
        End Function

        ''' <summary>
        ''' Persist the Object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Dim aDefMS As MileStoneDefinition
            Dim aCompDesc As New ormCompoundDesc
            Dim aSchemaDefTable As ObjectDefinition = CurrentSession.Objects.GetObject(objectname:=Schedule.ConstTableID)

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If


            Try

                If MyBase.Persist Then

                    '*** create compound for schedules
                    '***
                    If aSchemaDefTable Is Nothing Then
                        aSchemaDefTable.Create(objectID:=Schedule.ConstObjectID)
                    End If

                    aCompDesc.Tablename = Schedule.ConstTableID.ToLower
                    aCompDesc.compound_Tablename = ScheduleMilestone.constTableID.ToLower
                    aCompDesc.ID = _id
                    aCompDesc.compound_Relation = New String() {"uid", "updc"}
                    aCompDesc.compound_IDFieldname = "id"
                    aCompDesc.compound_ValueFieldname = "value"
                    aDefMS = Me.GetDefMilestone
                    If Not aDefMS Is Nothing Then
                        aCompDesc.Datatype = aDefMS.Datatype
                    End If
                    'aCompDesc.Aliases= {}
                    aCompDesc.Parameter = ""
                    aCompDesc.Title = "Milestone " & _id

                    If aSchemaDefTable.AddEntry(aCompDesc) Then
                        aSchemaDefTable.Persist()
                    End If

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
            Return ormDataObject.All(Of ScheduleMilestoneDefinition)()
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
        Public Overloads Function Create(ByVal scheduletype As String, ByVal ID As String, Optional domainID As String = "") As Boolean
            Dim pkarray() As Object = {LCase(scheduletype), LCase(ID), domainID}
            ' set the primaryKey
            Return MyBase.Create(pkarray, domainID:=domainID, checkUnique:=True)
        End Function

    End Class

    ''' <summary>
    ''' schedule definition object
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleDefinition.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, _
        description:="definition of schedules (types)", useCache:=True)> Public Class ScheduleDefinition
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Public Const ConstObjectID = "ScheduleDefinition"

        '*** Schema Tabble
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Const ConstTableID = "tblDefSchedules"

        '*** Keys
        <ormObjectEntry(typeid:=otFieldDataType.Text, title:="unique ID", size:=50, Description:="Unique ID of the schedule type definition", _
            primaryKeyordinal:=1, id:="SCT1", aliases:={"bs4"})> Public Const ConstFNType = "scheduletype"

        '** Fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, title:="description", Description:="description of the schedule definition", _
           id:="SCT2")> Public Const ConstFNDescription = "desc"


        ' fields
        <ormEntryMapping(EntryName:=ConstFNType)> Private _scheduletype As String = ""
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""

        ' components itself per key:=posno, item:=clsOTDBDefScheduleMilestone
        Private _members As New Dictionary(Of String, ScheduleMilestoneDefinition)
        Private _aliases As New Dictionary(Of String, String)

        '

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub
#Region "properties"


        ReadOnly Property ScheduleType()
            Get
                ScheduleType = _scheduletype
            End Get

        End Property
        Public Property description() As String
            Get
                description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        ReadOnly Property isNoSchedule() As Boolean
            Get
                If NoMembers > 0 Then
                    isNoSchedule = False
                Else
                    isNoSchedule = True
                End If
            End Get
        End Property

        ReadOnly Property NoMembers() As Long
            Get
                NoMembers = _members.Count - 1
            End Get
        End Property
#End Region

        Public Function GetMaxOrderNo() As Long
            Dim keys() As Object

            Dim i As Integer
            Dim max As Long

            If NoMembers >= 0 Then
                keys = Me.Orderno
                If IsArrayInitialized(keys) Then
                    For i = LBound(keys) To UBound(keys)
                        If keys(i) > max Then max = keys(i)
                    Next i
                    GetMaxOrderNo = max
                    Exit Function
                End If
            End If
            GetMaxOrderNo = 0
        End Function

        '***** getMileStoneIDByAlias returns the ID on a given AliasID
        '***** blank otherwise
        Public Function GetMilestoneIDByAlias(AliasID As String) As String

            If _aliases.ContainsKey(key:=LCase(AliasID)) Then
                GetMilestoneIDByAlias = _aliases.Item(key:=LCase(AliasID))
                Exit Function
            End If

            GetMilestoneIDByAlias = ""
        End Function
        '*** add a Component by cls OTDB
        '***
        Public Function AddMilestoneByID(anEntryID As String) As Boolean
            Dim flag As Boolean
            Dim existEntry As New ScheduleMilestoneDefinition
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim m As Object
            Dim posno As Long

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddMilestoneByID = False
                Exit Function
            End If

            ' load
            If Not anEntry.Inject(Me.ScheduleType, anEntryID) Then
                AddMilestoneByID = False
                Exit Function
            End If

            ' check Members
            For Each kvp As KeyValuePair(Of String, ScheduleMilestoneDefinition) In _members
                existEntry = kvp.Value
                ' check
                If existEntry.ID.ToLower = LCase(anEntryID) Then
                    AddMilestoneByID = False
                    Exit Function
                End If
            Next

            ' add the component
            AddMilestoneByID = Me.AddMember(anEntry)

        End Function

        '*** add a Component by cls OTDB
        '***
        Public Function AddMember(anEntry As ScheduleMilestoneDefinition) As Boolean
            Dim flag As Boolean
            Dim existEntry As New ScheduleMilestoneDefinition
            Dim aMilestone As New ScheduleMilestone
            Dim anObjectEntry As iObjectEntry
            Dim aSchedule As New Schedule
            Dim m As Object

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddMember = False
                Exit Function
            End If

            ' remove and overwrite
            If _members.ContainsKey(key:=anEntry.ID) Then
                Call _members.Remove(key:=anEntry.ID)
            End If
            ' load aliases
            anObjectEntry = ObjectDefinition.Retrieve(objectname:=ConstObjectID).GetEntry(anEntry.ID)
            If anObjectEntry IsNot Nothing Then
                For Each m In anObjectEntry.Aliases
                    If _aliases.ContainsKey(key:=LCase(m)) Then
                        Call _aliases.Remove(key:=LCase(m))
                    End If
                    Call _aliases.Add(key:=LCase(m), value:=anEntry.ID)
                Next m
            End If
            ' add entry
            _members.Add(key:=anEntry.ID, value:=anEntry)

            '
            AddMember = True

        End Function
        ''' <summary>
        ''' initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean

            _members.Clear()
            _aliases.Clear()
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' delete the data object and all members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Delete() As Boolean
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim initialEntry As New ScheduleMilestoneDefinition
            Dim m As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each aMilestoneDefinition In _members.Values
                aMilestoneDefinition.Delete()
            Next

            ' reset it
            _members = New Dictionary(Of String, ScheduleMilestoneDefinition)
            If Not anEntry.Create(scheduletype:=Me.ScheduleType, ID:="") Then
                Call anEntry.Inject(scheduletype:=Me.ScheduleType, ID:="")
            End If
            _members.Add(key:=anEntry.ID, value:=anEntry)

            _IsCreated = True
            Me.IsDeleted = True
            Me.Unload()

        End Function

        '**** orderno returns an object array of orderno's
        '****
        ''' <summary>
        ''' orderno returns an object array of orderno's
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Orderno() As Object()
            Dim orders() As Object
            Dim anEntry As ScheduleMilestoneDefinition
            Dim i As Integer
            Dim m As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                Orderno = orders
                Exit Function
            End If

            ' get each entry
            i = 0
            ' delete each entry
            For Each kvp As KeyValuePair(Of String, ScheduleMilestoneDefinition) In _members
                ReDim Preserve orders(i)
                anEntry = kvp.Value
                orders(i) = anEntry.Orderno
                i += 1
            Next


            Orderno = orders
        End Function
        '**** Members returns a Collection of Members (Milestone Definitions) in Order of the OrderNo
        '****
        ''' <summary>
        '''  Members returns a Collection of Members (Milestone Definitions) in Order of the OrderNo
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MembersByOrderNo() As Collection
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim aCollection As New Collection
            Dim m As Object
            Dim order() As Object


            If Not Me.IsCreated And Not _IsLoaded Then
                MembersByOrderNo = Nothing
                Exit Function
            End If

            order = Me.Orderno
            If IsArrayInitialized(order) Then
                Array.Sort(order)
                Assert(False)

                'Call modQSortInPlace.QSortInPlace(order, CompareMode:=vbDatabaseCompare)
                For Each m In order
                    anEntry = _members.Item(key:=m)
                    If anEntry.ID <> "" Then aCollection.Add(anEntry)
                Next m
            Else
                MembersByOrderNo = Nothing
                Exit Function
            End If

            MembersByOrderNo = aCollection
        End Function

        '**** Members returns a Collection of Members
        '****
        ''' <summary>
        '''  returns a Collection of Members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Members() As Collection
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim aCollection As New Collection
            Dim m As Object
            Dim i As Integer

            If Not Me.IsCreated And Not _IsLoaded Then
                Members = Nothing
                Exit Function
            End If

            ' delete each entry
            For Each kvp As KeyValuePair(Of String, ScheduleMilestoneDefinition) In _members
                anEntry = kvp.Value
                If anEntry.ID <> "" Then aCollection.Add(anEntry)
            Next


            Members = aCollection
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
        ''' 
        ''' </summary>
        ''' <param name="scheduletype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal scheduletype As String, Optional domainID As String = "") As Boolean
            Dim aStore As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aRecord As ormRecord
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim pkarray() As String = {LCase(scheduletype)}
            Try
                If MyBase.Inject(pkarray) Then

                    '*** load all milestones
                    aStore = GetTableStore(ScheduleMilestoneDefinition.ConstTableID)
                    Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="Inject", addAllFields:=True)
                    If Not aCommand.Prepared Then
                        aCommand.Where = ScheduleMilestoneDefinition.ConstTableID & ".[" & ScheduleMilestoneDefinition.ConstFNType & "] = @type"
                        aCommand.OrderBy = "[" & ScheduleMilestoneDefinition.ConstFNOrderNo & "] asc"
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@type", columnname:=ScheduleMilestoneDefinition.ConstFNType, _
                                                                         tablename:=ScheduleMilestoneDefinition.ConstTableID))
                        aCommand.Prepare()
                    End If
                    aCommand.SetParameterValue(ID:="@type", value:=scheduletype)

                    aRecordCollection = aCommand.RunSelect
                    _scheduletype = scheduletype

                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component
                        anEntry = New ScheduleMilestoneDefinition
                        If InfuseDataObject(record:=aRecord, dataobject:=anEntry) Then
                            If Not Me.AddMember(anEntry) Then
                            End If
                        End If
                    Next aRecord
                    'else

                    Return Me.IsLoaded
                Else
                    Unload()
                    Return Me.IsLoaded
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ScheduleDefinition.Inject", exception:=ex)
                Return False
            End Try


        End Function
        ''' <summary>
        ''' Persist the data object and all loaded definition memebrs
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            If timestamp = ConstNullDate Then timestamp = Date.Now
            Try
                If MyBase.Persist(timestamp) Then
                    Persist = True
                    ' persist each entry
                    If _members.Count > 0 Then
                        For Each anEntry In _members.Values
                            Persist = Persist And anEntry.Persist(timestamp)
                        Next
                    End If
                    Return Persist
                End If
                Return False

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ScheduleDefinition.Persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ScheduleDefinition)(silent:=silent)

            '            ''' OOUTDATED CODE

            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition

            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Size = 0
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Tablename = ConstTableID

            '            With aTable
            '                .Create(ConstTableID)
            '                .Delete()

            '                'Tablename

            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "scheduletype"
            '                aFieldDesc.Aliases = New String() {"bs4"}
            '                aFieldDesc.ID = "SCT1"
            '                aFieldDesc.ColumnName = "scheduletype"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)


            '                'Fieldnames
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "description"
            '                aFieldDesc.ID = "SCT2"
            '                aFieldDesc.ColumnName = "desc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .AlterSchema()
            '            End With


            '            ' Handle the error
            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="ScheduleDefinition.createSchema", tablename:=ConstTableID)
            '            CreateSchema = False
        End Function
        ''' <summary>
        ''' create the data object by primary key
        ''' </summary>
        ''' <param name="SCHEDULETYPE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal scheduletype As String, Optional domainid As String = "") As Boolean
            Dim anEntry As New ScheduleMilestoneDefinition
            Dim pkarray() As String = {LCase(scheduletype), domainid}
            If IsLoaded Then
                Create = False
                Exit Function
            End If

            ' set the primaryKey
            If MyBase.Create(pkarray, domainID:=domainid, checkUnique:=False) Then
                _scheduletype = LCase(scheduletype)
                _members = New Dictionary(Of String, ScheduleMilestoneDefinition)
                ' abort create if exists
                If Not anEntry.Create(scheduletype:=scheduletype, ID:="", domainID:=domainid) Then
                    Return False
                End If
                _members.Add(key:=0, value:=anEntry)

                Return Me.IsCreated
            End If


        End Function

    End Class

    ''' <summary>
    ''' schedule class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(Version:=1, ID:=Schedule.ConstObjectID, modulename:=constModuleScheduling, Title:="Schedule", Description:="schedules for business objects")> _
    Public Class Schedule
        Inherits ormDataObject
        Implements iotXChangeable
        Implements iormInfusable
        Implements iormPersistable
        Implements iotHasCompounds
        Implements iotCloneable(Of Schedule)

        Public Const ConstObjectID = "Schedule"

        '** Schema Table
        <ormSchemaTableAttribute(Version:=2, addDomainBehavior:=False, AddDeleteFieldBehavior:=True, addsparefields:=True)> _
        Public Const ConstTableID = "tblschedules"
        '** Indexes
        <ormSchemaIndexAttribute(columnname1:=ConstFNWorkspace, columnname2:=ConstFNUid, columnname3:=ConstFNUpdc)> Public Const ConstIndexWS = "workspaceID"
        <ormSchemaIndexAttribute(columnname1:=ConstFNUid)> Public Const ConstIndexUID = "uidIndex"

        '*** Columns
        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="unique ID", Description:="Unique ID of the schedule", _
            primaryKeyordinal:=1, XID:="SC2", aliases:={"SUID"})> Public Const ConstFNUid = "uid"
        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="update count", Description:="Update count of the schedule", _
           primaryKeyordinal:=2, XID:="SC3", aliases:={"BS3"})> Public Const ConstFNUpdc = "updc"


        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="forecast update count", Description:="forecast update count of the schedule" _
          )> Public Const ConstFNfcupdc = "fcupdc"

        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
            Description:="workspaceID ID of the schedule", useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
             foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> Public Const ConstFNWorkspace = Workspace.ConstFNID

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="revision", Description:="revision of the schedule", _
            XID:="SC5", aliases:={"BS2"}, Defaultvalue:="")> Public Const ConstFNPlanRev = "plrev"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, title:="is frozen", Description:="schedule is frozen flag", _
            XID:="SC6", aliases:={}, Defaultvalue:="false")> Public Const ConstFNisfrozen = "isfrozen"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="lifecycle status", Description:="lifecycle status of the schedule", _
            XID:="SC7", aliases:={"BS1"}, Defaultvalue:="", parameter:="LCStatus")> Public Const ConstFNlcstatus = "lcstatus"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="process status", Description:="process status of the schedule", _
            XID:="SC8", aliases:={"S1"}, Defaultvalue:="", parameter:="PStatus")> Public Const ConstFNpstatus = "pstatus"
        <ormObjectEntry(typeid:=otFieldDataType.Timestamp, title:="check timestamp", Description:="timestamp of check status of the schedule", _
            XID:="SC9", aliases:={}, Defaultvalue:="", parameter:="")> Public Const ConstFNCheckedOn = "checkedon"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, title:="planner", Description:="responsible planner of the schedule", _
            XID:="SC10", aliases:={}, Defaultvalue:="", parameter:="")> Public Const ConstFNPlanner = "resp"
        <ormObjectEntry(typeid:=otFieldDataType.Memo, title:="comment", Description:="comment of the schedule", _
            XID:="SC12", aliases:={}, Defaultvalue:="", parameter:="")> Public Const ConstFNComment = "cmt"
        <ormObjectEntry(typeid:=otFieldDataType.Timestamp, title:="last fc update", Description:="last forecast change of the schedule", _
            XID:="SC13", aliases:={}, parameter:="")> Public Const ConstFNFCupdatedOn = "fcupdon"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, title:="type", Description:="type of the schedule", _
            XID:="SC14", aliases:={"BS4"}, Defaultvalue:="")> Public Const ConstFNTypeid = "typeid"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, title:="baseline flag", Description:="flag if the schedule is a baseline", _
            XID:="SC15", aliases:={})> Public Const ConstFNIsBaseline = "isbaseline"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="baseline date", Description:="date of the baseline creation", _
            XID:="SC16", aliases:={})> Public Const ConstFNBlDate = "bldate"
        <ormObjectEntry(typeid:=otFieldDataType.Long, title:="baseline updc", Description:="updc of the last baseline of this schedule", _
            XID:="SC17", aliases:={}, Defaultvalue:="0")> Public Const ConstFNBlUpdc = "blupdc"

        <ormObjectEntry(typeid:=otFieldDataType.Numeric, title:="required capacity", Description:="required capacity of this schedule", _
            XID:="SC20", aliases:={"WBS2"}, Defaultvalue:="0")> Public Const ConstFNRequCap = "requ"
        <ormObjectEntry(typeid:=otFieldDataType.Numeric, title:="used capacity", Description:="used capacity of this schedule", _
            XID:="SC21", aliases:={"WBS3"}, Defaultvalue:="0")> Public Const ConstFNUsedCap = "used"
        <ormObjectEntry(typeid:=otFieldDataType.Date, title:="used capacity reference date", Description:="used capacity reference date of this schedule", _
            XID:="SC22", aliases:={"WBS4"})> Public Const ConstFNUsedCapRef = "ufdt"

        <ormObjectEntry(referenceObjectEntry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=100, title:="Activitiy Tag", Description:="Activity Tag", _
           XID:="SC30", aliases:={}, Defaultvalue:="", parameter:="")> Public Const ConstFNActTag = "acttag"

        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long = 0
        <ormEntryMapping(EntryName:=ConstFNUpdc)> Private _updc As Long = 0
        <ormEntryMapping(EntryName:=ConstFNfcupdc)> Private _fcupdc As Long    ' update count of just fc
        <ormEntryMapping(EntryName:=ConstFNPlanRev)> Private _plrev As String = ""
        <ormEntryMapping(EntryName:=ConstFNPlanner)> Private _planner As String = ""
        <ormEntryMapping(EntryName:=ConstFNisfrozen)> Private _isFrozen As Boolean
        <ormEntryMapping(EntryName:=ConstFNpstatus)> Private _pstatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNlcstatus)> Private _lfcstatus As String = ""
        <ormEntryMapping(EntryName:=ConstFNCheckedOn)> Private _checkedOn As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNFCupdatedOn)> Private _fcUpdatedOn As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNIsBaseline)> Private _isBaseline As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNBlDate)> Private _baselineDate As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNBlUpdc)> Private _baselineUPDC As Long = 0

        <ormEntryMapping(EntryName:=ConstFNWorkspace)> Private _workspace As String = ""
        <ormEntryMapping(EntryName:=ConstFNTypeid)> Private _typeid As String = ""
        <ormEntryMapping(EntryName:=ConstFNRequCap)> Private _requ As Double = 0
        <ormEntryMapping(EntryName:=ConstFNUsedCap)> Private _used As Double = 0
        <ormEntryMapping(EntryName:=ConstFNUsedCapRef)> Private _ufdt As Date = ConstNullDate
        <ormEntryMapping(EntryName:=ConstFNComment)> Private _comment As String = ""
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Private _msglogtag As String = ""
        <ormEntryMapping(EntryName:=ConstFNActTag)> Private _activetag As String = ""

        ' components itself per key:=id, item:=clsOTDBXScheduleMilestone
        Private s_members As New Dictionary(Of String, ScheduleMilestone)
        Private s_orgMSvalues As New Dictionary(Of String, Object)   'orgmembers -> original members before any change

        ' dynamic
        Private s_haveMilestonesChanged As Boolean
        Private s_isForeCastChanged As Boolean
        'Private s_milestones As New Dictionary -> superseded with members
        Private s_loadedFromHost As Boolean
        Private s_savedToHost As Boolean
        Private s_defschedule As New ScheduleDefinition

        Private s_msglog As New ObjectLog

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub

#Region "Properties"


        ReadOnly Property Uid() As Long
            Get
                Uid = _uid
            End Get

        End Property
        Public Property Comment() As String
            Get
                Comment = _comment
            End Get
            Set(value As String)
                _comment = value
                Me.IsChanged = True
            End Set
        End Property
        Public Property workspaceID() As String
            Get
                workspaceID = _workspace

            End Get
            Set(value As String)
                Dim m As Object
                Dim aMember As ScheduleMilestone

                If LCase(_workspace) <> LCase(value) Then
                    _workspace = value
                    Me.IsChanged = True
                    ' change all the members
                    For Each kvp As KeyValuePair(Of String, ScheduleMilestone) In s_members
                        aMember = kvp.Value
                        aMember.workspaceID = value
                    Next
                End If
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

                If s_members Is Nothing Then
                    NoMilestones = 0
                    Exit Property
                End If
                ' No of Components
                NoMilestones = s_members.Count
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
                IsForecastChanged = s_isForeCastChanged
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
                        s_defschedule = defschedule
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

        Public Property LoadedFromHost() As Boolean
            Get
                LoadedFromHost = s_loadedFromHost
            End Get
            Set(value As Boolean)
                s_loadedFromHost = value
            End Set
        End Property

        Public Property RequiredCapacity() As Double
            Get
                RequiredCapacity = _requ
            End Get
            Set(value As Double)
                If _requ <> value Then
                    _requ = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property UsedCapacity() As Double
            Get
                UsedCapacity = _used
            End Get
            Set(value As Double)
                If _used <> value Then
                    _used = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property UsedCapacityRefDate() As Date
            Get
                UsedCapacityRefDate = _ufdt
            End Get
            Set(value As Date)
                If value <> _ufdt Then
                    _ufdt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Revision() As String
            Get
                Revision = _plrev
            End Get
            Set(value As String)
                If _plrev <> value Then
                    _plrev = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Planner() As String
            Get
                Planner = _planner
            End Get
            Set(value As String)
                If _planner <> value Then
                    _planner = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ProcessStatus() As String
            Get
                ProcessStatus = _pstatus
            End Get
            Set(value As String)
                If _pstatus <> value Then
                    _pstatus = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property LFCStatus() As String
            Get
                LFCStatus = _pstatus
            End Get
            Set(value As String)
                If _lfcstatus <> value Then
                    _lfcstatus = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsFrozen() As Boolean
            Get
                IsFrozen = _isFrozen
            End Get
            Set(value As Boolean)
                If _isFrozen <> value Then
                    _isFrozen = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsBaseline() As Boolean
            Get
                IsBaseline = _isBaseline
            End Get
            Set(value As Boolean)
                If _isBaseline <> value Then
                    _isBaseline = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property StatusCheckedOn() As Date
            Get
                StatusCheckedOn = _checkedOn
            End Get
            Set(value As Date)
                If _checkedOn <> value Then
                    _checkedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property BaselineRefDate() As Date
            Get
                BaselineRefDate = _baselineDate
            End Get
            Set(value As Date)
                If _baselineDate <> value Then
                    _baselineDate = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property BaselineUPDC() As Long
            Get
                BaselineUPDC = _baselineUPDC
            End Get
            Set(value As Long)
                If _baselineUPDC <> value Then
                    _baselineUPDC = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property LastForecastUpdate() As Date
            Get
                LastForecastUpdate = _fcUpdatedOn
            End Get
            Set(value As Date)
                If _fcUpdatedOn <> value Then
                    _fcUpdatedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ReadOnly Property FCupdc() As Long
            Get
                FCupdc = _fcupdc
            End Get

        End Property
        ReadOnly Property Updc() As Long
            Get
                Updc = _updc
            End Get

        End Property

        ReadOnly Property Msglogtag() As String
            Get
                If _msglogtag = "" Then
                    _msglogtag = getUniqueTag()
                End If
                Msglogtag = _msglogtag
            End Get
        End Property
        ReadOnly Property Activetag() As String
            Get
                If _activetag = "" Then
                    _activetag = getUniqueTag()
                End If
                Activetag = _activetag

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
                Return s_haveMilestonesChanged
            End Get
        End Property
#End Region

        ''' <summary>
        ''' retrieve the related Schedule Definition object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDefSchedule() As ScheduleDefinition
            If s_defschedule Is Nothing Then
                s_defschedule = New ScheduleDefinition
            End If

            If Not s_defschedule.IsLoaded And Not s_defschedule.IsCreated Then
                s_defschedule = ScheduleDefinition.Retrieve(scheduletype:=_typeid)
                If s_defschedule Is Nothing Then
                    Call CoreMessageHandler(message:="schedule defintion doesn't exist", subname:="Schedule.defSchedule", _
                                          arg1:=_typeid)
                    s_defschedule = New ScheduleDefinition
                End If
            End If
            Return s_defschedule
        End Function
        ''' <summary>
        ''' retrieve the related Schedule Milestone Definition Object
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDefScheduleMilestone(ByVal ID As String) As ScheduleMilestoneDefinition
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


        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            Initialize = MyBase.Initialize
            s_members = New Dictionary(Of String, ScheduleMilestone)
            s_orgMSvalues = New Dictionary(Of String, Object)
            _workspace = CurrentSession.CurrentWorkspaceID
            s_haveMilestonesChanged = False
            _ufdt = ConstNullDate
            _checkedOn = ConstNullDate
            _fcUpdatedOn = ConstNullDate
            _baselineDate = ConstNullDate
            _ufdt = ConstNullDate
            s_isForeCastChanged = False
            SerializeWithHostApplication = isDefaultSerializeAtHostApplication(ConstTableID)
            s_defschedule = New ScheduleDefinition
            's_parameter_date1 = ot.ConstNullDate
            's_parameter_date2 = ot.ConstNullDate
            's_parameter_date3 = ot.ConstNullDate

        End Function

        '******* milestone returns the Milestone ID as object or Null if not exists
        '*******
        ''' <summary>
        ''' milestone returns the Milestone Value as object or Null if not exists
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="ORIGINAL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMilestoneValue(ByVal ID As String, Optional ORIGINAL As Boolean = False) As Object
            Dim aMember As New ScheduleMilestone
            Dim aDefSchedule As ScheduleDefinition = Me.GetDefSchedule
            Dim aRealID As String

            If Not IsCreated And Not IsLoaded Then
                Return Nothing
            End If

            ' check aliases
            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return Nothing
            End If

            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If

            ' return not original

            If s_members.ContainsKey(key:=LCase(aRealID)) Then
                aMember = s_members.Item(key:=LCase(aRealID))
                If Not ORIGINAL Then
                    Return aMember.Value
                ElseIf s_orgMSvalues.ContainsKey(LCase(aRealID)) Then
                    Return s_orgMSvalues.Item(LCase(aRealID))
                Else
                    Return Nothing
                End If

            Else
                Return Nothing
            End If


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
            Dim aDefSchedule As ScheduleDefinition = Me.GetDefSchedule
            Dim aRealID As String

            If Not IsCreated And Not IsLoaded Then
                Return Nothing
            End If

            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return Nothing
            End If

            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If

            ' return not original

            If s_members.ContainsKey(key:=LCase(aRealID)) Then
                aMember = s_members.Item(key:=LCase(aRealID))
                Return aMember
            Else
                Return Nothing
            End If

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
            Dim aDefSchedule As ScheduleDefinition = Me.GetDefSchedule
            Dim aRealID As String

            If Not IsCreated And Not IsLoaded Then
                SetMilestone = False
                Exit Function
            End If

            ' check aliases
            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                SetMilestone = False
                Exit Function
            End If
            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If

            ' return
            If s_members.ContainsKey(LCase(aRealID)) Then
                aMember = s_members.Item(LCase(aRealID))
            Else
                Call CoreMessageHandler(arg1:=ID, subname:="Schedule.setMilestone", tablename:=ConstTableID, _
                                      message:="ID doesnot exist in Milestone Entries")
                SetMilestone = False
                Exit Function
            End If

            isMemberchanged = False


            ' if the Member is only a Cache ?!
            If aMember.IsCacheNoSave Then
                Call CoreMessageHandler(message:="setMilestone to cached Item", _
                                      subname:="Schedule.setMilestone", _
                                      arg1:=LCase(ID) & ":" & CStr(Value))
                SetMilestone = False
                Exit Function
            End If

            ' convert it
            If (aMember.Datatype = otFieldDataType.[Date] Or aMember.Datatype = otFieldDataType.Timestamp) Then
                If IsDate(Value) And Not setNull Then
                    If aMember.Value <> CDate(Value) Then
                        aMember.Value = CDate(Value)
                        isMemberchanged = True
                    End If
                ElseIf setNull Then
                    If aMember.Value <> ConstNullDate Then
                        aMember.Value = ConstNullDate
                        isMemberchanged = True
                    End If
                Else
                    Call CoreMessageHandler(message:="milestone of date cannot set to", subname:="Schedule.setMilestone", _
                                                         arg1:=LCase(ID) & ":" & CStr(Value), messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

            ElseIf aMember.Datatype = otFieldDataType.Numeric Then
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

            ElseIf aMember.Datatype = otFieldDataType.[Long] Then
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

            ElseIf aMember.Datatype = otFieldDataType.Bool Then
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
                s_haveMilestonesChanged = True
                If aMember.IsForecast Then
                    s_isForeCastChanged = True
                End If
                SetMilestone = True
                Exit Function
            Else
                SetMilestone = True
                Exit Function
            End If


            '
            SetMilestone = False

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
            Dim aScheduleMSDefColl As New Collection
            Dim aCE As New CalendarEntry
            Dim flag As Boolean
            Dim aDate As Object
            Dim actDate As Object

            If Not _IsLoaded And Not Me.IsCreated Then
                MoveMilestone = False
                Exit Function
            End If

            If Me.IsFinished Then
                MoveMilestone = False
                Exit Function
            End If

            If Not aScheduleMSDef.Inject(Me.Typeid, MSID) Then
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
            If aDate <> ConstNullDate And IsDate(aDate) And actDate = ConstNullDate And IsDate(actDate) And aScheduleMSDef.ActualOfFC <> "" And aScheduleMSDef.ID <> "" Then
                ' only if there is this milestone
                aCE.Timestamp = aDate
                aDate = aCE.addDay(noDays, considerAvailibilty:=considerWorkingDays, calendarname:=CurrentSession.DefaultCalendarName)
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
            Dim aScheduleMSDefColl As New Collection
            Dim aScheduleMSDef As New ScheduleMilestoneDefinition
            Dim aCE As New CalendarEntry
            Dim started As Boolean
            Dim aDate As Object
            Dim actDate As Object

            If Not _IsLoaded And Not Me.IsCreated Then
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
                    If aDate <> ConstNullDate And IsDate(aDate) And _
                    actDate = ConstNullDate And IsDate(actDate) And aScheduleMSDef.ActualOfFC <> "" And aScheduleMSDef.ID <> "" Then
                        ' only if there is this milestone
                        aCE.Timestamp = aDate
                        aDate = aCE.addDay(noDays, considerAvailibilty:=considerWorkingDays, calendarname:=CurrentSession.DefaultCalendarName)
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
        Public Function GetDefScheduleMSbyOrder(Optional justDates As Boolean = True) As Collection
            Dim aScheduleDef As New ScheduleDefinition
            Dim atypeid As String
            Dim aDeliverableTrack As New Track
            Dim aCollection As New Collection
            Dim aMSDefCollection As New Collection
            Dim aScheduleMSDef As New ScheduleMilestoneDefinition
            Dim aMilestoneDef As New MileStoneDefinition

            If Not _IsLoaded And Not Me.IsCreated Then
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
                aMSDefCollection = aScheduleDef.Members    ' should be in the order
                If aMSDefCollection Is Nothing Or aMSDefCollection.Count = 0 Then
                    GetDefScheduleMSbyOrder = Nothing
                    Exit Function
                Else
                    aCollection = New Collection
                End If
                ' go through
                For Each aScheduleMSDef In aMSDefCollection
                    If aMilestoneDef.Inject(aScheduleMSDef.ID) Then
                        If (aMilestoneDef.Datatype = otMilestoneType.Status And Not justDates) Or justDates Then
                            Call aCollection.Add(Item:=aScheduleMSDef)
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
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of Schedule)()

            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim WorkspaceColumnNames As New Collection
            '            Dim uidcolumnnames As New Collection
            '            Dim aTable As New ObjectDefinition
            '            Dim aTableEntry As New ObjectEntryDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Relation = New String() {}
            '            aFieldDesc.Aliases = New String() {}
            '            aFieldDesc.Tablename = ConstTableID

            '            ' delete just fields -> keep compounds
            '            If aTable.Inject(ConstTableID) Then
            '                For Each aTableEntry In aTable.Entries
            '                    If aTableEntry.Typeid = otObjectEntryDefinitiontype.Field Then
            '                        aTableEntry.Delete()
            '                    End If
            '                Next aTableEntry
            '                aTable.Persist()
            '            End If
            '            aTable = New ObjectDefinition
            '            aTable.Create(ConstTableID)

            '            '******
            '            '****** Fields

            '            With aTable

            '                '*** workspaceID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workspaceID"
            '                aFieldDesc.ID = "ws"
            '                aFieldDesc.ColumnName = ConstFNWorkspace
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '**** UID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "uid of deliverable"
            '                aFieldDesc.ID = "SC2"
            '                aFieldDesc.Aliases = New String() {ConstFNUid}
            '                aFieldDesc.ColumnName = ConstFNUid
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)
            '                uidcolumnnames.Add(aFieldDesc.ColumnName)

            '                '***** updc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of schedule"
            '                aFieldDesc.ID = "SC3"
            '                aFieldDesc.Aliases = New String() {"bs3"}
            '                aFieldDesc.ColumnName = ConstFNUpdc
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                WorkspaceColumnNames.Add(aFieldDesc.ColumnName)

            '                '***** fcupdc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of forecast"
            '                aFieldDesc.ID = "SC4"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = ConstFNfcupdc
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** planning revision
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "deliverable revision"
            '                aFieldDesc.Aliases = New String() {"bs2"}
            '                aFieldDesc.ID = "SC5"
            '                aFieldDesc.ColumnName = "plrev"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** is frozen
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "schedule frozen"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC6"
            '                aFieldDesc.ColumnName = "isfrozen"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** lifecyclestatus
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "schedule lifecycle status"
            '                aFieldDesc.Aliases = New String() {"bs1"}
            '                aFieldDesc.ID = "SC7"
            '                aFieldDesc.ColumnName = "lcstatus"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** process status
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "schedule process status"
            '                aFieldDesc.Aliases = New String() {"s1"}
            '                aFieldDesc.ID = "SC8"
            '                aFieldDesc.ColumnName = "pstatus"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** checked date
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "schedule checked date"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC9"
            '                aFieldDesc.ColumnName = "checkedon"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** planner
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "responsible planner"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC10"
            '                aFieldDesc.ColumnName = "resp"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** msglogtag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "message log tag"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC11"
            '                aFieldDesc.ColumnName = "msglogtag"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '**** comment
            '                aFieldDesc.Datatype = otFieldDataType.Memo
            '                aFieldDesc.Title = "comment"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC12"
            '                aFieldDesc.ColumnName = "cmt"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** last fc update
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last forecast updated on"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC13"
            '                aFieldDesc.ColumnName = "fcupdon"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** scedule type
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "schedule type"
            '                aFieldDesc.Aliases = New String() {"bs4"}
            '                aFieldDesc.ID = "SC14"
            '                aFieldDesc.ColumnName = ConstFNTypeid
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                '**** is baseline
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "schedule is baseline"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC15"
            '                aFieldDesc.ColumnName = "isbaseline"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                '**** baseline date
            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "baseline date"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC16"
            '                aFieldDesc.ColumnName = "bldate"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                '**** baseline updc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "baseline updc"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "SC17"
            '                aFieldDesc.ColumnName = "blupdc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** required capacity
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "required capacity"
            '                aFieldDesc.ID = "SC20"
            '                aFieldDesc.Aliases = New String() {"wbs2"}
            '                aFieldDesc.ColumnName = ConstFNRequCap
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "used capacity"
            '                aFieldDesc.ID = "SC21"
            '                aFieldDesc.Aliases = New String() {"wbs3"}
            '                aFieldDesc.ColumnName = ConstFNUsedCap
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.[Date]
            '                aFieldDesc.Title = "used capacity date"
            '                aFieldDesc.ID = "SC22"
            '                aFieldDesc.Aliases = New String() {"wbs4"}
            '                aFieldDesc.ColumnName = "ufdt"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** activeTag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "tag of activitiy"
            '                aFieldDesc.ID = "sc30"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ColumnName = "acttag"
            '                aFieldDesc.Size = 100
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Aliases = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Aliases = New String() {}
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '                Call .AddIndex("workspaceID", WorkspaceColumnNames, isprimarykey:=False)
            '                Call .AddIndex("uidIndex", uidcolumnnames, isprimarykey:=False)

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
            '            Call CoreMessageHandler(subname:="Schedule.createSchema", tablename:=ConstTableID)
            '            CreateSchema = False
        End Function

        '***** loadMilestones -> load all Milestones as Members -> look for Actuals access
        '*****
        ''' <summary>
        ''' load all Milestones as Members -> look for Actuals access
        ''' </summary>
        ''' <param name="scheduletypeid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadMilestones(ByVal scheduletypeid As String) As Boolean
            Dim aTable As iormDataStore
            Dim CurrenWorkspace As Workspace = Workspace.Retrieve(Me.workspaceID)
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim updc As Long
            Dim isCache As Boolean
            Dim aWSID As String

            aTable = GetTableStore(ConstTableID)
            Dim aCollection As List(Of ScheduleMilestoneDefinition) = ScheduleMilestoneDefinition.AllByType(scheduletypeid)

            For Each aScheduleMSDef In aCollection
                ' load workspaceID

                ' define the Member
                Dim aMSDef As MileStoneDefinition = MileStoneDefinition.Retrieve(aScheduleMSDef.ID)

                If Not aScheduleMSDef.IsForbidden AndAlso Not aMSDef Is Nothing Then
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
                                    If aCurrSCHEDULE.LoadUniqueBy(UID:=_uid, workspaceID:=aWSID) Then
                                        updc = aCurrSCHEDULE.UPDC
                                        Exit For
                                    End If
                                End If
                            End If
                        Next
                    Else
                        updc = _updc
                        isCache = False
                    End If    ' actuals

                    '** load the milestone
                    Dim aMilestone As New ScheduleMilestone
                    If aMilestone.Inject(UID:=_uid, updc:=updc, ID:=aScheduleMSDef.ID) Then
                        ' iscache must be kept
                        aMilestone.IsCacheNoSave = isCache
                        '** include
                        Call AddMilestone(milestone:=aMilestone)
                    Else
                        CoreMessageHandler(message:="Milestone for uid #" & _uid & " from definition '" & aScheduleMSDef.ScheduleType & "' is missing", arg1:=aScheduleMSDef.ID, tablename:=ConstTableID, _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    End If
                Else
                    'Debug.Assert False
                End If

            Next aScheduleMSDef

            LoadMilestones = True
        End Function
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
                        s_loadedFromHost = True
                    End If
                End If

                s_haveMilestonesChanged = False

                '*** fill the Milestone Dictionary
                If Not LoadMilestones(scheduletypeid:=_typeid) Then
                    e.Proceed = False

                End If

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Schedule.Infuse")
            End Try

        End Sub
        ''' <summary>
        ''' delete the record and all members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Delete() As Boolean
            If IsLoaded Then
                ' delete each entry
                For Each anEntry As ScheduleMilestone In s_members.Values
                    anEntry.Delete()
                Next
                MyBase.Delete()
                If Me.IsDeleted Then
                    Me.Unload()
                End If
                Return Me.IsDeleted
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns all schedule milestones
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Milestones() As List(Of ScheduleMilestone)
            Return s_members.Values.ToList
        End Function
        '*** add a Component by cls OTDB
        '***
        ''' <summary>
        ''' Add a Milestone
        ''' </summary>
        ''' <param name="Milestone"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddMilestone(ByRef milestone As ScheduleMilestone) As Boolean

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                Return False
            End If

            ' remove and overwrite
            If s_members.ContainsKey(key:=milestone.ID) Then
                Call s_members.Remove(key:=milestone.ID)
            End If

            If s_orgMSvalues.ContainsKey(key:=milestone.ID) Then
                Call s_orgMSvalues.Remove(key:=milestone.ID)
            End If

            ' add Member Entry
            s_members.Add(key:=milestone.ID, value:=milestone)
            ' copy
            Call s_orgMSvalues.Add(key:=milestone.ID, value:=milestone.Value)

            '
            Return True
        End Function
        ''' <summary>
        ''' load the current schedule by uid and optionally workspaceID
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal uid As Long, Optional workspaceID As String = "") As Boolean
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            End If
            Dim aCurrSchedule As CurrentSchedule = CurrentSchedule.Retrieve(UID:=uid, workspaceID:=workspaceID)
            If aCurrSchedule IsNot Nothing Then
                Me.Inject(UID:=uid, updc:=aCurrSchedule.UPDC)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' loads an schedule from store
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal UID As Long, ByVal updc As Long) As Boolean
            Return MyBase.Inject(pkArray:={UID, updc})
        End Function
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
        '**** create : create the object by the PrimaryKeys
        '****
        ''' <summary>
        ''' create a persistable schedule
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name=constFNupdc></param>
        ''' <param name="workspaceID"></param>
        ''' <param name="SCHEDULETYPEID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal uid As Long, _
                                Optional ByVal updc As Long = 0, _
                                Optional ByVal workspaceID As String = "", _
                                Optional ByVal scheduletypeid As String = "") As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Create = False
                    Exit Function
                End If
            End If
            If workspaceID = "" Then
                _workspace = CurrentSession.CurrentWorkspaceID
            End If

            '* primary key
            Dim pkArray() As Object = {uid, updc}

            '* new key ?!
            If updc = 0 Then
                If Not Me.GetMaxUpdc(max:=updc, workspaceID:=workspaceID) Then
                    Call CoreMessageHandler(message:=" primary key values could not be created - cannot create object", arg1:=pkArray, _
                                            subname:="Schedule.create", tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                '* increase
                updc += 1
            End If

            If MyBase.Create(pkArray, checkUnique:=True) Then
                ' set the primaryKey
                _uid = pkArray(0)
                _updc = pkArray(1)
                _workspace = workspaceID
                If scheduletypeid <> "" Then
                    _typeid = scheduletypeid
                Else
                    _typeid = CurrentSession.DefaultScheduleTypeID
                End If


                ' this will set also the loadMilestones
                If _typeid <> "" Then
                    _typeid = scheduletypeid
                    Call LoadMilestones(scheduletypeid)
                End If
                Return Me.IsCreated
            End If

        End Function

        '**** getDeliverableTrack -> get Track for the corresponding Deliverable (same uid)
        '****
        ''' <summary>
        ''' retrieve the corresponding deliverableTrack
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDeliverableTrack() As Track
            Dim aTrackDef As New Track
            Dim aTarget As New CurrentTarget

            If IsLoaded Then
                If Not aTarget.Inject(uid:=Me.Uid, workspaceID:=Me.workspaceID) Then
                    aTarget.UPDC = 0
                End If
                If aTrackDef.Inject(deliverableUID:=Me.Uid, _
                                    scheduleUID:=Me.Uid, _
                                    scheduleUPDC:=Me.Updc, _
                                    targetUPDC:=aTarget.UPDC) Then
                    GetDeliverableTrack = aTrackDef
                End If
            End If

            GetDeliverableTrack = Nothing
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
        Public Function hasMilestone(ByVal ID As String, _
                                     Optional ByVal mstypeid As otMilestoneType = 0, _
                                     Optional ByVal hasData As Boolean = True) As Boolean
            Dim aVAlue As Object
            Dim aDefSchedule As ScheduleDefinition = Me.GetDefSchedule
            Dim aRealID As String = ""
            'Dim aDefScheduleMilestone As clsOTDBDefScheduleMilestone = clsOTDBDefScheduleMilestone.Retrieve(scheduletype:=Me.Typeid, ID:=aRealID)
            Dim aScheduleMilestone As ScheduleMilestone
            Dim aDefMilestone As MileStoneDefinition = MileStoneDefinition.Retrieve(id:=aRealID)

            If Not IsCreated And Not IsLoaded Then
                Return False
            End If

            ' check aliases
            If aDefSchedule Is Nothing Then
                Call CoreMessageHandler(message:="DefSchedule is not valid", arg1:=Me.Typeid, subname:="Schedule.getMilestone")
                Return False
            End If
            aRealID = aDefSchedule.GetMilestoneIDByAlias(AliasID:=LCase(ID))
            If aRealID = "" Then
                aRealID = LCase(ID)
            End If
            ' get the DefSchedule Milestone
            ' if mstypeid is missing
            If mstypeid = 0 And aDefMilestone IsNot Nothing Then
                mstypeid = aDefMilestone.Typeid
            End If

            ' if milestone exists in members
            If s_members.ContainsKey(LCase(aRealID)) Then
                aScheduleMilestone = s_members.Item(LCase(aRealID))
                aVAlue = aScheduleMilestone.Value

                Select Case mstypeid

                    ' check date
                    Case otMilestoneType.[Date]
                        If IsDate(aVAlue) Then
                            If hasData And aVAlue <> ConstNullDate Then
                                hasMilestone = True
                            ElseIf Not hasData Then
                                hasMilestone = True
                            Else
                                hasMilestone = False
                            End If
                        ElseIf Not hasData Then
                            hasMilestone = True
                        Else
                            hasMilestone = False
                        End If
                        '
                        ' check status
                    Case otMilestoneType.Status
                        If Trim(CStr(aVAlue)) <> "" And hasData Then
                            hasMilestone = True

                        ElseIf Trim(CStr(aVAlue)) = "" And hasData Then
                            hasMilestone = False
                        ElseIf Not hasData Then
                            hasMilestone = True
                        Else
                            hasMilestone = True
                        End If
                End Select

            Else
                hasMilestone = False
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
            HasMilestoneDate = Me.hasMilestone(ID:=ID, mstypeid:=otMilestoneType.[Date], hasData:=False)
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
            If Not Me.hasMilestone(ID:=ID, hasData:=True) Then
                Return ifNotExists
            Else
                Return False ' false = not missing value
            End If

        End Function

        '******* checks if schedule is finished
        '*******
        ''' <summary>
        ''' is the schedule finished
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsFinished() As Boolean
            Dim aVAlue As Object

            If _IsLoaded Or Me.IsCreated Then
                '*** HACK !
                If s_members.ContainsKey(LCase("bp10")) Then
                    aVAlue = Me.GetMilestoneValue("bp10")
                    If IsDate(aVAlue) And aVAlue <> ConstNullDate Then
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
        Public Function GetTimeInterval(TaskTypeID As String) As clsTimeInterval
            Dim aVAlue As Object
            Dim aTimeInterval As New clsTimeInterval

            If Not _IsLoaded And Not Me.IsCreated Then
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
                    If IsDate(aVAlue) And aVAlue <> ConstNullDate Then
                        aTimeInterval.enddate = CDate(aVAlue)
                    Else
                        aTimeInterval.enddate = ConstNullDate
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
                    If IsDate(aVAlue) And aVAlue <> ConstNullDate Then
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
                                      subname:="clsOTDBSchedules.increaserevision", break:=False)
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
                                Optional ByVal timestamp As Date = ot.ConstNullDate, _
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
                If s_msglog Is Nothing Then
                    s_msglog = New ObjectLog
                End If
                msglog = s_msglog
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
            If s_haveMilestonesChanged Then

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
                isProcessable = Me.Persist(timestamp:=timestamp, forceSerializeToOTDB:=forceSerializeToOTDB)
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
        ''' Persist the data object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <param name="ForceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate, _
                                 Optional ByVal forceSerializeToOTDB As Boolean = False) As Boolean
            Dim aMilestone As New ScheduleMilestone
            Dim m As Object

            Try
                If Not Feed() Then
                    Persist = False
                    Exit Function
                End If

                '*** overload it from the Application Container
                '***
                If Me.SerializeWithHostApplication Then
                    If overwriteToHostApplication(Me.Record) Then
                        s_savedToHost = True
                    End If
                End If
                If IsMissing(timestamp) Or Not IsDate(timestamp) Then
                    timestamp = Now
                End If
                If forceSerializeToOTDB Or (Not Me.SerializeWithHostApplication Or isOverloadingSuspended()) Then
                    ' persist all the milestones
                    For Each kvp As KeyValuePair(Of String, ScheduleMilestone) In s_members
                        aMilestone = kvp.Value
                        Call aMilestone.Persist(timestamp)
                    Next

                    ' set last forecast update
                    If Me.IsForecastChanged Then
                        Me.LastForecastUpdate = timestamp
                        Feed()
                    End If

                    Persist = MyBase.Persist(timestamp)

                End If

                ' reset change flags
                If Persist Then
                    s_isForeCastChanged = False
                    s_haveMilestonesChanged = False
                End If

                Exit Function

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Schedule.Persist", exception:=ex)
                Return False
            End Try



        End Function

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

            If Not IsLoaded And Not IsCreated Then
                Clone = Nothing
                Exit Function
            End If
            '* initialize
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Clone = Nothing
                    Exit Function
                End If
            End If


            Try

                If Not Feed() Then
                    Return Nothing
                End If

                '*** key ?
                If Updc = 0 Then
                    If Not Me.GetMaxUpdc(max:=pkarray(1), workspaceID:=Me.workspaceID) Then
                        Call CoreMessageHandler(message:="cannot create unique primary key values - abort clone", arg1:=pkarray, _
                                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
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
                        For Each kvp As KeyValuePair(Of String, ScheduleMilestone) In s_members
                            aMember = kvp.Value
                            aCloneMember = aMember.Clone(UID:=Uid, updc:=Updc, ID:=aMember.ID)
                            If Not aCloneMember Is Nothing Then
                                Call aNewObject.AddMilestone(milestone:=aCloneMember)
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

            Dim aNewObject As New Schedule
            Dim newRecord As New ormRecord
            Dim aWorkspace As New Workspace
            Dim aCurrSCHEDULE As New CurrentSchedule

            Dim newUPDC As Long

            Dim m As Object
            Dim aVAlue As Object

            If Not IsLoaded And Not IsCreated Then
                CloneToWorkspace = False
                Exit Function
            End If
            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    CloneToWorkspace = False
                    Exit Function
                End If
            End If
            '**
            If Not aWorkspace.Inject(workspaceID) Then
                Call CoreMessageHandler(arg1:=workspaceID, subname:="Schedule.cloneToWorkspace", message:="couldn't load workspaceID")
                CloneToWorkspace = False
                Exit Function
            End If

            ' get the new updc
            If Me.GetMaxUpdc(max:=newUPDC, workspaceID:=workspaceID) Then
                If newUPDC = 0 Then
                    newUPDC = aWorkspace.Min_schedule_updc
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
                If Not aCurrSCHEDULE.Inject(UID:=Me.Uid, workspaceID:=workspaceID) Then
                    Call aCurrSCHEDULE.Create(UID:=Me.Uid, workspaceID:=workspaceID)
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

            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            End If


            Try
                ' get
                Dim aStore As iormDataStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="getmaxupdc", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = "max(updc)"
                    aCommand.Where = "uid=@uid and wspace=@wspace"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@uid", ColumnName:=ConstFNUid, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@wspace", ColumnName:=ConstFNWorkspace, tablename:=ConstTableID))
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
                            If mymax >= (aWorkspaceDef.Max_schedule_updc - 10) Then
                                Call CoreMessageHandler(showmsgbox:=True, message:="Number range for workspaceID ends", _
                                                      arg1:=workspaceID, messagetype:=otCoreMessageType.ApplicationWarning)
                            End If
                        End If
                    Else
                        If aWorkspaceDef IsNot Nothing Then
                            mymax = aWorkspaceDef.Min_schedule_updc
                        Else
                            GetMaxUpdc = False
                        End If

                    End If
                    GetMaxUpdc = True

                Else
                    If aWorkspaceDef IsNot Nothing Then
                        mymax = aWorkspaceDef.Min_schedule_updc
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
        Public Function RunXPrecheck(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXPreCheck

        End Function
        ''' <summary>
        ''' run XChange on an envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(ByRef envelope As XEnvelope) As Boolean Implements iotXChangeable.RunXChange

            Dim aXCmd As otXChangeCommandType = envelope.GetObjectXCmd(objectname:=Me.ConstTableID)
            Dim aValue, anOldValue As Object

            '* load the schedule from the envelope
            If Not Me.Inject(envelope:=envelope) Then
                ' could not load the envelope -> Add ?!
                Dim anUID As Object = envelope.GetSlotValueByFieldname(fieldname:=ConstFNUid, tablename:=ConstTableID)
                Dim aTypeid As String = envelope.GetSlotValueByFieldname(fieldname:=ScheduleDefinition.ConstFNType, tablename:=ScheduleDefinition.ConstTableID)
                Dim anWSId As String = envelope.GetSlotValueByID(id:="WS")
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
                                If Me.hasMilestone(ID:=aSlot.XAttribute.ID) Then
                                    If Not aSlot.IsEmpty Then
                                        If Not Me.SetMilestone(ID:=aSlot.XAttribute.ID, Value:=aSlot.DBValue, setNull:=aSlot.IsNull) Then
                                            '*** error
                                        End If
                                    End If
                                Else
                                    '** error
                                End If
                            Else
                                '* change the underlying record
                                Me.Record.SetValue(index:=aSlot.XAttribute.Entryname, value:=aSlot.DBValue)
                            End If
                        End If

                    Next

                    '** if we have a change
                    If Me.IsChanged Or Me.haveMileStonesChanged Or Me.Record.IsChanged Then
                        If Me.Publish() Then
                            envelope.AddSlotByFieldname(fieldname:=ConstFNUpdc, tablename:=ConstTableID, value:=Me.Updc, _
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
                    Return envelope.RunDefaultXchange(Me)
            End Select

        End Function
        ''' <summary>
        ''' run XChange on a Schedule Object
        ''' </summary>
        ''' <param name="MAPPING"></param>
        ''' <param name="CHANGECONFIG"></param>
        ''' <param name="MSGLOG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function runXChangeOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
        ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean

            Dim aCMuid As clsOTDBXChangeMember
            Dim aCMupdc As clsOTDBXChangeMember
            Dim aCMWspace As clsOTDBXChangeMember
            Dim aChangeMember As New clsOTDBXChangeMember

            Dim anUID As Long
            Dim anUPDC As Long
            Dim aNewUPDC As Long
            Dim aCollection As New Collection
            Dim newSchedule As Boolean

            Dim aSchedule As New Schedule
            Dim aCurrSCHEDULE As New CurrentSchedule
            Dim aDeliverable As New Deliverable
            Dim aTrack As New Track
            Dim anObjectDef As New clsOTDBXChangeMember
            Dim anAttribute As New clsOTDBXChangeMember
            Dim aNewSchedule As New Schedule
            Dim aWorkspace As String
            Dim setCurrSchedule As Boolean
            Dim aVAlue As Object

            Dim aTimestamp As Date

            If CHANGECONFIG.ProcessedDate <> ConstNullDate Then
                aTimestamp = CHANGECONFIG.ProcessedDate
            Else
                aTimestamp = Now
            End If

            '*** ObjectDefinition
            anObjectDef = CHANGECONFIG.ObjectByName(ConstTableID)

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
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC2", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("SC2")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname)
                    runXChangeOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXChangeOLD = False
                Exit Function
            Else
                anUID = CLng(aVAlue)
            End If


            ' optional key updc
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC3", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                'Call msglog.addMsg("201", Nothing, Nothing, "SC3", "SC3", ourTableName, ChangeConfig.ConfigName)
                anUPDC = -1
            ElseIf Not IsNumeric(aVAlue) Then
                anUPDC = -1
            Else
                anUPDC = CLng(aVAlue)
                setCurrSchedule = False
            End If


            '*** try to load the current Schedule
            If anUPDC = -1 Then
                '*** indeed we have something to update
                setCurrSchedule = True
                ' get the updc
                If aCurrSCHEDULE.Inject(UID:=anUID, workspaceID:=aWorkspace) Then
                    anUPDC = aCurrSCHEDULE.UPDC

                    'System.Diagnostics.Debug.WriteLine(anUID, anUPDC)
                    'aCurrSchedule.initialize
                Else
                    'create necessary ?!
                    If anObjectDef.XChangeCmd <> otXChangeCommandType.UpdateCreate Then
                        Call MSGLOG.AddMsg("203", CHANGECONFIG.Configname, Nothing, Nothing, _
                                           CHANGECONFIG.Configname, anUID & ", <none>")
                        runXChangeOLD = False
                        Exit Function
                    End If
                    ' create an new UPDC
                    anUPDC = 1
                End If

            End If

            '** load the Schedule
            If Not aSchedule.Inject(UID:=anUID, updc:=anUPDC) Then
                If anObjectDef.XChangeCmd <> otXChangeCommandType.UpdateCreate Then
                    Call MSGLOG.AddMsg("203", Nothing, Nothing, "SC3", CHANGECONFIG.Configname, anUID & "," & anUPDC)
                    runXChangeOLD = False
                    Exit Function
                Else
                    ' create with the given UPDC !
                    Call aSchedule.Create(uid:=anUID, updc:=anUPDC)
                    newSchedule = True
                    aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC14", mapping:=MAPPING)
                    If IsNull(aVAlue) Then
                        Call MSGLOG.AddMsg("204", Nothing, Nothing, "SC14", CHANGECONFIG.Configname, anUID & "," & anUPDC)
                        runXChangeOLD = False
                        Exit Function
                    Else
                        ' missing is the type !!
                        ' must be looked up -> member fill !
                        aSchedule.workspaceID = aWorkspace    ' in this order because we need the workspaceID before type
                        aSchedule.Typeid = CStr(aVAlue)

                    End If



                End If
            Else
                newSchedule = False
                ' change the workspaceID ?!
                If aSchedule.workspaceID <> aWorkspace Then
                    Debug.Assert(False)
                    Debug.Print("workspaceID changed in Schedule")
                    aSchedule.workspaceID = aWorkspace
                End If
            End If

            ' add the UPDC to the MAPPING
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC3", _
                                                 objectname:=ConstTableID, mapping:=MAPPING)
            If IsNull(aVAlue) Then
                Call CHANGECONFIG.AddAttributeByID(id:="SC3", objectname:=ConstTableID, _
                                                   xcmd:=otXChangeCommandType.Read, isXChanged:=False)
            End If
            aChangeMember = CHANGECONFIG.AttributeByID("SC3")
            If MAPPING.ContainsKey(key:=aChangeMember.ordinal.Value) And Not aChangeMember.IsReadOnly Then
                Call MAPPING.Remove(key:=aChangeMember.ordinal.Value)
            End If
            If Not aChangeMember.IsReadOnly Then Call MAPPING.Add(key:=aChangeMember.ordinal.Value, value:=anUPDC)

            '********* check on the action xchange command
            '*********

            If anObjectDef.XChangeCmd = otXChangeCommandType.Read Then
                '* otRead with Compounds can be handled by the standard
                '*
                runXChangeOLD = CHANGECONFIG.runDefaultXChange4Object(anObjectDef, MAPPING, MSGLOG)
                Exit Function

            ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Update _
            Or anObjectDef.XChangeCmd = otXChangeCommandType.UpdateCreate _
            Or anObjectDef.XChangeCmd = otXChangeCommandType.Duplicate Then

                '**** update, updatecreate and duplicate are handled by the schedule publish function on
                '**** its own
                '***

                '*** set the Attributes if these are milestone=compounds
                '***

                For Each anAttribute In CHANGECONFIG.AttributesByObjectName(objectname:=ConstTableID)
                    If anAttribute.IsCompound And _
                    (anAttribute.XChangeCmd = otXChangeCommandType.Update Or anAttribute.XChangeCmd = otXChangeCommandType.UpdateCreate Or anAttribute.XChangeCmd = otXChangeCommandType.Duplicate) Then
                        ' read compound
                        'If aSchedule.existsMilestone(ID:=anAttribute.ID) Then
                        '    Call MAPPING.Remove(Key:=anAttribute.ordinal)
                        '    Call MAPPING.add(Key:=anAttribute.ordinal, ITEM:=aSchedule.getMilestone(ID:=anAttribute.ID, ORIGINAL:=True))
                        'End If
                        ' get out of the Mapping the Value
                        aVAlue = CHANGECONFIG.GetMemberValue(changemember:=anAttribute, _
                                                             objectname:=ConstTableID, mapping:=MAPPING)
                        If Not IsNull(aVAlue) Then
                            If Not aVAlue = Nothing Then
                                If aSchedule.hasMilestone(ID:=anAttribute.ID) Then
                                    ' convert to DB
                                    Call anAttribute.convertValue2DB(aVAlue, aVAlue, existingValue:=False)
                                    ' save
                                    If aSchedule.SetMilestone(ID:=anAttribute.ID, Value:=aVAlue) Then
                                        If aSchedule.GetMilestoneValue(ID:=anAttribute.ID, ORIGINAL:=True) <> aSchedule.GetMilestoneValue(ID:=anAttribute.ID) Then
                                            System.Diagnostics.Debug.WriteLine(anAttribute.ID, aSchedule.GetMilestoneValue(ID:=anAttribute.ID, ORIGINAL:=True), aVAlue)
                                        End If
                                    Else
                                        System.Diagnostics.Debug.Assert(False)
                                    End If
                                Else
                                    ' setting something which doesnot exist ?!
                                End If
                            End If
                        End If
                    End If    ' compound
                Next anAttribute

                ' publish -> persisted -> set new updc in MAPPING
                If aSchedule.Publish(msglog:=MSGLOG) Then
                    If IsNull(CHANGECONFIG.GetMemberValue(ID:="SC3", _
                                                          objectname:=ConstTableID, mapping:=MAPPING)) Then
                        Call CHANGECONFIG.AddAttributeByID(id:="SC3", isXChanged:=False, objectname:=ConstTableID, _
                                                           xcmd:=otXChangeCommandType.Read)
                        aChangeMember = CHANGECONFIG.AttributeByID("SC3")
                    Else
                        Call MAPPING.Remove(key:=aChangeMember.ordinal.Value)
                    End If
                    Call MAPPING.Add(key:=aChangeMember.ordinal.Value, value:=aSchedule.Updc)
                End If

                '** rest is up to standard (other fields)
                '**
                runXChangeOLD = CHANGECONFIG.runDefaultXChange4Object(XCHANGEOBJECT:=anObjectDef, _
                                                                   MAPPING:=MAPPING, MSGLOG:=MSGLOG, NoCompounds:=True)
                ' delete
            ElseIf anObjectDef.XChangeCmd = otXChangeCommandType.Delete Then
                '*** handle new entries on other objects such as Track ?!
                '    Debug.Assert False
            End If


            runXChangeOLD = True
        End Function
        ''' <summary>
        ''' run Xchange Precheck
        ''' </summary>
        ''' <param name="MAPPING"></param>
        ''' <param name="CHANGECONFIG"></param>
        ''' <param name="MSGLOG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function runXPreCheckOLD(ByRef MAPPING As Dictionary(Of Object, Object), _
        ByRef CHANGECONFIG As clsOTDBXChangeConfig, _
        Optional ByRef MSGLOG As ObjectLog = Nothing) As Boolean
            Dim aCMuid As clsOTDBXChangeMember
            Dim aCMupdc As clsOTDBXChangeMember
            Dim anObject As New clsOTDBXChangeMember
            Dim aVAlue As Object
            Dim anUPDC As Long
            Dim anUID As Long

            ' set msglog
            If MSGLOG Is Nothing Then
                MSGLOG = s_msglog
                MSGLOG.Create(_msglogtag)
            End If
            '** check on the min. required primary key uid
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC2", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                ' error condition
                aCMuid = CHANGECONFIG.AttributeByID("SC2")
                If aCMuid Is Nothing Then
                    Call MSGLOG.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                Else
                    Call MSGLOG.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname)
                    runXPreCheckOLD = False
                    Exit Function
                End If
                '**
            ElseIf Not IsNumeric(aVAlue) Then
                Call MSGLOG.AddMsg("202", Nothing, Nothing, "SC2", "SC2", ConstTableID, CHANGECONFIG.Configname, aVAlue, "numeric")
                runXPreCheckOLD = False
                Exit Function
            Else
                anUID = CLng(aVAlue)
            End If


            ' optional key updc
            aVAlue = CHANGECONFIG.GetMemberValue(ID:="SC3", mapping:=MAPPING)
            If IsNull(aVAlue) Then
                'Call msglog.addMsg("201", Nothing, Nothing, "SC3", "SC3", ourTableName, ChangeConfig.ConfigName)
                anUPDC = -1
            ElseIf Not IsNumeric(aVAlue) Then
                anUPDC = -1
            Else
                anUPDC = CLng(aVAlue)

            End If

            ' generell tests
            anObject = CHANGECONFIG.ObjectByName(Me.TableID)
            runXPreCheckOLD = CHANGECONFIG.runDefaultXPreCheck(anObject:=anObject, _
                                                            aMapping:=MAPPING, MSGLOG:=MSGLOG)


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
                If envelope.HasSlotByFieldname(fieldname:=Me.ConstFNUid, tablename:=Me.ConstTableID) Then
                    aValue = envelope.GetSlotValueByFieldname(fieldname:=Me.ConstFNUid, tablename:=Me.ConstTableID, asHostValue:=False)
                Else
                    aValue = Nothing
                End If
                If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                    CoreMessageHandler(message:="Envelope has no id 'uid'", messagetype:=otCoreMessageType.ApplicationError, _
                                       subname:="Schedule.Inject(Envelope)")
                    If envelope.Xchangeconfig.AttributeByFieldName(fieldname:=Me.ConstFNUid, tablename:=Me.ConstTableID) Is Nothing Then
                        Call envelope.MsgLog.AddMsg("200", Nothing, Nothing, "SC2", "SC2", ConstTableID, envelope.Xchangeconfig.Configname)
                    Else
                        Call envelope.MsgLog.AddMsg("201", Nothing, Nothing, "SC2", "SC2", ConstTableID, envelope.Xchangeconfig.Configname)
                    End If

                    Return False
                Else
                    uid = CLng(aValue)
                End If
                '** WS
                If envelope.HasSlotByID(id:="WS") Then
                    aValue = envelope.GetSlotValueByID(id:="WS", asHostValue:=False)
                    wsID = CStr(aValue)
                Else
                    wsID = CurrentSession.CurrentWorkspaceID
                    envelope.AddSlotByID(id:="WS", value:=wsID, isHostValue:=False, extendXConfig:=True, replaceSlotIfExists:=True)
                End If

                '** updc
                If envelope.HasSlotByFieldname(fieldname:=Me.ConstFNUpdc, tablename:=Me.ConstTableID) Then
                    aValue = envelope.GetSlotValueByFieldname(fieldname:=Me.ConstFNUpdc, tablename:=Me.ConstTableID, asHostValue:=False)
                Else
                    aValue = Nothing
                End If
                If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                    Dim aCurrSchedule As CurrentSchedule = CurrentSchedule.Retrieve(UID:=uid, workspaceID:=wsID)
                    If aCurrSchedule IsNot Nothing Then
                        updc = aCurrSchedule.UPDC
                        envelope.AddSlotByID(id:="SC3", value:=updc, isHostValue:=False, extendXConfig:=True)
                    Else
                        'CoreMessageHandler(message:="Envelope has no determinable id 'SC3'", messagetype:=otCoreMessageType.ApplicationError, _
                        '                   subname:="Schedule.Inject(Envelope)")
                        Return False
                    End If
                Else
                    updc = CLng(aValue)
                End If
                '*** load the schedule
                If Not Me.Inject(UID:=uid, updc:=updc) Then
                    CoreMessageHandler(message:="could not load the schedule ", arg1:=CStr(uid) & "." & CStr(updc), _
                                       messagetype:=otCoreMessageType.ApplicationError, subname:="Schedule.Inject(Envelope)")
                    Return False
                End If
            Else
                '** exists
                uid = Me.Uid
                envelope.AddSlotByID(id:="SC2", value:=uid, isHostValue:=False, extendXConfig:=True)
                updc = Me.Updc
                envelope.AddSlotByID(id:="SC3", value:=updc, isHostValue:=False, extendXConfig:=True)
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
            Dim anObjectDef As ObjectDefinition = CurrentSession.Objects.GetObject(objectname:=Me.ConstTableID)
            For Each anEntry In anObjectDef.Entries
                Dim anAttribute As Xchange.XConfigAttributeEntry = envelope.Xchangeconfig.AttributeByID(ID:=anEntry.XID, objectname:=Me.ConstTableID)
                If anAttribute IsNot Nothing AndAlso anAttribute.IsCompound Then
                    '** COMPOUNDS ARE ALWAYS MILESTONES FOR SCHEDULES
                    '**
                    avalue = Me.GetMilestoneValue(ID:=anAttribute.ID)
                    If avalue IsNot Nothing Then
                        envelope.AddSlotByID(id:=anAttribute.ID, tablename:=Me.ConstTableID, value:=avalue, isHostValue:=False)
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
    ''' Schedule Milestone Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ScheduleMilestone.ConstObjectID, modulename:=constModuleScheduling, Version:=1, description:="milestone data for schedules")> Public Class ScheduleMilestone
        Inherits ormDataObject
        Implements iormPersistable
        Implements iotCloneable(Of ScheduleMilestone)
        Implements iormInfusable

        Public Const ConstObjectID = "ScheduleMilestone"

        '** Table
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addsparefields:=True)> Public Const constTableID = "tblScheduleMilestones"

        '** Index
        <ormSchemaIndex(columnname1:=ConstFNUid, columnname2:=ConstFNUpdc)> Public constIndexCompound = ConstDefaultCompoundIndexName

        '** Keys
        <ormObjectEntry(referenceObjectEntry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUid, _
             primaryKeyordinal:=1, XID:="MST1", aliases:={"SUID"})> _
        Public Const ConstFNUid = Schedule.ConstFNUid
        <ormObjectEntry(referenceObjectEntry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc, _
           primaryKeyordinal:=2, XID:="MST2")> _
        Public Const ConstFNUpdc = Schedule.ConstFNUpdc
        '** link together
        <ormSchemaForeignKey(entrynames:={ConstFNUid, ConstFNUpdc}, foreignkeyreferences:={Schedule.ConstObjectID & "." & Schedule.ConstFNUid, _
                Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc}, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKSchedule = "fkschedules"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, defaultvalue:="", _
            title:="milestone id", Description:="id of the milestone", _
          primaryKeyordinal:=3, XID:="MST3")> Public Const ConstFNID = "id"

        '** Fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, defaultvalue:="", isnullable:=True, _
           title:="value", Description:="text presentation of the milestone value", XID:="MST4")> Public Const ConstFNvalue = "value"
        <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, _
          title:="value", Description:="date presentation of the milestone value", XID:="MST5")> Public Const ConstFNvaluedate = "valuedate"
        <ormObjectEntry(typeid:=otFieldDataType.Numeric, isnullable:=True, _
                 title:="value", Description:="numeric presentation of the milestone value", XID:="MST6")> Public Const ConstFNvaluenumeric = "valuenumeric"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, isnullable:=True, _
        title:="value", Description:="bool presentation of the milestone value", XID:="MST7")> Public Const ConstFNvaluebool = "valuebool"
        <ormObjectEntry(typeid:=otFieldDataType.Long, isnullable:=True, _
        title:="value", Description:="long presentation of the milestone value", XID:="MST8")> Public Const ConstFNvaluelong = "valuelong"
        <ormObjectEntry(typeid:=otFieldDataType.Long, defaultvalue:="0", _
        title:="datatype", Description:="datatype of the milestone value", XID:="MST10")> Public Const ConstFNDatatype = "datatype"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, defaultvalue:="0", _
        title:="is a forecast", Description:="true if the milestone is a forecast", XID:="MST11")> Public Const ConstFNIsFC = "isforecast"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, defaultvalue:="0", _
        title:="is a status", Description:="true if the milestone is a status", XID:="MST12")> Public Const ConstFNIsStatus = "isstatus"

        <ormObjectEntry(referenceObjectEntry:=ObjectLogMessage.ConstObjectID & "." & ObjectLogMessage.ConstFNTag)> _
        Public Const ConstFNmsglogtag = ObjectLogMessage.ConstFNTag

        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, _
             Description:="workspaceID ID of the schedule", useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNWorkspace = Workspace.ConstFNID
        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(typeid:=otFieldDataType.Memo, defaultvalue:="", _
                     title:="comment", Description:="comment", XID:="MST14")> Public Const ConstFNcmt = "cmt"


        ' fields

        <ormEntryMapping(EntryName:=ConstFNUid)> Private _uid As Long
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _updc As Long
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _id As String = ""

        '<ormEntryMapping(EntryName:=ConstFNUid)> -> special infuse
        Private _value As Object
        ' <ormEntryMapping(EntryName:=ConstFNUid)> -> special infuse
        Private _datatype As otFieldDataType
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _cmt As String = ""
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _workspaceID As String = ""
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _isStatus As Boolean

        'Private s_isActual As Boolean
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _isForecast As Boolean
        <ormEntryMapping(EntryName:=ConstFNUid)> Private _msglogtag As String = ""


        'dynamic
        Private s_loadedFromHost As Boolean
        Private s_savedToHost As Boolean
        Private s_isCacheNoSave As Boolean    ' if set this is not saved since taken from another updc
        Private s_msglog As New ObjectLog

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableID)

        End Sub

#Region "Properties"

        Public Property IsCacheNoSave() As Boolean
            Get
                IsCacheNoSave = s_isCacheNoSave
            End Get
            Set(value As Boolean)
                If value Then
                    s_isCacheNoSave = True
                Else
                    s_isCacheNoSave = False
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

        Public Property UID() As Long
            Get

                UID = _uid
            End Get
            Set(value As Long)
                If _uid <> value Then
                    _uid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property Updc() As Long
            Get
                Updc = _updc
            End Get
            Set(value As Long)
                If _updc <> value Then
                    _updc = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ID() As String
            Get
                ID = _id
            End Get
            Set(value As String)
                If LCase(_id) <> LCase(value) Then
                    _id = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property workspaceID() As String
            Get
                workspaceID = _workspaceID
            End Get
            Set(value As String)
                If LCase(_workspaceID) <> LCase(value) Then
                    _workspaceID = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Value() As Object
            Get
                Value = _value
            End Get
            Set(ByVal value As Object)
                If value <> _value Then
                    _value = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Datatype() As otFieldDataType
            Get
                Datatype = _datatype
            End Get
            Set(value As otFieldDataType)
                If _datatype <> value Then
                    _datatype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Comment() As String
            Get
                Comment = _cmt
            End Get
            Set(value As String)
                If _cmt <> value Then
                    _cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsForecast() As Boolean
            Get
                IsForecast = _isForecast
            End Get
            Set(value As Boolean)
                If _isForecast <> value Then
                    _isForecast = value
                    Me.IsChanged = True
                End If

            End Set
        End Property

        Public Property IsActual() As Boolean
            Get
                IsActual = Not _isForecast
            End Get
            Set(value As Boolean)
                If _isForecast <> Not value Then
                    _isForecast = Not value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property IsStatus() As Boolean
            Get
                IsStatus = _isStatus
            End Get
            Set(value As Boolean)
                If _isStatus <> value Then
                    _isStatus = value
                    _isStatus = True
                End If
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
                        s_loadedFromHost = True
                    End If
                End If

                _datatype = CLng(Record.GetValue(ConstFNDatatype))
                aVAlue = Record.GetValue(ConstFNvalue)
                ' select on Datatype
                Select Case _datatype

                    Case otFieldDataType.Numeric
                        aVAlue = Record.GetValue(ConstFNvaluenumeric)
                        _value = CDbl(aVAlue)
                    Case otFieldDataType.Text

                        _value = CStr(aVAlue)
                    Case otFieldDataType.Runtime, otFieldDataType.Formula, otFieldDataType.Binary
                        _value = ""
                        Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", messagetype:=otCoreMessageType.ApplicationError, _
                                              message:="runtime, formular, binary can't infuse", msglog:=s_msglog, arg1:=aVAlue)
                    Case otFieldDataType.[Date], otFieldDataType.Timestamp
                        aVAlue = Record.GetValue(ConstFNvaluedate)
                        If IsDate(aVAlue) Then
                            _value = CDate(aVAlue)
                        Else
                            Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", _
                                            message:="Value supposed to be a date cannot be converted", _
                                            messagetype:=otCoreMessageType.ApplicationError, _
                                            msglog:=s_msglog, arg1:=aVAlue)

                        End If

                    Case otFieldDataType.[Long]
                        aVAlue = Record.GetValue(ConstFNvaluelong)
                        _value = CLng(aVAlue)
                    Case otFieldDataType.Bool
                        aVAlue = Record.GetValue(ConstFNvaluebool)
                        _value = CBool(aVAlue)
                    Case otFieldDataType.Memo
                        _value = CStr(aVAlue)
                    Case Else
                        Call CoreMessageHandler(subname:="ScheduleMilestone.infuse", _
                                              message:="unknown datatype to be infused", msglog:=s_msglog, arg1:=aVAlue)
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
        Public Overloads Function Inject(ByVal UID As Long, ByVal updc As Long, ByVal ID As String) As Boolean
            Dim pkarray() As Object = {UID, updc, ID}
            Return MyBase.Inject(pkarray)
        End Function
        ''' <summary>
        ''' create the persistency schema 
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ScheduleMilestone)()

            '            '''** OUTDATED
            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim CompundColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition


            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Tablename = constTableID

            '            With aTable
            '                .Create(constTableID)
            '                .Delete()

            '                '***
            '                '*** Fields
            '                '****

            '                'Type
            '                aFieldDesc.Datatype = otFieldDataType.[Long]

            '                aFieldDesc.Title = "uid of the schedule"
            '                aFieldDesc.ColumnName = "uid"
            '                aFieldDesc.ID = "mst1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                CompundColumnNames.Add(aFieldDesc.ColumnName)
            '                'updc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "updc of schedule"
            '                aFieldDesc.ColumnName = "updc"
            '                aFieldDesc.ID = "mst2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '                CompundColumnNames.Add(aFieldDesc.ColumnName)
            '                'id
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "milestone id"
            '                aFieldDesc.ColumnName = "id"
            '                aFieldDesc.ID = "mst3"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                'value
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "value as text"
            '                aFieldDesc.ColumnName = "value"
            '                aFieldDesc.ID = "mst4"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'date
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "value as date"
            '                aFieldDesc.ColumnName = "valuedate"
            '                aFieldDesc.ID = "mst5"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'numeric
            '                aFieldDesc.Datatype = otFieldDataType.Numeric
            '                aFieldDesc.Title = "value as numeric"
            '                aFieldDesc.ColumnName = "valuenumeric"
            '                aFieldDesc.ID = "mst6"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'bool
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "value as bool"
            '                aFieldDesc.ColumnName = "valuebool"
            '                aFieldDesc.ID = "mst7"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                'bool
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "value as long"
            '                aFieldDesc.ColumnName = "valuelong"
            '                aFieldDesc.ID = "mst8"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                'datatype
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "datatype"
            '                aFieldDesc.ColumnName = "datatype"
            '                aFieldDesc.ID = "mst10"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' is forecast ?
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is an ForecastEntry"
            '                aFieldDesc.ColumnName = "isforecast"
            '                aFieldDesc.ID = "mst11"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' is status ?
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is an StatusEntry"
            '                aFieldDesc.ColumnName = "isstatus"
            '                aFieldDesc.ID = "mst12"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' cmt
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "message log tag"
            '                aFieldDesc.ColumnName = "msglogtag"
            '                aFieldDesc.ID = "mst13"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' workspaceID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workspaceID"
            '                aFieldDesc.ColumnName = "wspace"
            '                aFieldDesc.ID = "ws"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)


            '                ' msglogtag
            '                aFieldDesc.Datatype = otFieldDataType.Memo
            '                aFieldDesc.Title = "comment"
            '                aFieldDesc.ColumnName = "cmt"
            '                aFieldDesc.ID = "mst14"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_txt 1
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "parameter_txt 1"
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
            '                aFieldDesc.Datatype = otFieldDataType.Date
            '                aFieldDesc.Title = "parameter date 1 of condition"
            '                aFieldDesc.ColumnName = "param_date1"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                ' parameter_date 2
            '                aFieldDesc.Datatype = otFieldDataType.Date
            '                aFieldDesc.Title = "parameter date 2 of condition"
            '                aFieldDesc.ColumnName = "param_date2"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' parameter_date 3
            '                aFieldDesc.Datatype = otFieldDataType.Date
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
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

            '                '*** Compound Part key !!
            '                Call .AddIndex(ConstDefaultCompoundIndexName, CompundColumnNames, isprimarykey:=False)
            '                ' persist
            '                .Persist()
            '                ' change the database
            '                .CreateObjectSchema()
            '            End With

            '            CreateSchema = True
            '            Exit Function

            '            ' Handle the error
            'error_handle:
            '            Call CoreMessageHandler(subname:="ScheduleMilestone.createSchema")
            '            CreateSchema = False
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
                Call Me.Record.SetValue(ConstFNvaluedate, ConstNullDate)
                Call Me.Record.SetValue(ConstFNvaluenumeric, 0)
                Call Me.Record.SetValue(ConstFNvaluelong, 0)
                Call Me.Record.SetValue(ConstFNvaluebool, False)

                Dim avalue = DirectCast(e.DataObject, ScheduleMilestone).Value

                Select Case DirectCast(e.DataObject, ScheduleMilestone).Datatype

                    Case otFieldDataType.Numeric
                        If IsNumeric(avalue) Then Call Me.Record.SetValue(ConstFNvaluenumeric, CDbl(avalue))
                        Call Me.Record.SetValue("value", CStr(avalue))
                    Case otFieldDataType.Text, otFieldDataType.Memo
                        Call Me.Record.SetValue("value", CStr(avalue))
                    Case otFieldDataType.Runtime, otFieldDataType.Formula, otFieldDataType.Binary
                        Call CoreMessageHandler(subname:="ScheduleMilestone.persist", _
                                              message:="datatype (runtime, formular, binary) not specified how to be persisted", msglog:=s_msglog, arg1:=_datatype)
                    Case otFieldDataType.[Date]
                        If IsDate(avalue) Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CDate(avalue))
                            Call Me.Record.SetValue("value", Format(avalue, "dd.mm.yyyy"))
                        Else
                            Call Me.Record.SetValue("value", CStr(avalue))
                        End If
                    Case otFieldDataType.[Long]
                        If IsNumeric(avalue) Then Call Me.Record.SetValue(ConstFNvaluelong, CLng(avalue))
                        Call Me.Record.SetValue("value", CStr(avalue))
                    Case otFieldDataType.Timestamp
                        If IsDate(avalue) Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CDate(avalue))
                            Call Me.Record.SetValue("value", Format(avalue, "dd.mm.yyyy hh:mm:ss"))
                        Else
                            Call Me.Record.SetValue("value", CStr(avalue))
                        End If
                    Case otFieldDataType.Bool
                        If avalue = "" Or IsEmpty(avalue) Or IsNull(avalue) Or avalue Is Nothing Then
                            Call Me.Record.SetValue(ConstFNvaluebool, False)
                        ElseIf avalue = True Or avalue = False Then
                            Call Me.Record.SetValue(ConstFNvaluedate, CBool(avalue))
                        Else
                            Call Me.Record.SetValue(ConstFNvaluedate, True)
                        End If
                        Call Me.Record.SetValue("value", CStr(avalue))
                    Case Else
                        Call Me.Record.SetValue("value", CStr(avalue))
                        Call CoreMessageHandler(subname:="ScheduleMilestone.persist", _
                                              message:="datatype not specified how to be persisted", msglog:=s_msglog, arg1:=_datatype)
                End Select



            Catch ex As Exception
                Call CoreMessageHandler(subname:="ScheduleMilestone.UpdateRecord", exception:=ex)
            End Try
        End Sub

        ''' <summary>
        ''' Persist the data object to the datastore
        ''' </summary>
        ''' <param name="aTimestamp"></param>
        ''' <param name="forceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate, Optional forceSerializeToOTDB As Boolean = False) As Boolean

            If Me.Feed() Then
                '*** overload it from the Application Container
                '***
                If Me.SerializeWithHostApplication Then
                    If overwriteToHostApplication(Me.Record) Then
                        s_savedToHost = True
                        Return True
                    End If
                ElseIf forceSerializeToOTDB Or Not Me.SerializeWithHostApplication Then
                    ' persist with update timestamp
                    Return MyBase.Persist(timestamp:=timestamp, doFeedRecord:=False)
                End If
            End If


            Return False
        End Function

        ''' <summary>
        ''' create a persistable schedule milestone by primary key
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="updc"></param>
        ''' <param name="ID"></param>
        ''' <param name="FORCE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal UID As Long, ByVal updc As Long, ByVal ID As String) As Boolean
            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Create = False
                    Exit Function
                End If
            End If

            ' Check
            Dim pkarray() As Object = {UID, updc, ID}
            If MyBase.Create(pkarray, checkUnique:=True) Then
                _uid = UID
                _updc = updc
                _id = ID
                Return Me.IsCreated
            End If

            Return False

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
    <ormObject(id:=ScheduleLink.ConstObjectID, modulename:=constModuleScheduling, Version:=1, description:="link definitions between schedules and other business objects")> _
    Public Class ScheduleLink
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "ScheduleLink"

        '** Schema Table
        <ormSchemaTable(version:=1, addsparefields:=True, adddeletefieldbehavior:=True)> Public Const ConstTableID = "tblScheduleLinks"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNToTagObject, columnname2:=ConstFNToTaguid, columnname3:=ConstFNFromTagObject, columnname4:=ConstFNFromTaguid)> Public Const ConstIndTag = "used"

        '** keys
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, primarykeyordinal:=1, _
        XID:="SL1", title:="Linked From Object", description:="object link from the scheduled object")> Public Const ConstFNFromTagObject = "tagobject"
        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=2, _
            XID:="SL2", title:="Linked from UID", description:="uid link from the scheduled object")> Public Const ConstFNFromTaguid = "taguid"
        <ormObjectEntry(referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, primarykeyordinal:=3, defaultValue:="", _
            XID:="SL3", title:="Linked from Milestone", description:="uid link from the scheduled object milestone")> Public Const ConstFNFromMilestone = "ms"

        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, primarykeyordinal:=4, _
       XID:="SL4", title:="Linked to Object", description:="object link to the scheduled object")> Public Const ConstFNToTagObject = "tag2object"
        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=5, _
            XID:="SL5", title:="Linked to UID", description:="uid link to the scheduled object")> Public Const ConstFNToTaguid = "tag2uid"
        <ormObjectEntry(referenceobjectentry:=MileStoneDefinition.ConstObjectID & "." & MileStoneDefinition.ConstFNID, primarykeyordinal:=6, defaultValue:="", _
            XID:="SL6", title:="Linked to Milestone", description:="uid link to the scheduled object milestone")> Public Const ConstFNToMilestone = "2ms"


        '** fields
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, _
            XID:="SL7", title:="Linke Type", description:="object link type")> Public Const ConstFNTypeID = "typeid"

        '** Mapping
        <ormEntryMapping(EntryName:=ConstFNFromTagObject)> Private _tagObject As String
        <ormEntryMapping(EntryName:=ConstFNFromTaguid)> Private _tagUid As String
        <ormEntryMapping(EntryName:=ConstFNFromMilestone)> Private _tagMS As String
        <ormEntryMapping(EntryName:=ConstFNToTagObject)> Private _2tagObject As String
        <ormEntryMapping(EntryName:=ConstFNToTaguid)> Private _2taguid As Long
        <ormEntryMapping(EntryName:=ConstFNToMilestone)> Private _2tagMS As String
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
        ''' gets the TAG of the scheduled business object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TagObject() As String
            Get
                TagObject = _tagObject
            End Get

        End Property
        ''' <summary>
        ''' gets the TAG of the scheduled business object uid
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TagUID() As Long
            Get
                TagUID = _tagUid
            End Get

        End Property
        ''' <summary>
        ''' gets the TAG of the scheduled business object uid milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Milestone() As String
            Get
                Milestone = _tagMS
            End Get

        End Property
        ''' <summary>
        ''' gets the TAG to the scheduled business object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToTagObject() As String

            Get
                ToTagObject = _2tagObject
            End Get
            Set(value As String)
                If _2tagObject <> value Then
                    _2tagObject = value
                    _IsChanged = True
                End If
            End Set

        End Property
        ''' <summary>
        ''' gets the TAG to the scheduled business uid
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToTagUID() As Long
            Get
                ToTagUID = _2taguid
            End Get
            Set(value As Long)
                If _2taguid <> value Then
                    _2taguid = value
                    _IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the TAG to the scheduled business milestone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ToTagUIDMilestone() As String
            Get
                ToTagUIDMilestone = _2tagMS
            End Get
            Set(value As String)
                If _2tagMS <> value Then
                    _2tagMS = value
                    _IsChanged = True
                End If
            End Set
        End Property
#End Region

        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional ByVal silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of CurrentSchedule)(silent:=silent)
        End Function

    End Class


    ''' <summary>
    ''' the current schedule class links the current schedule updc  in a given workspace
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=CurrentSchedule.ConstObjectID, modulename:=ConstModuleScheduling, Version:=1, description:="pointer declaration (updc) to the current schedule in a workspace")> _
    Public Class CurrentSchedule
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "CurrentSchedule"
        '** Table Schema
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True)> Public Const ConstTableID = "tblCurrSchedule"

        '** index
        <ormSchemaIndex(columnname1:=ConstFNTagObject, columnname2:=ConstFNTaguid, columnname3:=ConstFNUID, columnname4:=ConstFNWorkspaceID)> Public Const ConstIndTag = "tags"

        '** keys
        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, primarykeyordinal:=1)> Public Const ConstFNWorkspaceID = Workspace.ConstFNID
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUid, primarykeyordinal:=4)> Public Const ConstFNUID = Schedule.ConstFNUid

        '** fields
        <ormObjectEntry(referenceobjectentry:=Schedule.ConstObjectID & "." & Schedule.ConstFNUpdc _
            )> Public Const ConstFNUPDC = Schedule.ConstFNUpdc
        <ormObjectEntry(typeid:=otFieldDataType.Bool, XID:="CS5", title:="Is Active", description:="set if active")> _
        Public Const ConstFNIsActive = "isactive"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, _
         XID:="CS2", title:="Linked Object", description:="object link to the scheduled object")> Public Const ConstFNTagObject = "tagobject"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
            XID:="CS3", title:="Linked UID", description:="uid link to the scheduled object")> Public Const ConstFNTaguid = "taguid"

        ' change FK Action since we have the workspace as FK (leads also to domians)
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.NOOP & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.NOOP & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** Mapping
        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _workspaceID As String
        <ormEntryMapping(EntryName:=ConstFNTagObject)> Private _tagObject As String
        <ormEntryMapping(EntryName:=ConstFNTaguid)> Private _tagUid As String
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
        Public Property WorkspaceID() As String
            Get
                WorkspaceID = _workspaceID
            End Get
            Set(value As String)
                If UCase(value) <> _workspaceID Then
                    _workspaceID = UCase(value)
                    Me.IsChanged = True
                End If
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
                UID = _uid
            End Get

        End Property
        ''' <summary>
        ''' gets the TAG of the scheduled business object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TagObject() As String
            Get
                TagObject = _tagObject
            End Get

        End Property
        ''' <summary>
        ''' gets the TAG of the scheduled business object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TagUID() As Long
            Get
                TagUID = _tagUid
            End Get

        End Property

        Public Property isActive() As Boolean
            Get
                isActive = _isActive
            End Get
            Set(value As Boolean)
                If value <> _isActive Then
                    _isActive = value
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
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional ByVal silent As Boolean = True) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of CurrentSchedule)(silent:=silent)


            '            Dim aFieldDesc As New ormFieldDescription
            '            Dim PrimaryColumnNames As New Collection
            '            Dim aTable As New ObjectDefinition

            '            aFieldDesc.Tablename = ConstTableID
            '            aFieldDesc.ID = ""
            '            aFieldDesc.Parameter = ""
            '            aFieldDesc.Size = 0

            '            With aTable
            '                .Create(ConstTableID)
            '                .Delete()

            '                aFieldDesc.Tablename = ConstTableID
            '                aFieldDesc.ID = ""
            '                aFieldDesc.Parameter = ""

            '                '*** UID
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "workspaceID"
            '                aFieldDesc.ID = "ws"
            '                aFieldDesc.ColumnName = "wspace"
            '                aFieldDesc.Size = 20
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                '**** UID
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "uid of deliverable"
            '                aFieldDesc.Aliases = New String() {"uid"}
            '                aFieldDesc.ID = "cs2"
            '                aFieldDesc.ColumnName = "uid"
            '                aFieldDesc.Size = 0
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

            '                '**** drev
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "revision of schedule"
            '                aFieldDesc.ID = "cs3"
            '                aFieldDesc.Aliases = New String() {"bs2"}
            '                aFieldDesc.ColumnName = "plrev"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '**** updc
            '                aFieldDesc.Datatype = otFieldDataType.[Long]
            '                aFieldDesc.Title = "update count of target"
            '                aFieldDesc.ID = "cs4"
            '                aFieldDesc.Aliases = New String() {"bs3"}
            '                aFieldDesc.ColumnName = "updc"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** isactive
            '                aFieldDesc.Datatype = otFieldDataType.Bool
            '                aFieldDesc.Title = "is an active setting"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = "cs5"
            '                aFieldDesc.ColumnName = "isactive"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***** message log tag
            '                aFieldDesc.Datatype = otFieldDataType.Text
            '                aFieldDesc.Title = "message log tag"
            '                aFieldDesc.Aliases = New String() {}
            '                aFieldDesc.ID = ""
            '                aFieldDesc.ColumnName = "msglogtag"
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                '***
            '                '*** TIMESTAMP
            '                '****
            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "last Update"
            '                aFieldDesc.ColumnName = ConstFNUpdatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '                aFieldDesc.Datatype = otFieldDataType.Timestamp
            '                aFieldDesc.Title = "creation Date"
            '                aFieldDesc.ColumnName = ConstFNCreatedOn
            '                aFieldDesc.ID = ""
            '                Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '                ' Index
            '                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

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
            '            Call CoreMessageHandler(subname:="clsOTDBCurrSchedule.createSchema", tablename:=ConstTableID)
            '            CreateSchema = False
        End Function


        ''' <summary>
        ''' retrieves a clsotdbcurrschedule from the datastore
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal UID As Long, Optional ByVal workspaceID As String = "") As CurrentSchedule
            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            Else
                workspaceID = Trim(workspaceID)
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
                If aCurrSchedule IsNot Nothing AndAlso aCurrSchedule.isActive Then
                    Return aCurrSchedule
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' loads the currschedule from the datastore
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Inject(ByVal UID As Long, Optional ByVal workspaceID As String = "") As Boolean
            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Inject = False
                    Exit Function
                End If
            End If

            ' if no workspaceID -> Default workspaceID
            If workspaceID = "" Then
                workspaceID = CurrentSession.CurrentWorkspaceID
            Else
                workspaceID = Trim(workspaceID)
            End If

            Dim aWSObj As Workspace = Workspace.Retrieve(id:=workspaceID)
            '*
            If aWSObj Is Nothing Then
                Call CoreMessageHandler(message:="Can't load workspaceID definition", _
                                      subname:="clsOTDBCurrSchedule.Inject", _
                                      arg1:=workspaceID)
                Return False
            End If

            ' check now the stack
            For Each aWorkspaceID In aWSObj.FCRelyingOn
                ' check if in workspaceID any data -> fall back to default (should be base)
                If Me.LoadUniqueBy(UID:=UID, workspaceID:=aWorkspaceID) Then
                    If Me.isActive Then
                        Return True
                    End If
                End If
            Next


            Return False

        End Function
        '**** Inject : load the object by the PrimaryKeys
        '****
        ''' <summary>
        ''' load the object by the PrimaryKeys
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadUniqueBy(ByVal UID As Long, ByVal workspaceID As String) As Boolean
            Dim pkarry() As Object = {Trim(workspaceID), UID}
            Return MyBase.Inject(pkArray:=pkarry)
        End Function

        '**** getthe TrackDef
        '****
        Public Function GetDeliverableTrack() As Track
            Dim aTrackDef As New Track
            Dim aCurrTarget As New CurrentTarget

            If IsLoaded Then
                '-> UID= ME.UID
                If Not aCurrTarget.Inject(uid:=Me.UID, workspaceID:=Me.WorkspaceID) Then
                    aCurrTarget.UPDC = 0
                End If
                If aTrackDef.Inject(deliverableUID:=Me.UID, _
                                    scheduleUID:=Me.UID, _
                                    scheduleUPDC:=Me.UPDC, _
                                    targetUPDC:=aCurrTarget.UPDC) Then
                    GetDeliverableTrack = aTrackDef
                End If
            End If

            GetDeliverableTrack = Nothing
        End Function


        '**** create : create a new Object with primary keys
        '****
        Public Function Create(ByVal UID As Long, Optional ByVal workspaceID As String = "") As Boolean
            If workspaceID = "" Then workspaceID = CurrentSession.CurrentWorkspaceID
            If MyBase.Create({workspaceID, UID}, checkUnique:=True) Then
                _isActive = True
                Return True
            Else
                Return False
            End If
        End Function

    End Class

End Namespace
