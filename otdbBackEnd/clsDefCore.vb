Option Explicit On

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE DEFINITION Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Diagnostics.Debug

Imports OnTrack
Imports OnTrack.Database

Namespace OnTrack


    ''' <summary>
    ''' Domain Setting Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DomainSetting
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** const
        <ormSchemaTableAttribute(adddeletefieldbehavior:=True, Version:=1)> Public Const ConstTableID As String = "tblDefDomainSettings"

        <ormSchemaColumnAttribute(id:="DMS1", _
            typeid:=otFieldDataType.Text, size:=50, _
            title:="domain", Description:="domain identifier", _
            primaryKeyordinal:=1)> _
        Const ConstFNDomainID As String = Domain.ConstFNDomainID

        <ormSchemaColumnAttribute(ID:="DMS2", _
           typeid:=otFieldDataType.Text, size:=100, primaryKeyordinal:=2, _
           title:="Setting", description:="ID of the setting per domain")> _
        Const ConstFNSettingID = "id"

        <ormSchemaColumnAttribute(ID:="DMS3", _
            typeid:=otFieldDataType.Text, size:=100, _
            title:="Description")> _
        Const ConstFNDescription = "desc"

        <ormSchemaColumnAttribute(ID:="DMS4", _
           typeid:=otFieldDataType.Text, size:=255, _
           title:="value", description:="value of the domain setting in string presentation")> _
        Const ConstFNValue = "value"

        <ormSchemaColumnAttribute(ID:="DMS5", _
          typeid:=otFieldDataType.Long, _
          title:="datatype", description:="datatype of the domain setting value")> _
        Const ConstFNDatatype = "datatype"

        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNDomainID)> Private _DomainID As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNSettingID)> Private _ID As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDescription)> Private _description As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNValue)> Private _valuestring As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDatatype)> Private _datatype As otFieldDataType = 0
        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' gets the ID of the Domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DomainID() As String
            Get
                DomainID = _DomainID
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property
        ''' <summary>
        ''' Description of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the datatype 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype As otFieldDataType
            Set(value As otFieldDataType)
                _datatype = value
            End Set
            Get
                Return _datatype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the value of the domain setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property value As Object
            Set(value As Object)
                If value Is Nothing Then
                    _valuestring = ""
                Else
                    _valuestring = value.ToString
                End If
            End Set
            Get
                Try
                    Select Case _datatype
                        Case otFieldDataType.Binary
                            Return CBool(_valuestring)
                        Case otFieldDataType.Date, otFieldDataType.Time, otFieldDataType.Timestamp
                            If _valuestring Is Nothing Then Return ConstNullDate
                            If IsDate(_valuestring) Then Return CDate(_valuestring)
                            If _valuestring = ConstNullDate.ToString OrElse _valuestring = ConstNullTime.ToString Then Return ConstNullDate
                            If _valuestring = "" Then Return ConstNullDate
                        Case otFieldDataType.List, otFieldDataType.Memo, otFieldDataType.Text
                            If _valuestring Is Nothing Then Return ""
                            Return CStr(_valuestring)
                        Case otFieldDataType.Numeric
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                        Case otFieldDataType.Long
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                            Return CLng(_valuestring)
                        Case Else
                            CoreMessageHandler(message:="data type not covered: " & _datatype, arg1:=_valuestring, subname:="DomainSetting.value", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                    End Select
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, message:="could not convert value to data type " & _datatype, _
                                       arg1:=_valuestring, subname:="DomainSetting.value", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End Try

            End Get
        End Property
#End Region

        ''' <summary>
        ''' initialize the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function

        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal domainID As String, ByVal id As String, Optional forcereload As Boolean = False) As DomainSetting
            Dim pkarray() As Object = {UCase(domainID), UCase(id)}
            Return Retrieve(Of DomainSetting)(pkArray:=pkarray, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveByDomain(ByVal domainID As String, Optional forcereload As Boolean = False) As List(Of DomainSetting)
            Dim aParameterslist As New List(Of ormSqlCommandParameter)
            aParameterslist.Add(New ormSqlCommandParameter(ID:="@id", fieldname:=ConstFNDomainID, tablename:=ConstTableID, value:=domainID))

            Dim aList As List(Of DomainSetting) = ormDataObject.All(Of DomainSetting)(ID:="allbyDomain", _
                                                                                      where:="[" & ConstFNDomainID & "] = @id", _
                                                                                      parameters:=aParameterslist)
            Return aList
        End Function
        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal domainID As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(domainID)), UCase(id)}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of DomainSetting)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal domainID As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(domainID), UCase(id)}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                _DomainID = UCase(domainID)
                _ID = UCase(id)
                Return True
            Else
                Return False
            End If
        End Function

    End Class
    ''' <summary>
    ''' User Definition Class of an OnTrack User
    ''' </summary>
    ''' <remarks></remarks>
    Public Class User
        Inherits ormDataObject
        Implements iormCloneable
        Implements iormInfusable

        <ormSchemaTable(version:=2, adddeletefieldbehavior:=True)> Public Const ConstTableID As String = "tblDefUsers"

        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=50, primarykeyordinal:=1, _
          ID:="U1", title:="username", description:="name of the OnTrack user")> _
        Public Const ConstFNUsername = "username"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=20, _
           ID:="U2", title:="password", description:="password of the OnTrack user")> _
        Public Const ConstFNPassword = "password"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=50, _
          ID:="U3", title:="group", description:="group of the OnTrack user")> _
        Public Const ConstFNgroup = "group"

        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=0, relation:={"p1"}, _
         ID:="U4", title:="person", description:="person name of the OnTrack user")> _
        Public Const ConstFNPerson = "person"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=255, _
        ID:="U5", title:="description", description:="description of the OnTrack user")> _
        Public Const ConstFNDescription = "desc"
        <ormSchemaColumn(typeid:=otFieldDataType.Bool, _
            ID:="U6", title:="is anonymous", description:="is user an anonymous user")> _
        Public Const ConstFNIsAnonymous = "isanon"
        <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=50, _
            ID:="U7", title:="Default Workspace", description:="default workspace of the OnTrack user")> _
        Public Const ConstFNDefaultWorkspace = "defws"

        <ormSchemaColumn(typeid:=otFieldDataType.Bool, _
           ID:="UR1", title:="Alter Schema Right", description:="has user the right to alter the database schema")> _
        Public Const ConstFNAlterSchema = "alterschema"
        <ormSchemaColumn(typeid:=otFieldDataType.Bool, _
          ID:="UR2", title:="Update Data Right", description:="has user the right to update data (new/change/delete)")> _
        Public Const ConstFNUpdateData = "updatedata"
        <ormSchemaColumn(typeid:=otFieldDataType.Bool, _
          ID:="UR3", title:="Read Data Right", description:="has user the right to read the database data")> _
        Public Const ConstFNReadData = "readdata"
        <ormSchemaColumn(typeid:=otFieldDataType.Bool, _
          ID:="UR4", title:="No Access", description:="has user no access")> _
        Public Const ConstFNNoAccess = "noright"


        'fields
        <ormColumnMapping(Fieldname:=ConstFNUsername)> Private _username As String
        <ormColumnMapping(Fieldname:=ConstFNPassword)> Private _password As String
        <ormColumnMapping(Fieldname:=ConstFNDescription)> Private _desc As String
        <ormColumnMapping(Fieldname:=ConstFNgroup)> Private _group As String
        <ormColumnMapping(Fieldname:=ConstFNPerson)> Private _personID As String

        <ormColumnMapping(Fieldname:=ConstFNDefaultWorkspace)> Private _DefaultWorkspace As String
        <ormColumnMapping(Fieldname:=ConstFNIsAnonymous)> Private _isAnonymous As Boolean
        <ormColumnMapping(Fieldname:=ConstFNReadData)> Private _hasRead As Boolean
        <ormColumnMapping(Fieldname:=ConstFNUpdateData)> Private _hasUpdate As Boolean
        <ormColumnMapping(Fieldname:=ConstFNAlterSchema)> Private _hasAlterSchema As Boolean
        <ormColumnMapping(Fieldname:=ConstFNNoAccess)> Private _hasNoRights As Boolean

        ' dynamics
        Private _settings As New Dictionary(Of String, UserSetting)
        Private _SettingsLoaded As Boolean = False

        ''' <summary>
        ''' initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            Initialize = MyBase.Initialize()
            _settings.Clear()
            _SettingsLoaded = False
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
        End Function
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(constTableID)
        End Sub

#Region "Properties"



        Public Property Description() As String
            Get
                Description = _desc
            End Get
            Set(ByVal avalue As String)
                _desc = avalue
                Me.IsChanged = True
            End Set
        End Property

        Public Property Group() As String
            Get
                Group = _group
            End Get
            Set(ByVal value As String)
                _group = value
                IsChanged = True
            End Set
        End Property
        Public Property DefaultWorkspaceID As String

            Get
                DefaultWorkspaceID = _DefaultWorkspace
            End Get
            Set(value As String)
                _DefaultWorkspace = value
                IsChanged = True
            End Set
        End Property

        Public Property Password() As String
            Get
                Password = _password
            End Get
            Set(value As String)
                _password = value
                IsChanged = True
            End Set
        End Property

        Public Property PersonName() As String
            Get
                PersonName = _personID
            End Get
            Set(value As String)
                _personID = value
                IsChanged = True
            End Set
        End Property

        ReadOnly Property Username() As String
            Get
                Username = _username
            End Get
        End Property
        ''' <summary>
        ''' has no rights at all ?! -> Blocked ?!
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasNoRights() As Boolean
            Get
                HasNoRights = _hasNoRights
            End Get
            Set(value As Boolean)
                _hasNoRights = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' has right to read
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasReadRights() As Boolean
            Get
                HasReadRights = _hasRead
            End Get
            Set(value As Boolean)
                _hasRead = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' has right to update and read data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasUpdateRights() As Boolean
            Get
                HasUpdateRights = _hasUpdate
            End Get
            Set(value As Boolean)
                _hasUpdate = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Has Right to update, read and alter schema data
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property HasAlterSchemaRights() As Boolean
            Get
                HasAlterSchemaRights = _hasAlterSchema
            End Get
            Set(value As Boolean)
                _hasAlterSchema = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' is anonymous user
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsAnonymous() As Boolean
            Get
                IsAnonymous = _isAnonymous
            End Get
            Set(value As Boolean)
                _isAnonymous = value
                IsChanged = True
            End Set
        End Property
#End Region
        ''' <summary>
        ''' returns a SQL String to insert the Admin User in the table -> bootstrap
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetInsertInitalUserSQLString(username As String, person As String, password As String, desc As String, group As String, defaultworkspace As String) As String

            Dim aSqlString As String = String.Format("INSERT INTO {0} ", ConstTableID)
            aSqlString &= "( [username], person, [password], [desc], [group], defws, isanon, alterschema, readdata, updatedata, noright, UpdatedOn, CreatedOn)"
            aSqlString &= String.Format("VALUES ('{0}','{1}', '{2}', '{3}', '{4}', '{5}', 0, 1,1,1,0, '{6}','{7}' )", _
                                        Username, person, password, desc, group, defaultworkspace, Date.Now.ToString("yyyyMMdd hh:mm:ss"), Date.Now.ToString("yyyyMMdd hh:mm:ss"))
            Return aSqlString

        End Function

        ''' <summary>
        ''' returns a SQL String to create the table on bootstrapping
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetCreateSqlString() As String

            Dim aSqlString As String = String.Format("CREATE TABLE {0} ", ConstTableID)
            aSqlString &= String.Format("( [{0}] nvarchar(50) not null, [{1}] nvarchar(50) not null, [{2}] nvarchar(50) not null, [{3}] nvarchar(50) not null, ", ConstFNUsername, ConstFNPassword, ConstFNPerson, ConstFNgroup)
            aSqlString &= String.Format("[{0}] nvarchar(max) not null default '', [{1}] bit not null default 0, [{2}] bit not null default 0, [{3}] bit not null default 0, [{4}] bit not null default 0, ", _
                                        ConstFNDefaultWorkspace, ConstFNAlterSchema, ConstFNUpdateData, ConstFNReadData, ConstFNNoAccess)
            aSqlString &= String.Format(" [{0}] nvarchar(max) not null default '', [{1}] DATETIME not null , [{2}] Datetime not null , " & _
                                                "CONSTRAINT [tblDefUsers_primarykey] PRIMARY KEY NONCLUSTERED ([{3} Asc) ", _
                                                ConstFNDescription, ConstFNUpdatedOn, ConstFNCreatedOn, ConstFNUsername)
            aSqlString &= "WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];"

            Return aSqlString
            '*** LEGACY working
            'Return "create table " & Me.TableID & _
            '                 " ( username nvarchar(50) not null, [password] nvarchar(50) not null, [person] nvarchar(50) not null, [group] nvarchar(50) not null, " & _
            '                 "defws nvarchar(max) not null default '', " & _
            '                 "isanon bit not null default 0, alterschema bit not null default 0, updatedata bit not null default 0, noright bit not null default 0," & _
            '                 "readdata bit not null default 1," & _
            '                 " [desc] nvarchar(max) not null default '', UpdatedOn DATETIME not null , CreatedOn Datetime not null , " & _
            '                 "CONSTRAINT [tblDefUsers_primarykey] PRIMARY KEY NONCLUSTERED ([username] Asc) " & _
            '                 "WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" & _
            '                 ") ON [PRIMARY];"
        End Function
        ''' <summary>
        ''' Returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of User)
            Return ormDataObject.All(Of User)(orderby:=ConstFNUsername)
        End Function

        '****** getAnonymous: "static" function to return the first Anonymous user
        '******
        ''' <summary>
        ''' returns the anonyous user ( first descending username)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAnonymous() As OnTrack.User
            Dim aObjectCollection As List(Of User)
            If CurrentSession.CurrentDBDriver.DatabaseType = otDBServerType.SQLServer Then
                aObjectCollection = ormDataObject.All(Of User)(orderby:=ConstFNUsername, where:=ConstFNIsAnonymous & "=1")
            Else
                aObjectCollection = ormDataObject.All(Of User)(orderby:=ConstFNUsername, where:=ConstFNIsAnonymous & "=true")
            End If

            If aObjectCollection.Count = 0 Then
                Return Nothing
            Else
                Return aObjectCollection.Item(1)
            End If

        End Function


        ''' <summary>
        ''' loads and infuses a User Definition object by primary key
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal username As String) As Boolean
            Dim primarykey() As Object = {username}
            Return MyBase.LoadBy(primarykey)
        End Function

        ''' <summary>
        ''' Retrieve a User Definition
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal username As String, Optional forcereload As Boolean = False) As User
            Return Retrieve(Of User)(pkArray:={username}, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' returns true if the setting exists
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasSetting(id As String) As Boolean
            LoadSettings() ' load since we might no have it during bootstrap
            Return _settings.ContainsKey(key:=id)
        End Function
        ''' <summary>
        ''' returns the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSetting(id As String) As Object
            LoadSettings()
            If Me.HasSetting(id:=id) Then
                Return _settings.Item(key:=id)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' sets the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSetting(id As String, datatype As otFieldDataType, value As Object) As Boolean
            Dim aSetting As New UserSetting
            LoadSettings()
            If Me.HasSetting(id:=id) Then
                aSetting = Me.GetSetting(id:=id)
            Else
                If Not aSetting.Create(Username:=Me.Username, id:=id) Then
                    aSetting = UserSetting.Retrieve(Username:=Me.Username, id:=id)
                End If
            End If

            If aSetting Is Nothing OrElse Not (aSetting.IsLoaded Or aSetting.IsCreated) Then
                Return False
            End If
            aSetting.Datatype = datatype
            aSetting.Value = value

            If Not Me.HasSetting(id:=id) Then _settings.Add(key:=id, value:=aSetting)
            Return True
        End Function
        ''' <summary>
        ''' Load the settings to the settings dictionary
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadSettings(Optional force As Boolean = False) As Boolean

            If _SettingsLoaded And Not force Then Return True

            Dim aListDomain As List(Of UserSetting) = UserSetting.RetrieveByUsername(Username:=Me.Username)

            '** overwrite
            For Each aSetting In aListDomain
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next

            _SettingsLoaded = False
            Return True
        End Function
        ''' <summary>
        ''' Persist the data object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <param name="ForceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate) As Boolean
            Try
                If Not FeedRecord() Then
                    Persist = False
                    Exit Function
                End If

                For Each aSetting In _settings.Values
                    aSetting.Persist()
                Next

                Persist = MyBase.Persist(timestamp)
                Exit Function

            Catch ex As Exception
                Call CoreMessageHandler(subname:="User.Persist", exception:=ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' create the persistency schema with use of database driver
        ''' ATTENTION ! This can only be called if database is set up
        ''' user createSql function otherwise
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of User)(silent:=silent)
        End Function

        ''' <summary>
        ''' Create persistency for this object
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal username As String) As Boolean
            Dim primarykey() As Object = {username}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                _username = username
                Return True
            Else
                Return False
            End If
        End Function

    End Class
    ''' <summary>
    ''' User Setting Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class UserSetting
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** const
        <ormSchemaTableAttribute(adddeletefieldbehavior:=True, Version:=1)> Public Const ConstTableID As String = "tblDefUserSettings"

        <ormSchemaColumnAttribute(id:="US1", _
            typeid:=otFieldDataType.Text, size:=50, _
            title:="Username", Description:="name of the OnTrack user", _
            primaryKeyordinal:=1)> _
        Const ConstFNUsername As String = User.ConstFNUsername

        <ormSchemaColumnAttribute(ID:="US2", _
           typeid:=otFieldDataType.Text, size:=100, primaryKeyordinal:=2, _
           title:="Setting", description:="ID of the setting per user")> _
        Const ConstFNSettingID = "id"

        <ormSchemaColumnAttribute(ID:="US3", _
            typeid:=otFieldDataType.Text, size:=100, _
            title:="Description")> _
        Const ConstFNDescription = "desc"

        <ormSchemaColumnAttribute(ID:="US4", _
           typeid:=otFieldDataType.Text, size:=255, _
           title:="value", description:="value of the user setting in string presentation")> _
        Const ConstFNValue = "value"

        <ormSchemaColumnAttribute(ID:="US5", _
          typeid:=otFieldDataType.Long, _
          title:="datatype", description:="datatype of the user setting value")> _
        Const ConstFNDatatype = "datatype"

        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNUsername)> Private _Username As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNSettingID)> Private _ID As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDescription)> Private _description As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNValue)> Private _valuestring As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDatatype)> Private _datatype As otFieldDataType = 0
        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        ''' <summary>
        ''' gets the ID of the Domain
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Username() As String
            Get
                Username = _Username
            End Get

        End Property
        ''' <summary>
        ''' gets the ID of the Setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property
        ''' <summary>
        ''' Description of the setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' returns the datatype 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype As otFieldDataType
            Set(value As otFieldDataType)
                _datatype = value
            End Set
            Get
                Return _datatype
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the value of the domain setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As Object
            Set(value As Object)
                If value Is Nothing Then
                    _valuestring = ""
                Else
                    _valuestring = value.ToString
                End If
            End Set
            Get
                Try
                    Select Case _datatype
                        Case otFieldDataType.Binary
                            Return CBool(_valuestring)
                        Case otFieldDataType.Date, otFieldDataType.Time, otFieldDataType.Timestamp
                            If _valuestring Is Nothing Then Return ConstNullDate
                            If IsDate(_valuestring) Then Return CDate(_valuestring)
                            If _valuestring = ConstNullDate.ToString OrElse _valuestring = ConstNullTime.ToString Then Return ConstNullDate
                            If _valuestring = "" Then Return ConstNullDate
                        Case otFieldDataType.List, otFieldDataType.Memo, otFieldDataType.Text
                            If _valuestring Is Nothing Then Return ""
                            Return CStr(_valuestring)
                        Case otFieldDataType.Numeric
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                        Case otFieldDataType.Long
                            If IsNumeric(_valuestring) Then
                                Return CDbl(_valuestring)
                            Else
                                Return CDbl(0)
                            End If
                            Return CLng(_valuestring)
                        Case Else
                            CoreMessageHandler(message:="data type not covered: " & _datatype, arg1:=_valuestring, subname:="DomainSetting.value", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                    End Select
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, message:="could not convert value to data type " & _datatype, _
                                       arg1:=_valuestring, subname:="DomainSetting.value", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End Try

            End Get
        End Property
#End Region

        ''' <summary>
        ''' initialize the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function

        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal Username As String, ByVal id As String, Optional forcereload As Boolean = False) As UserSetting
            Dim pkarray() As Object = {UCase(Username), UCase(id)}
            Return Retrieve(Of UserSetting)(pkArray:=pkarray, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function RetrieveByUsername(ByVal Username As String, Optional forcereload As Boolean = False) As List(Of UserSetting)
            Dim aParameterslist As New List(Of ormSqlCommandParameter)
            aParameterslist.Add(New ormSqlCommandParameter(ID:="@Username", fieldname:=ConstFNUsername, value:=Username))

            Dim aList As List(Of UserSetting) = ormDataObject.All(Of UserSetting)(ID:="allby" & Username, where:=ConstFNUsername & "= @Username", _
                                                                                      parameters:=aParameterslist)
            Return aList
        End Function
        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal Username As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(Username)), UCase(id)}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of UserSetting)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal Username As String, ByVal id As String) As Boolean
            Dim primarykey() As Object = {UCase(Username), UCase(id)}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                _Username = UCase(Username)
                _ID = UCase(id)
                Return True
            Else
                Return False
            End If
        End Function

    End Class


    '************************************************************************************
    '***** CLASS clsOTDBDefPerson describes additional database schema information
    '*****

    Public Class clsOTDBDefPerson
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Public Const constTableID As String = "tblDefPersons"

        ' fields
        Private s_name As String = ""
        Private s_description As String = ""
        Private s_managername As String = ""
        Private s_orgunitID As String = ""
        Private s_emailaddy As String = ""

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(constTableID)
        End Sub

#Region "Properties"

        ReadOnly Property Name() As String
            Get
                Name = s_name
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                s_description = value
                IsChanged = True
            End Set
        End Property


        Public Property ManagerName() As String
            Get
                ManagerName = s_managername
            End Get
            Set(value As String)
                s_managername = value
                IsChanged = True
            End Set
        End Property


        Public Property OrgUnitID() As String
            Get
                OrgUnitID = s_orgunitID
            End Get
            Set(value As String)
                s_orgunitID = value
                IsChanged = True
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize()
        End Function
        ''' <summary>
        ''' Infuses the person definition by record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            Try
                s_name = CStr(record.GetValue("name"))
                s_description = CStr(record.GetValue("desc"))
                s_managername = CStr(record.GetValue("managername"))
                s_orgunitID = CStr(record.GetValue("orgunitid"))
                s_emailaddy = CStr(record.GetValue("emailaddy"))

                Infuse = MyBase.Infuse(record)
                _IsLoaded = Infuse
                Exit Function
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefPerson.Infuse")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal name As String, Optional forcereload As Boolean = False) As clsOTDBDefPerson
            Dim primarykey() As Object = {name}
            Return Retrieve(Of clsOTDBDefPerson)(pkArray:=primarykey, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Load and infuses a object by primary key
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal name As String) As Boolean
            Dim primarykey() As Object = {name}
            Return MyBase.LoadBy(pkArray:=primarykey)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTableDef As New ObjectDefinition
            Dim IDColumnNames As New Collection

            With aTableDef
                .Create(constTableID)
                .Delete()

                aFieldDesc.Tablename = constTableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""

                '***
                '*** Fields
                '****

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "name"
                aFieldDesc.ID = "p1"
                aFieldDesc.ColumnName = "name"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ID = "p2"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "organisation unit id"
                aFieldDesc.ID = "p3"
                aFieldDesc.ColumnName = "orgunitid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "manager name"
                aFieldDesc.ID = "p4"
                aFieldDesc.ColumnName = "managername"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "email address"
                aFieldDesc.ID = "p5"
                aFieldDesc.ColumnName = "emailaddress"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
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
        ''' Persist the Person Defintion
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean


            Try
                Call Me.Record.SetValue("name", s_name)
                Call Me.Record.SetValue("desc", s_description)
                Call Me.Record.SetValue("orgunitid", s_orgunitID)
                Call Me.Record.SetValue("managername", s_managername)
                Call Me.Record.SetValue("emailaddy", s_emailaddy)

                Return MyBase.Persist(timestamp)
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefperson.persist")
                Return False
            End Try



        End Function
        ''' <summary>
        ''' returns a collection of all Person Definition Objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of clsOTDBDefPerson)
            Return ormDataObject.All(Of clsOTDBDefPerson)()
        End Function
        ''' <summary>
        ''' Creates the persistence object
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal name As String) As Boolean
            Dim primarykey() As Object = {name}
            ' set the primaryKey
            If MyBase.Create(primarykey, checkUnique:=True) Then
                s_name = name
                Return True
            Else
                Return False
            End If


        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefLogMessage describes an Error or Info Message
    '*****
    Public Class clsOTDBDefLogMessage
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Const _tableID As String = "tblDefLogMessages"

        ' fields
        Private s_id As Long
        Private s_weight As Integer
        Private s_area As String = ""
        Private s_typeid As otAppLogMessageType
        Private s_message As String = ""
        Private s_status1 As String = ""
        Private s_statustype1 As String = ""
        Private s_status2 As String = ""
        Private s_statustype2 As String = ""
        Private s_status3 As String = ""
        Private s_statustype3 As String = ""
        Private s_desc As String = ""


        ''' <summary>
        ''' constructor of a Message Definition
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(_tableID)
        End Sub


        ReadOnly Property ID() As Long
            Get
                ID = s_id
            End Get
        End Property

        Public Property Message() As String
            Get
                Message = s_message
            End Get
            Set(value As String)
                s_message = value
                IsChanged = True
            End Set
        End Property


        Public Property Weight() As Integer
            Get
                Weight = s_weight
            End Get
            Set(avalue As Integer)
                If s_weight <> avalue Then
                    s_weight = avalue
                    IsChanged = True
                End If
            End Set
        End Property


        Public Property TypeID() As otAppLogMessageType
            Get
                TypeID = s_typeid
            End Get
            Set(avalue As otAppLogMessageType)
                If s_typeid <> avalue Then
                    s_typeid = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Area() As String
            Get
                Area = s_area
            End Get
            Set(ByVal avalue As String)
                If s_area <> avalue Then
                    s_area = avalue
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode1() As String
            Get
                Statuscode1 = s_status1
            End Get
            Set(avalue As String)
                If s_status1 <> LCase(avalue) Then
                    s_status1 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statuscode2() As String
            Get
                Statuscode2 = s_status2
            End Get
            Set(avalue As String)
                If s_status2 <> LCase(avalue) Then
                    s_status2 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statuscode3() As String
            Get
                Statuscode3 = s_status3
            End Get
            Set(avalue As String)
                If s_status3 <> LCase(avalue) Then
                    s_status3 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Statustype1() As String
            Get
                Statustype1 = s_statustype1
            End Get
            Set(avalue As String)
                If s_statustype1 <> LCase(avalue) Then
                    s_statustype1 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statustype2() As String
            Get
                Statustype2 = s_statustype2
            End Get
            Set(avalue As String)
                If s_statustype2 <> LCase(avalue) Then
                    s_statustype2 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property
        Public Property Statustype3() As String
            Get
                Statustype3 = s_statustype3
            End Get
            Set(avalue As String)
                If s_statustype3 <> LCase(avalue) Then
                    s_statustype3 = LCase(avalue)
                    IsChanged = True
                End If
            End Set
        End Property
        Public Function GetStatusCodeOf(ByVal typeid As String) As String
            If Not _IsLoaded And Not Me.IsCreated Then
                GetStatusCodeOf = ""
                Exit Function
            End If

            If LCase(typeid) = Me.Statustype1 Then
                GetStatusCodeOf = Me.Statuscode1
                Exit Function
            ElseIf LCase(typeid) = Me.Statustype2 Then
                GetStatusCodeOf = Me.Statuscode2
                Exit Function
            ElseIf LCase(typeid) = Me.Statustype3 Then
                GetStatusCodeOf = Me.Statuscode2
                Exit Function
            Else
                GetStatusCodeOf = ""
                Exit Function
            End If
        End Function

        Public Function GetMessageTypeID(typeid As String) As otAppLogMessageType
            Select Case LCase(typeid)
                Case LCase(OTDBConst_MessageTypeid_error)
                    GetMessageTypeID = otAppLogMessageType.[Error]
                Case LCase(OTDBConst_MessageTypeid_info)
                    GetMessageTypeID = otAppLogMessageType.Info
                Case LCase(OTDBConst_MessageTypeid_attention)
                    GetMessageTypeID = otAppLogMessageType.Attention
                Case LCase(OTDBConst_MessageTypeid_warning)
                    GetMessageTypeID = otAppLogMessageType.Warning
                Case Else
                    GetMessageTypeID = 0
            End Select
        End Function
        Public Function GetMessageTypeName(typeid As otAppLogMessageType) As String
            Select Case typeid
                Case otAppLogMessageType.[Error]
                    GetMessageTypeName = OTDBConst_MessageTypeid_error
                Case otAppLogMessageType.Info
                    GetMessageTypeName = OTDBConst_MessageTypeid_info
                Case otAppLogMessageType.Attention
                    GetMessageTypeName = OTDBConst_MessageTypeid_attention
                Case otAppLogMessageType.Warning
                    GetMessageTypeName = OTDBConst_MessageTypeid_warning
            End Select
        End Function
        ''' <summary>
        ''' infuses the Log message definition by a record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse
            Dim aVAlue As Object

            '* init
            If Not IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try

                s_id = CLng(record.GetValue("id"))
                s_message = CStr(record.GetValue("msg"))
                s_area = CStr(record.GetValue("area"))
                s_weight = CInt(record.GetValue("weight"))
                aVAlue = record.GetValue("typeid")

                Select Case LCase(aVAlue)
                    Case LCase(OTDBConst_MessageTypeid_error)
                        s_typeid = otAppLogMessageType.[Error]
                    Case LCase(OTDBConst_MessageTypeid_info)
                        s_typeid = otAppLogMessageType.Info
                    Case LCase(OTDBConst_MessageTypeid_attention)
                        s_typeid = otAppLogMessageType.Attention
                    Case LCase(OTDBConst_MessageTypeid_warning)
                        s_typeid = otAppLogMessageType.Warning
                End Select

                If Not DBNull.Value.Equals(record.GetValue("scode1")) Then
                    s_status1 = CStr(record.GetValue("scode1"))
                Else
                    s_status1 = ""
                End If
                If Not DBNull.Value.Equals(record.GetValue("stype1")) Then
                    s_statustype1 = CStr(record.GetValue("stype1"))
                Else
                    s_statustype1 = ""
                End If

                If Not DBNull.Value.Equals(record.GetValue("scode2")) Then
                    s_status2 = CStr(record.GetValue("scode2"))
                Else
                    s_status2 = ""
                End If
                If Not DBNull.Value.Equals(record.GetValue("stype2")) Then
                    s_statustype2 = CStr(record.GetValue("stype2"))
                Else
                    s_statustype2 = ""
                End If

                If Not DBNull.Value.Equals(record.GetValue("scode3")) Then
                    s_status3 = CStr(record.GetValue("scode3"))
                Else
                    s_status3 = ""
                End If
                If Not DBNull.Value.Equals(record.GetValue("stype3")) Then
                    s_statustype3 = CStr(record.GetValue("stype3"))
                Else
                    s_statustype3 = ""
                End If

                If Not DBNull.Value.Equals(record.GetValue("desc")) Then
                    s_desc = CStr(record.GetValue("desc"))
                Else
                    s_desc = ""
                End If

                _IsLoaded = MyBase.Infuse(record)
                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefLogMessage.Infuse")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Load and Infuse the Log Message Definition from store
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal ID As String) As Boolean
            Dim aStore As iormDataStore
            Dim aRecord As ormRecord
            Dim primarykey() As Object = {ID}

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    LoadBy = False
                    Exit Function
                End If
            End If

            aStore = Me.TableStore
            ' try to load it from cache
            aRecord = loadFromCache(_tableID, primarykey)
            ' load it from database
            If aRecord Is Nothing Then
                'Set aStore = getTableClass(ourTableName)
                aRecord = aStore.GetRecordByPrimaryKey(primarykey)
            End If

            If aRecord Is Nothing Then
                Me.Unload()
                LoadBy = Me.IsLoaded
                Exit Function
            Else
                'me.record = aRecord
                _IsLoaded = Me.Infuse(Me.Record)
                Call AddToCache(_tableID, key:=primarykey, theOBJECT:=aRecord)
                LoadBy = Me.IsLoaded
                Exit Function
            End If


        End Function
        ''' <summary>
        ''' create the persitency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aTableDef As New ObjectDefinition

            With aTableDef
                .Create(_tableID)
                .Delete()

                aFieldDesc.Tablename = _tableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""
                aFieldDesc.Relation = New String() {}

                '***
                '*** Fields
                '****

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "message id"
                aFieldDesc.ID = "lm1"
                aFieldDesc.ColumnName = "id"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "area of message"
                aFieldDesc.ID = "lm2"
                aFieldDesc.ColumnName = "area"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Numeric
                aFieldDesc.Title = "weight of message"
                aFieldDesc.ID = "lm3"
                aFieldDesc.ColumnName = "weight"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "typeid of message"
                aFieldDesc.ID = "lm4"
                aFieldDesc.ColumnName = "typeid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "message"
                aFieldDesc.ID = "lm11"
                aFieldDesc.ColumnName = "msg"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Memo
                aFieldDesc.Title = "description"
                aFieldDesc.ID = "lm12"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                ' STATUS 1
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status code 1"
                aFieldDesc.ID = "lm5"
                aFieldDesc.Relation = New String() {"stat2"}
                aFieldDesc.ColumnName = "scode1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status type 1"
                aFieldDesc.ID = "lm6"
                aFieldDesc.Relation = New String() {"stat1"}
                aFieldDesc.ColumnName = "stype1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' STATUS 2
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status code 2"
                aFieldDesc.ID = "lm7"
                aFieldDesc.Relation = New String() {"stat2"}
                aFieldDesc.ColumnName = "scode2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status type 2"
                aFieldDesc.ID = "lm8"
                aFieldDesc.Relation = New String() {"stat1"}
                aFieldDesc.ColumnName = "stype2"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' STATUS 3
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status code 3"
                aFieldDesc.ID = "lm9"
                aFieldDesc.Relation = New String() {"stat2"}
                aFieldDesc.ColumnName = "scode3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "status type 3"
                aFieldDesc.ID = "lm10"
                aFieldDesc.Relation = New String() {"stat1"}
                aFieldDesc.ColumnName = "stype3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.Relation = Nothing
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.Relation = Nothing
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
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
        ''' Persist the Log Message Definition to the store
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Try
                Call Me.Record.SetValue("id", s_id)
                Call Me.Record.SetValue("msg", s_message)
                Call Me.Record.SetValue("area", s_area)
                Call Me.Record.SetValue("weight", s_weight)
                Select Case s_typeid
                    Case otAppLogMessageType.[Error]
                        Call Me.Record.SetValue("typeid", "ERROR")
                    Case otAppLogMessageType.Info
                        Call Me.Record.SetValue("typeid", "INFO")
                    Case otAppLogMessageType.Attention
                        Call Me.Record.SetValue("typeid", "ATTENTION")
                    Case otAppLogMessageType.Warning
                        Call Me.Record.SetValue("typeid", "WARNING")

                End Select
                Call Me.Record.SetValue("scode1", s_status1)
                Call Me.Record.SetValue("stype1", s_statustype1)
                Call Me.Record.SetValue("scode2", s_status2)
                Call Me.Record.SetValue("stype2", s_statustype2)
                Call Me.Record.SetValue("scode3", s_status3)
                Call Me.Record.SetValue("stype31", s_statustype3)
                Call Me.Record.SetValue("desc", s_desc)

                Return MyBase.Persist(timestamp)

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefLogMessage.persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' return all Log Message Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All() As List(Of clsOTDBDefLogMessage)
            Return ormDataObject.All(Of clsOTDBDefLogMessage)()
        End Function

        ''' <summary>
        ''' Create a persistable Log Message
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal id As String) As Boolean
            Dim primarykey() As Object = {id}
            ' set the primaryKey
            If MyBase.Create(primarykey, checkUnique:=True) Then
                s_id = id
                Return True
            Else
                Return False
            End If

        End Function
    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefStatusItem is the object for a OTDBRecord (which is the datastore)
    '*****       defines a Status for different typeids
    '*****
    Public Class clsOTDBDefStatusItem
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Public Const constTableID As String = "tblDefStatusItems"

        ' fields
        Private s_typeid As String = ""  ' Status Type
        Private s_code As String = ""  ' code

        Private s_name As String = ""
        Private s_description As String = ""
        Private s_kpicode As String = ""
        Private s_weight As Long

        Private s_bgcolor As Long
        Private s_kpibgcolor As Long

        Private s_fgcolor As Long
        Private s_kpifgcolor As Long

        Private s_endStatus As Boolean
        Private s_startStatus As Boolean
        Private s_intermediateStatus As Boolean

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(constTableID)
        End Sub


        ReadOnly Property TypeID() As String
            Get
                TypeID = s_typeid
            End Get

        End Property
        ReadOnly Property Code() As String
            Get
                Code = s_code
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                s_description = value
                IsChanged = True
            End Set
        End Property


        Public Property Name() As String
            Get
                Name = s_name
            End Get
            Set(value As String)
                s_name = value
                IsChanged = True
            End Set
        End Property

        Public Property KPICode() As String
            Get
                KPICode = s_kpicode
            End Get
            Set(value As String)
                If LCase(s_kpicode) <> LCase(value) Then
                    s_kpicode = LCase(value)
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property Weight() As Long
            Get
                Weight = s_weight
            End Get
            Set(value As Long)
                If value <> s_weight Then
                    s_weight = value
                    IsChanged = True
                End If
            End Set
        End Property

        Public Property IsStartStatus() As Boolean
            Get
                IsStartStatus = s_startStatus
            End Get
            Set(value As Boolean)
                s_startStatus = value
                IsChanged = True
            End Set
        End Property


        Public Property IsIntermediateStatus() As Boolean
            Get
                IsIntermediateStatus = s_intermediateStatus
            End Get
            Set(value As Boolean)
                s_intermediateStatus = value
                IsChanged = True
            End Set
        End Property


        Public Property IsEndStatus() As Boolean
            Get
                IsEndStatus = s_endStatus
            End Get
            Set(value As Boolean)
                s_endStatus = value
                IsChanged = True
            End Set
        End Property

        Public Property Formatbgcolor() As Long
            Get
                Formatbgcolor = s_bgcolor
            End Get
            Set(value As Long)
                s_bgcolor = value
                IsChanged = True
            End Set
        End Property



        Public Property Formatkpibgcolor() As Long
            Get
                Formatbgcolor = s_kpibgcolor
            End Get
            Set(value As Long)
                s_kpibgcolor = value
                IsChanged = True
            End Set
        End Property

        Public Property Formatfgcolor() As Long
            Get
                Formatfgcolor = s_fgcolor
            End Get
            Set(value As Long)
                s_fgcolor = value
                IsChanged = True
            End Set
        End Property


        Public Property Formatkpifgcolor() As Long
            Get
                Formatfgcolor = s_fgcolor
            End Get
            Set(value As Long)
                s_kpifgcolor = value
                IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' initialize
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean
            Cache.RegisterCacheFor(constTableID)
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' infuses a Definition of a Status Irem by record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try

                s_typeid = CStr(record.GetValue("typeid"))
                s_code = CStr(record.GetValue("code"))
                s_name = CStr(record.GetValue("name"))
                s_description = CStr(record.GetValue("desc"))
                s_kpicode = CStr(record.GetValue("kpicode"))

                s_endStatus = CBool(record.GetValue("isend"))
                s_startStatus = CBool(record.GetValue("isstart"))
                s_intermediateStatus = CBool(record.GetValue("isintermediate"))

                s_fgcolor = CLng(record.GetValue("fgcolor"))
                s_bgcolor = CLng(record.GetValue("bgcolor"))
                s_kpifgcolor = CLng(record.GetValue("kpifgcolor"))
                s_kpibgcolor = CLng(record.GetValue("kpibgcolor"))
                _IsLoaded = MyBase.Infuse(record)
                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefStatusItem.Infuse")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve([typeid] As String, code As String, Optional forcereload As Boolean = False) As clsOTDBDefStatusItem
            Dim pkarry() As Object = {LCase([typeid]), LCase(code)}
            Return Retrieve(Of clsOTDBDefStatusItem)(pkArray:=pkarry, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' Load and Infuse a status item defintion
        ''' </summary>
        ''' <param name="TYPEID"></param>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal typeid As String, ByVal code As String) As Boolean
            Dim pkarry() As Object = {LCase(typeid), LCase(code)}
            Return MyBase.LoadBy(pkArray:=pkarry)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aStore As New ObjectDefinition

            With aStore
                .Create(constTableID)
                .Delete()
                aFieldDesc.Tablename = constTableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""

                '***
                '*** Fields
                '****

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "type id of the status"
                aFieldDesc.ID = "stat1"
                aFieldDesc.ColumnName = "typeid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "code"
                aFieldDesc.ID = "stat2"
                aFieldDesc.ColumnName = "code"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "name of status"
                aFieldDesc.ID = "stat3"
                aFieldDesc.ColumnName = "name"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ID = "stat4"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "kpi code of this status"
                aFieldDesc.ID = "stat5"
                aFieldDesc.ColumnName = "kpicode"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "weight"
                aFieldDesc.ID = "stat6"
                aFieldDesc.ColumnName = "weight"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is end status"
                aFieldDesc.ID = "stat7"
                aFieldDesc.ColumnName = "isend"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is start status"
                aFieldDesc.ID = "stat8"
                aFieldDesc.ColumnName = "isstart"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is intermediate status"
                aFieldDesc.ID = "stat9"
                aFieldDesc.ColumnName = "isimed"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "foreground color"
                aFieldDesc.ID = "stat10"
                aFieldDesc.ColumnName = "fgcolor"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "background color"
                aFieldDesc.ID = "stat11"
                aFieldDesc.ColumnName = "bgcolor"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "kpi code foreground color"
                aFieldDesc.ID = "stat12"
                aFieldDesc.ColumnName = "kpifgcolor"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.[Long]
                aFieldDesc.Title = "kpi code background color"
                aFieldDesc.ID = "stat13"
                aFieldDesc.ColumnName = "kpibgcolor"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)

                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            '
            CreateSchema = True
            Exit Function


        End Function
        ''' <summary>
        ''' Persist the object
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Try
                Call Me.Record.SetValue("typeid", s_typeid)
                Call Me.Record.SetValue("code", s_code)
                Call Me.Record.SetValue("name", s_name)
                Call Me.Record.SetValue("kpicode", s_kpicode)
                Call Me.Record.SetValue("desc", s_description)

                Call Me.Record.SetValue("isend", s_endStatus)
                Call Me.Record.SetValue("isstart", s_startStatus)
                Call Me.Record.SetValue("isintermediate", s_intermediateStatus)

                Call Me.Record.SetValue("fgcolor", s_fgcolor)
                Call Me.Record.SetValue("bgcolor", s_bgcolor)
                Call Me.Record.SetValue("kpifgcolor", s_kpifgcolor)
                Call Me.Record.SetValue("kpibgcolor", s_kpibgcolor)
                Return MyBase.Persist(timestamp)
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOTDBDefStatusItem.persist", exception:=ex)
                Return False
            End Try



        End Function

        ''' <summary>
        ''' create a persistable object 
        ''' </summary>
        ''' <param name="typeid"></param>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal typeid As String, ByVal code As String) As Boolean
            ' set the primaryKey
            Dim primarykey() As Object = {LCase(typeid), LCase(code)}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                s_typeid = LCase(typeid)
                s_code = LCase(code)
                Return True
            Else
                Return False
            End If

        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefWorkspace describes additional database schema information
    '*****
    ''' <summary>
    ''' Workspace Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Workspace
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        '** const
        <ormSchemaTableAttribute(Version:=2, adddeletefieldbehavior:=True)> Const ConstTableID As String = "tblDefWorkspaces"

        <ormSchemaColumnAttribute(id:="WS", _
            typeid:=otFieldDataType.Text, size:=50, _
            title:="workspaceID", Description:="workspaceID identifier", _
            primaryKeyordinal:=1)> _
        Public Const ConstFNWorkspaceID As String = "wspace"

        <ormSchemaColumnAttribute(ID:="WS1", _
            typeid:=otFieldDataType.Text, size:=100, _
            title:="Description")> _
        Public Const ConstFNDescription = "desc"

        <ormSchemaColumnAttribute(ID:="WS2", _
            typeid:=otFieldDataType.Text, isArray:=True, _
            title:="forecast lookup order", description:="Forecasts milestones are lookup in this order. Must include this workspaceID ID.")> _
        Public Const ConstFNFCRelyOn = "fcrelyOn"

        <ormSchemaColumnAttribute(ID:="WS3", _
            typeid:=otFieldDataType.Text, isArray:=True, _
            title:="actual lookup order", description:="Actual milestones are looked up in this order. Must include this workspaceID ID")> _
        Public Const ConstFNActRelyOn = "actrelyOn"

        <ormSchemaColumnAttribute(ID:="WS4", _
            typeid:=otFieldDataType.Bool, _
            title:="Base", description:="if set this workspaceID is a base workspaceID") _
             > _
        Public Const ConstFNIsBase = "isbase"

        <ormSchemaColumnAttribute(ID:="WS5", _
              typeid:=otFieldDataType.Bool, _
              title:="has actuals", description:="if set this workspaceID has actual milestones") _
               > Public Const ConstFNHasAct = "hasact"

        <ormSchemaColumnAttribute(ID:="WS6", _
          typeid:=otFieldDataType.Text, isarray:=True, _
          title:="accesslist", description:="Accesslist") _
           > Public Const ConstFNAccesslist = "acclist"

        <ormSchemaColumnAttribute(ID:="WS7", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="min schedule updc", description:="Minimum update counter for schedules of this workspaceID") _
               > Public Const ConstMinScheduleUPC = "minsupdc"

        <ormSchemaColumnAttribute(ID:="WS8", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="max schedule updc", description:="Maximum update counter for schedules of this workspaceID") _
               > Public Const ConstFNMaxScheduleUPC = "maxsupdc"

        <ormSchemaColumnAttribute(ID:="WS9", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="min target updc", description:="Minimum update counter for targets of this workspaceID") _
               > Public Const ConstFNMinTargetUPDC = "mintupdc"

        <ormSchemaColumnAttribute(ID:="WS10", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="max target updc", description:="Minimum update counter for target of this workspaceID") _
               > Public Const ConstMaxTargetUPDC = "maxtupdc"

        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNWorkspaceID)> Private s_ID As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDescription)> Private s_description As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNIsBase)> Private s_isBasespace As Boolean
        <ormColumnMappingAttribute(fieldname:=ConstFNHasAct)> Private s_hasActuals As Boolean
        <ormColumnMappingAttribute(fieldname:=ConstFNFCRelyOn)> Private s_fcrelyingOn As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNActRelyOn)> Private s_actrelyingOn As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNAccesslist)> Private s_accesslistID As String = ""

        <ormColumnMappingAttribute(fieldname:=ConstMinScheduleUPC)> Private s_min_schedule_updc As Long
        <ormColumnMappingAttribute(fieldname:=ConstFNMaxScheduleUPC)> Private s_max_schedule_updc As Long
        <ormColumnMappingAttribute(fieldname:=ConstFNMinTargetUPDC)> Private s_min_target_updc As Long
        <ormColumnMappingAttribute(fieldname:=ConstMaxTargetUPDC)> Private s_max_target_updc As Long

        ' dynamics
        Private fc_wspace_stack As New Collection
        Private act_wspace_stack As New Collection

        ' further internals

        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"

        <ormPropertyMappingAttribute(ID:="ID", fieldname:=ConstFNWorkspaceID, tableid:=ConstTableID)> ReadOnly Property ID() As String
            Get
                ID = s_ID
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                s_description = value
                Me.IsChanged = True
            End Set
        End Property


        Public Property IsBasespace() As Boolean
            Get
                IsBasespace = s_isBasespace
            End Get
            Set(value As Boolean)
                s_isBasespace = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property HasActuals() As Boolean
            Get
                HasActuals = s_hasActuals
            End Get
            Set(value As Boolean)
                s_hasActuals = value
                Me.IsChanged = True
            End Set
        End Property


        Public Property FCRelyingOn() As String()
            Get
                FCRelyingOn = SplitMultbyChar(text:=s_fcrelyingOn, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(FCRelyingOn) Then
                    FCRelyingOn = New String() {}
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
                    s_fcrelyingOn = aStrValue
                    Me.IsChanged = True
                    'ElseIf Not isEmpty(Trim(aVAlue)) And Trim(aVAlue) <> "" And Not isNull(aVAlue) Then
                    '   s_fcrelyingOn = ConstDelimiter & UCase(Trim(avalue)) & ConstDelimiter
                Else
                    s_fcrelyingOn = ""
                End If
            End Set
        End Property


        Public Property ACTRelyingOn() As String()
            Get
                ACTRelyingOn = SplitMultbyChar(text:=s_actrelyingOn, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(ACTRelyingOn) Then
                    ACTRelyingOn = New String() {}
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
                    s_actrelyingOn = aStrValue
                    Me.IsChanged = True
                    'ElseIf Not isEmpty(Trim(aVAlue)) And Trim(aVAlue) <> "" And Not isNull(aVAlue) Then
                    '   s_actrelyingOn = ConstDelimiter & UCase(Trim(avalue)) & ConstDelimiter
                Else
                    s_actrelyingOn = ""
                End If
            End Set
        End Property

        Public Property AccesslistIDs() As String()
            Get
                AccesslistIDs = SplitMultbyChar(text:=s_accesslistID, DelimChar:=ConstDelimiter)
                If Not IsArrayInitialized(AccesslistIDs) Then
                    AccesslistIDs = New String() {}
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
                    s_accesslistID = aStrValue
                    Me.IsChanged = True
                    'ElseIf Not isEmpty(Trim(aVAlue)) And Trim(aVAlue) <> "" And Not isNull(aVAlue) Then
                    '   s_accesslistID = ConstDelimiter & UCase(Trim(avalue)) & ConstDelimiter
                Else
                    s_accesslistID = ""
                End If
            End Set
        End Property

        Public Property Min_schedule_updc() As Long
            Get
                Min_schedule_updc = s_min_schedule_updc
            End Get
            Set(value As Long)
                s_min_schedule_updc = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Max_schedule_updc() As Long
            Get
                Max_schedule_updc = s_max_schedule_updc
            End Get
            Set(value As Long)
                s_max_schedule_updc = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Min_target_updc() As Long
            Get
                Min_target_updc = s_min_target_updc
            End Get
            Set(value As Long)
                s_min_target_updc = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Max_target_updc() As Long
            Get
                Max_target_updc = s_max_target_updc
            End Get
            Set(value As Long)
                s_max_target_updc = value
                Me.IsChanged = True
            End Set
        End Property

#End Region

        ''' <summary>
        ''' initialize the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional forcereload As Boolean = False) As Workspace
            Dim pkarray() As Object = {UCase(id)}
            Return Retrieve(Of Workspace)(pkArray:=pkarray, forceReload:=forcereload)
        End Function

        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal workspaceID As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(workspaceID))}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of Workspace)(silent:=silent)

            '***
            '*** LEGACY
            Dim primaryColumnNames As New Collection
            Dim usedKeyColumnNames As New Collection
            Dim aFieldDesc As New ormFieldDescription
            Dim aStore As New ObjectDefinition


            aFieldDesc.ID = ""
            aFieldDesc.Parameter = ""
            aFieldDesc.Relation = New String() {}
            aFieldDesc.Aliases = New String() {}
            aFieldDesc.Tablename = ConstTableID

            Try


                With aStore
                    .Create(ConstTableID)
                    .Delete()
                    '***
                    '*** Fields
                    '****


                    'Tablename
                    aFieldDesc.Datatype = otFieldDataType.Text
                    aFieldDesc.Title = "workspaceID  id"
                    aFieldDesc.ID = "ws"
                    aFieldDesc.ColumnName = ConstFNWorkspaceID
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)
                    primaryColumnNames.Add(aFieldDesc.ColumnName)

                    'fieldnames
                    aFieldDesc.Datatype = otFieldDataType.Text
                    aFieldDesc.Title = "workspaceID description"
                    aFieldDesc.ID = "ws1"
                    aFieldDesc.ColumnName = "desc"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    ' relyOn
                    aFieldDesc.Datatype = otFieldDataType.Text
                    aFieldDesc.Title = "forecast relying on"
                    aFieldDesc.ID = "ws2"
                    aFieldDesc.ColumnName = "fcrelyOn"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    ' relyOn
                    aFieldDesc.Datatype = otFieldDataType.Text
                    aFieldDesc.Title = "actuals relying on"
                    aFieldDesc.ID = "ws3"
                    aFieldDesc.ColumnName = "actrelyOn"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.Bool
                    aFieldDesc.Title = "isBase workspaceID"
                    aFieldDesc.ID = "ws4"
                    aFieldDesc.ColumnName = "isbase"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.Bool
                    aFieldDesc.Title = "has actuals in workspaceID"
                    aFieldDesc.ID = "ws5"
                    aFieldDesc.ColumnName = "hasact"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    ' Access List
                    aFieldDesc.Datatype = otFieldDataType.Text
                    aFieldDesc.Title = "access list"
                    aFieldDesc.ID = "ws6"
                    aFieldDesc.ColumnName = "acclist"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.[Long]
                    aFieldDesc.Title = "min schedule updc"
                    aFieldDesc.ID = "ws10"
                    aFieldDesc.ColumnName = "minsupdc"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.[Long]
                    aFieldDesc.Title = "max schedule updc"
                    aFieldDesc.ID = "ws11"
                    aFieldDesc.ColumnName = "maxsupdc"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.[Long]
                    aFieldDesc.Title = "min Target updc"
                    aFieldDesc.ID = "ws12"
                    aFieldDesc.ColumnName = "mintupdc"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.[Long]
                    aFieldDesc.Title = "max Target updc"
                    aFieldDesc.ID = "ws13"
                    aFieldDesc.ColumnName = "maxtupdc"
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    '***
                    '*** TIMESTAMP
                    '****
                    aFieldDesc.Datatype = otFieldDataType.Timestamp
                    aFieldDesc.Title = "last Update"
                    aFieldDesc.ColumnName = ConstFNUpdatedOn
                    aFieldDesc.ID = ""
                    aFieldDesc.Aliases = New String() {}
                    aFieldDesc.Relation = New String() {}
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)

                    aFieldDesc.Datatype = otFieldDataType.Timestamp
                    aFieldDesc.Title = "creation Date"
                    aFieldDesc.ColumnName = ConstFNCreatedOn
                    aFieldDesc.ID = ""
                    aFieldDesc.Aliases = New String() {}
                    aFieldDesc.Relation = New String() {}
                    Call .AddFieldDesc(fielddesc:=aFieldDesc)
                    ' Index
                    Call .AddIndex("PrimaryKey", primaryColumnNames, isprimarykey:=True)
                    ' persist
                    .Persist()
                    ' change the database
                    .AlterSchema()
                End With

                Return True

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefWorkspace.CreateSchema")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal workspaceID As String) As Boolean
            Dim primarykey() As Object = {UCase(workspaceID)}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                s_ID = UCase(workspaceID)
                Return True
            Else
                Return False
            End If
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of clsotdbDefWorkspace) of all workspaceID Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Workspace)
            Dim aCollection As List(Of Workspace) = ormDataObject.All(Of Workspace)()
            Dim aList As New List(Of Workspace)
            For Each entry In aCollection
                aList.Add(entry)
                Cache.AddToCache(ConstTableID, entry.id, entry)
            Next
            Return aList
        End Function
#End Region
    End Class

    ''' <summary>
    ''' Domain Definition Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Domain
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

    
        '** const
        <ormSchemaTableAttribute(Version:=1)> Public Const ConstTableID As String = "tblDefDomains"

        <ormSchemaColumnAttribute(id:="DM1", _
            typeid:=otFieldDataType.Text, size:=50, _
            title:="domain", Description:="domain identifier", _
            primaryKeyordinal:=1)> _
        Public Const ConstFNDomainID As String = "domain"

        <ormSchemaColumnAttribute(ID:="DM2", _
            typeid:=otFieldDataType.Text, size:=100, _
            title:="Description")> _
        Public Const ConstFNDescription = "desc"

        <ormSchemaColumnAttribute(ID:="DM3", _
            typeid:=otFieldDataType.Bool, title:="Global", description:="if set this domain is the global domain") _
             > Public Const ConstFNIsGlobal = "isglobal"

        <ormSchemaColumnAttribute(ID:="DM10", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="min deliverable uid", description:="Minimum deliverable uid for domain") _
               > Public Const ConstFNMinDeliverableUID = "mindlvuid"

        <ormSchemaColumnAttribute(ID:="DM11", _
              typeid:=otFieldDataType.[Long], defaultValue:="0", _
              title:="max deliverable uid", description:="Maximum deliverable uid for domain") _
               > Public Const ConstFNMaxDeliverableUID = "maxdlvuid"


        ' fields
        <ormColumnMappingAttribute(fieldname:=ConstFNDomainID)> Private _ID As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNDescription)> Private _description As String = ""
        <ormColumnMappingAttribute(fieldname:=ConstFNIsGlobal)> Private _isGlobal As Boolean

        <ormColumnMappingAttribute(fieldname:=ConstFNMinDeliverableUID)> Private _min_deliverable_uid As Long
        <ormColumnMappingAttribute(fieldname:=ConstFNMaxDeliverableUID)> Private _max_deliverable_uid As Long

        ' dynamics
        Private _settings As New Dictionary(Of String, DomainSetting)
        Public Event OnInitialize As EventHandler(Of DomainEventArgs)
        Public Event OnReset As EventHandler(Of DomainEventArgs)

        Private _SessionDir As New Dictionary(Of String, Session)

        ''' <summary>
        ''' constructor of a clsOTDBDefWorkspace
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Call MyBase.New(ConstTableID)
        End Sub

#Region "Properties"
        <ormPropertyMappingAttribute(ID:="ID", fieldname:=ConstFNDomainID, tableid:=ConstTableID)> ReadOnly Property ID() As String
            Get
                ID = _ID
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property
        Public Property IsGlobal() As Boolean
            Get
                IsGlobal = _isGlobal
            End Get
            Set(value As Boolean)
                _isGlobal = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property MinDeliverableUID() As Long
            Get
                MinDeliverableUID = _min_deliverable_uid
            End Get
            Set(value As Long)
                _min_deliverable_uid = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property MaxDeliverableUID() As Long
            Get
                MaxDeliverableUID = _max_deliverable_uid
            End Get
            Set(value As Long)
                _max_deliverable_uid = value
                Me.IsChanged = True
            End Set
        End Property

#End Region
        ''' <summary>
        ''' handles the session start event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnSessionEnd(sender As Object, e As SessionEventArgs)
            If _SessionDir.ContainsKey(e.Session.SessionID) Then
                _SessionDir.Remove(e.Session.SessionID)
            End If

        End Sub
        ''' <summary>
        ''' handles the session end event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Public Sub OnSessionStart(sender As Object, e As SessionEventArgs)

        End Sub
        ''' <summary>
        ''' Register a Session a the Domain
        ''' </summary>
        ''' <param name="session"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RegisterSession(session As Session) As Boolean
            If _SessionDir.containsKey(session.SessionID) Then
                _Sessiondir.remove(session.SessionID)
            End If
            _SessionDir.Add(session.SessionID, session)
            AddHandler session.OnStarted, AddressOf OnSessionStart
            AddHandler session.OnEnding, AddressOf OnSessionEnd

        End Function
        ''' <summary>
        ''' initialize the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize

            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Initialize = MyBase.Initialize
            RaiseEvent OnInitialize(Me, New DomainEventArgs(Me))
            Return Initialize
        End Function
        Public Shared Function GlobalDomain() As Domain
            Return Retrieve(id:=ConstGlobalDomain)
        End Function
        ''' <summary>
        ''' Retrieve the workspaceID Cache Object
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional dbdriver As iormDBDriver = Nothing, Optional forcereload As Boolean = False) As Domain
            Dim pkarray() As Object = {UCase(id)}
            Return Retrieve(Of Domain)(pkarray:=pkarray, dbdriver:=dbdriver, forcereload:=forcereload)
        End Function

        ''' <summary>
        ''' load and infuse the current workspaceID object
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal domainID As String) As Boolean
            Dim primarykey() As Object = {UCase(Trim(domainID))}
            Return MyBase.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' returns true if the setting exists
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function hasSetting(id As String) As Boolean
            Return _settings.ContainsKey(key:=UCase(id))
        End Function
        ''' <summary>
        ''' returns the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSetting(id As String) As DomainSetting
            If Me.hasSetting(id:=id) Then
                Return _settings.Item(key:=UCase(id))
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' sets the setting valid in the domain
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SetSetting(id As String, datatype As otFieldDataType, value As Object) As Boolean
            Dim aSetting As New DomainSetting
            If Me.hasSetting(id:=id) Then
                aSetting = Me.GetSetting(id:=id)
            Else
                If Not aSetting.Create(domainID:=Me.ID, id:=id) Then
                    aSetting = DomainSetting.Retrieve(domainID:=Me.ID, id:=id)
                End If
            End If

            If aSetting Is Nothing OrElse Not (aSetting.IsLoaded Or aSetting.IsCreated) Then
                Return False
            End If
            aSetting.Datatype = datatype
            aSetting.value = value

            If Not Me.hasSetting(id:=id) Then _settings.Add(key:=aSetting.ID, value:=aSetting)
            Return True
        End Function
        ''' <summary>
        ''' Load the settings to the settings dictionary
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadSettings() As Boolean
            Dim aListDomain As New List(Of DomainSetting)
            If ConstGlobalDomain <> Me.ID Then aListDomain = DomainSetting.RetrieveByDomain(domainID:=Me.ID)
            Dim aListGlobal As List(Of DomainSetting) = DomainSetting.RetrieveByDomain(domainID:=ConstGlobalDomain)

            '** first for the global
            For Each aSetting In aListGlobal
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next

            '** overwrite
            For Each aSetting In aListDomain
                If _settings.ContainsKey(key:=aSetting.ID) Then
                    _settings.Remove(key:=aSetting.ID)
                End If
                _settings.Add(key:=aSetting.ID, value:=aSetting)
            Next
            Return True
        End Function
        ''' <summary>
        ''' Persist the data object
        ''' </summary>
        ''' <param name="TIMESTAMP"></param>
        ''' <param name="ForceSerializeToOTDB"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate) As Boolean
            Try
                If Not FeedRecord() Then
                    Persist = False
                    Exit Function
                End If

                For Each aSetting In _settings.Values
                    aSetting.Persist()
                Next

                Persist = MyBase.Persist(timestamp)
                Exit Function

            Catch ex As Exception
                Call CoreMessageHandler(subname:="Domain.Persist", exception:=ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' infuse the domain  by a record and load the settings
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            Try
                If MyBase.Infuse(record) Then
                    If Not LoadSettings() Then
                        Me.Unload()
                        Return False
                    End If
                End If
                Return Me.IsLoaded
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="Domain.Infuse")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' create the objects persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean
            Return ormDataObject.CreateSchema(Of Domain)(silent:=silent)
        End Function

        ''' <summary>
        ''' creates with this object a new persistable Def workspaceID
        ''' </summary>
        ''' <param name="workspaceID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Create(ByVal domainID As String) As Boolean
            Dim primarykey() As Object = {UCase(domainID)}
            If MyBase.Create(primarykey, checkUnique:=False) Then
                _ID = UCase(domainID)
                Return True
            Else
                Return False
            End If
        End Function

#Region "static routines"
        ''' <summary>
        ''' returns a List(of Domain) of all workspaceID Definitions
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of Domain)
            Dim aCollection As List(Of Domain) = ormDataObject.All(Of Domain)()
            Dim aList As New List(Of Domain)
            For Each entry In aCollection
                aList.Add(entry)
                Cache.AddToCache(ConstTableID, entry.id, entry)
            Next
            Return aList
        End Function
#End Region
    End Class
    '************************************************************************************
    '***** CLASS clsOTDBDefOrgUnit describes additional database schema information
    '*****
    ''' <summary>
    ''' Organization Unit Definition Class
    ''' </summary>
    ''' <remarks></remarks>

    Public Class clsOTDBDefOrgUnit
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Const _tableID As String = "tblDefOrgUnits"

        ' fields
        Private _id As String = ""
        Private _description As String = ""
        Private _manager As String = ""
        Private _siteid As String = ""
        Private _superiorOUID As String = ""
        Private _functionid As String = ""

        ''' <summary>
        ''' constructor of a DefOrgUnit
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(_tableID)
        End Sub

        ReadOnly Property ID() As String
            Get
                ID = _id
            End Get

        End Property

        Public Property Description() As String
            Get
                Description = _description
            End Get
            Set(value As String)
                _description = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Manager() As String
            Get
                Manager = _manager
            End Get
            Set(value As String)
                _manager = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Siteid() As String
            Get
                Siteid = _siteid
            End Get
            Set(value As String)
                _siteid = value
                Me.IsChanged = True
            End Set
        End Property


        Public Property SuperiorOUID() As String
            Get
                SuperiorOUID = _superiorOUID
            End Get
            Set(value As String)
                _superiorOUID = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Functionid() As String
            Get
                Functionid = _functionid
            End Get
            Set(value As String)
                _functionid = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' Infueses the DefOrgUnit Object with a record
        ''' </summary>
        ''' <param name="aRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If

            Try
                _id = CStr(record.GetValue("id"))
                _description = CStr(record.GetValue("desc"))
                _siteid = CStr(record.GetValue("sited"))
                _manager = CStr(record.GetValue("manager"))
                _functionid = CStr(record.GetValue("functionid"))
                _superiorOUID = CStr(record.GetValue("supouid"))
                _IsLoaded = MyBase.Infuse(record)
                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefOrgUnit.infuse")
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Retrieve a User Definition
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional forcereload As Boolean = False) As clsOTDBDefOrgUnit
            Return Retrieve(Of clsOTDBDefOrgUnit)(pkArray:={id}, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' loads and infuses a DefOrgUnit Object with the primary key
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal id As String) As Boolean
            Dim primarykey() As Object = {id}
            Return MyBase.LoadBy(pkArray:=primarykey)
        End Function
        ''' <summary>
        ''' create the persistence schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aStore As New ObjectDefinition

            With aStore
                .Create(_tableID)
                .Delete()

                aFieldDesc.Tablename = _tableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""


                '***
                '*** Fields
                '****

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "organisation unit id"
                aFieldDesc.ID = "OU1"
                aFieldDesc.ColumnName = "id"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "organization unit description"
                aFieldDesc.ID = "OU2"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "manager"
                aFieldDesc.ID = "OU3"
                aFieldDesc.Relation = New String() {"P1"}
                aFieldDesc.ColumnName = "manager"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "siteid"
                aFieldDesc.ID = "OU4"
                aFieldDesc.ColumnName = "siteid"
                aFieldDesc.Relation = New String() {"ous1"}
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "functionid"
                aFieldDesc.ID = "OU5"
                aFieldDesc.ColumnName = "functionid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "superior organisation unit ID"
                aFieldDesc.ID = "OU6"
                aFieldDesc.ColumnName = "supouid"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
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
        ''' Persists the Object in the data store
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Try
                Call Me.Record.SetValue("id", _id)
                Call Me.Record.SetValue("desc", _description)
                Call Me.Record.SetValue("siteid", _siteid)
                Call Me.Record.SetValue("manager", _manager)
                Call Me.Record.SetValue("functionid", _manager)
                Call Me.Record.SetValue("supouid", _superiorOUID)

                Return MyBase.Persist(timestamp)

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDEfOrgUnit.persist")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All() As List(Of clsOTDBDefOrgUnit)
            Return ormDataObject.All(Of clsOTDBDefOrgUnit)()
        End Function
        '**** create : create a new Object with primary keys
        '****
        Public Function Create(ByVal id As String) As Boolean
            Dim primarykey() As Object = {id}
            ' set the primaryKey
            If MyBase.Create(primarykey, checkUnique:=True) Then
                _id = id
                Return True
            Else
                Return False
            End If

        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefOUSite describes additional database schema information
    '*****

    Public Class clsOTDBDefOUSite
        Inherits ormDataObject
        Implements iormInfusable
        Implements iormPersistable

        Const _tableID As String = "tblDefOUSites"

        ' fields
        Private s_id As String
        Private s_description As String
        ''' <summary>
        ''' constructor of Def OUSite
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(_tableID)

        End Sub
        ''' <summary>
        ''' ID
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ID() As String
            Get
                ID = s_id
            End Get

        End Property
        ''' <summary>
        ''' Description
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Description = s_description
            End Get
            Set(value As String)
                s_description = value
                Me.IsChanged = True
            End Set
        End Property
        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' Infuses a DEFOUSite Object by a record
        ''' </summary>
        ''' <param name="record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If


            Try
                s_id = CStr(record.GetValue("id"))
                s_description = CStr(record.GetValue("desc"))
                _IsLoaded = MyBase.Infuse(record)
                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDEfOUSite.Infuse")
                Return False
            End Try


        End Function
        ''' <summary>
        ''' Retrieve
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Retrieve(ByVal id As String, Optional forcereload As Boolean = False) As clsOTDBDefOUSite
            Return Retrieve(Of clsOTDBDefOUSite)(pkArray:={id}, forceReload:=forcereload)
        End Function
        ''' <summary>
        ''' Load and infuse the object 
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadBy(ByVal id As String) As Boolean
            Dim pkarry() As Object = {id}
            Return MyBase.LoadBy(pkArray:=pkarry)
        End Function
        ''' <summary>
        ''' create the persistency object
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim primaryColumnNames As New Collection
            Dim aStore As New ObjectDefinition

            With aStore
                .Create(_tableID)
                .Delete()

                aFieldDesc.Tablename = _tableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""
                aFieldDesc.Relation = New String() {}
                '***
                '*** Fields
                '****

                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "site id"
                aFieldDesc.ID = "ous1"
                aFieldDesc.ColumnName = "id"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                primaryColumnNames.Add(aFieldDesc.ColumnName)

                'fieldnames
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "organization unit description"
                aFieldDesc.ID = "ous2"
                aFieldDesc.ColumnName = "desc"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", primaryColumnNames, isprimarykey:=True)

                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With

            CreateSchema = True
            Exit Function


        End Function

        ''' <summary>
        ''' Persist the data object
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Call Me.Record.SetValue("id", s_id)
            Call Me.Record.SetValue("desc", s_description)

            Return MyBase.Persist(timestamp)

        End Function
        ''' <summary>
        ''' returns a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function All() As List(Of clsOTDBDefOUSite)
            Return ormDataObject.All(Of clsOTDBDefOUSite)()
        End Function
        '**** create : create a new Object with primary keys
        '****
        Public Function Create(ByVal ID As String) As Boolean
            Dim primarykey() As Object = {ID}
            ' set the primaryKey
            If MyBase.Create(primarykey, checkUnique:=True) Then
                s_id = ID
                Return True
            Else
                Return False
            End If
        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsOTDBDefUserAccessList is the OnTrack User Definition Class
    '*****
    '*****
    ''' <summary>
    ''' Definition of User Access List Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOTDBDefUserAccessList
        Inherits ormDataObject
        Implements iormPersistable
        Implements iormInfusable

        Const _tableID = "tblDefUserAccessLists"

        Private s_id As String
        Private s_username As String       ' condition type

        'fields

        Private s_isAllUsers As Boolean
        Private s_desc As String
        Private s_hasRead As Boolean
        Private s_hasUpdate As Boolean
        Private s_hasAlterSchema As Boolean
        Private s_hasNoRights As Boolean

        ''' <summary>
        ''' initialize the object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Initialize() As Boolean Implements iormPersistable.Initialize
            Me.TableStore.SetProperty(ConstTPNCacheProperty, True)
            Return MyBase.Initialize
        End Function
        ''' <summary>
        ''' constuctor of a user Access List Object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New(_tableID)
        End Sub


        Public Property Description() As String
            Get
                Description = s_desc
            End Get
            Set(value As String)
                s_desc = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property ID() As String
            Get
                ID = s_id
            End Get
            Set(value As String)
                s_id = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property Username() As String
            Get
                Username = s_username
            End Get
            Set(value As String)
                s_username = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property HasNoRights() As Boolean
            Get
                HasNoRights = s_hasNoRights
            End Get
            Set(value As Boolean)
                s_hasNoRights = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property HasReadRights() As Boolean
            Get
                HasReadRights = s_hasRead
            End Get
            Set(value As Boolean)
                s_hasRead = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property HasUpdateRights() As Boolean
            Get
                HasUpdateRights = s_hasUpdate
            End Get
            Set(value As Boolean)
                s_hasUpdate = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property HasAlterSchemaRights() As Boolean
            Get
                HasAlterSchemaRights = s_hasAlterSchema
            End Get
            Set(value As Boolean)
                s_hasAlterSchema = value
                Me.IsChanged = True
            End Set
        End Property

        Public Property IsAllUsers() As Boolean
            Get
                IsAllUsers = s_isAllUsers
            End Get
            Set(value As Boolean)
                s_isAllUsers = value
                Me.IsChanged = True
            End Set
        End Property

        ''' <summary>
        ''' return a collection of all objects
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function All() As List(Of clsOTDBDefUserAccessList)
            Return ormDataObject.All(Of clsOTDBDefUserAccessList)(orderby:="username desc")
        End Function

        '****** getAnonymous: "static" function to return the first Anonymous user
        '******
        Public Shared Function GetAnonymous() As clsOTDBDefUserAccessList
            Dim aObjectCollection As List(Of clsOTDBDefUserAccessList) = ormDataObject.All(Of clsOTDBDefUserAccessList)(orderby:="name desc", where:="isall=1")

            If aObjectCollection.Count = 0 Then
                Return Nothing
            Else
                Return aObjectCollection.Item(1)
            End If

        End Function


        '**** infuese the object by a OTDBRecord
        '****
        Public Function Infuse(ByRef aRecord As ormRecord) As Boolean Implements iormInfusable.Infuse

            '* lazy init
            If Not Me.IsInitialized Then
                If Not Me.Initialize() Then
                    Infuse = False
                    Exit Function
                End If
            End If
            Try
                s_id = CStr(aRecord.GetValue("id"))
                s_username = CStr(aRecord.GetValue("username"))

                s_desc = CStr(aRecord.GetValue("desc"))

                s_isAllUsers = CBool(aRecord.GetValue("isall"))
                s_hasAlterSchema = CBool(aRecord.GetValue("alterschema"))
                s_hasNoRights = CBool(aRecord.GetValue("noright"))
                s_hasRead = CBool(aRecord.GetValue("readdata"))
                s_hasUpdate = CBool(aRecord.GetValue("updatedata"))

                _IsLoaded = MyBase.Infuse(aRecord)
                Return Me.IsLoaded

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefUserAccessList.Infuse")
                Return False
            End Try
            Exit Function

        End Function

        ''' <summary>
        ''' loads and infuses a DefUserAccessList by primary key
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="username"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function LoadBy(ByVal id As String, ByVal username As String) As Boolean
            Dim primarykey() As Object = {id, username}
            Return Me.LoadBy(primarykey)
        End Function
        ''' <summary>
        ''' create the persistency schema
        ''' </summary>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateSchema(Optional silent As Boolean = True) As Boolean

            Dim aFieldDesc As New ormFieldDescription
            Dim PrimaryColumnNames As New Collection
            Dim aStore As New ObjectDefinition

            With aStore
                .Create(_tableID)
                .Delete()

                aFieldDesc.Tablename = _tableID
                aFieldDesc.ID = ""
                aFieldDesc.Parameter = ""
                aFieldDesc.Relation = New String() {}

                '***
                '*** Fields
                '****

                ' Username
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "accesslist id"
                aFieldDesc.ColumnName = "id"
                aFieldDesc.ID = "acl1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)


                '
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "username of user"
                aFieldDesc.ColumnName = "username"
                aFieldDesc.ID = "u1"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                PrimaryColumnNames.Add(aFieldDesc.ColumnName)

                '
                aFieldDesc.Datatype = otFieldDataType.Text
                aFieldDesc.Title = "description"
                aFieldDesc.ColumnName = "desc"
                aFieldDesc.ID = "acl3"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' is anonymous
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "is all users"
                aFieldDesc.ColumnName = "isall"
                aFieldDesc.ID = "acl4"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' right
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "alter schema right"
                aFieldDesc.ColumnName = "alterschema"
                aFieldDesc.ID = "acl5"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' right
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "update data right"
                aFieldDesc.ColumnName = "updatedata"
                aFieldDesc.ID = "acl6"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' right
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "read data right"
                aFieldDesc.ColumnName = "readdata"
                aFieldDesc.ID = "acl7"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                ' right
                aFieldDesc.Datatype = otFieldDataType.Bool
                aFieldDesc.Title = "no right at all"
                aFieldDesc.ColumnName = "noright"
                aFieldDesc.ID = "acl8"
                Call .AddFieldDesc(fielddesc:=aFieldDesc)


                '***
                '*** TIMESTAMP
                '****
                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "last Update"
                aFieldDesc.ColumnName = ConstFNUpdatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)

                aFieldDesc.Datatype = otFieldDataType.Timestamp
                aFieldDesc.Title = "creation Date"
                aFieldDesc.ColumnName = ConstFNCreatedOn
                aFieldDesc.ID = ""
                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                ' Index
                Call .AddIndex("PrimaryKey", PrimaryColumnNames, isprimarykey:=True)


                ' persist
                .Persist()
                ' change the database
                .AlterSchema()
            End With
            '
            CreateSchema = True
            Exit Function

        End Function

        ''' <summary>
        ''' Persist the object
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Persist(Optional timestamp As Date = ot.ConstNullDate) As Boolean

            Try
                Call Me.Record.SetValue("id", s_id)
                Call Me.Record.SetValue("username", s_username)

                Call Me.Record.SetValue("desc", s_desc)
                Call Me.Record.SetValue("isall", s_isAllUsers)
                Call Me.Record.SetValue("noright", s_hasNoRights)
                Call Me.Record.SetValue("updatedata", s_hasUpdate)
                Call Me.Record.SetValue("readdata", s_hasRead)
                Call Me.Record.SetValue("alterschema", s_hasAlterSchema)

                Persist = MyBase.Persist(timestamp)

            Catch ex As Exception

                Call CoreMessageHandler(exception:=ex, subname:="clsOTDBDefUserAccessList.Persist")
                Return False

            End Try

        End Function
        ''' <summary>
        '''  Creates a persistable dataobject 
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="username"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Create(ByVal id As String, ByVal username As String) As Boolean
            Dim primarykey() As Object = {id, username}
            If MyBase.Create(primarykey, checkUnique:=True) Then
                ' set the primaryKey
                s_id = id
                s_username = username
                Return True
            Else
                Return False
            End If

        End Function
    End Class
End Namespace
