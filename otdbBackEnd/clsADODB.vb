REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Driver Wrapper Classes for ADO.CLASSIC On Track Database Backend Library
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
Imports System.Collections.Generic
Imports System.Diagnostics.Debug

Imports System.Data
Imports ADODB
Imports ADOX

Imports OnTrack
Imports OnTrack.UI

Namespace OnTrack.Database


    '************************************************************************************
    '***** CLASS clsADOCDriver describes the Database Driver to OnTrack
    '*****       based on ADO Classic 
    '*****

    Public Class clsADOCDriver
        Inherits ormDBDriver
        Implements iormDBDriver


        'Private _NativeConnection As ADODB.Connection
        Private _Catalog As ADOX.Catalog
        Private WithEvents _primaryConnection As clsADODBConnection

        Public Sub New(ID As String, ByRef session As Session)
            Call MyBase.New(ID, session)
            Me.ID = ID
            If _primaryConnection Is Nothing Then
                _primaryConnection = New clsADODBConnection("primary", Me, session)
            End If
        End Sub





        ''' <summary>
        ''' create a tablestore
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <param name="forceSchemaReload"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Protected Friend Overrides Function CreateNativeTableStore(TableID As String, forceSchemaReload As Boolean) As iormDataStore
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' create a tableschema
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Protected Friend Overrides Function CreateNativeTableSchema(TableID As String) As iotTableSchema
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' converts value to targetType of the native DB Driver
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="targetType"></param>
        ''' <param name="maxsize"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <param name="fieldname"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function Convert2DBData(value As Object, targetType As Long, Optional maxsize As Long = 0, Optional ByRef abostrophNecessary As Boolean = False, Optional fieldname As String = "") As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' create an assigned Native DBParameter to provided name and type
        ''' </summary>
        ''' <param name="parametername"></param>
        ''' <param name="datatype"></param>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function AssignNativeDBParameter(parametername As String, datatype As otFieldDataType, Optional maxsize As Long = 0, Optional value As Object = Nothing) As IDbDataParameter
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Create a Native IDBCommand (Sql Command)
        ''' </summary>
        ''' <param name="cmd"></param>
        ''' <param name="aNativeConnection"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function CreateNativeDBCommand(cmd As String, aNativeConnection As IDbConnection) As IDbCommand
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Create the User Definition Table
        ''' </summary>
        ''' <param name="nativeConnection"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' create the DB Parameter Table
        ''' </summary>
        ''' <param name="nativeConnection"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean
            ' TODO: Implement this method
            Throw New NotImplementedException()
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
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function
        Public Overrides Function RunSqlSelectCommand(id As String, _
                                                     Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                     Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                                   Implements iormDBDriver.RunSqlSelectCommand
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function
        ''' <summary>
        ''' verifyOnTrack
        ''' </summary>
        ''' <param name="verifyOnly"></param>
        ''' <param name="CreateOnMissing"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function VerifyOnTrackDatabase(verifyOnly As Boolean, createOnMissing As Boolean) As Boolean Implements iormDBDriver.VerifyOnTrackDatabase
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Gets the Type
        ''' </summary>
        ''' <value>The ID.</value>
        Public Overrides ReadOnly Property Type() As otDbDriverType Implements iormDBDriver.Type
            Get
                Return otDbDriverType.ADOClassic
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the catalog.
        ''' </summary>
        ''' <value>The catalog.</value>
        Public Property Catalog() As ADOX.Catalog
            Get
                Return Me._Catalog
            End Get
            Set(value As ADOX.Catalog)
                Me._Catalog = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the connection.
        ''' </summary>
        ''' <value>The connection.</value>
        Public ReadOnly Property CurrentConnection() As iormConnection Implements iormDBDriver.CurrentConnection
            Get
                Return Me._primaryConnection
            End Get

        End Property
        Public Property NativeConnection() As ADODB.Connection
            Get
                Return _primaryConnection.ADODBConnection
            End Get
            Set(value As ADODB.Connection)
                _primaryConnection.ADODBConnection = value
            End Set
        End Property
        '********
        '******** getOTDBCatalog: returns the ADODB Catalog
        '********
        Public Overrides Function GetCatalog(Optional ByVal FORCE As Boolean = False, _
                                            Optional ByRef NativeConnection As Object = Nothing) As Object Implements iormDBDriver.GetCatalog

            Dim aADOConnection As ADODB.Connection = Nothing

            If Not NativeConnection Is Nothing Then
                Try
                    aADOConnection = DirectCast(NativeConnection, ADODB.Connection)
                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetCatalog", exception:=ex, message:="couldn't cast to ADODB.connection")
                    Return Nothing
                End Try
            Else
                If Not Me.CurrentConnection Is Nothing Then
                    If Not Me.CurrentConnection.NativeConnection Is Nothing Then
                        aADOConnection = DirectCast(Me.CurrentConnection.NativeConnection, ADODB.Connection)
                    Else
                        Call CoreMessageHandler(subname:="clsADOCDriver.GetCatalog", message:="Native Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetCatalog", message:="Connection not available")
                    Return Nothing
                End If
            End If

            GetCatalog = GetAdoxCatalog(FORCE, aADOConnection)
        End Function
        Protected Function GetAdoxCatalog(ByVal FORCE As Boolean, _
                                          ByRef NativeConnection As ADODB.Connection) As ADOX.Catalog



            If Not _Catalog Is Nothing And Not FORCE Then
                Return _Catalog

            Else


                Try
                    _Catalog = New ADOX.Catalog
                    _Catalog.ActiveConnection = NativeConnection
                    Return _Catalog

                Catch ex As Exception
                    Call CoreMessageHandler(exception:=ex, subname:="clsADOCDriver.GetAdoxCatalog")
                    Return Nothing
                End Try

            End If
            Return Nothing
        End Function

        '*********
        '********* getTable returns a ADOX Table Object or creates one
        '*********
        Public Overrides Function GetTable(ByVal tablename As String, _
                                           Optional ByVal createOnMissing As Boolean = True, _
                                           Optional ByVal addToSchemaDir As Boolean = True, _
                                           Optional ByRef nativeConnection As Object = Nothing, _
                                           Optional ByRef nativeTableObject As Object = Nothing) As Object Implements iormDBDriver.GetTable

            Dim anADOConnecton As ADODB.Connection = Nothing
            Dim aTable As ADOX.Table

            '*** check on rights
            If createOnMissing Then
                If Me.CurrentConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetTable", message:="No current Connection to the Database")
                    Return Nothing
                Else
                    If Not Me.CurrentConnection.VerifyUserAccess(otAccessRight.otAlterSchema) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsADOCDriver.GetTable", message:="No right to alter schema of database")
                        Return Nothing
                    End If
                End If
            End If

            '*** cast the native connection
            Try
                If Not nativeConnection Is Nothing Then
                    anADOConnecton = DirectCast(nativeConnection, ADODB.Connection)
                Else
                    anADOConnecton = DirectCast(Me.CurrentConnection.NativeConnection, ADODB.Connection)
                End If
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADODB.getTable", exception:=ex, message:="invalid cast to ADODB.Connection")
                Return Nothing
            End Try

            Try
                Dim result As Object
                result = GetADOXTable(tablename, createOnMissing, addToSchemaDir, anADOConnecton, nativeTableObject)
                Return aTable

            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        '**** real function

        Protected Function GetADOXTable(ByVal tableID As String, _
                         ByVal createOnMissing As Boolean, _
                         ByVal addToSchemaDir As Boolean, _
                         ByRef nativeConnection As ADODB.Connection, _
                         ByRef nativeTableObject As ADOX.Table) As ADOX.Table
            Dim newTable As New ADOX.Table
            Dim otdbcn As ADODB.Connection
            Dim catalog As ADOX.Catalog
            Dim aSchemaDir As ObjectDefinition
            Dim cmd As ADODB.Command



            Try
                catalog = Me.GetAdoxCatalog(FORCE:=True, NativeConnection:=nativeConnection)
                If catalog Is Nothing Then
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getADOXTable", tablename:=tableID, _
                                          message:="catalog couldn't be obtained")
                    Return Nothing
                End If
                ' get the table
                GetADOXTable = catalog.Tables(tableID)

                ' check if containskey -> write
                If addToSchemaDir Then
                    ' set it here -> bootstrapping will fail otherwise
                    aSchemaDir = New ObjectDefinition
                    If Not aSchemaDir.LoadBy(tableID) Then
                        Call aSchemaDir.Create(tableID)
                        Call aSchemaDir.Persist()
                    End If
                End If
                ' return if existing
                Exit Function


            Catch ex As Exception

                If Not createOnMissing Then
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getADOXTable", tablename:=tableID, _
                                          message:="table is missing in database and flag not set to create it")
                    Return Nothing
                End If

                Try

                    ' create a new table
                    If nativeTableObject Is Nothing Then
                        newTable = New ADOX.Table
                    Else
                        newTable = nativeTableObject
                    End If
                    newTable.Name = tableID


                    ' create the table in the catalog
                    ' ADOX Style sometimes not working ?!
                    cmd = New ADODB.Command
                    cmd.ActiveConnection = nativeConnection

                    cmd.CommandText = "CREATE TABLE " & tableID    '& " "
                    cmd.CommandType = CommandTypeEnum.adCmdText
                    Call cmd.Execute(Options:=CommandTypeEnum.adCmdText)
                    'cmd.ActiveConnection.Close()

                    With catalog.Tables
                        '.Append newTable
                        Call .Refresh()
                    End With
                    ' set it
                    GetADOXTable = catalog.Tables(tableID)
                    If GetADOXTable Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADOCDriver.getADOXTable", tablename:=tableID, message:="Error while creating Table")
                    End If
                    ' check if containskey -> write
                    If addToSchemaDir Then
                        ' set it here -> bootstrapping will fail otherwise
                        aSchemaDir = New ObjectDefinition
                        Call aSchemaDir.Create(tableID)
                        Call aSchemaDir.Persist()
                    End If

                    Exit Function



                    '* Handle the error
                    '*
                Catch ex2 As Exception

                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getADOXTable", tablename:=tableID)
                    Return Nothing

                End Try
            End Try
        End Function

        '*********
        '********* getOTDBIndex or return the ADOX key Item if containskey
        '*********
        Public Overrides Function GetIndex(ByRef nativeTABLE As Object, _
                                           ByRef indexname As String, _
                                           ByRef ColumnNames As List(Of String), _
                                           Optional ByVal PrimaryKey As Boolean = False, _
                                            Optional ByVal forceCreation As Boolean = False, _
                                            Optional ByVal createOnMissing As Boolean = True, _
                                            Optional ByVal addToSchemaDir As Boolean = True) As Object Implements iormDBDriver.GetIndex


            Dim aTable As ADOX.Table

            '*** check on rights
            If createOnMissing Then
                If Me.CurrentConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetIndex", message:="No current Connection to the Database")
                    Return Nothing
                Else
                    If Not Me.CurrentConnection.VerifyUserAccess(otAccessRight.otAlterSchema) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsADOCDriver.GetIndex", message:="No right to alter schema of database")
                        Return Nothing
                    End If
                End If
            End If

            Try
                aTable = DirectCast(nativeTABLE, ADOX.Table)

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADODB.getIndex", exception:=ex, message:="invalid cast to ADOX.table")
                Return Nothing
            End Try

            Try
                Dim result As Object
                result = GetADOXIndex(aTable, indexname, ColumnNames, PrimaryKey, forceCreation, createOnMissing, _
                                      addToSchemaDir)
                Return result
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
        '***
        '*** real function
        Protected Function GetADOXIndex(ByRef TABLE As ADOX.Table, _
                                        ByRef indexname As String, _
                                        ByRef ColumnNames As List(Of String), _
                                        ByVal PrimaryKey As Boolean, _
                                         ByVal forceCreation As Boolean, _
                                         ByVal createOnMissing As Boolean, _
                                         ByVal addToSchemaDir As Boolean) As ADOX.Index

            Dim newColumn As New ADOX.Column

            Dim ind, anIndex As ADOX.Index
            Dim existingIndex, indexnotchanged As Boolean
            Dim existPrimaryName As String
            Dim ColumnName As Object
            Dim aColumn As Object
            Dim i, j As Integer
            Dim aSchemaDir As ObjectEntryDefinition

            Try
                existingIndex = False
                indexnotchanged = False
                anIndex = Nothing

                ' save the primary name
                For Each ind In TABLE.Indexes
                    If LCase(ind.Name) = LCase(indexname) Then
                        existingIndex = True
                        anIndex = ind
                    End If
                    If ind.PrimaryKey Then
                        existPrimaryName = ind.Name
                        If indexname = "" Then
                            indexname = ind.Name
                            existingIndex = True
                            anIndex = ind
                        End If
                    End If
                Next ind

                ' exit if not existing and not wanted
                If (Not forceCreation And Not createOnMissing) And Not existingIndex Then
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getOTDBIndex", tablename:=TABLE.Name, entryname:=ColumnName, arg1:="Indexname " & indexname, _
                                          message:="Index is not found in database")
                    GetADOXIndex = Nothing
                    Exit Function
                End If

                ' check all Members
                If Not forceCreation And existingIndex Then
                    i = 0
                    For Each ColumnName In ColumnNames
                        ' check
                        If Not IsNothing(ColumnName) Then
                            'For j = i To anIndex.Columns.count
                            ' not equal
                            aColumn = anIndex.Columns(i)
                            If LCase(aColumn.Name) <> LCase(ColumnName) Then
                                indexnotchanged = False
                                Exit For
                            Else
                                indexnotchanged = True
                                ' check if containskey -> write
                                If addToSchemaDir And anIndex.PrimaryKey Then
                                    ' set it here -> bootstrapping will fail otherwise
                                    aSchemaDir = New ObjectEntryDefinition
                                    If Not aSchemaDir.LoadBy(TABLE.Name, entryname:=ColumnName) Then
                                        Call aSchemaDir.Create(TABLE.Name, entryname:=ColumnName)
                                    End If
                                    aSchemaDir.Indexname = indexname
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
                    Next ColumnName
                    ' return
                    If indexnotchanged Then
                        GetADOXIndex = anIndex
                        Exit Function
                    End If
                End If

                ' if we have another Primary
                If PrimaryKey And LCase(indexname) <> LCase(existPrimaryName) And existPrimaryName <> "" Then
                    'IndexName is found and not the same ?!
                    System.Diagnostics.Debug.WriteLine("IndexName of table " & TABLE.Name & " is " & ind.Name & " and not " & indexname & " - getOTDBIndex aborted")
                    GetADOXIndex = Nothing
                    Exit Function
                    ' create primary key
                ElseIf PrimaryKey And existPrimaryName = "" Then
                    'create primary
                    anIndex = New ADOX.Index
                    If indexname = "" Then
                        anIndex.Name = "PrimaryKey"
                    Else
                        anIndex.Name = indexname
                    End If
                    anIndex.PrimaryKey = True
                    'ind.Clustered = True
                    anIndex.IndexNulls = AllowNullsEnum.adIndexNullsAllow
                    '** extend PrimaryKey
                ElseIf PrimaryKey And LCase(indexname) = LCase(existPrimaryName) Then
                    TABLE.Indexes.Delete(existPrimaryName)
                    anIndex = New ADOX.Index
                    If indexname = "" Then
                        anIndex.Name = "PrimaryKey"
                    Else
                        anIndex.Name = indexname
                    End If
                    anIndex.PrimaryKey = True
                    anIndex.IndexNulls = AllowNullsEnum.adIndexNullsAllow
                    '** extend Index -> Drop
                ElseIf Not PrimaryKey And existingIndex Then
                    TABLE.Indexes.Delete(indexname)
                    anIndex = New ADOX.Index
                    anIndex.Name = indexname
                    anIndex.IndexNulls = AllowNullsEnum.adIndexNullsAllow
                    '** create new
                ElseIf Not PrimaryKey And Not existingIndex Then
                    anIndex = New ADOX.Index
                    anIndex.Name = indexname
                    anIndex.IndexNulls = AllowNullsEnum.adIndexNullsAllow
                End If

                ' check on keys & indexes
                For Each ColumnName In ColumnNames
                    If Not IsNothing(ColumnName) Then
                        anIndex.Columns.Append(ColumnName)
                        ' check if containskey -> write
                        If addToSchemaDir And anIndex.PrimaryKey Then
                            ' set it here -> bootstrapping will fail otherwise
                            aSchemaDir = New ObjectEntryDefinition
                            If Not aSchemaDir.LoadBy(TABLE.Name, entryname:=ColumnName) Then
                                Call aSchemaDir.Create(TABLE.Name, entryname:=ColumnName)
                            End If
                            aSchemaDir.Indexname = indexname
                            aSchemaDir.IndexPosition = i + 1
                            aSchemaDir.IsKey = True
                            aSchemaDir.IsPrimaryKey = True
                            Call aSchemaDir.Persist()
                        End If
                    Else
                        System.Diagnostics.Debug.WriteLine("Nothing ColumnName in getOTDBIndex List")
                    End If
                Next ColumnName

                ' attach the Index
                If Not anIndex Is Nothing Then
                    TABLE.Indexes.Append(anIndex)
                End If

                '*** return
                Return anIndex


                '* Handle the error
                '*
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getOTDBIndex", _
                                      tablename:=TABLE.Name, entryname:=ColumnName, arg1:="Indexname " & indexname, exception:=ex)

                Return Nothing

            End Try


        End Function
        '*********
        '********* getColumn or return the ADOX Column Item if containskey
        '*********
        Public Overrides Function GetColumn(nativeTABLE As Object, _
                                            FieldDesc As ormFieldDescription, _
                                            Optional ByVal createOnMissing As Boolean = True, _
                                            Optional ByVal addToSchemaDir As Boolean = True) As Object Implements iormDBDriver.GetColumn


            Dim aTable As ADOX.Table

            '*** check on rights
            If createOnMissing Then
                If Me.CurrentConnection Is Nothing Then
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetColumn", message:="No current Connection to the Database")
                    Return Nothing
                Else
                    If Not Me.CurrentConnection.VerifyUserAccess(otAccessRight.otAlterSchema) Then
                        Call CoreMessageHandler(showmsgbox:=True, subname:="clsADOCDriver.GetColumn", message:="No right to alter schema of database")
                        Return Nothing
                    End If
                End If
            End If

            '*** cast to nativeTable
            Try
                aTable = DirectCast(nativeTABLE, ADOX.Table)

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADODB.getColumn", exception:=ex, message:="invalid cast to ADOX.table")
                Return Nothing
            End Try

            Try
                Dim result As Object
                result = GetADOXColumn(TABLE:=aTable, FieldDesc:=FieldDesc, createOnMissing:=createOnMissing, addToSchemaDir:=addToSchemaDir)
                Return result
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        '*** real function
        '***
        Protected Function GetADOXColumn(TABLE As ADOX.Table, _
                                         FieldDesc As ormFieldDescription, _
                                         ByVal createOnMissing As Boolean, _
                                         ByVal addToSchemaDir As Boolean) As ADOX.Column
            Dim newColumn As New ADOX.Column
            Dim keyfound, colfound As Boolean
            Dim aSchemaDir As New ObjectEntryDefinition

            Try

                GetADOXColumn = TABLE.Columns(FieldDesc.ColumnName)
                ' check if containskey -> write
                If addToSchemaDir Then
                    ' set it here -> bootstrapping will fail otherwise
                    aSchemaDir = New ObjectEntryDefinition
                    If Not aSchemaDir.LoadBy(TABLE.Name, entryname:=FieldDesc.ColumnName) Then
                        Call aSchemaDir.Create(TABLE.Name, entryname:=FieldDesc.ColumnName)
                    End If
                    aSchemaDir.Typeid = otSchemaDefTableEntryType.otField
                    Call aSchemaDir.SetByFieldDesc(FieldDesc)
                    'aSchemaDir.isPrimaryKey = aDBDesc.OTDBPrimaryKeys
                    aSchemaDir.IsPrimaryKey = False
                    Call aSchemaDir.Persist()

                End If

                'newColumn.Properties("Jet OLEDB:Allow Zero Length") = True
                'newColumn.Attributes = adColNullable
                'newColumn.Properties("Description").value = aDBDesc.Title

                ' check on keys
                keyfound = False
                colfound = False

                Exit Function

                '**** not found
                '****
            Catch e As Exception

                Try

                    ' Exit if not meant to be created
                    If Not createOnMissing Then
                        Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getOTDBColumn", tablename:=TABLE.Name, entryname:=FieldDesc.ColumnName, message:="Column of table is missing in table")
                        Return Nothing

                    End If

                    newColumn = New ADOX.Column

                    newColumn.Name = FieldDesc.ColumnName
                    'Set newColumn.ParentCatalog = Table.ParentCatalog
                    'newColumn.Type =
                    Select Case FieldDesc.Datatype
                        Case otFieldDataType.[Long]
                            newColumn.Type = ADOX.DataTypeEnum.adInteger
                        Case otFieldDataType.Numeric
                            newColumn.Type = ADOX.DataTypeEnum.adDouble
                            'newColumn.NumericScale = 3
                        Case otFieldDataType.List
                            newColumn.Type = ADOX.DataTypeEnum.adVarWChar
                        Case otFieldDataType.Text
                            newColumn.Type = ADOX.DataTypeEnum.adVarWChar
                            If FieldDesc.Size > 0 Then
                                newColumn.DefinedSize = FieldDesc.Size
                            End If
                        Case otFieldDataType.Memo
                            newColumn.Type = ADOX.DataTypeEnum.adLongVarWChar
                        Case otFieldDataType.Binary
                            newColumn.Type = ADOX.DataTypeEnum.adLongVarBinary
                        Case otFieldDataType.[Date]
                            newColumn.Type = ADOX.DataTypeEnum.adDate
                        Case otFieldDataType.Timestamp
                            newColumn.Type = ADOX.DataTypeEnum.adDate    'adDBTimeStamp
                        Case otFieldDataType.Bool
                            newColumn.Type = ADOX.DataTypeEnum.adBoolean
                        Case otFieldDataType.Runtime
                        Case otFieldDataType.Formula
                            System.Diagnostics.Debug.WriteLine(" Runtime, Formula are not transferred in OTDB")

                    End Select

                    ' add it
                    Call TABLE.Columns.Append(newColumn, Type:=newColumn.Type)

                    newColumn = TABLE.Columns(FieldDesc.ColumnName)
                    If Not setColumnProperty(newColumn, "Jet OLEDB:Allow Zero Length", True) Then
                    End If
                    If Not setColumnProperty(newColumn, "Nullable", False) Then
                    End If
                    If Not setColumnProperty(newColumn, "Description", FieldDesc.Title) Then
                    End If

                    'Call printADOXColumnProperty(newColumn)

                    GetADOXColumn = TABLE.Columns(FieldDesc.ColumnName)
                    ' check if containskey -> write
                    If addToSchemaDir Then
                        aSchemaDir = New ObjectEntryDefinition
                        If Not aSchemaDir.LoadBy(TABLE.Name, entryname:=FieldDesc.ColumnName) Then
                            Call aSchemaDir.Create(TABLE.Name, entryname:=FieldDesc.ColumnName)
                        End If
                        aSchemaDir.Typeid = otSchemaDefTableEntryType.otField
                        Call aSchemaDir.SetByFieldDesc(FieldDesc)

                        'aSchemaDir.isPrimaryKey = aDBDesc.OTDBPrimaryKeys
                        aSchemaDir.IsPrimaryKey = False
                        Call aSchemaDir.Persist()
                    End If

                    Exit Function

                    '* Handle the error
                    '*

                Catch ex As Exception

                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOCDriver.getOTDBColumn", tablename:=TABLE.Name, entryname:=FieldDesc.ColumnName, exception:=ex)
                    Return Nothing

                End Try

            End Try

        End Function

        Public Function setColumnProperty(aColumn As ADOX.Column, aPropName As String, aVAlue As Object) As Boolean
            Dim aProperty As ADOX.Property

            On Error Resume Next
            For Each aProperty In aColumn.Properties
                If aProperty.Name = aPropName Then
                    aProperty.Value = aVAlue
                    setColumnProperty = True
                    Exit Function
                End If
            Next aProperty

            setColumnProperty = False
        End Function
        Public Function printColumnProperty(aColumn As ADOX.Column) As Boolean
            Dim aProperty As ADOX.Property

            For Each aProperty In aColumn.Properties
                System.Diagnostics.Debug.WriteLine(aProperty.Name, aProperty.Value)
            Next aProperty

            printColumnProperty = True
        End Function

        '**********
        '********** setDBParameter: Set a Parameter in the OTDB
        '**********

        Public Overrides Function SetDBParameter(ByVal Parametername As String, _
                                                 ByVal Value As Object, _
                                                Optional ByRef nativeConnection As Object = Nothing, _
                                                Optional ByVal UpdateOnly As Boolean = False, _
                                                Optional ByVal silent As Boolean = False) As Boolean Implements iormDBDriver.SetDBParameter

            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = DirectCast(Me.CurrentConnection, clsADODBConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADOCDriver.setDBParameter", message:="Native Internal Connection not available")
                        Return False
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADOCDriver.setDBParameter", message:="Connection not available")
                    Return False
                End If

            End If

            '*** try to cast
            Try
                otdbcn = DirectCast(nativeConnection, ADODB.Connection)
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADODCDriver.setDBParameter", exception:=ex, message:="object is not castable to ADODB.Connection")
                Return False
            End Try

            Try

                rst = New ADODB.Recordset
                rst.Open(ConstParameterTableName, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                rst.MoveFirst()
                rst.Find("[ID]='" & Parametername & "'", 0, SearchDirectionEnum.adSearchForward, 1)
                ' not found
                If rst.EOF Then
                    If UpdateOnly And silent Then
                        SetDBParameter = False
                        Exit Function
                    ElseIf UpdateOnly And Not silent Then
                        With New clsCoreUIMessageBox
                            .type = clsCoreUIMessageBox.MessageType.Warning
                            .Title = "Tooling"
                            .Message = "The Parameter '" & Parametername & "' was not found in the OTDB Table tblParametersGlobal"
                            .buttons = clsCoreUIMessageBox.ButtonType.OK
                            .Show()
                        End With

                        SetDBParameter = False
                        Exit Function
                    ElseIf Not UpdateOnly Then
                        rst.AddNew()
                    End If
                End If

                ' value
                rst.Fields("ID").Value = Parametername
                rst.Fields("Value").Value = CStr(Value)
                rst.Fields("changedOn").Value = Date.Now()
                rst.Fields("description").Value = ""
                rst.Update()

                SetDBParameter = True
                Exit Function


            Catch ex As Exception
                ' Handle the error

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOCDriver.setDBParameter", tablename:="tblParametersGlobal", entryname:=Parametername)
                SetDBParameter = False
            End Try


        End Function

        '**********
        '********** getDBParameter: get a Parameter from the OTDB
        '**********

        Public Overrides Function GetDBParameter(Parametername As String, _
                                                 Optional ByRef nativeConnection As Object = Nothing, _
                                                 Optional silent As Boolean = False) As Object Implements iormDBDriver.GetDBParameter

            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    nativeConnection = DirectCast(Me.CurrentConnection, clsADODBConnection).NativeInternalConnection
                    If nativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADOCDriver.getDBParameter", message:="Native internal Connection not available")
                        Return ""
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADOCDriver.getDBParameter", message:="Connection not available")
                    Return ""
                End If
            End If
            '** cast to ADODB Connection
            Try
                otdbcn = DirectCast(nativeConnection, ADODB.Connection)
            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOCDriver.getDBParameter", exception:=ex, message:="object is not castable to ADODB.Connection")
                Return ""
            End Try

            Try
                rst = New ADODB.Recordset
                rst.Open(ConstParameterTableName, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                rst.MoveFirst()
                rst.Find("[ID]='" & Parametername & "'", 0, SearchDirectionEnum.adSearchForward, 1)

                ' not found
                If rst.EOF Then
                    If silent Then
                        Return ""
                    Else
                        CoreMessageHandler(message:="The Parameter '" & Parametername & "' was not found in the OTDB Table tblParametersGlobal", _
                               subname:="clsADOCDriver.getDBParameter", messagetype:=otCoreMessageType.ApplicationError)
                        GetDBParameter = ""
                        Exit Function
                    End If
                End If

                ' value
                Return rst.Fields("Value").Value



                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOCDriver.getDBParameter", tablename:="tblParametersGlobal", _
                                      exception:=ex, entryname:=Parametername)
                Return Nothing
            End Try

        End Function

        '******** getDefUser to be the highes UPDC
        '********

        Public Overrides Function GetUserValidation(ByVal username As String, _
                                             Optional ByVal selectAnonymous As Boolean = False, _
                                             Optional ByRef nativeConnection As Object = Nothing) As OTDBUserValidation _
        Implements iormDBDriver.GetUserValidation
            Dim anUser As New clsOTDBDefUser
            Dim aCollection As New Collection
            Dim aVAlue As Object
            Dim UserValidation As New OTDBUserValidation
            Dim aNativeConnection As ADODB.Connection


            Dim aName As Object

            '*** get the native Connection 
            If nativeConnection Is Nothing Then
                If Not Me.CurrentConnection Is Nothing Then
                    aNativeConnection = DirectCast(Me.CurrentConnection, clsADODBConnection).NativeInternalConnection
                    If aNativeConnection Is Nothing Then
                        Call CoreMessageHandler(subname:="clsADOCDriver.GetDefUserValidation", message:="Native internal Connection not available")
                        Return Nothing
                    End If
                Else
                    Call CoreMessageHandler(subname:="clsADOCDriver.GetDefUserUserValidation", message:="Connection not available")
                    Return Nothing
                End If
            Else
                aNativeConnection = nativeConnection
            End If


            Try
                Dim rst As New ADODB.Recordset
                Dim aRecord As New ormRecord
                Dim fld As ADODB.Field
                Dim cmdstr As String

                If Not selectAnonymous Then
                    cmdstr = "select * from " & anUser.TableID & " where username='" & username & "'"
                Else
                    cmdstr = "select * from " & anUser.TableID & " where  isanon=true order by username desc"
                End If
                '** open recordset
                rst.Open(cmdstr, aNativeConnection, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic)

                If Not rst.EOF Then
                    Try
                        UserValidation.Password = rst("password").Value
                        UserValidation.Username = rst("username").Value
                        UserValidation.IsAnonymous = rst("isanon").Value
                        UserValidation.HasAlterSchemaRights = rst("alterschema").Value
                        UserValidation.HasReadRights = rst("readdata").Value
                        UserValidation.HasUpdateRights = rst("updatedata").Value
                        UserValidation.HasNoRights = rst("noright").Value
                        UserValidation.validEntry = True

                    Catch ex As Exception
                        Call CoreMessageHandler(exception:=ex, subname:="clsADOCDriver.getUserValidation", message:="Couldn't read User Validation", _
                                              break:=False, noOtdbAvailable:=True)
                        UserValidation.validEntry = False
                        Return UserValidation

                    End Try

                    ' return successfull
                    rst.Close()
                    Return UserValidation

                End If


                ' close recordset
                rst.Close()
                ' close connection

                ' return
                UserValidation.validEntry = False
                Return UserValidation



                ' Handle the error
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsADOCDriver.getDefUser", exception:=ex)

                Return Nothing

            End Try

        End Function

        ''' <summary>
        ''' Gets the table store.
        ''' </summary>
        ''' <param name="Tablename">The tablename.</param>
        ''' <param name="Force">The force.</param>
        ''' <returns></returns>
        Public Function GetTableStore(ByVal tableID As String, Optional force As Boolean = False) As iormDataStore Implements iormDBDriver.GetTableStore
            'take existing or make new one
            If _TableDirectory.ContainsKey(tableID) And Not force Then
                GetTableStore = _TableDirectory.Item(tableID)
            Else
                Dim aNewStore As iormDataStore

                ' reload the existing object on force
                If force And _TableDirectory.ContainsKey(tableID) Then
                    aNewStore = _TableDirectory.Item(tableID)
                    _TableDirectory.Remove(key:=tableID)
                End If
                ' assign the Table
                aNewStore = New clsADOTableStore(Me.CurrentConnection, tableID, force)
                If Not aNewStore Is Nothing Then
                    If Not _TableDirectory.ContainsKey(tableID) Then
                        _TableDirectory.Add(key:=tableID, value:=aNewStore)
                    End If
                End If
                ' return
                GetTableStore = aNewStore

            End If

        End Function

        ''' <summary>
        ''' Gets the table store.
        ''' </summary>
        ''' <param name="Tablename">The tablename.</param>
        ''' <param name="Force">The force.</param>
        ''' <returns></returns>
        Public Function GetTableSchema(ByVal TableID As String, Optional ByVal force As Boolean = False) As iotTableSchema _
        Implements iormDBDriver.GetTableSchema

            'take existing or make new one
            If _TableSchemaDirectory.ContainsKey(TableID) And Not force Then
                Return _TableSchemaDirectory.Item(TableID)
            Else
                Dim aNewSchema As iotTableSchema

                ' reload the existing object on force
                If force And _TableSchemaDirectory.ContainsKey(TableID) Then
                    aNewSchema = _TableSchemaDirectory.Item(TableID)
                    _TableSchemaDirectory.Remove(key:=TableID)
                End If
                ' assign the Table
                aNewSchema = New clsADODBTableSchema(Me.CurrentConnection, TableID)
                If Not aNewSchema Is Nothing Then
                    If Not _TableSchemaDirectory.ContainsKey(TableID) Then
                        _TableSchemaDirectory.Add(key:=TableID, value:=aNewSchema)
                    End If
                End If
                ' return
                Return aNewSchema

            End If
        End Function

        '****** runs a SQLCommand
        '******
        Public Overrides Function RunSqlStatement(ByVal sqlcmdstr As String, _
                                                Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                Optional silent As Boolean = True, _
                                                Optional nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.RunSqlStatement

            Dim otdbcn As ADODB.Connection
            Dim cmd As ADODB.Command
            Dim rowsaffected As Long

            ' Connection
            Try
                If Me.CurrentConnection.isConnected Then
                    otdbcn = DirectCast(Me.CurrentConnection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.runSQLCommand", message:="Connection is not available")
                    Return False
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.runSQLCommand", exception:=ex)
                Return False
            End Try

            Try
                cmd = New ADODB.Command
                cmd.ActiveConnection = otdbcn
                cmd.CommandText = sqlcmdstr

                Call cmd.Execute(RecordsAffected:=rowsaffected)

                If rowsaffected > 0 Then
                    Return True
                End If

                Return False

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.runSQLCommand", _
                                 arg1:=sqlcmdstr)

                Return False
            End Try

        End Function
        ''' <summary>
        ''' persists the errorlog
        ''' </summary>
        ''' <param name="TableID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overrides Function PersistLog(ByRef log As ErrorLog) As Boolean Implements iormDBDriver.PersistLog
            Throw New NotImplementedException

        End Function
    End Class

    '************************************************************************************
    '***** CLASS clsOTDBConnection describes the Connection description to OnTrack
    '*****
    '*****

    Public Class clsADODBConnection
        Inherits ormConnection
        Implements iormConnection

        Private _nativeConnection As ADODB.Connection
        Private _nativeinternalConnection As ADODB.Connection

        Private _ADOXcatalog As ADOX.Catalog
        Private _ADOError As ADODB.Error
        Private _useseek As Boolean 'use seek instead of SQL

        Public WithEvents _ErrorLog As New ErrorLog(My.Computer.Name & "-" & My.User.Name & "-" & Date.Now.ToUniversalTime)

        Public Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
        Public Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

        Public Sub New(id As String, ByRef DatabaseDriver As iormDBDriver, ByRef session As Session)
            MyBase.New(id, DatabaseDriver, session)

            _nativeConnection = Nothing
            _nativeinternalConnection = Nothing
        End Sub

        '*******
        '*******
        Overrides ReadOnly Property isConnected As Boolean Implements iormConnection.isConnected
            Get
                If _nativeConnection Is Nothing Then
                    Return False
                ElseIf _nativeConnection.State = ObjectStateEnum.adStateOpen Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
        '*******
        '*******
        Overrides ReadOnly Property isInitialized As Boolean Implements iormConnection.IsInitialized
            Get
                If _nativeConnection Is Nothing Then
                    Return False

                Else
                    Return True
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the ADO error.
        ''' </summary>
        ''' <value>The ADO error.</value>
        Public Property ADOError() As ADODB.Error
            Get
                Return Me._ADOError
            End Get
            Protected Friend Set(value As ADODB.Error)
                Me._ADOError = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the ADOX catalog.
        ''' </summary>
        ''' <value>The ADOX catalog.</value>
        Public Property ADOXCatalog() As ADOX.Catalog
            Get

                Return Me._ADOXcatalog
            End Get
            Protected Friend Set(value As ADOX.Catalog)
                Me._ADOXcatalog = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the connection.
        ''' </summary>
        ''' <value>The connection.</value>
        Public Property ADODBConnection() As ADODB.Connection
            Get
                If _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ObjectStateEnum.adStateOpen Then
                    Return Nothing
                Else
                    Return DirectCast(Me.NativeConnection, ADODB.Connection)
                End If

            End Get
            Set(value As ADODB.Connection)
                Me._nativeConnection = value
            End Set
        End Property
        '**** NativeInternalConnection
        Protected Friend ReadOnly Property NativeInternalConnection As Object
            Get
                If _nativeinternalConnection Is Nothing OrElse _nativeinternalConnection.State <> ObjectStateEnum.adStateOpen Then
                    Try
                        '**** retrieve ConfigParameters
                        If Not Me.SetConnectionConfigParameters() Then
                            Call CoreMessageHandler(showmsgbox:=True, message:="Configuration Parameters couldnot be retrieved from a data source", _
                                                  subname:="clsADODBConnection.Connect")
                            Return Nothing
                        End If
                        ' connect 
                        _nativeinternalConnection = New ADODB.Connection
                        _nativeinternalConnection.ConnectionString = Me.Connectionstring
                        _nativeinternalConnection.CommandTimeout = 30
                        _nativeinternalConnection.Mode = ConnectModeEnum.adModeReadWrite
                        _nativeinternalConnection.Open()
                        ' check if state is open
                        If _nativeinternalConnection.State = ObjectStateEnum.adStateOpen Then
                            Return _nativeinternalConnection
                        Else
                            Call CoreMessageHandler(showmsgbox:=False, message:="internal connection couldnot be established", _
                                                                       subname:="clsADODBConnection.NativeInternalConnection")
                            Return Nothing
                        End If
                    Catch ex As Exception
                        Call CoreMessageHandler(showmsgbox:=False, message:="internal connection couldnot be established", _
                                                subname:="clsADODBConnection.NativeInternalConnection", exception:=ex)
                        Return Nothing
                    End Try
                Else
                    Return Me._nativeinternalConnection
                End If
            End Get
        End Property
        Friend Overrides ReadOnly Property NativeConnection As Object Implements iormConnection.NativeConnection
            Get
                If _nativeConnection Is Nothing OrElse _nativeConnection.State <> ObjectStateEnum.adStateOpen Then
                    Return Nothing
                Else
                    Return Me._nativeConnection
                End If
            End Get
        End Property
        Public Function RaiseOnConnected()
            RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
        End Function
        Public Function RaiseOnDisConnected()
            RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
        End Function

        '*****
        '***** reset : reset all the private members for a connection
        Friend Sub resetFromConnection()
            Call MyBase.ResetFromConnection()
            If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ObjectStateEnum.adStateClosed Then
                _nativeConnection.Close()
            End If
            'If Not _nativeinternalConnection Is Nothing AndAlso _nativeinternalConnection.State <> ObjectStateEnum.adStateClosed Then
            '_nativeinternalConnection.Close()
            'End If

            _nativeConnection = Nothing

            '_nativeinternalConnection = Nothing
            _ADOError = Nothing
            _ADOXcatalog = Nothing
            '_UILogin = Nothing
        End Sub
        '*****
        '***** disconnect : Disconnects from the Database and cleans up the Enviorment
        Public Overrides Function Disconnect() As Boolean Implements iormConnection.Disconnect

            '*** if connected anyhow
            If isConnected Then

                '***
                If _nativeConnection.State = ObjectStateEnum.adStateOpen Then
                    '** Event
                    RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
                    '** close
                    _nativeConnection.Close()

                ElseIf _nativeConnection.State = ObjectStateEnum.adStateClosed Then
                    ' already closed

                Else
                    '** Event
                    RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
                    '** close
                    _nativeConnection.Close()

                End If
            End If

            '*** reset
            Call resetFromConnection()
            Return True
        End Function

        '********
        '******** Connect : Connects to the Database and initialize Enviorement
        '********
        '********

        Public Overrides Function Connect(Optional ByVal FORCE As Boolean = False, _
                                        Optional ByVal AccessRequest As otAccessRight = otAccessRight.[readonly], _
                                        Optional ByVal OTDBUsername As String = "", _
                                        Optional ByVal OTDBPassword As String = "", _
                                        Optional ByVal exclusive As Boolean = False, _
                                        Optional ByVal notInitialize As Boolean = False, _
                                        Optional ByVal doLogin As Boolean = True) As Boolean Implements iormConnection.Connect

            ' return if connection is there
            If Not _nativeConnection Is Nothing And Not FORCE Then
                ' stay in the connection if we donot need another state -> Validate the Request
                ' if there is a connection and we have no need for higher access -> return
                If _nativeConnection.State = ObjectStateEnum.adStateOpen And ValidateAccessRequest(accessrequest:=AccessRequest) Then
                    ' initialize the parameter values of the OTDB
                    Call Initialize(force:=False)
                    Return True

                ElseIf _nativeConnection.State <> ObjectStateEnum.adStateClosed Then
                    _nativeConnection.Close()
                Else
                    'Set otdb_connection = Nothing
                    ' reset
                    System.Diagnostics.Debug.WriteLine("reseting")
                End If
            End If

            '**** retrieve ConfigParameters
            If Not Me.SetConnectionConfigParameters() Then
                Call CoreMessageHandler(showmsgbox:=True, message:="Configuration Parameters couldnot be retrieved from a data source", _
                                      subname:="clsADODBConnection.Connect")
                Return False
            End If

            '*** verify the User
            If Not Me.VerifyUserAccess(accessRequest:=AccessRequest, username:=OTDBUsername, password:=OTDBPassword, loginOnDisConnected:=doLogin, loginOnFailed:=True) Then
                Return False
            Else
                ' set the OTDBUser later on fully connection
            End If

            '*** open the connection
            Me.ADODBConnection = New ADODB.Connection

            Try
                ' set dbpassword
                _nativeConnection.ConnectionString = Me.Connectionstring
                _nativeConnection.CommandTimeout = 30
                If Me.Access = otAccessRight.[readonly] Then
                    _nativeConnection.Mode = ConnectModeEnum.adModeRead
                Else
                    _nativeConnection.Mode = ConnectModeEnum.adModeReadWrite
                End If

                ' open again
                _nativeConnection.Open()
                ' check if state is open
                If _nativeConnection.State = ObjectStateEnum.adStateOpen Then
                    ' set the Catalog
                    _ADOXcatalog = New ADOX.Catalog
                    _ADOXcatalog.ActiveConnection = _nativeinternalConnection

                    ' set the Access Request
                    _AccessLevel = AccessRequest
                    _OTDBUser = New clsOTDBDefUser
                    _OTDBUser.LoadBy(username:=OTDBUsername)


                    ' raise Connected Event
                    RaiseEvent OnConnection(Me, New ormConnectionEventArgs(Me))
                    ' return true
                    Return True
                End If


            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=True, subname:="clsADODBConnection.Connect", exception:=ex, _
                                      arg1:=_Connectionstring, noOtdbAvailable:=True, break:=False)
                If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ObjectStateEnum.adStateClosed Then
                    _nativeConnection.Close()
                End If
                '*** reset
                Call resetFromConnection()
                Return False
            End Try


        End Function

        '*****
        '***** finalize 
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            '*** close
            If Not _nativeConnection Is Nothing AndAlso _nativeConnection.State <> ObjectStateEnum.adStateClosed Then
                _nativeConnection.Close()
            End If
            '*** close
            If Not _nativeinternalConnection Is Nothing AndAlso _nativeinternalConnection.State <> ObjectStateEnum.adStateClosed Then
                _nativeinternalConnection.Close()
            End If
        End Sub
    End Class


    '************************************************************************************
    '***** CLASS clsADOTableStore describes the per Table reference and Helper Class
    '*****                    ORM Mapping Class and Table Access Workhorse
    '*****

    Public Class clsADOTableStore
        Inherits ormTableStore
        Implements iormDataStore

        '** initialize
        Public Sub New(Connection As iormConnection, TableID As String, ByVal forceSchemaReload As Boolean)
            Call MyBase.New(Connection:=Connection, tableID:=TableID, force:=forceSchemaReload)
        End Sub

#Region "Helpers"
        ReadOnly Property primaryKeyIndexName As String
            Get
                Return Me.TableSchema.PrimaryKeyIndexName
            End Get
        End Property
        Public Function getFieldIndex(anIndex As Object) As Integer
            Return Me.TableSchema.GetFieldordinal(anIndex)
        End Function

        '**** return fieldnames as Collection
        '****
        Public Function GetFieldnames() As List(Of String)
            Return Me.TableSchema.fieldnames()
        End Function
        '****
        '**** getColumns (1...) returns the ADOX Column
        Public Function getADOXColumn(ByVal i As UShort) As ADOX.Column
            Return DirectCast(Me.TableSchema, clsADODBTableSchema).GetColumn(i)
        End Function
        Public Function getfieldname(ByVal i As UShort) As String
            Return Me.TableSchema.Getfieldname(i)
        End Function
        Public Function hasfieldname(ByVal Name As String) As Boolean
            Return Me.TableSchema.Hasfieldname(Name)
        End Function

        Public Function getPrimaryKeyfieldname(ByVal i As UShort) As String
            Return Me.TableSchema.GetPrimaryKeyfieldname(i)
        End Function

        Public Function getPrimaryKeyFieldIndex(ByVal i As UShort) As Integer
            Return Me.TableSchema.GetordinalOfPrimaryKeyField(i)

        End Function
        '******* return the noPrimaryKeys
        '*******
        Public Function noPrimaryKeys() As Integer
            Return Me.TableSchema.NoPrimaryKeyFields()
        End Function

        '******* return the no. fields
        '*******
        Public Function NoFields() As Integer
            Return Me.TableSchema.NoFields()
        End Function

#End Region
        ''' <summary>
        ''' is Linq Available
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property isLinqAvailable As Boolean Implements iormDataStore.IsLinqAvailable
            Get
                Return False
            End Get
        End Property
        ''' <summary>
        ''' Convert2s the column data.
        ''' </summary>
        ''' <param name="value">The value.</param>
        ''' <param name="targetType">Type of the target.</param>
        ''' <param name="maxsize">The maxsize.</param>
        ''' <param name="abostrophNecessary">The abostroph necessary.</param>
        ''' <param name="fieldname">The fieldname.</param>
        ''' <returns></returns>
        Public Overrides Function Convert2ColumnData(value As Object, targetType As Long, Optional maxsize As Long = 0, Optional ByRef abostrophNecessary As Boolean = False, Optional fieldname As String = "") As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function


        '*********
        '********* cvt2ColumnData returns a object in the Datatype of the column
        '*********

        Public Overrides Function Convert2ColumnData(ByVal anIndex As Object, _
                                           ByVal aVAlue As Object, _
                                           Optional ByRef abostrophNecessary As Boolean = False) As Object Implements iormDataStore.Convert2ColumnData
            Dim DBColumn As New ADOX.Column
            Dim DBTable As New ADOX.Table
            Dim catalog As New ADOX.Catalog

            Dim result As Object
            Dim index As Integer

            result = Nothing

            Try
                '* Get the ADOX Table Definition
                DBTable = DirectCast(Me.TableSchema, clsADODBTableSchema).ADoxTable
                index = Me.getFieldIndex(anIndex)
                If index < 0 Then
                    Call CoreMessageHandler(subname:="clsADOTableStoreStore.cvt2ColumnData", _
                                          message:="iOTDBTableStore " & Me.TableID & " anIndex for " & anIndex & " not found", _
                                          tablename:=Me.TableID, arg1:=anIndex)
                    System.Diagnostics.Debug.WriteLine("iOTDBTableStore " & Me.TableID & " anIndex for " & anIndex & " not found")

                    Convert2ColumnData = DBNull.Value
                    Exit Function
                Else
                    DBColumn = Me.getADOXColumn(index)
                End If
                abostrophNecessary = False

                '*
                '*
                If IsError(aVAlue) Then
                    Call CoreMessageHandler(subname:="clsADOTableStore.cvt2ColumnData", _
                                          message:="Error in Formular of field value " & aVAlue & " while updating OTDB", _
                                          arg1:=aVAlue)
                    System.Diagnostics.Debug.WriteLine("Error in Formular of field value " & aVAlue & " while updating OTDB")
                    aVAlue = ""
                End If

                If DBColumn.Type = ADOX.DataTypeEnum.adInteger Or DBColumn.Type = ADOX.DataTypeEnum.adSmallInt Then
                    If String.IsNullOrWhiteSpace(aVAlue) Or IsError(aVAlue) Or DBNull.Value.Equals(aVAlue) Or aVAlue Is Nothing Then
                        result = 0
                    ElseIf IsNumeric(aVAlue) Then
                        result = CInt(aVAlue)
                    Else
                        Call CoreMessageHandler(subname:="clsADOTableStore.cvt2ColumnData", entryname:=Me.getfieldname(anIndex), _
                                              message:="OTDB data " & aVAlue & " is not convertible to Integer", _
                                              arg1:=aVAlue)
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Integer")
                        result = DBNull.Value
                    End If

                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adChar Or DBColumn.Type = ADOX.DataTypeEnum.adVarChar Or _
                DBColumn.Type = ADOX.DataTypeEnum.adVarWChar Or DBColumn.Type = ADOX.DataTypeEnum.adLongVarChar Or DBColumn.Type = ADOX.DataTypeEnum.adLongVarWChar Then
                    abostrophNecessary = True

                    If String.IsNullOrWhiteSpace(aVAlue) Or aVAlue Is Nothing Or IsError(aVAlue) Or DBNull.Value.Equals(aVAlue) Then
                        result = ""
                    Else
                        If DBColumn.DefinedSize < Len(CStr(aVAlue)) And DBColumn.DefinedSize <> 0 Then
                            result = Mid(CStr(aVAlue), 1, DBColumn.DefinedSize - 1)
                        Else
                            result = CStr(aVAlue)
                        End If
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adDate Then
                    If String.IsNullOrWhiteSpace(aVAlue) Or aVAlue Is Nothing Or IsError(aVAlue) Or DBNull.Value.Equals(aVAlue) Then
                        result = ConstNullDate
                    ElseIf IsDate(aVAlue) Then
                        result = CDate(aVAlue)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Date")
                        Call CoreMessageHandler(subname:="clsADOTableStore.cvt2ColumnData", entryname:=Me.getfieldname(anIndex), _
                                              message:="OTDB data " & aVAlue & " is not convertible to Date", _
                                              arg1:=aVAlue)
                        result = ConstNullDate
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adDouble Or DBColumn.Type = ADOX.DataTypeEnum.adDecimal Then
                    If String.IsNullOrWhiteSpace(aVAlue) Or aVAlue Is Nothing Or IsError(aVAlue) Or DBNull.Value.Equals(aVAlue) Then
                        result = 0
                    ElseIf IsNumeric(aVAlue) Then
                        result = CDbl(aVAlue)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Double")
                        Call CoreMessageHandler(subname:="clsADOTableStore.cvt2ColumnData", entryname:=Me.getfieldname(anIndex), _
                                              message:="OTDB data " & aVAlue & " is not convertible to Double", _
                                              arg1:=aVAlue)
                        result = DBNull.Value
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adBoolean Then
                    If String.IsNullOrWhiteSpace(aVAlue) Or aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue) Or (aVAlue = False) Then
                        result = False
                    Else
                        result = True
                    End If

                End If

                ' return
                Return result
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOTableStore.cvt2ColumnData", _
                                 tablename:=Me.TableID, entryname:=Me.getfieldname(anIndex), arg1:=anIndex & ": '" & aVAlue & "'")
                Return Nothing

            End Try


        End Function

        '*********
        '********* cvt2ObjData returns a object from the Datatype of the column to XLS nterpretation
        '*********

        Public Function convert2ObjectData(ByVal anIndex As Object, ByVal aVAlue As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object _
        Implements iormDataStore.Convert2ObjectData
            Dim DBColumn As New ADOX.Column
            Dim DBTable As New ADOX.Table
            Dim catalog As New ADOX.Catalog

            Dim result As Object
            Dim index As Integer

            result = Nothing

            Try
                DBTable = DirectCast(Me.TableSchema, clsADODBTableSchema).ADoxTable
                index = Me.getFieldIndex(anIndex)
                If index < 0 Then
                    System.Diagnostics.Debug.WriteLine("clsADOTableStore " & Me.TableID & " anIndex for " & anIndex & " not found")
                    convert2ObjectData = DBNull.Value
                    Exit Function
                Else
                    DBColumn = Me.getADOXColumn(index)
                End If
                abostrophNecessary = False

                '*
                '*
                'If IsError(aValue) Then
                '    System.Diagnostics.Debug.WriteLine "Error in Formular of field value " & aValue & " while updating OTDB"
                '    aValue = ""
                'End If

                If DBColumn.Type = ADOX.DataTypeEnum.adInteger Then
                    If (Not IsNumeric(aVAlue) Or aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue)) OrElse String.IsNullOrWhiteSpace(aVAlue) Then
                        result = 0
                    ElseIf IsNumeric(aVAlue) Then
                        result = CInt(aVAlue)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Integer")
                        result = DBNull.Value
                    End If

                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adChar Or DBColumn.Type = ADOX.DataTypeEnum.adVarChar Or DBColumn.Type = ADOX.DataTypeEnum.adVarWChar Or DBColumn.Type = ADOX.DataTypeEnum.adLongVarChar Or DBColumn.Type = ADOX.DataTypeEnum.adLongVarWChar Then
                    abostrophNecessary = True
                    If (aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue)) OrElse String.IsNullOrWhiteSpace(aVAlue) Then
                        result = ""
                    Else
                        result = CStr(aVAlue)
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adDate Then
                    If (Not IsDate(aVAlue) Or aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue)) OrElse String.IsNullOrWhiteSpace(aVAlue) Then
                        result = ConstNullDate
                    ElseIf IsDate(aVAlue) Then
                        result = CDate(aVAlue)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Date")
                        result = ConstNullDate
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adDouble Or DBColumn.Type = ADOX.DataTypeEnum.adDecimal Then
                    If (Not IsNumeric(aVAlue) Or aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue)) OrElse String.IsNullOrWhiteSpace(aVAlue) Then
                        result = 0
                    ElseIf IsNumeric(aVAlue) Then
                        result = CDbl(aVAlue)
                    Else
                        System.Diagnostics.Debug.WriteLine("OTDB data " & aVAlue & " is not convertible to Double")
                        result = DBNull.Value
                    End If
                ElseIf DBColumn.Type = ADOX.DataTypeEnum.adBoolean Then
                    If (aVAlue Is Nothing Or DBNull.Value.Equals(aVAlue) Or IsError(aVAlue) Or aVAlue = False) OrElse String.IsNullOrWhiteSpace(aVAlue) Then
                        result = False
                    Else
                        result = True
                    End If
                    'If isNothing(aValue) Or aValue = "-" Or IsError(aValue) Then
                    '    result = False
                    'Else
                    '    result = True
                    'End If

                End If

                ' return
                Return result
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOTableStore.cvt2ObjData", tablename:=Me.TableID, arg1:=anIndex & ": '" & aVAlue & "'")
                Return Nothing
            End Try

        End Function

        '****** delete by PrimaryKey
        '******
        Public Overrides Function DelRecordByPrimaryKey(ByRef keysArray() As Object, Optional silent As Boolean = False) As Boolean _
        Implements iormDataStore.DelRecordByPrimaryKey
            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset
            Dim useseek As Boolean
            Dim j, i As Integer
            Dim fieldname As String
            Dim Value As Object
            Dim wherestr As String
            Dim seekArray() As Object
            Dim abostrophNecessary As Boolean
            Dim cvtvalue As Object
            Dim cmdstr As String


            If Not IsArray(keysArray) Then
                Call CoreMessageHandler(subname:="clsADOTableStore.delRecordByPrimaryKey", message:="Empty Key Array")
                WriteLine("uups - no Array as primaryKey")
                Return False
            End If

            ' Connection
            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.delRecordByPrimaryKey", message:="Connection is not available")
                    Return False
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.delRecordByPrimaryKey", exception:=ex)
                Return False
            End Try


            rst = New ADODB.Recordset
            'rst.Open ActiveConnection:=otdbcn
            useseek = True    'rst.Supports(adSeek)
            wherestr = ""
            i = LBound(keysArray)

            '* get PrimaryKeys and their value -> build the criteria
            '*


            For j = 0 To (Me.noPrimaryKeys - 1)
                '
                If j <= UBound(keysArray) And j >= LBound(keysArray) Then
                    ' value of key
                    Value = keysArray(i)

                    ' get Primary Key
                    '* use seek ?!
                    If Me.Connection.Useseek Then
                        ReDim Preserve seekArray(i)
                        ' just build an array
                        seekArray(j) = Me.Convert2ColumnData(Me.getPrimaryKeyFieldIndex(j + 1), Value, abostrophNecessary)
                    Else
                        ' build a sql like whereclause
                        fieldname = Me.getPrimaryKeyfieldname(j + 1)
                        If fieldname <> "" Then
                            cvtvalue = Me.Convert2ColumnData(fieldname, Value, abostrophNecessary)
                            If Not DBNull.Value.Equals(cvtvalue) Then
                                If j = 0 Then
                                    wherestr = "[" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'" Or Value = ""
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                Else
                                    wherestr = wherestr & " and [" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'"
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                End If
                            End If
                        End If    ' fieldname <> ""
                    End If

                    '
                    i = i + 1
                End If
            Next j
            '** check on seekarray
            If seekArray.Length = 0 Then
                CoreMessageHandler(subname:="iOTDBTableStore.delRecordByPrimaryKey", message:="seekarray is empty", tablename:=Me.TableID)
                Return False
            End If
            ' find it
            Try


                If Me.Connection.Useseek Then
                    rst.Index = Me.primaryKeyIndexName
                    'Set the location of the cursor service.
                    rst.CursorLocation = CursorLocationEnum.adUseServer
                    rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic, CommandTypeEnum.adCmdTableDirect)
                    rst.Seek(seekArray, SeekEnum.adSeekLastEQ)

                Else
                    If Me.noPrimaryKeys > 1 Then
                        cmdstr = "SELECT * FROM " & Me.TableID & " WHERE " & wherestr
                        rst.Open(cmdstr, otdbcn, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic)
                    Else
                        cmdstr = wherestr
                        rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                        rst.Find(cmdstr)
                    End If
                End If
                ' not found
                If rst.EOF Then
                    rst.Close()
                    DelRecordByPrimaryKey = False
                    Exit Function
                End If

                '** delete the record
                '**
                rst.Delete()
                DelRecordByPrimaryKey = True
                ' close
                rst.Close()
                Exit Function

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.delRecordByPrimaryKeys", _
                                      tablename:=Me.TableID, entryname:=fieldname, exception:=ex)
                Return False
            End Try
            ' Handle the error
error_handle:
            Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.delRecordByPrimaryKeys", tablename:=Me.TableID, entryname:=fieldname)
            DelRecordByPrimaryKey = False

        End Function

        '****** getEntity by PrimaryKey
        '******
        Public Overrides Function GetRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As ormRecord _
        Implements iormDataStore.GetRecordByPrimaryKey
            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset
            Dim useseek As Boolean
            Dim j, i As Integer
            Dim fieldname As String
            Dim Value As Object
            Dim wherestr As String
            Dim seekArray() As Object
            Dim abostrophNecessary As Boolean
            Dim cvtvalue As Object
            Dim cmdstr As String

            If Not IsArray(pkArray) Then
                Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByPrimaryKey", message:="Empty Key Array")
                WriteLine("uups - no Array as primaryKey")
                Return Nothing
            End If

            ' Connection
            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByPrimaryKey", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByPrimaryKey", exception:=ex)
                Return Nothing
            End Try


            rst = New ADODB.Recordset
            'rst.Open ActiveConnection:=otdbcn
            useseek = True    'rst.Supports(adSeek)
            i = LBound(pkArray)

            '* get PrimaryKeys and their value -> build the criteria
            '*
            For j = 0 To (Me.noPrimaryKeys - 1)
                '
                If j <= UBound(pkArray) And j >= LBound(pkArray) Then
                    ' value of key
                    Value = pkArray(i)

                    ' get Primary Key
                    '* use seek ?!
                    If Me.Connection.Useseek Then
                        ReDim Preserve seekArray(i)
                        ' just build an array
                        seekArray(j) = Me.Convert2ColumnData(Me.getPrimaryKeyFieldIndex(j + 1), Value, abostrophNecessary)
                    Else
                        ' build a sql like whereclause
                        fieldname = Me.getPrimaryKeyfieldname(j + 1)
                        If fieldname <> "" Then
                            cvtvalue = Me.Convert2ColumnData(fieldname, Value, abostrophNecessary)
                            If Not DBNull.Value.Equals(cvtvalue) Then
                                If j = 0 Then
                                    wherestr = "[" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'" Or Value = ""
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                Else
                                    wherestr = wherestr & " and [" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'"
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                End If
                            End If
                        End If    ' fieldname <> ""
                    End If

                    '
                    i = i + 1
                End If
            Next j

            '** check on seekarray
            If seekArray.Length = 0 Then
                CoreMessageHandler(subname:="iOTDBTableStore.getRecordByPrimaryKey", message:="seekarray is empty", tablename:=Me.TableID)
                Return Nothing
            End If

            Try

                ' find it
                If Me.Connection.Useseek Then
                    rst.Index = Me.primaryKeyIndexName
                    'Set the location of the cursor service.
                    rst.CursorLocation = CursorLocationEnum.adUseServer
                    rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic, CommandTypeEnum.adCmdTableDirect)
                    rst.Seek(seekArray, SeekEnum.adSeekLastEQ)
                Else
                    If Me.noPrimaryKeys > 1 Then
                        cmdstr = "SELECT * FROM " & Me.TableID & " WHERE " & wherestr
                        rst.Open(cmdstr, otdbcn, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic)
                    Else
                        cmdstr = wherestr
                        rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                        rst.Find(cmdstr)
                    End If
                End If
                ' not found
                If rst.EOF Then
                    rst.Close()
                    GetRecordByPrimaryKey = Nothing
                    Exit Function
                End If

                '** Factory a new clsOTDBRecord
                '**
                Dim aNewEnt As ormRecord
                If InfuseRecord(aNewEnt, rst) Then
                    GetRecordByPrimaryKey = aNewEnt
                Else
                    GetRecordByPrimaryKey = Nothing
                End If

                ' close
                rst.Close()
                Exit Function

                '*****
                '***** Error Handling
                '*****
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.getRecordByPrimaryKey", _
                                      tablename:=Me.TableID, arg1:=pkArray, exception:=ex)
                If rst.State = ObjectStateEnum.adStateOpen Then
                    rst.Close()
                End If
                Return Nothing
            End Try


        End Function

        '****** getRecords by Index
        '******
        Public Overrides Function GetRecordsByIndex(indexname As String, ByRef keyArray() As Object, Optional silent As Boolean = False) As List(Of ormRecord) Implements iormDataStore.GetRecordsByIndex

            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset
            Dim useseek As Boolean
            Dim j, i As Integer
            Dim fieldname As String
            Dim Value As Object
            Dim wherestr As String
            Dim seekArray() As Object
            Dim abostrophNecessary As Boolean
            Dim cvtvalue As Object
            Dim cmdstr As String
            Dim aColCollection As ArrayList

            Dim ColNo As Integer
            Dim aNewEnt As ormRecord
            Dim aCollection As New List(Of ormRecord)
            Dim lastpos As Long
            Dim afterLast As Boolean


            If Not IsArray(keyArray) Then
                Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByIndex", message:="No Key Array is not available")
                Return Nothing
            End If

            ' Connection

            Try
                If Me.Connection.IsConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByIndex", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsByIndex", exception:=ex)
                Return Nothing
            End Try


            rst = New ADODB.Recordset
            useseek = False    ' rst.Supports(adSeek)

            i = LBound(keyArray)

            '* get Index and their value -> build the criteria
            '*
            If Me.TableSchema.HasIndex(indexname) Then
                aColCollection = Me.TableSchema.GetIndex(indexname)
            Else
                Call CoreMessageHandler(subname:="clsADOStore.getRecordsByIndex", arg1:=indexname, message:="Index doesnot exists for Table " & Me.TableID)
                Return Nothing
            End If



            For j = 0 To (aColCollection.Count - 1)
                '
                If j <= UBound(keyArray) And j >= LBound(keyArray) Then
                    ' value of key
                    Value = keyArray(i)
                    fieldname = aColCollection.Item(j)
                    ColNo = Me.TableSchema.GetFieldordinal(fieldname)

                    ' get Primary Key
                    '* use seek ?!
                    If Me.Connection.Useseek Then
                        ReDim Preserve seekArray(i)
                        ' just build an array
                        seekArray(j) = Me.Convert2ColumnData(ColNo, Value, abostrophNecessary)
                    Else
                        ' build a sql like whereclause

                        If fieldname <> "" Then
                            cvtvalue = Me.Convert2ColumnData(fieldname, Value, abostrophNecessary)
                            If Not DBNull.Value.Equals(cvtvalue) Then
                                If j = 0 Then
                                    wherestr = "[" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'" Or Value = ""
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                Else
                                    wherestr = wherestr & " and [" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'"
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                End If
                            End If
                        End If    ' fieldname <> ""
                    End If

                    '
                    i = i + 1
                End If
            Next j

            Try
                ' find it
                If Me.Connection.Useseek Then
                    rst.Index = indexname
                    'Set the location of the cursor service.
                    rst.CursorLocation = CursorLocationEnum.adUseServer
                    rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic, _
                             CommandTypeEnum.adCmdTableDirect)
                    rst.Seek(seekArray, SeekEnum.adSeekLastEQ)


                    lastpos = rst.AbsolutePosition    ' get last Postion
                    rst.Seek(seekArray, SeekEnum.adSeekFirstEQ)    ' seek from start
                    Do While Not rst.EOF And Not afterLast
                        aNewEnt = New ormRecord()
                        '** Factory a new clsOTDBRecord
                        If InfuseRecord(aNewEnt, rst) Then
                            aCollection.Add(Item:=aNewEnt)
                        Else
                            'System.Diagnostics.Debug.WriteLine "error"
                        End If
                        ' get next
                        rst.MoveNext()
                        ' moved out the last key criteria ?!
                        If lastpos <= rst.AbsolutePosition Then
                            afterLast = True
                        Else
                            afterLast = False
                        End If
                        'If rst. Then Exit Do
                    Loop
                Else
                    If aColCollection.Count > 1 Then
                        cmdstr = "SELECT * FROM " & Me.TableID & " WHERE " & wherestr
                        rst.Open(cmdstr, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)

                        Do While Not rst.EOF
                            '** Factory a new clsOTDBRecord
                            aNewEnt = New ormRecord
                            If InfuseRecord(aNewEnt, rst) Then
                                aCollection.Add(Item:=aNewEnt)
                            Else
                                'System.Diagnostics.Debug.WriteLine "error"
                            End If
                            ' get next
                            rst.MoveNext()
                        Loop
                    Else
                        cmdstr = wherestr
                        rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                        rst.Find(cmdstr, SearchDirection:=SearchDirectionEnum.adSearchForward)
                        Do While Not rst.EOF
                            '** Factory a new clsOTDBRecord
                            aNewEnt = New ormRecord
                            If InfuseRecord(aNewEnt, rst) Then
                                aCollection.Add(Item:=aNewEnt)
                            Else
                                'System.Diagnostics.Debug.WriteLine "error"
                            End If
                            ' get next
                            rst.Find(cmdstr, SkipRecords:=1)
                            'If rst.NoMatch Then Exit Do
                        Loop
                    End If
                End If


                ' close
                rst.Close()
                GetRecordsByIndex = aCollection
                Exit Function

                '*****
                '***** Error Handling
                '*****
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.getRecordByIndex", _
                                      tablename:=Me.TableID, arg1:=keyArray, entryname:=fieldname, exception:=ex)
                If rst.State = ObjectStateEnum.adStateOpen Then
                    rst.Close()
                End If
                Return Nothing
            End Try

        End Function

        '****** runs a SQLCommand
        '******
        Public Overrides Function RunSQLStatement(ByVal sqlcmdstr As String, _
                                                 Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
                                                Optional silent As Boolean = True) As Boolean _
        Implements iormDataStore.RunSqlStatement

            Return Me.Connection.DatabaseDriver.RunSqlStatement(sqlcmdstr:=sqlcmdstr, parameters:=parameters, silent:=silent)

        End Function
        '****** returns the Collection of Records by SQL
        '******
        Public Function GetRecordsBySQL(ByVal wherestr As String, _
        Optional ByVal fullsqlstr As String = "", _
        Optional ByVal innerjoin As String = "", _
        Optional ByVal orderby As String = "", _
        Optional ByVal silent As Boolean = False, _
        Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord) Implements iormDataStore.GetRecordsBySql

            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset

            Dim j, i As Integer

            Dim cmdstr As String
            Dim aCollection As New List(Of ormRecord)
            Dim aNewEnt As ormRecord
            Dim fieldstr As String

            ' Connection
            Try
                If Me.Connection.IsConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsBySQL", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.getRecordsBySQL", exception:=ex)
                Return Nothing
            End Try



            rst = New ADODB.Recordset

            If fullsqlstr <> "" Then
                cmdstr = fullsqlstr
            Else

                ' Select
                If innerjoin = "" Then
                    cmdstr = "SELECT * FROM " & Me.TableID & " WHERE " & wherestr
                Else
                    i = 0
                    For Each field As String In Me.TableSchema.fieldnames
                        If i = 0 Then
                            fieldstr = Me.TableID & "." & field
                            i += 1
                        Else
                            fieldstr = fieldstr & " , " & Me.TableID & "." & field
                        End If
                    Next


                    cmdstr = "SELECT " & fieldstr & " FROM " & Me.TableID & " " & innerjoin & " WHERE " & wherestr
                End If
                If orderby <> "" Then
                    cmdstr = cmdstr & " ORDER BY " & orderby
                End If
            End If

            Try

                rst.Open(cmdstr, otdbcn, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic)
                Do While Not rst.EOF
                    '** Factory a new clsOTDBRecord
                    If InfuseRecord(aNewEnt, rst) Then
                        aCollection.Add(Item:=aNewEnt)
                    Else
                        'System.Diagnostics.Debug.WriteLine "error"
                    End If
                    ' get next
                    rst.MoveNext()
                Loop

                ' close
                rst.Close()

                ' return
                If aCollection.Count > 0 Then
                    GetRecordsBySQL = aCollection
                Else
                    GetRecordsBySQL = Nothing
                End If

                Exit Function

                '******** error handling
            Catch ex As Exception

                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.getRecordsBySQL", tablename:=Me.TableID, _
                                      arg1:="Where :" & wherestr & " inner join: " & innerjoin & " full: " & fullsqlstr, _
                                      exception:=ex)
                If rst.State = ObjectStateEnum.adStateOpen Then
                    rst.Close()
                End If
                Return Nothing
            End Try



        End Function

        '******** infuseRecord of Table
        '********
        Public Function InfuseRecord(ByRef aNewEnt As ormRecord, ByRef RowObject As Object, Optional ByVal silent As Boolean = False) As Boolean _
        Implements iormDataStore.InfuseRecord
            Dim fieldname As String
            Dim cvtvalue, Value As Object
            Dim j As Integer
            Dim abostrophNecessary As Boolean
            Dim aRecordset As ADODB.Recordset

            aRecordset = DirectCast(RowObject, ADODB.Recordset)

            '** Factory a new clsOTDBRecord
            '**
            aNewEnt = New ormRecord
            aNewEnt.SetTable(Me.TableID)
            aNewEnt.IsLoaded = True

            For j = 1 To Me.NoFields
                ' get fields
                fieldname = Me.getfieldname(j)
                Value = aRecordset.Fields(fieldname).Value
                cvtvalue = Me.convert2ObjectData(j, Value, abostrophNecessary)
                Call aNewEnt.SetValue(j, cvtvalue)

            Next j

            Return True
        End Function

        '******** persistRecord of Table
        '********
        Public Overrides Function PersistRecord(ByRef record As ormRecord, _
                                                Optional ByVal timestamp As Date = ot.ConstNullDate, Optional ByVal silent As Boolean = False) As Boolean _
        Implements iormDataStore.PersistRecord

            Dim fieldname As String
            Dim cvtvalue, Value As Object
            Dim j As Integer
            Dim abostrophNecessary As Boolean
            Dim otdbcn As ADODB.Connection
            Dim rst As ADODB.Recordset
            Dim useseek As Boolean
            Dim i As Integer
            Dim wherestr As String
            Dim seekArray() As Object
            Dim cmdstr As String
            Dim changedRecord As Boolean
            Dim createflag As Boolean


            ' Connection

            Try
                If Me.Connection.isConnected Then
                    otdbcn = DirectCast(Me.Connection.NativeConnection, ADODB.Connection)
                Else
                    Call CoreMessageHandler(subname:="clsADOTableStore.PersistRecord", message:="Connection is not available")
                    Return Nothing
                End If

            Catch ex As Exception
                Call CoreMessageHandler(subname:="clsADOTableStore.PersistRecord", exception:=ex)
                Return Nothing
            End Try

            '*** Try to persist

            Try
                rst = New ADODB.Recordset
                'rst.Open ActiveConnection:=otdbcn
                useseek = True    'rst.Supports(adSeek)
                If timestamp = ConstNullDate Then
                    timestamp = Date.Now

                End If

                i = 0

                '* get PrimaryKeys and their value -> build the criteria
                '*
                For j = 0 To (Me.noPrimaryKeys - 1)
                    '
                    ' value of key
                    Value = record.GetValue(Me.getPrimaryKeyFieldIndex(j + 1))

                    ' get Primary Key
                    '* use seek ?!
                    If Me.Connection.Useseek Then
                        ReDim Preserve seekArray(i)
                        ' just build an array
                        seekArray(j) = Me.Convert2ColumnData(Me.getPrimaryKeyFieldIndex(j + 1), Value, abostrophNecessary)
                    Else
                        ' build a sql like whereclause
                        fieldname = Me.getPrimaryKeyfieldname(j + 1)
                        If fieldname <> "" Then
                            cvtvalue = Me.Convert2ColumnData(fieldname, Value, abostrophNecessary)
                            If Not DBNull.Value.Equals(cvtvalue) Then
                                If j = 0 Then
                                    wherestr = "[" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'" Or Value = ""
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                Else
                                    wherestr = wherestr & " and [" & fieldname & "] = "
                                    If abostrophNecessary Then
                                        wherestr = wherestr & "'" & cvtvalue & "'"
                                    Else
                                        wherestr = wherestr & cvtvalue
                                    End If
                                End If
                            End If
                        End If    ' fieldname <> ""
                    End If

                    '
                    i = i + 1
                Next j
                ' find it
                If Me.Connection.Useseek Then
                    rst.Index = Me.primaryKeyIndexName
                    'Set the location of the cursor service.
                    rst.CursorLocation = CursorLocationEnum.adUseServer
                    rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic, _
                             CommandTypeEnum.adCmdTableDirect)
                    rst.Seek(seekArray, SeekEnum.adSeekLastEQ)

                Else
                    If Me.noPrimaryKeys > 1 Then
                        cmdstr = "SELECT * FROM " & Me.TableID & " WHERE " & wherestr
                        rst.Open(cmdstr, otdbcn, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic)
                    Else
                        cmdstr = wherestr
                        rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                        rst.Find(cmdstr)
                    End If
                End If
                ' not found
                If rst.EOF Then
                    rst.Close()
                    rst.Open(Me.TableID, otdbcn, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic)
                    rst.AddNew()
                    createflag = True
                Else
                    createflag = False
                End If
                'get all fields
                For j = 1 To Me.NoFields
                    ' get fields
                    fieldname = Me.getfieldname(j)
                    If fieldname <> ConstFNUpdatedOn And fieldname <> "" And fieldname <> ConstFNCreatedOn Then
                        Value = record.GetValue(fieldname)
                        cvtvalue = Me.Convert2ColumnData(j, Value, abostrophNecessary)
                        If Not DBNull.Value.Equals(cvtvalue) And Not IsNothing(cvtvalue) Then
                            rst.Fields(fieldname).Value = cvtvalue
                            changedRecord = True
                        End If
                    End If
                Next j
                ' Update the record
                If changedRecord Then
                    If Me.getFieldIndex(ConstFNUpdatedOn) > 0 Then
                        rst.Fields(ConstFNUpdatedOn).Value = timestamp
                    End If
                    If Me.getFieldIndex(ConstFNCreatedOn) > 0 And createflag Then
                        rst(ConstFNCreatedOn).Value = timestamp
                    ElseIf Me.getFieldIndex(ConstFNCreatedOn) > 0 And Not createflag Then
                        If Not DBNull.Value.Equals(record.GetValue(ConstFNCreatedOn)) And Not record.GetValue(ConstFNCreatedOn) Is Nothing Then
                            rst.Fields(ConstFNCreatedOn).Value = record.GetValue(ConstFNCreatedOn)    'keep the value
                        ElseIf Me.getFieldIndex(ConstFNUpdatedOn) > 0 AndAlso _
                        Not DBNull.Value.Equals(record.GetValue(ConstFNUpdatedOn)) _
                        AndAlso Not record.GetValue(ConstFNUpdatedOn) Is Nothing Then
                            rst.Fields(ConstFNCreatedOn).Value = record.GetValue(ConstFNUpdatedOn)    'keep the value
                        Else
                            rst.Fields(ConstFNCreatedOn).Value = timestamp
                        End If
                    End If
                    rst.Update()
                End If


                ' close
                rst.Close()
                PersistRecord = True
                Exit Function
            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=silent, subname:="clsADOTableStore.persistRecord", tablename:=Me.TableID)
                PersistRecord = False
            End Try



        End Function

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class


    '*******************************************************************************************
    '***** CLASS clsADODBTableSchema describes the per Table the schema from the database itself
    '*****

    Public Class clsADODBTableSchema
        Inherits ormTableSchema
        Implements iotTableSchema

        Private _ADODBConnection As clsADODBConnection
        Private _ADODBRecordset As ADODB.Recordset
        Private _ADOXTable As ADOX.Table
        Private _ADOXColumns() As ADOX.Column    ' copy of the description

        Public Sub New(ByRef Connection As clsADODBConnection, ByVal TableID As String)
            MyBase.New()
            ReDim Preserve _ADOXColumns(0)
            _ADODBConnection = Connection
            _ADODBRecordset = New ADODB.Recordset
            Me.TableID = TableID ' sets also the ADOX Table
        End Sub

        ''' <summary>
        ''' returns the default Value
        ''' </summary>
        ''' <param name="index"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function GetDefaultValue(index As Object) As Object
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function
        ''' <summary>
        ''' returns the default Value
        ''' </summary>
        ''' <param name="index"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Overrides Function HasDefaultValue(index As Object) As Boolean
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Assigns the native DB parameter.
        ''' </summary>
        ''' <param name="p1">The p1.</param>
        ''' <returns></returns>
        Public Overrides Function AssignNativeDBParameter(fieldname As String, Optional parametername As String = "") As System.Data.IDbDataParameter Implements iotTableSchema.AssignNativeDBParameter
            ' TODO: Implement this method
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Gets the A dox table.
        ''' </summary>
        ''' <value>The A dox table.</value>
        Public ReadOnly Property ADoxTable() As ADOX.Table
            Get
                Return Me._ADOXTable
            End Get
        End Property

        '**** set the tablename
        '****
        Public Overrides Property TableID As String Implements iotTableSchema.TableID
            Get
                TableID = _TableID
            End Get
            Set(aTableName As String)
                _TableID = aTableName
                Refresh(reloadForce:=False)
            End Set
        End Property

        '****
        '**** getColumns (1...) returns the ADOX Column
        Public Function GetColumn(ByVal i As Integer) As ADOX.Column
            If i > 0 And i <= UBound(_ADOXColumns) + 1 Then
                Return _ADOXColumns(i - 1)
            Else
                Call CoreMessageHandler(message:="index of column out of range", arg1:=i, subname:="iOTDBTableStore.getColum")
                Return Nothing
            End If
        End Function

        '******* load the metaData
        '*******
        Public Overrides Function Refresh(Optional reloadForce As Boolean = False) As Boolean _
        Implements iotTableSchema.Refresh

            Dim catalog As ADOX.Catalog
            Dim col As ADOX.Column
            Dim ind As ADOX.Index
            Dim i As Integer
            Dim aTablename As String = _TableID
            Dim aColumnCollection As ArrayList

            ' load Tablename
            If aTablename = "" Then
                Call CoreMessageHandler(subname:="clsADOTableSchema.fillschemaForTable", _
                                      message:="Nothing Tablename to set to", _
                                      tablename:=aTablename)
                _IsInitialized = False
                Return False
            End If
            '

            Refresh = True

            Try

                catalog = DirectCast(_ADODBConnection.DatabaseDriver.GetCatalog(), ADOX.Catalog)
                If catalog Is Nothing Then
                    Call CoreMessageHandler(subname:="clsADOTableSchema.fillschemaForTable", _
                                          message:="no catalog available", _
                                          tablename:=aTablename, showmsgbox:=True)
                    Return False
                End If

                'catalog.ActiveConnection = otdbcn.ConnectionString
                _ADOXTable = catalog.Tables(aTablename)
                i = _ADOXTable.Columns.Count - 1
                ReDim _ADOXColumns(i)
                ReDim _fieldnames(i)
                ReDim _Primarykeys(0)
                ' set the Dictionaries if reload
                _fieldsDictionary = New Dictionary(Of String, Long)
                _indexDictionary = New Dictionary(Of String, ArrayList)
                aColumnCollection = New ArrayList
                _NoPrimaryKeys = 0
                'ReDim Preserve s_xlsdbdesc(s_table.Columns.count - 1)
                's_NoXlsDbdesc = UBound(s_xlsdbdesc) + 1

                For i = 0 To UBound(_ADOXColumns)  ' starts with zero
                    _ADOXColumns(i) = _ADOXTable.Columns.Item(i)
                    _fieldnames(i) = _ADOXTable.Columns.Item(i).Name

                    ' remove if existing
                    If _fieldsDictionary.ContainsKey(_ADOXTable.Columns.Item(i).Name) Then
                        _fieldsDictionary.Remove(_ADOXTable.Columns.Item(i).Name)
                    End If
                    ' add
                    _fieldsDictionary.Add(key:=_ADOXTable.Columns.Item(i).Name, value:=i + 1) 'store no field 1... not the array index
                    's_xlsdbdesc(i).id = ""
                Next i

                i = 0    ' start with 1

                ' each Index
                For Each ind In _ADOXTable.Indexes
                    ' remind that name
                    If ind.PrimaryKey Then _PrimaryKeyIndexName = ind.Name
                    ' add to index dir
                    If Not _indexDictionary.ContainsKey(ind.Name) Then
                        ' delete
                        'If s_indexDictionary.containskey(ind.Name) Then
                        '    s_indexDictionary.Remove (ind.Name)
                        'End If
                        ' get Collection of Columns
                        aColumnCollection = New ArrayList
                        For Each col In ind.Columns
                            If _fieldsDictionary.ContainsKey(col.Name) Then
                                i = _fieldsDictionary.Item(col.Name)
                                '
                                aColumnCollection.Add(col.Name)    ' item no is i .. starting from 1..
                                'fill old primary Key structure
                                If ind.PrimaryKey Then
                                    _NoPrimaryKeys = _NoPrimaryKeys + 1
                                    ReDim Preserve _Primarykeys(0 To _NoPrimaryKeys - 1)
                                    _Primarykeys(_NoPrimaryKeys - 1) = i - 1 ' set to the array 0...ubound
                                End If
                            Else
                                Call CoreMessageHandler(subname:="clsADOTableSchema.fillschemaForTable", _
                                                      message:="iOTDBTableStore : column " & col.Name & " not in dictionary ?!", _
                                                      tablename:=aTablename, entryname:=col.Name)
                                System.Diagnostics.Debug.WriteLine("iOTDBTableStore : column " & col.Name & " not in dictionary ?!")
                                Refresh = False
                            End If

                        Next col
                    Else
                        Call CoreMessageHandler(subname:="clsADOTableSchema.fillschemaForTable", _
                                              message:="iOTDBTableStore : index " & ind.Name & " twice ?!", _
                                              tablename:=aTablename, arg1:=ind.Name)
                        System.Diagnostics.Debug.WriteLine("iOTDBTableStore : index " & ind.Name & " twice ?!")
                        Refresh = False
                    End If
                    ' save the Collection of fields to the index directory
                    If Not aColumnCollection Is Nothing Then
                        _indexDictionary.Add(key:=ind.Name, value:=aColumnCollection)
                    End If
                Next ind

                'set it
                _IsInitialized = True
                Return True

            Catch ex As Exception
                Call CoreMessageHandler(showmsgbox:=False, subname:="clsADOTableSchema.fillschemaForTable", tablename:=aTablename, arg1:=reloadForce, exception:=ex)
                _IsInitialized = False
                Return False
            End Try


        End Function
    End Class
End Namespace