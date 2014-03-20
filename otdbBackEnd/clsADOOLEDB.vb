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
Option Explicit On

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


    ''' <summary>
    ''' oleDBDriver is the database driver for ADO.NET OLEDB drivers
    ''' </summary>
    ''' <remarks></remarks>
    Public Class oleDBDriver
        Inherits adonetDBDriver
        Implements iormDatabaseDriver

        Protected Friend Shadows WithEvents _primaryConnection As oledbConnection '-> in clsOTDBDriver
        Private Shadows _ParametersTableAdapter As New OleDbDataAdapter
        Shadows Event RequestBootstrapInstall(sender As Object, e As SessionBootstrapEventArgs) Implements iormDatabaseDriver.RequestBootstrapInstall

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="ID">an ID for this driver</param>
        ''' <remarks></remarks>
        Public Sub New(id As String, ByRef session As Session)
            Call MyBase.New(id, session)
            If Me._primaryConnection Is Nothing Then
                _primaryConnection = New oledbConnection(id:="primary", DatabaseDriver:=Me, session:=session, sequence:=ComplexPropertyStore.Sequence.primary)
            End If
        End Sub


        ''' <summary>
        ''' NativeConnection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads ReadOnly Property NativeConnection() As OleDb.OleDbConnection
            Get
                Return DirectCast(_primaryConnection.NativeConnection, System.Data.OleDb.OleDbConnection)
            End Get

        End Property

        ''' <summary>
        ''' builds the adapter for the parameters table
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
                .InsertCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection)
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
                .UpdateCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection)
                .UpdateCommand.Prepare()


                '***** DELETE
                .DeleteCommand = New OleDbCommand( _
                "DELETE FROM " & _parametersTableName & " WHERE ID = ?")
                .DeleteCommand.Parameters.Add( _
                "@ID", OleDbType.Char, 50, "ID").SourceVersion = _
                    DataRowVersion.Original
                .DeleteCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection)
                .DeleteCommand.Prepare()

            End With

        End Function
        '***
        '*** Initialize Driver
        ''' <summary>
        ''' Initialize the driver
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
                    _primaryConnection = New oledbConnection("primary", Me, _session, ComplexPropertyStore.Sequence.primary)
                End If

                '*** do we have the Table ?! - donot do this in bootstrapping since we are running in recursion then
                If Not Me.HasTable(_parametersTableName) And Not _session.IsBootstrappingInstallationRequested Then
                    If Not VerifyOnTrackDatabase(install:=False) Then
                        '* now in bootstrap ?!
                        If _session.IsBootstrappingInstallationRequested Then
                            CoreMessageHandler(message:="verifying the database failed moved to bootstrapping - caching parameters meanwhile", _
                                               subname:="oleDBDriver.Initialize", _
                                          messagetype:=otCoreMessageType.InternalWarning, arg1:=Me.ID)
                            Me.IsInitialized = True
                            Return True
                        Else
                            CoreMessageHandler(message:="verifying the database failed - failed to initialize driver", subname:="oleDBDriver.Initialize", _
                                              messagetype:=otCoreMessageType.InternalError, arg1:=Me.ID)
                            Me.IsInitialized = False
                            Return False
                        End If
                    End If
                End If

                '*** end of bootstrapping conditions reinitialize automatically
                '*** might be that we are now in bootstrapping
                If Not _session.IsBootstrappingInstallationRequested OrElse force Then
                    '*** set the DataTable
                    If _OnTrackDataSet Is Nothing Then _OnTrackDataSet = New DataSet(Me.ID & Date.Now.ToString)

                    '** create adapaters
                    If Me.HasTable(_parametersTableName) Then
                        ' the command
                        Dim aDBCommand = New OleDbCommand()
                        aDBCommand.CommandText = "select ID, [Value], changedOn, description from " & _parametersTableName
                        aDBCommand.Connection = DirectCast(_primaryConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection)
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

                        '** save the cache
                        If _BootStrapParameterCache.Count > 0 Then
                            For Each kvp As KeyValuePair(Of String, Object) In _BootStrapParameterCache
                                SetDBParameter(parametername:=kvp.Key, value:=kvp.Value, silent:=True)
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
                Call CoreMessageHandler(subname:="oleDBDriver.OnConnection", message:="couldnot Initialize Driver", _
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
        Protected Friend Overrides Function CreateNativeTableStore(ByVal TableID As String, ByVal forceSchemaReload As Boolean) As iormDataStore
            Return New oledbTableStore(Me.CurrentConnection, TableID, forceSchemaReload)
        End Function
        ''' <summary>
        ''' create a new TableSchema for this Driver
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNativeTableSchema(ByVal TableID As String) As iotDataSchema
            Return New oledbTableSchema(Me.CurrentConnection, TableID)
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CreateNativeDBCommand(commandstr As String, nativeConnection As IDbConnection) As IDbCommand Implements iormDatabaseDriver.CreateNativeDBCommand
            Return New OleDbCommand(commandstr, nativeConnection)
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
        ''' returns the target type for a OTDB FieldType - MAPPING
        ''' </summary>
        ''' <param name="type"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDatabaseDriver.GetTargetTypeFor

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
                        Return OleDbType.WChar
                    Case otFieldDataType.[Long]
                        Return OleDbType.Integer
                    Case otFieldDataType.Memo
                        Return OleDbType.WChar
                    Case otFieldDataType.Numeric
                        Return OleDbType.Double
                    Case otFieldDataType.Timestamp
                        Return OleDbType.Date
                    Case otFieldDataType.Text
                        Return OleDbType.WChar
                    Case Else

                        Call CoreMessageHandler(subname:="oleDBDriver.GetTargetTypefor", message:="Type not defined",
                                       messagetype:=otCoreMessageType.InternalException)
                End Select

            Catch ex As Exception
                Call CoreMessageHandler(subname:="oleDBDriver.GetTargetTypefor", message:="Exception", exception:=ex, _
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
        Public Overrides Function Convert2DBData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "", _
                                                    Optional isnullable As Boolean = False, _
                                                     Optional defaultvalue As Object = Nothing) As Boolean Implements iormDatabaseDriver.Convert2DBData

            Dim result As Object = Nothing
            Try
                
                ''' convert an array object to a string
                If IsArray(invalue) Then
                    invalue = Converter.Array2String(invalue)
                End If


                If targetType = OleDbType.BigInt OrElse targetType = OleDbType.Integer _
                      OrElse targetType = OleDbType.SmallInt OrElse targetType = OleDbType.TinyInt _
                      OrElse targetType = OleDbType.UnsignedBigInt OrElse targetType = OleDbType.UnsignedInt _
                      OrElse targetType = OleDbType.UnsignedSmallInt OrElse targetType = OleDbType.UnsignedTinyInt _
                      OrElse targetType = OleDbType.SmallInt OrElse targetType = OleDbType.TinyInt Then

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
                        Call CoreMessageHandler(subname:="oledbTableStore.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & invalue & " is not convertible to Integer", _
                                              arg1:=invalue, messagetype:=otCoreMessageType.InternalError)
                        System.Diagnostics.Debug.WriteLine("OTDB data " & invalue & " is not convertible to Integer")
                        outvalue = Nothing
                        Return False

                    End If

                ElseIf targetType = OleDbType.Char OrElse targetType = OleDbType.BSTR OrElse targetType = OleDbType.LongVarChar _
                OrElse targetType = OleDbType.LongVarWChar OrElse targetType = OleDbType.VarChar OrElse targetType = OleDbType.VarWChar _
                OrElse targetType = OleDbType.WChar Then

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

                ElseIf targetType = OleDbType.Date OrElse targetType = OleDbType.DBDate OrElse targetType = OleDbType.DBTime _
                OrElse targetType = OleDbType.DBTimeStamp Then
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
                        Call CoreMessageHandler(subname:="oledbTableStore.cvt2ColumnData", entryname:=fieldname, _
                                             message:="OTDB data " & invalue & " is not convertible to Date", _
                                             arg1:=invalue, messagetype:=otCoreMessageType.InternalError)
                        outvalue = Nothing
                        Return False
                    End If

                ElseIf targetType = OleDbType.Double OrElse targetType = OleDbType.Decimal _
                OrElse targetType = OleDbType.Single OrElse targetType = OleDbType.Numeric Then

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
                        Call CoreMessageHandler(subname:="oledbTableStore.cvt2ColumnData", entryname:=fieldname, _
                                              message:="OTDB data " & invalue & " is not convertible to Double", _
                                              arg1:=targetType, messagetype:=otCoreMessageType.InternalError)

                        outvalue = Nothing
                        Return False
                    End If

                ElseIf targetType = OleDbType.Boolean Then

                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToBoolean(False)

                    If isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
                         OrElse (IsNumeric(invalue) AndAlso invalue = 0)) Then
                        result = DBNull.Value
                    ElseIf isnullable AndAlso (invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse String.IsNullOrWhiteSpace(invalue.ToString) _
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
                Call CoreMessageHandler(message:="Exception", subname:="oledbTableStore.convert2ColumnData(Object, long ..", _
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
                                                      Implements iormDatabaseDriver.AssignNativeDBParameter


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
                Call CoreMessageHandler(subname:="oleDBDriver.assignDBParameter", message:="Exception", exception:=ex, _
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
        ''' returns True if the tablename exists in the datastore
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasTable(tableid As String, _
                                           Optional ByRef connection As iormConnection = Nothing, _
                                           Optional nativeConnection As Object = Nothing) As Boolean
            Dim myConnection As OnTrack.Database.oledbConnection
            Dim aTable As DataTable
            Dim myNativeConnection As OleDb.OleDbConnection

            '* if already loaded
            If _TableDirectory.ContainsKey(key:=tableid) Then Return True

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            If nativeConnection Is Nothing Then
                myNativeConnection = TryCast(myConnection.NativeInternalConnection, OleDb.OleDbConnection)
            Else
                myNativeConnection = TryCast(nativeConnection, OleDb.OleDbConnection)
            End If

            If myConnection Is Nothing OrElse myConnection.NativeInternalConnection Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.HasTable", message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            If myNativeConnection Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.HasTable", message:="No current internal Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights we cannot check on the User Table -> recursion
            '* do not check -> makes no sense since we are checking the database status before we are installing
            'If Not CurrentSession.IsBootstrappingInstallation AndAlso tableid <> User.ConstTableID Then
            '    If Not _currentUserValidation.ValidEntry AndAlso Not _currentUserValidation.HasReadRights Then
            '        If Not myConnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], loginOnFailed:=True) Then
            '            Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.HasTable", _
            '                                  message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
            '            Return Nothing
            '        End If
            '    End If
            'End If


            Try
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                aTable = myNativeConnection.GetSchema("COLUMNS", restrictionsTable)

                If aTable.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.hasTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Gets the table.
        ''' </summary>
        ''' <param name="tablename">The tablename.</param>
        ''' <param name="createOnMissing">The create on missing.</param>
        ''' <param name="NativeConnection">The native connection.</param>
        ''' <returns></returns>
        Public Overrides Function GetTable(tableid As String, _
                                           Optional createOrAlter As Boolean = False, _
                                           Optional ByRef connection As iormConnection = Nothing, _
                                            Optional ByRef nativeTableObject As Object = Nothing) As Object

            Dim myConnection As oledbConnection
            Dim aTable As DataTable
            Dim aStatement As String = ""

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            If myConnection Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.GetTable", tablename:=tableid, message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myConnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.GetTable", tablename:=tableid, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If


            Try
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)


                '** create the table
                '**
                If aTable.Rows.Count = 0 And createOrAlter Then

                    aStatement = "CREATE TABLE " & tableid & " ( tttemp  bit )"
                    Me.RunSqlStatement(aStatement, _
                                       nativeConnection:=DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection))

                    aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)

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
                    Call CoreMessageHandler(subname:="oleDBDriver.getTable", tablename:=tableid, _
                                          message:="Table was not found in database", messagetype:=otCoreMessageType.ApplicationWarning)
                    Return Nothing
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.getTable", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End Try

        End Function

        Public Overrides Function GetIndex(ByRef nativeTable As Object, ByRef indexdefinition As IndexDefinition, _
                                         Optional ByVal forceCreation As Boolean = False, _
                                         Optional ByVal createOrAlter As Boolean = False, _
                                          Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetIndex
            Dim aTable As DataTable = TryCast(nativeTable, DataTable)
            Dim myconnection As oledbConnection
            Dim atableid As String = ""

            '** no object ?!
            If aTable Is Nothing Then
                Return Nothing
            End If
            If connection Is Nothing Then
                myconnection = _primaryConnection
            Else
                myconnection = connection
            End If

            If myconnection Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.GetIndex", arg1:=indexdefinition.Name, _
                                      message:="No current Connection to the Database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
                '** Schema and User Creation are for free !
            End If
            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myconnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.GetIndex", arg1:=indexdefinition.Name, _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If



            Dim newindexname As String = indexdefinition.Name.Clone
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
                    Call CoreMessageHandler(message:="atableid couldn't be retrieved from nativetable object", subname:="oleDBDriver.getIndex", _
                                                 tablename:=atableid, arg1:=indexdefinition.Name, messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                Else
                    atableid = tableidList(0).TableName
                End If
                '** read indixes
                Dim restrictionsIndex() As String = {Nothing, Nothing, Nothing, Nothing, atableid}
                anIndexTable = DirectCast(myconnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("INDEXES", restrictionsIndex)

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

                If columnsIndexList.Count = 0 And Not createOrAlter Then
                    Return Nothing
                ElseIf columnsIndexList.Count = 0 And createOrAlter Then
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
                    If anIndexColumnsList.Count = indexdefinition.Columnnames.Count Then
                        For Each columnName As String In indexdefinition.Columnnames
                            If LCase(anIndexColumnsList.Item(i)) <> LCase(columnName) Then
                                indexnotchanged = False
                                Exit For
                            Else
                                indexnotchanged = True
                            End If

                            ' exit
                            If Not indexnotchanged Then
                                Exit For
                            End If
                            i = i + 1
                        Next columnName
                    Else
                        indexnotchanged = False ' different number of columnnames
                    End If
                    '** check if primary is different
                    If indexdefinition.IsPrimary <> isprimaryKey Or forceCreation Then
                        indexnotchanged = False
                    End If
                    ' return
                    If indexnotchanged Then
                        Return columnsIndexList
                    End If
                    End If


                    '** drop existing

                    If (isprimaryKey Or indexdefinition.IsPrimary) And existingprimaryName <> "" Then
                        aStatement = " ALTER TABLE " & atableid & " DROP CONSTRAINT [" & existingprimaryName & "]"
                        Me.RunSqlStatement(aStatement)
                    ElseIf existingIndex Then
                        aStatement = " DROP INDEX " & existingIndex
                        Me.RunSqlStatement(aStatement)
                    End If

                    '*** build new
                    If indexdefinition.IsPrimary Then
                        aStatement = " ALTER TABLE [" & atableid & "] ADD CONSTRAINT [" & atableid & "_" & indexdefinition.Name & "] PRIMARY KEY ("
                        Dim comma As Boolean = False
                        For Each name As String In indexdefinition.Columnnames
                            If comma Then aStatement &= ","
                            aStatement &= "[" & name & "]"
                            comma = True
                        Next
                        aStatement &= ")"
                        Me.RunSqlStatement(aStatement)
                Else
                    Dim UniqueStr As String = ""
                    If indexdefinition.IsUnique Then UniqueStr = "UNIQUE"
                    aStatement = " CREATE " & UniqueStr & " INDEX [" & atableid & "_" & indexdefinition.Name & "] ON [" & atableid & "] ("
                    Dim comma As Boolean = False
                    For Each name As String In indexdefinition.Columnnames
                        If comma Then aStatement &= ","
                        aStatement &= "[" & name & "]"
                        comma = True
                    Next
                    aStatement &= ")"
                    Me.RunSqlStatement(aStatement)
                    End If

                    '** read indixes

                    anIndexTable = DirectCast(myconnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("INDEXES", restrictionsIndex)

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
                        Call CoreMessageHandler(message:="creation of index failed", arg1:=indexdefinition.Name, _
                                                     subname:="oleDBDriver.getIndex", tablename:=atableid, _
                                                     messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.GetIndex", arg1:=aStatement, tablename:=atableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                myconnection.IsNativeInternalLocked = False
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.GetIndex", arg1:=indexdefinition.Name, tablename:=atableid, _
                                           message:="Exception", exception:=ex, messagetype:=otCoreMessageType.InternalError)
                myconnection.IsNativeInternalLocked = False
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
        Public Overrides Function VerifyColumnSchema(columndefinition As ColumnDefinition, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean
            Dim myConnection As oledbConnection
            Dim aTable As DataTable
            Dim tableid As String = columndefinition.Tablename
            Dim columnname As String = columndefinition.Name

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            '** do not session since we might checking this to get bootstrapping status before session is started
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not myConnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], useLoginWindow:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.verifyColumnSchema", tablename:=tableid, _
                                      message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            Try
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)


                '** select
                Dim columnsResultList = From columnRow In aTable.AsEnumerable _
                               Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                               Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                               DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                               [name] = columnRow.Field(Of String)("COLUMN_NAME"), _
                               Description = columnRow.Field(Of String)("DESCRIPTION"), _
                               CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                               IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                               Where [name] = columndefinition.Name

                If columnsResultList.Count = 0 Then
                    If Not silent Then
                        CoreMessageHandler(message:="verifying table column: column does not exist in database ", _
                                                      tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)

                    End If
                    Return False
                Else
                    '** what to check
                    For Each column In columnsResultList
                        '** check on datatype
                        If column.DataType <> GetTargetTypeFor(columndefinition.Datatype) Then
                            If Not silent Then
                                CoreMessageHandler(message:="verifying table column: column data type in database differs from column definition", arg1:=columndefinition.Datatype, _
                                                        tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                            End If
                            Return False
                        End If
                        '** check on size
                        If column.DataType = OleDbType.VarChar OrElse column.DataType = OleDbType.LongVarChar OrElse _
                            column.DataType = OleDbType.LongVarWChar OrElse column.DataType = OleDbType.VarWChar Then
                            If columndefinition.Size > column.CharacterMaxLength Then
                                If Not silent Then
                                    CoreMessageHandler(message:="verifying table column: column size in database differs from column definition", arg1:=columndefinition.Size, _
                                                        tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                                End If
                                Return False
                            End If
                        End If

                    Next
                    Return True
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
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
        Public Overrides Function VerifyColumnSchema(columnattribute As ormSchemaTableColumnAttribute, Optional ByRef connection As iormConnection = Nothing, Optional silent As Boolean = False) As Boolean
            Dim myConnection As oledbConnection
            Dim aTable As DataTable
            Dim tableid As String = columnattribute.Tablename
            Dim columnname As String = columnattribute.ColumnName

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            '** do not session since we might checking this to get bootstrapping status before session is started
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not myConnection.VerifyUserAccess(accessRequest:=otAccessRight.[ReadOnly], useLoginWindow:=True) Then
                Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.HasTable", tablename:=tableid, _
                                                 message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            Try
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)


                '** select
                Dim columnsResultList = From columnRow In aTable.AsEnumerable _
                               Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                               Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                               DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                               [name] = columnRow.Field(Of String)("COLUMN_NAME"), _
                               Description = columnRow.Field(Of String)("DESCRIPTION"), _
                               CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                               IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                               Where [name] = columnname

                If columnsResultList.Count = 0 Then
                    If Not silent Then
                        CoreMessageHandler(message:="verifying table column: column doesnot exist in database ", _
                                                      tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)
                    End If
                    Return False
                Else
                    '** what to check
                    For Each column In columnsResultList
                        '** check on datatype
                        If columnattribute.HasValueTypeID AndAlso column.DataType <> GetTargetTypeFor(columnattribute.Typeid) Then
                            If Not silent Then
                                CoreMessageHandler(message:="verifying table column: column data type in database differs from column attribute", arg1:=columnattribute.Typeid, _
                                                      tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)

                            End If
                            Return False
                        End If
                        '** check on size
                        If column.DataType = OleDbType.VarChar OrElse column.DataType = OleDbType.LongVarChar OrElse _
                            column.DataType = OleDbType.LongVarWChar OrElse column.DataType = OleDbType.VarWChar Then
                            If columnattribute.HasValueSize AndAlso columnattribute.Size > column.CharacterMaxLength Then
                                If Not silent Then
                                    CoreMessageHandler(message:="verifying table column: column size in database differs from column attribute", arg1:=columnattribute.Size, _
                                                       tablename:=tableid, columnname:=columnname, subname:="oledbdriver.verifyColumnSchema", messagetype:=otCoreMessageType.InternalError)

                                End If

                                Return False
                            End If
                        End If

                    Next
                    Return True
                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
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
        Public Overrides Function HasColumn(tableid As String, columnname As String, Optional ByRef connection As iormConnection = Nothing) As Boolean
            Dim myConnection As oledbConnection
            Dim aTable As DataTable

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            '* doesnot make any sense
            'If Not myConnection.VerifyUserAccess(otAccessRight.[ReadOnly], loginOnFailed:=True) And Not CurrentSession.IsBootstrappingInstallation Then
            '    Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.HasTable", tablename:=tableid, _
            '                          message:="No right to read schema of database", messagetype:=otCoreMessageType.ApplicationError)
            '    Return Nothing
            'End If

            Try
                Dim restrictionsTable() As String = {Nothing, Nothing, tableid}
                aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)

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
                                      subname:="oleDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing

            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=tableid, _
                                      subname:="oleDBDriver.hasColumn", messagetype:=otCoreMessageType.InternalError)
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
        Public Overrides Function GetColumn(nativeTable As Object, columndefinition As ColumnDefinition, _
                                            Optional createOrAlter As Boolean = False, _
                                            Optional ByRef connection As iormConnection = Nothing) As Object Implements iormDatabaseDriver.GetColumn

            Dim aTable As DataTable = TryCast(nativeTable, DataTable)
            Dim atableid As String = ""
            Dim myConnection As oledbConnection
            Dim aStatement As String = ""

            '** no object ?!
            If aTable Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.GetColumn", message:="native table parameter to function is nothing",
                                        messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If

            If connection Is Nothing Then
                myConnection = _primaryConnection
            Else
                myConnection = connection
            End If

            If myConnection Is Nothing Then
                Call CoreMessageHandler(subname:="oleDBDriver.GetColumn", message:="No current Connection to the Database", _
                                      messagetype:=otCoreMessageType.ApplicationError)
                Return Nothing
            End If
            '*** check on rights
            If createOrAlter And Not CurrentSession.IsBootstrappingInstallationRequested Then
                If Not myConnection.VerifyUserAccess(accessRequest:=otAccessRight.AlterSchema, useLoginWindow:=True) Then
                    Call CoreMessageHandler(showmsgbox:=True, subname:="oleDBDriver.GetColumn", _
                                          message:="No right to alter schema of database", messagetype:=otCoreMessageType.ApplicationError)
                    Return Nothing
                End If
            End If


            Try
                '** select
                Dim columnsList = From columnRow In aTable.AsEnumerable _
                               Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                               Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                               DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                               [name] = columnRow.Field(Of String)("COLUMN_NAME"), _
                               Description = columnRow.Field(Of String)("DESCRIPTION"), _
                               CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                               IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                               Where [name] = columndefinition.Name

                If columnsList.Count > 0 And Not createOrAlter Then
                    Return columnsList
                Else

                    '** create the column
                    '**

                    Dim tableidList = From columnRow In aTable.AsEnumerable _
                                          Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                          Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                          DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                                          [name] = columnRow.Field(Of String)("COLUMN_NAME"), _
                                          Description = columnRow.Field(Of String)("DESCRIPTION"), _
                                          CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                                          IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE")

                    If tableidList.Count = 0 Then
                        Call CoreMessageHandler(message:="atableid couldn't be retrieved from nativetable object", subname:="oleDBDriver.getColumn", _
                                                     tablename:=atableid, entryname:=columndefinition.Name, messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    Else
                        atableid = tableidList(0).TableName
                    End If

                    aStatement = "ALTER TABLE " & atableid
                    If columnsList.Count = 0 Then
                        aStatement &= " ADD COLUMN "
                    ElseIf Me.DatabaseType = otDBServerType.Access Then
                        aStatement &= " ALTER COLUMN "
                    Else
                        aStatement &= " MODIFY COLUMN "
                    End If
                    aStatement &= "[" & columndefinition.Name & "] "

                    Select Case columndefinition.Datatype
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
                            If columndefinition.Size = 0 Then
                                aStatement &= Const_MaxTextSize.ToString
                                aStatement &= ")"
                            ElseIf Me.DatabaseType = otDBServerType.Access And columndefinition.Size <= 255 Then
                                aStatement &= columndefinition.Size.ToString
                                aStatement &= ")"
                            ElseIf Me.DatabaseType = otDBServerType.Access And columndefinition.Size > 255 Then
                                aStatement &= " MEMO "
                            End If

                        Case otFieldDataType.Timestamp
                            aStatement &= " TIMESTAMP "
                        Case otFieldDataType.Time
                            aStatement &= " TIME "
                        Case Else
                            Call CoreMessageHandler(message:="Datatype is not implemented", tablename:=atableid, entryname:=columndefinition.Name, _
                                                         subname:="oleDBDriver.getColumn", arg1:=columndefinition.Datatype.ToString, _
                                                         messagetype:=otCoreMessageType.InternalError)
                            Return Nothing
                    End Select

                    If columndefinition.IsNullable Then
                        aStatement &= " NULL "
                    Else
                        aStatement &= " NOT NULL "
                    End If

                    If columndefinition.DefaultValue IsNot Nothing Then
                        '** to be implemented
                        '     aStatement &= " DEFAULT '" & columndefinition.DefaultValueString & "'" not working mus be differentiate to string sql presenttion of data
                    End If
                    '** Run it
                    Me.RunSqlStatement(aStatement, _
                                       nativeConnection:=DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection))

                    '** add uniqueness
                    If columndefinition.IsUnique Then
                        aStatement = "ALTER TABLE " & atableid & " ADD CONSTRAINT " & "C_" & atableid & "_" & columndefinition.Name & " UNIQUE (" & columndefinition.Name & ")"
                        '** Run it
                        Me.RunSqlStatement(aStatement, _
                                           nativeConnection:=DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection))
                    End If

                    '** get the result
                    Dim restrictionsTable() As String = {Nothing, Nothing, atableid}
                    aTable = DirectCast(myConnection.NativeInternalConnection, System.Data.OleDb.OleDbConnection).GetSchema("COLUMNS", restrictionsTable)
                    '** select
                    Dim columnsResultList = From columnRow In aTable.AsEnumerable _
                                           Select TableName = columnRow.Field(Of String)("TABLE_NAME"), _
                                           Columnordinal = columnRow.Field(Of Int64)("ORDINAL_POSITION"), _
                                           DataType = columnRow.Field(Of OleDbType)("DATA_TYPE"), _
                                           [name] = columnRow.Field(Of String)("COLUMN_NAME"), _
                                           Description = columnRow.Field(Of String)("DESCRIPTION"), _
                                           CharacterMaxLength = columnRow.Field(Of Nullable(Of Int64))("CHARACTER_MAXIMUM_LENGTH"), _
                                           IsNullable = columnRow.Field(Of Nullable(Of Boolean))("IS_NULLABLE") _
                                           Where [name] = columndefinition.Name



                    If columnsResultList.Count > 0 Then
                        Return columnsResultList
                    Else
                        Call CoreMessageHandler(message:="Add Column failed", subname:="oleDBDriver", _
                                                    tablename:=atableid, entryname:=columndefinition.Name, _
                                                    messagetype:=otCoreMessageType.InternalError)
                        Return Nothing
                    End If


                End If

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception", exception:=ex, arg1:=aStatement, tablename:=atableid, entryname:=columndefinition.Name, _
                                     subname:="oleDBDriver.getColumn", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception", exception:=ex, tablename:=atableid, entryname:=columndefinition.Name, _
                                      subname:="oleDBDriver.getColumn", messagetype:=otCoreMessageType.InternalError)
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

            Dim dataRows() As DataRow
            Dim insertFlag As Boolean = False

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = DirectCast(_primaryConnection, OnTrack.Database.oledbConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="oleDBDriver.setDBParameter", _
                                              message:="Native Internal Connection not available")
                        Return False
                    End If
                Else
                    Call CoreMessageHandler(subname:="oleDBDriver.setDBParameter", _
                                          message:="Connection not available")
                    Return False
                End If

            End If

            '** init driver
            If Not Me.IsInitialized Then
                Me.Initialize()
            End If
            Try
                '** on Bootstrapping in the cache
                '** but bootstrapping mode is not sufficnt
                If _BootStrapParameterCache IsNot Nothing AndAlso _ParametersTable Is Nothing Then
                    If _BootStrapParameterCache.ContainsKey(key:=parametername) Then
                        _BootStrapParameterCache.Remove(key:=parametername)
                    End If
                    _BootStrapParameterCache.Add(key:=parametername, value:=value)
                    Return True

                Else
                    '** diretc in the table
                    dataRows = _ParametersTable.Select("[" & ConstFNID & "]='" & parametername & "'")

                    ' not found
                    If dataRows.GetLength(0) = 0 Then
                        If updateOnly And silent Then
                            SetDBParameter = False
                            Exit Function
                        ElseIf updateOnly And Not silent Then
                            Call CoreMessageHandler(showmsgbox:=True, _
                                                  message:="The Parameter '" & parametername & "' was not found in the Table " & ConstParameterTableName, subname:="oleDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
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
                    dataRows(0)(ConstFNValue) = CStr(value)
                    dataRows(0)(ConstFNChangedOn) = Date.Now().ToString
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
                End If

            Catch ex As Exception
                ' Handle the error

                Call CoreMessageHandler(showmsgbox:=silent, subname:="oleDBDriver.setDBParameter", _
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

            Dim dataRows() As DataRow

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = _primaryConnection.NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="oleDBDriver.getDBParameter", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="oleDBDriver.getDBParameter", message:="Connection not available")
                    Return Nothing
                End If
            End If

            Try
                '** init driver
                If Not Me.IsInitialized Then
                    Me.Initialize()
                End If

                '** on Bootstrapping out of the cache
                '** but bootstrapping mode is not sufficnt
                If _BootStrapParameterCache IsNot Nothing AndAlso _ParametersTable Is Nothing Then
                    If _BootStrapParameterCache.ContainsKey(key:=parameterename) Then
                        Return _BootStrapParameterCache.Item(key:=parameterename)
                    Else
                        Return Nothing
                    End If
                Else
                    '** select row
                    dataRows = _ParametersTable.Select("[" & ConstFNID & "]='" & parameterename & "'")

                    ' not found
                    If dataRows.GetLength(0) = 0 Then
                        If silent Then
                            Return Nothing
                        ElseIf Not silent Then
                            Call CoreMessageHandler(showmsgbox:=True, _
                                                  message:="The Parameter '" & parameterename & "' was not found in the OTDB Table " & ConstParameterTableName, subname:="oleDBDriver.setdbparameter", messagetype:=otCoreMessageType.ApplicationError)
                            Return Nothing

                        End If
                    End If

                    ' value
                    Return dataRows(0)(ConstFNValue)
                End If


                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="oleDBDriver.getDBParameter", tablename:=ConstParameterTableName, _
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
        Implements iormDatabaseDriver.RunSqlStatement
            Dim anativeConnection As System.Data.OleDb.OleDbConnection
            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    anativeConnection = DirectCast(Me.CurrentConnection, adonetConnection).NativeInternalConnection
                    If anativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="oleDBDriver.runSQLCommand", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="oleDBDriver.runSQLCommand", message:="Connection not available")
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
                Call CoreMessageHandler(subname:="oleDBDriver.runSQLCommand", exception:=ex, arg1:=sqlcmdstr)
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


    ''' <summary>
    ''' OLE DB OnTrack Database Connection Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class oledbConnection
        Inherits adonetConnection
        Implements iormConnection

        'Protected Friend Shadows _nativeConnection As OleDbConnection
        'Protected Friend Shadows _nativeinternalConnection As OleDbConnection

        Public Shadows Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Shadows Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

        Public Sub New(ByVal id As String, ByRef databaseDriver As iormDatabaseDriver, ByRef session As Session, sequence As ComplexPropertyStore.Sequence)
            MyBase.New(id, databaseDriver, session, sequence)
        End Sub

        Public Shadows Function RaiseOnConnected() Handles MyBase.OnConnection
            RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
        End Function
        Public Shadows Function RaiseOnDisConnected() Handles MyBase.OnDisconnection
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
        End Function
        ''' <summary>
        ''' gets the native connection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Overrides ReadOnly Property NativeConnection() As Object
            Get
                Return _nativeConnection
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the connection.
        ''' </summary>
        ''' <value>The connection.</value>
        Public Property OledbConnection() As OleDb.OleDbConnection
            Get
                If _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ConnectionState.Closed Then
                    Return Nothing
                Else
                    Dim otdbcn As oledbConnection
                    Return DirectCast(Me.NativeConnection, System.Data.OleDb.OleDbConnection)
                End If

            End Get
            Protected Friend Set(value As OleDb.OleDbConnection)
                Me._nativeConnection = value
            End Set
        End Property


        ''' <summary>
        ''' create a new SQLConnection
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function CreateNewNativeConnection() As IDbConnection
            Return New System.Data.OleDb.OleDbConnection()
        End Function

    End Class


    ''' <summary>
    ''' CLASS describes the schema per table of the database itself
    ''' </summary>
    ''' <remarks></remarks>
    Public Class oledbTableSchema
        Inherits adonetTableSchema
        Implements iotDataSchema


        '***** internal variables
        '*****
        'Protected Friend Shadows _Connection As clsOLEDBConnection

        Public Sub New(ByRef connection As oledbConnection, ByVal tableID As String)
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
            Dim aDBColumnDescription As adoNetColumnDescription = GetColumnDescription(Me.GetFieldordinal(fieldname))
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
                Call CoreMessageHandler(subname:="oleDBTableSchema.buildParameter", message:="ColumnDescription couldn't be loaded", _
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
            Dim aCon As System.Data.OleDb.OleDbConnection = DirectCast(DirectCast(_Connection, oledbConnection).NativeInternalConnection, System.Data.OleDb.OleDbConnection)


            ' return if no TableID
            If _TableID = "" Then
                Call CoreMessageHandler(subname:="oleDBTableSchema.refresh", _
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
                    Call CoreMessageHandler(subname:="oleDBTableSchema.Refresh", tablename:=Me.TableID, _
                                          messagetype:=otCoreMessageType.InternalError, message:="table has no fields - does it exist ?")
                    _IsInitialized = False
                    Return False
                End If

                ReDim _Fieldnames(no - 1)
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
                        aColumnName = UCase(row.ColumnName.Substring(row.ColumnName.IndexOf(".") + 1, row.ColumnName.Length - row.ColumnName.IndexOf(".") + 1))
                    Else
                        aColumnName = UCase(row.ColumnName)
                    End If
                    '*
                    _Fieldnames(i) = aColumnName.ToUpper
                    '* set the description
                    _Columns(i) = New adoNetColumnDescription
                    With _Columns(i)
                        .ColumnName = aColumnName.ToUpper
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
                    If _fieldsDictionary.ContainsKey(aColumnName.ToUpper) Then
                        _fieldsDictionary.Remove(aColumnName.ToUpper)
                    End If
                    ' add
                    _fieldsDictionary.Add(key:=aColumnName.ToUpper, value:=i + 1) 'store no field 1... not the array index

                    '* 
                    i = i + 1
                Next



                '**** read each Index
                '****
                Dim anIndexName As String = ""
                For Each row In columnsIndexList

                    If row.ColumnName.Contains(".") Then
                        aColumnName = UCase(row.ColumnName.Substring(row.ColumnName.IndexOf(".") + 1, row.ColumnName.Length))
                    Else
                        aColumnName = UCase(row.ColumnName)
                    End If

                    If row.IndexName.ToUpper <> anIndexName.ToUpper Then
                        '** store
                        If anIndexName <> "" Then
                            If _indexDictionary.ContainsKey(anIndexName.ToUpper) Then
                                _indexDictionary.Remove(key:=anIndexName.ToUpper)
                            End If
                            _indexDictionary.Add(key:=anIndexName.ToUpper, value:=aColumnCollection)
                        End If
                        ' new
                        anIndexName = row.IndexName.ToUpper
                        aColumnCollection = New ArrayList
                    End If
                    '** Add To List
                    aColumnCollection.Add(aColumnName.ToUpper)

                    ' indx no
                    index = _fieldsDictionary.Item(aColumnName.ToUpper)
                    '
                    '** check if primaryKey
                    'fill old primary Key structure
                    If row.isPrimaryKey Then
                        _PrimaryKeyIndexName = row.IndexName.ToUpper
                        _NoPrimaryKeys = _NoPrimaryKeys + 1
                        ReDim Preserve _Primarykeys(0 To _NoPrimaryKeys - 1)
                        _Primarykeys(_NoPrimaryKeys - 1) = index - 1 ' set to the array 0...ubound
                    End If

                    If Not _fieldsDictionary.ContainsKey(aColumnName.ToUpper) Then
                        Call CoreMessageHandler(subname:="oleDBTableSchema.refresh", _
                                              message:="oleDBTableSchema : column " & row.ColumnName & " not in dictionary ?!", _
                                              tablename:=TableID, entryname:=row.ColumnName)

                        Return False
                    End If

                Next
                '** store final
                If anIndexName <> "" Then
                    If _indexDictionary.ContainsKey(anIndexName.ToUpper) Then
                        _indexDictionary.Remove(key:=anIndexName.ToUpper)
                    End If
                    _indexDictionary.Add(key:=anIndexName.ToUpper, value:=aColumnCollection)
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
                Call CoreMessageHandler(showmsgbox:=False, subname:="oleDBTableSchema.refresh", tablename:=_TableID, _
                                      arg1:=reloadForce, exception:=ex)
                _IsInitialized = False
                Return False
            End Try

        End Function

    End Class

    ''' <summary>
    ''' describes the ORM Mapping Function per Table for OLE DB
    ''' </summary>
    ''' <remarks></remarks>
    Public Class oledbTableStore
        Inherits adonetTableStore
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
        Public Overrides Function Convert2ColumnData(ByVal invalue As Object, ByRef outvalue As Object, _
                                                     targetType As Long, _
                                                     Optional ByVal maxsize As Long = 0, _
                                                    Optional ByRef abostrophNecessary As Boolean = False, _
                                                    Optional ByVal fieldname As String = "", _
                                                    Optional isnullable? As Boolean = Nothing, _
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
            Return Connection.DatabaseDriver.Convert2DBData(invalue:=invalue, outvalue:=outvalue, _
                                                            targetType:=targetType, maxsize:=maxsize, abostrophNecessary:=abostrophNecessary, _
                                       fieldname:=fieldname, isnullable:=isnullable, defaultvalue:=defaultvalue)
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
        Public Overrides Function Convert2ObjectData(ByVal index As Object, _
                                                     ByVal invalue As Object, _
                                                     ByRef outvalue As Object, _
                                                     Optional isnullable As Boolean? = Nothing, _
                                                     Optional defaultvalue As Object = Nothing, _
                                                     Optional ByRef abostrophNecessary As Boolean = False) As Boolean Implements iormDataStore.Convert2ObjectData
            Dim aSchema As oledbTableSchema = Me.TableSchema
            Dim aDBColumn As oledbTableSchema.adoNetColumnDescription
            Dim result As Object
            Dim fieldno As Integer

            result = Nothing

            Try

                fieldno = aSchema.GetFieldordinal(index)
                If fieldno < 0 Then
                    Call CoreMessageHandler(subname:="oledbTableStore.cvt2ColumnData", messagetype:=otCoreMessageType.InternalError, _
                                          message:="iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found", _
                                          tablename:=Me.TableID, arg1:=index)
                    System.Diagnostics.Debug.WriteLine("iOTDBTableStore " & Me.TableID & " anIndex for " & index & " not found")

                    Return False
                Else
                    aDBColumn = aSchema.GetColumnDescription(fieldno)
                End If
                abostrophNecessary = False
                If Not isnullable.HasValue Then
                    isnullable = Me.TableSchema.GetNullable(index)
                End If
                If defaultvalue = Nothing Then
                    defaultvalue = Me.TableSchema.GetDefaultValue(index)
                End If
                '*
                '*
                'If IsError(aValue) Then
                '    System.Diagnostics.Debug.WriteLine "Error in Formular of field invalue " & aValue & " while updating OTDB"
                '    aValue = ""
                'End If

                If aDBColumn.DataType = OleDbType.BigInt OrElse aDBColumn.DataType = OleDbType.Integer _
                OrElse aDBColumn.DataType = OleDbType.SmallInt OrElse aDBColumn.DataType = OleDbType.TinyInt _
                OrElse aDBColumn.DataType = OleDbType.UnsignedBigInt OrElse aDBColumn.DataType = OleDbType.UnsignedInt _
                OrElse aDBColumn.DataType = OleDbType.UnsignedSmallInt OrElse aDBColumn.DataType = OleDbType.UnsignedTinyInt _
                OrElse aDBColumn.DataType = OleDbType.SmallInt OrElse aDBColumn.DataType = OleDbType.TinyInt Then
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
                        Call CoreMessageHandler(subname:="oledbTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                              message:="OTDB data '" & invalue & "' is not convertible to Integer", _
                                              arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If


                ElseIf aDBColumn.DataType = OleDbType.Char OrElse aDBColumn.DataType = OleDbType.BSTR OrElse aDBColumn.DataType = OleDbType.LongVarChar _
                OrElse aDBColumn.DataType = OleDbType.LongVarWChar OrElse aDBColumn.DataType = OleDbType.VarChar OrElse aDBColumn.DataType = OleDbType.VarWChar _
                OrElse aDBColumn.DataType = OleDbType.WChar Then
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

                ElseIf aDBColumn.DataType = OleDbType.Date OrElse aDBColumn.DataType = OleDbType.DBDate OrElse aDBColumn.DataType = OleDbType.DBTime _
                OrElse aDBColumn.DataType = OleDbType.DBTimeStamp Then
                    If defaultvalue Is Nothing Then defaultvalue = Convert.ToDateTime(ConstNullDate)
                    If isnullable Then
                        result = New Nullable(Of DateTime)
                    Else
                        result = New DateTime
                    End If

                    If isnullable AndAlso (Not IsDate(invalue) OrElse invalue Is Nothing OrElse DBNull.Value.Equals(invalue) _
                                            OrElse String.IsNullOrWhiteSpace(invalue)) Then
                        result = New Nullable(Of DateTime)
                    ElseIf (Not IsDate(invalue) OrElse invalue Is Nothing OrElse DBNull.Value.Equals(invalue) OrElse IsError(invalue)) OrElse String.IsNullOrWhiteSpace(invalue) Then
                        result = Convert.ToDateTime(defaultvalue)
                    ElseIf IsDate(invalue) Then
                        result = Convert.ToDateTime(invalue)
                    Else
                        Call CoreMessageHandler(subname:="oledbTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                            message:="OTDB data '" & invalue & "' is not convertible to Date", _
                                            arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If

                ElseIf aDBColumn.DataType = OleDbType.Double OrElse aDBColumn.DataType = OleDbType.Decimal _
                OrElse aDBColumn.DataType = OleDbType.Single OrElse aDBColumn.DataType = OleDbType.Numeric Then
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
                        Call CoreMessageHandler(subname:="oledbTableStore.conver2ObjectData", messagetype:=otCoreMessageType.InternalError, _
                                             message:="OTDB data '" & invalue & "' is not convertible to Double", _
                                             arg1:=aDBColumn.DataType, tablename:=Me.TableID, entryname:=aDBColumn.ColumnName)
                        Return False
                    End If


                ElseIf aDBColumn.DataType = OleDbType.Boolean Then
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
                Call CoreMessageHandler(showmsgbox:=False, subname:="oledbTableStore.cvt2ObjData", _
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
                    aDataSet = DirectCast(Me.Connection.DatabaseDriver, oleDBDriver).OnTrackDataSet
                    ' Select Command
                    aCommand = DirectCast(Me.TableSchema, oledbTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          oledbTableSchema.CommandType.SelectType)
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
                        _cacheAdapter.SelectCommand.Connection = DirectCast(Me.Connection.NativeConnection, System.Data.OleDb.OleDbConnection)
                        _cacheAdapter.FillSchema(aDataSet, SchemaType.Source)
                        DirectCast(_cacheAdapter, System.Data.OleDb.OleDbDataAdapter).Fill(aDataSet, Me.TableID)
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
                    aCommand = DirectCast(Me.TableSchema, oledbTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          oledbTableSchema.CommandType.DeleteType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.DeleteCommand = aCommand
                    End If

                    ' Insert Command
                    aCommand = DirectCast(Me.TableSchema, oledbTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          oledbTableSchema.CommandType.InsertType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.InsertCommand = aCommand
                    End If
                    ' Update Command
                    aCommand = DirectCast(Me.TableSchema, oledbTableSchema).GetCommand(Me.TableSchema.PrimaryKeyIndexName, _
                                                                                          oledbTableSchema.CommandType.UpdateType)
                    If Not aCommand Is Nothing Then
                        _cacheAdapter.UpdateCommand = aCommand
                    End If

                    '** return true
                    Return True
                Else
                    Return False
                End If



            Catch ex As Exception
                Call CoreMessageHandler(subname:="oledbTableStore.initializeCache", exception:=ex, message:="Exception", _
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
                Return DirectCast(dataadapter, System.Data.OleDb.OleDbDataAdapter).Update(datatable)

            Catch ex As OleDb.OleDbException
                Call CoreMessageHandler(message:="Exception occured", subname:="oledbTableStore.UpdateDBDataTable", exception:=ex, _
                                    messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return Nothing
            Catch ex As Exception
                Call CoreMessageHandler(message:="Exception occured", subname:="oledbTableStore.UpdateDBDataTable", exception:=ex, _
                                       messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID)
                Return 0
            End Try

        End Function
    End Class
End Namespace