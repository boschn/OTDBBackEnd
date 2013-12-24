REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Driver Wrapper for ADO.NET Base Classes for On Track Database Backend Library
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
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Data
Imports OTDB
Imports System.Text.RegularExpressions
Imports OnTrack
Imports OnTrack.UI

Namespace OnTrack.Database


    '************************************************************************************
    '***** CLASS clsADONETDBDriver describes the  Database Driver  to OnTrack
    '*****       based on ADO.NET OLEDB
    '*****

    Public MustInherit Class clsADONETDBDriver
        Inherits ormDBDriver
        Implements iormDBDriver

        'Protected Friend Shadows WithEvents _primaryConnection As iOTDBConnection '-> in clsOTDBDriver
        Protected _OnTrackDataSet As New DataSet

        Protected _ParametersTableAdapter As System.Data.IDbDataAdapter
        Protected _ParametersTable As New DataTable
        Protected _parametersTableName As String = ConstParameterTableName

        Protected _IsInitialized As Boolean = False
        Protected _ErrorLogPersistCommand As IDbCommand = Nothing
        Protected _ErrorLogPersistTableschema As iotTableSchema = Nothing

        Protected _lock As New Object 'lockObject for driver instance
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="ID">an ID for this driver</param>
        ''' <remarks></remarks>
        Public Sub New(ID As String, ByRef session As Session)
            Call MyBase.New(ID, session)
        End Sub

        ''' <summary>
        ''' Gets the on track data set.
        ''' </summary>
        ''' <value>The on track data set.</value>
        Public ReadOnly Property OnTrackDataSet() As DataSet
            Get
                Return Me._OnTrackDataSet
            End Get
        End Property

        ''' <summary>
        ''' Gets the is initialized.
        ''' </summary>
        ''' <value>The is initialized.</value>
        Public Property IsInitialized() As Boolean
            Get
                Return Me._IsInitialized
            End Get
            Protected Friend Set(value As Boolean)
                _IsInitialized = value
            End Set
        End Property


        Public Overloads Property NativeConnection As IDbConnection

        '***
        '*** Initialize Driver
        Protected Friend Overridable Function Initialize(Optional Force As Boolean = False) As Boolean

            If Me.IsInitialized And Not Force Then
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' reset the Driver
        ''' </summary>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Protected Friend Overridable Function Reset() As Boolean
            Try

                _OnTrackDataSet = Nothing
                _ParametersTable = Nothing
                _ParametersTableAdapter = Nothing

                Me.IsInitialized = False
                Return True
            Catch ex As Exception
                Me.IsInitialized = False
                Call CoreMessageHandler(subname:="clsADONETDBDriver.reset", message:="couldnot de-Initialize Driver", _
                                      exception:=ex)
                Me.IsInitialized = False
                Return True
            End Try
        End Function
        '******
        '****** EventHandler for Connection
        Protected Friend Overridable Sub Connection_onConnection(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnConnection
            Call Me.Initialize()
        End Sub

        '******
        '****** EventHandler for DisConnection
        Protected Friend Overridable Sub Connection_onDisConnection(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnDisconnection
            Call Me.Reset()
        End Sub

        ''' <summary>
        ''' returns True if data store has the tablename
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasTable(tablename As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Gets the table.
        ''' </summary>
        ''' <param name="tablename">The tablename.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <returns></returns>
        Public Overrides Function GetTable(tablename As String, _
                                           Optional createOnMissing As Boolean = True, _
                                           Optional addToSchemaDir As Boolean = True, _
                                           Optional ByRef nativeConnection As Object = Nothing, _
                                            Optional ByRef nativeTableObject As Object = Nothing) As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Gets the index.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="indexname">The indexname.</param>
        ''' <param name="ColumnNames">The column names.</param>
        ''' <param name="PrimaryKey">The primary key.</param>
        ''' <param name="forceCreation">The force creation.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>
        Public Overrides Function GetIndex(ByRef nativeTABLE As Object, ByRef indexname As String, ByRef ColumnNames As List(Of String), Optional PrimaryKey As Boolean = False, Optional forceCreation As Boolean = False, Optional createOnMissing As Boolean = True, Optional addToSchemaDir As Boolean = True) As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' returns True if tablename has the column
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasColumn(tablename As String, columnname As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function
        ''' <summary>
        ''' Gets the column.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="aDBDesc">A DB desc.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>
        Public Overrides Function GetColumn(nativeTABLE As Object, aDBDesc As ormFieldDescription, Optional createOnMissing As Boolean = True, Optional addToSchemaDir As Boolean = True) As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function
        ''' <summary>
        ''' returns the target type for a OTDB FieldType - MAPPING
        ''' </summary>
        ''' <param name="type"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDBDriver.GetTargetTypeFor
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Gets the DB parameter.
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.CreateDBParameterTable
            Dim anativeConnection As IDbConnection

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    anativeConnection = DirectCast(Me.CurrentConnection, clsADONETConnection).NativeInternalConnection
                    If anativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADONETDBDriver.CreateDBParameterTable", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADONETDBDriver.CreateDBParameterTable", message:="Connection not available")
                    Return Nothing
                End If
            Else
                anativeConnection = nativeConnection
            End If

            '*** create
            If Not Me.HasTable(ConstParameterTableName) Then
                Me.RunSqlStatement("create table " & ConstParameterTableName & _
                                  "( ID nvarchar(50) not null, [Value] nvarchar(255) null, changedOn DATETIME  null,	Description nvarchar(255) null ," & _
                                  "CONSTRAINT [tblParametersGlobal_primaryKey] PRIMARY KEY NONCLUSTERED ([ID] Asc ) " & _
                                  "WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] " & _
                                  ") ON [PRIMARY] ;", _
                                  nativeConnection:=nativeConnection)
                'Me.RunSQLCommand("create unique index primaryKey on " & ConstParameterTableName & "(ID);", nativeConnection:=nativeConnection)
            End If
        End Function


        ''' <summary>
        ''' Gets the DB parameter.
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean _
            Implements iormDBDriver.CreateDBUserDefTable
            Dim anativeConnection As IDbConnection

            Try
                '*** get the native Connection 
                If nativeConnection Is Nothing Then
                    If Not Me.CurrentConnection Is Nothing Then
                        anativeConnection = DirectCast(Me.CurrentConnection, clsADONETConnection).NativeInternalConnection
                        If anativeConnection Is Nothing Then
                            Call CoreMessageHandler(subname:="clsADONETDBDriver.CreateDBUserDefTable", message:="Native internal Connection not available")
                            Return Nothing
                        End If
                    Else
                        Call CoreMessageHandler(subname:="clsADONETDBDriver.CreateDBUserDefTable", message:="Connection not available")
                        Return Nothing
                    End If
                Else
                    anativeConnection = nativeConnection
                End If

                '*** create
                If Not Me.HasTable(User.ConstTableID) Then
                    Me.RunSqlStatement(User.GetCreateSqlString, nativeConnection:=nativeConnection)
                    'Me.RunSQLCommand("create unique index primaryKey on " & anUser.TableID & "(username);", nativeConnection:=nativeConnection)

                    Dim anInsertStr As String = User.GetInsertInitalUserSQLString(username:="Admin", password:="axs2ontrack", desc:="Administrator", _
                                                                                  group:="Admins", defaultworkspace:="", person:="")
                    Me.RunSqlStatement(anInsertStr, nativeConnection:=nativeConnection)
                    '* old
                    'Me.RunSqlStatement("insert into " & User.ConstTableID & " ( [username], person, [password], [desc], [group], defws, isanon, alterschema, readdata, updatedata, noright, UpdatedOn, CreatedOn) " & _
                    '                        "values ('Admin','', 'Admin', 'Administrator', 'admins', '', 0, 1,1,1,0, '" & Date.Now.ToString("yyyyMMdd hh:mm:ss") & "','" & Date.Now.ToString("yyyyMMdd hh:mm:ss") & "' )", _
                    '                        nativeConnection:=anativeConnection)
                    With New UI.clsCoreUIMessageBox
                        .type = UI.clsCoreUIMessageBox.MessageType.Info
                        .Message = "An administrator user 'Admin' with password 'axs2ontrack' was created. Please change the password as soon as possible"
                        .buttons = UI.clsCoreUIMessageBox.ButtonType.OK
                        .Show()
                    End With
                    Call CoreMessageHandler(message:="An administrator user 'Admin' with password 'axs2ontrack' was created. Please change the password as soon as possible", _
                                            subname:="clsADONETDBDriver.CreateDBUserDefTable", messagetype:=otCoreMessageType.InternalInfo)
                    Return True
                Else
                    Call CoreMessageHandler(message:="Table for Users exists - no entries generated", tablename:=User.ConstTableID, _
                                            subname:="clsADONETDBDriver.CreateDBUserDefTable", messagetype:=otCoreMessageType.InternalInfo)
                    Return True
                End If

            Catch ex As SqlException
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.CreateDBUserDefTable", messagetype:=otCoreMessageType.InternalException)
                Return False
            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.CreateDBUserDefTable", messagetype:=otCoreMessageType.InternalException)
                Return False
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.CreateDBUserDefTable", messagetype:=otCoreMessageType.InternalException)
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Checks if the most important objects are here
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function VerifyOnTrackDatabase(verifyOnly As Boolean, createOnMissing As Boolean) As Boolean Implements iormDBDriver.VerifyOnTrackDatabase
            Dim anUser As New User
            Dim aNativeConnection = DirectCast(Me.CurrentConnection, clsADONETConnection).NativeInternalConnection

            If Not Me.HasTable(tablename:=_parametersTableName, nativeConnection:=aNativeConnection) OrElse _
                Not Me.HasTable(tablename:=anUser.TableID, nativeConnection:=aNativeConnection) Then

                If Not createOnMissing Then

                    Call CoreMessageHandler(showmsgbox:=True, message:="Table for Parameters not defined in Database.", arg1:=Me.CurrentConnection.Connectionstring, _
                                          tablename:=anUser.TableID, noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalError, subname:="clsADONETDBDriver.VerifyOnTrackDatabase")
                    Return False
                Else

                    Call CoreMessageHandler(message:="Table for Parameters not defined in Database.", arg1:=Me.CurrentConnection.Connectionstring, _
                                          tablename:=anUser.TableID, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationWarning, subname:="clsADONETDBDriver.VerifyOnTrackDatabase")
                    Dim result As clsCoreUIMessageBox.ResultType
                    With New clsCoreUIMessageBox
                        .Title = "IMPORTANT QUESTION"
                        .Message = "The Parameters Table is not existing in the database. Either the database is not set up properly or the connection has a problem." & vbLf & _
                            "Should the database schema be created ? This means that all data might be lost ..."
                        .buttons = clsCoreUIMessageBox.ButtonType.YesNo
                        .Show()
                        result = .result
                    End With
                    '*** create
                    '***
                    If result = clsCoreUIMessageBox.ResultType.Yes Then
                        '***
                        '*** create the database
                        Call createDatabase.Run() ' startups also a session and login

                    End If


                End If

            End If


            Return True
        End Function

        ''' <summary>
        ''' Gets the def user.
        ''' </summary>
        ''' <param name="Username">The username.</param>
        ''' <param name="SelectAnonymous"></param>
        ''' <param name="nativeConnection">The native connection.</param>
        ''' <returns></returns>
        Protected Friend Overrides Function GetUserValidation(username As String, Optional selectAnonymous As Boolean = False, _
                                                    Optional ByRef nativeConnection As Object = Nothing) As UserValidation
            Dim anUser As New User
            Dim aCollection As New Collection
            Dim anUserValidation As New UserValidation
            Dim aNativeConnection As IDbConnection
            Dim cmdstr As String


            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If _primaryConnection IsNot Nothing Then
                    aNativeConnection = DirectCast(_primaryConnection, clsADONETConnection).NativeInternalConnection
                    If aNativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADONETDBDriver.getUserValidation", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADONETDBDriver.getUserValidation", message:="Connection not available")
                    Return Nothing
                End If
            Else
                aNativeConnection = nativeConnection
            End If


            Try
                '** init driver
                If Not Me.IsInitialized Then
                    Me.Initialize()
                End If

                If Not Me.HasTable(tablename:=User.ConstTableID) Then
                    If Not Me.VerifyOnTrackDatabase(verifyOnly:=True, createOnMissing:=True) Then
                        Call CoreMessageHandler(subname:="clsADONETDBDriver.getUserValidation", message:="Database is not installed - Setup of schema failed")
                        Return Nothing
                    End If
                End If

                If Not selectAnonymous Then
                    cmdstr = "select * from " & User.ConstTableID & " where " & User.ConstFNUsername & " ='" & username & "'"
                Else
                    If Me.Type = otDbDriverType.ADONETSQL Then
                        cmdstr = "select * from " & User.ConstTableID & " where  " & User.ConstFNIsAnonymous & " <>0 order by " & User.ConstFNUsername & " desc"
                    ElseIf Me.Type = otDbDriverType.ADONETOLEDB Then
                        cmdstr = "select * from " & User.ConstTableID & " where  " & User.ConstFNIsAnonymous & " <> false order by " & User.ConstFNUsername & " desc"
                    Else
                        Call CoreMessageHandler(message:="DriverType is not implemented", subname:="clsADONETDBDriver.GetUserValidation", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If

                End If
                '** open recordset

                Dim aCommand As IDbCommand = Me.CreateNativeDBCommand(cmdstr, aNativeConnection)
                Dim aDataReader As IDataReader = aCommand.ExecuteReader

                If aDataReader.Read Then
                    Try
                        anUserValidation.Password = aDataReader("password")
                        anUserValidation.Username = aDataReader("username")
                        anUserValidation.IsAnonymous = aDataReader("isanon")
                        anUserValidation.HasAlterSchemaRights = aDataReader("alterschema")
                        anUserValidation.HasReadRights = aDataReader("readdata")
                        anUserValidation.HasUpdateRights = aDataReader("updatedata")
                        anUserValidation.HasNoRights = aDataReader("noright")
                        anUserValidation.validEntry = True

                    Catch ex As Exception
                        Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.getUserValidation", message:="Couldn't read User Validation", _
                                              break:=False, noOtdbAvailable:=True)
                        anUserValidation.validEntry = False
                        aDataReader.Close()
                        Return anUserValidation

                    End Try

                    ' return successfull
                    aDataReader.Close()
                    Return anUserValidation

                End If

                aDataReader.Close()
                ' return
                anUserValidation.validEntry = False
                Return anUserValidation

            Catch ex As OleDbException
                Call CoreMessageHandler(showmsgbox:=True, message:="OLEDB Database not available", subname:="clsADONETDBDriver.getUserValidation", exception:=ex)

                Return Nothing

            Catch ex As SqlException
                Call CoreMessageHandler(showmsgbox:=True, message:="SQL Server Database not available", subname:="clsADONETDBDriver.getUserValidation", exception:=ex)

                Return Nothing


                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsADONETDBDriver.getUserValidation", exception:=ex)

                Return Nothing

            End Try

        End Function

        ''' <summary>
        ''' run Sql Select Command by ID
        ''' </summary>
        ''' <param name="id">the ID of the stored SQLCommand</param>
        ''' <param name="parameters">optional a list of parameters for the values</param>
        ''' <param name="nativeConnection">optional a nativeConnection</param>
        ''' <returns>a list of clsotdbRecords</returns>
        ''' <remarks></remarks>
        Public Overrides Function RunSqlSelectCommand(id As String, _
                                                       Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                      Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                                    Implements iormDBDriver.RunSqlSelectCommand
            Try
                Dim aSqlCommand As iormSqlCommand


                '*** bookkeeping on commands
                If Me.HasSqlCommand(id) Then
                    aSqlCommand = Me.RetrieveSqlCommand(id)
                    Return Me.RunSqlSelectCommand(sqlcommand:=aSqlCommand, parametervalues:=parametervalues, nativeConnection:=nativeConnection)
                Else
                    Call CoreMessageHandler(message:="SQL command with this ID is not in store", subname:="clsADONETDBDriver.RunSqlSelectCommand", _
                                          messagetype:=otCoreMessageType.InternalError, arg1:=id)
                    Return New List(Of ormRecord)
                End If
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, message:="Exception", subname:="clsADONETDBDriver.RunSqlSelectCommand", _
                                          messagetype:=otCoreMessageType.InternalError, arg1:=id)
                Return New List(Of ormRecord)
            End Try
        End Function
        ''' <summary>
        ''' runs a Sql Select Command and returns a List of Records
        ''' </summary>
        ''' <param name="sqlcommand">a clsOTDBSqlSelectCommand</param>
        ''' <param name="parameters"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function RunSqlSelectCommand(ByRef sqlcommand As ormSqlSelectCommand, _
                                           Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                           Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                       Implements iormDBDriver.RunSqlSelectCommand


            Dim cvtvalue As Object
            '*** Execute and get Results
            Dim aDataReader As IDataReader
            Dim theResults As New List(Of ormRecord)
            Dim atableid As String = ""

            Try
                If Not sqlcommand.Prepared Then
                    If Not sqlcommand.Prepare Then
                        Call CoreMessageHandler(message:="SqlCommand couldn't be prepared", arg1:=sqlcommand.ID, _
                                               subname:="clsADONETDBDriver.runsqlselectCommand", messagetype:=otCoreMessageType.InternalError)
                        Return New List(Of ormRecord)
                    End If
                End If

                Dim aNativeCommand As IDbCommand
                aNativeCommand = sqlcommand.NativeCommand

                '***  Assign the values
                '** initial values
                For Each aParameter In sqlcommand.Parameters
                    If Not aParameter.NotColumn AndAlso (aParameter.Fieldname <> "" And aParameter.Tablename <> "") Then
                        Dim aTablestore As iormDataStore = Me.GetTableStore(aParameter.Tablename)
                        cvtvalue = aTablestore.Convert2ColumnData(aParameter.Fieldname, aParameter.Value)
                    Else
                        cvtvalue = Convert2DBData(aParameter.Value, GetTargetTypeFor(aParameter.Datatype))
                    End If
                    aNativeCommand.Parameters(aParameter.ID).value = cvtvalue
                Next
                '** Input Parameters 
                If Not parametervalues Is Nothing Then
                    ' overwrite the initial values
                    For Each kvp As KeyValuePair(Of String, Object) In parametervalues
                        If aNativeCommand.Parameters.Contains(kvp.Key) Then
                            Dim aParameter = sqlcommand.Parameters.Find(Function(x) x.ID = kvp.Key)

                            If Not aParameter.NotColumn And aParameter.Fieldname <> "" And aParameter.Tablename <> "" Then
                                Dim aTablestore As iormDataStore = Me.GetTableStore(aParameter.Tablename)
                                cvtvalue = aTablestore.Convert2ColumnData(aParameter.Fieldname, kvp.Value)
                            Else
                                cvtvalue = Convert2DBData(kvp.Value, GetTargetTypeFor(aParameter.Datatype))
                            End If
                            aNativeCommand.Parameters(aParameter.ID).value = cvtvalue

                        End If
                    Next
                End If

                '*** check if we have only on table -> to infuse this is necessary
                If sqlcommand.TableIDs.Count = 1 Then
                    atableid = sqlcommand.TableIDs(0)
                End If

                aDataReader = aNativeCommand.ExecuteReader

                While aDataReader.Read
                    Dim aRecord As New ormRecord() 'free flow record
                    For i = 0 To aDataReader.FieldCount - 1
                        '** might be that we have no other datatypes than a infuse can cope with
                        aRecord.SetValue(aDataReader.GetName(i), aDataReader.GetValue(i))
                    Next
                    theResults.Add(aRecord)
                End While

                aDataReader.Close()
                Return theResults

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.runSqlSelectCommand", arg1:=sqlcommand.SqlText, messagetype:=otCoreMessageType.InternalException)
                If Not aDataReader Is Nothing Then aDataReader.Close()
                Return New List(Of ormRecord)
            Catch ex As SqlException
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.runSqlSelectCommand", arg1:=sqlcommand.SqlText, messagetype:=otCoreMessageType.InternalException)
                If Not aDataReader Is Nothing Then aDataReader.Close()
                Return New List(Of ormRecord)
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETDBDriver.runSqlSelectCommand", arg1:=sqlcommand.SqlText, messagetype:=otCoreMessageType.InternalException)
                If Not aDataReader Is Nothing Then aDataReader.Close()
                Return New List(Of ormRecord)
            End Try


        End Function

        ''' <summary>
        ''' persists the errorlog
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function PersistLog(ByRef log As ErrorLog) As Boolean Implements iormDBDriver.PersistLog


            '** we need a valid connection also nativeInternal could work also
            If _primaryConnection Is Nothing OrElse Not Me._primaryConnection.IsConnected Then
                Return False
            End If

            Try
                If DirectCast(_primaryConnection, clsADONETConnection).IsNativeInternalLocked Then
                    Return False
                End If

                'DirectCast(_primaryConnection, clsADONETConnection).IsNativeInternalLocked = True

                '** build the command
                If _ErrorLogPersistCommand Is Nothing Then
                    '* get the schema
                    _ErrorLogPersistTableschema = Me.GetTableSchema(CoreError.ConstTableID)
                    If _ErrorLogPersistTableschema Is Nothing OrElse Not _ErrorLogPersistTableschema.IsInitialized Then
                        Return False
                    End If

                    '** we need just the insert
                    _ErrorLogPersistCommand = DirectCast(_ErrorLogPersistTableschema, clsADONETTableSchema). _
                        BuildCommand(_ErrorLogPersistTableschema.PrimaryKeyIndexName, _
                                     clsADONETTableSchema.CommandType.InsertType, _
                                     nativeconnection:=DirectCast(_primaryConnection, clsADONETConnection).NativeInternalConnection)
                    '** take it on the internal 
                    If _ErrorLogPersistCommand Is Nothing Then
                        'DirectCast(_primaryConnection, clsADONETConnection).IsNativeInternalLocked = False
                        Return False
                    End If
                End If

                '** flush the messages
                SyncLock DirectCast(_primaryConnection, clsADONETConnection).NativeInternalConnection
                    If _ErrorLogPersistCommand.Connection.State = ConnectionState.Open Then
                        PersistLog = False
                        Dim anError As CoreError
                        Do
                            anError = log.Retain
                            If anError IsNot Nothing AndAlso Not anError.Processed Then
                                'get all fields -> update
                                For Each fieldname As String In _ErrorLogPersistTableschema.fieldnames
                                    ' assign values
                                    If fieldname <> "" Then
                                        With _ErrorLogPersistCommand.Parameters.Item("@" & fieldname)
                                            '** set the value of parameter
                                            Select Case fieldname
                                                Case CoreError.ConstFNTag
                                                    If anError.Tag = "" Then
                                                        .value = CurrentSession.Errorlog.Tag
                                                    Else
                                                        .Value = anError.Tag
                                                    End If
                                                Case CoreError.ConstFNno
                                                    .value = anError.Entryno
                                                Case CoreError.ConstFNmessage
                                                    .value = anError.Message
                                                Case CoreError.ConstFNtimestamp
                                                    .value = anError.Timestamp
                                                Case CoreError.ConstFNID
                                                    .value = ""
                                                Case CoreError.ConstFNsubname
                                                    .value = anError.Subname
                                                Case CoreError.ConstFNtype
                                                    .value = anError.messagetype
                                                Case CoreError.ConstFNtablename
                                                    .value = anError.Tablename
                                                Case CoreError.ConstFNStack
                                                    .value = anError.StackTrace
                                                Case CoreError.ConstFNfieldname
                                                    .value = anError.EntryName
                                                Case CoreError.ConstFNarg
                                                    .value = anError.Arguments
                                                Case CoreError.ConstFNUpdatedOn, CoreError.ConstFNCreatedOn
                                                    .value = Date.Now
                                                Case CoreError.ConstFNIsDeleted
                                                    .value = False
                                                Case CoreError.ConstFNDeletedOn
                                                    .value = ConstNullDate
                                                Case CoreError.ConstFNUsername
                                                    .value = anError.Username
                                                Case Else
                                                    .value = DBNull.Value
                                            End Select

                                            If .value Is Nothing Then
                                                .value = DBNull.Value
                                            End If
                                        End With
                                    End If
                                Next

                                If _ErrorLogPersistCommand.ExecuteNonQuery() > 0 Then
                                    anError.Processed = True
                                    PersistLog = PersistLog And True
                                End If

                            End If
                        Loop Until anError Is Nothing

                        'DirectCast(_primaryConnection, clsADONETConnection).IsNativeInternalLocked = False
                        Return PersistLog
                    End If
                End SyncLock
            Catch ex As Exception
                Console.WriteLine(Date.Now.ToLocalTime & ": could not flush error log to database")
                'DirectCast(_primaryConnection, clsADONETConnection).IsNativeInternalLocked = False
                Return False
            End Try

        End Function

    End Class



    '**************
    '************** ConnectionEventArgs for the ConnectionEvents

    Public Class InternalConnectionEventArgs
        Inherits EventArgs

        Private _Connection As iormConnection
        Private _NativeConnection As IDbConnection

        Public Sub New(newConnection As iormConnection, nativeConnection As IDbConnection)
            _Connection = newConnection
        End Sub
        ''' <summary>
        ''' Gets the native connection.
        ''' </summary>
        ''' <value>The native connection.</value>
        Public ReadOnly Property NativeConnection() As IDbConnection
            Get
                Return Me._NativeConnection
            End Get
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
    '***** CLASS clsADONETConnection describes the Connection description to OnTrack
    '*****        based on ADO.NET  Driver
    '*****

    Public MustInherit Class clsADONETConnection
        Inherits ormConnection
        Implements iormConnection

        Protected Friend _IsConnected As Boolean = False

        Protected Friend _nativeConnection As IDbConnection
        Protected Friend _nativeinternalConnection As IDbConnection
        Private _IsNativeInternalLocked As Boolean = False

        ' Private _ADOXcatalog As ADOX.Catalog
        'Private _ADOError As ADODB.Error
        Protected Friend Shadows _useseek As Boolean = False 'use seek instead of SQL

        Protected Friend Shadows WithEvents _ErrorLog As New ErrorLog(My.Computer.Name & "-" & My.User.Name & "-" & Date.Now.ToUniversalTime)

        Public Shadows Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Shadows Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection
        Public Event OnInternalConnected As EventHandler(Of InternalConnectionEventArgs)

        Public Sub New(ByVal id As String, ByRef DatabaseDriver As iormDBDriver, ByRef session As Session, sequence As ot.ConfigSequence)
            MyBase.New(id, DatabaseDriver, session, sequence)
            _useseek = False
            _nativeConnection = Nothing
            _nativeinternalConnection = Nothing
        End Sub
        '*****
        '***** finalize 
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            '*** close
            Try
                If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    _nativeConnection.Close()
                End If
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETConnection.finalize", messagetype:=otCoreMessageType.InternalException _
                                       )

            End Try

            '*** close
            Try
                If Not _nativeinternalConnection Is Nothing AndAlso _nativeinternalConnection.State <> ConnectionState.Closed Then
                    _nativeinternalConnection.Close()
                End If
            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETConnection.finalize", messagetype:=otCoreMessageType.InternalException _
                                       )
            End Try

        End Sub
        Public Shadows Function RaiseOnConnected()
            RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
        End Function
        Public Shadows Function RaiseOnDisConnected()
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
        End Function
        ''' Gets the is initialized.
        ''' </summary>
        ''' <value>The is initialized.</value>
        Overrides ReadOnly Property isInitialized() As Boolean
            Get
                If _nativeConnection Is Nothing Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is native internal locked.
        ''' </summary>
        ''' <value>The is native internal locked.</value>
        Public Property IsNativeInternalLocked() As Boolean
            Get
                Return Me._IsNativeInternalLocked
            End Get
            Set(value As Boolean)
                Me._IsNativeInternalLocked = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the native connection.
        ''' </summary>
        ''' <value>The native connection.</value>
        Friend Overrides ReadOnly Property NativeConnection() As Object
            Get
                If _nativeConnection Is Nothing OrElse _nativeConnection.State <> ConnectionState.Open Then
                    Return Nothing
                Else
                    Return Me._nativeConnection
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets the is connected.
        ''' </summary>
        ''' <value>The is connected.</value>
        Overrides ReadOnly Property IsConnected() As Boolean
            Get
                Return _IsConnected
            End Get

        End Property
        ''' <summary>
        ''' Disconnects this instance of the connection with raising events
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Disconnect() As Boolean

            If Not MyBase.Disconnect() Then
                Return False
            End If


            ' Raise the event
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))

            '***
            If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Open Then
                '** close
                _nativeConnection.Close()
            End If

            '*** reset
            Call ResetFromConnection()
            '***
            Call CoreMessageHandler(showmsgbox:=False, message:=" Connection disconnected ", _
                                  break:=True, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo, _
                                  subname:="clsOTDBSession.Disconnect")

            '** close also the internal connection
            If Not _nativeinternalConnection Is Nothing AndAlso _nativeinternalConnection.State <> ConnectionState.Closed Then
                _nativeinternalConnection.Close()
                _nativeinternalConnection = Nothing
            End If

            Return True
        End Function


        ''' <summary>
        ''' gets the native internal connection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overridable ReadOnly Property NativeInternalConnection As IDbConnection
            Get
                If _nativeinternalConnection Is Nothing OrElse _nativeinternalConnection.State <> ConnectionState.Open Then
                    Try
                        '**** retrieve ConfigParameters
                        If Not Me.SetConnectionConfigParameters() Then
                            Call CoreMessageHandler(showmsgbox:=True, message:="Configuration Parameters couldnot be retrieved from a data source", _
                                                  subname:="clsADONETConnection.Connect")
                            Return Nothing
                        End If
                        ' connect 
                        _nativeinternalConnection = createNewNativeConnection()
                        _nativeinternalConnection.ConnectionString = Me.Connectionstring
                        _nativeinternalConnection.Open()
                        ' check if state is open
                        If _nativeinternalConnection.State = ConnectionState.Open Then
                            RaiseEvent OnInternalConnected(Me, New InternalConnectionEventArgs(newConnection:=Me, nativeConnection:=_nativeinternalConnection))
                            Return _nativeinternalConnection
                        Else
                            Call CoreMessageHandler(showmsgbox:=False, message:="internal connection couldnot be established", _
                                                  subname:="clsADONETConnection.NativeInternalConnection")
                            Return Nothing
                        End If
                    Catch ex As SqlException
                        Call CoreMessageHandler(showmsgbox:=True, message:="internal connection to database could not be established", _
                                              subname:="clsADONETConnection.NativeInternalConnection", exception:=ex)
                        Return Nothing
                    Catch ex As Exception
                        Call CoreMessageHandler(showmsgbox:=True, message:="internal connection couldnot be established", _
                                              subname:="clsADONETConnection.NativeInternalConnection", exception:=ex)
                        Return Nothing
                    End Try
                Else
                    Return Me._nativeinternalConnection
                End If
            End Get
        End Property


        '*****
        '***** reset : reset all the private members for a connection
        Protected Friend Overrides Sub ResetFromConnection()
            Call MyBase.ResetFromConnection()
            '** close the native Connection
            If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Open Then
                _nativeConnection.Close()
            End If
            'If Not _nativeinternalConnection Is Nothing AndAlso _nativeinternalConnection.State <> ObjectStateEnum.adStateClosed Then
            '_nativeinternalConnection.Close()
            'End If
            _IsConnected = False
            _nativeConnection = Nothing

            '_nativeinternalConnection = Nothing

        End Sub

        ''' <summary>
        ''' create a new native Connection (not connected)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function createNewNativeConnection() As IDbConnection


        ''' <summary>
        ''' Connects the specified FORCE.
        ''' </summary>
        ''' <param name="FORCE">The FORCE.</param>
        ''' <param name="AccessRequest">The access request.</param>
        ''' <param name="OTDBUsername">The OTDB username.</param>
        ''' <param name="OTDBPassword">The OTDB password.</param>
        ''' <param name="exclusive">The exclusive.</param>
        ''' <param name="notInitialize">The not initialize.</param>
        ''' <returns></returns>
        Public Overrides Function Connect(Optional FORCE As Boolean = False, _
        Optional accessRequest As otAccessRight = otAccessRight.[ReadOnly], _
        Optional domainID As String = "", _
        Optional OTDBUsername As String = "", _
        Optional OTDBPassword As String = "", _
        Optional exclusive As Boolean = False, _
        Optional notInitialize As Boolean = False, _
        Optional doLogin As Boolean = True) As Boolean

            ' return if connection is there
            If Not _nativeConnection Is Nothing And Not FORCE Then
                ' stay in the connection if we donot need another state -> Validate the Request
                ' if there is a connection and we have no need for higher access -> return
                If _nativeConnection.State = ConnectionState.Open And ValidateAccessRequest(accessrequest:=accessRequest) Then
                    ' initialize the parameter values of the OTDB
                    Call Initialize(force:=False)
                    Return True

                ElseIf _nativeConnection.State <> ConnectionState.Closed Then
                    _nativeConnection.Close()
                Else
                    'Set otdb_connection = Nothing
                    ' reset
                    System.Diagnostics.Debug.WriteLine("reseting")
                End If
            End If

            '*** check On Track
            If Not Me.DatabaseDriver.VerifyOnTrackDatabase(False, True) Then
                Call CoreMessageHandler(showmsgbox:=True, message:="OnTrack is not UpToDate - Contact your System Administrator", _
                                     subname:="clsADONETConnection.Connect", noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            '**** retrieve ConfigParameters
            If Not Me.SetConnectionConfigParameters() Then
                Call CoreMessageHandler(showmsgbox:=True, message:="Configuration Parameters couldnot be retrieved from a data source", _
                                      subname:="clsADONETConnection.Connect", noOtdbAvailable:=True, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            '*** verify the User
            If Not ot.ValidateUser(accessRequest:=accessRequest, username:=OTDBUsername, _
                                           password:=OTDBPassword, domainID:=domainID) Then
                Call CoreMessageHandler(showmsgbox:=True, message:="Connect not possible - user could not be validated", arg1:=OTDBUsername, _
                                    subname:="clsADONETConnection.Connect", noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)
                If Me.IsConnected Then
                    Me.Disconnect()
                End If
                Return False
            End If


            '*** we are connected =!
            If Me.IsConnected Then
                Me.Disconnect()
            End If
            '*** create the connection
            _nativeConnection = createNewNativeConnection()

            Try
                If Me.Connectionstring = "" Then
                    Call CoreMessageHandler(messagetype:=otCoreMessageType.InternalError, message:="Connection String to Database is empty", _
                                           subname:="clsADONETConnection.Connect", arg1:=Me.Connectionstring)
                    ResetFromConnection()
                    Return False
                End If
                ' set dbpassword
                _nativeConnection.ConnectionString = Me.Connectionstring


                If Me.Access = otAccessRight.[ReadOnly] Then
                    '_nativeConnection. = ConnectModeEnum.adModeRead
                Else
                    '_nativeConnection.Mode = ConnectModeEnum.adModeReadWrite
                End If


                ' open again
                _nativeConnection.Open()
                ' check if state is open
                If _nativeConnection.State = ConnectionState.Open Then
                    ' set the Access Request
                    _AccessLevel = accessRequest
                    _IsConnected = True ' even with no valid User Defintion we are Connection (otherwise we cannot load)
                    _OTDBDatabaseDriver.SetDBParameter("lastLogin_user", OTDBUsername)
                    _OTDBDatabaseDriver.SetDBParameter("lastLogin_timestamp", Date.Now.ToString)

                    _OTDBUser = New User
                    If Not _OTDBUser.LoadBy(username:=OTDBUsername) Then
                        Call CoreMessageHandler(message:="User Definition could not be loaded from the database", _
                                              subname:="clsADONETConnection.connect", arg1:=OTDBUsername, _
                                              messagetype:=otCoreMessageType.InternalError)
                        ResetFromConnection()
                        Return False

                    End If

                    ' raise Connected Event
                    RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me, domainID))
                    ' return true
                    Return True
                End If

            Catch ex As System.Data.DataException
                Call CoreMessageHandler(showmsgbox:=True, message:="internal connection to database could not be established" & vbLf, _
                                      subname:="clsADONETConnection.Connect", exception:=ex)
                If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    _nativeConnection.Close()
                End If
                '*** reset
                Call ResetFromConnection()
                Return False

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsADONETConnection.Connect", exception:=ex, _
                                      arg1:=_Connectionstring, noOtdbAvailable:=True, break:=False)
                If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    _nativeConnection.Close()
                End If
                '*** reset
                Call ResetFromConnection()
                Return False
            End Try

        End Function


    End Class



    '************************************************************************************
    '***** CLASS clsADONETTableSchema  CLASS describes the schema per table of the database itself
    '*****        based on ADO.NET OLEDB Driver
    '*****

    ''' <summary>
    ''' CLASS describes the schema per table of the database itself
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class clsADONETTableSchema
        Inherits ormTableSchema
        Implements iotTableSchema

        '** own ColumnDescription
        '**
        Class ColumnDescription
            Private _Description As String
            Private _ColumnName As String
            Private _IsNullable As Boolean
            Private _Ordinal As UShort
            Private _CharacterMaxLength As Nullable(Of Int64)
            Private _HasDefault As Boolean
            Private _Default As String
            Private _DataType As Long
            Private _Catalog As String
            Private _NumericPrecision As Nullable(Of Int64)
            Private _NumericScale As Nullable(Of Int64)
            Private _DateTimePrecision As Nullable(Of Int64)
            Private _CharachterOctetLength As Nullable(Of Int64)

            ''' <summary>
            ''' Initializes a new instance of the <see cref="ColumnDescription" /> class.
            ''' </summary>
            ''' <param name="characterMaxLength">Length of the character max.</param>
            Public Sub New()

            End Sub

#Region "Properties"


            ''' <summary>
            ''' Gets or sets the length of the charachter octet.
            ''' </summary>
            ''' <value>The length of the charachter octet.</value>
            Public Property CharachterOctetLength() As Nullable(Of Int64)
                Get
                    Return Me._CharachterOctetLength
                End Get
                Set(value As Nullable(Of Int64))
                    Me._CharachterOctetLength = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the date time precision.
            ''' </summary>
            ''' <value>The date time precision.</value>
            Public Property DateTimePrecision() As Nullable(Of Int64)
                Get
                    Return Me._DateTimePrecision
                End Get
                Set(value As Nullable(Of Int64))
                    Me._DateTimePrecision = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the numeric scale.
            ''' </summary>
            ''' <value>The numeric scale.</value>
            Public Property NumericScale() As Nullable(Of Int64)
                Get
                    Return Me._NumericScale
                End Get
                Set(value As Nullable(Of Int64))
                    Me._NumericScale = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the numeric precision.
            ''' </summary>
            ''' <value>The numeric precision.</value>
            Public Property NumericPrecision() As Nullable(Of Int64)
                Get
                    Return Me._NumericPrecision
                End Get
                Set(value As Nullable(Of Int64))
                    Me._NumericPrecision = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the catalog.
            ''' </summary>
            ''' <value>The catalog.</value>
            Public Property Catalog() As String
                Get
                    Return Me._Catalog
                End Get
                Set(value As String)
                    Me._Catalog = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the type of the data.
            ''' </summary>
            ''' <value>The type of the data.</value>
            Public Overridable Property DataType() As Long
                Get
                    Return Me._DataType
                End Get
                Set(value As Long)
                    Me._DataType = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the default.
            ''' </summary>
            ''' <value>The default.</value>
            Public Property [Default]() As String
                Get
                    Return Me._Default
                End Get
                Set(value As String)
                    Me._Default = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the has default.
            ''' </summary>
            ''' <value>The has default.</value>
            Public Property HasDefault() As Boolean
                Get
                    Return Me._HasDefault
                End Get
                Set(value As Boolean)
                    Me._HasDefault = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the length of the character max.
            ''' </summary>
            ''' <value>The length of the character max.</value>
            Public Property CharacterMaxLength() As Nullable(Of Int64)
                Get
                    Return Me._CharacterMaxLength
                End Get
                Set(value As Nullable(Of Int64))
                    Me._CharacterMaxLength = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the ordinal.
            ''' </summary>
            ''' <value>The ordinal.</value>
            Public Property Ordinal() As UShort
                Get
                    Return Me._Ordinal
                End Get
                Set(value As UShort)
                    Me._Ordinal = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the is nullable.
            ''' </summary>
            ''' <value>The is nullable.</value>
            Public Property IsNullable() As Boolean
                Get
                    Return Me._IsNullable
                End Get
                Set(value As Boolean)
                    Me._IsNullable = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the name of the column.
            ''' </summary>
            ''' <value>The name of the column.</value>
            Public Property ColumnName() As String
                Get
                    Return Me._ColumnName
                End Get
                Set(value As String)
                    Me._ColumnName = value
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
#End Region
        End Class

        '** own CommandKey

        Enum CommandType
            SelectType
            UpdateType
            DeleteType
            InsertType
        End Enum

        Structure CommandKey
            Public IndexName As String
            Public CommandType As CommandType
            Public Sub New(name As String, type As CommandType)
                IndexName = Name
                CommandType = Type
            End Sub
        End Structure

        '***** internal variables
        '*****
        Protected _Connection As iormConnection
        Protected _ColumnsTable As DataTable
        Protected _IndexTable As DataTable
        Protected _Columns() As ColumnDescription

        '**** CommandStore
        Protected _CommandStore As New Dictionary(Of CommandKey, IDbCommand)



        Public Sub New(ByRef connection As iormConnection, ByVal tableID As String)
            MyBase.New()
            'ReDim Preserve _ADOXColumns(0)
            _Connection = connection
            Me.TableID = tableID
        End Sub
        Protected Overrides Sub Finalize()
            _CommandStore = Nothing
            _Connection = Nothing
            _ColumnsTable = Nothing
            _IndexTable = Nothing
        End Sub
        ''' <summary>
        ''' resets the TableSchema
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overrides Sub reset()
            Call MyBase.reset()
            _CommandStore.Clear()
            _ColumnsTable.Clear()
            _IsInitialized = False
            _IndexTable.Clear()
            _Columns = Nothing

        End Sub
        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Overrides Property TableID() As String
            Get
                Return _TableID
            End Get
            Set(ByVal newTableID As String)
                _TableID = newTableID
            End Set
        End Property
        ''' <summary>
        ''' returns a Default Value for a fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GetDefaultValue(index As Object) As Object Implements iotTableSchema.GetDefaultValue
            Dim i As Integer = Me.GetFieldordinal(index:=index)
            Dim aDesc As ColumnDescription

            If i >= 0 Then
                aDesc = Me.GetColumnDescription(i)
                If aDesc IsNot Nothing Then
                    Dim aTablestore As iormDataStore = _Connection.DatabaseDriver.GetTableStore(Me.TableID)
                    Return aTablestore.Convert2ObjectData(i, aDesc.Default)
                End If
            End If

            Return Nothing

        End Function
        ''' <summary>
        ''' returns a Default Value for a fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasDefaultValue(index As Object) As Boolean Implements iotTableSchema.HasDefaultValue
            Dim i As Integer = Me.GetFieldordinal(index:=index)
            Dim aDesc As ColumnDescription

            If i >= 0 Then
                aDesc = Me.GetColumnDescription(i)
                If aDesc IsNot Nothing Then
                    Return aDesc.HasDefault
                End If
            End If

            Return False

        End Function
        ''' <summary>
        ''' get the ColumnDescription of Field 
        ''' </summary>
        ''' <param name="Index">Index no</param>
        ''' <returns>ColumnDescription</returns>
        ''' <remarks>Returns Nothing on range bound exception</remarks>
        Public Function GetColumnDescription(Index As UShort) As ColumnDescription
            If Index > 0 And Index <= _Columns.Length Then
                Return _Columns(Index - 1)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' return a Command
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <param name="commandtype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCommand(ByVal indexname As String, ByVal commandtype As CommandType) As IDbCommand

            If Not _indexDictionary.ContainsKey(indexname) Then
                Call CoreMessageHandler(subname:="clsADONETTableSchema.getCommand", message:="indexname not in IndexDictionary", _
                                      arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            '** return
            Dim aKey = New CommandKey(indexname, commandtype)
            If _CommandStore.ContainsKey(aKey) Then
                Return _CommandStore.Item(aKey)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function CreateNativeDBParameter() As IDbDataParameter
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function CreateNativeDBCommand() As IDbCommand
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function IsNativeDBTypeOfVar(type As Object) As Boolean


        ''' <summary>
        ''' buildcommand builds per Indexname and commandtype the Command and prepare it
        ''' </summary>
        ''' <param name="commandtype">type of clsADONETTableSchema.commandtype</param>
        ''' <param name="indexname">name of the index</param>
        ''' <returns>the IDBCommand </returns>
        ''' <remarks></remarks>
        Protected Friend Function BuildCommand(ByVal indexname As String, _
                                               ByVal commandtype As CommandType, _
                                               Optional ByRef nativeconnection As IDbConnection = Nothing) As IDbCommand

            ' set the IndxColumns
            Dim aColumnCollection As ArrayList
            Dim theIndexColumns() As Object
            Dim commandstr As String
            Dim aParameter As IDataParameter

            Try

                '' do not use initialized since buildcommand is part of initialized
                '' 
                If Me.NoFields = 0 Then
                    Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="table schema is not initialized - does it exist ?", _
                                          arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                Dim aCommand As IDbCommand = createNativeDBCommand()

                If nativeconnection Is Nothing Then
                    nativeconnection = DirectCast(Me._Connection.NativeConnection, IDbConnection)
                End If


                '*****
                '***** BUILD THE DIFFERENT COMMANDS
                '*****
                Select Case (commandtype)


                    '*********
                    '********* SELECT
                    '*********
                    Case clsADONETTableSchema.CommandType.SelectType
                        ' set the IndxColumns
                        If Not _indexDictionary.ContainsKey(indexname) Then
                            Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="indexname not in IndexDictionary", _
                                                  arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                        Else
                            aColumnCollection = _indexDictionary.Item(key:=indexname)
                            theIndexColumns = aColumnCollection.ToArray
                        End If
                        commandstr = "SELECT "
                        For i = 0 To _fieldnames.GetUpperBound(0)
                            commandstr &= String.Format("{0}.[{1}]", _TableID, _Fieldnames(i))
                            If i <> _fieldnames.GetUpperBound(0) Then
                                commandstr &= " , "
                            Else
                                commandstr &= " "
                            End If
                        Next
                        commandstr &= "FROM " & _TableID
                        '**
                        '** where
                        commandstr &= " WHERE "
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            If i > _Fieldnames.GetLowerBound(0) Then
                                commandstr &= " AND "
                            End If
                            commandstr &= String.Format("{0}.[{1}] = @{1}", _TableID, theIndexColumns(i))

                        Next

                        '** Add the Parameters
                        '**
                        aCommand.CommandText = commandstr
                        aCommand.Connection = nativeconnection
                        aCommand.CommandType = Data.CommandType.Text
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            aParameter = AssignNativeDBParameter(theIndexColumns(i))
                            If Not aParameter Is Nothing Then aCommand.Parameters.Add(aParameter)
                        Next
                        SyncLock aCommand.Connection
                            aCommand.Prepare()
                        End SyncLock
                        Return aCommand

                        '*********
                        '********* INSERT
                        '*********
                    Case clsADONETTableSchema.CommandType.InsertType

                        commandstr = "INSERT INTO " & _TableID & "( "
                        For i = 0 To _Fieldnames.GetUpperBound(0)
                            commandstr &= "[" & _Fieldnames(i) & "]"
                            If i <> _Fieldnames.GetUpperBound(0) Then
                                commandstr &= " , "
                            Else
                                commandstr &= " "
                            End If
                        Next
                        commandstr &= ") "
                        '**
                        '** where
                        commandstr &= " VALUES( "
                        For i = 0 To _Fieldnames.GetUpperBound(0)
                            commandstr &= "@" & _Fieldnames(i)
                            If i <> _Fieldnames.GetUpperBound(0) Then
                                commandstr &= " , "
                            Else
                                commandstr &= " "
                            End If
                        Next
                        commandstr &= ")"

                        '** Add the Parameters
                        '**
                        aCommand.CommandText = commandstr
                        aCommand.Connection = nativeconnection
                        aCommand.CommandType = Data.CommandType.Text
                        For i = 0 To _Fieldnames.GetUpperBound(0)
                            aParameter = AssignNativeDBParameter(_Fieldnames(i))
                            If Not aParameter Is Nothing Then aCommand.Parameters.Add(aParameter)
                        Next
                        SyncLock aCommand.Connection
                            aCommand.Prepare()
                        End SyncLock
                        Return aCommand

                        '*********
                        '********* UPDATE
                        '*********
                    Case clsADONETTableSchema.CommandType.UpdateType
                        ' set the IndxColumns
                        If Not _indexDictionary.ContainsKey(indexname) Then
                            Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="index name not in IndexDictionary", _
                                                  arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                        Else
                            aColumnCollection = _indexDictionary.Item(key:=indexname)
                            theIndexColumns = aColumnCollection.ToArray
                        End If
                        commandstr = "UPDATE " & _TableID
                        commandstr &= " SET "
                        Dim first As Boolean = True
                        For i = 0 To _Fieldnames.GetUpperBound(0)
                            '* do not include primary keys
                            If Not Me.HasPrimaryKeyFieldname(_Fieldnames(i)) Then
                                If Not first Then
                                    commandstr &= ", "
                                End If
                                commandstr &= String.Format("[{0}] = @{0}", _Fieldnames(i))
                                first = False
                            End If

                        Next
                        '**
                        '** where
                        commandstr &= " WHERE "
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            If i > _Fieldnames.GetLowerBound(0) Then
                                commandstr &= " AND "
                            End If
                            commandstr &= String.Format("{0}.[{1}] = @{1}", _TableID, theIndexColumns(i))
                        Next

                        '** Add the Parameters
                        '**
                        aCommand.CommandText = commandstr
                        aCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord
                        aCommand.Connection = nativeconnection
                        aCommand.CommandType = Data.CommandType.Text

                        '** UPDATE FIELDS
                        '**
                        For i = 0 To _Fieldnames.GetUpperBound(0)
                            If Not Me.HasPrimaryKeyFieldname(_Fieldnames(i)) Then
                                aParameter = AssignNativeDBParameter(_Fieldnames(i))
                                If Not aParameter Is Nothing Then aCommand.Parameters.Add(aParameter)
                            End If
                        Next
                        '***
                        '*** WHERE CLAUSE
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            aParameter = AssignNativeDBParameter(theIndexColumns(i))
                            If Not aParameter Is Nothing Then aCommand.Parameters.Add(aParameter)
                        Next
                        SyncLock aCommand.Connection
                            aCommand.Prepare()
                        End SyncLock

                        Return aCommand
                        '*********
                        '********* DELETE
                        '*********
                    Case clsADONETTableSchema.CommandType.DeleteType
                        ' set the IndxColumns
                        If Not _indexDictionary.ContainsKey(indexname) Then
                            Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="indexname not in IndexDictionary", _
                                                  arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                        Else
                            aColumnCollection = _indexDictionary.Item(key:=indexname)
                            theIndexColumns = aColumnCollection.ToArray
                        End If
                        commandstr = "DELETE FROM " & _TableID

                        '**
                        '** where
                        commandstr &= " WHERE "
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            If i > _Fieldnames.GetLowerBound(0) Then
                                commandstr &= " AND "
                            End If
                            commandstr &= String.Format("{0}.[{1}] = @{1}", _TableID, theIndexColumns(i))
                        Next

                        '** Add the Parameters
                        '**
                        aCommand.CommandText = commandstr
                        aCommand.Connection = nativeconnection
                        aCommand.CommandType = Data.CommandType.Text
                        For i = 0 To theIndexColumns.GetUpperBound(0)
                            aParameter = AssignNativeDBParameter(theIndexColumns(i))
                            If Not aParameter Is Nothing Then aCommand.Parameters.Add(aParameter)
                        Next
                        SyncLock aCommand.Connection
                            aCommand.Prepare()
                        End SyncLock
                        Return aCommand
                    Case Else
                        Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="commandtype not recognized or implemented", _
                                              arg1:=commandtype, messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                End Select

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableSchema.buildcommand", message:="exception for " & indexname, _
                                      arg1:=commandtype, messagetype:=otCoreMessageType.InternalError, exception:=ex)
                Return Nothing
            End Try
        End Function


    End Class


    '************************************************************************************
    '***** CLASS clsADONETTableStore describes the per Table reference and Helper Class
    '*****                    ORM Mapping Class and Table Access Workhorse
    '*****

    Public MustInherit Class clsADONETTableStore
        Inherits ormTableStore
        Implements iormDataStore


        Protected Friend _cacheTable As DataTable  ' DataTable to cache it
        Protected Friend _cacheViews As New Dictionary(Of String, DataView) ' Dictionary for Dataview per Index
        Protected Friend _cacheAdapter As Data.IDbDataAdapter

        '** initialize


        Public Sub New(connection As iormConnection, TableID As String, ByVal forceSchemaReload As Boolean)
            Call MyBase.New(Connection:=connection, tableID:=TableID, force:=forceSchemaReload)
        End Sub

        ''' converts a Object from OTDB VB.NET Data to ColumnData in the Database
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="value"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <returns>the converted object</returns>
        ''' <remarks></remarks>
        Public Overloads Function Convert2ColumnData(ByVal index As Object, ByVal value As Object, _
        Optional ByRef abostrophNecessary As Boolean = False) As Object Implements iormDataStore.Convert2ColumnData
            Dim aSchema As clsADONETTableSchema = Me.TableSchema
            Dim aDBColumn As clsADONETTableSchema.ColumnDescription
            Dim result As Object
            Dim fieldno As Integer

            result = Nothing
            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.convert2ColumnData", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Convert2ColumnData = DBNull.Value
                Exit Function
            End If


            Try

                fieldno = aSchema.GetFieldordinal(index)
                If fieldno < 0 Then
                    Call CoreMessageHandler(subname:="clsOLEDBTableStoreStore.cvt2ColumnData", _
                                          message:="iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found", _
                                          tablename:=Me.TableID, arg1:=index, messagetype:=otCoreMessageType.InternalError)
                    System.Diagnostics.Debug.WriteLine("iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found")

                    Convert2ColumnData = DBNull.Value
                    Exit Function
                Else
                    aDBColumn = aSchema.GetColumnDescription(fieldno)
                End If
                abostrophNecessary = False

                '*
                '*
                If IsError(value) Then
                    Call CoreMessageHandler(subname:="clsADONETTablestore.cvt2ColumnData", _
                                          message:="Error in Formular of field value " & value & " while updating OTDB", _
                                          arg1:=value, entryname:=aDBColumn.ColumnName, tablename:=aSchema.TableID, messagetype:=otCoreMessageType.InternalError)
                    System.Diagnostics.Debug.WriteLine("Error in Formular of field value " & value & " while updating OTDB")
                    value = ""
                End If

                '***
                '*** convert
                Return Connection.DatabaseDriver.Convert2DBData(value:=value, targetType:=aDBColumn.DataType, maxsize:=aDBColumn.CharacterMaxLength, _
                                          abostrophNecessary:=abostrophNecessary, fieldname:=aDBColumn.ColumnName)


            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsADONETTablestore.cvt2ColumnData", messagetype:=otCoreMessageType.InternalError, _
                                      tablename:=Me.TableID, entryname:=aDBColumn.ColumnName, exception:=ex, arg1:=index & ": '" & value & "'")
                Return Nothing

            End Try


        End Function
        '*********
        '********* cvt2ObjData returns a object from the Datatype of the column to XLS nterpretation
        '*********

        Public MustOverride Function Convert2ObjectData(ByVal index As Object, ByVal value As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object

        ''' <summary>
        ''' if Cache is Initialized and running 
        ''' </summary>
        ''' <returns>return true</returns>
        ''' <remarks></remarks>
        Public Function IsCacheInitialized() As Boolean
            If _cacheAdapter Is Nothing OrElse _cacheTable Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Function
        ''' <summary>
        ''' Initialize Cache 
        ''' </summary>
        ''' <returns>true if successfull </returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function InitializeCache(Optional ByVal force As Boolean = False) As Boolean

        ''' <summary>
        ''' specific Command
        ''' </summary>
        ''' <param name="commandstr"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend MustOverride Function CreateNativeDBCommand(ByVal commandstr As String, ByRef nativeConnection As IDbConnection) As IDbCommand

        ''' <summary>
        ''' deletes a Record in the database by Primary key
        ''' </summary>
        ''' <param name="primaryKeyArray">Array of Objects as Primary Key</param>
        ''' <param name="silent"></param>
        ''' <returns>true if successfull </returns>
        ''' <remarks></remarks>
        Public Overrides Function DelRecordByPrimaryKey(ByRef primaryKeyArray() As Object, Optional silent As Boolean = False) As Boolean _
        Implements iormDataStore.DelRecordByPrimaryKey
            Dim otdbcn As IDbConnection
            Dim aSQLDeleteCommand As IDbCommand

            Dim j As Integer
            Dim fieldname As String = ""
            Dim aValue As Object
            Dim wherestr As String = ""
            Dim abostrophNecessary As Boolean
            Dim acvtvalue As Object

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.DelRecordByPrimaryKey", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return False
            End If


            If Not IsArray(primaryKeyArray) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", message:="Empty Key Array")
                WriteLine("uups - no Array as primaryKey")
                Return False
            ElseIf primaryKeyArray.GetUpperBound(0) > (Me.TableSchema.NoPrimaryKeyFields - 1) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", message:="Size of Primary Key Array less than the number of primary keys", _
                                      arg1:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

            ' Connection
            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", message:="Connection is not available")
                    Return False
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", exception:=ex)
                Return False
            End Try

            '*** check on Property Cached
            If Me.HasProperty(ConstTPNCacheProperty) Then
                If Not Me.IsCacheInitialized Then
                    Me.InitializeCache()
                End If
            End If

            '* get PrimaryKeys and their value -> build the criteria
            '*
            aSQLDeleteCommand = TryCast(Me.TableSchema, clsADONETTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                               clsADONETTableSchema.CommandType.DeleteType)
            If aSQLDeleteCommand Is Nothing Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", message:="DeleteCommand is not in Store", _
                                      arg1:=Me.TableSchema.PrimaryKeyIndexName, messagetype:=otCoreMessageType.InternalError)
                Return False
            End If
            SyncLock aSQLDeleteCommand.Connection

                Try


                    For j = 0 To (Me.TableSchema.NoPrimaryKeyFields - 1)

                        ' value of key
                        aValue = primaryKeyArray(j)
                        fieldname = Me.TableSchema.GetPrimaryKeyfieldname(j + 1)
                        If j <> 0 Then
                            wherestr &= String.Format(" AND [{0}]", fieldname)
                        Else
                            wherestr &= String.Format(" [{0}]", fieldname)
                        End If
                        If fieldname <> "" Then
                            acvtvalue = Me.Convert2ColumnData(fieldname, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(acvtvalue) Then
                                aSQLDeleteCommand.Parameters(j).Value = acvtvalue
                                If abostrophNecessary Then
                                    wherestr &= " = '" & acvtvalue.ToString & "'"
                                Else
                                    wherestr &= " = " & acvtvalue.ToString
                                End If
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordByPrimaryKey", message:="Value for primary key couldnot be converted to ColumnData", _
                                                      arg1:=aValue, messagetype:=otCoreMessageType.InternalError, entryname:=fieldname, tablename:=Me.TableID)
                                Return Nothing

                            End If
                        End If

                    Next j

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsADONETTableStore.delRecordByPrimaryKey", message:="Exception", exception:=ex)
                    Return False
                End Try

                ' find it
                Try
                    '*** check on Property Cached
                    If Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized Then

                        Dim dataRows() As DataRow = _cacheTable.Select(wherestr)
                        SyncLock dataRows
                            ' not found
                            If dataRows.GetLength(0) = 0 Then
                                DelRecordByPrimaryKey = False
                            Else
                                dataRows(0).Delete()
                                DelRecordByPrimaryKey = True
                            End If
                        End SyncLock
                        '* InstantUpdate not implemented

                        If UpdateDBDataTable(_cacheAdapter, _cacheTable) > 0 Then
                            DelRecordByPrimaryKey = True
                        Else
                            DelRecordByPrimaryKey = False
                        End If

                        If False Then
                            If Me.HasProperty(ConstTPNCacheUpdateInstant) Then
                                If UpdateDBDataTable(_cacheAdapter, _cacheTable) > 0 Then
                                    DelRecordByPrimaryKey = True
                                Else
                                    DelRecordByPrimaryKey = False
                                End If
                            Else
                                CoreMessageHandler(message:="not implemented")
                            End If
                        End If

                    Else
                        If aSQLDeleteCommand.ExecuteNonQuery > 0 Then
                            DelRecordByPrimaryKey = True
                        Else
                            DelRecordByPrimaryKey = False
                        End If

                    End If

                    Return DelRecordByPrimaryKey


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.delRecordByPrimaryKeys", _
                                          tablename:=Me.TableID, entryname:=fieldname, exception:=ex)
                    Return False
                End Try

            End SyncLock

        End Function

        ''' <summary>
        ''' GetRecordbyPrimaryKey returns a clsOTDBRecord object by the Primarykey from the Database
        ''' </summary>
        ''' <param name="primaryKeyArray">PrimaryKey Array</param>
        ''' <param name="silent"></param>
        ''' <returns>returns Nothing if not found otherwise a clsOTDBRecord</returns>
        ''' <remarks></remarks>
        Public Overrides Function GetRecordByPrimaryKey(ByRef primaryKeyArray() As Object, Optional silent As Boolean = False) As ormRecord _
        Implements iormDataStore.GetRecordByPrimaryKey
            'Dim aConnection As IDbConnection
            Dim aSqlSelectCommand As IDbCommand
            Dim j As Integer
            Dim afieldname As String
            Dim aValue As Object
            Dim wherestr As String = ""
            Dim abostrophNecessary As Boolean
            Dim aCvtValue As Object


            If Not IsArray(primaryKeyArray) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByPrimaryKey", message:="Empty Key Array")
                WriteLine("uups - no Array as primaryKey")
                Return Nothing
            ElseIf primaryKeyArray.GetUpperBound(0) < (Me.TableSchema.NoPrimaryKeyFields - 1) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByPrimaryKey", message:="Size of Primary Key Array less than the number of primary keys", _
                                      arg1:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            End If

            ' Connection
            Try
                If Me.Connection.isConnected Then
                    'aConnection = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByPrimaryKey", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByPrimaryKey", exception:=ex)
                Return Nothing
            End Try

            ''' check if schema is initialized
            ''' 
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByPrimaryKey", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return Nothing
            End If

            '*** check on Property Cached
            If Me.HasProperty(ConstTPNCacheProperty) Then
                If Not Me.IsCacheInitialized Then
                    Me.InitializeCache()
                End If


            End If

            '* get PrimaryKeys and their value -> build the criteria
            '*
            aSqlSelectCommand = TryCast(Me.TableSchema, clsADONETTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, clsADONETTableSchema.CommandType.SelectType)
            If aSqlSelectCommand Is Nothing Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordByPrimaryKey", message:="Select Command is not in Store", _
                                      arg1:=Me.TableSchema.PrimaryKeyIndexName, messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            SyncLock aSqlSelectCommand.Connection

                Try

                    For j = 0 To (Me.TableSchema.NoPrimaryKeyFields - 1)

                        ' value of key
                        aValue = primaryKeyArray(j)
                        afieldname = Me.TableSchema.GetPrimaryKeyfieldname(j + 1)
                        If j <> 0 Then
                            wherestr &= String.Format(" AND [{0}]", afieldname)
                        Else
                            wherestr &= String.Format(" [{0}]", afieldname)
                        End If
                        If afieldname <> "" Then
                            aCvtValue = Me.Convert2ColumnData(afieldname, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(aCvtValue) And Not aCvtValue Is Nothing Then
                                ' build parameter
                                aSqlSelectCommand.Parameters(j).Value = aCvtValue
                                ' and build wherestring for cache
                                If abostrophNecessary Then
                                    wherestr &= " = '" & aCvtValue.ToString & "'"
                                Else
                                    wherestr &= " = " & aCvtValue.ToString
                                End If
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordByPrimaryKey", message:="Value for primary key couldnot be converted to ColumnData", _
                                                      arg1:=aValue, messagetype:=otCoreMessageType.InternalError, entryname:=afieldname, tablename:=Me.TableID)
                                Return Nothing

                            End If
                        End If

                    Next j

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordByPrimaryKey", message:="Exception", exception:=ex)
                    Return Nothing
                End Try


                '**** read
                Try
                    '*** check on Property Cached
                    If Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized Then
                        Dim dataRows() As DataRow = _cacheTable.Select(wherestr)

                        ' not found
                        If dataRows.GetLength(0) = 0 Then
                            Return Nothing
                        Else
                            '** Factory a new clsOTDBRecord
                            '**
                            Dim aNewEnt As New ormRecord
                            If InfuseRecord(record:=aNewEnt, dataobject:=dataRows(0)) Then
                                Return aNewEnt
                            Else

                                Return Nothing
                            End If
                        End If
                    Else
                        Dim aDataReader As IDataReader = aSqlSelectCommand.ExecuteReader
                        If aDataReader.Read Then
                            '** Factory a new clsOTDBRecord
                            '**
                            Dim aNewEnt As New ormRecord
                            If InfuseRecord(aNewEnt, aDataReader) Then
                                aDataReader.Close()
                                Return aNewEnt
                            Else
                                aDataReader.Close()
                                Return Nothing
                            End If
                        Else
                            aDataReader.Close()
                            Return Nothing
                        End If


                    End If



                    '*****
                    '***** Error Handling
                    '*****
              
                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.getRecordByPrimaryKey", _
                                          tablename:=Me.TableID, arg1:=primaryKeyArray, exception:=ex)

                    Return Nothing
                End Try

            End SyncLock

        End Function

        '****** getRecords by Index
        '******
        Public Overrides Function GetRecordsByIndex(indexname As String, ByRef keyArray() As Object, Optional silent As Boolean = False) As List(Of ormRecord) _
        Implements iormDataStore.GetRecordsByIndex
            'Dim aConnection As IDbConnection
            Dim aSqlSelectCommand As IDbCommand
            Dim j As Integer
            Dim fieldname As String
            Dim aValue As Object
            Dim anIndexColumnList As ArrayList
            Dim abostrophNecessary As Boolean
            Dim aCvtValue As Object
            Dim wherestr As String = ""
            Dim anewEnt As ormRecord
            Dim aCollection As New List(Of ormRecord)

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.DelRecordsByPrimaryKey", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return Nothing
            End If

            '* get Index and their value -> build the criteria
            '*
            If Me.TableSchema.HasIndex(indexname) Then

                anIndexColumnList = Me.TableSchema.GetIndex(indexname)
            ElseIf Me.TableSchema.HasIndex(String.Format("{0}_{1}", Me.TableID, indexname)) Then
                indexname = String.Format("{0}_{1}", Me.TableID, indexname)
                anIndexColumnList = Me.TableSchema.GetIndex(indexname)
            Else
                Call CoreMessageHandler(subname:="clsADOStore.getRecordsByIndex", arg1:=indexname, _
                                      message:="Index does not exists for Table " & Me.TableID, messagetype:=otCoreMessageType.InternalError, _
                                      tablename:=Me.TableID)
                Return Nothing
            End If

            If Not IsArray(keyArray) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Empty Key Array", _
                                      messagetype:=otCoreMessageType.InternalError, _
                                      tablename:=Me.TableID)
                WriteLine("uups - no Array as primaryKey")
                Return Nothing
            ElseIf keyArray.GetUpperBound(0) > (anIndexColumnList.Count - 1) Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Size of Primary Key Array less than the number of primary keys", _
                                      arg1:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            End If

            ' Connection
            Try
                If Me.Connection.IsConnected Then
                    'aConnection = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", exception:=ex)
                Return Nothing
            End Try

            '*** check on Property Cached
            If Me.HasProperty(ConstTPNCacheProperty) Then
                If Not Me.IsCacheInitialized Then
                    Me.InitializeCache()
                End If
            End If

            '* get PrimaryKeys and their value -> build the criteria
            '*
            aSqlSelectCommand = TryCast(Me.TableSchema, clsADONETTableSchema).GetCommand(indexname, clsADONETTableSchema.CommandType.SelectType)
            If aSqlSelectCommand Is Nothing Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Select Command is not in Store", _
                                      arg1:=Me.TableSchema.PrimaryKeyIndexName, messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            SyncLock aSqlSelectCommand.Connection

                Try

                    For j = 0 To (anIndexColumnList.Count - 1)

                        ' value of key
                        aValue = keyArray(j)
                        fieldname = anIndexColumnList.Item(j)
                        If j <> 0 Then
                            wherestr &= String.Format(" AND [{0}]", fieldname)
                        Else
                            wherestr &= "[" & fieldname & "]"
                        End If
                        If fieldname <> "" Then
                            aCvtValue = Me.Convert2ColumnData(fieldname, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(aCvtValue) Then
                                ' set parameter
                                aSqlSelectCommand.Parameters(j).Value = aCvtValue
                                ' and build wherestring for cache
                                If abostrophNecessary Then
                                    wherestr &= " = '" & aCvtValue.ToString & "'"
                                Else
                                    wherestr &= " = " & aCvtValue.ToString
                                End If
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Value for primary key couldnot be converted to ColumnData", _
                                                      arg1:=aValue, messagetype:=otCoreMessageType.InternalError, entryname:=fieldname, tablename:=Me.TableID)
                                Return Nothing

                            End If
                        End If

                    Next j

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsByIndex", message:="Exception", exception:=ex)
                    Return New List(Of ormRecord)
                End Try


                '**** read

                Try
                    '*** check on Property Cached
                    If Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized Then
                        Dim dataRows() As DataRow
                        If _cacheViews.ContainsKey(key:=indexname) Then
                            Dim aDataView = _cacheViews.Item(key:=indexname)

                            dataRows = aDataView.Table.Select()
                        Else
                            dataRows = _cacheTable.Select(wherestr)
                        End If

                        ' not found
                        If dataRows.GetLength(0) = 0 Then
                            Return aCollection
                        Else
                            For Each row In dataRows
                                anewEnt = New ormRecord
                                If InfuseRecord(anewEnt, row) Then
                                    aCollection.Add(Item:=anewEnt)
                                Else
                                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", message:="couldnot infuse a record", _
                                                          arg1:=anewEnt, tablename:=Me.TableID, break:=False)
                                End If
                            Next
                        End If
                    Else
                        Dim aDataReader As IDataReader

                        aDataReader = aSqlSelectCommand.ExecuteReader

                        Do While aDataReader.Read
                            '** Factory a new clsOTDBRecord
                            anewEnt = New ormRecord
                            If InfuseRecord(anewEnt, aDataReader) Then
                                aCollection.Add(Item:=anewEnt)
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", message:="couldnot infuse a record", _
                                                      arg1:=anewEnt, tablename:=Me.TableID, break:=False)
                            End If

                        Loop

                        aDataReader.Close()
                        Return aCollection
                    End If
                    '*****
                    '***** Error Handling
                    '*****
                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.getRecordsByIndex", _
                                          tablename:=Me.TableID, arg1:=keyArray, exception:=ex)

                    Return New List(Of ormRecord)
                End Try

            End SyncLock

        End Function

        ''' <summary>
        ''' Update a Datatable with the adapter
        ''' </summary>
        ''' <param name="datatable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function UpdateDBDataTable(ByRef dataadapter As IDbDataAdapter, ByRef datatable As DataTable) As Integer

        '****** runs a SQLCommand
        '******
        Public Overrides Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, Optional silent As Boolean = True) As Boolean _
        Implements iormDataStore.RunSqlStatement

            Return Me.Connection.DatabaseDriver.RunSqlStatement(sqlcmdstr:=sqlcmdstr, parameters:=parameters, silent:=silent)

        End Function
        '****** returns the Collection of Records by SQL
        '******
        Public Overrides Function GetRecordsBySql(ByVal wherestr As String, _
        Optional ByVal fullsqlstr As String = "", _
        Optional ByVal innerjoin As String = "", _
        Optional ByVal orderby As String = "", _
        Optional ByVal silent As Boolean = False, _
        Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) Implements iormDataStore.GetRecordsBySql

            Dim aConnection As IDbConnection
            Dim i As Integer
            Dim cmdstr As String
            Dim aCollection As New List(Of ormRecord)
            Dim aNewEnt As ormRecord
            Dim fieldstr As String

            ' Connection
            Try
                If Me.Connection.IsConnected Then
                    aConnection = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", exception:=ex)
                Return Nothing
            End Try

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordBySQL", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return Nothing
            End If

            '*** check on Property Cached
            If Me.HasProperty(ConstTPNCacheProperty) Then
                If Not Me.IsCacheInitialized Then
                    Me.InitializeCache()
                End If
            End If


            If fullsqlstr <> "" Then
                cmdstr = fullsqlstr
            Else

                i = 0
                fieldstr = ""
                For Each field As String In Me.TableSchema.fieldnames
                    If i = 0 Then
                        fieldstr = Me.TableID & ".[" & field & "]"
                        i += 1
                    Else
                        fieldstr &= " , " & Me.TableID & ".[" & field & "]"
                    End If
                Next

                ' Select
                If innerjoin = "" Then
                    cmdstr = String.Format("SELECT * FROM {0} WHERE {1}", Me.TableID, wherestr)
                Else
                    cmdstr = "SELECT " & fieldstr & " FROM " & Me.TableID & " " & innerjoin & " WHERE " & wherestr
                End If

                If orderby <> "" Then
                    cmdstr = cmdstr & " ORDER BY " & orderby
                End If
            End If

            Try
                '*** check on Property Cached
                If Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized Then
                    Dim dataRows() As DataRow = _cacheTable.Select(wherestr)

                    ' not found
                    If dataRows.GetLength(0) = 0 Then
                        Return aCollection
                    Else
                        For Each row In dataRows
                            aNewEnt = New ormRecord
                            If InfuseRecord(aNewEnt, row) Then
                                aCollection.Add(Item:=aNewEnt)
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", message:="couldnot infuse a record", _
                                                      arg1:=aNewEnt, tablename:=Me.TableID, break:=False)
                            End If
                        Next
                    End If
                Else
                    Dim aSqlCommand As IDbCommand = CreateNativeDBCommand(cmdstr, aConnection)
                    Dim aDataReader As IDataReader
                    SyncLock aSqlCommand.Connection
                        ' read
                        aDataReader = aSqlCommand.ExecuteReader
                        Do While aDataReader.Read
                            '** Factory a new clsOTDBRecord
                            aNewEnt = New ormRecord
                            If InfuseRecord(aNewEnt, aDataReader) Then
                                aCollection.Add(item:=aNewEnt)
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQL", message:="couldnot infuse a record", _
                                                      arg1:=aNewEnt, tablename:=Me.TableID, break:=False)
                            End If

                        Loop

                        ' close
                        aDataReader.Close()

                    End SyncLock
                End If



                ' return
                If aCollection.Count > 0 Then
                    GetRecordsBySql = aCollection
                Else
                    GetRecordsBySql = Nothing
                End If

                Exit Function

                '******** error handling
            Catch ex As Exception

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.getRecordsBySQL", tablename:=Me.TableID, _
                                      arg1:="Where :" & wherestr & " inner join: " & innerjoin & " full: " & fullsqlstr, _
                                      exception:=ex)

                Return New List(Of ormRecord)
            End Try



        End Function
        ''' <summary>
        ''' returns a collection of records selected by this helper command which creates an SqlCommand with an ID or reuse one
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <param name="wherestr"></param>
        ''' <param name="fullsqlstr"></param>
        ''' <param name="innerjoin"></param>
        ''' <param name="orderby"></param>
        ''' <param name="silent"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GetRecordsBySqlCommand(ByVal ID As String, _
                                    Optional ByVal wherestr As String = "", _
                                    Optional ByVal fullsqlstr As String = "", _
                                    Optional ByVal innerjoin As String = "", _
                                    Optional ByVal orderby As String = "", _
                                    Optional ByVal silent As Boolean = False, _
                                    Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) _
                                Implements iormDataStore.GetRecordsBySqlCommand


            Dim aCollection As New List(Of ormRecord)
            Dim aParameterValues As New Dictionary(Of String, Object)
            Dim aCommand As ormSqlSelectCommand

            '*** check on Property Cached
            If Me.HasProperty(ConstTPNCacheProperty) Then
                If Not Me.IsCacheInitialized Then
                    Me.InitializeCache()
                End If
            End If

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.GetRecordBySQLCommand", messagetype:=otCoreMessageType.InternalError, _
                                      message:="table schema could not be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return Nothing
            End If

            Try
                ' get
                aCommand = Me.CreateSqlSelectCommand(ID)
                SyncLock aCommand
                    If Not aCommand.Prepared Then
                        aCommand.AddTable(Me.TableID, addAllFields:=True)
                        aCommand.Where = wherestr
                        aCommand.InnerJoin = innerjoin
                        aCommand.OrderBy = orderby
                        'If fullsqlstr <> "" then aCommand.SqlText = fullsqlstr 
                        If parameters IsNot Nothing Then
                            For Each aParameter In parameters
                                aCommand.AddParameter(aParameter)
                                aParameterValues.Add(aParameter.ID, aParameter.Value)
                            Next
                        End If

                        If Not aCommand.Prepare Then
                            Call CoreMessageHandler(message:="couldnot prepare command", subname:="clsADONETTableStore.getRecordsBySQLCommand", _
                                                   messagetype:=otCoreMessageType.InternalError, arg1:=aCommand.SqlText)
                            Return New List(Of ormRecord)
                        End If
                    End If


                    '*** check on Property Cached
                    '***
                    If Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized Then
                        Dim aDataview = _cacheTable.AsDataView
                        If aCommand.OrderBy <> "" Then
                            aDataview.Sort = aCommand.OrderBy
                        End If
                        Dim wherestatement As String = aCommand.Where
                        wherestatement = wherestatement.Replace("[", " ").Replace("]", " ")
                        If wherestatement.Contains(".") Then
                            '** strip off all the table namings
                            wherestatement = Regex.Replace(wherestatement, "\S*\.", "")
                        End If
                        '** replace the values
                        If aCommand.Parameters IsNot Nothing Then
                            For Each aParameter In aCommand.Parameters
                                If aParameter.Datatype <> otFieldDataType.Memo And aParameter.Datatype <> otFieldDataType.Text And aParameter.Datatype <> otFieldDataType.List Then
                                    wherestatement = wherestatement.Replace(aParameter.ID, aParameter.Value)
                                Else
                                    wherestatement = wherestatement.Replace(aParameter.ID, "'" & aParameter.Value & "'")
                                End If
                            Next
                        End If

                        aDataview.RowFilter = wherestatement
                        Dim dataRows() As DataRow = aDataview.ToTable.Select()

                        ' not found
                        If dataRows.GetLength(0) = 0 Then
                            Return aCollection
                        Else
                            For Each row In dataRows
                                Dim aNewEnt = New ormRecord
                                If InfuseRecord(aNewEnt, row) Then
                                    aCollection.Add(item:=aNewEnt)
                                Else
                                    Call CoreMessageHandler(subname:="clsADONETTableStore.getRecordsBySQLCommand", message:="couldnot infuse a record", _
                                                          arg1:=aNewEnt, tablename:=Me.TableID, break:=False)
                                End If
                            Next
                        End If

                        Return aCollection
                    Else
                        '** NOCACHE
                        '** run the Command
                        Dim theRecords As List(Of ormRecord) = _
                            Me.Connection.DatabaseDriver.RunSqlSelectCommand(aCommand, parametervalues:=aParameterValues)

                        Return theRecords
                    End If
                End SyncLock
                '******** error handling
            Catch ex As Exception

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.getRecordsBySQLCommand", tablename:=Me.TableID, _
                                      arg1:="Where :" & wherestr & " inner join: " & innerjoin & " full: " & fullsqlstr, _
                                      exception:=ex)

                Return New List(Of ormRecord)
            End Try



        End Function

        ''' <summary>
        ''' infuse a Record with the Help of the Datareader Object
        ''' </summary>
        ''' <param name="record">clsOTDBRecord</param>
        ''' <param name="DataReader">an open Datareader which has just the data</param>
        ''' <param name="silent">no messages</param>
        ''' <returns>True if successfull and read</returns>
        ''' <remarks></remarks>
        Public Overrides Function InfuseRecord(ByRef record As ormRecord, _
        ByRef dataobject As Object, _
        Optional ByVal silent As Boolean = False) As Boolean _
        Implements iormDataStore.InfuseRecord
            Dim aDBColumn As clsADONETTableSchema.ColumnDescription
            Dim cvtvalue, Value As Object
            Dim j As Integer
            Dim abostrophNecessary As Boolean
            Dim ordinal As Nullable(Of Integer)
            Dim aDatareader As IDataReader = Nothing
            Dim aRow As DataRow = Nothing

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.InfuseRecord", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return Nothing
            End If

            Try
                If GetType(IDataReader).IsAssignableFrom(dataobject.GetType) AndAlso Not dataobject.GetType.IsAbstract Then
                    aDatareader = DirectCast(dataobject, IDataReader)

                ElseIf dataobject.GetType() = GetType(DataRow) Then
                    aRow = DirectCast(dataobject, DataRow)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.infuseRecord", message:="Data object has no known type", _
                                          arg1:=dataobject.GetType.ToString)
                    Return False

                End If
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.infuseRecord", exception:=ex, message:="Exception", _
                                      arg1:=dataobject.GetType.ToString)
                Return False
            End Try
            Try

                '** Factory a new clsOTDBRecord
                '**
                record = New ormRecord(TableID)
                record.IsLoaded = True ' definitely loaded ! not created

                For j = 1 To Me.TableSchema.NoFields
                    ' get fields
                    aDBColumn = DirectCast(Me.TableSchema, clsADONETTableSchema).GetColumnDescription(j)
                    If aDBColumn IsNot Nothing Then
                        Try
                            If Not aDatareader Is Nothing Then
                                ordinal = aDatareader.GetOrdinal(aDBColumn.ColumnName)
                            End If
                        Catch ex As System.IndexOutOfRangeException
                            Try
                                ordinal = aDatareader.GetOrdinal(String.Format("{0}.{1}", Me.TableID, aDBColumn.ColumnName))
                            Catch ex2 As Exception
                                Call CoreMessageHandler(exception:=ex2, message:="Exception", subname:="clsADONETTableStore.infuseRecord", _
                                                      arg1:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                            Finally
                                ordinal = Nothing
                            End Try
                        End Try

                        If aDatareader IsNot Nothing Then
                            If ordinal IsNot Nothing AndAlso ordinal >= 0 Then
                                Value = aDatareader.GetValue(ordinal)
                                cvtvalue = Me.Convert2ObjectData(j, Value, abostrophNecessary)
                                Call record.SetValue(j, cvtvalue)
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.infuseRecord", message:="ordinal missing - Field not in DataReader", _
                                                      entryname:=aDBColumn.ColumnName, tablename:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                            End If
                        Else
                            '** aRow
                            Value = aRow.Item(j - 1)
                            cvtvalue = Me.Convert2ObjectData(j, Value, abostrophNecessary)
                            Call record.SetValue(j, cvtvalue)
                        End If
                    Else
                        Call CoreMessageHandler(subname:="clsADONETTableStore.infuseRecord", message:="DBColumn missing - Field not in DataReader", _
                                              arg1:=j, tablename:=Me.TableID, messagetype:=otCoreMessageType.InternalError)
                    End If
                Next j

                Return True

            Catch ex As Exception
                Call CoreMessageHandler(exception:=ex, subname:="clsADONETTableStore.infuseRecord")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' persists aRecord to the database if aRecord is created or loaded
        ''' </summary>
        ''' <param name="record">clsOTDBRecord</param>
        ''' <param name="timestamp">the Timestamp to be used for the ChangedOn or CreatedOn</param>
        ''' <param name="silent">no messages</param>
        ''' <returns>true if successfull and written, false if error or no changes</returns>
        ''' <remarks></remarks>
        Public Function PersistCache(ByRef record As ormRecord, _
                                     Optional ByVal timestamp As Date = ot.ConstNullDate, _
                                     Optional ByVal silent As Boolean = False) As Boolean

            Dim fieldname As String
            Dim aCVTValue, aValue As Object
            Dim j As Integer
            Dim abostrophNecessary As Boolean
            Dim otdbcn As IDbConnection
            Dim wherestr As String = ""
            Dim changedRecord As Boolean
            Dim dataRows() As DataRow

            ' timestamp
            If timestamp = ConstNullDate Then
                timestamp = Date.Now
            End If

            ' Connection

            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", exception:=ex)
                Return Nothing
            End Try

            '*** check on Property Cached

            If Not Me.IsCacheInitialized Then
                Me.InitializeCache()
            End If

            '*** Try to persist

            Try
                '*** Check if not Status
                If record.IsUnknown OrElse (Not record.IsCreated And Not record.IsLoaded) Then
                    If Not record.CheckStatus Then
                        Return False
                    End If
                End If

                '*** Check which Command to use
                If record.IsLoaded Then

                    'build wherestring
                    For j = 0 To (Me.TableSchema.NoPrimaryKeyFields - 1)
                        ' value of key
                        fieldname = Me.TableSchema.GetPrimaryKeyfieldname(j + 1)
                        If j <> 0 Then
                            wherestr &= String.Format(" AND [{0}]", fieldname)
                        Else
                            wherestr &= String.Format("[{0}]", fieldname)
                        End If
                        aValue = record.GetValue(fieldname)
                        If fieldname <> "" Then
                            aCVTValue = Me.Convert2ColumnData(fieldname, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(aCVTValue) Then

                                If abostrophNecessary Then
                                    wherestr &= " = '" & aCVTValue.ToString & "'"
                                Else
                                    wherestr &= " = " & aCVTValue.ToString
                                End If
                            Else
                                Call CoreMessageHandler(subname:="clsADONETTableStore.PersistCache", message:="Value for primary key couldnot be converted to ColumnData", _
                                                      arg1:=aValue, messagetype:=otCoreMessageType.InternalError, entryname:=fieldname, tablename:=Me.TableID)
                                Return False

                            End If
                        End If

                    Next j

                    ' load
                    dataRows = _cacheTable.Select(wherestr)

                    If dataRows.Length = 0 Then
                        Call CoreMessageHandler(subname:="clsADONETTableStore.persistCache", message:="Datarow to update not found", tablename:=Me.TableID)
                        Return False
                    End If


                ElseIf record.IsCreated Then
                    ReDim dataRows(0)
                    dataRows(0) = _cacheTable.NewRow
                    'set all primary keys
                    For j = 1 To Me.TableSchema.NoFields
                        ' get fields
                        fieldname = Me.TableSchema.Getfieldname(j)
                        If Me.TableSchema.HasprimaryKeyfieldname(fieldname) Then
                            aValue = record.GetValue(fieldname)
                            aCVTValue = Me.Convert2ColumnData(j, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(aCVTValue) And Not IsNothing(aCVTValue) And record.IsValueChanged(j) Then
                                dataRows(0).Item(fieldname) = aCVTValue
                            End If
                        End If
                    Next j
                Else

                    Call CoreMessageHandler(subname:="clsADONETTableStore.persistCache", arg1:=Me.TableSchema.PrimaryKeyIndexName, _
                                          message:="record is nor loaded or created", messagetype:=otCoreMessageType.InternalError, _
                                          tablename:=Me.TableID)
                    Return False
                End If



                'get all fields
                For j = 1 To Me.TableSchema.NoFields
                    ' get fields
                    fieldname = Me.TableSchema.Getfieldname(j)

                    If Not Me.TableSchema.HasprimaryKeyfieldname(fieldname) Then
                        If fieldname <> ConstFNUpdatedOn And fieldname <> "" And fieldname <> ConstFNCreatedOn Then
                            aValue = record.GetValue(fieldname)
                            aCVTValue = Me.Convert2ColumnData(j, aValue, abostrophNecessary)
                            If Not DBNull.Value.Equals(aCVTValue) And Not IsNothing(aCVTValue) And record.IsValueChanged(j) Then
                                dataRows(0).Item(fieldname) = aCVTValue
                                changedRecord = True
                            End If
                        End If
                    End If
                Next j
                ' Update the record
                If changedRecord Then

                    '**** UpdateTimeStamp
                    If Me.TableSchema.GetFieldordinal(ConstFNUpdatedOn) > 0 Then
                        'rst.Fields(OTDBConst_UpdateOn).Value = aTimestamp
                        dataRows(0).Item(ConstFNUpdatedOn) = timestamp
                    End If

                    '*** Create Timestamp
                    If Me.TableSchema.GetFieldordinal(ConstFNCreatedOn) > 0 And record.IsCreated Then
                        dataRows(0).Item(ConstFNCreatedOn) = timestamp
                    ElseIf Me.TableSchema.GetFieldordinal(ConstFNCreatedOn) > 0 And Not record.IsCreated Then
                        If Not DBNull.Value.Equals(record.GetValue(ConstFNCreatedOn)) And Not record.GetValue(ConstFNCreatedOn) Is Nothing Then
                            dataRows(0).Item(ConstFNCreatedOn) = record.GetValue(ConstFNCreatedOn)    'keep the value
                        ElseIf Me.TableSchema.GetFieldordinal(ConstFNUpdatedOn) > 0 AndAlso _
                        Not DBNull.Value.Equals(record.GetValue(ConstFNUpdatedOn)) _
                        AndAlso Not record.GetValue(ConstFNUpdatedOn) Is Nothing Then
                            dataRows(0).Item(ConstFNCreatedOn) = record.GetValue(ConstFNUpdatedOn)    'keep the value
                        Else
                            dataRows(0).Item(ConstFNCreatedOn) = timestamp
                        End If
                    End If


                End If



                '** Run Command
                If changedRecord Then
                    '* add the record
                    If record.IsCreated Then
                        _cacheTable.Rows.Add(dataRows(0))
                        PersistCache = True
                    End If
                    ' save to the database not only the cache
                    ' synclock on connection of update (should be the same as insertCommand)
                    SyncLock _cacheAdapter.UpdateCommand.Connection
                        If Me.IsCacheInitialized Then
                            If UpdateDBDataTable(_cacheAdapter, _cacheTable) > 0 Then
                                PersistCache = True
                            End If
                        Else
                            CoreMessageHandler(message:="persist to an uninitialized cache ?!", subname:="clsADONETTableStore.PersistCache", _
                                                messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID, arg1:=dataRows.ToString)
                        End If

                    End SyncLock

                    If False Then
                        If Me.HasProperty(ConstTPNCacheUpdateInstant) AndAlso Me.IsCacheInitialized Then
                            SyncLock _cacheAdapter.UpdateCommand.Connection
                                If UpdateDBDataTable(_cacheAdapter, _cacheTable) > 0 Then
                                    PersistCache = True
                                End If
                            End SyncLock
                        ElseIf Not Me.HasProperty(ConstTPNCacheUpdateInstant) Then
                            CoreMessageHandler(message:="Perist later is not implemented", subname:="clsADONETTableStore.PersistCache", _
                                              messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID, arg1:=dataRows.ToString)
                        ElseIf Not Me.IsCacheInitialized Then
                            CoreMessageHandler(message:="persist to an uninitialized cache ?!", subname:="clsADONETTableStore.PersistCache", _
                                                messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID, arg1:=dataRows.ToString)
                        End If
                    End If
                    Return PersistCache
                Else
                    Return True
                End If



            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADONETTableStore.persistRecord", exception:=ex, tablename:=Me.TableID)
                Return False
            End Try



        End Function
        ''' <summary>
        ''' persists aRecord to the database if aRecord is created or loaded
        ''' </summary>
        ''' <param name="aRecord">clsOTDBRecord</param>
        ''' <param name="aTimestamp">the Timestamp to be used for the ChangedOn or CreatedOn</param>
        ''' <param name="silent">no messages</param>
        ''' <returns>true if successfull and written, false if error or no changes</returns>
        ''' <remarks></remarks>
        Public Overrides Function PersistRecord(ByRef record As ormRecord, _
                                                Optional timestamp As Date = ot.ConstNullDate, _
                                                Optional ByVal silent As Boolean = False) As Boolean _
        Implements iormDataStore.PersistRecord

            ' check if schema is initialized
            If Not Me.TableSchema.IsInitialized Then
                Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", messagetype:=otCoreMessageType.InternalError, _
                                      message:="tableschema couldnot be initialized - loaded to fail ?", tablename:=Me.TableID)
                Return False
            End If

            '*** check on Property Cached
            If (Me.HasProperty(ConstTPNCacheProperty) AndAlso Me.IsCacheInitialized) OrElse _
                (Me.HasProperty(ConstTPNCacheProperty) AndAlso Not Me.IsCacheInitialized AndAlso Me.InitializeCache) Then
                Return PersistCache(record, timestamp, silent)
            Else
                Return PersistDirect(record, timestamp, silent)
            End If
        End Function
        ''' <summary>
        ''' persists aRecord to the Cache if aRecord is created or loaded
        ''' </summary>
        ''' <param name="aRecord">clsOTDBRecord</param>
        ''' <param name="aTimestamp">the Timestamp to be used for the ChangedOn or CreatedOn</param>
        ''' <param name="silent">no messages</param>
        ''' <returns>true if successfull and written, false if error or no changes</returns>
        ''' <remarks></remarks>
        Public Function PersistDirect(ByRef record As ormRecord, _
                                      Optional ByVal timestamp As Date = ot.ConstNullDate, _
                                      Optional ByVal silent As Boolean = False) As Boolean


            Dim fieldname As String
            Dim aCVTValue, aValue As Object
            Dim j As Integer
            Dim abostrophNecessary As Boolean
            Dim otdbcn As IDbConnection

            Dim changedRecord As Boolean

            Dim persistCommand As IDbCommand

            ' timestamp
            If timestamp = ConstNullDate Then
                timestamp = Date.Now
            End If

            ' Connection

            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, IDbConnection)
                Else
                    Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", exception:=ex)
                Return Nothing
            End Try

            '*** Try to persist

            Try
                '*** Check if not Status
                If (Not record.IsCreated And Not record.IsLoaded) OrElse record.IsUnknown Then
                    If Not record.CheckStatus Then
                        Return False
                    End If
                End If

                '*** Check which Command to use
                '****
                '**** UPDATE
                If record.IsLoaded Then
                    persistCommand = TryCast(Me.TableSchema, clsADONETTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                                clsADONETTableSchema.CommandType.UpdateType)
                    If persistCommand Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADONETTableStore.persistRecord", arg1:=Me.TableSchema.PrimaryKeyIndexName, _
                                              message:="Update Command is not in store", messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                        Return False
                    End If
                ElseIf record.IsCreated Then
                    persistCommand = TryCast(Me.TableSchema, clsADONETTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                                clsADONETTableSchema.CommandType.InsertType)
                    If persistCommand Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADONETTableStore.persistRecord", arg1:=Me.TableSchema.PrimaryKeyIndexName, _
                                              message:="Update Command is not in store", messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                        Return False
                    End If

                End If

                '*** lock the command and generate the parameters
                SyncLock persistCommand.Connection
                    '**** UPDATE
                    If record.IsLoaded Then

                        'get all fields -> update
                        For j = 1 To Me.TableSchema.NoFields
                            ' get fields
                            fieldname = Me.TableSchema.Getfieldname(j)

                            If Not Me.TableSchema.HasprimaryKeyfieldname(fieldname) Then
                                If fieldname <> ConstFNUpdatedOn And fieldname <> "" And fieldname <> ConstFNCreatedOn Then
                                    aValue = record.GetValue(fieldname)
                                    aCVTValue = Me.Convert2ColumnData(j, aValue, abostrophNecessary)
                                    If Not DBNull.Value.Equals(aCVTValue) And Not IsNothing(aCVTValue) And record.IsValueChanged(j) Then
                                        persistCommand.Parameters.Item("@" & fieldname).Value = aCVTValue
                                        changedRecord = True
                                    End If
                                End If
                            End If
                        Next j
                        '*** set the primary key
                        For j = 0 To (Me.TableSchema.NoPrimaryKeyFields - 1)
                            ' value of key
                            fieldname = Me.TableSchema.GetPrimaryKeyfieldname(j + 1)
                            aValue = record.GetValue(fieldname)
                            If fieldname <> "" Then
                                aCVTValue = Me.Convert2ColumnData(fieldname, aValue, abostrophNecessary)
                                If Not DBNull.Value.Equals(aCVTValue) Then
                                    persistCommand.Parameters.Item("@" & fieldname).Value = aCVTValue

                                Else
                                    Call CoreMessageHandler(subname:="clsADONETTableStore.PersistRecord", message:="Value for primary key couldnot be converted to ColumnData", _
                                                          arg1:=aValue, messagetype:=otCoreMessageType.InternalError, entryname:=fieldname, tablename:=Me.TableID)
                                    Return False

                                End If
                            End If

                        Next j

                        '*****
                        '***** CREATE INSERT
                    ElseIf record.IsCreated Then
                        'get all fields -> update
                        For j = 1 To Me.TableSchema.NoFields
                            ' get fields
                            fieldname = Me.TableSchema.Getfieldname(j)
                            If fieldname <> ConstFNUpdatedOn And fieldname <> "" And fieldname <> ConstFNCreatedOn Then
                                aValue = record.GetValue(fieldname)
                                aCVTValue = Me.Convert2ColumnData(j, aValue, abostrophNecessary)
                                If Not DBNull.Value.Equals(aCVTValue) And Not IsNothing(aCVTValue) And record.IsValueChanged(j) Then
                                    persistCommand.Parameters.Item("@" & fieldname).Value = aCVTValue
                                    changedRecord = True
                                End If
                            End If

                        Next j
                    Else

                        Call CoreMessageHandler(subname:="clsADONETTableStore.persistRecord", arg1:=Me.TableSchema.PrimaryKeyIndexName, _
                                              message:="record is nor loaded or created", messagetype:=otCoreMessageType.InternalError, _
                                              tablename:=Me.TableID)
                        Return False
                    End If


                    ' Update the record
                    If changedRecord Then

                        '**** UpdateTimeStamp
                        If Me.TableSchema.GetFieldordinal(ConstFNUpdatedOn) > 0 Then
                            'rst.Fields(OTDBConst_UpdateOn).Value = aTimestamp
                            persistCommand.Parameters.Item("@" & ConstFNUpdatedOn).Value = timestamp
                        End If

                        '*** Create Timestamp
                        If Me.TableSchema.GetFieldordinal(ConstFNCreatedOn) > 0 And record.IsCreated Then
                            persistCommand.Parameters.Item("@" & ConstFNCreatedOn).Value = timestamp
                        ElseIf Me.TableSchema.GetFieldordinal(ConstFNCreatedOn) > 0 And Not record.IsCreated Then
                            If Not DBNull.Value.Equals(record.GetValue(ConstFNCreatedOn)) And Not record.GetValue(ConstFNCreatedOn) Is Nothing Then
                                persistCommand.Parameters.Item("@" & ConstFNCreatedOn).Value = record.GetValue(ConstFNCreatedOn)    'keep the value
                            ElseIf Me.TableSchema.GetFieldordinal(ConstFNUpdatedOn) > 0 AndAlso _
                            Not DBNull.Value.Equals(record.GetValue(ConstFNUpdatedOn)) _
                            AndAlso Not record.GetValue(ConstFNUpdatedOn) Is Nothing Then
                                persistCommand.Parameters.Item("@" & ConstFNCreatedOn).Value = record.GetValue(ConstFNUpdatedOn)    'keep the value
                            Else
                                persistCommand.Parameters.Item("@" & ConstFNCreatedOn).Value = timestamp
                            End If
                        End If

                        '*** really update now
                        persistCommand.ExecuteNonQuery()
                        Return True
                    Else
                        Return True 'always true if no error
                    End If

                End SyncLock

                Exit Function
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, exception:=ex, subname:="clsADONETTableStore.persistRecord", tablename:=Me.TableID, _
                                      messagetype:=otCoreMessageType.InternalException)
                Return False
            End Try



        End Function
    End Class

End Namespace