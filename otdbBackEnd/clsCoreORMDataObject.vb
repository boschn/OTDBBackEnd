
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA OBJECT CLASS
REM ***********
REM *********** Version: 2.0
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
    ''' abstract class for persistable OnTrack Data Objects
    ''' </summary>
    ''' <remarks>
    ''' Functional Design Principles
    ''' 1. a data object has a life cycle of initialized, created, loaded, deleted (must be set by derived classes)
    ''' 2. a data object is record bound
    ''' 3. a data object instance has a guid
    ''' 4. a data object has a domain id (might be overwritten by derived classes)
    ''' 5. a data object (derived class) has a object id and a class description
    ''' 6. a data object might be running in runtimeOnly mode (not persistable) -> mode might be also changed -> event raised
    ''' </remarks>
    Public MustInherit Class ormDataObject
        Implements iormDataObject
        Implements IDisposable

        ''' <summary>
        ''' guid as identity
        ''' </summary>
        ''' <remarks></remarks>
        Private _guid As Guid = Guid.NewGuid

        ''' <summary>
        ''' the Record
        ''' </summary>
        ''' <remarks></remarks>
        Protected WithEvents _record As ormRecord                   ' record to save persistency

        ''' <summary>
        ''' runtime only flag
        ''' </summary>
        ''' <remarks></remarks>
        Protected _RunTimeOnly As Boolean = False     'if Object is only kept in Memory (no persist, no Record according to table, no DBDriver necessary, no checkuniqueness)
        ''' <summary>
        ''' cache of the use cache property
        ''' </summary>
        ''' <remarks></remarks>
        Protected _useCache As Nullable(Of Boolean) 'cache variable of the ObjectDefinition.UseCache Property
        ''' <summary>
        ''' cache of the class description
        ''' </summary>
        ''' <remarks></remarks>
        Protected WithEvents _classDescription As ObjectClassDescription
        ''' <summary>
        ''' cache of the objectdefinition
        ''' </summary>
        ''' <remarks></remarks>
        Protected WithEvents _objectdefinition As ObjectDefinition
        ''' <summary>
        ''' primary key
        ''' </summary>
        ''' <remarks></remarks>
        Protected _primarykey As ormDatabaseKey

        ''' <summary>
        ''' IsInitialized flag
        ''' </summary>
        ''' <remarks></remarks>
        Protected _IsInitialized As Boolean = False 'true if initialized all internal members to run a persistable data object
        ''' <summary>
        ''' liefetime status and valiables
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Protected _isloaded As Boolean = False
        Protected _isCreated As Boolean = False   'true if created by .CreateXXX Functions
        Protected _IsChanged As Boolean = False  'true if has changed and persisted is needed to retrieve the object as it is now
        <ormObjectEntryMapping(EntryName:=ConstFNIsDeleted)> Protected _IsDeleted As Boolean = False

        ''' <summary>
        ''' Timestamps
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntryMapping(EntryName:=ConstFNUpdatedOn)> Protected _updatedOn As Nullable(Of Date)
        <ormObjectEntryMapping(EntryName:=ConstFNCreatedOn)> Protected _createdOn As Nullable(Of Date)
        <ormObjectEntryMapping(EntryName:=ConstFNDeletedOn)> Protected _deletedOn As Nullable(Of Date)
        Protected _changeTimeStamp As DateTime 'Internal Timestamp which is used if an entry is changed

        ''' <summary>
        ''' Domain ID
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
           title:="Domain", description:="domain of the business Object", _
           defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, _
           useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
           posordinal:=1000, _
           foreignkeyProperties:={ForeignKeyProperty.OnDelete & "(" & ForeignKeyActionProperty.Cascade & ")", _
                                   ForeignKeyProperty.OnUpdate & "(" & ForeignKeyActionProperty.Cascade & ")"})> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormObjectEntryMapping(EntryName:=ConstFNDomainID)> Protected _domainID As String = ConstGlobalDomain

        <ormObjectEntry(Datatype:=otDataType.Bool, isnullable:=True, posordinal:=1001, _
          title:="Ignore Domain", description:="flag if the domainValue is to be ignored -> look in global")> _
        Public Const ConstFNIsDomainIgnored As String = "domainignore"

        <ormObjectEntryMapping(EntryName:=ConstFNIsDomainIgnored)> Protected _DomainIsIgnored As Boolean = False

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
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnSwitchRuntimeOn(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnInitializing(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnInitialized(sender As Object, e As ormDataObjectEventArgs)

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Overridable Property DomainID() As String Implements iormDataObject.DomainID
            Get
                If Me.ObjectHasDomainBehavior Then
                    Return Me._domainID
                Else
                    Return CurrentSession.CurrentDomainID
                End If
            End Get
            Set(value As String)
                _domainID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the GUID for the Object.
        ''' </summary>
        ''' <value>T</value>
        Public ReadOnly Property Guid() As Guid Implements iormDataObject.GUID
            Get
                Return Me._guid
            End Get
        End Property
        ''' <summary>
        ''' True if a memory data object
        ''' </summary>
        ''' <value>The run time only.</value>
        Public ReadOnly Property RunTimeOnly() As Boolean Implements iormDataObject.RuntimeOnly
            Get
                Return Me._RunTimeOnly
            End Get
        End Property

        ''' <summary>
        ''' returns the object definition associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectDefinition As ObjectDefinition Implements iormDataObject.ObjectDefinition
            Get
                If _objectdefinition Is Nothing Then
                    _objectdefinition = CurrentSession.Objects.GetObject(objectid:=Me.ObjectID)
                End If
                Return _objectdefinition
            End Get
        End Property
        ''' <summary>
        ''' returns the object class description associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassDescription As ObjectClassDescription Implements iormDataObject.ObjectClassDescription
            Get
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
                Return _classDescription
            End Get

        End Property

        ''' <summary>
        ''' return true if the data object is initialized
        ''' </summary>
        ''' <value>The PS is initialized.</value>
        Public Overridable ReadOnly Property IsInitialized() As Boolean Implements iormDataObject.IsInitialized
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
        Public ReadOnly Property ObjectID() As String Implements iormDataObject.ObjectID
            Get

                If Me.ObjectClassDescription IsNot Nothing Then
                    Return Me.ObjectClassDescription.ID
                Else
                    CoreMessageHandler("object id for orm data object class could not be found", argument:=Me.GetType.Name, _
                                        procedure:="ormDataObejct.ObjectID", messagetype:=otCoreMessageType.InternalError)
                End If
                Return Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the isDeleted.
        ''' </summary>
        ''' <value>The isDeleted.</value>
        Public Overridable ReadOnly Property IsDeleted() As Boolean Implements iormDataObject.IsDeleted
            Get
                Return Me._IsDeleted
            End Get

        End Property

        ''' <summary>
        ''' returns true if object has domain behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDomainBehavior As Boolean Implements iormDataObject.ObjectHasDomainBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDomainBehavior")
                '    Return False
                'End If

                '** to avoid recursion loops for bootstrapping objects during 
                '** startup of session check these out and look into description
                If CurrentSession.IsBootstrappingInstallationRequested _
                    OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ObjectID)
                    If anObjectDecsription IsNot Nothing Then
                        Return anObjectDecsription.ObjectAttribute.AddDomainBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDomainBehavior
                    Return False
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object is cached
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UseCache As Boolean Implements iormDataObject.useCache
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.UseCache")
                '    Return False
                'End If
                If _useCache.HasValue Then
                    Return _useCache
                Else
                    '** to avoid recursion loops for bootstrapping objects during 
                    '** startup of session check these out and look into description
                    If CurrentSession.IsBootstrappingInstallationRequested _
                        OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                        Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ObjectID)
                        If anObjectDecsription IsNot Nothing AndAlso anObjectDecsription.ObjectAttribute.HasValueUseCache Then
                            _useCache = anObjectDecsription.ObjectAttribute.UseCache
                        Else
                            _useCache = False
                        End If
                    Else
                        Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                        If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.UseCache
                        _useCache = False
                    End If

                    Return _useCache
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object has delete per flag behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDeletePerFlagBehavior As Boolean Implements iormDataObject.ObjectHasDeletePerFlagBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDeletePerFlagBehavior")
                '    Return False
                'End If
                '** avoid loops while starting up with bootstraps or during installation
                If CurrentSession.IsBootstrappingInstallationRequested OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                    If anObjectDescription IsNot Nothing Then
                        Return anObjectDescription.ObjectAttribute.AddDeleteFieldBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    '** per flag
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDeleteFieldBehavior
                End If

            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the changed property
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public Overridable Property IsChanged() As Boolean Implements iormDataObject.IsChanged
            Get
                Return Me._IsChanged
            End Get
            Protected Friend Set(value As Boolean)
                Me._IsChanged = value
                _changeTimeStamp = DateTime.Now
            End Set
        End Property
        ''' <summary>
        ''' Gets the changed property time stamp
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public ReadOnly Property ChangeTimeStamp() As DateTime Implements iormDataObject.ChangeTimeStamp
            Get
                Return _changeTimeStamp
            End Get
        End Property
        ''' <summary>
        ''' True if the data object is loaded
        ''' </summary>
        ''' <value>The PS is loaded.</value>
        Public Overridable ReadOnly Property IsLoaded() As Boolean Implements iormDataObject.IsLoaded
            Get
                Return _isloaded
            End Get
        End Property
        ''' <summary>
        ''' returns the Object Tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectTag() As String
            Get

                Return ConstDelimiter & Me.ObjectID.ToUpper & Converter.Array2otString(Me.ObjectPrimaryKeyValues)
            End Get
        End Property


        ''' <summary>
        '''  returns True if the Object was Instanced by Create
        ''' </summary>
        ''' <value>The PS is created.</value>
        Public ReadOnly Property IsCreated() As Boolean Implements iormDataObject.IsCreated
            Get
                Return _isCreated
            End Get
        End Property

        ''' <summary>
        ''' returns the record
        ''' </summary>
        ''' <value>The record.</value>
        Public Property Record() As ormRecord Implements iormDataObject.Record
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
        ''' <summary>
        ''' returns an array of the primarykey entry names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectPrimaryKeyEntrynames As String()
            Get
                Return Me.ObjectPrimaryKey.EntryNames
            End Get
        End Property
        ''' <summary>
        ''' returns the primaryKeyvalues
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property ObjectPrimaryKeyValues As Object() Implements iormDataObject.ObjectPrimaryKeyValues
        ''' <summary>
        ''' returns the primary key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property ObjectPrimaryKey As ormDatabaseKey Implements iormDataObject.ObjectPrimaryKey

        ''' <summary>
        ''' gets the Creation date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property CreatedOn() As Date? Implements iormDataObject.CreatedOn
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
        ReadOnly Property UpdatedOn() As Date? Implements iormDataObject.UpdatedOn
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
        Property DeletedOn() As Date? Implements iormDataObject.DeletedOn
            Get
                DeletedOn = _deletedOn
            End Get
            Friend Set(value As Date?)
                _deletedOn = value
            End Set
        End Property

        ''' <summary>
        ''' returns the Version number of the Attribute set Persistance Version
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassVersion(dataobject As iormDataObject, Optional name As String = Nothing) As Long Implements iormDataObject.GetObjectClassVersion
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
                            If anAttribute.GetType().Equals(GetType(ormTableAttribute)) AndAlso String.IsNullOrWhiteSpace(name) Then
                                '** Schema Definition
                                Return (DirectCast(anAttribute, ormTableAttribute).Version)

                                '** FIELD COLUMN
                            ElseIf anAttribute.GetType().Equals(GetType(iormObjectEntry)) AndAlso Not String.IsNullOrWhiteSpace(name) Then
                                If name.ToLower = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                    Return DirectCast(anAttribute, iormObjectEntry).Version
                                End If

                                '** INDEX
                            ElseIf anAttribute.GetType().Equals(GetType(ormIndexAttribute)) Then
                                If name.ToLower = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                    Return DirectCast(anAttribute, ormIndexAttribute).Version
                                End If

                            End If

                        Next
                    End If
                Next

                Return 0

            Catch ex As Exception

                Call CoreMessageHandler(procedure:="ormDataObject.GetVersion(of T)", exception:=ex)
                Return 0

            End Try
        End Function
#End Region


        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub New(Optional runtimeonly As Boolean = False, Optional objectID As String = Nothing)
            _IsInitialized = False
            _RunTimeOnly = runtimeonly
            If Not String.IsNullOrWhiteSpace(objectID) Then
                _classDescription = ot.GetObjectClassDescriptionByID(id:=objectID)
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
            End If
        End Sub
        ''' <summary>
        ''' clean up with the object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Finialize()
            _IsInitialized = False
            _record = Nothing
        End Sub
        ''' <summary>
        ''' Performs application-defined tasks associated with freeing, releasing,
        ''' or resetting unmanaged resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Finalize()
        End Sub
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
        Protected Function SwitchRuntimeON() As Boolean
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
        ''' copy the Primary key to the record
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <remarks></remarks>
        Protected Function CopyPrimaryKeyToRecord(ByRef primarykey As ormDatabaseKey, ByRef record As ormRecord,
                                                Optional domainid As String = Nothing, _
                                                Optional runtimeOnly As Boolean = False) As Boolean
            ''' get list of column names
            ''' 
            Dim aList As List(Of String)
            If Not runtimeOnly Then
                ' do not take the tableschema anymore
                ' aList = Me.TableSchema.PrimaryKeys 'take it from the real schema
                'Else
                Dim aDescriptor As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                If aDescriptor IsNot Nothing Then
                    aList = aDescriptor.PrimaryKeyEntryNames.ToList
                Else
                    CoreMessageHandler(message:="no object class description found", objectname:=Me.ObjectID, procedure:="ormDataObject.CopyPrimaryKeyToRecord", _
                                       messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If

            Dim i As UShort = 0
            If String.IsNullOrEmpty(domainid) Then domainid = ConstGlobalDomain


            ''' lookup the list of primary keys
            ''' 
            For Each acolumnname In aList
                If (record.IsBound AndAlso record.HasIndex(acolumnname)) OrElse Not record.IsBound Then
                    If acolumnname IsNot Nothing Then
                        If acolumnname.ToUpper <> Domain.ConstFNDomainID Then
                            record.SetValue(acolumnname, primarykey(i))
                        Else
                            If primarykey(i) Is Nothing OrElse primarykey(i) = String.Empty Then
                                record.SetValue(acolumnname, domainid)
                            Else
                                record.SetValue(acolumnname, primarykey(i))
                            End If
                        End If

                    End If
                Else
                    CoreMessageHandler(message:="record index not found", objectname:=Me.ObjectID, procedure:="ormDataObject.CopyPrimaryKeyToRecord", _
                                       entryname:=acolumnname, messagetype:=otCoreMessageType.InternalError)
                End If
                i = i + 1
            Next

            Return True
        End Function

        ''' <summary>
        ''' extract out of a record a Primary Key array
        ''' </summary>
        ''' <param name="record"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Shared Function ExtractObjectPrimaryKey(record As ormRecord, objectID As String,
                                                    Optional runtimeOnly As Boolean = False) As ormDatabaseKey
            Dim thePrimaryKeyEntryNames As String()
            Dim pkarray As Object()
            Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(objectID)
            '*** extract the primary keys from record
            If Not CurrentSession.IsRepositoryAvailable Then
                If anObjectDescription IsNot Nothing Then
                    thePrimaryKeyEntryNames = anObjectDescription.PrimaryKeyEntryNames
                Else
                    CoreMessageHandler(message:="ObjectDescriptor not found", objectname:=objectID, argument:=objectID, _
                                        procedure:="ormDataobject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
                '* extract
                thePrimaryKeyEntryNames = anObjectDescription.PrimaryKeyEntryNames
                ReDim pkarray(thePrimaryKeyEntryNames.Length - 1)
                Dim i As UShort = 0
                For Each anEntry In anObjectDescription.PrimaryEntryAttributes
                    If record.HasIndex(anEntry.ContainerEntryName) Then
                        pkarray(i) = record.GetValue(index:=anEntry.ContainerEntryName)
                        i += 1
                    End If
                Next
            Else
                Dim anObjectDefinition = CurrentSession.Objects.GetObject(objectID)
                '* keynames of the object
                thePrimaryKeyEntryNames = anObjectDefinition.PrimaryKeyEntryNames
                If thePrimaryKeyEntryNames.Count = 0 Then
                    CoreMessageHandler(message:="objectdefinition has not primary keys", objectname:=anObjectDefinition.ObjectID, _
                                   procedure:="ormDataObject.ExtractPrimaryKey", messagetype:=otCoreMessageType.InternalWarning)
                    Return Nothing
                End If
                '* extract
                ReDim pkarray(thePrimaryKeyEntryNames.Length - 1)
                Dim i As UShort = 0
                For Each anEntry In anObjectDefinition.GetKeyEntries
                    If record.HasIndex(DirectCast(anEntry, ObjectContainerEntry).ContainerEntryName) Then
                        pkarray(i) = record.GetValue(index:=DirectCast(anEntry, ObjectContainerEntry).ContainerEntryName)
                        i += 1
                    End If
                Next

            End If

            Return New ormDatabaseKey(objectid:=objectID, keyvalues:=pkarray)
        End Function
        ''' <summary>
        ''' Merge Values of an record in own record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns>True if successfull </returns>
        ''' <remarks></remarks>
        Protected Function MergeRecord(record As ormRecord) As Boolean
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
        Public Overridable Function DetermineLiveStatus() As Boolean Implements iormDataObject.DetermineLiveStatus
            ''' check the record again -> if infused by a record by sql selectment if have nor created not loaded
            If Me.IsInitialized Then
                '** check on the records
                _isCreated = Me.Record.IsCreated
                Return Me.Record.IsLoaded
            End If
            Return False
        End Function
        ''' <summary>
        ''' checks if the data object is alive
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsAlive(Optional subname As String = Nothing, Optional throwError As Boolean = True) As Boolean Implements iormDataObject.IsAlive
            If Not Me.IsLoaded And Not Me.IsCreated Then
                DetermineLiveStatus()
                '** check again
                If Not Me.IsLoaded And Not Me.IsCreated Then
                    If throwError Then
                        If String.IsNullOrWhiteSpace(subname) Then subname = "ormDataObject.checkalive"
                        If Not subname.Contains("."c) Then subname = Me.GetType.Name & "." & subname

                        CoreMessageHandler(message:="object is not alive but operation requested", objectname:=Me.GetType.Name, _
                                           procedure:=subname, messagetype:=otCoreMessageType.InternalError)
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
        Public Overridable Function Initialize(Optional runtimeOnly As Boolean = False) As Boolean Implements iormDataObject.Initialize


            '** is a session running ?!
            If Not runtimeOnly AndAlso Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Call CoreMessageHandler(message:="data object cannot be initialized - start session to database first", _
                                           objectname:=Me.ObjectID, procedure:="ormDataobject.initialize", _
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



            ''' run on checks
            If Not _record.IsBound AndAlso Not Me.RunTimeOnly Then
                Call CoreMessageHandler(procedure:="ormDataObject.Initialize", message:="record is not set to table definition", _
                                        messagetype:=otCoreMessageType.InternalError, containerID:=Me.Record.TableIDs.FirstOrDefault, noOtdbAvailable:=True)
                Initialize = False
            End If

            '*** check on connected status if not on runtime
            If Not Me.RunTimeOnly Then
                If _record.TableStores IsNot Nothing Then
                    For Each aTablestore In _record.TableStores
                        If Not aTablestore Is Nothing AndAlso Not aTablestore.Connection Is Nothing Then
                            If Not aTablestore.Connection.IsConnected AndAlso Not aTablestore.Connection.Session.IsBootstrappingInstallationRequested Then
                                Call CoreMessageHandler(procedure:="ormDataObject.Initialize", message:="TableStore is not connected to database / no connection available", _
                                                        messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                                Initialize = False
                            End If
                        End If
                    Next
                Else
                    Call CoreMessageHandler(procedure:="ormDataObject.Initialize", message:="TableStore is nothing in record", _
                                                       messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                    Initialize = False
                End If

            End If

            '* default values
            _IsDeleted = False

            '** fire event
            ourEventArgs = New ormDataObjectEventArgs(Me, record:=Me.Record, usecache:=Me.UseCache, runtimeOnly:=runtimeOnly)
            ourEventArgs.Proceed = Initialize
            RaiseEvent OnInitialized(Me, ourEventArgs)
            '** set initialized
            _IsInitialized = ourEventArgs.Proceed
            Return Initialize
        End Function
    End Class
End Namespace

