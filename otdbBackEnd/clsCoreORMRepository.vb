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
        Private _tableDirectory As New Dictionary(Of String, TableDefinition)
        '** reference to all the XChange IDs
        Private _XIDDirectory As New Dictionary(Of String, List(Of iormObjectEntry))
        '** reference to all the aliases
        Private _aliasDirectory As New Dictionary(Of String, List(Of iormObjectEntry))

        Private _xidShortReference As Dictionary(Of String, List(Of String)) ' dictionary for cross referenceing
        Private _aliasShortReference As Dictionary(Of String, List(Of String)) ' dictionary for cross referencing

        '** reference to the session 
        Private _DomainID As String = ""
        Private WithEvents _Domain As Domain
        Private WithEvents _Session As Session ' reference to session which we belong

        Private _lock As New Object

        Public Event OnObjectDefinitionLoaded(sender As Object, e As ObjectRepository.EventArgs)
        ''' <summary>
        ''' construction with link to the connection
        ''' </summary>
        ''' <param name="connection"></param>
        ''' <remarks></remarks>

        Sub New(ByRef Session As Session)
            _Session = Session
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
        Public ReadOnly Property TableDefinitions As IEnumerable(Of TableDefinition)
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
            Dim anObjectDef As ObjectDefinition = ObjectDefinition.Retrieve(objectname:=ent.Objectname, domainID:=_DomainID)

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
            If _DomainID = "" And Not IsInitialized Then
                If e.Domain IsNot Nothing Then
                    _DomainID = e.Domain.DomainID
                End If

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
                IsInitialized = Me.Initialize
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
                _DomainID = ""
                _IsInitialized = False
                _Session = Nothing
            End SyncLock
            Return True
        End Function

        Public Sub OnDomainChanging(sender As Object, e As SessionEventArgs) Handles _Session.OnDomainChanging
            If Not IsInitialized Then
                SyncLock _lock
                    If e.NewDomain IsNot Nothing Then
                        _DomainID = e.NewDomain.DomainID
                    Else
                        _DomainID = DirectCast(sender, Session).CurrentDomainID
                    End If

                End SyncLock

            End If
        End Sub
        Public Sub OnDomainChanged(sender As Object, e As SessionEventArgs) Handles _Session.OnDomainChanged
            If Not IsInitialized Then
                SyncLock _lock
                    _DomainID = DirectCast(sender, Session).CurrentDomainID
                End SyncLock

                Initialize()
            End If
        End Sub
        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean
            Dim aDBDriver As iormDatabaseDriver

            '* donot doe it again
            If Me.IsInitialized Then Return False

            If _DomainID = "" Then
                CoreMessageHandler(message:="DomainID is not set in objectStore", arg1:=Me._Session.SessionID, messagetype:=otCoreMessageType.InternalError, _
                                   subname:="ObjectRepository.Initialize")
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
                aDBDriver = GetTableStore(ObjectDefinition.ConstTableID).Connection.DatabaseDriver
            Else
                CoreMessageHandler(message:="not able to get database driver", arg1:=_Session.SessionID, messagetype:=otCoreMessageType.InternalError, _
                                    subname:="ObjectRepository.Initialize")
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

                    CoreMessageHandler(message:="Initializing " & ot.GetBootStrapObjectClassIDs.Count & " OnTrack Bootstrapping Objects ....", messagetype:=otCoreMessageType.ApplicationInfo, subname:="ObjectRepository.Initialize")

                    Dim i As UShort = 1

                    '** load the bootstrapping core
                    For Each name In ot.GetBootStrapObjectClassIDs
                        name = Trim(name.ToUpper) ' for some reasons better to trim
                        Dim anObject As ObjectDefinition = _
                            ObjectDefinition.Retrieve(objectname:=name, dbdriver:=aDBDriver, domainID:=_DomainID)
                        If anObject IsNot Nothing Then
                            Me.LoadIntoRepository(anObject)
                            CoreMessageHandler(message:="Initialized OnTrack " & i & "/" & ot.GetBootStrapObjectClassIDs.Count & " Bootstrapping Object " & name, messagetype:=otCoreMessageType.ApplicationInfo, subname:="ObjectRepository.Initialize")

                        Else
                            CoreMessageHandler(message:="could not load object '" & name & "'", messagetype:=otCoreMessageType.InternalError, _
                                               subname:="ObjectRepository.Initialize")
                        End If
                        i += 1
                    Next
                    i = 1
                    CoreMessageHandler(message:="Initializing " & theObjectnames.Count & " OnTrack Objects ....", messagetype:=otCoreMessageType.ApplicationInfo, subname:="ObjectRepository.Initialize")
                    '** load all objects with entries and aliases
                    For Each name In theObjectnames
                        name = Trim(name.ToUpper) ' for some reasons bette to trim
                        Dim anObject As ObjectDefinition = _
                            ObjectDefinition.Retrieve(objectname:=name, dbdriver:=aDBDriver, domainID:=_DomainID)
                        If anObject IsNot Nothing Then
                            Me.LoadIntoRepository(anObject)
                            CoreMessageHandler(message:="Initialized " & i & "/" & theObjectnames.Count & " OnTrack Object " & name, messagetype:=otCoreMessageType.ApplicationInfo, subname:="ObjectRepository.Initialize")

                        Else
                            CoreMessageHandler(message:="could not load object '" & name & "'", messagetype:=otCoreMessageType.InternalError, _
                                               subname:="ObjectRepository.Initialize")
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
                               messagetype:=otCoreMessageType.ApplicationInfo, subname:="ObjectRepository.Initialize")

            Return Me.IsInitialized
        End Function

        ''' <summary>
        ''' Load Object into Store of Objects
        ''' </summary>
        ''' <param name="object"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function LoadIntoRepository(ByRef [object] As ObjectDefinition) As Boolean

            If Not [object].IsLoaded And Not [object].IsCreated Then
                Call CoreMessageHandler(message:="object is neither created nor loaded", subname:="ObjectRepository.LoadIntoRepository", _
                                        tablename:=[object].ID, messagetype:=otCoreMessageType.InternalError)

                Return False
            End If

            '*** check if version is the same as in code
            Dim aTableAttribute As ormSchemaTableAttribute = ot.GetSchemaTableAttribute(tablename:=[object].ID)
            If aTableAttribute IsNot Nothing Then
                If [object].Version <> aTableAttribute.Version Then
                    '_Session.CurrentDBDriver.VerifyOnTrackDatabase(verifyOnly:=False, createOnMissing:=True)
                    CoreMessageHandler(message:="Attention ! Version of object in object store V" & [object].Version & " is different from version in code V" & aTableAttribute.Version, _
                                       messagetype:=otCoreMessageType.InternalWarning, tablename:=[object].ID, subname:="ObjectStore.LoadIntoRepository")
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
                If Not _tableDirectory.ContainsKey(key:=aTableDefinition.Name) Then
                    _tableDirectory.Add(key:=aTableDefinition.Name, value:=aTableDefinition)
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
        Public Function GetTable(tablename As String, Optional runtimeOnly As Boolean? = Nothing) As TableDefinition

            tablename = tablename.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            If tablename.Contains("."c) Then
                tablename = Shuffle.NameSplitter(tablename).First
            End If

            '** name is given
            If tablename <> "" Then
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
                        Dim anObject = ObjectDefinition.Retrieve(objectname:=objectname, domainID:=_DomainID, runtimeOnly:=runtimeOnly)
                        '** no object in persistancy but creatable from class description
                        If anObject Is Nothing Then
                            anObject = ObjectDefinition.Create(objectID:=objectname, runTimeOnly:=runtimeOnly)
                            If anObject Is Nothing Then
                                CoreMessageHandler(message:="Failed to retrieve the object definition in non runtime mode", arg1:=objectname, _
                                                    objectname:=objectname, messagetype:=otCoreMessageType.InternalError, subname:="ObjectRepository.getTable")
                                Return Nothing
                            ElseIf Not anObject.SetupByClassDescription(ot.GetObjectClassType(objectname:=objectname), runtimeOnly:=runtimeOnly) Then
                                CoreMessageHandler(message:="Failed to setup the object definition from the object class description", arg1:=objectname, _
                                                    objectname:=objectname, messagetype:=otCoreMessageType.InternalError, subname:="ObjectRepository.getTable")
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
        Public Function GetColumnEntry(columnname As String, Optional tablename As String = "", Optional runtimeOnly As Boolean? = Nothing) As ColumnDefinition
            columnname = columnname.ToUpper
            tablename = tablename.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            If tablename = "" And columnname.Contains(".") Then
                Shuffle.NameSplitter(columnname, tablename, columnname)
            End If

            '** name is given
            If tablename <> "" Then
                If _tableDirectory.ContainsKey(tablename) Then
                    Dim aTable = _tableDirectory.Item(tablename)
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
        Public Function GetEntry(entryname As String, Optional objectname As String = "", Optional runtimeOnly As Boolean? = Nothing) As iormObjectEntry
            entryname = entryname.ToUpper
            objectname = objectname.ToUpper
            If runtimeOnly Is Nothing Then runtimeOnly = _Session.IsBootstrappingInstallationRequested

            '** objectname is given
            If objectname <> "" Then

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
                If Not aName Is Nothing AndAlso aName <> "" Then
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
        Public Function GetObject(objectid As String, Optional runtimeOnly As Boolean = False) As ObjectDefinition
            Dim anObject As ObjectDefinition
            objectid = objectid.ToUpper

            If _objectDirectory.ContainsKey(key:=objectid) Then
                Return _objectDirectory.Item(key:=objectid)
                ' try to reload
            Else
                '** no runtime -> better ask the session
                If Not runtimeOnly Then runtimeOnly = _Session.IsBootstrappingInstallationRequested
                '** retrieve Object
                anObject = ObjectDefinition.Retrieve(objectname:=objectid, domainID:=_DomainID, runtimeOnly:=runtimeOnly)
                '** no object in persistancy but creatable from class description
                If anObject Is Nothing AndAlso ot.GetObjectClassDescriptionByID(id:=objectid) IsNot Nothing Then
                    anObject = ObjectDefinition.Create(objectID:=objectid, runTimeOnly:=runtimeOnly)
                    If anObject Is Nothing Then
                        CoreMessageHandler(message:="Failed to retrieve the object definition in non runtime mode", arg1:=objectid, _
                                            objectname:=objectid, messagetype:=otCoreMessageType.InternalError, subname:="ObjectRepository.getObject")
                        Return Nothing
                    ElseIf Not anObject.SetupByClassDescription(ot.GetObjectClassType(objectname:=objectid), runtimeOnly:=runtimeOnly) Then
                        CoreMessageHandler(message:="Failed to setup the object definition from the object class description", arg1:=objectid, _
                                            objectname:=objectid, messagetype:=otCoreMessageType.InternalError, subname:="ObjectRepository.getObject")
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
        Public Function GetEntryByXID([xid] As String, Optional objectname As String = "") As IList(Of iormObjectEntry)
            xid = xid.ToUpper
            objectname = objectname.ToUpper
            If _XIDDirectory.ContainsKey(xid) Then
                If objectname = "" Then
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
                    If objectname <> "" AndAlso names(0) = objectname Then
                        Me.GetObject(names(0)) ' load the object full
                        If _XIDDirectory.ContainsKey(xid) Then
                            Return GetEntryByXID(xid) 'recursion by intention
                        Else
                            CoreMessageHandler(message:="xid could not be found in XIDDirectory although reference object was loaded", _
                                               arg1:=xid, objectname:=objectname, _
                                               subname:="ObjectRepository.GetEntryByXID", _
                                               messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of iormObjectEntry)
                        End If
                    Else
                        Me.GetObject(names(0)) ' load the object full
                    End If
                    ' return
                    If _XIDDirectory.ContainsKey(xid) Then
                        Return GetEntryByXID(xid)
                    Else
                        CoreMessageHandler(message:="xid could not be found in XIDDirectory although reference object was loaded", _
                                               arg1:=xid, _
                                               subname:="ObjectRepository.GetEntryByXID", _
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
        Public Function GetEntryByAlias([alias] As String, Optional objectname As String = "") As IList(Of iormObjectEntry)
            [alias] = [alias].ToUpper
            If _aliasDirectory.ContainsKey([alias]) Then
                If objectname = "" Then
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
                    If objectname <> "" AndAlso names(0) = objectname Then
                        Me.GetObject(names(0)) ' load the object full
                        If _aliasDirectory.ContainsKey([alias]) Then
                            Return GetEntryByAlias([alias]) 'recursion by intention
                        Else
                            CoreMessageHandler(message:="alias could not be found in Alias Directory although reference object was loaded", _
                                               arg1:=[alias], objectname:=objectname, _
                                               subname:="ObjectRepository.GetEntryByAlias", _
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
                                           arg1:=[alias], _
                                           subname:="ObjectRepository.GetEntryByalias", _
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
        Public Function GetEntryByAlias([aliases]() As String, Optional objectname As String = "") As List(Of iormObjectEntry)
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
    <ormObject(id:=ColumnDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Column Definition of a Table Definition", _
        Version:=2, usecache:=True, isbootstrap:=True)> _
    Public Class ColumnDefinition
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ColumnDefinition"
        '** Table
        <ormSchemaTableAttribute(Version:=2, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False)> Public Const ConstTableID = "tblTableColumnDefinitions"
        '** Index

        '*** Columns
        '*** Keys
        <ormObjectEntry(referenceobjectentry:=TableDefinition.ConstObjectID & "." & TableDefinition.ConstFNTablename, _
                        primaryKeyordinal:=1, useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNTableName As String = TableDefinition.ConstFNTablename

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primaryKeyordinal:=2, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Column Name", Description:="column name in the table")> Public Const ConstFNColumnname As String = "ColumnName"

        '** Column Specific

        <ormObjectEntry(defaultvalue:=0, Datatype:=otDataType.[Long], isnullable:=True, title:="Pos", Description:="position number in record")> _
        Public Const ConstFNPosition As String = "pos"

        <ormObjectEntry(Datatype:=otDataType.Memo, isnullable:=True, properties:={ObjectEntryProperty.Trim}, _
                        title:="Description", Description:="Description of the field")> Public Const ConstFNDescription As String = "desc"

        <ormObjectEntry(Datatype:=otDataType.List, innerDatatype:=otDataType.Text, _
                        title:="Properties", Description:="database column properties")> Public Const ConstFNProperties As String = "properties"

        <ormObjectEntry(defaultvalue:=otDataType.Text, referenceobjectentry:=ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNDatatype, _
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
        <ormEntryMapping(EntryName:=ConstFNTableName)> Private _tablename As String = ""
        <ormEntryMapping(EntryName:=ConstFNColumnname)> Private _ColumnName As String = ""
        <ormEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Private _datatype As otDataType = 0
        <ormEntryMapping(EntryName:=ConstFNUPDC)> Private _version As Long = 0
        <ormEntryMapping(EntryName:=ConstFNSize)> Private _size As Long?
        <ormEntryMapping(EntryName:=ConstFNIsNullable)> Private _isNullable As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNIsUnique)> Private _isUnique As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNDefaultValue)> Private _DefaultValue As String = Nothing ' that is ok since default might be missing for strings
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _Description As String = ""
        <ormEntryMapping(EntryName:=ConstFNPosition)> Private _Position As Long?
        <ormEntryMapping(EntryName:=ConstFNindexname)> Private _indexname As String = ""
        <ormEntryMapping(EntryName:=ConstFNPrimaryKey)> Private _isPrimaryKey As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNPrimaryKeyOrdinal)> Private _PrimaryKeyOrdinal As Long = 0

        '* relation to the Tabledefinition - no cascadeOnUpdate to prevent recursion loops
        <ormRelationAttribute(linkobject:=GetType(TableDefinition), toPrimarykeys:={ConstFNTableName}, createObjectIfNotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRTableDefinition = "table"
        '** the real thing
        <ormEntryMapping(relationName:=constRTableDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> _
        Private _Tabledefinition As TableDefinition


        '** dynamic


        ''' <summary>
        ''' constructor of a SchemaDefTableEntry
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

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
                    Return ""
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
        Public ReadOnly Property TableDefinition As TableDefinition
            Get
                If _Tabledefinition Is Nothing And _tablename <> "" Then
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

            If Not attribute.HasValueTableName OrElse Not attribute.HasValueColumnName Then
                CoreMessageHandler(message:="attribute has not set tablename or columnname", subname:="objectablecolumn.setbyAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=Me.ConstObjectID)
                Return False
            End If

            With attribute
                If .HasValueDBDefaultValue Then Me.DefaultValue = .DBDefaultValue
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueIsNullable Then Me.IsNullable = .IsNullable
                If .HasValueDataType Then Me.Datatype = .DataType
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueSize Then Me.Size = .Size
                If .HasValueParameter Then Me.Properties = Converter.otString2Array(.Parameter)
                If .hasValuePosOrdinal Then Me.Position = .Posordinal

                If .HasValuePrimaryKeyOrdinal Then
                    Me.IsPrimaryKey = True
                End If
                If .HasValueIsUnique Then Me.IsUnique = .IsUnique
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryKeyOrdinal
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
            Dim anObject = TryCast(e.DataObject, ColumnDefinition)
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
            Dim anObject = TryCast(e.DataObject, ColumnDefinition)
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
            Dim anObject = TryCast(e.DataObject, ColumnDefinition)
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
        Public Overloads Shared Function Retrieve(ByVal tablename As String, ByVal columnname As String, Optional forcereload As Boolean = False, Optional runtimeOnly As Boolean = False) As ColumnDefinition
            Return Retrieve(Of ColumnDefinition)(pkArray:={tablename.ToUpper, columnname.ToUpper}, forceReload:=forcereload, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnCreated
            Dim myself = TryCast(e.DataObject, ColumnDefinition)
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
                                        Optional ByVal checkunique As Boolean = True) As ColumnDefinition
            Dim primarykey() As Object = {tablename.ToUpper, columnname.ToUpper}

            ' create
            Return ormDataObject.CreateDataObject(Of ColumnDefinition)(pkArray:=primarykey, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


    End Class

    ''' <summary>
    ''' class for foreign key definition of multiple table columns
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(id:=ForeignKeyDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Foreign Key Definition of a Table", _
        Version:=1, usecache:=True, isbootstrap:=True)> _
    Public Class ForeignKeyDefinition
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ForeignKeyDefinition"
        '** Table
        <ormSchemaTableAttribute(Version:=1, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=False)> Public Const ConstTableID = "TBLTABLEFOREIGNKEYS"
        '** Index

        '*** Columns
        '*** Keys
        <ormObjectEntry(referenceobjectentry:=TableDefinition.ConstObjectID & "." & TableDefinition.ConstFNTablename, _
                        primaryKeyordinal:=1, useforeignKey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNTableName As String = TableDefinition.ConstFNTablename

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primaryKeyordinal:=2, _
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
        <ormEntryMapping(EntryName:=ConstFNTableName)> Private _tablename As String = ""
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""

        <ormEntryMapping(EntryName:=ConstFNUPDC)> Protected _version As Long = 0
        <ormEntryMapping(EntryName:=ConstFNDescription)> Private _description As String = ""


        <ormEntryMapping(EntryName:=ConstFNUseForeignKey)> Private _UseForeignkey As otForeignKeyImplementation = otForeignKeyImplementation.None
        <ormEntryMapping(EntryName:=ConstFNColumns)> Private _columnnames As String() = {}
        <ormEntryMapping(EntryName:=ConstFNForeignKeys)> Private _foreignKeys As String() = {}

        <ormEntryMapping(EntryName:=ConstFNForeignKeyProperties)> Private _foreignkeyPropStrings As String() = {}

        '* relation to the Tabledefinition - no cascadeOnUpdate to prevent recursion loops
        <ormRelationAttribute(linkobject:=GetType(TableDefinition), toPrimarykeys:={ConstFNTableName}, createObjectIfnotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRTableDefinition = "table"
        '** the real thing
        <ormEntryMapping(relationName:=constRTableDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnDemand)> _
        Private _Tabledefinition As TableDefinition


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
                    Dim refTableName As String = ""
                    Dim refColumnname As String = ""
                    Dim names = Shuffle.NameSplitter(reference)
                    If names.Count > 1 Then
                        refTableName = names(0)
                        refColumnname = names(1)
                    Else
                        refColumnname = names(0)
                        CoreMessageHandler(message:="an tablename is missing in columnnames reference", arg1:=reference, _
                                           subname:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)

                    End If

                    ' will not take 
                    Dim anReferenceAttribute As ormSchemaTableColumnAttribute = _
                        ot.GetSchemaTableColumnAttribute(columnname:=refColumnname, tablename:=refTableName)
                    If anReferenceAttribute IsNot Nothing Then
                        okflag = okflag And True
                    Else
                        CoreMessageHandler(message:="an table column attribute could not be found in columnnames reference - columnnames not set not set", _
                                           arg1:=reference, tablename:=refTableName, columnname:=refColumnname, _
                                           subname:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)
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
                    Dim refTableName As String = ""
                    Dim refColumnname As String = ""
                    Dim names = Shuffle.NameSplitter(reference)
                    If names.Count > 1 Then
                        refTableName = names(0)
                        refColumnname = names(1)
                    Else
                        refColumnname = names(0)
                        CoreMessageHandler(message:="an tablename is missing in columnnames reference", arg1:=reference, _
                                           subname:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)

                    End If

                    ' will not take 
                    Dim anReferenceAttribute As ormSchemaTableColumnAttribute = _
                        ot.GetSchemaTableColumnAttribute(columnname:=refColumnname, tablename:=refTableName)

                    If anReferenceAttribute IsNot Nothing Then
                        okflag = okflag And True
                    Else
                        CoreMessageHandler(message:="an table column attribute could not be found in columnnames reference - columnnames not set not set", _
                                           arg1:=reference, tablename:=refTableName, columnname:=refColumnname, _
                                           subname:="ForeignkeyDefinition.ColumnNames", messagetype:=otCoreMessageType.InternalError)
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
        Public ReadOnly Property TableDefinition As TableDefinition
            Get
                If _Tabledefinition Is Nothing And _tablename <> "" Then
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
                Dim refObjectName As String = ""
                Dim refObjectEntry As String = ""
                Dim names = Shuffle.NameSplitter(reference)
                If names.Count > 1 Then
                    refObjectName = names(0)
                    refObjectEntry = names(1)
                Else
                    refObjectEntry = names(0)

                    CoreMessageHandler(message:="an objectname is missing in foreign key reference", arg1:=reference, subname:="ForeignkeyDefinition.ForeignKeyReference", messagetype:=otCoreMessageType.InternalError)
                    Return aList
                End If

                ' will not take 
                Dim anReferenceAttribute As ormObjectEntryAttribute = _
                    ot.GetObjectEntryAttribute(entryname:=refObjectEntry, objectname:=refObjectName)

                If anReferenceAttribute IsNot Nothing Then
                    aList.Add(anReferenceAttribute.Tablename & "." & anReferenceAttribute.ColumnName)
                Else
                    CoreMessageHandler(message:="an object entry attribute could not be found in foreign key reference - foreign key reference not set", _
                                       arg1:=reference, objectname:=refObjectName, entryname:=refObjectName, _
                                       subname:="ForeignkeyDefinition.RetrieveColumnnames", messagetype:=otCoreMessageType.InternalError)

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
        Public Function SetByAttribute(attribute As ormSchemaForeignKeyAttribute) As Boolean
            If Not Me.IsAlive(subname:="ForeignKeyDefinition.SetByAttribute") Then
                Return False
            End If

            If Not attribute.HasValueTableName Then
                CoreMessageHandler(message:="attribute has not set tablename ", subname:="ForeignKeyDefinition.setbyAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=Me.ConstObjectID)
                Return False
            End If

            With attribute
                ' If .HasValueID Then Me.Id = .name
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueEntrynames Then Me.ColumnNames = RetrieveColumnnames(.Entrynames).ToArray
                If .HasValueUseForeignKey AndAlso .UseForeignKey <> otForeignKeyImplementation.None Then
                    Me.UseForeignKey = .UseForeignKey
                    If .HasValueForeignKeyReferences Then
                        Me.ForeignKeyReferences = RetrieveColumnnames(.ForeignKeyReferences).ToArray
                    Else
                        CoreMessageHandler(message:="no foreign key references found in attribute - foreign key implementation set to none", _
                                           arg1:=attribute.ID, columnname:=Me.Id, tablename:=Me.Tablename, _
                                            subname:="ColumnDefinition.SetByAttribute", messagetype:=otCoreMessageType.InternalError)
                        Me.UseForeignKey = otForeignKeyImplementation.None
                    End If

                    If .HasValueForeignKeyProperties Then
                        Me.ForeignKeyProperties = .ForeignKeyProperties
                    Else
                        Me.ForeignKeyProperties = {Database.ForeignKeyProperty.OnUpdate & "(" & OnTrack.Database.ForeignKeyActionProperty.Cascade & ")", _
                                                    OnTrack.Database.ForeignKeyProperty.OnDelete & "(" & OnTrack.Database.ForeignKeyActionProperty.Cascade & ")"
                                                   }
                    End If


                End If
            End With
            Return True
        End Function

        ''' <summary>
        ''' load and infuse the object by primary key
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Inject(ByVal tablename As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {tablename.ToUpper, id.ToUpper}
            Return MyBase.Inject(primarykey)
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
            Return Retrieve(Of ForeignKeyDefinition)(pkArray:={tablename.ToUpper, id.ToUpper}, forceReload:=forcereload, runtimeOnly:=runtimeOnly)
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnCreated
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
            Return ormDataObject.CreateDataObject(Of ForeignKeyDefinition)(pkArray:=primarykey, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


    End Class

    ''' <summary>
    ''' definition class Table defintion for an OTDB data object definition
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=IndexDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="index definition for table definitions", _
        isbootstrap:=True, usecache:=True, Version:=1)> _
    Public Class IndexDefinition
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "IndexDefinition"

        '** Table Definition
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID = "tblTableIndexDefinitions"

        '** Indices

        '** Primary key
        <ormObjectEntry(referenceobjectentry:=TableDefinition.ConstObjectID & "." & TableDefinition.ConstFNTablename, primarykeyordinal:=1, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const ConstFNTablename = TableDefinition.ConstFNTablename

        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primarykeyordinal:=2,
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
                         title:="Database Id", description:="id of the index in the database")> Public Const ConstFNDatabaseID = "dbid"

        <ormObjectEntry(defaultvalue:=False, dbdefaultvalue:="0", Datatype:=otDataType.Bool, _
                                  title:="IsUnique", Description:="set if the index is unique")> Public Const ConstFNIsUnique As String = "ISUNIQUE"
        'avoid loops
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID
        '** MAPPINGS
        <ormEntryMapping(entryname:=ConstFNIndexName)> Private _indexname As String = ""
        <ormEntryMapping(entryname:=ConstFNTablename)> Private _tablename As String = ""
        <ormEntryMapping(entryname:=ConstFNColumns)> Private _columnnames As String() = {}
        <ormEntryMapping(entryname:=ConstFNdesc)> Private _description As String = ""
        <ormEntryMapping(entryname:=ConstFNIsPrimary)> Private _isPrimary As Boolean = False
        <ormEntryMapping(entryname:=ConstFNIsUnique)> Private _isUnique As Boolean = False
        <ormEntryMapping(entryname:=ConstFNUPDC)> Private _Version As Long = 0
        <ormEntryMapping(entryname:=ConstFNProperties)> Private _properties As String() = {}
        <ormEntryMapping(entryname:=ConstFNDatabaseID)> Private _dbid As String = ""
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(ConstTableID)
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
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property DatabaseID() As String
            Get
                Return Me._dbid
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNDatabaseID, value:=value)
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
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnCreated
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
            Return ormDataObject.Retrieve(Of IndexDefinition)({tablename.ToUpper, indexname.ToUpper}, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of IndexDefinition)(silent:=silent)
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
            Return ormDataObject.CreateDataObject(Of IndexDefinition)({tablename.ToUpper, indexname.ToUpper}, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
        End Function

        ''' <summary>
        ''' Event Handler on Persisting
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub OnPersisting() Handles MyBase.OnPersisting
            If DatabaseID = "" Then Me.DatabaseID = Me.Name
        End Sub
    End Class

    ''' <summary>
    ''' definition class Table defintion for an OTDB data object definition
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=TableDefinition.ConstObjectID, modulename:=ConstModuleRepository, description:="Relational table definition of a database table", _
        usecache:=True, isbootstrap:=True, Version:=1)> _
    Public Class TableDefinition
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstObjectID = "TableDefinition"

        '** Table Definition
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID = "tblTableDefinitions"

        '** Indices

        '** Primary key
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, primarykeyordinal:=1, properties:={ObjectEntryProperty.Keyword}, _
                         title:="Table", description:="table name for the object")> Public Const ConstFNTablename = "table"

        '** Fields
        <ormObjectEntry(Datatype:=otDataType.Text, size:=100, defaultvalue:="PrimaryKey", properties:={ObjectEntryProperty.Keyword}, _
                         title:="PrimaryKey", description:="primary key name for the table")> Public Const ConstFNPrimaryKey = "primarykey"

        <ormObjectEntry(Datatype:=otDataType.Memo, _
                         title:="Table Description", description:="description of the table")> Public Const ConstFNdesc = "desc"

        <ormObjectEntry(Datatype:=otDataType.[Long], defaultvalue:=1, lowerRange:=0, _
                                  title:="Version", Description:="version counter of updating")> Public Const ConstFNUPDC As String = "updc"

        <ormObjectEntry(Datatype:=otDataType.List, size:=255, _
                                  title:="Properties", Description:="properties on table level")> Public Const ConstFNProperties As String = "properties"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, _
                        title:="Use Cache", Description:="set if the entry is object cached")> Public Const ConstFNUseCache As String = "usecache"

        <ormObjectEntry(Datatype:=otDataType.List, size:=255, _
                        title:="Cache", defaultvalue:="", Description:="cache properties on table level")> Public Const ConstFNCacheProperties As String = "cacheproperties"

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="TableDeleteFlagBehaviour", Description:="set if the object runs the delete per flag behavior")> Public Const ConstFNDeletePerFlag As String = "DeletePerFlag"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="TableSpareFieldsBehaviour", Description:="set if the object has additional spare fields behavior")> Public Const ConstFNSpareFieldsFlag As String = "SpareFields"
        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", _
                                  title:="DomainBehaviour", Description:="set if the object belongs to a domain")> Public Const ConstFNDomainFlag As String = "DomainBehavior"

        'avoid loops nonsense to have that here but it is inherited
        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
                       defaultvalue:=ConstGlobalDomain, dbdefaultvalue:=ConstGlobalDomain, useforeignkey:=otForeignKeyImplementation.None)> _
        Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '*** relations
        <ormRelationAttribute(linkobject:=GetType(ColumnDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNTablename}, toEntries:={ColumnDefinition.ConstFNTableName})> Public Const ConstRColumns = "columns"
        <ormRelationAttribute(linkobject:=GetType(IndexDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNTablename}, toEntries:={ColumnDefinition.ConstFNTableName})> Public Const ConstRIndices = "indices"
        <ormRelationAttribute(linkobject:=GetType(ForeignKeyDefinition), cascadeondelete:=True, cascadeonupdate:=True, _
           fromEntries:={ConstFNTablename}, toEntries:={ForeignKeyDefinition.ConstFNTableName})> Public Const ConstRForeignKeys = "foreignkeys"

        '*** Mapping
        <ormEntryMapping(EntryName:=ConstFNTablename)> Private _tablename As String = ""
        <ormEntryMapping(EntryName:=ConstFNdesc)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormEntryMapping(EntryName:=ConstFNPrimaryKey)> Private _pkname As String = "PrimaryKey"   ' name of Primary Key

        <ormEntryMapping(EntryName:=ConstFNUseCache)> Private _useCache As Boolean
        <ormEntryMapping(EntryName:=ConstFNCacheProperties)> Private _CacheProperties As String() = {}
        <ormEntryMapping(EntryName:=ConstFNDeletePerFlag)> Private _deletePerFlagBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNDomainFlag)> Private _domainBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNSpareFieldsFlag)> Private _SpareFieldsFlagBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNUPDC)> Private _Version As Long = 0

        '* relation mappings
        <ormEntryMapping(RelationName:=ConstRColumns, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
         keyentries:={ColumnDefinition.ConstFNColumnname})> Private _columns As New Dictionary(Of String, ColumnDefinition)

        <ormEntryMapping(RelationName:=ConstRIndices, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
         keyentries:={IndexDefinition.ConstFNIndexName})> Private _indices As New Dictionary(Of String, IndexDefinition)

        <ormEntryMapping(RelationName:=ConstRForeignKeys, infusemode:=otInfuseMode.OnDemand Or otInfuseMode.OnInject, _
        keyentries:={ForeignKeyDefinition.ConstFNID})> Private _foreignkeys As New Dictionary(Of String, ForeignKeyDefinition)

        '** runtime
        Public Event ObjectDefinitionChanged As EventHandler(Of ObjectDefinition.EventArgs)

        '** runtime
        Private _lock As New Object

        '** initialize
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub
#Region "Properties"

        ''' <summary>
        ''' Gets the tablename.
        ''' </summary>
        ''' <value>The tablename.</value>
        Public ReadOnly Property Name() As String
            Get
                Return Me._tablename
            End Get
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
        Public ReadOnly Property Columns As IEnumerable(Of ColumnDefinition)
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
        Public ReadOnly Property ForeignKeys As IEnumerable(Of ForeignKeyDefinition)
            Get
                Return _foreignkeys.Values.ToList
            End Get
        End Property
#End Region

        ''' <summary>
        ''' returns a List of all Tabledefinitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of TableDefinition)
            Return ormDataObject.AllDataObject(Of TableDefinition)()
        End Function
        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnCreated
            Dim myself = TryCast(e.DataObject, TableDefinition)
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
        Public Function SetValuesBy(attribute As ormSchemaTableAttribute) As Boolean
            If Not Me.IsAlive(subname:="TableDefinition.SetValuesBy") Then Return False

            '** set the values of the table definition
            With attribute
                If .HasValueAddDomainBehavior Then Me.DomainBehavior = .AddDomainBehavior
                If .HasValueDeleteFieldBehavior Then Me.DeletePerFlagBehavior = .AddDeleteFieldBehavior
                If .HasValueSpareFields Then Me.SpareFieldsBehavior = .AddSpareFields
                If .HasValueVersion Then Me.Version = .Version
                If .HasValueDescription Then Me.Description = .Description
                If .HasValuePrimaryKey Then Me.PrimaryKey = .PrimaryKey
                If .HasValueUseCache Then Me.UseCache = .UseCache
                If .HasValueCacheProperties Then Me.CacheProperties = .CacheProperties.ToList

                '** Add the Foreign Key Attributes
                For Each aForeignKeyAttribute In .ForeignKeyAttributes
                    Dim aForeignkey As ForeignKeyDefinition = ForeignKeyDefinition.Create(tablename:=Me.Name, id:=aForeignKeyAttribute.ID, checkunique:=True, runtimeOnly:=Me.RunTimeOnly)
                    If aForeignkey Is Nothing Then
                        aForeignkey = ForeignKeyDefinition.Retrieve(tablename:=Me.Name, id:=aForeignKeyAttribute.ID, runtimeOnly:=Me.RunTimeOnly)
                    End If
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
            Dim entry = TryCast(sender, ColumnDefinition)
            If entry IsNot Nothing Then
                'rebuild the primary key if necessary
                'do not take PrimaryKeyOrdial since this might be changed during rebuild
                If e.PropertyName = ColumnDefinition.ConstFNPrimaryKey Then
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
        Public Function AddColumn(entry As ColumnDefinition) As Boolean

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
                tblInfo = CurrentDBDriver.GetTable(Me.Name, createOrAlter:=False)
                If tblInfo Is Nothing Then
                    CoreMessageHandler(message:="table is not created in the database yet - run alter schema first before to AlterSchemaForeignRelations", _
                                        subname:="TableDefinition.AlterSchemaForeignKey", messagetype:=otCoreMessageType.InternalError, _
                                        tablename:=Me.Name)
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
                    If ot.CurrentConnection.DatabaseDriver.GetTableSchema(tableID:=Me.Name, force:=True) Is Nothing Then
                        Call CoreMessageHandler(subname:="TableDefinition.AlterSchemaForeignKey", tablename:=tblInfo.Name, _
                                                message:="Error while setTable in alterSchema")
                    End If
                End If

                Return result
            Catch ex As Exception
                Call CoreMessageHandler(subname:="TableDefinition.AlterSchemaForeignKey", exception:=ex)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' alterSchema changes the Database according the information here
        ''' </summary>
        ''' <param name="addToSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AlterSchema() As Boolean

            Dim tblInfo As Object
            Dim aCollection As New List(Of String)

            If Not IsAlive(subname:="TableDefinition.alterschema") Then Return False

            Try
                '** call to get object
                tblInfo = CurrentDBDriver.GetTable(Me.Name, createOrAlter:=True)

                Dim entrycoll As New SortedList(Of Long, ColumnDefinition)

                '** check which entries to use
                For Each anEntry In _columns.Values
                    If Not anEntry.Position.HasValue OrElse anEntry.Position <= 0 OrElse entrycoll.ContainsKey(anEntry.Position) Then
                        anEntry.Position = entrycoll.Keys.Max + 1
                    End If

                    entrycoll.Add(key:=anEntry.Position, value:=anEntry)
                Next


                ' create or alter fields of each entry
                For Each anEntry In entrycoll.Values
                    If Not CurrentDBDriver.VerifyColumnSchema(columndefinition:=anEntry, silent:=True) Then
                        CurrentDBDriver.GetColumn(tblInfo, anEntry, createOrAlter:=True)
                    End If
                Next

                '** call again to create
                tblInfo = CurrentDBDriver.GetTable(Me.Name, createOrAlter:=True, tableNativeObject:=tblInfo)
                If tblInfo Is Nothing Then Return False

                ' create index
                For Each anIndexdefinition In _indices.Values
                    '** create the index
                    Call CurrentDBDriver.GetIndex(tblInfo, indexdefinition:=anIndexdefinition, createOrAlter:=True)
                Next
                ' save the current version also in the DB paramter Table
                CurrentDBDriver.SetDBParameter(parametername:=ConstPNBSchemaVersion_TableHeader & Me.Name.ToUpper, value:=Me.Version, silent:=True)

                '    ' reset the Table description
                '    ' only if we are connected -> bootstrapping problem
                If CurrentSession.IsRunning Then
                    If ot.CurrentConnection.DatabaseDriver.GetTableSchema(tableID:=Me.Name, force:=True) Is Nothing Then
                        Call CoreMessageHandler(subname:="TableDefinition.alterSchema", tablename:=tblInfo.Name, _
                                                message:="Error while setTable in alterSchema")
                    End If
                End If

                Return True
            Catch ex As Exception
                Call CoreMessageHandler(subname:="TableDefinition.alterSchema", exception:=ex)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Adds an Index to the Table Definition
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddIndex(attribute As ormSchemaIndexAttribute) As Boolean
            ' Nothing

            If Not IsAlive(subname:="TableDefinition.addIndex") Then Return False
            If Not attribute.HasValuePrimaryKey Then attribute.IsPrimaryKey = False
            If Not attribute.HasValueVersion Then attribute.Version = 1
            If Not attribute.HasValueIsUnique Then attribute.IsUnique = False
            If Not attribute.HasValueDescription Then attribute.Description = "index for table " & Me.Name

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
                                 Optional description As String = "", _
                                 Optional isprimarykey As Boolean = False, _
                                 Optional isunique As Boolean = False, _
                                 Optional version As ULong = 1, _
                                 Optional replace As Boolean = False) As Boolean

            Dim fieldList As New List(Of String)
            Dim anEntry As New ColumnDefinition
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
                                   arg1:=indexname, tablename:=Me.Name, objectname:=Me.ConstObjectID, _
                                   subname:="TableDefinition.AddIndex(String...)", messagetype:=otCoreMessageType.InternalWarning)
                    Return True
                Else
                    CoreMessageHandler(message:=" index name already exists with this table definition - might be definied in a root class or name is not unique", _
                                   arg1:=indexname, tablename:=Me.Name, objectname:=Me.ConstObjectID, _
                                   subname:="TableDefinition.AddIndex(String...)", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

            End If

            ' check fields -> should be defined to be an index
            For Each aName In columnnames
                ' check
                If Not _columns.ContainsKey(aName.ToUpper) Then
                    CoreMessageHandler(subname:="TableDefinition.addIndex", _
                                            objectname:=Me.ConstObjectID, arg1:=aName, _
                                            tablename:=Me.Name, message:=" column does not exist in table for building index", _
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
            Dim anIndexDefinition = IndexDefinition.Retrieve(tablename:=Me.Name, indexname:=indexname, runtimeOnly:=Me.RunTimeOnly)
            If anIndexDefinition Is Nothing Then
                anIndexDefinition = IndexDefinition.Create(tablename:=Me.Name, indexname:=indexname, runTimeOnly:=RunTimeOnly)
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
            If Not Me.IsLoaded And Not Me.IsCreated And _pkname = "" Then
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
            If Not Me.IsAlive(subname:="GetPrimaryKeyColumnNames") And _pkname = "" Then
                Return New List(Of String)
            End If

            Return GetIndexFieldNames(_pkname)
        End Function
        ''' <summary>        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetPrimaryKeyEntries() As List(Of ColumnDefinition)
            ' Nothing
            If Not Me.IsAlive(subname:="GetPrimaryKeyEntries") And _pkname = "" Then
                Return New List(Of ColumnDefinition)
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
        Public Function GetIndexEntries(ByVal indexname As String) As List(Of ColumnDefinition)
            Dim aFieldCollection As New List(Of ColumnDefinition)

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
        Public Function GetEntry(columnname As String) As ColumnDefinition

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
                        CoreMessageHandler(message:="double primary key ordinal in column definition found - appended to the end", columnname:=anEntry.Name, _
                                           tablename:=Me.Name, subname:="TableDefinition.OnRelationloaded")
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
        Public Shared Function Retrieve(ByVal tablename As String, Optional dbdriver As iormDatabaseDriver = Nothing, Optional runtimeOnly As Boolean = False) As TableDefinition
            Return ormDataObject.Retrieve(Of TableDefinition)({tablename.ToUpper}, runtimeOnly:=runtimeOnly, dbdriver:=dbdriver)
        End Function

        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of TableDefinition)(silent:=silent)
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
                                ) As TableDefinition
            Return ormDataObject.CreateDataObject(Of TableDefinition)({tablename.ToUpper}, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
        End Function



    End Class


    ''' <summary>
    ''' definition class for the permission rules on a data object
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=ObjectPermission.ConstObjectID, modulename:=ConstModuleRepository, description:="permission rules for object access", _
        version:=1, isbootstrap:=True, usecache:=True)> _
    Public Class ObjectPermission
        Inherits ormDataObject

        Public Const ConstObjectID = "ObjectPermissionRule"

        <ormSchemaTable(version:=1, usecache:=True, adddomainbehavior:=True, adddeletefieldbehavior:=True)> Public Const ConstTableID = "tblObjectPermissions"


        '** Primary key
        <ormObjectEntry(referenceObjectEntry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, primarykeyordinal:=1 _
                       )> Public Const ConstFNObjectname = AbstractEntryDefinition.ConstFNObjectName

        <ormObjectEntry(referenceObjectEntry:=ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNEntryName, primarykeyordinal:=2 _
                        )> Public Const ConstFNEntryname = AbstractEntryDefinition.ConstFNEntryName

        <ormObjectEntry(Datatype:=otDataType.Text, size:=150, primarykeyordinal:=3, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        title:="Operation", description:="business object operation")> Public Const ConstFNOperation = "operation"

        <ormObjectEntry(Datatype:=otDataType.Long, primarykeyordinal:=4, defaultvalue:=10, _
                        title:="Rule Order", description:="ordinal of the rule")> Public Const ConstFNRuleordinal = "order"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=5, _
                       useforeignkey:=otForeignKeyImplementation.None, defaultvalue:=ConstGlobalDomain)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        '** build foreign key
        ' proplematic
        '<ormSchemaForeignKey(entrynames:={ConstFNObjectname, ConstFNEntryname, ConstFNDomainID}, _
        '    foreignkeyreferences:={ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNObjectName, _
        '                           ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNEntryName, _
        '                           ObjectColumnEntry.ConstObjectID & "." & ObjectColumnEntry.ConstFNDomainID}, _
        '                       useforeignkey:=otForeignKeyImplementation.NativeDatabase)> Public Const constFKprimary = "fkpermission"


        <ormSchemaForeignKey(entrynames:={ConstFNObjectname}, _
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
        <ormEntryMapping(entryname:=ConstFNObjectname)> Private _objectname As String = ""
        <ormEntryMapping(entryname:=ConstFNEntryname)> Private _entryname As String = ""
        <ormEntryMapping(entryname:=ConstFNOperation)> Private _operation As String = ""
        <ormEntryMapping(entryname:=ConstFNDomainID)> Private _domainID As String = ""
        <ormEntryMapping(entryname:=ConstFNRuleordinal)> Private _order As Long = 0
        <ormEntryMapping(entryname:=ConstFNRuleType)> Private _ruletype As String = ""
        <ormEntryMapping(entryname:=ConstFNRule)> Private _rule As String = ""
        <ormEntryMapping(entryname:=ConstFNAllow)> Private _allow As Boolean
        <ormEntryMapping(entryname:=ConstFNExitTrue)> Private _exitOnTrue As Boolean
        <ormEntryMapping(entryname:=ConstFNExitFalse)> Private _exitOnFalse As Boolean
        <ormEntryMapping(entryname:=ConstFNdesc)> Private _description As String = ""
        <ormEntryMapping(entryname:=ConstFNVersion)> Private _version As ULong = 0

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
        Public Shared Function ByObjectName(objectname As String, Optional DomainID As String = "") As List(Of ObjectPermission)
            Dim aCollection As New List(Of ObjectPermission)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            '** set the domain
            If DomainID = "" Then DomainID = CurrentSession.CurrentDomainID

            Try
                aStore = GetTableStore(ConstTableID)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="all", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ConstFNIsDeleted & "] = @deleted "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.Where &= " AND [" & ConstFNObjectname & "] = @objectname AND [" & ConstFNEntryname & "] = ''"

                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@objectname", ColumnName:=ConstFNObjectname, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@domainID", value:=DomainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aCommand.SetParameterValue(ID:="@objectname", value:=objectname.ToUpper)

                aRecordCollection = aCommand.RunSelect
                Dim instantDir As New Dictionary(Of String, ObjectPermission)

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aPermission As New ObjectPermission
                    If InfuseDataObject(record:=aRecord, dataobject:=aPermission) Then
                        '** add only the domain asked or if nothing in there
                        Dim key As String = aPermission.Objectname & ConstDelimiter & aPermission.Entryname & ConstDelimiter & aPermission.Operation & ConstDelimiter & aPermission.Order.ToString
                        If instantDir.ContainsKey(key) And aPermission.DomainID = DomainID Then
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

                Call CoreMessageHandler(exception:=ex, subname:="ObjectPermission.ByObjectname")
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
                                         Optional operationname As String = "", Optional entryname As String = "", Optional domainID As String = "", _
                                            Optional checkUnique As Boolean = True, Optional runtimeOnly As Boolean = False) As ObjectPermission
            Dim pkarray As Object() = {objectname.ToUpper, entryname.ToUpper, operationname.ToUpper, order, domainID.ToUpper}
            Return ormDataObject.CreateDataObject(Of ObjectPermission)(pkArray:=pkarray, domainID:=domainID, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
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
                                           Optional operationname As String = "", Optional entryname As String = "", Optional domainID As String = "", _
                                            Optional dbdriver As iormDatabaseDriver = Nothing, Optional runtimeOnly As Boolean = False) As ObjectPermission
            Dim pkarray As Object() = {objectname.ToUpper, entryname.ToUpper, operationname.ToUpper, order, domainID.ToUpper}
            Return ormDataObject.Retrieve(Of ObjectPermission)(pkArray:=pkarray, domainID:=domainID, dbdriver:=dbdriver, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ObjectPermission)(silent:=silent)
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
                CoreMessageHandler(exception:=ex, subname:="ObjectPermission.OnInfused", messagetype:=otCoreMessageType.InternalError)
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
                CoreMessageHandler(exception:=ex, subname:="ObjectPermission.OnInfused", messagetype:=otCoreMessageType.InternalError)
            End Try
        End Sub


        ''' <summary>
        ''' applies the current permission rule on the current user and returns the result
        ''' </summary>
        ''' <param name="user"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckFor([user] As User, ByRef [exit] As Boolean, Optional domainid As String = "") As Boolean
            If Not Me.IsAlive(subname:="CheckFor") Then Return False
            Dim result As Boolean
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

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
                                        CoreMessageHandler(message:="Groupname not found", arg1:=_permissionruleProperty.ToString, _
                                                subname:="ObjectPermission.CheckFor", objectname:=Me.Objectname, messagetype:=otCoreMessageType.InternalError)
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
                        CoreMessageHandler(message:="ObjectPermissionRuleProperty not implemented", arg1:=_permissionruleProperty.ToString, _
                                            subname:="ObjectPermission.CheckFor", objectname:=Me.Objectname, messagetype:=otCoreMessageType.InternalError)
                        result = False 'wrong value -> false

                End Select
                '* exit flag
                If (result AndAlso ExitOnTrue) OrElse (Not result AndAlso _exitOnFalse) Then
                    [exit] = True
                End If
                Return result

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectPermission.Checkfor")
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
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

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
        <ormSchemaTable(version:=1, usecache:=True)> Public Const ConstTableID = "tblObjectDefinitions"
        ''' <summary>
        ''' Index Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaIndex(columnname1:=ConstFNClass)> Public Const ConstIndexName = "name"

        ''' <summary>
        ''' Primary key Column
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, primarykeyordinal:=1, properties:={ObjectEntryProperty.Keyword}, _
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
        <ormObjectEntry(Datatype:=otDataType.List, size:=255, innerDatatype:=otDataType.Text, properties:={ObjectEntryProperty.Keyword}, _
                        title:="Tables", description:="tables of the object")> Public Const ConstFNtablenames = "tables"

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

        <ormRelationAttribute(linkobject:=GetType(ObjectColumnEntry), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNID}, toEntries:={ObjectColumnEntry.ConstFNObjectName}, linkjoin:="AND [" & ObjectColumnEntry.ConstFNType & "] = '" & "Column" & "'")> _
        Public Const ConstRColumnEntries = "ColumnEntries"

        <ormRelationAttribute(linkobject:=GetType(ObjectCompoundEntry), cascadeOnDelete:=True, cascadeOnUpdate:=True, _
            fromEntries:={ConstFNID}, toEntries:={ObjectCompoundEntry.ConstFNObjectName}, linkjoin:="AND [" & ObjectCompoundEntry.ConstFNType & "] = 'Compound'")> _
        Public Const ConstRCompoundEntries = "CompoundEntries"

        <ormEntryMapping(RelationName:=ConstRColumnEntries, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectColumnEntry.ConstFNEntryName})> _
        <ormEntryMapping(RelationName:=ConstRCompoundEntries, infuseMode:=otInfuseMode.OnInject Or otInfuseMode.OnDemand, _
            keyentries:={ObjectCompoundEntry.ConstFNEntryName})> Private WithEvents _objectentries As New Dictionary(Of String, iormObjectEntry) ' by id


        '*** Mapping
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNClass)> Public _class As String = ""
        <ormEntryMapping(EntryName:=ConstFNdesc)> Private _description As String = ""
        <ormEntryMapping(EntryName:=ConstFNProperties)> Private _properties As String() = {}
        <ormEntryMapping(EntryName:=ConstFNModule)> Private _modulename As String = ""
        <ormEntryMapping(EntryName:=ConstFNISActive)> Private _isactive As Boolean = True
        <ormEntryMapping(EntryName:=ConstFNUseCache)> Private _useCache As Boolean
        <ormEntryMapping(EntryName:=ConstFNCacheProperties)> Private _CacheProperties As String()
        <ormEntryMapping(EntryName:=ConstFNDeletePerFlag)> Private _deletePerFlagBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNDomainFlag)> Private _domainBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNSpareFieldsFlag)> Private _SpareFieldsFlagBehavior As Boolean = False
        <ormEntryMapping(EntryName:=ConstFNVersion)> Private _Version As Long = 0
        <ormEntryMapping(EntryName:=ConstFNPrimaryKeys)> Private _pknames As String() = {}
        <ormEntryMapping(EntryName:=ConstFNtablenames)> Private _tablenames As String() = {}
        <ormEntryMapping(EntryName:=ConstFNDefaultPermission)> Private _defaultpermission As Boolean = True

        ''' <summary>
        ''' Relations which will be handled by events
        ''' </summary>
        ''' <remarks></remarks>
        Private _tables As New Dictionary(Of String, TableDefinition) ' relations will be handled by events - list to load stored in _tablenames
        Private _objectpermissions As New Dictionary(Of String, SortedList(Of Long, ObjectPermission)) 'ObjectPermissions by Operation and the sorted rules list

        Public Shared Event ObjectDefinitionChanged As EventHandler(Of EventArgs)
        Public Shared Event OnObjectSchemaCreating(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event OnObjectSchemaCreated(sender As Object, e As ormDataObjectEventArgs)

        '** runtime variables
        Private _lock As New Object
        Private _DefaultDomainID As String = ""
        Private _isBootStrappingObject As Nullable(Of Boolean)

        '**** OPERATIONS

        '** initialize
        Public Sub New()
            Call MyBase.New(ConstTableID)

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
                Return Me._pknames
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
        Public ReadOnly Property Tables() As List(Of TableDefinition)
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
        ''' returns all Entries
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
        ''' gets a collection of object Entry definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntries(Optional onlyActive As Boolean = True) As IList(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.GetEntries") Then
                If onlyActive Then Return _objectentries.Values.Where(Function(x) x.IsActive = True).ToList
                Return _objectentries.Values.ToList
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
        Public Function GetOrderedEntries(Optional onlyActive As Boolean = True) As IOrderedEnumerable(Of iormObjectEntry)
            If Me.IsAlive(subname:="ObjectDefinition.Entries") Then
                If onlyActive Then Return _objectentries.Values.Where(Function(x) x.IsActive = True).ToList.OrderBy(Function(x) x.Ordinal)
                Return _objectentries.Values.ToList.OrderBy(Function(x) x.Ordinal)
            Else
                Dim aList As New List(Of iormObjectEntry)
                Return aList.OrderBy(Function(x) x.Ordinal)
            End If
        End Function


        ''' <summary>
        ''' OnCreated handles the creation event - set 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCreated(sender As Object, e As ormDataObjectEventArgs) Handles MyBase.ClassOnCreated
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
                If .HasValuePrimaryKeys Then Me._pknames = .PrimaryKeys
                If .HasValueTablenames Then Me.Tablenames = .Tablenames
                If .HasValueUseCache Then Me.UseCache = .UseCache
                If .HasValueCacheProperties Then Me.CacheProperties = .CacheProperties.ToList
                If .HasValueDefaultPermission Then Me.DefaultPermission = .DefaultPermission
                If .HasValueModulename Then Me.Modulename = .Modulename
            End With

            Return True
        End Function
        ''' <summary>
        ''' sets the values by attributes
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddPermissionRule(attribute As ormObjectTransactionAttribute, Optional runtimeOnly As Boolean = False, Optional domainID As String = "") As Boolean
            If Not IsAlive(subname:="AddPermissionRule") Then Return False

            '** bootstrap
            If Not runtimeOnly Then runtimeOnly = CurrentSession.IsBootstrappingInstallationRequested
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

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
                                                                                domainID:=domainID, checkUnique:=True, runtimeOnly:=runtimeOnly)

                        Try
                            aRule.RuleProperty = New ObjectPermissionRuleProperty([property])
                            If .HasValueDefaultAllowPermission Then aRule.Allow = attribute.DefaultAllowPermission
                            If .HasValueVersion Then aRule.Version = attribute.Version
                            If .HasValueDescription Then aRule.Description = attribute.Description


                            permissions.Add(key:=aRule.Order, value:=aRule)

                            '** add handlers
                            AddHandler MyBase.OnSwitchRuntimeOff, AddressOf aRule.OnSwitchRuntimeOff

                        Catch ex As Exception
                            CoreMessageHandler(exception:=ex, subname:="ObjectDefinition.AddPermissionRule", arg1:=[property])
                            Return False
                        End Try


                    Next
                Else
                    CoreMessageHandler(message:="Attribute has no operationname or no rules", subname:="ObjectDefinition.AddPermissionRule", _
                                       messagetype:=otCoreMessageType.InternalWarning, objectname:=Me.ObjectID, arg1:=attribute)
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
        Public Function AddTable(attribute As ormSchemaTableAttribute, Optional runtimeOnly As Boolean = False) As Boolean
            Dim aTableDefinition As TableDefinition

            '** bootstrap
            If Not runtimeOnly Then runtimeOnly = CurrentSession.IsBootstrappingInstallationRequested

            If attribute.TableName Is Nothing OrElse attribute.TableName = "" Then
                CoreMessageHandler(message:="attribute need a non-empty table name", objectname:=Me.ID, _
                                   messagetype:=otCoreMessageType.InternalError, subname:="ObjectDefinition.AddTableByAttribute")
                Return False
            End If

            '* does the table exist in the object
            If _tables.ContainsKey(key:=attribute.TableName) Then
                '**
                aTableDefinition = _tables.Item(key:=attribute.TableName)
            Else

                aTableDefinition = TableDefinition.Retrieve(tablename:=attribute.TableName, runtimeOnly:=runtimeOnly)
                If aTableDefinition Is Nothing Then
                    aTableDefinition = TableDefinition.Create(tablename:=attribute.TableName, checkunique:=True, runTimeOnly:=runtimeOnly)
                End If

                _tables.Add(key:=attribute.TableName, value:=aTableDefinition)
            End If

            ''' check if table is also listed in the relation field
            ''' 
            If _tablenames Is Nothing Then
                ReDim _tablenames(0)
                _tablenames(0) = attribute.TableName
            ElseIf Not _tablenames.Contains(attribute.TableName) Then
                ReDim Preserve _tablenames(_tablenames.GetUpperBound(0) + 1)
                _tablenames(_tablenames.GetUpperBound(0)) = attribute.TableName
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
        Public Function AddEntry(attribute As ormObjectEntryAttribute, Optional runtimeOnly As Boolean = False, Optional domainid As String = "") As Boolean
            Dim anEntry As iormObjectEntry
            Dim bootstrap As Boolean = runtimeOnly

            If Not attribute.HasValueEntryName Then
                CoreMessageHandler(message:="attribute as no entry name", subname:="ObjectDefinition.AddEntryByAttribute(ormEntryAttribute", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=_id)
                Return False
            End If

            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

            If _objectentries.ContainsKey(key:=attribute.EntryName) Then
                '**
                anEntry = _objectentries.Item(key:=attribute.EntryName)
            Else
                '''
                ''' the entries are added by event handler of the abstract entry
                If attribute.EntryType = otObjectEntryType.Column Then
                    anEntry = ObjectColumnEntry.Retrieve(objectname:=Me.ID, entryname:=attribute.EntryName, runtimeOnly:=bootstrap)
                    If anEntry Is Nothing Then
                        anEntry = ObjectColumnEntry.Create(objectname:=Me.ID.Clone, entryname:=attribute.EntryName.Clone, _
                                                           tablename:=attribute.Tablename.Clone, columnname:=attribute.ColumnName.Clone, _
                                                           checkunique:=True, domainID:=domainid, runtimeOnly:=bootstrap)
                    End If
                    '*** add the switchoff handler
                    AddHandler MyBase.OnSwitchRuntimeOff, AddressOf anEntry.OnswitchRuntimeOff

                ElseIf attribute.EntryType = otObjectEntryType.Compound Then
                    anEntry = ObjectCompoundEntry.Retrieve(objectname:=Me.ID, entryname:=attribute.EntryName, runtimeOnly:=bootstrap)
                    If anEntry Is Nothing Then
                        anEntry = ObjectCompoundEntry.Create(objectname:=Me.ID, entryname:=attribute.EntryName, checkunique:=True, runtimeOnly:=bootstrap)
                    End If

                Else
                    CoreMessageHandler(message:="EntryType of object entry attribute is unknown to create", subname:="ObjectDefinition.AddEntry(attribute)", _
                                        messagetype:=otCoreMessageType.InternalError, objectname:=attribute.ObjectName, entryname:=attribute.EntryName)
                    Return False
                End If
            End If

            '** set the entry according to the Attribute
            Return anEntry.AbstractEntryDefinition_SetByAttribute(attribute)

        End Function




        ''' <summary>
        ''' creates the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = False) As Boolean
            Return ormDataObject.CreateDataObjectSchema(Of ObjectDefinition)(silent:=silent)
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
                CoreMessageHandler(message:="object was not found by type", arg1:=objecttype.Name, objectname:=objecttype.Name, _
                                  subname:="objectdefinition.CreateObjectSchema(Shared)", messagetype:=otCoreMessageType.InternalError)
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
            '** fire event
            Dim ourEventArgs As New ormDataObjectEventArgs([object]:=Nothing)
            RaiseEvent OnObjectSchemaCreating(Nothing, e:=ourEventArgs)
            If ourEventArgs.AbortOperation Then
                Return False
            End If

            '*** create the tables -> creates the columns -> creates the indices
            For Each aTableDefinition In Me.Tables
                If aTableDefinition.AlterSchema() Then
                    result = result And True
                Else
                    result = result And False
                End If
            Next

            '** fire event
            ourEventArgs = New ormDataObjectEventArgs([object]:=Me)
            RaiseEvent OnObjectSchemaCreated(Nothing, e:=ourEventArgs)

            Return result
        End Function
        ''' <summary>
        ''' fills a object definition by attributes from ObjectClassDescription
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetupByClassDescription(objecttype As System.Type, Optional runtimeOnly As Boolean = False) As Boolean
            If objecttype Is Nothing Then
                CoreMessageHandler(message:="failed : object type is nothing", _
                                  subname:="objectdefinition.SetupByClassDescription(Shared)", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescription(type:=objecttype)
            Dim bootstrap As Boolean = runtimeOnly

            If objecttype.Equals(GetType(Configurables.ConfigItemSelector)) Then
                Debug.WriteLine("")
            End If
            If anObjectDescription Is Nothing Then
                CoreMessageHandler(message:="object was not found by type", arg1:=objecttype.Name, objectname:=objecttype.Name, _
                                  subname:="objectdefinition.SetupByClassDescription(Shared)", messagetype:=otCoreMessageType.InternalError)
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
                If Not anIndexAttribute.HasValueTableName Then
                    If Me.Tablenames.Count = 1 Then
                        anIndexAttribute.TableName = Me.Tablenames.FirstOrDefault
                    Else
                        CoreMessageHandler(message:="ambiguous index attribute has no table name property and oject has more than one table - index not created", _
                                           objectname:=Me.ID, arg1:=anIndexAttribute.IndexName, messagetype:=otCoreMessageType.InternalError, _
                                           subname:="objectdefinition.SetupByClassDescription(Type)")
                        Exit For
                    End If
                End If
                If Me.HasTable(anIndexAttribute.TableName) Then
                    '** add Index to table definition
                    '** no runTimeOnly since the AddIndex is getting this from the table
                    Me.GetTable(anIndexAttribute.TableName).AddIndex(anIndexAttribute)
                Else
                    CoreMessageHandler(message:="table name of index is not assigned to object definition - index not created", _
                                           objectname:=Me.ID, arg1:=anIndexAttribute.IndexName, tablename:=anIndexAttribute.TableName, _
                                           messagetype:=otCoreMessageType.InternalError, _
                                           subname:="objectdefinition.SetupByClassDescription(Type)")
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

            Return _pknames.Count
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key entry names
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetKeyNames() As List(Of String)
            If Not IsAlive(subname:="GetKeyNames") OrElse _pknames.Count = 0 Then Return New List(Of String)
            Return _pknames.ToList
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
            For Each aName In Me.GetKeyNames
                If _objectentries.ContainsKey(aName) Then
                    aList.Add(_objectentries.Item(aName))
                Else
                    CoreMessageHandler(message:="key name of object is not in the entries dictionary", messagetype:=otCoreMessageType.InternalError, _
                                        subname:="ObjectDefinition.GetKeyEntries", arg1:=aName, objectname:=Me.ID)
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
            If sender.GetType.Equals(GetType(ObjectColumnEntry)) Then
                Dim anEntry As ObjectColumnEntry = TryCast(sender, ObjectColumnEntry)
                If anEntry IsNot Nothing AndAlso e.PropertyName = ColumnDefinition.ConstFNPrimaryKey Then
                    ''' HACK ! just add up the primary keys - neglect if deleted or primarykey ordinal in table 
                    ''' 
                    If anEntry.IsPrimaryKey Then
                        If Not _pknames.Contains(anEntry.Entryname) Then
                            ReDim Preserve _pknames(_pknames.GetUpperBound(0) + 1)
                            _pknames(_pknames.GetUpperBound(0)) = anEntry.Entryname
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
                                    subname:="ObjectDefinition.AddEntry", messagetype:=otCoreMessageType.InternalWarning)
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
            AddHandler entry.PropertyChanged, AddressOf ObjectDefinition_OnEntryChanged

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
                    Return _objectentries.Item(key:=entryname.ToUpper).isAlive
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
        Public Function GetTable(tablename As String) As TableDefinition
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
        Public Function GetIndexAttribute(name As String) As ormSchemaIndexAttribute
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
        Public Shared Function AllActiveObjectNames(Optional ByRef dbdriver As iormDatabaseDriver = Nothing, Optional domainID As String = "") As List(Of String)

            Dim aCollection As New List(Of String)
            Dim aRecordCollection As List(Of ormRecord)
            Dim aStore As iormDataStore
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                If dbdriver Is Nothing Then
                    aStore = GetTableStore(ObjectDefinition.ConstTableID)
                Else
                    aStore = dbdriver.GetTableStore(ObjectDefinition.ConstTableID)
                End If

                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allObjects", addAllFields:=False)
                If Not aCommand.Prepared Then
                    aCommand.select = "DISTINCT " & ConstFNID
                    aCommand.Where = ConstFNIsDeleted & " = @deleted "
                    aCommand.Where = ConstFNISActive & " = @isactive "
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ConstFNIsDeleted, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@isactive", ColumnName:=ConstFNISActive, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.Prepare()
                End If

                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aCommand.SetParameterValue(ID:="@isactive", value:=True)
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    If Not aCollection.Contains(aRecord.GetValue(1).toupper) Then
                        aCollection.Add(aRecord.GetValue(1).toupper)
                    End If
                Next

                Return aCollection
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="ObjectDefinition.AllActiveObjectnames")
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
                                        Optional domainID As String = "", _
                                        Optional dbdriver As iormDatabaseDriver = Nothing, _
                                        Optional runtimeOnly As Boolean = False,
                                        Optional forceReload As Boolean = False) As ObjectDefinition
            Return Retrieve(Of ObjectDefinition)(pkArray:={objectname.ToUpper}, domainID:=domainID, dbdriver:=dbdriver, runtimeOnly:=runtimeOnly, forceReload:=forceReload)
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
                    Dim aColumnEntry = TryCast(anEntry, ObjectColumnEntry)
                    If aColumnEntry IsNot Nothing Then
                        If Not _tables.ContainsKey(aColumnEntry.TableName) Then
                            Dim aTable As TableDefinition = TableDefinition.Retrieve(tablename:=aColumnEntry.TableName, runtimeOnly:=Me.RunTimeOnly)
                            If aTable IsNot Nothing Then
                                _tables.Add(key:=aColumnEntry.TableName, value:=aTable)
                                If Not theTablenamesList.Contains(aColumnEntry.TableName) Then theTablenamesList.Add(aTable.Name)
                            End If
                        End If
                    End If
                End If
            Next

            ''' add the tables definied in the list but not elsethere (error condition ?!)
            ''' 
            For Each aName In theTablenamesList
                If Not _tables.ContainsKey(aName) Then
                    Dim aTable As TableDefinition = TableDefinition.Retrieve(tablename:=aName, runtimeOnly:=Me.RunTimeOnly)
                    If aTable IsNot Nothing Then
                        _tables.Add(key:=aName, value:=aTable)
                        If Not theTablenamesList.Contains(aName) Then theTablenamesList.Add(aTable.Name)
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
                If anEntry IsNot Nothing Then anEntry.IsActive = Me.HasDomainBehavior

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
                                Optional domainID As String = "",
                                Optional runTimeOnly As Boolean = False, _
                                Optional checkunique As Boolean = True, _
                                Optional version As UShort = 1) As ObjectDefinition

            Return ormDataObject.CreateDataObject(Of ObjectDefinition)({objectID.ToUpper}, domainID:=domainID, checkUnique:=checkunique, runtimeOnly:=runTimeOnly)
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
        Public Overloads Function GetQuery(name As String, Optional domainid As String = "") As iormQueriedEnumeration
            ''' function gets a queried enumeration mostly from the attribute unless we have no 
            ''' query objects in the core
            If Not Me.IsAlive(subname:="Objectdefinition.GetQuery") Then Return Nothing

            Dim aDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ID)
            If aDescription Is Nothing Then
                Call CoreMessageHandler(message:="data object class description cannot be retrieved", _
                                       objectname:=Me.Classname, arg1:=name, _
                                       subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            Dim anObjectID As String = Me.ID
            Dim type As System.Type = System.Type.GetType(Me.Classname, throwOnError:=False, ignoreCase:=True)
            If type Is Nothing Then
                Call CoreMessageHandler(message:="type cannot be retrieved from reflection", _
                                           objectname:=Me.Classname, arg1:=name, _
                                           subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
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
            If domainid = "" Then domainid = ConstGlobalDomain

            '** check on the operation right for this object for the current username (might be that during session startup otdb username is not set)
            If Not CurrentSession.IsStartingUp AndAlso Not ot.GetBootStrapObjectClassIDs.Contains(anObjectID) _
                AndAlso Not CurrentSession.ValidateAccessRights(accessrequest:=otAccessRight.ReadOnly, domainid:=domainid, _
                                                                objecttransactions:={anObjectID & "." & ConstOPInject}) Then
                '** request authorizartion
                If Not CurrentSession.RequestUserAccess(accessRequest:=otAccessRight.ReadOnly, domainID:=domainid, _
                                                                            username:=CurrentSession.Username, _
                                                                            objecttransactions:={anObjectID & "." & ConstOPInject}) Then
                    Call CoreMessageHandler(message:="data object cannot be retrieved - permission denied to user", _
                                            objectname:=anObjectID, arg1:=ConstOPInject, username:=CurrentSession.Username, _
                                            subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If

            '** get the store for the primary table 
            Dim aStore As iormDataStore = Me.DatabaseDriver.GetTableStore(tableID:=aDescription.PrimaryTable)
            If aStore Is Nothing Then
                Call CoreMessageHandler(message:="table store cannot be retrieved", _
                                           objectname:=anObjectID, arg1:=name, tablename:=aDescription.PrimaryTable, _
                                           subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            ''' get the Select-Command
            Dim aSelectCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(name)

            ''' prepare the command with the specials
            ''' 
            If Not aSelectCommand.Prepared Then
                Dim aQryAttribute As ormObjectQueryAttribute = aDescription.GetQueryAttribute(name:=name)
                Dim where As String
                Dim orderby As String
                Dim fieldnames As New List(Of String)
                Dim addallfields As Boolean

                If aQryAttribute Is Nothing Then
                    Call CoreMessageHandler(message:="query attribute could not be retrieved", _
                                           objectname:=anObjectID, arg1:=name, _
                                           subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    If aQryAttribute.HasValueWhere Then
                        where = aQryAttribute.Where
                    Else
                        where = ""
                    End If
                    If aQryAttribute.HasValueOrderBy Then
                        orderby = aQryAttribute.Orderby
                    Else
                        orderby = ""
                    End If
                    If aQryAttribute.HasValueAddAllFields Then addallfields = aQryAttribute.AddAllFields
                    If aQryAttribute.HasValueEntrynames Then
                        Call CoreMessageHandler(message:="retrieving entry names not yet implemented", _
                                         objectname:=anObjectID, arg1:=name, _
                                         subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If
                End If
                Dim hasDomainBehavior As Boolean
                Dim hasDeleteBehavior As Boolean

                ''' this returns only a definition if it was previously loaded
                ''' 
                If CurrentSession.IsBootstrappingInstallationRequested _
                  OrElse ot.GetBootStrapObjectClassnames.Contains(Me.Classname.ToUpper) Then
                    hasDomainBehavior = Me.ObjectHasDomainBehavior
                    hasDeleteBehavior = Me.ObjectHasDeletePerFlagBehavior
                Else
                    hasDomainBehavior = aDescription.ObjectAttribute.AddDomainBehavior
                    hasDeleteBehavior = aDescription.ObjectAttribute.AddDeleteFieldBehavior
                End If

                Dim primaryTablename As String = aDescription.PrimaryTable

                ''' add tables
                ''' 
                aSelectCommand.AddTable(primaryTablename, addAllFields:=addallfields)

                ''' build domain behavior and deleteflag
                ''' 
                If hasDomainBehavior Then
                    If domainid = "" Then domainid = CurrentSession.CurrentDomainID
                    ''' add where
                    If Not String.IsNullOrWhiteSpace(where) Then where &= " AND "
                    where &= String.Format(" ([{0}] = @{0} OR [{0}] = @Global{0})", ConstFNDomainID)
                    ''' add parameters
                    If aSelectCommand.Parameters.Find(Function(x)
                                                          Return x.ID.ToUpper = "@" & ConstFNDomainID.ToUpper
                                                      End Function) Is Nothing Then
                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tablename:=primaryTablename, value:=domainid)
                                       )
                    End If
                    If aSelectCommand.Parameters.Find(Function(x)
                                                          Return x.ID.ToUpper = "@Global" & ConstFNDomainID.ToUpper
                                                      End Function
                                      ) Is Nothing Then
                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@Global" & ConstFNDomainID, columnname:=ConstFNDomainID, _
                                                                  tablename:=primaryTablename, value:=ConstGlobalDomain)
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

                        aSelectCommand.AddParameter(New ormSqlCommandParameter(id:="@" & ConstFNIsDeleted, columnname:=ConstFNIsDeleted, tablename:=primaryTablename, _
                                                                  value:=False)
                                       )
                    End If
                End If

                ''' set the parameters
                aSelectCommand.Where = where
                aSelectCommand.OrderBy = orderby

                If Not aSelectCommand.Prepare() Then
                    Call CoreMessageHandler(message:="the select command could not be prepared", _
                                          objectname:=anObjectID, arg1:=name, _
                                          subname:="ObjectDefinition.GetQuery", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

            End If

            ''' return a new Queries enumeration with the embedded command
            Dim aQE As ormQueriedEnumeration = New ormQueriedEnumeration(type:=type, command:=aSelectCommand, id:=Me.ID & "." & name)


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
        Inherits ormDataObject
        Implements iormPersistable, iormInfusable, iormObjectEntry

        ''' <summary>
        ''' Object ID
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstObjectID = "ObjectEntry"

        ''' <summary>
        ''' Table Definition
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTableAttribute(Version:=5, usecache:=True, adddeletefieldbehavior:=True, addDomainBehavior:=True)> Public Const ConstTableID = "tblObjectEntries"

        ''' <summary>
        ''' Table Index Definitions
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaIndexAttribute(ColumnName1:=ConstFNxid)> Public Const ConstIndexXID = "XID" ' not unqiue
        <ormSchemaIndexAttribute(columnName1:=ConstFNDomainID, ColumnName2:=ConstFNxid)> Public Const ConstIndDomain = "Domain"
        <ormSchemaIndex(columnname1:=ConstFNObjectName, columnname2:=ConstFNType, columnname3:=ConstFNIsDeleted, columnname4:=ConstFNEntryName)> Public Const constINDtypes = "indexTypes"

        ''' <summary>
        ''' Primary Key Columns
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceobjectentry:=ObjectDefinition.ConstObjectID & "." & ObjectDefinition.ConstFNID, primaryKeyordinal:=1, _
            useforeignkey:=otForeignKeyImplementation.NativeDatabase)> _
        Public Const ConstFNObjectName As String = ObjectDefinition.ConstFNID

        <ormObjectEntry(dbdefaultvalue:="", Datatype:=otDataType.Text, size:=100, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        xid:="OED1", title:="Object Entry Name", Description:="entry (data slot) name of an Ontrack Object", primaryKeyordinal:=2)> _
        Public Const ConstFNEntryName As String = "entry"

        <ormObjectEntry(referenceobjectentry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, primarykeyordinal:=3)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ''' <summary>
        ''' Column Definitions
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormObjectEntry(Datatype:=otDataType.Text, defaultvalue:=otObjectEntryType.Column, size:=50, _
                       properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                       xid:="OED3", title:="Entry Type", Description:="OTDB schema entry type")> Public Const ConstFNType As String = "typeid"


        <ormObjectEntry(defaultvalue:=otDataType.Text, dbdefaultvalue:="3", Datatype:=otDataType.Long, _
                        xid:="OED11", title:="Datatype", Description:="OTDB field data type")> Public Const ConstFNDatatype As String = "datatype"

        <ormObjectEntry(Datatype:=otDataType.Text, isnullable:=True, _
                        title:="Inner Datatype", Description:="OTDB inner list data type")> Public Const ConstFNInnerDatatype As String = "innertype"

        <ormObjectEntry(referenceObjectentry:=ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNSize, _
                        xid:="OED13", Description:="max Length of the entry")> Public Const ConstFNSize As String = "size"

        <ormObjectEntry(Datatype:=otDataType.Long, defaultvalue:=1, dbdefaultvalue:="1", _
                         xid:="OED14", title:="Ordinal", Description:="ordinal of the object entry")> Public Const ConstFNordinal As String = "ordinal"

        <ormObjectEntry(referenceObjectentry:=ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNIsNullable, _
                           xid:="OED15", Description:="is nullable on the object entry level")> Public Const ConstFNIsNullable As String = "isnullable"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, isnullable:=True, _
                        xid:="OED16", title:="default value", description:="default value of the object entry on the object level")> _
        Public Const ConstFNDefaultValue As Object = "defaultvalue"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, isnullable:=True, properties:={ObjectEntryProperty.Keyword}, _
                        xid:="OED21", title:="XChangeID", Description:="ID for XChange manager")> Public Const ConstFNxid As String = "XID"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, defaultvalue:="", properties:={ObjectEntryProperty.Capitalize, ObjectEntryProperty.Trim}, _
                        xid:="OED22", title:="Title", Description:="title for column headers of the field")> Public Const ConstFNTitle As String = "title"

        <ormObjectEntry(Datatype:=otDataType.Memo, properties:={ObjectEntryProperty.Trim}, isnullable:=True, _
                        xid:="OED23", title:="Description", Description:="Description of the field")> Public Const ConstFNDescription As String = "desc"

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
        <ormEntryMapping(EntryName:=ConstFNxid)> Protected _xid As String 'nullable
        <ormEntryMapping(EntryName:=ConstFNObjectName)> Protected _objectname As String = ""
        <ormEntryMapping(EntryName:=ConstFNDatatype)> Protected _datatype As otDataType = 0
        <ormEntryMapping(EntryName:=ConstFNInnerDatatype)> Protected _innerdatatype As otDataType = 0
        <ormEntryMapping(EntryName:=ConstFNSize)> Protected _size As Long?
        <ormEntryMapping(EntryName:=ConstFNordinal)> Protected _ordinal As Long = 0
        <ormEntryMapping(EntryName:=ConstFNReadonly)> Protected _readonly As Boolean
        <ormEntryMapping(EntryName:=ConstFNActive)> Protected _active As Boolean
        <ormEntryMapping(EntryName:=ConstFNIsNullable)> Protected _isnullable As Boolean
        <ormEntryMapping(EntryName:=ConstFNDefaultValue)> Protected _defaultvalue As Object
        <ormEntryMapping(EntryName:=ConstFNEntryName)> Protected _entryname As String = ""
        <ormEntryMapping(EntryName:=ConstFNRelation)> Protected _Relation As String()
        <ormEntryMapping(EntryName:=ConstFNProperties)> Protected _propertystrings() As String
        <ormEntryMapping(EntryName:=ConstFNalias)> Protected _aliases As String()
        <ormEntryMapping(EntryName:=ConstFNTitle)> Protected _title As String
        <ormEntryMapping(EntryName:=ConstFNUPDC)> Protected _version As Long = 0
        <ormEntryMapping(EntryName:=ConstFNDescription)> Protected _Description As String
        <ormEntryMapping(Entryname:=ConstFNType)> Protected _typeid As otObjectEntryType
        <ormEntryMapping(entryname:=ConstFNValidate)> Protected _validate As Boolean = False
        <ormEntryMapping(entryname:=ConstFNRender)> Protected _render As Boolean = False
        <ormEntryMapping(entryname:=ConstFNValues)> Protected _listOfValues As List(Of String) = New List(Of String)
        <ormEntryMapping(entryname:=ConstFNLookupProperties)> Protected _LookupPropertyStrings As String()
        <ormEntryMapping(entryname:=ConstFNLookup)> Protected _lookupcondition As String
        <ormEntryMapping(entryname:=ConstFNLowerRange)> Protected _lowerRangeValue As Long?
        <ormEntryMapping(entryname:=ConstFNUpperRange)> Protected _upperRangeValue As Long?
        <ormEntryMapping(entryname:=ConstFNRenderRegexMatch)> Protected _renderRegexMatch As String
        <ormEntryMapping(entryname:=ConstFNRenderRegexPattern)> Protected _renderRegexPattern As String
        <ormEntryMapping(entryname:=ConstFNValidationRegex)> Protected _validateRegexMatch As String
        <ormEntryMapping(entryname:=ConstFNValidationProperties)> Protected _validationPropertyStrings As String()
        <ormEntryMapping(entryname:=ConstFNRenderProperties)> Protected _renderPropertyStrings As String()

        '** events
        'Public Shadows Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)

        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements iormObjectEntry.PropertyChanged

        ''' <summary>
        ''' dynamic members
        ''' </summary>
        ''' <remarks></remarks>
        Private _properties As New List(Of ObjectEntryProperty)
        Private _renderProperties As New List(Of RenderProperty)
        Private _runTimeOnly As Boolean = False 'dynmaic
        Private _validateProperties As New List(Of ObjectValidationProperty)
        Private _lookupProperties As New List(Of LookupProperty)
        'Protected _objectDefintion As ObjectDefinition leads to loops if loaded on infused

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            registerHandler()
        End Sub

#Region "Properties"
        ''' <summary>
        ''' returns True if object entry is mapped to a field member
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsMapped As Boolean Implements iormObjectEntry.IsMapped
            Get
                Dim aDescription = ot.GetObjectClassDescriptionByID(Me.Objectname)
                If aDescription IsNot Nothing Then
                    If aDescription.GetEntryFieldInfos(entryname:=Me.Entryname).Count > 0 Then Return True
                End If
                Return False
            End Get
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
                Return (_validateRegexMatch IsNot Nothing AndAlso _validateRegexMatch <> "")
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
                Return (_lookupcondition IsNot Nothing AndAlso _lookupcondition <> "")
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
                Return (_lookupcondition IsNot Nothing AndAlso _lookupcondition <> "")
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
                            CoreMessageHandler(message:="CTypeDynmaic failed on default value for type " & _datatype.ToString, arg1:=_defaultvalue, subname:="AbstractEntryDefinition.DefaultValue", messagetype:=otCoreMessageType.InternalError, _
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
        Public ReadOnly Property Objectname As String Implements iormObjectEntry.Objectname
            Get
                Objectname = _objectname
            End Get
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
        Public ReadOnly Property Entryname As String Implements iormObjectEntry.Entryname
            Get
                Return _entryname
            End Get
        End Property

        ''' <summary>
        ''' sets or gets the type OTDBSchemaDefTableEntryType of the field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Typeid As otObjectEntryType Implements iormObjectEntry.Typeid
            Get
                Typeid = Me._typeid

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
                If _typeid = otObjectEntryType.Column Then IsColumn = True
            End Get
            Set(value As Boolean)
                CoreMessageHandler(message:="Property IsField is not changeable", subname:="ObjectEntryDefinition.IsField", messagetype:=otCoreMessageType.InternalError, objectname:=Me.Objectname)
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
                CoreMessageHandler(message:="Property isCompound is not changeable", subname:="ObjectEntryDefinition.isCompound", messagetype:=otCoreMessageType.InternalError, objectname:=Me.Objectname)
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
                If _aliases Is Nothing Then Return {}
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
                Properties = _properties
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
                Title = _title
            End Get
            Set(value As String)
                SetValue(entryname:=ConstFNTitle, value:=value)
            End Set
        End Property

#End Region

        ''' <summary>
        ''' register all Event Handlers
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub registerHandler()
            AddHandler ormDataObject.OnCreated, AddressOf Me.AbstractEntryDefinition_OnCreated
            AddHandler ormDataObject.OnCreating, AddressOf Me.AbstractEntryDefinition_OnCreating
            AddHandler ormDataObject.OnInfused, AddressOf Me.AbstractEntryDefinition_OnInfused
            AddHandler ormDataObject.OnEntryChanged, AddressOf Me.AbstractEntryDefinition_OnEntryChanged
        End Sub
        ''' <summary>
        ''' deregister Event Handlers
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub deregisterHandler()
            RemoveHandler ormDataObject.OnCreated, AddressOf Me.AbstractEntryDefinition_OnCreated
            RemoveHandler ormDataObject.OnCreating, AddressOf Me.AbstractEntryDefinition_OnCreating
            RemoveHandler ormDataObject.OnInfused, AddressOf Me.AbstractEntryDefinition_OnInfused
            RemoveHandler ormDataObject.OnEntryChanged, AddressOf Me.AbstractEntryDefinition_OnEntryChanged
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
        ''' set the properties of an ObjectEntryDefinition by a SchemaColumnAttribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function AbstractEntryDefinition_SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean Implements iormObjectEntry.AbstractEntryDefinition_SetByAttribute
            If Not IsAlive(subname:="SetByAttribute") Then Return False


            With attribute

                '** Slot Entry Properties
                If .HasValueXID Then Me.XID = .XID

                If .HasValueIsReadonly Then Me.IsReadonly = .IsReadOnly
                If .HasValueIsActive Then Me.IsActive = .IsActive
                If .HasValueDescription Then Me.Description = .Description
                If .HasValueDataType Then Me.Datatype = .DataType
                If .HasValueInnerDataType Then Me.InnerDatatype = .InnerDataType
                If .hasValuePosOrdinal Then Me.Ordinal = .Posordinal
                If .HasValueSize Then Me.Size = .Size
                If .HasValueDefaultValue Then Me.Defaultvalue = .DefaultValue
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryKeyOrdinal
                If .HasValueTitle Then Me.Title = .Title
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
                    CoreMessageHandler(message:="Object entry must be bound to an existing object definition", arg1:=myself.Objectname, _
                                       subname:="AbstractEntryDefinition_OnCreating", objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
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
                    CoreMessageHandler(message:="Object entry must be bound to an existing object definition", arg1:=e.Record.GetValue(ConstFNObjectName), _
                                       subname:="AbstractEntryDefinition_OnCreating", objectname:=Me.ObjectID, messagetype:=otCoreMessageType.InternalError)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _lookupProperties = aLookupList ' assign
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="AbstractEntryDefinition_OnEntryChanged")
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
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
                            Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
                        End Try
                    Next
                    _lookupProperties = aLookupList ' assign

                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="AbstractEntryDefinition_OnInfused", exception:=ex)
            End Try

        End Sub
        ''' <summary>
        ''' returns a Dictionary of Entryname - list of objectnames
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetEntryReference(Optional domainid As String = "") As Dictionary(Of String, List(Of String))

            Dim aStore As iormDataStore = CurrentDBDriver.GetTableStore(ConstTableID)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = ConstFNEntryName & "," & ConstFNObjectName & "," & ConstFNDomainID
                    aCommand.Where = "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))

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
                Call CoreMessageHandler(exception:=ex, subname:="AbstractEntryDefinition.GetEntryReference")
                Return aDictionary
            End Try

        End Function
        ''' <summary>
        ''' returns a Dictionary of Alias - list of objectentrynames
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAliasReference(Optional domainid As String = "") As Dictionary(Of String, List(Of String))

            Dim aStore As iormDataStore = CurrentDBDriver.GetTableStore(ConstTableID)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = ConstFNalias & "," & ConstFNEntryName & "," & ConstFNObjectName & "," & ConstFNDomainID
                    aCommand.Where = ConstFNalias & " <> '' AND " & ConstFNalias & " IS NOT NULL AND " & ConstFNalias & " <> '" & ConstDelimiter & ConstDelimiter & "' AND "
                    aCommand.Where &= "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))

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
                Call CoreMessageHandler(exception:=ex, subname:="AbstractEntryDefinition.GetAliasReference")
                Return aDictionary
            End Try

        End Function
        ''' <summary>
        ''' returns a Dictionary of XID - (  ObjectEntryName in canonical form) Tuples
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetXIDReference(Optional domainid As String = "") As Dictionary(Of String, List(Of String))

            Dim aStore As iormDataStore = CurrentDBDriver.GetTableStore(ConstTableID)
            Dim aDictionary As New Dictionary(Of String, List(Of String))
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="GetXIDReference", addMe:=True, addAllFields:=False)

                '** prepare the command if necessary
                If Not aCommand.Prepared Then
                    aCommand.select = ConstFNxid & "," & ConstFNObjectName & "," & ConstFNEntryName & "," & ConstFNDomainID
                    aCommand.Where = ConstFNxid & " <> '' AND " & ConstFNxid & " IS NOT NULL AND "
                    aCommand.Where &= "([" & ConstFNDomainID & "] = @domain OR [" & ConstFNDomainID & "] = @globaldomain)"
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@domain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(id:="@globaldomain", ColumnName:=ConstFNDomainID, tablename:=ConstTableID))

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
                Call CoreMessageHandler(exception:=ex, subname:="AbstractEntryDefinition.GetXIDReference")
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
        Implements iormPersistable
        Implements iormInfusable


        '*** CONST Schema
        Public Const ConstObjectID = "ObjectCompoundEntry"

        '** Field and tabele are comming from the Abstract Class

        '** extend the Table with additional fields
        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=100, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED100", title:="Compound Table", Description:="name of the compound table")> _
        Public Const ConstFNFinalObjectID As String = "ctblname"

        <ormObjectEntry(Datatype:=otDataType.List, isnullable:=True, posordinal:=101, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED101", title:="Compound Relation", Description:="relation path to the compound object")> _
        Public Const ConstFNCompoundRelation As String = "crelation"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=50, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=102, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED102", title:="compound id field", Description:="name of the compound id field")> Public Const ConstFNCompoundIDEntryname As String = "cidfield"

        <ormObjectEntry(Datatype:=otDataType.Text, size:=255, properties:={ObjectEntryProperty.Keyword}, isnullable:=True, posordinal:=103, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        XID:="OED103", title:="compound value field", Description:="name of the compound value field")> Public Const ConstFNCompoundValueEntryName As String = "cvalfield"


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
        <ormEntryMapping(EntryName:=ConstFNFinalObjectID)> Private _cFinalObjectID As String
        <ormEntryMapping(EntryName:=ConstFNCompoundRelation)> Private _cRelation As String()
        <ormEntryMapping(EntryName:=ConstFNCompoundIDEntryname)> Private _cIDEntryname As String
        <ormEntryMapping(EntryName:=ConstFNCompoundValueEntryName)> Private _cValueEntryName As String
        <ormEntryMapping(EntryName:=ConstFNCompoundGetter)> Private _CompoundGetterMethodName As String
        <ormEntryMapping(EntryName:=ConstFNCompoundSetter)> Private _CompoundSetterMethodName As String
        <ormEntryMapping(EntryName:=ConstFNCompoundValidator)> Private _CompoundValidatorMethodName As String

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
            Call MyBase.New()
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
                SetValue(ConstFNCompoundGetter, Value)
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
                CoreMessageHandler(message:="ObjectCompoundEntry cannot be a primary key", subname:="ObjectCompoundEntry.PrimaryKeyOrdinal", messagetype:=otCoreMessageType.InternalWarning)

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
                CoreMessageHandler(message:="compound cannot be sparefield", subname:="ObjectCompoundEntry.IsSpareField", entryname:=Me.Entryname, objectname:=Me.Objectname)
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
                                                  Optional ByVal domainID As String = "", _
                                                  Optional runtimeOnly As Boolean = False) As ObjectCompoundEntry
            Return Retrieve(Of ObjectCompoundEntry)(pkArray:={objectname.ToUpper, entryname.ToUpper}, domainID:=domainID, runtimeOnly:=runtimeOnly)
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
                                            Optional ByVal domainID As String = "", _
                                            Optional ByVal runtimeOnly As Boolean = False, _
                                            Optional ByVal checkunique As Boolean = True) As ObjectCompoundEntry
            '** create with record to fill other values
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim arecord As New ormRecord
            With arecord
                .SetValue(ConstFNObjectName, objectname.ToUpper)
                .SetValue(ConstFNEntryName, entryname.ToUpper)
                .SetValue(ConstFNDomainID, domainID)
                .SetValue(ConstFNType, otObjectEntryType.Compound)
            End With

            ' create
            Return ormDataObject.CreateDataObject(Of ObjectCompoundEntry)(record:=arecord, domainID:=domainID, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function
    End Class
    ''' <summary>
    ''' class for Column ObjectEntry (data slots) - it mostly references to the ColumnDefinition object to keep the definition of the table columns unique
    ''' </summary>
    ''' <remarks></remarks>
    'explicit since we are not running through inherited classes
    <ormObject(id:=ObjectColumnEntry.ConstObjectID, modulename:=ConstModuleRepository, _
                AddDeletefieldBehavior:=True, AddDomainBehavior:=True, _
                Description:="Object Entry Definition as Column Entry (of a Table)", _
                usecache:=True, isbootstrap:=True, Version:=1)> _
    Public Class ObjectColumnEntry
        Inherits AbstractEntryDefinition
        Implements iormPersistable, iormInfusable, iormObjectEntry


        '*** CONST Schema
        Public Shadows Const ConstObjectID = "ObjectColumnEntry"

        '*** Columns
        <ormObjectEntry(referenceobjectentry:=TableDefinition.ConstObjectID & "." & TableDefinition.ConstFNTablename, posordinal:=90, isnullable:=True, _
                         properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        Description:="corresponding table name of the column ")> Public Const ConstFNTableName As String = TableDefinition.ConstFNTablename

        <ormObjectEntry(referenceobjectentry:=ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNColumnname, posordinal:=91, isnullable:=True, _
                        properties:={ObjectEntryProperty.Keyword}, validationPropertyStrings:={ObjectValidationProperty.NotEmpty}, _
                        Description:="corresponding column name of the object entry")> Public Const ConstFNColumnname As String = ColumnDefinition.ConstFNColumnname

        <ormObjectEntry(Datatype:=otDataType.Bool, defaultvalue:=False, dbdefaultvalue:="0", posordinal:=92, _
                       title:="SpareFieldTag", Description:="set if the entry is a spare entry")> Public Const ConstFNSpareFieldTag As String = "SpareFieldTag"

        ' foreign key doesnot work for some reasons - sqlserver doesnot allow
        '
        '<ormSchemaForeignKey(useforeignkey:=otForeignKeyImplementation.NativeDatabase, _
        'entrynames:={ConstFNTableName, ConstFNColumnname}, _
        'foreignkeyreferences:={ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNTableName, _
        'ColumnDefinition.ConstObjectID & "." & ColumnDefinition.ConstFNColumnname})> Public Const constFKColumns = "FKColumns"

        ''' <summary>
        ''' relation to the columndefinition - will be always created on create
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelationAttribute(linkobject:=GetType(ColumnDefinition), toPrimarykeys:={ConstFNTableName, ConstFNColumnname}, createObjectIfnotRetrieved:=True, _
            cascadeonCreate:=True, cascadeOnUpdate:=False)> Public Const constRColumnDefinition = "COLUMN"
        '** the real thing
        <ormEntryMapping(relationName:=constRColumnDefinition, InfuseMode:=otInfuseMode.OnCreate Or otInfuseMode.OnInject Or otInfuseMode.OnDefault)> _
        Private _columndefinition As ColumnDefinition

        ' fields
        <ormEntryMapping(EntryName:=ConstFNTableName)> Private _tablename As String = ""
        <ormEntryMapping(EntryName:=ConstFNColumnname)> Private _columnname As String = ""
        <ormEntryMapping(EntryName:=ConstFNSpareFieldTag)> Private _SpareFieldTag As Boolean = False



        ' further internals

        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        'Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements iormObjectEntry.PropertyChanged


        ''' <summary>
        ''' constructor 
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New()
            _typeid = otObjectEntryType.Column
            AddHandler ormDataObject.OnCreateDefaultValuesNeeded, AddressOf OnDefaultValuesNeeded
            AddHandler ormDataObject.OnFeeding, AddressOf OnFeeding
            AddHandler ormDataObject.OnValidating, AddressOf OnValidating
            AddHandler ormDataObject.OnInitializing, AddressOf OnInitialize
        End Sub

#Region "Properties"

        ''' <summary>
        ''' sets or gets the column name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Columnname() As String
            Get
                Columnname = _columnname
            End Get
            Set(value As String)
                SetValue(ConstFNColumnname, value)
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the table name of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TableName() As String
            Get
                TableName = _tablename
            End Get
            Set(value As String)
                SetValue(ConstFNTableName, value)
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
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.Description") Then
                    Return
                Else
                    If _columndefinition.Description Is Nothing OrElse Not _columndefinition.Description.Equals(value) Then
                        _columndefinition.Description = value
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.DBDefaultValue") Then
                    Return _columndefinition.DefaultValue
                Else : Return Nothing
                End If
            End Get
            Set(value As Object)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.DBDefaultValue") Then
                    Return
                End If
                If _columndefinition.DefaultValue Is Nothing OrElse Not _columndefinition.DefaultValue.Equals(value) Then
                    _columndefinition.DefaultValue = value
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.Datatype") Then
                    Return _columndefinition.Datatype
                Else : Return 0
                End If
            End Get
            Set(avalue As otDataType)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.Datatype") Then
                    Return
                End If
                _columndefinition.Datatype = avalue
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.PrimaryKeyOrdinal") Then
                    Return _columndefinition.PrimaryKeyOrdinal
                Else : Return 0
                End If
            End Get
            Set(avalue As Long)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.PrimaryKeyOrdinal") Then
                    Return
                End If
                If _columndefinition.PrimaryKeyOrdinal <> avalue Then
                    _columndefinition.PrimaryKeyOrdinal = avalue
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.Indexname") Then
                    Return _columndefinition.Indexname
                Else : Return ""
                End If
            End Get
            Set(value As String)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.Indexname") Then
                    Return
                End If
                If _columndefinition.Indexname.ToUpper <> value.ToUpper Then
                    _columndefinition.Indexname = value
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.IsPrimaryKey") Then
                    Return _columndefinition.IsPrimaryKey
                Else : Return False
                End If
            End Get
            Set(value As Boolean)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.IsPrimaryKey") Then
                    Return
                End If
                If _columndefinition.IsPrimaryKey <> value Then
                    _columndefinition.IsPrimaryKey = value
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
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.Size") Then
                    Return _columndefinition.Size
                Else : Return 0
                End If
            End Get
            Set(value As Long?)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.Size") Then
                    Return
                End If
                _columndefinition.Size = value
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
        Public Property ColumnOrdinal() As UShort
            Get
                If _columndefinition IsNot Nothing AndAlso _columndefinition.IsAlive(subname:="ObjectColumnEntry.Position") Then
                    Return _columndefinition.Position
                Else : Return 0
                End If
            End Get
            Set(value As UShort)
                If _columndefinition Is Nothing OrElse Not _columndefinition.IsAlive(subname:="ObjectColumnEntry.Position") Then
                    Return
                End If
                If _columndefinition.Position <> value Then
                    _columndefinition.Position = value
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
        Public ReadOnly Property ColumnDefinition As ColumnDefinition
            Get
                Return _columndefinition
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
            Dim anObject = TryCast(e.DataObject, ObjectColumnEntry)
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
            Dim anObject = TryCast(e.DataObject, ObjectColumnEntry)
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
            If _columndefinition Is Nothing Then _columndefinition = New ColumnDefinition
        End Sub


        ''' <summary>
        ''' set the properties of a Column Entry by a ormObjectEntryAttribute
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function AbstractEntryDefinition_SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean
            If Not IsAlive(subname:="SetByAttribute") Then Return False


            With attribute
                Me.Typeid = otObjectEntryType.Column
                '** Slot Entry Properties
                MyBase.AbstractEntryDefinition_SetByAttribute(attribute)

                If .HasValueTableName Then Me.TableName = .Tablename
                If .HasValueColumnName Then Me.Columnname = .ColumnName

                '* column attributes
                If .HasValueIsNullable Then Me.IsNullable = .IsNullable
                If .HasValueIsNullable Then Me.ColumnDefinition.IsNullable = .IsNullable ' should be the same in the beginning

                If .hasValuePosOrdinal Then Me.ColumnOrdinal = .Posordinal ' should be the position from a table definition not an object definition
                If .HasValuePrimaryKeyOrdinal Then Me.PrimaryKeyOrdinal = .PrimaryKeyOrdinal

                If .HasValueSize Then Me.Size = .Size
                If .HasValueDBDefaultValue Then Me.DBDefaultValue = .DBDefaultValue
                If .HasValueSpareFieldTag Then Me.IsSpareField = .SpareFieldTag
                If .HasValueDataType Then Me.Datatype = .DataType

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
        Public Overloads Shared Function Retrieve(ByVal objectname As String, entryname As String, Optional ByVal domainID As String = "", Optional runtimeOnly As Boolean = False) As ObjectColumnEntry
            Return Retrieve(Of ObjectColumnEntry)(pkArray:={objectname.ToUpper, entryname.ToUpper}, domainID:=domainID, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' Event Handler relation loaded
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnRelationLoaded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnRelationLoad
            Dim aColumnEntry = TryCast(e.DataObject, ObjectColumnEntry)
            '** add the new columndefinition element in the table definition
            If aColumnEntry IsNot Nothing AndAlso e.Infusemode = otInfuseMode.OnCreate Then
                '** set up the connection to the tabledefinition
                Dim aTableDefinition As TableDefinition = TableDefinition.Retrieve(tablename:=aColumnEntry.TableName, runtimeOnly:=e.DataObject.RunTimeOnly)
                If aTableDefinition IsNot Nothing AndAlso Not aTableDefinition.HasEntry(entryname:=aColumnEntry.Columnname) Then
                    aTableDefinition.AddColumn(aColumnEntry.ColumnDefinition)
                ElseIf aTableDefinition Is Nothing Then
                    CoreMessageHandler(message:="TableDefinition could not be retrieved", messagetype:=otCoreMessageType.InternalError, tablename:=_tablename, _
                                       objectname:=Me.ObjectID, subname:="ObjectColumnEntry.OnRelationloaded")
                End If

            End If

            ''' register for changed of the column definition
            ''' 
            If aColumnEntry IsNot Nothing AndAlso e.RelationIDs.Contains(constRColumnDefinition.ToUpper) Then
                If _columndefinition IsNot Nothing Then AddHandler _columndefinition.PropertyChanged, AddressOf ColumnDefinition_PropertyChanged
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
                                            Optional ByVal domainID As String = "", _
                                            Optional ByVal runtimeOnly As Boolean = False, _
                                            Optional ByVal checkunique As Boolean = True) As ObjectColumnEntry
            '** create with record to fill other values
            Dim arecord As New ormRecord
            With arecord
                .SetValue(ConstFNObjectName, objectname.ToUpper)
                .SetValue(ConstFNEntryName, entryname.ToUpper)
                .SetValue(ConstFNTableName, tablename.ToUpper)
                .SetValue(ConstFNColumnname, columnname.ToUpper)
                .SetValue(ConstFNDomainID, domainID)
                .SetValue(ConstFNType, otObjectEntryType.Column)
                If ordinal.HasValue Then .SetValue(ConstFNordinal, ordinal)

            End With

            ' create
            Return ormDataObject.CreateDataObject(Of ObjectColumnEntry)(record:=arecord, domainID:=domainID, checkUnique:=checkunique, runtimeOnly:=runtimeOnly)
        End Function


        ''' <summary>
        ''' handler for columndefinition property changed event raises the iormObjectEntry event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ColumnDefinition_PropertyChanged(sender As Object, e As PropertyChangedEventArgs)
            If e.PropertyName = ColumnDefinition.ConstFNPrimaryKeyOrdinal Then
                ' cascade it
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(e.PropertyName))
            ElseIf e.PropertyName = ColumnDefinition.ConstFNPrimaryKey Then
                ' cascade it
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(e.PropertyName))
            End If
        End Sub
    End Class


End Namespace