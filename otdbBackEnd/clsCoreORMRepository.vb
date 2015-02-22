Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Object Description Repository Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Text.RegularExpressions
Imports System.Collections.Concurrent

Imports System.IO
Imports System.Threading

Imports OnTrack
Imports OnTrack.Database
Imports System.Reflection
Imports OnTrack.Commons

Namespace OnTrack.Database

    ''' <summary>
    ''' static class for Database Constants
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Constants


    End Class
    ''' <summary>
    ''' store for all the  OTDB object information - loaded on connecting with the 
    ''' session in the domain
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectRepository

        '*** Event Arguments
        Public Class EventArgs
            Inherits System.EventArgs

            Private _objectname As String
            Private _objectdefinition As ObjectDefinition

            ''' <summary>
            ''' constructor
            ''' </summary>
            ''' <param name="objectname"></param>
            ''' <param name="description"></param>
            ''' <remarks></remarks>
            Public Sub New(objectname As String, objectdefinition As ObjectDefinition)
                _objectname = objectname
                _objectdefinition = objectdefinition
            End Sub

            ''' <summary>
            ''' Gets the objectdefinition.
            ''' </summary>
            ''' <value>The objectdefinition.</value>
            Public ReadOnly Property Objectdefinition() As ObjectDefinition
                Get
                    Return Me._objectdefinition
                End Get
            End Property

            ''' <summary>
            ''' Gets the objectname.
            ''' </summary>
            ''' <value>The objectname.</value>
            Public ReadOnly Property Objectname() As String
                Get
                    Return Me._objectname
                End Get
            End Property

        End Class


        Private _IsInitialized As Boolean = False
        '** cache of the objects by Object name
        Private _objectDirectory As New Dictionary(Of String, ObjectDefinition)
        '** cache of the objects by Object class name
        Private _objectClassDirectory As New Dictionary(Of String, ObjectDefinition)
        '** cache on the columns object 
        Private _entryDirectory As New Dictionary(Of String, iormObjectEntry)
        '** cache of all Table Definitions
        Private _tableDirectory As New Dictionary(Of String, ContainerDefinition)
        '** reference to all the XChange IDs
        Private _XIDDirectory As New Dictionary(Of String, List(Of iormObjectEntry))
        '** reference to all the aliases
        Private _aliasDirectory As New Dictionary(Of String, List(Of iormObjectEntry))

        Private _xidShortReference As Dictionary(Of String, List(Of String)) ' dictionary for cross referenceing
        Private _aliasShortReference As Dictionary(Of String, List(Of String)) ' dictionary for cross referencing

        '** reference to the session 
        Private _DomainID As String = String.empty
        Private WithEvents _Domain As Domain
        Private WithEvents _Session As Session ' reference to session which we belong

        Private _lock As New Object

        Public Event OnObjectDefinitionLoaded(sender As Object, e As ObjectRepository.EventArgs)
        ''' <summary>
        ''' construction with link to the connection
        ''' </summary>
        ''' <param name="connection"></param>
        ''' <remarks></remarks>

        Sub New(ByRef session As Session, domainid As String)
            _Session = session
            _DomainID = domainid
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the is initialiazed.
        ''' </summary>
        ''' <value>The is initialiazed.</value>
        Public Property IsInitialized() As Boolean
            Get
                Return Me._IsInitialized
            End Get
            Private Set(value As Boolean)
                Me._IsInitialized = value
            End Set
        End Property
        ''' <summary>
        ''' gets a list of all ObjectDefinitions in the repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectDefinitions As IEnumerable(Of ObjectDefinition)
            Get
                Return _objectDirectory.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a list of all ObjectEntry in the repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectEntryDefinitions As IEnumerable(Of iormObjectEntry)
            Get
                Return _entryDirectory.Values.ToList
            End Get
        End Property


        ''' <summary>
        ''' gets a list of all Xchange IDs in the repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property XIDs As IEnumerable(Of String)
            Get
                Return _XIDDirectory.Keys.ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a list of all Xchange Aliases in the repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Aliases As IEnumerable(Of String)
            Get
                Return _aliasDirectory.Keys.ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a list of all ObjectDefinitions in the repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableDefinitions As IEnumerable(Of ContainerDefinition)
            Get
                Return _tableDirectory.Values.ToList
            End Get
        End Property

#End Region

        ''' <summary>
        ''' registers a cache manager for this repository
        ''' </summary>
        ''' <param name="cache"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterCache(cache As iormObjectCacheManager) As Boolean
            AddHandler OnObjectDefinitionLoaded, AddressOf cache.OnObjectDefinitionLoaded
        End Function
        ''' <summary>
        ''' if an Object Definition changes
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnObjectDefinitionChanged(sender As Object, ent As ObjectDefinition.EventArgs)
            Dim anObjectDef As ObjectDefinition = ObjectDefinition.Retrieve(objectname:=ent.Objectname, domainid:=_DomainID)

            If anObjectDef IsNot Nothing Then
                If LoadIntoRepository(anObjectDef) Then
                    CoreMessageHandler(message:="object definition of " & ent.Objectname & " was reloaded in the Objects store", messagetype:=otCoreMessageType.InternalInfo)
                End If
            End If
        End Sub
        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnDomainInitialize(sender As Object, e As DomainEventArgs) Handles _Domain.OnInitialize
            If String.IsNullOrWhiteSpace(_DomainID) And Not IsInitialized Then
                If e.Domain IsNot Nothing Then _DomainID = e.Domain.ID
            End If

        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnDomainReset(sender As Object, e As DomainEventArgs) Handles _Domain.OnReset

        End Sub
        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnSessionStart(sender As Object, e As SessionEventArgs) Handles _Session.OnStarted
            If Not Me.IsInitialized Then
                ' Initialize if session is starting this domain repository
                If _DomainID = e.Session.CurrentDomainID Then IsInitialized = Me.Initialize
            End If
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnSessionEnd(sender As Object, e As SessionEventArgs) Handles _Session.OnEnding
            If Me.IsInitialized Then
                IsInitialized = Not Reset()
            End If
        End Sub

        ''' <summary>
        ''' Add an Entry by XID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddXID(ByRef entry As iormObjectEntry) As Boolean
            Dim entries As List(Of iormObjectEntry)

            If _XIDDirectory.ContainsKey(key:=UCase(entry.XID)) Then
                entries = _XIDDirectory.Item(key:=UCase(entry.XID))
            Else
                entries = New List(Of iormObjectEntry)
                SyncLock _lock
                    _XIDDirectory.Add(key:=UCase(entry.XID), value:=entries)
                End SyncLock
            End If

            SyncLock _lock
                entries.Add(entry)
            End SyncLock

            Return True
        End Function
        ''' <summary>
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddAlias(ByRef entry As iormObjectEntry) As Boolean
            Dim entries As List(Of iormObjectEntry)
            If entry.Aliases Is Nothing Then Return True

            For Each [alias] As String In entry.Aliases

                If _aliasDirectory.ContainsKey(key:=UCase([alias])) Then
                    entries = _aliasDirectory.Item(key:=UCase([alias]))
                Else
                    entries = New List(Of iormObjectEntry)
                    SyncLock _lock
                        _aliasDirectory.Add(key:=UCase([alias]), value:=entries)
                    End SyncLock
                End If

                SyncLock _lock
                    entries.Add(entry)
                End SyncLock
            Next

            Return True
        End Function
        ''' <summary>
        ''' reset all the data of the meta store
        ''' </summary>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Private Function Reset() As Boolean
            SyncLock _lock
                _aliasDirectory.Clear()
                _tableDirectory.Clear()
                _XIDDirectory.Clear()
                _objectDirectory.Clear()
                _entryDirectory.Clear()
                _Domain = Nothing
                _DomainID = String.empty
                _IsInitialized = False
                _Session = Nothing
            End SyncLock
            Return True
        End Function


        ''' <summary>
        ''' handler for the domain Changed Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDomainChanged(sender As Object, e As SessionEventArgs) Handles _Session.OnDomainChanged
            Dim aDomain As String
            SyncLock _lock
                aDomain = DirectCast(sender, Session).CurrentDomainID
            End SyncLock
            '** initialize the repository if we switched to the domain of it
            If aDomain = _DomainID Then Initialize()
        End Sub
        ''' <summary>
        ''' Initialize the repository and load the minimum objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize(Optional force As Boolean = False) As Boolean
            Dim aDBDriver As iormRelationalDatabaseDriver

            '* donot doe it again
            If Me.IsInitialized AndAlso Not force Then Return False

            If String.IsNullOrWhiteSpace(_DomainID) Then
                CoreMessageHandler(message:="DomainID is not set in objectStore", argument:=Me._Session.SessionID, messagetype:=otCoreMessageType.InternalError, _
                                   procedure:="ObjectRepository.Initialize")
                Return False
            End If

            '* too eaarly
            If _Session Is Nothing OrElse _Session.CurrentDBDriver Is Nothing _
            OrElse Not _Session.CurrentDBDriver.CurrentConnection.IsConnected Then
                Return False
            End If

            If _Session IsNot Nothing AndAlso _Session.IsRunning Then
                aDBDriver = _Session.CurrentDBDriver
            ElseIf Not _Session.IsBootstrappingInstallationRequested Then
                aDBDriver = GetTableStore(ObjectDefinition.ConstPrimaryTableID).Connection.DatabaseDriver
            Else
                CoreMessageHandler(message:="not able to get database driver", argument:=_Session.SessionID, messagetype:=otCoreMessageType.InternalError, _
                                    procedure:="ObjectRepository.Initialize")
                Return False
            End If

            Dim theObjectnames() As String
            Dim objectsToLoad As Object = ot.GetDBParameter(ot.ConstPNObjectsLoad, silent:=True)
            Dim delimiters() As Char = {",", ";", ConstDelimiter}

            If objectsToLoad IsNot Nothing And Not _Session.IsBootstrappingInstallationRequested Then
                SyncLock _lock
                    If objectsToLoad.ToString = "*" Then
                        theObjectnames = ObjectDefinition.AllActiveObjectNames(dbdriver:=aDBDriver).ToArray
                    Else
                        theObjectnames = objectsToLoad.ToString.Split(delimiters)
                    End If

                    CoreMessageHandler(message:="Initializing " & ot.GetBootStrapObjectClassIDs.Count & " OnTrack Bootstrapping Objects in Domain '" & _DomainID & "' ....", messagetype:=otCoreMessageType.ApplicationInfo, procedure:="ObjectRepository.Initialize")

                    Dim i As UShort = 1

                    '** load the bootstrapping core
                    For Each name In ot.GetBootStrapObjectClassIDs
                        name = Trim(name.ToUpper) ' for some reasons better to trim

                        Dim anObject As ObjectDefinition = Me.GetObject(objectid:=name, domainid:=_DomainID)

                        'ObjectDefinition.Retrieve(objectname:=name, dbdriver:=aDBDriver, domainID:=_DomainID)
                        If anObject IsNot Nothing Then
                            CoreMessageHandler(message:="Initialized OnTrack " & i & "/" & ot.GetBootStrapObjectClassIDs.Count & " Bootstrapping Object " & name & " in " & _DomainID, messagetype:=otCoreMessageType.ApplicationInfo, procedure:="ObjectRepository.Initialize")

                        Else
                            CoreMessageHandler(message:="could not load object '" & name & "'", messagetype:=otCoreMessageType.InternalError, _
                                               procedure:="ObjectRepository.Initialize")
                        End If
                        i += 1
                    Next
                    i = 1
                    CoreMessageHandler(message:="Initializing " & theObjectnames.Count & " OnTrack Objects ....", messagetype:=otCoreMessageType.ApplicationInfo, procedure:="ObjectRepository.Initialize")
                    '** load all objects with entries and aliases
                    For Each name In theObjectnames
                        name = Trim(name.ToUpper) ' for some reasons bette to trim

                        Dim anObject As ObjectDefinition = Me.GetObject(objectid:=name, domainid:=_DomainID)
                        If anObject IsNot Nothing Then
                            CoreMessageHandler(message:="Initialized " & i & "/" & theObjectnames.Count & " in " & _DomainID & " OnTrack Object " & name, messagetype:=otCoreMessageType.ApplicationInfo, procedure:="ObjectRepository.Initialize")

                        Else
                            CoreMessageHandler(message:="could not load object '" & name & "'", messagetype:=otCoreMessageType.InternalError, _
                                               procedure:="ObjectRepository.Initialize")
                        End If
                        i += 1
                    Next
                End SyncLock
            End If

            ''' load the cross references
            ''' 
            _xidShortReference = AbstractEntryDefinition.GetXIDReference(domainid:=_DomainID)
            _aliasShortReference = AbstractEntryDefinition.GetAliasReference(domainid:=_DomainID)

            SyncLock _lock
                Me.IsInitialized = True
            End SyncLock

            CoreMessageHandler(message:="Objects initialized for Domain '" & _DomainID & " in Session " & CurrentSession.SessionID & "' - " & _objectDirectory.Count & " objects loaded", _
                               messagetype:=otCoreMessageType.ApplicationInfo, procedure:="ObjectRepository.Initialize")

            Return Me.IsInitialized
        End Function

        ''' <summary>
        ''' Load Object into Store of Objects
        ''' </summary>
        ''' <param name="object"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function LoadIntoRepository(ByRef [object] As ObjectDefinition) As Boolean

            If Not [object].IsAlive(throwError:=False) Then
                Call CoreMessageHandler(message:="object is neither created nor loaded", procedure:="ObjectRepository.LoadIntoRepository", _
                                        containerID:=[object].ID, messagetype:=otCoreMessageType.InternalError)

                Return False
            End If

            '*** check if version is the same as in code
            Dim aTableAttribute As ormTableAttribute = ot.GetSchemaTableAttribute(tablename:=[object].ID)
            If aTableAttribute IsNot Nothing Then
                If [object].Version <> aTableAttribute.Version Then
                    '_Session.CurrentDBDriver.VerifyOnTrackDatabase(verifyOnly:=False, createOnMissing:=True)
                    CoreMessageHandler(message:="Attention ! Version of object in object store V" & [object].Version & " is different from version in code V" & aTableAttribute.Version, _
                                       messagetype:=otCoreMessageType.InternalWarning, containerID:=[object].ID, procedure:="ObjectStore.LoadIntoRepository")
                End If
            End If


            '** save it
            If _objectDirectory.ContainsKey([object].ID) Then
                _objectDirectory.Remove([object].ID)
            End If
            SyncLock _lock
                _objectDirectory.Add(key:=[object].ID, value:=[object])
            End SyncLock
            '** save it
            If _objectClassDirectory.ContainsKey([object].Classname) Then
                _objectClassDirectory.Remove([object].Classname)
            End If
            SyncLock _lock
                _objectClassDirectory.Add(key:=[object].Classname, value:=[object])
            End SyncLock
            '** load the table definitions
            For Each aTableDefinition In [object].Tables
                If Not _tableDirectory.ContainsKey(key:=aTableDefinition.ID) Then
                    _tableDirectory.Add(key:=aTableDefinition.ID, value:=aTableDefinition)
                End If
            Next
            For Each anEntry As iormObjectEntry In [object].GetEntries
                ' save the entry
                If _entryDirectory.ContainsKey(key:=[object].ID & "." & anEntry.Entryname) Then
                    SyncLock _lock
                        _entryDirectory.Remove(key:=[object].ID & "." & anEntry.Entryname)
                    End SyncLock
                End If
                SyncLock _lock
                    _entryDirectory.Add(key:=[object].ID & "." & anEntry.Entryname, value:=anEntry)
                End SyncLock

                '** cross references
                Me.AddXID(entry:=anEntry)
                Me.AddAlias(entry:=anEntry)

            Next

            RaiseEvent OnObjectDefinitionLoaded(Me, New ObjectRepository.EventArgs(objectname:=[object].ID, objectdefinition:=[object]))
            Return True
        End Function
        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetTable(tablename As String, Optional runtimeOnly As Boolean? = Nothing) As ContainerDefinition
            ' Me.Initialize() -> recursion since this function  is used on initializing
            tablename = tablename.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            If tablename.Contains("."c) Then
                tablename = Shuffle.NameSplitter(tablename).First
            End If

            '** name is given
            If tablename <> String.empty Then
                If _tableDirectory.ContainsKey(tablename) Then
                    Return _tableDirectory.Item(tablename)
                Else

                    '** no runtime -> better ask the session
                    If Not runtimeOnly Then runtimeOnly = _Session.IsBootstrappingInstallationRequested
                    Dim aList = ot.GetObjectClassDescriptionByTable(tableid:=tablename)

                    '** load the objects belonging to that class !
                    For Each classdescription In aList
                        Dim objectname As String = classdescription.ID
                        '** retrieve Object
                        Dim anObject = ObjectDefinition.Retrieve(objectname:=objectname, domainid:=_DomainID, runtimeOnly:=runtimeOnly)
                        '** no object in persistancy but creatable from class description
                        If anObject Is Nothing Then
                            anObject = ObjectDefinition.Create(objectID:=objectname, runTimeOnly:=runtimeOnly)
                            If anObject Is Nothing Then
                                CoreMessageHandler(message:="Failed to retrieve the object definition in non runtime mode", argument:=objectname, _
                                                    objectname:=objectname, messagetype:=otCoreMessageType.InternalError, procedure:="ObjectRepository.getTable")
                                Return Nothing
                            ElseIf Not anObject.SetupByClassDescription(ot.GetObjectClassType(objectname:=objectname), runtimeOnly:=runtimeOnly) Then
                                CoreMessageHandler(message:="Failed to setup the object definition from the object class description", argument:=objectname, _
                                                    objectname:=objectname, messagetype:=otCoreMessageType.InternalError, procedure:="ObjectRepository.getTable")
                                Return Nothing
                            End If
                        End If
                        If anObject IsNot Nothing Then
                            '*** add to repository and try again
                            LoadIntoRepository(anObject)
                            If _tableDirectory.ContainsKey(tablename) Then
                                Return _tableDirectory.Item(tablename)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If

                    Next


                    Return Nothing
                End If
            Else
                Return Nothing
            End If

            Return Nothing '** not found
        End Function
        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetColumnEntry(columnname As String, Optional tableid As String = Nothing, Optional runtimeOnly As Boolean? = Nothing) As ContainerEntryDefinition
            ' Me.Initialize() -> recursion since this function  is used on initializing
            columnname = columnname.ToUpper
            If Not String.IsNullOrWhiteSpace(tableid) Then tableid = tableid.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            If String.IsNullOrWhiteSpace(tableid) And columnname.Contains(".") Then
                Shuffle.NameSplitter(columnname, tableid, columnname)
            End If

            '** name is given
            If Not String.IsNullOrWhiteSpace(tableid) Then
                If _tableDirectory.ContainsKey(tableid) Then
                    Dim aTable = _tableDirectory.Item(tableid)
                    If aTable.HasEntry(columnname) Then
                        Return aTable.GetEntry(columnname)
                    Else
                        Return Nothing
                    End If
                    ' try to load

                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If

            Return Nothing '** not found
        End Function
        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntry(entryname As String, Optional objectname As String = Nothing, Optional runtimeOnly As Boolean? = Nothing) As iormObjectEntry
            ' Me.Initialize() -> recursion since this function  is used on initializing
            entryname = entryname.ToUpper
            If Not String.IsNullOrWhiteSpace(objectname) Then objectname = objectname.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            '** objectname is given
            If Not String.IsNullOrWhiteSpace(objectname) Then

                If HasEntry(objectname:=objectname, entryname:=entryname) Then
                    Return _entryDirectory.Item(key:=objectname & "." & entryname)
                    ' try to load
                ElseIf Not HasObject(objectid:=objectname) Then
                    If Me.GetObject(objectid:=objectname, runtimeOnly:=runtimeOnly) IsNot Nothing Then
                        If HasEntry(objectname:=objectname, entryname:=entryname) Then
                            Return _entryDirectory.Item(key:=objectname & "." & entryname)
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If

                '** try to find it by entryname only
            Else
                Dim aName As String = _entryDirectory.Keys.ToList.Find(Function(n As String)
                                                                           Return entryname.ToUpper = Shuffle.NameSplitter(n).Last
                                                                       End Function)
                If Not String.IsNullOrEmpty(aName) Then
                    Return _entryDirectory.Item(key:=aName)
                End If

            End If

            Return Nothing '** not found
        End Function

        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function HasObject(objectid As String) As Boolean
            ' Me.Initialize() -> recursion since this function  is used on initializing

            If _objectDirectory.ContainsKey(key:=objectid.ToUpper) Then
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' retrieves an Object by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetObjectByClassname(classname As String, Optional runtimeOnly As Boolean = False) As ObjectDefinition
            ' Me.Initialize() -> recursion since this function  is used on initializing
            If _objectClassDirectory.ContainsKey(key:=classname) Then
                Return _objectClassDirectory.Item(key:=classname)
                ' try to reload
            Else
            End If
        End Function
        ''' <summary>
        ''' retrieves an Object by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetObject(objectid As String, Optional domainid As String = Nothing, Optional runtimeOnly As Boolean = False) As ObjectDefinition
            ' Me.Initialize() -> recursion since this function  is used on initializing
            Dim anObject As ObjectDefinition
            objectid = objectid.ToUpper
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            If _objectDirectory.ContainsKey(key:=objectid) Then
                Return _objectDirectory.Item(key:=objectid)
                ' try to reload
            Else
                '** no runtime -> better ask the session
                If Not runtimeOnly Then runtimeOnly = _Session.IsBootstrappingInstallationRequested
                '** retrieve Object

                anObject = ObjectDefinition.Retrieve(objectname:=objectid, domainid:=domainid, runtimeOnly:=runtimeOnly)
                '** no object in persistancy but creatable from class description
                If anObject Is Nothing AndAlso ot.GetObjectClassDescriptionByID(id:=objectid) IsNot Nothing Then
                    anObject = ObjectDefinition.Create(objectID:=objectid, domainid:=domainid, runTimeOnly:=runtimeOnly)
                    If anObject Is Nothing Then
                        CoreMessageHandler(message:="Failed to retrieve the object definition in non runtime mode", argument:=objectid, _
                                            objectname:=objectid, messagetype:=otCoreMessageType.InternalError, procedure:="ObjectRepository.getObject")
                        Return Nothing
                    ElseIf Not anObject.SetupByClassDescription(ot.GetObjectClassType(objectname:=objectid), runtimeOnly:=runtimeOnly) Then
                        CoreMessageHandler(message:="Failed to setup the object definition from the object class description", argument:=objectid, _
                                            objectname:=objectid, messagetype:=otCoreMessageType.InternalError, procedure:="ObjectRepository.getObject")
                        Return Nothing
                    End If
                End If
                If anObject IsNot Nothing Then

                    '*** add to repository
                    LoadIntoRepository(anObject)
                    If HasObject(objectid:=objectid) Then
                        Return _objectDirectory.Item(key:=objectid)
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If
            End If

        End Function

        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function HasEntry(objectname As String, entryname As String) As Boolean
            ' Me.Initialize() -> recursion since this function  is used on initializing
            If _entryDirectory.ContainsKey(key:=objectname & "." & entryname) Then
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntries(objectname As String) As List(Of iormObjectEntry)
            ' Me.Initialize() -> recursion since this function  is used on initializing
            If _objectDirectory.ContainsKey(key:=objectname) Then
                Return _objectDirectory.Item(key:=objectname).GetEntries
            Else
                Return New List(Of iormObjectEntry)
            End If

        End Function

        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntriesByXID([xid] As String, Optional objectname As String = Nothing) As IList(Of iormObjectEntry)
            ' Me.Initialize() -> recursion since this function  is used on initializing
            xid = xid.ToUpper
            If Not String.IsNullOrWhiteSpace(objectname) Then objectname = objectname.ToUpper
            If _XIDDirectory.ContainsKey(xid) Then
                If String.IsNullOrWhiteSpace(objectname) Then
                    Return _XIDDirectory.Item(xid)
                Else
                    Dim aList As New List(Of iormObjectEntry)
                    For Each objectdef In _XIDDirectory.Item(key:=xid)
                        If objectname = objectdef.Objectname.ToUpper Then
                            aList.Add(objectdef)
                        End If
                    Next
                    Return aList
                End If
            ElseIf _xidShortReference.ContainsKey(xid) Then
                Dim aList As List(Of String) = _xidShortReference.Item(xid)
                For Each anEntryname In aList
                    Dim names As String() = Shuffle.NameSplitter(anEntryname)
                    If Not String.IsNullOrWhiteSpace(objectname) AndAlso names(0) = objectname Then
                        Me.GetObject(names(0)) ' load the object full
                        If _XIDDirectory.ContainsKey(xid) Then
                            Return GetEntriesByXID(xid) 'recursion by intention
                        Else
                            CoreMessageHandler(message:="xid could not be found in XIDDirectory although reference object was loaded", _
                                               argument:=xid, objectname:=objectname, _
                                               procedure:="ObjectRepository.GetEntryByXID", _
                                               messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of iormObjectEntry)
                        End If
                    Else
                        Me.GetObject(names(0)) ' load the object full
                    End If
                    ' return
                    If _XIDDirectory.ContainsKey(xid) Then
                        Return GetEntriesByXID(xid)
                    Else
                        CoreMessageHandler(message:="xid could not be found in XIDDirectory although reference object was loaded", _
                                               argument:=xid, _
                                               procedure:="ObjectRepository.GetEntryByXID", _
                                               messagetype:=otCoreMessageType.InternalError)
                        Return New List(Of iormObjectEntry)
                    End If
                Next
            Else
                Return GetEntryByAlias(alias:=[xid], objectname:=objectname)
            End If

        End Function
        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntryByAlias([alias] As String, Optional objectname As String = Nothing) As IList(Of iormObjectEntry)
            ' Me.Initialize() -> recursion since this function  is used on initializing
            [alias] = [alias].ToUpper
            If _aliasDirectory.ContainsKey([alias]) Then
                If String.IsNullOrWhiteSpace(objectname) Then
                    Return _aliasDirectory.Item(key:=[alias])
                Else
                    Dim aList As New List(Of iormObjectEntry)
                    For Each anEntry In _aliasDirectory.Item(key:=[alias])
                        If objectname.ToUpper = anEntry.Objectname.ToUpper Then
                            aList.Add(anEntry)
                        End If
                    Next
                    Return aList
                End If
            ElseIf _aliasShortReference.ContainsKey([alias]) Then
                Dim aList As List(Of String) = _aliasShortReference.Item([alias])
                For Each anEntryname In aList
                    Dim names As String() = Shuffle.NameSplitter(anEntryname)
                    If Not String.IsNullOrWhiteSpace(objectname) AndAlso names(0) = objectname Then
                        Me.GetObject(names(0)) ' load the object full
                        If _aliasDirectory.ContainsKey([alias]) Then
                            Return GetEntryByAlias([alias]) 'recursion by intention
                        Else
                            CoreMessageHandler(message:="alias could not be found in Alias Directory although reference object was loaded", _
                                               argument:=[alias], objectname:=objectname, _
                                               procedure:="ObjectRepository.GetEntryByAlias", _
                                               messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of iormObjectEntry)
                        End If
                    Else
                        Me.GetObject(names(0)) ' load the object full
                    End If
                Next
                ' return
                If _aliasDirectory.ContainsKey([alias]) Then
                    Return GetEntryByAlias([alias])
                Else
                    CoreMessageHandler(message:="alias could not be found in alias directory although reference object was loaded", _
                                           argument:=[alias], _
                                           procedure:="ObjectRepository.GetEntryByalias", _
                                           messagetype:=otCoreMessageType.InternalError)
                    Return New List(Of iormObjectEntry)
                End If
            Else
                Return New List(Of iormObjectEntry)
            End If

        End Function
        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntryByAlias([aliases]() As String, Optional objectname As String = Nothing) As List(Of iormObjectEntry)
            ' Me.Initialize() -> recursion since this function  is used on initializing
            Dim theEntries As New List(Of iormObjectEntry)

            For Each [alias] In aliases
                theEntries.AddRange(Me.GetEntryByAlias([alias], objectname:=objectname))
            Next

            Return theEntries
        End Function
    End Class

    ''' <summary>
    ''' class for Column Definition of a table
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ContainerEntryDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Column Definition of a Table Definition", _
        Version:=2, usecache:=True, isbootstrap:=True)> _
    Public Class ContainerEntryDefinition
        Inherits ormBusinessObject
        Implements iormRelationalPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ColumnDefinition"
        '** Table
        <ormTableAttribute(Version:=2, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False)> Public Const ConstPrimaryTableID = "tblTableColumnDefinitions"
        '** Index

        '*** Columns
        '*** Keys
        <ormObjectEntry(referenceobjectentry:=ContainerDefinition.ConstObjectID & "." & ContainerDefinition.ConstFNContainerID, _
                        PrimaryEntryOrdinal:=1, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNContainerID As String = ContainerDefinition.ConstFNContainerID

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, PrimaryEntryOrdinal:=2, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Column Name", Description:="column name in the table")> Public Const ConstFNContainerEntryName As String = "ColumnName"

        '** Column Specific

        <ormObjectEntry(defaultvalue:=0, Datatype:=otDataType.[Long], isnullable:=True, title:="Pos", Description:="position number in record")> _
        Public Const ConstFNPosition As String = "pos"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, properties:={ObjectEntryProperty.Trim}, _
                        title:="Description", Description:="Description of the field")> Public Const ConstFNDescription As String = "desc"

        <ormObjectEntry(Datatype:=otDataType.List, innerDatatype:=otDataType.Text, _
                        title:="Properties", Description:="database column properties")> Public Const ConstFNProperties As String = "properties"

        <ormObjectEntry(defaultvalue:=otDataType.Text, referenceobjectentry:=ObjectContainerEntry.ConstObjectID & "." & ObjectContainerEntry.ConstFNDatatype, _
                        title:="Datatype", Description:="OTDB field data type")> Public Const ConstFNDatatype As String = "datatype"

        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True,
                        title:="DefaultValue", Description:="default value of the field")> Public Const ConstFNDefaultValue As String = "default"

        <ormObjectEntry(defaultvalue:=0, Datatype:=otDataType.Long, lowerRange:=0, _
                    title:="UpdateCount", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "updc"

        <ormObjectEntry(Datatype:=otDataType.[Long], isnullable:=True, lowerRange:=0, _
                        title:="Size", Description:="max Length of the Column")> Public Const ConstFNSize As String = "size"

        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, properties:={ObjectEntryProperty.Keyword}, _
                       title:="Primary Key name", Description:="name of the primary key index")> Public Const ConstFNindexname As String = "indexname"

        <ormObjectEntry(defaultvalue:=False, Datatype:=otDataType.Bool, _
                    title:="Is primary Key", Description:="set if the entry is a primary key")> Public Const ConstFNPrimaryKey As String = "pkey"

        <ormObjectEntry(defaultvalue:=0, Datatype:=otDataType.Long, _
                    title:="Ordinal in Primary Key", Description:="Ordinal in Primary Key")> Public Const ConstFNPrimaryKeyOrdinal As String = "pkeyno"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, title:="Is Nullable", Description:="set if the entry is a nullable")> _
        Public Const ConstFNIsNullable As String = "isnull"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, title:="Is Unique", Description:="set if the entry is unique")> _
        Public Const ConstFNIsUnique As String = "ISUNIQUE"

        'avoid loops
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ' fields
        <ormObjectEntryMapping(EntryName:=ConstFNContainerID)> Private _tablename As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNContainerEntryName)> Private _ColumnName As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType = 0
        <ormObjectEntryMapping(EntryName:=ConstFNUPDC)> Private _version As Long = 0
        <ormObjectEntryMapping(EntryName:=ConstFNSize)> Private _size As Long?
        <ormObjectEntryMapping(EntryName:=ConstFNIsNullable)> Private _isNullable As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNIsUnique)> Private _isUnique As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNDefaultValue)> Private _DefaultValue As String = Nothing ' that is ok since default might be missing for strings
        <ormObjectEntryMapping(EntryName:=ConstFNDescription)> Private _Description As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNPosition)> Private _Position As Long?
        <ormObjectEntryMapping(EntryName:=ConstFNindexname)> Private _indexname As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNPrimaryKey)> Private _isPrimaryKey As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNPrimaryKeyOrdinal)> Private _PrimaryKeyOrdinal As Long = 0

        '* relation to the Tabledefinition - no cascadeOnUpdate to prevent recursion loops
        <ormRelationAttribute(linkobject:=GetType(ContainerDefinition), toPrimarykeys:={ConstFNContainerID}, createObjectIfNotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRTableDefinition = "table"
        '** the real thing
        <ormObjectEntryMapping(relationName:=constRTableDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> _
        Private _Tabledefinition As ContainerDefinition


        '** dynamic


        ''' <summary>
        ''' constructor of a SchemaDefTableEntry
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New()

        End Sub

#Region "Properties"
        ''' <summary>
        ''' return the IndexName if entry belongs to an index
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Indexname() As String
            Get
                Indexname = _indexname
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNindexname, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Public Property PrimaryKeyOrdinal() As Long
            Get
                Return Me._PrimaryKeyOrdinal
            End Get
            Set(value As Long)
                If SetValue(entryname:=ConstFNPrimaryKeyOrdinal, value:=value) Then
                    '* set also the primarykey flag which triggers of the primary key build
                    '* of the table
                    If value > 0 Then Me.IsPrimaryKey = True
                    If value <= 0 Then Me.IsPrimaryKey = False
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the position.
        ''' </summary>
        ''' <value>The position.</value>
        Public Property Position() As Long?
            Get
                Return Me._Position
            End Get
            Set(value As Long?)
                SetValue(entryname:=ConstFNPosition, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._Description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDescription, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets the default value as object representation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DefaultValue() As Object
            Get
                If _DefaultValue IsNot Nothing Then
                    Dim value = Converter.Object2otObject(_DefaultValue, Me.Datatype)
                    Return value
                ElseIf _isNullable Then
                    Return Nothing
                Else
                    ' we need a substitute for nothing 
                    Dim value = Converter.Object2otObject(_DefaultValue, Me.Datatype)
                    Return value
                End If
            End Get
            Set(value As Object)
                If value IsNot Nothing Then SetValue(entryname:=ConstFNDefaultValue, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the default value in string presentation
        ''' </summary>
        ''' <value>The default value.</value>
        Public ReadOnly Property DefaultValueString() As String
            Get
                If _DefaultValue Is Nothing And Me.IsNullable Then
                    Return String.empty
                End If
                Return Me.DefaultValue.ToString
            End Get
        End Property

        ''' <summary>
        ''' sets or gets the tablename of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Tablename() As String
            Get
                Tablename = _tablename
            End Get

        End Property


        ''' <summary>
        ''' Columnname
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String
            Get
                Return _ColumnName
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the is nullable.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Public Property IsNullable() As Boolean
            Get
                Return Me._isNullable
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsNullable, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is unique.
        ''' </summary>
        ''' <value></value>
        Public Property IsUnique() As Boolean
            Get
                Return Me._isUnique
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsUnique, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the field data type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype() As otDataType
            Get
                Datatype = _datatype
            End Get
            Set(value As otDataType)
                SetValue(entryname:=ConstFNDatatype, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version() As Long
            Get
                Return _version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the size
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Size() As Long?
            Get
                Size = _size
            End Get
            Set(value As Long?)
                SetValue(entryname:=ConstFNSize, value:=value)
            End Set
        End Property


        ''' <summary>
        ''' returns the parameter for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Properties() As String()
            Get
                Return _properties
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNProperties, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' returns true if Entry has a Primary Key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPrimaryKey() As Boolean
            Get
                IsPrimaryKey = _isPrimaryKey
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNPrimaryKey, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets the Tabledefinition object with lazy load
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableDefinition As ContainerDefinition
            Get
                If _Tabledefinition Is Nothing And _tablename <> String.empty Then
                    If Me.InfuseRelation(Me.constRTableDefinition) Then
                        Return _Tabledefinition
                    Else
                        Return Nothing
                    End If
                Else
                    Return _Tabledefinition
                End If
            End Get
        End Property
#End Region

        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off the column definition via event Handler
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' Increase the version
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IncVersion() As Long
            _version = _version + 1
            IncVersion = _version
        End Function
        ''' <summary>
        ''' set the properties of an ObjectEntryDefinition by a SchemaColumnAttribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean
            If Not Me.IsAlive(subname:="ObjectTableColumn.SetByAttribute") Then
                Return False
            End If

            If Not attribute.HasValueContainerID OrElse Not attribute.HasValueContainerEntryName Then
                CoreMessageHandler(message:="attribute has not set tablename or columnname", procedure:="objectablecolumn.setbyAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=Me.ConstObjectID)
                Return False
            End If

            With attribute
                If .HasValueDBDefaultValue Then Me.DefaultValue = .DBDefaultValue
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueIsNullable Then Me.IsNullable = .IsNullable
                If .HasValueDataType Then Me.Datatype = .Datatype
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueSize Then Me.Size = .Size
                If .HasValueParameter Then Me.Properties = Converter.otString2Array(.Parameter)
                If .hasValuePosOrdinal Then Me.Position = .Posordinal

                If .HasValuePrimaryKeyOrdinal Then
                    Me.IsPrimaryKey = True
                End If
                If .HasValueIsUnique Then Me.IsUnique = .IsUnique
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryEntryOrdinal
                If .HasValueUseForeignKey AndAlso .UseForeignKey <> otForeignKeyImplementation.None Then
                    '* normally we should check if the foreign key was transmitted to tables
                End If
            End With
        End Function

        ''' <summary>
        ''' Event Handler for defaultValues
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreateDefaultValuesNeeded

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            If e.Record.HasIndex(ConstFNDatatype) Then
                Dim adatatype As otDataType = e.Record.GetValue(ConstFNDatatype)
                If adatatype = otDataType.Text OrElse adatatype = otDataType.List Then
                    If Not e.Record.HasIndex(ConstFNSize) Then
                        e.Result = e.Result And e.Record.SetValue(ConstFNSize, ConstDBDriverMaxTextSize)
                        Exit Sub
                    Else
                        Dim aSizeValue As Object = e.Record.GetValue(ConstFNSize)
                        If Convert.ToInt64(aSizeValue) < 1 Then
                            e.Result = e.Result And e.Record.SetValue(ConstFNSize, ConstDBDriverMaxTextSize)
                            Exit Sub
                        End If
                    End If
                End If
            End If

        End Sub

        ''' <summary>
        ''' Event Handler for Validating - correct
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnValidating(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnValidating

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            Dim anObject = TryCast(e.DataObject, ContainerEntryDefinition)
            If anObject IsNot Nothing Then
                If anObject.Datatype = otDataType.Text Or anObject.Datatype = otDataType.List Then
                    If Not anObject.Size.HasValue OrElse (anObject.Size.HasValue AndAlso anObject.Size < 1) Then
                        anObject.Size = ConstDBDriverMaxTextSize
                    End If
                End If
            End If

        End Sub
        ''' <summary>
        ''' Event Handler for Infused
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            Dim anObject = TryCast(e.DataObject, ContainerEntryDefinition)
            If anObject IsNot Nothing Then
                If anObject.Datatype = otDataType.Text Or anObject.Datatype = otDataType.List Then
                    If Not anObject.Size.HasValue OrElse (anObject.Size.HasValue AndAlso anObject.Size < 1) Then
                        anObject.Size = ConstDBDriverMaxTextSize
                    End If
                End If
            End If

        End Sub

        ''' <summary>
        ''' Event Handler for Feeding 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnFeeding(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnFeeding

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            Dim anObject = TryCast(e.DataObject, ContainerEntryDefinition)
            If anObject IsNot Nothing Then
                If Not anObject.Datatype = otDataType.Text AndAlso Not anObject.Datatype = otDataType.List Then
                    If anObject.Size.HasValue Then
                        anObject.Size = Nothing
                        e.Result = True
                    End If
                End If
            End If

        End Sub
        ''' <summary>
        ''' retrives a ColumnDef Object
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="forcereload"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal tablename As String, ByVal columnname As String, Optional forcereload As Boolean = False, Optional runtimeOnly As Boolean = False) As ContainerEntryDefinition
            Return RetrieveDataObject(Of ContainerEntryDefinition)(pkArray:={tablename.ToUpper, columnname.ToUpper}, forceReload:=forcereload, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Dim myself = TryCast(e.DataObject, ContainerEntryDefinition)
            If myself IsNot Nothing Then myself.DomainID = ConstGlobalDomain
        End Sub
        ''' <summary>
        ''' create a new dataobject with primary keys
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <param name="typeid"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal tablename As String, ByVal columnname As String, _
                                        Optional ByVal runtimeOnly As Boolean = False, _
                                        Optional ByVal checkunique As Boolean = True) As ContainerEntryDefinition
            Dim primarykey() As Object = {tablename.ToUpper, columnname.ToUpper}

            ' create
            Return ormBusinessObject.CreateDataObject(Of ContainerEntryDefinition)(pkArray:=primarykey, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


    End Class

    ''' <summary>
    ''' class for foreign key definition of multiple table columns
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ForeignKeyDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Foreign Key Definition of a Table", _
        Version:=1, usecache:=True, isbootstrap:=True)> _
    Public Class ForeignKeyDefinition
        Inherits ormBusinessObject
        Implements iormRelationalPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ForeignKeyDefinition"
        '** Table
        <ormTableAttribute(Version:=1, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False)> Public Const ConstPrimaryTableID = "TBLTABLEFOREIGNKEYS"
        '** Index

        '*** Columns
        '*** Keys
        <ormObjectEntry(referenceobjectentry:=ContainerDefinition.ConstObjectID & "." & ContainerDefinition.ConstFNContainerID, _
                        PrimaryEntryOrdinal:=1, useforeignKey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNTableName As String = ContainerDefinition.ConstFNContainerID

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, PrimaryEntryOrdinal:=2, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Foreign Key Name", Description:="name of the foreign key in the table")> Public Const ConstFNID As String = "ID"

        '** Fields
        <ormObjectEntry(Datatype:=otDataType.List, title:="Columns", _
            Description:="table column references")> Public Const ConstFNColumns As String = "COLUMNS"

        <ormObjectEntry(Datatype:=otDataType.Long, defaultvalue:=otForeignKeyImplementation.None, _
            title:="Use Foreign Key", _
            Description:="describes the implementation layer of foreign key or if 0 then foreign key is not used")> _
        Public Const ConstFNUseForeignKey As String = "USEFOREIGNKEY"

        <ormObjectEntry(Datatype:=otDataType.List, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
            title:="Foreign Key References", Description:="foreign key table columns references")> Public Const ConstFNForeignKeys As String = "FOREIGNKEYS"

        <ormObjectEntry(Datatype:=otDataType.List, _
            title:="Foreign Key Properties", Description:="Foreign Key Properties")> Public Const ConstFNForeignKeyProperties As String = "FOREIGNKEYPROP"

        <ormObjectEntry(Datatype:=otDataType.Memo, properties:={ObjectEntryProperty.Trim}, isnullable:=True, _
                       title:="Description", Description:="Description of the foreign key")> Public Const ConstFNDescription As String = "DESCRIPTION"
        <ormObjectEntry(defaultvalue:=1, Datatype:=otDataType.[Long], lowerrange:=1, _
                        title:="UpdateCount", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "UPDC"

        'avoid loops
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID
        ' fields
        <ormObjectEntryMapping(EntryName:=ConstFNTableName)> Private _tablename As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNID)> Private _id As String = String.empty

        <ormObjectEntryMapping(EntryName:=ConstFNUPDC)> Protected _version As Long = 0
        <ormObjectEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = String.empty


        <ormObjectEntryMapping(EntryName:=ConstFNUseForeignKey)> Private _UseForeignkey As otForeignKeyImplementation = otForeignKeyImplementation.None
        <ormObjectEntryMapping(EntryName:=ConstFNColumns)> Private _columnnames As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNForeignKeys)> Private _foreignKeys As String() = {}

        <ormObjectEntryMapping(EntryName:=ConstFNForeignKeyProperties)> Private _foreignkeyPropStrings As String() = {}

        '* relation to the Tabledefinition - no cascadeOnUpdate to prevent recursion loops
        <ormRelationAttribute(linkobject:=GetType(ContainerDefinition), toPrimarykeys:={ConstFNTableName}, createObjectIfnotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRTableDefinition = "table"
        '** the real thing
        <ormObjectEntryMapping(relationName:=constRTableDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> _
        Private _Tabledefinition As ContainerDefinition


        '** dynamic
        Private _foreignkeyproperties As New List(Of ForeignKeyProperty)



#Region "Properties"

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDescription, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the tablename of the foreign key (source)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Tablename() As String
            Get
                Tablename = _tablename
            End Get

        End Property
        ''' <summary>
        ''' sets or gets the id of the foreign key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Id() As String
            Get
                Return _id
            End Get
        End Property
        ''' <summary>
        ''' returns version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version() As Long
            Get
                Return _version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is foreign key implementation.
        ''' </summary>
        ''' <value>T</value>
        Public Property UseForeignKey() As otForeignKeyImplementation
            Get
                Return Me._UseForeignkey
            End Get
            Set(value As otForeignKeyImplementation)
                SetValue(entryname:=ConstFNUseForeignKey, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns a list of tablenames which are referenced in the foreign key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ForeignKeyReferenceTables As IList(Of String)
            Get
                Dim aList As New List(Of String)

                For Each aReference In Me.ForeignKeyReferences
                    Dim names As String() = Shuffle.NameSplitter(aReference)
                    If Not aList.Contains(names(0)) Then aList.Add(names(0))
                Next
                Return aList
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is foreign Key reference string.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Public Property ForeignKeyReferences() As String()
            Get
                Return Me._foreignKeys
            End Get
            Set(value As String())
                Dim okflag As Boolean = True

                For Each reference In value
                    Dim refTableName As String = String.empty
                    Dim refColumnname As String = String.empty
                    Dim names = Shuffle.NameSplitter(reference)
                    If names.Count > 1 Then
                        refTableName = names(0)
                        refColumnname = names(1)
                    Else
                        refColumnname = names(0)
                        CoreMessageHandler(message:="an tablename is missing in columnnames reference", argument:=reference, _
                                           procedure:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)

                    End If

                    ' will not take 
                    Dim anReferenceAttribute As ormContainerEntryAttribute = _
                        ot.GetSchemaTableColumnAttribute(columnname:=refColumnname, tableid:=refTableName)
                    If anReferenceAttribute IsNot Nothing Then
                        okflag = okflag And True
                    Else
                        CoreMessageHandler(message:="an table column attribute could not be found in columnnames reference - columnnames not set not set", _
                                           argument:=reference, containerID:=refTableName, containerEntryName:=refColumnname, _
                                           procedure:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)
                        okflag = okflag And False
                    End If
                Next

                If okflag Then SetValue(entryname:=ConstFNForeignKeys, value:=value)

            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is entry names Key reference string.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Public Property ColumnNames() As String()
            Get
                Return Me._columnnames
            End Get
            Set(value As String())
                Dim okflag = True

                For Each reference In value
                    Dim refTableID As String = String.Empty
                    Dim refColumnname As String = String.Empty
                    Dim names = Shuffle.NameSplitter(reference)
                    If names.Count > 1 Then
                        refTableID = names(0)
                        refColumnname = names(1)
                    Else
                        refColumnname = names(0)
                        CoreMessageHandler(message:="an tablename is missing in columnnames reference", argument:=reference, _
                                           procedure:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)

                    End If

                    ' will not take 
                    Dim anReferenceAttribute As ormContainerEntryAttribute = _
                        ot.GetSchemaTableColumnAttribute(columnname:=refColumnname, tableid:=refTableID)

                    If anReferenceAttribute IsNot Nothing Then
                        okflag = okflag And True
                    Else
                        CoreMessageHandler(message:="an table column attribute could not be found in columnnames reference - columnnames not set not set", _
                                           argument:=reference, containerID:=refTableID, containerEntryName:=refColumnname, _
                                           procedure:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)
                        okflag = okflag And False
                    End If
                Next
                If okflag Then SetValue(entryname:=ConstFNColumns, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the parameter for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeignKeyProperties() As String()
            Get
                Return _foreignkeyPropStrings
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNForeignKeyProperties, value:=value)
                If _foreignkeyproperties IsNot Nothing Then
                    _foreignkeyproperties.Clear()
                Else
                    _foreignkeyproperties = New List(Of ForeignKeyProperty)
                End If

                For Each aP In value
                    If Database.ForeignKeyProperty.Validate(Of ForeignKeyProperty)(aP) Then
                        _foreignkeyproperties.Add(New ForeignKeyProperty(aP))
                    End If
                Next
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Properties for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeignKeyProperty As List(Of ForeignKeyProperty)
            Get
                If _foreignkeyPropStrings.Count <> _foreignkeyproperties.Count Then
                    _foreignkeyproperties.Clear()
                    For Each aps In _foreignkeyPropStrings
                        _foreignkeyproperties.Add(New ForeignKeyProperty(aps))
                    Next
                End If
                Return _foreignkeyproperties
            End Get
            Set(value As List(Of ForeignKeyProperty))
                Dim aPropertyString As New List(Of String)
                For Each aP In value
                    aPropertyString.Add(aP.ToString)
                Next
                If SetValue(entryname:=ConstFNForeignKeyProperties, value:=aPropertyString.ToArray) Then
                    _foreignkeyproperties = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' gets the Tabledefinition object with lazy load
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableDefinition As ContainerDefinition
            Get
                If _Tabledefinition Is Nothing And _tablename <> String.empty Then
                    If Me.InfuseRelation(Me.constRTableDefinition) Then
                        Return _Tabledefinition
                    Else
                        Return Nothing
                    End If
                Else
                    Return _Tabledefinition
                End If
            End Get
        End Property
#End Region

        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off the column definition via event Handler
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' Increase the version
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>toupper.split
        Public Function IncVersion() As Long
            _version = _version + 1
            IncVersion = _version
        End Function
        ''' <summary>
        ''' gets a list of columnnames out of objectentry names
        ''' </summary>
        ''' <value></value>
        Public Function RetrieveColumnnames(ObjectEntrynames As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim aList As New List(Of String)

            For Each reference In ObjectEntrynames
                Dim refObjectName As String = String.empty
                Dim refObjectEntry As String = String.empty
                Dim names = Shuffle.NameSplitter(reference)
                If names.Count > 1 Then
                    refObjectName = names(0)
                    refObjectEntry = names(1)
                Else
                    refObjectEntry = names(0)

                    CoreMessageHandler(message:="an object name is missing in foreign key reference", argument:=reference, procedure:="ForeignkeyDefinition.ForeignKeyReference", messagetype:=otCoreMessageType.InternalError)
                    Return aList
                End If

                ' will not take 
                Dim anReferenceAttribute As ormObjectEntryAttribute = _
                    ot.GetObjectEntryAttribute(entryname:=refObjectEntry, objectname:=refObjectName)

                If anReferenceAttribute IsNot Nothing Then
                    aList.Add(anReferenceAttribute.ContainerID & "." & anReferenceAttribute.ContainerEntryName)
                Else
                    CoreMessageHandler(message:="an object entry attribute could not be found in foreign key reference - foreign key reference not set", _
                                       argument:=reference, objectname:=refObjectName, entryname:=refObjectName, _
                                       procedure:="ForeignkeyDefinition.RetrieveColumnnames", messagetype:=otCoreMessageType.InternalError)

                End If
            Next

            Return aList

        End Function


        ''' <summary>
        ''' set the properties of an ObjectEntryDefinition by a SchemaColumnAttribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByAttribute(attribute As ormForeignKeyAttribute) As Boolean
            If Not Me.IsAlive(subname:="ForeignKeyDefinition.SetByAttribute") Then
                Return False
            End If

            If Not attribute.HasValueTableID Then
                CoreMessageHandler(message:="attribute has not set table name ", procedure:="ForeignKeyDefinition.setbyAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=Me.ConstObjectID)
                Return False
            End If

            With attribute
                'If .HasValueID Then Me.Id = .name
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueEntrynames Then Me.ColumnNames = RetrieveColumnnames(.Entrynames).ToArray
                If .HasValueUseForeignKey AndAlso .UseForeignKey <> otForeignKeyImplementation.None Then
                    Me.UseForeignKey = .UseForeignKey
                    If .HasValueForeignKeyReferences Then
                        Me.ForeignKeyReferences = RetrieveColumnnames(.ForeignKeyReferences).ToArray
                        If Me.ForeignKeyReferences.Count = 0 Then
                            CoreMessageHandler(message:="no valid foreign key references found in attribute - foreign key implementation set to none", _
                                           argument:=attribute.ID, containerID:=Me.Tablename, _
                                            procedure:="ColumnDefinition.SetByAttribute", messagetype:=otCoreMessageType.InternalError)
                            Me.UseForeignKey = otForeignKeyImplementation.None
                        End If
                    Else
                        CoreMessageHandler(message:="no foreign key references found in attribute - foreign key implementation set to none", _
                                           argument:=attribute.ID, containerID:=Me.Tablename, _
                                            procedure:="ColumnDefinition.SetByAttribute", messagetype:=otCoreMessageType.InternalError)
                        Me.UseForeignKey = otForeignKeyImplementation.None
                    End If

                    If .HasValueForeignKeyProperties Then
                        Me.ForeignKeyProperties = .ForeignKeyProperties
                    Else
                        ''' set default properties
                        ''' OnUpdate to Cascade, OnDelete to Cascade
                        Me.ForeignKeyProperties = {Database.ForeignKeyProperty.OnUpdate & "(" & OnTrack.Database.ForeignKeyActionProperty.Cascade & ")", _
                                                    OnTrack.Database.ForeignKeyProperty.OnDelete & "(" & OnTrack.Database.ForeignKeyActionProperty.Cascade & ")"
                                                   }
                    End If


                End If
            End With
            Return True
        End Function

        ''' <summary>
        ''' retrives a foreign key Object
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="forcereload"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal tablename As String, ByVal id As String, Optional forcereload As Boolean = False, Optional runtimeOnly As Boolean = False) As ForeignKeyDefinition
            Return RetrieveDataObject(Of ForeignKeyDefinition)(pkArray:={tablename.ToUpper, id.ToUpper}, forceReload:=forcereload, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Dim myself = TryCast(e.DataObject, ForeignKeyDefinition)
            If myself IsNot Nothing Then myself.DomainID = ConstGlobalDomain
        End Sub
        ''' <summary>
        ''' create a new dataobject with primary keys
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <param name="typeid"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Create(ByVal tablename As String, ByVal id As String, _
                                        Optional ByVal runtimeOnly As Boolean = False, _
                                        Optional ByVal checkunique As Boolean = True) As ForeignKeyDefinition
            Dim primarykey() As Object = {tablename.ToUpper, id.ToUpper}

            ' create
            Return ormBusinessObject.CreateDataObject(Of ForeignKeyDefinition)(pkArray:=primarykey, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


    End Class

    ''' <summary>
    ''' definition class Table defintion for an OTDB data object definition
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=IndexDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="index definition for table definitions", _
        isbootstrap:=True, usecache:=True, Version:=1)> _
    Public Class IndexDefinition
        Inherits ormBusinessObject
        Implements iormInfusable
        Implements iormRelationalPersistable

        Public Const ConstObjectID = "IndexDefinition"

        '** Table Definition
        <ormTableAttribute(version:=1, usecache:=True)> Public Const ConstPrimaryTableID = "tblTableIndexDefinitions"

        '** Indices

        '** Primary key
        <ormObjectEntry(referenceobjectentry:=ContainerDefinition.ConstObjectID & "." & ContainerDefinition.ConstFNContainerID, PrimaryEntryOrdinal:=1, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNTablename = ContainerDefinition.ConstFNContainerID

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, PrimaryEntryOrdinal:=2,
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Index Name", description:="index name for the table")> Public Const ConstFNIndexName = "index"
        '** Fields
        <ormObjectEntry(Datatype:=otDataType.List, properties:={ObjectEntryProperty.Keyword}, _
                         title:="Columns", description:="column names of the index in order")> Public Const ConstFNColumns = "columns"

        <ormObjectEntry(defaultvalue:=False, dbdefaultvalue:="0", Datatype:=otDataType.Bool, _
                        title:="IsPrimaryKey", Description:="set if the index is the primary key")> Public Const ConstFNIsPrimary As String = "isprimary"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, _
                         title:="Index Description", description:="description of the index")> Public Const ConstFNdesc = "desc"

        <ormObjectEntry(defaultvalue:="1", Datatype:=otDataType.[Long], lowerRange:=0, _
                                  title:="Version", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "updc"

        <ormObjectEntry(Datatype:=otDataType.List, _
                         title:="Properties", description:="properties of the index")> Public Const ConstFNProperties = "properties"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, defaultvalue:="", properties:={ObjectEntryProperty.Keyword}, _
                         title:="Database Id", description:="id of the index in the database")> Public Const ConstFNNativeIndexName = "dbid"

        <ormObjectEntry(defaultvalue:=False, dbdefaultvalue:="0", Datatype:=otDataType.Bool, _
                                  title:="IsUnique", Description:="set if the index is unique")> Public Const ConstFNIsUnique As String = "ISUNIQUE"
        'avoid loops
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID
        '** MAPPINGS
        <ormObjectEntryMapping(entryname:=ConstFNIndexName)> Private _indexname As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNTablename)> Private _tablename As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNColumns)> Private _columnnames As String() = {}
        <ormObjectEntryMapping(entryname:=ConstFNdesc)> Private _description As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNIsPrimary)> Private _isPrimary As Boolean = False
        <ormObjectEntryMapping(entryname:=ConstFNIsUnique)> Private _isUnique As Boolean = False
        <ormObjectEntryMapping(entryname:=ConstFNUPDC)> Private _Version As Long = 0
        <ormObjectEntryMapping(entryname:=ConstFNProperties)> Private _properties As String() = {}
        <ormObjectEntryMapping(entryname:=ConstFNNativeIndexName)> Private _nativeIndexname As String = String.empty
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(objectID:=ConstObjectID)
        End Sub

#Region "Properties"



        ''' <summary>
        ''' Gets or sets the properties.
        ''' </summary>
        ''' <value>The properties.</value>
        Public Property Properties() As String()
            Get
                Return Me._properties
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNProperties, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property Version() As Long
            Get
                Return Me._Version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the is primary.
        ''' </summary>
        ''' <value>The is primary.</value>
        Public Property IsPrimary() As Boolean
            Get
                Return Me._isPrimary
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsPrimary, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is primary.
        ''' </summary>
        ''' <value>The is primary.</value>
        Public Property IsUnique() As Boolean
            Get
                Return Me._isUnique
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsUnique, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNdesc, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the native name.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property NativeIndexname() As String
            Get
                Return Me._nativeIndexname
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNNativeIndexName, value:=value)
            End Set

        End Property
        ''' <summary>
        ''' Gets or sets the columnnames.
        ''' </summary>
        ''' <value>The columnnames.</value>
        Public Property Columnnames() As String()
            Get
                Return Me._columnnames
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNColumns, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the tablename.
        ''' </summary>
        ''' <value>The tablename.</value>
        Public ReadOnly Property Tablename() As String
            Get
                Return Me._tablename
            End Get
        End Property

        ''' <summary>
        ''' Gets the indexname.
        ''' </summary>
        ''' <value>The indexname.</value>
        Public ReadOnly Property Name() As String
            Get
                Return Me._indexname
            End Get
        End Property
#End Region

        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Dim myself = TryCast(e.DataObject, IndexDefinition)
            If myself IsNot Nothing Then myself.DomainID = ConstGlobalDomain
        End Sub
        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off 
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' returns a index definition
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal tablename As String, ByVal indexname As String, Optional runtimeOnly As Boolean = False) As IndexDefinition
            Return ormBusinessObject.RetrieveDataObject(Of IndexDefinition)({tablename.ToUpper, indexname.ToUpper}, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormBusinessObject.CreateDataObjectSchema(Of IndexDefinition)(silent:=silent)
        End Function
        ''' <summary>
        ''' create a new data object of that type
        ''' </summary>
        ''' <param name="tablename">tablename of the table</param>
        ''' <param name="runTimeOnly">if no save is possible -> bootstrapping</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal tablename As String, ByVal indexname As String, _
                                        Optional runTimeOnly As Boolean = False, _
                                        Optional checkunique As Boolean = True) As IndexDefinition
            Return ormBusinessObject.CreateDataObject(Of IndexDefinition)({tablename.ToUpper, indexname.ToUpper}, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
        End Function

        ''' <summary>
        ''' Event Handler on Persisting
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub OnPersisting() Handles MyBase.OnPersisting
            If nativeIndexname = String.empty Then Me.nativeIndexname = Me.Name
        End Sub
    End Class

    ''' <summary>
    ''' definition class Table defintion for an OTDB data object definition
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=ContainerDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Relational table definition of a database table", _
        usecache:=True, isbootstrap:=True, Version:=1)> _
    Public Class ContainerDefinition
        Inherits ormBusinessObject
        Implements iormInfusable
        Implements iormRelationalPersistable

        Public Const ConstObjectID = "ContainerDefinition"

        '** Table Definition
        <ormTable(version:=1, usecache:=True)> Public Const ConstPrimaryTableID = "tblContainerDefinitions"

        '** Indices

        '** Primary key
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, PrimaryEntryOrdinal:=1, properties:={ObjectEntryProperty.Keyword}, _
                         title:="Container", description:="OnTrack container id for the object")> Public Const ConstFNContainerID = "ID"

        '** Fields

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, defaultvalue:=ot.ConstDefaultPrimaryDBDriver, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Database Driver", description:="Database Driver")> Public Const ConstFNDatabaseDriver = "DBDRIVER"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, defaultvalue:=ot.ConstDefaultContainerType, properties:={ObjectEntryProperty.Keyword}, _
                       title:=" Type", description:="Container Type")> Public Const ConstFNContainerType = "CONTAINER TYPE"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, defaultvalue:="PrimaryKey", properties:={ObjectEntryProperty.Keyword}, _
                         title:="PrimaryKey", description:="primary key name for the table")> Public Const ConstFNPrimaryKey = "primarykey"

        <ormObjectEntry(Datatype:=otDataType.Memo, _
                         title:="Container Description", description:="description of the table")> Public Const ConstFNdesc = "desc"

        <ormObjectEntry(Datatype:=otDataType.[Long], defaultvalue:=1, lowerRange:=0, _
                                  title:="Version", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "updc"

        <ormObjectEntry(Datatype:=otDataType.List, size:=255, _
                                  title:="Properties", Description:="properties on table level")> Public Const ConstFNProperties As String = "properties"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
                        title:="Use Cache", Description:="set if the entry is object cached")> Public Const ConstFNUseCache As String = "usecache"

        <ormObjectEntry(Datatype:=otDataType.List, size:=255, _
                        title:="Cache", defaultvalue:="", Description:="cache properties on table level")> Public Const ConstFNCacheProperties As String = "cacheproperties"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="DeleteFlagBehaviour", Description:="set if the object runs the delete per flag behavior")> Public Const ConstFNDeletePerFlag As String = "DeletePerFlag"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="SpareFieldsBehaviour", Description:="set if the object has additional spare fields behavior")> Public Const ConstFNSpareFieldsFlag As String = "SpareFields"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="DomainBehaviour", Description:="set if the object belongs to a domain")> Public Const ConstFNDomainFlag As String = "DomainBehavior"

        'avoid loops nonsense to have that here but it is inherited
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** relations
        <ormRelationAttribute(linkobject:=GetType(ContainerEntryDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNContainerID}, toEntries:={ContainerEntryDefinition.ConstFNContainerID})> Public Const ConstRColumns = "columns"
        <ormRelationAttribute(linkobject:=GetType(IndexDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNContainerID}, toEntries:={ContainerEntryDefinition.ConstFNContainerID})> Public Const ConstRIndices = "indices"
        <ormRelationAttribute(linkobject:=GetType(ForeignKeyDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNContainerID}, toEntries:={ForeignKeyDefinition.ConstFNTableName})> Public Const ConstRForeignKeys = "foreignkeys"

        '*** Mapping
        <ormObjectEntryMapping(EntryName:=ConstFNContainerID)> Private _tablename As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNDatabaseDriver)> Private _DatabaseDriverID As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNContainerType)> Private _containertype As otContainerType = otContainerType.Table

        <ormObjectEntryMapping(EntryName:=ConstFNdesc)> Private _description As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNPrimaryKey)> Private _pkname As String = "PrimaryKey"   ' name of Primary Key

        <ormObjectEntryMapping(EntryName:=ConstFNUseCache)> Private _useCache As Boolean
        <ormObjectEntryMapping(EntryName:=ConstFNCacheProperties)> Private _CacheProperties As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNDeletePerFlag)> Private _deletePerFlagBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNDomainFlag)> Private _domainBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNSpareFieldsFlag)> Private _SpareFieldsFlagBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNUPDC)> Private _Version As Long = 0

        '* relation mappings
        <ormObjectEntryMapping(RelationName:=ConstRColumns, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
         keyentries:={ContainerEntryDefinition.ConstFNContainerEntryName})> Private _columns As New Dictionary(Of String, ContainerEntryDefinition)

        <ormObjectEntryMapping(RelationName:=ConstRIndices, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
         keyentries:={IndexDefinition.ConstFNIndexName})> Private _indices As New Dictionary(Of String, IndexDefinition)

        <ormObjectEntryMapping(RelationName:=ConstRForeignKeys, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
        keyentries:={ForeignKeyDefinition.ConstFNID})> Private _foreignkeys As New Dictionary(Of String, ForeignKeyDefinition)

        '** runtime
        Public Event ObjectDefinitionChanged As EventHandler(Of ObjectDefinition.EventArgs)

        '** runtime
        Private _lock As New Object

        '** initialize
        Public Sub New()
            Call MyBase.New(objectID:=ConstObjectID)

        End Sub
#Region "Properties"

        ''' <summary>
        ''' Gets the tablename.
        ''' </summary>
        ''' <value>The tablename.</value>
        Public ReadOnly Property ID() As String
            Get
                Return Me._tablename
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the container type
        ''' </summary>
        ''' <value>The pkname.</value>
        Public Property ContainerType() As otContainerType
            Get
                Return Me._containertype
            End Get
            Set(value As otContainerType)
                SetValue(entryname:=ConstFNContainerType, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the primary key name.
        ''' </summary>
        ''' <value>The pkname.</value>
        Public Property PrimaryDatabaseDriverID() As String
            Get
                Return Me._DatabaseDriverID
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDatabaseDriver, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the primary key name.
        ''' </summary>
        ''' <value>The pkname.</value>
        Public Property PrimaryKey() As String
            Get
                Return Me._pkname
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNPrimaryKey, value:=value)
            End Set
        End Property


        ''' <summary>
        ''' Gets or sets the Description.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNdesc, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the cache selection string
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Properties As String()
            Set(value As String())
                SetValue(entryname:=ConstFNProperties, value:=value)
            End Set
            Get
                Return _properties.ToArray
            End Get
        End Property
        ''' <summary>
        ''' use Cache on this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property UseCache As Boolean
            Set(value As Boolean)
                SetValue(entryname:=ConstFNUseCache, value:=value)
            End Set
            Get
                Return _useCache
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the cache selection string
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property CacheProperties As List(Of String)
            Set(value As List(Of String))
                SetValue(entryname:=ConstFNCacheProperties, value:=value.ToArray)
            End Set
            Get
                Return _CacheProperties.ToList
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain behavior.
        ''' </summary>
        ''' <value>The domain behavior.</value>
        Public Property DomainBehavior() As Boolean
            Get
                Return Me._domainBehavior
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNDomainFlag, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Version As Long
            Get
                Return _Version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the the spare fields behavior. Means extra fields are available.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property SpareFieldsBehavior
            Set(value)
                SetValue(entryname:=ConstFNSpareFieldsFlag, value:=value)
            End Set
            Get
                Return _SpareFieldsFlagBehavior
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the delete per flag behavior. If true a deleteflag and a delete date are available.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property DeletePerFlagBehavior As Boolean
            Set(value As Boolean)
                SetValue(entryname:=ConstFNDeletePerFlag, value:=value)
            End Set
            Get
                Return _deletePerFlagBehavior
            End Get
        End Property
        ''' <summary>
        ''' returns a List of Column Definitions
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Columns As IList(Of ContainerEntryDefinition)
            Get
                Return _columns.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' returns a List of foreign keys
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ForeignKeys As IList(Of ForeignKeyDefinition)
            Get
                Return _foreignkeys.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' returns a List of indices
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Indices As IList(Of IndexDefinition)
            Get
                Return _indices.Values.ToList
            End Get
        End Property
#End Region

        ''' <summary>
        ''' returns a List of all Tabledefinitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of ContainerDefinition)
            Return ormBusinessObject.AllDataObject(Of ContainerDefinition)()
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Dim myself = TryCast(e.DataObject, ContainerDefinition)
            If myself IsNot Nothing Then myself.DomainID = ConstGlobalDomain
        End Sub
        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off the column definition via event Handler
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' adds a table entry by an table attribute 
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetValuesBy(attribute As iormContainerAttribute) As Boolean
            If Not Me.IsAlive(subname:="ContainerDefinition.SetValuesBy") Then Return False

            '** set the values of the table definition
            With attribute
                If .HasValueDatabaseDriverID Then
                    Me.PrimaryDatabaseDriverID = .DatabaseDriverID
                ElseIf attribute.GetType Is GetType(ormTableAttribute) Then
                    Me.PrimaryDatabaseDriverID = ConstDefaultPrimaryDBDriver
                End If

                If .hasValueContainerType Then
                    Me.ContainerType = .containertype
                Else
                    Me.ContainerType = otContainerType.Table
                End If

                If .HasValueAddDomainBehavior Then Me.DomainBehavior = .AddDomainBehavior
                If .HasValueDeleteFieldBehavior Then Me.DeletePerFlagBehavior = .AddDeleteFieldBehavior
                If .HasValueSpareFields Then Me.SpareFieldsBehavior = .AddSpareFields
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueDescription Then Me.Description = .Description
                If .HasValuePrimaryKey Then Me.PrimaryKey = .PrimaryKey
                If .HasValueUseCache Then Me.UseCache = .UseCache
                If .HasValueCacheProperties Then Me.CacheProperties = .CacheProperties.ToList

                '** Add the Foreign Key Attributes
                For Each aForeignKeyAttribute In .ForeignkeyAttributes

                    '** create or retrieve the foreign key data object
                    Dim aForeignkey As ForeignKeyDefinition = ForeignKeyDefinition.Create(tablename:=Me.ID, id:=aForeignKeyAttribute.ID, checkunique:=True, runtimeOnly:=Me.RunTimeOnly)
                    If aForeignkey Is Nothing Then
                        aForeignkey = ForeignKeyDefinition.Retrieve(tablename:=Me.ID, id:=aForeignKeyAttribute.ID, runtimeOnly:=Me.RunTimeOnly)
                    End If

                    '** set the foreign key data object by the attribute and add it
                    If aForeignkey.SetByAttribute(aForeignKeyAttribute) Then
                        Me.AddForeignKey(entry:=aForeignkey)
                    End If
                Next
            End With

            Return True
        End Function

        ''' <summary>
        ''' Event Handler if a Columndefinition is changing
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub OnEntryChanged(sender As Object, e As PropertyChangedEventArgs)
            Dim entry = TryCast(sender, ContainerEntryDefinition)
            If entry IsNot Nothing Then
                'rebuild the primary key if necessary
                'do not take PrimaryKeyOrdial since this might be changed during rebuild
                If e.PropertyName = ContainerEntryDefinition.ConstFNPrimaryKey Then
                    RebuildPrimaryKey()
                End If

            End If
        End Sub

        ''' <summary>
        ''' Add a columnDefinition
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddColumn(entry As ContainerEntryDefinition) As Boolean

            If Not Me.IsAlive(subname:="AddColumn") Then Return False

            ' remove and overwrite
            If _columns.ContainsKey(key:=entry.Name.ToUpper) Then
                Call _columns.Remove(key:=entry.Name.ToUpper)
            End If
            ' add entry
            _columns.Add(key:=entry.Name.ToUpper, value:=entry)


            '** add Handler
            AddHandler entry.PropertyChanged, AddressOf Me.OnEntryChanged
            AddHandler MyBase.OnSwitchRuntimeOff, AddressOf entry.OnSwitchRuntimeOff

            'rebuild the primary key if necessary
            If entry.IsPrimaryKey Then
                RebuildPrimaryKey()
            End If

            '** return
            Return True
        End Function

        ''' <summary>
        ''' Add a columnDefinition
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddForeignKey(entry As ForeignKeyDefinition) As Boolean

            If Not Me.IsAlive(subname:="AddForeignKey") Then Return False

            ' remove and overwrite
            If _foreignkeys.ContainsKey(key:=entry.Id.ToUpper) Then
                Call _foreignkeys.Remove(key:=entry.Id.ToUpper)
            End If
            ' add entry
            _foreignkeys.Add(key:=entry.Id.ToUpper, value:=entry)


            '** add Handler
            AddHandler entry.PropertyChanged, AddressOf Me.OnEntryChanged
            AddHandler MyBase.OnSwitchRuntimeOff, AddressOf entry.OnSwitchRuntimeOff

            '** return
            Return True
        End Function

        ''' <summary>
        ''' alterSchema foreign relations changes the Database foreign keys according the information here
        ''' this should only be run after all table are created by alter schema
        ''' </summary>
        ''' <param name="addToSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AlterSchemaForeignRelations() As Boolean

            Dim tblInfo As Object
            Dim result As Boolean = True
            Dim aCollection As New List(Of String)

            If Not IsAlive(subname:="TableDefinition.AlterSchemaForeignRelations") Then Return False

            Try
                '** call to get object
                tblInfo = CurrentDBDriver.GetContainerObject(Me.ID, createOrAlter:=False)
                If tblInfo Is Nothing Then
                    CoreMessageHandler(message:="table is not created in the database yet - run alter schema first before to AlterSchemaForeignRelations", _
                                        procedure:="TableDefinition.AlterSchemaForeignKey", messagetype:=otCoreMessageType.InternalError, _
                                        containerID:=Me.ID)
                    Return False
                End If

                ' create or alter foreign keys on the column level of each entry
                For Each anEntry In _foreignkeys.Values
                    If anEntry.UseForeignKey = otForeignKeyImplementation.NativeDatabase Then
                        Dim fklist = CurrentDBDriver.GetForeignKeys(tblInfo, anEntry, createOrAlter:=True)
                        If fklist Is Nothing OrElse fklist.Count = 0 Then
                            result = result And False
                        End If
                    End If

                Next


                '    ' reset the Table description
                '    ' only if we are connected -> bootstrapping problem
                If result AndAlso CurrentSession.IsRunning Then
                    If ot.CurrentConnection.DatabaseDriver.RetrieveContainerSchema(containerID:=Me.ID, force:=True) Is Nothing Then
                        Call CoreMessageHandler(procedure:="TableDefinition.AlterSchemaForeignKey", containerID:=tblInfo.Name, _
                                                message:="Error while setTable in alterSchema")
                    End If
                End If

                Return result
            Catch ex As Exception
                Call CoreMessageHandler(procedure:="TableDefinition.AlterSchemaForeignKey", exception:=ex)
                Return False
            End Try

        End Function

        ''' <summary>
        '''  changes the Database according the information here
        ''' </summary>
        ''' <param name="addToSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AlterTableSchema(Optional databasedriver As iormRelationalDatabaseDriver = Nothing) As Boolean

            Dim tblInfo As Object

            If Not IsAlive(subname:="TableDefinition.alterschema") Then Return False

            ''' set the database driver
            If databasedriver Is Nothing Then
                If CurrentDBDriver.GetType.GetInterfaces.Contains(GetType(iormRelationalDatabaseDriver)) Then
                    databasedriver = CType(ot.CurrentDBDriver, iormRelationalDatabaseDriver)
                Else
                    Call CoreMessageHandler(procedure:="TableDefinition.alterSchema", containerID:=tblInfo.Name, _
                                               message:="data base driver is not a relational driver - table canot be altered")
                    Return False
                End If
            End If

            Try
                '** call to get object
                tblInfo = databasedriver.GetContainerObject(Me.ID, createOrAlter:=True)

                Dim entrycoll As New SortedList(Of Long, ContainerEntryDefinition)

                '** check which entries to use
                For Each anEntry In _columns.Values
                    If Not anEntry.Position.HasValue OrElse anEntry.Position <= 0 OrElse entrycoll.ContainsKey(anEntry.Position) Then
                        anEntry.Position = entrycoll.Keys.Max + 1
                    End If

                    entrycoll.Add(key:=anEntry.Position, value:=anEntry)
                Next


                ' create or alter fields of each entry
                For Each anEntry In entrycoll.Values
                    If Not databasedriver.VerifyColumnSchema(containerEntryDefinition:=anEntry, silent:=True) Then
                        databasedriver.GetColumn(tblInfo, anEntry, createOrAlter:=True)
                    End If
                Next

                '** call again to create
                tblInfo = databasedriver.GetTable(Me.ID, createOrAlter:=True, nativeContainerObject:=tblInfo)
                If tblInfo Is Nothing Then Return False

                ' create index
                For Each anIndexdefinition In _indices.Values
                    '** create the index
                    Call databasedriver.GetIndex(tblInfo, indexdefinition:=anIndexdefinition, createOrAlter:=True)
                Next
                ' save the current version also in the DB paramter Table
                databasedriver.SetDBParameter(parametername:=ConstPNBSchemaVersion_TableHeader & Me.ID.ToUpper, value:=Me.Version, silent:=True)

                '    ' reset the Table description
                '    ' only if we are connected -> bootstrapping problem
                If CurrentSession.IsRunning Then
                    If databasedriver.RetrieveTableSchema(tableID:=Me.ID, force:=True) Is Nothing Then
                        Call CoreMessageHandler(procedure:="TableDefinition.alterSchema", containerID:=tblInfo.Name, _
                                                message:="Error while setTable in alterSchema")
                    End If
                End If

                Return True
            Catch ex As Exception
                Call CoreMessageHandler(procedure:="TableDefinition.alterSchema", exception:=ex)
                Return False
            End Try

        End Function
        ''' <summary>
        '''  drop the tableschema from the database
        ''' </summary>
        ''' <param name="addToSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DropTableSchema() As Boolean

            If Not IsAlive(subname:="TableDefinition.DropTableSchema") Then Return False

            Try
                '** call to get object
                If CurrentDBDriver.DropContainerObject(Me.ID) Then
                    ' save the current version also in the DB paramter Table
                    CurrentDBDriver.DeleteDBParameter(parametername:=ConstPNBSchemaVersion_TableHeader & Me.ID.ToUpper, silent:=True)
                End If


                '    ' reset the Table description
                '    ' only if we are connected -> bootstrapping problem
                If CurrentSession.IsRunning Then
                    If Not ot.CurrentConnection.DatabaseDriver.RetrieveContainerSchema(containerID:=Me.ID, force:=True) Is Nothing Then
                        Call CoreMessageHandler(procedure:="TableDefinition.DropTableSchema", containerID:=ID, _
                                                message:="Error while drop table schema -> repository must be changed too")
                    End If
                End If

                Return True
            Catch ex As Exception
                Call CoreMessageHandler(procedure:="TableDefinition.DropTableSchema", exception:=ex)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Adds an Index to the Table Definition
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddIndex(attribute As ormIndexAttribute) As Boolean
            ' Nothing

            If Not IsAlive(subname:="TableDefinition.addIndex") Then Return False
            If Not attribute.Enabled Then Return False ' abort if not enabled

            If Not attribute.HasValuePrimaryKey Then attribute.IsPrimaryKey = False
            If Not attribute.HasValueVersion Then attribute.Version = 1
            If Not attribute.HasValueIsUnique Then attribute.IsUnique = False
            If Not attribute.HasValueDescription Then attribute.Description = "index for table " & Me.ID

            If attribute.HasValueIndexName Then
                Return AddIndex(indexname:=attribute.IndexName, _
                                columnnames:=attribute.ColumnNames, _
                                description:=Description, _
                                isprimarykey:=attribute.IsPrimaryKey, _
                                isunique:=attribute.IsUnique, _
                                version:=attribute.Version)
            End If
        End Function
        ''' <summary>
        ''' create and add an Index definition to the table
        ''' </summary>
        ''' <param name="anIndexName"></param>
        ''' <param name="aFieldCollection"></param>
        ''' <param name="PrimaryKey"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddIndex(ByVal indexname As String, _
                                 ByRef columnnames As IEnumerable(Of String), _
                                 Optional description As String = Nothing, _
                                 Optional isprimarykey As Boolean = False, _
                                 Optional isunique As Boolean = False, _
                                 Optional version As ULong = 1, _
                                 Optional replace As Boolean = False) As Boolean

            Dim fieldList As New List(Of String)
            Dim anEntry As New ContainerEntryDefinition
            Dim i As Long = 1

            ''' this is not checked since this is called during relation load while infusing
            ''' means we are not yet alive but need to add the index
            ' If Not IsAlive(subname:="TableDefinition.addIndex") Then Return False

            ' exist warning
            If _indices.ContainsKey(indexname.ToUpper) And Not replace Then
                Dim anIndex = _indices.Item(indexname.ToUpper)
                Dim same As Boolean = False

                If anIndex.IsPrimary = isprimarykey Then
                    Dim n = 0
                    same = True
                    For Each acolumnname In columnnames
                        If n < columnnames.Count - 1 AndAlso anIndex.Columnnames.ElementAt(n).ToUpper <> acolumnname.ToUpper Then
                            same = False
                            Exit For
                        End If
                        n += 1
                    Next
                End If

                If same Then
                    CoreMessageHandler(message:=" index already defined for this table - identical index with same name found", _
                                   argument:=indexname, containerID:=Me.ID, objectname:=Me.ConstObjectID, _
                                   procedure:="TableDefinition.AddIndex(String...)", messagetype:=otCoreMessageType.InternalWarning)
                    Return True
                Else
                    CoreMessageHandler(message:=" index name already exists with this table definition - might be definied in a root class or name is not unique", _
                                   argument:=indexname, containerID:=Me.ID, objectname:=Me.ConstObjectID, _
                                   procedure:="TableDefinition.AddIndex(String...)", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

            End If

            ' check fields -> should be defined to be an index
            For Each aName In columnnames
                ' check
                If Not _columns.ContainsKey(aName.ToUpper) Then
                    CoreMessageHandler(procedure:="TableDefinition.addIndex", _
                                            argument:=aName, _
                                            containerID:=Me.ID, message:=" column does not exist in table for building index " & indexname.ToUpper, _
                                            messagetype:=otCoreMessageType.InternalError)
                    Return False
                Else
                    If isprimarykey Then
                        anEntry = _columns.Item(aName.ToUpper)
                        anEntry.Indexname = indexname
                        anEntry.PrimaryKeyOrdinal = i
                        i += 1
                    End If

                    fieldList.Add(aName.ToUpper)
                End If
            Next aName

            ' remove
            If _indices.ContainsKey(indexname.ToUpper) Then
                _indices.Remove(indexname.ToUpper)
            End If

            ' add index
            Dim anIndexDefinition = IndexDefinition.Retrieve(tablename:=Me.ID, indexname:=indexname, runtimeOnly:=Me.RunTimeOnly)
            If anIndexDefinition Is Nothing Then
                anIndexDefinition = IndexDefinition.Create(tablename:=Me.ID, indexname:=indexname, runTimeOnly:=RunTimeOnly)
            End If

            anIndexDefinition.Columnnames = fieldList.ToArray
            anIndexDefinition.IsPrimary = isprimarykey
            anIndexDefinition.Version = version
            anIndexDefinition.Description = description
            anIndexDefinition.IsUnique = isunique
            _indices.Add(key:=indexname.ToUpper, value:=anIndexDefinition)

            '** add handlers
            AddHandler MyBase.OnSwitchRuntimeOff, AddressOf anIndexDefinition.OnSwitchRuntimeOff

            Return True
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetNoPrimaryKeys() As UShort
            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated And _pkname = String.Empty Then
                Return 0
            End If

            Return GetNoIndexFields(_pkname)
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetPrimaryKeyColumnNames() As List(Of String)
            ' Nothing
            If Not Me.IsAlive(subname:="GetPrimaryKeyColumnNames") And _pkname = String.Empty Then
                Return New List(Of String)
            End If

            Return GetIndexFieldNames(_pkname)
        End Function
        ''' <summary>        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetPrimaryKeyEntries() As List(Of ContainerEntryDefinition)
            ' Nothing
            If Not Me.IsAlive(subname:="GetPrimaryKeyEntries") And _pkname = String.Empty Then
                Return New List(Of ContainerEntryDefinition)
            End If

            Return GetIndexEntries(_pkname)
        End Function
        ''' <summary>
        ''' retrieves a list of Fieldnames of an Index
        ''' </summary>
        ''' <param name="IndexName">name of the Index</param>
        ''' <returns>List (of String)</returns>
        ''' <remarks></remarks>
        Public Function GetIndexFieldNames(ByVal indexname As String) As List(Of String)
            ' Nothing
            If Not Me.IsAlive(subname:="GetIndexFieldNames") Then
                Return New List(Of String)
            End If

            ' get the existing collection
            If _indices.ContainsKey(indexname.ToUpper) Then
                Return _indices.Item(indexname.ToUpper).Columnnames.ToList
            End If

            Return New List(Of String)
        End Function
        ''' <summary>
        ''' retrieves a list of Fieldnames of an Index
        ''' </summary>
        ''' <param name="IndexName">name of the Index</param>
        ''' <returns>List (of String)</returns>
        ''' <remarks></remarks>
        Public Function GetNoIndexFields(ByVal indexname As String) As UShort
            ' Nothing
            If Not Me.IsAlive(subname:="GetNoIndexFields") Then
                Return 0
            End If

            ' get the existing collection
            If _indices.ContainsKey(indexname.ToUpper) Then
                Return _indices.Item(indexname.ToUpper).Columnnames.Count
            End If

            Return 0
        End Function
        ''' <summary>
        ''' retrieves a list of Fieldnames of an Index
        ''' </summary>
        ''' <param name="IndexName">name of the Index</param>
        ''' <returns>List (of String)</returns>
        ''' <remarks></remarks>
        Public Function GetIndexEntries(ByVal indexname As String) As List(Of ContainerEntryDefinition)
            Dim aFieldCollection As New List(Of ContainerEntryDefinition)

            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return aFieldCollection
            End If

            For Each anEntryname In Me.GetIndexFieldNames(indexname)
                aFieldCollection.Add(Me.GetEntry(anEntryname))
            Next

            Return aFieldCollection
        End Function

        ''' <summary>
        ''' Delete the record and all members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub OnDeleted(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnDeleted
            ' reset it
            _columns.Clear()
            _foreignkeys.Clear()
            _indices.Clear()
        End Sub

        ''' <summary>
        ''' gets an entry by entryname or nothing
        ''' </summary>
        ''' <param name="entryname">name of the entry</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasEntry(entryname As String) As Boolean

            If Not Me.IsAlive(subname:="HasEntry") Then
                Return False
            End If

            If _columns.ContainsKey(key:=entryname) Then
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' gets an entry by columnname or nothing
        ''' </summary>
        ''' <param name="columnname">name of the column</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntry(columnname As String) As ContainerEntryDefinition

            If Not IsAlive(subname:="GetEntry") Then
                Return Nothing
            End If

            If _columns.ContainsKey(key:=columnname) Then
                Return _columns.Item(key:=columnname)
            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' Helper for rebuilding the Primary Key
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub RebuildPrimaryKey()
            '** determine the primary key and save it to indices
            Dim pkList As New SortedList(Of UShort, String)
            For Each anEntry In _columns.Values
                If anEntry.IsPrimaryKey Then
                    If pkList.ContainsKey(key:=anEntry.PrimaryKeyOrdinal) Then
                        CoreMessageHandler(message:="double primary key ordinal in column definition found - appended to the end", containerEntryName:=anEntry.Name, _
                                           containerID:=Me.ID, procedure:="TableDefinition.OnRelationloaded")
                        anEntry.PrimaryKeyOrdinal = pkList.Keys.Max + 1
                    End If
                    pkList.Add(key:=anEntry.PrimaryKeyOrdinal, value:=anEntry.Name)
                End If
            Next
            '** add it
            If pkList.Count > 0 Then Me.AddIndex(indexname:=Me.PrimaryKey, columnnames:=pkList.Values.ToList, isprimarykey:=True, replace:=True)
        End Sub



        ''' <summary>
        ''' Event handler for relations loaded
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRelationLoaded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnRelationLoad
            If e.Infusemode <> otInfuseMode.OnCreate Then RebuildPrimaryKey()

            For Each anEntry In Me.Columns
                '** add the PropertyChange Event 
                AddHandler anEntry.PropertyChanged, AddressOf Me.OnEntryChanged
                '** add handlers
                AddHandler MyBase.OnSwitchRuntimeOff, AddressOf anEntry.OnSwitchRuntimeOff
            Next
            For Each anEntry In Me.ForeignKeys
                '** add the PropertyChange Event 
                AddHandler anEntry.PropertyChanged, AddressOf Me.OnEntryChanged
                '** add handlers
                AddHandler MyBase.OnSwitchRuntimeOff, AddressOf anEntry.OnSwitchRuntimeOff
            Next
        End Sub
        ''' <summary>
        ''' returns a objecttabledefintion object
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal tablename As String, Optional dbdriver As iormRelationalDatabaseDriver = Nothing, Optional runtimeOnly As Boolean = False) As ContainerDefinition
            Return ormBusinessObject.RetrieveDataObject(Of ContainerDefinition)({tablename.ToUpper}, runtimeOnly:=runtimeOnly, dbdriver:=dbdriver)
        End Function


        ''' <summary>
        ''' create a new data object of that type
        ''' </summary>
        ''' <param name="tablename">tablename of the table</param>
        ''' <param name="runTimeOnly">if no save is possible -> bootstrapping</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal tablename As String, _
                                Optional runTimeOnly As Boolean = False, _
                                Optional checkunique As Boolean = True _
                                ) As ContainerDefinition
            Return ormBusinessObject.CreateDataObject(Of ContainerDefinition)({tablename.ToUpper}, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
        End Function



    End Class


    ''' <summary>
    ''' definition class for the permission rules on a data object
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=ObjectPermission.ConstObjectID, modulename:=ConstModuleRepository, description:="permission rules for object access", _
        version:=1, isbootstrap:=True, usecache:=True)> _
    Public Class ObjectPermission
        Inherits ormBusinessObject

        Public Const ConstObjectID = "ObjectPermissionRule"

        <ormTableAttribute(version:=1, usecache:=True, adddomainbehavior:=True, adddeletefieldbehavior:=True)> Public Const ConstPrimaryTableID = "tblObjectPermissions"


        '** Primary key
        <ormObjectEntry(referenceObjectEntry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, PrimaryEntryOrdinal:=1 _
                       )> Public Const ConstFNObjectname = AbstractEntryDefinition.ConstFNObjectName

        <ormObjectEntry(referenceObjectEntry:=ObjectContainerEntry.ConstObjectID & "." & ObjectContainerEntry.ConstFNEntryName, PrimaryEntryOrdinal:=2 _
                        )> Public Const ConstFNEntryname = AbstractEntryDefinition.ConstFNEntryName

        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, PrimaryEntryOrdinal:=3, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Operation", description:="business object operation")> Public Const ConstFNOperation = "operation"

        <ormObjectEntry(Datatype:=otDataType.Long, PrimaryEntryOrdinal:=4, defaultvalue:=10, _
                        title:="Rule Order", description:="ordinal of the rule")> Public Const ConstFNRuleordinal = "order"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, PrimaryEntryOrdinal:=5, _
                       useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** build foreign key
        ' proplematic
        '<ormForeignKey(entrynames:={ConstFNObjectname, ConstFNEntryname, ConstFNDomainID}, _
        '    foreignkeyreferences:={ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNObjectName, _
        '                           ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNEntryName, _
        '                           ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNDomainID}, _
        '                       useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKprimary = "fkpermission"


        <ormForeignKeyAttribute(entrynames:={ConstFNObjectname}, _
                             foreignkeyreferences:={ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID}, _
                             useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKprimary = "fkpermission"
        '** Fields

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, _
            title:="RuleType", description:="rule condition")> Public Const ConstFNRuleType = "typeid"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, _
            title:="Rule", description:="rule condition")> Public Const ConstFNRule = "rule"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
            title:="Allow Operation", description:="if condition andalso true allow Operation orelse if condition then disallow")> _
        Public Const ConstFNAllow = "allow"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
            title:="Exit Operation", description:="if condition andalso exittrue then stop rule processing")> _
        Public Const ConstFNExitTrue = "exitontrue"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
            title:="Exit Operation", description:="if not condition andalso exitfalse then stop rule processing")> _
        Public Const ConstFNExitFalse = "exitonfalse"
        <ormObjectEntry(Datatype:=otDataType.Memo, _
            title:="Description", description:="description of the permission rule")> Public Const ConstFNdesc = "desc"
        <ormObjectEntry(defaultvalue:=0, Datatype:=otDataType.[Long], _
            title:="Version", Description:="version counter of updating")> Public Const ConstFNVersion As String = "updc"

        '*** Mappings
        <ormObjectEntryMapping(entryname:=ConstFNObjectname)> Private _objectname As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNEntryname)> Private _entryname As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNOperation)> Private _operation As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNDomainID)> Private _domainID As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNRuleordinal)> Private _order As Long = 0
        <ormObjectEntryMapping(entryname:=ConstFNRuleType)> Private _ruletype As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNRule)> Private _rule As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNAllow)> Private _allow As Boolean
        <ormObjectEntryMapping(entryname:=ConstFNExitTrue)> Private _exitOnTrue As Boolean
        <ormObjectEntryMapping(entryname:=ConstFNExitFalse)> Private _exitOnFalse As Boolean
        <ormObjectEntryMapping(entryname:=ConstFNdesc)> Private _description As String = String.empty
        <ormObjectEntryMapping(entryname:=ConstFNVersion)> Private _version As ULong = 0

        '*** dynmaic
        Private _permissionruleProperty As ObjectPermissionRuleProperty

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property Version() As ULong
            Get
                Return Me._version
            End Get
            Set(value As ULong)
                SetValue(entryname:=ConstFNVersion, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNdesc, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the exit.
        ''' </summary>
        ''' <value>The exit.</value>
        Public Property [ExitOnFalse]() As Boolean
            Get
                Return Me._exitOnFalse
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNExitFalse, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the exit.
        ''' </summary>
        ''' <value>The exit.</value>
        Public Property [ExitOnTrue]() As Boolean
            Get
                Return Me._exitOnTrue
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNExitTrue, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the allow.
        ''' </summary>
        ''' <value>The allow.</value>
        Public Property Allow() As Boolean
            Get
                Return Me._allow
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNAllow, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the rule.
        ''' </summary>
        ''' <value>The rule.</value>
        Public Property Rule() As String
            Get
                Return Me._rule
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNRule, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the ruletype.
        ''' </summary>
        ''' <value>The ruletype.</value>
        Public Property Ruletype() As String
            Get
                Return Me._ruletype
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNRuleType, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the order.
        ''' </summary>
        ''' <value>The order.</value>
        Public ReadOnly Property Order() As Long
            Get
                Return Me._order
            End Get
        End Property

        ''' <summary>
        ''' Gets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public ReadOnly Property DomainID() As String
            Get
                Return Me._domainID
            End Get
        End Property

        ''' <summary>
        ''' Gets the operation.
        ''' </summary>
        ''' <value>The operation.</value>
        Public ReadOnly Property Operation() As String
            Get
                Return Me._operation
            End Get
        End Property

        ''' <summary>
        ''' Gets the entryname.
        ''' </summary>
        ''' <value>The entryname.</value>
        Public ReadOnly Property Entryname() As String
            Get
                Return Me._entryname
            End Get
        End Property

        ''' <summary>
        ''' Gets the objectname.
        ''' </summary>
        ''' <value>The objectname.</value>
        Public ReadOnly Property Objectname() As String
            Get
                Return Me._objectname
            End Get
        End Property
        ''' <summary>
        ''' set or gets the RuleProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RuleProperty As ObjectPermissionRuleProperty
            Set(value As ObjectPermissionRuleProperty)
                If _permissionruleProperty Is Nothing OrElse _permissionruleProperty.ToString = value.ToString Then
                    Me.Ruletype = "PROPERTY"
                    Me.ExitOnTrue = value.ExitOnTrue
                    Me.ExitOnFalse = value.ExitOnFalse
                    _permissionruleProperty = value
                    Me.IsChanged = True
                End If
            End Set
            Get
                Return _permissionruleProperty
            End Get
        End Property

#End Region

        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off the column definition via event Handler
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' returns a List of  Permissions for an objectname for the active domainID
        ''' </summary>
        ''' <param name="objectdefinition"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ByObjectName(objectname As String, Optional domainid As String = Nothing) As List(Of ObjectPermission)
            Dim aCollection As New List(Of ObjectPermission)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormRelationalTableStore
            '** set the domain
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstPrimaryTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="all", addAllFields:=True)
                If Not aCommand.IsPrepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.Where &= " AND [" & ConstFNObjectname & "] = @objectname AND [" & ConstFNEntryname & "] = ''"

                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@objectname", ColumnName:=ConstFNObjectname, tableid:=ConstPrimaryTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainid)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aCommand.SetParameterValue(ID:="@objectname", value:=objectname.ToUpper)

                aRecordCollection = aCommand.RunSelect
                Dim instantDir As New Dictionary(Of String, ObjectPermission)

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aPermission As New ObjectPermission
                    If InfuseDataObject(record:=aRecord, dataobject:=aPermission) Then
                        '** add only the domain asked or if nothing in there
                        Dim key As String = aPermission.Objectname & ConstDelimiter & aPermission.Entryname & ConstDelimiter & aPermission.Operation & ConstDelimiter & aPermission.Order.ToString
                        If instantDir.ContainsKey(key) And aPermission.DomainID = domainid Then
                            instantDir.Remove(key:=key)
                            instantDir.Add(key:=key, value:=aPermission)
                        ElseIf Not instantDir.ContainsKey(key) Then
                            instantDir.Add(key:=key, value:=aPermission)
                        End If
                    End If

                Next

                '** transfer the active entries
                For Each apermission In instantDir.Values
                    aCollection.Add(item:=apermission)
                Next

                Return aCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, procedure:="ObjectPermission.ByObjectname")
                Return aCollection

            End Try

        End Function


        ''' <summary>
        ''' creates a ObjectPermission
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="order"></param>
        ''' <param name="operationname"></param>
        ''' <param name="entryname"></param>
        ''' <param name="domainID"></param>
        ''' <param name="checkUnique"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function Create(objectname As String, order As Long, _
                                         Optional operationname As String = "", _
                                         Optional entryname As String = "", _
                                         Optional domainid As String = Nothing, _
                                         Optional checkUnique As Boolean = True, _
                                            Optional runtimeOnly As Boolean = False) As ObjectPermission
            If String.IsNullOrEmpty(domainid) Then domainid = CurrentSession.CurrentDomainID
            Dim pkarray As Object() = {objectname.ToUpper, entryname.ToUpper, operationname.ToUpper, order, domainid}
            Return ormBusinessObject.CreateDataObject(Of ObjectPermission)(pkArray:=pkarray, domainID:=domainid, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' retrieves a ObjectPermission
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="order"></param>
        ''' <param name="operationname"></param>
        ''' <param name="entryname"></param>
        ''' <param name="domainID"></param>
        ''' <param name="checkUnique"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Shared Function Retrieve(objectname As String, order As Long, _
                                           Optional operationname As String = "", _
                                           Optional entryname As String = "", _
                                           Optional domainid As String = Nothing, _
                                            Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
                                            Optional runtimeOnly As Boolean = False) As ObjectPermission
            If String.IsNullOrEmpty(domainid) Then domainid = CurrentSession.CurrentDomainID
            Dim pkarray As Object() = {objectname.ToUpper, entryname.ToUpper, operationname.ToUpper, order, domainid}
            Return ormBusinessObject.RetrieveDataObject(Of ObjectPermission)(pkArray:=pkarray, domainID:=domainid, dbdriver:=dbdriver, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormBusinessObject.CreateDataObjectSchema(Of ObjectPermission)(silent:=silent)
        End Function
        ''' <summary>
        ''' Handler for the RecordFed Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnFeeding(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnFeeding
            Try
                If _permissionruleProperty IsNot Nothing Then
                    Me.Ruletype = "PROPERTY"
                    Me.Rule = _permissionruleProperty.ToString
                End If

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, procedure:="ObjectPermission.OnInfused", messagetype:=otCoreMessageType.InternalError)
            End Try
        End Sub

        ''' <summary>
        ''' Handler for the OnInfused Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused
            Try
                If Me.Ruletype = "PROPERTY" Then Me._permissionruleProperty = New ObjectPermissionRuleProperty(Me.Rule)
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, procedure:="ObjectPermission.OnInfused", messagetype:=otCoreMessageType.InternalError)
            End Try
        End Sub


        ''' <summary>
        ''' applies the current permission rule on the current user and returns the result
        ''' </summary>
        ''' <param name="user"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckFor([user] As User, ByRef [exit] As Boolean, Optional domainid As String = Nothing) As Boolean
            If Not Me.IsAlive(subname:="CheckFor") Then Return False
            Dim result As Boolean
            If String.IsNullOrEmpty(domainid) Then domainid = CurrentSession.CurrentDomainID

            Try

                '** evaluate the rules
                Select Case _permissionruleProperty.[Enum]
                    '*** check on user rights
                    '*** and on the user's group rights
                    Case otObjectPermissionRuleProperty.DBAccess
                        If _permissionruleProperty.Validate Then
                            Dim accessright = New AccessRightProperty(_permissionruleProperty.Arguments(0).ToString)
                            result = AccessRightProperty.CoverRights(rights:=user.AccessRight, covers:=accessright.[Enum])
                            If Not result Then
                                For Each groupname In user.GroupNames
                                    Dim aGroup As Commons.Group = Commons.Group.Retrieve(groupname:=groupname)
                                    If aGroup IsNot Nothing Then
                                        result = AccessRightProperty.CoverRights(rights:=aGroup.AccessRight, covers:=accessright.[Enum])
                                    Else
                                        CoreMessageHandler(message:="Groupname not found", argument:=_permissionruleProperty.ToString, _
                                                procedure:="ObjectPermission.CheckFor", objectname:=Me.Objectname, messagetype:=otCoreMessageType.InternalError)
                                        '* do not set  a result
                                    End If
                                Next
                            End If

                        Else
                            result = False 'wrong value -> false
                        End If

                        '*** check on membership
                    Case otObjectPermissionRuleProperty.Group
                        If _permissionruleProperty.Validate Then
                            Dim groupname As String = _permissionruleProperty.Arguments(0).ToString
                            If user.GroupNames.Contains(groupname) Then
                                result = True
                            Else
                                result = False
                            End If
                        Else
                            result = False 'wrong value -> false
                        End If

                        '** compare the individual member
                    Case otObjectPermissionRuleProperty.User
                        If _permissionruleProperty.Validate Then
                            Dim username As String = _permissionruleProperty.Arguments(0).ToString
                            If user.Username.ToUpper = username.ToUpper Then
                                result = True
                            Else
                                result = False
                            End If
                        Else
                            result = False 'wrong value -> false
                        End If
                    Case Else
                        CoreMessageHandler(message:="ObjectPermissionRuleProperty not implemented", argument:=_permissionruleProperty.ToString, _
                                            procedure:="ObjectPermission.CheckFor", objectname:=Me.Objectname, messagetype:=otCoreMessageType.InternalError)
                        result = False 'wrong value -> false

                End Select
                '* exit flag
                If (result AndAlso ExitOnTrue) OrElse (Not result AndAlso _exitOnFalse) Then
                    [exit] = True
                End If
                Return result

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, procedure:="ObjectPermission.Checkfor")
                Return False
            End Try


        End Function
    End Class

    ''' <summary>
    ''' definition class data for an OTDB data object classes
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=ObjectDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="persistable Business Object definition", _
        Version:=1, isbootstrap:=True, usecache:=True)> _
    Public Class ObjectDefinition
        Inherits ormBusinessObject
        Implements iormInfusable
        Implements iormRelationalPersistable

        Public Const ConstObjectID = "ObjectDefinition"



        ''' <summary>
        ''' Object Defintion Event Arguments
        ''' </summary>
        ''' <remarks></remarks>

        Public Class EventArgs
            Inherits System.EventArgs

            Private _objectname As String

            Public Sub New(objectname As String)
                _objectname = objectname
            End Sub
            ''' <summary>
            ''' Gets the error.
            ''' </summary>
            ''' <value>The error.</value>
            Public ReadOnly Property Objectname() As String
                Get
                    Return _objectname
                End Get
            End Property

        End Class

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormTableAttribute(version:=2, usecache:=True)> Public Const ConstPrimaryTableID = "tblObjectDefinitions"
        ''' <summary>
        ''' Index Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormIndex(columnname1:=ConstFNClass)> Public Const ConstIndexName = "name"

        ''' <summary>
        ''' Primary key Column
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, PrimaryEntryOrdinal:=1, properties:={ObjectEntryProperty.Keyword}, _
                         XID:="OBJID", title:="Object ID", description:="unique name of the Object")> Public Const ConstFNID = "id"

        ''' <summary>
        ''' Column Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, properties:={ObjectEntryProperty.Trim}, _
                        title:="Object Class Name", description:="class name of the Object")> Public Const ConstFNClass = "class"
        <ormObjectEntry(Datatype:=otDataType.Memo, _
                        title:="Object Description", description:="description of the Object")> Public Const ConstFNdesc = "desc"
        <ormObjectEntry(defaultvalue:="0", Datatype:=otDataType.[Long], _
                        title:="Version", Description:="version counter of updating")> Public Const ConstFNVersion As String = "updc"
        <ormObjectEntry(Datatype:=otDataType.Bool, _
                        title:="Is Active", defaultvalue:=True, dbdefaultvalue:="1", _
                        Description:="set if the object is active")> Public Const ConstFNISActive As String = "isactive"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, properties:={ObjectEntryProperty.Upper, ObjectEntryProperty.Trim}, _
                        title:="Object Module", description:="name of the module the object belongs to")> Public Const ConstFNModule = "module"
        <ormObjectEntry(Datatype:=otDataType.List, size:=255, innerDatatype:=otDataType.Text, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Properties", Description:="properties on object level")> Public Const ConstFNProperties As String = "properties"
        <ormObjectEntry(Datatype:=otDataType.Bool, _
                        title:="Use Cache", defaultvalue:=False, Description:="set if the entry is object cached")> Public Const ConstFNUseCache As String = "objectcache"
        <ormObjectEntry(Datatype:=otDataType.List, size:=255, innerDatatype:=otDataType.Text, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Cache", Description:="cache properties on object level")> Public Const ConstFNCacheProperties As String = "cacheproperties"
        <ormObjectEntry(Datatype:=otDataType.List, size:=255, innerDatatype:=otDataType.Text, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Primary Key", description:="names of the object unique keys")> Public Const ConstFNPrimaryKeys = "primarykeynames"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, properties:={ObjectEntryProperty.Upper, ObjectEntryProperty.Trim}, _
                       title:="Primary Table", description:="name of the primary table of the object")> Public Const ConstFNPrimaryTable = "primarytable"

        <ormObjectEntry(Datatype:=otDataType.List, size:=255, innerDatatype:=otDataType.Text, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Tables", description:="tables of the object")> Public Const ConstFNtablenames = "tables"
        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, properties:={ObjectEntryProperty.Upper, ObjectEntryProperty.Trim}, isnullable:=True, _
                       title:="Retrieve ViewID", description:="name of the primary view of the object (if multiple tables)")> Public Const ConstFNRetrieveView = "RETRIEVEFROMVIEWID"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="TableDeleteFlagBehaviour", Description:="set if the object runs the delete per flag behavior")> Public Const ConstFNDeletePerFlag As String = "deleteperflag"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="TableSpareFieldsBehaviour", Description:="set if the object has additional spare fields behavior")> Public Const ConstFNSpareFieldsFlag As String = "spareFields"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="DomainBehaviour", Description:="set if the object belongs to a domain")> Public Const ConstFNDomainFlag As String = "domainBehavior"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="1", _
                                title:="DefaultPermission", Description:="permission for object if no permissions are found")> Public Const ConstFNDefaultPermission As String = "defaultpermission"

        '** do not loop in foreign keys
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                      useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' RELATIONS for Entries - Capitalize is correct Column instead COLUMN
        '''

        <ormRelationAttribute(linkobject:=GetType(ObjectContainerEntry), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNID}, toEntries:={ObjectContainerEntry.ConstFNObjectName}, linkjoin:="AND [" & ObjectContainerEntry.ConstFNType & "] = '" & "Column" & "'")> _
        Public Const ConstRColumnEntries = "ColumnEntries"

        <ormRelationAttribute(linkobject:=GetType(ObjectCompoundEntry), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNID}, toEntries:={ObjectCompoundEntry.ConstFNObjectName}, linkjoin:="AND [" & ObjectCompoundEntry.ConstFNType & "] = 'Compound'")> _
        Public Const ConstRCompoundEntries = "CompoundEntries"

        <ormObjectEntryMapping(RelationName:=ConstRColumnEntries, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectContainerEntry.ConstFNEntryName})> _
        <ormObjectEntryMapping(RelationName:=ConstRCompoundEntries, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectCompoundEntry.ConstFNEntryName})> Private WithEvents _objectentries As New Dictionary(Of String, iormObjectEntry) ' by id


        '*** Mapping
        <ormObjectEntryMapping(EntryName:=ConstFNID)> Private _id As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNClass)> Public _class As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNdesc)> Private _description As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNModule)> Private _modulename As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNISActive)> Private _isactive As Boolean = True
        <ormObjectEntryMapping(EntryName:=ConstFNUseCache)> Private _useCache As Boolean
        <ormObjectEntryMapping(EntryName:=ConstFNCacheProperties)> Private _CacheProperties As String()
        <ormObjectEntryMapping(EntryName:=ConstFNDeletePerFlag)> Private _deletePerFlagBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNDomainFlag)> Private _domainBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNSpareFieldsFlag)> Private _SpareFieldsFlagBehavior As Boolean = False
        <ormObjectEntryMapping(EntryName:=ConstFNVersion)> Private _Version As Long = 0
        <ormObjectEntryMapping(EntryName:=ConstFNPrimaryKeys)> Private _pkentrynames As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNtablenames)> Private _tablenames As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNDefaultPermission)> Private _defaultpermission As Boolean = True
        <ormObjectEntryMapping(EntryName:=ConstFNPrimaryTable)> Private _primarytableid As String
        <ormObjectEntryMapping(EntryName:=ConstFNRetrieveView)> Private _retrieveFromViewID As String
        ''' <summary>
        ''' Relations which will be handled by events
        ''' </summary>
        ''' <remarks></remarks>
        Private _tables As New Dictionary(Of String, ContainerDefinition) ' relations will be handled by events - list to load stored in _tablenames
        Private _objectpermissions As New Dictionary(Of String, SortedList(Of Long, ObjectPermission)) 'ObjectPermissions by Operation and the sorted rules list

        Public Shared Event ObjectDefinitionChanged As EventHandler(Of EventArgs)
        Public Shared Event OnObjectSchemaCreating(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event OnObjectSchemaCreated(sender As Object, e As ormDataObjectEventArgs)

        '** runtime variables
        Private _lock As New Object
        Private _DefaultDomainID As String = String.empty
        Private _isBootStrappingObject As Nullable(Of Boolean)

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(objectid:=ConstObjectID)

        End Sub

#Region "Properties"

        ''' <summary>
        ''' gets the ID of the object defintion
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get
        End Property

        ''' <summary>
        ''' retrieves number of entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Count() As Long
            Get
                Count = _objectentries.Count - 1
            End Get

        End Property

        ''' <summary>
        ''' Gets or sets the primarytableid.
        ''' </summary>
        ''' <value>The primarytableid.</value>
        Public Property PrimaryTableId() As String
            Get
                Return Me._primarytableid
            End Get
            Set(value As String)
                SetValue(ConstFNPrimaryTable, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the view id from which the object is retrieved (optional - otherwise from the tables)
        ''' </summary>
        ''' <value>The primarytableid.</value>
        Public Property RetrieveObjectFromViewID As String
            Get
                Return Me._retrieveFromViewID
            End Get
            Set(value As String)
                SetValue(ConstFNRetrieveView, value)
            End Set
        End Property

        ''' <summary>
        ''' returns true if this Object is a Bootstrapping Object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsBootStrappingObject As Boolean
            Get
                If Not _isBootStrappingObject.HasValue Then
                    _isBootStrappingObject = ot.GetBootStrapObjectClassIDs.Contains(Me.ID)
                End If
                Return _isBootStrappingObject
            End Get
        End Property
        ''' <summary>
        ''' set or gets the object active
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property IsActive() As Boolean
            Get
                Return Me._isactive
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNISActive, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Module name.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public ReadOnly Property ObjectType() As System.Type
            Get
                Return System.Type.GetType(Me.Classname)
            End Get

        End Property
        ''' <summary>
        ''' Gets or sets the .net class name.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Classname() As String
            Get
                'Return Me._class
                Return GetValue(entryname:=ConstFNClass)
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNClass, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Module name.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Keys() As String()
            Get
                Return Me._pkentrynames
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNPrimaryKeys, value:=value)
                'Me._pknames = value
            End Set
        End Property
        ''' <summary>
        ''' Gets a list of the table definitions
        ''' </summary>
        ''' <value>The parameters.</value>
        Public ReadOnly Property Tables() As List(Of ContainerDefinition)
            Get
                Return Me._tables.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' Returns a ordered enumerable of ObjectPermissionRules 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PermissionRules As IEnumerable(Of ObjectPermission)
            Get
                Dim aList As New List(Of ObjectPermission)
                For Each aSubList As SortedList(Of Long, ObjectPermission) In _objectpermissions.Values
                    For Each aPermission In aSubList.Values
                        aList.Add(aPermission)
                    Next
                Next
                Return aList
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the table names.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Tablenames() As String()
            Get
                Return Me._tablenames
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNtablenames, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Module name.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Modulename() As String
            Get
                Return Me._modulename
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNModule, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' set or gets the default permission (true if accessible) if no permission rules are applying
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property DefaultPermission() As Boolean
            Get
                Return Me._defaultpermission
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNDefaultPermission, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Description.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNdesc, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the parameters.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Properties() As String()
            Get
                Return Me._properties
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNProperties, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' use Cache on this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property UseCache As Boolean
            Set(value As Boolean)
                SetValue(entryname:=ConstFNUseCache, value:=value)
            End Set
            Get
                Return _useCache
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the cache selection string
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property CacheProperties As List(Of String)
            Set(value As List(Of String))
                SetValue(entryname:=ConstFNCacheProperties, value:=value.ToArray)
            End Set
            Get
                Return _CacheProperties.ToList
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain behavior.
        ''' </summary>
        ''' <value>The domain behavior.</value>
        Public Property HasDomainBehavior() As Boolean
            Get
                Return Me._domainBehavior
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNDomainFlag, value:=value)
                'Me._domainBehavior = value
            End Set
        End Property
        ''' <summary>
        ''' gets or set the version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Version As Long
            Get
                Return _Version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNVersion, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or set the the spare fields behavior for the Object descibed
        ''' . Means extra fields are available.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property HasSpareFieldsBehavior
            Set(value)
                SetValue(entryname:=ConstFNSpareFieldsFlag, value:=value)
            End Set
            Get
                Return _SpareFieldsFlagBehavior
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the delete per flag behavior for the Object (not the Object Definition).
        '''  If true a deleteflag and a delete date are available.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property HasDeleteFieldBehavior As Boolean
            Set(value As Boolean)
                SetValue(entryname:=ConstFNDeletePerFlag, value:=value)
            End Set
            Get
                Return _deletePerFlagBehavior
            End Get
        End Property

        ''' <summary>
        ''' returns a List of CompoundEntryObjectNames
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CompoundEntryObjectNames As IList(Of String)
            Get
                Dim aList As New List(Of String)

                For Each anEntry As iormObjectEntry In _objectentries.Values
                    If anEntry.IsCompound AndAlso Not aList.Contains(anEntry.Objectname) Then
                        aList.Add(anEntry.Objectname)
                    End If
                Next

                Return aList
            End Get
        End Property
#End Region


        ''' <summary>
        ''' returns a list of entry names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Entrynames(Optional onlyActive As Boolean = True) As IList(Of String)
            If Not Me.IsAlive(subname:="ObjectDefinition.Entrynames") Then Return New List(Of String)
            If onlyActive Then
                Dim alist As List(Of String) = _objectentries.Where(Function(x) x.Value.IsActive).Select(Function(x) x.Key).ToList
                Return alist
            End If

            Return _objectentries.Keys.ToList()
        End Function
        ''' <summary>
        ''' returns all Entries -unordered active or not
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Entries As IList(Of iormObjectEntry)
            Get
                Return _objectentries.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' gets a collection of (active) object Entry definitions ordered by ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntries(Optional onlyActive As Boolean = True) As IList(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.GetEntries") Then
                If onlyActive Then Return _objectentries.Values.Where(Function(x) x.IsActive = True).OrderBy(Function(x) x.Ordinal).ToList
                Return _objectentries.Values.OrderBy(Function(x) x.Ordinal).ToList
            Else
                Return New List(Of iormObjectEntry)
            End If
        End Function
        ''' <summary>
        ''' gets a collection of object compound Entry definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCompoundEntries(Optional onlyActive As Boolean = True) As IList(Of iormObjectEntry)
            Dim aList As New List(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.GetCompoundEntries") Then
                If onlyActive Then
                    aList = _objectentries.Values.Where(Function(x) x.IsActive And x.IsCompound).ToList()
                Else
                    aList = _objectentries.Values.Where(Function(x) x.IsCompound).ToList
                End If
                If aList IsNot Nothing AndAlso aList.Count > 0 Then Return aList
            End If

            Return New List(Of iormObjectEntry)
        End Function
        ''' <summary>
        ''' gets a collection of object column Entry definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetColumnEntries(Optional onlyActive As Boolean = True) As IList(Of iormObjectEntry)
            Dim aList As New List(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.GetColumnEntries") Then
                If onlyActive Then
                    aList = _objectentries.Values.Where(Function(x) x.IsActive And x.IsColumn).ToList()
                Else
                    aList = _objectentries.Values.Where(Function(x) x.IsColumn).ToList
                End If
                If aList IsNot Nothing AndAlso aList.Count > 0 Then Return aList
            End If

            Return New List(Of iormObjectEntry)
        End Function

        ''' <summary>
        ''' gets a Ilist  of object Entry definitions ordered by the ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetOrderedEntries(Optional onlyActive As Boolean = True) As IList(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.Entries") Then
                If onlyActive Then Return _objectentries.Values.Where(Function(x) x.IsActive = True).ToList.OrderBy(Function(x) x.Ordinal).ToList
                Return _objectentries.Values.ToList.OrderBy(Function(x) x.Ordinal).ToList
            Else
                Dim aList As New List(Of iormObjectEntry)
                Return aList.OrderBy(Function(x) x.Ordinal).ToList
            End If
        End Function


        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnCreated
            Dim myself = TryCast(e.DataObject, ObjectDefinition)
            If myself IsNot Nothing Then myself.DomainID = ConstGlobalDomain
        End Sub
        ''' <summary>
        ''' sets the values by attributes
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetValuesBy(attribute As ormObjectAttribute) As Boolean
            If Not IsAlive(subname:="SetValuesBy") Then Return False

            With attribute
                If .HasValueClassname Then Me.Classname = .ClassName
                If .HasValueProperties Then Me.Properties = .Properties
                If .HasValueDomainBehavior Then Me.HasDomainBehavior = .AddDomainBehavior
                If .HasValueSpareFieldsBehavior Then Me.HasSpareFieldsBehavior = .AddSpareFieldsBehavior
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueDeleteFieldBehavior Then Me.HasDeleteFieldBehavior = .AddDeleteFieldBehavior
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueIsActive Then Me.IsActive = .IsActive
                If .HasValuePrimaryKeys Then Me._pkentrynames = .PrimaryKeyEntryNames
                If .HasValueContainerIDs Then Me.Tablenames = .ContainerIDs
                If .HasValueUseCache Then Me.UseCache = .UseCache
                If .HasValueCacheProperties Then Me.CacheProperties = .CacheProperties.ToList
                If .HasValueDefaultPermission Then Me.DefaultPermission = .DefaultPermission
                If .HasValueModulename Then Me.Modulename = .Modulename
                If .HasValuePrimaryContainerID Then Me.PrimaryTableId = .PrimaryContainerID
                If .HasValueRetrieveObjectFromViewID Then Me.RetrieveObjectFromViewID = .RetrieveObjectFromViewID
            End With

            Return True
        End Function
        ''' <summary>
        ''' sets the values by attributes
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddPermissionRule(attribute As ormObjectTransactionAttribute, Optional runtimeOnly As Boolean = False, Optional domainid As String = Nothing) As Boolean
            If Not IsAlive(subname:="AddPermissionRule") Then Return False

            '** bootstrap
            If Not runtimeOnly Then runtimeOnly = CurrentSession.IsBootstrappingInstallationRequested
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            With attribute

                If .HasValuePermissionRules AndAlso .HasValueTransactionName Then
                    For Each [property] In attribute.PermissionRules
                        Dim permissions As New SortedList(Of Long, ObjectPermission)
                        Dim orderno As ULong
                        Dim max As ULong = 0

                        If _objectpermissions.ContainsKey(key:=attribute.TransactionName.ToUpper) Then
                            permissions = _objectpermissions.Item(key:=attribute.TransactionName.ToUpper)
                            For Each aPermission In permissions.Values
                                If max = 0 OrElse max < aPermission.Order Then max = aPermission.Order
                            Next
                            orderno = max + 10
                        Else
                            _objectpermissions.Add(key:=attribute.TransactionName.ToUpper, value:=permissions)
                            orderno = 10
                        End If


                        Dim aRule As ObjectPermission = ObjectPermission.Create(objectname:=Me.ID, order:=orderno, operationname:=attribute.TransactionName, _
                                                                                domainid:=domainid, checkUnique:=True, runtimeOnly:=runtimeOnly)

                        Try
                            aRule.RuleProperty = New ObjectPermissionRuleProperty([property])
                            If .HasValueDefaultAllowPermission Then aRule.Allow = attribute.DefaultAllowPermission
                            If .HasValueVersion Then aRule.Version = attribute.Version
                            If .HasValueDescription Then aRule.Description = attribute.Description


                            permissions.Add(key:=aRule.Order, value:=aRule)

                            '** add handlers
                            AddHandler MyBase.OnSwitchRuntimeOff, AddressOf aRule.OnSwitchRuntimeOff

                        Catch ex As Exception
                            CoreMessageHandler(exception:=ex, procedure:="ObjectDefinition.AddPermissionRule", argument:=[property])
                            Return False
                        End Try


                    Next
                Else
                    CoreMessageHandler(message:="Attribute has no operationname or no rules", procedure:="ObjectDefinition.AddPermissionRule", _
                                       messagetype:=otCoreMessageType.InternalWarning, objectname:=Me.ObjectID, argument:=attribute)
                End If

            End With

            Return True
        End Function

        ''' <summary>
        ''' adds a table entry by an table attribute 
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddTable(attribute As ormTableAttribute, Optional runtimeOnly As Boolean = False) As Boolean
            Dim aTableDefinition As ContainerDefinition

            '** bootstrap
            If Not runtimeOnly Then runtimeOnly = CurrentSession.IsBootstrappingInstallationRequested

            If attribute.TableID Is Nothing OrElse attribute.TableID = String.empty Then
                CoreMessageHandler(message:="attribute need a non-empty table name", objectname:=Me.ID, _
                                   messagetype:=otCoreMessageType.InternalError, procedure:="ObjectDefinition.AddTableByAttribute")
                Return False
            End If

            '* does the table exist in the object
            If _tables.ContainsKey(key:=attribute.TableID) Then
                '**
                aTableDefinition = _tables.Item(key:=attribute.TableID)
            Else

                aTableDefinition = ContainerDefinition.Retrieve(tablename:=attribute.TableID, runtimeOnly:=runtimeOnly)
                If aTableDefinition Is Nothing Then
                    aTableDefinition = ContainerDefinition.Create(tablename:=attribute.TableID, checkunique:=True, runTimeOnly:=runtimeOnly)
                End If

                _tables.Add(key:=attribute.TableID, value:=aTableDefinition)
            End If

            ''' check if table is also listed in the relation field
            ''' 
            If _tablenames Is Nothing Then
                ReDim _tablenames(0)
                _tablenames(0) = attribute.TableID
            ElseIf Not _tablenames.Contains(attribute.TableID) Then
                ReDim Preserve _tablenames(_tablenames.GetUpperBound(0) + 1)
                _tablenames(_tablenames.GetUpperBound(0)) = attribute.TableID
            End If

            '** set the values of the table definition
            With attribute
                If Not .HasValueAddDomainBehavior Then .AddDomainBehavior = Me.HasDomainBehavior
                If Not .HasValueDeleteFieldBehavior Then .AddDeleteFieldBehavior = Me.HasDeleteFieldBehavior
                If Not .HasValueSpareFields Then .AddSpareFields = Me.HasSpareFieldsBehavior
                If Not .HasValueVersion Then .Version = 1
            End With
            '* set the values of the table definition
            aTableDefinition.SetValuesBy(attribute)
            '** set the object
            Me.HasDomainBehavior = Me.HasDomainBehavior Or aTableDefinition.DomainBehavior
            Me.HasDeleteFieldBehavior = Me.HasDeleteFieldBehavior Or aTableDefinition.DeletePerFlagBehavior
            Me.HasSpareFieldsBehavior = Me.HasSpareFieldsBehavior Or aTableDefinition.SpareFieldsBehavior

            '** add Handlers in the Table
            AddHandler Me.OnSwitchRuntimeOff, AddressOf aTableDefinition.OnSwitchRuntimeOff

            Return True
        End Function

        ''' <summary>
        ''' adds a column entry by an ObjectEntry Attribute 
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntry(attribute As ormObjectEntryAttribute, Optional runtimeOnly As Boolean = False, Optional domainid As String = Nothing) As Boolean
            Dim anEntry As iormObjectEntry
            Dim bootstrap As Boolean = runtimeOnly

            If Not attribute.HasValueEntryName Then
                CoreMessageHandler(message:="attribute as no entry name", procedure:="ObjectDefinition.AddEntryByAttribute(ormEntryAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=_id)
                Return False
            End If

            If String.IsNullOrEmpty(domainid) Then domainid = CurrentSession.CurrentDomainID

            If _objectentries.ContainsKey(key:=attribute.Entryname) Then
                '**
                anEntry = _objectentries.Item(key:=attribute.Entryname)
            Else
                '''
                ''' the entries are added by event handler of the abstract entry
                If attribute.EntryType = otObjectEntryType.ContainerEntry Then
                    anEntry = ObjectContainerEntry.Retrieve(objectname:=Me.ID, entryname:=attribute.Entryname, runtimeOnly:=bootstrap)
                    If anEntry Is Nothing Then
                        anEntry = ObjectContainerEntry.Create(objectname:=Me.ID.Clone, entryname:=attribute.Entryname.Clone, _
                                                           tablename:=attribute.ContainerID.Clone, columnname:=attribute.ContainerEntryName.Clone, _
                                                           checkunique:=True, domainid:=domainid, runtimeOnly:=bootstrap)
                    End If
                    '*** add the switchoff handler
                    AddHandler MyBase.OnSwitchRuntimeOff, AddressOf anEntry.OnswitchRuntimeOff

                ElseIf attribute.EntryType = otObjectEntryType.Compound Then
                    anEntry = ObjectCompoundEntry.Retrieve(objectname:=Me.ID, entryname:=attribute.Entryname, runtimeOnly:=bootstrap)
                    If anEntry Is Nothing Then
                        anEntry = ObjectCompoundEntry.Create(objectname:=Me.ID, entryname:=attribute.Entryname, checkunique:=True, runtimeOnly:=bootstrap)
                    End If

                Else
                    CoreMessageHandler(message:="EntryType of object entry attribute is unknown to create", procedure:="ObjectDefinition.AddEntry(attribute)", _
                                        messagetype:=otCoreMessageType.InternalError, objectname:=attribute.Objectname, entryname:=attribute.Entryname)
                    Return False
                End If
            End If

            '** set the entry according to the Attribute
            Return anEntry.SetByAttribute(attribute)

        End Function

        ''' <summary>
        ''' static create object schema out of attributes
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateObjectSchema(objecttype As System.Type) As Boolean

            Dim anObjectDefinition As ObjectDefinition
            Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescription(type:=objecttype)

            If anObjectDescription IsNot Nothing Then
                '** check if this is an bootstrap
                If anObjectDescription.ObjectAttribute.IsBootstrap Then ot.CurrentDBDriver.VerifyOnTrackDatabase()
                '** get ObjectDefinitoin
                anObjectDefinition = ot.CurrentSession.Objects.GetObject(anObjectDescription.ObjectAttribute.ID)
                '** run through the instance
                Return anObjectDefinition.CreateObjectSchema()
            Else
                CoreMessageHandler(message:="object was not found by type", argument:=objecttype.Name, objectname:=objecttype.Name, _
                                  procedure:="objectdefinition.CreateObjectSchema(Shared)", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

        End Function

        ''' <summary>
        ''' Create the Object Schema in the Database for this ObjectDefinition
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateObjectSchema(Optional silent As Boolean = False) As Boolean
            Dim result As Boolean = True
            If Not Me.IsAlive(subname:="CreateObjectSchema") Then Return False

            ''' fire event
            ''' 
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=Nothing)
            RaiseEvent OnObjectSchemaCreating(Nothing, e:=ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return False
            End If


            ''' create the tables -> creates the columns -> creates the indices
            ''' 

            For Each aTableDefinition In Me.Tables
                If aTableDefinition.AlterTableSchema() Then
                    result = result And True
                Else
                    result = result And False
                End If
            Next

            ''' create a view to infuse the objects from if this was specified
            ''' 
            Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ID)
            If result AndAlso aDescription.ObjectAttribute.HasValueBuildRetrieveView _
                      AndAlso aDescription.ObjectAttribute.BuildRetrieveView Then
                Dim aName As String
                If aDescription.ObjectAttribute.HasValueRetrieveObjectFromViewID Then
                    aName = aDescription.ObjectAttribute.RetrieveObjectFromViewID
                Else
                    aName = "VW" & Me.ID & "s"
                End If
                ''' build the view
                ''' 
                If Me.AlterRetrieveViewSchema(aName) Then
                    Me.RetrieveObjectFromViewID = aName
                Else
                    Me.RetrieveObjectFromViewID = Nothing
                End If

            End If

            '** fire event
            ourEventArgs = New ormDataObjectEventArgs([object]:=Me)
            RaiseEvent OnObjectSchemaCreated(Nothing, e:=ourEventArgs)

            Return result
        End Function

        ''' <summary>
        ''' Create the Primary view schema out of multiple tables
        ''' </summary>
        ''' <param name="viewid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AlterRetrieveViewSchema(viewid As String) As Boolean

            If Not IsAlive(subname:="ObjectDefinition.AlterPrimaryViewSchema") Then Return False
            Dim sqlselectcmd As New Text.StringBuilder("SELECT ")
            Dim aTableSequence As New List(Of ContainerDefinition)

            Try
                '' build the sequence
                aTableSequence.Add(Me.Tables.First(Function(x) x.ID = Me.PrimaryTableId))
                For Each aTable As ContainerDefinition In Me.Tables
                    If aTable.ID <> Me.PrimaryTableId Then aTableSequence.Add(aTable)
                Next


                ''' add the resulting columns from all tables
                ''' 
                Dim firstColumn As Boolean = True 'leave it here that also the columns of the other tables get in the list with comma
                For Each aTable In aTableSequence
                    Dim aTablename As String = ot.CurrentDBDriver.GetNativeDBObjectName(aTable.ID)
                    For Each aColumn As ContainerEntryDefinition In aTable.Columns
                        If firstColumn Then
                            firstColumn = False
                        Else
                            sqlselectcmd.AppendFormat(", ")
                        End If
                        sqlselectcmd.AppendFormat(" [{0}].[{1}] AS '{2}.{1}' ", aTable.ID, aColumn.Name, aTable.ID)
                    Next
                Next

                ''' add the resulting tables and inner joins
                ''' 
                For Each aTable In aTableSequence

                    ''' build the joins
                    If aTableSequence.First.ID = aTable.ID Then
                        sqlselectcmd.AppendFormat(" FROM [{0}] AS [{1}] ", ot.CurrentDBDriver.GetNativeDBObjectName(aTable.ID), aTable.ID)
                    Else

                        Dim i As Integer = aTableSequence.IndexOf(aTable)
                        sqlselectcmd.AppendFormat(" INNER JOIN [{0}] AS [{1}] ON ", ot.CurrentDBDriver.GetNativeDBObjectName(aTable.ID), aTable.ID)

                        ''' search for a foreign key which is a primary table link
                        ''' 
                        For Each aForeignKey In aTable.ForeignKeys
                            If aForeignKey.ForeignKeyProperty.Where(Function(x) x.Enum = otForeignKeyProperty.PrimaryTableLink).Count > 0 Then
                                ''' look for a table which is not me ...
                                For Each nextTable As ContainerDefinition In aTableSequence.SkipWhile(Function(x) x.ID = aTable.ID)
                                    ''' ... and also in the foreign key reference
                                    If aForeignKey.ForeignKeyReferenceTables.Contains(nextTable.ID) Then
                                        '' add each condition
                                        For i = aForeignKey.ColumnNames.GetLowerBound(0) To aForeignKey.ColumnNames.GetUpperBound(0)
                                            If i > 0 Then sqlselectcmd.AppendFormat(" AND ")
                                            '** columnnames(i) is in tableid.columnname notation
                                            '** the foreignkeyreference is tableid.columnname notation
                                            sqlselectcmd.AppendFormat(" {0} = {1} ", aForeignKey.ColumnNames(i), aForeignKey.ForeignKeyReferences(i))
                                        Next
                                        Exit For 'exit the inner loop since we build the join
                                    End If
                                Next
                            End If

                        Next
                    End If
                Next

                ''' create
                ''' 
                If CurrentDBDriver.GetType.GetInterfaces.Contains(GetType(Database.iormRelationalDatabaseDriver)) Then
                    Dim aView = CType(CurrentDBDriver, iormRelationalDatabaseDriver).GetView(createOrAlter:=True, name:=viewid, sqlselect:=sqlselectcmd.ToString)
                    If aView IsNot Nothing Then

                        ''' create an index for each primary key on the view
                        ''' 
                        For Each aTable In aTableSequence

                            For Each anIndex In aTable.Indices
                                If anIndex.IsPrimary Then
                                    ' add index -> do not save since views are also not saved
                                    Dim anIndexDefinition = IndexDefinition.Retrieve(tablename:=viewid, indexname:=aTable.ID & "." & aTable.PrimaryKey, runtimeOnly:=True)
                                    If anIndexDefinition Is Nothing Then
                                        anIndexDefinition = IndexDefinition.Create(tablename:=viewid, indexname:=aTable.ID & "." & aTable.PrimaryKey, runTimeOnly:=True)
                                    End If

                                    Dim thecolumnnames As New List(Of String)
                                    For Each aColumnname In anIndex.Columnnames
                                        thecolumnnames.Add(aTable.ID & "." & aColumnname)
                                    Next
                                    anIndexDefinition.Columnnames = thecolumnnames.ToArray
                                    anIndexDefinition.IsPrimary = False
                                    anIndexDefinition.Version = anIndex.Version
                                    anIndexDefinition.Description = Description
                                    anIndexDefinition.IsUnique = True
                                    ' CurrentDBDriver.GetIndex(aView, anIndexDefinition, createOrAlter:=True)
                                End If
                            Next
                        Next
                    Else
                        Return False
                    End If
                Else
                    CoreMessageHandler("failed to build view - not a releational database driver", argument:=sqlselectcmd.ToString, _
                                  procedure:="ObjectDefinition.AlterRetrieveViewSchema")
                    Return False
                End If

            Catch ex As Exception
                CoreMessageHandler("failed to build view", argument:=sqlselectcmd.ToString, exception:=ex, _
                                    procedure:="ObjectDefinition.AlterRetrieveViewSchema")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' fills a object definition by attributes from ObjectClassDescription
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetupByClassDescription(objecttype As System.Type, Optional runtimeOnly As Boolean = False) As Boolean
            If objecttype Is Nothing Then
                CoreMessageHandler(message:="failed : object type is nothing", _
                                  procedure:="objectdefinition.SetupByClassDescription(Shared)", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescription(type:=objecttype)
            Dim bootstrap As Boolean = runtimeOnly


            If anObjectDescription Is Nothing Then
                CoreMessageHandler(message:="object was not found by type", argument:=objecttype.Name, objectname:=objecttype.Name, _
                                  procedure:="objectdefinition.SetupByClassDescription(Shared)", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If


            '*** check on bootstrap
            If CurrentSession.IsBootstrappingInstallationRequested Then
                bootstrap = True
            Else
                bootstrap = runtimeOnly
            End If

            '* set the object description 
            Me.SetValuesBy(attribute:=anObjectDescription.ObjectAttribute)

            '* set the tables
            For Each aTableAttribute In anObjectDescription.TableAttributes
                Me.AddTable(attribute:=aTableAttribute, runtimeOnly:=runtimeOnly)
            Next
            '* add the entries
            For Each anEntryAttribute In anObjectDescription.ObjectEntryAttributes
                Me.AddEntry(attribute:=anEntryAttribute, runtimeOnly:=runtimeOnly)
            Next
            '* add foreign Keys
            '* -> done in table attributes setting

            '* set the table index
            For Each anIndexAttribute In anObjectDescription.IndexAttributes
                If anIndexAttribute.Enabled Then
                    If Not anIndexAttribute.HasValueTableID Then
                        If Me.Tablenames.Count = 1 Then
                            anIndexAttribute.TableID = Me.Tablenames.FirstOrDefault
                        Else
                            CoreMessageHandler(message:="ambiguous index attribute has no table name property and oject has more than one table - index not created", _
                                               objectname:=Me.ID, argument:=anIndexAttribute.IndexName, messagetype:=otCoreMessageType.InternalError, _
                                               procedure:="objectdefinition.SetupByClassDescription(Type)")
                            Exit For
                        End If
                    End If
                    If Me.HasTable(anIndexAttribute.TableID) Then
                        '** add Index to table definition
                        '** no runTimeOnly since the AddIndex is getting this from the table
                        Me.GetTable(anIndexAttribute.TableID).AddIndex(anIndexAttribute)
                    Else
                        CoreMessageHandler(message:="table name of index is not assigned to object definition - index not created", _
                                               objectname:=Me.ID, argument:=anIndexAttribute.IndexName, containerID:=anIndexAttribute.TableID, _
                                               messagetype:=otCoreMessageType.InternalError, _
                                               procedure:="objectdefinition.SetupByClassDescription(Type)")
                    End If
                End If
            Next

            '* set the permission rules
            For Each anAttribute In anObjectDescription.TransactionAttributes
                Me.AddPermissionRule(attribute:=anAttribute, runtimeOnly:=runtimeOnly)
            Next

            Return True
        End Function

        ''' <summary>
        ''' retrieve the List of Primary Key entry names
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetNoKeys() As UShort
            ' Nothing
            If Not Me.IsLoaded And Not Me.IsCreated Then
                Return 0
            End If

            Return _pkentrynames.Count
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key entry names
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function PrimaryKeyEntryNames() As String()
            If Not IsAlive(subname:="PrimaryKeyEntryNames") OrElse _pkentrynames.Count = 0 Then Return {}
            Return _pkentrynames
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetKeyEntries() As List(Of AbstractEntryDefinition)
            ' Nothing
            If Not IsAlive(subname:="getKeyEntries") Then Return New List(Of AbstractEntryDefinition)
            Dim aList As New List(Of AbstractEntryDefinition)
            For Each aName In Me.PrimaryKeyEntryNames
                If _objectentries.ContainsKey(aName) Then
                    aList.Add(_objectentries.Item(aName))
                Else
                    CoreMessageHandler(message:="key name of object is not in the entries dictionary", messagetype:=otCoreMessageType.InternalError, _
                                        procedure:="ObjectDefinition.GetKeyEntries", argument:=aName, objectname:=Me.ID)
                End If
            Next
            Return aList
        End Function

        ''' <summary>
        ''' event handler for the PropertyChanged Event of an Entry
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectDefinition_OnEntryChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
            If sender.GetType.Equals(GetType(ObjectContainerEntry)) Then
                Dim anEntry As ObjectContainerEntry = TryCast(sender, ObjectContainerEntry)
                If anEntry IsNot Nothing AndAlso e.PropertyName = ContainerEntryDefinition.ConstFNPrimaryKey Then
                    ''' HACK ! just add up the primary keys - neglect if deleted or primarykey ordinal in table 
                    ''' 
                    If anEntry.IsPrimaryKey Then
                        If Not _pkentrynames.Contains(anEntry.Entryname) Then
                            ReDim Preserve _pkentrynames(_pkentrynames.GetUpperBound(0) + 1)
                            _pkentrynames(_pkentrynames.GetUpperBound(0)) = anEntry.Entryname
                        End If
                    End If
                End If
            End If
        End Sub
        ''' <summary>
        ''' Add an Entry by Object Entry Definition
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntry(entry As iormObjectEntry) As Boolean
            If Not IsAlive(subname:="AddEntry") Then Return False
            ' remove and overwrite
            If _objectentries.ContainsKey(key:=entry.Entryname.ToUpper) Then
                CoreMessageHandler(message:="Warning ! entry already exists in Object Definition - will be replaced", objectname:=Me.ID, entryname:=entry.Entryname, _
                                    procedure:="ObjectDefinition.AddEntry", messagetype:=otCoreMessageType.InternalWarning)
                Call _objectentries.Remove(key:=entry.Entryname.ToUpper)
            End If
            '** check if Entry is primary and also a key of this object
            '** ---> CODE MOVED TO EVENT ONPROPERTYCHANGED
            'If entry.IsColumn AndAlso DirectCast(entry, ObjectColumnEntry).IsPrimaryKey Then
            '    If Not _pknames.Contains(entry.Entryname) Then
            '        ReDim Preserve _pknames(_pknames.GetUpperBound(0) + 1)
            '        _pknames(_pknames.GetUpperBound(0)) = entry.Entryname
            '    End If
            'End If
            ' register handler
            AddHandler TryCast(entry, ormBusinessObject).PropertyChanged, AddressOf ObjectDefinition_OnEntryChanged

            ' add entry
            _objectentries.Add(key:=entry.Entryname.ToUpper, value:=entry)
            '** synchronize the table names after object entry is added
            SynchronizeTables()

            ''' yes we have changed
            Me.IsChanged = True
            '
            Return True

        End Function


        ''' <summary>
        ''' gets an entry by entryname or nothing
        ''' </summary>
        ''' <param name="entryname">name of the entry</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasEntry(entryname As String, Optional isActive As Boolean = True) As Boolean
            If Not IsAlive(subname:="Hasentry") Then Return False
            If isActive Then
                If _objectentries.ContainsKey(key:=entryname.ToUpper) Then
                    Return _objectentries.Item(key:=entryname.ToUpper).IsActive
                Else
                    Return False
                End If
            Else
                Return _objectentries.ContainsKey(key:=entryname.ToUpper)
            End If

        End Function
        ''' <summary>
        ''' gets the Table Object for the tablename assosciated with this object definition
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTable(tablename As String) As ContainerDefinition
            If Not Me.IsAlive(subname:="ObjectDefinition.Gettable") Then Return Nothing
            If _tables.ContainsKey(key:=tablename.ToUpper) Then
                Return _tables.Item(key:=tablename.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns true if the tablename exists in the table dictionary
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasTable(tablename As String) As Boolean
            If Not Me.IsAlive(subname:="ObjectDefinition.hastable") Then Return Nothing
            Return _tables.ContainsKey(key:=tablename.ToUpper)
        End Function
        ''' <summary>
        ''' returns the Object Class Description for the Object Definition Instance
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetClassDescription() As ObjectClassDescription
            If Not IsAlive(subname:="GetClassDescription") Then Return Nothing
            Return ot.GetObjectClassDescription(Me.ID)
        End Function
        ''' <summary>
        ''' returns a list of relation Attributes defined in the class description
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRelationAttributes() As List(Of ormRelationAttribute)
            Dim aDescription As ObjectClassDescription = Me.GetClassDescription
            If aDescription Is Nothing Then Return New List(Of ormRelationAttribute)
            Return aDescription.RelationAttributes

        End Function
        ''' <summary>
        ''' returns a list of relation Attributes defined in the class description
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRelationAttribute(name As String) As ormRelationAttribute
            Dim aDescription As ObjectClassDescription = Me.GetClassDescription
            If aDescription Is Nothing Then Return Nothing

            Return aDescription.GetRelationAttribute(relationname:=name)
        End Function
        ''' <summary>
        ''' returns a list of relation Attributes defined in the class description
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetIndexAttribute(name As String) As ormIndexAttribute
            Dim aDescription As ObjectClassDescription = Me.GetClassDescription
            If aDescription Is Nothing Then Return Nothing

            Return aDescription.IndexAttributes.Select(Function(s) s.IndexName = name)
        End Function
        ''' <summary>
        ''' gets an entry by entryname or nothing
        ''' </summary>
        ''' <param name="entryname">name of the entry</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntry(entryname As String) As iormObjectEntry

            If Not Me.IsCreated And Not Me.IsLoaded Then
                Return Nothing
            End If

            If _objectentries.ContainsKey(key:=entryname.ToUpper) Then
                Return _objectentries.Item(key:=entryname.ToUpper)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' returns a list of all active object names
        ''' </summary>
        ''' <param name="tablename">the tablename</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function AllActiveObjectNames(Optional ByRef dbdriver As iormRelationalDatabaseDriver = Nothing, Optional domainid As String = Nothing) As List(Of String)

            Dim aCollection As New List(Of String)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormRelationalTableStore
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            Try
                If dbdriver Is Nothing Then
                    aStore = GetTableStore(ObjectDefinition.ConstPrimaryTableID)
                Else
                    aStore = dbdriver.GetTableStore(ObjectDefinition.ConstPrimaryTableID)
                End If

                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allObjects", addAllFields:=False)
                If Not aCommand.IsPrepared Then
                    aCommand.select = "DISTINCT " & ConstFNID
                    aCommand.Where = ConstFNIsDeleted & " = @deleted "
                    aCommand.Where = ConstFNISActive & " = @isactive "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@isactive", ColumnName:=ConstFNISActive, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@isactive", value:=True)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainid)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    If Not aCollection.Contains(aRecord.GetValue(1).toupper) Then
                        aCollection.Add(aRecord.GetValue(1).toupper)
                    End If
                Next

                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, procedure:="ObjectDefinition.AllActiveObjectnames")
                Return aCollection
            End Try

        End Function
        ''' <summary>
        ''' Retrieves an Object Definition from the persistence store
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="domainID"></param>
        ''' <param name="dbdriver"></param>
        ''' <param name="forceReload"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal objectname As String, _
                                        Optional domainid As String = Nothing, _
                                        Optional dbdriver As iormRelationalDatabaseDriver = Nothing, _
                                        Optional runtimeOnly As Boolean = False,
                                        Optional forceReload As Boolean = False) As ObjectDefinition
            Return RetrieveDataObject(Of ObjectDefinition)(pkArray:={objectname.ToUpper}, domainID:=domainid, dbdriver:=dbdriver, runtimeOnly:=runtimeOnly, forceReload:=forceReload)
        End Function

        ''' <summary>
        ''' handles the OnPersisted Event - used to persist the tables & permissions since these are dynamic and not relation mapped
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnPersisted(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnPersisted
            Dim myself = TryCast(e.DataObject, ObjectDefinition)
            If myself IsNot Nothing Then
                SynchronizeTables()
                '*** save the tables
                For Each aTable In myself.Tables
                    aTable.Persist(e.Timestamp)
                Next
                '*** save the permissions
                For Each aPermission In myself.PermissionRules
                    aPermission.Persist(e.Timestamp)
                Next
            End If

        End Sub

        ''' <summary>
        ''' little routine to synchronize tablenames (as stored foreign key in the database) and the runtime structure _tables and entries
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub SynchronizeTables()
            '** build from ground - no entries if no columnentry exists
            Dim theTablenamesList As New List(Of String)

            ''' add the tables dependend on the object entries
            ''' 
            For Each anEntry In _objectentries.Values
                If anEntry.IsColumn Then
                    Dim aColumnEntry = TryCast(anEntry, ObjectContainerEntry)
                    If aColumnEntry IsNot Nothing Then
                        If Not _tables.ContainsKey(aColumnEntry.ContainerID) Then
                            Dim aTable As ContainerDefinition = ContainerDefinition.Retrieve(tablename:=aColumnEntry.ContainerID, runtimeOnly:=Me.RunTimeOnly)
                            If aTable IsNot Nothing Then
                                _tables.Add(key:=aColumnEntry.ContainerID, value:=aTable)
                                If Not theTablenamesList.Contains(aColumnEntry.ContainerID) Then theTablenamesList.Add(aTable.ID)
                            End If
                        End If
                    End If
                End If
            Next

            ''' add the tables definied in the list but not elsethere (error condition ?!)
            ''' 
            For Each aName In theTablenamesList
                If Not _tables.ContainsKey(aName) Then
                    Dim aTable As ContainerDefinition = ContainerDefinition.Retrieve(tablename:=aName, runtimeOnly:=Me.RunTimeOnly)
                    If aTable IsNot Nothing Then
                        _tables.Add(key:=aName, value:=aTable)
                        If Not theTablenamesList.Contains(aName) Then theTablenamesList.Add(aTable.ID)
                    Else
                        theTablenamesList.Remove(aName)
                    End If
                End If
            Next

            ''' set the _tablenames
            ''' 
            _tablenames = theTablenamesList.ToArray
        End Sub
        ''' <summary>
        ''' handles the OnPersisted Event - used to persist the tables since these are dynamic and not relation mapped
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.OnInfused
            Dim myself = TryCast(e.DataObject, ObjectDefinition)
            If myself IsNot Nothing AndAlso Not myself.RunTimeOnly Then
                ''' overwrite the class to make sure this always fits to this backend version
                ''' 
                Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(TryCast(e.DataObject, ObjectDefinition).ID)
                If aDescription IsNot Nothing Then
                    TryCast(e.DataObject, ObjectDefinition).Classname = aDescription.ObjectAttribute.ClassName
                End If
                ''' infuse also the Object Permission
                ''' 
                Dim permissions = ObjectPermission.ByObjectName(TryCast(e.DataObject, ObjectDefinition).ID)
                For Each aPermission In permissions
                    Dim aSet As New SortedList(Of Long, ObjectPermission)
                    If _objectpermissions.ContainsKey(key:=aPermission.Operation) Then
                        aSet = _objectpermissions.Item(key:=aPermission.Operation)
                    Else
                        _objectpermissions.Add(key:=aPermission.Operation, value:=aSet)
                    End If
                    aSet.Add(key:=aPermission.Order, value:=aPermission)
                Next
                ''' infuse also the tables list
                ''' 
                SynchronizeTables()

                ''' switch on/off entries
                ''' 
                Dim anEntry As iormObjectEntry
                anEntry = Me.GetEntry(entryname:=Domain.ConstFNIsDomainIgnored)
                If anEntry IsNot Nothing Then anEntry.IsActive = Me.HasDomainBehavior
                anEntry = Me.GetEntry(entryname:=Domain.ConstFNDomainID)
                If anEntry IsNot Nothing And Me.ID <> Domain.ConstObjectID Then anEntry.IsActive = Me.HasDomainBehavior

                anEntry = Me.GetEntry(entryname:=ConstFNIsDeleted)
                If anEntry IsNot Nothing Then anEntry.IsActive = Me.HasDeleteFieldBehavior
                anEntry = Me.GetEntry(entryname:=ConstFNDeletedOn)
                If anEntry IsNot Nothing Then anEntry.IsActive = Me.HasDeleteFieldBehavior

                ''' Spare fields 
                ''' 
                If Me.HasSpareFieldsBehavior Then
                    For Each anEntry In Me.GetEntries
                        If anEntry.IsSpareField Then
                            anEntry.IsActive = Me.HasSpareFieldsBehavior
                        End If
                    Next
                End If
            End If

        End Sub
        ''' <summary>
        ''' creates an new object definition in the persistnce store
        ''' </summary>
        ''' <param name="objectID"></param>
        ''' <param name="domainID"></param>
        ''' <param name="runTimeOnly"></param>
        ''' <param name="checkunique"></param>
        ''' <param name="version"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal objectID As String, _
                                Optional domainid As String = Nothing,
                                Optional runTimeOnly As Boolean = False, _
                                Optional checkunique As Boolean = True, _
                                Optional version As UShort = 1) As ObjectDefinition

            Return ormBusinessObject.CreateDataObject(Of ObjectDefinition)({objectID.ToUpper}, domainID:=domainid, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
        End Function


        ''' <summary>
        ''' gets the permission for an user and a specified operation - returns true if permission is given
        ''' </summary>
        ''' <param name="user"></param>
        ''' <param name="operationname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEffectivePermission([user] As Commons.User, domainid As String, transactionname As String) As Boolean
            Dim result As Boolean = DefaultPermission
            Dim permissions As SortedList(Of Long, ObjectPermission)
            If _objectpermissions.ContainsKey(key:=transactionname.ToUpper) Then
                permissions = _objectpermissions.Item(key:=transactionname.ToUpper)

                '** check all rules of the permissions
                For Each permission As ObjectPermission In permissions.Values
                    Dim exitflag As Boolean
                    result = permission.CheckFor([user], exit:=exitflag)
                    If exitflag Then Return result
                Next

                Return result
            Else
                Return _defaultpermission
            End If

        End Function


        ''' <summary>
        ''' Returns a Query Enumeration
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function GetQuery(name As String, Optional domainid As String = Nothing) As iormQueriedEnumeration
            ''' function gets a queried enumeration mostly from the attribute unless we have no 
            ''' query objects in the core
            If Not Me.IsAlive(subname:="Objectdefinition.GetQuery") Then Return Nothing

            Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ID)
            If aDescription Is Nothing Then
                Call CoreMessageHandler(message:="data object class description cannot be retrieved", _
                                       objectname:=Me.Classname, argument:=name, _
                                       procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            Dim anObjectID As String = Me.ID
            Dim type As System.Type = System.Type.GetType(Me.Classname, throwOnError:=False, ignoreCase:=True)
            If type Is Nothing Then
                Call CoreMessageHandler(message:="type cannot be retrieved from reflection", _
                                           objectname:=Me.Classname, argument:=name, _
                                           procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            '** is a session running ?!
            'If Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
            '    Call CoreMessageHandler(message:="data object cannot be retrieved - start session to database first", _
            '                            objectname:=anObjectID, arg1:=name, _
            '                            subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.ApplicationError)
            '    Return Nothing
            'End If

            '** DOMAIN ID
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
            If Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainid, _
                                                                objecttransactions:={anObjectID & "." & ConstOPInject}) Then
                '** request authorizartion
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainid:=domainid, _
                                                                            username:=CurrentSession.CurrentUsername, _
                                                                            objecttransactions:={anObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                            objectname:=anObjectID, argument:=ConstOPInject, username:=CurrentSession.CurrentUsername, _
                                            procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If

            '** get the store for the primary table 
            Dim aStore As iormRelationalTableStore = Me.DatabaseDriver.GetTableStore(tableID:=aDescription.PrimaryContainerID)
            If aStore Is Nothing Then
                Call CoreMessageHandler(message:="table store cannot be retrieved", _
                                           objectname:=anObjectID, argument:=name, containerID:=aDescription.PrimaryContainerID, _
                                           procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Dim hasDomainBehavior As Boolean
            Dim hasDeleteBehavior As Boolean

            ''' this returns only a definition if it was previously loaded
            ''' 
            If CurrentSession.IsBootstrappingInstallationRequested _
              OrElse ot.GetBootStrapObjectClassnames.Contains(Me.Classname.ToUpper) Then
                hasDomainBehavior = Me.HasDomainBehavior
                hasDeleteBehavior = Me.HasDeleteFieldBehavior
            Else
                hasDomainBehavior = aDescription.ObjectAttribute.AddDomainBehavior
                hasDeleteBehavior = aDescription.ObjectAttribute.AddDeleteFieldBehavior
            End If

            ''' get the Select-Command
            Dim aSelectCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(name)

            ''' prepare the command with the specials
            ''' 
            If Not aSelectCommand.IsPrepared Then
                Dim aQryAttribute As ormObjectQueryAttribute = aDescription.GetQueryAttribute(name:=name)
                Dim where As String
                Dim orderby As String
                Dim fieldnames As New List(Of String)
                Dim addallfields As Boolean

                If aQryAttribute Is Nothing Then
                    Call CoreMessageHandler(message:="query attribute could not be retrieved", _
                                           objectname:=anObjectID, argument:=name, _
                                           procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    If aQryAttribute.HasValueWhere Then
                        where = aQryAttribute.Where
                    Else
                        where = String.empty
                    End If
                    If aQryAttribute.HasValueOrderBy Then
                        orderby = aQryAttribute.Orderby
                    Else
                        orderby = String.empty
                    End If
                    If aQryAttribute.HasValueAddAllFields Then addallfields = aQryAttribute.AddAllFields
                    If aQryAttribute.HasValueEntrynames Then
                        Call CoreMessageHandler(message:="retrieving entry names not yet implemented", _
                                         objectname:=anObjectID, argument:=name, _
                                         procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                End If


                Dim primaryTableID As String = aDescription.PrimaryContainerID

                ''' add tables
                ''' 
                aSelectCommand.AddTable(primaryTableID, addAllFields:=addallfields)

                ''' build domain behavior and deleteflag
                ''' 
                If hasDomainBehavior Then
                    ''' add where
                    If Not String.IsNullOrWhiteSpace(where) Then where &= " AND "
                    where &= String.Format(" ([{0}] = @{0} OR [{0}] = @Global{0})", ConstFNDomainID)
                    ''' add parameters
                    If aSelectCommand.Parameters.Find(Function(x)
                                                          Return x.ID.ToUpper = "@" & ConstFNDomainID.ToUpper
                                                      End Function) Is Nothing Then
                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tableid:=primaryTableID, value:=domainid)
                                       )
                    End If
                    If aSelectCommand.Parameters.Find(Function(x)
                                                          Return x.ID.ToUpper = "@Global" & ConstFNDomainID.ToUpper
                                                      End Function
                                      ) Is Nothing Then
                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@Global" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tableid:=primaryTableID, value:=ConstGlobalDomain)
                                       )
                    End If
                End If
                ''' delete 
                ''' 
                If hasDeleteBehavior Then
                    If Not String.IsNullOrWhiteSpace(where) Then where &= " AND "
                    where &= String.Format(" [{0}] = @{0}", ConstFNIsDeleted)
                    If aSelectCommand.Parameters.Find(Function(x)
                                                          Return x.ID.ToUpper = "@" & ConstFNIsDeleted.ToUpper
                                                      End Function
                                       ) Is Nothing Then

                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@" & ConstFNIsDeleted, columnname:=ConstFNIsDeleted, tableid:=primaryTableID, _
                                                                  value:=False)
                                       )
                    End If
                End If

                ''' set the parameters
                aSelectCommand.Where = where
                aSelectCommand.OrderBy = orderby

                If Not aSelectCommand.Prepare() Then
                    Call CoreMessageHandler(message:="the select command could not be prepared", _
                                          objectname:=anObjectID, argument:=name, _
                                          procedure:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

            End If

            ''' set the current domain parameters
            ''' 
            If hasDomainBehavior Then
                ''' add where
                ''' add parameters
                Dim aParameter As ormSqlCommandParameter = _
                    aSelectCommand.Parameters.Find(Function(x)
                                                       Return x.ID.ToUpper = "@" & ConstFNDomainID.ToUpper
                                                   End Function)
                If aParameter IsNot Nothing Then aParameter.Value = CurrentSession.CurrentDomainID
            End If


            ''' return a new Queries enumeration with the embedded command
            Dim aQE As ormSQLQueriedEnumeration = New ormSQLQueriedEnumeration(type:=type, command:=aSelectCommand, id:=Me.ID & "." & name)


            ''' further definitions
            ''' 

            ''' return the new queried Enumeration
            ''' 
            Return aQE
        End Function
    End Class


    ''' <summary>
    ''' abstract class for ObjectEntry (data slots) Definition 
    ''' Subclasses are ObjectColumnEntry and ObjecCompoundEntry
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=AbstractEntryDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Abstract ObjectEntry definition", _
        useCache:=True, AddDeletefieldBehavior:=True, AddDomainBehavior:=True, isbootstrap:=True, Version:=1)> _
    Public MustInherit Class AbstractEntryDefinition
        Inherits ormBusinessObject
        Implements iormRelationalPersistable, iormInfusable, iormObjectEntry, System.ComponentModel.INotifyPropertyChanged

        ''' <summary>
        ''' Object ID
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstObjectID = "ObjectEntry"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormTableAttribute(Version:=5, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Const ConstPrimaryTableID = "tblObjectEntries"

        ''' <summary>
        ''' Table Index Definitions
        ''' </summary>
        ''' <remarks></remarks>
        <ormIndex(ColumnName1:=ConstFNxid)> Public Const ConstIndexXID = "XID" ' not unqiue
        <ormIndex(columnName1:=ConstFNDomainID, ColumnName2:=ConstFNxid)> Public Const ConstIndDomain = "Domain"
        <ormIndex(columnname1:=ConstFNObjectName, columnname2:=ConstFNType, columnname3:=ConstFNIsDeleted, columnname4:=ConstFNEntryName)> Public Const constINDtypes = "indexTypes"

        ''' <summary>
        ''' Primary Key Columns
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, PrimaryEntryOrdinal:=1, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNObjectName As String = ObjectDefinition.ConstFNID

        <ormObjectEntry(dbdefaultvalue:="", Datatype:=otDataType.Text, size:=100, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        xid:="OED1", title:="Object Entry Name", Description:="entry (data slot) name of an Ontrack Object", PrimaryEntryOrdinal:=2)> _
        Public Const ConstFNEntryName As String = "entry"

        <ormObjectEntry(referenceobjectentry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, PrimaryEntryOrdinal:=3)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' Column Definitions
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormObjectEntry(Datatype:=otDataType.Text, defaultvalue:=otObjectEntryType.ContainerEntry, size:=50, _
                       properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                       xid:="OED3", title:="Entry Type", Description:="OTDB schema entry type")> Public Const ConstFNType As String = "typeid"


        <ormObjectEntry(defaultvalue:=otDataType.Text, dbdefaultvalue:="3", Datatype:=otDataType.Long, _
                        xid:="OED11", title:="Datatype", Description:="OTDB field data type")> Public Const ConstFNDatatype As String = "datatype"

        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, _
                        title:="Inner Datatype", Description:="OTDB inner list data type")> Public Const ConstFNInnerDatatype As String = "innertype"

        <ormObjectEntry(referenceObjectentry:=ContainerEntryDefinition.ConstObjectID & "." & ContainerEntryDefinition.ConstFNSize, _
                        xid:="OED13", Description:="max Length of the entry")> Public Const ConstFNSize As String = "size"

        <ormObjectEntry(Datatype:=otDataType.Long, defaultvalue:=1, dbdefaultvalue:="1", _
                         xid:="OED14", title:="Ordinal", Description:="ordinal of the object entry")> Public Const ConstFNordinal As String = "ordinal"

        <ormObjectEntry(referenceObjectentry:=ContainerEntryDefinition.ConstObjectID & "." & ContainerEntryDefinition.ConstFNIsNullable, _
                           xid:="OED15", Description:="is nullable on the object entry level")> Public Const ConstFNIsNullable As String = "isnullable"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
                        xid:="OED16", title:="default value", description:="default value of the object entry on the object level")> _
        Public Const ConstFNDefaultValue As Object = "defaultvalue"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, isnullable:=True, properties:={ObjectEntryProperty.Keyword}, _
                        xid:="OED21", title:="XChangeID", Description:="ID for XChange manager")> Public Const ConstFNxid As String = "XID"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, defaultvalue:="", properties:={ObjectEntryProperty.Capitalize, ObjectEntryProperty.Trim}, _
                        xid:="OED22", title:="Title", Description:="title for column headers of the field")> Public Const ConstFNTitle As String = "TITLE"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, defaultvalue:="", properties:={ObjectEntryProperty.Capitalize, ObjectEntryProperty.Trim}, _
                       xid:="OED25", title:="Title", Description:="category of the object entry")> Public Const ConstFNCategory As String = "CATEGORY"

        <ormObjectEntry(Datatype:=otDataType.Memo, properties:={ObjectEntryProperty.Trim}, isnullable:=True, _
                        xid:="OED23", title:="Description", Description:="Description of the field")> Public Const ConstFNDescription As String = "DESC"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, innerDatatype:=otDataType.Text, _
                        properties:={ObjectEntryProperty.Keyword}, _
                        xid:="OED24", title:="XChange alias ID", Description:="aliases ID for XChange manager")> Public Const ConstFNalias As String = "alias"



        <ormObjectEntry(Datatype:=otDataType.List, innerDatatype:=otDataType.Text, isnullable:=True, _
                        xid:="OED17", title:="Properties", Description:="properties and property functions for the entry")> _
        Public Const ConstFNProperties As String = "properties"

        <ormObjectEntry(defaultvalue:=1, Datatype:=otDataType.[Long], lowerrange:=0, _
                        title:="UpdateCount", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "updc"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
                        xid:="OED18", title:="Read Only", Description:="set if the object entry is created internally and can not be changed")> _
        Public Const ConstFNReadonly As String = "readonly"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=True, _
                        xid:="OED19", title:="Is Active", Description:="set if the object entry is activated")> _
        Public Const ConstFNActive As String = "active"

        <ormObjectEntry(Datatype:=otDataType.List, innerDatatype:=otDataType.Text, isnullable:=True, title:="Relation", Description:="relation information")> _
        Public Const ConstFNRelation As String = "relation"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=True, dbdefaultvalue:="0", _
                        xid:="OED31", title:="Validate Entry", Description:="set if the object entry will be validated")> _
        Public Const ConstFNValidate As String = "validate"

        <ormObjectEntry(Datatype:=otDataType.List, innerDatatype:=otDataType.Text, isnullable:=True, _
                        xid:="OED32", title:="List of Values", Description:="list of possible values")> Public Const ConstFNValues As String = "values"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, _
          xid:="OED33", title:="Lookup Properties", Description:="list of lookup properties")> Public Const ConstFNLookupProperties As String = "lproperties"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, _
           xid:="OED34", title:="Dynamic Lookup Condition", Description:="lookup condition of possible values")> Public Const ConstFNLookup As String = "lookup"

        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
            xid:="OED35", title:="Lower Range", Description:="lower range value")> Public Const ConstFNLowerRange As String = "lower"

        <ormObjectEntry(Datatype:=otDataType.Long, isnullable:=True, _
           xid:="OED36", title:="Upper Range", Description:="upper range value")> Public Const ConstFNUpperRange As String = "upper"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, _
            xid:="OED37", title:="Validation Properties", Description:="list of validation properties")> Public Const ConstFNValidationProperties As String = "vproperties"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
           xid:="OED38", title:="Validation Regex Condition", Description:="regex match for validation to be true")> Public Const ConstFNValidationRegex As String = "validregex"


        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
          xid:="OED41", title:="Render Entry", Description:="set if the object entry will be rendered to a string presentation")> _
        Public Const ConstFNRender As String = "render"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, _
           xid:="OED42", title:="Render Properties", Description:="list of render properties")> Public Const ConstFNRenderProperties As String = "rproperties"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
            xid:="OED43", title:="Render Regex Condition", Description:="regex match for render to be true")> Public Const ConstFNRenderRegexMatch As String = "renderregexmatch"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
           xid:="OED44", title:="Render Regex Replace", Description:="regex replace pattern for rendering")> Public Const ConstFNRenderRegexPattern As String = "renderregexreplace"

        ''' <summary>
        ''' Member Mapping of persistable Columns
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntryMapping(EntryName:=ConstFNxid)> Protected _xid As String 'nullable
        <ormObjectEntryMapping(EntryName:=ConstFNObjectName)> Protected _objectname As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNDatatype)> Protected _datatype As otDataType = 0
        <ormObjectEntryMapping(EntryName:=ConstFNInnerDatatype)> Protected _innerdatatype As otDataType = 0
        <ormObjectEntryMapping(EntryName:=ConstFNSize)> Protected _size As Long?
        <ormObjectEntryMapping(EntryName:=ConstFNordinal)> Protected _ordinal As Long = 0
        <ormObjectEntryMapping(EntryName:=ConstFNReadonly)> Protected _readonly As Boolean
        <ormObjectEntryMapping(EntryName:=ConstFNActive)> Protected _active As Boolean
        <ormObjectEntryMapping(EntryName:=ConstFNIsNullable)> Protected _isnullable As Boolean
        <ormObjectEntryMapping(EntryName:=ConstFNDefaultValue)> Protected _defaultvalue As Object
        <ormObjectEntryMapping(EntryName:=ConstFNEntryName)> Protected _entryname As String = String.empty
        <ormObjectEntryMapping(EntryName:=ConstFNRelation)> Protected _Relation As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNProperties)> Protected _propertystrings() As String
        <ormObjectEntryMapping(EntryName:=ConstFNalias)> Protected _aliases As String() = {}
        <ormObjectEntryMapping(EntryName:=ConstFNTitle)> Protected _title As String
        <ormObjectEntryMapping(EntryName:=ConstFNCategory)> Protected _category As String
        <ormObjectEntryMapping(EntryName:=ConstFNUPDC)> Protected _version As Long = 0
        <ormObjectEntryMapping(EntryName:=ConstFNDescription)> Protected _Description As String
        <ormObjectEntryMapping(Entryname:=ConstFNType)> Protected _typeid As otObjectEntryType
        <ormObjectEntryMapping(entryname:=ConstFNValidate)> Protected _validate As Boolean = False
        <ormObjectEntryMapping(entryname:=ConstFNRender)> Protected _render As Boolean = False
        <ormObjectEntryMapping(entryname:=ConstFNValues)> Protected _listOfValues As List(Of String) = New List(Of String)
        <ormObjectEntryMapping(entryname:=ConstFNLookupProperties)> Protected _LookupPropertyStrings As String()
        <ormObjectEntryMapping(entryname:=ConstFNLookup)> Protected _lookupcondition As String
        <ormObjectEntryMapping(entryname:=ConstFNLowerRange)> Protected _lowerRangeValue As Long?
        <ormObjectEntryMapping(entryname:=ConstFNUpperRange)> Protected _upperRangeValue As Long?
        <ormObjectEntryMapping(entryname:=ConstFNRenderRegexMatch)> Protected _renderRegexMatch As String
        <ormObjectEntryMapping(entryname:=ConstFNRenderRegexPattern)> Protected _renderRegexPattern As String
        <ormObjectEntryMapping(entryname:=ConstFNValidationRegex)> Protected _validateRegexMatch As String
        <ormObjectEntryMapping(entryname:=ConstFNValidationProperties)> Protected _validationPropertyStrings As String()
        <ormObjectEntryMapping(entryname:=ConstFNRenderProperties)> Protected _renderPropertyStrings As String()

        '** events
        'Public Shadows Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)

        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

        ''' <summary>
        ''' dynamic members
        ''' </summary>
        ''' <remarks></remarks>
        Private _properties As New List(Of ObjectEntryProperty)
        Private _renderProperties As New List(Of RenderProperty)
        Private _runTimeOnly As Boolean = False 'dynmaic
        Private _validateProperties As New List(Of ObjectValidationProperty)
        Private _lookupProperties As New List(Of LookupProperty)
        Protected _myobjectDefintion As ObjectDefinition 'leads to loops if loaded on infused

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(Optional runtimeonly As Boolean = False, Optional objectid As String = Nothing)
            MyBase.New(runtimeonly:=runtimeonly, objectID:=objectid)
            registerHandler()
        End Sub

#Region "Properties"
        ''' <summary>
        ''' returns True if object entry is mapped to a field member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsMapped As Boolean Implements iormObjectEntry.IsMapped
            Get
                Dim aDescription = ot.GetObjectClassDescriptionByID(Me.Objectname)
                If aDescription IsNot Nothing Then
                    If aDescription.GetEntryFieldInfos(entryname:=Me.Entryname).Count > 0 Then Return True
                End If
                Return False
            End Get
            Set(value As Boolean)
                Throw New InvalidOperationException
            End Set

        End Property
        ''' <summary>
        ''' gets or sets the readonly flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [IsReadonly] As Boolean Implements iormObjectEntry.IsReadonly
            Get
                Return _readonly
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNReadonly, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the active flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsActive As Boolean Implements iormObjectEntry.IsActive
            Get
                Return _active
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNActive, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the validation flag - object takes part in validation if true
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsValidating As Boolean Implements iormObjectEntry.IsValidating
            Get
                Return _validate
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNValidate, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the render flag - object takes part in rendering if true
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsRendering As Boolean Implements iormObjectEntry.IsRendering
            Get
                Return _render
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNRender, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' True if ObjectEntry has a defined lower value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasLowerRangeValue As Boolean Implements iormObjectEntry.HasLowerRangeValue
            Get
                If Not Me.IsAlive(subname:="HasLowerRangeValue") Then Return False
                Return _lowerRangeValue.HasValue
            End Get
        End Property
        ''' <summary>
        ''' gets the lower range Value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LowerRangeValue As Long? Implements iormObjectEntry.LowerRangeValue
            Get
                Return _lowerRangeValue
            End Get
            Set(value As Long?)
                SetValue(entryname:=ConstFNLowerRange, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' True if ObjectEntry has a defined upper value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasUpperRangeValue As Boolean Implements iormObjectEntry.HasUpperRangeValue
            Get
                Return _upperRangeValue.HasValue
            End Get
        End Property
        ''' <summary>
        ''' gets the upper range Value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UpperRangeValue As Long? Implements iormObjectEntry.UpperRangeValue
            Get
                Return _upperRangeValue
            End Get
            Set(value As Long?)
                SetValue(entryname:=ConstFNUpperRange, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there are possible values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasPossibleValues As Boolean Implements iormObjectEntry.HasPossibleValues
            Get
                If Not Me.IsAlive(subname:="HasPossibleValues") Then Return False
                Return (_listOfValues IsNot Nothing AndAlso _listOfValues.Count > 0)
            End Get
        End Property
        ''' <summary>
        ''' gets the list of possible values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PossibleValues As List(Of String) Implements iormObjectEntry.PossibleValues
            Get
                Return _listOfValues
            End Get
            Set(value As List(Of String))
                SetValue(entryname:=ConstFNValues, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there are validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasValidationProperties As Boolean Implements iormObjectEntry.HasValidationProperties
            Get
                If Not Me.IsAlive(subname:="HasValidationProperties") Then Return False
                Return (_validateProperties IsNot Nothing AndAlso _validateProperties.Count > 0)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Validationproperties As List(Of ObjectValidationProperty) Implements iormObjectEntry.ValidationProperties
            Get
                Return _validateProperties
            End Get
            Set(value As List(Of ObjectValidationProperty))
                Dim aPropertyString As New List(Of String)
                For Each aP In value
                    aPropertyString.Add(aP.ToString)
                Next
                If SetValue(entryname:=ConstFNValidationProperties, value:=aPropertyString.ToArray) Then
                    _validateProperties = value
                End If

            End Set
        End Property
        Public Property ValidationPropertyStrings As String() Implements iormObjectEntry.ValidationPropertyStrings
            Get
                If _validationPropertyStrings Is Nothing Then Return {}
                Return _validationPropertyStrings
            End Get
            Set(value As String())
                SetValue(ConstFNValidationProperties, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there is a regular expression condition for validating the object value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasValidateRegExpression As Boolean Implements iormObjectEntry.HasValidateRegExpression
            Get
                If Not Me.IsAlive(subname:="HasValidateRegExpression") Then Return False
                Return (_validateRegexMatch IsNot Nothing AndAlso _validateRegexMatch <> String.empty)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the regular expression condition for validating the object value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ValidateRegExpression As String Implements iormObjectEntry.ValidateRegExpression
            Get
                Return _validateRegexMatch
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNValidationRegex, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there are validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasRenderProperties As Boolean Implements iormObjectEntry.HasRenderProperties
            Get
                If Not Me.IsAlive(subname:="HasRenderProperties") Then Return False
                Return (_renderProperties IsNot Nothing AndAlso _renderProperties.Count > 0)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RenderProperties As List(Of RenderProperty) Implements iormObjectEntry.RenderProperties
            Get
                Return _renderProperties
            End Get
            Set(value As List(Of RenderProperty))
                Dim aPropertyString As New List(Of String)
                For Each aP In value
                    aPropertyString.Add(aP.ToString)
                Next
                If SetValue(entryname:=ConstFNRenderProperties, value:=aPropertyString.ToArray) Then
                    _renderProperties = value
                End If
            End Set
        End Property
        Public Property RenderPropertyStrings As String() Implements iormObjectEntry.RenderPropertyStrings
            Get
                If _renderPropertyStrings Is Nothing Then Return {}
                Return _renderPropertyStrings
            End Get
            Set(value As String())
                SetValue(ConstFNRenderProperties, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there is a regular expression condition for rendering the object value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasRenderRegExpression As Boolean Implements iormObjectEntry.HasRenderRegExpression
            Get
                If Not Me.IsAlive(subname:="HasRenderRegExpression") Then Return False
                Return (_lookupcondition IsNot Nothing AndAlso _lookupcondition <> String.empty)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the regular expression condition for validating the object value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RenderRegExpMatch As String Implements iormObjectEntry.RenderRegExpMatch
            Get
                Return _renderRegexMatch
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNRenderRegexMatch, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the regular expression condition for validating the object value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RenderRegExpPattern As String Implements iormObjectEntry.RenderRegExpPattern
            Get
                Return _renderRegexPattern
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNRenderRegexPattern, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there are validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasLookupProperties As Boolean Implements iormObjectEntry.HasLookupProperties
            Get
                If Not Me.IsAlive(subname:="HasLookupProperties") Then Return False
                Return (_lookupProperties IsNot Nothing AndAlso _lookupProperties.Count > 0)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the validation properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LookupProperties As List(Of LookupProperty) Implements iormObjectEntry.LookupProperties
            Get
                Return _lookupProperties
            End Get
            Set(value As List(Of LookupProperty))
                Dim aPropertyString As New List(Of String)
                For Each aP In value
                    aPropertyString.Add(aP.ToString)
                Next
                If SetValue(entryname:=ConstFNLookupProperties, value:=aPropertyString.ToArray) Then
                    _lookupProperties = value
                End If

            End Set
        End Property
        Public Property LookupPropertyStrings As String() Implements iormObjectEntry.LookupPropertyStrings
            Get
                If _LookupPropertyStrings Is Nothing Then Return {}
                Return _LookupPropertyStrings
            End Get
            Set(value As String())
                SetValue(ConstFNLookupProperties, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if there are lookup condition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HasLookupCondition As Boolean Implements iormObjectEntry.HasLookupCondition
            Get
                If Not Me.IsAlive(subname:="HasLookupValues") Then Return False
                Return (_lookupcondition IsNot Nothing AndAlso _lookupcondition <> String.empty)
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the lookup condition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LookupCondition As String Implements iormObjectEntry.LookupCondition
            Get
                Return _lookupcondition
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNLookup, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Overridable Property Description As String Implements iormObjectEntry.Description
            Get
                Return Me._Description
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDescription, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the nullable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Property IsNullable As Boolean Implements iormObjectEntry.IsNullable
        ''' <summary>
        ''' gets or sets the size
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Property Size As Long? Implements iormObjectEntry.Size
        ''' <summary>
        ''' gets or sets the datatype
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Property Datatype As otDataType Implements iormObjectEntry.Datatype
        ''' <summary>
        ''' gets or sets the inner datatype
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property InnerDatatype As otDataType? Implements iormObjectEntry.InnerDatatype
            Get
                Return _innerdatatype
            End Get
            Set(value As otDataType?)
                SetValue(entryname:=ConstFNInnerDatatype, value:=value)
            End Set
        End Property
        '''' <summary>
        '''' gets the default value on the object level
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        Public Overridable Property Defaultvalue As Object Implements iormObjectEntry.DefaultValue
            Get
                If Not _isnullable AndAlso _defaultvalue Is Nothing Then
                    Return ot.GetDefaultValue(_datatype)
                Else
                    If _defaultvalue IsNot Nothing Then

                        Try

                            ''' check on enumerations and transform to it
                            Dim aMappingList = ot.GetObjectClassDescriptionByID(Me.Objectname).GetEntryFieldInfos(entryname:=Me.Entryname)
                            For Each aMapping In aMappingList
                                If aMapping.FieldType.IsEnum Then
                                    '* transform
                                    Dim anewValue = CTypeDynamic([Enum].Parse(aMapping.FieldType, _defaultvalue.ToString, ignoreCase:=True), aMapping.FieldType)
                                    Return anewValue
                                ElseIf Reflector.IsNullable(aMapping.FieldType) AndAlso Nullable.GetUnderlyingType(aMapping.FieldType).IsEnum Then
                                    '* transform
                                    Dim anewValue = CTypeDynamic([Enum].Parse(Nullable.GetUnderlyingType(aMapping.FieldType), _defaultvalue.ToString, ignoreCase:=True), Nullable.GetUnderlyingType(aMapping.FieldType))
                                    Return anewValue
                                End If
                            Next
                            ''' normal conversion
                            Dim aValue As Object = Converter.Object2otObject(_defaultvalue.ToString, _datatype)
                            aValue = CTypeDynamic(_defaultvalue, ot.GetDatatypeMappingOf(_datatype))
                            Return aValue
                        Catch ex As Exception
                            CoreMessageHandler(message:="CTypeDynmaic failed on default value for type " & _datatype.ToString, argument:=_defaultvalue, procedure:="AbstractEntryDefinition.DefaultValue", messagetype:=otCoreMessageType.InternalError, _
                                               objectname:=Me.Objectname)
                            Return ot.GetDefaultValue(_datatype)
                        End Try

                    Else
                        Return Nothing
                    End If
                End If
            End Get
            Set(value As Object)
                SetValue(entryname:=ConstFNDefaultValue, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the nullable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Property PrimaryKeyOrdinal As Long Implements iormObjectEntry.PrimaryKeyOrdinal
        ''' <summary>
        ''' gets or sets the nullable
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <summary>
        ''' returns the Position Ordinal in the record 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Ordinal As Long Implements iormObjectEntry.Ordinal
            Get
                Return _ordinal
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNordinal, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the object name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Objectname As String Implements iormObjectEntry.Objectname
            Get
                Return _objectname
            End Get
            Set(value As String)
                Throw New InvalidOperationException
            End Set
        End Property

        ''' <summary>
        ''' Object cannot be persisted only.
        ''' </summary>
        ''' <value>The run tim only.</value>
        Public ReadOnly Property RunTimeOnly As Boolean
            Get
                Return Me._runTimeOnly
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the XchangeManager ID for the field 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property XID As String Implements iormObjectEntry.XID
            Get
                XID = _xid
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNxid, value:=value)
            End Set

        End Property
        ''' <summary>
        '''  gets the name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Entryname As String Implements iormObjectEntry.Entryname
            Get
                Return _entryname
            End Get
            Set(value As String)
                Throw New InvalidOperationException("not allowed to set Entryname")
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the type OTDBSchemaDefTableEntryType of the field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Typeid As otObjectEntryType Implements iormObjectEntry.Typeid
            Get
                Return Me._typeid

            End Get
            Protected Set(value As otObjectEntryType)
                SetValue(entryname:=ConstFNType, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets true if this field is a spare field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public MustOverride Property IsSpareField() As Boolean Implements iormObjectEntry.IsSpareField


        ''' <summary>
        ''' gets true if a column / field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsColumn() As Boolean Implements iormObjectEntry.IsColumn
            Get
                If _typeid = otObjectEntryType.ContainerEntry Then IsColumn = True
            End Get
            Set(value As Boolean)
                CoreMessageHandler(message:="Property IsField is not changeable", procedure:="ObjectEntryDefinition.IsField", messagetype:=otCoreMessageType.InternalError, objectname:=Me.Objectname)
            End Set
        End Property
        ''' <summary>
        ''' returns true if entry is a compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property IsCompound() As Boolean Implements iormObjectEntry.IsCompound
            Get
                If _typeid = otObjectEntryType.Compound Then IsCompound = True
            End Get
            Set(value As Boolean)
                CoreMessageHandler(message:="Property isCompound is not changeable", procedure:="ObjectEntryDefinition.isCompound", messagetype:=otCoreMessageType.InternalError, objectname:=Me.Objectname)
            End Set
        End Property
        ''' <summary>
        ''' returns version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version() As Long Implements iormObjectEntry.Version
            Get
                Return _version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' returns a array of aliases
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Aliases() As String() Implements iormObjectEntry.Aliases
            Get
                Return _aliases
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNalias, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the relation information of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Relation() As String()
            Get
                If _Relation Is Nothing Then Return {}
                Return _Relation
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNRelation, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the Properties for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Properties As List(Of ObjectEntryProperty) Implements iormObjectEntry.Properties
            Get
                Return _properties
            End Get
            Set(value As List(Of ObjectEntryProperty))
                Dim aPropertyString As New List(Of String)
                For Each aP In value
                    aPropertyString.Add(aP.ToString)
                Next
                If SetValue(entryname:=ConstFNProperties, value:=aPropertyString.ToArray) Then
                    _properties = value
                End If
            End Set
        End Property
        Public Property PropertyStrings As String() Implements iormObjectEntry.PropertyStrings
            Get
                Return _propertystrings
            End Get
            Set(value As String())
                SetValue(ConstFNProperties, value)
            End Set
        End Property
        ''' <summary>
        ''' returns Title (Column Header)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title() As String Implements iormObjectEntry.Title
            Get
                Return _title
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNTitle, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns Title (Column Header)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Category() As String Implements iormObjectEntry.Category
            Get
                Return _category
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNCategory, value:=value)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' register all Event Handlers
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub RegisterHandler()
            AddHandler ormBusinessObject.OnCreated, AddressOf Me.AbstractEntryDefinition_OnCreated
            AddHandler ormBusinessObject.OnCreating, AddressOf Me.AbstractEntryDefinition_OnCreating
            AddHandler ormBusinessObject.OnInfused, AddressOf Me.AbstractEntryDefinition_OnInfused
            AddHandler ormBusinessObject.OnEntryChanged, AddressOf Me.AbstractEntryDefinition_OnEntryChanged
        End Sub
        ''' <summary>
        ''' deregister Event Handlers
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub DeRegisterHandler()
            RemoveHandler ormBusinessObject.OnCreated, AddressOf Me.AbstractEntryDefinition_OnCreated
            RemoveHandler ormBusinessObject.OnCreating, AddressOf Me.AbstractEntryDefinition_OnCreating
            RemoveHandler ormBusinessObject.OnInfused, AddressOf Me.AbstractEntryDefinition_OnInfused
            RemoveHandler ormBusinessObject.OnEntryChanged, AddressOf Me.AbstractEntryDefinition_OnEntryChanged
        End Sub
        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectEntry.OnswitchRuntimeOff

        ''' <summary>
        ''' Increase the version
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IncVersion() As Long
            _version = _version + 1
            IncVersion = _version
        End Function
        ''' <summary>
        ''' returns the ObjectDefintion of this Entry
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectDefinition() As ObjectDefinition Implements iormObjectEntry.GetObjectDefinition
            If _myobjectDefintion Is Nothing Then
                _myobjectDefintion = CurrentSession.Objects.GetObject(objectid:=Me.Objectname)
            End If
            Return _myobjectDefintion
        End Function
        ''' <summary>
        ''' set the properties by a attribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean Implements iormObjectEntry.SetByAttribute
            If Not IsAlive(subname:="SetByAttribute") Then Return False


            With attribute

                '** Slot Entry Properties
                If .HasValueXID Then Me.XID = .XID

                If .HasValueIsReadonly Then Me.IsReadonly = .IsReadOnly
                If .HasValueIsActive Then Me.IsActive = .IsActive
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueDataType Then Me.Datatype = .Datatype
                If .HasValueInnerDataType Then Me.InnerDatatype = .InnerDatatype
                If .hasValuePosOrdinal Then Me.Ordinal = .Posordinal
                If .HasValueSize Then Me.Size = .Size
                If .HasValueDefaultValue Then Me.Defaultvalue = .DefaultValue
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryEntryOrdinal
                If .HasValueTitle Then Me.Title = .Title
                If .HasValueCategory Then Me.Category = .Category
                If .HasValueAliases Then Me.Aliases = .Aliases
                If .HasValueVersion Then Me.Version = .Version

                If .HasValueRelation Then Me.Relation = .Relation
                ' properties
                If .HasValueObjectEntryProperties Then
                    Me.Properties = .ObjectEntryProperties.ToList
                End If
                ' render
                If .HasValueRender Then Me.IsRendering = .Render
                If .HasValueRenderProperties Then Me.RenderProperties = .RenderProperties.ToList
                If .HasValueRenderRegExpMatch Then Me.RenderRegExpMatch = .RenderRegExpMatch
                If .HasValueRenderRegExpPattern Then Me.RenderRegExpPattern = .RenderRegExpPattern
                ' validate
                If .HasValueValidate Then Me.IsValidating = .Validate
                If .HasValueLowerRange Then Me.LowerRangeValue = .LowerRange
                If .HasValueUpperRange Then Me.UpperRangeValue = .UpperRange
                If .HasValueValidationProperties Then Me.Validationproperties = .ValidationProperties.ToList
                If .HasValueLookupProperties Then Me.LookupProperties = .LookupProperties.ToList
                If .HasValueLookupCondition Then Me.LookupCondition = .LookupCondition
                If .HasValueValues Then Me.PossibleValues = .Values.ToList


            End With

            Return True
        End Function

        ''' <summary>
        ''' handler for OnCreated
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub AbstractEntryDefinition_OnCreated(sender As Object, e As ormDataObjectEventArgs)
            Dim myself As AbstractEntryDefinition = TryCast(e.DataObject, AbstractEntryDefinition)

            If myself IsNot Nothing Then
                Dim anObjectDefintion = CurrentSession.Objects.GetObject(objectid:=myself.Objectname, runtimeOnly:=myself.RunTimeOnly)
                If anObjectDefintion Is Nothing Then
                    CoreMessageHandler(message:="Object entry must be bound to an existing object definition", argument:=myself.Objectname, _
                                       procedure:="AbstractEntryDefinition_OnCreating", objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
                    e.AbortOperation = True
                Else
                    ''' add it to the object definition
                    anObjectDefintion.AddEntry(Me)
                End If
            End If
        End Sub

        ''' <summary>
        ''' handler for onCreating 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub AbstractEntryDefinition_OnCreating(sender As Object, e As ormDataObjectEventArgs)
            Dim myself As AbstractEntryDefinition = TryCast(e.DataObject, AbstractEntryDefinition)

            If myself IsNot Nothing Then
                Dim anObjectDefintion = CurrentSession.Objects.GetObject(objectid:=e.Record.GetValue(ConstFNObjectName), runtimeOnly:=myself.RunTimeOnly)
                If anObjectDefintion Is Nothing Then
                    CoreMessageHandler(message:="Object entry must be bound to an existing object definition", argument:=e.Record.GetValue(ConstFNObjectName), _
                                       procedure:="AbstractEntryDefinition_OnCreating", objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
                    e.AbortOperation = True
                End If
            End If
        End Sub

        ''' <summary>
        ''' handler for entry changed event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub AbstractEntryDefinition_OnEntryChanged(sender As Object, e As ormDataObjectEntryEventArgs)

            Try
                If e.ObjectEntryName.ToUpper = ConstFNProperties.ToUpper AndAlso _propertystrings IsNot Nothing Then
                    '** the property list in Object presentation
                    Dim aList As New List(Of ObjectEntryProperty)
                    For Each propstring In _propertystrings
                        Try
                            Dim aProperty As ObjectEntryProperty = New ObjectEntryProperty(propstring)
                            aList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _properties = aList ' assign


                ElseIf e.ObjectEntryName.ToUpper = ConstFNValidationProperties.ToUpper AndAlso _validationPropertyStrings IsNot Nothing Then

                    '** the property list in Object presentation
                    Dim aValidationList As New List(Of ObjectValidationProperty)
                    For Each propstring In _validationPropertyStrings
                        Try
                            Dim aProperty As ObjectValidationProperty = New ObjectValidationProperty(propstring)
                            aValidationList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _validateProperties = aValidationList ' assign


                ElseIf e.ObjectEntryName.ToUpper = ConstFNRenderProperties.ToUpper AndAlso _renderPropertyStrings IsNot Nothing Then

                    '** the property list in Object presentation
                    Dim aRenderList As New List(Of RenderProperty)
                    For Each propstring In _renderPropertyStrings
                        Try
                            Dim aProperty As RenderProperty = New RenderProperty(propstring)
                            aRenderList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _renderProperties = aRenderList ' assign

                ElseIf e.ObjectEntryName.ToUpper = ConstFNLookupProperties.ToUpper AndAlso _LookupPropertyStrings IsNot Nothing Then
                    '** the property list in Object presentation
                    Dim aLookupList As New List(Of LookupProperty)
                    For Each propstring In _LookupPropertyStrings
                        Try
                            Dim aProperty As LookupProperty = New LookupProperty(propstring)
                            aLookupList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _lookupProperties = aLookupList ' assign
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, procedure:="AbstractEntryDefinition_OnEntryChanged")
            End Try

        End Sub
        ''' <summary>
        ''' infuses the object from a record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub AbstractEntryDefinition_OnInfused(sender As Object, e As ormDataObjectEventArgs)

            Try

                ' this is not working - it brings us in an endless loop since the objectdefinition is not in the repository nor in the cache
                ' an while loading the relations such as EntryDefinitions we land here again

                'If _objectDefintion Is Nothing OrElse _objectDefintion.ID <> _objectname Then
                '    _objectDefintion = OnTrack.Database.ObjectDefinition.Retrieve(objectname:=_objectname, runtimeOnly:=_runTimeOnly)
                '    If _objectDefintion Is Nothing Then
                '        CoreMessageHandler(message:="Object entry must be bound to an existing object definition", arg1:=_objectname, _
                '                           subname:="AbstractEntryDefinition_OnInfused", objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
                '    End If
                'End If

                '''
                ''' setvalue and events are not called during infusion
                '''


                ''** the property list in Object presentation
                If _propertystrings IsNot Nothing Then
                    Dim aList As New List(Of ObjectEntryProperty)
                    For Each propstring In _propertystrings
                        Try
                            Dim aProperty As ObjectEntryProperty = New ObjectEntryProperty(propstring)
                            aList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _properties = aList ' assign
                End If


                ''** the property list in Object presentation

                If _validationPropertyStrings IsNot Nothing Then
                    Dim aValidationList As New List(Of ObjectValidationProperty)
                    For Each propstring In _validationPropertyStrings
                        Try
                            Dim aProperty As ObjectValidationProperty = New ObjectValidationProperty(propstring)
                            aValidationList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _validateProperties = aValidationList ' assign
                End If

                ''** the property list in Object presentation
                If _renderPropertyStrings IsNot Nothing Then
                    Dim aRenderList As New List(Of RenderProperty)
                    For Each propstring In _renderPropertyStrings
                        Try
                            Dim aProperty As RenderProperty = New RenderProperty(propstring)
                            aRenderList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _renderProperties = aRenderList ' assign
                End If

                ''** the property list in Object presentation
                If _LookupPropertyStrings IsNot Nothing Then
                    Dim aLookupList As New List(Of LookupProperty)
                    For Each propstring In _LookupPropertyStrings
                        Try
                            Dim aProperty As LookupProperty = New LookupProperty(propstring)
                            aLookupList.Add(aProperty)
                        Catch ex As Exception
                            Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _lookupProperties = aLookupList ' assign

                End If

            Catch ex As Exception
                Call CoreMessageHandler(procedure:="AbstractEntryDefinition_OnInfused", exception:=ex)
            End Try

        End Sub
        ''' <summary>
        ''' returns a Dictionary of Entryname - list of objectnames
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetEntryReference(Optional domainid As String = Nothing) As Dictionary(Of String, List(Of String))

            Dim aStore = TryCast(CurrentDBDriver.RetrieveContainerStore(ConstPrimaryTableID), iormRelationalTableStore)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.IsPrepared Then
                    aCommand.select = ConstFNEntryName & "," & ConstFNObjectName & "," & ConstFNDomainID
                    aCommand.Where = "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))

                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@domain", value:=domainid)
                aCommand.SetParameterValue(ID:="@globaldomain", value:=ConstGlobalDomain)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                ''' check the domain active records
                ''' 
                If theRecords.Count > 0 Then
                    Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                    For Each aRecord In theRecords
                        Dim pk As String = aRecord.GetValue(2) & ConstDelimiter & aRecord.GetValue(1) & ConstDelimiter & aRecord.GetValue(3)
                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(3).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Next

                    ''' build the index
                    ''' 
                    For Each aRecord In aDomainRecordCollection.Values
                        Dim aList As New List(Of String)
                        If Not aDictionary.ContainsKey(aRecord.GetValue(1)) Then
                            aDictionary.Add(key:=aRecord.GetValue(1), value:=aList)
                        Else
                            aList = aDictionary.Item(key:=aRecord.GetValue(1))
                        End If
                        If Not aList.Contains(aRecord.GetValue(2)) Then
                            aList.Add(aRecord.GetValue(2))
                        End If
                    Next
                End If

                ''' return the Records
                Return aDictionary

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, procedure:="AbstractEntryDefinition.GetEntryReference")
                Return aDictionary
            End Try

        End Function
        ''' <summary>
        ''' returns a Dictionary of Alias - list of objectentrynames
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAliasReference(Optional domainid As String = Nothing) As Dictionary(Of String, List(Of String))

            Dim aStore As iormRelationalTableStore = CurrentDBDriver.RetrieveContainerStore(ConstPrimaryTableID)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.IsPrepared Then
                    aCommand.select = ConstFNalias & "," & ConstFNEntryName & "," & ConstFNObjectName & "," & ConstFNDomainID
                    aCommand.Where = ConstFNalias & " <> '' AND " & ConstFNalias & " IS NOT NULL AND " & ConstFNalias & " <> '" & ConstDelimiter & ConstDelimiter & "' AND "
                    aCommand.Where &= "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))

                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@domain", value:=domainid)
                aCommand.SetParameterValue(ID:="@globaldomain", value:=ConstGlobalDomain)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                ''' check the domain active records
                ''' 
                If theRecords.Count > 0 Then
                    Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                    For Each aRecord In theRecords
                        Dim pk As String = aRecord.GetValue(3) & ConstDelimiter & aRecord.GetValue(2) & ConstDelimiter & aRecord.GetValue(4)
                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(4).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Next

                    ''' build the index
                    ''' 
                    For Each aRecord In aDomainRecordCollection.Values
                        Dim aList As New List(Of String)
                        Dim aliases As String() = Converter.otString2Array(aRecord.GetValue(1))
                        For Each anAlias In aliases
                            If Not aDictionary.ContainsKey(anAlias) Then
                                aDictionary.Add(key:=anAlias, value:=aList)
                            Else
                                aList = aDictionary.Item(key:=anAlias)
                            End If
                            If Not aList.Contains(aRecord.GetValue(3) & "." & aRecord.GetValue(2)) Then
                                aList.Add(aRecord.GetValue(3) & "." & aRecord.GetValue(2))
                            End If
                        Next

                    Next
                End If

                ''' return the Records
                Return aDictionary

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, procedure:="AbstractEntryDefinition.GetAliasReference")
                Return aDictionary
            End Try

        End Function
        ''' <summary>
        ''' returns a Dictionary of XID - (  ObjectEntryName in canonical form) Tuples
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetXIDReference(Optional domainid As String = Nothing) As Dictionary(Of String, List(Of String))

            Dim aStore As iormRelationalTableStore = CurrentDBDriver.RetrieveContainerStore(ConstPrimaryTableID)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.IsPrepared Then
                    aCommand.select = ConstFNxid & "," & ConstFNObjectName & "," & ConstFNEntryName & "," & ConstFNDomainID
                    aCommand.Where = ConstFNxid & " <> '' AND " & ConstFNxid & " IS NOT NULL AND "
                    aCommand.Where &= "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tableid:=ConstPrimaryTableID))

                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@domain", value:=domainid)
                aCommand.SetParameterValue(ID:="@globaldomain", value:=ConstGlobalDomain)

                '** run the Command
                Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                ''' check the domain active records
                ''' 
                If theRecords.Count > 0 Then
                    Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                    For Each aRecord In theRecords
                        Dim pk As String = aRecord.GetValue(2) & ConstDelimiter & aRecord.GetValue(3) & ConstDelimiter & aRecord.GetValue(4)
                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(4).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Next

                    ''' build the index
                    ''' 
                    For Each aRecord In aDomainRecordCollection.Values
                        Dim aList As New List(Of String)
                        If Not aDictionary.ContainsKey(aRecord.GetValue(1)) Then
                            aDictionary.Add(key:=aRecord.GetValue(1), value:=aList)
                        Else
                            aList = aDictionary.Item(key:=aRecord.GetValue(1))
                        End If
                        If Not aList.Contains(aRecord.GetValue(2) & "." & aRecord.GetValue(3)) Then
                            aList.Add(aRecord.GetValue(2) & "." & aRecord.GetValue(3))
                        End If
                    Next
                End If

                ''' return the Records
                Return aDictionary

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, procedure:="AbstractEntryDefinition.GetXIDReference")
                Return aDictionary
            End Try

        End Function


    End Class


    ''' <summary>
    ''' class for ObjectEntry (data slots)
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(ID:=ObjectCompoundEntry.ConstObjectID, modulename:=ConstModuleRepository, _
        description:="Compound definition of an object entry definition.", _
             AddDeleteFieldBehavior:=True, AddDomainBehavior:=True, _
            usecache:=True, isbootstrap:=True, Version:=1)> _
    Public Class ObjectCompoundEntry
        Inherits AbstractEntryDefinition
        Implements iormRelationalPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ObjectCompoundEntry"

        '** Field and tabele are comming from the Abstract Class

        '** extend the Table with additional fields
        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=100, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED100", title:="Compound object", Description:="name of the compound reference object")> _
        Public Const ConstFNFinalObjectID As String = "COBJECTNAME"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, posordinal:=101, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED101", title:="Compound Relation", Description:="relation path to the compound reference object")> _
        Public Const ConstFNCompoundRelation As String = "CRELATION"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=102, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED102", title:="compound id object entry", Description:="name of the compound reference id object entry")> Public Const ConstFNCompoundIDEntryname As String = "CIDENTRY"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=103, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED103", title:="compound value object entry", Description:="name of the compound reference value object entry")> Public Const ConstFNCompoundValueEntryName As String = "CVALUEENTRY"


        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=110, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED104", title:="compound setter operation", Description:="name of the compound setter method")> Public Const ConstFNCompoundSetter As String = "CSETTER"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=111, _
                       properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                       XID:="OED105", title:="compound getter operation", Description:="name of the compound getter method")> Public Const ConstFNCompoundGetter As String = "CGETTER"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=112, _
                      properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                      XID:="OED106", title:="compound validator operation", Description:="name of the compound validator method")> Public Const ConstFNCompoundValidator As String = "CVALIDATE"


        '** compound settings
        <ormObjectEntryMapping(EntryName:=ConstFNFinalObjectID)> Private _cFinalObjectID As String
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundRelation)> Private _cRelation As String()
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundIDEntryname)> Private _cIDEntryname As String
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundValueEntryName)> Private _cValueEntryName As String
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundGetter)> Private _CompoundGetterMethodName As String
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundSetter)> Private _CompoundSetterMethodName As String
        <ormObjectEntryMapping(EntryName:=ConstFNCompoundValidator)> Private _CompoundValidatorMethodName As String

        ''' method tags
        ''' 
        Public Const ConstCompoundSetter = "SETTER"
        Public Const ConstCompoundGetter = "GETTER"
        Public Const ConstCompoundValidator = "VALIDATOR"

        ''' <summary>
        ''' constructor of a SchemaDefTableEntry
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(objectid:=ConstObjectID)
            MyBase.Typeid = otObjectEntryType.Compound
        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the name of the compound validator method.
        ''' </summary>
        ''' <value>The name of the compound getter method.</value>
        Public Property CompoundValidatorMethodName() As String
            Get
                Return Me._CompoundValidatorMethodName
            End Get
            Set(value As String)
                SetValue(ConstFNCompoundValidator, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the compound getter method.
        ''' </summary>
        ''' <value>The name of the compound getter method.</value>
        Public Property CompoundGetterMethodName() As String
            Get
                Return Me._CompoundGetterMethodName
            End Get
            Set(value As String)
                SetValue(ConstFNCompoundGetter, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the compound setter method.
        ''' </summary>
        ''' <value>The name of the compound setter method.</value>
        Public Property CompoundSetterMethodName() As String
            Get
                Return Me._CompoundSetterMethodName
            End Get
            Set(value As String)
                SetValue(ConstFNCompoundSetter, value)
            End Set
        End Property

        ''' <summary>
        ''' not applicable for Compound Entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property PrimaryKeyOrdinal As Long
            Get
                Return 0
            End Get
            Set(value As Long)
                CoreMessageHandler(message:="ObjectCompoundEntry cannot be a primary key", procedure:="ObjectCompoundEntry.PrimaryKeyOrdinal", messagetype:=otCoreMessageType.InternalWarning)

            End Set
        End Property
        ''' <summary>
        ''' returns the Nullable Property
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property IsNullable() As Boolean
            Get
                Return _isnullable
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsNullable, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the size
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property Size() As Long?
            Get
                Return _size
            End Get
            Set(value As Long?)
                SetValue(entryname:=ConstFNSize, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the field data type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property Datatype() As otDataType
            Get
                Return _datatype
            End Get
            Set(value As otDataType)
                SetValue(entryname:=ConstFNDatatype, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the inner list data type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property InnerDatatype() As otDataType?
            Get
                Return _innerdatatype
            End Get
            Set(value As otDataType?)
                SetValue(entryname:=ConstFNInnerDatatype, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' gets the default value as object representation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property DefaultValue() As Object
            Get
                Return Converter.Object2otObject(_defaultvalue, Me.Datatype)
            End Get
            Set(value As Object)
                If value IsNot Nothing Then
                    SetValue(entryname:=ConstFNDefaultValue, value:=value.ToString)
                Else
                    SetValue(entryname:=ConstFNDefaultValue, value:=Nothing)
                End If

            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the default value in string presentation
        ''' </summary>
        ''' <value>The default value.</value>
        Public Property DefaultValueString() As String
            Get
                Return Me._defaultvalue.ToString
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDefaultValue, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' returns version
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version() As Long
            Get
                Return _version
            End Get
            Set(value As Long)
                SetValue(entryname:=ConstFNUPDC, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the resulting Compound Object ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundObjectID() As String
            Get
                Return _cFinalObjectID
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNFinalObjectID, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' returns the entryname of the compound ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundIDEntryname() As String
            Get
                Return _cIDEntryname
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNCompoundIDEntryname, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the entryname of the compounds value in the resulting object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundValueEntryName() As String
            Get
                Return _cValueEntryName
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNCompoundValueEntryName, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' returns the path of relations of a compound to the resulting object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundRelationPath() As String()
            Get
                Return _cRelation
            End Get
            Set(value As String())
                SetValue(entryname:=ConstFNCompoundRelation, value:=value)
            End Set
        End Property

        ''' <summary>
        ''' gets true if a spare field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property IsSpareField() As Boolean
            Get
                Return False
            End Get
            Set(value As Boolean)
                CoreMessageHandler(message:="compound cannot be sparefield", procedure:="ObjectCompoundEntry.IsSpareField", entryname:=Me.Entryname, objectname:=Me.Objectname)
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Overrides Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off the column definition via event Handler
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub

        ''' <summary>
        ''' Increase the version
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IncVersion() As Long
            _version = _version + 1
            IncVersion = _version
        End Function

        ''' <summary>
        ''' retrieves an Object entry Definition from persistence store
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="entryname"></param>
        ''' <param name="domainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal objectname As String, entryname As String, _
                                                  Optional ByVal domainID As String = Nothing, _
                                                  Optional runtimeOnly As Boolean = False) As ObjectCompoundEntry
            If String.IsnullorEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            Return RetrieveDataObject(Of ObjectCompoundEntry)(pkArray:={objectname.ToUpper, entryname.ToUpper, domainID}, domainID:=domainID, runtimeOnly:=runtimeOnly)
        End Function


        ''' <summary>
        ''' create a new dataobject with primary keys
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <param name="typeid"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal objectname As String, ByVal entryname As String, _
                                            Optional ByVal domainid As String = Nothing, _
                                            Optional ByVal runtimeOnly As Boolean = False, _
                                            Optional ByVal checkunique As Boolean = True) As ObjectCompoundEntry
            '** create with record to fill other values
            If String.IsnullorEmpty(domainID) Then domainid = CurrentSession.CurrentDomainID
            Dim arecord As New ormRecord
            With arecord
                .SetValue(ConstFNObjectName, objectname.ToUpper)
                .SetValue(ConstFNEntryName, entryname.ToUpper)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(ConstFNType, otObjectEntryType.Compound)
            End With

            ' create
            Return ormBusinessObject.CreateDataObject(Of ObjectCompoundEntry)(record:=arecord, domainID:=domainid, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function
    End Class
    ''' <summary>
    ''' class for Column ObjectEntry (data slots) - it mostly references to the ColumnDefinition object to keep the definition of the table columns unique
    ''' </summary>
    ''' <remarks></remarks>
    'explicit since we are not running through inherited classes
    <ormObject(id:=ObjectContainerEntry.ConstObjectID, modulename:=ConstModuleRepository, _
                AddDeletefieldBehavior:=True, AddDomainBehavior:=True, _
                Description:="Object Entry Definition as Column Entry (of a Table)", _
                usecache:=True, isbootstrap:=True, Version:=1)> _
    Public Class ObjectContainerEntry
        Inherits AbstractEntryDefinition
        Implements iormRelationalPersistable, iormInfusable, iormObjectEntry, System.ComponentModel.INotifyPropertyChanged


        '*** CONST Schema
        Public Shadows Const ConstObjectID = "ObjectContainerEntry"

        '*** Columns
        <ormObjectEntry(referenceobjectentry:=ContainerDefinition.ConstObjectID & "." & ContainerDefinition.ConstFNContainerID, posordinal:=90, isnullable:=True, _
                         properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        Description:="corresponding container id of the entry ")> Public Const ConstFNContainerID As String = ContainerDefinition.ConstFNContainerID

        <ormObjectEntry(referenceobjectentry:=ContainerEntryDefinition.ConstObjectID & "." & ContainerEntryDefinition.ConstFNContainerEntryName, posordinal:=91, isnullable:=True, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        Description:="corresponding container entry name of the object entry")> Public Const ConstFNContainerEntryName As String = ContainerEntryDefinition.ConstFNContainerEntryName

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", posordinal:=92, _
                       title:="SpareFieldTag", Description:="set if the entry is a spare entry")> Public Const ConstFNSpareFieldTag As String = "SpareFieldTag"

        ' foreign key doesnot work for some reasons - sqlserver doesnot allow
        '
        '<ormForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
        'entrynames:={ConstFNTableName, ConstFNColumnname}, _
        'foreignkeyreferences:={ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNTableName, _
        'ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNColumnname})> Public Const constFKColumns = "FKColumns"

        ''' <summary>
        ''' relation to the columndefinition - will be always created on create
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelationAttribute(linkobject:=GetType(ContainerEntryDefinition), toPrimarykeys:={ConstFNContainerID, ConstFNContainerEntryName}, createObjectIfnotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRColumnDefinition = "COLUMN"
        '** the real thing
        <ormObjectEntryMapping(relationName:=constRColumnDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnInject Or otInfuseMode.OnDefault)> _
        Private _containerentrydefinition As ContainerEntryDefinition

        ' fields
        <ormObjectEntryMapping(EntryName:=ConstFNContainerID)> Private _containerid As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNContainerEntryName)> Private _containerEntryName As String = String.Empty
        <ormObjectEntryMapping(EntryName:=ConstFNSpareFieldTag)> Private _SpareFieldTag As Boolean = False



        ' further internals

        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged


        ''' <summary>
        ''' constructor 
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(objectid:=ConstObjectID)
            _typeid = otObjectEntryType.ContainerEntry
            AddHandler ormBusinessObject.OnCreateDefaultValuesNeeded, AddressOf OnDefaultValuesNeeded
            AddHandler ormBusinessObject.OnFeeding, AddressOf OnFeeding
            AddHandler ormBusinessObject.OnValidating, AddressOf OnValidating
            AddHandler ormBusinessObject.OnInitializing, AddressOf OnInitialize
        End Sub

#Region "Properties"

        ''' <summary>
        ''' sets or gets the column name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContainerEntryName() As String
            Get
                ContainerEntryName = _containerEntryName
            End Get
            Set(value As String)
                SetValue(ConstFNContainerEntryName, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the table name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContainerID() As String
            Get
                ContainerID = _containerid
            End Get
            Set(value As String)
                SetValue(ConstFNContainerID, value)
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Overrides Property Description() As String
            Get
                Return Me._Description
            End Get
            Set(value As String)
                '* set own value
                SetValue(entryname:=ConstFNDescription, value:=value)
                '** sets the column description
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Description") Then
                    Return
                Else
                    If _containerentrydefinition.Description Is Nothing OrElse Not _containerentrydefinition.Description.Equals(value) Then
                        _containerentrydefinition.Description = value
                    End If
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets true if a spare field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property IsSpareField() As Boolean
            Get
                Return Me._SpareFieldTag
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNSpareFieldTag, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the default value (database level) of the column entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DBDefaultValue() As Object
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.DBDefaultValue") Then
                    Return _containerentrydefinition.DefaultValue
                Else : Return Nothing
                End If
            End Get
            Set(value As Object)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.DBDefaultValue") Then
                    Return
                End If
                If _containerentrydefinition.DefaultValue Is Nothing OrElse Not _containerentrydefinition.DefaultValue.Equals(value) Then
                    _containerentrydefinition.DefaultValue = value
                    IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the Datatype
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property Datatype() As otDataType
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Datatype") Then
                    Return _containerentrydefinition.Datatype
                Else : Return 0
                End If
            End Get
            Set(avalue As otDataType)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Datatype") Then
                    Return
                End If
                _containerentrydefinition.Datatype = avalue
                SetValue(ConstFNDatatype, avalue) '*local copy
            End Set
        End Property

        ''' <summary>
        ''' returns the Position in the primary key ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property PrimaryKeyOrdinal() As Long
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.PrimaryKeyOrdinal") Then
                    Return _containerentrydefinition.PrimaryKeyOrdinal
                Else : Return 0
                End If
            End Get
            Set(avalue As Long)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.PrimaryKeyOrdinal") Then
                    Return
                End If
                If _containerentrydefinition.PrimaryKeyOrdinal <> avalue Then
                    _containerentrydefinition.PrimaryKeyOrdinal = avalue
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' return the IndexName if entry belongs to an index
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Indexname() As String
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Indexname") Then
                    Return _containerentrydefinition.Indexname
                Else : Return String.Empty
                End If
            End Get
            Set(value As String)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Indexname") Then
                    Return
                End If
                If _containerentrydefinition.Indexname.ToUpper <> value.ToUpper Then
                    _containerentrydefinition.Indexname = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns true if column accepts null
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property IsNullable() As Boolean
            Get
                Return _isnullable 'local one ! might differ
            End Get
            Set(value As Boolean)
                '* local copy might differ to _columndefinition
                SetValue(ConstFNIsNullable, value)
            End Set
        End Property
        ''' <summary>
        ''' returns true if Entry has a Primary Key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsPrimaryKey() As Boolean
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.IsPrimaryKey") Then
                    Return _containerentrydefinition.IsPrimaryKey
                Else : Return False
                End If
            End Get
            Set(value As Boolean)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.IsPrimaryKey") Then
                    Return
                End If
                If _containerentrydefinition.IsPrimaryKey <> value Then
                    _containerentrydefinition.IsPrimaryKey = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the datasize 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property Size() As Long?
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Size") Then
                    Return _containerentrydefinition.Size
                Else : Return 0
                End If
            End Get
            Set(value As Long?)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Size") Then
                    Return
                End If
                _containerentrydefinition.Size = value
                '* local copy
                SetValue(ConstFNSize, value)
            End Set
        End Property

        ''' <summary>
        ''' returns the Position Ordinal in the table (record)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContainerEntryOrdinal() As UShort
            Get
                If _containerentrydefinition IsNot Nothing AndAlso _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Position") Then
                    Return _containerentrydefinition.Position
                Else : Return 0
                End If
            End Get
            Set(value As UShort)
                If _containerentrydefinition Is Nothing OrElse Not _containerentrydefinition.IsAlive(subname:="ObjectColumnEntry.Position") Then
                    Return
                End If
                If _containerentrydefinition.Position <> value Then
                    _containerentrydefinition.Position = value
                    IsChanged = True
                End If
            End Set
        End Property

        ''' <summary>
        ''' returns the corresponding columndefinition
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Definition As ContainerEntryDefinition
            Get
                Return _containerentrydefinition
            End Get
        End Property
#End Region



        ''' <summary>
        ''' Event Handler for defaultValues
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs)

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            If e.Record.HasIndex(ConstFNDatatype) Then
                Dim adatatype As otDataType = e.Record.GetValue(ConstFNDatatype)
                If adatatype = otDataType.Text OrElse adatatype = otDataType.List Then
                    If Not e.Record.HasIndex(ConstFNSize) Then
                        e.Result = e.Result And e.Record.SetValue(ConstFNSize, ConstDBDriverMaxTextSize)
                        Exit Sub
                    Else
                        Dim aSizeValue As Object = e.Record.GetValue(ConstFNSize)
                        If Convert.ToInt64(aSizeValue) < 1 Then
                            e.Result = e.Result And e.Record.SetValue(ConstFNSize, ConstDBDriverMaxTextSize)
                            Exit Sub
                        End If
                    End If
                End If
            End If

        End Sub

        ''' <summary>
        ''' Event Handler for Validating - correct
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnValidating(sender As Object, e As ormDataObjectEventArgs)

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            Dim anObject = TryCast(e.DataObject, ObjectContainerEntry)
            If anObject IsNot Nothing Then
                If anObject.Datatype = otDataType.Text Or anObject.Datatype = otDataType.List Then
                    If Not anObject.Size.HasValue OrElse (anObject.Size.HasValue AndAlso anObject.Size < 1) Then
                        anObject.Size = ConstDBDriverMaxTextSize
                    End If
                End If
            End If

        End Sub


        ''' <summary>
        ''' Event Handler for Feeding 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnFeeding(sender As Object, e As ormDataObjectEventArgs)

            ''' check if we have a datatype text or list
            ''' then set also the size
            ''' 
            Dim anObject = TryCast(e.DataObject, ObjectContainerEntry)
            If anObject IsNot Nothing Then
                If Not anObject.Datatype = otDataType.Text AndAlso Not anObject.Datatype = otDataType.List Then
                    If anObject.Size.HasValue Then
                        anObject.Size = Nothing
                        e.Result = True
                    End If
                End If
            End If

        End Sub
        ''' <summary>
        ''' Handler for the SwitchRuntimeOFF Event after Bootstrapping 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Overrides Sub OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
            '** also switch runtime off 
            '** column definition must be switched off via tabledefinition
            e.Result = Me.SwitchRuntimeOff()
            If Not e.Result Then e.AbortOperation = True
        End Sub
        ''' <summary>
        ''' Initialize Event handler
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnInitialize(sender As Object, e As ormDataObjectEventArgs)
            If _containerentrydefinition Is Nothing Then _containerentrydefinition = New ContainerEntryDefinition
        End Sub


        ''' <summary>
        ''' set the properties of a Column Entry by a ormObjectEntryAttribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean
            If Not IsAlive(subname:="SetByAttribute") Then Return False


            With attribute
                Me.Typeid = otObjectEntryType.ContainerEntry
                '** Slot Entry Properties
                MyBase.SetByAttribute(attribute)

                If .HasValueContainerID Then Me.ContainerID = .ContainerID
                If .HasValueContainerEntryName Then Me.ContainerEntryName = .ContainerEntryName

                '* column attributes
                If .HasValueIsNullable Then Me.IsNullable = .IsNullable
                If .HasValueIsNullable Then Me.Definition.IsNullable = .IsNullable ' should be the same in the beginning

                If .hasValuePosOrdinal Then Me.ContainerEntryOrdinal = .Posordinal ' should be the position from a table definition not an object definition
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryEntryOrdinal

                If .HasValueSize Then Me.Size = .Size
                If .HasValueDBDefaultValue Then Me.DBDefaultValue = .DBDefaultValue
                If .HasValueSpareFieldTag Then Me.IsSpareField = .SpareFieldTag
                If .HasValueDataType Then Me.Datatype = .Datatype

                If .HasValueUseForeignKey And .UseForeignKey <> otForeignKeyImplementation.None Then
                    ' we should check if the foreign key from attribute is now in the table.foreignkeys
                End If

            End With

            Return True
        End Function

        ''' <summary>
        ''' retrieves an Object entry Definition from persistence store
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <param name="entryname"></param>
        ''' <param name="domainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal objectname As String, entryname As String, _
                                                  Optional ByVal domainID As String = Nothing, _
                                                  Optional runtimeOnly As Boolean = False) As ObjectContainerEntry
            If String.IsNullOrEmpty(domainID) Then domainID = CurrentSession.CurrentDomainID
            Return RetrieveDataObject(Of ObjectContainerEntry)(pkArray:={objectname.ToUpper, entryname.ToUpper, domainID}, domainID:=domainID, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' Event Handler relation loaded
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRelationLoaded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnRelationLoad
            Dim aColumnEntry = TryCast(e.DataObject, ObjectContainerEntry)
            '** add the new columndefinition element in the table definition
            If aColumnEntry IsNot Nothing AndAlso e.Infusemode = otInfuseMode.OnCreate Then
                '** set up the connection to the tabledefinition
                Dim aTableDefinition As ContainerDefinition = ContainerDefinition.Retrieve(tablename:=aColumnEntry.ContainerID, runtimeOnly:=e.DataObject.RunTimeOnly)
                If aTableDefinition IsNot Nothing AndAlso Not aTableDefinition.HasEntry(entryname:=aColumnEntry.ContainerEntryName) Then
                    aTableDefinition.AddColumn(aColumnEntry.Definition)
                ElseIf aTableDefinition Is Nothing Then
                    CoreMessageHandler(message:="TableDefinition could not be retrieved", messagetype:=otCoreMessageType.InternalError, containerID:=_containerid, _
                                       objectname:=Me.ObjectID, procedure:="ObjectColumnEntry.OnRelationloaded")
                End If

            End If

            ''' register for changed of the column definition
            ''' 
            If aColumnEntry IsNot Nothing AndAlso e.RelationIDs.Contains(constRColumnDefinition.ToUpper) Then
                If _containerentrydefinition IsNot Nothing Then AddHandler _containerentrydefinition.PropertyChanged, AddressOf ColumnDefinition_PropertyChanged
            End If
        End Sub
        ''' <summary>
        ''' create a new dataobject with primary keys
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <param name="typeid"></param>
        ''' <param name="runtimeOnly"></param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal objectname As String, _
                                      ByVal entryname As String, _
                                      ByVal tablename As String, _
                                      ByVal columnname As String, _
                                      Optional ByVal ordinal As Long? = Nothing, _
                                            Optional ByVal domainid As String = Nothing, _
                                            Optional ByVal runtimeOnly As Boolean = False, _
                                            Optional ByVal checkunique As Boolean = True) As ObjectContainerEntry
            '** create with record to fill other values
            Dim arecord As New ormRecord
            With arecord
                .SetValue(ConstFNObjectName, objectname.ToUpper)
                .SetValue(ConstFNEntryName, entryname.ToUpper)
                .SetValue(ConstFNContainerID, tablename.ToUpper)
                .SetValue(ConstFNContainerEntryName, columnname.ToUpper)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(ConstFNType, otObjectEntryType.ContainerEntry)
                If ordinal.HasValue Then .SetValue(ConstFNordinal, ordinal)

            End With

            ' create
            Return ormBusinessObject.CreateDataObject(Of ObjectContainerEntry)(record:=arecord, domainID:=domainid, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


        ''' <summary>
        ''' handler for columndefinition property changed event raises the iormObjectEntry event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ColumnDefinition_PropertyChanged(sender As Object, e As PropertyChangedEventArgs)
            If e.PropertyName = Definition.ConstFNPrimaryKeyOrdinal Then
                ' cascade it
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(e.PropertyName))
            ElseIf e.PropertyName = Definition.ConstFNPrimaryKey Then
                ' cascade it
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(e.PropertyName))
            End If
        End Sub
    End Class


End Namespace