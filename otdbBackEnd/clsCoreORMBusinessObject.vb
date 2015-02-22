
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM Business Object Class - heavy weight relational business object
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-31
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>
Option Explicit On
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Reflection
Imports OnTrack.Commons

Namespace OnTrack.Database

    ''' <summary>
    ''' abstract base class for all business objects based on relational persistence
    ''' handles the data operations with an embedded record
    ''' raises all data events
    ''' </summary>
    ''' <remarks>
    ''' functional Design principles
    ''' 1. derived from infusable
    ''' 2. own features : SpareField Flag
    ''' 3. rights on operations - who is allowed
    ''' 4. persists relational in multiple tables
    ''' 5. tracks a message log
    ''' 6. allows validation of entry members
    ''' 7. allows cloneing
    ''' 8. implements CRUD operations such as Create, Retrieve, Update, Delete
    ''' </remarks>
    Partial Public MustInherit Class ormBusinessObject
        Inherits ormRelationalInfusable
        Implements System.ComponentModel.INotifyPropertyChanged
        Implements iormRelationalPersistable
        Implements iormCloneable
        Implements iormValidatable
        Implements iormQueriable
        Implements iormLoggable
        Implements IDisposable

        ''' <summary>
        ''' important objects to drive data object behavior
        ''' </summary>
        ''' <remarks></remarks>

        Private WithEvents _validator As ObjectValidator          ' valitator to validate

        ''' <summary>
        ''' tables for storing the record in 
        ''' </summary>
        ''' <remarks></remarks>
        Private _primaryTableID As String
        Private _tableids As String() = {}
        Private _tableisloaded As Boolean() 'status of the loaded tables

        ''' <summary>
        ''' cached links and objects
        ''' </summary>
        ''' <remarks></remarks>
       
        Private WithEvents _defaultdbdriver As iormRelationalDatabaseDriver

        ''' <summary>
        ''' members to check interaction to COM Host Application
        ''' </summary>
        ''' <remarks></remarks>

        Private _persistInHostApplication As Boolean = False 'true if the data object will be persisted and retrieved from the COM Host Application
        Private _IsloadedFromHost As Boolean = False
        Private _IsSavedToHost As Boolean = False

        ''' <summary>
        ''' identifier for ormLoggable
        ''' </summary>
        ''' <remarks></remarks>
        Protected _contextidentifier As String
        Protected _tupleidentifier As String
        Protected _entityidentifier As String

        ''' <summary>
        ''' Spare member entries
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, posordinal:=1101, _
        title:="text parameter 1", description:="text parameter 1")> Public Const ConstFNParamText1 = "param_txt1"
        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, posordinal:=1102, _
        title:="text parameter 2", description:="text parameter 2")> Public Const ConstFNParamText2 = "param_txt2"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, spareFieldTag:=True, posordinal:=1103, _
        title:="text parameter 3", description:="text parameter 3")> Public Const ConstFNParamText3 = "param_txt3"
        <ormObjectEntry(Datatype:=otDataType.Numeric, isnullable:=True, spareFieldTag:=True, posordinal:=1201, _
        title:="numeric parameter 1", description:="numeric parameter 1")> Public Const ConstFNParamNum1 = "param_num1"
        <ormObjectEntry(Datatype:=otDataType.Numeric, isnullable:=True, spareFieldTag:=True, posordinal:=1202, _
        title:="numeric parameter 2", description:="numeric parameter 2")> Public Const ConstFNParamNum2 = "param_num2"
        <ormObjectEntry(Datatype:=otDataType.Numeric, isnullable:=True, spareFieldTag:=True, posordinal:=1203, _
        title:="numeric parameter 3", description:="numeric parameter 3")> Public Const ConstFNParamNum3 = "param_num3"
        <ormObjectEntry(Datatype:=otDataType.Date, isnullable:=True, spareFieldTag:=True, posordinal:=1301, _
        title:="date parameter 1", description:="date parameter 1")> Public Const ConstFNParamDate1 = "param_date1"
        <ormObjectEntry(Datatype:=otDataType.Date, isnullable:=True, spareFieldTag:=True, posordinal:=1302, _
        title:="date parameter 2", description:="date parameter 2")> Public Const ConstFNParamDate2 = "param_date2"
        <ormObjectEntry(Datatype:=otDataType.Date, isnullable:=True, spareFieldTag:=True, posordinal:=1303, _
        title:="date parameter 3", description:="date parameter 3")> Public Const ConstFNParamDate3 = "param_date3"
        <ormObjectEntry(Datatype:=otDataType.Bool, isnullable:=True, spareFieldTag:=True, posordinal:=1401, _
        title:="flag parameter 1", description:="flag parameter 1")> Public Const ConstFNParamFlag1 = "param_flag1"
        <ormObjectEntry(Datatype:=otDataType.Bool, isnullable:=True, spareFieldTag:=True, posordinal:=1402, _
        title:="flag parameter 2", description:="flag parameter 2")> Public Const ConstFNParamFlag2 = "param_flag2"
        <ormObjectEntry(Datatype:=otDataType.Bool, isnullable:=True, spareFieldTag:=True, posordinal:=1403, _
        title:="flag parameter 3", description:="flag parameter 3")> Public Const ConstFNParamFlag3 = "param_flag3"

        ''' <summary>
        ''' MSG LOG TAG
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=ObjectMessage.ConstObjectID & "." & ObjectMessage.ConstFNTag, isnullable:=True)> _
        Public Const ConstFNMSGLOGTAG = ObjectMessage.ConstFNTag



        ''' <summary>
        ''' ColumnMapping
        ''' </summary>
        ''' <remarks></remarks>
        '** Spare Parameters
        <ormObjectEntryMapping(EntryName:=ConstFNParamText1)> Protected _parameter_txt1 As String
        <ormObjectEntryMapping(EntryName:=ConstFNParamText2)> Protected _parameter_txt2 As String
        <ormObjectEntryMapping(EntryName:=ConstFNParamText3)> Protected _parameter_txt3 As String
        <ormObjectEntryMapping(EntryName:=ConstFNParamNum1)> Protected _parameter_num1 As Nullable(Of Double)
        <ormObjectEntryMapping(EntryName:=ConstFNParamNum2)> Protected _parameter_num2 As Nullable(Of Double)
        <ormObjectEntryMapping(EntryName:=ConstFNParamNum3)> Protected _parameter_num3 As Nullable(Of Double)
        <ormObjectEntryMapping(EntryName:=ConstFNParamDate1)> Protected _parameter_date1 As Nullable(Of Date)
        <ormObjectEntryMapping(EntryName:=ConstFNParamDate2)> Protected _parameter_date2 As Nullable(Of Date)
        <ormObjectEntryMapping(EntryName:=ConstFNParamDate3)> Protected _parameter_date3 As Nullable(Of Date)
        <ormObjectEntryMapping(EntryName:=ConstFNParamFlag1)> Protected _parameter_flag1 As Nullable(Of Boolean)
        <ormObjectEntryMapping(EntryName:=ConstFNParamFlag2)> Protected _parameter_flag2 As Nullable(Of Boolean)
        <ormObjectEntryMapping(EntryName:=ConstFNParamFlag3)> Protected _parameter_flag3 As Nullable(Of Boolean)

        ''' <summary>
        ''' message log tag for the business object
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntryMapping(EntryName:=ConstFNMSGLOGTAG)> Protected _msglogtag As String

        '''
        ''' Transactions DEFAULTS
        ''' 
        <ormObjectTransactionAttribute(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                        Description:="create an instance of persist able data object")> Public Const ConstOPCreate = "Create"
        <ormObjectTransactionAttribute(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                       Description:="retrieve a data object")> Public Const ConstOPRetrieve = "Retrieve"
        <ormObjectTransactionAttribute(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadUpdate & ", true, true)"}, _
                       Description:="delete a data object")> Public Const ConstOPDelete = "Delete"
        <ormObjectTransactionAttribute(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                       Description:="inject a data object")> Public Const ConstOPInject = "Inject"
        <ormObjectTransactionAttribute(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadUpdate & ", true, true)"}, _
                       Description:="perist a data object")> Public Const ConstOPPersist = "Persist"


        ''' Queries
        ''' 
        <ormObjectQuery(Description:="All Objects", where:="")> Public Const ConstQRYAll = "All"


        ''' <summary>
        ''' Operation Constants
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstOPGetObjectMessages = "GetObjectMessages"

        ''' <summary>
        ''' Relation to Message Log
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ObjectMessage), retrieveOperation:=ConstOPGetObjectMessages, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRMessageLog = "RelObjectMessage"

        <ormObjectEntryMapping(relationName:=ConstRMessageLog, infusemode:=otInfuseMode.OnDemand)> Protected WithEvents _ObjectMessageLog As ObjectMessageLog '  MessageLog
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub New(Optional runtimeonly As Boolean = False, Optional objectID As String = Nothing)
            MyBase.New(runtimeonly:=runtimeonly, objectID:=objectID)

        End Sub
        ''' <summary>
        ''' clean up with the object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Finialize()
            MyBase.Finalize()
            _primaryTableID = String.Empty
            _defaultdbdriver = Nothing
            _ObjectMessageLog = Nothing
            _relationMgr = Nothing
        End Sub


        ''' <summary>
        ''' operation to load the object messages into the local container
        ''' </summary>
        ''' <param name="id">the property</param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(operationname:=ConstOPGetObjectMessages)> Public Function LoadObjectMessages() As iormRelationalCollection(Of ObjectMessage)
            If Not IsAlive(subname:="LoadObjectMessages") Then Return New ormRelationCollection(Of ObjectMessage)(Nothing, keyentrynames:={ObjectMessage.ConstFNNo})

            ''' assign the messagelog
            If _ObjectMessageLog Is Nothing Then _ObjectMessageLog = New ObjectMessageLog(Me)

            ''' load the existing log and merge it into the current one
            ''' 
            If Not Me.RunTimeOnly Then
                Dim aRetrieveLog As ObjectMessageLog = ObjectMessageLog.Retrieve(Me.ObjectTag)
                For Each aMessage In aRetrieveLog
                    If _ObjectMessageLog.ContainsKey(key:=aMessage.No) Then
                        aMessage.No = _ObjectMessageLog.Max(Function(x) x.No) + 1
                    End If
                    _ObjectMessageLog.Add(aMessage)
                Next
            End If

            Return _ObjectMessageLog
        End Function

        ''' <summary>
        ''' set one or all tables to unloaded
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function SetUnloaded(Optional tableid As String = Nothing) As Boolean
            If _tableisloaded.Count = 0 Then
                Return False
            ElseIf tableid Is Nothing AndAlso Me.IsLoaded Then
                For i As UShort = _tableisloaded.GetLowerBound(0) To _tableisloaded.GetUpperBound(0)
                    _tableisloaded(i) = False
                Next
            ElseIf tableid IsNot Nothing Then
                For i As UShort = _tableisloaded.GetLowerBound(0) To _tableisloaded.GetUpperBound(0)
                    If Me.TableIDs(i) = tableid Then _tableisloaded(i) = False
                Next
            End If
            Return True
        End Function
        ''' <summary>
        ''' set one or all tables to loaded
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Sub Setloaded(Optional tableid As String = Nothing)
            If _tableisloaded.Count = 0 Then
                Return
            ElseIf tableid Is Nothing Then
                For i As UShort = _tableisloaded.GetLowerBound(0) To _tableisloaded.GetUpperBound(0)
                    _tableisloaded(i) = True
                Next
                MyBase._isloaded = True
            ElseIf tableid IsNot Nothing Then
                For i As UShort = _tableisloaded.GetLowerBound(0) To _tableisloaded.GetUpperBound(0)
                    If Me.TableIDs(i) = tableid Then _tableisloaded(i) = True
                Next
            End If
        End Sub

        ''' <summary>
        ''' sets the Livecycle status of this object if created or loaded
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function DetermineLiveStatus() As Boolean Implements iormDataObject.DetermineLiveStatus
            ''' check the record again -> if infused by a record by sql selectment if have nor created not loaded
            If Me.IsInitialized Then
                '** check on the records
                _isCreated = Me.Record.IsCreated
                If Me.Record.IsLoaded Then
                    For Each atableid In Me.Record.TableIDs
                        Me.Setloaded(atableid)
                    Next
                End If
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' <summary>
        ''' injects a new instance a dataobject and infuses it
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function InjectDataObject(ByRef primarykey As ormDatabaseKey, type As System.Type, _
                                                                     Optional domainid As String = Nothing, _
                                                                     Optional dbdriver As iormRelationalDatabaseDriver = Nothing) As iormRelationalPersistable
            Dim aDataObject As iormRelationalPersistable = ot.CreateDataObjectInstance(type)

            If aDataObject.Inject(primarykey, domainid:=domainid, dbdriver:=dbdriver) Then
                Return aDataObject
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' injects a new  iormpersistable DataObject by Type
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function InjectDataObject(Of T As {iormInfusable, iormPersistable, New})(ByRef primarykey As ormDatabaseKey, _
                                                                                               Optional domainid As String = Nothing, _
                                                                                                Optional dbdriver As iormDatabaseDriver = Nothing) As T
            Return InjectDataObject(primarykey:=primarykey, type:=GetType(T), domainid:=domainid, dbdriver:=dbdriver)
        End Function

        ''' <summary>
        ''' injects retrieving records from the datastores and infuses the object from the inside out
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Inject(ByRef primarykey As ormDatabaseKey, _
                                           Optional domainid As String = Nothing, _
                                           Optional dbdriver As iormDatabaseDriver = Nothing, _
                                           Optional loadDeleted As Boolean = False) As Boolean Implements iormPersistable.Inject
            Dim aRecord As ormRecord
            Dim aStore As iormContainerStore
            Dim ourEventArgs As ormDataObjectEventArgs
            Dim anewDataobject As iormRelationalPersistable = Me

            '* init
            If Not Me.IsInitialized AndAlso Not Me.Initialize Then
                Return False
            End If

            '** check on the operation right for this object
            If Not RunTimeOnly AndAlso Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainid, _
                                                                objecttransactions:={Me.ObjectID & "." & ConstOPInject}) Then
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, _
                                                        domainid:=domainid, _
                                                        username:=CurrentSession.CurrentUsername, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                        objecttransactions:={Me.ObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be injected - permission denied to user", _
                                            objectname:=Me.ObjectID, argument:=ConstOPInject, username:=CurrentSession.CurrentUsername, _
                                            procedure:="ormBusinessObject.Inject", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            Try
                _RunTimeOnly = False

                ''' fix the primary key
                ''' 
                primarykey.ChecknFix(domainid:=domainid, runtimeOnly:=RunTimeOnly)

                '** fire event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=aRecord, key:=primarykey, infusemode:=otInfuseMode.OnInject, runtimeOnly:=Me.RunTimeOnly)
                ourEventArgs.UseCache = Me.UseCache
                RaiseEvent OnInjecting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Me.Record = ourEventArgs.Record
                    End If
                    '** reset the infuse mode
                    Return ourEventArgs.Result
                ElseIf ourEventArgs.Result Then
                    primarykey = ourEventArgs.Key
                    aRecord = ourEventArgs.Record
                End If

                ''' How to Inject
                '''
                Dim UseView As Boolean = False
                Dim retrieveViewID As String
                If CurrentSession.IsRepositoryAvailable Then
                    UseView = Not String.IsNullOrWhiteSpace(Me.ObjectDefinition.RetrieveObjectFromViewID)
                    If UseView Then retrieveViewID = Me.ObjectDefinition.RetrieveObjectFromViewID
                Else
                    UseView = Me.ObjectClassDescription.ObjectAttribute.HasValueRetrieveObjectFromViewID
                    If UseView Then retrieveViewID = Me.ObjectClassDescription.ObjectAttribute.RetrieveObjectFromViewID
                End If

                ''' check how many tables to inject from -> get the record
                ''' 
                If Me.TableIDs.Count = 1 AndAlso Not UseView Then
                    If dbdriver Is Nothing Then dbdriver = Me.DatabaseDriver
                    aStore = CType(dbdriver, iormRelationalDatabaseDriver).GetTableStore(Me.ObjectPrimaryTableID)

                    ''' the primary table is always loaded with the pkarray
                    aRecord = aStore.GetRecordByPrimaryKey(primarykey.Values)

                ElseIf Me.TableIDs.Count > 1 Then

                    ''' check if injecting from a view
                    If UseView Then
                        aStore = CType(dbdriver, iormRelationalDatabaseDriver).GetViewReader(Me.ObjectDefinition.RetrieveObjectFromViewID)
                        aRecord = aStore.GetRecordByPrimaryKey(primarykey.Values)

                    Else
                        ''' not implemented -> load from multiple tables
                        ''' 
                        Throw New NotImplementedException("not implemented to load from multiple tables")

                    End If
                End If

                '* still nothing ?!

                If aRecord Is Nothing Then
                    Me.SetUnloaded()
                    Return False
                Else
                    '* what about deleted objects
                    If Me.ObjectHasDeletePerFlagBehavior Then
                        If aRecord.HasIndex(ConstFNIsDeleted) Then
                            If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                _IsDeleted = True
                                '* load only on deleted
                                If Not loadDeleted Then
                                    Me.SetUnloaded()
                                    _isCreated = False
                                    Return False
                                End If
                            Else
                                _IsDeleted = False
                            End If
                        Else
                            CoreMessageHandler(message:="object has delete per flag behavior but no flag", messagetype:=otCoreMessageType.InternalError, _
                                                procedure:="ormBusinessObject.Inject", containerID:=Me.ObjectPrimaryTableID, entryname:=ConstFNIsDeleted)
                            _IsDeleted = False
                        End If
                    Else
                        _IsDeleted = False
                    End If

                    ''' INFUSE THE OBJECT (partially) from the record
                    ''' 

                    If InfuseDataObject(record:=aRecord, dataobject:=anewDataobject, mode:=otInfuseMode.OnInject) Then
                        If Me.Guid <> anewDataobject.GUID Then
                            CoreMessageHandler(message:="object was substituted during infuse", messagetype:=otCoreMessageType.InternalError, _
                                                procedure:="ormBusinessObject.Inject", containerID:=Me.ObjectPrimaryTableID, objectname:=Me.ObjectID)
                            Return False
                        End If

                        '** set all tables to be loaded
                        ''' Array.ForEach(Of Boolean)(_tableisloaded, Function(x) x = True) -> in .infuse method
                        '** set the primary keys
                        ''' _primarykey = primarykey -> in .infuse method
                    Else
                        CoreMessageHandler(message:="unable to inject a new data object from record", messagetype:=otCoreMessageType.InternalError, _
                                            procedure:="ormBusinessObject.Inject", containerID:=Me.ObjectPrimaryTableID, objectname:=Me.ObjectID)
                        Return False
                    End If


                End If


                '** fire event
                ourEventArgs = New ormDataObjectEventArgs(anewDataobject, record:=Me.Record, key:=primarykey, infuseMode:=otInfuseMode.OnInject, runtimeOnly:=Me.RunTimeOnly)
                ourEventArgs.Proceed = Me.IsLoaded
                ourEventArgs.UseCache = Me.UseCache
                RaiseEvent OnInjected(Me, ourEventArgs)

                If ourEventArgs.Proceed Then
                    _isCreated = False
                    _IsChanged = False
                End If
                '** return
                Return Me.IsLoaded
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, procedure:="ormBusinessObject.Inject", argument:=primarykey, containerID:=_primaryTableID)
                Return False
            End Try


        End Function

        ''' <summary>
        ''' Persist the object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overridable Function Persist(Optional timestamp As DateTime? = Nothing, Optional doFeedRecord As Boolean = True) As Boolean Implements iormRelationalPersistable.Persist

            '* init
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then Return False
            '** must be alive from data store
            If Not IsAlive(subname:="Persist") Then Return False
            If Not timestamp.HasValue OrElse timestamp = constNullDate Then timestamp = DateTime.Now

            '''
            ''' object on runtime -> no save
            ''' 
            If Me.RunTimeOnly Then
                CoreMessageHandler(message:="object on runtime could not be persisted", messagetype:=otCoreMessageType.InternalWarning, _
                                 procedure:="ormBusinessObject.Persist", dataobject:=Me)
                Return False
            End If

            '''
            ''' record must be alive
            ''' 
            If Not Me.Record.Alive Then
                CoreMessageHandler(message:="record is not alive in data object - cannot persist", messagetype:=otCoreMessageType.InternalError, _
                                   procedure:="ormBusinessObject.Persist", objectname:=Me.ObjectID, containerID:=Me.ObjectPrimaryTableID)
                Return False
            End If
            '** check on the operation right for this object
            If Not CurrentSession.IsStartingUp AndAlso _
                Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, objecttransactions:={Me.ObjectID & "." & ConstOPPersist}) Then
                '** authorize
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, _
                                                    messagetext:="Please provide another user to authorize requested operation", _
                                                    username:=CurrentSession.CurrentUsername, loginOnFailed:=True, _
                                                    objecttransactions:={Me.ObjectID & "." & ConstOPPersist}) Then
                    Call CoreMessageHandler(message:="data object cannot be persisted - permission denied to user", _
                                            objectname:=Me.ObjectID, argument:=ConstOPPersist, username:=CurrentSession.CurrentUsername, _
                                            procedure:="ormBusinessObject.Persist", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If
            '**
            Try
                '* if object was deleted an its now repersisted
                Dim isdeleted As Boolean = _IsDeleted
                _IsDeleted = False

                '** fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, key:=Me.ObjectPrimaryKey, _
                                                               timestamp:=timestamp, usecache:=Me.UseCache, domainID:=DomainID, _
                                                               domainBehavior:=Me.ObjectHasDomainBehavior, runtimeOnly:=Me.RunTimeOnly)
                RaiseEvent ClassOnPersisting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return False
                Else
                    _record = ourEventArgs.Record
                End If

                '** fire event
                RaiseEvent OnPersisting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return False
                Else
                    _record = ourEventArgs.Record
                End If

                '''
                ''' Validate the object 
                ''' 
                If Me.Validate(Me.ObjectMessageLog) = otValidationResultType.FailedNoProceed Then
                    ''' Failed ?!
                    ''' 
                    CoreMessageHandler(message:="persist operation rejected due to failing validation", messagetype:=otCoreMessageType.ApplicationWarning, _
                                        procedure:="ormBusinessObject.Persist", argument:=Converter.Array2StringList(Me.ObjectPrimaryKeyValues), objectname:=Me.ObjectID, _
                                        msglog:=Me.ObjectMessageLog)

                    ''' return
                    Return False
                End If


                '** feed record
                If doFeedRecord Then Feed()

                '''
                ''' persist the data object through the record
                ''' 
                If Not Me.Record.Persist(timestamp) Then
                    CoreMessageHandler("data object could not persist", dataobject:=Me, procedure:="ormBusinessObject.Persist", messagetype:=otCoreMessageType.InternalError)
                    Persist = False
                Else
                    ''' set it loaded
                    For Each aTableID In Me.Record.TableIDs
                        Me.Setloaded(aTableID)
                    Next
                    '''
                    ''' cascade the operation through the related members
                    ''' 
                    If Not Me.CascadeRelations(cascadeUpdate:=True, timestamp:=timestamp, uniquenesswaschecked:=_UniquenessInStoreWasChecked) Then
                        Persist = False
                    Else
                        Persist = True
                    End If
                End If
                ''' persist the object messages
                ''' 
                If _ObjectMessageLog IsNot Nothing AndAlso _ObjectMessageLog.Count > 0 Then
                    For Each aMessage In _ObjectMessageLog
                        If Not aMessage.RunTimeOnly AndAlso aMessage.IsPersisted Then
                            aMessage.Persist(timestamp:=timestamp)
                        End If
                    Next
                End If

                '** set flags -> we are persisted anyway even if the events might demand to abort
                '''
                If Persist Then
                    _isCreated = False
                    _IsChanged = False
                    'Me.Setloaded() -> above
                    _IsDeleted = False
                Else
                    _IsDeleted = isdeleted
                End If


                '** fire event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, key:=Me.ObjectPrimaryKey, _
                                                               timestamp:=timestamp, usecache:=Me.UseCache, domainID:=DomainID, _
                                                               domainBehavior:=Me.ObjectHasDomainBehavior, runtimeOnly:=Me.RunTimeOnly)
                RaiseEvent OnPersisted(Me, ourEventArgs)
                Persist = ourEventArgs.Proceed And Persist

                RaiseEvent ClassOnPersisted(Me, ourEventArgs)
                Persist = ourEventArgs.Proceed And Persist

                Return Persist

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, procedure:="ormBusinessObject.Persist")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' shared create the schema for this object by reflection
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObjectSchema(Of T)(Optional silent As Boolean = False, Optional dbdriver As iormRelationalDatabaseDriver = Nothing) As Boolean
            '** check on Bootstrapping conditions
            Dim aClassDescription = ot.GetObjectClassDescription(GetType(T))
            If dbdriver Is Nothing Then dbdriver = CurrentDBDriver
            If aClassDescription.ObjectAttribute.IsBootstrap And Not CurrentSession.IsBootstrappingInstallationRequested Then
                dbdriver.VerifyOnTrackDatabase() 'check if a bootstrap needs to be issued
            End If
            Dim anObjectDefinition = ot.CurrentSession.Objects.GetObject(aClassDescription.ObjectAttribute.ID)
            If anObjectDefinition IsNot Nothing Then
                Return anObjectDefinition.CreateObjectSchema(silent:=silent)
            End If
            Return False

        End Function

        ''' <summary>
        ''' create a dataobject from a type
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <param name="type"></param>
        ''' <param name="domainID"></param>
        ''' <param name="checkUnique"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ormObjectOperationMethod(Description:="Creates a Data Object by primary keys from store)", _
            OperationName:="GeneralCreateByPrimaryKeys", TransactionID:=ConstOPCreate, tag:=ObjectClassDescription.ConstMTCreateDataObject)> _
        Public Shared Function CreateDataObject(primarykey As ormDatabaseKey, type As System.Type, _
                                 Optional domainID As String = Nothing,
                                 Optional checkUnique As Boolean? = Nothing, _
                                 Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable

            Dim aDataobject As iormRelationalPersistable = ot.CreateDataObjectInstance(type)
            ''' defautl values
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            ''' Substitute the DomainID if necessary
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID

            ''' fix the primary key
            ''' 
            primarykey.ChecknFix(domainid:=domainID, runtimeOnly:=runtimeOnly)

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=TryCast(aDataobject, ormBusinessObject), _
                                                           record:=aDataobject.Record, _
                                                          key:=primarykey, _
                                                           usecache:=aDataobject.useCache, _
                                                           runtimeonly:=runtimeOnly)
            RaiseEvent ClassOnCreating(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    Return ourEventArgs.DataObject
                Else
                    Return Nothing
                End If
            ElseIf ourEventArgs.Result Then
                primarykey = ourEventArgs.Key
            End If

            If aDataobject.Create(primarykey, domainID:=domainID, runTimeonly:=runtimeOnly, checkUnique:=checkUnique) Then
                '** fire event
                ourEventArgs = New ormDataObjectEventArgs([object]:=TryCast(aDataobject, ormBusinessObject), _
                                                               record:=aDataobject.Record, _
                                                              key:=primarykey, _
                                                               usecache:=aDataobject.useCache, _
                                                               runtimeonly:=runtimeOnly)
                RaiseEvent ClassOnCreated(Nothing, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Return ourEventArgs.DataObject
                    Else
                        Return Nothing
                    End If
                End If
                Return aDataobject
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' Create a Dataobject of Type T with a primary key with values in order of the array
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <param name="domainID"></param>
        ''' <param name="checkUnique"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObject(Of T As {iormPersistable, New}) _
                           (ByRef pkArray() As Object,
                            Optional domainID As String = Nothing,
                            Optional checkUnique As Boolean? = Nothing, _
                            Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable
            Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescription(typename:=GetType(T).FullName)
            Dim aPrimaryKey As New ormDatabaseKey(objectid:=aDescription.ID, keyvalues:=pkArray)

            Return CreateDataObject(primarykey:=aPrimaryKey, type:=GetType(T), domainID:=domainID, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' create a persistable dataobject of type T 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <param name="checkUnique"></param>
        ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
        ''' <remarks></remarks>

        Public Shared Function CreateDataObject(Of T As {iormInfusable, iormPersistable, New}) _
                            (ByRef primarykey As ormDatabaseKey,
                             Optional domainID As String = Nothing,
                             Optional checkUnique As Boolean? = Nothing, _
                             Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable
            Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescription(typename:=GetType(T).Name)
            Return CreateDataObject(primarykey:=primarykey, type:=GetType(T), domainID:=domainID, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' create a persistable dataobject of type T out of data of a record
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <param name="checkUnique"></param>
        ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObject(Of T As {iormPersistable, New}) _
                            (ByRef record As ormRecord,
                             Optional domainID As String = Nothing,
                             Optional checkUnique As Boolean? = Nothing, _
                             Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable
            Dim aDataObject As New T
            ''' defautl values
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            ''' Get the Primary key
            Dim aPrimaryKey As ormDatabaseKey = ExtractObjectPrimaryKey(record:=record, objectID:=aDataObject.ObjectID, runtimeOnly:=runtimeOnly)
            ''' Substitute the DomainID if necessary
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID

            ''' fix primary key
            ''' 
            aPrimaryKey.ChecknFix(domainid:=domainID, runtimeOnly:=runtimeOnly)

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormBusinessObject), _
                                                           record:=record, _
                                                          key:=aPrimaryKey, _
                                                           usecache:=aDataObject.useCache, _
                                                           runtimeonly:=runtimeOnly)
            RaiseEvent ClassOnCreating(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    Return ourEventArgs.DataObject
                Else
                    Return Nothing
                End If
            Else
                record = ourEventArgs.Record
            End If

            If aDataObject.Create(record, domainID:=domainID, runtimeOnly:=runtimeOnly, checkUnique:=checkUnique) Then
                '** fire event
                ourEventArgs = New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormBusinessObject), _
                                                               record:=record, _
                                                               key:=ExtractObjectPrimaryKey(record:=record, objectID:=aDataObject.ObjectID, runtimeOnly:=runtimeOnly), _
                                                               usecache:=aDataObject.useCache, _
                                                               runtimeonly:=runtimeOnly)
                RaiseEvent ClassOnCreated(Nothing, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Return ourEventArgs.DataObject
                    Else
                        Return Nothing
                    End If
                End If
            Else
                Return Nothing
            End If

            Return aDataObject
        End Function
        ''' <summary>
        ''' helper for checking the uniqueness during creation
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function CheckUniqueness(primarykey As ormDatabaseKey, record As ormRecord, Optional runtimeOnly As Boolean = False) As Boolean

            '*** Check on Not Runtime
            If Not runtimeOnly OrElse Me.UseCache Then
                Dim aRecord As ormRecord
                '* fire Event and check uniqueness in cache if we have one
                Dim ourEventArgs = New ormDataObjectEventArgs(Me, record:=record, key:=primarykey, usecache:=Me.UseCache, runtimeOnly:=runtimeOnly)
                RaiseEvent ClassOnCheckingUniqueness(Me, ourEventArgs)

                '* skip
                If ourEventArgs.Proceed AndAlso Not runtimeOnly Then
                    ' Check
                    Dim aStore As iormRelationalTableStore = Me.PrimaryTableStore
                    aRecord = aStore.GetRecordByPrimaryKey(primarykey.Values)

                    '* not found
                    If aRecord IsNot Nothing Then
                        If Me.ObjectHasDeletePerFlagBehavior Then
                            If aRecord.HasIndex(ConstFNIsDeleted) Then
                                If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                    CoreMessageHandler(message:="deleted (per flag) object found - use undelete instead of create", messagetype:=otCoreMessageType.ApplicationWarning, _
                                                        argument:=primarykey.Values, containerID:=Me.ObjectPrimaryTableID)
                                    Return False
                                End If
                            End If
                        Else
                            Return False
                        End If

                    Else
                        '** use the result to check record on uniqueness
                        record.IsCreated = True
                        Return True ' unqiue
                    End If

                    Return False ' not unique
                Else
                    Return ourEventArgs.Proceed
                End If

            Else

                Return True ' if runTimeOnly only the Cache could be checked
            End If

        End Function


        ''' <summary>
        ''' generic function to create a data object by  a record
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <param name="domainID" > optional domain ID for domain behavior</param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Create(ByRef record As ormRecord, _
                                              Optional domainID As String = Nothing, _
                                              Optional checkUnique As Boolean? = Nothing, _
                                              Optional runtimeOnly As Boolean? = Nothing) As Boolean Implements iormRelationalPersistable.Create

            ''' defautl values
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be created - start session to database first", _
                                           objectname:=Me.ObjectID, argument:=ConstOPCreate, _
                                           messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            '** initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize(runtimeOnly:=runtimeOnly) Then
                Call CoreMessageHandler(message:="dataobject can not be initialized", containerID:=_primaryTableID, argument:=record.ToString, _
                                        procedure:="ormBusinessObject.create", messagetype:=otCoreMessageType.InternalError)

                Return False
            End If
            '** is the object loaded -> no reinit
            If Me.IsLoaded Then
                Call CoreMessageHandler(message:="data object cannot be created if it has state loaded", objectname:=Me.ObjectID, containerID:=_primaryTableID, argument:=record.ToString, _
                                        procedure:="ormBusinessObject.create", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            '** check on the operation right for this object
            If Not runtimeOnly AndAlso _
                   Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, domainid:=domainID, _
                                                                objecttransactions:={Me.ObjectID & "." & ConstOPCreate}) Then
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, loginOnFailed:=True, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                        domainid:=domainID, objecttransactions:={Me.ObjectID & "." & ConstOPCreate}) Then
                    Call CoreMessageHandler(message:="data object cannot be created - permission denied to user", _
                                            objectname:=Me.ObjectID, argument:=ConstOPCreate, _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '**
            Dim aPrimaryKey As ormDatabaseKey

            '** domainid
            If String.IsNullOrEmpty(domainID) Then domainID = ConstGlobalDomain

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs(record:=record, object:=Me, infuseMode:=otInfuseMode.OnCreate, _
                                                           usecache:=Me.UseCache, runtimeonly:=runtimeOnly)
            RaiseEvent OnCreating(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then record = ourEventArgs.Record
                Return ourEventArgs.Proceed
            ElseIf ourEventArgs.Result Then
                record = ourEventArgs.Record
            End If

            '** leave the primary key extraction here after

            '* extract the primary key
            aPrimaryKey = ExtractObjectPrimaryKey(record, objectID:=Me.ObjectID, runtimeOnly:=runtimeOnly)
            '** check for domainBehavior
            aPrimaryKey.ChecknFix(domainid:=domainID, runtimeOnly:=runtimeOnly)

            '** keys must be set in the object itself
            '** create
            _UniquenessInStoreWasChecked = Not runtimeOnly And checkUnique ' remember
            If checkUnique AndAlso Not CheckUniqueness(primarykey:=aPrimaryKey, record:=record, runtimeOnly:=runtimeOnly) Then
                Return False '* not unique
            End If

            '** set on the runtime Only Flag
            If runtimeOnly Then SwitchRuntimeON()

            '''
            ''' raise the Default Values Needed Event
            ''' 
            RaiseEvent OnCreateDefaultValuesNeeded(Me, ourEventArgs)
            If ourEventArgs.Result Then
                record = ourEventArgs.Record
            End If
            ''' set default values
            If Me.ObjectHasDomainBehavior Then
                If Not record.HasIndex(ConstFNDomainID) OrElse String.IsNullOrWhiteSpace(record.GetValue(ConstFNDomainID)) Then
                    record.SetValue(ConstFNDomainID, domainID)
                End If
            End If


            ''' set the record (and merge with property assignement)
            ''' 
            If _record Is Nothing Then
                _record = record
            Else
                MergeRecord(record)
            End If

            ''' infuse what we have in the record
            ''' 
            Dim aDataobject = Me

            If Not InfuseDataObject(record:=record, dataobject:=aDataobject, mode:=otInfuseMode.OnCreate) Then
                CoreMessageHandler(message:="InfuseDataobject failed", messagetype:=otCoreMessageType.InternalError, procedure:="ormBusinessObject.Create")
                If aDataobject.Guid <> Me.Guid Then
                    CoreMessageHandler(message:="data object was substituted in instance create function during infuse ?!", messagetype:=otCoreMessageType.InternalWarning, _
                        procedure:="ormBusinessObject.Create")
                End If
            End If

            '** set status
            _domainID = domainID
            _isCreated = True
            _IsDeleted = False
            Me.SetUnloaded()
            _IsChanged = False

            '* fire Event
            ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, _
                                                      key:=aPrimaryKey, _
                                                      usecache:=Me.UseCache, _
                                                      infuseMode:=otInfuseMode.OnCreate, _
                                                      runtimeonly:=runtimeOnly)
            RaiseEvent OnCreated(Me, ourEventArgs)

            Return ourEventArgs.Proceed

        End Function

        ''' <summary>
        ''' generic function to create a dataobject by primary key
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <param name="domainID" > optional domain ID for domain behavior</param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable Function Create(primarykey As ormDatabaseKey, _
                                              Optional domainID As String = Nothing, _
                                              Optional checkUnique As Boolean? = Nothing, _
                                              Optional runtimeOnly As Boolean? = Nothing) As Boolean Implements iormRelationalPersistable.Create
            ''' defautl values
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False

            '*** add the primary keys
            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be created - start session to database first", _
                                          procedure:="ormBusinessObject.create", objectname:=Me.ObjectID, argument:=ConstOPCreate, _
                                           messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            '** initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize(runtimeOnly:=runtimeOnly) Then
                Call CoreMessageHandler(message:="data object can not be initialized", containerID:=_primaryTableID, argument:=Record.ToString, _
                                        procedure:="ormBusinessObject.create", messagetype:=otCoreMessageType.InternalError)

                Return False
            End If

            '** set default
            If String.IsNullOrEmpty(domainID) Then domainID = ConstGlobalDomain

            '** copy the primary keys
            CopyPrimaryKeyToRecord(primarykey:=primarykey, record:=Me.Record, domainid:=domainID, runtimeOnly:=runtimeOnly)

            ''' run the create with this record
            ''' 
            Return Create(record:=Me.Record, domainID:=domainID, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
        End Function


        ''' <summary>
        ''' Retrieve a data object from the cache or load it
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveDataObject(Of T As {iormInfusable, ormDataObject, iormPersistable, New}) _
            (pkArray() As Object, _
             Optional domainID As String = Nothing, _
             Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
             Optional forceReload As Boolean? = Nothing, _
             Optional runtimeOnly As Boolean? = Nothing) As T
            Return RetrieveDataObject(pkArray:=pkArray, type:=GetType(T), domainID:=domainID, dbdriver:=dbdriver, forceReload:=forceReload, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' Retrieve a data object from the cache or load it
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveDataObject(Of T As {iormInfusable, ormDataObject, iormPersistable, New}) _
            (key As ormDatabaseKey, _
             Optional domainID As String = Nothing, _
             Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
             Optional forceReload As Boolean? = Nothing, _
             Optional runtimeOnly As Boolean? = Nothing) As T
            Return RetrieveDataObject(key:=key, type:=GetType(T), domainID:=domainID, dbdriver:=dbdriver, forceReload:=forceReload, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' Retrieve a data object from the cache or load it - use an array of values which are supposed to be the primary key of the object
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public Overloads Shared Function RetrieveDataObject(pkArray() As Object, type As System.Type, _
                 Optional domainID As String = Nothing, _
                 Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
                 Optional forceReload As Boolean? = Nothing, _
                 Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable

            ''' get the primarykey of the object out of a record and might be primarykey of a secondary table
            ''' 
            Dim aDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(type:=type)
            Dim aPrimaryKey = New ormDatabaseKey(objectid:=aDescriptor.ID, keyvalues:=pkArray)


            Return RetrieveDataObject(key:=aPrimaryKey, type:=type, domainID:=domainID, dbdriver:=dbdriver, forceReload:=forceReload, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' Retrieve a data object from the cache or load it
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ormObjectOperationMethod(Description:="Retrieve a Data Object by primary keys from store)", _
            OperationName:="GeneralRetrieveBy PrimaryKeys", Tag:=ObjectClassDescription.ConstMTRetrieve, TransactionID:=ConstOPRetrieve)> _
        Public Overloads Shared Function RetrieveDataObject(key As ormDatabaseKey, type As System.Type, _
                                                             Optional domainID As String = Nothing, _
                                                             Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
                                                             Optional forceReload As Boolean? = Nothing, _
                                                             Optional runtimeOnly As Boolean? = Nothing) As iormRelationalPersistable

            Dim useCache As Boolean = True
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            If Not forceReload.HasValue Then forceReload = False
            Dim anObject As iormRelationalPersistable = ot.CreateDataObjectInstance(type)


            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be retrieved - start session to database first", _
                                        objectname:=anObject.ObjectID, _
                                        procedure:="ormBusinessObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
            If Not runtimeOnly AndAlso Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObject.ObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                '** request authorizartion
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                            username:=CurrentSession.CurrentUsername, _
                                                                            objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                            objectname:=anObject.ObjectID, argument:=ConstOPInject, username:=CurrentSession.CurrentUsername, _
                                            procedure:="ormBusinessObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If



            '** use Cache ?!
            useCache = anObject.useCache
            Dim hasDomainBehavior As Boolean = anObject.ObjectHasDomainBehavior

            ''' fix primary key
            ''' 
            key.ChecknFix(domainid:=domainID, runtimeOnly:=runtimeOnly)

            ''' check if we have key
            ''' 
            If key.Count = 0 Then
                Call CoreMessageHandler(message:="data object cannot be retrieved - no primary key and also no record for keys provided", _
                                       objectname:=anObject.ObjectID, argument:=ConstOPInject, username:=CurrentSession.CurrentUsername, _
                                       procedure:="ormBusinessObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '* fire event
            Dim ourEventArgs As New ormDataObjectEventArgs(anObject, domainID:=domainID, domainBehavior:=hasDomainBehavior, key:=key, usecache:=useCache)
            RaiseEvent ClassOnRetrieving(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    Return ourEventArgs.DataObject
                Else
                    Return Nothing
                End If

                '*** we have a result yes to use the dataobject supplied
            ElseIf ourEventArgs.Result Then
                anObject = ourEventArgs.DataObject
                useCache = False ' switch off cache

                ''' no positive result from the events
                ''' check if we take the substitute domainID
            ElseIf Not ourEventArgs.Result Then
                If hasDomainBehavior AndAlso domainID <> ConstGlobalDomain Then
                    '* Domain Behavior - is global cached but it might be that we are missing the domain related one if one has been created
                    '* after load of the object - since not in cache
                    key.SubstituteDomainID(domainid:=ConstGlobalDomain, runtimeOnly:=runtimeOnly)
                    '* fire event again
                    ourEventArgs = New ormDataObjectEventArgs(anObject, domainID:=domainID, domainBehavior:=hasDomainBehavior, key:=key)
                    RaiseEvent ClassOnRetrieving(Nothing, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        If ourEventArgs.Result Then
                            Return ourEventArgs.DataObject
                        Else
                            Return Nothing
                        End If
                    ElseIf ourEventArgs.Result Then
                        '** retrieved by success
                        anObject = ourEventArgs.DataObject
                        useCache = False ' switch off cache
                    Else
                        anObject = Nothing
                    End If
                Else
                    anObject = Nothing ' load it
                End If
            Else
                anObject = Nothing ' load it
            End If

            '* load object if not runtime only
            If (anObject Is Nothing OrElse forceReload) And Not runtimeOnly Then
                '* Inject the data object
                anObject = ormBusinessObject.InjectDataObject(primarykey:=key, type:=type, domainid:=domainID, dbdriver:=dbdriver)
                '* domain substitution ?!
                If anObject Is Nothing AndAlso hasDomainBehavior AndAlso domainID <> ConstGlobalDomain Then
                    '* on domain behavior ? -> reload from  the global domain
                    Dim domainKey As ormDatabaseKey = key.Clone
                    key.SubstituteDomainID(domainid:=ConstGlobalDomain, substitueOnlyNothingDomain:=False, runtimeOnly:=runtimeOnly)
                    anObject = ormBusinessObject.RetrieveDataObject(key:=key, type:=type, domainID:=ConstGlobalDomain, dbdriver:=dbdriver)
                    ''' add it to cache
                    If anObject IsNot Nothing Then
                        RaiseEvent ClassOnOverloaded(Nothing, _
                                                      New ormDataObjectOverloadedEventArgs(globalPrimaryKey:=key, domainPrimaryKey:=domainKey, dataobject:=anObject))
                    End If
                End If
            End If

            '* fire event
            If anObject IsNot Nothing Then
                ourEventArgs = New ormDataObjectEventArgs(anObject, record:=anObject.Record, key:=key, usecache:=useCache)
            Else
                ourEventArgs = New ormDataObjectEventArgs(Nothing, record:=Nothing, key:=key, usecache:=useCache)
            End If

            '** fire event
            RaiseEvent ClassOnRetrieved(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    Return ourEventArgs.DataObject
                Else
                    Return Nothing
                End If
            End If
            Return anObject

        End Function
        ''' <summary>
        ''' clone a dataobject with a new pkarray. return nothing if fails
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="cloneobject"></param>
        ''' <param name="newpkarray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CloneDataObject(Of T As {iormRelationalPersistable, iormCloneable, New})(cloneobject As iormCloneable(Of T), newpkarray As Object()) As T
            Return cloneobject.Clone(newpkarray)
        End Function

        ''' <summary>
        ''' this method must be overritten
        ''' </summary>
        ''' <param name="newpkarray"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function CloneObject(newpkarray As Object(), Optional runtimeOnly As Boolean? = Nothing) As Object Implements iormCloneable.Clone
            ''' by intention
            Throw New NotImplementedException(message:="use derived version instead")
        End Function

        ''' <summary>
        ''' cloe the object with an primary key array
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="newpkarray"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Clone(Of T As {iormPersistable, iormInfusable, Class, New})(newpkarray As Object(), _
                                                                                                   Optional runtimeOnly As Boolean? = Nothing) As T
            Dim aPrimarykey As New ormDatabaseKey(objectid:=Me.ObjectID, keyvalues:=newpkarray)
            Return Me.Clone(Of T)(newprimarykey:=aPrimarykey, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(Of T As {iormPersistable, iormInfusable, Class, New})(newprimarykey As ormDatabaseKey, _
                                                                                             Optional runtimeOnly As Boolean? = Nothing) As T
            '
            '*** now we copy the object
            Dim aNewObject As New T
            Dim newRecord As New ormRecord
            If Not runtimeOnly.HasValue Then runtimeOnly = Me.RunTimeOnly

            '**
            If Not Me.IsAlive(subname:="clone") Then Return Nothing


            '* fire class event
            Dim ourEventArgs As New ormDataObjectCloneEventArgs(newObject:=TryCast(aNewObject, ormBusinessObject), oldObject:=Me)
            ourEventArgs.UseCache = Me.UseCache
            RaiseEvent ClassOnCloning(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    If ourEventArgs.DataObject IsNot Nothing Then
                        Return TryCast(ourEventArgs.DataObject, T)
                    Else
                        CoreMessageHandler(message:="ClassOnCloning: cannot convert persistable to class", argument:=GetType(T).Name, procedure:="ormBusinessObject.Clone(of T)", _
                                           messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If
            End If

            '* fire object event
            RaiseEvent OnCloning(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result AndAlso ourEventArgs.DataObject IsNot Nothing Then
                    Return TryCast(ourEventArgs.DataObject, T)
                Else
                    Return Nothing
                End If
            End If

            ' set it
            If Not runtimeOnly Then newRecord.SetTable(Me.ObjectPrimaryTableID)

            ' go through the table and overwrite the Record if the rights are there
            For Each entryname In Me.Record.Keys
                If entryname <> ConstFNCreatedOn And entryname <> ConstFNUpdatedOn _
                    And entryname <> ConstFNIsDeleted And entryname <> ConstFNDeletedOn _
                    And entryname <> ConstFNIsDomainIgnored Then

                    Call newRecord.SetValue(entryname, Me.Record.GetValue(entryname))
                End If
            Next entryname

            ''' copy the new primary keys
            Me.CopyPrimaryKeyToRecord(newprimarykey, newRecord, runtimeOnly:=Me.RunTimeOnly)

            ''' create the new object with the record
            ''' 
            If Not aNewObject.Create(record:=newRecord, checkUnique:=True) Then
                Call CoreMessageHandler(message:="object new keys are not unique - clone aborted", argument:=newprimarykey, containerID:=_primaryTableID, _
                                       messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            '** Fire Event
            ourEventArgs = New ormDataObjectCloneEventArgs(newObject:=TryCast(aNewObject, ormBusinessObject), oldObject:=Me)

            RaiseEvent OnCloned(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result AndAlso ourEventArgs.DataObject IsNot Nothing Then
                    Return TryCast(ourEventArgs.DataObject, T)
                Else
                    Return Nothing
                End If
            End If

            '** Fire class Event
            RaiseEvent ClassOnCloned(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result AndAlso ourEventArgs.DataObject IsNot Nothing Then
                    Return TryCast(ourEventArgs.DataObject, T)
                Else
                    Return Nothing
                End If
            End If

            ''' return
            ''' 
            Return aNewObject
        End Function


        ''' <summary>
        ''' Undelete the data object
        ''' </summary>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Public Function Undelete() As Boolean
            If Not Me.IsInitialized Then
                If Not Me.Initialize Then
                    Return False
                End If
            End If

            '* fire event
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, key:=Me.ObjectPrimaryKey, runtimeOnly:=Me.RunTimeOnly)
            RaiseEvent OnUnDeleting(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return ourEventArgs.Result
            End If

            '* undelete if possible
            Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
            If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.HasDeleteFieldBehavior Then
                _IsDeleted = False
                _deletedOn = Nothing
                '* fire event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, _
                                                          key:=Me.ExtractObjectPrimaryKey(record:=Me.Record, objectID:=Me.ObjectID, runtimeOnly:=Me.RunTimeOnly), _
                                                           runtimeOnly:=Me.RunTimeOnly, usecache:=Me.UseCache)
                ourEventArgs.Result = True
                ourEventArgs.Proceed = True
                RaiseEvent OnUnDeleted(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                End If
                If ourEventArgs.Result Then
                    CoreMessageHandler(message:="data object undeleted", procedure:="ormBusinessObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                        containerID:=Me.ObjectPrimaryTableID)
                    Return True
                Else
                    CoreMessageHandler(message:="data object cannot be undeleted by event - delete per flag behavior not set", procedure:="ormBusinessObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                     containerID:=Me.ObjectPrimaryTableID)
                    Return False
                End If

            Else
                CoreMessageHandler(message:="data object cannot be undeleted - delete per flag behavior not set", procedure:="ormBusinessObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                     containerID:=Me.ObjectPrimaryTableID)
                Return False
            End If


        End Function
        ''' <summary>
        ''' Delete the object and its persistancy
        ''' </summary>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overridable Function Delete(Optional timestamp As DateTime? = Nothing) As Boolean Implements iormRelationalPersistable.Delete

            '** initialize -> no error if not alive
            If Not Me.IsAlive(throwError:=False) Then Return False
            If Not timestamp.HasValue OrElse timestamp = constNullDate Then timestamp = DateTime.Now

            '** check on the operation right for this object
            If Not RunTimeOnly AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, _
                                                                               domainid:=DomainID, _
                                                                                objecttransactions:={Me.ObjectID & "." & ConstOPDelete}) Then

                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, username:=CurrentSession.CurrentUsername, _
                                                        domainid:=DomainID, loginOnFailed:=True, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                         objecttransactions:={Me.ObjectID & "." & ConstOPDelete}) Then
                    Call CoreMessageHandler(message:="data object cannot be deleted - permission denied to user", _
                                            objectname:=Me.ObjectID, argument:=ConstOPDelete, username:=CurrentSession.CurrentUsername, _
                                            procedure:="ormBusinessObject.Delete", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '** Fire Event
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, key:=Me.ObjectPrimaryKey, _
                                                           usecache:=Me.UseCache, runtimeOnly:=Me.RunTimeOnly, timestamp:=timestamp)
            RaiseEvent ClassOnDeleting(Me, ourEventArgs)
            RaiseEvent OnDeleting(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return ourEventArgs.Result
            End If

            '*** cascade the operation through the related members
            Dim result As Boolean = Me.CascadeRelations(cascadeDelete:=True)

            If result Then
                '** determine how to delete
                Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                '** per flag
                If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.HasDeleteFieldBehavior Then
                    _IsDeleted = True
                    _deletedOn = timestamp
                    Feed()
                    '** save only on the record level
                    If Me.IsLoaded AndAlso Not Me.RunTimeOnly Then _IsDeleted = _record.Persist(timestamp)
                Else
                    'delete the  object itself
                    If Not Me.RunTimeOnly AndAlso Me.IsLoaded Then _IsDeleted = _record.Delete()
                    If _IsDeleted Then
                        Me.SetUnloaded()
                        _deletedOn = timestamp
                    End If

                End If

                '** fire Event
                ourEventArgs.Result = _IsDeleted
                RaiseEvent OnDeleted(Me, ourEventArgs)
                RaiseEvent ClassOnDeleted(Me, ourEventArgs)
                Return _IsDeleted
            Else
                CoreMessageHandler("object could not delete  cascaded objected", procedure:="ormBusinessObject.Delete", objectname:=Me.ObjectID, _
                                   argument:=Converter.Array2StringList(Me.ObjectPrimaryKeyValues))
                Return False
            End If

        End Function
       
       
    End Class


End Namespace
