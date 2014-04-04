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

Namespace OnTrack.Calendar

    ''' <summary>
    ''' Calendar Entry Class
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=CalendarEntry.ConstObjectID, modulename:=ConstModuleCalendar, description:="object to store an calendar entry", _
        usecache:=True, Version:=1)> Public Class CalendarEntry
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "CalendarEntry"
        '** Schema
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True, addDomainBehavior:=True, usecache:=True, addsparefields:=True)> _
        Public Const ConstTableid As String = "tblCalendarEntries"

        <ormSchemaIndex(columnname1:=constFNName, columnname2:=constFNRefID, columnname3:=constFNID, columnname4:=constFNDomainID)> Public Const constINDEXRefID = "refid"
        <ormSchemaIndex(columnname1:=constFNName, columnname2:=constFNTimestamp, columnname3:=ConstFNTypeID, columnname4:=constFNDomainID)> Public Const constIndexType = "typeid"
        <ormSchemaIndex(columnname1:=constFNName, columnname2:=constFNTimestamp, columnname3:=constFNDomainID, columnname4:=ConstFNTypeID)> Public Const constIndexDomain = "domain"

        '*** keys
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=50, primarykeyordinal:=1, _
            XID:="CAL1", title:="Name", description:="name of calendar")> Public Const constFNName = "cname"
        <ormObjectEntry(typeid:=otFieldDataType.Long, primarykeyordinal:=2, _
           XID:="CAL2", title:="EntryNo", description:="entry no in the calendar")> Public Const constFNID = "id"
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const constFNDomainID = Domain.ConstFNDomainID

        '** columns
        <ormObjectEntry(typeid:=otFieldDataType.Timestamp, _
         XID:="CAL4", title:="Timestamp", description:="timestamp entry in the calendar")> Public Const constFNTimestamp = "timestamp"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
         XID:="CAL5", title:="Type", description:="entry type in the calendar")> Public Const ConstFNTypeID = "typeid"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, _
        XID:="CAL6", title:="Description", description:="entry description in the calendar")> Public Const ConstFNDescription = "desc"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
          XID:="CAL8", title:="RefID", description:="entry refID in the calendar")> Public Const constFNRefID = "refid"

        <ormObjectEntry(typeid:=otFieldDataType.Bool, XID:="cal9", title:="Not Available", description:="not available")> _
        Public Const constFNNotAvail = "notavail"
        <ormObjectEntry(typeid:=otFieldDataType.Bool, XID:="cal10", title:="Is Important", description:="is important entry (prioritized)")> _
        Public Const constFNIsImportant = "isimp"

        <ormObjectEntry(typeid:=otFieldDataType.Long, _
          XID:="CAL20", title:="TimeSpan", description:="length in minutes")> Public Const constFNLength = "length"


        '** not mapped

        <ormObjectEntry(typeid:=otFieldDataType.Long, _
          XID:="CAL31", title:="Week", description:="week of the year")> Public Const constFNNoWeek = "noweek"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
         XID:="CAL32", title:="Day", description:="day of the year")> Public Const constFNNoDay = "noday"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
         XID:="CAL33", title:="Weekday", description:="number of day in the week")> Public Const constFNweekday = "noweekday"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
         XID:="CAL34", title:="Quarter", description:="no of quarter of the year")> Public Const constFNQuarter = "quarter"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
         XID:="CAL35", title:="Year", description:="the year")> Public Const constFNYear = "year"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
        XID:="CAL36", title:="Month", description:="the month")> Public Const constFNmonth = "month"
        <ormObjectEntry(typeid:=otFieldDataType.Long, _
        XID:="CAL37", title:="Day", description:="the day")> Public Const constFNDay = "day"
        <ormObjectEntry(typeid:=otFieldDataType.Time, _
        XID:="CAL38", title:="Time", description:="time")> Public Const constFNTime = "timevalue"
        <ormObjectEntry(typeid:=otFieldDataType.Date, _
        XID:="CAL39", title:="Date", description:="date")> Public Const constFNDate = "datevalue"
        <ormObjectEntry(typeid:=otFieldDataType.Text, size:=10, _
        XID:="CAL40", title:="WeekYear", description:="Week and Year representation")> Public Const constFNWeekYear = "weekofyear"


        '** mappings
        <ormEntryMapping(EntryName:=constFNID)> Private _entryid As Long = 0
        <ormEntryMapping(EntryName:=constFNName)> Private _name As String = ""
        <ormEntryMapping(EntryName:=constFNTimestamp)> Private _timestamp As Date = ConstNullDate
        <ormEntryMapping(EntryName:=constFNRefID)> Private _refid As Long = 0
        <ormEntryMapping(EntryName:=constFNLength)> Private _length As Long = 0
        ' fields
        <ormEntryMapping(EntryName:=ConstFNTypeID)> Private _EntryType As otCalendarEntryType
        <ormEntryMapping(EntryName:=constFNIsImportant)> Private s_isImportant As Boolean = False
        <ormEntryMapping(EntryName:=constFNNotAvail)> Private s_notAvailable As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private s_description As String = ""



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
        Public Sub OnRecordFed(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnFed
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
        ''' Return a Collection of all Calendar Entries
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of CalendarEntry)
            Return ormDataObject.AllDataObject(Of CalendarEntry)()
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
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

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
            Dim aStore As iormDataStore


            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID


            Try
                aStore = GetTableStore(ConstTableid)
                Dim cached = aStore.GetProperty(ormTableStore.ConstTPNCacheProperty)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand("AllByDate")

                '** prepare the command if necessary
                If Not aCommand.Prepared Then

                    aCommand.AddTable(ConstTableid, addAllFields:=True)
                    '** Depends on the server
                    If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer And cached Is Nothing Then
                        aCommand.Where = String.Format(" [{0}] = @cname and CONVERT(nvarchar, [{1}], 104) = @datestr and [{2}] = @domainID", _
                                                       {constFNName, constFNTimestamp, constFNDomainID})
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, columnname:=constFNName))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@datestr", datatype:=otFieldDataType.Text, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", datatype:=otFieldDataType.Text, notColumn:=True))

                    ElseIf aCommand.DatabaseDriver.DatabaseType = otDBServerType.Access And cached Is Nothing Then
                        aCommand.Where = String.Format(" [{0}] = @cname and [{1}] = @date and [{2}]=@domainID", _
                        {constFNName, constFNTimestamp, constFNDomainID})
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, columnname:=constFNName))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date", datatype:=otFieldDataType.Date, notColumn:=True))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", datatype:=otFieldDataType.Text, notColumn:=True))

                        ''' just cached against DataTable
                    ElseIf cached IsNot Nothing Then
                        aCommand.Where = String.Format(" [{0}] = @cname and [{1}] = @date and [{2}]=@domainID", _
                       {constFNName, constFNTimestamp, constFNDomainID})
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@cname", datatype:=otFieldDataType.Text, columnname:=constFNName))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@date", datatype:=otFieldDataType.Date, notColumn:=True))
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
                If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer And cached Is Nothing Then
                    aCommand.SetParameterValue("@datestr", Format("dd.MM.yyyy", refDate))
                Else
                    aCommand.SetParameterValue("@date", refDate)
                End If
                aCommand.SetParameterValue("@domainID", domainID)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                If theRecords.Count >= 0 Then
                    For Each aRecord As ormRecord In theRecords
                        Dim aNewObject As New CalendarEntry
                        If InfuseDataObject(record:=aRecord, dataobject:=aNewObject) Then
                            aCollection.Add(item:=aNewObject)
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
        Public Shared Function GenerateDays(ByVal fromdate As Date, ByVal untildate As Date, Optional ByVal name As String = "", Optional domainID As String = "") As Boolean
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
                If aCollection Is Nothing OrElse aCollection.Count = 0 Then
                    anEntry = CalendarEntry.Create(name:=name, domainID:=domainID)
                    If anEntry IsNot Nothing Then
                        With anEntry
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
        Public Shared Function Create(Optional ByVal name As String = "", Optional entryid As Long = 0, Optional domainID As String = "") As CalendarEntry
            Dim primarykey() As Object = {name, entryid, domainID}
            If name = "" Then
                name = CurrentSession.DefaultCalendarName
            End If
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            '** create the key
            If entryid = 0 Then
                Dim pkarray() As Object = {name, Nothing, Nothing}
                If Not ot.GetTableStore(ConstTableid).CreateUniquePkValue(pkarray) Then
                    Call CoreMessageHandler(message:="unique key couldnot be created", subname:="CalendarEntry.Create", arg1:=name, _
                                                tablename:=ConstTableid, messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
                primarykey = {name, pkarray(1), domainID}
            End If
            Return CreateDataObject(Of CalendarEntry)(pkArray:=primarykey, domainID:=domainID)
        End Function

    End Class
End Namespace