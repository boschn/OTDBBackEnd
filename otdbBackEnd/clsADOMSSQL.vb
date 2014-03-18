REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Driver Wrapper for ADO.NET MS SQL Classes for On Track Database Backend Library
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
Imports System.Data.Sql
Imports System.Data.SqlClient
Imports Microsoft.SqlServer.Management.Smo
Imports Microsoft.SqlServer.Management.Common
Imports System.Text


Imports OnTrack

Namespace OnTrack.Database

    ''' <summary>
    ''' SQL Server OnTrack Database Driver
    ''' </summary>
    ''' <remarks></remarks>
    Public Class mssqlDBDriver
        Inherits adonetDBDriver
        Implements iormDatabaseDriver

        Protected Shadows WithEvents _primaryConnection As mssqlConnection '-> in clsOTDBDriver
        Private Shadows _ParametersTableAdapter As New SqlDataAdapter
        Shadows Event RequestBootstrapInstall(sender As Object, e As SessionBootstrapEventArgs) Implements iormDatabaseDriver.RequestBootstrapInstall

        Private _internallock As New Object 'internal lock
        Private _parameterlock As New Object 'internal lock

        ''' <summary>
        ''' 
        ''' 
        ''' Constructor
        ''' </summary>
        ''' <param name="ID">an ID for this driver</param>
        ''' <remarks></remarks>
        Public Sub New(ID As String, ByRef session As Session)
            Call MyBase.New(ID, session)
            Me.ID = ID
            If Me._primaryConnection Is Nothing Then
                _primaryConnection = New mssqlConnection(id:="primary", DatabaseDriver:=Me, session:=session, sequence:=ConfigSequence.primary)
            End If
        End Sub


        ''' <summary>
        ''' NativeConnection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads ReadOnly Property NativeConnection() As SqlConnection
            Get
                Return DirectCast(_primaryConnection.NativeConnection, SqlConnection)
            End Get

        End Property
        ''' <summary>
        ''' build Adapter for parameter table
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function BuildParameterAdapter()

            With _ParametersTableAdapter


                .SelectCommand.Prepare()

                ' Create the commands.
                '**** INSERT
                .InsertCommand = New SqlCommand( _
                "INSERT INTO " & _parametersTableName & " (ID, [Value], changedOn, description) " & _
                "VALUES (@ID , @Value , @changedOn , @description)")
                ' Create the parameters.
                .InsertCommand.Parameters.Add( _
                "@ID", SqlDbType.Char, 50, "ID")
                .InsertCommand.Parameters.Add( _
                "@Value", SqlDbType.VarChar, 250, "Value")
                .InsertCommand.Parameters.Add( _
                "@changedOn", SqlDbType.VarChar, 50, "changedOn")
                .InsertCommand.Parameters.Add( _
                "@description", SqlDbType.VarChar, 250, "description")
                .InsertCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, SqlConnection)
                .InsertCommand.Prepare()


                '**** UPDATE
                .UpdateCommand = New SqlCommand( _
                "UPDATE " & _parametersTableName & " SET [Value] = @value , changedOn = @changedOn , description = @description  " & _
                "WHERE ID = @ID")
                ' Create the parameters.
                .UpdateCommand.Parameters.Add( _
                "@Value", SqlDbType.VarChar, 250, "Value")
                .UpdateCommand.Parameters.Add( _
                "@changedOn", SqlDbType.VarChar, 50, "changedOn")
                .UpdateCommand.Parameters.Add( _
                "@description", SqlDbType.VarChar, 250, "description")
                .UpdateCommand.Parameters.Add( _
                "@ID", SqlDbType.Char, 50, "ID").SourceVersion = _
                    DataRowVersion.Original
                .UpdateCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, SqlConnection)
                .UpdateCommand.Prepare()


                '***** DELETE
                .DeleteCommand = New SqlCommand( _
                "DELETE FROM " & _parametersTableName & " WHERE ID = @ID")
                .DeleteCommand.Parameters.Add( _
                "@ID", SqlDbType.Char, 50, "ID").SourceVersion = _
                    DataRowVersion.Original
                .DeleteCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, SqlConnection)
                .DeleteCommand.Prepare()

            End With

        End Function
        ''' <summary>
        ''' initialize driver
        ''' </summary>
        ''' <param name="Force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function Initialize(Optional force As Boolean = False) As Boolean

            If Me.IsInitialized And Not force Then
                Return True
            End If

            Try
                Call MyBase.Initialize(Force:=force)

                ' we have no Connection ?!
                If _primaryConnection Is Nothing Then
                    _primaryConnection = New mssqlConnection("primary", Me, _session, ConfigSequence.primary)
                End If

                '*** do we have the Table ?! - donot do this in bootstrapping since we are running in recursion then
                If Not Me.HasTable(_parametersTableName) And Not _session.IsBootstrappingInstallationRequested Then
                    If Not VerifyOnTrackDatabase(install:=False) Then
                        '* now in bootstrap ?!
                        If _session.IsBootstrappingInstallationRequested Then
                            CoreMessageHandler(message:="verifying the database failed moved to bootstrapping - caching parameters meanwhile", subname:="mssqlDBDriver.Initialize", _
                                          messagetype:=otCoreMessageType.InternalWarning, arg1:=Me.ID)
                            Me.IsInitialized = True
                            Return True
                        Else
                            CoreMessageHandler(message:="verifying the database failed - failed to initialize driver", subname:="mssqlDBDriver.Initialize", _
                                              messagetype:=otCoreMessageType.InternalError, arg1:=Me.ID)
                            Me.IsInitialized = False
                            Return False
                        End If
                    End If
                End If

                '*** end of bootstrapping conditions reinitialize automatically
                '*** verifyOnTrackDatabase might set bootstrapping mode
                If Not _session.IsBootstrappingInstallationRequested OrElse force Then
                    '*** set the DataTable
                    If _OnTrackDataSet Is Nothing Then _OnTrackDataSet = New DataSet(Me.ID & Date.Now.ToString)

                    '** create adapaters
                    If Me.HasTable(_parametersTableName) Then
                        ' the command
                        Dim aDBCommand = New SqlCommand()
                        aDBCommand.CommandText = "select ID, [Value], changedOn, description from " & _parametersTableName
                        aDBCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, SqlConnection)
                        ' fill with adapter
                        _ParametersTableAdapter = New SqlDataAdapter()
                        _ParametersTableAdapter.SelectCommand = aDBCommand
                        _ParametersTableAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                        _ParametersTableAdapter.FillSchema(_OnTrackDataSet, SchemaType.Source)
                        _ParametersTableAdapter.Fill(_OnTrackDataSet, _parametersTableName)
                        ' build Commands
                        Call BuildParameterAdapter()
                        ' set the Table
                        _ParametersTable = _OnTrackDataSet.Tables(_parametersTableName)

                        '** save the cache
                        If _BootStrapParameterCache.Count > 0 Then
                            For Each kvp As KeyValuePair(Of String, Object) In _BootStrapParameterCache
                                SetDBParameter(parametername:=kvp.Key, Value:=kvp.Value, silent:=True)
                            Next
                            _BootStrapParameterCache.Clear()
                        End If
                    Else
                        '** important to recognize where to write data
                        _ParametersTable = Nothing
                    End If

                End If


                Me.IsInitialized = True
                Return True
            Catch ex As Exception
                Me.IsInitialized = False
                Call CoreMessageHandler(subname:="mssqlDBDriver.OnConnection", message:="couldnot Initialize Driver", _
                                      exception:=ex)
                Me.IsInitialized = False
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Gets the type.
        ''' </summary>
        ''' <value>The type.</value>
        Public Overrides ReadOnly Property Type() As otDbDriverType
            Get
                Return otDbDriverType.ADONETSQL
            End Get
        End Property
        ''' <summary>
        ''' create a new TableStore for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNativeTableStore(ByVal TableID As String, ByVal forceSchemaReload As Boolean) As iormDataStore
            Return New mssqlTableStore(Me.CurrentConnection, TableID, forceSchemaReload)
        End Function
        ''' <summary>
        ''' create a new TableSchema for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNativeTableSchema(ByVal TableID As String) As iotDataSchema
            Return New mssqlTableSchema(Me.CurrentConnection, TableID)
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CreateNativeDBCommand(commandstr As String, nativeConnection As IDbConnection) As IDbCommand Implements iormDatabaseDriver.CreateNativeDBCommand
            Return New SqlCommand(commandstr, nativeConnection)
        End Function
        ''' <summary>
        '''  raise the RequestBootStrapInstall Event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub RaiseRequestBootstrapInstall(sender As Object, ByRef e As EventArgs)
            RaiseEvent RequestBootstrapInstall(sender, e)
        End Sub
        ''' <summary>
        ''' converts data to a specific native database type
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="targetType"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Convert2DBData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "", _
                                                    Optional isnullable As Boolean = False, _
                                                    Optional defaultvalue As Object = Nothing) As Boolean Implements iormDatabaseDriver.Convert2DBData
            Dim result As Object = Nothing
            Try
                
                '*** array conversion should not occure on this level
                If IsArray(invalue) Then
                    invalue = Converter.Array2String(invalue)
                End If

                If targetType = SqlDataType.BigInt OrElse targetType = SqlDataType.Int _
                OrElse targetType = SqlDataType.SmallInt OrElse targetType = SqlDataType.TinyInt Then

                    If defaultvalue Is Nothing Then defaultvalue = 0

                    If isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                        OrElse DBNull.Value.Equals(invalue)) Then
                        result = DBNull.Value
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                        OrElse IsError(invalue) OrElse DBNull.Value.Equals(invalue)) Then
                        result = Convert.ToUInt64(defaultvalue)
                    ElseIf IsNumeric(invalue) Then
                        result = Convert.ToUInt64(invalue)
                    Else
                        Call CoreMessageHandler(subname:="mssqlDBDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & invalue & " is not convertible to Long", _
                                              arg1:=invalue, messagetype:=otCoreMessageType.InternalError)
                        outvalue = Nothing
                        Return False
                    End If

                ElseIf targetType = SqlDataType.Char OrElse targetType = SqlDataType.NText _
                    OrElse targetType = SqlDataType.VarChar OrElse targetType = SqlDataType.Text _
                     OrElse targetType = SqlDataType.NVarChar OrElse targetType = SqlDataType.VarCharMax _
                     OrElse targetType = SqlDataType.NVarCharMax Then

                    abostrophNecessary = True
                    If defaultvalue Is Nothing Then defaultvalue = ""

                    If isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue)) Then
                        result = DBNull.Value
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue) OrElse _
                                               DBNull.Value.Equals(invalue)) Then
                        result = Convert.ToString(defaultvalue)
                    Else
                        If maxsize < Len(CStr(invalue)) And maxsize > 1 Then
                            result = Mid(Convert.ToString(invalue), 0, maxsize - 1)
                        Else
                            result = Convert.ToString(invalue)
                        End If
                    End If

                ElseIf targetType = SqlDataType.Date OrElse targetType = SqlDataType.SmallDateTime OrElse targetType = SqlDataType.Time _
                OrElse targetType = SqlDataType.Timestamp OrElse targetType = SqlDataType.DateTime OrElse targetType = SqlDataType.DateTime2 _
                OrElse targetType = SqlDataType.DateTimeOffset Then

                    If defaultvalue Is Nothing Then defaultvalue = ConstNullDate

                    If isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) OrElse _
                         DBNull.Value.Equals(invalue)) Then
                        result = DBNull.Value
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) OrElse _
                         DBNull.Value.Equals(invalue)) Then
                        result = Convert.ToDateTime(defaultvalue)
                    ElseIf IsDate(invalue) Then
                        result = Convert.ToDateTime(invalue)
                    ElseIf invalue.GetType = GetType(TimeSpan) Then
                        result = invalue
                    Else
                        Call CoreMessageHandler(subname:="mssqlDBDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & invalue & " is not convertible to Date", _
                                              arg1:=invalue, messagetype:=otCoreMessageType.InternalError)
                        outvalue = Nothing
                        Return False
                    End If

                ElseIf targetType = SqlDataType.Float OrElse targetType = SqlDataType.Decimal _
                OrElse targetType = SqlDataType.Real Then

                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToDouble(0)

                    If isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                        OrElse DBNull.Value.Equals(invalue)) Then
                        result = DBNull.Value
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                        OrElse DBNull.Value.Equals(invalue)) Then
                        result = defaultvalue
                    ElseIf IsNumeric(invalue) Then
                        result = Convert.ToDouble(invalue)
                    Else
                        Call CoreMessageHandler(subname:="mssqlDBDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & invalue & " is not convertible to Double", _
                                              arg1:=targetType, messagetype:=otCoreMessageType.InternalError)
                        outvalue = Nothing
                        Return False
                    End If

                ElseIf targetType = SqlDataType.Bit Then

                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToBoolean(False)

                    If isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                         OrElse (IsNumeric(invalue) AndAlso invalue = 0)) Then
                        result = DBNull.Value
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                         OrElse (IsNumeric(invalue) AndAlso invalue = 0)) Then
                        result = defaultvalue
                    ElseIf TypeOf (invalue) Is Boolean Then
                        result = Convert.ToBoolean(invalue)
                    Else
                        result = True
                    End If

                End If

                ' return
                outvalue = result
                Return True

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", subname:="mssqlDBDriver.convert2ColumnData(Object, long ..", _
                                       exception:=ex, messagetype:=otCoreMessageType.InternalException)
                outvalue = Nothing
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns the target type for a OTDB FieldType - MAPPING
        ''' </summary>
        ''' <param name="type"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDatabaseDriver.GetTargetTypeFor

            Try
                '** returns SQLDataType which is SMO DataType and not SQLDbtype for ADONET !

                Select Case type
                    Case otFieldDataType.Binary
                        Return SqlDataType.Binary
                    Case otFieldDataType.Bool
                        Return SqlDataType.Bit
                    Case otFieldDataType.[Date]
                        Return SqlDataType.Date
                    Case otFieldDataType.[Time]
                        Return SqlDataType.Time
                    Case otFieldDataType.List
                        Return SqlDataType.NVarChar
                    Case otFieldDataType.[Long]
                        Return SqlDataType.BigInt
                    Case otFieldDataType.Memo
                        Return SqlDataType.NVarChar
                    Case otFieldDataType.Numeric
                        Return SqlDataType.Decimal
                    Case otFieldDataType.Timestamp
                        Return SqlDataType.DateTime
                    Case otFieldDataType.Text
                        Return SqlDataType.NVarChar
                    Case Else

                        Call CoreMessageHandler(subname:="mssqlDBDriver.GetTargetTypefor", message:="Type not defined",
                                       messagetype:=otCoreMessageType.InternalException)
                End Select

            Catch ex As Exception
                Call CoreMessageHandler(subname:="mssqlDBDriver.GetTargetTypefor", message:="Exception", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalException)
                Return 0
            End Try

        End Function
        ''' <summary>
        ''' create an assigned Native DBParameter to provided name and type
        ''' </summary>
        ''' <param name="parametername">name of parameter</param>
        ''' <param name="datatype">otdb datatype</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function AssignNativeDBParameter(parametername As String, _
                                                          datatype As otFieldDataType, _
                                                           Optional maxsize As Long = 0, _
                                                          Optional value As Object = Nothing) As System.Data.IDbDataParameter _
                                                      Implements iormDatabaseDriver.AssignNativeDBParameter


            Try
                Dim aParameter As New SqlParameter()

                aParameter.ParameterName = parametername

                Select Case datatype
                    Case otFieldDataType.Binary
                        aParameter.SqlDbType = SqlDbType.Binary
                    Case otFieldDataType.Bool
                        aParameter.SqlDbType = SqlDbType.Bit
                    Case otFieldDataType.[Date]
                        aParameter.SqlDbType = SqlDbType.Date
                    Case otFieldDataType.[Time]
                        aParameter.SqlDbType = SqlDbType.Time
                    Case otFieldDataType.List
                        aParameter.SqlDbType = SqlDbType.NVarChar
                    Case otFieldDataType.[Long]
                        aParameter.SqlDbType = SqlDbType.BigInt
                    Case otFieldDataType.Memo
                        aParameter.SqlDbType = SqlDbType.NVarChar
                    Case otFieldDataType.Numeric
                        aParameter.SqlDbType = SqlDbType.Decimal
                    Case otFieldDataType.Timestamp
                        aParameter.SqlDbType = SqlDbType.DateTime
                    Case otFieldDataType.Text
                        aParameter.SqlDbType = SqlDbType.NVarChar
                    Case Else

                        Call CoreMessageHandler(subname:="mssqlDBDriver.AssignNativeDBParameter", message:="Type not defined",
                                       messagetype:=otCoreMessageType.InternalException)
                End Select

                Select Case datatype
                    Case otFieldDataType.Bool
                        aParameter.SqlValue = False
                    Case otFieldDataType.[Date]
                        aParameter.SqlValue = ConstNullDate
                    Case otFieldDataType.[Time]
                        If maxsize = 0 Then aParameter.Size = 7
                        aParameter.SqlValue = ot.ConstNullTime
                    Case otFieldDataType.List
                        If maxsize = 0 Then aParameter.Size = Const_MaxTextSize
                        aParameter.SqlValue = ""
                    Case otFieldDataType.[Long]
                        aParameter.SqlValue = 0
                    Case otFieldDataType.Memo
                        If maxsize = 0 Then aParameter.Size = Const_MaxMemoSize
                        aParameter.SqlValue = ""
                    Case otFieldDataType.Numeric
                        aParameter.SqlValue = 0
                    Case otFieldDataType.Timestamp
                        aParameter.SqlValue = ConstNullDate
                    Case otFieldDataType.Text
                        If maxsize = 0 Then aParameter.Size = Const_MaxTextSize
                        aParameter.SqlValue = ""

                End Select
                If Not value Is Nothing Then
                    aParameter.SqlValue = value
                End If
                Return aParameter
            Catch ex As Exception
                Call CoreMessageHandler(subname:="mssqlDBDriver.assignDBParameter", message:="Exception", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalException)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Gets the catalog.
        ''' </summary>
        ''' <param name="FORCE">The FORCE.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <returns></returns>
        Public Overrides Function GetCatalog(Optional FORCE As Boolean = False, Optional ByRef connection As iormConnection = Nothing) As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function
        ''' <summary>
        ''' True if table ID exists in data store
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasTable(tableID As String, _
                                           Optional ByRef connection As iormConnection = Nothing, _
                                           Optional nativeConnection As Object = Nothing) As Boolean

            Dim myconnection As mssqlConnection
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Dim myNativeConnection As SqlConnection

            '* if already loaded
            If _TableDirectory.ContainsKey(key:=tableID) Then Return True

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If
            If nativeConnection Is Nothing Then
                myNativeConnection = TryCast(myconnection.NativeInternalConnection, SqlConnection)
            Else
                myNativeConnection = TryCast(nativeConnection, SqlConnection)
            End If
            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.hasTable", _
                                            messagetype:=otCoreMessageType.InternalError, tablename:=tableID)
                Return Nothing
            End If
            If myNativeConnection Is Nothing Then
                Call CoreMessageHandler(subname:="mssqlDBDriver.HasTable", message:="No current internal Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights - avoid recursion if we are looking for the User Table
            '** makes no sense since we are checkin before installation if we need to install
            'If Not CurrentSession.IsBootstrappingInstallation AndAlso tableID <> User.ConstTableID Then
            '    If Not _currentUserValidation.ValidEntry AndAlso Not _currentUserValidation.HasReadRights Then
            '        If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], loginOnFailed:=True) Then
            '            Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.HasTable", tablename:=tableID, _
            '                                  message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
            '            Return False
            '        End If
            '    End If
            'End If


            Try
                SyncLock _internallock
                    smoconnection = myconnection.SMOConnection ' will be setup during internal connection
                    database = myconnection.Database

                    If smoconnection Is Nothing OrElse database Is Nothing Then
                        Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                              subname:="mssqlDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If

                    database.Tables.Refresh()
                    Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)
                    Return existsOnServer
                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, tablename:=tableID, _
                                      subname:="mssqlDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableID, _
                                      subname:="mssqlDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Gets the table object.
        ''' </summary>
        ''' <param name="tablename">The tablename.</param>
        ''' <param name="createOrAlter">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <returns></returns>
        Public Overrides Function GetTable(tableID As String, _
                                           Optional createOrAlter As Boolean = False, _
                                           Optional ByRef connection As iormConnection = Nothing, _
                                           Optional ByRef nativeTableObject As Object = Nothing) As Object

            Dim aTable As Table
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Dim localCreated As Boolean = False
            Dim myconnection As mssqlConnection

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If
            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.GetTable", _
                                            messagetype:=otCoreMessageType.InternalError, tablename:=tableID)
                Return Nothing
            End If

            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetTable", tablename:=tableID, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If



            Try
                SyncLock _internallock
                    smoconnection = myconnection.SMOConnection
                    database = myconnection.Database

                    If smoconnection Is Nothing OrElse database Is Nothing Then
                        Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                              subname:="mssqlDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If

                    database.Tables.Refresh()
                    Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)

                    '*** Exists and nothing supplied -> get it
                    If existsOnServer And (nativeTableObject Is Nothing OrElse nativeTableObject.GetType <> GetType(Table)) Then
                        aTable = database.Tables(tableID)
                        aTable.Refresh()
                        Return aTable

                        '*** Doesnot Exist, create and nothing supplied -> createLocal Object
                    ElseIf Not existsOnServer And createOrAlter And (nativeTableObject Is Nothing OrElse nativeTableObject.GetType <> GetType(Table)) Then
                        aTable = New Table(database, name:=tableID)
                        localCreated = True
                    Else
                        aTable = nativeTableObject
                    End If

                    '*** No CreateAlter -> return the Object
                    '*** CreatorAlter but the Object exists and was localCreated (means no object transmitted for change)
                    '*** return the refreshed

                    If (Not createOrAlter Or localCreated) AndAlso existsOnServer Then
                        If Not aTable Is Nothing Then aTable.Refresh()
                        Return aTable

                        '** doesnot Exist 
                    ElseIf (Not createOrAlter And Not existsOnServer) Then
                        Call CoreMessageHandler(subname:="mssqlDBDriver.gettable", message:="Table does not exist", messagetype:=otCoreMessageType.InternalWarning, _
                                               break:=False, tablename:=tableID, arg1:=tableID)
                        Return Nothing
                    End If

                    '** create the table
                    '**
                    If createOrAlter Then
                        If Not localCreated And Not myconnection.Database.Tables.Contains(name:=tableID) Then
                            aTable.Create()
                        ElseIf myconnection.Database.Tables.Contains(name:=tableID) Then
                            aTable.Alter()
                        End If

                        Return aTable
                    Else
                        Call CoreMessageHandler(subname:="mssqlDBDriver.getTable", tablename:=tableID, _
                                              message:="Table was not found in database", messagetype:=otCoreMessageType.ApplicationWarning)
                        Return Nothing
                    End If
                End SyncLock

                Return aTable

            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, tablename:=tableID, _
                                      subname:="mssqlDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableID, _
                                      subname:="mssqlDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function

       
        ''' <summary>
        ''' Gets the index.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="indexname">The indexname.</param>
        ''' <param name="ColumnNames">The column names.</param>
        ''' <param name="PrimaryKey">The primary key.</param>
        ''' <param name="forceCreation">The force creation.</param>
        ''' <param name="createOrAlter">The create on missing.</param>
        ''' <returns></returns>
        Public Overrides Function GetIndex(ByRef nativeTable As Object, _
                                           ByRef indexdefinition As IndexDefinition, _
                                            Optional ByVal forceCreation As Boolean = False, _
                                            Optional ByVal createOrAlter As Boolean = False, _
                                             Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetIndex


            Dim aTable As Table = DirectCast(nativeTable, Table)
            Dim myconnection As mssqlConnection
            Dim existingIndex As Boolean = False
            Dim indexnotchanged As Boolean = False
            Dim aIndexColumn As IndexedColumn
            Dim existPrimaryName As String = ""
            Dim anIndex As Index
            Dim i As UShort = 0

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If
            '*** object
            If Not nativeTable.GetType = GetType(Table) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.getIndex", _
                                             message:="No SMO TableObject given to function", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.getIndex", _
                                            messagetype:=otCoreMessageType.InternalError, arg1:=indexdefinition.Name, tablename:=aTable.Name)
                Return Nothing
            End If

            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not connection.VerifyUserAccess(otAccessRight.AlterSchema) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.getIndex", arg1:=indexdefinition.Name, tablename:=aTable.Name, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If


            Try

                SyncLock _internallock
                    '**
                    If aTable.Indexes.Count = 0 Then aTable.Refresh()

                    ' save the primary name
                    For Each index As Index In aTable.Indexes
                        If LCase(index.Name) = LCase(indexdefinition.Name) Or LCase(index.Name) = LCase(aTable.Name & "_" & indexdefinition.Name) Then
                            existingIndex = True
                            anIndex = index
                        End If
                        If index.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                            existPrimaryName = index.Name
                            If indexdefinition.Name = "" Then
                                indexdefinition.DatabaseID = index.Name
                                existingIndex = True
                                anIndex = index
                            End If
                        End If
                    Next

                    '** check on changes
                    If (aTable.Indexes.Contains(name:=LCase(indexdefinition.Name)) OrElse _
                        aTable.Indexes.Contains(name:=LCase(aTable.Name & "_" & indexdefinition.Name))) _
                        And Not forceCreation Then

                        If aTable.Indexes.Contains(name:=LCase(indexdefinition.Name)) Then
                            anIndex = aTable.Indexes(name:=LCase(indexdefinition.Name))
                        Else
                            anIndex = aTable.Indexes(name:=LCase(aTable.Name & "_" & indexdefinition.Name))
                        End If
                        ' check all Members
                        If Not forceCreation And existingIndex Then
                            i = 0
                            For Each columnName As String In indexdefinition.Columnnames
                                ' check
                                If Not IsNothing(columnName) Then

                                    ' not equal
                                    aIndexColumn = anIndex.IndexedColumns(i)
                                    If LCase(aIndexColumn.Name) <> LCase(columnName) Then
                                        indexnotchanged = False
                                        Exit For
                                    Else
                                        indexnotchanged = True
                                    End If
                                End If
                                ' exit
                                If Not indexnotchanged Then
                                    Exit For
                                End If
                                i = i + 1
                            Next columnName
                            ' return
                            If indexnotchanged Then
                                Return anIndex
                            End If
                        End If

                        '** exit
                    ElseIf Not createOrAlter Then

                        Call CoreMessageHandler(message:="index does not exist", subname:="mssqlDBDriver.getIndex", arg1:=indexdefinition.Name, _
                                               tablename:=aTable.Name, messagetype:=otCoreMessageType.InternalError)

                        Return Nothing

                    End If

                    '** create
                    myconnection.IsNativeInternalLocked = True

                    ' if we have another Primary
                    If indexdefinition.IsPrimary And LCase(indexdefinition.Name) <> LCase(existPrimaryName) And existPrimaryName <> "" Then
                        'indexdefinition.Name is found and not the same ?!
                        Call CoreMessageHandler(message:="indexdefinition.Name of table " & aTable.Name & " is " & anIndex.Name & " and not " & indexdefinition.Name & " - getOTDBIndex aborted", _
                                              messagetype:=otCoreMessageType.InternalError, subname:="mssqlDBDriver.getIndex", arg1:=indexdefinition.Name, tablename:=aTable.Name)
                        Return Nothing
                        ' create primary key
                    ElseIf indexdefinition.IsPrimary And existPrimaryName = "" Then
                        'create primary
                        If indexdefinition.DatabaseID = "" Then
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_primarykey")
                        Else
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_" & indexdefinition.Name)
                        End If
                        anIndex = New Index(parent:=aTable, name:=indexdefinition.DatabaseID)
                        anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey
                        anIndex.IndexType = IndexType.NonClusteredIndex
                        anIndex.IgnoreDuplicateKeys = False
                        anIndex.IsUnique = True

                        '** extend indexdefinition.isprimary
                    ElseIf indexdefinition.IsPrimary And LCase(indexdefinition.Name) = LCase(existPrimaryName) Then
                        '* DROP !
                        anIndex.Drop()

                        '* create
                        If indexdefinition.DatabaseID = "" Then
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_primarykey")
                        Else
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_" & indexdefinition.Name)
                        End If
                        anIndex = New Index(parent:=aTable, name:=indexdefinition.DatabaseID)
                        anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey
                        anIndex.IndexType = IndexType.NonClusteredIndex
                        anIndex.IgnoreDuplicateKeys = False
                        anIndex.IsUnique = True
                        'anIndex.Recreate()

                        '** extend Index -> Drop
                    ElseIf Not indexdefinition.IsPrimary And existingIndex Then
                        anIndex.Drop()
                        If indexdefinition.DatabaseID = "" Then
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_IND")
                        Else
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_" & indexdefinition.DatabaseID)
                        End If
                        anIndex = New Index(parent:=aTable, name:=indexdefinition.Name)
                        anIndex.Name = indexdefinition.Name
                        anIndex.IndexKeyType = IndexKeyType.None
                        anIndex.IgnoreDuplicateKeys = Not indexdefinition.IsUnique
                        anIndex.IsUnique = indexdefinition.IsUnique
                        '** create new
                    ElseIf Not indexdefinition.IsPrimary And Not existingIndex Then
                        If indexdefinition.DatabaseID = "" Then
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_IND")
                        Else
                            indexdefinition.DatabaseID = LCase(aTable.Name & "_" & indexdefinition.DatabaseID)
                        End If
                        anIndex = New Index(parent:=aTable, name:=indexdefinition.Name)
                        anIndex.Name = indexdefinition.Name
                        anIndex.IndexKeyType = IndexKeyType.None
                        anIndex.IgnoreDuplicateKeys = Not indexdefinition.IsUnique
                        anIndex.IsUnique = indexdefinition.IsUnique
                    End If

                    ' check on keys & indexes
                    For Each aColumnname As String In indexdefinition.Columnnames
                        Dim indexColumn As IndexedColumn = New IndexedColumn(anIndex, aColumnname)
                        anIndex.IndexedColumns.Add(indexColumn)
                    Next

                    ' attach the Index
                    If Not anIndex Is Nothing Then
                        anIndex.Create()
                        Return anIndex
                    Else
                        Return Nothing
                    End If

                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, tablename:=aTable.Name, arg1:=indexdefinition.Name, _
                                      subname:="mssqlDBDriver.GetIndex", messagetype:=otCoreMessageType.InternalError)
                myconnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetIndex", arg1:=indexdefinition.Name, tablename:=aTable.Name, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                myconnection.IsNativeInternalLocked = False
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' returns True if table Id has columnname in datastore
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasColumn(tableID As String, columnname As String, Optional ByRef connection As iormConnection = Nothing) As Boolean
            Dim aTable As Table
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Dim myconnection As mssqlConnection

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If

            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.HasColumn", _
                                            messagetype:=otCoreMessageType.InternalError, tablename:=tableID, arg1:=columnname)
                Return Nothing
            End If

            '*** check on rights
            '*** makes no sense
            'If Not CurrentSession.IsBootstrappingInstallation Then
            '    If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.ReadOnly, loginOnFailed:=True) Then
            '        Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.hasColumn", _
            '                              message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
            '        Return Nothing
            '    End If
            'End If



            Try
                myconnection.IsNativeInternalLocked = True
                SyncLock _internallock

                    smoconnection = myconnection.SMOConnection
                    database = myconnection.Database

                    If smoconnection Is Nothing OrElse database Is Nothing Then
                        Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                              subname:="mssqlDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else
                        myconnection.IsNativeInternalLocked = True
                    End If

                    database.Tables.Refresh()
                    Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)
                    If Not existsOnServer Then
                        Return False
                    End If
                    aTable = database.Tables.Item(tableID)


                    '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    If aTable.Columns.Contains(name:=columnname) Then
                        Return True
                    Else
                        Return False
                    End If

                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, entryname:=columnname, tablename:=tableID, _
                                      subname:="mssqlDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.hasColumn", entryname:=columnname, tablename:=tableID, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' returns True if table Id has column name in data store
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function VerifyColumnSchema(columndefinition As ColumnDefinition, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean
            Dim aTable As Table
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Dim myconnection As mssqlConnection
            Dim tableid As String = columndefinition.Tablename
            Dim columnname As String = columndefinition.Name

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If

            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.HasColumn", _
                                            messagetype:=otCoreMessageType.InternalError, tablename:=tableid, arg1:=columnname)
                Return Nothing
            End If

            '*** check on rights
            '** do not session since we might checking this to get bootstrapping status before session is started
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], useLoginWindow:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.hasColumn", _
                                          message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If



            Try
                myconnection.IsNativeInternalLocked = True
                SyncLock _internallock

                    smoconnection = myconnection.SMOConnection
                    database = myconnection.Database

                    If smoconnection Is Nothing OrElse database Is Nothing Then
                        Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableid, _
                                              subname:="mssqlDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else
                        myconnection.IsNativeInternalLocked = True
                    End If

                    database.Tables.Refresh()
                    Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableid)
                    If Not existsOnServer Then
                        Return False
                    End If
                    aTable = database.Tables.Item(tableid)


                    '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    '** check name
                    If aTable.Columns.Contains(name:=columnname) Then
                        Dim column = aTable.Columns(columnname)
                        '** set standard sizes or other specials
                        Select Case columndefinition.Datatype
                            Case otFieldDataType.[Long]
                                If column.DataType.SqlDataType <> SqlDataType.BigInt Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be Long", arg1:=columndefinition.Datatype, _
                                                   tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If

                            Case otFieldDataType.Numeric
                                If column.DataType.SqlDataType <> SqlDataType.Real Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be REAL", arg1:=columndefinition.Datatype, _
                                                 tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.List, otFieldDataType.Text
                                If column.DataType.SqlDataType <> SqlDataType.NVarChar Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be NVARCHAR", arg1:=columndefinition.Datatype, _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                                If columndefinition.Size > 0 Then
                                    If column.DataType.MaximumLength < columndefinition.Size Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column maximum length differs", arg1:=columndefinition.Size, _
                                               tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)

                                        Return False
                                    End If
                                End If
                            Case otFieldDataType.Memo
                                If column.DataType.SqlDataType <> SqlDataType.NVarCharMax Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be NVARCHARMAX", arg1:=columndefinition.Datatype, _
                                              tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.Binary
                                If column.DataType.SqlDataType <> SqlDataType.VarBinaryMax Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be VARBINARYMAX", arg1:=columndefinition.Datatype, _
                                             tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.[Date]
                                If column.DataType.SqlDataType <> SqlDataType.Date Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be DATE", arg1:=columndefinition.Datatype, _
                                            tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.Time
                                If column.DataType.SqlDataType <> SqlDataType.Time Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be TIME", arg1:=columndefinition.Datatype, _
                                            tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.Timestamp
                                If column.DataType.SqlDataType <> SqlDataType.DateTime Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be DATETIME", arg1:=columndefinition.Datatype, _
                                            tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                            Case otFieldDataType.Bool
                                If column.DataType.SqlDataType <> SqlDataType.Bit Then
                                    If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be BIT", arg1:=columndefinition.Datatype, _
                                            tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                    Return False
                                End If
                        End Select


                        Return True
                    Else
                        If Not silent Then CoreMessageHandler(message:="verifying table column: column does not exist in database ", _
                                                  tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)


                        Return False
                    End If

                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, entryname:=columnname, tablename:=tableid, _
                                      subname:="mssqlDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.hasColumn", entryname:=columnname, tablename:=tableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' returns True if table Id has columnname in datastore
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function VerifyColumnSchema(columnattribute As ormSchemaTableColumnAttribute, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean
            Dim aTable As Table
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Dim myconnection As mssqlConnection
            Dim tableid As String = columnattribute.Tablename
            Dim columnname As String = columnattribute.ColumnName

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If

            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.verifyColumnSchema", _
                                            messagetype:=otCoreMessageType.InternalError, tablename:=tableid, arg1:=columnname)
                Return Nothing
            End If

            '*** check on rights
            '** do not session since we might checking this to get bootstrapping status before session is started
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], useLoginWindow:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.verifyColumnSchema", _
                                      message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If



            Try
                myconnection.IsNativeInternalLocked = True
                SyncLock _internallock

                    smoconnection = myconnection.SMOConnection
                    database = myconnection.Database

                    If smoconnection Is Nothing OrElse database Is Nothing Then
                        Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableid, _
                                              subname:="mssqlDBDriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else
                        myconnection.IsNativeInternalLocked = True
                    End If

                    database.Tables.Refresh()
                    Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableid)
                    If Not existsOnServer Then
                        Return False
                    End If
                    aTable = database.Tables.Item(tableid)


                    '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    '** check name
                    If aTable.Columns.Contains(name:=columnname) Then
                        Dim column = aTable.Columns(columnname)
                        If columnattribute.HasValueTypeID Then
                            '** set standard sizes or other specials
                            Select Case columnattribute.Typeid
                                Case otFieldDataType.[Long]
                                    If column.DataType.SqlDataType <> SqlDataType.BigInt Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be Long", arg1:=columnattribute.Typeid, _
                                                       tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If

                                Case otFieldDataType.Numeric
                                    If column.DataType.SqlDataType <> SqlDataType.Real Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be REAL", arg1:=columnattribute.Typeid, _
                                                     tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.List, otFieldDataType.Text
                                    If column.DataType.SqlDataType <> SqlDataType.NVarChar Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be NVARCHAR", arg1:=columnattribute.Typeid, _
                                                    tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                    If columnattribute.HasValueSize AndAlso columnattribute.Size > 0 Then
                                        If column.DataType.MaximumLength < columnattribute.Size Then
                                            If Not silent Then CoreMessageHandler(message:="verifying table column: column maximum length differs", arg1:=columnattribute.Size, _
                                                   tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)

                                            Return False
                                        End If
                                    End If
                                Case otFieldDataType.Memo
                                    If column.DataType.SqlDataType <> SqlDataType.NVarCharMax Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be NVARCHARMAX", arg1:=columnattribute.Typeid, _
                                                  tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.Binary
                                    If column.DataType.SqlDataType <> SqlDataType.VarBinaryMax Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be VARBINARYMAX", arg1:=columnattribute.Typeid, _
                                                 tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.[Date]
                                    If column.DataType.SqlDataType <> SqlDataType.Date Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be DATE", arg1:=columnattribute.Typeid, _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.Time
                                    If column.DataType.SqlDataType <> SqlDataType.Time Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be TIME", arg1:=columnattribute.Typeid, _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.Timestamp
                                    If column.DataType.SqlDataType <> SqlDataType.DateTime Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be DATETIME", arg1:=columnattribute.Typeid, _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.Bool
                                    If column.DataType.SqlDataType <> SqlDataType.Bit Then
                                        If Not silent Then CoreMessageHandler(message:="verifying table column: column data type differs - should be BIT", arg1:=columnattribute.Typeid, _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                        Return False
                                    End If
                                Case otFieldDataType.Runtime
                                Case otFieldDataType.Formula
                                    If Not silent Then Call CoreMessageHandler(subname:="mssqlDBDriver.verifyColumnSchema", tablename:=aTable.Name, arg1:=columnattribute.ColumnName, _
                                                           message:="runtime, formular not supported as fieldtypes", messagetype:=otCoreMessageType.InternalError)

                            End Select

                        End If

                        Return True
                    Else
                        If Not silent Then CoreMessageHandler(message:="verifying table column: column does not exist in database ", _
                                                tablename:=tableid, columnname:=columnname, subname:="mssqldbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)


                        Return False
                    End If

                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, entryname:=columnname, tablename:=tableid, _
                                      subname:="mssqlDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.hasColumn", entryname:=columnname, tablename:=tableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                myconnection.IsNativeInternalLocked = False
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Gets the column.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="aDBDesc">A DB desc.</param>
        ''' <param name="createOrAlter">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>

        Public Overrides Function GetColumn(nativeTable As Object, columndefinition As ColumnDefinition, _
                                            Optional createOrAlter As Boolean = False, _
                                            Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetColumn
            Dim myconnection As mssqlConnection

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If
            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.GetColumn", _
                                            messagetype:=otCoreMessageType.InternalError, columnname:=columndefinition.Name, tablename:=columndefinition.Tablename)
                Return Nothing
            End If

            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetColumn", columnname:=columndefinition.Name, tablename:=columndefinition.Tablename, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If


            '*** object
            If Not nativeTable.GetType = GetType(Table) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetColumn", columnname:=columndefinition.Name, tablename:=columndefinition.Tablename, _
                                             message:="No SMO TableObject given to function", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Dim aTable As Table = DirectCast(nativeTable, Table)
            Dim newColumn As Column
            Dim aDatatype As New DataType
            Dim addColumn As Boolean = False

            Try

                SyncLock _internallock

                    '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    If aTable.Columns.Contains(name:=columndefinition.Name) And Not createOrAlter Then
                        Return aTable.Columns(name:=columndefinition.Name)
                    ElseIf Not aTable.Columns.Contains(name:=columndefinition.Name) And Not createOrAlter Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetColumn", arg1:=columndefinition.Name, tablename:=aTable.Name, _
                                                    message:="Column does not exist", messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else

                        

                        '**
                        '** create normal database column
                        '**
                        If aTable.Columns.Contains(name:=columndefinition.Name) Then
                            newColumn = aTable.Columns(name:=columndefinition.Name)
                            aDatatype = newColumn.DataType
                        Else
                            newColumn = New Column(parent:=aTable, name:=columndefinition.Name)
                            aDatatype = New DataType
                            addColumn = True
                        End If


                        'aDatatype.SqlDataType = GetTargetTypeFor(columndefinition.Datatype) is not working since we have SMO sqldatatype here and not 
                        'isqltype

                        '** set standard sizes or other specials
                        Select Case columndefinition.Datatype
                            Case otFieldDataType.[Long]
                                aDatatype.SqlDataType = SqlDataType.BigInt
                            Case otFieldDataType.Numeric
                                aDatatype.SqlDataType = SqlDataType.Real

                            Case otFieldDataType.List, otFieldDataType.Text
                                aDatatype.SqlDataType = SqlDataType.NVarChar
                                If columndefinition.Size > 0 Then
                                    aDatatype.MaximumLength = columndefinition.Size
                                Else
                                    aDatatype.MaximumLength = Const_MaxTextSize
                                End If
                            Case otFieldDataType.Memo
                                aDatatype.SqlDataType = SqlDataType.NVarCharMax
                            Case otFieldDataType.Binary
                                aDatatype.SqlDataType = SqlDataType.VarBinaryMax
                            Case otFieldDataType.[Date]
                                aDatatype.SqlDataType = SqlDataType.Date
                            Case otFieldDataType.Time
                                aDatatype.SqlDataType = SqlDataType.Time
                                aDatatype.MaximumLength = 7
                            Case otFieldDataType.Timestamp
                                aDatatype.SqlDataType = SqlDataType.DateTime
                            Case otFieldDataType.Bool
                                aDatatype.SqlDataType = SqlDataType.Bit
                            Case otFieldDataType.Runtime
                            Case otFieldDataType.Formula
                                Call CoreMessageHandler(subname:="mssqlDBDriver.getColumn", tablename:=aTable.Name, arg1:=columndefinition.Name, _
                                                       message:="runtime, formular not supported as fieldtypes", messagetype:=otCoreMessageType.InternalError)
                            Case Else
                                Call CoreMessageHandler(subname:="mssqlDBDriver.getColumn", tablename:=aTable.Name, arg1:=columndefinition.Name, _
                                                      message:="datatype not implemented", messagetype:=otCoreMessageType.InternalError)
                        End Select
                        newColumn.DataType = aDatatype
                        ' default value
                        If columndefinition.DefaultValue IsNot Nothing Then
                            If columndefinition.Datatype = otFieldDataType.Time Then
                                newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = _
                                    "'" & CDate(columndefinition.DefaultValue).ToString("HH:mm:ss") & "'"
                            ElseIf columndefinition.Datatype = otFieldDataType.Date Then
                                newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = _
                                "'" & CDate(columndefinition.DefaultValue).ToString("yyyy-MM-dd") & "T00:00:00Z'"
                            ElseIf columndefinition.Datatype = otFieldDataType.Timestamp Then
                                newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = _
                                    "'" & (Convert.ToDateTime(columndefinition.DefaultValue).ToString("yyyy-MM-ddTHH:mm:ssZ")) & "'"
                            ElseIf columndefinition.Datatype = otFieldDataType.Bool Then
                                If columndefinition.DefaultValue Then
                                    newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = "1"
                                Else
                                    newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = "0"
                                End If
                            ElseIf columndefinition.Datatype = otFieldDataType.Text OrElse columndefinition.Datatype = otFieldDataType.List Then
                                newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = "'" & columndefinition.DefaultValueString & "'"
                            ElseIf columndefinition.DefaultValueString <> "" Then
                                newColumn.AddDefaultConstraint("DEFAULT_" & nativeTable.name & "." & columndefinition.Name).Text = columndefinition.DefaultValueString
                            End If



                        End If

                        ' per default Nullable
                        If aTable.State = SqlSmoState.Creating Then
                            newColumn.Nullable = columndefinition.IsNullable
                            ' SQL Server throws error if not nullable or default value on change
                        ElseIf columndefinition.DefaultValue Is Nothing Then
                            newColumn.Nullable = True
                        End If

                        '** enfore uniqueness
                        If columndefinition.IsUnique Then
                            newColumn.Identity = True
                        End If

                        '** extended Properties
                        newColumn.ExtendedProperties.Refresh()
                        If newColumn.ExtendedProperties.Contains("MS_Description") Then
                            newColumn.ExtendedProperties("MS_Description").Value = columndefinition.Description
                        Else
                            Dim newEP As ExtendedProperty = New ExtendedProperty(parent:=newColumn, name:="MS_Description", propertyValue:=columndefinition.Description)
                            newColumn.ExtendedProperties.Add(newEP)
                            'newEP.Create() -> doesnot work
                        End If

                        '** add it
                        If addColumn Then aTable.Columns.Add(newColumn)
                        '** unique ?
                        
                        '*** return new column
                        Return newColumn

                    End If


                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, columnname:=columndefinition.Name, tablename:=columndefinition.Tablename, _
                                      subname:="mssqlDBDriver.GetColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetColumn", columnname:=columndefinition.Name, tablename:=columndefinition.Tablename, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Gets the foreign keys.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="aDBDesc">A DB desc.</param>
        ''' <param name="createOrAlter">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>

        Public Overrides Function GetForeignKeys(nativeTable As Object, foreignkeydefinition As ForeignKeyDefinition, _
                                            Optional createOrAlter As Boolean = False, _
                                            Optional ByRef connection As iormConnection = Nothing) As IEnumerable(Of Object) Implements iormDatabaseDriver.GetForeignKeys

            Dim myconnection As mssqlConnection

            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If
            ' ** important ! Access to native Internal Connection opens up the internal connection and also the SMO Driver
            ' **
            If myconnection Is Nothing Or myconnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(message:="internal connection connection is nothing - no table can be retrieved", subname:="mssqlDBDriver.GetColumn", _
                                            messagetype:=otCoreMessageType.InternalError, columnname:=foreignkeydefinition.Id, tablename:=foreignkeydefinition.Tablename)
                Return Nothing
            End If

            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.getForeignKey", columnname:=foreignkeydefinition.Id, tablename:=foreignkeydefinition.Tablename, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If


            '*** object
            If Not nativeTable.GetType = GetType(Table) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.getForeignKey", columnname:=foreignkeydefinition.Id, tablename:=foreignkeydefinition.Tablename, _
                                             message:="No SMO TableObject given to function", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Dim aTable As Table = DirectCast(nativeTable, Table)

            Try

                SyncLock _internallock

                    '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    Dim i As UShort
                    Dim resultkeys As New List(Of ForeignKey)
                    Dim alterflag As Boolean
                    '*** just return 
                    If Not createOrAlter Then

                        For Each aForeignkey As ForeignKey In aTable.ForeignKeys
                            For Each aColumnname In foreignkeydefinition.ColumnNames
                                If aForeignkey.Columns.Contains(aColumnname) Then
                                    resultkeys.Add(aForeignkey)
                                    Exit For
                                End If
                            Next
                        Next

                        Return resultkeys

                        '***
                        '*** Drop all Foreign Key usages if not nativeDatabase
                    ElseIf createOrAlter And (foreignkeydefinition.UseForeignKey <> otForeignKeyImplementation.NativeDatabase) Then
                        CoreMessageHandler(message:="Foreign Key usage is not 'native database' - drop all existing foreign keys", tablename:=aTable.Name, _
                                           arg1:=foreignkeydefinition.Id, _
                                               subname:="mssqlDBDriver.getForeignKey", messagetype:=otCoreMessageType.InternalWarning)

                        '** delete all existing key
                        alterflag = False
                        Dim aList As New List(Of ForeignKey)
                        For Each aexistingkey As ForeignKey In aTable.ForeignKeys
                            aList.Add(aexistingkey)
                            alterflag = True
                        Next
                        If alterflag Then
                            For Each existingkey In aList
                                existingkey.Drop()
                            Next
                        End If
                        Return resultkeys
                        '**
                        '** create foreign key
                        '**
                    ElseIf createOrAlter AndAlso foreignkeydefinition.UseForeignKey And otForeignKeyImplementation.NativeDatabase Then
                        Dim theColumnnames As String()
                        Dim theFKColumnnames As String()
                        Dim theFKTablenames As String()
                        Dim fkproperties As List(Of ForeignKeyProperty)
                        Dim aForeignKeyName As String = foreignkeydefinition.Id
                        Dim fkerror As Boolean = False

                        If foreignkeydefinition Is Nothing OrElse foreignkeydefinition.ForeignKeyReferences Is Nothing _
                            OrElse foreignkeydefinition.ForeignKeyReferences.Count = 0 Then
                            CoreMessageHandler(message:="Foreign Key Reference of column definition is not set - drop all existing foreign keys", tablename:=aTable.Name, _
                                               arg1:=foreignkeydefinition.Id, _
                                                subname:="mssqlDBDriver.getForeignKey", messagetype:=otCoreMessageType.InternalError)

                            '** delete all existing key
                            '** delete all existing key
                            alterflag = False
                            Dim aList As New List(Of ForeignKey)
                            For Each aexistingkey As ForeignKey In aTable.ForeignKeys
                                aList.Add(aexistingkey)
                                alterflag = True
                            Next
                            If alterflag Then
                                For Each existingkey In aList
                                    existingkey.Drop()
                                Next
                            End If
                            fkerror = True
                        Else
                            '** check count
                            If foreignkeydefinition.ForeignKeyReferences.Count <> foreignkeydefinition.ColumnNames.Count Then
                                CoreMessageHandler(message:="number of foreign Key references is different then the number of columnnames ", _
                                                      tablename:=aTable.Name, arg1:=foreignkeydefinition.Id, _
                                                       subname:="mssqlDBDriver.getForeignKey", messagetype:=otCoreMessageType.InternalError)
                                Return Nothing
                                fkerror = True
                            End If

                            

                            '** do bookeeping for new foreign keys for this table
                            '**
                            Dim no As UShort = foreignkeydefinition.ForeignKeyReferences.Count
                            ReDim theColumnnames(no - 1)
                            ReDim theFKColumnnames(no - 1)
                            ReDim theFKTablenames(no - 1)
                            Dim anTableColumnAttribute As ormSchemaTableColumnAttribute
                            i = 0

                            For i = 0 To no - 1
                                Dim afkreference As String = foreignkeydefinition.ForeignKeyReferences(i)
                                '** complete reference
                                If Not afkreference.Contains("."c) And Not afkreference.Contains(ConstDelimiter) Then
                                    CoreMessageHandler(message:="Foreign Key Reference of column definition has no object name part divided by '.'", _
                                                       tablename:=aTable.Name, columnname:=foreignkeydefinition.Id, _
                                                        arg1:=afkreference, subname:="mssqlDBDriver.getForeignKey", messagetype:=otCoreMessageType.InternalError)
                                    fkerror = True
                                Else
                                    Dim names = afkreference.ToUpper.Split("."c, ConstDelimiter)
                                    theFKTablenames(i) = names(0).Clone
                                    theFKColumnnames(i) = names(1).Clone
                                    If fkproperties Is Nothing AndAlso foreignkeydefinition.ForeignKeyProperty IsNot Nothing Then fkproperties = foreignkeydefinition.ForeignKeyProperty
                                    names = foreignkeydefinition.ColumnNames(i).ToUpper.Split("."c, ConstDelimiter)
                                    If names.Count > 0 Then
                                        theColumnnames(i) = names(1).Clone
                                    Else
                                        theColumnnames(i) = names(0).Clone
                                    End If

                                    '** resolve the reference - must be loaded previously
                                    Dim anColumnEntry = CurrentSession.Objects.GetColumnEntry(columnname:=theFKColumnnames(i), tablename:=theFKTablenames(i), runtimeOnly:=foreignkeydefinition.RunTimeOnly)
                                    If anColumnEntry Is Nothing Then
                                        anTableColumnAttribute = ot.GetSchemaTableColumnAttribute(columnname:=theFKColumnnames(i), tablename:=theFKTablenames(i))
                                        If anTableColumnAttribute Is Nothing Then
                                            CoreMessageHandler(message:="Foreign Key Reference of column definition was not found in the object repository - foreign key not set", _
                                                               tablename:=aTable.Name, columnname:=theFKColumnnames(i), _
                                                               arg1:=afkreference, subname:="mssqlDBDriver.getForeignKey", _
                                                               messagetype:=otCoreMessageType.InternalError)
                                            fkerror = True
                                        Else
                                            If anTableColumnAttribute.HasValueColumnName Then theFKColumnnames(i) = anTableColumnAttribute.ColumnName
                                            If anTableColumnAttribute.HasValueTableName Then theFKTablenames(i) = anTableColumnAttribute.Tablename
                                            If fkproperties Is Nothing AndAlso anTableColumnAttribute.HasValueForeignKeyProperties Then fkproperties = anTableColumnAttribute.ForeignKeyProperty.ToList
                                        End If
                                    End If

                                End If

                            Next
                        End If

                        '*** create keys
                        '***
                        If Not fkerror Then
                            Dim aforeignkey As ForeignKey
                            Dim uniquetables = theFKTablenames.Distinct.ToArray
                            i = 0
                            For Each aTablename In uniquetables
                                '** delete existing key
                                If aTable.ForeignKeys.Contains(aForeignKeyName & "_" & i) Then
                                    aTable.ForeignKeys.Item(aForeignKeyName & "_" & i).Drop()
                                End If
                               
                                '** rebuild
                                aforeignkey = New ForeignKey(aTable, aForeignKeyName & "_" & i)
                                'Add columns as the foreign key column.
                                For i = 0 To theFKColumnnames.Count - 1
                                    If theFKTablenames(i) = aTablename Then
                                        If theColumnnames(i) IsNot Nothing AndAlso theFKColumnnames(i) IsNot Nothing Then
                                            Dim fkColumn As ForeignKeyColumn
                                            fkColumn = New ForeignKeyColumn(aforeignkey, theColumnnames(i), theFKColumnnames(i))
                                            aforeignkey.Columns.Add(fkColumn)
                                        End If
                                    End If
                                Next
                                'Set the referenced table and schema.
                                aforeignkey.ReferencedTable = aTablename
                                aforeignkey.IsEnabled = True

                                'foreignkey.ReferencedTableSchema 
                                If fkproperties IsNot Nothing Then
                                    For Each [aProperty] In fkproperties
                                        If aProperty.Enum = otForeignKeyProperty.OnUpdate Then
                                            Select Case aProperty.ActionProperty.Enum
                                                Case otForeignKeyAction.Cascade
                                                    aforeignkey.UpdateAction = ForeignKeyAction.Cascade
                                                Case otForeignKeyAction.SetDefault
                                                    aforeignkey.UpdateAction = ForeignKeyAction.SetDefault
                                                Case otForeignKeyAction.SetNull
                                                    aforeignkey.UpdateAction = ForeignKeyAction.SetNull
                                                Case otForeignKeyAction.Restrict
                                                    CoreMessageHandler(message:="Restricted foreign key action OnUpdate is not implemented in MS-SQLServer", messagetype:=otCoreMessageType.InternalError, _
                                                                       subname:="mssqlDBDriver.getForeignKey")
                                                Case otForeignKeyAction.Noop
                                                    aforeignkey.UpdateAction = ForeignKeyAction.NoAction
                                                Case Else
                                                    CoreMessageHandler(message:="Restricted foreign key action OnUpdate not implemented ", arg1:=aProperty.ActionProperty.ToString, messagetype:=otCoreMessageType.InternalError, _
                                                                      subname:="mssqlDBDriver.getForeignKey")
                                            End Select
                                        ElseIf aProperty.Enum = otForeignKeyProperty.OnDelete Then
                                            Select Case aProperty.ActionProperty.Enum
                                                Case otForeignKeyAction.Cascade
                                                    aforeignkey.DeleteAction = ForeignKeyAction.Cascade
                                                Case otForeignKeyAction.SetDefault
                                                    aforeignkey.DeleteAction = ForeignKeyAction.SetDefault
                                                Case otForeignKeyAction.SetNull
                                                    aforeignkey.DeleteAction = ForeignKeyAction.SetNull
                                                Case otForeignKeyAction.Restrict
                                                    CoreMessageHandler(message:="Restricted foreign key action for OnDelete is not implemented in MS-SQLServer", messagetype:=otCoreMessageType.InternalError, _
                                                                       subname:="mssqlDBDriver.getForeignKey")
                                                Case otForeignKeyAction.Noop
                                                    aforeignkey.DeleteAction = ForeignKeyAction.NoAction
                                                Case Else
                                                    CoreMessageHandler(message:="Restricted foreign key action for OnDelete not implemented ", arg1:=aProperty.ActionProperty.ToString, messagetype:=otCoreMessageType.InternalError, _
                                                                      subname:="mssqlDBDriver.getForeignKey")
                                            End Select

                                        End If
                                    Next
                                End If
                                'Create the foreign key on the instance of SQL Server.
                                aforeignkey.Create()
                                resultkeys.Add(aforeignkey)
                            Next

                        End If

                        Return resultkeys
                    End If


                End SyncLock


            Catch smoex As SmoException

                Dim sb As New StringBuilder
                sb.AppendLine("This is an SMO Exception")
                'Display the SMO exception message.
                sb.AppendLine(smoex.Message)
                'Display the sequence of non-SMO exceptions that caused the SMO exception.
                Dim ex As Exception
                ex = smoex.InnerException
                If ex Is Nothing Then
                Else
                    Do While ex.InnerException IsNot (Nothing)
                        sb.AppendLine(ex.InnerException.Message)
                        ex = ex.InnerException
                    Loop
                End If

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, arg1:=foreignkeydefinition.Id, tablename:=foreignkeydefinition.Tablename, _
                                      subname:="mssqlDBDriver.GetColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="mssqlDBDriver.GetColumn", arg1:=foreignkeydefinition.Id, tablename:=foreignkeydefinition.Tablename, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try
        End Function


        ''' <summary>
        ''' Runs the SQL Command
        ''' </summary>
        ''' <param name="sqlcmdstr"></param>
        ''' <param name="parameters"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function RunSqlStatement(ByVal sqlcmdstr As String, _
                                                  Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                  Optional silent As Boolean = True, Optional nativeConnection As Object = Nothing) As Boolean _
        Implements iormDatabaseDriver.RunSqlStatement
            Dim anativeConnection As SqlConnection
            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    anativeConnection = DirectCast(Me.CurrentConnection, adonetConnection).NativeInternalConnection
                    If anativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="mssqlDBDriver.runSQLCommand", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="mssqlDBDriver.runSQLCommand", message:="Connection not available")
                    Return Nothing
                End If
            Else
                anativeConnection = nativeConnection
            End If
            Try
                SyncLock anativeConnection
                    Dim aSQLCommand As New SqlCommand
                    aSQLCommand.Connection = anativeConnection
                    aSQLCommand.CommandText = sqlcmdstr
                    aSQLCommand.CommandType = CommandType.Text
                    aSQLCommand.Prepare()

                    If aSQLCommand.ExecuteNonQuery() > 0 Then
                        Return True
                    Else
                        Return False
                    End If

                End SyncLock

            Catch ex As Exception
                Call CoreMessageHandler(subname:="mssqlDBDriver.runSQLCommand", arg1:=sqlcmdstr, exception:=ex)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' Sets the DB parameter.
        ''' </summary>
        ''' <param name="Parametername">The parametername.</param>
        ''' <param name="Value">The value.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <param name="UpdateOnly">The update only.</param>
        ''' <param name="silent">The silent.</param>
        ''' <returns></returns>
        Public Overrides Function SetDBParameter(parametername As String, Value As Object, Optional ByRef nativeConnection As Object = Nothing, _
        Optional updateOnly As Boolean = False, Optional silent As Boolean = False) As Boolean


            Dim dataRows() As DataRow
            Dim insertFlag As Boolean = False

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = DirectCast(_primaryConnection, mssqlConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="mssqlDBDriver.setDBParameter", _
                                              message:="Native Internal Connection not available")
                        Return False
                    End If
                Else
                    Call CoreMessageHandler(subname:="mssqlDBDriver.setDBParameter", _
                                          message:="Connection not available")
                    Return False
                End If

            End If


            '** init driver
            If Not Me.IsInitialized AndAlso Not Me.Initialize() Then
                Call CoreMessageHandler(subname:="mssqlDBDriver.setDBParameter", messagetype:=otCoreMessageType.InternalError, _
                                          message:="couldnot initialize database driver")
                Return False
            End If

            Try
                '** on Bootstrapping in the cache
                '** but bootstrapping mode is not sufficnt
                If _BootStrapParameterCache IsNot Nothing AndAlso _ParametersTable Is Nothing Then
                    If _BootStrapParameterCache.ContainsKey(key:=parametername) Then
                        _BootStrapParameterCache.Remove(key:=parametername)
                    End If
                    _BootStrapParameterCache.Add(key:=parametername, value:=Value)
                    Return True

                Else

                    '*** to the table
                    SyncLock _parameterlock
                        dataRows = _ParametersTable.Select("[" & ConstFNID & "]='" & parametername & "'")

                        ' not found
                        If dataRows.GetLength(0) = 0 Then
                            If updateOnly And silent Then
                                SetDBParameter = False
                                Exit Function
                            ElseIf updateOnly And Not silent Then
                                Call CoreMessageHandler(showmsgbox:=True, _
                                                      message:="The Parameter '" & parametername & "' was not found in the OTDB Table " & ConstParameterTableName, subname:="mssqlDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                                Return False
                            ElseIf Not updateOnly Then
                                ReDim dataRows(0)
                                dataRows(0) = _ParametersTable.NewRow
                                dataRows(0)(constFNDescription) = ""

                                insertFlag = True
                            End If
                        End If

                        ' value
                        'dataRows(0).BeginEdit()
                        dataRows(0)(ConstFNID) = parametername
                        dataRows(0)(ConstFNValue) = CStr(Value)
                        dataRows(0)(ConstFNChangedOn) = Date.Now().ToString
                        'dataRows(0).EndEdit()

                        '* add to table
                        If insertFlag Then
                            _ParametersTable.Rows.Add(dataRows(0))
                        End If

                        '* save only if not in bootstrapping
                        Dim i = _ParametersTableAdapter.Update(_ParametersTable)
                        If i > 0 Then
                            _ParametersTable.AcceptChanges()
                            Return True
                        Else
                            Return False
                        End If

                    End SyncLock
                End If
            Catch ex As Exception
                ' Handle the error

                Call CoreMessageHandler(showmsgbox:=silent, subname:="mssqlDBDriver.setDBParameter", _
                                      tablename:=_parametersTableName, entryname:=parametername)
                SetDBParameter = False
            End Try


        End Function

        ''' <summary>
        ''' Gets the DB parameter.
        ''' </summary>
        ''' <param name="PARAMETERNAME">The PARAMETERNAME.</param>
        ''' <param name="nativeConnection">The native connection.</param>
        ''' <param name="silent">The silent.</param>
        ''' <returns></returns>
        Public Overrides Function GetDBParameter(parametername As String, Optional ByRef nativeConnection As Object = Nothing, Optional silent As Boolean = False) As Object
            Dim dataRows() As DataRow

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = _primaryConnection.NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="mssqlDBDriver.getDBParameter", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="mssqlDBDriver.getDBParameter", message:="Connection not available")
                    Return Nothing
                End If
            End If


            Try
                '** init driver
                If Not Me.IsInitialized AndAlso Not Initialize() Then
                    Call CoreMessageHandler(subname:="mssqlDBDriver.getDBParameter", tablename:=ConstParameterTableName, _
                                       message:="couldnot initialize database driver", arg1:=Me.ID, entryname:=parametername)
                    Return False
                End If

                '** on Bootstrapping out of the cache
                '** but bootstrapping mode is not sufficnt
                If _BootStrapParameterCache IsNot Nothing AndAlso _ParametersTable Is Nothing Then
                    If _BootStrapParameterCache.ContainsKey(key:=parametername) Then
                        Return _BootStrapParameterCache.Item(key:=parametername)
                    Else
                        Return Nothing
                    End If
                Else
                    SyncLock _parameterlock
                        '** out of the table
                        '** select row
                        dataRows = _ParametersTable.Select("[" & ConstFNID & "]='" & parametername & "'")

                        ' not found
                        If dataRows.GetLength(0) = 0 Then
                            If silent Then
                                Return ""
                            ElseIf Not silent Then
                                Call CoreMessageHandler(showmsgbox:=True, _
                                                      message:="The Parameter '" & parametername & "' was not found in the OTDB Table " & ConstParameterTableName, subname:="mssqlDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                                Return Nothing

                            End If
                        End If

                        ' value
                        Return dataRows(0)(ConstFNValue)

                    End SyncLock
                End If
                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="mssqlDBDriver.getDBParameter", tablename:=ConstParameterTableName, _
                                      exception:=ex, entryname:=parametername)
                Return Nothing
            End Try

        End Function


        ''' <summary>
        ''' EventHandler for onConnect
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Friend Sub Connection_onConnection(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnConnection
            Call Me.Initialize()
        End Sub

        ''' <summary>
        ''' EventHandler for onDisConnect
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Friend Sub Connection_onDisConnection(sender As Object, e As ormConnectionEventArgs) Handles _primaryConnection.OnDisconnection
            Call Me.Reset()
        End Sub
    End Class


    ''' <summary>
    ''' SQL Server OnTrack Database Connection Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class mssqlConnection
        Inherits adonetConnection
        Implements iormConnection

        'Protected Friend Shadows _nativeConnection As SqlConnection
        'Protected Friend Shadows _nativeinternalConnection As SqlConnection

        '** SMO Objects
        Protected _SMOConnection As Microsoft.SqlServer.Management.Common.ServerConnection
        Protected _Server As Microsoft.SqlServer.Management.Smo.Server
        Protected _Database As Microsoft.SqlServer.Management.Smo.Database

        Public Shadows Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Shadows Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="databaseDriver"></param>
        ''' <param name="session"></param>
        ''' <param name="sequence"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal id As String, ByRef databaseDriver As iormDatabaseDriver, ByRef session As Session, sequence As ot.ConfigSequence)
            MyBase.New(id, DatabaseDriver, session, sequence)

        End Sub
        ''' <summary>
        ''' Gets the SMO connection.
        ''' </summary>
        ''' <value>The SMO connection.</value>
        Public ReadOnly Property SMOConnection() As Microsoft.SqlServer.Management.Common.ServerConnection
            Get
                Return Me._SMOConnection
            End Get
        End Property
        ''' <summary>
        ''' Gets the server.
        ''' </summary>
        ''' <value>The server.</value>
        Public ReadOnly Property Server() As Microsoft.SqlServer.Management.Smo.Server
            Get
                Return Me._Server
            End Get
        End Property
        ''' <summary>
        ''' Gets the database.
        ''' </summary>
        ''' <value>The database.</value>
        Public ReadOnly Property Database() As Microsoft.SqlServer.Management.Smo.Database
            Get
                Return Me._Database
            End Get
        End Property

        ''' <summary>
        ''' create a smo server connection and returns it. Sets also the scripting optimization and the default fields to load
        ''' </summary>
        ''' <param name="connection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateSMOConnection(connection As IDbConnection) As ServerConnection
            Dim aSMOConnection As ServerConnection
            aSMOConnection = New Microsoft.SqlServer.Management.Common.ServerConnection()
            aSMOConnection.ServerInstance = DirectCast(connection, SqlConnection).DataSource
            aSMOConnection.SqlExecutionModes = SqlExecutionModes.ExecuteSql
            aSMOConnection.AutoDisconnectMode = AutoDisconnectMode.NoAutoDisconnect

            If Not aSMOConnection Is Nothing Then
                _Server = New Server(aSMOConnection)
                _Server.ConnectionContext.LoginSecure = False
                _Server.ConnectionContext.Login = Me._Dbuser
                _Server.ConnectionContext.Password = Me._Dbpassword
                _Server.Refresh()
                ' get the database
                If _Server.Databases.Contains(DirectCast(_nativeinternalConnection, SqlConnection).Database) Then
                    _Database = _Server.Databases(DirectCast(_nativeinternalConnection, SqlConnection).Database)
                Else
                    Call CoreMessageHandler(showmsgbox:=True, message:="Database " & Me.DBName & " is not existing on server " & _Server.Name, _
                                           messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="mssqlConnection.CreateSMOConnection")
                    _Database = Nothing

                End If
                '** set what to load
                Dim scriptingOptions As ScriptingOptions = New ScriptingOptions()
                scriptingOptions.ExtendedProperties = True
                scriptingOptions.Indexes = True
                scriptingOptions.DriAllKeys = True
                scriptingOptions.DriForeignKeys = True

                _Database.PrefetchObjects(GetType(Table), scriptingOptions)

                _Server.SetDefaultInitFields(GetType(Table), {"CreateDate"})
                _Server.SetDefaultInitFields(GetType(Index), {"IndexKeyType"})
                _Server.SetDefaultInitFields(GetType(Column), {"Nullable", "ID", "Default", "DataType"})

                Return aSMOConnection
            Else
                Call CoreMessageHandler(message:="SMO Object for Database " & Me.DBName & " is not existing for server " & _Server.Name, break:=False, _
                                           messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="mssqlConnection.CreateSMOConnection")
                Return Nothing
            End If
        End Function

        Private Sub mssqlConnection_OnDisconnection(sender As Object, e As ormConnectionEventArgs) Handles Me.OnDisconnection

        End Sub
        ''' <summary>
        ''' Event Handler onInternalConnection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function OnInternalConnection(sender As Object, arguments As InternalConnectionEventArgs) As Boolean _
            Handles MyBase.OnInternalConnected
            If _SMOConnection Is Nothing Then
                _SMOConnection = CreateSMOConnection(_nativeinternalConnection)
            End If
            If _SMOConnection Is Nothing Then
                Call CoreMessageHandler(message:="SMO Object for Database " & Me.DBName & " is not existing for server " & _Server.Name, break:=False, _
                                           messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="mssqlConnection.OnInternalConnection")
            End If
        End Function

        Public Shadows Function RaiseOnConnected() Handles MyBase.OnConnection
            RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))

        End Function
        Public Shadows Function RaiseOnDisConnected() Handles MyBase.OnDisconnection
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
            _Server = Nothing
            _Database = Nothing
            _SMOConnection.ForceDisconnected()
            _SMOConnection = Nothing
        End Function

        ''' <summary>
        ''' Gets or sets the connection.
        ''' </summary>
        ''' <value>The connection.</value>
        Public Property SqlConnection() As SqlConnection
            Get
                If _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    Return Nothing
                Else
                    Return DirectCast(Me.NativeConnection, SqlConnection)
                End If

            End Get
            Protected Friend Set(value As SqlConnection)
                Me._nativeConnection = value
            End Set
        End Property


        ''' <summary>
        ''' create a new SQLConnection
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNewNativeConnection() As IDbConnection

            Return New SqlConnection()
        End Function


    End Class

    ''' <summary>
    ''' CLASS describes the schema per table of the database itself
    ''' </summary>
    ''' <remarks></remarks>
    Public Class mssqlTableSchema
        Inherits adonetTableSchema
        Implements iotDataSchema


        '***** internal variables
        '*****


        Public Sub New(ByRef connection As mssqlConnection, ByVal tableID As String)
            MyBase.New(connection, tableID)

        End Sub


        Protected Friend Overrides Function CreateNativeDBParameter() As IDbDataParameter
            Return New SqlParameter()
        End Function
        Protected Friend Overrides Function CreateNativeDBCommand() As IDbCommand
            Return New SqlCommand()
        End Function
        Protected Friend Overrides Function IsNativeDBTypeOfVar(type As Object) As Boolean
            Dim datatype As SqlDataType = type

            If datatype = SqlDataType.NVarChar Or datatype = SqlDataType.NText Or datatype = SqlDataType.VarChar _
             Or datatype = SqlDataType.VarChar Or datatype = SqlDataType.Binary Or datatype = SqlDataType.Variant _
             Or datatype = SqlDataType.NVarCharMax Or datatype = SqlDataType.VarCharMax Or datatype = SqlDataType.NChar _
             Or datatype = SqlDataType.VarBinary Or datatype = SqlDataType.Text Then
                Return True
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' Create aDBParameter
        ''' </summary>
        ''' <param name="columnname">name of the Column</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function AssignNativeDBParameter(fieldname As String, _
                                                          Optional parametername As String = "") As System.Data.IDbDataParameter Implements iotDataSchema.AssignNativeDBParameter
            Dim aDBColumnDescription As adoNetColumnDescription = GetColumnDescription(Me.GetFieldordinal(fieldname))
            Dim aParameter As SqlParameter

            If Not aDBColumnDescription Is Nothing Then

                aParameter = CreateNativeDBParameter()

                If parametername = "" Then
                    aParameter.ParameterName = "@" & fieldname
                Else
                    If parametername.First = "@" Then
                        aParameter.ParameterName = parametername
                    Else
                        aParameter.ParameterName = "@" & parametername
                    End If
                End If
                'aParameter.SqlDbType = aDBColumnDescription.DataType
                Select Case aDBColumnDescription.DataType
                    Case SqlDataType.BigInt
                        aParameter.SqlDbType = SqlDbType.BigInt
                    Case SqlDataType.SmallInt
                        aParameter.SqlDbType = SqlDbType.SmallInt
                    Case SqlDataType.Int
                        aParameter.SqlDbType = SqlDbType.Int
                    Case SqlDataType.NVarChar, SqlDataType.NVarCharMax, SqlDataType.NChar, SqlDataType.NText, SqlDataType.VarChar, SqlDataType.VarCharMax, SqlDataType.Text
                        aParameter.SqlDbType = SqlDbType.NVarChar
                    Case SqlDataType.Bit
                        aParameter.SqlDbType = SqlDbType.Bit
                    Case SqlDataType.Numeric, SqlDataType.Real, SqlDataType.Float
                        aParameter.SqlDbType = SqlDbType.Float
                    Case SqlDataType.Money
                        aParameter.SqlDbType = SqlDbType.Money
                    Case SqlDataType.SmallMoney
                        aParameter.SqlDbType = SqlDbType.SmallMoney
                    Case SqlDataType.DateTime
                        aParameter.SqlDbType = SqlDbType.DateTime
                    Case SqlDataType.DateTime2
                        aParameter.SqlDbType = SqlDbType.DateTime2
                    Case SqlDataType.Date
                        aParameter.SqlDbType = SqlDbType.Date
                    Case SqlDataType.SmallDateTime
                        aParameter.SqlDbType = SqlDbType.SmallDateTime
                    Case SqlDataType.Time
                        aParameter.SqlDbType = SqlDbType.Time
                        aParameter.Size = 7
                    Case Else
                        Call CoreMessageHandler(subname:="mssqlTableSchema.AssignNativeDBParameter", break:=False, message:="SqlDatatype not handled", _
                                               messagetype:=otCoreMessageType.ApplicationError)
                        aParameter.SqlDbType = SqlDbType.Variant
                End Select

                aParameter.SourceColumn = fieldname

                '** set the length
                If IsNativeDBTypeOfVar(aDBColumnDescription.DataType) Then
                    If aDBColumnDescription.CharacterMaxLength = 0 Then
                        aParameter.Size = Const_MaxMemoSize
                    Else
                        aParameter.Size = aDBColumnDescription.CharacterMaxLength
                    End If

                Else
                    If aDBColumnDescription.CharacterMaxLength <> 0 Then
                        ' aParameter.Size = aDBColumnDescription.CharacterMaxLength
                    End If
                    ' aParameter.Size = 0
                End If
                Return aParameter
            Else
                Call CoreMessageHandler(subname:="mssqlTableSchema.buildParameter", message:="ColumnDescription couldn't be loaded", _
                                                     arg1:=fieldname, tablename:=_TableID, messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            End If
        End Function
        ''' <summary>
        ''' Fills the schema for table.
        ''' </summary>
        ''' <param name="aTableName">Name of a table.</param>
        ''' <param name="reloadForce">The reload force.</param>
        ''' <returns></returns>
        Public Overrides Function Refresh(Optional reloadForce As Boolean = False) As Boolean
            Dim no As UShort
            Dim index As Integer
            Dim aColumnCollection As ArrayList
            Dim myConnection As mssqlConnection = DirectCast(Me._Connection, mssqlConnection)
            Dim aCon As SqlConnection = DirectCast(myConnection.NativeInternalConnection, SqlConnection)


            ' not working 
            If myConnection.Database Is Nothing OrElse Not myConnection.SMOConnection.IsOpen Then
                Call CoreMessageHandler(subname:="mssqlTableSchema.refresh", _
                                     message:="SMO Connection is not open", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                _IsInitialized = False
                Return False
            End If
            ' return if no TableID
            If _TableID = "" Then
                Call CoreMessageHandler(subname:="mssqlTableSchema.refresh", _
                                      message:="Nothing Tablename to set to", _
                                      tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                _IsInitialized = False
                Return False
            End If
            '

            Refresh = True

            Try
                myConnection.IsNativeInternalLocked = True
                Dim aTable As Table = New Table(myConnection.Database, name:=TableID)
                If aTable Is Nothing Then
                    Call CoreMessageHandler(subname:="mssqlTableSchema.refresh", _
                                     message:="Table couldnot be loaded from SMO", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    myConnection.IsNativeInternalLocked = False
                    _IsInitialized = False
                    Return False
                End If

               

                '** reload the Table
                '**
                aTable.Refresh()
                myConnection.IsNativeInternalLocked = False
                If False Then
                    Call CoreMessageHandler(subname:="mssqlTableSchema.refresh", _
                                     message:="Table couldnot initialized from SMO", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    _IsInitialized = False
                    Return False
                End If

                no = aTable.Columns.Count
                If no = 0 Then
                    Call CoreMessageHandler(subname:="mssqlTableSchema.refresh", _
                                                    message:="Table couldnot initialized from SMO - does it exist ????", _
                                                    tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    _IsInitialized = False
                    Return False
                Else
                    ReDim _fieldnames(no - 1)
                    ReDim _Columns(no - 1)
                End If

                ' set the Dictionaries if reload
                _fieldsDictionary = New Dictionary(Of String, Long)
                _indexDictionary = New Dictionary(Of String, ArrayList)
                aColumnCollection = New ArrayList
                _NoPrimaryKeys = 0
                Dim i As UShort = 0

                '*
                myConnection.IsNativeInternalLocked = True

                '* each column
                For Each aColumn As Column In aTable.Columns

                    '*
                    _fieldnames(i) = aColumn.Name.Clone
                    '* set the description
                    _Columns(i) = New adoNetColumnDescription
                    With _Columns(i)
                        .ColumnName = aColumn.Name.ToUpper

                        '* time penalty to heavy for refreshing
                        ' If Not aColumn.ExtendedProperties.Contains("MS_Description") Then aColumn.ExtendedProperties.Refresh()
                        'If aColumn.ExtendedProperties.Contains("MS_Description") Then
                        '.Description = aColumn.ExtendedProperties("MS_Description").Value
                        'Else
                        .Description = ""
                        'End If
                        If aColumn.Default <> "" Then
                            .HasDefault = True
                        Else
                            .HasDefault = False
                        End If
                        'If aColumn.DataType.MaximumLength Is Nothing Then
                        .CharacterMaxLength = aColumn.DataType.MaximumLength
                        'End If
                        .IsNullable = aColumn.Nullable
                        .DataType = aColumn.DataType.SqlDataType
                        .Ordinal = aColumn.ID
                        .Default = aColumn.Default.Clone
                        .Catalog = aColumn.DefaultSchema.Clone
                        '.DateTimePrecision = aColumn.DataType.DateTimePrecision
                        .NumericPrecision = aColumn.DataType.NumericPrecision
                        .NumericScale = aColumn.DataType.NumericScale
                        .CharachterOctetLength = aColumn.DataType.MaximumLength
                    End With
                    ' remove if existing
                    If _fieldsDictionary.ContainsKey(_fieldnames(i)) Then
                        _fieldsDictionary.Remove(_fieldnames(i))
                    End If
                    ' add
                    _fieldsDictionary.Add(key:=_fieldnames(i), value:=i + 1) 'store no field 1... not the array index

                    '* inc
                    i += 1
                Next

                '** Crossreference the Indices
                For Each anIndex As Index In aTable.Indexes
                    'anIndex.Refresh()

                    ' new
                    aColumnCollection = New ArrayList

                    For Each aColumn In anIndex.IndexedColumns

                        ' indx no
                        index = _fieldsDictionary.Item(aColumn.name.toupper)
                        '
                        '** check if primaryKey
                        'fill old primary Key structure
                        If anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                            _PrimaryKeyIndexName = anIndex.Name.ToUpper
                            _NoPrimaryKeys = _NoPrimaryKeys + 1
                            ReDim Preserve _Primarykeys(0 To _NoPrimaryKeys - 1)
                            _Primarykeys(_NoPrimaryKeys - 1) = index - 1 ' set to the array 0...ubound
                        End If

                        aColumnCollection.Add(aColumn.name.toupper)

                    Next

                    '** store final

                    If _indexDictionary.ContainsKey(anIndex.Name.ToUpper) Then
                        _indexDictionary.Remove(key:=anIndex.Name.ToUpper)
                    End If
                    _indexDictionary.Add(key:=anIndex.Name.ToUpper, value:=aColumnCollection)
                Next

                myConnection.IsNativeInternalLocked = False

                '**** read each Index
                '****
                Dim anIndexName As String = ""

                '**** build the commands
                '****
                Dim enumValues As Array = System.[Enum].GetValues(GetType(CommandType))
                For Each anIndexName In _indexDictionary.Keys
                    Dim aNewCommand As SqlCommand
                    For Each aCommandType In enumValues
                        Dim aNewKey = New CommandKey(anIndexName, aCommandType)
                        aNewCommand = BuildCommand(anIndexName, aCommandType)
                        If Not aNewCommand Is Nothing Then
                            If _CommandStore.ContainsKey(aNewKey) Then
                                _CommandStore.Remove(aNewKey)
                            End If
                            _CommandStore.Add(aNewKey, aNewCommand)
                        End If
                    Next


                Next

                _IsInitialized = True
                Return True

            Catch ex As Exception
                myConnection.IsNativeInternalLocked = False
                Call CoreMessageHandler(showmsgbox:=False, subname:="mssqlTableSchema.refresh", tablename:=_TableID, _
                                      arg1:=reloadForce, exception:=ex)

                _IsInitialized = False
                Return False
            End Try

        End Function

    End Class

    '************************************************************************************
    '***** CLASS mssqlTableStore describes the per Table reference and Helper Class
    '*****                    ORM Mapping Class and Table Access Workhorse
    '*****

    Public Class mssqlTableStore
        Inherits adonetTableStore
        Implements iormDataStore

        'Protected Friend Shadows _cacheAdapter As sqlDataAdapter

        '** initialize
        Public Sub New(connection As iormConnection, tableID As String, ByVal forceSchemaReload As Boolean)
            Call MyBase.New(Connection:=connection, tableID:=tableID, forceSchemaReload:=forceSchemaReload)
        End Sub


        ''' <summary>
        ''' is Linq Available
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property IsLinqAvailable As Boolean Implements iormDataStore.IsLinqAvailable
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' create the specific native Command
        ''' </summary>
        ''' <param name="commandstr"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNativeDBCommand(commandstr As String, ByRef nativeConnection As IDbConnection) As IDbCommand
            Return New SqlCommand(cmdText:=commandstr, connection:=nativeConnection)
        End Function

        ''' <summary>
        ''' converts data to a specific type
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="targetType"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Convert2ColumnData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "", _
                                                    Optional isnullable As Boolean? = Nothing, _
                                                    Optional defaultvalue As Object = Nothing) As Boolean _
                                                Implements iormDataStore.Convert2ColumnData

            If Not isnullable.HasValue And fieldname <> "" Then
                isnullable = Me.TableSchema.GetNullable(fieldname)
            Else
                isnullable = False
            End If
            If defaultvalue Is Nothing And fieldname <> "" Then
                defaultvalue = Me.TableSchema.GetDefaultValue(fieldname)
            End If
            '** return
            Return Me.Connection.DatabaseDriver.Convert2DBData(invalue:=invalue, outvalue:=outvalue, _
                                                               targetType:=targetType, maxsize:=maxsize, abostrophNecessary:=abostrophNecessary, _
                                                             fieldname:=fieldname, isnullable:=isnullable, defaultvalue:=defaultvalue)
        End Function


        '*********
        '********* cvt2ObjData returns a object from the Datatype of the column to XLS nterpretation
        '*********
        ''' <summary>
        ''' returns a object from the Datatype of the column to Host interpretation
        ''' </summary>
        ''' <param name="index">index as object (name or index 1..n)</param>
        ''' <param name="value">value to convert</param>
        ''' <param name="abostrophNecessary">True if necessary</param>
        ''' <returns>convered value </returns>
        ''' <remarks></remarks>
        Public Overrides Function Convert2ObjectData(ByVal index As Object, _
                                                     ByVal invalue As Object, _
                                                     ByRef outvalue As Object, _
                                                     Optional isnullable As Boolean? = Nothing, _
                                                     Optional defaultvalue As Object = Nothing, _
                                                     Optional ByRef abostrophNecessary As Boolean = False) As Boolean Implements iormDataStore.Convert2ObjectData
            Dim aSchema As mssqlTableSchema = Me.TableSchema
            Dim aDBColumn As mssqlTableSchema.adoNetColumnDescription
            Dim result As Object = Nothing
            Dim fieldno As Integer


            Try

                fieldno = aSchema.GetFieldordinal(index)
                If fieldno < 0 Then
                    Call CoreMessageHandler(subname:="mssqlTableStore.cvt2ColumnData", messagetype:=otCoreMessageType.InternalError, _
                                          message:="mssqlTableStore " & Me.TableID & " anIndex for " & index & " not found", _
                                          tablename:=Me.TableID, arg1:=index)
                    System.Diagnostics.Debug.WriteLine("mssqlTableStore " & Me.TableID & " anIndex for " & index & " not found")

                    Return False
                Else
                    aDBColumn = aSchema.GetColumnDescription(fieldno)
                End If

                If Not isnullable.HasValue Then
                    isnullable = Me.TableSchema.GetNullable(index)
                End If
                If defaultvalue = Nothing Then
                    defaultvalue = Me.TableSchema.GetDefaultValue(index)
                End If
                abostrophNecessary = False

                '*
                '*
                'If IsError(ainvalue) Then
                '    System.Diagnostics.Debug.WriteLine "Error in Formular of field invalue " & ainvalue & " while updating OTDB"
                '    ainvalue = ""
                'End If

                If aDBColumn.DataType = SqlDataType.BigInt Or aDBColumn.DataType = SqlDataType.Int _
                    Or aDBColumn.DataType = SqlDataType.SmallInt Or aDBColumn.DataType = SqlDataType.TinyInt Then

                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToInt64(0)
                    If isnullable Then
                        result = New Nullable(Of Long)
                    Else
                        result = New Long
                    End If

                    If isnullable AndAlso (Not IsNumeric(invalue) OrElse invalue Is Nothing OrElse _
                                               DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = New Nullable(Of Long)
                    ElseIf Not isnullable AndAlso (Not IsNumeric(invalue) OrElse invalue Is Nothing OrElse _
                                               DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = Convert.ToInt64(defaultvalue)
                    ElseIf IsNumeric(invalue) Then
                        result = Convert.ToInt64(invalue)
                    Else
                        Call CoreMessageHandler(subname:="mssqlTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & invalue & "' is not convertible to Integer", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Char Or aDBColumn.DataType = SqlDataType.NText _
                     Or aDBColumn.DataType = SqlDataType.VarChar Or aDBColumn.DataType = SqlDataType.Text _
                      Or aDBColumn.DataType = SqlDataType.NVarChar Or aDBColumn.DataType = SqlDataType.VarCharMax _
                      Or aDBColumn.DataType = SqlDataType.NVarCharMax Then
                    abostrophNecessary = True
                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToString("")

                    If isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse _
                                          String.IsNullOrWhiteSpace(invalue)) Then
                        result = Nothing
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse _
                                          String.IsNullOrWhiteSpace(invalue)) Then
                        result = Convert.ToString(defaultvalue)
                    Else
                        result = Convert.ToString(invalue)
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Date Or aDBColumn.DataType = SqlDataType.SmallDateTime Or aDBColumn.DataType = SqlDataType.Time _
                Or aDBColumn.DataType = SqlDataType.Timestamp Or aDBColumn.DataType = SqlDataType.DateTime Or aDBColumn.DataType = SqlDataType.DateTime2 _
                Or aDBColumn.DataType = SqlDataType.DateTimeOffset Then
                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToDateTime(ConstNullDate)
                    If isnullable Then
                        result = New Nullable(Of DateTime)
                    Else
                        result = New DateTime
                    End If

                    If isnullable AndAlso (Not IsDate(invalue) OrElse invalue Is Nothing OrElse DBNull.Value.Equals(invalue) _
                                            OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = New Nullable(Of DateTime)
                    ElseIf (Not IsDate(invalue) Or invalue Is Nothing Or DBNull.Value.Equals(invalue) Or IsError(invalue)) OrElse String.IsNullOrWhiteSpace(invalue) Then
                        result = Convert.ToDateTime(defaultvalue)
                    ElseIf IsDate(invalue) Then
                        result = Convert.ToDateTime(invalue)
                    Else
                        Call CoreMessageHandler(subname:="mssqlTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & invalue & "' is not convertible to Date", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Float Or aDBColumn.DataType = SqlDataType.Decimal _
               Or aDBColumn.DataType = SqlDataType.Real Then
                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToDouble(0)
                    If isnullable Then
                        result = New Nullable(Of Double)
                    Else
                        result = New Double
                    End If

                    If isnullable AndAlso (Not IsNumeric(invalue) OrElse invalue Is Nothing OrElse _
                        DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = New Nullable(Of Double)
                    ElseIf isnullable AndAlso (Not IsNumeric(invalue) OrElse invalue Is Nothing OrElse _
                        DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = Convert.ToDouble(defaultvalue)
                    ElseIf IsNumeric(invalue) Then
                        result = Convert.ToDouble(invalue)
                    Else
                        Call CoreMessageHandler(subname:="mssqlTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & invalue & "' is not convertible to Double", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Bit Then
                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToBoolean(False)
                    If isnullable Then
                        result = New Nullable(Of Boolean)
                    Else
                        result = New Boolean
                    End If

                    If isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) _
                                               OrElse invalue = False) OrElse String.IsNullOrWhiteSpace(invalue) Then
                        result = New Nullable(Of Boolean)
                    ElseIf Not isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) _
                                               OrElse invalue = False) OrElse String.IsNullOrWhiteSpace(invalue) Then
                        result = Convert.ToBoolean(False)
                    Else
                        result = True
                    End If

                End If

                ' return
                outvalue = result
                Return True
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="mssqlTableStore.cvt2ObjData", _
                                      arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName, exception:=ex, _
                                      messagetype:=otCoreMessageType.InternalError)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Initialize Cache 
        ''' </summary>
        ''' <returns>true if successfull </returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function InitializeCache(Optional ByVal force As Boolean = False) As Boolean

            Dim aCommand As SqlCommand
            Dim aDataSet As DataSet

            Try
                '** initialize
                If Not Me.IsCacheInitialized Or force Then
                    '** if the connection is during bootstrapping installation not available
                    Dim anativeConnection As SqlConnection = DirectCast(Me.Connection.NativeConnection, SqlConnection)
                    If anativeConnection Is Nothing OrElse _
                        (Not anativeConnection.State = ConnectionState.Open AndAlso DirectCast(Me.Connection, mssqlConnection).NativeInternalConnection.State = ConnectionState.Open) Then
                        anativeConnection = DirectCast(Me.Connection, mssqlConnection).NativeInternalConnection
                    End If
                    ' set theAdapter
                    _cacheAdapter = New SqlDataAdapter
                    MyBase._cacheAdapter = _cacheAdapter
                    _cacheAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                    aDataSet = DirectCast(Me.Connection.DatabaseDriver, mssqlDBDriver).OnTrackDataSet
                    ' Select Command
                    aCommand = DirectCast(Me.TableSchema, mssqlTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          adonetTableSchema.CommandType.SelectType)
                    If Not aCommand Is Nothing Then
                        ' create cache with select on all but no where -> aCommand holds where on the primary keys
                        Dim selectstr As String = "SELECT "
                        For i = 1 To Me.TableSchema.NoFields
                            selectstr &= "[" & Me.TableSchema.Getfieldname(i) & "]"
                            If i < Me.TableSchema.NoFields Then
                                selectstr &= ","
                            End If
                        Next
                        selectstr &= " FROM " & Me.TableID
                        _cacheAdapter.SelectCommand = New SqlCommand(selectstr)
                        _cacheAdapter.SelectCommand.CommandType = CommandType.Text
                        _cacheAdapter.SelectCommand.Connection = anativeConnection
                        _cacheAdapter.FillSchema(aDataSet, SchemaType.Source)
                        DirectCast(_cacheAdapter, SqlDataAdapter).Fill(aDataSet, Me.TableID)
                        ' set the Table
                        _cacheTable = aDataSet.Tables(Me.TableID)
                        If _cacheTable Is Nothing Then
                            CoreMessageHandler(message:="Cache Table couldnot be read from database", _
                                                arg1:=selectstr, subname:="mssqlTableStore.InitializeCache", messagetype:=otCoreMessageType.InternalError)
                            Return False
                        End If

                        ' set the nulls
                        For Each aColumn As Data.DataColumn In _cacheTable.Columns
                            aColumn.AllowDBNull = Me.TableSchema.GetNullable(aColumn.ColumnName)
                        Next

                        ' Build DataViews per Index
                        For Each indexName As String In Me.TableSchema.Indices
                            Dim aDataview As DataView

                            If _cacheViews.ContainsKey(key:=indexName) Then
                                aDataview = _cacheViews.Item(key:=indexName)
                            Else
                                aDataview = New DataView(_cacheTable)
                            End If

                            Dim fieldlist As String = ""
                            For Each fieldname In Me.TableSchema.GetIndex(indexName)
                                If fieldlist = "" Then
                                    fieldlist &= fieldname
                                Else
                                    fieldlist &= "," & fieldname
                                End If
                            Next
                            aDataview.Sort = fieldlist
                            If Not _cacheViews.ContainsKey(key:=indexName) Then
                                _cacheViews.Add(key:=indexName, value:=aDataview)
                            End If
                        Next


                    End If

                    ' Delete Command
                    aCommand = DirectCast(Me.TableSchema, mssqlTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          adonetTableSchema.CommandType.DeleteType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.DeleteCommand = aCommand
                    End If

                    ' Insert Command
                    aCommand = DirectCast(Me.TableSchema, mssqlTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          adonetTableSchema.CommandType.InsertType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.InsertCommand = aCommand
                    End If
                    ' Update Command
                    aCommand = DirectCast(Me.TableSchema, mssqlTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          adonetTableSchema.CommandType.UpdateType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.UpdateCommand = aCommand
                    End If

                    '** return true
                    Return True
                Else
                    Return False
                End If



            Catch ex As Exception
                Call CoreMessageHandler(subname:="mssqlTableStore.initializeCache", exception:=ex, message:="Exception", _
                                      messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return False
            End Try

        End Function
        ''' <summary>
        ''' update the cache Datatable
        ''' </summary>
        ''' <param name="datatable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function UpdateDBDataTable(ByRef dataadapter As IDbDataAdapter, ByRef datatable As DataTable) As Integer
            Try
                Return DirectCast(dataadapter, SqlDataAdapter).Update(datatable)
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception occured", subname:="mssqlTableStore.UpdateDBDataTable", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return 0
            End Try

        End Function
    End Class

End Namespace
