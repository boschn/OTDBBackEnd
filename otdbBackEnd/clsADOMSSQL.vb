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


    '************************************************************************************
    '***** CLASS clsMSSQLDriver describes the  Database Driver  to OnTrack
    '*****       based on ADO.NET MS SQL
    '*****

    Public Class clsMSSQLDriver
        Inherits clsADONETDBDriver
        Implements iormDBDriver

        Protected Shadows WithEvents _primaryConnection As clsMSSQLConnection '-> in clsOTDBDriver
        Private Shadows _ParametersTableAdapter As New SqlDataAdapter

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="ID">an ID for this driver</param>
        ''' <remarks></remarks>
        Public Sub New(ID As String, ByRef session As Session)
            Call MyBase.New(ID, session)
            Me.ID = ID
            If Me._primaryConnection Is Nothing Then
                _primaryConnection = New clsMSSQLConnection(id:="primary", DatabaseDriver:=Me, session:=session, sequence:=ConfigSequence.primary)
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
        Protected Friend Function Initialize(Optional Force As Boolean = False) As Boolean

            If Me.IsInitialized And Not Force Then
                Return True
            End If

            Try
                Call MyBase.Initialize()

                ' we have no Connection ?!
                If _primaryConnection Is Nothing Then
                    _primaryConnection = New clsMSSQLConnection("primary", Me, _session, ConfigSequence.primary)
                End If
                '*** set the DataTable
                _OnTrackDataSet = New DataSet("onTrackSession -" & Date.Now.ToString)
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

                Me.IsInitialized = True
                Return True
            Catch ex As Exception
                Me.IsInitialized = False
                Call CoreMessageHandler(subname:="clsMSSQLDriver.OnConnection", message:="couldnot Initialize Driver", _
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
            Return New clsMSSQLTableStore(Me.CurrentConnection, TableID, forceSchemaReload)
        End Function
        ''' <summary>
        ''' create a new TableSchema for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNativeTableSchema(ByVal TableID As String) As iotTableSchema
            Return New clsMSSQLTableSchema(Me.CurrentConnection, TableID)
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function createNativeDBCommand(commandstr As String, nativeConnection As IDbConnection) As IDbCommand Implements iormDBDriver.CreateNativeDBCommand
            Return New SqlCommand(commandstr, nativeConnection)
        End Function

        ''' <summary>
        ''' converts data to a specific type
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="targetType"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Convert2DBData(ByVal value As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "") As Object Implements iormDBDriver.Convert2DBData

            Dim result As Object
            Try
                '*
                '*
                If IsError(value) Then
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.cvt2ColumnData", _
                                          message:="Error in Formular of field value " & value & " while updating OTDB", _
                                          arg1:=value, messagetype:=otCoreMessageType.InternalError)
                    System.Diagnostics.Debug.WriteLine("Error in Formular of field value " & value & " while updating OTDB")
                    value = ""
                End If


                If targetType = SqlDataType.BigInt Or targetType = SqlDataType.Int _
                Or targetType = SqlDataType.SmallInt Or targetType = SqlDataType.TinyInt Then

                    If value Is Nothing OrElse String.IsNullOrWhiteSpace(value.ToString) _
                        OrElse IsError(value) OrElse DBNull.Value.Equals(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CLng(value)
                    Else
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Integer", _
                                              arg1:=value, messagetype:=otCoreMessageType.InternalError)
                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Integer")
                        result = DBNull.Value
                    End If

                ElseIf targetType = SqlDataType.Char Or targetType = SqlDataType.NText _
                    Or targetType = SqlDataType.VarChar Or targetType = SqlDataType.Text _
                     Or targetType = SqlDataType.NVarChar Or targetType = SqlDataType.VarCharMax _
                     Or targetType = SqlDataType.NVarCharMax Then

                    abostrophNecessary = True

                    If value Is Nothing OrElse String.IsNullOrWhiteSpace(value) OrElse IsError(value) OrElse DBNull.Value.Equals(value) Then
                        result = ""
                    Else
                        If maxsize < Len(CStr(value)) And maxsize > 1 Then
                            result = Mid(CStr(value), 0, maxsize - 1)
                        Else
                            result = CStr(value)
                        End If


                    End If

                ElseIf targetType = SqlDataType.Date Or targetType = SqlDataType.SmallDateTime Or targetType = SqlDataType.Time _
                Or targetType = SqlDataType.Timestamp Or targetType = SqlDataType.DateTime Or targetType = SqlDataType.DateTime2 _
                Or targetType = SqlDataType.DateTimeOffset Then
                    If value Is Nothing OrElse String.IsNullOrWhiteSpace(value.ToString) OrElse _
                        IsError(value) OrElse DBNull.Value.Equals(value) Then
                        result = ConstNullDate
                    ElseIf IsDate(value) Then
                        result = CDate(value)
                    ElseIf value.GetType = GetType(TimeSpan) Then
                        result = value
                    Else

                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Date")
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Date", _
                                              arg1:=value, messagetype:=otCoreMessageType.InternalError)
                        result = ConstNullDate
                    End If
                ElseIf targetType = SqlDataType.Float Or targetType = SqlDataType.Decimal _
                Or targetType = SqlDataType.Real Then
                    If value Is Nothing OrElse String.IsNullOrWhiteSpace(value.ToString) _
                        OrElse IsError(value) OrElse DBNull.Value.Equals(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CDbl(value)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Double")
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Double", _
                                              arg1:=targetType, messagetype:=otCoreMessageType.InternalError)
                        result = DBNull.Value
                    End If
                ElseIf targetType = SqlDataType.Bit Then
                    If value Is Nothing OrElse DBNull.Value.Equals(value) OrElse String.IsNullOrWhiteSpace(value.ToString) _
                        OrElse IsError(value) OrElse (IsNumeric(value) AndAlso value = 0) Then
                        result = False
                    ElseIf TypeOf (value) Is Boolean Then
                        result = value
                    Else
                        result = True
                    End If

                End If

                ' return
                Return result
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", subname:="clsMSSQLDriver.convert2ColumnData(Object, long ..", _
                                       exception:=ex, messagetype:=otCoreMessageType.InternalException)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' returns the target type for a OTDB FieldType - MAPPING
        ''' </summary>
        ''' <param name="type"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDBDriver.GetTargetTypeFor

            Try


                Select Case type
                    Case otFieldDataType.Binary
                        Return SqlDbType.Binary
                    Case otFieldDataType.Bool
                        Return SqlDbType.Bit
                    Case otFieldDataType.[Date]
                        Return SqlDbType.Date
                    Case otFieldDataType.[Time]
                        Return SqlDbType.Time
                    Case otFieldDataType.List
                        Return SqlDbType.NVarChar
                    Case otFieldDataType.[Long]
                        Return SqlDbType.BigInt
                    Case otFieldDataType.Memo
                        Return SqlDbType.NVarChar
                    Case otFieldDataType.Numeric
                        Return SqlDbType.Decimal
                    Case otFieldDataType.Timestamp
                        Return SqlDbType.DateTime
                    Case otFieldDataType.Text
                        Return SqlDbType.NVarChar
                    Case Else

                        Call CoreMessageHandler(subname:="clsMSSQLDriver.GetTargetTypefor", message:="Type not defined",
                                       messagetype:=otCoreMessageType.InternalException)
                End Select

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsMSSQLDriver.GetTargetTypefor", message:="Exception", exception:=ex, _
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
                                                      Implements iormDBDriver.AssignNativeDBParameter


            Try
                Dim aParameter As New SqlParameter()

                aParameter.ParameterName = parametername
                aParameter.SqlDbType = GetTargetTypeFor(datatype)
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
                Call CoreMessageHandler(subname:="clsMSSQLDriver.assignDBParameter", message:="Exception", exception:=ex, _
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
        Public Overrides Function GetCatalog(Optional FORCE As Boolean = False, Optional ByRef NativeConnection As Object = Nothing) As Object
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
        Public Overrides Function HasTable(tableID As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
            Dim anUser As New User
            Dim aSchema As New ObjectEntryDefinition
            Dim aTable As Table

            '* if already loaded
            If _TableDirectory.ContainsKey(key:=tableID) Then Return True

            '*** check on rights
            If LCase(tableID) <> LCase(User.ConstTableID) And LCase(tableID) <> LCase(ObjectDefinition.ConstTableID) Then
                If Not _primaryConnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], loginOnFailed:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.HasTable", tablename:=tableID, _
                                          message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            ''* connected ?!
            'If _primaryConnection Is Nothing OrElse Not _primaryConnection.IsConnected Then
            '    Call CoreMessageHandler(subname:="clsMSSQLDriver.HasTable", tablename:=tableID, _
            '                            message:="not connected to database", messagetype:=otCoreMessageType.InternalError)
            '    Return False
            'End If

            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Try

                If nativeConnection Is Nothing Then
                    smoconnection = _primaryConnection.SMOConnection
                    database = _primaryConnection.Database
                Else
                    smoconnection = New ServerConnection(TryCast(nativeConnection, SqlConnection))
                    database = _primaryConnection.Database
                End If

                If smoconnection Is Nothing OrElse database Is Nothing Then
                    Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                          subname:="clsMSSQLDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    _primaryConnection.IsNativeInternalLocked = True
                End If


                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(message:="_primaryConnection is nothing - no table can be retrieved", subname:="clsMSQLDriver.hasTable", _
                                                messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                database.Tables.Refresh()
                Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)

                Return existsOnServer

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
                                      subname:="clsMSSQLDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableID, _
                                      subname:="clsMSSQLDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
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
                                           Optional createOrAlter As Boolean = True, _
                                           Optional addToSchemaDir As Boolean = True, _
                                           Optional ByRef nativeConnection As Object = Nothing, _
                                           Optional ByRef nativeTableObject As Object = Nothing) As Object

            Dim anUser As New User
            Dim aSchema As New ObjectEntryDefinition
            Dim aTable As Table


            If _primaryConnection Is Nothing Then
                Call CoreMessageHandler(subname:="clsMSSQLDriver.GetTable", tablename:=tableID, _
                                      message:="No current Connection to the Database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
                '** Schema and User Creation are for free !
            End If
            '*** check on rights
            If createOrAlter Then
                If LCase(tableID) <> LCase(anUser.TableID) And LCase(tableID) <> LCase(aSchema.TableID) Then
                    If Not _primaryConnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, loginOnFailed:=True) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetTable", tablename:=tableID, _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If

            ' Do While _primaryConnection.IsNativeInternalLocked
            'System.Threading.Thread.CurrentThread.Sleep(1000)
            'Loop
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Try

                If nativeConnection Is Nothing Then
                    smoconnection = _primaryConnection.SMOConnection
                    database = _primaryConnection.Database
                Else
                    smoconnection = New ServerConnection(TryCast(nativeConnection, SqlConnection))
                    database = _primaryConnection.Database
                End If

                Dim aSchemaDir As ObjectDefinition
                Dim localCreated As Boolean = False

                If smoconnection Is Nothing OrElse database Is Nothing Then
                    Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                          subname:="clsMSSQLDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    _primaryConnection.IsNativeInternalLocked = True
                End If

                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(message:="_primaryConnection is nothing - no table can be retrieved", subname:="clsMSQLDriver.GetTable", _
                                                messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                database.Tables.Refresh()
                Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)

                '*** Exists and nothing supplied -> get it
                If existsOnServer And (nativeTableObject Is Nothing OrElse nativeTableObject.GetType <> GetType(Table)) Then
                    aTable = database.Tables(tableID)

                    aTable.Refresh()
                    _primaryConnection.IsNativeInternalLocked = False
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
                    _primaryConnection.IsNativeInternalLocked = False
                    Return aTable
                    '** doesnot Exist 
                ElseIf (Not createOrAlter And Not existsOnServer) Then
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.gettable", message:="Table doesnot exist", messagetype:=otCoreMessageType.InternalWarning, _
                                           break:=False, tablename:=tableID, arg1:=tableID)
                    Return Nothing
                End If

                '** create the table
                '**
                If createOrAlter Then
                    If Not localCreated And Not _primaryConnection.Database.Tables.Contains(name:=tableID) Then
                        aTable.Create()
                    ElseIf _primaryConnection.Database.Tables.Contains(name:=tableID) Then
                        aTable.Alter()
                    End If
                    ' check if containskey -> write
                    If addToSchemaDir Then
                        ' set it here -> bootstrapping will fail otherwise
                        aSchemaDir = New ObjectDefinition
                        Call aSchemaDir.Create(tableID)
                        Call aSchemaDir.Persist()
                    End If
                    Return aTable
                Else
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.getTable", tablename:=tableID, _
                                          message:="Table was not found in database", messagetype:=otCoreMessageType.ApplicationWarning)
                    _primaryConnection.IsNativeInternalLocked = False
                    Return Nothing
                End If

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
                                      subname:="clsMSSQLDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableID, _
                                      subname:="clsMSSQLDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
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
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>
        Public Overrides Function GetIndex(ByRef nativeTable As Object, _
                                           ByRef indexname As String, _
                                           ByRef columnNames As List(Of String), _
                                           Optional ByVal primaryKey As Boolean = False, _
                                            Optional ByVal forceCreation As Boolean = False, _
                                            Optional ByVal createOrAlter As Boolean = True, _
                                            Optional ByVal addToSchemaDir As Boolean = True) As Object Implements iormDBDriver.GetIndex


            Dim anUser As New User
            Dim aSchema As New ObjectEntryDefinition
            '*** object
            If Not nativeTable.GetType = GetType(Table) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.getIndex", _
                                             message:="No SMO TableObject given to funciton", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Dim aTable As Table = DirectCast(nativeTable, Table)
            If _primaryConnection Is Nothing Then
                Call CoreMessageHandler(subname:="clsMSSQLDriver.getIndex", message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights
            If createOrAlter Then
                If LCase(aTable.Name) <> LCase(anUser.TableID) And LCase(aTable.Name) <> LCase(aSchema.TableID) Then
                    If Not _primaryConnection.VerifyUserAccess(otAccessRight.AlterSchema) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.getIndex", _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If



            Dim aSchemaDir As New ObjectEntryDefinition
            Dim existingIndex As Boolean = False
            Dim indexnotchanged As Boolean = False
            Dim aIndexColumn As IndexedColumn
            Dim existPrimaryName As String = ""
            Dim anIndex As Index
            Dim i As UShort = 0

            Try
                'Do While _primaryConnection.IsNativeInternalLocked
                'System.Threading.Thread.CurrentThread.Sleep(1000)
                'Loop

                _primaryConnection.IsNativeInternalLocked = True
                '**
                If aTable.Indexes.Count = 0 Then aTable.Refresh()
                _primaryConnection.IsNativeInternalLocked = False

                ' save the primary name
                For Each index As Index In aTable.Indexes
                    If LCase(index.Name) = LCase(indexname) Or LCase(index.Name) = LCase(aTable.Name & "_" & indexname) Then
                        existingIndex = True
                        anIndex = index
                    End If
                    If index.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                        existPrimaryName = index.Name
                        If indexname = "" Then
                            indexname = index.Name
                            existingIndex = True
                            anIndex = index
                        End If
                    End If
                Next

                '** check on changes
                If (aTable.Indexes.Contains(name:=LCase(indexname)) OrElse _
                    aTable.Indexes.Contains(name:=LCase(aTable.Name & "_" & indexname))) _
                    And Not forceCreation Then

                    If aTable.Indexes.Contains(name:=LCase(indexname)) Then
                        anIndex = aTable.Indexes(name:=LCase(indexname))
                    Else
                        anIndex = aTable.Indexes(name:=LCase(aTable.Name & "_" & indexname))
                    End If
                    ' check all Members
                    If Not forceCreation And existingIndex Then
                        i = 0
                        For Each columnName As String In columnNames
                            ' check
                            If Not IsNothing(columnName) Then
                                'For j = i To anIndex.Columns.count
                                ' not equal
                                aIndexColumn = anIndex.IndexedColumns(i)
                                If LCase(aIndexColumn.Name) <> LCase(columnName) Then
                                    indexnotchanged = False
                                    Exit For
                                Else
                                    indexnotchanged = True
                                    ' check if containskey -> write
                                    If addToSchemaDir And anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                                        ' set it here -> bootstrapping will fail otherwise
                                        aSchemaDir = New ObjectEntryDefinition
                                        If Not aSchemaDir.LoadBy(aTable.Name, entryname:=columnName) Then
                                            Call aSchemaDir.Create(aTable.Name, entryname:=columnName)
                                        End If
                                        aSchemaDir.Indexname = LCase(aTable.Name & "_" & indexname)
                                        aSchemaDir.IndexPosition = i + 1
                                        aSchemaDir.IsKey = True
                                        aSchemaDir.IsPrimaryKey = True
                                        Call aSchemaDir.Persist()
                                    End If
                                End If
                                'Next j
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

                    Call CoreMessageHandler(message:="index doesnot exist", subname:="clsMSSQLDriver.getIndex", arg1:=indexname, _
                                           tablename:=aTable.Name, messagetype:=otCoreMessageType.InternalError)

                    Return Nothing

                End If

                '** create
                _primaryConnection.IsNativeInternalLocked = True

                ' if we have another Primary
                If primaryKey And LCase(indexname) <> LCase(existPrimaryName) And existPrimaryName <> "" Then
                    'IndexName is found and not the same ?!
                    Call CoreMessageHandler(message:="IndexName of table " & aTable.Name & " is " & anIndex.Name & " and not " & indexname & " - getOTDBIndex aborted", _
                                          messagetype:=otCoreMessageType.InternalError, subname:="clsMSSQLDriver.getIndex")
                    Return Nothing
                    ' create primary key
                ElseIf primaryKey And existPrimaryName = "" Then
                    'create primary
                    If indexname = "" Then
                        indexname = LCase(aTable.Name & "_primarykey")
                    Else
                        indexname = LCase(aTable.Name & "_" & indexname)
                    End If
                    anIndex = New Index(parent:=aTable, name:=indexname)
                    anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey
                    anIndex.IndexType = IndexType.NonClusteredIndex
                    anIndex.IgnoreDuplicateKeys = False
                    anIndex.IsUnique = True
                    '** extend PrimaryKey
                ElseIf primaryKey And LCase(indexname) = LCase(existPrimaryName) Then
                    '* DROP !
                    anIndex.Drop()

                    '* create
                    If indexname = "" Then
                        indexname = LCase(aTable.Name & "_primarykey")
                    Else
                        indexname = LCase(aTable.Name & "_" & indexname)
                    End If
                    anIndex = New Index(parent:=aTable, name:=indexname)
                    anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey
                    anIndex.IndexType = IndexType.NonClusteredIndex
                    anIndex.IgnoreDuplicateKeys = False
                    anIndex.IsUnique = True
                    'anIndex.Recreate()

                    '** extend Index -> Drop
                ElseIf Not primaryKey And existingIndex Then
                    anIndex.Drop()
                    If indexname = "" Then
                        indexname = LCase(aTable.Name & "_IND")
                    Else
                        indexname = LCase(aTable.Name & "_" & indexname)
                    End If
                    anIndex = New Index(parent:=aTable, name:=indexname)
                    anIndex.Name = indexname
                    anIndex.IndexKeyType = IndexKeyType.None
                    anIndex.IgnoreDuplicateKeys = True
                    anIndex.IsUnique = False
                    '** create new
                ElseIf Not primaryKey And Not existingIndex Then
                    If indexname = "" Then
                        indexname = LCase(aTable.Name & "_IND")
                    Else
                        indexname = LCase(aTable.Name & "_" & indexname)
                    End If
                    anIndex = New Index(parent:=aTable, name:=indexname)
                    anIndex.Name = indexname
                    anIndex.IndexKeyType = IndexKeyType.None
                    anIndex.IgnoreDuplicateKeys = True
                    anIndex.IsUnique = False
                End If

                _primaryConnection.IsNativeInternalLocked = False
                ' check on keys & indexes
                For Each aColumnname As String In columnNames
                    If Not IsNothing(aColumnname) Then
                        Dim indexColumn As IndexedColumn = New IndexedColumn(anIndex, aColumnname)
                        anIndex.IndexedColumns.Add(indexColumn)

                        ' check if containskey -> write
                        If addToSchemaDir And anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                            ' set it here -> bootstrapping will fail otherwise
                            aSchemaDir = New ObjectEntryDefinition
                            If Not aSchemaDir.LoadBy(aTable.Name, entryname:=aColumnname) Then
                                Call aSchemaDir.Create(aTable.Name, entryname:=aColumnname)
                            End If
                            aSchemaDir.Indexname = LCase(aTable.Name & "_" & indexname)
                            aSchemaDir.IndexPosition = i + 1
                            aSchemaDir.IsKey = True
                            aSchemaDir.IsPrimaryKey = True
                            Call aSchemaDir.Persist()
                        End If
                    Else
                        System.Diagnostics.Debug.WriteLine("Nothing ColumnName in getOTDBIndex List")
                    End If
                Next

                ' attach the Index
                If Not anIndex Is Nothing Then
                    anIndex.Create()
                    _primaryConnection.IsNativeInternalLocked = False
                    Return anIndex
                Else
                    _primaryConnection.IsNativeInternalLocked = False
                    Return Nothing
                End If


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

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, tablename:=aTable.Name, _
                                      subname:="clsMSSQLDriver.GetIndex", messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetIndex", arg1:=indexname, tablename:=aTable.Name, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
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
        Public Overrides Function HasColumn(tableID As String, columnname As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean

            
            If _primaryConnection Is Nothing Then
                Call CoreMessageHandler(subname:="clsMSSQLDriver.hasColumn", message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights
            If Not _primaryConnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, loginOnFailed:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.hasColumn", _
                                      message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If



            Dim aTable As Table
            Dim smoconnection As ServerConnection
            Dim database As Microsoft.SqlServer.Management.Smo.Database
            Try

                If nativeConnection Is Nothing Then
                    smoconnection = _primaryConnection.SMOConnection
                    database = _primaryConnection.Database
                Else
                    smoconnection = New ServerConnection(TryCast(nativeConnection, SqlConnection))
                    database = _primaryConnection.Database
                End If

                If smoconnection Is Nothing OrElse database Is Nothing Then
                    Call CoreMessageHandler(message:="SMO is not initialized", tablename:=tableID, _
                                          subname:="clsMSSQLDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    _primaryConnection.IsNativeInternalLocked = True
                End If


                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(message:="_primaryConnection is nothing - no table can be retrieved", subname:="clsMSQLDriver.hasTable", _
                                                messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                database.Tables.Refresh()
                Dim existsOnServer As Boolean = database.Tables.Contains(name:=tableID)
                If Not existsOnServer Then
                    Return False
                End If
                aTable = database.Tables.Item(tableID)
                _primaryConnection.IsNativeInternalLocked = True
                SyncLock _primaryConnection.NativeInternalConnection

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
                                      subname:="clsMSSQLDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.hasColumn", entryname:=columnname, tablename:=tableID, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                _primaryConnection.IsNativeInternalLocked = False
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
        Public Overrides Function GetColumn(nativeTable As Object, fieldDesc As ormFieldDescription, _
                                            Optional createOrAlter As Boolean = True, Optional addToSchemaDir As Boolean = True) As Object

            Dim anUser As New User
            Dim aSchema As New ObjectEntryDefinition

            If _primaryConnection Is Nothing Then
                Call CoreMessageHandler(subname:="clsMSSQLDriver.GetColumn", message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights
            If createOrAlter Then
                If LCase(fieldDesc.Tablename) <> LCase(anUser.TableID) And LCase(fieldDesc.Tablename) <> LCase(aSchema.TableID) Then
                    If Not _primaryConnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, loginOnFailed:=True) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetColumn", _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If


            '*** object
            If Not nativeTable.GetType = GetType(Table) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetColumn", _
                                             message:="No SMO TableObject given to funciton", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If
            Dim aTable As Table = DirectCast(nativeTable, Table)


            Dim aSchemaDir As New ObjectEntryDefinition
            Dim newColumn As Column

            Try
                ' Do While _primaryConnection.IsNativeInternalLocked
                'System.Threading.Thread.CurrentThread.Sleep(1000)
                'Loop

                _primaryConnection.IsNativeInternalLocked = True
                SyncLock _primaryConnection.NativeInternalConnection

                '**
                    If aTable.Columns.Count = 0 Then aTable.Refresh()

                    If aTable.Columns.Contains(name:=fieldDesc.ColumnName) Then
                        ' check if containskey -> write
                        If addToSchemaDir Then
                            ' set it here -> bootstrapping will fail otherwise
                            aSchemaDir = New ObjectEntryDefinition
                            If Not aSchemaDir.LoadBy(aTable.Name, entryname:=fieldDesc.ColumnName) Then
                                Call aSchemaDir.Create(aTable.Name, entryname:=fieldDesc.ColumnName)
                            End If
                            aSchemaDir.Typeid = otSchemaDefTableEntryType.Field
                            Call aSchemaDir.SetByFieldDesc(fieldDesc)
                            'aSchemaDir.isPrimaryKey = aDBDesc.OTDBPrimaryKeys
                            aSchemaDir.IsPrimaryKey = False
                            Call aSchemaDir.Persist()

                        End If
                        ' rturn and do not change !
                        _primaryConnection.IsNativeInternalLocked = False
                        Return aTable.Columns(name:=fieldDesc.ID)
                    End If
                '** create
                    If createOrAlter Then
                        newColumn = New Column(parent:=aTable, name:=fieldDesc.ColumnName)
                        Dim aDatatype As New DataType

                        Select Case fieldDesc.Datatype
                            Case otFieldDataType.[Long]
                                aDatatype.SqlDataType = SqlDataType.BigInt
                            Case otFieldDataType.Numeric
                                aDatatype.SqlDataType = SqlDataType.Real

                            Case otFieldDataType.List, otFieldDataType.Text
                                aDatatype.SqlDataType = SqlDataType.NVarChar
                                If fieldDesc.Size > 0 Then
                                    aDatatype.MaximumLength = fieldDesc.Size
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
                                Call CoreMessageHandler(subname:="clsMSSQLDriver.getColumn", tablename:=aTable.Name, arg1:=fieldDesc.ColumnName, _
                                                       message:="runtime, formular not supported as fieldtypes", messagetype:=otCoreMessageType.InternalError)

                        End Select
                        newColumn.DataType = aDatatype
                        ' default value
                        If Not fieldDesc.DefaultValue Is Nothing Then newColumn.Default = fieldDesc.DefaultValue
                        ' per default Nullable
                        If aTable.State = SqlSmoState.Creating Then
                            newColumn.Nullable = fieldDesc.IsNullable
                            ' SQL Server throws error if not nullable or default value on change
                        ElseIf fieldDesc.DefaultValue Is Nothing Then
                            newColumn.Nullable = True

                        End If


                        newColumn.ExtendedProperties.Refresh()
                        If newColumn.ExtendedProperties.Contains("MS_Description") Then
                            newColumn.ExtendedProperties("MS_Description").Value = fieldDesc.Title
                        Else
                            Dim newEP As ExtendedProperty = New ExtendedProperty(parent:=newColumn, name:="MS_Description", propertyValue:=fieldDesc.Title)
                            newColumn.ExtendedProperties.Add(newEP)
                            'newEP.Create() -> doesnot work

                        End If
                        ' add it
                        Call aTable.Columns.Add(newColumn)
                        _primaryConnection.IsNativeInternalLocked = False

                        ' check if containskey -> write
                        If addToSchemaDir Then
                            ' set it here -> bootstrapping will fail otherwise
                            aSchemaDir = New ObjectEntryDefinition
                            If Not aSchemaDir.LoadBy(aTable.Name, entryname:=fieldDesc.ColumnName) Then
                                Call aSchemaDir.Create(aTable.Name, entryname:=fieldDesc.ColumnName)
                            End If
                            aSchemaDir.Typeid = otSchemaDefTableEntryType.Field
                            Call aSchemaDir.SetByFieldDesc(fieldDesc)
                            'aSchemaDir.isPrimaryKey = aDBDesc.OTDBPrimaryKeys
                            aSchemaDir.IsPrimaryKey = False
                            Call aSchemaDir.Persist()

                        End If

                        ' rturn and do not change !

                        Return newColumn

                    Else
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetColumn", arg1:=fieldDesc.ID, tablename:=aTable.Name, _
                                                    message:="Column doensot exist", messagetype:=otCoreMessageType.InternalError)
                        ' rturn and do not change !
                        _primaryConnection.IsNativeInternalLocked = False
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

                Call CoreMessageHandler(message:=sb.ToString, exception:=ex, tablename:=aTable.Name, _
                                      subname:="clsMSSQLDriver.GetColumn", messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetColumn", arg1:=fieldDesc.ID, tablename:=aTable.Name, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                ' rturn and do not change !
                _primaryConnection.IsNativeInternalLocked = False
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
        Implements iormDBDriver.RunSqlStatement
            Dim anativeConnection As SqlConnection
            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    anativeConnection = DirectCast(Me.CurrentConnection, clsADONETConnection).NativeInternalConnection
                    If anativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.runSQLCommand", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.runSQLCommand", message:="Connection not available")
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
                Call CoreMessageHandler(subname:="clsMSSQLDriver.runSQLCommand", exception:=ex)
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
                    nativeConnection = DirectCast(_primaryConnection, clsMSSQLConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.setDBParameter", _
                                              message:="Native Internal Connection not available")
                        Return False
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.setDBParameter", _
                                          message:="Connection not available")
                    Return False
                End If

            End If


            '** init driver
            If Not Me.IsInitialized Then
                Me.Initialize()
            End If

            Try
                SyncLock _ParametersTableAdapter.UpdateCommand.Connection
                    dataRows = _ParametersTable.Select("[ID]='" & parametername & "'")

                    ' not found
                    If dataRows.GetLength(0) = 0 Then
                        If updateOnly And silent Then
                            SetDBParameter = False
                            Exit Function
                        ElseIf updateOnly And Not silent Then
                            Call CoreMessageHandler(showmsgbox:=True, _
                                                  message:="The Parameter '" & parametername & "' was not found in the OTDB Table tblParametersGlobal", subname:="clsMSSQLDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                            Return False
                        ElseIf Not updateOnly Then
                            ReDim dataRows(0)
                            dataRows(0) = _ParametersTable.NewRow
                            dataRows(0)("description") = ""

                            insertFlag = True
                        End If
                    End If

                    ' value
                    'dataRows(0).BeginEdit()
                    dataRows(0)("ID") = parametername
                    dataRows(0)("Value") = CStr(Value)
                    dataRows(0)("changedOn") = Date.Now().ToString
                    'dataRows(0).EndEdit()

                    '* add to table
                    If insertFlag Then
                        _ParametersTable.Rows.Add(dataRows(0))
                    End If

                    '*
                    Dim i = _ParametersTableAdapter.Update(_ParametersTable)
                    If i > 0 Then
                        _ParametersTable.AcceptChanges()
                        Return True
                    Else
                        Return False
                    End If

                End SyncLock
                
            Catch ex As Exception
                ' Handle the error

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsMSSQLDriver.setDBParameter", _
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
                        Call CoreMessageHandler(subname:="clsMSSQLDriver.getDBParameter", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsMSSQLDriver.getDBParameter", message:="Connection not available")
                    Return Nothing
                End If
            End If


            Try
                '** init driver
                If Not Me.IsInitialized Then
                    Me.Initialize()
                End If

                '** select row
                dataRows = _ParametersTable.Select("[ID]='" & parametername & "'")

                ' not found
                If dataRows.GetLength(0) = 0 Then
                    If silent Then
                        Return ""
                    ElseIf Not silent Then
                        Call CoreMessageHandler(showmsgbox:=True, _
                                              message:="The Parameter '" & parametername & "' was not found in the OTDB Table tblParametersGlobal", subname:="clsMSSQLDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing

                    End If
                End If

                ' value
                Return dataRows(0)("Value")

                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsMSSQLDriver.getDBParameter", tablename:="tblParametersGlobal", _
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

    '************************************************************************************
    '***** CLASS clsMSSQLConnection describes the Connection description to OnTrack
    '*****        based on ADO.NET OLEDB Driver
    '*****

    Public Class clsMSSQLConnection
        Inherits clsADONETConnection
        Implements iormConnection

        'Protected Friend Shadows _nativeConnection As SqlConnection
        'Protected Friend Shadows _nativeinternalConnection As SqlConnection

        '** SMO Objects
        Protected _SMOConnection As Microsoft.SqlServer.Management.Common.ServerConnection
        Protected _Server As Microsoft.SqlServer.Management.Smo.Server
        Protected _Database As Microsoft.SqlServer.Management.Smo.Database

        Public Shadows Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Shadows Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

        Public Sub New(ByVal id As String, ByRef DatabaseDriver As iormDBDriver, ByRef session As Session, sequence As ot.ConfigSequence)
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
        ''' Event Handler onInternalConnection
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function OnInternalConnection(sender As Object, arguments As InternalConnectionEventArgs) As Boolean _
            Handles MyBase.OnInternalConnected
            If _SMOConnection Is Nothing Then
                _SMOConnection = New Microsoft.SqlServer.Management.Common.ServerConnection()
                _SMOConnection.ServerInstance = DirectCast(_nativeinternalConnection, SqlConnection).DataSource
                _SMOConnection.SqlExecutionModes = SqlExecutionModes.ExecuteSql
                _SMOConnection.AutoDisconnectMode = AutoDisconnectMode.NoAutoDisconnect

            End If
            If Not _SMOConnection Is Nothing Then
                _Server = New Server(_SMOConnection)
                _Server.ConnectionContext.LoginSecure = False
                _Server.ConnectionContext.Login = Me._Dbuser
                _Server.ConnectionContext.Password = Me._Dbpassword
                _Server.Refresh()
                ' get the database
                If _Server.Databases.Contains(DirectCast(_nativeinternalConnection, SqlConnection).Database) Then
                    _Database = _Server.Databases(DirectCast(_nativeinternalConnection, SqlConnection).Database)
                Else
                    Call CoreMessageHandler(showmsgbox:=True, message:="Database " & Me.DBName & " is not existing on server " & _Server.Name, _
                                           messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="clsMSSQLConnection.OnInternalConnection")
                    _Database = Nothing

                End If
            Else
                Call CoreMessageHandler(message:="SMO Object for Database " & Me.DBName & " is not existing for server " & _Server.Name, break:=False, _
                                           messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="clsMSSQLConnection.OnInternalConnection")
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


    '************************************************************************************
    '***** CLASS clsMSSQLTableSchema  CLASS describes the schema per table of the database itself
    '*****        based on ADO.NET OLEDB Driver
    '*****

    ''' <summary>
    ''' CLASS describes the schema per table of the database itself
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsMSSQLTableSchema
        Inherits clsADONETTableSchema
        Implements iotTableSchema


        '***** internal variables
        '*****


        Public Sub New(ByRef connection As clsMSSQLConnection, ByVal tableID As String)
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
                                                          Optional parametername As String = "") As System.Data.IDbDataParameter Implements iotTableSchema.AssignNativeDBParameter
            Dim aDBColumnDescription As ColumnDescription = GetColumnDescription(Me.GetFieldordinal(fieldname))
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
                        Call CoreMessageHandler(subname:="clsMSSQLTableSchema.AssignNativeDBParameter", break:=False, message:="SqlDatatype not handled", _
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
                Call CoreMessageHandler(subname:="clsADONETTableSchema.buildParameter", message:="ColumnDescription couldn't be loaded", _
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
            Dim myConnection As clsMSSQLConnection = DirectCast(Me._Connection, clsMSSQLConnection)
            Dim aCon As SqlConnection = DirectCast(myConnection.NativeInternalConnection, SqlConnection)


            ' not working 
            If myConnection.Database Is Nothing OrElse Not myConnection.SMOConnection.IsOpen Then
                Call CoreMessageHandler(subname:="clsMSSQLTableSchema.refresh", _
                                     message:="SMO Connection is not open", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                _IsInitialized = False
                Return False
            End If
            ' return if no TableID
            If _TableID = "" Then
                Call CoreMessageHandler(subname:="clsMSSQLTableSchema.refresh", _
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
                    Call CoreMessageHandler(subname:="clsMSSQLTableSchema.refresh", _
                                     message:="Table couldnot be loaded from SMO", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    myConnection.IsNativeInternalLocked = False
                    _IsInitialized = False
                    Return False
                End If

                '** save the Table
                '**
                aTable.Refresh()
                myConnection.IsNativeInternalLocked = False
                If False Then
                    Call CoreMessageHandler(subname:="clsMSSQLTableSchema.refresh", _
                                     message:="Table couldnot initialized from SMO", _
                                     tablename:=TableID, messagetype:=otCoreMessageType.InternalError)
                    _IsInitialized = False
                    Return False
                End If

                no = aTable.Columns.Count
                If no = 0 Then
                    Call CoreMessageHandler(subname:="clsMSSQLTableSchema.refresh", _
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
                    _Columns(i) = New ColumnDescription
                    With _Columns(i)
                        .ColumnName = LCase(aColumn.Name)

                        aColumn.ExtendedProperties.Refresh()
                        If aColumn.ExtendedProperties.Contains("MS_Description") Then
                            .Description = aColumn.ExtendedProperties("MS_Description").Value
                        Else
                            .Description = ""
                        End If
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
                    anIndex.Refresh()

                    ' new
                    aColumnCollection = New ArrayList

                    For Each aColumn In anIndex.IndexedColumns

                        ' indx no
                        index = _fieldsDictionary.Item(aColumn.name)
                        '
                        '** check if primaryKey
                        'fill old primary Key structure
                        If anIndex.IndexKeyType = IndexKeyType.DriPrimaryKey Then
                            _PrimaryKeyIndexName = anIndex.Name.Clone
                            _NoPrimaryKeys = _NoPrimaryKeys + 1
                            ReDim Preserve _Primarykeys(0 To _NoPrimaryKeys - 1)
                            _Primarykeys(_NoPrimaryKeys - 1) = index - 1 ' set to the array 0...ubound
                        End If

                        aColumnCollection.Add(aColumn.name)

                    Next

                    '** store final

                    If _indexDictionary.ContainsKey(anIndex.Name) Then
                        _indexDictionary.Remove(key:=anIndex.Name)
                    End If
                    _indexDictionary.Add(key:=anIndex.Name, value:=aColumnCollection)
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
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsMSSQLTableSchema.refresh", tablename:=_TableID, _
                                      arg1:=reloadForce, exception:=ex)

                _IsInitialized = False
                Return False
            End Try

        End Function

    End Class

    '************************************************************************************
    '***** CLASS clsMSSQLTableStore describes the per Table reference and Helper Class
    '*****                    ORM Mapping Class and Table Access Workhorse
    '*****

    Public Class clsMSSQLTableStore
        Inherits clsADONETTableStore
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
        Public Overrides Function Convert2ColumnData(ByVal value As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "") As Object _
                                                Implements iormDataStore.Convert2ColumnData
            Return Me.Connection.DatabaseDriver.Convert2DBData(value:=value, targetType:=targetType, maxsize:=maxsize, abostrophNecessary:=abostrophNecessary, _
                                       fieldname:=fieldname)
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
        Public Overrides Function Convert2ObjectData(ByVal index As Object, ByVal value As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object _
        Implements iormDataStore.Convert2ObjectData
            Dim aSchema As clsMSSQLTableSchema = Me.TableSchema
            Dim aDBColumn As clsMSSQLTableSchema.ColumnDescription
            Dim result As Object = Nothing
            Dim fieldno As Integer

            Try

                fieldno = aSchema.GetFieldordinal(index)
                If fieldno < 0 Then
                    Call CoreMessageHandler(subname:="clsOLEDBTableStoreStore.cvt2ColumnData", messagetype:=otCoreMessageType.InternalError, _
                                          message:="iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found", _
                                          tablename:=Me.TableID, arg1:=index)
                    System.Diagnostics.Debug.WriteLine("iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found")

                    Return DBNull.Value
                Else
                    aDBColumn = aSchema.GetColumnDescription(fieldno)
                End If
                abostrophNecessary = False

                '*
                '*
                'If IsError(aValue) Then
                '    System.Diagnostics.Debug.WriteLine "Error in Formular of field value " & aValue & " while updating OTDB"
                '    aValue = ""
                'End If

                If aDBColumn.DataType = SqlDataType.BigInt Or aDBColumn.DataType = SqlDataType.Int _
              Or aDBColumn.DataType = SqlDataType.SmallInt Or aDBColumn.DataType = SqlDataType.TinyInt Then
                    If (Not IsNumeric(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CLng(value)
                    Else
                        Call CoreMessageHandler(subname:="clsMSSQLTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Integer", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = DBNull.Value
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Char Or aDBColumn.DataType = SqlDataType.NText _
                     Or aDBColumn.DataType = SqlDataType.VarChar Or aDBColumn.DataType = SqlDataType.Text _
                      Or aDBColumn.DataType = SqlDataType.NVarChar Or aDBColumn.DataType = SqlDataType.VarCharMax _
                      Or aDBColumn.DataType = SqlDataType.NVarCharMax Then
                    abostrophNecessary = True
                    If (value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = ""
                    Else
                        result = CStr(value)
                    End If

                ElseIf aDBColumn.DataType = SqlDataType.Date Or aDBColumn.DataType = SqlDataType.SmallDateTime Or aDBColumn.DataType = SqlDataType.Time _
                Or aDBColumn.DataType = SqlDataType.Timestamp Or aDBColumn.DataType = SqlDataType.DateTime Or aDBColumn.DataType = SqlDataType.DateTime2 _
                Or aDBColumn.DataType = SqlDataType.DateTimeOffset Then

                    If (Not IsDate(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = ConstNullDate
                    ElseIf IsDate(value) Then
                        result = CDate(value)
                    Else
                        Call CoreMessageHandler(subname:="clsMSSQLTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Date", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = ConstNullDate
                    End If
                ElseIf aDBColumn.DataType = SqlDataType.Float Or aDBColumn.DataType = SqlDataType.Decimal _
               Or aDBColumn.DataType = SqlDataType.Real Then
                    If (Not IsNumeric(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CDbl(value)
                    Else
                        Call CoreMessageHandler(subname:="clsMSSQLTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Double", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = DBNull.Value
                    End If
                ElseIf aDBColumn.DataType = SqlDataType.Bit Then
                    If (value Is Nothing OrElse DBNull.Value.Equals(value) OrElse IsError(value) OrElse value = False) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = False
                    Else
                        result = True
                    End If

                End If

                ' return
                Return result
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsMSSQLTableStore.cvt2ObjData", _
                                      arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName, exception:=ex, _
                                      messagetype:=otCoreMessageType.InternalError)
                Return Nothing
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
                    ' set theAdapter
                    _cacheAdapter = New SqlDataAdapter
                    MyBase._cacheAdapter = _cacheAdapter
                    _cacheAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                    aDataSet = DirectCast(Me.Connection.DatabaseDriver, clsMSSQLDriver).OnTrackDataSet
                    ' Select Command
                    aCommand = DirectCast(Me.TableSchema, clsMSSQLTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsADONETTableSchema.CommandType.SelectType)
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
                        _cacheAdapter.SelectCommand.Connection = DirectCast(Me.Connection.NativeConnection, SqlConnection)
                        _cacheAdapter.FillSchema(aDataSet, SchemaType.Source)
                        DirectCast(_cacheAdapter, SqlDataAdapter).Fill(aDataSet, Me.TableID)
                        ' set the Table
                        _cacheTable = aDataSet.Tables(Me.TableID)
                        If _cacheTable Is Nothing Then
                            Debug.Assert(False)
                        End If

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
                    aCommand = DirectCast(Me.TableSchema, clsMSSQLTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsADONETTableSchema.CommandType.DeleteType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.DeleteCommand = aCommand
                    End If

                    ' Insert Command
                    aCommand = DirectCast(Me.TableSchema, clsMSSQLTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsADONETTableSchema.CommandType.InsertType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.InsertCommand = aCommand
                    End If
                    ' Update Command
                    aCommand = DirectCast(Me.TableSchema, clsMSSQLTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsADONETTableSchema.CommandType.UpdateType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.UpdateCommand = aCommand
                    End If

                    '** return true
                    Return True
                Else
                    Return False
                End If



            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsMSSQLTableStore.initializeCache", exception:=ex, message:="Exception", _
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
                Call CoreMessageHandler(message:="Exception occured", subname:="clsMSSQLTableStore.UpdateDBDataTable", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return 0
            End Try

        End Function
    End Class

End Namespace
