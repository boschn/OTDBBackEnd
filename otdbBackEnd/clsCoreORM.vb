
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE ORM Classes for On Track Database Backend Library
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
Imports System.Data
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Attribute
Imports System.IO
Imports System.Text.RegularExpressions

Imports OnTrack.UI
Imports System.Reflection
Imports OnTrack.Commons

Namespace OnTrack
    Namespace Database
       
        ''' <summary>
        ''' an neutral SQL Command
        ''' </summary>
        ''' <remarks></remarks>

        Public Class ormSqlCommand
            Implements iormSqlCommand

            Private _ID As String = String.empty  ' an Unique ID to store
            Protected _parameters As New Dictionary(Of String, ormSqlCommandParameter)
            Protected _parametervalues As New Dictionary(Of String, Object)

            Protected _type As otSQLCommandTypes
            Protected _SqlStatement As String = String.empty
            Protected _SqlText As String = String.empty ' the build SQL Text

            Protected _databaseDriver As iormRelationalDatabaseDriver
            Protected _tablestores As New Dictionary(Of String, iormRelationalTableStore)
            Protected _buildTextRequired As Boolean = True
            Protected _buildVersion As UShort = 0
            Protected _nativeCommand As System.Data.IDbCommand
            Protected _Prepared As Boolean = False

            Public Sub New(ID As String, Optional databasedriver As iormRelationalDatabaseDriver = Nothing)
                _ID = ID
                _databaseDriver = databasedriver
            End Sub

            Public Property ID As String Implements iormSqlCommand.ID
                Get
                    Return _ID
                End Get
                Set(value As String)
                    _ID = ID
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the database driver.
            ''' </summary>
            ''' <value>The database driver.</value>
            Public Property DatabaseDriver() As iormRelationalDatabaseDriver
                Get
                    Return Me._databaseDriver
                End Get
                Set(value As iormRelationalDatabaseDriver)
                    Me._databaseDriver = value
                End Set
            End Property
            ''' <summary>
            ''' returns the build version
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property BuildVersion As UShort Implements iormSqlCommand.BuildVersion
                Get
                    Return _buildVersion
                End Get
            End Property
            ''' <summary>
            ''' returns a copy of the parameters list
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property Parameters As List(Of ormSqlCommandParameter) Implements iormSqlCommand.Parameters
                Get
                    Return _parameters.Values.ToList
                End Get

            End Property
            ''' <summary>
            ''' set the Native Command
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property NativeCommand As System.Data.IDbCommand Implements iormSqlCommand.NativeCommand
                Set(value As System.Data.IDbCommand)
                    _nativeCommand = value
                End Set
                Get
                    Return _nativeCommand
                End Get
            End Property
            ''' <summary>
            ''' returns the build SQL Statement
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable ReadOnly Property SqlText As String Implements iormSqlCommand.SqlText
                Get
                    If Not String.IsNullOrWhiteSpace(_SqlText) OrElse Me.BuildTextRequired Then
                        If Me.BuildTextRequired Then Call BuildSqlText()
                        Return _SqlText
                    Else
                        Return _SqlStatement
                    End If

                End Get
            End Property
            Public Property CustomerSqlStatement As String Implements iormSqlCommand.CustomerSqlStatement
                Get
                    Return _SqlStatement
                End Get
                Set(value As String)
                    _SqlStatement = value
                    _SqlText = value
                    Me.BuildTextRequired = False
                End Set
            End Property

            ''' <summary>
            ''' returns a copy of the table list
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property TableIDs As List(Of String) Implements iormSqlCommand.TableIDs
                Get
                    Return _tablestores.Keys.ToList()
                End Get

            End Property
            ''' <summary>
            ''' Type of the Sql Command -> Select, Delete etc.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property [Type] As otSQLCommandTypes Implements iormSqlCommand.Type
                Get
                    Return _type
                End Get
            End Property
            ''' <summary>
            ''' True if the SQL Statement has to be build, false if it has been build
            ''' </summary>
            ''' <value>True</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property BuildTextRequired As Boolean
                Set(value As Boolean)
                    _buildTextRequired = value
                End Set
                Get
                    Return _buildTextRequired
                End Get
            End Property
            ''' <summary>
            ''' True if the Native sql command is prepared
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property IsPrepared As Boolean
                Get
                    Return _Prepared
                End Get
            End Property
            ''' <summary>
            ''' add a Parameter for the command
            ''' </summary>
            ''' <param name="parameter">a new Parameter</param>
            ''' <returns>true if successful</returns>
            ''' <remarks></remarks>
            Public Function AddParameter(parameter As ormSqlCommandParameter) As Boolean Implements iormSqlCommand.AddParameter

                '**
                '** some checking

                '** PARAMETER ID
                If String.IsNullOrWhiteSpace(parameter.ID) AndAlso String.IsNullOrWhiteSpace(parameter.ColumnName) AndAlso Not parameter.NotColumn Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, message:=" id not set in parameter for sql command", messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf String.IsNullOrWhiteSpace(parameter.ID) AndAlso Not String.IsNullOrWhiteSpace(parameter.ColumnName) AndAlso Not parameter.NotColumn Then
                    parameter.ID = "@" & parameter.ColumnName
                ElseIf Not String.IsNullOrWhiteSpace(parameter.ID) Then
                    parameter.ID = Regex.Replace(parameter.ID, "\s", String.Empty) ' no white chars allowed
                End If

                '** TABLENAME
                If Not parameter.NotColumn Then
                    If Me.TableIDs.Count = 0 Then
                        Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, _
                                              message:="no tablename  set in parameter for sql command", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    ElseIf String.IsNullOrWhiteSpace(parameter.TableID) AndAlso Not String.IsNullOrWhiteSpace(Me.TableIDs(0)) Then
                        parameter.TableID = Me.TableIDs(0)
                        Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, _
                                              message:=" tablename not set in parameter for sql command - first table used", _
                                              messagetype:=otCoreMessageType.InternalWarning, containerID:=Me.TableIDs(0))

                    ElseIf String.IsNullOrWhiteSpace(parameter.TableID) AndAlso String.IsNullOrWhiteSpace(Me.TableIDs(0)) Then
                        Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, _
                                              message:=" tablename not set in parameter for sql command - no default table", _
                                             messagetype:=otCoreMessageType.InternalError)

                        Return False
                    End If
                End If

                '** fieldnames
                If String.IsNullOrWhiteSpace(parameter.ColumnName) AndAlso String.IsNullOrWhiteSpace(parameter.ID) Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, _
                                          message:=" fieldname not set in parameter for sql command", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf Not String.IsNullOrWhiteSpace(parameter.ColumnName) AndAlso String.IsNullOrWhiteSpace(parameter.ID) AndAlso Not parameter.NotColumn Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, _
                                         message:=" fieldname not set in parameter for sql command - use ID without @", _
                                         messagetype:=otCoreMessageType.InternalWarning, containerID:=parameter.TableID, entryname:=parameter.ID)
                    If parameter.ID.First = "@" Then
                        parameter.ColumnName = parameter.ID.Substring(2)
                    Else
                        parameter.ColumnName = parameter.ID
                    End If
                End If

                '** table name ?!
                If String.IsNullOrWhiteSpace(parameter.TableID) AndAlso Not parameter.NotColumn Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", containerID:=parameter.TableID, _
                                          message:="table name is blank", argument:=parameter.ID)
                    Return False
                End If
                If Not parameter.NotColumn AndAlso Not String.IsNullOrWhiteSpace(parameter.TableID) AndAlso Not GetTableStore(parameter.TableID).ContainerSchema.IsInitialized Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", containerID:=parameter.TableID, _
                                           message:="couldnot initialize table schema")
                    Return False
                End If

                If Not parameter.NotColumn AndAlso Not Me._tablestores.ContainsKey(parameter.TableID) Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, entryname:=parameter.ID, _
                                          message:=" tablename of parameter is not used in sql command", _
                                      messagetype:=otCoreMessageType.InternalError, containerID:=parameter.TableID)
                    Return False
                ElseIf Not parameter.NotColumn AndAlso Not Me._tablestores.Item(key:=parameter.TableID).ContainerSchema.HasEntryName(parameter.ColumnName) Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", argument:=Me.ID, entryname:=parameter.ColumnName, _
                                         message:=" fieldname of parameter is not used in table schema", _
                                     messagetype:=otCoreMessageType.InternalError, containerID:=parameter.TableID)
                    Return False

                End If


                ''' datatype
                If parameter.NotColumn And parameter.Datatype = 0 Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.AddParameter", _
                                          argument:=Me.ID, message:=" datatype not set in parameter for sql command", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                    ''' datatype lookup
                ElseIf Not parameter.NotColumn AndAlso parameter.Datatype = 0 Then

                    ''' look up internally first
                    ''' 
                    Dim anAttribute As ormContainerEntryAttribute = ot.GetSchemaTableColumnAttribute(tableid:=parameter.TableID, columnname:=parameter.ColumnName)
                    If anAttribute IsNot Nothing AndAlso anAttribute.HasValueDataType Then
                        parameter.Datatype = anAttribute.Datatype
                    End If
                    ''' datatype still not resolved
                    If parameter.Datatype = 0 Then
                        Dim aSchemaEntry As ContainerEntryDefinition = CurrentSession.Objects.GetColumnEntry(columnname:=parameter.ColumnName, tableid:=parameter.TableID)
                        If aSchemaEntry IsNot Nothing Then parameter.Datatype = aSchemaEntry.Datatype

                    End If
                End If

                '** add the paramter
                If _parameters.ContainsKey(key:=parameter.ID) Then
                    _parameters.Remove(key:=parameter.ID)
                End If
                _parameters.Add(key:=parameter.ID, value:=parameter)
                Return True
            End Function

            ''' <summary>
            ''' Add Table 
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function AddTable(tableid As String) As Boolean
                Dim aTablestore As iormRelationalTableStore
                tableid = tableid.ToUpper
                If Me._databaseDriver Is Nothing Then
                    aTablestore = GetTableStore(tableid:=tableid)
                    If aTablestore Is Nothing Then
                        Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", containerID:=tableid, procedure:="clsOTDBSelectCommand.ADDTable", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        Me.DatabaseDriver = aTablestore.Connection.DatabaseDriver
                    End If
                Else
                    aTablestore = _databaseDriver.GetTableStore(tableID:=tableid)
                End If


                If aTablestore Is Nothing Then
                    Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", containerID:=tableid, procedure:="clsOTDBSelectCommand.ADDTable", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                If Not _tablestores.ContainsKey(key:=tableid) Then
                    _tablestores.Add(key:=tableid, value:=aTablestore)
                End If

                Return True
            End Function
            ''' Sets the parameter value.
            ''' </summary>
            ''' <param name="name">The name of the parameter.</param>
            ''' <param name="value">The value of the object</param>
            ''' <returns></returns>
            Public Function SetParameterValue(ID As String, [value] As Object) As Boolean Implements iormSqlCommand.SetParameterValue
                If Not _parameters.ContainsKey(key:=ID) Then
                    Call CoreMessageHandler(message:="Parameter ID not in Command", argument:=Me.ID, entryname:=ID, procedure:="ormSqlCommand.SetParameterValue", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                ID = Regex.Replace(ID, "\s", String.empty) ' no white chars allowed
                If _parametervalues.ContainsKey(key:=ID) Then
                    _parametervalues.Remove(key:=ID)
                End If

                _parametervalues.Add(key:=ID, value:=[value])

                Return True
            End Function
            ''' <summary>
            ''' returns True if the Command has the parameter
            ''' </summary>
            ''' <param name="ID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function HasParameter(ID As String) As Boolean Implements iormSqlCommand.HasParameter
                ID = Regex.Replace(ID, "\s", String.empty) ' no white chars allowed
                If Not _parameters.ContainsKey(key:=ID) Then
                    Return False
                Else
                    Return True
                End If
            End Function
            ''' Sets the parameter value.
            ''' </summary>
            ''' <param name="name">The name of the parameter.</param>
            ''' <param name="value">The value of the object</param>
            ''' <returns></returns>
            Public Function GetParameterValue(ID As String) As Object Implements iormSqlCommand.GetParameterValue
                ID = Regex.Replace(ID, "\s", String.empty) ' no white chars allowed
                If Not _parameters.ContainsKey(key:=ID) Then
                    Call CoreMessageHandler(message:="Parameter ID not in Command", argument:=Me.ID, entryname:=ID, procedure:="ormSqlCommand.SetParameterValue", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                If _parametervalues.ContainsKey(key:=ID) Then
                    Return _parametervalues.Item(key:=ID)
                Else
                    Dim aParameter As ormSqlCommandParameter = _parameters.Item(key:=ID)
                    Return aParameter.Value
                End If

            End Function
            ''' <summary>
            ''' builds the SQL text for the Command
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function BuildSqlText() As String
                IncBuildVersion()
                _SqlText = _SqlStatement ' simple
                Return _SqlText
            End Function
            ''' <summary>
            ''' prepares the command. returns true if successfull
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Prepare() As Boolean Implements iormSqlCommand.Prepare
                Dim aNativeConnection As System.Data.IDbConnection
                Dim aNativeCommand As System.Data.IDbCommand
                Dim cvtvalue As Object
                Dim aTablestore As iormRelationalTableStore

                If Me.DatabaseDriver Is Nothing And ot.IsConnected Then
                    Me.DatabaseDriver = CurrentDBDriver
                    aNativeConnection = CurrentDBDriver.CurrentConnection.NativeConnection
                ElseIf Me.DatabaseDriver Is Nothing Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", argument:=Me.ID, message:="database driver missing", _
                                                messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf Me.DatabaseDriver.CurrentConnection Is Nothing Then
                    Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", argument:=Me.ID, message:="driver is not connected or connection is missing", _
                                                messagetype:=otCoreMessageType.InternalError)
                    Return False
                Else
                    aNativeConnection = DatabaseDriver.CurrentConnection.NativeConnection
                End If

                Try
                    Dim aSqlText As String
                    '** Build the Sql String
                    If Me.BuildTextRequired Then
                        aSqlText = Me.BuildSqlText()
                    Else
                        aSqlText = Me.SqlText
                    End If
                    '**
                    If String.IsNullOrWhiteSpace(aSqlText) Then
                        Call CoreMessageHandler(message:="No SQL statement could be build", argument:=Me.ID, _
                                               procedure:="ormSqlCommand.Prepare", _
                                               messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                    'DatabaseDriver.StoreSqlCommand(me)
                    aNativeCommand = _databaseDriver.CreateNativeDBCommand(aSqlText, aNativeConnection)
                    Me.NativeCommand = aNativeCommand
                    '** prepare
                    aNativeCommand.CommandText = aSqlText
                    If aNativeCommand.Connection Is Nothing Then
                        aNativeCommand.Connection = aNativeConnection
                    End If

                    aNativeCommand.CommandType = Data.CommandType.Text
                    '** add Parameter
                    For Each aParameter In Me.Parameters
                        '** add Column Parameter

                        If Not aParameter.NotColumn And aParameter.TableID <> String.empty And aParameter.ColumnName <> String.empty Then
                            aTablestore = _databaseDriver.GetTableStore(aParameter.TableID)
                            If Not aTablestore.ContainerSchema.IsInitialized Then
                                Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", containerID:=aParameter.TableID, _
                                                       message:="couldnot initialize table schema")
                                Return False
                            End If
                            Dim aNativeParameter As System.Data.IDbDataParameter = _
                                aTablestore.ContainerSchema.AssignNativeDBParameter(columnname:=aParameter.ColumnName, parametername:=aParameter.ID)
                            If Not aParameter Is Nothing Then aNativeCommand.Parameters.Add(aNativeParameter)
                        ElseIf aParameter.NotColumn Then
                            Dim aNativeParameter As System.Data.IDbDataParameter = _
                               _databaseDriver.AssignNativeDBParameter(parametername:=aParameter.ID, datatype:=aParameter.Datatype)
                            If Not aParameter Is Nothing Then aNativeCommand.Parameters.Add(aNativeParameter)
                        Else
                            Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", argument:=aParameter.ID, message:="Tablename missing", _
                                                  entryname:=aParameter.ColumnName, messagetype:=otCoreMessageType.InternalError)
                        End If
                    Next
                    '** prepare the native
                    aNativeCommand.Prepare()
                    Me._Prepared = True
                    '** initial values
                    aTablestore = Nothing ' reset
                    For Each aParameter In Me.Parameters
                        If aParameter.ColumnName <> String.empty And aParameter.TableID <> String.empty Then
                            If aTablestore Is Nothing OrElse aTablestore.ContainerID <> aParameter.TableID Then
                                aTablestore = _databaseDriver.GetTableStore(aParameter.TableID)
                            End If
                            If Not aTablestore.Convert2ContainerData(aParameter.ColumnName, invalue:=aParameter.Value, outvalue:=cvtvalue) Then
                                Call CoreMessageHandler(message:="parameter value could not be converted", containerEntryName:=aParameter.ColumnName, _
                                                        entryname:=aParameter.ID, argument:=aParameter.Value, messagetype:=otCoreMessageType.InternalError, _
                                                        procedure:="ormSqlCommand.Prepare")
                            End If
                        Else
                            cvtvalue = aParameter.Value
                        End If
                        If aNativeCommand.Parameters.Contains(aParameter.ID) Then
                            aNativeCommand.Parameters(aParameter.ID).value = cvtvalue
                        Else
                            Call CoreMessageHandler(message:="Parameter ID is not in native sql command", entryname:=aParameter.ID, argument:=Me.ID, _
                                                   messagetype:=otCoreMessageType.InternalError, procedure:="ormSqlCommand.Prepare")

                        End If

                    Next

                    Return True

                Catch ex As OleDb.OleDbException
                    Me._Prepared = False
                    Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", message:="Exception", argument:=Me.ID, _
                                           exception:=ex, messagetype:=otCoreMessageType.InternalException)
                    Return False
                Catch ex As Exception
                    Me._Prepared = False
                    Call CoreMessageHandler(procedure:="ormSqlCommand.Prepare", message:="Exception", argument:=Me.ID, _
                                           exception:=ex, messagetype:=otCoreMessageType.InternalException)
                    Return False
                End Try




            End Function
            ''' <summary>
            ''' increase the buildVersion
            ''' </summary>
            ''' <returns>the new build version</returns>
            ''' <remarks></remarks>
            Protected Function IncBuildVersion() As UShort
                Return (_buildVersion = _buildVersion + 1)
            End Function
            ''' <summary>
            ''' Run the Sql Select Statement and returns a List of ormRecords
            ''' </summary>
            ''' <param name="parameters">parameters of value</param>
            ''' <param name="connection">a optional native connection</param>
            ''' <returns>list of ormRecords (might be empty)</returns>
            ''' <remarks></remarks>
            Public Function Run(Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                               Optional nativeConnection As Object = Nothing) As Boolean
                '** set the parameters value to current command parameters value 
                '** if not specified
                Dim aParametervalues As Dictionary(Of String, Object)
                If parametervalues Is Nothing Then
                    aParametervalues = _parametervalues
                Else
                    aParametervalues = parametervalues
                End If

                ''' if we are running on one table only with all fields
                ''' then use the tablestore select with type checking

                ''' else run against the database driver
                ''' 
                '*** run it 
                If Me.IsPrepared Then
                    Return Me.DatabaseDriver.RunSqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                Else
                    If Me.Prepare() Then
                        Return Me.DatabaseDriver.RunSqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                    Else
                        Call CoreMessageHandler(procedure:="clsOTDBSqlSelectCommand.run", message:="Command is not prepared", argument:=Me.ID, _
                                                         messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                End If

            End Function
        End Class



        '*******************************************************************************************
        '***** CLASS clsOTDBStoreParameter  defines a Parameter for SQL Commands
        '*****
        ''' <summary>
        ''' Parameter definition for a SQL Command
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ormSqlCommandParameter

            Private _ID As String = String.empty
            Private _NotColumn As Boolean = False
            Private _tablename As String = Nothing
            Private _columname As String = Nothing
            Private _datatype As otDataType = 0
            Private _value As Object

            ''' <summary>
            ''' constructor for a Sql Command parameter
            ''' </summary>
            ''' <param name="ID">the ID in the sql statement</param>
            ''' <param name="datatype">datatype </param>
            ''' <param name="fieldname">fieldname </param>
            ''' <param name="tablename">tablename</param>
            ''' <param name="value"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal ID As String, _
                           Optional datatype As otDataType = 0, _
                           Optional columnname As String = Nothing, _
                           Optional tableid As String = Nothing, _
                           Optional value As Object = Nothing, _
                           Optional notColumn As Boolean = False)
                _ID = Regex.Replace(ID, "\s", String.Empty) ' no white chars allowed
                _datatype = datatype
                If Not String.IsNullOrWhiteSpace(columnname) Then _columname = columnname.ToUpper
                If Not String.IsNullOrWhiteSpace(TableID) Then _tablename = TableID.ToUpper
                If Not value Is Nothing Then _value = value
                _NotColumn = notColumn
            End Sub
            ''' <summary>
            ''' Gets or sets the not column.
            ''' </summary>
            ''' <value>The not column.</value>
            Public Property NotColumn() As Boolean
                Get
                    Return Me._NotColumn
                End Get
                Set(value As Boolean)
                    Me._NotColumn = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the value.
            ''' </summary>
            ''' <value>The value.</value>
            Public Property Value() As Object
                Get
                    Return Me._value
                End Get
                Set(value As Object)
                    Me._value = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the datatype.
            ''' </summary>
            ''' <value>The datatype.</value>
            Public Property Datatype() As otDataType
                Get
                    Return Me._datatype
                End Get
                Set(value As otDataType)
                    Me._datatype = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the fieldname.
            ''' </summary>
            ''' <value>The fieldname.</value>
            Public Property ColumnName() As String
                Get
                    Return Me._columname
                End Get
                Set(value As String)
                    Me._columname = value.ToUpper
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the fieldname.
            ''' </summary>
            ''' <value>The fieldname.</value>
            Public Property TableID() As String
                Get
                    Return Me._tablename
                End Get
                Set(value As String)
                    Me._tablename = value.ToUpper
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the ID.
            ''' </summary>
            ''' <value>The name.</value>
            Public Property ID() As String
                Get
                    Return Me._ID
                End Get
                Set(value As String)
                    Me._ID = Regex.Replace(ID, "\s", String.empty) ' no white chars allowed
                End Set
            End Property

        End Class

        '************************************************************************************
        '*****  CLASS clsOTDBSelectCommand 
        '***** 
        '*****
        Public Enum ormSelectResultFieldType
            TableField
            InLineFunction
        End Enum
        ''' <summary>
        '''  a flexible Select Command
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ormSqlSelectCommand
            Inherits ormSqlCommand
            Implements iormSqlCommand

            'Private _tablestores As New Dictionary(Of String, iOTDBTableStore) 'store the used Tablestores
            Private _fields As New Dictionary(Of String, ormResultField)

            Private _select As String = String.empty
            Private _innerjoin As String = String.empty
            Private _orderby As String = String.empty
            Private _where As String = String.empty
            Private _AllFieldsAdded As Boolean



            ''' <summary>
            ''' Class for Storing the select result fields per Table(store)
            ''' </summary>
            ''' <remarks></remarks>
            Public Class ormResultField
                Implements IHashCodeProvider

                Private _myCommand As ormSqlSelectCommand ' Backreference
                Private _name As String
                Private _tablestore As iormRelationalTableStore
                Private _type As ormSelectResultFieldType


                ''' <summary>
                ''' constructs a new Result field for command
                ''' </summary>
                ''' <param name="aCommand"></param>
                ''' <remarks></remarks>
                Public Sub New(command As ormSqlSelectCommand)
                    _myCommand = command
                End Sub
                ''' <summary>
                ''' constructs a new resultfield for command 
                ''' </summary>
                ''' <param name="aCommand"></param>
                ''' <param name="tableid"></param>
                ''' <param name="fieldname"></param>
                ''' <remarks></remarks>
                Public Sub New(command As ormSqlSelectCommand, tableid As String, fieldname As String)
                    _myCommand = command
                    Me.[TableID] = tableid
                    _name = fieldname
                End Sub
                ''' <summary>
                ''' Gets or sets the name.
                ''' </summary>
                ''' <value>The name.</value>
                Public Property Name() As String
                    Get
                        Return Me._name
                    End Get
                    Set(value As String)
                        Me._name = value
                    End Set
                End Property
                ''' <summary>
                ''' Gets or sets the name.
                ''' </summary>
                ''' <value>The name.</value>
                Public Property [Type]() As ormSelectResultFieldType
                    Get
                        Return Me._type
                    End Get
                    Set(value As ormSelectResultFieldType)
                        Me._type = value
                    End Set
                End Property
                ''' <summary>
                ''' Gets or sets the Tablestore used
                ''' </summary>
                ''' <value>The name.</value>
                Public Property [Tablestore]() As iormRelationalTableStore
                    Get
                        Return Me._tablestore
                    End Get
                    Set(value As iormRelationalTableStore)
                        Me._tablestore = value
                        If _myCommand.DatabaseDriver Is Nothing Then
                            _myCommand.DatabaseDriver = value.Connection.DatabaseDriver
                        End If
                    End Set
                End Property

                ''' <summary>
                ''' returns the nativetablename if a tablestore is set
                ''' </summary>
                ''' <value></value>
                ''' <returns></returns>
                ''' <remarks></remarks>
                Public ReadOnly Property [NativeTablename] As String
                    Get
                        If _tablestore IsNot Nothing Then
                            Return _tablestore.NativeDBObjectname
                        End If
                        Return String.Empty
                    End Get
                End Property
                ''' <summary>
                ''' Gets or sets the Tablestore / Tablename.
                ''' </summary>
                ''' <value>The name.</value>
                Public Property [TableID]() As String
                    Get
                        If _tablestore Is Nothing Then
                            Return String.Empty
                        Else
                            Return _tablestore.ContainerID
                        End If

                    End Get
                    Set(value As String)
                        Dim aTablestore As iormRelationalTableStore
                        '** set it to current connection 
                        If Not _myCommand.DatabaseDriver Is Nothing Then
                            _myCommand.DatabaseDriver = ot.CurrentConnection.DatabaseDriver
                        End If
                        ' retrieve the tablestore
                        If Not _myCommand._tablestores.ContainsKey(key:=value) Then
                            ' add it
                            aTablestore = Me._myCommand.DatabaseDriver.GetTableStore(tableID:=value)
                            If aTablestore IsNot Nothing Then
                                _myCommand._tablestores.Add(key:=aTablestore.ContainerID, value:=aTablestore)
                            End If
                        Else
                            aTablestore = _myCommand._tablestores.Item(value)
                        End If
                        _tablestore = aTablestore ' set it
                    End Set
                End Property


                ''' <summary>
                ''' Returns a hash code for the specified object.
                ''' </summary>
                ''' <param name="obj">The <see cref="T:System.Object" /> for which a hash code is
                ''' to be returned.</param>
                ''' <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" />
                ''' is a reference type and <paramref name="obj" /> is null. </exception>
                ''' <returns>A hash code for the specified object.</returns>
                Public Function GetHashCode(obj As Object) As Integer Implements IHashCodeProvider.GetHashCode
                    Return (Me.[TableID] & _name).GetHashCode
                End Function

            End Class

            ''' <summary>
            ''' Constructor of the OTDB Select command
            ''' </summary>
            ''' <param name="ID">the unique ID to store it</param>
            ''' <remarks></remarks>
            Public Sub New(ID As String)
                Call MyBase.New(ID:=ID)
                _type = otSQLCommandTypes.SELECT
            End Sub
            ''' <summary>
            ''' Gets the completefor object.
            ''' </summary>
            ''' <value>The completefor object.</value>
            Public ReadOnly Property AllFieldsAdded() As Boolean
                Get
                    Return Me._AllFieldsAdded
                End Get
            End Property
            ''' <summary>
            ''' sets or gets the innerjoin 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property InnerJoin As String
                Get
                    Return _innerjoin
                End Get
                Set(value As String)
                    _innerjoin = value
                    Me.BuildTextRequired = True
                End Set
            End Property
            ''' <summary>
            '''  sets the select part of an Sql Select without SELECT Keyword
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property [select] As String
                Get
                    Return _select
                End Get
                Set(value As String)
                    _select = value
                    Me.BuildTextRequired = True

                End Set
            End Property
            ''' <summary>
            ''' set or gets the orderby string
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property OrderBy As String
                Get
                    Return _orderby
                End Get
                Set(value As String)
                    _orderby = value
                    Me.BuildTextRequired = True
                End Set
            End Property
            ''' <summary>
            ''' sets or gets the wherestr
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property Where As String
                Get
                    Return _where
                End Get
                Set(value As String)

                    _where = value
                    Me.BuildTextRequired = True
                End Set
            End Property

            ''' <summary>
            ''' Add Table with fields to the Resultfields
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function AddTable(tableid As String, addAllFields As Boolean, Optional addFieldnames As List(Of String) = Nothing) As Boolean
                Dim aTablestore As iormRelationalTableStore
                tableid = tableid.ToUpper
                If Me._databaseDriver Is Nothing Then
                    aTablestore = GetTableStore(tableid:=tableid)
                    If aTablestore Is Nothing Then
                        Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", containerID:=tableid, procedure:="clsOTDBSelectCommand.ADDTable", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        Me.DatabaseDriver = aTablestore.Connection.DatabaseDriver
                    End If
                Else
                    aTablestore = _databaseDriver.GetTableStore(tableID:=tableid)
                End If


                If aTablestore Is Nothing Then
                    Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", containerID:=tableid, procedure:="clsOTDBSelectCommand.ADDTable", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                If Not _tablestores.ContainsKey(key:=tableid) Then
                    _tablestores.Add(key:=tableid, value:=aTablestore)
                End If

                '*** include all fields
                If addAllFields Then
                    For Each aFieldname As String In aTablestore.ContainerSchema.EntryNames
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname.ToUpper) Then
                            _fields.Add(key:=tableid & "." & aFieldname.ToUpper, value:=New ormResultField(Me, tableid:=tableid, fieldname:=aFieldname.ToUpper))
                        End If
                    Next
                    _AllFieldsAdded = True
                End If

                '** include specific fields
                If Not addFieldnames Is Nothing Then
                    For Each aFieldname As String In addFieldnames
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname.ToUpper) Then
                            _fields.Add(key:=tableid & "." & aFieldname, value:=New ormResultField(Me, tableid:=tableid, fieldname:=aFieldname.ToUpper))
                        End If
                    Next
                End If

                Return True
            End Function
            ''' <summary>
            ''' builds the SQL text for the Command
            ''' </summary>
            ''' <returns>True if successfull </returns>
            ''' <remarks></remarks>
            Public Overrides Function BuildSqlText() As String
                Me._SqlText = "SELECT "
                Dim aTableList As New List(Of String)
                Dim first As Boolean = True

                '** fill tables first 
                For Each atableid In _tablestores.Keys
                    'Dim aTablename = kvp.Key
                    If Not aTableList.Contains(atableid) Then
                        aTableList.Add(atableid)
                    End If
                Next

                '*** build the result list
                If String.IsNullOrWhiteSpace(_select) Then
                    first = True
                    '*
                    For Each aResultField In _fields.Values
                        Dim aTablename = aResultField.[TableID]
                        If Not String.IsNullOrWhiteSpace(aTablename) Then
                            If Not aTableList.Contains(aTablename) Then aTableList.Add(aTablename)

                            If Not first Then Me._SqlText &= ","
                            Me._SqlText &= "[" & aResultField.NativeTablename & "].[" & aResultField.Name & "] "
                        Else
                            Me._SqlText &= "[" & aResultField.Name & "] "
                        End If

                        first = False
                    Next

                    If aTableList.Count = 0 Then
                        Call CoreMessageHandler(message:="no table and no fields in sql statement", procedure:="clsOTDBSqlSelectCommand.BuildSqlText", _
                                               argument:=Me.ID, messagetype:=otCoreMessageType.InternalError)
                        Me.BuildTextRequired = True
                        Return String.Empty
                    End If
                Else
                    ''' TODO: add the additional parameter sql text
                    ''' and keep allfieldsadded
                    Me._SqlText &= _select
                    If _AllFieldsAdded Then _AllFieldsAdded = False ' reset the allfieldsadded in any case
                End If

                '*** build the tables
                first = True
                Me._SqlText &= " FROM "
                For Each aTableID In aTableList

                    '** if innerjoin has the tablename
                    If String.IsNullOrWhiteSpace(_innerjoin) OrElse _
                        (Not String.IsNullOrWhiteSpace(_innerjoin) AndAlso Not _innerjoin.ToUpper.Contains(aTableID)) Then
                        If Not first Then
                            Me._SqlText &= ","
                        End If
                        Me._SqlText &= "[" & Me.DatabaseDriver.GetNativeDBObjectName(aTableID) & "]"
                        first = False
                    End If
                Next

                '*** innerjoin
                If Not String.IsNullOrWhiteSpace(_innerjoin) Then
                    If Not _innerjoin.ToLower.Contains("join") Then
                        Me._SqlText &= " INNER JOIN "
                    End If
                    _SqlText &= _innerjoin
                End If

                '*** where 
                If _where <> String.empty Then
                    If Not _where.ToLower.Contains("where") Then
                        Me._SqlText &= " WHERE "
                    End If
                    _SqlText &= _where
                End If

                '*** order by 
                If _orderby <> String.empty Then
                    If Not _where.ToLower.Contains("order by") Then
                        Me._SqlText &= " ORDER BY "
                    End If
                    Me._SqlText &= _orderby
                End If

                '*
                IncBuildVersion()
                Me.BuildTextRequired = False
                '*
                Return Me._SqlText
            End Function
            ''' <summary>
            ''' Run the Sql Select Statement and returns a List of ormRecords
            ''' </summary>
            ''' <param name="parameters">parameters of value</param>
            ''' <param name="connection">a optional native connection</param>
            ''' <returns>list of ormRecords (might be empty)</returns>
            ''' <remarks></remarks>
            Public Function RunSelect(Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                      Optional nativeConnection As Object = Nothing) As List(Of ormRecord)
                '** set the parameters value to current command parameters value 
                '** if not specified
                Dim aParametervalues As Dictionary(Of String, Object)
                If parametervalues Is Nothing Then
                    aParametervalues = _parametervalues
                Else
                    aParametervalues = parametervalues
                End If

                ''' if we are running on one table only with all fields
                ''' then use the tablestore select with type checking
                If _tablestores.Count = 1 And _AllFieldsAdded Then
                    Dim aStore As iormRelationalTableStore = _tablestores.Values.First
                    '*** run it
                    If Me.IsPrepared Then
                        Return aStore.GetRecordsBySqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues)
                    Else
                        If Me.Prepare() Then
                            Return aStore.GetRecordsBySqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues)
                        Else
                            Call CoreMessageHandler(procedure:="clsOTDBSqlSelectCommand.runSelect", message:="Command is not prepared", argument:=Me.ID, _
                                                             messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of ormRecord)
                        End If
                    End If
                Else
                    ''' else run against the database driver
                    ''' 
                    '*** run it
                    If Me.IsPrepared Then
                        Return Me.DatabaseDriver.RunSqlSelectCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                    Else
                        If Me.Prepare() Then
                            Return Me.DatabaseDriver.RunSqlSelectCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                        Else
                            Call CoreMessageHandler(procedure:="clsOTDBSqlSelectCommand.runSelect", message:="Command is not prepared", argument:=Me.ID, _
                                                             messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of ormRecord)
                        End If
                    End If
                End If

            End Function
        End Class


        ''' <summary>
        ''' abstract ORM Driver class for Relational Database Drivers
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormRDBDriver
            Implements iormRelationalDatabaseDriver

            Protected _ID As String
            Protected _TableDirectory As New Dictionary(Of String, iormRelationalTableStore)    'Table Directory of TableStored
            Protected _ViewDirectory As New Dictionary(Of String, iormRelationalTableStore)    'view Directory of TableStore
            Protected _TableSchemaDirectory As New Dictionary(Of String, iormContainerSchema)    'Table Directory of container schema
            Protected _ViewSchemaDirectory As New Dictionary(Of String, iormContainerSchema)    'view Directory of container schema


            Protected WithEvents _primaryConnection As iormConnection ' primary connection
            Protected WithEvents _session As Session
            Protected _CommandStore As New Dictionary(Of String, iormSqlCommand) ' store of the SqlCommands to handle

            Protected _lockObject As New Object 'Lock object instead of me

            ''' <summary>
            ''' Const
            ''' </summary>
            ''' <remarks></remarks>
            Public Const ConstDBParameterTableName As String = "TBLOTDBPARAMETERS"
            Public Const ConstOLDParameterTableName As String = "TBLDBPARAMETERS" 'Legacy Parameter Table w/o Application ID
            '** Field names of parameter table
            Public Const ConstFNSetupID = "SETUP"
            Public Const ConstFNID = "ID"
            Public Const ConstFNValue = "VALUE"
            Public Const ConstFNChangedOn = "CHANGEDON"
            Public Const constFNDescription = "DESCRIPTION"
            '* the events
            Public Event RequestBootstrapInstall(sender As Object, e As SessionBootstrapEventArgs) Implements iormPrimaryDriver.RequestBootstrapInstall
#Region "Properties"

            ''' <summary>
            ''' return true if driver is supporting a relational database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property IsRelationalDriver As Boolean Implements iormDatabaseDriver.IsRelationalDriver
                Get
                    Return True
                End Get
            End Property

            ''' <summary>
            ''' return true if driver is supporting  hosting an OnTrack database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property IsPrimaryDriver As Boolean Implements iormDatabaseDriver.IsPrimaryDriver
                Get
                    Return True
                End Get
            End Property
            ''' <summary>
            ''' Gets the session.
            ''' </summary>
            ''' <value>The session.</value>
            Public ReadOnly Property Session() As Session
                Get
                    Return Me._session
                End Get
            End Property

            ''' <summary>
            ''' Returns the Parameter Tablename
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride ReadOnly Property DBParameterTablename As String Implements iormRelationalDatabaseDriver.DBParameterContainerName

            ''' <summary>
            ''' returns the OTDBServertype
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property DatabaseType As otDBServerType Implements iormRelationalDatabaseDriver.DatabaseType
                Get
                    If _primaryConnection Is Nothing Then
                        Return 0
                    Else
                        Return _primaryConnection.Databasetype
                    End If

                End Get
            End Property
            '' <summary>
            ''' Gets the type.
            ''' </summary>
            ''' <value>The type.</value>
            Public MustOverride ReadOnly Property Type() As otDbDriverType Implements iormRelationalDatabaseDriver.Type

            ''' <summary>
            ''' Gets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Overridable Property ID() As String Implements iormRelationalDatabaseDriver.ID
                Set(value As String)
                    _ID = value
                End Set
                Get
                    Return _ID
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the table schema directory.
            ''' </summary>
            ''' <value>The table schema directory.</value>
            Public Property TableSchemaDirectory() As Dictionary(Of String, iormContainerSchema)
                Get
                    Return Me._TableSchemaDirectory
                End Get
                Set(value As Dictionary(Of String, iormContainerSchema))
                    Me._TableSchemaDirectory = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the table directory.
            ''' </summary>
            ''' <value>The table directory.</value>
            Public Property TableDirectory() As Dictionary(Of String, iormRelationalTableStore)
                Get
                    Return Me._TableDirectory
                End Get
                Set(value As Dictionary(Of String, iormRelationalTableStore))
                    Me._TableDirectory = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the connection.
            ''' </summary>
            ''' <value>The connection.</value>
            Public Overridable ReadOnly Property CurrentConnection() As iormConnection Implements iormRelationalDatabaseDriver.CurrentConnection
                Get
                    Return _primaryConnection
                End Get

            End Property
#End Region
            ''' <summary>
            ''' constructor
            ''' </summary>
            ''' <param name="session"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal id As String, ByRef session As Session)
                _ID = id
                _session = session

            End Sub

            ''' <summary>
            ''' checks if SqlCommand is in Store of the driver
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <remarks></remarks>
            ''' <returns>True if successful</returns>
            Public Function HasSqlCommand(id As String) As Boolean Implements iormRelationalDatabaseDriver.HasSqlCommand
                Return _CommandStore.ContainsKey(key:=id)
            End Function

            ''' <summary>
            ''' Store the Command by its ID - replace if existing
            ''' </summary>
            ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
            ''' <remarks></remarks>
            ''' <returns>true if successful</returns>
            Public Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean Implements iormRelationalDatabaseDriver.StoreSqlCommand
                If _CommandStore.ContainsKey(key:=sqlCommand.ID) Then
                    _CommandStore.Remove(key:=sqlCommand.ID)
                End If
                _CommandStore.Add(key:=sqlCommand.ID, value:=sqlCommand)
                Return True
            End Function

            ''' <summary>
            ''' Retrieve the Command from Store
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <remarks></remarks>
            ''' <returns>a iOTDBSqlCommand</returns>
            Public Function RetrieveSqlCommand(id As String) As iormSqlCommand Implements iormRelationalDatabaseDriver.RetrieveSqlCommand
                If _CommandStore.ContainsKey(key:=id) Then
                    Return _CommandStore.Item(key:=id)
                End If

                Return Nothing
            End Function
            ''' <summary>
            ''' Creates a Command and store it or gets the current Command
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>a iOTDBSqlCommand</returns>
            ''' <remarks></remarks>
            Public Overridable Function CreateSqlCommand(id As String) As iormSqlCommand Implements iormRelationalDatabaseDriver.CreateSqlCommand
                '* get the ID

                If Me.HasSqlCommand(id) Then
                    Return Me.RetrieveSqlCommand(id)
                Else
                    Dim aSqlCommand As iormSqlCommand = New ormSqlCommand(id)
                    Me.StoreSqlCommand(aSqlCommand)
                    Return aSqlCommand
                End If
            End Function
            ''' <summary>
            ''' Creates a Command and store it or gets the current Command
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>a iOTDBSqlCommand</returns>
            ''' <remarks></remarks>
            Public Overridable Function CreateSqlSelectCommand(id As String) As iormSqlCommand Implements iormRelationalDatabaseDriver.CreateSqlSelectCommand
                '* get the ID

                If Me.HasSqlCommand(id) Then
                    Return Me.RetrieveSqlCommand(id)
                Else
                    Dim aSqlCommand As iormSqlCommand = New ormSqlSelectCommand(id)
                    Me.StoreSqlCommand(aSqlCommand)
                    Return aSqlCommand
                End If
            End Function
            ''' <summary>
            ''' Register a connection at the Driver to be used
            ''' </summary>
            ''' <param name="connection">a iOTDBConnection</param>
            ''' <returns>true if successful</returns>
            ''' <remarks></remarks>
            Protected Overridable Function RegisterConnection(ByRef connection As iormConnection) As Boolean Implements iormDatabaseDriver.RegisterConnection
                If _primaryConnection Is Nothing Then
                    _primaryConnection = connection
                    Return True
                Else
                    Return False
                End If
            End Function
            ''' <summary>
            ''' Handles the onDisconnect Event of the Driver
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Function OnDisconnect() As Boolean Handles _primaryConnection.OnDisconnection
                _TableDirectory.Clear()
                _TableSchemaDirectory.Clear()
                Return True
            End Function

            ''' <summary>
            ''' installs the ONTrack Database Schema
            ''' </summary>
            ''' <param name="askBefore"></param>
            ''' <param name="modules"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function InstallOnTrackDatabase(askBefore As Boolean, modules As String()) As Boolean Implements iormPrimaryDriver.InstallOnTrackDatabase

            ''' <summary>
            ''' returns true if an OnTrack Admin User is available in the database
            ''' </summary>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasAdminUserValidation(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormPrimaryDriver.HasAdminUserValidation

            ''' <summary>
            ''' Gets or creates the foreign key for a columndefinition
            ''' </summary>
            ''' <param name="nativeTable">The native table.</param>
            ''' <param name="columndefinition">The columndefinition.</param>
            ''' <param name="createOrAlter">The create or alter.</param>
            ''' <param name="connection">The connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetForeignKeys(nativeTable As Object, foreignkeydefinition As ForeignKeyDefinition, Optional createOrAlter As Boolean = False, Optional ByRef connection As iormConnection = Nothing) As IEnumerable(Of Object) Implements iormDatabaseDriver.GetForeignKeys

            ''' <summary>
            ''' Creates the global domain.
            ''' </summary>
            ''' <param name="nativeConnection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function CreateGlobalDomain(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormPrimaryDriver.CreateGlobalDomain



            ''' <summary>
            ''' verifyOnTrack
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyOnTrackDatabase(Optional modules As String() = Nothing, Optional install As Boolean = False, Optional verifySchema As Boolean = False) As Boolean Implements iormPrimaryDriver.VerifyOnTrackDatabase


            ''' <summary>
            ''' create an assigned Native DBParameter to provided name and type
            ''' </summary>
            ''' <param name="parametername"></param>
            ''' <param name="datatype"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function AssignNativeDBParameter(parametername As String, datatype As otDataType, _
                                                                  Optional maxsize As Long = 0, _
                                                                 Optional value As Object = Nothing) As System.Data.IDbDataParameter Implements iormRelationalDatabaseDriver.AssignNativeDBParameter

            ''' <summary>
            ''' returns the target type for a OTDB FieldType - MAPPING
            ''' </summary>
            ''' <param name="type"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function GetTargetTypeFor(type As otDataType) As Long Implements iormDatabaseDriver.GetTargetTypeFor
            '
            ''' <summary>
            '''  converts value to targetType of the native DB Driver
            ''' </summary>
            ''' <param name="value"></param>
            ''' <param name="targetType"></param>
            ''' <param name="maxsize"></param>
            ''' <param name="abostrophNecessary"></param>
            ''' <param name="fieldname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function Convert2DBData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                        targetType As Long, _
                                                        Optional ByVal maxsize As Long = 0, _
                                                       Optional ByRef abostrophNecessary As Boolean = False, _
                                                       Optional ByVal fieldname As String = Nothing, _
                                                       Optional isnullable As Boolean = False,
                                                        Optional defaultvalue As Object = Nothing) As Boolean Implements iormDatabaseDriver.Convert2DBData

            ''' <summary>
            ''' Runs the SQL select command.
            ''' </summary>
            ''' <param name="sqlcommand">The sqlcommand.</param>
            ''' <param name="parametervalues">The parametervalues.</param>
            ''' <param name="nativeConnection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function RunSqlCommand(ByRef sqlcommand As ormSqlCommand, _
                                                       Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                       Optional nativeConnection As Object = Nothing) As Boolean Implements iormRelationalDatabaseDriver.RunSqlCommand


            ''' <summary>
            ''' Convert2s the object data.
            ''' </summary>
            ''' <param name="invalue">The invalue.</param>
            ''' <param name="outvalue">The outvalue.</param>
            ''' <param name="sourceType">Type of the source.</param>
            ''' <param name="isnullable">The isnullable.</param>
            ''' <param name="defaultvalue">The defaultvalue.</param>
            ''' <param name="abostrophNecessary">The abostroph necessary.</param>
            ''' <returns></returns>
            Public MustOverride Function Convert2ObjectData(invalue As Object, _
                                                            ByRef outvalue As Object, _
                                                            sourceType As Long, _
                                                            Optional isnullable As Boolean? = Nothing, _
                                                            Optional defaultvalue As Object = Nothing, _
                                                            Optional ByRef abostrophNecessary As Boolean = False) As Boolean Implements iormDatabaseDriver.Convert2ObjectData


            ''' <summary>
            ''' returns True if data store has the table
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasTable(tableid As String, _
                                                  Optional ByRef connection As iormConnection = Nothing, _
                                                  Optional nativeConnection As Object = Nothing) As Boolean Implements iormRelationalDatabaseDriver.HasTable, iormDatabaseDriver.HasContainerID

            ''' <summary>
            ''' returns True if data store has the table by definition
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyTableSchema(tabledefinition As ContainerDefinition, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormRelationalDatabaseDriver.VerifyTableSchema

            ''' <summary>
            ''' returns True if data store has the table attribute
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyTableSchema(tableattribute As ormTableAttribute, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormRelationalDatabaseDriver.VerifyContainerSchema

            ''' <summary>
            ''' returns True if data store has the table attribute
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyContainerSchema(containerAttribute As iormContainerAttribute, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.VerifyContainerSchema


            ''' <summary>
            ''' Gets, creates or alters the table.
            ''' </summary>
            ''' <param name="tableid">The ot tableid.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetTable(tableid As String, _
                            Optional createOrAlter As Boolean = False, _
                            Optional ByRef connection As iormConnection = Nothing, _
                             Optional ByRef nativeTableObject As Object = Nothing) As Object Implements iormRelationalDatabaseDriver.GetTable, iormDatabaseDriver.GetContainerObject

            ''' <summary>
            ''' drops a table in the database by id
            ''' </summary>
            ''' <param name="id"></param>
            ''' <param name="connection"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function DropTable(id As String, _
                                                   Optional ByRef connection As iormConnection = Nothing) As Boolean Implements iormDatabaseDriver.DropContainerObject, iormRelationalDatabaseDriver.DropTable

            ''' <summary>
            ''' returns true if the datastore has the view by viewname
            ''' </summary>
            ''' <param name="name"></param>
            ''' <param name="connection"></param>
            ''' <param name="nativeConnection"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function HasView(viewid As String, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormRelationalDatabaseDriver.HasView


            ''' <summary>
            ''' returns or creates a View in the data store
            ''' </summary>
            ''' <param name="name"></param>
            ''' <param name="sqlselect"></param>
            ''' <param name="createOrAlter"></param>
            ''' <param name="connection"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function GetView(viewid As String, Optional sqlselect As String = Nothing, Optional createOrAlter As Boolean = False, Optional ByRef connection As iormConnection = Nothing) As Object Implements iormRelationalDatabaseDriver.GetView


            ''' <summary>
            ''' drops a view by id
            ''' </summary>
            ''' <param name="id"></param>
            ''' <param name="connection"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function DropView(id As String, Optional ByRef connection As iormConnection = Nothing) As Boolean Implements iormRelationalDatabaseDriver.DropView


            ''' <summary>
            ''' Gets the index.
            ''' </summary>
            ''' <param name="nativeTABLE">The native TABLE.</param>
            ''' <param name="indexname">The indexname.</param>
            ''' <param name="ColumnNames">The column names.</param>
            ''' <param name="PrimaryKey">The primary key.</param>
            ''' <param name="forceCreation">The force creation.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <returns></returns>

            Public MustOverride Function GetIndex(ByRef nativeTable As Object, ByRef indexdefinition As IndexDefinition, _
                                                   Optional ByVal forceCreation As Boolean = False, _
                                                   Optional ByVal createOrAlter As Boolean = False, _
                                                   Optional ByRef connection As iormConnection = Nothing) As Object Implements iormRelationalDatabaseDriver.GetIndex

            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasColumn(tablename As String, _
                                                   columnname As String, _
                                                   Optional ByRef connection As iormConnection = Nothing) As Boolean Implements iormRelationalDatabaseDriver.HasColumn, iormDatabaseDriver.HasContainerEntryID
            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyColumnSchema(containerEntryDefinition As ContainerEntryDefinition, _
                                                            Optional ByRef connection As iormConnection = Nothing, _
                                                            Optional silent As Boolean = False) As Boolean Implements iormRelationalDatabaseDriver.VerifyColumnSchema, iormDatabaseDriver.VerifyContainerEntrySchema

            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyColumnSchema(attribute As iormContainerEntryAttribute, _
                                                            Optional ByRef connection As iormConnection = Nothing, _
                                                            Optional silent As Boolean = False) As Boolean Implements iormRelationalDatabaseDriver.VerifyColumnSchema, iormDatabaseDriver.VerifyContainerEntrySchema

            ''' <summary>
            ''' Gets the column.
            ''' </summary>
            ''' <param name="nativeTABLE">The native TABLE.</param>
            ''' <param name="aDBDesc">A DB desc.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <returns></returns>
            Public MustOverride Function GetColumn(nativeTable As Object, _
                                                   columndefinition As ContainerEntryDefinition, _
                                                   Optional createOrAlter As Boolean = False, _
                                                   Optional ByRef connection As iormConnection = Nothing) As Object Implements iormRelationalDatabaseDriver.GetColumn, iormDatabaseDriver.GetContainerEntryObject


            ''' <summary>
            ''' Create the User Definition Table
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormPrimaryDriver.CreateDBUserDefTable

            ''' <summary>
            ''' create the DB Parameter Table
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormPrimaryDriver.CreateDBParameterContainer


            ''' <summary>
            ''' drops the DB parameter table - given with setup then just the setup related entries
            ''' if then there is no setup related entries at all -> drop the full table
            ''' </summary>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function DropDBParameterTable(Optional setup As String = Nothing, _
                                                              Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormPrimaryDriver.DropDBParameterContainer

            ''' <summary>
            ''' deletes a DB Parameter
            ''' </summary>
            ''' <param name="parametername"></param>
            ''' <param name="nativeConnection"></param>
            ''' <param name="silent"></param>
            ''' <param name="setupID"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function DeleteDBParameter(parametername As String, _
                                                           Optional ByRef nativeConnection As Object = Nothing, _
                                                           Optional silent As Boolean = False, _
                                                           Optional setupID As String = Nothing) As Boolean Implements iormPrimaryDriver.DeleteDBParameter


            ''' <summary>
            ''' Sets the DB parameter.
            ''' </summary>
            ''' <param name="Parametername">The parametername.</param>
            ''' <param name="Value">The value.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <param name="UpdateOnly">The update only.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function SetDBParameter(parametername As String, _
                                                        value As Object, _
                                                        Optional ByRef nativeConnection As Object = Nothing, _
                                                        Optional updateOnly As Boolean = False, _
                                                        Optional silent As Boolean = False, _
                                                        Optional setupID As String = Nothing, _
                                                        Optional description As String = Nothing) As Boolean Implements iormPrimaryDriver.SetDBParameter

            ''' <summary>
            ''' Gets the DB parameter.
            ''' </summary>
            ''' <param name="PARAMETERNAME">The PARAMETERNAME.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function GetDBParameter(parametername As String, _
                                                        Optional ByRef nativeConnection As Object = Nothing, _
                                                        Optional silent As Boolean = False, _
                                                        Optional setupID As String = Nothing) As Object Implements iormPrimaryDriver.GetDBParameter



            ''' <summary>
            ''' validates the User, Passoword, Access Right in the Domain
            ''' </summary>
            ''' <param name="username"></param>
            ''' <param name="password"></param>
            ''' <param name="accessright"></param>
            ''' <param name="domainID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function ValidateUser(ByVal username As String, _
                                         ByVal password As String, _
                                         ByVal accessRequest As otAccessRight, _
                                         Optional domainid As String = Nothing) As Boolean Implements iormPrimaryDriver.ValidateUser

                Dim aValidation As UserValidation
                aValidation.ValidEntry = False
                aValidation = GetUserValidation(username:=username)

                If Not aValidation.ValidEntry Then
                    Return False
                Else
                    ''' if validation has nothing then continiue with any password
                    ''' fail if provided password is nothing
                    If aValidation.Password IsNot Nothing AndAlso (password Is Nothing OrElse aValidation.Password <> password) Then
                        Return False
                    End If

                    '** check against the validation
                    Dim aAccessProperty As AccessRightProperty

                    If aValidation.ValidEntry Then
                        If aValidation.HasAlterSchemaRights Then
                            aAccessProperty = New AccessRightProperty(otAccessRight.AlterSchema)
                        ElseIf aValidation.HasUpdateRights Then
                            aAccessProperty = New AccessRightProperty(otAccessRight.ReadUpdateData)
                        ElseIf aValidation.HasReadRights Then
                            aAccessProperty = New AccessRightProperty(otAccessRight.ReadOnly)
                        Else
                            Return False 'return if no Right in the validation
                        End If
                    End If
                    ''' ToDo: forbidd access for domains
                    ''' 
                    ''' check if Rights are covered
                    Return aAccessProperty.CoverRights(accessRequest)
                End If

            End Function
            ''' <summary>
            ''' Gets the ontrack user validation object.
            ''' </summary>
            ''' <param name="Username">The username.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Protected Friend MustOverride Function GetUserValidation(username As String, _
                                                                     Optional ByVal selectAnonymous As Boolean = False, _
                                                                     Optional ByRef nativeConnection As Object = Nothing) As UserValidation Implements iormPrimaryDriver.GetUserValidation

            ''' <summary>
            ''' create a tablestore 
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableStore(ByVal tableID As String, ByVal forceSchemaReload As Boolean) As iormRelationalTableStore
            ''' <summary>
            ''' create a tableschema
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableSchema(ByVal tableID As String) As iormContainerSchema

            ''' <summary>
            ''' create a native view reader 
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeViewReader(ByVal viewID As String, ByVal forceSchemaReload As Boolean) As iormRelationalTableStore
            ''' <summary>
            ''' create native view schema object
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeViewSchema(ByVal viewID As String) As iormContainerSchema

            ''' <summary>
            ''' persists the errorlog
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function PersistLog(ByRef log As SessionMessageLog) As Boolean Implements iormPrimaryDriver.PersistLog
            ''' <summary>
            ''' Gets the data store which is the tablestore
            ''' </summary>
            ''' <param name="tableID">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function RetrieveContainerStore(ByVal containerid As String, Optional ByVal force As Boolean = False) As iormContainerStore Implements iormDatabaseDriver.RetrieveContainerStore
                Return Me.GetTableStore(containerid, force)
            End Function
            ''' <summary>
            ''' Gets the table store.
            ''' </summary>
            ''' <param name="tableID">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetTableStore(ByVal tableID As String, Optional ByVal force As Boolean = False) As iormRelationalTableStore Implements iormRelationalDatabaseDriver.GetTableStore
                'take existing or make new one
                If _TableDirectory.ContainsKey(tableID.ToUpper) And Not force Then
                    Return _TableDirectory.Item(tableID.ToUpper)
                Else
                    Dim aNewStore As iormRelationalTableStore

                    ' reload the existing object on force
                    If _TableDirectory.ContainsKey(tableID.ToUpper) Then
                        aNewStore = _TableDirectory.Item(tableID.ToUpper)
                        aNewStore.Refresh(force)
                        Return aNewStore
                    End If
                    ' assign the Table

                    aNewStore = CreateNativeTableStore(tableID.ToUpper, forceSchemaReload:=force)
                    If Not aNewStore Is Nothing Then
                        If Not _TableDirectory.ContainsKey(tableID.ToUpper) Then
                            _TableDirectory.Add(key:=tableID.ToUpper, value:=aNewStore)
                        End If
                    End If
                    ' return
                    Return aNewStore

                End If

            End Function

            ''' <summary>
            ''' Gets the table schema.
            ''' </summary>
            ''' <param name="Tablename">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetTableSchema(ByVal tableID As String, Optional ByVal force As Boolean = False) As iormContainerSchema Implements iormDatabaseDriver.RetrieveContainerSchema, iormRelationalDatabaseDriver.RetrieveTableSchema

                'take existing or make new one
                If _TableSchemaDirectory.ContainsKey(tableID.ToUpper) And Not force Then
                    Return _TableSchemaDirectory.Item(tableID.ToUpper)
                Else
                    Dim aNewSchema As iormContainerSchema

                    ' delete the existing object
                    If _TableSchemaDirectory.ContainsKey(tableID.ToUpper) Then
                        aNewSchema = _TableSchemaDirectory.Item(tableID.ToUpper)
                        SyncLock aNewSchema
                            If force Or Not aNewSchema.IsInitialized Then aNewSchema.Refresh(force)
                        End SyncLock
                        Return aNewSchema
                    End If
                    ' assign the Table
                    aNewSchema = CreateNativeTableSchema(tableID.ToUpper)

                    If Not aNewSchema Is Nothing Then
                        SyncLock _lockObject
                            _TableSchemaDirectory.Add(key:=tableID.ToUpper, value:=aNewSchema)
                        End SyncLock

                        If Not aNewSchema.IsInitialized Then
                            SyncLock aNewSchema
                                aNewSchema.Refresh(reloadForce:=force)
                            End SyncLock
                        End If
                    End If

                    ' return
                    Return aNewSchema
                End If

            End Function
            ''' <summary>
            ''' Gets the view reader
            ''' </summary>
            ''' <param name="tableID">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetViewReader(ByVal viewID As String, Optional ByVal force As Boolean = False) As iormRelationalTableStore Implements iormRelationalDatabaseDriver.GetViewReader
                'take existing or make new one
                If _ViewDirectory.ContainsKey(viewID.ToUpper) And Not force Then
                    Return _ViewDirectory.Item(viewID.ToUpper)
                Else
                    Dim aNewStore As iormRelationalTableStore

                    ' reload the existing object on force
                    If _ViewDirectory.ContainsKey(viewID.ToUpper) Then
                        aNewStore = _ViewDirectory.Item(viewID.ToUpper)
                        aNewStore.Refresh(force)
                        Return aNewStore
                    End If
                    ' assign the Table

                    aNewStore = CreateNativeViewReader(viewID.ToUpper, forceSchemaReload:=force)
                    If Not aNewStore Is Nothing Then
                        If Not _ViewDirectory.ContainsKey(viewID.ToUpper) Then
                            _ViewDirectory.Add(key:=viewID.ToUpper, value:=aNewStore)
                        End If
                    End If
                    ' return
                    Return aNewStore

                End If

            End Function
            ''' <summary>
            ''' Gets the view schema
            ''' </summary>
            ''' <param name="Tablename">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetViewSchema(ByVal viewID As String, Optional ByVal force As Boolean = False) As iormContainerSchema _
            Implements iormRelationalDatabaseDriver.GetViewSchema

                'take existing or make new one
                If _ViewSchemaDirectory.ContainsKey(viewID.ToUpper) And Not force Then
                    Return _ViewSchemaDirectory.Item(viewID.ToUpper)
                Else
                    Dim aNewSchema As iormContainerSchema

                    ' delete the existing object
                    If _ViewSchemaDirectory.ContainsKey(viewID.ToUpper) Then
                        aNewSchema = _ViewSchemaDirectory.Item(viewID.ToUpper)
                        SyncLock aNewSchema
                            If force Or Not aNewSchema.IsInitialized Then aNewSchema.Refresh(force)
                        End SyncLock
                        Return aNewSchema
                    End If
                    ' assign the Table
                    aNewSchema = CreateNativeViewSchema(viewID.ToUpper)

                    If Not aNewSchema Is Nothing Then
                        SyncLock _lockObject
                            _ViewSchemaDirectory.Add(key:=viewID.ToUpper, value:=aNewSchema)
                        End SyncLock

                        If Not aNewSchema.IsInitialized Then
                            SyncLock aNewSchema
                                aNewSchema.Refresh(reloadForce:=force)
                            End SyncLock
                        End If
                    End If

                    ' return
                    Return aNewSchema
                End If

            End Function
            ''' <summary>
            ''' Runs the SQL Command
            ''' </summary>
            ''' <param name="sqlcmdstr"></param>
            ''' <param name="parameters"></param>
            ''' <param name="silent"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                      Optional silent As Boolean = True, Optional nativeConnection As Object = Nothing) As Boolean _
                                                  Implements iormRelationalDatabaseDriver.RunSqlStatement


            ''' <summary>
            ''' Runs the SQL select command.
            ''' </summary>
            ''' <param name="sqlcommand">The sqlcommand.</param>
            ''' <param name="parameters">The parameters.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function RunSqlSelectCommand(ByRef sqlcommand As ormSqlSelectCommand, _
                                                Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                            Implements iormRelationalDatabaseDriver.RunSqlSelectCommand

            Public MustOverride Function RunSqlSelectCommand(id As String, _
                                                         Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                         Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                                       Implements iormRelationalDatabaseDriver.RunSqlSelectCommand
            ''' <summary>
            ''' Create a Native IDBCommand (Sql Command)
            ''' </summary>
            ''' <param name="cmd"></param>
            ''' <param name="aNativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateNativeDBCommand(cmd As String, aNativeConnection As System.Data.IDbConnection) As System.Data.IDbCommand Implements iormRelationalDatabaseDriver.CreateNativeDBCommand

            ''' <summary>
            ''' returns the native tablename in the native database
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetNativeDBObjectName(tableid As String) As String Implements iormRelationalDatabaseDriver.GetNativeDBObjectName
                If String.IsNullOrWhiteSpace(Me.Session.CurrentSetupID) OrElse tableid = ConstDBParameterTableName Then
                    ' create the native name as simple copy of the tableid
                    Return tableid
                Else
                    ' create the tablename out of the SetupID "_" tableid
                    Return Me.Session.CurrentSetupID & "_" & tableid
                End If
            End Function

            ''' <summary>
            ''' returns the native tablename in the native database
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetNativeIndexname(indexid As String) As String Implements iormRelationalDatabaseDriver.GetNativeIndexName
                If String.IsNullOrWhiteSpace(Me.Session.CurrentSetupID) Then
                    ' create the native name as simple copy of the indexid
                    Return indexid
                Else
                    ' create the indexname out of the SetupID "_" indexid
                    Return Me.Session.CurrentSetupID & "_" & indexid
                End If
            End Function

            ''' <summary>
            ''' returns the native view name in the native database
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetNativeViewname(viewid As String) As String Implements iormRelationalDatabaseDriver.GetNativeViewName
                If String.IsNullOrWhiteSpace(Me.Session.CurrentSetupID) Then
                    ' create the native name as simple copy of the viewid
                    Return viewid
                Else
                    ' create the viewname out of the SetupID "_" viewid
                    Return Me.Session.CurrentSetupID & "_" & viewid
                End If
            End Function

            ''' <summary>
            ''' returns the native foreign key name in the native database
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetNativeForeignkeyName(foreignkeyid As String) As String Implements iormRelationalDatabaseDriver.GetNativeForeignKeyName
                If String.IsNullOrWhiteSpace(Me.Session.CurrentSetupID) Then
                    ' create the native name as simple copy of the viewid
                    Return foreignkeyid
                Else
                    ' create the foreignkey name out of the SetupID "_" foreignkeyid
                    Return Me.Session.CurrentSetupID & "_" & foreignkeyid
                End If
            End Function
        End Class


        '************************************************************************************
        '***** neutral CLASS ormConnection describes the Connection description to OnTrack
        '*****
        '*****

        Public MustInherit Class ormConnection
            Implements iormConnection

            Private _ID As String
            Protected _Session As Session
            Protected _Databasetype As otDBServerType
            Protected _Connectionstring As String = String.empty  'the  Connection String
            Protected _Path As String = String.empty  'where the database is if access
            Protected _Name As String = String.empty  'name of the database or file
            Protected _Dbuser As String = String.empty  'User name to use to access the database
            Protected _Dbpassword As String = String.empty   'password to use to access the database
            Protected _Sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary ' configuration sequence of the connection
            'Protected _OTDBUser As New User    ' OTDB User -> moved to session 
            Protected _AccessLevel As otAccessRight    ' access

            Protected _UILogin As CoreLoginForm
            Protected _cacheUserValidateon As UserValidation
            Protected _OTDBDatabaseDriver As iormRelationalDatabaseDriver
            Protected _useseek As Boolean 'use seek instead of SQL
            Protected _lockObject As New Object ' use lock object for sync locking

            Protected WithEvents _ErrorLog As SessionMessageLog
            Protected WithEvents _configurations As ComplexPropertyStore

            Public Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
            Public Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

            ''' <summary>
            ''' constructor of Connection
            ''' </summary>
            ''' <param name="id"></param>
            ''' <param name="databasedriver"></param>
            ''' <param name="session"></param>
            ''' <remarks></remarks>
            Public Sub New(id As String, databasedriver As iormRelationalDatabaseDriver, ByRef session As Session, sequence As ComplexPropertyStore.Sequence)
                _OTDBDatabaseDriver = databasedriver
                _OTDBDatabaseDriver.RegisterConnection(Me)
                _Session = session
                _configurations = session.Configurations
                _ErrorLog = session.Errorlog
                _ID = id
                _Sequence = sequence
                _Databasetype = Nothing

                '_OTDBUser = Nothing
                _AccessLevel = Nothing
                _UILogin = Nothing
            End Sub
            ''' <summary>
            ''' Gets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public ReadOnly Property ID() As String Implements iormConnection.ID
                Get
                    Return _ID
                End Get
            End Property
            ''' <summary>
            ''' Gets the use seek.
            ''' </summary>
            ''' <value>The use seek.</value>
            Public ReadOnly Property Useseek() As Boolean Implements iormConnection.Useseek
                Get
                    Return _useseek
                End Get
            End Property
            ''' <summary>
            ''' returns the Sequence of the Database Configuration
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Sequence As ComplexPropertyStore.Sequence
                Get
                    Return _Sequence
                End Get
            End Property
            ''' <summary>
            ''' Gets the session.
            ''' </summary>
            ''' <value>The session.</value>
            Public ReadOnly Property Session() As Session Implements iormConnection.Session
                Get
                    Return Me._Session
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the DatabaseEnvirorment.
            ''' </summary>
            ''' <value>iOTDBDatabaseEnvirorment</value>
            Public Property DatabaseDriver() As iormRelationalDatabaseDriver Implements iormConnection.DatabaseDriver
                Get
                    Return _OTDBDatabaseDriver
                End Get
                Friend Set(value As iormRelationalDatabaseDriver)
                    _OTDBDatabaseDriver = value
                End Set
            End Property

            ''' <summary>
            ''' Gets the error log.
            ''' </summary>
            ''' <value>The error log.</value>
            Public ReadOnly Property ErrorLog() As SessionMessageLog Implements iormConnection.ErrorLog
                Get
                    If _ErrorLog Is Nothing Then
                        _ErrorLog = New SessionMessageLog(My.Computer.Name & "-" & My.User.Name & "-" & Date.Now.ToUniversalTime)
                    End If
                    Return _ErrorLog
                End Get
            End Property

            '*******
            '*******
            MustOverride ReadOnly Property IsConnected As Boolean Implements iormConnection.IsConnected

            '*******
            '*******
            MustOverride ReadOnly Property IsInitialized As Boolean Implements iormConnection.IsInitialized

            '*******
            '*******
            Friend MustOverride ReadOnly Property NativeConnection As Object Implements iormConnection.NativeConnection

            ''' <summary>
            ''' Gets or sets the UI login.
            ''' </summary>
            ''' <value>The UI login.</value>
            Public Property UILogin() As CoreLoginForm Implements iormConnection.UILogin
                Get
                    If _UILogin Is Nothing Then
                        _UILogin = New CoreLoginForm()
                    End If
                    Return Me._UILogin
                End Get
                Set(value As CoreLoginForm)
                    Me._UILogin = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the access.
            ''' </summary>
            ''' <value>The access.</value>
            Public Property Access() As otAccessRight Implements iormConnection.Access
                Get
                    Return Me._AccessLevel
                End Get
                Set(value As otAccessRight)
                    Me._AccessLevel = value
                End Set
            End Property


            ''' <summary>
            ''' Gets or sets the dbpassword.
            ''' </summary>
            ''' <value>The dbpassword.</value>
            Public Property Dbpassword() As String Implements iormConnection.Dbpassword
                Get
                    Return Me._Dbpassword
                End Get
                Set(value As String)
                    Me._Dbpassword = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the dbuser.
            ''' </summary>
            ''' <value>The dbuser.</value>
            Public Property Dbuser() As String Implements iormConnection.Dbuser
                Get
                    Return Me._Dbuser
                End Get
                Set(value As String)
                    Me._Dbuser = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the name.
            ''' </summary>
            ''' <value>The name.</value>
            Public Property DBName() As String Implements iormConnection.DBName
                Get
                    Return Me._Name
                End Get
                Set(value As String)
                    Me._Name = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the path.
            ''' </summary>
            ''' <value>The path.</value>
            Public Property PathOrAddress() As String Implements iormConnection.PathOrAddress
                Get
                    Return Me._Path
                End Get
                Set(value As String)
                    Me._Path = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the connectionstring.
            ''' </summary>
            ''' <value>The connectionstring.</value>
            Public Property Connectionstring() As String Implements iormConnection.Connectionstring
                Get
                    Return Me._Connectionstring
                End Get
                Set(value As String)
                    Me._Connectionstring = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the databasetype.
            ''' </summary>
            ''' <value>The databasetype.</value>
            Public Property Databasetype() As otDBServerType Implements iormConnection.Databasetype
                Get
                    Return Me._Databasetype
                End Get
                Set(value As otDBServerType)
                    Me._Databasetype = value
                End Set
            End Property

            Public Function RaiseOnConnected()
                RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
            End Function
            Public Function RaiseOnDisConnected()
                RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
            End Function

            '*****
            '***** reset : reset all the private members for a connection
            Protected Friend Overridable Sub ResetFromConnection()
                '_Connectionstring = String.empty

                '_Path = String.empty
                '_Name = String.empty
                _Dbuser = Nothing
                _Dbpassword = Nothing
                '_OTDBUser = Nothing
                _AccessLevel = Nothing

                '_UILogin = Nothing
            End Sub
            '*****
            '***** disconnect : Disconnects from the Database and cleans up the Enviorment
            Public Overridable Function Disconnect() As Boolean Implements iormConnection.Disconnect
                If Not Me.IsConnected Then
                    Return False
                End If
                ' Raise the event -> not working here ?!
                RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
                Return True
            End Function

            ''' <summary>
            ''' Event Handler for the Configuration Property Changed Event
            ''' </summary>
            ''' <param name="sender"></param>
            ''' <param name="e"></param>
            ''' <remarks></remarks>
            Public Sub OnConfigPropertyChanged(sender As Object, e As ComplexPropertyStore.EventArgs) Handles _configurations.OnPropertyChanged
                '** do only something if we have run through
                If Me.IsConnected Then
                    '** do nothing if we are running
                    CoreMessageHandler(message:="current config set name was changed after connection is connected -ignored", procedure:="ormConnection.OnCurrentConfigSetChanged", argument:=e.Setname, messagetype:=otCoreMessageType.InternalError)
                Else
                    SetConnectionConfigParameters()

                End If
            End Sub

            ''' <summary>
            ''' retrieve the Config parameters of OnTrack and sets it in the Connection
            ''' </summary>
            ''' <param name="propertyBag">a Dictionary of string, Object</param>
            ''' <returns>true if successfull</returns>
            ''' <remarks></remarks>
            Protected Friend Overridable Function SetConnectionConfigParameters() As Boolean Implements iormConnection.SetConnectionConfigParameters
                Dim connectionstring As String
                Dim Value As Object

                ' DBType
                Me.Databasetype = CLng(_configurations.GetProperty(name:=ConstCPNDBType, setname:=_Session.ConfigSetname, sequence:=_Sequence))

                '* useseek
                Value = _configurations.GetProperty(name:=ConstCPNDBUseseek, setname:=_Session.ConfigSetname, sequence:=_Sequence)
                If TypeOf (Value) Is Boolean Then
                    _useseek = Value
                ElseIf TypeOf (Value) Is String Then
                    If LCase(Trim(Value)) = "true" Then
                        _useseek = True
                    Else
                        _useseek = False
                    End If

                End If

                ' get the path
                Me.PathOrAddress = _configurations.GetProperty(name:=ConstCPNDBPath, setname:=_Session.ConfigSetname, sequence:=_Sequence)

                ' get the Database Name if we have it
                Me.DBName = _configurations.GetProperty(ConstCPNDBName, setname:=_Session.ConfigSetname, sequence:=_Sequence)

                ' get the Database user if we have it
                Me.Dbuser = _configurations.GetProperty(ConstCPNDBUser, setname:=_Session.ConfigSetname, sequence:=_Sequence)


                ' get the Database password if we have it
                Me.Dbpassword = _configurations.GetProperty(name:=ConstCPNDBPassword, setname:=_Session.ConfigSetname, sequence:=_Sequence)

                ' get the Database password if we have it
                Dim UseMars As String = _configurations.GetProperty(name:=ConstCPNDBSQLServerUseMars, setname:=_Session.ConfigSetname, sequence:=_Sequence)

                ' get the connection string
                connectionstring = _configurations.GetProperty(name:=ConstCPNDBConnection, setname:=_Session.ConfigSetname, sequence:=_Sequence)

                '***
                Call CoreMessageHandler(message:="Config connection parameters :" & Me.ID & vbLf & _
                                            " DatabaseType : " & Me.Databasetype.ToString & vbLf & _
                                            " Useseek : " & _useseek.ToString & vbLf & _
                                            " PathOrAddress :" & Me.PathOrAddress & vbLf & _
                                            " DBUser : " & Me.Dbuser & vbLf & _
                                            " DBPassword : " & Me.Dbpassword & vbLf & _
                                            " connectionsstring :" & connectionstring, _
                                            messagetype:=otCoreMessageType.InternalInfo, procedure:="ormConnection.SetconnectionConfigParameters")
                '** default
                '** we have no connection string than build one
                If String.IsNullOrWhiteSpace(connectionstring) Then
                    ' build the connectionstring for access
                    If Me.Databasetype = otDBServerType.Access Then
                        If Mid(_Path, Len(_Path), 1) <> "\" Then _Path &= "\"
                        If System.IO.File.Exists(_Path & _Name) Then
                            Me.Connectionstring = "Provider=Microsoft.ACE.OLEDB.12.0;" & _
                            "Data Source=" & _Path & _Name & ";"
                            Call CoreMessageHandler(message:="Config connection parameters :" & Me.ID & vbLf & _
                                          " created connectionsstring :" & Me.Connectionstring, _
                                          messagetype:=otCoreMessageType.InternalInfo, procedure:="ormConnection.SetconnectionConfigParameters")
                            Return True
                        Else
                            Call CoreMessageHandler(showmsgbox:=True, argument:=_Path & _Name, procedure:="ormConnection.retrieveConfigParameters", _
                                                  message:=" OnTrack database " & _Name & " doesnot exist at given location " & _Path, _
                                                  break:=False, noOtdbAvailable:=True)
                            '*** reset
                            Call ResetFromConnection()
                            Return False
                        End If

                        ' build the connectionstring for SQLServer
                    ElseIf _Databasetype = otDBServerType.SQLServer Then
                        ' set the seek
                        _useseek = False
                        Me.Connectionstring = "Data Source=" & _Path & "; Database=" & _Name & ";User Id=" & _Dbuser & ";Password=" & _Dbpassword & ";"
                        If UseMars IsNot Nothing AndAlso CBool(UseMars) Then
                            Me.Connectionstring &= "MultipleActiveResultSets=True;"
                        End If
                        Call CoreMessageHandler(message:="Config connection parameters :" & Me.ID & vbLf & _
                                          " created connectionsstring :" & Me.Connectionstring, _
                                          messagetype:=otCoreMessageType.InternalInfo, procedure:="ormConnection.SetconnectionConfigParameters")
                        Return True
                    Else
                        Call CoreMessageHandler(showmsgbox:=True, argument:=_Connectionstring, procedure:="ormConnection.retrieveConfigParameters", _
                                              message:=" OnTrack database " & _Name & " has not a valid database type.", _
                                              break:=False, noOtdbAvailable:=True)
                        '*** reset
                        Call ResetFromConnection()
                        Return False
                    End If
                End If


                Return True

            End Function

            '********
            '******** Connect : Connects to the Database and initialize Enviorement
            '********
            '********

            Public MustOverride Function Connect(Optional ByVal force As Boolean = False, _
            Optional ByVal accessRequest As otAccessRight = otAccessRight.[ReadOnly], _
            Optional ByVal domainid As String = Nothing, _
            Optional ByVal OTDBUsername As String = Nothing, _
            Optional ByVal OTDBPassword As String = Nothing, _
            Optional ByVal exclusive As Boolean = False, _
            Optional ByVal notInitialize As Boolean = False, _
            Optional ByVal doLogin As Boolean = True) As Boolean Implements iormConnection.Connect

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
                                                  Optional domainid As String = Nothing, _
                                                  Optional ByRef [Objectnames] As List(Of String) = Nothing) As Boolean Implements iormConnection.ValidateAccessRequest

                '

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
                                                Optional ByRef username As String = Nothing, _
                                                Optional ByRef password As String = Nothing, _
                                                Optional ByRef domainid As String = Nothing, _
                                                Optional ByRef [Objectnames] As List(Of String) = Nothing, _
                                                Optional useLoginWindow As Boolean = True, Optional messagetext As String = Nothing) As Boolean Implements iormConnection.VerifyUserAccess
                Dim userValidation As UserValidation
                userValidation.ValidEntry = False

                '****
                '**** no connection -> login
                If Not Me.IsConnected Then

                    If String.IsnullorEmpty(domainID) Then domainid = ConstGlobalDomain
                    '*** OTDBUsername supplied

                    If useLoginWindow And accessRequest <> ConstDefaultAccessRight Then

                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = Nothing
                        Me.UILogin.Password = Nothing

                        'LoginWindow
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.PossibleConfigSets = ot.ConfigSetNamesToSelect
                        'Me.UILogin.Databasedriver = Me.DatabaseDriver
                        Me.UILogin.EnableChangeConfigSet = True
                        If messagetext IsNot Nothing Then Me.UILogin.Messagetext = messagetext

                        Me.UILogin.Domain = domainid
                        Me.UILogin.EnableDomain = False

                        '* reset user validation we have
                        _cacheUserValidateon.ValidEntry = False

                        Me.UILogin.Accessright = accessRequest
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = Me.HigherAccessRequest(accessrequest:=accessRequest)

                        Me.UILogin.Show()

                        username = Me.UILogin.Username
                        password = Me.UILogin.Password
                        accessRequest = Me.UILogin.Accessright

                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        ' just check the provided username
                    ElseIf username <> String.Empty And password <> String.Empty And accessRequest <> ConstDefaultAccessRight Then
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* no username but default accessrequest then look for the anonymous user
                    ElseIf accessRequest = ConstDefaultAccessRight Then
                        userValidation = Me.DatabaseDriver.GetUserValidation(username:=String.Empty, selectAnonymous:=True)
                        If userValidation.ValidEntry Then
                            username = userValidation.Username
                            password = userValidation.Password
                        End If
                    End If

                    ' if user is still nothing -> not verified
                    If Not userValidation.ValidEntry Then
                        Call CoreMessageHandler(showmsgbox:=True, _
                                              message:=" Access to OnTrack Database is prohibited - User not found", _
                                              argument:=userValidation.Username, noOtdbAvailable:=True, break:=False)

                        _cacheUserValidateon.ValidEntry = False
                        '*** reset
                        Call ResetFromConnection()
                        Return False
                    Else

                        '*** old validation again
                        If _cacheUserValidateon.ValidEntry AndAlso userValidation.Password = _cacheUserValidateon.Password And userValidation.Username = _cacheUserValidateon.Username Then
                            '** do nothing

                            '**** Check Password
                            '****
                        ElseIf userValidation.Password = password Then
                            _cacheUserValidateon = userValidation
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, message:="User verified successfully *", _
                                                  argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        Else
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, message:="User not verified successfully", _
                                                  argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)
                            _cacheUserValidateon.ValidEntry = False
                            Return False
                        End If

                    End If

                    '****
                    '**** CONNECTION !
                Else
                    '** stay in the current domain 
                    If String.IsnullorEmpty(domainID) Then domainid = ot.CurrentSession.CurrentDomainID
                    '** validate the current user with the request
                    If Me.ValidateAccessRequest(accessrequest:=accessRequest, domainid:=domainid) Then
                        Return True
                        '* change the current user if anonymous
                    ElseIf useLoginWindow And ot.CurrentSession.OTdbUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainid
                        Me.UILogin.EnableDomain = False
                        Me.UILogin.PossibleDomains = New List(Of String)
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.EnableChangeConfigSet = False
                        Me.UILogin.Accessright = accessRequest
                        Me.UILogin.Messagetext = "<html><strong>Welcome !</strong><br />Please change to a valid user and password for authorization of the needed access right.</html>"
                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = Nothing
                        Me.UILogin.Password = Nothing
                        Me.UILogin.Show()
                        username = LoginWindow.Username
                        password = LoginWindow.Password
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* check password -> relogin on connected -> EventHandler ?!
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, _
                                                    message:="User change verified successfully on domain '" & domainid & "'", _
                               argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                            '* set the new access level
                            _AccessLevel = accessRequest

                            '** donot change the user
                            'Dim anOTDBUser As User = User.Retrieve(username:=username)
                            'If anOTDBUser IsNot Nothing Then
                            '    _OTDBUser = anOTDBUser
                            '    Me.Session.UserChangedEvent(_OTDBUser)
                            'Else
                            '    CoreMessageHandler(message:="user definition cannot be loaded", messagetype:=otCoreMessageType.InternalError, _
                            '                        arg1:=username, noOtdbAvailable:=False, subname:="ormConnection.verifyUserAccess")
                            '    Return False

                            'End If

                        Else
                            '** fallback
                            username = CurrentSession.OTdbUser.Username
                            password = CurrentSession.OTdbUser.Password
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                               argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                            Return False
                        End If
                        '* the current access level is not for this request
                    ElseIf useLoginWindow And Not CurrentSession.OTdbUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainid
                        Me.UILogin.EnableDomain = False
                        Me.UILogin.PossibleDomains = New List(Of String)
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.EnableChangeConfigSet = False
                        Me.UILogin.Accessright = accessRequest

                        Me.UILogin.Messagetext = "<html><strong>Attention !</strong><br />Please confirm by your password to obtain the access right.</html>"
                        Me.UILogin.EnableUsername = False
                        Me.UILogin.Username = CurrentSession.OTdbUser.Username
                        Me.UILogin.Password = password
                        Me.UILogin.Show()
                        ' return input
                        username = LoginWindow.Username
                        password = LoginWindow.Password
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* check password
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, message:="User change verified successfully (1)", _
                               argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                            '* set the new access level
                            _AccessLevel = accessRequest
                        Else
                            '** fallback
                            username = CurrentSession.OTdbUser.Username
                            password = CurrentSession.OTdbUser.Password
                            Call CoreMessageHandler(procedure:="ormConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                               argument:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                            Return False
                        End If

                        '*** just check the provided username
                    ElseIf username <> String.Empty And password <> String.Empty Then
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                    End If
                End If

                '**** Check the UserValidation Rights

                '* exclude user
                ' TODO AccessRightProperty.CoverRights(rights:=otAccessRight.AlterSchema, covers:=otAccessRight.ReadOnly)

                If userValidation.HasNoRights Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                          message:=" Access to OnTrack Database is prohibited - User has no rights", _
                                          break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                    '*** reset
                    If Not Me.IsConnected Then
                        ResetFromConnection()
                    Else
                        ' Disconnect() -> Do not ! fall back to old user
                    End If

                    Return False
                    '* check on the rights
                ElseIf Not userValidation.HasAlterSchemaRights And accessRequest = otAccessRight.AlterSchema Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                          message:=" Access to OnTrack Database is prohibited - User has no alter schema rights", _
                                          break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                    '*** reset
                    If Not Me.IsConnected Then
                        ResetFromConnection()
                    Else
                        ' Disconnect() -> Do not ! fall back to old user
                    End If
                    Return False
                ElseIf Not userValidation.HasUpdateRights And accessRequest = otAccessRight.ReadUpdateData Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                          message:=" Access to OnTrack Database is prohibited - User has no update rights", _
                                          break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                    '*** reset
                    If Not Me.IsConnected Then
                        ResetFromConnection()
                    Else
                        ' Disconnect() -> Do not ! fall back to old user
                    End If
                    Return False
                ElseIf Not userValidation.HasReadRights And accessRequest = otAccessRight.[ReadOnly] Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                          message:=" Access to OnTrack Database is prohibited - User has no read rights", _
                                          break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                    '*** reset
                    If Not Me.IsConnected Then
                        ResetFromConnection()
                    Else
                        ' Disconnect() -> Do not ! fall back to old user
                    End If
                    Return False
                End If
                '*** return true

                Return True

            End Function
        End Class
        '**************
        '************** ConnectionEventArgs for the ConnectionEvents
        ''' <summary>
        ''' defines the Connection Event Arguments
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ormConnectionEventArgs
            Inherits EventArgs

            Private _Connection As iormConnection
            Private _domain As String

            Public Sub New(newConnection As iormConnection, Optional domain As String = Nothing)
                _Connection = newConnection
                _domain = domain
            End Sub
            ''' <summary>
            ''' Gets or sets the domain.
            ''' </summary>
            ''' <value>The domain.</value>
            Public Property DomainID() As String
                Get
                    Return Me._domain
                End Get
                Set(value As String)
                    Me._domain = value
                End Set
            End Property

            ''' <summary>
            ''' Gets the error.
            ''' </summary>
            ''' <value>The error.</value>
            Public ReadOnly Property [Connection]() As iormConnection
                Get
                    Return _Connection
                End Get
            End Property

        End Class


        ''' <summary>
        ''' defines a abstract relational table store for sql tables and views
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormDataReader
            Implements iormRelationalTableStore

            Protected _DBObjectID As String ' Id of the database object
            Protected _DataSchema As iormContainerSchema  'Schema (Description) of the Table or DataStore
            Protected _Connection As iormConnection  ' Connection to use to access the Table or Datastore

            Private _PropertyBag As New Dictionary(Of String, Object)

            '*** Tablestore Cache Property names
            ''' <summary>
            ''' Table Property Name "Cache Property"
            ''' </summary>
            ''' <remarks></remarks>
            Public Const ConstTPNCacheProperty = "CacheDataTable"

            ''' <summary>
            ''' Table Property Name for FULL CACHING
            ''' </summary>
            ''' <remarks></remarks>
            Protected Const ConstTPNFullCaching = "FULL"
            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <param name="force"></param>
            ''' <remarks></remarks>
            Protected Sub New(connection As iormConnection, dbobjectid As String, ByVal force As Boolean)
                Call MyBase.New()
                Me.Connection = connection
                Me.ContainerID = dbobjectid
                Me.Refresh(force:=force)
            End Sub
            ''' <summary>
            ''' creates an unique key value. provide primary key array in the form {field1, field2, nothing}. "Nothing" will be increased.
            ''' </summary>
            ''' <param name="pkArray"></param>
            ''' <remarks></remarks>
            ''' <returns>True if successfull new value</returns>
            Public MustOverride Function CreateUniquePkValue(ByRef pkArray() As Object, Optional tag As String = Nothing) As Boolean Implements iormRelationalTableStore.CreateUniquePkValue


            ''' <summary>
            ''' Refresh
            ''' </summary>
            ''' <param name="force"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function Refresh(Optional ByVal force As Boolean = False) As Boolean Implements iormRelationalTableStore.Refresh

            ''' <summary>
            ''' returns the native Database Object Name
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeDBObjectname As String Implements iormRelationalTableStore.NativeDBObjectname
                Get
                    '**
                    If Not Me.ContainerSchema.IsInitialized Then
                        Return Nothing
                    End If
                    Return _DataSchema.NativeDBContainerName
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the database object ID.
            ''' </summary>
            ''' <value>The table ID.</value>
            Public Property ContainerID As String Implements iormRelationalTableStore.ContainerID
                Get
                    Return Me._DBObjectID
                End Get
                Protected Set(value As String)
                    Me._DBObjectID = value.ToUpper
                End Set
            End Property

            ''' <summary>
            ''' Gets the records by SQL command.
            ''' </summary>
            ''' <param name="sqlcommand">The sqlcommand.</param>
            ''' <param name="parameters">The parameters.</param>
            ''' <returns></returns>
            Public MustOverride Function GetRecordsBySqlCommand(ByRef sqlcommand As ormSqlSelectCommand, Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing) As List(Of ormRecord) Implements iormRelationalTableStore.GetRecordsBySqlCommand


            ''' <summary>
            ''' Gets or sets the connection.
            ''' </summary>
            ''' <value>The connection.</value>
            Public Overridable Property Connection() As iormConnection Implements iormRelationalTableStore.Connection
                Get
                    Return _Connection
                End Get
                Friend Set(value As iormConnection)
                    _Connection = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the DB table schema.
            ''' </summary>
            ''' <value>The DB table schema.</value>
            Public Overridable Property ContainerSchema() As iormContainerSchema Implements iormRelationalTableStore.ContainerSchema
                Get
                    Return _DataSchema
                End Get
                Friend Set(value As iormContainerSchema)
                    _DataSchema = value
                End Set
            End Property
            ''' <summary>
            ''' sets a Property to the TableStore
            ''' </summary>
            ''' <param name="Name">Name of the Property</param>
            ''' <param name="Object">ObjectValue</param>
            ''' <returns>returns True if succesfull</returns>
            ''' <remarks></remarks>
            Public Function SetProperty(ByVal name As String, ByVal value As Object) As Boolean Implements iormRelationalTableStore.SetProperty
                If _PropertyBag.ContainsKey(name) Then
                    _PropertyBag.Remove(name)
                End If
                _PropertyBag.Add(name, value)
                Return True
            End Function
            ''' <summary>
            ''' Gets the Property of a Tablestore
            ''' </summary>
            ''' <param name="name">name of property</param>
            ''' <returns>object of the property</returns>
            ''' <remarks></remarks>
            Public Function GetProperty(ByVal name As String) As Object Implements iormRelationalTableStore.GetProperty
                If _PropertyBag.ContainsKey(name) Then
                    Return _PropertyBag.Item(name)
                End If
                Return Nothing
            End Function
            ''' <summary>
            ''' has Tablestore the named property
            ''' </summary>
            ''' <param name="name">name of property</param>
            ''' <returns>return true</returns>
            ''' <remarks></remarks>
            Public Function HasProperty(ByVal name As String) As Boolean Implements iormRelationalTableStore.HasProperty
                Return _PropertyBag.ContainsKey(name)
            End Function
            ''' <summary>
            ''' Dels the record by primary key.
            ''' </summary>
            ''' <param name="aKeyArr">A key arr.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function DeleteRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As Boolean Implements iormRelationalTableStore.DeleteRecordByPrimaryKey
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' Runs the SQL command.
            ''' </summary>
            ''' <param name="command">The command.</param>
            ''' <param name="parameters">The parameters.</param>
            ''' <returns></returns>
            '''   
            Public Overridable Function RunSqlCommand(ByRef command As ormSqlCommand, _
                                                      Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing) As Boolean _
                Implements iormRelationalTableStore.RunSqlCommand
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' Gets the record by primary key.
            ''' </summary>
            ''' <param name="aKeyArr">A key arr.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function GetRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As ormRecord Implements iormRelationalTableStore.GetRecordByPrimaryKey
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' Gets the records by SQL.
            ''' </summary>
            ''' <param name="wherestr">The wherestr.</param>
            ''' <param name="fullsqlstr">The fullsqlstr.</param>
            ''' <param name="innerjoin">The innerjoin.</param>
            ''' <param name="orderby">The orderby.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function GetRecordsBySql(wherestr As String, Optional fullsqlstr As String = Nothing, _
                                                         Optional innerjoin As String = Nothing, Optional orderby As String = Nothing, _
                                                         Optional silent As Boolean = False, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) Implements iormRelationalTableStore.GetRecordsBySql
                Throw New NotImplementedException
            End Function
            ''' <summary>
            ''' Is Linq in this TableStore available
            ''' </summary>
            ''' <value>True if available</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable ReadOnly Property IsLinqAvailable As Boolean Implements iormRelationalTableStore.IsLinqAvailable
                Get
                    Return False
                End Get
            End Property
            ''' <summary>
            ''' gets a List of ormRecords by SQLCommand
            ''' </summary>
            ''' <param name="id">ID of the Command to store</param>
            ''' <param name="wherestr"></param>
            ''' <param name="fullsqlstr"></param>
            ''' <param name="innerjoin"></param>
            ''' <param name="orderby"></param>
            ''' <param name="silent"></param>
            ''' <param name="parameters"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetRecordsbySQlCommand(id As String, Optional wherestr As String = Nothing, Optional fullsqlstr As String = Nothing, _
                                                   Optional innerjoin As String = Nothing, Optional orderby As String = Nothing, Optional silent As Boolean = False, _
                                                   Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) _
                                               Implements iormRelationalTableStore.GetRecordsBySqlCommand
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' Gets the index of the records by.
            ''' </summary>
            ''' <param name="indexname">The indexname.</param>
            ''' <param name="aKeyArr">A key arr.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function GetRecordsByIndex(indexname As String, ByRef keysArray As Object(), Optional silent As Boolean = False) As List(Of ormRecord) Implements iormRelationalTableStore.GetRecordsByIndex
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' Infuses the record.
            ''' </summary>
            ''' <param name="aNewEnt">A new ent.</param>
            ''' <param name="aRecordSet">A record set.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function InfuseRecord(ByRef newRecord As ormRecord, ByRef RowObject As Object, Optional ByVal silent As Boolean = False, Optional CreateNewRecord As Boolean = False) As Boolean Implements iormRelationalTableStore.InfuseRecord
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' Persists the record.
            ''' </summary>
            ''' <param name="aRecord">A record.</param>
            ''' <param name="aTimestamp">A timestamp.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function PersistRecord(ByRef record As ormRecord, Optional timestamp As DateTime = ot.constNullDate, Optional ByVal silent As Boolean = False) As Boolean Implements iormRelationalTableStore.PersistRecord
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' Runs the SQL command.
            ''' </summary>
            ''' <param name="sqlcmdstr">The SQLCMDSTR.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function RunSQLStatement(sqlcmdstr As String, _
                                                        Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                        Optional silent As Boolean = True) As Boolean _
                Implements iormRelationalTableStore.RunSqlStatement
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' converts an object value to column data
            ''' </summary>
            ''' <param name="invalue"></param>
            ''' <param name="outvalue"></param>
            ''' <param name="targetType"></param>
            ''' <param name="maxsize"></param>
            ''' <param name="abostrophNecessary"></param>
            ''' <param name="fieldname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function Convert2ContainerData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                        targetType As Long, _
                                                        Optional ByVal maxsize As Long = 0, _
                                                       Optional ByRef abostrophNecessary As Boolean = False, _
                                                       Optional ByVal fieldname As String = Nothing, _
                                                        Optional isnullable As Boolean? = Nothing, _
                                                        Optional defaultvalue As Object = Nothing _
                                                    ) As Boolean Implements iormRelationalTableStore.Convert2ContainerData


            ''' <summary>
            ''' Convert2s the column data.
            ''' </summary>
            ''' <param name="anIndex">An index.</param>
            ''' <param name="aVAlue">A V alue.</param>
            ''' <param name="abostrophNecessary">The abostroph necessary.</param>
            ''' <returns></returns>
            Public Overridable Function Convert2ContainerData(index As Object, ByVal invalue As Object, ByRef outvalue As Object, _
                                                           Optional ByRef abostrophNecessary As Boolean = False, _
                                                           Optional isnullable As Boolean? = Nothing, _
                                                        Optional defaultvalue As Object = Nothing _
                                                    ) As Boolean Implements iormRelationalTableStore.Convert2ContainerData
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function

            ''' <summary>
            ''' Convert2s the object data.
            ''' </summary>
            ''' <param name="anIndex">An index.</param>
            ''' <param name="aVAlue">A V alue.</param>
            ''' <param name="abostrophNecessary">The abostroph necessary.</param>
            ''' <returns></returns>
            Public Overridable Function Convert2ObjectData(index As Object, _
                                                           ByVal invalue As Object, _
                                                           ByRef outvalue As Object, _
                                                           Optional isnullable As Boolean? = Nothing, _
                                                            Optional defaultvalue As Object = Nothing, _
                                                           Optional ByRef abostrophNecessary As Boolean = False) As Boolean Implements iormRelationalTableStore.Convert2ObjectData
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' checks if SqlCommand is in Store of the driver
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>True if successful</returns>
            ''' <remarks></remarks>
            Public Overridable Function HasSqlCommand(id As String) As Boolean Implements iormRelationalTableStore.HasSqlCommand
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' Store the Command by its ID - replace if existing
            ''' </summary>
            ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
            ''' <returns>true if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean Implements iormRelationalTableStore.StoreSqlCommand
                sqlCommand.ID = Me.GetSqlCommandID(sqlCommand.ID)

                Dim anExistingSqlCommand As iormSqlCommand
                If Me.Connection.DatabaseDriver.HasSqlCommand(sqlCommand.ID) Then
                    anExistingSqlCommand = Me.Connection.DatabaseDriver.RetrieveSqlCommand(sqlCommand.ID)
                    If anExistingSqlCommand.BuildVersion > sqlCommand.BuildVersion Then
                        Call CoreMessageHandler(messagetype:=otCoreMessageType.InternalWarning, procedure:="ormDataStore.StoreSQLCommand", argument:=sqlCommand.ID, _
                                               message:=" SqlCommand in Store has higher buildversion as the one to save ?! - not saved")
                        Return False
                    End If
                End If

                Me.Connection.DatabaseDriver.StoreSqlCommand(sqlCommand)
                Return True
            End Function
            ''' <summary>
            ''' Retrieve the Command from Store
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>a iOTDBSqlCommand</returns>
            ''' <remarks></remarks>
            Public Overridable Function RetrieveSqlCommand(id As String) As iormSqlCommand Implements iormRelationalTableStore.RetrieveSqlCommand
                '* get the ID
                id = Me.GetSqlCommandID(id)
                If Me.Connection.DatabaseDriver.HasSqlCommand(id) Then
                    Return Me.Connection.DatabaseDriver.RetrieveSqlCommand(id)
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' Creates a Command and store it or gets the current Command
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>a iOTDBSqlCommand</returns>
            ''' <remarks></remarks>
            Public Overridable Function CreateSqlCommand(id As String) As iormSqlCommand Implements iormRelationalTableStore.CreateSqlCommand
                '* get the ID
                id = Me.GetSqlCommandID(id)
                If Me.Connection.DatabaseDriver.HasSqlCommand(id) Then
                    Return Me.Connection.DatabaseDriver.RetrieveSqlCommand(id)
                Else
                    Dim aSqlCommand As iormSqlCommand = New ormSqlCommand(id)
                    Me.Connection.DatabaseDriver.StoreSqlCommand(aSqlCommand)
                    Return aSqlCommand
                End If
            End Function
            ''' <summary>
            ''' Creates a Command and store it or gets the current Command
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>a iOTDBSqlCommand</returns>
            ''' <remarks></remarks>
            Public Overridable Function CreateSqlSelectCommand(id As String, Optional addMe As Boolean = True, Optional addAllFields As Boolean = True) As iormSqlCommand Implements iormRelationalTableStore.CreateSqlSelectCommand
                '* get the ID
                id = Me.GetSqlCommandID(id)
                If Me.Connection.DatabaseDriver.HasSqlCommand(id) Then
                    Return Me.Connection.DatabaseDriver.RetrieveSqlCommand(id)
                Else
                    Dim aSqlCommand As iormSqlCommand = New ormSqlSelectCommand(id)
                    Me.Connection.DatabaseDriver.StoreSqlCommand(aSqlCommand)
                    If addMe Then
                        DirectCast(aSqlCommand, ormSqlSelectCommand).AddTable(tableid:=Me.ContainerID, addAllFields:=addAllFields)
                    End If
                    Return aSqlCommand
                End If
            End Function
            ''' <summary>
            ''' returns a ID for this Tablestore. Add the name of the table in front of the ID
            ''' </summary>
            ''' <param name="id">SqlcommandID</param>
            ''' <returns>the id</returns>
            ''' <remarks></remarks>
            Public Function GetSqlCommandID(id As String) As String
                If Not id.ToLower.Contains((LCase(Me.ContainerID & "."))) Then
                    Return Me.ContainerID & "." & id
                Else
                    Return id
                End If
            End Function
        End Class

        ''' <summary>
        ''' TopLevel abstract ViewReader Class
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormViewReader
            Inherits ormDataReader
            Implements iormRelationalTableStore


            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <param name="force"></param>
            ''' <remarks></remarks>
            Protected Sub New(connection As iormConnection, viewid As String, ByVal force As Boolean)
                Call MyBase.New(connection:=connection, dbobjectid:=viewid, force:=force)
            End Sub


            ''' <summary>
            ''' Refresh
            ''' </summary>
            ''' <param name="force"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Refresh(Optional ByVal force As Boolean = False) As Boolean Implements iormRelationalTableStore.Refresh
                ''' TODO: on Connection Refresh
                '** 
                If Not Connection Is Nothing AndAlso (Connection.IsConnected OrElse Connection.Session.IsBootstrappingInstallationRequested) Then

                    ''** all cache properties for tables used in starting up will be determined
                    ''** by schema
                    'If CurrentSession.IsStartingUp Then
                    '    Dim aTable = ot.GetSchemaTableAttribute(Me.ViewID)
                    '    If aTable IsNot Nothing Then
                    '        If aTable.HasValueUseCache AndAlso aTable.UseCache Then
                    '            If Not aTable.HasValueCacheProperties Then
                    '                Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                    '            Else
                    '                '** set properties
                    '                Dim ext As String = String.empty
                    '                Dim i As Integer = 0
                    '                For Each aproperty In aTable.CacheProperties
                    '                    Me.SetProperty(ConstTPNCacheProperty & ext, aproperty)
                    '                    ext = i.ToString
                    '                    i += 1
                    '                Next

                    '            End If
                    '        End If

                    '    End If
                    '    '** set the cache property if running from the object definitions
                    'ElseIf CurrentSession.IsRunning Then
                    '    Dim aTable = CurrentSession.Objects.GetTable(tablename:=Me.ViewID)
                    '    If aTable IsNot Nothing Then
                    '        If aTable.UseCache And aTable.CacheProperties.Count = 0 Then
                    '            Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                    '        Else
                    '            '** set properties
                    '            Dim ext As String = String.empty
                    '            Dim i As Integer = 0
                    '            For Each aproperty In aTable.CacheProperties
                    '                Me.SetProperty(ConstTPNCacheProperty & ext, aproperty)
                    '                ext = i.ToString
                    '                i += 1
                    '            Next

                    '        End If
                    '    End If
                    'End If

                    '** create and assign the table schema
                    If Me.ViewSchema Is Nothing OrElse force Then Me._DataSchema = Connection.DatabaseDriver.GetViewSchema(Me.ViewID, force:=force)
                    If ViewSchema Is Nothing OrElse Not ViewSchema.IsInitialized Then
                        Call CoreMessageHandler(break:=True, message:=" Schema for TableID '" & Me.ViewID & "' couldnot be loaded", containerID:=Me.ViewID, _
                                              messagetype:=otCoreMessageType.InternalError, procedure:="ormViewReader.Refresh")
                        Return False
                    End If
                End If
            End Function

            ''' <summary>
            ''' returns the native Tablename of this store from the schema
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeViewName As String Implements iormRelationalTableStore.NativeDBObjectname
                Get
                    '**
                    If Not Me.ContainerSchema.IsInitialized Then
                        Return Nothing
                    End If
                    Return Me.ContainerSchema.NativeDBContainerName
                End Get
            End Property

            ''' <summary>
            ''' return the associated Tableschema
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property ViewSchema As iormContainerSchema
                Get
                    Return Me.ContainerSchema
                End Get
            End Property
            ''' <summary>
            ''' Gets or sets the view ID.
            ''' </summary>
            ''' 
            ''' <value>The view ID.</value>
            Public Property ViewID As String Implements iormRelationalTableStore.ContainerID
                Get
                    Return MyBase.ContainerID
                End Get
                Protected Set(value As String)
                    MyBase.ContainerID = value.ToUpper
                End Set
            End Property


        End Class


        ''' <summary>
        ''' TopLevel OTDB Tablestore implementation base class
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormTableStore
            Inherits ormDataReader
            Implements iormRelationalTableStore

            ''' <summary>
            ''' Table Property Name "Cache Update Instant"
            ''' </summary>
            ''' <remarks></remarks>
            Public Const ConstTPNCacheUpdateInstant = "CacheDataTableUpdateImmediatly"

            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <param name="force"></param>
            ''' <remarks></remarks>
            Protected Sub New(connection As iormConnection, tableID As String, ByVal force As Boolean)
                Call MyBase.New(connection:=connection, dbobjectid:=tableID, force:=force)
            End Sub
            ''' <summary>
            ''' creates an unique key value. provide primary key array in the form {field1, field2, nothing}. "Nothing" will be increased.
            ''' </summary>
            ''' <param name="pkArray"></param>
            ''' <remarks></remarks>
            ''' <returns>True if successfull new value</returns>
            Public Overrides Function CreateUniquePkValue(ByRef pkArray() As Object, Optional tag As String = Nothing) As Boolean Implements iormRelationalTableStore.CreateUniquePkValue

                '**
                If Not Me.ContainerSchema.IsInitialized Then
                    Return False
                End If

                '** redim 
                ReDim Preserve pkArray(Me.ContainerSchema.NoPrimaryEntries() - 1)
                Dim anIndex As UShort = 0
                Dim keyfieldname As String

                Try
                    ' get
                    Dim aStore As iormRelationalTableStore = GetTableStore(Me.TableID)
                    Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="CreateUniquePkValue" & tag, addMe:=True, addAllFields:=False)

                    '** prepare the command if necessary

                    ''' this command lives from the first call !! -> all elements in pkArray not fixed will be regarded as elements to be fixed
                    If Not aCommand.IsPrepared Then
                        '* retrieve the maximum field
                        For Each pkvalue In pkArray
                            If pkvalue Is Nothing Then
                                keyfieldname = ContainerSchema.GetPrimaryEntryNames(anIndex + 1)
                                Exit For
                            End If
                            anIndex += 1
                        Next
                        '*
                        aCommand.select = "max( [" & keyfieldname & "] )"
                        If anIndex > 0 Then
                            For j = 0 To anIndex - 1 ' an index points to the keyfieldname, parameter is the rest
                                If j > 0 Then aCommand.Where &= " AND "
                                aCommand.Where &= "[" & ContainerSchema.GetPrimaryEntryNames(j + 1) & "] = @" & ContainerSchema.GetPrimaryEntryNames(j + 1)
                                aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & ContainerSchema.GetPrimaryEntryNames(j + 1), _
                                                                                     columnname:=ContainerSchema.GetPrimaryEntryNames(j + 1), tableid:=Me.TableID))
                            Next
                        End If
                        aCommand.Prepare()
                    End If

                    '* retrieve the maximum field -> and sets the index
                    anIndex = 0
                    For Each pkvalue In pkArray
                        If Not pkvalue Is Nothing Then
                            aCommand.SetParameterValue(ID:="@" & ContainerSchema.GetPrimaryEntryNames(anIndex + 1), value:=pkvalue)
                        Else
                            Exit For
                        End If
                        anIndex += 1
                    Next
                    '** run the Command
                    Dim theRecords As List(Of ormRecord) = aCommand.RunSelect

                    '*** increments ! -> need to be incrementable
                    If theRecords.Count > 0 Then
                        ' returns always one field Max !
                        If Not DBNull.Value.Equals(theRecords.Item(0).GetValue(1)) AndAlso IsNumeric(theRecords.Item(0).GetValue(1)) Then
                            pkArray(anIndex) = CLng(theRecords.Item(0).GetValue(1)) + 1
                            Return True
                        Else
                            pkArray(anIndex) = CLng(1)
                            Return True
                        End If

                    Else
                        pkArray(anIndex) = CLng(1)
                        Return True
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, exception:=ex, procedure:="ormTableStore.CreateUniquePkValue")
                    Return False
                End Try


            End Function

            ''' <summary>
            ''' Refresh
            ''' </summary>
            ''' <param name="force"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function Refresh(Optional ByVal force As Boolean = False) As Boolean Implements iormRelationalTableStore.Refresh
                ''' TODO: on Connection Refresh
                '** 
                If Connection IsNot Nothing AndAlso (Connection.IsConnected OrElse Connection.Session.IsBootstrappingInstallationRequested) Then

                    '** all cache properties for tables used in starting up will be determined
                    '** by schema
                    If CurrentSession.IsStartingUp Then
                        Dim aTable = ot.GetSchemaTableAttribute(Me.TableID)
                        If aTable IsNot Nothing Then
                            If aTable.HasValueUseCache AndAlso aTable.UseCache Then
                                If Not aTable.HasValueCacheProperties Then
                                    Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                                Else
                                    '** set properties
                                    Dim ext As String = String.Empty
                                    Dim i As Integer = 0
                                    For Each aproperty In aTable.CacheProperties
                                        Me.SetProperty(ConstTPNCacheProperty & ext, aproperty)
                                        ext = i.ToString
                                        i += 1
                                    Next

                                End If
                            End If

                        End If
                        '** set the cache property if running from the object definitions
                    ElseIf CurrentSession.IsRunning Then
                        Dim aTable = CurrentSession.Objects.GetTable(tablename:=Me.TableID)
                        If aTable IsNot Nothing Then
                            If aTable.UseCache And aTable.CacheProperties.Count = 0 Then
                                Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                            Else
                                '** set properties
                                Dim ext As String = String.Empty
                                Dim i As Integer = 0
                                For Each aproperty In aTable.CacheProperties
                                    Me.SetProperty(ConstTPNCacheProperty & ext, aproperty)
                                    ext = i.ToString
                                    i += 1
                                Next

                            End If
                        End If
                    End If

                    '** create and assign the table schema
                    If Me.TableSchema Is Nothing OrElse force Then Me._DataSchema = Connection.DatabaseDriver.RetrieveContainerSchema(Me.TableID, force:=force)
                    If TableSchema Is Nothing OrElse Not TableSchema.IsInitialized Then
                        Call CoreMessageHandler(break:=True, message:=" Schema for TableID '" & Me.TableID & "' couldnot be loaded", containerID:=Me.TableID, _
                                              messagetype:=otCoreMessageType.InternalError, procedure:="ormTableStore.Refresh")
                        Return False
                    End If
                End If
            End Function

            ''' <summary>
            ''' returns the native Tablename of this store from the schema
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeViewName As String Implements iormRelationalTableStore.NativeDBObjectname
                Get
                    '**
                    If Not Me.ContainerSchema.IsInitialized Then
                        Return Nothing
                    End If
                    Return Me.ContainerSchema.NativeDBContainerName
                End Get
            End Property

            ''' <summary>
            ''' return the associated Tableschema
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property TableSchema As iormContainerSchema
                Get
                    Return Me.ContainerSchema
                End Get
            End Property
            ''' <summary>
            ''' Gets or sets the table ID.
            ''' </summary>
            ''' 
            ''' <value>The table ID.</value>
            Public Property TableID As String Implements iormRelationalTableStore.ContainerID
                Get
                    Return MyBase.ContainerID
                End Get
                Protected Set(value As String)
                    MyBase.ContainerID = value.ToUpper
                End Set
            End Property


        End Class

        ''' <summary>
        ''' describes the current schema in the data base (meta data from the native Database)
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormContainerSchema
            Implements iormContainerSchema

            Protected _Connection As iormConnection
            Protected _ContainerID As String
            Protected _nativeDBObjectname As String ' the tablename of the table in the database

            Protected _fieldsDictionary As Dictionary(Of String, Long)    ' crossreference to the Arrays
            Protected _indexDictionary As Dictionary(Of String, ArrayList)    ' crossreference of the Index


            Protected _entrynames() As String    ' Fieldnames in OTDB
            Protected _Primarykeys() As UShort    ' indices for primary keys
            Protected _NoPrimaryKeys As UShort
            Protected _PrimaryKeyIndexName As String
            Protected _DomainIDPrimaryKeyOrdinal As Short = -1 ' cache the Primary Key Ordinal of domainID for domainbehavior


            Protected _IsInitialized As Boolean = False
            Protected _lockObject As New Object ' Lock Object

            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="containerId"></param>
            ''' <remarks></remarks>
            Public Sub New(ByRef connection As iormConnection, ByVal dbobjectid As String)
                ReDim Preserve _entrynames(0)

                _fieldsDictionary = New Dictionary(Of String, Long)
                _indexDictionary = New Dictionary(Of String, ArrayList)
                _Connection = connection
                _ContainerID = dbobjectid
                _NoPrimaryKeys = 0
                ReDim Preserve _Primarykeys(0 To 0)
            End Sub
            ''' <summary>
            ''' Assigns the native DB parameter.
            ''' </summary>
            ''' <param name="p1">The p1.</param>
            ''' <returns></returns>
            Public MustOverride Function AssignNativeDBParameter(fieldname As String, _
                                                                 Optional parametername As String = Nothing) As System.Data.IDbDataParameter Implements iormContainerSchema.AssignNativeDBParameter


            ''' <summary>
            ''' Gets or sets the is initialized. Should be True if the tableschema has a containerId 
            ''' </summary>
            ''' <value>The is initialized.</value>
            Public ReadOnly Property IsInitialized() As Boolean Implements iormContainerSchema.IsInitialized
                Get
                    Return Me._IsInitialized
                End Get

            End Property

            ''' <summary>
            ''' resets the  to hold nothing
            ''' </summary>
            ''' <remarks></remarks>
            Protected Overridable Sub Reset()
                Dim nullArray As Object = {}
                _entrynames = nullArray
                _fieldsDictionary.Clear()
                _indexDictionary.Clear()
                _ContainerID = Nothing
                _nativeDBObjectname = Nothing
                _PrimaryKeyIndexName = Nothing
                _Primarykeys = nullArray
                _NoPrimaryKeys = 0
                _DomainIDPrimaryKeyOrdinal = -1
            End Sub

            ''' <summary>
            ''' returns the containerId of the table
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property ContainerID() As String Implements iormContainerSchema.ContainerID
                Get
                    Return _ContainerID
                End Get
            End Property
            ''' <summary>
            ''' returns the native tablename of this table in the database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeDBObjectname As String Implements iormContainerSchema.NativeDBContainerName
                Get
                    If _nativeDBObjectname Is Nothing Then
                        _nativeDBObjectname = _Connection.DatabaseDriver.GetNativeDBObjectName(_ContainerID)
                    End If
                    Return _nativeDBObjectname
                End Get
            End Property

            ''' <summary>
            ''' Names of the Indices of the table
            ''' </summary>
            ''' <value>List(of String)</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Indices As List(Of String) Implements iormContainerSchema.Indices
                Get
                    Return _indexDictionary.Keys.ToList
                End Get

            End Property
            ''' <summary>
            ''' refresh the table schema
            ''' </summary>
            ''' <param name="reloadForce"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function Refresh(Optional reloadForce As Boolean = False) As Boolean Implements iormContainerSchema.Refresh
            ''' <summary>
            ''' returns the primary Key ordinal (1..n) for the domain ID or less zero if not in primary key
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ''' <summary>
            ''' returns the primary Key ordinal (1..n) for the domain ID or less zero if not in primary key
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetDomainIDPKOrdinal() As Integer Implements iormContainerSchema.GetDomainIDPKOrdinal
                If _DomainIDPrimaryKeyOrdinal < 0 Then
                    Dim i As Integer = Me.GetEntryOrdinal(index:=Domain.ConstFNDomainID)
                    If i < 0 Then
                        Return -1
                    Else
                        If Not Me.HasPrimaryEntryName(name:=Domain.ConstFNDomainID.ToUpper) Then
                            Return -1
                        Else
                            For i = 1 To Me.NoPrimaryEntries
                                If Me.GetPrimaryEntrynames(i).ToUpper = Domain.ConstFNDomainID.ToUpper Then
                                    _DomainIDPrimaryKeyOrdinal = i
                                    Return i
                                End If
                            Next
                            Return -1
                        End If
                    End If
                Else
                    Return _DomainIDPrimaryKeyOrdinal
                End If

            End Function


            ''' <summary>
            ''' Gets the nullable property.
            ''' </summary>
            ''' <param name="index">The index.</param>
            ''' <returns></returns>
            Public MustOverride Function GetNullable(index As Object) As Boolean Implements iormContainerSchema.GetNullable

            ''' <summary>
            ''' returns the default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function GetDefaultValue(ByVal index As Object) As Object Implements iormContainerSchema.GetDefaultValue

            ''' <summary>
            ''' returns if there is a default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasDefaultValue(ByVal index As Object) As Boolean Implements iormContainerSchema.HasDefaultValue


            '**** getIndex returns the ArrayList of Fieldnames for the Index or Nothing
            ''' <summary>
            '''  returns the ArrayList of Fieldnames for the Index or empty array list if not found
            ''' </summary>
            ''' <param name="indexname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetIndex(indexname As String) As ArrayList Implements iormContainerSchema.GetIndex


                If Not _indexDictionary.ContainsKey(indexname) Then
                    Return New ArrayList
                Else
                    Return _indexDictionary.Item(indexname)
                End If

            End Function
            '**** hasIndex returns true if index by Name exists
            ''' <summary>
            ''' returns true if index by Name exists
            ''' </summary>
            ''' <param name="indexname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function HasIndex(indexname As String) As Boolean Implements iormContainerSchema.HasIndex
                If Not _indexDictionary.ContainsKey(indexname) Then
                    Return False
                Else
                    Return True
                End If

            End Function
            '**** primaryKeyIndexName
            ''' <summary>
            ''' gets the primarykey name
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable ReadOnly Property PrimaryKeyIndexName As String Implements iormContainerSchema.PrimaryKeyIndexName
                Get
                    Throw New NotImplementedException
                End Get
            End Property
            '******* return the no. fields
            '*******
            ''' <summary>
            ''' gets the number of fields
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NoEntries() As Integer Implements iormContainerSchema.NoEntries
                Get
                    Return UBound(_entrynames) + 1 'zero bound
                End Get
            End Property
            ''' <summary>
            ''' List of Fieldnames
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property EntryNames As List(Of String) Implements iormContainerSchema.EntryNames
                Get
                    Return _entrynames.ToList
                End Get
            End Property


            ''' <summary>
            ''' Get the Fieldordinal (position in record) by Index - can be numeric or the columnname
            ''' </summary>
            ''' <param name="anIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetEntryOrdinal(index As Object) As Integer Implements iormContainerSchema.GetEntryOrdinal
                Dim i As ULong

                Try
                    If IsNumeric(index) Then
                        If CLng(index) > 0 And CLng(index) <= (_entrynames.GetUpperBound(0) + 1) Then
                            Return CLng(index)
                        Else
                            Call CoreMessageHandler(message:="index of column out of range", _
                                             argument:=index, procedure:="ormContainerSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                            Return i
                        End If
                    ElseIf _fieldsDictionary.ContainsKey(index) Then
                        Return _fieldsDictionary.Item(index)
                    ElseIf _fieldsDictionary.ContainsKey(index.toupper) Then
                        Return _fieldsDictionary.Item(index.toupper)

                    Else
                        Call CoreMessageHandler(message:="index of column out of range", _
                                              argument:=index, procedure:="ormContainerSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                        Return -1
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(argument:=index, procedure:="ormContainerSchema.getFieldIndex", exception:=ex)
                    Return -1
                End Try

            End Function


            ''' <summary>
            ''' get the fieldname by index i - nothing if not in range
            ''' </summary>
            ''' <param name="i"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetEntryName(ByVal i As Integer) As String Implements iormContainerSchema.GetEntryName

                If i > 0 And i <= UBound(_entrynames) + 1 Then
                    Return _entrynames(i - 1)
                Else
                    Call CoreMessageHandler(message:="index of column out of range", argument:=i, containerID:=Me.ContainerID, _
                                          messagetype:=otCoreMessageType.InternalError, procedure:="ormContainerSchema.getFieldName")
                    Return Nothing
                End If
            End Function

            '*** check if fieldname by Name exists
            ''' <summary>
            ''' check if entryname exists
            ''' </summary>
            ''' <param name="name"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function HasEntryName(ByVal name As String) As Boolean Implements iormContainerSchema.HasEntryName

                For i = LBound(_entrynames) To UBound(_entrynames)
                    If _entrynames(i).ToUpper = name.ToUpper Then
                        Return True
                    End If
                Next i

                Return False
            End Function

            ''' <summary>
            ''' List of primary key field names
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable ReadOnly Property PrimaryEntryNames() As List(Of String) Implements iormContainerSchema.PrimaryEntryNames
                Get
                    Dim aList As New List(Of String)
                    For i = 1 To Me.NoPrimaryEntries
                        aList.Add(Me.GetPrimaryEntrynames(i))
                    Next
                    Return aList
                End Get
            End Property

            ''' <summary>
            ''' gets the fieldname of the primary key field by number (1..)
            ''' </summary>
            ''' <param name="i">1..n</param>
            ''' <returnsString></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetPrimaryEntrynames(i As UShort) As String Implements iormContainerSchema.GetPrimaryEntryNames
                Dim aCollection As ArrayList

                If i < 1 Then
                    Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyFieldName", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          argument:=i)
                    Return String.Empty
                End If

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        If i > aCollection.Count Then
                            Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  argument:=i)
                            Return String.Empty

                        End If

                        '*** return the item (Name)
                        Return aCollection.Item(i - 1)
                    Else
                        Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              argument:=i)
                        Return String.Empty
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, procedure:="ormContainerSchema.getPrimaryKeyFieldName", _
                                          containerID:=_ContainerID, exception:=ex)
                    Return String.Empty
                End Try

            End Function
            ''' <summary>
            ''' gets the fieldname of the primary key field by number
            ''' </summary>
            ''' <param name="i">1..n</param>
            ''' <returnsString></returns>
            ''' <remarks></remarks>
            Public Overridable Function HasPrimaryEntryName(ByRef name As String) As Boolean Implements iormContainerSchema.HasPrimaryEntryName
                Dim aCollection As ArrayList

                Try

                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)

                        '*** return the item (Name)
                        Return aCollection.Contains(name.ToUpper)
                    Else
                        Call CoreMessageHandler(procedure:="ormContainerSchema.hasPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        Return Nothing
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, procedure:="ormContainerSchema.hasPrimaryKeyName", _
                                          containerID:=_ContainerID, exception:=ex)
                    Return Nothing
                End Try

            End Function

            ''' <summary>
            ''' gets the field ordinal of the primary Key field by number i. (e.g.returns the ordinal of the primarykey field #2)
            ''' </summary>
            ''' <param name="i">number of primary key field 1..n </param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function GetOrdinalOfPrimaryEntry(i As UShort) As Integer Implements iormContainerSchema.GetOrdinalOfPrimaryEntry
                Dim aCollection As ArrayList
                Dim aFieldName As String


                If i < 1 Then
                    Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyFieldIndex", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          argument:=i)
                    GetOrdinalOfPrimaryEntry = -1
                    Exit Function
                End If

                Try
                    If _indexDictionary.ContainsKey((_PrimaryKeyIndexName)) Then
                        aCollection = _indexDictionary.Item((_PrimaryKeyIndexName))

                        If i > aCollection.Count Then
                            Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  argument:=i)
                            GetOrdinalOfPrimaryEntry = -1
                            Exit Function
                        End If

                        aFieldName = aCollection.Item(i - 1)
                        GetOrdinalOfPrimaryEntry = _fieldsDictionary.Item((aFieldName))
                        Exit Function
                    Else
                        Call CoreMessageHandler(procedure:="ormContainerSchema.getPrimaryKeyFieldIndex", _
                                              message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              argument:=i)
                        System.Diagnostics.Debug.WriteLine("ormContainerSchema: primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        GetOrdinalOfPrimaryEntry = -1
                        Exit Function
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, procedure:="ormContainerSchema.getPrimaryKeyFieldIndex", containerID:=Me.containerId, exception:=ex)
                    Return -1
                End Try
            End Function

            ''' <summary>
            ''' get the number of primary key fields
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function NoPrimaryEntries() As Integer Implements iormContainerSchema.NoPrimaryEntries
                Dim aCollection As ArrayList

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        Return aCollection.Count

                    Else
                        Call CoreMessageHandler(procedure:="ormContainerSchema.noPrimaryKeysFields", message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              argument:=_PrimaryKeyIndexName, containerID:=_ContainerID)
                        Return -1

                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, procedure:="ormContainerSchema.noPrimaryKeys", containerID:=_ContainerID, exception:=ex)
                    Return -1
                End Try


            End Function

        End Class

        ''' <summary>
        ''' describes the schema independent of the base database
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormViewSchema
            Inherits ormContainerSchema
            Implements iormContainerSchema

            ''' <summary>
            ''' List of Tables a View relies on
            ''' </summary>
            ''' <remarks></remarks>
            Protected _tableschemas As List(Of iormContainerSchema)

            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <remarks></remarks>
            Public Sub New(ByRef connection As iormConnection, ByVal viewid As String)
                MyBase.New(connection:=connection, dbobjectid:=viewid)
            End Sub


            ''' <summary>
            ''' returns the tableid of the table
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property ViewID() As String Implements iormContainerSchema.ContainerID
                Get
                    Return MyBase.ContainerID
                End Get
            End Property
            ''' <summary>
            ''' returns the native tablename of this table in the database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeViewname As String Implements iormContainerSchema.NativeDBContainerName
                Get
                    Return MyBase.NativeDBObjectname
                End Get
            End Property

        End Class
        ''' <summary>
        ''' describes the schema independent of the base database
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormTableSchema
            Inherits ormContainerSchema
            Implements iormContainerSchema


            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <remarks></remarks>
            Public Sub New(ByRef connection As iormConnection, ByVal tableID As String)
                MyBase.New(connection:=connection, dbobjectid:=tableID)
            End Sub


            ''' <summary>
            ''' resets the TableSchema to hold nothing
            ''' </summary>
            ''' <remarks></remarks>
            Protected Overridable Sub Reset()
                MyBase.Reset()
            End Sub

            ''' <summary>
            ''' returns the tableid of the table
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property TableID() As String Implements iormContainerSchema.ContainerID
                Get
                    Return MyBase.ContainerID
                End Get
            End Property
            ''' <summary>
            ''' returns the native tablename of this table in the database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NativeTablename As String Implements iormContainerSchema.NativeDBContainerName
                Get
                    Return MyBase.NativeDBObjectname
                End Get
            End Property



            '**** primaryKeyIndexName
            ''' <summary>
            ''' gets the primarykey name
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides ReadOnly Property PrimaryKeyIndexName As String Implements iormContainerSchema.PrimaryKeyIndexName
                Get
                    PrimaryKeyIndexName = _PrimaryKeyIndexName
                End Get
            End Property


        End Class
    End Namespace
End Namespace
