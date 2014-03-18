
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

Namespace OnTrack
    Namespace Database

        
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

            '** record for persistence
            Private _record As New ormRecord
            Protected _primaryTableID As String = ""
            Private _classDescription As ObjectClassDescription
            Private _dbdriver As iormDatabaseDriver
            Protected _IsCreated As Boolean = False
            Protected _IsLoaded As Boolean = False
            Protected _IsChanged As Boolean = False

            Private _CurrentInfuseMode As otInfuseMode = otInfuseMode.OnDefault ' Infuse Mode for the mappings
            'if Object is only kept in Memory (no persist, no Record according to table, no DBDriver necessary, no checkuniqueness)
            Private _RunTimeOnly As Boolean = False

            Protected _IsInitialized As Boolean = False
            Protected _serializeWithHostApplication As Boolean = False
            Protected _IsloadedFromHost As Boolean = False
            Protected _IsSavedToHost As Boolean = False

            '** events
            Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

            '** Lifecycle Events
            Protected Shared Event ClassOnRetrieving(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnRetrieved(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnInjected(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnInjecting(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnInfusing(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnInfused(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnInfusing(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnInfused(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event OnColumnMappingInfusing(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event OnColumnMappingInfused(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnPersisting(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnPersisted(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnPersisting(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnPersisted(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnRecordFeeding(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnRecordFedd(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnRecordFeeding(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnRecordFed(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnUnDeleting(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnUnDeleted(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnDeleting(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnDeleted(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnDeleting(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnDeleted(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnCreating(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnCreated(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnCloning(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnCloned(sender As Object, e As ormDataObjectEventArgs)
            Protected Event OnInitializing(sender As Object, e As ormDataObjectEventArgs)
            Protected Event OnInitialized(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event OnCheckingUniqueness(sender As Object, e As ormDataObjectEventArgs)

            '* Validation Events
            Event ClassOnValidating(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.ClassOnValidating
            Event ClassOnValidated(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.ClassOnValidated
            Event OnValidating(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidating
            Event OnValidated(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidated

            '* relation Events
            Protected Shared Event ClassOnRelationLoading(sender As Object, e As ormDataObjectEventArgs)
            Protected Shared Event ClassOnRelationLoaded(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnRelationLoading(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnRelationLoad(sender As Object, e As ormDataObjectEventArgs)

            '** Events for the Switch from Runtime Mode on to Off
            Public Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            Public Event OnSwitchRuntimeOn(sender As Object, e As ormDataObjectEventArgs)

            'Public Shared Property ConstTableID
            <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                title:="Domain", description:="domain of the business Object", _
                defaultvalue:=ConstGlobalDomain, _
                useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
                foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.Cascade & ")", ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
            Public Const ConstFNDomainID = Domain.ConstFNDomainID

            '** Deleted flag
            <ormObjectEntry(typeid:=otFieldDataType.Bool, defaultvalue:="0", isnullable:=True, _
                title:="Ignore Domain", description:="flag if the domainValue is to be ignored -> look in global")> _
            Public Const ConstFNIsDomainIgnored As String = "domainignore"

            '** Column names and definition
            <ormObjectEntry(typeid:=otFieldDataType.Timestamp, _
                title:="Updated On", Description:="last update time stamp in the data store")> Public Const ConstFNUpdatedOn As String = ot.ConstFNUpdatedOn

            <ormObjectEntry(typeid:=otFieldDataType.Timestamp, _
                title:="Created On", Description:="creation time stamp in the data store")> Public Const ConstFNCreatedOn As String = ot.ConstFNCreatedOn

            '** deleted Field
            <ormObjectEntry(typeid:=otFieldDataType.Timestamp, isnullable:=True, defaultvalue:=ConstNullTimestampString, _
                title:="Deleted On", Description:="time stamp when the deletion flag was set")> Public Const ConstFNDeletedOn As String = ot.ConstFNDeletedOn

            '** Deleted flag
            <ormObjectEntry(typeid:=otFieldDataType.Bool, defaultvalue:="0", _
                title:="Deleted", description:="flag if the entry in the data stored is regarded as deleted depends on the deleteflagbehavior")> _
            Public Const ConstFNIsDeleted As String = ot.ConstFNIsDeleted

            '** Spare Parameters are all nullable
            <ormObjectEntry(typeid:=otFieldDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, _
            title:="text parameter 1", description:="text parameter 1")> Public Const ConstFNParamText1 = "param_txt1"
            <ormObjectEntry(typeid:=otFieldDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, _
            title:="text parameter 2", description:="text parameter 2")> Public Const ConstFNParamText2 = "param_txt2"
            <ormObjectEntry(typeid:=otFieldDataType.Text, size:=255, isnullable:=True, spareFieldTag:=True, _
            title:="text parameter 3", description:="text parameter 3")> Public Const ConstFNParamText3 = "param_txt3"
            <ormObjectEntry(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 1", description:="numeric parameter 1")> Public Const ConstFNParamNum1 = "param_num1"
            <ormObjectEntry(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 2", description:="numeric parameter 2")> Public Const ConstFNParamNum2 = "param_num2"
            <ormObjectEntry(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 3", description:="numeric parameter 3")> Public Const ConstFNParamNum3 = "param_num3"
            <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 1", description:="date parameter 1")> Public Const ConstFNParamDate1 = "param_date1"
            <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 2", description:="date parameter 2")> Public Const ConstFNParamDate2 = "param_date2"
            <ormObjectEntry(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 3", description:="date parameter 3")> Public Const ConstFNParamDate3 = "param_date3"
            <ormObjectEntry(typeid:=otFieldDataType.Bool, isnullable:=True, spareFieldTag:=True, _
            title:="flag parameter 1", description:="flag parameter 1")> Public Const ConstFNParamFlag1 = "param_flag1"
            <ormObjectEntry(typeid:=otFieldDataType.Bool, isnullable:=True, spareFieldTag:=True, _
            title:="flag parameter 2", description:="flag parameter 2")> Public Const ConstFNParamFlag2 = "param_flag2"
            <ormObjectEntry(typeid:=otFieldDataType.Bool, isnullable:=True, spareFieldTag:=True, _
            title:="flag parameter 3", description:="flag parameter 3")> Public Const ConstFNParamFlag3 = "param_flag3"

            '** columnMapping of persistable fields
            <ormEntryMapping(EntryName:=ConstFNUpdatedOn)> Protected _updatedOn As Date = ot.ConstNullDate
            <ormEntryMapping(EntryName:=ConstFNCreatedOn)> Protected _createdOn As Date = ConstNullDate
            <ormEntryMapping(EntryName:=ConstFNDeletedOn)> Protected _deletedOn As Date = ConstNullDate
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


            '**** OPERATION DEFAULTS
            <ormObjectOperation(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                            Description:="create an instance of persist able data object")> Protected Const ConstOPCreate = "Create"
            <ormObjectOperation(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                           Description:="retrieve a data object")> Protected Const ConstOPRetrieve = "Retrieve"
            <ormObjectOperation(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadUpdate & ", true, true)"}, _
                           Description:="delete a data object")> Protected Const ConstOPDelete = "Delete"
            <ormObjectOperation(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadonly & ", true, true)"}, _
                           Description:="inject a data object")> Protected Const ConstOPInject = "Inject"
            <ormObjectOperation(DefaultAllowPermission:=True, PermissionRules:={ObjectPermissionRuleProperty.DBAccess & "(" & AccessRightProperty.ConstARReadUpdate & ", true, true)"}, _
                           Description:="perist a data object")> Protected Const ConstOPPersist = "Persist"
#Region "Properties"

            ''' <summary>
            ''' Sets the flag for ignoring the domainentry (delete on domain level)
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property IsDomainIgnored As Boolean
                Get
                    Return _DomainIsIgnored
                End Get
                Set(value As Boolean)
                    SetValue(entryname:=ConstFNIsDomainIgnored, value:=value)
                End Set
            End Property
            ''' <summary>
            ''' Gets the table store.
            ''' </summary>
            ''' <value>The table store.</value>
            Public ReadOnly Property TableStore() As iormDataStore Implements iormPersistable.TableStore
                Get
                    If _record.Alive AndAlso Not _record.TableStore Is Nothing Then
                        Return _record.TableStore
                    ElseIf Me._primaryTableID <> "" And Not Me.RunTimeOnly Then
                        Return GetTableStore(tableid:=_primaryTableID)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property
            ''' <summary>
            ''' True if a memory data object
            ''' </summary>
            ''' <value>The run time only.</value>
            Public ReadOnly Property RunTimeOnly() As Boolean Implements iormPersistable.RuntimeOnly
                Get
                    Return Me._RunTimeOnly
                End Get
            End Property
            ''' <summary>
            ''' Gets the current infuse mode.
            ''' </summary>
            ''' <value>The current infuse mode.</value>
            Public Property CurrentInfuseMode() As otInfuseMode
                Get
                    Return Me._CurrentInfuseMode
                End Get
                Private Set(value As otInfuseMode)
                    _CurrentInfuseMode = value
                End Set
            End Property
            ''' <summary>
            ''' returns the object definition associated with this data object
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property ObjectDefinition As ObjectDefinition
                Get
                    If CurrentSession.IsRunning Or CurrentSession.IsStartingUp Then
                        Return CurrentSession.Objects.GetObject(objectname:=Me.ObjectID)
                    Else
                        CoreMessageHandler(message:="not connected to ontrack - connect first", tablename:=Me.TableID, _
                                           subname:="ormDataObject.ObjectDefinition", messagetype:=otCoreMessageType.InternalWarning)
                        Return Nothing
                    End If

                End Get
            End Property
            ''' <summary>
            ''' returns the object class description associated with this data object
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property ObjectClassDescription As ObjectClassDescription Implements iormPersistable.ObjectClassDescription
                Get
                    If _classDescription Is Nothing Then
                        _classDescription = ot.GetObjectClassDescription(Me.GetType)
                    End If
                    Return _classDescription
                End Get
                Set(value As ObjectClassDescription)
                    If Not _IsInitialized Then
                        _classDescription = value
                    End If
                End Set
            End Property
            ''' <summary>
            ''' returns the tableschema associated with this data object
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property TableSchema() As iotDataSchema
                Get
                    If Me.TableStore IsNot Nothing Then
                        Return Me.TableStore.TableSchema
                    Else
                        Return Nothing
                    End If

                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the domain ID.
            ''' </summary>
            ''' <value>The domain ID.</value>
            Public Property DomainID() As String
                Get
                    If CurrentSession.IsRunning AndAlso _
                        Me.ObjectDefinition IsNot Nothing AndAlso Me.ObjectDefinition.DomainBehavior Then
                        Return Me._domainID
                    Else
                        Return CurrentSession.CurrentDomainID
                    End If
                End Get
                Set(value As String)
                    Me._domainID = value
                End Set
            End Property
            ''' <summary>
            ''' sets or gets the DBDriver for the data object to use
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property DBDriver As iormDatabaseDriver Implements iormPersistable.DbDriver

                Set(value As iormDatabaseDriver)
                    If Not _IsInitialized Then
                        _dbdriver = value
                    Else
                        Call CoreMessageHandler(message:="can not set the dbdriver while initialised", subname:="ormDataobject.DBDriver", _
                                                messagetype:=otCoreMessageType.InternalError)
                    End If
                End Set
                Get
                    Return _dbdriver
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the PS is initialized.
            ''' </summary>
            ''' <value>The PS is initialized.</value>
            Public ReadOnly Property IsInitialized() As Boolean Implements iormPersistable.IsInitialized
                Get
                    Return Me._IsInitialized
                End Get

            End Property
            ''' <summary>
            ''' returns the ObjectID of the Class of this instance
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property ObjectID() As String Implements iormPersistable.ObjectID
                Get
                    If Me.ObjectClassDescription IsNot Nothing Then
                        Return Me.ObjectClassDescription.ID

                    Else
                        CoreMessageHandler("object id for orm data object class could not be found", arg1:=Me.GetType.Name, _
                                            subname:="ormDataObejct.ObjectID", messagetype:=otCoreMessageType.InternalError)
                    End If
                    Return Nothing
                End Get
            End Property
            ''' <summary>
            ''' Gets or sets the isDeleted.
            ''' </summary>
            ''' <value>The isDeleted.</value>
            Public Property IsDeleted() As Boolean
                Get
                    Return Me._IsDeleted
                End Get
                Protected Friend Set(value As Boolean)
                    Me._IsDeleted = value
                    If value = False Then
                        _deletedOn = ConstNullDate
                    End If
                End Set
            End Property
            ''' <summary>
            ''' returns true if object has domain behavior
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property HasDomainBehavior As Boolean
                Get
                    '** to avoid recursion loops for bootstrapping objects during 
                    '** startup of session check these out and look into description
                    If CurrentSession.IsBootstrappingInstallationRequested _
                        OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                        Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescription(Me.ObjectID)
                        If anObjectDecsription IsNot Nothing Then
                            Return anObjectDecsription.ObjectAttribute.AddDomainBehaviorFlag
                        Else
                            Return False
                        End If
                    Else
                        Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                        If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.DomainBehavior
                        Return False
                    End If

                End Get

            End Property
            ''' <summary>
            ''' returns true if object has delete per flag behavior
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property HasDeletePerFlagBehavior As Boolean
                Get
                    '** avoid loops while starting up with bootstraps or during installation
                    If CurrentSession.IsBootstrappingInstallationRequested OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                        Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                        If anObjectDescription IsNot Nothing Then
                            Return anObjectDescription.ObjectAttribute.DeleteFieldFlag
                        Else
                            Return False
                        End If
                    Else
                        Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                        '** per flag
                        If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.DeletePerFlagBehavior
                    End If

                End Get
            End Property
            ''' <summary>
            ''' Gets or sets the PS is changed.
            ''' </summary>
            ''' <value>The PS is changed.</value>
            Public Property IsChanged() As Boolean
                Get
                    Return Me._IsChanged
                End Get
                Protected Friend Set(value As Boolean)
                    Me._IsChanged = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the PS is loaded.
            ''' </summary>
            ''' <value>The PS is loaded.</value>
            Public ReadOnly Property IsLoaded() As Boolean Implements iormPersistable.IsLoaded
                Get
                    Return Me._IsLoaded
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the PS is created.
            ''' </summary>
            ''' <value>The PS is created.</value>
            Public ReadOnly Property IsCreated() As Boolean Implements iormPersistable.IsCreated
                Get
                    Return Me._IsCreated
                End Get
            End Property
            ''' <summary>
            ''' unload the Dataobject from the datastore
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function Unload() As Boolean
                _IsLoaded = False
            End Function
            ''' <summary>
            ''' Gets or sets the OTDB record.
            ''' </summary>
            ''' <value>The OTDB record.</value>
            Public Property Record() As ormRecord Implements iormPersistable.Record
                Get
                    Return Me._record
                End Get
                Set(value As ormRecord)
                    If _record Is Nothing Then
                        Me._record = value
                    Else
                        MergeRecord(value)
                    End If
                End Set
            End Property

            Public Property LoadedFromHost() As Boolean
                Get
                    LoadedFromHost = _IsloadedFromHost
                End Get
                Protected Friend Set(value As Boolean)
                    _IsloadedFromHost = value
                End Set
            End Property
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property SavedToHost() As Boolean
                Get
                    SavedToHost = _IsSavedToHost
                End Get
                Protected Friend Set(value As Boolean)
                    _IsSavedToHost = value
                End Set
            End Property
            '** set the serialize with HostApplication
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property SerializeWithHostApplication() As Boolean
                Get
                    SerializeWithHostApplication = _serializeWithHostApplication
                End Get
                Protected Friend Set(value As Boolean)
                    If value Then
                        If isRegisteredAtHostApplication(Me.TableID) Then
                            _serializeWithHostApplication = True
                        Else
                            _serializeWithHostApplication = registerHostApplicationFor(Me.TableID, AllObjectSerialize:=False)
                        End If
                    Else
                        _serializeWithHostApplication = False
                    End If
                End Set
            End Property


            ''' <summary>
            ''' gets the TableID of the persistency table
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property TableID() As String Implements iormPersistable.TableID
                Get
                    TableID = _primaryTableID
                End Get
            End Property
            ''' <summary>
            ''' gets the Creation date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property CreatedOn() As Date
                Get
                    CreatedOn = _createdOn
                End Get
            End Property
            ''' <summary>
            ''' gets the last update date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property UpdatedOn() As Date
                Get
                    UpdatedOn = _updatedOn
                End Get
            End Property
            ''' <summary>
            ''' gets the deletion date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Property DeletedOn() As Date
                Get
                    DeletedOn = _deletedOn
                End Get
                Friend Set(value As Date)
                    DeletedOn = value
                End Set
            End Property

            Public Property parameter_num1() As Double?
                Get
                    Return _parameter_num1
                End Get
                Set(value As Double?)
                    If _parameter_num1 <> value Then
                        _parameter_num1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_num2() As Double?
                Get
                    Return _parameter_num2
                End Get
                Set(value As Double?)
                    If _parameter_num2 <> value Then
                        _parameter_num2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_num3() As Double?
                Get
                    Return _parameter_num3
                End Get
                Set(value As Double?)
                    If _parameter_num3 <> value Then
                        _parameter_num3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date1() As Date?
                Get
                    Return _parameter_date1
                End Get
                Set(value As Date?)
                    If _parameter_date1 <> value Then
                        _parameter_date1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date2() As Date?
                Get
                    Return _parameter_date2
                End Get
                Set(value As Date?)
                    If _parameter_date2 <> value Then
                        _parameter_date2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date3() As Date?
                Get
                    Return _parameter_date3
                End Get
                Set(value As Date?)
                    _parameter_date3 = value
                    Me.IsChanged = True
                End Set
            End Property
            Public Property parameter_flag1() As Boolean?
                Get
                    Return _parameter_flag1
                End Get
                Set(value As Boolean?)
                    If _parameter_flag1 <> value Then
                        _parameter_flag1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_flag3() As Boolean?
                Get
                    parameter_flag3 = _parameter_flag3
                End Get
                Set(value As Boolean?)
                    If _parameter_flag3 <> value Then
                        _parameter_flag3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_flag2() As Boolean?
                Get
                    Return _parameter_flag2
                End Get
                Set(value As Boolean?)
                    If _parameter_flag2 <> value Then
                        _parameter_flag2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt1() As String
                Get
                    Return _parameter_txt1
                End Get
                Set(value As String)
                    If _parameter_txt1 <> value Then
                        _parameter_txt1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt2() As String
                Get
                    Return _parameter_txt2
                End Get
                Set(value As String)
                    If _parameter_txt2 <> value Then
                        _parameter_txt2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt3() As String
                Get
                    Return _parameter_txt3
                End Get
                Set(value As String)
                    If _parameter_txt3 <> value Then
                        _parameter_txt3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
#End Region


            ''' <summary>
            ''' constructor for ormDataObject
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <remarks></remarks>
            Protected Sub New(Optional tableid As String = "", Optional objectID As String = "", Optional dbdriver As iormDatabaseDriver = Nothing)
                _IsInitialized = False
                If tableid <> "" Then _primaryTableID = tableid
                If objectID <> "" Then
                    _classDescription = ot.GetObjectClassDescriptionByID(id:=objectID)
                    If _classDescription Is Nothing Then
                        _classDescription = ot.GetObjectClassDescription(Me.GetType)
                    End If
                End If
                _dbdriver = dbdriver
            End Sub
            ''' <summary>
            ''' clean up with the object
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub Finialize()
                _IsInitialized = False
                Me.Record = Nothing
                _primaryTableID = ""
                _dbdriver = Nothing
            End Sub

            '*****
            '*****
            Private Sub NotifyPropertyChanged(Optional ByVal propertyname As String = Nothing)
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyname))

            End Sub

            ''' <summary>
            ''' validates the Business Object as total
            ''' </summary>
            ''' <remarks></remarks>
            ''' <returns>True if validated and OK</returns>
            Public Function Validate() As otValidationResultType Implements iormValidatable.Validate
                Return otValidationResultType.Succeeded
            End Function

            ''' <summary>
            ''' validates a named object entry of the object
            ''' </summary>
            ''' <param name="enryname"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Protected Function Validate(enryname As String, value As Object) As otValidationResultType Implements iormValidatable.Validate
                Dim result As otValidationResultType
                If Not CurrentSession.IsBootstrappingInstallationRequested Then
                    '' while doing it different
                    result = otValidationResultType.Succeeded
                Else
                    Dim i = 1
                    result = otValidationResultType.Succeeded
                End If
                Return result
            End Function

            ''' <summary>
            ''' raises the PropetyChanged Event
            ''' </summary>
            ''' <param name="entryname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function RaiseObjectEntryChanged(entryname As String) As Boolean
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(entryname))
            End Function

            ''' <summary>
            ''' Apply the ObjectEntryProperty to a value
            ''' </summary>
            ''' <param name="entryname"></param>
            ''' <param name="in"></param>
            ''' <param name="out"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function ApplyObjectEntryProperty(entryname As String, ByVal [in] As Object, ByRef out As Object) As Boolean
                Try
                    Dim theProperties As IEnumerable(Of ObjectEntryProperty)
                    If (Not CurrentSession.IsBootstrappingInstallationRequested AndAlso _
                        (Not CurrentSession.IsStartingUp AndAlso ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID))) _
                        AndAlso Me.ObjectDefinition.HasEntry(entryname:=entryname) Then
                        theProperties = Me.ObjectDefinition.GetEntry(entryname).Properties
                    ElseIf Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname) IsNot Nothing Then
                        theProperties = Me.ObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname).ObjectEntryProperties
                        Return True
                    Else
                        CoreMessageHandler(message:="entry of object definition could not be found", objectname:=Me.ObjectID, entryname:=entryname, _
                                            subname:="ormDataObject.ApplyObjectEntryProperty", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                    Dim result As Boolean = True
                    If theProperties IsNot Nothing Then
                        For Each aProperty In theProperties
                            result = result And aProperty.Apply([in]:=[in], out:=out)
                        Next
                    Else
                        CoreMessageHandler(message:="ObjectEntryProperty is nothing", subname:="ormDataObject.ApplyObjectEntryProperty", messagetype:=otCoreMessageType.InternalError)

                    End If
                    Return result
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormDataObject.ApplyObjectEntryProperty")
                    Return False
                End Try

            End Function
            ''' <summary>
            ''' applies object entry properties, validates and sets a value of a entry/member
            ''' raises the propertychanged event
            ''' if it is different to its value
            ''' </summary>
            ''' <param name="entryname"></param>
            ''' <param name="member"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function GetValue(entryname As String, Optional ByRef fieldmembername As String = "") As Object
                Dim result As Boolean = False

                Try

                    Dim value As Object
                    Dim aClassDescription = Me.ObjectClassDescription ' ot.GetObjectClassDescription(Me.GetType)
                    If aClassDescription Is Nothing Then
                        CoreMessageHandler(message:=" Object's Class Description could not be retrieved - object not defined ?!", arg1:=value, _
                                          objectname:=Me.ObjectID, entryname:=entryname, _
                                           messagetype:=otCoreMessageType.InternalError, subname:="ormDataObjectGSetValue")
                        Return False
                    End If

                    Dim afieldinfos = aClassDescription.GetEntryFieldInfos(entryname)
                    If afieldinfos.Count = 0 Then
                        CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to a class field member or the entry name is not valid", arg1:=value, _
                                           objectname:=Me.ObjectID, entryname:=entryname, _
                                            messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    ElseIf afieldinfos.Count > 1 And fieldmembername = "" Then
                       

                    End If

                    '** search the fields
                    For Each field In afieldinfos

                        If Not Reflector.GetFieldValue(field:=field, dataobject:=Me, value:=value) Then
                            CoreMessageHandler(message:="field value ob data object couldnot be retrieved", _
                                                objectname:=Me.ObjectID, subname:="ormDataObject.getValue", _
                                                messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.TableID)
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
                    CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to multiple field member - the specified fieldname was not found", arg1:=fieldmembername, _
                                          objectname:=Me.ObjectID, entryname:=entryname, messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.GetValue")
                    Return value

                    Return False

                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormDataObject.getvalue")
                    Return Nothing
                End Try

            End Function
            ''' <summary>
            ''' applies object entry properties, validates and sets a value of a entry/member
            ''' raises the propertychanged event
            ''' if it is different to its value
            ''' </summary>
            ''' <param name="entryname"></param>
            ''' <param name="member"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function SetValue(entryname As String, ByVal value As Object) As Boolean
                Dim result As Boolean = False
                '** apply any conversion Properties
                If Not ApplyObjectEntryProperty(entryname:=entryname, [in]:=value, out:=value) Then
                    CoreMessageHandler(message:="applying object entry properties failed - value not set", arg1:=value, subname:="ormDataObject.SetValue", _
                                       objectname:=Me.ObjectID, entryname:=entryname, _
                                       messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

                Try
                    Dim aValidateResult As otValidationResultType = Validate(entryname, value)
                    '** Validate against the ObjectEntry Rules
                    If aValidateResult = otValidationResultType.Succeeded Or aValidateResult = otValidationResultType.FailedButSave Then

                        Dim oldvalue As Object
                        Dim aClassDescription = Me.ObjectClassDescription 'ot.GetObjectClassDescription(Me.GetType)
                        If aClassDescription Is Nothing Then
                            CoreMessageHandler(message:=" Object's Class Description could not be retrieved - object not defined ?!", arg1:=value, _
                                              objectname:=Me.ObjectID, entryname:=entryname, _
                                               messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                            Return False
                        End If
                        Dim afieldinfos = aClassDescription.GetEntryFieldInfos(entryname)
                        If afieldinfos.Count = 0 Then
                            CoreMessageHandler(message:="Warning ! ObjectEntry is not mapped to a class field member or the entry name is not valid", arg1:=value, _
                                               objectname:=Me.ObjectID, entryname:=entryname, _
                                                messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.SetValue")
                        End If
                        For Each field In afieldinfos
                            If Not Reflector.GetFieldValue(field:=field, dataobject:=Me, value:=oldvalue) Then
                                CoreMessageHandler(message:="field value ob data object couldnot be get", _
                                                    objectname:=Me.ObjectID, subname:="ormDataObject.setValue", _
                                                    messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.TableID)
                            End If

                            '*** if different value
                            If value IsNot Nothing AndAlso Not value.Equals(oldvalue) Then
                                If Not Reflector.SetFieldValue(field:=field, dataobject:=Me, value:=value) Then
                                    CoreMessageHandler(message:="field value ob data object couldnot be set", _
                                                        objectname:=Me.ObjectID, subname:="ormDataObject.setValue", _
                                                        messagetype:=otCoreMessageType.InternalError, entryname:=entryname, tablename:=Me.TableID)
                                End If
                                result = True
                            End If
                        Next

                        '** raise changed event
                        If result Then
                            Me.IsChanged = True
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
                For Each key In record.Keys
                    If (_record.IsTableBound AndAlso _record.HasIndex(key)) OrElse Not _record.IsTableBound Then Me._record.SetValue(key, record.GetValue(key))
                Next
                Return True
            End Function
            ''' <summary>
            ''' checks if the data object is alive
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function IsAlive(Optional throwError As Boolean = True, Optional subname As String = "") As Boolean Implements iormPersistable.isalive
                If Not Me.IsLoaded And Not Me.IsCreated Then
                    If throwError Then
                        If subname = "" Then subname = "ormDataObject.checkalive"
                        CoreMessageHandler(message:="object is not alive but operation requested", objectname:=Me.GetType.Name, _
                                           subname:=subname, tablename:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                    End If
                    Return False
                End If
                Return True
            End Function
            ''' <summary>
            ''' initialize the data object
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Initialize(Optional runtimeOnly As Boolean = False) As Boolean Implements iormPersistable.Initialize
                Dim ourEventArgs As New ormDataObjectEventArgs(Me)

                '** is a session running ?!
                If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                    Call CoreMessageHandler(message:="data object cannot be initialized - start session to database first", _
                                               objectname:=Me.ObjectID, subname:="ormDataobject.initialize", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If

                '*** Class Description
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
                If _classDescription Is Nothing Then
                    CoreMessageHandler("object id for orm data object class could not be found", arg1:=Me.GetType.Name, _
                                        subname:="ormDataObejct.Initialize", messagetype:=otCoreMessageType.InternalError)
                    ourEventArgs.Result = False
                    ourEventArgs.AbortOperation = True
                Else
                    ourEventArgs.Result = True
                    ourEventArgs.AbortOperation = False
                End If
                '*** tableid
                If Me.TableID = "" AndAlso _classDescription IsNot Nothing Then
                    _primaryTableID = _classDescription.PrimaryTable
                End If
                '*** 
                If Me.TableID = "" Then
                    ourEventArgs.Result = False
                    ourEventArgs.AbortOperation = False
                End If

                '** fire event
                RaiseEvent OnInitializing(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                End If
                Initialize = True

                '** set tableid
                If Me.TableID <> "" And ourEventArgs.Proceed Then
                    '** get new Table
                    If _record Is Nothing OrElse runtimeOnly Then
                        _record = New ormRecord(Me.TableID, dbdriver:=_dbdriver, runtimeOnly:=runtimeOnly)
                    Else
                        _record.SetTable(Me.TableID) 'now we are not runtimeonly anymore -> set also the table and let's have a fixed structure
                    End If


                    If _record.IsTableBound OrElse _RunTimeOnly OrElse runtimeOnly Then
                        Initialize = True
                    Else
                        Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="record ist not set to tabledefinition", _
                                                messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID, noOtdbAvailable:=True)
                        Initialize = False
                    End If

                    '*** check on connected status if not bootstrapping
                    If Not Me.Record.TableStore Is Nothing AndAlso Not Me.Record.TableStore.Connection Is Nothing Then
                        If Not Me.Record.TableStore.Connection.IsConnected AndAlso Not Me.Record.TableStore.Connection.Session.IsBootstrappingInstallationRequested Then
                            Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="TableStore is not connected to database / no connection available", _
                                                    messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                            Initialize = False
                        End If
                    End If

                    '** register for caching
                    'Call Cache.RegisterCacheFor(ObjectTag:=Me.TableID)

                ElseIf Me.TableID = "" Then
                    Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="Tablename / id is blank for OTDB object", _
                                            messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                    Initialize = False
                End If

                '* default values
                _updatedOn = ConstNullDate
                _createdOn = ConstNullDate
                _deletedOn = ConstNullDate
                _IsDeleted = False
                
                '*** here we could infuse the default values

                '** fire event
                ourEventArgs = New ormDataObjectEventArgs(object:=Me, record:=Me.Record)
                ourEventArgs.Proceed = Initialize
                RaiseEvent OnInitialized(Me, ourEventArgs)
                '** set initialized
                _IsInitialized = ourEventArgs.Proceed
                Return ourEventArgs.Proceed
            End Function
            ''' <summary>
            ''' load DataObject by Type and Primary Key-Array
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="pkArray"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function InjectDataObject(Of T As {iormInfusable, iormPersistable, New})(pkArray() As Object, Optional domainID As String = "", Optional dbdriver As iormDatabaseDriver = Nothing) As iormPersistable
                Dim aDataObject As New T

                If dbdriver IsNot Nothing Then aDataObject.DbDriver = dbdriver
                If aDataObject.Inject(pkArray, domainID:=domainID) Then
                    Return aDataObject
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' loads and infuse the deliverable by primary key from the data store
            ''' </summary>
            ''' <param name="UID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function Inject(ByRef pkArray() As Object, Optional domainID As String = "", Optional loadDeleted As Boolean = False) As Boolean Implements iormPersistable.Inject
                Dim aRecord As ormRecord

                '* init
                If Not Me.IsInitialized AndAlso Not Me.Initialize Then
                    Return False
                End If
                '** check on the operation right for this object
                If Not RunTimeOnly AndAlso Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) _
                    AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                    objectoperations:={Me.ObjectID & "." & ConstOPInject}) Then
                    If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, _
                                                            domainID:=domainID, _
                                                            username:=CurrentSession.Username, _
                                                             messagetext:="Please provide another user to authorize requested operation", _
                                                            objectoperations:={Me.ObjectID & "." & ConstOPInject}) Then
                        Call CoreMessageHandler(message:="data object cannot be injected - permission denied to user", _
                                                objectname:=Me.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                                subname:="ormDataObject.Inject", messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    End If
                End If

                Try
                    '** set the infuse mode
                    Me.CurrentInfuseMode = otInfuseMode.OnInject
                    _RunTimeOnly = False

                    '** check for domainBehavior
                    If Me.HasDomainBehavior Then
                        SubstituteDomainIDinPKArray(pkarray:=pkArray, domainid:=domainID, runtimeOnly:=RunTimeOnly)
                    End If

                    '** fire event
                    Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=aRecord, pkarray:=pkArray)
                    Dim useRecordCache = Me.TableStore.HasProperty(ConstTPNCacheProperty)
                    ourEventArgs.UseCache = useRecordCache
                    RaiseEvent OnInjecting(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        If ourEventArgs.Result Then
                            Me.Record = ourEventArgs.Record
                        End If
                        '** reset the infuse mode
                        Me.CurrentInfuseMode = otInfuseMode.OnDefault
                        Return ourEventArgs.Result
                    Else
                        pkArray = ourEventArgs.Pkarray
                        aRecord = ourEventArgs.Record
                        useRecordCache = ourEventArgs.UseCache
                    End If

                    '* use record level Cache ...
                    If ourEventArgs.UseCache Then
                        ' try to load it from cache
                        aRecord = TryCast(LoadFromCache("Record" & ConstDelimiter & _primaryTableID, pkArray), ormRecord)
                    End If
                    '** load from tablestore if we do not have it
                    If aRecord Is Nothing Then
                        aRecord = Me.TableStore.GetRecordByPrimaryKey(pkArray)
                    End If
                    '* on domain behavior ? -> reload from  the global domain
                    If Me.HasDomainBehavior AndAlso aRecord Is Nothing AndAlso domainID <> ConstGlobalDomain Then
                        SubstituteDomainIDinPKArray(pkarray:=pkArray, domainid:=ConstGlobalDomain, runtimeOnly:=RunTimeOnly)
                        aRecord = Me.TableStore.GetRecordByPrimaryKey(pkArray)
                    End If

                    '* still nothing ?!
                    If aRecord Is Nothing Then
                        _IsLoaded = False
                        '** reset the infuse mode
                        Me.CurrentInfuseMode = otInfuseMode.OnDefault
                        Return False
                    Else
                        '* what about deleted objects
                        If Me.HasDeletePerFlagBehavior Then
                            If aRecord.HasIndex(ConstFNIsDeleted) Then
                                If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                    _IsDeleted = True
                                    '* load only on deleted
                                    If Not loadDeleted Then
                                        _IsLoaded = False
                                        _IsCreated = False
                                        '** reset the infuse mode
                                        Me.CurrentInfuseMode = otInfuseMode.OnDefault
                                        Return False
                                    End If
                                Else
                                    _IsDeleted = False
                                End If
                            Else
                                CoreMessageHandler(message:="object has delete per flag behavior but no flag", messagetype:=otCoreMessageType.InternalError, _
                                                    subname:="ormDataObject.Inject", tablename:=Me.TableID, entryname:=ConstFNIsDeleted)
                                _IsDeleted = False
                            End If
                        Else
                            _IsDeleted = False
                        End If

                        '** add to cache
                        If ourEventArgs.UseCache Then AddToCache("Record" & ConstDelimiter & _primaryTableID, key:=pkArray, theOBJECT:=aRecord)
                        _IsLoaded = Me.Infuse(aRecord)

                        '** reset flags
                        If Me.IsLoaded Then
                            _IsCreated = False
                            _IsChanged = False
                        End If

                        '** fire event
                        ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=pkArray)
                        ourEventArgs.Proceed = _IsLoaded
                        ourEventArgs.UseCache = useRecordCache
                        RaiseEvent OnInjected(Me, ourEventArgs)
                        _IsLoaded = ourEventArgs.Proceed

                        '** return
                        Return Me.IsLoaded
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.Inject", arg1:=pkArray, tablename:=_primaryTableID)
                    '** reset the infuse mode
                    Me.CurrentInfuseMode = otInfuseMode.OnDefault
                    Return False
                End Try


            End Function

            ''' <summary>
            ''' Persist the object to the datastore
            ''' </summary>
            ''' <param name="timestamp"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function Persist(Optional timestamp As Date = ot.ConstNullDate, Optional doFeedRecord As Boolean = True) As Boolean Implements iormPersistable.Persist

                '* init
                If Not Me.IsInitialized AndAlso Not Me.Initialize() Then Return False
                '** must be alive from data store
                If Not IsAlive(subname:="Persist") Then
                    Return False
                End If
                '*** runtime only object cannot be persisted
                If Me.RunTimeOnly Then Return False
                '** record must be alive too
                If Not Me.Record.Alive Then
                    CoreMessageHandler(message:="record is not alive in data object - cannot persist", messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ormDataObject.Persist", objectname:=Me.ObjectID, tablename:=Me.TableID)
                    Return False
                End If
                '** check on the operation right for this object
                If Not CurrentSession.IsStartingUp AndAlso _
                    Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, objectoperations:={Me.ObjectID & "." & ConstOPPersist}) Then
                    '** authorize
                    If CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, _
                                                        messagetext:="Please provide another user to authorize requested operation", _
                                                        username:=CurrentSession.Username, _
                                                        objectoperations:={Me.ObjectID & "." & ConstOPPersist}) Then
                        Call CoreMessageHandler(message:="data object cannot be persisted - permission denied to user", _
                                                objectname:=Me.ObjectID, arg1:=ConstOPPersist, username:=CurrentSession.Username, _
                                                subname:="ormDataObject.Persist", messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    End If
                End If

                Try
                    '* if object was deleted an its now repersisted
                    Dim isdeleted As Boolean = _IsDeleted
                    _IsDeleted = False

                    '** fire event
                    Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record)
                    RaiseEvent ClassOnPersisting(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        Return False
                    Else
                        _record = ourEventArgs.Record
                    End If

                    '** fire event
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record)
                    RaiseEvent OnPersisting(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        Return False
                    Else
                        _record = ourEventArgs.Record
                    End If

                    '** feed record
                    If doFeedRecord Then FeedRecord()

                    '** persist through the record
                    Persist = Me.Record.Persist(timestamp)

                    '*** cascade the operation through the related members
                    Persist = Persist And CascadeRelation(Me, Me.ObjectClassDescription, cascadeUpdate:=True, cascadeDelete:=False)

                    '** fire event
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=Record)
                    RaiseEvent OnPersisted(Me, ourEventArgs)
                    Persist = ourEventArgs.Proceed

                    RaiseEvent ClassOnPersisted(Me, ourEventArgs)
                    Persist = ourEventArgs.Proceed And ourEventArgs.Proceed

                    '** reset flags
                    If Persist Then
                        _IsCreated = False
                        _IsChanged = False
                        _IsLoaded = True
                        _IsDeleted = False
                    Else
                        _IsDeleted = isdeleted
                    End If
                    Return Persist

                Catch ex As Exception
                    Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Persist")
                    Return False
                End Try
            End Function
            ''' <summary>
            ''' Static Function ALL returns a Collection of all objects
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function All(Of T As {iormInfusable, iormPersistable, New})(Optional ID As String = "All", _
                                                                                      Optional domainID As String = "",
                                                                                       Optional where As String = "", _
                                                                                       Optional orderby As String = "", _
                                                                                       Optional parameters As List(Of ormSqlCommandParameter) = Nothing) _
                                                                                   As List(Of T)
                Dim aCollection As New List(Of T)
                Dim aRecordCollection As New List(Of ormRecord)
                Dim aStore As iormDataStore
                Dim anObject As New T

                '** is a session running ?!
                If Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - start session to database first", _
                                            objectname:=anObject.ObjectID, _
                                            subname:="ormDataObject.All", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If

                '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
                If Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObject.ObjectID) _
                    AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                    objectoperations:={anObject.ObjectID & "." & ConstOPInject}) Then
                    '** request authorizartion
                    If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainID:=domainID, _
                                                                                username:=CurrentSession.Username, _
                                                                                objectoperations:={anObject.ObjectID & "." & ConstOPInject}) Then
                        Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                                objectname:=anObject.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                                subname:="ormDataObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If

                Try
                    '** TODO: Add Domain Behavior
                    aStore = anObject.TableStore
                    aRecordCollection = aStore.GetRecordsBySqlCommand(id:=ID, wherestr:=where, orderby:=orderby, parameters:=parameters)

                    If aRecordCollection.Count > 0 Then
                        For Each aRecord In aRecordCollection
                            anObject = New T
                            If anObject.Infuse(aRecord) Then
                                aCollection.Add(item:=anObject)
                            End If
                        Next aRecord

                    End If
                    Return aCollection

                Catch ex As Exception
                    Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.All(of T)")
                    Return aCollection
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
            Public Function GetVersion(dataobject As iormPersistable, Optional name As String = "") As Long Implements iormPersistable.GetVersion
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

                'Dim aFieldList As System.Reflection.FieldInfo()
                'Dim tableIDs As New List(Of String)
                'Dim tableAttrIds As New List(Of String)
                'Dim tableAttrDeleteFlags As New List(Of Boolean)
                'Dim tableAttrSpareFieldsFlags As New List(Of Boolean)
                'Dim tableAttrDomainIDFlags As New List(Of Boolean)
                'Dim tableVersions As New List(Of UShort)
                'Dim fieldDescs As New List(Of ormFieldDescription)
                'Dim primaryKeyList As New SortedList(Of Short, String)
                'Dim indexList As New Dictionary(Of String, String())
                'Dim ordinalPos As Long = 1

                'Try
                '    '** fire event
                '    Dim ourEventArgs As New ormDataObjectEventArgs([object]:=Nothing)
                '    RaiseEvent OnSchemaCreating(Nothing, e:=ourEventArgs)
                '    If ourEventArgs.AbortOperation Then
                '        Return False
                '    End If

                '    '***
                '    '*** go through all ORM Attributes and extract object definition properties
                '    '***
                '    Dim aDescriptor As ormObjectClassDescription = ot.GetObjectClassDescription(name:=GetType(T).Name)
                '    If aDescriptor Is Nothing Then
                '        CoreMessageHandler(message:="couldnot retrieve descriptor for business object class from core store", arg1:=GetType(T).Name, _
                '                            messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.createSchema")
                '        Return False
                '    End If

                '    '*** read the object attributes
                '    Dim theObjectAttribute As ormObjectAttribute = aDescriptor.ObjectAttribute
                '    If theObjectAttribute Is Nothing Then
                '        CoreMessageHandler(message:="the object class has no object attribute", arg1:=aDescriptor.Type.Name, _
                '                            subname:="ormDataObject.createSchema", messagetype:=otCoreMessageType.InternalError)
                '    Else

                '    End If
                '    '*** read the Table Attributes
                '    For Each anAttribute In aDescriptor.TableAttributes
                '        '** Schema Definition
                '        tableIDs.Add(anAttribute.TableName)
                '        tableAttrIds.Add(anAttribute.ID)
                '        tableVersions.Add(anAttribute.Version)
                '        tableAttrDeleteFlags.Add(anAttribute.AddDeleteFieldBehavior)
                '        tableAttrSpareFieldsFlags.Add(anAttribute.AddSpareFields)
                '        tableAttrDomainIDFlags.Add(anAttribute.AddDomainBehavior)
                '    Next anAttribute

                '    '***
                '    '*** retrieve all ColumnAttributes and convert to field descriptors
                '    For Each anAttribute In aDescriptor.ObjectEntryAttributes
                '        With anAttribute
                '            Dim anOTDBFieldDesc As New ormFieldDescription
                '            anOTDBFieldDesc.ColumnName = anAttribute.ColumnName
                '            '*** REFERENCE OBJECT ENTRY
                '            If anAttribute.HasValueReferenceObjectEntry Then
                '                Debug.WriteLine(anAttribute)
                '            End If

                '            '** Take Object Values
                '            If .HasValueID Then
                '                anOTDBFieldDesc.ID = .ID
                '            Else : anOTDBFieldDesc.ID = ""
                '            End If
                '            If .HasValueTitle Then
                '                anOTDBFieldDesc.Title = .Title
                '            Else : anOTDBFieldDesc.Title = ""
                '            End If
                '            If .HasValueRelation Then
                '                anOTDBFieldDesc.Relation = .Relation
                '            Else : anOTDBFieldDesc.Relation = {}
                '            End If
                '            If .HasValueAliases Then
                '                anOTDBFieldDesc.Aliases = .Aliases
                '            Else : anOTDBFieldDesc.Aliases = {}
                '            End If
                '            If .HasValueIsNullable Then
                '                anOTDBFieldDesc.IsNullable = .IsNullable
                '            Else : anOTDBFieldDesc.IsNullable = False
                '            End If
                '            If .HasValueTypeID Then
                '                anOTDBFieldDesc.Datatype = .Typeid
                '            Else : anOTDBFieldDesc.Datatype = otFieldDataType.Text
                '            End If

                '            If .HasValueParameter Then
                '                anOTDBFieldDesc.Parameter = .Parameter
                '            Else : anOTDBFieldDesc.Parameter = ""
                '            End If

                '            If .HasValueSize Then
                '                anOTDBFieldDesc.Size = .Size
                '            Else : anOTDBFieldDesc.Size = 0
                '            End If

                '            If .HasValueDescription Then
                '                anOTDBFieldDesc.Description = .Description
                '            Else : anOTDBFieldDesc.Description = ""
                '            End If

                '            If .DefaultValue IsNot Nothing Then
                '                anOTDBFieldDesc.DefaultValue = .DefaultValue
                '            Else : anOTDBFieldDesc.DefaultValue = ""
                '            End If

                '            If .HasValueVersion Then
                '                anOTDBFieldDesc.Version = .Version
                '            Else : anOTDBFieldDesc.Version = 1
                '            End If

                '            If .HasValueSpareFieldTag Then
                '                anOTDBFieldDesc.SpareFieldTag = .SpareFieldTag
                '            Else : anOTDBFieldDesc.SpareFieldTag = False
                '            End If

                '            '** ordinal position given or by the way they are coming
                '            If .hasValuePosOrdinal Then
                '                anOTDBFieldDesc.ordinalPosition = ordinalPos
                '                ordinalPos += 1
                '            Else
                '                anOTDBFieldDesc.ordinalPosition = .Posordinal
                '            End If


                '            '** add the field
                '            fieldDescs.Add(anOTDBFieldDesc)

                '            If .HasValueKeyOrdinal Then
                '                If primaryKeyList.ContainsKey(.KeyOrdinal) Then
                '                    Call CoreMessageHandler(subname:="ormDataObject.CreateSchema(of T)", message:="Primary key member has a position number more than once", _
                '                                           arg1:=anOTDBFieldDesc.ColumnName, messagetype:=otCoreMessageType.InternalError)
                '                    Return False
                '                End If
                '                primaryKeyList.Add(.KeyOrdinal, anOTDBFieldDesc.ColumnName)
                '            End If
                '        End With

                '    Next

                '    '**** Index
                '    '****
                '    For Each anAttribute In aDescriptor.IndexAttributes
                '        Dim theColumnNames As String() = anAttribute.ColumnNames
                '        Dim theIndexname As String = anAttribute.IndexName

                '        If indexList.ContainsKey(theIndexname) Then
                '            indexList.Remove(theIndexname)
                '        End If
                '        indexList.Add(key:=theIndexname, value:=theColumnNames)
                '    Next

                '    Dim I As ULong = 0
                '    '*** create the table with schema entries
                '    '***
                '    For Each aTableID In tableIDs
                '        Dim aObjectDefinition As New ObjectDefinition

                '        With aObjectDefinition
                '            .Create(aTableID, checkunique:=Not addToSchema, runTimeOnly:=Not addToSchema, version:=tableVersions(I))
                '            '** delete the schema
                '            If addToSchema Then .Delete()
                '            .DomainID = CurrentSession.CurrentDomainID
                '            .Version = tableVersions(I)
                '            '* set table specific attributes
                '            If tableAttrSpareFieldsFlags(I) Then
                '                .SpareFieldsBehavior = True
                '            Else
                '                .SpareFieldsBehavior = False
                '            End If
                '            If tableAttrDeleteFlags(I) Then
                '                .DeletePerFlagBehavior = True
                '            Else
                '                .DeletePerFlagBehavior = False
                '            End If
                '            If tableAttrDomainIDFlags(I) Then
                '                .DomainBehavior = True
                '            Else
                '                .DomainBehavior = False
                '            End If

                '            '** create the the fields
                '            For Each aFieldDesc In fieldDescs
                '                aFieldDesc.Tablename = aTableID ' set here
                '                Call .AddEntry(fielddesc:=aFieldDesc)
                '            Next

                '            ' create primary key
                '            Dim aCollection As New Collection
                '            For Each key In primaryKeyList.Keys
                '                aCollection.Add(primaryKeyList.Item(key))
                '            Next
                '            Call .AddIndex("PrimaryKey", aCollection, isprimarykey:=True)

                '            ' create additional index
                '            For Each kvp As KeyValuePair(Of String, String()) In indexList
                '                ' Index
                '                Dim anIndexCollection As New Collection
                '                For Each fieldname As String In kvp.Value
                '                    anIndexCollection.Add(fieldname)
                '                Next
                '                .AddIndex(indexname:=kvp.Key, fieldnames:=anIndexCollection, isprimarykey:=False)
                '            Next
                '            ' persist
                '            'If addToSchema Then .Persist()
                '            ' change the database
                '            .CreateObjectSchema(addToSchema:=addToSchema)
                '            If addToSchema Then .Persist()
                '            '** fire event
                '            ourEventArgs = New ormDataObjectEventArgs([object]:=aObjectDefinition)
                '            RaiseEvent OnSchemaCreated(Nothing, e:=ourEventArgs)

                '        End With


                '        '* reload the tablestore
                '        If CurrentSession.IsRunning Then
                '            CurrentSession.CurrentDBDriver.GetTableStore(tableID:=aTableID, force:=True)
                '        End If

                '        '** now try to persist
                '        If Not addToSchema Then
                '            aObjectDefinition.Delete()
                '            aObjectDefinition.Persist()
                '        End If
                '        '* success
                '        Call CoreMessageHandler(messagetype:=otCoreMessageType.ApplicationInfo, message:="The schema for " & aTableID & " is updated", _
                '                               subname:="ormDataObject.createSchema(of T)")
                '        I = I + 1
                '    Next

                '    Return True
                'Catch ex As Exception

                '    Call CoreMessageHandler(subname:="ormDataObject.CreateSchema(of T)", exception:=ex)
                '    Return False

                'End Try



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
            ''' create a persistable dataobject of type T 
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="pkArray"></param>
            ''' <param name="checkUnique"></param>
            ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
            ''' <remarks></remarks>
            Protected Shared Function CreateDataObject(Of T As {iormInfusable, iormPersistable, New}) _
                                (ByRef pkArray() As Object,
                                 Optional domainID As String = "",
                                 Optional checkUnique As Boolean = False, _
                                 Optional runtimeOnly As Boolean = False) As iormPersistable
                Dim aDataObject As New T
                If aDataObject.Create(pkArray, domainID:=domainID, runTimeonly:=runtimeOnly, checkUnique:=checkUnique) Then
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
            Protected Shared Function CreateDataObject(Of T As {iormInfusable, iormPersistable, New}) _
                                (ByRef record As ormRecord,
                                 Optional domainID As String = "",
                                 Optional checkUnique As Boolean = False, _
                                 Optional runtimeOnly As Boolean = False) As iormPersistable
                Dim aDataObject As New T
                If aDataObject.Create(record, domainID:=domainID, runtimeOnly:=runtimeOnly, checkUnique:=checkUnique) Then
                    Return aDataObject
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' copy the Primary key to the record
            ''' </summary>
            ''' <param name="pkArray"></param>
            ''' <remarks></remarks>
            Private Function CopyPrimaryKeyToRecord(ByRef pkArray() As Object, ByRef record As ormRecord,
                                                    Optional domainID As String = "", _
                                                    Optional runtimeOnly As Boolean = False) As Boolean
                Dim aList As List(Of String)
                If Not runtimeOnly Then
                    aList = Me.TableSchema.PrimaryKeys 'take it from the real schema
                Else
                    Dim aDescriptor As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                    If aDescriptor IsNot Nothing Then
                        aList = aDescriptor.Keynames.ToList
                    Else
                        CoreMessageHandler(message:="no object class description found", objectname:=Me.ObjectID, subname:="ormDataObject.CopyPrimaryKeyToRecord", _
                                           messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                End If
                Dim i As UShort = 0
                If domainID = "" Then domainID = ConstGlobalDomain
                ReDim Preserve pkArray(aList.Count - 1)
                For Each acolumnname In aList
                    If (record.IsTableBound AndAlso record.HasIndex(acolumnname)) OrElse Not record.IsTableBound Then
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
            ''' helper substitutes the DomainID in the primary key
            ''' </summary>
            ''' <param name="pkarray"></param>
            ''' <param name="runtimeOnly"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function SubstituteDomainIDinPKArray(ByRef pkarray As Object(), domainid As String, Optional runtimeOnly As Boolean = False) As Boolean
                Dim domindex As Integer = -1

                ''** offer 2 possibilites either by schema if rnot runtime
                '*** or by ObjectClassDescription on runtimeOnly
                If Not runtimeOnly Then
                    domindex = Me.TableSchema.GetDomainIDPKOrdinal
                    If domindex > 0 Then
                        If domainid = "" Then domainid = CurrentSession.CurrentDomainID
                        If pkarray.Count = Me.TableSchema.NoPrimaryKeyFields Then
                            pkarray(domindex - 1) = UCase(domainid)
                        Else
                            ReDim Preserve pkarray(Me.TableSchema.NoPrimaryKeyFields)
                            pkarray(domindex - 1) = UCase(domainid)
                        End If
                    Else
                        CoreMessageHandler(message:="domainID is not in primary key although domain behavior is set", subname:="ormDataObject.SubstituteDomainIDinPKArray", _
                                           arg1:=domainid, tablename:=Me.TableID, objectname:=Me.ObjectID, columnname:=ConstFNDomainID, messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                    Return True
                Else
                    Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescription(Me.GetType)
                    If anObjectDescription IsNot Nothing Then
                        Dim keynames As String() = anObjectDescription.Keynames
                        domindex = Array.FindIndex(keynames, Function(s) s.ToLower = Domain.ConstFNDomainID.ToLower)
                        If domindex >= 0 Then
                            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
                            If pkarray.Count = keynames.Count Then
                                pkarray(domindex - 1) = UCase(domainid)
                            Else
                                ReDim Preserve pkarray(keynames.Count)
                                pkarray(domindex - 1) = UCase(domainid)
                            End If
                        Else
                            CoreMessageHandler(message:="domainID is not in primary key although domain behavior is set", subname:="ormDataObject.SubstituteDomainIDinPKArray", _
                                         arg1:=domainid, tablename:=Me.TableID, objectname:=Me.ObjectID, columnname:=ConstFNDomainID, messagetype:=otCoreMessageType.InternalError)
                            Return False
                        End If
                    Else
                        CoreMessageHandler(message:="an objectclassdescription couldnot be retrieved", subname:="ormDataObject.SubstituteDomainIDinPKArray", _
                                           arg1:=domainid, tablename:=Me.TableID, objectname:=Me.ObjectID, columnname:=ConstFNDomainID, messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                End If

            End Function

            ''' <summary>
            ''' helper for checking the uniqueness during creation
            ''' </summary>
            ''' <param name="pkarray"></param>
            ''' <param name="runtimeOnly"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function CheckUniqueness(pkarray As Object(), Optional runtimeOnly As Boolean = False) As Boolean

                '*** Check on Not Runtime
                If Not runtimeOnly Then
                    Dim aRecord As ormRecord
                    '* fire Event
                    Dim ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=pkarray)
                    RaiseEvent OnCheckingUniqueness(Me, ourEventArgs)

                    '* skip
                    If ourEventArgs.Proceed Then
                        ' Check
                        Dim aStore As iormDataStore = Me.TableStore
                        aRecord = aStore.GetRecordByPrimaryKey(pkarray)

                        '* not found
                        If aRecord IsNot Nothing Then
                            If Me.HasDeletePerFlagBehavior Then
                                If aRecord.HasIndex(ConstFNIsDeleted) Then
                                    If CBool(aRecord.GetValue(ConstFNIsDeleted)) Then
                                        CoreMessageHandler(message:="deleted (per flag) object found - use undelete instead of create", messagetype:=otCoreMessageType.ApplicationWarning, _
                                                            arg1:=pkarray, tablename:=Me.TableID)
                                        Return False
                                    End If
                                End If
                            Else
                                Return False
                            End If

                        Else
                            Return True ' unqiue
                        End If

                        Return True
                    Else
                        '** abort if Event brought not unique
                        If Not ourEventArgs.Result Then
                            Return False
                        End If
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
            Private Function ExtractPrimaryKey(record As ormRecord, Optional runtimeOnly As Boolean = False) As Object()
                Dim pknames As String()
                '*** extract the primary keys from record
                If runtimeOnly Then
                    Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescription(Me.GetType)
                    If anObjectDescription IsNot Nothing Then
                        pknames = anObjectDescription.Keynames
                    Else
                        CoreMessageHandler(message:="ObjectDescriptor not found", objectname:=Me.ObjectID, arg1:=Me.GetType.Name, _
                                            subname:="ormDataobject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalError)
                        Return {}
                    End If
                Else
                    '* keynames of the object
                    pknames = Me.ObjectDefinition.GetKeyNames.ToArray
                    If pknames.Count = 0 Then pknames = Me.TableSchema.PrimaryKeys.ToArray
                End If

                '** get the 
                Dim pkarray As Object()
                ReDim pkarray(pknames.Length - 1)
                Dim i As UShort = 0
                For Each aName In pknames
                    If record.HasIndex(aName) Then
                        pkarray(i) = record.GetValue(index:=aName)
                        If Not SetValue(entryname:=aName, value:=pkarray(i)) Then
                            'CoreMessageHandler(message:="value in in primary key could not set", arg1:=pkarray(i), entryname:=aName, objectname:=Me.ObjectID, _
                            '                   subname:="ormDataoBject.ExtractPrimaryKey", messagetype:=otCoreMessageType.ApplicationError)
                        End If
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
            Protected Overridable Function Create(ByRef record As ormRecord, _
                                                  Optional domainID As String = "", _
                                                  Optional checkUnique As Boolean = False, _
                                                  Optional runtimeOnly As Boolean = False) As Boolean Implements iormPersistable.Create

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
                       Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainID, _
                                                                    objectoperations:={Me.ObjectID & "." & ConstOPCreate}) Then
                    If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadUpdateData, _
                                                             messagetext:="Please provide another user to authorize requested operation", _
                                                            domainID:=domainID, objectoperations:={Me.ObjectID & "." & ConstOPCreate}) Then
                        Call CoreMessageHandler(message:="data object cannot be created - permission denied to user", _
                                                objectname:=Me.ObjectID, arg1:=ConstOPCreate, _
                                                messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    End If
                End If

                '**
                Dim pkarray As Object()
                '** domainid
                If domainID = "" Then domainID = ConstGlobalDomain

                '* extract the primary key
                pkarray = ExtractPrimaryKey(record, runtimeOnly:=runtimeOnly)
                '** check for domainBehavior
                If Me.HasDomainBehavior Then
                    SubstituteDomainIDinPKArray(pkarray:=pkarray, domainid:=domainID, runtimeOnly:=runtimeOnly)
                End If

                '** fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=record, pkarray:=pkarray)
                RaiseEvent ClassOnCreating(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                Else
                    pkarray = ourEventArgs.Pkarray
                    record = ourEventArgs.Record
                End If

                '** keys must be set in the object itself
                '** create
                If checkUnique AndAlso Not CheckUniqueness(pkarray:=pkarray, runtimeOnly:=runtimeOnly) Then
                    Return False '* not unique
                End If

                '** set on the runtime Only Flag
                If runtimeOnly Then SwitchRuntimeON()

                '** the infuse mode
                Me.CurrentInfuseMode = otInfuseMode.OnCreate
                '** infuse what we have
                Me.Infuse(record)
                '** reset the infuse mode
                Me.CurrentInfuseMode = otInfuseMode.OnDefault

                '** set status
                _domainID = domainID
                _IsCreated = True
                _IsDeleted = False
                _deletedOn = ConstNullDate
                _IsLoaded = False
                _IsChanged = False

                '* fire Event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, pkarray:=pkarray)
                ourEventArgs.UseCache = True
                RaiseEvent ClassOnCreated(Me, ourEventArgs)
                If ourEventArgs.UseCache Then
                    Cache.RegisterCacheFor(Me.ObjectID)
                    Cache.AddToCache(objectTag:=Me.ObjectID, key:=pkarray, theOBJECT:=Me)
                End If
                Return ourEventArgs.Result

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
                                                  Optional domainID As String = "", _
                                                  Optional checkUnique As Boolean = False, _
                                                  Optional runtimeOnly As Boolean = False) As Boolean Implements iormPersistable.Create

                Dim aRecord As New ormRecord
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
                If domainID = "" Then domainID = ConstGlobalDomain

                '** copy the primary keys
                CopyPrimaryKeyToRecord(pkArray:=pkArray, record:=aRecord, domainID:=domainID, runtimeOnly:=runtimeOnly)

                Return Create(record:=aRecord, domainID:=domainID, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
            End Function
            ''' <summary>
            ''' clone a dataobject with a new pkarray. return nothing if fails
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="cloneobject"></param>
            ''' <param name="newpkarray"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function CloneDataObject(Of T As {iormPersistable, iormCloneable, iormInfusable, New})(cloneobject As iotCloneable(Of T), newpkarray As Object()) As T
                Return cloneobject.Clone(newpkarray)
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
                 Optional domainID As String = "", _
                 Optional dbdriver As iormDatabaseDriver = Nothing, _
                 Optional forceReload As Boolean = False, _
                 Optional runtimeOnly As Boolean = False) As T

                Dim anObject As New T
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
                                                                    objectoperations:={anObject.ObjectID & "." & ConstOPInject}) Then
                    '** request authorizartion
                    If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainID:=domainID, _
                                                                                username:=CurrentSession.Username, _
                                                                                objectoperations:={anObject.ObjectID & "." & ConstOPInject}) Then
                        Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                                objectname:=anObject.ObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                                subname:="ormDataObject.Retrieve", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If

                Dim hasDomainBehavior As Boolean = anObject.HasDomainBehavior
                If domainID = "" Then domainID = CurrentSession.CurrentDomainID
                '** check for domainBehavior
                If hasDomainBehavior Then
                    anObject.SubstituteDomainIDinPKArray(pkarray:=pkArray, domainid:=domainID, runtimeOnly:=runtimeOnly)
                End If

                '* fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(anObject, pkArray:=pkArray)
                ourEventArgs.UseCache = True ' default
                RaiseEvent ClassOnRetrieving(Nothing, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Return ourEventArgs.DataObject
                    Else
                        Return Nothing
                    End If
                End If

                '* use Cache
                If ourEventArgs.UseCache Then
                    anObject = Cache.LoadFromCache(objecttag:=anObject.ObjectID, key:=pkArray)
                    '* Domain Behavior - is global cached but it might be that we are missing the domain related one if one has been created
                    '* after load of the object - since not in cache
                    If anObject Is Nothing AndAlso hasDomainBehavior AndAlso domainID <> ConstGlobalDomain Then
                        anObject.SubstituteDomainIDinPKArray(pkarray:=pkArray, domainid:=ConstGlobalDomain, runtimeOnly:=runtimeOnly)
                        anObject = Cache.LoadFromCache(objecttag:=anObject.TableID, key:=pkArray)
                    End If
                End If


                '* load object if not runtime only
                If (anObject Is Nothing OrElse forceReload) And Not runtimeOnly Then
                    anObject = ormDataObject.InjectDataObject(Of T)(pkArray:=pkArray, domainID:=domainID, dbdriver:=dbdriver)
                    If Not anObject Is Nothing AndAlso ourEventArgs.UseCache Then
                        Cache.RegisterCacheFor(anObject.ObjectID)
                        Cache.AddToCache(objectTag:=anObject.ObjectID, key:=pkArray, theOBJECT:=anObject)
                    End If

                End If

                '* fire event
                If anObject IsNot Nothing Then
                    ourEventArgs = New ormDataObjectEventArgs(anObject, record:=anObject.Record)
                Else
                    ourEventArgs = New ormDataObjectEventArgs(Nothing, record:=Nothing)
                End If

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
            ''' 
            ''' <summary>
            ''' clone the object with the new primary key
            ''' </summary>
            ''' <param name="pkarray">primary key array</param>
            ''' <remarks></remarks>
            ''' <returns>the new cloned object or nothing</returns>
            Public Overloads Function Clone(Of T As {iormPersistable, iormInfusable, Class, New})(newpkarray As Object()) As T Implements iormCloneable.Clone
                '
                '*** now we copy the object
                Dim aNewObject As New T
                Dim newRecord As New ormRecord

                '**
                If Not Me.IsAlive(subname:="clone") Then
                    Return Nothing
                End If

                '* init
                If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                    Return Nothing
                End If

                '* fire event
                Dim ourEventArgs As New ormDataObjectEventArgs(TryCast(aNewObject, ormDataObject), record:=Me.Record, pkarray:=newpkarray)
                RaiseEvent OnCloning(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    If ourEventArgs.Result Then
                        Dim aDataobject = TryCast(ourEventArgs.DataObject, T)
                        If aDataobject IsNot Nothing Then
                            Return aDataobject
                        Else
                            CoreMessageHandler(message:="OnCloning: cannot convert persistable to class", arg1:=GetType(T).Name, subname:="ormDataObject.Clone(of T)", _
                                               messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                End If

                ' set it
                If Not Me.RunTimeOnly Then newRecord.SetTable(Me.TableID)

                ' go through the table and overwrite the Record if the rights are there
                For Each keyname In Me.Record.Keys
                    If keyname <> ConstFNCreatedOn And keyname <> ConstFNUpdatedOn And keyname <> ConstFNIsDeleted And keyname <> ConstFNDeletedOn Then
                        Call newRecord.SetValue(keyname, Me.Record.GetValue(keyname))
                    End If
                Next keyname

                If Not aNewObject.Create(pkArray:=newpkarray, checkUnique:=True) Then
                    Call CoreMessageHandler(message:="object new keys are not unique - clone aborted", arg1:=newpkarray, tablename:=_primaryTableID, _
                                           messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                ' actually here it we should clone all members too !
                If aNewObject.Infuse(newRecord) Then
                    '** Fire Event
                    ourEventArgs = New ormDataObjectEventArgs(TryCast(aNewObject, ormDataObject), record:=aNewObject.Record, pkarray:=newpkarray)
                    ourEventArgs.Result = True
                    ourEventArgs.Proceed = True
                    RaiseEvent OnCloned(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        If Not ourEventArgs.Result Then
                            Return Nothing
                        End If
                    End If
                    Dim aDataobject = TryCast(ourEventArgs.DataObject, T)
                    If aDataobject IsNot Nothing Then
                        Return aDataobject
                    Else
                        CoreMessageHandler(message:="OnCloned: cannot convert persistable to class", arg1:=GetType(T).Name, _
                                           subname:="ormDataObject.Clone(of T)", _
                                           messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' load the relations and infuses the values in the mapped members
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function InfuseRelation(id As String) As Boolean
                Try
                    If Not Me.IsAlive(subname:="InfuseRelation") Then Return False
                    Dim aDescriptor As ObjectClassDescription = Me.ObjectClassDescription
                    Dim result As Boolean = InfuseRelationMapped(dataobject:=Me, classdescriptor:=aDescriptor, _
                                                         mode:=otInfuseMode.OnDemand, relationid:=id)
                    Return result
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormDataObject.loadRelations")
                    Me.CurrentInfuseMode = otInfuseMode.OnDefault
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
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record)
                RaiseEvent OnUnDeleting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                End If

                '* undelete if possible
                Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.DeletePerFlagBehavior Then
                    _IsDeleted = False
                    _deletedOn = ConstNullDate
                    '* fire event
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record)
                    ourEventArgs.Result = True
                    ourEventArgs.Proceed = True
                    RaiseEvent OnUnDeleted(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        Return ourEventArgs.Result
                    End If
                    If ourEventArgs.Result Then
                        CoreMessageHandler(message:="data object undeleted", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                            tablename:=Me.TableID)
                        Return True
                    Else
                        CoreMessageHandler(message:="data object cannot be undeleted by event - delete per flag behavior not set", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                         tablename:=Me.TableID)
                        Return False
                    End If

                Else
                    CoreMessageHandler(message:="data object cannot be undeleted - delete per flag behavior not set", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                         tablename:=Me.TableID)
                    Return False
                End If


            End Function
            ''' <summary>
            ''' Delete the object and its persistancy
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Delete() As Boolean Implements iormPersistable.Delete
                '** initialize
                If Not Me.IsInitialized AndAlso Not Me.Initialize Then Return False
                '** check on the operation right for this object
                If Not RunTimeOnly AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadUpdateData, _
                                                                                   domainid:=DomainID, _
                                                                                    objectoperations:={Me.ObjectID & "." & ConstOPDelete}) Then

                    If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, username:=CurrentSession.Username, _
                                                            domainID:=DomainID, _
                                                             messagetext:="Please provide another user to authorize requested operation", _
                                                             objectoperations:={Me.ObjectID & "." & ConstOPDelete}) Then
                        Call CoreMessageHandler(message:="data object cannot be deleted - permission denied to user", _
                                                objectname:=Me.ObjectID, arg1:=ConstOPDelete, username:=CurrentSession.Username, _
                                                subname:="ormDataObject.Delete", messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    End If
                End If

                '** Fire Event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=Me.Record)
                RaiseEvent ClassOnDeleting(Me, ourEventArgs)
                RaiseEvent OnDeleting(Me, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                End If

                '*** cascade the operation through the related members
                Dim result As Boolean = CascadeRelation(Me, Me.ObjectClassDescription, cascadeUpdate:=False, cascadeDelete:=True)

                '** determine how to delete
                Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                '** per flag
                If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.DeletePerFlagBehavior Then
                    _IsDeleted = True
                    _deletedOn = Date.Now()
                    Me.Persist()
                Else
                    'delete the  object itself
                    If Not Me.RunTimeOnly Then _IsDeleted = _record.Delete()
                    If _IsDeleted Then
                        Me.Unload()
                        _deletedOn = Date.Now()
                    End If

                End If

                '** fire Event
                ourEventArgs.Result = _IsDeleted
                RaiseEvent OnDeleted(Me, ourEventArgs)
                RaiseEvent ClassOnDeleted(Me, ourEventArgs)
                _IsDeleted = ourEventArgs.Result
                Return _IsDeleted
            End Function
            ''' <summary>
            ''' infuse a data objects objectentry column mapped members
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Shared Function InfuseColumnMapping(ByRef dataobject As iormPersistable, ByRef record As ormRecord, mode As otInfuseMode, _
                                                        Optional ByRef classdescriptor As ObjectClassDescription = Nothing) As Boolean
                '** Fire Event
                Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, record:=record)
                RaiseEvent OnColumnMappingInfusing(dataobject, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                Else
                    dataobject = ourEventArgs.DataObject
                    record = ourEventArgs.Record
                End If
                Dim objectentryname As String
                If classdescriptor Is Nothing Then classdescriptor = dataobject.ObjectClassDescription

                '*** infuse each mapped column to member
                '*** if it is in the record
                Try
                    
                    For Each aColumnName In classdescriptor.MappedColumnNames
                        Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedColumnFieldInfos(columnname:=aColumnName)
                        For Each aField In aFieldList
                            Dim aMappingAttribute = classdescriptor.GetEntryMappingAttributes(aField.Name)
                            If (mode And aMappingAttribute.InfuseMode) Then
                                objectentryname = aMappingAttribute.EntryName
                                Dim isNull As Boolean
                                Dim aValue As Object
                                If record.HasIndex(aColumnName) Then
                                    '*** set the class internal field
                                    aValue = record.GetValue(aColumnName, isNull:=isNull)

                                    If Not isNull AndAlso aValue IsNot Nothing Then
                                        If Not Reflector.SetFieldValue(field:=aField, dataobject:=dataobject, value:=aValue) Then
                                            CoreMessageHandler(message:="field value ob data object couldnot be set", _
                                                                objectname:=dataobject.ObjectID, subname:="ormDataObject.InfuseColumnMapping", _
                                                                messagetype:=otCoreMessageType.InternalError, entryname:=objectentryname, tablename:=dataobject.TableID)
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Next


                    '** Fire Event OnColumnMappingInfused
                    ourEventArgs = New ormDataObjectEventArgs(dataobject, record:=record)
                    ourEventArgs.Proceed = True
                    ourEventArgs.Result = True
                    RaiseEvent OnColumnMappingInfused(dataobject, ourEventArgs)
                    Return ourEventArgs.Result

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormDataObject.InfuseColumnMapping", exception:=ex, objectname:=dataobject.ObjectID, _
                                            entryname:=objectentryname, tablename:=dataobject.TableID)
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
                    aList = aDescriptor.Keynames.ToList
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
                                            tablename:=dataobject.TableID)
                    Return False

                End Try

            End Function

            ''' <summary>
            ''' Raise the Instance OnRelationLoading
            ''' </summary>
            ''' <param name="sender"></param>
            ''' <param name="e"></param>
            ''' <remarks></remarks>
            Private Sub RaiseOnRelationLoading(sender As Object, e As ormDataObjectEventArgs)
                RaiseEvent OnRelationLoading(sender, e)
            End Sub
            ''' <summary>
            ''' Raise the Instance OnRelationLoaded
            ''' </summary>
            ''' <param name="sender"></param>
            ''' <param name="e"></param>
            ''' <remarks></remarks>
            Private Sub RaiseOnRelationLoaded(sender As Object, e As ormDataObjectEventArgs)
                RaiseEvent OnRelationLoad(sender, e)
            End Sub
            ''' <summary>
            ''' infuse the relation mapped Members of a dataobject
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <param name="classdescriptor"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Shared Function InfuseRelationMapped(ByRef dataobject As iormPersistable, ByRef classdescriptor As ObjectClassDescription, _
                                                         mode As otInfuseMode, Optional relationid As String = "") As Boolean
                '* Fire Event OnRelationLoading
                Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, Nothing, relationID:=relationid)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = True
                RaiseEvent ClassOnRelationLoading(dataobject, ourEventArgs)
                dataobject = ourEventArgs.DataObject
                If Not ourEventArgs.Proceed Then Return ourEventArgs.Result

                Try

                    '*** Raise Event
                    DirectCast(dataobject, ormDataObject).RaiseOnRelationLoading(dataobject, ourEventArgs)
                    If Not ourEventArgs.Proceed Then Return ourEventArgs.Result

                    '***
                    '*** Fill in the relations
                    For Each aRelationAttribute In classdescriptor.RelationAttributes
                        '** run through specific event
                        If relationid = "" OrElse relationid.ToLower = aRelationAttribute.Name.ToLower Then
                            Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedRelationFieldInfos(relationName:=aRelationAttribute.Name)

                            For Each aFieldInfo In aFieldList
                                Dim aMappingAttribute = classdescriptor.GetEntryMappingAttributes(aFieldInfo.Name)
                                If (mode And aMappingAttribute.InfuseMode) Then
                                    '** get it by primary key and retrieve
                                    If aRelationAttribute.HasValueToPrimarykeys Then
                                        Dim anObject = Reflector.GetRelatedObjectByRetrieve(attribute:=aRelationAttribute, _
                                                                             dataobject:=dataobject, classdescriptor:=classdescriptor)
                                        If anObject IsNot Nothing Then
                                            '** setfieldvalue by hook or slooow
                                            If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=anObject) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                        message:="could not object mapped entry", _
                                                        arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.TableID)

                                            End If

                                        ElseIf aRelationAttribute.CascadeOnCreate Then
                                            anObject = Reflector.GetRelatedObjectByCreate(attribute:=aRelationAttribute, _
                                                                             dataobject:=dataobject, classdescriptor:=classdescriptor)
                                            '** setfieldvalue by hook or slooow
                                            If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=anObject) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                        message:="could not object mapped entry", _
                                                        arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.TableID)

                                            End If
                                        End If

                                        '** get the related objects by query somehow
                                    Else
                                        Dim aList As List(Of iormPersistable) = _
                                            Reflector.GetRelatedObjects(attribute:=aRelationAttribute, dataobject:=dataobject, classdescriptor:=classdescriptor)

                                        '** if array
                                        If aFieldInfo.FieldType.IsArray Then
                                            '** setfieldvalue by hook or slooow
                                            If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList.ToArray) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                                       message:="could not object mapped entry", _
                                                                       arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.TableID)

                                            End If

                                            '*** Lists
                                        ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IList)) Then
                                            '** setfieldvalue by hook or slooow
                                            If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                                        message:="could not object mapped entry", _
                                                                        arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.TableID)
                                            End If


                                            '*** Dictionary
                                        ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IDictionary)) Then
                                            Dim aDirectory As IDictionary = aFieldInfo.GetValue(dataobject)
                                            Dim typedef As Type() = aFieldInfo.FieldType.GetGenericArguments()

                                            '** create
                                            If aDirectory Is Nothing Then
                                                If aFieldInfo.FieldType.IsGenericType Then
                                                    Dim specifictype = aFieldInfo.FieldType.MakeGenericType(typedef)
                                                    aDirectory = Activator.CreateInstance(specifictype)
                                                Else
                                                    aDirectory = Activator.CreateInstance(aFieldInfo.FieldType)
                                                End If

                                                '** setfieldvalue by hook or slooow
                                                If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aDirectory) Then
                                                    Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                            message:="could not object mapped entry", _
                                                            arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.TableID)

                                                End If
                                            End If

                                            '** assign
                                            For Each anObject In aList
                                                If typedef(0) = GetType(String) Then
                                                    Dim aKey As String = ""
                                                    For i = 0 To aMappingAttribute.KeyEntries.Count - 1
                                                        If i > 0 Then aKey &= ConstDelimiter
                                                        aKey &= anObject.Record.GetValue(aMappingAttribute.KeyEntries(i)).ToString
                                                    Next
                                                    aDirectory.Add(key:=aKey, value:=anObject)
                                                ElseIf typedef(0).Equals(GetType(Int64)) And IsNumeric(anObject.Record.GetValue(aMappingAttribute.KeyEntries(0))) Then
                                                    Dim aKey As Long = CLng(anObject.Record.GetValue(aMappingAttribute.KeyEntries(0)))
                                                    aDirectory.Add(key:=aKey, value:=anObject)
                                                Else
                                                    Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", message:="cannot convert key to dicitionary from List of iormpersistables", _
                                                                            objectname:=dataobject.ObjectID, tablename:=dataobject.TableID)
                                                End If
                                            Next

                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next

                    '* Fire Event OnRelationLoading
                    ourEventArgs = New ormDataObjectEventArgs(dataobject, Nothing, , relationID:=relationid)
                    ourEventArgs.Proceed = True
                    ourEventArgs.Result = True
                    '*** Raise Event
                    DirectCast(dataobject, ormDataObject).RaiseOnRelationLoaded(dataobject, ourEventArgs)
                    If Not ourEventArgs.Proceed Then Return ourEventArgs.Result

                    '* Fire Event OnRelationLoading
                    RaiseEvent ClassOnRelationLoaded(dataobject, ourEventArgs)
                    dataobject = ourEventArgs.DataObject
                    Return ourEventArgs.Result
                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", exception:=ex, objectname:=dataobject.ObjectID, _
                                            tablename:=dataobject.TableID)
                    Return False

                End Try

            End Function
            ''' <summary>
            ''' cascade the update of relational data
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <param name="classdescriptor"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Shared Function CascadeRelation(ByRef dataobject As iormPersistable, ByRef classdescriptor As ObjectClassDescription, _
                                                          cascadeUpdate As Boolean, cascadeDelete As Boolean, Optional relationid As String = "") As Boolean
                '* Fire Event OnRelationLoading
                Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, Nothing, relationID:=relationid)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = True
                RaiseEvent ClassOnRelationLoading(dataobject, ourEventArgs)
                dataobject = ourEventArgs.DataObject
                If Not ourEventArgs.Proceed Then Return ourEventArgs.Result


                Try
                    SyncLock dataobject
                        '***
                        '*** Fill in the relations
                        For Each aRelationAttribute In classdescriptor.RelationAttributes
                            
                            '** run through specific relation condition 
                            If (relationid = "" OrElse relationid.ToLower = aRelationAttribute.Name.ToLower) And _
                                ((cascadeUpdate AndAlso cascadeUpdate = aRelationAttribute.CascadeOnUpdate) OrElse _
                                 (cascadeDelete AndAlso cascadeDelete = aRelationAttribute.CascadeOnDelete)) Then
                                '* get the list
                                Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedRelationFieldInfos(relationName:=aRelationAttribute.Name)

                                For Each aFieldInfo In aFieldList
                                    Dim aMappingAttribute = classdescriptor.GetEntryMappingAttributes(aFieldInfo.Name)

                                    '** if direct persistable
                                    If aFieldInfo.FieldType.GetInterfaces().Contains(GetType(iormPersistable)) Then

                                        Dim anobject As Object
                                        '** get value 
                                        If Not Reflector.GetFieldValue(aFieldInfo, dataobject, anobject) Then
                                            anobject = aFieldInfo.GetValue(dataobject)
                                        End If

                                        Dim ansubdataobject = TryCast(anobject, iormPersistable)
                                        If ansubdataobject IsNot Nothing Then
                                            If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                                '** persist
                                                ansubdataobject.Persist()
                                            ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                                '** persist
                                                ansubdataobject.Delete()
                                            End If
                                        Else
                                            CoreMessageHandler(message:="mapped field in data object does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                               messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                            Return False
                                        End If

                                        '** get the related objects by query somehow
                                    Else
                                        '** and Dicitionary
                                        If aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IDictionary)) Then
                                            Dim aDictionary As IDictionary
                                            '** get values either by hook or by slow getvalue
                                            If Not Reflector.GetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aDictionary) Then
                                                aDictionary = aFieldInfo.GetValue(dataobject)
                                            End If
                                            For Each anEntry In aDictionary.Values
                                                Dim anSubdataObject As iormPersistable = TryCast(anEntry, iormPersistable)
                                                If anSubdataObject IsNot Nothing Then
                                                    If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                                        '** persist
                                                        anSubdataObject.Persist()
                                                    ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                                        '** persist
                                                        anSubdataObject.Delete()
                                                    End If
                                                Else
                                                    CoreMessageHandler(message:="mapped inner field in dictionary object of type enumerable does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                               messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                                    Return False
                                                End If
                                            Next

                                            '** run through the enumerables and try to cascade
                                        ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) Then
                                            Dim aList As IEnumerable
                                            '** get values either by hook or by slow getvalue
                                            If Not Reflector.GetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList) Then
                                                aList = aFieldInfo.GetValue(dataobject)
                                            End If
                                            If aList Is Nothing Then
                                                CoreMessageHandler(message:="mapped inner field in container object of type enumerable is not initialized in class", subname:="ormDataObject.CascadeRelation", _
                                                                   messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                                Return False
                                            Else
                                                For Each anEntry In aList
                                                    Dim anSubdataObject As iormPersistable = TryCast(anEntry, iormPersistable)
                                                    If anSubdataObject IsNot Nothing Then
                                                        If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                                            '** persist
                                                            anSubdataObject.Persist()
                                                        ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                                            '** persist
                                                            anSubdataObject.Delete()
                                                        End If
                                                    Else
                                                        CoreMessageHandler(message:="mapped inner field in container object of type enumerable does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                                   messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                                        Return False
                                                    End If
                                                Next
                                            End If
                                           

                                        Else
                                            CoreMessageHandler(message:="generic data handling container object neither of enumerable or dictionary", _
                                                                subname:="ormDataObject.CascadeRelation", messagetype:=otCoreMessageType.InternalError)
                                        End If

                                    End If

                                Next
                            End If
                        Next

                    End SyncLock

                    '* Fire Event OnRelationLoading
                    ourEventArgs = New ormDataObjectEventArgs(dataobject, Nothing, , relationID:=relationid)
                    ourEventArgs.Proceed = True
                    ourEventArgs.Result = True
                    RaiseEvent ClassOnRelationLoaded(dataobject, ourEventArgs)
                    dataobject = ourEventArgs.DataObject
                    Return ourEventArgs.Result
                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", exception:=ex, objectname:=dataobject.ObjectID, _
                                            tablename:=dataobject.TableID)
                    Return False

                End Try

            End Function
            ''' <summary>
            ''' infuse a dataobject by a record - use reflection
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <param name="record"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function InfuseDataObject(ByRef dataobject As iormPersistable, ByRef record As ormRecord, Optional mode As otInfuseMode = otInfuseMode.OnDefault) As Boolean


                '** Fire Event
                Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, record:=record)
                RaiseEvent ClassOnInfusing(dataobject, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                Else
                    dataobject = ourEventArgs.DataObject
                    record = ourEventArgs.Record
                End If

                Dim aDescriptor As ObjectClassDescription = dataobject.ObjectClassDescription
                If aDescriptor Is Nothing Then
                    CoreMessageHandler(message:="could not retrieve descriptor for business object class from core store", arg1:=dataobject.GetType.Name, _
                                        messagetype:=otCoreMessageType.InternalError, subname:="ormDataObject.createSchema")
                    Return False
                End If


                '*** INFUSE THE COLUMN MAPPED MEMBERS
                Dim aResult As Boolean = InfuseColumnMapping(dataobject:=dataobject, record:=record, classdescriptor:=aDescriptor, mode:=mode)

                '*** INFUSE THE RELATION MAPPED MEMBERS

                aResult = aResult And InfuseRelationMapped(dataobject:=dataobject, classdescriptor:=aDescriptor, mode:=mode)

                '** Fire Event OnColumnMappingInfused
                ourEventArgs = New ormDataObjectEventArgs(dataobject, record:=record)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = aResult
                RaiseEvent ClassOnInfused(dataobject, ourEventArgs)
                Return ourEventArgs.Result



            End Function

            ''' <summary>
            ''' Feed the record belonging to the data object
            ''' </summary>
            ''' <returns>True if successful</returns>
            ''' <remarks></remarks>
            Public Function FeedRecord(Optional record As ormRecord = Nothing) As Boolean Implements iormPersistable.feedrecord

                Dim classdescriptor As ObjectClassDescription = Me.ObjectClassDescription
                Dim result As Boolean = True

                '** defaultvalue
                If record Is Nothing Then record = Me.Record

                '** Fire Class Event
                Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=record)
                RaiseEvent ClassOnRecordFeeding(Nothing, ourEventArgs)
                If ourEventArgs.AbortOperation Then
                    Return ourEventArgs.Result
                Else
                    record = ourEventArgs.Record
                End If
                '** Fire Event
                ourEventArgs = New ormDataObjectEventArgs(Me, record:=record)
                RaiseEvent OnRecordFeeding(Me, ourEventArgs)
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
                            If record.HasIndex(aColumnName) Then
                                If aField.FieldType.IsValueType OrElse aField.FieldType.Equals(GetType(String)) OrElse _
                                    aField.FieldType.IsArray OrElse aField.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) Then
                                    '** get the value by hook or slooow
                                    If Not Reflector.GetFieldValue(field:=aField, dataobject:=Me, value:=aValue) Then
                                        aValue = aField.GetValue(Me)
                                    End If

                                    '** convert into List
                                    If anEntryAttribute.Typeid = otFieldDataType.List Then
                                        If aValue IsNot Nothing Then aValue = Converter.Enumerable2String(aValue)

                                        '* 
                                    ElseIf aField.FieldType.IsArray OrElse _
                                        (aField.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) AndAlso Not aField.FieldType.Equals(GetType(String))) Then
                                        CoreMessageHandler(message:="field member is an array or list type but object entry attribute is not list - transfered to list presentation", objectname:=Me.ObjectID, columnname:=aColumnName, _
                                                       arg1:=aField.Name, entryname:=anEntryAttribute.EntryName, messagetype:=otCoreMessageType.InternalWarning, _
                                                       subname:="ormDataobject.feedRecord")
                                        aValue = Converter.Enumerable2String(aValue)
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
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=record)
                    ourEventArgs.Result = result
                    RaiseEvent OnRecordFedd(Nothing, ourEventArgs)
                    result = ourEventArgs.Result

                    '** Fire Class Event
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=record)
                    ourEventArgs.Result = result
                    RaiseEvent ClassOnRecordFed(Nothing, ourEventArgs)
                    Return ourEventArgs.Result

                Catch ex As Exception

                    Call CoreMessageHandler(subname:="ormDataObject.FeedRecord", exception:=ex, tablename:=Me.TableID, objectname:=Me.ObjectID)
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
                Return dataobject.feedrecord(record:=record)
            End Function
            ''' <summary>
            ''' infuses a data object by a record
            ''' </summary>
            ''' <param name="Record">a fixed ormRecord with the persistence data</param>
            ''' <returns>true if successful</returns>
            ''' <remarks>might be overwritten by class descendants but make sure that you call mybase.infuse</remarks>
            Public Overridable Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

                '* lazy init
                If Not Me.IsInitialized AndAlso Not Me.Initialize() Then Return False

                Try
                    '** Fire Event
                    Dim ourEventArgs As New ormDataObjectEventArgs(Me, record:=record)
                    ourEventArgs.Result = True
                    ourEventArgs.AbortOperation = False
                    RaiseEvent OnInfusing(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        Return ourEventArgs.Result
                    Else
                        record = ourEventArgs.Record
                    End If

                    Me.Record = record
                    If Not InfuseDataObject(dataobject:=Me, record:=Me.Record, mode:=Me.CurrentInfuseMode) Then
                        '** minimal program if we failed to infuse by reflection
                        If record.HasIndex(ConstFNUpdatedOn) Then
                            _updatedOn = CDate(record.GetValue(ConstFNUpdatedOn))
                        End If
                        If record.HasIndex(ConstFNCreatedOn) Then
                            _createdOn = CDate(record.GetValue(ConstFNCreatedOn))
                        End If
                        If record.HasIndex(ConstFNDeletedOn) Then
                            _createdOn = CDate(record.GetValue(ConstFNDeletedOn))
                        End If
                    End If

                    '** Fire Event
                    ourEventArgs = New ormDataObjectEventArgs(Me, record:=record)
                    ourEventArgs.Result = True
                    ourEventArgs.AbortOperation = False
                    RaiseEvent OnInfused(Me, ourEventArgs)
                    If ourEventArgs.AbortOperation Then
                        Return ourEventArgs.Result
                    Else
                        record = ourEventArgs.Record
                    End If
                    record.IsLoaded = ourEventArgs.Result
                    _IsLoaded = ourEventArgs.Result
                    Return _IsLoaded

                Catch ex As Exception
                    Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Infuse", _
                                           tablename:=Me.TableID, messagetype:=otCoreMessageType.InternalException)
                    Return False
                End Try


            End Function

        End Class


        ''' <summary>
        ''' Event Arguments for Data Object Events
        ''' </summary>
        ''' <remarks></remarks>

        Public Class ormDataObjectEventArgs
            Inherits EventArgs

            Private _Object As ormDataObject
            Private _Record As ormRecord
            Private _DescribedByAttributes As Boolean = False
            Private _UseCache As Boolean = False
            Private _pkarray As Object()
            Private _relationID As String = ""
            Private _Abort As Boolean = False
            Private _result As Boolean = True

            ''' <summary>
            ''' constructor
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub New([object] As ormDataObject, _
                           Optional record As ormRecord = Nothing, _
                           Optional describedByAttributes As Boolean = False, _
                            Optional relationID As String = "", _
                            Optional pkarray As Object() = Nothing)
                _Object = [object]
                _Record = record
                _relationID = relationID
                _DescribedByAttributes = describedByAttributes
                _pkarray = pkarray
                _result = True
                _Abort = False
            End Sub

            ''' <summary>
            ''' Gets or sets the relation ID.
            ''' </summary>
            ''' <value>The relation ID.</value>
            Public Property RelationID() As String
                Get
                    Return Me._relationID
                End Get
                Set(value As String)
                    Me._relationID = Value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the result.
            ''' </summary>
            ''' <value>The result.</value>
            Public Property Result() As Boolean
                Get
                    Return Me._result
                End Get
                Set(value As Boolean)
                    Me._result = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the pkarray.
            ''' </summary>
            ''' <value>The pkarray.</value>
            Public Property Pkarray() As Object()
                Get
                    Return Me._pkarray
                End Get
                Set(value As Object())
                    Me._pkarray = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the use cache.
            ''' </summary>
            ''' <value>The use cache.</value>
            Public Property UseCache() As Boolean
                Get
                    Return Me._UseCache
                End Get
                Set(value As Boolean)
                    Me._UseCache = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the abort.
            ''' </summary>
            ''' <value>The abort.</value>
            Public Property AbortOperation() As Boolean
                Get
                    Return Me._Abort
                End Get
                Set(value As Boolean)
                    Me._Abort = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets if to proceed.
            ''' </summary>
            ''' <value>The abort.</value>
            Public Property Proceed() As Boolean
                Get
                    Return Not Me._Abort
                End Get
                Set(value As Boolean)
                    Me._Abort = Not value
                    Me._result = value
                End Set
            End Property
            ''' <summary>
            ''' Gets the described by attributes.
            ''' </summary>
            ''' <value>The described by attributes.</value>
            Public ReadOnly Property DescribedByAttributes() As Boolean
                Get
                    Return Me._DescribedByAttributes
                End Get
            End Property

            ''' <summary>
            ''' Gets the record.
            ''' </summary>
            ''' <value>The record.</value>
            Public ReadOnly Property Record() As ormRecord
                Get
                    Return Me._Record
                End Get
            End Property

            ''' <summary>
            ''' Gets the object.
            ''' </summary>
            ''' <value>The object.</value>
            Public ReadOnly Property DataObject() As ormDataObject
                Get
                    Return Me._Object
                End Get
            End Property

        End Class

    End Namespace
End Namespace
