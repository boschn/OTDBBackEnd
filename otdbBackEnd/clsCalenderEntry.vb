REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** BUSINESS OBJECTs: Calendar Classes for On Track Database Backend Library
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

Imports OnTrack.Database

Namespace OnTrack


    '************************************************************************************
    '***** CLASS clsOTDBCalenderEntry is the object for a OTDBRecord (which is the datastore)
    '***** describes an Entry in the Calendar of <name>
    '*****

    ''' <summary>
    ''' Calendar Entry Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOTDBCalendarEntry
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const constTableid As String = "tblCalendarEntries"

        ' keys
        Private s_entryid As Long = 0
        Private s_name As String = ""
        Private s_timestamp As Date = ConstNullDate
        Private s_refid As Long = 0
        Private s_length As Long = 0
        ' fields
        Private s_EntryType As otCalendarEntryType
        Private s_isImportant As Boolean = False
        Private s_notAvailable As Boolean = False
        Private s_description As String = ""

        '
        Private s_parameter_txt1 As String = ""
        Private s_parameter_txt2 As String = ""
        Private s_parameter_txt3 As String = ""
        Private s_parameter_num1 As Double = 0
        Private s_parameter_num2 As Double = 0
        Private s_parameter_num3 As Double = 0
        Private s_parameter_date1 As Date = ConstNullDate
        Private s_parameter_date2 As Date = ConstNullDate
        Private s_parameter_date3 As Date = ConstNullDate
        Private s_parameter_flag1 As Boolean = False
        Private s_parameter_flag2 As Boolean = False
        Private s_parameter_flag3 As Boolean = False

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableid)
        End Sub

#Region "properties"


        ReadOnly Property Name() As String
            Get
                Name = s_name
            End Get

        End Property

        ReadOnly Property ID() As Long
            Get
                ID = s_entryid
            End Get

        End Property
        Public Property entrytype() As otCalendarEntryType
            Get
                entrytype = s_EntryType
            End Get
            Set(value As otCalendarEntryType)
                s_EntryType = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Timestamp entry of the calendar
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Timestamp() As Date
            Get
                Timestamp = s_timestamp
            End Get
            Set(value As Date)
                s_timestamp = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns or sets the date portion of the timestamp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datevalue() As Date
            Get
                Datevalue = s_timestamp.Date
            End Get
            Set(value As Date)
                s_timestamp = New DateTime(year:=value.Year, month:=value.Month, day:=value.Day, _
                                           hour:=s_timestamp.Hour, minute:=s_timestamp.Minute, [second]:=s_timestamp.Second, millisecond:=s_timestamp.Millisecond)
                's_timestamp = CDate(Format(value, "dd.mm.yyyy") & " " & Format(CDate(s_timestamp), "hh:mm"))
                Me.IsChanged = True
            End Set
        End Property

        ' length of an entry in minutes
        Public Property Length() As Long
            Get
                Length = s_length
            End Get
            Set(value As Long)
                s_length = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the Timeportion
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Timevalue() As TimeSpan
            Get
                Return s_timestamp.TimeOfDay
            End Get
            Set(value As TimeSpan)
                s_timestamp = New DateTime(year:=s_timestamp.Year, month:=s_timestamp.Month, day:=s_timestamp.Day, _
                                          hour:=value.Hours, minute:=value.Minutes, [second]:=value.Seconds, millisecond:=value.Milliseconds)


                Me.IsChanged = True
            End Set
        End Property

        Public Property description() As String
            Get
                description = s_description
            End Get
            Set(value As String)
                s_description = value
                Me.IsChanged = True
            End Set
        End Property

        ReadOnly Property weekofyear() As String
            Get
                Dim myear As Integer
                myear = Me.year
                If Me.month = 1 And Me.week >= 52 Then
                    myear = myear - 1
                End If

                weekofyear = CStr(myear) & "-" & Format(DatePart("ww", s_timestamp, vbMonday, vbFirstFourDays), "0#")
            End Get

        End Property

        ReadOnly Property week() As Integer
            Get
                week = DatePart("ww", s_timestamp, vbMonday, vbFirstFourDays)
            End Get
        End Property
        ReadOnly Property weekday() As DayOfWeek
            Get
                weekday = DatePart("w", s_timestamp)
            End Get
        End Property

        ReadOnly Property dayofyear() As Integer
            Get
                dayofyear = DatePart("y", s_timestamp)
            End Get

        End Property
        ReadOnly Property dayofmonth() As Integer
            Get
                dayofmonth = DatePart("d", s_timestamp)
            End Get

        End Property

        ReadOnly Property month() As Integer
            Get
                month = DatePart("m", s_timestamp)
            End Get

        End Property
        ReadOnly Property year() As Integer
            Get
                year = DatePart("yyyy", s_timestamp)
            End Get

        End Property
        ReadOnly Property quarter() As Integer
            Get
                quarter = DatePart("q", s_timestamp)
            End Get

        End Property

        ReadOnly Property hour() As Integer
            Get
                hour = DatePart("h", s_timestamp)
            End Get

        End Property
        ReadOnly Property minute() As Integer
            Get
                minute = DatePart("m", s_timestamp)
            End Get

        End Property

        Public Property isImportant() As Boolean
            Get
                isImportant = s_isImportant
            End Get
            Set(value As Boolean)
                s_isImportant = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property notAvailable() As Boolean
            Get
                notAvailable = s_notAvailable
            End Get
            Set(value As Boolean)
                s_notAvailable = value
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

        Public Function deltaYear(newDate As Date) As Long
            deltaYear = DateDiff("y", s_timestamp, newDate)
        End Function
        Public Function deltaMonth(newDate As Date) As Long
            deltaMonth = DateDiff("m", s_timestamp, newDate)
        End Function
        Public Function deltaWeek(newDate As Date) As Long
            deltaWeek = DateDiff("ww", s_timestamp, newDate)
        End Function

        Public Function deltaDay(ByVal newDate As Date, _
        Optional ByVal considerAvailibilty As Boolean = True, _
        Optional ByVal calendarname As String = "") As Long
            Dim anEntry As New clsOTDBCalendarEntry
            Dim currDate As Date
            Dim delta As Long
            Dim exitflag As Boolean
            ' delta
            delta = 0
            deltaDay = DateDiff("d", s_timestamp, newDate)
            '
            If considerAvailibilty Then
                currDate = s_timestamp
                If deltaDay < 0 Then
                    delta = -AvailableDays(fromdate:=newDate, untildate:=s_timestamp, name:=calendarname)
                ElseIf deltaDay > 0 Then
                    delta = AvailableDays(fromdate:=s_timestamp, untildate:=newDate, name:=calendarname)
                Else : delta = 0
                End If
                ' if the new date is not available
                'If Not anEntry.isAvailableOn(newDate, name:=calendarname) Then
                '    If deltaDay < 0 And delta <> 0 Then
                '        delta = delta + 1
                '    ElseIf deltaDay > 0 And delta <> 0 Then
                '        delta = delta - 1
                '    End If
                'End If
                deltaDay = delta
                Exit Function
            End If

        End Function
        Public Function deltaHour(newDate As Date) As Long
            deltaHour = DateDiff("h", s_timestamp, newDate)
        End Function
        Public Function deltaMinute(newDate As Date) As Long
            deltaMinute = DateDiff("m", s_timestamp, newDate)
        End Function

        Public Function addYear(aVAlue As Integer) As Date
            addYear = DateAdd("y", aVAlue, s_timestamp)
        End Function
        Public Function addMonth(aVAlue As Integer) As Date
            addMonth = DateAdd("m", aVAlue, s_timestamp)
        End Function
        Public Function addWeek(aVAlue As Integer) As Date
            addWeek = DateAdd("ww", aVAlue, s_timestamp)
        End Function
        Public Function addDay(ByVal aVAlue As Integer, _
        Optional ByVal considerAvailibilty As Boolean = True, _
        Optional ByVal calendarname As String = "") As Date
            Dim anEntry As New clsOTDBCalendarEntry
            Dim currDate As Date
            Dim newDate As Date
            Dim delta As Long
            Dim exitflag As Boolean
            ' delta
            addDay = DateAdd("d", aVAlue, s_timestamp)
            '
            If considerAvailibilty Then
                currDate = s_timestamp
                addDay = Me.NextAvailableDate(currDate, aVAlue, calendarname)
                Exit Function
            End If

        End Function
        Public Function addHour(aVAlue As Integer) As Date
            addHour = DateAdd("h", aVAlue, s_timestamp)
        End Function
        Public Function addMinute(aVAlue As Integer) As Date
            addMinute = DateAdd("m", aVAlue, s_timestamp)
        End Function

        Public Function incYear(aVAlue As Integer) As Date
            Me.Timestamp = Me.addYear(aVAlue)
            incYear = Me.Timestamp
        End Function
        Public Function incMonth(aVAlue As Integer) As Date
            Me.Timestamp = Me.addMonth(aVAlue)
            incMonth = Me.Timestamp
        End Function
        Public Function incDay(aVAlue As Integer) As Date
            Me.Timestamp = Me.addDay(aVAlue)
            incDay = Me.Timestamp
        End Function
        Public Function incWeek(aVAlue As Integer) As Date
            Me.Timestamp = Me.addWeek(aVAlue)
            incWeek = Me.Timestamp
        End Function
        Public Function incHour(aVAlue As Integer) As Date
            Me.Timestamp = Me.addHour(aVAlue)
            incHour = Me.Timestamp
        End Function
        Public Function incMinute(aVAlue As Integer) As Date
            Me.Timestamp = Me.addMinute(aVAlue)
            incMinute = Me.Timestamp
        End Function

        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            Call registerCacheFor(constTableid)

            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)

            s_timestamp = ConstNullDate
            s_parameter_date1 = ConstNullDate
            s_parameter_date2 = ConstNullDate
            s_parameter_date3 = ConstNullDate
            s_refid = 0
            s_entryid = 0
            Return MyBase.Initialize
        End Function

        ''' <summary>
        ''' Infuse the object by the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            Try

                s_name = CStr(record.GetValue("cname"))
                s_entryid = CLng(record.GetValue("id"))
                s_refid = CLng(record.GetValue("refid"))
                s_length = CLng(record.GetValue("length"))
                s_description = CStr(record.GetValue("desc"))
                s_timestamp = CDate(record.GetValue("timestamp"))
                s_EntryType = CInt(record.GetValue("typeid"))
                s_isImportant = CBool(record.GetValue("isimp"))
                s_notAvailable = CBool(record.GetValue("notavail"))
                s_description = CStr(record.GetValue("desc"))

                s_parameter_txt1 = CStr(record.GetValue("param_txt1"))
                s_parameter_txt2 = CStr(record.GetValue("param_txt2"))
                s_parameter_txt3 = CStr(record.GetValue("param_txt3"))
                s_parameter_num1 = CDbl(record.GetValue("param_num1"))
                s_parameter_num2 = CDbl(record.GetValue("param_num2"))
                s_parameter_num3 = CDbl(record.GetValue("param_num3"))
                s_parameter_date1 = CDate(record.GetValue("param_date1"))
                s_parameter_date2 = CDate(record.GetValue("param_date2"))
                s_parameter_date3 = CDate(record.GetValue("param_date3"))
                s_parameter_flag1 = CBool(record.GetValue("param_flag1"))
                s_parameter_flag2 = CBool(record.GetValue("param_flag2"))
                s_parameter_flag3 = CBool(record.GetValue("param_flag3"))

                Return MyBase.Infuse(record)

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBCalendarEntry.Infuse")
                Return False
            End Try


        End Function

        ''' <summary>
        ''' loads and infuses the object
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal name As String, ByVal ID As Long) As Boolean
            Dim primarykey() As Object = {name, ID}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' create persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean


            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim OrderByColumnNames As New Collection
            Dim RefByColumnNames As New Collection
            Dim aTable As New ObjectDefinition

            With aTable

                .Create(constTableid)

                aFieldDesc.Tablename = constTableid
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""

                '***
                '*** Fields
                '****
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "calender name"
                aFieldDesc.ID = "cal1"
                aFieldDesc.ColumnName = "cname"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
                OrderByColumnNames.Add(aFieldDesc.ColumnName)
                RefByColumnNames.Add(aFieldDesc.ColumnName)


                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "reference entry id"
                aFieldDesc.ID = "cal18"
                aFieldDesc.ColumnName = "refid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                RefByColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "entry id"
                aFieldDesc.ID = "cal2"
                aFieldDesc.ColumnName = "id"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
                RefByColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "Timestamp"
                aFieldDesc.ID = "cal4"
                aFieldDesc.ColumnName = "timestamp"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                OrderByColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "entry type id"
                aFieldDesc.ID = "cal5"
                aFieldDesc.ColumnName = "typeid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                OrderByColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ID = "cal16"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "length in min"
                aFieldDesc.ID = "cal19"
                aFieldDesc.ColumnName = "length"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "week of the year"
                aFieldDesc.ID = "cal6"
                aFieldDesc.ColumnName = "noweek"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "day of year"
                aFieldDesc.ID = "cal7"
                aFieldDesc.ColumnName = "noday"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "number of weekday"
                aFieldDesc.ID = "cal8"
                aFieldDesc.ColumnName = "noweekday"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "year"
                aFieldDesc.ID = "cal9"
                aFieldDesc.ColumnName = "year"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "month"
                aFieldDesc.ID = "cal10"
                aFieldDesc.ColumnName = "month"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "day of month"
                aFieldDesc.ID = "cal11"
                aFieldDesc.ColumnName = "day"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Time
                aFieldDesc.Title = "timestamp"
                aFieldDesc.ID = "cal12"
                aFieldDesc.ColumnName = "timevalue"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Date
                aFieldDesc.Title = "timestamp"
                aFieldDesc.ID = "cal13"
                aFieldDesc.ColumnName = "datevalue"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "not available"
                aFieldDesc.ID = "cal14"
                aFieldDesc.ColumnName = "notavail"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is important"
                aFieldDesc.ID = "cal15"
                aFieldDesc.ColumnName = "isimp"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "quarter"
                aFieldDesc.ID = "cal17"
                aFieldDesc.ColumnName = "quarter"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "week and year"
                aFieldDesc.ID = "cal20"
                aFieldDesc.ColumnName = "weekofyear"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 1
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 1 of condition"
                aFieldDesc.ColumnName = "param_txt1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 2
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 2 of condition"
                aFieldDesc.ColumnName = "param_txt2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_txt 2
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "parameter_txt 3 of condition"
                aFieldDesc.ColumnName = "param_txt3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_num 1
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 1 of condition"
                aFieldDesc.ColumnName = "param_num1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_num 2
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 2 of condition"
                aFieldDesc.ColumnName = "param_num2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_num 2
                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "parameter numeric 3 of condition"
                aFieldDesc.ColumnName = "param_num3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_date 1
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 1 of condition"
                aFieldDesc.ColumnName = "param_date1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_date 2
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 2 of condition"
                aFieldDesc.ColumnName = "param_date2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_date 3
                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "parameter date 3 of condition"
                aFieldDesc.ColumnName = "param_date3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' parameter_flag 1
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 1 of condition"
                aFieldDesc.ColumnName = "param_flag1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_flag 2
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 2 of condition"
                aFieldDesc.ColumnName = "param_flag2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' parameter_flag 3
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "parameter flag 3 of condition"
                aFieldDesc.ColumnName = "param_flag3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                Call .AddIndex("OrderbyDate", OrderByColumnNames, isprimarykey:=False)
                Call .AddIndex("OrderbyRefID", RefByColumnNames, isprimarykey:=False)

                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            '
            CreateSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBDefScheduleMilestone.createSchema", tablename:=constTableid)
            CreateSchema = False
        End Function

        ''' <summary>
        ''' persist the object to the tablestore
        ''' </summary>
        ''' <param name="timestamp">timestamp to use</param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean


            Try
                Call Me.Record.SetValue("cname", s_name)
                Call Me.Record.SetValue("id", s_entryid)
                Call Me.Record.SetValue("refid", s_refid)
                Call Me.Record.SetValue("length", s_length)
                Call Me.Record.SetValue("desc", s_description)
                Call Me.Record.SetValue("timestamp", s_timestamp)
                Call Me.Record.SetValue("typeid", s_EntryType)
                Call Me.Record.SetValue("isimp", s_isImportant)
                Call Me.Record.SetValue("notavail", s_notAvailable)

                Call Me.Record.SetValue("year", Me.year)
                Call Me.Record.SetValue("month", Me.month)
                Call Me.Record.SetValue("day", Me.dayofmonth)
                Call Me.Record.SetValue("noweek", Me.week)
                Call Me.Record.SetValue("noday", Me.dayofyear)
                Call Me.Record.SetValue("quarter", Me.quarter)
                Call Me.Record.SetValue("datevalue", Me.Datevalue)
                Call Me.Record.SetValue("timevalue", Me.Timevalue)
                Call Me.Record.SetValue("noweekday", Me.weekday)
                Call Me.Record.SetValue("weekofyear", Me.weekofyear)

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
                Call CoreMessageHandler(subname:="clsOTDBCalendarEntry.Persist", exception:=ex, tablename:=Me.TableID, _
                                      messagetype:=otCoreMessageType.InternalException)

            End Try


        End Function
        ''' <summary>
        ''' Return a Collection of all Calendar Entries
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of clsOTDBCalendarEntry)
            Return ormDataObject.All(Of clsOTDBCalendarEntry)()
        End Function

        ''' <summary>
        ''' Returns the number of available days between two dates
        ''' </summary>
        ''' <param name="fromdate"></param>
        ''' <param name="untildate"></param>
        ''' <param name="name">default calendar</param>
        ''' <returns>days in long</returns>
        ''' <remarks></remarks>
        Public Shared Function AvailableDays(ByVal fromdate As Date, ByVal untildate As Date, Optional ByVal name As String = "") As Long

            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If

            '**
            Try
                Dim aStore = GetTableStore(constTableid)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="availabledays", addAllFields:=False, addMe:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = "count(id)"
                    aCommand.Where = "cname=@cname and timestamp > @date1 and timestamp <@date2 and notavail <> @avail and typeid=@typeID"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", fieldname:="cname", tablename:=constTableid))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date1", datatype:=otFieldDataType.Date, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date2", datatype:=otFieldDataType.Date, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@avail", datatype:=otFieldDataType.Bool, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@typeid", datatype:=otFieldDataType.[Long], notColumn:=True))
                    aCommand.Prepare()
                End If

                '** values
                aCommand.SetParameterValue(ID:="@cnamd", value:=name)
                aCommand.SetParameterValue(ID:="@date1", value:=fromdate)
                aCommand.SetParameterValue(ID:="@date2", value:=untildate)
                aCommand.SetParameterValue(ID:="@typeid", value:=True)
                aCommand.SetParameterValue(ID:="@typeid", value:=otCalendarEntryType.DayEntry)

                Dim resultRecords As List(Of ormRecord) = aCommand.RunSelect

                If resultRecords.Count > 0 Then
                    If Not IsNull(resultRecords.Item(0).GetValue(1)) And IsNumeric(resultRecords.Item(0).GetValue(1)) Then
                        AvailableDays = CLng(resultRecords.Item(0).GetValue(1)) + 1
                    Else
                        AvailableDays = 0
                    End If
                Else
                    AvailableDays = 0
                End If

                Return AvailableDays

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBCalendarEntry.AvailableDays")
                Return -1
            End Try

        End Function
        ''' <summary>
        ''' returnss the next available date from a date in no of  days
        ''' </summary>
        ''' <param name="fromdate">From Date</param>
        ''' <param name="noDays">number of days</param>
        ''' <param name="Name">default calendar</param>
        ''' <returns>next date</returns>
        ''' <remarks></remarks>
        Public Shared Function NextAvailableDate(ByVal fromdate As Date, ByVal noDays As Integer, Optional ByVal name As String = "") As Date

            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If

            '**
            Try
                Dim aStore = GetTableStore(constTableid)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="nextavailabledate", addAllFields:=False, addMe:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = "[timestamp]"
                    If noDays < 0 Then
                        aCommand.Where = "cname=@cname and timestamp < @date1  and notavail <> @avail and typeid=@typeID"
                        aCommand.OrderBy = "[" & constTableid & ".timestamp] desc"
                    Else
                        aCommand.Where = "cname=@cname and timestamp > @date1  and notavail <> @avail and typeid=@typeID"
                        aCommand.OrderBy = "[" & constTableid & ".timestamp] asc"
                    End If

                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", fieldname:="cname", tablename:=constTableid))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date1", datatype:=otFieldDataType.Date, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@avail", datatype:=otFieldDataType.Bool, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@typeid", datatype:=otFieldDataType.[Long], notColumn:=True))
                    aCommand.Prepare()
                End If

                '** values
                aCommand.SetParameterValue(ID:="@cnamd", value:=name)
                aCommand.SetParameterValue(ID:="@date1", value:=fromdate)
                aCommand.SetParameterValue(ID:="@typeid", value:=True)
                aCommand.SetParameterValue(ID:="@typeid", value:=otCalendarEntryType.DayEntry)

                Dim resultRecords As List(Of ormRecord) = aCommand.RunSelect

                If resultRecords.Count > noDays Then
                    NextAvailableDate = resultRecords.Item(noDays - 1).GetValue(1)
                Else
                    Call CoreMessageHandler(subname:="clsOTDBCalendarentry.nextavailableDate", message:="requested no of days is behind calendar end - regenerate calendar", _
                                           messagetype:=otCoreMessageType.ApplicationError, arg1:=noDays)
                    NextAvailableDate = resultRecords.Last.GetValue(1)
                End If

                Return NextAvailableDate

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBCalendarEntry.nextAvailableDate")
                Return ConstNullDate
            End Try



        End Function

        '****** isAvailable looks for otDayEntries showing availibility
        '******
        ''' <summary>
        ''' isAvailable looks for otDayEntries showing availibility
        ''' </summary>
        ''' <param name="refdate"></param>
        ''' <param name="Name">default calendar</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsAvailableOn(ByVal refdate As Date, Optional ByVal name As String = "") As Boolean
            Dim aCollection As New Collection
            Dim anEntry As New clsOTDBCalendarEntry

            If IsMissing(name) Then
                name = CurrentSession.DefaultCalendarName
            Else
                name = CStr(name)
            End If

            aCollection = AllByDate(name:=name, refDate:=refdate)
            If aCollection Is Nothing Or aCollection.Count = 0 Then
                IsAvailableOn = True
                Exit Function
            End If

            For Each anEntry In aCollection
                If anEntry.entrytype = otCalendarEntryType.DayEntry Or anEntry.entrytype = otCalendarEntryType.AbsentEntry Then
                    If anEntry.notAvailable Then
                        IsAvailableOn = False
                        Exit Function
                    End If
                End If
            Next anEntry

            IsAvailableOn = True
        End Function

        ''' <summary>
        ''' returns all calendar entries by refence date
        ''' </summary>
        ''' <param name="refDate"></param>
        ''' <param name="name"></param>
        ''' <returns>a collection of objects</returns>
        ''' <remarks></remarks>
        Public Shared Function AllByDate(ByVal refDate As Date, Optional ByVal name As String = "") As Collection
            Dim aCollection As New Collection
            Dim aTable As iormDataStore


            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If

            Try

                aTable = GetTableStore(constTableid)

                'wherestr = "timestamp = #" & Format(refDate, "mm-dd-yyyy") & "# and cname='" & Name & "'"
                'aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand("AllByDate")
                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.AddTable(constTableid, addAllFields:=True)
                    '** Depends on the server
                    If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer Then
                        aCommand.Where = " cname = @cname and convert(varchar, [timestamp], 104) = @datestr;"
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, fieldname:="cname"))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@datestr", datatype:=otFieldDataType.Text, notColumn:=True))
                    Else
                        aCommand.Where = " cname = @cname and [timestamp] = @date;"
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, fieldname:="cname"))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date", datatype:=otFieldDataType.[Date], notColumn:=True))
                    End If


                    aCommand.Prepare()
                End If

                ' set Parameter
                aCommand.SetParameterValue("@cname", name)
                If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer Then
                    aCommand.SetParameterValue("@datestr", Format(refDate, "dd.MM.yyyy"))
                Else
                    aCommand.SetParameterValue("@date", refDate)
                End If

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                If theRecords.Count >= 0 Then

                    For Each aRecord As ormRecord In theRecords
                        Dim aNewObject As New clsOTDBCalendarEntry
                        aNewObject = New clsOTDBCalendarEntry
                        If aNewObject.Infuse(aRecord) Then
                            aCollection.Add(Item:=aNewObject)
                        End If
                    Next aRecord

                End If
                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBCalendarEntry.AllByDate", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                Return aCollection
            End Try

        End Function

        ''' <summary>
        ''' Initialize the calendar with dates from a date until a date
        ''' </summary>
        ''' <param name="fromdate">from date to initalize</param>
        ''' <param name="untildate">to date </param>
        ''' <param name="name">name of the calendar (optional)</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function GenerateDays(ByVal fromdate As Date, ByVal untildate As Date, Optional ByVal name As String = "") As Boolean
            Dim aCollection As New Collection
            Dim currDate As Date
            Dim anEntry As New clsOTDBCalendarEntry

            ' calendar name
            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If

            ' start
            currDate = fromdate
            Do While currDate <= untildate

                'exists ?
                aCollection = clsOTDBCalendarEntry.AllByDate(refDate:=currDate, name:=name)
                If aCollection Is Nothing Or aCollection.Count = 0 Then
                    anEntry = New clsOTDBCalendarEntry
                    With anEntry
                        .Create(name)
                        .Datevalue = currDate
                        .notAvailable = False
                        .description = "working day"
                        ' weekend
                        If .weekday = vbSaturday Or .weekday = vbSunday Then
                            .notAvailable = True
                            .description = "weekend"
                        Else
                            .notAvailable = False
                        End If
                        ' new year
                        If .month = 1 And (.dayofmonth = 1) Then
                            .notAvailable = True
                            .description = "new year"
                        ElseIf .month = 10 And .dayofmonth = 3 Then
                            .notAvailable = True
                            .description = "reunifcation day in germany"
                        ElseIf .month = 5 And .dayofmonth = 1 Then
                            .notAvailable = True
                            .description = "labor day in germany"
                        ElseIf .month = 11 And .dayofmonth = 1 Then
                            .notAvailable = True
                            .description = "allerseelen in germany"
                            ' christmas
                        ElseIf .month = 12 And (.dayofmonth = 24 Or .dayofmonth = 26 Or .dayofmonth = 25) Then
                            .notAvailable = True
                            .description = "christmas"
                        End If
                        .entrytype = otCalendarEntryType.DayEntry
                        .Persist()
                    End With
                End If

                ' inc
                currDate = DateAdd("d", 1, currDate)
            Loop

            Return True

        End Function

        ''' <summary>
        ''' Creates an persistable calendar entry
        ''' </summary>
        ''' <param name="name">name of calendar</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(Optional ByVal name As String = "", Optional entryid As Long = 0) As Boolean
            Dim primarykey() As Object = {name, entryid}
            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If


            '** create the key
            If entryid = 0 Then
                Dim pkarray() As Object = {name, Nothing}
                If Not Me.TableStore.CreateUniquePkValue(pkarray) Then
                    Call CoreMessageHandler(message:="unique key couldnot be created", subname:="clsOTDBCalendarEntry.Create", arg1:=name, _
                                                tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                primarykey = pkarray
            End If
            If Not MyBase.Create(primarykey) Then
                Return False
            End If

            ' set the primaryKey
            s_name = LCase(name)
            Try
                s_entryid = CLng(primarykey(1))
            Catch ex As Exception
                Call CoreMessageHandler(message:="unique id couldnot be retrieved", subname:="clsOTDBCalendarEntry.Create", arg1:=primarykey, _
                                               tablename:=TableID, entryname:="entryid", messagetype:=otCoreMessageType.InternalError)
                Return False
            End Try


            Return Me.IsCreated
        End Function

    End Class
End Namespace