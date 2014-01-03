REM ***********************************************************************************************************************************************
REM *********** CORE CLASSES DEFINITIONS (Enumerations, Interfaces, Types) for On Track Database Backend Library
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
Imports OnTrack
Imports OnTrack.Database

Namespace OnTrack

    '*************************************************************************************
    '*************************************************************************************

    Public Enum otAccessRight
        Prohibited = 0
        [ReadOnly] = 1
        ReadUpdateData = 2
        AlterSchema = 4
    End Enum

    '********** Structure to Use to Validate UserInformation
    '**********

    Public Structure UserValidation
        Public validEntry As Boolean

        Public Username As String
        Public Password As String
        Public IsProhibited As Boolean
        Public IsAnonymous As Boolean
        Public HasNoRights As Boolean
        Public HasReadRights As Boolean
        Public HasUpdateRights As Boolean
        Public HasAlterSchemaRights As Boolean
    End Structure

    '************************************************************************************
    '**** INTERFACE iOTDBForm defines a Wrapper for a Form UI for the Core to use
    '****           
    '****
        
    Public Interface iOTDBUIAbstractForm

    End Interface
End Namespace

Namespace OnTrack.Database
    '*************************************************************************************
    '* Declare the config

    Public Enum otDBServerType
        Access = 1
        SQLServer = 2
    End Enum

    '*************************************************************************************
    '**** ENUM OTDBDatabaseEnvirormentType -> type of enviorments for database

    Public Enum otDbDriverType
        ADOClassic
        ADONETSQL
        ADONETOLEDB
    End Enum
    '*************************************************************************************
    '**** ENUM OTDBFieldDatatype -> type of datafields
    ''' <summary>
    ''' Data Types for OnTrack Database Fields
    ''' </summary>
    ''' <remarks></remarks>
        
    <TypeConverter(GetType(Long))> Public Enum otFieldDataType
        Numeric = 1
        List = 2
        Text = 3
        Runtime = 4
        Formula = 5
        [Date] = 6
        [Long] = 7
        Timestamp = 8
        Bool = 9
        Memo = 10
        Binary = 11
        Time = 12
    End Enum


    '************************************************************************************
    '**** INTERFACE iOTDBDatabaseEnvirorment defines a Wrapper fora Database with the
    '****           ORM functions for a DataObject
    '****

    Public Interface iormDBDriver

        '** the ID
        Property ID() As String
        '** the type
        ReadOnly Property Type As otDbDriverType
        '** the connection
        ReadOnly Property CurrentConnection As iormConnection
        ''' <summary>
        ''' the Type of the Server
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DatabaseType As otDBServerType
        ''' <summary>
        ''' Persist the Session or ErrorLog
        ''' </summary>
        ''' <param name="log"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function PersistLog(ByRef log As OnTrack.ErrorLog) As Boolean

        ''' <summary>
        ''' verify OnTrack if Data Objects are there and up to date
        ''' </summary>
        ''' <returns>true if OnTrack is ok</returns>
        ''' <remarks></remarks>
        Function VerifyOnTrackDatabase(verifyOnly As Boolean, createOnMissing As Boolean) As Boolean

        '*** Register Connection
        Function RegisterConnection(ByRef connection As iormConnection) As Boolean

        '*** create
        Function GetCatalog(Optional ByVal force As Boolean = False, _
        Optional ByRef nativeConnection As Object = Nothing) As Object

        ''' <summary>
        ''' returns true if the datastore has the table
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasTable(ByVal tableID As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean

        ''' <summary>
        ''' returns or creates a Table in the data store
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="createOrAlter"></param>
        ''' <param name="addToSchemaDir"></param>
        ''' <param name="nativeConnection"></param>
        ''' <param name="tableNativeObject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetTable(ByVal tablename As String, _
        Optional ByVal createOrAlter As Boolean = True, _
        Optional ByVal addToSchemaDir As Boolean = True, _
        Optional ByRef nativeConnection As Object = Nothing, _
        Optional ByRef tableNativeObject As Object = Nothing) As Object

        ''' <summary>
        ''' returns or create an Index in the data store
        ''' </summary>
        ''' <param name="nativeTable"></param>
        ''' <param name="indexname"></param>
        ''' <param name="columnNames"></param>
        ''' <param name="primaryKey"></param>
        ''' <param name="forceCreation"></param>
        ''' <param name="createOrAlter"></param>
        ''' <param name="addToSchemaDir"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetIndex(ByRef nativeTable As Object, _
        ByRef indexname As String, _
        ByRef columnNames As List(Of String), _
        Optional ByVal primaryKey As Boolean = False, _
        Optional ByVal forceCreation As Boolean = False, _
        Optional ByVal createOrAlter As Boolean = True, _
        Optional ByVal addToSchemaDir As Boolean = True) As Object
        ''' <summary>
        ''' returns true if the data store has the columnname in the table
        ''' </summary>
        ''' <param name="tablename"></param>
        ''' <param name="columnname"></param>
        ''' <param name="nativeConnection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasColumn(tableID As String, columnname As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean
        ''' <summary>
        ''' returns or creates a column in the data store
        ''' </summary>
        ''' <param name="nativeTable"></param>
        ''' <param name="aDBDesc"></param>
        ''' <param name="createOrAlter"></param>
        ''' <param name="addToSchemaDir"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetColumn(nativeTable As Object, fielddescription As ormFieldDescription, _
        Optional ByVal createOrAlter As Boolean = True, _
        Optional ByVal addToSchemaDir As Boolean = True) As Object
        '** Parameter
        Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean

        Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean

        Function SetDBParameter(ByVal parametername As String, ByVal value As Object, Optional ByRef nativeConnection As Object = Nothing, _
        Optional ByVal updateOnly As Boolean = False, Optional ByVal silent As Boolean = False) As Boolean
        Function GetDBParameter(parametername As String, Optional ByRef nativeConnection As Object = Nothing, Optional silent As Boolean = False) As Object

        '*** get user definition
        Function GetUserValidation(ByVal username As String, Optional ByVal selectAnonymous As Boolean = False, Optional ByRef nativeConnection As Object = Nothing) As UserValidation
        '*** get the TableStore
        Function GetTableStore(ByVal tableID As String, Optional ByVal force As Boolean = False) As iormDataStore
        Function GetTableSchema(ByVal tableID As String, Optional ByVal force As Boolean = False) As iotDataSchema

        Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, _
        Optional silent As Boolean = True, Optional nativeConnection As Object = Nothing) As Boolean

        ''' <summary>
        ''' run a Select Command and return the List of Records
        ''' </summary>
        ''' <param name="sqlcommand">a clsOTDBSqlSelectCommand</param>
        ''' <param name="parameters">optional list of Parameters for the values</param>
        ''' <param name="nativeConnection">optional native Connection</param>
        ''' <returns>list of clsOTDBRecords</returns>
        ''' <remarks></remarks>
        Function RunSqlSelectCommand(ByRef sqlcommand As ormSqlSelectCommand, _
        Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
        Optional nativeConnection As Object = Nothing) As List(Of ormRecord)

        Function RunSqlSelectCommand(id As String, _
        Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
        Optional nativeConnection As Object = Nothing) As List(Of ormRecord)
        ''' <summary>
        ''' checks if SqlCommand is in Store of the driver
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function HasSqlCommand(id As String) As Boolean
        ''' <summary>
        ''' Store the Command by its ID - replace if existing
        ''' </summary>
        ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean
        ''' <summary>
        ''' Retrieve the Command from Store
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>a iOTDBSqlCommand</returns>
        ''' <remarks></remarks>
        Function RetrieveSqlCommand(id As String) As iormSqlCommand
        ''' <summary>
        ''' Creates a native DB Command
        ''' </summary>
        ''' <param name="p1">Command name</param>
        ''' <param name="aNativeConnection"></param>
        ''' <returns>a idbcommand</returns>
        ''' <remarks></remarks>
        Function CreateNativeDBCommand(p1 As String, aNativeConnection As Data.IDbConnection) As Data.IDbCommand
        ''' <summary>
        ''' creates and assigns a native DB Paramter by otdb datatype
        ''' </summary>
        ''' <param name="parametername"></param>
        ''' <param name="datatype"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AssignNativeDBParameter(parametername As String, _
        datatype As otFieldDataType, _
        Optional maxsize As Long = 0, _
        Optional value As Object = Nothing) As System.Data.IDbDataParameter

        ''' <summary>
        ''' returns the target type for a OTDB FieldType - MAPPING
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetTargetTypeFor(type As otFieldDataType) As Long

        ''' <summary>
        ''' convert a value to column data type
        ''' </summary>
        ''' <param name="value">value</param>
        ''' <param name="targetType">target data type of the native driver</param>
        ''' <param name="maxsize">optional max size of string / text</param>
        ''' <param name="abostrophNecessary">optional true if abostrop in sql necessary</param>
        ''' <param name="fieldname">optional fieldname to use on error handling</param>
        ''' <returns>the converted object</returns>
        ''' <remarks></remarks>
        Function Convert2DBData(ByVal value As Object, _
        targetType As Long, _
        Optional ByVal maxsize As Long = 0, _
        Optional ByRef abostrophNecessary As Boolean = False, _
        Optional ByVal fieldname As String = "") As Object
    End Interface
    '************************************************************************************
    '**** INTERFACE iOTDBTableStore defines a Wrapper Connector to a Database with the
    '****           ORM functions for a DataObject
    '****
    ''' <summary>
    ''' defines an interface for persistency classes which are able to persist clsOTDBRecord 
    ''' through an iotdbconnection object
    ''' </summary>
    ''' <remarks></remarks>

    Public Interface iormDataStore


        ''' <summary>
        ''' sets or gets the connection to the database
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Connection As iormConnection
        ''' <summary>
        ''' sets or gets the schema class for this tablestore
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property TableSchema As iotDataSchema
        ''' <summary>
        ''' set or gets the ID (name) of the table
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property TableID As String
        ''' <summary>
        ''' returns true if the tablestore supports Linq
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsLinqAvailable As Boolean

        ''' <summary>
        ''' returns a new unique key value
        ''' </summary>
        ''' <param name="pkArray">sets or fills this array</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function CreateUniquePkValue(ByRef pkArray() As Object) As Boolean
        ''' <summary>
        ''' Refresh the data of the tablestore
        ''' </summary>
        ''' <param name="force"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Refresh(Optional ByVal force As Boolean = False) As Boolean
        ''' <summary>
        ''' deletes the data record by primary key array
        ''' </summary>
        ''' <param name="aKeyArr"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function DelRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As Boolean
        ''' <summary>
        ''' retrieves a clsOTDBRecord by primary key arrary
        ''' </summary>
        ''' <param name="aKeyArr"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetRecordByPrimaryKey(ByRef pkArray() As Object, Optional silent As Boolean = False) As ormRecord

        '****** returns the Collection of Records by SQL

        Function GetRecordsBySql(ByVal wherestr As String, Optional ByVal fullsqlstr As String = "", Optional ByVal innerjoin As String = "", _
        Optional ByVal orderby As String = "", Optional ByVal silent As Boolean = False, _
        Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord)
        '****** returns the Collection of Records by SQL
        ''' <summary>
        ''' retrieves a collection of records by retrieving or creating a sql command from the data store
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
        Function GetRecordsBySqlCommand(ByVal id As String, _
        Optional ByVal wherestr As String = "", _
        Optional ByVal fullsqlstr As String = "", _
        Optional ByVal innerjoin As String = "", _
        Optional ByVal orderby As String = "", _
        Optional ByVal silent As Boolean = False, _
        Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing) As List(Of ormRecord)
        '****** getRecords by Index
        ''' <summary>
        ''' returns a collection of clsotdbrecord by an named index / view and keys Array in the datastore
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <param name="keyArray"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetRecordsByIndex(indexname As String, ByRef keyArray() As Object, Optional silent As Boolean = False) As List(Of ormRecord)

        '******** infuseRecord of Table
        ''' <summary>
        ''' infuses a clsotdbRecord from the datastore
        ''' </summary>
        ''' <param name="newRecord"></param>
        ''' <param name="rowObject"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function InfuseRecord(ByRef newRecord As ormRecord, ByRef rowObject As Object, Optional ByVal silent As Boolean = False) As Boolean
        '******** persist Record
        ''' <summary>
        ''' persists a clsotdbRecord to the data store
        ''' </summary>
        ''' <param name="record"></param>
        ''' <param name="timestamp"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function PersistRecord(ByRef record As ormRecord, Optional ByVal timestamp As Date = ot.ConstNullDate, Optional ByVal silent As Boolean = False) As Boolean

        '****** runs a string SQL Statement
        ''' <summary>
        ''' runs a plain sql statement
        ''' </summary>
        ''' <param name="sqlcmdstr"></param>
        ''' <param name="parameters"></param>
        ''' <param name="silent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RunSqlStatement(ByVal sqlcmdstr As String, Optional ByRef parameters As List(Of ormSqlCommandParameter) = Nothing, Optional silent As Boolean = True) As Boolean

        '****** runs a SQLCommand
        ''' <summary>
        ''' runs a sql command 
        ''' </summary>
        ''' <param name="command"></param>
        ''' <param name="parametervalues"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RunSqlCommand(ByRef command As ormSqlCommand, Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing) As Boolean

        ''' <summary>
        ''' convert a value to column data type
        ''' </summary>
        ''' <param name="value">value</param>
        ''' <param name="targetType">target data type of the native driver</param>
        ''' <param name="maxsize">optional max size of string / text</param>
        ''' <param name="abostrophNecessary">optional true if abostrop in sql necessary</param>
        ''' <param name="fieldname">optional fieldname to use on error handling</param>
        ''' <returns>the converted object</returns>
        ''' <remarks></remarks>
        Function Convert2ColumnData(ByVal value As Object, _
        targetType As Long, _
        Optional ByVal maxsize As Long = 0, _
        Optional ByRef abostrophNecessary As Boolean = False, _
        Optional ByVal fieldname As String = "") As Object
        ''' <summary>
        ''' convert a value to data type of the column
        ''' </summary>
        ''' <param name="index">column name</param>
        ''' <param name="value">value </param>
        ''' <param name="abostrophNecessary">true if abostrop in sql necessary</param>
        ''' <returns>converted value</returns>
        ''' <remarks></remarks>
        Function Convert2ColumnData(ByVal index As Object, _
        ByVal value As Object, _
        Optional ByRef abostrophNecessary As Boolean = False) As Object

        '********* cvt2ObjData returns a object from the native Datatype 
        ''' <summary>
        ''' convert data from the data store to object
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="value"></param>
        ''' <param name="abostrophNecessary"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Convert2ObjectData(ByVal index As Object, _
        ByVal value As Object, _
        Optional ByRef abostrophNecessary As Boolean = False) As Object

        ''' <summary>
        ''' returns true if the tablestore has the named property
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasProperty(ByVal name As String) As Boolean
        ''' <summary>
        ''' returns the Property by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetProperty(ByVal name As String) As Object
        ''' <summary>
        ''' sets the property by name for the tablestore
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function SetProperty(ByVal name As String, ByVal value As Object) As Boolean

        ''' <summary>
        ''' checks if SqlCommand is in Store of the driver
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function HasSqlCommand(id As String) As Boolean
        ''' <summary>
        ''' Store the Command by its ID - replace if existing
        ''' </summary>
        ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
        ''' <returns>true if successfull</returns>
        ''' <remarks></remarks>
        Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean
        ''' <summary>
        ''' Retrieve the Command from Store
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>a iOTDBSqlCommand</returns>
        ''' <remarks></remarks>
        Function RetrieveSqlCommand(id As String) As iormSqlCommand
        ''' <summary>
        ''' Retrieve the Command from Store or create new command
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>a iOTDBSqlCommand</returns>
        ''' <remarks></remarks>
        Function CreateSqlCommand(id As String) As iormSqlCommand
        ''' <summary>
        ''' Retrieve the Command from Store or create a new Select Command
        ''' </summary>
        ''' <param name="id">id of the command</param>
        ''' <returns>a iOTDBSqlCommand</returns>
        ''' <remarks></remarks>
        Function CreateSqlSelectCommand(id As String, Optional addMe As Boolean = True, Optional addAllFields As Boolean = True) As iormSqlCommand
    End Interface

    '****************************************************************************************
    '**** INTERFACE iOTDBTableSchema defines a Interface for the native Schema Description
    '****

    Public Enum OTDBSQLCommandTypes
        [SELECT] = 1
        UPDATE
        INSERT
        DELETE
    End Enum

    Public Interface iormSqlCommand

        Property ID As String
        ReadOnly Property TableIDs As List(Of String)
        ReadOnly Property [Type] As OTDBSQLCommandTypes
        Property CustomerSqlStatement As String
        ReadOnly Property BuildVersion As UShort
        ReadOnly Property SqlText As String
        Property NativeCommand As System.Data.IDbCommand

        ReadOnly Property Parameters As List(Of ormSqlCommandParameter)

        Function AddParameter(parameter As ormSqlCommandParameter) As Boolean
        Function SetParameterValue(ID As String, value As Object) As Boolean
        Function GetParameterValue(ID As String) As Object
        Function Prepare() As Boolean

    End Interface

    '****************************************************************************************
    '**** INTERFACE iOTDBTableSchema defines a Interface for the native Schema Description
    '****
    ''' <summary>
    ''' interface for a native table schema for a table store
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iotDataSchema
        ''' <summary>
        ''' associated table id of the schema
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property TableID As String
        ''' <summary>
        ''' True if Schema is read and initialized
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsInitialized() As Boolean
        ''' <summary>
        ''' all Indices's as list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Indices As List(Of String)
        ''' <summary>
        ''' refresh loads the schema
        ''' </summary>
        ''' <param name="reloadForce"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Refresh(Optional reloadForce As Boolean = False) As Boolean
        ''' <summary>
        ''' gets the name of the primary key
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property PrimaryKeyIndexName As String
        ''' <summary>
        ''' gets the fieldname ordinals in the schema
        ''' </summary>
        ''' <param name="anIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetFieldordinal(index As Object) As Integer

        '**** return fieldnames as Collection
        '****
        ''' <summary>
        ''' all fieldnames in the schema as List
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Fieldnames() As List(Of String)

        '** return fieldname by index 
        '** Nothing if out of range
        ''' <summary>
        ''' return the fieldname by ordinal
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Getfieldname(ByVal i As Integer) As String
        ''' <summary>
        ''' true if the fieldname exists in the primary key
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasprimaryKeyfieldname(ByRef name As String) As Boolean
        '*** check if fieldname by Name exists
        ''' <summary>
        ''' true if the fieldname exists in the schema
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Hasfieldname(ByVal name As String) As Boolean

        ''' <summary>
        ''' returns the ordinal number of the domainID in the primary key array - less zero if not in the primary key
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetDomainIDPKOrdinal() As Integer

        ''' <summary>
        ''' returns the Default Value for a fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetDefaultValue(ByVal index As Object) As Object

        ''' <summary>
        ''' returns the if there is a Default Value for a fieldname
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasDefaultValue(ByVal index As Object) As Boolean

        '**** get the Primary Key fieldname by Index i
        '***  returns "" if there is none
        ''' <summary>
        ''' get the Primary Key fieldname by Index i.returns "" if there is none
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetPrimaryKeyfieldname(i As UShort) As String

        '**** get the Primary Key fieldname no by field index i
        '***  returns -1 if there is none
        ''' <summary>
        '''  get the Primary Key fieldname no by field index i.  returns -1 if there is none
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetordinalOfPrimaryKeyField(i As UShort) As Integer

        '******* return the noPrimaryKeys
        ''' <summary>
        ''' the number of fields in the primary key
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function NoPrimaryKeyFields() As Integer

        ''' <summary>
        ''' the number of fields
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property NoFields() As Integer
        ''' <summary>
        ''' gets an Index by name
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetIndex(indexname As String) As ArrayList
        ''' <summary>
        ''' True if index exists
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasIndex(indexname As String) As Boolean
        ''' <summary>
        ''' Assign a native DB parameters and return
        ''' </summary>
        ''' <param name="fieldname"></param>
        ''' <param name="parametername"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AssignNativeDBParameter(fieldname As String, Optional parametername As String = "") As System.Data.IDbDataParameter

    End Interface

    '************************************************************************************
    '**** INTERFACE iOTDBConnection defines a DatabaseConnection
    '****

    Public Interface iormConnection


        '******** Connect : Connects to the Database and initialize Environment
        Function Connect(Optional ByVal FORCE As Boolean = False, _
        Optional ByVal access As otAccessRight = otAccessRight.[readonly], _
         Optional ByVal domainID As String = "", _
        Optional ByVal OTDBUsername As String = "", _
        Optional ByVal OTDBPassword As String = "", _
        Optional ByVal exclusive As Boolean = False, _
        Optional ByVal notInitialize As Boolean = False, _
        Optional ByVal doLogin As Boolean = True) As Boolean

        '**** ID of the Connection
        ReadOnly Property ID As String

        '**** useSeek Property
        ReadOnly Property Useseek As Boolean

        '*** ErrorLog
        ReadOnly Property [ErrorLog] As ErrorLog

        ''' <summary>
        ''' returns true if connected
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsConnected As Boolean

        ''' <summary>
        ''' returns true if connection is initialized
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsInitialized As Boolean

        ''' <summary>
        ''' gets the Session of the Connection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Session As Session

        ''' <summary>
        ''' Gets or sets the UI login.
        ''' </summary>
        ''' <value>The UI login.</value>
        Property UILogin As UI.clsCoreUILogin

        ''' <summary>
        ''' Gets or sets the access.
        ''' </summary>
        ''' <value>The access.</value>
        Property Access As otAccessRight

        ''' <summary>
        ''' Gets or sets the user.
        ''' </summary>
        ''' <value>The user.</value>
        Property OTDBUser As OnTrack.User

        ''' <summary>
        ''' Gets or sets the dbpassword.
        ''' </summary>
        ''' <value>The dbpassword.</value>
        Property Dbpassword As String

        ''' <summary>
        ''' Gets or sets the dbuser.
        ''' </summary>
        ''' <value>The dbuser.</value>
        Property Dbuser As String

        ''' <summary>
        ''' Gets or sets the name of the database or file.
        ''' </summary>
        ''' <value>The name.</value>
        Property DBName As String

        ''' <summary>
        ''' Gets or sets the path.
        ''' </summary>
        ''' <value>The path.</value>
        Property PathOrAddress As String

        ''' <summary>
        ''' Gets or sets the connectionstring.
        ''' </summary>
        ''' <value>The connectionstring.</value>
        Property Connectionstring As String

        ''' <summary>
        ''' Gets or sets the databasetype.
        ''' </summary>
        ''' <value>OnTrackDatabaseServer</value>
        Property Databasetype As otDBServerType
        ''' <summary>
        ''' Gets or sets the DatabaseEnvirorment.
        ''' </summary>
        ''' <value>iOTDBDatabaseEnvirorment</value>
        Property DatabaseDriver As iormDBDriver
        ''' <summary>
        ''' Gets the NativeConnection.
        ''' </summary>
        ''' <value>Object</value>

        ReadOnly Property NativeConnection As Object

        '***** disconnect : Disconnects from the Database and cleans up the Enviorment
        Function Disconnect() As Boolean

        Function SetConnectionConfigParameters() As Boolean

        Function ValidateAccessRequest(accessRequest As otAccessRight, _
                                       Optional domainID As String = "", _
                                        Optional ByRef [Objectnames] As List(Of String) = Nothing) As Boolean
        Function VerifyUserAccess(accessRequest As otAccessRight, _
        Optional ByRef username As String = "", _
        Optional ByRef password As String = "", _
        Optional ByRef domainID As String = "", _
        Optional ByRef [Objectnames] As List(Of String) = Nothing, _
        Optional forceLogin As Boolean = False, _
        Optional loginOnDemand As Boolean = False) As Boolean

        '*** Events
        Event OnConnection As EventHandler(Of ormConnectionEventArgs)
        Event OnDisconnection As EventHandler(Of ormConnectionEventArgs)

    End Interface


    '************************************************************************************
    '**** INTERFACE iOTDBDataObject
    '****
    ''' <summary>
    ''' interface describes a persistable OTDB Data Object
    ''' </summary>
    ''' <remarks></remarks>

    Public Interface iormPersistable

        Property DbDriver As iormDBDriver
        ''' <summary>
        ''' Tablestore associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TableStore As iormDataStore
        ''' <summary>
        ''' TableID associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Record As ormRecord
        ''' <summary>
        ''' TableID associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TableID As String
        ''' <summary>
        ''' True if data object is loaded from data store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsLoaded As Boolean
        ''' <summary>
        ''' True if data object is created in the data store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsCreated As Boolean
        ''' <summary>
        ''' True if data object is initialized and working
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsInitialized As Boolean
        ''' <summary>
        ''' Initialize the data object
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Initialize() As Boolean
        ''' <summary>
        ''' load and infuse the dataobject by primary key
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function LoadBy(ByRef pkArray() As Object, Optional domainID As String = "", Optional loadDeleted As Boolean = False) As Boolean
        ''' <summary>
        ''' create a persistable dataobject
        ''' </summary>
        ''' <param name="pkArray"></param>
        ''' <param name="checkUnique"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Create(ByRef pkArray() As Object, Optional domainID As String = "", Optional checkUnique As Boolean = False, Optional noInitialize As Boolean = False) As Boolean

        ''' <summary>
        ''' deletes a persistable object in the datastore
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Delete() As Boolean

        ''' <summary>
        ''' Perists the object in the datastore
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <param name="doFeedRecord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Persist(Optional timestamp As Date = ConstNullDate, Optional doFeedRecord As Boolean = True) As Boolean
        'Function CreateSchema(Optional silent As Boolean = True) As Boolean
        ''' <summary>
        ''' returns the version by attribute of the persistance objects
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="dataobject"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetVersion(dataobject As iormPersistable, Optional name As String = "") As Long

    End Interface

    ''' <summary>
    ''' interface infusable if an Object can be infused by a record
    ''' </summary>
    ''' <remarks></remarks>

    Public Interface iormInfusable
        ''' <summary>
        ''' Infuse the object with data from the record
        ''' </summary>
        ''' <param name="record">record </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Infuse(ByRef record As ormRecord) As Boolean


    End Interface
    ''' <summary>
    ''' interface cloneable if an object can be cloned
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>

    Public Interface iotCloneable(Of T As {iormPersistable, iormInfusable, New})
        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <returns>the new cloned object or nothing</returns>
        ''' <remarks></remarks>
        Function Clone(pkarray() As Object) As T
    End Interface
    ''' <summary>
    ''' interface cloneable if an object can be cloned
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>

    Public Interface iormCloneable
        ''' <summary>
        ''' clone the object with the new primary key
        ''' </summary>
        ''' <param name="pkarray">primary key array</param>
        ''' <returns>the new cloned object or nothing</returns>
        ''' <remarks></remarks>
        Function Clone(Of T As {iormPersistable, iormInfusable, Class, New})(newpkarray() As Object) As T
    End Interface

    ''' <summary>
    ''' interface for having an Compound 
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iotHasCompounds
        ''' <summary>
        ''' adds compounds slots of an instance (out of the envelope) to the envelope
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddSlotCompounds(ByRef envelope As Xchange.XEnvelope) As Boolean

    End Interface
End Namespace
    
Namespace OnTrack

    '************************************************************************************
    '***** Interface iOTDBLoggable for Object receiving Messages
    '*****

    Public Interface otLoggable


        '***** ContextIdentifier (identifier) sets the context of the message receiver
        '*****
        Property ContextIdentifier As String

        '***** setTuple (identifier) sets the Tuple of the message receiver
        '*****
        Property TupleIdentifier As String

        '***** setEntitity (identifier) sets the context of the message receiver
        '*****
        Property EntitityIdentifier As String

        '***** raiseMessage informs the Receiver about the Message-Event
        '*****
        Function raiseMessage(ByVal index As Long, ByRef MSGLOG As ObjectLog) As Boolean

        '***** hands over the msglog object to the receiver
        '*****
        Function attachMessageLog(ByRef MSGLOG As ObjectLog) As Boolean

    End Interface
End Namespace
    
Namespace OnTrack.XChange

    '************************************************************************************
    '***** Interface iOTDBXChange
    '*****

    Public Interface iotXChangeable
        ''' <summary>
        ''' runs the XChange 
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function RunXChange(ByRef envelope As XEnvelope) As Boolean

        ''' <summary>
        ''' runs the Precheck
        ''' </summary>
        ''' <param name="envelope"></param>
        ''' <returns>True if successful</returns>
        ''' <remarks></remarks>
        Function RunXPreCheck(ByRef envelope As XEnvelope) As Boolean

    End Interface
End Namespace

Namespace OnTrack

    ''' <summary>
    ''' Message types of the On Track Database Core
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otCoreMessageType
        InternalError = 1
        InternalWarning = 2
        InternalException = 3
        InternalInfo = 7
        ApplicationError = 4
        ApplicationWarning = 5
        ApplicationInfo = 6
        ApplicationException = 8
    End Enum
    ' Enum of CalenderEntryTypes

    Public Enum otCalendarEntryType
        DayEntry = 1
        MonthEntry = 2
        YearEntry = 3
        WeekEntry = 4
        AbsentEntry = 5
        EventEntry = 6
        MilestoneEntry = 7

    End Enum
    ' Enum of MilestoneTypes

    Public Enum otMilestoneType
        [Date] = 1
        Status = 2

    End Enum
    ' Enum of MilestoneTypes

    Public Enum otObjectEntryDefinitiontype
        Field = 1
        Compound = 2
        Table = 3
    End Enum

    ' Enum ofRelativeToInterval

    Public Enum otIntervalRelativeType
        IntervalRight = -1
        IntervalMiddle = 0
        IntervalLeft = 1
        IntervalInvalid = -2

    End Enum
    ' Type of links between objects
    Public Enum otScheduleLinkType
        Deliverable = 1
    End Enum
    'LogMessageTypes

    Public Enum otAppLogMessageType
        [Error] = 1
        Info = 3
        Attention = 2
        Warning = 4
    End Enum
    'Xchg_cmd

    Public Enum otXChangeCommandType
        Update = 1
        Delete = 2
        UpdateCreate = 3
        Duplicate = 4
        Read = 5
    End Enum


End Namespace


