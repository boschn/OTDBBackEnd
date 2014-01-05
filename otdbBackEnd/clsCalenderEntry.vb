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
    Public Class CalendarEntry
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** Schema
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addDomainBehavior:=True, addsparefields:=True)> _
        Public Const ConstTableid As String = "tblCalendarEntries"

        <ormSchemaIndex(columnname1:=constFNName, columnname2:=constFNRefID, columnname3:=constFNID, columnname4:=constFNDomainID)> Public Const constINDEXRefID = "refid"
        <ormSchemaIndex(columnname1:=constFNName, columnname2:=constFNTimestamp, columnname3:=ConstFNTypeID, columnname4:=constFNDomainID)> Public Const constIndexType = "typeid"

        ' keys
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=50, primarykeyordinal:=1, _
            ID:="CAL1", title:="Name", description:="name of calendar")> Public Const constFNName = "cname"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, primarykeyordinal:=2, _
           ID:="CAL2", title:="EntryNo", description:="entry no in the calendar")> Public Const constFNID = "id"
        <ormSchemaColumn(referenceObjecTEntry:=Domain.ConstTableID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3)> _
        Public Shadows Const constFNDomainID = Domain.ConstFNDomainID

        <ormSchemaColumn(typeid:=otFieldDataType.Timestamp, _
         ID:="CAL4", title:="Timestamp", description:="timestamp entry in the calendar")> Public Const constFNTimestamp = "timestamp"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
         ID:="CAL5", title:="Type", description:="entry type in the calendar")> Public Const ConstFNTypeID = "typeid"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=255, _
        ID:="CAL6", title:="Description", description:="entry description in the calendar")> Public Const ConstFNDescription = "desc"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
          ID:="CAL8", title:="RefID", description:="entry refID in the calendar")> Public Const constFNRefID = "refid"

        <ormSchemaColumn(typeid:=otFieldDataType.Bool, iD:="cal9", title:="Not Available", description:="not available")> _
        Public Const constFNNotAvail = "notavail"
        <ormSchemaColumn(typeid:=otFieldDataType.Bool, iD:="cal10", title:="Is Important", description:="is important entry (prioritized)")> _
        Public Const constFNIsImportant = "isimp"

        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
          ID:="CAL20", title:="TimeSpan", description:="length in minutes")> Public Const constFNLength = "length"


        '** not mapped

        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
          ID:="CAL31", title:="Week", description:="week of the year")> Public Const constFNNoWeek = "noweek"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
         ID:="CAL32", title:="Day", description:="day of the year")> Public Const constFNNoDay = "noday"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
         ID:="CAL33", title:="Weekday", description:="number of day in the week")> Public Const constFNweekday = "noweekday"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
         ID:="CAL34", title:="Quarter", description:="no of quarter of the year")> Public Const constFNQuarter = "quarter"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
         ID:="CAL35", title:="Year", description:="the year")> Public Const constFNYear = "year"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
        ID:="CAL36", title:="Month", description:="the month")> Public Const constFNmonth = "month"
        <ormSchemaColumn(typeid:=otFieldDataType.Long, _
        ID:="CAL37", title:="Day", description:="the day")> Public Const constFNDay = "day"
        <ormSchemaColumn(typeid:=otFieldDataType.Time, _
        ID:="CAL38", title:="Time", description:="time")> Public Const constFNTime = "timevalue"
        <ormSchemaColumn(typeid:=otFieldDataType.Date, _
        ID:="CAL39", title:="Date", description:="date")> Public Const constFNDate = "datevalue"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=10, _
        ID:="CAL40", title:="WeekYear", description:="Week and Year representation")> Public Const constFNWeekYear = "weekofyear"


        '** mappings
        <ormColumnMapping(ColumnName:=ConstFNID)> Private _entryid As Long = 0
        <ormColumnMapping(ColumnName:=ConstFNName)> Private _name As String = ""
        <ormColumnMapping(ColumnName:=ConstFNTimestamp)> Private _timestamp As Date = ConstNullDate
        <ormColumnMapping(ColumnName:=ConstFNRefID)> Private _refid As Long = 0
        <ormColumnMapping(ColumnName:=ConstFNLength)> Private _length As Long = 0
        ' fields
        <ormColumnMapping(ColumnName:=ConstFNTypeID)> Private _EntryType As otCalendarEntryType
        <ormColumnMapping(ColumnName:=ConstFNIsImportant)> Private s_isImportant As Boolean = False
        <ormColumnMapping(ColumnName:=ConstFNNotAvail)> Private s_notAvailable As Boolean = False
        <ormColumnMapping(ColumnName:=ConstFNDescription)> Private s_description As String = ""



        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableid)
        End Sub

#Region "Properties"


        ReadOnly Property Name() As String
            Get
                Name = _name
            End Get

        End Property

        ReadOnly Property ID() As Long
            Get
                ID = _entryid
            End Get

        End Property
        Public Property entrytype() As otCalendarEntryType
            Get
                entrytype = _EntryType
            End Get
            Set(value As otCalendarEntryType)
                _EntryType = value
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
                Timestamp = _timestamp
            End Get
            Set(value As Date)
                _timestamp = value
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
                Datevalue = _timestamp.Date
            End Get
            Set(value As Date)
                _timestamp = New DateTime(year:=value.Year, month:=value.Month, day:=value.Day, _
                                           hour:=_timestamp.Hour, minute:=_timestamp.Minute, [second]:=_timestamp.Second, millisecond:=_timestamp.Millisecond)
                's_timestamp = CDate(Format(value, "dd.mm.yyyy") & " " & Format(CDate(s_timestamp), "hh:mm"))
                Me.IsChanged = True
            End Set
        End Property

        ' length of an entry in minutes
        Public Property Length() As Long
            Get
                Length = _length
            End Get
            Set(value As Long)
                _length = value
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
                Return _timestamp.TimeOfDay
            End Get
            Set(value As TimeSpan)
                _timestamp = New DateTime(year:=_timestamp.Year, month:=_timestamp.Month, day:=_timestamp.Day, _
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

                weekofyear = CStr(myear) & "-" & Format(DatePart("ww", _timestamp, vbMonday, vbFirstFourDays), "0#")
            End Get

        End Property

        ReadOnly Property week() As Integer
            Get
                week = DatePart("ww", _timestamp, vbMonday, vbFirstFourDays)
            End Get
        End Property
        ReadOnly Property weekday() As DayOfWeek
            Get
                weekday = DatePart("w", _timestamp)
            End Get
        End Property

        ReadOnly Property dayofyear() As Integer
            Get
                dayofyear = DatePart("y", _timestamp)
            End Get

        End Property
        ReadOnly Property dayofmonth() As Integer
            Get
                dayofmonth = DatePart("d", _timestamp)
            End Get

        End Property

        ReadOnly Property month() As Integer
            Get
                month = DatePart("m", _timestamp)
            End Get

        End Property
        ReadOnly Property year() As Integer
            Get
                year = DatePart("yyyy", _timestamp)
            End Get

        End Property
        ReadOnly Property quarter() As Integer
            Get
                quarter = DatePart("q", _timestamp)
            End Get

        End Property

        ReadOnly Property hour() As Integer
            Get
                hour = DatePart("h", _timestamp)
            End Get

        End Property
        ReadOnly Property minute() As Integer
            Get
                minute = DatePart("m", _timestamp)
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

#End Region

        Public Function deltaYear(newDate As Date) As Long
            deltaYear = DateDiff("y", _timestamp, newDate)
        End Function
        Public Function deltaMonth(newDate As Date) As Long
            deltaMonth = DateDiff("m", _timestamp, newDate)
        End Function
        Public Function deltaWeek(newDate As Date) As Long
            deltaWeek = DateDiff("ww", _timestamp, newDate)
        End Function

        Public Function deltaDay(ByVal newDate As Date, _
        Optional ByVal considerAvailibilty As Boolean = True, _
        Optional ByVal calendarname As String = "") As Long
            Dim anEntry As New CalendarEntry
            Dim currDate As Date
            Dim delta As Long
            Dim exitflag As Boolean
            ' delta
            delta = 0
            deltaDay = DateDiff("d", _timestamp, newDate)
            '
            If considerAvailibilty Then
                currDate = _timestamp
                If deltaDay < 0 Then
                    delta = -AvailableDays(fromdate:=newDate, untildate:=_timestamp, name:=calendarname)
                ElseIf deltaDay > 0 Then
                    delta = AvailableDays(fromdate:=_timestamp, untildate:=newDate, name:=calendarname)
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
            deltaHour = DateDiff("h", _timestamp, newDate)
        End Function
        Public Function deltaMinute(newDate As Date) As Long
            deltaMinute = DateDiff("m", _timestamp, newDate)
        End Function

        Public Function addYear(aVAlue As Integer) As Date
            addYear = DateAdd("y", aVAlue, _timestamp)
        End Function
        Public Function addMonth(aVAlue As Integer) As Date
            addMonth = DateAdd("m", aVAlue, _timestamp)
        End Function
        Public Function addWeek(aVAlue As Integer) As Date
            addWeek = DateAdd("ww", aVAlue, _timestamp)
        End Function
        Public Function addDay(ByVal aVAlue As Integer, _
        Optional ByVal considerAvailibilty As Boolean = True, _
        Optional ByVal calendarname As String = "") As Date
            Dim anEntry As New CalendarEntry
            Dim currDate As Date
            Dim newDate As Date
            Dim delta As Long
            Dim exitflag As Boolean
            ' delta
            addDay = DateAdd("d", aVAlue, _timestamp)
            '
            If considerAvailibilty Then
                currDate = _timestamp
                addDay = Me.NextAvailableDate(currDate, aVAlue, calendarname)
                Exit Function
            End If

        End Function
        Public Function addHour(aVAlue As Integer) As Date
            addHour = DateAdd("h", aVAlue, _timestamp)
        End Function
        Public Function addMinute(aVAlue As Integer) As Date
            addMinute = DateAdd("m", aVAlue, _timestamp)
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
        ''' Event Handler for record Fed
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRecordFed(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnRecordFed
            Try
                If e.Record.HasIndex(constFNYear) Then e.Record.SetValue(constFNYear, Me.year)
                If e.Record.HasIndex(constFNmonth) Then e.Record.SetValue(constFNmonth, Me.month)
                If e.Record.HasIndex(constFNDay) Then e.Record.SetValue(constFNDay, Me.dayofmonth)
                If e.Record.HasIndex(constFNNoWeek) Then e.Record.SetValue(constFNNoWeek, Me.week)
                If e.Record.HasIndex(constFNNoDay) Then Call e.Record.SetValue(constFNNoDay, Me.dayofyear)
                If e.Record.HasIndex(constFNQuarter) Then Call e.Record.SetValue(constFNQuarter, Me.quarter)
                If e.Record.HasIndex(constFNDate) Then Call e.Record.SetValue(constFNDate, Me.Datevalue)
                If e.Record.HasIndex(constFNTime) Then Call e.Record.SetValue(constFNTime, Me.Timevalue)
                If e.Record.HasIndex(constFNweekday) Then Call e.Record.SetValue(constFNweekday, Me.weekday)
                If e.Record.HasIndex(constFNWeekYear) Then Call e.Record.SetValue(constFNWeekYear, Me.weekofyear)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="CalendarEntry.OnRecordFed")
            End Try

        End Sub

        ''' <summary>
        ''' loads and infuses the object
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal name As String, ByVal ID As Long, Optional domainID As String = "") As CalendarEntry
            Dim primarykey() As Object = {name, ID, domainID}
            Return Retrieve(Of CalendarEntry)(pkArray:=primarykey, domainID:=domainID)
        End Function
        ''' <summary>
        ''' loads and infuses the object
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal name As String, ByVal ID As Long, Optional domainID As String = "") As Boolean
            Dim primarykey() As Object = {name, ID, domainID}
            Return MyBase.LoadBy(pkArray:=primarykey, domainID:=domainID)
        End Function
        ''' <summary>
        ''' create persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of CalendarEntry)(silent:=silent)

            'Dim aFieldDesc As New ormFieldDescription
            'Dim PrimaryColumnNames As New Collection
            'Dim OrderByColumnNames As New Collection
            'Dim RefByColumnNames As New Collection
            'Dim aTable As New ObjectDefinition

            'With aTable

            '    .Create(constTableid)

            '    aFieldDesc.Tablename = constTableid
            '    aFieldDesc.ID = ""
            '    aFieldDesc.Parameter = ""

            '    '***
            '    '*** Fields
            '    '****
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "calender name"
            '    aFieldDesc.ID = "cal1"
            '    aFieldDesc.ColumnName = "cname"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '    OrderByColumnNames.Add(aFieldDesc.ColumnName)
            '    RefByColumnNames.Add(aFieldDesc.ColumnName)


            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "reference entry id"
            '    aFieldDesc.ID = "cal18"
            '    aFieldDesc.ColumnName = "refid"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    RefByColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "entry id"
            '    aFieldDesc.ID = "cal2"
            '    aFieldDesc.ColumnName = "id"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    PrimaryColumnNames.Add(aFieldDesc.ColumnName)
            '    RefByColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "Timestamp"
            '    aFieldDesc.ID = "cal4"
            '    aFieldDesc.ColumnName = "timestamp"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    OrderByColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "entry type id"
            '    aFieldDesc.ID = "cal5"
            '    aFieldDesc.ColumnName = "typeid"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    OrderByColumnNames.Add(aFieldDesc.ColumnName)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "description"
            '    aFieldDesc.ID = "cal16"
            '    aFieldDesc.ColumnName = "desc"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "length in min"
            '    aFieldDesc.ID = "cal19"
            '    aFieldDesc.ColumnName = "length"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "week of the year"
            '    aFieldDesc.ID = "cal6"
            '    aFieldDesc.ColumnName = "noweek"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "day of year"
            '    aFieldDesc.ID = "cal7"
            '    aFieldDesc.ColumnName = "noday"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "number of weekday"
            '    aFieldDesc.ID = "cal8"
            '    aFieldDesc.ColumnName = "noweekday"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "year"
            '    aFieldDesc.ID = "cal9"
            '    aFieldDesc.ColumnName = "year"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "month"
            '    aFieldDesc.ID = "cal10"
            '    aFieldDesc.ColumnName = "month"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "day of month"
            '    aFieldDesc.ID = "cal11"
            '    aFieldDesc.ColumnName = "day"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Time
            '    aFieldDesc.Title = "timestamp"
            '    aFieldDesc.ID = "cal12"
            '    aFieldDesc.ColumnName = "timevalue"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Date
            '    aFieldDesc.Title = "timestamp"
            '    aFieldDesc.ID = "cal13"
            '    aFieldDesc.ColumnName = "datevalue"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "not available"
            '    aFieldDesc.ID = "cal14"
            '    aFieldDesc.ColumnName = "notavail"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Bool
            '    aFieldDesc.Title = "is important"
            '    aFieldDesc.ID = "cal15"
            '    aFieldDesc.ColumnName = "isimp"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.[Long]
            '    aFieldDesc.Title = "quarter"
            '    aFieldDesc.ID = "cal17"
            '    aFieldDesc.ColumnName = "quarter"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "week and year"
            '    aFieldDesc.ID = "cal20"
            '    aFieldDesc.ColumnName = "weekofyear"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 1
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 1 of condition"
            '    aFieldDesc.ColumnName = "param_txt1"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 2
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 2 of condition"
            '    aFieldDesc.ColumnName = "param_txt2"
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    ' parameter_txt 2
            '    aFieldDesc.Datatype = otFieldDataType.Text
            '    aFieldDesc.Title = "parameter_txt 3 of condition"
            '    aFieldDesc.ColumnName = "param_txt3"
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
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)

            '    aFieldDesc.Datatype = otFieldDataType.Timestamp
            '    aFieldDesc.Title = "creation Date"
            '    aFieldDesc.ColumnName = ConstFNCreatedOn
            '    aFieldDesc.ID = ""
            '    Call .AddFieldDesc(fielddesc:=aFieldDesc)
            '    ' Index
            '    Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
            '    Call .AddIndex("OrderbyDate", OrderByColumnNames, isprimarykey:=False)
            '    Call .AddIndex("OrderbyRefID", RefByColumnNames, isprimarykey:=False)

            '    ' persist
            '    .Persist()
            '    ' change the database
            '    .AlterSchema()
            'End With

            ''
            'CreateSchema = True
            'Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBDefScheduleMilestone.createSchema", tablename:=ConstTableid)
            CreateSchema = False
        End Function


        ''' <summary>
        ''' Return a Collection of all Calendar Entries
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of CalendarEntry)
            Return ormDataObject.All(Of CalendarEntry)()
        End Function

        ''' <summary>
        ''' Returns the number of available days between two dates
        ''' </summary>
        ''' <param name="fromdate"></param>
        ''' <param name="untildate"></param>
        ''' <param name="name">default calendar</param>
        ''' <returns>days in long</returns>
        ''' <remarks></remarks>
        Public Shared Function AvailableDays(ByVal fromdate As Date, ByVal untildate As Date, _
                                             Optional ByVal name As String = "", _
                                             Optional domainid As String = "") As Long

            '* default parameters
            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If

            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

            '** run sqlstatement
            Try
                Dim aStore = GetTableStore(ConstTableid)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="availabledays", addAllFields:=False, addMe:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = "count(id)"
                    aCommand.Where = String.Format("[{0}}=@cname and [{1}] > @date1 and [{1}] <@date2 and [{2}] <> @avail and [{3}]=@typeID and ([{5}]=@domainid )", _
                    {constFNName, constFNTimestamp, constFNNotAvail, constFNDomainID})
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", columnname:="cname", tablename:=ConstTableid))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date1", datatype:=otFieldDataType.Date, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date2", datatype:=otFieldDataType.Date, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@avail", datatype:=otFieldDataType.Bool, notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@typeid", datatype:=otFieldDataType.[Long], notColumn:=True))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainid", datatype:=otFieldDataType.Text, notColumn:=True))
                    'aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalDomain", datatype:=otFieldDataType.Text, notColumn:=True))

                    aCommand.Prepare()
                End If

                '** values
                aCommand.SetParameterValue(ID:="@cnamd", value:=name)
                aCommand.SetParameterValue(ID:="@date1", value:=fromdate)
                aCommand.SetParameterValue(ID:="@date2", value:=untildate)
                aCommand.SetParameterValue(ID:="@typeid", value:=True)
                aCommand.SetParameterValue(ID:="@typeid", value:=otCalendarEntryType.DayEntry)
                aCommand.SetParameterValue(ID:="@domainid", value:=domainid)
                'aCommand.SetParameterValue(ID:="@globalDomain", value:=ConstGlobalDomain)

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
                Call CoreMessageHandler(exception:=ex, subname:="CalendarEntry.AvailableDays")
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
        Public Shared Function NextAvailableDate(ByVal fromdate As Date, ByVal noDays As Integer, _
                                                 Optional ByVal name As String = "", _
                                                 Optional domainID As String = "") As Date

            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If
            If DomainID = "" Then DomainID = CurrentSession.CurrentDomainID

            '**
            Try
                Dim aStore = GetTableStore(ConstTableid)
                Dim aCommand As ormSqlSelectCommand
                If noDays < 0 Then
                    aCommand = aStore.CreateSqlSelectCommand(id:="nextavailabledate-neg", addAllFields:=False, addMe:=False)
                    If Not aCommand.Prepared Then
                        aCommand.select = "[" & constFNTimestamp & "]"
                        aCommand.Where = String.Format("[{0}]=@cname and [{1}] < @date1  and [{2}] <> @avail and [{3}]=@typeID and [{4}]=@domainID", _
                            {constFNName, constFNTimestamp, constFNNotAvail, ConstFNTypeID, constFNDomainID})

                        aCommand.OrderBy = "[" & constFNTimestamp & "] desc"
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", columnname:="cname", tablename:=ConstTableid))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date1", datatype:=otFieldDataType.Date, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@avail", datatype:=otFieldDataType.Bool, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@typeid", datatype:=otFieldDataType.[Long], notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainid", datatype:=otFieldDataType.Text, notColumn:=True))

                        aCommand.Prepare()
                    End If

                Else
                    aCommand = aStore.CreateSqlSelectCommand(id:="nextavailabledate-pos", addAllFields:=False, addMe:=False)
                    If Not aCommand.Prepared Then

                        aCommand.select = "[" & constFNTimestamp & "]"
                        aCommand.Where = String.Format("[{0}]=@cname and [{1}] > @date1  and [{2}] <> @avail and [{3}]=@typeID and [{4}]=@domainID", _
                           {constFNName, constFNTimestamp, constFNNotAvail, ConstFNTypeID, constFNDomainID})

                        aCommand.OrderBy = "[" & constFNTimestamp & "] asc"

                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", columnname:="cname", tablename:=ConstTableid))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date1", datatype:=otFieldDataType.Date, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@avail", datatype:=otFieldDataType.Bool, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@typeid", datatype:=otFieldDataType.[Long], notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainid", datatype:=otFieldDataType.Text, notColumn:=True))

                        aCommand.Prepare()

                    End If
                End If

                '** values
                aCommand.SetParameterValue(ID:="@cnamd", value:=name)
                aCommand.SetParameterValue(ID:="@date1", value:=fromdate)
                aCommand.SetParameterValue(ID:="@avail", value:=True)
                aCommand.SetParameterValue(ID:="@typeid", value:=otCalendarEntryType.DayEntry)
                aCommand.SetParameterValue(ID:="@domainid", value:=domainID)

                Dim resultRecords As List(Of ormRecord) = aCommand.RunSelect

                If resultRecords.Count > noDays Then
                    NextAvailableDate = resultRecords.Item(noDays - 1).GetValue(1)
                Else
                    Call CoreMessageHandler(subname:="CalendarEntry.nextavailableDate", message:="requested no of days is behind calendar end - regenerate calendar", _
                                           messagetype:=otCoreMessageType.ApplicationError, arg1:=noDays)
                    NextAvailableDate = resultRecords.Last.GetValue(1)
                End If

                Return NextAvailableDate

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="CalendarEntry.nextAvailableDate")
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
        Public Function IsAvailableOn(ByVal refdate As Date, Optional ByVal name As String = "", Optional domainID As String = "") As Boolean
            Dim aCollection As New List(Of CalendarEntry)
            Dim anEntry As New CalendarEntry

            If name = "" Then name = CurrentSession.DefaultCalendarName
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            aCollection = AllByDate(name:=name, refDate:=refdate, domainID:=domainID)
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
        Public Shared Function AllByDate(ByVal refDate As Date, Optional ByVal name As String = "", Optional domainID As String = "") As List(Of CalendarEntry)
            Dim aCollection As New List(Of CalendarEntry)
            Dim aTable As iormDataStore


            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try

                aTable = GetTableStore(ConstTableid)

                'wherestr = "timestamp = #" & Format(refDate, "mm-dd-yyyy") & "# and cname='" & Name & "'"
                'aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand("AllByDate")
                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.AddTable(ConstTableid, addAllFields:=True)
                    '** Depends on the server
                    If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer Then
                        aCommand.Where = String.Format(" [{0}] = @cname and convert(varchar, [{1}], 104) = @datestr and [{2}]=@domainID", _
                                                       {constFNName, constFNTimestamp, constFNDomainID})
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, columnname:=constFNName))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@datestr", datatype:=otFieldDataType.Text, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", datatype:=otFieldDataType.Text, notColumn:=True))

                    ElseIf aCommand.DatabaseDriver.DatabaseType = otDBServerType.Access Then
                        aCommand.Where = String.Format(" [{0}] = @cname and [{1}] = @date and [{2}]=@domainID", _
                        {constFNName, constFNTimestamp, constFNDomainID})
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, columnname:=constFNName))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date", datatype:=otFieldDataType.[Date], notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", datatype:=otFieldDataType.Text, notColumn:=True))
                    Else
                        CoreMessageHandler(message:="DatabaseType not recognized for SQL Statement", messagetype:=otCoreMessageType.InternalError, _
                                           subname:="CalendarEntry.AllByDate", tablename:=ConstTableid, arg1:=aCommand.DatabaseDriver.DatabaseType)
                        Return aCollection
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
                aCommand.SetParameterValue("@domainid", domainID)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                If theRecords.Count >= 0 Then

                    For Each aRecord As ormRecord In theRecords
                        Dim aNewObject As New CalendarEntry
                        aNewObject = New CalendarEntry
                        If aNewObject.Infuse(aRecord) Then
                            aCollection.Add(Item:=aNewObject)
                        End If
                    Next aRecord

                End If
                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(subname:="CalendarEntry.AllByDate", exception:=ex, messagetype:=otCoreMessageType.InternalError)
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
        Public Function GenerateDays(ByVal fromdate As Date, ByVal untildate As Date, Optional ByVal name As String = "", Optional domainID As String = "") As Boolean
            Dim aCollection As New List(Of CalendarEntry)
            Dim currDate As Date
            Dim anEntry As New CalendarEntry

            ' calendar name
            If name = "" Then name = CurrentSession.DefaultCalendarName
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            ' start
            currDate = fromdate
            Do While currDate <= untildate

                'exists ?
                aCollection = CalendarEntry.AllByDate(refDate:=currDate, name:=name, domainID:=domainID)
                If aCollection Is Nothing Or aCollection.Count = 0 Then
                    anEntry = New CalendarEntry
                    With anEntry
                        .Create(name, domainID:=domainID)
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
        Public Overloads Function Create(Optional ByVal name As String = "", Optional entryid As Long = 0, Optional domainID As String = "") As Boolean
            Dim primarykey() As Object = {name, entryid, domainID}
            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            '** create the key
            If entryid = 0 Then
                Dim pkarray() As Object = {name, Nothing, Nothing}
                If Not Me.TableStore.CreateUniquePkValue(pkarray) Then
                    Call CoreMessageHandler(message:="unique key couldnot be created", subname:="CalendarEntry.Create", arg1:=name, _
                                                tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                primarykey = pkarray
            End If
            If Not MyBase.Create(primarykey, domainID:=domainID) Then
                Return False
            End If

            ' set the primaryKey
            _name = LCase(name)
            Try
                _entryid = CLng(primarykey(1))
            Catch ex As Exception
                Call CoreMessageHandler(message:="unique id couldnot be retrieved", subname:="CalendarEntry.Create", arg1:=primarykey, _
                                               tablename:=TableID, entryname:="entryid", messagetype:=otCoreMessageType.InternalError)
                Return False
            End Try


            Return Me.IsCreated
        End Function

    End Class
End Namespace