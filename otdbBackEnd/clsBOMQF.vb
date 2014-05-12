
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

    ''' <summary>
    ''' CLASS MessageQueue is a persistable transactional interface object for exchanging data with the
    ''' OnTrack Database via Messages
    ''' </summary>
    ''' <remarks>
    ''' Design principles
    ''' 
    ''' 1. Create a Message queue and assign a xconfiguration
    ''' 2. Add messages by the function .CreateMessage or the Property .messages.addnew. Both will return 
    '''    a new message which is attached
    '''
    ''' </remarks>
    ''' 
    <ormObject(Version:=2, id:=MessageQueue.ConstObjectID, modulename:=ConstModuleXChange, _
        Description:="message queue object with message entries as interacting transactional interface with ontrack", _
        title:="Message Queue", adddeletefieldbehavior:=True, usecache:=True)> _
    Public Class MessageQueue
        Inherits ormDataObject
        Implements otLoggable
        Implements iormInfusable
        Implements iormPersistable

        ''' <summary>
        ''' Object 
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstObjectID = "MessageQueue"

        ''' <summary>
        ''' TableDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(adddeletefieldbehavior:=True, Version:=2)> Public Const ConstTableID = "TBLMESSAGEQUEUES"

        ''' <summary>
        ''' Primary Key
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, primarykeyordinal:=1, _
            properties:={ObjectEntryProperty.Keyword}, _
            title:="ID", description:="id of the message queue")> Public Const ConstFNID = "id"

        ''' <summary>
        ''' persistable column entries 
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(ReferenceObjectEntry:=Commons.Workspace.ConstObjectID & "." & Commons.Workspace.ConstFNID, isnullable:=True, _
            Title:="WorkspaceID", description:="ID of the default workspace of this message queue")> Public Const ConstFNWorkspaceID = "DEFAULTWSPACE"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Domain.ConstObjectID & "." & Commons.Domain.ConstFNDomainID, isnullable:=True, _
            useforeignkey:=otForeignKeyImplementation.None, _
            Title:="DomainID", description:="ID of the domain for this message queue")> Public Const ConstFNDomainID = Commons.Domain.ConstFNDomainID

        <ormObjectEntry(ReferenceObjectEntry:=XChangeConfiguration.constObjectID & "." & XChangeConfiguration.constFNID, isnullable:=True, _
            Title:="Title", description:="Title oder header of the message queue")> Public Const ConstFNXConfigID = XChangeConfiguration.constFNID

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
           Title:="Title", description:="Title oder header of the message queue")> Public Const ConstFNTitle = "TITLE"
        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
          Title:="Comment", description:="descriptive text comment for this message queue")> Public Const ConstFNComment = "COMMENT"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.OrgUnit.ConstObjectID & "." & Commons.OrgUnit.ConstFNID, isnullable:=True, _
             Title:="Requesting OrgUnit", description:="organization unit which is requesting the messages")> Public Const ConstFNREQOU = "REQOU"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Person.ConstObjectID & "." & Commons.Person.constFNID, isnullable:=True, _
             Title:="Request Person", description:="responsible person who is requesting the messages")> Public Const ConstFNREQPERSON = "REQPERSON"

        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
         Title:="Request Date", description:="date on which the message queue was issued")> Public Const ConstFNReqDate = "REQDATE"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
        Title:="Processed Timestamp", description:="Timestamp of last processed")> Public Const ConstFNProcStamp = "PROCSTAMP"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.StatusItem.ConstObjectID & "." & Commons.StatusItem.constFNCode, isnullable:=True, _
            Title:="Processed Status", description:="status code of the last process run")> Public Const ConstFNProcStatus = "PROCSTATUS"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.User.ConstObjectID & "." & Commons.User.ConstFNUsername, isnullable:=True, _
           Title:="Processor", description:="username of processed message queue")> Public Const ConstFNProcUser = "PROCUSER"

        ''' <summary>
        ''' Member Mapping
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNID)> Private _id As String = ""

        <ormEntryMapping(entryname:=ConstFNREQPERSON)> Private _requestingPerson As String
        <ormEntryMapping(entryname:=ConstFNREQOU)> Private _requestingOU As String
        <ormEntryMapping(entryname:=ConstFNReqDate)> Private _requestDate As Date

        <ormEntryMapping(entryname:=ConstFNWorkspaceID)> Private _workspaceID As String
        <ormEntryMapping(entryname:=ConstFNDomainID)> Private _domainID As String

        <ormEntryMapping(entryname:=ConstFNXConfigID)> Private _XConfigID As String

        <ormEntryMapping(entryname:=ConstFNTitle)> Private _title As String
        <ormEntryMapping(entryname:=ConstFNComment)> Private _cmt As String

        <ormEntryMapping(entryname:=ConstFNProcStamp)> Private _procTimeStamp As DateTime
        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _procStatus As String
        <ormEntryMapping(entryname:=ConstFNProcUser)> Private _procUsername As String

        ''' <summary>
        ''' Relation to XCOnfig ID
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkobject:=GetType(XChangeConfiguration), toprimarykeys:={ConstFNXConfigID}, _
            cascadeoncreate:=False, cascadeonDelete:=False, cascadeonUpdate:=False)> Public Const ConstRXConfig = "RElXConfig"

        <ormEntryMapping(relationname:=ConstRXConfig, infusemode:=otInfuseMode.OnDemand)> Private _xchangeconfig As New XChangeConfiguration

        ''' <summary>
        ''' Relation to Mesages
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormRelation(linkobject:=GetType(MQMessage), fromentries:={ConstFNID}, toentries:={MQMessage.ConstFNMQID}, _
          cascadeoncreate:=False, cascadeonDelete:=True, cascadeonUpdate:=True)> Public Const ConstRXMessages = "RELMESSAGES"

        <ormEntryMapping(relationname:=ConstRXMessages, infusemode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand)> _
        Private WithEvents _messages As ormRelationNewableCollection(Of MQMessage) = New ormRelationCollection(Of MQMessage)(Me, keyentrynames:={MQMessage.constFNIDNO})




        '** for ERROR MSG
        Private s_ContextIdentifier As String
        Private s_TupleIdentifier As String
        Private s_EntitityIdentifier As String


        '** not saved -> ordinals of the special MQF Columns -> for write back and preprocess
        Public Actionordinal As Object
        Public ProcessStatusordinal As Object
        Public ProcessDateordinal As Object
        Public ProcessLogordinal As Object
        'Private s_msglog As New ObjectMessageLog





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

        ''' <summary>
        ''' gets the ID of the message queue
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
        ''' gets the size of the message queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Size() As Long
            Get
                Size = _messages.Count
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the requestedBy-Person
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RequestedBy() As String
            Get
                Return _requestingPerson
            End Get
            Set(value As String)
                SetValue(ConstFNREQPERSON, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the requesting organization unit id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RequestedByOU() As String
            Get
                Return _requestingOU
            End Get
            Set(value As String)
                SetValue(ConstFNREQOU, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the default workspace id
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
        ''' returns the messages
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Messages() As iormRelationalCollection(Of MQMessage)
            Get
                Return _messages
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the xchange configuration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XChangeConfigName() As String
            Get
                Return _XConfigID
            End Get
            Set(value As String)
                SetValue(ConstFNXConfigID, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the status code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Statuscode() As String
            Get
                Return _procStatus
            End Get
            Set(value As String)
                SetValue(ConstFNProcStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the title of the message queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title() As String
            Get
                Return _title
            End Get
            Set(value As String)
                SetValue(ConstFNTitle, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the processor username
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessedByUsername() As String
            Get
                Return _procUsername
            End Get
            Set(value As String)
                SetValue(ConstFNProcUser, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Comment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Comment() As String
            Get
                Return _cmt
            End Get
            Set(value As String)
                SetValue(ConstFNComment, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Process Time stamp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Processdate() As DateTime?
            Get
                Return _procTimeStamp
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNProcStamp, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the request date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RequestedOn() As Date?
            Get
                Return _requestDate
            End Get
            Set(value As Date?)
                SetValue(ConstFNReqDate, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the XChangeConfiguration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XChangeConfig() As XChangeConfiguration
            Get
                If Me.XChangeConfigName IsNot Nothing AndAlso Me.XChangeConfigName <> "" Then
                    If Me.GetRelationStatus(ConstRXConfig) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRXConfig)
                Else
                    _xchangeconfig = Nothing
                End If
                Return _xchangeconfig
            End Get
            Set(value As XChangeConfiguration)
                If value IsNot Nothing Then Me.XChangeConfigName = value.Configname
                _xchangeconfig = value
            End Set
        End Property

      
#End Region

       
        ''' <summary>
        ''' generates the key for the messages
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MessageQueue_OnKeysRequested(sender As Object, e As ormRelationNewableCollection(Of MQMessage).EventArgs) Handles _messages.RequestKeys

            ''' create a idno for the message
            Dim max As Long
            If _messages.Keys.Count > 0 Then
                max = _messages.Keys.Select(Function(x) x.Values(0)).Max
            Else
                max = 0
            End If

            e.Keys = {max + 1}
        End Sub

        ''' <summary>
        ''' generates the key for the messages
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MessageQueue_OnNew(sender As Object, e As ormRelationNewableCollection(Of MQMessage).EventArgs) Handles _messages.OnNew

            ''' set the key entry to connect to this Messagequeue
            If e.Dataobject IsNot Nothing Then
                e.Dataobject.SetValue(MQMessage.ConstFNMQID, Me.ID)
            End If
        End Sub
        ''' <summary>
        ''' creates a new Message with optional idno and returns it
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateMessage(Optional no As Long? = Nothing) As MQMessage
            If no.HasValue Then
                Return _messages.AddCreate({no})
            Else
                Return _messages.AddCreate()
            End If
        End Function
       
        
        ''' <summary>
        ''' create a persistable message queue object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(id As String) As MessageQueue
            Return ormDataObject.CreateDataObject(Of MessageQueue)(pkArray:={id.ToUpper}, checkUnique:=True)
        End Function
        ''' <summary>
        ''' retrieves a message queue object from store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(id As String) As MessageQueue
            Return ormDataObject.Retrieve(Of MessageQueue)(pkArray:={id.ToUpper})
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
            For Each aMQFRowEntry In Me.Messages
                ' if processable than process
                If aMQFRowEntry.Processable Then
                    Process = Process And aMQFRowEntry.RunXChange()
                End If
            Next aMQFRowEntry

            Return Process
        End Function


        ''' <summary>
        ''' precheck -> check the MQF
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Precheck() As Boolean
            Precheck = True
            ' step through the RowEntries
            For Each aMQFRowEntry In Me.Messages
                Precheck = Precheck And aMQFRowEntry.RunPreCheck()
            Next aMQFRowEntry

            Return Precheck
        End Function

    End Class

    ''' <summary>
    ''' MQMessage is a message object to the Database
    ''' </summary>
    ''' <remarks>
    ''' design principles
    ''' 
    ''' 1) a message is created by the queue-function .createMessage
    ''' 
    ''' 2) the idno is the row no or any other ordinal number
    ''' 
    ''' 3) the message consists out of multiples slots - create them by the .CreateSlot method
    ''' 
    ''' 4) the message is been used to build an XEnvelope at runtime
    ''' 
    ''' </remarks>
    ''' 
    <ormObject(Version:=2, id:=MQMessage.ConstObjectID, modulename:=ConstModuleXChange, _
       Description:="message object of a message queue as interacting transactional interface with ontrack", _
       title:="Message", adddeletefieldbehavior:=True, usecache:=True)> _
    Public Class MQMessage
        Inherits ormDataObject
        Implements otLoggable
        Implements iormInfusable
        Implements iormPersistable

        ''' <summary>
        ''' Object ID
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstObjectID = "MQMessage"

        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True)> Const ConstTableID = "TBLMQMESSAGES"

        ''' <summary>
        ''' Primary Keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNID, primarykeyordinal:=1, _
         useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNMQID = "MQID"

        <ormObjectEntry(typeid:=otDataType.Long, primarykeyordinal:=2, lowerRange:=0, _
            Title:="ID", description:="ordinal ID of the message")> Public Const constFNIDNO = "IDNO"

        ''' <summary>
        ''' Column Entries
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, _
            properties:={ObjectEntryProperty.Keyword}, _
            title:="Action", description:="Transaction to be carried out with the slots")> Public Const ConstFNAction = "ACTION"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                    title:="Processed", description:="is message processed")> Public Const ConstFNProcessed = "PROCESSED"

        <ormObjectEntry(typeid:=otDataType.Bool, isnullable:=True, _
                     title:="Processable", description:="is message processable")> Public Const ConstFNProcessable = "PROCESSABLE"

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStamp _
          )> Public Const ConstFNPROCSTAMP = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStatus _
          )> Public Const ConstFNProcStatus = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Domain.ConstObjectID & "." & Commons.Domain.ConstFNDomainID, isnullable:=True, _
           useforeignkey:=otForeignKeyImplementation.None, _
           Title:="DomainID", description:="ID of the domain for this message")> Public Const ConstFNDomainID = Commons.Domain.ConstFNDomainID

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNMQID)> Private _messagequeueID As String = ""
        <ormEntryMapping(entryname:=constFNIDNO)> Private _idno As Long

        <ormEntryMapping(entryname:=ConstFNAction)> Private _action As String

        <ormEntryMapping(entryname:=ConstFNProcessed)> Private _processed As Boolean
        <ormEntryMapping(entryname:=ConstFNProcessable)> Private _processable As Boolean?
        <ormEntryMapping(entryname:=ConstFNPROCSTAMP)> Private _processedOn As DateTime?
        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _processstatus As String

        ''' <summary>
        ''' Relation to the Slots
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(MQXSlot), fromEntries:={ConstFNMQID, constFNIDNO}, ToEntries:={MQXSlot.ConstFNMQID, MQXSlot.ConstFNSlotNo}, _
            cascadeOnCreate:=False, cascadeOndelete:=True, cascadeOnUpdate:=True)> Public Const ConstRSlots = "RELSLOTS"

        <ormEntryMapping(relationname:=ConstRSlots)> Private WithEvents _slots As ormRelationNewableCollection(Of MQXSlot) = _
            New ormRelationNewableCollection(Of MQXSlot)(Me, keyentrynames:={MQXSlot.ConstFNSlotNo})

        '** for ERROR MSG
        Private s_ContextIdentifier As Object
        Private s_TupleIdentifier As Object
        Private s_EntitityIdentifier As Object

        Public _queue As MessageQueue 'backlink

#Region "Properties"

        ''' <summary>
        ''' gets the ID of the messageQueue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        ReadOnly Property MessageQueueID() As String
            Get
                Return _messagequeueID
            End Get

        End Property
        ''' <summary>
        ''' gets the IDNO of this message
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IDNO() As Long
            Get
                Return _idno
            End Get

        End Property
        ''' <summary>
        ''' returns true if processable, false if not, nothing not preprocessed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Processable() As Boolean?
            Get
                Return _processable
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNProcessable, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if processed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Processed() As Boolean
            Get
                Return _processable
            End Get
            Set(value As Boolean)
                SetValue(ConstFNProcessed, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the transaction name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Action() As String
            Get
                Return _action
            End Get
            Set(value As String)
                SetValue(ConstFNAction, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the statuscode
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Statuscode() As String
            Get
                Return _processstatus
            End Get
            Set(value As String)
                SetValue(ConstFNProcStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the processed timestamp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessedOn() As DateTime?
            Get
                Return _processedOn
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNPROCSTAMP, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the number of xslots
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Size() As Long
            Get
                Size = _slots.Count
            End Get

        End Property
        ''' <summary>
        ''' returns a List of Members
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Slots() As iormRelationalCollection(Of MQXSlot)
            Get
                Return _slots
            End Get
        End Property
#End Region

        ''' <summary>
        ''' generates the key for the messages
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MQMessagee_OnKeysRequested(sender As Object, e As ormRelationNewableCollection(Of MQXSlot).EventArgs) Handles _slots.RequestKeys

            ''' create a idno for the message
            Dim max As Long
            If _slots.Keys.Count > 0 Then
                max = _slots.Keys.Select(Function(x) x.Values(0)).Max
            Else
                max = 0
            End If

            e.Keys = {max + 1}
        End Sub

        ''' <summary>
        ''' generates the key for the messages
        ''' </summary>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MQMessage_OnNew(sender As Object, e As ormRelationNewableCollection(Of MQXSlot).EventArgs) Handles _slots.OnNew

            ''' set the key entry to connect to this Messagequeue
            If e.Dataobject IsNot Nothing Then
                e.Dataobject.SetValue(MQXSlot.ConstFNMQID, Me.MessageQueueID)
                e.Dataobject.SetValue(MQXSlot.ConstFNIDNO, Me.IDNO)
            End If
        End Sub
        ''' <summary>
        ''' adds a exiting slot to the message
        ''' </summary>
        ''' <param name="aNewMember"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddSlot(ByRef slot As MQXSlot) As Boolean
            If Not Me.IsAlive("AddSlot") Then Return False
            If slot.MessageQueueID <> Me.MessageQueueID Then
                CoreMessageHandler(message:="slot has different messagequeue binding", arg1:=slot.MessageQueueID, subname:="MQMessage.AddSlot", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            If slot.MessageID <> Me.IDNO Then
                CoreMessageHandler(message:="slot has different message binding", arg1:=slot.MessageID, subname:="MQMessage.AddSlot", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            _slots.Add(slot)
            Return True
        End Function
        '*** add new Member to the collection
        ''' <summary>
        ''' create a new slot by the xchange entry idno
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function CreateSlot(ByVal idno As Long) As MQXSlot
            If Not Me.IsAlive("CreateSlot") Then Return Nothing
            Return _slots.AddCreate(keys:={idno})
        End Function
       
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
                    VerifyAction = True
                    ' COMMAND FREEZE
                Case ConstMQFOpFreeze

                    VerifyAction = True
                    ' ADD AFTER NOT IMPLEMENTED YET
                Case ConstMQFOpAddAfter
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    VerifyAction = True
                    ' ADD NEW REVISION
                Case ConstMQFOpAddRevision

                    VerifyAction = True
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
                    VerifyAction = True
                Case ConstMQFOpDelete
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '"ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
                    'theMessages(n).processable = False
                    VerifyAction = True
                Case ""
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "INFO: in row #" & rowno & " empty operation code - skipped ")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_skipped
                    VerifyAction = False
                Case Else
                    'theMessages(n).log = addLog(theMessages(n).log, _
                    '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is unknown !")
                    'Set theMessages(n).status = New clsMQFStatus
                    'theMessages(n).status.code = constStatusCode_error
                    VerifyAction = False

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
                                            Optional ByRef msglog As ObjectMessageLog = Nothing) As Boolean

            If actioncommand = "" Then
                actioncommand = LCase(Me.Action)
            End If

            'msglog
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog

            ' check on it
            Select Case LCase(Trim(actioncommand))
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
                    'Call MSGLOG.AddMsg("300", Me._queue.TAG, Me.Rowno, "mqf1")
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
        ''' Create Persistable Object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal id As String, ByVal no As Long) As MQMessage
            Dim pkarry() As Object = {id.ToUpper, no}
            Return ormDataObject.CreateDataObject(Of MQMessage)(pkArray:=pkarry)
        End Function

        ''' <summary>
        ''' retrieves Persistable Object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal id As String, ByVal no As Long) As MQMessage
            Dim pkarry() As Object = {id.ToUpper, no}
            Return ormDataObject.Retrieve(Of MQMessage)(pkArray:=pkarry)
        End Function

        ''' <summary>
        ''' run the Precheck on the Entry
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RunPreCheck(Optional ByRef msglog As ObjectMessageLog = Nothing) As Boolean
            RunPreCheck = RunXChange(justprecheck:=True, msglog:=msglog)

        End Function

        ''' <summary>
        ''' Fill Mapping from the Entry
        ''' </summary>
        ''' <param name="mapping"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FillMapping(ByRef mapping As Dictionary(Of Object, Object)) As Boolean
            Dim aMapping As New Dictionary(Of Object, Object)
            Dim aMember As MQXSlot
            Dim aConfig As XChangeConfiguration
            Dim aConfigmember As IXChangeConfigEntry
            Dim aValue As Object


            If Not Me.IsLoaded And Not Me.IsCreated Then
                FillMapping = False
                Exit Function
            End If

            '** need the quueue
            If Me._queue Is Nothing Then
                Call CoreMessageHandler(subname:="MessageQueueEntry.runXChange", arg1:=Me.MessageQueueID, _
                                      message:="queue couldn't be loaded")

                FillMapping = False
                Exit Function
            Else
                aConfig = Me._queue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="MessageQueueEntry.runXChange", _
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
            For Each aMember In _slots
                '**
                'If aMember.ordinal = 32 Then Debug.Assert False

                If Not aMapping.ContainsKey(key:=aMember.ordinal.Value) Then
                    Call aMapping.Add(key:=aMember.ordinal.Value, value:=aMember.Value)
                End If
            Next aMember

            ' add the workspaceID to the MAPPING
            ' rework : aValue = aConfig.GetMemberValue(ID:="WS", mapping:=aMapping)
            If IsNull(aValue) Then
                Call aConfig.AddEntryByXID(Xid:="WS", xcmd:=otXChangeCommandType.Read, isXChanged:=False)
                aValue = Me._queue.WorkspaceID
                If aValue = "" Then
                    aValue = CurrentSession.CurrentWorkspaceID
                End If
                ' add the change Member
                aConfigmember = aConfig.GetEntryByXID("WS")
                If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
                    Call aMapping.Remove(key:=aConfigmember.Ordinal.Value)
                End If
                Call aMapping.Add(key:=aConfigmember.Ordinal.Value, value:=aValue)
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
                                   Optional ByRef msglog As ObjectMessageLog = Nothing, _
                                   Optional ByRef MAPPING As Dictionary(Of Object, Object) = Nothing) As Boolean
            Dim aMapping As New Dictionary(Of Object, Object)
            Dim aConfig As XChangeConfiguration
            Dim aConfigmember As IXChangeConfigEntry


            If Not Me.IsLoaded And Not Me.IsCreated Then
                RunXChange = False
                Exit Function
            End If

            If Not Me.IsActionProcessable Then
                RunXChange = False
                Exit Function
            End If


            If Me._queue Is Nothing Then
                Call CoreMessageHandler(subname:="MessageQueueEntry.runXChange", arg1:=Me.MessageQueueID, _
                                      message:="queue couldn't be loaded")

                RunXChange = False
                Exit Function
            Else
                aConfig = Me._queue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="MessageQueueEntry.runXChange", _
                                          arg1:=Me._queue.XChangeConfigName, _
                                          message:="XChangeConfig couldn't be loaded")

                    RunXChange = False
                    Exit Function
                End If

            End If

            '***
            If Not MAPPING Is Nothing Then
                aMapping = MAPPING
            End If

            If Not Me.FillMapping(aMapping) Then
                Call CoreMessageHandler(subname:="MessageQueueEntry.runXChange", arg1:=Me.MessageQueueID, _
                                      message:="mapping couldn't be filled")
                RunXChange = False
                Exit Function
            End If

            'msglog
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog


            '
            ' check the object command
            ' set it to the highes command necessary
            For Each aConfigmember In aConfig.ObjectsByOrderNo
                If aConfigmember.IsXChanged Then
                    aConfigmember.XChangeCmd = aConfig.GetHighestObjectXCmd(aConfigmember.Objectname)
                End If
            Next aConfigmember

            ' call the precheck function
            'runXChange = aConfig.RunXPreCheck(aMapping, MSGLOG)

            '** exit here just on Precheck
            If justprecheck Then
                Exit Function
            End If

            '** check on the status -> possible to continue ?
            'runXChange = aConfig.RunXChange(aMapping, MSGLOG)

            MAPPING = aMapping

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

    ''' <summary>
    ''' a data slot in the message
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' design principles
    ''' 
    ''' 1) IDNO of the slot is the IDNO of the XChangeEntry in the XCHangeConfiguration - therefore also the ordinals for the slot applies
    ''' 
    ''' 2) Create a Slot by the Message.CreateSlot function and not standalone
    ''' </remarks>
    ''' 
      <ormObject(Version:=2, id:=MQXSlot.ConstObjectID, modulename:=ConstModuleXChange, _
        Description:="xchange slot object of a message in the message queue to hold the xchange entry", _
        title:="Message", adddeletefieldbehavior:=True, usecache:=True)> _
    Public Class MQXSlot
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Public Const ConstObjectID = "MQXSlot"
        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(adddeletefieldbehavior:=True, version:=2)> Public Const ConstTableID = "TBLMQSLOTS"

        ''' <summary>
        ''' Primary keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=MQMessage.ConstObjectID & "." & MQMessage.ConstFNMQID, primarykeyordinal:=1)> Public Const ConstFNMQID = "MQID"
        <ormObjectEntry(referenceObjectEntry:=MQMessage.ConstObjectID & "." & MQMessage.constFNIDNO, primarykeyordinal:=2)> Public Const ConstFNIDNO = "MSGIDNO"

        <ormSchemaForeignKey(entrynames:={ConstFNMQID, ConstFNIDNO}, _
           foreignkeyreferences:={MQMessage.ConstObjectID & "." & MQMessage.ConstFNMQID, MQMessage.ConstObjectID & "." & MQMessage.constFNIDNO}, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFKMEssage = "FKMessage"

        <ormObjectEntry(referenceobjectentry:=XChangeObjectEntry.ConstObjectID & "." & XChangeObjectEntry.constFNIDNo, primarykeyordinal:=3, _
           Title:="XChangeEntry Reference", description:="reference ID of the xchange object entry of the xconfiguration")> _
        Public Const ConstFNSlotNo = "SLOTIDNO"

        ''' <summary>
        ''' Column entry
        ''' </summary>
        ''' <remarks></remarks>
        ''' 

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=otDataType.Date, dbdefaultvalue:="6", isnullable:=True, _
                    title:="datatype", Description:="datatype of the message slot value")> Public Const ConstFNDatatype = "DATATYPE"

        <ormObjectEntry(typeid:=otDataType.Text, defaultvalue:="", isnullable:=True, _
                    title:="value", Description:="text presentation of the slot value")> Public Const ConstFNvalue = "VALUE"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                     title:="Processed", description:="is message processed")> Public Const ConstFNProcessed = "PROCESSED"

        <ormObjectEntry(typeid:=otDataType.Bool, isnullable:=True, _
                     title:="Processable", description:="is message processable")> Public Const ConstFNProcessable = "PROCESSABLE"

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStamp _
          )> Public Const ConstFNPROCSTAMP = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStatus _
          )> Public Const ConstFNProcStatus = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Domain.ConstObjectID & "." & Commons.Domain.ConstFNDomainID, isnullable:=True, _
          useforeignkey:=otForeignKeyImplementation.None, _
          Title:="DomainID", description:="ID of the domain for this slot")> Public Const ConstFNDomainID = Commons.Domain.ConstFNDomainID

        ''' <summary>
        ''' Mapping
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNMQID)> Private _mqid As String = ""
        <ormEntryMapping(entryname:=ConstFNIDNO)> Private _messageidno As Long
        <ormEntryMapping(entryname:=ConstFNSlotNo)> Private _slotno As Long

        <ormEntryMapping(entryname:=ConstFNDatatype)> Private _datatype As otDataType?
        <ormEntryMapping(entryname:=ConstFNvalue)> Private _valuestring As String

        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _procStatus As String
        <ormEntryMapping(entryname:=ConstFNPROCSTAMP)> Private _ProcTimestamp As Date?
        <ormEntryMapping(entryname:=ConstFNProcessed)> Private _IsProcessed As Boolean
        <ormEntryMapping(entryname:=ConstFNProcessable)> Private _IsProcessable As Boolean


        '** dynmaic
        Private _message As MQMessage 'backlink
        Private _messagequeue As MessageQueue 'backlink
        Private _ordinal As Ordinal 'cache
        Private _data As Object
        Private _xconfigentry As IXChangeConfigEntry 'cache

#Region "Properties"

        ''' <summary>
        ''' returns the Message of the Message Queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property XChangeConfigEntry As IXChangeConfigEntry
            Get
                If _xconfigentry Is Nothing Then
                    If Me.MessageQueue IsNot Nothing Then
                        If Me.MessageQueue.XChangeConfig IsNot Nothing Then _xconfigentry = Me.MessageQueue.XChangeConfig.GetEntry(Me.ID)
                    End If
                End If

                Return _xconfigentry
            End Get
        End Property
        ''' <summary>
        ''' returns the Message of the Message Queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Message As MQMessage
            Get
                If _message Is Nothing Then _message = MQMessage.Retrieve(Me.MessageQueueID, Me.MessageID)
                Return _message
            End Get
        End Property
        ''' <summary>
        ''' returns the  Message Queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property MessageQueue As MessageQueue
            Get
                If _messagequeue Is Nothing Then _messagequeue = MessageQueue.Retrieve(Me.MessageQueueID)
                Return _messagequeue
            End Get
        End Property
        ''' <summary>
        ''' <summary>
        ''' returns the message queue id
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property MessageQueueID() As String
            Get
                Return _mqid
            End Get
        End Property
        ''' <summary>
        ''' returns the Message ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property MessageID() As Long
            Get
                Return _messageidno
            End Get

        End Property
        ''' <summary>
        ''' returns the slot ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As Long
            Get
                Return _slotno
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the value object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property Value() As Object
            Get
                Return _data
            End Get
            Set(value As Object)
                If _data Is Nothing OrElse Not _data.Equals(value) Then
                    _data = value
                    Me.IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the value text presentation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ValueString() As String
            Get
                Return _valuestring
            End Get
            Set(value As String)
                SetValue(ConstFNvalue, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the ordinal of the xchange config entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public ReadOnly Property [ordinal]() As Ordinal
            Get
                If _ordinal Is Nothing Then
                    If Me.XChangeConfigEntry IsNot Nothing Then _ordinal = Me.XChangeConfigEntry.Ordinal
                End If
                Return _ordinal
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the datatype
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype() As otDataType?
            Get
                Datatype = _datatype
            End Get
            Set(value As otDataType?)
                SetValue(ConstFNDatatype, value)
            End Set
        End Property
        ''' <summary>
        ''' sets the status code
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Statuscode() As String
            Get
                Return _procStatus
            End Get
            Set(value As String)
                SetValue(ConstFNProcStatus, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the processed timestamp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProcessedOn() As DateTime?
            Get
                Return _ProcTimestamp
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNPROCSTAMP, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the processed flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsProcessed() As Boolean
            Get
                Return IsProcessed
            End Get
            Set(value As Boolean)
                SetValue(ConstFNProcessed, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the Is processable flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsProcessable() As Boolean
            Get
                Return _IsProcessable
            End Get
            Set(value As Boolean)
                SetValue(ConstFNProcessable, value)
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Set the default values
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MQXSlot_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnDefaultValuesNeeded
            ' no datatype in the xconfigentry
            'Dim aMessagequeue = MessageQueue.Retrieve(Me.MessageQueueID)

            'If aMessagequeue IsNot Nothing Then
            '    If aMessagequeue.XChangeConfig IsNot Nothing Then
            '        Dim anid As Object = e.Record.GetValue(ConstFNIDNO)
            '        Dim aXConfigEntry As IXChangeConfigEntry
            '        If anid IsNot Nothing AndAlso IsNumeric(anid) Then aXConfigEntry = aMessagequeue.XChangeConfig.GetEntry(anid)
            '        If aXConfigEntry IsNot Nothing Then e.Record.SetValue(ConstFNDatatype, aXConfigEntry.)

            '    End If

            'End If
        End Sub

        ''' <summary>
        ''' Update the record from the properties
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MQXSlot_OnFeedRecord(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnFeeding


            Try
                '** special Handling
                If Me.Value IsNot Nothing Then
                    Select Case Me.Datatype
                        Case Nothing
                            Me.Datatype = otDataType.Text
                            Me.ValueString = Me.Value.ToString
                        Case otDataType.Timestamp
                            Me.ValueString = Format(Me.Value, "yyyy-MM-ddThh:mm:ss") 'format in iso
                        Case otDataType.Time
                            Me.ValueString = Format(Me.Value, "hh:mm:ss") 'format in iso
                        Case otDataType.Date
                            Me.ValueString = Format(Me.Value, "yyyy-MM-dd") 'format in iso
                        Case otDataType.Bool
                            If CBool(Me.Value) Then
                                Me.ValueString = "TRUE"
                            Else
                                Me.ValueString = "FALSE"
                            End If
                        Case otDataType.Long
                            Me.ValueString = CLng(Me.Value).ToString
                        Case otDataType.Numeric
                            Me.ValueString = CDbl(Me.Value).ToString
                        Case otDataType.Text, otDataType.Memo
                            Me.ValueString = Me.Value.ToString
                        Case Else
                            CoreMessageHandler(message:="datatype is not implemented", subname:="MQXSlot.OnFeeding", _
                                               arg1:=Me.Datatype, messagetype:=otCoreMessageType.InternalError)
                    End Select

                Else
                    Me.ValueString = Nothing
                End If


            Catch ex As Exception
                Call CoreMessageHandler(subname:="MQXslot.OnFeeding", exception:=ex)
            End Try
        End Sub

        ''' <summary>
        ''' Infuse the data object by record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub MQXSlot_OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused
            Dim aVAlue As Object


            Try
                aVAlue = Record.GetValue(ConstFNvalue)
                ' select on Datatype
                Select Case _datatype
                    Case otDataType.Numeric
                        If aVAlue IsNot Nothing Then _data = CDbl(aVAlue)
                    Case otDataType.Text, otDataType.Memo
                        If aVAlue IsNot Nothing Then _data = CStr(aVAlue)
                    Case otDataType.Runtime, otDataType.Formula, otDataType.Binary
                        _data = ""
                        Call CoreMessageHandler(subname:="MQXSlot.oninfused", messagetype:=otCoreMessageType.ApplicationError, _
                                              message:="runtime, formular, binary can't infuse", arg1:=aVAlue)
                    Case otDataType.[Date], otDataType.Timestamp
                        If Microsoft.VisualBasic.IsDate(aVAlue) Then
                            _data = CDate(aVAlue)
                        End If

                    Case otDataType.[Long]
                        If aVAlue IsNot Nothing Then _data = CLng(aVAlue)
                    Case otDataType.Bool
                        If aVAlue IsNot Nothing Then _data = CBool(aVAlue)

                    Case Else
                        Call CoreMessageHandler(subname:="MQXSlot.oninfused", _
                                              message:="unknown datatype to be infused", arg1:=aVAlue)
                End Select


            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="MQXSlot.oninfused")
            End Try


        End Sub
        ''' <summary>
        ''' create a persistable object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal mqid As String, ByVal messageidno As Long, ByVal slotidno As Long) As MQXSlot
            Dim pkarry() As Object = {mqid.ToUpper, messageidno, slotidno}
            Return ormDataObject.CreateDataObject(Of MQXSlot)(pkArray:=pkarry, checkUnique:=True)
        End Function
        ''' <summary>
        ''' create a persistable object
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <param name=constFNRowno></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal mqid As String, ByVal messageidno As Long, ByVal slotidno As Long) As MQXSlot
            Dim pkarry() As Object = {mqid.ToUpper, messageidno, slotidno}

            Return ormDataObject.Retrieve(Of MQXSlot)(pkArray:=pkarry)
        End Function
    End Class
End Namespace
