
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE ORM Attribute Classes for On Track Database Backend Library
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-06
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

Namespace OnTrack.Database
    ''' <summary>
    ''' OTDBDataObject Attribute links a class variable to a datastore table and field
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormEntryMapping
        Inherits Attribute

        Private _ID As String
        Private _entryname As String 'Object Entry Name
        Private _columnname As String ' table column optional
        Private _tableID As String ' table name optional
        Private _relationName As String '** if a relation definition is used
        Private _keyentries As String() ' name of the entries for keys (if the datastructure has a key such as dictionary)
        Private _InfuseMode As Nullable(Of otInfuseMode)

        ''' <summary>
        ''' Gets or sets the infuse mode.
        ''' </summary>
        ''' <value>The infuse mode.</value>
        Public Property InfuseMode() As otInfuseMode
            Get
                Return Me._InfuseMode
            End Get
            Set
                Me._InfuseMode = Value
            End Set
        End Property
        Public ReadOnly Property HasValueInfuseMode As Boolean
            Get
                Return _InfuseMode.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the relation.
        ''' </summary>
        ''' <value>The name of the relation.</value>
        Public Property RelationName() As String
            Get
                Return Me._relationName
            End Get
            Set(value As String)
                Me._relationName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueRelationName As Boolean
            Get
                Return _relationName IsNot Nothing AndAlso _relationName <> ""
            End Get
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
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing AndAlso _ID <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Property TableName() As String
            Get
                Return Me._tableID
            End Get
            Set(value As String)
                Me._tableID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTablename As Boolean
            Get
                Return _tableID IsNot Nothing AndAlso _tableID <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object's entry name.
        ''' </summary>
        ''' <value>The entry name.</value>
        Public Property EntryName() As String
            Get
                Return Me._entryname
            End Get
            Set(value As String)
                Me._entryname = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueEntryName As Boolean
            Get
                Return _entryname IsNot Nothing AndAlso _entryname <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the field name.
        ''' </summary>
        ''' <value>The fieldname.</value>
        Public Property ColumnName() As String
            Get
                Return Me._columnname
            End Get
            Set(value As String)
                Me._columnname = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueColumnName As Boolean
            Get
                Return _columnname IsNot Nothing AndAlso _columnname <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the field name.
        ''' </summary>
        ''' <value>The fieldname.</value>
        Public Property KeyEntries() As String()
            Get
                Return Me._keyentries
            End Get
            Set(value As String())
                For Each s In value
                    s = s.ToUpper
                Next
                Me._keyentries = value
            End Set
        End Property
        Public ReadOnly Property HasValueKeysEntries As Boolean
            Get
                Return _keyentries IsNot Nothing AndAlso _keyentries.Count > 0
            End Get
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
        Public Property TableName() As String
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
        Private _ID As String
        Private _Version As Nullable(Of UShort) = 1 'needed for checksum
        Private _DeleteFieldFlag As Nullable(Of Boolean)
        Private _SpareFieldsFlag As Nullable(Of Boolean)
        Private _AddDomainBehaviorFlag As Nullable(Of Boolean)
        Private _TableName As String
        Private _ObjectID As String
        Private _Description As String = ""
        Private _PrimaryKeyName As String
        Private _CacheProperties As String()
        Private _useCache As Nullable(Of Boolean)

        '** dynamic
        Private _columns As New Dictionary(Of String, ormSchemaTableColumnAttribute)
        Private _foreignkeys As New Dictionary(Of String, ormSchemaForeignKeyAttribute)

        Public Sub New()

        End Sub
        ''' <summary>
        ''' Gets or sets the cache is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property UseCache() As Boolean
            Get
                Return Me._useCache
            End Get
            Set(value As Boolean)
                Me._useCache = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseCache As Boolean
            Get
                Return _useCache.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cache select.
        ''' </summary>
        ''' <value>cache.</value>
        Public Property CacheProperties() As String()
            Get
                Return Me._CacheProperties
            End Get
            Set(value As String())
                Me._CacheProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueCacheProperties As Boolean
            Get
                Return _CacheProperties IsNot Nothing AndAlso _CacheProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Add an entry by TabeColumn
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddColumn(entry As ormSchemaTableColumnAttribute) As Boolean
            If _columns.ContainsKey(entry.ColumnName.ToUpper) Then
                _columns.Remove(entry.ColumnName.ToUpper)
            End If
            _columns.Add(key:=entry.ColumnName.ToUpper, value:=entry)
            Return True
        End Function
        ''' <summary>
        ''' Add an entry by TabeColumn
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UpdateColumn(entry As ormSchemaTableColumnAttribute) As Boolean
            If _columns.ContainsKey(entry.ColumnName.ToUpper) Then
                _columns.Remove(entry.ColumnName.ToUpper)
            End If
            _columns.Add(key:=entry.ColumnName.ToUpper, value:=entry)
            Return True
        End Function
        ''' <summary>
        ''' returns an entry by columnname or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetColumn(columnname As String) As ormSchemaTableColumnAttribute
            If _columns.ContainsKey(columnname.ToUpper) Then
                Return _columns.Item(columnname.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns an entry by columnname or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasColumn(columnname As String) As Boolean
            Return _columns.ContainsKey(columnname.ToUpper)
        End Function
        ''' <summary>
        ''' remove an entry by columnname 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveColumn(columnname As String) As Boolean
            If _columns.ContainsKey(columnname.ToUpper) Then
                _columns.Remove(columnname.ToUpper)
                Return True
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ColumnAttributes As IEnumerable(Of ormSchemaTableColumnAttribute)
            Get
                Return _columns.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' Add an foreign key entry
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddForeignKey(entry As ormSchemaForeignKeyAttribute) As Boolean
            If _foreignkeys.ContainsKey(entry.ID.ToUpper) Then
                _foreignkeys.Remove(entry.ID.ToUpper)
            End If
            _foreignkeys.Add(key:=entry.ID.ToUpper, value:=entry)
            Return True
        End Function
        ''' <summary>
        ''' returns an foreign key attribute
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetForeignkey(id As String) As ormSchemaForeignKeyAttribute
            If _foreignkeys.ContainsKey(id.ToUpper) Then
                Return _foreignkeys.Item(id.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns true if an foreign key entry exists
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasForeignkey(id As String) As Boolean
            Return _foreignkeys.ContainsKey(id.ToUpper)
        End Function
        ''' <summary>
        ''' remove a foreign key entry
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveForeignKey(id As String) As Boolean
            If _foreignkeys.ContainsKey(id.ToUpper) Then
                _foreignkeys.Remove(id.ToUpper)
                Return True
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ForeignKeyAttributes As IEnumerable(Of ormSchemaForeignKeyAttribute)
            Get
                Return _foreignkeys.Values.ToList
            End Get
        End Property
        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ColumnNames As IEnumerable(Of String)
            Get
                Return _columns.Keys.ToList
            End Get
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing AndAlso _Description <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property PrimaryKey() As String
            Get
                Return Me._PrimaryKeyName
            End Get
            Set(value As String)
                Me._PrimaryKeyName = value
            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKey As Boolean
            Get
                Return _PrimaryKeyName IsNot Nothing AndAlso _PrimaryKeyName <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object ID.
        ''' </summary>
        ''' <value>The object ID.</value>
        Public Property ObjectID() As String
            Get
                Return Me._ObjectID
            End Get
            Set(value As String)
                Me._ObjectID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueObjectID As Boolean
            Get
                Return _ObjectID IsNot Nothing AndAlso _ObjectID <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property TableName() As String
            Get
                Return Me._TableName
            End Get
            Set(value As String)
                Me._TableName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableName As Boolean
            Get
                Return _TableName IsNot Nothing AndAlso _TableName <> ""
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the add domain ID flag.
        ''' </summary>
        ''' <value>The add domain ID flag.</value>
        Public Property AddDomainBehavior() As Boolean
            Get
                Return Me._AddDomainBehaviorFlag
            End Get
            Set(value As Boolean)
                Me._AddDomainBehaviorFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueAddDomainBehavior As Boolean
            Get
                Return _AddDomainBehaviorFlag.HasValue
            End Get
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
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
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing AndAlso _ID <> ""
            End Get
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
        Public ReadOnly Property HasValueDeleteFieldBehavior As Boolean
            Get
                Return _DeleteFieldFlag.HasValue
            End Get
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
        Public ReadOnly Property HasValueSpareFields As Boolean
            Get
                Return _SpareFieldsFlag.HasValue
            End Get
        End Property
    End Class
    ''' <summary>
    ''' Attribute Class for marking an constant field member in a class as Table name such as
    ''' <otSchemaTable(Version:=1)>Const constTableName = "tblName"
    ''' Version will be saved into clsOTDBDEfSchemaTable
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormSchemaRelationAttribute
        Inherits Attribute
        Private _Name As String
        Private _Version As Nullable(Of UShort)
        Private _TableName As String
        Private _LinkedwithObject As System.Type
        Private _LinkJoin As String
        Private _FromEntries As String()
        Private _ToEntries As String()
        Private _ToPrimaryKeys As String()

        Private _CascadeOnCreate As Nullable(Of Boolean)
        Private _CascadeOnDelete As Nullable(Of Boolean)
        Private _CascadeOnUpdate As Nullable(Of Boolean)
        Public Sub New()

        End Sub

        ''' <summary>
        ''' Gets or sets the cascade on update.
        ''' </summary>
        ''' <value>The cascade on update.</value>
        Public Property CascadeOnUpdate() As Boolean
            Get
                Return Me._CascadeOnUpdate
            End Get
            Set(value As Boolean)
                Me._CascadeOnUpdate = Value
            End Set
        End Property
        Public ReadOnly Property HasValueCascadeOnUpdate As Boolean
            Get
                Return _CascadeOnUpdate.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cascade on delete.
        ''' </summary>
        ''' <value>The cascade on delete.</value>
        Public Property CascadeOnDelete() As Boolean
            Get
                Return Me._CascadeOnDelete
            End Get
            Set(value As Boolean)
                Me._CascadeOnDelete = Value
            End Set
        End Property
        Public ReadOnly Property HasValueCascadeOnDelete As Boolean
            Get
                Return _CascadeOnDelete.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cascade on create.
        ''' </summary>
        ''' <value>The cascade on create.</value>
        Public Property CascadeOnCreate() As Boolean
            Get
                Return Me._CascadeOnCreate
            End Get
            Set(value As Boolean)
                Me._CascadeOnCreate = Value
            End Set
        End Property
        Public ReadOnly Property HasValueCascadeOnCreate As Boolean
            Get
                Return _CascadeOnCreate.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets to primary keys of the linkes object.
        ''' </summary>
        ''' <value>To primary keys.</value>
        Public Property ToPrimaryKeys() As String()
            Get
                Return Me._ToPrimaryKeys
            End Get
            Set(value As String())
                Me._ToPrimaryKeys = Value
            End Set
        End Property
        Public ReadOnly Property HasValueToPrimarykeys As Boolean
            Get
                Return _ToPrimaryKeys IsNot Nothing AndAlso _ToPrimaryKeys.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets to entries.
        ''' </summary>
        ''' <value>To entries.</value>
        Public Property ToEntries() As String()
            Get
                Return Me._ToEntries
            End Get
            Set(value As String())
                Me._ToEntries = Value
            End Set
        End Property
        Public ReadOnly Property HasValueToEntries As Boolean
            Get
                Return _ToEntries IsNot Nothing AndAlso _ToEntries.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets from entries.
        ''' </summary>
        ''' <value>From entries.</value>
        Public Property FromEntries() As String()
            Get
                Return Me._FromEntries
            End Get
            Set(value As String())
                Me._FromEntries = Value
            End Set
        End Property
        Public ReadOnly Property HasValueFromEntries As Boolean
            Get
                Return _FromEntries IsNot Nothing AndAlso _FromEntries.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the linkedwith object.
        ''' </summary>
        ''' <value>The linkedwith object.</value>
        Public Property LinkObject() As Type
            Get
                Return Me._LinkedwithObject
            End Get
            Set(value As Type)
                Me._LinkedwithObject = value
            End Set
        End Property
        Public ReadOnly Property HasValueLinkedObject As Boolean
            Get
                Return _LinkedwithObject IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the link join.
        ''' </summary>
        ''' <value>The link join.</value>
        Public Property LinkJoin() As String
            Get
                Return Me._LinkJoin
            End Get
            Set(value As String)
                Me._LinkJoin = value
            End Set
        End Property
        Public ReadOnly Property HasValueLinkJOin As Boolean
            Get
                Return _LinkJoin IsNot Nothing AndAlso _LinkJoin <> ""
            End Get
        End Property
       
        ''' <summary>
        ''' Gets the name.
        ''' </summary>
        ''' <value>The name.</value>
        Public Property Name() As String
            Get
                Return Me._Name
            End Get
            Set(value As String)
                _Name = value
            End Set
        End Property
        Public ReadOnly Property HasValueName As Boolean
            Get
                Return _Name IsNot Nothing AndAlso _Name <> ""
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property TableName() As String
            Get
                Return Me._TableName
            End Get
            Set(value As String)
                Me._TableName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableName As Boolean
            Get
                Return _TableName IsNot Nothing AndAlso _TableName <> ""
            End Get
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property

    End Class


    ''' <summary>
    ''' Attributes for Schema Generation of an Index
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormSchemaIndexAttribute
        Inherits Attribute

        Private _indexName As String
        Private _ColumnNames() As String = {}
        Private _Version As Nullable(Of UShort)
        Private _TableName As String = Nothing
        Private _description As String
        Private _isprimaryKey As Nullable(Of Boolean) = False
        Private _isunique As Nullable(Of Boolean) = False
        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property TableName() As String
            Get
                Return Me._TableName
            End Get
            Set(value As String)
                Me._TableName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableName As Boolean
            Get
                Return _TableName IsNot Nothing AndAlso _TableName <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                Me._description = value
            End Set
        End Property
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _description IsNot Nothing AndAlso _description <> ""
            End Get
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets unique flag on this index.
        ''' </summary>
        ''' <value></value>
        Public Property IsUnique() As Boolean
            Get
                Return Me._isunique
            End Get
            Set(value As Boolean)
                Me._isunique = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsUnique As Boolean
            Get
                Return _isunique.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the primary key flag on this indeex.
        ''' </summary>
        ''' <value></value>
        Public Property IsPrimaryKey() As Boolean
            Get
                Return Me._isprimaryKey
            End Get
            Set(value As Boolean)
                Me._isprimaryKey = value
            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKey As Boolean
            Get
                Return _isprimaryKey.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name.
        ''' </summary>
        ''' <value>The name.</value>
        Public Property IndexName() As String
            Get
                Return Me._indexName
            End Get
            Set(value As String)
                Me._indexName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueIndexName As Boolean
            Get
                Return _indexName IsNot Nothing AndAlso _indexName <> ""
            End Get
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
        Public ReadOnly Property HasValueColumnNames As Boolean
            Get
                Return _ColumnNames IsNot Nothing AndAlso _ColumnNames.Count > 0
            End Get
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
                Me._ColumnNames(0) = value.ToUpper
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
                Me._ColumnNames(1) = value.ToUpper
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
                Me._ColumnNames(2) = value.ToUpper
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
                Me._ColumnNames(3) = value.ToUpper
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
                Me._ColumnNames(4) = value.ToUpper
            End Set
        End Property

    End Class
    ''' <summary>
    ''' Attribute for Const fields to describe the schema
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormSchemaTableColumnAttribute
        Inherits Attribute
        Protected _ID As String = Nothing

        Protected _TableID As String = Nothing
        Protected _Typeid As Nullable(Of otFieldDataType)
        Protected _InnerTypeID As Nullable(Of otFieldDataType)
        Protected _size As Nullable(Of Long)
        Protected _Parameter As String = Nothing
        Protected _primaryKeyOrdinal As Nullable(Of Short)
        Protected _relation() As String = Nothing
        Protected _IsNullable As Nullable(Of Boolean)
        Protected _IsUnique As Nullable(Of Boolean)
        Protected _DefaultValue As String = Nothing
        Protected _Version As Nullable(Of UShort)
        Protected _Posordinal As Nullable(Of UShort)
        Protected _ReferenceTableEntry As String = Nothing
        Protected _ReferenceObjectEntry As String = Nothing ' needed for resolving 
        Protected _UseForeignKey As Nullable(Of otForeignKeyImplementation) = otForeignKeyImplementation.None
        Protected _ForeignKeyReference As String() = Nothing
        Protected _ForeignKeyProperties As ForeignKeyProperty()
        Protected _ColumnName As String = Nothing
        Protected _Description As String = Nothing
        ''' <summary>
        ''' Gets or sets the ID.
        ''' </summary>
        ''' <value>The ID.</value>
        Public Property ID() As String
            Get
                Return Me._ID
            End Get
            Set(value As String)
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing
            End Get
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
                Me._ColumnName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueColumnName As Boolean
            Get
                Return _ColumnName IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the reference object entry. Has the form [objectname].[entryname] 
        ''' such as Deliverable.constObjectID & "." & deliverable.constFNUID
        ''' </summary>
        ''' <value>The reference object entry.</value>
        Public Property ReferenceObjectEntry() As String
            Get
                Return Me._ReferenceObjectEntry
            End Get
            Set(value As String)
                Me._ReferenceObjectEntry = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueReferenceObjectEntry As Boolean
            Get
                Return _ReferenceObjectEntry IsNot Nothing AndAlso _ReferenceObjectEntry <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the reference table entry. Has the form [tablename].[columnname] 
        ''' such as Deliverable.constTableID & "." & deliverable.constFNUID
        ''' </summary>
        ''' <value>The reference object entry.</value>
        'Public Property ReferenceTableEntry() As String
        '    Get
        '        Return Me._ReferenceTableEntry
        '    End Get
        '    Set(value As String)
        '        Me._ReferenceTableEntry = value.ToUpper
        '    End Set
        'End Property
        'Public ReadOnly Property HasValueTableEntry As Boolean
        '    Get
        '        Return _ReferenceTableEntry IsNot Nothing AndAlso _ReferenceTableEntry = ""
        '    End Get
        'End Property
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the pos ordinal.
        ''' </summary>
        ''' <value>The pos ordinal.</value>
        Public Property Posordinal() As UShort
            Get
                Return Me._Posordinal
            End Get
            Set(value As UShort)
                Me._Posordinal = value
            End Set
        End Property

        Public ReadOnly Property HasValuePosOrdinal As Boolean
            Get
                Return _Posordinal.HasValue
            End Get
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
        Public ReadOnly Property HasValueDefaultValue As Boolean
            Get
                Return _DefaultValue IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Property Tablename() As String
            Get
                Return Me._TableID
            End Get
            Set(value As String)
                Me._TableID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableName As Boolean
            Get
                Return _TableID IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueTypeID As Boolean
            Get
                Return _Typeid.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the inner typeid of list.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public Property InnerTypeid() As otFieldDataType
            Get
                Return Me._InnerTypeID
            End Get
            Set(value As otFieldDataType)
                Me._InnerTypeID = value
            End Set
        End Property
        Public ReadOnly Property HasValueInnerTypeID As Boolean
            Get
                Return _InnerTypeID.HasValue
            End Get
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
        Public ReadOnly Property HasValueSize As Boolean
            Get
                Return _size.HasValue
            End Get
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
        Public ReadOnly Property HasValueParameter() As Boolean
            Get
                Return _Parameter IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueIsNullable()
            Get
                Return _IsNullable.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the Unique Property.
        ''' </summary>
        ''' <value></value>
        Public Property IsUnique() As Boolean
            Get
                Return Me._IsUnique
            End Get
            Set(value As Boolean)
                Me._IsUnique = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsUnique()
            Get
                Return _IsUnique.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is foreign Key flag. References must be set
        ''' </summary>
        ''' <value></value>
        Public Property UseForeignKey() As otForeignKeyImplementation
            Get
                Return Me._UseForeignKey
            End Get
            Set(value As otForeignKeyImplementation)
                Me._UseForeignKey = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseForeignKey()
            Get
                Return _UseForeignKey.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key reference.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property ForeignKeyReferences() As String()
            Get
                Return Me._ForeignKeyReference
            End Get
            Set(value As String())
                Me._ForeignKeyReference = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyReferences As Boolean
            Get
                Return _ForeignKeyReference IsNot Nothing AndAlso _ForeignKeyReference.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key properties as string
        ''' </summary>
        ''' <value>string</value>
        Public Property ForeignKeyProperties() As String()
            Get
                Dim aList As New List(Of String)
                For Each aP In _ForeignKeyProperties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of ForeignKeyProperty)
                    For Each aValue In value
                        aList.Add(New ForeignKeyProperty(aValue))
                    Next
                    Me._ForeignKeyProperties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormSchemaTableColumnAttribute.ForeignKeyProperties")
                End Try
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the foreign key properties as list of ForeignKeyProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeignKeyProperty As ForeignKeyProperty()
            Get
                Return _ForeignKeyProperties
            End Get
            Set(value As ForeignKeyProperty())
                _ForeignKeyProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyProperties As Boolean
            Get
                Return _ForeignKeyProperties IsNot Nothing AndAlso _ForeignKeyProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Public Property PrimaryKeyOrdinal() As Short
            Get
                Return Me._primaryKeyOrdinal
            End Get
            Set(value As Short)
                If value > 0 Then
                    Me._primaryKeyOrdinal = value
                Else
                    CoreMessageHandler(message:="position index is less or equal 0", arg1:=value, subname:="ormSchemaColumn.PrimaryKeyordinal", messagetype:=otCoreMessageType.InternalError)
                    Debug.Assert(False)
                End If

            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKeyOrdinal As Boolean
            Get
                Return _primaryKeyOrdinal.HasValue
            End Get
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
        Public ReadOnly Property HasValueRelation As Boolean
            Get
                Return _relation IsNot Nothing AndAlso _relation.Count > 0
            End Get
        End Property

        ''' <summary>
        ''' gets or sets the version counter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version As UShort
            Get
                Return Me._Version
            End Get
            Set(value As UShort)
                Me._Version = value
            End Set
        End Property
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property

    End Class
    ''' <summary>
    ''' Attribute for Const fields to describe foreign keys with multiple keys
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormSchemaForeignKeyAttribute
        Inherits Attribute
        Private _ID As String
        Private _TableID As String = Nothing
        Private _ObjectID As String = Nothing
        Private _Version As Nullable(Of UShort)
        Private _UseForeignKey As Nullable(Of otForeignKeyImplementation) = otForeignKeyImplementation.None
        Private _ForeignKeyReferences As String() = {}
        Private _ForeignKeyProperties As ForeignKeyProperty()
        Private _Entrynames As String() = {}
        Private _Description As String = Nothing
        ''' <summary>
        ''' Gets or sets the name of the column.
        ''' </summary>
        ''' <value>The name of the column.</value>
        Public Property Entrynames() As String()
            Get
                Return Me._Entrynames
            End Get
            Set(value As String())
                For i = 0 To value.Count - 1
                    value(i) = value(i).ToUpper
                Next
                Me._Entrynames = value
            End Set
        End Property
        Public ReadOnly Property HasValueEntrynames As Boolean
            Get
                Return _Entrynames IsNot Nothing AndAlso _Entrynames.Count > 0
            End Get
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the ID
        ''' </summary>
        ''' <value>The description.</value>
        Public Property ID As String
            Get
                Return Me._ID
            End Get
            Set(value As String)
                Me._ID = value
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing
            End Get
        End Property
       
        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Property Tablename() As String
            Get
                Return Me._TableID
            End Get
            Set(value As String)
                Me._TableID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableName As Boolean
            Get
                Return _TableID IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Property ObjectID() As String
            Get
                Return Me._ObjectID
            End Get
            Set(value As String)
                Me._ObjectID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueObjectID As Boolean
            Get
                Return _ObjectID IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is foreign Key flag. References must be set
        ''' </summary>
        ''' <value></value>
        Public Property UseForeignKey() As otForeignKeyImplementation
            Get
                Return Me._UseForeignKey
            End Get
            Set(value As otForeignKeyImplementation)
                Me._UseForeignKey = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseForeignKey()
            Get
                Return _UseForeignKey.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key reference.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property ForeignKeyReferences() As String()
            Get
                Return Me._ForeignKeyReferences
            End Get
            Set(value As String())
                For i = 0 To value.Count - 1
                    value(i) = value(i).ToUpper
                Next
                Me._ForeignKeyReferences = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyReferences As Boolean
            Get
                Return _ForeignKeyReferences IsNot Nothing AndAlso _ForeignKeyReferences.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key properties as string
        ''' </summary>
        ''' <value>string</value>
        Public Property ForeignKeyProperties() As String()
            Get
                Dim aList As New List(Of String)
                For Each aP In _ForeignKeyProperties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of ForeignKeyProperty)
                    For Each aValue In value
                        aList.Add(New ForeignKeyProperty(aValue))
                    Next
                    Me._ForeignKeyProperties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormSchemaTableColumnAttribute.ForeignKeyProperties")
                End Try
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the foreign key properties as list of ForeignKeyProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeignKeyProperty As ForeignKeyProperty()
            Get
                Return _ForeignKeyProperties
            End Get
            Set(value As ForeignKeyProperty())
                _ForeignKeyProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyProperties As Boolean
            Get
                Return _ForeignKeyProperties IsNot Nothing AndAlso _ForeignKeyProperties.Count > 0
            End Get
        End Property
        
        ''' <summary>
        ''' gets or sets the version counter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version As UShort
            Get
                Return Me._Version
            End Get
            Set(value As UShort)
                Me._Version = value
            End Set
        End Property
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property

    End Class
    ''' <summary>
    ''' Attribute for Object Entry fields to describe the schema
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormObjectEntryAttribute
        Inherits ormSchemaTableColumnAttribute



        Private _Title As String = Nothing
        Private _EntryType As Nullable(Of otObjectEntryDefinitiontype) = otObjectEntryDefinitiontype.Column

        Private _Parameter As String = Nothing
        Private _KeyOrdinal As Nullable(Of UShort)
        Private _DefaultValue As String = Nothing
        Private _Version As Nullable(Of UShort)
        Private _Posordinal As Nullable(Of UShort)
        Private _SpareFieldTag As Nullable(Of Boolean)
        Private _aliases() As String = Nothing
        Private _relation() As String = Nothing

        Private _objectEntryName As String = Nothing
        Private _objectName As String = Nothing
        Private _properties As ObjectEntryProperty()

        Private _validate As Nullable(Of Boolean)
        Private _LowerRange As String = Nothing
        Private _upperRange As String = Nothing
        Private _Values As String()
        Private _lookupCondition As String = Nothing
        Private _ValidationProperties As String()
        Private _validateRegExp As String = Nothing

        Private _render As Nullable(Of Boolean)
        Private _RenderProperties As String()
        Private _RenderRegExpMatch As String
        Private _RenderRegExpPattern As String
        ''' <summary>
        ''' Gets or sets the type of the entry.
        ''' </summary>
        ''' <value>The type of the entry.</value>
        Public Property EntryType() As otObjectEntryDefinitiontype
            Get
                Return Me._EntryType
            End Get
            Set
                Me._EntryType = Value
            End Set
        End Property
        Public ReadOnly Property HasValueEntryType As Boolean
            Get
                Return _EntryType.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the lookup condition.
        ''' </summary>
        ''' <value>The lookup condition.</value>
        Public Property LookupCondition() As String
            Get
                Return Me._lookupCondition
            End Get
            Set
                Me._lookupCondition = Value
            End Set
        End Property
        Public ReadOnly Property HasValueLookupCondition As Boolean
            Get
                Return _lookupCondition IsNot Nothing 'AndAlso _validateRegExp <> "" empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render reg exp pattern.
        ''' </summary>
        ''' <value>The render reg exp pattern.</value>
        Public Property RenderRegExpPattern() As String
            Get
                Return Me._RenderRegExpPattern
            End Get
            Set
                Me._RenderRegExpPattern = Value
            End Set
        End Property
        Public ReadOnly Property HasValueRenderRegExpPattern As Boolean
            Get
                Return _RenderRegExpPattern IsNot Nothing 'AndAlso _validateRegExp <> "" empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render reg exp match.
        ''' </summary>
        ''' <value>The render reg exp match.</value>
        Public Property RenderRegExpMatch() As String
            Get
                Return Me._RenderRegExpMatch
            End Get
            Set
                Me._RenderRegExpMatch = Value
            End Set
        End Property
        Public ReadOnly Property HasValueRenderRegExpMatch As Boolean
            Get
                Return _RenderRegExpMatch IsNot Nothing 'AndAlso _validateRegExp <> "" empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object entry properties.
        ''' </summary>
        ''' <value>The render properties.</value>
        Public Property Properties() As String()
            Get
                Dim aList As New List(Of String)
                For Each aP In _properties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of ObjectEntryProperty)
                    For Each aValue In value
                        aList.Add(New ObjectEntryProperty(aValue))
                    Next
                    Me._properties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, subname:="ormObjectEntryAttribute.Properties")
                End Try
            End Set
        End Property
        Public Property ObjectEntryProperties As ObjectEntryProperty()
            Get
                Return _properties
            End Get
            Set(value As ObjectEntryProperty())
                _properties = value
            End Set
        End Property
        Public ReadOnly Property HasValueProperties As Boolean
            Get
                Return _properties IsNot Nothing AndAlso _properties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render properties.
        ''' </summary>
        ''' <value>The render properties.</value>
        Public Property RenderProperties() As String()
            Get
                Return Me._RenderProperties
            End Get
            Set
                Me._RenderProperties = Value
            End Set
        End Property
        Public ReadOnly Property HasValueRenderProperties As Boolean
            Get
                Return _RenderProperties IsNot Nothing AndAlso _RenderProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render.
        ''' </summary>
        ''' <value>The render.</value>
        Public Property Render() As Boolean?
            Get
                Return Me._render
            End Get
            Set
                Me._render = Value
            End Set
        End Property
        Public ReadOnly Property HasValueRender As Boolean
            Get
                Return _render.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validate reg exp.
        ''' </summary>
        ''' <value>The validate reg exp.</value>
        Public Property ValidateRegExp() As String
            Get
                Return Me._validateRegExp
            End Get
            Set
                Me._validateRegExp = Value
            End Set
        End Property
        Public ReadOnly Property HasValueValidateRegExp As Boolean
            Get
                Return _validateRegExp IsNot Nothing 'AndAlso _validateRegExp <> "" empty is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validation properties.
        ''' </summary>
        ''' <value>The validation properties.</value>
        Public Property ValidationProperties() As String()
            Get
                Return Me._ValidationProperties
            End Get
            Set
                Me._ValidationProperties = Value
            End Set
        End Property
        Public ReadOnly Property HasValueValidationproperties As Boolean
            Get
                Return _ValidationProperties IsNot Nothing AndAlso _ValidationProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the values.
        ''' </summary>
        ''' <value>The values.</value>
        Public Property Values() As String()
            Get
                Return Me._Values
            End Get
            Set
                Me._Values = Value
            End Set
        End Property
        Public ReadOnly Property HasValueValues As Boolean
            Get
                Return _Values IsNot Nothing AndAlso _Values.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the upper range.
        ''' </summary>
        ''' <value>The upper range.</value>
        Public Property UpperRange() As String
            Get
                Return Me._upperRange
            End Get
            Set
                Me._upperRange = Value
            End Set
        End Property
        Public ReadOnly Property HasValueUpperRange As Boolean
            Get
                Return _upperRange IsNot Nothing AndAlso _upperRange <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the lower range.
        ''' </summary>
        ''' <value>The lower range.</value>
        Public Property LowerRange() As String
            Get
                Return Me._LowerRange
            End Get
            Set
                Me._LowerRange = Value
            End Set
        End Property
        Public ReadOnly Property HasValueLowerRange As Boolean
            Get
                Return _LowerRange IsNot Nothing AndAlso _LowerRange <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validate.
        ''' </summary>
        ''' <value>The validate.</value>
        Public Property Validate() As Boolean?
            Get
                Return Me._validate
            End Get
            Set
                Me._validate = Value
            End Set
        End Property
        Public ReadOnly Property HasValueValidate As Boolean
            Get
                Return _validate.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the object.
        ''' </summary>
        ''' <value>The name of the object.</value>
        Public Property ObjectName() As String
            Get
                Return Me._objectName
            End Get
            Set(value As String)
                Me._objectName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueObjectName As Boolean
            Get
                Return _objectName IsNot Nothing AndAlso _objectName <> ""
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the column.
        ''' </summary>
        ''' <value>The name of the column.</value>
        Public Property EntryName() As String
            Get
                Return Me._objectEntryName
            End Get
            Set(value As String)
                Me._objectEntryName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueEntryName As Boolean
            Get
                Return _objectEntryName IsNot Nothing AndAlso _objectEntryName <> ""
            End Get
        End Property
       
        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Public Property KeyOrdinal() As Short
            Get
                Return Me._KeyOrdinal
            End Get
            Set(value As Short)
                If value > 0 Then
                    Me._KeyOrdinal = value
                Else
                    CoreMessageHandler(message:="position index is less or equal 0", arg1:=value, subname:="ormObjectEntry.Keyordinal", messagetype:=otCoreMessageType.InternalError)
                    Debug.Assert(False)
                End If

            End Set
        End Property
        Public ReadOnly Property HasValueKeyOrdinal As Boolean
            Get
                Return _KeyOrdinal.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the pos ordinal.
        ''' </summary>
        ''' <value>The pos ordinal.</value>
        Public Property Posordinal() As UShort
            Get
                Return Me._Posordinal
            End Get
            Set(value As UShort)
                Me._Posordinal = value
            End Set
        End Property

        Public ReadOnly Property hasValuePosOrdinal As Boolean
            Get
                Return _Posordinal.HasValue
            End Get
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
        Public ReadOnly Property HasValueSpareFieldTag As Boolean
            Get
                Return _SpareFieldTag.HasValue
            End Get
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
        Public ReadOnly Property HasValueTitle As Boolean
            Get
                Return _Title IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the parameter.
        ''' </summary>
        ''' <value>The parameter.</value>
        'Public Property Parameter() As String
        '    Get
        '        Return Me._Parameter
        '    End Get
        '    Set(value As String)
        '        Me._Parameter = value
        '    End Set
        'End Property
        'Public ReadOnly Property HasValueParameter() As Boolean
        '    Get
        '        Return _Parameter IsNot Nothing
        '    End Get
        'End Property

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
        Public ReadOnly Property HasValueRelation As Boolean
            Get
                Return _relation IsNot Nothing AndAlso _relation.Count > 0
            End Get
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
        Public ReadOnly Property HasValueAliases As Boolean
            Get
                Return _aliases IsNot Nothing AndAlso _aliases.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' gets or sets the version counter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Version As UShort
            Get
                Return Me._Version
            End Get
            Set(value As UShort)
                Me._Version = value
            End Set
        End Property
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property
        ''' <summary>
        ''' returns a String presentation of an ObjEctEntry Attribute
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToString() As String
            Dim name As String = Me.GetType.Name & "[" & Me.ObjectName & "." & Me.EntryName
            If Me.HasValueReferenceObjectEntry Then
                name &= "{" & Me.ReferenceObjectEntry & "}"
            End If
            name &= "]"
            Return name
        End Function
    End Class
    ''' <summary>
    ''' Attribute for Const fields to describe the schema
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormObjectAttribute
        Inherits Attribute
        Private _ID As String = Nothing
        Private _ClassName As String = Nothing
        Private _Tablenames As String()
        Private _Title As String = Nothing
        Private _Description As String = Nothing
        Private _Version As Nullable(Of UShort) = 1
        Private _Properties As String()

        Private _DeleteFieldFlag As Nullable(Of Boolean) = False
        Private _SpareFieldsFlag As Nullable(Of Boolean) = False
        Private _AddDomainBehaviorFlag As Nullable(Of Boolean) = False
        Private _Modulename As String = Nothing
        Private _IsActive As Nullable(Of Boolean) = True
        Private _PrimaryKeys As String()
        Private _isBootstrapObject As Nullable(Of Boolean) = False
        Private _useCache As Nullable(Of Boolean)
        Private _defaultPermission As Nullable(Of Boolean) = True
        Private _CacheProperties As String()
        ''' <summary>
        ''' Gets or sets the primary keys.
        ''' </summary>
        ''' <value>The primary keys.</value>
        Public Property PrimaryKeys() As String()
            Get
                Return Me._PrimaryKeys
            End Get
            Set(value As String())
                For Each s In value
                    If s IsNot Nothing Then s = s.ToUpper
                Next
                Me._PrimaryKeys = value
            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKeys As Boolean
            Get
                Return _PrimaryKeys IsNot Nothing AndAlso _PrimaryKeys.Count > 0
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property IsActive() As Boolean
            Get
                Return Me._IsActive
            End Get
            Set(value As Boolean)
                Me._IsActive = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsActive As Boolean
            Get
                Return _IsActive.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property DefaultPermission() As Boolean
            Get
                Return Me._defaultPermission
            End Get
            Set(value As Boolean)
                Me._defaultPermission = value
            End Set
        End Property
        Public ReadOnly Property HasValueDefaultPermission As Boolean
            Get
                Return _defaultPermission.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object Properties
        ''' </summary>
        ''' <value>cache.</value>
        Public Property Properties() As String()
            Get
                Return Me._Properties
            End Get
            Set(value As String())
                Me._Properties = value
            End Set
        End Property
        Public ReadOnly Property HasValueProperties As Boolean
            Get
                Return _Properties IsNot Nothing AndAlso _Properties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets bootstrap object flag.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property IsBootstrap() As Boolean
            Get
                Return Me._isBootstrapObject
            End Get
            Set(value As Boolean)
                Me._isBootstrapObject = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsBootstap As Boolean
            Get
                Return _isBootstrapObject.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cache is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property UseCache() As Boolean
            Get
                Return Me._useCache
            End Get
            Set(value As Boolean)
                Me._useCache = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseCache As Boolean
            Get
                Return _useCache.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cache select.
        ''' </summary>
        ''' <value>cache.</value>
        Public Property CacheProperties() As String()
            Get
                Return Me._CacheProperties
            End Get
            Set(value As String())
                Me._CacheProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueCacheProperties As Boolean
            Get
                Return _CacheProperties IsNot Nothing AndAlso _CacheProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the modulename.
        ''' </summary>
        ''' <value>The modulename.</value>
        Public Property Modulename() As String
            Get
                Return Me._Modulename
            End Get
            Set(value As String)
                Me._Modulename = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueModulename As Boolean
            Get
                Return _Modulename IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the add domain behavior flag.
        ''' </summary>
        ''' <value>The add domain behavior flag.</value>
        Public Property AddDomainBehaviorFlag() As Boolean
            Get
                Return Me._AddDomainBehaviorFlag
            End Get
            Set(value As Boolean)
                Me._AddDomainBehaviorFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueDomainBehavior As Boolean
            Get
                Return _AddDomainBehaviorFlag.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the spare fields flag.
        ''' </summary>
        ''' <value>The spare fields flag.</value>
        Public Property SpareFieldsFlag() As Boolean
            Get
                Return Me._SpareFieldsFlag
            End Get
            Set(value As Boolean)
                Me._SpareFieldsFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueSpareFields As Boolean
            Get
                Return _SpareFieldsFlag.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the delete field flag.
        ''' </summary>
        ''' <value>The delete field flag.</value>
        Public Property DeleteFieldFlag() As Boolean
            Get
                Return Me._DeleteFieldFlag
            End Get
            Set(value As Boolean)
                Me._DeleteFieldFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueDeleteField As Boolean
            Get
                Return _DeleteFieldFlag.HasValue
            End Get
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueTitle As Boolean
            Get
                Return _Title IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the tablenames.
        ''' </summary>
        ''' <value>The tablenames.</value>
        Public Property Tablenames() As String()
            Get
                Return Me._Tablenames
            End Get
            Set(value As String())
                For Each s In value
                    s = s.ToUpper
                Next
                Me._Tablenames = value
            End Set
        End Property
        Public ReadOnly Property HasValueTablenames As Boolean
            Get
                Return _Tablenames IsNot Nothing AndAlso _Tablenames.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the class.
        ''' </summary>
        ''' <value>The name of the class.</value>
        Public Property ClassName() As String
            Get
                Return Me._ClassName
            End Get
            Set(value As String)
                Me._ClassName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueClassname As Boolean
            Get
                Return _ClassName IsNot Nothing AndAlso _ClassName <> """"
            End Get
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
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing AndAlso _ID <> ""
            End Get
        End Property
    End Class
    ''' <summary>
    ''' Attribute for Const fields to describe an object operation
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormObjectOperationAttribute
        Inherits Attribute
        Private _ID As String = Nothing
        Private _OperationName As String = Nothing
        Private _Title As String = Nothing
        Private _Description As String = Nothing
        Private _Version As Nullable(Of UShort) = 1
        Private _PermissionRules As String()
        Private _DefaultAllowPermission As Nullable(Of Boolean) = True

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property OperationName As String
            Get
                Return Me._OperationName
            End Get
            Set(value As String)
                Me._OperationName = value
            End Set
        End Property
        Public ReadOnly Property HasValueOperationName As Boolean
            Get
                Return _OperationName IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets bootstrap object flag.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property DefaultAllowPermission() As Boolean
            Get
                Return Me._DefaultAllowPermission
            End Get
            Set(value As Boolean)
                Me._DefaultAllowPermission = value
            End Set
        End Property
        Public ReadOnly Property HasValueDefaultAllowPermission As Boolean
            Get
                Return _DefaultAllowPermission.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the object Properties
        ''' </summary>
        ''' <value>cache.</value>
        Public Property PermissionRules() As String()
            Get
                Return Me._PermissionRules
            End Get
            Set(value As String())
                Me._PermissionRules = value
            End Set
        End Property
        Public ReadOnly Property HasValuePermissionRules As Boolean
            Get
                Return _PermissionRules IsNot Nothing AndAlso _PermissionRules.Count > 0
            End Get
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueTitle As Boolean
            Get
                Return _Title IsNot Nothing
            End Get
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
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing AndAlso _ID <> ""
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Attribute for Const fields to describe an object operation method - connects the opeation to different methods in the class
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)> _
    Public Class ormObjectOperationMethodAttribute
        Inherits Attribute
        Private _ID As String = Nothing
        Private _OperationName As String = Nothing
        Private _Version As Nullable(Of ULong)
        Private _Description As String = Nothing
        Private _Title As String = Nothing
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
        Public ReadOnly Property HasValueVersion As Boolean
            Get
                Return _Version.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property OperationName As String
            Get
                Return Me._OperationName
            End Get
            Set(value As String)
                Me._OperationName = value
            End Set
        End Property
        Public ReadOnly Property HasValueOperationName As Boolean
            Get
                Return _OperationName IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueDescription As Boolean
            Get
                Return _Description IsNot Nothing
            End Get
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
        Public ReadOnly Property HasValueTitle As Boolean
            Get
                Return _Title IsNot Nothing
            End Get
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
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean
            Get
                Return _ID IsNot Nothing AndAlso _ID <> ""
            End Get
        End Property
    End Class

End Namespace
