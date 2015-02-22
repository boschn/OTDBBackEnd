REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** Attributes Declaration
REM *********** 
REM *********** Version: 2.00
REM *********** Created: 2015-02-13
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2015
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports OnTrack.Database


Namespace OnTrack.Database
    ''' <summary>
    ''' defines a general container attribute interface (container for persisting data objects to)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iormContainerAttribute

        ''' <summary>
        ''' returns the container Type of the Container Attribute
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ContainerType As otContainerType
        ReadOnly Property HasValueContainerType As Boolean
        ''' <summary>
        ''' returns an Inenumerale of all foreign key attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ForeignkeyAttributes As IEnumerable(Of ormForeignKeyAttribute)
        ''' <summary>
        ''' returns an inenumerable of all index attributes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IndexAttributes As IEnumerable(Of ormIndexAttribute)
        ''' <summary>
        ''' remove an index
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RemoveIndex(indexname As String) As Boolean
        ''' <summary>
        ''' returns true if the index exists
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <param name="onlyenabled"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasIndex(indexname As String, Optional onlyenabled As Boolean = False) As Boolean
        ''' <summary>
        ''' retrieves the index attribute
        ''' </summary>
        ''' <param name="indexname"></param>
        ''' <param name="onlyenabled"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetIndex(indexname As String, Optional onlyenabled As Boolean = True) As ormIndexAttribute
        ''' <summary>
        ''' update the index attribute
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateIndex(index As ormIndexAttribute) As Boolean
        ''' <summary>
        ''' add index attribute
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddIndex(index As ormIndexAttribute) As Boolean
        ''' <summary>
        ''' adds a foreign key attribute
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddForeignKey(foreignkey As ormForeignKeyAttribute) As Boolean
        ''' <summary>
        ''' retrieves a foreign key attribute
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="enabledonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetForeignKey(id As String, Optional enabledonly As Boolean = True) As ormForeignKeyAttribute
        ''' <summary>
        ''' removes a foreign key attribute from the container
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RemoveForeignKey(id As String) As Boolean
        ''' <summary>
        ''' returns true if the foreign key attribute exists
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="enabledonly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasForeignKey(id As String, Optional enabledonly As Boolean = True) As Boolean

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Property Enabled() As Boolean

        ''' <summary>
        ''' Gets or sets the cache is active.
        ''' </summary>
        ''' <value>The is active.</value>
        Property UseCache() As Boolean

        ''' <summary>
        ''' true if has value UseCache
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueUseCache() As Boolean

        ''' <summary>
        ''' Gets or sets the cache select.
        ''' </summary>
        ''' <value>cache.</value>
        Property CacheProperties() As String()

        ''' <summary>
        ''' true if there is a CacheProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueCacheProperties() As Boolean

        ''' <summary>
        ''' id of the correlated database driver
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property DatabaseDriverID As String

        ''' <summary>
        ''' returns true if database driver id is set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueDatabaseDriverID As Boolean

        ''' <summary>
        ''' Add a member
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function AddEntry(entry As iormContainerEntryAttribute) As Boolean

        ''' <summary>
        ''' update an entry 
        ''' </summary>
        ''' <param name="entry"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function UpdateEntry(entry As iormContainerEntryAttribute) As Boolean

        ''' <summary>
        ''' returns an entry by entry name or nothing
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function GetEntry(entryname As String, Optional onlyenabled As Boolean = True) As iormContainerEntryAttribute

        ''' <summary>
        ''' returns true if an entryname exists
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function HasEntry(entryname As String, Optional onlyenabled As Boolean = Nothing) As Boolean

        ''' <summary>
        ''' remove an entry by name 
        ''' </summary>
        ''' <param name="columnname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RemoveEntry(entryname As String) As Boolean

        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property EntryAttributes() As IEnumerable(Of iormContainerEntryAttribute)

        ''' <summary>
        ''' sets or returns the Names of the PrimaryKey Columns
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property PrimaryEntryNames() As String()

        ''' <summary>
        ''' returns a List of all Entries
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property EntryNames() As IEnumerable(Of String)

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Property Description() As String

        ''' <summary>
        ''' returns true if the description has a value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueDescription() As Boolean

        ''' <summary>
        ''' Gets or sets name of the Primary key 
        ''' </summary>
        ''' <value>The description.</value>
        Property PrimaryKey() As String
        ''' <summary>
        ''' returns true if the primary name is set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValuePrimaryKey() As Boolean

        ''' <summary>
        ''' Gets or sets the object ID.
        ''' </summary>
        ''' <value>The object ID.</value>
        Property ObjectID() As String

        ReadOnly Property HasValueObjectID() As Boolean

        ''' <summary>
        ''' Gets or sets the unique name of the container (such as tables).
        ''' </summary>
        ''' <value>The name of the table.</value>
        Property ContainerID() As String

        ReadOnly Property HasValueContainerID() As Boolean

        ''' <summary>
        ''' Gets or sets the add domain ID flag.
        ''' </summary>
        ''' <value>The add domain ID flag.</value>
        Property AddDomainBehavior() As Boolean
        ReadOnly Property HasValueAddDomainBehavior() As Boolean

        ''' <summary>
        ''' Gets or sets the version.
        ''' </summary>
        ''' <value>The version.</value>
        Property Version() As UShort

        ReadOnly Property HasValueVersion() As Boolean

        ''' <summary>
        ''' Gets or sets the ID of the Attribute
        ''' </summary>
        ''' <value>The ID.</value>
        Property ID() As String

        ReadOnly Property HasValueID() As Boolean

        ''' <summary>
        ''' sets or gets the add deletefield flag. This will add a field for deletion the record to the schema.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property AddDeleteFieldBehavior() As Boolean
        ReadOnly Property HasValueDeleteFieldBehavior() As Boolean

        ''' <summary>
        ''' sets or gets the add ParameterField flag. 
        ''' This will add extra fields for additional parameters (reserve and spare) to the data object.
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property AddSpareFields() As Boolean

        ReadOnly Property HasValueSpareFields() As Boolean

    End Interface
    ''' <summary>
    ''' defines the interface for a member of a container (to store the object entry)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iormContainerEntryAttribute

        ''' <summary>
        ''' Gets or sets the enabled.
        ''' </summary>
        ''' <value>The enabled.</value>
        Property Enabled() As Boolean

        ''' <summary>
        ''' Gets or sets the ID.
        ''' </summary>
        ''' <value>The ID.</value>
        Property ID() As String

        ''' <summary>
        ''' true if the ID has a value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueID() As Boolean

        ''' <summary>
        ''' Gets or sets the name of the entry
        ''' </summary>
        ''' <value>The name of the Member.</value>
        Property ContainerEntryName() As String

        ReadOnly Property HasValueContainerEntryName() As Boolean

        ''' <summary>
        ''' Gets or sets the reference object entry. Has the form [objectname].[entryname] 
        ''' such as Deliverable.constObjectID & "." & deliverable.constFNUID
        ''' </summary>
        ''' <value>The reference object entry.</value>
        Property ReferenceObjectEntry() As String

        ReadOnly Property HasValueReferenceObjectEntry() As Boolean

        ''' <summary>
        ''' Gets or sets the description.
        ''' </summary>
        ''' <value>The description.</value>
        Property Description() As String
        ''' <summary>
        ''' true if the description has a value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueDescription() As Boolean

        ''' <summary>
        ''' Gets or sets the pos ordinal.
        ''' </summary>
        ''' <value>The pos ordinal.</value>
        Property Posordinal() As UShort

        ReadOnly Property HasValuePosOrdinal() As Boolean

        ''' <summary>
        ''' Gets or sets the default value in DB presentation.
        ''' </summary>
        ''' <value>The default value.</value>
        Property DBDefaultValue() As String

        ReadOnly Property HasValueDBDefaultValue() As Boolean

        ''' <summary>
        ''' Gets or sets the container ID.
        ''' </summary>
        ''' <value>The table ID.</value>
        Property ContainerID() As String

        ReadOnly Property HasValueContainerID() As Boolean

        ''' <summary>
        ''' Gets or sets the Datatype.
        ''' </summary>
        ''' <value>The typeid.</value>
        Property DataType() As otDataType

        ReadOnly Property HasValueDataType() As Boolean

        ''' <summary>
        ''' Gets or sets the nested inner Datatype of Datatype list.
        ''' </summary>
        ''' <value>The typeid.</value>
        Property InnerDataType() As otDataType

        ReadOnly Property HasValueInnerDataType() As Boolean

        ''' <summary>
        ''' Gets or sets the size.
        ''' </summary>
        ''' <value>The size.</value>
        Property Size() As Long

       ReadOnly Property HasValueSize() As Boolean

        ''' <summary>
        ''' Gets or sets the parameter.
        ''' </summary>
        ''' <value>The parameter.</value>
        Property Parameter() As String
        ReadOnly Property HasValueParameter() As Boolean

        ''' <summary>
        ''' Gets or sets the is nullable.
        ''' </summary>
        ''' <value>The is nullable.</value>
        Property IsNullable() As Boolean
        ReadOnly Property HasValueIsNullable() As Object

        ''' <summary>
        ''' Gets or sets the Unique Property.
        ''' </summary>
        ''' <value></value>
        Property IsUnique() As Boolean
        ReadOnly Property HasValueIsUnique() As Object


        ''' <summary>
        ''' Gets or sets the primary key ordinal.
        ''' </summary>
        ''' <value>The primary key ordinal.</value>
        Property PrimaryEntryOrdinal() As Short

        ''' <summary>
        ''' returns true if the primary key ordinal has a value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValuePrimaryKeyOrdinal() As Boolean

        ''' <summary>
        ''' gets or sets the version counter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Version() As UShort

        ''' <summary>
        ''' returns true if the version has a value
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property HasValueVersion() As Boolean

        ''' <summary>
        ''' get or sets the relation descriptions of this entry by string
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Relation As String()

        ReadOnly Property HasValueRelation As Boolean

        ''' <summary>
        ''' sets or gets the ForeignKey properties string representation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ForeignKeyProperties As String()
        ''' <summary>
        ''' gets or sets the Foreign Key Property Array
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ForeignKeyProperty As ForeignKeyProperty()

        ReadOnly Property HasValueForeignKeyProperties As Boolean

        ''' <summary>
        ''' gets or sets the foreign key reference
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property ForeignKeyReferences As String()

        ReadOnly Property HasValueForeignKeyReferences As Boolean

        ''' <summary>
        ''' gets or sets the UseForeignKey flag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Property UseForeignKey As otForeignKeyImplementation
        ReadOnly Property HasValueUseForeignKey As Boolean




    End Interface
End Namespace