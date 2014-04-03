
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

Namespace OnTrack
    Namespace Database
        '************************************************************************************
        '***** CLASS ormSqlCommand describes an SQL Command to be used for aTableStore
        '***** or a DB Driver
        '*****
        ''' <summary>
        ''' an neutral SQL Command
        ''' </summary>
        ''' <remarks></remarks>

        Public Class ormSqlCommand
            Implements iormSqlCommand

            Private _ID As String = ""  ' an Unique ID to store
            Protected _parameters As New Dictionary(Of String, ormSqlCommandParameter)
            Protected _parametervalues As New Dictionary(Of String, Object)

            Protected _type As OTDBSQLCommandTypes
            Protected _SqlStatement As String = ""
            Protected _SqlText As String = "" ' the build SQL Text

            Protected _databaseDriver As iormDatabaseDriver
            Protected _tablestores As New Dictionary(Of String, iormDataStore)
            Protected _buildTextRequired As Boolean = True
            Protected _buildVersion As UShort = 0
            Protected _nativeCommand As System.Data.IDbCommand
            Protected _Prepared As Boolean = False

            Public Sub New(ID As String, Optional databasedriver As iormDatabaseDriver = Nothing)
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
            Public Property DatabaseDriver() As iormDatabaseDriver
                Get
                    Return Me._databaseDriver
                End Get
                Set(value As iormDatabaseDriver)
                    Me._databaseDriver = value
                End Set
            End Property
            Public ReadOnly Property BuildVersion As UShort Implements iormSqlCommand.BuildVersion
                Get
                    Return _buildVersion
                End Get
            End Property

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
                    If Me._SqlText <> "" Or Me.BuildTextRequired Then
                        If Me.BuildTextRequired Then
                            Call BuildSqlText()
                        End If
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
            Public ReadOnly Property [Type] As OTDBSQLCommandTypes Implements iormSqlCommand.Type
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
            Public ReadOnly Property Prepared As Boolean
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
                If parameter.ID = "" And parameter.Fieldname = "" And Not parameter.NotColumn Then
                    Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, message:=" id not set in parameter for sql command", messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf parameter.ID = "" And parameter.Fieldname <> "" And Not parameter.NotColumn Then
                    parameter.ID = "@" & parameter.Fieldname
                ElseIf parameter.ID <> "" Then
                    parameter.ID = Regex.Replace(parameter.ID, "\s", "") ' no white chars allowed
                End If

                '** TABLENAME
                If Not parameter.NotColumn Then
                    If Me.TableIDs.Count = 0 Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, _
                                              message:="no tablename  set in parameter for sql command", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    ElseIf parameter.Tablename = "" And Me.TableIDs(0) <> "" Then
                        parameter.Tablename = Me.TableIDs(0)
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, _
                                              message:=" tablename not set in parameter for sql command - first table used", _
                                              messagetype:=otCoreMessageType.InternalWarning, tablename:=Me.TableIDs(0))

                    ElseIf parameter.Tablename = "" And Me.TableIDs(0) = "" Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, _
                                              message:=" tablename not set in parameter for sql command - no default table", _
                                             messagetype:=otCoreMessageType.InternalError)

                        Return False
                    End If
                End If

                    '** fieldnames
                    If parameter.Fieldname = "" And parameter.ID = "" Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, _
                                              message:=" fieldname not set in parameter for sql command", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    ElseIf parameter.ID <> "" And parameter.Fieldname = "" And Not parameter.NotColumn Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, _
                                             message:=" fieldname not set in parameter for sql command - use ID without @", _
                                             messagetype:=otCoreMessageType.InternalWarning, tablename:=parameter.Tablename, entryname:=parameter.ID)
                        If parameter.ID.First = "@" Then
                            parameter.Fieldname = parameter.ID.Substring(2)
                        Else
                            parameter.Fieldname = parameter.ID
                        End If
                    End If
                    '** table name ?!
                    If parameter.Tablename = "" And Not parameter.NotColumn Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", tablename:=parameter.Tablename, _
                                              message:="table name is blank", arg1:=parameter.ID)
                        Return False
                    End If
                    If Not parameter.NotColumn And parameter.Tablename <> "" AndAlso Not GetTableStore(parameter.Tablename).TableSchema.IsInitialized Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", tablename:=parameter.Tablename, _
                                               message:="couldnot initialize table schema")
                        Return False
                    End If

                    If Not parameter.NotColumn AndAlso Not Me._tablestores.ContainsKey(parameter.Tablename) Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, entryname:=parameter.ID, _
                                              message:=" tablename of parameter is not used in sql command", _
                                          messagetype:=otCoreMessageType.InternalError, tablename:=parameter.Tablename)
                        Return False
                    ElseIf Not parameter.NotColumn AndAlso Not Me._tablestores.Item(key:=parameter.Tablename).TableSchema.Hasfieldname(parameter.Fieldname) Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", arg1:=Me.ID, entryname:=parameter.Fieldname, _
                                             message:=" fieldname of parameter is not used in table schema", _
                                         messagetype:=otCoreMessageType.InternalError, tablename:=parameter.Tablename)
                        Return False

                    End If


                    ''' datatype
                    If parameter.NotColumn And parameter.Datatype = 0 Then
                        Call CoreMessageHandler(subname:="ormSqlCommand.AddParameter", _
                                              arg1:=Me.ID, message:=" datatype not set in parameter for sql command", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                        ''' datatype lookup
                    ElseIf Not parameter.NotColumn AndAlso parameter.Datatype = 0 Then

                        ''' look up internally first
                        ''' 
                        Dim anAttribute As ormObjectEntryAttribute = ot.GetSchemaTableColumnAttribute(tablename:=parameter.Tablename, columnname:=parameter.Fieldname)
                        If anAttribute IsNot Nothing AndAlso anAttribute.HasValueTypeID Then
                            parameter.Datatype = anAttribute.Typeid
                        End If
                        ''' datatype still not resolved
                        If parameter.Datatype = 0 Then
                            Dim aSchemaEntry As ColumnDefinition = CurrentSession.Objects.GetColumnEntry(columnname:=parameter.Fieldname, tablename:=parameter.Tablename)
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
            ''' Sets the parameter value.
            ''' </summary>
            ''' <param name="name">The name of the parameter.</param>
            ''' <param name="value">The value of the object</param>
            ''' <returns></returns>
            Public Function SetParameterValue(ID As String, [value] As Object) As Boolean Implements iormSqlCommand.SetParameterValue
                If Not _parameters.ContainsKey(key:=ID) Then
                    Call CoreMessageHandler(message:="Parameter ID not in Command", arg1:=Me.ID, entryname:=ID, subname:="ormSqlCommand.SetParameterValue", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
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
                ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
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
                ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
                If Not _parameters.ContainsKey(key:=ID) Then
                    Call CoreMessageHandler(message:="Parameter ID not in Command", arg1:=Me.ID, entryname:=ID, subname:="ormSqlCommand.SetParameterValue", _
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
            ''' prepares the command
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Prepare() As Boolean Implements iormSqlCommand.Prepare
                Dim aNativeConnection As System.Data.IDbConnection
                Dim aNativeCommand As System.Data.IDbCommand
                Dim cvtvalue As Object
                 Dim aTablestore As iormDataStore
                If Me.DatabaseDriver Is Nothing Then
                    Call CoreMessageHandler(subname:="ormSqlCommand.Prepare", arg1:=Me.ID, message:="database driver missing", _
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
                    If aSqlText = "" Then
                        Call CoreMessageHandler(message:="No SQL statement could'nt be build", arg1:=Me.ID, _
                                               subname:="ormSqlCommand.Prepare", _
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

                        If Not aParameter.NotColumn And aParameter.Tablename <> "" And aParameter.Fieldname <> "" Then
                            aTablestore = _databaseDriver.GetTableStore(aParameter.Tablename)
                            If Not aTablestore.TableSchema.IsInitialized Then
                                Call CoreMessageHandler(subname:="ormSqlCommand.Prepare", tablename:=aParameter.Tablename, _
                                                       message:="couldnot initialize table schema")
                                Return False
                            End If
                            Dim aNativeParameter As System.Data.IDbDataParameter = _
                                aTablestore.TableSchema.AssignNativeDBParameter(fieldname:=aParameter.Fieldname, parametername:=aParameter.ID)
                            If Not aParameter Is Nothing Then aNativeCommand.Parameters.Add(aNativeParameter)
                        ElseIf aParameter.NotColumn Then
                            Dim aNativeParameter As System.Data.IDbDataParameter = _
                               _databaseDriver.AssignNativeDBParameter(parametername:=aParameter.ID, datatype:=aParameter.Datatype)
                            If Not aParameter Is Nothing Then aNativeCommand.Parameters.Add(aNativeParameter)
                        Else
                            Call CoreMessageHandler(subname:="ormSqlCommand.Prepare", arg1:=aParameter.ID, message:="Tablename missing", _
                                                  entryname:=aParameter.Fieldname, messagetype:=otCoreMessageType.InternalError)
                        End If
                    Next
                    '** prepare the native
                    aNativeCommand.Prepare()
                    Me._Prepared = True
                    '** initial values
                    aTablestore = Nothing ' reset
                    For Each aParameter In Me.Parameters
                        If aParameter.Fieldname <> "" And aParameter.Tablename <> "" Then
                            If aTablestore Is Nothing OrElse aTablestore.TableID <> aParameter.Tablename Then
                                aTablestore = _databaseDriver.GetTableStore(aParameter.Tablename)
                            End If
                            If Not aTablestore.Convert2ColumnData(aParameter.Fieldname, invalue:=aParameter.Value, outvalue:=cvtvalue) Then
                                Call CoreMessageHandler(message:="parameter value could not be converted", columnname:=aParameter.Fieldname, _
                                                        entryname:=aParameter.ID, arg1:=aParameter.Value, messagetype:=otCoreMessageType.InternalError, _
                                                        subname:="ormSqlCommand.Prepare")
                            End If
                        Else
                            cvtvalue = aParameter.Value
                        End If
                        If aNativeCommand.Parameters.Contains(aParameter.ID) Then
                            aNativeCommand.Parameters(aParameter.ID).value = cvtvalue
                        Else
                            Call CoreMessageHandler(message:="Parameter ID is not in native sql command", entryname:=aParameter.ID, arg1:=Me.ID, _
                                                   messagetype:=otCoreMessageType.InternalError, subname:="ormSqlCommand.Prepare")

                        End If

                    Next

                    Return True

                Catch ex As OleDb.OleDbException
                    Me._Prepared = False
                    Call CoreMessageHandler(subname:="ormSqlCommand.Prepare", message:="Exception", arg1:=Me.ID, _
                                           exception:=ex, messagetype:=otCoreMessageType.InternalException)
                    Return False
                Catch ex As Exception
                    Me._Prepared = False
                    Call CoreMessageHandler(subname:="ormSqlCommand.Prepare", message:="Exception", arg1:=Me.ID, _
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
                If Me.Prepared Then
                    Return Me.DatabaseDriver.RunSqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                Else
                    If Me.Prepare() Then
                        Return Me.DatabaseDriver.RunSqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBSqlSelectCommand.run", message:="Command is not prepared", arg1:=Me.ID, _
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

            Private _ID As String = ""
            Private _NotColumn As Boolean = False
            Private _tablename As String = ""
            Private _columname As String = ""
            Private _datatype As otFieldDataType = 0
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
                           Optional datatype As otFieldDataType = 0, _
                           Optional columnname As String = "", _
                           Optional tablename As String = "", _
                           Optional value As Object = Nothing, _
                           Optional notColumn As Boolean = False)
                _ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
                _datatype = datatype
                If columnname <> "" Then _columname = columnname.ToUpper
                If tablename <> "" Then _tablename = tablename.ToUpper
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
            Public Property Datatype() As otFieldDataType
                Get
                    Return Me._datatype
                End Get
                Set(value As otFieldDataType)
                    Me._datatype = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the fieldname.
            ''' </summary>
            ''' <value>The fieldname.</value>
            Public Property Fieldname() As String
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
            Public Property Tablename() As String
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
                    Me._ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
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
            Private _fields As New Dictionary(Of String, ResultField)

            Private _select As String = ""
            Private _innerjoin As String = ""
            Private _orderby As String = ""
            Private _where As String = ""
            Private _AllFieldsAdded As Boolean



            ''' <summary>
            ''' Class for Storing the select result fields per Table(store)
            ''' </summary>
            ''' <remarks></remarks>
            Public Class ResultField
                Implements IHashCodeProvider

                Private _myCommand As ormSqlSelectCommand ' Backreference
                Private _name As String
                Private _tablestore As iormDataStore
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
                    Me.Tablename = tableid
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
                Public Property [Tablestore]() As iormDataStore
                    Get
                        Return Me._tablestore
                    End Get
                    Set(value As iormDataStore)
                        Me._tablestore = value
                        If _myCommand.DatabaseDriver Is Nothing Then
                            _myCommand.DatabaseDriver = value.Connection.DatabaseDriver
                        End If
                    End Set
                End Property
                ''' <summary>
                ''' Gets or sets the Tablestore / Tablename.
                ''' </summary>
                ''' <value>The name.</value>
                Public Property [Tablename]() As String
                    Get
                        If _tablestore Is Nothing Then
                            Return ""
                        Else
                            Return _tablestore.TableID
                        End If

                    End Get
                    Set(value As String)
                        Dim aTablestore As iormDataStore
                        '** set it to current connection 
                        If Not _myCommand.DatabaseDriver Is Nothing Then
                            _myCommand.DatabaseDriver = ot.CurrentConnection.DatabaseDriver
                        End If
                        ' retrieve the tablestore
                        If Not _myCommand._tablestores.ContainsKey(key:=value) Then
                            ' add it
                            aTablestore = Me._myCommand.DatabaseDriver.GetTableStore(tableID:=value)
                            If aTablestore IsNot Nothing Then
                                _myCommand._tablestores.Add(key:=aTablestore.TableID, value:=aTablestore)
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
                    Return (Me.Tablename & _name).GetHashCode
                End Function

            End Class

            ''' <summary>
            ''' Constructor of the OTDB Select command
            ''' </summary>
            ''' <param name="ID">the unique ID to store it</param>
            ''' <remarks></remarks>
            Public Sub New(ID As String)
                Call MyBase.New(ID:=ID)
                _type = OTDBSQLCommandTypes.SELECT
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
            ''' returns the build SQL Statement
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides ReadOnly Property SqlText As String Implements iormSqlCommand.SqlText
                Get
                    If Me.BuildTextRequired Then
                        BuildSqlText()
                        Return _SqlText
                    Else
                        Return _SqlStatement
                    End If

                End Get
            End Property
            ''' <summary>
            ''' Add Table with fields to the Resultfields
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function AddTable(tableid As String, Optional addAllFields As Boolean = True, Optional addFieldnames As List(Of String) = Nothing) As Boolean
                Dim aTablestore As iormDataStore
                tableid = tableid.ToUpper
                If Me._databaseDriver Is Nothing Then
                    aTablestore = GetTableStore(tableid:=tableid)
                    If aTablestore Is Nothing Then
                        Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", tablename:=tableid, subname:="clsOTDBSelectCommand.ADDTable", _
                                              messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        Me.DatabaseDriver = aTablestore.Connection.DatabaseDriver
                    End If
                Else
                    aTablestore = _databaseDriver.GetTableStore(tableID:=tableid)
                End If


                If aTablestore Is Nothing Then
                    Call CoreMessageHandler(message:="Tablestore couldnot be retrieved", tablename:=tableid, subname:="clsOTDBSelectCommand.ADDTable", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                If Not _tablestores.ContainsKey(key:=tableid) Then
                    _tablestores.Add(key:=tableid, value:=aTablestore)
                End If

                '*** include all fields
                If addAllFields Then
                    For Each aFieldname As String In aTablestore.TableSchema.Fieldnames
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname.ToUpper) Then
                            _fields.Add(key:=tableid & "." & aFieldname.ToUpper, value:=New ResultField(Me, tableid:=tableid, fieldname:=aFieldname.ToUpper))
                        End If
                    Next
                    _AllFieldsAdded = True
                End If

                '** include specific fields
                If Not addFieldnames Is Nothing Then
                    For Each aFieldname As String In addFieldnames
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname.ToUpper) Then
                            _fields.Add(key:=tableid & "." & aFieldname, value:=New ResultField(Me, tableid:=tableid, fieldname:=aFieldname.ToUpper))
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
                For Each atablename In _tablestores.Keys
                    'Dim aTablename = kvp.Key
                    If Not aTableList.Contains(atablename) Then
                        aTableList.Add(atablename)
                    End If
                Next

                '*** build the result list
                If _select = "" Then
                    first = True
                    '*
                    For Each aResultField In _fields.Values
                        Dim aTablename = aResultField.Tablename
                        If Not aTableList.Contains(aTablename) Then
                            aTableList.Add(aTablename)
                        End If
                        Dim aFieldname = aResultField.Name

                        If Not first Then
                            Me._SqlText &= ","
                        End If
                        Me._SqlText &= aTablename & ".[" & aFieldname & "] "
                        first = False
                    Next

                    If aTableList.Count = 0 Then
                        Call CoreMessageHandler(message:="no table and no fields in sql statement", subname:="clsOTDBSqlSelectCommand.BuildSqlText", _
                                               arg1:=Me.ID, messagetype:=otCoreMessageType.InternalError)
                        Me.BuildTextRequired = True
                        Return ""
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
                For Each aTablename In aTableList

                    '** if innerjoin has the tablename
                    If Not _innerjoin.ToUpper.Contains(aTablename) Then
                        If Not first Then
                            Me._SqlText &= ","
                        End If
                        Me._SqlText &= aTablename
                        first = False
                    End If
                Next

                '*** innerjoin
                If _innerjoin <> "" Then
                    If Not _innerjoin.tolower.Contains("join") Then
                        Me._SqlText &= " inner join "
                    End If
                    _SqlText &= _innerjoin
                End If

                '*** where 
                If _where <> "" Then
                    If Not _where.tolower.Contains("where") Then
                        Me._SqlText &= " WHERE "
                    End If
                    _SqlText &= _where
                End If

                '*** order by 
                If _orderby <> "" Then
                    If Not _where.tolower.Contains("order by") Then
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
                    Dim aStore As iormDataStore = _tablestores.Values.First
                    '*** run it
                    If Me.Prepared Then
                        Return aStore.GetRecordsBySqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues)
                    Else
                        If Me.Prepare() Then
                            Return aStore.GetRecordsBySqlCommand(sqlcommand:=Me, parametervalues:=aParametervalues)
                        Else
                            Call CoreMessageHandler(subname:="clsOTDBSqlSelectCommand.runSelect", message:="Command is not prepared", arg1:=Me.ID, _
                                                             messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of ormRecord)
                        End If
                    End If
                Else
                    ''' else run against the database driver
                    ''' 
                    '*** run it
                    If Me.Prepared Then
                        Return Me.DatabaseDriver.RunSqlSelectCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                    Else
                        If Me.Prepare() Then
                            Return Me.DatabaseDriver.RunSqlSelectCommand(sqlcommand:=Me, parametervalues:=aParametervalues, nativeConnection:=nativeConnection)
                        Else
                            Call CoreMessageHandler(subname:="clsOTDBSqlSelectCommand.runSelect", message:="Command is not prepared", arg1:=Me.ID, _
                                                             messagetype:=otCoreMessageType.InternalError)
                            Return New List(Of ormRecord)
                        End If
                    End If
                End If
                
            End Function
        End Class

        '************************************************************************************
        '***** neutral CLASS clsOTDBDriver describes the Environment of the Database Implementation
        '***** on which OnTrack runs
        '*****
        ''' <summary>
        ''' abstract ORM Driver class for Database Drivers
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormDatabaseDriver
            Implements iormDatabaseDriver

            Protected _ID As String
            Protected _TableDirectory As New Dictionary(Of String, iormDataStore)    'Table Directory of iOTDBTableStore
            Protected _TableSchemaDirectory As New Dictionary(Of String, iotDataSchema)    'Table Directory of iOTDBTableSchema
            Protected WithEvents _primaryConnection As iormConnection ' primary connection
            Protected WithEvents _session As Session
            Protected _CommandStore As New Dictionary(Of String, iormSqlCommand) ' store of the SqlCommands to handle

            Protected _lockObject As New Object 'Lock object instead of me
            '* the events
            Public Event RequestBootstrapInstall(sender As Object, e As SessionBootstrapEventArgs) Implements iormDatabaseDriver.RequestBootstrapInstall
#Region "Properties"

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
            ''' returns the OTDBServertype
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public ReadOnly Property DatabaseType As otDBServerType Implements iormDatabaseDriver.DatabaseType
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
            Public MustOverride ReadOnly Property Type() As otDbDriverType Implements iormDatabaseDriver.Type

            ''' <summary>
            ''' Gets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Overridable Property ID() As String Implements iormDatabaseDriver.ID
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
            Public Property TableSchemaDirectory() As Dictionary(Of String, iotDataSchema)
                Get
                    Return Me._TableSchemaDirectory
                End Get
                Set(value As Dictionary(Of String, iotDataSchema))
                    Me._TableSchemaDirectory = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the table directory.
            ''' </summary>
            ''' <value>The table directory.</value>
            Public Property TableDirectory() As Dictionary(Of String, iormDataStore)
                Get
                    Return Me._TableDirectory
                End Get
                Set(value As Dictionary(Of String, iormDataStore))
                    Me._TableDirectory = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the connection.
            ''' </summary>
            ''' <value>The connection.</value>
            Public Overridable ReadOnly Property CurrentConnection() As iormConnection Implements iormDatabaseDriver.CurrentConnection
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
            Public Function HasSqlCommand(id As String) As Boolean Implements iormDatabaseDriver.HasSqlCommand
                Return _CommandStore.ContainsKey(key:=id)
            End Function

            ''' <summary>
            ''' Store the Command by its ID - replace if existing
            ''' </summary>
            ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
            ''' <remarks></remarks>
            ''' <returns>true if successful</returns>
            Public Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean Implements iormDatabaseDriver.StoreSqlCommand
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
            Public Function RetrieveSqlCommand(id As String) As iormSqlCommand Implements iormDatabaseDriver.RetrieveSqlCommand
                If _CommandStore.ContainsKey(key:=id) Then
                    Return _CommandStore.Item(key:=id)
                End If

                Return Nothing
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

            Public MustOverride Function InstallOnTrackDatabase(askBefore As Boolean, modules As String()) As Boolean Implements iormDatabaseDriver.InstallOnTrackDatabase
            Public MustOverride Function HasAdminUserValidation(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.HasAdminUserValidation

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
            Public MustOverride Function CreateGlobalDomain(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.CreateGlobalDomain



            ''' <summary>
            ''' verifyOnTrack
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyOnTrackDatabase(Optional modules As String() = Nothing, Optional install As Boolean = False, Optional verifySchema As Boolean = False) As Boolean Implements iormDatabaseDriver.VerifyOnTrackDatabase


            ''' <summary>
            ''' create an assigned Native DBParameter to provided name and type
            ''' </summary>
            ''' <param name="parametername"></param>
            ''' <param name="datatype"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function AssignNativeDBParameter(parametername As String, datatype As otFieldDataType, _
                                                                  Optional maxsize As Long = 0, _
                                                                 Optional value As Object = Nothing) As System.Data.IDbDataParameter Implements iormDatabaseDriver.AssignNativeDBParameter

            ''' <summary>
            ''' returns the target type for a OTDB FieldType - MAPPING
            ''' </summary>
            ''' <param name="type"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDatabaseDriver.GetTargetTypeFor
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
                                                       Optional ByVal fieldname As String = "", _
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
                                                       Optional nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.RunSqlCommand


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

            ''' Gets the catalog.
            ''' </summary>
            ''' <param name="FORCE">The FORCE.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetCatalog(Optional force As Boolean = False, Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetCatalog
            ' TODO: Implement this method

            ''' <summary>
            ''' returns True if data store has the table
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasTable(tablename As String, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.HasTable

            ''' <summary>
            ''' returns True if data store has the table by definition
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyTableSchema(tabledefinition As TableDefinition, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.VerifyTableSchema

            ''' <summary>
            ''' returns True if data store has the table attribute
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyTableSchema(tableattribute As ormSchemaTableAttribute, Optional ByRef connection As iormConnection = Nothing, Optional nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.VerifyTableSchema

            ''' <summary>
            ''' Gets the table.
            ''' </summary>
            ''' <param name="tablename">The tablename.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetTable(tablename As String, _
                            Optional createOrAlter As Boolean = False, _
                            Optional ByRef connection As iormConnection = Nothing, _
                             Optional ByRef nativeTableObject As Object = Nothing) As Object Implements iormDatabaseDriver.GetTable

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
            Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetIndex

            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasColumn(tablename As String, columnname As String, Optional ByRef connection As iormConnection = Nothing) As Boolean Implements iormDatabaseDriver.HasColumn
            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyColumnSchema(columndefinition As ColumnDefinition, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean Implements iormDatabaseDriver.VerifyColumnSchema

            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyColumnSchema(columnattribute As ormSchemaTableColumnAttribute, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean Implements iormDatabaseDriver.VerifyColumnSchema

            ''' <summary>
            ''' Gets the column.
            ''' </summary>
            ''' <param name="nativeTABLE">The native TABLE.</param>
            ''' <param name="aDBDesc">A DB desc.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <returns></returns>
            Public MustOverride Function GetColumn(nativeTable As Object, columndefinition As ColumnDefinition, Optional createOrAlter As Boolean = False, _
                                                   Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetColumn


            ''' <summary>
            ''' Create the User Definition Table
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.CreateDBUserDefTable

            ''' <summary>
            ''' create the DB Parameter Table
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDatabaseDriver.CreateDBParameterTable

            ''' <summary>
            ''' Sets the DB parameter.
            ''' </summary>
            ''' <param name="Parametername">The parametername.</param>
            ''' <param name="Value">The value.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <param name="UpdateOnly">The update only.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function SetDBParameter(parametername As String, Value As Object, _
                                                        Optional ByRef nativeConnection As Object = Nothing, Optional UpdateOnly As Boolean = False, Optional silent As Boolean = False) As Boolean Implements iormDatabaseDriver.SetDBParameter

            ''' <summary>
            ''' Gets the DB parameter.
            ''' </summary>
            ''' <param name="PARAMETERNAME">The PARAMETERNAME.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function GetDBParameter(parametername As String, Optional ByRef nativeConnection As Object = Nothing, Optional silent As Boolean = False) As Object Implements iormDatabaseDriver.GetDBParameter



            ''' <summary>
            ''' validates the User, Passoword, Access Right in the Domain
            ''' </summary>
            ''' <param name="username"></param>
            ''' <param name="password"></param>
            ''' <param name="accessright"></param>
            ''' <param name="domainID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function ValidateUser(ByVal username As String, ByVal password As String, ByVal accessRequest As otAccessRight, Optional domainid As String = "") As Boolean Implements iormDatabaseDriver.validateUser
                Dim aValidation As UserValidation
                aValidation.ValidEntry = False
                aValidation = GetUserValidation(username:=username)

                If Not aValidation.ValidEntry Then
                    Return False
                Else
                    If aValidation.Password <> password Then
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
            ''' Gets the def user.
            ''' </summary>
            ''' <param name="Username">The username.</param>
            ''' <param name="connection">The native connection.</param>
            ''' <returns></returns>
            Protected Friend MustOverride Function GetUserValidation(username As String, Optional ByVal selectAnonymous As Boolean = False, _
                                                                     Optional ByRef nativeConnection As Object = Nothing) As UserValidation Implements iormDatabaseDriver.GetUserValidation

            ''' <summary>
            ''' create a tablestore 
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableStore(ByVal tableID As String, ByVal forceSchemaReload As Boolean) As iormDataStore
            ''' <summary>
            ''' create a tableschema
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableSchema(ByVal tableID As String) As iotDataSchema

            ''' <summary>
            ''' persists the errorlog
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function PersistLog(ByRef log As MessageLog) As Boolean Implements iormDatabaseDriver.PersistLog
            ''' <summary>
            ''' Gets the table store.
            ''' </summary>
            ''' <param name="tableID">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetTableStore(ByVal tableID As String, Optional ByVal force As Boolean = False) As iormDataStore Implements iormDatabaseDriver.GetTableStore
                'take existing or make new one
                If _TableDirectory.ContainsKey(tableID.ToUpper) And Not force Then
                    Return _TableDirectory.Item(tableID.ToUpper)
                Else
                    Dim aNewStore As iormDataStore

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
            ''' Gets the table store.
            ''' </summary>
            ''' <param name="Tablename">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetTableSchema(ByVal tableID As String, Optional ByVal force As Boolean = False) As iotDataSchema _
            Implements iormDatabaseDriver.GetTableSchema

                'take existing or make new one
                If _TableSchemaDirectory.ContainsKey(tableID.ToUpper) And Not force Then
                    Return _TableSchemaDirectory.Item(tableID.ToUpper)
                Else
                    Dim aNewSchema As iotDataSchema

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
            ''' Runs the SQL Command
            ''' </summary>
            ''' <param name="sqlcmdstr"></param>
            ''' <param name="parameters"></param>
            ''' <param name="silent"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                      Optional silent As Boolean = True, Optional nativeConnection As Object = Nothing) As Boolean _
                                                  Implements iormDatabaseDriver.RunSqlStatement


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
                                            Implements iormDatabaseDriver.RunSqlSelectCommand

            Public MustOverride Function RunSqlSelectCommand(id As String, _
                                                         Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                         Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                                       Implements iormDatabaseDriver.RunSqlSelectCommand
            ''' <summary>
            ''' Create a Native IDBCommand (Sql Command)
            ''' </summary>
            ''' <param name="cmd"></param>
            ''' <param name="aNativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateNativeDBCommand(cmd As String, aNativeConnection As System.Data.IDbConnection) As System.Data.IDbCommand Implements iormDatabaseDriver.CreateNativeDBCommand


        End Class


        ''' <summary>
        ''' represents a record data tuple for to be stored and retrieved in a data store
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ormRecord
            Inherits Dynamic.DynamicObject

            Private _FixEntries As Boolean = False
            Private _IsTableSet As Boolean = False
            Private _TableStore As iormDataStore = Nothing
            Private _DbDriver As iormDatabaseDriver = Nothing
            Private _entrynames() As String = {}
            Private _Values() As Object = {}
            Private _OriginalValues() As Object = {}
            Private _isCreated As Boolean = False
            Private _isUnknown As Boolean = True
            Private _isLoaded As Boolean = False
            Private _isChanged As Boolean = False
            Private _tableid As String = ""

            '** initialize
            Public Sub New()

            End Sub
            Public Sub New(ByVal tableID As String, _
                           Optional dbdriver As iormDatabaseDriver = Nothing, _
                           Optional fillDefaultValues As Boolean = False, _
                           Optional runtimeOnly As Boolean = False)
                _DbDriver = dbdriver
                _tableid = tableID
                If Not runtimeOnly Then
                    Me.SetTable(tableID, forceReload:=False, dbdriver:=dbdriver, fillDefaultValues:=fillDefaultValues)
                    _FixEntries = True
                End If
            End Sub

            Public Sub Finalize()

                _TableStore = Nothing
                _Values = Nothing
                _OriginalValues = Nothing
            End Sub

            ' If you try to get a value of a property that is
            ' not defined in the class, this method is called.
            ''' <summary>
            ''' dynamic getValue Property
            ''' </summary>
            ''' <param name="binder"></param>
            ''' <param name="result"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function TryGetMember(
                ByVal binder As System.Dynamic.GetMemberBinder,
                ByRef result As Object) As Boolean

                ' Converting the property name to lowercase
                ' so that property names become case-insensitive.
                Dim name As String = binder.Name

                ' If the property name is found in a dictionary,
                ' set the result parameter to the property value and return true.
                ' Otherwise, return false.
                Dim flag As Boolean
                result = Me.GetValue(index:=name, notFound:=flag)
                Return flag
            End Function
            ''' <summary>
            ''' Dynamic setValue Property
            ''' </summary>
            ''' <param name="binder"></param>
            ''' <param name="value"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function TrySetMember(
                ByVal binder As System.Dynamic.SetMemberBinder,
                ByVal value As Object) As Boolean

                ' Converting the property name to lowercase
                ' so that property names become case-insensitive.
                Return Me.SetValue(index:=binder.Name, value:=value)

            End Function
            ''' <summary>
            ''' Gets the is table set.
            ''' </summary>
            ''' <value>The is table set.</value>
            Public ReadOnly Property IsTableBound() As Boolean
                Get
                    Return Me._IsTableSet
                End Get
            End Property

            ''' <summary>
            ''' set if this record is a new Record in the databse
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property IsCreated As Boolean
                Get
                    Return _isCreated
                End Get
                Friend Set(value As Boolean)

                    If value Then
                        _isCreated = True
                        _isLoaded = False
                        _isUnknown = False
                    End If
                End Set
            End Property
            ''' <summary>
            ''' set if the record state is unkown if new or load
            ''' </summary>
            ''' <value>The is unknown.</value>
            Public Property IsUnknown() As Boolean
                Get
                    Return Me._isUnknown
                End Get
                Set(value As Boolean)
                    Me._isUnknown = value
                    If value Then
                        Me.iscreated = False
                        Me.isloaded = False
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the is changed.
            ''' </summary>
            ''' <value>The is changed.</value>
            Public Property IsChanged() As Boolean
                Get
                    Return Me._isChanged
                End Get
                Friend Set(value As Boolean)
                    Me._isChanged = value
                End Set
            End Property
            ''' <summary>
            ''' set if record is loaded
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property IsLoaded As Boolean
                Get
                    Return _isLoaded
                End Get
                Friend Set(value As Boolean)
                    If value Then
                        _isCreated = False
                        _isLoaded = True
                        _isUnknown = False
                    End If
                End Set
            End Property
            ''' <summary>
            ''' returns true if record is alive
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Alive As Boolean
                Get
                    If _FixEntries Then
                        Return _IsTableSet
                    Else
                        Return True
                    End If

                End Get
            End Property
            ''' <summary>
            ''' returns Length of Record
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Length As Integer
                Get
                    Length = UBound(_Values)
                End Get
            End Property
            ''' <summary>
            '''  the TableID to the Record
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property TableID As String
                Get
                    If _TableStore IsNot Nothing Then
                        _tableid = _TableStore.TableID
                        Return _TableStore.TableID
                    Else
                        Return _tableid
                    End If
                End Get
                Private Set(value As String)
                    _tableid = value
                End Set
            End Property
            ''' <summary>
            ''' returns the tablestore or nothing
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property TableStore As iormDataStore
                Get
                    If Alive Then
                        Return _TableStore
                    Else
                        Return Nothing
                    End If
                End Get

            End Property
            ''' <summary>
            ''' load a record into this record from the datareader
            ''' </summary>
            ''' <param name="datareader"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function LoadFrom(ByRef datarow As DataRow) As Boolean
                Dim result As Boolean = True
                Try
                    ''' if tableset then only check which fields are in the datareader
                    ''' 
                    _isLoaded = True ' important

                    If _IsTableSet Then
                        For j = 1 To _TableStore.TableSchema.NoFields

                            Dim aColumnname As String = _TableStore.TableSchema.Getfieldname(j)
                            If datarow.Table.Columns.Contains(aColumnname) Then
                                Dim aValue As Object = datarow.Item(aColumnname)
                                If _TableStore.Convert2ObjectData(index:=j, invalue:=datarow.Item(aColumnname), outvalue:=aValue) Then
                                    If Not SetValue(j, aValue) Then
                                        CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                                           columnname:=aColumnname, tablename:=_tableid, subname:="ormRecord.LoadFrom(Datarow)")
                                        result = False
                                    Else
                                        result = result And True
                                    End If
                                Else
                                    CoreMessageHandler(message:="could not convert value from data reader", arg1:=datarow.Item(aColumnname), _
                                                       columnname:=aColumnname, tablename:=_tableid, subname:="ormRecord.LoadFrom(Datarow)")
                                    result = False
                                End If
                            Else
                                CoreMessageHandler(message:="column from table not in datareader - record uncomplete", columnname:=aColumnname, _
                                                   tablename:=_tableid, subname:="ormRecord.LoadFrom(Datarow)")
                                result = False
                            End If
                        Next j

                        Return result
                    Else
                        ''' take all the values from datareader and move it 
                        ''' 
                        For j = 0 To datarow.Table.Columns.Count - 1
                            Dim aColumnname As String = datarow.Table.Columns.Item(j).ColumnName
                            Dim aValue As Object = datarow.Item(j)

                            ''' how to convert ?!
                            ''' 
                            ''' datarow has system types !!
                            ''' Dim Outvalue = CTypeDynamic (avalue, atype)
                            '''
                            If Not SetValue(datarow.Table.TableName.ToUpper & "." & aColumnname.ToUpper, aValue) Then
                                CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                                   columnname:=aColumnname, tablename:=_tableid, subname:="ormRecord.LoadFrom(Datarow)")
                                result = False
                            Else
                                result = True
                            End If
                        Next

                        Return result
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormRecord.LoadFrom(Datarow)", exception:=ex, message:="Exception", tablename:=_tableid)
                    Return False
                End Try

            End Function

            ''' <summary>
            ''' load a record into this record from the datareader
            ''' </summary>
            ''' <param name="datareader"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function LoadFrom(ByRef datareader As IDataReader) As Boolean
                Dim result As Boolean = True

                Try
                    ''' if tableset then only check which fields are in the datareader
                    ''' 
                    _isLoaded = True ' important

                    If _IsTableSet Then
                        For j = 1 To _TableStore.TableSchema.NoFields
                            Dim found As Integer = -1
                            Dim aColumnname As String = _TableStore.TableSchema.Getfieldname(j)
                            For i = 0 To datareader.FieldCount - 1
                                If datareader.GetName(i) = aColumnname Then
                                    ''' uuuh slow
                                    ''' 
                                    found = i
                                    Exit For
                                End If
                            Next
                            If found >= 0 Then
                                Dim aValue As Object
                                If _TableStore.Convert2ObjectData(index:=j, invalue:=datareader.Item(found), outvalue:=aValue) Then
                                    If Not SetValue(j, aValue) Then
                                        CoreMessageHandler(message:="ormRecord.LoadFrom(IDataReader)", arg1:=aValue, columnname:=aColumnname, tablename:=_tableid, subname:="ormRecord.LoadFrom")
                                        result = False
                                    Else
                                        result = result And True
                                    End If
                                Else
                                    CoreMessageHandler(message:="ormRecord.LoadFrom(IDataReader)", arg1:=datareader.Item(aColumnname), columnname:=aColumnname, tablename:=_tableid, subname:="ormRecord.LoadFrom")
                                    result = False
                                End If
                            Else
                                CoreMessageHandler(message:="column from table not in datareader - record uncomplete", columnname:=aColumnname, _
                                                   tablename:=_tableid, subname:="ormRecord.LoadFrom(IDataReader)")
                                result = False
                            End If
                        Next j

                        Return result
                    Else
                        ''' take all the values from datareader and move it 
                        ''' 
                        For j = 0 To datareader.FieldCount - 1
                            Dim aName As String = datareader.GetName(j)
                            If aName = "" Then aName = j.ToString
                            Dim aValue As Object = datareader.Item(j)

                            ''' how to convert ?!
                            ''' we have already system type

                            If Not SetValue(aName, aValue) Then
                                CoreMessageHandler(message:="could not set value from data reader", arg1:=aValue, _
                                                    tablename:=_tableid, subname:="ormRecord.LoadFrom(IDataReader)")
                                result = False
                            Else
                                result = result And True
                            End If
                        Next

                        Return result
                    End If

                   
                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormRecord.LoadFrom(IDataReader)", exception:=ex, message:="Exception", _
                                          arg1:=_tableid)
                    Return False
                End Try
                
            End Function

            ''' <summary>
            ''' checkStatus if loaded or created by checking if Record exists in Table. Sets the isChanged / isLoaded Property
            ''' </summary>
            ''' <returns>true if successfully checked</returns>
            ''' <remarks></remarks>
            Public Function CheckStatus() As Boolean
                '** not loaded and not created but alive ?!
                If Not Me.IsLoaded And Not Me.IsCreated And Alive Then

                    Dim pkarr() As Object
                    Dim i, index As Integer
                    Dim value As Object
                    Dim aRecord As ormRecord
                    Try
                        ReDim pkarr(0 To _TableStore.TableSchema.NoPrimaryKeyFields - 1)
                        For i = 1 To _TableStore.TableSchema.NoPrimaryKeyFields
                            index = _TableStore.TableSchema.GetordinalOfPrimaryKeyField(i)
                            value = Me.GetValue(index)
                            pkarr(i - 1) = value
                        Next i
                        ' delete
                        aRecord = _TableStore.GetRecordByPrimaryKey(pkarr)
                        If Not aRecord Is Nothing Then
                            Me.IsLoaded = True
                        Else
                            Me.IsCreated = True
                        End If
                    Catch ex As Exception
                        Call CoreMessageHandler(exception:=ex, message:="Exception", messagetype:=otCoreMessageType.InternalException, _
                                              subname:="ormRecord.checkStatus")
                        Return False
                    End Try


                End If

                Return True
            End Function

            ''' <summary>
            ''' sets the default value to an index
            ''' </summary>
            ''' <param name="index"></param>
            ''' <param name="tableid"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetDefaultValue(index As Object) As Object
                Dim i As Integer

                If Not Me.Alive Or Not Me.IsTableBound Then
                    Return Nothing
                End If

                If IsNumeric(index) Then
                    i = CInt(index)
                Else
                    If Not _TableStore.TableSchema.Hasfieldname(index) Then
                        Return Nothing
                    Else
                        i = _TableStore.TableSchema.GetFieldordinal(index)
                    End If
                End If

                ' prevent overflow
                If Not (i > 0 And i <= _Values.Count) Then
                    Return Nothing
                End If

                '* set the default values
                '* do not allow recursion on objectentrydefinition table itself
                '* since this is not included 

                If _TableStore.TableID.ToLower <> AbstractEntryDefinition.ConstTableID.ToLower Then
                    ''' get default value out of the object entry store not from the db itself
                    ''' 
                    Dim anEntry As ColumnDefinition = CurrentSession.Objects.GetColumnEntry(columnname:=_TableStore.TableSchema.Getfieldname(i), tablename:=_TableStore.TableID)
                    If anEntry IsNot Nothing Then
                        Return anEntry.DefaultValue
                    Else
                        '* try to get defaults from the underlying database -> default might also be nothing
                        Return _TableStore.TableSchema.GetDefaultValue(i)
                    End If
                Else
                    '* try to get defaults from the underlying database -> default might also be nothing
                    Return _TableStore.TableSchema.GetDefaultValue(i)
                End If
            End Function
            ''' <summary>
            ''' Assign a TableStore to this Record
            ''' </summary>
            ''' <param name="TableID">Name of the Table</param>
            ''' <param name="ForceReload">Forece to reaassign</param>
            ''' <returns>True if ssuccessfull</returns>
            ''' <remarks></remarks>
            Public Function SetTable(ByVal tableID As String, _
                                     Optional dbdriver As iormDatabaseDriver = Nothing, _
                                     Optional tablestore As iormDataStore = Nothing, _
                                     Optional forceReload As Boolean = False, _
                                     Optional fillDefaultValues As Boolean = False) As Boolean

                If Not _IsTableSet Or forceReload Then

                    If tablestore Is Nothing Then
                        If dbdriver Is Nothing Then
                            tablestore = GetTableStore(tableID, force:=forceReload)
                        Else
                            tablestore = dbdriver.GetTableStore(tableID, force:=forceReload)
                        End If
                    End If


                    If Not tablestore Is Nothing AndAlso Not tablestore.TableSchema Is Nothing _
                        AndAlso tablestore.TableSchema.IsInitialized Then

                        _TableStore = tablestore
                        _IsTableSet = True
                        _FixEntries = True

                        ' get the number of fields
                        If _TableStore.TableSchema.NoFields > 0 Then

                            '*** if there have been entries before or was set to another table
                            '*** preserve as much as possible
                            If _entrynames.GetUpperBound(0) > 0 Then
                                '** re-sort 
                                Dim newValues(_TableStore.TableSchema.NoFields - 1) As Object
                                Dim newOrigValues(_TableStore.TableSchema.NoFields - 1) As Object
                                For Each fieldname In tablestore.TableSchema.Fieldnames
                                    If _entrynames.Contains(fieldname) Then
                                        newValues(_TableStore.TableSchema.GetFieldordinal(fieldname) - 1) = _Values(Array.IndexOf(_entrynames, fieldname))
                                        newOrigValues(_TableStore.TableSchema.GetFieldordinal(fieldname) - 1) = _OriginalValues(Array.IndexOf(_entrynames, fieldname))
                                    End If
                                Next
                                '** change over
                                _Values = newValues
                                _OriginalValues = newOrigValues
                                _entrynames = tablestore.TableSchema.Fieldnames.ToArray
                            Else
                                '*** redim else and set the default values
                                ReDim Preserve _Values(0 To _TableStore.TableSchema.NoFields - 1)
                                ReDim Preserve _OriginalValues(0 To _TableStore.TableSchema.NoFields - 1)
                                _entrynames = tablestore.TableSchema.Fieldnames.ToArray
                                '* set the default values
                                If fillDefaultValues Then
                                    For i = 1 To _TableStore.TableSchema.NoFields
                                        If Not tablestore.TableSchema.GetNullable(i) Then
                                            _Values(i - 1) = Me.GetDefaultValue(i)
                                        Else
                                            _Values(i - 1) = Nothing
                                        End If

                                        _OriginalValues(i - 1) = _Values(i - 1)
                                    Next
                                End If
                            End If
                        End If
                        Return _IsTableSet

                    Else
                        Call CoreMessageHandler(message:="Tablestore or tableschema is not initialized", subname:="ormRecord.setTable", _
                                              messagetype:=otCoreMessageType.InternalError, tablename:=tableID)
                        Return False
                    End If
                    Return False
                Else
                    Return True 'already set
                End If
            End Function
            ''' <summary>
            ''' persists the Record in the Database
            ''' </summary>
            ''' <param name="aTimestamp">Optional TimeStamp for using the persist</param>
            ''' <returns>true if successfull</returns>
            ''' <remarks></remarks>
            Public Function Persist(Optional ByVal timestamp As Date = ot.ConstNullDate) As Boolean
                '** try to set the table
                If Not _IsTableSet And _tableid <> "" Then
                    Me.SetTable(tableID:=_tableid)
                End If
                '** only on success
                If _IsTableSet Then
                    If timestamp = ConstNullDate Then timestamp = Date.Now
                    '' check for status
                    If Not Me.IsCreated AndAlso Not Me.IsLoaded Then CheckStatus()
                    '* switch to loaded
                    If _TableStore.PersistRecord(Me, timestamp:=timestamp) Then
                        Me.IsLoaded = True
                        Me.IsCreated = False
                        Me.IsChanged = False
                        Return True
                    End If
                End If
                Return False
            End Function

            ''' <summary>
            ''' Deletes the Record in the Database
            ''' </summary>
            ''' <returns>true if successfull</returns>
            ''' <remarks></remarks>

            Public Function Delete() As Boolean
                Dim pkarr() As Object
                Dim i, index As Integer
                Dim Value, cvtvalue As Object

                If _IsTableSet Then
                    ReDim pkarr(0 To _TableStore.TableSchema.NoPrimaryKeyFields - 1)
                    For i = 0 To _TableStore.TableSchema.NoPrimaryKeyFields - 1
                        index = _TableStore.TableSchema.GetordinalOfPrimaryKeyField(i + 1)
                        Value = Me.GetValue(index)
                        ' cvtvalue = s_Table.cvt2ColumnData(index, value) -> done by delRecord
                        pkarr(i) = Value
                    Next i
                    ' delete
                    Return _TableStore.DelRecordByPrimaryKey(pkarr)
                    Exit Function
                Else
                    Call CoreMessageHandler(subname:="ormRecord.delete", message:="Record not bound to a TableStore", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                Delete = False
            End Function
            ''' <summary>
            ''' returns true if the record has the index either numerical (1..) or by name
            ''' a tablename in form [tablename].[columnname] will be stripped of and checked too 
            ''' </summary>
            ''' <param name="anIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function HasIndex(index As Object) As Boolean
                If IsNumeric(index) Then
                    Dim i = CInt(index)
                    If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    '** strip tablename only check on set tables
                    Dim names = index.ToString.Split({CChar(ConstDelimiter), "."c})
                    If names.Count > 1 Then
                        If _IsTableSet Then
                            If _TableStore.TableID.ToUpper <> names(0).ToUpper Then
                                Return False  'wrong table
                            End If
                        End If
                        index = names(1)
                    Else
                        index = index.ToString
                    End If

                    Return Me.Keys.Find(Function(x)
                                            Return x.ToUpper = index.ToString.ToUpper
                                        End Function) IsNot Nothing
                End If

            End Function

            ''' <summary>
            ''' retus a list of the primaryKeys
            ''' </summary>
            ''' <returns>List(of String)</returns>
            ''' <remarks></remarks>
            Public Function Keys() As List(Of String)
                ' no table ?!
                If Not Me.Alive Then
                    Keys = New List(Of String)
                    Exit Function
                ElseIf _IsTableSet Then
                    Keys = _TableStore.TableSchema.Fieldnames
                Else
                    Keys = _entrynames.ToList
                End If
            End Function
            ''' <summary>
            ''' returns True if Value of anIndex is Changed
            ''' </summary>
            ''' <param name="anIndex">index in Number 1..n or fieldname</param>
            ''' <returns>True on Change</returns>
            ''' <remarks></remarks>
            Public Function IsValueChanged(ByVal anIndex As Object) As Boolean
                Dim i As Integer

                ' no table ?!
                If Not _IsTableSet Then
                    Call CoreMessageHandler(subname:="ormRecord.isValueChanged", arg1:=anIndex, message:="record is not bound to table")
                    Return False
                End If

                If IsNumeric(anIndex) Then
                    i = CInt(anIndex)
                Else
                    i = _TableStore.TableSchema.GetFieldordinal(anIndex)
                End If
                ' set the value
                If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                    If (Not _OriginalValues(i - 1) Is Nothing AndAlso Not _OriginalValues(i - 1).Equals(_Values(i - 1)) _
                        OrElse IsCreated) Then
                        Return True
                    Else
                        _isChanged = _isChanged And False
                        Return False
                    End If

                Else

                    Call CoreMessageHandler(message:="Index of " & anIndex & " is out of bound of OTDBTableEnt '" & _TableStore.TableID & "'", _
                                          subname:="ormRecord.isIndexChangedValue", arg1:=anIndex, entryname:=anIndex, tablename:=_TableStore.TableID, noOtdbAvailable:=True)
                    Return False
                End If

            End Function
            ''' <summary>
            ''' sets the record to an array
            ''' </summary>
            ''' <param name="array"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function [Set](ByRef [array] As Object(), Optional ByRef names As Object() = Nothing) As Boolean
                ' no table ?!
                If Not Me.Alive Then
                    Return False
                End If
                '** fixed ?!
                Try
                    If _Values.GetUpperBound(0) > 0 Then
                        If [array].GetUpperBound(0) <> _Values.GetUpperBound(0) Then
                            CoreMessageHandler(message:="input array has different upper bound than the set values array", arg1:=[array].GetUpperBound(0), _
                                                messagetype:=otCoreMessageType.InternalError)
                            Return False
                        Else
                            _OriginalValues = _Values.Clone
                            _Values = [array].Clone
                            If Not names Is Nothing Then
                                _entrynames = names.Clone
                            End If
                            Return True
                        End If
                    Else
                        ReDim _Values([array].Length)
                        ReDim _OriginalValues([array].Length)
                        _Values = [array].Clone
                        If Not names Is Nothing Then
                            _entrynames = names.Clone
                        End If
                    End If

                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormRecord.Set")
                    Return False
                End Try



            End Function


            ''' <summary>
            ''' set the Value of an Entry of the Record
            ''' </summary>
            ''' <param name="anIndex">Index as No 1...n or name or [tablename].[columnname]</param>
            ''' <param name="anValue">value</param>
            ''' <param name="FORCE"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function SetValue(ByVal index As Object, ByVal value As Object, Optional ByVal force As Boolean = False) As Boolean
                Dim i As Long
                Dim isNullable As Boolean = False

                Try
                    ' no table ?!
                    If Not Me.Alive And Not force Then
                        SetValue = False
                        Exit Function
                    End If
                    '*
                    If DBNull.Value.Equals(value) Then
                        value = Nothing
                    End If
                    '*** if fixed entries
                    If _IsTableSet Then
                        If IsNumeric(index) Then
                            i = CLng(index)

                        Else
                            '** strip tablename only check on set tables
                            Dim names = index.ToString.Split({CChar(ConstDelimiter), "."c})
                            If names.Count > 1 Then
                                If _TableStore.TableID.ToLower <> LCase(names(0)) Then
                                    CoreMessageHandler(message:="column name has wrong table id", arg1:=index, tablename:=_TableStore.TableID, _
                                                        messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.SetValue")
                                    Return False  'wrong table
                                End If
                                index = names(1)
                            Else
                                index = index.ToString
                            End If
                            '** get index
                            i = _TableStore.TableSchema.GetFieldordinal(index)
                        End If
                        isNullable = _TableStore.TableSchema.GetNullable(index)
                        '*** else dynamic extend
                    Else
                        Dim found As Boolean = False

                        If IsNumeric(index) Then
                            If (index - 1) < _Values.GetUpperBound(0) Then
                                i = index
                                found = True
                            End If
                        Else
                            '** strip tablename only check on set tables
                            Dim names = index.ToString.Split({CChar(ConstDelimiter), "."c})
                            If names.Count > 1 Then
                                index = names(1)
                            Else
                                index = index.ToString
                            End If

                            '** compare the entry names
                            For j = 0 To _entrynames.GetUpperBound(0)
                                If LCase(_entrynames(j)) = index.tolower Then
                                    i = j + 1
                                    found = True
                                    Exit For
                                End If
                            Next
                        End If
                        '** extend
                        If Not found Then
                            i = _entrynames.GetUpperBound(0) + 1
                            ReDim Preserve _entrynames(i)
                            ReDim Preserve _Values(i)
                            ReDim Preserve _OriginalValues(i)
                            Dim anIndex As String = CStr(index)
                            If anIndex.Contains(".") Then
                                anIndex = LCase(anIndex.Substring(anIndex.IndexOf(".") + 1, anIndex.Length - (anIndex.IndexOf(".") + 1)))
                            Else
                                anIndex = anIndex.ToLower
                            End If
                            _entrynames(i) = anIndex
                            i = i + 1
                        End If

                    End If

                    ' set the value
                    If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                        _OriginalValues(i - 1) = _Values(i - 1)
                        If (value Is Nothing AndAlso isNullable) Then
                            _Values(i - 1) = Nothing
                        ElseIf value Is Nothing AndAlso isNullable AndAlso Reflector.IsNullableTypeOrString(value) Then
                            _Values(i - 1) = Nothing
                        ElseIf value Is Nothing And Not isNullable Then
                            _Values(i - 1) = GetDefaultValue(i)
                        Else
                            _Values(i - 1) = value
                        End If

                        If _OriginalValues(i - 1) Is Nothing Then
                            _isChanged = False
                        ElseIf (Not _OriginalValues(i - 1) Is Nothing And Not _Values(i - 1) Is Nothing) _
                            AndAlso ((_OriginalValues(i - 1).GetType().Equals(_Values(i - 1)) AndAlso _OriginalValues(i - 1) <> _Values(i - 1))) _
                            OrElse (Not _OriginalValues(i - 1).GetType().Equals(_Values(i - 1))) Then
                            _isChanged = True
                        ElseIf (Not _OriginalValues(i - 1) Is Nothing And _Values(i - 1) Is Nothing) Then
                            _isChanged = True
                        End If
                    Else

                        Call CoreMessageHandler(message:="Index of " & index & " is out of bound of OTDBTableEnt '" & _TableStore.TableID & "'", _
                                              subname:="ormRecord.setValue", arg1:=value, entryname:=index, tablename:=_TableStore.TableID, noOtdbAvailable:=True)
                        SetValue = False
                        Return SetValue
                    End If

                    Return True


                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormRecord.setValue", exception:=ex)
                    Return False
                End Try


            End Function
            ''' <summary>
            ''' returns True if the indexed entry in the record is null or doesnot exist
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function IsNull(index As Object) As Boolean
                Dim nullvalue As Boolean
                Dim notfound As Boolean
                If Not Me.HasIndex(index:=index) Then Return False
                Dim avalue As Object = Me.GetValue(index:=index, isNull:=nullvalue, notFound:=notfound)
                Return nullvalue
            End Function
            ''' <summary>
            ''' gets the Value of an Entry of the Record
            ''' </summary>
            ''' <param name="anIndex">Index 1...n or name of the Field</param>
            ''' <returns>the value as object or Null of not found</returns>
            ''' <remarks></remarks>
            Public Function GetValue(index As Object, Optional ByRef isNull As Boolean = False, Optional ByRef notFound As Boolean = False) As Object
                Dim i As Long
                Dim isNullable As Boolean = False

                Try

                    ' no table ?!
                    If Not Me.Alive Then
                        GetValue = False
                        Exit Function
                    End If

                    '*** if fixed entries
                    If _IsTableSet Then
                        If IsNumeric(index) Then
                            i = CLng(index)
                        Else
                            '** strip tablename only check on set tables
                            Dim names = index.ToString.Split({CChar(ConstDelimiter), "."c})
                            If names.Count > 1 Then
                                If _TableStore.TableID.ToLower <> LCase(names(0)) Then
                                    CoreMessageHandler(message:="column name has wrong table id", arg1:=index, tablename:=_TableStore.TableID, _
                                                        messagetype:=otCoreMessageType.InternalError, subname:="ormRecord.GetValue")
                                    Return Nothing  'wrong table
                                End If
                                index = names(1)
                            Else
                                index = index.ToString
                            End If

                            i = _TableStore.TableSchema.GetFieldordinal(index)
                        End If
                        isNullable = _TableStore.TableSchema.GetNullable(index)
                    Else
                        If IsNumeric(index) Then
                            i = CLng(index)
                        Else
                            Dim found As Boolean
                            '** strip tablename only check on set tables
                            Dim names = index.ToString.Split({CChar(ConstDelimiter), "."c})
                            If names.Count > 1 Then
                                index = names(1)
                            Else
                                index = index.ToString
                            End If

                            For j = 0 To _entrynames.GetUpperBound(0)
                                If LCase(_entrynames(j)) = index.tolower Then
                                    i = j + 1
                                    found = True
                                    Exit For
                                End If
                            Next

                            If Not found Then
                                Call CoreMessageHandler(message:="the non-numeric index of '" & index & "' does not exist in record ", _
                                            subname:="ormRecord.getValue", messagetype:=otCoreMessageType.InternalError)
                                notFound = True
                                Return Nothing
                            End If
                        End If
                    End If

                    ' Get the value
                    If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                        If DBNull.Value.Equals(_Values(i - 1)) Then
                            isNull = True
                            Return Nothing
                        Else
                            isNull = False
                            Return _Values(i - 1)
                        End If
                    Else
                        Call CoreMessageHandler(message:="Index of " & index & " is out of bound of tablestore or doesnot exist in record '" & _TableStore.TableID & "'", _
                                              subname:="ormRecord.getValue", entryname:=index, tablename:=_TableStore.TableID, messagetype:=otCoreMessageType.InternalError)
                        notFound = True
                        Return DBNull.Value
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="ormRecord.getValue", exception:=ex)
                    Return DBNull.Value
                End Try
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
            Protected _Connectionstring As String = ""  'the  Connection String
            Protected _Path As String = ""  'where the database is if access
            Protected _Name As String = ""  'name of the database or file
            Protected _Dbuser As String = ""  'User name to use to access the database
            Protected _Dbpassword As String = ""   'password to use to access the database
            Protected _Sequence As ComplexPropertyStore.Sequence = ComplexPropertyStore.Sequence.Primary ' configuration sequence of the connection
            'Protected _OTDBUser As New User    ' OTDB User -> moved to session 
            Protected _AccessLevel As otAccessRight    ' access

            Protected _UILogin As CoreLoginForm
            Protected _cacheUserValidateon As UserValidation
            Protected _OTDBDatabaseDriver As iormDatabaseDriver
            Protected _useseek As Boolean 'use seek instead of SQL
            Protected _lockObject As New Object ' use lock object for sync locking

            Protected WithEvents _ErrorLog As MessageLog
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
            Public Sub New(id As String, databasedriver As iormDatabaseDriver, ByRef session As Session, sequence As ComplexPropertyStore.Sequence)
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
            Public Property DatabaseDriver() As iormDatabaseDriver Implements iormConnection.DatabaseDriver
                Get
                    Return _OTDBDatabaseDriver
                End Get
                Friend Set(value As iormDatabaseDriver)
                    _OTDBDatabaseDriver = value
                End Set
            End Property

            ''' <summary>
            ''' Gets the error log.
            ''' </summary>
            ''' <value>The error log.</value>
            Public ReadOnly Property ErrorLog() As MessageLog Implements iormConnection.ErrorLog
                Get
                    If _ErrorLog Is Nothing Then
                        _ErrorLog = New MessageLog(My.Computer.Name & "-" & My.User.Name & "-" & Date.Now.ToUniversalTime)
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
                '_Connectionstring = ""

                '_Path = ""
                '_Name = ""
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
                    CoreMessageHandler(message:="current config set name was changed after connection is connected -ignored", subname:="ormConnection.OnCurrentConfigSetChanged", arg1:=e.Setname, messagetype:=otCoreMessageType.InternalError)
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
                                            messagetype:=otCoreMessageType.InternalInfo, subname:="ormConnection.SetconnectionConfigParameters")
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
                                          messagetype:=otCoreMessageType.InternalInfo, subname:="ormConnection.SetconnectionConfigParameters")
                            Return True
                        Else
                            Call CoreMessageHandler(showmsgbox:=True, arg1:=_Path & _Name, subname:="ormConnection.retrieveConfigParameters", _
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
                        Call CoreMessageHandler(message:="Config connection parameters :" & Me.ID & vbLf & _
                                          " created connectionsstring :" & Me.Connectionstring, _
                                          messagetype:=otCoreMessageType.InternalInfo, subname:="ormConnection.SetconnectionConfigParameters")
                        Return True
                    Else
                        Call CoreMessageHandler(showmsgbox:=True, arg1:=_Connectionstring, subname:="ormConnection.retrieveConfigParameters", _
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
            Optional ByVal domain As String = "", _
            Optional ByVal OTDBUsername As String = "", _
            Optional ByVal OTDBPassword As String = "", _
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
                                                  Optional domain As String = "", _
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
                                                Optional ByRef username As String = "", _
                                                Optional ByRef password As String = "", _
                                                Optional ByRef domainID As String = "", _
                                                Optional ByRef [Objectnames] As List(Of String) = Nothing, _
                                                Optional useLoginWindow As Boolean = True, Optional messagetext As String = Nothing) As Boolean Implements iormConnection.VerifyUserAccess
                Dim userValidation As UserValidation
                userValidation.ValidEntry = False

                '****
                '**** no connection -> login
                If Not Me.IsConnected Then

                    If domainID = "" Then domainID = ConstGlobalDomain
                    '*** OTDBUsername supplied

                    If useLoginWindow And accessRequest <> ConstDefaultAccessRight Then

                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = ""
                        Me.UILogin.Password = ""

                        'LoginWindow
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.PossibleConfigSets = ot.ConfigSetNamesToSelect
                        'Me.UILogin.Databasedriver = Me.DatabaseDriver
                        Me.UILogin.EnableChangeConfigSet = True
                        If messagetext IsNot Nothing Then Me.UILogin.Messagetext = messagetext

                        Me.UILogin.Domain = domainID
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
                    ElseIf username <> "" And password <> "" And accessRequest <> ConstDefaultAccessRight Then
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* no username but default accessrequest then look for the anonymous user
                    ElseIf accessRequest = ConstDefaultAccessRight Then
                        userValidation = Me.DatabaseDriver.GetUserValidation(username:="", selectAnonymous:=True)
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
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, message:="User verified successfully *", _
                                                  arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        Else
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, message:="User not verified successfully", _
                                                  arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)
                            _cacheUserValidateon.ValidEntry = False
                            Return False
                        End If

                    End If

                    '****
                    '**** CONNECTION !
                Else
                    '** stay in the current domain 
                    If domainID = "" Then domainID = ot.CurrentSession.CurrentDomainID
                    '** validate the current user with the request
                    If Me.ValidateAccessRequest(accessrequest:=accessRequest, domain:=domainID) Then
                        Return True
                        '* change the current user if anonymous
                    ElseIf useLoginWindow And ot.CurrentSession.OTdbUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainID
                        Me.UILogin.EnableDomain = False
                        Me.UILogin.PossibleDomains = New List(Of String)
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.EnableChangeConfigSet = False
                        Me.UILogin.Accessright = accessRequest
                        Me.UILogin.Messagetext = "<html><strong>Welcome !</strong><br />Please change to a valid user and password for authorization of the needed access right.</html>"
                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = ""
                        Me.UILogin.Password = ""
                        Me.UILogin.Show()
                        username = LoginWindow.Username
                        password = LoginWindow.Password
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* check password -> relogin on connected -> EventHandler ?!
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, _
                                                    message:="User change verified successfully on domain '" & domainID & "'", _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
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
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                            Return False
                        End If
                        '* the current access level is not for this request
                    ElseIf useLoginWindow And Not CurrentSession.OTdbUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainID
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
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, message:="User change verified successfully (1)", _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                            '* set the new access level
                            _AccessLevel = accessRequest
                        Else
                            '** fallback
                            username = CurrentSession.OTdbUser.Username
                            password = CurrentSession.OTdbUser.Password
                            Call CoreMessageHandler(subname:="ormConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                            Return False
                        End If

                        '*** just check the provided username
                    ElseIf username <> "" And password <> "" Then
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

            Public Sub New(newConnection As iormConnection, Optional domain As String = "")
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

        '************************************************************************************
        '***** CLASS clsOTDBFieldDesc is a helper for the FieldDesc Attributes
        '*****
        '*****

        Public Class ormFieldDescription
            ''' <summary>
            ''' Name in the table (data store)
            ''' </summary>
            ''' <remarks></remarks>
            Public ColumnName As String = ""
            ''' <summary>
            ''' ID for XChange Manager
            ''' </summary>
            ''' <remarks></remarks>
            Public ID As String = ""
            ''' <summary>
            ''' Default Title to be used on column heads
            ''' </summary>
            ''' <remarks></remarks>
            Public Title As String = ""
            ''' <summary>
            ''' Description about the Field
            ''' </summary>
            ''' <remarks></remarks>
            Public Description As String = ""
            ''' <summary>
            ''' Aliases to be used for XChange Manager (Array)
            ''' </summary>
            ''' <remarks></remarks>
            Public Aliases As String() = {}
            ''' <summary>
            ''' OTDB Datatype of the Field
            ''' </summary>
            ''' <remarks></remarks>
            Public Datatype As otFieldDataType
            ''' <summary>
            ''' Parameters to be used
            ''' </summary>
            ''' <remarks></remarks>
            Public Parameter As String = ""
            ''' <summary>
            ''' Tablename of the Datastore
            ''' </summary>
            ''' <remarks></remarks>
            Public Tablename As String = ""
            ''' <summary>
            ''' Relation Description as String Array
            ''' </summary>
            ''' <remarks></remarks>
            Public Relation As String() = {}
            ''' <summary>
            ''' Size
            ''' </summary>
            ''' <remarks></remarks>
            Public Size As Long = 255
            ''' <summary>
            ''' Is Nullable
            ''' </summary>
            ''' <remarks></remarks>
            Public IsNullable As Boolean = False
            ''' <summary>
            ''' Is Transformed to an Array
            ''' </summary>
            ''' <remarks></remarks>
            Public IsArray As Boolean = False
            ''' <summary>
            ''' DefaultValue of the Field
            ''' </summary>
            ''' <remarks></remarks>
            Public DefaultValue As Object
            ''' <summary>
            ''' Version count
            ''' </summary>
            ''' <remarks></remarks>
            Public Version As UShort
            ''' <summary>
            ''' Position in the Record
            ''' </summary>
            ''' <remarks></remarks>
            Public ordinalPosition As UShort
            ''' <summary>
            ''' if set true this Field is a spare field
            ''' </summary>
            ''' <remarks></remarks>
            Public SpareFieldTag As Boolean

        End Class


        '************************************************************************************
        '***** CLASS clsOTDBCompoundDecs is a helper for the CompoundsDesc Attributes
        '***** a compound are data tupples which apear to be for the XChange Manager in the 
        '***** base class but are as relation with a parameter id in a sub class and another table
        '***** such as milestones which are parameters from the schedule definition
        '*****

        Public Class ormCompoundDesc
            Inherits ormFieldDescription

            '*** Additional Compound Information
            ''' <summary>
            ''' the tablename in the datastore of the compound
            ''' </summary>
            ''' <remarks></remarks>
            Public compound_Tablename As String
            ''' <summary>
            ''' relation condition fields
            ''' </summary>
            ''' <remarks></remarks>
            Public compound_Relation As Object
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <remarks></remarks>
            Public compound_IDFieldname As String
            ''' <summary>
            ''' fieldname which has the ID of the compound field as value
            ''' </summary>
            ''' <remarks></remarks>
            Public compound_ValueFieldname As String

        End Class

        
        ''' <summary>
        ''' TopLevel OTDB Tablestore implementation base class
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormTableStore
            Implements iormDataStore

            Private _TableID As String 'Name of the Table or Datastore in the Database
            Private _TableSchema As iotDataSchema  'Schema (Description) of the Table or DataStore
            Private _Connection As iormConnection  ' Connection to use to access the Table or Datastore

            Private _PropertyBag As New Dictionary(Of String, Object)

            '*** Tablestore Cache Property names
            ''' <summary>
            ''' Table Property Name "Cache Property"
            ''' </summary>
            ''' <remarks></remarks>
            Public Const ConstTPNCacheProperty = "CacheDataTable"
            ''' <summary>
            ''' Table Property Name "Cache Update Instant"
            ''' </summary>
            ''' <remarks></remarks>
            Public Const ConstTPNCacheUpdateInstant = "CacheDataTableUpdateImmediatly"
            ''' <summary>
            ''' Table Property Name for FULL CACHING
            ''' </summary>
            ''' <remarks></remarks>
            Private Const ConstTPNFullCaching = "FULL"
            ''' <summary>
            ''' constuctor
            ''' </summary>
            ''' <param name="connection"></param>
            ''' <param name="tableID"></param>
            ''' <param name="force"></param>
            ''' <remarks></remarks>
            Protected Sub New(connection As iormConnection, tableID As String, ByVal force As Boolean)
                Call MyBase.New()

                Me.Connection = connection
                Me.TableID = tableID

                Refresh(force:=True)

            End Sub
            ''' <summary>
            ''' creates an unique key value. provide primary key array in the form {field1, field2, nothing}. "Nothing" will be increased.
            ''' </summary>
            ''' <param name="pkArray"></param>
            ''' <remarks></remarks>
            ''' <returns>True if successfull new value</returns>
            Public Overridable Function CreateUniquePkValue(ByRef pkArray() As Object) As Boolean Implements iormDataStore.CreateUniquePkValue

                '**
                If Not Me.TableSchema.IsInitialized Then
                    Return False
                End If

                '** redim 
                ReDim Preserve pkArray(Me.TableSchema.NoPrimaryKeyFields() - 1)
                Dim anIndex As UShort = 0
                Dim keyfieldname As String

                Try
                    ' get
                    Dim aStore As iormDataStore = GetTableStore(_TableID)
                    Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="CreateUniquePkValue", addMe:=True, addAllFields:=False)

                    '** prepare the command if necessary

                    ''' this command lives from the first call !! -> all elements in pkArray not fixed will be regarded as elements to be fixed
                    If Not aCommand.Prepared Then
                        '* retrieve the maximum field
                        For Each pkvalue In pkArray
                            If pkvalue Is Nothing Then
                                keyfieldname = TableSchema.GetPrimaryKeyfieldname(anIndex + 1)
                                Exit For
                            End If
                            anIndex += 1
                        Next
                        '*
                        aCommand.select = "max( [" & keyfieldname & "] )"
                        If anIndex > 0 Then
                            For j = 0 To anIndex - 1 ' an index points to the keyfieldname, parameter is the rest
                                If j > 0 Then aCommand.Where &= " AND "
                                aCommand.Where &= "[" & TableSchema.GetPrimaryKeyfieldname(j + 1) & "] = @" & TableSchema.GetPrimaryKeyfieldname(j + 1)
                                aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & TableSchema.GetPrimaryKeyfieldname(j + 1), _
                                                                                     columnname:=TableSchema.GetPrimaryKeyfieldname(j + 1), tablename:=Me.TableID))
                            Next
                        End If
                        aCommand.Prepare()
                    End If

                    '* retrieve the maximum field -> and sets the index
                    anIndex = 0
                    For Each pkvalue In pkArray
                        If Not pkvalue Is Nothing Then
                            aCommand.SetParameterValue(ID:="@" & TableSchema.GetPrimaryKeyfieldname(anIndex + 1), value:=pkvalue)
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
                            pkArray(anIndex) = 1
                            Return True
                        End If

                    Else
                        pkArray(anIndex) = 1
                        Return True
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, exception:=ex, subname:="clsOTDBTableStore.CreateUniquePkValue")
                    Return False
                End Try


            End Function

            ''' <summary>
            ''' Refresh
            ''' </summary>
            ''' <param name="force"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function Refresh(Optional ByVal force As Boolean = False) As Boolean Implements iormDataStore.Refresh
                ''' TODO: on Connection Refresh
                '** 
                If Not Connection Is Nothing AndAlso (Connection.IsConnected OrElse Connection.Session.IsBootstrappingInstallationRequested) Then

                    '** all cache properties for tables used in starting up will be determined
                    '** by schema
                    If CurrentSession.IsStartingUp Then
                        Dim aTable = ot.GetSchemaTableAttribute(TableID)
                        If aTable IsNot Nothing Then
                            If aTable.HasValueUseCache AndAlso aTable.UseCache Then
                                If Not aTable.HasValueCacheProperties Then
                                    Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                                Else
                                    '** set properties
                                    Dim ext As String = ""
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
                        Dim aTable = CurrentSession.Objects.GetTable(tablename:=TableID)
                        If aTable IsNot Nothing Then
                            If aTable.UseCache And aTable.CacheProperties.Count = 0 Then
                                Me.SetProperty(ConstTPNCacheProperty, ConstTPNFullCaching)
                            Else
                                '** set properties
                                Dim ext As String = ""
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
                    If _TableSchema Is Nothing OrElse force Then _TableSchema = Connection.DatabaseDriver.GetTableSchema(TableID, force:=force)
                    If _TableSchema Is Nothing OrElse Not _TableSchema.IsInitialized Then
                        Call CoreMessageHandler(break:=True, message:=" Schema for TableID '" & TableID & "' couldnot be loaded", tablename:=TableID, _
                                              messagetype:=otCoreMessageType.InternalError, subname:="clsOTDBTablestore.Refresh")
                        Return False
                    End If
                End If
            End Function


            ''' <summary>
            ''' Gets or sets the table ID.
            ''' </summary>
            ''' <value>The table ID.</value>
            Public Property TableID() As String Implements iormDataStore.TableID
                Get
                    Return Me._TableID
                End Get
                Set(value As String)
                    Me._TableID = value.ToUpper
                End Set
            End Property

            ''' <summary>
            ''' Gets the records by SQL command.
            ''' </summary>
            ''' <param name="sqlcommand">The sqlcommand.</param>
            ''' <param name="parameters">The parameters.</param>
            ''' <returns></returns>
            Public MustOverride Function GetRecordsBySqlCommand(ByRef sqlcommand As ormSqlSelectCommand, Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing) As List(Of ormRecord) Implements iormDataStore.GetRecordsBySqlCommand
            

            ''' <summary>
            ''' Gets or sets the connection.
            ''' </summary>
            ''' <value>The connection.</value>
            Public Overridable Property Connection() As iormConnection Implements iormDataStore.Connection
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
            Public Overridable Property TableSchema() As iotDataSchema Implements iormDataStore.TableSchema
                Get
                    Return _TableSchema
                End Get
                Friend Set(value As iotDataSchema)
                    _TableSchema = value
                End Set
            End Property
            ''' <summary>
            ''' sets a Property to the TableStore
            ''' </summary>
            ''' <param name="Name">Name of the Property</param>
            ''' <param name="Object">ObjectValue</param>
            ''' <returns>returns True if succesfull</returns>
            ''' <remarks></remarks>
            Public Function SetProperty(ByVal name As String, ByVal value As Object) As Boolean Implements iormDataStore.SetProperty
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
            Public Function GetProperty(ByVal name As String) As Object Implements iormDataStore.GetProperty
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
            Public Function HasProperty(ByVal name As String) As Boolean Implements iormDataStore.HasProperty
                Return _PropertyBag.ContainsKey(name)
            End Function
            ''' <summary>
            ''' Dels the record by primary key.
            ''' </summary>
            ''' <param name="aKeyArr">A key arr.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function DelRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As Boolean Implements iormDataStore.DelRecordByPrimaryKey
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
                Implements iormDataStore.RunSqlCommand
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' Gets the record by primary key.
            ''' </summary>
            ''' <param name="aKeyArr">A key arr.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public Overridable Function GetRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As ormRecord Implements iormDataStore.GetRecordByPrimaryKey
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
            Public Overridable Function GetRecordsBySql(wherestr As String, Optional fullsqlstr As String = "", _
                                                         Optional innerjoin As String = "", Optional orderby As String = "", _
                                                         Optional silent As Boolean = False, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) Implements iormDataStore.GetRecordsBySql
                Throw New NotImplementedException
            End Function
            ''' <summary>
            ''' Is Linq in this TableStore available
            ''' </summary>
            ''' <value>True if available</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable ReadOnly Property IsLinqAvailable As Boolean Implements iormDataStore.IsLinqAvailable
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
            Public Overridable Function GetRecordsbySQlCommand(id As String, Optional wherestr As String = "", Optional fullsqlstr As String = "", _
                                                   Optional innerjoin As String = "", Optional orderby As String = "", Optional silent As Boolean = False, _
                                                   Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) _
                                               Implements iormDataStore.GetRecordsBySqlCommand
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
            Public Overridable Function GetRecordsByIndex(indexname As String, ByRef keysArray As Object(), Optional silent As Boolean = False) As List(Of ormRecord) Implements iormDataStore.GetRecordsByIndex
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
            Public Overridable Function InfuseRecord(ByRef newRecord As ormRecord, ByRef RowObject As Object, Optional ByVal silent As Boolean = False, Optional CreateNewRecord As Boolean = False) As Boolean Implements iormDataStore.InfuseRecord
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
            Public Overridable Function PersistRecord(ByRef record As ormRecord, Optional timestamp As DateTime = ot.ConstNullDate, Optional ByVal silent As Boolean = False) As Boolean Implements iormDataStore.PersistRecord
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
                Implements iormDataStore.RunSqlStatement
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
            Public MustOverride Function Convert2ColumnData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                        targetType As Long, _
                                                        Optional ByVal maxsize As Long = 0, _
                                                       Optional ByRef abostrophNecessary As Boolean = False, _
                                                       Optional ByVal fieldname As String = "", _
                                                        Optional isnullable As Boolean? = Nothing, _
                                                        Optional defaultvalue As Object = Nothing _
                                                    ) As Boolean Implements iormDataStore.Convert2ColumnData


            ''' <summary>
            ''' Convert2s the column data.
            ''' </summary>
            ''' <param name="anIndex">An index.</param>
            ''' <param name="aVAlue">A V alue.</param>
            ''' <param name="abostrophNecessary">The abostroph necessary.</param>
            ''' <returns></returns>
            Public Overridable Function Convert2ColumnData(index As Object, ByVal invalue As Object, ByRef outvalue As Object, _
                                                           Optional ByRef abostrophNecessary As Boolean = False, _
                                                           Optional isnullable As Boolean? = Nothing, _
                                                        Optional defaultvalue As Object = Nothing _
                                                    ) As Boolean Implements iormDataStore.Convert2ColumnData
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
                                                           Optional ByRef abostrophNecessary As Boolean = False) As Boolean Implements iormDataStore.Convert2ObjectData
                ' TODO: Implement this method
                Throw New NotImplementedException()
            End Function
            ''' <summary>
            ''' checks if SqlCommand is in Store of the driver
            ''' </summary>
            ''' <param name="id">id of the command</param>
            ''' <returns>True if successful</returns>
            ''' <remarks></remarks>
            Public Overridable Function HasSqlCommand(id As String) As Boolean Implements iormDataStore.HasSqlCommand

            End Function
            ''' <summary>
            ''' Store the Command by its ID - replace if existing
            ''' </summary>
            ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
            ''' <returns>true if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean Implements iormDataStore.StoreSqlCommand
                sqlCommand.ID = Me.GetSqlCommandID(sqlCommand.ID)

                Dim anExistingSqlCommand As iormSqlCommand
                If Me.Connection.DatabaseDriver.HasSqlCommand(sqlCommand.ID) Then
                    anExistingSqlCommand = Me.Connection.DatabaseDriver.RetrieveSqlCommand(sqlCommand.ID)
                    If anExistingSqlCommand.BuildVersion > sqlCommand.BuildVersion Then
                        Call CoreMessageHandler(messagetype:=otCoreMessageType.InternalWarning, subname:="clsOTBTableStore.StoreSQLCommand", arg1:=sqlCommand.ID, _
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
            Public Overridable Function RetrieveSqlCommand(id As String) As iormSqlCommand Implements iormDataStore.RetrieveSqlCommand
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
            Public Overridable Function CreateSqlCommand(id As String) As iormSqlCommand Implements iormDataStore.CreateSqlCommand
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
            Public Overridable Function CreateSqlSelectCommand(id As String, Optional addMe As Boolean = True, Optional addAllFields As Boolean = True) As iormSqlCommand Implements iormDataStore.CreateSqlSelectCommand
                '* get the ID
                id = Me.GetSqlCommandID(id)
                If Me.Connection.DatabaseDriver.HasSqlCommand(id) Then
                    Return Me.Connection.DatabaseDriver.RetrieveSqlCommand(id)
                Else
                    Dim aSqlCommand As iormSqlCommand = New ormSqlSelectCommand(id)
                    Me.Connection.DatabaseDriver.StoreSqlCommand(aSqlCommand)
                    If addMe Then
                        DirectCast(aSqlCommand, ormSqlSelectCommand).AddTable(tableid:=Me.TableID, addAllFields:=addAllFields)
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
                If Not id.ToLower.Contains((LCase(Me.TableID & "."))) Then
                    Return Me.TableID & "." & id
                Else
                    Return id
                End If
            End Function
        End Class



        '*******************************************************************************************
        '***** CLASS ormTableSchema describes the per Table the schema from the database itself
        '*****

        Public MustInherit Class ormTableSchema
            Implements iotDataSchema

            Protected _TableID As String

            Protected _fieldsDictionary As Dictionary(Of String, Long)    ' crossreference to the Arrays
            Protected _indexDictionary As Dictionary(Of String, ArrayList)    ' crossreference of the Index

            Protected _Fieldnames() As String    ' Fieldnames in OTDB
            Protected _Primarykeys() As UShort    ' indices for primary keys
            Protected _NoPrimaryKeys As UShort
            Protected _PrimaryKeyIndexName As String

            Protected _IsInitialized As Boolean = False
            Protected _DomainIDPrimaryKeyOrdinal As Short = -1 ' cache the Primary Key Ordinal of domainID for domainbehavior
            Protected _lockObject As New Object ' Lock Object

            ''' <summary>
            ''' constructor
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub New()

                _NoPrimaryKeys = 0
                ReDim Preserve _Fieldnames(0)
                ReDim Preserve _Primarykeys(0 To 0)

                _fieldsDictionary = New Dictionary(Of String, Long)
                _indexDictionary = New Dictionary(Of String, ArrayList)

            End Sub
            ''' <summary>
            ''' Assigns the native DB parameter.
            ''' </summary>
            ''' <param name="p1">The p1.</param>
            ''' <returns></returns>
            Public MustOverride Function AssignNativeDBParameter(fieldname As String, _
                                                                 Optional parametername As String = "") As System.Data.IDbDataParameter Implements iotDataSchema.AssignNativeDBParameter


            ''' <summary>
            ''' Gets or sets the is initialized. Should be True if the tableschema has a tableid 
            ''' </summary>
            ''' <value>The is initialized.</value>
            Public ReadOnly Property IsInitialized() As Boolean Implements iotDataSchema.IsInitialized
                Get
                    Return Me._IsInitialized
                End Get

            End Property

            ''' <summary>
            ''' resets the TableSchema to hold nothing
            ''' </summary>
            ''' <remarks></remarks>
            Protected Overridable Sub Reset()
                Dim nullArray As Object = {}
                _Fieldnames = nullArray
                _fieldsDictionary.Clear()
                _indexDictionary.Clear()
                _PrimaryKeyIndexName = ""
                _Primarykeys = nullArray
                _NoPrimaryKeys = 0
                _TableID = ""
                _DomainIDPrimaryKeyOrdinal = -1
            End Sub

            MustOverride Property TableID As String Implements iotDataSchema.TableID
            Public MustOverride Function Refresh(Optional reloadForce As Boolean = False) As Boolean Implements iotDataSchema.Refresh
            ''' <summary>
            ''' Names of the Indices of the table
            ''' </summary>
            ''' <value>List(of String)</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Indices As List(Of String) Implements iotDataSchema.Indices
                Get
                    Return _indexDictionary.Keys.ToList
                End Get

            End Property
            ''' <summary>
            ''' returns the primary Key ordinal (1..n) for the domain ID or less zero if not in primary key
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetDomainIDPKOrdinal() As Integer Implements iotDataSchema.GetDomainIDPKOrdinal
                If _DomainIDPrimaryKeyOrdinal < 0 Then
                    Dim i As Integer = Me.GetFieldordinal(index:=Domain.ConstFNDomainID)
                    If i < 0 Then
                        Return -1
                    Else
                        If Not Me.HasPrimaryKeyFieldname(name:=Domain.ConstFNDomainID.ToUpper) Then
                            Return -1
                        Else
                            For i = 1 To Me.NoPrimaryKeyFields
                                If Me.GetPrimaryKeyFieldname(i).ToUpper = Domain.ConstFNDomainID.ToUpper Then
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
            Public MustOverride Function GetNullable(index As Object) As Boolean Implements iotDataSchema.GetNullable

            ''' <summary>
            ''' returns the default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function GetDefaultValue(ByVal index As Object) As Object Implements iotDataSchema.GetDefaultValue

            ''' <summary>
            ''' returns if there is a default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasDefaultValue(ByVal index As Object) As Boolean Implements iotDataSchema.HasDefaultValue


            '**** getIndex returns the ArrayList of Fieldnames for the Index or Nothing
            ''' <summary>
            '''  returns the ArrayList of Fieldnames for the Index or empty array list if not found
            ''' </summary>
            ''' <param name="indexname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetIndex(indexname As String) As ArrayList Implements iotDataSchema.GetIndex


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
            Public Function HasIndex(indexname As String) As Boolean Implements iotDataSchema.HasIndex
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
            ReadOnly Property PrimaryKeyIndexName As String Implements iotDataSchema.PrimaryKeyIndexName
                Get
                    PrimaryKeyIndexName = _PrimaryKeyIndexName
                End Get
            End Property
            '******* return the no. fields
            '*******
            ''' <summary>
            ''' gets the number of fields
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property NoFields() As Integer Implements iotDataSchema.NoFields
                Get
                    Return UBound(_Fieldnames) + 1 'zero bound
                End Get
            End Property
            ''' <summary>
            ''' List of Fieldnames
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Fieldnames() As List(Of String) Implements iotDataSchema.Fieldnames
                Get
                    Return _Fieldnames.ToList
                End Get
            End Property
            ''' <summary>
            ''' List of primary key field names
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Primarykeys() As List(Of String) Implements iotDataSchema.PrimaryKeys
                Get
                    Dim aList As New List(Of String)
                    For i = 1 To Me.NoPrimaryKeyFields
                        aList.Add(Me.GetPrimaryKeyFieldname(i))
                    Next
                    Return aList
                End Get
            End Property
            '***** gets the FieldIndex of index as numeric (than must be in range) or name
            ''' <summary>
            ''' Get the Fieldordinal (position in record) by Index - can be numeric or the columnname
            ''' </summary>
            ''' <param name="anIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetFieldordinal(index As Object) As Integer Implements iotDataSchema.GetFieldordinal
                Dim i As ULong

                Try
                    If IsNumeric(index) Then
                        If CLng(index) > 0 And CLng(index) <= (_Fieldnames.GetUpperBound(0) + 1) Then
                            Return CLng(index)
                        Else
                            Call CoreMessageHandler(message:="index of column out of range", _
                                             arg1:=index, subname:="ormTableSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                            Return i
                        End If
                    ElseIf _fieldsDictionary.ContainsKey(index) Then
                        Return _fieldsDictionary.Item(index)
                    ElseIf _fieldsDictionary.ContainsKey(index.toupper) Then
                        Return _fieldsDictionary.Item(index.toupper)

                    Else
                        Call CoreMessageHandler(message:="index of column out of range", _
                                              arg1:=index, subname:="ormTableSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                        Return -1
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(arg1:=index, subname:="ormTableSchema.getFieldIndex", exception:=ex)
                    Return -1
                End Try

            End Function


            ''' <summary>
            ''' get the fieldname by index i - nothing if not in range
            ''' </summary>
            ''' <param name="i"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetFieldname(ByVal i As Integer) As String Implements iotDataSchema.Getfieldname

                If i > 0 And i <= UBound(_Fieldnames) + 1 Then
                    Return _Fieldnames(i - 1)
                Else
                    Call CoreMessageHandler(message:="index of column out of range", arg1:=i, tablename:=Me.TableID, _
                                          messagetype:=otCoreMessageType.InternalError, subname:="ormTableSchema.getFieldName")
                    Return Nothing
                End If
            End Function

            '*** check if fieldname by Name exists
            ''' <summary>
            ''' check if fieldname exists
            ''' </summary>
            ''' <param name="name"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function HasFieldname(ByVal name As String) As Boolean Implements iotDataSchema.Hasfieldname

                For i = LBound(_Fieldnames) To UBound(_Fieldnames)
                    If _Fieldnames(i).ToUpper = name.ToUpper Then
                        Return True
                    End If
                Next i

                HasFieldname = False
            End Function

            ''' <summary>
            ''' gets the fieldname of the primary key field by number (1..)
            ''' </summary>
            ''' <param name="i">1..n</param>
            ''' <returnsString></returns>
            ''' <remarks></remarks>
            Public Function GetPrimaryKeyFieldname(i As UShort) As String Implements iotDataSchema.GetPrimaryKeyfieldname
                Dim aCollection As ArrayList

                If i < 1 Then
                    Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyFieldName", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          arg1:=i)
                    Return ""
                End If

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        If i > aCollection.Count Then
                            Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  arg1:=i)
                            Return ""

                        End If

                        '*** return the item (Name)
                        Return aCollection.Item(i - 1)
                    Else
                        Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=i)
                        Return ""
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="ormTableSchema.getPrimaryKeyFieldName", _
                                          tablename:=_TableID, exception:=ex)
                    Return ""
                End Try

            End Function
            ''' <summary>
            ''' gets the fieldname of the primary key field by number
            ''' </summary>
            ''' <param name="i">1..n</param>
            ''' <returnsString></returns>
            ''' <remarks></remarks>
            Public Function HasPrimaryKeyFieldname(ByRef name As String) As Boolean Implements iotDataSchema.HasprimaryKeyfieldname
                Dim aCollection As ArrayList


                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)

                        '*** return the item (Name)
                        Return aCollection.Contains(name.ToUpper)
                    Else
                        Call CoreMessageHandler(subname:="ormTableSchema.hasPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        Return Nothing
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="ormTableSchema.hasPrimaryKeyName", _
                                          tablename:=_TableID, exception:=ex)
                    Return Nothing
                End Try

            End Function

            ''' <summary>
            ''' gets the field ordinal of the primary Key field by number i. (e.g.returns the ordinal of the primarykey field #2)
            ''' </summary>
            ''' <param name="i">number of primary key field 1..n </param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetordinalOfPrimaryKeyField(i As UShort) As Integer Implements iotDataSchema.GetordinalOfPrimaryKeyField
                Dim aCollection As ArrayList
                Dim aFieldName As String


                If i < 1 Then
                    Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyFieldIndex", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          arg1:=i)
                    GetordinalOfPrimaryKeyField = -1
                    Exit Function
                End If

                Try


                    If _indexDictionary.ContainsKey((_PrimaryKeyIndexName)) Then
                        aCollection = _indexDictionary.Item((_PrimaryKeyIndexName))

                        If i > aCollection.Count Then
                            Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  arg1:=i)
                            GetordinalOfPrimaryKeyField = -1
                            Exit Function
                        End If

                        aFieldName = aCollection.Item(i - 1)
                        GetordinalOfPrimaryKeyField = _fieldsDictionary.Item((aFieldName))
                        Exit Function
                    Else
                        Call CoreMessageHandler(subname:="ormTableSchema.getPrimaryKeyFieldIndex", _
                                              message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=i)
                        System.Diagnostics.Debug.WriteLine("ormTableSchema: primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        GetordinalOfPrimaryKeyField = -1
                        Exit Function
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="ormTableSchema.getPrimaryKeyFieldIndex", tablename:=_TableID, exception:=ex)
                    Return -1
                End Try
            End Function

            ''' <summary>
            ''' get the number of primary key fields
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function NoPrimaryKeyFields() As Integer Implements iotDataSchema.NoPrimaryKeyFields
                Dim aCollection As ArrayList

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        Return aCollection.Count

                    Else
                        Call CoreMessageHandler(subname:="ormTableSchema.noPrimaryKeysFields", message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=_PrimaryKeyIndexName, tablename:=_TableID)
                        Return -1

                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="ormTableSchema.noPrimaryKeys", tablename:=TableID, exception:=ex)
                    Return -1
                End Try


            End Function


        End Class
    End Namespace
End Namespace
