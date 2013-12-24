
REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** MESSAGE QUEUE OBJECTS Classes
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** TO DO Log:
REM ***********             
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************

Option Explicit On
Imports System.Diagnostics
Imports System.Linq
Imports System.Diagnostics.Debug

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.Scheduling
Imports OnTrack.Deliverables


Namespace OnTrack.Xchange
    '************************************************************************************
    '***** CLASS clsOTDBMessageQueue is a representation class for a Log as Messages
    '*****
    Public Class clsOTDBMessageQueue
        Inherits ormDataObject
        Implements otLoggable
        Implements iormInfusable
        Implements iormPersistable


        Public Const ConstTableID = "tblMessageQueue"
        Public Const ConstFNTag = "tag"

        ' Data
        Private s_tag As String


        Private s_reqby As String
        Private s_reqbyOU As String
        Private s_reqOn As Date
        Private s_workspace As String
        Private s_xChangeConfigName As String
        Private s_status As String
        Private s_desc As String
        Private s_cmt As String
        Private s_timestamp As Date
        Private s_procbyUser As String
        Private s_xChangeConfig As New clsOTDBXChangeConfig
        Private s_msglogtag As String
        '** for ERROR MSG
        Private s_ContextIdentifier As String
        Private s_TupleIdentifier As String
        Private s_EntitityIdentifier As String


        '** not saved -> ordinals of the special MQF Columns -> for write back and preprocess
        Public Actionordinal As Object
        Public ProcessStatusordinal As Object
        Public ProcessDateordinal As Object
        Public ProcessLogordinal As Object
        Private s_msglog As New clsOTDBMessagelog


        Private _entries As New List(Of clsOTDBMessageQueueEntry)

        ''' <summary>
        ''' constructore
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the context identifier.
        ''' </summary>
        ''' <value>The context identifier.</value>
        Public Property ContextIdentifier() As String Implements otLoggable.ContextIdentifier
            Get
                ContextIdentifier = s_ContextIdentifier
            End Get
            Set(value As String)

                s_ContextIdentifier = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tuple identifier.
        ''' </summary>
        ''' <value>The tuple identifier.</value>
        Public Property TupleIdentifier() As String Implements otLoggable.TupleIdentifier
            Get
                TupleIdentifier = s_TupleIdentifier
            End Get
            Set(value As String)

                s_TupleIdentifier = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the entitity identifier.
        ''' </summary>
        ''' <value>The entitity identifier.</value>
        Public Property EntitityIdentifier() As String Implements otLoggable.EntitityIdentifier
            Get
                EntitityIdentifier = s_EntitityIdentifier
            End Get
            Set(value As String)

                s_EntitityIdentifier = value
            End Set
        End Property



        ReadOnly Property TAG()
            Get
                TAG = s_tag
            End Get

        End Property

        ReadOnly Property Size() As Long
            Get
                size = _entries.Count
            End Get

        End Property

        Public Property RequestedBy() As String
            Get
                requestedBy = s_reqby
            End Get
            Set(value As String)
                If LCase(s_reqby) <> LCase(value) Then
                    s_reqby = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property RequestedByOU() As String
            Get
                requestedByOU = s_reqbyOU
            End Get
            Set(value As String)
                If LCase(s_reqbyOU) <> LCase(value) Then
                    s_reqbyOU = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ReadOnly Property MSGLOG() As clsOTDBMessagelog
            Get
                MSGLOG = s_msglog
            End Get

        End Property

        Public Property WorkspaceID() As String
            Get
                WorkspaceID = s_workspace
            End Get
            Set(value As String)
                If LCase(s_workspace) <> LCase(value) Then
                    s_workspace = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ReadOnly Property Entries() As List(Of clsOTDBMessageQueueEntry)
            Get
                Entries = _entries
            End Get

        End Property
        Public Property XChangeConfigName() As String
            Get
                XChangeConfigName = s_xChangeConfigName
            End Get
            Set(value As String)
                If LCase(s_xChangeConfigName) <> LCase(value) Then
                    s_xChangeConfigName = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode() As String
            Get
                statuscode = s_status
            End Get
            Set(value As String)
                If LCase(s_status) <> LCase(value) Then
                    s_status = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Description() As String
            Get
                description = s_desc
            End Get
            Set(value As String)
                If s_desc <> value Then
                    s_desc = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ProcessedByUsername() As String
            Get
                processedByUsername = s_procbyUser
            End Get
            Set(value As String)
                If s_procbyUser <> value Then
                    s_procbyUser = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Comment() As String
            Get
                COMMENT = s_cmt
            End Get
            Set(value As String)
                If s_cmt <> value Then
                    s_cmt = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Processdate() As Date
            Get
                processdate = s_timestamp
            End Get
            Set(value As Date)
                If s_timestamp <> value Then
                    s_timestamp = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property RequestedOn() As Date
            Get
                requestedOn = s_reqOn
            End Get
            Set(value As Date)
                If s_reqOn <> value Then
                    s_reqOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property XChangeConfig() As clsOTDBXChangeConfig
            Get
                XchangeConfig = s_xChangeConfig
            End Get
            Set(value As clsOTDBXChangeConfig)
                If LCase(s_xChangeConfig.Configname) <> LCase(value.Configname) Then
                    s_xChangeConfig = value
                    s_xChangeConfigName = value.Configname
                    Me.IsChanged = True
                End If
            End Set
        End Property

        '****** getUniqueTag
        Public Function GetUniqueTag()
            getUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & s_tag & ConstDelimiter
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = GetUniqueTag()
                End If
                msglogtag = s_msglogtag
            End Get

        End Property
#End Region

        ''' <summary>
        ''' deletesthe persisted objecte and subobjects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Delete() As Boolean
            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each anEntry In _entries
                anEntry.Delete()
            Next anEntry

            ' reset it
            MyBase.Delete()

            _entries.Clear()
            Return Me.IsDeleted

        End Function
        ''' <summary>
        ''' Get Entry by RowNo
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntry(ByVal rowno As Long) As clsOTDBMessageQueueEntry
            Dim anEntry As clsOTDBMessageQueueEntry
            If rowno <= Me.Size And rowno > 0 Then
                anEntry = _entries.Item(rowno)
                GetEntry = anEntry
                Exit Function
            End If

            GetEntry = Nothing
        End Function
        ''' <summary>
        ''' creates an Entry with optional rowno
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateEntry(Optional rowno As Long = 0) As clsOTDBMessageQueueEntry
            'Dim rowno As Long

            Dim anEntry As New clsOTDBMessageQueueEntry
            anEntry = New clsOTDBMessageQueueEntry
            If rowno = 0 Then
                rowno = Me.Size + 1
            End If
            If Not anEntry.create(Me.TAG, rowno) Then
                Call anEntry.LoadBy(Me.TAG, rowno)
            End If
            anEntry._queue = Me
            '* add it
            Call _entries.Add(item:=anEntry)


            createEntry = anEntry

        End Function
        ''' <summary>
        ''' Infuses the Object from the record
        ''' </summary>
        ''' <param name="aRecord"></param>
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
                s_tag = CStr(record.GetValue(ConstFNTag))
                s_reqby = CStr(record.GetValue("reqby"))
                s_reqbyOU = CStr(record.GetValue("reqbyou"))
                s_workspace = CStr(record.GetValue("wspace"))
                s_xChangeConfigName = CStr(record.GetValue("xchg"))
                s_status = CStr(record.GetValue("status"))
                s_desc = CStr(record.GetValue("desc"))
                s_cmt = CStr(record.GetValue("cmt"))
                If IsNull(record.GetValue("timestamp")) Then
                    s_timestamp = ConstNullDate
                Else
                    s_timestamp = CDate(record.GetValue("timestamp"))
                End If
                s_procbyUser = CStr(record.GetValue("procuser"))
                s_msglogtag = CStr(record.GetValue("msglogtag"))

                '** Infuse the subenries
                Dim aRecordCollection As New List(Of ormRecord)
                Dim aTable As iormDataStore = GetTableStore(clsOTDBMessageQueueEntry.ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="loadby")
                If Not aCommand.Prepared Then
                    aCommand.Where = clsOTDBMessageQueueEntry.ConstTableID & ".[" & clsOTDBMessageQueueEntry.ConstFNTag & "] = @tag"
                    aCommand.OrderBy = clsOTDBMessageQueueEntry.ConstTableID & ".[" & clsOTDBMessageQueueEntry.ConstFNRowno & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@tag", fieldname:=clsOTDBMessageQueueEntry.ConstFNTag, _
                                                                           tablename:=clsOTDBMessageQueueEntry.ConstTableID))
                End If
                If aCommand.Prepared Then
                    _entries.Clear()
                    aCommand.SetParameterValue(ID:="@tag", value:=LCase(s_tag))
                    aRecordCollection = aCommand.RunSelect
                    ' records read
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component
                        Dim anEntry As clsOTDBMessageQueueEntry = New clsOTDBMessageQueueEntry
                        If anEntry.Infuse(aRecord) Then
                            _entries.Add(anEntry)
                        End If
                    Next aRecord
                End If

                Return MyBase.Infuse(record)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueue.Infuse")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' loads a message queue from the datastore
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Loadby(ByVal TAG As String) As Boolean
            Dim pkarry() As Object = {LCase(TAG)}

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    loadBy = False
                    Exit Function
                End If
            End If


            If MyBase.LoadBy(pkArray:=pkarry) Then
                s_tag = TAG
                ' set msglog
                Me.ContextIdentifier = s_tag
            End If


        End Function
        ''' <summary>
        ''' Persists the Queue and all entries
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ConstNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Return False
                End If
            End If
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Persist = False
                Exit Function
            End If

            Try
                Call Me.Record.SetValue(ConstFNTag, s_tag)
                Call Me.Record.SetValue("desc", s_desc)
                Call Me.Record.SetValue("reqby", s_reqby)
                Call Me.Record.SetValue("reqbyou", s_reqbyOU)
                Call Me.Record.SetValue("wspace", s_workspace)
                Call Me.Record.SetValue("xchg", s_xChangeConfigName)
                Call Me.Record.SetValue("status", s_status)
                Call Me.Record.SetValue("cmt", s_cmt)
                Call Me.Record.SetValue("timestamp", s_timestamp)
                Call Me.Record.SetValue("procuser", s_procbyUser)
                Call Me.Record.SetValue("msglogtag", s_msglogtag)
                '
                timestamp = Date.Now
                Persist = MyBase.Persist(timestamp)

                For Each anEntry In _entries
                    If Not String.IsNullOrWhiteSpace(anEntry.Action) And UCase(anEntry.Action) <> ConstMQFOpNoop Then
                        Persist = Persist And anEntry.PERSIST(timestamp)
                    End If
                Next
                Return Persist
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueue.persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' create Persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = ConstTableID

            With aTable
                .Create(ConstTableID)
                .Delete()



                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message queue tag"
                aFieldDesc.ID = "mqf1"
                aFieldDesc.ColumnName = ConstFNTag
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)


                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "requested by"
                aFieldDesc.ColumnName = "reqby"
                aFieldDesc.ID = "mqf2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msgid
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "requested by OU"
                aFieldDesc.ColumnName = "reqbyou"
                aFieldDesc.ID = "mqf3"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "workspaceID"
                aFieldDesc.ColumnName = "wspace"
                aFieldDesc.ID = "mqf4"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "xchange config name"
                aFieldDesc.ColumnName = "xchg"
                aFieldDesc.ID = "mqf5"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status"
                aFieldDesc.ColumnName = "status"
                aFieldDesc.ID = "mqf6"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "title"
                aFieldDesc.ColumnName = "desc"
                aFieldDesc.ID = "mqf7"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "cmt"
                aFieldDesc.ColumnName = "cmt"
                aFieldDesc.ID = "mqf8"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last processed timestamp"
                aFieldDesc.ColumnName = "timestamp"
                aFieldDesc.ID = "mqf9"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "processed by username"
                aFieldDesc.ColumnName = "procuser"
                aFieldDesc.ID = "mqf10"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.[Date]
                aFieldDesc.Title = "requested on"
                aFieldDesc.ColumnName = "reqon"
                aFieldDesc.ID = "mqf11"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "msglog"
                aFieldDesc.ColumnName = "msglogtag"
                aFieldDesc.ID = "mqf12"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            createSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBMessageQueue.createSchema")
            createSchema = False
        End Function
        ''' <summary>
        ''' create a persistble message queue
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal tag As String) As Boolean
            Dim pkarry() As Object = {LCase(tag)}

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                Create = False
                Exit Function
            End If

            If MyBase.Create(pkArray:=pkarry, checkUnique:=True) Then
                s_tag = LCase(tag)
                ' set msglog
                Me.ContextIdentifier = s_tag
            End If

            Return Me.IsCreated

        End Function

        '***** 
        '*****
        ''' <summary>
        ''' process -> write the MQF to the Database through the XChangeManager
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Process() As Boolean
            Process = True
            ' step through the RowEntries
            For Each aMQFRowEntry In Me.Entries
                ' if processable than process
                If aMQFRowEntry.Processable Then
                    Process = Process And aMQFRowEntry.runXChange()
                End If
            Next aMQFRowEntry

            Return Process
        End Function

        ''' <summary>
        ''' Raises the message.
        ''' </summary>
        ''' <param name="index">The index.</param>
        ''' <param name="MSGLOG">The MSGLOG.</param>
        ''' <returns></returns>
        Public Function raiseMessage(index As Long, ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.raiseMessage
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Attaches the message log.
        ''' </summary>
        ''' <param name="MSGLOG">The MSGLOG.</param>
        ''' <returns></returns>
        Public Function attachMessageLog(ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.attachMessageLog
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' precheck -> check the MQF
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Precheck() As Boolean
            Precheck = True
            ' step through the RowEntries
            For Each aMQFRowEntry In Me.Entries
                Precheck = Precheck And aMQFRowEntry.RunPreCheck()
            Next aMQFRowEntry

            Return Precheck
        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsTODBMessageQueueEntry is a representation class for a Log as Messages
    '*****

    Public Class clsOTDBMessageQueueEntry
        Inherits ormDataObject
        Implements otLoggable
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstTableID = "tblMessageQueueEntry"

        Public Const ConstFNTag = "tag"
        Public Const ConstFNRowno = "rowno"

        ' Data
        Private s_tag As String = ""
        Private s_rowno As Long
        Private s_action As String = ""
        Private s_processable As Boolean
        Private s_processedOn As Date = ConstNullDate
        Private s_status As String = ""
        '
        Private _members As New List(Of clsOTDBMessageQueueMember)

        '** for ERROR MSG
        Private s_ContextIdentifier As Object
        Private s_TupleIdentifier As Object
        Private s_EntitityIdentifier As Object

        Public _queue As clsOTDBMessageQueue
        Private s_msglogtag As String = ""
        Private s_msglog As New clsOTDBMessagelog

        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)
        End Sub

#Region "Properties"


        ReadOnly Property Tag() As String
            Get
                TAG = s_tag
            End Get

        End Property
        ReadOnly Property Rowno() As Long
            Get
                rowno = s_rowno
            End Get

        End Property

        Public Property Processable() As Boolean

            Get
                processable = s_processable
            End Get
            Set(value As Boolean)
                If s_processable <> value Then
                    s_processable = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        Public Property Action() As String
            Get
                action = s_action
            End Get
            Set(value As String)
                If LCase(s_action) <> LCase(value) Then
                    s_action = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode() As String
            Get
                statuscode = s_status
            End Get
            Set(value As String)
                If LCase(s_status) <> LCase(value) Then
                    s_status = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ProcessedOn() As Date
            Get
                processedOn = s_processedOn
            End Get
            Set(value As Date)
                If LCase(s_processedOn) <> LCase(value) Then
                    s_processedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        ReadOnly Property Size() As Long
            Get
                size = _members.Count
            End Get

        End Property
        ReadOnly Property msglog() As clsOTDBMessagelog
            Get
                MSGLOG = s_msglog
            End Get

        End Property
        '****** getUniqueTag
        Public Function GetUniqueTag()
            getUniqueTag = ConstDelimiter & ConstTableID & ConstDelimiter & s_tag & ConstDelimiter & s_rowno & ConstDelimiter
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


        '*** add new Member to the collection
        ''' <summary>
        ''' adds a Member to the Entry
        ''' </summary>
        ''' <param name="aNewMember"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddMember(ByRef member As clsOTDBMessageQueueMember) As Boolean
            If Not Me.IsCreated And Not _IsLoaded Then
                AddMember = False
                Exit Function
            End If

            _members.Add(member)
            AddMember = True
        End Function
        '*** add new Member to the collection
        ''' <summary>
        ''' GetMember by index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetMember(ByVal i As Long) As clsOTDBMessageQueueMember
            If Not Me.IsCreated And Not _IsLoaded Then
                getMember = Nothing
                Exit Function
            End If

            If i <= _members.Count And i > 0 Then
                getMember = _members.Item(i)
                Exit Function
            End If

            getMember = Nothing
        End Function
        ''' <summary>
        ''' returns a List of Members
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Members() As List(Of clsOTDBMessageQueueMember)
            Get
                Members = _members
            End Get
        End Property
        ''' <summary>
        ''' verify action
        ''' </summary>
        ''' <param name="ActionCommand"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function VerifyAction(Optional ByVal actioncommand As String = "") As Boolean

            If actioncommand = "" Then
                actioncommand = LCase(Me.Action)
            Else
                actioncommand = CStr(actioncommand)
            End If

            ' check on it
            Select Case LCase(Trim(actioncommand))
                ' CHANGE
                Case ConstMQFOpChange
                    verifyAction = True
                    ' COMMAND FREEZE
                Case ConstMQFOpFreeze

                    verifyAction = True
                    ' ADD AFTER NOT IMPLEMENTED YET
                Case ConstMQFOpAddAfter
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    verifyAction = True
                    ' ADD NEW REVISION
                Case ConstMQFOpAddRevision

                    verifyAction = True
                    ' NOOP
                Case ConstMQFOpNoop
                    'aMQFRowEntry.action = ConstMQFOpNoop
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '"INFO: in row #" & rowno & ": operation code '" & value & "' is meant to do nothing.")
                    'theMessages(n).processable = theMessages(n).processable And True
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    'theMessages(n).processable = False
                    ' DELETE NOT IMPLEMENTED YET
                    verifyAction = True
                Case ConstMQFOpDelete
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '"ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
                    'theMessages(n).processable = False
                    verifyAction = True
                Case ""
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "INFO: in row #" & rowno & " empty operation code - skipped ")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    verifyAction = False
                Case Else
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is unknown !")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_error
                    verifyAction = False

            End Select

        End Function
        ''' <summary>
        ''' Is Action Processable
        ''' </summary>
        ''' <param name="ActionCommand"></param>
        ''' <param name="MSGLOG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsActionProcessable(Optional ByVal actioncommand As String = "", _
                                            Optional ByRef MSGLOG As clsOTDBMessagelog = Nothing) As Boolean

            If actioncommand = "" Then
                actioncommand = LCase(Me.Action)
            End If

            'msglog
            If MSGLOG Is Nothing Then
                MSGLOG = New clsOTDBMessagelog
                MSGLOG.Create(Me.msglogtag)
                s_msglog = MSGLOG
            End If

            ' check on it
            Select Case LCase(Trim(ActionCommand))
                ' CHANGE
                Case ConstMQFOpChange
                    IsActionProcessable = True
                    ' COMMAND FREEZE
                Case ConstMQFOpFreeze
                    IsActionProcessable = True
                    ' ADD AFTER NOT IMPLEMENTED YET
                Case ConstMQFOpAddAfter
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    IsActionProcessable = True
                    ' ADD NEW REVISION
                Case ConstMQFOpAddRevision
                    IsActionProcessable = True
                    ' NOOP
                Case ConstMQFOpNoop
                    'aMQFRowEntry.action = ConstMQFOpNoop
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '"INFO: in row #" & rowno & ": operation code '" & value & "' is meant to do nothing.")
                    'theMessages(n).processable = theMessages(n).processable And True
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    'theMessages(n).processable = False
                    ' DELETE NOT IMPLEMENTED YET
                    IsActionProcessable = False
                Case ConstMQFOpDelete
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    ' "ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")

                    'theMessages(n).processable = False
                    IsActionProcessable = False
                Case ""
                    Call MSGLOG.AddMsg("300", Me._queue.TAG, Me.Rowno, "mqf1")
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "INFO: in row #" & rowno & " empty operation code - skipped ")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    IsActionProcessable = False
                Case Else
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is unknown !")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_error
                    IsActionProcessable = False

            End Select

        End Function
        ''' <summary>
        ''' Deletes all persistable objects in the datastore
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Delete() As Boolean

            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each member
            For Each member In _members
                member.Delete()
            Next
            '* me
            MyBase.Delete()

            ' reset it
            _members.Clear()
            Return Me.IsDeleted

        End Function
        ''' <summary>
        ''' infuse the object from the record
        ''' </summary>
        ''' <param name="aRecord"></param>
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
                s_tag = CStr(record.GetValue(ConstFNTag))
                s_rowno = CLng(Record.GetValue(constFNRowno))
                s_action = CStr(Record.GetValue("action"))
                s_processable = CBool(Record.GetValue("isprocessable"))
                s_processedOn = CDate(Record.GetValue("timestamp"))
                s_msglogtag = CBool(Record.GetValue("msglogtag"))
                s_status = CBool(record.GetValue("status"))

                ' load the members
                Dim aTable As iormDataStore = GetTableStore(clsOTDBMessageQueueMember.constTableID)

                Dim aCommand As ormSqlSelectCommand = aTable.CreateSqlSelectCommand(id:="loadby")
                If Not aCommand.Prepared Then
                    aCommand.Where = clsOTDBMessageQueueMember.constTableID & ".[" & ConstFNTag & "] = @tag and " & _
                                     clsOTDBMessageQueueMember.constTableID & ".[" & ConstFNRowno & "] = @rowno"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@tag", _
                                                                           fieldname:=clsOTDBMessageQueueMember.ConstFNTag, _
                                                                           tablename:=clsOTDBMessageQueueMember.constTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & clsOTDBMessageQueueMember.ConstFNRowno, _
                                                                           fieldname:=clsOTDBMessageQueueMember.ConstFNRowno, _
                                                                           tablename:=clsOTDBMessageQueueMember.constTableID))
                    aCommand.OrderBy = clsOTDBMessageQueueMember.constTableID & ".[" & clsOTDBMessageQueueMember.ConstFNIDNo & "] asc"
                    aCommand.Prepare()
                End If
                If aCommand.Prepared Then
                    aCommand.SetParameterValue(ID:="@" & clsOTDBMessageQueueMember.ConstFNTag, value:=LCase(s_tag))
                    aCommand.SetParameterValue(ID:="@" & clsOTDBMessageQueueMember.ConstFNRowno, value:=s_rowno)
                    Dim aRecordCollection As List(Of ormRecord) = aCommand.RunSelect
                    _members.Clear()
                    For Each aRecord In aRecordCollection
                        ' add the Entry as Component
                        Dim anEntry = New clsOTDBMessageQueueMember
                        If anEntry.Infuse(aRecord) Then
                            Call _members.Add(item:=anEntry)
                        End If
                    Next aRecord
                End If

                Return MyBase.Infuse(Record)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueueEntry.Infuse")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Load the object from the datastore
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal tag As String, ByVal rowno As Long) As Boolean
            Dim pkarry() As Object = {LCase(tag), rowno}

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            Return MyBase.LoadBy(pkArray:=pkarry)
        End Function
        ''' <summary>
        ''' creates a member
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateMember() As clsOTDBMessageQueueMember
            Dim ID As Long

            Dim aMember As New clsOTDBMessageQueueMember
            aMember = New clsOTDBMessageQueueMember
            ID = Me.Size + 1
            If Not aMember.Create(Me.Tag, Me.Rowno, ID) Then
                Call aMember.LoadBy(Me.Tag, Rowno, ID)
            End If
            aMember._entry = Me
            '* add it
            Call _members.Add(aMember)
            Return aMember
        End Function
        ''' <summary>
        ''' Persist the Entry with members
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ConstNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Return False
                End If
            End If
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return False
            End If

            Try
                Call Me.Record.SetValue(ConstFNTag, s_tag)
                Call Me.Record.SetValue(ConstFNRowno, s_rowno)
                Call Me.Record.SetValue("action", s_action)
                Call Me.Record.SetValue("isprocessable", s_processable)
                Call Me.Record.SetValue("timestamp", s_processedOn)
                Call Me.Record.SetValue("msglogtag", s_msglogtag)
                Call Me.Record.SetValue("status", s_status)
                If timestamp = ConstNullDate Then timestamp = Date.Now
                Persist = MyBase.Persist(timestamp)
                For Each aMember In _members
                    Persist = Persist And aMember.Persist(timestamp)
                Next
                Return Persist
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueueEntry.Persist")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Create Persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = ConstTableID
            aFieldDesc.Aliases = New String() {}

            With aTable
                .Create(ConstTableID)
                .Delete()

                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message queue tag"
                aFieldDesc.ID = "mqfe1"
                aFieldDesc.ColumnName = ConstFNTag
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'index pos
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "rowno index (primary key)"
                aFieldDesc.ColumnName = constFNRowno
                aFieldDesc.ID = "mqfe2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)


                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "action"
                aFieldDesc.ColumnName = "action"
                aFieldDesc.ID = "mqfe4"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message log tag"
                aFieldDesc.ColumnName = "msglogtag"
                aFieldDesc.ID = "mqfe10"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status of message"
                aFieldDesc.ColumnName = "status"
                aFieldDesc.ID = "mqfe11"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last processed timestamp"
                aFieldDesc.ColumnName = "timestamp"
                aFieldDesc.ID = "mqfe12"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "successful processed"
                aFieldDesc.ColumnName = "issuccess"
                aFieldDesc.ID = "mqfe13"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is to be approved"
                aFieldDesc.ColumnName = "istobeappr"
                aFieldDesc.ID = "mqfe14"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is to be approved"
                aFieldDesc.ColumnName = "isapproved"
                aFieldDesc.ID = "mqfe15"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "approved timestamp"
                aFieldDesc.ColumnName = "approvedon"
                aFieldDesc.ID = "mqfe16"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "filed for approval timestamp"
                aFieldDesc.ColumnName = "filedon"
                aFieldDesc.ID = "mqfe17"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is processable"
                aFieldDesc.ColumnName = "isprocessable"
                aFieldDesc.ID = "mqfe18"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}



                '***
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With


            createSchema = True
            Exit Function


            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBMessageQueue.createSchema")
            createSchema = False
        End Function
        ''' <summary>
        ''' Create Persistable Object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal tag As String, ByVal rowno As Long) As Boolean
            Dim pkarry() As Object = {LCase(tag), rowno}

            If MyBase.Create(pkArray:=pkarry) Then
                ' set the primaryKey
                s_tag = LCase(tag)
                s_rowno = rowno
            End If
            Return Me.IsCreated
        End Function

        ''' <summary>
        ''' run the Precheck on the Entry
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunPreCheck(Optional ByRef msglog As clsOTDBMessagelog = Nothing) As Boolean
            RunPreCheck = runXChange(justprecheck:=True, MSGLOG:=MSGLOG)

        End Function

        ''' <summary>
        ''' Fill Mapping from the Entry
        ''' </summary>
        ''' <param name="mapping"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FillMapping(ByRef mapping As Dictionary(Of Object, Object)) As Boolean
            Dim aMapping As New Dictionary(Of Object, Object)
            Dim aMember As clsOTDBMessageQueueMember
            Dim aConfig As clsOTDBXChangeConfig
            Dim aConfigmember As clsOTDBXChangeMember
            Dim aValue As Object


            If Not _IsLoaded And Not Me.IsCreated Then
                FillMapping = False
                Exit Function
            End If

            '** need the quueue
            If Me._queue Is Nothing Then
                Call CoreMessageHandler(subname:="clsOTDBMessageQueueEntry.runXChange", arg1:=Me.TAG, _
                                      message:="queue couldn't be loaded")

                FillMapping = False
                Exit Function
            Else
                aConfig = Me._queue.XCHANGECONFIG
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOTDBMessageQueueEntry.runXChange", _
                                          arg1:=Me._queue.XChangeConfigName, _
                                          message:="XChangeConfig couldn't be loaded")

                    FillMapping = False
                    Exit Function
                End If
            End If

            If Not mapping Is Nothing Then
                aMapping = mapping
            End If

            ' for each Member Check it with the XChangeConfig routines
            For Each aMember In _members
                '**
                'If aMember.ordinal = 32 Then Debug.Assert False

                If Not aMapping.ContainsKey(key:=aMember.ordinal.Value) Then
                    Call aMapping.Add(key:=aMember.ordinal.Value, value:=aMember.Value)
                End If
            Next aMember

            ' add the workspaceID to the MAPPING
            aValue = aConfig.GetMemberValue(ID:="WS", mapping:=aMapping)
            If IsNull(aValue) Then
                Call aConfig.AddAttributeByID(id:="WS", xcmd:=otXChangeCommandType.Read, isXChanged:=False)
                aValue = Me._queue.WorkspaceID
                If aValue = "" Then
                    aValue = CurrentSession.CurrentWorkspaceID
                End If
                ' add the change Member
                aConfigmember = aConfig.AttributeByID("WS")
                If aMapping.ContainsKey(key:=aConfigmember.ordinal.Value) Then
                    Call aMapping.Remove(key:=aConfigmember.ordinal.Value)
                End If
                Call aMapping.Add(key:=aConfigmember.ordinal.Value, value:=aValue)
            End If

            mapping = aMapping
            FillMapping = True
        End Function
        ''' <summary>
        ''' Run XChange on the Enry
        ''' </summary>
        ''' <param name="justprecheck"></param>
        ''' <param name="MSGLOG"></param>
        ''' <param name="MAPPING"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunXChange(Optional justprecheck As Boolean = False, _
                                   Optional ByRef MSGLOG As clsOTDBMessagelog = Nothing, _
                                   Optional ByRef MAPPING As Dictionary(Of Object, Object) = Nothing) As Boolean
            Dim aMapping As New Dictionary(Of Object, Object)
            Dim aConfig As clsOTDBXChangeConfig
            Dim aConfigmember As clsOTDBXChangeMember


            If Not _IsLoaded And Not Me.IsCreated Then
                runXChange = False
                Exit Function
            End If

            If Not Me.IsActionProcessable Then
                runXChange = False
                Exit Function
            End If


            If Me._queue Is Nothing Then
                Call CoreMessageHandler(subname:="clsOTDBMessageQueueEntry.runXChange", arg1:=Me.Tag, _
                                      message:="queue couldn't be loaded")

                runXChange = False
                Exit Function
            Else
                aConfig = Me._queue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOTDBMessageQueueEntry.runXChange", _
                                          arg1:=Me._queue.XChangeConfigName, _
                                          message:="XChangeConfig couldn't be loaded")

                    runXChange = False
                    Exit Function
                End If

            End If

            '***
            If Not MAPPING Is Nothing Then
                aMapping = MAPPING
            End If

            If Not Me.FillMapping(aMapping) Then
                Call CoreMessageHandler(subname:="clsOTDBMessageQueueEntry.runXChange", arg1:=Me.Tag, _
                                      message:="mapping couldn't be filled")
                runXChange = False
                Exit Function
            End If

            'msglog
            If MSGLOG Is Nothing Then
                MSGLOG = New clsOTDBMessagelog
                MSGLOG.Create(Me.msglogtag)
                s_msglog = MSGLOG
            End If

            '
            ' check the object command
            ' set it to the highes command necessary
            For Each aConfigmember In aConfig.ObjectsByOrderNo
                If aConfigmember.IsXChanged Then
                    aConfigmember.XChangeCmd = aConfig.GetHighestObjectXCmd(aConfigmember.Objectname)
                End If
            Next aConfigmember

            ' call the precheck function
            runXChange = aConfig.RunXPreCheck(aMapping, MSGLOG)

            '** exit here just on Precheck
            If justprecheck Then
                Exit Function
            End If

            '** check on the status -> possible to continue ?
            runXChange = aConfig.RunXChange(aMapping, MSGLOG)

            MAPPING = aMapping

        End Function

        '***** raiseMessage informs the Receiver about the Message-Event
        '*****
        Public Function RaiseMessage(ByVal index As Long, _
                                                   ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.raiseMessage
            '*** implement
            Throw New NotImplementedException
            '***
        End Function

        '***** hands over the msglog object to the receiver
        '*****
        Public Function AttachMessageLog(ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.attachMessageLog
            s_msglog = MSGLOG
        End Function


        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property ContextIdentifier() As String Implements otLoggable.ContextIdentifier
            Get
                ContextIdentifier = s_ContextIdentifier
            End Get
            Set(value As String)
                s_ContextIdentifier = value
            End Set
        End Property


        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property TupleIdentifier() As String Implements otLoggable.TupleIdentifier
            Get
                TupleIdentifier = s_TupleIdentifier
            End Get
            Set(value As String)
                s_EntitityIdentifier = value
            End Set
        End Property


        '***** TupleIdentifier(identifier) sets the context of the message receiver
        '*****
        Public Property EntitityIdentifier As String Implements otLoggable.EntitityIdentifier
            Get
                EntitityIdentifier = s_EntitityIdentifier
            End Get
            Set(value As String)
                s_EntitityIdentifier = value
            End Set
        End Property


    End Class
    '************************************************************************************
    '***** CLASS clsTODBMessageQueueMember is a helper for the FieldDesc Attributes
    '*****
    '*****
    Public Class clsOTDBMessageQueueMember
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable


        Public Const constTableID = "tblMessageQueueMember"

        Public Const ConstFNTag = "tag"
        Public Const ConstFNRowno = "rowno"
        Public Const ConstFNIDNo = "idno"

        ' keys
        Private s_tag As String = ""
        Private s_rowno As Long
        Private s_id As Long

        Private s_xcmd As otXChangeCommandType
        Private s_xid As String = ""
        Private s_objectname As String = ""
        Private s_fieldname As String = ""
        Private s_data As Object
        Private s_datatype As otFieldDataType
        Private s_msglogtag As String = ""
        Private s_status As String = ""
        Private s_timestamp As Date = ConstNullDate
        Private s_isSuccess As Boolean
        Private s_isToBeApproved As Boolean
        Private s_isApproved As Boolean
        Private s_filedOn As Date = ConstNullDate
        Private s_approvedOn As Date = ConstNullDate
        Private s_isProcessable As Boolean
        Private s_ordinal As Ordinal = New Ordinal(0)

        '** dynmaic
        Public _entry As clsOTDBMessageQueueEntry
        Private s_msglog As New clsOTDBMessagelog


        '** initialize
        Public Sub New()
            Call MyBase.New(constTableID)
        End Sub

#Region "Properties"

        ReadOnly Property TAG() As String
            Get
                TAG = s_tag
            End Get

        End Property

        ReadOnly Property Rowno() As Long
            Get
                rowno = s_rowno
            End Get

        End Property
        ReadOnly Property ID() As Long
            Get
                ID = s_id
            End Get

        End Property
        'Public Property Let id(value As String)
        '    If LCase(s_id) <> LCase(value) Then
        '        s_id = value
        '        me.ischanged = True
        '    End If
        'End Property

        Public Property XChangeCmd() As otXChangeCommandType
            Get
                xChangeCmd = s_xcmd
            End Get
            Set(value As otXChangeCommandType)
                If LCase(s_xcmd) <> LCase(value) Then
                    s_xcmd = value
                    Me.IsChanged = True
                End If
            End Set
        End Property


        Public Property XChangeID() As String
            Get
                xChangeID = s_xid
            End Get
            Set(value As String)
                If LCase(s_xid) <> LCase(value) Then
                    s_xid = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Objectname() As String
            Get
                OBJECTNAME = s_objectname
            End Get
            Set(value As String)
                If LCase(s_objectname) <> LCase(value) Then
                    s_objectname = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Entryname() As String
            Get
                Entryname = s_fieldname
            End Get
            Set(value As String)
                If LCase(s_fieldname) <> LCase(value) Then
                    s_fieldname = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Value() As Object
            Get
                Value = s_data
            End Get
            Set(value As Object)
                If s_data Is Nothing OrElse Not s_data.Equals(value) Then
                    s_data = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property [ordinal]() As Ordinal
            Get
                ordinal = s_ordinal
            End Get
            Set(value As Ordinal)
                If s_ordinal Is Nothing OrElse Not s_ordinal.Equals(value) Then
                    s_ordinal = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Datatype() As otFieldDataType
            Get
                DATATYPE = s_datatype
            End Get
            Set(value As otFieldDataType)
                If s_datatype <> value Then
                    s_datatype = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode() As String
            Get
                statuscode = s_status
            End Get
            Set(value As String)
                If LCase(s_status) <> LCase(value) Then
                    s_status = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        Public Property ProcessedOn() As Date
            Get
                processedOn = s_timestamp
            End Get
            Set(value As Date)
                If s_timestamp <> value Then
                    s_timestamp = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsSuccessfullProcessed() As Boolean
            Get
                isSuccessfullProcessed = s_isSuccess
            End Get
            Set(value As Boolean)
                If s_isSuccess <> value Then
                    s_isSuccess = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property ApprovedOn() As Date
            Get
                approvedOn = s_approvedOn
            End Get
            Set(value As Date)
                If s_approvedOn <> value Then
                    s_approvedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsToBeApproved() As Boolean
            Get
                isToBeApproved = s_isToBeApproved
            End Get
            Set(value As Boolean)
                If s_isToBeApproved <> value Then
                    s_isToBeApproved = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property FiledForApprovalOn() As Date
            Get
                filedForApprovalOn = s_approvedOn
            End Get
            Set(value As Date)
                If s_filedOn <> value Then
                    s_filedOn = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsApproved() As Boolean
            Get
                isApproved = s_isApproved
            End Get
            Set(value As Boolean)
                If s_isApproved <> value Then
                    s_isApproved = value
                    Me.IsChanged = True
                End If
            End Set
        End Property

        Public Property IsProcessable() As Boolean
            Get
                isProcessable = s_isProcessable
            End Get
            Set(value As Boolean)
                If s_isProcessable <> value Then
                    s_isProcessable = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        '****** getUniqueTag
        Public Function GetUniqueTag()
            getUniqueTag = ConstDelimiter & constTableID & ConstDelimiter & s_tag & ConstDelimiter & s_rowno & ConstDelimiter & s_id
        End Function
        ReadOnly Property Msglogtag() As String
            Get
                If s_msglogtag = "" Then
                    s_msglogtag = GetUniqueTag()
                End If
                msglogtag = s_msglogtag
            End Get

        End Property
#End Region

        ''' <summary>
        ''' Infuse the object by the record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            Try
                s_tag = CStr(record.GetValue(ConstFNTag))
                s_rowno = CLng(record.GetValue(constFNRowno))
                s_id = CLng(record.GetValue(constFNIdNo))

                s_xcmd = CStr(record.GetValue("xcmd"))
                s_xid = CStr(record.GetValue("xid"))

                s_objectname = CStr(record.GetValue("objectname"))
                s_fieldname = CStr(record.GetValue("fieldname"))
                s_data = (record.GetValue("data"))
                s_datatype = CLng(record.GetValue("datatype"))
                s_msglogtag = CStr(record.GetValue("msglogtag"))
                s_status = CStr(record.GetValue("status"))
                s_ordinal = record.GetValue("ordinal")

                If Not IsNull(record.GetValue("timestamp")) Then
                    s_filedOn = CDate(record.GetValue("timestamp"))
                Else
                    s_filedOn = ConstNullDate
                End If
                s_isSuccess = CBool(record.GetValue("issuccess"))
                s_isToBeApproved = CBool(record.GetValue("istobeappr"))
                s_isApproved = CBool(record.GetValue("isapproved"))

                If Not IsNull(record.GetValue("approvedon")) Then
                    s_filedOn = CDate(record.GetValue("approvedon"))
                Else
                    s_filedOn = ConstNullDate
                End If
                If Not IsNull(record.GetValue("filedon")) Then
                    s_filedOn = CDate(record.GetValue("filedon"))
                Else
                    s_filedOn = ConstNullDate
                End If
                s_isProcessable = CBool(record.GetValue("isprocessable"))

                _IsLoaded = MyBase.Infuse(record)
                Return Me.IsLoaded

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueueMember.infuse")
                Return False
            End Try



        End Function

        ''' <summary>
        ''' load the object from the persistence store
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal TAG As String, ByVal rowno As Long, ByVal ID As Long) As Boolean
            Dim pkarry() As Object = {LCase(TAG), rowno, ID}
            Return MyBase.LoadBy(pkArray:=pkarry)
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
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = constTableID
            aFieldDesc.Aliases = New String() {}

            With aTable
                .Create(constTableID)
                .Delete()
                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message queue tag"
                aFieldDesc.ID = "mqfm1"
                aFieldDesc.ColumnName = ConstFNTag
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'index pos
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "rowno index (primary key)"
                aFieldDesc.ColumnName = constFNRowno
                aFieldDesc.ID = "mqfm2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'index pos
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "posno in index (primary key)"
                aFieldDesc.ColumnName = constFNIdNo
                aFieldDesc.ID = "mqfm3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "xcmd"
                aFieldDesc.ColumnName = "xcmd"
                aFieldDesc.Size = 50
                aFieldDesc.ID = "mqfm4"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msgid
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "id"
                aFieldDesc.ColumnName = "xid"
                aFieldDesc.Size = 20
                aFieldDesc.ID = "mqfm5"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "objectname"
                aFieldDesc.ColumnName = "objectname"
                aFieldDesc.ID = "mqfm6"
                aFieldDesc.Size = 100
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "fieldname"
                aFieldDesc.ColumnName = "fieldname"
                aFieldDesc.ID = "mqfm7"
                aFieldDesc.Size = 50
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "data"
                aFieldDesc.ColumnName = "data"
                aFieldDesc.ID = "mqfm8"
                aFieldDesc.Size = 0
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "datatype"
                aFieldDesc.ColumnName = "datatype"
                aFieldDesc.ID = "mqfm9"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message log tag"
                aFieldDesc.ColumnName = "msglogtag"
                aFieldDesc.ID = "mqfm10"
                aFieldDesc.Size = 100
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status of message"
                aFieldDesc.ColumnName = "status"
                aFieldDesc.ID = "mqfm11"
                aFieldDesc.Relation = New String() {}
                aFieldDesc.Size = 100
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last processed timestamp"
                aFieldDesc.ColumnName = "timestamp"
                aFieldDesc.ID = "mqfm12"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "successful processed"
                aFieldDesc.ColumnName = "issuccess"
                aFieldDesc.ID = "mqfm13"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is to be approved"
                aFieldDesc.ColumnName = "istobeappr"
                aFieldDesc.ID = "mqfm14"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is to be approved"
                aFieldDesc.ColumnName = "isapproved"
                aFieldDesc.ID = "mqfm15"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "approved timestamp"
                aFieldDesc.ColumnName = "approvedon"
                aFieldDesc.ID = "mqfm16"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "filed for approval timestamp"
                aFieldDesc.ColumnName = "filedon"
                aFieldDesc.ID = "mqfm17"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is processable"
                aFieldDesc.ColumnName = "isprocessable"
                aFieldDesc.ID = "mqfm18"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "ordinal"
                aFieldDesc.ColumnName = "ordinal"
                aFieldDesc.ID = "mqfm19"
                aFieldDesc.Relation = New String() {}
                aFieldDesc.Size = 100
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                aFieldDesc.Relation = New String() {}
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            createSchema = True
            Exit Function

            ' Handle the error
error_handle:
            Call CoreMessageHandler(subname:="clsOTDBMessageQueueMember.createSchema")
            createSchema = False
        End Function

        ''' <summary>
        ''' persist the Object
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If


            Try
                Call Me.Record.SetValue(ConstFNTag, s_tag)
                Call Me.Record.SetValue(constFNRowno, s_rowno)
                Call Me.Record.SetValue(constFNIdNo, s_id)
                Call Me.Record.SetValue("xcmd", s_xcmd)
                Call Me.Record.SetValue("xid", s_xid)
                Call Me.Record.SetValue("objectname", s_objectname)
                Call Me.Record.SetValue("fieldname", s_fieldname)
                Call Me.Record.SetValue("ordinal", s_ordinal)
                Call Me.Record.SetValue("data", s_data)
                Call Me.Record.SetValue("datatype", s_datatype)
                Call Me.Record.SetValue("msglogtag", s_msglogtag)
                Call Me.Record.SetValue("status", s_status)
                Call Me.Record.SetValue("timestamp", s_timestamp)
                Call Me.Record.SetValue("issuccess", s_isSuccess)
                Call Me.Record.SetValue("istobeappr", s_isToBeApproved)
                Call Me.Record.SetValue("isapproved", s_isApproved)
                Call Me.Record.SetValue("approvedon", s_approvedOn)
                Call Me.Record.SetValue("filedon", s_filedOn)
                Call Me.Record.SetValue("isprocessable", s_isProcessable)
                Return MyBase.Persist(timestamp)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageQueueMember.Persist")
                Return False
            End Try



        End Function
        ''' <summary>
        ''' create a persistable object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal tag As String, ByVal rowno As Long, ByVal ID As Long) As Boolean
            Dim pkarry() As Object = {LCase(tag), rowno, ID}

            If MyBase.Create(pkArray:=pkarry, checkUnique:=False) Then
                ' set the primaryKey
                s_tag = LCase(tag)
                s_rowno = rowno
                s_id = ID
            End If

            Return Me.IsCreated

        End Function

    End Class
End Namespace
