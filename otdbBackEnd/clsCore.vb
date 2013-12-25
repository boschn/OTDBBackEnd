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

Namespace OnTrack


    '************************************************************************************
    '***** Session Class holds all the Session based Data for On Track Database
    '***** 
    '*****

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
        Private _OTDBUser As User
        Private _Username As String = ""
        Private _errorLog As ErrorLog
        Private _logagent As SessionAgent
        Private _UseConfigSetName As String = ""
        Private _CurrentDomainID As String = ConstGlobalDomain
        Private _loadDomainReqeusted As Boolean = False
        Private _CurrentWorkspaceID As String = ""

        ' initialized Flag
        Private _IsInitialized As Boolean = False
        Private _IsRunning As Boolean = False

        ' the environments
        Private WithEvents _primaryDBDriver As iormDBDriver
        Private WithEvents _primaryConnection As iormConnection


        Private _CurrentDomain As Domain
        Private _UILogin As UI.clsCoreUILogin
        Private _AccessLevel As otAccessRight    ' access

        Private _DomainObjectsDir As New Dictionary(Of String, ObjectStore)
        'shadow Reference for Events
        ' our Events
        Public Event OnDomainChanging As EventHandler(Of SessionEventArgs)
        Public Event OnDomainChanged As EventHandler(Of SessionEventArgs)
        Public Event OnStarted As EventHandler(Of SessionEventArgs)
        Public Event OnEnding As EventHandler(Of SessionEventArgs)
        Public Event OnConfigSetChange As EventHandler(Of SessionEventArgs)
        Public Event ObjectDefinitionChanged As EventHandler(Of ObjectDefintionEventArgs)

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
        Public Sub New(Optional id As String = "", Optional configSetname As String = "")
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
            _errorLog = New ErrorLog(_SessionID)
            _logagent = New SessionAgent(Me)

            If configSetname <> "" Then
                _UseConfigSetName = configSetname
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
        End Sub

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the domain.
        ''' </summary>
        ''' <value>The domain.</value>
        Public Property CurrentDomainID() As String
            Get
                Return Me._CurrentDomainID
            End Get
            Set(value As String)

                Call SetDomain(value)
            End Set
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
                Me._DefaultDeliverableTypeID = Value
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
                Me._AccessLevel = Value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the Objects.
        ''' </summary>
        ''' <value>The Objects.</value>
        Public ReadOnly Property Objects() As ObjectStore
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
        ''' returns the ConfigSetName to be used to connect to the databased
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ConfigSetName As String
            Get
                Return _UseConfigSetName
            End Get
            Set(value As String)
                If _UseConfigSetName <> value Then
                    If Not Me.IsRunning Then
                        '*
                        If Me.Initialize(useConfigsetName:=value) Then
                            _UseConfigSetName = value
                        End If
                    Else
                        CoreMessageHandler(message:="a running session can not be set to another config set name", arg1:=value, messagetype:=otCoreMessageType.ApplicationError, subname:="Sesion.ConfigSetName")
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
        ''' Gets or sets the default workspaceID.
        ''' </summary>
        ''' <value>The default workspaceID.</value>
        Public Property CurrentWorkspaceID() As String
            Get
                Return Me._CurrentWorkspaceID
            End Get
            Set(value As String)
                Me._CurrentWorkspaceID = value
            End Set
        End Property
        ''' <summary>
        ''' the errorlog of the session
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Errorlog As ErrorLog
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
        Public Property UILogin() As UI.clsCoreUILogin
            Get
                If _UILogin Is Nothing Then
                    _UILogin = New UI.clsCoreUILogin
                End If
                Return Me._UILogin
            End Get
            Set(value As UI.clsCoreUILogin)
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
        Public Property CurrentDBDriver() As iormDBDriver
            Get
                If Me.IsInitialized OrElse Me.Initialize Then
                    Return Me._primaryDBDriver
                Else
                    Return Nothing
                End If
            End Get
            Protected Set(value As iormDBDriver)
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
        ''' Initialize the Session 
        ''' </summary>
        ''' <param name="DBDriver">DBDriver to be provided</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Initialize(Optional dbDriver As iormDBDriver = Nothing, Optional useConfigsetName As String = "") As Boolean
            Dim aValue As Object

            ' set the configuration set to be used
            If useConfigsetName = "" Then
                _UseConfigSetName = ot.CurrentConfigSetName
            Else
                _UseConfigSetName = useConfigsetName
            End If

            '* load config set
            If Not ot.HasConfigSetName(_UseConfigSetName) Then
                If Not ot.HasConfigSetName(ConstGlobalConfigSetName) Then
                    CoreMessageHandler(showmsgbox:=True, message:="configuration-set name '" & ConstGlobalConfigSetName & "' does not exist - can not connect", subname:="Session.initialize", messagetype:=otCoreMessageType.InternalError)
                    Return False
                Else
                    CoreMessageHandler(showmsgbox:=True, message:="configuration-set name '" & useConfigsetName & "' does not exist - use '" & ConstGlobalConfigSetName & "' instead", _
                                       subname:="Session.Initialize", messagetype:=otCoreMessageType.InternalWarning)

                    _UseConfigSetName = ConstGlobalConfigSetName

                End If
            End If
            Try
                If dbDriver Is Nothing Then
                    _primaryDBDriver = ot.GetDatabaseDriver(session:=Me)
                Else
                    '** take the supplied one
                    _primaryDBDriver = dbDriver
                End If
                Call CoreMessageHandler(message:="The Database Driver is set to " & UCase(_primaryDBDriver.ID), _
                                       noOtdbAvailable:=True, subname:="Session.Initialize", _
                                       messagetype:=otCoreMessageType.InternalInfo)

                '** set the connection for events
                _primaryConnection = _primaryDBDriver.CurrentConnection

                '** create ObjectStore
                Dim aStore As New ObjectStore(Me)

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
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnConnecting(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnConnection
            Me.StartUpSessionEnviorment(FORCE:=True, domainID:=e.DomainID)
        End Sub

        ''' <summary>
        ''' initialize on Connection Event
        ''' </summary>
        ''' <value>The session ID.</value>

        Private Sub OnDisConnecting(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnDisconnection
            Me.ShutDownSessionEnviorment()
        End Sub

        ''' <summary>
        ''' requires from OTDB the Access Rights
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
                anUsername = _primaryConnection.OTDBUser.Username
                Return _primaryConnection.VerifyUserAccess(accessRequest:=accessRequest, username:=anUsername, domainID:=domainID, loginOnDemand:=True)
            Else
                If domainID = "" Then domainID = ConstGlobalDomain
                Me.StartUp(AccessRequest:=accessRequest, domainID:=domainID)
                Return _primaryConnection.ValidateAccessRequest(accessRequest:=accessRequest, domainID:=domainID)
            End If

        End Function
        ''' <summary>
        ''' Raises the Event ObjectChagedDefinition
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub RaiseObjectChangedDefinitionEvent(sender As Object, e As ObjectDefintionEventArgs)
            If _DomainObjectsDir.ContainsKey(key:=_CurrentDomainID) Then
                _DomainObjectsDir.Item(key:=_CurrentDomainID).OnObjectDefinitionChanged(sender, e)
            End If
        End Sub
        ''' <summary>
        ''' Returns a List of Higher Access Rights then the one selected
        ''' </summary>
        ''' <param name="accessrequest"></param>
        ''' <param name="domain" >Domain to validate for</param>
        ''' <param name="Objects" >list of Obejectnames to validate in the domain</param>
        ''' <returns>eturns false if reverification of User is needed or true if currentAccessLevel includes this new request Level</returns>
        ''' <remarks></remarks>

        Private Function HigherAccessRequest(ByVal accessrequest As otAccessRight) As List(Of String)

            Dim aResult As New List(Of String)

            If accessrequest = otAccessRight.AlterSchema Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
            End If

            If accessrequest = otAccessRight.ReadUpdateData Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
                aResult.Add(otAccessRight.ReadUpdateData.ToString)
            End If

            If accessrequest = otAccessRight.ReadOnly Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
                aResult.Add(otAccessRight.ReadUpdateData.ToString)
                aResult.Add(otAccessRight.ReadOnly.ToString)
            End If

            Return aResult
        End Function

        ''' <summary>
        ''' Validate the Access Request against the current Access Level of the user
        ''' </summary>
        ''' <param name="accessrequest"></param>
        ''' <param name="domain" >Domain to validate for</param>
        ''' <param name="Objects" >list of Obejectnames to validate in the domain</param>
        ''' <returns>eturns false if reverification of User is needed or true if currentAccessLevel includes this new request Level</returns>
        ''' <remarks></remarks>

        Public Function ValidateAccessRequest(accessrequest As otAccessRight, _
        Optional domain As String = "", _
        Optional ByRef [Objectnames] As List(Of String) = Nothing) As Boolean

            ' if we have no user -> reverification
            If _OTDBUser Is Nothing OrElse Not _OTDBUser.IsLoaded Then
                Return False
            End If

            If accessrequest = _AccessLevel Then
                Return True
            ElseIf accessrequest = otAccessRight.[ReadOnly] And _
            (_AccessLevel = otAccessRight.ReadUpdateData Or _AccessLevel = otAccessRight.AlterSchema) Then
                Return True
            ElseIf accessrequest = otAccessRight.ReadUpdateData And _AccessLevel = otAccessRight.AlterSchema Then
                Return True
                ' will never be reached !
            ElseIf accessrequest = otAccessRight.AlterSchema And _AccessLevel = otAccessRight.AlterSchema Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' verify the user access to OnTrack Database - if necessary start a Login with Loginwindow. Check on user rights.
        ''' </summary>
        ''' <param name="accessRequest">needed User right</param>
        ''' <param name="username">default username to use</param>
        ''' <param name="password">default password to use</param>
        ''' <param name="forceLogin">force a Login window in any case</param>
        ''' <param name="loginOnDemand">do a Login window and reconnect if right is not necessary</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function VerifyUserAccess(accessRequest As otAccessRight, _
        Optional ByRef username As String = "", _
        Optional ByRef password As String = "", _
        Optional ByRef domainID As String = "", _
        Optional ByRef [Objectnames] As List(Of String) = Nothing, _
        Optional loginOnDisConnected As Boolean = False, _
        Optional loginOnFailed As Boolean = False) As Boolean
            Dim userValidation As UserValidation
            userValidation.validEntry = False

            '****
            '**** no connection -> login
            '****
            If Not Me.IsRunning Then

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
                    Me.UILogin.EnableConfigSet = True

                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    Me.UILogin.Session = Me

                    Me.UILogin.Accessright = accessRequest
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = Me.HigherAccessRequest(accessrequest:=accessRequest)

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
                            SetDomain(Me.UILogin.Domain)
                        End If
                        '* validate
                        userValidation = _primaryDBDriver.GetUserValidation(username)
                    End If

                    ' just check the provided username
                ElseIf username <> "" And password <> "" And accessRequest <> ConstDefaultAccessRight Then
                    userValidation = _primaryDBDriver.GetUserValidation(username)
                    '* no username but default accessrequest then look for the anonymous user
                ElseIf accessRequest = ConstDefaultAccessRight Then
                    userValidation = _primaryDBDriver.GetUserValidation(username:="", selectAnonymous:=True)
                    If userValidation.validEntry Then
                        username = userValidation.Username
                        password = userValidation.Password
                    End If
                End If

                ' if user is still nothing -> not verified
                If Not userValidation.validEntry Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                            message:=" Access to OnTrack Database is prohibited - User not found", _
                                            arg1:=userValidation.Username, noOtdbAvailable:=True, break:=False)

                    '*** reset
                    Call ShutDown()
                    Return False
                Else
                    '**** Check Password
                    '****
                    If ot.ValidateUser(accessRequest:=accessRequest, username:=username, password:=password, domainID:=domainID, databasedriver:=_primaryDBDriver) Then
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
                If Me.ValidateAccessRequest(accessrequest:=accessRequest, domain:=domainID) Then
                    Return True
                    '* change the current user if anonymous
                    '*
                ElseIf loginOnFailed And ot.CurrentConnection.OTDBUser.IsAnonymous Then
                    '** check if new OTDBUsername is valid
                    'LoginWindow
                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    Me.UILogin.PossibleDomains = New List(Of String)
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                    Me.UILogin.Configset = _UseConfigSetName
                    Me.UILogin.EnableConfigSet = False
                    Me.UILogin.Accessright = accessRequest
                    Me.UILogin.Messagetext = "<html><strong>Welcome !</strong><br />Please change to a valid user and password for the needed access right.</html>"
                    Me.UILogin.EnableUsername = True
                    Me.UILogin.Username = ""
                    Me.UILogin.Password = ""
                    Me.UILogin.Session = Me

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
                    If ot.ValidateUser(accessRequest:=accessRequest, username:=username, password:=password, _
                                       domainID:=domainID, databasedriver:=_primaryDBDriver) Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, _
                                                message:="User change verified successfully on domain '" & domainID & "'", _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        If Me.CurrentDomainID <> Me.UILogin.Domain Then
                            SetDomain(Me.UILogin.Domain)
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
                ElseIf loginOnFailed And Not ot.CurrentConnection.OTDBUser.IsAnonymous Then
                    '** check if new OTDBUsername is valid
                    'LoginWindow
                    Me.UILogin.Domain = domainID
                    Me.UILogin.EnableDomain = False
                    Me.UILogin.PossibleDomains = New List(Of String)
                    Me.UILogin.enableAccess = True
                    Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                    Me.UILogin.Configset = _UseConfigSetName
                    Me.UILogin.EnableConfigSet = False
                    Me.UILogin.Accessright = accessRequest

                    Me.UILogin.Messagetext = "<html><strong>Attention !</strong><br />Please confirm by your password to obtain the access right.</html>"
                    Me.UILogin.EnableUsername = False
                    Me.UILogin.Username = ot.CurrentConnection.OTDBUser.Username
                    Me.UILogin.Password = password
                    Me.UILogin.Session = Me

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
                        SetDomain(Me.UILogin.Domain)
                    End If
                    If Me.CurrentDomainID <> Me.UILogin.Domain Then
                        SetDomain(Me.UILogin.Domain)
                    End If
                    userValidation = _primaryDBDriver.GetUserValidation(username)
                    '* check password
                    If ot.ValidateUser(accessRequest:=accessRequest, username:=username, password:=password, _
                                       domainID:=domainID, databasedriver:=_primaryDBDriver) Then
                        Call CoreMessageHandler(subname:="Session.verifyUserAccess", break:=False, message:="User change verified successfully", _
                                                arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
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
        Optional OTDBPassword As String = "") As Boolean
            Dim aConfigsetname As String
            If useconfigsetname <> "" AndAlso ot.HasConfigSetName(useconfigsetname, ConfigSequence.primary) Then
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
                Return False
            End If
            '** domain
            If domainID = "" Then Me.CurrentDomainID = ConstGlobalDomain ' set the current domain (_domainID)

            If VerifyUserAccess(accessRequest:=AccessRequest, username:=OTDBUsername, _
                                password:=OTDBPassword, domainID:=_CurrentDomainID, loginOnDisConnected:=True, loginOnFailed:=True) Then
                '** Connect 
                Return _primaryConnection.Connect(FORCE:=True, _
                                                  access:=AccessRequest, domainID:=domainID, _
                                                  OTDBUsername:=OTDBUsername, _
                                                  OTDBPassword:=OTDBPassword, doLogin:=True)
                '** Initialize through events
            Else
                CoreMessageHandler(message:="user could not be verified", messagetype:=otCoreMessageType.InternalInfo, _
                                   subname:="Session.Startup")
                Return False
            End If

            Return True
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
            _errorLog.clear()
            Return True
        End Function

        ''' <summary>
        ''' sets the current Domain
        ''' </summary>
        ''' <param name="newDomainID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function SetDomain(newDomainID As String) As Boolean
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
            If _primaryDBDriver.HasTable(OnTrack.Domain.ConstTableID) Then
                newDomain = Domain.Retrieve(id:=newDomainID, dbdriver:=Me._primaryDBDriver)
                If newDomain Is Nothing Then
                    CoreMessageHandler(message:="domain does not exist", arg1:=newDomainID, subname:="Session.SetDomain", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
                newDomain.RegisterSession(Me)
                If Not _DomainObjectsDir.ContainsKey(key:=newDomainID) Then
                    Dim aStore = New ObjectStore(Me)
                    _DomainObjectsDir.Add(key:=newDomainID, value:=aStore)
                End If
                RaiseEvent OnDomainChanging(Me, New SessionEventArgs(Me, newDomain))

            Else

                '** bootstrapping database install
                newDomainID = ConstGlobalDomain
                'newDomain = New Domain()
                'newDomain.Create(domainID:=newDomainID)
                Me._CurrentDomain = Nothing
                Me._CurrentDomainID = newDomainID
                _loadDomainReqeusted = True
                RaiseEvent OnDomainChanging(Me, New SessionEventArgs(Me, Nothing))

                Return True
            End If


            '*** read the Domain Settings
            '***
            If newDomain IsNot Nothing Then
                '* change event

                If newDomain.hasSetting(id:=ConstCPDependencySynchroMinOverlap) Then
                    _DependencySynchroMinOverlap = newDomain.GetSetting(id:=ConstCPDependencySynchroMinOverlap).value
                Else
                    _DependencySynchroMinOverlap = 7
                End If

                If newDomain.hasSetting(id:=ConstCPDefaultWorkspace) Then
                    _DefaultWorkspace = newDomain.GetSetting(id:=ConstCPDefaultWorkspace).value
                    _CurrentWorkspaceID = _DefaultWorkspace
                Else
                    _DefaultWorkspace = ""
                End If

                If newDomain.hasSetting(id:=ConstCPDefaultCalendarName) Then
                    _DefaultCalendarName = newDomain.GetSetting(id:=ConstCPDefaultCalendarName).value
                Else
                    _DefaultCalendarName = "default"
                End If

                If newDomain.hasSetting(id:=ConstCPDefaultTodayLatency) Then
                    _TodayLatency = newDomain.GetSetting(id:=ConstCPDefaultTodayLatency).value
                Else
                    _TodayLatency = -14
                End If

                If newDomain.hasSetting(id:=ConstCDefaultScheduleTypeID) Then
                    _DefaultScheduleTypeID = newDomain.GetSetting(id:=ConstCDefaultScheduleTypeID).value
                Else
                    _DefaultScheduleTypeID = "none"
                End If

                If newDomain.hasSetting(id:=ConstCDefaultDeliverableTypeID) Then
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
        Private Function StartUpSessionEnviorment(Optional ByVal FORCE As Boolean = False, Optional domainID As String = "") As Boolean
            Dim aVAlue As Object

            If Not IsRunning Or FORCE Then

                '** start the Agent
                If Not _logagent Is Nothing Then
                    aVAlue = ot.GetConfigProperty(constCPNUseLogAgent)
                    If CBool(aVAlue) Then
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
                    IsRunning = False
                    Return False
                End If

                '*** Parameters
                '***
                _OTDBUser = _primaryDBDriver.CurrentConnection.OTDBUser
                If Not _OTDBUser Is Nothing AndAlso _OTDBUser.IsLoaded Then
                    _Username = _OTDBUser.Username
                End If

                '** load Domain
                If domainID = "" Then domainID = Me.CurrentDomainID
                '* set it here that we are really loading in SetDomain and not only 
                '* assigning _DomainID (if no connection is available)
                If setDomain(newDomainID:=domainID) Then
                    Call CoreMessageHandler(message:=" Session Domain set to " & domainID, _
                                            messagetype:=otCoreMessageType.InternalInfo, _
                                            subname:="Session.startupSesssionEnviorment")
                End If

                '*** Object to load initially
                aVAlue = _primaryDBDriver.GetDBParameter(ConstPNObjectsLoad, silent:=True)
                If aVAlue Is Nothing OrElse aVAlue = "" Then
                    Call _primaryDBDriver.SetDBParameter(ConstPNObjectsLoad, _
                                                         User.ConstTableID & "," & _
                                                         Scheduling.Schedule.ConstTableID & ", " & _
                                                         Scheduling.ScheduleMilestone.constTableID & ", " & _
                                                         Deliverables.Deliverable.ConstTableID & ", " & _
                                                         Parts.clsOTDBPart.constTableID _
                                                         , silent:=True)

                End If

                '*** set started
                IsRunning = True
                '*** we are started
                RaiseEvent OnStarted(Me, New SessionEventArgs(Me))

            End If
            Return IsRunning
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
        End Sub

        Private Sub Session_OnStarted(sender As Object, e As SessionEventArgs) Handles Me.OnStarted

        End Sub
    End Class
    ''' <summary>
    ''' Object Defintion Event Arguments
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectDefintionEventArgs
        Inherits EventArgs

        Private _objectname As String

        Public Sub New(objectname As String)
            _objectname = objectname
        End Sub
        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        ''' <value>The error.</value>
        Public ReadOnly Property objectname() As String
            Get
                Return _objectname
            End Get
        End Property

    End Class


    '**************
    '************** SessionEventArgs for the SessionEvents
    ''' <summary>
    ''' Session Event Arguments
    ''' </summary>
    ''' <remarks></remarks>

    Public Class SessionEventArgs
        Inherits EventArgs

        Private _Session As Session
        Private _NewDomain As Domain

        Public Sub New(Session As Session, Optional newDomain As Domain = Nothing)
            _Session = Session
            _NewDomain = newDomain
        End Sub
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
            _Session = Session
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
    ''' 
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
    '************************************************************************************
    '******* CLASS clsOTDBError describes an ErrorCondition
    '*******
    ''' <summary>
    ''' describes a persistable Core Error Message
    ''' </summary>
    ''' <remarks></remarks>

    Public Class CoreError
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable
        Implements iormCloneable
        Implements ICloneable

        '*** CONST Schema
        '** Table
        <ormSchemaTableAttribute(Version:=4)> Public Const ConstTableID = "tblSessionErrorlog"

        '*** Schema Field Definitions
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=100, _
        title:="session", Description:="sessiontag", _
        primaryKeyordinal:=1)> _
        Public Const ConstFNTag As String = "tag"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Long, _
        title:="no", Description:="number of entry", _
        primaryKeyordinal:=2)> _
        Public Const ConstFNno As String = "no"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=100, isnullable:=True, _
        title:="message id", Description:="id of the message")> _
        Public Const ConstFNID As String = "id"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Memo, isnullable:=True, _
        title:="message", Description:="message text")> _
        Public Const ConstFNmessage As String = "message"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=100, isnullable:=True, _
        title:="routine", Description:="routine name")> _
        Public Const ConstFNsubname As String = "subname"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Timestamp, isnullable:=True, _
        title:="timestamp", Description:="timestamp of entry")> _
        Public Const ConstFNtimestamp As String = "timestamp"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=100, isnullable:=True, _
        title:="tablename", Description:="tablename")> _
        Public Const ConstFNtablename As String = "tablename"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=100, isnullable:=True, _
        title:="fieldname", Description:="fieldname")> _
        Public Const ConstFNfieldname As String = "fieldname"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, size:=255, isnullable:=True, _
        title:="argument", Description:="argument of the message")> _
        Public Const ConstFNarg As String = "arg"
        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Long, isnullable:=True, _
        title:="message type id", Description:="id of the message type")> _
        Public Const ConstFNtype As String = "typeid"
        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, isnullable:=True, title:="Username of the session", Description:="name of the user for this session")> _
        Public Const ConstFNUsername As String = "username"
        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Memo, isnullable:=True, title:="stack trace", Description:="caller stack trace")> _
        Public Const ConstFNStack As String = "stack"
        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNTag)> Private _tag As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNID)> Private _id As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNno)> Private _entryno As Long = 0
        <ormColumnMappingAttribute(fieldname:=ConstFNmessage)> Private _Message As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNsubname)> Private _Subname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNtimestamp)> Private _Timestamp As Date = ConstNullDate
        <ormColumnMappingAttribute(fieldname:=ConstFNtablename)> Private _Tablename As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNfieldname)> Private _EntryName As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNtype)> Private _ErrorType As otCoreMessageType
        <ormColumnMappingAttribute(fieldname:=ConstFNUsername)> Private _Username As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNStack)> Private _StackTrace As String = ""

        Private _processed As Boolean = False
        Private _Exception As Exception
        <ormColumnMappingAttribute(fieldname:=ConstFNarg)> Private _Arguments As String = ""

        Public Sub New()
            Call MyBase.New(ConstTableID)
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
            Set
                Me._StackTrace = Value
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
        Public Property EntryName() As String
            Get
                Return Me._EntryName
            End Get
            Set(value As String)
                Me._EntryName = value
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

        '''
#Region "DataObject"
        ''' <summary>
        ''' create the schema for this object
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of CoreError)(addToSchema:=True)
        End Function

        ''' <summary>
        ''' create a persistable Error
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal sessiontag As String, ByVal entryno As Long) As Boolean
            Dim primarykey() As Object = {sessiontag, entryno}
            ' create
            If MyBase.Create(primarykey, checkUnique:=False, noInitialize:=True) Then
                _tag = sessiontag
                _entryno = entryno
                Return Me.IsCreated
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize()
            Return _IsInitialized
        End Function
        ''' <summary>
        ''' load and infuse the object by primary key
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal sessiontag As String, ByVal entryno As Long) As Boolean
            Dim primarykey() As Object = {sessiontag, entryno}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' Persist the data object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Return MyBase.Persist(timestamp)
        End Function

#End Region

        Function Clone() As Object Implements System.ICloneable.Clone
            Dim aClone As New CoreError
            With aClone
                .Tag = Me.Tag.Clone
                .ID = Me.ID.Clone
                .Exception = Me.Exception
                .Username = Me.Username.Clone
                .Entryno = Me.Entryno
                .Tablename = Me.Tablename.Clone
                .EntryName = Me.EntryName.Clone
                .Message = Me.Message.Clone
                .messagetype = Me.messagetype
                .Timestamp = Me.Timestamp
                .StackTrace = Me.StackTrace
            End With

            Return aClone
        End Function
    End Class

    Public Class otErrorEventArgs
        Inherits EventArgs

        Private _error As CoreError

        Public Sub New(newError As CoreError)
            _error = newError
        End Sub
        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        ''' <value>The error.</value>
        Public ReadOnly Property [Error]() As CoreError
            Get
                Return Me._error
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Describes an ErrorLog of Core Errors
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ErrorLog
        Implements IEnumerable
        Implements ICloneable

        Public Event onErrorRaised As EventHandler(Of otErrorEventArgs)
        Public Event onLogClear As EventHandler(Of otErrorEventArgs)
        '*** log
        Private _log As New SortedList(Of Long, CoreError)
        Private _queue As New ConcurrentQueue(Of CoreError)
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
                Dim aList As List(Of CoreError) = _log.Values.ToList
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
            RaiseEvent onLogClear(Me, New otErrorEventArgs(Nothing))
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
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            '** we have a session
            If CurrentSession.IsRunning Then
                '*** only if the table is there
                If CurrentSession.CurrentDBDriver.GetTable(CoreError.ConstTableID) Is Nothing Then
                    Return False
                End If

                SyncLock _lockObject
                    For Each anError As CoreError In _log.Values
                        If Not anError.Processed Then
                            If anError.Create(sessiontag:=_tag, entryno:=anError.Entryno) Then
                                anError.Persist()
                                anError.Processed = True ' do not again
                            End If
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
        Public Sub Enqueue(otdberror As CoreError)
            Dim aClone As CoreError = otdberror.Clone

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

            RaiseEvent onErrorRaised(Me, New otErrorEventArgs(aClone))
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
        Public Function PeekFirst() As CoreError
            Dim anError As CoreError
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
        Public Function PeekLast() As CoreError
            Dim anError As CoreError
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
        Public Function Retain() As CoreError
            Dim anError As CoreError
            SyncLock _lockObject
                If _queue.TryDequeue([anError]) Then
                    Return anError
                Else
                    Return Nothing
                End If
            End SyncLock
        End Function

    End Class



    '************************************************************************************
    '***** CLASS XMapordinal represents the ordinal for the map

    Public Enum otordinalType
        longType
        stringType

    End Enum

    ''' <summary>
    ''' ordinal class describes values as ordinal values (ordering)
    ''' </summary>
    ''' <remarks></remarks>

    Public Class Ordinal
        Implements IEqualityComparer(Of Ordinal)
        Implements IConvertible
        Implements IComparable(Of Ordinal)
        Implements IComparer(Of Ordinal)

        Private _ordinalvalue As Object
        Private _ordinalType As otordinalType

        Public Sub New(ByVal value As Object)
            ' return depending on the type

            If TypeOf value Is Long Or TypeOf value Is Integer Or TypeOf value Is UShort _
            Or TypeOf value Is Short Or TypeOf value Is UInteger Or TypeOf value Is ULong Then
                _ordinalType = otordinalType.longType
                _ordinalvalue = CLng(value)
            ElseIf IsNumeric(value) Then
                _ordinalType = otordinalType.longType
                _ordinalvalue = CLng(value)
            ElseIf TypeOf value Is Ordinal Then
                _ordinalType = CType(value, Ordinal).Type
                _ordinalvalue = CType(value, Ordinal).Value

            ElseIf value.ToString Then
                _ordinalType = otordinalType.stringType
                _ordinalvalue = String.Copy(value.ToString)
            Else
                Throw New Exception("value is not casteable to a XMAPordinalType")

            End If

        End Sub
        Public Sub New(ByVal value As Object, ByVal type As otordinalType)
            _ordinalType = type
            Me.Value = value
        End Sub
        Public Sub New(ByVal type As otordinalType)
            _ordinalType = type
            _ordinalvalue = Nothing
        End Sub

        Public Function ToString() As String
            Return _ordinalvalue.ToString
        End Function
        ''' <summary>
        ''' Equalses the specified x.
        ''' </summary>
        ''' <param name="x">The x.</param>
        ''' <param name="y">The y.</param>
        ''' <returns></returns>
        Public Function [Equals](x As Ordinal, y As Ordinal) As Boolean Implements IEqualityComparer(Of Ordinal).[Equals]
            Select Case x._ordinalType
                Case otordinalType.longType
                    Return x.Value.Equals(y.Value)
                Case otordinalType.stringType
                    If String.Compare(x.Value, y.Value, False) = 0 Then
                        Return True
                    Else
                        Return False
                    End If
            End Select

            Return x.Value = y.Value
        End Function
        ''' <summary>
        ''' Compares two objects and returns a value indicating whether one is less
        ''' than, equal to, or greater than the other.
        ''' </summary>
        ''' <param name="x">The first object to compare.</param>
        ''' <param name="y">The second object to compare.</param>
        ''' <exception cref="T:System.ArgumentException">Neither <paramref name="x" /> nor
        ''' <paramref name="y" /> implements the <see cref="T:System.IComparable" /> interface.-or-
        ''' <paramref name="x" /> and <paramref name="y" /> are of different types and neither
        ''' one can handle comparisons with the other. </exception>
        ''' <returns>
        ''' A signed integer that indicates the relative values of <paramref name="x" />
        ''' and <paramref name="y" />, as shown in the following table.Value Meaning Less
        ''' than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" />
        ''' equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater
        ''' than <paramref name="y" />.
        ''' </returns>
        Public Function [Compare](x As Ordinal, y As Ordinal) As Integer Implements IComparer(Of Ordinal).[Compare]

            '** depend on the type
            Select Case x.Type
                Case otordinalType.longType
                    ' try to compare numeric
                    If IsNumeric(y.Value) Then
                        If Me.Value > CLng(y.Value) Then
                            Return 1
                        ElseIf Me.Value < CLng(y.Value) Then
                            Return -1
                        Else
                            Return 0

                        End If
                    Else
                        Return -1
                    End If
                Case otordinalType.stringType
                    Return String.Compare(y.Value, y.Value.ToString)

            End Select
        End Function
        ''' <summary>
        ''' Compares to.
        ''' </summary>
        ''' <param name="other">The other.</param>
        ''' <returns></returns>
        Public Function CompareTo(other As Ordinal) As Integer Implements IComparable(Of Ordinal).CompareTo
            Return Compare(Me, other)

        End Function

        ''' <summary>
        ''' Gets the hash code.
        ''' </summary>
        ''' <param name="obj">The obj.</param>
        ''' <returns></returns>
        Public Function GetHashCode(obj As Ordinal) As Integer Implements IEqualityComparer(Of Ordinal).GetHashCode
            Return _ordinalvalue.GetHashCode
        End Function
        ''' <summary>
        ''' Value of the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Get
                Select Case Me.Type
                    Case otordinalType.longType
                        Return CLng(_ordinalvalue)
                    Case otordinalType.stringType
                        Return CStr(_ordinalvalue)
                End Select
                Return Nothing
            End Get
            Set(value As Object)
                Select Case Me.Type
                    Case otordinalType.longType
                        _ordinalvalue = CLng(value)
                    Case otordinalType.stringType
                        _ordinalvalue = CStr(value)
                End Select

                _ordinalvalue = value
            End Set

        End Property
        ''' <summary>
        ''' Datatype of the ordinal
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Type As otordinalType
            Get
                Return _ordinalType
            End Get
        End Property
        ''' <summary>
        ''' gets the Typecode of the ordinal
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            If _ordinalType = otordinalType.longType Then
                Return TypeCode.UInt64
            ElseIf _ordinalType = otordinalType.stringType Then
                Return TypeCode.String
            Else
                Return TypeCode.Object
            End If

        End Function

        Public Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            Return _ordinalvalue <> Nothing
        End Function

        Public Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Return Convert.ToByte(_ordinalvalue)
        End Function

        Public Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Return Convert.ToChar(_ordinalvalue)
        End Function

        Public Function ToDateTime(provider As IFormatProvider) As Date Implements IConvertible.ToDateTime

        End Function

        Public Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Return Convert.ToDecimal(_ordinalvalue)
        End Function

        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Return Convert.ToDouble(_ordinalvalue)
        End Function

        Public Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Return Convert.ToInt16(_ordinalvalue)
        End Function

        Public Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Return Convert.ToInt32(_ordinalvalue)
        End Function

        Public Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Return Convert.ToInt64(_ordinalvalue)
        End Function

        Public Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Return Convert.ToSByte(_ordinalvalue)
        End Function

        Public Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Return Convert.ToSingle(_ordinalvalue)
        End Function

        Public Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return Convert.ToString(_ordinalvalue)
        End Function

        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            ' DirectCast(_ordinalvalue, conversionType)
        End Function

        Public Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Return Convert.ToUInt16(_ordinalvalue)
        End Function

        Public Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Return Convert.ToUInt32(_ordinalvalue)
        End Function

        Public Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Return Convert.ToUInt64(_ordinalvalue)
        End Function

        Public Shared Operator =(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value = y.Value
        End Operator
        Public Shared Operator <(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value < y.Value
        End Operator
        Public Shared Operator >(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value > y.Value
        End Operator
        Public Shared Operator <>(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value <> y.Value
        End Operator
        Public Shared Operator +(x As Ordinal, y As Ordinal) As Boolean
            Return x.Value + y.Value
        End Operator

        Function ToUInt64() As Integer
            If IsNumeric(_ordinalvalue) Then Return CLng(_ordinalvalue)
            Throw New NotImplementedException
        End Function
        ''' <summary>
        ''' compares this to an ordinal
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Equals(value As Ordinal) As Boolean
            Return Me.Compare(Me, value) = 0
        End Function

    End Class

    '************************************************************************************
    '***** CLASS Objects is the central store for all the OTDB Meta Object Definitions
    '*****
    '*****
    ''' <summary>
    ''' store for all the meto OTDB object information - loaded on connecting with the 
    ''' session
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectStore

        Private _IsInitialized As Boolean = False

        '** cache of the table objects
        Private _objectDirectory As New Dictionary(Of String, ObjectDefinition)
        '** cache on the columns object 
        Private _entryDirectory As New Dictionary(Of String, ObjectEntryDefinition)

        '** reference to all the XChange IDs
        Private _IDsDirectory As New Dictionary(Of String, List(Of ObjectEntryDefinition))
        '** reference to all the aliases
        Private _aliasDirectory As New Dictionary(Of String, List(Of ObjectEntryDefinition))

        '** reference to the session 
        Private _DomainID As String = ""
        Private WithEvents _Domain As Domain
        Private WithEvents _Session As Session

        Private _lock As New Object
        ''' <summary>
        ''' construction with link to the connection
        ''' </summary>
        ''' <param name="connection"></param>
        ''' <remarks></remarks>
      
        Sub New(ByRef Session As Session)
            _Session = Session

        End Sub
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
        ''' if an Object Definition changes
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnObjectDefinitionChanged(sender As Object, ent As OnTrack.ObjectDefintionEventArgs)
            Dim anObjectDef As ObjectDefinition = New ObjectDefinition

            If anObjectDef.LoadBy(ent.objectname, domainID:=_DomainID) Then
                If LoadObjectDefinition(anObjectDef) Then
                    CoreMessageHandler(message:="object definition of " & ent.objectname & " was reloaded in the Objects store", messagetype:=otCoreMessageType.InternalInfo)
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
        ''' Add an Entry by ID
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AddID(ByRef entry As ObjectEntryDefinition) As Boolean
            Dim entries As List(Of ObjectEntryDefinition)

            If _IDsDirectory.ContainsKey(key:=UCase(entry.ID)) Then
                entries = _IDsDirectory.Item(key:=UCase(entry.ID))
            Else
                entries = New List(Of ObjectEntryDefinition)
                SyncLock Me
                    _IDsDirectory.Add(key:=UCase(entry.ID), value:=entries)
                End SyncLock
            End If

            SyncLock Me
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
        Private Function AddAlias(ByRef entry As ObjectEntryDefinition) As Boolean
            Dim entries As List(Of ObjectEntryDefinition)

            For Each [alias] As String In entry.Aliases

                If _aliasDirectory.ContainsKey(key:=UCase([alias])) Then
                    entries = _aliasDirectory.Item(key:=UCase([alias]))
                Else
                    entries = New List(Of ObjectEntryDefinition)
                    SyncLock Me
                        _aliasDirectory.Add(key:=UCase([alias]), value:=entries)
                    End SyncLock
                End If

                SyncLock Me
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
                _IDsDirectory.Clear()
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
            Dim aTablestore As iormDataStore

            '* donot doe it again
            If Me.IsInitialized Then Return False

            If _DomainID = "" Then
                CoreMessageHandler(message:="DomainID is not set in objectStore", arg1:=Me._Session.SessionID, messagetype:=otCoreMessageType.InternalError, subname:="ObjectStore.Initialize")
                Return False
            End If

            '* too eaarly
            If _Session Is Nothing OrElse _Session.CurrentDBDriver Is Nothing _
                OrElse Not _Session.CurrentDBDriver.CurrentConnection.IsConnected Then
                Return False
            End If

            If _Session IsNot Nothing AndAlso _Session.IsRunning Then
                aTablestore = _Session.CurrentDBDriver.GetTableStore(ObjectEntryDefinition.ConstTableID)
            Else
                aTablestore = GetTableStore(ObjectEntryDefinition.ConstTableID)
            End If

            Dim theObjectnames() As String
            Dim objectsToLoad As Object = ot.GetDBParameter(ot.ConstPNObjectsLoad)
            Dim delimiters() As Char = {",", ";", ConstDelimiter}

            If objectsToLoad IsNot Nothing Then
                SyncLock _lock
                    If objectsToLoad.ToString = "*" Then
                        theObjectnames = ObjectDefinition.AllObjectnames(tablestore:=aTablestore).ToArray
                    Else
                        theObjectnames = objectsToLoad.ToString.Split(delimiters)
                    End If

                    CoreMessageHandler(message:="Initialize OnTrack Objects ....", messagetype:=otCoreMessageType.ApplicationInfo, subname:="Objects.Initialize")
                    '** load all objects with entries and aliases
                    For Each name In theObjectnames
                        name = Trim(LCase(name)) ' for some reasons bette to trim
                        Dim anObject As New ObjectDefinition
                        If anObject.LoadBy(objectname:=name, tablestore:=aTablestore, domainID:=_DomainID) Then
                            Me.LoadObjectDefinition(anObject)
                        Else
                            CoreMessageHandler(message:="could not load object '" & name & "'", messagetype:=otCoreMessageType.ApplicationError, _
                                               subname:="Objects.Initialize")
                        End If
                    Next
                End SyncLock
            End If

            SyncLock _lock
                Me.IsInitialized = True
            End SyncLock

            CoreMessageHandler(message:="Objects initialized for Domain '" & _DomainID & " in Session " & CurrentSession.SessionID & "' - " & _objectDirectory.Count & " objects loaded", _
                               messagetype:=otCoreMessageType.ApplicationInfo, subname:="Objects.Initialize")

            Return Me.IsInitialized
        End Function

        ''' <summary>
        ''' Load Object into Store of Objects
        ''' </summary>
        ''' <param name="object"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function LoadObjectDefinition(ByRef [object] As ObjectDefinition) As Boolean

            If Not [object].IsLoaded And Not [object].IsCreated Then
                Call CoreMessageHandler(message:="object is neither created nor loaded", tablename:=[object].Name, messagetype:=otCoreMessageType.InternalError)

                Return False
            End If
            '*** check if version is the same as in code

            Dim aDatatype As System.Type = ot.GetDataObjectType([object].Name)

            If aDatatype IsNot Nothing Then
                Dim aDataobject As iormPersistable = Activator.CreateInstance(aDatatype)
                Dim aVersion = aDataobject.GetVersion(dataobject:=aDataobject)

                If [object].Version <> aVersion Then
                    '_Session.CurrentDBDriver.VerifyOnTrackDatabase(verifyOnly:=False, createOnMissing:=True)
                    CoreMessageHandler(message:="Attention ! Version of object in object store V" & [object].Version & " is different from version in code V" & aVersion, _
                                        messagetype:=otCoreMessageType.InternalWarning, tablename:=[object].Name, subname:="ObjectStore.LoadObjectDefintion")
                End If
            End If
            

            If _objectDirectory.ContainsKey([object].Name) Then
                _objectDirectory.Remove([object].Name)
            End If
            SyncLock _lock
                _objectDirectory.Add(key:=[object].Name, value:=[object])
            End SyncLock

            For Each anEntry As ObjectEntryDefinition In [object].Entries
                ' save the entry
                If _entryDirectory.ContainsKey(key:=[object].Name & "." & anEntry.Name) Then
                    SyncLock Me
                        _entryDirectory.Remove(key:=[object].Name & "." & anEntry.Name)
                    End SyncLock
                End If
                SyncLock Me
                    _entryDirectory.Add(key:=[object].Name & "." & anEntry.Name, value:=anEntry)
                End SyncLock

                '** cross references
                Me.AddID(entry:=anEntry)
                Me.AddAlias(entry:=anEntry)

            Next


            Return True
        End Function

        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntry(entryname As String, Optional objectname As String = "") As ObjectEntryDefinition

            Dim anObject As New ObjectDefinition


            If objectname <> "" Then
                If HasEntry(objectname:=objectname, entryname:=entryname) Then
                    Return _entryDirectory.Item(key:=objectname & "." & entryname)
                    ' try to load
                ElseIf Not HasObject(objectname:=objectname) AndAlso _
                    anObject.LoadBy(objectname:=objectname, domainID:=_DomainID) Then
                    SyncLock Me
                        LoadObjectDefinition(anObject)
                        If HasEntry(objectname:=objectname, entryname:=entryname) Then
                            Return _entryDirectory.Item(key:=objectname & "." & entryname)
                        Else
                            Return Nothing
                        End If
                    End SyncLock
                    ' nothing
                Else
                    Return Nothing
                End If
            Else
                Dim aName As String = _entryDirectory.Keys.ToList.Find(Function(n As String)
                                                                           Return LCase(entryname) = LCase(Split(n, ".").Last)
                                                                       End Function)
                If Not aName Is Nothing AndAlso aName <> "" Then
                    Return _entryDirectory.Item(key:=aName)
                End If

            End If



            Return Nothing
        End Function

        ''' <summary>
        ''' retrieves an Entry by name
        ''' </summary>
        ''' <param name="objectname">name of the object</param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function HasObject(objectname As String) As Boolean

            If _objectDirectory.ContainsKey(key:=objectname) Then
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
        Public Function GetObject(objectname As String) As ObjectDefinition
            Dim anObject As New ObjectDefinition

            SyncLock Me
                If _objectDirectory.ContainsKey(key:=objectname) Then
                    Return _objectDirectory.Item(key:=objectname)
                    ' try to reload
                ElseIf anObject.LoadBy(objectname:=objectname, domainID:=_DomainID) Then
                    LoadObjectDefinition(anObject)
                    If HasObject(objectname:=objectname) Then
                        Return _objectDirectory.Item(key:=objectname)
                    Else
                        Return Nothing
                    End If
                    ' nothing
                Else
                    Return Nothing
                End If
            End SyncLock


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
        Public Function GetEntries(objectname As String) As List(Of ObjectEntryDefinition)
            If _objectDirectory.ContainsKey(key:=objectname) Then
                Return _objectDirectory.Item(key:=objectname).Entries
            Else
                Return New List(Of ObjectEntryDefinition)
            End If

        End Function

        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntryByID([id] As String, Optional objectname As String = "") As List(Of ObjectEntryDefinition)
            If _IDsDirectory.ContainsKey(UCase([id])) Then
                If objectname = "" Then
                    Return _IDsDirectory.Item(key:=UCase([id]))
                Else
                    Dim aList As New List(Of ObjectEntryDefinition)
                    For Each objectdef In _IDsDirectory.Item(key:=UCase(id))
                        If LCase(objectname) = LCase(objectdef.Objectname) Then
                            aList.Add(objectdef)
                        End If
                    Next
                    Return aList
                End If
            Else
                Return GetEntryByAlias(alias:=id, objectname:=objectname)
            End If

        End Function
        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntryByAlias([alias] As String, Optional objectname As String = "") As List(Of ObjectEntryDefinition)
            If _aliasDirectory.ContainsKey(UCase([alias])) Then
                If objectname = "" Then
                    Return _aliasDirectory.Item(key:=UCase([alias]))
                Else
                    Dim aList As New List(Of ObjectEntryDefinition)
                    For Each objectdef In _aliasDirectory.Item(key:=UCase([alias]))
                        If LCase(objectname) = LCase(objectdef.Objectname) Then
                            aList.Add(objectdef)
                        End If
                    Next
                    Return aList
                End If

            Else
                Return New List(Of ObjectEntryDefinition)
            End If

        End Function
        ''' <summary>
        ''' retrieves an Entry by Alias ID
        ''' </summary>
        ''' <param name="Alias"></param>
        ''' <returns>an Entry object or nothing </returns>
        ''' <remarks></remarks>
        Public Function GetEntryByAlias([aliases]() As String, Optional objectname As String = "") As List(Of ObjectEntryDefinition)
            Dim theEntries As New List(Of ObjectEntryDefinition)

            For Each [alias] In aliases
                theEntries.AddRange(Me.GetEntryByAlias([alias], objectname:=objectname))
            Next

            Return theEntries
        End Function
    End Class

    '************************************************************************************
    '***** CLASS clsOTDBSchemaDefTable is the object for a OTDBRecord (which is the datastore)
    '*****
    '*****
    ''' <summary>
    ''' Meta data for an OTDB data object 
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ObjectDefinition
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const ConstTableID = ObjectEntryDefinition.ConstTableID

        Public Const ConstFNTablename = ObjectEntryDefinition.ConstFNTableName
        Public Const ConstFNPrimaryKey = ObjectEntryDefinition.ConstFNPrimaryKey
        Public Const ConstFNUseCache = ObjectEntryDefinition.ConstFNUseCache
        Public Const ConstFNDeletePerFlag = ObjectEntryDefinition.ConstFNDeletePerFlag
        Public Const ConstFNSpareFieldsFLAG = ObjectEntryDefinition.ConstFNSpareFieldsFlag
        'Public Const ConstFNVersion = ObjectEntryDefinition.constFNVersion ?? doesnot exists -> BUG

        ' key
        Private _tablename As String = ""
        ' components itself per key:=posno, value:=cmid
        Private _entries As New Dictionary(Of String, ObjectEntryDefinition)
        Private _entriesordinalPos As New SortedDictionary(Of Long, ObjectEntryDefinition) ' sorted to ordinal position in the record
        Private _indices As New Dictionary(Of String, List(Of String))    ' save the indices as <key, collection of fields>
        Private _pkname As String = ""   ' name of Primary Key
        Private _useCache As Boolean
        Private _CacheSelect As String = ""
        Private _deletePerFlagBehavior As Boolean = False
        Private _domainBehavior As Boolean = False
        Private _SpareFieldsFlagBehavior As Boolean = False
        Private _Version As Long = 0

        Public Event ObjectDefinitionChanged As EventHandler(Of ObjectDefintionEventArgs)

        '** runtime
        Private _runtimeOnly As Boolean

        '** initialize
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub
#Region "Properties"

        ''' <summary>
        ''' gets the tablename of the defintion
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Name() As String
            Get
                Name = _tablename
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
                Count = _entries.Count - 1
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
                _useCache = value
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
        Property CacheSelect As String
            Set(value As String)
                _CacheSelect = value
            End Set
            Get
                Return _CacheSelect
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain behavior.
        ''' </summary>
        ''' <value>The domain behavior.</value>
        Public Property DomainBehavior() As Boolean
            Get
                Return Me._DomainBehavior
            End Get
            Set(value As Boolean)
                Me._DomainBehavior = Value
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
                _Version = value
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
                Me._SpareFieldsFlagBehavior = value
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
                Me._deletePerFlagBehavior = value
            End Set
            Get
                Return _deletePerFlagBehavior
            End Get
        End Property
        ''' <summary>
        ''' returns true if this object is not persisted
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property RuntimeOnly As Boolean
            Get
                Return _runtimeOnly
            End Get
        End Property
        ''' <summary>
        ''' returns a list of entrynames
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Entrynames() As List(Of String)
            Get
                If Not Me.IsCreated And Not _IsLoaded Then
                    Entrynames = Nothing
                    Exit Property
                End If
                Return _entries.Keys.ToList()
            End Get
        End Property

        ''' <summary>
        ''' gets a collection of object Entry definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Entries() As List(Of ObjectEntryDefinition)
            Get
                Dim aCollection As New List(Of ObjectEntryDefinition)

                If Me.IsCreated Or Me.IsLoaded Then
                    For Each anEntry As ObjectEntryDefinition In _entries.Values
                        If anEntry.Name <> "" Then aCollection.Add(anEntry)
                    Next
                End If

                Return aCollection
            End Get
        End Property
#End Region

        '*** add a Component by cls OTDB
        '***
        ''' <summary>
        ''' add a Component by cls OTDB
        ''' </summary>
        ''' <param name="fielddesc"></param>
        ''' <param name="reset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddFieldDesc(ByRef fielddesc As ormFieldDescription, Optional ByVal reset As Boolean = True) As Boolean
            Dim anEntry As New ObjectEntryDefinition
            Dim posno As Long

            SyncLock Me
                ' check Members
                If Me.HasEntry(LCase(fielddesc.ColumnName)) Then
                    Call CoreMessageHandler(message:=" Entry containsKey in Schema", subname:="ObjectDefinition.AddFieldDesc", _
                                            messagetype:=otCoreMessageType.InternalError, _
                                            arg1:=fielddesc.ColumnName, tablename:=ConstTableID)
                    Return False
                End If

                ' create new Member
                anEntry = New ObjectEntryDefinition
                fielddesc.ColumnName = LCase(Regex.Replace(fielddesc.ColumnName, "\s", "")) ' no white chars allowed
                If fielddesc.ordinalPosition = 0 Then
                    fielddesc.ordinalPosition = Me.GetMaxPosNo + 1
                End If

                If Not anEntry.Create(Me.Name, entryname:=fielddesc.ColumnName, runtimeOnly:=Me.RuntimeOnly) Then
                    Call anEntry.LoadBy(Me.Name, entryname:=fielddesc.ColumnName)
                End If
                Call anEntry.SetByFieldDesc(fielddesc)
                ' add the component
                AddFieldDesc = Me.AddEntry(anEntry)

                If reset Then
                    With fielddesc
                        .Aliases = New String() {}
                        .ID = ""
                        '.Name = ""
                        '.dataType = 0
                        .Size = 0
                        .Parameter = ""
                        .Relation = New String() {}
                        .Title = ""
                        .SpareFieldTag = False

                        .ordinalPosition = 0
                        '.tablename = ""
                    End With
                End If
            End SyncLock


        End Function
        ''' <summary>
        ''' add a Compound description to field
        ''' </summary>
        ''' <param name="COMPOUNDDESC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddCompoundDesc(compounddesc As ormCompoundDesc) As Boolean
            Dim anEntry As New ObjectEntryDefinition


            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddCompoundDesc = False
                Exit Function
            End If
            SyncLock Me
                ' check Members
                If Me.HasEntry(LCase(compounddesc.ID)) Then
                    Call CoreMessageHandler(message:=" compound already in object definition", subname:="ObjectDefinition.AddCompoundDesc", _
                                            messagetype:=otCoreMessageType.InternalError, _
                                            arg1:=compounddesc.ID, tablename:=ConstTableID)
                    Return False
                End If

                ' create new Member
                anEntry = New ObjectEntryDefinition
                If compounddesc.ordinalPosition = 0 Then
                    compounddesc.ordinalPosition = Me.GetMaxPosNo + 1
                End If
                If Not anEntry.Create(Me.Name, entryname:=LCase(compounddesc.ID)) Then
                    Call anEntry.LoadBy(Me.Name, entryname:=LCase(compounddesc.ID))
                End If
                Call anEntry.SetByCompoundDesc(compounddesc)


                ' add the component
                AddCompoundDesc = Me.AddEntry(anEntry)

                '* TODO: Automatically create the Index CompoundNameIndex
            End SyncLock


        End Function

        '**** alterSchema changes the Database according the information here
        '****
        ''' <summary>
        ''' alterSchema changes the Database according the information here
        ''' </summary>
        ''' <param name="addToSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AlterSchema(Optional ByVal addToSchema As Boolean = False) As Boolean
            Dim anEntry As New ObjectEntryDefinition
            Dim aTimestamp As Date
            Dim aName As Object
            Dim flag As Boolean
            Dim tblInfo As Object
            Dim aCollection As New List(Of String)
            Dim aFieldDesc As New ormFieldDescription
            Dim aOTDBrecord As New ormRecord
            Dim process As Boolean


            If Not IsLoaded And Not IsCreated Then
                AlterSchema = False
                Exit Function
            End If

            ' set Timestamp
            aTimestamp = Now
            ' get the table
            ' free lunch for OTDBSchemaDefTable and UserDefinition -> Bootstrap
            If LCase(User.ConstTableID) <> LCase(Me.Name) And LCase(Me.Name) <> LCase(Me.TableID) Then
                If Not CurrentSession.RequireAccessRight(accessRequest:=otAccessRight.AlterSchema) Then
                    Return False
                End If
            End If

            Try
                '** call to get object
                tblInfo = CurrentDBDriver.GetTable(Me.Name, createOrAlter:=True, addToSchemaDir:=addToSchema)

                Dim entrycoll As List(Of ObjectEntryDefinition)
                '** check which entries to use
                If _entries.Count = _entriesordinalPos.Count Then
                    entrycoll = _entriesordinalPos.Values.ToList
                Else
                    entrycoll = _entries.Values.ToList
                End If
                ' create fields each entry
                For Each anEntry In entrycoll
                    '*** delete flags

                    If anEntry.Name <> "" And anEntry.Typeid = otSchemaDefTableEntryType.Field Then
                        process = True ' default
                        '*** check some behavior and rules
                        If (anEntry.Name = ormDataObject.ConstFNDeletedOn Or anEntry.Name = ConstFNIsDeleted) _
                        And Not Me.DeletePerFlagBehavior Then
                            process = False
                        ElseIf anEntry.SpareFieldTag And Not Me.SpareFieldsBehavior Then
                            process = False
                        End If
                        '** move data to field description structure
                        aFieldDesc.Datatype = anEntry.Datatype
                        aFieldDesc.ID = anEntry.ID
                        aFieldDesc.ColumnName = anEntry.Name
                        aFieldDesc.Parameter = anEntry.Parameter
                        aFieldDesc.Title = anEntry.Title
                        aFieldDesc.Tablename = anEntry.Objectname
                        aFieldDesc.Aliases = anEntry.Aliases
                        aFieldDesc.Relation = anEntry.Relation
                        aFieldDesc.Size = anEntry.Size
                        aFieldDesc.IsNullable = anEntry.IsNullable
                        aFieldDesc.Version = anEntry.Version
                        'anEntry.getByFieldDesc (aFieldDesc)
                        '** add column
                        If process Then
                            Call CurrentDBDriver.GetColumn(tblInfo, aFieldDesc, createOrAlter:=True, addToSchemaDir:=addToSchema)
                        End If

                    End If
                Next

                '** call again to create
                tblInfo = CurrentDBDriver.GetTable(Me.Name, createOrAlter:=True, _
                                                   addToSchemaDir:=addToSchema, tableNativeObject:=tblInfo)

                ' create index
                For Each aName In _indices.Keys
                    aCollection = _indices.Item(key:=aName)
                    If Not aCollection Is Nothing And aCollection.Count > 0 Then
                        If LCase(aName) = LCase(_pkname) Then
                            flag = True
                        Else
                            flag = False
                        End If
                        Call CurrentDBDriver.GetIndex(tblInfo, indexname:=CStr(aName), columnNames:=aCollection, _
                                                      primaryKey:=flag, addToSchemaDir:=addToSchema)
                    End If
                Next aName

                ' reset the Table description
                ' only if we are connected -> bootstrapping problem
                If CurrentSession.IsRunning Then
                    If ot.CurrentConnection.DatabaseDriver.GetTableSchema(tableID:=Me.Name, force:=True) Is Nothing Then
                        Call CoreMessageHandler(subname:="clsOTDBSchemaDefTable.alterSchema", tablename:=tblInfo.Name, _
                                                message:="Error while setTable in alterSchema")
                    End If
                    'RaiseEvent ObjectDefinitionChanged(Me, New ObjectDefintionEventArgs(objectname:=Me.name)) in persist
                End If


                Return True
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBSchemaDefTable.alterSchema", exception:=ex)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' add Index description to table
        ''' </summary>
        ''' <param name="anIndexName"></param>
        ''' <param name="aFieldCollection"></param>
        ''' <param name="PrimaryKey"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddIndex(ByVal indexname As String, ByRef fieldnames As Collection, Optional ByVal isprimarykey As Boolean = False) As Boolean

            Dim existEntry As New List(Of String)
            Dim fieldList As New List(Of String)
            Dim anEntry As New ObjectEntryDefinition
            Dim i As Long = 1


            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                AddIndex = False
                Exit Function
            End If

            ' get the existing collection
            If _indices.ContainsKey(LCase(indexname)) Then
                existEntry = _indices.Item(LCase(indexname))
            End If

            ' check fields -> should be defined to be an index
            For Each aName In fieldnames
                ' check
                If Not _entries.ContainsKey(LCase(aName)) Then
                    AddIndex = False
                    Call CoreMessageHandler(arg1:=aName, subname:="clsOTDBSchemaDefTable.addIndex", _
                                            tablename:=Me.TableID, message:=" field doesnot exist in table for building index")
                    Exit Function
                Else
                    If isprimarykey Then
                        anEntry = _entries.Item(LCase(aName))
                        anEntry.Indexname = indexname
                        anEntry.IndexPosition = i
                        i += 1
                    End If

                    fieldList.Add(LCase(aName))
                End If
            Next aName

            ' remove
            If _indices.ContainsKey(LCase(indexname)) Then
                _indices.Remove(LCase((indexname)))
            End If

            ' add index
            Call _indices.Add(key:=LCase(indexname), value:=fieldList)
            If isprimarykey Then
                _pkname = LCase(indexname)
            End If

            ' return
            AddIndex = True

        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetNoPrimaryKeys() As UShort
            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated And _pkname = "" Then
                Return 0
            End If

            Return GetNoIndexFields(_pkname)
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetPrimaryKeyFieldNames() As List(Of String)
            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated And _pkname = "" Then
                Return New List(Of String)
            End If

            Return GetIndexFieldNames(_pkname)
        End Function
        ''' <summary>
        ''' retrieve the List of Primary Key Fieldnames
        ''' </summary>
        ''' <returns>List(of String)</returns>
        ''' <remarks></remarks>
        Public Function GetPrimaryKeyEntries() As List(Of ObjectEntryDefinition)
            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated And _pkname = "" Then
                Return New List(Of ObjectEntryDefinition)
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
            If Not _IsLoaded And Not Me.IsCreated Then
                Return New List(Of String)
            End If

            ' get the existing collection
            If _indices.ContainsKey(LCase(indexname)) Then
                Return _indices.Item(LCase(indexname))
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
            If Not _IsLoaded And Not Me.IsCreated Then
                Return 0
            End If

            ' get the existing collection
            If _indices.ContainsKey(LCase(indexname)) Then
                Return _indices.Item(LCase(indexname)).Count
            End If

            Return 0
        End Function
        ''' <summary>
        ''' retrieves a list of Fieldnames of an Index
        ''' </summary>
        ''' <param name="IndexName">name of the Index</param>
        ''' <returns>List (of String)</returns>
        ''' <remarks></remarks>
        Public Function GetIndexEntries(ByVal indexname As String) As List(Of ObjectEntryDefinition)
            Dim aFieldCollection As New List(Of ObjectEntryDefinition)

            ' Nothing
            If Not _IsLoaded And Not Me.IsCreated Then
                Return aFieldCollection
            End If

            For Each anEntryname In Me.GetIndexFieldNames(indexname)
                aFieldCollection.Add(Me.GetEntry(anEntryname))
            Next

            Return aFieldCollection
        End Function
        ''' <summary>
        ''' Add a Component by Table Entry
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddEntry(entry As ObjectEntryDefinition) As Boolean

            ' remove and overwrite
            If _entries.ContainsKey(key:=LCase(entry.Name)) Then
                Call _entries.Remove(key:=LCase(entry.Name))
            End If
            ' add entry
            _entries.Add(key:=LCase(entry.Name), value:=entry)

            If _entriesordinalPos.ContainsKey(entry.Position) Then
                Call _entriesordinalPos.Remove(key:=entry.Position)
            End If
            Call _entriesordinalPos.Add(key:=entry.Position, value:=entry)
            '
            Return True

        End Function

        ''' <summary>
        ''' Delete the record and all members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Delete() As Boolean
            Dim anEntry As New ObjectEntryDefinition
            Dim initialEntry As New ObjectEntryDefinition

            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Delete = False
                    Exit Function
                End If
            End If
            If Me.IsCreated Then
                Me.LoadBy(Me.Name)
            End If
            If Not _IsLoaded And Not Me.IsCreated Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each entry As ObjectEntryDefinition In _entries.Values
                entry.Delete()
            Next

            ' reset it
            _entries.Clear()
            _entriesordinalPos.Clear()
            anEntry = New ObjectEntryDefinition
            If Not anEntry.Create(objectname:=Me.Name, entryname:="", typeid:=otSchemaDefTableEntryType.Table) Then
                Call anEntry.LoadBy(objectname:=Me.Name, entryname:="")
                anEntry.Name = ""
            End If

            _entries.Add(key:="", value:=anEntry)

            _IsCreated = True
            IsDeleted = True
            _IsLoaded = False

        End Function

        ''' <summary>
        ''' gets an entry by entryname or nothing
        ''' </summary>
        ''' <param name="entryname">name of the entry</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasEntry(entryname As String) As Boolean

            If Not Me.IsCreated And Not _IsLoaded Then
                Return False
            End If

            If _entries.ContainsKey(key:=entryname) Then
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' gets an entry by entryname or nothing
        ''' </summary>
        ''' <param name="entryname">name of the entry</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetEntry(entryname As String) As ObjectEntryDefinition

            If Not Me.IsCreated And Not _IsLoaded Then
                Return Nothing
            End If

            If _entries.ContainsKey(key:=entryname) Then
                Return _entries.Item(key:=entryname)
            Else
                Return Nothing
            End If

        End Function
        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize()
            If Not Me.TableStore Is Nothing Then
                Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            End If
            Return _IsInitialized
        End Function
        ''' <summary>
        ''' Infuse
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Infuse(ByRef aRecord As ormRecord) As Boolean
            ' not implemented
            Infuse = False
        End Function
        ''' <summary>
        ''' loadBy a SchemaDefTable with primary key
        ''' </summary>
        ''' <param name="tablename">the tablename</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Shared Function AllObjectnames(Optional ByRef tablestore As iormDataStore = Nothing) As List(Of String)
            Dim aRecordCollection As New List(Of ormRecord)
            Dim theTablenames As New List(Of String)

            ' run the sql statement
            If tablestore Is Nothing Then
                tablestore = ot.GetTableStore(ObjectEntryDefinition.ConstTableID)
            End If

            Try
                Dim aCommand As ormSqlSelectCommand = tablestore.CreateSqlSelectCommand(id:="AllTablenames")
                If Not aCommand.Prepared Then
                    aCommand.select = "DISTINCT " & ObjectEntryDefinition.ConstFNTableName
                    aCommand.Where = ObjectEntryDefinition.ConstFNType & " = 'TABLE'"

                    aCommand.Prepare()
                End If

                aRecordCollection = aCommand.RunSelect

                ' records read
                For Each aRecord As ormRecord In aRecordCollection
                    theTablenames.Add(aRecord.GetValue(1))
                Next

                '
                Return theTablenames

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBSchemaDefTable.AllTablenames")

                Return theTablenames
            End Try

        End Function

        ''' <summary>
        ''' loadBy a SchemaDefTable with primary key
        ''' </summary>
        ''' <param name="tablename">the table name</param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal objectname As String, _
                               Optional domainID As String = "", _
                               Optional ByRef tablestore As iormDataStore = Nothing) As Boolean
            Dim aRecordCollection As New List(Of ormRecord)
            Dim aIndexCollection As New Collection

            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            ' run the sql statement
            If tablestore Is Nothing Then
                tablestore = Me.TableStore
            End If

            If domainID = "" Then domainID = CurrentSession.CurrentDomainID

            Try
                Dim aCommand As ormSqlSelectCommand = tablestore.CreateSqlSelectCommand(id:="AllByTablename")
                If Not aCommand.Prepared Then
                    aCommand.AddTable(ConstTableID)
                    If aCommand.DatabaseDriver.DatabaseType = otDBServerType.SQLServer Then
                        aCommand.Where = "LOWER(" & ConstTableID & ".[" & ConstFNTablename & "]) = @tablename"
                    Else
                        aCommand.Where = "LCASE(" & ConstTableID & ".[" & ConstFNTablename & "]) = @tablename"
                    End If
                    aCommand.Where &= " AND ([" & ConstFNDomainID & "] = @domainID OR [" & ConstFNDomainID & "] = @globalID)"
                    aCommand.Where &= " AND " & ConstFNIsDeleted & " = @deleted "
                    aCommand.OrderBy = "[" & ObjectEntryDefinition.ConstFNFieldname & "] , [" & ObjectEntryDefinition.ConstFNDomainID & "]"
                    '* use datatype since object definition is not described itself
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@tablename", fieldname:=ConstFNTablename, tablename:=ConstTableID, datatype:=otFieldDataType.Text))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", notColumn:=True, datatype:=otFieldDataType.Text))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", notColumn:=True, datatype:=otFieldDataType.Text))
                    aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", fieldname:=ConstFNIsDeleted, tablename:=ConstTableID, datatype:=otFieldDataType.Bool))
                    aCommand.Prepare()
                End If
                
                aCommand.SetParameterValue("@tablename", LCase(objectname))
                aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)
                aCommand.SetParameterValue(ID:="@deleted", value:=False)
                aRecordCollection = aCommand.RunSelect

                If aRecordCollection.Count = 0 Then
                    _IsLoaded = False
                    LoadBy = False
                    Exit Function
                Else
                    _tablename = objectname
                    _IsLoaded = True
                    Dim aFieldDir As New Dictionary(Of String, ObjectEntryDefinition)

                    ' records read
                    For Each aRecord As ormRecord In aRecordCollection

                        ' add the Entry as Component
                        Dim anEntry As ObjectEntryDefinition = New ObjectEntryDefinition
                        If anEntry.Infuse(aRecord) Then
                            If aFieldDir.ContainsKey(key:=anEntry.Entryname) Then
                                Dim anexistingItem As ObjectEntryDefinition = aFieldDir.Item(key:=anEntry.Entryname)
                                If anEntry.DomainID = domainID And anexistingItem.DomainID = ConstGlobalDomain Then
                                    aFieldDir.Remove(key:=anEntry.Entryname)
                                    aFieldDir.Add(key:=anEntry.Entryname, value:=anEntry)
                                End If
                            ElseIf anEntry.DomainID = domainID Or anEntry.DomainID = ConstGlobalDomain Then
                                aFieldDir.Add(key:=anEntry.Entryname, value:=anEntry)
                            End If

                        End If
                    Next aRecord

                    '** the remainin mix is per Domain
                    For Each anEntry In aFieldDir.Values
                        If anEntry.IsTable Then
                            _CacheSelect = anEntry.CacheSelect
                            _useCache = anEntry.UseCache
                            _Version = anEntry.Version
                            _deletePerFlagBehavior = anEntry.DeleteFlagBehavior
                            _SpareFieldsFlagBehavior = anEntry.SpareFieldsBehavior
                            _domainBehavior = anEntry.DomainBehavior
                            _domainID = anEntry.DomainID
                        End If
                        If Not Me.AddEntry(anEntry) Then
                        End If
                        ' set primary key
                        If anEntry.IsPrimaryKey And anEntry.Indexname <> "" Then
                            _pkname = anEntry.Indexname
                            aIndexCollection.Add(anEntry.Name)
                        End If
                    Next
                    ' add primary key
                    If aIndexCollection.Count > 0 Then
                        Call Me.AddIndex(_pkname, aIndexCollection, isprimarykey:=True)
                    End If

                    '
                    _IsLoaded = True
                    LoadBy = True
                    Exit Function
                End If
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="clsOTDBSchemaDefTable.LoadBy")
                _IsLoaded = False
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Persist the Object to the data store
        ''' </summary>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Dim anEntry As New ObjectEntryDefinition
            Dim headentry As New ObjectEntryDefinition
            Dim anIndexColl As List(Of String)
            Dim i As Integer
            Dim flag As Boolean
            Dim changed As Boolean
            Dim m As Object
            Dim n As Object

            '*
            If _runtimeOnly Then
                Call CoreMessageHandler(message:="Object is runtime Only and will not be persisted", subname:="clsOTDBSchemaDefTable.Persist", _
                                        messagetype:=otCoreMessageType.InternalWarning, break:=False, arg1:=Me.Name)

                Return False
            End If
            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not IsLoaded And Not IsCreated Then
                Persist = False
                Exit Function
            End If

            If timestamp = ConstNullDate Then
                timestamp = Date.Now
            End If
            ' persist each entry
            For Each anEntry In _entries.Values

                If anEntry.Name <> "" Then
                    If anEntry.IsChanged Then
                        changed = True
                    End If
                    ' inprimary ?
                    i = 0
                    flag = False
                    If _indices.ContainsKey(_pkname) Then
                        anIndexColl = _indices.Item(_pkname)
                        For i = 0 To anIndexColl.Count - 1
                            If LCase(anEntry.Name) = LCase(anIndexColl.ElementAt(i)) Then
                                flag = True
                                Exit For
                            End If
                        Next i
                        If flag Then
                            anEntry.IsPrimaryKey = True
                            anEntry.IndexPosition = i
                        Else
                            anEntry.IsPrimaryKey = False
                            anEntry.IndexPosition = 0
                        End If
                    Else
                        anEntry.IsPrimaryKey = False
                        anEntry.IndexPosition = 0
                    End If


                    ' persist member first
                    anEntry.Persist(timestamp)
                Else
                    headentry = anEntry
                End If
            Next
            ' persist head
            If changed Then
                headentry.Version = _Version
                headentry.UseCache = _useCache
                headentry.CacheSelect = _CacheSelect
                headentry.SpareFieldsBehavior = _SpareFieldsFlagBehavior
                headentry.DeleteFlagBehavior = _deletePerFlagBehavior
                headentry.domainBehavior = _domainBehavior
            End If

            headentry.Typeid = otSchemaDefTableEntryType.Table
            Persist = headentry.Persist(timestamp)

            '** raise my change
            RaiseEvent ObjectDefinitionChanged(Me, New ObjectDefintionEventArgs(objectname:=Me.Name)) ' in persist
            '** announce to session
            Me.TableStore.Connection.Session.RaiseObjectChangedDefinitionEvent(Me, New ObjectDefintionEventArgs(objectname:=Me.TableID))

            Exit Function

errorhandle:

            Persist = False

        End Function

        '**** create : create a new Object with primary keys
        '****
        ''' <summary>
        ''' create a new data object of that type
        ''' </summary>
        ''' <param name="tablename">tablename of the table</param>
        ''' <param name="runTimeOnly">if no save is possible -> bootstrapping</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal tablename As String, _
        Optional runTimeOnly As Boolean = False, _
        Optional checkunique As Boolean = False, _
        Optional version As UShort = 1) As Boolean
            Dim anEntry As New ObjectEntryDefinition

            '* init
            If Not IsInitialized And Not runTimeOnly Then
                If Not Me.Initialize() Then
                    Create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                Create = False
                Exit Function
            End If

            ' set the primaryKey
            _tablename = LCase(tablename)
            _entries = New Dictionary(Of String, ObjectEntryDefinition)

            ' abort create if containsKey
            If Not anEntry.Create(objectname:=LCase(tablename), _
                                  typeid:=otSchemaDefTableEntryType.Table, _
                                  checkunique:=checkunique, _
                                  runtimeOnly:=runTimeOnly) Then
                Create = False
                Exit Function
            End If

            anEntry.Version = version ' set the version for the head entry
            _entries.Add(key:="", value:=anEntry) ' headentry

            _runtimeOnly = runTimeOnly
            _IsCreated = True
            Create = IsCreated

        End Function

        ''' <summary>
        ''' retrieves the max posno / entry index
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMaxPosNo() As UShort
            If _entriesordinalPos.Count = 0 Then
                Return 0
            Else
                Return _entriesordinalPos.Keys.Max
            End If

        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBSchemaDefTableEntry describes additional database schema information
    '*****

    Public Class ObjectEntryDefinition
        Inherits ormDataObject
        Implements iormPersistable


        '*** CONST Schema
        '** Table
        <ormSchemaTableAttribute(Version:=5, adddeletefieldbehavior:=True, adddomainID:=True)> Public Const ConstTableID = "tblSchemaDirectory"
        '** Index
        <ormSchemaIndexAttribute(ColumnName1:=ConstFNxid, columnname2:=ConstFNTableName)> Public Const ConstIndexXID = "ID"
        <ormSchemaIndexAttribute(columnName1:=ConstFNDomainID, ColumnName2:=ConstFNxid, columnname3:=ConstFNTableName)> Public Const ConstIndDomain = "Domain"

        '*** Columns
        <ormSchemaColumnAttribute(defaultvalue:="", typeid:=otFieldDataType.Text, size:=100, _
                                  title:="Table Name", Description:="tablename in the datastore", primaryKeyordinal:=1)> _
        Public Const ConstFNTableName As String = "tblname"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=100, defaultvalue:="", _
                                  title:="Column Name", Description:="column name in the datastore", primaryKeyordinal:=2)> _
        Public Const ConstFNFieldname As String = "fieldname"

        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=100, primarykeyordinal:=3, _
               title:="Domain", description:="domain of the entry")> Public Const ConstFNDomainID = Domain.ConstFNDomainID

        <ormSchemaColumnAttribute(defaultvalue:="0", typeid:=otFieldDataType.[Long], title:="Pos", Description:="position number in record")> _
        Public Const ConstFNPosition As String = "pos"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, defaultvalue:="", _
                                  title:="XChangeID", Description:="ID for XChange manager")> _
        Public Const ConstFNxid As String = "id"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, defaultvalue:="", _
                                  title:="Title", Description:="title for column headers of the field")> _
        Public Const ConstFNTitle As String = "title"

        <ormSchemaColumnAttribute(defaultvalue:="", typeid:=otFieldDataType.Memo, isnullable:=True, _
                                  title:="Description", Description:="Description of the field")> _
        Public Const ConstFNDescription As String = "desc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, defaultvalue:="", isarray:=True, _
                                  title:="XChange alias ID", Description:="aliases ID for XChange manager")> _
        Public Const ConstFNalias As String = "alias"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, defaultvalue:="FIELD", _
                                  title:="Type", Description:="OTDB schema entry type")> _
        Public Const ConstFNType As String = "typeid"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, defaultvalue:="", isarray:=True, _
                                  title:="Parameter", Description:="parameters")> _
        Public Const ConstFNParameter As String = "parameter"

        <ormSchemaColumnAttribute(defaultvalue:="1", typeid:=otFieldDataType.[Long], _
                                  title:="Datatype", Description:="OTDB field data type")> _
        Public Const ConstFNDatatype As String = "datatype"

        <ormSchemaColumnAttribute(defaultvalue:="", typeid:=otFieldDataType.Text, isnullable:=True, isarray:=True, _
                                  title:="DefaultValue", Description:="default value of the field")> _
        Public Const ConstFNDefaultValue As String = "default"

        <ormSchemaColumnAttribute(defaultvalue:="0", typeid:=otFieldDataType.[Long], _
                                  title:="UpdateCount", Description:="version counter of updating")> _
        Public Const ConstFNUPDC As String = "updc"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.[Long], defaultvalue:="0", _
                                  title:="Size", Description:="max Length of the Column")> _
        Public Const ConstFNSize As String = "size"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, isarray:=True, _
        title:="Relation", Description:="relation information")> _
        Public Const ConstFNRelation As String = "relation"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Text, size:=50, defaultvalue:="", _
                                  title:="Index", Description:="index name")> _
        Public Const ConstFNIndex As String = "index"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.[Long], defaultvalue:="0", _
                                  title:="IndexPosition", Description:="position number in index")> _
        Public Const ConstFNIndexPosition As String = "posin"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="Is Key", Description:="set if the entry is a key")> _
        Public Const ConstFNKey As String = "key"

        <ormSchemaColumnAttribute(defaultvalue:="0", typeid:=otFieldDataType.Bool, _
                                  title:="Is primary Key", Description:="set if the entry is a primary key")> _
        Public Const ConstFNPrimaryKey As String = "pkey"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="Is Nullable", Description:="set if the entry is a nullable")> _
        Public Const ConstFNIsNullable As String = "isnull"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", _
                                  title:="Is Array", Description:="set if the entry value is an array of values")> _
        Public Const ConstFNIsArray As String = "isarray"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Bool, _
        title:="use cache", defaultvalue:="", Description:="set if the entry is cached")> _
        Public Const ConstFNUseCache As String = "cache"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Memo, _
        title:="Cache", defaultvalue:="", Description:="selection what to cache")> _
        Public Const ConstFNCacheSelection As String = "cacheselect"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, defaultvalue:="", size:=50, _
        title:="Compound Table", Description:="name of the compound table")> _
        Public Const ConstFNCompoundTable As String = "ctblname"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, defaultvalue:="", isarray:=True, _
        title:="Compound Relation", Description:="relation column names of the compound table")> _
        Public Const ConstFNCompoundRelation As String = "crelation"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, defaultvalue:="", size:=50, _
        title:="compound id field", Description:="name of the compound id field")> _
        Public Const ConstFNCompoundIDField As String = "cidfield"

        <ormSchemaColumnAttribute( _
        typeid:=otFieldDataType.Text, defaultvalue:="", size:=255, _
        title:="compound value field", Description:="name of the compound value field")> _
        Public Const ConstFNCompoundValueField As String = "cvalfield"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="TableDeleteFlagBehaviour", Description:="set if the table runs the delete per flag behavior")> _
        Public Const ConstFNDeletePerFlag As String = "RecordDeletePerFlag"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="TableSpareFieldsBehaviour", Description:="set if the table has additional spare fields behavior")> _
        Public Const ConstFNSpareFieldsFlag As String = "RecordSpareFieldsFlag"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="SpareFieldTag", Description:="set if the field is a spare field")> _
        Public Const ConstFNSpareFieldTag As String = "SpareFieldTag"

        <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, defaultvalue:="0", title:="DomainBehaviour", Description:="set if the table entries are belong to a domain")> _
        Public Const ConstFNDomainFlag As String = "DomainBehaviorFlag"

        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNxid)> Private _xid As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNTableName)> Private _objectname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNFieldname)> Private _entryname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNRelation)> Private _relation As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNParameter)> Private _parameter As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNalias)> Private _aliases As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNTitle)> Private _title As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDatatype)> Private _datatype As otFieldDataType
        <ormColumnMappingAttribute(fieldname:=ConstFNUPDC)> Private _version As Long = 0
        <ormColumnMappingAttribute(fieldname:=ConstFNSize)> Private _size As Long = 0
        <ormColumnMappingAttribute(fieldname:=ConstFNIsNullable)> Private _isNullable As Boolean = False
        <ormColumnMappingAttribute(fieldname:=ConstFNIsArray)> Private _isArray As Boolean = False
        <ormColumnMappingAttribute(fieldname:=ConstFNDefaultValue)> Private _DefaultValue As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDescription)> Private _Description As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNPosition)> Private _Position As Long = 0
        <ormColumnMapping(fieldname:=ConstFNDeletePerFlag)> Private _deletePerFlagBehavior As Boolean = False
        <ormColumnMapping(fieldname:=ConstFNSpareFieldsFlag)> Private _SpareFieldsFlagBehavior As Boolean = False
        <ormColumnMapping(fieldname:=ConstFNDomainFlag)> Private _domainFlagBehavior As Boolean = False
        <ormColumnMapping(fieldname:=ConstFNSpareFieldTag)> Private _SpareFieldTag As Boolean = False
        '<otColumnMapping(fieldname:=ConstFNType)> exclude from mapping since Type conversion must be handled
        Private _typeid As otSchemaDefTableEntryType

        <ormColumnMappingAttribute(fieldname:=ConstFNIndex)> Private _indexname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNIndexPosition)> Private _posin As Long
        <ormColumnMappingAttribute(fieldname:=ConstFNPrimaryKey)> Private _isPrimaryKey As Boolean = False
        <ormColumnMappingAttribute(fieldname:=ConstFNKey)> Private _isKey As Boolean = False

        <ormColumnMappingAttribute(fieldname:=ConstFNUseCache)> Private _useCache As Boolean = False
        <ormColumnMappingAttribute(fieldname:=ConstFNCacheSelection)> Private _CacheSelect As String = ""

        '** compound settings
        <ormColumnMappingAttribute(fieldname:=ConstFNCompoundTable)> Private _cTablename As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNCompoundRelation)> Private _cRelation As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNCompoundIDField)> Private _cIDFieldname As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNCompoundValueField)> Private _cValueFieldname As String = ""

        ' further internals
        Private _runTimeOnly As Boolean = False

        ''' <summary>
        ''' constructor of a SchemaDefTableEntry
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)

        End Sub

#Region "Properties"

        ''' <summary>
        ''' Gets or sets the position.
        ''' </summary>
        ''' <value>The position.</value>
        Public Property Position() As Long
            Get
                Return Me._Position
            End Get
            Set(value As Long)
                Me._Position = value
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
                Me._Description = value
            End Set
        End Property
        ''' <summary>
        ''' gets the default value as object representation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DefaultValue() As Object
            Get
                Select Case Me.Datatype
                    Case otFieldDataType.Bool
                        If _DefaultValue Is Nothing Then
                            Return False
                        ElseIf IsNumeric(_DefaultValue) Then
                            If CLng(_DefaultValue) = 0 Then
                                Return False
                            Else
                                Return True
                            End If
                        ElseIf String.IsNullOrWhiteSpace(_DefaultValue) Then
                            Return False
                        ElseIf LCase(CStr(_DefaultValue)) = "true" OrElse LCase(CStr(_DefaultValue)) = "yes" Then
                            Return True
                        ElseIf LCase(CStr(_DefaultValue)) = "false" OrElse LCase(CStr(_DefaultValue)) = "no" Then
                            Return False
                        Else
                            Return CBool(_DefaultValue)
                        End If

                    Case otFieldDataType.Long
                        If _DefaultValue Is Nothing Then
                            Return CLng(0)
                        ElseIf IsNumeric(_DefaultValue) Then
                            Return CLng(_DefaultValue)
                        Else
                            Return CLng(0)
                        End If

                    Case otFieldDataType.Numeric
                        If _DefaultValue Is Nothing Then
                            Return CDbl(0)
                        ElseIf IsNumeric(_DefaultValue) Then
                            Return CDbl(_DefaultValue)
                        Else
                            Return CDbl(0)
                        End If

                    Case otFieldDataType.List, otFieldDataType.Memo, otFieldDataType.Text
                        If _DefaultValue Is Nothing Then
                            Return ""
                        Else
                            Return _DefaultValue
                        End If

                    Case otFieldDataType.Date, otFieldDataType.Timestamp
                        If _DefaultValue Is Nothing OrElse Not IsDate(_DefaultValue) Then
                            Return ConstNullDate
                        Else
                            Return CDate(_DefaultValue)
                        End If

                    Case otFieldDataType.Time
                        If _DefaultValue Is Nothing OrElse Not IsDate(_DefaultValue) Then
                            Return ConstNullTime
                        Else
                            Return CDate(_DefaultValue)
                        End If

                    Case Else
                        Return Nothing
                End Select

            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the default value in string presentation
        ''' </summary>
        ''' <value>The default value.</value>
        Public Property DefaultValueString() As String
            Get
                Return Me._DefaultValue
            End Get
            Set(value As String)
                Me._DefaultValue = value
            End Set
        End Property

        ''' <summary>
        ''' sets or gets the tablename of the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Objectname() As String
            Get
                Objectname = _objectname
            End Get
            Set(value As String)
                If LCase(_objectname) <> LCase(value) Then
                    _objectname = value
                    IsChanged = True
                End If
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
                _useCache = value
            End Set
            Get
                Return _useCache
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the cache selection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property CacheSelect As String
            Set(value As String)
                _CacheSelect = value
            End Set
            Get
                Return _CacheSelect
            End Get
        End Property
        ''' <summary>
        ''' Object cannot be persisted only.
        ''' </summary>
        ''' <value>The run tim only.</value>
        Public ReadOnly Property RunTimeOnly() As Boolean
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
        Public Property ID() As String
            Get
                ID = _xid
            End Get
            Set(avalue As String)
                If LCase(_xid) <> LCase(avalue) Then
                    _xid = avalue
                    IsChanged = True
                End If
            End Set

        End Property
        Public Property Entryname As String
            Get
                Return Name
            End Get
            Set(value As String)
                Me.Name = value
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the name of the columns
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Name = _entryname
            End Get
            Set(value As String)
                If LCase(_entryname) <> LCase(value) Then
                    _entryname = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the type OTDBSchemaDefTableEntryType of the field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Typeid() As otSchemaDefTableEntryType
            Get
                Typeid = Me._typeid

            End Get
            Set(value As otSchemaDefTableEntryType)
                If _typeid <> value Then
                    _typeid = value
                    IsChanged = True
                End If
            End Set
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
                Me._isNullable = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is array flag. Field will be transformed to and from an array
        ''' </summary>
        ''' <value>The is array.</value>
        Public Property IsArray() As Boolean
            Get
                Return Me._isArray
            End Get
            Set(value As Boolean)
                Me._isArray = value
            End Set
        End Property
        ''' <summary>
        ''' sets or gets true if this field is a spare field
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SpareFieldTag()
            Get
                Return Me._SpareFieldTag
            End Get
            Set(value)
                Me._SpareFieldTag = value
            End Set
        End Property

        ''' <summary>
        ''' IsField ?
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsField() As Boolean
            Get
                If _typeid = otSchemaDefTableEntryType.Field Then IsField = True
            End Get
        End Property
        ''' <summary>
        ''' returns true if entry is a compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsCompound() As Boolean
            Get
                If _typeid = otSchemaDefTableEntryType.Compound Then IsCompound = True
            End Get
        End Property
        ''' <summary>
        ''' returns true if entry is a Table
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsTable() As Boolean
            Get
                If _typeid = otSchemaDefTableEntryType.Table Then IsTable = True
            End Get
        End Property
        ''' <summary>
        ''' returns the field data type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype() As otFieldDataType
            Get
                Datatype = _datatype
            End Get
            Set(avalue As otFieldDataType)
                If _datatype <> avalue Then
                    _datatype = avalue
                    IsChanged = True
                End If
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
                Version = _version
            End Get
            Set(value As Long)
                _version = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the size
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Size() As Long
            Get
                Size = _size
            End Get
            Set(value As Long)
                If _size <> value Then
                    _size = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns a array of aliases
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Aliases() As String()
            Get
                Aliases = SplitMultbyChar(text:=UCase(_aliases), DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(Aliases) Then
                    Aliases = New String() {}
                End If
            End Get
            Set(avalue As String())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = ConstDelimiter & UCase(avalue(i)) & ConstDelimiter
                        Else
                            aStrValue = aStrValue & UCase(avalue(i)) & ConstDelimiter
                        End If
                    Next i
                    _aliases = aStrValue
                    IsChanged = True
                    'ElseIf Not IsNothing(Trim(avalue)) And Trim(avalue) <> "" And Not isNull(avalue) Then
                    's_aliases = ConstDelimiter & UCase(Trim(avalue)) & ConstDelimiter
                Else
                    _aliases = ""
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the relation ob the entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Relation() As Object
            Get
                Relation = SplitMultbyChar(text:=_relation, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(Relation) Then
                    Relation = New String() {}
                End If
            End Get
            Set(avalue As Object)
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = ConstDelimiter & UCase(avalue(i)) & ConstDelimiter
                        Else
                            aStrValue = aStrValue & avalue(i) & ConstDelimiter
                        End If
                    Next i
                    _relation = aStrValue
                    IsChanged = True
                    'ElseIf Not IsNothing(Trim(aVAlue)) And Trim(aVAlue) <> "" And Not isNull(aVAlue) Then
                    '   s_relation = ConstDelimiter & CStr(Trim(avalue)) & ConstDelimiter
                Else
                    _relation = ""
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the parameter for the object entry
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Parameter() As String
            Get
                Parameter = _parameter
            End Get
            Set(value As String)
                If LCase(_parameter) <> LCase(value) Then
                    _parameter = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns Title (Column Header)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title() As String
            Get
                Title = _title
            End Get
            Set(value As String)
                If _title <> value Then
                    _title = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the Position in the index
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IndexPosition() As Long
            Get
                IndexPosition = _posin
            End Get
            Set(avalue As Long)
                If _posin <> avalue Then
                    _posin = avalue
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
                Indexname = _indexname
            End Get
            Set(value As String)
                If LCase(_indexname) <> LCase(value) Then
                    _indexname = value
                    IsChanged = True
                End If
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
                If _isPrimaryKey <> value Then
                    _isPrimaryKey = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns True if the Entry belongs to a Key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsKey() As Boolean
            Get
                IsKey = _isKey
            End Get
            Set(value As Boolean)
                If _isKey <> value Then
                    _isKey = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the CompoundTablename
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundTablename() As String
            Get
                CompoundTablename = _cTablename
            End Get
            Set(value As String)
                If LCase(_cTablename) <> LCase(value) Then
                    _cTablename = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the fieldname of the compound ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundIDFieldname() As String
            Get
                CompoundIDFieldname = _cIDFieldname
            End Get
            Set(value As String)
                If LCase(_cIDFieldname) <> LCase(value) Then
                    _cIDFieldname = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the fieldname of the compounds value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundValueFieldname() As String
            Get
                CompoundValueFieldname = _cValueFieldname
            End Get
            Set(value As String)
                If LCase(_cValueFieldname) <> LCase(value) Then
                    _cValueFieldname = value
                    IsChanged = True
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns the array of relations of a compound
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CompoundRelation() As String()
            Get
                CompoundRelation = SplitMultbyChar(text:=_cRelation, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(CompoundRelation) Then
                    CompoundRelation = New String() {}
                End If
            End Get
            Set(avalue As String())
                Dim i As Integer
                If IsArrayInitialized(avalue) Then
                    Dim aStrValue As String
                    For i = LBound(avalue) To UBound(avalue)
                        If i = LBound(avalue) Then
                            aStrValue = ConstDelimiter & UCase(avalue(i)) & ConstDelimiter
                        Else
                            aStrValue = aStrValue & avalue(i) & ConstDelimiter
                        End If
                    Next i
                    _cRelation = aStrValue
                    IsChanged = True
                    'ElseIf Not IsNothing(Trim(avalue)) And Trim(avalue) <> "" And Not isNull(avalue) Then
                    '   s_cRelation = ConstDelimiter & CStr(Trim(avalue)) & ConstDelimiter
                Else
                    _cRelation = ""
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the behavior of the table to delete per flag (if true) not per purge the record.
        ''' </summary>
        ''' <value></value>
        Public Property DeleteFlagBehavior() As Boolean
            Get
                Return Me._deletePerFlagBehavior
            End Get
            Set(value As Boolean)
                Me._deletePerFlagBehavior = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the behavior of the table to run with additional (spare) fields.
        ''' </summary>
        ''' <value></value>
        Public Property SpareFieldsBehavior() As Boolean
            Get
                Return Me._SpareFieldsFlagBehavior
            End Get
            Set(value As Boolean)
                Me._SpareFieldsFlagBehavior = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the behavior of the table entries to belong to a domain
        ''' </summary>
        ''' <value></value>
        Public Property DomainBehavior() As Boolean
            Get
                Return Me._domainFlagBehavior
            End Get
            Set(value As Boolean)
                Me._domainFlagBehavior = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            _IsInitialized = MyBase.Initialize()
            If Not Me.TableStore Is Nothing Then
                Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            End If
            Return _IsInitialized
        End Function
        ''' <summary>
        ''' Increase the version
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IncVersion() As Long
            _version = _version + 1
            IncVersion = _version
        End Function

        '**** set the values by CompoundDesc
        '****
        ''' <summary>
        ''' sets the values of this schemadefTableEntry by a FieldDescription
        ''' </summary>
        ''' <param name="FIELDDESC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByFieldDesc(ByRef fielddesc As ormFieldDescription) As Boolean
            If Not IsLoaded And Not IsCreated Then
                SetByFieldDesc = False
                Exit Function
            End If

            Me.ID = UCase(fielddesc.ID)
            Me.Parameter = fielddesc.Parameter
            Me.Title = fielddesc.Title
            Me.Datatype = fielddesc.Datatype
            Me.Objectname = fielddesc.Tablename
            Me.Size = fielddesc.Size
            Me.Typeid = otSchemaDefTableEntryType.Field
            Me.Relation = fielddesc.Relation
            Me.Aliases = fielddesc.Aliases
            Me.IsNullable = fielddesc.IsNullable
            Me.DefaultValueString = fielddesc.DefaultValue
            Me.IsArray = fielddesc.IsArray
            Me.Description = fielddesc.Description
            Me.Version = fielddesc.Version
            Me.Position = fielddesc.ordinalPosition
            Me.SpareFieldTag = fielddesc.SpareFieldTag

            SetByFieldDesc = Me.IsChanged
        End Function
        '**** set the values by CompoundDesc
        '****
        ''' <summary>
        ''' sets the values of this schemadefTableEntry by a CompoundDescription
        ''' </summary>
        ''' <param name="compounddesc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetByCompoundDesc(ByRef compounddesc As ormCompoundDesc) As Boolean
            If Not IsLoaded And Not IsCreated Then
                SetByCompoundDesc = False
                Exit Function
            End If

            If Me.SetByFieldDesc(compounddesc) Then
                Me.Typeid = otSchemaDefTableEntryType.Compound
                Me.CompoundIDFieldname = compounddesc.compound_IDFieldname
                Me.CompoundTablename = compounddesc.compound_Tablename
                Me.CompoundValueFieldname = compounddesc.compound_ValueFieldname
                Me.CompoundRelation = compounddesc.compound_Relation
                'Me.name = COMPOUNDDESC.name

                SetByCompoundDesc = Me.IsChanged
            End If
        End Function
        '**** get the values by FieldDesc
        '****
        ''' <summary>
        ''' fills a compound description out of this SchemaDefTableentry
        ''' </summary>
        ''' <param name="compounddesc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetByCompoundDesc(ByRef compounddesc As ormCompoundDesc) As Boolean
            If Not IsLoaded And Not IsCreated Then
                GetByCompoundDesc = False
                Exit Function
            End If

            If Me.GetByFieldDesc(compounddesc) Then
                compounddesc.compound_IDFieldname = Me.CompoundIDFieldname
                compounddesc.compound_Relation = Me.CompoundRelation
                compounddesc.compound_Tablename = Me.CompoundTablename
                compounddesc.compound_ValueFieldname = Me.CompoundValueFieldname

                GetByCompoundDesc = True
            End If
        End Function
        '**** get the values by FieldDesc
        '****
        ''' <summary>
        ''' fills a field description out of this SchemaDefTableEntry
        ''' </summary>
        ''' <param name="fielddesc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetByFieldDesc(ByRef fielddesc As ormFieldDescription) As Boolean
            If Not IsLoaded And Not IsCreated Then
                GetByFieldDesc = False
                Exit Function
            End If

            fielddesc.ID = UCase(Me.ID)
            fielddesc.Parameter = Me.Parameter
            fielddesc.Title = Me.Title
            fielddesc.Datatype = Me.Datatype
            fielddesc.Tablename = Me.Objectname
            fielddesc.Version = Me.Version
            fielddesc.Aliases = Me.Aliases
            fielddesc.Relation = Me.Relation
            fielddesc.Size = Me.Size
            fielddesc.IsNullable = Me.IsNullable
            fielddesc.IsArray = Me.IsArray
            fielddesc.Description = Me.Description
            fielddesc.DefaultValue = Me.DefaultValueString
            'FIELDDESC.Name = Me.Name

            GetByFieldDesc = True
        End Function

        ''' <summary>
        ''' infuses the object from a record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean
            Dim aVAlue As String

            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try
                '*** infuse all and then specail handling
                If MyBase.Infuse(record) Then

                    aVAlue = CStr(record.GetValue("typeid"))
                    Select Case LCase(aVAlue)
                        Case "field"
                            _typeid = otSchemaDefTableEntryType.Field
                        Case "compound"
                            _typeid = otSchemaDefTableEntryType.Compound
                        Case "table"
                            _typeid = otSchemaDefTableEntryType.Table
                        Case Else
                            Call CoreMessageHandler(arg1:=aVAlue, subname:="clsOTDBSchemaDefTableEntry.infuse", _
                                                    entryname:="typeid", tablename:=ConstTableID, message:=" type id has a unknown value")
                            _typeid = 0
                    End Select
                End If

                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBSchemaDefTableEntry.Infuse", exception:=ex)
                Return False
            End Try

        End Function

        '**** allByID
        '****
        Public Function AllByID(ByVal ID As String, Optional ByVal tablename As String = "") As Collection
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim returnCollection As New Collection
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String
            Dim aNew As New ObjectEntryDefinition

            '* lazy init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    AllByID = Nothing
                    Exit Function
                End If
            End If

            On Error GoTo error_handler

            aTable = GetTableStore(Me.TableID)
            wherestr = " ( ID = '" & UCase(ID) & "' or alias like '%" & ConstDelimiter & UCase(ID) & ConstDelimiter & "%' )"
            If tablename <> "" Then
                wherestr = wherestr & " and tblname = '" & tablename & "'"
            End If
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                _IsLoaded = False
                AllByID = Nothing
                Exit Function
            Else
                For Each aRecord In aRecordCollection

                    aNew = New ObjectEntryDefinition
                    If aNew.Infuse(aRecord) Then
                        aCollection.Add(Item:=aNew)
                    End If
                Next aRecord
                AllByID = aCollection
                Exit Function
            End If

error_handler:

            AllByID = Nothing
            Exit Function
        End Function

        '**** loadByID
        '****
        Public Function LoadByID(ByVal ID As String, Optional ByVal objectname As String = "") As Boolean
            Dim aCollection As New Collection
            Dim aRecordCollection As List(Of ormRecord)
            Dim aTable As iormDataStore
            Dim aRecord As ormRecord
            Dim wherestr As String

            '* lazy init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    LoadByID = False
                    Exit Function
                End If
            End If

            On Error GoTo error_handler

            aTable = GetTableStore(Me.TableID)
            wherestr = " ( ID = '" & UCase(ID) & "' or alias like '%" & ConstDelimiter & UCase(ID) & ConstDelimiter & "%' )"
            If objectname <> "" Then
                wherestr = wherestr & " and tblname = '" & LCase(objectname) & "'"
            End If
            aRecordCollection = aTable.GetRecordsBySql(wherestr:=wherestr)

            If aRecordCollection Is Nothing Then
                _IsLoaded = False
                LoadByID = False
                Exit Function
            Else
                For Each aRecord In aRecordCollection
                    ' take the first
                    If Infuse(aRecord) Then
                        LoadByID = True
                        Exit Function
                    End If
                Next aRecord
                LoadByID = False
                Exit Function
            End If

error_handler:

            LoadByID = False
            Exit Function
        End Function
        ''' <summary>
        ''' load and infuse the object by primary key
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal objectname As String, _
                                         Optional ByVal entryname As String = "", _
                                         Optional ByVal domainID As String = "") As Boolean
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {LCase(objectname), entryname, domainID}
            If MyBase.LoadBy(primarykey) Then
                Return False
            Else
                Dim primarykeyGlobal() As Object = {LCase(objectname), entryname, ConstGlobalDomain}
                Return MyBase.LoadBy(primarykeyGlobal)
            End If
        End Function

        ''' <summary>
        ''' create the schema for this object
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of ObjectEntryDefinition)(addToSchema:=False)
        End Function

        ''' <summary>
        ''' Persist the data object to the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean
            Dim aVAlue As Object

            '* init
            If _runTimeOnly Then
                Call CoreMessageHandler(subname:="clsOTDBSchemaDefTableEntry.Persist", message:="object is runtimeOnly and not persisted", _
                                        arg1:=Me.Objectname, break:=False, messagetype:=otCoreMessageType.InternalWarning)
                Return False
            End If

            'On Error GoTo errorhandle
            If _typeid = otSchemaDefTableEntryType.Table Then
                _entryname = ""
                _xid = ""
                _aliases = ""
                IsPrimaryKey = False
                IsKey = False
                _indexname = ""
                _posin = 0
                _Position = -1
            End If
            Try
                ' feed to record and special type conversion
                If ormDataObject.FeedRecord(Me, Record) Then

                    Select Case _typeid
                        Case otSchemaDefTableEntryType.Field
                            aVAlue = "FIELD"
                        Case otSchemaDefTableEntryType.Compound
                            aVAlue = "COMPOUND"
                        Case otSchemaDefTableEntryType.Table
                            aVAlue = "TABLE"
                        Case Else
                            Call CoreMessageHandler(arg1:=aVAlue, subname:="clsOTDBSchemaDefTableEntry.persist", _
                                                    entryname:="typeid", tablename:=ConstTableID, message:=" type id has a unknown value")
                            aVAlue = "??"
                    End Select
                    Record.SetValue(ConstFNType, aVAlue)
                    ' persist with update timestamp
                    Return MyBase.Persist(timestamp, doFeedRecord:=False)
                End If
                Return False
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBSchemaDefTableEntry.Persist")
                Return False
            End Try

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
        Public Overloads Function Create(ByVal objectname As String, _
        Optional ByVal entryname As String = "", _
        Optional ByVal domainID As String = "", _
        Optional ByVal typeid As otSchemaDefTableEntryType = Nothing, _
        Optional ByVal runtimeOnly As Boolean = False, _
        Optional ByVal checkunique As Boolean = True) As Boolean
            If domainID = "" Then domainID = CurrentSession.CurrentDomainID
            Dim primarykey() As Object = {LCase(objectname), entryname, domainID}

            ' create
            If MyBase.Create(primarykey, checkUnique:=checkunique, noInitialize:=runtimeOnly) Then
                ' set the primaryKey
                _objectname = LCase(objectname)
                _entryname = entryname
                _typeid = typeid
                _runTimeOnly = runtimeOnly
                _domainID = domainID
                Return Me.IsCreated
            Else
                Return False
            End If

        End Function

        '***  checkOnFieldList
        Public Function CheckonFieldList(Value As Object, Optional MSGLOG As clsOTDBMessagelog = Nothing) As Boolean
            ' TODO:
            Throw New NotImplementedException
        End Function
        '***  checkOnFieldType checks the field values on Integrity

        Public Function CheckOnFieldType(Value As Object, Optional MSGLOG As clsOTDBMessagelog = Nothing) As Boolean

            ' default
            CheckOnFieldType = True

            '** check on Datatypes
            Select Case Me.Datatype

                Case otFieldDataType.Numeric
                    If Not IsNumeric(Value) Then
                        'Call MSGLOG.addMsg("100", Nothing, Nothing, Me.ID, Value)
                        CheckOnFieldType = False
                    End If
                Case otFieldDataType.List
                    If Not CheckonFieldList(Value, MSGLOG) Then
                        'Call msglog.addMsg("101", Me.id, -1, Value, Me.parameter)
                        CheckOnFieldType = False
                    End If
                Case otFieldDataType.Text

                Case otFieldDataType.Runtime

                Case otFieldDataType.Formula

                Case otFieldDataType.[Date], otFieldDataType.[Time], otFieldDataType.Timestamp
                    If Not IsDate(Value) Then
                        'Call MSGLOG.addMsg("104", Nothing, Nothing, Me.ID, Value)
                        CheckOnFieldType = False
                    End If
                Case otFieldDataType.[Long]
                    If Not IsNumeric(Value) Then
                        'Call MSGLOG.addMsg("102", Nothing, Nothing, Me.ID, Value)
                        CheckOnFieldType = False
                    End If
                Case otFieldDataType.Timestamp

                Case otFieldDataType.Bool

                Case otFieldDataType.Memo

                Case otFieldDataType.Binary

                Case Else

                    'Call MSGLOG.addMsg("105", Nothing, Nothing, Me.ID, Me.DATATYPE)
                    CheckOnFieldType = False

            End Select

            '** say checking is ok
            If CheckOnFieldType Then
                'Call MSGLOG.addMsg("190", Nothing, Nothing, Me.ID, Me.DATATYPE)
            End If

        End Function

    End Class


    '************************************************************************************
    '***** CLASS clsOTDBMessageLog is a representation class for a Log as Messages
    '*****

    Public Class clsOTDBMessagelog
    Inherits ormDataObject
    Implements otLoggable
    Implements iormInfusable
    Implements iormPersistable


        Const _tableID As String = "tblMessageLogs"

        ' Data
        Private s_tag As String = ""
        Private s_members As New Collection

        Private s_DefaultFCLCstatus As New clsOTDBDefStatusItem
        Private s_DefaultProcessStatus As New clsOTDBDefStatusItem

        '** for ERROR MSG
        Private s_ContextIdentifier As Object
        Private s_TupleIdentifier As Object
        Private s_EntitityIdentifier As Object

        '** initialize
        Public Sub New()
            Call MyBase.New(_tableID)
        End Sub

        ReadOnly Property TAG()
            Get
                TAG = s_tag
            End Get

        End Property

        ReadOnly Property Size() As Integer
            Get
                Size = s_members.Count
            End Get
        End Property
        ReadOnly Property count() As Integer
            Get
                count = Size
            End Get

        End Property
        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize() As Boolean

            s_members = New Collection
            s_ContextIdentifier = Nothing
            s_TupleIdentifier = Nothing
            s_EntitityIdentifier = Nothing
            'Set s_members = New Dictionary
            's_parameter_date1 = ot.ConstNullDate
            's_parameter_date2 = ot.ConstNullDate
            's_parameter_date3 = ot.ConstNullDate
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' delete the Log and all members
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Delete() As Boolean
            Dim anEntry As New clsOTDBMessageLogMember
            Dim initialEntry As New clsOTDBMessageLogMember
            Dim m As Object

            If Not Me.IsCreated And Not _IsLoaded Then
                Delete = False
                Exit Function
            End If

            ' delete each entry
            For Each m In s_members
                anEntry = m
                anEntry.Delete()
            Next m

            ' reset it

            s_members = New Collection
            'If Not anEntry.create(tag:=Me.tag, id:=0) Then
            '    Call anEntry.loadBy(tag:=Me.tag, id:=0)
            'End If
            's_members.add value:=anEntry

            _IsCreated = True
            _IsDeleted = True
            _IsLoaded = False

        End Function

        '*** addMsg adds a Message to the MessageLog with the associated
        '***
        '*** Contextordinal (can be Nothing) as MQF or other ordinal
        '*** Tupleordinal (can be Nothing) as Row or Dataset
        '*** Entity (can be Nothing) per Field or ID

        '***
        '*** looks up the Messages and Parameters from the MessageLogTable
        '*** returns true if successfull

        Public Function AddMsg(ByVal msgid As String, _
        ByVal ContextIdentifier As String, _
        ByVal TupleIdentifier As String, _
        ByVal EntitityIdentifier As String, _
        ParamArray Args() As Object) As Boolean

            Dim i As Integer
            Dim aMSGDef As New clsOTDBDefLogMessage()
            Dim messagetext As String
            Dim fclcStatusCode As String
            Dim ProcessStatusCode As String
            Dim weight As Single
            Dim areaString As String
            Dim newFCLCStatus As New clsOTDBDefStatusItem
            Dim newProcessStatus As New clsOTDBDefStatusItem
            Dim Value As Object
            Dim messagetype As Integer
            Dim aMember As New clsOTDBMessageLogMember

            ' get the Table
            If Not aMSGDef.LoadBy(ID:=msgid) Then
                Call CoreMessageHandler(showmsgbox:=True, arg1:=msgid, subname:="clsOTDBMessageLog.addmsg", message:=" Message ID couldn't be found")
                AddMsg = False
                Exit Function
            End If
            ' get values
            aMember.Msgid = msgid
            aMember.Message = aMSGDef.Message
            aMember.Msgdef = aMSGDef
            If IsNothing(ContextIdentifier) Or IsNothing(ContextIdentifier) Then
                ContextIdentifier = s_ContextIdentifier
            End If
            If IsNothing(TupleIdentifier) Or IsNothing(TupleIdentifier) Then
                TupleIdentifier = s_TupleIdentifier
            End If
            If IsNothing(EntitityIdentifier) Or IsNothing(EntitityIdentifier) Then
                EntitityIdentifier = s_EntitityIdentifier
            End If
            '* set it
            aMember.ContextIdentifier = ContextIdentifier
            aMember.TupleIdentifier = TupleIdentifier
            aMember.EntitityIdentifier = EntitityIdentifier

            fclcStatusCode = aMSGDef.GetStatusCodeOf(OTDBConst_StatusTypeid_FCLF)
            ProcessStatusCode = aMSGDef.GetStatusCodeOf(OTDBConst_StatusTypeid_ScheduleProcess)
            weight = CSng(aMSGDef.Weight)
            areaString = aMSGDef.Area
            messagetype = aMSGDef.TypeID


            '* replace
            If Not IsNothing(TupleIdentifier) Then
                aMember.Message = Replace(aMember.Message, "%uid%", TupleIdentifier)
                aMember.Message = Replace(aMember.Message, "%Tupleid%", TupleIdentifier)
            End If
            If Not IsNothing(ContextIdentifier) Then
                aMember.Message = Replace(aMember.Message, "%contextid%", ContextIdentifier)
            End If
            If Not IsNothing(EntitityIdentifier) Then
                aMember.Message = Replace(aMember.Message, "%EntitiyID%", EntitityIdentifier)
                aMember.Message = Replace(aMember.Message, "%ids%", EntitityIdentifier)
            End If

            'aMember.message = Replace(aMember.message, "%rowno%", aRowNo)
            aMember.Message = Replace(aMember.Message, "%type%", UCase(aMSGDef.GetMessageTypeName(aMSGDef.TypeID)))
            aMember.Message = Replace(aMember.Message, "%errno%", msgid)

            '*
            For i = LBound(Args) To UBound(Args)
                aMember.Message = Replace(aMember.Message, "%" & i + 1, Args(i))
            Next i

            '* save

            Call s_members.Add(aMember)

            '*
            AddMsg = True
            Exit Function
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAllMsg() As String()
            Dim m As Object
            Dim i As Integer
            Dim msgs() As String
            Dim aMember As clsOTDBMessageLogMember

            For i = 1 To s_members.Count
                ReDim Preserve msgs(i)
                aMember = s_members.Item(i)
                msgs(i) = aMember.Message
            Next i

            GetAllMsg = msgs
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMsgDef(i) As clsOTDBDefLogMessage
            Dim aMember As clsOTDBMessageLogMember
            If i <= Me.Size And i > 0 Then
                aMember = s_members.Item(i)
                GetMsgDef = aMember.Msgdef
                Exit Function
            End If

            GetMsgDef = Nothing
        End Function
        Public Function GetMessage(i) As String
            Dim aMember As clsOTDBMessageLogMember
            If i <= Me.Size And i > 0 Then
                aMember = s_members.Item(i)
                GetMessage = aMember.Message
                Exit Function
            End If

            GetMessage = ""
        End Function
        Public Function GetMSGID(i) As String
            Dim aMember As clsOTDBMessageLogMember
            If i <= Me.Size And i > 0 Then
                aMember = s_members.Item(i)
                GetMSGID = aMember.Msgid
                Exit Function
            End If

            GetMSGID = ""
        End Function
        Public Function GetMember(i) As clsOTDBMessageLogMember
            Dim aMember As clsOTDBMessageLogMember
            If i <= Me.Size And i > 0 Then
                aMember = s_members.Item(i)
                GetMember = aMember
                Exit Function
            End If

            GetMember = Nothing
        End Function
        Public Function GetTypeID(i As Integer) As otAppLogMessageType
            Dim aMember As clsOTDBMessageLogMember

            If i <= Me.Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetTypeID = aMember.Msgdef.TypeID
            Else
                GetTypeID = 0
            End If
        End Function
        Public Function GetWeight(i As Integer) As Single
            Dim aMember As clsOTDBMessageLogMember

            If i <= Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetWeight = aMember.Msgdef.Weight
                'getWeight = msgweight(i - 1)
            Else
                GetWeight = 0
            End If
        End Function
        Public Function GetEntitityID(i As Integer) As Object
            Dim aMember As clsOTDBMessageLogMember

            If i <= Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetEntitityID = aMember.EntitityIdentifier
            Else
                GetEntitityID = Nothing
            End If
        End Function
        Public Function GetContextID(i As Integer) As Object
            Dim aMember As clsOTDBMessageLogMember

            If i <= Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetContextID = aMember.ContextIdentifier
            Else
                GetContextID = Nothing
            End If
        End Function
        Public Function GetTupleID(i As Integer) As Object
            Dim aMember As clsOTDBMessageLogMember

            If i <= Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetTupleID = aMember.TupleIdentifier
            Else
                GetTupleID = Nothing
            End If
        End Function
        Public Function GetArea(i As Integer) As String
            Dim aMember As clsOTDBMessageLogMember

            If i <= Size And i > 0 Then
                aMember = Me.GetMember(i)
                GetArea = aMember.Msgdef.Area
                'getArea = area(i - 1)
            Else
                GetArea = ""
            End If
        End Function
        Public Function GetMsg(i As Integer) As String
            GetMsg = GetMessage(i)
        End Function

        Public Function GetStatus(Optional ByVal TYPEID As Object = Nothing, Optional ByVal i As Integer = 0) As Object
            Dim max As Integer
            Dim curweight As Single
            Dim aMember As clsOTDBMessageLogMember
            Dim aDefMSG As New clsOTDBDefLogMessage
            Dim code As String
            Dim aStatus As New clsOTDBDefStatusItem

            Dim newStatus As New clsOTDBDefStatusItem

            ' specific of an entry
            If Not IsNothing(i) And i > 0 Then
                If i <= Me.Size Then
                    aMember = Me.GetMember(i)
                    aDefMSG = aMember.Msgdef
                    '** per TypeID
                    If Not IsNothing(TYPEID) Then
                        code = aDefMSG.GetStatusCodeOf(TYPEID)
                        If aStatus.LoadBy(TYPEID, code) Then
                            GetStatus = aStatus
                        Else
                            GetStatus = Nothing
                        End If
                    Else
                        Dim code1, code2, code3 As String
                        Dim weight1, weight2, weight3 As Integer
                        Dim status1 As New clsOTDBDefStatusItem
                        Dim status2 As New clsOTDBDefStatusItem
                        Dim status3 As New clsOTDBDefStatusItem

                        If status1.LoadBy(aDefMSG.Statustype1, aDefMSG.Statuscode1) Then
                            weight1 = status1.Weight
                        Else
                            weight1 = 0
                        End If
                        If status2.LoadBy(aDefMSG.Statustype2, aDefMSG.Statuscode2) Then
                            weight2 = status2.Weight
                        Else
                            weight2 = 0
                        End If
                        If status3.LoadBy(aDefMSG.Statustype3, aDefMSG.Statuscode3) Then
                            weight3 = status3.Weight
                        Else
                            weight3 = 0
                        End If
                        ' get maximum
                        If weight1 = 0 And weight2 = 0 And weight3 = 0 Then
                            GetStatus = Nothing
                        ElseIf weight1 >= weight2 And weight1 >= weight3 Then
                            GetStatus = status1
                        ElseIf weight2 >= weight1 And weight2 >= weight3 Then
                            GetStatus = status2
                        ElseIf weight3 >= weight2 And weight3 >= weight2 Then
                            GetStatus = status3
                        End If

                    End If
                Else
                    GetStatus = Nothing
                End If
                Exit Function
            End If

            ' else return the maximum
            If Size = 0 And Not IsNothing(TYPEID) Then
                If TYPEID = OTDBConst_StatusTypeid_ScheduleProcess Then
                    GetStatus = s_DefaultProcessStatus
                    Exit Function
                ElseIf TYPEID = OTDBConst_StatusTypeid_FCLF Then
                    GetStatus = s_DefaultFCLCstatus
                    Exit Function
                End If
            End If

            ' return the status assoc. with the highest weight of messages
            curweight = 0
            For i = 1 To Me.Size
                If Me.GetWeight(i) > curweight Then
                    curweight = Me.GetWeight(i)
                    aMember = Me.GetMember(i)
                    aDefMSG = aMember.Msgdef
                    code = aDefMSG.GetStatusCodeOf(TYPEID)
                    aStatus = New clsOTDBDefStatusItem
                    If aStatus.LoadBy(TYPEID, code) Then
                        GetStatus = aStatus
                    Else
                        GetStatus = Nothing
                    End If
                End If
            Next i

        End Function

        Public Function GetProcessStatus(Optional ByVal i As Integer = 0) As Object

            GetProcessStatus = Me.GetStatus(OTDBConst_StatusTypeid_ScheduleProcess, i)


        End Function
        Public Function getFCLCStatus(Optional ByVal i As Integer = 0) As Object
            getFCLCStatus = Me.GetStatus(OTDBConst_StatusTypeid_FCLF, i)
        End Function
        ''' <summary>
        ''' infuses the message log by a record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse
            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try
                s_tag = CStr(record.GetValue("tag"))
                's_description = CStr(aRecord.getValue("desc"))

                Infuse = MyBase.Infuse(record)
                _IsLoaded = True
                Exit Function

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBMessagelog.Infuse")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' load and infuse the message log by primary key
        ''' </summary>
        ''' <param name="TAG"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal TAG As String) As Boolean
            Dim aTable As iormDataStore
            Dim aRecordCollection As List(Of ormRecord)
            Dim aRecord As ormRecord
            Dim cmid As String
            Dim posno As Long
            Dim qty As Double
            Dim anEntry As New clsOTDBMessageLogMember

            Dim pkarry(1) As Object

            '* lazy init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            ' set the primaryKey
            pkarry(0) = LCase(TAG)
            ' try to load it from cache
            'Set aRecord = loadFromCache(ourTableName, PKArry)
            'If aRecord Is Nothing Then
            'Set aTable = getOTDBTableClass(ourTableName)
            'Set aRecord = aTable.getRecordByPrimaryKey(PKArry)
            'End If

            'If aRecord Is Nothing Then
            '    isLoaded = False
            '    loadBy = isLoaded
            '    Exit Function
            'Else
            'Set me.record = aRecord
            'isLoaded = Me.infuse(me.record)
            'loadBy = isLoaded
            'Call addToCache(ourTableName, Key:=PKArry, Object:=aRecord)
            ' load the members
            Dim wherestr As String
            aTable = GetTableStore(anEntry.TableID)
            aRecordCollection = aTable.GetRecordsBySql(wherestr:="tag = '" & TAG & "'", orderby:=" id asc")
            ' record collection
            If aRecordCollection Is Nothing Then
                _IsLoaded = False
                LoadBy = False
                Exit Function
            Else
                s_tag = TAG
                _IsLoaded = True
                ' records read
                For Each aRecord In aRecordCollection

                    ' add the Entry as Component
                    anEntry = New clsOTDBMessageLogMember
                    If anEntry.Infuse(aRecord) Then

                    End If
                Next aRecord
                '
                _IsLoaded = True
                LoadBy = True
                Exit Function
            End If
            Exit Function
            'End If


            error_handler:
            _IsLoaded = False
            LoadBy = True
            Exit Function
        End Function

        ''' <summary>
        ''' persist the message log
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist() As Boolean
            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Persist = False
                    Exit Function
                End If
            End If
            If Not IsLoaded And Not IsCreated Then
                Persist = False
                Exit Function
            End If

            'If Not me.record.alive Then
            '    persist = False
            '    Exit Function
            'End If

            ' persist the head
            'Call me.record.setValue("tag", s_tag)
            'Call me.record.setValue("desc", s_description)

            'persist = me.record.persist

            Dim anEntry As clsOTDBMessageLogMember
            Dim aTimestamp As Date

            ' set Timestamp
            aTimestamp = Now
            ' delete each entry
            If s_members.Count > 0 Then
                For Each anEntry In s_members
                    anEntry.Persist(aTimestamp)
                Next anEntry
            End If

            Return True
            Exit Function

            errorhandle:

            Persist = False

        End Function

        '********** static createSchema
        '********** create the Schema for the Directory to enable bootstrapping provide the Connection to be used
        '**********
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean


            CreateSchema = False
        End Function

        ''' <summary>
        ''' create a message log with a primary key
        ''' </summary>
        ''' <param name="tag"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal tag As String) As Boolean
            Dim primarykey() As Object = {LCase(tag)}

            '* lazy init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Create = False
                    Exit Function
                End If
            End If

            If IsLoaded Then
                Create = False
                Exit Function
            End If

            ' set the primaryKey
            If MyBase.Create(primarykey, checkUnique:=False) Then
                s_tag = LCase(tag)
                s_members = New Collection
                Return True
            Else
                Return False
            End If

        End Function

        '***** raiseMessage informs the Receiver about the Message-Event
        '*****
        Public Function raiseMessage(ByVal index As Long, _
        ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.raiseMessage

        End Function

        '***** hands over the msglog object to the receiver
        '*****
        Public Function attachMessageLog(ByRef MSGLOG As clsOTDBMessagelog) As Boolean Implements otLoggable.attachMessageLog

        End Function

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property ContextIdentifier As String Implements otLoggable.ContextIdentifier
            Get
                ContextIdentifier = s_ContextIdentifier
            End Get
            Set(value As String)
                s_ContextIdentifier = value
            End Set
        End Property

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property TupleIdentifier() As String Implements otLoggable.TupleIdentifier
            Get
                TupleIdentifier = s_TupleIdentifier
            End Get
            Set(value As String)
                s_TupleIdentifier = value
            End Set
        End Property

        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Public Property EntitityIdentifier() As String Implements otLoggable.EntitityIdentifier
            Get
                EntitityIdentifier = s_EntitityIdentifier
            End Get
            Set(value As String)
                s_EntitityIdentifier = value
            End Set
        End Property

    End Class


    '************************************************************************************
    '***** CLASS lsOTDBMessageLogMember is a helper for the FieldDesc Attributes
    '*****
    '*****

    Public Class clsOTDBMessageLogMember
    Inherits ormDataObject
    Implements iormInfusable
    Implements iormPersistable


        Const _TableID As String = "tblMessageLogs"

        Private s_tag As String = ""
        Private s_id As Long
        Private s_msgid As String = ""

        Private s_message As String = ""
        Private s_msgdef As New clsOTDBDefLogMessage
        Private s_ContextID As String = ""
        Private s_TupleID As String = ""
        Private s_EntitityID As String = ""

        ''' <summary>
        ''' constructor of a message log member
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(_TableID)
        End Sub

        #Region "properties"

        ReadOnly Property TAG() As String
            Get
                TAG = s_tag
            End Get
        End Property

        Public Property ID() As Long
            Get
                ID = s_id
            End Get
            Set(value As Long)
                If value <> s_id Then
                    s_id = value
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Msgid() As String
            Get
                Msgid = s_msgid
            End Get
            Set(avalue As String)
                If LCase(s_msgid) <> LCase(avalue) Then
                    s_msgid = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Message() As String
            Get
                Message = s_message
            End Get
            Set(value As String)
                If LCase(Message) <> LCase(value) Then
                    s_message = value
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Msgdef() As clsOTDBDefLogMessage
            Get
                Msgdef = s_msgdef

            End Get
            Set(avalue As clsOTDBDefLogMessage)
                If s_msgdef.ID <> avalue.ID Then
                    s_msgdef = avalue
                    Me.Msgid = avalue.ID
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property ContextIdentifier() As Object
            Get
                ContextIdentifier = s_ContextID
            End Get
            Set(value As Object)
                If LCase(s_ContextID) <> LCase(value) Then
                    s_ContextID = value
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property TupleIdentifier() As Object
            Get
                TupleIdentifier = s_TupleID
            End Get
            Set(avalue As Object)
                If LCase(s_TupleID) <> LCase(avalue) Then
                    s_TupleID = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property EntitityIdentifier() As Object
            Get
                EntitityIdentifier = s_EntitityID

            End Get
            Set(value As Object)
                If LCase(s_EntitityID) <> LCase(value) Then
                    s_EntitityID = value
                    IsChanged = True
                End If
            End Set
        End Property
        #End Region


        ''' <summary>
        ''' infuses a message log member by record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try
                s_tag = CStr(record.GetValue("tag"))
                s_id = CLng(record.GetValue("idno"))

                s_message = CStr(record.GetValue("msgtxt"))
                s_msgid = CStr(record.GetValue("msgid"))
                s_ContextID = CStr(record.GetValue("contextid"))
                s_TupleID = CStr(record.GetValue("Tupleid"))
                s_EntitityID = CStr(record.GetValue("entitityid"))
                s_msgdef = New clsOTDBDefLogMessage
                If s_msgid <> "" Then
                    If Not s_msgdef.LoadBy(s_msgid) Then
                        Call CoreMessageHandler(arg1:=s_msgid, message:="msgid not found in tblDefLogMessages", _
                                                subname:="clsOTDBMessageLogMember.infuse")
                    End If
                End If

                Return MyBase.Infuse(record)
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageLogMember.Infuse")
                Return False
            End Try


        End Function

        ''' <summary>
        ''' loads and infuses a message log member
        ''' </summary>
        ''' <param name="msglogtag"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal msglogtag As String, ByVal ID As Long) As Boolean
            Dim primarykey() As Object = {LCase(msglogtag), ID}
            Return MyBase.LoadBy(primarykey)
        End Function

        ''' <summary>
        ''' create peristency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTable As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Tablename = _TableID

            With aTable
                .Create(_TableID)
                .Delete()

                '***
                '*** Fields
                '****

                'Type
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "msglogtag"
                aFieldDesc.ID = "log1"
                aFieldDesc.ColumnName = "tag"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)
                'index pos
                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "posno in index (primary key)"
                aFieldDesc.ColumnName = "idno"
                aFieldDesc.ID = "log2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message text"
                aFieldDesc.ColumnName = "msgtxt"
                aFieldDesc.ID = "log3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' msgid
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message id"
                aFieldDesc.ColumnName = "msgid"
                aFieldDesc.ID = "log4"
                aFieldDesc.Relation = New String() {"lm1"}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "context Identifier"
                aFieldDesc.ColumnName = "contextid"
                aFieldDesc.ID = "log5"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "Tuple Identifier"
                aFieldDesc.ColumnName = "Tupleid"
                aFieldDesc.ID = "log6"
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' id
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "Member Identifier"
                aFieldDesc.ColumnName = "entitityid"
                aFieldDesc.ID = "log7"
                aFieldDesc.Relation = New String() {"xid"}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Relation = New String() {}
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                aFieldDesc.Relation = New String() {}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)
                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            CreateSchema = True
            Exit Function
        End Function

        ''' <summary>
        ''' Persist the message log member
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Try
                'On Error GoTo errorhandle
                Call Me.Record.SetValue("tag", s_tag)
                Call Me.Record.SetValue("id", s_id)
                Call Me.Record.SetValue("msgtxt", s_message)
                Call Me.Record.SetValue("msgid", s_msgid)

                Call Me.Record.SetValue("contextid", s_ContextID)
                Call Me.Record.SetValue("Tupleid", s_TupleID)
                Call Me.Record.SetValue("entitityid", s_EntitityID)

                Return MyBase.Persist(timestamp)

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBMessageLogMember.Persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Create a persistable Message Log Member by primary key
        ''' </summary>
        ''' <param name="msglogtag"></param>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal msglogtag As String, ByVal ID As Long) As Boolean
            Dim primarykey() As Object = {LCase(msglogtag), ID}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                ' set the primaryKey
                s_tag = LCase(msglogtag)
                s_id = ID

                Return Me.IsCreated
            Else
                Return False
            End If


        End Function

    End Class
End Namespace
