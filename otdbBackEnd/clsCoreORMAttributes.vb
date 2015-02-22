
REM ***********************************************************************************************************************************************''' <summary>''' <summary>
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
    ''' ChangeLogEntryAttribute implements a ChangeLogEntry for a Class
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormDatabaseDriverAttribute
        Inherits Attribute

        Private _version As Long?
        Private _description As String
        Private _autoinstance As Boolean?
        Private _id As String


        ''' <summary>
        ''' Gets or sets the id.
        ''' </summary>
        ''' <value>The id.</value>
        Public Property Id() As String
            Get
                Return _id
            End Get
            Set(value As String)
                _id = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the Automatic Singelton Instancing flag.
        ''' </summary>
        ''' <value>The autoinstance.</value>
        Public Property AutoInstance As Boolean
            Get
                Return _autoinstance
            End Get
            Set(value As Boolean)
                _autoinstance = value
            End Set
        End Property
        Public ReadOnly Property HasValueAutoInstance As Boolean
            Get
                Return _autoinstance.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                Me._description = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property Version() As Long
            Get
                If Not _version.HasValue Then _version = 1
                Return Me._version
            End Get
            Set(value As Long)
                Me._version = value
            End Set
        End Property


    End Class
    ''' <summary>
    ''' ChangeLogEntryAttribute implements a ChangeLogEntry for a Class
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Field Or AttributeTargets.Module Or AttributeTargets.Method, _
        AllowMultiple:=True, Inherited:=False)> _
    Public Class ormChangeLogEntry
        Inherits Attribute

        Private _application As String
        Private _module As String
        Private _version As Long?
        Private _release As Long?
        Private _patch As Long?

        Private _changeimplno As Long?
        Private _releasedate As DateTime?
        Private _changeID As String
        Private _description As String


        ''' <summary>
        ''' Gets or sets the changeimplno.
        ''' </summary>
        ''' <value>The changeimplno.</value>
        Public Property Changeimplno() As Long
            Get
                If Not _changeimplno.HasValue Then _changeimplno = 1

                Return Me._changeimplno
            End Get
            Set(value As Long)
                Me._changeimplno = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String
            Get
                Return Me._description
            End Get
            Set(value As String)
                Me._description = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the change ID.
        ''' </summary>
        ''' <value>The change ID.</value>
        Public Property ChangeID() As String
            Get
                Return Me._changeID
            End Get
            Set(value As String)
                Me._changeID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the releasedate.
        ''' </summary>
        ''' <value>The releasedate.</value>
        Public Property Releasedate() As DateTime
            Get
                If Not _releasedate.HasValue Then _releasedate = DateTime.Now
                Return _releasedate
            End Get
            Set(value As DateTime)
                Me._releasedate = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the patch.
        ''' </summary>
        ''' <value>The patch.</value>
        Public Property Patch() As Long
            Get
                If Not _patch.HasValue Then _patch = 0
                Return Me._patch
            End Get
            Set(value As Long)
                Me._patch = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the release.
        ''' </summary>
        ''' <value>The release.</value>
        Public Property Release() As Long
            Get
                If Not _release.HasValue Then _release = 1
                Return Me._release
            End Get
            Set(value As Long)
                Me._release = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property Version() As Long
            Get
                If Not _version.HasValue Then _version = 1
                Return Me._version
            End Get
            Set(value As Long)
                Me._version = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the module.
        ''' </summary>
        ''' <value>The module.</value>
        Public Property [Module]() As String
            Get
                Return Me._module
            End Get
            Set(value As String)
                Me._module = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the application.
        ''' </summary>
        ''' <value>The application.</value>
        Public Property Application() As String
            Get
                Return Me._application
            End Get
            Set(value As String)
                Me._application = value
            End Set
        End Property

    End Class


    ''' <summary>
    ''' OTDBDataObject Attribute links a class variable to a datastore table and field
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=True, Inherited:=True)> _
    Public Class ormObjectEntryMapping
        Inherits Attribute

        Private _ID As String
        Private _entryname As String 'Object Entry Name
        Private _columnname As String ' table column optional
        Private _tableID As String ' table name optional
        Private _relationName As String '** if a relation definition is used
        Private _keyentries As String() ' name of the entries for keys (if the datastructure has a key such as dictionary)
        Private _InfuseMode As Nullable(Of otInfuseMode)
        Private _enabled As Boolean = True

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the infuse mode.
        ''' </summary>
        ''' <value>The infuse mode.</value>
        Public Property InfuseMode() As otInfuseMode
            Get
                Return Me._InfuseMode
            End Get
            Set(value As otInfuseMode)
                Me._InfuseMode = value
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
                Return _relationName IsNot Nothing AndAlso _relationName <> String.empty
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
                Return _ID IsNot Nothing AndAlso _ID <> String.empty
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
                Me._tableID = UCase(value)
            End Set
        End Property
        Public ReadOnly Property HasValueTablename As Boolean
            Get
                Return _tableID IsNot Nothing AndAlso _tableID <> String.empty
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
                Return _entryname IsNot Nothing AndAlso _entryname <> String.empty
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
                Return _columnname IsNot Nothing AndAlso _columnname <> String.empty
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
        Private _ID As String = String.empty
        Private _fieldname As String = String.empty
        Private _tableID As String = String.empty

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
    ''' abstract Attribute Class for a data container
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public MustInherit Class ormContainerAttribute
        Inherits Attribute
        Implements iormContainerAttribute

        Protected _ID As String
        Protected _Version As Nullable(Of UShort) = 1 'needed for checksum
        Protected _DeleteFieldFlag As Nullable(Of Boolean)
        Protected _SpareFieldsFlag As Nullable(Of Boolean)
        Protected _AddDomainBehaviorFlag As Nullable(Of Boolean)
        Protected _ContainerID As String
        Protected _ObjectID As String
        Protected _Description As String = String.Empty
        Protected _PrimaryKeyName As String
        Protected _CacheProperties As String()
        Protected _useCache As Nullable(Of Boolean)
        Protected _enabled As Boolean = True
        Protected _DataBaseDriverID As String
        Protected _containertype As otContainerType?

        '** dynamic
        Protected _entries As New Dictionary(Of String, ormContainerEntryAttribute)
        Protected _primaryEntries As New SortedList(Of UShort, String)
        Private _indices As New Dictionary(Of String, ormIndexAttribute)
        Private _foreignkeys As New Dictionary(Of String, ormForeignKeyAttribute)

        ''' <summary>
        '''  construcotr
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

        End Sub
        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean Implements iormContainerAttribute.Enabled
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the container type
        ''' </summary>
        ''' <value>The is active.</value>
        Public Overridable Property Containertype As otContainerType Implements iormContainerAttribute.ContainerType
            Get
                Return Me._containertype
            End Get
            Set(value As otContainerType)
                Me._containertype = value
            End Set
        End Property
        Public ReadOnly Property HasValueContainerType As Boolean Implements iormContainerAttribute.HasValueContainerType
            Get
                Return _containertype.HasValue
            End Get
        End Property


        ''' <summary>
        ''' Gets or sets the cache is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property UseCache() As Boolean Implements iormContainerAttribute.UseCache
            Get
                Return Me._useCache
            End Get
            Set(value As Boolean)
                Me._useCache = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseCache As Boolean Implements iormContainerAttribute.HasValueUseCache
            Get
                Return _useCache.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the cache select.
        ''' </summary>
        ''' <value>cache.</value>
        Public Property CacheProperties() As String() Implements iormContainerAttribute.CacheProperties
            Get
                Return Me._CacheProperties
            End Get
            Set(value As String())
                Me._CacheProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueCacheProperties As Boolean Implements iormContainerAttribute.HasValueCacheProperties
            Get
                Return _CacheProperties IsNot Nothing AndAlso _CacheProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Add a member
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function AddEntry(member As iormContainerEntryAttribute) As Boolean Implements iormContainerAttribute.AddEntry
            If _entries.ContainsKey(member.ContainerEntryName.ToUpper) Then
                _entries.Remove(member.ContainerEntryName.ToUpper)
            End If
            _entries.Add(key:=member.ContainerEntryName.ToUpper, value:=member)
            If member.HasValuePrimaryKeyOrdinal Then
                If _primaryEntries.ContainsKey(member.PrimaryEntryOrdinal) Then _primaryEntries.Remove(member.PrimaryEntryOrdinal)
                _primaryEntries.Add(key:=member.PrimaryEntryOrdinal, value:=member.ContainerEntryName)
            End If
            Return True
        End Function
        ''' <summary>
        ''' Add an entry by TabeColumn
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function UpdateEntry(member As iormContainerEntryAttribute) As Boolean Implements iormContainerAttribute.UpdateEntry
            If _entries.ContainsKey(member.ContainerEntryName.ToUpper) Then
                _entries.Remove(member.ContainerEntryName.ToUpper)
            End If
            _entries.Add(key:=member.ContainerEntryName.ToUpper, value:=member)
            If member.HasValuePrimaryKeyOrdinal Then
                If _primaryEntries.ContainsKey(member.PrimaryEntryOrdinal) Then _primaryEntries.Remove(member.PrimaryEntryOrdinal)
                _primaryEntries.Add(key:=member.PrimaryEntryOrdinal, value:=member.ContainerEntryName)
            Else
                If _primaryEntries.Values.Contains(member.ContainerEntryName) Then
                    _primaryEntries.Remove(_primaryEntries.First(Function(x) x.Key = member.ContainerEntryName).Key)
                End If
            End If
            Return True
        End Function
        ''' <summary>
        ''' returns an entry by member name or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GetEntry(name As String, Optional onlyenabled As Boolean = True) As iormContainerEntryAttribute Implements iormContainerAttribute.GetEntry
            If _entries.ContainsKey(name.ToUpper) Then
                Dim anAttribute As iormContainerEntryAttribute = _entries.Item(key:=name.ToUpper)
                If onlyenabled AndAlso Not anAttribute.Enabled Then Return Nothing
                Return anAttribute
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns an member by member name or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function HasEntry(name As String, Optional onlyenabled As Boolean = Nothing) As Boolean Implements iormContainerAttribute.HasEntry
            Dim result As Boolean = _entries.ContainsKey(name.ToUpper)
            If onlyenabled AndAlso result Then
                result = _entries.Item(name.ToUpper).Enabled
            End If
            Return result
        End Function
        ''' <summary>
        ''' remove a member by name 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function RemoveEntry(membername As String) As Boolean Implements iormContainerAttribute.RemoveEntry
            If _entries.ContainsKey(membername.ToUpper) Then
                _entries.Remove(membername.ToUpper)
                If _primaryEntries.Values.Contains(membername) Then
                    _primaryEntries.Remove(_primaryEntries.First(Function(x) x.Key = membername).Key)
                End If
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
        Public ReadOnly Property EntryAttributes As IEnumerable(Of iormContainerEntryAttribute) Implements iormContainerAttribute.EntryAttributes
            Get
                Return _entries.Values.Where(Function(x) x.Enabled = True).ToList
            End Get
        End Property

        ''' <summary>
        ''' sets or returns the Names of the PrimaryKey Columns
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PrimaryEntryNames As String() Implements iormContainerAttribute.PrimaryEntryNames
            Get
                Return _primaryEntries.Values.ToArray
            End Get
            Set(value As String())
                _primaryEntries.Clear()

                For i = value.GetLowerBound(0) To value.GetUpperBound(0)
                    _primaryEntries.Add(key:=i, value:=value(i))
                Next

            End Set
        End Property
        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property EntryNames As IEnumerable(Of String) Implements iormContainerAttribute.EntryNames
            Get
                Return _entries.Values.Where(Function(x) x.Enabled = True).SelectMany(Function(x) x.ContainerEntryName).ToList
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String Implements iormContainerAttribute.Description
            Get
                Return Me._Description
            End Get
            Set(value As String)
                Me._Description = value
            End Set
        End Property
        Public ReadOnly Property HasValueDescription As Boolean Implements iormContainerAttribute.HasValueDescription
            Get
                Return _Description IsNot Nothing AndAlso _Description <> String.Empty
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the Primary key Name.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property PrimaryKey() As String Implements iormContainerAttribute.PrimaryKey
            Get
                Return Me._PrimaryKeyName
            End Get
            Set(value As String)
                Me._PrimaryKeyName = value
            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKey As Boolean Implements iormContainerAttribute.HasValuePrimaryKey
            Get
                Return Not String.IsNullOrWhiteSpace(_PrimaryKeyName)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object ID.
        ''' </summary>
        ''' <value>The object ID.</value>
        Public Property ObjectID() As String Implements iormContainerAttribute.ObjectID
            Get
                Return Me._ObjectID
            End Get
            Set(value As String)
                Me._ObjectID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueObjectID As Boolean Implements iormContainerAttribute.HasValueObjectID
            Get
                Return Not String.IsNullOrWhiteSpace(_ObjectID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object ID.
        ''' </summary>
        ''' <value>The object ID.</value>
        Public Property DatabaseDriverID() As String Implements iormContainerAttribute.DatabaseDriverID
            Get
                Return Me._DataBaseDriverID
            End Get
            Set(value As String)
                Me._DataBaseDriverID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueDatabaseDriverID As Boolean Implements iormContainerAttribute.HasValueDatabaseDriverID
            Get
                Return Not String.IsNullOrWhiteSpace(_DataBaseDriverID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the unique name of the container (such as tables).
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property ContainerID() As String Implements iormContainerAttribute.ContainerID
            Get
                Return Me._ContainerID
            End Get
            Set(value As String)
                Me._ContainerID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueContainerID As Boolean Implements iormContainerAttribute.HasValueContainerID
            Get
                Return Not String.IsNullOrWhiteSpace(_ContainerID)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the add domain ID flag.
        ''' </summary>
        ''' <value>The add domain ID flag.</value>
        Public Property AddDomainBehavior() As Boolean Implements iormContainerAttribute.AddDomainBehavior
            Get
                Return Me._AddDomainBehaviorFlag
            End Get
            Set(value As Boolean)
                Me._AddDomainBehaviorFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueAddDomainBehavior As Boolean Implements iormContainerAttribute.HasValueAddDomainBehavior
            Get
                Return _AddDomainBehaviorFlag.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property Version() As UShort Implements iormContainerAttribute.Version
            Get
                Return Me._Version
            End Get
            Set(value As UShort)
                Me._Version = value
            End Set
        End Property
        Public ReadOnly Property HasValueVersion As Boolean Implements iormContainerAttribute.HasValueVersion
            Get
                Return _Version.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the ID of the Attribute
        ''' </summary>
        ''' <value>The ID.</value>
        Public Property ID() As String Implements iormContainerAttribute.ID
            Get
                Return Me._ID
            End Get
            Set(value As String)
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean Implements iormContainerAttribute.HasValueID
            Get
                Return _ID IsNot Nothing AndAlso _ID <> String.Empty
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the add deletefield flag. This will add a field for deletion the record to the schema.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AddDeleteFieldBehavior As Boolean Implements iormContainerAttribute.AddDeleteFieldBehavior
            Get
                Return Me._DeleteFieldFlag
            End Get
            Set(value As Boolean)
                _DeleteFieldFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueDeleteFieldBehavior As Boolean Implements iormContainerAttribute.HasValueDeleteFieldBehavior
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
        Public Property AddSpareFields As Boolean Implements iormContainerAttribute.AddSpareFields
            Get
                Return Me._SpareFieldsFlag
            End Get
            Set(value As Boolean)
                _SpareFieldsFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueSpareFields As Boolean Implements iormContainerAttribute.HasValueSpareFields
            Get
                Return _SpareFieldsFlag.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Add an foreign key entry
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddForeignKey(entry As ormForeignKeyAttribute) As Boolean Implements iormContainerAttribute.AddForeignKey
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
        Public Function GetForeignkey(id As String, Optional enabledonly As Boolean = True) As ormForeignKeyAttribute Implements iormContainerAttribute.GetForeignKey
            If _foreignkeys.ContainsKey(id.ToUpper) Then
                Dim anAttribute As ormForeignKeyAttribute = _foreignkeys.Item(id.ToUpper)
                If enabledonly AndAlso Not anAttribute.Enabled Then Return Nothing
                Return anAttribute
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
        Public Function HasForeignkey(id As String, Optional enabledonly As Boolean = True) As Boolean Implements iormContainerAttribute.HasForeignKey
            Dim result As Boolean = _foreignkeys.ContainsKey(id.ToUpper)
            If enabledonly And result Then
                Dim anAttribute As ormForeignKeyAttribute = _foreignkeys.Item(id.ToUpper)
                If Not anAttribute.Enabled Then Return False
            End If
            Return result
        End Function
        ''' <summary>
        ''' remove a foreign key entry
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveForeignKey(id As String) As Boolean Implements iormContainerAttribute.RemoveForeignKey
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
        Public ReadOnly Property ForeignKeyAttributes As IEnumerable(Of ormForeignKeyAttribute) Implements iormContainerAttribute.ForeignkeyAttributes
            Get
                Return _foreignkeys.Values.Where(Function(x) x.Enabled = True).ToList
            End Get
        End Property

        ''' <summary>
        ''' Add an index
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AddIndex(index As ormIndexAttribute) As Boolean Implements iormContainerAttribute.AddIndex
            If _indices.ContainsKey(index.IndexName.ToUpper) Then
                _indices.Remove(index.IndexName.ToUpper)
            End If
            _indices.Add(key:=index.IndexName.ToUpper, value:=index)
            Return True
        End Function
        ''' <summary>
        ''' update an index 
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UpdateIndex(index As ormIndexAttribute) As Boolean Implements iormContainerAttribute.UpdateIndex
            If _indices.ContainsKey(index.IndexName.ToUpper) Then
                _indices.Remove(index.IndexName.ToUpper)
            End If
            _indices.Add(key:=index.IndexName.ToUpper, value:=index)
            Return True
        End Function
        ''' <summary>
        ''' returns an entry by columnname or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetIndex(indexname As String, Optional onlyenabled As Boolean = True) As ormIndexAttribute Implements iormContainerAttribute.GetIndex
            If _indices.ContainsKey(indexname.ToUpper) Then
                Dim anAttribute As ormIndexAttribute = _indices.Item(key:=indexname.ToUpper)
                If onlyenabled AndAlso Not anAttribute.Enabled Then Return Nothing
                Return anAttribute
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' returns true if the indexname exists in the table attribute
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function HasIndex(indexname As String, Optional onlyenabled As Boolean = Nothing) As Boolean Implements iormContainerAttribute.HasIndex
            Dim result As Boolean = _indices.ContainsKey(indexname.ToUpper)
            If onlyenabled AndAlso result Then
                result = _indices.Item(indexname.ToUpper).Enabled
            End If
            Return result
        End Function
        ''' <summary>
        ''' remove an index
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RemoveIndex(indexname As String) As Boolean Implements iormContainerAttribute.RemoveIndex
            If _indices.ContainsKey(indexname.ToUpper) Then
                _indices.Remove(indexname.ToUpper)
                Return True
            Else
                Return False
            End If
        End Function
        ''' <summary>
        ''' returns a List of all index attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IndexAttributes As IEnumerable(Of ormIndexAttribute) Implements iormContainerAttribute.IndexAttributes
            Get
                Return _indices.Values.Where(Function(x) x.Enabled = True).ToList
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
    Public Class ormTableAttribute
        Inherits ormContainerAttribute
        Implements iormContainerAttribute


        '** dynamic
        'Private _columns As New Dictionary(Of String, ormColumnAttribute)


        ''' <summary>
        '''  construcotr
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
        End Sub
        ''' <summary>
        ''' Gets or sets the container type
        ''' </summary>
        ''' <value>The is active.</value>
        Public Overrides Property Containertype As otContainerType Implements iormContainerAttribute.ContainerType
            Get
                Return otContainerType.Table
            End Get
            Set(value As otContainerType)
                Throw New NotSupportedException(" do not set a container type here")
            End Set
        End Property
        '''' <summary>
        '''' Add a member - which is a columnattribute
        '''' </summary>
        '''' <param name="member"></param>
        '''' <remarks></remarks>
        '''' <returns></returns>
        'Public Overloads Function AddEntry(member As iormContainerEntryAttribute) As Boolean Implements iormContainerAttribute.AddEntry
        '    Return AddColumn(member)
        'End Function
        '''' <summary>
        '''' Add an entry by TabeColumn
        '''' </summary>
        '''' <param name="entry"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Function AddColumn(entry As ormContainerEntryAttribute) As Boolean
        '    If _columns.ContainsKey(entry.ContainerEntryName.ToUpper) Then
        '        _columns.Remove(entry.ContainerEntryName.ToUpper)
        '    End If
        '    _columns.Add(key:=entry.ContainerEntryName.ToUpper, value:=entry)
        '    If entry.HasValuePrimaryKeyOrdinal Then
        '        If _primaryEntries.ContainsKey(entry.PrimaryEntryOrdinal) Then _primaryEntries.Remove(entry.PrimaryEntryOrdinal)
        '        _primaryEntries.Add(key:=entry.PrimaryEntryOrdinal, value:=entry.ContainerEntryName)
        '    End If
        '    Return True
        'End Function
        '''' <summary>
        '''' Add an entry by TabeColumn
        '''' </summary>
        '''' <param name="entry"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Function UpdateColumn(entry As ormContainerEntryAttribute) As Boolean
        '    If _columns.ContainsKey(entry.ContainerEntryName.ToUpper) Then
        '        _columns.Remove(entry.ContainerEntryName.ToUpper)
        '    End If
        '    _columns.Add(key:=entry.ContainerEntryName.ToUpper, value:=entry)
        '    If entry.HasValuePrimaryKeyOrdinal Then
        '        If _primaryEntries.ContainsKey(entry.PrimaryEntryOrdinal) Then _primaryEntries.Remove(entry.PrimaryEntryOrdinal)
        '        _primaryEntries.Add(key:=entry.PrimaryEntryOrdinal, value:=entry.ContainerEntryName)
        '    Else
        '        If _primaryEntries.Values.Contains(entry.ContainerEntryName) Then
        '            _primaryEntries.Remove(_primaryEntries.First(Function(x) x.Key = entry.ContainerEntryName).Key)
        '        End If
        '    End If
        '    Return True
        'End Function
        '''' <summary>
        '''' Add an entry by iormContainerMember
        '''' </summary>
        '''' <param name="member"></param>
        '''' <remarks></remarks>
        '''' <returns></returns>
        'Public Overloads Function UpdateEntry(member As iormContainerEntryAttribute) As Boolean Implements iormContainerAttribute.UpdateEntry
        '    Return UpdateColumn(member)
        'End Function
        '''' <summary>
        '''' returns an entry by member name or nothing
        '''' </summary>
        '''' <param name="membername"></param>
        '''' <param name="onlyenabled"></param>
        '''' <remarks></remarks>
        '''' <returns></returns>
        'Public Overloads Function GetEntry(membername As String, Optional onlyenabled As Boolean = True) As iormContainerEntryAttribute Implements iormContainerAttribute.GetEntry
        '    Return GetColumn(membername, onlyenabled)
        'End Function


        '''' <summary>
        '''' returns an entry by columnname or nothing
        '''' </summary>
        '''' <param name="columnname"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Function GetColumn(columnname As String, Optional onlyenabled As Boolean = True) As ormContainerEntryAttribute
        '    If _columns.ContainsKey(columnname.ToUpper) Then
        '        Dim anAttribute As ormContainerEntryAttribute = _columns.Item(key:=columnname.ToUpper)
        '        If onlyenabled AndAlso Not anAttribute.Enabled Then Return Nothing
        '        Return anAttribute
        '    Else
        '        Return Nothing
        '    End If
        'End Function
        '''' <summary>
        '''' returns an entry by columnname or nothing
        '''' </summary>
        '''' <param name="columnname"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Overloads Function HasColumn(columnname As String, Optional onlyenabled As Boolean = Nothing) As Boolean Implements iormContainerAttribute.HasEntry
        '    Dim result As Boolean = _columns.ContainsKey(columnname.ToUpper)
        '    If onlyenabled AndAlso result Then
        '        result = _columns.Item(columnname.ToUpper).Enabled
        '    End If
        '    Return result
        'End Function
        '''' <summary>
        '''' remove an entry by columnname 
        '''' </summary>
        '''' <param name="columnname"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Overloads Function RemoveColumn(columnname As String) As Boolean Implements iormContainerAttribute.RemoveEntry
        '    If _columns.ContainsKey(columnname.ToUpper) Then
        '        _columns.Remove(columnname.ToUpper)
        '        If _primaryEntries.Values.Contains(columnname) Then
        '            _primaryEntries.Remove(_primaryEntries.First(Function(x) x.Key = columnname).Key)
        '        End If
        '        Return True
        '    Else
        '        Return False
        '    End If
        'End Function
        '''' <summary>
        '''' returns a List of all Entries
        '''' </summary>
        '''' <returns></returns>
        '''' <remarks></remarks>
        '''' <value></value>
        'Public Overloads ReadOnly Property MemberAttributes() As IEnumerable(Of iormContainerEntryAttribute) Implements iormContainerAttribute.EntryAttributes
        '    Get
        '        Return ColumnAttributes
        '    End Get
        'End Property
        '''' <summary>
        '''' returns a List of all Entries
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public ReadOnly Property ColumnAttributes As IEnumerable(Of ormContainerEntryAttribute)
        '    Get
        '        Return _columns.Values.Where(Function(x) x.Enabled = True).ToList
        '    End Get
        'End Property


        '''' <summary>
        '''' sets or returns the Names of the PrimaryKey Columns
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Overloads Property PrimaryKeyColumnNames As String() Implements iormContainerAttribute.PrimaryEntryNames
        '    Get
        '        Return _primaryEntries.Values.ToArray
        '    End Get
        '    Set(value As String())
        '        _primaryEntries.Clear()

        '        For i = value.GetLowerBound(0) To value.GetUpperBound(0)
        '            _primaryEntries.Add(key:=i, value:=value(i))
        '        Next

        '    End Set
        'End Property
        '''' <summary>
        '''' returns a List of all Entries
        '''' </summary>
        '''' <value></value>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Overloads ReadOnly Property ColumnNames As IEnumerable(Of String) Implements iormContainerAttribute.EntryNames
        '    Get
        '        Return _columns.Values.Where(Function(x) x.Enabled = True).SelectMany(Function(x) x.ContainerEntryName).ToList
        '    End Get
        'End Property

        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Overloads Property TableID() As String Implements iormContainerAttribute.ContainerID
            Get
                Return _ContainerID
            End Get
            Set(value As String)
                _ContainerID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableID As Boolean Implements iormContainerAttribute.HasValueContainerID
            Get
                Return Not String.IsNullOrWhiteSpace(_ContainerID)
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
    Public Class ormRelationAttribute
        Inherits Attribute
        Private _Name As String
        Private _Version As Nullable(Of UShort)
        Private _TableName As String
        Private _enabled As Boolean = True
        Private _LinkedwithObject As System.Type
        Private _LinkJoin As String
        Private _FromEntries As String()
        Private _ToEntries As String()
        Private _ToPrimaryKeys As String()
        Private _RetrieveOperationID As String
        Private _CreateOperationID As String
        Private _DeleteOperationID As String
        Private _CreateObjectIfNotRetrieved As Boolean? = False

        Private _CascadeOnCreate As Nullable(Of Boolean)
        Private _CascadeOnDelete As Nullable(Of Boolean)
        Private _CascadeOnUpdate As Nullable(Of Boolean)
        Public Sub New()

        End Sub

        ''' <summary>
        ''' Gets or sets the create object if not retrieved flag - which means that the relation manager
        ''' tries to create automaticaly objects we they cannot be retrieved (not existing).
        ''' </summary>
        ''' <value>The create object if not retrieved.</value>
        Public Property CreateObjectIfNotRetrieved() As Boolean
            Get
                Return Me._CreateObjectIfNotRetrieved
            End Get
            Set(value As Boolean)
                Me._CreateObjectIfNotRetrieved = value
            End Set
        End Property
        Public ReadOnly Property HasValueCreateObjectIfNotRetrieved As Boolean
            Get
                Return _CreateObjectIfNotRetrieved.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the cascade on update.
        ''' </summary>
        ''' <value>The cascade on update.</value>
        Public Property CascadeOnUpdate() As Boolean
            Get
                Return Me._CascadeOnUpdate
            End Get
            Set(value As Boolean)
                Me._CascadeOnUpdate = value
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
                Me._CascadeOnDelete = value
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
                Me._CascadeOnCreate = value
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
                Me._ToPrimaryKeys = value
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
                Me._ToEntries = value
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
                Me._FromEntries = value
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
                Return _LinkJoin IsNot Nothing AndAlso _LinkJoin <> String.empty
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the operation ID to call instead of select.
        ''' </summary>
        ''' <value>The link join.</value>
        Public Property RetrieveOperation() As String
            Get
                Return Me._RetrieveOperationID
            End Get
            Set(value As String)
                Me._RetrieveOperationID = value
            End Set
        End Property
        Public ReadOnly Property HasValueRetrieveOperationID As Boolean
            Get
                Return _RetrieveOperationID IsNot Nothing AndAlso _RetrieveOperationID <> String.empty
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the operation ID to call instead to create an relation object if needed.
        ''' </summary>
        ''' <value>The link join.</value>
        Public Property CreateOperation() As String
            Get
                Return Me._CreateOperationID
            End Get
            Set(value As String)
                Me._CreateOperationID = value
            End Set
        End Property
        Public ReadOnly Property HasValueCreateOperationID As Boolean
            Get
                Return _CreateOperationID IsNot Nothing AndAlso _CreateOperationID <> String.empty
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
                Return _Name IsNot Nothing AndAlso _Name <> String.empty
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property TableID() As String
            Get
                Return Me._TableName
            End Get
            Set(value As String)
                Me._TableName = UCase(value)
            End Set
        End Property
        Public ReadOnly Property HasValueTableID As Boolean
            Get
                Return _TableName IsNot Nothing AndAlso _TableName <> String.empty
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
    Public Class ormIndexAttribute
        Inherits Attribute

        Private _indexName As String
        Private _ColumnNames() As String = {}
        Private _enabled As Boolean = True
        Private _Version As Nullable(Of UShort)
        Private _TableName As String = Nothing
        Private _description As String
        Private _isprimaryKey As Nullable(Of Boolean) = False
        Private _isUnique As Nullable(Of Boolean) = False
        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the table.
        ''' </summary>
        ''' <value>The name of the table.</value>
        Public Property TableID() As String
            Get
                Return Me._TableName
            End Get
            Set(value As String)
                Me._TableName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableID As Boolean
            Get
                Return _TableName IsNot Nothing AndAlso _TableName <> String.empty
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
                Return _description IsNot Nothing AndAlso _description <> String.empty
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
                Return _indexName IsNot Nothing AndAlso _indexName <> String.empty
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
    Public Class ormContainerEntryAttribute
        Inherits Attribute
        Implements iormContainerEntryAttribute

        Protected _ID As String = Nothing
        Protected _ContainerID As String = Nothing
        Protected _Datatype As Nullable(Of otDataType)
        Protected _InnerDataType As Nullable(Of otDataType)
        Protected _size As Nullable(Of Long)
        Protected _Parameter As String = Nothing
        Protected _PrimaryEntryOrdinal As Nullable(Of Short)
        Protected _relation() As String = Nothing
        Protected _IsNullable As Nullable(Of Boolean)
        Protected _IsUnique As Nullable(Of Boolean)
        Protected _DBDefaultValue As String = Nothing
        Protected _Version As Nullable(Of UShort)
        Protected _Posordinal As Nullable(Of UShort)
        'Protected _ReferenceContainerEntry As String = Nothing
        Protected _ReferenceObjectEntry As String = Nothing ' needed for resolving 
        Protected _UseForeignKey As Nullable(Of otForeignKeyImplementation) = otForeignKeyImplementation.None
        Protected _ForeignKeyReference As String() = Nothing
        Protected _ForeignKeyProperties As ForeignKeyProperty()
        Protected _ContainerEntryName As String = Nothing
        Protected _Description As String = Nothing
        Protected _enabled As Boolean = True

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean Implements iormContainerEntryAttribute.Enabled
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the ID.
        ''' </summary>
        ''' <value>The ID.</value>
        Public Property ID() As String Implements iormContainerEntryAttribute.ID
            Get
                Return Me._ID
            End Get
            Set(value As String)
                Me._ID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueID As Boolean Implements iormContainerEntryAttribute.HasValueID
            Get
                Return _ID IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the name of the column.
        ''' </summary>
        ''' <value>The name of the column.</value>
        Public Property ContainerEntryName() As String Implements iormContainerEntryAttribute.ContainerEntryName
            Get
                Return Me._ContainerEntryName
            End Get
            Set(value As String)
                Me._ContainerEntryName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueContainerEntryName As Boolean Implements iormContainerEntryAttribute.HasValueContainerEntryName
            Get
                Return _ContainerEntryName IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the reference object entry. Has the form [objectname].[entryname] 
        ''' such as Deliverable.constObjectID & "." & deliverable.constFNUID
        ''' </summary>
        ''' <value>The reference object entry.</value>
        Public Property ReferenceObjectEntry() As String Implements iormContainerEntryAttribute.ReferenceObjectEntry
            Get
                Return Me._ReferenceObjectEntry
            End Get
            Set(value As String)
                Me._ReferenceObjectEntry = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueReferenceObjectEntry As Boolean Implements iormContainerEntryAttribute.HasValueReferenceObjectEntry
            Get
                Return Not String.IsNullOrWhiteSpace(_ReferenceObjectEntry)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String Implements iormContainerEntryAttribute.Description
            Get
                Return Me._Description
            End Get
            Set(value As String)
                Me._Description = value
            End Set
        End Property
        Public ReadOnly Property HasValueDescription As Boolean Implements iormContainerEntryAttribute.HasValueDescription
            Get
                Return _Description IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the pos ordinal.
        ''' </summary>
        ''' <value>The pos ordinal.</value>
        Public Property Posordinal() As UShort Implements iormContainerEntryAttribute.Posordinal
            Get
                Return Me._Posordinal
            End Get
            Set(value As UShort)
                Me._Posordinal = value
            End Set
        End Property

        Public ReadOnly Property HasValuePosOrdinal As Boolean Implements iormContainerEntryAttribute.HasValuePosOrdinal
            Get
                Return _Posordinal.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the default value in DB presentation.
        ''' </summary>
        ''' <value>The default value.</value>
        Public Property DBDefaultValue() As String Implements iormContainerEntryAttribute.DBDefaultValue
            Get
                Return Me._DBDefaultValue
            End Get
            Set(value As String)
                Me._DBDefaultValue = value
            End Set
        End Property
        Public ReadOnly Property HasValueDBDefaultValue As Boolean Implements iormContainerEntryAttribute.HasValueDBDefaultValue
            Get
                Return _DBDefaultValue IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the table ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Public Property ContainerID() As String Implements iormContainerEntryAttribute.ContainerID
            Get
                Return Me._ContainerID
            End Get
            Set(value As String)
                Me._ContainerID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueContainerID As Boolean Implements iormContainerEntryAttribute.HasValueContainerID
            Get
                Return _ContainerID IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the Datatype.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public Property DataType() As otDataType Implements iormContainerEntryAttribute.DataType
            Get
                Return Me._Datatype
            End Get
            Set(value As otDataType)
                Me._Datatype = value
            End Set
        End Property
        Public ReadOnly Property HasValueDataType As Boolean Implements iormContainerEntryAttribute.HasValueDataType
            Get
                Return _Datatype.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the nested inner Datatype of Datatype list.
        ''' </summary>
        ''' <value>The typeid.</value>
        Public Property InnerDataType() As otDataType Implements iormContainerEntryAttribute.InnerDataType
            Get
                Return Me._InnerDataType
            End Get
            Set(value As otDataType)
                Me._InnerDataType = value
            End Set
        End Property
        Public ReadOnly Property HasValueInnerDataType As Boolean Implements iormContainerEntryAttribute.HasValueInnerDataType
            Get
                Return _InnerDataType.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the size.
        ''' </summary>
        ''' <value>The size.</value>
        Public Property Size() As Long Implements iormContainerEntryAttribute.Size
            Get
                Return Me._size
            End Get
            Set(value As Long)
                Me._size = value
            End Set
        End Property
        Public ReadOnly Property HasValueSize As Boolean Implements iormContainerEntryAttribute.HasValueSize
            Get
                Return _size.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the parameter.
        ''' </summary>
        ''' <value>The parameter.</value>
        Public Property Parameter() As String Implements iormContainerEntryAttribute.Parameter
            Get
                Return Me._Parameter
            End Get
            Set(value As String)
                Me._Parameter = value
            End Set
        End Property
        Public ReadOnly Property HasValueParameter() As Boolean Implements iormContainerEntryAttribute.HasValueParameter
            Get
                Return _Parameter IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is nullable.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Public Property IsNullable() As Boolean Implements iormContainerEntryAttribute.IsNullable
            Get
                Return Me._IsNullable
            End Get
            Set(value As Boolean)
                Me._IsNullable = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsNullable() Implements iormContainerEntryAttribute.HasValueIsNullable
            Get
                Return _IsNullable.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the Unique Property.
        ''' </summary>
        ''' <value></value>
        Public Property IsUnique() As Boolean Implements iormContainerEntryAttribute.IsUnique
            Get
                Return Me._IsUnique
            End Get
            Set(value As Boolean)
                Me._IsUnique = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsUnique() Implements iormContainerEntryAttribute.HasValueIsUnique
            Get
                Return _IsUnique.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the is foreign Key flag. References must be set
        ''' </summary>
        ''' <value></value>
        Public Property UseForeignKey() As otForeignKeyImplementation Implements iormContainerEntryAttribute.UseForeignKey
            Get
                Return Me._UseForeignKey
            End Get
            Set(value As otForeignKeyImplementation)
                Me._UseForeignKey = value
            End Set
        End Property
        Public ReadOnly Property HasValueUseForeignKey() As Boolean Implements iormContainerEntryAttribute.hasValueUseForeignKey
            Get
                Return _UseForeignKey.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key reference.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property ForeignKeyReferences() As String() Implements iormContainerEntryAttribute.ForeignKeyReferences
            Get
                Return Me._ForeignKeyReference
            End Get
            Set(value As String())
                Me._ForeignKeyReference = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyReferences As Boolean Implements iormContainerEntryAttribute.HasValueForeignKeyReferences
            Get
                Return _ForeignKeyReference IsNot Nothing AndAlso _ForeignKeyReference.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the foreign key properties as string
        ''' </summary>
        ''' <value>string</value>
        Public Property ForeignKeyProperties() As String() Implements iormContainerEntryAttribute.ForeignKeyProperties
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
                    CoreMessageHandler(exception:=ex, procedure:="ormContainerEntryAttribute.ForeignKeyProperties")
                End Try
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the foreign key properties as list of ForeignKeyProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ForeignKeyProperty As ForeignKeyProperty() Implements iormContainerEntryAttribute.foreignkeyproperty
            Get
                Return _ForeignKeyProperties
            End Get
            Set(value As ForeignKeyProperty())
                _ForeignKeyProperties = value
            End Set
        End Property
        Public ReadOnly Property HasValueForeignKeyProperties As Boolean Implements iormContainerEntryAttribute.hasValueForeignKeyProperties
            Get
                Return _ForeignKeyProperties IsNot Nothing AndAlso _ForeignKeyProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Public Property PrimaryEntryOrdinal() As Short Implements iormContainerEntryAttribute.PrimaryEntryOrdinal
            Get
                Return Me._primaryEntryOrdinal
            End Get
            Set(value As Short)
                If value > 0 Then
                    Me._primaryEntryOrdinal = value
                Else
                    CoreMessageHandler(message:="position index is less or equal 0", argument:=value, procedure:="ormContainerEntryAttribute.PrimaryEntryordinal", messagetype:=otCoreMessageType.InternalError)
                    Debug.Assert(False)
                End If

            End Set
        End Property
        Public ReadOnly Property HasValuePrimaryKeyOrdinal As Boolean Implements iormContainerEntryAttribute.HasValuePrimaryKeyOrdinal
            Get
                Return _primaryEntryOrdinal.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the relation.
        ''' </summary>
        ''' <value>The relation.</value>
        Public Property Relation() As String() Implements iormContainerEntryAttribute.Relation
            Get
                Return Me._relation
            End Get
            Set(value As String())
                Me._relation = value
            End Set
        End Property
        Public ReadOnly Property HasValueRelation As Boolean Implements iormContainerEntryAttribute.hasValueRelation
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
        Public Property Version As UShort Implements iormContainerEntryAttribute.Version
            Get
                Return Me._Version
            End Get
            Set(value As UShort)
                Me._Version = value
            End Set
        End Property
        Public ReadOnly Property HasValueVersion As Boolean Implements iormContainerEntryAttribute.HasValueVersion
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
    Public Class ormForeignKeyAttribute
        Inherits Attribute
        Private _ID As String
        Private _TableID As String = Nothing
        Private _enabled As Boolean = True
        Private _ObjectID As String = Nothing
        Private _Version As Nullable(Of UShort)
        Private _UseForeignKey As Nullable(Of otForeignKeyImplementation) = otForeignKeyImplementation.None
        Private _ForeignKeyReferences As String() = {}
        Private _ForeignKeyProperties As ForeignKeyProperty()
        Private _Entrynames As String() = {}
        Private _Description As String = Nothing
        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

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
        Public Property TableID() As String
            Get
                Return Me._TableID
            End Get
            Set(value As String)
                Me._TableID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueTableID As Boolean
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
                    CoreMessageHandler(exception:=ex, procedure:="ormSchemaForeignKeyAttribute.ForeignKeyProperties")
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
        Inherits ormContainerEntryAttribute
        Implements iormObjectEntry


        Private _Title As String = Nothing
        Private _EntryType As Nullable(Of otObjectEntryType) = otObjectEntryType.ContainerEntry

        Private _isReadonly As Nullable(Of Boolean)
        Private _isActive As Nullable(Of Boolean)
        Private _Parameter As String = Nothing
        Private _KeyOrdinal As Nullable(Of UShort)
        Private _DefaultValue As Object = Nothing
        Private _Version As Nullable(Of UShort)
        Private _Posordinal As Nullable(Of UShort)
        Private _SpareFieldTag As Nullable(Of Boolean)
        Private _XID As String = Nothing
        Private _aliases() As String = Nothing
        Private _relation() As String = Nothing

        Private _objectEntryName As String = Nothing
        Private _category As String = Nothing
        Private _objectName As String = Nothing
        Private _properties As ObjectEntryProperty()

        Private _validate As Nullable(Of Boolean)
        Private _LowerRange As Nullable(Of Long) = Nothing
        Private _upperRange As Nullable(Of Long) = Nothing
        Private _Values As String()
        Private _lookupCondition As String = Nothing
        Private _LookupProperties As LookupProperty()
        Private _ValidationProperties As ObjectValidationProperty()
        Private _validateRegExp As String = Nothing

        Private _render As Nullable(Of Boolean)
        Private _RenderProperties As RenderProperty()
        Private _RenderRegExpMatch As String
        Private _RenderRegExpPattern As String


        ''' <summary>
        ''' Gets or sets the type of the entry.
        ''' </summary>
        ''' <value>The type of the entry.</value>
        Public Property EntryType() As otObjectEntryType Implements iormObjectEntry.Typeid
            Get
                Return Me._EntryType
            End Get
            Set(value As otObjectEntryType)
                Me._EntryType = value
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
        Public Property LookupCondition() As String Implements iormObjectEntry.LookupCondition
            Get
                Return Me._lookupCondition
            End Get
            Set(value As String)
                Me._lookupCondition = value
            End Set
        End Property
        Public ReadOnly Property HasValueLookupCondition As Boolean Implements iormObjectEntry.HasLookupCondition
            Get
                Return _lookupCondition IsNot Nothing 'AndAlso _validateRegExp <> String.empty empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render reg exp pattern.
        ''' </summary>
        ''' <value>The render reg exp pattern.</value>
        Public Property RenderRegExpPattern() As String Implements iormObjectEntry.RenderRegExpPattern
            Get
                Return Me._RenderRegExpPattern
            End Get
            Set(value As String)
                Me._RenderRegExpPattern = value
            End Set
        End Property
        Public ReadOnly Property HasValueRenderRegExpPattern As Boolean Implements iormObjectEntry.HasRenderRegExpression
            Get
                Return _RenderRegExpPattern IsNot Nothing 'AndAlso _validateRegExp <> String.empty empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render reg exp match.
        ''' </summary>
        ''' <value>The render reg exp match.</value>
        Public Property RenderRegExpMatch() As String Implements iormObjectEntry.RenderRegExpMatch
            Get
                Return Me._RenderRegExpMatch
            End Get
            Set(value As String)
                Me._RenderRegExpMatch = value
            End Set
        End Property
        Public ReadOnly Property HasValueRenderRegExpMatch As Boolean 'Implements iormObjectEntry.HasRenderRegMatch
            Get
                Return _RenderRegExpMatch IsNot Nothing 'AndAlso _validateRegExp <> String.empty empty string is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the object entry properties.
        ''' </summary>
        ''' <value>The render properties.</value>
        Public Property Properties() As String() Implements iormObjectEntry.PropertyStrings
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
                    CoreMessageHandler(exception:=ex, procedure:="ormObjectEntryAttribute.Properties")
                End Try
            End Set
        End Property
        Public Property ObjectEntryProperties As List(Of ObjectEntryProperty) Implements iormObjectEntry.Properties
            Get
                Return _properties.ToList
            End Get
            Set(value As List(Of ObjectEntryProperty))
                _properties = value.ToArray
            End Set
        End Property
        Public ReadOnly Property HasValueObjectEntryProperties As Boolean
            Get
                Return _properties IsNot Nothing AndAlso _properties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render properties.
        ''' </summary>
        ''' <value>The render properties.</value>
        '''  ''' <summary>
        ''' Gets or sets the object entry properties.
        ''' </summary>
        ''' <value>The render properties.</value>
        Public Property RenderPropertyStrings() As String() Implements iormObjectEntry.RenderPropertyStrings
            Get
                Dim aList As New List(Of String)
                For Each aP In _RenderProperties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of RenderProperty)
                    For Each aValue In value
                        aList.Add(New RenderProperty(aValue))
                    Next
                    Me._RenderProperties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, procedure:="ormObjectEntryAttribute.RenderPropertyStrings")
                End Try
            End Set
        End Property
        Public Property RenderProperties() As List(Of RenderProperty) Implements iormObjectEntry.RenderProperties
            Get
                Return Me._RenderProperties.ToList
            End Get
            Set(value As List(Of RenderProperty))
                Me._RenderProperties = value.ToArray
            End Set
        End Property
        Public ReadOnly Property HasValueRenderProperties As Boolean
            Get
                Return _RenderProperties IsNot Nothing AndAlso _RenderProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the isActive flag
        ''' </summary>
        ''' <value>The render.</value>
        Public Property IsActive As Boolean Implements iormObjectEntry.IsActive
            Get
                Return Me._isActive
            End Get
            Set(value As Boolean)
                Me._isActive = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsActive As Boolean
            Get
                Return _isActive.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the readonly flag
        ''' </summary>
        ''' <value>The render.</value>
        Public Property [IsReadOnly] As Boolean Implements iormObjectEntry.IsReadonly
            Get
                Return Me._isReadonly
            End Get
            Set(value As Boolean)
                Me._isReadonly = value
            End Set
        End Property
        Public ReadOnly Property HasValueIsReadonly As Boolean
            Get
                Return _isReadonly.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the render.
        ''' </summary>
        ''' <value>The render.</value>
        Public Property Render() As Boolean Implements iormObjectEntry.IsRendering
            Get
                Return Me._render
            End Get
            Set(value As Boolean)
                Me._render = value
            End Set
        End Property
        Public ReadOnly Property HasValueRender As Boolean Implements iormObjectEntry.HasRenderProperties
            Get
                Return _render.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validate reg exp.
        ''' </summary>
        ''' <value>The validate reg exp.</value>
        Public Property ValidateRegExp() As String Implements iormObjectEntry.ValidateRegExpression
            Get
                Return Me._validateRegExp
            End Get
            Set(value As String)
                Me._validateRegExp = value
            End Set
        End Property
        Public ReadOnly Property HasValueValidateRegExp As Boolean Implements iormObjectEntry.HasValidateRegExpression
            Get
                Return _validateRegExp IsNot Nothing 'AndAlso _validateRegExp <> String.empty empty is possible
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the Lookup properties.
        ''' </summary>
        ''' <value>The validation properties.</value>
        '''  
        Public Property LookupPropertyStrings() As String() Implements iormObjectEntry.LookupPropertyStrings
            Get
                Dim aList As New List(Of String)
                For Each aP In _ValidationProperties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of LookupProperty)
                    For Each aValue In value
                        aList.Add(New LookupProperty(aValue))
                    Next
                    Me._LookupProperties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, procedure:="ormObjectEntryAttribute.LookupPropertyStrings")
                End Try
            End Set
        End Property
        Public Property LookupProperties() As List(Of LookupProperty) Implements iormObjectEntry.LookupProperties
            Get
                Return Me._LookupProperties.ToList
            End Get
            Set(value As List(Of LookupProperty))
                Me._LookupProperties = value.ToArray
            End Set
        End Property
        Public ReadOnly Property HasValueLookupProperties As Boolean Implements iormObjectEntry.HasLookupProperties
            Get
                Return _LookupProperties IsNot Nothing AndAlso _LookupProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validation properties.
        ''' </summary>
        ''' <value>The validation properties.</value>
        '''  
        Public Property ValidationPropertyStrings() As String() Implements iormObjectEntry.ValidationPropertyStrings
            Get
                Dim aList As New List(Of String)
                For Each aP In _ValidationProperties
                    aList.Add(aP.ToString)
                Next
                Return aList.ToArray
            End Get
            Set(value As String())
                Try
                    Dim aList As New List(Of ObjectValidationProperty)
                    For Each aValue In value
                        aList.Add(New ObjectValidationProperty(aValue))
                    Next
                    Me._ValidationProperties = aList.ToArray
                Catch ex As Exception
                    CoreMessageHandler(exception:=ex, procedure:="ormObjectEntryAttribute.ValidationPropertyStrings")
                End Try
            End Set
        End Property
        Public Property ValidationProperties() As List(Of ObjectValidationProperty) Implements iormObjectEntry.ValidationProperties
            Get
                Return Me._ValidationProperties.ToList
            End Get
            Set(value As List(Of ObjectValidationProperty))
                Me._ValidationProperties = value.ToArray
            End Set
        End Property
        Public ReadOnly Property HasValueValidationProperties As Boolean Implements iormObjectEntry.HasValidationProperties
            Get
                Return _ValidationProperties IsNot Nothing AndAlso _ValidationProperties.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the possible values in string presentation as list
        ''' </summary>
        ''' <value>The values.</value>
        Public Property PossibleValues As List(Of String) Implements iormObjectEntry.PossibleValues
            Get
                Return Me._Values.ToList
            End Get
            Set(value As List(Of String))
                Me._Values = value.ToArray
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Description() As String Implements iormObjectEntry.Description
            Get
                Return MyBase.Description
            End Get
            Set(value As String)
                MyBase.Description = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the possible values in string presentation as array.
        ''' </summary>
        ''' <value>The values.</value>
        Public Property Values As String()
            Get
                Return Me._Values
            End Get
            Set(value As String())
                Me._Values = value
            End Set
        End Property
        Public ReadOnly Property HasValueValues As Boolean Implements iormObjectEntry.HasPossibleValues
            Get
                Return _Values IsNot Nothing AndAlso _Values.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the upper range.
        ''' </summary>
        ''' <value>The upper range.</value>
        Public Property UpperRange() As Long
            Get
                Return Me._upperRange
            End Get
            Set(value As Long)
                Me._upperRange = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the upper range as nullable
        ''' </summary>
        ''' <value>The upper range.</value>
        Public Property UpperRangeValue() As Long? Implements iormObjectEntry.UpperRangeValue
            Get
                Return Me._upperRange
            End Get
            Set(value As Long?)
                Me._upperRange = value
            End Set
        End Property
        Public ReadOnly Property HasValueUpperRange As Boolean Implements iormObjectEntry.HasUpperRangeValue
            Get
                Return _upperRange.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the lower range as nullable.
        ''' </summary>
        ''' <value>The lower range.</value>
        Public Property LowerRangeValue() As Long? Implements iormObjectEntry.LowerRangeValue
            Get
                Return Me._LowerRange
            End Get
            Set(value As Long?)
                Me._LowerRange = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the lower range.
        ''' </summary>
        ''' <value>The lower range.</value>
        Public Property LowerRange() As Long
            Get
                Return Me._LowerRange
            End Get
            Set(value As Long)
                Me._LowerRange = value
            End Set
        End Property
        Public ReadOnly Property HasValueLowerRange As Boolean Implements iormObjectEntry.HasLowerRangeValue
            Get
                Return _LowerRange.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the validate.
        ''' </summary>
        ''' <value>The validate.</value>
        Public Property Validate() As Boolean Implements iormObjectEntry.IsValidating
            Get
                Return Me._validate
            End Get
            Set(value As Boolean)
                Me._validate = value
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
        Public Property ObjectName() As String Implements iormObjectEntry.Objectname
            Get
                Return Me._objectName
            End Get
            Set(value As String)
                Me._objectName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueObjectName As Boolean
            Get
                Return _objectName IsNot Nothing AndAlso _objectName <> String.Empty
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the default value in DB presentation.
        ''' </summary>
        ''' <value>The default value.</value>
        Public Property DefaultValue() As Object Implements iormObjectEntry.DefaultValue
            Get
                Return Me._DefaultValue
            End Get
            Set(value As Object)
                Me._DefaultValue = value
            End Set
        End Property
        Public ReadOnly Property HasValueDefaultValue As Boolean
            Get
                Return _DefaultValue IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the column.
        ''' </summary>
        ''' <value>The name of the column.</value>
        Public Property EntryName() As String Implements iormObjectEntry.Entryname
            Get
                Return Me._objectEntryName
            End Get
            Set(value As String)
                Me._objectEntryName = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueEntryName As Boolean
            Get
                Return _objectEntryName IsNot Nothing AndAlso _objectEntryName <> String.Empty
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the name of the column.
        ''' </summary>
        ''' <value>The name of the column.</value>
        Public Property XID() As String Implements iormObjectEntry.XID
            Get
                Return Me._XID
            End Get
            Set(value As String)
                Me._XID = value.ToUpper
            End Set
        End Property
        Public ReadOnly Property HasValueXID As Boolean
            Get
                Return _XID IsNot Nothing AndAlso _XID <> String.Empty
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Public Property KeyOrdinal() As Long Implements iormObjectEntry.PrimaryKeyOrdinal
            Get
                Return Me._KeyOrdinal
            End Get
            Set(value As Long)
                If value > 0 Then
                    Me._KeyOrdinal = value
                Else
                    CoreMessageHandler(message:="position index is less or equal 0", argument:=value, procedure:="ormObjectEntry.Keyordinal", messagetype:=otCoreMessageType.InternalError)
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
        Public Property Posordinal() As Long Implements iormObjectEntry.Ordinal
            Get
                Return Me._Posordinal
            End Get
            Set(value As Long)
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
        Public Property SpareFieldTag As Boolean Implements iormObjectEntry.IsSpareField
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
        ''' Gets or sets the category.
        ''' </summary>
        ''' <value>The category.</value>
        Public Property Category() As String Implements iormObjectEntry.Category
            Get
                Return Me._category
            End Get
            Set(value As String)
                Me._category = value
            End Set
        End Property
        Public ReadOnly Property HasValueCategory As Boolean
            Get
                Return Not String.IsNullOrEmpty(_category)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the title.
        ''' </summary>
        ''' <value>The title.</value>
        Public Property Title() As String Implements iormObjectEntry.Title
            Get
                Return Me._Title
            End Get
            Set(value As String)
                Me._Title = value
            End Set
        End Property
        Public ReadOnly Property HasValueTitle As Boolean
            Get
                Return Not String.IsNullOrEmpty(_Title)
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

        Public Function GetObjectDefinition() As ObjectDefinition Implements iormObjectEntry.GetObjectDefinition
            Throw New InvalidOperationException("not applicable for attributes")
            Return Nothing
        End Function
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
        Public Property Aliases() As String() Implements iormObjectEntry.Aliases
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
        Public Property Version As Long Implements iormObjectEntry.Version
            Get
                Return Me._Version
            End Get
            Set(value As Long)
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
        ''' <summary>
        ''' set the datatype for the objectentry attribute (stub)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Datatype() As otDataType Implements iormObjectEntry.Datatype
            Get
                Return MyBase.DataType
            End Get
            Set(value As otDataType)
                MyBase.DataType = value
            End Set
        End Property

        ''' <summary>
        ''' returns true if the Entry is mapped to a class member field
        ''' </summary>
        ''' Inherits iormPersistable -&gt; ObjectEntryAttribute is also covering this
        ''' Inherits System.ComponentModel.INotifyPropertyChanged
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <value></value>
        Public Property IsMapped() As Boolean Implements iormObjectEntry.IsMapped
            Get
                ' TODO: Implement this property setter
                Throw New InvalidOperationException()
            End Get
            Set(value As Boolean)
                ' TODO: Implement this property setter
                Throw New InvalidOperationException()
            End Set
        End Property
        ''' <summary>
        ''' returns True if the Entry is a Column
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <value></value>
        Public Property IsColumn() As Boolean Implements iormObjectEntry.IsColumn
            Get
                Return True
            End Get
            Set(value As Boolean)
                ' TODO: Implement this property setter
                Throw New InvalidOperationException()
            End Set
        End Property
        ''' <summary>
        ''' returns true if the Entry is a Compound entry
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <value></value>
        Public Property IsCompound() As Boolean Implements iormObjectEntry.IsCompound
            Get
                Return False
            End Get
            Set(value As Boolean)
                ' TODO: Implement this property setter
                Throw New InvalidOperationException()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the size.
        ''' </summary>
        ''' <value>The size.</value>
        Public Property Size() As Long
            Get
                Return MyBase.Size
            End Get
            Set(value As Long)
                MyBase.Size = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the size as nullable.
        ''' </summary>
        ''' <value>The size.</value>
        Public Property SizeValue() As Long? Implements iormObjectEntry.Size
            Get
                Return MyBase.Size
            End Get
            Set(value As Long?)
                If value.HasValue Then
                    MyBase.Size = value.Value
                Else
                    MyBase._size = Nothing
                End If


            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the is nullable.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Public Property IsNullable() As Boolean Implements iormObjectEntry.IsNullable
            Get
                Return MyBase.IsNullable
            End Get
            Set(value As Boolean)
                MyBase.IsNullable = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the inner datatype.
        ''' </summary>
        ''' <value>The inner datatype.</value>
        Public Property InnerDatatype() As otDataType
            Get
                Return MyBase.InnerDataType
            End Get
            Set(value As otDataType)
                If value = 0 Then
                    MyBase.InnerDataType = Nothing
                Else
                    MyBase.InnerDataType = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the inner datatype as nullable.
        ''' </summary>
        ''' <value>The inner datatype.</value>
        Public Property InnerDatatypeValue() As otDataType? Implements iormObjectEntry.InnerDatatype
            Get
                Return MyBase.InnerDataType
            End Get
            Set(value As otDataType?)
                MyBase.InnerDataType = value
            End Set
        End Property

        ''' <summary>
        ''' Abstracts the entry definition_ set by attribute.
        ''' </summary>
        ''' <param name="attribute">The attribute.</param>
        ''' <returns></returns>
        Public Function SetByAttribute(attribute As ormObjectEntryAttribute) As Boolean Implements iormObjectEntry.SetByAttribute
            ' TODO: Implement this property setter
            Throw New InvalidOperationException()
        End Function
        ''' <summary>
        ''' handler for the OnSwitchRuntimeOff event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub OnswitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs) Implements iormObjectEntry.OnswitchRuntimeOff
            ' TODO: Implement this property setter
            Throw New InvalidOperationException()
        End Sub
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
        Private _ContainerIDs As String()
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
        Private _PrimaryContainerID As String
        Private _retrieveFromViewID As String
        Private _buildRetrieveView As Nullable(Of Boolean)

        ''' <summary>
        ''' Gets or sets the primary table ID.
        ''' </summary>
        ''' <value>The primary table ID.</value>
        Public Property PrimaryContainerID() As String
            Get
                Return Me._PrimaryContainerID
            End Get
            Set(value As String)
                Me._PrimaryContainerID = Value
            End Set
        End Property

        Public ReadOnly Property HasValuePrimaryContainerID As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_PrimaryContainerID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the primary keys.
        ''' </summary>
        ''' <value>The primary keys.</value>
        Public Property PrimaryKeyEntryNames() As String()
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
        Public Property AddDomainBehavior() As Boolean
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
        Public Property AddSpareFieldsBehavior() As Boolean
            Get
                Return Me._SpareFieldsFlag
            End Get
            Set(value As Boolean)
                Me._SpareFieldsFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueSpareFieldsBehavior As Boolean
            Get
                Return _SpareFieldsFlag.HasValue
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the delete field flag.
        ''' </summary>
        ''' <value>The delete field flag.</value>
        Public Property AddDeleteFieldBehavior() As Boolean
            Get
                Return Me._DeleteFieldFlag
            End Get
            Set(value As Boolean)
                Me._DeleteFieldFlag = value
            End Set
        End Property
        Public ReadOnly Property HasValueDeleteFieldBehavior As Boolean
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
        Public Property ContainerIDs() As String()
            Get
                Return Me._ContainerIDs
            End Get
            Set(value As String())
                For Each s In value
                    If Not String.IsNullOrEmpty(s) Then s = s.ToUpper
                Next
                Me._ContainerIDs = value
            End Set
        End Property
        Public ReadOnly Property HasValueContainerIDs As Boolean
            Get
                Return _ContainerIDs IsNot Nothing AndAlso _ContainerIDs.Count > 0
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
                Me._ClassName = value
            End Set
        End Property
        Public ReadOnly Property HasValueClassname As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_ClassName)
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
                Return Not String.IsNullOrWhiteSpace(_ID)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the viewID for retrieving objects - if nothing tables are used
        ''' </summary>
        ''' <value>The primary table ID.</value>
        Public Property RetrieveObjectFromViewID As String
            Get
                Return Me._retrieveFromViewID
            End Get
            Set(value As String)
                Me._retrieveFromViewID = value
            End Set
        End Property

        Public ReadOnly Property HasValueRetrieveObjectFromViewID As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_retrieveFromViewID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the build-view-for-retrievinging object
        ''' </summary>
        ''' <value>The is active.</value>
        Public Property BuildRetrieveView As Boolean
            Get
                Return Me._buildRetrieveView
            End Get
            Set(value As Boolean)
                Me._buildRetrieveView = value
            End Set
        End Property
        Public ReadOnly Property HasValueBuildRetrieveView As Boolean
            Get
                Return _buildRetrieveView.HasValue
            End Get
        End Property
    End Class
    ''' <summary>
    ''' Attribute for Const fields to describe an object operation
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormObjectTransactionAttribute
        Inherits Attribute
        Private _ID As String = Nothing
        Private _TransactionName As String = Nothing
        Private _enabled As Boolean = True
        Private _Title As String = Nothing
        Private _Description As String = Nothing
        Private _Version As Nullable(Of UShort) = 1
        Private _PermissionRules As String()
        Private _DefaultAllowPermission As Nullable(Of Boolean) = True

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the transaction.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property TransactionName As String
            Get
                Return Me._TransactionName
            End Get
            Set(value As String)
                Me._TransactionName = value
            End Set
        End Property
        Public ReadOnly Property HasValueTransactionName As Boolean
            Get
                Return _TransactionName IsNot Nothing AndAlso _TransactionName <> String.empty
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
                Return Not String.IsNullOrWhiteSpace(_ID)
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
        Private _ParameterEntries As String()
        Private _MethodInfo As MethodInfo
        Private _Tag As String
        Private _TransactionID As String
        Private _Properties As String()
        Private _UIVisible As Boolean?
        Private _ClassDescription As ObjectClassDescription


        ''' <summary>
        ''' Gets or sets the A class description.
        ''' </summary>
        ''' <value>The A class description.</value>
        Public Property ClassDescription() As ObjectClassDescription
            Get
                Return Me._ClassDescription
            End Get
            Set(value As ObjectClassDescription)
                Me._ClassDescription = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the UI visible.
        ''' </summary>
        ''' <value>The UI visible.</value>
        Public Property UIVisible() As Boolean
            Get
                If _UIVisible.HasValue Then Return Me._UIVisible
                Return False
            End Get
            Set(value As Boolean)
                Me._UIVisible = value
            End Set
        End Property
        Public ReadOnly Property HasValueUIVisible As Boolean
            Get
                Return _UIVisible.HasValue
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the Properties
        ''' </summary>
        ''' <value>The tag.</value>
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
                Return _Properties IsNot Nothing AndAlso _Properties.Count <> 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the tag ( free search tag ).
        ''' </summary>
        ''' <value>The tag.</value>
        Public Property Tag() As String
            Get
                Return Me._Tag
            End Get
            Set(value As String)
                Me._Tag = value
            End Set
        End Property
        Public ReadOnly Property HasValueTag As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_Tag)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the transaction ID
        ''' </summary>
        ''' <value>The tag.</value>
        Public Property TransactionID() As String
            Get
                Return Me._TransactionID
            End Get
            Set(value As String)
                Me._TransactionID = value
            End Set
        End Property
        Public ReadOnly Property HasValueTransactionID As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_TransactionID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the method info.
        ''' </summary>
        ''' <value>The method info.</value>
        Public Property MethodInfo() As MethodInfo
            Get
                Return Me._MethodInfo
            End Get
            Set(value As MethodInfo)
                Me._MethodInfo = value
            End Set
        End Property
        Public ReadOnly Property HasValueMethodInfo As Boolean
            Get
                Return _MethodInfo IsNot Nothing
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
        ''' Gets or sets to entries definition of the methods parameters - must match.
        ''' </summary>
        ''' <value>To entries.</value>
        Public Property ParameterEntries() As String()
            Get
                Return Me._ParameterEntries
            End Get
            Set(value As String())
                Me._ParameterEntries = value
            End Set
        End Property
        Public ReadOnly Property HasValueParameterEntries As Boolean
            Get
                Return _ParameterEntries IsNot Nothing AndAlso _ParameterEntries.Count > 0
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
                Return Not String.IsNullOrWhiteSpace(_ID)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Attribute for Const fields to describe an Query for the Object Class
    ''' </summary>
    ''' <remarks></remarks>

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)> _
    Public Class ormObjectQueryAttribute
        Inherits Attribute
        Private _ID As String = Nothing
        Private _enabled As Boolean = True
        Private _where As String = Nothing
        Private _orderBy As String = Nothing
        Private _Description As String = Nothing
        Private _Version As Nullable(Of UShort) = 1
        Private _addAllFields As Nullable(Of Boolean)
        Private _Entrynames As String()

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Public Property Enabled() As Boolean
            Get
                Return Me._enabled
            End Get
            Set(value As Boolean)
                Me._enabled = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the where part of a query.
        ''' </summary>
        ''' <value>The description.</value>
        Public Property Where As String
            Get
                Return Me._where
            End Get
            Set(value As String)
                Me._where = value
            End Set
        End Property
        Public ReadOnly Property HasValueWhere As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_where)
            End Get
        End Property


        ''' <summary>
        ''' Gets or sets the object Properties
        ''' </summary>
        ''' <value>cache.</value>
        Public Property EntryNames() As String()
            Get
                Return Me._Entrynames
            End Get
            Set(value As String())
                Me._Entrynames = value
            End Set
        End Property
        Public ReadOnly Property HasValueEntrynames As Boolean
            Get
                Return _Entrynames IsNot Nothing AndAlso _Entrynames.Count > 0
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Public Property AddAllFields() As Boolean?
            Get
                Return Me._addAllFields
            End Get
            Set(value As Boolean?)
                Me._addAllFields = value
            End Set
        End Property
        Public ReadOnly Property HasValueAddAllFields As Boolean
            Get
                Return _addAllFields.HasValue
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
        ''' Gets or sets the orderby.
        ''' </summary>
        ''' <value>The title.</value>
        Public Property Orderby() As String
            Get
                Return Me._orderBy
            End Get
            Set(value As String)
                Me._orderBy = value
            End Set
        End Property
        Public ReadOnly Property HasValueOrderBy As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_orderBy)
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
                Return Not String.IsNullOrWhiteSpace(_ID)
            End Get
        End Property
    End Class

End Namespace

