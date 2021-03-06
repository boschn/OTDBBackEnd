﻿REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE Module (all static functions) for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Diagnostics.Debug
Imports System.Reflection

Imports OnTrack
Imports OnTrack.Database
Imports OnTrack.UI
Imports System.Threading

' Delegate declaration.
'
Public Delegate Sub onErrorRaised(sender As Object, e As ormErrorEventArgs)

Namespace OnTrack

    Public Module ot

        ''' <summary>
        ''' Version with Changelog
        ''' </summary>
        ''' <remarks></remarks>
        <ormChangeLogEntry(Application:=ConstApplicationBackend, Module:=ConstModuleCommons, Version:=1, Release:=0, patch:=3, changeimplno:=1, _
            description:="Introducing ChangeLogEntries as Business Objects")> _
        Public Const ConstVersionCoreBackend As String = "1.0.3"

        ' max size
        Public Const ConstDBDriverMaxTextSize = 255
        Public Const constDBDriverMaxMemoSize = 16000

        'Depend TypeIDs
        ''' <summary>
        ''' Dependency Type Interfaces
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstDepTypeIDIFC As String = "IFC"
        Public Const OTDBConst_DependStatus_g2 As String = "g2"
        Public Const OTDBConst_DependStatus_g1 As String = "g1"
        Public Const OTDBConst_DependStatus_y1 As String = "y1"
        Public Const OTDBConst_DependStatus_y2 As String = "y2"
        Public Const OTDBConst_DependStatus_r1 As String = "r1"
        Public Const OTDBConst_DependStatus_r2 As String = "r2"
        Public Const OTDBConst_DependStatus_r3 As String = "r3"    ' r3 no valid schedules
        ' public const
        Public Const constNullDate As Date = #1/1/1900#
        Public Const ConstNullTime As Date = #12:00:00 AM#
        Public Const ConstNullTimestampString = "1900-01-01T00:00:00"
        '** common fieldnames
        Public Const ConstFNUpdatedOn As String = "UPDATEDON"
        Public Const ConstFNCreatedOn As String = "CREATEDON"
        Public Const ConstFNDeletedOn As String = "DELETEDON"
        Public Const ConstFNIsDeleted As String = "ISDELETED"

        Public Const ConstDefaultTrackItemListDevOrder As String = "dev.order"
        ''' <summary>
        ''' Default Delimiter for String Expressions
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstDelimiter As String = "|"
        Public Const ConstFirstPlanRevision As String = "V1.0"

        Public Const ConstDefaultConfigFileName As String = "otdbconfig.ini"
        Public Const ConstDefaultToolingNamePattern As String = "OnTrack*"
        Public Const ConstDefaultAccessRight As Integer = otAccessRight.[ReadOnly]

        Public Const ConstXChangeClearFieldValue As String = "-"
        Private Const OTDBConst_ConfigDBPassword As String = "axs2ontrack"

        Public Const OTDBConst_MessageTypeid_warning = "WARNING"
        Public Const OTDBConst_MessageTypeid_attention = "ATTENTION"
        Public Const OTDBConst_MessageTypeid_info = "INFO"
        Public Const OTDBConst_MessageTypeid_error = "ERROR"

        ''' <summary>
        ''' StatusTypes
        ''' </summary>
        ''' <remarks></remarks>

        Public Const ConstStatusType_XEnvelope As String = "XCHANGEENVELOPE"
        Public Const ConstStatusType_MQF As String = "MQF"
        Public Const ConstStatusType_MQMessage As String = "MQMESSAGE"
        Public Const ConstStatusType_ObjectEntryValidation As String = "ENTRYVALIDATOR"
        Public Const ConstStatusType_ObjectValidation As String = "OBJECTVALIDATOR"
        Public Const ConstStatusType_ScheduleLifecycle As String = "SCHEDULINGLFCL"
        Public Const ConstStatusType_ScheduleProcess As String = "SCHEDULINGPROC"
        Public Const ConstStatusType_Tracking As String = "TRACKING"

        ''' <summary>
        ''' obsolete
        ''' </summary>
        ''' <remarks></remarks>
        Public Const OTDBConst_StatusTypeid_FCLF = "FCLF"
        Public Const OTDBConst_StatusTypeid_ScheduleProcess = "SPROC"
        Public Const OTDBConst_StatusTypeid_MQF = "MQF"

        ''' <summary>
        ''' CompoundIndex
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstDefaultCompoundIndexName = "CompoundIndex"

        ''' parameters stored with DB Driver Parameters
        '
        Public Const ConstPNObjectsLoad = "loadobjects"
        Public Const ConstPNBootStrapSchemaChecksum = "bootstrapschemaversion"
        Public Const ConstPNBSchemaVersion_TableHeader = "schemaversion_"
        Public Const ConstPNBSchemaVersion = "dbschemaversion"
        Public Const ConstPNCalendarInitializedFrom = "calendarinitializedfrom"
        Public Const ConstPNCalendarInitializedto = "calendarinitializedto"

        ''' <summary>
        ''' The Schema Version - increase here to trigger recreation of the database schema
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <ormChangeLogEntry(application:=ConstApplicationBackend, module:=ConstPNBSchemaVersion, version:=11, release:=0, patch:=0, changeimplno:=1, _
            description:="ChangeLog Entry added")> _
        Public Const ConstOTDBSchemaVersion = 11

        '** config parameters
        ''' <summary>
        ''' Config Property name
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstGlobalConfigSetName = "global"

        ''' <summary>
        '''  Parameters names for config parameters read from .ini or documents
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstCPNUseConfigSetName = "otdb_parameter_configsetname" ' ConfigSetname to use
        Public Const ConstCPNConfigFileName = "otdb_parameter_configfilename"
        Public Const ConstCPNConfigFileLocation = "otdb_parameter_configfilelocation"
        Public Const ConstCPNDriverName = "otdb_parameter_drivername"
        Public Const ConstCPNDBType = "otdb_parameter_databasetype"
        Public Const ConstCPNDBPath = "otdb_parameter_dbpath"
        Public Const ConstCPNDBName = "otdb_parameter_dbname"
        Public Const ConstCPNDBUser = "otdb_parameter_dbuser"
        Public Const ConstCPNDBPassword = "otdb_parameter_dbpassword"
        Public Const ConstCPNDBSQLServerUseMars = "otdb_parameter_sqlserverusemars"
        Public Const ConstCPNDBConnection = "otdb_parameter_connection"
        Public Const ConstCPNDBUseseek = "otdb_parameter_driver_useseek"
        Public Const ConstCPNDescription = "otdb_parameter_configset_description"
        Public Const constCPNUseLogAgent = "otdb_parameter_uselogagent"
        Public Const constCPNDefaultDomainid = "otdb_parameter_default_domainid"
        ''' <summary>
        ''' config Property value
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstCPVDBTypeSqlServer = "sqlserver"
        Public Const ConstCPVDBTypeAccess = "access"
        Public Const ConstCPVDriverADOClassic = "adoclassic"
        Public Const ConstCPVDriverOleDB = "OLEDB"
        Public Const ConstCPVDriverMSSQL = "MSSQL"

        ''' <summary>
        ''' Global Domain Name
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstGlobalDomain = "@"


        '** MQF operation codes
        Public Const ConstMQFOpDelete = "DELETE"
        Public Const ConstMQFOpChange = "CHANGE"
        Public Const ConstMQFOpFreeze = "FREEZE"
        Public Const ConstMQFOpNoop = "NOOP"
        Public Const ConstMQFOpAddRevision = "ADD-REVISION"
        Public Const ConstMQFOpAddAfter = "ADD-AFTER"

        '**** create ordinal with this
        Public Const constXCHCreateordinal = 990000000000

        ''' <summary>
        ''' Application names
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstApplicationBackend = "otBackend"

        ''' <summary>
        ''' Name of the different OnTrack Modules
        ''' </summary>
        ''' <remarks></remarks>
        Public Const ConstModuleCommons = "Commons"
        Public Const ConstModuleRepository = "Repository"
        Public Const ConstModuleCalendar = "Calendar"
        Public Const ConstModuleConfiguration = "Configuration"
        Public Const ConstModuleProperties = "Properties"
        Public Const ConstModuleScheduling = "Scheduling"
        Public Const ConstModuleParts = "Parts"
        Public Const ConstModuleDeliverables = "Deliverables"
        Public Const ConstModuleStatistics = "Statistics"
        Public Const ConstModuleMessageQueue = "Message Queuing"
        Public Const ConstModuleDependency = "Dependencies"
        Public Const ConstModuleTracking = "Tracking"
        Public Const ConstModuleXChange = "XChange"

       
        Public NullArray As Object = {}


        ''' <summary>
        ''' Variables
        ''' </summary>
        ''' <remarks></remarks>
        Private _ApplicationName As String = ""
        Private _Version As String

        Private WithEvents _CurrentSession As Session
        Private _configfilelocations As List(Of String) = New List(Of String)
        Private _UsedConfigFileLocation As String = ""
        ' initialized Flag
        Private _OTDBIsInitialized As Boolean = False

        '*** config sets
        Private _configurations As New ComplexPropertyStore(ConstGlobalConfigSetName)
        Private _configPropertiesRead As Boolean = False

        '** dictionary for dataobjects
        Private _tableDataObjects As New Dictionary(Of String, System.Type)
        Private _ObjectClassStore As New ObjectClassRepository
        Private _bootstrapObjectIds As New List(Of String)
        Private _bootstrapclassnames As New List(Of String)

        ''' <summary>
        ''' global OnTrack ChangeLog
        ''' </summary>
        ''' <remarks></remarks>
        Private _changelog As New OnTrackChangeLog

#Region "Properties"
        ''' <summary>
        ''' returns the Changelog
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property OnTrackChangeLog As OnTrackChangeLog
            Get
                Return _changelog
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property AssemblyVersion() As String
            Get
                If String.IsNullOrWhiteSpace(_Version) Then Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
                Return _Version
            End Get
            Set(value As String)
                _Version = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the application.
        ''' </summary>
        ''' <value>The name of the application.</value>
        Public Property ApplicationName() As String
            Get
                If String.IsNullOrWhiteSpace(_ApplicationName) Then
                    ' Get all Title attributes on this assembly
                    Dim attributes As Object() = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(GetType(AssemblyTitleAttribute), False)
                    ' If there is at least one Title attribute
                    If attributes.Length > 0 Then
                        ' Select the first one
                        Dim titleAttribute As AssemblyTitleAttribute = CType(attributes(0), AssemblyTitleAttribute)
                        ' If it is not an empty string, return it
                        If titleAttribute.Title <> "" Then
                            Return titleAttribute.Title
                        End If
                    End If
                End If

                Return _ApplicationName
            End Get
            Set(value As String)
                _ApplicationName = value
            End Set
        End Property
        ''' <summary>
        ''' returns the name of the standard Config set to be used - might be nothing if not set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CurrentConfigSetName As String
            Get
                If _configurations Is Nothing OrElse _configurations.CurrentSet = "" Then
                    Return GetConfigProperty(ConstCPNUseConfigSetName, configsetname:=ConstGlobalConfigSetName)
                Else
                    Return _configurations.CurrentSet
                End If

            End Get
            Set(value As String)
                _configurations.CurrentSet = value
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the name of the current config file name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CurrentConfigFileName As String
            Get
                Return GetConfigProperty(ConstCPNConfigFileName, configsetname:=ConstGlobalConfigSetName)
            End Get
            Set(value As String)
                SetConfigProperty(ConstCPNUseConfigSetName, value:=value, configsetname:=ConstGlobalConfigSetName)
            End Set
        End Property
        ''' <summary>
        ''' Gets the configfilelocations.
        ''' </summary>
        ''' <value>The configfilelocations.</value>
        Public ReadOnly Property ConfigFileLocations() As List(Of String)
            Get
                Return _configfilelocations
            End Get
        End Property
        ''' <summary>
        ''' gets the Object Class Repository
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassRepository As ObjectClassRepository
            Get
                Return _ObjectClassStore
            End Get
        End Property
        ''' <summary>
        ''' Property CurrentSession 
        ''' </summary>
        ''' <value></value>
        ''' <returns>the current session object</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CurrentSession As Session
            Get
                '* Init -> during bootstrapping startup it might be that _CurrentSession is set
                If _CurrentSession Is Nothing AndAlso Not IsInitialized Then
                    If Not Initialize() Then
                        Return Nothing
                    End If
                End If

                Return _CurrentSession
            End Get

        End Property

        ''' <summary>
        ''' Gets the primary DB env.
        ''' </summary>
        ''' <value>The primary DB env.</value>
        Public ReadOnly Property CurrentDBDriver() As iormDatabaseDriver
            Get
                If IsInitialized OrElse Initialize() Then
                    Return CurrentSession.CurrentDBDriver
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' returns the otdb errorlog or nothing
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Errorlog As SessionMessageLog
            Get

                If IsInitialized OrElse Initialize() Then
                    Return CurrentSession.Errorlog
                Else
                    Return Nothing
                End If
            End Get
        End Property
        ReadOnly Property DBConnectionString As String
            Get

                If CurrentConnection(autoConnect:=False) Is Nothing Then
                    Return ""
                Else
                    Return CurrentConnection(autoConnect:=False).Connectionstring
                End If
            End Get
        End Property
        ReadOnly Property LoginWindow As CoreLoginForm
            Get
                If CurrentConnection(autoConnect:=False) Is Nothing Then
                    Return Nothing
                Else
                    Return CurrentConnection(autoConnect:=False).UILogin
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the OTD bis initialized.
        ''' </summary>
        ''' <value>The OTD bis initialized.</value>
        Public Property IsInitialized() As Boolean
            Get
                Return _OTDBIsInitialized
            End Get
            Friend Set(value As Boolean)
                _OTDBIsInitialized = value
            End Set
        End Property
        ''' <summary>
        ''' returns an IEnumerable of all Object Class Descriptions
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassDescriptions As IEnumerable(Of ObjectClassDescription)
            Get
                Return _ObjectClassStore.ObjectClassDescriptions()
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the O TDB connection.
        ''' </summary>
        ''' <value>The O TDB connection.</value>
        ReadOnly Property CurrentConnection(Optional autoConnect As Boolean = True, _
        Optional accessRequest As otAccessRight = ConstDefaultAccessRight, _
        Optional username As String = "", _
        Optional password As String = "") As OnTrack.Database.iormConnection
            Get
                '* Init
                If Not IsInitialized Then
                    If Not Initialize() Then
                        Return Nothing
                    End If
                End If

                ' ** select the Connection
                If Not CurrentSession.CurrentDBDriver Is Nothing AndAlso Not CurrentSession.CurrentDBDriver.CurrentConnection Is Nothing Then
                    Return CurrentSession.CurrentDBDriver.CurrentConnection
                Else
                    Call CoreMessageHandler(showmsgbox:=True, subname:="CurrentConnection", noOtdbAvailable:=True, message:="Connection is not set before Connect")
                    Return Nothing
                End If

                '* connect ?!
                If autoConnect = True Then
                    If CurrentSession.StartUp(AccessRequest:=accessRequest, OTDBUsername:=username, OTDBPassword:=password) Then
                        Return CurrentSession.CurrentDBDriver.CurrentConnection
                    ElseIf autoConnect = False Then
                        Return CurrentSession.CurrentDBDriver.CurrentConnection
                    Else
                        Return Nothing
                    End If
                End If

                Return CurrentSession.CurrentDBDriver.CurrentConnection
            End Get

        End Property
        ''' <summary>
        ''' return True if the Current Connection exists to the database
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsConnected As Boolean
            Get
                If CurrentConnection(autoConnect:=False) Is Nothing Then
                    Return False
                Else
                    Return CurrentConnection(autoConnect:=False).IsConnected
                End If
            End Get

        End Property
        ''' <summary>
        ''' gets the used location for the config file location
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UsedConfigFileLocation As String
            Get
                Return _UsedConfigFileLocation
            End Get
        End Property
        ''' <summary>
        ''' returns Current Username in the current connection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Username As String
            Get
                If Not CurrentSession.IsRunning Then
                    Return ""
                Else
                    Return CurrentSession.OTdbUser.Username
                End If
            End Get

        End Property
        ''' <summary>
        ''' retuns a list of Installed OnTrack Modules
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property InstalledModules As String()
            Get
                If IsInitialized OrElse Initialize() Then
                    Return _ObjectClassStore.GetModulenames().ToArray()
                End If
            End Get
        End Property
        ''' <summary>
        ''' returns the bootstrap schema Version
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SchemaVersion() As ULong
            Get
                Return ConstOTDBSchemaVersion
            End Get

        End Property
        ''' <summary>
        ''' returns a list of selectable config set names without global
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ConfigSetNamesToSelect As List(Of String)
            Get
                Return _configurations.SetNames.FindAll(Function(x) x <> ConstGlobalConfigSetName)
            End Get
        End Property
        ''' <summary>
        ''' returns a list of ConfigSetnames
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ConfigSetNames As List(Of String)
            Get
                Return _configurations.SetNames
            End Get
        End Property
#End Region

        '****
        '**** addConfigFilePath add a file path to the locations to look into
        Public Sub AddConfigFilePath(path As String)
            If path <> "" AndAlso Not _configfilelocations.Contains(path) Then _configfilelocations.Add(path)
        End Sub
        ''' <summary>
        ''' reads the config parameters from the configfile
        ''' </summary>
        ''' <param name="configFilePath">path where to read config file</param>
        ''' <param name="configFileName">name of the config file to read</param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Private Function ReadConfigFile(Optional ByVal configFilePath As String = "", Optional ByVal configFileName As String = "") As Boolean
            Dim readData As String
            Dim found As Boolean
            Dim reader As StreamReader
            Dim splitAttributes As Object
            Dim valueString As String
            Dim valueObject As Object
            Dim identifier As String
            Dim parameterName As String
            Dim configsetname As String = ConstGlobalConfigSetName
            Dim driver As String = "primary"
            Dim sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary
            Dim weight As UShort = 15

            '** get the config file name
            If configFileName = "" Then
                If HasConfigProperty(ConstCPNConfigFileName) Then
                    configFileName = GetConfigProperty(ConstCPNConfigFileName)
                End If
                If configFileName = "" Then
                    configFileName = My.Settings.DefaultConfigFileName
                End If
                If configFileName = "" Then
                    Call CoreMessageHandler(subname:="modCore.GetConfigFromFile", _
                                            message:="no config file defined", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            '*

            found = False
            ' check the configfilepath first
            If configFilePath <> "" Then
                If Mid(configFilePath, Len(configFilePath), 1) <> "\" Then configFilePath = configFilePath & "\"

                If File.Exists(configFilePath & configFileName) Then
                    found = True
                End If
            End If
            '** still not found
            If Not found Then
                ' than the other paths
                For i = ConfigFileLocations.Count - 1 To 0 Step -1
                    Dim path = ConfigFileLocations.ElementAt(i)
                    If path <> "" Then
                        If Mid(path, Len(path), 1) <> "\" Then path = path & "\"
                        If File.Exists(path & configFileName) Then
                            configFilePath = path
                            found = True
                            Exit For
                        End If
                    End If
                Next
            End If
            '** still nothing
            If Not found Then
                Return False
            End If

            '* if not containskey ?!
            'FileOpen(1, ConfigFilePath & ConfigFileName, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)

            reader = New StreamReader(configFilePath & configFileName)

            _UsedConfigFileLocation = configFilePath ' remember

            Try

                Do
                    readData = reader.ReadLine
                    valueString = ""
                    valueObject = Nothing

                    '** comment
                    If Regex.IsMatch(readData, "^\s*[;|\*|//|/*|-]") Then
                        identifier = ""
                        '*** Configuration Name Section
                    ElseIf Regex.IsMatch(readData, "\[\s*(?<name>\w.*\w)\s*\]") Then
                        Dim match As Match = Regex.Match(readData, "\[\s*(?<name>\w.*\w)\s*\]")
                        valueString = match.Groups("name").Value
                        If Regex.IsMatch(valueString, "\:") Then
                            Dim matchconfig As Match = Regex.Match(valueString, "(?<name>.*)\s*\:\s*(?<driver>.*)")
                            configsetname = matchconfig.Groups("name").Value
                            driver = matchconfig.Groups("driver").Value
                            Select Case driver.ToLower
                                Case "primary", "0", ComplexPropertyStore.Sequence.Primary.ToString.ToLower
                                    sequence = ComplexPropertyStore.Sequence.Primary
                                Case "secondary", "1", ComplexPropertyStore.Sequence.Secondary.ToString.ToLower
                                    sequence = ComplexPropertyStore.Sequence.Secondary
                                Case Else
                                    sequence = ComplexPropertyStore.Sequence.Primary
                                    CoreMessageHandler(message:="driver sequence not recognized - primary assumed", arg1:=driver, subname:="ReadConfigFile", messagetype:=otCoreMessageType.InternalError)
                            End Select

                        Else
                            configsetname = valueString
                            sequence = ComplexPropertyStore.Sequence.Primary
                        End If
                        identifier = ""
                        '* parameter
                    ElseIf Regex.IsMatch(readData, "^\s*(?<name>.+)\s*[\=]\s*(?<value>.*)") Then
                        Dim match As Match = Regex.Match(readData, "^\s*(?<name>.+)\s*[\=]\s*(?<value>.*)")
                        identifier = Trim(match.Groups("name").Value)
                        valueString = Trim(match.Groups("value").Value)
                        parameterName = ""
                        '** select
                        Select Case identifier.ToLower
                            Case "use", "current", ConstCPNUseConfigSetName
                                'ot.CurrentConfigSetName = valueString this doesnot work since the Config set might not be loaded 
                                parameterName = ConstCPNUseConfigSetName
                            Case "defaultdomainid", constCPNDefaultDomainid.ToLower
                                parameterName = constCPNDefaultDomainid
                            Case "path", ConstCPNDBPath.ToLower
                                parameterName = ConstCPNDBPath
                            Case "name", ConstCPNDBName
                                parameterName = ConstCPNDBName
                            Case "logagent", constCPNUseLogAgent
                                parameterName = constCPNUseLogAgent
                                Select Case valueString.ToLower
                                    Case "true", "1"
                                        valueObject = True
                                    Case "false", "0"
                                        valueObject = False
                                    Case Else
                                        valueObject = 0
                                End Select
                            Case "usemars", ConstCPNDBSQLServerUseMars
                                parameterName = ConstCPNDBSQLServerUseMars
                                Select Case valueString.ToLower
                                    Case "true", "1"
                                        valueObject = True
                                    Case "false", "0"
                                        valueObject = False
                                    Case Else
                                        valueObject = 0
                                End Select
                            Case "user", ConstCPNDBUser
                                parameterName = ConstCPNDBUser
                            Case "description", ConstCPNDescription
                                parameterName = ConstCPNDescription
                            Case "password", ConstCPNDBPassword
                                parameterName = ConstCPNDBPassword
                            Case "connectionstring", ConstCPNDBConnection
                                parameterName = ConstCPNDBConnection
                            Case "database", ConstCPNDBType
                                parameterName = ConstCPNDBType
                                Select Case valueString.ToLower
                                    '** SQL SERVER
                                    Case ConstCPVDBTypeSqlServer, otDBServerType.SQLServer.ToString.ToLower
                                        valueObject = otDBServerType.SQLServer
                                        '** set the default parameter
                                        If Not ot.HasConfigProperty(constCPNUseLogAgent, configsetname:=configsetname) Then
                                            '*** yes to SessionLog
                                            ot.SetConfigProperty(constCPNUseLogAgent, configsetname:=configsetname, value:=True, weight:=20)
                                        End If
                                        If Not ot.HasConfigProperty(ConstCPNDriverName, configsetname:=configsetname) Then
                                            '*** no to SessionLog
                                            ot.SetConfigProperty(ConstCPNDriverName, configsetname:=configsetname, value:=otDbDriverType.ADONETSQL, weight:=20)
                                        End If

                                        '** ACCESS
                                    Case ConstCPVDBTypeAccess, otDBServerType.Access.ToString.ToLower
                                        valueObject = otDBServerType.Access
                                        '** set the default parameter
                                        If Not ot.HasConfigProperty(constCPNUseLogAgent, configsetname:=configsetname) Then
                                            '*** no to SessionLog
                                            ot.SetConfigProperty(constCPNUseLogAgent, configsetname:=configsetname, value:=False, weight:=20)
                                        End If
                                        If Not ot.HasConfigProperty(ConstCPNDriverName, configsetname:=configsetname) Then
                                            '*** no to SessionLog
                                            ot.SetConfigProperty(ConstCPNDriverName, configsetname:=configsetname, value:=otDbDriverType.ADONETOLEDB, weight:=20)
                                        End If

                                    Case Else
                                        valueObject = 0
                                End Select
                            Case "drivername", ConstCPNDriverName
                                parameterName = ConstCPNDriverName
                                Select Case valueString.ToLower
                                    '** OLEDB
                                    Case ConstCPVDriverOleDB, otDbDriverType.ADONETOLEDB.ToString.ToLower
                                        valueObject = otDbDriverType.ADONETOLEDB
                                        '** SQL
                                    Case ConstCPVDriverMSSQL, otDbDriverType.ADONETSQL.ToString.ToLower
                                        valueObject = otDbDriverType.ADONETSQL
                                        '** set the default parameter
                                    Case Else
                                        valueObject = 0
                                End Select

                            Case ""
                                parameterName = ""
                            Case Else
                                CoreMessageHandler(message:="the config file parameter was not recognized", arg1:=identifier, messagetype:=otCoreMessageType.ApplicationError, _
                                                   subname:="ot.GetConfigFromFile")
                                parameterName = ""
                        End Select

                        '** set the value to the found parametername
                        '** high value for the UseConfigSetName
                        If parameterName = ConstCPNUseConfigSetName And configsetname = ConstGlobalConfigSetName Then
                            weight = 99 ' must be the same value as in ot.currentconfigset
                        Else
                            weight = 15
                        End If
                        If parameterName <> "" AndAlso valueObject Is Nothing Then
                            SetConfigProperty(name:=parameterName, weight:=weight, value:=valueString, configsetname:=configsetname, sequence:=sequence)
                        ElseIf parameterName <> "" AndAlso valueObject IsNot Nothing Then
                            SetConfigProperty(name:=parameterName, weight:=weight, value:=valueObject, configsetname:=configsetname, sequence:=sequence)
                        End If

                        valueString = ""
                        valueObject = Nothing
                    End If


                Loop Until reader.Peek = -1

                Call CoreMessageHandler(message:=" config file '" & configFilePath & configFileName & "' read from file system", _
                                        subname:="modOTDB.getConfigFromFile", messagetype:=otCoreMessageType.InternalInfo)

                Return True


            Catch ex As Exception
                reader.Close()
                Call CoreMessageHandler(subname:="modCore.GetConfigFromFile", message:="couldnot read config file ", arg1:=configFileName, _
                                        exception:=ex, messagetype:=otCoreMessageType.ApplicationError)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns the config set for a configsetname with a driversequence
        ''' </summary>
        ''' <param name="configsetname"></param>
        ''' <param name="driverseq"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetConfigSet(configsetname As String, Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Dictionary(Of String, SortedList(Of UShort, Object))
            Return _configurations.GetSet(configsetname, sequence:=sequence)
        End Function
        ''' <summary>
        ''' returns true if the named configset has the config property
        ''' </summary>
        ''' <param name="configsetname"></param>
        ''' <param name="driverseq"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigSetProperty(propertyname As String, _
                                             Optional configsetname As String = Nothing, _
                                             Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Boolean
            Return _configurations.HasProperty(name:=propertyname, setname:=configsetname, sequence:=sequence)
        End Function
        ''' <summary>
        ''' sets a Property to the TableStore
        ''' </summary>
        ''' <param name="Name">Name of the Property</param>
        ''' <param name="Object">ObjectValue</param>
        ''' <returns>returns True if succesfull</returns>
        ''' <remarks></remarks>
        Public Function SetConfigProperty(ByVal name As String, ByVal value As Object, _
                                            Optional ByVal weight As UShort = 0,
                                            Optional configsetname As String = "", _
                                            Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Boolean
            Return _configurations.SetProperty(name:=name, value:=value, weight:=weight, setname:=configsetname, sequence:=sequence)
        End Function
        ''' <summary>
        ''' Gets the Property of a config set. if configsetname is ommitted then check currentconfigset and the global one
        ''' </summary>
        ''' <param name="name">name of property</param>
        ''' <returns>object of the property</returns>
        ''' <remarks></remarks>
        Public Function GetConfigProperty(ByVal name As String, Optional weight As UShort = 0, _
        Optional configsetname As String = "", _
        Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Object
            Return _configurations.GetProperty(name:=name, weight:=weight, setname:=configsetname, sequence:=sequence)
        End Function


        ''' <summary>
        ''' returns true if the config-set name exists 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasConfigSetName(ByVal configsetname As String, Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Boolean
            Return _configurations.HasSet(setname:=configsetname, sequence:=sequence)
        End Function
        ''' <summary>
        ''' has the config set the named property
        ''' </summary>
        ''' <param name="name">name of property</param>
        ''' <returns>return true</returns>
        ''' <remarks></remarks>
        Public Function HasConfigProperty(ByVal name As String, Optional configsetname As String = "", Optional sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary) As Boolean
            Return _configurations.HasProperty(name:=name, setname:=configsetname, sequence:=sequence)
        End Function
        ''' <summary>
        ''' retrieve the Config parameters of OnTrack and write it to the PropertyBag
        ''' </summary>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Public Function RetrieveConfigProperties(Optional force As Boolean = False) As Boolean

            Dim value As Object

            '** donot do it multiple times
            If _configPropertiesRead And Not force Then
                Return True
            End If
            '** default config set 
            SetConfigProperty(ConstCPNUseConfigSetName, weight:=10, value:=ConstGlobalConfigSetName)

            '** get the driver
            If Not My.Settings.Properties.Item(ConstCPNDriverName) Is Nothing Then
                value = My.Settings.Properties.Item(ConstCPNDriverName).DefaultValue
                If value <> "" Then
                    SetConfigProperty(ConstCPNDriverName, weight:=10, value:=value, configsetname:=ConstGlobalConfigSetName)
                End If
            End If

            ' add config path the local path of the assembly
            Dim uri As System.Uri
            uri = New System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            AddConfigFilePath(System.IO.Path.GetDirectoryName(uri.LocalPath))
            AddConfigFilePath(System.IO.Path.GetDirectoryName(uri.LocalPath) & "\Resources")

            value = My.Settings.Default.DefaultConfigFileName
            If String.IsNullOrWhiteSpace(value) Then
                value = ConstDefaultConfigFileName
            End If
            SetConfigProperty(ConstCPNConfigFileName, weight:=10, value:=value, configsetname:=ConstGlobalConfigSetName)


            '*** read the config file
            If Not String.IsNullOrWhiteSpace(value) Then

                '** is it an ini
                If Not value.Contains(".") Then
                    value &= ".ini"
                End If

                '** read
                If ReadConfigFile(configFileName:=value) Then
                    RetrieveConfigProperties = True
                End If
            End If

            '** set the default for the log agent
            SetConfigProperty(constCPNUseLogAgent, weight:=10, value:=False)

            _configPropertiesRead = True
            Return RetrieveConfigProperties

        End Function

        ''' <summary>
        ''' Retrieves a List of  ObjectClasses Descriptions referenced by a tableid
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescriptionByTable(tableid As String) As List(Of ObjectClassDescription)
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectClassDescriptionsByTable(tablename:=tableid)
            End If
        End Function
        ''' <summary>
        ''' Retrieves the schema table attribute by name
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetTableAttribute(tableid As String) As ormSchemaTableAttribute
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetTableAttribute(tablename:=tableid)
            End If
        End Function
        ''' <summary>
        ''' Retrieves the ObjectClasses as system.type referenced by a tableid
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassByTable(tableid As String) As List(Of System.Type)
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectClassesForTable(tablename:=tableid)
            End If
        End Function
        ''' <summary>
        ''' returns a SchemaTableAttriute for tablename from the core repisotory
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSchemaTableAttribute(tablename As String) As ormSchemaTableAttribute
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetTableAttribute(tablename:=tablename.ToUpper)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns the names of the bootstrapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapObjectClassDescriptions() As List(Of ObjectClassDescription)
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetBootStrapObjectClassDescriptions()
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns the names of the bootstrapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapObjectClassIDs() As List(Of String)
            If _bootstrapObjectIds.Count = 0 Then
                For Each aClassDescription In GetBootStrapObjectClassDescriptions()
                    _bootstrapObjectIds.Add(aClassDescription.ID)
                Next
            End If

            Return _bootstrapObjectIds
        End Function
        ''' <summary>
        ''' returns the names of the bootstrapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapObjectClassnames() As List(Of String)
            If _bootstrapclassnames.Count = 0 Then
                For Each aClassDescription In GetBootStrapObjectClassDescriptions()
                    _bootstrapclassnames.Add(aClassDescription.ObjectAttribute.ClassName)
                Next
            End If

            Return _bootstrapclassnames
        End Function
        ''' <summary>
        ''' returns the object class description for a type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectClassDescription(type As Type) As ObjectClassDescription
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectClassDescription(typename:=type.FullName)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' Returns a List of ObjectClassDescriptions per Modulename
        ''' </summary>
        ''' <param name="modulename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetObjectClassDescriptionsForModule(modulename As String) As List(Of ObjectClassDescription)
            If IsInitialized OrElse Initialize() Then
                If _ObjectClassStore.GetModulenames.Contains(modulename.ToUpper) Then
                    Return _ObjectClassStore.GetObjectClassDescriptions(modulename)
                Else
                    CoreMessageHandler(message:="Module name does not exist in Object Class Repository", arg1:=modulename.ToUpper, _
                                        subname:="ot.GetObjectClassDescriptionsForModule", messagetype:=otCoreMessageType.InternalError)
                    Return New List(Of ObjectClassDescription)
                End If

            Else
                Return New List(Of ObjectClassDescription)
            End If
        End Function
        ''' <summary>
        ''' returns a List of Boot strapping tables
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetBootStrapTableNames() As List(Of String)
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetBootStrapTableNames
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns a method hook for a class
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMethodInfo(typename As String, methodname As String) As MethodInfo
            If IsInitialized OrElse Initialize() Then
                Dim anDescriptor = _ObjectClassStore.GetObjectClassDescription(typename:=typename)
                If anDescriptor IsNot Nothing Then Return MethodInfo.GetMethodFromHandle(anDescriptor.GetMethodInfoHook(name:=methodname))
                Return Nothing
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns a method hook for a class
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetMethodInfo([type] As Type, methodname As String) As MethodInfo
            If IsInitialized OrElse Initialize() Then
                Dim anDescriptor = _ObjectClassStore.GetObjectClassDescription([type])
                If anDescriptor IsNot Nothing Then Return MethodInfo.GetMethodFromHandle(anDescriptor.GetMethodInfoHook(name:=methodname))
                Return Nothing
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' Creates an instance of an data object by type
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDataObjectInstance(type As Type) As iormPersistable
            Return _ObjectClassStore.CreateInstance(type:=type)
        End Function
        ''' <summary>
        ''' returns the type of the business object class if any
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassType(objectname As String) As System.Type
            Dim aType = _ObjectClassStore.GetObjectClassType(objectname:=objectname)
            '** this was not the classname ?! - try the ID
            If aType Is Nothing Then
                Dim aDescription = GetObjectClassDescriptionByID(id:=objectname)
                If aDescription IsNot Nothing Then
                    Return aDescription.Type
                Else
                    Return Nothing
                End If
            End If

            Return aType
        End Function
        ''' <summary>
        ''' returns a objectEntry Attribute for entryname and objectname from the core repisotory
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectEntryAttribute(entryname As String, objectname As String) As ormObjectEntryAttribute
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectEntryAttribute(entryname:=entryname, objectname:=objectname)
            End If
        End Function
        ''' <summary>
        ''' returns the bootstrap schema Version
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetBootStrapSchemaChecksum() As ULong
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.BootstrapSchemaChecksum
            End If
        End Function
        ''' <summary>
        ''' returns a SchemaColumnAttribute for columnname and tablename from the core repisotory
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <param name="tablename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSchemaTableColumnAttribute(columnname As String, tablename As String) As ormObjectEntryAttribute
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetSchemaColumnAttribute(columnname:=columnname.ToUpper, tablename:=tablename.ToUpper)
            End If
        End Function
        ''' <summary>
        ''' returns the ObjectClassDescription for an type name
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescription(typename As String) As ObjectClassDescription
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectClassDescription(typename:=typename)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns the ObjectClassDescription for an objectid
        ''' </summary>
        ''' <param name="objectname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObjectClassDescriptionByID(id As String) As ObjectClassDescription
            If IsInitialized OrElse Initialize() Then
                Return _ObjectClassStore.GetObjectClassDescriptionByID(id:=id)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Initialize the OTDB Envirormenent
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Initialize(Optional ByVal force As Boolean = False) As Boolean


            Try

                If Not IsInitialized Or force Then

                    '** Add the CORE UI Mappings as per Default
                    If Not UserInterface.HasNativeUI(UserInterface.LoginFormName) Then
                        UserInterface.RegisterNativeUI(UserInterface.LoginFormName, GetType(UIWinFormLogin))
                        UserInterface.RegisterNativeUI(UserInterface.MessageboxFormName, GetType(UIWinFormMessageBox))
                    End If

                    ''' register all data objects which have a direct orm mapping
                    If _ObjectClassStore.Initialize(force:=True) Then
                        Call CoreMessageHandler(showmsgbox:=False, message:=_ObjectClassStore.Count & " object class descriptions collected and setup", _
                                             noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalInfo, _
                                            subname:="Initialize")
                    End If

                    '***** Request a Session -> now we have a session log
                    _CurrentSession = New Session(_configurations)

                    '***
                    Dim ipproperties As Net.NetworkInformation.IPGlobalProperties = Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                    Dim strHostName As String
                    Dim strIPAddress As String
                    strHostName = ipproperties.HostName
                    If ipproperties.DomainName <> "" Then strHostName &= "." & ipproperties.DomainName
                    strIPAddress = System.Net.Dns.GetHostByName(strHostName).AddressList(0).ToString()

                    Dim message As String = My.Application.Info.AssemblyName & " started in version " & My.Application.Info.Version.ToString _
                    & " loaded from " & My.Application.Info.DirectoryPath & " on system " & My.Computer.Name
                    If My.Computer.Network.IsAvailable Then
                        message &= String.Format(" ({0}, {1}) ", strHostName, strIPAddress)
                    Else
                        message &= " ( standalone ) "
                    End If

                    message &= "with culture" & My.Computer.Info.InstalledUICulture.DisplayName & ")" _
                    & " running : " & My.Computer.Info.OSFullName

                    '** message
                    Call CoreMessageHandler(showmsgbox:=False, message:=message, _
                                            noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalInfo, _
                                            subname:="Initialize")

                    ''' set intiialized
                    IsInitialized = True

                    ''' refresh change log after initialized since changelog is a ormrelationCollection
                    ''' 
                    If _changelog.Refresh(type:=GetType(ormChangeLogEntry)) Then
                        Call CoreMessageHandler(showmsgbox:=False, message:=_ObjectClassStore.Count & " object class descriptions collected and setup", _
                                            noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalInfo, _
                                           subname:="Initialize")
                    End If
                End If

                Return IsInitialized

            Catch ex As Exception

                Call CoreMessageHandler(subname:="modOTDB.Initialize", exception:=ex)
                IsInitialized = False
                Return False
            End Try


        End Function

        '**********
        '********** getDBParameter: get a Parameter from the OTDB
        '**********
        ''' <summary>
        ''' retrieve a DB Parameter from Ontrack from the central core module
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Function GetDBParameter(name As String, Optional silent As Boolean = False) As Object
            Dim result As Object

            '*** initialized ?!
            If Not IsInitialized AndAlso Not Initialize() Then
                Call CoreMessageHandler(noOtdbAvailable:=False, message:="Initialize of database envirorment failed", _
                                        subname:="GetDBParameter", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            '*** result
            result = CurrentSession.CurrentDBDriver.GetDBParameter(parametername:=name, silent:=silent)
            Return result

        End Function

        '**********
        '********** setDBParameter: set a Parameter to the OTDB
        '**********
        ''' <summary>
        ''' sets a DB Parameter (in the DB) from the central core module
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="updateOnly"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetDBParameter(ByVal name As String, ByVal value As Object, _
        Optional ByVal updateOnly As Boolean = False, _
        Optional ByVal silent As Boolean = False) As Boolean
            '*** initialized ?!
            If Not IsInitialized AndAlso Not Initialize() Then
                Call CoreMessageHandler(noOtdbAvailable:=False, message:="Initialize of database envirorment failed", _
                                        subname:="SetDBParameter", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            '***
            Return CurrentSession.CurrentDBDriver.SetDBParameter(parametername:=name, silent:=silent, value:=value, updateOnly:=updateOnly)

        End Function

        '*******
        '******* getTableClass : returns a new or existing clsOTDBTableObject from the Collection
        ''' <summary>
        ''' returns a TableStore for the id from the central OnTrack Core Module
        ''' </summary>
        ''' <param name="tableid"></param>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetTableStore(tableid As String, Optional ByVal force As Boolean = False) As iormDataStore

            '*** initialized ?!
            If Not IsInitialized AndAlso Not Initialize() Then
                Call CoreMessageHandler(noOtdbAvailable:=False, message:="Initialize of database envirorment failed", _
                                            messagetype:=otCoreMessageType.InternalError, subname:="GetTableStore")
                Return Nothing
            End If

            '*** get tablestore if connected or bootstrapping
            If Not CurrentSession.CurrentDBDriver.CurrentConnection Is Nothing AndAlso _
                (CurrentSession.CurrentDBDriver.CurrentConnection.IsConnected OrElse CurrentSession.IsBootstrappingInstallationRequested) Then
                Return CurrentSession.CurrentDBDriver.GetTableStore(tableID:=tableid, force:=force)
            Else
                Call CoreMessageHandler(noOtdbAvailable:=False, message:="Primary connection failed to be connected", _
                                        messagetype:=otCoreMessageType.InternalError, subname:="GetTableStore")
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' requires access to the OnTrack Database  - starts a session if not running otherwise just validates
        ''' </summary>
        ''' <param name="AccessRequest">otAccessRight</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function RequireAccess(accessRequest As otAccessRight, _
                                            Optional domainID As String = Nothing, _
                                            Optional reLogin As Boolean = True) As Boolean
            Return CurrentSession.RequireAccessRight(accessRequest:=accessRequest, domainID:=domainID, reLogin:=reLogin)
        End Function

        ''' <summary>
        ''' requires access to the OnTrack Database  - starts a session if not running otherwise just validates
        ''' </summary>
        ''' <param name="AccessRequest">otAccessRight</param>
        ''' <returns>True if successfull</returns>
        ''' <remarks></remarks>
        Public Function Startup(accessRequest As otAccessRight, _
                                            Optional domainID As String = Nothing, _
                                            Optional messagetext As String = "") As Boolean

            '*** startup
            If Not CurrentSession.IsRunning AndAlso Not CurrentSession.IsStartingUp Then
                Return CurrentSession.StartUp(AccessRequest:=accessRequest, domainID:=domainID, messagetext:=messagetext)
            Else
                Return RequireAccess(accessRequest:=accessRequest, domainID:=domainID)
            End If
        End Function

        ''' <summary>
        ''' Add Error Message to the ErrorLog of the Current Session
        ''' </summary>
        ''' <param name="otdberror">clsOTDBError object</param>
        ''' <returns>true if successful</returns>
        ''' <remarks></remarks>
        Function AddErrorToLog(ByRef otdberror As SessionMessage) As Boolean

            '** use _currentsession -> do not initialize log should be always there
            If Not _CurrentSession Is Nothing Then
                _CurrentSession.Errorlog.Enqueue(otdberror)
                Return True
            Else
                Return False
            End If
        End Function
        '********
        '******** getLastError
        ''' <summary>
        ''' returns the most recent error from error log or nothing
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetLastError() As SessionMessage

            If Not _CurrentSession Is Nothing Then
                Return _CurrentSession.Errorlog.PeekLast
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' returns the Standard Default Values for a datatype of OnTrack Datatypes
        ''' </summary>
        ''' <param name="datatype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDefaultValue(datatype As otDataType) As Object

            Select Case datatype
                Case otDataType.Bool
                    Return False
                Case otDataType.Date
                    Return constNullDate
                Case otDataType.List
                    ''' To do implement inner Type
                    ''' or accept Object()
                    Dim aValue As New List(Of String)
                    Return aValue.ToArray
                Case otDataType.Long
                    Return 0
                Case otDataType.Memo
                    Return ""
                Case otDataType.Numeric
                    Return 0
                Case otDataType.Text
                    Return ""
                Case otDataType.Time
                    Return ConstNullTime
                Case otDataType.Timestamp
                    Return constNullDate
                Case Else
                    CoreMessageHandler(message:="datatype must be implemented", messagetype:=otCoreMessageType.InternalError, subname:="DefaultValue")
                    Return Nothing
            End Select

        End Function
        ''' <summary>
        ''' Mapping of otdb Datatypes to native .NET Datatypes
        ''' </summary>
        ''' <param name="datatype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDatatypeMappingOf(datatype As otDataType) As System.Type

            Select Case datatype
                Case otDataType.Date
                    Return GetType(DateTime)
                Case otDataType.Bool
                    Return GetType(Boolean)
                Case otDataType.List
                    Return GetType(List(Of String))
                Case otDataType.Long
                    Return GetType(Long)
                Case otDataType.Memo
                    Return GetType(String)
                Case otDataType.Text
                    Return GetType(String)
                Case otDataType.Time
                    Return GetType(DateTime)
                Case otDataType.Numeric
                    Return GetType(Double)
                Case otDataType.Timestamp
                    Return GetType(DateTime)
                Case Else
                    CoreMessageHandler(message:="Mapping for datatype must be implemented", arg1:=datatype, subname:="DataTypeMapping", messagetype:=otCoreMessageType.InternalError)
                    Throw New NotImplementedException("mapping in DatatypeMapping is not implemented")
            End Select
        End Function
        ''' <summary>
        ''' central error handler .. all messages and error conditions are fed here
        ''' </summary>
        ''' <param name="SHOWMSGBOX"></param>
        ''' <param name="EXCEPTION"></param>
        ''' <param name="ARG1"></param>
        ''' <param name="SUBNAME"></param>
        ''' <param name="TABLENAME"></param>
        ''' <param name="ENTRYNAME"></param>
        ''' <param name="message"></param>
        ''' <param name="break"></param>
        ''' <param name="NoOTDBAvailable"></param>
        ''' <param name="messagetype"></param>
        ''' <param name="MSGLOG"></param>
        ''' <remarks></remarks>
        Public Sub CoreMessageHandler(Optional ByVal message As String = "", _
                                        Optional ByVal exception As Exception = Nothing, _
                                        Optional ByVal arg1 As Object = Nothing, _
                                        Optional ByVal subname As String = "", _
                                        Optional ByVal tablename As String = "", _
                                        Optional ByVal columnname As String = "", _
                                        Optional ByVal objectname As String = "", _
                                        Optional ByVal entryname As String = "", _
                                        Optional ByVal showmsgbox As Boolean = False, _
                                        Optional ByVal break As Boolean = False, _
                                        Optional ByVal noOtdbAvailable As Boolean = False, _
                                        Optional ByVal messagetype As otCoreMessageType = otCoreMessageType.ApplicationError, _
                                        Optional ByRef msglog As ObjectMessageLog = Nothing, _
                                        Optional ByVal username As String = "", _
                                        Optional ByVal tagvalues As Object = Nothing, _
                                        Optional ByVal domainid As String = "", _
                                        Optional ByVal dataobject As iormPersistable = Nothing)
            '<CallerMemberName> Optional memberName As String = Nothing, _
            '   <CallerFilePath> Optional sourcefilePath As String = Nothing, _
            '  <CallerLineNumber()> Optional sourceLineNumber As Integer = 0)
            Dim exmessagetext As String = ""
            Dim routinestack As String = ""
            Dim aNewError As New SessionMessage
            Dim tagvaluestring As String
            Try


                ''' EXCEPTION HANDLING
                ''' 
                If exception IsNot Nothing Then
                    messagetype = otCoreMessageType.InternalException
                    '** build the extended exception message
                    exmessagetext &= vbLf & "Exception of " & exception.GetType.ToString
                    exmessagetext &= vbLf & " --> " & exception.Message
                    exmessagetext &= vbLf & "Source: " & exception.Source

                    If exception.InnerException IsNot Nothing Then
                        exmessagetext &= vbLf & "Inner Exception --> " & exception.InnerException.ToString
                    End If

                    If TypeOf exception Is SqlException Then
                        Dim sqlexcept As SqlException = TryCast(exception, SqlException)
                        If sqlexcept IsNot Nothing Then
                            exmessagetext &= vbLf & "Errorcode:" & sqlexcept.ErrorCode
                            exmessagetext &= vbLf & "Errors:" & sqlexcept.Errors.ToString
                            exmessagetext &= vbLf & "LineNumber:" & sqlexcept.LineNumber
                            exmessagetext &= vbLf & "Server:" & sqlexcept.Server
                        End If
                    ElseIf TypeOf exception Is OleDbException Then
                        Dim oleexcept As OleDbException = TryCast(exception, OleDbException)
                        If oleexcept IsNot Nothing Then
                            exmessagetext &= vbLf & "Errorcode:" & oleexcept.ErrorCode
                            exmessagetext &= vbLf & "Errors:" & oleexcept.Errors.ToString
                        End If

                    End If

                    routinestack &= exception.StackTrace


                End If

                '*** dataobject default values
                '***
                If dataobject IsNot Nothing Then
                    If String.IsNullOrWhiteSpace(objectname) Then objectname = dataobject.ObjectID
                    If String.IsNullOrWhiteSpace(tablename) Then tablename = dataobject.primaryTableID
                    If tagvalues Is Nothing Then tagvalues = dataobject.ObjectPrimaryKeyValues
                    If String.IsNullOrWhiteSpace(domainid) AndAlso dataobject.ObjectHasDomainBehavior Then domainid = dataobject.DomainID
                End If

                '**** add to the Connection.errorlog
                '****
                With aNewError
                    .Message = message & vbLf
                    .Message &= exmessagetext
                    If msglog IsNot Nothing Then .Message &= vbLf & msglog.MessageText
                    .Subname = subname
                    .Exception = exception
                    .messagetype = messagetype
                    .StackTrace = routinestack

                    '.Arguments = arg1
                    If arg1 IsNot Nothing And Not IsArray(arg1) Then
                        .Arguments = arg1.ToString
                    Else
                        .Arguments = ""
                    End If


                    '* object tag values
                    If tagvalues IsNot Nothing Then
                        If tagvalues.GetType.IsArray Then
                            tagvaluestring = Converter.Array2otString(tagvalues)
                        Else
                            tagvaluestring = CStr(tagvalues)
                        End If
                    Else
                        tagvaluestring = String.Empty
                    End If

                    .Objectname = objectname
                    .ObjectEntry = entryname
                    .Objecttag = tagvaluestring
                    .Tablename = tablename
                    .Columnname = columnname
                    .Timestamp = Date.Now
                    If Not String.IsNullOrWhiteSpace(domainid) Then
                        .Domainid = domainid
                    ElseIf _CurrentSession IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(_CurrentSession.CurrentDomainID) Then
                        .Domainid = _CurrentSession.CurrentDomainID
                    End If
                    If String.IsNullOrWhiteSpace(username) AndAlso _CurrentSession IsNot Nothing AndAlso _CurrentSession.IsRunning Then 'use the internal variable not to startup a session
                        .Username = _CurrentSession.Username
                    Else
                        .Username = username
                    End If
                End With

                ''' Add to Log for flushing later
                ''' 
                AddErrorToLog(aNewError)


                ''' Diagnostic Log output
                ''' 

                System.Diagnostics.Debug.WriteLine(Date.Now)

                Select Case (messagetype)
                    Case otCoreMessageType.ApplicationInfo
                        System.Diagnostics.Debug.WriteLine("> Type: INFO")
                    Case otCoreMessageType.ApplicationError
                        System.Diagnostics.Debug.WriteLine("> Type: ERROR")
                    Case otCoreMessageType.ApplicationWarning
                        System.Diagnostics.Debug.WriteLine("> Type: WARNING")
                    Case otCoreMessageType.InternalException
                        System.Diagnostics.Debug.WriteLine("> Type: Exception")
                    Case otCoreMessageType.InternalInfo
                        System.Diagnostics.Debug.WriteLine("> Type: Internal INFORMATION")
                    Case otCoreMessageType.InternalError
                        System.Diagnostics.Debug.WriteLine("> Type: Internal ERROR")
                    Case otCoreMessageType.InternalWarning
                        System.Diagnostics.Debug.WriteLine("> Type: Internal Warning")
                    Case otCoreMessageType.InternalException
                        System.Diagnostics.Debug.WriteLine("> Type: Internal Exception")
                End Select

                System.Diagnostics.Debug.WriteLine("> OnTrack Session Message:" & message)
                If msglog IsNot Nothing Then System.Diagnostics.Debug.WriteLine(">> Object Message Log :" & msglog.MessageText)
                If arg1 IsNot Nothing Then System.Diagnostics.Debug.WriteLine("> Arguments:" & arg1.ToString)
                If tagvaluestring IsNot Nothing Then System.Diagnostics.Debug.WriteLine("> Object Tag:" & tagvaluestring)
                If tablename IsNot Nothing AndAlso tablename <> "" Then System.Diagnostics.Debug.WriteLine("> Tablename: " & tablename)
                If columnname IsNot Nothing AndAlso columnname <> "" Then System.Diagnostics.Debug.WriteLine("> Columnname: " & columnname)
                If objectname IsNot Nothing AndAlso objectname <> "" Then System.Diagnostics.Debug.WriteLine("> Objectname: " & objectname)
                If entryname IsNot Nothing AndAlso entryname <> "" Then System.Diagnostics.Debug.WriteLine("> Entry: " & entryname)
                If subname IsNot Nothing AndAlso subname <> "" Then System.Diagnostics.Debug.WriteLine("> Routine:" & CStr(subname))
                If exmessagetext <> "" Then System.Diagnostics.Debug.WriteLine("> Exception Message:" & exmessagetext)
                If routinestack <> "" Then System.Diagnostics.Debug.WriteLine("> Stack:" & routinestack)


                '''
                ''' Messagebox Handling
                '''
                If showmsgbox Then
                    With New CoreMessageBox
                        '* Message Heaxder
                        Select Case messagetype
                            Case otCoreMessageType.ApplicationError
                                .Title = "ERROR"
                                .type = CoreMessageBox.MessageType.Error
                            Case otCoreMessageType.ApplicationInfo
                                .Title = "INFO"
                                .type = CoreMessageBox.MessageType.Info
                            Case otCoreMessageType.ApplicationWarning
                                .Title = "WARNING"
                                .type = CoreMessageBox.MessageType.Warning
                            Case otCoreMessageType.ApplicationException
                                .Title = "EXCEPTION"
                                .type = CoreMessageBox.MessageType.Error
                            Case otCoreMessageType.InternalInfo
                                .Title = "INTERNAL INFO"
                                .type = CoreMessageBox.MessageType.Info
                            Case otCoreMessageType.InternalError
                                .Title = "INTERNAL ERROR"
                                .type = CoreMessageBox.MessageType.Error
                            Case otCoreMessageType.InternalException
                                .Title = exception.GetType.ToString & " INTERNAL EXCEPTION FROM " & exception.Source
                                .type = CoreMessageBox.MessageType.Error
                            Case otCoreMessageType.InternalWarning
                                .Title = "INTERNAL WARNING"
                                .type = CoreMessageBox.MessageType.Warning
                        End Select
                        .Title &= " from " & subname
                        '* Message
                        .Message = "Message: " & message
                        If arg1 IsNot Nothing Then .Message &= vbLf & "Argument:" & arg1
                        If objectname IsNot Nothing AndAlso objectname <> "" Then .Message &= vbLf & "Object: " & objectname
                        If entryname IsNot Nothing AndAlso entryname <> "" Then .Message &= vbLf & "Entry: " & entryname
                        If tablename IsNot Nothing AndAlso tablename <> "" Then .Message &= vbLf & "Table: " & tablename
                        If columnname IsNot Nothing AndAlso columnname <> "" Then .Message &= vbLf & "Column: " & columnname
                        If subname IsNot Nothing AndAlso subname <> "" Then .Message &= vbLf & "Routine: " & CStr(subname)
                        .Message &= vbLf & exmessagetext


                        .buttons = CoreMessageBox.ButtonType.OK
                        .Show()
                    End With

                End If

                ' break
                If messagetype <> otCoreMessageType.ApplicationInfo And messagetype <> otCoreMessageType.InternalInfo Then
                    Debug.Assert(Not break)
                End If

            Catch ex As Exception
                Debug.WriteLine("{0} Exception raised in CoreMessageHandler", Date.Now)
                Debug.WriteLine("{0}", ex.Message)
                Debug.WriteLine("{0}", ex.StackTrace)
                Debug.Assert(False)
            End Try
        End Sub

    End Module


End Namespace