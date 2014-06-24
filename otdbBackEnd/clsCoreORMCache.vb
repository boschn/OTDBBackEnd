REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CACHE Class for ORM iormPersistables based on events
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-03-14
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2014
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics.Debug

Imports OnTrack.Database

Namespace OnTrack.Database

    ''' <summary>
    ''' Interface for Cache Manager
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iormObjectCacheManager

        Function Halt(Optional force As Boolean = False) As Boolean

        Function Shutdown(Optional force As Boolean = False) As Boolean

        Function Start(Optional force As Boolean = False) As Boolean

        ''' <summary>
        ''' Handler for the OnObjectDefinitionLoaded event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnObjectDefinitionLoaded(sender As Object, e As ObjectRepository.EventArgs)

        ''' <summary>
        ''' Handler for the ObjectClassDescriptionLoaded Event of the ORM Object Repository
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnObjectClassDescriptionLoaded(sender As Object, e As ObjectClassRepository.EventArgs)

        ''' <summary>
        ''' OnCreating Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnCreatingDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnCreated Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnCreatedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnCloning Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnCloningDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnCloned Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnClonedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnDeletedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnUnDeletedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnPersisted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnPersistedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnRetrieving Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnRetrievingDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnRetrieved Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnRetrievedDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnCheckingUniquenessDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        ''' <summary>
        ''' after infusion of dataobject
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnInfusedDataObject(sender As Object, e As ormDataObjectEventArgs)

        ''' <summary>
        ''' starting infusion of dataobject
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnInfusingDataObject(sender As Object, e As ormDataObjectEventArgs)

        ''' <summary>
        ''' after Overloaded a domain specific dataobject with a global domain event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnOverloadedDataObject(sender As Object, e As ormDataObjectOverloadedEventArgs)

    End Interface

    ''' <summary>
    ''' Object Cache Manager Implementation
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormObjectCacheManager
        Implements iormObjectCacheManager
        ''' <summary>
        ''' persistence status of the cached object
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum PersistenceStatus
            CreatedNotPersisted = 1
            Retrieved = 2
            Persisted = 4
            Deleted = 8
            Created = 16
        End Enum

        ''' <summary>
        ''' generic cached object instance (tuppel with some additional data)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <remarks></remarks>

        Private Class CachedObject(Of T)
            Private _object As T
            '** bookkeeping
            Private _GUID As Guid = Guid.NewGuid
            Private _comeToAlive As DateTime = DateTime.Now
            Private _creationDate As DateTime
            Private _lastAccessStamp As DateTime
            Private _persistedDate As DateTime
            Private _retrieveData As DateTime
            Private _lockobject As New Object
            Private _persistenceStatus As PersistenceStatus = 0

            ''' <summary>
            ''' Constructor
            ''' </summary>
            ''' <param name="object"></param>
            ''' <remarks></remarks>
            Public Sub New(ByRef [object] As T)
                _object = [object]
            End Sub

#Region "Properties"


            ''' <summary>
            ''' Gets the come to alive.
            ''' </summary>
            ''' <value>The come to alive.</value>
            Public ReadOnly Property ComeToAlive() As DateTime
                Get
                    Return Me._comeToAlive
                End Get
            End Property

            ''' <summary>
            ''' Gets the GUID.
            ''' </summary>
            ''' <value>The GUID.</value>
            Public ReadOnly Property Guid() As Guid
                Get
                    Return Me._GUID
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the deleted flag
            ''' </summary>
            ''' <value>The is deleted.</value>
            Public Property IsDeleted() As Boolean
                Get
                    Return _persistenceStatus And PersistenceStatus.Deleted
                End Get
                Set(value As Boolean)
                    If value Then
                        '** switch on
                        _persistenceStatus = _persistenceStatus Or PersistenceStatus.Deleted
                    ElseIf _persistenceStatus And PersistenceStatus.Deleted Then
                        '** switch off if on  else off anyways
                        _persistenceStatus = _persistenceStatus Xor PersistenceStatus.Deleted
                    End If
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the deleted flag
            ''' </summary>
            ''' <value>The is deleted.</value>
            Public Property IsCreated() As Boolean
                Get
                    Return _persistenceStatus And PersistenceStatus.Created
                End Get
                Set(value As Boolean)
                    If value Then
                        '** switch on
                        _persistenceStatus = _persistenceStatus Or PersistenceStatus.Created
                    ElseIf _persistenceStatus And PersistenceStatus.Created Then
                        '** switch off if on  else off anyways
                        _persistenceStatus = _persistenceStatus Xor PersistenceStatus.Created
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the is persisted.
            ''' </summary>
            ''' <value>The is persisted.</value>
            Public Property IsPersisted() As Boolean
                Get
                    Return _persistenceStatus And PersistenceStatus.Persisted
                End Get
                Set(value As Boolean)
                    If value Then
                        '** switch on
                        _persistenceStatus = _persistenceStatus Or PersistenceStatus.Persisted
                    ElseIf _persistenceStatus And PersistenceStatus.Persisted Then
                        '** switch off if on  else off anyways
                        _persistenceStatus = _persistenceStatus Xor PersistenceStatus.Persisted
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the is retrieved.
            ''' </summary>
            ''' <value>The is retrieved.</value>
            Public Property IsRetrieved() As Boolean
                Get
                    Return _persistenceStatus And PersistenceStatus.Retrieved
                End Get
                Set(value As Boolean)
                    If value Then
                        '** switch on
                        _persistenceStatus = _persistenceStatus Or PersistenceStatus.Retrieved
                    ElseIf _persistenceStatus And PersistenceStatus.Retrieved Then
                        '** switch off if on  else off anyways
                        _persistenceStatus = _persistenceStatus Xor PersistenceStatus.Retrieved
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the retrieve data.
            ''' </summary>
            ''' <value>The retrieve data.</value>
            Public Property RetrieveData() As DateTime
                Get
                    Return Me._retrieveData
                End Get
                Set(value As DateTime)
                    Me._retrieveData = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the persisted date.
            ''' </summary>
            ''' <value>The persisted date.</value>
            Public Property PersistedDate() As DateTime
                Get
                    Return Me._persistedDate
                End Get
                Set(value As DateTime)
                    Me._persistedDate = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the last access stamp.
            ''' </summary>
            ''' <value>The last access stamp.</value>
            Public Property LastAccessStamp() As DateTime
                Get
                    Return Me._lastAccessStamp
                End Get
                Set(value As DateTime)
                    Me._lastAccessStamp = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the creation date.
            ''' </summary>
            ''' <value>The creation date.</value>
            Public Property CreationDate() As DateTime
                Get
                    Return Me._creationDate
                End Get
                Set(value As DateTime)
                    Me._creationDate = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the object.
            ''' </summary>
            ''' <value>The object.</value>
            Public Property [Object]() As T
                Get
                    Return Me._object
                End Get
                Set(value As T)
                    Me._object = value
                End Set
            End Property
#End Region


        End Class

        ''' <summary>
        ''' registered object classes
        ''' </summary>
        ''' <remarks></remarks>
        Private _registeredObjects As New Dictionary(Of String, ormDataTupleMetaClass(Of iormPersistable))

        ''' <summary>
        ''' the Object Cache of overloaded objects per object id  
        ''' and the primary key of the domain specific object but the object of the overload
        ''' </summary>
        ''' <remarks></remarks>
        Private _cachedOverloadedObjects As New SortedList(Of String, Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable)))

        ''' <summary>
        ''' the Object Cache per objectid and then the primary key of the objects of loaded objects
        ''' </summary>
        ''' <remarks></remarks>
        Private _cachedLoadedObjects As New SortedList(Of String, Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable)))

        ''' <summary>
        ''' dynamic
        ''' </summary>
        ''' <remarks></remarks>
        Private _isInitialized As Boolean = False
        Private _isStarted As Boolean = False
        Private _domainid As String

        Private WithEvents _session As Session
        Private _lockObject As New Object
        Private _ormDataObjectIsHooked As Boolean = False ' VERY BAD : avoid shared events in this base class to be event handled multiple times

        ''' Define the Assignments of shared iorm persistable Events to the Cache Methods
        ''' IMPORTANT !
        Private _assignments As String(,) = {{"ClassOnInfusing", "OnInfusingDataObject"}, _
                                             {"ClassOnInfused", "OnInfusedDataObject"}, _
                                             {"ClassOnRetrieved", "OnRetrievedDataObject"}, _
                                             {"ClassOnOverloaded", "OnOverloadedDataObject"}, _
                                             {"ClassOnRetrieving", "OnRetrievingDataObject"}, _
                                             {"ClassOnCreated", "OnCreatedDataObject"}, _
                                             {"ClassOnCreating", "OnCreatingDataObject"}, _
                                             {"ClassOnCheckingUniqueness", "OnCheckinqUniquenessDataObject"}, _
                                             {"ClassOnDeleted", "OnDeletedDataObject"}, _
                                             {"ClassOnUnDeleted", "OnUnDeletedDataObject"}, _
                                             {"ClassOnCloning", "OnCloningDataObject"}, _
                                             {"ClassOnCloned", "OnClonedDataObject"}, _
                                             {"ClassOnPersisted", "OnPersistedDataObject"} _
                                       }
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="session"></param>
        ''' <remarks></remarks>
        Sub New(session As Session, domainid As String)
            _session = session
            _domainid = domainid
        End Sub

        ''' <summary>
        ''' DomainHandler for DomainChanging
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub _session_OnDomainChanged(sender As Object, e As SessionEventArgs) Handles _session.OnDomainChanged
            If e.Session.CurrentDomainID <> _domainid Then
                Me.Halt()
            Else
                Me.Start()
            End If
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnSessionStart(sender As Object, e As SessionEventArgs) Handles _session.OnStarted
            If e.Session.CurrentDomainID = _domainid Then Me.Start()
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>
        Private Sub OnSessionEnd(sender As Object, e As SessionEventArgs) Handles _session.OnEnding
            If Me._isInitialized Then
                Me.Shutdown(force:=True)
            End If
        End Sub

        ''' <summary>
        ''' starts the cache
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Start(Optional force As Boolean = False) As Boolean Implements iormObjectCacheManager.Start
            If Me.Initialize(force:=force) Then
                _isStarted = True
            End If
        End Function

        ''' <summary>
        ''' halts the cache
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Halt(Optional force As Boolean = False) As Boolean Implements iormObjectCacheManager.Halt
            If Me.Initialize(force:=force) Then
                _isStarted = False
            End If
        End Function

        ''' <summary>
        ''' shutdowns the cache
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Shutdown(Optional force As Boolean = False) As Boolean Implements iormObjectCacheManager.Shutdown
            ''' flush all objects
            ''' 
            _cachedLoadedObjects.Clear()
            _registeredObjects.Clear()
            _isStarted = False
        End Function

        ''' <summary>
        ''' flush the cache
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function FlushCache() As Boolean
            _cachedLoadedObjects.Clear()
        End Function

        ''' <summary>
        ''' Initialize the Cache
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Initialize(Optional force As Boolean = False) As Boolean

            If _isInitialized And Not force Then
                Return True
            End If

            ''' check all descriptions to see which we need to cache
            ''' might be that object repository is deactivating the cache on some objects
            ''' if activating where it was not before we might loose some objects
            ''' 
            For Each aDescription In ot.ObjectClassRepository.ObjectClassDescriptions
                If aDescription.ObjectAttribute.HasValueUseCache AndAlso aDescription.ObjectAttribute.UseCache Then
                    Me.RegisterObjectClass(aDescription.ObjectAttribute.ClassName)
                End If
            Next

            _isInitialized = True
            Return _isInitialized
        End Function

        ''' <summary>
        ''' Register an typename
        ''' </summary>
        ''' <param name="classname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterObjectClass(typename As String) As Boolean
            '*** if cache is used
            '*** register the object class type
            '***
            Dim anEntry As ormDataTupleMetaClass(Of iormPersistable)
            Try

                If Not _registeredObjects.ContainsKey(key:=typename) Then
                    Dim aType = System.Type.GetType(typeName:=typename, throwOnError:=False, ignoreCase:=True)
                    If aType IsNot Nothing And aType.GetInterface(GetType(iormPersistable).Name, ignoreCase:=True) IsNot Nothing Then
                        anEntry = New ormDataTupleMetaClass(Of iormPersistable)(aType)
                        _registeredObjects.Add(key:=typename, value:=anEntry)
                        '** WORKAROUND with ORM DataObjectisHooked :-(
                        If Not _ormDataObjectIsHooked Then Me.RegisterEvents(aType)
                    End If
                End If


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormObjectCacheManager.RegisterObjectClass", arg1:=typename, messagetype:=otCoreMessageType.InternalError)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Register an typename
        ''' </summary>
        ''' <param name="classname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DeRegisterObjectClass(typename As String) As Boolean
            '*** if cache is used
            '*** deregister the object class type
            '***
            Try
                If _registeredObjects.ContainsKey(key:=typename) Then
                    Dim aType = Type.GetType(typename)
                    If aType IsNot Nothing Then
                        _registeredObjects.Remove(key:=typename)
                        Me.DeRegisterEvents(aType)
                        Return True
                    End If
                End If
                Return True
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormObjectCacheManager.DeRegisterObjectClass", arg1:=typename, messagetype:=otCoreMessageType.InternalError)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' register the caching routines at the iormpersistable class
        ''' </summary>
        ''' <param name="aClass"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function RegisterEvents([type] As System.Type) As Boolean
            Dim eventinfo As Reflection.EventInfo
            Dim amethod As Reflection.MethodInfo
            Dim adelegate As [Delegate]
            Dim result As Boolean = True

            Try
                ''' no shared events on interfaces possible :-(
                ''' therefore we need to hardcode the registration of shared events
                ''' 
                If [type].GetInterfaces.Contains(GetType(iormPersistable)) Then

                    If GetType(ormDataObject).IsAssignableFrom([type]) AndAlso Not _ormDataObjectIsHooked Then
                        For i = 0 To _assignments.GetUpperBound(0)
                            Dim anEventname As String = _assignments(i, 0)
                            Dim aDelegateName As String = _assignments(i, 1)
                            eventinfo = [type].GetEvent(anEventname, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public)
                            If eventinfo IsNot Nothing Then
                                amethod = Me.GetType().GetMethod(aDelegateName, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public)
                                If amethod IsNot Nothing Then
                                    '''  doesnot work :-(  anEventname & EventHandler connot be received from base classes :-(
                                    ''' therefore we cannot check if we have already hooked up the base static event
                                    ''' means that events will be registered once per class BUT multiple in the base class
                                    ''' since we cannot change the declaring type and not check it otherwise
                                    ''' therefore manually check for base class ormDataObject and set flag and skip it 
                                    ' Dim aFields = [type].GetFields(bindingAttr:=Reflection.BindingFlags.FlattenHierarchy Or 
                                    'Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public)
                                    'If aFieldInfo IsNot Nothing Then
                                    '    Dim value = aFieldInfo.GetValue(Nothing)
                                    'End If
                                    adelegate = [Delegate].CreateDelegate(eventinfo.EventHandlerType, Me, amethod)
                                    eventinfo.AddEventHandler([type], adelegate)
                                    'System.Diagnostics.Debug.WriteLine("created " & [type].Name & " -> " & aDelegateName)

                                    result = result And True
                                Else
                                    CoreMessageHandler(message:="Method does not exist in iormPersistable implementation '" & [type].Name & "'", arg1:=aDelegateName, subname:="ormObjectCacheManager.RegisterEvents", messagetype:=otCoreMessageType.InternalError)
                                    result = False
                                End If
                            Else
                                CoreMessageHandler(message:="Event does not exist in iormPersistable implementation '" & [type].Name & "'", arg1:=anEventname, subname:="ormObjectCacheManager.RegisterEvents", messagetype:=otCoreMessageType.InternalError)
                                result = False
                            End If
                        Next

                        '*** donot doe the base class twice
                        _ormDataObjectIsHooked = True
                        ''' 
                    ElseIf [type].IsAssignableFrom(GetType(ormDataObject)) Then
                        Return _ormDataObjectIsHooked

                    End If

                    Return result
                Else
                    CoreMessageHandler(message:="type is not a iormPersistable implementation: '" & [type].Name & "'", subname:="ormObjectCacheManager.RegisterEvents", messagetype:=otCoreMessageType.InternalError)
                    result = False
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormObjectCacheManager.RegisterEvents")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' register the caching routines at the iormpersistable class
        ''' </summary>
        ''' <param name="aClass"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function DeRegisterEvents([type] As System.Type) As Boolean
            Dim eventinfo As Reflection.EventInfo
            Dim amethod As Reflection.MethodInfo
            Dim adelegate As [Delegate]
            Dim result As Boolean = True

            Try
                ''' no shared events on interfaces possible :-(
                ''' therefore we need to hardcode the registration of shared events
                ''' 
                If [type].GetInterfaces.Contains(GetType(iormPersistable)) Then

                    For i = 0 To _assignments.GetUpperBound(0)
                        Dim anEventname As String = _assignments(i, 0)
                        Dim aDelegateName As String = _assignments(i, 1)

                        eventinfo = [type].GetEvent(anEventname, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public)
                        If eventinfo IsNot Nothing Then
                            amethod = Me.GetType().GetMethod(aDelegateName, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public)
                            If amethod IsNot Nothing Then
                                adelegate = [Delegate].CreateDelegate(eventinfo.EventHandlerType, Me, amethod)
                                eventinfo.RemoveEventHandler([type], adelegate)
                                result = result And True
                            Else
                                CoreMessageHandler(message:="Method does not exist in iormPersistable implementation '" & [type].Name & "'", arg1:=aDelegateName, subname:="ormObjectCacheManager.DeRegisterEvents", messagetype:=otCoreMessageType.InternalError)
                                result = False
                            End If
                        Else
                            CoreMessageHandler(message:="Event does not exist in iormPersistable implementation '" & [type].Name & "'", arg1:=aDelegateName, subname:="ormObjectCacheManager.DeRegisterEvents", messagetype:=otCoreMessageType.InternalError)
                            result = False
                        End If
                    Next

                    Return result
                Else
                    CoreMessageHandler(message:="type is not a iormPersistable implementation: '" & [type].Name & "'", subname:="ormObjectCacheManager.RegisterEvents", messagetype:=otCoreMessageType.InternalError)
                    result = False
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ormObjectCacheManager.RegisterEvents")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Handler for the ObjectDefinitionLoaded Event of the ORM Object Repository
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnObjectDefinitionLoaded(sender As Object, e As ObjectRepository.EventArgs) Implements iormObjectCacheManager.OnObjectDefinitionLoaded
            '*** if cache is used
            '*** register the object class type
            '***
            Me.Initialize()
            Dim anEntry As ormDataTupleMetaClass(Of iormPersistable)
            If e.Objectdefinition.UseCache Then
                Me.RegisterObjectClass(typename:=e.Objectdefinition.Classname)
            Else
                Me.DeRegisterObjectClass(typename:=e.Objectdefinition.Classname)
            End If

        End Sub
        ''' <summary>
        ''' Handler for the OnObjectClassDescriptionLoaded Event of the ORM Object Class Repository
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnObjectClassDescriptionLoaded(sender As Object, e As ObjectClassRepository.EventArgs) Implements iormObjectCacheManager.OnObjectClassDescriptionLoaded
            '*** if cache is used
            '*** deregister the object class type
            '***
            Me.Initialize()
            If e.Description.ObjectAttribute.HasValueUseCache AndAlso e.Description.ObjectAttribute.UseCache Then
                Me.RegisterObjectClass(typename:=e.Objectname)
            Else
                Me.DeRegisterObjectClass(typename:=e.Objectname)
            End If
        End Sub

        ''' <summary>
        ''' OnCreating Event Handler for the ORM Data Object - check if the object exists in cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreatingDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnCreatingDataObject
            If _isStarted AndAlso e.UseCache Then
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                ''' do not check if the primary key contains nothing (in cases in which keys will be generated
                ''' 
                If theobjects IsNot Nothing AndAlso Not e.Pkarray.Contains(Nothing) Then
                    Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        If e.DataObject IsNot Nothing Then
                            aBucket.LastAccessStamp = DateTime.Now
                            e.Result = True ' yes we have a result
                            e.AbortOperation = True ' abort creating use object instead
                            Exit Sub
                        End If
                    ElseIf e.DataObject.ObjectHasDomainBehavior Then
                        ''' check the overload cache -> do nothing since this might be the start of an end overloading
                        ''' 
                        'SyncLock _lockObject
                        '    If _cachedOverloadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        '        theobjects = _cachedOverloadedObjects.Item(e.DataObject.GetType.Name)
                        '    Else
                        '        e.Result = False
                        '        e.AbortOperation = False
                        '        Exit Sub
                        '    End If
                        'End SyncLock

                        'searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                        '''' found in overload
                        'If theobjects.ContainsKey(key:=searchkeys) Then
                        '    Dim aBucket = theobjects.Item(key:=searchkeys)
                        '    e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        '    If e.DataObject IsNot Nothing Then
                        '        aBucket.LastAccessStamp = DateTime.Now
                        '        e.Result = True 'success
                        '        e.AbortOperation = True
                        '        Exit Sub
                        '    End If
                        'End If
                    End If
                End If
            End If

            ''' no result
            e.AbortOperation = False
            e.Result = True
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnCreated Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreatedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnCreatedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.IsCreated = True
                    aBucket.CreationDate = DateTime.Now
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    ''' check if new object ends an overload
                    ''' 
                    EndOverloading(searchkeys, e.DataObject)
                    e.AbortOperation = False
                    e.Result = False 'success
                    Exit Sub
                Else
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject.Guid <> e.DataObject.Guid Then
                        CoreMessageHandler("Warning ! objects of same type and keys already in cache", subname:="ormObjectCacheManager.OnCreatedDataObject", messagetype:=otCoreMessageType.InternalWarning, _
                                        objectname:=e.DataObject.GetType.Name, arg1:=e.Pkarray)
                        e.Result = False
                        e.AbortOperation = True
                        Exit Sub
                    Else
                        e.Result = True
                        e.AbortOperation = False
                        Exit Sub
                    End If

                End If


            End If


            e.AbortOperation = False
            e.Result = False
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnCloning Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCloningDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnCloningDataObject
            If _isStarted AndAlso e.UseCache Then
                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock

                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        If e.DataObject IsNot Nothing Then
                            aBucket.LastAccessStamp = DateTime.Now
                            e.Result = True 'success
                            e.AbortOperation = True ' abort cloning use object insted
                            Exit Sub
                        End If
                    ElseIf e.DataObject.ObjectHasDomainBehavior Then
                        ''' do nothing -> this might the start of end overloading
                        ''' 
                    End If
                End If


                e.AbortOperation = False
                e.Result = False
                Exit Sub

            End If
            '*** do really nothing we not on start
        End Sub

        ''' <summary>
        ''' OnCloned Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnClonedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnClonedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.IsCreated = True
                    aBucket.CreationDate = DateTime.Now
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    EndOverloading(searchkeys, e.DataObject)
                    e.AbortOperation = False
                    e.Result = True 'success
                    Exit Sub
                Else
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject.Guid <> e.DataObject.Guid Then
                        CoreMessageHandler("Warning ! objects of same type and keys already in cache", subname:="ormObjectCacheManager.OnClonedDataObject", messagetype:=otCoreMessageType.InternalWarning, _
                                        objectname:=e.DataObject.GetType.Name, arg1:=e.Pkarray)
                        e.DataObject = Nothing
                        e.Result = False
                        e.AbortOperation = True
                        Exit Sub
                    Else
                        e.Result = True
                        e.AbortOperation = False
                        Exit Sub
                    End If

                End If


                e.AbortOperation = False
                e.Result = False
                Exit Sub

            End If
            '*** do really nothing we not on start
        End Sub

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object - mark object as deleted
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDeletedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnDeletedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                Dim aBucket As CachedObject(Of iormPersistable)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                Else
                    aBucket = theobjects.Item(key:=searchkeys)
                End If
                aBucket.LastAccessStamp = DateTime.Now
                aBucket.IsDeleted = True

                e.AbortOperation = False
                e.Result = False
                Exit Sub

            End If
            '*** do really nothing we not on start
        End Sub

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnUnDeletedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnUnDeletedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                Dim aBucket As CachedObject(Of iormPersistable)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                Else
                    aBucket = theobjects.Item(key:=searchkeys)
                End If
                aBucket.LastAccessStamp = DateTime.Now
                aBucket.IsDeleted = False
            End If

            '*** do really nothing we not on start
        End Sub

        ''' <summary>
        ''' OnPersisted Event Handler for the ORM Data Object - check if object needs to be added and set persistance timestamp
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnPersistedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnPersistedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                Dim aBucket As CachedObject(Of iormPersistable)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                Else
                    aBucket = theobjects.Item(key:=searchkeys)
                End If
                EndOverloading(searchkeys, e.DataObject)
                aBucket.PersistedDate = DateTime.Now
                aBucket.IsPersisted = True

                e.AbortOperation = False
                e.Result = False
                Exit Sub

            End If
            '*** do really nothing we not on start
        End Sub

        ''' <summary>
        ''' checks and deletes an overloading object
        ''' </summary>
        ''' <param name="searchkeys"></param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function EndOverloading(searchkeys As ormPrimaryKey(Of iormPersistable), dataobject As ormDataObject) As Boolean

            If dataobject IsNot Nothing AndAlso _
                dataobject.ObjectHasDomainBehavior AndAlso dataobject.DomainID <> ConstGlobalDomain Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))

                '** get the data
                SyncLock _lockObject
                    If _cachedOverloadedObjects.ContainsKey(dataobject.GetType.Name) Then
                        theobjects = _cachedOverloadedObjects.Item(dataobject.GetType.Name)
                    Else
                        Return False
                    End If
                End SyncLock

                Dim aBucket As CachedObject(Of iormPersistable)
                If theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = theobjects.Item(key:=searchkeys)
                    Return theobjects.TryRemove(key:=searchkeys, value:=aBucket)
                End If
            End If

            Return False
        End Function

        ''' <summary>
        ''' OnRetrieving Event Handler for the ORM Data Object - add to cache the overloading of domain specific
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnOverloadedDataObject(sender As Object, e As ormDataObjectOverloadedEventArgs) Implements iormObjectCacheManager.OnOverloadedDataObject
            ''' store only if object is not in globaldomain
            ''' 
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedOverloadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedOverloadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedOverloadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.DomainPKArray)
                Dim aBucket As CachedObject(Of iormPersistable)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                Else
                    aBucket = theobjects.Item(key:=searchkeys)
                End If
                aBucket.PersistedDate = DateTime.Now
                aBucket.IsPersisted = True

                e.AbortOperation = False
                e.Result = False
                Exit Sub
            End If

            '*** do really nothing
        End Sub
        ''' <summary>
        ''' OnRetrieving Event Handler for the ORM Data Object - check if object exists in cache and use it from there
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRetrievingDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnRetrievingDataObject
            If _isStarted AndAlso e.UseCache Then
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        If e.DataObject IsNot Nothing Then
                            aBucket.LastAccessStamp = DateTime.Now
                            e.Result = True 'success
                            e.AbortOperation = True
                            Exit Sub
                        End If
                    ElseIf e.DataObject.ObjectHasDomainBehavior Then
                        ''' check the overload cache
                        ''' 
                        SyncLock _lockObject
                            If _cachedOverloadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                                theobjects = _cachedOverloadedObjects.Item(e.DataObject.GetType.Name)
                            Else
                                e.Result = False
                                e.AbortOperation = False
                                Exit Sub
                            End If
                        End SyncLock

                        searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                        ''' found in overload
                        If theobjects.ContainsKey(key:=searchkeys) Then
                            Dim aBucket = theobjects.Item(key:=searchkeys)
                            e.DataObject = TryCast(aBucket.Object, ormDataObject)
                            If e.DataObject IsNot Nothing Then
                                aBucket.LastAccessStamp = DateTime.Now
                                e.Result = True 'success
                                e.AbortOperation = True
                                Exit Sub
                            End If
                        End If

                    End If
                End If
           
                e.AbortOperation = False
                e.Result = False
                Exit Sub
            End If

            '*** do really nothing
        End Sub

        ''' <summary>
        ''' OnRetrieved Event Handler for the ORM Data Object - add retrieved object to cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRetrievedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnRetrievedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub


                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.RetrieveData = DateTime.Now
                    aBucket.IsRetrieved = True
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    EndOverloading(searchkeys, e.DataObject)
                    e.AbortOperation = False
                    e.Result = True 'success
                    Exit Sub
                Else
                    ''' it might be that a retrieved object was stored 
                    ''' previously under infused 
                    ''' to check on this we would need a GUID for each Bucket
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject As iormPersistable = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject IsNot Nothing AndAlso aDataObject.GUID <> e.DataObject.Guid Then
                        CoreMessageHandler(message:="Dataobject was retrieved which was already in cache but under another guid", subname:="ormObjectCacheManager.OnRetrievedDataObject", objectname:=e.DataObject.ObjectID, _
                                           messagetype:=otCoreMessageType.InternalWarning, arg1:=Converter.Array2otString(e.DataObject.PrimaryKeyValues))
                        e.Result = False ' do nothing in the case
                        e.AbortOperation = False
                    Else
                        aBucket.RetrieveData = DateTime.Now
                        aBucket.IsRetrieved = True
                        e.Result = False
                        e.AbortOperation = False
                    End If

                    Exit Sub
                End If


                e.AbortOperation = False
                e.Result = False
                Exit Sub
            End If

            '*** do really nothing
        End Sub
        ''' <summary>
        ''' OnCreating Event Handler for the ORM Data Object - check if the object exists in cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfusingDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnInfusingDataObject
            If _isStarted AndAlso e.UseCache Then
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        aBucket.LastAccessStamp = DateTime.Now
                        Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                        If aDataObject IsNot Nothing AndAlso aDataObject.Guid <> e.DataObject.Guid Then
                            '** return the existing object
                            e.DataObject = aDataObject
                            e.Result = True
                            e.AbortOperation = True
                            Exit Sub
                        End If
                    End If
                End If

                e.AbortOperation = False
                e.Result = False
                Exit Sub
            End If

            '*** do really nothing
        End Sub

        ''' <summary>
        ''' OnRetrieved Event Handler for the ORM Data Object - add retrieved object to cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfusedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnInfusedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub


                '** get the data
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedLoadedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    If e.DataObject.IsLoaded Then
                        aBucket.RetrieveData = DateTime.Now
                        aBucket.IsRetrieved = e.DataObject.IsLoaded
                    End If
                    If e.DataObject.IsCreated Then
                        aBucket.IsCreated = e.DataObject.IsCreated
                        aBucket.CreationDate = DateTime.Now
                    End If

                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    EndOverloading(searchkeys, e.DataObject)
                    e.AbortOperation = False
                    e.Result = False 'success
                    Exit Sub
                Else
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject IsNot Nothing AndAlso aDataObject.Guid <> e.DataObject.Guid Then
                        'CoreMessageHandler(message:="Warning ! infused object already in cache", subname:="ormObjectCacheManager.OnInfusedDataObject", _
                        '                   messagetype:=otCoreMessageType.InternalWarning, _
                        '                  objectname:=aDataObject.ObjectID, arg1:=e.Pkarray)
                        e.DataObject = aDataObject
                        e.Result = True
                        e.AbortOperation = True
                        Exit Sub
                    End If
                End If

                e.AbortOperation = False
                e.Result = False
                Exit Sub
            End If

            '*** do really nothing
        End Sub
        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCheckinqUniquenessDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnCheckingUniquenessDataObject
            If _isStarted AndAlso e.UseCache Then
                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ormPrimaryKey(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedLoadedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedLoadedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ormPrimaryKey(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        aBucket.LastAccessStamp = DateTime.Now
                        e.Proceed = False
                        e.AbortOperation = True ' abort creating use object instead
                        Exit Sub
                    End If
                End If

                e.Proceed = True
                e.AbortOperation = False
                Exit Sub
            End If
        End Sub

    End Class
End Namespace