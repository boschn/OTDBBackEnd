﻿
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
Imports OnTrack.Commons
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
        Implements ormLoggable
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
            Title:="Xconfig", description:="ID of the XConfiguration for this Message Queue")> Public Const ConstFNXConfigID = XChangeConfiguration.constFNID

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
           Title:="Title", description:="Title oder header of the message queue")> Public Const ConstFNTitle = "TITLE"
        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
          Title:="Comment", description:="descriptive text comment for this message queue")> Public Const ConstFNComment = "COMMENT"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
           Title:="Plan Revision", description:="plan revision for this message queue")> Public Const ConstFNPlanRevision = "PlanRevision"


        <ormObjectEntry(ReferenceObjectEntry:=Commons.OrgUnit.ConstObjectID & "." & Commons.OrgUnit.ConstFNID, isnullable:=True, _
            Title:="Creator OrgUnit", description:="organization unit which is creating the messages")> Public Const ConstFNCREATEOU = "CREATEOU"
        <ormObjectEntry(ReferenceObjectEntry:=Commons.Person.ConstObjectID & "." & Commons.Person.constFNID, isnullable:=True, _
             Title:="Creator", description:="responsible person who is creating the messages")> Public Const ConstFNCREATEPERSON = "CREATEPERSON"
        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
         Title:="Creation Date", description:="date on which the message queue was created")> Public Const ConstFNCREATEDate = "CREATEDATE"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.OrgUnit.ConstObjectID & "." & Commons.OrgUnit.ConstFNID, isnullable:=True, _
             Title:="Requesting OrgUnit", description:="organization unit which is requesting the messages")> Public Const ConstFNREQOU = "REQOU"
        <ormObjectEntry(ReferenceObjectEntry:=Commons.Person.ConstObjectID & "." & Commons.Person.constFNID, isnullable:=True, _
             Title:="Request Person", description:="responsible person who is requesting the messages")> Public Const ConstFNREQPERSON = "REQPERSON"
        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
         Title:="Request Date", description:="date on which the message queue was issued")> Public Const ConstFNReqDate = "REQDATE"

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Person.ConstObjectID & "." & Commons.Person.constFNID, isnullable:=True, _
            Title:="Request Person", description:="responsible person who is approving the messages")> Public Const ConstFNApprovingPERSON = "APPROVEPERSON"
        <ormObjectEntry(typeid:=otDataType.Date, isnullable:=True, _
         Title:="Request Date", description:="date on which the message queue was approved")> Public Const ConstFNApprovalDate = "APPROVEDATE"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
            Title:="Precheck Timestamp", description:="Timestamp of last precheck")> Public Const ConstFNPreStamp = "PRESTAMP"
        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
                Title:="Processed Timestamp", description:="Timestamp of last processed")> Public Const ConstFNProcStamp = "PROCSTAMP"
        <ormObjectEntry(ReferenceObjectEntry:=Commons.StatusItem.ConstObjectID & "." & Commons.StatusItem.constFNCode, isnullable:=True, _
            Title:="Processed Status", description:="status code of the last process run")> Public Const ConstFNProcStatus = "PROCSTATUS"
        <ormObjectEntry(ReferenceObjectEntry:=Commons.User.ConstObjectID & "." & Commons.User.ConstFNUsername, isnullable:=True, _
           Title:="Processor", description:="username of processed message queue")> Public Const ConstFNProcUser = "PROCUSER"
        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
          Title:="Process Comment", description:="descriptive text comment for processing the message queue")> Public Const ConstFNProcComment = "ProcCOMMENT"

        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNContextID)> Public Const ConstFNContextID = ObjectMessage.ConstFNContextID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNTupleID)> Public Const ConstFNTupleID = ObjectMessage.ConstFNTupleID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNEntityID)> Public Const ConstFNEntityID = ObjectMessage.ConstFNEntityID

        ''' <summary>
        ''' Member Mapping
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNID)> Private _id As String = ""

        <ormEntryMapping(entryname:=ConstFNREQPERSON)> Private _requestingPerson As String
        <ormEntryMapping(entryname:=ConstFNREQOU)> Private _requestingOU As String
        <ormEntryMapping(entryname:=ConstFNReqDate)> Private _requestDate As DateTime?

        <ormEntryMapping(entryname:=ConstFNCREATEPERSON)> Private _creatingPerson As String
        <ormEntryMapping(entryname:=ConstFNCREATEOU)> Private _creatingOU As String
        <ormEntryMapping(entryname:=ConstFNCREATEDate)> Private _creationDate As DateTime?

        <ormEntryMapping(entryname:=ConstFNApprovingPERSON)> Private _approverperson As String
        <ormEntryMapping(entryname:=ConstFNApprovalDate)> Private _ApprovalDate As DateTime?

        <ormEntryMapping(entryname:=ConstFNWorkspaceID)> Private _workspaceID As String
        <ormEntryMapping(entryname:=ConstFNDomainID)> Private _domainID As String

        <ormEntryMapping(entryname:=ConstFNXConfigID)> Private _XConfigID As String

        <ormEntryMapping(entryname:=ConstFNTitle)> Private _title As String
        <ormEntryMapping(entryname:=ConstFNComment)> Private _cmt As String
        <ormEntryMapping(entryname:=ConstFNPlanRevision)> Private _planrevision As String

        <ormEntryMapping(entryname:=ConstFNPreStamp)> Private _preTimeStamp As DateTime?
        <ormEntryMapping(entryname:=ConstFNProcStamp)> Private _procTimeStamp As DateTime?
        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _procStatus As String
        <ormEntryMapping(entryname:=ConstFNProcUser)> Private _procUsername As String
        <ormEntryMapping(entryname:=ConstFNProcComment)> Private _procComment As String

        <ormEntryMapping(entryname:=ConstFNContextID)> Private _ContextIdentifier As String
        <ormEntryMapping(entryname:=ConstFNTupleID)> Private _TupleIdentifier As String
        <ormEntryMapping(entryname:=ConstFNEntityID)> Private _EntitityIdentifier As String
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
        Private WithEvents _messages As ormRelationNewableCollection(Of MQMessage) = New ormRelationNewableCollection(Of MQMessage)(Me, keyentrynames:={MQMessage.constFNIDNO})

        '''
        ''' dynamic members
        ''' 

        '** not saved -> ordinals of the special MQF Columns -> for write back and preprocess
        Private _Actionordinal As Object
        Private _UIDOrdinal As Object
        Private _ProcessStatusordinal As Object
        Private _ProcessDateordinal As Object
        Private _ProcessLogordinal As Object
        Private _mqfslots As New List(Of String) 'slot ids used in this message queue
        Private _XBag As XBag

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the approval date.
        ''' </summary>
        ''' <value>The approval date.</value>
        Public Property ApprovalDate() As DateTime?
            Get
                Return Me._ApprovalDate
            End Get
            Set(value As DateTime?)

                SetValue(ConstFNApprovalDate, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the approver person.
        ''' </summary>
        ''' <value>The approverperson.</value>
        Public Property ApprovedBy() As String
            Get
                Return Me._approverperson
            End Get
            Set(value As String)
                SetValue(ConstFNApprovingPERSON, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the planrevision.
        ''' </summary>
        ''' <value>The planrevision.</value>
        Public Property Planrevision() As String
            Get
                Return Me._planrevision
            End Get
            Set(value As String)

                SetValue(ConstFNPlanRevision, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the proc comment.
        ''' </summary>
        ''' <value>The proc comment.</value>
        Public Property ProcessComment() As String
            Get
                Return Me._procComment
            End Get
            Set(value As String)
                SetValue(ConstFNProcComment, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the creation date.
        ''' </summary>
        ''' <value>The creation date.</value>
        Public Property CreationDate() As DateTime?
            Get
                Return Me._creationDate
            End Get
            Set
                SetValue(ConstFNCREATEDate, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the creating OU.
        ''' </summary>
        ''' <value>The creating OU.</value>
        Public Property CreatingOU() As String
            Get
                Return Me._creatingOU
            End Get
            Set
                SetValue(ConstFNCREATEOU, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the creating person.
        ''' </summary>
        ''' <value>The creating person.</value>
        Public Property Creator() As String
            Get
                Return Me._creatingPerson
            End Get
            Set(value As String)
                SetValue(ConstFNCREATEPERSON, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the actual used slot ids.
        ''' </summary>
        ''' <value>The mqfslots.</value>
        Public ReadOnly Property UsedSlotIDs() As List(Of String)
            Get
                Return Me._mqfslots
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the process logordinal.
        ''' </summary>
        ''' <value>The process logordinal.</value>
        Public Property ProcessLogordinal() As Object
            Get
                Return Me._ProcessLogordinal
            End Get
            Set
                Me._ProcessLogordinal = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the process dateordinal.
        ''' </summary>
        ''' <value>The process dateordinal.</value>
        Public Property ProcessDateordinal() As Object
            Get
                Return Me._ProcessDateordinal
            End Get
            Set
                Me._ProcessDateordinal = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the process dateordinal.
        ''' </summary>
        ''' <value>The process dateordinal.</value>
        Public Property UIDOrdinal() As Object
            Get
                Return Me._UIDOrdinal
            End Get
            Set(value As Object)
                Me._UIDOrdinal = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the process statusordinal.
        ''' </summary>
        ''' <value>The process statusordinal.</value>
        Public Property ProcessStatusordinal() As Object
            Get
                Return Me._ProcessStatusordinal
            End Get
            Set
                Me._ProcessStatusordinal = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the  actionordinal.
        ''' </summary>
        ''' <value>The P actionordinal.</value>
        Public Property ActionOrdinal() As Object
            Get
                Return Me._Actionordinal
            End Get
            Set(value As Object)
                Me._Actionordinal = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the context identifier.
        ''' </summary>
        ''' <value>The context identifier.</value>
        Public Property ContextIdentifier() As String Implements ormLoggable.ContextIdentifier
            Get
                Return _ContextIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNContextID, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tuple identifier.
        ''' </summary>
        ''' <value>The tuple identifier.</value>
        Public Property TupleIdentifier() As String Implements ormLoggable.TupleIdentifier
            Get
                Return _TupleIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNTupleID, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the entitity identifier.
        ''' </summary>
        ''' <value>The entitity identifier.</value>
        Public Property EntityIdentifier() As String Implements ormLoggable.EntityIdentifier
            Get
                Return _EntitityIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNEntityID, value)
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
        Public Property PrecheckDate() As DateTime?
            Get
                Return _preTimeStamp
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNPreStamp, value)
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
        ''' <summary>
        ''' returns true if the MessageQueue is processable - at least one message can be processed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Processable As Boolean
            Get
                Dim result As Boolean = True
                For Each aMessage As MQMessage In Me.Messages
                    result = aMessage.Processable Or result
                Next
                Return result
            End Get
        End Property
        ''' <summary>
        ''' returns true if the MessageQueue is processed - at least one message is processed with success
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Processed As Boolean
            Get
                Dim result As Boolean = True
                For Each aMessage As MQMessage In Me.Messages
                    result = aMessage.Processed Or result
                Next
                Return result
            End Get
        End Property
#End Region
        ''' <summary>
        ''' returns the status Code of the ProcessStatus
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ProcessStatusCode() As String
            If Not Me.IsAlive("ProcessStatus") Then Return Nothing
            Dim aStatus As StatusItem = Me.ProcessStatus

            If aStatus IsNot Nothing Then Return aStatus.Code
            Return Nothing

        End Function
        ''' <summary>
        ''' returns the status Code of the ProcessStatus - nothing if not processed
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ProcessStatus() As StatusItem
            If Not Me.IsAlive("ProcessStatus") Then Return Nothing
            If Me.Processdate IsNot Nothing Then Return Me.GetHighestStatusItem()
            Return Nothing
        End Function
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
            ''' returns the highest Status Item for the Messages for this MQMessage
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
        Public Function GetHighestStatusItem() As StatusItem

            Dim highest, astatusitem As StatusItem
            For Each aMessage In Me.Messages
                If aMessage.ObjectMessageLog IsNot Nothing Then
                    astatusitem = aMessage.ObjectMessageLog.GetHighesStatusItem
                    If astatusitem IsNot Nothing Then
                        Dim aweight As Integer = astatusitem.Weight
                        If highest Is Nothing OrElse aweight > highest.Weight Then
                            highest = astatusitem
                        End If
                    End If

                End If
            Next
            Return highest

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

        ''' <summary>
        ''' returns a XBAG out of this Message Queue
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetXBag() As XBag
            If Not Me.IsAlive("GetXBAG") Then Return Nothing

            If _XBag IsNot Nothing Then Return _XBag

            ''' create a XBag
            _XBag = New XBag(Me.XChangeConfig)
            _XBag.ContextIdentifier = Me.ContextIdentifier
            Return _XBag
        End Function
        '***** 
        '*****
        ''' <summary>
        ''' process -> write the MQF to the Database through the XChangeManager
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Process(Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean
            Process = True
            Dim progress As Long
            Dim maximum As Long
            For Each aMessage As MQMessage In Me.Messages
                If aMessage.PrecheckedOn IsNot Nothing And aMessage.Processable Then maximum += 1
            Next

            ' step through the RowEntries
            For Each aMessage As MQMessage In Me.Messages
                If aMessage.PrecheckedOn IsNot Nothing And aMessage.Processable Then
                    Process = Process And aMessage.Process(workerthread:=workerthread)
                    If workerthread IsNot Nothing Then
                        progress += 1
                        workerthread.ReportProgress((progress / maximum) * 100, "processing ...")
                    End If
                End If
            Next

            Me.Processdate = DateTime.Now
            Return Process
        End Function


        ''' <summary>
        ''' precheck -> check the MQF
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Precheck(Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean
            Precheck = True
            Dim progress As Long
            Dim maximum As Long = Me.Messages.Count

            ' step through the RowEntries
            For Each aMessage As MQMessage In Me.Messages
                Precheck = Precheck And aMessage.PreCheck(workerthread)
                If workerthread IsNot Nothing Then
                    progress += 1
                    workerthread.ReportProgress((progress / maximum) * 100, "prechecking ...")
                End If
            Next
            Me.precheckdate = DateTime.Now

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
        Implements ormLoggable
        Implements iormInfusable
        Implements iormPersistable


        ''' <summary>
        ''' Class for Event Arguments
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EventArgs
            Inherits System.EventArgs

            Private _mqmessage As MQMessage
            Private _processsuccess As Boolean

            Public Sub New([mqmessage] As MQMessage, result As Boolean)
                _mqmessage = mqmessage
                _processsuccess = result
            End Sub

            ''' <summary>
            ''' Gets the processsuccess.
            ''' </summary>
            ''' <value>The processsuccess.</value>
            Public ReadOnly Property Processsuccess() As Boolean
                Get
                    Return Me._processsuccess
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the mqmessage.
            ''' </summary>
            ''' <value>The mqmessage.</value>
            Public ReadOnly Property Mqmessage() As MQMessage
                Get
                    Return Me._mqmessage
                End Get
                
            End Property

        End Class
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
                    title:="Processed", description:="is message processed with success")> Public Const ConstFNProcessed = "PROCESSED"

        <ormObjectEntry(typeid:=otDataType.Bool, isnullable:=True, _
                     title:="Processable", description:="is message processable (success on precheck)")> Public Const ConstFNProcessable = "PROCESSABLE"

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStamp _
          )> Public Const ConstFNPROCSTAMP = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStamp, _
            title:="Prechecked", Description:="timestamp when the prechecked run was done" _
          )> Public Const ConstFNPRESTAMP = "PRECSTAMP"

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStatus _
          )> Public Const ConstFNProcStatus = MessageQueue.ConstFNProcStatus

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Domain.ConstObjectID & "." & Commons.Domain.ConstFNDomainID, isnullable:=True, _
           useforeignkey:=otForeignKeyImplementation.None, _
           Title:="DomainID", description:="ID of the domain for this message")> Public Const ConstFNDomainID = Commons.Domain.ConstFNDomainID

        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNContextID)> Public Const ConstFNContextID = ObjectMessage.ConstFNContextID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNTupleID)> Public Const ConstFNTupleID = ObjectMessage.ConstFNTupleID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNEntityID)> Public Const ConstFNEntityID = ObjectMessage.ConstFNEntityID

        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNMQID)> Private _messagequeueID As String = ""
        <ormEntryMapping(entryname:=constFNIDNO)> Private _idno As Long

        <ormEntryMapping(entryname:=ConstFNAction)> Private _action As String

        <ormEntryMapping(entryname:=ConstFNProcessed)> Private _processed As Boolean
        <ormEntryMapping(entryname:=ConstFNProcessable)> Private _processable As Boolean? = True 'init value
        <ormEntryMapping(entryname:=ConstFNPROCSTAMP)> Private _processedOn As DateTime?
        <ormEntryMapping(entryname:=ConstFNPRESTAMP)> Private _precheckedOn As DateTime?

        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _processstatus As String

        <ormEntryMapping(entryname:=ConstFNContextID)> Private _ContextIdentifier As String
        <ormEntryMapping(entryname:=ConstFNTupleID)> Private _TupleIdentifier As String
        <ormEntryMapping(entryname:=ConstFNEntityID)> Private _EntitityIdentifier As String
        ''' <summary>
        ''' Relation to the Slots
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(MQXSlot), fromEntries:={ConstFNMQID, constFNIDNO}, ToEntries:={MQXSlot.ConstFNMQID, MQXSlot.ConstFNSlotID}, _
            cascadeOnCreate:=False, cascadeOndelete:=True, cascadeOnUpdate:=True)> Public Const ConstRSlots = "RELSLOTS"

        <ormEntryMapping(relationname:=ConstRSlots)> Private WithEvents _slots As ormRelationNewableCollection(Of MQXSlot) = _
            New ormRelationNewableCollection(Of MQXSlot)(Me, keyentrynames:={MQXSlot.ConstFNSlotID})

        ''' <summary>
        ''' dynamic data
        ''' </summary>
        ''' <remarks></remarks>
        Private _messagequeue As MessageQueue 'backlink
        Private _statusitem As Commons.StatusItem
        Private _envelope As XEnvelope


        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event OnPreChecked(sender As Object, e As MQMessage.EventArgs)
        Public Event OnProcessed(sender As Object, e As MQMessage.EventArgs)

#Region "Properties"

        ''' <summary>
        ''' returns a XEnvelope associated with this MQMessage
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Xenvelope As XEnvelope
            Get
                If _envelope Is Nothing And Me.MessageQueue IsNot Nothing Then
                    Dim aXBag = Me.MessageQueue.GetXBag
                    If aXBag.ContainsKey(key:=Me.IDNO) Then
                        _envelope = aXBag.Item(key:=Me.IDNO)
                    Else
                        _envelope = aXBag.AddEnvelope(key:=Me.IDNO)
                    End If

                    _envelope.TupleIdentifier = Me.TupleIdentifier
                    AddHandler _envelope.MessageLog.OnObjectMessageAdded, AddressOf MQMessage_OnEnvelopeObjectMessageAdded
                End If
                Return _envelope
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the status item.
        ''' </summary>
        ''' <value>The status item.</value>
        Public Property Statusitem() As StatusItem
            Get
                Return Me._statusitem
            End Get
            Private Set(value As StatusItem)
                Me._statusitem = value
                Me.Statuscode = value.Code
            End Set
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
        ''' sets or gets the processed timestamp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrecheckedOn() As DateTime?
            Get
                Return _precheckedOn
            End Get
            Set(value As DateTime?)
                SetValue(ConstFNPRESTAMP, value)
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
                Dim aSlotid As String = e.Dataobject.GetValue(MQXSlot.ConstFNSlotID)
                If aSlotid IsNot Nothing Then
                    If Not Me.MessageQueue.UsedSlotIDs.contains(aSlotid) Then Me.MessageQueue.usedslotids.add(aSlotid)
                End If
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
        'Public Function VerifyAction(Optional ByVal actioncommand As String = "") As Boolean

        '    If actioncommand = "" Then
        '        actioncommand = LCase(Me.Action)
        '    Else
        '        actioncommand = CStr(actioncommand)
        '    End If

        '    ' check on it
        '    Select Case LCase(Trim(actioncommand))
        '        ' CHANGE
        '        Case ConstMQFOpChange
        '            VerifyAction = True
        '            ' COMMAND FREEZE
        '        Case ConstMQFOpFreeze

        '            VerifyAction = True
        '            ' ADD AFTER NOT IMPLEMENTED YET
        '        Case ConstMQFOpAddAfter
        '            'theMessages(n).log = addLog(theMessages(n).log, _
        '            '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
        '            'Set theMessages(n).status = New clsMQFStatus
        '            'theMessages(n).status.code = constStatusCode_skipped
        '            VerifyAction = True
        '            ' ADD NEW REVISION
        '        Case ConstMQFOpAddRevision

        '            VerifyAction = True
        '            ' NOOP
        '        Case ConstMQFOpNoop
        '            'aMQFRowEntry.action = ConstMQFOpNoop
        '            'theMessages(n).log = addLog(theMessages(n).log, _
        '            '"INFO: in row #" & rowno & ": operation code '" & value & "' is meant to do nothing.")
        '            'theMessages(n).processable = theMessages(n).processable And True
        '            'Set theMessages(n).status = New clsMQFStatus
        '            'theMessages(n).status.code = constStatusCode_skipped
        '            'theMessages(n).processable = False
        '            ' DELETE NOT IMPLEMENTED YET
        '            VerifyAction = True
        '        Case ConstMQFOpDelete
        '            'theMessages(n).log = addLog(theMessages(n).log, _
        '            '"ERROR: in row #" & rowno & ": operation code '" & value & "' is not yet implemented.")
        '            'theMessages(n).processable = False
        '            VerifyAction = True
        '        Case ""
        '            'theMessages(n).log = addLog(theMessages(n).log, _
        '            '                            "INFO: in row #" & rowno & " empty operation code - skipped ")
        '            'Set theMessages(n).status = New clsMQFStatus
        '            'theMessages(n).status.code = constStatusCode_skipped
        '            VerifyAction = False
        '        Case Else
        '            'theMessages(n).log = addLog(theMessages(n).log, _
        '            '                            "ERROR: in row #" & rowno & ": operation code '" & value & "' is unknown !")
        '            'Set theMessages(n).status = New clsMQFStatus
        '            'theMessages(n).status.code = constStatusCode_error
        '            VerifyAction = False

        '    End Select

        'End Function
        ''' <summary>
        ''' Is Action Processable
        ''' </summary>
        ''' <param name="ActionCommand"></param>
        ''' <param name="MSGLOG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsActionProcessable(Optional ByVal actioncommand As String = Nothing, _
                                            Optional ByRef msglog As ObjectMessageLog = Nothing) As Boolean

            If actioncommand IsNot Nothing Then actioncommand = actioncommand.Trim.ToUpper
            If actioncommand Is Nothing Then actioncommand = Me.Action.ToUpper

            'msglog
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog

            ' check on it
            Select Case actioncommand
                ' CHANGE
                Case ConstMQFOpChange
                    IsActionProcessable = True

                    ' COMMAND FREEZE
                Case ConstMQFOpFreeze
                    '511;@;MQF;message operation '%1%' in %Tupleidentifier% is not yet implemented;Please correct;99;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                    Me.ObjectMessageLog.Add(511, Nothing, Nothing, Nothing, Nothing, ConstMQFOpFreeze)
                    IsActionProcessable = False
                    
                Case ConstMQFOpAddAfter
                    IsActionProcessable = True

                    ' ADD NEW REVISION
                Case ConstMQFOpAddRevision
                    IsActionProcessable = True

                    ' NOOP
                Case ConstMQFOpNoop
                    '573;@;MQF;message in row %Tupleidentifier% has operation NOOP which means 'DO NOTHING' - skip processing;;99;Error;false;|S1|;|MQMESSAGE|
                    msglog.Add(573, Nothing, Nothing, Nothing, Nothing)
                    IsActionProcessable = True

                Case ConstMQFOpDelete
                    IsActionProcessable = True

                Case ""
                    '513;@;MQF;message operation is missing - message not processed;;99;Error;false;|R1|S1|;|XCHANGEENVELOPE|MQMESSAGE|
                    msglog.Add(513, Nothing, Nothing, Nothing, Nothing)
                    IsActionProcessable = False
                Case Else
                    '510;@;MQF;message operation '%1%' in %Tupleidentifier% is unknown;Please correct;99;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                    msglog.Add(510, Nothing, Nothing, Nothing, Nothing, actioncommand)
                    IsActionProcessable = False

            End Select

            Return IsActionProcessable
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
        Public Function PreCheck(Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean
            Dim result As Boolean

            If Not Me.IsAlive("Precheck") Then Return False
            Dim aconfigname As String = ""
            If Me.MessageQueue IsNot Nothing AndAlso Me.MessageQueue.XChangeConfig IsNot Nothing Then
                aconfigname = Me.MessageQueue.XChangeConfig.Configname
            End If

            '''
            ''' Check the highest Status if already on aborting than leave it
            ''' 
            Dim astatusitem As Commons.StatusItem = Me.ObjectMessageLog.GetHighesStatusItem(statustype:=ConstStatusType_MQMessage)
            If astatusitem IsNot Nothing AndAlso astatusitem.Aborting Then
                '' skip it
                '575;@;MQF;message in row %Tupleidentifier% has skip status - skipping again;;99;Error;false;|S1|;|MQMESSAGE|
                Me.ObjectMessageLog.Add(575, Nothing, Nothing, Nothing, Nothing)
                result = True
                '''
                ''' check if action there
                ''' 
            ElseIf String.IsNullOrWhiteSpace(Me.Action) Then
                '513;@;MQF;message operation is missing - message not processed;;80;Error;false;|R1|;|XCHANGEENVELOPE|
                Me.ObjectMessageLog.Add(513, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier, aconfigname)
                result = False
            ElseIf Me.IsActionProcessable(Me.Action, Me.ObjectMessageLog) Then

                '''
                ''' run the commands
                ''' 

                Select Case Me.Action.ToUpper

                    Case ot.ConstMQFOpNoop
                        '''
                        ''' Do Nothing by intention
                        ''' 
                        result = True

                    Case ot.ConstMQFOpChange, ot.ConstMQFOpAddRevision, ot.ConstMQFOpAddRevision
                        '''
                        ''' run the XChange through the envelope
                        ''' 
                        Me.Processed = Me.RunXChange(justprecheck:=True, workerthread:=workerthread)
                        Me.Statusitem = Me.ObjectMessageLog.GetHighesStatusItem(ConstStatusType_XEnvelope)
                        If Me.Statusitem.Aborting Then
                            result = False
                        Else
                            result = True
                        End If

                        '******
                        '****** freeze
                    Case ot.ConstMQFOpFreeze
                        '511;@;MQF;message operation '%1%' in %Tupleidentifier% is not yet implemented;Please correct;99;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                        Me.ObjectMessageLog.Add(511, Nothing, Nothing, Nothing, Nothing, ConstMQFOpFreeze)
                        result = False 'not implemented

                        '****
                        '**** Delete Deliverable
                    Case ot.ConstMQFOpDelete
                        result = True 'not implemented

                    Case ""
                        '''
                        ''' Operation missing
                        ''' 
                        result = False
                        If Me.MessageQueue IsNot Nothing AndAlso Me.MessageQueue.XChangeConfig IsNot Nothing Then
                            aconfigname = Me.MessageQueue.XChangeConfig.Configname
                        End If
                        '513;@;MQF;message operation is missing - message not processed;;80;Error;false;|R1|;|XCHANGEENVELOPE|
                        Me.ObjectMessageLog.Add(513, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier, aconfigname, Me.Action)
                    Case Else
                        '''
                        ''' Operation not known
                        ''' 
                        result = False
                        If Me.MessageQueue IsNot Nothing AndAlso Me.MessageQueue.XChangeConfig IsNot Nothing Then
                            aconfigname = Me.MessageQueue.XChangeConfig.Configname
                        End If
                        '512;@;MQF;message operation '%2%' in xchange configuration '%1%' is invalid - operation '%2%' aborted;;90;Error;false;|R1|;|XCHANGEENVELOPE|
                        Me.ObjectMessageLog.Add(512, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier, aconfigname, _
                                                Me.Action)

                End Select
            End If


            ''' return
            Me.PrecheckedOn = Date.Now
            Me.Processable = result

            RaiseEvent OnPreChecked(Me, New MQMessage.EventArgs(MQMessage:=Me, result:=result))
            Return result
        End Function

        

        ''' <summary>
        ''' Process the Message
        ''' </summary>
        ''' <param name="workerthread"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Process(Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean
            Dim result As Boolean

            If Not Me.IsAlive("Process") Then Return False

            ''' preprocess needed first
            If Me.PrecheckedOn Is Nothing Then
                Me.ObjectMessageLog.Add(1291, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier)
                Return False
                ''' needs to be successfull
            ElseIf Not Me.Processable Then
                Me.ObjectMessageLog.Add(1292, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier)
                Return False

            ElseIf String.IsNullOrWhiteSpace(Me.Action) Then
                '1010;@;XCHANGE;xchange command '%2%' in xchange configuration '%1%' is invalid - operation '%2%' aborted;;90;Error;false;|R1|;|XCHANGEENVELOPE|
                Me.ObjectMessageLog.Add(1010, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier)
                Return False

            ElseIf Me.ObjectMessageLog.GetHighesStatusItem(statustype:=ConstStatusType_MQMessage).Code = "S1" Then
                '' skip it
                '576;@;MQF;message in row %Tupleidentifier% has skip status - skipping processing again;;99;Error;false;|S1|;|MQMESSAGE|
                Me.ObjectMessageLog.Add(576, Nothing, Nothing, Nothing, Nothing)
                Return True

            End If

            ''' 
            ''' run the commands
            ''' 

            Select Case Me.Action.ToUpper


                Case ot.ConstMQFOpNoop
                    '''
                    ''' Do Nothing by intention
                    ''' 
                    Me.PrecheckedOn = Date.Now
                    Me.Processable = True
                    Me.ObjectMessageLog.Add(1290, Nothing, Me.ContextIdentifier, Me.TupleIdentifier, Me.EntityIdentifier, Me.MessageQueue.ID, ot.ConstMQFOpNoop)
                    RaiseEvent OnProcessed(Me, New MQMessage.EventArgs(MQMessage:=Me, result:=result))

                Case ot.ConstMQFOpChange, ot.ConstMQFOpDelete
                    '''
                    ''' run the XChange through the envelope
                    ''' 
                    Me.ProcessedOn = Date.Now
                    Me.Processed = Me.RunXChange(justprecheck:=False, workerthread:=workerthread)
                    Me.Statusitem = Me.ObjectMessageLog.GetHighesStatusItem(ConstStatusType_XEnvelope)
                    If Me.Statusitem.Aborting Then
                        result = False
                    Else
                        result = True
                    End If
                    Me.Processed = result
                    RaiseEvent OnProcessed(Me, New MQMessage.EventArgs(MQMessage:=Me, result:=result))

                    'Call updateRowXlsDoc9(INPUTMAPPING:=aMapping, INPUTXCHANGECONFIG:=MQFObject.XChangeConfig)

                    '****
                    '**** ADD REVISION
                    '****
                Case ot.ConstMQFOpAddRevision

                    '''
                    ''' run the XChange through the envelope
                    ''' 
                    Me.ProcessedOn = Date.Now
                    Me.Processed = Me.RunOpAddRevision(justprecheck:=False, workerthread:=workerthread)
                    Me.Statusitem = Me.ObjectMessageLog.GetHighesStatusItem(ConstStatusType_XEnvelope)
                    If Me.Statusitem.Aborting Then
                        result = False
                    Else
                        result = True
                    End If
                    Me.Processed = result
                    RaiseEvent OnProcessed(Me, New MQMessage.EventArgs(MQMessage:=Me, result:=result))


                    '****
                    '**** ADD-AFTER
                    '****
                Case ot.ConstMQFOpAddAfter

                    '''
                    ''' run the XChange through the envelope
                    ''' 
                    Me.ProcessedOn = Date.Now
                    Me.Processed = Me.RunOpAddAfter(justprecheck:=False, workerthread:=workerthread)
                    Me.Statusitem = Me.ObjectMessageLog.GetHighesStatusItem(ConstStatusType_XEnvelope)
                    If Me.Statusitem.Aborting Then
                        result = False
                    Else
                        result = True
                    End If
                    Me.Processed = result
                    RaiseEvent OnProcessed(Me, New MQMessage.EventArgs(MQMessage:=Me, result:=result))


                    '******
                    '****** freeze
                Case ot.ConstMQFOpFreeze
                    '511;@;MQF;message operation '%1%' in %Tupleidentifier% is not yet implemented;Please correct;99;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                    Me.ObjectMessageLog.Add(511, Nothing, Nothing, Nothing, Nothing, ConstMQFOpFreeze)


                    'aMapping = New Dictionary(Of Object, Object)
                    'Call aMQFRowEntry.FillMapping(aMapping)
                    '' get UID
                    'aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="uid")
                    'If Not aConfigmember Is Nothing Then
                    '    If aConfigmember.IsLoaded Or aConfigmember.IsCreated Then
                    '        If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
                    '            anUID = aMapping.Item(key:=aConfigmember.Ordinal.Value)
                    '            aDeliverable = Deliverables.Deliverable.Retrieve(uid:=anUID)
                    '            If aDeliverable IsNot Nothing Then
                    '                If Not aDeliverable.IsDeleted Then
                    '                    '*** set the workspaceID
                    '                    ' REWORK: aValue = MQFObject.XCHANGECONFIG.GetMemberValue(ID:="WS", mapping:=aMapping)
                    '                    If IsNull(aValue) Then
                    '                        aWorkspace = CurrentSession.CurrentWorkspaceID
                    '                    Else
                    '                        aWorkspace = CStr(aValue)
                    '                    End If
                    '                    '***get the schedule
                    '                    aSchedule = aDeliverable.GetWorkScheduleEdition(workspaceID:=aWorkspace)
                    '                    If Not aSchedule Is Nothing Then
                    '                        If aSchedule.IsLoaded Then
                    '                            '*** reference date
                    '                            aRefdate = MQFObject.RequestedOn
                    '                            If aRefdate = constNullDate Then
                    '                                aRefdate = Now
                    '                            End If
                    '                            '*** draw baseline
                    '                            Call aSchedule.DrawBaseline(REFDATE:=aRefdate)
                    '                        End If
                    '                    End If
                    '                End If

                    '            End If
                    '        End If
                    '    End If
                    'End If

                Case Else
                    '510;@;MQF;message operation '%1%' in %Tupleidentifier% is unknown;Please correct;99;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                    Me.ObjectMessageLog.Add(510, Nothing, Nothing, Nothing, Nothing, Me.Action.ToUpper)
                    result = False


            End Select

            Return result

        End Function

        ''' <summary>
        ''' Run MQF Command Add-After on the MQMessage
        ''' </summary>
        ''' <param name="justprecheck"></param>
        ''' <param name="MSGLOG"></param>
        ''' <param name="MAPPING"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RunOpAddAfter(Optional justprecheck As Boolean = False, _
                                       Optional msglog As ObjectMessageLog = Nothing, _
                                       Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean

            If Not Me.IsAlive("RunOpAddAfter") Then Return False
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog

            Dim aConfig As XChangeConfiguration
            Dim aConfigEntry As IXChangeConfigEntry
            Dim anoldUID As Long?

            If Me.MessageQueue Is Nothing Then
                Call CoreMessageHandler(subname:="MQMessage.RunOpAddAfter", arg1:=Me.MessageQueueID, message:="queue couldn't be loaded", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            Else
                aConfig = Me.MessageQueue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="MQMessage.RunOpAddAfter", arg1:=Me.MessageQueue.XChangeConfigName, message:="XChangeConfig couldn't be loaded", _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '''
            ''' check for which object this is valid
            If aConfig.GetObjectByName(objectname:=Deliverable.ConstObjectID) Is Nothing Then
                '580;@;MQF;message in row %Tupleidentifier% with operation %1% is not possible for object type %2% - operation failed;;95;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                msglog.Add(580, Nothing, Nothing, Nothing, Nothing, ConstMQFOpAddAfter, Converter.Enumerable2String(aConfig.ObjectnamesByOrderNo), aConfig.Configname)
                Return False
            End If

            ' check the object command
            ' set it to the highes command necessary
            For Each aConfigEntry In aConfig.ObjectsByOrderNo
                If aConfigEntry.IsXChanged Then
                    aConfigEntry.XChangeCmd = aConfig.GetHighestObjectXCmd(aConfigEntry.Objectname)
                End If
            Next aConfigEntry

            ''' fill the envelope
            ''' 
            For Each aSlot As Xchange.MQXSlot In Me.Slots
                For Each aConfigEntry In aSlot.XChangeConfigEntries
                    If Not Me.Xenvelope.AddSlotbyXEntry(entry:=aConfigEntry, _
                                                  value:=aSlot.Value, isHostValue:=True, _
                                                  overwriteValue:=False, replaceSlotIfexists:=False, _
                                                  ValueIsNull:=aSlot.IsNull, SlotIsEmpty:=aSlot.IsEmpty) Then

                    End If
                Next
            Next

            '''
            ''' process the AddAfter
            ''' 
            anoldUID = Me.Xenvelope.GetSlotValueByObjectEntryName(entryname:=Deliverable.constFNUid, objectname:=Deliverable.ConstObjectID, asHostValue:=False)

            '' create the deliverable -> deliverable type should be in here
            Dim aDeliverable As Deliverable = Deliverables.Deliverable.Create()
            If aDeliverable Is Nothing Then
                Return False
            End If
            System.Diagnostics.Debug.Write("new deliverable added: " & aDeliverable.Uid & " to be added after uid #" & anOldUID)

            ''' extend the config
            aConfigEntry = Me.MessageQueue.XChangeConfig.GetEntryByObjectEntryName(entryname:=Deliverable.constFNUid)
            If aConfigEntry Is Nothing Then
                aConfig.AddEntryByObjectEntry(entryname:=Deliverable.constFNUid, objectname:=Deliverable.ConstObjectID, _
                                              isXChanged:=True, xcmd:=otXChangeCommandType.UpdateCreate)
            End If
            ''' add / substitute the new slot
            If Not Me.Xenvelope.AddSlotbyXEntry(entry:=aConfigEntry, _
                                              value:=aDeliverable.Uid, isHostValue:=False, _
                                              overwriteValue:=True, replaceSlotIfexists:=True, _
                                              ValueIsNull:=False, SlotIsEmpty:=False) Then

            End If


            '''
            ''' run the xchange
            ''' 
            Dim result As Boolean
            If justprecheck Then
                result = Me.Xenvelope.RunXPreCheck(msglog)
            Else
                result = Me.Xenvelope.RunXChange(msglog)
            End If

            ''' 
            ''' extend the Outline
            '''
            If result And aConfig.Outline IsNot Nothing Then
                If aConfig.Outline.DynamicBehaviour = False Then
                    Throw New NotImplementedException("Outline Update notimplemented")
                End If
            End If

            ''' return
            Return result
        End Function

        ''' <summary>
        ''' Run MQF Command Add-After on the MQMessage
        ''' </summary>
        ''' <param name="justprecheck"></param>
        ''' <param name="MSGLOG"></param>
        ''' <param name="MAPPING"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RunOpAddRevision(Optional justprecheck As Boolean = False, _
                                       Optional msglog As ObjectMessageLog = Nothing, _
                                       Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean

            If Not Me.IsAlive("RunOpAddRevision") Then Return False
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog

            Dim aConfig As XChangeConfiguration
            Dim aConfigEntry As IXChangeConfigEntry
            Dim anoldUID As Long?

            If Me.MessageQueue Is Nothing Then
                Call CoreMessageHandler(subname:="MQMessage.RunOpAddRevision", arg1:=Me.MessageQueueID, message:="queue couldn't be loaded", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            Else
                aConfig = Me.MessageQueue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="MQMessage.RunOpAddRevision", arg1:=Me.MessageQueue.XChangeConfigName, message:="XChangeConfig couldn't be loaded", _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '''
            ''' check for which object this is valid
            If aConfig.GetObjectByName(objectname:=Deliverable.ConstObjectID) Is Nothing Then
                '580;@;MQF;message in row %Tupleidentifier% with operation %1% is not possible for object type %2% - operation failed;;95;Error;false;|R1|R1|;|XCHANGEENVELOPE|MQMESSAGE|
                msglog.Add(580, Nothing, Nothing, Nothing, Nothing, ConstMQFOpAddAfter, Converter.Enumerable2String(aConfig.ObjectnamesByOrderNo), aConfig.Configname)
                Return False
            End If

            ' check the object command
            ' set it to the highes command necessary
            For Each aConfigEntry In aConfig.ObjectsByOrderNo
                If aConfigEntry.IsXChanged Then
                    aConfigEntry.XChangeCmd = aConfig.GetHighestObjectXCmd(aConfigEntry.Objectname)
                End If
            Next aConfigEntry

            ''' fill the envelope
            ''' 
            For Each aSlot As XChange.MQXSlot In Me.Slots
                For Each aConfigEntry In aSlot.XChangeConfigEntries
                    If Not Me.Xenvelope.AddSlotbyXEntry(entry:=aConfigEntry, _
                                                  value:=aSlot.Value, isHostValue:=True, _
                                                  overwriteValue:=False, replaceSlotIfexists:=False, _
                                                  ValueIsNull:=aSlot.IsNull, SlotIsEmpty:=aSlot.IsEmpty) Then

                    End If
                Next
            Next


            '' fill the Mapping
            'aMapping = New Dictionary(Of Object, Object)
            'Call aMQFRowEntry.FillMapping(aMapping)
            '' get UID
            'aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="uid")
            'If Not aConfigmember Is Nothing Then
            '    If aConfigmember.IsLoaded Or aConfigmember.IsCreated Then
            '        If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
            '            anUID = aMapping.Item(key:=aConfigmember.Ordinal.Value)
            '            aDeliverable = Deliverables.Deliverable.Retrieve(uid:=anUID)
            '            If aDeliverable Is Nothing Then
            '                '** revision ?!
            '                aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="c16")
            '                If Not aConfigmember Is Nothing Then
            '                    If aConfigmember.IsLoaded Or aConfigmember.IsCreated Then
            '                        If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
            '                            aRev = aMapping.Item(key:=aConfigmember.Ordinal.Value)
            '                        Else
            '                            aRev = ""
            '                        End If
            '                    Else
            '                        aRev = ""
            '                    End If
            '                Else
            '                    aRev = ""
            '                End If
            '                '**
            '                aNewDeliverable = aDeliverable.AddRevision(newRevision:=aRev, persist:=True)
            '                If Not aNewDeliverable Is Nothing Then
            '                    ' substitute UID
            '                    aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="uid")
            '                    Call aMapping.Remove(key:=aConfigmember.Ordinal.Value)
            '                    Call aMapping.Add(key:=aConfigmember.Ordinal.Value, value:=aNewDeliverable.Uid)
            '                    ' substitute REV
            '                    aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="c16")
            '                    If Not aConfigmember Is Nothing Then
            '                        If aConfigmember.IsLoaded Or aConfigmember.IsCreated Then
            '                            If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
            '                                Call aMapping.Remove(key:=aConfigmember.Ordinal.Value)
            '                            End If
            '                            Call aMapping.Add(key:=aConfigmember.Ordinal.Value, value:=aNewDeliverable.Revision)
            '                        End If
            '                    End If
            '                    ' substitute TYPEID or ADD
            '                    aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="SC14")
            '                    If aConfigmember Is Nothing Then
            '                        If MQFObject.XChangeConfig.AddEntryByXID(Xid:="SC14") Then
            '                            aConfigmember = MQFObject.XChangeConfig.GetEntryByXID(XID:="SC14")
            '                        End If
            '                    End If
            '                    If aConfigmember.IsLoaded Or aConfigmember.IsCreated Then
            '                        If aMapping.ContainsKey(key:=aConfigmember.Ordinal.Value) Then
            '                            Call aMapping.Remove(key:=aConfigmember.Ordinal.Value)
            '                        End If
            '                        Dim aTrack As Deliverables.Track
            '                        aTrack = aNewDeliverable.GetTrack
            '                        If Not aTrack Is Nothing Then
            '                            Call aMapping.Add(key:=aConfigmember.Ordinal.Value, value:=aTrack.Scheduletype)
            '                        End If
            '                        'Call aMapping.Add(key:=aConfigmember.ordinal.value, c:=aNewDeliverable.getTrack.SCHEDULETYPE)
            '                    End If

            '                    '*** runxchange
            '                    'Call aMQFRowEntry.RunXChange(MAPPING:=aMapping)
            '                    aMQFRowEntry.ProcessedOn = Now
            '                    'how to save new uid ?!
            '                    'Call updateRowXlsDoc9(INPUTMAPPING:=aMapping, INPUTXCHANGECONFIG:=MQFObject.XCHANGECONFIG)
            '                Else
            '                    Call CoreMessageHandler(subname:="MQF.processXLSMQF", message:="AddRevision failed", _
            '                                          arg1:=aDeliverable.Uid)
            '                End If
            '            Else
            '                Call CoreMessageHandler(subname:="MQF.processXLSMQF", message:="uid not in mapping", _
            '                                      arg1:=anUID)
            '            End If
            '        Else
            '            Call CoreMessageHandler(subname:="MQF.processXLSMQF", message:="load of Deliverable failed", _
            '                                  arg1:=aConfigmember.Ordinal.Value)
            '        End If
            '    Else
            '        Call CoreMessageHandler(subname:="MQF.processXLSMQF", message:="uid id not in configuration", _
            '                              arg1:="uid")
            '    End If
            'Else
            '    Call CoreMessageHandler(subname:="MQF.processXLSMQF", message:="uid id not in configuration", _
            '                          arg1:="uid")
            'End If


            '''
            ''' process the AddAfter
            ''' 
            anoldUID = Me.Xenvelope.GetSlotValueByObjectEntryName(entryname:=Deliverable.constFNUid, objectname:=Deliverable.ConstObjectID, asHostValue:=False)

            '' create the deliverable -> deliverable type should be in here
            Dim aDeliverable As Deliverable = Deliverables.Deliverable.Create()
            If aDeliverable Is Nothing Then
                Return False
            End If
            System.Diagnostics.Debug.Write("new deliverable added: " & aDeliverable.Uid & " to be added after uid #" & anoldUID)

            ''' extend the config
            aConfigEntry = Me.MessageQueue.XChangeConfig.GetEntryByObjectEntryName(entryname:=Deliverable.constFNUid)
            If aConfigEntry Is Nothing Then
                aConfig.AddEntryByObjectEntry(entryname:=Deliverable.constFNUid, objectname:=Deliverable.ConstObjectID, _
                                              isXChanged:=True, xcmd:=otXChangeCommandType.UpdateCreate)
            End If
            ''' add / substitute the new slot
            If Not Me.Xenvelope.AddSlotbyXEntry(entry:=aConfigEntry, _
                                              value:=aDeliverable.Uid, isHostValue:=False, _
                                              overwriteValue:=True, replaceSlotIfexists:=True, _
                                              ValueIsNull:=False, SlotIsEmpty:=False) Then

            End If


            '''
            ''' run the xchange
            ''' 
            Dim result As Boolean
            If justprecheck Then
                result = Me.Xenvelope.RunXPreCheck(msglog)
            Else
                result = Me.Xenvelope.RunXChange(msglog)
            End If

            ''' 
            ''' extend the Outline
            '''
            If result And aConfig.Outline IsNot Nothing Then
                If aConfig.Outline.DynamicBehaviour = False Then
                    Throw New NotImplementedException("Outline Update notimplemented")
                End If
            End If

            ''' return
            Return result
        End Function
        ''' <summary>
        ''' Run XChange on the Enry
        ''' </summary>
        ''' <param name="justprecheck"></param>
        ''' <param name="MSGLOG"></param>
        ''' <param name="MAPPING"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RunXChange(Optional justprecheck As Boolean = False, _
                                   Optional ByRef workerthread As ComponentModel.BackgroundWorker = Nothing) As Boolean

            If Not Me.IsAlive("RunXChange") Then Return False

            Dim aConfig As XChangeConfiguration
            Dim aConfigmember As IXChangeConfigEntry

            If Me.MessageQueue Is Nothing Then
                Call CoreMessageHandler(subname:="MQMessage.runXChange", arg1:=Me.MessageQueueID, message:="queue couldn't be loaded", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            Else
                aConfig = Me.MessageQueue.XChangeConfig
                If aConfig Is Nothing Then
                    Call CoreMessageHandler(subname:="MQMessage.runXChange", arg1:=Me.MessageQueue.XChangeConfigName, message:="XChangeConfig couldn't be loaded", _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            ' check the object command
            ' set it to the highes command necessary
            For Each aConfigmember In aConfig.ObjectsByOrderNo
                If aConfigmember.IsXChanged Then
                    aConfigmember.XChangeCmd = aConfig.GetHighestObjectXCmd(aConfigmember.Objectname)
                End If
            Next aConfigmember

            ''' fill the envelope
            ''' 
            For Each aSlot As Xchange.MQXSlot In Me.Slots
                For Each aConfigEntry As IXChangeConfigEntry In aSlot.XChangeConfigEntries
                    If Not Me.Xenvelope.AddSlotbyXEntry(entry:=aConfigEntry, _
                                                  value:=aSlot.Value, isHostValue:=True, _
                                                  overwriteValue:=False, replaceSlotIfexists:=False, _
                                                  ValueIsNull:=aSlot.IsNull, SlotIsEmpty:=aSlot.IsEmpty) Then

                    End If
                Next
            Next

            '''
            ''' run
            ''' 
            Dim result As Boolean
            If justprecheck Then
                result = Me.Xenvelope.RunXPreCheck(Me.ObjectMessageLog)
            Else
                result = Me.Xenvelope.RunXChange(Me.ObjectMessageLog)
            End If

            Return result
        End Function


        ''' <summary>
        ''' sets or gets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContextIdentifier() As String Implements ormLoggable.ContextIdentifier
            Get
                Return _ContextIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNContextID, value)
            End Set
        End Property


        ''' <summary>
        ''' sets or gets the tuple identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TupleIdentifier() As String Implements ormLoggable.TupleIdentifier
            Get
                Return _TupleIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNTupleID, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the entitiy identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EntityIdentifier As String Implements ormLoggable.EntityIdentifier
            Get
                Return _EntitityIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNEntityID, value)
            End Set
        End Property

        ''' <summary>
        ''' Handler for the Envelope Message Added Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub MQMessage_OnEnvelopeObjectMessageAdded(sender As Object, e As OnTrack.ObjectMessageLog.EventArgs)
            Me.ObjectMessageLog.CopyFrom(e.Message)
        End Sub


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
        Implements ormLoggable

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

        <ormObjectEntry(referenceObjectEntry:=XChange.XChangeObjectEntry.ConstObjectID & "." & XChange.XChangeObjectEntry.constFNordinal, _
            dbdefaultvalue:="0", defaultvalue:=0, isnullable:=False, primarykeyordinal:=3, _
           Title:="Identity Number", description:="reference ID (Ordinal No in the XChangeConfiguration)")> _
        Public Const ConstFNSlotID = "SLOTID"

        ''' <summary>
        ''' Column entry
        ''' </summary>
        ''' <remarks></remarks>
        ''' 

        <ormObjectEntry(typeid:=otDataType.Long, defaultvalue:=otDataType.Date, dbdefaultvalue:="6", isnullable:=True, _
                    title:="datatype", Description:="datatype of the message slot value")> Public Const ConstFNDatatype = "DATATYPE"

        <ormObjectEntry(typeid:=otDataType.Text, defaultvalue:="", isnullable:=True, _
                    title:="value", Description:="text presentation of the slot value")> Public Const ConstFNvalue = "VALUE"

        <ormObjectEntry(typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
                     title:="IsEmpty", description:="is slot empty (none)")> Public Const ConstFNIsEmpty = "ISEMPTY"

        <ormObjectEntry(typeid:=otDataType.Bool, dbdefaultvalue:="0", defaultvalue:=False, _
                     title:="IsNull", description:="is slot null (initializing value)")> Public Const ConstFNIsNull = "ISNULL"

        <ormObjectEntry(typeid:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                     title:="Processed", description:="is message processed")> Public Const ConstFNProcessed = "PROCESSED"

        <ormObjectEntry(typeid:=otDataType.Bool, isnullable:=True, _
                     title:="Processable", description:="is message processable")> Public Const ConstFNProcessable = "PROCESSABLE"

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStamp _
          )> Public Const ConstFNProcStamp = MessageQueue.ConstFNProcStamp

        <ormObjectEntry(referenceobjectentry:=MessageQueue.ConstObjectID & "." & MessageQueue.ConstFNProcStatus _
          )> Public Const ConstFNProcStatus = MessageQueue.ConstFNProcStatus

        <ormObjectEntry(ReferenceObjectEntry:=Commons.Domain.ConstObjectID & "." & Commons.Domain.ConstFNDomainID, isnullable:=True, _
          useforeignkey:=otForeignKeyImplementation.None, _
          Title:="DomainID", description:="ID of the domain for this slot")> Public Const ConstFNDomainID = Commons.Domain.ConstFNDomainID

        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNContextID)> Public Const ConstFNContextID = ObjectMessage.ConstFNContextID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNTupleID)> Public Const ConstFNTupleID = ObjectMessage.ConstFNTupleID
        <ormObjectEntry(ReferenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNEntityID)> Public Const ConstFNEntityID = ObjectMessage.ConstFNEntityID

        ''' <summary>
        ''' Mapping
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(entryname:=ConstFNMQID)> Private _mqid As String = ""
        <ormEntryMapping(entryname:=ConstFNIDNO)> Private _messageidno As Long
        <ormEntryMapping(entryname:=ConstFNSlotID)> Private _slotid As String = ""

        <ormEntryMapping(entryname:=ConstFNDatatype)> Private _datatype As otDataType?
        <ormEntryMapping(entryname:=ConstFNvalue)> Private _valuestring As String
        <ormEntryMapping(entryname:=ConstFNIsNull)> Private _isnull As Boolean
        <ormEntryMapping(entryname:=ConstFNIsEmpty)> Private _isempty As Boolean

        <ormEntryMapping(entryname:=ConstFNProcStatus)> Private _procStatus As String
        <ormEntryMapping(entryname:=ConstFNProcStamp)> Private _ProcTimestamp As Date?
        <ormEntryMapping(entryname:=ConstFNProcessed)> Private _IsProcessed As Boolean
        <ormEntryMapping(entryname:=ConstFNProcessable)> Private _IsProcessable As Boolean

        <ormEntryMapping(entryname:=ConstFNContextID)> Private _ContextIdentifier As String
        <ormEntryMapping(entryname:=ConstFNTupleID)> Private _TupleIdentifier As String
        <ormEntryMapping(entryname:=ConstFNEntityID)> Private _EntitityIdentifier As String

        '** dynmaic
        Private _message As MQMessage 'backlink
        Private _messagequeue As MessageQueue 'backlink
        Private _ordinal As Ordinal 'cache
        Private _data As Object
        Private _xconfigentry As IList(Of IXChangeConfigEntry) 'cache

#Region "Properties"


        ''' <summary>
        ''' Gets or sets the isempty.
        ''' </summary>
        ''' <value>The isempty.</value>
        Public Property IsEmpty() As Boolean
            Get
                Return Me._isempty
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsEmpty, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the isnull.
        ''' </summary>
        ''' <value>The isnull.</value>
        Public Property IsNull() As Boolean
            Get
                Return Me._isnull
            End Get
            Set(value As Boolean)
                SetValue(ConstFNIsNull, value)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContextIdentifier() As String Implements ormLoggable.ContextIdentifier
            Get
                Return _ContextIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNContextID, value)
            End Set
        End Property


        ''' <summary>
        ''' sets or gets the tuple identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TupleIdentifier() As String Implements ormLoggable.TupleIdentifier
            Get
                Return _TupleIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNTupleID, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the entitiy identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EntityIdentifier As String Implements ormLoggable.EntityIdentifier
            Get
                Return _EntitityIdentifier
            End Get
            Set(value As String)
                SetValue(ConstFNEntityID, value)
            End Set
        End Property
        ''' <summary>
        ''' returns the Message of the Message Queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property XChangeConfiguration As XChangeConfiguration
            Get
                If Me.MessageQueue IsNot Nothing Then
                    Return Me.MessageQueue.XChangeConfig
                End If
            End Get
        End Property
        ''' <summary>
        ''' returns the Message of the Message Queue
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property XChangeConfigEntries As IList(Of IXChangeConfigEntry)
            Get
                If _xconfigentry Is Nothing Then
                    If Me.MessageQueue IsNot Nothing Then
                        If Me.MessageQueue.XChangeConfig IsNot Nothing Then _xconfigentry = Me.MessageQueue.XChangeConfig.GetEntriesByMappingOrdinal(New Ordinal(Me.ID))
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
        ReadOnly Property ID() As String
            Get
                Return _slotid
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

        Public ReadOnly Property [Ordinal] As Ordinal
            Get
                If _ordinal Is Nothing Then
                    _ordinal = New Ordinal(Me.ID)
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
                SetValue(ConstFNProcStamp, value)
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
            Dim anMQID = e.Record.GetValue(ConstFNMQID)
            Dim aMessagequeue = MessageQueue.Retrieve(anMQID)
            Try
                If aMessagequeue IsNot Nothing Then
                    If aMessagequeue.XChangeConfig IsNot Nothing Then
                        Dim anid As String = e.Record.GetValue(ConstFNSlotID)
                        Dim aXConfigEntry As IXChangeConfigEntry
                        If anid IsNot Nothing Then
                            Dim aList = aMessagequeue.XChangeConfig.GetEntriesByMappingOrdinal(New Ordinal(anid))
                            If aList.Count > 0 Then aXConfigEntry = aList.First
                        End If
                        If aXConfigEntry IsNot Nothing Then e.Record.SetValue(ConstFNDatatype, aXConfigEntry.ObjectEntryDefinition.Datatype)

                    End If
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="MQXSlot_OnDefaultValuesNeeded")
            End Try
           
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
        Public Shared Function Create(ByVal mqid As String, ByVal messageidno As Long, ByVal slotid As Ordinal) As MQXSlot
            Dim pkarry() As Object = {mqid.ToUpper, messageidno, slotid.Value.ToString}
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
        Public Shared Function Retrieve(ByVal mqid As String, ByVal messageidno As Long, ByVal slotid As Ordinal) As MQXSlot
            Dim pkarry() As Object = {mqid.ToUpper, messageidno, slotid.Value.ToString}

            Return ormDataObject.Retrieve(Of MQXSlot)(pkArray:=pkarry)
        End Function
    End Class
End Namespace
