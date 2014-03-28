﻿REM ***********************************************************************************************************************************************
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

Namespace OnTrack.database

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
        Sub OnRetrievedDataObject(sender As Object, e As OnTrack.database.ormDataObjectEventArgs)

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub OnCheckingUniquenessDataObject(sender As Object, e As OnTrack.Database.ormDataObjectEventArgs)

        Sub OnInfusedDataObject(sender As Object, e As ormDataObjectEventArgs)

        Sub OnInfusingDataObject(sender As Object, e As ormDataObjectEventArgs)

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
        ''' Registery with some meta information
        ''' </summary>
        ''' <remarks></remarks>
        Public Class RegisteryEntry(Of T)
            Private _objecttype As System.Type
            Private _objecttypename As String = ""
            Private _noKeys As UShort = 0

            Private _lockobject As New Object

            ''' <summary>
            ''' constructor with an ormDataObject Class Type
            ''' </summary>
            ''' <param name="type"></param>
            ''' <remarks></remarks>
            Public Sub New([type] As System.Type)
                If [type].GetInterfaces.Contains(GetType(T)) OrElse [type].IsAssignableFrom(GetType(T)) Then
                    Dim aDescriptor = ot.GetObjectClassDescription([type])
                    If aDescriptor IsNot Nothing Then
                        _noKeys = aDescriptor.PrimaryKeyEntryNames.Count
                    Else
                        Throw New Exception("registerentry: descriptor not found")
                    End If
                Else
                    Throw New Exception("registeryEntry: " & [type].Name & " has no interface or base class for " & GetType(T).Name)
                End If
            End Sub
            ''' <summary>
            ''' Gets the objecttype.
            ''' </summary>
            ''' <value>The objecttype.</value>
            Public ReadOnly Property Objecttype() As Type
                Get
                    Return Me._objecttype
                End Get
            End Property

            ''' <summary>
            ''' Gets the objecttypename.
            ''' </summary>
            ''' <value>The objecttypename.</value>
            Public ReadOnly Property Objecttypename() As String
                Get
                    If _objecttype IsNot Nothing Then Return Me._objecttype.Name
                    Return ""
                End Get
            End Property

            ''' <summary>
            ''' Gets the no keys.
            ''' </summary>
            ''' <value>The no keys.</value>
            Public ReadOnly Property NoKeys() As UShort
                Get
                    Return Me._noKeys
                End Get
            End Property

        End Class

        ''' <summary>
        ''' the generic object unique key class
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ObjectKeys(Of T)
            Implements IHashCodeProvider
            Implements IQueryable


            '** Keys is an array of objects
            Private _keys() As Object
            Private _registery As RegisteryEntry(Of T)

            Private _lockobject As New Object

            ''' <summary>
            ''' constructor of an keyentry
            ''' </summary>
            ''' <param name="registeryentry"></param>
            ''' <remarks></remarks>
            Public Sub New(registeryentry As RegisteryEntry(Of T))
                _registery = registeryentry
                ReDim _keys(_registery.NoKeys)
            End Sub
            Public Sub New(nokeys As UShort)
                ReDim _keys(nokeys)
            End Sub
            Public Sub New(keys() As Object)
                _keys = keys
            End Sub
            ''' <summary>
            ''' Gets or sets the keys.
            ''' </summary>
            ''' <value>The keys.</value>
            Public Property Keys() As Object()
                Get
                    Return Me._keys
                End Get
                Set(value As Object())
                    If value.GetUpperBound(0) <> _registery.NoKeys - 1 Then Throw New Exception("keys of this type have different bound")
                    Me._keys = value
                End Set
            End Property

            ''' <summary>
            ''' returns a hash value for the keys
            ''' </summary>
            ''' <param name="obj"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overloads Function GetHashCode(o As Object) As Integer Implements IHashCodeProvider.GetHashCode
                Dim aKey As ObjectKeys(Of T) = TryCast(o, ObjectKeys(Of T))
                If aKey Is Nothing Then Return o.GetHashCode
                If aKey.Keys Is Nothing Then Return 0

                Dim hashvalue As Integer = 0
                For i = 0 To aKey.Keys.Count - 1
                    If aKey.Keys(i) Is Nothing Then
                        hashvalue = hashvalue Xor 0
                    Else
                        hashvalue = hashvalue Xor aKey.Keys(i).GetHashCode()
                    End If
                Next
                Return hashvalue
            End Function
            ''' <summary>
            ''' Equal routine of 2 keys
            ''' </summary>
            ''' <param name="obj"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Equals(obj As Object) As Boolean
                Dim aKey As ObjectKeys(Of T) = TryCast(obj, ObjectKeys(Of T))
                If aKey Is Nothing Then
                    Return False
                Else
                    If (aKey.Keys Is Nothing AndAlso Me.Keys IsNot Nothing) OrElse _
                        (aKey.Keys IsNot Nothing AndAlso Me.Keys Is Nothing) Then
                        Return False
                    End If
                    If (aKey.Keys Is Nothing AndAlso Me.Keys Is Nothing)  Then
                        Return True
                    End If

                    If aKey.Keys.Count <> Me.Keys.Count Then Return False
                    For i = 0 To aKey.Keys.Count - 1
                        If Not aKey.Keys(i).Equals(Me.Keys(i)) Then Return False
                    Next
                    Return True
                    End If
            End Function
            ''' <summary>
            ''' returns a hash value for the keys
            ''' </summary>
            ''' <param name="obj"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetHashCode() As Integer
                Return Me.GetHashCode(Me)
            End Function

            ''' <summary>
            ''' get an enumerator
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return _keys.ToList
            End Function

            Public ReadOnly Property ElementType As Type Implements IQueryable.ElementType
                Get

                End Get
            End Property

            Public ReadOnly Property Expression As Expressions.Expression Implements IQueryable.Expression
                Get

                End Get
            End Property

            Public ReadOnly Property Provider As IQueryProvider Implements IQueryable.Provider
                Get

                End Get
            End Property

            ''' <summary>
            ''' toString
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function ToString() As String
                If _keys IsNot Nothing Then
                    Dim s As String = "["
                    For i = 0 To _keys.Count - 1
                        If s <> "[" Then s &= ","
                        s &= _keys(i).ToString
                    Next
                    Return s & "]"
                Else
                    Return "[]"
                End If

            End Function
        End Class

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
        Private _registeredObjects As New Dictionary(Of String, RegisteryEntry(Of iormPersistable))

        ''' <summary>
        ''' the Object Cache
        ''' </summary>
        ''' <remarks></remarks>
        Private _cachedObjects As New SortedList(Of String, Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable)))

        ''' <summary>
        ''' dynamic
        ''' </summary>
        ''' <remarks></remarks>
        Private _isInitialized As Boolean = False
        Private _isStarted As Boolean = False

        Private WithEvents _session As Session
        Private _lockObject As New Object
        Private _ormDataObjectIsHooked As Boolean = False ' VERY BAD : avoid shared events in this base class to be event handled multiple times

        ''' Define the Assignments of shared iorm persistable Events to the Cache Methods
        ''' IMPORTANT !
        Private _assignments As String(,) = {{"ClassOnInfusing", "OnInfusingDataObject"}, _
                                             {"ClassOnInfused", "OnInfusedDataObject"}, _
                                             {"ClassOnRetrieved", "OnRetrievedDataObject"}, _
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
        Sub New(session As Session)
            _session = session
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnSessionStart(sender As Object, e As SessionEventArgs) Handles _session.OnStarted
            Me.Start()
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
            _cachedObjects.Clear()
            _registeredObjects.Clear()
            _isStarted = False
        End Function

        ''' <summary>
        ''' flush the cache
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function FlushCache() As Boolean
            _cachedObjects.Clear()
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
            Dim anEntry As RegisteryEntry(Of iormPersistable)
            Try

                If Not _registeredObjects.ContainsKey(key:=typename) Then
                    Dim aType = ot.GetObjectClassType(objectname:=typename)
                    If aType IsNot Nothing Then
                        anEntry = New RegisteryEntry(Of iormPersistable)(aType)
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
                                    ''' therefore manually check for base clas ormDataObject and set flag and skip it 
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
            Dim anEntry As RegisteryEntry(Of iormPersistable)
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
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        aBucket.LastAccessStamp = DateTime.Now
                        e.Result = True ' yes we have a result
                        e.AbortOperation = True ' abort creating use object instead
                        Exit Sub
                    Else
                        e.AbortOperation = False
                        e.Result = True
                        Exit Sub
                    End If
                Else
                    e.AbortOperation = False
                    e.Result = True
                    Exit Sub
                End If
            End If
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
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.IsCreated = True
                    aBucket.CreationDate = DateTime.Now
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)

                    e.AbortOperation = False
                    e.Result = True 'success
                    Exit Sub
                Else
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject.Guid <> e.DataObject.Guid Then
                        CoreMessageHandler("Warning ! objects of same type and keys already in cache", subname:="ormObjectCacheManager.OnCreatedDataObject", messagetype:=otCoreMessageType.InternalWarning, _
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
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        aBucket.LastAccessStamp = DateTime.Now
                        e.Result = True 'success
                        e.AbortOperation = True ' abort cloning use object insted
                        Exit Sub
                    Else
                        e.AbortOperation = False
                        e.Result = True
                        Exit Sub
                    End If
                Else
                    e.AbortOperation = False
                    e.Result = False
                    Exit Sub
                End If
            End If
            e.AbortOperation = False
            e.Result = False
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnCloned Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnClonedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnClonedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub
                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.CreationDate = DateTime.Now
                    aBucket.IsCreated = True
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    e.Result = True 'success
                    Exit Sub
                Else
                    CoreMessageHandler("Warning ! cloned Object already in cache", subname:="ormObjectCacheManager.OnRetrievedDataObject", messagetype:=otCoreMessageType.InternalWarning, _
                                        objectname:=e.DataObject.GetType.Name, arg1:=e.Pkarray)
                    e.DataObject = Nothing
                    e.Result = False
                    Exit Sub
                End If

            End If

            e.Result = False
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object - mark object as deleted
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDeletedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnDeletedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
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
                e.Result = True 'success
                Exit Sub
            End If
            e.AbortOperation = False
            e.Result = False
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnUnDeletedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnUnDeletedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                Dim aBucket As CachedObject(Of iormPersistable)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                Else
                    aBucket = theobjects.Item(key:=searchkeys)
                End If
                aBucket.LastAccessStamp = DateTime.Now
                aBucket.IsDeleted = False
                e.AbortOperation = False
                e.Result = True 'success
                Exit Sub
            End If
            e.AbortOperation = False
            e.Result = False
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnPersisted Event Handler for the ORM Data Object - check if object needs to be added and set persistance timestamp
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnPersistedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnPersistedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub

                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
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
                e.Result = True 'success
                Exit Sub
            End If
            e.AbortOperation = False
            e.Result = True
            Exit Sub
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
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        aBucket.LastAccessStamp = DateTime.Now
                        e.Result = True 'success
                        e.AbortOperation = False
                        Exit Sub
                    Else
                        e.DataObject = Nothing
                        e.Result = False
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
        ''' OnRetrieved Event Handler for the ORM Data Object - add retrieved object to cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRetrievedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnRetrievedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub


                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                If Not theobjects.ContainsKey(key:=searchkeys) Then
                    Dim aBucket = New CachedObject(Of iormPersistable)(e.DataObject)
                    aBucket.RetrieveData = DateTime.Now
                    aBucket.IsRetrieved = True
                    theobjects.TryAdd(key:=searchkeys, value:=aBucket)
                    e.AbortOperation = False
                    e.Result = True 'success
                    Exit Sub
                Else
                    ''' it might be that a retrieved object was stored 
                    ''' previously under infused 
                    ''' to check on this we would need a GUID for each Bucket
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    e.DataObject = TryCast(aBucket.Object, ormDataObject)
                    aBucket.RetrieveData = DateTime.Now
                    aBucket.IsRetrieved = True
                    e.Result = True ' do nothing in the case
                    e.AbortOperation = False
                    Exit Sub
                End If

            End If

            e.AbortOperation = False
            e.Result = False
            Exit Sub
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
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        aBucket.LastAccessStamp = DateTime.Now
                        Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                        If aDataObject.Guid <> e.DataObject.Guid Then
                            '** return the existing object
                            e.DataObject = aDataObject
                            e.Result = True
                            e.AbortOperation = True
                            Exit Sub
                        Else
                            e.Result = False
                            e.AbortOperation = False
                            Exit Sub
                        End If
                        
                    Else
                        e.AbortOperation = False
                        e.Result = True
                        Exit Sub
                    End If
                Else
                    e.AbortOperation = False
                    e.Result = True
                    Exit Sub
                End If
            End If
            e.AbortOperation = False
            e.Result = True
            Exit Sub
        End Sub

        ''' <summary>
        ''' OnRetrieved Event Handler for the ORM Data Object - add retrieved object to cache
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfusedDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnInfusedDataObject
            If _isStarted AndAlso e.UseCache Then
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                If e.DataObject Is Nothing OrElse e.Pkarray Is Nothing OrElse e.Pkarray.Count = 0 Then Exit Sub


                '** get the data
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    Else
                        theobjects = New Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))()
                        _cachedObjects.Add(key:=e.DataObject.GetType.Name, value:=theobjects)
                    End If
                End SyncLock

                Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
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
                    e.AbortOperation = False
                    e.Result = True 'success
                    Exit Sub
                Else
                    Dim aBucket = theobjects.Item(key:=searchkeys)
                    Dim aDataObject = TryCast(aBucket.Object, ormDataObject)
                    If aDataObject.Guid <> e.DataObject.Guid Then
                        CoreMessageHandler("Warning ! infused object already in cache", subname:="ormObjectCacheManager.OnInfusedDataObject", _
                                           messagetype:=otCoreMessageType.InternalWarning, _
                                          objectname:=aDataObject.ObjectID, arg1:=e.Pkarray)
                        e.DataObject = aDataObject
                        e.Result = False
                        e.AbortOperation = True
                        Exit Sub
                    Else
                        e.DataObject = aDataObject
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
        ''' OnDeleted Event Handler for the ORM Data Object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCheckinqUniquenessDataObject(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectCacheManager.OnCheckingUniquenessDataObject
            If _isStarted AndAlso e.UseCache Then
                '** get the data
                Dim theobjects As Concurrent.ConcurrentDictionary(Of ObjectKeys(Of iormPersistable), CachedObject(Of iormPersistable))
                SyncLock _lockObject
                    If _cachedObjects.ContainsKey(e.DataObject.GetType.Name) Then
                        theobjects = _cachedObjects.Item(e.DataObject.GetType.Name)
                    End If
                End SyncLock
                If theobjects IsNot Nothing Then
                    Dim searchkeys = New ObjectKeys(Of iormPersistable)(e.Pkarray)
                    If theobjects.ContainsKey(key:=searchkeys) Then
                        Dim aBucket = theobjects.Item(key:=searchkeys)
                        e.DataObject = TryCast(aBucket.Object, ormDataObject)
                        aBucket.LastAccessStamp = DateTime.Now
                        e.Proceed = False
                        e.Result = False 'success
                        e.AbortOperation = True ' abort creating use object instead
                        Exit Sub
                    Else
                        e.Result = True
                        e.Proceed = True
                        e.AbortOperation = False
                        Exit Sub
                    End If
                Else
                    e.Result = True
                    e.Proceed = True
                    e.AbortOperation = False
                    Exit Sub
                End If
            End If
            e.Proceed = True
            e.Result = True
            e.AbortOperation = False
            Exit Sub
        End Sub

    End Class
End Namespace