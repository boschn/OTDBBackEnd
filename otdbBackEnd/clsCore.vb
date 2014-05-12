Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
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

Namespace OnTrack

    ''' <summary>
    ''' Session Class holds all the Session based Data for On Track Database
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Session

        Private _SessionID As String

        '******  PARAMETERS
        Private _DependencySynchroMinOverlap As Integer  '= 7
        Private _DefaultWorkspace As String    '= ""
        Private _DefaultCalendarName As String    '= ""
        Private _TodayLatency As Integer
        Private _DefaultScheduleTypeID As String = ""
        Private _DefaultDeliverableTypeID As String = ""

        '*** SESSION
        Private _OTDBUser As Commons.User
        Private _Username As String = ""
        Private _errorLog As SessionMessageLog
        Private _logagent As SessionAgent
        Private _UseConfigSetName As String = ""
        Private _CurrentDomainID As String = ConstGlobalDomain
        Private _loadDomainReqeusted As Boolean = False
        Private _CurrentWorkspaceID As String = ""

        ' initialized Flag
        Private _IsInitialized As Boolean = False
        Private _IsStartupRunning As Boolean = False
        Private _IsRunning As Boolean = False
        Private _IsBootstrappingInstallRequested As Boolean = False ' BootstrappingInstall ?
        Private _IsInstallationRunning As Boolean = False ' actual Installallation running ?

        ' the environments
        Private WithEvents _primaryDBDriver As iormDatabaseDriver
        Private WithEvents _primaryConnection As iormConnection
        Private WithEvents _configurations As ComplexPropertyStore

        Private _CurrentDomain As Domain
        Private _UILogin As UI.CoreLoginForm
        Private _AccessLevel As otAccessRight    ' access

        Private _DomainObjectsDir As New Dictionary(Of String, ObjectRepository)
        Private _ObjectPermissionCache As New Dictionary(Of String, Boolean)

        Private _ObjectCache As ormObjectCacheManager

        'shadow Reference for Events
        ' our Events
        Public Event OnDomainChanging As EventHandler(Of SessionEventArgs)
        Public Event OnDomainChanged As EventHandler(Of SessionEventArgs)
        Public Event OnWorkspaceChanging As EventHandler(Of SessionEventArgs)
        Public Event OnWorkspaceChanged As EventHandler(Of SessionEventArgs)
        Public Event OnStarted As EventHandler(Of SessionEventArgs)
        Public Event OnEnding As EventHandler(Of SessionEventArgs)
        Public Event OnConfigSetChange As EventHandler(Of SessionEventArgs)
        Public Event ObjectDefinitionChanged As EventHandler(Of ObjectDefinition.EventArgs)
        Public Event StartOfBootStrapInstallation As EventHandler(Of SessionEventArgs)
        Public Event EndOfBootStrapInstallation As EventHandler(Of SessionEventArgs)

        '*** const
        Public Const ConstCPDependencySynchroMinOverlap = "DependencySynchroMinOverlap"
        Public Const ConstCPDefaultWorkspace = "DefaultWorkspace"
        Public Const ConstCPDefaultCalendarName = "DefaultCalendarName"
        Public Const ConstCPDefaultTodayLatency = "DefaultTodayLatency"
        Public Const ConstCDefaultScheduleTypeID = "DefaultScheduleTypeID"
        Public Const ConstCDefaultDeliverableTypeID = "DefaultDeliverableTypeID"
        'Public Const ConstCPDependencySynchroMinOverlap = "DependencySynchro_MinOverlap"
        'Public Const ConstCPDependencySynchroMinOverlap = "DependencySynchro_MinOverlap"

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="SessionID"> unqiue ID of the Session</param>
        ''' <remarks></remarks>
        Public Sub New(configurations As ComplexPropertyStore, Optional id As String = "")
            '* ID
            If id <> "" Then
                id = UCase(id)
            ElseIf ApplicationName <> "" Then
                id = ApplicationName
            Else
                id = My.Application.Info.Title & "." & My.Application.Info.AssemblyName & "." & My.Application.Info.Version.ToString
            End If
            '* session
            _SessionID = ConstDelimiter & Date.Now.ToString("s") & ConstDelimiter & My.Computer.Name & ConstDelimiter _
            & My.User.Name & ConstDelimiter & id & ConstDelimiter
            '* init
            _errorLog = New SessionMessageLog(_SessionID)
            _logagent = New SessionAgent(Me)

            '** register the configuration
            If configurations IsNot Nothing Then
                _UseConfigSetName = configurations.CurrentSet
                _configurations = configurations
            End If
        End Sub

        ''' <summary>
        ''' Finalize
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Finalize()
            _primaryDBDriver = Nothing
            _primaryConnection = Nothing
            _logagent = Nothing
            _UILogin = Nothing
            _DomainObjectsDir = Nothing
            _ObjectCache = Nothing
        End Sub

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the domain ID (if setting then the domains will be switched).
        ''' </summary>
        ''' <value>The domain.</value>
        Public Property CurrentDomainID() As String
            Get
                Return Me._CurrentDomainID
            End Get
            Set(value As String)
                Call SwitchToDomain(value)
            End Set
        End Property
        ''' <summary>
        ''' Gets  the domain.
        ''' </summary>
        ''' <value>The domain.</value>
        Public ReadOnly Property CurrentDomain() As Domain
            Get
                Return Me._CurrentDomain
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the default deliverable type ID.
        ''' </summary>
        ''' <value>The default deliverable type ID.</value>
        Public Property DefaultDeliverableTypeID() As String
            Get
                Return Me._DefaultDeliverableTypeID
            End Get
            Set(value As String)
                Me._DefaultDeliverableTypeID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the access level.
        ''' </summary>
        ''' <value>The access level.</value>
        Public Property AccessLevel() As otAccessRight
            Get
                Return Me._AccessLevel
            End Get
            Set(value As otAccessRight)
                Me._AccessLevel = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Objects.
        ''' </summary>
        ''' <value>The Objects.</value>
        Public ReadOnly Property Objects() As ObjectRepository
            Get

                If _DomainObjectsDir.ContainsKey(key:=_CurrentDomainID) Then
                    Return _DomainObjectsDir.Item(key:=_CurrentDomainID)
                Else
                    Return Nothing
                End If
            End Get

        End Property

        ''' <summary>
        ''' Gets the user name.
        ''' </summary>
        ''' <value>The user name.</value>
        Public ReadOnly Property Username() As String
            Get
                Return Me._Username
            End Get
        End Property
        ''' <summary>
        '''  returns if session is running
        ''' </summary>
        ''' <value>The is running.</value>
        Public Property IsRunning() As Boolean
            Get
                Return Me._IsRunning
            End Get
            Private Set(value As Boolean)
                _IsRunning = value
            End Set
        End Property

        ''' Gets the O TDB user.
        ''' </summary>
        ''' <value>The O TDB user.</value>
        Public ReadOnly Property OTdbUser() As User
            Get
                Return Me._OTDBUser
            End Get
        End Property
        ''' <summary>
        ''' Gets the configurations ComplexPropertyStore.
        ''' </summary>
        ''' <value>The configurations.</value>
        Public ReadOnly Property Configurations() As ComplexPropertyStore
            Get
                Return Me._configurations
            End Get
        End Property
        ''' <summary>
        ''' returns the setname to be used to connect to the databased
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ConfigSetname As String
            Get
                Return _configurations.CurrentSet
            End Get
            Set(value As String)
                If _UseConfigSetName <> value Then
                    If Not Me.IsRunning Then
                        _configurations.CurrentSet = value ' raises event
                    Else
                        CoreMessageHandler(message:="a running session can not be set to another config set name", arg1:=value, messagetype:=otCoreMessageType.ApplicationError, subname:="Sesion.setname")
                    End If
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the today latency.
        ''' </summary>
        ''' <value>The today latency.</value>
        Public Property TodayLatency() As Integer
            Get
                Return Me._TodayLatency
            End Get
            Set(value As Integer)
                Me._TodayLatency = value
            End Set
        End Property

        ''' <summary>
        ''' set or gets the DefaultScheduleTypeID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DefaultScheduleTypeID As String
            Get
                Return _DefaultScheduleTypeID
            End Get
            Set(ByVal value As String)
                _DefaultScheduleTypeID = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the default name of the calendar.
        ''' </summary>
        ''' <value>The default name of the calendar.</value>
        Public Property DefaultCalendarName() As String
            Get
                Return Me._DefaultCalendarName
            End Get
            Set(value As String)
                Me._DefaultCalendarName = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the default workspaceID.
        ''' </summary>
        ''' <value>The default workspaceID.</value>
        Public Property DefaultWorkspaceID() As String
            Get
                Return Me._DefaultWorkspace
            End Get
            Set(value As String)
                Me._DefaultWorkspace = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is bootstrapping mode.
        ''' </summary>
        ''' <value>The is bootstrapping installation.</value>
        Public Property IsBootstrappingInstallationRequested() As Boolean
            Get
                Return Me._IsBootstrappingInstallRequested
            End Get
            Private Set(value As Boolean)
                Me._IsBootstrappingInstallRequested = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is installation Mode
        ''' </summary>
        ''' <value>The is bootstrapping installation.</value>
        Public Property IsInstallationRunning() As Boolean
            Get
                Return Me._IsInstallationRunning
            End Get
            Private Set(value As Boolean)
                Me._IsInstallationRunning = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is statup Mode
        ''' </summary>
        ''' <value></value>
        Public Property IsStartingUp() As Boolean
            Get
                Return Me._IsStartupRunning
            End Get
            Private Set(value As Boolean)
                Me._IsStartupRunning = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the default workspaceID.
        ''' </summary>
        ''' <value>The default workspaceID.</value>
        Public Property CurrentWorkspaceID() As String
            Get
                Return Me._CurrentWorkspaceID
            End Get
            Set(value As String)
                If value <> _CurrentWorkspaceID Then
                    Dim e As SessionEventArgs = New SessionEventArgs(session:=Me, newWorkspaceid:=value)
                    RaiseEvent OnWorkspaceChanging(sender:=Me, e:=e)
                    If e.AbortOperation Then Return
                    Me._CurrentWorkspaceID = value
                    RaiseEvent OnWorkspaceChanging(sender:=Me, e:=e)
                End If
            End Set
        End Property
        ''' <summary>
        ''' the errorlog of the session
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Errorlog As SessionMessageLog
            Get
                Return _errorLog
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the dependency synchro min overlap.
        ''' </summary>
        ''' <value>The dependency synchro min overlap.</value>
        Public Property DependencySynchroMinOverlap() As Integer
            Get
                Return Me._DependencySynchroMinOverlap
            End Get
            Set(value As Integer)
                Me._DependencySynchroMinOverlap = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the UI login.
        ''' </summary>
        ''' <value>The UI login.</value>
        Public Property UILogin() As UI.CoreLoginForm
            Get
                If _UILogin Is Nothing Then
                    _UILogin = New UI.CoreLoginForm()
                End If
                Return Me._UILogin
            End Get
            Set(value As UI.CoreLoginForm)
                Me._UILogin = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is initialized.
        ''' </summary>
        ''' <value>The is initialized.</value>
        Public Property IsInitialized() As Boolean
            Get
                Return Me._IsInitialized
            End Get
            Private Set(value As Boolean)
                Me._IsInitialized = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the primary DB driver.
        ''' </summary>
        ''' <value>The primary DB driver.</value>
        Public Property CurrentDBDriver() As iormDatabaseDriver
            Get
                If Me.IsInitialized OrElse Me.Initialize Then
                    Return Me._primaryDBDriver
                Else
                    Return Nothing
                End If
            End Get
            Protected Set(value As iormDatabaseDriver)
                Me._primaryDBDriver = value
                Me._primaryConnection = value.CurrentConnection
                Me.IsInitialized = True
            End Set
        End Property
        ''' <summary>
        ''' Gets the session ID.
        ''' </summary>
        ''' <value>The session ID.</value>
        Public ReadOnly Property SessionID() As String
            Get
                Return Me._SessionID
            End Get
        End Property
#End Region


        ''' <summary>
        ''' Event Handler for the Current ConfigurationSet Changed Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnCurrentConfigSetChanged(sender As Object, e As ComplexPropertyStore.EventArgs) Handles _configurations.OnCurrentSetChanged
            '** do only something if we have run through
            If Me.IsRunning Then
                '** do nothing if we are running
                CoreMessageHandler(message:="current config set name was changed after session is running -ignored", subname:="OnCurrentConfigSetChanged", arg1:=e.Setname, messagetype:=otCoreMessageType.InternalError)
            Else
                ''' create or get the Database Driver
                _primaryDBDriver = CreateOrGetDatabaseDriver(session:=Me)
                If _primaryDBDriver IsNot Nothing Then
                    '** set the connection for events
                    _primaryConnection = _primaryDBDriver.CurrentConnection
                    If _primaryConnection Is Nothing Then
                        CoreMessageHandler(message:="The database connection could not be set - initialization of session aborted ", _
                                           noOtdbAvailable:=True, subname:="Session.OnCurrentConfigSetChange", _
                                           messagetype:=otCoreMessageType.InternalInfo)
                    End If
                End If

            End If

        End Sub
        ''' <summary>
        ''' Event Handler for the Configuration Property Changed Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnConfigPropertyChanged(sender As Object, e As ComplexPropertyStore.EventArgs) Handles _configurations.OnPropertyChanged
            '** do only something if we have run through
            If Me.IsRunning Then
                '** do nothing if we are running
                CoreMessageHandler(message:="current config set name was changed after session is running -ignored", subname:="OnCurrentConfigSetChanged", arg1:=e.Setname, messagetype:=otCoreMessageType.InternalError)
            Else
                If Me.IsInitialized Then
                    ''' propagate the change shoud be running automatically 
                End If
            End If
        End Sub

        ''' <summary>
        ''' returns the DBDriver Object for a session
        ''' </summary>
        ''' <param name="configsetname"></param>
        ''' <param name="session"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function CreateOrGetDatabaseDriver(Optional session As Session = Nothing) As iormDatabaseDriver
            Dim avalue As Object
            Dim aDBDriver As iormDatabaseDriver


            If session Is Nothing Then session = ot.CurrentSession

            '*** which Environment / Driver to use look into configurations config 
            avalue = _configurations.GetProperty(name:=ConstCPNDriverName, setname:=session.ConfigSetname)
            If avalue IsNot Nothing AndAlso DirectCast(avalue, otDbDriverType) = otDbDriverType.ADOClassic Then
                Call CoreMessageHandler(showmsgbox:=True, message:="Initialization of database driver failed. Type of Database Environment " & ConstCPNDriverName & " is outdated. Parameter DefaultDBEnvirormentName has unknown value", _
                                        noOtdbAvailable:=True, arg1:=avalue, subname:="Session.GetDatabaseDriver", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            ElseIf avalue IsNot Nothing AndAlso DirectCast(avalue, otDbDriverType) = otDbDriverType.ADONETOLEDB Then
                aDBDriver = New oleDBDriver(ID:=avalue, session:=session)
            ElseIf avalue IsNot Nothing AndAlso DirectCast(avalue, otDbDriverType) = otDbDriverType.ADONETSQL Then
                aDBDriver = New mssqlDBDriver(ID:=avalue, session:=session)
            Else
                Return Nothing
            End If

            Return aDBDriver
        End Function


        ''' <summary>
        ''' Initialize the Session 
        ''' </summary>
        ''' <param name="DBDriver">DBDriver to be provided</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Initialize(Optional useConfigsetName As String = "") As Boolean
            '
            Try

                '*** Retrieve Config Properties and set the Bag
                If Not ot.RetrieveConfigProperties() Then
                    Call CoreMessageHandler(showmsgbox:=True, message:="config properties couldnot be retrieved - Initialized failed. ", _
                                            noOtdbAvailable:=True, subname:="Session.Initialize", messagetype:=otCoreMessageType.InternalError)
                    Return False
                Else
                    Call CoreMessageHandler(showmsgbox:=False, message:="config properties could be retrieved", _
                                            noOtdbAvailable:=True, subname:="Session.Initialize", messagetype:=otCoreMessageType.InternalInfo)
                End If

                ' set the configuration set to be used
                If useConfigsetName = "" Then
                    '** get the default - trigger change event
                    If _configurations.CurrentSet IsNot Nothing Then
                        useConfigsetName = _configurations.CurrentSet
                    Else
                        useConfigsetName = _configurations.GetProperty(name:=ConstCPNUseConfigSetName, setname:=ConstGlobalConfigSetName)
                    End If

                ElseIf Not _configurations.HasSet(useConfigsetName) Then
                    Call CoreMessageHandler(message:="config properties set could not be retrieved from config set properties - Initialized failed. ", _
                                           noOtdbAvailable:=True, subname:="Session.Initialize", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                '** set a specific - trigger change event
                _configurations.CurrentSet = useConfigsetName
                '** here we should have a database driver and a connection by event handling
                '** and reading the properties if not something is wrong
                If _primaryDBDriver Is Nothing OrElse _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(showmsgbox:=True, message:="config properties are invalid - Session to Ontrack failed to initialize. ", _
                                           noOtdbAvailable:=True, subname:="Session.Initialize", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                '** create Object Cache
                If _ObjectCache Is Nothing Then _ObjectCache = New ormObjectCacheManager(Me)
                ot.ObjectClassRepository.RegisterCacheManager(_ObjectCache)
                _ObjectCache.Start()

                '** create ObjectStore
                Dim aStore As New ObjectRepository(Me)
                aStore.RegisterCache(_ObjectCache)

                _DomainObjectsDir.Clear()
                _DomainObjectsDir.Add(key:=ConstGlobalDomain, value:=aStore)
                _CurrentDomainID = ConstGlobalDomain
                _loadDomainReqeusted = True
                _CurrentDomain = Nothing

                '** fine 
                Call CoreMessageHandler(message:="The Session '" & Me.SessionID & "' is initialized ", _
                                        noOtdbAvailable:=True, subname:="Session.Initialize", _
                                        messagetype:=otCoreMessageType.InternalInfo)

                _IsInitialized = True
                Return Me.IsInitialized

            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, noOtdbAvailable:=True, subname:="Session.Initialize")
                Return False
            End Try



        End Function
        ''' <summary>
        ''' EventHandler for BootstrapInstall requested by primaryDBDriver
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub OnRequestBootstrapInstall(sender As Object, e As SessionBootstrapEventArgs) Handles _primaryDBDriver.RequestBootstrapInstall
            If Not _IsInitialized AndAlso Not Initialize() Then Return

            If Not _IsBootstrappingInstallRequested Then
                If _primaryDBDriver IsNot Nothing Then
                    _IsBootstrappingInstallRequested = True
                    RaiseEvent StartOfBootStrapInstallation(Me, New SessionEventArgs(Me))
                    Call CoreMessageHandler(subname:="Session.OnRequestBootstrapInstall", message:="bootstrapping mode started", _
                                               arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalInfo)
                End If
            End If

            If Not _IsInstallationRunning AndAlso e.Install Then
                Call CoreMessageHandler(subname:="Session.OnRequestBootstrapInstall", message:="bootstrapping installation started", _
                                                arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalInfo)
                '** issue an installation
                e.InstallationResult = _primaryDBDriver.InstallOnTrackDatabase(askBefore:=e.AskBefore, modules:=e.Modules)
            End If
        End Sub
        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnConnecting(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnConnection
            Me.StartUpSessionEnviorment(force:=True, domainID:=e.DomainID)
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnDisConnecting(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnDisconnection
            Me.ShutDownSessionEnviorment()
        End Sub
        ''' <summary>
        ''' Install the Ontrack database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InstallOnTrackDatabase(Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Boolean
            '** lazy initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                CoreMessageHandler(subname:="Session.InstallOnTrackDatabase", message:="failed to initialize session", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            '** install
            If sequence = sequence.Primary Then
                If _primaryDBDriver.InstallOnTrackDatabase(askBefore:=True, modules:={}) Then
                    Return True
                Else
                    CoreMessageHandler(subname:="Session.InstallOnTrackDatabase", message:="installation failed", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                End If
            Else
                CoreMessageHandler(subname:="Session.InstallOnTrackDatabase", message:="other sequences not implemented", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If


        End Function
        ''' <summary>
        ''' Abort the Starting up if possible
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RequestToAbortStartingUp() As Boolean
            _IsStartupRunning = False
            Return Not _IsStartupRunning
        End Function
        ''' <summary>
        ''' requests and checks if an end of bootstrap is possible 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RequestEndofBootstrap() As Boolean
            '** lazy initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                Call CoreMessageHandler(subname:="Session.RequestEndofBootstrap", message:="failed to initialize session", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            If Me.IsBootstrappingInstallationRequested Then
                '** check should not only be on existence also on the columns
                If Not CurrentDBDriver.VerifyOnTrackDatabase Then
                    '** raise event
                    RaiseEvent EndOfBootStrapInstallation(Me, New SessionEventArgs(Me, abortOperation:=True))
                    Call CoreMessageHandler(subname:="Session.RequestEndofBootstrap", message:="bootstrapping aborted - verify failed", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalInfo)
                    Me.IsBootstrappingInstallationRequested = False
                    Me.IsInstallationRunning = False
                    Return False ' return false to indicate that state is not ok
                Else
                    '** raise event
                    RaiseEvent EndOfBootStrapInstallation(Me, New SessionEventArgs(Me))
                    Call CoreMessageHandler(subname:="Session.RequestEndofBootstrap", message:="bootstrapping ended", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalInfo)
                    Me.IsBootstrappingInstallationRequested = False
                    Me.IsInstallationRunning = False
                    Return True
                End If
            Else
                Return True
            End If
        End Function
        ''' <summary>
        ''' requires from OTDB the Access Rights - starts a session if not running otherwise just validates
        ''' </summary>
        ''' <param name="AccessRequest">otAccessRight</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function RequireAccessRight(accessRequest As otAccessRight, _
                                            Optional domainID As String = "", _
                                            Optional reLogin As Boolean = True) As Boolean
            Dim anUsername As String
            '** lazy initialize
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                Call CoreMessageHandler(subname:="Session.RequireAccessRight", message:="failed to initialize session", _
                                        arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            '* take the OTDBDriver
            If _primaryDBDriver Is Nothing Then
                Me.CurrentDBDriver = OnTrack.ot.CurrentDBDriver
            End If

            '* how to check and wha to do

            If Me.IsRunning Then
                If domainID = "" Then domainID = Me.CurrentDomainID
                anUsername = Me.OTdbUser.Username
                Return Me.RequestUserAccess(accessRequest:=accessRequest, username:=anUsername, domainID:=domainID, loginOnFailed:=reLogin)
            Else
                If domainID = "" Then domainID = ConstGlobalDomain
                If Me.StartUp(AccessRequest:=accessRequest, domainID:=domainID) Then
                    Return Me.ValidateAccessRights(accessrequest:=accessRequest, domainid:=domainID)
                Else
                    CoreMessageHandler(message:="failed to startup a session", subname:="Session.RequireAccessRight", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
            End If

        End Function
        ''' <summary>
        ''' Raises the Event ObjectChagedDefinition
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub RaiseObjectChangedDefinitionEvent(sender As Object, e As ObjectDefinition.EventArgs)
            If _DomainObjectsDir.ContainsKey(key:=_CurrentDomainID) Then
                _DomainObjectsDir.Item(key:=_CurrentDomainID).OnObjectDefinitionChanged(sender, e)
            End If
        End Sub
        ''' <summary>
        ''' Raises the Event RaiseChangeConfigSet
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub RaiseChangeConfigSetEvent(setname As String)
            RaiseEvent OnConfigSetChange(Me, New SessionEventArgs(session:=Me, newConfigSetName:=setname))

        End Sub

        ''' <summary>
        ''' Validate the User against the Database with the accessRight
        ''' </summary>
        ''' <param name="username"></param>
        ''' <param name="password"></param>
        ''' <param name="accessRequest"></param>
        ''' <param name="domainID"></param>
        ''' <param name="databasedriver"></param>
        ''' <param name="uservalidation"></param>
        ''' <param name="messagetext"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ValidateUser(ByVal username As String, ByVal password As String, ByVal accessRequest As otAccessRight, ByVal domainID As String, _
          Optional databasedriver As iormDatabaseDriver = Nothing, Optional uservalidation As UserValidation = Nothing, Optional messagetext As String = "") As Boolean

            If databasedriver Is Nothing Then databasedriver = _primaryDBDriver
            If databasedriver Is Nothing Then
                CoreMessageHandler(message:="database driver is not available ", subname:="Session.ValidateUser", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            Return databasedriver.validateUser(username:=username, password:=password, accessRequest:=accessRequest)
        End Function

        ''' <summary>
        ''' Validate the Access Request against the current OnTrack DB Access Level of the user and the objects operations
        ''' (database driver and connection are checking just the access level)
        ''' </summary>
        ''' <param name="accessrequest"></param>
        ''' <param name="domain" >Domain to validate for</param>
        ''' <param name="Objects" >list of Obejectnames to validate in the domain</param>
        ''' <returns>eturns false if reverification of User is needed or true if currentAccessLevel includes this new request Level</returns>
        ''' <remarks></remarks>

        Public Function ValidateAccessRights(accessrequest As otAccessRight, _
                                                Optional domainid As String = "", _
                                                Optional ByRef objecttransactions As String() = Nothing) As Boolean
            Dim result As Boolean = False

            '** during startup we might not have a otdbuser
            If Me.IsStartingUp AndAlso (_OTDBUser Is Nothing OrElse Not _OTDBUser.IsAlive) Then
                Return True
            ElseIf _OTDBUser Is Nothing OrElse Not _OTDBUser.IsAlive Then
                CoreMessageHandler(message:="no otdb user is loaded into the session -failed to validate accessrights", messagetype:=otCoreMessageType.InternalError, _
                                                  subname:="Session.validateAccessRights")
                Return False
            End If

            '** check on the ontrackdatabase request
            result = AccessRightProperty.CoverRights(rights:=_AccessLevel, covers:=accessrequest)
            If Not result Then Return result

            'exit 
            If objecttransactions Is Nothing OrElse objecttransactions.Count = 0 OrElse Me.IsBootstrappingInstallationRequested Then Return result

            '** check all objecttransactions if level iss sufficent
            For Each opname In objecttransactions
                '** check cache
                If _ObjectPermissionCache.ContainsKey(opname.ToUpper) Then
                    result = result And True
                Else
                    Dim anObjectname As String
                    Dim anTransactionname As String
                    Converter.SplitFullName(opname, anObjectname, anTransactionname)
                    If anObjectname = "" Then
                        CoreMessageHandler(message:="ObjectID is missing in operation name", arg1:=opname, subname:="Session.validateOTDBAccessLevel", messagetype:=otCoreMessageType.InternalError)
                        result = result And False
                    ElseIf anTransactionname = "" Then
                        CoreMessageHandler(message:="Operation Name is missing in operation name", arg1:=opname, subname:="Session.validateOTDBAccessLevel", messagetype:=otCoreMessageType.InternalError)
                        result = result And False
                    Else
                        Dim aObjectDefinition = Me.Objects.GetObject(objectid:=anObjectname, runtimeOnly:=Me.IsBootstrappingInstallationRequested)
                        If aObjectDefinition Is Nothing And Not Me.IsBootstrappingInstallationRequested Then
                            CoreMessageHandler(message:="Object is missing in object repository", arg1:=opname, subname:="Session.validateOTDBAccessLevel", messagetype:=otCoreMessageType.InternalError)
                            result = result And False
                        Else
                            '** get the ObjectDefinition's effective permissions
                            result = result And aObjectDefinition.GetEffectivePermission(user:=_OTDBUser, domainid:=domainid, transactionname:=anTransactionname)
                            '** put it in cache
                            If _ObjectPermissionCache.ContainsKey(opname.ToUpper) Then
                                _ObjectPermissionCache.Remove(opname.ToUpper)
                            Else
                                _ObjectPermissionCache.Add(key:=opname.ToUpper, value:=result)
                            End If
                        End If

                    End If
                End If


            Next

            Return result
        End Function

        ''' <summary>
        ''' request the user access to OnTrack Database (running or not) - if necessary start a Login with Loginwindow. Check on user rights.
        ''' </summary>
        ''' <param name="accessRequest">needed User right</param>
        ''' <param name="username">default username to use</param>
        ''' <param name="password">default password to use</param>
        ''' <param name="forceLogin">force a Login window in any case</param>
        ''' <param name="loginOnDemand">do a Login window and reconnect if right is not necessary</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RequestUserAccess(accessRequest As otAccessRight, _
                                            Optional ByRef username As String = "", _
                                            Optional ByRef password As String = "", _
                                            Optional ByRef domainID As String = "", _
                                            Optional ByRef [objecttransactions] As String() = Nothing, _
                                            Optional loginOnDisConnected As Boolean = False, _
                                            Optional loginOnFailed As Boolean = False, _
                                            Optional messagetext As String = "") As Boolean

            Dim userValidation As UserValidation
            userValidation.ValidEntry = False


            '****
            '**** rights during bootstrapping
            '****


            If Me.IsBootstrappingInstallationRequested Then

                Return True
                '****
                '**** no connection -> login
                '****

            ElseIf Not Me.IsRunning Then

                If domainID = "" Then domainID = ConstGlobalDomain
                '*** OTDBUsername supplied

                If loginOnDisConnected And accessRequest <> ConstDefaultAccessRight Then
                    If Me.OTdbUser IsNot Nothing AndAlso Me.OTdbUser.IsAnonymous Then
                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = ""
                        Me.UILogin.Password = ""
                    End If
                    'LoginWindow
                    Me.UILogin.Configset = ot.CurrentConfigSetName
                    Me.UILogin.PossibleConfigSets = ot.ConfigSetNamesToSelect
                    Me.UILogin.EnableChangeConfigSet = True
                    If messagetext <> "" Then Me.UILogin.Messagetext = messagetext
                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    'Me.UILogin.Session = Me

                    Me.UILogin.Accessright = accessRequest
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = AccessRightProperty.GetHigherAccessRequests(accessrequest:=accessRequest)

                    Me.UILogin.Show()

                    If Not Me.UILogin.Ok Then
                        CoreMessageHandler(message:="login aborted by user", subname:="Session.verifyuserAccess", messagetype:=otCoreMessageType.ApplicationInfo)
                        Return False
                    Else
                        username = Me.UILogin.Username
                        password = Me.UILogin.Password
                        accessRequest = Me.UILogin.Accessright
                        '** change the currentConfigSet
                        If UILogin.Configset <> _UseConfigSetName Then
                            _UseConfigSetName = UILogin.Configset
                        End If
                        If Me.CurrentDomainID <> Me.UILogin.Domain Then
                            SwitchToDomain(Me.UILogin.Domain)
                        End If
                        '* validate
                        userValidation = _primaryDBDriver.GetUserValidation(username)
                    End If

                    ' just check the provided username
                ElseIf username <> "" Then
                    userValidation = _primaryDBDriver.GetUserValidation(username)
                    If userValidation.ValidEntry AndAlso password = "" Then
                        password = userValidation.Password
                    End If
                    '* no username but default accessrequest then look for the anonymous user
                ElseIf accessRequest = ConstDefaultAccessRight Then
                    userValidation = _primaryDBDriver.GetUserValidation(username:="", selectAnonymous:=True)
                    If userValidation.ValidEntry Then
                        username = userValidation.Username
                        password = userValidation.Password
                    End If
                End If

                ' if user is still nothing -> not verified
                If Not userValidation.ValidEntry Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                            message:=" Access to OnTrack Database is prohibited - User not found", _
                                            arg1:=userValidation.Username, noOtdbAvailable:=True, break:=False)

                    '*** reset
                    Call ShutDown()
                    Return False
                Else
                    '**** Check Password
                    '****
                    If _primaryDBDriver.validateUser(accessRequest:=accessRequest, username:=username, password:=password, domainid:=domainID) Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User verified successfully", _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                    Else
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User not verified successfully", _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)
                        Return False
                    End If

                End If

                '****
                '**** CONNECTION !
            Else
                '** stay in the current domain 
                If domainID = "" Then domainID = ot.CurrentSession.CurrentDomainID

                '** validate the current user with the request if it is failing then
                '** do check again
                If Me.ValidateAccessRights(accessrequest:=accessRequest, domainid:=domainID, objecttransactions:=[objecttransactions]) Then
                    Return True
                    '* change the current user if anonymous
                    '*
                ElseIf loginOnFailed And OTdbUser.IsAnonymous Then
                    '** check if new OTDBUsername is valid
                    'LoginWindow
                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    Me.UILogin.PossibleDomains = New List(Of String)
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = AccessRightProperty.GetHigherAccessRequests(accessRequest)
                    Me.UILogin.Configset = ot.CurrentConfigSetName
                    Me.UILogin.EnableChangeConfigSet = False
                    Me.UILogin.Accessright = accessRequest
                    If messagetext <> "" Then
                        Me.UILogin.Messagetext = messagetext
                    Else
                        Me.UILogin.Messagetext = "<html><strong>Welcome !</strong><br />Please change to a valid user and password for the needed access right.</html>"
                    End If
                    Me.UILogin.EnableUsername = True
                    Me.UILogin.Username = ""
                    Me.UILogin.Password = ""
                    'Me.UILogin.Session = Me

                    Me.UILogin.Show()

                    If Not Me.UILogin.Ok Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, _
                                                message:="login aborted by user - fall back to user " & username, _
                                                arg1:=username, messagetype:=otCoreMessageType.ApplicationInfo)
                        Return False
                    End If


                    username = UILogin.Username
                    password = UILogin.Password

                    userValidation = _primaryDBDriver.GetUserValidation(username)

                    '* check validation -> relogin on connected -> EventHandler ?!
                    '* or abortion of the login window
                    If _primaryDBDriver.validateUser(accessRequest:=accessRequest, username:=username, password:=password, domainid:=domainID) Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, _
                                                message:="User change verified successfully on domain '" & domainID & "'", _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        If Me.CurrentDomainID <> Me.UILogin.Domain Then
                            SwitchToDomain(Me.UILogin.Domain)
                        End If

                        '* set the new access level
                        _AccessLevel = accessRequest
                        Dim anOTDBUser As User = User.Retrieve(username:=username)
                        If anOTDBUser IsNot Nothing Then
                            _OTDBUser = anOTDBUser
                            Me.UserChangedEvent(_OTDBUser)
                        Else
                            CoreMessageHandler(message:="user definition cannot be loaded", messagetype:=otCoreMessageType.InternalError, _
                                               arg1:=username, noOtdbAvailable:=False, subname:="Session.verifyUserAccess")
                            username = _OTDBUser.Username
                            password = _OTDBUser.Password
                            Return False
                        End If

                    Else
                        '** fall back
                        username = _OTDBUser.Username
                        password = _OTDBUser.Password

                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)


                        Return False
                    End If


                    '* the current access level is not for this request
                    '*
                ElseIf loginOnFailed And Not Me.OTdbUser.IsAnonymous Then
                    '** check if new OTDBUsername is valid
                    'LoginWindow
                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    Me.UILogin.PossibleDomains = New List(Of String)
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = AccessRightProperty.GetHigherAccessRequests(accessRequest)
                    Me.UILogin.Configset = ot.CurrentConfigSetName
                    Me.UILogin.EnableChangeConfigSet = False
                    Me.UILogin.Accessright = accessRequest
                    If messagetext <> "" Then
                        Me.UILogin.Messagetext = messagetext
                    Else
                        Me.UILogin.Messagetext = "<html><strong>Attention !</strong><br />Please confirm by your password to obtain the access right.</html>"
                    End If
                    Me.UILogin.EnableUsername = False
                    Me.UILogin.Username = Me.OTdbUser.Username
                    Me.UILogin.Password = password
                    'Me.UILogin.Session = Me

                    Me.UILogin.Show()
                    If Not Me.UILogin.Ok Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, _
                                                message:="login aborted by user - fall back to user " & username, _
                                                arg1:=username, messagetype:=otCoreMessageType.ApplicationInfo)
                        Return False
                    End If
                    ' return input
                    username = UILogin.Username
                    password = UILogin.Password
                    If Me.CurrentDomainID <> Me.UILogin.Domain Then
                        SwitchToDomain(Me.UILogin.Domain)
                    End If
                    If Me.CurrentDomainID <> Me.UILogin.Domain Then
                        SwitchToDomain(Me.UILogin.Domain)
                    End If
                    userValidation = _primaryDBDriver.GetUserValidation(username)
                    '* check password
                    If _primaryDBDriver.validateUser(accessRequest:=accessRequest, username:=username, password:=password, domainid:=domainID) Then
                        '** not again
                        'Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User change verified successfully", _
                        '                        arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        '* set the new access level
                        _AccessLevel = accessRequest
                    Else
                        '** fallback
                        username = _OTDBUser.Username
                        password = _OTDBUser.Password
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                        Return False
                    End If

                    '*** just check the provided username
                ElseIf username <> "" And password <> "" Then
                    userValidation = _primaryDBDriver.GetUserValidation(username)
                End If
            End If

            '**** Check the UserValidation Rights

            '* exclude user
            If userValidation.HasNoRights Then
                Call CoreMessageHandler(showmsgbox:=True, _
                                        message:=" Access to OnTrack Database is prohibited - User has no rights", _
                                        break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                '*** shutdown 
                If Not Me.IsRunning Then
                    ShutDown()
                Else
                    ' Disconnect() -> Do not ! fall back to old user
                End If

                Return False
                '* check on the rights
            ElseIf Not userValidation.HasAlterSchemaRights And accessRequest = otAccessRight.AlterSchema Then
                Call CoreMessageHandler(showmsgbox:=True, _
                                        message:=" Access to OnTrack Database is prohibited - User has no alter schema rights", _
                                        break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                '*** shutdown 
                If Not Me.IsRunning Then
                    ShutDown()
                Else
                    ' Disconnect() -> Do not ! fall back to old user
                End If
                Return False
            ElseIf Not userValidation.HasUpdateRights And accessRequest = otAccessRight.ReadUpdateData Then
                Call CoreMessageHandler(showmsgbox:=True, _
                                        message:=" Access to OnTrack Database is prohibited - User has no update rights", _
                                        break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                '*** shutdown 
                If Not Me.IsRunning Then
                    ShutDown()
                Else
                    ' Disconnect() -> Do not ! fall back to old user
                End If
                Return False
            ElseIf Not userValidation.HasReadRights And accessRequest = otAccessRight.[ReadOnly] Then
                Call CoreMessageHandler(showmsgbox:=True, _
                                        message:=" Access to OnTrack Database is prohibited - User has no read rights", _
                                        break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                '*** shutdown 
                If Not Me.IsRunning Then
                    ShutDown()
                Else
                    ' Disconnect() -> Do not ! fall back to old user
                End If
                Return False
            End If
            '*** return true

            Return True

        End Function


        ''' <summary>
        ''' Initiate/Start a new Session or do nothing if a Session is already initiated
        ''' </summary>
        ''' <param name="OTDBUsername"></param>
        ''' <param name="OTDBPasswort"></param>
        ''' <param name="AccessRequest"></param>
        ''' <returns>True if successfull False else</returns>
        ''' <remarks></remarks>
        Public Function StartUp(AccessRequest As otAccessRight, Optional useconfigsetname As String = "", _
                            Optional domainID As String = "", _
                            Optional OTDBUsername As String = "", _
                            Optional OTDBPassword As String = "", _
                            Optional installIfNecessary As Boolean? = Nothing, _
                            Optional ByVal messagetext As String = "") As Boolean
            Dim aConfigsetname As String
            Dim aValue As Object
            Dim result As Boolean

            Try
                If Me.IsRunning Or Me.IsStartingUp Then
                    CoreMessageHandler(message:="Session is already running or starting up - further startups not possible", arg1:=Me.SessionID, subname:="Session.Startup", messagetype:=otCoreMessageType.InternalInfo)
                    Return False
                End If

                '** default is install on startup
                If Not installIfNecessary.HasValue Then installIfNecessary = True

                '** set statup
                Me.IsStartingUp = True

                If useconfigsetname <> "" AndAlso ot.HasConfigSetName(useconfigsetname, ComplexPropertyStore.Sequence.Primary) Then
                    _UseConfigSetName = useconfigsetname
                End If
                '** lazy initialize
                If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                    Call CoreMessageHandler(subname:="Session.Startup", message:="failed to initialize session", arg1:=Me.SessionID, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                '* take the OTDBDriver
                If _primaryDBDriver Is Nothing Then
                    CoreMessageHandler(message:="primary database driver is not set", messagetype:=otCoreMessageType.InternalError, _
                                       subname:="Session.Startup")
                    '** reset
                    If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                    Me.IsRunning = False
                    Me.IsStartingUp = False
                    Return False
                End If
                '** domain
                If domainID = "" Then Me.CurrentDomainID = ConstGlobalDomain ' set the current domain (_domainID)

                '*** get the Schema Version
                aValue = _primaryDBDriver.GetDBParameter(ConstPNBSchemaVersion, silent:=True)
                If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                    result = _primaryDBDriver.VerifyOnTrackDatabase(install:=installIfNecessary, modules:=ot.InstalledModules, verifySchema:=False)
                ElseIf ot.SchemaVersion < Convert.ToUInt64(aValue) Then
                    CoreMessageHandler(showmsgbox:=True, message:="Verifying the OnTrack Database failed. The Tooling schema version of # " & ot.SchemaVersion & _
                                       " is less than the database schema version of #" & aValue & " - Session could not start up", _
                                       messagetype:=otCoreMessageType.InternalError, subname:="Session.Startup")
                    '** reset
                    If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                    Me.IsRunning = False
                    Me.IsStartingUp = False
                    Return False
                ElseIf ot.SchemaVersion > Convert.ToUInt64(aValue) Then
                    result = _primaryDBDriver.VerifyOnTrackDatabase(install:=installIfNecessary, modules:=ot.InstalledModules, verifySchema:=False)
                Else
                    '** check also the bootstrap version
                    aValue = _primaryDBDriver.GetDBParameter(ConstPNBootStrapSchemaChecksum, silent:=True)
                    If aValue Is Nothing OrElse Not IsNumeric(aValue) OrElse ot.GetBootStrapSchemaChecksum <> Convert.ToUInt64(aValue) Then
                        result = _primaryDBDriver.VerifyOnTrackDatabase(install:=installIfNecessary, modules:=ot.InstalledModules, verifySchema:=False)
                    Else
                        result = True
                    End If
                End If
                '** the starting up aborted
                If Not Me.IsStartingUp Then
                    CoreMessageHandler(message:="Startup of Session was aborted", _
                                       messagetype:=otCoreMessageType.InternalInfo, subname:="Session.Startup")
                    '** reset
                    If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                    Me.IsRunning = False
                    Me.IsStartingUp = False
                    Return False
                End If

                '** the installation failed
                If Not result And installIfNecessary Then
                    CoreMessageHandler(showmsgbox:=True, message:="Verifying and Installing the OnTrack Database failed - Session could not start up", _
                                       messagetype:=otCoreMessageType.InternalError, subname:="Session.Startup")
                    '** reset
                    If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                    Me.IsRunning = False
                    Me.IsStartingUp = False
                    Return False
                ElseIf Not installIfNecessary And Not result Then
                    CoreMessageHandler(showmsgbox:=True, message:="Verifying  the OnTrack Database failed - Session will be started anyway on demand", _
                                                      messagetype:=otCoreMessageType.InternalWarning, subname:="Session.Startup")
                End If

                '** request access
                If RequestUserAccess(accessRequest:=AccessRequest, username:=OTDBUsername, _
                                    password:=OTDBPassword, domainID:=_CurrentDomainID, loginOnDisConnected:=True, loginOnFailed:=True, messagetext:=messagetext.Clone) Then
                    '** the starting up aborted
                    If Not Me.IsStartingUp Then
                        CoreMessageHandler(message:="Startup of Session was aborted", _
                                           messagetype:=otCoreMessageType.InternalInfo, subname:="Session.Startup")
                        '** reset
                        If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                        Me.IsRunning = False
                        Me.IsStartingUp = False
                        Return False
                    End If

                    ''' Connect - if we return we are not starting up again we have started
                    '''
                    If Not _primaryConnection.Connect(FORCE:=True, _
                                                      access:=AccessRequest, _
                                                      domainID:=domainID, _
                                                      OTDBUsername:=OTDBUsername, _
                                                      OTDBPassword:=OTDBPassword, _
                                                      doLogin:=True) Then

                        ''' start up message
                        CoreMessageHandler(message:="Could not connect to OnTrack Database though primary connection", arg1:=_primaryConnection.ID, _
                                                      messagetype:=otCoreMessageType.InternalError, subname:="Session.Startup")
                        '** reset
                        If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                        Me.IsRunning = False
                        Me.IsStartingUp = False
                        Return False
                    End If

                    '** Initialize through events
                Else
                    CoreMessageHandler(message:="user could not be verified - abort to start up a session", messagetype:=otCoreMessageType.InternalInfo, arg1:=OTDBUsername, _
                                       subname:="Session.Startup")
                    '** reset
                    If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                    Me.IsRunning = False
                    Me.IsStartingUp = False
                    Return False
                End If

                Return True

            Catch ex As ormNoConnectionException
                Return False
            Catch ex As ormException
                CoreMessageHandler(exception:=ex, subname:="Session.Startup")
                Return False
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="Session.Startup")
                Return False

            End Try

        End Function
        ''' <summary>
        ''' Initiate closeDown this Session and the Connection to OnTrack Database
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ShutDown(Optional force As Boolean = False) As Boolean

            '***
            Call CoreMessageHandler(showmsgbox:=False, message:="Session Shutdown", arg1:=_SessionID, _
                                    break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                    subname:="Session.ShutDown")

            '*** shut down the primary connection
            If Not _primaryConnection Is Nothing AndAlso _primaryConnection.IsConnected Then
                _primaryConnection.Disconnect()
                ' Call Me.ShutDownSessionEnviorment()  -> Event Driven
            Else
                Call Me.ShutDownSessionEnviorment()
            End If

            'reset
            _IsRunning = False
            _CurrentDomain = Nothing
            _CurrentDomainID = ""
            _CurrentWorkspaceID = ""
            _AccessLevel = 0
            _Username = ""
            _IsInitialized = False
            For Each anObjectstore In _DomainObjectsDir.Values
                'anObjectstore.reset()
            Next
            _DomainObjectsDir.Clear()
            _errorLog.Clear()
            Return True
        End Function

        ''' <summary>
        ''' sets the current Domain
        ''' </summary>
        ''' <param name="newDomainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SwitchToDomain(newDomainID As String) As Boolean
            Dim newDomain As Domain

            '* return if not running -> me.running might be false but connection is there since
            '* we are coming here during startup
            If _primaryDBDriver Is Nothing OrElse _primaryConnection Is Nothing _
            OrElse (_primaryConnection IsNot Nothing And Not _primaryConnection.IsConnected) Then
                _CurrentDomainID = newDomainID
                _loadDomainReqeusted = True
                Return True
            End If

            '* no change
            If (_CurrentDomainID <> "" And newDomainID = _CurrentDomainID) And Not _loadDomainReqeusted Then
                Return True
            End If

            '** if table exists -> no bootstrap

            newDomain = Domain.Retrieve(id:=newDomainID, dbdriver:=Me._primaryDBDriver, runtimeOnly:=Me.IsBootstrappingInstallationRequested)
            Dim saveDomain As Boolean = False

            '** check on bootstrapping 
            If newDomain Is Nothing And Not Me.IsBootstrappingInstallationRequested Then
                CoreMessageHandler(message:="domain does not exist", arg1:=newDomainID, subname:="Session.SetDomain", messagetype:=otCoreMessageType.ApplicationError)
                Return False
            ElseIf newDomain Is Nothing And Me.IsBootstrappingInstallationRequested Then
                '** bootstrapping database install
                newDomainID = ConstGlobalDomain
                'newDomain = New Domain()
                'newDomain.Create(domainID:=newDomainID)
                Me._CurrentDomain = Nothing
                Me._CurrentDomainID = newDomainID
                _loadDomainReqeusted = True
                RaiseEvent OnDomainChanging(Me, New SessionEventArgs(Me, Nothing))

                Return True
            Else

                '** we have a domain
                newDomain.RegisterSession(Me)
                If Not _DomainObjectsDir.ContainsKey(key:=newDomainID) Then
                    Dim aStore = New ObjectRepository(Me)
                    _DomainObjectsDir.Add(key:=newDomainID, value:=aStore)
                    aStore.RegisterCache(_ObjectCache)
                End If
                '* reset cache
                _ObjectPermissionCache.Clear()
                '** raise event
                RaiseEvent OnDomainChanging(Me, New SessionEventArgs(Me, newDomain))

                '*** read the Domain Settings
                '***

                If newDomain.HasSetting(id:=ConstCPDependencySynchroMinOverlap) Then
                    _DependencySynchroMinOverlap = newDomain.GetSetting(id:=ConstCPDependencySynchroMinOverlap).value
                Else
                    _DependencySynchroMinOverlap = 7
                End If

                If newDomain.HasSetting(id:=ConstCPDefaultWorkspace) Then
                    _DefaultWorkspace = newDomain.GetSetting(id:=ConstCPDefaultWorkspace).value
                    _CurrentWorkspaceID = _DefaultWorkspace
                Else
                    _DefaultWorkspace = ""
                End If

                If newDomain.HasSetting(id:=ConstCPDefaultCalendarName) Then
                    _DefaultCalendarName = newDomain.GetSetting(id:=ConstCPDefaultCalendarName).value
                Else
                    _DefaultCalendarName = "default"
                End If

                If newDomain.HasSetting(id:=ConstCPDefaultTodayLatency) Then
                    _TodayLatency = newDomain.GetSetting(id:=ConstCPDefaultTodayLatency).value
                Else
                    _TodayLatency = -14
                End If

                If newDomain.HasSetting(id:=ConstCDefaultScheduleTypeID) Then
                    _DefaultScheduleTypeID = newDomain.GetSetting(id:=ConstCDefaultScheduleTypeID).value
                Else
                    _DefaultScheduleTypeID = "none"

                End If

                If newDomain.HasSetting(id:=ConstCDefaultDeliverableTypeID) Then
                    _DefaultDeliverableTypeID = newDomain.GetSetting(id:=ConstCDefaultDeliverableTypeID).value
                Else
                    _DefaultDeliverableTypeID = ""
                End If

            End If


            Me._CurrentDomain = newDomain
            Me._CurrentDomainID = newDomainID
            _loadDomainReqeusted = False


            RaiseEvent OnDomainChanged(Me, New SessionEventArgs(Me))

            Return True
        End Function
        ''' <summary>
        ''' Initialize and set all Parameters
        ''' </summary>
        ''' <param name="FORCE"></param>
        ''' <returns>true if successful</returns>
        ''' <remarks></remarks>
        Private Function StartUpSessionEnviorment(Optional ByVal force As Boolean = False, Optional domainID As String = "") As Boolean
            Dim aValue As Object

            Try

                If Not IsRunning Or force Then


                    '** start the Agent
                    If Not _logagent Is Nothing Then
                        aValue = ot.GetConfigProperty(constCPNUseLogAgent)
                        If CBool(aValue) Then
                            _logagent.Start()
                            '***
                            Call CoreMessageHandler(showmsgbox:=False, message:=" LogAgent for Session started ", arg1:=_SessionID, _
                                                    break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                                    subname:="Session.startupSesssionEnviorment")
                        Else
                            '***
                            Call CoreMessageHandler(showmsgbox:=False, message:=" LogAgent for Session not used by configuration ", arg1:=_SessionID, _
                                                    break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                                    subname:="Session.startupSesssionEnviorment")
                        End If

                    End If
                    '** check driver
                    If _primaryDBDriver Is Nothing Or Not IsInitialized Then
                        '***
                        Call CoreMessageHandler(showmsgbox:=False, message:=" Session cannot initiated no DBDriver set ", _
                                                break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalError, _
                                                subname:="Session.startupSesssionEnviorment")
                        Me.IsStartingUp = False
                        IsRunning = False
                        Return False
                    End If

                    '*** Parameters
                    '***
                    _Username = _primaryDBDriver.CurrentConnection.Dbuser
                    _OTDBUser = User.Retrieve(username:=_primaryDBDriver.CurrentConnection.Dbuser)
                    If Not _OTDBUser Is Nothing AndAlso _OTDBUser.IsLoaded Then
                        _Username = _OTDBUser.Username
                        _AccessLevel = _OTDBUser.AccessRight
                    Else
                        Call CoreMessageHandler(showmsgbox:=True, message:=" Session could not initiate - user could not be retrieved from database", _
                                               break:=False, arg1:=_primaryDBDriver.CurrentConnection.Dbuser, noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalError, _
                                               subname:="Session.startupSesssionEnviorment")
                        IsRunning = False
                        Me.IsStartingUp = False
                        Return False
                    End If

                    '** load Domain
                    If domainID = "" Then domainID = Me.CurrentDomainID
                    '* set it here that we are really loading in SetDomain and not only 
                    '* assigning _DomainID (if no connection is available)
                    If SwitchToDomain(newDomainID:=domainID) Then
                        Call CoreMessageHandler(message:="Session Domain set to " & domainID, _
                                                messagetype:=otCoreMessageType.InternalInfo, _
                                                subname:="Session.startupSesssionEnviorment")
                    End If
                    '** the starting up aborted
                    If Not Me.IsStartingUp Then
                        CoreMessageHandler(message:="Startup of Session was aborted", _
                                           messagetype:=otCoreMessageType.InternalInfo, subname:="Session.StartupSessionEnviorment")
                        '** reset
                        If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                        Me.IsRunning = False
                        Me.IsStartingUp = False
                        Return False
                    End If

                    '*** get the Schema Version
                    aValue = _primaryDBDriver.GetDBParameter(ConstPNBootStrapSchemaChecksum, silent:=True)
                    If aValue Is Nothing OrElse Not IsNumeric(aValue) Then
                        _primaryDBDriver.VerifyOnTrackDatabase()
                    ElseIf ot.GetBootStrapSchemaChecksum <> Convert.ToUInt64(aValue) Then
                        _primaryDBDriver.VerifyOnTrackDatabase()
                    End If
                    '** the starting up aborted
                    If Not Me.IsStartingUp Then
                        CoreMessageHandler(message:="Startup of Session was aborted", _
                                           messagetype:=otCoreMessageType.InternalInfo, subname:="Session.StartupSessionEnviorment")
                        '** reset
                        If IsBootstrappingInstallationRequested Then Me.RequestEndofBootstrap()
                        Me.IsRunning = False
                        Me.IsStartingUp = False
                        Return False
                    End If
                    '*** set started
                    Me.IsStartingUp = False
                    IsRunning = True
                    '*** we are started
                    RaiseEvent OnStarted(Me, New SessionEventArgs(Me))

                End If
                Return IsRunning

            Catch ex As ormNoConnectionException
                Me.IsRunning = False
                Me.IsStartingUp = False
                Return False

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="Session.StartupSessionEnviorment")
                Me.IsRunning = False
                Me.IsStartingUp = False
                Return False
            End Try

        End Function

        ''' <summary>
        ''' reset the Session or close it down
        ''' </summary>
        ''' <param name="FORCE">true if to do it even not initialized</param>
        ''' <returns>True if successfully reseted</returns>
        ''' <remarks></remarks>
        Private Function ShutDownSessionEnviorment(Optional ByVal force As Boolean = False) As Boolean
            Dim aValue As Object

            If Not Me.IsInitialized OrElse Not Me.IsRunning Then
                Return False
            End If

            '*** we are ending
            RaiseEvent OnEnding(Me, New SessionEventArgs(Me))


            '** stop the Agent
            If Not _logagent Is Nothing Then
                _logagent.Stop()
                aValue = ot.GetConfigProperty(constCPNUseLogAgent)
                If CBool(aValue) Then
                    '***
                    Call CoreMessageHandler(showmsgbox:=False, message:="LogAgent for Session stopped ", arg1:=_SessionID, _
                                            break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                            subname:="Session.shutdownSessionEviorment")
                Else
                    '***
                    Call CoreMessageHandler(showmsgbox:=False, message:=" LogAgent for Session not used by configuration but stopped anyway ", arg1:=_SessionID, _
                                            break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                            subname:="Session.startupSesssionEnviorment")
                End If

            End If
            '*** Parameters
            '***
            _ObjectPermissionCache.Clear()
            _OTDBUser = Nothing
            IsRunning = False
            Call CoreMessageHandler(showmsgbox:=False, message:="Session ended ", arg1:=_SessionID, _
                                    break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                    subname:="Session.shutdownSessionEviorment")
            '** flush the log
            Me.CurrentDBDriver.PersistLog(Me.Errorlog)
            Return True

        End Function

        ''' <summary>
        ''' changes the session user to a new object
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub UserChangedEvent(newuser As User)
            _OTDBUser = newuser
            _Username = _OTDBUser.Username
            _ObjectPermissionCache.Clear()
        End Sub


    End Class

    ''' <summary>
    ''' Session Event Arguments
    ''' </summary>
    ''' <remarks></remarks>

    Public Class SessionEventArgs
        Inherits EventArgs

        Private _Session As Session
        Private _NewDomain As Domain
        Private _newConfigSetName As String
        Private _newWorkspaceID As String

        Private _Cancel As Boolean

        Public Sub New(Session As Session, Optional newDomain As Domain = Nothing, Optional abortOperation As Boolean? = Nothing, Optional newWorkspaceID As String = Nothing, Optional newConfigsetName As String = Nothing)
            _Session = Session
            _NewDomain = newDomain
            _newWorkspaceID = newWorkspaceID
            If abortOperation.HasValue Then _Cancel = abortOperation
            If newConfigsetName IsNot Nothing Then _newConfigSetName = newConfigsetName
        End Sub
        ''' <summary>
        ''' Gets the abort operation.
        ''' </summary>
        ''' <value>The abort operation.</value>
        Public ReadOnly Property AbortOperation() As Boolean
            Get
                Return Me._Cancel
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the new domain ID.
        ''' </summary>
        ''' <value>The new domain ID.</value>
        Public Property NewDomain() As Domain
            Get
                Return Me._NewDomain
            End Get
            Set(value As Domain)
                Me._NewDomain = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        ''' <value>The error.</value>
        Public ReadOnly Property [Session]() As Session
            Get
                Return _Session
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Domain Event Arguments
    ''' </summary>
    ''' <remarks></remarks>

    Public Class DomainEventArgs
        Inherits EventArgs

        Private _Session As Session
        Private _Domain As Domain

        Public Sub New(domain As Domain, Optional session As Session = Nothing)
            _Session = session
            _Domain = domain
        End Sub
        ''' <summary>
        ''' Gets or sets the new domain ID.
        ''' </summary>
        ''' <value>The new domain ID.</value>
        Public Property Domain() As Domain
            Get
                Return Me._Domain
            End Get
            Set(value As Domain)
                Me._Domain = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        ''' <value>The error.</value>
        Public ReadOnly Property [Session]() As Session
            Get
                Return _Session
            End Get
        End Property

    End Class

    ''' <summary>
    '''  Session Agent Class
    ''' </summary>
    ''' <remarks></remarks>

    Public Class SessionAgent
        Private _workerTimer As TimerCallback  'Workerthread
        Private _autoEvent As New AutoResetEvent(False)
        Private _threadTimer As System.Threading.Timer
        Private _session As Session
        Private _workinprogress As Boolean = False
        Private _stopped As Boolean = False

        Public Sub New(session As Session)
            _session = session
        End Sub
        ''' <summary>
        ''' Worker Sub 
        ''' </summary>
        ''' <param name="stateInfo"></param>
        ''' <remarks></remarks>
        Private Sub Worker(stateInfo As Object)
            If _session.IsRunning Then
                If Not _workinprogress AndAlso Not _stopped Then
                    _workinprogress = True
                    _session.CurrentDBDriver.PersistLog(_session.Errorlog)
                    _workinprogress = False
                End If
            End If
        End Sub
        ''' <summary>
        ''' Start the Agent
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Start()
            Initialize()
        End Sub
        ''' <summary>
        ''' Stop the the Agent
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub [Stop]()
            _stopped = True
            If Not _threadTimer Is Nothing Then
                ' When autoEvent signals, change the period to every  
                ' 1/2 second.
                _autoEvent.WaitOne(500, False)
                _threadTimer.Change(New TimeSpan(0), New TimeSpan(0, 0, 0, 250))

                ' When autoEvent signals the second time, dispose of  
                ' the timer.
                _autoEvent.WaitOne(500, False)
                _threadTimer.Dispose()
                Console.WriteLine(vbCrLf & "Destroying timer.")
                _threadTimer = Nothing
            End If
        End Sub
        Private Sub Initialize()
            If _threadTimer Is Nothing Then
                _workerTimer = AddressOf Me.Worker
                Dim delayTime As New TimeSpan(0, 0, 0, 50)
                Dim intervalTime As New TimeSpan(0, 0, 60)
                ' Create a timer that signals the delegate to invoke  
                ' CheckStatus after one second, and every 1/4 second  
                ' thereafter.
                Console.WriteLine("{0} Creating timer." & vbCrLf, _
                                  DateTime.Now.ToString("h:mm:ss.fff"))
                _threadTimer = New System.Threading.Timer(AddressOf Worker, _autoEvent, delayTime, intervalTime)

            End If

        End Sub

    End Class

    ''' <summary>
    ''' describes a persistable Session Log Message
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(id:=SessionMessage.ConstObjectID, description:="message generated during an OnTrack session", modulename:=ConstModuleCommons, Version:=1)> _
    Public Class SessionMessage
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable
        Implements iormCloneable
        Implements ICloneable

        '*** CONST Schema
        Public Const ConstObjectID = "SessionMessage"
        '** Table
        <ormSchemaTableAttribute(Version:=5)> Public Const ConstTableID = "tblSessionLogMessages"

        ''' <summary>
        ''' primary keys
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, _
                         title:="Session", Description:="sessiontag", primaryKeyordinal:=1)> Public Const ConstFNTag As String = "tag"

        <ormObjectEntry(typeid:=otDataType.Long, _
                         title:="no", Description:="number of entry", primaryKeyordinal:=2)> Public Const ConstFNno As String = "no"

        ''' <summary>
        ''' column definitions
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Message ID", Description:="id of the message")> Public Const ConstFNID As String = "id"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, _
                         title:="Message", Description:="message text")> Public Const ConstFNmessage As String = "message"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Routine", Description:="routine name")> Public Const ConstFNsubname As String = "subname"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
                         title:="Timestamp", Description:="timestamp of entry")> Public Const ConstFNtimestamp As String = "timestamp"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Object", Description:="object name")> Public Const ConstFNObjectname As String = "object"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="ObjectEntry", Description:="object entry")> Public Const ConstFNObjectentry As String = "objectentry"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Table", Description:="tablename")> Public Const ConstFNtablename As String = "table"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         title:="Column", Description:="columnname in the table")> Public Const ConstFNColumn As String = "column"

        <ormObjectEntry(typeid:=otDataType.Text, size:=255, isnullable:=True, _
                         title:="Argument", Description:="argument of the message")> Public Const ConstFNarg As String = "arg"

        <ormObjectEntry(typeid:=otDataType.Long, isnullable:=True, _
                         title:="message type id", Description:="id of the message type")> Public Const ConstFNtype As String = "typeid"

        <ormObjectEntry(typeid:=otDataType.Text, size:=50, isnullable:=True, title:="Username of the session", Description:="name of the user for this session")> _
        Public Const ConstFNUsername As String = "username"

        <ormObjectEntry(typeid:=otDataType.Memo, isnullable:=True, title:="stack trace", Description:="caller stack trace")> _
        Public Const ConstFNStack As String = "stack"

        <ormObjectEntry(referenceObjectEntry:=Domain.ConstObjectID & "." & Domain.ConstFNDomainID, _
         useforeignkey:=otForeignKeyImplementation.None, isnullable:=True)> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        ' fields
        <ormEntryMapping(EntryName:=ConstFNTag)> Private _tag As String = ""
        <ormEntryMapping(EntryName:=ConstFNID)> Private _id As String = ""
        <ormEntryMapping(EntryName:=ConstFNno)> Private _entryno As Long = 0
        <ormEntryMapping(EntryName:=ConstFNmessage)> Private _Message As String = ""
        <ormEntryMapping(EntryName:=ConstFNsubname)> Private _Subname As String = ""
        <ormEntryMapping(EntryName:=ConstFNtimestamp)> Private _Timestamp As Date = constNullDate
        <ormEntryMapping(EntryName:=ConstFNObjectname)> Private _Objectname As String = ""
        <ormEntryMapping(EntryName:=ConstFNObjectentry)> Private _Entryname As String = ""
        <ormEntryMapping(EntryName:=ConstFNtablename)> Private _Tablename As String = ""
        <ormEntryMapping(EntryName:=ConstFNColumn)> Private _Columnname As String = ""
        <ormEntryMapping(EntryName:=ConstFNtype)> Private _ErrorType As otCoreMessageType
        <ormEntryMapping(EntryName:=ConstFNUsername)> Private _Username As String = ""
        <ormEntryMapping(EntryName:=ConstFNStack)> Private _StackTrace As String = ""
        <ormEntryMapping(EntryName:=ConstFNarg)> Private _Arguments As String = ""

        '** dynamic
        Private _processed As Boolean = False
        Private _Exception As Exception

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New()
            _ErrorType = otCoreMessageType.ApplicationInfo
            _Timestamp = DateTime.Now()
        End Sub

#Region "Properties"


        ''' <summary>
        ''' Gets or sets the stack trace.
        ''' </summary>
        ''' <value>The stack trace.</value>
        Public Property StackTrace() As String
            Get
                Return Me._StackTrace
            End Get
            Set(value As String)
                Me._StackTrace = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the username.
        ''' </summary>
        ''' <value>The username.</value>
        Public Property ID() As String
            Get
                Return Me._id
            End Get
            Set(value As String)
                Me._id = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the username.
        ''' </summary>
        ''' <value>The username.</value>
        Public Property Username() As String
            Get
                Return Me._Username
            End Get
            Set(value As String)
                Me._Username = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tag.
        ''' </summary>
        ''' <value>The tag.</value>
        Public Property Tag() As String
            Get
                Return Me._tag
            End Get
            Set(value As String)
                _tag = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the processed.
        ''' </summary>
        ''' <value>The processed.</value>
        Public Property Processed() As Boolean
            Get
                Return Me._processed
            End Get
            Set(value As Boolean)
                Me._processed = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the entryno.
        ''' </summary>
        ''' <value>The entryno.</value>
        Public Property Entryno() As Long
            Get
                Return Me._entryno
            End Get
            Set(value As Long)
                Me._entryno = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the entry.
        ''' </summary>
        ''' <value>The name of the entry.</value>
        Public Property Columnname() As String
            Get
                Return Me._Columnname
            End Get
            Set(value As String)
                Me._Columnname = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the type of the error.
        ''' </summary>
        ''' <value>The type of the error.</value>
        Public Property messagetype() As otCoreMessageType
            Get
                Return Me._ErrorType
            End Get
            Set(value As otCoreMessageType)
                Me._ErrorType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tablename.
        ''' </summary>
        ''' <value>The tablename.</value>
        Public Property Tablename() As String
            Get
                Return Me._Tablename
            End Get
            Set(value As String)
                Me._Tablename = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the object.
        ''' </summary>
        ''' <value>The name of the entry.</value>
        Public Property Objectname() As String
            Get
                Return Me._Objectname
            End Get
            Set(value As String)
                Me._Objectname = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the object entry.
        ''' </summary>
        ''' <value>The name of the entry.</value>
        Public Property ObjectEntry() As String
            Get
                Return Me._Entryname
            End Get
            Set(value As String)
                Me._Entryname = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public Property Timestamp() As DateTime
            Get
                Return Me._Timestamp
            End Get
            Set(value As DateTime)
                Me._Timestamp = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the subname.
        ''' </summary>
        ''' <value>The subname.</value>
        Public Property Subname() As String
            Get
                Return Me._Subname
            End Get
            Set(value As String)
                Me._Subname = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the arguments.
        ''' </summary>
        ''' <value>The arguments.</value>
        Public Property Arguments() As String
            Get
                Return Me._Arguments
            End Get
            Set(value As String)
                Me._Arguments = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the exception.
        ''' </summary>
        ''' <value>The exception.</value>
        Public Property Exception() As Exception
            Get
                Return Me._Exception
            End Get
            Set(value As Exception)
                Me._Exception = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the message.
        ''' </summary>
        ''' <value>The message.</value>
        Public Property Message() As String
            Get
                Return Me._Message
            End Get
            Set(value As String)
                Me._Message = value
            End Set
        End Property
#End Region



        ''' <summary>
        ''' create a persistable Error
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateDataObject(ByVal sessiontag As String, ByVal entryno As Long) As SessionMessage
            Dim primarykey() As Object = {sessiontag, entryno}
            ' create
            Return ormDataObject.CreateDataObject(Of SessionMessage)(primarykey, checkUnique:=False, runtimeOnly:=True)
        End Function

        ''' <summary>
        ''' create an object after it was used
        ''' </summary>
        ''' <param name="sessiontag"></param>
        ''' <param name="entryno"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal sessiontag As String, ByVal entryno As Long) As Boolean
            Dim primarykey() As Object = {sessiontag, entryno}
            Return MyBase.Create(primarykey, checkUnique:=False, runtimeOnly:=True)
        End Function
        ''' <summary>
        ''' load and infuse the object by primary key
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal sessiontag As String, ByVal entryno As Long) As SessionMessage
            Dim primarykey() As Object = {sessiontag, entryno}
            Return ormDataObject.Retrieve(Of SessionMessage)(pkArray:=primarykey)
        End Function



        ''' <summary>
        ''' clone the error
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Clone() As Object Implements System.ICloneable.Clone
            Dim aClone As New SessionMessage
            With aClone
                If Me.Tag IsNot Nothing Then .Tag = Me.Tag.Clone
                If Me.ID IsNot Nothing Then .ID = Me.ID.Clone
                .Exception = Me.Exception
                If Me.Username IsNot Nothing Then .Username = Me.Username.Clone
                .Entryno = Me.Entryno
                If Me.Tablename IsNot Nothing Then .Tablename = Me.Tablename.Clone
                If Me.Columnname IsNot Nothing Then .Columnname = Me.Columnname.Clone
                If Me.Message IsNot Nothing Then .Message = Me.Message.Clone
                .messagetype = Me.messagetype
                .Timestamp = Me.Timestamp
                .StackTrace = Me.StackTrace
                If Me.Objectname IsNot Nothing Then .Objectname = Me.Objectname.Clone
                If Me.ObjectEntry IsNot Nothing Then .ObjectEntry = Me.ObjectEntry.Clone
            End With

            Return aClone
        End Function
    End Class

    ''' <summary>
    ''' Event Arguments for Request Bootstrapping Installation
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SessionBootstrapEventArgs
        Inherits EventArgs

        Private _install As Boolean = False
        Private _askbefore As Boolean = True
        Private _modules As String()
        Private _installationResult As Boolean = False

        Public Sub New(install As Boolean, modules As String(), Optional AskBefore As Boolean = True)
            _install = install
            _modules = modules
            _askbefore = AskBefore
        End Sub

        Public ReadOnly Property Install As Boolean
            Get
                Return _install
            End Get
        End Property
        Public ReadOnly Property AskBefore As Boolean
            Get
                Return _askbefore
            End Get
        End Property
        Public ReadOnly Property Modules As String()
            Get
                Return _modules
            End Get
        End Property
        Public Property InstallationResult As Boolean
            Get
                Return _installationResult
            End Get
            Set(value As Boolean)
                _installationResult = value
            End Set
        End Property
    End Class



    ''' <summary>
    ''' Describes an not persistable Log of Messages. Can be persisted by SessionLogMessages
    ''' </summary>
    ''' <remarks></remarks>

    Public Class SessionMessageLog
        Implements IEnumerable
        Implements ICloneable

        Public Event onErrorRaised As EventHandler(Of ormErrorEventArgs)
        Public Event onLogClear As EventHandler(Of ormErrorEventArgs)
        '*** log
        Private _log As New SortedList(Of Long, SessionMessage)
        Private _queue As New ConcurrentQueue(Of SessionMessage)
        Private _maxEntry As Long = 0
        Private _tag As String
        Private _lockObject As New Object ' lock object instead of me

        Public Sub New(tag As String)
            _tag = tag
        End Sub
        ''' <summary>
        ''' Gets the tag.
        ''' </summary>
        ''' <value>The tag.</value>
        Public ReadOnly Property Tag() As String
            Get
                Return Me._tag
            End Get
        End Property

        ''' <summary>
        ''' Returns an enumerator that iterates through a collection.
        ''' </summary>
        ''' <returns>
        ''' An <see cref="T:System.Collections.IEnumerator" /> object that can be
        ''' used to iterate through the collection.
        ''' </returns>
        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Dim anEnumerator As IEnumerator
            SyncLock _lockObject
                Dim aList As List(Of SessionMessage) = _log.Values.ToList
                anEnumerator = aList.GetEnumerator
            End SyncLock
            Return anEnumerator
        End Function

        Public Function Clone() As Object Implements System.ICloneable.Clone
            Dim m As New System.IO.MemoryStream()
            Dim f As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            f.Serialize(m, Me)
            m.Seek(0, IO.SeekOrigin.Begin)
            Return f.Deserialize(m)
        End Function
        ''' <summary>
        ''' Clears the error log from all messages
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Clear()
            RaiseEvent onLogClear(Me, New ormErrorEventArgs(Nothing))
            _log.Clear()
            '_queue = Nothing leave it for flush
            Return True
        End Function
        ''' <summary>
        ''' Persist the Messages
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.constNullDate) As Boolean
            '** we have a session
            If CurrentSession.IsRunning Then
                '*** only if the table is there
                If CurrentSession.CurrentDBDriver.GetTable(SessionMessage.ConstTableID) Is Nothing Then
                    Return False
                End If

                SyncLock _lockObject
                    For Each anError As SessionMessage In _log.Values
                        If Not anError.Processed And anError.IsAlive Then
                            anError.Persist()
                            anError.Processed = True ' do not again
                        End If
                    Next
                End SyncLock

            End If

            Return False
        End Function
        ''' <summary>
        ''' Add an otdb error object to the log
        ''' </summary>
        ''' <param name="otdberror"></param>
        ''' <remarks></remarks>
        Public Sub Enqueue(otdberror As SessionMessage)
            Dim aClone As SessionMessage = otdberror.Clone

            ' add
            SyncLock _lockObject

                If aClone.Timestamp = Nothing Then
                    aClone.Timestamp = DateTime.Now()
                End If

                aClone.Tag = Me.Tag
                aClone.Entryno = _maxEntry + 1

                If Not aClone.Exception Is Nothing And aClone.Message = "" Then
                    aClone.Message = aClone.Exception.Message
                End If

                _queue.Enqueue(aClone)
                _log.Add(key:=aClone.Entryno, value:=aClone)
                _maxEntry += 1

            End SyncLock

            RaiseEvent onErrorRaised(Me, New ormErrorEventArgs(aClone))
        End Sub
        ''' <summary>
        ''' returns the size of the log
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Size() As Long
            SyncLock _lockObject
                Return _log.Count
            End SyncLock
        End Function
        ''' <summary>
        ''' try to get the first Error from log
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PeekFirst() As SessionMessage
            Dim anError As SessionMessage
            SyncLock _lockObject
                If _queue.TryPeek(anError) Then
                    Return anError
                Else
                    Return Nothing
                End If
            End SyncLock
        End Function
        ''' <summary>
        ''' try to get the most recent error from log without removing
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function PeekLast() As SessionMessage
            Dim anError As SessionMessage
            SyncLock _lockObject
                If _queue.Count >= 1 Then
                    Return _queue.ToArray.Last
                Else
                    Return Nothing
                End If
            End SyncLock
        End Function
        ''' <summary>
        ''' remove and returns the first error in the error log 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Retain() As SessionMessage
            Dim anError As SessionMessage
            SyncLock _lockObject
                If _queue.TryDequeue([anError]) Then
                    Return anError
                Else
                    Return Nothing
                End If
            End SyncLock
        End Function

    End Class

    ''' <summary>
    ''' ObjectLog for Messages for Business Objects 
    ''' </summary>
    ''' <remarks>
    ''' 
    ''' The ObjectMessageLog is not an Data Object on its own. it is derived from the RelationCollection and
    ''' embedded as relation Member in a data object class
    ''' </remarks>
    Public Class ObjectMessageLog
        Inherits ormRelationCollection(Of ObjectMessage)
        Implements otLoggable

        ''' <summary>
        ''' Event Args
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EventArgs
            Inherits System.EventArgs

            Private _log As ObjectMessageLog
            Private _objectmessage As ObjectMessage

            Public Sub New(log As ObjectMessageLog, message As ObjectMessage)
                _log = Log
                _objectmessage = message
            End Sub

            ''' <summary>
            ''' Gets  the objectmessage log.
            ''' </summary>
            ''' <value>The objectmessage.</value>
            Public ReadOnly Property Log() As ObjectMessageLog
                Get
                    Return Me._log
                End Get
            End Property
            ''' <summary>
            ''' Gets  the objectmessage.
            ''' </summary>
            ''' <value>The objectmessage.</value>
            Public ReadOnly Property Message() As ObjectMessage
                Get
                    Return Me._objectmessage
                End Get
            End Property

        End Class
        ''' <summary>
        ''' Variables
        ''' </summary>
        ''' <remarks></remarks>
        Private _tag As String = ""

        'defaultvalues
        Private _ContextIdentifier As String
        Private _TupleIdentifier As String
        Private _EntitityIdentifier As String

        ''' <summary>
        ''' Events 
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event ObjectMessageAdded(sender As Object, e As ObjectMessageLog.EventArgs)

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="container"></param>
        ''' <remarks></remarks>

        Public Sub New(Optional container As ormDataObject = Nothing, _
                       Optional contextidenifier As String = Nothing, _
                       Optional tupleidentifier As String = Nothing, _
                       Optional entitityidentifier As String = Nothing)

            MyBase.New(container:=container, keyentrynames:={ObjectMessage.ConstFNNo})
            If container IsNot Nothing Then AddHandler container.OnInfused, AddressOf Me.ObjectMessageLog_OnInfused
            If contextidenifier IsNot Nothing Then _ContextIdentifier = contextidenifier
            If tupleidentifier IsNot Nothing Then _TupleIdentifier = tupleidentifier
            If entitityidentifier IsNot Nothing Then _EntitityIdentifier = entitityidentifier

        End Sub

#Region "Properties"

        ''' <summary>
        ''' gets the Tag of the Log
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Tag()
            Get
                Return _tag
            End Get
        End Property

        ''' <summary>
        ''' returns the greatest message no in the log
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MaxMessageNo()
            Get
                Return Me.Keys.Max(Function(x) x.Item(0))
            End Get
        End Property

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property ContextIdentifier As String Implements otLoggable.ContextIdentifier
            Get
                ContextIdentifier = _ContextIdentifier
            End Get
            Set(value As String)
                _ContextIdentifier = value
            End Set
        End Property

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property TupleIdentifier() As String Implements otLoggable.TupleIdentifier
            Get
                TupleIdentifier = _TupleIdentifier
            End Get
            Set(value As String)
                _TupleIdentifier = value
            End Set
        End Property

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property EntitityIdentifier() As String Implements otLoggable.EntitityIdentifier
            Get
                EntitityIdentifier = _EntitityIdentifier
            End Get
            Set(value As String)
                _EntitityIdentifier = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' event handler for tag
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectMessageLog_OnInfused(sender As Object, e As ormDataObjectEventArgs)
            _tag = TryCast(_container, ormDataObject).ObjectTag
        End Sub

        ''' <summary>
        ''' event handler for adding a message to the log to set the idno
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ObjectMessageLog_OnAdding(sender As Object, e As ormRelationCollection(Of ObjectMessage).EventArgs) Handles MyBase.OnAdding

        End Sub

        ''' <summary>
        ''' retrieves the log and loads all messages for the container object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Retrieve() As iormRelationalCollection(Of ObjectMessage)
            '''
            ''' check if the new Property value is different then old one
            ''' 
            '** build query
            Dim newCollection As ormRelationCollection(Of ObjectMessage) = New ormRelationCollection(Of ObjectMessage)(Nothing, keyentrynames:={ObjectMessage.ConstFNNo})
            Dim aTag = TryCast(_container, ormDataObject).ObjectTag
            Try
                Dim aStore As iormDataStore = _container.PrimaryTableStore
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="RetrieveObjectMessages", addAllFields:=True)
                If Not aCommand.Prepared Then
                    aCommand.Where = "[" & ObjectMessage.ConstFNTag & "] = @tag "
                    aCommand.Where &= " AND [" & ObjectMessage.ConstFNIsDeleted & "] = @deleted "
                    aCommand.OrderBy = "[" & ObjectMessage.ConstFNNo & "] asc"
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@tag", ColumnName:=ObjectMessage.ConstFNTag, tablename:=ObjectMessage.ConstTableID))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=ObjectMessage.ConstFNIsDeleted, tablename:=ObjectMessage.ConstTableID))
                    aCommand.Prepare()
                End If
                aCommand.SetParameterValue(ID:="@tag", value:=aTag)
                aCommand.SetParameterValue(ID:="@deleted", value:=False)

                Dim aRecordCollection = aCommand.RunSelect

                For Each aRecord As ormRecord In aRecordCollection
                    Dim aMessage As New ObjectMessage
                    If aMessage.InfuseDataObject(record:=aRecord, dataobject:=aMessage) Then
                        newCollection.Add(item:=aMessage)
                    End If
                Next

                Return newCollection


            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="ObjectMessageLog.Retrieve")
                Return newCollection

            End Try
        End Function
        '*** addMsg adds a Message to the MessageLog with the associated
        '***
        '*** Contextordinal (can be Nothing) as MQF or other ordinal
        '*** Tupleordinal (can be Nothing) as Row or Dataset
        '*** Entity (can be Nothing) per Field or ID

        '***
        '*** looks up the Messages and Parameters from the MessageLogTable
        '*** returns true if successfull

        ''' <summary>
        ''' adds a message of the message type uid to the log
        ''' </summary>
        ''' <param name="msguid"></param>
        ''' <param name="ContextIdentifier"></param>
        ''' <param name="TupleIdentifier"></param>
        ''' <param name="EntitityIdentifier"></param>
        ''' <param name="Args"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Add(ByVal msguid As Long,
                            ByVal domainid As String,
                             ByVal contextIdentifier As String, _
                             ByVal tupleIdentifier As String, _
                             ByVal entitityIdentifier As String, _
                             ParamArray args() As Object) As Boolean


            ''' default values
            If domainid Is Nothing OrElse domainid = "" Then domainid = CurrentSession.CurrentDomainID
            If contextIdentifier Is Nothing OrElse contextIdentifier = "" Then contextIdentifier = Me.ContextIdentifier
            If tupleIdentifier Is Nothing OrElse tupleIdentifier = "" Then tupleIdentifier = Me.TupleIdentifier
            If entitityIdentifier Is Nothing OrElse entitityIdentifier = "" Then entitityIdentifier = Me.EntitityIdentifier

            ''' 
            ''' get the Message Definition
            Dim aMessageDefinition As ObjectMessageType = ObjectMessageType.Retrieve(uid:=msguid, domainID:=domainid)
            If aMessageDefinition Is Nothing Then
                CoreMessageHandler(message:="object message type of uid '" & msguid.ToString & "' could not be retrieved", subname:="ObjectMessageLog.Add", _
                                   messagetype:=otCoreMessageType.InternalError, objectname:=_container.ObjectID, arg1:=Me.Tag)
            End If

            ''' create a Message
            ''' 
            Dim anIDNo As Long
            If Me.Size > 0 Then
                anIDNo = Me.MaxMessageNo + 1
            Else
                anIDNo = 1
            End If

            '''

            Dim aMessage As ObjectMessage = ObjectMessage.Create(msglogtag:=Me.Tag, no:=anIDNo, messageTypeUID:=msguid, _
                                                                 contextIdentifier:=contextIdentifier, tupleIdentifier:=tupleIdentifier, entitityIdentifier:=entitityIdentifier, _
                                                                 parameters:=args, runtimeOnly:=aMessageDefinition.IsPersisted)


            If aMessage IsNot Nothing Then
                Me.Add(item:=aMessage)
                Return True
            End If


            Return False
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAllMessageTexts() As List(Of String)
            Dim aList As New List(Of String)

            For Each aMessage In Me
                aList.Add(aMessage.Message)
            Next

            Return aList
        End Function
       

        Public Function GetStatus(Optional ByVal TYPEID As Object = Nothing, Optional ByVal i As Integer = 0) As Object
            'Dim max As Integer
            'Dim curweight As Single
            'Dim aMember As ObjectMessage
            'Dim aDefMSG As New ObjectMessageType
            'Dim code As String
            'Dim aStatus As New StatusItem

            'Dim newStatus As New StatusItem

            '' specific of an entry
            'If Not IsNothing(i) And i > 0 Then
            '    If i <= Me.Count Then
            '        aMember = Me.GetMember(i)
            '        aDefMSG = aMember.MessageType
            '        '** per TypeID
            '        If Not IsNothing(TYPEID) Then
            '            code = aDefMSG.StatusCodeOf(TYPEID)
            '            If aStatus.Inject(TYPEID, code) Then
            '                GetStatus = aStatus
            '            Else
            '                GetStatus = Nothing
            '            End If
            '        Else
            '            Dim code1, code2, code3 As String
            '            Dim weight1, weight2, weight3 As Integer
            '            Dim status1 As New StatusItem
            '            Dim status2 As New StatusItem
            '            Dim status3 As New StatusItem

            '            'If status1.Inject(aDefMSG.Statustype1, aDefMSG.Statuscode1) Then
            '            '    weight1 = status1.Weight
            '            'Else
            '            '    weight1 = 0
            '            'End If
            '            'If status2.Inject(aDefMSG.Statustype2, aDefMSG.Statuscode2) Then
            '            '    weight2 = status2.Weight
            '            'Else
            '            '    weight2 = 0
            '            'End If
            '            'If status3.Inject(aDefMSG.Statustype3, aDefMSG.Statuscode3) Then
            '            '    weight3 = status3.Weight
            '            'Else
            '            '    weight3 = 0
            '            'End If
            '            ' get maximum
            '            If weight1 = 0 And weight2 = 0 And weight3 = 0 Then
            '                GetStatus = Nothing
            '            ElseIf weight1 >= weight2 And weight1 >= weight3 Then
            '                GetStatus = status1
            '            ElseIf weight2 >= weight1 And weight2 >= weight3 Then
            '                GetStatus = status2
            '            ElseIf weight3 >= weight2 And weight3 >= weight2 Then
            '                GetStatus = status3
            '            End If

            '        End If
            '    Else
            '        GetStatus = Nothing
            '    End If
            '    Exit Function
            'End If

            '' else return the maximum
            'If Me.Count = 0 And Not IsNothing(TYPEID) Then
            '    If TYPEID = OTDBConst_StatusTypeid_ScheduleProcess Then
            '        'GetStatus = s_DefaultProcessStatus
            '        Exit Function
            '    ElseIf TYPEID = OTDBConst_StatusTypeid_FCLF Then
            '        'GetStatus = s_DefaultFCLCstatus
            '        Exit Function
            '    End If
            'End If

            '' return the status assoc. with the highest weight of messages
            'curweight = 0
            'For i = 1 To Me.Count
            '    If Me.GetWeight(i) > curweight Then
            '        curweight = Me.GetWeight(i)
            '        aMember = Me.GetMember(i)
            '        aDefMSG = aMember.MessageType
            '        code = aDefMSG.StatusCodeOf(TYPEID)
            '        aStatus = New StatusItem
            '        If aStatus.Inject(TYPEID, code) Then
            '            GetStatus = aStatus
            '        Else
            '            GetStatus = Nothing
            '        End If
            '    End If
            'Next i

        End Function

        Public Function GetProcessStatus(Optional ByVal i As Integer = 0) As Object

            GetProcessStatus = Me.GetStatus(OTDBConst_StatusTypeid_ScheduleProcess, i)


        End Function
        Public Function getFCLCStatus(Optional ByVal i As Integer = 0) As Object
            getFCLCStatus = Me.GetStatus(OTDBConst_StatusTypeid_FCLF, i)
        End Function
       


    End Class



    ''' <summary>
    ''' Message Entries of a Object Log 
    ''' </summary>
    ''' <remarks></remarks>

    <ormObject(version:=1, id:=ObjectMessage.ConstObjectID, modulename:=ConstModuleCommons)> Public Class ObjectMessage
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '* schema
        Public Const ConstObjectID = "ObjectMessage"

        ''' <summary>
        ''' Table
        ''' </summary>
        ''' <remarks></remarks>
        <ormSchemaTable(version:=1)> Public Const ConstTableID As String = "tblObjectMessages"

        ''' <summary>
        ''' Primary Key Entries
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(typeid:=otDataType.Text, PosOrdinal:=1, PrimarykeyOrdinal:=1, _
                         ID:="olog1", title:="Tag", description:="tag to the object message log")> Public Shadows Const ConstFNTag = "msglogtag"
        <ormObjectEntry(typeid:=otDataType.Long, PrimarykeyOrdinal:=2, _
                         ID:="olog2", title:="Number", description:="number of the object message")> Public Const ConstFNNo = "idno"

        ''' <summary>
        ''' ColumnEntries
        ''' </summary>
        ''' <remarks></remarks>
        <ormObjectEntry(referenceObjectEntry:=ObjectMessageType.ConstObjectID & "." & ObjectMessageType.ConstFNUID, _
                         ID:="olog3")> Public Const ConstFNMessageTypeUID = ObjectMessageType.ConstFNUID

        <ormObjectEntry(referenceobjectentry:=ObjectMessageType.ConstObjectID & "." & ObjectMessageType.constFNText, isnullable:=True, _
                         ID:="olog4", title:="Message", description:="the object message")> Public Const ConstFNMessage = "MESSAGE"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         ID:="olog5", title:="ContextID", description:="context of the object message")> Public Const ConstFNContextID = "CONTEXTID"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         ID:="olog6", title:="TupleID", description:="tuple of the object message")> Public Const ConstFNTupleID = "TUPLEID"
        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                         ID:="olog7", title:="EntityID", description:="entity of the object message")> Public Const ConstFNEntityID = "ENTITYID"
        <ormObjectEntry(typeid:=otDataType.List, isnullable:=True, _
                        ID:="olog8", title:="Parameters", description:="parameters for the message")> Public Const ConstFNParameters = "PARAMETERS"

        <ormObjectEntry(typeid:=otDataType.Timestamp, isnullable:=True, _
                       ID:="olog9", title:="Timestamp", description:="timestamp of the message")> Public Const ConstFNTimeStamp = "TIMESTAMP"

        <ormObjectEntry(referenceObjectEntry:=ObjectMessageType.ConstObjectID & "." & ObjectMessageType.constFNArea, isnullable:=True, _
                        ID:="olog11")> Public Const ConstFNArea = "AREA"
        <ormObjectEntry(referenceObjectEntry:=ObjectMessageType.ConstObjectID & "." & ObjectMessageType.constFNWeight, isnullable:=True, _
                       ID:="olog12")> Public Const ConstFNWeight = "WEIGHT"

        <ormObjectEntry(referenceObjectEntry:=User.ConstObjectID & "." & User.ConstFNUsername, isnullable:=True, _
                       ID:="olog13", title:="Username", description:="username of the session")> Public Const ConstFNUsername = "USER"

        <ormObjectEntry(typeid:=otDataType.Text, size:=100, isnullable:=True, _
                       ID:="olog14", title:="Session", description:="session in which the error occured")> Public Const ConstFNSessionTAG = "SESSIONTAG"

        <ormObjectEntry(referenceObjectEntry:=SessionMessage.ConstObjectID & "." & SessionMessage.ConstFNID, isnullable:=True, _
                      ID:="olog15", title:="Session Message No", description:="referenced session message no")> Public Const ConstFNSessionMSGNo = "SESSIONMSGNO"

        <ormObjectEntry(referenceObjectEntry:=Workspace.ConstObjectID & "." & Workspace.ConstFNID, isnullable:=True, _
                     ID:="olog16", title:="current Workspace id", description:="current workspace id")> Public Const ConstFNWORKSPACEID = "WORKSPACEID"
        ''' <summary>
        ''' Mappings
        ''' </summary>
        ''' <remarks></remarks>
        <ormEntryMapping(EntryName:=ConstFNTag)> Private _tag As String = ""
        <ormEntryMapping(EntryName:=ConstFNNo)> Private _no As Long
        <ormEntryMapping(EntryName:=ConstFNMessageTypeUID)> Private _typeuid As Long
        <ormEntryMapping(EntryName:=ConstFNMessage)> Private _message As String

        <ormEntryMapping(EntryName:=ConstFNContextID)> Private _ContextID As String
        <ormEntryMapping(EntryName:=ConstFNTupleID)> Private _TupleID As String
        <ormEntryMapping(EntryName:=ConstFNEntityID)> Private _EntitityID As String
        <ormEntryMapping(EntryName:=ConstFNParameters)> Private _Parameters As String()

        <ormEntryMapping(EntryName:=ConstFNArea)> Private _Area As String
        <ormEntryMapping(EntryName:=ConstFNWeight)> Private _Weight As Double?
        <ormEntryMapping(EntryName:=ConstFNTimeStamp)> Private _Timestamp As DateTime?
        <ormEntryMapping(EntryName:=ConstFNUsername)> Private _username As String
        <ormEntryMapping(EntryName:=ConstFNSessionTAG)> Private _sessionid As String
        <ormEntryMapping(EntryName:=ConstFNWorkspaceID)> Private _workspaceID As String
        <ormEntryMapping(EntryName:=ConstFNSessionMSGNo)> Private _sessionmsgno As Long
        ''' <summary>
        ''' Relation to ScheduleDefinition
        ''' </summary>
        ''' <remarks></remarks>
        <ormRelation(linkObject:=GetType(ObjectMessageType), toprimaryKeys:={ConstFNMessageTypeUID}, _
                     cascadeonCreate:=True, cascadeOnDelete:=False, cascadeOnUpdate:=False)> _
        Public Const ConstRMessageType = "RelMessageType"

        <ormEntryMapping(relationName:=ConstRMessageType, infusemode:=otInfuseMode.OnCreate OrElse otInfuseMode.OnInject OrElse otInfuseMode.OnDemand)> Private _messagetype As New ObjectMessageType

        ''' <summary>
        ''' runtime dynamic members
        ''' </summary>
        ''' <remarks></remarks>
        Private _lock As New Object


#Region "properties"

        ''' <summary>
        ''' Gets or sets the workspace ID.
        ''' </summary>
        ''' <value>The workspace ID.</value>
        Public Property WorkspaceID() As String
            Get
                Return Me._workspaceID
            End Get
            Set(value As String)

                SetValue(ConstFNWORKSPACEID, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the sessionid.
        ''' </summary>
        ''' <value>The sessionid.</value>
        Public Property Sessionid() As String
            Get
                Return Me._sessionid
            End Get
            Set
                SetValue(ConstFNSessionTAG, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the sessionmsgno.
        ''' </summary>
        ''' <value>The sessionmsgno.</value>
        Public Property SessionMessageNo() As Long
            Get
                Return Me._sessionmsgno
            End Get
            Set(value As Long)
                SetValue(ConstFNSessionMSGNo, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the weight.
        ''' </summary>
        ''' <value>The weight.</value>
        Public Property Weight() As Double?
            Get
                Return Me._Weight
            End Get
            Set
                Me._Weight = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the area.
        ''' </summary>
        ''' <value>The area.</value>
        Public Property Area() As String
            Get
                Return Me._Area
            End Get
            Set
                SetValue(ConstFNArea, Value)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the parameters.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public Property Parameters() As String()
            Get
                Return Me._Parameters
            End Get
            Set
                SetValue(ConstFNParameters, Value)
            End Set
        End Property

        ''' <summary>
        ''' gets the tag of the log message
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Tag() As String
            Get
                Return _tag
            End Get
        End Property

        ''' <summary>
        ''' gets the index number
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property No() As Long
            Get
                Return _no
            End Get

        End Property
        ''' <summary>
        ''' gets or sets the message type uid
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Msgid() As Long
            Get
                Return _typeuid
            End Get
            Set(avalue As Long)
                SetValue(ConstFNMessageTypeUID, avalue)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the messagetext
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Message() As String
            Get
                Return _message
            End Get
            Private Set(value As String)
                SetValue(ConstFNMessage, value)
            End Set
        End Property

        ''' <summary>
        ''' gets the Message type object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MessageType() As ObjectMessageType
            Get
                If Me.GetRelationStatus(ConstRMessageType) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRMessageType)
                Return _messagetype
            End Get

        End Property

        ''' <summary>
        ''' gets or sets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContextIdentifier() As Object
            Get
                Return _ContextID
            End Get
            Set(value As Object)
                SetValue(ConstFNContextID, value)
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the data tupple identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TupleIdentifier() As Object
            Get
                Return _TupleID
            End Get
            Set(avalue As Object)
                SetValue(ConstFNTupleID, value:=avalue)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the entitity identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EntitityIdentifier() As Object
            Get
                Return _EntitityID
            End Get
            Set(value As Object)
                SetValue(ConstFNEntityID, value)
            End Set
        End Property
#End Region



        ''' <summary>
        ''' loads and infuses a message log member
        ''' </summary>
        ''' <param name="msglogtag"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Retrieve(ByVal msglogtag As String, ByVal ID As Long) As ObjectMessage
            Dim primarykey() As Object = {msglogtag.ToUpper, ID}
            Return ormDataObject.Retrieve(Of ObjectMessage)(primarykey)
        End Function


        ''' <summary>
        ''' Create a persistable Message Log Member by primary key
        ''' </summary>
        ''' <param name="msglogtag"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Create(ByVal msglogtag As String, _
                                      ByVal no As Long, _
                                      ByVal messageTypeUID As Long, _
                                      ByVal contextIdentifier As String, _
                                      ByVal tupleIdentifier As String, _
                                      ByVal entitityIdentifier As String, _
                                      Optional parameters As Object() = Nothing,
                                      Optional ByVal domainid As String = "", _
                                      Optional checkUnique As Boolean = False, _
                                      Optional runtimeOnly As Boolean = True) As ObjectMessage
            If domainid = "" Then domainid = CurrentSession.CurrentDomainID
            Dim aRecord As New ormRecord
            With aRecord
                .SetValue(ConstFNTag, msglogtag.ToUpper)
                .SetValue(ConstFNMessageTypeUID, messageTypeUID)
                .SetValue(ConstFNNo, No)
                .SetValue(ConstFNDomainID, domainid)
                .SetValue(ConstFNContextID, contextIdentifier)
                .SetValue(ConstFNTupleID, tupleIdentifier)
                .SetValue(ConstFNEntityID, entitityIdentifier)

                If parameters IsNot Nothing Then .SetValue(ConstFNParameters, Converter.Array2String(parameters))
            End With
            Return ormDataObject.CreateDataObject(Of ObjectMessage)(aRecord, checkUnique:=checkUnique, runtimeOnly:=runtimeOnly)
        End Function

        ''' <summary>
        ''' handles the default value needed event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectMessage_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnDefaultValuesNeeded

            ''' defaults
            If Not e.Record.HasIndex(ConstFNSessionTAG) OrElse e.Record.GetValue(ConstFNSessionTAG) Is Nothing Then e.Record.SetValue(ConstFNSessionTAG, CurrentSession.SessionID)
            If not e.record.HasIndex(ConstFNUsername) orelse e.Record.GetValue(ConstFNUsername) Is Nothing Then e.Record.SetValue(ConstFNUsername, CurrentSession.Username)
            If Not e.Record.HasIndex(ConstFNDomainID) OrElse e.Record.GetValue(ConstFNDomainID) Is Nothing Then e.Record.SetValue(ConstFNDomainID, CurrentSession.CurrentDomainID)
            If Not e.Record.HasIndex(ConstFNWorkspaceID) OrElse e.Record.GetValue(ConstFNWorkspaceID) Is Nothing Then e.Record.SetValue(ConstFNWorkspaceID, CurrentSession.CurrentWorkspaceID)
            If Not e.Record.HasIndex(ConstFNTimeStamp) OrElse e.Record.GetValue(ConstFNTimeStamp) Is Nothing Then e.Record.SetValue(ConstFNTimeStamp, Date.Now)

        End Sub

        ''' <summary>
        ''' Infused Handler to set some stuff
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub ObjectMessage_OnInfused(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnInfused
            Dim aMessageDefinition = Me.MessageType
            If aMessageDefinition IsNot Nothing Then

                ''' set the values from the definition
                Me.Message = aMessageDefinition.Message
                Me.Weight = aMessageDefinition.Weight
                Me.Area = aMessageDefinition.Area
                If Me.Sessionid Is Nothing Then Me.Sessionid = CurrentSession.SessionID

                ''' replace
                ''' 
                If Me.TupleIdentifier IsNot Nothing Then
                    Me.Message = Strings.Replace(Me.Message, "%uid%", Me.TupleIdentifier)
                    Me.Message = Strings.Replace(Me.Message, "%Tupleid%", Me.TupleIdentifier)
                End If
                If Me.ContextIdentifier IsNot Nothing Then
                    Me.Message = Strings.Replace(Me.Message, "%contextid%", ContextIdentifier)
                End If
                If Me.EntitityIdentifier IsNot Nothing Then
                    Me.Message = Strings.Replace(Me.Message, "%entitiyid%", EntitityIdentifier)
                    Me.Message = Strings.Replace(Me.Message, "%ids%", EntitityIdentifier)
                End If

                'aMember.message = Replace(aMember.message, "%rowno%", aRowNo)
                Me.Message = Replace(Me.Message, "%type%", aMessageDefinition.type.ToString.ToUpper)
                Me.Message = Strings.Replace(Me.Message, "%errno%", Strings.Format(aMessageDefinition.ID, "00000"))

                '*
                For i = LBound(Me.Parameters) To UBound(Me.Parameters)
                    Me.Message = Strings.Replace(Me.Message, "%" & i + 1, Me.Parameters(i))
                Next i
            End If
        End Sub
    End Class
End Namespace
