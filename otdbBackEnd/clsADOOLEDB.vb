REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Driver Wrapper for ADO.NET OLEDB Classes for On Track Database Back end Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Option Explicit Off

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Data
Imports System.Data.OleDb
Imports System.Linq
Imports System.Text.RegularExpressions

Imports otdb
Imports OnTrack

Namespace OnTrack.Database


    '************************************************************************************
    '***** CLASS clsOLEDBDriver describes the  Database Driver  to OnTrack
    '*****       based on ADO.NET OLEDB
    '*****
    ''' <summary>
    ''' clsOLEDBDriver is the database driver for ADO.NET OLEDB drivers
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOLEDBDriver
        Inherits clsADONETDBDriver
        Implements iormDBDriver

        Protected Friend Shadows WithEvents _primaryConnection As clsOLEDBConnection '-> in clsOTDBDriver
        Private Shadows _ParametersTableAdapter As New OleDbDataAdapter

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="ID">an ID for this driver</param>
        ''' <remarks></remarks>
        Public Sub New(id As String, ByRef session As Session)
            Call MyBase.New(id, session)
            If Me._primaryConnection Is Nothing Then
                _primaryConnection = New clsOLEDBConnection(id:="primary", DatabaseDriver:=Me, session:=session, sequence:=ConfigSequence.primary)
            End If
        End Sub


        ''' <summary>
        ''' NativeConnection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads ReadOnly Property NativeConnection() As OleDbConnection
            Get
                Return DirectCast(_primaryConnection.NativeConnection, OleDbConnection)
            End Get

        End Property
        Private Function BuildParameterAdapter()

            With _ParametersTableAdapter


                .SelectCommand.Prepare()

                ' Create the commands.
                '**** INSERT
                .InsertCommand = New OleDbCommand( _
                "INSERT INTO " & _parametersTableName & " (ID, [Value], changedOn, description) " & _
                "VALUES (?, ?, ?, ?)")
                ' Create the parameters.
                .InsertCommand.Parameters.Add( _
                "@ID", OleDbType.Char, 50, "ID")
                .InsertCommand.Parameters.Add( _
                "@Value", OleDbType.VarChar, 250, "Value")
                .InsertCommand.Parameters.Add( _
                "@changedOn", OleDbType.VarChar, 50, "changedOn")
                .InsertCommand.Parameters.Add( _
                "@description", OleDbType.VarChar, 250, "description")
                .InsertCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection)
                .InsertCommand.Prepare()


                '**** UPDATE
                .UpdateCommand = New OleDbCommand( _
                "UPDATE " & _parametersTableName & " SET [Value] = ? , changedOn = ? , description =?  " & _
                "WHERE ID = ?")
                ' Create the parameters.
                .UpdateCommand.Parameters.Add( _
                "@Value", OleDbType.VarChar, 250, "Value")
                .UpdateCommand.Parameters.Add( _
                "@changedOn", OleDbType.VarChar, 50, "changedOn")
                .UpdateCommand.Parameters.Add( _
                "@description", OleDbType.VarChar, 250, "description")
                .UpdateCommand.Parameters.Add( _
                "@ID", OleDbType.Char, 50, "ID").SourceVersion = _
                    DataRowVersion.Original
                .UpdateCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection)
                .UpdateCommand.Prepare()


                '***** DELETE
                .DeleteCommand = New OleDbCommand( _
                "DELETE FROM " & _parametersTableName & " WHERE ID = ?")
                .DeleteCommand.Parameters.Add( _
                "@ID", OleDbType.Char, 50, "ID").SourceVersion = _
                    DataRowVersion.Original
                .DeleteCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection)
                .DeleteCommand.Prepare()

            End With

        End Function
        '***
        '*** Initialize Driver
        Protected Friend Function Initialize(Optional Force As Boolean = False) As Boolean

            If Me.IsInitialized And Not Force Then
                Return True
            End If

            Try
                Call MyBase.Initialize()

                ' we have no Connection ?!
                If _primaryConnection Is Nothing Then
                    _primaryConnection = New clsOLEDBConnection("primary", Me, _session, ConfigSequence.primary)
                End If
                '*** set the DataTable
                _OnTrackDataSet = New DataSet("onTrackSession -" & Date.Now.ToString)
                ' the command
                Dim aDBCommand = New OleDbCommand()
                aDBCommand.CommandText = "select ID, [Value], changedOn, description from " & _parametersTableName
                aDBCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection)
                ' fill with adapter
                _ParametersTableAdapter = New OleDbDataAdapter()
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
                Call CoreMessageHandler(subname:="clsOLEDBDriver.OnConnection", message:="couldnot Initialize Driver", _
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
                Return otDbDriverType.ADONETOLEDB
            End Get
        End Property
        ''' <summary>
        ''' create a new TableStore for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function createNativeTableStore(ByVal TableID As String, ByVal forceSchemaReload As Boolean) As iormDataStore
            Return New clsOLEDBTableStore(Me.CurrentConnection, TableID, forceSchemaReload)
        End Function
        ''' <summary>
        ''' create a new TableSchema for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function createNativeTableSchema(ByVal TableID As String) As iotDataSchema
            Return New clsOLEDBTableSchema(Me.CurrentConnection, TableID)
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CreateNativeDBCommand(commandstr As String, nativeConnection As IDbConnection) As IDbCommand Implements iormDBDriver.CreateNativeDBCommand
            Return New OleDbCommand(commandstr, nativeConnection)
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
                        Return OleDbType.Binary
                    Case otFieldDataType.Bool
                        Return OleDbType.Boolean
                    Case otFieldDataType.[Date]
                        Return OleDbType.Date
                    Case otFieldDataType.Time
                        Return OleDbType.DBTime
                    Case otFieldDataType.List
                        Return OleDbType.LongVarWChar
                    Case otFieldDataType.[Long]
                        Return OleDbType.BigInt
                    Case otFieldDataType.Memo
                        Return OleDbType.LongVarWChar
                        Return OleDbType.Decimal
                    Case otFieldDataType.Timestamp
                        Return OleDbType.DBTimeStamp
                    Case otFieldDataType.Text
                        Return OleDbType.LongVarWChar
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
                    Call CoreMessageHandler(subname:="clsOLEDBTablestore.cvt2ColumnData", _
                                          message:="Error in Formular of field value " & value & " while updating OTDB", _
                                          arg1:=value, messagetype:=otCoreMessageType.InternalError)
                    System.Diagnostics.Debug.WriteLine("Error in Formular of field value " & value & " while updating OTDB")
                    value = ""
                End If

                If targetType = OleDbType.BigInt Or targetType = OleDbType.Integer _
                      Or targetType = OleDbType.SmallInt Or targetType = OleDbType.TinyInt _
                      Or targetType = OleDbType.UnsignedBigInt Or targetType = OleDbType.UnsignedInt _
                      Or targetType = OleDbType.UnsignedSmallInt Or targetType = OleDbType.UnsignedTinyInt _
                      Or targetType = OleDbType.SmallInt Or targetType = OleDbType.TinyInt Then

                    If value Is Nothing OrElse IsError(value) OrElse DBNull.Value.Equals(value) _
                        OrElse String.IsNullOrWhiteSpace(value.ToString) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CLng(value)
                    Else
                        Call CoreMessageHandler(subname:="clsOLEDBTableStore.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Integer", _
                                              arg1:=value, messagetype:=otCoreMessageType.InternalError)
                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Integer")
                        result = DBNull.Value
                    End If

                ElseIf targetType = OleDbType.Char Or targetType = OleDbType.BSTR Or targetType = OleDbType.LongVarChar _
                Or targetType = OleDbType.LongVarWChar Or targetType = OleDbType.VarChar Or targetType = OleDbType.VarWChar _
                Or targetType = OleDbType.WChar Then
                    abostrophNecessary = True

                    If value Is Nothing OrElse IsError(value) OrElse DBNull.Value.Equals(value) OrElse String.IsNullOrWhiteSpace(value.ToString) Then
                        result = ""
                    Else
                        If maxsize < Len(CStr(value)) And maxsize <> 0 Then
                            result = Mid(CStr(value), 1, maxsize - 1)
                        Else
                            result = CStr(value)
                        End If
                    End If

                ElseIf targetType = OleDbType.Date Or targetType = OleDbType.DBDate Or targetType = OleDbType.DBTime _
                Or targetType = OleDbType.DBTimeStamp Then
                    If value Is Nothing OrElse IsError(value) Or DBNull.Value.Equals(value) _
                        OrElse String.IsNullOrWhiteSpace(value.ToString) Then
                        result = ConstNullDate
                    ElseIf IsDate(value) Then
                        result = CDate(value)
                    ElseIf value.GetType = GetType(TimeSpan) Then
                        result = value
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Date")
                        Call CoreMessageHandler(subname:="clsOLEDBTableStore.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Date", _
                                              arg1:=value, messagetype:=otCoreMessageType.InternalError)
                        result = ConstNullDate
                    End If
                ElseIf targetType = OleDbType.Double Or targetType = OleDbType.Decimal _
                Or targetType = OleDbType.Single Or targetType = OleDbType.Numeric Then
                    If value Is Nothing OrElse IsError(value) Or DBNull.Value.Equals(value) _
                       OrElse String.IsNullOrWhiteSpace(value.ToString) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CDbl(value)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & value & " is not convertible to Double")
                        Call CoreMessageHandler(subname:="clsOLEDBTableStore.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & value & " is not convertible to Double", _
                                              arg1:=targetType, messagetype:=otCoreMessageType.InternalError)
                        result = DBNull.Value
                    End If
                ElseIf targetType = OleDbType.Boolean Then
                    If value Is Nothing OrElse IsError(value) OrElse DBNull.Value.Equals(value) _
                       OrElse String.IsNullOrWhiteSpace(value.ToString) OrElse (IsNumeric(value) AndAlso value = 0) Then
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
                Call CoreMessageHandler(message:="Exception", subname:="clsOLEDBTablestore.convert2ColumnData(Object, long ..", _
                                       exception:=ex, messagetype:=otCoreMessageType.InternalException)
                Return Nothing
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
                Dim aParameter As New OleDbParameter()

                aParameter.ParameterName = parametername
                aParameter.OleDbType = GetTargetTypeFor(datatype)
                Select Case datatype
                 
                    Case otFieldDataType.Bool
                        aParameter.Value = False
                    Case otFieldDataType.[Date]
                        aParameter.Value = ConstNullDate
                    Case otFieldDataType.Time
                        aParameter.Value = ot.ConstNullTime
                    Case otFieldDataType.List
                        If maxsize = 0 Then aParameter.Size = Const_MaxTextSize
                        aParameter.Value = ""
                    Case otFieldDataType.[Long]
                        aParameter.Value = 0
                    Case otFieldDataType.Memo
                        If maxsize = 0 Then aParameter.Size = Const_MaxMemoSize
                        aParameter.Value = ""
                    Case otFieldDataType.Numeric
                        aParameter.Value = 0
                    Case otFieldDataType.Timestamp
                        aParameter.Value = ConstNullDate
                    Case otFieldDataType.Text
                        If maxsize = 0 Then aParameter.Size = Const_MaxTextSize
                        aParameter.Value = ""

                End Select
                If Not value Is Nothing Then
                    aParameter.Value = value
                End If
                Return aParameter
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOLEDBDriver.assignDBParameter", message:="Exception", exception:=ex, _
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
        ''' returns True if the tablename exists in the datastore
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasTable(tableid As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
            Dim myConnection As clsOLEDBConnection
            Dim aSchemaDir As ObjectDefinition
            Dim aTable As DataTable


            '* if already loaded
            If _TableDirectory.ContainsKey(key:=tableid) Then Return True



            '* check rights
            If LCase(tableid) <> LCase(User.ConstTableID) And LCase(tableid) <> LCase(ObjectDefinition.ConstTableID) Then
                If Not _primaryConnection.VerifyUserAccess(otAccessRight.[ReadOnly], loginOnFailed:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="clsOLEDBDriver.HasTable", tablename:=tableid, _
                                          message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return False
                End If
            End If

            'If _primaryConnection Is Nothing OrElse Not _primaryConnection.IsConnected Then
            '    Call CoreMessageHandler(subname:="clsOLEDBDriver.HasTable", tablename:=tableid, _
            '                         message:="not connected to database", messagetype:=otCoreMessageType.InternalError)
            '    Return fase
            'End If

            Try
                myConnection = DirectCast(_primaryConnection, clsOLEDBConnection)
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                If nativeConnection Is Nothing Then
                    aTable = DirectCast(myConnection.NativeInternalConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                Else
                    aTable = DirectCast(nativeConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                End If

                If aTable.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' Gets the table.
        ''' </summary>
        ''' <param name="tablename">The tablename.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <returns></returns>
        Public Overrides Function GetTable(tableid As String, _
                                           Optional createOnMissing As Boolean = True, _
                                           Optional addToSchemaDir As Boolean = True, _
                                           Optional ByRef nativeConnection As Object = Nothing, _
                                            Optional ByRef nativeTableObject As Object = Nothing) As Object

            '*** check on rights
            If createOnMissing Then
                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.GetTable", tablename:=tableid, _
                                          message:="No current Connection to the Database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                Else
                    If Not _primaryConnection.VerifyUserAccess(otAccessRight.AlterSchema, loginOnFailed:=True) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsOLEDBDriver.GetTable", tablename:=tableid, _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If



            Dim myConnection As clsOLEDBConnection
            Dim aSchemaDir As ObjectDefinition
            Dim aTable As DataTable
            Dim aStatement As String = ""

            Try
                myConnection = DirectCast(_primaryConnection, clsOLEDBConnection)
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                If nativeConnection Is Nothing Then
                    aTable = DirectCast(myConnection.NativeInternalConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                Else
                    aTable = DirectCast(nativeConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                End If



                '** create the table
                '**
                If aTable.Rows.Count = 0 And createOnMissing Then

                    aStatement = "CREATE TABLE " & tableid & " ( tttemp  bit )"
                    Me.RunSqlStatement(aStatement, _
                                       nativeConnection:=DirectCast(myConnection.NativeInternalConnection, OleDbConnection))

                    aTable = DirectCast(myConnection.NativeInternalConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)

                    ' check if containskey -> write
                    If addToSchemaDir Then
                        ' set it here -> bootstrapping will fail otherwise
                        aSchemaDir = New ObjectDefinition
                        Call aSchemaDir.Create(tableid)
                        Call aSchemaDir.Persist()
                    End If
                    Return aTable

                ElseIf aTable.Rows.Count > 0 Then
                    'Dim columnRow As System.Data.DataRow
                    '** select
                    Dim columnsList = From columnRow In aTable.AsEnumerable _
                                      Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                      [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME") _
                                      Where [ColumnName] = "tttemp"



                    If columnsList.Count > 0 Then
                        Me.RunSqlStatement(sqlcmdstr:="ALTER TABLE [" & tableid & "] DROP [tttemp]")
                    End If
                    Return aTable
                Else
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.getTable", tablename:=tableid, _
                                          message:="Table was not found in database", messagetype:=otCoreMessageType.ApplicationWarning)
                    Return Nothing
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
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
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>
        Public Overrides Function GetIndex(ByRef nativeTABLE As Object, _
                                           ByRef indexname As String, _
                                           ByRef ColumnNames As List(Of String), _
                                           Optional PrimaryKey As Boolean = False, _
                                           Optional forceCreation As Boolean = False, _
                                           Optional createOnMissing As Boolean = True, _
                                           Optional addToSchemaDir As Boolean = True) As Object
            Dim aTable As DataTable = TryCast(nativeTABLE, DataTable)
            Dim atableid As String = ""

            '** no object ?!
            If aTable Is Nothing Then
                Return Nothing
            End If

            '*** check on rights
            If createOnMissing Then
                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.GetIndex", tablename:=atableid, _
                                          message:="No current Connection to the Database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                Else
                    If Not _primaryConnection.VerifyUserAccess(otAccessRight.AlterSchema, loginOnFailed:=True) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsOLEDBDriver.GetIndex", tablename:=atableid, _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If


            Dim newindexname As String = indexname.Clone

            Dim aSchemaDir As ObjectEntryDefinition
            Dim aStatement As String = ""

            Dim anIndexTable As DataTable
            Dim existingIndex As Boolean = False
            Dim indexnotchanged As Boolean = False
            Dim existingprimaryName As String = ""
            Dim existingIndexName As String = ""
            Dim isprimaryKey As Boolean = False
            Dim i As UShort = 0

            Try
                '** awkwar get the tableid
                Dim tableidList = From columnRow In aTable.AsEnumerable _
                     Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                     Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                     DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                     [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                     Description = columnRow.Field(Of String)("DESCRIPTION"), _
                     CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                     IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE")

                If tableidList.Count = 0 Then
                    Call CoreMessageHandler(message:="atableid couldn't be retrieved from nativetable object", subname:="clsOLEDBDriver.getIndex", _
                                                 tablename:=atableid, arg1:=indexname, messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    atableid = tableidList(0).TableName
                End If
                '** read indixes
                Dim restrictionsIndex() As String = {Nothing, Nothing, Nothing, Nothing, atableid}
                anIndexTable = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection). _
                                                GetSchema("INDEXES", restrictionsIndex)

                Dim columnsIndexList = From indexRow In anIndexTable.AsEnumerable _
                                        Select TableName = indexRow.Field(Of String)("TABLE_NAME"), _
                                        theIndexName = indexRow.Field(Of String)("INDEX_NAME"), _
                                        Columnordinal = indexRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                        ColumnName = indexRow.Field(Of String)("COLUMN_NAME"), _
                                        IndexisPrimaryKey = indexRow.Field(Of Boolean)("PRIMARY_KEY") _
                                        Where [ColumnName] <> "" And TableName <> "" And Columnordinal > 0 And (theIndexName = newindexname Or theIndexName = atableid & "_" & newindexname) _
                                        Order By TableName, newindexname, Columnordinal, ColumnName

                Dim primaryIndexList = From indexRow In anIndexTable.AsEnumerable _
                Select TableName = indexRow.Field(Of String)("TABLE_NAME"), _
                theIndexName = indexRow.Field(Of String)("INDEX_NAME"), _
                Columnordinal = indexRow.Field(Of Int64)("ORDINAL_POSITION"), _
                ColumnName = indexRow.Field(Of String)("COLUMN_NAME"), _
                IndexisPrimaryKey = indexRow.Field(Of Boolean)("PRIMARY_KEY") _
                Where [ColumnName] <> "" And TableName <> "" And Columnordinal > 0 And IndexisPrimaryKey = True _
                Order By TableName, newindexname, Columnordinal, ColumnName

                If primaryIndexList.Count > 0 Then
                    existingprimaryName = primaryIndexList(0).theIndexName
                End If

                If columnsIndexList.Count = 0 And Not createOnMissing Then
                    Return Nothing
                ElseIf columnsIndexList.Count = 0 And createOnMissing Then
                    existingIndex = False
                    indexnotchanged = False
                ElseIf Not forceCreation Then
                    i = 0
                    ' get an list
                    Dim anIndexColumnsList As New List(Of String)
                    For Each anIndex In columnsIndexList
                        anIndexColumnsList.Add(anIndex.ColumnName)
                        If anIndex.IndexisPrimaryKey Then
                            isprimaryKey = True
                        End If
                        existingIndexName = anIndex.theIndexName
                    Next
                    ' go through
                    For Each columnName As String In ColumnNames
                        If LCase(anIndexColumnsList.Item(i)) <> LCase(columnName) Then
                            indexnotchanged = False
                            Exit For
                        Else
                            indexnotchanged = True
                            ' check if containskey -> write
                            If addToSchemaDir And PrimaryKey Then
                                ' set it here -> bootstrapping will fail otherwise
                                aSchemaDir = New ObjectEntryDefinition
                                If Not aSchemaDir.LoadBy(atableid, entryname:=columnName) Then
                                    Call aSchemaDir.Create(atableid, entryname:=columnName)
                                End If
                                aSchemaDir.Indexname = LCase(atableid & "_" & indexname)
                                aSchemaDir.IndexPosition = i + 1
                                aSchemaDir.IsKey = True
                                aSchemaDir.IsPrimaryKey = True
                                Call aSchemaDir.Persist()
                            End If
                        End If
                        'Next j

                        ' exit
                        If Not indexnotchanged Then
                            Exit For
                        End If
                        i = i + 1
                    Next columnName
                    '** check if primary is different
                    If PrimaryKey <> isprimaryKey Or forceCreation Then
                        indexnotchanged = False
                    End If
                    ' return
                    If indexnotchanged Then
                        Return columnsIndexList
                    End If
                End If


                '** drop existing

                If (isprimaryKey Or PrimaryKey) And existingprimaryName <> "" Then
                    aStatement = " ALTER TABLE " & atableid & " DROP CONSTRAINT [" & existingprimaryName & "]"
                    Me.RunSqlStatement(aStatement)
                ElseIf existingIndex Then
                    aStatement = " DROP INDEX " & existingIndex
                    Me.RunSqlStatement(aStatement)
                End If



                '*** build new
                If PrimaryKey Then
                    aStatement = " ALTER TABLE [" & atableid & "] ADD CONSTRAINT [" & atableid & "_" & indexname & "] PRIMARY KEY ("
                    Dim comma As Boolean = False
                    For Each name As String In ColumnNames
                        If comma Then aStatement &= ","
                        aStatement &= "[" & name & "]"
                        comma = True
                    Next
                    aStatement &= ")"
                    Me.RunSqlStatement(aStatement)
                Else
                    aStatement = " CREATE INDEX [" & atableid & "_" & indexname & "] ON [" & atableid & "] ("
                    Dim comma As Boolean = False
                    For Each name As String In ColumnNames
                        If comma Then aStatement &= ","
                        aStatement &= "[" & name & "]"
                        comma = True
                    Next
                    aStatement &= ")"
                    Me.RunSqlStatement(aStatement)
                End If

                '** read indixes

                anIndexTable = DirectCast(_primaryConnection.NativeInternalConnection, OleDbConnection). _
                                                GetSchema("INDEXES", restrictionsIndex)


                Dim columnsResultIndexList = From indexRow In anIndexTable.AsEnumerable Select TableName = indexRow.Field(Of String)("TABLE_NAME"), _
                                             theIndexName = indexRow.Field(Of String)("INDEX_NAME"), _
                                             Columnordinal = indexRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                             ColumnName = indexRow.Field(Of String)("COLUMN_NAME"), _
                                            IndexisPrimaryKey = indexRow.Field(Of Boolean)("PRIMARY_KEY") _
                                            Where [ColumnName] <> "" And TableName <> "" And Columnordinal > 0 And (theIndexName = newindexname Or theIndexName = atableid & "_" & newindexname) _
                                            Order By TableName, newindexname, Columnordinal, ColumnName

                If columnsResultIndexList.Count > 0 Then
                    Return columnsResultIndexList
                Else
                    Call CoreMessageHandler(message:="creation of index failed", arg1:=indexname, _
                                                 subname:="clsOLEDBDriver.getIndex", tablename:=atableid, _
                                                 messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetIndex", arg1:=indexname, tablename:=atableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsMSSQLDriver.GetIndex", arg1:=indexname, tablename:=atableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                _primaryConnection.IsNativeInternalLocked = False
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' returns True if the table id has the Column
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasColumn(tableid As String, columnname As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
            Dim myConnection As clsOLEDBConnection
            Dim aSchemaDir As ObjectDefinition
            Dim aTable As DataTable

            If Not _primaryConnection.VerifyUserAccess(otAccessRight.[ReadOnly], loginOnFailed:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsOLEDBDriver.HasTable", tablename:=tableid, _
                                      message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            Try
                myConnection = DirectCast(_primaryConnection, clsOLEDBConnection)
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                If nativeConnection Is Nothing Then
                    aTable = DirectCast(myConnection.NativeInternalConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                Else
                    aTable = DirectCast(nativeConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                End If

                '** select
                Dim columnsResultList = From columnRow In aTable.AsEnumerable _
                                       Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                       [FieldName] = columnRow.Field(Of String)("COLUMN_NAME") _
                                       Where [FieldName] = columnname

                If columnsResultList.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="clsOLEDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try


        End Function
        ''' <summary>
        ''' Gets the column.
        ''' </summary>
        ''' <param name="nativeTABLE">The native TABLE.</param>
        ''' <param name="aDBDesc">A DB desc.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="addToSchemaDir">The add to schema dir.</param>
        ''' <returns></returns>
        Public Overrides Function GetColumn(nativeTABLE As Object, _
                                            fielddesc As ormFieldDescription, _
                                            Optional createOnMissing As Boolean = True, _
                                            Optional addToSchemaDir As Boolean = True) As Object


            Dim aTable As DataTable = TryCast(nativeTABLE, DataTable)
            Dim atableid As String = ""

            '** no object ?!
            If aTable Is Nothing Then
                Return Nothing
            End If

            '*** check on rights
            If createOnMissing Then
                If _primaryConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.GetTable", tablename:=atableid, _
                                          message:="No current Connection to the Database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                Else
                    If Not _primaryConnection.VerifyUserAccess(otAccessRight.AlterSchema, loginOnFailed:=True) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsOLEDBDriver.GetTable", tablename:=atableid, _
                                              message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing
                    End If
                End If
            End If



            Dim myConnection As clsOLEDBConnection
            Dim aSchemaDir As ObjectDefinition

            Dim aStatement As String = ""

            Try
                myConnection = DirectCast(_primaryConnection, clsOLEDBConnection)

                '** select
                Dim columnsList = From columnRow In aTable.AsEnumerable _
                               Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                               Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                               DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                               [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                               Description = columnRow.Field(Of String)("DESCRIPTION"), _
                               CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                               IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                               Where [ColumnName] = fielddesc.ColumnName

                If columnsList.Count > 0 Then
                    Return columnsList
                End If

                '** create the column
                '**
                If columnsList.Count = 0 And createOnMissing Then

                    Dim tableidList = From columnRow In aTable.AsEnumerable _
                                          Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                          Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                          DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                                          [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                                          Description = columnRow.Field(Of String)("DESCRIPTION"), _
                                          CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                                          IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE")

                    If tableidList.Count = 0 Then
                        Call CoreMessageHandler(message:="atableid couldn't be retrieved from nativetable object", subname:="clsOLEDBDriver.getColumn", _
                                                     tablename:=atableid, entryname:=fielddesc.ColumnName, messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else
                        atableid = tableidList(0).TableName
                    End If

                    aStatement = "ALTER TABLE " & atableid & " ADD COLUMN [" & fielddesc.ColumnName & "] "

                    Select Case fielddesc.Datatype
                        Case otFieldDataType.Bool
                            aStatement &= " BIT "
                        Case otFieldDataType.Binary
                            aStatement &= " BINARY VARYING"
                        Case otFieldDataType.Date
                            aStatement &= " DATE "
                        Case otFieldDataType.Long
                            If Me.DatabaseType = otDBServerType.Access Then
                                aStatement &= " INTEGER "
                            Else
                                aStatement &= " BIG INT "
                            End If

                        Case otFieldDataType.Memo
                            If Me.DatabaseType = otDBServerType.Access Then
                                aStatement &= " MEMO "
                            Else
                                aStatement &= " NVARCHAR(" & Const_MaxMemoSize.ToString & ")"
                            End If
                        Case otFieldDataType.Numeric
                            aStatement &= " FLOAT "
                        Case otFieldDataType.Text, otFieldDataType.List
                            aStatement &= " NVARCHAR("
                            If fielddesc.Size = 0 Then
                                aStatement &= Const_MaxTextSize.ToString & ")"
                            Else
                                aStatement &= fielddesc.Size.ToString & ")"
                            End If

                        Case otFieldDataType.Timestamp
                            aStatement &= " TIMESTAMP "
                        Case otFieldDataType.Time
                            aStatement &= " TIME "
                        Case Else
                            Call CoreMessageHandler(message:="Datatype is not implemented", tablename:=atableid, entryname:=fielddesc.ColumnName, _
                                                         subname:="clsOLEDBDriver.getColumn", arg1:=fielddesc.Datatype.ToString, _
                                                         messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                    End Select

                    If fielddesc.IsNullable Then
                        aStatement &= " NULL "
                    Else
                        aStatement &= " NOT NULL "
                    End If

                    '** Run it
                    Me.RunSqlStatement(aStatement, _
                                       nativeConnection:=DirectCast(myConnection.NativeInternalConnection, OleDbConnection))

                    Dim restrictionsTable() As String = {Nothing, Nothing, atableid}
                    aTable = DirectCast(myConnection.NativeInternalConnection, OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                    '** select
                    Dim columnsResultList = From columnRow In aTable.AsEnumerable _
                                           Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                           Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                           DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                                           [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                                           Description = columnRow.Field(Of String)("DESCRIPTION"), _
                                           CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                                           IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                                           Where [ColumnName] = fielddesc.ColumnName


                    ' check if containskey -> write
                    If addToSchemaDir Then
                        ' set it here -> bootstrapping will fail otherwise
                        aSchemaDir = New ObjectDefinition
                        Call aSchemaDir.Create(atableid)
                        Call aSchemaDir.Persist()
                    End If

                    If columnsResultList.Count > 0 Then
                        Return columnsResultList
                    Else
                        Call CoreMessageHandler(message:="Add Column failed", subname:="clsOLEDBDriver", _
                                                    tablename:=atableid, entryname:=fielddesc.ColumnName, _
                                                    messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If

                ElseIf Not createOnMissing Then
                    Return Nothing
                Else
                    Return columnsList
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=atableid, entryname:=fielddesc.ColumnName, _
                                     subname:="clsOLEDBDriver.getColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=atableid, entryname:=fielddesc.ColumnName, _
                                      subname:="clsOLEDBDriver.getColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
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
        Public Overrides Function SetDBParameter(parametername As String, _
                                                value As Object, _
                                                Optional ByRef nativeConnection As Object = Nothing, _
                                                Optional updateOnly As Boolean = False, _
                                                Optional silent As Boolean = False) As Boolean

            Dim otdbcn As OleDbConnection
            Dim dataRows() As DataRow
            Dim insertFlag As Boolean = False

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = DirectCast(_primaryConnection, clsOLEDBConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsOLEDBDriver.setDBParameter", _
                                              message:="Native Internal Connection not available")
                        Return False
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.setDBParameter", _
                                          message:="Connection not available")
                    Return False
                End If

            End If

            '*** try to cast
            Try
                otdbcn = DirectCast(nativeConnection, OleDbConnection)
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOLEDBDriver.setDBParameter", _
                                      exception:=ex, message:="object is not castable to OLEDBConnection")
                Return False
            End Try

            '** init driver
            If Not Me.IsInitialized Then
                Me.Initialize()
            End If
            Try
                dataRows = _ParametersTable.Select("[ID]='" & parametername & "'")

                ' not found
                If dataRows.GetLength(0) = 0 Then
                    If updateOnly And silent Then
                        SetDBParameter = False
                        Exit Function
                    ElseIf updateOnly And Not silent Then
                        Call CoreMessageHandler(showmsgbox:=True, _
                                              message:="The Parameter '" & parametername & "' was not found in the OTDB Table tblParametersGlobal", subname:="clsOLEDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
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
                dataRows(0)("Value") = CStr(value)
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

            Catch ex As Exception
                ' Handle the error

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsOLEDBDriver.setDBParameter", _
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
        Public Overrides Function GetDBParameter(parameterename As String, _
                                                Optional ByRef nativeConnection As Object = Nothing, _
                                                Optional silent As Boolean = False) As Object

            Dim otdbcn As OleDbConnection
            Dim dataRows() As DataRow

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = _primaryConnection.NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsOLEDBDriver.getDBParameter", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsOLEDBDriver.getDBParameter", message:="Connection not available")
                    Return Nothing
                End If
            End If
            '** cast to OLEDB Connection
            Try
                otdbcn = DirectCast(nativeConnection, OleDbConnection)
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOLEDBDriver.getDBParameter", exception:=ex, message:="object is not castable to OLEDBConnection")
                Return Nothing
            End Try

            Try
                '** init driver
                If Not Me.IsInitialized Then
                    Me.Initialize()
                End If

                '** select row
                dataRows = _ParametersTable.Select("[ID]='" & parameterename & "'")

                ' not found
                If dataRows.GetLength(0) = 0 Then
                    If silent Then
                        Return Nothing
                    ElseIf Not silent Then
                        Call CoreMessageHandler(showmsgbox:=True, _
                                              message:="The Parameter '" & parameterename & "' was not found in the OTDB Table tblParametersGlobal", subname:="clsOLEDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                        Return Nothing

                    End If
                End If

                ' value
                Return dataRows(0)("Value")

                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsOLEDBDriver.getDBParameter", tablename:="tblParametersGlobal", _
                                      exception:=ex, entryname:=parameterename)
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
        Public Overrides Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                  Optional silent As Boolean = True, Optional nativeConnection As Object = Nothing) As Boolean _
        Implements iormDBDriver.RunSqlStatement
            Dim anativeConnection As OleDbConnection
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
                Dim aSQLCommand As New OleDbCommand
                aSQLCommand.Connection = anativeConnection
                aSQLCommand.CommandText = sqlcmdstr

                If aSQLCommand.ExecuteNonQuery() > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOLEDBDriver.runSQLCommand", exception:=ex)
                Return False
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
    '***** CLASS clsOLEDBConnection describes the Connection description to OnTrack
    '*****        based on ADO.NET OLEDB Driver
    '*****

    Public Class clsOLEDBConnection
        Inherits clsADONETConnection
        Implements iormConnection

        'Protected Friend Shadows _nativeConnection As OleDbConnection
        'Protected Friend Shadows _nativeinternalConnection As OleDbConnection

        Public Shadows Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Shadows Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

        Public Sub New(ByVal id As String, ByRef databaseDriver As iormDBDriver, ByRef session As Session, sequence As ot.ConfigSequence)
            MyBase.New(id, databaseDriver, session, sequence)
        End Sub

        Public Shadows Function RaiseOnConnected() Handles MyBase.OnConnection
            RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
        End Function
        Public Shadows Function RaiseOnDisConnected() Handles MyBase.OnDisconnection
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
        End Function

        ''' <summary>
        ''' Gets or sets the connection.
        ''' </summary>
        ''' <value>The connection.</value>
        Public Property OLEDBConnection() As OleDbConnection
            Get
                If _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    Return Nothing
                Else
                    Return DirectCast(Me.NativeConnection, OleDbConnection)
                End If

            End Get
            Protected Friend Set(value As OleDbConnection)
                Me._nativeConnection = value
            End Set
        End Property


        ''' <summary>
        ''' create a new SQLConnection
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNewNativeConnection() As IDbConnection
            Return New OleDbConnection()
        End Function

    End Class


    '************************************************************************************
    '***** CLASS clsOLEDBTableSchema  CLASS describes the schema per table of the database itself
    '*****        based on ADO.NET OLEDB Driver
    '*****

    ''' <summary>
    ''' CLASS describes the schema per table of the database itself
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clsOLEDBTableSchema
        Inherits clsADONETTableSchema
        Implements iotDataSchema


        '***** internal variables
        '*****
        'Protected Friend Shadows _Connection As clsOLEDBConnection

        Public Sub New(ByRef connection As clsOLEDBConnection, ByVal tableID As String)
            MyBase.New(connection, tableID)

        End Sub


        Protected Friend Overrides Function createNativeDBParameter() As IDbDataParameter
            Return New OleDbParameter()
        End Function
        Protected Friend Overrides Function createNativeDBCommand() As IDbCommand
            Return New OleDbCommand()
        End Function
        Protected Friend Overrides Function isNativeDBTypeOfVar(type As Object) As Boolean
            Dim datatype As OleDbType = type

            If datatype = OleDbType.LongVarChar Or datatype = OleDbType.LongVarWChar _
             Or datatype = OleDbType.VarChar Or datatype = OleDbType.VarWChar _
             Or datatype = OleDbType.WChar Or datatype = OleDbType.BSTR _
             Or datatype = OleDbType.Binary Or datatype = OleDbType.Variant _
             Or datatype = OleDbType.LongVarBinary Or datatype = OleDbType.VarBinary Then
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
                                                           Optional parametername As String = "") As IDbDataParameter Implements iotDataSchema.AssignNativeDBParameter
            Dim aDBColumnDescription As ColumnDescription = GetColumnDescription(Me.GetFieldordinal(fieldname))
            Dim aParameter As OleDbParameter

            If Not aDBColumnDescription Is Nothing Then

                aParameter = createNativeDBParameter()
                If parametername = "" Then
                    aParameter.ParameterName = "@" & fieldname
                Else
                    If parametername.First = "@" Then
                        aParameter.ParameterName = parametername
                    Else
                        aParameter.ParameterName = "@" & parametername
                    End If

                End If

                aParameter.OleDbType = aDBColumnDescription.DataType
                aParameter.SourceColumn = fieldname

                '** set the length
                If isNativeDBTypeOfVar(aDBColumnDescription.DataType) Then
                    If aDBColumnDescription.CharacterMaxLength = 0 Then
                        aParameter.Size = Const_MaxMemoSize
                    Else
                        aParameter.Size = aDBColumnDescription.CharacterMaxLength
                    End If

                Else
                    If aDBColumnDescription.CharacterMaxLength <> 0 Then
                        aParameter.Size = aDBColumnDescription.CharacterMaxLength
                    End If
                    aParameter.Size = 0
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
            Dim aColumnName As String = ""
            Dim aCon As OleDbConnection = DirectCast(DirectCast(_Connection, clsOLEDBConnection).NativeInternalConnection, OleDbConnection)


            ' return if no TableID
            If _TableID = "" Then
                Call CoreMessageHandler(subname:="clsOLEDBTableSchema.refresh", _
                                      message:="Nothing table name to set to", _
                                      tablename:=TableID)
                _IsInitialized = False
                Return False
            End If
            '


            Refresh = True

            Try

                ' set the SchemaTable
                Dim restrictionsTable() As String = {Nothing, Nothing, _TableID}
                _ColumnsTable = aCon.GetSchema("COLUMNS", restrictionsTable)
                Dim columnsList = From columnRow In _ColumnsTable.AsEnumerable _
                            Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                            Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                            DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                            [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                            Description = columnRow.Field(Of String)("DESCRIPTION"), _
                            CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                            IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                            Where [ColumnName] <> "" Order By TableName, Columnordinal

                no = columnsList.Count()

                Dim columnsList1 = From columnRow In _ColumnsTable.AsEnumerable _
                                    Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                    [ColumnName] = columnRow.Field(Of String)("COLUMN_NAME"), _
                                    Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                    DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                                    IsNullable = columnRow.Field(Of Boolean)("IS_NULLABLE"), _
                                    HasDefault = columnRow.Field(Of Boolean)("COLUMN_HASDEFAULT"), _
                                    [Default] = columnRow.Field(Of String)("COLUMN_DEFAULT"), _
                                    CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                                    CharacterOctetLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_OCTET_LENGTH"), _
                                    Description = columnRow.Field(Of String)("DESCRIPTION"), _
                                    NumericPrecision = columnRow.Field(Of Nullable(Of Int64))("NUMERIC_PRECISION"), _
                                    NumericScale = columnRow.Field(Of Nullable(Of Int64))("NUMERIC_SCALE"), _
                                    DateTimePrecision = columnRow.Field(Of Nullable(Of Int64))("DATETIME_PRECISION"), _
                                    Catalog = columnRow.Field(Of String)("TABLE_CATALOG") _
                                    Where [ColumnName] <> "" And TableName <> "" And Columnordinal > 0 _
                                    Order By TableName, Columnordinal


                '** read indixes
                Dim restrictionsIndex() As String = {Nothing, Nothing, Nothing, Nothing, _TableID}
                ' get the Index Table
                _IndexTable = aCon.GetSchema("INDEXES", restrictionsIndex)

                Dim columnsIndexList = From indexRow In _IndexTable.AsEnumerable _
                                            Select TableName = indexRow.Field(Of String)("TABLE_NAME"), _
                                            IndexName = indexRow.Field(Of String)("INDEX_NAME"), _
                                            Columnordinal = indexRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                            ColumnName = indexRow.Field(Of String)("COLUMN_NAME"), _
                                            isPrimaryKey = indexRow.Field(Of Boolean)("PRIMARY_KEY") _
                                            Where [ColumnName] <> "" And TableName <> "" And Columnordinal > 0 _
                                            Order By TableName, IndexName, Columnordinal, ColumnName


                no = columnsList.Count

                If no = 0 Then
                    Call CoreMessageHandler(subname:="clsOLEDBTableSchema.Refresh", tablename:=Me.TableID, _
                                          messagetype:=otCoreMessageType.InternalError, message:="table has no fields - does it exist ?")
                    _IsInitialized = False
                    Return False
                End If

                ReDim _fieldnames(no - 1)
                ReDim _Columns(no - 1)

                ' set the Dictionaries if reload
                _fieldsDictionary = New Dictionary(Of String, Long)
                _indexDictionary = New Dictionary(Of String, ArrayList)
                aColumnCollection = New ArrayList
                _NoPrimaryKeys = 0

                '**** read all the column / fieldnames
                '****
                Dim i As UShort = 0
                For Each row In columnsList

                    '*
                    If row.ColumnName.Contains(".") Then
                        aColumnName = LCase(row.ColumnName.Substring(row.ColumnName.IndexOf(".") + 1, row.ColumnName.Length - row.ColumnName.IndexOf(".") + 1))
                    Else
                        aColumnName = LCase(row.ColumnName)
                    End If
                    '*
                    _Fieldnames(i) = aColumnName
                    '* set the description
                    _Columns(i) = New ColumnDescription
                    With _Columns(i)
                        .ColumnName = aColumnName
                        .Description = row.Description
                        '.HasDefault = row.HasDefault
                        .CharacterMaxLength = row.CharacterMaxLength
                        If Not row.CharacterMaxLength Is Nothing Then
                            .CharacterMaxLength = CLng(row.CharacterMaxLength)
                        Else
                            .CharacterMaxLength = 0
                        End If
                        .IsNullable = row.IsNullable
                        .DataType = row.DataType
                        .Ordinal = row.Columnordinal
                        .Default = Nothing
                        .HasDefault = False
                        '.Catalog = row.Catalog
                        '.DateTimePrecision = row.DateTimePrecision
                        '.NumericPrecision = row.NumericPrecision
                        '.NumericScale = row.NumericScale
                        '.CharachterOctetLength = row.CharacterOctetLength
                    End With

                    ' remove if existing
                    If _fieldsDictionary.ContainsKey(aColumnName) Then
                        _fieldsDictionary.Remove(aColumnName)
                    End If
                    ' add
                    _fieldsDictionary.Add(key:=aColumnName, value:=i + 1) 'store no field 1... not the array index

                    '* 
                    i = i + 1
                Next



                '**** read each Index
                '****
                Dim anIndexName As String = ""
                For Each row In columnsIndexList

                    If row.ColumnName.Contains(".") Then
                        aColumnName = LCase(row.ColumnName.Substring(row.ColumnName.IndexOf(".") + 1, row.ColumnName.Length))
                    Else
                        aColumnName = LCase(row.ColumnName)
                    End If

                    If row.IndexName <> anIndexName Then
                        '** store
                        If anIndexName <> "" Then
                            If _indexDictionary.ContainsKey(anIndexName) Then
                                _indexDictionary.Remove(key:=anIndexName)
                            End If
                            _indexDictionary.Add(key:=anIndexName, value:=aColumnCollection)
                        End If
                        ' new
                        anIndexName = row.IndexName.Clone
                        aColumnCollection = New ArrayList
                    End If
                    '** Add To List
                    aColumnCollection.Add(aColumnName)

                    ' indx no
                    index = _fieldsDictionary.Item(aColumnName)
                    '
                    '** check if primaryKey
                    'fill old primary Key structure
                    If row.isPrimaryKey Then
                        _PrimaryKeyIndexName = row.IndexName.Clone
                        _NoPrimaryKeys = _NoPrimaryKeys + 1
                        ReDim Preserve _Primarykeys(0 To _NoPrimaryKeys - 1)
                        _Primarykeys(_NoPrimaryKeys - 1) = index - 1 ' set to the array 0...ubound
                    End If

                    If Not _fieldsDictionary.ContainsKey(aColumnName) Then
                        Call CoreMessageHandler(subname:="clsOLEDBTableSchema.refresh", _
                                              message:="clsOLEDBTableSchema : column " & row.ColumnName & " not in dictionary ?!", _
                                              tablename:=TableID, entryname:=row.ColumnName)

                        Return False
                    End If

                Next
                '** store final
                If anIndexName <> "" Then
                    If _indexDictionary.ContainsKey(anIndexName) Then
                        _indexDictionary.Remove(key:=anIndexName)
                    End If
                    _indexDictionary.Add(key:=anIndexName, value:=aColumnCollection)
                End If

                '**** build the commands
                '****
                Dim enumValues As Array = System.[Enum].GetValues(GetType(CommandType))
                For Each anIndexName In _indexDictionary.Keys
                    Dim aNewCommand As OleDbCommand
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
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsOLEDBTableSchema.refresh", tablename:=_TableID, _
                                      arg1:=reloadForce, exception:=ex)
                _IsInitialized = False
                Return False
            End Try

        End Function

    End Class


    '************************************************************************************
    '***** CLASS clsOLEDBTableStore describes the per Table reference and Helper Class
    '*****                    ORM Mapping Class and Table Access Workhorse
    '*****

    Public Class clsOLEDBTableStore
        Inherits clsADONETTableStore
        Implements iormDataStore

        'Protected Friend Shadows _cacheAdapter As OleDbDataAdapter

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
        Protected Friend Overrides Function createNativeDBCommand(commandstr As String, ByRef nativeConnection As IDbConnection) As IDbCommand
            Return New OleDbCommand(cmdText:=commandstr, connection:=nativeConnection)
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
            Return Connection.DatabaseDriver.Convert2DBData(value:=value, targetType:=targetType, maxsize:=maxsize, abostrophNecessary:=abostrophNecessary, _
                                       fieldname:=fieldname)
        End Function


        '*********
        '********* cvt2ObjData returns a object from the Datatype of the column to XLS nterpretation
        '*********
        ''' <summary>
        ''' returns a object from the Data type of the column to Host interpretation
        ''' </summary>
        ''' <param name="index">index as object (name or index 1..n)</param>
        ''' <param name="value">value to convert</param>
        ''' <param name="abostrophNecessary">True if necessary</param>
        ''' <returns>converted value </returns>
        ''' <remarks></remarks>
        Public Overrides Function Convert2ObjectData(ByVal index As Object, ByVal value As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object _
        Implements iormDataStore.Convert2ObjectData
            Dim aSchema As clsOLEDBTableSchema = Me.TableSchema
            Dim aDBColumn As clsOLEDBTableSchema.ColumnDescription
            Dim result As Object
            Dim fieldno As Integer

            result = Nothing

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

                If aDBColumn.DataType = OleDbType.BigInt Or aDBColumn.DataType = OleDbType.Integer _
                Or aDBColumn.DataType = OleDbType.SmallInt Or aDBColumn.DataType = OleDbType.TinyInt _
                Or aDBColumn.DataType = OleDbType.UnsignedBigInt Or aDBColumn.DataType = OleDbType.UnsignedInt _
                Or aDBColumn.DataType = OleDbType.UnsignedSmallInt Or aDBColumn.DataType = OleDbType.UnsignedTinyInt _
                Or aDBColumn.DataType = OleDbType.SmallInt Or aDBColumn.DataType = OleDbType.TinyInt Then
                    If (Not IsNumeric(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CLng(value)
                    Else
                        Call CoreMessageHandler(subname:="clsOLEDBTablestore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Integer", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = DBNull.Value
                    End If

                ElseIf aDBColumn.DataType = OleDbType.Char Or aDBColumn.DataType = OleDbType.BSTR Or aDBColumn.DataType = OleDbType.LongVarChar _
                Or aDBColumn.DataType = OleDbType.LongVarWChar Or aDBColumn.DataType = OleDbType.VarChar Or aDBColumn.DataType = OleDbType.VarWChar _
                Or aDBColumn.DataType = OleDbType.WChar Then
                    abostrophNecessary = True
                    If (value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = ""
                    Else
                        result = CStr(value)
                    End If

                ElseIf aDBColumn.DataType = OleDbType.Date Or aDBColumn.DataType = OleDbType.DBDate Or aDBColumn.DataType = OleDbType.DBTime _
                Or aDBColumn.DataType = OleDbType.DBTimeStamp Then

                    If (Not IsDate(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = ConstNullDate
                    ElseIf IsDate(value) Then
                        result = CDate(value)
                    Else
                        Call CoreMessageHandler(subname:="clsOLEDBTablestore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Date", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = ConstNullDate
                    End If
                ElseIf aDBColumn.DataType = OleDbType.Double Or aDBColumn.DataType = OleDbType.Decimal _
                Or aDBColumn.DataType = OleDbType.Single Or aDBColumn.DataType = OleDbType.Numeric Then
                    If (Not IsNumeric(value) Or value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value)) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = 0
                    ElseIf IsNumeric(value) Then
                        result = CDbl(value)
                    Else
                        Call CoreMessageHandler(subname:="clsOLEDBTablestore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & value & "' is not convertible to Double", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        result = DBNull.Value
                    End If
                ElseIf aDBColumn.DataType = OleDbType.Boolean Then
                    If (value Is Nothing Or DBNull.Value.Equals(value) Or IsError(value) Or value = False) OrElse String.IsNullOrWhiteSpace(value) Then
                        result = False
                    Else
                        result = True
                    End If

                End If

                ' return
                Return result
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsOLEDBTableStore.cvt2ObjData", _
                                      arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName, exception:=ex, _
                                      messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Initialize Cache 
        ''' </summary>
        ''' <returns>true if successful </returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function InitializeCache(Optional ByVal force As Boolean = False) As Boolean

            Dim aCommand As OleDbCommand
            Dim aDataSet As DataSet

            Try
                '** initialize
                If Not Me.IsCacheInitialized Or force Then
                    ' set theAdapter
                    _cacheAdapter = New OleDbDataAdapter
                    _cacheAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                    aDataSet = DirectCast(Me.Connection.DatabaseDriver, clsOLEDBDriver).OnTrackDataSet
                    ' Select Command
                    aCommand = DirectCast(Me.TableSchema, clsOLEDBTableSchema).getCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsOLEDBTableSchema.CommandType.SelectType)
                    If Not aCommand Is Nothing Then
                        Dim selectstr As String = "SELECT "
                        For i = 1 To Me.TableSchema.NoFields
                            selectstr &= "[" & Me.TableSchema.Getfieldname(i) & "]"
                            If i < Me.TableSchema.NoFields Then
                                selectstr &= ","
                            End If
                        Next
                        selectstr &= " FROM " & Me.TableID
                        _cacheAdapter.SelectCommand = New OleDbCommand(selectstr)
                        _cacheAdapter.SelectCommand.CommandType = CommandType.Text
                        _cacheAdapter.SelectCommand.Connection = DirectCast(Me.Connection.NativeConnection, OleDbConnection)
                        _cacheAdapter.FillSchema(aDataSet, SchemaType.Source)
                        DirectCast(_cacheAdapter, OleDbDataAdapter).Fill(aDataSet, Me.TableID)
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
                            For Each fieldname In Me.TableSchema.getIndex(indexName)
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
                    aCommand = DirectCast(Me.TableSchema, clsOLEDBTableSchema).getCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsOLEDBTableSchema.CommandType.DeleteType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.DeleteCommand = aCommand
                    End If

                    ' Insert Command
                    aCommand = DirectCast(Me.TableSchema, clsOLEDBTableSchema).getCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsOLEDBTableSchema.CommandType.InsertType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.InsertCommand = aCommand
                    End If
                    ' Update Command
                    aCommand = DirectCast(Me.TableSchema, clsOLEDBTableSchema).getCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          clsOLEDBTableSchema.CommandType.UpdateType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.UpdateCommand = aCommand
                    End If

                    '** return true
                    Return True
                Else
                    Return False
                End If



            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsOLEDBTablestore.initializeCache", exception:=ex, message:="Exception", _
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
                Return DirectCast(dataadapter, OleDbDataAdapter).Update(datatable)

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception occured", subname:="clsOLEDBTableStore.UpdateDBDataTable", exception:=ex, _
                                    messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception occured", subname:="clsOLEDBTableStore.UpdateDBDataTable", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return 0
            End Try

        End Function
    End Class
End Namespace