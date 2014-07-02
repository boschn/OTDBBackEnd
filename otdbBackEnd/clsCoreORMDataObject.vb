
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA OBJECT CLASSES
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-31
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Reflection
Imports OnTrack.Commons

Namespace OnTrack.Database



    ''' <summary>
    ''' abstract base class for all data objects
    ''' handles the data operations with an embedded record
    ''' raises all data events
    ''' </summary>
    ''' <remarks></remarks>
    Partial Public MustInherit Class ormDataObject
        Implements System.ComponentModel.INotifyPropertyChanged
        Implements iormPersistable
        Implements iormInfusable
        Implements iormCloneable
        Implements iormValidatable
        Implements iormQueriable
        Implements iormLoggable 

        ''' <summary>
        ''' important objects to drive data object behavior
        ''' </summary>
        ''' <remarks></remarks>

        Private WithEvents _record As ormRecord                   ' record to save persistency
        Private WithEvents _relationMgr As DataObjectRelationMgr  ' relation manager to manage to objects relations
        Private WithEvents _validator As ObjectValidator          ' valitator to validate

        ''' <summary>
        ''' guid as identity
        ''' </summary>
        ''' <remarks></remarks>
        Private _guid As Guid = Guid.NewGuid

        ''' <summary>
        ''' tables for storing the record in 
        ''' </summary>
        ''' <remarks></remarks>
        Private _primaryTableID As String = ""
        Private _tableids As String() = {}
        Private _primarykeynames As String() = {} ' cache object primary key names
        Private _primaryKeyValues As Object = {} ' cache of object primary key values must be unique

        ''' <summary>
        ''' cached links and objects
        ''' </summary>
        ''' <remarks></remarks>
        Private WithEvents _classDescription As ObjectClassDescription
        Private WithEvents _objectdefinition As ObjectDefinition
        Private WithEvents _defaultdbdriver As iormDatabaseDriver

        ''' <summary>
        ''' liefetime status and valiables
        ''' </summary>
        ''' <remarks></remarks>
        Private _isCreated As Boolean = False   'true if created by .CreateXXX Functions
        Private _isLoaded As Boolean = False    'true if loaded by .RETRIEVE
        Private _isInfused As Boolean = False   'true if infused by .INFUSEDXXXX
        Private _InfusionTimeStamp As DateTime
        Private _IsChanged As Boolean = False  'true if has changed and persisted is needed to retrieve the object as it is now
        Private _changeTimeStamp As DateTime
        Private _IsInitialized As Boolean = False 'true if initialized all internal members to run a persistable data object

        Private _UniquenessInStoreWasChecked As Boolean 'true if the check uniqueness function has run 

        Private _RunTimeOnly As Boolean = False     'if Object is only kept in Memory (no persist, no Record according to table, no DBDriver necessary, no checkuniqueness)
        Private _useCache As Nullable(Of Boolean) 'cache variable of the ObjectDefinition.UseCache Property

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
        ''' Persistence Data Definition
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
            title:="Domain", description:="domain of the business Object", _
            defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
            posordinal:=1000, _
            foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.Cascade & ")", _
                                    ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntry(Datatype:=otDataType.Bool, isnullable:=True, posordinal:=1001, _
            title:="Ignore Domain", description:="flag if the domainValue is to be ignored -> look in global")> _
        Public Const ConstFNIsDomainIgnored As String = "domainignore"



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
        ''' Member Entries to drive lifecycle
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, isreadonly:=True, posordinal:=9901, _
           title:="Updated On", Description:="last update time stamp in the data store")> Public Const ConstFNUpdatedOn As String = ot.ConstFNUpdatedOn

        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, isreadonly:=True, posordinal:=9902, _
            title:="Created On", Description:="creation time stamp in the data store")> Public Const ConstFNCreatedOn As String = ot.ConstFNCreatedOn

        <ormObjectEntry(Datatype:=otDataType.Timestamp, isnullable:=True, isreadonly:=True, posordinal:=9903, _
            title:="Deleted On", Description:="time stamp when the deletion flag was set")> Public Const ConstFNDeletedOn As String = ot.ConstFNDeletedOn

        '** Deleted flag
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", posordinal:=9904, _
            title:="Deleted", description:="flag if the entry in the data stored is regarded as deleted depends on the deleteflagbehavior")> _
        Public Const ConstFNIsDeleted As String = ot.ConstFNIsDeleted

        ''' <summary>
        ''' ColumnMapping
        ''' </summary>
        ''' <remarks></remarks>

        <ormEntryMapping(EntryName:=ConstFNUpdatedOn)> Protected _updatedOn As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNCreatedOn)> Protected _createdOn As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNDeletedOn)> Protected _deletedOn As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNIsDeleted)> Protected _IsDeleted As Boolean = False

        '** Spare Parameters
        <ormEntryMapping(EntryName:=ConstFNParamText1)> Protected _parameter_txt1 As String
        <ormEntryMapping(EntryName:=ConstFNParamText2)> Protected _parameter_txt2 As String
        <ormEntryMapping(EntryName:=ConstFNParamText3)> Protected _parameter_txt3 As String
        <ormEntryMapping(EntryName:=ConstFNParamNum1)> Protected _parameter_num1 As Nullable(Of Double)
        <ormEntryMapping(EntryName:=ConstFNParamNum2)> Protected _parameter_num2 As Nullable(Of Double)
        <ormEntryMapping(EntryName:=ConstFNParamNum3)> Protected _parameter_num3 As Nullable(Of Double)
        <ormEntryMapping(EntryName:=ConstFNParamDate1)> Protected _parameter_date1 As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNParamDate2)> Protected _parameter_date2 As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNParamDate3)> Protected _parameter_date3 As Nullable(Of Date)
        <ormEntryMapping(EntryName:=ConstFNParamFlag1)> Protected _parameter_flag1 As Nullable(Of Boolean)
        <ormEntryMapping(EntryName:=ConstFNParamFlag2)> Protected _parameter_flag2 As Nullable(Of Boolean)
        <ormEntryMapping(EntryName:=ConstFNParamFlag3)> Protected _parameter_flag3 As Nullable(Of Boolean)

        <ormEntryMapping(EntryName:=ConstFNDomainID)> Protected _domainID As String = ConstGlobalDomain
        <ormEntryMapping(EntryName:=ConstFNIsDomainIgnored)> Protected _DomainIsIgnored As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNmsglogtag)> Protected _msglogtag As String

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
        ''' Relation to ScheduleDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ObjectMessage), retrieveOperation:=ConstOPGetObjectMessages, _
                     cascadeonCreate:=False, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRMessageLog = "RelObjectMessage"

        <ormEntryMapping(relationName:=ConstRMessageLog, infusemode:=otInfuseMode.OnDemand)> Protected WithEvents _ObjectMessageLog As ObjectMessageLog '  MessageLog

        ''' <summary>
        ''' constructor for ormDataObject
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <remarks></remarks>
        ''' 
        Protected Sub New(Optional runtimeonly As Boolean = False)
            _IsInitialized = False
            _RunTimeOnly = runtimeonly
            _relationMgr = New DataObjectRelationMgr(Me)
        End Sub

        Protected Sub New(tableid As String, _
                          Optional objectID As String = "", _
                          Optional dbdriver As iormDatabaseDriver = Nothing, _
                          Optional runtimeonly As Boolean = False)
            _IsInitialized = False
            If tableid <> "" Then _primaryTableID = tableid
            If objectID <> "" Then
                _classDescription = ot.GetObjectClassDescriptionByID(id:=objectID)
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
            End If
            _RunTimeOnly = runtimeonly
            _defaultdbdriver = dbdriver
            _relationMgr = New DataObjectRelationMgr(Me)
        End Sub
        ''' <summary>
        ''' clean up with the object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Finialize()
            _IsInitialized = False
            Me.Record = Nothing
            _primaryTableID = ""
            _defaultdbdriver = Nothing
            _ObjectMessageLog = Nothing
            _relationMgr = Nothing
        End Sub

        ' If you try to get a value of a property that is
        ' not defined in the class, this method is called.
        ''' <summary>
        ''' dynamic getValue Property
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TryGetMember(ByVal binder As System.Dynamic.GetMemberBinder, ByRef result As Object) As Boolean
            If Not Me.IsAlive(throwError:=False) Then Return False
            ' Converting the property name to lowercase
            ' so that property names become case-insensitive.
            Dim name As String = binder.Name

            ' If the property name is found in a dictionary,
            ' set the result parameter to the property value and return true.
            ' Otherwise, return false.

            If Me.ObjectDefinition.HasEntry(name) Then
                result = Me.GetValue(entryname:=name)
                Return True
            End If

            Return False
        End Function
        ''' <summary>
        ''' Dynamic setValue Property
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TrySetMember(ByVal binder As System.Dynamic.SetMemberBinder, ByVal value As Object) As Boolean
            If Not Me.IsAlive(throwError:=False) Then Return False

            If Not Me.ObjectDefinition.HasEntry(binder.Name) Then
                Return False
            End If

            Return False
            ' Converting the property name to lowercase
            ' so that property names become case-insensitive.
            Return Me.SetValue(entryname:=binder.Name, value:=value)
        End Function

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
        ''' returns the value of the compound entry name
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetCompoundValue(entryname As String) As Object
            Try
                Dim anObjectEntry = Me.ObjectDefinition.GetEntry(entryname)
                If Not anObjectEntry.IsCompound Then
                    CoreMessageHandler(message:="Object entry is a not a compound - use GetValue", arg1:=entryname, _
                         objectname:=Me.ObjectID, entryname:=entryname, _
                          messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetCompoundValue")
                    Return Nothing
                End If

                '''
                ''' 1. check if compound is connected with a getter ?!
                ''' 
                Dim aGetterName As String = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundGetterMethodName
                If aGetterName IsNot Nothing Then

                    ''' branch out to setter method
                    ''' 
                    Dim aOperationAttribute = Me.ObjectClassDescription.GetObjectOperationAttribute(name:=aGetterName)
                    If aOperationAttribute Is Nothing Then
                        CoreMessageHandler(message:="operation id not found in the class description repository", arg1:=aGetterName, _
                                           messagetype:=otCoreMessageType.InternalError, objectname:=Me.ObjectID, _
                                           subname:="DataObjetRelationMGr.GetRelatedObjectsFromOperation")
                        Return Nothing
                    End If

                    ''' check the data on the method to be called
                    ''' 

                    Dim aMethodInfo As MethodInfo = aOperationAttribute.MethodInfo
                    Dim aReturnType As System.Type = aMethodInfo.ReturnType
                    If Not aReturnType.Equals(GetType(Boolean)) Then
                        Call CoreMessageHandler(subname:="ormDataObject.GetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                      message:="getter operation must return a boolean value", _
                                      arg1:=aGetterName, objectname:=Me.ObjectID, entryname:=entryname)
                    End If
                    Dim aDelegate As ObjectClassDescription.OperationCallerDelegate = Me.ObjectClassDescription.GetOperartionCallerDelegate(aGetterName)
                    Dim theParameterEntries As String() = aOperationAttribute.ParameterEntries
                    Dim theParameters As Object()
                    Dim returnValueIndex As Integer
                    Dim returnValue As Object ' dummy
                    ReDim theParameters(aMethodInfo.GetParameters.Count - 1)
                  
                    ''' set the parameters for the delegate
                    For i = 0 To theParameters.GetUpperBound(0)
                        Dim j As Integer = aMethodInfo.GetParameters(i).Position
                        If j >= theParameterEntries.GetLowerBound(0) AndAlso j <= theParameterEntries.GetUpperBound(0) _
                            AndAlso theParameterEntries(j) IsNot Nothing Then

                            Select Case theParameterEntries(j)
                                Case ObjectCompoundEntry.ConstFNEntryName
                                    theParameters(j) = entryname
                                Case ObjectCompoundEntry.ConstFNValues
                                    theParameters(j) = returnValue
                                    returnValueIndex = j
                                Case Domain.ConstFNDomainID
                                    theParameters(j) = Me.DomainID
                            End Select

                        End If
                    Next

                    ''' call the Operation
                    ''' 
                    Dim result As Object = aDelegate(Me, theParameters)
                    If DirectCast(result, Boolean) = True Then
                        Return theParameters(returnValueIndex)
                    Else
                        Call CoreMessageHandler(subname:="ormDataObject.GetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                      message:="getter operation failed to return a  value", _
                                      arg1:=aGetterName, objectname:=Me.ObjectID, entryname:=entryname)
                        Return Nothing
                    End If

                Else
                    '''
                    '''2.  get the relation path and resolve to object
                    ''' 
                    Dim aRelationPath As String() = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundRelationPath
                    Dim names = aRelationPath(0).Split("."c)
                    Dim aRelationname As String

                    If names.Count > 1 Then
                        aRelationname = names(1)
                    Else
                        aRelationname = names(0)
                    End If


                    ''' request a relation load
                    ''' 
                    If _relationMgr.Status(aRelationname) = DataObjectRelationMgr.RelationStatus.Unloaded Then
                        Me.InfuseRelation(aRelationname)
                    End If

                    ''' get the entry which is holding the needed data object
                    ''' 
                    Dim aFieldList As List(Of FieldInfo) = Me.ObjectClassDescription.GetMappedRelationFieldInfos(relationName:=aRelationname)

                    ''' if last hop
                    ''' 
                    ''' have we reached the last hop ?
                    ''' 
                   
                    Dim searchvalue As Object = Nothing ' by intension (all are selected if nothing)
                    Dim searchvalueentryname As String
                    Dim searchentryname As String
                    ''' if last hop
                    ''' 
                    ''' have we reached the last hop ?
                    ''' 
                    If aRelationPath.Count = 2 Then
                        searchvalue = entryname
                        searchentryname = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundIDEntryname
                        searchvalueentryname = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundValueEntryName
                    Else
                        searchvalueentryname = entryname
                        ' do not search anything -> get the objects returned to relation
                        searchentryname = Nothing
                        searchvalue = Nothing
                    End If
                    ''' get the reference data object selected by compoundID - and also load it
                    ''' 
                    Dim theReferenceObjects = _relationMgr.GetObjectsFromContainer(relationname:=aRelationname, entryname:=searchentryname, value:=searchvalue, _
                                                                                   loadRelationIfNotloaded:=True)

                    ''' request the value from there
                    ''' 
                    If theReferenceObjects.Count > 0 Then
                        ' prevent having no value
                        If searchvalueentryname Is Nothing Then searchvalueentryname = entryname
                        Return theReferenceObjects.First.GetValue(searchvalueentryname)
                    ElseIf _relationMgr.Status(aRelationname) = DataObjectRelationMgr.RelationStatus.Loaded Then
                        Return Nothing
                    Else
                        Call CoreMessageHandler(subname:="ormDataObject.GetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                       message:="relation couldnot be loaded - no value could be returned to compound", _
                                       arg1:=aRelationname, objectname:=Me.ObjectID, entryname:=entryname, tablename:=Me.PrimaryTableID)
                        Return Nothing
                    End If


                End If




            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormDataObject.GetCompoundValue", objectname:=Me.ObjectID, arg1:=Me.PrimaryKeyValues, entryname:=entryname, tablename:=Me.PrimaryTableID)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' returns the value of an object entry of this object either a column entry or a compound
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="member"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetValue(entryname As String, Optional ByRef fieldmembername As String = "") As Object Implements iormPersistable.GetValue
            Dim result As Boolean = False

            Try

                Dim value As Object
                Dim aClassDescription = Me.ObjectClassDescription ' ot.GetObjectClassDescription(Me.GetType)
                If aClassDescription Is Nothing Then
                    CoreMessageHandler(message:=" Object's Class Description could not be retrieved - object not defined ?!", arg1:=value, _
                                      objectname:=Me.ObjectID, entryname:=entryname, _
                                       messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    Return False
                End If

                '''
                ''' check if object entry is a compound -> branch out
                ''' 
                If Not aClassDescription.HasObjectEntryAttribute(entryname) Then
                    If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                        CoreMessageHandler(message:="Object entry does not exist in object class description - on bootstrap installation or session startup compounds are not allowed to be used. ", _
                                       arg1:=entryname, objectname:=Me.ObjectID, entryname:=entryname, _
                                       messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    Else
                        If Me.ObjectDefinition.HasEntry(entryname, isActive:=True) Then
                            Dim anObjectEntry = Me.ObjectDefinition.GetEntry(entryname)
                            '''
                            ''' branch out to retrieve compound value
                            ''' 
                            If anObjectEntry.IsCompound Then
                                Return Me.GetCompoundValue(entryname)
                            Else
                                ''' well it is a column although the class doesnot know it ? Might be not enabled
                                ''' 
                                CoreMessageHandler(message:="Object entry is a column entry although not described as one in the class description", arg1:=entryname, _
                                     objectname:=Me.ObjectID, entryname:=entryname, _
                                      messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                                Return Nothing
                            End If

                        Else
                            CoreMessageHandler(message:="Object entry does not exist in object definition or is not enabled", arg1:=entryname, _
                                      objectname:=Me.ObjectID, entryname:=entryname, _
                                       messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                        End If
                    End If
                End If

                '''
                ''' retrieve the fieldinfos of the mapping
                ''' 
                Dim thefieldinfos = aClassDescription.GetEntryFieldInfos(entryname)
                If thefieldinfos.Count = 0 Then
                    CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to a class field member or the entry name is not valid", arg1:=value, _
                                       objectname:=Me.ObjectID, entryname:=entryname, _
                                        messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    Return Nothing

                ElseIf thefieldinfos.Count > 1 And fieldmembername = "" Then
                    ''' ????
                End If

                '''
                ''' retrieve the ObjectEntryAttribute
                ''' 
                Dim anEntryAttribute = aClassDescription.GetObjectEntryAttribute(entryname)
                If anEntryAttribute Is Nothing Then
                    CoreMessageHandler(message:="object entry attribute couldnot be retrieved from class description", arg1:=value, _
                                       objectname:=Me.ObjectID, entryname:=entryname, _
                                        messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    Return Nothing
                End If

                Dim isnullable As Boolean = False
                If anEntryAttribute.HasValueIsNullable Then
                    isnullable = anEntryAttribute.IsNullable
                End If


                '''
                ''' search values of the mapped fields
                ''' 
                For Each field In thefieldinfos

                    If Not Reflector.GetFieldValue(field:=field, dataobject:=Me, value:=value) Then
                        CoreMessageHandler(message:="field value ob data object couldnot be retrieved", _
                                            objectname:=Me.ObjectID, subname:="ormDataObject.getValue", _
                                            messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.PrimaryTableID)
                    End If

                    '** if not specified take the first one
                    If fieldmembername = "" Then
                        fieldmembername = field.Name
                        Return value

                        '** check if specified
                    ElseIf fieldmembername.ToUpper = field.Name.ToUpper Then
                        Return value
                    End If

                Next

                '  the field was not found but the entry
                CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to class member", _
                                      objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                Return value


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormDataObject.getvalue", arg1:=entryname)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' sets the value of the compound entry name
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SetCompoundValue(entryname As String, value As Object) As Boolean
            Try
                Dim oldvalue As Object
                Dim anObjectEntry = Me.ObjectDefinition.GetEntry(entryname)
                If Not anObjectEntry.IsCompound Then
                    CoreMessageHandler(message:="Object entry is a not a compound - use SetValue", arg1:=entryname, _
                         objectname:=Me.ObjectID, entryname:=entryname, _
                          messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetCompoundValue")
                    Return False
                ElseIf anObjectEntry.IsReadonly Then
                    CoreMessageHandler(message:="Object entry is read-Only - set Value is forbidden", arg1:=entryname, _
                         objectname:=Me.ObjectID, entryname:=entryname, _
                          messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetCompoundValue")
                    Return False
                End If

                '''
                ''' 1. check if compound is connected with a setter ?!
                ''' 
                Dim aSetterName As String = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundSetterMethodName
                If aSetterName IsNot Nothing AndAlso aSetterName <> "" Then

                    ''' 
                    ''' -> branch out to setter method
                    ''' 
                    Dim aOperationAttribute = Me.ObjectClassDescription.GetObjectOperationAttribute(name:=aSetterName)
                    If aOperationAttribute Is Nothing Then
                        CoreMessageHandler(message:="operation id not found in the class description repository", arg1:=aSetterName, _
                                           messagetype:=otCoreMessageType.InternalError,
                                           objectname:=Me.ObjectID, _
                                           subname:="DataObjetRelationMGr.SetRelatedObjectsFromOperation")
                        Return Nothing
                    End If

                    ''' check the data on the method to be called
                    ''' 

                    Dim aMethodInfo As MethodInfo = aOperationAttribute.MethodInfo
                    Dim aReturnType As System.Type = aMethodInfo.ReturnType
                    If Not aReturnType.Equals(GetType(Boolean)) Then
                        Call CoreMessageHandler(subname:="ormDataObject.SetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                      message:="Setter operation must return a boolean value", _
                                      arg1:=aSetterName, objectname:=Me.ObjectID, entryname:=entryname)
                    End If
                    Dim aDelegate As ObjectClassDescription.OperationCallerDelegate = Me.ObjectClassDescription.GetOperartionCallerDelegate(aSetterName)
                    Dim theParameterEntries As String() = aOperationAttribute.ParameterEntries
                    Dim theParameters As Object()
                    ReDim theParameters(aMethodInfo.GetParameters.Count - 1)
                    ''' set the parameters for the delegate
                    For i = 0 To theParameters.GetUpperBound(0)
                        Dim j As Integer = aMethodInfo.GetParameters(i).Position
                        If j >= 0 AndAlso j <= theParameters.GetUpperBound(0) Then
                            Select Case theParameterEntries(j)
                                Case ObjectCompoundEntry.ConstFNEntryName
                                    theParameters(j) = entryname
                                Case ObjectCompoundEntry.ConstFNValues
                                    theParameters(j) = value
                                Case Domain.ConstFNDomainID
                                    theParameters(j) = Me.DomainID
                            End Select

                        End If
                    Next

                    ''' Raise the event
                    Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=entryname, value:=value)
                    RaiseEvent OnEntryChanging(Me, e:=args)
                    If args.Proceed Then
                        ''' call the Operation
                        ''' 
                        Dim result As Object = aDelegate(Me, theParameters)
                        If DirectCast(result, Boolean) = True Then

                            RaiseEvent OnEntryChanged(Me, e:=args)
                            Return args.Proceed
                        Else
                            Call CoreMessageHandler(subname:="ormDataObject.SetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                          message:="setter operation failed", arg1:=aSetterName, objectname:=Me.ObjectID, entryname:=entryname)

                            Return Nothing
                        End If
                    Else
                        Return False
                    End If

                Else
                    '''
                    ''' 2. get the relation and travel along it
                    ''' 
                    Dim aRelationPath As String() = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundRelationPath
                    Dim names = aRelationPath(0).Split("."c)
                    Dim aRelationname As String
                    Dim lastHop As Boolean = False

                    If names.Count > 1 Then
                        aRelationname = names(1)
                    Else
                        aRelationname = names(0)
                    End If

                    ''' request a relation load
                    ''' 
                    If _relationMgr.Status(aRelationname) = DataObjectRelationMgr.RelationStatus.Unloaded Then
                        Me.InfuseRelation(aRelationname)
                    End If


                    ''' if last hop
                    ''' 
                    ''' have we reached the last hop ?
                    ''' 


                    Dim searchvalue As Object = Nothing ' by intension (all are selected if nothing)
                    Dim searchvalueentryname As String
                    Dim searchentryname As String
                    ''' if last hop
                    ''' 
                    ''' have we reached the last hop ?
                    ''' 
                    If aRelationPath.Count = 2 Then
                        searchvalue = entryname
                        searchentryname = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundIDEntryname
                        searchvalueentryname = TryCast(anObjectEntry, ObjectCompoundEntry).CompoundValueEntryName
                    Else
                        searchvalueentryname = entryname
                        ' do not search anything -> get the objects returned to relation
                        searchentryname = Nothing
                        searchvalue = Nothing
                    End If

                    ''' get the reference data object selected by compoundID and load it 
                    ''' 
                    Dim theReferenceObjects = _relationMgr.GetObjectsFromContainer(relationname:=aRelationname, entryname:=searchentryname, value:=searchvalue, _
                                                                                  loadRelationIfNotloaded:=True)

                    ''' request the value from there
                    ''' 
                    If theReferenceObjects.Count > 0 Then
                        '' prevent having no value
                        If searchvalueentryname Is Nothing Then searchvalueentryname = entryname
                        Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=searchvalueentryname, value:=value)
                        RaiseEvent OnEntryChanging(Me, e:=args)
                        If args.Proceed Then
                            ''' recursion call to the setvalue of the next object (related one) to resolve the entry
                            ''' 
                            If theReferenceObjects.First.SetValue(searchvalueentryname, value) Then
                                RaiseEvent OnEntryChanged(Me, e:=args)
                                Return args.Proceed
                            End If
                        Else
                            Return False
                        End If

                    ElseIf _relationMgr.Status(aRelationname) = DataObjectRelationMgr.RelationStatus.Loaded Then
                        '''
                        ''' create the relation and reload
                        ''' 
                        If _relationMgr.CreateNInfuseRelations(mode:=otInfuseMode.None, relationnames:={aRelationname}.ToList) Then
                            theReferenceObjects = _relationMgr.GetObjectsFromContainer(relationname:=aRelationname, entryname:=searchentryname, value:=searchvalue, _
                                                                                       loadRelationIfNotloaded:=True)
                            ''' request the value from there
                            ''' 
                            If theReferenceObjects.Count > 0 Then

                                ''' recursion call to the setvalue of the next object (related one) to resolve the entry
                                ''' 

                                Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=searchvalueentryname, value:=value)
                                RaiseEvent OnEntryChanging(Me, e:=args)
                                If args.Proceed Then
                                    ''' recursion call to the setvalue of the next object (related one) to resolve the entry
                                    ''' 
                                    If theReferenceObjects.First.SetValue(searchvalueentryname, value) Then
                                        RaiseEvent OnEntryChanged(Me, e:=args)
                                        Return args.Proceed
                                    End If
                                Else
                                    Return False
                                End If
                            Else
                                Call CoreMessageHandler(subname:="ormDataObject.SetCompoundValue", messagetype:=otCoreMessageType.InternalWarning, _
                                          message:="compound could not be set - related object create succeeded but retrieve failed ", _
                                          arg1:=aRelationname, objectname:=Me.ObjectID, entryname:=entryname, tablename:=Me.PrimaryTableID)
                                Return True
                            End If

                        Else
                            Call CoreMessageHandler(subname:="ormDataObject.SetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                           message:="compound could not be set - related object could not be created ", _
                                           arg1:=aRelationname, objectname:=Me.ObjectID, entryname:=entryname, tablename:=Me.PrimaryTableID)
                            Return True
                        End If

                    Else
                        Call CoreMessageHandler(subname:="ormDataObject.SetCompoundValue", messagetype:=otCoreMessageType.InternalError, _
                                       message:="relation could not be loaded - no value could be returned to compound", _
                                       arg1:=aRelationname, objectname:=Me.ObjectID, entryname:=entryname, tablename:=Me.PrimaryTableID)
                        Return False
                    End If

                End If


            Catch ex As Exception


                CoreMessageHandler(exception:=ex, subname:="ormDataObject.SetCompoundValue")
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' normalize a value and apply EntryProperties
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function NormalizeValue(entryname As String, ByRef value As Object) As Boolean Implements iormInfusable.Normalizevalue
            Dim result As Boolean = False
            Dim outvalue As Object
            Dim isnullable As Boolean = False
            Dim aDatatype As otDataType
            Dim anEntry As iormObjectEntry
            ''' 
            ''' APPLY THE ENTRY PROPERTIES AND TRANSFORM THE VALUE REQUESTED
            ''' 
            If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                anEntry = Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname)
                If anEntry Is Nothing Then
                    CoreMessageHandler(message:="entryname not found in object class repository - value not checked", arg1:=value, subname:="ormDataObject.NormalizeValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

                ''' set value to default value if nothing and not nullable
                ''' 
                If Not anEntry.IsNullable AndAlso value Is Nothing Then
                    value = Me.ObjectEntryDefaultValue(entryname:=entryname)
                    If value Is Nothing Then value = ot.GetDefaultValue(anEntry.DataType)
                End If

                ''' on bootstrapping let the routine to sort out how to get the properties
                If Not EntryProperties.Apply(objectid:=Me.ObjectID, entryname:=entryname, [in]:=value, out:=outvalue) Then
                    CoreMessageHandler(message:="applying object entry properties failed - value not checked", arg1:=value, subname:="ormDataObject.EqualsValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                Else
                    value = outvalue
                End If
            Else
                anEntry = Me.ObjectDefinition.GetEntry(entryname:=entryname)
                If anEntry Is Nothing Then
                    CoreMessageHandler(message:="entryname not found in object class repository - value not checked", arg1:=value, subname:="ormDataObject.NormalizeValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                Else
                    aDatatype = anEntry.Datatype
                End If

                ''' set value to default value if nothing and not nullable
                ''' 
                If Not anEntry.IsNullable AndAlso value Is Nothing Then
                    value = Me.ObjectEntryDefaultValue(entryname:=entryname)
                    If value Is Nothing Then value = ot.GetDefaultValue(anEntry.Datatype)
                End If

                ''' use semy optimized way - object definition is cached / entry has to be looked up
                ''' 
                If Not EntryProperties.Apply(Me.ObjectDefinition, entryname:=entryname, [in]:=value, out:=outvalue) Then
                    CoreMessageHandler(message:="applying object entry properties failed - value not checked", arg1:=value, subname:="ormDataObject.EqualsValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                Else
                    value = outvalue
                End If
            End If

            Return True
        End Function
        ''' <summary>
        ''' check if the entryname has the same value as supplied
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="member"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EqualsValue(entryname As String, ByVal value As Object) As Boolean Implements iormPersistable.EqualsValue
            Dim result As Boolean = False
            Dim outvalue As Object
            Dim isnullable As Boolean = False
            Dim aDatatype As otDataType

            ''' 
            ''' PHASE I : APPLY THE ENTRY PROPERTIES AND TRANSFORM THE VALUE REQUESTED
            ''' 
            If Not NormalizeValue(entryname, value) Then
                CoreMessageHandler(message:="Warning ! Could not normalize value", arg1:=value, objectname:=Me.ObjectID, _
                                    entryname:=entryname, subname:="ormDataObject.equalsValue")
            End If

            ''' get datatype
            ''' 
            If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                Dim anEntry = Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname)
                If anEntry Is Nothing Then
                    CoreMessageHandler(message:="entryname not found in object class repository - value not checked", arg1:=value, subname:="ormDataObject.EqualsValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                Else
                    aDatatype = anEntry.DataType
                End If

            Else
                Dim anEntry = Me.ObjectDefinition.GetEntry(entryname:=entryname)
                If anEntry Is Nothing Then
                    CoreMessageHandler(message:="entryname not found in object class repository - value not checked", arg1:=value, subname:="ormDataObject.EqualsValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                Else
                    aDatatype = anEntry.Datatype
                End If

            End If

            '''
            ''' PHASE II: DO EUALIT CHECKING
            ''' 

            Try
                ''' get the existing value
                Dim anExistingValue As Object = Me.GetValue(entryname)
                Dim aConvertedvalue As Object

                ''' doe the checks
                If anExistingValue Is Nothing AndAlso value Is Nothing Then
                    Return True
                ElseIf anExistingValue Is Nothing AndAlso value IsNot Nothing Then
                    Return False
                ElseIf anExistingValue IsNot Nothing AndAlso value Is Nothing Then
                    Return False
                ElseIf anExistingValue IsNot Nothing AndAlso value IsNot Nothing Then

                    If anExistingValue.GetType.IsValueType AndAlso value.GetType.IsValueType Then
                        aConvertedvalue = Convert.ChangeType(value, ot.GetDatatypeMappingOf(aDatatype))
                        Return anExistingValue.Equals(aConvertedvalue)
                    ElseIf anExistingValue.GetType Is value.GetType Then
                        Return anExistingValue.Equals(value)
                        ''' special case
                    ElseIf value.GetType Is GetType(String) AndAlso anExistingValue.GetType.IsArray Then
                        Return Converter.otString2Array(value).SequenceEqual(anExistingValue)
                        'Return Array.Equals(aConvertedvalue, anExistingValue)
                    ElseIf value.GetType Is GetType(String) AndAlso anExistingValue.GetType.GetInterfaces.Contains(GetType(IList)) Then
                        Return Converter.otString2Array(value).ToList.SequenceEqual(anExistingValue)
                        'aConvertedvalue = Converter.String2Array(value).ToList
                        'Return anExistingValue.Equals(aConvertedvalue) ' list compare
                    ElseIf anExistingValue.GetType.IsEnum Then
                        If value.GetType.Equals(GetType(String)) Then
                            '* transform
                            aConvertedvalue = CTypeDynamic([Enum].Parse(anExistingValue.GetType, value, ignoreCase:=True), anExistingValue.GetType)
                        Else
                            aConvertedvalue = CTypeDynamic(value, anExistingValue.GetType)
                        End If
                        Return anExistingValue.Equals(aConvertedvalue)
                    Else
                        Throw New NotImplementedException("checking")
                        Return False
                    End If

                End If

                Return False
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormDataObject.EqualsValue", arg1:=value, entryname:=entryname, objectname:=Me.ObjectID)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' applies object entry properties, validates and sets a value of a entry/member
        ''' the value might be changed during validation
        ''' raises the propertychanged event
        ''' if it is different to its value
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="member"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetValue(entryname As String, ByVal value As Object) As Boolean Implements iormPersistable.SetValue
            Dim result As Boolean = False
            Dim isnullable As Boolean = False
            Dim anObjectEntry As iormObjectEntry
            Dim oldvalue As Object

            ''' 
            ''' PHASE I : APPLY THE ENTRY PROPERTIES AND TRANSFORM THE VALUE REQUESTED
            ''' 
            If Not Me.NormalizeValue(entryname, value) Then
                CoreMessageHandler(message:="Warning ! Could not normalize value", arg1:=value, objectname:=Me.ObjectID, _
                                    entryname:=entryname, subname:="ormDataObject.SetValue")
            End If

            '''
            ''' PHASE II: DO VALIDATION
            ''' 

            Try
                ''' validate the new value
                ''' 
                Dim aValidateResult As otValidationResultType = Validate(entryname, value)

                '** Validate against the ObjectEntry Rules
                If aValidateResult = otValidationResultType.Succeeded Or aValidateResult = otValidationResultType.FailedButProceed Then

                    ''' get the description
                    Dim aClassDescription = Me.ObjectClassDescription 'ot.GetObjectClassDescription(Me.GetType)
                    If aClassDescription Is Nothing Then
                        CoreMessageHandler(message:=" Object's Class Description could not be retrieved - object not defined ?!", arg1:=value, _
                                          objectname:=Me.ObjectID, entryname:=entryname, _
                                           messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                        Return False
                    End If

                    ''' decide if compound or columnentry - on compound then branch out
                    ''' 
                    ''' 

                    If Not aClassDescription.HasObjectEntryAttribute(entryname) Then
                        If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                            CoreMessageHandler(message:="Object entry does not exist in object class description - on bootstrap installation or session startup compounds are not allowed to be used. ", _
                                           arg1:=entryname, objectname:=Me.ObjectID, entryname:=entryname, _
                                           messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                            Return False
                        Else
                            If Me.ObjectDefinition.HasEntry(entryname) Then
                                anObjectEntry = Me.ObjectDefinition.GetEntry(entryname)
                                '''
                                ''' branch out to set the compound value
                                ''' 
                                If anObjectEntry.IsCompound Then
                                    Return SetCompoundValue(entryname, value)
                                Else
                                    CoreMessageHandler(message:="Object entry does not exist in object class description as column entry but is also not a compound in the object definition ?!", arg1:=entryname, _
                                                       objectname:=Me.ObjectID, entryname:=entryname, _
                                                       messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                                    Return False

                                End If

                            Else
                                CoreMessageHandler(message:="Object entry does not exist in object definition", arg1:=entryname, _
                                          objectname:=Me.ObjectID, entryname:=entryname, _
                                           messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                                Return False
                            End If
                        End If
                    End If


                    ''' get the fieldinfos of the entry
                    ''' 
                    Dim afieldinfos = aClassDescription.GetEntryFieldInfos(entryname)
                    If afieldinfos.Count = 0 Then
                        ' might be by intention
                        'CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to a class field member or the entry name is not valid", arg1:=value, _
                        '                   objectname:=Me.ObjectID, entryname:=entryname, _
                        '                    messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                    End If


                    ''' do checks depending on the session status
                    ''' 
                    If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                        ''' get the entry attribute
                        Dim anEntryAttribute = aClassDescription.GetObjectEntryAttribute(entryname)
                        If anEntryAttribute Is Nothing Then
                            CoreMessageHandler(message:="object entry attribute couldnot be retrieved from class description", arg1:=value, _
                                               objectname:=Me.ObjectID, entryname:=entryname, _
                                                messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                            Return False
                        End If
                        ''' get the readonly
                        If anEntryAttribute.HasValueIsReadonly AndAlso anEntryAttribute.IsReadOnly Then
                            Return True ' fake it
                        End If
                        If anEntryAttribute.HasValueIsNullable Then
                            isnullable = anEntryAttribute.IsNullable
                        End If
                    Else
                        Dim anEntry As iormObjectEntry = Me.ObjectDefinition.GetEntry(entryname:=entryname)
                        If anEntry.IsReadonly Then
                            Return True ' fake it
                        End If
                        '' take nullable
                        isnullable = anEntry.IsNullable
                    End If

                    ''' get old values
                    ''' and set the new values if different
                    ''' 
                    For Each field In afieldinfos
                        oldvalue = Nothing
                        If Not Reflector.GetFieldValue(field:=field, dataobject:=Me, value:=oldvalue) Then
                            CoreMessageHandler(message:="field value of data object could not be retrieved by getvalue", _
                                                objectname:=Me.ObjectID, subname:="ormDataObject.setValue", _
                                                messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.PrimaryTableID)
                            Return False
                        End If

                        '*** if different value
                        If (oldvalue IsNot Nothing AndAlso value Is Nothing AndAlso isnullable) _
                            OrElse (oldvalue Is Nothing AndAlso value IsNot Nothing AndAlso isnullable) _
                            OrElse (value IsNot Nothing AndAlso Not value.Equals(oldvalue)) Then
                            '' raise event
                            Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=entryname, value:=value)
                            RaiseEvent OnEntryChanging(Me, e:=args)
                            If args.Proceed Then
                                'If args.Result Then value = args.Value possible but should not be done since validation 

                                '' reflector set
                                If Not Reflector.SetFieldValue(field:=field, dataobject:=Me, value:=value) Then
                                    CoreMessageHandler(message:="field value of data object could not be set", _
                                                        objectname:=Me.ObjectID, subname:="ormDataObject.setValue", _
                                                        messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.PrimaryTableID)
                                    Return False
                                End If
                            End If
                            result = args.Proceed
                        ElseIf (Not isnullable AndAlso value Is Nothing) Then
                            CoreMessageHandler(message:="field value is nothing although no nullable allowed", _
                                                    objectname:=Me.ObjectID, subname:="ormDataObject.setValue", _
                                                    messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.PrimaryTableID)
                            Return False
                        Else
                            Return True 'no difference no change but report everything is fine
                        End If

                    Next

                    ''' raise events
                    ''' 
                    If result Then
                        Me.IsChanged = True
                        Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=entryname, value:=value)
                        RaiseEvent OnEntryChanged(Me, e:=args)
                        RaiseObjectEntryChanged(entryname)
                    End If

                    Return result
                End If

                Return False

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormDataObject.setvalue", arg1:=value, entryname:=entryname, objectname:=Me.ObjectID)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Merge Values of an record in own record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns>True if successfull </returns>
        ''' <remarks></remarks>
        Private Function MergeRecord(record As ormRecord) As Boolean
            If _record Is Nothing Then
                _record = record
                Return True
            Else
                For Each key In record.Keys
                    If (_record.IsBound AndAlso _record.HasIndex(key)) OrElse Not _record.IsBound Then Me._record.SetValue(key, record.GetValue(key))
                Next
                ' take over also the status if we have none
                If Not _record.IsLoaded AndAlso Not _record.IsCreated AndAlso (record.IsCreated OrElse record.IsLoaded) Then _record.IsLoaded = record.IsLoaded

                Return True
            End If
        End Function

        ''' <summary>
        ''' sets the Livecycle status of this object if created or loaded
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DetermineLiveStatus() As Boolean Implements iormPersistable.DetermineLiveStatus
            ''' check the record again -> if infused by a record by sql selectment if have nor created not loaded
            If Me.IsInitialized Then
                '** check on the records
                _isCreated = Me.Record.IsCreated
                _isLoaded = Me.Record.IsLoaded
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' checks if the data object is alive
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsAlive(Optional subname As String = "", Optional throwError As Boolean = True) As Boolean Implements iormPersistable.IsAlive
            If Not Me.IsLoaded And Not Me.IsCreated Then
                DetermineLiveStatus()
                '** check again
                If Not Me.IsLoaded And Not Me.IsCreated Then
                    If throwError Then
                        If Not subname.Contains("."c) Then subname = Me.GetType.Name & "." & subname
                        If subname = "" Then subname = "ormDataObject.checkalive"

                        CoreMessageHandler(message:="object is not alive but operation requested", objectname:=Me.GetType.Name, _
                                           subname:=subname, tablename:=Me.PrimaryTableID, messagetype:=otCoreMessageType.InternalError)
                    End If
                    Return False
                End If
            End If

            ''' success
            Return True
        End Function
        ''' <summary>
        ''' initialize the data object
        ''' </summary>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overridable Function Initialize(Optional runtimeOnly As Boolean = False) As Boolean Implements iormPersistable.Initialize


            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be initialized - start session to database first", _
                                           objectname:=Me.ObjectID, subname:="ormDataobject.initialize", _
                                           messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End If

            ''' set the runtime flag 
            _RunTimeOnly = runtimeOnly

            ''' set the properties which are not initializing by themselves
            ''' 


            ''' fire event
            ''' 
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, usecache:=Me.UseCache, runtimeOnly:=runtimeOnly)
            RaiseEvent OnInitializing(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return False
            End If

            ''' set the return value
            Initialize = True

            ''' get new  record if necessary
            ''' STILL we rely on One Table for the Record
            If _record Is Nothing Then
                _record = New ormRecord(Me.TableIDs, dbdriver:=_defaultdbdriver, runtimeOnly:=runtimeOnly)
                'now we are not runtime only anymore -> set also the table and let's have a fixed structure
            ElseIf Not Me.RunTimeOnly Then
                _record.SetTables(Me.TableIDs, dbdriver:=_defaultdbdriver)
            End If

            ''' run on checks
            If Not _record.IsBound AndAlso Not Me.RunTimeOnly Then
                Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="record is not set to table definition", _
                                        messagetype:=otCoreMessageType.InternalError, tablename:=Me.PrimaryTableID, noOtdbAvailable:=True)
                Initialize = False
            End If

            '*** check on connected status if not on runtime
            If Not Me.RunTimeOnly Then
                If _record.TableStores IsNot Nothing Then
                    For Each aTablestore In _record.TableStores
                        If Not aTablestore Is Nothing AndAlso Not aTablestore.Connection Is Nothing Then
                            If Not aTablestore.Connection.IsConnected AndAlso Not aTablestore.Connection.Session.IsBootstrappingInstallationRequested Then
                                Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="TableStore is not connected to database / no connection available", _
                                                        messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                                Initialize = False
                            End If
                        End If
                    Next
                Else
                    Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="TableStore is nothing in record", _
                                                       messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                    Initialize = False
                End If

            End If

            '* default values
            '_updatedOn = ConstNullDate is nullable 
            '_createdOn = ConstNullDate is nullable 
            '_deletedOn = ConstNullDate is nullable 
            _IsDeleted = False

            '** fire event
            ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, usecache:=Me.UseCache, runtimeOnly:=runtimeOnly)
            ourEventArgs.Proceed = Initialize
            RaiseEvent OnInitialized(Me, ourEventArgs)
            '** set initialized
            _IsInitialized = ourEventArgs.Proceed
            Return Initialize
        End Function
        ''' <summary>
        ''' load DataObject by Type and Primary Key-Array
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function InjectDataObject(pkArray() As Object, type As System.Type, _
                                                                     Optional domainid As String = Nothing, _
                                                                     Optional dbdriver As iormDatabaseDriver = Nothing) As iormPersistable
            Dim aDataObject As iormPersistable = ot.CreateDataObjectInstance(type)

            If aDataObject.Inject(pkArray, domainID:=domainID, dbdriver:=dbdriver) Then
                Return aDataObject
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' injects a iormpersistable DataObject by Type and Primary Key-Array
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function InjectDataObject(Of T As {iormInfusable, iormPersistable, New})(pkArray() As Object, _
                                                                                               Optional domainid As String = Nothing, _
                                                                                               Optional dbdriver As iormDatabaseDriver = Nothing) As T
            Return InjectDataObject(pkArray:=pkArray, type:=GetType(T), domainID:=domainID, dbdriver:=dbdriver)
        End Function
        ''' <summary>
        ''' loads and infuse the deliverable by primary key from the data store
        ''' </summary>
        ''' <param name="UID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Inject(ByRef pkArray() As Object, _
                                           Optional domainid As String = Nothing, _
                                           Optional dbdriver As iormDatabaseDriver = Nothing, _
                                           Optional loadDeleted As Boolean = False) As Boolean Implements iormPersistable.Inject
            Dim aRecord As ormRecord
            Dim aStore As iormDataStore

            '* init
            If Not Me.IsInitialized AndAlso Not Me.Initialize Then
                Return False
            End If
            '** check on the operation right for this object
            If Not RunTimeOnly AndAlso Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                objecttransactions:={Me.ObjectID & "." & ConstOPInject}) Then
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, _
                                                        domainID:=domainID, _
                                                        username:=CurrentSession.Username, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                        objecttransactions:={Me.ObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be injected - permission denied to user", _
                                            objectname:=Me.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                            subname:="ormDataObject.Inject", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            Try
                _RunTimeOnly = False

                ''' fix the primary key
                Shuffle.ChecknFixPimaryKey(Me.ObjectID, pkarray:=pkArray, domainid:=domainID, runtimeOnly:=RunTimeOnly)

                '** fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=aRecord, pkarray:=pkArray, infusemode:=otInfuseMode.OnInject, runtimeOnly:=Me.RunTimeOnly)
                ourEventArgs.UseCache = Me.UseCache
                RaiseEvent OnInjecting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Me.Record = ourEventArgs.Record
                    End If
                    '** reset the infuse mode
                    Return ourEventArgs.Result
                Else
                    pkArray = ourEventArgs.Pkarray
                    aRecord = ourEventArgs.Record
                End If

                ''' load from table store if we do not have it
                ''' IMPORTANT ! Still we need the primary Table as only source for the object
                ''' 
                If aRecord Is Nothing Then
                    '''
                    ''' TO DO: If we have multiple Tables - get a concept for the primary and
                    ''' load the all, merge the records to one
                    If dbdriver Is Nothing Then
                        aStore = Me.PrimaryTableStore
                    Else
                        aStore = dbdriver.GetTableStore(Me.PrimaryTableID)
                    End If

                    '''
                    ''' load the record from the store
                    ''' 
                    aRecord = aStore.GetRecordByPrimaryKey(pkArray)

                End If

                '* still nothing ?!
                If aRecord Is Nothing Then
                    _isLoaded = False
                    Return False
                Else
                    '* what about deleted objects
                    If Me.ObjectHasDeletePerFlagBehavior Then
                        If aRecord.HasIndex(ConstFNIsDeleted) Then
                            If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                _IsDeleted = True
                                '* load only on deleted
                                If Not loadDeleted Then
                                    _isLoaded = False
                                    _isCreated = False
                                    Return False
                                End If
                            Else
                                _IsDeleted = False
                            End If
                        Else
                            CoreMessageHandler(message:="object has delete per flag behavior but no flag", messagetype:=otCoreMessageType.InternalError, _
                                                subname:="ormDataObject.Inject", tablename:=Me.PrimaryTableID, entryname:=ConstFNIsDeleted)
                            _IsDeleted = False
                        End If
                    Else
                        _IsDeleted = False
                    End If

                    ''' INFUSE THE OBJECT from the record
                    ''' 
                    Dim anewDataobject As iormPersistable = Me
                    '** reset flags
                    If InfuseDataObject(record:=aRecord, dataobject:=anewDataobject, mode:=otInfuseMode.OnInject) Then
                        If Me.Guid <> anewDataobject.Guid Then
                            CoreMessageHandler(message:="object was substituted during infuse", messagetype:=otCoreMessageType.InternalError, _
                                                subname:="ormDataObject.Inject", tablename:=Me.PrimaryTableID, objectname:=Me.ObjectID)
                            Return False
                        End If

                        _isCreated = False
                        _isLoaded = True
                        _IsChanged = False
                        '** set the primary keys
                        _primaryKeyValues = pkArray
                    End If

                    '** fire event
                    ourEventArgs = New ormDataObjectEventArgs(anewDataobject, record:=Me.Record, pkarray:=pkArray, infuseMode:=otInfuseMode.OnInject, runtimeOnly:=Me.RunTimeOnly)
                    ourEventArgs.Proceed = Me.IsLoaded
                    ourEventArgs.UseCache = Me.UseCache
                    RaiseEvent OnInjected(Me, ourEventArgs)
                    _isLoaded = ourEventArgs.Proceed

                    '** return
                    Return Me.IsLoaded
                End If

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.Inject", arg1:=pkArray, tablename:=_primaryTableID)
                Return False
            End Try


        End Function

        ''' <summary>
        ''' Persist the object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overridable Function Persist(Optional timestamp As DateTime? = Nothing, Optional doFeedRecord As Boolean = True) As Boolean Implements iormPersistable.Persist

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
                                 subname:="ormDataObject.Persist", dataobject:=Me)
                Return False
            End If

            '''
            ''' record must be alive
            ''' 
            If Not Me.Record.Alive Then
                CoreMessageHandler(message:="record is not alive in data object - cannot persist", messagetype:=otCoreMessageType.InternalError, _
                                   subname:="ormDataObject.Persist", objectname:=Me.ObjectID, tablename:=Me.PrimaryTableID)
                Return False
            End If
            '** check on the operation right for this object
            If Not CurrentSession.IsStartingUp AndAlso _
                Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, objecttransactions:={Me.ObjectID & "." & ConstOPPersist}) Then
                '** authorize
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, _
                                                    messagetext:="Please provide another user to authorize requested operation", _
                                                    username:=CurrentSession.Username, loginOnFailed:=True, _
                                                    objecttransactions:={Me.ObjectID & "." & ConstOPPersist}) Then
                    Call CoreMessageHandler(message:="data object cannot be persisted - permission denied to user", _
                                            objectname:=Me.ObjectID, arg1:=ConstOPPersist, username:=CurrentSession.Username, _
                                            subname:="ormDataObject.Persist", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If
            '**
            Try
                '* if object was deleted an its now repersisted
                Dim isdeleted As Boolean = _IsDeleted
                _IsDeleted = False

                '** fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=Me.PrimaryKeyValues, _
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
                                        subname:="ormDataObject.Persist", arg1:=Converter.Array2StringList(Me.PrimaryKeyValues), objectname:=Me.ObjectID, _
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
                    CoreMessageHandler("data object could not persist", dataobject:=Me, subname:="ormDataObject.Persist", messagetype:=otCoreMessageType.InternalError)
                    Persist = False
                Else
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
                    _isLoaded = True
                    _IsDeleted = False
                Else
                    _IsDeleted = isdeleted
                End If


                '** fire event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=Me.PrimaryKeyValues, _
                                                               timestamp:=timestamp, usecache:=Me.UseCache, domainID:=DomainID, _
                                                               domainBehavior:=Me.ObjectHasDomainBehavior, runtimeOnly:=Me.RunTimeOnly)
                RaiseEvent OnPersisted(Me, ourEventArgs)
                Persist = ourEventArgs.Proceed And Persist

                RaiseEvent ClassOnPersisted(Me, ourEventArgs)
                Persist = ourEventArgs.Proceed And Persist

                Return Persist

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Persist")
                Return False
            End Try
        End Function


        ''' <summary>
        ''' returns the Version number of the Attribute set Persistance Version
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassVersion(dataobject As iormPersistable, Optional name As String = "") As Long Implements iormPersistable.GetObjectClassVersion
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = (dataobject.GetType).GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each Const Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attribtes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            '** TABLE
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) AndAlso name = "" Then
                                '** Schema Definition
                                Return (DirectCast(anAttribute, ormSchemaTableAttribute).Version)

                                '** FIELD COLUMN
                            ElseIf anAttribute.GetType().Equals(GetType(ormObjectEntryAttribute)) AndAlso name <> " " Then
                                If name.ToLower = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                    Return DirectCast(anAttribute, ormObjectEntryAttribute).Version
                                End If

                                '** INDEX
                            ElseIf anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                If name.ToLower = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                    Return DirectCast(anAttribute, ormSchemaIndexAttribute).Version
                                End If

                            End If

                        Next
                    End If
                Next


            Catch ex As Exception

                Call CoreMessageHandler(subname:="ormDataObject.GetVersion(of T)", exception:=ex)
                Return False

            End Try
        End Function



        ''' <summary>
        ''' shared create the schema for this object by reflection
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObjectSchema(Of T)(Optional silent As Boolean = False, Optional dbdriver As iormDatabaseDriver = Nothing) As Boolean
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
        ''' Helper for Adding Handlers to SwitchRuntimeOff Event
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <remarks></remarks>
        Public Sub AddSwitchRuntimeOffhandler(handler As [Delegate])
            AddHandler Me.OnSwitchRuntimeOff, handler
        End Sub
        ''' <summary>
        ''' Switch off the Runtime Mode
        ''' </summary>
        ''' <remarks></remarks>
        Public Function SwitchRuntimeOff() As Boolean
            If _RunTimeOnly Then
                Dim ourEventArgs As New ormDataObjectEventArgs(Me)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = True
                RaiseEvent OnSwitchRuntimeOff(Me, ourEventArgs)
                '** no
                If Not ourEventArgs.Proceed Then Return ourEventArgs.Result
                '** proceed
                _RunTimeOnly = Not Me.Initialize(runtimeOnly:=False)
                Return Not _RunTimeOnly
            End If
            Return True
        End Function
        ''' <summary>
        ''' set the dataobject to Runtime
        ''' </summary>
        ''' <remarks></remarks>
        Private Function SwitchRuntimeON() As Boolean
            If Not _RunTimeOnly Then
                Dim ourEventArgs As New ormDataObjectEventArgs(Me)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = True
                RaiseEvent OnSwitchRuntimeOn(Me, ourEventArgs)
                '** no
                If Not ourEventArgs.Proceed Then Return ourEventArgs.Result
                _RunTimeOnly = True
            End If

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
        Public Shared Function CreateDataObject(ByRef pkArray() As Object, type As System.Type, _
                                 Optional domainID As String = Nothing,
                                 Optional checkUnique As Boolean? = Nothing, _
                                 Optional runtimeOnly As Boolean? = Nothing) As iormPersistable

            Dim aDataobject As iormPersistable = ot.CreateDataObjectInstance(type)
            ''' defautl values
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            ''' Substitute the DomainID if necessary
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            ''' fix the primary key
            Shuffle.ChecknFixPimaryKey(aDataobject.ObjectID, domainid:=domainID, pkarray:=pkArray, runtimeOnly:=runtimeOnly)

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=TryCast(aDataobject, ormDataObject), _
                                                           record:=aDataobject.Record, _
                                                           pkarray:=pkArray, _
                                                           usecache:=aDataobject.useCache, _
                                                           runtimeonly:=runtimeOnly)
            RaiseEvent ClassOnCreating(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    Return ourEventArgs.DataObject
                Else
                    Return Nothing
                End If
            Else
                pkArray = ourEventArgs.Pkarray
            End If

            If aDataobject.Create(pkArray, domainID:=domainID, runTimeonly:=runtimeOnly, checkUnique:=checkUnique) Then
                '** fire event
                ourEventArgs = New ormDataObjectEventArgs([object]:=TryCast(aDataobject, ormDataObject), _
                                                               record:=aDataobject.Record, _
                                                               pkarray:=pkArray, _
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
        ''' create a persistable dataobject of type T 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <param name="checkUnique"></param>
        ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
        ''' <remarks></remarks>

        Public Shared Function CreateDataObject(Of T As {iormInfusable, iormPersistable, New}) _
                            (ByRef pkArray() As Object,
                             Optional domainID As String = Nothing,
                             Optional checkUnique As Boolean? = Nothing, _
                             Optional runtimeOnly As Boolean? = Nothing) As iormPersistable
            Dim aDataObject As New T

            ''' defautl values
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False

            ''' fix primary key
            Shuffle.ChecknFixPimaryKey(aDataObject.ObjectID, domainid:=domainID, pkarray:=pkArray, runtimeOnly:=runtimeOnly)

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormDataObject), _
                                                           record:=aDataObject.Record, _
                                                           pkarray:=pkArray, _
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
                pkArray = ourEventArgs.Pkarray
            End If

            If aDataObject.Create(pkArray, domainID:=domainID, runTimeonly:=runtimeOnly, checkUnique:=checkUnique) Then
                '** fire event
                pkArray = ExtractPrimaryKey(aDataObject.Record, aDataObject.ObjectID, runtimeOnly:=aDataObject.RuntimeOnly)
                ourEventArgs = New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormDataObject), _
                                                               record:=aDataObject.Record, _
                                                               pkarray:=pkArray, _
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
                Return aDataObject
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' create a persistable dataobject of type T out of data of a record
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="pkArray"></param>
        ''' <param name="checkUnique"></param>
        ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObject(Of T As {iormInfusable, iormPersistable, New}) _
                            (ByRef record As ormRecord,
                             Optional domainID As String = Nothing,
                             Optional checkUnique As Boolean? = Nothing, _
                             Optional runtimeOnly As Boolean? = Nothing) As iormPersistable
            Dim aDataObject As New T
            ''' defautl values
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            ''' Get the Primary key
            Dim pkarray As Object() = ExtractPrimaryKey(record:=record, objectID:=aDataObject.ObjectID, runtimeOnly:=runtimeOnly)
            ''' Substitute the DomainID if necessary
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID

            ''' fix primary key
            Shuffle.ChecknFixPimaryKey(aDataObject.ObjectID, domainid:=domainID, pkarray:=pkarray, runtimeOnly:=runtimeOnly)

            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormDataObject), _
                                                           record:=record, _
                                                           pkarray:=pkarray, _
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
                ourEventArgs = New ormDataObjectEventArgs([object]:=TryCast(aDataObject, ormDataObject), _
                                                               record:=record, _
                                                               pkarray:=ExtractPrimaryKey(record:=record, objectID:=aDataObject.ObjectID, runtimeOnly:=runtimeOnly), _
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
        ''' copy the Primary key to the record
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <remarks></remarks>
        Private Function CopyPrimaryKeyToRecord(ByRef pkArray() As Object, ByRef record As ormRecord,
                                                Optional domainid As String = Nothing, _
                                                Optional runtimeOnly As Boolean = False) As Boolean
            Dim aList As List(Of String)
            If Not runtimeOnly Then
                aList = Me.TableSchema.PrimaryKeys 'take it from the real schema
            Else
                Dim aDescriptor As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                If aDescriptor IsNot Nothing Then
                    aList = aDescriptor.PrimaryKeyEntryNames.ToList
                Else
                    CoreMessageHandler(message:="no object class description found", objectname:=Me.ObjectID, subname:="ormDataObject.CopyPrimaryKeyToRecord", _
                                       messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If
            Dim i As UShort = 0
            If String.IsNullOrWhiteSpace(domainID) Then domainID = ConstGlobalDomain
            ReDim Preserve pkArray(aList.Count - 1)
            For Each acolumnname In aList
                If (record.IsBound AndAlso record.HasIndex(acolumnname)) OrElse Not record.IsBound Then
                    If acolumnname IsNot Nothing Then
                        If acolumnname.ToUpper <> Domain.ConstFNDomainID Then
                            record.SetValue(acolumnname, pkArray(i))
                        Else
                            If pkArray(i) Is Nothing OrElse pkArray(i) = "" Then
                                record.SetValue(acolumnname, domainID)
                            Else
                                record.SetValue(acolumnname, pkArray(i))
                            End If
                        End If

                    End If


                Else
                    CoreMessageHandler(message:="record index not found", objectname:=Me.ObjectID, subname:="ormDataObject.CopyPrimaryKeyToRecord", _
                                       entryname:=acolumnname, messagetype:=otCoreMessageType.InternalError)
                End If
                i = i + 1
            Next

            Return True
        End Function

        ''' <summary>
        ''' helper for checking the uniqueness during creation
        ''' </summary>
        ''' <param name="pkarray"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CheckUniqueness(pkarray As Object(), record As ormRecord, Optional runtimeOnly As Boolean = False) As Boolean

            '*** Check on Not Runtime
            If Not runtimeOnly OrElse Me.UseCache Then
                Dim aRecord As ormRecord
                '* fire Event and check uniqueness in cache if we have one
                Dim ourEventArgs = New ormDataObjectEventArgs(Me, record:=record, pkarray:=pkarray, usecache:=Me.UseCache, runtimeOnly:=runtimeOnly)
                RaiseEvent ClassOnCheckingUniqueness(Me, ourEventArgs)

                '* skip
                If ourEventArgs.Proceed AndAlso Not runtimeOnly Then
                    ' Check
                    Dim aStore As iormDataStore = Me.PrimaryTableStore
                    aRecord = aStore.GetRecordByPrimaryKey(pkarray)

                    '* not found
                    If aRecord IsNot Nothing Then
                        If Me.ObjectHasDeletePerFlagBehavior Then
                            If aRecord.HasIndex(ConstFNIsDeleted) Then
                                If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                    CoreMessageHandler(message:="deleted (per flag) object found - use undelete instead of create", messagetype:=otCoreMessageType.ApplicationWarning, _
                                                        arg1:=pkarray, tablename:=Me.PrimaryTableID)
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
        ''' extract out of a record a Primary Key array
        ''' </summary>
        ''' <param name="record"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function ExtractPrimaryKey(record As ormRecord, objectID As String,
                                                                                  Optional runtimeOnly As Boolean = False) As Object()
            Dim pknames As String()
            Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(objectID)
            '*** extract the primary keys from record
            If runtimeOnly OrElse anObjectDescription.ObjectAttribute.IsBootstrap Then
                If anObjectDescription IsNot Nothing Then
                    pknames = anObjectDescription.PrimaryKeyEntryNames
                Else
                    CoreMessageHandler(message:="ObjectDescriptor not found", objectname:=objectID, arg1:=objectID, _
                                        subname:="ormDataobject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalError)
                    Return {}
                End If
            ElseIf CurrentSession.IsRunning Or CurrentSession.IsStartingUp Then
                Dim anObjectDefinition = CurrentSession.Objects.GetObject(objectID)
                '* keynames of the object
                pknames = anObjectDefinition.GetKeyNames.ToArray
                If pknames.Count = 0 Then
                    CoreMessageHandler(message:="objectdefinition has not primary keys", objectname:=anObjectDefinition.ObjectID, _
                                   subname:="ormDataObject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalWarning)
                    Return Nothing
                End If
            Else
                CoreMessageHandler(message:="couldnot obtain primary keys for object type", objectname:=objectID, _
                                   subname:="ormDataObject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalWarning)
                Return Nothing
            End If

            '** get the 
            Dim pkarray As Object()
            ReDim pkarray(pknames.Length - 1)
            Dim i As UShort = 0
            For Each aName In pknames
                If record.HasIndex(aName) Then
                    pkarray(i) = record.GetValue(index:=aName)
                    i += 1
                End If
            Next

            Return pkarray
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
                                              Optional runtimeOnly As Boolean? = Nothing) As Boolean Implements iormPersistable.Create

            ''' defautl values
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be created - start session to database first", _
                                           objectname:=Me.ObjectID, arg1:=ConstOPCreate, _
                                           messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '** initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize(runtimeOnly:=runtimeOnly) Then
                Call CoreMessageHandler(message:="dataobject can not be initialized", tablename:=_primaryTableID, arg1:=record.ToString, _
                                        subname:="ormDataObject.create", messagetype:=otCoreMessageType.InternalError)

                Return False
            End If
            '** is the object loaded -> no reinit
            If Me.IsLoaded Then
                Call CoreMessageHandler(message:="data object cannot be created if it has state loaded", objectname:=Me.ObjectID, tablename:=_primaryTableID, arg1:=record.ToString, _
                                        subname:="ormDataObject.create", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            '** check on the operation right for this object
            If Not runtimeOnly AndAlso _
                   Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, domainid:=domainID, _
                                                                objecttransactions:={Me.ObjectID & "." & ConstOPCreate}) Then
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, loginOnFailed:=True, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                        domainID:=domainID, objecttransactions:={Me.ObjectID & "." & ConstOPCreate}) Then
                    Call CoreMessageHandler(message:="data object cannot be created - permission denied to user", _
                                            objectname:=Me.ObjectID, arg1:=ConstOPCreate, _
                                            messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '**
            Dim pkarray As Object()

            '** domainid
            If String.IsNullOrWhiteSpace(domainID) Then domainID = ConstGlobalDomain

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

            '** leave the pkarray extraction here after

            '* extract the primary key
            pkarray = ExtractPrimaryKey(record, objectID:=Me.ObjectID, runtimeOnly:=runtimeOnly)
            '** check for domainBehavior
            Shuffle.ChecknFixPimaryKey(Me.ObjectID, pkarray:=pkarray, domainid:=domainID, runtimeOnly:=runtimeOnly)
            '** keys must be set in the object itself
            '** create
            _UniquenessInStoreWasChecked = Not runtimeOnly And checkUnique ' remember
            If checkUnique AndAlso Not CheckUniqueness(pkarray:=pkarray, record:=record, runtimeOnly:=runtimeOnly) Then
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
                CoreMessageHandler(message:="InfuseDataobject failed", messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.Create")
                If aDataobject.Guid <> Me.Guid Then
                    CoreMessageHandler(message:="data object was substituted in instance create function during infuse ?!", messagetype:=otCoreMessageType.InternalWarning, _
                        subname:="ormDataObject.Create")
                End If
            End If

            '** set status
            _domainID = domainID
            _isCreated = True
            _IsDeleted = False
            _isLoaded = False
            _IsChanged = False

            '* fire Event
            ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, _
                                                      pkarray:=pkarray, _
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
        Protected Overridable Function Create(ByRef pkArray() As Object, _
                                              Optional domainID As String = Nothing, _
                                              Optional checkUnique As Boolean? = Nothing, _
                                              Optional runtimeOnly As Boolean? = Nothing) As Boolean Implements iormPersistable.Create
            ''' defautl values
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not checkUnique.HasValue Then checkUnique = True
            If Not runtimeOnly.HasValue Then runtimeOnly = False

            '*** add the primary keys
            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be created - start session to database first", _
                                          subname:="ormDataObject.create", objectname:=Me.ObjectID, arg1:=ConstOPCreate, _
                                           messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '** initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize(runtimeOnly:=runtimeOnly) Then
                Call CoreMessageHandler(message:="dataobject can not be initialized", tablename:=_primaryTableID, arg1:=Record.ToString, _
                                        subname:="ormDataObject.create", messagetype:=otCoreMessageType.InternalError)

                Return False
            End If

            '** set default
            If String.IsNullOrWhiteSpace(domainID) Then domainID = ConstGlobalDomain

            '** copy the primary keys
            CopyPrimaryKeyToRecord(pkArray:=pkArray, record:=Me.Record, domainID:=domainID, runtimeOnly:=runtimeOnly)

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
        Public Overloads Shared Function Retrieve(Of T As {iormInfusable, ormDataObject, iormPersistable, New}) _
            (pkArray() As Object, _
             Optional domainID As String = Nothing, _
             Optional dbdriver As iormDatabaseDriver = Nothing, _
             Optional forceReload As Boolean? = Nothing, _
             Optional runtimeOnly As Boolean? = Nothing) As T
            Return Retrieve(pkArray:=pkArray, type:=GetType(T), domainID:=domainID, dbdriver:=dbdriver, forceReload:=forceReload, runtimeOnly:=runtimeOnly)
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
            OperationName:="GeneralRetrieveByPrimaryKeys", Tag:=ObjectClassDescription.ConstMTRetrieve, TransactionID:=ConstOPRetrieve)> _
        Public Overloads Shared Function Retrieve(pkArray() As Object, type As System.Type, _
                 Optional domainID As String = Nothing, _
                 Optional dbdriver As iormDatabaseDriver = Nothing, _
                 Optional forceReload As Boolean? = Nothing, _
                 Optional runtimeOnly As Boolean? = Nothing) As iormPersistable

            Dim useCache As Boolean = True
            If String.IsNullOrWhiteSpace(domainID) Then domainID = CurrentSession.CurrentDomainID
            If Not runtimeOnly.HasValue Then runtimeOnly = False
            If Not forceReload.HasValue Then forceReload = False
            Dim anObject As iormPersistable = ot.CreateDataObjectInstance(type)


            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be retrieved - start session to database first", _
                                        objectname:=anObject.ObjectID, _
                                        subname:="ormDataObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
            If Not runtimeOnly AndAlso Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObject.ObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                '** request authorizartion
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainID:=domainID, _
                                                                            username:=CurrentSession.Username, _
                                                                            objecttransactions:={anObject.ObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                            objectname:=anObject.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                            subname:="ormDataObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If



            '** use Cache ?!
            useCache = anObject.useCache
            Dim hasDomainBehavior As Boolean = anObject.ObjectHasDomainBehavior
            Dim aObjectID As String = anObject.ObjectID

            ''' fix primary key
            ''' 
            Shuffle.ChecknFixPimaryKey(aObjectID, pkarray:=pkArray, domainid:=domainID, runtimeOnly:=runtimeOnly)

            '* fire event
            Dim ourEventArgs As New ormDataObjectEventArgs(anObject, domainID:=domainID, domainBehavior:=hasDomainBehavior, pkArray:=pkArray, usecache:=useCache)
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
                    Shuffle.ChecknFixPimaryKey(aObjectID, pkarray:=pkArray, domainid:=ConstGlobalDomain, runtimeOnly:=runtimeOnly)
                    '* fire event again
                    ourEventArgs = New ormDataObjectEventArgs(anObject, domainID:=domainID, domainBehavior:=hasDomainBehavior, pkArray:=pkArray)
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
                anObject = ormDataObject.InjectDataObject(pkArray:=pkArray, type:=type, domainID:=domainID, dbdriver:=dbdriver)
                If anObject Is Nothing AndAlso hasDomainBehavior AndAlso domainID <> ConstGlobalDomain Then
                    '* on domain behavior ? -> reload from  the global domain
                    Dim domainPKArray As Object() = pkArray.Clone
                    Shuffle.ChecknFixPimaryKey(aObjectID, pkarray:=pkArray, domainid:=ConstGlobalDomain, substitueOnlyNothingDomain:=False, runtimeOnly:=runtimeOnly)
                    anObject = ormDataObject.Retrieve(pkArray:=pkArray, type:=type, domainID:=ConstGlobalDomain, dbdriver:=dbdriver)
                    ''' add it to cache
                    If anObject IsNot Nothing Then
                        RaiseEvent ClassOnOverloaded(Nothing, _
                                                      New ormDataObjectOverloadedEventArgs(globalPKarray:=pkArray, domainPKArray:=domainPKArray, dataobject:=anObject))
                    End If
                End If
            End If

            '* fire event
            If anObject IsNot Nothing Then
                ourEventArgs = New ormDataObjectEventArgs(anObject, record:=anObject.Record, pkarray:=pkArray, usecache:=useCache)
            Else
                ourEventArgs = New ormDataObjectEventArgs(Nothing, record:=Nothing, pkarray:=pkArray, usecache:=useCache)
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
        Public Shared Function CloneDataObject(Of T As {iormPersistable, iormCloneable, iormInfusable, New})(cloneobject As iormCloneable(Of T), newpkarray As Object()) As T
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
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <remarks></remarks>
        ''' <returns>the new cloned object or nothing</returns>
        Public Overloads Function Clone(Of T As {iormPersistable, iormInfusable, Class, New})(newpkarray As Object(), _
                                                                                                    Optional runtimeOnly As Boolean? = Nothing) As T
            '
            '*** now we copy the object
            Dim aNewObject As New T
            Dim newRecord As New ormRecord
            If Not runtimeOnly.HasValue Then runtimeOnly = Me.RunTimeOnly

            '**
            If Not Me.IsAlive(subname:="clone") Then Return Nothing


            '* fire class event
            Dim ourEventArgs As New ormDataObjectCloneEventArgs(newObject:=TryCast(aNewObject, ormDataObject), oldObject:=Me)
            ourEventArgs.UseCache = Me.UseCache
            RaiseEvent ClassOnCloning(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    If ourEventArgs.DataObject IsNot Nothing Then
                        Return TryCast(ourEventArgs.DataObject, T)
                    Else
                        CoreMessageHandler(message:="ClassOnCloning: cannot convert persistable to class", arg1:=GetType(T).Name, subname:="ormDataObject.Clone(of T)", _
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
            If Not runtimeOnly Then newRecord.SetTable(Me.PrimaryTableID)

            ' go through the table and overwrite the Record if the rights are there
            For Each entryname In Me.Record.Keys
                If entryname <> ConstFNCreatedOn And entryname <> ConstFNUpdatedOn _
                    And entryname <> ConstFNIsDeleted And entryname <> ConstFNDeletedOn _
                    And entryname <> ConstFNIsDomainIgnored Then

                    Call newRecord.SetValue(entryname, Me.Record.GetValue(entryname))
                End If
            Next entryname

            ''' copy the new primary keys
            Me.CopyPrimaryKeyToRecord(newpkarray, newRecord, runtimeOnly:=Me.RunTimeOnly)

            ''' create the new object with the record
            ''' 
            If Not aNewObject.Create(record:=newRecord, checkUnique:=True) Then
                Call CoreMessageHandler(message:="object new keys are not unique - clone aborted", arg1:=newpkarray, tablename:=_primaryTableID, _
                                       messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            '** Fire Event
            ourEventArgs = New ormDataObjectCloneEventArgs(newObject:=TryCast(aNewObject, ormDataObject), oldObject:=Me)

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
        ''' request to load the relations and infuses the values in the mapped members
        ''' </summary>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InfuseRelation(id As String, Optional force As Boolean = False) As Boolean Implements iormInfusable.InfuseRelation
            If Not Me.IsInitialized Then
                If Not Me.Initialize Then
                    Return False
                End If
            End If

            Try
                If Not Me.IsAlive(subname:="InfuseRelation") Then Return False
                Dim result As Boolean = InfuseRelationMapped(mode:=otInfuseMode.OnDemand, relationid:=id, force:=force)
                Return result

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormDataObject.infuseRelation")
                Return False
            End Try
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
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=Me.PrimaryKeyValues, runtimeOnly:=Me.RunTimeOnly)
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
                                                          pkarray:=Me.ExtractPrimaryKey(record:=Me.Record, objectID:=Me.ObjectID, runtimeOnly:=Me.RunTimeOnly), _
                                                           runtimeOnly:=Me.RunTimeOnly, usecache:=Me.UseCache)
                ourEventArgs.Result = True
                ourEventArgs.Proceed = True
                RaiseEvent OnUnDeleted(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                End If
                If ourEventArgs.Result Then
                    CoreMessageHandler(message:="data object undeleted", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                        tablename:=Me.PrimaryTableID)
                    Return True
                Else
                    CoreMessageHandler(message:="data object cannot be undeleted by event - delete per flag behavior not set", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                     tablename:=Me.PrimaryTableID)
                    Return False
                End If

            Else
                CoreMessageHandler(message:="data object cannot be undeleted - delete per flag behavior not set", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                     tablename:=Me.PrimaryTableID)
                Return False
            End If


        End Function
        ''' <summary>
        ''' Delete the object and its persistancy
        ''' </summary>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overridable Function Delete(Optional timestamp As DateTime? = Nothing) As Boolean Implements iormPersistable.Delete

            '** initialize -> no error if not alive
            If Not Me.IsAlive(throwError:=False) Then Return False
            If Not timestamp.HasValue OrElse timestamp = constNullDate Then timestamp = DateTime.Now

            '** check on the operation right for this object
            If Not RunTimeOnly AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, _
                                                                               domainid:=DomainID, _
                                                                                objecttransactions:={Me.ObjectID & "." & ConstOPDelete}) Then

                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, username:=CurrentSession.Username, _
                                                        domainid:=DomainID, loginOnFailed:=True, _
                                                         messagetext:="Please provide another user to authorize requested operation", _
                                                         objecttransactions:={Me.ObjectID & "." & ConstOPDelete}) Then
                    Call CoreMessageHandler(message:="data object cannot be deleted - permission denied to user", _
                                            objectname:=Me.ObjectID, arg1:=ConstOPDelete, username:=CurrentSession.Username, _
                                            subname:="ormDataObject.Delete", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '** Fire Event
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=Me.PrimaryKeyValues, _
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
                        Me.Unload()
                        _deletedOn = timestamp
                    End If

                End If

                '** fire Event
                ourEventArgs.Result = _IsDeleted
                RaiseEvent OnDeleted(Me, ourEventArgs)
                RaiseEvent ClassOnDeleted(Me, ourEventArgs)
                Return _IsDeleted
            Else
                CoreMessageHandler("object could not delete  cascaded objected", subname:="ormDataObject.Delete", objectname:=Me.ObjectID, _
                                   arg1:=Converter.Array2StringList(Me.PrimaryKeyValues))
                Return False
            End If

        End Function
        ''' <summary>
        ''' infuse a data objects object entry column mapped members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function InfuseColumnMapping(mode As otInfuseMode) As Boolean
            '** Fire Event
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record, infuseMode:=mode, runtimeOnly:=Me.RunTimeOnly)
            RaiseEvent ClassOnColumnMappingInfusing(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return ourEventArgs.Proceed
            ElseIf ourEventArgs.Result Then
                Me.Record = ourEventArgs.Record
            End If
            Dim objectentryname As String

            '*** infuse each mapped column to member
            '*** if it is in the record
            Try


                For Each aColumnName In Me.ObjectClassDescription.MappedColumnNames
                    Dim aFieldList As List(Of FieldInfo) = Me.ObjectClassDescription.GetMappedColumnFieldInfos(columnname:=aColumnName)

                    For Each aField In aFieldList
                        Dim aMappingAttribute = Me.ObjectClassDescription.GetEntryMappingAttributes(aField.Name)
                        If aMappingAttribute IsNot Nothing AndAlso (mode And aMappingAttribute.InfuseMode) Then
                            objectentryname = aMappingAttribute.EntryName
                            Dim isNull As Boolean
                            Dim aValue As Object

                            If Me.Record.HasIndex(aColumnName) Then
                                'Dim aStopwatch1 As New Diagnostics.Stopwatch
                                'aStopwatch1.Start()
                                '*** set the class internal field
                                aValue = Me.Record.GetValue(aColumnName, isNull:=isNull)

                                ''' check on Default Values on Object level
                                ''' on the OnCreate Infuse
                                If mode = otInfuseMode.OnCreate AndAlso (isNull OrElse aValue Is Nothing) Then
                                    ''' during bootstrapping installation we use just the value from class description
                                    ''' (doesnot matter if runtime or not in this case)
                                    If CurrentSession.IsBootstrappingInstallationRequested Then

                                        ''' only if not nullable we use a default value
                                        If Not Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=objectentryname).IsNullable Then
                                            aValue = Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=objectentryname).DefaultValue
                                        End If
                                    Else
                                        Dim anEntry As iormObjectEntry = Me.ObjectDefinition.GetEntry(entryname:=objectentryname)

                                        ''' only if not nullable we use a default value
                                        If anEntry IsNot Nothing Then
                                            aValue = Me.ObjectEntryDefaultValue(anEntry.Entryname)
                                            isNull = False 'reset for the value setting
                                        Else
                                            CoreMessageHandler(message:="object entry not found in object repository", _
                                                                objectname:=Me.ObjectID, subname:="ormDataObject.InfuseColumnMapping", _
                                                                messagetype:=otCoreMessageType.InternalError, entryname:=objectentryname, tablename:=Me.PrimaryTableID)

                                        End If
                                    End If
                                End If

                                'aStopwatch1.Stop()
                                'Debug.WriteLine(">>>>>> GETVALUE:" & aStopwatch1.ElapsedTicks)
                                'Dim aStopwatch2 As New Diagnostics.Stopwatch
                                'aStopwatch2.Start()

                                ''' set the value
                                ''' 
                                If Not isNull AndAlso aValue IsNot Nothing Then
                                    If Not Reflector.SetFieldValue(field:=aField, dataobject:=Me, value:=aValue) Then
                                        CoreMessageHandler(message:="field value ob data object couldnot be set", _
                                                            objectname:=Me.ObjectID, subname:="ormDataObject.InfuseColumnMapping", _
                                                            messagetype:=otCoreMessageType.InternalError, entryname:=objectentryname, tablename:=Me.PrimaryTableID)
                                    End If

                                End If

                                'aStopwatch2.Stop()
                                'Debug.WriteLine(">>>>>> SETVALUE:" & aStopwatch2.ElapsedTicks)
                            End If
                        End If
                    Next
                Next


                '** Fire Event OnColumnMappingInfused
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, infuseMode:=mode, runtimeOnly:=Me.RunTimeOnly)
                RaiseEvent ClassOnColumnMappingInfused(Me, ourEventArgs)
                Return ourEventArgs.Proceed

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormDataObject.InfuseColumnMapping", exception:=ex, objectname:=Me.ObjectID, _
                                        entryname:=objectentryname, tablename:=Me.PrimaryTableID)
                Return False

            End Try

        End Function

        ''' <summary>
        ''' infuse a data objects objectentry column mapped members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function InfusePrimaryKeys(ByRef dataobject As iormPersistable, ByRef pkarray As Object(), _
                                                  Optional runtimeOnly As Boolean = False) As Boolean
            Dim aList As List(Of String)
            Dim aDescriptor As ObjectClassDescription = dataobject.ObjectClassDescription
            Dim i As UShort = 0
            If aDescriptor Is Nothing Then
                CoreMessageHandler(message:="no object class description found", objectname:=dataobject.ObjectID, subname:="ormDataObject.InfusePrimaryKeys", _
                                   messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            If Not runtimeOnly Then
                Dim atablestore As iormDataStore = ot.GetTableStore(aDescriptor.Tables.First)
                aList = atablestore.TableSchema.PrimaryKeys 'take it from the real schema
            Else
                aList = aDescriptor.PrimaryKeyEntryNames.ToList
            End If

            '*** infuse each mapped column to member
            '*** if it is in the record
            Try
                SyncLock dataobject
                    For Each aColumnName In aList
                        Dim aFieldList As List(Of FieldInfo) = aDescriptor.GetMappedColumnFieldInfos(columnname:=aColumnName)
                        For Each aField In aFieldList
                            Dim aValue As Object = pkarray(i)
                            Reflector.SetFieldValue(field:=aField, dataobject:=dataobject, value:=aValue)
                        Next
                    Next
                End SyncLock

            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormDataObject.InfusePrimaryKeys", exception:=ex, objectname:=dataobject.ObjectID, _
                                        tablename:=dataobject.primaryTableID)
                Return False

            End Try

        End Function

        ''' <summary>
        ''' infuse a data object by a record - use reflection and cache. Substitute data object if it is in cache
        ''' </summary>
        ''' <param name="dataobject"></param>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function InfuseDataObject(ByRef record As ormRecord, ByRef dataobject As iormPersistable, _
                                                Optional mode? As otInfuseMode = otInfuseMode.OnDefault) As Boolean

            If dataobject Is Nothing Then
                CoreMessageHandler(message:="data object must not be nothing", subname:="ormDataObject.InfuseDataObject", _
                                   messagetype:=otCoreMessageType.InternalError, _
                                    tablename:=record.TableIDs.First)
                Return False
            End If
            '** extract primary keys
            Dim pkarray() = ExtractPrimaryKey(record:=record, objectID:=dataobject.ObjectID, runtimeOnly:=dataobject.RuntimeOnly)
            '** Fire Event
            Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, record:=record, pkarray:=pkarray, usecache:=dataobject.useCache, infuseMode:=mode)
            RaiseEvent ClassOnInfusing(dataobject, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    If ourEventArgs.DataObject IsNot Nothing Then dataobject = ourEventArgs.DataObject
                    Return True
                Else
                    Return False
                End If
            End If

            Dim aDescriptor As ObjectClassDescription = dataobject.ObjectClassDescription
            If aDescriptor Is Nothing Then
                CoreMessageHandler(message:="could not retrieve descriptor for business object class from core store", arg1:=dataobject.GetType.Name, _
                                    messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.createSchema")
                Return False
            End If

            '''
            ''' Infuse the instance
            If Not TryCast(dataobject, iormInfusable).Infuse(record:=record, mode:=mode) Then
                Return False
            End If

            '** Fire Event ClassOnInfused
            ourEventArgs = New ormDataObjectEventArgs(dataobject, record:=record, pkarray:=pkarray, usecache:=dataobject.useCache, infuseMode:=mode)

            RaiseEvent ClassOnInfused(dataobject, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                If ourEventArgs.Result Then
                    If ourEventArgs.DataObject IsNot Nothing Then dataobject = ourEventArgs.DataObject
                    Return True
                Else
                    Return False
                End If
            End If

            Return ourEventArgs.Proceed

        End Function

        ''' <summary>
        ''' Feed the record belonging to the data object
        ''' </summary>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Public Function Feed(Optional record As ormRecord = Nothing) As Boolean Implements iormPersistable.Feed

            Dim classdescriptor As ObjectClassDescription = Me.ObjectClassDescription
            Dim result As Boolean = True

            '** defaultvalue
            If record Is Nothing Then record = Me.Record

            '** Fire Class Event
            Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=record, pkarray:=Me.PrimaryKeyValues, usecache:=Me.UseCache)
            RaiseEvent ClassOnFeeding(Nothing, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return ourEventArgs.Result
            Else
                record = ourEventArgs.Record
            End If
            '** Fire Event
            RaiseEvent OnFeeding(Me, ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return ourEventArgs.Result
            Else
                record = ourEventArgs.Record
            End If
            Try

                '*** feed each mapped column to record
                '*** if it is in the record

                For Each aColumnName In classdescriptor.MappedColumnNames
                    Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedColumnFieldInfos(columnname:=aColumnName)
                    For Each aField In aFieldList
                        Dim aMappedAttribute = classdescriptor.GetEntryMappingAttributes(aField.Name)
                        Dim anEntryAttribute = classdescriptor.GetObjectEntryAttribute(aMappedAttribute.EntryName)

                        Dim aValue As Object
                        ''' set the value to the record either if it is bound and the columnname is a member or if it is not bound
                        ''' 
                        If Not record.IsBound OrElse (record.IsBound AndAlso record.HasIndex(aColumnName)) Then
                            If aField.FieldType.IsValueType OrElse aField.FieldType.Equals(GetType(String)) OrElse aField.FieldType.Equals(GetType(Object)) OrElse _
                                aField.FieldType.IsArray OrElse aField.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) Then
                                '** get the value by hook or slooow
                                If Not Reflector.GetFieldValue(field:=aField, dataobject:=Me, value:=aValue) Then
                                    aValue = aField.GetValue(Me)
                                End If

                                '** convert into List
                                If anEntryAttribute.DataType = otDataType.List Then
                                    If aValue IsNot Nothing Then aValue = Converter.Enumerable2otString(aValue)

                                    '* 
                                ElseIf aField.FieldType.IsArray OrElse _
                                    (aField.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) AndAlso Not aField.FieldType.Equals(GetType(String))) Then
                                    CoreMessageHandler(message:="field member is an array or list type but object entry attribute is not list - transfered to list presentation", objectname:=Me.ObjectID, columnname:=aColumnName, _
                                                   arg1:=aField.Name, entryname:=anEntryAttribute.EntryName, messagetype:=otCoreMessageType.InternalWarning, _
                                                   subname:="ormDataobject.feedRecord")
                                    aValue = Converter.Enumerable2otString(aValue)
                                End If
                                '*** set the class internal field
                                record.SetValue(aColumnName, value:=aValue)
                                result = result And True
                            Else
                                CoreMessageHandler(message:="field member is not a value type", objectname:=Me.ObjectID, columnname:=aColumnName, _
                                                    arg1:=aField.Name, entryname:=anEntryAttribute.EntryName, messagetype:=otCoreMessageType.InternalError, _
                                                    subname:="ormDataobject.feedRecord")
                                result = result And False
                            End If

                        End If

                    Next
                Next


                '** Fire Event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=record, pkarray:=_primaryKeyValues, _
                                                          usecache:=Me.UseCache)

                ourEventArgs.Result = result
                RaiseEvent OnFed(Nothing, ourEventArgs)
                result = ourEventArgs.Result

                '** Fire Class Event
                ourEventArgs.Result = result
                RaiseEvent ClassOnFed(Nothing, ourEventArgs)
                Return ourEventArgs.Result

            Catch ex As Exception

                Call CoreMessageHandler(subname:="ormDataObject.FeedRecord", exception:=ex, tablename:=Me.PrimaryTableID, objectname:=Me.ObjectID)
                Return False

            End Try


        End Function
        ''' <summary>
        ''' feed the record from the field of an data object - use reflection of attribute otfieldname
        ''' </summary>
        ''' <param name="dataobject"></param>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FeedRecordDataObject(ByRef dataobject As iormPersistable, ByRef record As ormRecord) As Boolean
            Return dataobject.Feed(record:=record)
        End Function
        ''' <summary>
        ''' infuses a data object by a record
        ''' </summary>
        ''' <param name="Record">a fixed ormRecord with the persistence data</param>
        ''' <returns>true if successful</returns>
        ''' <remarks>might be overwritten by class descendants but make sure that you call mybase.infuse</remarks>
        Private Function Infuse(ByRef record As ormRecord, Optional mode? As otInfuseMode = Nothing) As Boolean Implements iormInfusable.Infuse

            '* lazy init
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then Return False

            Try
                Dim pkArray = ExtractPrimaryKey(record:=record, objectID:=Me.ObjectID, runtimeOnly:=Me.RunTimeOnly)
                '** Fire Event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=record, pkarray:=pkArray, usecache:=Me.UseCache, infusemode:=mode, _
                                                               runtimeOnly:=Me.RunTimeOnly)

                RaiseEvent OnInfusing(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Proceed
                Else
                    record = ourEventArgs.Record
                End If

                ''' merge the record according
                ''' Me.Record = record
                If _record Is Nothing Then
                    Me._record = record
                Else
                    Me.MergeRecord(record)
                End If
                ''' if we have no load nor create state but are infused
                ''' 
                If Not Me.IsLoaded AndAlso Not Me.IsCreated AndAlso (record.IsCreated Or record.IsLoaded) Then
                    _isCreated = record.IsCreated
                    _isLoaded = record.IsLoaded
                    If _isLoaded Then _UniquenessInStoreWasChecked = True ' loaded is always uniqenuess checked
                End If
                '** default mode value
                If Not mode.HasValue Then mode = otInfuseMode.OnDefault

                '*** INFUSE THE COLUMN MAPPED MEMBERS
                Dim aResult As Boolean = InfuseColumnMapping(mode:=mode)

                '*** Fire OnColumnsInfused
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=record, pkarray:=pkArray, infusemode:=mode, runtimeOnly:=Me.RunTimeOnly, usecache:=Me.UseCache)
                RaiseEvent OnColumnsInfused(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Proceed
                End If

                '*** INFUSE THE RELATION MAPPED MEMBERS
                aResult = aResult And InfuseRelationMapped(mode:=mode)
               
                If Not aResult Then
                    Return aResult
                End If

                '** Fire Event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=record, pkarray:=pkArray, infusemode:=mode, runtimeOnly:=Me.RunTimeOnly, usecache:=Me.UseCache)
                RaiseEvent OnInfused(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Proceed
                Else
                    If ourEventArgs.Result Then record = ourEventArgs.Record
                End If

                ''' status
                ''' 
                _isInfused = True
                _InfusionTimeStamp = DateTime.Now

                Return True

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Infuse", _
                                       tablename:=Me.PrimaryTableID, messagetype:=otCoreMessageType.InternalException)
                Return False
            End Try


        End Function

    End Class


End Namespace
