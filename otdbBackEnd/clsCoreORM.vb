
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

Namespace OnTrack
    Namespace Database
        ''' <summary>
        ''' OTDBDataObject Attribute links a class variable to a datastore table and field
        ''' </summary>
        ''' <remarks></remarks>

        <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
        Public Class ormColumnMappingAttribute
            Inherits Attribute

            Private _ID As String = ""
            Private _fieldname As String = ""
            Private _tableID As String = ""

            ''' <summary>
            ''' Gets or sets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Property ID() As String
                Get
                    Return Me._ID
                End Get
                Set(value As String)
                    Me._ID = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the table ID.
            ''' </summary>
            ''' <value>The table ID.</value>
            Public Property TableID() As String
                Get
                    Return Me._tableID
                End Get
                Set(value As String)
                    Me._tableID = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the fieldname.
            ''' </summary>
            ''' <value>The fieldname.</value>
            Public Property Fieldname() As String
                Get
                    Return Me._fieldname
                End Get
                Set(value As String)
                    Me._fieldname = value
                End Set
            End Property

        End Class
        ''' <summary>
        ''' Mapping a instance field member to a fieldname of a schema description
        ''' </summary>
        ''' <remarks></remarks>
        <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)> _
        Public Class ormPropertyMappingAttribute
            Inherits Attribute
            Private _ID As String = ""
            Private _fieldname As String = ""
            Private _tableID As String = ""

            ''' <summary>
            ''' Gets or sets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Property ID() As String
                Get
                    Return Me._ID
                End Get
                Set(value As String)
                    Me._ID = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the table ID.
            ''' </summary>
            ''' <value>The table ID.</value>
            Public Property TableID() As String
                Get
                    Return Me._tableID
                End Get
                Set(value As String)
                    Me._tableID = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the fieldname.
            ''' </summary>
            ''' <value>The fieldname.</value>
            Public Property Fieldname() As String
                Get
                    Return Me._fieldname
                End Get
                Set(value As String)
                    Me._fieldname = value
                End Set
            End Property

        End Class

        ''' <summary>
        ''' Attribute Class for marking an constant field member in a class as Table name such as
        ''' <otSchemaTable(Version:=1)>Const constTableName = "tblName"
        ''' Version will be saved into clsOTDBDEfSchemaTable
        ''' </summary>
        ''' <remarks></remarks>
        <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
        Public Class ormSchemaTableAttribute
            Inherits Attribute
            Private _ID As String = ""
            Private _Version As UShort = 0
            Private _DeleteFieldFlag As Boolean = False
            Private _SpareFieldsFlag As Boolean = False
            Private _AddDomainIDFlag As Boolean = False

            Public Sub New()

            End Sub
            Public Sub New(ID As String)
                _ID = ID
            End Sub
            ''' <summary>
            ''' Gets or sets the add domain ID flag.
            ''' </summary>
            ''' <value>The add domain ID flag.</value>
            Public Property AddDomainID() As Boolean
                Get
                    Return Me._AddDomainIDFlag
                End Get
                Set(value As Boolean)
                    Me._AddDomainIDFlag = Value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the version.
            ''' </summary>
            ''' <value>The version.</value>
            Public Property Version() As UShort
                Get
                    Return Me._Version
                End Get
                Set(value As UShort)
                    Me._Version = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Property ID() As String
                Get
                    Return Me._ID
                End Get
                Set(value As String)
                    Me._ID = value
                End Set
            End Property
            ''' <summary>
            ''' sets or gets the add deletefield flag. This will add a field for deletion the record to the schema.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property AddDeleteFieldBehavior As Boolean
                Get
                    Return Me._DeleteFieldFlag
                End Get
                Set(value As Boolean)
                    _DeleteFieldFlag = value
                End Set
            End Property
            ''' <summary>
            ''' sets or gets the add ParameterField flag. 
            ''' This will add extra fields for additional parameters (reserve and spare) to the data object.
            ''' 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property AddSpareFields As Boolean
                Get
                    Return Me._SpareFieldsFlag
                End Get
                Set(value As Boolean)
                    _SpareFieldsFlag = value
                End Set
            End Property

        End Class
        ''' <summary>
        ''' Attributes for Schema Generation of an Index
        ''' </summary>
        ''' <remarks></remarks>
        <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
        Public Class ormSchemaIndexAttribute
            Inherits Attribute

            Private _Name As String
            Private _ColumnNames() As String = {}
            Private _Version As UShort = 0

            ''' <summary>
            ''' Gets or sets the version.
            ''' </summary>
            ''' <value>The version.</value>
            Public Property Version() As UShort
                Get
                    Return Me._Version
                End Get
                Set
                    Me._Version = Value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the name.
            ''' </summary>
            ''' <value>The name.</value>
            Public Property Name() As String
                Get
                    Return Me._Name
                End Get
                Set(value As String)
                    Me._Name = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnNames() As String()
                Get
                    Return Me._ColumnNames
                End Get
                Set(value As String())
                    Me._ColumnNames = value
                End Set
            End Property
            Public Property n As UShort
                Get
                    Return _ColumnNames.GetUpperBound(0)
                End Get
                Set(value As UShort)
                    ReDim Preserve _ColumnNames(value)
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnName1() As String
                Get
                    Return Me._ColumnNames(0)
                End Get
                Set(value As String)
                    If _ColumnNames.GetUpperBound(0) < 0 Then ReDim Preserve _ColumnNames(0)
                    Me._ColumnNames(0) = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnName2() As String
                Get
                    Return Me._ColumnNames(1)
                End Get
                Set(value As String)
                    If _ColumnNames.GetUpperBound(0) < 1 Then ReDim Preserve _ColumnNames(1)
                    Me._ColumnNames(1) = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnName3() As String
                Get
                    Return Me._ColumnNames(2)
                End Get
                Set(value As String)
                    If _ColumnNames.GetUpperBound(0) < 2 Then ReDim Preserve _ColumnNames(2)
                    Me._ColumnNames(2) = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnName4() As String
                Get
                    Return Me._ColumnNames(3)
                End Get
                Set(value As String)
                    If _ColumnNames.GetUpperBound(0) < 3 Then ReDim Preserve _ColumnNames(3)
                    Me._ColumnNames(3) = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the column names.
            ''' </summary>
            ''' <value>The column names.</value>
            Public Property ColumnName5() As String
                Get
                    Return Me._ColumnNames(4)
                End Get
                Set(value As String)
                    If _ColumnNames.GetUpperBound(0) < 4 Then ReDim Preserve _ColumnNames(4)
                    Me._ColumnNames(4) = value
                End Set
            End Property

        End Class
        ''' <summary>
        ''' Attribute for Const fields to describe the schema
        ''' </summary>
        ''' <remarks></remarks>
        <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
        Public Class ormSchemaColumnAttribute
            Inherits Attribute
            Private _ID As String = ""
            Private _TableID As String = ""
            Private _Typeid As otFieldDataType
            Private _Title As String = ""
            Private _size As Long = 0
            Private _Parameter As String
            Private _primaryKeyordinal As Short = -1
            Private _aliases() As String = {}
            Private _relation() As String = {}
            Private _IsNullable As Boolean = False
            Private _IsArray As Boolean = False
            Private _Description As String = ""
            Private _DefaultValue As String = ""
            Private _Version As UShort = 1
            Private _Posordinal As UShort = 0
            Private _SpareFieldTag As Boolean = False

            ''' <summary>
            ''' Gets or sets the pos ordinal.
            ''' </summary>
            ''' <value>The pos ordinal.</value>
            Public Property Posordinal() As UShort
                Get
                    Return Me._Posordinal
                End Get
                Set(value As UShort)
                    Me._Posordinal = Value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the default value.
            ''' </summary>
            ''' <value>The default value.</value>
            Public Property DefaultValue() As String
                Get
                    Return Me._DefaultValue
                End Get
                Set(value As String)
                    Me._DefaultValue = value
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

            ''' <summary>
            ''' Gets or sets the is array flag. data field will be transformed into array
            ''' </summary>
            ''' <value>The is array.</value>
            Public Property IsArray() As Boolean
                Get
                    Return Me._IsArray
                End Get
                Set(value As Boolean)
                    Me._IsArray = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Property ID() As String
                Get
                    Return Me._ID
                End Get
                Set(value As String)
                    Me._ID = value
                End Set
            End Property
            ''' <summary>
            ''' set or gets if this field is a spare field
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property SpareFieldTag As Boolean
                Get
                    Return _SpareFieldTag
                End Get
                Set(ByVal value As Boolean)
                    _SpareFieldTag = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the title.
            ''' </summary>
            ''' <value>The title.</value>
            Public Property Title() As String
                Get
                    Return Me._Title
                End Get
                Set(value As String)
                    Me._Title = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the table ID.
            ''' </summary>
            ''' <value>The table ID.</value>
            Public Property TableID() As String
                Get
                    Return Me._TableID
                End Get
                Set(value As String)
                    Me._TableID = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the typeid.
            ''' </summary>
            ''' <value>The typeid.</value>
            Public Property Typeid() As otFieldDataType
                Get
                    Return Me._Typeid
                End Get
                Set(value As otFieldDataType)
                    Me._Typeid = value
                End Set
            End Property


            ''' <summary>
            ''' Gets or sets the size.
            ''' </summary>
            ''' <value>The size.</value>
            Public Property Size() As Long
                Get
                    Return Me._size
                End Get
                Set(value As Long)
                    Me._size = value
                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the parameter.
            ''' </summary>
            ''' <value>The parameter.</value>
            Public Property Parameter() As String
                Get
                    Return Me._Parameter
                End Get
                Set(value As String)
                    Me._Parameter = value
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
            ''' Gets or sets the primary key ordinal.
            ''' </summary>
            ''' <value>The primary key ordinal.</value>
            Public Property PrimaryKeyordinal() As Short
                Get
                    Return Me._primaryKeyordinal
                End Get
                Set(value As Short)
                    If value > 0 Then
                        Me._primaryKeyordinal = value
                    Else
                        CoreMessageHandler(message:="position index is less or equal 0", arg1:=value, subname:="ormSchemaColumn.PrimaryKeyordinal", messagetype:=otCoreMessageType.InternalError)
                        Debug.Assert(False)
                    End If

                End Set
            End Property
            ''' <summary>
            ''' Gets or sets the relation.
            ''' </summary>
            ''' <value>The relation.</value>
            Public Property Relation() As String()
                Get
                    Return Me._relation
                End Get
                Set(value As String())
                    Me._relation = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the aliases.
            ''' </summary>
            ''' <value>The aliases.</value>
            Public Property Aliases() As String()
                Get
                    Return Me._aliases
                End Get
                Set(value As String())
                    Me._aliases = value
                End Set
            End Property

            Public Property Version As UShort
                Get
                    Return Me._Version
                End Get
                Set(value As UShort)
                    Me._Version = value
                End Set
            End Property

        End Class
        '************************************************************************************
        '***** CLASS clsOTDBSQLCommand describes an SQL Command to be used for aTableStore
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

            Protected _databaseDriver As iormDBDriver
            Protected _tablestores As New Dictionary(Of String, iormDataStore)
            Protected _buildTextRequired As Boolean = True
            Protected _buildVersion As UShort = 0
            Protected _nativeCommand As System.Data.IDbCommand
            Protected _Prepared As Boolean = False

            Public Sub New(ID As String)
                _ID = ID
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
            Public Property DatabaseDriver() As iormDBDriver
                Get
                    Return Me._databaseDriver
                End Get
                Set(value As iormDBDriver)
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
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, message:=" id not set in parameter for sql command", messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf parameter.ID = "" And parameter.Fieldname <> "" And Not parameter.NotColumn Then
                    parameter.ID = "@" & parameter.Fieldname
                ElseIf parameter.ID <> "" Then
                    parameter.ID = Regex.Replace(parameter.ID, "\s", "") ' no white chars allowed
                End If

                '** TABLENAME
                If parameter.Tablename = "" And Me.TableIDs(0) <> "" And Not parameter.NotColumn Then
                    parameter.Tablename = Me.TableIDs(0)
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, _
                                          message:=" tablename not set in parameter for sql command - first table used", _
                                          messagetype:=otCoreMessageType.InternalWarning, tablename:=Me.TableIDs(0))

                ElseIf parameter.Tablename = "" And Me.TableIDs(0) = "" And Not parameter.NotColumn Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, _
                                          message:=" tablename not set in parameter for sql command - no default table", _
                                         messagetype:=otCoreMessageType.InternalError)

                    Return False
                End If
                '** fieldnames
                If parameter.Fieldname = "" And parameter.ID = "" Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, _
                                          message:=" fieldname not set in parameter for sql command", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                ElseIf parameter.ID <> "" And parameter.Fieldname = "" And Not parameter.NotColumn Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, _
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
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", tablename:=parameter.Tablename, _
                                          message:="table name is blank", arg1:=parameter.ID)
                    Return False
                End If
                If Not parameter.NotColumn And parameter.Tablename <> "" AndAlso Not GetTableStore(parameter.Tablename).TableSchema.IsInitialized Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", tablename:=parameter.Tablename, _
                                           message:="couldnot initialize table schema")
                    Return False
                End If

                If Not parameter.NotColumn AndAlso Not Me._tablestores.ContainsKey(parameter.Tablename) Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, entryname:=parameter.ID, _
                                          message:=" tablename of parameter is not used in sql command", _
                                      messagetype:=otCoreMessageType.InternalError, tablename:=parameter.Tablename)
                    Return False
                ElseIf Not parameter.NotColumn AndAlso Not Me._tablestores.Item(key:=parameter.Tablename).TableSchema.HasFieldname(parameter.Fieldname) Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", arg1:=Me.ID, entryname:=parameter.Fieldname, _
                                         message:=" fieldname of parameter is not used in table schema", _
                                     messagetype:=otCoreMessageType.InternalError, tablename:=parameter.Tablename)
                    Return False

                End If


                ''' datatype
                If parameter.NotColumn And parameter.Datatype = 0 Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.AddParameter", _
                                          arg1:=Me.ID, message:=" datatype not set in parameter for sql command", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                    ''' datatype lookup
                ElseIf Not parameter.NotColumn AndAlso parameter.Datatype = 0 Then

                    ''' look up internally first
                    Dim aDOType As System.Type = GetDataObjectType(parameter.Tablename)
                    If Not aDOType Is Nothing Then
                        ''' look up internal
                        Dim aFieldList As System.Reflection.FieldInfo()
                        Try
                            aFieldList = aDOType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Public Or _
                                                           Reflection.BindingFlags.Static Or Reflection.BindingFlags.FlattenHierarchy)
                            '** look into each Const Type (Fields)
                            For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList
                                If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                                    '** Attribtes
                                    For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                        If anAttribute.GetType().Equals(GetType(ormSchemaColumnAttribute)) Then
                                            If aFieldInfo.GetValue(Nothing) = parameter.Fieldname Then
                                                parameter.Datatype = DirectCast(anAttribute, ormSchemaColumnAttribute).Typeid
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    If parameter.Datatype <> 0 Then Exit For
                                End If
                            Next

                        Catch ex As Exception
                            Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Addparameter", exception:=ex)
                        End Try

                    End If
                    ''' datatype still not resolved
                    If parameter.Datatype = 0 Then
                        Dim aSchemaEntry As ObjectEntryDefinition = CurrentSession.Objects.GetEntry(entryname:=parameter.Fieldname, objectname:=parameter.Tablename)
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
                    Call CoreMessageHandler(message:="Parameter ID not in Command", arg1:=Me.ID, entryname:=ID, subname:="clsOTDBSqlCommand.SetParameterValue", _
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
            ''' Sets the parameter value.
            ''' </summary>
            ''' <param name="name">The name of the parameter.</param>
            ''' <param name="value">The value of the object</param>
            ''' <returns></returns>
            Public Function GetParameterValue(ID As String) As Object Implements iormSqlCommand.GetParameterValue
                If Not _parameters.ContainsKey(key:=ID) Then
                    Call CoreMessageHandler(message:="Parameter ID not in Command", arg1:=Me.ID, entryname:=ID, subname:="clsOTDBSqlCommand.SetParameterValue", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If
                ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
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

                If Me.DatabaseDriver Is Nothing Then
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Prepare", arg1:=Me.ID, message:="database driver missing", _
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
                                               subname:="clsOTDBSqlCommand.Prepare", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                    'DatabaseDriver.StoreSqlCommand(me)
                    aNativeCommand = _databaseDriver.CreateNativeDBCommand(aSqlText, aNativeConnection)
                    Me.NativeCommand = aNativeCommand
                    '** prepare
                    aNativeCommand.CommandText = aSqlText
                    aNativeCommand.Connection = aNativeConnection
                    aNativeCommand.CommandType = Data.CommandType.Text
                    '** add Parameter
                    For Each aParameter In Me.Parameters
                        '** add Column Parameter

                        If Not aParameter.NotColumn And aParameter.Tablename <> "" And aParameter.Fieldname <> "" Then
                            Dim aTablestore As iormDataStore = _databaseDriver.GetTableStore(aParameter.Tablename)
                            If Not aTablestore.TableSchema.IsInitialized Then
                                Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Prepare", tablename:=aParameter.Tablename, _
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
                            Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Prepare", arg1:=aParameter.ID, message:="Tablename missing", _
                                                  entryname:=aParameter.Fieldname, messagetype:=otCoreMessageType.InternalError)
                        End If
                    Next
                    '** prepare the native
                    aNativeCommand.Prepare()
                    Me._Prepared = True
                    '** initial values
                    For Each aParameter In Me.Parameters
                        If aParameter.Fieldname <> "" And aParameter.Tablename <> "" Then
                            Dim aTablestore As iormDataStore = _databaseDriver.GetTableStore(aParameter.Tablename)
                            cvtvalue = aTablestore.Convert2ColumnData(aParameter.Fieldname, aParameter.Value)
                        Else
                            cvtvalue = aParameter.Value
                        End If
                        If aNativeCommand.Parameters.Contains(aParameter.ID) Then
                            aNativeCommand.Parameters(aParameter.ID).value = cvtvalue
                        Else
                            Call CoreMessageHandler(message:="Parameter ID is not in native sql command", entryname:=aParameter.ID, arg1:=Me.ID, _
                                                   messagetype:=otCoreMessageType.InternalError, subname:="clsOTDBSqlCommand.Prepare")

                        End If

                    Next

                    Return True

                Catch ex As OleDb.OleDbException
                    Me._Prepared = False
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Prepare", message:="Exception", arg1:=Me.ID, _
                                           exception:=ex, messagetype:=otCoreMessageType.InternalException)
                    Return False
                Catch ex As Exception
                    Me._Prepared = False
                    Call CoreMessageHandler(subname:="clsOTDBSqlCommand.Prepare", message:="Exception", arg1:=Me.ID, _
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
            Private _fieldname As String = ""
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
                           Optional fieldname As String = "", _
                           Optional tablename As String = "", _
                           Optional value As Object = Nothing, _
                           Optional notColumn As Boolean = False)
                _ID = Regex.Replace(ID, "\s", "") ' no white chars allowed
                _datatype = datatype
                If fieldname <> "" Then _fieldname = fieldname
                If tablename <> "" Then _tablename = tablename
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
                    Return Me._fieldname
                End Get
                Set(value As String)
                    Me._fieldname = value
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
                    Me._tablename = value
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
                        aTablestore = Me._myCommand.DatabaseDriver.GetTableStore(tableID:=value)
                        If Not aTablestore Is Nothing Then
                            If Not _myCommand._tablestores.ContainsKey(key:=value) Then
                                ' add it
                                _myCommand._tablestores.Add(key:=aTablestore.TableID, value:=aTablestore)
                            End If
                            _tablestore = aTablestore ' set it
                        End If
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
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname) Then
                            _fields.Add(key:=tableid & "." & aFieldname, value:=New ResultField(Me, tableid:=tableid, fieldname:=aFieldname))
                        End If
                    Next
                End If

                '** include specific fields
                If Not addFieldnames Is Nothing Then
                    For Each aFieldname As String In addFieldnames
                        If Not _fields.ContainsKey(key:=tableid & "." & aFieldname) Then
                            _fields.Add(key:=tableid & "." & aFieldname, value:=New ResultField(Me, tableid:=tableid, fieldname:=aFieldname))
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
                    If Not aTableList.Contains(aTablename) Then
                        aTableList.Add(aTablename)
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
                    Me._SqlText &= _select
                End If

                '*** build the tables
                first = True
                Me._SqlText &= " FROM "
                For Each aTablename In aTableList

                    '** if innerjoin has the tablename
                    If Not LCase(_innerjoin).Contains(LCase(aTablename)) Then
                        If Not first Then
                            Me._SqlText &= ","
                        End If
                        Me._SqlText &= aTablename
                        first = False
                    End If
                Next

                '*** innerjoin
                If _innerjoin <> "" Then
                    If Not LCase(_innerjoin).Contains("join") Then
                        Me._SqlText &= " inner join "
                    End If
                    _SqlText &= _innerjoin
                End If

                '*** where 
                If _where <> "" Then
                    If Not LCase(_where).Contains("where") Then
                        Me._SqlText &= " WHERE "
                    End If
                    _SqlText &= _where
                End If

                '*** order by 
                If _orderby <> "" Then
                    If Not LCase(_where).Contains("order by") Then
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
            ''' Run the Sql Select Statement and returns a List of clsOTDBRecords
            ''' </summary>
            ''' <param name="parameters">parameters of value</param>
            ''' <param name="nativeConnection">a optional native connection</param>
            ''' <returns>list of clsotdbRecords (might be empty)</returns>
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
            End Function
        End Class

        '************************************************************************************
        '***** neutral CLASS clsOTDBDriver describes the Environment of the Database Implementation
        '***** on which OnTrack runs
        '*****

        Public MustInherit Class ormDBDriver
            Implements iormDBDriver

            Protected _ID As String
            Protected _TableDirectory As New Dictionary(Of String, iormDataStore)    'Table Directory of iOTDBTableStore
            Protected _TableSchemaDirectory As New Dictionary(Of String, iotTableSchema)    'Table Directory of iOTDBTableSchema
            Protected WithEvents _primaryConnection As iormConnection ' primary connection
            Protected WithEvents _session As Session
            Protected _CommandStore As New Dictionary(Of String, iormSqlCommand) ' store of the SqlCommands to handle

            Protected _lockObject As New Object 'Lock object instead of me

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

            Public ReadOnly Property DatabaseType As otDBServerType Implements iormDBDriver.DatabaseType
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
            Public MustOverride ReadOnly Property Type() As otDbDriverType Implements iormDBDriver.Type

            ''' <summary>
            ''' Gets the ID.
            ''' </summary>
            ''' <value>The ID.</value>
            Public Overridable Property ID() As String Implements iormDBDriver.ID
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
            Public Property TableSchemaDirectory() As Dictionary(Of String, iotTableSchema)
                Get
                    Return Me._TableSchemaDirectory
                End Get
                Set(value As Dictionary(Of String, iotTableSchema))
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
            Public Overridable ReadOnly Property CurrentConnection() As iormConnection Implements iormDBDriver.CurrentConnection
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
            Public Function HasSqlCommand(id As String) As Boolean Implements iormDBDriver.HasSqlCommand
                Return _CommandStore.ContainsKey(key:=id)
            End Function

            ''' <summary>
            ''' Store the Command by its ID - replace if existing
            ''' </summary>
            ''' <param name="sqlCommand">a iOTDBSqlCommand</param>
            ''' <remarks></remarks>
            ''' <returns>true if successful</returns>
            Public Function StoreSqlCommand(ByRef sqlCommand As iormSqlCommand) As Boolean Implements iormDBDriver.StoreSqlCommand
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
            Public Function RetrieveSqlCommand(id As String) As iormSqlCommand Implements iormDBDriver.RetrieveSqlCommand
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
            Protected Overridable Function RegisterConnection(ByRef connection As iormConnection) As Boolean Implements iormDBDriver.RegisterConnection
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

            '
            ''' <summary>
            ''' verifyOnTrack
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function VerifyOnTrackDatabase(verifyOnly As Boolean, createOrAlter As Boolean) As Boolean Implements iormDBDriver.VerifyOnTrackDatabase


            ''' <summary>
            ''' create an assigned Native DBParameter to provided name and type
            ''' </summary>
            ''' <param name="parametername"></param>
            ''' <param name="datatype"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function AssignNativeDBParameter(parametername As String, datatype As otFieldDataType, _
                                                                  Optional maxsize As Long = 0, _
                                                                 Optional value As Object = Nothing) As System.Data.IDbDataParameter Implements iormDBDriver.AssignNativeDBParameter

            ''' <summary>
            ''' returns the target type for a OTDB FieldType - MAPPING
            ''' </summary>
            ''' <param name="type"></param>
            ''' <remarks></remarks>
            ''' <returns></returns>
            Public MustOverride Function GetTargetTypeFor(type As otFieldDataType) As Long Implements iormDBDriver.GetTargetTypeFor
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
            Public MustOverride Function Convert2DBData(ByVal value As Object, _
                                                        targetType As Long, _
                                                        Optional ByVal maxsize As Long = 0, _
                                                       Optional ByRef abostrophNecessary As Boolean = False, _
                                                       Optional ByVal fieldname As String = "") As Object Implements iormDBDriver.Convert2DBData

            ''' Gets the catalog.
            ''' </summary>
            ''' <param name="FORCE">The FORCE.</param>
            ''' <param name="NativeConnection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetCatalog(Optional force As Boolean = False, Optional ByRef nativeConnection As Object = Nothing) As Object Implements iormDBDriver.GetCatalog
            ' TODO: Implement this method

            ''' <summary>
            ''' returns True if data store has the table
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasTable(tablename As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.HasTable

            ''' <summary>
            ''' Gets the table.
            ''' </summary>
            ''' <param name="tablename">The tablename.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <param name="NativeConnection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function GetTable(tablename As String, _
                            Optional createOrAlter As Boolean = True, _
                            Optional addToSchemaDir As Boolean = True, _
                            Optional ByRef nativeConnection As Object = Nothing, _
                             Optional ByRef nativeTableObject As Object = Nothing) As Object Implements iormDBDriver.GetTable

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
            Public MustOverride Function GetIndex(ByRef nativeTABLE As Object, _
            ByRef indexname As String, _
            ByRef ColumnNames As List(Of String), _
            Optional ByVal PrimaryKey As Boolean = False, _
            Optional ByVal forceCreation As Boolean = False, _
            Optional ByVal createOrAlter As Boolean = True, _
            Optional ByVal addToSchemaDir As Boolean = True) As Object Implements iormDBDriver.GetIndex

            ''' <summary>
            ''' returns True if the column exists in the table 
            ''' </summary>
            ''' <param name="tablename"></param>
            ''' <param name="columnname"></param>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasColumn(tablename As String, columnname As String, Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.HasColumn
            ''' <summary>
            ''' Gets the column.
            ''' </summary>
            ''' <param name="nativeTABLE">The native TABLE.</param>
            ''' <param name="aDBDesc">A DB desc.</param>
            ''' <param name="createOrAlter">The create on missing.</param>
            ''' <param name="addToSchemaDir">The add to schema dir.</param>
            ''' <returns></returns>
            Public MustOverride Function GetColumn(nativeTABLE As Object, aDBDesc As ormFieldDescription, Optional createOrAlter As Boolean = True, Optional addToSchemaDir As Boolean = True) As Object Implements iormDBDriver.GetColumn

            ''' <summary>
            ''' Create the User Definition Table
            ''' </summary>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBUserDefTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.CreateDBUserDefTable

            ''' <summary>
            ''' create the DB Parameter Table
            ''' </summary>
            ''' <param name="nativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateDBParameterTable(Optional ByRef nativeConnection As Object = Nothing) As Boolean Implements iormDBDriver.CreateDBParameterTable

            ''' <summary>
            ''' Sets the DB parameter.
            ''' </summary>
            ''' <param name="Parametername">The parametername.</param>
            ''' <param name="Value">The value.</param>
            ''' <param name="NativeConnection">The native connection.</param>
            ''' <param name="UpdateOnly">The update only.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function SetDBParameter(parametername As String, Value As Object, Optional ByRef NativeConnection As Object = Nothing, Optional UpdateOnly As Boolean = False, Optional silent As Boolean = False) As Boolean Implements iormDBDriver.SetDBParameter

            ''' <summary>
            ''' Gets the DB parameter.
            ''' </summary>
            ''' <param name="PARAMETERNAME">The PARAMETERNAME.</param>
            ''' <param name="nativeConnection">The native connection.</param>
            ''' <param name="silent">The silent.</param>
            ''' <returns></returns>
            Public MustOverride Function GetDBParameter(parametername As String, Optional ByRef nativeConnection As Object = Nothing, Optional silent As Boolean = False) As Object Implements iormDBDriver.GetDBParameter


            ''' <summary>
            ''' Gets the def user.
            ''' </summary>
            ''' <param name="Username">The username.</param>
            ''' <param name="nativeConnection">The native connection.</param>
            ''' <returns></returns>
            Protected Friend MustOverride Function GetUserValidation(username As String, Optional ByVal selectAnonymous As Boolean = False, Optional ByRef nativeConnection As Object = Nothing) As UserValidation Implements iormDBDriver.GetUserValidation

            ''' <summary>
            ''' create a tablestore 
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableStore(ByVal TableID As String, ByVal forceSchemaReload As Boolean) As iormDataStore
            ''' <summary>
            ''' create a tableschema
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function CreateNativeTableSchema(ByVal tableID As String) As iotTableSchema

            ''' <summary>
            ''' persists the errorlog
            ''' </summary>
            ''' <param name="TableID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Friend MustOverride Function PersistLog(ByRef log As ErrorLog) As Boolean Implements iormDBDriver.PersistLog
            ''' <summary>
            ''' Gets the table store.
            ''' </summary>
            ''' <param name="tableID">The tablename.</param>
            ''' <param name="Force">The force.</param>
            ''' <returns></returns>
            Public Function GetTableStore(ByVal tableID As String, Optional ByVal force As Boolean = False) As iormDataStore Implements iormDBDriver.GetTableStore
                'take existing or make new one
                If _TableDirectory.ContainsKey(tableID) And Not force Then
                    GetTableStore = _TableDirectory.Item(tableID)
                Else
                    Dim aNewStore As iormDataStore

                    ' reload the existing object on force
                    If _TableDirectory.ContainsKey(tableID) Then
                        aNewStore = _TableDirectory.Item(tableID)
                        aNewStore.Refresh(force)
                        Return aNewStore
                    End If
                    ' assign the Table

                    aNewStore = CreateNativeTableStore(tableID, forceSchemaReload:=force)
                    If Not aNewStore Is Nothing Then
                        If Not _TableDirectory.ContainsKey(tableID) Then
                            _TableDirectory.Add(key:=tableID, value:=aNewStore)
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
            Public Function GetTableSchema(ByVal tableID As String, Optional ByVal force As Boolean = False) As iotTableSchema _
            Implements iormDBDriver.GetTableSchema

                'take existing or make new one
                If _TableSchemaDirectory.ContainsKey(tableID) And Not force Then
                    Return _TableSchemaDirectory.Item(tableID)
                Else
                    Dim aNewSchema As iotTableSchema

                    ' delete the existing object
                    If _TableSchemaDirectory.ContainsKey(tableID) Then
                        aNewSchema = _TableSchemaDirectory.Item(tableID)
                        SyncLock aNewSchema
                            If force Or Not aNewSchema.IsInitialized Then aNewSchema.Refresh(force)
                        End SyncLock
                        Return aNewSchema
                    End If
                    ' assign the Table
                    aNewSchema = CreateNativeTableSchema(tableID)

                    If Not aNewSchema Is Nothing Then
                        SyncLock _lockObject
                            _TableSchemaDirectory.Add(key:=tableID, value:=aNewSchema)
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
                                                  Implements iormDBDriver.RunSqlStatement


            ''' <summary>
            ''' Runs the SQL select command.
            ''' </summary>
            ''' <param name="sqlcommand">The sqlcommand.</param>
            ''' <param name="parameters">The parameters.</param>
            ''' <param name="nativeConnection">The native connection.</param>
            ''' <returns></returns>
            Public MustOverride Function RunSqlSelectCommand(ByRef sqlcommand As ormSqlSelectCommand, _
                                                Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                            Implements iormDBDriver.RunSqlSelectCommand

            Public MustOverride Function RunSqlSelectCommand(id As String, _
                                                         Optional ByRef parametervalues As Dictionary(Of String, Object) = Nothing, _
                                                         Optional nativeConnection As Object = Nothing) As List(Of ormRecord) _
                                                       Implements iormDBDriver.RunSqlSelectCommand
            ''' <summary>
            ''' Create a Native IDBCommand (Sql Command)
            ''' </summary>
            ''' <param name="cmd"></param>
            ''' <param name="aNativeConnection"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function CreateNativeDBCommand(cmd As String, aNativeConnection As System.Data.IDbConnection) As System.Data.IDbCommand Implements iormDBDriver.CreateNativeDBCommand


        End Class


        '************************************************************************************
        '***** CLASS clsOTDBRecord describes a Row per Table reference and Helper Class
        '*****
        '*****

        'Implements iOTDBTableEnt
        ''' <summary>
        ''' represents a record data tuple for to be stored and retrieved in a data store
        ''' </summary>
        ''' <remarks></remarks>
        Public Class ormRecord

            Private _FixEntries As Boolean = False
            Private _IsTableSet As Boolean = False
            Private _TableStore As iormDataStore = Nothing
            Private _DbDriver As iormDBDriver = Nothing
            Private _entrynames() As String = {}
            Private _Values() As Object = {}
            Private _OriginalValues() As Object = {}
            Private _isCreated As Boolean = False
            Private _isUnknown As Boolean = True
            Private _isLoaded As Boolean = False
            Private _isChanged As Boolean = False

            '** initialize
            Public Sub New()

            End Sub
            Public Sub New(ByVal tableID As String, Optional dbdriver As iormDBDriver = Nothing, Optional fillDefaultValues As Boolean = False)
                _DbDriver = dbdriver
                Me.SetTable(tableID, forceReload:=False, fillDefaultValues:=fillDefaultValues)
                _FixEntries = True
            End Sub

            Public Sub Finalize()

                _TableStore = Nothing
                _Values = Nothing
                _OriginalValues = Nothing
            End Sub

            ''' <summary>
            ''' Gets the is table set.
            ''' </summary>
            ''' <value>The is table set.</value>
            Public ReadOnly Property IsTableSet() As Boolean
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
                        _isCreated = False
                        _isLoaded = False
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
            Public ReadOnly Property Alive As Boolean
                Get
                    If _FixEntries Then
                        Return _IsTableSet
                    Else
                        Return True
                    End If

                End Get
            End Property

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
            Public ReadOnly Property TableID
                Get
                    If _TableStore IsNot Nothing Then
                        Return _TableStore.TableID
                    Else
                        Return ""
                    End If
                End Get
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
            ''' checkStatus if loaded or created by checking if Record exists in Table. Sets the isChanged / isLoaded Property
            ''' </summary>
            ''' <returns>true if successfully checked</returns>
            ''' <remarks></remarks>
            Public Function CheckStatus() As Boolean
                '** not loaded and not created but alive ?!
                If Not _isLoaded And Not _isCreated And Alive Then

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
                            _isLoaded = True
                        Else
                            _isCreated = True
                        End If
                    Catch ex As Exception
                        Call CoreMessageHandler(exception:=ex, message:="Exception", messagetype:=otCoreMessageType.InternalException, _
                                              subname:="clsOTDBRecord.checkStatus")
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

                If Not Me.Alive Or Not Me.IsTableSet Then
                    Return Nothing
                End If

                If IsNumeric(index) Then
                    i = CInt(index)
                Else
                    If Not _TableStore.TableSchema.HasFieldname(index) Then
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

                If LCase(_TableStore.TableID) <> LCase(ObjectEntryDefinition.ConstTableID) Then
                    Dim anObject As ObjectDefinition = CurrentSession.Objects.GetObject(_TableStore.TableID)
                    If anObject IsNot Nothing Then

                        '** get default value out o fthe object entry store not from the db itself
                        Dim anEntry As ObjectEntryDefinition = anObject.GetEntry(_TableStore.TableSchema.GetFieldname(i))
                        If anEntry IsNot Nothing Then
                            Return anEntry.DefaultValue
                        Else
                            '** Fieldname not in EntryDefinition (additional field)
                            Call CoreMessageHandler(message:="fieldname not in object entry definition store - additional field ?!", messagetype:=otCoreMessageType.InternalWarning, _
                                                     subname:="clsOTDBRecord.GetDefaultValue", entryname:=_TableStore.TableSchema.GetFieldname(i), tablename:=TableID)
                        End If

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
                                     Optional dbdriver As iormDBDriver = Nothing, _
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
                                For Each fieldname In tablestore.TableSchema.fieldnames
                                    If _entrynames.Contains(fieldname) Then
                                        newValues(_TableStore.TableSchema.GetFieldordinal(fieldname)) = _Values(Array.IndexOf(_entrynames, fieldname))
                                        newOrigValues(_TableStore.TableSchema.GetFieldordinal(fieldname)) = _OriginalValues(Array.IndexOf(_entrynames, fieldname))
                                    End If
                                Next
                                '** change over
                                _Values = newValues
                                _OriginalValues = newOrigValues
                            Else
                                '*** redim else and set the default values
                                ReDim Preserve _Values(0 To _TableStore.TableSchema.NoFields - 1)
                                ReDim Preserve _OriginalValues(0 To _TableStore.TableSchema.NoFields - 1)

                                '* set the default values
                                If fillDefaultValues Then
                                    For i = 1 To _TableStore.TableSchema.NoFields
                                        _Values(i - 1) = Me.GetDefaultValue(i)
                                        _OriginalValues(i - 1) = _Values(i - 1)
                                    Next
                                End If
                            End If
                        End If
                        Return _IsTableSet

                    Else
                        Call CoreMessageHandler(message:="Tablestore or tableschema is not initialized", subname:="clsOTDBRecord.setTable", _
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

                If _IsTableSet Then
                    If timestamp = ConstNullDate Then timestamp = Date.Now
                    Persist = _TableStore.PersistRecord(Me, timestamp:=timestamp)
                    '* switch to loaded
                    If Persist Then
                        Me.IsLoaded = True
                        Me.IsCreated = False
                        Me.IsChanged = False
                    End If
                    Exit Function
                End If
                Persist = False
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
                    Call CoreMessageHandler(subname:="clsOTDBRecord.delete", message:="Record not bound to a TableStore", _
                                          messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                Delete = False
            End Function

            Public Function HasIndex(ByRef anIndex As Object)
                Return Me.Keys.Contains(LCase(anIndex))
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
                    Keys = _TableStore.TableSchema.fieldnames
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
                    Call CoreMessageHandler(subname:="clsOTDBRecord.isValueChanged", arg1:=anIndex, message:="record is not bound to table")
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
                                          subname:="clsOTDBRecord.isIndexChangedValue", arg1:=anIndex, entryname:=anIndex, tablename:=_TableStore.TableID, noOtdbAvailable:=True)
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
                    CoreMessageHandler(exception:=ex, subname:="clsOTDBRecord.Set")
                    Return False
                End Try



            End Function


            ''' <summary>
            ''' set the Value of an Entry of the Record
            ''' </summary>
            ''' <param name="anIndex">Index as No 1...n or name</param>
            ''' <param name="anValue">value</param>
            ''' <param name="FORCE"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function SetValue(ByVal index As Object, ByVal value As Object, Optional ByVal force As Boolean = False) As Boolean
                Dim i As Long

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
                            i = _TableStore.TableSchema.GetFieldordinal(index)
                        End If
                        '*** else dynamic extend
                    Else
                        Dim found As Boolean = False

                        If IsNumeric(index) Then
                            If (index - 1) < _Values.GetUpperBound(0) Then
                                i = index
                                found = True
                            End If
                        Else
                            For j = 0 To _entrynames.GetUpperBound(0)
                                If LCase(_entrynames(j)) = LCase(index) Then
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
                                anIndex = LCase(anIndex)
                            End If
                            _entrynames(i) = anIndex
                            i = i + 1
                        End If

                    End If

                    ' set the value
                    If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                        _OriginalValues(i - 1) = _Values(i - 1)
                        If value Is Nothing Then
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
                                              subname:="clsOTDBRecord.setValue", arg1:=value, entryname:=index, tablename:=_TableStore.TableID, noOtdbAvailable:=True)
                        SetValue = False
                        Return SetValue
                    End If

                    Return True


                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsOTDBRecord.setValue", exception:=ex)
                    Return False
                End Try


            End Function

            ''' <summary>
            ''' gets the Value of an Entry of the Record
            ''' </summary>
            ''' <param name="anIndex">Index 0...n or name of the Field</param>
            ''' <returns>the value as object or Null of not found</returns>
            ''' <remarks></remarks>
            Public Function GetValue(index As Object) As Object
                Dim i As Long

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
                            i = _TableStore.TableSchema.GetFieldordinal(index)
                        End If
                    Else
                        If IsNumeric(index) Then
                            i = CLng(index)
                        Else
                            Dim found As Boolean

                            For j = 0 To _entrynames.GetUpperBound(0)
                                If LCase(_entrynames(j)) = LCase(index) Then
                                    i = j + 1
                                    found = True
                                    Exit For
                                End If
                            Next

                            If Not found Then
                                Return DBNull.Value
                            End If
                        End If
                    End If

                    ' set the value
                    If (i - 1) >= LBound(_Values) And (i - 1) <= UBound(_Values) Then
                        If DBNull.Value.Equals(_Values(i - 1)) Then
                            Return DBNull.Value   ' what to do on DbNull.value ?
                        Else
                            Return _Values(i - 1)

                        End If
                    Else
                        Call CoreMessageHandler(message:="Index of " & index & " is out of bound of tablestore or doesnot exist in record '" & _TableStore.TableID & "'", _
                                              subname:="clsOTDBRecord.getValue", entryname:=index, tablename:=_TableStore.TableID)
                        Return DBNull.Value
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(subname:="clsOTDBRecord.getValue", exception:=ex)
                    Return DBNull.Value
                End Try
            End Function

        End Class


        '************************************************************************************
        '***** neutral CLASS clsOTDBConnection describes the Connection description to OnTrack
        '*****
        '*****

        Public MustInherit Class ormConnection
            Implements iormConnection

            Private _ID As String
            Protected _Session As Session
            Protected _Databasetype As otDBServerType
            Protected _Connectionstring As String    'the  Connection String
            Protected _Path As String    'where the database is if access
            Protected _Name As String    'name of the database or file
            Protected _Dbuser As String    'User name to use to access the database
            Protected _Dbpassword As String    'password to use to access the database
            Protected _Sequence As ot.ConfigSequence = ConfigSequence.primary ' configuration sequence of the connection
            Protected _OTDBUser As New User    ' OTDB User
            Protected _AccessLevel As otAccessRight    ' access

            Protected _UILogin As clsCoreUILogin

            Protected _OTDBDatabaseDriver As iormDBDriver
            Protected _useseek As Boolean 'use seek instead of SQL

            Protected WithEvents _ErrorLog As ErrorLog

            Public Event OnConnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnConnection
            Public Event OnDisconnection As EventHandler(Of ormConnectionEventArgs) Implements iormConnection.OnDisconnection

            ''' <summary>
            ''' constructor of Connection
            ''' </summary>
            ''' <param name="id"></param>
            ''' <param name="databasedriver"></param>
            ''' <param name="session"></param>
            ''' <remarks></remarks>
            Public Sub New(id As String, databasedriver As iormDBDriver, ByRef session As Session, sequence As ot.ConfigSequence)
                _OTDBDatabaseDriver = databasedriver
                _OTDBDatabaseDriver.RegisterConnection(Me)
                _Session = session
                _ErrorLog = session.Errorlog
                _ID = id
                _Sequence = sequence
                _Databasetype = Nothing
                _OTDBUser = Nothing
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
            Public ReadOnly Property Sequence As ot.ConfigSequence
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
            Public Property DatabaseDriver() As iormDBDriver Implements iormConnection.DatabaseDriver
                Get
                    Return _OTDBDatabaseDriver
                End Get
                Friend Set(value As iormDBDriver)
                    _OTDBDatabaseDriver = value
                End Set
            End Property

            ''' <summary>
            ''' Gets the error log.
            ''' </summary>
            ''' <value>The error log.</value>
            Public ReadOnly Property ErrorLog() As ErrorLog Implements iormConnection.ErrorLog
                Get
                    If _ErrorLog Is Nothing Then
                        _ErrorLog = New ErrorLog(My.Computer.Name & "-" & My.User.Name & "-" & Date.Now.ToUniversalTime)
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
            Public Property UILogin() As clsCoreUILogin Implements iormConnection.UILogin
                Get
                    If _UILogin Is Nothing Then
                        _UILogin = New clsCoreUILogin
                    End If
                    Return Me._UILogin
                End Get
                Set(value As clsCoreUILogin)
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
            ''' Gets or sets the user.
            ''' </summary>
            ''' <value>The user.</value>
            Public Property OTDBUser() As User Implements iormConnection.OTDBUser
                Get
                    Return Me._OTDBUser
                End Get
                Set(value As User)
                    Me._OTDBUser = value
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
                '_Dbuser = ""
                '_Dbpassword = ""
                _OTDBUser = Nothing
                _AccessLevel = Nothing

                '_UILogin = Nothing
            End Sub
            '*****
            '***** disconnect : Disconnects from the Database and cleans up the Enviorment
            Public Overridable Function Disconnect() As Boolean Implements iormConnection.Disconnect
                If Not Me.isConnected Then
                    Return False
                End If
                ' Raise the event -> not working here ?!
                RaiseEvent OnDisconnection(Me, New ormConnectionEventArgs(Me))
                Return True
            End Function



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
                Me.Databasetype = CLng(GetConfigProperty(name:=ConstCPNDBType, configsetname:=_Session.ConfigSetName, sequence:=_Sequence))

                '* useseek
                Value = GetConfigProperty(name:=ConstCPNDBUseseek, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)
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
                Me.PathOrAddress = GetConfigProperty(name:=ConstCPNDBPath, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)

                ' get the Database Name if we have it
                Me.DBName = GetConfigProperty(ConstCPNDBName, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)

                ' get the Database user if we have it
                Me.Dbuser = GetConfigProperty(ConstCPNDBUser, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)


                ' get the Database password if we have it
                Me.Dbpassword = GetConfigProperty(name:=ConstCPNDBPassword, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)


                ' get the connection string
                connectionstring = GetConfigProperty(name:=ConstCPNDBConnection, configsetname:=_Session.ConfigSetName, sequence:=_Sequence)

                '***
                Call CoreMessageHandler(message:="Config connection parameters :" & Me.ID & vbLf & _
                                            " DatabaseType : " & Me.Databasetype.ToString & vbLf & _
                                            " Useseek : " & _useseek.ToString & vbLf & _
                                            " PathOrAddress :" & Me.PathOrAddress & vbLf & _
                                            " DBUser : " & Me.Dbuser & vbLf & _
                                            " DBPassword : " & Me.Dbpassword & vbLf & _
                                            " connectionsstring :" & connectionstring, _
                                            messagetype:=otCoreMessageType.InternalInfo, subname:="clsOTDBConnection.SetconnectionConfigParameters")
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
                                          messagetype:=otCoreMessageType.InternalInfo, subname:="clsOTDBConnection.SetconnectionConfigParameters")
                            Return True
                        Else
                            Call CoreMessageHandler(showmsgbox:=True, arg1:=_Path & _Name, subname:="clsOTDBConnection.retrieveConfigParameters", _
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
                                          messagetype:=otCoreMessageType.InternalInfo, subname:="clsOTDBConnection.SetconnectionConfigParameters")
                        Return True
                    Else
                        Call CoreMessageHandler(showmsgbox:=True, arg1:=_Connectionstring, subname:="clsOTDBConnection.retrieveConfigParameters", _
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
            Optional ByVal accessRequest As otAccessRight = otAccessRight.[readonly], _
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

                ' if we have no user -> reverification
                If _OTDBUser Is Nothing OrElse Not _OTDBUser.IsLoaded Then
                    Return False
                End If

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
                                                Optional loginOnDisConnected As Boolean = False, _
                                                Optional loginOnFailed As Boolean = False) As Boolean Implements iormConnection.VerifyUserAccess
                Dim userValidation As UserValidation
                userValidation.validEntry = False

                '****
                '**** no connection -> login
                If Not Me.isConnected Then

                    If domainID = "" Then domainID = ConstGlobalDomain
                    '*** OTDBUsername supplied

                    If loginOnDisConnected And accessRequest <> ConstDefaultAccessRight Then
                        If Me.OTDBUser IsNot Nothing AndAlso Me.OTDBUser.IsAnonymous Then
                            Me.UILogin.EnableUsername = True
                            Me.UILogin.Username = ""
                            Me.UILogin.Password = ""
                        End If
                        'LoginWindow
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.PossibleConfigSets = ot.ConfigSetNamesToSelect
                        Me.UILogin.EnableConfigSet = True

                        Me.UILogin.Domain = domainID
                        Me.UILogin.EnableDomain = False

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
                        If userValidation.validEntry Then
                            username = userValidation.Username
                            password = userValidation.Password
                        End If
                    End If

                    ' if user is still nothing -> not verified
                    If Not userValidation.validEntry Then
                        Call CoreMessageHandler(showmsgbox:=True, _
                                              message:=" Access to OnTrack Database is prohibited - User not found", _
                                              arg1:=userValidation.Username, noOtdbAvailable:=True, break:=False)

                        '*** reset
                        Call ResetFromConnection()
                        Return False
                    Else
                        '**** Check Password
                        '****
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, message:="User verified successfully", _
                                                  arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                        Else
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, message:="User not verified successfully", _
                                                  arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)
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
                    ElseIf loginOnFailed And ot.CurrentConnection.OTDBUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainID
                        Me.UILogin.EnableDomain = False
                        Me.UILogin.PossibleDomains = New List(Of String)
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.EnableConfigSet = False
                        Me.UILogin.Accessright = accessRequest
                        Me.UILogin.Messagetext = "<html><strong>Welcome !</strong><br />Please change to a valid user and password for the needed access right.</html>"
                        Me.UILogin.EnableUsername = True
                        Me.UILogin.Username = ""
                        Me.UILogin.Password = ""
                        Me.UILogin.Show()
                        username = LoginWindow.Username
                        password = LoginWindow.Password
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* check password -> relogin on connected -> EventHandler ?!
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, _
                                                    message:="User change verified successfully on domain '" & domainID & "'", _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                            '* set the new access level
                            _AccessLevel = accessRequest
                            Dim anOTDBUser As New User
                            If anOTDBUser.LoadBy(username:=username) Then
                                _OTDBUser = anOTDBUser
                                Me.Session.UserChangedEvent(_OTDBUser)
                            Else
                                CoreMessageHandler(message:="user definition cannot be loaded", messagetype:=otCoreMessageType.InternalError, _
                                                    arg1:=username, noOtdbAvailable:=False, subname:="clsOTDBConnection.verifyUserAccess")
                                Return False

                            End If

                        Else
                            '** fallback
                            username = _OTDBUser.Username
                            password = _OTDBUser.Password
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError, showmsgbox:=True)
                            Return False
                        End If
                        '* the current access level is not for this request
                    ElseIf loginOnFailed And Not ot.CurrentConnection.OTDBUser.IsAnonymous Then
                        '** check if new OTDBUsername is valid
                        'LoginWindow
                        Me.UILogin.Domain = domainID
                        Me.UILogin.EnableDomain = False
                        Me.UILogin.PossibleDomains = New List(Of String)
                        Me.UILogin.enableAccess = True
                        Me.UILogin.PossibleRights = HigherAccessRequest(accessRequest)
                        Me.UILogin.Configset = ot.CurrentConfigSetName
                        Me.UILogin.EnableConfigSet = False
                        Me.UILogin.Accessright = accessRequest
                       
                        Me.UILogin.Messagetext = "<html><strong>Attention !</strong><br />Please confirm by your password to obtain the access right.</html>"
                        Me.UILogin.EnableUsername = False
                        Me.UILogin.Username = ot.CurrentConnection.OTDBUser.Username
                        Me.UILogin.Password = password
                        Me.UILogin.Show()
                        ' return input
                        username = LoginWindow.Username
                        password = LoginWindow.Password
                        userValidation = Me.DatabaseDriver.GetUserValidation(username)
                        '* check password
                        If userValidation.Password = password Then
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, message:="User change verified successfully", _
                               arg1:=username, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationInfo)
                            '* set the new access level
                            _AccessLevel = accessRequest
                        Else
                            '** fallback
                            username = _OTDBUser.Username
                            password = _OTDBUser.Password
                            Call CoreMessageHandler(subname:="clsOTDBConnection.verifyUserAccess", break:=False, message:="User couldnot be verified - fallback to user " & username, _
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
                If userValidation.HasNoRights Then
                    Call CoreMessageHandler(showmsgbox:=True, _
                                          message:=" Access to OnTrack Database is prohibited - User has no rights", _
                                          break:=False, noOtdbAvailable:=True, messagetype:=otCoreMessageType.ApplicationError)

                    '*** reset
                    If Not Me.isConnected Then
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
                    If Not Me.isConnected Then
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
                    If Not Me.isConnected Then
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
                    If Not Me.isConnected Then
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
                Set
                    Me._domain = Value
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


        '***************************************************************************************************
        '**** ormDataObject is a neutral Class as Base Class for the DataObjects
        '****                   implements the Life cycle
        '****
        ''' <summary>
        ''' a persistable base object in a data store
        ''' </summary>
        ''' <remarks></remarks>
        Partial Public MustInherit Class ormDataObject
            Implements System.ComponentModel.INotifyPropertyChanged
            Implements iormPersistable
            Implements iormInfusable
            Implements iormCloneable

            '** record for persistence
            Private _record As New ormRecord
            Protected _TableID As String = ""
            Private _dbdriver As iormDBDriver
            Protected _IsCreated As Boolean = False
            Protected _IsLoaded As Boolean = False
            Protected _IsChanged As Boolean = False

            Protected _IsInitialized As Boolean = False
            Protected _serializeWithHostApplication As Boolean = False
            Protected _IsloadedFromHost As Boolean = False
            Protected _IsSavedToHost As Boolean = False

            '** events
            Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

            'Public Shared Property ConstTableID
            <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=100, _
                title:="Domain", description:="domain of the business Object", ID:="DM1")> Public Const ConstFNDomainID = Domain.ConstFNDomainID

            '** Column names and definition
            <ormSchemaColumnAttribute(typeid:=otFieldDataType.Timestamp, _
                title:="updated on", _
                Description:="last update time stamp in the data store")> _
            Public Const ConstFNUpdatedOn As String = ot.ConstFNUpdatedOn

            <ormSchemaColumnAttribute(typeid:=otFieldDataType.Timestamp, _
                title:="created on", _
                Description:="creation time stamp in the data store")> _
            Public Const ConstFNCreatedOn As String = ot.ConstFNCreatedOn

            '** deleted Field
            <ormSchemaColumnAttribute(typeid:=otFieldDataType.Timestamp, _
                title:="deleted on", _
                Description:="time stamp when the deletion flag was set")> _
            Public Const ConstFNDeletedOn As String = ot.ConstFNDeletedOn

            '** Deleted flag
            <ormSchemaColumnAttribute(typeid:=otFieldDataType.Bool, _
                title:="is deleted", _
                description:="flag if the entry in the data stored is regarded as deleted depends on the deleteflagbehavior")> _
            Public Const ConstFNIsDeleted As String = ot.ConstFNIsDeleted

            '** Spare Parameters
            <ormSchemaColumn(typeid:=otFieldDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, _
            title:="text parameter 1", description:="text parameter 1")> Public Const ConstFNParamText1 = "param_txt1"
            <ormSchemaColumn(typeid:=otFieldDataType.Text, isnullable:=True, size:=255, spareFieldTag:=True, _
            title:="text parameter 2", description:="text parameter 2")> Public Const ConstFNParamText2 = "param_txt2"
            <ormSchemaColumn(typeid:=otFieldDataType.Text, size:=255, isnullable:=True, spareFieldTag:=True, _
            title:="text parameter 3", description:="text parameter 3")> Public Const ConstFNParamText3 = "param_txt3"
            <ormSchemaColumn(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 1", description:="numeric parameter 1")> Public Const ConstFNParamNum1 = "param_num1"
            <ormSchemaColumn(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 2", description:="numeric parameter 2")> Public Const ConstFNParamNum2 = "param_num2"
            <ormSchemaColumn(typeid:=otFieldDataType.Numeric, isnullable:=True, spareFieldTag:=True, _
            title:="numeric parameter 3", description:="numeric parameter 3")> Public Const ConstFNParamNum3 = "param_num3"
            <ormSchemaColumn(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 1", description:="date parameter 1")> Public Const ConstFNParamDate1 = "param_date1"
            <ormSchemaColumn(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 2", description:="date parameter 2")> Public Const ConstFNParamDate2 = "param_date2"
            <ormSchemaColumn(typeid:=otFieldDataType.Date, isnullable:=True, spareFieldTag:=True, _
            title:="date parameter 3", description:="date parameter 3")> Public Const ConstFNParamDate3 = "param_date3"
            <ormSchemaColumn(typeid:=otFieldDataType.Bool, isnullable:=True, defaultvalue:="0", spareFieldTag:=True, _
            title:="flag parameter 1", description:="flag parameter 1")> Public Const ConstFNParamFlag1 = "param_flag1"
            <ormSchemaColumn(typeid:=otFieldDataType.Bool, isnullable:=True, defaultvalue:="0", spareFieldTag:=True, _
            title:="flag parameter 2", description:="flag parameter 2")> Public Const ConstFNParamFlag2 = "param_flag2"
            <ormSchemaColumn(typeid:=otFieldDataType.Bool, isnullable:=True, defaultvalue:="0", spareFieldTag:=True, _
            title:="flag parameter 3", description:="flag parameter 3")> Public Const ConstFNParamFlag3 = "param_flag3"

            '** columnMapping of persistable fields
            <ormColumnMappingAttribute(fieldname:=ConstFNUpdatedOn)> Protected _updatedOn As Date = ot.ConstNullDate
            <ormColumnMappingAttribute(fieldname:=ConstFNCreatedOn)> Protected _createdOn As Date = ConstNullDate
            <ormColumnMappingAttribute(fieldname:=ConstFNDeletedOn)> Protected _deletedOn As Date = ConstNullDate
            <ormColumnMappingAttribute(fieldname:=ConstFNIsDeleted)> Protected _IsDeleted As Boolean = False

            '** Spare Parameters
            <ormColumnMapping(fieldname:=ConstFNParamText1)> Protected _parameter_txt1 As String = ""
            <ormColumnMapping(fieldname:=ConstFNParamText2)> Protected _parameter_txt2 As String = ""
            <ormColumnMapping(fieldname:=ConstFNParamText3)> Protected _parameter_txt3 As String = ""
            <ormColumnMapping(fieldname:=ConstFNParamNum1)> Protected _parameter_num1 As Double
            <ormColumnMapping(fieldname:=ConstFNParamNum2)> Protected _parameter_num2 As Double
            <ormColumnMapping(fieldname:=ConstFNParamNum3)> Protected _parameter_num3 As Double
            <ormColumnMapping(fieldname:=ConstFNParamDate1)> Protected _parameter_date1 As Date = ConstNullDate
            <ormColumnMapping(fieldname:=ConstFNParamDate2)> Protected _parameter_date2 As Date = ConstNullDate
            <ormColumnMapping(fieldname:=ConstFNParamDate3)> Protected _parameter_date3 As Date = ConstNullDate
            <ormColumnMapping(fieldname:=ConstFNParamFlag1)> Protected _parameter_flag1 As Boolean
            <ormColumnMapping(fieldname:=ConstFNParamFlag2)> Protected _parameter_flag2 As Boolean
            <ormColumnMapping(fieldname:=ConstFNParamFlag3)> Protected _parameter_flag3 As Boolean

            <ormColumnMapping(fieldname:=ConstFNDomainID)> Protected _domainID As String = ""
#Region "Properties"
            ''' <summary>
            ''' Gets the table store.
            ''' </summary>
            ''' <value>The table store.</value>
            Public ReadOnly Property TableStore() As iormDataStore Implements iormPersistable.TableStore
                Get
                    If _record.Alive AndAlso Not _record.TableStore Is Nothing Then
                        Return _record.TableStore
                    ElseIf Me._TableID <> "" Then
                        Return GetTableStore(tableid:=_TableID)
                    Else
                        Return Nothing
                    End If
                End Get
            End Property
            ''' <summary>
            ''' returns the object definition associated with this data object
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property ObjectDefinition As ObjectDefinition
                Get
                    If CurrentSession.IsRunning Then
                        Return CurrentSession.Objects.GetObject(objectname:=Me.TableID)
                    Else
                        CoreMessageHandler(message:="not connected to ontrack - connect first", tablename:=Me.TableID, _
                                           subname:="ormDataObject.ObjectDefinition", messagetype:=otCoreMessageType.InternalWarning)
                        Return Nothing
                    End If

                End Get
            End Property
            ''' <summary>
            ''' returns the tableschema associated with this data object
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property TableSchema() As iotTableSchema
                Get
                    If Me.TableStore IsNot Nothing Then
                        Return Me.TableStore.TableSchema
                    Else
                        Return Nothing
                    End If

                End Get
            End Property

            

            ''' <summary>
            ''' Gets or sets the domain ID.
            ''' </summary>
            ''' <value>The domain ID.</value>
            Public Property DomainID() As String
                Get
                    If CurrentSession.IsRunning AndAlso _
                        Me.ObjectDefinition IsNot Nothing AndAlso Me.ObjectDefinition.DomainBehavior Then
                        Return Me._domainID
                    Else
                        Return CurrentSession.CurrentDomainID
                    End If
                End Get
                Set(value As String)
                    Me._domainID = Value
                End Set
            End Property
            ''' <summary>
            ''' sets or gets the DBDriver for the data object to use
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property DBDriver As iormDBDriver Implements iormPersistable.DbDriver

                Set(value As iormDBDriver)
                    If Not _IsInitialized Then
                        _dbdriver = value
                    Else
                        Call CoreMessageHandler(message:="can not set the dbdriver while initialised", subname:="ormDataobject.DBDriver", messagetype:=otCoreMessageType.InternalError)
                    End If
                End Set
                Get
                    Return _dbdriver
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the PS is initialized.
            ''' </summary>
            ''' <value>The PS is initialized.</value>
            Public ReadOnly Property IsInitialized() As Boolean Implements iormPersistable.IsInitialized
                Get
                    Return Me._IsInitialized
                End Get

            End Property

            ''' <summary>
            ''' Gets or sets the isDeleted.
            ''' </summary>
            ''' <value>The isDeleted.</value>
            Public Property IsDeleted() As Boolean
                Get
                    Return Me._IsDeleted
                End Get
                Protected Friend Set(value As Boolean)
                    Me._IsDeleted = value
                    If value = False Then
                        _deletedOn = ConstNullDate
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the PS is changed.
            ''' </summary>
            ''' <value>The PS is changed.</value>
            Public Property IsChanged() As Boolean
                Get
                    Return Me._IsChanged
                End Get
                Protected Friend Set(value As Boolean)
                    Me._IsChanged = value
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the PS is loaded.
            ''' </summary>
            ''' <value>The PS is loaded.</value>
            Public ReadOnly Property IsLoaded() As Boolean Implements iormPersistable.IsLoaded
                Get
                    Return Me._IsLoaded
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the PS is created.
            ''' </summary>
            ''' <value>The PS is created.</value>
            Public ReadOnly Property IsCreated() As Boolean Implements iormPersistable.IsCreated
                Get
                    Return Me._IsCreated
                End Get
            End Property
            ''' <summary>
            ''' unload the Dataobject from the datastore
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Function Unload() As Boolean
                _IsLoaded = False
            End Function
            ''' <summary>
            ''' Gets or sets the OTDB record.
            ''' </summary>
            ''' <value>The OTDB record.</value>
            Public Property Record() As ormRecord Implements iormPersistable.Record
                Get
                    Return Me._record
                End Get
                Set(value As ormRecord)
                    If _record Is Nothing Then
                        Me._record = value
                    Else
                        MergeRecord(value)
                    End If
                End Set
            End Property
            ''' <summary>
            ''' Merge Values of an record in own record
            ''' </summary>
            ''' <param name="record"></param>
            ''' <returns>True if successfull </returns>
            ''' <remarks></remarks>
            Private Function MergeRecord(record As ormRecord) As Boolean
                For Each key In record.Keys
                    If _record.HasIndex(key) Then Me._record.SetValue(key, record.GetValue(key))
                Next
                Return True
            End Function
            Public Property LoadedFromHost() As Boolean
                Get
                    LoadedFromHost = _IsloadedFromHost
                End Get
                Protected Friend Set(value As Boolean)
                    _IsloadedFromHost = value
                End Set
            End Property
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property SavedToHost() As Boolean
                Get
                    SavedToHost = _IsSavedToHost
                End Get
                Protected Friend Set(value As Boolean)
                    _IsSavedToHost = value
                End Set
            End Property
            '** set the serialize with HostApplication
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property SerializeWithHostApplication() As Boolean
                Get
                    SerializeWithHostApplication = _serializeWithHostApplication
                End Get
                Protected Friend Set(value As Boolean)
                    If value Then
                        If isRegisteredAtHostApplication(Me.TableID) Then
                            _serializeWithHostApplication = True
                        Else
                            _serializeWithHostApplication = registerHostApplicationFor(Me.TableID, AllObjectSerialize:=False)
                        End If
                    Else
                        _serializeWithHostApplication = False
                    End If
                End Set
            End Property


            ''' <summary>
            ''' gets the TableID of the persistency table
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property TableID() As String Implements iormPersistable.TableID
                Get
                    TableID = _TableID
                End Get
            End Property
            ''' <summary>
            ''' gets the Creation date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property CreatedOn() As Date
                Get
                    CreatedOn = _createdOn
                End Get
            End Property
            ''' <summary>
            ''' gets the last update date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            ReadOnly Property UpdatedOn() As Date
                Get
                    UpdatedOn = _updatedOn
                End Get
            End Property
            ''' <summary>
            ''' gets the deletion date in the persistence store
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Property DeletedOn() As Date
                Get
                    DeletedOn = _deletedOn
                End Get
                Friend Set(value As Date)
                    DeletedOn = value
                End Set
            End Property

            Public Property parameter_num1() As Double
                Get
                    Return _parameter_num1
                End Get
                Set(value As Double)
                    If _parameter_num1 <> value Then
                        _parameter_num1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_num2() As Double
                Get
                    Return _parameter_num2
                End Get
                Set(value As Double)
                    If _parameter_num2 <> value Then
                        _parameter_num2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_num3() As Double
                Get
                    Return _parameter_num3
                End Get
                Set(value As Double)
                    If _parameter_num3 <> value Then
                        _parameter_num3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date1() As Date
                Get
                    Return _parameter_date1
                End Get
                Set(value As Date)
                    If _parameter_date1 <> value Then
                        _parameter_date1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date2() As Date
                Get
                    Return _parameter_date2
                End Get
                Set(value As Date)
                    If _parameter_date2 <> value Then
                        _parameter_date2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_date3() As Date
                Get
                    Return _parameter_date3
                End Get
                Set(value As Date)
                    _parameter_date3 = value
                    Me.IsChanged = True
                End Set
            End Property
            Public Property parameter_flag1() As Boolean
                Get
                    Return _parameter_flag1
                End Get
                Set(value As Boolean)
                    If _parameter_flag1 <> value Then
                        _parameter_flag1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_flag3() As Boolean
                Get
                    parameter_flag3 = _parameter_flag3
                End Get
                Set(value As Boolean)
                    If _parameter_flag3 <> value Then
                        _parameter_flag3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_flag2() As Boolean
                Get
                    Return _parameter_flag2
                End Get
                Set(value As Boolean)
                    If _parameter_flag2 <> value Then
                        _parameter_flag2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt1() As String
                Get
                    Return _parameter_txt1
                End Get
                Set(value As String)
                    If _parameter_txt1 <> value Then
                        _parameter_txt1 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt2() As String
                Get
                    Return _parameter_txt2
                End Get
                Set(value As String)
                    If _parameter_txt2 <> value Then
                        _parameter_txt2 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
            Public Property parameter_txt3() As String
                Get
                    Return _parameter_txt3
                End Get
                Set(value As String)
                    If _parameter_txt3 <> value Then
                        _parameter_txt3 = value
                        Me.IsChanged = True
                    End If
                End Set
            End Property
#End Region


            ''' <summary>
            ''' constructor for ormDataObject
            ''' </summary>
            ''' <param name="tableid"></param>
            ''' <remarks></remarks>
            Protected Sub New(tableid As String, Optional dbdriver As iormDBDriver = Nothing)
                _IsInitialized = False
                _TableID = tableid
                _dbdriver = dbdriver
            End Sub
            ''' <summary>
            ''' clean up with the object
            ''' </summary>
            ''' <remarks></remarks>
            Public Sub Finialize()
                _IsInitialized = False
                Me.Record = Nothing
                _TableID = ""
                _dbdriver = Nothing
            End Sub
          
            '*****
            '*****
            Private Sub NotifyPropertyChanged(Optional ByVal propertyname As String = Nothing)
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyname))

            End Sub
            ''' <summary>
            ''' initialize the data object
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Initialize() As Boolean Implements iormPersistable.Initialize

                '** set tableid
                If Me.TableID <> "" Then
                    _record = New ormRecord(Me.TableID, dbdriver:=_dbdriver)
                    If _record.IsTableSet Then
                        Initialize = True
                    Else
                        Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="record ist not set to tabledefinition", _
                                                messagetype:=otCoreMessageType.InternalError, tablename:=Me.TableID, noOtdbAvailable:=True)
                        Initialize = False
                    End If

                    If Not Me.Record.TableStore Is Nothing AndAlso Not Me.Record.TableStore.Connection Is Nothing _
                    AndAlso Not Me.Record.TableStore.Connection.IsConnected Then
                        Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="TableStore is not connected to database / no connection available", _
                                                messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                        Initialize = False
                    End If
                    '** register for caching
                    'Call Cache.RegisterCacheFor(ObjectTag:=Me.TableID)

                ElseIf Me.TableID = "" Then
                    Call CoreMessageHandler(subname:="ormDataObject.Initialize", message:="Tablename / id is blank for OTDB object", _
                                            messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True)
                    Initialize = False
                End If

                '* default values
                _updatedOn = ConstNullDate
                _createdOn = ConstNullDate
                _deletedOn = ConstNullDate
                _IsDeleted = False
                _parameter_date1 = ConstNullDate
                _parameter_date2 = ConstNullDate
                _parameter_date3 = ConstNullDate
                _parameter_flag1 = False
                _parameter_flag2 = False
                _parameter_flag3 = False
                _parameter_num1 = 0
                _parameter_num2 = 0
                _parameter_num3 = 0
                _parameter_txt1 = ""
                _parameter_txt2 = ""
                _parameter_txt3 = ""
                '** set initialized
                _IsInitialized = Initialize
                Return Initialize
            End Function
            ''' <summary>
            ''' load DataObject by Type and Primary Key-Array
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="pkArray"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function LoadDataObjectBy(Of T As {iormInfusable, iormPersistable, New})(pkArray() As Object, Optional dbdriver As iormDBDriver = Nothing) As iormPersistable
                Dim aDataObject As New T
                If dbdriver IsNot Nothing Then aDataObject.dbdriver = dbdriver
                If aDataObject.LoadBy(pkArray) Then
                    Return aDataObject
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' loads and infuse the deliverable by primary key from the data store
            ''' </summary>
            ''' <param name="UID"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function LoadBy(ByRef pkArray() As Object) As Boolean Implements iormPersistable.LoadBy
                Dim aRecord As ormRecord
                '* init
                If Not Me.IsInitialized AndAlso Not Me.Initialize Then
                    Return False
                End If


                Try
                    ' try to load it from cache
                    aRecord = TryCast(LoadFromCache("Record" & _TableID, pkArray), ormRecord)
                    If aRecord Is Nothing Then
                        aRecord = Me.TableStore.GetRecordByPrimaryKey(pkArray)
                    End If

                    If aRecord Is Nothing Then
                        _IsLoaded = False
                        Return False
                    Else
                        Call AddToCache("Record" & _TableID, key:=pkArray, theOBJECT:=aRecord)
                        _IsLoaded = Me.Infuse(aRecord)
                        '* reset flags
                        If Me.IsLoaded Then
                            _IsCreated = False
                            _IsDeleted = False
                            _IsChanged = False
                        End If

                        Return Me.IsLoaded
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.Loadby", arg1:=pkArray, tablename:=_TableID)
                    Return False
                End Try


            End Function

            ''' <summary>
            ''' Persist the object to the datastore
            ''' </summary>
            ''' <param name="timestamp"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overridable Function Persist(Optional timestamp As Date = ot.ConstNullDate, Optional doFeedRecord As Boolean = True) As Boolean Implements iormPersistable.Persist

                '* init
                If Me Is Nothing Then
                    If Not Me.IsInitialized Then
                        If Not Me.Initialize() Then
                            Persist = False
                            Exit Function
                        End If
                    End If
                Else
                    If Not Me.IsInitialized Then
                        If Not Me.Initialize Then
                            Return False

                        End If
                    End If
                End If


                If Not _IsLoaded And Not Me.IsCreated Then
                    Call CoreMessageHandler(message:="data object is neither loaded nor created - unknown state", _
                                          subname:="ormDataObject.Persist", _
                                          tablename:=_TableID)
                    Return False
                End If

                If Not Me.Record.Alive Then
                    Persist = False
                    Exit Function
                End If

                Try
                    '* if object was deleted an its now repersisted
                    Dim isdeleted As Boolean = _IsDeleted

                    _IsDeleted = False

                    If doFeedRecord Then
                        FeedRecord(Me, Record)
                    End If

                    Persist = Me.Record.Persist(timestamp)

                    '** reset flags
                    If Persist Then
                        _IsCreated = False
                        _IsChanged = False
                        _IsLoaded = True
                        _IsDeleted = False
                    Else
                        _IsDeleted = isdeleted
                    End If
                    Return Persist
                Catch ex As Exception
                    Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Persist")
                    Return False
                End Try



            End Function
            ''' <summary>
            ''' Static Function ALL returns a Collection of all objects
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function All(Of T As {iormInfusable, iormPersistable, New})(Optional ID As String = "All", _
                                                                                       Optional where As String = "", _
                                                                                       Optional orderby As String = "", _
                                                                                       Optional parameters As List(Of ormSqlCommandParameter) = Nothing) _
                                                                                   As List(Of T)
                Dim aCollection As New List(Of T)
                Dim aRecordCollection As New List(Of ormRecord)
                Dim aStore As iormDataStore
                Dim aNewObject As New T

                Try
                    aStore = aNewObject.TableStore
                    aRecordCollection = aStore.GetRecordsBySqlCommand(id:=ID, wherestr:=where, orderby:=orderby, parameters:=parameters)

                    If aRecordCollection.Count > 0 Then
                        For Each aRecord In aRecordCollection
                            aNewObject = New T
                            If aNewObject.Infuse(aRecord) Then
                                aCollection.Add(Item:=aNewObject)
                            End If
                        Next aRecord

                    End If
                    Return aCollection

                Catch ex As Exception
                    Call CoreMessageHandler(exception:=ex, subname:="ormDataObject.All(of T)")
                    Return aCollection
                End Try


            End Function
            ''' <summary>
            ''' returns the Version number of the Attribute set Persistance Version
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="name"></param>
            ''' <param name="dataobject"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetVersion(dataobject As iormPersistable, Optional name As String = "") As Long Implements iormPersistable.GetVersion
                Dim aFieldList As System.Reflection.FieldInfo()

                Try
                    '***
                    '*** collect all the attributes first
                    '***
                    aFieldList = (dataobject.GetType).GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                      Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                      Reflection.BindingFlags.FlattenHierarchy)
                    '** look into each Const Type (Fields)
                    For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                        If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                            '** Attribtes
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                '** TABLE
                                If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) AndAlso name = "" Then
                                    '** Schema Definition
                                    Return (DirectCast(anAttribute, ormSchemaTableAttribute).Version)

                                    '** FIELD COLUMN
                                ElseIf anAttribute.GetType().Equals(GetType(ormSchemaColumnAttribute)) AndAlso name <> " " Then
                                    If LCase(name) = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                        Return DirectCast(anAttribute, ormSchemaColumnAttribute).Version
                                    End If

                                    '** INDEX
                                ElseIf anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                    If LCase(name) = LCase(CStr(aFieldInfo.GetValue(dataobject))) Then
                                        Return DirectCast(anAttribute, ormSchemaIndexAttribute).Version
                                    End If

                                End If

                            Next
                        End If
                    Next


                Catch ex As Exception

                    Call CoreMessageHandler(subname:="ormDataObject.GetVersion(of T)", exception:=ex)
                    Return False

                End Try
            End Function
            ''' <summary>
            ''' create the schema for this object by reflection
            ''' </summary>
            ''' <param name="silent"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function CreateSchema(Of T)(Optional silent As Boolean = False, Optional addToSchema As Boolean = True) As Boolean
                Dim aFieldList As System.Reflection.FieldInfo()
                Dim tableIDs As New List(Of String)
                Dim tableAttrIds As New List(Of String)
                Dim tableAttrDeleteFlags As New List(Of Boolean)
                Dim tableAttrSpareFieldsFlags As New List(Of Boolean)
                Dim tableAttrDomainIDFlags As New List(Of Boolean)
                Dim tableVersions As New List(Of UShort)
                Dim fieldDescs As New List(Of ormFieldDescription)
                Dim primaryKeyList As New SortedList(Of Short, String)
                Dim indexList As New Dictionary(Of String, String())
                Dim ordinalPos As Long = 1

                Try
                    '***
                    '*** collect all the attributes first
                    '***
                    aFieldList = GetType(T).GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                      Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                      Reflection.BindingFlags.FlattenHierarchy)
                    '** look into each Const Type (Fields)
                    For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                        If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                            '** Attribtes
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                                '** TABLE
                                If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                    '** Schema Definition
                                    tableIDs.Add(aFieldInfo.GetValue(Nothing))
                                    tableAttrIds.Add(DirectCast(anAttribute, ormSchemaTableAttribute).ID)
                                    tableVersions.Add(DirectCast(anAttribute, ormSchemaTableAttribute).Version)
                                    tableAttrDeleteFlags.Add(DirectCast(anAttribute, ormSchemaTableAttribute).AddDeleteFieldBehavior)
                                    tableAttrSpareFieldsFlags.Add(DirectCast(anAttribute, ormSchemaTableAttribute).AddSpareFields)
                                    tableAttrDomainIDFlags.Add(DirectCast(anAttribute, ormSchemaTableAttribute).AddDomainID)
                                    '** FIELD COLUMN
                                ElseIf anAttribute.GetType().Equals(GetType(ormSchemaColumnAttribute)) Then
                                    Dim aSchemaColumnAttribute = DirectCast(anAttribute, ormSchemaColumnAttribute)
                                    With aSchemaColumnAttribute
                                        Dim anOTDBFieldDesc As New ormFieldDescription
                                        anOTDBFieldDesc.ColumnName = aFieldInfo.GetValue(Nothing)
                                        anOTDBFieldDesc.ID = .ID
                                        anOTDBFieldDesc.Title = .Title
                                        anOTDBFieldDesc.Relation = .Relation
                                        anOTDBFieldDesc.Aliases = .Aliases
                                        anOTDBFieldDesc.IsNullable = .IsNullable
                                        anOTDBFieldDesc.Datatype = .Typeid
                                        anOTDBFieldDesc.Parameter = .Parameter
                                        anOTDBFieldDesc.Size = .Size
                                        anOTDBFieldDesc.Description = .Description
                                        anOTDBFieldDesc.DefaultValue = .DefaultValue
                                        anOTDBFieldDesc.IsArray = .IsArray
                                        anOTDBFieldDesc.Version = .Version
                                        '** ordinal position given or by the way they are coming
                                        If .Posordinal = 0 Then
                                            anOTDBFieldDesc.ordinalPosition = ordinalPos
                                            ordinalPos += 1
                                        Else
                                            anOTDBFieldDesc.ordinalPosition = .Posordinal
                                        End If

                                        anOTDBFieldDesc.SpareFieldTag = .SpareFieldTag

                                        '** add the field
                                        fieldDescs.Add(anOTDBFieldDesc)

                                        If .PrimaryKeyordinal > 0 Then
                                            If primaryKeyList.ContainsKey(.PrimaryKeyordinal) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.CreateSchema(of T)", message:="Primary key member has a position number more than once", _
                                                                       arg1:=anOTDBFieldDesc.ColumnName, messagetype:=otCoreMessageType.InternalError)
                                                Return False
                                            End If
                                            primaryKeyList.Add(.PrimaryKeyordinal, anOTDBFieldDesc.ColumnName)
                                        End If
                                    End With
                                    '** INDEX
                                ElseIf anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                    Dim theColumnNames As String() = DirectCast(anAttribute, ormSchemaIndexAttribute).ColumnNames
                                    Dim theIndexname As String = aFieldInfo.GetValue(Nothing)

                                    If indexList.ContainsKey(theIndexname) Then
                                        indexList.Remove(theIndexname)
                                    End If
                                    indexList.Add(key:=theIndexname, value:=theColumnNames)
                                End If

                            Next
                        End If
                    Next

                    Dim I As ULong = 0
                    '*** create the table with schema entries
                    For Each aTableID In tableIDs
                        Dim aStore As New ObjectDefinition

                        With aStore
                            .Create(aTableID, checkunique:=Not addToSchema, runTimeOnly:=Not addToSchema, version:=tableVersions(I))
                            '** delete the schema
                            If addToSchema Then .Delete()
                            .DomainID = CurrentSession.CurrentDomainID
                            .Version = tableVersions(I)
                            '* set table specific attributes
                            If tableAttrSpareFieldsFlags(I) Then
                                .SpareFieldsBehavior = True
                            Else
                                .SpareFieldsBehavior = False
                            End If
                            If tableAttrDeleteFlags(I) Then
                                .DeletePerFlagBehavior = True
                            Else
                                .DeletePerFlagBehavior = False
                            End If
                            If tableAttrDomainIDFlags(I) Then
                                .DomainBehavior = True
                            Else
                                .DomainBehavior = False
                            End If

                            '** create the the fields
                            For Each aFieldDesc In fieldDescs
                                aFieldDesc.Tablename = aTableID ' set here
                                Call .AddFieldDesc(fielddesc:=aFieldDesc)
                            Next

                            ' create primary key
                            Dim aCollection As New Collection
                            For Each key In primaryKeyList.Keys
                                aCollection.Add(primaryKeyList.Item(key))
                            Next
                            Call .AddIndex("PrimaryKey", aCollection, isprimarykey:=True)

                            ' create additional index
                            For Each kvp As KeyValuePair(Of String, String()) In indexList
                                ' Index
                                Dim anIndexCollection As New Collection
                                For Each fieldname As String In kvp.Value
                                    anIndexCollection.Add(fieldname)
                                Next
                                .AddIndex(indexname:=kvp.Key, fieldnames:=anIndexCollection, isprimarykey:=False)
                            Next
                            ' persist
                            If addToSchema Then .Persist()
                            ' change the database
                            .AlterSchema(addToSchema:=addToSchema)
                        End With

                        '* reload the tablestore
                        If CurrentSession.IsRunning Then
                            CurrentSession.CurrentDBDriver.GetTableStore(tableID:=aTableID, force:=True)
                        End If

                        '** now try to persist
                        If Not addToSchema Then
                            aStore.Delete()
                            aStore.Persist()
                        End If
                        '* success
                        Call CoreMessageHandler(messagetype:=otCoreMessageType.ApplicationInfo, message:="The schema for " & aTableID & " is updated", _
                                               subname:="ormDataObject.createSchema(of T)")
                        I = I + 1
                    Next

                    Return True
                Catch ex As Exception

                    Call CoreMessageHandler(subname:="ormDataObject.CreateSchema(of T)", exception:=ex)
                    Return False

                End Try



            End Function
            ''' <summary>
            ''' create a persistable dataobject of type T 
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="pkArray"></param>
            ''' <param name="checkUnique"></param>
            ''' <returns>the iotdbdataobject or nothing (if checkUnique)</returns>
            ''' <remarks></remarks>
            Protected Shared Function CreateDataObjectBy(Of T As {iormInfusable, iormPersistable, New}) _
                                (ByRef pkArray() As Object, Optional checkUnique As Boolean = False) As iormPersistable
                Dim aDataObject As New T

                If aDataObject.Create(pkArray, checkUnique:=checkUnique) Then
                    Return aDataObject
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' generic function to create a dataobject by primary key
            ''' </summary>
            ''' <param name="pkArray"></param>
            ''' <param name="dataobject"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Protected Overridable Function Create(ByRef pkArray() As Object, _
                                                  Optional checkUnique As Boolean = False, _
                                                  Optional noInitialize As Boolean = False) As Boolean Implements iormPersistable.Create

                '** initialize
                If Not noInitialize AndAlso Not Me.IsInitialized AndAlso Not Me.Initialize Then
                    Call CoreMessageHandler(message:="dataobject cannot be initialized", tablename:=_TableID, arg1:=pkArray, _
                                           messagetype:=otCoreMessageType.InternalError)

                    Return False
                End If
                '** is the object loaded -> no reinit
                If Me.IsLoaded Then
                    Call CoreMessageHandler(message:="dataobject cannot be created if it has state loaded", tablename:=_TableID, arg1:=pkArray, _
                                           messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                '** keys must be set in the object itself
                '** create
                If checkUnique Then
                    Me.Record = New ormRecord(Me.TableID)
                    ' Check
                    Dim aStore As iormDataStore = Me.TableStore
                    Dim aRecord As ormRecord = aStore.GetRecordByPrimaryKey(pkArray)

                    If Not aRecord Is Nothing Then
                        Create = False
                        'Call OTDBErrorHandler(tablename:=ourTableName, entryname:="partid, posno", _
                        'subname:="clsOTDBBOMMember.create", message:=" double key as should be unique", arg1:=partid & posno)
                        Exit Function
                    Else
                        Me.Record.IsCreated = True
                    End If
                End If

                If Not Me.Record.IsTableSet AndAlso Not noInitialize Then Me.Record.SetTable(Me.TableID)

                _IsCreated = True
                _IsDeleted = False
                _deletedOn = ConstNullDate
                _IsLoaded = False
                _IsChanged = False
                Return True
            End Function
            ''' <summary>
            ''' clone a dataobject with a new pkarray. return nothing if fails
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="cloneobject"></param>
            ''' <param name="newpkarray"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function CloneDataObject(Of T As {iormPersistable, iormCloneable, iormInfusable, New})(cloneobject As iotCloneable(Of T), newpkarray As Object()) As T
                Return cloneobject.Clone(newpkarray)
            End Function

            ''' <summary>
            ''' Retrieve a data object from the cache or load it
            ''' </summary>
            ''' <typeparam name="T"></typeparam>
            ''' <param name="pkArray"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overloads Shared Function Retrieve(Of T As {iormInfusable, ormDataObject, iormPersistable, New})(pkArray() As Object, _
                                                                                                                    Optional dbdriver As iormDBDriver = Nothing, _
                                                                                                                    Optional forceReload As Boolean = False) As T
                Dim anObject As New T
                anObject = Cache.LoadFromCache(objecttag:=anObject.TableID, key:=pkArray)
                If anObject Is Nothing OrElse forceReload Then
                    anObject = ormDataObject.LoadDataObjectBy(Of T)(pkArray:=pkArray, dbdriver:=dbdriver)
                    If Not anObject Is Nothing Then
                        Cache.RegisterCacheFor(anObject.TableID)
                        Cache.AddToCache(objectTag:=anObject.TableID, key:=pkArray, theOBJECT:=anObject)
                    End If

                End If

                Return anObject

            End Function
            ''' 
            ''' <summary>
            ''' clone the object with the new primary key
            ''' </summary>
            ''' <param name="pkarray">primary key array</param>
            ''' <remarks></remarks>
            ''' <returns>the new cloned object or nothing</returns>
            Public Overloads Function Clone(Of T As {iormPersistable, iormInfusable, New})(newpkarray As Object()) As T Implements iormCloneable.Clone
                '
                '*** now we copy the object
                Dim aNewObject As New T
                Dim newRecord As New ormRecord

                '**
                If Not Me.IsLoaded And Not Me.IsCreated Then
                    Return Nothing
                End If

                '* init
                If Not Me.IsInitialized Then
                    If Not Me.Initialize() Then
                        Return Nothing
                    End If
                End If

                ' set it
                newRecord.SetTable(Me.TableID)

                ' go through the table and overwrite the Record if the rights are there
                For Each keyname In Me.Record.Keys
                    If keyname <> ConstFNCreatedOn And keyname <> ConstFNUpdatedOn And keyname <> ConstFNIsDeleted And keyname <> ConstFNDeletedOn Then
                        Call newRecord.SetValue(keyname, Me.Record.GetValue(keyname))
                    End If
                Next keyname

                If Not aNewObject.Create(pkArray:=newpkarray, checkUnique:=True) Then
                    Call CoreMessageHandler(message:="object new keys are not unique - clone aborted", arg1:=newpkarray, tablename:=_TableID, _
                                           messagetype:=otCoreMessageType.InternalError)
                    Return Nothing
                End If

                ' actually here it we should clone all members too !
                If aNewObject.Infuse(newRecord) Then
                    Return aNewObject
                Else
                    Return Nothing
                End If
            End Function
            ''' <summary>
            ''' Undelete the data object
            ''' </summary>
            ''' <returns>True if successful</returns>
            ''' <remarks></remarks>
            Public Function Undelete() As Boolean
                If Not Me.IsInitialized Then
                    If Not Me.Initialize Then
                        Return False
                    End If
                End If
                Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.DeletePerFlagBehavior Then
                    _IsDeleted = False
                    _deletedOn = ConstNullDate
                    CoreMessageHandler(message:="data object undeleted", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                        tablename:=Me.TableID)
                    Return True
                Else
                    CoreMessageHandler(message:="data object cannot be undeleted - delete per flag behavior not set", subname:="ormDataObject.undelete", messagetype:=otCoreMessageType.InternalInfo, _
                                         tablename:=Me.TableID)
                    Return False
                End If


            End Function
            ''' <summary>
            ''' Delete the object and its persistancy
            ''' </summary>
            ''' <returns>True if successfull</returns>
            ''' <remarks></remarks>
            Public Overridable Function Delete() As Boolean Implements iormPersistable.Delete

                If Not Me.IsInitialized Then
                    If Not Me.Initialize Then
                        Return False
                    End If
                End If
                Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                If aObjectDefinition IsNot Nothing AndAlso aObjectDefinition.DeletePerFlagBehavior Then
                    _IsDeleted = True
                    _deletedOn = Date.Now()
                    Me.Persist()
                Else
                    'delete the  object itself
                    _IsDeleted = _record.Delete()
                    If _IsDeleted Then
                        Me.Unload()
                        _deletedOn = Date.Now()
                    End If

                End If

                Return _IsDeleted
            End Function
            ''' <summary>
            ''' infuse a dataobject by a record - use reflection
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <param name="record"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function Infuse(ByRef dataobject As iormPersistable, ByRef record As ormRecord)
                Dim aMemberList As System.Reflection.FieldInfo()

                Try
                    aMemberList = dataobject.GetType().GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                                 Reflection.BindingFlags.Public Or Reflection.BindingFlags.FlattenHierarchy)
                    For Each aMember As System.Reflection.MemberInfo In aMemberList
                        Dim aValue As Object

                        If aMember.MemberType = Reflection.MemberTypes.Field Then
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aMember)
                                If anAttribute.GetType().Equals(GetType(ormColumnMappingAttribute)) Then
                                    Dim aField As System.Reflection.FieldInfo = DirectCast(aMember, System.Reflection.FieldInfo)
                                    Dim aFieldType As Type = aField.FieldType
                                    If record.HasIndex(DirectCast(anAttribute, ormColumnMappingAttribute).Fieldname) Then
                                        '*** set the class internal field
                                        aValue = record.GetValue(DirectCast(anAttribute, ormColumnMappingAttribute).Fieldname)
                                        Dim converter As TypeConverter = TypeDescriptor.GetConverter(aField.FieldType)
                                        If aValue Is Nothing OrElse aField.FieldType.Equals(aValue.GetType) Then
                                            aField.SetValue(dataobject, aValue)
                                        ElseIf converter.GetType.Equals(GetType(EnumConverter)) Then
                                            Dim anewValue As Object = CTypeDynamic(aValue, aFieldType)
                                            aField.SetValue(dataobject, anewValue)
                                        ElseIf converter.CanConvertFrom(aValue.GetType) Then
                                            Dim anewvalue As Object = converter.ConvertFrom(aValue)
                                            aField.SetValue(dataobject, anewvalue)
                                        ElseIf aField.FieldType.Equals(GetType(Long)) AndAlso IsNumeric(aValue) Then
                                            aField.SetValue(dataobject, CLng(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(Boolean)) Then
                                            aField.SetValue(dataobject, CBool(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(String)) Then
                                            aField.SetValue(dataobject, CStr(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(Integer)) AndAlso IsNumeric(aValue) Then
                                            aField.SetValue(dataobject, CInt(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(UInteger)) AndAlso IsNumeric(aValue) _
                                            AndAlso aValue >= UInteger.MinValue AndAlso aValue <= UInteger.MaxValue Then
                                            aField.SetValue(dataobject, CUInt(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(UShort)) And IsNumeric(aValue) _
                                            AndAlso aValue >= UShort.MinValue AndAlso aValue <= UShort.MaxValue Then
                                            aField.SetValue(dataobject, CUShort(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(ULong)) And IsNumeric(aValue) _
                                             AndAlso aValue >= ULong.MinValue AndAlso aValue <= ULong.MaxValue Then
                                            aField.SetValue(dataobject, CULng(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(Double)) And IsNumeric(aValue) _
                                            AndAlso aValue >= Double.MinValue AndAlso aValue <= Double.MaxValue Then
                                            aField.SetValue(dataobject, CDbl(aValue))
                                        ElseIf aField.FieldType.Equals(GetType(Decimal)) And IsNumeric(aValue) _
                                          AndAlso aValue >= Decimal.MinValue AndAlso aValue <= Decimal.MaxValue Then
                                            aField.SetValue(dataobject, CDec(aValue))
                                        Else
                                            Call CoreMessageHandler(subname:="ormDataObject.infuse", message:="cannot convert record value type to field type", _
                                                                   entryname:=DirectCast(anAttribute, ormColumnMappingAttribute).Fieldname, tablename:=dataobject.TableID, _
                                                                   arg1:=aField.Name, messagetype:=otCoreMessageType.InternalError)
                                        End If

                                    End If
                                End If

                            Next
                        End If
                    Next

                    Return True
                Catch ex As Exception

                    Call CoreMessageHandler(subname:="ormDataObject.Infuse", exception:=ex, tablename:=dataobject.TableID)
                    Return False

                End Try

            End Function

            ''' <summary>
            ''' Feed the record belonging to the data object
            ''' </summary>
            ''' <returns>True if successful</returns>
            ''' <remarks></remarks>
            Public Function FeedRecord() As Boolean
                If Me.IsLoaded Or Me.IsCreated Then
                    Return FeedRecord(Me, Me.Record)
                End If
                Return False
            End Function
            ''' <summary>
            ''' feed the record from the field of an data object - use reflection of attribute otfieldname
            ''' </summary>
            ''' <param name="dataobject"></param>
            ''' <param name="record"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Function FeedRecord(ByRef dataobject As iormPersistable, ByRef record As ormRecord) As Boolean
                Dim aMemberList As System.Reflection.FieldInfo()

                Try
                    aMemberList = dataobject.GetType().GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                                 Reflection.BindingFlags.Public Or Reflection.BindingFlags.FlattenHierarchy)
                    For Each aMember As System.Reflection.MemberInfo In aMemberList
                        Dim aValue As Object

                        If aMember.MemberType = Reflection.MemberTypes.Field Then
                            For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aMember)
                                If anAttribute.GetType().Equals(GetType(ormColumnMappingAttribute)) Then
                                    Dim aField As System.Reflection.FieldInfo = DirectCast(aMember, System.Reflection.FieldInfo)
                                    If Not record.IsTableSet OrElse _
                                        (record.IsTableSet And record.HasIndex(DirectCast(anAttribute, ormColumnMappingAttribute).Fieldname)) Then
                                        aValue = aField.GetValue(dataobject)
                                        record.SetValue(DirectCast(anAttribute, ormColumnMappingAttribute).Fieldname, aValue)
                                    End If
                                End If

                            Next
                        End If
                    Next

                    Return True
                Catch ex As Exception

                    Call CoreMessageHandler(subname:="ormDataObject.FeedRecord", exception:=ex, tablename:=dataobject.TableID)
                    Return False

                End Try




            End Function
            ''' <summary>
            ''' infuses a dataobject by a record
            ''' </summary>
            ''' <param name="Record">a fixed clsOTDBRecord with the persistence data</param>
            ''' <returns>true if successfull</returns>
            ''' <remarks>might be overwritten by class descendants but make sure that you call mybase.infuse</remarks>
            Public Overridable Function Infuse(ByRef record As ormRecord) As Boolean Implements iormInfusable.Infuse

                '* lazy init
                If Not Me.IsInitialized Then
                    If Not Me.Initialize() Then
                        Infuse = False
                        Exit Function
                    End If
                End If

                Try
                    Me.Record = record
                    If Not Infuse(Me, record) Then
                        '** minimal program if we failed to infuse by reflection
                        If Me.TableSchema.HasFieldname(ConstFNUpdatedOn) Then
                            _updatedOn = CDate(record.GetValue(ConstFNUpdatedOn))
                        End If
                        If Me.TableSchema.HasFieldname(ConstFNCreatedOn) Then
                            _createdOn = CDate(record.GetValue(ConstFNCreatedOn))
                        End If
                        If Me.TableSchema.HasFieldname(ConstFNDeletedOn) Then
                            _createdOn = CDate(record.GetValue(ConstFNDeletedOn))
                        End If
                    End If

                    record.IsLoaded = True
                    _IsLoaded = True
                    Return True

                Catch ex As Exception
                    Call CoreMessageHandler(message:="Exception", exception:=ex, subname:="ormDataObject.Infuse", _
                                           tablename:=Me.TableID, messagetype:=otCoreMessageType.InternalException)
                    Return False
                End Try


            End Function

        End Class


        '*******************************************************************************************
        '***** CLASS clsOTDBTableSTore is the neutral class workhorse ORM class for peristence 
        '*****
        ''' <summary>
        ''' TopLevel OTDB Tablestore implementation base class
        ''' </summary>
        ''' <remarks></remarks>
        Public MustInherit Class ormTableStore
            Implements iormDataStore

            Private _TableID As String 'Name of the Table or Datastore in the Database
            Private _TableSchema As iotTableSchema  'Schema (Description) of the Table or DataStore
            Private _Connection As iormConnection  ' Connection to use to access the Table or Datastore

            Private _PropertyBag As New Dictionary(Of String, Object)

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
                                keyfieldname = TableSchema.GetPrimaryKeyFieldname(anIndex + 1)
                                Exit For
                            End If
                            anIndex += 1
                        Next
                        '*
                        aCommand.select = "max(" & keyfieldname & ")"
                        If anIndex > 0 Then
                            For j = 0 To anIndex - 1 ' an index points to the keyfieldname, parameter is the rest
                                If j > 0 Then aCommand.Where &= " AND "
                                aCommand.Where &= TableSchema.GetPrimaryKeyFieldname(j + 1) & " = @" & TableSchema.GetPrimaryKeyFieldname(j + 1)
                                aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & TableSchema.GetPrimaryKeyFieldname(j + 1), _
                                                                                     fieldname:=TableSchema.GetPrimaryKeyFieldname(j + 1), tablename:=Me.TableID))
                            Next
                        End If
                        aCommand.Prepare()
                    End If

                    '* retrieve the maximum field -> and sets the index
                    anIndex = 0
                    For Each pkvalue In pkArray
                        If Not pkvalue Is Nothing Then
                            aCommand.SetParameterValue(ID:="@" & TableSchema.GetPrimaryKeyFieldname(anIndex + 1), value:=pkvalue)
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
                        If Not IsNull(theRecords.Item(0).GetValue(1)) And IsNumeric(theRecords.Item(0).GetValue(1)) Then
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
                ''' TODO: Connection Refresh
                '** 
                If Not Connection Is Nothing AndAlso Connection.IsConnected Then
                    Me.TableSchema = Connection.DatabaseDriver.GetTableSchema(TableID, force:=force)

                    If Me.TableSchema Is Nothing OrElse Not Me.TableSchema.IsInitialized Then
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
                    Me._TableID = value
                End Set
            End Property

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
            Public Overridable Property TableSchema() As iotTableSchema Implements iormDataStore.TableSchema
                Get
                    Return _TableSchema
                End Get
                Friend Set(value As iotTableSchema)
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
            ''' gets a List of clsOTDBRecords by SQLCommand
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
            Public Overridable Function InfuseRecord(ByRef newRecord As ormRecord, ByRef RowObject As Object, Optional ByVal silent As Boolean = False) As Boolean Implements iormDataStore.InfuseRecord
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

            Public MustOverride Function Convert2ColumnData(ByVal value As Object, _
                                                        targetType As Long, _
                                                        Optional ByVal maxsize As Long = 0, _
                                                       Optional ByRef abostrophNecessary As Boolean = False, _
                                                       Optional ByVal fieldname As String = "") As Object Implements iormDataStore.Convert2ColumnData


            ''' <summary>
            ''' Convert2s the column data.
            ''' </summary>
            ''' <param name="anIndex">An index.</param>
            ''' <param name="aVAlue">A V alue.</param>
            ''' <param name="abostrophNecessary">The abostroph necessary.</param>
            ''' <returns></returns>
            Public Overridable Function Convert2ColumnData(index As Object, ByVal value As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object Implements iormDataStore.Convert2ColumnData
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
            Public Overridable Function Convert2ObjectData(index As Object, ByVal value As Object, Optional ByRef abostrophNecessary As Boolean = False) As Object Implements iormDataStore.Convert2ObjectData
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
                If Not LCase(id).Contains((LCase(Me.TableID & "."))) Then
                    Return Me.TableID & "." & id
                Else
                    Return id
                End If
            End Function
        End Class



        '*******************************************************************************************
        '***** CLASS clsOTDBTableSchema describes the per Table the schema from the database itself
        '*****

        Public MustInherit Class ormTableSchema
            Implements iotTableSchema

            Protected _TableID As String

            Protected _fieldsDictionary As Dictionary(Of String, Long)    ' crossreference to the Arrays
            Protected _indexDictionary As Dictionary(Of String, ArrayList)    ' crossreference of the Index

            Protected _Fieldnames() As String    ' Fieldnames in OTDB
            Protected _Primarykeys() As UShort    ' indices for primary keys
            Protected _NoPrimaryKeys As UShort
            Protected _PrimaryKeyIndexName As String

            Protected _IsInitialized As Boolean = False

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
                                                                 Optional parametername As String = "") As System.Data.IDbDataParameter Implements iotTableSchema.AssignNativeDBParameter


            ''' <summary>
            ''' Gets or sets the is initialized. Should be True if the tableschema has a tableid 
            ''' </summary>
            ''' <value>The is initialized.</value>
            Public ReadOnly Property IsInitialized() As Boolean Implements iotTableSchema.IsInitialized
                Get
                    Return Me._IsInitialized
                End Get

            End Property

            ''' <summary>
            ''' resets the TableSchema to hold nothing
            ''' </summary>
            ''' <remarks></remarks>
            Protected Overridable Sub reset()
                Dim nullArray As Object = {}
                _Fieldnames = nullArray
                _fieldsDictionary.Clear()
                _indexDictionary.Clear()
                _PrimaryKeyIndexName = ""
                _Primarykeys = nullArray
                _NoPrimaryKeys = 0
                _TableID = ""
            End Sub

            MustOverride Property TableID As String Implements iotTableSchema.TableID
            Public MustOverride Function Refresh(Optional reloadForce As Boolean = False) As Boolean Implements iotTableSchema.Refresh
            ''' <summary>
            ''' Names of the Indices of the table
            ''' </summary>
            ''' <value>List(of String)</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Indices As List(Of String) Implements iotTableSchema.Indices
                Get
                    Return _indexDictionary.Keys.ToList
                End Get

            End Property
            ''' <summary>
            ''' returns the default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function GetDefaultValue(ByVal index As Object) As Object Implements iotTableSchema.GetDefaultValue

            ''' <summary>
            ''' returns if there is a default Value
            ''' </summary>
            ''' <param name="index"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public MustOverride Function HasDefaultValue(ByVal index As Object) As Boolean Implements iotTableSchema.HasDefaultValue


            '**** getIndex returns the ArrayList of Fieldnames for the Index or Nothing
            ''' <summary>
            '''  returns the ArrayList of Fieldnames for the Index or empty array list if not found
            ''' </summary>
            ''' <param name="indexname"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetIndex(indexname As String) As ArrayList Implements iotTableSchema.GetIndex


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
            Public Function HasIndex(indexname As String) As Boolean Implements iotTableSchema.HasIndex
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
            ReadOnly Property PrimaryKeyIndexName As String Implements iotTableSchema.PrimaryKeyIndexName
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
            Public ReadOnly Property NoFields() As Integer Implements iotTableSchema.NoFields
                Get
                    Return UBound(_Fieldnames) + 1 'zero bound
                End Get
            End Property
            ''' <summary>
            ''' List of Fieldnames
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Fieldnames() As List(Of String) Implements iotTableSchema.Fieldnames
                Get
                    Return _Fieldnames.ToList

                End Get

            End Property

            '***** gets the FieldIndex of index as numeric (than must be in range) or name
            ''' <summary>
            ''' Get the Fieldordinal (position in record) by Index - can be numeric or the columnname
            ''' </summary>
            ''' <param name="anIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetFieldordinal(index As Object) As Integer Implements iotTableSchema.GetFieldordinal
                Dim i As ULong

                Try
                    If IsNumeric(index) Then
                        If CLng(index) > 0 And CLng(index) <= (_Fieldnames.GetUpperBound(0) + 1) Then
                            Return CLng(index)
                        Else
                            Call CoreMessageHandler(message:="index of column out of range", _
                                             arg1:=index, subname:="clsOTDBTableSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                            Return i
                        End If
                    ElseIf _fieldsDictionary.ContainsKey(index) Then
                        Return _fieldsDictionary.Item(index)
                    ElseIf _fieldsDictionary.ContainsKey(LCase(index)) Then
                        Return _fieldsDictionary.Item(LCase(index))

                    Else
                        Call CoreMessageHandler(message:="index of column out of range", _
                                              arg1:=index, subname:="clsOTDBTableSchema.getFieldIndex", messagetype:=otCoreMessageType.InternalError)
                        Return -1
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(arg1:=index, subname:="clsOTDBTableSchema.getFieldIndex", exception:=ex)
                    Return -1
                End Try

            End Function


            ''' <summary>
            ''' get the fieldname by index i - nothing if not in range
            ''' </summary>
            ''' <param name="i"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetFieldname(ByVal i As Integer) As String Implements iotTableSchema.GetFieldname

                If i > 0 And i <= UBound(_Fieldnames) + 1 Then
                    Return _Fieldnames(i - 1)
                Else
                    Call CoreMessageHandler(message:="index of column out of range", arg1:=i, tablename:=Me.TableID, _
                                          messagetype:=otCoreMessageType.InternalError, subname:="clsOTDBTableSchema.getFieldName")
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
            Public Function HasFieldname(ByVal name As String) As Boolean Implements iotTableSchema.HasFieldname

                Dim i As Integer

                For i = LBound(_Fieldnames) To UBound(_Fieldnames)
                    If LCase(_Fieldnames(i)) = LCase(name) Then
                        HasFieldname = True
                        Exit Function
                    End If
                Next i

                HasFieldname = False
            End Function

            ''' <summary>
            ''' gets the fieldname of the primary key field by number
            ''' </summary>
            ''' <param name="i">1..n</param>
            ''' <returnsString></returns>
            ''' <remarks></remarks>
            Public Function GetPrimaryKeyFieldname(i As UShort) As String Implements iotTableSchema.GetPrimaryKeyFieldname
                Dim aCollection As ArrayList

                If i < 1 Then
                    Call CoreMessageHandler(subname:="clsOTDTable.getPrimaryKeyFieldName", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          arg1:=i)
                    Return ""
                End If

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        If i > aCollection.Count Then
                            Call CoreMessageHandler(subname:="clsOTDTable.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  arg1:=i)
                            Return ""

                        End If

                        '*** return the item (Name)
                        Return aCollection.Item(i - 1)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBTableSchema.getPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=i)
                        Return ""
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsOTDTable.getPrimaryKeyFieldName", _
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
            Public Function HasPrimaryKeyFieldname(ByRef name As String) As Boolean Implements iotTableSchema.HasprimaryKeyFieldname
                Dim aCollection As ArrayList


                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)

                        '*** return the item (Name)
                        Return aCollection.Contains(name)
                    Else
                        Call CoreMessageHandler(subname:="clsOTDBTableSchema.hasPrimaryKeyName", _
                                              message:="Primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        Return Nothing
                    End If


                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsOTDTable.hasPrimaryKeyName", _
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
            Public Function GetordinalOfPrimaryKeyField(i As UShort) As Integer Implements iotTableSchema.GetordinalOfPrimaryKeyField
                Dim aCollection As ArrayList
                Dim aFieldName As String


                If i < 1 Then
                    Call CoreMessageHandler(subname:="clsOTDTable.getPrimaryKeyFieldIndex", _
                                          message:="primary Key no : " & i.ToString & " is less then 1", _
                                          arg1:=i)
                    GetordinalOfPrimaryKeyField = -1
                    Exit Function
                End If

                Try


                    If _indexDictionary.ContainsKey((_PrimaryKeyIndexName)) Then
                        aCollection = _indexDictionary.Item((_PrimaryKeyIndexName))

                        If i > aCollection.Count Then
                            Call CoreMessageHandler(subname:="clsOTDTable.getPrimaryKeyFieldIndex", _
                                                  message:="primary Key no : " & i.ToString & " is out of range ", _
                                                  arg1:=i)
                            GetordinalOfPrimaryKeyField = -1
                            Exit Function
                        End If

                        aFieldName = aCollection.Item(i - 1)
                        GetordinalOfPrimaryKeyField = _fieldsDictionary.Item((aFieldName))
                        Exit Function
                    Else
                        Call CoreMessageHandler(subname:="clsOTDTable.getPrimaryKeyFieldIndex", _
                                              message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=i)
                        System.Diagnostics.Debug.WriteLine("clsOTDBTableSchema: primary Key : " & _PrimaryKeyIndexName & " does not exist !")
                        GetordinalOfPrimaryKeyField = -1
                        Exit Function
                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsOTDTable.getPrimaryKeyFieldIndex", tablename:=_TableID, exception:=ex)
                    Return -1
                End Try
            End Function

            ''' <summary>
            ''' get the number of primary key fields
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function NoPrimaryKeyFields() As Integer Implements iotTableSchema.NoPrimaryKeyFields
                Dim aCollection As ArrayList

                Try


                    If _indexDictionary.ContainsKey(_PrimaryKeyIndexName) Then
                        aCollection = _indexDictionary.Item(_PrimaryKeyIndexName)
                        Return aCollection.Count

                    Else
                        Call CoreMessageHandler(subname:="clsOTDBTableSchema.noPrimaryKeysFields", message:="primary Key : " & _PrimaryKeyIndexName & " does not exist !", _
                                              arg1:=_PrimaryKeyIndexName, tablename:=_TableID)
                        Return -1

                    End If

                Catch ex As Exception
                    Call CoreMessageHandler(showmsgbox:=False, subname:="clsOTDTable.noPrimaryKeys", tablename:=TableID, exception:=ex)
                    Return -1
                End Try


            End Function


        End Class
    End Namespace
End Namespace
